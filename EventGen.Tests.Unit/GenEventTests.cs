using NUnit.Framework;
using System;

namespace EventGen.Tests.Unit
{
    [TestFixture]
    public class GenEventTests
    {
        private GenEvent genEvent;

        [SetUp]
        public void Setup()
        {
            genEvent = new GenEvent();
        }

        [Test]
        public void GenEventInitialized()
        {
            Assert.That(genEvent.Message, Is.Empty);
            Assert.That(genEvent.Source, Is.Empty);
            Assert.That(genEvent.When, Is.EqualTo(DateTime.Now).Within(1).Seconds);
        }

        [Test]
        public void GenEventInitializedWithArguments()
        {
            genEvent = new GenEvent("source", "message of messageness");

            Assert.That(genEvent.Source, Is.EqualTo("source"));
            Assert.That(genEvent.Message, Is.EqualTo("message of messageness"));
            Assert.That(genEvent.When, Is.EqualTo(DateTime.Now).Within(1).Seconds);
        }
    }
}
