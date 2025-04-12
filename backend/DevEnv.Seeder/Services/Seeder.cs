using System.Threading.Tasks.Dataflow;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using DevEnv.Seeder.Dtos;
using Microsoft.Extensions.Hosting;

namespace DevEnv.Seeder.Services;

public sealed class Seeder : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly KeycloakClient _keycloakClient;
    private readonly ApiClient _apiClient;

    private const int UserCount = 10;
    private const int ForumCount = 1;
    private const int CategoryPerForum = 1;
    private const int ThreadPerCategory = 5;
    private const int PostPerThread = 20;

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

        var users = Enumerable.Range(1, UserCount).Select(x => $"user{x}").ToArray();

        string GetRandomUser() => users[Random.Shared.Next(0, UserCount)];

        await Task.WhenAll(users.Select(user => _keycloakClient.CreateUserAsync(new CreateUserRequestBody
            {
                Username = user,
                FirstName = "Иван",
                LastName = "Иванов",
                Email = $"{user}@app.com",
                Enabled = true,
                Credentials = credentials
            }, cancellationToken)
        ));

        await Task.WhenAll(users.Select(async user =>
            {
                var fileBytes = await File.ReadAllBytesAsync($"./Content/{user}.webp", cancellationToken);
                await _apiClient.UploadAvatar(fileBytes, user, cancellationToken);
            }
        ));

        var executionOptions = new ExecutionDataflowBlockOptions
            { MaxDegreeOfParallelism = Environment.ProcessorCount };

        var createForumBlock = new TransformBlock<int, ForumId>(async i => await _apiClient.CreateForumAsync(
                new CreateForumRequest { Title = ForumTitle.From($"Новый форум {i}") }, GetRandomUser(),
                cancellationToken),
            executionOptions);

        var createCategoryBlock = new TransformManyBlock<ForumId, CreateCategoryRequest>(forumId =>
                Enumerable.Range(1, CategoryPerForum).Select(i => new CreateCategoryRequest
                    { ForumId = forumId, Title = CategoryTitle.From($"Новая категория {i}") }),
            executionOptions);

        var createThreadBlock = new TransformManyBlock<CreateCategoryRequest, CreateThreadRequest>(async request =>
            {
                var categoryId = await _apiClient.CreateCategoryAsync(request, GetRandomUser(), cancellationToken);
                return Enumerable.Range(1, ThreadPerCategory).Select(i => new CreateThreadRequest
                    { CategoryId = categoryId, Title = $"Новый тред {i}" });
            },
            executionOptions);

        var createPostsBlock = new TransformManyBlock<CreateThreadRequest, CreatePostRequest>(async request =>
            {
                var threadId = await _apiClient.CreateThreadAsync(request, GetRandomUser(), cancellationToken);
                return Enumerable.Range(1, PostPerThread).Select(i => new CreatePostRequest
                {
                    ThreadId = threadId, Body = new CreatePostRequest.FromBody { Content = $"Новый пост {i}" }
                });
            },
            executionOptions);

        var postBlock = new ActionBlock<CreatePostRequest>(
            async request => { await _apiClient.CreatePostAsync(request, GetRandomUser(), cancellationToken); },
            executionOptions);

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        createForumBlock.LinkTo(createCategoryBlock, linkOptions);
        createCategoryBlock.LinkTo(createThreadBlock, linkOptions);
        createThreadBlock.LinkTo(createPostsBlock, linkOptions);
        createPostsBlock.LinkTo(postBlock, linkOptions);

        for (var i = 0; i < ForumCount; i++)
        {
            await createForumBlock.SendAsync(i, cancellationToken);
        }

        createForumBlock.Complete();

        await postBlock.Completion;

        _appLifetime.StopApplication();
    }
}