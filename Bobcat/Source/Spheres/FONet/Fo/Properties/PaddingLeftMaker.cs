using System.Text;

namespace Fonet.Fo.Properties
{
    internal class PaddingLeftMaker : GenericPadding
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new PaddingLeftMaker(propName);
        }

        protected PaddingLeftMaker(string name) : base(name) { }


        public override Property Compute(PropertyList propertyList)
        {
            FObj parentFO = propertyList.GetParentFObj();
            StringBuilder sbExpr = new StringBuilder();
            sbExpr.Append("padding-");
            sbExpr.Append(propertyList.WmAbsToRel(PropertyList.LEFT));

            Property p = propertyList.GetExplicitOrShorthandProperty(sbExpr.ToString());

            if (p != null)
            {
                p = ConvertProperty(p, propertyList, parentFO);
            }

            return p;
        }

    }
}