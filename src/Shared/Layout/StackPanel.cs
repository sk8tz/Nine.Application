namespace Nine.Application.Layout
{
    using System;
    using System.Collections.Generic;

    public static class StackLayout
    {
        class StackPanel<T> : ILayoutPanel<T>
        {
            public DesiredSize Measure(float width, float height)
            {
                throw new NotImplementedException();
            }

            public void Arrange(float x, float y, float width, float height)
            {

            }
        }

        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, params LayoutView<T>[] views)
        {
            throw new NotImplementedException();
        }

        public static LayoutView<T> StackHorizontally<T>(this LayoutScope<T> scope, IReadOnlyList<LayoutView<T>> views, float spacing = 0)
        {
            throw new NotImplementedException();
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, params LayoutView<T>[] views)
        {
            throw new NotImplementedException();
        }

        public static LayoutView<T> StackVertically<T>(this LayoutScope<T> scope, IReadOnlyList<LayoutView<T>> views, float spacing = 0)
        {
            throw new NotImplementedException();
        }
    }
}
