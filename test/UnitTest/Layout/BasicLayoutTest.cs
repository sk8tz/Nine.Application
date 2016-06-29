namespace Nine.Application.Layout
{
    using Xunit;

    public class BasicLayoutTest : LayoutTest
    {
        public static TheoryData<float, float, LayoutView<Size>[], Rectangle[]> Layouts
                = new TheoryData<float, float, LayoutView<Size>[], Rectangle[]>
        {
            { 0, 0, new LayoutView<Size>[0], new Rectangle[0] },

            {
                100, 50,
                new LayoutView<Size>[]
                {
                    new Size(10, 5),
                    new Size(1, 2),

                    new LayoutView<Size>{ View = new Size(10, 5), HorizontalAlignment = HorizontalAlignment.Right },
                    new LayoutView<Size>{ View = new Size(10, 5), HorizontalAlignment = HorizontalAlignment.Center },
                    new LayoutView<Size>{ View = new Size(10, 5), HorizontalAlignment = HorizontalAlignment.Stretch },

                    new LayoutView<Size>{ View = new Size(5, 10), VerticalAlignment = VerticalAlignment.Bottom },
                    new LayoutView<Size>{ View = new Size(5, 10), VerticalAlignment = VerticalAlignment.Center },
                    new LayoutView<Size>{ View = new Size(5, 10), VerticalAlignment = VerticalAlignment.Stretch },
                },
                new []
                {
                    new Rectangle(0, 0, 10, 5),
                    new Rectangle(0, 0, 1, 2),

                    new Rectangle(90, 0, 10, 5),
                    new Rectangle(45, 0, 10, 5),
                    new Rectangle(0, 0, 100, 5),

                    new Rectangle(0, 40, 5, 10),
                    new Rectangle(0, 20, 5, 10),
                    new Rectangle(0, 0, 5, 50),
                }
            }
        };

        [Theory, MemberData(nameof(Layouts))]
        public void with_views(float width, float height, LayoutView<Size>[] views, Rectangle[] expected)
        {
            Assert.Equal(expected, Layout(width, height, scope => scope.WithViews(views)));
        }
    }
}
