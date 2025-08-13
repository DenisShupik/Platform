using Mapster;
using SharedKernel.Infrastructure.Abstractions;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Cache;
using UserService.Infrastructure.Grpc.Contracts;

namespace UserService.Infrastructure.Grpc.Client;

public sealed class UserServiceGrpcClient : IUserServiceClient
{
    private sealed class UserDataLoader : DataLoader<UserId, UserDto>
    {
        private readonly IGrpcUserService _grpcClient;

        public UserDataLoader(IGrpcUserService grpcClient, int maxBatchSize, TimeSpan maxWaitTime) : base(
            maxBatchSize, maxWaitTime)
        {
            _grpcClient = grpcClient;
        }

        protected override async Task<Dictionary<UserId, UserDto>> FetchAsync(IReadOnlyList<UserId> keys,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _grpcClient.GetUsersAsync(
                    new GetUsersRequest { UserIds = keys.ToHashSet() }, cancellationToken);
                return response.Users.Select(user => user.Adapt<UserDto>()).ToDictionary(userDto => userDto.UserId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private readonly IUserServiceCache _cache;
    private readonly IGrpcUserService _grpcClient;
    private readonly UserDataLoader _dataLoader;

    public UserServiceGrpcClient(IUserServiceCache cache, IGrpcUserService grpcClient)
    {
        _cache = cache;
        _grpcClient = grpcClient;
        _dataLoader = new UserDataLoader(_grpcClient, 100, TimeSpan.FromMilliseconds(25));
    }

    public async ValueTask<UserDto> GetUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            userId,
            async ct =>
            {
                var response = await _grpcClient.GetUserAsync(
                    new GetUserRequest { UserId = userId }, ct);
                return response.Adapt<UserDto>();
            }, cancellationToken: cancellationToken);
    }

    public async ValueTask<IReadOnlyList<UserDto>> GetUsersAsync(ICollection<UserId> userIds,
        CancellationToken cancellationToken)
    {
        var tasks = userIds
            .Select(userId =>
                _cache.GetOrSetAsync(userId, _ => _dataLoader.LoadAsync(userId), cancellationToken).AsTask());
        return await Task.WhenAll(tasks);
    }
}