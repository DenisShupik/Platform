using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Server;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.Options;
using UserService.Application;
using UserService.Infrastructure.Grpc.Contracts;
using UserService.Infrastructure;
using UserService.Infrastructure.Persistence;
using UserService.Presentation;
using UserService.Presentation.Grpc;
using UserService.Presentation.Options;
using UserService.Presentation.Rest;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices<UserServiceOptions>();
builder.AddPresentationServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization()
    ;

app
    .UseSwagger()
    .UseSwaggerUI(options =>
    {
        var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;
        options.OAuthClientId(keycloakOptions.Audience);
        options.OAuthUsePkce();
    });

app.MapUserApi();

app.MapGrpcService<GrpcUserService>();
app.MapCodeFirstGrpcReflectionService();
var schemaGenerator = new ProtoBuf.Grpc.Reflection.SchemaGenerator
{
    ProtoSyntax = ProtoBuf.Meta.ProtoSyntax.Proto3
};
var schema = schemaGenerator.GetSchema<IGrpcUserService>();
Console.WriteLine(schema);

app.Logger.StartingApp();

await app.RunAsync();