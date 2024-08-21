using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class DominantBaselineMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new DominantBaselineMaker2(propName);
        }

        protected DominantBaselineMaker2(string name) : base(name) { }


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
    internal class DominantBaselineMaker : ToBeImplementedProperty.Maker
    {
        protected static readonly EnumProperty s_propAUTO = new EnumProperty(Constants.AUTO);
        protected static readonly EnumProperty s_propUSE_SCRIPT = new EnumProperty(Constants.USE_SCRIPT);
        protected static readonly EnumProperty s_propNO_CHANGE = new EnumProperty(Constants.NO_CHANGE);
        protected static readonly EnumProperty s_propRESET_SIZE = new EnumProperty(Constants.RESET_SIZE);
        protected static readonly EnumProperty s_propIDEOGRAPHIC = new EnumProperty(Constants.IDEOGRAPHIC);
        protected static readonly EnumProperty s_propALPHABETIC = new EnumProperty(Constants.ALPHABETIC);
        protected static readonly EnumProperty s_propHANGING = new EnumProperty(Constants.HANGING);
        protected static readonly EnumProperty s_propMATHEMATICAL = new EnumProperty(Constants.MATHEMATICAL);
        protected static readonly EnumProperty s_propCENTRAL = new EnumProperty(Constants.CENTRAL);
        protected static readonly EnumProperty s_propMIDDLE = new EnumProperty(Constants.MIDDLE);
        protected static readonly EnumProperty s_propTEXT_AFTER_EDGE = new EnumProperty(Constants.TEXT_AFTER_EDGE);
        protected static readonly EnumProperty s_propTEXT_BEFORE_EDGE = new EnumProperty(Constants.TEXT_BEFORE_EDGE);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new DominantBaselineMaker(propName);
        }

        protected DominantBaselineMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("auto")) { return s_propAUTO; }
            if (value.Equals("use-script")) { return s_propUSE_SCRIPT; }
            if (value.Equals("no-change")) { return s_propNO_CHANGE; }
            if (value.Equals("reset-size")) { return s_propRESET_SIZE; }
            if (value.Equals("ideographic")) { return s_propIDEOGRAPHIC; }
            if (value.Equals("alphabetic")) { return s_propALPHABETIC; }
            if (value.Equals("hanging")) { return s_propHANGING; }
            if (value.Equals("mathematical")) { return s_propMATHEMATICAL; }
            if (value.Equals("central")) { return s_propCENTRAL; }
            if (value.Equals("middle")) { return s_propMIDDLE; }
            if (value.Equals("text-after-edge")) { return s_propTEXT_AFTER_EDGE; }
            if (value.Equals("text-before-edge")) { return s_propTEXT_BEFORE_EDGE; }
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