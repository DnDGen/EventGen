using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DnDGen.EventGen.Tests.Unit
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

        [Test]
        public async Task SetClientID_Task_Awaited()
        {
            var clientID = Guid.NewGuid();
            var task = GetTaskClientIDAsync();

            clientIDManager.SetClientID(clientID);

            await Task.WhenAll(task);

            Assert.That(task.Result, Is.EqualTo(clientID));
        }

        [Test]
        public async Task SetClientID_Task_Run()
        {
            var clientID = Guid.NewGuid();
            var task = Task.Run(GetTaskClientIDAsync);

            clientIDManager.SetClientID(clientID, task);

            await Task.WhenAll(task);

            Assert.That(task.Result, Is.EqualTo(clientID));
        }

        [Test]
        public async Task SetDifferentClientIDPerTask()
        {
            var clientID1 = Guid.NewGuid();
            var clientID2 = Guid.NewGuid();
            var task1 = GetTaskClientIDAsync();
            var task2 = GetTaskClientIDAsync();

            clientIDManager.SetClientID(clientID1, task1);
            clientIDManager.SetClientID(clientID2, task2);

            await Task.WhenAll(task1, task2);

            Assert.That(task1.Result, Is.EqualTo(clientID1));
            Assert.That(task2.Result, Is.EqualTo(clientID2));
        }

        [Test]
        public async Task SetSameClientIDPerTask()
        {
            var clientID = Guid.NewGuid();
            var task1 = GetTaskClientIDAsync();
            var task2 = GetTaskClientIDAsync();

            clientIDManager.SetClientID(clientID, task1);
            clientIDManager.SetClientID(clientID, task2);

            await Task.WhenAll(task1, task2);

            Assert.That(task1.Result, Is.EqualTo(clientID));
            Assert.That(task2.Result, Is.EqualTo(clientID));
        }

        private async Task<Guid> GetTaskClientIDAsync()
        {
            await Task.Delay(100);

            return clientIDManager.GetClientID();
        }

        [Test]
        public void IfClientIdNotSetForTask_ThrowException()
        {
            Assert.That(async () => await GetTaskClientIDAsync(),
                Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("No Client ID has been set for this thread."));
        }
    }
}
