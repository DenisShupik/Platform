using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Application.Enums;
using SharedKernel.Infrastructure.Extensions;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostReadRepository : IPostReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public PostReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, PostNotFoundError>> GetOneAsync<T>(PostId postId, CancellationToken cancellationToken)
    {
        var post = await _dbContext.Posts
            .Where(e => e.PostId == postId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (post == null) return new PostNotFoundError(postId);

        return post;
    }

    public async Task<IReadOnlyList<T>> GetThreadPostsAsync<T>(GetThreadPostsPagedQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Posts.Where(e => e.ThreadId == request.ThreadId);

        if (request.Sort is { Field: GetThreadPostsPagedQuery.GetThreadPostsPagedQuerySortType.Index } sort)
        {
            query = sort.Order == SortOrderType.Ascending
                ? query.OrderBy(e => e.CreatedAt).ThenBy(e => e.PostId)
                : query.OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.PostId);
        }

        return await query
            .ApplyPagination(request)
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);
    }
}