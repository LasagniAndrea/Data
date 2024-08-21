namespace Fonet.Layout
{
    internal class PageMaster
    {
        private readonly int width;
        private readonly int height;
        private BodyRegionArea body;
        private RegionArea before;
        private RegionArea after;
        private RegionArea start;
        private RegionArea end;

        public PageMaster(int pageWidth, int pageHeight)
        {
            this.width = pageWidth;
            this.height = pageHeight;
        }

        public void AddAfter(RegionArea region)
        {
            this.after = region;
        }

        public void AddBefore(RegionArea region)
        {
            this.before = region;
        }

        public void AddBody(BodyRegionArea region)
        {
            this.body = region;
        }

        public void AddEnd(RegionArea region)
        {
            this.end = region;
        }

        public void AddStart(RegionArea region)
        {
            this.start = region;
        }

        public int GetHeight()
        {
            return this.height;
        }

        public int GetWidth()
        {
            return this.width;
        }

        public Page MakePage(AreaTree areaTree)
        {
            Page p = new Page(areaTree, this.height, this.width);
            if (this.body != null)
            {
                p.AddBody(body.MakeBodyAreaContainer());
            }
            if (this.before != null)
            {
                p.AddBefore(before.MakeAreaContainer());
            }
            if (this.after != null)
            {
                p.AddAfter(after.MakeAreaContainer());
            }
            if (this.start != null)
            {
                p.AddStart(start.MakeAreaContainer());
            }
            if (this.end != null)
            {
                p.AddEnd(end.MakeAreaContainer());
            }

            return p;
        }

    }
}