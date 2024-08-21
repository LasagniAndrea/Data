using System.Collections;
using Fonet.Layout;

namespace Fonet.Fo.Pagination
{
    internal class SimplePageMaster : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new SimplePageMaster(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        private readonly Hashtable _regions;

        private readonly LayoutMasterSet layoutMasterSet;
        private PageMaster pageMaster;
        private readonly string masterName;
        private bool beforePrecedence;
        private int beforeHeight;
        private bool afterPrecedence;
        private int afterHeight;

        protected SimplePageMaster(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:simple-page-master";

            if (parent.GetName().Equals("fo:layout-master-set"))
            {
                this.layoutMasterSet = (LayoutMasterSet)parent;
                masterName = this.properties.GetProperty("master-name").GetString();
                if (masterName == null)
                {
                    FonetDriver.ActiveDriver.FireFonetWarning(
                        "simple-page-master does not have a master-name and so is being ignored");
                }
                else
                {
                    this.layoutMasterSet.AddSimplePageMaster(this);
                }
            }
            else
            {
                throw new FonetException("fo:simple-page-master must be child "
                    + "of fo:layout-master-set, not "
                    + parent.GetName());
            }
            _regions = new Hashtable();
        }

        protected internal override void End()
        {
            int pageWidth =
                this.properties.GetProperty("page-width").GetLength().MValue();
            int pageHeight =
                this.properties.GetProperty("page-height").GetLength().MValue();
            MarginProps mProps = propMgr.GetMarginProps();

            int contentRectangleXPosition = mProps.marginLeft;
            int contentRectangleYPosition = pageHeight - mProps.marginTop;
            int contentRectangleWidth = pageWidth - mProps.marginLeft
                - mProps.marginRight;
            int contentRectangleHeight = pageHeight - mProps.marginTop
                - mProps.marginBottom;

            this.pageMaster = new PageMaster(pageWidth, pageHeight);
            if (GetRegion(RegionBody.REGION_CLASS) != null)
            {
                BodyRegionArea body =
                    (BodyRegionArea)GetRegion(RegionBody.REGION_CLASS).MakeRegionArea(contentRectangleXPosition,
                                                                                       contentRectangleYPosition,
                                                                                       contentRectangleWidth,
                                                                                       contentRectangleHeight);
                this.pageMaster.AddBody(body);
            }
            else
            {
                FonetDriver.ActiveDriver.FireFonetError(
                    "simple-page-master must have a region of class " +
                        RegionBody.REGION_CLASS);
            }

            if (GetRegion(RegionBefore.REGION_CLASS) != null)
            {
                RegionArea before =
                    GetRegion(RegionBefore.REGION_CLASS).MakeRegionArea(contentRectangleXPosition,
                                                                        contentRectangleYPosition, contentRectangleWidth,
                                                                        contentRectangleHeight);
                this.pageMaster.AddBefore(before);
                beforePrecedence =
                    ((RegionBefore)GetRegion(RegionBefore.REGION_CLASS)).GetPrecedence();
                beforeHeight = before.GetHeight();
            }
            else
            {
                beforePrecedence = false;
            }

            if (GetRegion(RegionAfter.REGION_CLASS) != null)
            {
                RegionArea after =
                    GetRegion(RegionAfter.REGION_CLASS).MakeRegionArea(contentRectangleXPosition,
                                                                       contentRectangleYPosition, contentRectangleWidth,
                                                                       contentRectangleHeight);
                this.pageMaster.AddAfter(after);
                afterPrecedence =
                    ((RegionAfter)GetRegion(RegionAfter.REGION_CLASS)).GetPrecedence();
                afterHeight = after.GetHeight();
            }
            else
            {
                afterPrecedence = false;
            }

            if (GetRegion(RegionStart.REGION_CLASS) != null)
            {
                RegionArea start =
                    ((RegionStart)GetRegion(RegionStart.REGION_CLASS)).MakeRegionArea(contentRectangleXPosition,
                                                                                       contentRectangleYPosition, 
                                                                                       contentRectangleHeight, beforePrecedence,
                                                                                       afterPrecedence, beforeHeight, afterHeight);
                this.pageMaster.AddStart(start);
            }

            if (GetRegion(RegionEnd.REGION_CLASS) != null)
            {
                RegionArea end =
                    ((RegionEnd)GetRegion(RegionEnd.REGION_CLASS)).MakeRegionArea(contentRectangleXPosition,
                                                                                   contentRectangleYPosition, contentRectangleWidth,
                                                                                   contentRectangleHeight, beforePrecedence,
                                                                                   afterPrecedence, beforeHeight, afterHeight);
                this.pageMaster.AddEnd(end);
            }
        }

        public PageMaster GetPageMaster()
        {
            return this.pageMaster;
        }

        public PageMaster GetNextPageMaster()
        {
            return this.pageMaster;
        }

        public string GetMasterName()
        {
            return masterName;
        }


        protected internal void AddRegion(Region region)
        {
            if (_regions.ContainsKey(region.GetRegionClass()))
            {
                throw new FonetException("Only one region of class "
                    + region.GetRegionClass()
                    + " allowed within a simple-page-master.");
            }
            else
            {
                _regions.Add(region.GetRegionClass(), region);
            }
        }

        protected internal Region GetRegion(string regionClass)
        {
            return (Region)_regions[regionClass];
        }

        protected internal Hashtable GetRegions()
        {
            return _regions;
        }

        protected internal bool RegionNameExists(string regionName)
        {
            foreach (Region r in _regions.Values)
            {
                if (r.GetRegionName().Equals(regionName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}