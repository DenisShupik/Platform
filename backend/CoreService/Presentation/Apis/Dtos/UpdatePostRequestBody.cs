using CoreService.Domain.ValueObjects;

namespace CoreService.Presentation.Apis.Dtos;

public sealed class UpdatePostRequestBody
{
    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    public PostContent Content { get; set; }

    /// <summary>
    /// Маркер версии записи
    /// </summary>
    public uint RowVersion { get; set; }
}