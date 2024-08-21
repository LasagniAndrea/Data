namespace Fonet.Fo.Flow
{
    using Fonet.Fo.Properties;
    using Fonet.Layout;

    internal class BasicLink : Inline
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new BasicLink(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        public BasicLink(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:basic-link";
        }

        public override Status Layout(Area area)
        {
            string destination;
            int linkType;
            _ = propMgr.GetAccessibilityProps();
            _ = propMgr.GetAuralProps();
            _ = propMgr.GetBorderAndPadding();
            _ = propMgr.GetBackgroundProps();
            _ = propMgr.GetMarginInlineProps();
            _ = propMgr.GetRelativePositionProps();

            if (!(destination =
                this.properties.GetProperty("internal-destination").GetString()).Equals(""))
            {
                linkType = LinkSet.INTERNAL;
            }
            else if (!(destination =
                this.properties.GetProperty("external-destination").GetString()).Equals(""))
            {
                linkType = LinkSet.EXTERNAL;
            }
            else
            {
                throw new FonetException("internal-destination or external-destination must be specified in basic-link");
            }

            if (this.marker == MarkerStart)
            {
                string id = this.properties.GetProperty("id").GetString();
                area.GetIDReferences().InitializeID(id, area);
                this.marker = 0;
            }

            LinkSet ls = new LinkSet(destination, area, linkType);

            AreaContainer ac = area.GetNearestAncestorAreaContainer();
            while (ac != null && ac.GetPosition() != Position.ABSOLUTE)
            {
                ac = ac.GetNearestAncestorAreaContainer();
            }
            if (ac == null)
            {
                ac = area.GetPage().GetBody().GetCurrentColumnArea();
            }

            int numChildren = this.children.Count;
            for (int i = this.marker; i < numChildren; i++)
            {
                FONode fo = (FONode)children[i];
                fo.SetLinkSet(ls);

                Status status;
                if ((status = fo.Layout(area)).IsIncomplete())
                {
                    this.marker = i;
                    return status;
                }
            }

            ls.ApplyAreaContainerOffsets(ac, area);
            area.GetPage().AddLinkSet(ls);

            return new Status(Status.OK);
        }
    }
}