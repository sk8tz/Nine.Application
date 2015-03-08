using Android.App;
using Android.OS;
using Xunit;
using Xunit.Sdk;
using Xunit.Abstractions;

namespace Nine.Application.Android.Test
{
    [Activity(Label = "Nine.Application.Android.Test", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IMessageSink
    {
        protected override void OnCreate(Bundle bundle)
        {
            ContextProvider.Current = () => this;

            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            
            var test = new XunitTestFramework(this);
            var executor = test.GetExecutor(typeof(AppUITest).Assembly.GetName());
            executor.RunAll(this, TestFrameworkOptions.ForDiscovery(), TestFrameworkOptions.ForExecution());
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            Title = message.ToString();
            return true;
        }
    }
}
