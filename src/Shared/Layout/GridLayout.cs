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

        public float Width;
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

        public float Height;
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
            private readonly GridLayoutView<T>[] _views;
            private readonly float[] _columnWidths;
            private readonly float[] _rowHeights;
            private readonly int _maxColumnSpan;
            private readonly int _maxRowSpan;

            public GridPanel(LayoutScope<T> scope, GridDefinition grid, GridLayoutView<T>[] views)
            {
                Debug.Assert(views.Length > 1);

                var columnCount = _grid.Columns.Count;
                var rowCount = _grid.Rows.Count;

                _scope = scope;
                _grid = grid;
                _views = views;
                _columnWidths = new float[columnCount];
                _rowHeights = new float[rowCount];

                for (var i = 0; i < _views.Length; i++)
                {
                    if (_views[i].ColumnSpan <= 0) _views[i].ColumnSpan = 1;
                    if (_views[i].Column + _views[i].ColumnSpan > columnCount) _views[i].ColumnSpan = columnCount - _views[i].Column;
                    if (_views[i].ColumnSpan > _maxColumnSpan) _maxColumnSpan = _views[i].ColumnSpan;

                    if (_views[i].RowSpan <= 0) _views[i].RowSpan = 1;
                    if (_views[i].Row + _views[i].RowSpan > rowCount) _views[i].RowSpan = rowCount - _views[i].Row;
                    if (_views[i].RowSpan > _maxRowSpan) _maxRowSpan = _views[i].RowSpan;
                }
            }

            public Size Measure(float width, float height)
            {
                var unknownWidth = width;
                var unknownHeight = height;

                for (var c = 0; c < _grid.Columns.Count; c++)
                {
                    if (_grid.Columns[c].Type == GridUnitType.Pixel)
                    {
                        unknownWidth -= (_columnWidths[c] = _grid.Columns[c].Width);
                    }
                    else
                    {
                        _columnWidths[c] = 0;
                    }
                }

                for (var cSpan = 1; cSpan < _maxColumnSpan; cSpan++)
                {
                    for (var c = 0; c < _grid.Columns.Count; c++)
                    {
                        var maxSpan = Math.Min(cSpan, _grid.Columns.Count - c);

                        if (_grid.Columns[c].Type != GridUnitType.Pixel)
                        {
                            var columnSpanWidth = 0.0f;

                            var measureWidth = unknownWidth - _columnWidths[c];

                            for (var i = 1; i < maxSpan; i++)
                            {
                                if (_grid.Columns[c + i].Type != GridUnitType.Pixel)
                                {
                                    measureWidth -= _columnWidths[c + i];
                                }
                            }

                            for (var i = 0; i < _views.Length; i++)
                            {
                                if (_views[i].ColumnSpan == maxSpan && _views[i].Column == c)
                                {
                                    var size = _scope.Measure(_views[i].View, measureWidth, height);

                                    if (size.Width > columnSpanWidth) columnSpanWidth = size.Width;
                                }
                            }

                            _columnWidths[c + maxSpan - 1] = columnSpanWidth;
                        }
                    }
                }

                return new Size(width - unknownWidth, height - unknownHeight);
            }

            public void Arrange(float x, float y, float width, float height)
            {

            }
        }
    }
}
