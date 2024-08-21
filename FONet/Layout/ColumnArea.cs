using Fonet.Fo.Properties;
using Fonet.Render.Pdf;

namespace Fonet.Layout
{
    internal class ColumnArea : AreaContainer
    {
        private int columnIndex;
        private readonly int maxColumns;

        public ColumnArea(FontState fontState, int xPosition, int yPosition,
                          int allocationWidth, int maxHeight, int columnCount)
            : base(fontState, xPosition, yPosition,
                   allocationWidth, maxHeight, Position.ABSOLUTE)
        {
            this.maxColumns = columnCount;
            this.SetAreaName("normal-flow-ref.-area");
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderAreaContainer(this);
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

        public int GetColumnIndex()
        {
            return columnIndex;
        }

        public void SetColumnIndex(int columnIndex)
        {
            this.columnIndex = columnIndex;
        }

        public void IncrementSpanIndex()
        {
            SpanArea span = (SpanArea)this.parent;
            span.SetCurrentColumn(span.GetCurrentColumn() + 1);
        }

    }
}