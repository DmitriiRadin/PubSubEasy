using System.Runtime.CompilerServices;

namespace PubSubEasy;

public interface IPubSubQueue<TQueueItem>
{
    void Publish(TQueueItem queueItem);
    ConfiguredCancelableAsyncEnumerable<TQueueItem> Consume(CancellationToken cancellationToken);
    ConfiguredCancelableAsyncEnumerable<TQueueItem> Consume(TimeSpan timeout);
}