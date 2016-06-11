namespace Nine.Application.Layout
{
    using System.Linq;
    using Xunit;

    public class BasicLayoutTest : LayoutTest
    {
        [Theory]
        [InlineData(0, 0, "", "")]
        [InlineData(100, 50, "10,5;1,2", "0,0,1,2;0,0,10,5")]
        public void with_views(float width, float height, string sizes, string expected)
        {
            Assert.Equal(expected, LayoutAsString(width, height, scope
                => scope.WithViews(ParseSizes(sizes).Select(s => (LayoutView<Size>)s).ToArray())));
        }
    }
}
