namespace SharedKernel.Paging;

public sealed class KeysetPageResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = null!;
}