using FluentValidation;
using Generator.Attributes;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using Npgsql;
using OneOf;
using OneOf.Types;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.Helpers;

namespace NotificationService.Application.UseCases;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class CreateThreadSubscriptionCommand
{
    /// <summary>
    /// Каналы, по которым пользователь подписан на уведомления по данной теме
    /// </summary>
    public required HashSet<ChannelType> Channels { get; init; }
}

public sealed class CreateThreadSubscriptionCommandValidator : AbstractValidator<CreateThreadSubscriptionCommand>
{
    public CreateThreadSubscriptionCommandValidator()
    {
        RuleFor(e => e.Channels).NotEmpty();
        RuleForEach(e => e.Channels).IsInEnum();
    }
}

[GenerateOneOf]
public partial class CreateThreadSubscriptionResult : OneOfBase<Success, DuplicateThreadSubscriptionError>;

public sealed class CreateThreadSubscriptionCommandHandler
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

    public async Task<CreateThreadSubscriptionResult> HandleAsync(CreateThreadSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var threadSubscription = new ThreadSubscription(request.UserId, request.ThreadId, request.Channels.ToArray());
        await _threadSubscriptionWriteRepository.AddAsync(threadSubscription, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                return new DuplicateThreadSubscriptionError(request.UserId, request.ThreadId);
            throw;
        }

        return OneOfHelper.Success;
    }
}