namespace Nine.Application.Layout
{
    using System;

    public struct Size
    {
        public static readonly Size Zero;

        public float Width, Height;

        public Size(float width, float height) { Width = width;  Height = height; }

        public override string ToString() => $"{Width}x{Height}";
    }

    public struct Rectangle
    {
        public static readonly Rectangle Zero = new Rectangle();

        public float X, Y, Width, Height;

        public Rectangle(float x, float y, float width, float height) { X = x; Y = y; Width = width; Height = height; }

        public override string ToString() => $"({X},{Y}) {Width}x{Height}";
    }

    public struct Thickness
    {
        public static readonly Thickness Zero = new Thickness();

        public float Left, Right, Top, Bottom;

        public Thickness(float value) { Left = value; Right = value; Top = value; Bottom = value; }
        public Thickness(float horizontal, float vertical) { Left = Right = horizontal; Top = Bottom = vertical; }
        public Thickness(float left, float top, float right, float bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }

        public static implicit operator Thickness(float value) => new Thickness(value);

        public override string ToString() => $"L:{Left} T:{Top} R:{Right} B:{Bottom}";
    }

    public enum HorizontalAlignment { Left, Center, Right, Stretch }

    public enum VerticalAlignment { Top, Center, Bottom, Stretch }

    public struct LayoutView<T>
    {
        public static readonly LayoutView<T> None = default(LayoutView<T>);

        public T View;
        public ILayoutPanel<T> Panel;

        public static implicit operator LayoutView<T>(T view) => new LayoutView<T> { View = view };
    }

    public interface ILayoutPanel<T>
    {
        Size Measure(float width, float height);

        void Arrange(float x, float y, float width, float height);
    }

    public interface ILayoutAdapter<T>
    {
        Size Measure(T view, float width, float height);

        void Arrange(T view, float x, float y, float width, float height);
    }

    public static class LayoutExtensions
    {
        public static LayoutView<T> ToLayoutView<T>(this ILayoutPanel<T> panel) => new LayoutView<T> { Panel = panel };
    }

    public struct LayoutScope<T>
    {
        private readonly ILayoutAdapter<T> _adapter;

        public LayoutScope(ILayoutAdapter<T> adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapter = adapter;
        }

        public Size Measure(LayoutView<T> view, float width, float height)
        {
            if (view.Panel != null) return view.Panel.Measure(width, height);
            if (view.View != null) return _adapter.Measure(view.View, width, height);

            return Size.Zero;
        }

        public void Arrange(LayoutView<T> view, float x, float y, float width, float height)
        {
            if (view.Panel != null) view.Panel.Arrange(x, y, width, height);
            else if (view.View != null) _adapter.Arrange(view.View, x, y, width, height);
        }
    }

    public static class BasicLayout
    {
        public static LayoutView<T> WithViews<T>(this LayoutScope<T> scope, params LayoutView<T>[] views)
        {
            return new BasicPanel<T>(scope, views).ToLayoutView();
        }

        class BasicPanel<T> : ILayoutPanel<T>
        {
            private readonly LayoutScope<T> _scope;
            private readonly LayoutView<T>[] _views;

            public BasicPanel(LayoutScope<T> scope, LayoutView<T>[] views)
            {
                _scope = scope;
                _views = views;
            }

            public Size Measure(float width, float height)
            {
                var finalSize = Size.Zero;

                for (var i = 0; i < _views.Length; i++)
                {
                    var size = _scope.Measure(_views[i], width, height);

                    if (size.Width > finalSize.Width) finalSize.Width = size.Width;
                    if (size.Height > finalSize.Height) finalSize.Height = size.Height;
                }

                return finalSize;
            }

            public void Arrange(float x, float y, float width, float height)
            {
                for (var i = 0; i < _views.Length; i++)
                {
                    _scope.Arrange(_views[i], x, y, width, height);
                }
            }
        }
    }
}
