using System.Runtime.CompilerServices;

namespace PubSubEasy;

public class PubSubQueue<TQueueItem> : IPubSubQueue<TQueueItem>, IAsyncEnumerable<TQueueItem>
{
    private readonly int _maxItemCount;
    private readonly List<TQueueItem> _queueItems;
    private TaskCompletionSource _enqueueTaskCompletionSource;
    private int _reverseOffset;
    private readonly object _monitor = new();

    public PubSubQueue(int maxItemCount)
    {
        _maxItemCount = maxItemCount;
        _reverseOffset = 0;
        _queueItems = new List<TQueueItem>();
        _enqueueTaskCompletionSource = new TaskCompletionSource();
    }

    public void Publish(TQueueItem queueItem)
    {
        lock (_monitor)
        {
            _queueItems.Add(queueItem);

            while (_queueItems.Count > _maxItemCount)
            {
                _queueItems.RemoveAt(0);
                _reverseOffset++;
            }

            _enqueueTaskCompletionSource.TrySetResult();
            _enqueueTaskCompletionSource = new TaskCompletionSource();
        }
    }

    public ConfiguredCancelableAsyncEnumerable<TQueueItem> Consume(CancellationToken cancellationToken)
    {
        return this.WithCancellation(cancellationToken);
    }

    public ConfiguredCancelableAsyncEnumerable<TQueueItem> Consume(TimeSpan timeout)
    {
        var cancellationToken = new CancellationTokenSource(timeout).Token;
        return this.WithCancellation(cancellationToken);
    }

    public async IAsyncEnumerator<TQueueItem> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        var offset = _reverseOffset;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Task publishTask;

            lock (_monitor)
            {
                publishTask = _enqueueTaskCompletionSource.Task;

                if (offset - _reverseOffset < _queueItems.Count)
                {
                    var queueItem = _queueItems[offset - _reverseOffset];
                    offset++;
                    yield return queueItem;
                    continue;
                }
            }

            var cancellationTokenTaskCompletionSource = new TaskCompletionSource();
            cancellationToken.Register(() => { cancellationTokenTaskCompletionSource.SetCanceled(cancellationToken); });

            await Task.WhenAny(publishTask, cancellationTokenTaskCompletionSource.Task);
        }
    }
}