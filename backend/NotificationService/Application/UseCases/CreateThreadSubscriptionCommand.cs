using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using Npgsql;
using OneOf;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace NotificationService.Application.UseCases;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class CreateThreadSubscriptionCommand : ICommand<OneOf<Success, DuplicateThreadSubscriptionError>>
{
    public required EnumSet<ChannelType> Channels { get; init; }
}

public sealed class
    CreateThreadSubscriptionCommandHandler : ICommandHandler<CreateThreadSubscriptionCommand,
    OneOf<Success, DuplicateThreadSubscriptionError>>
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

    public async Task<OneOf<Success, DuplicateThreadSubscriptionError>> HandleAsync(
        CreateThreadSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var threadSubscription = new ThreadSubscription(command.UserId, command.ThreadId, command.Channels);
        await _threadSubscriptionWriteRepository.AddAsync(threadSubscription, cancellationToken);

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