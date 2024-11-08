using SharedKernel;
using SharedKernel.Paging;

namespace CoreService.Application.DTOs;

public sealed class GetForumsRequest : LongKeysetPageRequest
{
}

public sealed class GetForumsRequestValidator : LongKeysetPageRequestValidator<GetForumsRequest>
{
}