namespace Nine.Application.WindowsStore.Test
{
    using Windows.UI.Xaml.Controls;

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += async (a, b) =>
            {
                var test = new AppUITest();
                await test.confirm(new AppUI());
            };
        }
    }
}
