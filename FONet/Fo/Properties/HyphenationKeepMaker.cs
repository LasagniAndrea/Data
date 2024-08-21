using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class HyphenationKeepMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new HyphenationKeepMaker2(propName);
        }

        protected HyphenationKeepMaker2(string name) : base(name) { }


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
    internal class HyphenationKeepMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propAUTO = new EnumProperty(Constants.AUTO);
        protected static readonly EnumProperty s_propCOLUMN = new EnumProperty(Constants.COLUMN);
        protected static readonly EnumProperty s_propPAGE = new EnumProperty(Constants.PAGE);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new HyphenationKeepMaker(propName);
        }

        protected HyphenationKeepMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("auto")) { return s_propAUTO; }
            if (value.Equals("column")) { return s_propCOLUMN; }
            if (value.Equals("page")) { return s_propPAGE; }
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