namespace Fonet.Fo.Flow
{
    using System;
    using System.Collections;
    using Fonet.DataTypes;
    using Fonet.Fo.Properties;
    using Fonet.Layout;

    internal abstract class AbstractTableBody : FObj
    {
        protected int spaceBefore;
        protected int spaceAfter;
        protected string id;
        protected ArrayList columns;
        protected RowSpanMgr rowSpanMgr;
        protected AreaContainer areaContainer;

        public AbstractTableBody(FObj parent, PropertyList propertyList) : base(parent, propertyList)
        {
            if (!(parent is Table))
            {
                FonetDriver.ActiveDriver.FireFonetError(
                    "A table body must be child of fo:table, not " + parent.GetName());
            }
        }

        public void SetColumns(ArrayList columns)
        {
            this.columns = columns;
        }

        public virtual void SetYPosition(int value)
        {
            areaContainer.SetYPosition(value);
        }

        public virtual int GetYPosition()
        {
            return areaContainer.GetCurrentYPosition();
        }

        public int GetHeight()
        {
            return areaContainer.GetHeight() + spaceBefore + spaceAfter;
        }

        // EG 20180423 Analyse du code Correction [CA2200]
        public override Status Layout(Area area)
        {
            if (this.marker == MarkerBreakAfter)
            {
                return new Status(Status.OK);
            }

            if (this.marker == MarkerStart)
            {
                _ = propMgr.GetAccessibilityProps();
                _ = propMgr.GetAuralProps();
                _ = propMgr.GetBorderAndPadding();
                _ = propMgr.GetBackgroundProps();
                _ = propMgr.GetRelativePositionProps();

                this.spaceBefore = this.properties.GetProperty("space-before.optimum").GetLength().MValue();
                this.spaceAfter = this.properties.GetProperty("space-after.optimum").GetLength().MValue();
                this.id = this.properties.GetProperty("id").GetString();

                try
                {
                    area.GetIDReferences().CreateID(id);
                }
                catch (FonetException)
                {
                    throw;
                }

                if (area is BlockArea)
                {
                    area.End();
                }

                if (rowSpanMgr == null)
                {
                    rowSpanMgr = new RowSpanMgr(columns.Count);
                }

                this.marker = 0;
            }

            if ((spaceBefore != 0) && (this.marker == 0))
            {
                area.IncreaseHeight(spaceBefore);
            }

            if (marker == 0)
            {
                area.GetIDReferences().ConfigureID(id, area);
            }

            int spaceLeft = area.SpaceLeft();

            this.areaContainer =
                new AreaContainer(propMgr.GetFontState(area.GetFontInfo()), 0,
                                  area.GetContentHeight(),
                                  area.GetContentWidth(),
                                  area.SpaceLeft(), Position.RELATIVE);
            areaContainer.foCreator = this;
            areaContainer.SetPage(area.GetPage());
            areaContainer.SetParent(area);
            areaContainer.SetBackground(propMgr.GetBackgroundProps());
            areaContainer.SetBorderAndPadding(propMgr.GetBorderAndPadding());
            areaContainer.Start();

            areaContainer.SetAbsoluteHeight(area.GetAbsoluteHeight());
            areaContainer.SetIDReferences(area.GetIDReferences());

            Hashtable keepWith = new Hashtable();
            int numChildren = this.children.Count;
            TableRow lastRow = null;
            bool endKeepGroup = true;
            for (int i = this.marker; i < numChildren; i++)
            {
                Object child = children[i];
                if (child is Marker marker1)
                {
                    marker1.Layout(area);
                    continue;
                }
                if (!(child is TableRow))
                {
                    throw new FonetException("Currently only Table Rows are supported in table body, header and footer");
                }
                TableRow row = (TableRow)child;

                row.SetRowSpanMgr(rowSpanMgr);
                row.SetColumns(columns);
                row.DoSetup(areaContainer);
                if ((row.GetKeepWithPrevious().GetKeepType() != KeepValue.KEEP_WITH_AUTO ||
                    row.GetKeepWithNext().GetKeepType() != KeepValue.KEEP_WITH_AUTO ||
                    row.GetKeepTogether().GetKeepType() != KeepValue.KEEP_WITH_AUTO) &&
                    lastRow != null && !keepWith.Contains(lastRow))
                {
                    keepWith.Add(lastRow, null);
                }
                else
                {
                    if (endKeepGroup && keepWith.Count > 0)
                    {
                        keepWith = new Hashtable();
                    }
                    if (endKeepGroup && i > this.marker)
                    {
                        rowSpanMgr.SetIgnoreKeeps(false);
                    }
                }

                bool bRowStartsArea = (i == this.marker);
                if (bRowStartsArea == false && keepWith.Count > 0)
                {
                    if (children.IndexOf(keepWith[0]) == this.marker)
                    {
                        bRowStartsArea = true;
                    }
                }
                row.SetIgnoreKeepTogether(bRowStartsArea && StartsAC(area));
                Status status = row.Layout(areaContainer);
                if (status.IsIncomplete())
                {
                    if (status.IsPageBreak())
                    {
                        this.marker = i;
                        area.AddChild(areaContainer);

                        area.IncreaseHeight(areaContainer.GetHeight());
                        if (i == numChildren - 1)
                        {
                            this.marker = MarkerBreakAfter;
                            if (spaceAfter != 0)
                            {
                                area.IncreaseHeight(spaceAfter);
                            }
                        }
                        return status;
                    }
                    if ((keepWith.Count > 0)
                        && (!rowSpanMgr.IgnoreKeeps()))
                    {
                        row.RemoveLayout(areaContainer);
                        foreach (TableRow tr in keepWith.Keys)
                        {
                            tr.RemoveLayout(areaContainer);
                            i--;
                        }
                        if (i == 0)
                        {
                            ResetMarker();

                            rowSpanMgr.SetIgnoreKeeps(true);

                            return new Status(Status.AREA_FULL_NONE);
                        }
                    }
                    this.marker = i;
                    if ((i != 0) && (status.GetCode() == Status.AREA_FULL_NONE))
                    {
                        status = new Status(Status.AREA_FULL_SOME);
                    }
                    if (!((i == 0) && (areaContainer.GetContentHeight() <= 0)))
                    {
                        area.AddChild(areaContainer);

                        area.IncreaseHeight(areaContainer.GetHeight());
                    }

                    rowSpanMgr.SetIgnoreKeeps(true);

                    return status;
                }
                else if (status.GetCode() == Status.KEEP_WITH_NEXT
                    || rowSpanMgr.HasUnfinishedSpans())
                {
                    keepWith.Add(row, null);
                    endKeepGroup = false;
                }
                else
                {
                    endKeepGroup = true;
                }
                lastRow = row;
                area.SetMaxHeight(area.GetMaxHeight() - spaceLeft
                    + this.areaContainer.GetMaxHeight());
                spaceLeft = area.SpaceLeft();
            }
            area.AddChild(areaContainer);
            areaContainer.End();

            area.IncreaseHeight(areaContainer.GetHeight());

            if (spaceAfter != 0)
            {
                area.IncreaseHeight(spaceAfter);
                area.SetMaxHeight(area.GetMaxHeight() - spaceAfter);
            }

            if (area is BlockArea)
            {
                area.Start();
            }

            return new Status(Status.OK);
        }

        internal void RemoveLayout(Area area)
        {
            if (areaContainer != null)
            {
                area.RemoveChild(areaContainer);
            }
            if (spaceBefore != 0)
            {
                area.IncreaseHeight(-spaceBefore);
            }
            if (spaceAfter != 0)
            {
                area.IncreaseHeight(-spaceAfter);
            }
            this.ResetMarker();
            this.RemoveID(area.GetIDReferences());
        }

        private bool StartsAC(Area area)
        {
            Area parent;
            while ((parent = area.GetParent()) != null &&
                parent.HasNonSpaceChildren() == false)
            {
                if (parent is AreaContainer container &&
                    container.GetPosition() == Position.ABSOLUTE)
                {
                    return true;
                }
                area = parent;
            }
            return false;
        }
    }
}