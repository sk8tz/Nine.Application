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
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            Title = message.ToString();
            return true;
        }
    }
}