namespace Fonet.Fo.Flow
{
    using Fonet.DataTypes;
    using Fonet.Fo.Properties;
    using Fonet.Layout;

    internal class PageNumber : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new PageNumber(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        private float red;
        private float green;
        private float blue;
        private int wrapOption;
        private int whiteSpaceCollapse;
        private TextState ts;

        public PageNumber(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:page-number";
        }

        public override Status Layout(Area area)
        {
            if (!(area is BlockArea))
            {
                FonetDriver.ActiveDriver.FireFonetWarning(
                    "Page-number outside block area");
                return new Status(Status.OK);
            }
            if (this.marker == MarkerStart)
            {
                _ = propMgr.GetAccessibilityProps();
                _ = propMgr.GetAuralProps();
                _ = propMgr.GetBorderAndPadding();
                _ = propMgr.GetBackgroundProps();
                _ = propMgr.GetMarginInlineProps();
                _ = propMgr.GetRelativePositionProps();

                ColorType c = this.properties.GetProperty("color").GetColorType();
                this.red = c.Red;
                this.green = c.Green;
                this.blue = c.Blue;

                this.wrapOption = this.properties.GetProperty("wrap-option").GetEnum();
                this.whiteSpaceCollapse =
                    this.properties.GetProperty("white-space-collapse").GetEnum();
                ts = new TextState();
                this.marker = 0;

                string id = this.properties.GetProperty("id").GetString();
                area.GetIDReferences().InitializeID(id, area);
            }

            string p = area.GetPage().GetFormattedNumber();
            this.marker = FOText.AddText((BlockArea)area,
                                         propMgr.GetFontState(area.GetFontInfo()),
                                         red, green, blue, wrapOption, null,
                                         whiteSpaceCollapse, p.ToCharArray(), 0,
                                         p.Length, ts, VerticalAlign.BASELINE);
            return new Status(Status.OK);
        }
    }
}