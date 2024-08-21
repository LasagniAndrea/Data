using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class LineHeightShiftAdjustmentMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new LineHeightShiftAdjustmentMaker2(propName);
        }

        protected LineHeightShiftAdjustmentMaker2(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "consider-shifts", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
    // EG 20160404 Migration vs2013
    internal class LineHeightShiftAdjustmentMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propCONSIDER_SHIFTS = new EnumProperty(Constants.CONSIDER_SHIFTS);
        protected static readonly EnumProperty s_propDISREGARD_SHIFTS = new EnumProperty(Constants.DISREGARD_SHIFTS);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new LineHeightShiftAdjustmentMaker(propName);
        }

        protected LineHeightShiftAdjustmentMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("consider-shifts")) { return s_propCONSIDER_SHIFTS; }
            if (value.Equals("disregard-shifts")) { return s_propDISREGARD_SHIFTS; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

            return base.CheckEnumValues(value);
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "consider-shifts", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
}