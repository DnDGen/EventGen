using DnDGen.EventGen.IoC;
using Ninject;
using NUnit.Framework;

namespace DnDGen.EventGen.Tests.Integration
{
    [TestFixture]
    public abstract class IntegrationTests
    {
        private IKernel kernel;

        [OneTimeSetUp]
        public void IntegrationTestsFixtureSetup()
        {
            kernel = new StandardKernel(new NinjectSettings() { InjectNonPublic = true });

            var eventGenLoader = new EventGenModuleLoader();
            eventGenLoader.LoadModules(kernel);
        }

        [SetUp]
        public void IntegrationTestsSetup()
        {
            kernel.Inject(this);
        }

        protected T GetNewInstanceOf<T>()
        {
            return kernel.Get<T>();
        }
    }
}
