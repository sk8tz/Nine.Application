namespace Nine.Application.Layout
{
    using System;
    using Windows.Foundation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class LayoutPanel : Panel
    {
        public Func<LayoutScope<UIElement>, LayoutView<UIElement>> LayoutHandler;

        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
        {
            var scope = new LayoutScope<UIElement>(LayoutAdapter.Instance);
            var root = LayoutHandler?.Invoke(scope);
            if (root != null)
            {
                var size = scope.Measure(root.Value, (float)availableSize.Width, (float)availableSize.Height);

                return new Windows.Foundation.Size(size.Width, size.Height);
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
        {
            var scope = new LayoutScope<UIElement>(LayoutAdapter.Instance);
            var root = LayoutHandler?.Invoke(scope);
            if (root != null)
            {
                scope.Arrange(root.Value, 0, 0, (float)finalSize.Width, (float)finalSize.Height);
            }

            return base.ArrangeOverride(finalSize);
        }

        class LayoutAdapter : ILayoutAdapter<UIElement>
        {
            public static readonly LayoutAdapter Instance = new LayoutAdapter();

            public Size Measure(UIElement view, float width, float height)
            {
                view.Measure(new Windows.Foundation.Size(width, height));

                return new Size((float)view.DesiredSize.Width, (float)view.DesiredSize.Height);
            }

            public void Arrange(UIElement view, float x, float y, float width, float height)
            {
                view.Arrange(new Rect(x, y, width, height));
            }
        }
    }
}
