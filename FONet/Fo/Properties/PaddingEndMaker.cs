using System.Text;

namespace Fonet.Fo.Properties
{
    internal class PaddingEndMaker : GenericCondPadding
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new PaddingEndMaker(propName);
        }

        protected PaddingEndMaker(string name) : base(name) { }


        public override bool IsCorrespondingForced(PropertyList propertyList)
        {
            StringBuilder sbExpr = new StringBuilder
            {
                Length = 0
            };
            sbExpr.Append("padding-");
            sbExpr.Append(propertyList.WmRelToAbs(PropertyList.END));

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
            sbExpr.Append("padding-");
            sbExpr.Append(propertyList.WmRelToAbs(PropertyList.END));

            Property p = propertyList.GetExplicitOrShorthandProperty(sbExpr.ToString());

            if (p != null)
            {
                p = ConvertProperty(p, propertyList, parentFO);
            }

            return p;
        }

        protected override string GetDefaultForConditionality()
        {
            return "discard";
        }

    }
}