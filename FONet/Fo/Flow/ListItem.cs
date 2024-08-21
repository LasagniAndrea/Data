namespace Fonet.Fo.Flow
{
    using Fonet.Layout;

    internal class ListItem : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new ListItem(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        private int align;
        private int alignLast;
        private int lineHeight;
        private int spaceBefore;
        private int spaceAfter;
        private string id;
        private BlockArea blockArea;

        public ListItem(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:list-item";
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
                this.spaceBefore =
                    this.properties.GetProperty("space-before.optimum").GetLength().MValue();
                this.spaceAfter =
                    this.properties.GetProperty("space-after.optimum").GetLength().MValue();
                this.id = this.properties.GetProperty("id").GetString();

                area.GetIDReferences().CreateID(id);

                this.marker = 0;
            }

            if (area is BlockArea)
            {
                area.End();
            }

            if (spaceBefore != 0)
            {
                area.AddDisplaySpace(spaceBefore);
            }

            this.blockArea =
                new BlockArea(propMgr.GetFontState(area.GetFontInfo()),
                              area.GetAllocationWidth(), area.SpaceLeft(), 0, 0,
                              0, align, alignLast, lineHeight);
            this.blockArea.SetTableCellXOffset(area.GetTableCellXOffset());
            this.blockArea.SetGeneratedBy(this);
            this.areasGenerated++;
            if (this.areasGenerated == 1)
            {
                this.blockArea.IsFirst(true);
            }
            this.blockArea.AddLineagePair(this, this.areasGenerated);

            blockArea.SetParent(area);
            blockArea.SetPage(area.GetPage());
            blockArea.Start();

            blockArea.SetAbsoluteHeight(area.GetAbsoluteHeight());
            blockArea.SetIDReferences(area.GetIDReferences());

            int numChildren = this.children.Count;
            if (numChildren != 2)
            {
                throw new FonetException("list-item must have exactly two children");
            }
            ListItemLabel label = (ListItemLabel)children[0];
            ListItemBody body = (ListItemBody)children[1];

            Status status;

            if (this.marker == 0)
            {
                area.GetIDReferences().ConfigureID(id, area);

                status = label.Layout(blockArea);
                if (status.IsIncomplete())
                {
                    return status;
                }
            }

            status = body.Layout(blockArea);
            if (status.IsIncomplete())
            {
                blockArea.End();
                area.AddChild(blockArea);
                area.IncreaseHeight(blockArea.GetHeight());
                this.marker = 1;
                return status;
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
            this.blockArea.IsLast(true);
            return new Status(Status.OK);
        }

        public override int GetContentWidth()
        {
            if (blockArea != null)
            {
                return blockArea.GetContentWidth();
            }
            else
            {
                return 0;
            }
        }
    }
}