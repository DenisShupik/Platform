using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using SharedKernel.Application.Interfaces;

namespace CoreService.Application.UseCases;

[IncludeAsRequired(typeof(Forum),nameof(Forum.Title), nameof(Forum.CreatedBy))]
public sealed partial class CreateForumCommand;

public sealed class CreateForumCommandHandler
{
    private readonly IForumRepository _forumRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateForumCommandHandler(
        IForumRepository forumRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumRepository = forumRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ForumId> HandleAsync(CreateForumCommand request, CancellationToken cancellationToken)
    {
        var forum = new Forum(request.Title, request.CreatedBy, DateTime.UtcNow);
        await _forumRepository.AddAsync(forum, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return forum.ForumId;
    }
}