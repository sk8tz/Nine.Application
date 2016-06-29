namespace Nine.Application.Layout
{
    using Xunit;

    public class GridLayoutTest : LayoutTest
    {
        public static TheoryData<float, float, GridDefinition, GridLayoutView<Size>[], Rectangle[]> Layouts
                = new TheoryData<float, float, GridDefinition, GridLayoutView<Size>[], Rectangle[]>
        {
            { 0, 0, null, new GridLayoutView<Size>[0], new Rectangle[0] },

            { 200, 100,
                new GridDefinition(
                    new [] { "100", "100" },
                    new [] { "50", "50" }),
                new [] 
                {
                    new GridLayoutView<Size> { View = { View = new Size(20, 10), HorizontalAlignment = HorizontalAlignment.Center } },
                    new GridLayoutView<Size> { View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch }, Column = 2 },
                    new GridLayoutView<Size> { View = new Size(20, 10), Row = 2 },
                    new GridLayoutView<Size> { View = new Size(20, 10), Row = 2, Column = 2 },
                },
                new []
                {
                    new Rectangle(40, 0, 20, 10),
                    new Rectangle(100, 0, 20, 50),
                    new Rectangle(0, 50, 20, 10),
                    new Rectangle(100, 50, 20, 10),
                }
            }
        };

        [Theory, MemberData(nameof(Layouts))]
        public void gird(float width, float height, GridDefinition grid, GridLayoutView<Size>[] items, Rectangle[] expected)
        {
            Assert.Equal(expected, Layout(width, height, scope => scope.Grid(grid, items)));
        }
    }
}
