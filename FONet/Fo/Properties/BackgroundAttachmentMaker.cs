using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class BackgroundAttachmentMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new BackgroundAttachmentMaker2(propName);
        }

        protected BackgroundAttachmentMaker2(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "scroll", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }
    }
    // EG 20160404 Migration vs2013
    internal class BackgroundAttachmentMaker : EnumProperty.Maker
    {
        protected static readonly EnumProperty s_propSCROLL = new EnumProperty(Constants.SCROLL);
        protected static readonly EnumProperty s_propFIXED = new EnumProperty(Constants.FIXED);
        protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);
        new public static PropertyMaker Maker(string propName)
        {
            return new BackgroundAttachmentMaker(propName);
        }

        protected BackgroundAttachmentMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("scroll")) { return s_propSCROLL; }
            if (value.Equals("fixed")) { return s_propFIXED; }
            if (value.Equals("inherit")) { return s_propINHERIT; }

            return base.CheckEnumValues(value);
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "scroll", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }
    }
}