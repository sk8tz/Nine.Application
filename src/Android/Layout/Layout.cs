namespace Nine.Application.Layout
{
    using System;
    using Android.Content;
    using Android.Views;

    public class LayoutPanel : ViewGroup
    {
        public Func<LayoutScope<View>, LayoutView<View>> LayoutHandler;

        public LayoutPanel(Context context) : base(context) { }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var scope = new LayoutScope<View>(LayoutAdapter.Instance);
            var root = LayoutHandler?.Invoke(scope);
            if (root != null)
            {
                scope.Arrange(root.Value, l, t, r - l, b - t);
            }
        }

        class LayoutAdapter : ILayoutAdapter<View>
        {
            public static readonly LayoutAdapter Instance = new LayoutAdapter();

            public Size Measure(View view, float width, float height)
            {
                view.Measure((int)width, (int)height);

                var measuredWidth = view.MeasuredWidth;
                var measuredHeight = view.MeasuredHeight;

                if (view.LayoutParameters != null)
                {
                    if (view.LayoutParameters.Width > measuredWidth) measuredWidth = view.LayoutParameters.Width;
                    if (view.LayoutParameters.Height > measuredHeight) measuredHeight = view.LayoutParameters.Height;
                }

                return new Size(measuredWidth, measuredHeight);
            }

            public void Arrange(View view, float x, float y, float width, float height)
            {
                view.Layout((int)x, (int)y, (int)(x + width), (int)(y + height));
            }
        }
    }
}
