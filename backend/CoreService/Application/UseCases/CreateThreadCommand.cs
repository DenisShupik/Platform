using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;
using OneOf;

namespace CoreService.Application.UseCases;

public sealed class CreateThreadCommand
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Название темы
    /// </summary>
    public required ThreadTitle Title { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId UserId { get; init; }
}

public sealed class CreateThreadCommandHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork
    )
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OneOf<ThreadId, CategoryNotFoundError>> HandleAsync(CreateThreadCommand request,
        CancellationToken cancellationToken)
    {
        var categoryOrError = await _categoryRepository.GetAsync<CategoryThreadAddable>(request.CategoryId, cancellationToken);

        if (categoryOrError.TryPickT1(out var error, out var category)) return error;

        var thread = new Thread
        {
            ThreadId = ThreadId.From(Guid.CreateVersion7()),
            CategoryId = request.CategoryId,
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = request.UserId
        };

        category.AddThread(thread);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return thread.ThreadId;
    }
}