namespace Fonet.Fo.Flow
{
    using Fonet.Layout;

    internal class ListBlock : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new ListBlock(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        private int align;
        private int alignLast;
        private int lineHeight;
        private int startIndent;
        private int endIndent;
        private int spaceBefore;
        private int spaceAfter;

        public ListBlock(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:list-block";
        }

        public override Status Layout(Area area)
        {
            if (this.marker == MarkerStart)
            {
                /*
                AccessibilityProps mAccProps = propMgr.GetAccessibilityProps();
                AuralProps mAurProps = propMgr.GetAuralProps();
                BorderAndPadding bap = propMgr.GetBorderAndPadding();
                BackgroundProps bProps = propMgr.GetBackgroundProps();
                MarginProps mProps = propMgr.GetMarginProps();
                RelativePositionProps mRelProps = propMgr.GetRelativePositionProps();
                */

                this.align = this.properties.GetProperty("text-align").GetEnum();
                this.alignLast = this.properties.GetProperty("text-align-last").GetEnum();
                this.lineHeight =
                    this.properties.GetProperty("line-height").GetLength().MValue();
                this.startIndent =
                    this.properties.GetProperty("start-indent").GetLength().MValue();
                this.endIndent =
                    this.properties.GetProperty("end-indent").GetLength().MValue();
                this.spaceBefore =
                    this.properties.GetProperty("space-before.optimum").GetLength().MValue();
                this.spaceAfter =
                    this.properties.GetProperty("space-after.optimum").GetLength().MValue();

                this.marker = 0;

                if (area is BlockArea)
                {
                    area.End();
                }

                if (spaceBefore != 0)
                {
                    area.AddDisplaySpace(spaceBefore);
                }

                if (this.isInTableCell)
                {
                    startIndent += forcedStartOffset;
                    endIndent += area.GetAllocationWidth() - forcedWidth
                        - forcedStartOffset;
                }

                string id = this.properties.GetProperty("id").GetString();
                area.GetIDReferences().InitializeID(id, area);
            }

            BlockArea blockArea =
                new BlockArea(propMgr.GetFontState(area.GetFontInfo()),
                              area.GetAllocationWidth(), area.SpaceLeft(),
                              startIndent, endIndent, 0, align, alignLast,
                              lineHeight);
            blockArea.SetTableCellXOffset(area.GetTableCellXOffset());
            blockArea.SetGeneratedBy(this);
            this.areasGenerated++;
            if (this.areasGenerated == 1)
            {
                blockArea.IsFirst(true);
            }
            blockArea.AddLineagePair(this, this.areasGenerated);

            blockArea.SetParent(area);
            blockArea.SetPage(area.GetPage());
            blockArea.SetBackground(propMgr.GetBackgroundProps());
            blockArea.Start();

            blockArea.SetAbsoluteHeight(area.GetAbsoluteHeight());
            blockArea.SetIDReferences(area.GetIDReferences());

            int numChildren = this.children.Count;
            for (int i = this.marker; i < numChildren; i++)
            {
                if (!(children[i] is ListItem))
                {
                    FonetDriver.ActiveDriver.FireFonetError(
                        "Children of list-blocks must be list-items");
                    return new Status(Status.OK);
                }
                ListItem listItem = (ListItem)children[i];
                Status status;
                if ((status = listItem.Layout(blockArea)).IsIncomplete())
                {
                    if (status.GetCode() == Status.AREA_FULL_NONE && i > 0)
                    {
                        status = new Status(Status.AREA_FULL_SOME);
                    }
                    this.marker = i;
                    blockArea.End();
                    area.AddChild(blockArea);
                    area.IncreaseHeight(blockArea.GetHeight());
                    return status;
                }
            }

            blockArea.End();
            area.AddChild(blockArea);
            area.IncreaseHeight(blockArea.GetHeight());

            if (spaceAfter != 0)
            {
                area.AddDisplaySpace(spaceAfter);
            }

            if (area is BlockArea)
            {
                area.Start();
            }

            blockArea.IsLast(true);
            return new Status(Status.OK);
        }
    }
}