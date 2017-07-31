using DnDGen.Stress;
using NUnit.Framework;
using System.Reflection;

namespace EventGen.Tests.Integration.Stress
{
    [TestFixture]
    public abstract class StressTests : IntegrationTests
    {
        protected readonly Stressor stressor;

        public StressTests()
        {
            var runningAssembly = Assembly.GetExecutingAssembly();

#if STRESS
            var isFullStress = true;
#else
            var isFullStress = false;
#endif

            stressor = new Stressor(isFullStress, runningAssembly);
        }
    }
}
