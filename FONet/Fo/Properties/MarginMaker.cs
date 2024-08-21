using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class MarginMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new MarginMaker2(propName);
        }

        protected MarginMaker2(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
    internal class MarginMaker : ListProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new MarginMaker(propName);
        }

        protected MarginMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }
    }
}