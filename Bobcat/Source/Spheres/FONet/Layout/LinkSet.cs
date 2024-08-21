namespace Fonet.Layout
{
    using System.Collections;
    using System.Drawing;
    using Fonet.Layout.Inline;

    internal class LinkSet
    {
        private readonly string destination;
        private ArrayList rects = new ArrayList();
        private int xoffset = 0;
        private int yoffset = 0;
        private int maxY = 0;
        protected int startIndent = 0;
        protected int endIndent = 0;
        private readonly int linkType;
        private readonly Area area;
        public const int INTERNAL = 0;
        public const int EXTERNAL = 1;
        private int contentRectangleWidth = 0;

        public LinkSet(string destination, Area area, int linkType)
        {
            this.destination = destination;
            this.area = area;
            this.linkType = linkType;
        }

        public void AddRect(Rectangle r, LineArea lineArea, InlineArea inlineArea)
        {
            LinkedRectangle linkedRectangle = new LinkedRectangle(r, lineArea, inlineArea);
            linkedRectangle.SetY(this.yoffset);
            if (this.yoffset > maxY)
            {
                maxY = this.yoffset;
            }
            rects.Add(linkedRectangle);
        }

        public void SetYOffset(int y)
        {
            this.yoffset = y;
        }

        public void SetXOffset(int x)
        {
            this.xoffset = x;
        }

        public void SetContentRectangleWidth(int contentRectangleWidth)
        {
            this.contentRectangleWidth = contentRectangleWidth;
        }

        public void ApplyAreaContainerOffsets(AreaContainer ac, Area area)
        {
            int height = area.GetAbsoluteHeight();
            BlockArea ba = (BlockArea)area;
            foreach (LinkedRectangle r in rects)
            {
                r.SetX(r.GetX() + ac.GetXPosition() + area.GetTableCellXOffset());
                r.SetY(ac.GetYPosition() - height + (maxY - r.GetY()) - ba.GetHalfLeading());
            }
        }

        public void MergeLinks()
        {
            int numRects = rects.Count;
            if (numRects == 1)
            {
                return;
            }

            LinkedRectangle curRect = new LinkedRectangle((LinkedRectangle)rects[0]);
            ArrayList nv = new ArrayList();

            for (int ri = 1; ri < numRects; ri++)
            {
                LinkedRectangle r = (LinkedRectangle)rects[ri];

                if (r.GetLineArea() == curRect.GetLineArea())
                {
                    curRect.SetWidth(r.GetX() + r.GetWidth() - curRect.GetX());
                }
                else
                {
                    nv.Add(curRect);
                    curRect = new LinkedRectangle(r);
                }

                if (ri == numRects - 1)
                {
                    nv.Add(curRect);
                }
            }

            rects = nv;
        }

        public void Align()
        {
            foreach (LinkedRectangle r in rects)
            {
                r.SetX(r.GetX() + r.GetLineArea().GetStartIndent()
                    + r.GetInlineArea().GetXOffset());
            }
        }

        public string GetDest()
        {
            return this.destination;
        }

        public ArrayList GetRects()
        {
            return this.rects;
        }

        public int GetEndIndent()
        {
            return endIndent;
        }

        public int GetStartIndent()
        {
            return startIndent;
        }

        public Area GetArea()
        {
            return area;
        }

        public int GetLinkType()
        {
            return linkType;
        }

    }
}