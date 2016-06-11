namespace Nine.Application.Layout
{
    using System;
    using CoreGraphics;
    using UIKit;

    public class LayoutPanel : UIView
    {
        public Func<LayoutScope<UIView>, LayoutView<UIView>> LayoutHandler;

        public override void LayoutSubviews()
        {
            var scope = new LayoutScope<UIView>(LayoutAdapter.Instance);
            var root = LayoutHandler?.Invoke(scope);
            if (root != null)
            {
                var frame = Frame;
                scope.Arrange(root.Value, (float)frame.X, (float)frame.Y, (float)frame.Width, (float)frame.Height);
            }
        }

        class LayoutAdapter : ILayoutAdapter<UIView>
        {
            public static readonly LayoutAdapter Instance = new LayoutAdapter();

            public Size Measure(UIView view, float width, float height)
            {
                var size = view.SizeThatFits(new CGSize(width, height));

                return new Size((float)size.Width, (float)size.Height);
            }

            public void Arrange(UIView view, float x, float y, float width, float height)
            {
                view.Frame = new CGRect(x, y, width, height);
            }
        }
    }
}
