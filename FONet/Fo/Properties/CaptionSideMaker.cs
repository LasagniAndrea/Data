using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class CaptionSideMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new CaptionSideMaker2(propName);
        }

        protected CaptionSideMaker2(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "before", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
    // EG 20160404 Migration vs2013
    internal class CaptionSideMaker : ToBeImplementedProperty.Maker
    {
        protected static readonly EnumProperty s_propBEFORE = new EnumProperty(Constants.BEFORE);
        protected static readonly EnumProperty s_propAFTER = new EnumProperty(Constants.AFTER);
        protected static readonly EnumProperty s_propSTART = new EnumProperty(Constants.START);
        protected static readonly EnumProperty s_propEND = new EnumProperty(Constants.END);
        protected static readonly EnumProperty s_propTOP = new EnumProperty(Constants.TOP);
        protected static readonly EnumProperty s_propBOTTOM = new EnumProperty(Constants.BOTTOM);
        protected static readonly EnumProperty s_propLEFT = new EnumProperty(Constants.LEFT);
        protected static readonly EnumProperty s_propRIGHT = new EnumProperty(Constants.RIGHT);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new CaptionSideMaker(propName);
        }

        protected CaptionSideMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("before")) { return s_propBEFORE; }
            if (value.Equals("after")) { return s_propAFTER; }
            if (value.Equals("start")) { return s_propSTART; }
            if (value.Equals("end")) { return s_propEND; }
            if (value.Equals("top")) { return s_propTOP; }
            if (value.Equals("bottom")) { return s_propBOTTOM; }
            if (value.Equals("left")) { return s_propLEFT; }
            if (value.Equals("right")) { return s_propRIGHT; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

            return base.CheckEnumValues(value);
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "before", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
}