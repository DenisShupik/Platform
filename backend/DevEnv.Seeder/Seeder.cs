using Microsoft.Extensions.Hosting;

namespace DevEnv.Seeder;

public sealed class Seeder : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly KeycloakClient _keycloakClient;

    public Seeder(
        IHostApplicationLifetime appLifetime,
        KeycloakClient keycloakClient
    )
    {
        _appLifetime = appLifetime;
        _keycloakClient = keycloakClient;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // TODO: заменить на пробу
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        
        List<CreateUserRequestBody.Credential> credentials =
        [
            new()
            {
                Type = "password",
                Value = "12345678",
                Temporary = false
            }
        ];

        for (var i = 0; i < 10; i++)
        {
            var createUserRequestBody = new CreateUserRequestBody
            {
                Username = $"user{i}",
                FirstName = "Иван",
                LastName = "Иванов",
                Email = $"user{i}@app.com",
                Enabled = true,
                Credentials = credentials
            };

            await _keycloakClient.CreateUserAsync(createUserRequestBody, cancellationToken);
        }
        
        _appLifetime.StopApplication();
    }
}