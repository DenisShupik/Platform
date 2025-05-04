using Generator.Attributes;
using UserService.Domain.Entities;

namespace UserService.Application.Dtos;

[Omit(typeof(User))]
public sealed partial class UserDto;