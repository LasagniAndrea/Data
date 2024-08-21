using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class WhiteSpaceTreatmentMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new WhiteSpaceTreatmentMaker2(propName);
        }

        protected WhiteSpaceTreatmentMaker2(string name) : base(name) { }

        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "preserve", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }

    // EG 20160404 Migration vs2013
    internal class WhiteSpaceTreatmentMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propIGNORE = new EnumProperty(Constants.IGNORE);
        protected static readonly EnumProperty s_propPRESERVE = new EnumProperty(Constants.PRESERVE);
        protected static readonly EnumProperty s_propIGNORE_IF_BEFORE_LINEFEED = new EnumProperty(Constants.IGNORE_IF_BEFORE_LINEFEED);
        protected static readonly EnumProperty s_propIGNORE_IF_AFTER_LINEFEED = new EnumProperty(Constants.IGNORE_IF_AFTER_LINEFEED);
        protected static readonly EnumProperty s_propIGNORE_IF_SURROUNDING_LINEFEED = new EnumProperty(Constants.IGNORE_IF_SURROUNDING_LINEFEED);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new WhiteSpaceTreatmentMaker(propName);
        }

        protected WhiteSpaceTreatmentMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("ignore")) { return s_propIGNORE; }
            if (value.Equals("preserve")) { return s_propPRESERVE; }
            if (value.Equals("ignore-if-before-linefeed")) { return s_propIGNORE_IF_BEFORE_LINEFEED; }
            if (value.Equals("ignore-if-after-linefeed")) { return s_propIGNORE_IF_AFTER_LINEFEED; }
            if (value.Equals("ignore-if-surrounding-linefeed")) { return s_propIGNORE_IF_SURROUNDING_LINEFEED; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

            return base.CheckEnumValues(value);
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "ignore-if-surrounding-linefeed", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
}