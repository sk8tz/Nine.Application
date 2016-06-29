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
        public HorizontalAlignment HorizontalAlignment;
        public VerticalAlignment VerticalAlignment;
        public LayoutFrame Frame;

        public static implicit operator LayoutView<T>(T view) => new LayoutView<T> { View = view };
    }

    public struct LayoutFrame
    {
        public Thickness Margin;

        public float MinWidth;
        public float MinHeight;
        public float MaxWidth;
        public float MaxHeight;

        public static implicit operator LayoutFrame(Thickness margin) => new LayoutFrame { Margin = margin };
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
        public static LayoutView<T> ToLayoutView<T>(this ILayoutPanel<T> panel) => new LayoutView<T>
        {
            Panel = panel,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    public struct LayoutScope<T>
    {
        private readonly ILayoutAdapter<T> _adapter;

        public LayoutScope(ILayoutAdapter<T> adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapter = adapter;
        }

        public Size Measure(ref LayoutView<T> view, float width, float height)
        {
            Size size = Size.Zero;

            if (view.Panel != null)
            {
                size = view.Panel.Measure(width, height);
            }
            else if (view.View != null)
            {
                size = _adapter.Measure(view.View, width, height);
            }

            if (view.Frame.MaxWidth > 0 && size.Width > view.Frame.MaxWidth) size.Width = view.Frame.MaxWidth;
            if (size.Width < view.Frame.MinWidth) size.Width = view.Frame.MinWidth;

            if (view.Frame.MaxHeight > 0 && size.Height > view.Frame.MaxHeight) size.Height = view.Frame.MaxHeight;
            if (size.Height < view.Frame.MinHeight) size.Height = view.Frame.MinHeight;

            size.Width += view.Frame.Margin.Left + view.Frame.Margin.Right;
            size.Height += view.Frame.Margin.Top + view.Frame.Margin.Bottom;

            return size;
        }
        
        public void Arrange(ref LayoutView<T> view, float x, float y, float width, float height)
        {
            var size = Measure(ref view, width, height);

            Arrange(ref view, x, y, width, height, size.Width, size.Height);
        }

        public void Arrange(ref LayoutView<T> view, float x, float y, float width, float height, float viewWidth, float viewHeight)
        {
            if (view.HorizontalAlignment == HorizontalAlignment.Right)
            {
                x += width - viewWidth;
            }
            else if (view.HorizontalAlignment == HorizontalAlignment.Center)
            {
                x += (width - viewWidth) / 2;
            }

            if (view.HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                width = viewWidth;
            }

            if (view.VerticalAlignment == VerticalAlignment.Bottom)
            {
                y += height - viewHeight;
            }
            else if (view.VerticalAlignment == VerticalAlignment.Center)
            {
                y += (height - viewHeight) / 2;
            }

            if (view.VerticalAlignment != VerticalAlignment.Stretch)
            {
                height = viewHeight;
            }

            if (view.Panel != null)
            {
                view.Panel.Arrange(x, y, width, height);
            }
            else if (view.View != null)
            {
                _adapter.Arrange(view.View, x, y, width, height);
            }
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
                    var size = _scope.Measure(ref _views[i], width, height);

                    if (size.Width > finalSize.Width) finalSize.Width = size.Width;
                    if (size.Height > finalSize.Height) finalSize.Height = size.Height;
                }

                return finalSize;
            }

            public void Arrange(float x, float y, float width, float height)
            {
                for (var i = 0; i < _views.Length; i++)
                {
                    _scope.Arrange(ref _views[i], x, y, width, height);
                }
            }
        }
    }
}
