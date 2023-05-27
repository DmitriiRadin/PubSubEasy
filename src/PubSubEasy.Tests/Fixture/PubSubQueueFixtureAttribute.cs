using AutoFixture.Xunit2;

namespace PubSubEasy.Tests.Fixture;

public class PubSubQueueFixtureAttribute : AutoDataAttribute
{
    public PubSubQueueFixtureAttribute() : base(() => new PubSubQueueFixture())
    {
    }
}