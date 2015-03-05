using Android.App;
using Android.OS;

namespace Nine.Application.Android.Test
{
    [Activity(Label = "Nine.Application.Android.Test", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.Main);

            var test = new AppUITest();
            await test.confirm_yes(new AppUI());
        }
    }
}
