namespace Fonet.Fo.Flow
{
    using Fonet.Layout;

    internal class Marker : FObjMixed
    {
        private readonly string markerClassName;

        private Area registryArea;

        private bool isFirst;

        private bool isLast;

        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new Marker(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        public Marker(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:marker";

            this.markerClassName =
                this.properties.GetProperty("marker-class-name").GetString();
            ts = propMgr.GetTextDecoration(parent);

            try
            {
                parent.AddMarker(this.markerClassName);
            }
            catch (FonetException)
            {
            }
        }

        public override Status Layout(Area area)
        {
            this.registryArea = area;
            area.GetPage().RegisterMarker(this);
            return new Status(Status.OK);
        }

        public Status LayoutMarker(Area area)
        {
            if (this.marker == MarkerStart)
            {
                this.marker = 0;
            }

            int numChildren = this.children.Count;
            for (int i = this.marker; i < numChildren; i++)
            {
                FONode fo = (FONode)children[i];

                Status status;
                if ((status = fo.Layout(area)).IsIncomplete())
                {
                    this.marker = i;
                    return status;
                }
            }

            return new Status(Status.OK);
        }

        public string GetMarkerClassName()
        {
            return markerClassName;
        }

        public Area GetRegistryArea()
        {
            return registryArea;
        }

        public void ReleaseRegistryArea()
        {
            isFirst = registryArea.IsFirst();
            isLast = registryArea.IsLast();
            registryArea = null;
        }

        public new void ResetMarker()
        {
            if (registryArea != null)
            {
                Page page = registryArea.GetPage();
                if (page != null)
                {
                    page.UnregisterMarker(this);
                }
            }
        }

        public void ResetMarkerContent()
        {
            base.ResetMarker();
        }

        public override bool MayPrecedeMarker()
        {
            return true;
        }
    }
}