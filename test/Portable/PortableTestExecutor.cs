namespace Xunit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Xunit.Sdk;
    using Xunit.Abstractions;
    using System.Threading;


    /// <summary>
    /// Xunit test executor takes an assembly name. 
    /// On platforms like windows phone and windows RT, it is not possible to dynamically load an assembly using Assembly.Load.
    /// This executor loads text cases directly from loaded assemblies.
    /// </summary>
    class PortableTestExecutor : ISourceInformationProvider, ITestFrameworkDiscoveryOptions, ITestFrameworkExecutionOptions
    {
        public async Task RunAll(IMessageSink sink, params Assembly[] testAssemblies)
        {
            sink = new PortableMessageSink(sink);
            var discoverySink = new TestDiscoveryVisitor();
            foreach (var assembly in testAssemblies)
            {
                var assemblyInfo = Reflector.Wrap(assembly);
                using (var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, this, sink, null))
                {
                    discoverer.Find(false, discoverySink, this);
                    discoverySink.Finished.WaitOne();
                    await RunTestCases(discoverySink.TestCases.Cast<IXunitTestCase>(), assemblyInfo, sink);
                }
            }
        }

        public async Task RunTestCases(IEnumerable<IXunitTestCase> testCases, IAssemblyInfo assemblyInfo, IMessageSink sink = null)
        {
            using (var runner = new PortableTestAssemblyRunner(new TestAssembly(assemblyInfo), testCases, sink, sink, this))
            {
                await runner.RunAsync();
            }
        }

        public void Dispose() { }

        public ISourceInformation GetSourceInformation(ITestCase testCase) => null;

        public TValue GetValue<TValue>(string name) => default(TValue);
        public void SetValue<TValue>(string name, TValue value) { }

        class TestDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
        {
            public TestDiscoveryVisitor()
            {
                TestCases = new List<ITestCase>();
            }

            public List<ITestCase> TestCases { get; private set; }

            protected override bool Visit(ITestCaseDiscoveryMessage discovery)
            {
                TestCases.Add(discovery.TestCase);
                return true;
            }
        }

        class PortableTestAssemblyRunner : XunitTestAssemblyRunner
        {
            public PortableTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
                : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
            { }

            protected override void SetupSyncContext(int maxParallelThreads)
            {
                // Use existing sync context
            }
        }

        class PortableMessageSink : IMessageSink
        {
            private IMessageSink sink;
            private SynchronizationContext syncContext = SynchronizationContext.Current;

            public PortableMessageSink(IMessageSink sink) { this.sink = sink; }

            public bool OnMessage(IMessageSinkMessage message)
            {
                if (syncContext != null && sink != null)
                {
                    syncContext.Post(state => sink.OnMessage((IMessageSinkMessage)state), message);
                }
                return true;
            }
        }
    }
}