using AutoFixture.AutoMoq;

namespace PubSubEasy.Tests.Fixture;

public class PubSubQueueFixture : AutoFixture.Fixture
{
    public PubSubQueueFixture()
    {
        Customize(new AutoMoqCustomization());
    }
}