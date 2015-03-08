namespace Nine.Application.Windows.Test
{
    using System.Windows;

    public partial class MainWindow : Window
    {
        public MainWindow()
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
