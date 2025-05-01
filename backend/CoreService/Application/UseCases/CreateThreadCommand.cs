using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using SharedKernel.Application.Interfaces;
using OneOf;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

[IncludeAsRequired(typeof(Thread),nameof(Thread.CategoryId), nameof(Thread.Title), nameof(Thread.CreatedBy))]
public sealed partial class CreateThreadCommand;

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

        var thread = category.AddThread(request.Title, request.CreatedBy, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return thread.ThreadId;
    }
}