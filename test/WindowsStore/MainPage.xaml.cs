namespace Nine.Application.WindowsStore.Test
{
    using System;
    using System.Reflection;
    using Windows.UI.Xaml.Controls;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public sealed partial class MainPage : Page, IMessageSink
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += (a, b) =>
            {
                var assembly = typeof(AppUITest).GetTypeInfo().Assembly;
                var framework = new XunitTestFramework(this);
                framework.GetExecutor(assembly.GetName()).RunAll(this, null, null);
            };
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            return true;
        }
    }
}
