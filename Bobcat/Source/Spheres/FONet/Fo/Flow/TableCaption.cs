using Fonet.Layout;

namespace Fonet.Fo.Flow
{
    internal class TableCaption : ToBeImplementedElement
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new TableCaption(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        protected TableCaption(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:table-caption";
        }

        public override Status Layout(Area area)
        {
            _ = propMgr.GetAccessibilityProps();
            _ = propMgr.GetAuralProps();
            _ = propMgr.GetBorderAndPadding();
            _ = propMgr.GetBackgroundProps();
            _ = propMgr.GetRelativePositionProps();
            return base.Layout(area);
        }
    }
}