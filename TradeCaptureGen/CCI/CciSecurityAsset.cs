#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// CciSecurityAsset
    /// </summary>
    // EG 20150412 [20513] BANCAPERTA
    public class CciSecurityAsset : ContainerCciBase, IContainerCciFactory,  IContainerCciSpecified, IContainerCciGetInfoButton, ICciPresentation 
    {
        #region Enum
        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("securityId")]
            securityId,
            [System.Xml.Serialization.XmlEnumAttribute("securityName")]
            securityName,
            [System.Xml.Serialization.XmlEnumAttribute("securityDescription")]
            securityDescription,
            [System.Xml.Serialization.XmlEnumAttribute("securityIssueDate")]
            securityIssueDate,
            issuer,
            isNewAsset,
            template,
            unknown
        }
        #endregion
        
        #region Members
        
        private readonly CciTradeBase cciTrade;
        public ISecurityAsset securityAsset;
        public CciDebtSecurity cciDebtSecurity;
        
        #endregion
        
        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }

        public bool IsNewAsset
        {
            get { return BoolFunc.IsTrue(CcisBase.GetNewValue(CciClientId(CciEnum.isNewAsset))); }
        }

        #endregion

        #region constructor
        public CciSecurityAsset(CciTradeBase pTrade, string pPrefix, ISecurityAsset pSecurityAsset) :
            base(pPrefix, pTrade.Ccis)
        {
            cciTrade = pTrade;

            securityAsset = pSecurityAsset;

            if (pSecurityAsset.DebtSecuritySpecified)
                cciDebtSecurity = new CciDebtSecurity(pTrade, pSecurityAsset.DebtSecurity);
        }
        #endregion constructor

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            //Don't erase
            CreateInstance();

            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.securityName.ToString()), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.securityDescription.ToString()), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.securityIssueDate.ToString()), false, TypeData.TypeDataEnum.date);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.issuer.ToString()), false, TypeData.TypeDataEnum.@string);

            if (null != cciDebtSecurity)
                cciDebtSecurity.AddCciSystem();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {

            CciTools.CreateInstance(this, securityAsset);
            CciDebtSecurity cciDebtSecurityCurrent = new CciDebtSecurity(cciTrade, securityAsset.DebtSecurity, Prefix + TradeCustomCaptureInfos.CCst.Prefix_debtSecurity);

            bool isOk = CcisBase.Contains(cciDebtSecurityCurrent.cciSecurity.CciClientId(CciSecurity.CciEnum.couponType));
            if (isOk)
            {
                cciDebtSecurity = cciDebtSecurityCurrent;
                if (false == securityAsset.DebtSecuritySpecified)
                    securityAsset.DebtSecurity = cciTrade.CurrentTrade.Product.ProductBase.CreateDebtSecurity();

                cciDebtSecurity.debtSecurity = securityAsset.DebtSecurity;
                cciDebtSecurity.Initialize_FromCci();
            }

        }
        
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
        public void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (CciEnum cciEnum in Enum.GetValues(tCciEnum))
            {
                CustomCaptureInfo cci = Cci(cciEnum);  
                if (cci != null)
                {
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    bool isToValidate = false;

                    switch (cciEnum)
                    {
                        case CciEnum.securityId:
                            data = securityAsset.SecurityId.Value;
                            if (StrFunc.IsFilled(securityAsset.OtcmlId) && securityAsset.OTCmlId > 0)
                            {
                                SQL_TradeDebtSecurity debtsec = new SQL_TradeDebtSecurity(cciTrade.CSCacheOn, securityAsset.OTCmlId);
                                if (debtsec.IsLoaded)
                                    sql_Table = (SQL_Table)debtsec;
                            }
                            break;

                        case CciEnum.isNewAsset:
                            if (securityAsset.IsNewAssetSpecified)
                                data = securityAsset.IsNewAsset.ToString().ToLower();
                            break;

                        case CciEnum.template:
                            if (securityAsset.IdTTemplateSpecified)
                                data = securityAsset.IdTTemplate.ToString();
                            break;

                        case CciEnum.securityName:
                            if (securityAsset.SecurityNameSpecified)
                                data = securityAsset.SecurityName.Value;
                            break;

                        case CciEnum.securityDescription:
                            if (securityAsset.SecurityDescriptionSpecified)
                                data = securityAsset.SecurityDescription.Value;
                            break;

                        case CciEnum.securityIssueDate:
                            if (securityAsset.SecurityIssueDateSpecified)
                                data = securityAsset.SecurityIssueDate.Value;
                            break;

                        case CciEnum.issuer:
                            IParty issuer = null;
                            if (null != securityAsset.Issuer)
                                issuer = securityAsset.Issuer;
                            else if (securityAsset.IssuerReferenceSpecified && (null != securityAsset.IssuerReference))
                                issuer = cciTrade.DataDocument.GetParty(securityAsset.IssuerReference.HRef);

                            if (null != issuer)
                            {
                                SQL_Actor sql_Actor = null;
                                if (StrFunc.IsFilled(issuer.OtcmlId) && issuer.OTCmlId > 0)
                                    sql_Actor = new SQL_Actor(cciTrade.CSCacheOn, issuer.OTCmlId);
                                else if (StrFunc.IsFilled(issuer.PartyId))
                                    sql_Actor = new SQL_Actor(cciTrade.CSCacheOn, issuer.PartyId);
                                //
                                if (null != sql_Actor)
                                    sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.ISSUER });
                                //
                                if ((null != sql_Actor) && sql_Actor.IsLoaded)
                                {
                                    //Affectation .XmlId
                                    //Ne pas toucher => Permet d'afficher les trades même si le xmlId d'un acteur a changé (Si son code bic ou identifier a changé)
                                    sql_Actor.XmlId = issuer.Id;
                                    //
                                    data = sql_Actor.Identifier;
                                    sql_Table = (SQL_Table)sql_Actor;
                                    isToValidate = (issuer.OTCmlId == 0);
                                }
                                else
                                {
                                    if (StrFunc.IsFilled(issuer.Id))
                                    {
                                        isToValidate = (issuer.Id == TradeCommonCustomCaptureInfos.PartyUnknown);
                                        // 20090512 RD
                                        if (false == isToValidate)
                                            isToValidate = (issuer.Id.ToLower() == TradeCommonCustomCaptureInfos.PartyIssuer.ToLower());
                                    }
                                    //
                                    data = issuer.Id;
                                }
                            }

                            break;

                        default:
                            isSetting = false;
                            break;

                    }
                    if (isSetting)
                    {
                        CcisBase.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }
                }
            }
            //
            if (null != cciDebtSecurity)
                cciDebtSecurity.Initialize_FromDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                
                CciEnum elt = CciEnum.unknown;
                if (Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                
                switch (elt)
                {
                    case CciEnum.securityId:
                        break;
                    case CciEnum.template:
                        break;
                    case CciEnum.isNewAsset:
                        if (IsNewAsset)
                        {
                            //En mode création de titre il ne peut y avoir d'erreur
                            CcisBase.SetErrorMsg(CciClientId(CciEnum.securityId), string.Empty);
                        }
                        else
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.template), string.Empty);
                            //histoire de revalider le titre saisi dans securityId on positionne le lastValue à empty
                            Cci(CciEnum.securityId).LastValue = string.Empty;
                        }
                        break;

                    case CciEnum.issuer:
                        if (null != cciDebtSecurity)
                        {
                            string lastIssuer_id = pCci.LastValue;
                            string issuer_id = pCci.NewValue;
                            
                            if (null != pCci.LastSql_Table)
                                lastIssuer_id = ((SQL_Actor)pCci.LastSql_Table).XmlId;
                            if (null != pCci.Sql_Table)
                                issuer_id = ((SQL_Actor)pCci.Sql_Table).XmlId;
                                                            
                            cciDebtSecurity.SynchronizePayerReceiver(lastIssuer_id, issuer_id);
                            
                            Ccis.ProcessInitialize_Issuer(pCci, cciDebtSecurity.cciSecurity);
                        }
                        break;

                    case CciEnum.unknown:
                        break;

                }

            }
            //Les éléments du dataDocument géré par cciDebtSecurity ne sont considérés que l'orsque l'asset n'est pas renseigné
            //car dans ce cas toutes les données proviennent de l'asset chargé
            if (null == Cci(CciEnum.securityId).Sql_Table && null != cciDebtSecurity)
                cciDebtSecurity.ProcessInitialize(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            if (!isOk && (null != cciDebtSecurity))
                isOk = cciDebtSecurity.IsClientId_PayerOrReceiver(pCci);
            return isOk;
        }
        
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
        public void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    
                    #region Reset variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.securityId:
                            if (IsNewAsset)
                            {
                                securityAsset.SecurityId.Value = data;
                            }
                            else
                            {
                                // Load Asset
                                DumpDebtSecurity_ToDocument(cci, data);
                                //
                                ////20090625 RD Ne pas effacer la saisie de l'utilisateur
                                if (null == cci.Sql_Table)
                                {
                                    string tempNewValue = cci.NewValue;
                                    Clear();
                                    cci.NewValue = tempNewValue;
                                }
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;


                        case CciEnum.isNewAsset:
                            //
                            securityAsset.IsNewAsset = BoolFunc.IsTrue(data);
                            securityAsset.IsNewAssetSpecified = true;
                            if (securityAsset.IsNewAsset)
                                securityAsset.OTCmlId = 0; // pour passer en mode création

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;


                        case CciEnum.template:
                            securityAsset.IdTTemplateSpecified = StrFunc.IsFilled(data) & IsNewAsset;
                            //
                            if (securityAsset.IdTTemplateSpecified)
                                securityAsset.IdTTemplate = IntFunc.IntValue(data);
                            //   
                            if (securityAsset.IdTTemplateSpecified)
                            {
                                // Load Template
                                DumpDebtSecurity_ToDocument(securityAsset.IdTTemplate);
                                securityAsset.SecurityId.Value = CcisBase.GetNewValue(CciClientId(CciEnum.securityId));
                                //
                                // C'est un nouveau Asset
                                securityAsset.OTCmlId = 0;
                                // Name et Display sont volontairement non retenus pour alimenter le SecurityAsset
                                securityAsset.SecurityName.Value = string.Empty;
                                securityAsset.SecurityNameSpecified = false;
                                securityAsset.SecurityDescription.Value = string.Empty;
                                securityAsset.SecurityDescriptionSpecified = false;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;


                        case CciEnum.securityName:
                            securityAsset.SecurityName.Value = data;
                            securityAsset.SecurityNameSpecified = StrFunc.IsFilled(data);
                            break;

                        case CciEnum.securityDescription:
                            securityAsset.SecurityDescription.Value = data;
                            securityAsset.SecurityDescriptionSpecified = StrFunc.IsFilled(data);
                            break;


                        case CciEnum.securityIssueDate:
                            securityAsset.SecurityIssueDate.Value = data;
                            securityAsset.SecurityIssueDateSpecified = StrFunc.IsFilled(data);
                            break;


                        case CciEnum.issuer:
                            // Load Asset
                            if (StrFunc.IsFilled(data) && IsNewAsset)
                            {
                                DumpIssuer_ToDocument(cci, data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            }
                            break;

                        default:
                            isSetting = false;
                            break;

                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //20090507 RD 
            //Les éléments du dataDocument géré par cciDebtSecurity ne sont considérés que lorsque l'asset n'est pas renseigné
            //car dans ce cas toutes les données proviennent de l'asset chargé
            if (null == Cci(CciEnum.securityId).Sql_Table && null != cciDebtSecurity)
                cciDebtSecurity.Dump_ToDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
            if (null != cciDebtSecurity)
                cciDebtSecurity.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string _)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            CcisBase.Set(CciClientId(CciEnum.securityName), "IsEnabled", IsNewAsset);
            CcisBase.Set(CciClientId(CciEnum.securityDescription), "IsEnabled", IsNewAsset);
            CcisBase.Set(CciClientId(CciEnum.securityIssueDate), "IsEnabled", IsNewAsset);
            CcisBase.Set(CciClientId(CciEnum.issuer), "IsEnabled", IsNewAsset);
            CcisBase.Set(CciClientId(CciEnum.template), "IsEnabled", IsNewAsset);
            //

            if (null != cciDebtSecurity)
                cciDebtSecurity.SetEnabled(IsNewAsset);

            // je ne sais pas est-ce que c'est le bon endroit mais j'ai pas le choix
            CcisBase.Set(CciClientId(CciEnum.securityId), "IsMandatory", (false == IsNewAsset));

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.securityId, pCci))
            {
                pCci.Display = string.Empty;
                //
                string name = securityAsset.SecurityId.Value;
                string description = string.Empty;
                if (securityAsset.SecurityDescriptionSpecified)
                    description = securityAsset.SecurityDescription.Value;
                //                        
                if (StrFunc.IsFilled(description))
                {
                    if (description.StartsWith(name))
                        pCci.Display = description;
                    else
                        pCci.Display = name + Cst.Space + "/" + Cst.Space + description;
                }
                else
                    pCci.Display = name;
            }
            //
            if (null != cciDebtSecurity)
                cciDebtSecurity.SetDisplay(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            if (null != cciDebtSecurity)
                cciDebtSecurity.Initialize_Document();
        }
        
        #endregion
        
        
        
        #region IContainerCciSpecified Membres
        public bool IsSpecified { get { return Cci(CciEnum.securityId).IsFilled; } }
        #endregion

        #region IContainerCciGetInfoButton Membres
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            //            
            #region buttons of cciDebtSecurity
            if (!isOk && null != cciDebtSecurity)
                isOk = cciDebtSecurity.SetButtonZoom(pCci, pCo, ref pIsObjSpecified, ref pIsEnabled);
            #endregion
            //
            return isOk;

        }
        #endregion

        #region IContainerCciGetInfoButton Membres
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

            if (IsCci(CciEnum.securityId, pCci))
            {
                pCo.DynamicArgument = null;
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Consultation = "DEBTSECURITY";
                pCo.SqlColumn = "IDENTIFIER";
                //
                StringDynamicData idi = new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDI", cciTrade.Product.IdI.ToString());
                StringDynamicData dt = new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "TRADEDATE", cciTrade.DataDocument.TradeHeader.TradeDate.Value)
                {
                    dataformat = DtFunc.FmtISODate
                };
                //
                pCo.DynamicArgument = new string[2] { idi.Serialize(), dt.Serialize() };
            }


        }
        #endregion

        #region Methods
        /// <summary>
        /// Dump a DebtSecurity into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpDebtSecurity_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            //
            //#warning suppression potentiel de l'emtteur alors qu'il peut-être utilisé par ailleurs
            //Il faudrait vérifier que l'émetteur n'est pas utilisé par ailleurs
            //Pour l'instant on néglige puisque le trade ne pourra être validé (href orphelin d'Id)
            //
            //20090925 FI La suppression de l'émetteur ne s'effectue que s'il n'est pas contrepartie
            if (pCci.IsLastFilled && securityAsset.DebtSecuritySpecified && StrFunc.IsFilled(securityAsset.DebtSecurity.Stream[0].PayerPartyReference.HRef))
            {
                if (StrFunc.IsFilled(Cci(CciEnum.issuer).LastValue) && (false == Ccis.IsValueUseInCciParty(Cci(CciEnum.issuer).LastValue)))
                    cciTrade.DataDocument.RemoveParty(securityAsset.DebtSecurity.Stream[0].PayerPartyReference.HRef);
            }
            //
            pCci.ErrorMsg = string.Empty;
            pCci.Sql_Table = null;
            securityAsset.OTCmlId = 0;
            //
            bool isLoaded = false;
            bool isFound = false;
            SQL_TradeDebtSecurity sqlDebtSecurityAsset = null;
            if (StrFunc.IsFilled(pData))
                sqlDebtSecurityAsset = SearchDebtSecurity(cciTrade.CSCacheOn, pData, cciTrade.Product.IdI, out isLoaded, out isFound);
            //
            if (isFound)
            {
                #region isFound
                pCci.NewValue = sqlDebtSecurityAsset.Identifier;
                pCci.Sql_Table = sqlDebtSecurityAsset;
                pCci.ErrorMsg = string.Empty;
                // 
                // 20090626 RD Load Asset
                DumpDebtSecurity_ToDocument(sqlDebtSecurityAsset.IdT);
                #endregion isFound
            }
            else
            {
                //					
                if (pCci.IsFilled)
                {
                    if (isLoaded)
                        pCci.ErrorMsg = Ressource.GetString("Msg_DebtSecurityAssetNotUnique");
                    else
                        pCci.ErrorMsg = Ressource.GetString("Msg_DebtSecurityAssetNotFound");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDebtSecurity_IDT"></param>
        // EG 20150412 [20513] BANCAPERTA
        private void DumpDebtSecurity_ToDocument(int pDebtSecurity_IDT)
        {
            securityAsset.OTCmlId = 0;

            CaptureTools.SetSecurityAssetInSecurityAsset(securityAsset, cciTrade.DataDocument, cciTrade.CSCacheOn, null, pDebtSecurity_IDT);
            if (cciTrade.Product.IsDebtSecurityTransaction)
            {
                IDebtSecurityTransaction debtSecurityTransaction = cciTrade.Product.Product as IDebtSecurityTransaction;
                debtSecurityTransaction.SecurityAssetSpecified = (securityAsset.OTCmlId > 0);
                debtSecurityTransaction.SecurityAssetReferenceSpecified = false;
                //
                if (debtSecurityTransaction.SecurityAssetSpecified)
                {
                    for (int i = 0; i < ArrFunc.Count(securityAsset.DebtSecurity.Stream); i++)
                        securityAsset.DebtSecurity.Stream[i].ReceiverPartyReference.HRef = debtSecurityTransaction.GrossAmount.PayerPartyReference.HRef;
                }
            }
            else if (cciTrade.Product.IsDebtSecurityOption)
            {
                IDebtSecurityOption debtSecurityOption = cciTrade.Product.Product as IDebtSecurityOption;
                debtSecurityOption.SecurityAssetSpecified = (securityAsset.OTCmlId > 0);
            }

            if (securityAsset.OTCmlId > 0)
            {
                //if ((securityAsset.issuerSpecified) && (TradeCommonCustomCaptureInfos.PartyIssuer != securityAsset.issuer.id))
                //{
                //    securityAsset.issuer = cciTrade.DataDocument.AddParty(securityAsset.issuer);
                //    cciTrade.DataDocument.SetAdditionalInfoOnParty(cciTrade.CSCacheOn, securityAsset.issuer, false);
                //}
                if ((securityAsset.IssuerReferenceSpecified) && (TradeCommonCustomCaptureInfos.PartyIssuer != securityAsset.IssuerReference.HRef))
                {
                    securityAsset.Issuer = cciTrade.DataDocument.AddParty(securityAsset.Issuer);
                    cciTrade.DataDocument.SetAdditionalInfoOnParty(cciTrade.CSCacheOn, securityAsset.Issuer, false);
                }
            }
            cciTrade.Ccis.IsToSynchronizeWithDocument = (securityAsset.OTCmlId > 0);

        }
        
        /// <summary>
        /// Dump a party (issuer) into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpIssuer_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            SQL_Actor sql_Actor = null;
            IParty party = null;
            bool isLoaded = false;
            bool isFound = false;

            if (StrFunc.IsFilled(pData))
            {
                #region Check in Database, if actor is a valid
                //20090618 PL Add multi search
                SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
                string search_actor = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_actor",
                                            typeof(System.String), IDTypeSearch.ToString());
                string[] aSearch_actor = search_actor.Split(";".ToCharArray());
                int searchCount = aSearch_actor.Length;
                for (int k = 0; k < searchCount; k++)
                {
                    try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearch_actor[k], true); }
                    catch { continue; }
                    //20090618 PL Change i < 2 to i < 3
                    for (int i = 0; i < 3; i++)
                    {
                        string dataToFind = pData;
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        else if (i == 2)//20090618 PL Newness
                            dataToFind = "%" + pData.Replace(" ", "%") + "%";
                        //
                        sql_Actor = new SQL_Actor(cciTrade.CSCacheOn, IDTypeSearch, dataToFind,
                            SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, CcisBase.User, CcisBase.SessionId);
                        sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.ISSUER });
                        sql_Actor.MaxRows = 2; //NB: Afin de retourner au max 2 lignes
                        //
                        isLoaded = sql_Actor.IsLoaded;
                        int rowsCount = sql_Actor.RowsCount;
                        isFound = isLoaded && (rowsCount == 1);
                        //20090618 PL Replace isFound by isLoaded for Break 
                        //if (isFound)
                        if (isLoaded)
                            break;
                    }
                    if (isLoaded)
                        break;
                }
                #endregion Check in Database, if actor is a valid Counterparty, Broker, ...
            }
            //
            string party_id = isFound ? sql_Actor.XmlId : pCci.NewValue;
            //
            #region Remove Last Party if is not use
            bool isLastInUSe = Ccis.IsValueUseInCciParty(pCci.LastValue);
            //
            // RD 20200921 [25246] l'Id de l'acteur est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
            string lastParty_id = XMLTools.GetXmlId(pCci.LastValue);
            if (null != pCci.LastSql_Table)
                lastParty_id = ((SQL_Actor)pCci.LastSql_Table).XmlId;
            //
            if (false == isLastInUSe)
                cciTrade.DataDocument.RemoveParty(lastParty_id);
            #endregion Remove Last Party if is not use
            //
            #region Add Party
            if (StrFunc.IsFilled(pData))
            {
                // 20090916 RD / partyId de la Party Issuer n'est pas renseigné
                if (isFound)
                    party = cciTrade.DataDocument.AddParty(sql_Actor);
                else
                    party = cciTrade.DataDocument.AddParty(party_id);
                //
                //securityAsset.issuer = party;
                //securityAsset.issuerSpecified = (null != party);
                securityAsset.IssuerReferenceSpecified = (null != party);
                if (securityAsset.IssuerReferenceSpecified)
                    securityAsset.IssuerReference = cciTrade.Product.Product.ProductBase.CreatePartyOrAccountReference(party.Id);

            }
            #endregion Add Party
            //
            if (isFound)
            {
                #region Party is found in Database
                pCci.NewValue = sql_Actor.Identifier;
                pCci.Sql_Table = sql_Actor;
                pCci.ErrorMsg = string.Empty;
                //
                Tools.SetParty(party, sql_Actor, cciTrade.DataDocument.IsEfsMLversionUpperThenVersion2);
                //
                #region Enrichir la Party "Issuer"
                // Enrichir la Party "Issuer" de l'Asset titre
                cciTrade.DataDocument.SetAdditionalInfoOnParty(cciTrade.CSCacheOn, party, false);
                //DataDocumentContainer.SetAdditionalInfoOnParty(cciTrade.CSCacheOn, party, false);
                #endregion
                #endregion Party is found in Database
            }
            else
            {
                #region Party is NOT found in Database
                pCci.ErrorMsg = string.Empty;
                pCci.Sql_Table = null;
                //					
                if (pCci.IsFilled)
                {
                    if (cciTrade.IsStEnvTemplate)
                    {
                        if (pCci.NewValue != Cst.FpML_EntityOfUserIdentifier)
                            pCci.ErrorMsg = Ressource.GetString("Msg_ActorNotFound");
                    }
                    else
                    {
                        if (isLoaded)
                            pCci.ErrorMsg = Ressource.GetString("Msg_ActorNotUnique");
                        else
                            pCci.ErrorMsg = Ressource.GetString("Msg_ActorNotFound");
                    }
                }
                //
                if (null != party)
                {
                    party.Id = party_id;
                    party.OTCmlId = 0;
                    party.PartyId = party_id;
                    party.PartyName = string.Empty;
                }
                #endregion Party is NOT found in Database
            }

        }
        /// <summary>
        /// 
        /// </summary>
        private void CreateInstance()
        {
            CciTools.CreateInstance(this, securityAsset, "CciEnum");
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {

            CciTools.SetCciContainer(this, "NewValue", string.Empty);
            CcisBase.SetNewValue(CciClientId(CciEnum.isNewAsset), "false");
            //
            if (null != cciDebtSecurity)
                cciDebtSecurity.Clear();

        }

        /// <summary>
        /// Recherche le titre qui matche avec {pData}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pData">Donnée en entrée</param>
        /// <param name="pIdI"></param>
        /// <param name="isFound">true s'il existe 1 et 1 seul tritre qui matche avec {pData}</param>
        /// <param name="isLoaded">true s'il existe au moins 1 titre qui latche avec {pData}</param>
        /// <returns></returns>
        /// FI 20121116 [18224] tuning Spheres® n'attaque plus la vu
        private static SQL_TradeDebtSecurity SearchDebtSecurity(string pCS, string pData, int pIdI, out bool isLoaded, out bool isFound)
        {
            SQL_TradeDebtSecurity sqlDebtSecurityAsset = null;
            isLoaded = false;
            isFound = false;

            //20090618 PL Add multi search
            SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
            string search_security = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_security",
                                        typeof(System.String), IDTypeSearch.ToString());
            string[] aSearch_security = search_security.Split(";".ToCharArray());
            int searchCount = aSearch_security.Length;
            for (int k = 0; k < searchCount; k++)
            {
                try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearch_security[k], true); }
                catch { continue; }

                //20090618 PL Change i < 2 to i < 3
                for (int i = 0; i < 3; i++)
                {
                    string dataToFind = pData.Replace("%", SQL_TableWithID.StringForPERCENT);
                    if (i == 1)
                        dataToFind = pData.Replace(" ", "%") + "%";
                    else if (i == 2)//20090618 PL Newness
                        dataToFind = "%" + pData.Replace(" ", "%") + "%";
                    //     
                    sqlDebtSecurityAsset = new SQL_TradeDebtSecurity(pCS, IDTypeSearch, dataToFind,
                        Cst.StatusEnvironment.REGULAR, SQL_Table.RestrictEnum.No, null, null)
                    {
                        //FI 20121116 [18224]  sqlDebtSecurityAsset.isUseView = false (meilleur for les perfs)
                        IsUseView = false,
                        IdIForAssetEnv_In = pIdI,
                        MaxRows = 2 //NB: Afin de retourner au max 2 lignes
                    };

                    //FI 20121116 [18224] Seule la colonne IDT est 
                    isLoaded = sqlDebtSecurityAsset.LoadTable(new string[] { "TRADE.IDT", "TRADE.IDENTIFIER" });
                    int rowsCount = sqlDebtSecurityAsset.RowsCount;
                    isFound = isLoaded && (rowsCount == 1);
                    //20090618 PL Replace isFound by isLoaded for Break 
                    //if (isFound)
                    if (isLoaded)
                        break;
                }
                if (isLoaded)
                    break;
            }

            return sqlDebtSecurityAsset;
        }








        #endregion


        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        ///FI 20120625 Add ICciPresentation implementation
        // EG 20190823 [FIXEDINCOME] Call cciDebtSecurity.DumpSpecific_ToGUI
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            //FI 20120622 pour l'instant sql_Table n'est pas renseigné donc spheres ne rentre pas dans pPage.SetOpenFormDebtSecurity
            CustomCaptureInfo cciSecurityId = Cci(CciSecurityAsset.CciEnum.securityId);
            if ((null != cciSecurityId) && (null != cciSecurityId.Sql_Table))
                pPage.SetOpenFormDebtSecurity(cciSecurityId);

            if (null != cciDebtSecurity)
                cciDebtSecurity.DumpSpecific_ToGUI(pPage);
        }

        #endregion
    }
    
}
