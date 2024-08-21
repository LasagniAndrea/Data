using System;
using System.Collections;
using Fonet.Fo.Flow;
using Fonet.Fo.Properties;
using Fonet.Layout;

namespace Fonet.Fo.Pagination
{
    internal class PageSequence : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new PageSequence(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        private const int EXPLICIT = 0;
        private const int AUTO = 1;
        private const int AUTO_EVEN = 2;
        private const int AUTO_ODD = 3;

        private readonly Root root;
        private readonly LayoutMasterSet layoutMasterSet;
        private readonly Hashtable _flowMap;
        private readonly string masterName;
        private bool _isFlowSet = false;
        private Page currentPage;
        private readonly string ipnValue;
        private int currentPageNumber = 0;
        private readonly PageNumberGenerator pageNumberGenerator;
        private readonly int forcePageCount = 0;
        private int pageCount = 0;
        private bool isForcing = false;
        private readonly int pageNumberType;
        private bool thisIsFirstPage;
        private ISubSequenceSpecifier currentSubsequence;
        private int currentSubsequenceNumber = -1;
        private string currentPageMasterName;

        protected PageSequence(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:page-sequence";

            if (parent.GetName().Equals("fo:root"))
            {
                this.root = (Root)parent;
            }
            else
            {
                throw new FonetException("page-sequence must be child of root, not "
                    + parent.GetName());
            }

            layoutMasterSet = root.GetLayoutMasterSet();
            layoutMasterSet.CheckRegionNames();

            _flowMap = new Hashtable();

            thisIsFirstPage = true;
            ipnValue = this.properties.GetProperty("initial-page-number").GetString();

            if (ipnValue.Equals("auto"))
            {
                pageNumberType = AUTO;
            }
            else if (ipnValue.Equals("auto-even"))
            {
                pageNumberType = AUTO_EVEN;
            }
            else if (ipnValue.Equals("auto-odd"))
            {
                pageNumberType = AUTO_ODD;
            }
            else
            {
                pageNumberType = EXPLICIT;
                try
                {
                    int pageStart = Int32.Parse(ipnValue);
                    this.currentPageNumber = (pageStart > 0) ? pageStart - 1 : 0;
                }
                catch (FormatException)
                {
                    throw new FonetException("\"" + ipnValue
                        + "\" is not a valid value for initial-page-number");
                }
            }

            masterName = this.properties.GetProperty("master-reference").GetString();

            this.pageNumberGenerator =
                new PageNumberGenerator(this.properties.GetProperty("format").GetString(),
                                        this.properties.GetProperty("grouping-separator").GetCharacter(),
                                        this.properties.GetProperty("grouping-size").GetNumber().IntValue(),
                                        this.properties.GetProperty("letter-value").GetEnum());

            this.forcePageCount =
                this.properties.GetProperty("force-page-count").GetEnum();

        }


        public void AddFlow(Flow.Flow flow)
        {
            if (_flowMap.ContainsKey(flow.GetFlowName()))
            {
                throw new FonetException("flow-names must be unique within an fo:page-sequence");
            }
            if (!this.layoutMasterSet.RegionNameExists(flow.GetFlowName()))
            {
                FonetDriver.ActiveDriver.FireFonetError(
                    "region-name '" + flow.GetFlowName() + "' doesn't exist in the layout-master-set.");
            }
            _flowMap.Add(flow.GetFlowName(), flow);
            IsFlowSet = true;
        }

        public void Format(AreaTree areaTree)
        {
            Status status = new Status(Status.OK);

            this.layoutMasterSet.ResetPageMasters();

            int firstAvailPageNumber;
            do
            {
                firstAvailPageNumber = this.root.GetRunningPageNumberCounter();
                bool tempIsFirstPage = false;

                if (thisIsFirstPage)
                {
                    tempIsFirstPage = thisIsFirstPage;
                    if (pageNumberType == AUTO)
                    {
                        this.currentPageNumber =
                            this.root.GetRunningPageNumberCounter();
                    }
                    else if (pageNumberType == AUTO_ODD)
                    {
                        this.currentPageNumber =
                            this.root.GetRunningPageNumberCounter();
                        if (this.currentPageNumber % 2 == 1)
                        {
                            this.currentPageNumber++;
                        }
                    }
                    else if (pageNumberType == AUTO_EVEN)
                    {
                        this.currentPageNumber =
                            this.root.GetRunningPageNumberCounter();
                        if (this.currentPageNumber % 2 == 0)
                        {
                            this.currentPageNumber++;
                        }
                    }
                    thisIsFirstPage = false;
                }

                this.currentPageNumber++;
                bool isEmptyPage;
                if ((status.GetCode() == Status.FORCE_PAGE_BREAK_EVEN)
                    && ((currentPageNumber % 2) == 1))
                {
                    isEmptyPage = true;
                }
                else if ((status.GetCode() == Status.FORCE_PAGE_BREAK_ODD)
                    && ((currentPageNumber % 2) == 0))
                {
                    isEmptyPage = true;
                }
                else
                {
                    isEmptyPage = false;
                }

                currentPage = MakePage(areaTree, firstAvailPageNumber,
                                       tempIsFirstPage, isEmptyPage);

                currentPage.SetNumber(this.currentPageNumber);
                string formattedPageNumber =
                    pageNumberGenerator.MakeFormattedPageNumber(this.currentPageNumber);
                currentPage.SetFormattedNumber(formattedPageNumber);
                this.root.SetRunningPageNumberCounter(this.currentPageNumber);

                FonetDriver.ActiveDriver.FireFonetInfo(
                    "[" + currentPageNumber + "]");

                if ((status.GetCode() == Status.FORCE_PAGE_BREAK_EVEN)
                    && ((currentPageNumber % 2) == 1)) { }
                else if ((status.GetCode() == Status.FORCE_PAGE_BREAK_ODD)
                    && ((currentPageNumber % 2) == 0)) { }
                else
                {
                    BodyAreaContainer bodyArea = currentPage.GetBody();
                    bodyArea.SetIDReferences(areaTree.GetIDReferences());

                    Flow.Flow flow = GetCurrentFlow(RegionBody.REGION_CLASS);

                    if (flow == null)
                    {
                        FonetDriver.ActiveDriver.FireFonetError(
                            "No flow found for region-body in page-master '" + currentPageMasterName + "'");
                        break;
                    }
                    else
                    {
                        status = flow.Layout(bodyArea);
                    }

                }

                currentPage.SetPageSequence(this);
                FormatStaticContent(areaTree);

                areaTree.AddPage(currentPage);
                this.pageCount++;
            } while (FlowsAreIncomplete());
            ForcePage(areaTree, firstAvailPageNumber);
            currentPage = null;
        }

        private Page MakePage(AreaTree areaTree, int firstAvailPageNumber,
                              bool isFirstPage,
                              bool isEmptyPage)
        {
            PageMaster pageMaster = GetNextPageMaster(masterName,
                                                      firstAvailPageNumber,
                                                      isFirstPage, isEmptyPage);
            if (pageMaster == null)
            {
                throw new FonetException("page masters exhausted. Cannot recover.");
            }
            Page p = pageMaster.MakePage(areaTree);
            if (currentPage != null)
            {
                ArrayList foots = currentPage.GetPendingFootnotes();
                p.SetPendingFootnotes(foots);
            }
            return p;
        }

        private void FormatStaticContent(AreaTree areaTree)
        {
            SimplePageMaster simpleMaster = GetCurrentSimplePageMaster();

            if (simpleMaster.GetRegion(RegionBefore.REGION_CLASS) != null
                && (currentPage.GetBefore() != null))
            {
                Flow.Flow staticFlow =
                    (Flow.Flow)_flowMap[simpleMaster.GetRegion(RegionBefore.REGION_CLASS).GetRegionName()];
                if (staticFlow != null)
                {
                    AreaContainer beforeArea = currentPage.GetBefore();
                    beforeArea.SetIDReferences(areaTree.GetIDReferences());
                    LayoutStaticContent(staticFlow,
                                        simpleMaster.GetRegion(RegionBefore.REGION_CLASS),
                                        beforeArea);
                }
            }

            if (simpleMaster.GetRegion(RegionAfter.REGION_CLASS) != null
                && (currentPage.GetAfter() != null))
            {
                Flow.Flow staticFlow =
                    (Flow.Flow)_flowMap[simpleMaster.GetRegion(RegionAfter.REGION_CLASS).GetRegionName()];
                if (staticFlow != null)
                {
                    AreaContainer afterArea = currentPage.GetAfter();
                    afterArea.SetIDReferences(areaTree.GetIDReferences());
                    LayoutStaticContent(staticFlow,
                                        simpleMaster.GetRegion(RegionAfter.REGION_CLASS),
                                        afterArea);
                }
            }

            if (simpleMaster.GetRegion(RegionStart.REGION_CLASS) != null
                && (currentPage.GetStart() != null))
            {
                Flow.Flow staticFlow =
                    (Flow.Flow)_flowMap[simpleMaster.GetRegion(RegionStart.REGION_CLASS).GetRegionName()];
                if (staticFlow != null)
                {
                    AreaContainer startArea = currentPage.GetStart();
                    startArea.SetIDReferences(areaTree.GetIDReferences());
                    LayoutStaticContent(staticFlow,
                                        simpleMaster.GetRegion(RegionStart.REGION_CLASS),
                                        startArea);
                }
            }

            if (simpleMaster.GetRegion(RegionEnd.REGION_CLASS) != null
                && (currentPage.GetEnd() != null))
            {
                Flow.Flow staticFlow =
                    (Flow.Flow)_flowMap[simpleMaster.GetRegion(RegionEnd.REGION_CLASS).GetRegionName()];
                if (staticFlow != null)
                {
                    AreaContainer endArea = currentPage.GetEnd();
                    endArea.SetIDReferences(areaTree.GetIDReferences());
                    LayoutStaticContent(staticFlow,
                                        simpleMaster.GetRegion(RegionEnd.REGION_CLASS),
                                        endArea);
                }
            }

        }

        private void LayoutStaticContent(Flow.Flow flow, Region region,
                                         AreaContainer area)
        {
            if (flow is StaticContent content)
            {
                content.Layout(area, region);
            }
            else
            {
                FonetDriver.ActiveDriver.FireFonetError(region.GetName()
                    + " only supports static-content flows currently. "
                    + "Cannot use flow named '"
                    + flow.GetFlowName() + "'");
            }
        }

        private ISubSequenceSpecifier GetNextSubsequence(PageSequenceMaster master)
        {
            if (master.GetSubSequenceSpecifierCount()
                > currentSubsequenceNumber + 1)
            {
                currentSubsequence =
                    master.GetSubSequenceSpecifier(currentSubsequenceNumber + 1);
                currentSubsequenceNumber++;
                return currentSubsequence;
            }
            else
            {
                return null;
            }
        }

        private SimplePageMaster GetNextSimplePageMaster(PageSequenceMaster sequenceMaster,
                                                         int currentPageNumber, bool thisIsFirstPage,
                                                         bool isEmptyPage)
        {
            if (isForcing)
            {
                return this.layoutMasterSet.GetSimplePageMaster(
                    GetNextPageMasterName(sequenceMaster, currentPageNumber, false, true));
            }
            string nextPageMaster =
                GetNextPageMasterName(sequenceMaster, currentPageNumber, thisIsFirstPage, isEmptyPage);
            return this.layoutMasterSet.GetSimplePageMaster(nextPageMaster);

        }

        private string GetNextPageMasterName(PageSequenceMaster sequenceMaster,
                                             int currentPageNumber,
                                             bool thisIsFirstPage,
                                             bool isEmptyPage)
        {
            if (null == currentSubsequence)
            {
                currentSubsequence = GetNextSubsequence(sequenceMaster);
            }

            string nextPageMaster =
                currentSubsequence.GetNextPageMaster(currentPageNumber,
                                                     thisIsFirstPage,
                                                     isEmptyPage);


            if (null == nextPageMaster
                || IsFlowForMasterNameDone(currentPageMasterName))
            {
                ISubSequenceSpecifier nextSubsequence =
                    GetNextSubsequence(sequenceMaster);
                if (nextSubsequence == null)
                {
                    FonetDriver.ActiveDriver.FireFonetError(
                        "Page subsequences exhausted. Using previous subsequence.");
                    thisIsFirstPage =
                        true;
                    currentSubsequence.Reset();
                }
                else
                {
                    currentSubsequence = nextSubsequence;
                }

                nextPageMaster =
                    currentSubsequence.GetNextPageMaster(currentPageNumber,
                                                         thisIsFirstPage,
                                                         isEmptyPage);
            }
            currentPageMasterName = nextPageMaster;

            return nextPageMaster;

        }

        private SimplePageMaster GetCurrentSimplePageMaster()
        {
            return this.layoutMasterSet.GetSimplePageMaster(currentPageMasterName);
        }

        private string GetCurrentPageMasterName()
        {
            return currentPageMasterName;
        }

        private PageMaster GetNextPageMaster(string pageSequenceName,
                                             int currentPageNumber,
                                             bool thisIsFirstPage,
                                             bool isEmptyPage)
        {
            PageSequenceMaster sequenceMaster =
                this.layoutMasterSet.GetPageSequenceMaster(pageSequenceName);

            PageMaster pageMaster;
            if (sequenceMaster != null)
            {
                pageMaster = GetNextSimplePageMaster(sequenceMaster,
                                                     currentPageNumber,
                                                     thisIsFirstPage,
                                                     isEmptyPage).GetPageMaster();

            }
            else
            {
                SimplePageMaster simpleMaster =
                    this.layoutMasterSet.GetSimplePageMaster(pageSequenceName);
                if (simpleMaster == null)
                {
                    throw new FonetException("'master-reference' for 'fo:page-sequence'"
                        + "matches no 'simple-page-master' or 'page-sequence-master'");
                }
                currentPageMasterName = pageSequenceName;

                pageMaster = simpleMaster.GetNextPageMaster();
            }
            return pageMaster;
        }

        private bool FlowsAreIncomplete()
        {
            bool isIncomplete = false;

            foreach (Flow.Flow flow in _flowMap.Values)
            {
                if (flow is StaticContent)
                {
                    continue;
                }
                Status status = flow.GetStatus();
                isIncomplete |= status.IsIncomplete();
            }
            return isIncomplete;
        }

        private Flow.Flow GetCurrentFlow(string regionClass)
        {
            Region region = GetCurrentSimplePageMaster().GetRegion(regionClass);
            if (region != null)
            {
                Flow.Flow flow = (Flow.Flow)_flowMap[region.GetRegionName()];
                return flow;

            }
            else
            {
                FonetDriver.ActiveDriver.FireFonetInfo(
                    "flow is null. regionClass = '" + regionClass
                        + "' currentSPM = "
                        + GetCurrentSimplePageMaster());
                return null;
            }

        }

        private bool IsFlowForMasterNameDone(string masterName)
        {
            if (isForcing)
            {
                return false;
            }
            if (masterName != null)
            {
                SimplePageMaster spm =
                    this.layoutMasterSet.GetSimplePageMaster(masterName);
                Region region = spm.GetRegion(RegionBody.REGION_CLASS);


                Flow.Flow flow = (Flow.Flow)_flowMap[region.GetRegionName()];
                if ((null == flow) || flow.GetStatus().IsIncomplete())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsFlowSet
        {
            get { return _isFlowSet; }
            set { _isFlowSet = value; }
        }

        public string IpnValue
        {
            get { return ipnValue; }
        }

        public int CurrentPageNumber
        {
            get { return currentPageNumber; }
        }

        public int PageCount
        {
            get { return this.pageCount; }
        }

        private void ForcePage(AreaTree areaTree, int firstAvailPageNumber)
        {
            bool bmakePage = false;
            if (this.forcePageCount == ForcePageCount.AUTO)
            {
                PageSequence nextSequence =
                    this.root.GetSucceedingPageSequence(this);
                if (nextSequence != null)
                {
                    if (nextSequence.IpnValue.Equals("auto"))
                    {
                        // do nothing
                    }
                    else if (nextSequence.IpnValue.Equals("auto-odd"))
                    {
                        if (firstAvailPageNumber % 2 == 0)
                        {
                            bmakePage = true;
                        }
                    }
                    else if (nextSequence.IpnValue.Equals("auto-even"))
                    {
                        if (firstAvailPageNumber % 2 != 0)
                        {
                            bmakePage = true;
                        }
                    }
                    else
                    {
                        int nextSequenceStartPageNumber =
                            nextSequence.CurrentPageNumber;
                        if ((nextSequenceStartPageNumber % 2 == 0)
                            && (firstAvailPageNumber % 2 == 0))
                        {
                            bmakePage = true;
                        }
                        else if ((nextSequenceStartPageNumber % 2 != 0)
                            && (firstAvailPageNumber % 2 != 0))
                        {
                            bmakePage = true;
                        }
                    }
                }
            }
            else if ((this.forcePageCount == ForcePageCount.EVEN)
                && (this.pageCount % 2 != 0))
            {
                bmakePage = true;
            }
            else if ((this.forcePageCount == ForcePageCount.ODD)
                && (this.pageCount % 2 == 0))
            {
                bmakePage = true;
            }
            else if ((this.forcePageCount == ForcePageCount.END_ON_EVEN)
                && (firstAvailPageNumber % 2 == 0))
            {
                bmakePage = true;
            }
            else if ((this.forcePageCount == ForcePageCount.END_ON_ODD)
                && (firstAvailPageNumber % 2 != 0))
            {
                bmakePage = true;
            }
            else if (this.forcePageCount == ForcePageCount.NO_FORCE)
            {
                // do nothing
            }

            if (bmakePage)
            {
                try
                {
                    this.isForcing = true;
                    this.currentPageNumber++;
                    firstAvailPageNumber = this.currentPageNumber;
                    currentPage = MakePage(areaTree, firstAvailPageNumber, false, true);
                    string formattedPageNumber =
                        pageNumberGenerator.MakeFormattedPageNumber(this.currentPageNumber);
                    currentPage.SetFormattedNumber(formattedPageNumber);
                    currentPage.SetPageSequence(this);
                    FormatStaticContent(areaTree);

                    FonetDriver.ActiveDriver.FireFonetInfo(
                        "[forced-" + firstAvailPageNumber + "]");

                    areaTree.AddPage(currentPage);
                    this.root.SetRunningPageNumberCounter(this.currentPageNumber);
                    this.isForcing = false;
                }
                catch (FonetException)
                {
                    FonetDriver.ActiveDriver.FireFonetInfo(
                        "'force-page-count' failure");
                }
            }
        }

    }
}