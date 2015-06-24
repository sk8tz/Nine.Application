namespace Nine.Application.WindowsStore.Test
{
    using System;
    using System.Reflection;
    using Windows.UI.Xaml.Controls;
    using Xunit;
    using Xunit.Abstractions;

    public sealed partial class MainPage : Page, IMessageSink
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            Output.Text = message.ToString();
            return true;
        }
    }
}
