using Common;

namespace TopicService.Application.DTOs;

public sealed class GetSectionsRequest : LongKeysetPageRequest
{
}

public sealed class GetSectionsRequestValidator : LongKeysetPageRequestValidator<GetSectionsRequest>
{
}