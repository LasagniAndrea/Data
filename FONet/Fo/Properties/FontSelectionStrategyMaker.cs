using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class FontSelectionStrategyMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new FontSelectionStrategyMaker2(propName);
        }

        protected FontSelectionStrategyMaker2(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
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
    internal class FontSelectionStrategyMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propAUTO = new EnumProperty(Constants.AUTO);
        protected static readonly EnumProperty s_propCHARACTER_BY_CHARACTER = new EnumProperty(Constants.CHARACTER_BY_CHARACTER);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new FontSelectionStrategyMaker(propName);
        }

        protected FontSelectionStrategyMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("auto")) { return s_propAUTO; }
            if (value.Equals("character-by-character")) { return s_propCHARACTER_BY_CHARACTER; }
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