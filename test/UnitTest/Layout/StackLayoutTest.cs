namespace Nine.Application.Layout
{
    using System.Linq;
    using Xunit;

    public class StackLayoutTest : LayoutTest
    {
        [Theory]
        [InlineData(0, 0, 0, "", "")]
        [InlineData(100, 2, 1, "10,5;12,8", "0,0,10,5;0,6,12,8")]
        public void stack_v(float width, float height, float spacing, string sizes, string expected)
        {
            Assert.Equal(expected, LayoutAsString(width, height, scope
                => scope.StackVertically(spacing, ParseSizes(sizes).Select(s => (VerticalStackLayoutView<Size>)s).ToArray())));
        }

        [Theory]
        [InlineData(0, 0, 0, "", "")]
        [InlineData(100, 2, 1, "10,5;12,8", "0,0,10,5;11,0,12,8")]
        public void stack_h(float width, float height, float spacing, string sizes, string expected)
        {
            Assert.Equal(expected, LayoutAsString(width, height, scope
                => scope.StackHorizontally(spacing, ParseSizes(sizes).Select(s => (HorizontalStackLayoutView<Size>)s).ToArray())));
        }

        [Fact]
        public void stack_v_h()
        {
            var expected = "0,0,40,40;40,0,40,40;80,0,40,40;-10,40,40,10";

            Assert.Equal(expected, LayoutAsString(20, 1, scope
                => scope.StackVertically(
                    scope.StackHorizontally(
                        new Size(40, 40),
                        new Size(40, 40),
                        new Size(40, 40)),
                    new VerticalStackLayoutView<Size>
                    {
                        Alignment = HorizontalAlignment.Center,
                        View = new Size(40, 10),
                    })));
        }
    }
}
