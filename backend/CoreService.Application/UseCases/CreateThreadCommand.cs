using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using OneOf;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.CategoryId), nameof(Thread.Title),
    nameof(Thread.CreatedBy), nameof(Thread.AccessLevel))]
public sealed partial class CreateThreadCommand : ICommand<OneOf<ThreadId, CategoryNotFoundError>>;

public sealed class
    CreateThreadCommandHandler : ICommandHandler<CreateThreadCommand, OneOf<ThreadId, CategoryNotFoundError>>
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

    public async Task<OneOf<ThreadId, CategoryNotFoundError>> HandleAsync(CreateThreadCommand command,
        CancellationToken cancellationToken)
    {
        var categoryOrError =
            await _categoryWriteRepository.GetAsync<CategoryThreadAddable>(command.CategoryId, cancellationToken);

        if (categoryOrError.TryPickT1(out var error, out var category)) return error;

        var thread = category.AddThread(command.Title, command.CreatedBy, DateTime.UtcNow, command.AccessLevel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return thread.ThreadId;
    }
}