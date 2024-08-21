using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class LastLineEndIndentMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new LastLineEndIndentMaker2(propName);
        }

        protected LastLineEndIndentMaker2(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "0pt", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
    // EG 20160404 Migration vs2013
    internal class LastLineEndIndentMaker : LengthProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new LastLineEndIndentMaker(propName);
        }

        protected LastLineEndIndentMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "0pt", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
}