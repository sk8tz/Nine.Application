using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nine.Application;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Run();
        }

        private async void Run()
        {
            var ui = new AppUI();

            //while (true)
            {
                await Task.Delay(1000); ui.Toast("hi", "toast lksjdffsdlkfjlsjf;lsfd;asj;fkadfjs;lkfjdskf==================---=-===================");
                //ui.Toast("notification title", "messagea");
                //Debug.WriteLine(await ui.Input("enter text here", "default", "ok", false, default(CancellationToken)));
                //await ui.Confirm("hi", "dialog message", "yes", "no", new CancellationTokenSource(1000).Token);
            }
        }
    }
}
