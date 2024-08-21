using Fonet.Render.Pdf;

namespace Fonet.Layout
{
    internal class DisplaySpace : Space
    {
        private readonly int size;

        public DisplaySpace(int size)
        {
            this.size = size;
        }

        public int GetSize()
        {
            return size;
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderDisplaySpace(this);
        }

    }
}