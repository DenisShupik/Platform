namespace Shared.Infrastructure.Diagnostics;

internal sealed class RepositoryCallContextAccessor
{
    private readonly AsyncLocal<Context?> _current = new();

    public string? Current => _current.Value?.Name;

    public IDisposable Push(string name)
    {
        var parent = _current.Value;
        _current.Value = new Context(name);
        return new PopWhenDisposed(this, parent);
    }

    private sealed record Context(string Name);

    private sealed class PopWhenDisposed(RepositoryCallContextAccessor accessor, Context? parent) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            accessor._current.Value = parent;
            _disposed = true;
        }
    }
}
