using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Foundation;
using UIKit;

namespace Nine.Application.iOS.Test
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);

            // If you have defined a view, add it here:
            // window.RootViewController  = navigationController;

			window.BackgroundColor = UIColor.White;

            // make the window visible
            window.MakeKeyAndVisible();

			Test();

            return true;
        }

		private async void Test()
		{
			var ui = new AppUI();

            ui.Toast("title", "message this is a really long message, it should not show the complete message, but there must be some ... at the end of the toeast");
            ui.Toast(null, "message1");
            ui.Toast("title1", null);
            //ui.Notify("title", "message1", new CancellationTokenSource(1000).Token);
            //ui.Notify("title", "message2", new CancellationTokenSource(1000).Token);
            //ui.Notify("title", "message3", new CancellationTokenSource(1000).Token);

            if (false)
            {
                var media = new MediaLibrary();

                media.BeginCaptureAudio();
                await Task.Delay(5000);
                using (var audio = media.EndCaptureAudio())
                using (var o = File.Create("a.wav"))
                {
                    audio.CopyTo(o);
                }

                media.PlaySound("a.wav");
                await Task.Delay(1000);
                media.StopSound();


                using (var img = await media.PickImage(ImageLocation.Library, 32))
                {
                    await ui.Confirm(null,
                        await media.SaveImageToLibrary(img, "filename") ?? "null",
                        "yes", "no", default(CancellationToken));
                }
            }
		}
    }
}