using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

[Omit(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId), nameof(Forum.Categories))]
public sealed partial class CreateForumCommand : ICommand<ForumId>;

public sealed class CreateForumCommandHandler : ICommandHandler<CreateForumCommand, ForumId>
{
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateForumCommandHandler(
        IForumWriteRepository forumWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ForumId> HandleAsync(CreateForumCommand command, CancellationToken cancellationToken)
    {
        var forum = new Forum(command.Title, command.CreatedBy, DateTime.UtcNow, command.AccessPolicyId,
            command.CategoryCreatePolicyId, command.ThreadCreatePolicyId, command.PostCreatePolicyId);
        await _forumWriteRepository.AddAsync(forum, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return forum.ForumId;
    }
}