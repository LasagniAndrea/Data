using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class RelativePositionMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new RelativePositionMaker2(propName);
        }

        protected RelativePositionMaker2(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "static", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
    // EG 20160404 Migration vs2013
    internal class RelativePositionMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propSTATIC = new EnumProperty(Constants.STATIC);
        protected static readonly EnumProperty s_propRELATIVE = new EnumProperty(Constants.RELATIVE);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);
        new public static PropertyMaker Maker(string propName)
        {
            return new RelativePositionMaker(propName);
        }

        protected RelativePositionMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("static")) { return s_propSTATIC; }
            if (value.Equals("relative")) { return s_propRELATIVE; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

            return base.CheckEnumValues(value);
        }


        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "static", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
}