namespace Nine.Application.Layout
{
    using Windows.Foundation;
    using Windows.UI.Xaml;

    public static class WindowsLayoutExtensions
    {
        class LayoutAdapter : ILayoutAdapter<UIElement>
        {
            public DesiredSize Measure(UIElement view, float width, float height)
            {
                view.Measure(new Size(width, height));

                return new DesiredSize((float)view.DesiredSize.Width, (float)view.DesiredSize.Height);
            }

            public void Arrange(UIElement view, float x, float y, float width, float height)
            {
                view.Arrange(new Rect(x, y, width, height));
            }
        }

        private static readonly LayoutAdapter s_layoutAdapter = new LayoutAdapter();

        public static LayoutScope<UIElement> BeginLayout(this FrameworkElement container)
        {
            var bounds = new Rectangle { X = 0, Y = 0, Width = (float)container.ActualWidth, Height = (float)container.ActualHeight };

            return new LayoutScope<UIElement>(bounds, s_layoutAdapter);
        }
    }
}
