using System.Text.Json.Serialization;
using CoreService.Application.Enums;
using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.Dtos;

[JsonPolymorphic]
[JsonDerivedType(typeof(PostAddedActivityDto), nameof(ActivityType.PostAdded))]
[Omit(typeof(Activity), PropertyGenerationMode.AsPublic)]
public abstract partial class ActivityDto;

[Omit(typeof(PostAddedActivity), PropertyGenerationMode.AsPublic)]
public sealed partial class PostAddedActivityDto : ActivityDto;