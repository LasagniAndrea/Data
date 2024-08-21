using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class ScalingMethodMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new ScalingMethodMaker2(propName);
        }

        protected ScalingMethodMaker2(string name) : base(name) { }


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
    internal class ScalingMethodMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propAUTO = new EnumProperty(Constants.AUTO);
        protected static readonly EnumProperty s_propINTEGER_PIXELS = new EnumProperty(Constants.INTEGER_PIXELS);
        protected static readonly EnumProperty s_propRESAMPLE_ANY_METHOD = new EnumProperty(Constants.RESAMPLE_ANY_METHOD);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);
        new public static PropertyMaker Maker(string propName)
        {
            return new ScalingMethodMaker(propName);
        }

        protected ScalingMethodMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("auto")) { return s_propAUTO; }
            if (value.Equals("integer-pixels")) { return s_propINTEGER_PIXELS; }
            if (value.Equals("resample-any-method")) { return s_propRESAMPLE_ANY_METHOD; }
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