using DevEnv.Extensions;
using DevEnv.Resources;
using FileService.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Options;

var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username", "admin");
var password = builder.AddParameter("password", "12345678");

var keycloakOptions = builder.GetOptions<KeycloakOptions, KeycloakOptionsValidator>();
var rabbitMqOptions = builder.GetOptions<RabbitMqOptions, RabbitMqOptionsValidator>();
var valkeyOptions = builder.GetOptions<ValkeyOptions, ValkeyOptionsValidator>();
var s3Options = builder.GetOptions<S3Options, S3OptionsValidator>();

var infrastructurePath = builder.Configuration.GetValue<string>("InfrastructurePath");

var postgres = builder
        .AddPostgres("db", username, password, port: 5432)
        .WithImageTag("18.0")
        .WithEnvironment("POSTGRES_DB", "postgres")
        .WithBindMount($"{infrastructurePath}/postgres.sql", "/docker-entrypoint-initdb.d/postgres.sql",
            true)
    ;

postgres.AddDatabase("postgres");

var valkey = builder
        .AddValkey("valkey", 6379, password)
        .WithImageTag("8.1.3")
    ;

var rabbitmq = builder
        .AddRabbitMQ("rabbitmq", username, password, 5672)
        .WithImageTag("4.2.0-beta.4-management")
        .WithManagementPlugin(15672)
    ;

var keycloak = builder
        .AddKeycloak("keycloak", 8080, username, password)
        .WithImageTag("26.4.0")
        .WithEnvironment("KK_TO_RMQ_URL", "rabbitmq")
        .WithEnvironment("KK_TO_RMQ_VHOST", "/")
        .WithEnvironment("KK_TO_RMQ_USERNAME", username)
        .WithEnvironment("KK_TO_RMQ_PASSWORD", password)
        .WithEnvironment("KK_TO_RMQ_EXCHANGE", "keycloak")
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_REALM", keycloakOptions.Realm)
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_USER_CLIENT_ID", keycloakOptions.Audience)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_ID",
            builder.Configuration.GetValue<string>("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_ID"))
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_SECRET",
            builder.Configuration.GetValue<string>("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_SECRET"))
        .WithRealmImport($"{infrastructurePath}/keycloak.json")
        .WithBindMount($"{infrastructurePath}/keycloak-to-rabbit-3.0.5.jar",
            "/opt/keycloak/providers/keycloak-to-rabbit-3.0.5.jar",
            true)
        .WithReference(rabbitmq)
        .WaitFor(rabbitmq)
    ;

var minio = builder.AddMinio("minio", username, password, $"{infrastructurePath}/minio.sh");

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
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithReference(keycloak)
            .WaitFor(keycloak)
            .WithReference(rabbitmq)
            .WaitFor(rabbitmq)
        ;

    var userService = builder.AddProject<Projects.UserService>("user-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddRabbitMqOptions(rabbitMqOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithReference(keycloak)
            .WaitFor(keycloak)
            .WithReference(rabbitmq)
            .WaitFor(rabbitmq)
        ;

    var fileService = builder.AddProject<Projects.FileService>("file-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddS3Options(s3Options)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(keycloak)
            .WaitFor(keycloak)
            .WithReference(minio)
            .WaitFor(minio)
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
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithReference(keycloak)
            .WaitFor(keycloak)
            .WithReference(rabbitmq)
            .WaitFor(rabbitmq)
            .WithReference(valkey)
            .WaitFor(valkey)
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
            .WithReference(keycloak)
            .WaitFor(keycloak)
            .WithReference(valkey)
            .WaitFor(valkey)
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