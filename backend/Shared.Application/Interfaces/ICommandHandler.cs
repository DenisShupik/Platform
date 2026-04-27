using System.Threading;
using System.Threading.Tasks;

namespace Shared.Application.Interfaces;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}