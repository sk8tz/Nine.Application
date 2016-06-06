namespace Nine.Application.Layout
{
    using System;
    using System.Diagnostics;

    public struct Rectangle
    {
        public float X, Y, Width, Height;
    }

    public struct DesiredSize
    {
        public float DesiredWidth, DesiredHeight, MinWidth, MinHeight;

        public DesiredSize(float width, float height)
        {
            Debug.Assert(!float.IsNaN(width));
            Debug.Assert(!float.IsNaN(height));

            DesiredWidth = MinWidth = width;
            DesiredHeight = MinHeight = height;
        }

        public DesiredSize(float width, float height, float minWidth, float minHeight)
        {
            Debug.Assert(!float.IsNaN(width));
            Debug.Assert(!float.IsNaN(height));
            Debug.Assert(!float.IsNaN(minWidth));
            Debug.Assert(!float.IsNaN(minHeight));

            DesiredWidth = width;
            DesiredHeight = height;
            MinWidth = minWidth;
            MinHeight = minHeight;
        }
    }

    public struct LayoutView<T>
    {
        public readonly T View;
        public readonly ILayoutPanel<T> Panel;

        public LayoutView(T view, ILayoutPanel<T> panel)
        {
            View = view;
            Panel = panel;
        }

        public static implicit operator LayoutView<T>(T view) => new LayoutView<T>(view, null);
    }

    public interface ILayoutPanel<T>
    {
        DesiredSize Measure(float width, float height);

        void Arrange(float x, float y, float width, float height);
    }

    public interface ILayoutAdapter<T>
    {
        DesiredSize Measure(T view, float width, float height);

        void Arrange(T view, float x, float y, float width, float height);
    }

    public struct LayoutScope<T> : IDisposable
    {
        public readonly Rectangle Bounds;
        public readonly ILayoutAdapter<T> Adapter;

        public ILayoutPanel<T> LayoutRoot;

        public LayoutScope(Rectangle bounds, ILayoutAdapter<T> adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            Adapter = adapter;
            Bounds = bounds;
            LayoutRoot = null;
        }

        public void Dispose()
        {
            if (LayoutRoot != null)
            {
                var desiredSize = LayoutRoot.Measure(Bounds.Width, Bounds.Height);

                LayoutRoot.Arrange(Bounds.X, Bounds.Y, desiredSize.DesiredWidth, desiredSize.DesiredHeight);
            }
        }
    }
}
