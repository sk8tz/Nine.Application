namespace Nine.Application.Layout
{
    using Xunit;

    public class StackLayoutTest : LayoutTest
    {
        public static TheoryData<float, float, float, LayoutView<Size>[], Rectangle[]> VerticalLayouts =
                  new TheoryData<float, float, float, LayoutView<Size>[], Rectangle[]>
        {
            { 0, 0, 0, new LayoutView<Size>[0], new Rectangle[0] },
            { 100, 2, 1,
                new LayoutView<Size>[]
                {
                    new Size(10, 5),
                    new Size(12, 8),
                },
                new []
                {
                    new Rectangle(0, 0, 10, 5),
                    new Rectangle(0, 6, 12, 8),
                }
            }
        };

        public static TheoryData<float, float, float, LayoutView<Size>[], Rectangle[]> HorizontalLayouts =
                  new TheoryData<float, float, float, LayoutView<Size>[], Rectangle[]>
        {
            { 0, 0, 0, new LayoutView<Size>[0], new Rectangle[0] },
            { 100, 2, 1,
                new LayoutView<Size>[]
                {
                    new Size(10, 5),
                    new Size(12, 8),
                },
                new []
                {
                    new Rectangle(0, 0, 10, 5),
                    new Rectangle(11, 0, 12, 8),
                }
            }
        };

        [Theory, MemberData(nameof(VerticalLayouts))]
        public void stack_v(float width, float height, float spacing, LayoutView<Size>[] views, Rectangle[] expected)
        {
            Assert.Equal(expected, Layout(width, height, scope => scope.StackVertically(spacing, views)));
        }

        [Theory, MemberData(nameof(HorizontalLayouts))]
        public void stack_h(float width, float height, float spacing, LayoutView<Size>[] views, Rectangle[] expected)
        {
            Assert.Equal(expected, Layout(width, height, scope => scope.StackHorizontally(spacing, views)));
        }

        [Fact]
        public void stack_v_h()
        {
            var expected = new[]
            {
                new Rectangle(0,0,40,40),
                new Rectangle(40,0,40,40),
                new Rectangle(80,0,40,40),
                new Rectangle(-10,40,40,10),
            };

            Assert.Equal(expected, Layout(20, 1, scope
                => scope.StackVertically(
                    scope.StackHorizontally(
                        new Size(40, 40),
                        new Size(40, 40),
                        new Size(40, 40)),
                    new LayoutView<Size>
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        View = new Size(40, 10),
                    })));
        }
    }
}
