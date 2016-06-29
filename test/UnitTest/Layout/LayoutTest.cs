namespace Nine.Application.Layout
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class LayoutTest
    {
        public static Rectangle[] Layout(float width, float height, Func<LayoutScope<Size>, LayoutView<Size>> layoutHandler)
        {
            var adapter = new TestLayoutAdapter();

            var scope = new LayoutScope<Size>(adapter);

            var root = layoutHandler(scope);

            if ((root.View.Width != 0 && root.View.Height != 0) || root.Panel != null)
            {
                scope.Arrange(ref root, 0, 0, width, height);
            }

            return adapter.ArrangedRectangles.ToArray();
        }

        public static IEnumerable<Size> ParseSizes(string sizes)
        {
            return from size in sizes.Split(';')
                   where !string.IsNullOrEmpty(size)
                   let wh = size.Split(',')
                   select new Size(float.Parse(wh[0]), float.Parse(wh[1]));
        }

        class TestLayoutAdapter : ILayoutAdapter<Size>
        {
            public readonly List<Rectangle> ArrangedRectangles = new List<Rectangle>();

            public void Arrange(Size view, float x, float y, float width, float height)
            {
                ArrangedRectangles.Add(new Rectangle(x, y, width, height));
            }

            public Size Measure(Size view, float width, float height)
            {
                return new Size(view.Width, view.Height);
            }
        }
    }
}
