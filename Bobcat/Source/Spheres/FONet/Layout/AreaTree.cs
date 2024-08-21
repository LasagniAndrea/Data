using System.Collections;
using System.IO;
using Fonet.DataTypes;
using Fonet.Fo.Pagination;

namespace Fonet.Layout
{
    internal class AreaTree
    {
        private FontInfo fontInfo;

        private readonly StreamRenderer streamRenderer;

        public AreaTree(StreamRenderer streamRenderer)
        {
            this.streamRenderer = streamRenderer;
        }

        public void SetFontInfo(FontInfo fontInfo)
        {
            this.fontInfo = fontInfo;
        }

        public FontInfo GetFontInfo()
        {
            return this.fontInfo;
        }

        public void AddPage(Page page)
        {
            try
            {
                streamRenderer.QueuePage(page);
            }
            catch (IOException e)
            {
                throw new FonetException("", e);
            }
        }

        public IDReferences GetIDReferences()
        {
            return streamRenderer.GetIDReferences();
        }

        public ArrayList GetDocumentMarkers()
        {
            return streamRenderer.GetDocumentMarkers();
        }

        public PageSequence GetCurrentPageSequence()
        {
            return streamRenderer.GetCurrentPageSequence();
        }

        public ArrayList GetCurrentPageSequenceMarkers()
        {
            return streamRenderer.GetCurrentPageSequenceMarkers();
        }
    }
}