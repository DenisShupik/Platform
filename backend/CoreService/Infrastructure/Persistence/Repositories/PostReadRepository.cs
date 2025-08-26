using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Generator.Attributes;

namespace CoreService.Infrastructure.Persistence.Repositories;

[AddApplySort(typeof(GetThreadPostsPagedQuery.GetThreadPostsPagedQuerySortType), typeof(Post))]
internal static partial class PostReadRepositoryExtensions
{
    private static readonly Expression<Func<Post, object>> IndexExpression = e => new { e.CreatedAt, e.PostId };
}

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
        var query = _dbContext.Posts
            .Where(e => e.ThreadId == request.ThreadId)
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>();

        var result = await query.ToListAsyncLinqToDB(cancellationToken);

        return result;
    }
}