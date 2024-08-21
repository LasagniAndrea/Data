#region using directives
using System;
using System.Collections;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Runtime.InteropServices;
#endregion using directives
namespace EFS.Controls
{

    /// <summary>
    /// 
    /// </summary>
    public enum PagingTypeEnum
    {
        /// <summary>
        /// Pagination SQL, seules les lignes de la page courante sont stockées dans la source de donnée [Dataset] 
        /// </summary>
        CustomPaging,
        /// <summary>
        /// Pagination native, toutes les lignes du jeu de résulat sont stockées dans la source de donnée [Dataset]
        /// </summary>
        NativePaging,
    }

    /// <summary>
    /// Extension du DataPager pour compatiblité Bootstrap 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class UnorderedListDataPager : DataPager
    {
        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Ul; }
        }

        public UnorderedListDataPager()
            : base()
        {
            NextPreviousPagerField npfield = new NextPreviousPagerField();
            npfield.ButtonCssClass = "command";
            npfield.FirstPageText = "&laquo;";
            npfield.PreviousPageText = "‹";
            npfield.RenderDisabledButtonsAsLabels = true;
            npfield.RenderDisabledButtonsAsLabels = true;
            npfield.ShowFirstPageButton = true;
            npfield.ShowPreviousPageButton = true;
            npfield.ShowLastPageButton = false;
            npfield.ShowNextPageButton = false;
            this.Fields.Add(npfield);

            NumericPagerField numField = new NumericPagerField();
            numField.ButtonCount = 10;
            numField.CurrentPageLabelCssClass = "current";
            numField.NumericButtonCssClass = "command";
            numField.NextPreviousButtonCssClass = "command";
            this.Fields.Add(numField);

            npfield = new NextPreviousPagerField();
            npfield.ButtonCssClass = "command";
            npfield.LastPageText = "&raquo;";
            npfield.NextPageText = "›";
            npfield.RenderDisabledButtonsAsLabels = true;
            npfield.RenderDisabledButtonsAsLabels = true;
            npfield.ShowFirstPageButton = false;
            npfield.ShowPreviousPageButton = false;
            npfield.ShowLastPageButton = true;
            npfield.ShowNextPageButton = true;
            this.Fields.Add(npfield);

            TemplatePagerField tpfield = new TemplatePagerField();
            tpfield.PagerTemplate = new RecordPagerTemplate();
            this.Fields.Add(tpfield);

        }
        /// <summary>
        /// Transformation du DataPager de base (ASP) pour obtenir une UI de type Bootstrap
        /// </summary>
        /// <param name="writer"></param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (HasControls())
            {
                foreach (Control child in Controls)
                {
                    var item = child as DataPagerFieldItem;
                    if (item == null || !item.HasControls())
                    {
                        if ((child is HtmlGenericControl)) // && ("record" == ((Label)child).CssClass))
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Li);
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                            child.RenderControl(writer);
                            writer.RenderEndTag();
                        }
                        else
                        {
                            child.RenderControl(writer);
                        }
                        continue;
                    }

                    foreach (Control ctrl in item.Controls)
                    {
                        var space = ctrl as LiteralControl;
                        if (space != null && space.Text == "&nbsp;")
                            continue;

                        if (ctrl is Label)
                        {
                            Label lbl = ctrl as Label;
                            switch (lbl.CssClass)
                            {
                                case "current":
                                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "active");
                                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                                    break;
                                case "command":
                                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "disabled");
                                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                                    break;
                            }
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                            writer.RenderBeginTag(HtmlTextWriterTag.A);
                            ctrl.RenderControl(writer);
                            writer.RenderEndTag();
                            writer.RenderEndTag();
                        }
                        else
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Li);
                            ctrl.RenderControl(writer);
                            writer.RenderEndTag();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A template that goes within a data pager template field to display record count information.
    /// </summary>
    internal class RecordPagerTemplate : ITemplate
    {
        /// <summary>
        /// Instantiates this template within a parent control.
        /// </summary>
        /// <param name="container"></param>
        public void InstantiateIn(Control container)
        {
            DataPager pager = container.NamingContainer as DataPager;

            if (null != pager)
            {
                HtmlGenericControl lbl = new HtmlGenericControl("span");
                lbl.Attributes.Add("class", "record");
                lbl.InnerHtml = String.Format("{0} {1} to {2} of {3}",
                        @"<i class='glyphicon glyphicon-info-sign'></i>",
                        pager.StartRowIndex + 1,
                        Math.Min(pager.StartRowIndex + pager.PageSize, pager.TotalRowCount),
                        pager.TotalRowCount);
                pager.Controls.Add(lbl);
            }
        }
    }

    public partial class GridViewTemplate
    {
        public event EventHandler<PageEventArgs> TotalRowCountAvailable;

        public int MaximumRows
        {
            get { return this.PageSize; }
        }
        public int StartRowIndex
        {
            get { return this.PageSize * this.PageIndex; }
        }

        protected virtual void OnTotalRowCountAvailable(PageEventArgs e)
        {
            if (TotalRowCountAvailable != null)
                TotalRowCountAvailable(this, e);
        }

        public void SetPageProperties(int pStartRowIndex, int pMaximumRows, bool pDataBind)
        {
            int newPageIndex = (pStartRowIndex / pMaximumRows);
            if (PageIndex != newPageIndex)
            {
                PageSize = pMaximumRows;
                OnPageIndexChanging(new GridViewPageEventArgs(newPageIndex));
                RequiresDataBinding = true;
            }
        }

        protected override int CreateChildControls(IEnumerable pDataSource, bool pDataBinding)
        {
            int baseResult = base.CreateChildControls(pDataSource, pDataBinding);
            if (null != pDataSource)
            {
                int dataSourceCount = (IsBoundUsingDataSourceID && pDataBinding) ? GetTotalRowsFromDataSource(pDataSource) : GetSourceCount(pDataSource);
                OnTotalRowCountAvailable(new PageEventArgs(StartRowIndex, MaximumRows, dataSourceCount));
            }
            return baseResult;
        }

        private int GetTotalRowsFromDataSource(IEnumerable pDataSource)
        {
            DataSourceView view = this.GetData();
            if (AllowPaging && view.CanPage && view.CanRetrieveTotalRowCount)
                return base.SelectArguments.TotalRowCount;
            else
                return (PageIndex * PageSize) + GetSourceCount(pDataSource);
        }

        //Gets the row count from a manually bound source or from a source in viewstate
        private int GetSourceCount(IEnumerable pDataSource)
        {
            ICollection source = pDataSource as ICollection;

            return source != null ?
                 source.Count :
                 (from x in pDataSource.OfType<object>() select 1).Sum();
        }
        /* END PAGER */

        public Unit GridHeight { get; set; }

        private String CalculateWidth()
        {
            string strWidth = "auto";
            if (!this.Width.IsEmpty)
            {
                strWidth = String.Format("{0}{1}", this.Width.Value, ((this.Width.Type == UnitType.Percentage) ? "%" : "px"));
            }
            return strWidth;
        }

        private String CalculateHeight()
        {
            string strHeight = "200px";
            if (!this.GridHeight.IsEmpty)
            {
                strHeight = String.Format("{0}{1}", this.GridHeight.Value, ((this.GridHeight.Type == UnitType.Percentage) ? "%" : "px"));
            }
            return strHeight;
        }
    }
}
