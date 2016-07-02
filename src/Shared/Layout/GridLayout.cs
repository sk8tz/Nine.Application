namespace Nine.Application.Layout
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public enum GridUnitType { Auto, Pixel, Star }

    public class GridDefinition
    {
        public static readonly GridDefinition Default = new GridDefinition(new[] { GridColumnDefinition.Fill }, new[] { GridRowDefinition.Fill });

        internal readonly float KnownWidth;
        internal readonly float KnownHeight;

        internal readonly float[] ColumnStarWeights;
        internal readonly float[] RowStarWeights;

        public readonly IReadOnlyList<GridColumnDefinition> Columns;
        public readonly IReadOnlyList<GridRowDefinition> Rows;

        public GridDefinition(IEnumerable<string> columns, IEnumerable<string> rows)
            : this(columns.Select(c => (GridColumnDefinition)c), rows.Select(r => (GridRowDefinition)r))
        { }

        public GridDefinition(IEnumerable<GridColumnDefinition> columns, IEnumerable<GridRowDefinition> rows)
        {
            Columns = columns.Any() ? columns.ToArray() : Default.Columns;
            Rows = rows.Any() ? rows.ToArray() : Default.Rows;

            KnownWidth = Columns.Where(c => c.Type == GridUnitType.Pixel).Sum(c => c.Width);
            KnownHeight = Rows.Where(r => r.Type == GridUnitType.Pixel).Sum(r => r.Height);

            var totalColumnStarWeights = Columns.Where(c => c.Type == GridUnitType.Star).Sum(c => c.Width);
            if (totalColumnStarWeights > 0)
            {
                ColumnStarWeights = new float[Columns.Count];
                for (var i = 0; i < Columns.Count; i++)
                {
                    ColumnStarWeights[i] = Columns[i].Width / totalColumnStarWeights;
                }
            }

            var totalRowStarWeights = Rows.Where(r => r.Type == GridUnitType.Star).Sum(r => r.Height);
            if (totalRowStarWeights > 0)
            {
                RowStarWeights = new float[Rows.Count];
                for (var i = 0; i < Rows.Count; i++)
                {
                    RowStarWeights[i] = Rows[i].Height / totalRowStarWeights;
                }
            }
        }
    }

    public struct GridColumnDefinition
    {
        public static readonly GridColumnDefinition Auto = new GridColumnDefinition { Type = GridUnitType.Auto };
        public static readonly GridColumnDefinition Fill = new GridColumnDefinition { Type = GridUnitType.Star, Width = 1 };

        public float Width;
        public GridUnitType Type;

        public static implicit operator GridColumnDefinition(float width)
            => new GridColumnDefinition { Width = width, Type = GridUnitType.Pixel };

        public static implicit operator GridColumnDefinition(string width)
        {
            if (width == null || width.Length == 0 || width.Equals("Auto", StringComparison.OrdinalIgnoreCase)) return Auto;
            if (width == "*") return Fill;
            if (width[width.Length - 1] == '*') return new GridColumnDefinition { Width = float.Parse(width.Substring(0, width.Length - 1)), Type = GridUnitType.Star };
            return new GridColumnDefinition { Width = float.Parse(width), Type = GridUnitType.Pixel };
        }

        public override string ToString() => $"{Width} {Type}";
    }

    public struct GridRowDefinition
    {
        public static readonly GridRowDefinition Auto = new GridRowDefinition { Type = GridUnitType.Auto };
        public static readonly GridRowDefinition Fill = new GridRowDefinition { Type = GridUnitType.Star, Height = 1 };

        public float Height;
        public GridUnitType Type;

        public static implicit operator GridRowDefinition(float height)
            => new GridRowDefinition { Height = height, Type = GridUnitType.Pixel };

        public static implicit operator GridRowDefinition(string height)
        {
            if (height == null || height.Length == 0 || height.Equals("Auto", StringComparison.OrdinalIgnoreCase)) return Auto;
            if (height == "*") return Fill;
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

        public static implicit operator GridLayoutView<T>(T view) => new GridLayoutView<T> { View = view };
        public static implicit operator GridLayoutView<T>(LayoutView<T> view) => new GridLayoutView<T> { View = view };
    }

    public static class GridLayout
    {
        public static LayoutView<T> Grid<T>(this LayoutScope<T> scope, GridDefinition grid, params GridLayoutView<T>[] views)
        {
            if (views.Length == 0) return LayoutView<T>.None;

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
                Debug.Assert(views.Length > 0);

                _scope = scope;
                _grid = grid ?? GridDefinition.Default;
                _views = views;

                var columnCount = _grid.Columns.Count;
                var rowCount = _grid.Rows.Count;

                _columnWidths = new float[columnCount];
                _rowHeights = new float[rowCount];

                for (var i = 0; i < _views.Length; i++)
                {
                    if (_views[i].Column < 0) _views[i].Column = 0;
                    if (_views[i].ColumnSpan <= 0) _views[i].ColumnSpan = 1;
                    if (_views[i].Column + _views[i].ColumnSpan > columnCount) _views[i].ColumnSpan = columnCount - _views[i].Column;
                    if (_views[i].ColumnSpan > _maxColumnSpan) _maxColumnSpan = _views[i].ColumnSpan;

                    if (_views[i].Row < 0) _views[i].Row = 0;
                    if (_views[i].RowSpan <= 0) _views[i].RowSpan = 1;
                    if (_views[i].Row + _views[i].RowSpan > rowCount) _views[i].RowSpan = rowCount - _views[i].Row;
                    if (_views[i].RowSpan > _maxRowSpan) _maxRowSpan = _views[i].RowSpan;
                }
            }

            public Size Measure(float width, float height)
            {
                MeasureColumns(width, height);
                MeasureRows(width, height);
                
                var minWidth = 0.0f;
                for (var i = 0; i < _columnWidths.Length; i++)
                {
                    minWidth += _columnWidths[i];
                }

                var minHeight = 0.0f;
                for (var i = 0; i < _rowHeights.Length; i++)
                {
                    minHeight += _rowHeights[i];
                }

                return new Size(minWidth, minHeight);
            }

            private void MeasureColumns(float width, float height)
            {
                var columns = (GridColumnDefinition[])_grid.Columns;
                var unknownSize = width - _grid.KnownWidth;

                for (var i = 0; i < columns.Length; i++)
                {
                    _columnWidths[i] = (columns[i].Type == GridUnitType.Pixel) ? columns[i].Width : 0;
                }

                for (var span = 1; span <= _maxColumnSpan; span++)
                {
                    for (var cursor = 0; cursor <= columns.Length - span; cursor++)
                    {
                        var lastUnknownCursor = -1;

                        for (var i = cursor + span - 1; i >= cursor; i--)
                        {
                            if (columns[i].Type != GridUnitType.Pixel)
                            {
                                lastUnknownCursor = i;
                                break;
                            }
                        }

                        if (lastUnknownCursor == -1)
                        {
                            continue;
                        }

                        var measureSize = _columnWidths[cursor];

                        for (var i = cursor + 1; i < cursor + span; i++)
                        {
                            measureSize += _columnWidths[i];
                        }

                        measureSize += unknownSize;

                        var spanSize = 0.0f;

                        for (var i = 0; i < _views.Length; i++)
                        {
                            if (_views[i].ColumnSpan == span && _views[i].Column == cursor)
                            {
                                var size = _scope.Measure(ref _views[i].View, measureSize, height);

                                if (size.Width > spanSize) spanSize = size.Width;
                            }
                        }

                        if (spanSize > 0)
                        {
                            for (var i = cursor; i < cursor + span; i++)
                            {
                                if (i != lastUnknownCursor)
                                {
                                    spanSize -= _columnWidths[i];
                                }
                            }

                            var original = _columnWidths[lastUnknownCursor];

                            _columnWidths[lastUnknownCursor] = spanSize;

                            unknownSize -= (spanSize - original);
                        }
                    }
                }
            }

            private void MeasureRows(float width, float height)
            {
                var rows = (GridRowDefinition[])_grid.Rows;

                var unknownSize = height;
                for (var i = 0; i < rows.Length; i++)
                {
                    _rowHeights[i] = rows[i].Type == GridUnitType.Pixel ? rows[i].Height : 0;
                }

                for (var span = 1; span <= _maxRowSpan; span++)
                {
                    for (var cursor = 0; cursor <= rows.Length - span; cursor++)
                    {
                        var lastUnknownCursor = -1;

                        for (var i = cursor + span - 1; i >= cursor; i--)
                        {
                            if (rows[i].Type != GridUnitType.Pixel)
                            {
                                lastUnknownCursor = i;
                                break;
                            }
                        }

                        if (lastUnknownCursor == -1)
                        {
                            continue;
                        }

                        var measureSize = _rowHeights[cursor];

                        for (var i = cursor + 1; i < cursor + span; i++)
                        {
                            measureSize += _rowHeights[i];
                        }

                        measureSize += unknownSize;

                        var spanSize = 0.0f;

                        for (var i = 0; i < _views.Length; i++)
                        {
                            if (_views[i].RowSpan == span && _views[i].Row == cursor)
                            {
                                var size = _scope.Measure(ref _views[i].View, width, measureSize);

                                if (size.Height > spanSize) spanSize = size.Height;
                            }
                        }

                        if (spanSize > 0)
                        {
                            for (var i = cursor; i < cursor + span; i++)
                            {
                                if (i != lastUnknownCursor)
                                {
                                    spanSize -= _rowHeights[i];
                                }
                            }

                            var original = _rowHeights[lastUnknownCursor];
                            _rowHeights[lastUnknownCursor] = spanSize;

                            unknownSize -= (spanSize - original);
                        }
                    }
                }
            }

            public void Arrange(float x, float y, float width, float height)
            {
                MeasureColumns(width, height);
                MeasureRows(width, height);
                
                if (_grid.ColumnStarWeights != null)
                {
                    var columns = (GridColumnDefinition[])_grid.Columns;

                    var starWidth = 0.0f;

                    for (var i = 0; i < _columnWidths.Length; i++)
                    {
                        if (columns[i].Type != GridUnitType.Star)
                        {
                            starWidth += _columnWidths[i];
                        }
                    }

                    starWidth = width - starWidth;

                    if (starWidth > 0)
                    {
                        for (var i = 0; i < _grid.ColumnStarWeights.Length; i++)
                        {
                            if (columns[i].Type == GridUnitType.Star)
                            {
                                var columnWidth = starWidth * _grid.ColumnStarWeights[i];
                                if (columnWidth > _columnWidths[i])
                                {
                                    _columnWidths[i] = columnWidth;
                                }
                            }
                        }
                    }
                }

                if (_grid.RowStarWeights != null)
                {
                    var rows = (GridRowDefinition[])_grid.Rows;
                    var starHeight = 0.0f;

                    for (var i = 0; i < _rowHeights.Length; i++)
                    {
                        if (rows[i].Type != GridUnitType.Star)
                        {
                            starHeight += _rowHeights[i];
                        }
                    }

                    starHeight = height - starHeight;
                    if (starHeight > 0)
                    {
                        for (var i = 0; i < _grid.RowStarWeights.Length; i++)
                        {
                            if (_grid.Rows[i].Type == GridUnitType.Star)
                            {
                                var rowHeight = starHeight * _grid.RowStarWeights[i];
                                if (rowHeight > _rowHeights[i])
                                {
                                    _rowHeights[i] = rowHeight;
                                }
                            }
                        }
                    }
                }

                var columnLefts = _columnWidths;
                var currentLeft = _columnWidths[0];

                for (var i = 1; i < _columnWidths.Length; i++)
                {
                    currentLeft += _columnWidths[i];
                    _columnWidths[i] = currentLeft;
                }

                var rowTops = _rowHeights;
                var currentTop = _rowHeights[0];

                for (var i = 1; i < _rowHeights.Length; i++)
                {
                    currentTop += _rowHeights[i];
                    _rowHeights[i] = currentTop;
                }

                for (var i = 0; i < _views.Length; i++)
                {
                    var view = _views[i];

                    var left = view.Column == 0 ? 0 : columnLefts[view.Column - 1];
                    var right = columnLefts[view.Column + view.ColumnSpan - 1];
                    var top = view.Row == 0 ? 0 : rowTops[view.Row - 1];
                    var bottom = rowTops[view.Row + view.RowSpan - 1];

                    _scope.Arrange(ref view.View, left, top, right - left, bottom - top);
                }
            }
        }
    }
}
