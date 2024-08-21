using System.Collections;
using Fonet.Fo.Flow;
using Fonet.Render.Pdf;

namespace Fonet.Layout
{
    internal class BlockArea : Area
    {
        protected int startIndent;
        protected int endIndent;
        protected int textIndent;
        protected int lineHeight;
        protected int halfLeading;
        protected int align;
        protected int alignLastLine;
        protected LineArea currentLineArea;
        protected LinkSet currentLinkSet;
        protected bool hasLines = false;
        protected HyphenationProps hyphProps;
        protected ArrayList pendingFootnotes = null;

        public BlockArea(FontState fontState, int allocationWidth, int maxHeight,
                         int startIndent, int endIndent, int textIndent,
                         int align, int alignLastLine, int lineHeight)
            : base(fontState, allocationWidth, maxHeight)
        {
            this.startIndent = startIndent;
            this.endIndent = endIndent;
            this.textIndent = textIndent;
            this.contentRectangleWidth = allocationWidth - startIndent
                - endIndent;
            this.align = align;
            this.alignLastLine = alignLastLine;
            this.lineHeight = lineHeight;

            if (fontState != null)
            {
                this.halfLeading = (lineHeight - fontState.FontSize) / 2;
            }
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderBlockArea(this);
        }

        protected void AddLineArea(LineArea la)
        {
            if (!la.IsEmpty())
            {
                la.CalcVerticalAlign();
                this.AddDisplaySpace(this.halfLeading);
                int size = la.GetHeight();
                this.AddChild(la);
                this.IncreaseHeight(size);
                this.AddDisplaySpace(this.halfLeading);
            }
            if (pendingFootnotes != null)
            {
                foreach (FootnoteBody fb in pendingFootnotes)
                {
                    Page page = GetPage();
                    if (!Footnote.LayoutFootnote(page, fb, this))
                    {
                        page.AddPendingFootnote(fb);
                    }
                }
                pendingFootnotes = null;
            }
        }

        public LineArea GetCurrentLineArea()
        {
            if (currentHeight + lineHeight > maxHeight)
            {
                return null;
            }
            this.currentLineArea.ChangeHyphenation(hyphProps);
            this.hasLines = true;
            return this.currentLineArea;
        }

        public LineArea CreateNextLineArea()
        {
            if (this.hasLines)
            {
                this.currentLineArea.Align(this.align);
                this.AddLineArea(this.currentLineArea);
            }
            this.currentLineArea = new LineArea(fontState, lineHeight,allocationWidth, startIndent, endIndent, currentLineArea);
            this.currentLineArea.ChangeHyphenation(hyphProps);
            if (currentHeight + lineHeight > maxHeight)
            {
                return null;
            }
            return this.currentLineArea;
        }

        public void SetupLinkSet(LinkSet ls)
        {
            if (ls != null)
            {
                this.currentLinkSet = ls;
                ls.SetYOffset(currentHeight);
            }
        }

        public override void End()
        {
            if (this.hasLines)
            {
                this.currentLineArea.AddPending();
                this.currentLineArea.Align(this.alignLastLine);
                this.AddLineArea(this.currentLineArea);
            }
        }

        public override void Start()
        {
            currentLineArea = new LineArea(fontState, lineHeight, allocationWidth, startIndent + textIndent, endIndent,null);
        }

        public int GetEndIndent()
        {
            return endIndent;
        }

        public int GetStartIndent()
        {
            return startIndent;
        }

        public void SetIndents(int startIndent, int endIndent)
        {
            this.startIndent = startIndent;
            this.endIndent = endIndent;
            this.contentRectangleWidth = allocationWidth - startIndent
                - endIndent;
        }

        public override int SpaceLeft()
        {
            return maxHeight - currentHeight - (GetPaddingTop() + GetPaddingBottom() + GetBorderTopWidth() + GetBorderBottomWidth());
        }

        public int GetHalfLeading()
        {
            return halfLeading;
        }

        public void SetHyphenation(HyphenationProps hyphProps)
        {
            this.hyphProps = hyphProps;
        }

        public void AddFootnote(FootnoteBody fb)
        {
            if (pendingFootnotes == null)
            {
                pendingFootnotes = new ArrayList();
            }
            pendingFootnotes.Add(fb);
        }

    }
}