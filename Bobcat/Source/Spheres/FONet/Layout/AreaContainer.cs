using Fonet.Render.Pdf;

namespace Fonet.Layout
{
    internal class AreaContainer : Area
    {
        private int xPosition;
        private int yPosition;
        private readonly int position;

        private string areaName;

        public AreaContainer(FontState fontState, int xPosition, int yPosition,
                             int allocationWidth, int maxHeight, int position)
            : base(fontState, allocationWidth, maxHeight)
        {
            this.xPosition = xPosition;
            this.yPosition = yPosition;
            this.position = position;
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderAreaContainer(this);
        }

        public int GetPosition()
        {
            return position;
        }

        public int GetXPosition()
        {
            return xPosition;
        }

        public void SetXPosition(int value)
        {
            xPosition = value;
        }

        public int GetYPosition()
        {
            return yPosition;
        }

        public int GetCurrentYPosition()
        {
            return yPosition;
        }

        public void SetYPosition(int value)
        {
            yPosition = value;
        }

        public void ShiftYPosition(int value)
        {
            yPosition += value;
        }

        public string GetAreaName()
        {
            return areaName;
        }

        public void SetAreaName(string areaName)
        {
            this.areaName = areaName;
        }
    }
}