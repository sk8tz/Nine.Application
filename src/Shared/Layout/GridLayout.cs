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

        public readonly IReadOnlyList<GridColumnDefinition> Columns;
        public readonly IReadOnlyList<GridRowDefinition> Rows;

        public GridDefinition(IEnumerable<string> columns, IEnumerable<string> rows)
        {
            Columns = columns.Any() ? columns.Select(c => (GridColumnDefinition)c).ToArray() : Default.Columns;
            Rows = rows.Any() ? rows.Select(r => (GridRowDefinition)r).ToArray() : Default.Rows;
        }

        public GridDefinition(IEnumerable<GridColumnDefinition> columns, IEnumerable<GridRowDefinition> rows)
        {
            Columns = columns.Any() ? columns.ToArray() : Default.Columns;
            Rows = rows.Any() ? rows.ToArray() : Default.Rows;
        }
    }

    public struct GridColumnDefinition
    {
        public static readonly GridColumnDefinition Auto = new GridColumnDefinition { Type = GridUnitType.Auto };
        public static readonly GridColumnDefinition Fill = new GridColumnDefinition { Type = GridUnitType.Star };

        public float Width;
        public GridUnitType Type;

        public static implicit operator GridColumnDefinition(float width)
            => new GridColumnDefinition { Width = width, Type = GridUnitType.Pixel };

        public static implicit operator GridColumnDefinition(string width)
        {
            if (width == null || width.Length == 0 || width.Equals("Auto", StringComparison.OrdinalIgnoreCase)) return Auto;
            if (width[width.Length - 1] == '*') return new GridColumnDefinition { Width = float.Parse(width.Substring(0, width.Length - 1)), Type = GridUnitType.Star };
            return new GridColumnDefinition { Width = float.Parse(width), Type = GridUnitType.Pixel };
        }

        public override string ToString() => $"{Width} {Type}";
    }

    public struct GridRowDefinition
    {
        public static readonly GridRowDefinition Auto = new GridRowDefinition { Type = GridUnitType.Auto };
        public static readonly GridRowDefinition Fill = new GridRowDefinition { Type = GridUnitType.Star };

        public float Height;
        public GridUnitType Type;

        public static implicit operator GridRowDefinition(float height)
            => new GridRowDefinition { Height = height, Type = GridUnitType.Pixel };

        public static implicit operator GridRowDefinition(string height)
        {
            if (height == null || height.Length == 0 || height.Equals("Auto", StringComparison.OrdinalIgnoreCase)) return Auto;
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
                return new Size(MeasureColumns(width, height), MeasureRows(width, height));
            }

            private float MeasureColumns(float width, float height)
            {
                var unknownSize = width;

                for (var i = 0; i < _grid.Columns.Count; i++)
                {
                    if (_grid.Columns[i].Type == GridUnitType.Pixel)
                    {
                        unknownSize -= (_columnWidths[i] = _grid.Columns[i].Width);
                    }
                    else
                    {
                        _columnWidths[i] = 0;
                    }
                }

                for (var span = 1; span <= _maxColumnSpan; span++)
                {
                    for (var cursor = 0; cursor <= _grid.Columns.Count - span; cursor++)
                    {
                        var lastUnknownCursor = -1;

                        for (var i = cursor + span - 1; i >= cursor; i--)
                        {
                            if (_grid.Columns[i].Type != GridUnitType.Pixel)
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

                var totalSize = 0.0f;

                for (var i = 0; i < _columnWidths.Length; i++)
                {
                    totalSize += _columnWidths[i];
                }

                return totalSize;
            }

            private float MeasureRows(float width, float height)
            {
                var unknownSize = height;

                for (var i = 0; i < _grid.Rows.Count; i++)
                {
                    if (_grid.Rows[i].Type == GridUnitType.Pixel)
                    {
                        unknownSize -= (_rowHeights[i] = _grid.Rows[i].Height);
                    }
                    else
                    {
                        _rowHeights[i] = 0;
                    }
                }

                for (var span = 1; span <= _maxRowSpan; span++)
                {
                    for (var cursor = 0; cursor <= _grid.Rows.Count - span; cursor++)
                    {
                        var lastUnknownCursor = -1;

                        for (var i = cursor + span - 1; i >= cursor; i--)
                        {
                            if (_grid.Rows[i].Type != GridUnitType.Pixel)
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

                var totalSize = 0.0f;

                for (var i = 0; i < _rowHeights.Length; i++)
                {
                    totalSize += _rowHeights[i];
                }

                return totalSize;
            }

            public void Arrange(float x, float y, float width, float height)
            {
                var minWidth = MeasureColumns(width, height);
                var minHeight = MeasureRows(width, height);

                var starWidth = Math.Max(0, width - minWidth);
                var starHeight = Math.Max(0, height - minHeight);

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
