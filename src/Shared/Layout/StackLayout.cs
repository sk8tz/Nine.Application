namespace Nine.Application.Layout
{
    using System.Diagnostics;

    public static class StackLayout
    {
        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, float spacing, params LayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0];

            return new HStackPanel<T>(scope, views, spacing).ToLayoutView();
        }

        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, params LayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0];

            return new HStackPanel<T>(scope, views, 0).ToLayoutView();
        }

        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, LayoutView<T>[] views, float spacing = 0)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0];

            return new HStackPanel<T>(scope, views, spacing).ToLayoutView();
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, float spacing, params LayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0];

            return new VStackPanel<T>(scope, views, spacing).ToLayoutView();
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, params LayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0];

            return new VStackPanel<T>(scope, views, 0).ToLayoutView();
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, LayoutView<T>[] views, float spacing = 0)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0];

            return new VStackPanel<T>(scope, views, spacing).ToLayoutView();
        }

        class HStackPanel<T> : ILayoutPanel<T>
        {
            private readonly LayoutScope<T> _scope;
            private readonly LayoutView<T>[] _views;
            private readonly float _spacing;

            public HStackPanel(LayoutScope<T> scope, LayoutView<T>[] views, float spacing)
            {
                Debug.Assert(views.Length > 1);

                _scope = scope;
                _views = views;
                _spacing = spacing;
            }

            public Size Measure(float width, float height)
            {
                var finalSize = Size.Zero;

                for (var i = 0; i < _views.Length; i++)
                {
                    var size = _scope.Measure(ref _views[i], width, height);

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

                for (var i = 0; i < _views.Length; i++)
                {
                    var size = _scope.Measure(ref _views[i], width, height);

                    _scope.Arrange(ref _views[i], offset, y, size.Width, height, size.Width, size.Height);

                    offset += size.Width;
                    offset += _spacing;
                }
            }
        }

        class VStackPanel<T> : ILayoutPanel<T>
        {
            private readonly LayoutScope<T> _scope;
            private readonly LayoutView<T>[] _views;
            private readonly float _spacing;

            public VStackPanel(LayoutScope<T> scope, LayoutView<T>[] views, float spacing)
            {
                Debug.Assert(views.Length > 1);

                _scope = scope;
                _views = views;
                _spacing = spacing;
            }

            public Size Measure(float width, float height)
            {
                var finalSize = Size.Zero;

                for (var i = 0; i < _views.Length; i++)
                {
                    var size = _scope.Measure(ref _views[i], width, height);

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

                for (var i = 0; i < _views.Length; i++)
                {
                    var size = _scope.Measure(ref _views[i], width, height);

                    _scope.Arrange(ref _views[i], x, offset, width, size.Height, size.Width, size.Height);

                    offset += size.Height;
                    offset += _spacing;
                }
            }
        }
    }
}
