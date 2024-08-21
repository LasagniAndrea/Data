namespace Fonet.Layout.Inline
{
    using Fonet.Render.Pdf;

    internal class ForeignObjectArea : InlineArea
    {
        protected int xOffset = 0;
        protected int align;
        protected int valign;
        protected int scaling;
        protected Area foreignObject;
        protected int cheight;
        protected int cwidth;
        protected int awidth;
        protected int aheight;
        protected int width;
        private bool wauto;
        private bool hauto;
        private bool cwauto;
        private bool chauto;
        private int overflow;

        public ForeignObjectArea(FontState fontState, int width)
            : base(fontState, width, 0, 0, 0)
        {
        }

        public override void Render(PdfRenderer renderer)
        {
            if (foreignObject != null)
            {
                renderer.RenderForeignObjectArea(this);
            }
        }

        public override int GetContentWidth()
        {
            return GetEffectiveWidth();
        }

        public override int GetHeight()
        {
            return GetEffectiveHeight();
        }

        public override int GetContentHeight()
        {
            return GetEffectiveHeight();
        }

        public override int GetXOffset()
        {
            return this.xOffset;
        }

        public void SetStartIndent(int startIndent)
        {
            xOffset = startIndent;
        }

        public void SetObject(Area fobject)
        {
            foreignObject = fobject;
        }

        public Area GetObject()
        {
            return foreignObject;
        }

        public void SetSizeAuto(bool wa, bool ha)
        {
            wauto = wa;
            hauto = ha;
        }

        public void SetContentSizeAuto(bool wa, bool ha)
        {
            cwauto = wa;
            chauto = ha;
        }

        public bool IsContentWidthAuto()
        {
            return cwauto;
        }

        public bool IsContentHeightAuto()
        {
            return chauto;
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

        public void SetOverflow(int o)
        {
            this.overflow = o;
        }

        public int GetOverflow()
        {
            return this.overflow;
        }

        public override void SetHeight(int height)
        {
            this.height = height;
        }

        public void SetWidth(int width)
        {
            this.width = width;
        }

        public void SetContentHeight(int cheight)
        {
            this.cheight = cheight;
        }

        public void SetContentWidth(int cwidth)
        {
            this.cwidth = cwidth;
        }

        public void SetScaling(int scaling)
        {
            this.scaling = scaling;
        }

        public int ScalingMethod()
        {
            return this.scaling;
        }

        public void SetIntrinsicWidth(int w)
        {
            awidth = w;
        }

        public void SetIntrinsicHeight(int h)
        {
            aheight = h;
        }

        public int GetIntrinsicHeight()
        {
            return aheight;
        }

        public int GetIntrinsicWidth()
        {
            return awidth;
        }

        public int GetEffectiveHeight()
        {
            if (this.hauto)
            {
                if (this.chauto)
                {
                    return aheight;
                }
                else
                {
                    return this.cheight;
                }
            }
            else
            {
                return this.height;
            }
        }

        public int GetEffectiveWidth()
        {
            if (this.wauto)
            {
                if (this.cwauto)
                {
                    return awidth;
                }
                else
                {
                    return this.cwidth;
                }
            }
            else
            {
                return this.width;
            }
        }

    }
}