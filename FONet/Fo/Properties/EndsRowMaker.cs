using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class EndsRowMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new EndsRowMaker2(propName);
        }

        protected EndsRowMaker2(string name) : base(name) { }


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
    internal class EndsRowMaker : GenericBoolean
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new EndsRowMaker(propName);
        }

        protected EndsRowMaker(string name) : base(name) { }


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