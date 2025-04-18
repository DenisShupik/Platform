using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class CreateForumCommand
{
    /// <summary>
    /// Название форума
    /// </summary>
    public required ForumTitle Title { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId UserId { get; init; }
}

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
        var forum = new Forum
        {
            ForumId = ForumId.From(Guid.CreateVersion7()),
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = request.UserId
        };
        await _forumRepository.AddAsync(forum, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return forum.ForumId;
    }
}