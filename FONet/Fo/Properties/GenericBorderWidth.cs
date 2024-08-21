using System.Collections;

namespace Fonet.Fo.Properties
{
    internal class GenericBorderWidth : LengthProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new GenericBorderWidth(propName);
        }

        protected GenericBorderWidth(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        public override Property GetShorthand(PropertyList propertyList)
        {
            Property p = null;
            ListProperty listprop;

            if (p == null)
            {
                listprop = (ListProperty)propertyList.GetExplicitProperty("border-width");
                if (listprop != null)
                {
                    IShorthandParser shparser = new BoxPropShorthandParser(listprop);
                    p = shparser.GetValueForProperty(PropName, this, propertyList);
                }
            }

            return p;
        }

        private static Hashtable s_htKeywords;

        private static void InitKeywords()
        {
            s_htKeywords = new Hashtable(4)
            {
                // EG 20160404 Migration vs2013
                { "none", "0" },
                { "thin", "0.5pt" },
                { "medium", "1pt" },
                { "thick", "2pt" }
            };
        }

        protected override string CheckValueKeywords(string keyword)
        {
            if (s_htKeywords == null)
            {
                InitKeywords();
            }
            string value = (string)s_htKeywords[keyword];
            if (value == null)
            {
                return base.CheckValueKeywords(keyword);
            }
            else
            {
                return value;
            }
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "0pt", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }
    }
}