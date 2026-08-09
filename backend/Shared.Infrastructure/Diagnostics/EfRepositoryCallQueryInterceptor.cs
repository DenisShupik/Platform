using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace Shared.Infrastructure.Diagnostics;

internal sealed class EfRepositoryCallQueryInterceptor(RepositoryCallContextAccessor contextAccessor)
    : IQueryExpressionInterceptor
{
    private static readonly MethodInfo TagWithMethod = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method =>
            method.Name == nameof(EntityFrameworkQueryableExtensions.TagWith) &&
            method.IsGenericMethodDefinition &&
            method.GetParameters() is [{ ParameterType: var queryType }, { ParameterType: var tagType }] &&
            queryType.IsGenericType &&
            queryType.GetGenericTypeDefinition() == typeof(IQueryable<>) &&
            tagType == typeof(string));

    public Expression QueryCompilationStarting(Expression queryExpression, QueryExpressionEventData eventData)
    {
        var repositoryCall = contextAccessor.Current;
        return repositoryCall is null
            ? queryExpression
            : new RepositoryCallTagVisitor(repositoryCall).Visit(queryExpression);
    }

    private sealed class RepositoryCallTagVisitor(string repositoryCall) : ExpressionVisitor
    {
        private bool _tagged;

        protected override Expression VisitExtension(Expression node)
        {
            if (_tagged || node is not QueryRootExpression queryRoot)
            {
                return base.VisitExtension(node);
            }

            _tagged = true;
            return Expression.Call(
                TagWithMethod.MakeGenericMethod(queryRoot.ElementType),
                queryRoot,
                Expression.Constant(repositoryCall));
        }
    }
}
