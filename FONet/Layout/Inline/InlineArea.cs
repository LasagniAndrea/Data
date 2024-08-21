namespace Fonet.Layout.Inline
{
    internal abstract class InlineArea : Area
    {
        private int yOffset = 0;
        private int xOffset = 0;
        protected int height = 0;
        private int verticalAlign = 0;
        protected string pageNumberId = null;
        private readonly float red, green, blue;
        protected bool underlined = false;
        protected bool overlined = false;
        protected bool lineThrough = false;

        public InlineArea(
            FontState fontState, int width, float red,
            float green, float blue)
            : base(fontState)
        {
            this.contentRectangleWidth = width;
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public float GetBlue()
        {
            return this.blue;
        }

        public float GetGreen()
        {
            return this.green;
        }

        public float GetRed()
        {
            return this.red;
        }

        public override void SetHeight(int height)
        {
            this.height = height;
        }

        public override int GetHeight()
        {
            return this.height;
        }

        public virtual void SetVerticalAlign(int align)
        {
            this.verticalAlign = align;
        }

        public virtual int GetVerticalAlign()
        {
            return this.verticalAlign;
        }

        public void SetYOffset(int yOffset)
        {
            this.yOffset = yOffset;
        }

        public int GetYOffset()
        {
            return this.yOffset;
        }

        public void SetXOffset(int xOffset)
        {
            this.xOffset = xOffset;
        }

        public virtual int GetXOffset()
        {
            return this.xOffset;
        }

        public string GetPageNumberID()
        {
            return pageNumberId;
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

    }
}