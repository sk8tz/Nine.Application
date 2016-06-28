namespace Nine.Application.Layout
{
    using Xunit;

    public class GridLayoutTest : LayoutTest
    {
        public static TheoryData<float, float, GridDefinition, GridLayoutView<Size>[], Rectangle[]> Cases = new TheoryData<float, float, GridDefinition, GridLayoutView<Size>[], Rectangle[]>
        {
            { 200, 100, null, new GridLayoutView<Size>[0], new Rectangle[0] },

            { 200, 100,
                new GridDefinition(
                    new [] { "100", "100" },
                    new [] { "50", "50" }),
                new [] 
                {
                    new GridLayoutView<Size> { View = new Size(10, 10) },
                    new GridLayoutView<Size> { View = new Size(10, 10), Column = 2 },
                    new GridLayoutView<Size> { View = new Size(10, 10), Row = 2 },
                    new GridLayoutView<Size> { View = new Size(10, 10), Row = 2, Column = 2 },
                },
                new []
                {
                    new Rectangle(0, 0, 10, 10),
                    new Rectangle(100, 0, 10, 10),
                    new Rectangle(0, 50, 10, 10),
                    new Rectangle(100, 50, 10, 10),
                }
            }
        };

        [Theory, MemberData(nameof(Cases))]
        public void gird(float width, float height, GridDefinition grid, GridLayoutView<Size>[] items, Rectangle[] expected)
        {
            Assert.Equal(expected, Layout(width, height, scope => scope.Grid(grid, items)));
        }
    }
}
