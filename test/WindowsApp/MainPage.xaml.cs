namespace WindowsApp
{
    using Windows.UI.Xaml.Controls;
    using Nine.Application.Layout;
    using Windows.UI.Xaml;

    public sealed partial class MainPage : Page
    {
        Button _btn1 = new Button { Content = "button 1" };
        Button _btn2 = new Button { Content = "button 2 ljkjlklk", Height = 100 };
        Button _btn3 = new Button { Content = "button 3", VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch };

        public MainPage()
        {
            InitializeComponent();

            var root = new LayoutPanel();

            root.Children.Add(_btn1);
            root.Children.Add(_btn2);
            root.Children.Add(_btn3);

            root.Layout += scope => 
                scope.StackVertically(
                    10.0f,
                    _btn1,
                    scope.StackHorizontally(
                        5.0f,
                        _btn2,
                        new HorizontalStackLayoutView<UIElement> { View = _btn3, Alignment = Nine.Application.Layout.VerticalAlignment.Stretch }));

            Content = root;
        }
    }
}
