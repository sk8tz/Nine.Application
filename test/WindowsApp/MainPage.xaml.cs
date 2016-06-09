namespace WindowsApp
{
    using Windows.UI.Xaml.Controls;
    using Nine.Application.Layout;
    using Windows.UI.Xaml;

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            var btn1 = new Button { Content = "button 1" };
            var btn2 = new Button { Content = "button 2 ljkjlklk", Height = 100 };
            var btn3 = new Button { Content = "button 3", VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch };

            var root = new LayoutPanel();

            root.Children.Add(btn1);
            root.Children.Add(btn2);
            root.Children.Add(btn3);

            root.LayoutHandler = scope =>
                scope.StackVertically(
                    10.0f,
                    btn1,
                    scope.StackHorizontally(
                        5.0f,
                        btn2,
                        new HorizontalStackLayoutView<UIElement> { View = btn3, Alignment = Nine.Application.Layout.VerticalAlignment.Stretch }));

            Content = root;
        }
    }
}
