using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using FluentValidation;

namespace CoreService.Application.UseCases;

public sealed class CreateForumRequest
{
    /// <summary>
    /// Наименование раздела
    /// </summary>
    public ForumTitle Title { get; set; }
}