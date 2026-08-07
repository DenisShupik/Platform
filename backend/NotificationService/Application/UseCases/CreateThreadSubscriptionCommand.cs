using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using Npgsql;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

using CreateThreadSubscriptionCommandResult = Result<Success, DuplicateThreadSubscriptionError, NotAdminError>;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class CreateThreadSubscriptionCommand : ICommand<CreateThreadSubscriptionCommandResult>
{
    public required EnumSet<ChannelType> Channels { get; init; }
    public required UserIdRole RequestedBy { get; init; }
}

public sealed class
    CreateThreadSubscriptionCommandHandler : ICommandHandler<CreateThreadSubscriptionCommand,
    CreateThreadSubscriptionCommandResult>
{
    private readonly IThreadSubscriptionWriteRepository _threadSubscriptionWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadSubscriptionCommandHandler(
        IThreadSubscriptionWriteRepository threadSubscriptionWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadSubscriptionWriteRepository = threadSubscriptionWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateThreadSubscriptionCommandResult> HandleAsync(
        CreateThreadSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.UserId != command.RequestedBy.UserId && command.RequestedBy.Role != Role.Administrator)
            return new NotAdminError();

        var threadSubscription = new ThreadSubscription(command.UserId, command.ThreadId, command.Channels);
        _threadSubscriptionWriteRepository.Add(threadSubscription);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                return new DuplicateThreadSubscriptionError(command.UserId, command.ThreadId);
            throw;
        }

        return Success.Instance;
    }
}
