using System;
using Shared.Domain.ValueObjects;

namespace Shared.Application.Interfaces;

public interface ICommand<TResponse> : IRequest<TResponse>;

public interface ICreateCommand<TResponse> : ICommand<TResponse>
{
    ActorContext RequestedBy { get; init; }
    DateTime CreatedAt { get; init; }
}

public interface IUpdateCommand<TResponse> : ICommand<TResponse>
{
    ActorContext RequestedBy { get; init; }
    DateTime UpdatedAt { get; init; }
}

public interface IDeleteCommand<TResponse> : ICommand<TResponse>
{
    ActorContext RequestedBy { get; init; }
    DateTime DeletedAt { get; init; }
}
