using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using SharedKernel.Application.Interfaces;

namespace CoreService.Application.UseCases;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.Title), nameof(Forum.CreatedBy))]
public sealed partial class CreateForumCommand;

public sealed class CreateForumCommandHandler
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

    public async Task<ForumId> HandleAsync(CreateForumCommand request, CancellationToken cancellationToken)
    {
        var forum = new Forum(request.Title, request.CreatedBy, DateTime.UtcNow);
        await _forumWriteRepository.AddAsync(forum, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return forum.ForumId;
    }
}