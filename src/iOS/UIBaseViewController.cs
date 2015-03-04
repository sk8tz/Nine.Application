namespace Nine.Application
{
    using System;
    using UIKit;

    public class UIBaseViewController : UIViewController
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

        public UIBaseViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            UpdateTitle();
            base.ViewDidLoad();
        }

        private void UpdateTitle()
        {
            var title = !string.IsNullOrEmpty(titleOverride) ? titleOverride : defaultTitle;
            Title = title ?? "";
        }
    }
}
