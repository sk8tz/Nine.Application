namespace iOSApp
{
    using Foundation;
    using UIKit;
    using Nine.Application.Layout;

    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            Window.RootViewController = new UIViewController();

            var btn1 = new UIButton();
            btn1.SetTitle("button 1", UIControlState.Normal);

            var btn2 = new UIButton();
            btn2.SetTitle("button 2 ljkjlklk", UIControlState.Normal);

            var btn3 = new UIButton();
            btn3.SetTitle("button 3", UIControlState.Normal);

            var root = new LayoutPanel();
            root.AddSubviews(btn1, btn2, btn3);

            root.LayoutHandler = scope =>
                scope.StackVertically(
                    10.0f,
                    btn1,
                    scope.StackHorizontally(
                        5.0f,
                        btn2,
                        new HorizontalStackLayoutView<UIView> { View = btn3, Alignment = VerticalAlignment.Stretch }));

            Window.MakeKeyAndVisible();

            return true;
        }
    }
}


