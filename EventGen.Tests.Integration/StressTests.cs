using Ninject;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EventGen.Tests.Integration
{
    [TestFixture]
    public abstract class StressTests : IntegrationTests
    {
        [Inject]
        public Stopwatch StressStopwatch { get; set; }

        private const int ConfidentIterations = 1000000;
        private const int TravisJobOutputTimeLimit = 60 * 10;
        private const int TravisJobBuildTimeLimit = 60 * 50 - 3 * 60; //INFO: Taking 3 minutes off to account for initial build time before running the stress tests

        private readonly int timeLimitInSeconds;

        private int iterations;

        public StressTests()
        {
            var methods = GetType().GetMethods();
            var stressTestsCount = methods.Sum(m => m.GetCustomAttributes<TestAttribute>(true).Count());
            var stressTestCasesCount = methods.Sum(m => m.GetCustomAttributes<TestCaseAttribute>().Count());
            var stressTestsTotal = stressTestsCount + stressTestCasesCount;

            var timeLimitPerTest = TravisJobBuildTimeLimit / stressTestsTotal;
            Assert.That(timeLimitPerTest, Is.AtLeast(10));
#if STRESS
            timeLimitInSeconds = Math.Min(timeLimitPerTest, TravisJobOutputTimeLimit - 10);
#else
            timeLimitInSeconds = 1;
#endif
        }

        [SetUp]
        public void StressSetup()
        {
            iterations = 0;

            var timeout = new TimeSpan(0, 0, timeLimitInSeconds);
            Console.WriteLine($"Stress timeout is {timeout}");

            StressStopwatch.Start();
        }

        [TearDown]
        public void StressTearDown()
        {
            WriteStressSummary();

            StressStopwatch.Reset();
        }

        private void WriteStressSummary()
        {
            var iterationsPerSecond = Math.Round(iterations / StressStopwatch.Elapsed.TotalSeconds, 2);
            var timePercentage = Math.Round(StressStopwatch.Elapsed.TotalSeconds / timeLimitInSeconds * 100, 2);
            var iterationPercentage = Math.Round((double)iterations / ConfidentIterations * 100, 2);
            var status = (timePercentage >= 100 || iterationPercentage >= 100) ? "PASSED" : "FAILED";

            Console.WriteLine($"Stress test complete");
            Console.WriteLine($"\tTime: {StressStopwatch.Elapsed} ({timePercentage}%)");
            Console.WriteLine($"\tIterations: {iterations} ({iterationPercentage}%)");
            Console.WriteLine($"\tIterations Per Second: {iterationsPerSecond}");
            Console.WriteLine($"\tLikely Status: {status}");
        }

        protected void Stress(Action setup, Action test, Action teardown)
        {
            do
            {
                setup();
                test();
                teardown();
            }
            while (TestShouldKeepRunning());
        }

        private bool TestShouldKeepRunning()
        {
            iterations++;
            return StressStopwatch.Elapsed.TotalSeconds < timeLimitInSeconds && iterations < ConfidentIterations;
        }
    }
}
