using System;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;

namespace Shared.Application.Interfaces;

public interface ICommand<TResponse> : IRequest<TResponse>;

public interface ICreateCommand<TResponse> : ICommand<TResponse>
{
    UserId CreatedBy { get; init; }
    DateTime CreatedAt { get; init; }
    Role CreatorRole { get; init; }
}

public interface IUpdateCommand<TResponse> : ICommand<TResponse>
{
    UserId UpdatedBy { get; init; }
    DateTime UpdatedAt { get; init; }
    Role UpdaterRole { get; init; }
}

public interface IDeleteCommand<TResponse> : ICommand<TResponse>
{
    UserId DeletedBy { get; init; }
    DateTime DeletedAt { get; init; }
    Role DeleterRole { get; init; }
}