namespace Nine.Application.Layout
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public struct HorizontalStackLayoutView<T>
    {
        public LayoutView<T> View;
        public VerticalAlignment Alignment;

        public static implicit operator HorizontalStackLayoutView<T>(T view) => new HorizontalStackLayoutView<T> { View = view };
        public static implicit operator HorizontalStackLayoutView<T>(LayoutView<T> view) => new HorizontalStackLayoutView<T> { View = view };
    }

    public struct VerticalStackLayoutView<T>
    {
        public LayoutView<T> View;
        public HorizontalAlignment Alignment;

        public static implicit operator VerticalStackLayoutView<T>(T view) => new VerticalStackLayoutView<T> { View = view };
        public static implicit operator VerticalStackLayoutView<T>(LayoutView<T> view) => new VerticalStackLayoutView<T> { View = view };
    }

    public static class StackLayout
    {
        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, float spacing, params HorizontalStackLayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0].View;

            return new HStackPanel<T>(scope, views, spacing).ToLayoutView();
        }

        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, params HorizontalStackLayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0].View;

            return new HStackPanel<T>(scope, views, 0).ToLayoutView();
        }

        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, IReadOnlyList<HorizontalStackLayoutView<T>> views, float spacing = 0)
        {
            if (views.Count == 0) return LayoutView<T>.None;
            if (views.Count == 1) return views[0].View;

            return new HStackPanel<T>(scope, views, spacing).ToLayoutView();
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, float spacing, params VerticalStackLayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0].View;

            return new VStackPanel<T>(scope, views, spacing).ToLayoutView();
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, params VerticalStackLayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0].View;

            return new VStackPanel<T>(scope, views, 0).ToLayoutView();
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, IReadOnlyList<VerticalStackLayoutView<T>> views, float spacing = 0)
        {
            if (views.Count == 0) return LayoutView<T>.None;
            if (views.Count == 1) return views[0].View;

            return new VStackPanel<T>(scope, views, spacing).ToLayoutView();
        }

        class HStackPanel<T> : ILayoutPanel<T>
        {
            private readonly LayoutScope<T> _scope;
            private readonly IReadOnlyList<HorizontalStackLayoutView<T>> _views;
            private readonly float _spacing;

            public HStackPanel(LayoutScope<T> scope, IReadOnlyList<HorizontalStackLayoutView<T>> views, float spacing)
            {
                Debug.Assert(views.Count > 1);

                _scope = scope;
                _views = views;
                _spacing = spacing;
            }

            public Size Measure(float width, float height)
            {
                var finalSize = Size.Zero;

                for (var i = 0; i < _views.Count; i++)
                {
                    var size = _scope.Measure(_views[i].View, width, height);

                    if (size.Height > finalSize.Height) finalSize.Height = size.Height;

                    finalSize.Width += _spacing;
                    finalSize.Width += size.Width;
                }

                finalSize.Width -= _spacing;

                return finalSize;
            }

            public void Arrange(float x, float y, float width, float height)
            {
                var offset = x;

                for (var i = 0; i < _views.Count; i++)
                {
                    var view = _views[i];
                    var size = _scope.Measure(view.View, width, height);

                    if (view.Alignment == VerticalAlignment.Top)
                    {
                        _scope.Arrange(view.View, offset, y, size.Width, size.Height);
                    }
                    else if (view.Alignment == VerticalAlignment.Stretch)
                    {
                        _scope.Arrange(view.View, offset, y, size.Width, height);
                    }
                    else if (view.Alignment == VerticalAlignment.Center)
                    {
                        _scope.Arrange(view.View, offset, y + (height - size.Height) * 0.5f, size.Width, size.Height);
                    }
                    else if (view.Alignment == VerticalAlignment.Bottom)
                    {
                        _scope.Arrange(view.View, offset, y + height - size.Height, size.Width, size.Height);
                    }

                    offset += size.Width;
                    offset += _spacing;
                }
            }
        }

        class VStackPanel<T> : ILayoutPanel<T>
        {
            private readonly LayoutScope<T> _scope;
            private readonly IReadOnlyList<VerticalStackLayoutView<T>> _views;
            private readonly float _spacing;

            public VStackPanel(LayoutScope<T> scope, IReadOnlyList<VerticalStackLayoutView<T>> views, float spacing)
            {
                Debug.Assert(views.Count > 1);

                _scope = scope;
                _views = views;
                _spacing = spacing;
            }

            public Size Measure(float width, float height)
            {
                var finalSize = Size.Zero;

                for (var i = 0; i < _views.Count; i++)
                {
                    var size = _scope.Measure(_views[i].View, width, height);

                    if (size.Width > finalSize.Width) finalSize.Width = size.Width;

                    finalSize.Height += _spacing;
                    finalSize.Height += size.Width;
                }

                finalSize.Height -= _spacing;

                return finalSize;
            }

            public void Arrange(float x, float y, float width, float height)
            {
                var offset = y;

                for (var i = 0; i < _views.Count; i++)
                {
                    var view = _views[i];
                    var size = _scope.Measure(view.View, width, height);

                    if (view.Alignment == HorizontalAlignment.Left)
                    {
                        _scope.Arrange(view.View, x, offset, size.Width, size.Height);
                    }
                    else if (view.Alignment == HorizontalAlignment.Stretch)
                    {
                        _scope.Arrange(view.View, x, offset, width, size.Height);
                    }
                    else if (view.Alignment == HorizontalAlignment.Center)
                    {
                        _scope.Arrange(view.View, x + (width - size.Width) * 0.5f, offset, size.Width, size.Height);
                    }
                    else if (view.Alignment == HorizontalAlignment.Right)
                    {
                        _scope.Arrange(view.View, x + width - size.Width, offset, size.Width, size.Height);
                    }

                    offset += size.Height;
                    offset += _spacing;
                }
            }
        }
    }
}
