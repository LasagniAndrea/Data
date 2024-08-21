using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class StartsRowMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new StartsRowMaker2(propName);
        }

        protected StartsRowMaker2(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "false", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
    // EG 20160404 Migration vs2013
    internal class StartsRowMaker : GenericBoolean
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new StartsRowMaker(propName);
        }

        protected StartsRowMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "false", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
}