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
                    new GridLayoutView<Size> { View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch }, Column = 1 },
                    new GridLayoutView<Size> { View = new Size(20, 10), Row = 1 },
                    new GridLayoutView<Size> { View = new Size(20, 10), Row = 1, Column = 1 },

                    new GridLayoutView<Size> { Row = 0, RowSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { Row = 1, RowSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { Column = 0, ColumnSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { Column = 1, ColumnSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { RowSpan = 2, ColumnSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
                    new GridLayoutView<Size> { Row = 1, Column = 1, RowSpan = 2, ColumnSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch } },
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
            },

            { 200, 100,
                new GridDefinition(
                    new [] { "100", "Auto", "Auto" },
                    new [] { "Auto", "Auto", "50" }),
                new []
                {
                    new GridLayoutView<Size> { View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch } },
                    new GridLayoutView<Size> { View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch }, Row = 2 },

                    new GridLayoutView<Size> { View = { View = new Size(20, 10) }, Column = 1 },
                    new GridLayoutView<Size> { View = { View = new Size(140, 10) }, ColumnSpan = 2 },
                    new GridLayoutView<Size> { View = { View = new Size(100, 10) }, Column = 1, ColumnSpan = 2 },
                    new GridLayoutView<Size> { View = { View = new Size(20, 10), HorizontalAlignment = HorizontalAlignment.Stretch }, Column = 2 },
                },
                new []
                {
                    new Rectangle(0, 0, 20, 10),
                    new Rectangle(0, 10, 20, 50),

                    new Rectangle(100, 0, 20, 10),
                    new Rectangle(0, 0, 140, 10),
                    new Rectangle(100, 0, 100, 10),
                    new Rectangle(140, 0, 60, 10),
                }
            },

            { 200, 100,
                new GridDefinition(
                    new [] { "20", "Auto", "20" },
                    new [] { "20", "Auto", "Auto"}),
                new []
                {
                    new GridLayoutView<Size> { View = new Size(100, 10), ColumnSpan = 3 },
                    new GridLayoutView<Size> { View = { View = new Size(20, 10), HorizontalAlignment = HorizontalAlignment.Stretch }, Column = 1 },

                    new GridLayoutView<Size> { View = { View = new Size(20, 20), VerticalAlignment = VerticalAlignment.Stretch }, Row = 2 },
                    new GridLayoutView<Size> { View = new Size(20, 60), RowSpan = 3 },
                },
                new []
                {
                    new Rectangle(0, 0, 100, 10),
                    new Rectangle(20, 0, 60, 10),

                    new Rectangle(0, 20, 20, 40),
                    new Rectangle(0, 0, 20, 60),
                }
            },

            { 200, 100,
                new GridDefinition(
                    new [] { "20", "Auto", "*"},
                    new [] { "10", "3*", "6*" }),
                new []
                {
                    new GridLayoutView<Size> { View = new Size(60, 10), Column = 1 },
                    new GridLayoutView<Size> { View = { View = new Size(20, 10), HorizontalAlignment = HorizontalAlignment.Stretch }, Column = 2 },

                    new GridLayoutView<Size> { Row = 1, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch } },
                    new GridLayoutView<Size> { Row = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch } },
                    new GridLayoutView<Size> { Row = 1, RowSpan = 2, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch } },
                    new GridLayoutView<Size> { Row = 0, RowSpan = 3, View = { View = new Size(20, 10), VerticalAlignment = VerticalAlignment.Stretch } },
                },
                new []
                {
                    new Rectangle(20, 0, 60, 10),
                    new Rectangle(80, 0, 120, 10),

                    new Rectangle(0, 10, 20, 30),
                    new Rectangle(0, 40, 20, 60),
                    new Rectangle(0, 10, 20, 90),
                    new Rectangle(0, 0, 20, 100),
                }
            },
        };
        
        [Theory, MemberData(nameof(Layouts))]
        public void grid(float width, float height, GridDefinition grid, GridLayoutView<Size>[] items, Rectangle[] expected)
        {
            Assert.Equal(expected, Layout(width, height, scope => scope.Grid(grid, items)));
        }
    }
}
