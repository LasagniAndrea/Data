namespace Fonet.Fo.Flow
{
    using Fonet.Layout;

    internal class FootnoteBody : FObj
    {
        private readonly int align = 0;

        private readonly int alignLast = 0;

        private readonly int lineHeight = 0;

        private readonly int startIndent = 0;

        private readonly int endIndent = 0;

        private readonly int textIndent = 0;

        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new FootnoteBody(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        public FootnoteBody(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:footnote-body";
            this.areaClass = AreaClass.SetAreaClass(AreaClass.XSL_FOOTNOTE);
        }

        public override Status Layout(Area area)
        {
            if (this.marker == MarkerStart)
            {
                this.marker = 0;
            }
            BlockArea blockArea =
                new BlockArea(propMgr.GetFontState(area.GetFontInfo()),
                              area.GetAllocationWidth(), area.SpaceLeft(),
                              startIndent, endIndent, textIndent, align,
                              alignLast, lineHeight);
            blockArea.SetGeneratedBy(this);
            blockArea.IsFirst(true);
            blockArea.SetParent(area);
            blockArea.SetPage(area.GetPage());
            blockArea.Start();

            blockArea.SetAbsoluteHeight(area.GetAbsoluteHeight());
            blockArea.SetIDReferences(area.GetIDReferences());

            blockArea.SetTableCellXOffset(area.GetTableCellXOffset());

            int numChildren = this.children.Count;
            for (int i = this.marker; i < numChildren; i++)
            {
                FONode fo = (FONode)children[i];
                Status status;
                if ((status = fo.Layout(blockArea)).IsIncomplete())
                {
                    this.ResetMarker();
                    return status;
                }
            }
            blockArea.End();
            area.AddChild(blockArea);
            area.IncreaseHeight(blockArea.GetHeight());
            blockArea.IsLast(true);
            return new Status(Status.OK);
        }
    }
}