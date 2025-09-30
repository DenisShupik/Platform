using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetThreadPostsPagedQuery<>), typeof(Post))]
internal static partial class PostReadRepositoryExtensions
{
    [SortExpression<GetThreadPostsPagedQuerySortType>(GetThreadPostsPagedQuerySortType.Index)]
    private static readonly Expression<Func<Post, object>> IndexExpression = e => new { e.CreatedAt, e.PostId };
}

public sealed class PostReadRepository : IPostReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public PostReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T, PostNotFoundError>> GetOneAsync<T>(PostId postId, CancellationToken cancellationToken) where T : notnull
    {
        var post = await _dbContext.Posts
            .Where(e => e.PostId == postId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (post == null) return new PostNotFoundError(postId);

        return post;
    }

    public async Task<Result<IReadOnlyList<T>, ThreadNotFoundError>> GetThreadPostsAsync<T>(
        GetThreadPostsPagedQuery<T> request,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.Threads.AnyAsyncLinqToDB(e => e.ThreadId == request.ThreadId, cancellationToken))
            return new ThreadNotFoundError(request.ThreadId);

        var query = _dbContext.Posts
            .Where(e => e.ThreadId == request.ThreadId)
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>();

        var result = await query.ToListAsyncLinqToDB(cancellationToken);

        return result;
    }
}