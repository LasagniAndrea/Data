namespace Fonet.Fo.Flow
{
    using Fonet.DataTypes;
    using Fonet.Fo.Properties;
    using Fonet.Layout;

    internal class PageNumberCitation : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new PageNumberCitation(parent, propertyList);
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
        private Area area;
        private string pageNumber;
        private string refId;
        private string id;
        private TextState ts;

        public PageNumberCitation(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:page-number-citation";
        }

        public override Status Layout(Area area)
        {
            if (!(area is BlockArea))
            {
                FonetDriver.ActiveDriver.FireFonetWarning(
                    "Page-number-citation outside block area");
                return new Status(Status.OK);
            }

            IDReferences idReferences = area.GetIDReferences();
            this.area = area;
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

                this.refId = this.properties.GetProperty("ref-id").GetString();

                if (this.refId.Equals(""))
                {
                    throw new FonetException("page-number-citation must contain \"ref-id\"");
                }

                this.id = this.properties.GetProperty("id").GetString();
                idReferences.CreateID(id);
                ts = new TextState();

                this.marker = 0;
            }

            if (marker == 0)
            {
                idReferences.ConfigureID(id, area);
            }


            pageNumber = idReferences.GetPageNumber(refId);

            if (pageNumber != null)
            {
                this.marker =
                    FOText.AddText((BlockArea)area,
                                   propMgr.GetFontState(area.GetFontInfo()), red,
                                   green, blue, wrapOption, null,
                                   whiteSpaceCollapse, pageNumber.ToCharArray(),
                                   0, pageNumber.Length, ts,
                                   VerticalAlign.BASELINE);
            }
            else
            {
                BlockArea blockArea = (BlockArea)area;
                LineArea la = blockArea.GetCurrentLineArea();
                if (la == null)
                {
                    return new Status(Status.AREA_FULL_NONE);
                }
                la.ChangeFont(propMgr.GetFontState(area.GetFontInfo()));
                la.ChangeColor(red, green, blue);
                la.ChangeWrapOption(wrapOption);
                la.ChangeWhiteSpaceCollapse(whiteSpaceCollapse);
                la.AddPageNumberCitation(refId);
                this.marker = -1;
            }

            if (this.marker == -1)
            {
                return new Status(Status.OK);
            }
            else
            {
                return new Status(Status.AREA_FULL_NONE);
            }
        }
    }
}