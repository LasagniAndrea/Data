namespace Fonet.Fo.Flow
{
    using System.Collections;
    using Fonet.Layout;

    internal class Footnote : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new Footnote(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        public Footnote(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:footnote";
        }

        public override Status Layout(Area area)
        {
            FONode inline = null;
            FONode fbody = null;
            if (this.marker == MarkerStart)
            {
                this.marker = 0;
            }
            int numChildren = this.children.Count;
            for (int i = this.marker; i < numChildren; i++)
            {
                FONode fo = (FONode)children[i];
                if (fo is Inline)
                {
                    inline = fo;
                    Status status = fo.Layout(area);
                    if (status.IsIncomplete())
                    {
                        return status;
                    }
                }
                else if (inline != null && fo is FootnoteBody)
                {
                    fbody = fo;
                    if (area is BlockArea area1)
                    {
                        area1.AddFootnote((FootnoteBody)fbody);
                    }
                    else
                    {
                        Page page = area.GetPage();
                        LayoutFootnote(page, (FootnoteBody)fbody, area);
                    }
                }
            }
            if (fbody == null)
            {
                FonetDriver.ActiveDriver.FireFonetWarning(
                    "No footnote-body in footnote");
            }
            if (area is BlockArea) { }
            return new Status(Status.OK);
        }

        public static bool LayoutFootnote(Page p, FootnoteBody fb, Area area)
        {
            try
            {
                BodyAreaContainer bac = p.GetBody();
                AreaContainer footArea = bac.GetFootnoteReferenceArea();
                footArea.SetIDReferences(bac.GetIDReferences());
                int basePos = footArea.GetCurrentYPosition()
                    - footArea.GetHeight();
                int oldHeight = footArea.GetHeight();
                if (area != null)
                {
                    footArea.SetMaxHeight(area.GetMaxHeight() - area.GetHeight()
                        + footArea.GetHeight());
                }
                else
                {
                    footArea.SetMaxHeight(bac.GetMaxHeight()
                        + footArea.GetHeight());
                }
                Status status = fb.Layout(footArea);
                if (status.IsIncomplete())
                {
                    return false;
                }
                else
                {
                    if (area != null)
                    {
                        area.SetMaxHeight(area.GetMaxHeight()
                            - footArea.GetHeight() + oldHeight);
                    }
                    if (bac.GetFootnoteState() == 0)
                    {
                        Area ar = bac.GetMainReferenceArea();
                        DecreaseMaxHeight(ar, footArea.GetHeight() - oldHeight);
                        footArea.SetYPosition(basePos + footArea.GetHeight());
                    }
                }
            }
            catch (FonetException)
            {
                return false;
            }
            return true;
        }

        protected static void DecreaseMaxHeight(Area ar, int change)
        {
            ar.SetMaxHeight(ar.GetMaxHeight() - change);
            ArrayList childs = ar.GetChildren();
            foreach (object obj in childs)
            {
                if (obj is Area childArea)
                {
                    DecreaseMaxHeight(childArea, change);
                }
            }
        }
    }
}