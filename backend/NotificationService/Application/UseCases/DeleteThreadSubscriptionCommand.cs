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
public sealed partial class DeleteThreadSubscriptionCommand;

[GenerateOneOf]
public partial class DeleteThreadSubscriptionCommandResult : OneOfBase<ThreadSubscriptionNotFoundError, Success>;

public sealed class DeleteThreadSubscriptionCommandHandler
{
    private readonly IThreadSubscriptionRepository _threadSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteThreadSubscriptionCommandHandler(
        IThreadSubscriptionRepository threadSubscriptionRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadSubscriptionRepository = threadSubscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteThreadSubscriptionCommandResult> HandleAsync(DeleteThreadSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        _threadSubscriptionRepository.Remove(request.UserId, request.ThreadId, cancellationToken);

        if (await _unitOfWork.SaveChangesAsync(cancellationToken) == 0)
        {
            return new ThreadSubscriptionNotFoundError(request.UserId, request.ThreadId);       
        }
        
        return OneOfHelper.Success;
    }
}