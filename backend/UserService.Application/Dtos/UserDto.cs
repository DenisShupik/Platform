using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;

namespace UserService.Application.Dtos;

[Include(typeof(User), PropertyGenerationMode.AsPublic,
    nameof(User.UserId), nameof(User.Username), nameof(User.Enabled), nameof(User.CreatedAt))]
public sealed partial class UserDto;
