namespace Fonet.Fo.Flow
{
    using Fonet.Layout;

    internal class TableAndCaption : ToBeImplementedElement
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new TableAndCaption(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        protected TableAndCaption(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:table-and-caption";
        }

        public override Status Layout(Area area)
        {
            _ = propMgr.GetAccessibilityProps();
            _ = propMgr.GetAuralProps();
            _ = propMgr.GetBorderAndPadding();
            _ = propMgr.GetBackgroundProps();
            _ = propMgr.GetMarginProps();
            _ = propMgr.GetRelativePositionProps();
            return base.Layout(area);
        }
    }
}