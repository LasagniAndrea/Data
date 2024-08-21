namespace Fonet.Fo.Flow
{
    using Fonet.DataTypes;
    using Fonet.Fo.Properties;
    using Fonet.Layout;

    internal class TableColumn : FObj
    {
        private Length columnWidthPropVal;
        private int columnWidth;
        private int columnOffset;
        private int numColumnsRepeated;
        private int iColumnNumber;
        private bool setup = false;
        private AreaContainer areaContainer;

        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new TableColumn(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        public TableColumn(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:table-column";
        }

        public Length GetColumnWidthAsLength()
        {
            return columnWidthPropVal;
        }

        public int GetColumnWidth()
        {
            return columnWidth;
        }

        public void SetColumnWidth(int columnWidth)
        {
            this.columnWidth = columnWidth;
        }

        public int GetColumnNumber()
        {
            return iColumnNumber;
        }

        public int GetNumColumnsRepeated()
        {
            return numColumnsRepeated;
        }

        public void DoSetup(Area area)
        {
            _ = propMgr.GetBorderAndPadding();
            _ = propMgr.GetBackgroundProps();

            this.iColumnNumber =
                this.properties.GetProperty("column-number").GetNumber().IntValue();

            this.numColumnsRepeated =
                this.properties.GetProperty("number-columns-repeated").GetNumber().IntValue();

            this.columnWidthPropVal =
                this.properties.GetProperty("column-width").GetLength();

            this.columnWidth = columnWidthPropVal.MValue();

            string id = this.properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);

            setup = true;
        }

        public override Status Layout(Area area)
        {
            if (this.marker == MarkerBreakAfter)
            {
                return new Status(Status.OK);
            }

            if (this.marker == MarkerStart)
            {
                if (!setup)
                {
                    DoSetup(area);
                }
            }
            if (columnWidth > 0)
            {
                this.areaContainer =
                    new AreaContainer(propMgr.GetFontState(area.GetFontInfo()),
                                      columnOffset, 0, columnWidth,
                                      area.GetContentHeight(), Position.RELATIVE);
                areaContainer.foCreator = this;
                areaContainer.SetPage(area.GetPage());
                areaContainer.SetBorderAndPadding(propMgr.GetBorderAndPadding());
                areaContainer.SetBackground(propMgr.GetBackgroundProps());
                areaContainer.SetHeight(area.GetHeight());
                area.AddChild(areaContainer);
            }
            return new Status(Status.OK);
        }

        public void SetColumnOffset(int columnOffset)
        {
            this.columnOffset = columnOffset;
        }

        public void SetHeight(int height)
        {
            if (areaContainer != null)
            {
                areaContainer.SetMaxHeight(height);
                areaContainer.SetHeight(height);
            }
        }
    }
}