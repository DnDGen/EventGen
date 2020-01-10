using NUnit.Framework;

namespace DnDGen.EventGen.Tests.Integration.IoC.Modules
{
    [TestFixture]
    public class EventGenModuleTests : IntegrationTests
    {
        [Test]
        public void GenEventQueueIsInjected()
        {
            var eventQueue = GetNewInstanceOf<GenEventQueue>();
            Assert.That(eventQueue, Is.Not.Null);
            Assert.That(eventQueue, Is.InstanceOf<DomainGenEventQueue>());
        }

        [Test]
        public void GenEventQueueIsInjectedAsSingleton()
        {
            var eventQueue = GetNewInstanceOf<GenEventQueue>();
            var second = GetNewInstanceOf<GenEventQueue>();
            Assert.That(eventQueue, Is.EqualTo(second));
        }

        [Test]
        public void ClientIDManagerIsInjected()
        {
            var clientIdManager = GetNewInstanceOf<ClientIDManager>();
            Assert.That(clientIdManager, Is.Not.Null);
            Assert.That(clientIdManager, Is.InstanceOf<ThreadClientIDManager>());
        }

        [Test]
        public void ClientIDManagerIsInjectedAsSingleton()
        {
            var clientIdManager = GetNewInstanceOf<ClientIDManager>();
            var second = GetNewInstanceOf<ClientIDManager>();
            Assert.That(clientIdManager, Is.EqualTo(second));
        }
    }
}
