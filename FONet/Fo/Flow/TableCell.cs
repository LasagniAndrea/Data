namespace Fonet.Fo.Flow
{
    using Fonet.Fo.Properties;
    using Fonet.Layout;

    internal class TableCell : FObj
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new TableCell(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        private string id;
        private int numColumnsSpanned;
        private int numRowsSpanned;
        private int iColNumber = -1;
        protected int startOffset;
        protected int width;
        protected int beforeOffset = 0;
        protected int startAdjust = 0;
        protected int widthAdjust = 0;
        protected int borderHeight = 0;
        protected int minCellHeight = 0;
        protected int height = 0;
        protected int top;
        protected int verticalAlign;
        protected bool bRelativeAlign = false;
        private bool bSepBorders = true;
        private bool bDone = false;
        private int m_borderSeparation = 0;
        private AreaContainer cellArea;

        public TableCell(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this.name = "fo:table-cell";
            DoSetup();
        }

        public void SetStartOffset(int offset)
        {
            startOffset = offset;
        }

        public void SetWidth(int width)
        {
            this.width = width;
        }

        public int GetColumnNumber()
        {
            return iColNumber;
        }

        public int GetNumColumnsSpanned()
        {
            return numColumnsSpanned;
        }

        public int GetNumRowsSpanned()
        {
            return numRowsSpanned;
        }

        public void DoSetup()
        {
            _ = propMgr.GetAccessibilityProps();
            _ = propMgr.GetAuralProps();
            _ = propMgr.GetBorderAndPadding();
            _ = propMgr.GetBackgroundProps();
            _ = propMgr.GetRelativePositionProps();

            this.iColNumber =
                properties.GetProperty("column-number").GetNumber().IntValue();
            if (iColNumber < 0)
            {
                iColNumber = 0;
            }
            this.numColumnsSpanned =
                this.properties.GetProperty("number-columns-spanned").GetNumber().IntValue();
            if (numColumnsSpanned < 1)
            {
                numColumnsSpanned = 1;
            }
            this.numRowsSpanned =
                this.properties.GetProperty("number-rows-spanned").GetNumber().IntValue();
            if (numRowsSpanned < 1)
            {
                numRowsSpanned = 1;
            }

            this.id = this.properties.GetProperty("id").GetString();

            bSepBorders = (this.properties.GetProperty("border-collapse").GetEnum()
                == BorderCollapse.SEPARATE);

            CalcBorders(propMgr.GetBorderAndPadding());

            verticalAlign = this.properties.GetProperty("display-align").GetEnum();
            if (verticalAlign == DisplayAlign.AUTO)
            {
                bRelativeAlign = true;
                verticalAlign = this.properties.GetProperty("relative-align").GetEnum();
            }
            else
            {
                bRelativeAlign = false;
            }

            this.minCellHeight =
                this.properties.GetProperty("height").GetLength().MValue();
        }


        public override Status Layout(Area area)
        {
            _ = area.GetAbsoluteHeight();
            if (this.marker == MarkerBreakAfter)
            {
                return new Status(Status.OK);
            }

            if (this.marker == MarkerStart)
            {

                area.GetIDReferences().CreateID(id);

                this.marker = 0;
                this.bDone = false;
            }

            if (marker == 0)
            {
                area.GetIDReferences().ConfigureID(id, area);
            }

            int spaceLeft = area.SpaceLeft() - m_borderSeparation;
            this.cellArea =
                new AreaContainer(propMgr.GetFontState(area.GetFontInfo()),
                                  startOffset + startAdjust, beforeOffset,
                                  width - widthAdjust, spaceLeft,
                                  Position.RELATIVE);

            cellArea.foCreator = this;
            cellArea.SetPage(area.GetPage());
            cellArea.SetParent(area);
            cellArea.SetBorderAndPadding(
                (BorderAndPadding)propMgr.GetBorderAndPadding().Clone());
            cellArea.SetBackground(propMgr.GetBackgroundProps());
            cellArea.Start();

            cellArea.SetAbsoluteHeight(area.GetAbsoluteHeight());
            cellArea.SetIDReferences(area.GetIDReferences());
            cellArea.SetTableCellXOffset(startOffset + startAdjust);

            int numChildren = this.children.Count;
            for (int i = this.marker; bDone == false && i < numChildren; i++)
            {
                FObj fo = (FObj)children[i];
                fo.SetIsInTableCell();
                fo.ForceWidth(width);

                this.marker = i;

                Status status;
                if ((status = fo.Layout(cellArea)).IsIncomplete())
                {
                    if ((i == 0) && (status.GetCode() == Status.AREA_FULL_NONE))
                    {
                        return new Status(Status.AREA_FULL_NONE);
                    }
                    else
                    {
                        area.AddChild(cellArea);
                        return new Status(Status.AREA_FULL_SOME);
                    }
                }

                area.SetMaxHeight(area.GetMaxHeight() - spaceLeft
                    + this.cellArea.GetMaxHeight());
            }
            this.bDone = true;
            cellArea.End();
            area.AddChild(cellArea);

            if (minCellHeight > cellArea.GetContentHeight())
            {
                cellArea.SetHeight(minCellHeight);
            }

            height = cellArea.GetHeight();
            top = cellArea.GetCurrentYPosition();

            return new Status(Status.OK);
        }

        public int GetHeight()
        {
            return cellArea.GetHeight() + m_borderSeparation - borderHeight;
        }

        public void SetRowHeight(int h)
        {
            int delta = h - GetHeight();
            if (bRelativeAlign)
            {
                cellArea.IncreaseHeight(delta);
            }
            else if (delta > 0)
            {
                BorderAndPadding cellBP = cellArea.GetBorderAndPadding();
                switch (verticalAlign)
                {
                    case DisplayAlign.CENTER:
                        cellArea.ShiftYPosition(delta / 2);
                        cellBP.SetPaddingLength(BorderAndPadding.TOP,cellBP.GetPaddingTop(false) + delta / 2);
                        cellBP.SetPaddingLength(BorderAndPadding.BOTTOM,cellBP.GetPaddingBottom(false) + delta - delta / 2);
                        break;
                    case DisplayAlign.AFTER:
                        cellBP.SetPaddingLength(BorderAndPadding.TOP,cellBP.GetPaddingTop(false) + delta);
                        cellArea.ShiftYPosition(delta);
                        break;
                    case DisplayAlign.BEFORE:
                        cellBP.SetPaddingLength(BorderAndPadding.BOTTOM,cellBP.GetPaddingBottom(false) + delta);
                        break;
                    default:
                        break;
                }
            }
        }

        private void CalcBorders(BorderAndPadding bp)
        {
            if (this.bSepBorders)
            {
                int iSep = properties.GetProperty("border-separation.inline-progression-direction").GetLength().MValue();
                this.startAdjust = iSep / 2 + bp.GetBorderLeftWidth(false) + bp.GetPaddingLeft(false);
                this.widthAdjust = startAdjust + iSep - iSep / 2 + bp.GetBorderRightWidth(false) + bp.GetPaddingRight(false);
                m_borderSeparation = properties.GetProperty("border-separation.block-progression-direction").GetLength().MValue();
                this.beforeOffset = m_borderSeparation / 2 + bp.GetBorderTopWidth(false) + bp.GetPaddingTop(false);

            }
            else
            {
                int borderStart = bp.GetBorderLeftWidth(false);
                int borderEnd = bp.GetBorderRightWidth(false);
                int borderBefore = bp.GetBorderTopWidth(false);
                int borderAfter = bp.GetBorderBottomWidth(false);

                this.startAdjust = borderStart / 2 + bp.GetPaddingLeft(false);

                this.widthAdjust = startAdjust + borderEnd / 2 + bp.GetPaddingRight(false);
                this.beforeOffset = borderBefore / 2 + bp.GetPaddingTop(false);
                this.borderHeight = (borderBefore + borderAfter) / 2;
            }
        }
    }
}