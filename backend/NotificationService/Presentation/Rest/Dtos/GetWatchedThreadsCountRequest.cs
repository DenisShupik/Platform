using Shared.Presentation.Generator.Attributes;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetWatchedThreadsCountRequest;
