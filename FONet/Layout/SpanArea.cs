namespace Fonet.Layout
{
    using Fonet.Fo.Properties;
    using Fonet.Render.Pdf;

    internal class SpanArea : AreaContainer
    {
        private readonly int columnCount;
        private int currentColumn = 1;
        private readonly int columnGap = 0;
        private bool _isBalanced = false;

        public SpanArea(FontState fontState, int xPosition, int yPosition,
                        int allocationWidth, int maxHeight, int columnCount,
                        int columnGap) :
            base(fontState, xPosition, yPosition, allocationWidth, maxHeight,
                                 Position.ABSOLUTE)
        {
            this.contentRectangleWidth = allocationWidth;
            this.columnCount = columnCount;
            this.columnGap = columnGap;

            int columnWidth = (allocationWidth - columnGap * (columnCount - 1))
                / columnCount;
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                int colXPosition = (xPosition
                    + columnIndex * (columnWidth + columnGap));
                int colYPosition = yPosition;
                ColumnArea colArea = new ColumnArea(fontState, colXPosition,
                                                    colYPosition, columnWidth,
                                                    maxHeight, columnCount);
                AddChild(colArea);
                colArea.SetColumnIndex(columnIndex + 1);
            }
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderSpanArea(this);
        }

        public override void End()
        {
        }

        public override void Start()
        {
        }

        public override int SpaceLeft()
        {
            return maxHeight - currentHeight;
        }

        public int GetColumnCount()
        {
            return columnCount;
        }

        public int GetCurrentColumn()
        {
            return currentColumn;
        }

        public void SetCurrentColumn(int currentColumn)
        {
            if (currentColumn <= columnCount)
            {
                this.currentColumn = currentColumn;
            }
            else
            {
                this.currentColumn = columnCount;
            }
        }

        public AreaContainer GetCurrentColumnArea()
        {
            return (AreaContainer)GetChildren()[currentColumn - 1];
        }

        public bool IsBalanced()
        {
            return _isBalanced;
        }

        public void SetIsBalanced()
        {
            _isBalanced = true;
        }

        public int GetTotalContentHeight()
        {
            int totalContentHeight = 0;
            foreach (AreaContainer ac in GetChildren())
            {
                totalContentHeight += ac.GetContentHeight();
            }
            return totalContentHeight;
        }

        public int GetMaxContentHeight()
        {
            int maxContentHeight = 0;
            foreach (AreaContainer nextElm in GetChildren())
            {
                if (nextElm.GetContentHeight() > maxContentHeight)
                {
                    maxContentHeight = nextElm.GetContentHeight();
                }
            }
            return maxContentHeight;
        }

        public override void SetPage(Page page)
        {
            this.page = page;
            foreach (AreaContainer ac in GetChildren())
            {
                ac.SetPage(page);
            }
        }

        public bool IsLastColumn()
        {
            return (currentColumn == columnCount);
        }
    }
}