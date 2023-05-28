using AutoFixture;
using FluentAssertions;
using PubSubEasy.Tests.Fixture;

namespace PubSubEasy.Tests;

public class PubSubQueueTests
{
    private readonly PubSubQueue<string> _pubSubQueue;
    private readonly PubSubQueueFixture _fixture;

    public PubSubQueueTests()
    {
        _pubSubQueue = new PubSubQueue<string>(10000);
        _fixture = new PubSubQueueFixture();
    }

    [Fact]
    public async Task Test1()
    {
        var firstItem = _fixture.Create<string>();
        var secondItem = _fixture.Create<string>();

        _pubSubQueue.Publish(firstItem);

        await Task.Run(async () =>
        {
            await Task.Delay(50);
            _pubSubQueue.Publish(secondItem);
        });

        await foreach (var item in _pubSubQueue.Consume(TimeSpan.FromMilliseconds(1000)))
        {
            if (item == secondItem) break;
        }

        await foreach (var item in _pubSubQueue.Consume(TimeSpan.FromMilliseconds(1000)))
        {
            if (item == secondItem) break;
        }
    }

    [Fact]
    public async Task Test2()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(50)).Token;
        var action = async () =>
        {
            await foreach (var _ in _pubSubQueue.Consume(cancellationToken))
            {
            }
        };

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Test3()
    {
        var action = async () =>
        {
            await foreach (var _ in _pubSubQueue.Consume(TimeSpan.FromMilliseconds(50)))
            {
            }
        };

        await action.Should().ThrowAsync<OperationCanceledException>();
    }


    [Fact]
    public async Task Test4()
    {
        var pubSubQueue = new PubSubQueue<string>(5);

        var listItems = _fixture.CreateMany<string>(10).ToList();

        foreach (var item in listItems)
        {
            pubSubQueue.Publish(item);
        }
        

        var items = new List<string>();

        await foreach (var item in pubSubQueue.Consume(TimeSpan.FromSeconds(1)))
        {
            items.Add(item);

            if (item == listItems.Last())
            {
                break;
            }
        }

        items.Should().BeEquivalentTo(listItems.TakeLast(5));
    }
}