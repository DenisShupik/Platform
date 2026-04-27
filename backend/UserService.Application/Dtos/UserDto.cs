using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;

namespace UserService.Application.Dtos;

[Omit(typeof(User), PropertyGenerationMode.AsPublic)]
public sealed partial class UserDto;