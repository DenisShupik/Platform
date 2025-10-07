using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.Extensions.Hosting;
using NotificationService.Domain.Enums;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Tests.Dtos;
using Shared.Tests.Services;
using CreateCategoryRequestBody = CoreService.Presentation.Rest.Dtos.CreateCategoryRequestBody;
using CreateForumRequestBody = CoreService.Presentation.Rest.Dtos.CreateForumRequestBody;
using CreatePostRequestBody = CoreService.Presentation.Rest.Dtos.CreatePostRequestBody;
using CreateThreadRequestBody = CoreService.Presentation.Rest.Dtos.CreateThreadRequestBody;
using CreateThreadSubscriptionRequestBody =
    NotificationService.Presentation.Rest.Dtos.CreateThreadSubscriptionRequestBody;

namespace DevEnv.Seeder.Services;

public sealed class Seeder : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Fixture _fixture;
    private readonly KeycloakAdminClient _keycloakAdminClient;
    private readonly IHttpClientFactory _httpClientFactory;

    private const int ForumCount = 2;
    private const int CategoryPerForum = 1;
    private const int ThreadPerCategory = 2;
    private const int PostPerThread = 20;

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

        await Task.WhenAll(_fixture.Users.Select(user => _keycloakAdminClient.CreateUserAsync(new CreateUserRequestBody
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

        var executionOptions = new ExecutionDataflowBlockOptions
            { MaxDegreeOfParallelism = Environment.ProcessorCount };

        var accessPolicyId = await randomUserCoreServiceClient.CreatePolicyAsync(
            new CreatePolicyRequestBody
            {
                Type = PolicyType.Access,
                Value = PolicyValue.Any
            },
            stoppingToken);

        var categoryCreatePolicyId = await randomUserCoreServiceClient.CreatePolicyAsync(
            new CreatePolicyRequestBody
            {
                Type = PolicyType.CategoryCreate,
                Value = PolicyValue.Any
            },
            stoppingToken);

        var threadCreatePolicyId = await randomUserCoreServiceClient.CreatePolicyAsync(
            new CreatePolicyRequestBody
            {
                Type = PolicyType.ThreadCreate,
                Value = PolicyValue.Any
            },
            stoppingToken);

        var postCreatePolicyId = await randomUserCoreServiceClient.CreatePolicyAsync(
            new CreatePolicyRequestBody
            {
                Type = PolicyType.PostCreate,
                Value = PolicyValue.Any
            },
            stoppingToken);

        var createForumBlock = new TransformBlock<int, ForumId>(async i =>
                await randomUserCoreServiceClient.CreateForumAsync(
                    new CreateForumRequestBody
                    {
                        Title = ForumTitle.From($"Новый форум {i}"),
                        AccessPolicyValue = PolicyValue.Any,
                        CategoryCreatePolicyValue = PolicyValue.Any,
                        ThreadCreatePolicyValue = PolicyValue.Any,
                        PostCreatePolicyValue = PolicyValue.Any
                    },
                    stoppingToken),
            executionOptions);

        var createCategoryBlock = new TransformManyBlock<ForumId, CreateCategoryRequestBody>(forumId =>
                Enumerable
                    .Range(1, CategoryPerForum)
                    .Select(i => new CreateCategoryRequestBody
                    {
                        ForumId = forumId,
                        Title = CategoryTitle.From($"Новый раздел {i}"),
                        AccessPolicyId = accessPolicyId,
                        ThreadCreatePolicyId = threadCreatePolicyId,
                        PostCreatePolicyId = postCreatePolicyId
                    })
                    .ToArray(),
            executionOptions);

        var createThreadBlock = new TransformManyBlock<CreateCategoryRequestBody, CreateThreadRequestBody>(
            async request =>
            {
                var categoryId =
                    await randomUserCoreServiceClient.CreateCategoryAsync(request, stoppingToken);

                return Enumerable
                    .Range(1, ThreadPerCategory)
                    .Select(i => new CreateThreadRequestBody
                    {
                        CategoryId = categoryId,
                        Title = ThreadTitle.From($"Новая тема {i}"),
                        AccessPolicyId = accessPolicyId,
                        PostCreatePolicyId = postCreatePolicyId
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

        for (var i = 0; i < ForumCount; i++)
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

            await notificationServiceClientUser1.CreateThreadSubscriptionAsync(new CreateThreadSubscriptionRequest
            {
                ThreadId = threadIdArray[0],
                Body = new CreateThreadSubscriptionRequestBody
                {
                    Channels = [ChannelType.Internal]
                }
            }, stoppingToken);

            await notificationServiceClientUser1.CreateThreadSubscriptionAsync(new CreateThreadSubscriptionRequest
            {
                ThreadId = threadIdArray[1],
                Body = new CreateThreadSubscriptionRequestBody
                {
                    Channels = [ChannelType.Internal]
                }
            }, stoppingToken);

            await notificationServiceClientUser1.CreateThreadSubscriptionAsync(new CreateThreadSubscriptionRequest
            {
                ThreadId = threadIdArray[2],
                Body = new CreateThreadSubscriptionRequestBody
                {
                    Channels = [ChannelType.Internal]
                }
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
        }

        _appLifetime.StopApplication();
    }
}