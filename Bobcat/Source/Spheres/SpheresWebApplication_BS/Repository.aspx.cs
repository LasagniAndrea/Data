using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GridViewProcessor;
using SpheresMenu = EFS.Common.Web.Menu;
using EfsML.DynamicData;

namespace EFS.Spheres
{
    public partial class Repository : RepositoryPageBase
    {
        // EG 20180423 Analyse du code Correction [CA2200]
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("Start List.OnInit");
            try
            {
                base.OnInit(e);
                SetMainInfo();
                InitializeComponent();
                InitChkColumn();
            }
            catch (Exception) { throw; } //TrapException(ex); }

            AddAuditTimeStep("End List.OnInit");
        }

        private void SetMainInfo()
        {
            string mainMenu = ControlsTools.MainMenuName(idMenu);
            nvb.Attributes.Add("data-menu", mainMenu);
            rowtitle.Attributes.Add("data-menu", mainMenu);

            Pair<string, Cst.Capture.ModeEnum> action = actionTitle;
            titleaction.Text = action.First;
            pnlaction.Attributes.Add("data-menu", mainMenu);
            pnlaction.Attributes.Add("data-action", action.Second.ToString().ToLower());
        }

        private void InitializeComponent()
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //chkDisplayColumn.Attributes.Add("onclick", "javaScript:DisplayColumnName(this);");
            //chkDisplayToolTip.Attributes.Add("onclick", "javaScript:DisplayPopover(this);");
        }

        /// <summary>
        /// Initialisation des checks Column (Nom de la colonne et affichage aide en ligne
        /// </summary>
        private void InitChkColumn()
        {
            string javascript = string.Empty;

            BS_ClassicCheckBox chkShowColumnName = new BS_ClassicCheckBox("chkShowColumnName", true,
                Ressource.GetString("Repository" + AdditionalCheckBoxEnum.ShowColumnName), false);
            chkShowColumnName.CssClass = "col-sm-7 " + chkShowColumnName.CssClass;
            if (IsPostBack == false)
                chkShowColumnName.chk.Checked = false;

            chkShowColumnName.chk.Attributes.Add("onclick", "javaScript:ShowColumnName(this);");
            plhCheckColumn.Controls.Add(chkShowColumnName);

            BS_ClassicCheckBox chkShowPopover = new BS_ClassicCheckBox("chkShowPopover", true,
                Ressource.GetString("Repository" + AdditionalCheckBoxEnum.ShowPopover), false);
            chkShowPopover.CssClass = "col-sm-5 " + chkShowPopover.CssClass;
            if (IsPostBack == false)
                chkShowPopover.chk.Checked = false;

            chkShowPopover.chk.Attributes.Add("onclick", "javaScript:ShowPopover(this);");
            plhCheckColumn.Controls.Add(chkShowPopover);
        }

        /// <summary>
        /// Affichage du titre du référentiel
        /// Affichage de l'identifiant non significatif (COLONNE + ID)
        /// </summary>
        public override void SetRepositoryTitle()
        {
            lblMnuTitle.Text = htmlName;
            lblIdSystem.Visible = referential.ExistsColumnIDENTITY && (false == referential.Column[referential.IndexIDENTITY].IsIdentity.sourceSpecified);
            if (lblIdSystem.Visible)
            {
                lblIdSystem.Attributes.Add("prt-data", referential.Column[referential.IndexIDENTITY].ColumnName);
                if (null != referential.dataRow)
                    lblIdSystem.Text = referential.dataRow[referential.IndexColSQL_IDENTITY].ToString();
            }
        }
        #region SetActionDates
        /// <summary>
        /// Affichage de la date de création de l'enregistrement (Date + User)
        /// Affichage de la date de dernière modification de l'enregistrement (Date + User)
        /// </summary>
        public override void SetActionDates()
        {
            lblInsertDate.Visible = false;
            lblUpdateDate.Visible = false;

            if (referential.ExistsColumnsINS && (false == (referential.dataRow[referential.IndexColSQL_DTINS] is DBNull)))  
            {
                lblInsertDate.Visible = true;
                lblInsertDate.Text = Ressource.GetString_CreatedByWithTime(Convert.ToDateTime(referential.dataRow[referential.IndexColSQL_DTINS]), 
                    referential.dataRow[referential.IndexColSQL_IDAINS + 1].ToString());
            }
            if (referential.ExistsColumnsUPD && (false == (referential.dataRow[referential.IndexColSQL_DTUPD] is DBNull)))  
            {
                lblUpdateDate.Visible = true;
                lblUpdateDate.Text = Ressource.GetString_LastModifyByWithTime(Convert.ToDateTime(referential.dataRow[referential.IndexColSQL_DTUPD]),
                    referential.dataRow[referential.IndexColSQL_IDAUPD + 1].ToString());
            }
        }
        #endregion SetActionDates

        #region DisplayButtons
        public override void DisplayButtons()
        {
            DisplayButtonsConsult();
            DisplayButtonsAction();
            DisplayButtonsValidation();
        }
        #endregion DisplayButtons
        #region DisplayButtonLogo
        /// Gestion et Affichage du bouton Logo
        private void DisplayButtonLogo()
        {
            btnLogo.Visible = referential.LogoSpecified && referential.Logo.Value;
            btnLogo.Visible &= (referential.Column[referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());
            if (btnLogo.Visible)
            {
                // Add an image to attach an image to data (only for referential that have LO column (eg.: LOLOGO ))
                string LO_ColumnName = "LOLOGO";
                string imagePath = "logo";
                string toolTip = "btnLogo";

                string currentDisplayName = string.Empty;
                if (referential.IndexColSQL_DISPLAYNAME != -1)
                    currentDisplayName = referential.dataRow[referential.IndexColSQL_DISPLAYNAME].ToString();

                if (referential.Logo.columnnameSpecified)
                    LO_ColumnName = referential.Logo.columnname;

                if ((false == Convert.IsDBNull(referential.dataRow[LO_ColumnName]) &&
                    StrFunc.IsFilled(referential.dataRow[LO_ColumnName].ToString().Trim())))
                {
                    // Adding custom image in case we have an image attribute ("image") specified on the Logo node
                    // Check if the file really exists on the server machine
                    if (referential.Logo.imageSpecified && 
                        System.IO.File.Exists(Server.MapPath(String.Format("images/{0}", referential.Logo.image))))
                        imagePath = referential.Logo.image;

                    // Adding custom tooltip in case we have a tooltip attribute ("tooltip") specified on the Logo node
                    if (referential.Logo.tooltipSpecified)
                        toolTip = referential.Logo.tooltip;
                }
                btnLogo.Attributes.Add("title",Ressource.GetString(toolTip));
                btnLogo.Attributes.Add("prt-logo", imagePath);

                string[] columnName = new string[1];
                columnName[0] = referential.Column[referential.IndexDataKeyField].ColumnName;

                string[] columnDatatype = new string[1];
                columnDatatype[0] = referential.Column[referential.IndexDataKeyField].DataType.value;

                string[] columnValue = new string[1];
                columnValue[0] = valueDataKeyField;

                string[] uploadMIMETypes = new string[7];
                uploadMIMETypes[0] = Cst.TypeMIME.Image.ALL;
                uploadMIMETypes[1] = Cst.TypeMIME.Image.Bmp;
                uploadMIMETypes[2] = Cst.TypeMIME.Image.Gif;
                uploadMIMETypes[3] = Cst.TypeMIME.Image.Jpeg;
                uploadMIMETypes[4] = Cst.TypeMIME.Image.PJpeg;
                uploadMIMETypes[5] = Cst.TypeMIME.Image.Tiff;
                uploadMIMETypes[6] = Cst.TypeMIME.Image.XPng;

                string columnIDAUPD = string.Empty;
                string columnDTUPD = string.Empty;
                if (true == referential.ExistsColumnsUPD)
                {
                    columnIDAUPD = "IDAUPD";
                    columnDTUPD = "DTUPD";
                }

                btnLogo.Attributes.Add("onclick", 
                JavaScript.GetWindowOpenDBImageViewer(referential.TableName, uploadMIMETypes, referential.TableName, 
                LO_ColumnName, columnName, columnValue, columnDatatype, currentDisplayName, columnIDAUPD, columnDTUPD) + "return false;");
            }

        }
        #endregion DisplayButtonLogo
        #region DisplayButtonNotePad
        /// Gestion et Affichage du bouton NotePad
        private void DisplayButtonNotePad()
        {
            btnNotepad.Visible = referential.NotepadSpecified && referential.Notepad.Value;
            btnNotepad.Visible &= (referential.Column[referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());
            if (btnNotepad.Visible)
            {
                string NP_ColumnName = Cst.AliasNOTEPADACTOR + "_" + "DISPLAYNAME";
                string toolTip = Ressource.GetString("btnNotePad");
                if ((false == Convert.IsDBNull(referential.dataRow[NP_ColumnName]) &&
                    StrFunc.IsFilled(referential.dataRow[NP_ColumnName].ToString().Trim())))
                {
                    //Tips TRIM (PL)
                    if (listType == Cst.ListType.TRIM.ToString())
                        toolTip = "Thread";

                    string displayName = referential.dataRow[NP_ColumnName].ToString();
                    DateTime dtUpd = DateTime.MinValue;
                    if (false == Convert.IsDBNull(referential.dataRow[Cst.AliasNOTEPAD + "_" + "DTUPD"]))
                    {
                        dtUpd = Convert.ToDateTime(referential.dataRow[Cst.AliasNOTEPAD + "_" + "DTUPD"]);
                        toolTip += Cst.CrLf + Ressource.GetString_LastModifyBy(dtUpd, displayName);
                    }
                    btnNotepad.Attributes["class"] = btnNotepad.Attributes["class"].Replace("notepadempty", "notepad");
                    btnNotepad.Attributes.Add("title", toolTip);
                }

                string notepadTableName = objectName;
                string notepadIDValue = valueDataKeyField;
                string notepadKeyType = (TypeData.IsTypeString(referential.Column[referential.IndexDataKeyField].DataType.value) ? "1" : "0");
                string notepadDisplayValue = referential.dataRow[referential.IndexColSQL_KeyField].ToString();
                if (referential.NotepadSpecified && referential.Notepad.tablenameSpecified)
                {
                    notepadTableName = referential.Notepad.tablename;
                    if (referential.Notepad.IDSpecified)
                    {
                        notepadIDValue = referential.dataRow[referential.Notepad.ID].ToString();
                        notepadKeyType = (TypeData.IsTypeString(referential[referential.Notepad.ID].DataType.value) ? "1" : "0");
                        notepadDisplayValue = string.Empty;
                    }
                }

                //btnNotepad.Attributes.Add("onclick",
                //JavaScript.GetWindowOpenNotepad(notepadTableName, notepadIDValue, idMenu, notepadDisplayValue,
                //referential.consultationMode, notepadKeyType, listType) + "return false;");
                btnNotepad.Attributes.Add("data-load-url",
                JavaScript.GetUrlNotepad(notepadTableName, notepadIDValue, idMenu, 
                HttpUtility.UrlEncode(notepadDisplayValue),
                referential.consultationMode, notepadKeyType, listType));
            }
        }
        #endregion DisplayButtonNotePad
        #region DisplayButtonAttacheDoc
        /// <summary>
        /// Gestion et Affichage du bouton AttacheDoc
        /// </summary>
        private void DisplayButtonAttacheDoc()
        {
            btnAttacheDoc.Visible = referential.NotepadSpecified && referential.Notepad.Value;
            btnAttacheDoc.Visible &= (referential.Column[referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());
            if (btnAttacheDoc.Visible)
            {
                string AD_ColumnName = Cst.AliasATTACHEDDOCACTOR + "_" + "DISPLAYNAME";
                string toolTip = Ressource.GetString("btnAttachedDoc");
                // EG 20170426 Test Columns.Contains
                if (referential.dataRow.Table.Columns.Contains(AD_ColumnName))
                {
                    if ((false == Convert.IsDBNull(referential.dataRow[AD_ColumnName]) &&
                        StrFunc.IsFilled(referential.dataRow[AD_ColumnName].ToString().Trim())))
                    {
                        string displayName = referential.dataRow[AD_ColumnName].ToString();
                        DateTime dtUpd = DateTime.MinValue;
                        if (false == Convert.IsDBNull(referential.dataRow[Cst.AliasATTACHEDDOC + "_" + "DTUPD"]))
                        {
                            dtUpd = Convert.ToDateTime(referential.dataRow[Cst.AliasATTACHEDDOC + "_" + "DTUPD"]);
                            toolTip += Cst.CrLf + Ressource.GetString_LastModifyBy(dtUpd, displayName);
                        }
                        btnAttacheDoc.Attributes["class"] = btnAttacheDoc.Attributes["class"].Replace("attacheddocempty", "attacheddoc");
                        btnAttacheDoc.Attributes.Add("title", toolTip);
                    }
                }

                string tableAttachedDoc = Cst.OTCml_TBL.ATTACHEDDOC.ToString();
                if (TypeData.IsTypeString(referential.Column[referential.IndexDataKeyField].DataType.value))
                    tableAttachedDoc = Cst.OTCml_TBL.ATTACHEDDOCS.ToString();

                string viewTableAttachedDoc = string.Empty;

                string attachedDocTableName = referential.TableName;
                string attachedDocIDValue = valueDataKeyField;
                string attachedDocKeyType = (TypeData.IsTypeString(referential.Column[referential.IndexDataKeyField].DataType.value) ? "1" : "0");
                string attachedDocDisplayValue = referential.dataRow[referential.IndexColSQL_KeyField].ToString();
                string attachedDocDescriptionValue = string.Empty;
                if (referential.ExistsColumnDESCRIPTION && (referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString().Length > 0))
                    attachedDocDescriptionValue = referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString();
                if (referential.AttachedDocSpecified && referential.AttachedDoc.tablenameSpecified)
                {
                    attachedDocTableName = referential.AttachedDoc.tablename;
                    if (referential.AttachedDoc.IDSpecified)
                    {
                        attachedDocIDValue = referential.dataRow[referential.AttachedDoc.ID].ToString();
                        attachedDocKeyType = (TypeData.IsTypeString(referential[referential.AttachedDoc.ID].DataType.value) ? "1" : "0");
                        attachedDocDisplayValue = string.Empty;
                        attachedDocDescriptionValue = string.Empty;
                    }
                }

                btnAttacheDoc.Attributes.Add("onclick",
                JavaScript.GetWindowOpenAttachedDoc(tableAttachedDoc, attachedDocIDValue, attachedDocDisplayValue,
                attachedDocDescriptionValue, idMenu, attachedDocTableName, viewTableAttachedDoc) + "return false;");
            }
        }
        #endregion DisplayButtonAttacheDoc
        #region DisplayButtonsAction
        private void DisplayButtonsAction()
        {
            btnLogo.Visible = false;
            btnAttacheDoc.Visible = false;
            btnNotepad.Visible = false;
            if ((false == isMakingChecking_ActionChecking) && (false == referential.isNewRecord))
            {
                // btnLogo
                DisplayButtonLogo();
                // btnAttacheDoc
                DisplayButtonAttacheDoc();
                // btnNotepad
                DisplayButtonNotePad();
            }
        }
        #endregion DisplayButtonsAction
        #region DisplayButtonsConsult
        private void DisplayButtonsConsult()
        {
            bool IsVisible = isOpenFromGrid && (false == referential.isNewRecord);
            btnFirstRecord.Visible = IsVisible;
            btnPreviousRecord.Visible = IsVisible;
            btnNextRecord.Visible = IsVisible;
            btnLastRecord.Visible = IsVisible;
        }
        #endregion DisplayButtonsConsult
        #region DisplayButtonsValidation
        private void DisplayButtonsValidation()
        {
            btnCancel.Attributes.Add("onclick", "self.close();return false;");
            btnApply.Visible = false;
            btnCancel.Visible = false;
            btnDuplicate.Visible = false;
            btnRecord.Visible = false;
            btnRemove.Visible = false;

            // En mode Normal, Spheres® peut afficher un formulaire vide en mode ReadOnly
            // C'est le cas lorsque l'enregistrement n'est pas disponible 
            // => il n'existe plus ou les restrictions n'autorisent pas l'accès à cet enregistrement)
            if ((Cst.ConsultationMode.ReadOnly == referential.consultationMode) || referential.isLookReadOnly)
            {
                // RAS
            }
            else if (Cst.ConsultationMode.Select == referential.consultationMode)
            {
                btnCancel.Visible = true;
                btnRecord.Visible = true;
            }
            else if (((Cst.ConsultationMode.Normal == referential.consultationMode) || 
                      (Cst.ConsultationMode.PartialReadOnly == referential.consultationMode)) &&
                      (referential.Create || referential.Modify || referential.Remove))
            {
                if (isMakingChecking_ActionChecking)
                {
                    btnRecord.Visible = true;
                }
                else
                {
                    btnApply.Visible = true;
                    btnCancel.Visible = true;
                    btnRecord.Visible = true;
                }

                if (false == referential.isNewRecord)
                {
                    // Note: Si non spécifié , on affiche le bouton "Remove"
                    if ((false == referential.RemoveSpecified) || referential.Remove)
                        btnRemove.Visible = true;

                    // PL 20161124 - RATP 4Eyes - MakingChecking
                    if (false == isMakingChecking_ActionChecking)
                    {
                        // Note: Si non spécifié , on affiche le bouton "Duplicate"
                        if ((false == referential.DuplicateSpecified) || referential.Duplicate)
                            btnDuplicate.Visible = true;
                    }
                }


                if ((false == referential.isNewRecord) && referential.ButtonSpecified)
                {
                    int index = 0;
                    foreach (ReferentialButton button in referential.Button)
                    {
                        AddButtonSpecific(button, ++index);
                    }
                }
            }
            /*
            if (referential.HasDataLightDisplay)
            {
                AddCheckBox(tr2, AdditionalCheckBoxEnum.ShowAllData);
            }

            if (SessionTools.IsSessionSysAdmin)
            {
                AddCheckBox(tr2, AdditionalCheckBoxEnum.ShowColumnName);
            }
            */
        }
        #endregion DisplayButtonsValidation


        #region CreateControlMenuChilds
        /// <summary>
        /// Initialisation du bouton des menus enfants
        /// </summary>
        public override void CreateControlMenuChilds()
        {
            if (isExistMenuChild)
            {
                if (!isMenuChildLoaded)
                {
                    SpheresMenu.Menus menus = SessionTools.Menus;
                    GetMenuChilds(idMenu, menus, menuDetail);
                    isMenuChildLoaded = true;
                    if ((null != menuDetail) && menuDetail.HasSubItems)
                    {
                        // Menus de 1er niveau avec sous-menus 
                        AddControlMenuButton(menuDetail);
                        // Actions de 1er niveau
                        AddControlMenuOthers(menuDetail);
                    }
                }
            }
        }
        #endregion CreateControlMenuChilds
        #region AddControlMenuAction
        private Control AddControlMenuAction(EFS.GridViewProcessor.MenuItem pMenu)
        {
            // <li><a href="#">Dropdown Link 1</a></li>
            HtmlGenericControl li = new HtmlGenericControl("li");
            HtmlAnchor a = new HtmlAnchor();

            string url = String.Format("{0}&IDMenu={1}&FK={2}&SubTitle={3}",
                pMenu.url, pMenu.idMenu, valueDataKeyField,
            HttpUtility.UrlEncode(pMenu.SubTitle + ": " + referential.dataRow[referential.IndexColSQL_KeyField].ToString(), System.Text.Encoding.UTF8));

            if (referential.ExistsColumnDESCRIPTION && (referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString().Length > 0))
                url += HttpUtility.UrlEncode(" - " + referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString(), System.Text.Encoding.UTF8);

            a.HRef = url;

            a.InnerText = pMenu.Label;
            a.Title = pMenu.Label;
            li.Controls.Add(a);
            return li;
        }
        #endregion AddControlMenuAction
        #region AddControlMenuSeparator
        private Control AddControlMenuSeparator()
        {
            // <li role="separator" class="divider"></li>
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("role", "separator");
            li.Attributes.Add("class", "divider");
            return li;
        }
        #endregion AddControlMenuAction
        #region AddControlMenuDropDown
        private Control AddControlMenuDropDown(EFS.GridViewProcessor.MenuItem pMenu)
        {
            // <li class="dropdown dropdown-submenu"><a href="#" class="dropdown-toggle" data-toggle="dropdown">Dropdown Link 4</a>
            //     <ul class="dropdown-menu">
            //     ...
            //     </ul>
            // </li>

            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "dropdown dropdown-submenu");
            HtmlAnchor a = new HtmlAnchor();
            a.Attributes.Add("class", "dropdown-toggle");
            a.Attributes.Add("data-toggle", "dropdown");
            a.HRef = "#";
            a.InnerText = pMenu.Label;
            a.Title = pMenu.Label;
            li.Controls.Add(a);

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "dropdown-menu");
            if (pMenu.HasSubItems)
            {
                pMenu.subItems.ForEach(item =>
                { 
                    if (item.isAction)
                        ul.Controls.Add(AddControlMenuAction(item));
                    else
                        ul.Controls.Add(AddControlMenuDropDown(item));
                });
            }
            li.Controls.Add(ul);
            return li;
        }
        #endregion AddControlMenuDropDown
        #region AddControlMenuButton
        private void AddControlMenuButton(EFS.GridViewProcessor.MenuItem pMenu)
        {
            // <button class="btn btn-xs btn-details dropdown-toggle" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" title="Details">
            //     <span class="glyphicon glyphicon-th-list" aria-hidden="true">
            //     <span class="caret"></span>
            // </button>
            // <ul class="dropdown-menu">
            // ...
            // </ul>
            if (pMenu.HasSubItemsActions)
            {
                if (pMenu.subItems.Exists(item => false == item.isAction))
                {
                    HtmlGenericControl li = new HtmlGenericControl("li");
                    li.Attributes.Add("class", "detail");
                    HtmlGenericControl span = new HtmlGenericControl("span");
                    span.Attributes.Add("class", "glyphicon glyphicon-th-list");
                    li.Controls.Add(span);
                    plhMenuDetail.Controls.Add(li);
                }
                pMenu.subItems.FindAll(item => false == item.isAction).ForEach(item => 
                {
                    HtmlGenericControl li = new HtmlGenericControl("li");

                    HtmlButton btn = new HtmlButton();
                    btn.Attributes.Add("class", "btn btn-xs btn-details dropdown-toggle");
                    btn.Attributes.Add("data-toggle", "dropdown");
                    btn.Attributes.Add("aria-haspopup", "true");
                    btn.Attributes.Add("aria-expanded", "false");
                    btn.InnerText = item.Label;
                    
                    HtmlGenericControl span = new HtmlGenericControl("span");
                    span.Attributes.Add("class", "caret");
                    btn.Controls.Add(span);

                    HtmlGenericControl ul = new HtmlGenericControl("ul");
                    ul.Attributes.Add("class", "dropdown-menu");
                    if (item.HasSubItems)
                    {
                        EFS.GridViewProcessor.MenuItem previousSubItem = null;
                        item.subItems.ForEach(subItem =>
                        {
                            if ((null != previousSubItem) && (previousSubItem.isAction != subItem.isAction))
                                ul.Controls.Add(AddControlMenuSeparator());
                            
                            if (subItem.isAction)
                                ul.Controls.Add(AddControlMenuAction(subItem));
                            else
                                ul.Controls.Add(AddControlMenuDropDown(subItem));

                            previousSubItem = subItem;
                        });
                    }

                    li.Controls.Add(btn);
                    li.Controls.Add(ul);
                    plhMenuDetail.Controls.Add(li);
                });
            }
        }
        #endregion AddControlMenuDropDown
        #region AddControlMenuOthers
        private void AddControlMenuOthers(EFS.GridViewProcessor.MenuItem pMenu)
        {
            // <button class="btn btn-xs btn-details dropdown-toggle" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" title="Details">
            //     <span class="caret"></span>
            // </button>
            // <ul class="dropdown-menu">
            // ...
            // </ul>
            if (pMenu.HasSubItemsActions)
            {
                HtmlGenericControl li = new HtmlGenericControl("li");

                HtmlButton btn = new HtmlButton();
                btn.Attributes.Add("class", "btn btn-xs btn-details dropdown-toggle");
                btn.Attributes.Add("data-toggle", "dropdown");
                btn.Attributes.Add("aria-haspopup", "true");
                btn.Attributes.Add("aria-expanded", "false");

                HtmlGenericControl span = null;
                if (pMenu.subItems.Exists(item => false == item.isAction))
                {
                    btn.InnerText = Ressource.GetString("lblOthers");
                }
                else 
                {
                    span = new HtmlGenericControl("span");
                    span.Attributes.Add("class", "glyphicon glyphicon-th-list");
                    btn.Controls.Add(span);
                }

                span = new HtmlGenericControl("span");
                span.Attributes.Add("class", "caret");
                btn.Controls.Add(span);

                HtmlGenericControl ul = new HtmlGenericControl("ul");
                ul.Attributes.Add("class", "dropdown-menu");

                pMenu.subItems.FindAll(item => item.isAction).ForEach(item => ul.Controls.Add(AddControlMenuAction(item)));

                li.Controls.Add(btn);
                li.Controls.Add(ul);
                plhMenuDetailOthers.Controls.Add(li);
            }
        }
        #endregion AddControlMenuDropDown


        #region CreateGridSystem
        /// <summary>
        /// Construction du squelette du formulaire
        /// </summary>
        protected override void CreateGridSystem()
        {
            plhGridSystem.Controls.Add(ConstructGridSystem(pnlGridSystem));
        }
        #endregion CreateGridSystem
        #region WriteControls
        /// <summary>
        /// Création des contrôles liés aux colonnes du formulaire
        /// </summary>
        protected override void CreateControls()
        {
            referential.form.CreateControls(this, pnlGridSystem);
        }
        #endregion WriteControls
    }
}