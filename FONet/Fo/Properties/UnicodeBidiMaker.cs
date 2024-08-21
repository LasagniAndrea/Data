using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class UnicodeBidiMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new UnicodeBidiMaker2(propName);
        }

        protected UnicodeBidiMaker2(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
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
    internal class UnicodeBidiMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propNORMAL = new EnumProperty(Constants.NORMAL);
        protected static readonly EnumProperty s_propEMBED = new EnumProperty(Constants.EMBED);
        protected static readonly EnumProperty s_propBIDI_OVERRIDE = new EnumProperty(Constants.BIDI_OVERRIDE);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new UnicodeBidiMaker(propName);
        }

        protected UnicodeBidiMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("normal")) { return s_propNORMAL; }
            if (value.Equals("embed")) { return s_propEMBED; }
            if (value.Equals("bidi-override")) { return s_propBIDI_OVERRIDE; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

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