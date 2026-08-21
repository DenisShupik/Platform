using Aspire.Hosting.ApplicationModel;
using DevEnv.Extensions;
using DevEnv.Resources;
using FileService.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Options;
using Shared.Presentation.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("compose");

var username = builder.AddParameter("username", "admin");
var password = builder.AddParameter("password", "12345678");

var keycloakOptions = builder.GetOptions<KeycloakOptions, KeycloakOptionsValidator>();
var internalApiOptions = builder.GetOptions<InternalApiOptions, InternalApiOptionsValidator>();
var coreServiceAccount = builder.Configuration.GetRequiredSection("ServiceAccounts:CoreService")
    .Get<ServiceAccountOptions>() ?? throw new InvalidOperationException("CoreService account is not configured.");
var notificationServiceAccount = builder.Configuration.GetRequiredSection("ServiceAccounts:NotificationService")
    .Get<ServiceAccountOptions>() ?? throw new InvalidOperationException("NotificationService account is not configured.");
var provisioningServiceAccount = builder.Configuration.GetRequiredSection("ServiceAccounts:ProvisioningService")
    .Get<ServiceAccountOptions>() ?? throw new InvalidOperationException("Provisioning service account is not configured.");
var keycloakAdminOptions = builder.GetOptions<KeycloakAdminOptions, KeycloakAdminOptionsValidator>();
var rabbitMqOptions = builder.GetOptions<RabbitMqOptions, RabbitMqOptionsValidator>();
var valkeyOptions = builder.GetOptions<ValkeyOptions, ValkeyOptionsValidator>();
var s3Options = builder.GetOptions<S3Options, S3OptionsValidator>();

var infrastructurePath = builder.Configuration.GetValue<string>("InfrastructurePath");
var infrastructureDirectory = Path.GetFullPath(infrastructurePath!, builder.AppHostDirectory);

var dbServer = builder
        .AddPostgres("db-server", username, password, port: 5432)
        .WithImageTag("19beta3")
        .WithEnvironment("POSTGRES_DB", "postgres")
        .WithContainerFiles("/docker-entrypoint-initdb.d", [
            new ContainerFile
            {
                Name = "postgres.sql",
                SourcePath = Path.Combine(infrastructureDirectory, "postgres.sql")
            }
        ])
    ;

var db = dbServer.AddDatabase("db", "platform_db");

var cache = builder
        .AddValkey("cache", 6379, password)
        .WithImageTag("9.1.1")
    ;

var broker = builder
        .AddRabbitMQ("broker", username, password, 5672)
        .WithImageTag("4.3.5")
        .WithManagementPlugin(15672)
    ;

// Нужно из-за https://github.com/microsoft/aspire/issues/17661, иначе начинает работать через https
#pragma warning disable ASPIRECERTIFICATES001
var identity = builder
        .AddKeycloak("identity", 8080, username, password)
        .WithImageTag("26.7.2")
        .WithoutHttpsCertificate()
        .WithEnvironment("KK_TO_RMQ_URL", "broker")
        .WithEnvironment("KK_TO_RMQ_VHOST", "/")
        .WithEnvironment("KK_TO_RMQ_USERNAME", username)
        .WithEnvironment("KK_TO_RMQ_PASSWORD", password)
        .WithEnvironment("KK_TO_RMQ_EXCHANGE", "keycloak")
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_REALM", keycloakOptions.Realm)
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_USER_CLIENT_ID", keycloakOptions.Audience)
        .WithEnvironment("PUBLIC_APP_KEYCLOAK_INTERNAL_AUDIENCE", keycloakOptions.InternalAudience)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_CORE_SERVICE_CLIENT_ID", coreServiceAccount.ClientId)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_CORE_SERVICE_CLIENT_SECRET", coreServiceAccount.ClientSecret)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_NOTIFICATION_SERVICE_CLIENT_ID", notificationServiceAccount.ClientId)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_NOTIFICATION_SERVICE_CLIENT_SECRET", notificationServiceAccount.ClientSecret)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_PROVISIONING_SERVICE_CLIENT_ID", provisioningServiceAccount.ClientId)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_PROVISIONING_SERVICE_CLIENT_SECRET", provisioningServiceAccount.ClientSecret)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_ADMIN_CLIENT_ID", keycloakAdminOptions.ClientId)
        .WithEnvironment("PRIVATE_APP_KEYCLOAK_ADMIN_CLIENT_SECRET",
            builder.Configuration.GetValue<string>("KeycloakAdminOptions:ClientSecret"))
        .WithRealmImport($"{infrastructurePath}/keycloak.json")
        .WithContainerFiles("/opt/keycloak/providers", [
            new ContainerFile
            {
                Name = "keycloak-to-rabbit-3.0.5.jar",
                SourcePath = Path.Combine(infrastructureDirectory, "keycloak-to-rabbit-3.0.5.jar")
            }
        ])
        .WithReference(broker)
        .WaitFor(broker)
        .WithHttpHealthCheck($"/realms/{keycloakOptions.Realm}/.well-known/openid-configuration")
    ;
#pragma warning restore ASPIRECERTIFICATES001

var storage = builder.AddRustFs("storage", username, password);

if (!builder.Configuration.GetValue<bool>("DisableServices"))
{
    var coreService = builder.AddProject<Projects.CoreService>("core-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddInternalApiOptions(internalApiOptions)
            .AddServiceAccountOptions(coreServiceAccount)
            .AddRabbitMqOptions(rabbitMqOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithHttpHealthCheck(ServiceHealthCheckExtensions.ReadinessPath, endpointName: "Rest")
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
            .AddInternalApiOptions(internalApiOptions)
            .AddKeycloakAdminOptions(keycloakAdminOptions)
            .AddRabbitMqOptions(rabbitMqOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithHttpHealthCheck(ServiceHealthCheckExtensions.ReadinessPath, endpointName: "Rest")
            .WithReference(db)
            .WaitFor(db)
            .WithReference(identity)
            .WaitFor(identity)
            .WithReference(broker)
            .WaitFor(broker)
        ;

    coreService
        .WithReference(userService)
        .WaitFor(userService);

    var fileService = builder.AddProject<Projects.FileService>("file-service", static project =>
            {
                project.ExcludeLaunchProfile = true;
                project.ExcludeKestrelEndpoints = false;
            })
            .AddKeycloakOptions(keycloakOptions)
            .AddInternalApiOptions(internalApiOptions)
            .AddS3Options(s3Options)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithHttpHealthCheck(ServiceHealthCheckExtensions.ReadinessPath)
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
            .AddInternalApiOptions(internalApiOptions)
            .AddServiceAccountOptions(notificationServiceAccount)
            .AddRabbitMqOptions(rabbitMqOptions)
            .AddRedisOptions(valkeyOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithHttpHealthCheck(ServiceHealthCheckExtensions.ReadinessPath)
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
            .AddInternalApiOptions(internalApiOptions)
            .AddRedisOptions(valkeyOptions)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithHttpHealthCheck(ServiceHealthCheckExtensions.ReadinessPath)
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
                .AddInternalApiOptions(internalApiOptions)
                .AddServiceAccountOptions(provisioningServiceAccount)
                .AddKeycloakAdminOptions(keycloakAdminOptions)
                .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
                .WithReference(identity)
                .WaitFor(identity)
                .WithReference(userService)
                .WaitFor(userService)
                .WithReference(coreService)
                .WaitFor(coreService)
                .WithReference(apiGateway)
                .WaitFor(apiGateway)
            ;
    }
}

var app = builder.Build();
await app.RunAsync();
