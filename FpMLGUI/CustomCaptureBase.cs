#region Using Directives
using System;
using System.Collections;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls ;
using System.Reflection; 

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web; 
using EFS.EFSTools;  

using FpML.Enum;
#endregion 

namespace EFS.GUI
{
	#region Interface ICustomCaptureInfos
	public interface ICustomCaptureInfos
	{
		CustomCaptureInfosBase CcisBase {get;}
	}
	#endregion
	
	#region Interface IContainerCciFactory
	/// <summary>
	/// Interface to implemented for class Trade[product] (ie: TradeFra)
	/// Warning: This methods are "virtual method" of the class "TradeBase"
	/// </summary>
	public interface IContainerCciFactory 
	{
		#region Initialize_FromCci
		/// <summary>
		/// Initialisalize class from ccis  screenObject ( Ex 3 OtherPartyPaments if occurs="3")
		/// </summary>
		void Initialize_FromCci();
		#endregion Initialize_FromCci
		#region AddCciSystem
		/// <summary>
		/// Adding missing controls that are necessary for process intilialize
		/// (ie: Buyer et Seller du fra)
		/// </summary>
		void AddCciSystem();
		#endregion AddCciSystem
		#region Initialize_FromDocument
		/// <summary>
		/// Initialisation des CCI à partir des données  présentes dans les classes du Document XML
		/// </summary>
		void Initialize_FromDocument();
		#endregion Initialize_FromDocument
		#region Dump_ToDocument
		/// <summary>
		/// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
		/// </summary>
		void Dump_ToDocument();
		#endregion Dump_ToDocument
		#region ProcessInitialize
		/// <summary>
		/// Initialization others data following modification
		/// </summary>
		/// <param name="pProcessQueue"></param>
		/// <param name="pCci"></param>
		void ProcessInitialize(CustomCaptureInfo pCci);
		#endregion ProcessInitialize
		#region IsClientId_PayerOrReceiver
		/// <summary>
		/// 
		/// </summary>
		bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci);
		#endregion IsClientId_PayerOrReceiver
		#region CleanUp
		/// <summary>
		/// CleanUp => Procedure appelée afin de nettoyer le document Fpml avant validation ou avant chgt d'écran 
		/// Ex OtherPartyPayment 
		/// s'il existe 3 OtherPartyPayment sur le screen objects => IL y aura 3 instances de Trade.otherpartyPayment
		/// Les instances non alimentées seront supprimées pour conformité  vis à vis de l'xsd fpml
		/// </summary>
		void CleanUp();
		#endregion CleanUp
		#region RemoveLastItemInArray
		void RemoveLastItemInArray(string pPrefix);
		#endregion RemoveLastItemInArray
	}    
	#endregion interface IContainerCciFactory
	
	#region Interface IContainerCci
	public interface IContainerCci
	{
		#region CciClientId
		/// <summary>
		/// retourne le nom complet du cci (sans le prefix du control)  s'il appartient à la classe trade en cours ( Ex TradeFra ou TradeFxLeg......)
		/// Ex Fra => TradeFra.CciClientId( "toto") => Retourne le cci tel que ClientId_WithoutPrefix = fra_toto  
		/// </summary>
		string CciClientId(string pClientId_Key);
		#endregion
		#region Cci
		/// <summary>
		/// retourne le cci s'il appartient à la classe trade en cours
		/// Ex Fra => TradeFra.CciTradeIdentifier("Toto") => Retourne le ccis[fra_toto] 
		/// </summary>
		CustomCaptureInfo Cci(string pClientId_Key);
		#endregion
		#region IsCciOfContainer
		/// <summary>
		/// retourne si le cci accessible  depuis l'instance trade en cours
		/// parametre = nom complet (sans le prefix control)
		/// </summary>
		bool IsCciOfContainer(string pClientId_WithoutPrefix);
		#endregion
		#region CciContainerKey
		/// <summary>
		/// retourne l'élement à partir du nom complet (sans le prefix control)
		/// Ex Fra => TradeFra.CciContainerKey("fra_Toto") => Retourne "Toto"
		/// </summary>
		string CciContainerKey(string pClientId_WithoutPrefix);
		#endregion
	}
	#endregion interface IContainerCci

	#region class CustomCaptureInfosBase
	/// <summary>
	/// CustomCaptureInfos: Contient une collection de classe CustomCaptureInfo.
	/// </summary>
	public abstract class CustomCaptureInfosBase : CollectionBase 
	{
		#region public Enum
		public enum ProcessQueueEnum{High, Low, None}
		#endregion 
		
		#region Members
		protected  ICustomCaptureInfos     obj;			   // Obj With Cci
		protected  IContainerCciFactory    cciContainer;   // cciContainer for dumpTo_document
		//
		//stokage Session en cours (for restriction au données)
		public     string                  sessionId;      // Ne doit être valorisé dans ds un contexte Web (et pas ds des services par Ex)
		public     bool                    isSessionAdmin;
		//
		private    Queue myHigh;
		private    Queue myLow;
		#endregion Members
			
		#region Accessor
		public bool cciContainerSpecified
		{
			get{return (null != cciContainer);}
		}
		public virtual bool isPreserveData
		{
			get{return false;}
		}
		public virtual bool isUseProcessInitialize
		{
			get{return true;}
		}
		#endregion Accessor

		#region Constructor
		public CustomCaptureInfosBase(ICustomCaptureInfos pobj):this(pobj,null,true)
		{}
		public CustomCaptureInfosBase(ICustomCaptureInfos pobj,string pSessionId,bool pIsSessionAdmin)
		{
			
			sessionId       = pSessionId;
			isSessionAdmin  = pIsSessionAdmin;
			//
			myHigh = new Queue();
			myLow  = new Queue();
			//
			obj = pobj;
			InitializeCciContainer();
			//
		}
		#endregion Constructor
		
		#region public Indexeurs
		public CustomCaptureInfo this[int pIndex] 
		{
			get
			{
				return (CustomCaptureInfo)this.List[pIndex];
			}
		}        
		public CustomCaptureInfo this[string pClientId_WithoutPrefix] 
		{
			get
			{
				CustomCaptureInfo retCci = null;
				foreach (CustomCaptureInfo cci in this)
				{
					if (cci.ClientId_WithoutPrefix == pClientId_WithoutPrefix)
					{
						retCci = cci;
						break;
					}
				}
				return retCci;
			}
		}        
		#endregion Indexeurs
		
		#region public add
		public  void   Add(CustomCaptureInfo pCci) 
		{
			if (!Contains(pCci.ClientId_WithoutPrefix))  
				List.Add(pCci);
		}
		#endregion
		#region public Remove
		public  void   Remove(string pClientId_WithoutPrefix) 
		{
			if (Contains(pClientId_WithoutPrefix))  
			{
				for(int i= this.Count - 1;-1 < i; i--)
				{
					CustomCaptureInfo cci = this[i];
					if (pClientId_WithoutPrefix == cci.ClientId_WithoutPrefix)
					{
						this.RemoveAt(i); 
						break;
					}
				}
			}
		}
		#endregion


		#region public Contains
		public  bool   Contains (string pClientId_WithoutPrefix)
		{
			return (null != this[pClientId_WithoutPrefix]);
		}
		#endregion
		#region public Synchronize
		public  void   Synchronize(string pClientId_WithoutPrefix, string pLastValue, string pNewValue)
		{
			Synchronize(pClientId_WithoutPrefix,pLastValue,pNewValue,false);
		}
		public  void   Synchronize(string pClientId_WithoutPrefix, string pLastValue, string pNewValue, bool pIsAlways)
		{
			if (Contains(pClientId_WithoutPrefix))
			{
				CustomCaptureInfo cci = this[pClientId_WithoutPrefix];
				if ( (cci.newValue == pLastValue) || (StrFunc.IsEmpty(cci.newValue) && StrFunc.IsEmpty(pLastValue)))
				{
					if ((cci.isMandatory) || pIsAlways)
						cci.newValue = pNewValue;
					else if (StrFunc.IsFilled(cci.newValue))
						cci.newValue = pNewValue;
				}
			}
		}
		#endregion public Synchronize
		#region public Finalize
		public  void   Finalize(string pClientId_WithoutPrefix, ProcessQueueEnum pProcessQueue)
		{
			if (Contains(pClientId_WithoutPrefix))
			{
				CustomCaptureInfo cci = this[pClientId_WithoutPrefix];
				switch (pProcessQueue)
				{
					case ProcessQueueEnum.High:
					case ProcessQueueEnum.Low:
						Enqueue(cci,pProcessQueue);
						break;
					default:
						cci.Finalize(! isPreserveData ); 
						break;
				}
			}
		} 
		#endregion public Finalize
		#region public SetQuickClientId
		public  void   SetQuickClientId (string pClientId_WithoutPrefix, string pQuickClientId)
		{
			if (this.Contains(pClientId_WithoutPrefix))
			{
				this[pClientId_WithoutPrefix].quickClientId = pQuickClientId; 
				this[pClientId_WithoutPrefix].quickDataPosition = GetMaxQuickDataPosition( pQuickClientId) + 1;
			}
		}
		#endregion public SetQuickClientId
		#region public GetNewValue
		public  string GetNewValue(string pClientId_WithoutPrefix)
		{
			string ret = string.Empty ;
			if (this.Contains(pClientId_WithoutPrefix))
			{
				ret = this[pClientId_WithoutPrefix].newValue; 
			}
			return  ret; 
		}
		#endregion public GetNewValue
		#region public Set
		public  void   Set(string pClientId, string pField, object pValue)
		{
			CustomCaptureInfo cci = null ;
			//		
			if(this.Contains(pClientId))
				cci = this[pClientId];
			//				
			if (null != cci)		
			{
				FieldInfo fld=  cci.GetType().GetField(pField); 
				if (null != fld)
				{
					fld.SetValue(cci,pValue); 
				}
			}
		}
		#endregion public Set
		#region public SetNewValue
		public  void   SetNewValue(string pClientId, string pNewValue )
		{
			SetNewValue(pClientId,pNewValue,false);
		}
		//
		public  void   SetNewValue(string pClientId, string pNewValue, bool pbOnlyIfIsEmpty )
		{
			//pbOnlyIfIsEmpty => permet d'initialiser la new Value lorsqu'elle est vide
			CustomCaptureInfo cci = null ;
			//		
			if(this.Contains(pClientId))
				cci = this[pClientId];
			//				
			if (null != cci)		
			{
				if (pbOnlyIfIsEmpty && (StrFunc.IsEmpty(cci.newValue)))
					cci.newValue = pNewValue;
				else
					cci.newValue = pNewValue;
			}
		}
		#endregion SetNewValue
		#region public SetErrorMsg
		public  void   SetErrorMsg(string pClientId_WithoutPrefix, string pErrorMsg)
		{
			if (this.Contains(pClientId_WithoutPrefix))
				this[pClientId_WithoutPrefix].errorMsg = pErrorMsg  ;
		}        
		#endregion SetErrorMsg
		#region public ClearErrorMsg
		public  void   ClearErrorMsg(string pClientId_WithoutPrefix)
		{
			SetErrorMsg(pClientId_WithoutPrefix, string.Empty);
		}        
		#endregion public ClearErrorMsg
		#region public Reset
		public  void  Reset() 
		{
			base.Clear();
			myHigh.Clear();  
			myLow.Clear();
		}
		#endregion public  Reset
		#region  public RemoveCciOf
		public  void   RemoveCciOf(IContainerCci pTradeCci)
		{
			for(int i= this.Count - 1;-1 < i; i--)
			{
				CustomCaptureInfo cci = this[i];
				if (pTradeCci.IsCciOfContainer(cci.ClientId_WithoutPrefix))
					this.RemoveAt(i); 
			}
		}
		#endregion RemoveCciOf
		#region private Enqueue
		private void   Enqueue (CustomCaptureInfo pCci , ProcessQueueEnum  processQueue)
		{
			if (isUseProcessInitialize)
			{
				switch (processQueue)
				{
					case ProcessQueueEnum.High:
						myHigh.Enqueue(pCci);
						break;
					case ProcessQueueEnum.Low:
						myLow.Enqueue(pCci);
						break;
				}
			}
		}
		#endregion private Enqueue
		#region public CloneGlobalCciInTrade
		public void CloneGlobalCciInTrade (string key , CustomCaptureInfo pCciSource, IContainerCci pTrade)
		{
			//Ex  S'il existe Irs_calculationPeriodDates_effectiveDate => 
			//    et que pTrade correspond à cciStream[1] alors un cci Irs1_calculationPeriodDates_effectiveDate sera généré
			//			
			if (!Contains(pTrade.CciClientId(key)))  
			{
				CustomCaptureInfo cci = (CustomCaptureInfo) pCciSource.Clone(CustomCaptureInfo.CloneMode.CciAttribut); 
				cci.clientId = pCciSource.ClientId_Prefix +  pTrade.CciClientId(key);
				this.Add(cci);  
			}
		}
		#endregion CloneGlobalCciInTrade
		
		#region public virtual Initialize
		public virtual void  InitializeCciContainer()
		{
			//Ex   cciContainer  = new TradeFra(pobj);  
			cciContainer    = null;
			
		}
		#endregion public virtual Initialize
		#region public virtual InitializeCci
		public virtual void InitializeCci(CustomCaptureInfo pCci,  SQL_Table pSql_Table, string pData)
		{
			try
			{
				pCci.Initialize(pSql_Table, pData);
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex) {throw new OTCmlException("CustomCaptureInfosBase.InitializeCci",ex);}
		}
        
		#endregion public virtual InitializeCci

		#region public SaveCapture
		/// <summary>
		///  Initialize from GUI And Dump To Document With clean UP
		/// </summary>
		/// <param name="pPage"></param>
		public void SaveCapture(Page pPage)
		{
			//Mise a jour Document Fpml
			UpdCapture(pPage); 
			//Nettoyage du Document Fpml => Ex supp des OtherPartyPayment non alimenté
			if (cciContainerSpecified)
				cciContainer.CleanUp(); 
		}
		#endregion 
		#region public UpdCapture
		/// <summary>
		/// Initialize from GUI And Dump To Document
		/// </summary>
		/// <param name="pPage"></param>
		public void UpdCapture(Page pPage)
		{
			try
			{
				//Mise a jour From GUI
				Initialize_FromGUI(pPage);
				//Mise a jour From Document Fmpl
				Dump_ToDocument(0);
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex) {throw new OTCmlException("CustomCaptureInfosBase.UpdCapture",ex);}	
		}
		#endregion UpdCapture
		#region public UpdCaptureAndDisplay
		/// <summary>
		/// Initialize from GUI, Dump To Document, Display  In GUI 
		/// </summary>
		/// <param name="pPage"></param>
		public void UpdCaptureAndDisplay(PageBase pPage)
		{
			try
			{
				UpdCapture(pPage);
				Dump_ToGUI(pPage);
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex) {throw new OTCmlException("CustomCaptureInfosBase.LoadCapture",ex);}	
		}
		#endregion UpdCaptureAndDisplay
		#region public LoadCapture
		/// <summary>
		/// Initialize from Document, Display  In GUI 
		/// </summary>
		/// <param name="pPage"></param>
		public void LoadCapture(PageBase pPage)
		{
			try
			{
				Initialize_FromDocument();
				Dump_ToDocument(0);//20050314 PL pour Initialisation des mots clefs (Ex EntityOfUSer)
				Dump_ToGUI(pPage);
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex) {throw new OTCmlException("CustomCaptureInfosBase.LoadCapture",ex);}	
		}
		#endregion LoadCapture
		#region public CleanUp
		/// <summary>
		/// Remove empty item, set specified flag and set default party before Validation
		/// </summary>
		public void CleanUp()
		{
			if (cciContainerSpecified)
				cciContainer.CleanUp(); 
		}
		#endregion
		
		#region public RemoveLastItemInArray
		/// <summary>
		/// Remove empty item, set specified flag and set default party before Validation
		/// </summary>
		public void RemoveLastItemInArray(string pPrefix)
		{
			if (cciContainerSpecified)
				cciContainer.RemoveLastItemInArray(pPrefix); 
		}
		#endregion

		#region private Initialize_FromDocument
		/// <summary>
		/// 1/ Purge des classes du Document XML
		/// 2/ Initialisation des CCI à partir des données présentes dans les classes du Document XML
		/// </summary>
		/// <param name="pPage"></param>
		private void Initialize_FromDocument()
		{
			try
			{
				//
				if (cciContainerSpecified)
				{
					cciContainer.Initialize_FromCci();  // attention  Initialize_FromCci être appelée avant AddCciSystem
					cciContainer.AddCciSystem();       
					cciContainer.Initialize_FromDocument();
				}
				//
				InitializeQuickInput (); 
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex) {throw new OTCmlException("CustomCaptureInfosBase.Initialize_FromDocument",ex);}
		}        
		#endregion Initialize_FromDocument
		#region private Initialize_FromGUI
		/// <summary>
		/// Initialisation des CCI à partir des données présentes dans les contrôles IHM
		/// </summary>
		/// <param name="pPage"></param>
		private void Initialize_FromGUI(Page pPage)
		{
			try
			{
				string eventTarget = string.Empty + pPage.Request.Params["__EVENTTARGET"];
				string clientId;
				string data;
				Control control;
            
				//Note: isProcessAllObject=true si la page est postée par autre chose qu'un contrôle de saisie (ie: menu consult, F5, ...)
				bool isProcessAllObject = !eventTarget.StartsWith(Cst.DDL) && !eventTarget.StartsWith(Cst.HSL) &&
					!eventTarget.StartsWith(Cst.CHK) && !eventTarget.StartsWith(Cst.HCK) &&
					!eventTarget.StartsWith(Cst.TXT) && !eventTarget.StartsWith(Cst.QKI) &&
					!eventTarget.StartsWith(Cst.BUT);

				foreach (CustomCaptureInfo cci in this)
				{
					clientId   = cci.clientId;
					//Note: On traite ici les contrôles non AutoPostback et le contrôle qui a généré le Postback
					//if (isProcessAllObject || (!cci.isAutoPostback) || (clientId == eventTarget))
					//20060503 Mise en commentaire
					//On met à jour tous mes contrôles en permanence, aucune incidence particulière car les calculs ne sont générés que lorsque Last!= New
					if(true)
					{
						data       = string.Empty;
						control = (Control)pPage.FindControl(clientId);
						if ( null != control )
						{
							switch (cci.ClientId_Prefix)
							{
								case Cst.TXT:
								case Cst.QKI:
									data = ((TextBox)control).Text.Trim();
									break;
								case Cst.DDL:
									data = ((DropDownList)control).SelectedValue;
									break;
								case Cst.HSL:
									data = ((HtmlSelect)control).Value;
									break;
								case Cst.CHK:
								case Cst.HCK:
									PropertyInfo pty = control.GetType().GetProperty("Checked");; 
									data = (bool) pty.GetValue(control,null)? Cst.FpML_Boolean_True: Cst.FpML_Boolean_False;
									break;
							}
							//
							try
							{
								//Si preserve on conserve les données saisie (Ex pour Date : Today ne doit pas être interprété)
								if (false == isPreserveData)  
									cci.NewValueFromLiteral = data; 
								else
									cci.newValue = data;
							}
							catch{cci.newValue = string.Empty;} // Eviter de planter en saisie 
							//
							if ((cci.HasChanged) && (clientId == eventTarget)) 
								//Warning: Ne pas écrire: cci.IsInputByUser = cci.HasChanged 
								cci.isInputByUser = true;
						}
					}
				}
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex) {throw new OTCmlException("CustomCaptureInfosBase.Initialize_FromGUI",ex);}
		}        
		#endregion Initialize_FromGUI
		#region private Dump_ToDocument
		/// <summary>
		/// 1/ Déversement des données issues des CCI, dans les classes du Document XML
		/// 2/ Gestion des incidences sur les autres données via 2 queues (High, Low)
		/// 3/ Appel récursif afin de gérer les éventuels nouveaux déversement suite à la gestion des incidences
		/// 4/ Purge des classes du Document XML
		/// </summary>
		private void Dump_ToDocument(int pguard)
		{
			try
			{
				bool isRecursive     = false;
				//
				SynchronizeQuickIputAndCcis();
				//
				if (cciContainerSpecified)
				{
					cciContainer.Dump_ToDocument();
					//
					#region Management of queues
            
					CustomCaptureInfo currentCci;
					bool isFound = true;
					while (isFound && (++pguard < 999))
					{
						isFound = false;
                                
						while ((myHigh.Count > 0) && (++pguard < 999))
						{
							isRecursive = true;
							currentCci = (CustomCaptureInfo)myHigh.Dequeue();
							//System.Diagnostics.Debug.WriteLine("High: " + currentCci.ClientId);
							cciContainer.ProcessInitialize (currentCci);
							//Debug();
						}
						if (myLow.Count > 0)
						{
							isRecursive = true;
							isFound = true;
							currentCci = (CustomCaptureInfo)myLow.Dequeue();
							//System.Diagnostics.Debug.WriteLine("Low: " + currentCci.ClientId);
							cciContainer.ProcessInitialize(currentCci);
							//Debug();
						}
					}
					if ((pguard >= 999))
					{
						throw(new OTCmlException ("CustomCapturesInfos.Dump_ToDocument","Infinite Loop"));
					}
					#endregion Management of queues
					if (isRecursive)
						Dump_ToDocument(pguard);
				}
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex) {throw new OTCmlException("CustomCaptureInfosBase.Dump_ToDocument",ex);}
		
		}                
		#endregion Dump_ToDocumentmethods
		#region public virtual Dump_ToGUI 
		/// <summary>
		/// 1/ Déversement des CCI sur l'IHM
		/// 2/ Mise à Disabled de certains contrôles
		/// 3/ Reload de certaines DDL
		/// </summary>
		/// <param name="pPage"></param>
		public virtual void Dump_ToGUI(PageBase pPage)
		{
			try
			{
			
				string data;
				string warningMsg = string.Empty; 
				bool isControlEnabled = true;
			
				Control control;
				//
				foreach (CustomCaptureInfo cci in this)
				{
					data             = cci.newValue;
					isControlEnabled = cci.isEnabled;
					control          = (Control)pPage.FindControl(cci.clientId);
					//
					if ( null != control )
					{
						switch (cci.ClientId_Prefix)
						{
								#region TextBox
							case Cst.TXT:
							case Cst.QKI:
								TextBox txt = (TextBox)control;
								//
								if (isPreserveData)   
									data = cci.newValue;
								else
									data = cci.NewValueFmtToCurrentCulture;
								//
								txt.Enabled = isControlEnabled;
								txt.Text = data;
								break;
								#endregion
								#region DropDownList
							case  Cst.HSL:
								ControlsTools.DDLSelectByValue((HtmlSelect)control, data);
								break;
							case Cst.DDL:
								DropDownList ddl = (DropDownList)control;
								//
								if (!(cci.isMandatory))
									ControlsTools.DDLLoad_AddListItemEmptyEmpty(ddl);	
								//
								ddl.Enabled = isControlEnabled;
								bool isFound = ControlsTools.DDLSelectByValue(ddl, data);
							
								if ((!isFound) && (StrFunc.IsFilled(data)))
								{
									if (StrFunc.IsEmpty(warningMsg))
										warningMsg = Ressource.GetString("Msg_DataUnavailableOrRemoved", "Warning: Data disabled or removed !") + Cst.CrLf;
									//
								
									Label label   = pPage.FindControl(Cst.LBL + cci.ClientId_WithoutPrefix) as Label;
									string  caption = cci. ClientId_WithoutPrefix; 
									if ( null != label)
										caption = label.Text;
									warningMsg += Cst.CrLf + caption + ": " + data;
									//
									string msg_tmp = " " + Ressource.GetString("Msg_UnavailableOrRemoved", "[disabled or removed]");
									//
									ddl.Items.Add(new ListItem(data + msg_tmp, data));
									isFound = ControlsTools.DDLSelectByValue(ddl, data);
								}
				
								break;
								#endregion
								#region CheckBox
							case Cst.CHK:
							case Cst.HCK: 
								PropertyInfo pty; 
								//
								pty  = control.GetType().GetProperty("Enabled");
								pty.SetValue(control, cci.isEnabled,null);
								//
								pty  = control.GetType().GetProperty("Checked");
								pty.SetValue(control, cci.IsFilledValue,null);
								break;
								#endregion
						}
					
						#region Display management
						//Tentative de passage à Disabled pour l'éventuel label associé
						Label lbl = (Label)pPage.FindControl(Cst.LBL + cci.ClientId_WithoutPrefix);
						if (null != lbl)
							lbl.Enabled = isControlEnabled;
						//Tentative de passage à Disabled pour l'éventuel image associé
						Image img = (Image)pPage.FindControl(Cst.IMG + cci.ClientId_WithoutPrefix);
						if (null != img)
							img.Enabled = isControlEnabled;

						control  = (Control)pPage.FindControl(Cst.DSP + cci.ClientId_WithoutPrefix);
						if ( null != control )
						{
							string msg = string.Empty;
							
							System.Drawing.Color color = System.Drawing.Color.Empty;
							if (cci.HasError)
							{
								msg   = cci.errorMsg;
								color = System.Drawing.Color.Red;
							}
							else if(StrFunc.IsFilled(cci.display))
								msg   = cci.display;
							else if (null != cci.sql_Table)
								msg = cci.sql_Table.FirstRow["DISPLAYNAME"].ToString();
							//
							((Label)control).Text      = msg;
							((Label)control).ForeColor = color;
						}                        
						//}
						#endregion Display management
					}
				}
				//
				if (StrFunc.IsFilled(warningMsg))
					JavaScript.AlertStartUpImmediate(pPage, warningMsg);
				//
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex) {throw new OTCmlException("CustomCaptureInfosBase.Dump_ToGUI",ex);}

		}        
		#endregion Dump_ToGUI

		#region private QuickInput
		private int  GetMaxQuickDataPosition(string pQuickClientId)
		{
			int ret = 0;
			for (int i=0 ; i< this.Count; i++)
			{
				if ((this[i].quickClientId == pQuickClientId) && (this[i].quickDataPosition > ret))
				{
					ret = this[i].quickDataPosition;
				}
			}
			return ret;
		}
		private void InitializeQuickInput()
		{
			// Initialisation  des Cci QuickInput à partir des Ccis 
			foreach ( CustomCaptureInfo cci  in this)
			{
				if ( cci.IsQuickInput )
				{
					string data = string.Empty;
					string quickClientId= cci.ClientId_WithoutPrefix;
					//
					for (int i= 1 ; i< this.Count ; i++) 
					{
						if(ConstainsCciOfQuickInput(quickClientId,i))	
						{							
							if (cci.IsQuickFormatOTCml) 
								data  += CciOfQuickInput(quickClientId,i).NewValueFmtToCurrentCulture + cci.QuickSeparator  ;  
							else
								data  += CciOfQuickInput(quickClientId,i).newValue + cci.QuickSeparator  ;  
						}
					}
					//
					cci.Initialize(null, data);
				}
			}
		}
		private void SetNewValueQuickInput( CustomCaptureInfo  pCci)
		{
			if (pCci.IsQuickInput)
			{
				string data = string.Empty;
				string quickClientId= pCci.ClientId_WithoutPrefix;
				//
				for (int i= 1 ; i< this.Count ; i++) 
				{
					if(ConstainsCciOfQuickInput(quickClientId,i))	
					{							
						if (pCci.IsQuickFormatOTCml) 
							data  += CciOfQuickInput(quickClientId,i).NewValueFmtToCurrentCulture + pCci.QuickSeparator ;  
						else
							data  += CciOfQuickInput(quickClientId,i).newValue + pCci.QuickSeparator  ;  
					}
				}
				pCci.newValue = data;
			}
		
		}
		private bool ConstainsCciOfQuickInput( string pQuickClientId, int pQuickPosition )
		{
			// Test Existence d'un Cci Tel que Cci.QuickClientId = pQuickClientId et  Cci.QuickDataPosition= pQuickPosition
			return (null!=CciOfQuickInput(pQuickClientId,pQuickPosition));
		}
		private CustomCaptureInfo  CciOfQuickInput ( string pQuickClientId, int pQuickPosition )
		{
			// Retounre Cci Tel que Cci.QuickClientId = pQuickClientId et  Cci.QuickDataPosition= pQuickPosition
			CustomCaptureInfo cci =null ;
			for (int i=0 ; i< this.Count; i++)
			{
				if ((this[i].quickClientId == pQuickClientId) && (this[i].quickDataPosition == pQuickPosition))
				{
					cci  = this[i] ;
					break;
				}
				
			}
			return cci;
		}
		private void SynchronizeQuickIputAndCcis()
		{
			foreach (CustomCaptureInfo cci in this)
			{
				if (cci.IsQuickInput)
				{
					if (cci.HasChanged) //=> Mise à jour des Ccis associés
					{
						string[] aNewValue = cci.newValue.Split( cci.QuickSeparator.ToCharArray()  );
						for (int i=0 ; i< aNewValue.Length ; i++)
						{
							if (this.ConstainsCciOfQuickInput(cci.ClientId_WithoutPrefix,i+1)) 
							{
								if (cci.IsQuickFormatOTCml)
									this.CciOfQuickInput(cci.ClientId_WithoutPrefix,i+1).NewValueFromLiteral = aNewValue[i];  		
								else
									this.CciOfQuickInput(cci.ClientId_WithoutPrefix,i+1).newValue = aNewValue[i];
							}
						}
					}  
					this.SetNewValueQuickInput(cci); // Alimentation de NewValue du cci Quick (avec Formatage)
					cci.Finalize(false); 
				}
			}
		}
		#endregion QuickInput

		#region Debug methods
		private void Debug_WriteLine(bool pIsAll)
		{
			System.Diagnostics.Debug.WriteLine( "--------------------------------------------------" );
			foreach (CustomCaptureInfo cci in this)
			{
				if (pIsAll || cci.HasChanged)
				{
					string msg = cci.clientId + ": ";
					msg += cci.lastValue + " / " + cci.newValue;
					//                    msg += (cci.IsAutoPostback      ? " - AutoPostback":string.Empty);
					//                    msg += (cci.HasChanged          ? " - Changed":string.Empty);
					msg += (cci.isInputByUser       ? " - InputByUser":string.Empty);
					//                    msg += (cci.IsCalculateBySystem ? " - CalculateBySystem":string.Empty);
					msg += (cci.HasError            ? " - " + cci.errorMsg:string.Empty);

					System.Diagnostics.Debug.WriteLine( msg );
				}
			}
		}        
		public void Debug()
		{
			this.Debug_WriteLine(true);
		}
		public void Debug_Change()
		{
			this.Debug_WriteLine(false);
		}
        
		public void DebugClientId()
		{
			foreach (CustomCaptureInfo cci in this)
			{
				System.Diagnostics.Debug.WriteLine(cci.clientId)  ;
			}	
		}
		
		#endregion Debug methods 
	}
	#endregion class CustomCaptureInfos

	#region class CustomCaptureInfo
	/// <summary>
	/// CustomCaptureInfo: Contient les informations d'une zone de saisie sur un écran personnalisé. 
	/// </summary>
	public class CustomCaptureInfo : ICloneable
	{
		#region public Enum
		public enum CloneMode
		{
			CciAttribut,
			CciData,
			CciAll
		}
		#endregion
		
		#region  Members
		public string    clientId;
		public bool	     isAutoPostback;
		public bool      isMandatory;
		public bool      isEnabled;
		public Cst.TypeData.TypeDataEnum  dataType;
		public EFSRegex.TypeRegex regex;
		public string    listRetrieval;
		public string    lastValue;
		public string    newValue;
		public bool      isInputByUser;
		public bool      isCalculateBySystem;
		public string    errorMsg;
		public string    display;
		public SQL_Table sql_Table;
		public SQL_Table lastSql_Table;
		public  string	 quickClientId;
		public  int		 quickDataPosition;
		private string   quickSeparator;
		private string   quickFormat;
		private string   typeRelativeTo;
		#endregion Members

		#region get property
		public bool   IsFilled
		{
			get{return StrFunc.IsFilled( newValue );}
		}
		
		public bool   IsEmpty
		{
			get{return StrFunc.IsEmpty( newValue );}
		}
		
		public bool   IsLastFilled
		{
			get{return StrFunc.IsFilled( lastValue  );}
		}
		
		public bool   IsLastEmpty
		{
			get{return StrFunc.IsEmpty( lastValue );}
		}
		
		
		public bool   IsFilledValue
		{
			get
			{
				bool ret = false;
				try 
				{
					
					if (IsTypeDecimal)
						ret = StrFunc.IsDecimalInvariantFilled(newValue);
					else if (IsTypeDate)
						ret = DtFunc.IsDateTimeFilled(DtFunc.StringToDateTime(newValue,DtFunc.FmtFpMLDate));
					else if (IsTypeBool) 
						ret = BoolFunc.IsTrue(newValue);  
					else
						ret = IsFilled;
				}
				catch{ret = false;}
				return ret;
			}
		}
		
		public bool   IsEmptyValue
		{
			get
			{
				return !IsFilledValue;
			}
		}


		public bool   HasChanged
		{
			get
			{
				bool ret=false;
				if (IsTypeDecimal)
				{
					ret = (DecValue(false)!= DecValue(true)); 
				}
				else
				{
					if (StrFunc.IsEmpty(newValue))
						ret = StrFunc.IsFilled(lastValue);
					else
						ret = (lastValue != newValue);
				}
				return ret;
			}
		}
		public bool   HasError
		{
			get{return (errorMsg.Length > 0);}
		}
		public bool   IsTypeString
		{
			get{return (Cst.TypeData.IsTypeString(dataType));}
		}
		public bool   IsTypeBool
		{
			get{return (Cst.TypeData.IsTypeBool(dataType));}
		}
		
		public bool   IsTypeDecimal
		{
			get{return (Cst.TypeData.IsTypeDec(dataType));}
		}
		public bool   IsTypeDate
		{
			get{return (Cst.TypeData.IsTypeDate(dataType));}
		}
		public bool   IsTypeTime
		{
			get{return (Cst.TypeData.IsTypeTime(dataType));}
		}
		public bool   IsRegexFixedRate
		{
			get
			{ 
				return  (EFSRegex.TypeRegex.RegexFixedRate        == regex)  || 
					    (EFSRegex.TypeRegex.RegexFixedRateExtend  == regex   || 
					    (EFSRegex.TypeRegex.RegexFixedRatePercent == regex));
			}		
		}
		public bool   IsRegexPercent
		{
			get
			{ 
				return  ( (EFSRegex.TypeRegex.RegexPercent        == regex)    || 
						  (EFSRegex.TypeRegex.RegexPercentExtend  == regex)); 
			}		
		}
		public bool   IsRegexRate 
		{
			// Cci FixedRate or FloatingRate
			get{return (EFSRegex.TypeRegex.RegexRate == regex);}
		}
		public bool   IsRegexFxRate
		{
			get
			{ 
				return (EFSRegex.TypeRegex.RegexFxRate == regex) || (EFSRegex.TypeRegex.RegexFxRateExtend  == regex);
			}		
	
		}
		public bool   IsRegexAmount
		{
			get	
			{
				return 	(EFSRegex.TypeRegex.RegexAmount == regex ) ||(EFSRegex.TypeRegex.RegexAmountExtend == regex )	;
			}
		}
		public bool   IsFixedRate
		{
			get 
			{
				bool  bRet = (IsRegexFixedRate) ;
				if (!bRet )
					bRet = (IsRegexRate && RateTools.IsFixedRate(newValue));
				return bRet;
			} 
		}
		public string ClientId_WithoutPrefix
		{
			get
			{
				int lenPrefix = ClientId_Prefix.Length;
				return clientId.Substring(lenPrefix);
			}
		}
		public string ClientId_Prefix
		{
			get
			{
				if (3 <= clientId.Length)  
					return clientId.Substring(0,3);
				else
					return string.Empty;
			}
		}
		public bool   IsQuickInput
		{
			get{return (Cst.QKI == this.ClientId_Prefix);}
		}
		public string QuickSeparator
		{
			set 
			{
				quickSeparator=string.Empty;
				if (this.IsQuickInput)
					quickSeparator=value;
			}
			
			get {return quickSeparator;}				
		}
		public string QuickFormat
		{
			set 
			{
				quickFormat=string.Empty;
				if (this.IsQuickInput)
				{
					quickFormat ="OTCml"; //default
					if(StrFunc.IsFilled(value))  
						quickFormat=value;
				}
			}
			
			get {return quickFormat;}				
		}
		public bool   IsQuickFormatOTCml
		{
			get {return (quickFormat=="OTCml");}				
		}
		
		public bool   IsRelaTiveTo
		{
			get
			{
				return StrFunc.IsFilled(this.typeRelativeTo);  
			}
		}
		public string TypeRelaTiveTo
		{
			get
			{
				return typeRelativeTo;  
			}

		}
		
		public string NewValueFmtToCurrentCulture
		{
			// Format NewValue With CurrentCulture
			get
			{
				string data= newValue; 
				#region isDecimal
				if (IsFixedRate || IsRegexPercent)
				{
					FixedRate fixedRate = new FixedRate(data, CultureInfo.InvariantCulture );
					data = fixedRate.ToString();
					fixedRate = null;
				}
				else if (IsRegexFxRate)
				{
					EFSTools.FxRate fxRate = new FxRate(data,CultureInfo.InvariantCulture);
					data = fxRate.ToString();
					fxRate = null;
				}
				else if (IsTypeDecimal)
					data =  StrFunc.FmtDecimalToCurrentCulture(data);
				#endregion 
				#region isDate
				if (IsTypeDate )
					data = DtFunc.GetDateString(data);
				#endregion 
				#region istime
				if (IsTypeTime )
					data = DtFunc.GetLongTimeString(data); 
				#endregion 
				return data;
			}
		}
		#endregion get property
        
		#region Set property
		public string NewValueFromLiteral
		{
			//Set New Value With FmplFormat (Invariant Culture)
			set 
			{
				try
				{
					string data = value;
					//
					if (StrFunc.IsFilled(data)) 
					{
					
						#region isDecimal
						if ( (this.IsRegexFixedRate || this.IsDataValidForFixedRate(data)) 
							||
							(this.IsRegexPercent)
							)
						{
							if (!data.EndsWith("%"))
								data += " %";
							FixedRate fixedRate = new FixedRate(data);
							data = StrFunc.FmtDecimalToInvariantCulture(fixedRate.Value);
						}
						else if (this.IsRegexFxRate)
						{
							EFSTools.FxRate fxRate = new EFSTools.FxRate(data);
							data = StrFunc.FmtDecimalToInvariantCulture(fxRate.Value);
						}
						else if (this.IsRegexAmount) 
							data =  StrFunc.FmtDecimalAmountToInvariantCulture(data);
					
						else if (this.IsTypeDecimal )
							data =  StrFunc.FmtDecimalToInvariantCulture(data);
						#endregion 
						#region isDate
						if (IsTypeDate) 
							data = DtFunc.GetDateTimeString(data,DtFunc.FmtFpMLDate);
						#endregion 
						#region isType
						if (IsTypeTime) 
							data = DtFunc.GetDateTimeString(data,DtFunc.FmtFpMLTime);
						#endregion 
						#region isbool 
						if (IsTypeBool) 
							try 
							{ 
								bool isOk= bool.Parse(data);
								data = isOk? Cst.FpML_Boolean_True:Cst.FpML_Boolean_False;
							}	
							catch {data="false";}
						#endregion 
					}
					newValue = data;
				}
				catch(OTCmlException ex) {throw ex;}
				catch(Exception ex) {throw new OTCmlException("CustomCaptureInfo.NewValueFromLiteral",ex);}
			}
		}
		#endregion Set property

		#region constructors
		public CustomCaptureInfo(string pClientId, bool pIsMandatory, Cst.TypeData.TypeDataEnum  pDataType, bool pIsAutoPostback)
		{
			clientId            = pClientId;
			isMandatory         = pIsMandatory;
			isEnabled           = true;
			dataType            = pDataType;
			isAutoPostback      = pIsAutoPostback;
			errorMsg            = string.Empty;

			regex               = EFSRegex.TypeRegex.None;
			listRetrieval       = string.Empty;
			quickClientId	    = string.Empty;
			quickDataPosition   = 0;
			
			QuickSeparator      = ";";	
			QuickFormat	        = string.Empty; 
			typeRelativeTo      = string.Empty;
		}
		public CustomCaptureInfo(string pClientId, bool pIsMandatory, Cst.TypeData.TypeDataEnum pDataType): 
			this(pClientId, pIsMandatory, pDataType, false){}
		public CustomCaptureInfo(string pClientId, bool pIsMandatory, Cst.TypeData.TypeDataEnum pDataType, bool pIsAutoPostback, string pListRetrieval, string pRelativeTo):
			this(pClientId, pIsMandatory, pDataType, pIsAutoPostback)
		{
			listRetrieval   = pListRetrieval;
			typeRelativeTo  = pRelativeTo;
		}
		public CustomCaptureInfo(string pClientId, bool pIsMandatory, Cst.TypeData.TypeDataEnum pDataType, bool pIsAutoPostback, EFSRegex.TypeRegex pRegEx):
			this(pClientId, pIsMandatory, pDataType, pIsAutoPostback)
		{
			regex  = pRegEx;
		}
		public CustomCaptureInfo():
			this(string.Empty, false,  Cst.TypeData.TypeDataEnum.unknown, false){}
		
		#endregion constructors
        
		#region Public Methodes
        
		public bool IsDataValidForFixedRate (string pData)
		{
			bool ret =false;
			if (this.IsRegexRate) 
			{
				if (!pData.EndsWith("%"))
					pData += " %";
				ret = EFSRegex.IsMatch(pData, EFSRegex.TypeRegex.RegexFixedRatePercent);      
			}
			return ret;
		}
		
		public void Reset()
		{
			newValue      = string.Empty;
			errorMsg      = string.Empty;
			sql_Table     = null;
			lastSql_Table = null;
		}
		
		public bool   IsListRetrieval( string plistRetrieval )
		{
			return ( listRetrieval == plistRetrieval.ToLower()  );
		}
		
		public void Initialize(SQL_Table pSql_Table, string pData)
		{
			//
			newValue  = pData;
			sql_Table = pSql_Table;
			//
			//Warning: On teste seulement "null" et pas "empty"
			if ( null == lastValue )
			{
				lastValue     = newValue;
				lastSql_Table = sql_Table;
			}
		}
        
		public void Finalize ( bool pbSetMsgErrorOnMandatory ) 
		{
			//Reset LastValue 
			lastValue     = newValue;
			lastSql_Table = sql_Table;
			//
			if (pbSetMsgErrorOnMandatory)
			{
				if (isMandatory)
				{
					string errMsgIsMandatory = Ressource.GetString("ISMANDATORY");
					if (StrFunc.IsEmpty(newValue))
					{
						if (errorMsg.IndexOf(errMsgIsMandatory)<0)
							errorMsg += errMsgIsMandatory;
					}
					else
					{
						errorMsg = errorMsg.Replace(errMsgIsMandatory, string.Empty);
					}
				}
			}
		}
		#endregion Public Methodes
        
		#region Private Methodes
		private decimal DecValue(bool pbNew)
		{
			string Value= (pbNew ? newValue : lastValue);
			//
			if (IsTypeDecimal)  
			{
				if (StrFunc.IsEmpty(Value) )
					return decimal.Zero;
				else
					try{return decimal.Parse(Value, CultureInfo.InvariantCulture);}
					catch{return decimal.Zero;}
			}
			else
				return Decimal.Zero; 
		}	
		#endregion Private Methodes

		#region Membres de ICloneable
		public object Clone()
		{
			return this.Clone(CloneMode.CciAll);   
		}
		public object Clone(CloneMode plMode)
		{
			CustomCaptureInfo clone		= new CustomCaptureInfo();
			
			if ( (CloneMode.CciAttribut == plMode) ||  (CloneMode.CciAll == plMode))
			{
				clone.clientId 				= clientId;
				clone.isAutoPostback		= isAutoPostback;
				clone.isMandatory			= isMandatory;
				clone.dataType				= dataType;
				clone.regex					= regex;
				clone.listRetrieval			= listRetrieval;
				//clone.sql_Table				= (sql_Table)this.sql_Table.Clone();
				//clone.Lastsql_Table			= (sql_Table)this.Lastsql_Table.Clone();
			}
			
			if ( (CloneMode.CciData == plMode) ||  (CloneMode.CciAll == plMode))
			{
				clone.lastValue				= lastValue;
				clone.newValue 				= newValue ;
				clone.isInputByUser 		= isInputByUser ;
				clone.isCalculateBySystem 	= isCalculateBySystem ;
				clone.errorMsg 				= errorMsg ;
				clone.display 				= display ;
			}
			return clone;
		}
		#endregion
	}
	#endregion class CustomCaptureInfo

	#region Class CciCompare
	public class CciCompare : IComparable
	{
		#region Members
		public string  key;
		private string sValue;
		private bool   isInputByUser;
		private int    order;
		private Cst.TypeData.TypeDataEnum dataType;
		#endregion Members

		public bool   IsFilledValue
		{
			get
			{
				bool ret = false;
				try 
				{
					if (Cst.TypeData.IsTypeDec(dataType))
						ret = StrFunc.IsDecimalInvariantFilled(sValue);
					else if (Cst.TypeData.IsTypeDate(dataType))
						ret = DtFunc.IsDateTimeFilled(DtFunc.StringToDateTime(sValue,DtFunc.FmtFpMLDate));
					else
						ret = StrFunc.IsFilled(sValue);
				}
				catch{ret = false;}
				return ret;
			}
		}
		#region Constructor
		public CciCompare (string pkey,CustomCaptureInfo pCci, int pOrder) : this(pkey,pCci.dataType,pCci.newValue,pCci.isInputByUser, pOrder)   
		{
		}
		public CciCompare (string pkey,Cst.TypeData.TypeDataEnum pdataType,string pValue,bool pIsInputByUser, int pOrder)
		{
			key      = pkey; 
			dataType = pdataType;
			sValue   = pValue;
			isInputByUser = pIsInputByUser;
			order    = pOrder;
		}
		#endregion cosntructor

		#region Members de IComparable
		public int CompareTo(object pobj)
		{
			if(pobj is CciCompare) 
			{
				int ret = 0 ; //Like Equal
				CciCompare cci2 = (CciCompare) pobj;
				//
				if (StrFunc.IsEmpty(sValue) && StrFunc.IsFilled(cci2.sValue))
					ret = -1; // cette instance est inférieure à pObj
				if (StrFunc.IsFilled(sValue) && StrFunc.IsEmpty(cci2.sValue))
					ret = 1; // cette instance est supérieur à pObj
				//
				if ((ret == 0) && (dataType == cci2.dataType))  
				{
					if ((false==IsFilledValue) && cci2.IsFilledValue)
						ret = -1; // cette instance est inférieure à pObj
					if (IsFilledValue  && (false == cci2.IsFilledValue))
						ret = 1; // cette instance est supérieur à pObj
				}
				//
				if (ret ==0) 
					ret = isInputByUser.CompareTo(cci2.isInputByUser);   // cette instance est inférieure si IsInputByUser =false 
				//
				if (ret ==0) 
					ret = order.CompareTo(cci2.order);   // cette instance est inférieure si son order et inférieur
				return ret;
			}
			throw new ArgumentException("object is not a CciCompare");    
		}
		#endregion Members de IComparable
	}
	#endregion

	#region Class RateTools
	/// <summary>
	/// Description résumée de RateTools.
	/// </summary>
	public sealed class RateTools
	{
		public const string 
			FLOATING_RATE_TENOR_SEPERATOR = "/"	;
		
		public enum  RateTypeEnum		
		{
			RATE_FIXED,
			RATE_FLOATING,
		}
		#region constructor
		public RateTools() {}
		#endregion constructor
		#region GetTypeRate()
		static RateTypeEnum  GetTypeRate(string pRate)
		{
			if (IsFixedRate( pRate ))
				return RateTypeEnum.RATE_FIXED;
			else
				return RateTypeEnum.RATE_FLOATING;
		}
		#endregion 
		#region GetFloatingRateWithTenor() methods
		static public string GetFloatingRateWithTenor(string pFloatingRate, string pPeriodMultiplier, string pPeriod)
		{
			string ret = pFloatingRate;
			if ( StrFunc.IsFilled( pPeriodMultiplier ) && StrFunc.IsFilled( pPeriod ) )
			{
				ret += FLOATING_RATE_TENOR_SEPERATOR + pPeriodMultiplier + pPeriod;
			}
			return ret;
		}
		static public string GetFloatingRateWithTenor(string pFloatingRate, int pPeriodMultiplier, PeriodEnum pPeriod)
		{
			return GetFloatingRateWithTenor(pFloatingRate, pPeriodMultiplier.ToString(), pPeriod.ToString());
		}
		#endregion 
		#region GetFloatingRateWithoutTenor()
		/// <summary>
		/// Return a floating string with tenor if exist (ie: EUR-EURIBOR-Telerate 3m, EUR-EONIA)
		/// </summary>
		/// <param name="pFloatingRateWithTenor"></param>
		/// <returns></returns>
		static public string GetFloatingRateWithoutTenor(string pFloatingRateWithTenor)
		{
			string ret = pFloatingRateWithTenor;
			int pos = pFloatingRateWithTenor.IndexOf(FLOATING_RATE_TENOR_SEPERATOR);
			if ( pos > 0 )
				ret = pFloatingRateWithTenor.Substring( 0, pos );
			return ret;
		}
		#endregion 
        
		#region IsFloatingRateWithTenor() methods
		/// <summary>
		/// Return true if the rate is a floating rate with tenor (ie: EUR-EURIBOR-Telerate 3M)
		/// </summary>
		/// <param name="pRate"></param>
		/// <returns></returns>
		static public bool IsFloatingRateWithTenor(string pFloatingRate)
		{
			return pFloatingRate.IndexOf( FLOATING_RATE_TENOR_SEPERATOR ) > 0;
		}
		/// <summary>
		/// Return true if the rate is a floating rate with tenor (ie: EUR-EURIBOR-Telerate 3M)
		/// </summary>
		/// <param name="pFloatingRate"></param>
		/// <param name="opTenor">Return the string tenor (ie: 3M)</param>
		/// <returns></returns>
		static public bool IsFloatingRateWithTenor(string pFloatingRate, out string opTenor)
		{
			bool ret = IsFloatingRateWithTenor(pFloatingRate);
			if (ret)
				opTenor = pFloatingRate.Substring( pFloatingRate.IndexOf( FLOATING_RATE_TENOR_SEPERATOR ) +1 ).Trim();
			else
				opTenor = string.Empty;
			return ret;
		}
		static public bool IsFloatingRateWithTenor(string pFloatingRate, out int opTenor_periodMultiplier, out PeriodEnum opTenor_period)
		{
			string tenor = string.Empty;
			bool ret = IsFloatingRateWithTenor(pFloatingRate, out tenor);
			if (ret)
			{
				System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("\\d+");
				opTenor_periodMultiplier = Int32.Parse(re.Match( tenor ).Value);
				string period            = tenor.Substring( opTenor_periodMultiplier.ToString().Length ).Trim();
				opTenor_period           = FpML_EnumTools.Period_StringToEnum(period);
			}
			else
			{
				opTenor_periodMultiplier = 0;
				opTenor_period           = PeriodEnum.D;
			}
			return ret;
		}
		#endregion IsFloatingRateWithTenor()
		#region IsFloatingRate()
		/// <summary>
		/// Return true if the rate is a floating rate
		/// </summary>
		/// <param name="pRate"></param>
		/// <returns></returns>
		static public bool IsFloatingRate(string pRate)
		{
			return !IsFixedRate(pRate);
		}
		#endregion
		#region IsFixedRate()
		static public bool IsFixedRate(string pRate)
		{
			return EFSRegex.IsMatch(pRate,  EFSRegex.TypeRegex.RegexFixedRateExtend );  
		}
		
		#endregion
	}
	#endregion class RateTools
}
