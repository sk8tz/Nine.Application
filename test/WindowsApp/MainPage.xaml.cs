namespace WindowsApp
{
    using System;
    using Windows.Foundation;
    using Windows.UI.Xaml.Controls;
    using Nine.Application.Layout;

    public sealed partial class MainPage : Page
    {
        Button _btn1 = new Button { Content = "button 1" };
        Button _btn2 = new Button { Content = "button 2" };
        Button _btn3 = new Button { Content = "button 3" };

        public MainPage()
        {
            InitializeComponent();

            _root.Children.Add(_btn1);
            _root.Children.Add(_btn2);
            _root.Children.Add(_btn3);

            SizeChanged += (a, b) => Layout();
        }

        private void Layout()
        {
            using (var scope = _root.BeginLayout())
            {
                scope.StackVertically(_btn1,
                     scope.StackHorizontally(_btn2, _btn3));
            }
        }
    }
}
