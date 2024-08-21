using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class DirectionMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new DirectionMaker2(propName);
        }

        protected DirectionMaker2(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "ltr", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
    // EG 20160404 Migration vs2013
    internal class DirectionMaker : ToBeImplementedProperty.Maker
    {
        protected static readonly EnumProperty s_propLTR= new EnumProperty(Constants.LTR);
        protected static readonly EnumProperty s_propRTL = new EnumProperty(Constants.RTL);
        new public static PropertyMaker Maker(string propName)
        {
            return new DirectionMaker(propName);
        }

        protected DirectionMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("ltr")) { return s_propLTR; }
            if (value.Equals("rtl")) { return s_propRTL; }

            return base.CheckEnumValues(value);
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "ltr", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
}