using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.ValueObjects;
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

[GenerateOneOf]
public partial class CreateThreadCommandResult : OneOfBase<CategoryNotFoundError, ThreadId>;

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

    public async Task<CreateThreadCommandResult> HandleAsync(CreateThreadCommand request,
        CancellationToken cancellationToken)
    {
        var categoryOrError =
            await _categoryRepository.GetAsync<CategoryThreadAddable>(request.CategoryId, cancellationToken);

        if (categoryOrError.TryPickT1(out var error, out var category)) return error;

        var thread = category.AddThread(request.Title, request.UserId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return thread.ThreadId;
    }
}