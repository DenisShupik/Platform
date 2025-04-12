using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using FluentValidation;

namespace CoreService.Application.UseCases;

public sealed class CreateCategoryRequest
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public ForumId ForumId { get; set; }

    /// <summary>
    /// Наименование категории
    /// </summary>
    public CategoryTitle Title { get; set; }
}