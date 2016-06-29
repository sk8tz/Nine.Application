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

                    new GridLayoutView<Size> { Row = 1, RowSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { Row = 2, RowSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { Column = 1, ColumnSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { Column = 2, ColumnSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { RowSpan = 2, ColumnSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { Row = 2, Column = 2, RowSpan = 2, ColumnSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                },
                new []
                {
                    new Rectangle(40, 0, 20, 10),
                    new Rectangle(100, 0, 20, 50),
                    new Rectangle(0, 50, 20, 10),
                    new Rectangle(100, 50, 20, 10),
                    
                    new Rectangle(0, 0, 100, 100),
                    new Rectangle(0, 50, 100, 50),
                    new Rectangle(0, 0, 200, 50),
                    new Rectangle(100, 0, 100, 50),
                    new Rectangle(0, 0, 200, 100),
                    new Rectangle(100, 50, 100, 50),
                }
            }
        };

        [Theory, MemberData(nameof(Layouts))]
        public void grid(float width, float height, GridDefinition grid, GridLayoutView<Size>[] items, Rectangle[] expected)
        {
            Assert.Equal(expected, Layout(width, height, scope => scope.Grid(grid, items)));
        }
    }
}
