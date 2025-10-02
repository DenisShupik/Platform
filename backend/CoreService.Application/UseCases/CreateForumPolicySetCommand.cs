using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Omit(typeof(ForumPolicySet), PropertyGenerationMode.AsRequired, nameof(ForumPolicySet.ForumPolicySetId),
    nameof(ForumPolicySet.UpdatedBy), nameof(ForumPolicySet.UpdatedAt))]
public sealed partial class CreateForumPolicySetCommand : ICommand<ForumPolicySetId>
{
    public required UserId CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public sealed class CreateForumPolicySetCommandHandler : ICommandHandler<CreateForumPolicySetCommand, ForumPolicySetId>
{
    private readonly IPolicySetWriteRepository _policySetWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateForumPolicySetCommandHandler(
        IPolicySetWriteRepository policySetWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _policySetWriteRepository = policySetWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ForumPolicySetId> HandleAsync(CreateForumPolicySetCommand command,
        CancellationToken cancellationToken)
    {
        var policySet = new ForumPolicySet(command.CategoryCreate, command.ThreadCreate, command.PostCreate,
            command.Access, command.CreatedBy, command.CreatedAt);
        await _policySetWriteRepository.AddAsync(policySet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return policySet.ForumPolicySetId;
    }
}