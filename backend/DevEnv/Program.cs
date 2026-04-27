using DevEnv.Extensions;
using DevEnv.Resources;
using FileService.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Options;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("compose");

var username = builder.AddParameter("username", "admin");
var password = builder.AddParameter("password", "12345678");

var keycloakOptions = builder.GetOptions<KeycloakOptions, KeycloakOptionsValidator>();
var rabbitMqOptions = builder.GetOptions<RabbitMqOptions, RabbitMqOptionsValidator>();
var valkeyOptions = builder.GetOptions<ValkeyOptions, ValkeyOptionsValidator>();
var s3Options = builder.GetOptions<S3Options, S3OptionsValidator>();

var infrastructurePath = builder.Configuration.GetValue<string>("InfrastructurePath");

var dbServer = builder
        .AddPostgres("db-server", username, password, port: 5432)
        .WithImageTag("18.3")
        .WithEnvironment("POSTGRES_DB", "postgres")
        .WithBindMount($"{infrastructurePath}/postgres.sql", "/docker-entrypoint-initdb.d/postgres.sql",
            true)
    ;

var db = dbServer.AddDatabase("db", "platform_db");

var cache = builder
        .AddValkey("cache", 6379, password)
        .WithImageTag("9.0.3")
    ;

var broker = builder
        .AddRabbitMQ("broker", username, password, 5672)
        .WithImageTag("4.3.0")
        .WithManagementPlugin(15672)
    ;

var identity = builder
        .AddKeycloak("identity", 8080, username, password)
        .WithImageTag("26.6.1")
        .WithEnvironment("KK_TO_RMQ_URL", "broker")
        .WithEnvironment("KK_TO_RMQ_VHOST", "/")
        .WithEnvironment("KK_TO_RMQ_USERNAME", username)
        .WithEnvironment("KK_TO_RMQ_PASSWORD", password)
        .WithEnvironment("KK_TO_RMQ_EXCHANGE", "keycloak")
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_REALM", keycloakOptions.Realm)
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_USER_CLIENT_ID", keycloakOptions.Audience)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_ID",
            builder.Configuration.GetValue<string>("KeycloakOptions:ServiceClientId"))
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_SECRET",
            builder.Configuration.GetValue<string>("KeycloakOptions:ServiceClientSecret"))
        .WithRealmImport($"{infrastructurePath}/keycloak.json")
        .WithBindMount($"{infrastructurePath}/keycloak-to-rabbit-3.0.5.jar",
            "/opt/keycloak/providers/keycloak-to-rabbit-3.0.5.jar",
            true)
        .WithReference(broker)
        .WaitFor(broker)
    ;

var storage = builder.AddRustFs("storage", username, password);

if (!builder.Configuration.GetValue<bool>("DisableServices"))
{
    var coreService = builder.AddProject<Projects.CoreService>("core-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddRabbitMqOptions(rabbitMqOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(db)
            .WaitFor(db)
            .WithReference(identity)
            .WaitFor(identity)
            .WithReference(broker)
            .WaitFor(broker)
        ;

    var userService = builder.AddProject<Projects.UserService>("user-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddRabbitMqOptions(rabbitMqOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(db)
            .WaitFor(db)
            .WithReference(identity)
            .WaitFor(identity)
            .WithReference(broker)
            .WaitFor(broker)
        ;

    var fileService = builder.AddProject<Projects.FileService>("file-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddS3Options(s3Options)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(identity)
            .WaitFor(identity)
            .WithReference(storage)
            .WaitFor(storage)
        ;

    var notificationService = builder.AddProject<Projects.NotificationService>("notification-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddRabbitMqOptions(rabbitMqOptions)
            .AddRedisOptions(valkeyOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(db)
            .WaitFor(db)
            .WithReference(identity)
            .WaitFor(identity)
            .WithReference(broker)
            .WaitFor(broker)
            .WithReference(cache)
            .WaitFor(cache)
            .WithReference(coreService)
            .WaitFor(coreService)
        ;

    var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .WithUrlForEndpoint("http", url =>
            {
                url.DisplayText = "Swagger UI";
                url.Url = "/swagger";
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddRedisOptions(valkeyOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(identity)
            .WaitFor(identity)
            .WithReference(cache)
            .WaitFor(cache)
            .WithReference(coreService)
            .WaitFor(coreService)
            .WithReference(userService)
            .WaitFor(userService)
            .WithReference(fileService)
            .WaitFor(fileService)
            .WithReference(notificationService)
            .WaitFor(notificationService)
        ;

    if (builder.Configuration.GetValue<bool>("Seeding"))
    {
        var seeder = builder.AddProject<Projects.DevEnv_Seeder>("seeder")
                .AddKeycloakOptions(keycloakOptions)
                .WithReference(apiGateway)
                .WaitFor(apiGateway)
            ;
    }
}

var app = builder.Build();
await app.RunAsync();