namespace Fonet.Layout
{
    using System.Collections;
    using Fonet.DataTypes;
    using Fonet.Fo.Flow;
    using Fonet.Fo.Pagination;
    using Fonet.Render.Pdf;

    internal class Page
    {
        private readonly int height;
        private readonly int width;
        private BodyAreaContainer body;
        private AreaContainer before;
        private AreaContainer after;
        private AreaContainer start;
        private AreaContainer end;
        private readonly AreaTree areaTree;
        private PageSequence pageSequence;
        protected int pageNumber = 0;
        protected string formattedPageNumber;
        protected ArrayList linkSets = new ArrayList();
        private readonly ArrayList idList = new ArrayList();
        private ArrayList footnotes = null;
        private readonly ArrayList markers = null;

        internal Page(AreaTree areaTree, int height, int width)
        {
            this.areaTree = areaTree;
            this.height = height;
            this.width = width;
            markers = new ArrayList();
        }

        public IDReferences GetIDReferences()
        {
            return areaTree.GetIDReferences();
        }

        public void SetPageSequence(PageSequence pageSequence)
        {
            this.pageSequence = pageSequence;
        }

        public PageSequence GetPageSequence()
        {
            return pageSequence;
        }

        public AreaTree GetAreaTree()
        {
            return areaTree;
        }

        public void SetNumber(int number)
        {
            pageNumber = number;
        }

        public int GetNumber()
        {
            return pageNumber;
        }

        public void SetFormattedNumber(string number)
        {
            formattedPageNumber = number;
        }

        public string GetFormattedNumber()
        {
            return formattedPageNumber;
        }

        internal void AddAfter(AreaContainer area)
        {
            after = area;
            area.SetPage(this);
        }

        internal void AddBefore(AreaContainer area)
        {
            before = area;
            area.SetPage(this);
        }

        public void AddBody(BodyAreaContainer area)
        {
            body = area;
            area.SetPage(this);
            ((BodyAreaContainer)area).GetMainReferenceArea().SetPage(this);
            ((BodyAreaContainer)area).GetBeforeFloatReferenceArea().SetPage(this);
            ((BodyAreaContainer)area).GetFootnoteReferenceArea().SetPage(this);
        }

        internal void AddEnd(AreaContainer area)
        {
            end = area;
            area.SetPage(this);
        }

        internal void AddStart(AreaContainer area)
        {
            start = area;
            area.SetPage(this);
        }

        public void Render(PdfRenderer renderer)
        {
            renderer.RenderPage(this);
        }

        public AreaContainer GetAfter()
        {
            return after;
        }

        public AreaContainer GetBefore()
        {
            return before;
        }

        public AreaContainer GetStart()
        {
            return start;
        }

        public AreaContainer GetEnd()
        {
            return end;
        }

        public BodyAreaContainer GetBody()
        {
            return body;
        }

        public int GetHeight()
        {
            return height;
        }

        public int GetWidth()
        {
            return width;
        }

        public FontInfo GetFontInfo()
        {
            return areaTree.GetFontInfo();
        }

        public void AddLinkSet(LinkSet linkSet)
        {
            linkSets.Add(linkSet);
        }

        public ArrayList GetLinkSets()
        {
            return linkSets;
        }

        public bool HasLinks()
        {
            return linkSets.Count != 0;
        }

        public void AddToIDList(string id)
        {
            idList.Add(id);
        }

        public ArrayList GetIDList()
        {
            return idList;
        }

        public ArrayList GetPendingFootnotes()
        {
            return footnotes;
        }

        public void SetPendingFootnotes(ArrayList v)
        {
            footnotes = v;
            if (footnotes != null)
            {
                foreach (FootnoteBody fb in footnotes)
                {
                    if (!Footnote.LayoutFootnote(this, fb, null))
                    {
                        // footnotes are too large to fit on empty page.
                    }
                }
                footnotes = null;
            }
        }

        public void AddPendingFootnote(FootnoteBody fb)
        {
            if (footnotes == null)
            {
                footnotes = new ArrayList();
            }
            footnotes.Add(fb);
        }

        public void UnregisterMarker(Marker marker)
        {
            markers.Remove(marker);
        }

        public void RegisterMarker(Marker marker)
        {
            markers.Add(marker);
        }

        public ArrayList GetMarkers()
        {
            return this.markers;
        }

    }
}