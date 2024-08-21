#region using directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GridViewProcessor;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS.ListControl
{
    public partial class ListDisplay : UserControl
    {
        #region Members
        #endregion Members
        #region Accessors
        private ListBase listBase
        {
            get { return (ListBase)this.Page; }
        }
        private string CS
        {
            get { return listBase.CS; }
        }
        private string IdLstTemplate
        {
            get { return listBase.idLstTemplate; }
        }
        private string IdLstConsult
        {
            get { return listBase.idLstConsult; }
        }
        private Int32 IdA
        {
            get { return listBase.idA; }
        }
        private string ParentGUID
        {
            get { return listBase.parentGUID; }
        }
        #endregion Accessors
        protected void Page_Load(object sender, EventArgs e)
        {
            CreateControls();
        }

        #region Methods
        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            this.ID = "uc_lstdisplay";
            lblAvailableColumnList.InnerText = " " + Ressource.GetString("lblAvailableColumnList");
            lblSelectedColumnList.InnerText = " " + Ressource.GetString("lblSelectedColumnList");
            lblSortGroupColumnList.InnerText = " " + Ressource.GetString("lblSortGroupColumnList");
            string scriptRecord = String.Format("RecordDisplay('#{0}_blColumnDisplayed', '#{1}hidLstDisplay');RecordDisplay('#{0}_blColumnSorted', '#{1}hidLstSorted');",
                this.ClientID, ((PageBase)this.Page).ContentPlaceHolder_ClientID);
            btnOk.Attributes.Add("onclick", scriptRecord);
            btnOk.ServerClick += new EventHandler(OnValid);
            btnCancel.ServerClick += new EventHandler(OnCancel);
            btnOkAndSave.Attributes.Add("onclick", scriptRecord);
            btnOkAndSave.ServerClick += new EventHandler(OnValid);

            base.OnInit(e);
        }
        protected void CreateControls()
        {
            DropDownList ddlGroupingSet = null;
            Panel pnlGroupingSet = new Panel();
            //pnlGroupingSet.EnableViewState = false;
            pnlGroupingSet.CssClass = "form-group form-group-xs";

            Label lblGroupingSet = new Label();
            lblGroupingSet.Text = Ressource.GetString("GroupingSet");
            lblGroupingSet.CssClass = "col-sm-4 control-label";
            pnlGroupingSet.Controls.Add(lblGroupingSet);

            ddlGroupingSet = new DropDownList();
            //ddlGroupingSet.EnableViewState = true;
            ddlGroupingSet.ID = "ddlGroupingSet";
            ddlGroupingSet.CssClass = "form-control input-xs";

            ListItem item = new ListItem("", Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.Unknown));
            ddlGroupingSet.Items.Add(item);

            item = new ListItem(Ressource.GetString(String.Format("GroupingSet{0}", Cst.GroupingSet.TotalDetails)),
                Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.TotalDetails));
            ddlGroupingSet.Items.Add(item);

            item = new ListItem(Ressource.GetString(String.Format("GroupingSet{0}", Cst.GroupingSet.TotalSubTotalDetails)),
                Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.TotalSubTotalDetails));
            ddlGroupingSet.Items.Add(item);

            item = new ListItem(Ressource.GetString(String.Format("GroupingSet{0}", Cst.GroupingSet.TotalSubTotal)),
                Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.TotalSubTotal));
            ddlGroupingSet.Items.Add(item);

            item = new ListItem(Ressource.GetString(String.Format("GroupingSet{0}", Cst.GroupingSet.Total)),
                Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.Total));
            ddlGroupingSet.Items.Add(item);

            Panel pnl = new Panel();
            pnl.CssClass = "col-sm-8";
            pnl.Controls.Add(ddlGroupingSet);
            pnlGroupingSet.Controls.Add(pnl);
            plhColumnsSorted.Controls.AddAt(0, pnlGroupingSet);
        }
        public void LoadDataControls()
        {
            if (null != listBase)
            {
                listBase.LoadBulletedListAvailable(plhColumnsAvailable, LstSelectionTypeEnum.DISP);
                if (listBase.isSQLSource)
                {
                    listBase.LoadBulletedListSelected(plhColumnsDisplayed, LstSelectionTypeEnum.DISP);
                    listBase.LoadBulletedListSelected(plhColumnsSorted, LstSelectionTypeEnum.SORT);
                }
            }
        }
        #region OnCancel
        public void OnCancel(object sender, EventArgs e)
        {
            // Nothing TO DO
            //plhColumnsDisplayed.Controls.Clear();
            //plhColumnsSorted.Controls.Clear();
            //plhColumnsAvailable.Controls.Clear();
        }
        #endregion OnCancel
        #region OnValid
        public void OnValid(object sender, EventArgs e)
        {
            if (null != listBase)
            {
                listBase.isClose = true;
                listBase.SaveDisplayedOrSortedData(plhColumnsDisplayed, LstSelectionTypeEnum.DISP);
                listBase.SaveDisplayedOrSortedData(plhColumnsSorted, LstSelectionTypeEnum.SORT);

                Control ctrl = sender as Control;
                switch (ctrl.ID)
                {
                    case "btnOk":
                        RepositoryWeb.WriteTemplateSession(this.Page, listBase.idLstConsult, listBase.idLstTemplate, listBase.idA, listBase.parentGUID);
                        break;
                    case "btnOkAndSave":
                        listBase.initialNameLstTemplate = listBase.consult.template.IDLSTTEMPLATE_WithoutPrefix;
                        listBase.newNameLstTemplate = listBase.consult.template.IDLSTTEMPLATE_WithoutPrefix;
                        listBase.SaveTemplate();
                        break;
                }
            }
        }
        #endregion OnValid

        #endregion Methods

    }
}