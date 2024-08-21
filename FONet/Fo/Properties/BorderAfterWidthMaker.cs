using System.Text;

namespace Fonet.Fo.Properties
{
    internal class BorderAfterWidthMaker : GenericCondBorderWidth
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new BorderAfterWidthMaker(propName);
        }

        protected BorderAfterWidthMaker(string name) : base(name) { }


        public override bool IsCorrespondingForced(PropertyList propertyList)
        {
            StringBuilder sbExpr = new StringBuilder
            {
                Length = 0
            };
            sbExpr.Append("border-");
            sbExpr.Append(propertyList.WmRelToAbs(PropertyList.AFTER));
            sbExpr.Append("-width");
            if (propertyList.GetExplicitProperty(sbExpr.ToString()) != null)
            {
                return true;
            }

            return false;
        }

        public override Property Compute(PropertyList propertyList)
        {
            FObj parentFO = propertyList.GetParentFObj();
            StringBuilder sbExpr = new StringBuilder();
            sbExpr.Append("border-");
            sbExpr.Append(propertyList.WmRelToAbs(PropertyList.AFTER));
            sbExpr.Append("-width");
            Property p = propertyList.GetExplicitOrShorthandProperty(sbExpr.ToString());

            if (p != null)
            {
                p = ConvertProperty(p, propertyList, parentFO);
            }

            return p;
        }

        protected override string GetDefaultForConditionality()
        {
            return "retain";
        }

    }
}