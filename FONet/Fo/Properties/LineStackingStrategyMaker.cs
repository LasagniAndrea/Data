using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class LineStackingStrategyMaker1 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new LineStackingStrategyMaker1(propName);
        }

        protected LineStackingStrategyMaker1(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "line-height", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }
    }
    // EG 20160404 Migration vs2013
    internal class LineStackingStrategyMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propLINE_HEIGHT = new EnumProperty(Constants.LINE_HEIGHT);
        protected static readonly EnumProperty s_propFONT_HEIGHT = new EnumProperty(Constants.FONT_HEIGHT);
        protected static readonly EnumProperty s_propMAX_HEIGHT = new EnumProperty(Constants.MAX_HEIGHT);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);
        new public static PropertyMaker Maker(string propName)
        {
            return new LineStackingStrategyMaker(propName);
        }

        protected LineStackingStrategyMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("line-height")) { return s_propLINE_HEIGHT; }
            if (value.Equals("font-height")) { return s_propFONT_HEIGHT; }
            if (value.Equals("max-height")) { return s_propMAX_HEIGHT; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

            return base.CheckEnumValues(value);
        }


        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "line-height", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }
    }
}