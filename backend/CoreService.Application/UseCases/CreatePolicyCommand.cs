using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using CreatePolicyCommandResult = PolicyId;

[Omit(typeof(Policy), PropertyGenerationMode.AsRequired, nameof(Policy.PolicyId))]
public sealed partial class CreatePolicyCommand : ICommand<CreatePolicyCommandResult>
{
    public required UserId CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public sealed class CreatePolicyCommandHandler : ICommandHandler<CreatePolicyCommand, CreatePolicyCommandResult>
{
    private readonly IAccessWriteRepository _accessWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePolicyCommandHandler(
        IAccessWriteRepository accessWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _accessWriteRepository = accessWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePolicyCommandResult> HandleAsync(CreatePolicyCommand command,
        CancellationToken cancellationToken)
    {
        var policy = new Policy(command.Type, command.Value);
        await _accessWriteRepository.AddAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return policy.PolicyId;
    }
}