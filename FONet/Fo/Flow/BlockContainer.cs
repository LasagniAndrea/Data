namespace Fonet.Fo.Flow
{
    using Fonet.Fo.Properties;
    using Fonet.Layout;

    internal class BlockContainer : FObj
    {
        private int position;
        private int top;
        private int bottom;
        private int left;
        private int right;
        private int width;
        private int height;
        private int span;
        private AreaContainer areaContainer;

        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new BlockContainer(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        protected BlockContainer(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:block-container";
            this.span = this.properties.GetProperty("span").GetEnum();
        }

        public override Status Layout(Area area)
        {
            if (this.marker == MarkerStart)
            {
                _ = propMgr.GetAbsolutePositionProps();
                _ = propMgr.GetBorderAndPadding();
                _ = propMgr.GetBackgroundProps();
                _ = propMgr.GetMarginProps();

                this.marker = 0;
                this.position = this.properties.GetProperty("position").GetEnum();
                this.top = this.properties.GetProperty("top").GetLength().MValue();
                this.bottom = this.properties.GetProperty("bottom").GetLength().MValue();
                this.left = this.properties.GetProperty("left").GetLength().MValue();
                this.right = this.properties.GetProperty("right").GetLength().MValue();
                this.width = this.properties.GetProperty("width").GetLength().MValue();
                this.height = this.properties.GetProperty("height").GetLength().MValue();
                span = this.properties.GetProperty("span").GetEnum();

                string id = this.properties.GetProperty("id").GetString();
                area.GetIDReferences().InitializeID(id, area);
            }

            AreaContainer container = (AreaContainer)area;
            if ((this.width == 0) && (this.height == 0))
            {
                width = right - left;
                height = bottom - top;
            }

            this.areaContainer =
                new AreaContainer(propMgr.GetFontState(container.GetFontInfo()),
                                  container.GetXPosition() + left,
                                  container.GetYPosition() - top, width, height,
                                  position);

            areaContainer.SetPage(area.GetPage());
            areaContainer.SetBackground(propMgr.GetBackgroundProps());
            areaContainer.SetBorderAndPadding(propMgr.GetBorderAndPadding());
            areaContainer.Start();

            areaContainer.SetAbsoluteHeight(0);
            areaContainer.SetIDReferences(area.GetIDReferences());

            int numChildren = this.children.Count;
            for (int i = this.marker; i < numChildren; i++)
            {
                FObj fo = (FObj)children[i];
                _ = fo.Layout(areaContainer);
            }

            areaContainer.End();
            if (position == Position.ABSOLUTE)
            {
                areaContainer.SetHeight(height);
            }
            area.AddChild(areaContainer);

            return new Status(Status.OK);
        }

        public override int GetContentWidth()
        {
            if (areaContainer != null)
            {
                return areaContainer.GetContentWidth();
            }
            else
            {
                return 0;
            }
        }

        public override bool GeneratesReferenceAreas()
        {
            return true;
        }

        public int GetSpan()
        {
            return this.span;
        }
    }
}