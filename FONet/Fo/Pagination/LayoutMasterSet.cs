namespace Fonet.Fo.Pagination
{
    using System.Collections;

    internal class LayoutMasterSet : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new LayoutMasterSet(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        private readonly Hashtable simplePageMasters;
        private readonly Hashtable pageSequenceMasters;
        private readonly Hashtable allRegions;

        private readonly Root root;

        protected internal LayoutMasterSet(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:layout-master-set";
            this.simplePageMasters = new Hashtable();
            this.pageSequenceMasters = new Hashtable();

            if (parent.GetName().Equals("fo:root"))
            {
                this.root = (Root)parent;
                root.SetLayoutMasterSet(this);
            }
            else
            {
                throw new FonetException("fo:layout-master-set must be child of fo:root, not "
                    + parent.GetName());
            }
            allRegions = new Hashtable();

        }

        protected internal void AddSimplePageMaster(SimplePageMaster simplePageMaster)
        {
            if (ExistsName(simplePageMaster.GetMasterName()))
            {
                throw new FonetException("'master-name' ("
                    + simplePageMaster.GetMasterName()
                    + ") must be unique "
                    + "across page-masters and page-sequence-masters");
            }
            this.simplePageMasters.Add(simplePageMaster.GetMasterName(),
                                       simplePageMaster);
        }

        protected internal SimplePageMaster GetSimplePageMaster(string masterName)
        {
            return (SimplePageMaster)this.simplePageMasters[masterName];
        }

        protected internal void AddPageSequenceMaster(string masterName, PageSequenceMaster pageSequenceMaster)
        {
            if (ExistsName(masterName))
            {
                throw new FonetException("'master-name' (" + masterName
                    + ") must be unique "
                    + "across page-masters and page-sequence-masters");
            }
            this.pageSequenceMasters.Add(masterName, pageSequenceMaster);
        }

        protected internal PageSequenceMaster GetPageSequenceMaster(string masterName)
        {
            return (PageSequenceMaster)this.pageSequenceMasters[masterName];
        }

        private bool ExistsName(string masterName)
        {
            if (simplePageMasters.ContainsKey(masterName)
                || pageSequenceMasters.ContainsKey(masterName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected internal void ResetPageMasters()
        {
            foreach (PageSequenceMaster psm in pageSequenceMasters.Values)
            {
                psm.Reset();
            }
        }

        protected internal void CheckRegionNames()
        {
            foreach (SimplePageMaster spm in simplePageMasters.Values)
            {
                foreach (Region region in spm.GetRegions().Values)
                {
                    if (allRegions.ContainsKey(region.GetRegionName()))
                    {
                        string localClass = (string)allRegions[region.GetRegionName()];
                        if (!localClass.Equals(region.GetRegionClass()))
                        {
                            throw new FonetException("Duplicate region-names ("
                                + region.GetRegionName()
                                + ") must map "
                                + "to the same region-class ("
                                + localClass + "!="
                                + region.GetRegionClass()
                                + ")");
                        }
                    }
                    allRegions[region.GetRegionName()] = region.GetRegionClass();
                }
            }
        }

        protected internal bool RegionNameExists(string regionName)
        {
            bool result = false;
            foreach (SimplePageMaster spm in simplePageMasters.Values)
            {
                result = spm.RegionNameExists(regionName);
                if (result)
                {
                    return result;
                }
            }
            return result;
        }
    }
}