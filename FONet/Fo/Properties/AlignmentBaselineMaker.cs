using Fonet.DataTypes;

namespace Fonet.Fo.Properties
{
    internal class AlignmentBaselineMaker2 : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new AlignmentBaselineMaker2(propName);
        }

        protected AlignmentBaselineMaker2(string name) : base(name) { }


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
    internal class AlignmentBaselineMaker : LengthProperty.Maker
    {
  protected static readonly EnumProperty s_propAUTO = new EnumProperty(Constants.AUTO);
  protected static readonly EnumProperty s_propBASELINE = new EnumProperty(Constants.BASELINE);
  protected static readonly EnumProperty s_propBEFORE_EDGE = new EnumProperty(Constants.BEFORE_EDGE);
  protected static readonly EnumProperty s_propTEXT_BEFORE_EDGE = new EnumProperty(Constants.TEXT_BEFORE_EDGE);
  protected static readonly EnumProperty s_propMIDDLE = new EnumProperty(Constants.MIDDLE);
  protected static readonly EnumProperty s_propCENTRAL = new EnumProperty(Constants.CENTRAL);
  protected static readonly EnumProperty s_propAFTER_EDGE = new EnumProperty(Constants.AFTER_EDGE);
  protected static readonly EnumProperty s_propTEXT_AFTER_EDGE = new EnumProperty(Constants.TEXT_AFTER_EDGE);
  protected static readonly EnumProperty s_propIDEOGRAPHIC = new EnumProperty(Constants.IDEOGRAPHIC);
  protected static readonly EnumProperty s_propALPHABETIC = new EnumProperty(Constants.ALPHABETIC);
  protected static readonly EnumProperty s_propHANGING = new EnumProperty(Constants.HANGING);
  protected static readonly EnumProperty s_propMATHEMATICAL = new EnumProperty(Constants.MATHEMATICAL);
  protected static readonly EnumProperty s_propINHERIT = new EnumProperty(Constants.INHERIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new AlignmentBaselineMaker(propName);
        }

        protected AlignmentBaselineMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("auto")) { return s_propAUTO; }
            if (value.Equals("baseline")) { return s_propBASELINE; }
            if (value.Equals("before-edge")) { return s_propBEFORE_EDGE; }
            if (value.Equals("text-before-edge")) { return s_propTEXT_BEFORE_EDGE; }
            if (value.Equals("middle")) { return s_propMIDDLE; }
            if (value.Equals("central")) { return s_propCENTRAL; }
            if (value.Equals("after-edge")) { return s_propAFTER_EDGE; }
            if (value.Equals("text-after-edge")) { return s_propTEXT_AFTER_EDGE; }
            if (value.Equals("ideographic")) { return s_propIDEOGRAPHIC; }
            if (value.Equals("alphabetic")) { return s_propALPHABETIC; }
            if (value.Equals("hanging")) { return s_propHANGING; }
            if (value.Equals("mathematical")) { return s_propMATHEMATICAL; }
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