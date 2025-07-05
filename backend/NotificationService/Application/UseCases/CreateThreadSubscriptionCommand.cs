using Generator.Attributes;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Npgsql;
using OneOf;
using OneOf.Types;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.Helpers;

namespace NotificationService.Application.UseCases;

[IncludeAsRequired(typeof(ThreadSubscription), nameof(ThreadSubscription.UserId), nameof(ThreadSubscription.ThreadId))]
public sealed partial class CreateThreadSubscriptionCommand;

[GenerateOneOf]
public partial class CreateThreadSubscriptionResult : OneOfBase<DuplicateThreadSubscriptionError, Success>;

public sealed class CreateThreadSubscriptionCommandHandler
{
    private readonly IThreadSubscriptionRepository _threadSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadSubscriptionCommandHandler(
        IThreadSubscriptionRepository threadSubscriptionRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadSubscriptionRepository = threadSubscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateThreadSubscriptionResult> HandleAsync(CreateThreadSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var threadSubscription = new ThreadSubscription(request.UserId, request.ThreadId);
        await _threadSubscriptionRepository.AddAsync(threadSubscription, cancellationToken);

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