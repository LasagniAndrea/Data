using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class WhiteSpaceMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new WhiteSpaceMaker2(propName);
        }

        protected WhiteSpaceMaker2(string name) : base(name) { }

        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "normal", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
    // EG 20160404 Migration vs2013
    internal class WhiteSpaceMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propNORMAL = new EnumProperty(Constants.NORMAL);
        protected static readonly EnumProperty s_propPRE = new EnumProperty(Constants.PRE);
        protected static readonly EnumProperty s_propNOWRAP = new EnumProperty(Constants.NOWRAP);

        new public static PropertyMaker Maker(string propName)
        {
            return new WhiteSpaceMaker(propName);
        }

        protected WhiteSpaceMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("normal")) { return s_propNORMAL;}
            if (value.Equals("pre")) { return s_propPRE; }
            if (value.Equals("nowrap")) { return s_propNOWRAP; }

            return base.CheckEnumValues(value);
        }


        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "normal", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }

}