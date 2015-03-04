namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Graphics;
    using Android.OS;
    using Android.Provider;
    using Android.Views;
    using Android.Widget;

    class ClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener, IDialogInterfaceOnCancelListener, IMenuItemOnMenuItemClickListener
    {
        private readonly Action<IDialogInterface, int> action2;
        private readonly Action action;

        public ClickListener(Action<IDialogInterface, int> action2)
        {
            this.action2 = action2;
        }

        public ClickListener(Action action)
        {
            this.action = action;
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            if (action2 != null)
            {
                action2(dialog, which);
            }
        }

        public void OnCancel(IDialogInterface dialog)
        {
            if (action2 != null)
            {
                action2(dialog, 0);
            }
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (action != null)
            {
                action();
            }
            return true;
        }
    }
}