using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class ContentTypeMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new ContentTypeMaker2(propName);
        }

        protected ContentTypeMaker2(string name) : base(name) { }


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
    internal class ContentTypeMaker : StringProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new ContentTypeMaker(propName);
        }

        protected ContentTypeMaker(string name) : base(name) { }


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
}