using System;
using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class BorderSpacingMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new BorderSpacingMaker2(propName);
        }

        protected BorderSpacingMaker2(string name) : base(name) { }

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
    internal class BorderSpacingMaker : LengthPairProperty.Maker
    {
        private static readonly PropertyMaker s_BlockProgressionDirectionMaker = new LengthProperty.Maker("border-spacing.block-progression-direction");

        private static readonly PropertyMaker s_InlineProgressionDirectionMaker = new LengthProperty.Maker("border-spacing.inline-progression-direction");

        new public static PropertyMaker Maker(string propName)
        {
            return new BorderSpacingMaker(propName);
        }

        protected BorderSpacingMaker(string name) 
            : base(name)
        { 
            m_shorthandMaker = GetSubpropMaker("block-progression-direction"); 
        }

        private readonly PropertyMaker m_shorthandMaker;

        public override Property CheckEnumValues(string value)
        {
            return m_shorthandMaker.CheckEnumValues(value);
        }

        protected override bool IsCompoundMaker()
        {
            return true;
        }

        protected override PropertyMaker GetSubpropMaker(string subprop)
        {
            if (subprop.Equals("block-progression-direction"))
            {
                return s_BlockProgressionDirectionMaker;
            }

            if (subprop.Equals("inline-progression-direction"))
            {
                return s_InlineProgressionDirectionMaker;
            }

            return base.GetSubpropMaker(subprop);
        }

        protected override Property SetSubprop(Property baseProp, string subpropName, Property subProp)
        {
            LengthPair val = baseProp.GetLengthPair();
            val.SetComponent(subpropName, subProp, false);
            return baseProp;
        }

        public override Property GetSubpropValue(Property baseProp, string subpropName)
        {
            LengthPair val = baseProp.GetLengthPair();
            return val.GetComponent(subpropName);
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = MakeCompound(propertyList, propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

        protected override Property MakeCompound(PropertyList pList, FObj fo)
        {
            LengthPair p = new LengthPair();
            Property subProp;

            subProp = GetSubpropMaker("block-progression-direction").Make(pList, GetDefaultForBlockProgressionDirection(), fo);
            p.SetComponent("block-progression-direction", subProp, true);

            subProp = GetSubpropMaker("inline-progression-direction").Make(pList, GetDefaultForInlineProgressionDirection(), fo);
            p.SetComponent("inline-progression-direction", subProp, true);

            return new LengthPairProperty(p);
        }

        protected virtual String GetDefaultForBlockProgressionDirection()
        {
            return "0pt";

        }

        protected virtual String GetDefaultForInlineProgressionDirection()
        {
            return "0pt";

        }

        public override Property ConvertProperty(Property p, PropertyList pList, FObj fo)
        {
            if (p is LengthPairProperty)
            {
                return p;
            }
            if (!(p is EnumProperty))
            {
                p = m_shorthandMaker.ConvertProperty(p, pList, fo);
            }
            if (p != null)
            {
                Property prop = MakeCompound(pList, fo);
                LengthPair pval = prop.GetLengthPair();

                pval.SetComponent("block-progression-direction", p, false);
                pval.SetComponent("inline-progression-direction", p, false);
                return prop;
            }
            else
            {
                return null;
            }
        }

        public override bool IsInherited()
        {
            return true;
        }




    }
}