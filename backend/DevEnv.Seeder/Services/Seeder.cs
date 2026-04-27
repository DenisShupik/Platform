using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.Extensions.Hosting;
using NotificationService.Domain.Enums;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Domain.Enums;
using Shared.Tests.Dtos;
using Shared.Tests.Services;

namespace DevEnv.Seeder.Services;

public sealed class Seeder : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Fixture _fixture;
    private readonly KeycloakAdminClient _keycloakAdminClient;
    private readonly IHttpClientFactory _httpClientFactory;

    private const int ForumCount = 2;
    private const int CategoryPerForum = 2;
    private const int ThreadPerCategory = 2;
    private const int PostPerThread = 10;

    public Seeder(
        IHostApplicationLifetime appLifetime,
        Fixture fixture,
        KeycloakAdminClient keycloakAdminClient,
        IHttpClientFactory httpClientFactory
    )
    {
        _appLifetime = appLifetime;
        _fixture = fixture;
        _keycloakAdminClient = keycloakAdminClient;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var threadIds = new ConcurrentBag<ThreadId>();

        var randomUserCoreServiceClient = new CoreServiceClient(_httpClientFactory.CreateClient("randomUser"));
        var moderatorCoreServiceClient = new CoreServiceClient(_httpClientFactory.CreateClient("moderator"));

        var coreServiceClients = new Dictionary<string, CoreServiceClient>();
        var fileCoreServiceClient = new Dictionary<string, FileServiceClient>();
        foreach (var user in _fixture.Users)
        {
            coreServiceClients.Add(user, new CoreServiceClient(_httpClientFactory.CreateClient(user)));
            fileCoreServiceClient.Add(user, new FileServiceClient(_httpClientFactory.CreateClient(user)));
        }

        // TODO: заменить на пробу
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        List<CreateUserRequestBody.Credential> credentials =
        [
            new()
            {
                Type = "password",
                Value = "12345678",
                Temporary = false
            }
        ];

        await Task.WhenAll(_fixture.Users.Select(user => _keycloakAdminClient.CreateUserAsync(
            new CreateUserRequestBody
            {
                Username = user,
                FirstName = "Иван",
                LastName = "Иванов",
                Email = $"{user}@app.com",
                Enabled = true,
                Credentials = credentials
            }, stoppingToken)
        ));

        await Task.WhenAll(_fixture.Users.Select(async user =>
            {
                var fileBytes = await File.ReadAllBytesAsync($"./Content/{user}.webp", stoppingToken);
                await fileCoreServiceClient[user].UploadAvatar(fileBytes, stoppingToken);
            }
        ));

        var moderatorUserId = await _keycloakAdminClient.CreateUserAsync(
            new CreateUserRequestBody
            {
                Username = "moderator",
                FirstName = "Иван",
                LastName = "Иванов",
                Email = "moderator@app.com",
                Enabled = true,
                Credentials = credentials
            }, stoppingToken);

        AssignRoleToUserRequestBody requestBody =
            [new() { ClientId = Guid.Parse("d2c62a5e-c2e2-419b-a176-cc45be86d1eb"), Role = nameof(Role.Moderator) }];

        await _keycloakAdminClient.AssignRoleToUserAsync(moderatorUserId, requestBody, stoppingToken);

        var executionOptions = new ExecutionDataflowBlockOptions
            { MaxDegreeOfParallelism = Environment.ProcessorCount };

        var createForumBlock = new TransformBlock<int, ForumId>(async i =>
                await moderatorCoreServiceClient.CreateForumAsync(
                    new CreateForumRequestBody
                    {
                        Title = ForumTitle.From($"Общий форум {i}"),
                    },
                    stoppingToken),
            executionOptions);

        var createCategoryBlock = new TransformManyBlock<ForumId, CreateCategoryRequestBody>(forumId =>
                Enumerable
                    .Range(1, CategoryPerForum)
                    .Select(i => new CreateCategoryRequestBody
                    {
                        ForumId = forumId,
                        Title = CategoryTitle.From($"Раздел обсуждений {i}")
                    })
                    .ToArray(),
            executionOptions);

        var createThreadBlock = new TransformManyBlock<CreateCategoryRequestBody, CreateThreadRequestBody>(
            async request =>
            {
                var categoryId =
                    await moderatorCoreServiceClient.CreateCategoryAsync(request, stoppingToken);

                return Enumerable
                    .Range(1, ThreadPerCategory)
                    .Select(i => new CreateThreadRequestBody
                    {
                        CategoryId = categoryId,
                        Title = ThreadTitle.From($"Обсуждение {i}")
                    })
                    .ToArray();
            },
            executionOptions);

        var createPostsBlock =
            new TransformManyBlock<CreateThreadRequestBody, (ThreadId ThreadId, CreatePostRequestBody Body)>(
                async request =>
                {
                    var user = _fixture.GetRandomUser();
                    var client = coreServiceClients[user];
                    var threadId = await client.CreateThreadAsync(request, stoppingToken);

                    threadIds.Add(threadId);

                    await client.CreatePostAsync(threadId,
                        new CreatePostRequestBody { Content = PostContent.From("Это заглавное сообщение темы") },
                        stoppingToken);

                    await client.RequestThreadApprovalAsync(threadId, stoppingToken);

                    await moderatorCoreServiceClient.ApproveThreadAsync(threadId, stoppingToken);

                    return Enumerable
                        .Range(1, PostPerThread - 1)
                        .Select(i => (threadId, new CreatePostRequestBody
                        {
                            Content = PostContent.From($"Новое сообщение {i}")
                        }))
                        .ToArray();
                },
                executionOptions);

        var postBlock = new ActionBlock<(ThreadId ThreadId, CreatePostRequestBody Body)>(
            async request =>
            {
                await randomUserCoreServiceClient.CreatePostAsync(request.ThreadId, request.Body, stoppingToken);
            },
            executionOptions);

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        createForumBlock.LinkTo(createCategoryBlock, linkOptions);
        createCategoryBlock.LinkTo(createThreadBlock, linkOptions);
        createThreadBlock.LinkTo(createPostsBlock, linkOptions);
        createPostsBlock.LinkTo(postBlock, linkOptions);

        for (var i = 0;
             i < ForumCount;
             i++)
        {
            await createForumBlock.SendAsync(i, stoppingToken);
        }

        createForumBlock.Complete();

        await postBlock.Completion;

        {
            var threadIdArray = threadIds.ToArray();

            var notificationServiceClientUser1 =
                new NotificationServiceClient(_httpClientFactory.CreateClient(_fixture.Users[0]));
            var coreServiceClientUser2 = coreServiceClients[_fixture.Users[1]];
            var coreServiceClientUser3 = coreServiceClients[_fixture.Users[3]];
            var coreServiceClientUser4 = coreServiceClients[_fixture.Users[4]];

            await notificationServiceClientUser1.CreateThreadSubscriptionAsync(threadIdArray[0],
                new CreateThreadSubscriptionRequestBody
                {
                    Channels = [ChannelType.Internal]
                }, stoppingToken);

            await notificationServiceClientUser1.CreateThreadSubscriptionAsync(threadIdArray[1],
                new CreateThreadSubscriptionRequestBody
                {
                    Channels = [ChannelType.Internal]
                }, stoppingToken);

            await notificationServiceClientUser1.CreateThreadSubscriptionAsync(threadIdArray[2],
                new CreateThreadSubscriptionRequestBody
                {
                    Channels = [ChannelType.Internal]
                }, stoppingToken);

            await coreServiceClientUser2.CreatePostAsync(threadIdArray[0],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 1") },
                stoppingToken);

            var postId = await coreServiceClientUser2.CreatePostAsync(threadIdArray[1],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 1") },
                stoppingToken);

            var post = await coreServiceClientUser2.GetPostAsync(postId, stoppingToken);

            await coreServiceClientUser2.UpdatePostAsync(postId,
                new UpdatePostRequestBody
                {
                    Content = PostContent.From("Отредактированное сообщение 1"),
                    RowVersion = post.RowVersion
                },
                stoppingToken);

            await coreServiceClientUser2.CreatePostAsync(threadIdArray[2],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 1") },
                stoppingToken);

            await coreServiceClientUser3.CreatePostAsync(threadIdArray[0],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 2") },
                stoppingToken);

            await coreServiceClientUser3.CreatePostAsync(threadIdArray[1],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 2") },
                stoppingToken);

            await coreServiceClientUser3.CreatePostAsync(threadIdArray[2],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 2") },
                stoppingToken);

            await coreServiceClientUser4.CreatePostAsync(threadIdArray[0],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 3") },
                stoppingToken);

            await coreServiceClientUser4.CreatePostAsync(threadIdArray[1],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 3") },
                stoppingToken);

            await coreServiceClientUser4.CreatePostAsync(threadIdArray[2],
                new CreatePostRequestBody { Content = PostContent.From("Новое сообщение 3") },
                stoppingToken);

            var user1 = _fixture.Users[0];
            var coreServiceClientUser1 = coreServiceClients[user1];

            var rejectedThreadId1 = await coreServiceClientUser1.CreateThreadAsync(new CreateThreadRequestBody
            {
                CategoryId = await moderatorCoreServiceClient.CreateCategoryAsync(new CreateCategoryRequestBody
                {
                    ForumId = await moderatorCoreServiceClient.CreateForumAsync(new CreateForumRequestBody
                    {
                        Title = ForumTitle.From("Предложения и обратная связь"),
                    }, stoppingToken),
                    Title = CategoryTitle.From("Технические предложения")
                }, stoppingToken),
                Title = ThreadTitle.From("Добавить темную тему")
            }, stoppingToken);

            await coreServiceClientUser1.CreatePostAsync(rejectedThreadId1,
                new CreatePostRequestBody
                    { Content = PostContent.From("Было бы здорово иметь темную тему в приложении") },
                stoppingToken);

            await coreServiceClientUser1.RequestThreadApprovalAsync(rejectedThreadId1, stoppingToken);
            await moderatorCoreServiceClient.RejectThreadAsync(rejectedThreadId1, stoppingToken);

            var rejectedThreadId2 = await coreServiceClientUser1.CreateThreadAsync(new CreateThreadRequestBody
            {
                CategoryId = await moderatorCoreServiceClient.CreateCategoryAsync(new CreateCategoryRequestBody
                {
                    ForumId = await moderatorCoreServiceClient.CreateForumAsync(new CreateForumRequestBody
                    {
                        Title = ForumTitle.From("Архив старых тем"),
                    }, stoppingToken),
                    Title = CategoryTitle.From("Устаревшие обсуждения")
                }, stoppingToken),
                Title = ThreadTitle.From("Старая тема для удаления")
            }, stoppingToken);

            await coreServiceClientUser1.CreatePostAsync(rejectedThreadId2,
                new CreatePostRequestBody { Content = PostContent.From("Это сообщение уже не актуально") },
                stoppingToken);

            await coreServiceClientUser1.RequestThreadApprovalAsync(rejectedThreadId2, stoppingToken);
            await moderatorCoreServiceClient.RejectThreadAsync(rejectedThreadId2, stoppingToken);
        }

        _appLifetime.StopApplication();
    }
}