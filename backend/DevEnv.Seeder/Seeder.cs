using CoreService.Application.UseCases;
using Microsoft.Extensions.Hosting;

namespace DevEnv.Seeder;

public sealed class Seeder : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly KeycloakClient _keycloakClient;
    private readonly ApiClient _apiClient;

    public Seeder(
        IHostApplicationLifetime appLifetime,
        KeycloakClient keycloakClient,
        ApiClient apiClient
    )
    {
        _appLifetime = appLifetime;
        _keycloakClient = keycloakClient;
        _apiClient = apiClient;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // TODO: заменить на пробу
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

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

        var forumId = await _apiClient.CreateForumAsync(new CreateForumRequest { Title = "Новый форум" },
            cancellationToken);

        foreach (var c in Enumerable.Range(1, 20))
        {
            var categoryId = await _apiClient.CreateCategoryAsync(
                new CreateCategoryRequest { ForumId = forumId, Title = $"Новая категория {c}" },
                cancellationToken);

            foreach (var t in Enumerable.Range(1, 20))
            {
                var threadId = await _apiClient.CreateThreadAsync(
                    new CreateThreadRequest { CategoryId = categoryId, Title = $"Новый тред {t}" },
                    cancellationToken);

                foreach (var p in Enumerable.Range(1, 20))
                {
                    var postId = await _apiClient.CreatePostAsync(
                        new CreatePostRequest
                        {
                            ThreadId = threadId, Body = new CreatePostRequest.FromBody { Content = $"Новый пост {p}" }
                        },
                        cancellationToken);
                }
            }
        }

        _appLifetime.StopApplication();
    }
}