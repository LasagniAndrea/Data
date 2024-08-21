using System;
using System.Collections;
using Fonet.DataTypes;
using Fonet.Fo;
using Fonet.Fo.Flow;
using Fonet.Fo.Properties;
using Fonet.Render.Pdf;

namespace Fonet.Layout
{
    internal class BodyAreaContainer : Area
    {
        private int xPosition;
        private int yPosition;
        private readonly int position;
        private readonly int columnCount;
        private readonly int columnGap;
        private readonly AreaContainer mainReferenceArea;
        private readonly AreaContainer beforeFloatReferenceArea;
        private readonly AreaContainer footnoteReferenceArea;
        private readonly int mainRefAreaHeight;
        private readonly int beforeFloatRefAreaHeight;
        private readonly int footnoteRefAreaHeight;
        private readonly int mainYPosition = 0;
        private int footnoteYPosition;
        private bool _isNewSpanArea;
        private int footnoteState = 0;

        public BodyAreaContainer(FontState fontState, int xPosition,
                                 int yPosition, int allocationWidth,
                                 int maxHeight, int position, int columnCount,
                                 int columnGap)
            : base(fontState, allocationWidth, maxHeight)
        {
            this.xPosition = xPosition;
            this.yPosition = yPosition;
            this.position = position;
            this.columnCount = columnCount;
            this.columnGap = columnGap;

            beforeFloatRefAreaHeight = 0;
            footnoteRefAreaHeight = 0;
            mainRefAreaHeight = maxHeight - beforeFloatRefAreaHeight
                - footnoteRefAreaHeight;
            beforeFloatReferenceArea = new AreaContainer(fontState, xPosition,
                                                         yPosition, allocationWidth, beforeFloatRefAreaHeight,
                                                         Position.ABSOLUTE);
            beforeFloatReferenceArea.SetAreaName("before-float-reference-area");
            this.AddChild(beforeFloatReferenceArea);
            mainReferenceArea = new AreaContainer(fontState, xPosition,
                                                  yPosition, allocationWidth,
                                                  mainRefAreaHeight,
                                                  Position.ABSOLUTE);
            mainReferenceArea.SetAreaName("main-reference-area");
            this.AddChild(mainReferenceArea);
            int footnoteRefAreaYPosition = yPosition - mainRefAreaHeight;
            footnoteReferenceArea = new AreaContainer(fontState, xPosition,
                                                      footnoteRefAreaYPosition,
                                                      allocationWidth,
                                                      footnoteRefAreaHeight,
                                                      Position.ABSOLUTE);
            footnoteReferenceArea.SetAreaName("footnote-reference-area");
            this.AddChild(footnoteReferenceArea);

        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderBodyAreaContainer(this);
        }

        public int GetPosition()
        {
            return position;
        }

        public int GetXPosition()
        {
            return xPosition + GetPaddingLeft() + GetBorderLeftWidth();
        }

        public void SetXPosition(int value)
        {
            xPosition = value;
        }

        public int GetYPosition()
        {
            return yPosition + GetPaddingTop() + GetBorderTopWidth();
        }

        public void SetYPosition(int value)
        {
            yPosition = value;
        }

        public AreaContainer GetMainReferenceArea()
        {
            return mainReferenceArea;
        }

        public AreaContainer GetBeforeFloatReferenceArea()
        {
            return beforeFloatReferenceArea;
        }

        public AreaContainer GetFootnoteReferenceArea()
        {
            return footnoteReferenceArea;
        }

        public override void SetIDReferences(IDReferences idReferences)
        {
            mainReferenceArea.SetIDReferences(idReferences);
        }

        public override IDReferences GetIDReferences()
        {
            return mainReferenceArea.GetIDReferences();
        }

        public AreaContainer GetNextArea(FObj fo)
        {
            _isNewSpanArea = false;

            int span = Span.NONE;
            if (fo is Block block)
            {
                span = block.GetSpan();
            }
            else if (fo is BlockContainer container)
            {
                span = container.GetSpan();
            }

            if (this.mainReferenceArea.GetChildren().Count == 0)
            {
                if (span == Span.ALL)
                {
                    return AddSpanArea(1);
                }
                else
                {
                    return AddSpanArea(columnCount);
                }
            }

            ArrayList spanAreas = this.mainReferenceArea.GetChildren();
            SpanArea spanArea = (SpanArea)spanAreas[spanAreas.Count - 1];

            if ((span == Span.ALL) && (spanArea.GetColumnCount() == 1))
            {
                return spanArea.GetCurrentColumnArea();
            }
            else if ((span == Span.NONE)
                && (spanArea.GetColumnCount() == columnCount))
            {
                return spanArea.GetCurrentColumnArea();
            }
            else if (span == Span.ALL)
            {
                return AddSpanArea(1);
            }
            else if (span == Span.NONE)
            {
                return AddSpanArea(columnCount);
            }
            else
            {
                throw new FonetException("BodyAreaContainer::getNextArea(): Span attribute messed up");
            }
        }

        private AreaContainer AddSpanArea(int numColumns)
        {
            ResetHeights();
            int spanAreaYPosition = GetYPosition()
                - this.mainReferenceArea.GetContentHeight();

            SpanArea spanArea = new SpanArea(fontState, GetXPosition(),
                                             spanAreaYPosition, allocationWidth,
                                             GetRemainingHeight(), numColumns,
                                             columnGap);
            this.mainReferenceArea.AddChild(spanArea);
            spanArea.SetPage(this.GetPage());
            this._isNewSpanArea = true;
            return spanArea.GetCurrentColumnArea();
        }

        public bool IsBalancingRequired(FObj fo)
        {
            if (this.mainReferenceArea.GetChildren().Count == 0)
            {
                return false;
            }

            ArrayList spanAreas = this.mainReferenceArea.GetChildren();
            SpanArea spanArea = (SpanArea)spanAreas[spanAreas.Count - 1];

            if (spanArea.IsBalanced())
            {
                return false;
            }

            int span = Span.NONE;
            if (fo is Block block)
            {
                span = block.GetSpan();
            }
            else if (fo is BlockContainer container)
            {
                span = container.GetSpan();
            }

            if ((span == Span.ALL) && (spanArea.GetColumnCount() == 1))
            {
                return false;
            }
            else if ((span == Span.NONE)
                && (spanArea.GetColumnCount() == columnCount))
            {
                return false;
            }
            else if (span == Span.ALL)
            {
                return true;
            }
            else if (span == Span.NONE)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        public void ResetSpanArea()
        {
            ArrayList spanAreas = this.mainReferenceArea.GetChildren();
            SpanArea spanArea = (SpanArea)spanAreas[spanAreas.Count - 1];

            if (!spanArea.IsBalanced())
            {
                int newHeight = spanArea.GetTotalContentHeight()
                    / spanArea.GetColumnCount();
                newHeight += 2 * 15600;

                this.mainReferenceArea.RemoveChild(spanArea);
                ResetHeights();
                SpanArea newSpanArea = new SpanArea(fontState, GetXPosition(),
                                                    spanArea.GetYPosition(),
                                                    allocationWidth, newHeight,
                                                    spanArea.GetColumnCount(),
                                                    columnGap);
                this.mainReferenceArea.AddChild(newSpanArea);
                newSpanArea.SetPage(this.GetPage());
                newSpanArea.SetIsBalanced();
                this._isNewSpanArea = true;
            }
            else
            {
                throw new Exception("Trying to balance balanced area");
            }
        }

        public int GetRemainingHeight()
        {
            return this.mainReferenceArea.GetMaxHeight()
                - this.mainReferenceArea.GetContentHeight();
        }

        private void ResetHeights()
        {
            int totalHeight = 0;
            foreach (SpanArea spanArea in mainReferenceArea.GetChildren())
            {
                int spanContentHeight = spanArea.GetMaxContentHeight();
                int spanMaxHeight = spanArea.GetMaxHeight();

                totalHeight += (spanContentHeight < spanMaxHeight)
                    ? spanContentHeight : spanMaxHeight;
            }
            mainReferenceArea.SetHeight(totalHeight);
        }

        public bool IsLastColumn()
        {
            ArrayList spanAreas = this.mainReferenceArea.GetChildren();
            SpanArea spanArea = (SpanArea)spanAreas[spanAreas.Count - 1];
            return spanArea.IsLastColumn();
        }

        public bool IsNewSpanArea()
        {
            return _isNewSpanArea;
        }

        public AreaContainer GetCurrentColumnArea()
        {
            ArrayList spanAreas = this.mainReferenceArea.GetChildren();
            SpanArea spanArea = (SpanArea)spanAreas[spanAreas.Count - 1];
            return spanArea.GetCurrentColumnArea();
        }

        public int GetFootnoteState()
        {
            return footnoteState;
        }

        public bool NeedsFootnoteAdjusting()
        {
            footnoteYPosition = footnoteReferenceArea.GetYPosition();
            switch (footnoteState)
            {
                case 0:
                    ResetHeights();
                    if (footnoteReferenceArea.GetHeight() > 0
                        && mainYPosition + mainReferenceArea.GetHeight()
                            > footnoteYPosition)
                    {
                        return true;
                    }
                    break;
                case 1:
                    break;
            }
            return false;
        }

        public void AdjustFootnoteArea()
        {
            footnoteState++;
            if (footnoteState == 1)
            {
                mainReferenceArea.SetMaxHeight(footnoteReferenceArea.GetYPosition()
                    - mainYPosition);
                footnoteYPosition = footnoteReferenceArea.GetYPosition();
                footnoteReferenceArea.SetMaxHeight(footnoteReferenceArea.GetHeight());

                foreach (object obj in footnoteReferenceArea.GetChildren())
                {
                    if (obj is Area childArea)
                    {
                        footnoteReferenceArea.RemoveChild(childArea);
                    }
                }

                GetPage().SetPendingFootnotes(null);
            }
        }

        protected static void ResetMaxHeight(Area ar, int change)
        {
            ar.SetMaxHeight(change);
            foreach (object obj in ar.GetChildren())
            {
                if (obj is Area childArea)
                {
                    ResetMaxHeight(childArea, change);
                }
            }
        }

    }
}