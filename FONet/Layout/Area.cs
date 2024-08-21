using System.Collections;
using Fonet.DataTypes;
using Fonet.Fo;
using Fonet.Fo.Flow;
using Fonet.Layout.Inline;

namespace Fonet.Layout
{
    internal abstract class Area : Box
    {
        protected FontState fontState;
        protected BorderAndPadding bp = null;
        protected ArrayList children = new ArrayList();
        protected int maxHeight;
        protected int currentHeight = 0;
        protected int tableCellXOffset = 0;
        private int absoluteYTop = 0;
        protected int contentRectangleWidth;
        protected int allocationWidth;
        protected Page page;
        protected BackgroundProps background;
        private IDReferences idReferences;
        protected ArrayList markers;
        protected FObj generatedBy;
        protected Hashtable returnedBy;
        protected string areaClass = null;
        protected bool _isFirst = false;
        protected bool _isLast = false;

        public FObj foCreator;

        public Area(FontState fontState)
        {
            SetFontState(fontState);
            this.markers = new ArrayList();
            this.returnedBy = new Hashtable();
        }

        public Area(FontState fontState, int allocationWidth, int maxHeight)
        {
            SetFontState(fontState);
            this.allocationWidth = allocationWidth;
            this.contentRectangleWidth = allocationWidth;
            this.maxHeight = maxHeight;
            this.markers = new ArrayList();
            this.returnedBy = new Hashtable();
        }

        private void SetFontState(FontState fontState)
        {
            this.fontState = fontState;
        }

        public void AddChild(Box child)
        {
            this.children.Add(child);
            child.parent = this;
        }

        public void AddChildAtStart(Box child)
        {
            this.children.Insert(0, child);
            child.parent = this;
        }

        public void AddDisplaySpace(int size)
        {
            this.AddChild(new DisplaySpace(size));
            this.currentHeight += size;
        }

        public void AddInlineSpace(int size)
        {
            this.AddChild(new InlineSpace(size));
        }

        public FontInfo GetFontInfo()
        {
            return this.page.GetFontInfo();
        }

        public virtual void End()
        {
        }

        public int GetAllocationWidth()
        {
            return this.allocationWidth;
        }

        public void SetAllocationWidth(int w)
        {
            this.allocationWidth = w;
            this.contentRectangleWidth = this.allocationWidth;
        }

        public ArrayList GetChildren()
        {
            return this.children;
        }

        public bool HasChildren()
        {
            return (this.children.Count != 0);
        }

        public bool HasNonSpaceChildren()
        {
            if (this.children.Count > 0)
            {
                foreach (object child in children)
                {
                    if (!(child is DisplaySpace))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual int GetContentWidth()
        {
            return contentRectangleWidth;
        }

        public FontState GetFontState()
        {
            return this.fontState;
        }

        public virtual int GetContentHeight()
        {
            return this.currentHeight;
        }

        public virtual int GetHeight()
        {
            return this.currentHeight + GetPaddingTop() + GetPaddingBottom() + GetBorderTopWidth() + GetBorderBottomWidth();
        }

        public int GetMaxHeight()
        {
            return this.maxHeight;
        }

        public Page GetPage()
        {
            return this.page;
        }

        public BackgroundProps GetBackground()
        {
            return this.background;
        }

        public int GetPaddingTop()
        {
            return (bp == null ? 0 : bp.GetPaddingTop(false));
        }

        public int GetPaddingLeft()
        {
            return (bp == null ? 0 : bp.GetPaddingLeft(false));
        }

        public int GetPaddingBottom()
        {
            return (bp == null ? 0 : bp.GetPaddingBottom(false));
        }

        public int GetPaddingRight()
        {
            return (bp == null ? 0 : bp.GetPaddingRight(false));
        }

        public int GetBorderTopWidth()
        {
            return (bp == null ? 0 : bp.GetBorderTopWidth(false));
        }

        public int GetBorderRightWidth()
        {
            return (bp == null ? 0 : bp.GetBorderRightWidth(false));
        }

        public int GetBorderLeftWidth()
        {
            return (bp == null ? 0 : bp.GetBorderLeftWidth(false));
        }

        public int GetBorderBottomWidth()
        {
            return (bp == null ? 0 : bp.GetBorderBottomWidth(false));
        }

        public int GetTableCellXOffset()
        {
            return tableCellXOffset;
        }

        public void SetTableCellXOffset(int offset)
        {
            tableCellXOffset = offset;
        }

        public int GetAbsoluteHeight()
        {
            return absoluteYTop + GetPaddingTop() + GetBorderTopWidth() + currentHeight;
        }

        public void SetAbsoluteHeight(int value)
        {
            absoluteYTop = value;
        }

        public void IncreaseHeight(int amount)
        {
            this.currentHeight += amount;
        }

        public void RemoveChild(Area area)
        {
            this.currentHeight -= area.GetHeight();
            this.children.Remove(area);
        }

        public void RemoveChild(DisplaySpace spacer)
        {
            this.currentHeight -= spacer.GetSize();
            this.children.Remove(spacer);
        }

        public void Remove()
        {
            this.parent.RemoveChild(this);
        }

        public virtual void SetPage(Page page)
        {
            this.page = page;
        }

        public void SetBackground(BackgroundProps bg)
        {
            this.background = bg;
        }

        public void SetBorderAndPadding(BorderAndPadding bp)
        {
            this.bp = bp;
        }

        public virtual int SpaceLeft()
        {
            return maxHeight - currentHeight;
        }

        public virtual void Start()
        {
        }

        public virtual void SetHeight(int height)
        {
            if (height > currentHeight)
            {
                currentHeight = height;
            }

            if (currentHeight > GetMaxHeight())
            {
                currentHeight = GetMaxHeight();
            }
        }

        public void SetMaxHeight(int height)
        {
            this.maxHeight = height;
        }

        public Area GetParent()
        {
            return this.parent;
        }

        public void SetParent(Area parent)
        {
            this.parent = parent;
        }

        public virtual void SetIDReferences(IDReferences idReferences)
        {
            this.idReferences = idReferences;
        }

        public virtual IDReferences GetIDReferences()
        {
            return idReferences;
        }

        public FObj GetfoCreator()
        {
            return this.foCreator;
        }

        public AreaContainer GetNearestAncestorAreaContainer()
        {
            Area area = this.GetParent();
            while (area != null && !(area is AreaContainer))
            {
                area = area.GetParent();
            }
            return (AreaContainer)area;
        }

        public BorderAndPadding GetBorderAndPadding()
        {
            return bp;
        }

        public void AddMarker(Marker marker)
        {
            markers.Add(marker);
        }

        public void AddMarkers(ArrayList markers)
        {
            foreach (object o in markers)
            {
                this.markers.Add(o);
            }
        }

        public void AddLineagePair(FObj fo, int areaPosition)
        {
            returnedBy.Add(fo, areaPosition);
        }

        public ArrayList GetMarkers()
        {
            return markers;
        }

        public void SetGeneratedBy(FObj generatedBy)
        {
            this.generatedBy = generatedBy;
        }

        public FObj GetGeneratedBy()
        {
            return generatedBy;
        }

        public void IsFirst(bool isFirst)
        {
            _isFirst = isFirst;
        }

        public bool IsFirst()
        {
            return _isFirst;
        }

        public void IsLast(bool isLast)
        {
            _isLast = isLast;
        }

        public bool IsLast()
        {
            return _isLast;
        }
    }
}