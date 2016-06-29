namespace AndroidApp
{
    using Android.App;
    using Android.OS;
    using Android.Views;
    using Android.Widget;
    using Nine.Application.Layout;

    [Activity(Label = "AndroidApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var btn1 = new Button(this) { Text = "button 1" };
            var btn2 = new Button(this) { Text = "button 2 ljkjlklk", LayoutParameters = new ViewGroup.LayoutParams(300, 300) };
            var btn3 = new Button(this) { Text = "button 3" };
            
            var root = new LayoutPanel(this);

            root.AddView(btn1);
            root.AddView(btn2);
            root.AddView(btn3);

            root.LayoutHandler = scope =>
                scope.StackVertically(
                    10.0f,
                    btn1,
                    scope.StackHorizontally(
                        5.0f,
                        btn2,
                        new LayoutView<View> { View = btn3, VerticalAlignment = VerticalAlignment.Center }));

            SetContentView(root);
        }
    }
}

