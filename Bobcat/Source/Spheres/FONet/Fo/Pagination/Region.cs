namespace Fonet.Fo.Pagination
{
    using Fonet.Layout;

    internal abstract class Region : FObj
    {
        public const string PROP_REGION_NAME = "region-name";

        private readonly SimplePageMaster _layoutMaster;
        private string _regionName;

        protected Region(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = GetElementName();

            if (null == this.properties.GetProperty(PROP_REGION_NAME))
            {
                SetRegionName(GetDefaultRegionName());
            }
            else if (this.properties.GetProperty(PROP_REGION_NAME).GetString().Equals(""))
            {
                SetRegionName(GetDefaultRegionName());
            }
            else
            {
                SetRegionName(this.properties.GetProperty(PROP_REGION_NAME).GetString());
                if (IsReserved(GetRegionName())
                    && !GetRegionName().Equals(GetDefaultRegionName()))
                {
                    throw new FonetException(PROP_REGION_NAME + " '" + _regionName
                        + "' for " + this.name
                        + " not permitted.");
                }
            }

            if (parent.GetName().Equals("fo:simple-page-master"))
            {
                _layoutMaster = (SimplePageMaster)parent;
                GetPageMaster().AddRegion(this);
            }
            else
            {
                throw new FonetException(GetElementName() + " must be child "
                    + "of simple-page-master, not "
                    + parent.GetName());
            }
        }

        public abstract RegionArea MakeRegionArea(int allocationRectangleXPosition,
                                                  int allocationRectangleYPosition,
                                                  int allocationRectangleWidth,
                                                  int allocationRectangleHeight);

        protected abstract string GetDefaultRegionName();

        protected abstract string GetElementName();

        public abstract string GetRegionClass();

        public string GetRegionName()
        {
            return _regionName;
        }

        private void SetRegionName(string name)
        {
            _regionName = name;
        }

        protected SimplePageMaster GetPageMaster()
        {
            return _layoutMaster;
        }

        protected bool IsReserved(string name)
        {
            return (name.Equals("xsl-region-before")
                || name.Equals("xsl-region-start")
                || name.Equals("xsl-region-end")
                || name.Equals("xsl-region-after")
                || name.Equals("xsl-before-float-separator")
                || name.Equals("xsl-footnote-separator"));
        }

        public override bool GeneratesReferenceAreas()
        {
            return true;
        }
    }
}