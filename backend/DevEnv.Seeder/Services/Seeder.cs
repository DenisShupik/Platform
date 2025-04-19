using System.Threading.Tasks.Dataflow;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Apis.Dtos;
using Microsoft.Extensions.Hosting;
using SharedKernel.Tests.Dtos;
using SharedKernel.Tests.Services;

namespace DevEnv.Seeder.Services;

public sealed class Seeder : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Fixture _fixture;
    private readonly KeycloakAdminClient _keycloakAdminClient;
    private readonly CoreServiceClient _coreServiceClient;
    private readonly FileServiceClient _fileServiceClient;


    private const int ForumCount = 1;
    private const int CategoryPerForum = 1;
    private const int ThreadPerCategory = 5;
    private const int PostPerThread = 20;

    public Seeder(
        IHostApplicationLifetime appLifetime,
        Fixture fixture,
        KeycloakAdminClient keycloakAdminClient,
        CoreServiceClient coreServiceClient,
        FileServiceClient fileServiceClient
    )
    {
        _appLifetime = appLifetime;
        _fixture = fixture;
        _keycloakAdminClient = keycloakAdminClient;
        _coreServiceClient = coreServiceClient;
        _fileServiceClient = fileServiceClient;
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

        await Task.WhenAll(_fixture.Users.Select(user => _keycloakAdminClient.CreateUserAsync(new CreateUserRequestBody
            {
                Username = user,
                FirstName = "Иван",
                LastName = "Иванов",
                Email = $"{user}@app.com",
                Enabled = true,
                Credentials = credentials
            }, cancellationToken)
        ));

        await Task.WhenAll(_fixture.Users.Select(async user =>
            {
                var fileBytes = await File.ReadAllBytesAsync($"./Content/{user}.webp", cancellationToken);
                await _fileServiceClient.UploadAvatar(fileBytes, cancellationToken);
            }
        ));

        var executionOptions = new ExecutionDataflowBlockOptions
            { MaxDegreeOfParallelism = Environment.ProcessorCount };

        var createForumBlock = new TransformBlock<int, ForumId>(async i => await _coreServiceClient.CreateForumAsync(
                new CreateForumRequestBody { Title = ForumTitle.From($"Новый форум {i}") }, cancellationToken),
            executionOptions);

        var createCategoryBlock = new TransformManyBlock<ForumId, CreateCategoryRequestBody>(forumId =>
                Enumerable.Range(1, CategoryPerForum).Select(i => new CreateCategoryRequestBody
                    { ForumId = forumId, Title = CategoryTitle.From($"Новая категория {i}") }),
            executionOptions);

        var createThreadBlock = new TransformManyBlock<CreateCategoryRequestBody, CreateThreadRequestBody>(async request =>
            {
                var categoryId =
                    await _coreServiceClient.CreateCategoryAsync(request, cancellationToken);
                return Enumerable.Range(1, ThreadPerCategory).Select(i => new CreateThreadRequestBody
                    { CategoryId = categoryId, Title = ThreadTitle.From($"Новый тред {i}") });
            },
            executionOptions);

        var createPostsBlock =
            new TransformManyBlock<CreateThreadRequestBody, (ThreadId ThreadId, CreatePostRequestBody Body)>(
                async request =>
                {
                    var threadId = await _coreServiceClient.CreateThreadAsync(request, cancellationToken);
                    return Enumerable.Range(1, PostPerThread).Select(i => (threadId, new CreatePostRequestBody
                    {
                        Content = $"Новый пост {i}"
                    }));
                },
                executionOptions);

        var postBlock = new ActionBlock<(ThreadId ThreadId, CreatePostRequestBody Body)>(
            async request =>
            {
                await _coreServiceClient.CreatePostAsync(request.ThreadId, request.Body, cancellationToken);
            },
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