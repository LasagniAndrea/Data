namespace Fonet.Layout
{
    using System.Drawing;
    using Fonet.Layout.Inline;

    internal class LinkedRectangle
    {
        protected Rectangle link;
        protected LineArea lineArea;
        protected InlineArea inlineArea;

        public LinkedRectangle(Rectangle link, LineArea lineArea,
                               InlineArea inlineArea)
        {
            this.link = link;
            this.lineArea = lineArea;
            this.inlineArea = inlineArea;
        }

        public LinkedRectangle(LinkedRectangle lr)
        {
            this.link = lr.GetRectangle();
            this.lineArea = lr.GetLineArea();
            this.inlineArea = lr.GetInlineArea();
        }

        public void SetRectangle(Rectangle link)
        {
            this.link = link;
        }

        public Rectangle GetRectangle()
        {
            return this.link;
        }

        public LineArea GetLineArea()
        {
            return this.lineArea;
        }

        public void SetLineArea(LineArea lineArea)
        {
            this.lineArea = lineArea;
        }

        public InlineArea GetInlineArea()
        {
            return this.inlineArea;
        }

        public void SetLineArea(InlineArea inlineArea)
        {
            this.inlineArea = inlineArea;
        }

        public void SetX(int x)
        {
            this.link.X = x;
        }

        public void SetY(int y)
        {
            this.link.Y = y;
        }

        public void SetWidth(int width)
        {
            this.link.Width = width;
        }

        public void SetHeight(int height)
        {
            this.link.Height = height;
        }

        public int GetX()
        {
            return this.link.X;
        }

        public int GetY()
        {
            return this.link.Y;
        }

        public int GetWidth()
        {
            return this.link.Width;
        }

        public int GetHeight()
        {
            return this.link.Height;
        }

    }
}