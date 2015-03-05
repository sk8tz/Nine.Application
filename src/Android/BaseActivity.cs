namespace Nine.Application
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Views;

    public class BaseActivity : Activity
    {
        private string defaultTitle;
        private string titleOverride;

        public string DefaultTitle
        {
            get { return defaultTitle; }
            set { if (defaultTitle != value || Title != value) { defaultTitle = value; UpdateTitle(); } }
        }

        public string TitleOverride
        {
            get { return titleOverride; }
            set { if (titleOverride != value || Title != value) { titleOverride = value; UpdateTitle(); } }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ActivityContext.Current = this;
            UpdateTitle();
            RequestWindowFeature(WindowFeatures.IndeterminateProgress);
        }

        protected override void OnStart()
        {
            ActivityContext.Current = this;
            base.OnStart();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            MediaLibrary.SetActivityResult(requestCode, resultCode, data);
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void UpdateTitle()
        {
            var title = !string.IsNullOrEmpty(titleOverride) ? titleOverride : defaultTitle;
            Title = title ?? "";
        }
    }
}
