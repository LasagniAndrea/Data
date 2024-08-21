namespace Fonet.Image
{
    using Fonet.Layout;
    using Fonet.Layout.Inline;
    using Fonet.Render.Pdf;

    internal class ImageArea : InlineArea
    {
        protected int xOffset = 0;
        protected int align;
        protected int valign;
        protected FonetImage image;

        public ImageArea(FontState fontState, FonetImage img, int width, int height, int align)
            : base(fontState, width, 0, 0, 0)
        {
            this.currentHeight = height;
            this.contentRectangleWidth = width;
            this.height = height;
            this.image = img;
            this.align = align;
        }

        public override int GetXOffset()
        {
            return this.xOffset;
        }

        public FonetImage GetImage()
        {
            return this.image;
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderImageArea(this);
        }

        public int GetImageHeight()
        {
            return currentHeight;
        }

        public void SetAlign(int align)
        {
            this.align = align;
        }

        public int GetAlign()
        {
            return this.align;
        }

        public override void SetVerticalAlign(int align)
        {
            this.valign = align;
        }

        public override int GetVerticalAlign()
        {
            return this.valign;
        }

        public void SetStartIndent(int startIndent)
        {
            xOffset = startIndent;
        }

    }
}