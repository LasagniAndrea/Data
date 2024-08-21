using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class LinefeedTreatmentMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new LinefeedTreatmentMaker2(propName);
        }

        protected LinefeedTreatmentMaker2(string name) : base(name) { }

        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "treat-as-space", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
    // EG 20160404 Migration vs2013
    internal class LinefeedTreatmentMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propIGNORE = new EnumProperty(Constants.IGNORE);
        protected static readonly EnumProperty s_propPRESERVE = new EnumProperty(Constants.PRESERVE);
        protected static readonly EnumProperty s_propTREAT_AS_SPACE = new EnumProperty(Constants.TREAT_AS_SPACE);
        protected static readonly EnumProperty s_propTREAT_AS_ZERO_WIDTH_SPACE = new EnumProperty(Constants.TREAT_AS_ZERO_WIDTH_SPACE);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new LinefeedTreatmentMaker(propName);
        }

        protected LinefeedTreatmentMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("ignore")) { return s_propIGNORE; }
            if (value.Equals("preserve")) { return s_propPRESERVE; }
            if (value.Equals("treat-as-space")) { return s_propTREAT_AS_SPACE; }
            if (value.Equals("treat-as-zero-width-space")) { return s_propTREAT_AS_ZERO_WIDTH_SPACE; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

            return base.CheckEnumValues(value);
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "treat-as-space", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
}