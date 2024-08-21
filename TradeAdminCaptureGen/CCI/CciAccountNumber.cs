#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.DynamicData;
using EfsML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciAccountNumber
    /// </summary>
    public class CciAccountNumber : IContainerCciFactory, IContainerCci, IContainerCciGetInfoButton
    {

        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("currency")]
            cashCurrency,
            [System.Xml.Serialization.XmlEnumAttribute("correspondant")]
            correspondant,
            [System.Xml.Serialization.XmlEnumAttribute("accountName")]
            accountName,
            [System.Xml.Serialization.XmlEnumAttribute("accountNumber")]
            accountNumber,
            [System.Xml.Serialization.XmlEnumAttribute("nostroAccountNumber")]
            nostroAccount,
            [System.Xml.Serialization.XmlEnumAttribute("journalCode")]
            journalCode,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        protected CciInvoiceSettlement m_CciInvoiceSettlement;
        protected IAccountNumber m_AccountNumber;
        protected string m_Prefix;
        protected TradeAdminCustomCaptureInfos m_Ccis;
        #endregion Members
        #region Accessors
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }
        #endregion Ccis
        #region AccountNumber
        public IAccountNumber AccountNumber
        {
            get { return m_AccountNumber; }
            set
            {
                m_AccountNumber = value;
                //InitializeSQLTable();
            }
        }
        #endregion AccountNumber
        #endregion Accessors
        #region Constructors
        public CciAccountNumber(CciInvoiceSettlement pCciInvoiceSettlement, string pPrefix, IAccountNumber pAccountNumber)
        {
            m_CciInvoiceSettlement = pCciInvoiceSettlement;
            m_Ccis = pCciInvoiceSettlement.Ccis;
            m_Prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            m_AccountNumber = pAccountNumber;
            //InitializeSQLTable();
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// Adding missing controls that are necessary for process intilialize
        /// </summary>
        /// FI 20170116  [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.accountName), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.accountNumber), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.nostroAccount), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.journalCode), true, TypeData.TypeDataEnum.@string);
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, m_AccountNumber);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        // 20090407 EG Replace RoleActor.CSS par RoleActor.ACCOUNTSERVICER
        public void Initialize_FromDocument()
        {
            string data;
            //string display;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);
            //CustomCaptureInfosBase.ProcessQueueEnum processQueue;

            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    //display = string.Empty;
                    isSetting = true;
                    isToValidate = false;
                    sql_Table = null;
                    //processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.cashCurrency:
                            #region CashCurrency
                            data = m_AccountNumber.Currency.Value;
                            #endregion CashCurrency
                            break;
                        case CciEnum.correspondant:
                            #region Correspondant
                            if (StrFunc.IsFilled(m_AccountNumber.Correspondant.OtcmlId) && (0 < m_AccountNumber.Correspondant.OTCmlId))
                            {
                                SQL_Actor sql_Actor = new SQL_Actor(m_CciInvoiceSettlement.CS, m_AccountNumber.Correspondant.OTCmlId);
                                // 20090407 EG
                                sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.ACCOUNTSERVICER });
                                if (sql_Actor.IsLoaded)
                                {
                                    sql_Actor.XmlId = m_AccountNumber.Correspondant.Id;
                                    data = sql_Actor.Identifier;
                                    sql_Table = (SQL_Table)sql_Actor;
                                    isToValidate = (0 == m_AccountNumber.Correspondant.OTCmlId);
                                }
                                else
                                {
                                    isToValidate = (m_AccountNumber.Correspondant.Id == TradeCommonCustomCaptureInfos.PartyUnknown);
                                    data = m_AccountNumber.Correspondant.Id;
                                }
                            }
                            #endregion Correspondant
                            break;
                        case CciEnum.accountName:
                            #region AccountName
                            data = m_AccountNumber.AccountName.Value;
                            #endregion AccountName
                            break;
                        case CciEnum.accountNumber:
                            #region AccountNumber
                            data = m_AccountNumber.AccountNumber.Value;
                            #endregion AccountNumber
                            break;
                        case CciEnum.nostroAccount:
                            #region Nostro Account
                            data = m_AccountNumber.NostroAccountNumber.Value;
                            #endregion Nostro Account
                            break;
                        case CciEnum.journalCode:
                            #region Journal Code
                            data = m_AccountNumber.JournalCode.Value;
                            #endregion Journal Code
                            break;
                        default:
                            #region Default
                            isSetting = false;
                            #endregion Default
                            break;
                    }
                    if (isSetting)
                    {
                        Ccis.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }
                }
            }
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    data = cci.NewValue;
                    isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.cashCurrency:
                            #region CashCurrency
                            m_AccountNumber.Currency.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion CashCurrency
                            break;
                        case CciEnum.correspondant:
                            #region Correspondant
                            DumpCorrespondant_ToDocument(cci, data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Correspondant
                            break;
                        case CciEnum.accountName:
                            #region AccountName
                            m_AccountNumber.AccountName.Value = data;
                            #endregion AccountName
                            break;
                        case CciEnum.accountNumber:
                            #region AccountNumber
                            m_AccountNumber.AccountNumber.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion AccountNumber
                            break;
                        case CciEnum.nostroAccount:
                            #region NostroAccount
                            m_AccountNumber.NostroAccountNumber.Value = data;
                            #endregion NostroAccount
                            break;
                        case CciEnum.journalCode:
                            #region Journal Code
                            m_AccountNumber.JournalCode.Value = data;
                            #endregion Journal Code
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion Dump_ToDocument
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region ProcessInitialize
        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);
                //
                switch (key)
                {
                    case CciEnum.cashCurrency:
                        #region CashCurrency
                        SetAndDisplayCashCurrency();
                        GetNostroAmount();
                        #endregion CashCurrency
                        break;
                    case CciEnum.correspondant:
                    case CciEnum.accountNumber:
                        #region Correspondant
                        GetNostroAmount();
                        #endregion Correspondant
                        break;

                }
            }
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.correspondant, pCci) && (null != pCci.Sql_Table))
                pCci.Display = pCci.Sql_Table.FirstRow["DISPLAYNAME"].ToString();
            if (IsCci(CciEnum.accountNumber, pCci))
                pCci.Display = AccountNumber.NostroAccountNumber.Value;
            if (IsCci(CciEnum.journalCode, pCci))
                pCci.Display = AccountNumber.JournalCode.Value;
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return m_Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return m_Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return m_Prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(m_Prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(m_Prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #region ITradeGetInfoButton Members
        #region SetButtonReferential
        // 20090420 EG Affectation Condition sur Aide Référentiel
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            if (IsCci(CciEnum.correspondant, pCci))
            {
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Referential = "ACTOR";
                pCo.Condition = RoleActor.ACCOUNTSERVICER.ToString();
            }
            else if (IsCci(CciEnum.accountNumber, pCci))
            {
                pCo.DynamicArgument = null;
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Referential = "ACCOUNTAT";
                pCo.SqlColumn = "ACCOUNTNUMBER";
                pCo.Condition = "INVOICINGSETTLEMENT";
                pCo.Fk = null;
                SQL_Table sql_Table = m_CciInvoiceSettlement.cciParty[0].Cci(CciTradeParty.CciEnum.actor).Sql_Table; ;
                if (null != sql_Table)
                {
                    SQL_Actor sql_Actor = (SQL_Actor)sql_Table;
                    pCo.Fk = sql_Actor.Id.ToString();
                }
                StringDynamicData idC = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "IDC", m_AccountNumber.Currency.Value);
                StringDynamicData idCorresp = new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDA_CORRESPONDENT", m_AccountNumber.Correspondant.OTCmlId.ToString());
                pCo.DynamicArgument = new string[2] { idC.Serialize(), idCorresp.Serialize() };
            }
        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonZoom
        #endregion ITradeGetInfoButton Members
        #endregion Interfaces
        #region Methods
        #region DumpCorrespondant_ToDocument
        // 20090407 EG Replace RoleActor.CSS par RoleActor.ACCOUNTSERVICER
        // EG 20121003 Ticket: 18167
        private void DumpCorrespondant_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            SQL_Actor sql_Actor = null;
            bool isLoaded = false;
            bool isFound = false;
            pCci.ErrorMsg = string.Empty;
            if (StrFunc.IsFilled(pData))
            {
                #region Check in Database, if actor is a valid
                //20100909 EG Add multi search
                SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
                string search_actor = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_actor",
                                            typeof(System.String), IDTypeSearch.ToString());
                string[] aSearch_actor = search_actor.Split(";".ToCharArray());
                int searchCount = aSearch_actor.Length;
                for (int k = 0; k < searchCount; k++)
                {
                    //20090618 PL Change i < 2 to i < 3
                    for (int i = 0; i < 3; i++)
                    {
                        string dataToFind = pData;
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        else if (i == 2)//20090618 PL Newness
                            dataToFind = "%" + pData.Replace(" ", "%") + "%";
                        //
                        sql_Actor = new SQL_Actor(m_CciInvoiceSettlement.CSCacheOn, IDTypeSearch, dataToFind,
                            SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, Ccis.User, Ccis.SessionId)
                        {
                            MaxRows = 2 //NB: Afin de retourner au max 2 lignes
                        };
                        sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.ACCOUNTSERVICER });
                        isLoaded = sql_Actor.IsLoaded;
                        int rowsCount = sql_Actor.RowsCount;
                        isFound = isLoaded && (rowsCount == 1);
                        if (isLoaded)
                            break;
                    }
                    if (isLoaded)
                        break;
                }
                if (isFound)
                {
                    pCci.NewValue = sql_Actor.Identifier;
                    pCci.Sql_Table = sql_Actor;
                    pCci.ErrorMsg = string.Empty;
                    AccountNumber.Correspondant.Value = sql_Actor.Identifier;
                    AccountNumber.Correspondant.Scheme = Cst.OTCml_ActorIdScheme;
                    AccountNumber.Correspondant.OTCmlId = sql_Actor.Id;
                }
                else
                {
                    pCci.ErrorMsg = string.Empty;
                    pCci.Sql_Table = null;
                    if (pCci.IsFilled || (pCci.IsEmpty && pCci.IsMandatory))
                    {
                        if (isLoaded)
                            pCci.ErrorMsg = Ressource.GetString("Msg_ActorNotUnique");
                        else
                            pCci.ErrorMsg = Ressource.GetString("Msg_ActorNotFound");
                    }
                    ResetAccountNumber();
                }
                #endregion Check in Database, if actor is a valid
            }
            else if (pCci.IsMandatory)
            {
                // EG 20121003 Ticket: 18167
                pCci.Sql_Table = null;
                pCci.ErrorMsg = Ressource.GetString("ISMANDATORY");
                ResetAccountNumber();
            }
        }
        #endregion DumpCorrespondant_ToDocument
        #region GetNostroAmount
        // 20090407 EG Replace RoleActor.CSS par RoleActor.ACCOUNTSERVICER
        // EG 20100909 Suite Visite HPC 20100907
        // EG 20121005 Dataset remplace DataReader (suite Ticket: 18167)
        // EG 20140826 Refactoring Lecture du 1er correspondant ACCOUNTSERVICE (0 < Rows.Count au lieu de 1 ==  Rows.Count)
        public void GetNostroAmount()
        {
            CustomCaptureInfo cciActorReceiver = m_CciInvoiceSettlement.cciParty[0].Cci(CciTradeParty.CciEnum.actor);
            CustomCaptureInfo cciActorCorrespondant = Cci(CciEnum.correspondant);
            CustomCaptureInfo cciAccountNumber = Cci(CciEnum.nostroAccount);
            CustomCaptureInfo cciCashcurrency = Cci(CciEnum.cashCurrency);
            if ((null != cciActorReceiver) && (null != cciCashcurrency))
            {
                DataParameters parameters = new DataParameters(new DataParameter[] { });
                SQL_Actor actorReceiver = (SQL_Actor)cciActorReceiver.Sql_Table;
                if (null != actorReceiver)
                {
                    if (((null != cciActorCorrespondant) && (cciActorCorrespondant.IsFilledValue || cciActorCorrespondant.HasError))
                        || ((null != cciAccountNumber) && cciAccountNumber.IsFilledValue))
                    {
                        #region Parameters
                        parameters.Add(DataParameter.GetParameter(m_CciInvoiceSettlement.CS, DataParameter.ParameterEnum.IDA), actorReceiver.Id);
                        parameters.Add(DataParameter.GetParameter(m_CciInvoiceSettlement.CS, DataParameter.ParameterEnum.IDC), AccountNumber.Currency.Value);
                        if ((null != cciActorCorrespondant))
                        {
                            SQL_Actor actorCorrespondant = (SQL_Actor)cciActorCorrespondant.Sql_Table;
                            if (null != actorCorrespondant)
                            {
                                parameters.Add(new DataParameter(m_CciInvoiceSettlement.CS, "IDA_CORRESPONDENT", DbType.Int32), actorCorrespondant.Id);
                            }
                        }
                        if ((null != cciAccountNumber) && cciAccountNumber.IsFilledValue)
                        {
                            string accountNumber = AccountNumber.AccountNumber.Value;
                            if (-1 == accountNumber.IndexOf("%"))
                                accountNumber += "%";

                            parameters.Add(new DataParameter(m_CciInvoiceSettlement.CS, "ACCOUNTNUMBER", DbType.String, SQLCst.UT_ACCOUNTNUMBER_LEN), accountNumber);
                        }
                        #endregion Parameters

                        ResetAccountNumber();

                        string SQLQuery = @"select acc.IDA_CORRESPONDENT, acc.IDC, acc.ACCOUNTNUMBER, acc.ACCOUNTNUMBERIDENT, acc.ACCOUNTNAME, acc.NOSTROACCOUNTNUMBER, acc.JOURNALCODE 
                        from dbo.ACCOUNTAT acc 
                        inner join dbo.ACTOR cor on (cor.IDA = acc.IDA_CORRESPONDENT)
                        where (acc.IDA = @IDA) and (acc.IDC = @IDC)" + Cst.CrLf;
                        if (parameters.Contains("IDA_CORRESPONDENT"))
                            SQLQuery += SQLCst.AND + "(acc.IDA_CORRESPONDENT = @IDA_CORRESPONDENT)";
                        if (parameters.Contains("ACCOUNTNUMBER"))
                            SQLQuery += SQLCst.AND + "(acc.ACCOUNTNUMBER like @ACCOUNTNUMBER)";

                        SQLQuery += "order by cor.IDENTIFIER" + Cst.CrLf;

                        QueryParameters queryParameters = new QueryParameters(m_CciInvoiceSettlement.CS, SQLQuery.ToString(), parameters);
                        DataSet ds = DataHelper.ExecuteDataset(CSTools.SetCacheOn(m_CciInvoiceSettlement.CS), CommandType.Text,
                queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                        if ((null !=ds) && (0 < ds.Tables[0].Rows.Count))
                        {
                            //DataRow dr = ds.Tables[0].Rows[0];
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                m_AccountNumber.Correspondant.OTCmlId = Convert.ToInt32(row["IDA_CORRESPONDENT"]);
                                if (null != cciActorCorrespondant)
                                {
                                    SQL_Actor sql_Actor = new SQL_Actor(m_CciInvoiceSettlement.CS, m_AccountNumber.Correspondant.OTCmlId);
                                    sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.ACCOUNTSERVICER });
                                    if (sql_Actor.IsLoaded)
                                    {
                                        cciActorCorrespondant.Sql_Table = (SQL_Actor)sql_Actor;
                                        m_AccountNumber.Correspondant.Value = sql_Actor.Identifier;
                                        m_AccountNumber.Currency.Value = row["IDC"].ToString();
                                        m_AccountNumber.AccountName.Value = row["ACCOUNTNAME"].ToString();
                                        m_AccountNumber.AccountNumber.Value = row["ACCOUNTNUMBER"].ToString();
                                        m_AccountNumber.NostroAccountNumber.Value = row["NOSTROACCOUNTNUMBER"].ToString();
                                        m_AccountNumber.JournalCode.Value = row["JOURNALCODE"].ToString();
                                        cciActorCorrespondant.ErrorMsg = string.Empty;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (((null != cciActorCorrespondant) && cciActorCorrespondant.IsEmptyValue) ||
                             ((null != cciAccountNumber) && cciAccountNumber.IsEmptyValue))
                    {
                        // EG 20121003 Ticket: 18167
                        ResetAccountNumber();
                    }
                }
                else
                {
                    // EG 20121003 Ticket: 18167
                    ResetAccountNumber();
                }
            }
            else
                ResetAccountNumber();

            SetAndDisplayNostroAccount();
        }
        #endregion GetNostroAmount
        #region InitializeSQLTable
        /*
        private void InitializeSQLTable()
        {
        }
        */
        #endregion InitializeSQLTable
        #region ResetAccountNumber
        public void ResetAccountNumber()
        {
            m_AccountNumber.AccountName.Value = string.Empty;
            m_AccountNumber.AccountNumber.Value = string.Empty;
            m_AccountNumber.NostroAccountNumber.Value = string.Empty;
            m_AccountNumber.Correspondant.Value = string.Empty;
            m_AccountNumber.Correspondant.Scheme = Cst.OTCml_ActorId;
            m_AccountNumber.Correspondant.OTCmlId = 0;
            m_AccountNumber.JournalCode.Value = string.Empty;
        }
        #endregion ResetAccountNumber

        #region SetAndDisplayCashCurrency
        public void SetAndDisplayCashCurrency()
        {
            string currency = m_AccountNumber.Currency.Value;
            Ccis.SetNewValue(m_CciInvoiceSettlement.CciClientId(CciInvoiceSettlement.CciEnum.cashAmount_currency), currency);
            Ccis.SetNewValue(m_CciInvoiceSettlement.CciClientId(CciInvoiceSettlement.CciEnum.bankChargesAmount_currency), currency);
            Ccis.SetNewValue(m_CciInvoiceSettlement.CciClientId(CciInvoiceSettlement.CciEnum.vatBankChargesAmount_currency), currency);
            Ccis.SetNewValue(m_CciInvoiceSettlement.CciClientId(CciInvoiceSettlement.CciEnum.fxGainOrLossAmount_currency), currency);
            Ccis.SetNewValue(m_CciInvoiceSettlement.CciClientId(CciInvoiceSettlement.CciEnum.netCashAmount_currency), currency);

            string lastValue = Ccis.GetLastValue(m_CciInvoiceSettlement.CciClientId(CciInvoiceSettlement.CciEnum.cashAmount_currency));
            if (StrFunc.IsEmpty(lastValue))
            {
                Ccis.SetNewValue(m_CciInvoiceSettlement.CciClientId(CciInvoiceSettlement.CciEnum.settlementAmount_currency), currency);
                Ccis.SetNewValue(m_CciInvoiceSettlement.CciClientId(CciInvoiceSettlement.CciEnum.unallocatedAmount_currency), currency);
            }
        }
        #endregion SetAndDisplayCashCurrency
        #region SetAndDisplayNostroAccount
        // EG 20100909 Suite Visite HPC 20100907
        // EG 20121003 Ticket: 18167
        public void SetAndDisplayNostroAccount()
        {
            Ccis.SetNewValue(CciClientId(CciEnum.cashCurrency), m_AccountNumber.Currency.Value);
            Ccis.SetNewValue(CciClientId(CciEnum.correspondant), m_AccountNumber.Correspondant.Value);
            Ccis.SetNewValue(CciClientId(CciEnum.accountName), m_AccountNumber.AccountName.Value);
            Ccis.SetNewValue(CciClientId(CciEnum.accountNumber), m_AccountNumber.AccountNumber.Value);
            Ccis.SetNewValue(CciClientId(CciEnum.nostroAccount), m_AccountNumber.NostroAccountNumber.Value);
            Ccis.SetNewValue(CciClientId(CciEnum.journalCode), m_AccountNumber.JournalCode.Value);
        }
        #endregion SetAndDisplayNostroAccount
        #endregion Methods
    }
}
