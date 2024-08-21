using EFS.ACommon;
using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    #region class HtmlTools
    /// <summary>
    /// Functions to use with controls
    /// </summary>
    /// 
    public sealed class HtmlTools
    {
        #region infosCSS
        public static string cssInfosDefault = "infosDefault";
        public static string cssInfosTable = "infosTable";
        public static string cssInfosColumnName = "infosColumnName";
        public static string cssInfosOperator = "infosOperator";
        public static string cssInfosData = "infosData";
        public static string cssInfosAnd = "infosAnd";
        public static string cssInfosFilterBackground = "infosFilterBackground";
        public static string cssInfosSort = "infosSort";
        // FI 20190607 [XXXXX] Add
        public static string cssinfosMaskHide = "infosMaskHide"; 
        #endregion
        //
        #region InitRegularExpression

        private static string InitRegularExpression(string pRegularExpression, ref string pRegularExpressionMsg)
        {
            string regularExpression = string.Empty;

            if (StrFunc.IsFilled(pRegularExpression))
            {
                if (pRegularExpression.ToUpper().StartsWith("REGEX"))
                {
                    try
                    {
                        EFSRegex.TypeRegex regExType = (EFSRegex.TypeRegex)Enum.Parse(typeof(EFSRegex.TypeRegex), pRegularExpression, true);
                        regularExpression = EFSRegex.RegularExpression(regExType);
                        pRegularExpressionMsg = EFSRegex.ErrorMessage(regExType);
                    }
                    catch { };
                }
                if (StrFunc.IsEmpty(regularExpression))
                {
                    regularExpression = Ressource.GetString(pRegularExpression, string.Empty, true);
                    pRegularExpressionMsg = Ressource.GetString(pRegularExpression + "Error", "Format error : " + pRegularExpression, true);
                }
                if (StrFunc.IsEmpty(regularExpression))
                {
                    regularExpression = pRegularExpression;
                    pRegularExpressionMsg = "Format error : " + pRegularExpression;
                }
            }
            return regularExpression;
        }
        #endregion
        //
        #region AddStyleList
        private static void AddStyleList(WebControl pWebCtrl, string pStyles)
        {
            if (StrFunc.IsFilled(pStyles))
            {
                string[] styleList = pStyles.Split(';');
                foreach (string s in styleList)
                {
                    string[] styleValue = s.Split(':');
                    pWebCtrl.Style.Add(styleValue[0].Trim(), styleValue[1].Trim());
                }
            }
        }
        #endregion
        //
        #region CreateTable
        public static Table CreateTable()
        {
            return CreateTable(string.Empty, string.Empty, 0, Unit.Percentage(100), Unit.Percentage(100));
        }
        //public static Table CreateTable(string pCssClass)
        //{
        //    return CreateTable(string.Empty, pCssClass, 0, Unit.Percentage(100));
        //}
        //public static Table CreateTable(Unit pWidth)
        //{
        //    return CreateTable(pWidth, Unit.Percentage(100));
        //}
        //public static Table CreateTable(Unit pWidth, Unit pHeight)
        //{
        //    return CreateTable(string.Empty, string.Empty, 0, pWidth, pHeight);
        //}
        public static Table CreateTable(int pCellPading)
        {
            return CreateTable(string.Empty, string.Empty, pCellPading, Unit.Percentage(100), Unit.Percentage(100));
        }
        //public static Table CreateTable(string pCssClass, int pCellPading)
        //{
        //    return CreateTable(string.Empty, pCssClass, pCellPading, Unit.Percentage(100), Unit.Percentage(100));
        //}
        public static Table CreateTable(string pID, string pCssClass, int pCellPading, Unit pWidth, Unit pHeight)
        {
            Table table = new Table
            {
                Width = pWidth,
                Height = pHeight,
                CellSpacing = 0,
                CellPadding = pCellPading
            };

            if (StrFunc.IsFilled(pCssClass))
                table.CssClass = pCssClass;

            if (StrFunc.IsFilled(pID))
                table.ID = pID;

            return table;
        }
        #endregion CreateTable
        //
        #region public HTMLBold
        public static string HTMLBold(string pData)
        {
            return "<b>" + pData + "</b>";
        }
        #endregion
        #region public HTMLBold_Remove
        public static string HTMLBold_Remove(string pData)
        {
            return pData.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
        }
        #endregion
        //
        #region public static GenerateHtmlForm
        public static HtmlForm GenerateHtmlForm()
        {

            //
            int border = Convert.ToInt32(SystemSettings.GetAppSettings("Spheres_DebugDesign"));
            // Add TblMain and RowMain and CellMain
            Table table = new Table
            {
                ID = "tblForm",
                BorderColor = Color.Blue,
                BorderWidth = border,
                CellPadding = 0,
                CellSpacing = 0,
                Width = Unit.Percentage(100)
            };
            TableRow tr = new TableRow
            {
                ID = "rowForm"
            };
            TableCell td = new TableCell
            {
                ID = "cellForm"
            };
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            //
            HtmlForm form = new HtmlForm();
            form.Controls.Add(table);
            //
            return form;

        }
        #endregion
        //
        #region NewLabelInCell
        public static TableCell NewLabelInCell()
        {
            return NewLabelInCell(string.Empty);
        }
        public static TableCell NewLabelInCell(string pText)
        {
            return NewLabelInCell(pText, string.Empty);
        }
        public static TableCell NewLabelInCell(string pText, string pCss)
        {
            return NewLabelInCell(pText, pCss, string.Empty);
        }
        public static TableCell NewLabelInCell(string pText, Unit pWidth)
        {
            return NewLabelInCell(pText, EFSCssClass.Label, string.Empty, pWidth);
        }
        public static TableCell NewLabelInCell(string pText, string pCss, string pStyle)
        {
            return NewLabelInCell(pText, pCss, pStyle, Unit.Percentage(0));
        }
        public static TableCell NewLabelInCell(string pText, string pCss, string pStyle, Unit pWidth)
        {
            return NewLabelInCell(pText, string.Empty, pCss, pStyle, pWidth);
        }
        public static TableCell NewLabelInCell(string pText, string pCss, HorizontalAlign pHorizAlign)
        {
            return NewLabelInCell(pText, string.Empty, pCss, pHorizAlign);
        }
        public static TableCell NewLabelInCell(string pText, string pCss, HorizontalAlign pHorizAlign, Unit pWidth)
        {
            TableCell td = NewLabelInCell(pText, pCss, pHorizAlign);
            if (pWidth != Unit.Percentage(0))
                td.Width = pWidth;
            //
            return td;
        }
        public static TableCell NewLabelInCell(string pText, string pId, string pCss, HorizontalAlign pHorizAlign)
        {
            TableCell td = NewLabelInCell(pText, pId, pCss, string.Empty);
            td.HorizontalAlign = pHorizAlign;
            return td;
        }
        public static TableCell NewLabelInCell(string pText, string pId, string pCss, string pStyle)
        {
            return NewLabelInCell(pText, pId, pCss, pStyle, Unit.Percentage(0));
        }
        public static TableCell NewLabelInCell(string pText, string pId, string pCss, string pStyle, Unit pWidth)
        {
            return NewLabelInCell(pText, pId, pCss, pStyle, true, pWidth);
        }
        public static TableCell NewLabelInCell(string pText, string pId, string pCss, string pStyle, bool pSetData, Unit pWidth)
        {
            WCTooltipLabel lblParams = new WCTooltipLabel();
            //
            if (StrFunc.IsFilled(pCss))
                lblParams.CssClass = pCss;
            //
            if (pSetData)
            {
                if (StrFunc.IsFilled(pText))
                    lblParams.Text = pText;
                else
                    lblParams.Text = Cst.Space;
            }
            if (StrFunc.IsFilled(pStyle))
                AddStyleList(lblParams, pStyle);

            if (StrFunc.IsFilled(pId))
                lblParams.ID = pId;

            return NewControlInCell(lblParams, pWidth, HorizontalAlign.Left);
        }
        #endregion
        #region NewCheckBoxInCell
        //public static TableCell NewCheckBoxInCell (string pText, string pChcked, string pId, string pCss, string pStyle, bool pSetData) 
        //{
        //    return NewCheckBoxInCell(pText, pChcked, pId, pCss, pStyle, pSetData, Unit.Percentage(0));
        //}
        //public static TableCell NewCheckBoxInCell (string pText, string pChcked, string pId, string pCss, string pStyle, bool pSetData, Unit pWidth)
        ////GP 20070129. Surchage ajouté.
        //{
        //    return NewCheckBoxInCell(pText, pChcked, pId, pCss, pStyle, pSetData, pWidth, true);
        //}
        public static TableCell NewCheckBoxInCell(string pText, string pChcked, string pId, string pCss, string pStyle, bool pSetData, Unit pWidth, bool pIsEnabled)
        {
            CheckBox ckbParams = new CheckBox
            {
                TextAlign = TextAlign.Right,
                Enabled = pIsEnabled,
                Text = StrFunc.IsFilled(pText) ? pText : Cst.Space
            };

            if (StrFunc.IsFilled(pCss))
                ckbParams.CssClass = pCss;

            if (pSetData)
                ckbParams.Checked = BoolFunc.IsTrue(pChcked);

            if (StrFunc.IsFilled(pStyle))
                AddStyleList(ckbParams, pStyle);

            if (StrFunc.IsFilled(pId))
                ckbParams.ID = pId;

            return NewControlInCell(ckbParams, pWidth, HorizontalAlign.Left);
        }
        #endregion
        #region NewDropDownListInCell
        //public static TableCell NewDropDownListInCell ( string pId, string pCss, string pStyle)
        //{
        //    return NewDropDownListInCell ( pId, pCss, pStyle, Unit.Percentage(0));
        //}
        public static TableCell NewDropDownListInCell(string pId, string pCss, string pStyle, bool pIsAutoPostBack, EventHandler pSelectedIndexChanged)
        {
            return NewDropDownListInCell(pId, pCss, pStyle, Unit.Percentage(0), HorizontalAlign.Left, pIsAutoPostBack, pSelectedIndexChanged);
        }
        //public static TableCell NewDropDownListInCell ( string pId, string pCss, string pStyle, Unit pWidth)
        //{
        //    return NewDropDownListInCell ( pId, pCss, pStyle, pWidth,HorizontalAlign.Left, false,null);
        //}
        public static TableCell NewDropDownListInCell(string pId, string pCss, string pStyle, Unit pWidth, HorizontalAlign pHorizontalAlign, bool pIsAutoPostBack, EventHandler pSelectedIndexChanged)
        {
            WCDropDownList2 ddlParams = new WCDropDownList2
            {
                AutoPostBack = pIsAutoPostBack
            };

            if (StrFunc.IsFilled(pId))
                ddlParams.ID = pId;

            if (StrFunc.IsFilled(pCss))
                ddlParams.CssClass = pCss;

            if (StrFunc.IsFilled(pStyle))
                AddStyleList(ddlParams, pStyle);

            if (pSelectedIndexChanged != null)
                ddlParams.SelectedIndexChanged += pSelectedIndexChanged;

            return NewControlInCell(ddlParams, pWidth, pHorizontalAlign);
        }
        #endregion
        #region NewDataListInCell
        public static TableCell NewDataListInCell(string pDataList, string pId, string pCss, Unit pWidth, bool pIsMandatory, bool pIsEnabled)
        {
            string dataValue = null;
            return NewDataListInCell(pDataList, false, dataValue, pId, pCss, pWidth, pIsMandatory, pIsEnabled);
        }
        //GP 20070129 Argument bool pIsEnabled ajouté.
        //PL 20100909 Add pDataValue
        public static TableCell NewDataListInCell(string pDataList, bool pIsRessource, string pDataValue, string pId, string pCss, Unit pWidth, bool pIsMandatory, bool pIsEnabled)
        {
            WCDropDownList2 ddlList = new WCDropDownList2
            {
                ID = pId,
                CssClass = pCss,
                Enabled = pIsEnabled
            };

            ControlsTools.DDLLoad_FromListRetrieval(null, ddlList, SessionTools.CS, pDataList, !pIsMandatory, pIsRessource, string.Empty);
            if (pDataValue != null)
            {
                //PL 20100909 Select by Text, and if not found the by Value
                if (!ControlsTools.DDLSelectByText(ddlList, pDataValue))
                    ControlsTools.DDLSelectByValue(ddlList, pDataValue);
            }

            return NewControlInCell(ddlList, pWidth);
        }
        #endregion
        #region NewTextBoxInCell
        public static WCTextBox NewWCTextBox(string pText, string pId, string pCss, string pStyle, bool pSetData, bool pIsMandatory, string pRegularExpression)
        {
            WCTextBox txtbParams;
            string regularExpressionMsg = string.Empty;
            string requiredErrMsg = Ressource.GetString("MissingData", "Missing data", true);

            #region Init RegularExpression
            string regularExpression = InitRegularExpression(pRegularExpression, ref regularExpressionMsg);
            #endregion
            #region new WCTextBox()
            //Textbox with required and regularExpression 			
            if ((pIsMandatory) && ((pRegularExpression.Length > 0) || (pText.Length > 0)))
                txtbParams = new WCTextBox(pId, requiredErrMsg, regularExpression, true, regularExpressionMsg);
            //Textbox with required 
            else if (pIsMandatory)
                txtbParams = new WCTextBox(pId, requiredErrMsg, string.Empty, true, string.Empty);
            //Textbox with regularExpression 
            else if ((pRegularExpression.Length > 0) || (pText.Length > 0))
                txtbParams = new WCTextBox(pId, string.Empty, regularExpression, true, regularExpressionMsg);
            //Textbox without regularExpression
            else
                txtbParams = new WCTextBox(pId, string.Empty, string.Empty, true, string.Empty);
            #endregion new WCTextBox()

            if (StrFunc.IsFilled(pCss))
                txtbParams.CssClass = pCss;

            if (pSetData)
                txtbParams.Text = pText;

            if (StrFunc.IsFilled(pStyle))
                AddStyleList(txtbParams, pStyle);

            return txtbParams;
        }

        public static TableCell NewTextBoxInCell(string pText, string pId, string pCss, string pStyle, bool pSetData)
        {
            return NewTextBoxInCell(pText, pId, pCss, pStyle, pSetData, Unit.Percentage(0));
        }
        public static TableCell NewTextBoxInCell(string pText, string pId, string pCss, string pStyle, bool pSetData, Unit pWidth)
        {
            return NewTextBoxInCell(pText, pId, pCss, pStyle, pSetData, pWidth, false, string.Empty, true); //GP 20070129 : valeur 'true' ajouté pour le param. pIsEnabled
        }
        public static TableCell NewTextBoxInCell(string pText, string pId, string pCss, string pStyle, bool pSetData, Unit pWidth, bool pIsMandatory, string pRegularExpression)
        //GP 20070129. Surchage ajouté.
        {
            return NewTextBoxInCell(pText, pId, pCss, pStyle, pSetData, pWidth, pIsMandatory, pRegularExpression, true);
        }
        public static TableCell NewTextBoxInCell(string pText, string pId, string pCss, string pStyle, bool pSetData, Unit pWidth, bool pIsMandatory, string pRegularExpression, bool pIsEnabled)
        //GP 20070129. Argument pIsEnabled ajouté.
        {
            WCTextBox txtbParams = NewWCTextBox(pText, pId, pCss, pStyle, pSetData, pIsMandatory, pRegularExpression);
            if (pWidth.Type == UnitType.Percentage)
                txtbParams.Width = Unit.Percentage(100);
            else
                txtbParams.Width = pWidth;
            //GP 20070129. Ajouté la suivante ligne pour la proprieté ReadOnly (par ex. aux paramètres de type 'Output')
            txtbParams.ReadOnly = !pIsEnabled;

            return NewControlInCell(txtbParams, pWidth, HorizontalAlign.Left);
        }
        #endregion
        #region NewCellInCell
        /// <summary>
        /// Ajoute la TableCell dans une TableRow 
        /// </summary>
        /// <param name="pTableCell"></param>
        /// <returns></returns>
        public static TableCell NewCellInCell(TableCell pTableCell)
        {
            TableRow tr = NewControlInRow(pTableCell);

            return NewRowInCell(tr);
        }
        #endregion
        #region NewRowInCell
        public static TableCell NewRowInCell(TableRow pRow)
        {
            return NewRowInCell(pRow, Unit.Percentage(0));
        }
        /// <summary>
        ///  Ajoute {pRow} dans un table, Ajoute la table dans une cellule et applique le {pCss} à la cellule
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pCss"></param>
        /// <returns></returns>
        public static TableCell NewRowInCell(TableRow pRow, string pCss)
        {
            TableCell tableCell = NewRowInCell(pRow, Unit.Percentage(0));
            tableCell.CssClass = pCss;
            //
            return tableCell;
        }
        /// <summary>
        /// Ajoute {pRow} dans une table, applique {pWidth} à la table puis Ajoute cette table dans une nouvelle cellule
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pWidth"></param>
        /// <returns></returns>
        public static TableCell NewRowInCell(TableRow pRow, Unit pWidth)
        {
            Table table = CreateTable();
            //
            table.Controls.Add(pRow);
            table.Width = Unit.Percentage(100);
            //
            return NewControlInCell(table, pWidth);
        }
        #endregion
        //
        #region NewControlInCell
        public static TableCell NewControlInCell()
        {
            return NewControlInCell(Unit.Percentage(0));
        }
        public static TableCell NewControlInCell(Control pControl)
        {
            return NewControlInCell(pControl, Unit.Percentage(0));
        }
        public static TableCell NewControlInCell(Unit pWidth)
        {
            return NewControlInCell(null, pWidth);
        }
        public static TableCell NewControlInCell(HorizontalAlign pHorizAlign)
        {
            return NewControlInCell(null, pHorizAlign);
        }
        public static TableCell NewControlInCell(Control pControl, HorizontalAlign pHorizAlign)
        {
            return NewControlInCell(pControl, Unit.Percentage(0), pHorizAlign);
        }
        public static TableCell NewControlInCell(Control pControl, Unit pWidth, HorizontalAlign pHorizAlign)
        {
            TableCell td = NewControlInCell(pControl, pWidth);
            td.HorizontalAlign = pHorizAlign;
            return td;
        }
        public static TableCell NewControlInCell(Control pControl, Unit pWidth)
        {
            TableCell td = new TableCell();
            //
            if (pControl != null)
                td.Controls.Add(pControl);
            else
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace));
            //
            if (pWidth != Unit.Percentage(0))
                td.Width = pWidth;
            //
            return td;
        }
        #endregion
        //
        #region NewLigneInRow
        public static TableRow NewLigneInRow(Unit pRowHeight, string pRowCss)
        {
            TableRow tr = NewLigneInRow(pRowHeight);
            tr.CssClass = pRowCss;
            return tr;
        }
        public static TableRow NewLigneInRow(Unit pRowHeight)
        {
            TableRow tr = NewLigneInRow();
            tr.Height = pRowHeight;
            return tr;
        }
        public static TableRow NewLigneInRow(Unit pRowHeight, string pRowCss, int pColSpan)
        {
            TableRow tr = NewLigneInRow(pColSpan);
            tr.Height = pRowHeight;
            tr.CssClass = pRowCss;
            return tr;
        }
        public static TableRow NewLigneInRow()
        {
            return NewLigneInRow(0);
        }
        public static TableRow NewLigneInRow(int pColSpan)
        {
            TableRow tr = new TableRow();
            TableCell td = new TableCell
            {
                ColumnSpan = pColSpan
            };
            tr.Controls.Add(td);

            return tr;
        }
        #endregion
        #region NewTableInRow
        public static TableRow NewTableInRow(Table pTable)
        {
            TableRow tr = new TableRow();
            TableCell td = NewControlInCell(pTable, Unit.Percentage(100), HorizontalAlign.Left);

            tr.Controls.Add(td);

            return tr;
        }
        #endregion
        #region NewRowInRow
        public static TableRow NewRowInRow(TableRow pTableRow)
        {
            Table table = new Table();
            table.Controls.Add(pTableRow);

            return NewTableInRow(table);
        }
        #endregion
        #region NewRoundedTableInRow
        public static TableRow NewRoundedTableInRow(Table pTable, string pCss)
        {
            TableRow tr = new TableRow();
            TableCell td = NewRoundedTableInCell(pTable, pCss, Unit.Percentage(100));
            tr.Controls.Add(td);
            //
            return tr;
        }
        #endregion
        #region NewRoundedTableInCell
        public static TableCell NewRoundedTableInCell(WebControl pControl, string pCss, Unit pCellWidth)
        {
            TableCell td = NewRoundedTableInCell(pControl, pCss);
            td.Width = pCellWidth;
            //
            return td;
        }
        public static TableCell NewRoundedTableInCell(WebControl pControl, string pCss)
        {
            TableCell td;
            WCRoundedTable roundedTable = new WCRoundedTable(pCss);
            roundedTable.AddContent(pControl);
            td = NewControlInCell(roundedTable);
            return td;
        }
        #endregion

        #region NewTableHInRow
        public static TableRow NewTableHInRow(Table pTable, string pTitle)
        {
            TableRow tr = new TableRow();
            TableCell td = NewTableHInCell(pTable, pTitle);
            tr.Controls.Add(td);
            return tr;
        }
        public static TableRow NewTableHInRow(Table pTable, string pTitle, string pCssHeader, bool pIsReverse)
        {
            TableRow tr = new TableRow();
            TableCell td = NewTableHInCell(pTable, pTitle, pCssHeader, pIsReverse);
            tr.Controls.Add(td);
            return tr;
        }
        #endregion NewTableHInRow
        #region NewTableHInCell
        public static TableCell NewTableHInCell(WebControl pControl, string pTitle)
        {
            TableCell td;
            WCTogglePanel pnl = new WCTogglePanel();
            pnl.SetHeaderTitle(pTitle);
            pnl.AddContent(pControl);
            td = NewControlInCell(pnl);
            return td;
        }
        public static TableCell NewTableHInCell(WebControl pControl, string pTitle, string pCssHeader, bool pIsReverse)
        {
            TableCell td;
            WCTogglePanel pnl = new WCTogglePanel(Color.Transparent, pTitle, pCssHeader, pIsReverse);
            pnl.AddContent(pControl);
            td = NewControlInCell(pnl);
            return td;
        }
        #endregion NewTableHInCell
        //
        #region NewControlInRow
        public static TableRow NewControlInRow()
        {
            return NewControlInRow(new LiteralControl(Cst.HTMLSpace));
        }
        /// <summary>
        /// Ajoute le contrôle dans une cellule et ajoute la cellule dans une table
        /// </summary>
        /// <param name="pControl"></param>
        /// <returns></returns>
        public static TableRow NewControlInRow(Control pControl)
        {
            TableRow tr = new TableRow();
            TableCell td = NewControlInCell(pControl, Unit.Percentage(100), HorizontalAlign.Left);

            tr.Controls.Add(td);

            return tr;
        }
        #endregion
        // 
        #region NewTblCellSpace
        public static TableCell NewTblCellSpace(string cssName)
        {
            //20090909 PL Use Cst.HTMLSpace
            //return NewTblCell(" ", cssName);
            return NewTblCell(Cst.HTMLSpace, cssName);
        }
        #endregion
        #region private NewTblCellSpace100pct
        public static TableCell NewTblCellSpace100pct(string cssName)
        {
            //20090909 PL Use Cst.HTMLSpace
            //TableCell tblCellSpace = NewTblCell(" ", cssName);
            TableCell tblCellSpace = NewTblCell(Cst.HTMLSpace, cssName);
            tblCellSpace.Width = Unit.Percentage(100);
            //
            return tblCellSpace;
        }
        #endregion NewTblCellSpace100pct
        #region private NewTblCellSpace
        public static TableCell NewTblCell(string text, string cssName)
        {
            TableCell tblCell;
            tblCell = new TableCell
            {
                Text = text,
                CssClass = cssName,
                Wrap = false
            };
            if (text.TrimEnd().Length == 0)
                tblCell.Width = Unit.Pixel(4);
            return tblCell;
        }
        #endregion

        public static Label NewLabel(string pText, string pCssClass)
        {
            Label lbl = new Label
            {
                Text = pText,
                CssClass = pCssClass
            };
            return lbl;
        }
    }
    #endregion
}