namespace Nine.Application.Layout
{
    using System;
    using System.Diagnostics;

    public struct Rectangle
    {
        public float X, Y, Width, Height;

        public override string ToString() => $"({X},{Y}) {Width}x{Height}";
    }

    public struct Thickness
    {
        public static readonly Thickness Zero = new Thickness();

        public float Left, Right, Top, Bottom;

        public static implicit operator Thickness(float value)
            => new Thickness { Left = value, Right = value, Top = value, Bottom = value };

        public override string ToString() => $"L:{Left} T:{Top} R:{Right} B:{Bottom}";
    }

    public struct DesiredSize
    {
        public static readonly DesiredSize Empty;

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

    public enum HorizontalAlignment { Left, Center, Right, Stretch }

    public enum VerticalAlignment { Top, Center, Bottom, Stretch }

    public struct LayoutView<T>
    {
        public T View;
        public ILayoutPanel<T> Panel;
        public Thickness Margin;

        public static implicit operator LayoutView<T>(T view) => new LayoutView<T> { View = view };
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

        public LayoutView<T> WithPanel(ILayoutPanel<T> panel)
        {
            Debug.Assert(panel != null);

            return new LayoutView<T> { Panel = panel };
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
