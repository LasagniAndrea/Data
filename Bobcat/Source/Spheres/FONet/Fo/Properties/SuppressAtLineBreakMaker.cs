using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class SuppressAtLineBreakMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new SuppressAtLineBreakMaker2(propName);
        }

        protected SuppressAtLineBreakMaker2(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "auto", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
    // EG 20160404 Migration vs2013
    internal class SuppressAtLineBreakMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propAUTO = new EnumProperty(Constants.AUTO);
        protected static readonly EnumProperty s_propSUPPRESS = new EnumProperty(Constants.SUPPRESS);
        protected static readonly EnumProperty s_propRETAIN = new EnumProperty(Constants.RETAIN);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new SuppressAtLineBreakMaker(propName);
        }

        protected SuppressAtLineBreakMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("auto")) { return s_propAUTO; }
            if (value.Equals("suppress")) { return s_propSUPPRESS; }
            if (value.Equals("retain")) { return s_propRETAIN; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

            return base.CheckEnumValues(value);
        }


        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "auto", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
}