namespace Fonet.Fo.Flow
{
    using Fonet.Layout;

    internal class Inline : FObjMixed
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new Inline(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        public Inline(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:inline";
            if (parent.GetName().Equals("fo:flow"))
            {
                throw new FonetException("inline formatting objects cannot"
                    + " be directly under flow");
            }
            /*
            AccessibilityProps mAccProps = propMgr.GetAccessibilityProps();
            AuralProps mAurProps = propMgr.GetAuralProps();
            BorderAndPadding bap = propMgr.GetBorderAndPadding();
            BackgroundProps bProps = propMgr.GetBackgroundProps();
            MarginInlineProps mProps = propMgr.GetMarginInlineProps();
            RelativePositionProps mRelProps = propMgr.GetRelativePositionProps();
            */
            ts = propMgr.GetTextDecoration(parent);
        }

        protected internal override void AddCharacters(char[] data, int start, int length)
        {
            FOText ft = new FOText(data, start, length, this);
            ft.SetUnderlined(ts.GetUnderlined());
            ft.SetOverlined(ts.GetOverlined());
            ft.SetLineThrough(ts.GetLineThrough());
            children.Add(ft);
        }
    }
}