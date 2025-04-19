using System.ComponentModel.DataAnnotations;
using DevEnv;
using DevEnv.Resources;
using FileService.Presentation.Options;
using Microsoft.Extensions.Configuration;
using SharedKernel.Presentation.Options;

var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username", "admin");
var password = builder.AddParameter("password", "12345678");

var keycloakOptions = builder.Configuration.GetRequiredSection(nameof(KeycloakOptions)).Get<KeycloakOptions>();

var infrastructurePath = builder.Configuration.GetValue<string>("InfrastructurePath");

if (keycloakOptions != null)
{
    var validator = new KeycloakOptionsValidator();
    var result = validator.Validate(keycloakOptions);
    if (!result.IsValid) throw new ValidationException(result.ToString());
}

var s3Options = builder.Configuration.GetRequiredSection(nameof(S3Options)).Get<S3Options>()!;

var postgres = builder
    .AddPostgres("db", username, password, port: 5432)
    .WithImageTag("17.4")
    .WithEnvironment("POSTGRES_DB", "postgres")
    .WithBindMount($"{infrastructurePath}/postgres.sql", "/docker-entrypoint-initdb.d/postgres.sql",
            true)
    ;

postgres.AddDatabase("postgres");

var redis = builder
        .AddRedis("redis", 6379)
        .WithImageTag("7.4.2")
    ;

var rabbitmq = builder
        .AddRabbitMQ("rabbitmq", username, password, 5672)
        .WithImageTag("4.1.0")
        .WithManagementPlugin(15672)
    ;

var keycloak = builder
        .AddKeycloak("keycloak", 8080, username, password)
        .WithImageTag("26.2.0")
        .WithEnvironment("KK_TO_RMQ_URL", "rabbitmq")
        .WithEnvironment("KK_TO_RMQ_VHOST", "/")
        .WithEnvironment("KK_TO_RMQ_USERNAME", username)
        .WithEnvironment("KK_TO_RMQ_PASSWORD", password)
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_REALM", keycloakOptions.Realm)
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_USER_CLIENT_ID", keycloakOptions.Audience)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_ID",  builder.Configuration.GetValue<string>("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_ID"))
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_SECRET", builder.Configuration.GetValue<string>("PRIVATE_APP_KEYCLOAK_SERVICE_CLIENT_SECRET"))
        .WithRealmImport($"{infrastructurePath}/keycloak.json", true)
        .WithBindMount($"{infrastructurePath}/keycloak-to-rabbit-3.0.5.jar", "/opt/keycloak/providers/keycloak-to-rabbit-3.0.5.jar",
            true)
        .WithReference(rabbitmq)
        .WaitFor(rabbitmq)
    ;

var minio = builder.AddMinio("minio", username, password,$"{infrastructurePath}/minio.sh");

if (!builder.Configuration.GetValue<bool>("DisableServices"))
{

    var coreService = builder.AddProject<Projects.CoreService>("core-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithReference(keycloak)
            .WaitFor(keycloak)
            .WithReference(redis)
            .WaitFor(redis)
        ;

    var userService = builder.AddProject<Projects.UserService>("user-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithReference(keycloak)
            .WaitFor(keycloak)
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

    var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithReference(coreService)
            .WaitFor(coreService)
            .WithReference(userService)
            .WaitFor(userService)
            .WithReference(fileService)
            .WaitFor(fileService)
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