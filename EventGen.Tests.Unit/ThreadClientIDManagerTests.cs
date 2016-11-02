using NUnit.Framework;
using System;
using System.Threading;

namespace EventGen.Tests.Unit
{
    [TestFixture]
    public class ThreadClientIDManagerTests
    {
        private ClientIDManager clientIDManager;
        private Guid firstClientID;
        private Guid secondClientID;
        private Guid firstManagedClientID;
        private Guid secondManagedClientID;

        [SetUp]
        public void Setup()
        {
            clientIDManager = new ThreadClientIDManager();
        }

        [Test]
        public void SetClientID()
        {
            var clientID = Guid.NewGuid();
            clientIDManager.SetClientID(clientID);

            var managedClientID = clientIDManager.GetClientID();
            Assert.That(managedClientID, Is.EqualTo(clientID));
        }

        [Test]
        public void SetClientIDPerThread()
        {
            var first = new Thread(SetFirstClientID);
            var second = new Thread(SetSecondClientID);

            first.Start();
            second.Start();
            Thread.Sleep(1);

            Assert.That(firstClientID, Is.Not.Null);
            Assert.That(secondClientID, Is.Not.Null);
            Assert.That(firstManagedClientID, Is.Not.Null);
            Assert.That(secondManagedClientID, Is.Not.Null);
            Assert.That(firstClientID, Is.Not.EqualTo(secondClientID));
            Assert.That(firstManagedClientID, Is.Not.EqualTo(secondManagedClientID));
            Assert.That(firstClientID, Is.EqualTo(firstManagedClientID));
            Assert.That(secondClientID, Is.EqualTo(secondManagedClientID));
        }

        private void SetFirstClientID()
        {
            firstClientID = Guid.NewGuid();
            clientIDManager.SetClientID(firstClientID);
            firstManagedClientID = clientIDManager.GetClientID();
        }

        private void SetSecondClientID()
        {
            secondClientID = Guid.NewGuid();
            clientIDManager.SetClientID(secondClientID);
            secondManagedClientID = clientIDManager.GetClientID();
        }

        [Test]
        public void IfClientIdNotSetForThread_ThrowException()
        {
            Assert.That(() => clientIDManager.GetClientID(), Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("No Client ID has been set for this thread."));
        }
    }
}
