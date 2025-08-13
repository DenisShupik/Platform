using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using SharedKernel.Application.Interfaces;
using OneOf;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.CategoryId), nameof(Thread.Title), nameof(Thread.CreatedBy))]
public sealed partial class CreateThreadCommand;

[GenerateOneOf]
public partial class CreateThreadCommandResult : OneOfBase<ThreadId, CategoryNotFoundError>;

public sealed class CreateThreadCommandHandler
{
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadCommandHandler(
        ICategoryWriteRepository categoryWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _categoryWriteRepository = categoryWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateThreadCommandResult> HandleAsync(CreateThreadCommand request,
        CancellationToken cancellationToken)
    {
        var categoryOrError =
            await _categoryWriteRepository.GetAsync<CategoryThreadAddable>(request.CategoryId, cancellationToken);

        if (categoryOrError.TryPickT1(out var error, out var category)) return error;

        var thread = category.AddThread(request.Title, request.CreatedBy, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return thread.ThreadId;
    }
}