using System;
using Microsoft.Phone.Controls;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Nine.Application.WindowsPhone.Test
{
    public partial class MainPage : PhoneApplicationPage, IMessageSink
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

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