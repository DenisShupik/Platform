using System.Threading.Channels;

namespace SharedKernel.Infrastructure.Abstractions;

public abstract class DataLoader<TKey, TValue> : IDisposable where TKey : notnull
{
    private readonly int _maxBatchSize;
    private readonly TimeSpan _maxWaitTime;

    private readonly Channel<Request> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _worker;
    private bool _disposed;

    protected DataLoader(int maxBatchSize, TimeSpan maxWaitTime)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxBatchSize);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxWaitTime, TimeSpan.Zero);

        _maxBatchSize = maxBatchSize;
        _maxWaitTime = maxWaitTime;

        _channel = Channel.CreateUnbounded<Request>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _worker = Task.Run(WorkerLoopAsync);
    }

    protected abstract Task<Dictionary<TKey, TValue>> FetchAsync(IReadOnlyList<TKey> keys, CancellationToken cancellationToken);

    public Task<TValue> LoadAsync(TKey key)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataLoader<TKey, TValue>));

        var tcs = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);

        var req = new Request(key, tcs);

        if (!_channel.Writer.TryWrite(req))
        {
            tcs.TrySetException(new ObjectDisposedException(nameof(DataLoader<TKey, TValue>)));
        }

        return tcs.Task;
    }

    private async Task WorkerLoopAsync()
    {
        var reader = _channel.Reader;
        try
        {
            while (await reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
            {
                if (!reader.TryRead(out var first)) continue;

                var map = new Dictionary<TKey, List<Request>>(capacity: Math.Min(16, _maxBatchSize));
                AddToMap(map, first);

                var delayTask = Task.Delay(_maxWaitTime, _cts.Token);

                while (map.Count < _maxBatchSize)
                {
                    if (reader.TryRead(out var next))
                    {
                        AddToMap(map, next);
                        if (map.Count >= _maxBatchSize) break;
                        continue;
                    }

                    var waitTask = reader.WaitToReadAsync(_cts.Token).AsTask();
                    var completed = await Task.WhenAny(waitTask, delayTask).ConfigureAwait(false);

                    if (completed == delayTask) break;
                }

                var keys = new List<TKey>(map.Count);
                keys.AddRange(map.Keys);

                try
                {
                    var results = await FetchAsync(keys, _cts.Token).ConfigureAwait(false);

                    foreach (var (key, requests) in map)
                    {
                        if (results != null && results.TryGetValue(key, out var value))
                        {
                            foreach (var r in requests)
                            {
                                r.Tcs.TrySetResult(value);
                            }
                        }
                        else
                        {
                            var ex = new KeyNotFoundException($"Key '{key}' not found in fetch results.");
                            foreach (var r in requests)
                            {
                                r.Tcs.TrySetException(ex);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (_cts.IsCancellationRequested)
                {
                    foreach (var r in map.SelectMany(kv => kv.Value))
                        r.Tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    foreach (var r in map.SelectMany(kv => kv.Value))
                        r.Tcs.TrySetException(ex);
                }
            }
        }
        catch (OperationCanceledException) { /* корректная остановка */ }
        catch (Exception ex)
        {
            while (reader.TryRead(out var leftover))
            {
                leftover.Tcs.TrySetException(ex);
            }
        }
    }

    private static void AddToMap(Dictionary<TKey, List<Request>> map, Request req)
    {
        if (map.TryGetValue(req.Key, out var list))
            list.Add(req);
        else
            map[req.Key] = [req];
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        _channel.Writer.Complete();

        try { _worker.Wait(); } catch { /* игнорируем */ }
        _cts.Dispose();
    }

    private readonly struct Request
    {
        public TKey Key { get; }
        public TaskCompletionSource<TValue> Tcs { get; }

        public Request(TKey key, TaskCompletionSource<TValue> tcs)
        {
            Key = key;
            Tcs = tcs;
        }
    }
}