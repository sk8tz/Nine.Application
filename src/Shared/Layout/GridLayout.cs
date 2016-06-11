namespace Nine.Application.Layout
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public enum GridUnitType { Auto, Pixel, Star }

    public class GridDefinition
    {
        public readonly IReadOnlyList<GridColumnDefinition> Columns;
        public readonly IReadOnlyList<GridRowDefinition> Rows;

        public GridDefinition(IEnumerable<GridColumnDefinition> columns, IEnumerable<GridRowDefinition> rows)
        {
            Columns = columns.Any() ? columns.ToArray() : new[] { GridColumnDefinition.Auto };
            Rows = rows.Any() ? rows.ToArray() : new[] { GridRowDefinition.Auto };
        }
    }

    public struct GridColumnDefinition
    {
        public static readonly GridColumnDefinition Auto = new GridColumnDefinition { Type = GridUnitType.Auto };

        public float Width, MinWidth, MaxWidth;

        public GridUnitType Type;

        public static implicit operator GridColumnDefinition(float width)
            => new GridColumnDefinition { Width = width, Type = GridUnitType.Pixel };

        public static implicit operator GridColumnDefinition(string width)
        {
            if (width == null || width.Length == 0) return Auto;
            if (width[width.Length - 1] == '*') return new GridColumnDefinition { Width = float.Parse(width.Substring(0, width.Length - 1)), Type = GridUnitType.Star };
            return new GridColumnDefinition { Width = float.Parse(width), Type = GridUnitType.Pixel };
        }

        public override string ToString() => $"{Width} {Type}";
    }

    public struct GridRowDefinition
    {
        public static readonly GridRowDefinition Auto = new GridRowDefinition { Type = GridUnitType.Auto };

        public float Height, MinHeight, MaxHeight;

        public GridUnitType Type;

        public static implicit operator GridRowDefinition(float height)
            => new GridRowDefinition { Height = height, Type = GridUnitType.Pixel };

        public static implicit operator GridRowDefinition(string height)
        {
            if (height == null || height.Length == 0) return Auto;
            if (height[height.Length - 1] == '*') return new GridRowDefinition { Height = float.Parse(height.Substring(0, height.Length - 1)), Type = GridUnitType.Star };
            return new GridRowDefinition { Height = float.Parse(height), Type = GridUnitType.Pixel };
        }

        public override string ToString() => $"{Height} {Type}";
    }

    public struct GridLayoutView<T>
    {
        public LayoutView<T> View;

        public int Row;
        public int Column;
        public int RowSpan;
        public int ColumnSpan;
        public HorizontalAlignment HorizontalAlignment;
        public VerticalAlignment VerticalAlignment;

        public static implicit operator GridLayoutView<T>(T view) => new GridLayoutView<T> { View = view };
        public static implicit operator GridLayoutView<T>(LayoutView<T> view) => new GridLayoutView<T> { View = view };
    }

    public static class GridLayout
    {
        public static LayoutView<T> Grid<T>(this LayoutScope<T> scope, GridDefinition grid, params GridLayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;
            if (views.Length == 1) return views[0].View;

            return new GridPanel<T>(scope, grid, views).ToLayoutView();
        }

        class GridPanel<T> : ILayoutPanel<T>
        {
            private readonly LayoutScope<T> _scope;
            private readonly GridDefinition _grid;
            private readonly IReadOnlyList<GridLayoutView<T>> _views;
            private readonly Size[] _viewSizes;
            private readonly float[] _columnWidths;
            private readonly float[] _rowHeights;

            public GridPanel(LayoutScope<T> scope, GridDefinition grid, IReadOnlyList<GridLayoutView<T>> views)
            {
                Debug.Assert(views.Count > 1);

                _scope = scope;
                _grid = grid;
                _views = views;
                _viewSizes = new Size[_views.Count];
                _columnWidths = new float[_grid.Columns.Count];
                _rowHeights = new float[_grid.Rows.Count];
            }

            public Size Measure(float width, float height)
            {
                var finalSize = Size.Zero;

                for (var i = 0; i < _views.Count; i++)
                {
                    _viewSizes[i] = _scope.Measure(_views[i].View, width, height);
                }

                for (var c = 0; c < _grid.Columns.Count; c++)
                {
                    var column = _grid.Columns[c];
                    var columnWidth = Math.Max(MeasureColumnWidth(ref column, c), column.MinWidth);
                    finalSize.Width += column.MaxWidth == 0.0 ? columnWidth : Math.Min(columnWidth, column.MaxWidth);
                }

                for (var r = 0; r < _grid.Rows.Count; r++)
                {
                    var row = _grid.Rows[r];
                    var rowHeight = Math.Max(MeasureRowHeight(ref row, r), row.MinHeight);
                    finalSize.Height += row.MaxHeight == 0.0 ? rowHeight : Math.Min(rowHeight, row.MaxHeight);
                }

                return finalSize;
            }

            private float MeasureColumnWidth(ref GridColumnDefinition column, int c)
            {
                if (column.Type == GridUnitType.Star)
                {
                    throw new NotImplementedException();
                }

                if (column.Type == GridUnitType.Auto)
                {
                    var width = 0.0f;
                    for (var i = 0; i < _views.Count; i++)
                    {
                        var view = _views[i];
                        if (c >= view.Column && c < view.Column + Math.Max(1, view.ColumnSpan))
                        {
                            if (_viewSizes[i].Width > width)
                            {
                                width = _viewSizes[i].Width;
                            }
                        }
                    }
                    return width;
                }

                return column.Width;
            }

            private float MeasureRowHeight(ref GridRowDefinition row, float r)
            {
                if (row.Type == GridUnitType.Star)
                {
                    throw new NotImplementedException();
                }

                if (row.Type == GridUnitType.Auto)
                {
                    var height = 0.0f;
                    for (var i = 0; i < _views.Count; i++)
                    {
                        var view = _views[i];
                        if (r >= view.Row && r < view.Row + Math.Max(1, view.RowSpan))
                        {
                            if (_viewSizes[i].Height > height)
                            {
                                height = _viewSizes[i].Height;
                            }
                        }
                    }
                    return height;
                }

                return row.Height;
            }

            public void Arrange(float x, float y, float width, float height)
            {
                for (var i = 0; i < _views.Count; i++)
                {
                    _viewSizes[i] = _scope.Measure(_views[i].View, width, height);
                }

                for (var c = 0; c < _grid.Columns.Count; c++)
                {
                    var column = _grid.Columns[c];
                    var columnWidth = Math.Max(MeasureColumnWidth(ref column, c), column.MinWidth);
                    if (column.MaxWidth != 0.0) columnWidth = Math.Min(columnWidth, column.MaxWidth);

                    _columnWidths[c] = columnWidth;
                }

                for (var r = 0; r < _grid.Rows.Count; r++)
                {
                    var row = _grid.Rows[r];
                    var rowHeight = Math.Max(MeasureRowHeight(ref row, r), row.MinHeight);
                    if (row.MaxHeight != 0.0) rowHeight = Math.Min(rowHeight, row.MaxHeight);

                    _rowHeights[r] = rowHeight;
                }

                for (var i = 0; i < _views.Count; i++)
                {
                    var view = _views[i];

                    _scope.Arrange(view.View, 0, 0, width, height);
                }
            }
        }
    }
}
