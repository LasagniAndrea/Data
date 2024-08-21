using Fonet.Render.Pdf;

namespace Fonet.Layout.Inline
{
    internal class InlineSpace : Space
    {
        private int size;
        private bool resizeable = true;
        private bool eatable = false;
        protected bool underlined = false;
        protected bool overlined = false;
        protected bool lineThrough = false;

        public InlineSpace(int amount)
        {
            this.size = amount;
        }

        public InlineSpace(int amount, bool resizeable)
        {
            this.resizeable = resizeable;
            this.size = amount;
        }

        public void SetUnderlined(bool ul)
        {
            this.underlined = ul;
        }

        public bool GetUnderlined()
        {
            return this.underlined;
        }

        public void SetOverlined(bool ol)
        {
            this.overlined = ol;
        }

        public bool GetOverlined()
        {
            return this.overlined;
        }

        public void SetLineThrough(bool lt)
        {
            this.lineThrough = lt;
        }

        public bool GetLineThrough()
        {
            return this.lineThrough;
        }

        public int GetSize()
        {
            return size;
        }

        public void SetSize(int amount)
        {
            this.size = amount;
        }

        public bool GetResizeable()
        {
            return resizeable;
        }

        public void SetResizeable(bool resizeable)
        {
            this.resizeable = resizeable;
        }

        public void SetEatable(bool eatable)
        {
            this.eatable = eatable;
        }

        public bool IsEatable()
        {
            return eatable;
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderInlineSpace(this);
        }

    }
}