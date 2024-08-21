using Fonet.Fo.Properties;
using Fonet.Layout;

namespace Fonet.Fo.Pagination
{
    internal class RegionBefore : Region
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new RegionBefore(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        public const string REGION_CLASS = "before";

        private readonly int precedence;

        protected RegionBefore(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            precedence = this.properties.GetProperty("precedence").GetEnum();
        }


        public override RegionArea MakeRegionArea(int allocationRectangleXPosition,
                                                  int allocationRectangleYPosition,
                                                  int allocationRectangleWidth,
                                                  int allocationRectangleHeight)
        {
            _ = propMgr.GetBorderAndPadding();
            BackgroundProps bProps = propMgr.GetBackgroundProps();
            int extent = this.properties.GetProperty("extent").GetLength().MValue();

            RegionArea area = new RegionArea(
                allocationRectangleXPosition,
                allocationRectangleYPosition,
                allocationRectangleWidth,
                extent);
            area.SetBackground(bProps);

            return area;
        }


        protected override string GetDefaultRegionName()
        {
            return "xsl-region-before";
        }

        protected override string GetElementName()
        {
            return "fo:region-before";
        }

        public override string GetRegionClass()
        {
            return REGION_CLASS;
        }

        public bool GetPrecedence()
        {
            return (precedence == Precedence.TRUE);
        }

    }
}