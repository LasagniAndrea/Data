#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.ApplicationBlocks.Data;


using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EFS.GUI.ComplexControls;

using FpML.Enum;

#endregion Using Directives

namespace EFS.GUI
{
	#region Constructor
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// 	/// <newpara>Classe de construction du GUI pour une saisie Full en fonction de la structure d'un EfsDocument</newpara>
	///</summary>
	// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
	public class FullConstructor
	{
		#region InvokeMember Enums
		private enum InvokeMemberEnum
		{
			IsClassic,
			IsEnum,
			IsInterface,
			IsChoice,
			IsDerive,
			IsDeriveSpecified
		}
		#endregion InvokeMember Enums
		//
		#region Members
		/// <summary>Jeton utilisé pour la construction d'ID d'une balise Titre</summary>
		/// <remarks><code>ID d'une balise Titre = "B" + nbBalise.ToString()</code></remarks>
		/// 
		//private bool                m_DocumentVersionSpecified;
		private DocumentVersionEnum m_DocumentVersion;
		private int                 m_Nb;
        private int                 m_NbBalise;
		private ArrayList           m_aHistoryBalise;
		private ArrayList           m_aKeyBaliseName;
		private ArrayList           m_ListFpMLReference;
		private bool                m_IsChildrenInclude;
		private ExtendEnumsGUI      m_ExtendEnumsGUI;
		private static readonly BindingFlags m_FpMLBindingFlags   = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
		private readonly string m_CssMode;
		#endregion Members

		#region Accessors
		#region MainCssClassName
		public string MainCssClassName
		{
			get { return m_CssMode + " input"; }
		}
		#endregion MainCssClassName
		#region HistoryBalise
		public ArrayList HistoryBalise
		{
			get{return m_aHistoryBalise;}
			set {m_aHistoryBalise = value;}
		}
		#endregion HistoryBalise
		#region IsChildrenInclude
		public bool IsChildrenInclude
		{
			get{return m_IsChildrenInclude;}
			set {m_IsChildrenInclude = value;}
		}
		#endregion IsChildrenInclude
		#region ExtendEnumsGUI
		public ExtendEnumsGUI ExtendEnumsGUI
		{
			get{return m_ExtendEnumsGUI;}
			set {m_ExtendEnumsGUI = value;}
		}
		#endregion ExtendEnumsGUI
		#region KeyBaliseName
		public ArrayList KeyBaliseName
		{
			get{return m_aKeyBaliseName;}
			set {m_aKeyBaliseName = value;}
		}
		#endregion KeyBaliseName
		#region ListFpMLReference
		public ArrayList ListFpMLReference
		{
			get{return m_ListFpMLReference;}
			set {m_ListFpMLReference = value;}
		}
		#endregion ListFpMLReference
		#region NbBalise
		public int NbBalise
		{
			get{return m_NbBalise;}
			set {m_NbBalise = value;}
		}
		#endregion NbBalise
		#region DocumentVersion
		public DocumentVersionEnum DocumentVersion
		{
			get { return m_DocumentVersion; }
			set { m_DocumentVersion = value; }
		}
		#endregion DocumentVersion
		#endregion Accessors

		#region Constructors
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		public FullConstructor(string pCSSMode)
		{
			Initialize();
			m_CssMode = pCSSMode;
		}
		public FullConstructor(DocumentVersionEnum pDocumentVersion)
		{
			Initialize();
			//m_DocumentVersionSpecified	= true;
			DocumentVersion	= pDocumentVersion;
		}
		#endregion Constructors
		//	
		#region private Initialize
		private void Initialize()
		{
			NbBalise = 0;
			HistoryBalise = new ArrayList();
			KeyBaliseName = new ArrayList();
			ListFpMLReference = new ArrayList();
			//Chargement des enums et schemes
			//FI 20120124 mise en commentaire les enums sont chargés à la connexion ou en cas de modifs
			//ExtendEnumsTools.LoadFpMLEnumsAndSchemes(pSource);
			// FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum
			//ExtendEnumsGUI = new ExtendEnumsGUI(ExtendEnumsTools.ListEnumsSchemes.Items);
			ExtendEnumsGUI = new ExtendEnumsGUI(new DataEnabledEnum(SessionTools.CS).GetData());
		}
		#endregion Initialize
		#region public Display
		/// <summary>
		/// <newpara><b>Description :</b></newpara>
		/// <newpara><b>Contents :</b></newpara>
		/// <newpara>Pour chaque classe FpML, recherche et invocation de la méthode de création d'IHM 
		/// associé à l'objet et son champ courant</newpara>
		///</summary>
		/// <revision>
		///     <version>1.2.0</version><date>20071003</date><author>EG</author>
		///     <comment>
		///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
		///     </comment>
		/// </revision>
		public void Display(object pCurrent,FieldInfo pFldCurrent,ref PlaceHolder pPlaceHolder,object pParent,FieldInfo pFldParent,params bool[] pIsParam)
		{
			Display(pCurrent,pFldCurrent,ref pPlaceHolder,pParent,pFldParent,null,null,pIsParam);
		}
		///
        // EG 20180423 Analyse du code Correction [CA2200]
        public void Display(object pCurrent, FieldInfo pFldCurrent, ref PlaceHolder pPlaceHolder, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, params bool[] pIsParam)
		{
            try
            {

#if DEBUG
                if (pFldCurrent.Name == "securityIssueDate")
                    System.Diagnostics.Debug.Write(pCurrent.GetType().Name + "-" + pFldCurrent.Name + Environment.NewLine);
#endif
                bool isStartDiv = pIsParam.Length == 0 || pIsParam[0];
                bool isEndDiv = pIsParam.Length <= 1 || pIsParam[1];
                bool isStep = pIsParam.Length > 2 && pIsParam[2];
                bool isAutoClose = false;

                #region CloseDiv Adding
                if (isEndDiv && pFldCurrent.IsDefined(typeof(CloseDivGUI), true))
                {
                    object[] attributes = pFldCurrent.GetCustomAttributes(typeof(CloseDivGUI), true);
                    foreach (CloseDivGUI closeDiv in attributes)
                        pPlaceHolder.Controls.Add(MethodsGUI.MakeDiv(closeDiv));
                }
                #endregion CloseDiv Adding
                #region OpenDiv Adding
                if (isStartDiv && pFldCurrent.IsDefined(typeof(OpenDivGUI), true))
                {
                    string uniqueId = MethodsGUI.CreateUniqueID(this, pParent, pFldParent, pCurrent, pFldCurrent, 0);
                    object[] attributes = pFldCurrent.GetCustomAttributes(typeof(OpenDivGUI), true);
                    foreach (OpenDivGUI openDiv in attributes)
                    {
                        if (openDiv.IsProduct)
                            openDiv.Name = MethodsGUI.GetInstrumentName(pCurrent, pFldCurrent);
                        openDiv.HelpURL = MethodsGUI.GetHelpLink(pCurrent, pFldCurrent);
                        openDiv.UniqueId = uniqueId;
                        pPlaceHolder.Controls.Add(MethodsGUI.MakeDiv(this, openDiv, false));
                        isAutoClose = openDiv.IsAutoClose;
                    }
                }
                #endregion OpenDiv Adding
                #region CreateControl Adding
                // Control éventuel sur la classe ou l'attribut de la classe courant
                Type typeTarget = null;
                object target = null;
                MethodInfo method = null;
                //InvokeMemberEnum member = GetInvokeMember(pCurrent,pFldCurrent,ref typeTarget,ref target,ref method,isStep);
                GetInvokeMember(pCurrent, pFldCurrent, ref typeTarget, ref target, ref method, isStep);
                if (null != method)
                {
                    ControlGUI controlGUI = MethodsGUI.GetControl(pFldCurrent);
                    if ((null != controlGUI) &&
                        controlGUI.IsLabel && StrFunc.IsEmpty(controlGUI.Name) && null != pFldParent)
                        controlGUI.Name = pFldParent.Name;

                    ParameterInfo[] pi = method.GetParameters();
                    object result = null;
                    if (0 < pi.Length)
                    {
                        object[] _params = new object[] { pCurrent, pFldCurrent, pParent, pFldParent, controlGUI, pGrandParent, pFldGrandParent, this };
                        String[] _names = new String[] { pi[0].Name, pi[1].Name, pi[2].Name, pi[3].Name, pi[4].Name, pi[5].Name, pi[6].Name, pi[7].Name };
                        result = typeTarget.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, target, _params, null, null, _names);
                    }
                    if (null != result)
                    {
                        Control ctrlResult = (Control)result;
                        if (0 < ctrlResult.Controls.Count)
                        {
                            if (MethodsGUI.IsLineFeedBefore(controlGUI.LineFeed))
                                pPlaceHolder.Controls.Add(new LiteralControl("<br/>"));
                            pPlaceHolder.Controls.Add(ctrlResult);
                            if (MethodsGUI.IsLineFeedAfter(controlGUI.LineFeed))
                                pPlaceHolder.Controls.Add(new LiteralControl("<br/>"));

                            if (isStartDiv && pFldCurrent.IsDefined(typeof(OpenDivGUI), true))
                            {
                                object[] attributes = pFldCurrent.GetCustomAttributes(typeof(OpenDivGUI), true);
                                foreach (OpenDivGUI openDiv in attributes)
                                {
                                    if (openDiv.IsCopyPaste)
                                    {
                                        string objectValueName = pCurrent.GetType().Name;
                                        //										
                                        object oChild = pFldCurrent.GetValue(pCurrent);
                                        if ((null != oChild) && (false == oChild.GetType().IsArray))
                                            objectValueName = oChild.GetType().Name;
                                        //
                                        ctrlResult.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
                                        FpMLCopyPasteButton btnCopyPaste = new FpMLCopyPasteButton(GetUniqueID(), openDiv.Name, objectValueName, pFldCurrent.Name, false, true);
                                        ctrlResult.Controls.Add(btnCopyPaste);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    KeyBaliseName.Add(MethodsGUI.CreateUniqueID(this, pParent, pFldParent, pCurrent, pFldCurrent, 0));
                #endregion CreateControl Adding

                #region AutoClosed
                if (isAutoClose)
                {

                    pPlaceHolder.Controls.Add(MethodsGUI.MakeDiv(this, new CloseDivGUI(MethodsGUI.LevelEnum.First), false));
                }
                #endregion AutoClosed
            }
            catch (Exception)
            {
#if DEBUG
                System.Diagnostics.Debug.Write(pCurrent.GetType().Name + "-" + pFldCurrent.Name + Environment.NewLine);
#endif
                throw;
            }
		}
		#endregion Display
		#region public LoadEnumObjectReference
		public void LoadEnumObjectReference(string pFpMLKeyReference,string pOldValue,string pNewValue)
		{
			LoadEnumObjectReference(pFpMLKeyReference,pOldValue,pOldValue,pNewValue,pNewValue);
		}
		public void LoadEnumObjectReference(string pFpMLKeyReference,string pOldValue,string pOldExtValue,
			string pNewValue,string pNewExtValue)
		{
			//pNewExtValue : pNewExtendValue
			if (StrFunc.IsFilled(pOldValue))
			{
				FpMLItemReference itemOldReference = new FpMLItemReference(pFpMLKeyReference,pOldValue,pOldExtValue);
				int i = ListFpMLReference.BinarySearch(itemOldReference, new CompareFpMLItemReference());
				if (-1 < i)
					ListFpMLReference.RemoveAt(i);
			}
			if (StrFunc.IsFilled(pNewValue))
			{
				FpMLItemReference itemNewReference = new FpMLItemReference(pFpMLKeyReference,pNewValue,pNewExtValue);
				int i = ListFpMLReference.BinarySearch(itemNewReference, new CompareFpMLItemReference());
				if (0 > i)
				{
					ListFpMLReference.Add(itemNewReference);
					ListFpMLReference.Sort(new CompareFpMLItemReference());
				}
			}
		}
		#endregion LoadEnumObjectReference
		#region public Read
		/// <summary>
		/// Boucle Principale d'analyse des classes (récursive)
		/// </summary>
		/// <param name="oCurrent">Objet courant</param>
		/// <param name="phCapture">Container de l'ensemble des contrôles générés</param>
		/// <revision>
		///     <version>1.2.0</version><date>20071003</date><author>EG</author>
		///     <comment>
		///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
		///     </comment>
		/// </revision>
		public void Read(object pCurrent,ref PlaceHolder pPlaceHolder)
		{
			Read(pCurrent,ref pPlaceHolder,null,null);
		}
		public void Read(object pCurrent,ref PlaceHolder pPlaceHolder,object pParent,FieldInfo pFldParent)
		{
			Read(pCurrent,ref pPlaceHolder,pParent,pFldParent,null,null);
		}
		public void Read(object pCurrent,ref PlaceHolder pPlaceHolder,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent)
		{
			//Type tCurrent = pCurrent.GetType();
			//
			ArrayList aFieldsCurrent = SortFieldInfo(pCurrent);
			FieldInfo fldCurrent;
			for (int i=0;i<aFieldsCurrent.Count;i++)
			{
				IsChildrenInclude = true;
				fldCurrent        = (FieldInfo)aFieldsCurrent[i];
				object oChildCurrent = fldCurrent.GetValue(pCurrent);
				if (false == MethodsGUI.IsNoneControl(fldCurrent))
				{
					if (null != oChildCurrent)
					{
						Type tChildCurrent = oChildCurrent.GetType();
						#region Array
						if (tChildCurrent.IsArray)
						{
							Type tInterface = tChildCurrent.GetElementType().GetInterface("IEFS_Array");
							if ((null != tInterface) || (tChildCurrent.GetElementType().IsEnum))
							{
								Display(pCurrent,fldCurrent,ref pPlaceHolder,pParent,pFldParent,pGrandParent,pFldGrandParent);
								#region CloseDiv Specified
								if ((i+1) != aFieldsCurrent.Count)
									if (((FieldInfo)aFieldsCurrent[i+1]).IsDefined(typeof(CloseDivGUI),true) && 
										MethodsGUI.IsNoneControl(fldCurrent))
									{
										object[] attributes = ((FieldInfo)aFieldsCurrent[i+1]).GetCustomAttributes(typeof(CloseDivGUI),true);
										pPlaceHolder.Controls.Add(MethodsGUI.MakeDiv(attributes[0]));
									}
								#endregion CloseDiv Specified
							}
						}
							#endregion Array
						#region Element
						else
						{
							Display(pCurrent,fldCurrent,ref pPlaceHolder,pParent,pFldParent,pGrandParent,pFldGrandParent,true,DisplayEndDiv(i,pCurrent,aFieldsCurrent));
							if (tChildCurrent.IsClass && ("EFS.EfsML.dll" == tChildCurrent.Module.ScopeName) && 
								(-1 == fldCurrent.Name.IndexOf("Item")) && IsChildrenInclude) 
								Read(oChildCurrent,ref pPlaceHolder,pCurrent,fldCurrent,pParent,pFldParent);
							else if (-1 < fldCurrent.Name.IndexOf(Cst.FpML_SerializeKeySpecified))
								i++;
						}
						#endregion Element
					}
					else
						Display(pCurrent,fldCurrent,ref pPlaceHolder,pParent,pFldParent,pGrandParent,pFldGrandParent,true,DisplayEndDiv(i,pCurrent,aFieldsCurrent));
				}
			}
		
		}
		#endregion Read
		//
		#region public LoadFpmlReference
		public void LoadListFpMLReference(object pCurrent)
		{
			if (null != pCurrent)
			{
				//Type tCurrent = pCurrent.GetType();
				//
				ArrayList aFieldsCurrent = SortFieldInfo(pCurrent);
				//
				for (int i=0;i<aFieldsCurrent.Count;i++)
				{
					FieldInfo fldCurrent = (FieldInfo)aFieldsCurrent[i];
					object oChildCurrent = fldCurrent.GetValue(pCurrent);
					
					if (null!= oChildCurrent)
					{
						if (oChildCurrent.GetType().IsArray)
						{
							foreach (object objItem in (System.Array)oChildCurrent)
								LoadListFpMLReference(objItem);
						}
						else
						{
							if (oChildCurrent.GetType().Equals(typeof(EFS_Id)))  
							{
								string[] reference =  MethodsGUI.GetReference(pCurrent,fldCurrent);
								if (ArrFunc.IsFilled(reference))
								{ 
									for (int j = 0 ; j< reference.Length; j++)
										LoadEnumObjectReference(reference[j] , string.Empty , ((EFS_Id)oChildCurrent).Value)  ;
								}
							}
							else
								LoadListFpMLReference(oChildCurrent);
						}
					}
				
				}
			}
		}
		#endregion
		#region public Start
		public void Start(ref PlaceHolder pPlaceHolder,object pDocument)
		{
            #region Root OpenDivGUI 
            OpenDivGUI openDiv = new OpenDivGUI(MethodsGUI.LevelEnum.First, Cst.OTCml_Name, true)
            {
                UniqueId = Cst.OTCml_Name
            };
            pPlaceHolder.Controls.Add(MethodsGUI.MakeDiv(this,openDiv));
			#endregion Root OpenDivGUI 
			//
			Read(pDocument,ref pPlaceHolder);
			//
			#region Root CloseDivGUI
			pPlaceHolder.Controls.Add(MethodsGUI.MakeDiv(new CloseDivGUI(MethodsGUI.LevelEnum.First)));
			#endregion Root CloseDivGUI
		}
		#endregion Start
		#region public DeleteHistoryBalise
		public void DeleteHistoryBalise(int pIndex)
		{
			if (pIndex <= HistoryBalise.Count)
				HistoryBalise.RemoveRange(pIndex,HistoryBalise.Count-pIndex);
		}
		#endregion
        #region public GetUniqueID
        public string GetUniqueID()
        {
            m_Nb++;
            return "fCtl" + m_Nb.ToString();
        }
        #endregion
        //
		#region private DisplayEndDiv
		private static bool DisplayEndDiv(int pItem,object pObj,ArrayList paFlds)
		{
			bool isEndDiv = false;
			if (0 < pItem)
			{
				object previousObj = ((FieldInfo) paFlds[pItem-1]).GetValue(pObj);
				if (null != previousObj)
					isEndDiv = (false == previousObj.GetType().IsArray) || 
						null != (previousObj.GetType().GetElementType().GetInterface("IEFS_Array"));
				else
					isEndDiv = true;
			}
			return isEndDiv;
		}
		#endregion DisplayEndDiv
		#region private GetInvokeMember
		private InvokeMemberEnum GetInvokeMember(object pCurrent,FieldInfo pFldCurrent,ref Type pTypeTarget,ref object pTarget,ref MethodInfo pMethod,bool pIsStep)
		{
			MethodInfo method = null;
			// Enum ou Booleen rattaché à un Enum
			if (IsEnumFieldInfo(pCurrent,pFldCurrent,ref method))
			{
				#region IsEnum
				pTarget     = ExtendEnumsGUI;
				pTypeTarget = pTarget.GetType();
				pMethod     = method;
				return InvokeMemberEnum.IsEnum;
				#endregion IsEnum
			}
				// la classe possède une interface pour traiter les Array ou les choice.
			else if (IsInterfaceFieldInfo(pFldCurrent,ref method)) 
			{
				#region IsInterface
				if (pFldCurrent.FieldType.IsArray)
					pTypeTarget = pFldCurrent.FieldType.GetElementType();
				else
					pTypeTarget = pFldCurrent.FieldType;
				pTarget         = pTypeTarget.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
				pMethod         = method;
				return InvokeMemberEnum.IsInterface;
				#endregion IsInterface
			}
				// Nous sommes en présence d'un objet générique (relatif à un choice)
			else if (IsChoiceFieldInfo(pCurrent,pFldCurrent,ref method,pIsStep))
			{
				#region IsChoice
				pTarget     = pFldCurrent.GetValue(pCurrent);
				pTypeTarget = pTarget.GetType();
				if (null == pTarget)
				{
					pTarget = pTypeTarget.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
					if (-1 == method.Name.IndexOf(MethodsGUI.CreateControlEnum.Optional.ToString()))
						pFldCurrent.SetValue(pCurrent,pTarget);
				}
				pMethod = method;
				return InvokeMemberEnum.IsChoice;
				#endregion IsChoice
			}
				// la classe hérite d'une classe de type GUI
			else if (IsDeriveFieldInfo(pFldCurrent,ref method,pIsStep))
			{
				#region IsDerive
				pTypeTarget = pFldCurrent.FieldType.BaseType;
				pTarget     = pFldCurrent.GetValue(pCurrent);
				if (null == pTarget)
				{
					pTarget = pFldCurrent.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
					if (-1 == method.Name.IndexOf(MethodsGUI.CreateControlEnum.Optional.ToString()))
						pFldCurrent.SetValue(pCurrent,pTarget);
				}
				pMethod = method;
				return InvokeMemberEnum.IsDerive;
				#endregion IsDerive
			}
				// la classe hérite d'une classe de type xxxGUI (cas Specified)
			else if (IsDeriveSpecifiedFieldInfo(pCurrent,pFldCurrent,ref method,pIsStep))
			{
				#region IsDeriveSpecified
				FieldInfo fldAssociate = pCurrent.GetType().GetField(pFldCurrent.Name.Replace(Cst.FpML_SerializeKeySpecified,string.Empty));
				pTypeTarget            = fldAssociate.FieldType.BaseType;
				pTarget                = fldAssociate.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
				pMethod                = method;
				return InvokeMemberEnum.IsDeriveSpecified;
				#endregion IsDeriveSpecified
			}
				// tous les autres cas
			else
			{
				#region IsClassic
				pTarget     = pCurrent;
				pTypeTarget = pTarget.GetType();
				pMethod     = pTypeTarget.GetMethod("INIT_" + pFldCurrent.Name);
				return InvokeMemberEnum.IsClassic;
				#endregion IsClassic
			}
		}
		#endregion GetInvokeMember
		#region private IsEnumFieldInfo
		/// <summary>
		/// <newpara><b>Description :</b></newpara>
		/// <newpara><b>Contents :</b></newpara>
		/// <newpara>Determine si le fieldinfo en cours d'analyse est un Enum ou un boolean 'Specified' rattaché à un Enum</newpara>
		///</summary>
		private bool IsEnumFieldInfo(object pCurrent,FieldInfo pFldCurrent,ref MethodInfo pMethod)
		{
			bool isEnum = pFldCurrent.FieldType.IsEnum;
			Type tEnum  = ExtendEnumsGUI.GetType();
			//
			if (isEnum)
			{
				//
				FieldInfo fld = pCurrent.GetType().GetField(pFldCurrent.Name + Cst.FpML_SerializeKeySpecified);
				if (null != fld)
					pMethod = tEnum.GetMethod(MethodsGUI.CreateControlEnum.Optional.ToString());
				else
					pMethod = tEnum.GetMethod(MethodsGUI.CreateControlEnum.Mandatory.ToString());
			}
			else
			{
				if (-1 < pFldCurrent.Name.IndexOf(Cst.FpML_SerializeKeySpecified))
				{
					FieldInfo fld = pCurrent.GetType().GetField(pFldCurrent.Name.Replace(Cst.FpML_SerializeKeySpecified,string.Empty));
					if (null != fld)
					{
						isEnum = fld.FieldType.IsEnum;
						if (isEnum) 
							pMethod = tEnum.GetMethod(MethodsGUI.CreateControlEnum.Optional.ToString());
					}
				}
			}
			return isEnum;
		}
		#endregion IsEnumFieldInfo
		#region private IsInterfaceFieldInfo
		/// <summary>
		/// <newpara><b>Description :</b></newpara>
		/// <newpara><b>Contents :</b></newpara>
		/// <newpara>Determine si le fieldinfo en cours d'analyse possède une interface de gestion d'un objet de type Array
		/// ou choice </newpara>
		///</summary>
		private static bool IsInterfaceFieldInfo(FieldInfo pFldCurrent,ref MethodInfo pMethod)
		{
			pMethod     = null;
			Type tField = pFldCurrent.FieldType;
			if (tField.IsArray && (null != tField.GetElementType().GetInterface("IEFS_Array")))
				pMethod = tField.GetElementType().GetInterface("IEFS_Array").GetMethod("DisplayArray");
			else if (null != tField.GetInterface("IEFS_Choice"))
				pMethod = tField.GetInterface("IEFS_Choice").GetMethod("DisplayChoice");
			return (null != pMethod);
		}
		#endregion IsInterfaceFieldInfo
		#region private IsChoiceFieldInfo
		/// <summary>
		/// <newpara><b>Description :</b></newpara>
		/// <newpara><b>Contents :</b></newpara>
		/// <newpara>Determine si le fieldinfo en cours d'analyse est de type System.Object (un choice se cache derriere)</newpara>
		///</summary>
		private static bool IsChoiceFieldInfo(object pCurrent,FieldInfo pFldCurrent,ref MethodInfo pMethod,bool pIsStep)
		{
			pMethod = null;
			MethodsGUI.CreateControlEnum createControl = MethodsGUI.GetCreateControl(pFldCurrent,pIsStep);
			if (pFldCurrent.FieldType.Equals(typeof(System.Object)))
			{
				if (null == pFldCurrent.GetValue(pCurrent))
					pFldCurrent.SetValue(pCurrent,pFldCurrent.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null));
				Type t  = pFldCurrent.GetValue(pCurrent).GetType();
				pMethod = t.GetMethod(createControl.ToString());
			}
			return (null != pMethod);
		}
		#endregion IsChoiceFieldInfo
		#region private IsDeriveFieldInfo
		/// <summary>
		/// <newpara><b>Description :</b></newpara>
		/// <newpara><b>Contents :</b></newpara>
		/// <newpara>Determine si le fieldinfo en cours d'analyse hérite d'une classe</newpara>
		///</summary>
		private static bool IsDeriveFieldInfo(FieldInfo pFldCurrent,ref MethodInfo pMethod,bool pIsStep)
		{
			pMethod = null;
			MethodsGUI.CreateControlEnum createControl = MethodsGUI.GetCreateControl(pFldCurrent,pIsStep);
			Type tBase = pFldCurrent.FieldType.BaseType;
			if (null != tBase)
			{
				if (MethodsGUI.IsValidatorOptionalControl(createControl))
					pMethod = tBase.GetMethod(MethodsGUI.CreateControlEnum.Mandatory.ToString());
				else
					pMethod = tBase.GetMethod(createControl.ToString());
			}
			return (null != pMethod);
		}
		#endregion IsDeriveFieldInfo
		#region private IsDeriveSpecifiedFieldInfo
		/// <summary>
		/// <newpara><b>Description :</b></newpara>
		/// <newpara><b>Contents :</b></newpara>
		/// <newpara>Determine si le fieldinfo en cours d'analyse hérite d'une classe</newpara>
		///</summary>
		private static bool IsDeriveSpecifiedFieldInfo(object pCurrent,FieldInfo pFldCurrent,ref MethodInfo pMethod,bool pIsStep)
		{
			pMethod = null;
			if (-1 < pFldCurrent.Name.IndexOf(Cst.FpML_SerializeKeySpecified))
			{
				MethodsGUI.CreateControlEnum createControl = MethodsGUI.GetCreateControl(pFldCurrent,pIsStep);
				FieldInfo fldAssociate = pCurrent.GetType().GetField(pFldCurrent.Name.Replace(Cst.FpML_SerializeKeySpecified,string.Empty));
				if (null != fldAssociate)
				{
					Type tBase = fldAssociate.FieldType.BaseType;
					if (null != tBase)
						pMethod = tBase.GetMethod(createControl.ToString());
				}
			}
			return (null != pMethod);
		}
		#endregion IsDeriveSpecifiedFieldInfo

		#region private SortFieldInfo
		private static ArrayList SortFieldInfo(object pCurrent)
		{
			ArrayList aFlds = new ArrayList(); 
			bool hasContinue = true;
			Type tCurrent = pCurrent.GetType();
			FieldInfo[] flds = tCurrent.GetFields(m_FpMLBindingFlags | BindingFlags.DeclaredOnly);

			if (ArrFunc.IsFilled(flds))
			{
				Array.Sort(flds, new CompareFieldInfo());
				aFlds.AddRange(flds);
			}
			while (hasContinue)
			{
				tCurrent = tCurrent.BaseType;
				if ((null == tCurrent) || (tCurrent.IsPrimitive) || (tCurrent.IsValueType))
					hasContinue = false;
				else
				{
					flds = tCurrent.GetFields(m_FpMLBindingFlags | BindingFlags.DeclaredOnly);
					if (ArrFunc.IsFilled(flds))
					{
						Array.Sort(flds, new CompareFieldInfo());
						aFlds.InsertRange(0, flds);
					}
				}
			}
			return aFlds;
		}
		#endregion SortFieldInfo
		//
	}
	#region CompareFieldInfo
	public class CompareFieldInfo : IComparer
	{
		public int Compare(object pFld1, object pFld2)
		{
			FieldInfo fld1 = (FieldInfo)pFld1;
			FieldInfo fld2 = (FieldInfo)pFld2;
			return (fld1.FieldHandle.Value.ToInt64().CompareTo(fld2.FieldHandle.Value.ToInt64()));
		}
	}
	#endregion CompareYieldCurvePoint

	#endregion FullConstructor
}
