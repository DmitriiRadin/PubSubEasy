namespace PubSubEasy;

public class PubSubQueue<TQueueItem> 
{
    private readonly Queue<TQueueItem> _queueItems;

    public PubSubQueue()
    {
        _queueItems = new Queue<TQueueItem>();
    }
}