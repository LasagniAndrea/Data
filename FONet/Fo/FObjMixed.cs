using Fonet.Layout;

namespace Fonet.Fo
{
    internal class FObjMixed : FObj
    {
        protected TextState ts;

        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new FObjMixed(parent, propertyList);
            }

        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        protected FObjMixed(FObj parent, PropertyList propertyList)
            : base(parent, propertyList) { }

        public TextState GetTextState()
        {
            return ts;
        }

        protected internal override void AddCharacters(char[] data, int start, int length)
        {
            FOText ft = new FOText(data, start, length, this);
            ft.SetUnderlined(ts.GetUnderlined());
            ft.SetOverlined(ts.GetOverlined());
            ft.SetLineThrough(ts.GetLineThrough());
            AddChild(ft);
        }

        public override Status Layout(Area area)
        {
            if (this.properties != null)
            {
                Property prop = this.properties.GetProperty("id");
                if (prop != null)
                {
                    string id = prop.GetString();

                    if (this.marker == MarkerStart)
                    {
                        if (area.GetIDReferences() != null)
                        {
                            area.GetIDReferences().CreateID(id);
                        }
                        this.marker = 0;
                    }

                    if (this.marker == 0)
                    {
                        if (area.GetIDReferences() != null)
                        {
                            area.GetIDReferences().ConfigureID(id, area);
                        }
                    }
                }
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

    }

}