namespace Nine.Application.Layout
{
    using System;
    using System.Collections.Generic;

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
        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, params HorizontalStackLayoutView<T>[] views)
        {
            return scope.WithPanel(new HStackPanel<T>(views, 0, true));
        }

        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, IReadOnlyList<HorizontalStackLayoutView<T>> views, float spacing = 0)
        {
            return scope.WithPanel(new HStackPanel<T>(views, spacing, true));
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, params VerticalStackLayoutView<T>[] views)
        {
            return scope.WithPanel(new VStackPanel<T>(views, 0, false));
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, IReadOnlyList<VerticalStackLayoutView<T>> views, float spacing = 0)
        {
            return scope.WithPanel(new VStackPanel<T>(views, spacing, false));
        }

        class HStackPanel<T> : ILayoutPanel<T>
        {
            private readonly IReadOnlyList<HorizontalStackLayoutView<T>> _views;
            private readonly float _spacing;
            private readonly bool _horizontal;

            public HStackPanel(IReadOnlyList<HorizontalStackLayoutView<T>> views, float spacing, bool horizontal)
            {
                _views = views;
                _spacing = spacing;
                _horizontal = horizontal;
            }

            public DesiredSize Measure(float width, float height)
            {
                if (_views.Count < 0) return DesiredSize.Empty;

                throw new NotImplementedException();
            }

            public void Arrange(float x, float y, float width, float height)
            {

            }
        }

        class VStackPanel<T> : ILayoutPanel<T>
        {
            private readonly IReadOnlyList<VerticalStackLayoutView<T>> _views;
            private readonly float _spacing;
            private readonly bool _horizontal;

            public VStackPanel(IReadOnlyList<VerticalStackLayoutView<T>> views, float spacing, bool horizontal)
            {
                _views = views;
                _spacing = spacing;
                _horizontal = horizontal;
            }

            public DesiredSize Measure(float width, float height)
            {
                if (_views.Count < 0) return DesiredSize.Empty;

                throw new NotImplementedException();
            }

            public void Arrange(float x, float y, float width, float height)
            {

            }
        }
    }
}
