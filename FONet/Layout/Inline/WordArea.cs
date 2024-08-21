using Fonet.Render.Pdf;

namespace Fonet.Layout.Inline
{
    internal class WordArea : InlineArea
    {
        private readonly string text;

        public WordArea(
            FontState fontState, float red, float green,
            float blue, string text, int width)
            : base(fontState, width, red, green, blue)
        {
            this.text = text;
            this.contentRectangleWidth = width;
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderWordArea(this);
        }

        public string GetText()
        {
            return this.text;
        }

    }
}