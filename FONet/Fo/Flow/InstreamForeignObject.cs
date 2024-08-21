namespace Fonet.Fo.Flow
{
    using Fonet.Fo.Properties;
    using Fonet.Layout;
    using Fonet.Layout.Inline;

    internal class InstreamForeignObject : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new InstreamForeignObject(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        private int breakBefore;
        private int breakAfter;
        private int scaling;
        private int width;
        private int height;
        private int contwidth;
        private int contheight;
        private bool wauto;
        private bool hauto;
        private bool cwauto;
        private bool chauto;
        private int spaceBefore;
        private int spaceAfter;
        private int startIndent;
        private int endIndent;
        private ForeignObjectArea areaCurrent;

        public InstreamForeignObject(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:instream-foreign-object";
        }

        public override Status Layout(Area area)
        {
            if (this.marker == MarkerBreakAfter)
            {
                return new Status(Status.OK);
            }

            if (this.marker == MarkerStart)
            {
                /*
                AccessibilityProps mAccProps = propMgr.GetAccessibilityProps();
                AuralProps mAurProps = propMgr.GetAuralProps();
                BorderAndPadding bap = propMgr.GetBorderAndPadding();
                BackgroundProps bProps = propMgr.GetBackgroundProps();
                MarginInlineProps mProps = propMgr.GetMarginInlineProps();
                RelativePositionProps mRelProps = propMgr.GetRelativePositionProps();
                */
                string id = this.properties.GetProperty("id").GetString();
                int align = this.properties.GetProperty("text-align").GetEnum();
                int valign = this.properties.GetProperty("vertical-align").GetEnum();
                int overflow = this.properties.GetProperty("overflow").GetEnum();

                this.breakBefore = this.properties.GetProperty("break-before").GetEnum();
                this.breakAfter = this.properties.GetProperty("break-after").GetEnum();
                this.width = this.properties.GetProperty("width").GetLength().MValue();
                this.height = this.properties.GetProperty("height").GetLength().MValue();
                this.contwidth =
                    this.properties.GetProperty("content-width").GetLength().MValue();
                this.contheight =
                    this.properties.GetProperty("content-height").GetLength().MValue();
                this.wauto = this.properties.GetProperty("width").GetLength().IsAuto();
                this.hauto = this.properties.GetProperty("height").GetLength().IsAuto();
                this.cwauto =
                    this.properties.GetProperty("content-width").GetLength().IsAuto();
                this.chauto =
                    this.properties.GetProperty("content-height").GetLength().IsAuto();

                this.startIndent =
                    this.properties.GetProperty("start-indent").GetLength().MValue();
                this.endIndent =
                    this.properties.GetProperty("end-indent").GetLength().MValue();
                this.spaceBefore =
                    this.properties.GetProperty("space-before.optimum").GetLength().MValue();
                this.spaceAfter =
                    this.properties.GetProperty("space-after.optimum").GetLength().MValue();

                this.scaling = this.properties.GetProperty("scaling").GetEnum();

                area.GetIDReferences().CreateID(id);
                if (this.areaCurrent == null)
                {
                    this.areaCurrent =
                        new ForeignObjectArea(propMgr.GetFontState(area.GetFontInfo()),
                                              area.GetAllocationWidth());

                    this.areaCurrent.Start();
                    areaCurrent.SetWidth(this.width);
                    areaCurrent.SetHeight(this.height);
                    areaCurrent.SetContentWidth(this.contwidth);
                    areaCurrent.SetContentHeight(this.contheight);
                    areaCurrent.SetScaling(this.scaling);
                    areaCurrent.SetAlign(align);
                    areaCurrent.SetVerticalAlign(valign);
                    areaCurrent.SetOverflow(overflow);
                    areaCurrent.SetSizeAuto(wauto, hauto);
                    areaCurrent.SetContentSizeAuto(cwauto, chauto);

                    areaCurrent.SetPage(area.GetPage());

                    int numChildren = this.children.Count;
                    if (numChildren > 1)
                    {
                        throw new FonetException("Only one child element is allowed in an instream-foreign-object");
                    }
                    if (this.children.Count > 0)
                    {
                        FONode fo = (FONode)children[0];
                        Status status;
                        if ((status =
                            fo.Layout(this.areaCurrent)).IsIncomplete())
                        {
                            return status;
                        }
                        this.areaCurrent.End();
                    }
                }

                this.marker = 0;

                if (breakBefore == BreakBefore.PAGE
                    || ((spaceBefore + areaCurrent.GetEffectiveHeight())
                        > area.SpaceLeft()))
                {
                    return new Status(Status.FORCE_PAGE_BREAK);
                }

                if (breakBefore == BreakBefore.ODD_PAGE)
                {
                    return new Status(Status.FORCE_PAGE_BREAK_ODD);
                }

                if (breakBefore == BreakBefore.EVEN_PAGE)
                {
                    return new Status(Status.FORCE_PAGE_BREAK_EVEN);
                }
            }

            if (this.areaCurrent == null)
            {
                return new Status(Status.OK);
            }

            if (area is BlockArea ba)
            {
                LineArea la = ba.GetCurrentLineArea();
                if (la == null)
                {
                    return new Status(Status.AREA_FULL_NONE);
                }
                la.AddPending();
                if (areaCurrent.GetEffectiveWidth() > la.GetRemainingWidth())
                {
                    la = ba.CreateNextLineArea();
                    if (la == null)
                    {
                        return new Status(Status.AREA_FULL_NONE);
                    }
                }
                la.AddInlineArea(areaCurrent, GetLinkSet());
            }
            else
            {
                area.AddChild(areaCurrent);
                area.IncreaseHeight(areaCurrent.GetEffectiveHeight());
            }

            if (this.isInTableCell)
            {
                startIndent += forcedStartOffset;
            }

            areaCurrent.SetStartIndent(startIndent);
            areaCurrent.SetPage(area.GetPage());

            if (breakAfter == BreakAfter.PAGE)
            {
                this.marker = MarkerBreakAfter;
                return new Status(Status.FORCE_PAGE_BREAK);
            }

            if (breakAfter == BreakAfter.ODD_PAGE)
            {
                this.marker = MarkerBreakAfter;
                return new Status(Status.FORCE_PAGE_BREAK_ODD);
            }

            if (breakAfter == BreakAfter.EVEN_PAGE)
            {
                this.marker = MarkerBreakAfter;
                return new Status(Status.FORCE_PAGE_BREAK_EVEN);
            }

            areaCurrent = null;
            return new Status(Status.OK);
        }
    }
}