using System.Text;

namespace Fonet.Fo.Properties
{
    internal class BorderLeftWidthMaker : GenericBorderWidth
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new BorderLeftWidthMaker(propName);
        }

        protected BorderLeftWidthMaker(string name) : base(name) { }


        public override Property Compute(PropertyList propertyList)
        {
            FObj parentFO = propertyList.GetParentFObj();
            StringBuilder sbExpr = new StringBuilder();
            sbExpr.Append("border-");
            sbExpr.Append(propertyList.WmAbsToRel(PropertyList.LEFT));
            sbExpr.Append("-width");
            Property p = propertyList.GetExplicitOrShorthandProperty(sbExpr.ToString());

            if (p != null)
            {
                p = ConvertProperty(p, propertyList, parentFO);
            }

            return p;
        }

        public override Property GetShorthand(PropertyList propertyList)
        {
            Property p = null;
            ListProperty listprop;

            if (p == null)
            {
                listprop = (ListProperty)propertyList.GetExplicitProperty("border-left");
                if (listprop != null)
                {
                    // Get a parser for the shorthand to set the individual properties
                    IShorthandParser shparser = new GenericShorthandParser(listprop);
                    p = shparser.GetValueForProperty(PropName, this, propertyList);
                }
            }

            if (p == null)
            {
                listprop = (ListProperty)propertyList.GetExplicitProperty("border-width");
                if (listprop != null)
                {
                    // Get a parser for the shorthand to set the individual properties
                    IShorthandParser shparser = new BoxPropShorthandParser(listprop);
                    p = shparser.GetValueForProperty(PropName, this, propertyList);
                }
            }

            if (p == null)
            {
                listprop = (ListProperty)propertyList.GetExplicitProperty("border");
                if (listprop != null)
                {
                    // Get a parser for the shorthand to set the individual properties
                    IShorthandParser shparser = new GenericShorthandParser(listprop);
                    p = shparser.GetValueForProperty(PropName, this, propertyList);
                }
            }

            return p;
        }

    }
}