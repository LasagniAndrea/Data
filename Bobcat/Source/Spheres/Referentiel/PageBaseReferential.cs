#region VSS Auto-Comments
/* 
 * *********************************************************************
 $History: PageBaseReferential.cs $
 * 
 * *****************  Version 1  *****************
 * User: administrateur Date: 5/11/09    Time: 11:20
 * Created in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 1  *****************
 * User: Filipe       Date: 12/06/09   Time: 10:03
 * Created in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 66  *****************
 * User: Razik        Date: 22/04/09   Time: 15:49
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 65  *****************
 * User: Filipe       Date: 16/04/09   Time: 16:58
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 64  *****************
 * User: Filipe       Date: 27/02/09   Time: 11:38
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 63  *****************
 * User: Eric         Date: 13/02/09   Time: 12:25
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 62  *****************
 * User: Filipe       Date: 4/02/09    Time: 13:11
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 61  *****************
 * User: Filipe       Date: 31/12/08   Time: 12:31
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 60  *****************
 * User: Filipe       Date: 30/12/08   Time: 9:47
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 59  *****************
 * User: Pascal       Date: 24/12/08   Time: 12:57
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 58  *****************
 * User: Pascal       Date: 22/12/08   Time: 19:08
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 57  ***************** 
 * User: Pascal       Date: 22/09/08   Time: 18:19
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 56  *****************
 * User: Filipe       Date: 11/09/08   Time: 13:30
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 55  *****************
 * User: Pascal       Date: 5/09/08    Time: 16:00
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 54  *****************
 * User: Filipe       Date: 29/08/08   Time: 12:48
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 53  *****************
 * User: Filipe       Date: 25/08/08   Time: 16:32
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 52  *****************
 * User: Pascal       Date: 21/08/08   Time: 16:16
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * **********************************************************************
*/
//
#endregion
#region using
using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Xml; 


using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web; 
using EFS.Referentiel;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EFS.Restriction;

//20071212 FI Ticket 16012 => Migration Asp2.0
using SpheresMenu = EFS.Common.Web.Menu;  


#endregion using

namespace EFS.Referentiel
{
	/// <summary>
	/// Description résumée de PageBaseReferential.
	/// </summary>
	public class PageBaseReferential : PageBase
	{
		#region Members
		
		private string   idFormParent;
		
		private string   tableName;
		private string   conditionApplicationForSQLWhere;
		private string[] param;
		private string   dataKeyField, valueDataKeyField;
		private string   dataForeignKeyField, valueForeignKeyField;
		
		private bool     isNewRecord;    
		private bool     isDuplicateRecord;    
		private Cst.ConsultationMode consultationMode;
		public  string   pageFooterLeft;
		public  string	 pageFooterRight;
		public  string   pageInfoLeft;
		public  string	 pageInfoRight;
		private ReferentialsReferential referential;
		
		private string   listType;
		private string   idMenu;
		private string   titleMenu;
		private bool     isExistMenuChild;

		private PlaceHolder[]          plhMenuDetail;		
		private skmMenu.MenuItemParent menuDetail; 
		#endregion Members

		#region Accessor
		private string imageReferential
		{
			get
			{
				string ret = string.Empty; 
				if (null!= referential)
					ret =  referential.Image;  
				return ret; 
			}
		}

		private string labelReferential
		{
			get
			{
				string ret =  Ressource.GetString(idMenu, titleMenu); 
				ret = StrFunc.IsEmpty(ret)  ? tableName : ret;
				return ret ;
			}
		}
		private string nameReferential
		{
			get
			{
				return Ressource.GetString(listType, true) + ": " + HtmlTools.HTMLBold(labelReferential); 
			}
		}
		
		private string modeReferential
		{
			get
			{
				return  Ressource.GetString_SelectionCreationModification(consultationMode, isNewRecord);
			}
		}

		#region public dataReferentielName
		public string dataReferentielName
		{
			get
			{
				//return prefixeForDataCache + ObjectName + (prefixeForDataCache.StartsWith("FK") ? "FK" : this.ClientID);
				string prefix = "FORM";
				//				bool isConsultation = (null != Page.Request["Consultation"]);
				//				if (isConsultation)
				//					prefix = "VIEW";
				string ret  = prefix + tableName;
				//20060228 PL/FL Add Collaborator_IDENTIFIER dans le nom 
				ret += SessionTools.Collaborator_IDENTIFIER.Replace("-",string.Empty);

				if(StrFunc.IsFilled(dataKeyField) && StrFunc.IsFilled(valueDataKeyField))
					ret += "_" + dataKeyField + "=" + valueDataKeyField;

				if(StrFunc.IsFilled(dataForeignKeyField) && StrFunc.IsFilled(valueForeignKeyField))
					ret += "_" + dataForeignKeyField + "=" + valueForeignKeyField;
				
				return ret;
			}
		}
		#endregion DataSet (cache)   
		#region dataReferentiel
		public DataSet dataReferentiel
		{
			get
			{
				return (DataSet) HttpContext.Current.Session[dataReferentielName];
			}
			set
			{
				HttpContext.Current.Session[dataReferentielName] = value;
			}
		}		       
		#endregion DataSet (cache)   
		#endregion Accessor

		#region constructor
		public PageBaseReferential()
		{
			plhMenuDetail = new PlaceHolder[2];
			menuDetail    = new skmMenu.MenuItemParent(1); 
			
			isExistMenuChild = false;
			pageFooterLeft   = string.Empty;
			pageFooterRight  = string.Empty;
			pageInfoLeft     = string.Empty;
			pageInfoRight    = string.Empty;
		}
		#endregion constructor
			
		#region public AddFooterXXX AddInfoXXX
		public void AddFooterLeft (string pMsgAdd)
		{
			if (StrFunc.IsFilled(pageFooterLeft))
				pageFooterLeft += Cst.CrLf;
			pageFooterLeft += pMsgAdd + Cst.HTMLSpace;
		}
		public void AddFooterRight(string pMsgAdd)
		{
			if (StrFunc.IsFilled(pageFooterRight))
				pageFooterRight += Cst.CrLf;
			pageFooterRight += pMsgAdd + Cst.HTMLSpace;
		}
		public void AddInfoLeft (string pMsgAdd)
		{
			if (StrFunc.IsFilled(pageInfoLeft))
				pageInfoLeft += Cst.CrLf;
			pageInfoLeft += pMsgAdd + Cst.HTMLSpace;
		}
		public void AddInfoRight(string pMsgAdd)
		{
			if (StrFunc.IsFilled(pageInfoRight))
				pageInfoRight += Cst.CrLf;
			pageInfoRight += pMsgAdd + Cst.HTMLSpace;
		}
		#endregion

		#region protected override OnInit
		protected override void OnInit(System.EventArgs e)
		{
            base.OnInit(e);
            //
            #region Variables initialization
            //example: "http://localhost/OTC/Referential/Referential.aspx?T=Referential&O=BUSINESSCENTER&M=0&N=0&PK=IDBC&PKV=ARBA&FK=&FKV=&F=frmConsult&IDMenu=16540"
			AbortRessource = true;
			//
			tableName			= Request.QueryString["O"];		
			listType		    = Request.QueryString["T"];		//Folder pour recherche du fichier XML 
			if (StrFunc.IsEmpty(listType))
				listType = "Referential";
			dataKeyField		 = Request.QueryString["PK"];
			valueDataKeyField	 = Request.QueryString["PKV"];
			dataForeignKeyField  = Request.QueryString["FK"];				
			valueForeignKeyField = Request.QueryString["FKV"];
			//
			isNewRecord          = (Convert.ToInt32(Request.QueryString["N"])==1);  //New record
			isDuplicateRecord    = (Convert.ToInt32(Request.QueryString["DPC"])==1);//Mode duplicate
            idFormParent = Request.QueryString["F"];		
			//IDMenu => permet de recuperer les habilitations sur le referentiel, 
			//			permet aussi d'afficher le titre lorsque ce dernier est inexistant
			idMenu		         = Request.QueryString["IDMenu"];                   
			titleMenu	         = Request.QueryString["TitleMenu"];                
			conditionApplicationForSQLWhere = Request.QueryString["CondApp"];      
			param                = ReferentialTools.GetQueryStringParam(Page); // %%PARAM%%
			//
			try 
			{
				consultationMode = (Cst.ConsultationMode)Convert.ToInt32( Request.QueryString["M"] ); 
			}
			catch
			{
				consultationMode = Cst.ConsultationMode.Normal;
			}
			//
			if (!IsPostBack)
			{
				if (this.Request.QueryString["OCR"] == "1")    
					JavaScript.OpenerCallReload (this);    
			}
			//
			//ReferentialTools.DeserializeXML_ForModeRW(CS, idMenu, "../" + listType, tableName, conditionApplicationForSQLWhere, param, out referential);
            ReferentialTools.DeserializeXML_ForModeRW(CS, idMenu, listType, tableName, conditionApplicationForSQLWhere, param, out referential);
			//20070104 PL Add if/else
			if (StrFunc.IsFilled(Request.QueryString["DA"]))
                referential.dynamicArgs = StrFunc.StringArrayList.StringListToStringArray(Request.QueryString["DA"]);
			else
				referential.dynamicArgs = null;
			//
            //GLOPPL 20081222 TEST Cst.ErrLevel ret = ReferentialTools.InitializeReferentialForForm_2(this, ref referential, isNewRecord, isDuplicateRecord,
            //    consultationMode, dataKeyField, valueDataKeyField,valueForeignKeyField);
			//			if(ret == Cst.ErrLevel.SUCCESS)
			//			{
			//Tips TRIM (PL)
			//			if (title == Cst.ListType.TRIM.ToString())
			//				referential.Image = Cst.Img_View;
			//			//
			if (isDuplicateRecord)
			{
				//Tip: 20050528 PL On joue avec la valeur des variables pour considérer la duplication comme une création
				isDuplicateRecord       = false;
				isNewRecord             = true;
				referential.isNewRecord = isNewRecord;
			}
			#endregion 
			//
			#region BuildPage
			HtmlPageTitle titleLeft  = new HtmlPageTitle(nameReferential, null, imageReferential, pageFooterLeft, Color.Red, "FooterLeftId");
			HtmlPageTitle titleRight = new HtmlPageTitle(modeReferential, null, null, pageFooterRight, Color.Transparent, "FooterRightId");
			PageSubTitle = HtmlTools.HTMLBold_Remove(nameReferential) + " - " + modeReferential;
			//			
			GenerateHtmlForm();
            //
            if (this.isBuildPageOldMethod)
            {
                ControlsTools.BuildPage(this, Form, PageFullTitle, titleLeft, titleRight, null, referential.HelpUrl, false, null);
            }
            else
            {
                FormTools.AddBanniere(Form, titleLeft, titleRight, referential.HelpUrl,IsLookV2);
                PageTools.BuildPage(this, Form, PageFullTitle, null, false, null);
            }
			#endregion 
			//
			#region MenuChild
			if (isExistMenuChild)
			{
				if (!referential.ToolBarSpecified || referential.ToolBar.IndexOf("top")>=0)
				{
					skmMenu.Menu menuMain_top;
					menuMain_top = new skmMenu.Menu(0,this.Page,"menuMain_top","mnuMasterTable","mnuTable",null,null,null);
					menuMain_top.DataSource = menuDetail.initXmlWriter(null,null,null);
					menuMain_top.Layout     = skmMenu.MenuLayout.Horizontal;
					menuMain_top.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
					plhMenuDetail[0].Controls.Add(menuMain_top);
					//menuMain_top.MenuItemClick+=new skmMenu.MenuItemClickedEventHandler(OnConsult);                    
				}
				if (!referential.ToolBarSpecified || referential.ToolBar.IndexOf("bottom")>=0)
				{
					skmMenu.Menu menuMain_bottom;
					menuMain_bottom = new skmMenu.Menu(1,this.Page,"menuMain_bottom","mnuMasterTable","mnuTable",null,null,null);
					menuMain_bottom.DataSource = menuDetail.initXmlWriter(null,null,null);
					menuMain_bottom.Layout     = skmMenu.MenuLayout.Horizontal;
					menuMain_bottom.LayoutDOWN = skmMenu.MenuLayoutDOWN.UP;
					plhMenuDetail[1].Controls.Add(menuMain_bottom);
					//menuMain_bottom.MenuItemClick+=new skmMenu.MenuItemClickedEventHandler(OnConsult);
				}           
			}            

			Control tblMenu;
			tblMenu = FindControl("tblMenu_Head");
			if( tblMenu != null)
				tblMenu.DataBind();
			tblMenu = FindControl("tblMenu_Foot");
			if( tblMenu != null)
				tblMenu.DataBind();
			#endregion 
			//
			#region SetFocus
			//Set focus on first control (Seulement en création, afin d'éviter les désagrément sur un DDL liés à la molette de la souris)
			if (!IsPostBack && referential.isNewRecord)
			{
				if (StrFunc.IsFilled(referential.firstVisibleAndEnabledControlID))
				{
					JavaScript.SetFocus(this);
					JavaScript.ScriptOnStartUp(this,"SetFocus("+ JavaScript.JSString(referential.firstVisibleAndEnabledControlID) + ");" ,"PageSetFocus");
				}
			}
			#endregion 
			//
		}
		#endregion override  OnInit
		#region protected override OnPrerender
		protected override void OnPreRender(EventArgs e)
		{
			Control ctrl = this.FindControl("tblFooter");
			if (null != ctrl)
			{
				Control ctrlFooterLeft = ctrl.FindControl("FooterLeftId");
				if (StrFunc.IsFilled(pageFooterLeft))
					((TableCell)ctrlFooterLeft).Text = pageFooterLeft ;
				//	
				Control ctrlFooterRight = ctrl.FindControl("FooterRightId");
				if (StrFunc.IsFilled(pageFooterRight))
					((TableCell)ctrlFooterRight).Text = pageFooterRight ;
			}
			//
			base.OnPreRender (e);
		}

		#endregion
		#region protected override GenerateHtmlForm 
		protected override void GenerateHtmlForm()
		{
			base.GenerateHtmlForm(); 

			#region Header
			if ( (!referential.ToolBarSpecified) || (referential.ToolBar.IndexOf("top")>=0) )
				AddButton("_Head");
				AddInformation(string.Empty);
			#endregion 
			
			#region Create And LoadControls
			WriteControls();
			//
			//Ajout des controles dans HtmlForm
			int count = this.Controls.Count;
			for( int i = 0; i<count; ++i )
			{
				System.Web.UI.Control ctrl  = this.Controls[0];
				CellForm.Controls.Add( ctrl );
				Control panel =  this.FindControl("Panel");
				this.Controls.Remove( ctrl );
			}
			#endregion 
			
			#region Footer
			if ( (!referential.ToolBarSpecified) || (referential.ToolBar.IndexOf("bottom")>=0) )
				AddButton("_Foot");
			#endregion 
		}
		#endregion
        #region protected override CreateChildControls
        protected override void CreateChildControls()
        {
            scriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageReferential.js"));
            //
            base.CreateChildControls();
        }
        #endregion

		#region protected AddInformation
		protected void AddInformation(string pSuffix)
		{
			Table table = new Table();
			table.ID    = "tblInfo" + pSuffix;
			TableRow tr = new TableRow();
			table.Width = Unit.Percentage(100);
			
			#region Information (ID System, Créé par, Modifié par...)
			TableCell td       = new TableCell();
			td.HorizontalAlign = HorizontalAlign.Left;
			td.Font.Size       = FontUnit.XXSmall;
			td.ForeColor       = Color.Black;
			td.Text            = (pageInfoLeft);
			tr.Cells.Add(td);
			//
			td       = new TableCell();
			td.HorizontalAlign = HorizontalAlign.Right;
			td.Font.Size       = FontUnit.XXSmall;
			td.ForeColor       = Color.Black;
			td.Text            = (pageInfoRight);
			tr.Cells.Add(td);
			//
			table.Rows.Add(tr);
			#endregion
			CellForm.Controls.Add(table);
		}
		#endregion
		#region protected AddButton
		protected void AddButton(string pSuffix)
		{
			Table table = new Table();
			table.ID    = "tblMenu" + pSuffix;
			TableRow tr = new TableRow();
			table.Width = Unit.Percentage(100);
			
			#region Validation (Enregistrer, Annuler...)
            bool isButtonAdded = false;
			if (referential.consultationMode == Cst.ConsultationMode.Select)
			{
                isButtonAdded = true;
                AddButtonValidate(pSuffix, tr);
				AddButtonCancel(pSuffix, tr);
			}
            else if ((referential.consultationMode == Cst.ConsultationMode.Normal)
                    && (referential.Create || referential.Modify || referential.Remove))
            {
                isButtonAdded = true;
                AddButtonRecord(pSuffix, tr, "btnOk");
                AddButtonCancel(pSuffix, tr);
                AddButtonRecord(pSuffix, tr, "btnApply");
                if (!referential.isNewRecord)
                {
                    //Note: Si non spécifié , on affiche le bouton "Remove"
                    if (!referential.RemoveSpecified || referential.Remove)
                        AddButtonRemove(pSuffix, tr);
                    //Note: Si non spécifié , on affiche le bouton "Duplicate"
                    if (!referential.DuplicateSpecified || referential.Duplicate)
                        AddButtonDuplicate(pSuffix, tr);

                }
            }
			if (!isButtonAdded)
				AddButtonClose(pSuffix, tr);
			#endregion
            
			Table table2       = new Table();
			table2.CssClass    = "mnuTable";
			table2.Width       = Unit.Percentage(100);
			table2.BorderWidth = 1;
			table2.CellPadding = 0;
			table2.CellSpacing = 0;
			TableRow tr2       = new TableRow();
			AddButtonChildReferential( pSuffix, tr2);
            
			#region Notepad / DocAttached / ImgAttached
			if ( !referential.isNewRecord )
				AddToolBarAction(pSuffix, tr2);
			#endregion
			table2.Rows.Add(tr2);
			TableCell td = new TableCell();
			td.Controls.Add(table2);
			td.Width = Unit.Percentage(100);
			tr.Cells.Add(td);
			table.Rows.Add(tr);
			CellForm.Controls.Add(table);
		}
		#endregion protected AddButton
		#region protected AddToolBarAction
		protected void AddToolBarAction(string pSuffix, TableRow pTr)
		{
			pTr.Cells.Add(ControlsTools.WebTdSeparator());

			TableCell td = new TableCell();
			td.ID = "tdTbrAction" + pSuffix;
			td.VerticalAlign   = VerticalAlign.Top;
			td.HorizontalAlign = HorizontalAlign.Left;
            
			PlaceHolder phToolBar = new PlaceHolder();
			skmMenu.Menu mnuToolBar;
			mnuToolBar = new skmMenu.Menu((pSuffix=="_Head"?2:3),this.Page,"mnuAction" + pSuffix,"mnuMasterTable","mnuTable",null,null,null);
			skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(3);


			string currentDisplayName = string.Empty;
			if(referential.IndexColSQL_DISPLAYNAME != -1)
				currentDisplayName = referential.dataRow[referential.IndexColSQL_DISPLAYNAME].ToString();

            //Add an image to attach an image to data (only for referential that have LO column (eg.: LOLOGO ))
            string LO_ColumnName = "LOLOGO";
            string imagePath, toolTip;
            bool isEmpty, isEnabled;

			isEnabled = (referential.LogoSpecified && true == referential.Logo.Value);
			isEnabled &= (referential.Column[referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());

			// to disable: set isEmpty to true
            if (isEnabled)
            {
                if (referential.Logo.columnnameSpecified)
                    LO_ColumnName = referential.Logo.columnname;
                isEmpty = (referential.dataRow[LO_ColumnName] == null || StrFunc.IsEmpty(referential.dataRow[LO_ColumnName].ToString().Trim()));
            }
            else
                isEmpty = true;
			if (isEmpty)
				imagePath = Cst.Img_Logo;
			else
				imagePath = Cst.Img_LogoFull;
			mnu[0]           = new skmMenu.MenuItemParent(0);
			mnu[0].Enabled   = isEnabled;
			mnu[0].Hidden    = (false == isEnabled);
			mnu[0].aToolTip  = Ressource.GetString("btnLogo");
			mnu[0].eImageUrl = imagePath;

			string[] uploadMIMETypes = new string[6];
			string[] columnName      = new string[1];
			string[] columnValue     = new string[1];
			string[] columnDatatype  = new string[1];
			uploadMIMETypes[0] = Cst.TypeMIME.Image.ALL;            
			uploadMIMETypes[1] = Cst.TypeMIME.Image.Bmp;
			uploadMIMETypes[2] = Cst.TypeMIME.Image.Gif;            
			uploadMIMETypes[3] = Cst.TypeMIME.Image.Jpeg;            
			uploadMIMETypes[4] = Cst.TypeMIME.Image.PJpeg;            
			uploadMIMETypes[5] = Cst.TypeMIME.Image.Tiff;
			columnName[0]      = referential.Column[referential.IndexDataKeyField].ColumnName;
			columnValue[0]     = valueDataKeyField;
			columnDatatype[0]  = referential.Column[referential.IndexDataKeyField].DataType;

			string columnIDAUPD = string.Empty;
			string columnDTUPD  = string.Empty;
			if(true == referential.ExistsColumnsUPD)
			{
				columnIDAUPD = "IDAUPD";
				columnDTUPD  = "DTUPD";
			}

            mnu[0].eUrl = JavaScript.GetWindowOpenDBImageViewer(referential.TableName, uploadMIMETypes, referential.TableName, LO_ColumnName, 
                columnName, columnValue, columnDatatype, currentDisplayName, columnIDAUPD, columnDTUPD);


			#region NOTEPAD
			isEnabled = (true == referential.NotepadSpecified && true == referential.Notepad);
			isEnabled &= (referential.Column[referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());

			// to disable: set isEmpty to true
			if (isEnabled)
				isEmpty = (referential.dataRow[Cst.AliasNOTEPADACTOR + "_" + "DISPLAYNAME"] == null || StrFunc.IsEmpty(referential.dataRow[Cst.AliasNOTEPADACTOR + "_" + "DISPLAYNAME"].ToString().Trim()));
			else
				isEmpty = true;
			//
			//Tips TRIM (PL)
			if (listType == Cst.ListType.TRIM.ToString())
				toolTip = "Thread";
			else
				toolTip = Ressource.GetString("btnNotePad");
			//
			imagePath = Cst.Img_Notepad;
			if (!isEmpty)
			{
				string   displayName = referential.dataRow[Cst.AliasNOTEPADACTOR + "_" + "DISPLAYNAME"].ToString();
				DateTime dtUpd;
				if(referential.dataRow[Cst.AliasNOTEPAD + "_" + "DTUPD"] == DBNull.Value)
					dtUpd = DateTime.MinValue;
				else
					dtUpd = Convert.ToDateTime(referential.dataRow[Cst.AliasNOTEPAD + "_" + "DTUPD"]);
				imagePath = Cst.Img_NotepadFull;
				if(dtUpd != DateTime.MinValue)
					toolTip  +=  Cst.CrLf + Ressource.GetString_LastModifyBy(dtUpd, displayName, false);
			}
			mnu[1]           = new skmMenu.MenuItemParent(0);
			mnu[1].Enabled   = isEnabled;
			mnu[1].Hidden    = (false == isEnabled);
			mnu[1].aToolTip  = toolTip;
			mnu[1].eImageUrl = imagePath;
			mnu[1].eUrl      = JavaScript.GetWindowOpenNotepad(tableName, valueDataKeyField, idMenu, referential.dataRow[referential.IndexColSQL_KeyField].ToString(), 
				referential.consultationMode, (TypeData.IsTypeString(referential.Column[referential.IndexDataKeyField].DataType) ? "1":"0"),
				listType);
			#endregion NOTEPAD

			#region ATTACHEDDOC
			isEnabled = (true == referential.AttachedDocSpecified && true == referential.AttachedDoc);
			isEnabled &= (referential.Column[referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());

			// to disable: set isEmpty to true            
			if (true == isEnabled)
				isEmpty = (referential.dataRow[Cst.AliasATTACHEDDOCACTOR + "_" + "DISPLAYNAME"] == null || StrFunc.IsEmpty(referential.dataRow[Cst.AliasATTACHEDDOCACTOR + "_" + "DISPLAYNAME"].ToString().Trim()));
			else
				isEmpty = true;
			if(true == isEmpty)
			{
				imagePath = Cst.Img_AttachedDoc;
				toolTip   = Ressource.GetString("btnAttachedDoc");
			}
			else
			{
				string   displayName = referential.dataRow[Cst.AliasATTACHEDDOCACTOR + "_" + "DISPLAYNAME"].ToString();
				DateTime dtUpd;
				if(referential.dataRow[Cst.AliasATTACHEDDOC + "_" + "DTUPD"] == DBNull.Value)
					dtUpd = DateTime.MinValue;
				else
					dtUpd = Convert.ToDateTime(referential.dataRow[Cst.AliasATTACHEDDOC + "_" + "DTUPD"]);
				imagePath = Cst.Img_AttachedDocFull;
				toolTip   = Ressource.GetString("btnAttachedDoc") ;
				if(dtUpd != DateTime.MinValue)
					toolTip  +=  Cst.CrLf + Ressource.GetString_LastModifyBy(dtUpd, displayName, false);
			}
			mnu[2]           = new skmMenu.MenuItemParent(0);
			mnu[2].Enabled   = isEnabled;
			mnu[2].Hidden    = (false == isEnabled);
			mnu[2].aToolTip  = toolTip;
			mnu[2].eImageUrl = imagePath;
			//mnu[2].eUrl      = JavaScript.GetWindowOpenAttachedDoc();

			string tableAttachedDoc;
			tableAttachedDoc = Cst.OTCml_TBL.ATTACHEDDOC.ToString();
			if(TypeData.IsTypeString(referential.Column[referential.IndexDataKeyField].DataType))
				tableAttachedDoc += "S";

			string keyFieldValue    = referential.dataRow[referential.IndexColSQL_KeyField].ToString();
			string descriptionValue = string.Empty;
			if (referential.ExistsColumnDESCRIPTION && (referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString().Length>0))
				descriptionValue = referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString();           

			mnu[2].eUrl      = JavaScript.GetWindowOpenAttachedDoc(tableAttachedDoc, valueDataKeyField, keyFieldValue,descriptionValue,idMenu, referential.TableName);

			#endregion ATTACHEDDOC

			phToolBar.ID = "tbrAction" + pSuffix;
			td.Controls.Add(phToolBar);
			mnuToolBar.DataSource     = mnu.initXmlWriter(null,null,null);
			mnuToolBar.Layout         = skmMenu.MenuLayout.Horizontal;
			mnuToolBar.LayoutDOWN     = skmMenu.MenuLayoutDOWN.DOWN;
			//mnuToolBar.MenuItemClick += new skmMenu.MenuItemClickedEventHandler(OnAction);
			phToolBar.Controls.Add(mnuToolBar);
			pTr.Cells.Add(td);
			pTr.Cells.Add(ControlsTools.WebTdSeparator(true));
		}
      
		#endregion
		
		#region protected AddButtonChildReferential
		protected void AddButtonChildReferential(string pSuffix, TableRow pTr )
		{
			//Add button for acces child referential (e.g.: Book for Actor)
			string eText = Ressource.GetString("btnDetail");
			
			SpheresMenu.Menus menus = SessionTools.Menus;
			isExistMenuChild =  (null != menus);
			//
            if (referential.consultationMode == Cst.ConsultationMode.ReadOnlyWithoutChildren)
                isExistMenuChild = false;
            else
			    isExistMenuChild = isExistMenuChild && menus.IsParent(idMenu);
			if (isExistMenuChild)
			{
				skmMenu.MenuItemParent menuRoot;
				menuRoot = menuDetail[0];
				CreateChildsMenu(idMenu, ref menuRoot, menus);
				menuDetail[0] = menuRoot;
			}

			menuDetail[0].Enabled  = true;
			menuDetail[0].eText     = eText;
			menuDetail[0].eImageUrl = Cst.Img_Detail;
			int indexplh = (pSuffix == "_Head" ? 0:1);
			plhMenuDetail[indexplh]    = new PlaceHolder();
			plhMenuDetail[indexplh].ID = "plhMenuDetail" + pSuffix;
				
			pTr.Cells.Add(ControlsTools.WebTdSeparator());

			TableCell td               = new TableCell();
			td.ID                      = "tdTbrScreen" + pSuffix;
			td.VerticalAlign           = VerticalAlign.Top;
			td.HorizontalAlign         = HorizontalAlign.Left;

			td.Controls.Add(plhMenuDetail[indexplh]);
			pTr.Cells.Add(td);
                
		}
		#endregion
		#region protected AddButtonValidate 	
		protected void AddButtonValidate( string pSuffix, TableRow pTr )
		{      
			WCToolTipButton button = ControlsTools.GetButtonAction("btnValidate", pSuffix, true);
			button.Click += new EventHandler( this.OnValidateClick );

			TableCell td       = new TableCell();
			td.ID              = "td" + button.ID;
			td.VerticalAlign   = VerticalAlign.Middle;
			td.HorizontalAlign = HorizontalAlign.Left;
			td.Controls.Add(button);
			pTr.Cells.Add(td);
		}
		#endregion
		#region protected AddButtonDuplicate 	
		protected void AddButtonDuplicate(string pSuffix, TableRow pTr )
		{
			//
			bool isEnabled = ( referential.Modify && referential.Create );
			//
            if (isEnabled)
            {
                isEnabled = (null != SessionTools.License);
                if (isEnabled)
                    isEnabled = (SessionTools.License.IsLicFunctionalityAuthorised_Add(referential.TableName, true));
            }
			//
            WCToolTipButton button = ControlsTools.GetButtonDuplicate("btnDuplicate", pSuffix, isEnabled);
			button.Click += new EventHandler( this.OnDuplicateClick );
			//
			string js  = Ressource.GetString("Msg_Duplicate");
			js  = "return confirm(" + JavaScript.JSString(js) + ");";
			button.Attributes.Add("onclick", js);
			//
			TableCell td       = new TableCell();
			td.ID              = "td" + button.ID;
			td.Wrap            = false;
			td.VerticalAlign   = VerticalAlign.Middle;
			td.HorizontalAlign = HorizontalAlign.Left;
			td.Controls.Add( new LiteralControl(Cst.HTMLSpace + Cst.HTMLSpace) );
			td.Controls.Add(button);
			pTr.Cells.Add(td);
		}
		#endregion
		#region protected AddButtonRecord
		protected void AddButtonRecord (string pSuffix, TableRow pTr, string pBtnName)
		{
			bool isEnabled =  ((referential.isNewRecord && referential.Create) || referential.Modify );
			//
			if ((isEnabled) && (referential.isNewRecord && referential.Create ))
			{
                isEnabled = (null != SessionTools.License);
                if (isEnabled)
                    isEnabled = (SessionTools.License.IsLicFunctionalityAuthorised_Add(referential.TableName, true));
				if (false == isEnabled)
				{
					string msg = Ressource.GetString("Msg_LicFunctionality");
					if (!StrFunc.ContainsIn(pageFooterLeft, msg))
						pageFooterLeft += msg;
					this.MsgForAlertImmediate = msg;
				}
			}
			//	
            WCToolTipButton button = ControlsTools.GetButtonAction(pBtnName, pSuffix, isEnabled);
			button.CommandName = pBtnName;
			button.Command += new CommandEventHandler( this.OnRecordClick );
			//
			bool RecordAuthorized = true;
			if (referential.ExistsColumnROWATTRIBUT)
			{
				if (referential.dataRow[referential.IndexColSQL_ROWATTRIBUT].ToString() == Cst.RowAttribut_System)
					RecordAuthorized = false;
			}
			if (!RecordAuthorized)
			{
				string js = "alert('" + Ressource.GetStringForJS("Msg_DataSystem") + "');return false;";
				button.Attributes.Add("onclick", js);
			}
			
			TableCell td       = new TableCell();
			td.ID              = "td" + button.ID;
			td.VerticalAlign   = VerticalAlign.Middle;
			td.HorizontalAlign = HorizontalAlign.Left;
			td.Controls.Add(button);
			pTr.Cells.Add(td);
		}
		#endregion
		#region protected AddButtonCancel
		protected void AddButtonCancel( string pSuffix, TableRow pTr )
		{
            WCToolTipButton button = ControlsTools.GetButtonCancel(pSuffix, false);

			TableCell td       = new TableCell();
			td.ID              = "td" + button.ID;
			td.VerticalAlign   = VerticalAlign.Middle;
			td.HorizontalAlign = HorizontalAlign.Left;
			td.Controls.Add(button);
			pTr.Cells.Add(td);
		}
		#endregion
		#region protected AddButtonClose
		protected void AddButtonClose (string pSuffix, TableRow pTr)
		{
			//Button button = ControlsTools.GetButtonClose(pSuffix,false);
            WCToolTipImageButton button = ControlsTools.GetButtonClose2(pSuffix, false);
            //
			TableCell td       = new TableCell();
			td.ID              = "td" + button.ID;
			td.VerticalAlign   = VerticalAlign.Middle;
			td.HorizontalAlign = HorizontalAlign.Left;
			td.Controls.Add(button);
			pTr.Cells.Add(td);
		}
		#endregion
		#region protected AddButtonRemove
		protected void AddButtonRemove(  string pSuffix, TableRow pTr )
		{
			//20070619 PL/CC
			//bool isEnabled = ( referential.Modify && referential.Remove );
			bool isEnabled = referential.Remove;
			//
			WCToolTipButton button = ControlsTools.GetButtonRemove("btnRemove", pSuffix, isEnabled);
			button.Click += new EventHandler( this.OnRemoveClick );
			
			string js, msg = string.Empty;
			bool RemoveAuthorized = true;
			if (referential.ExistsColumnROWATTRIBUT)
			{
				switch (referential.dataRow[referential.IndexColSQL_ROWATTRIBUT].ToString())
				{
					case Cst.RowAttribut_System:
						RemoveAuthorized = false;
						msg = "Msg_DataSystem";
						break;
					case Cst.RowAttribut_Protected:
						RemoveAuthorized = false;
						msg = "Msg_DataProtected";
						break;
				}
			}
			if (RemoveAuthorized)
			{
				js  = Ressource.GetString("Msg_Remove") + Cst.CrLf + Cst.CrLf;
				js += Ressource.GetStringForJS(idMenu) + ": " + referential.dataRow[referential.IndexColSQL_KeyField].ToString();
				if (referential.ExistsColumnDESCRIPTION && StrFunc.IsFilled(referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString()))
					js += " - " + referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString();
				js  = "return confirm(" + JavaScript.JSString(js) + ");";
			}
			else
				js = "alert('" + Ressource.GetStringForJS(msg) + "');return false;";
			button.Attributes.Add("onclick", js);
					
			TableCell td       = new TableCell();
			td.ID              = "td" + button.ID;
			td.Wrap            = false;//Nécessaire pour le look sous Firefox (20060308 PL)
			td.VerticalAlign   = VerticalAlign.Middle;
			td.HorizontalAlign = HorizontalAlign.Left;
			td.Controls.Add( new LiteralControl(Cst.HTMLSpace + Cst.HTMLSpace) );
			td.Controls.Add(button);
			pTr.Cells.Add(td);
			//            form.Controls.Add( button );
			//            form.Controls.Add( new LiteralControl(Cst.HTMLSpace) );
		}
		#endregion
		
		#region protected OnValidateClick 
		protected void OnValidateClick( object sender, EventArgs e )
		{
			ReturnAndClose();
		}
		#endregion
		#region protected OnDuplicateClick 	
		protected void OnDuplicateClick( object sender, EventArgs e )
		{
			//On reload la page pour simuler une création
			string url  = this.Request.RawUrl;
			url  = url.Replace("&N=0","&N=1");                
			url  = url.Replace("&OCR=1", string.Empty);
			url += "&DPC=1"; //DPC:DuPliCate
			Server.Transfer(url);
		}
		#endregion
		#region protected OnRecordClick
		protected void OnRecordClick( object sender, CommandEventArgs e )
		{
			if (Page.IsValid) 
			{
				//
				ReferentialTools.UpdateDataRowFromControls2(this, ref referential);
				//
				switch (e.CommandName)
				{
					case "btnApply":
						Save(false);
						break ;
					default:
						Save(true);
						break;
				}
			}
		}
		#endregion
		#region protected OnRemoveClick
		protected void OnRemoveClick( object sender, EventArgs e )
		{
			//Delete principal
			referential.dataRow.Delete();
			//Delete EXTLID si existant
			if (referential.drExternal != null)
			{
				for (int i=0;i < referential.drExternal.GetLength(0);i++)
					referential.drExternal[i].Delete();
			}
			Save(true);
		}
		#endregion
		#region protected OnXmlClick
		protected void OnXmlClick( object sender, ImageClickEventArgs e )
		{
			try
			{
				string clientId = ((Control)sender).ID;  // Que sur table Main
				string col = clientId; 
				//
				if (StrFunc.IsFilled(col))
				{
					col = StrFunc.PutOffSuffixNumeric(col);   
					if (3 <= col.Length)  
						col =  col.Substring(3);
				}
				//
				int i = referential.GetIndexColSQL(col);    
				if (i > - 1)
				{
					string strXml = referential.dataRow[i].ToString();
					if (StrFunc.IsFilled(strXml))
						DisplayXml("Referential_XML", "Referential_" + referential.TableName,referential.dataRow[i].ToString());
				}
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("PageBaseReferential.OnXmlClick", ex);}
		}
		#endregion OnXmlClick

		#region private InitVariables
		private void InitVariables(int pColumnByRow, out int opTotalColumnByRow, out string opdefaultInputWidth, out string opdefaultLabelWidth)
		{
			if (pColumnByRow<=0)
				pColumnByRow = referential.ColumnByRow;
			if (pColumnByRow==0)
				pColumnByRow=2;
			opTotalColumnByRow = pColumnByRow*4;//*4 --> label + textbox + empty zone before label and textbox
			opdefaultLabelWidth = @" Width=""" + Convert.ToString((100*1/4) / (pColumnByRow)) + @"%""";
			opdefaultInputWidth = @" Width=""" + Convert.ToString((100*3/4) / (pColumnByRow)) + @"%""";
		}
		#endregion
		#region private CreateChildsMenu
        private void CreateChildsMenu(string pIdMenu, ref skmMenu.MenuItemParent opMenuRoot, SpheresMenu.Menus pSessionToolsMenus)
        {
            //			Debug.WriteLine("================================================");
            //			Debug.WriteLine("Menu: "+pIdMenu);
            //			Debug.WriteLine("================================================");
            #region Décompte du nombre de menus enfants
            int nbChildMenu = 0;

            SpheresMenu.Menu  menu;
            ArrayList alMenuChild = new ArrayList();
            for (int i = 0; i < pSessionToolsMenus.Count; i++)
            {
                menu = (SpheresMenu.Menu)pSessionToolsMenus[i];
                //20080108 PL
                //if (menu.IdMenu_Parent == pIdMenu)
                if ((menu.IdMenu_Parent == pIdMenu) && (menu.IsEnabled))
                {
                    nbChildMenu++;
                    alMenuChild.Add(menu);
                }
            }

            //			Debug.WriteLine("Menus enfants: "+nbChildMenu.ToString());
            //			Debug.WriteLine("================================================");
            #endregion
            opMenuRoot = new skmMenu.MenuItemParent(nbChildMenu);

            #region Affecting childs of current menu
            int current = 0;
            for (int i = 0; i < alMenuChild.Count; i++)
            {
                menu = (SpheresMenu.Menu)alMenuChild[i];
                #region Menu enfant
                //20080108 PL
                //if (menu.IdMenu_Parent.ToString() == pIdMenu)
                if ((menu.IdMenu_Parent == pIdMenu) && (menu.IsEnabled))
                {
                    //					Debug.WriteLine("	Menu: " + menu.IdMenu);
                    //					Debug.WriteLine("------------------------------------------------");

                    #region Creation du menu enfant
                    //string label = Ressource.GetString(menu.IdMenu.ToString());
                    string label = Ressource.GetMenu(menu.IdMenu, menu.Displayname);
                    string subTitle = Ressource.GetMenu(menu.IdMenu_Parent, menu.IdMenu_Parent);

                    StringBuilder s = new StringBuilder();
                    //s.Append(@"../" + menu.Url + "&IDMenu=" + menu.IdMenu);
                    s.Append(menu.Url + "&IDMenu=" + menu.IdMenu);
                    s.Append("&FK=" + valueDataKeyField);
                    s.Append("&SubTitle=" + subTitle + ": " + referential.dataRow[referential.IndexColSQL_KeyField].ToString());

                    if (referential.ExistsColumnDESCRIPTION && (referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString().Length > 0))
                        s.Append(" - " + referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString());
                    s.Append(@"&");

                    //20051019 PL Replace de la ligne suivante
                    //if (pSessionToolsMenus.IsParent(menu.IdMenu))
                    if ((!menu.IsAction) && (pSessionToolsMenus.IsParent(menu.IdMenu)))
                    {
                        skmMenu.MenuItemParent menuRoot;
                        menuRoot = opMenuRoot[current];
                        CreateChildsMenu(menu.IdMenu, ref menuRoot, pSessionToolsMenus);
                        opMenuRoot[current] = menuRoot;
                    }

                    bool isEnabled = (!referential.isNewRecord);
                    opMenuRoot[current].Enabled = isEnabled;
                    opMenuRoot[current].eText = label;
                    if (menu.IsAction)
                        opMenuRoot[current].eUrl = JavaScript.GetWindowOpen(s.ToString(), Cst.WindowOpenStyle.OTCml_FormReferential);
                    else
                        opMenuRoot[current].eUrl = string.Empty;
                    opMenuRoot[current].eLayout = skmMenu.MenuLayout.Vertical.ToString();
                    #endregion Creation du menu enfant

                    current++;
                }
                #endregion Menu enfant
            }
            #endregion
        }        
		
		#endregion
		#region private WriteControls
		private void WriteControls()
		{
			int columnRemaining  = 0;
			int totalColumnByRow = 0;
			int numBlockOnRow    = 0;
			int blockRemaining   = 0;
			int indexColSQL      =-1;
			string defaultInputWidth, defaultLabelWidth;          

			bool isVisible   = false;
			bool isNewTR     = false;
			bool isDebugEven = false;
			string label = string.Empty, html = string.Empty;
			string checkBoxAlignment = "Right";
            
			ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn();
			
			InitVariables(referential.ColumnByRow, out totalColumnByRow, out defaultInputWidth, out defaultLabelWidth);
			
			for (int indexEltXML=0;indexEltXML<referential.Column.Length;indexEltXML++)
			{
				rrc = referential.Column[indexEltXML];

				if (referential.isNewRecord && rrc.IsExternal 
                    && (false == (referential.IsForm && referential.IndexKeyField != -1)))
				{
					rrc.IsHide = true;
					rrc.IsHideSpecified = true;
					if (rrc.html_BLOCK != null)
						rrc.html_BLOCK = null;
				}
                
				if (rrc.Ressource == null)
                    rrc.Ressource = string.Empty;

				indexColSQL++;
				if (rrc.ExistsRelation)
					indexColSQL++;                

				//20051026 PL Modif de la ligne suivante pour permettre la visualisation des Identity (ie: ISSI)
				//isVisible = !(rrc.IsIdentity || rrc.IsHide);
				isVisible  = (!rrc.IsHide && !(referential.isNewRecord && rrc.IsIdentity)) ; 
				isNewTR    = ( (columnRemaining == 0) && !rrc.IsLastControlLinked );
				
				html = string.Empty;
				bool setBLOCK = (rrc.html_BLOCK!=null) || (indexEltXML==0); 
				bool setHR = false;
				bool isTextBoxModeMultiLine = (rrc.TextMode == TextBoxMode.MultiLine.ToString()) 
					||(TypeData.IsTypeString(rrc.DataType) && (rrc.Length >= 1000));
				//
				setHR = (rrc.html_HR!=null) || (indexEltXML==referential.Column.Length);
				setHR = setHR || (isTextBoxModeMultiLine && !setBLOCK && !rrc.IsHide);

				#region html_TABLE
				if (setBLOCK)
				{
					if (blockRemaining == 0)
						if ((rrc.html_BLOCK!=null) && (rrc.html_BLOCK[0].blockbyrowSpecified) && (rrc.html_BLOCK[0].blockbyrow > 1))
						{
							numBlockOnRow  = rrc.html_BLOCK[0].blockbyrow;
							blockRemaining = numBlockOnRow;
						}

					if (rrc.html_BLOCK!=null)
						InitVariables(rrc.html_BLOCK[0].columnbyrow, out totalColumnByRow, out defaultInputWidth, out defaultLabelWidth);
					else if (rrc.html_HR!=null)
						InitVariables(rrc.html_HR[0].columnbyrow, out totalColumnByRow, out defaultInputWidth, out defaultLabelWidth);

					//if no group block or first block of group
					if ((blockRemaining == 0) || (blockRemaining == numBlockOnRow))
					{
						html = @"<TABLE WIDTH=""100%"" border="""+borderDesign+@""" bordercolor="""+(isDebugEven? "DarkGray":"Black")+@""" cellSpacing=""1"" cellPadding=""1"">";
						isDebugEven = !isDebugEven;
						//Empty row
						html += @"<tr>";
						if ((numBlockOnRow > 0) && (blockRemaining == numBlockOnRow))//First
						{
							if (rrc.html_BLOCK[0].width==null)
							{
								for (int n=0;n<(numBlockOnRow*2)-1;n+=2)
								{
									if (n>0)
										html += @"<td>&nbsp;</td>";
									html += @"<td style=""COLOR: white"" Width=""" + Convert.ToString(100 / (numBlockOnRow)) + @"%"">.</td>";
								}
							}
							else
							{
								string width="100%";
								int lastEltXML=indexEltXML;
								ReferentialsReferentialColumn rrc_tmp;
								for (int n=0;n<(numBlockOnRow*2)-1;n+=2)
								{
									if (n>0)
										html += @"<td>&nbsp;</td>";
									for (lastEltXML=lastEltXML;lastEltXML<referential.Column.Length;lastEltXML++)
									{
										rrc_tmp = referential.Column[lastEltXML];
										if (rrc_tmp.html_BLOCK!=null)
										{
											if (rrc_tmp.html_BLOCK[0].width!=null)
												width = rrc_tmp.html_BLOCK[0].width;
											lastEltXML++;
											break;
										}
									}
									html += @"<td style=""COLOR: white"" Width=""" + width + @""">.</td>";
								}
							}
						}
						else
							//Create empty row
							for (int n=0;n<totalColumnByRow;n+=4)
							{
								html += @"<td width=""1"">&nbsp;</td><td style=""COLOR: white"""+defaultLabelWidth+">.</td>";
								html += @"<td>&nbsp;</td><td style=""FONT-SIZE: 1px; COLOR: white"""+defaultInputWidth+">.</td>";
							}
						html += @"</tr>";
						CellForm.Controls.Add( new LiteralControl( html ));
					}
					if (blockRemaining > 0)
					{
						string width = Convert.ToString(100/numBlockOnRow)+"%";
						html =string.Empty;

						if ( (rrc.html_BLOCK!=null) && (rrc.html_BLOCK[0].width!=null) )
							width = rrc.html_BLOCK[0].width;

						if (blockRemaining == numBlockOnRow)
							html += Cst.CrLf +  @"<TR>";
						html += @"<TD width="""+width+@""" align=""left"" valign=""top"">";
						html += @"<TABLE WIDTH=""100%"" border="""+borderDesign+@""" bordercolor=""yellow"" cellSpacing=""1"" cellPadding=""1"">";
						CellForm.Controls.Add( new LiteralControl( html ));
					}					
				}
				#endregion 
				#region html_TITLE
				/*if (setTITLE)
				{
					string title=null, color=null;
					if ( (rrc.html_TITLE!=null) && (rrc.html_TITLE[0].color!=null) )
						color = rrc.html_TITLE[0].color;
					if ( (rrc.html_TITLE!=null) && (rrc.html_TITLE[0].title!=null) )
						title = rrc.html_TITLE[0].title;
					form.Controls.Add( new LiteralControl( ControlsTools.GetStaticTitleHtmlPage( this, title, color, rrc.Colspan ) ));
					//isNewTR = true;
				}*/
				#endregion
				#region html_BLOCK
				if (setBLOCK)
				{
					string title=null, color=null;
					if ( (rrc.html_BLOCK!=null) && (rrc.html_BLOCK[0].color!=null) )
						color = rrc.html_BLOCK[0].color;
					if ( (rrc.html_BLOCK!=null) && (rrc.html_BLOCK[0].title!=null) )
						title = rrc.html_BLOCK[0].title;
					CellForm.Controls.Add( new LiteralControl( ControlsTools.GetStaticBlockHtmlPage( this, title, color, totalColumnByRow ) ));
					isNewTR = true;
				}
				#endregion 
				#region html_HR
				if (setHR)
				{
					string title=null, color=null, size=null;
					if (rrc.html_HR!=null)
					{
						if (rrc.html_HR[0].title!=null)
							title = rrc.html_HR[0].title;
						if (rrc.html_HR[0].color!=null)
							color = rrc.html_HR[0].color;
						if (rrc.html_HR[0].size!=null)
							size = rrc.html_HR[0].size;
					}
					CellForm.Controls.Add( new LiteralControl( ControlsTools.GetHRHtmlPage( color, totalColumnByRow, size, title ) ));
					isNewTR = true;
				}
				#endregion 

				if (isVisible)
				{
					string labelWidth = defaultLabelWidth;
					string inputWidth = defaultInputWidth;
					try
					{
						bool isPercent = false;

						if (rrc.Ressource == "")
							rrc.LabelWidth = @" Width=""" + 0.ToString() + @"""";

						if (rrc.LabelWidth.Length>0)
							labelWidth = @" Width=""" + rrc.LabelWidth + @"""";
						else if (false && rrc.Colspan > 1) 
						{
							isPercent = (labelWidth.IndexOf("%")>1);
							labelWidth = labelWidth.Replace("%", string.Empty);
							labelWidth = labelWidth.Replace("Width=", string.Empty);
							labelWidth = labelWidth.Replace(@"""", string.Empty);
							labelWidth = labelWidth.Trim();
							labelWidth = @" Width="""+Convert.ToString( Convert.ToInt32(labelWidth)*rrc.Colspan );
							labelWidth += (isPercent ? "%" : string.Empty) + @"""";
						}
						//if (rrc.InputWidth.Length>0)
                        if ((rrc.InputWidth.Length > 0) && ((!rrc.HasInformationControls) || (!rrc.Information.LabelMessageSpecified)))// 20080619 PL
							inputWidth = @" Width=""" + rrc.InputWidth + @"""";
						else if (rrc.Colspan > 1)
						{
							string _labelWidth = labelWidth.Replace("%", string.Empty);
							_labelWidth = _labelWidth.Replace("Width=", string.Empty);
							_labelWidth = _labelWidth.Replace(@"""", string.Empty);
							_labelWidth = _labelWidth.Trim();
						
							isPercent = (inputWidth.IndexOf("%")>1);
							inputWidth = inputWidth.Replace("%", string.Empty);
							inputWidth = inputWidth.Replace("Width=", string.Empty);
							inputWidth = inputWidth.Replace(@"""", string.Empty);
							inputWidth = inputWidth.Trim();
							inputWidth = @" Width="""+Convert.ToString( (Convert.ToInt32(inputWidth)*rrc.Colspan) +  (Convert.ToInt32(labelWidth)*(rrc.Colspan-1)));
							inputWidth += (isPercent ? "%" : string.Empty) + @"""";
						}
						if(rrc.IsFirstControlLinked)
							inputWidth = GetWidthControlLinked(rrc.InputWidth);
					}
					catch
					{}

					if (isNewTR)
					{
						columnRemaining = totalColumnByRow;
						html = "<TR>";
					}
					else
						html = string.Empty;

					#region CheckBox (TextAlign Right) / RadioButton
					if (TypeData.IsTypeBool(rrc.DataType) && (checkBoxAlignment=="Right"))
					{
						html += @"<TD>&nbsp;</TD><TD";
                        
						if (rrc.Colspan<=0)
						{
							//colspan until end row
							html += @" colspan="""+(columnRemaining -1).ToString()+@"""";
							columnRemaining=0;
						}
						else
						{
							//
							html += @" colspan="""+(3 + ((rrc.Colspan-1)*4)).ToString()+@"""";
							columnRemaining-=(4 + ((rrc.Colspan-1)*4));
						}
						html += @">" + Cst.CrLf;                        

						//Information Controls
						if (rrc.HasInformationControls)
						{
							html += @"<TABLE WIDTH=""100%"" border=""" + borderDesign + @""" bordercolor=""Green"" cellSpacing=""0"" cellPadding=""0""><TR>";
							if ((!rrc.Information.LabelMessageSpecified) && (!ReferentialTools.IsDataForDDL(rrc)))
								html += @"<TD WIDTH=""100%"" nowrap>";
							else 
								html += @"<TD nowrap>";
						}
						CellForm.Controls.Add( new LiteralControl( html ));

                        if (rrc.ControlMainSpecified)
                        {
                            if (rrc.DataType.StartsWith("bool2"))
                            {
                                RadioButton radioButton = (RadioButton)rrc.ControlMain;
                                string[] text = radioButton.Text.Split("|".ToCharArray());
                                radioButton.GroupName = radioButton.ID;
                                radioButton.Text = text[0];
                                radioButton.ToolTip = string.Empty;
                                RadioButton radioButton1 = new RadioButton();
                                radioButton1.AutoPostBack = radioButton.AutoPostBack;
                                radioButton1.Checked = !radioButton.Checked;
                                radioButton1.CssClass = radioButton.CssClass;
                                radioButton1.Enabled = radioButton.Enabled;
                                radioButton1.EnableViewState = radioButton.EnableViewState;
                                radioButton1.GroupName = radioButton.GroupName;
                                radioButton1.ID = radioButton.ID + "2";
                                if (text.Length > 1)
                                    radioButton1.Text = text[1];
                                radioButton1.Visible = radioButton.Visible;
                                //
                                CellForm.Controls.Add(radioButton);
                                if (rrc.DataType == "bool2h")
                                    CellForm.Controls.Add(new LiteralControl(Cst.HTMLSpace2));
                                else
                                    CellForm.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
                                CellForm.Controls.Add(radioButton1);
                            }
                            else
                                CellForm.Controls.Add(rrc.ControlMain);
                        }

						//Information Controls
						if (rrc.HasInformationControls)
						{
							bool isExistLabelMessage = (rrc.InformationControls[1] != null);
							html  = @"</TD><TD width=""1"">&nbsp;</TD>";
							html += @"<TD" + (isExistLabelMessage ? string.Empty:@" width=""100%""") +">";
							CellForm.Controls.Add( new LiteralControl( html ));
							CellForm.Controls.Add( rrc.InformationControls[0] );
							if (isExistLabelMessage)
							{
								html = @"</TD><TD width=""1"">&nbsp;</TD><TD width=""100%"">";
								CellForm.Controls.Add( new LiteralControl( html ));

								CellForm.Controls.Add( rrc.InformationControls[1] );
							}
							html = @"</TD></TR></TABLE>";
							CellForm.Controls.Add( new LiteralControl( html ));
						}

						CellForm.Controls.Add( new LiteralControl( @"</TD>" ));
                    }
                    #endregion CheckBox (TextAlign Right) / RadioButton
                    #region Control of capture (Textbox, WCTextBox, DDL, Checkbox (TextAlign Left), ...)
                    else
					{
						#region Add Label
						if (! (rrc.IsLastControlLinked || rrc.IsMiddleControlLinked) )
						{
							html += @"<TD width=""1"">&nbsp;</TD><TD" + labelWidth;
							if (isTextBoxModeMultiLine)
								html += @" valign=""top""";
							html += ">";
							CellForm.Controls.Add( new LiteralControl( html ));
							columnRemaining-=2;

							if(rrc.ControlLabelSpecified)
								CellForm.Controls.Add(rrc.ControlLabel);

							CellForm.Controls.Add( new LiteralControl( @"</TD>" ));
						}
						#endregion Add Label
						
						html = string.Empty;
						if (!(rrc.IsLastControlLinked || rrc.IsMiddleControlLinked))
						{
							html = @"<TD width=""1"">&nbsp;</TD><TD";
							if (rrc.Colspan<=0)
							{
								//colspan until end row
								html += @" colspan="""+(columnRemaining -1).ToString()+@"""";
								columnRemaining=0;
							}
							else
							{
								html += @" colspan="""+(1 + ((rrc.Colspan-1)*4)).ToString()+@"""";
								columnRemaining-=(2 + ((rrc.Colspan-1)*4));
							}
							if (!rrc.IsFirstControlLinked)//GlopPL 20031215 add if
								//if (!((Cst.IsTypeForCalendar(rrc.DataType) || rrc.HasInformationControls)))//glopkk
								if (!TypeData.IsTypeDateOrDateTime(rrc.DataType))
									html += inputWidth; 
							html += ">"; 
						}
						if ( ArrFunc.IsFilled(rrc.OtherControls) || rrc.HasInformationControls)
						{
							//Create table with 2 columns for TextBox and WCImgCalendar or columns with information controls
							if (TypeData.IsTypeDate(rrc.DataType))
								html += @"<TABLE WIDTH=""120px"" border=""" + borderDesign + @""" bordercolor=""Green"" cellSpacing=""0"" cellPadding=""0"">";
							else if (TypeData.IsTypeDateTime(rrc.DataType))
								html += @"<TABLE WIDTH=""180px"" border=""" + borderDesign + @""" bordercolor=""Green"" cellSpacing=""0"" cellPadding=""0"">";
							else
								html += @"<TABLE WIDTH=""100%"" border=""" + borderDesign + @""" bordercolor=""Green"" cellSpacing=""0"" cellPadding=""0"">";
                            html += Cst.CrLf + @"<TR>";
							//
							if (TypeData.IsTypeDateOrDateTime(rrc.DataType))
								//Cela permet de cadrer le bouton calendar à droite
								html += @"<TD WIDTH=""100%"">";
                            else if ((null != rrc.Information && !rrc.Information.LabelMessageSpecified) && (!ReferentialTools.IsDataForDDL(rrc)))
                                //Cela permet de cadrer l'image (i ou !) à droite
                                html += @"<TD WIDTH=""100%"">";
                            else
                            {
                                if (StrFunc.IsFilled(rrc.InputWidth))//20080619 PL add du if()
                                {
                                    Unit customWidth;
                                    if (rrc.InputWidth.EndsWith("%") && rrc.InputWidth.Length > 1)
                                        customWidth = Unit.Percentage(Convert.ToInt32(rrc.InputWidth.Substring(0, rrc.InputWidth.Length - 1)));
                                    else if (rrc.InputWidth.EndsWith("px") && rrc.InputWidth.Length > 2)
                                        customWidth = Unit.Pixel(Convert.ToInt32(rrc.InputWidth.Substring(0, rrc.InputWidth.Length - 2)));
                                    else
                                        customWidth = Unit.Pixel(Convert.ToInt32(rrc.InputWidth));
                                    html += @"<TD width="""+ customWidth +@""">";
                                }
                                else
                                    html += @"<TD>";
                            }
						}
						else if (rrc.IsFirstControlLinked )
							//Create table with n columns for columns linked	
							html += @"<TABLE WIDTH=""100%"" border="""+borderDesign+@""" bordercolor=""Red"" cellSpacing=""0"" cellPadding=""0""><TR><TD " + inputWidth + " >";                            
						//
                        CellForm.Controls.Add( new LiteralControl( html ));
						//GlopPL gérer rrc.Scale                       

						#region CheckBox (Align Left)
						if (TypeData.IsTypeBool(rrc.DataType))
						{
                            if (rrc.ControlMainSpecified)
                            {
                                CellForm.Controls.Add(rrc.ControlMain);
                            }

							//Information Controls
							if (rrc.HasInformationControls)
							{
								html = @"</TD><TD width=""1"">&nbsp;</TD><TD width=""15"">";
								CellForm.Controls.Add( new LiteralControl( html ));

								CellForm.Controls.Add( rrc.InformationControls[0] );

								html = @"</TD><TD width=""1"">&nbsp;</TD><TD width=""50%"">";
								CellForm.Controls.Add( new LiteralControl( html ));

								CellForm.Controls.Add( rrc.InformationControls[1] );

								html = @"</TD><TD width=""100%"">&nbsp;</TD></TR></TABLE>";
								CellForm.Controls.Add( new LiteralControl( html ));
							}
						}
							#endregion CheckBox (Align Left)
							#region TextBox, WCTextBox, DropDownList
						else
						{
							#region DropDownList,HtmlSelect
							if (ReferentialTools.IsDataForDDL(rrc))
							{
								if(rrc.ControlMainSpecified)
									CellForm.Controls.Add(rrc.ControlMain);
								else if(rrc.HtmlControlMainSpecified)
									CellForm.Controls.Add(rrc.HtmlControlMain);         
								//
                                if (!ReferentialTools.IsDataForDDLParticular(rrc) && (rrc.Relation[0].DDLType != null) && Cst.IsDDLTypeStyleComponentColor(rrc.Relation[0].DDLType.Value))
								{
									JavaScript.SetColor((PageBase)this);
									if ( !rrc.IsFirstControlLinked && !rrc.IsMiddleControlLinked && rrc.IsLastControlLinked)
									{
										string ctrlID = "preview" + rrc.FirstControlLinkedColumnName;
										html = @"</TD><TD width=""1%"">&nbsp;</TD><TD>";
										CellForm.Controls.Add( new LiteralControl( html ));
										HtmlInputText htmlInput = new HtmlInputText();
                                        htmlInput.Value = "PREVIEW";
                                        htmlInput.Style.Add("width", "70");
                                        htmlInput.Style.Add("align", "center");
										htmlInput.ID = ctrlID;
										CellForm.Controls.Add(htmlInput);
										html = @"</TD><TD width=""1%"">&nbsp;</TD><TD>";
										CellForm.Controls.Add( new LiteralControl( html ));
									}
								}
                                
								//Information Controls
								if (rrc.HasInformationControls)
								{
									bool isExistLabelMessage = (rrc.InformationControls[1] != null);
									html  = @"</TD><TD width=""1"">&nbsp;</TD>";
									html += @"<TD" + (isExistLabelMessage ? string.Empty:@" width=""100%""") +">";
									CellForm.Controls.Add( new LiteralControl( html ));
									CellForm.Controls.Add( rrc.InformationControls[0] );
									if (isExistLabelMessage)
									{
										html = @"</TD><TD width=""1"">&nbsp;</TD><TD width=""100%"">";
										CellForm.Controls.Add( new LiteralControl( html ));

										CellForm.Controls.Add( rrc.InformationControls[1] );
									}
									html = @"</TD></TR></TABLE>";
									CellForm.Controls.Add( new LiteralControl( html ));
								}
							}
								#endregion DropDownList,HtmlSelect
								#region TextBox, WCTextBox
							else
							{   					
								if(rrc.ControlMainSpecified)
									CellForm.Controls.Add(rrc.ControlMain);
								//
								if (TypeData.IsTypeDateOrDateTime(rrc.DataType) && ArrFunc.IsFilled(rrc.OtherControls) && rrc.OtherControls[0] != null)
								{   
									//Exist TR with 4 TD for TextBox and WCImgCalendar
									html = @"</TD><TD width=""1"">&nbsp;</TD><TD>";
									CellForm.Controls.Add( new LiteralControl( html ));

									WCImgCalendar imgCalendar = (WCImgCalendar)rrc.OtherControls[0];
									CellForm.Controls.Add( imgCalendar );

									html = "</TD></TR></TABLE>";
									CellForm.Controls.Add( new LiteralControl( html ));
								} 
								// GLOP FI 20070206 A faire plus tard....
								if (rrc.IsDataXml && ArrFunc.IsFilled(rrc.OtherControls) && rrc.OtherControls[0] != null)
								{   
									//Exist TR with 4 TD for TextBox and WCImgCalendar
									html = @"</TD><TD width=""1"">&nbsp;</TD><TD>";
									CellForm.Controls.Add( new LiteralControl( html ));

									ImageButton imgbutXml = (ImageButton)rrc.OtherControls[0];
									imgbutXml.Click       += new ImageClickEventHandler( OnXmlClick );
									CellForm.Controls.Add( imgbutXml );

									html = "</TD></TR></TABLE>";
									CellForm.Controls.Add( new LiteralControl( html ));
								}
								//
								//Information Controls
								if (rrc.HasInformationControls)
								{
									bool isExistLabelMessage = (rrc.InformationControls[1] != null);
									html  = @"</TD><TD width=""1"">&nbsp;</TD>";
									if (isExistLabelMessage)
										html += @"<TD width=""1"">";
									else
										html += @"<TD width=""95%"">";
									
									CellForm.Controls.Add( new LiteralControl( html ));
									CellForm.Controls.Add( rrc.InformationControls[0] );
									if (isExistLabelMessage)
									{
										html = @"</TD><TD width=""1"">&nbsp;</TD>" + Cst.CrLf;
                                        if (StrFunc.IsFilled(rrc.InputWidth))//20080619 PL add du if()
                                            html += @"<TD>" + Cst.CrLf;
                                        else
                                            html += @"<TD width=""60%"">" + Cst.CrLf;
										CellForm.Controls.Add( new LiteralControl( html ));
										CellForm.Controls.Add( rrc.InformationControls[1] );
									}
                                    html = Cst.CrLf + @"</TD></TR></TABLE>" + Cst.CrLf;
									CellForm.Controls.Add( new LiteralControl( html ));
								}
							}
							#endregion TextBox, WCTextBox

							if (rrc.IsFirstControlLinked || rrc.IsMiddleControlLinked)
							{   
								//Exist TR with n TD for columns linked
								html = @"</TD><TD width=""1"">&nbsp;</TD><TD ";
								html += @"width=" + GetWidthControlLinked(rrc.InputWidth);
								html += @">";
								CellForm.Controls.Add( new LiteralControl( html ));
							}
							else if (rrc.IsLastControlLinked)
							{   
								//Exist TR with n TD for columns linked
								//Ajout d'une cellule vide à taille non renseignée (<TD>&nbsp;</TD>) avant de fermer le table 
								// --> permet de forcer le resize uniquement sur cette cellule
								html = @"</TD><TD>&nbsp;</TD></TR></TABLE>";
								CellForm.Controls.Add( new LiteralControl( html ));
							}
							if (!(rrc.IsFirstControlLinked || rrc.IsMiddleControlLinked) )
								CellForm.Controls.Add( new LiteralControl( "</TD>" ));
						}
						#endregion TextBox, WCTextBox, DropDownList
					}
					#endregion Add Control of capture (Textbox, WCTextBox, DDL, Checkbox (TextAlign Left), ...)
				}                
               
				//Close TR balise if : 
				// - Last column of row							
				// - Last data
				// - Next data is html command
				// - Next data is EXTLLINK data

				bool isCloseTR    = false;
				bool isCloseTABLE = (indexEltXML==referential.Column.Length-1);
				html = string.Empty;

				if (!isCloseTABLE)
				{
					isCloseTABLE = ( (indexEltXML<referential.Column.Length-1) && (referential.Column[indexEltXML+1].html_BLOCK!=null) ); 
					isCloseTR = (isCloseTABLE || ((columnRemaining<=0) && !(rrc.IsFirstControlLinked || rrc.IsMiddleControlLinked) ));
					if (!isCloseTR)
					{
						isCloseTR = ( isCloseTR || ( (indexEltXML<referential.Column.Length-1) && (referential.Column[indexEltXML+1].html_HR!=null) ) ); 
						isCloseTR = ( isCloseTR || (indexColSQL+1==referential.IndexColSQL_EXTLLINK) ); 
					}
				}
				if (isCloseTR)
				{
					columnRemaining = 0;
					html = @"</TR>";
				}
				if (isCloseTABLE)
				{
					html += @"</TABLE>" + Cst.CrLf;
					
					if (blockRemaining > 0)
					{
						blockRemaining--;
						html += @"</TD>";
						if (blockRemaining == 0)//Last 
						{
							html += Cst.CrLf +  @"</TR></TABLE>";
						}
						else
							html += Cst.CrLf +  @"<TD>&nbsp;</TD>";
					}
				}
				if (html.Length>0)
					CellForm.Controls.Add( new LiteralControl( html ) );		
	
			}
			CellForm.Controls.Add( new LiteralControl( ControlsTools.GetHRHtmlPage( null, 0 ) ));
			
			//Add ValidationSummary control for all Validator controls
			ValidationSummary validationSummary = new ValidationSummary();
			validationSummary.ShowMessageBox = true;
			validationSummary.ShowSummary = false;
			CellForm.Controls.Add(validationSummary);
		}
		#endregion
		#region private Save
		private void Save(bool pIsSelfClose)
		{
			int rowsAffected          = 0;
			string message            = string.Empty;
			string error              = string.Empty;
			string keyField           = string.Empty;
			string keyFieldValue      = string.Empty;
			
			Cst.ErrLevel ret = ReferentialTools.SaveReferentialDatas(this, ref referential, out rowsAffected, out message, out error, out keyField, out keyFieldValue);
			
			if (ret ==  Cst.ErrLevel.SQLDEFINED)
				JavaScript.AlertImmediate(this, Ressource.GetString(message)  + Cst.CrLf + Cst.CrLf + "(" + error + ")", false);         
			else if (!(rowsAffected > 0))
				JavaScript.AlertImmediate(this, Ressource.GetString("Msg_NoModification"), false);
			else
			{
				if (referential.TableName == Cst.OTCml_TBL.INSTRUMENT.ToString())
				{
#warning 20050729 PL (temporaire, à revoir... insert template post new instr)
					//	Création d'un template pour les éventuels nouveaux instruments, à partir
					//  du template de l'instrument portant sur le même proudct et ayant le nom du product
					try
					{
						int nbRow = 0;
						string concat = DataHelper.GetSigneStringConcatenation(CS);  
						
						string sqlQuery;

						sqlQuery  = "insert into dbo.TRADE" + Cst.CrLf;
						sqlQuery += "(" + Cst.CrLf;
						sqlQuery += "IDI, IDENTIFIER, DISPLAYNAME, DESCRIPTION, ";
						sqlQuery += "DTTRADE, DTTIMESTAMP, ";
						sqlQuery += "SOURCE, DTSYS, ";
                        sqlQuery += "TRADEXML, ";
						sqlQuery += "EXTLLINK, ROWATTRIBUT" + Cst.CrLf;
						sqlQuery += ")" + Cst.CrLf; 
						sqlQuery += @"
select 
i2.IDI, i2.IDENTIFIER" + concat + @"'(From Template '" + concat + @"t.identifier" + concat + @"')'," +
							"i2.DISPLAYNAME" + concat + @"'(From Template '" + concat + @"t.identifier" + concat + @"')',"+
							@"null as DESCRIPTION, 
" + DataHelper.SQLGetDate(CS) + @" As DTTRADE, " + DataHelper.SQLGetDate(CS) + @" as DTTIMESTAMP, 
t.SOURCE," + DataHelper.SQLGetDate(CS) + @" as DTSYS,
t.TRADEXML, 
null as EXTLLINK, 'X' as ROWATTRIBUT 
from dbo.TRADE t
inner join dbo.TRADESTSYS tsys on tsys.IDT=t.IDT 
inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
inner join dbo.PRODUCT p on p.IDP=i.IDP and p.IDENTIFIER=i.IDENTIFIER
inner join dbo.INSTRUMENT i2 on i2.IDP=p.IDP and i2.IDI not in 
(
	select IDI 
	from dbo.TRADE 
	inner join dbo.TRADESTSYS on TRADESTSYS.IDT=TRADE.IDT 	
	where IDSTENVIRONMENT='TEMPLATE'
)
where tsys.IDSTENVIRONMENT='TEMPLATE'
and t.IDENTIFIER = p.IDENTIFIER
";
						nbRow = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery);
 
						if (nbRow > 0)
						{
							sqlQuery  = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRADETRAIL + "(IDT, IDA, DTSYS, SCREENNAME,"; 
							sqlQuery += "ACTION, HOSTNAME, APPNAME, APPVERSION, APPBROWSER, IDTRADE_P";
							sqlQuery += ")" + Cst.CrLf;						    
							sqlQuery += SQLCst.SELECT;
							sqlQuery += "t.IDT";
							sqlQuery += ", " + SessionTools.Collaborator_IDA.ToString();
							sqlQuery += ", " + DataHelper.SQLGetDate(SessionTools.CS);
							sqlQuery += ", " + DataHelper.SQLString("Full");
							sqlQuery += ", " + DataHelper.SQLString("Create");
							sqlQuery += ", " + DataHelper.SQLString(SessionTools.HostName);
							sqlQuery += ", " + DataHelper.SQLString(Software.Name);
							sqlQuery += ", " + DataHelper.SQLString(Software.VersionBuild);
							sqlQuery += ", " + DataHelper.SQLString(SessionTools.BrowserInfo);
							//Le dernier enregistrement dans TRADETRAIL correspond au trade present dans TRADE donc pas de IDTRADE_P car pas de ligne correspondante dans TRADE_P
							sqlQuery += ", " + SQLCst.NULL + Cst.CrLf;
							sqlQuery += @"from dbo.TRADE t 
inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
left outer join dbo.TRADESTSYS tst on tst.IDT=t.IDT 
where (t.ROWATTRIBUT ='X') and (tst.IDT is null)
";
							DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery);
						}

						nbRow = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery);
 
						if (nbRow > 0)
						{
							sqlQuery  = "insert into dbo.TRADESTSYS" + Cst.CrLf;
							sqlQuery += "(" + Cst.CrLf;
							sqlQuery += "IDT, DTEXECUTION, ";
							sqlQuery += "IDSTENVIRONMENT, DTSTENVIRONMENT, IDASTENVIRONMENT, ";
							sqlQuery += "IDSTACTIVATION, DTSTACTIVATION, IDASTACTIVATION, ";
							sqlQuery += "IDSTPRIORITY, DTSTPRIORITY, IDASTPRIORITY ";
							sqlQuery += ")" + Cst.CrLf; 
							sqlQuery += @"
select
t.IDT, null as DTEXECUTION,
'TEMPLATE' as IDSTENVIRONMET, t.DTSYS as DTSTENVIRONMENT, 1 as IDASTENVIRONMENT, 
'REGULAR' as IDSTACTIVATION, t.DTSYS as DTSTACTIVATION, 1 as IDASTACTIVATION, 
'REGULAR' as IDSTPRIORITY, t.DTSYS as DTSTPRIORITY, 1 as IDASTPRIORITY ";
							
                            sqlQuery += @"from dbo.TRADE t 
inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
left outer join dbo.TRADESTSYS tst on tst.IDT=t.IDT 
where (t.ROWATTRIBUT ='X') and (tst.IDT is null)
";
							DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery);
						}
					}
					catch(Exception e){System.Diagnostics.Debug.WriteLine("Insert Template: " + e.Message); }
				}
				
				//S'il s'agit d'une "Création" et qu'on ne ferme pas la fenêtre (Bouton "Apply"), on reload la page pour aafficher les menu childs
				if (!pIsSelfClose && referential.isNewRecord)
				{
					if (StrFunc.IsFilled(keyField) && StrFunc.IsFilled(keyFieldValue))
					{
						int pos1, pos2;
						string url  = this.Request.RawUrl;
						url  = url.Replace("&N=1","&N=0");                
						url  = url.Replace("&DPC=1", string.Empty);

						pos1 = url.IndexOf("&PK=");
						pos2 = url.IndexOf("&", pos1 + 4);
						url  = url.Remove(pos1, pos2 - pos1);
						pos1 = url.IndexOf("&PKV=");
						pos2 = url.IndexOf("&", pos1 + 5);
						if (pos2 >= 0)
							url  = url.Remove(pos1, pos2 - pos1);
						else
							url  = url.Substring(0, pos1);
                        
						url += "&PK=" + keyField;  
						url += "&PKV=" + keyFieldValue;
						url += "&OCR=1"; //OCR:OpenerCallReload
						Server.Transfer(url);
					}
					else
						pIsSelfClose = true;
				}

				//si selfclose ou si le referentiel a pour datakeyfield ROWVERSION : on ferme la fenetre
				if (pIsSelfClose || referential.IsROWVERSIONDataKeyField)
					//20071029 FI 15915 Appel à Refresh à la place de Reload 
					//JavaScript.OpenerCallReloadAndClose(this);    
					JavaScript.OpenerCallRefreshAndClose(this);    
				else
					//20071029 FI 15915 Appel à Refresh à la place de Reload 
					//JavaScript.OpenerCallReload(this);    
					JavaScript.OpenerCallRefresh(this);    
			}
		}
		#endregion
		#region private ReturnAndClose
		private void ReturnAndClose()
		{
			string dataKeyField = null;
			string keyField		= null;
			string description  = null;

			if (referential.ExistsColumnDataKeyField)
				dataKeyField = referential.dataRow[referential.IndexColSQL_DataKeyField].ToString();
			if (referential.ExistsColumnKeyField)
				keyField = referential.dataRow[referential.IndexColSQL_KeyField].ToString();
			if (referential.ExistsColumnDISPLAYNAME)
				description	= referential.dataRow[referential.IndexColSQL_DISPLAYNAME].ToString();
			else if (referential.ExistsColumnDESCRIPTION)
				description	= referential.dataRow[referential.IndexColSQL_DESCRIPTION].ToString();

			string display = string.Empty;
			if (dataKeyField != null)
				display += "  dataKeyField: " + dataKeyField;
			if (keyField != null)
				display += "  keyField: " + keyField;
			if (description != null)
				display += "  description: " + description;
			display = display.TrimStart();

			JavaScript.AlertImmediate(this, display, true);
		}        
     
		#endregion
		#region private GetSQLColumn
		private string GetSQLColumn(string pColumnName)
		{
			if (0 == pColumnName.IndexOf("."))
				return SQLCst.TBLMAIN + "." + pColumnName;
			else
				return pColumnName;
		}
		#endregion
		#region private GetWidthControlLinked
		private string GetWidthControlLinked(string pInputWidth)
		{
			string inputWidth = string.Empty;
			if ( StrFunc.IsFilled(pInputWidth) )
				inputWidth = @" Width=""" + pInputWidth + @"""";
			else if (referential.NbControlLinked > 0)
				inputWidth = ((int) (90 / referential.NbControlLinked)).ToString() + "%";
			//
			return inputWidth;
		}
		#endregion
	}
}