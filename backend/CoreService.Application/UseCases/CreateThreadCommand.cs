using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CreateThreadCommandResult = Result<
    ThreadId,
    CategoryNotFoundError,
    PolicyViolationError,
    PolicyRestrictedError,
    PolicyDowngradeError
>;

[Omit(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId), nameof(Thread.Status),
    nameof(Thread.ReadPolicyId), nameof(Thread.PostCreatePolicyId), nameof(Thread.Posts))]
public sealed partial class CreateThreadCommand : ICommand<CreateThreadCommandResult>
{
    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public required PolicyValue? ReadPolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public required PolicyValue? PostCreatePolicyValue { get; init; }
}

public sealed class
    CreateThreadCommandHandler : ICommandHandler<CreateThreadCommand, CreateThreadCommandResult>
{
    private readonly IAccessReadRepository _accessReadRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadCommandHandler(
        IAccessReadRepository accessReadRepository,
        ICategoryWriteRepository categoryWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _categoryWriteRepository = categoryWriteRepository;
        _unitOfWork = unitOfWork;
        _accessReadRepository = accessReadRepository;
    }

    public async Task<CreateThreadCommandResult> HandleAsync(CreateThreadCommand command,
        CancellationToken cancellationToken)
    {
        {
            if (!(await _accessReadRepository.EvaluatedCategoryPolicy(command.CategoryId, command.CreatedBy,
                    PolicyType.ThreadCreate, command.CreatedAt, cancellationToken))
                .TryOrExtend<ThreadId, PolicyDowngradeError>(out var error))
                return error.Value;
        }
        
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var categoryResult =
            await _categoryWriteRepository.GetAsync(command.CategoryId, cancellationToken);

        CategoryThreadAddable category;
        {
            if (!categoryResult.TryGet(out var value, out var error)) return error;
            category = value;
        }

        Thread thread;
        {
            if (!category
                    .AddThread(command.Title, command.CreatedBy, command.CreatedAt, command.ReadPolicyValue,
                        command.PostCreatePolicyValue)
                    .TryGet(out var value, out var error)
               ) return error;
            thread = value;
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return thread.ThreadId;
    }
}