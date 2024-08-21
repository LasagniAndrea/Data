using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using System;
using System.Globalization;

// TODO FI 20130321 cette espace de nom doit sortir de SpheresIO
// Il faudra nécessairement le faire lorsque l'on faudra offrir une saisie de POSREQUEST sur une position 
// PM 20180219 [23824] Déplacé dans CommonIO à partir de SpheresIO (PosRequestCustomCaptureInfos.cs)
namespace EFS.PosRequestInformation
{

    /// <summary>
    /// Collection CCi chargée d'alimenter PosRequestInput
    /// </summary>
    public class PosRequestCustomCaptureInfos : CustomCaptureInfosBase
    {

        /// <summary>
        /// Représente le format d'affichage et de saisie préférentiel des échéances sur ASSET ETD
        /// </summary>
        private Cst.ETDMaturityInputFormatEnum _fmtETDMaturityInput;

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public PosRequestPositionInput PosRequestInput
        {
            get { return (PosRequestPositionInput)Obj; }
        }

        /// <summary>
        /// Obtient ou dédinit le format de saisie et d'affichage préférnetiel des échéances ETD
        /// </summary>
        public Cst.ETDMaturityInputFormatEnum FmtETDMaturityInput
        {
            get
            {
                return _fmtETDMaturityInput;
            }
            set
            {
                _fmtETDMaturityInput = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public CciPosRequestPosition CciPosRequest
        {
            get { return (CciPosRequestPosition)CciContainer; }
            set { CciContainer = value; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public PosRequestCustomCaptureInfos() :
            base()
        {
            _fmtETDMaturityInput = Cst.ETDMaturityInputFormatEnum.FIX;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pObj"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSessionAdmin"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        public PosRequestCustomCaptureInfos(string pCS, ICustomCaptureInfos pObj, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
            : base(pCS, pObj, pUser, pSessionId, pIsGetDefaultOnInitializeCci)
        {
            _fmtETDMaturityInput = Cst.ETDMaturityInputFormatEnum.FIX;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public override void InitializeCciContainer()
        {
            CciPosRequest = new CciPosRequestPosition(this);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class CciPosRequestPosition : IContainerCciFactory, IContainerCci
    {
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("requestType")]
            requestType,
            [System.Xml.Serialization.XmlEnumAttribute("requestMode")]
            requestMode,
            [System.Xml.Serialization.XmlEnumAttribute("clearingBusinessDate")]
            clearingBusinessDate,
            [System.Xml.Serialization.XmlEnumAttribute("qty")]
            qty,
            [System.Xml.Serialization.XmlEnumAttribute("actorDealer")]
            actorDealer,
            [System.Xml.Serialization.XmlEnumAttribute("bookDealer")]
            bookDealer,
            [System.Xml.Serialization.XmlEnumAttribute("actorClearer")]
            actorClearer,
            [System.Xml.Serialization.XmlEnumAttribute("bookClearer")]
            bookClearer,
            [System.Xml.Serialization.XmlEnumAttribute("actorEntityDealer")]
            actorEntityDealer,
            [System.Xml.Serialization.XmlEnumAttribute("notes")]
            notes,
            [System.Xml.Serialization.XmlEnumAttribute("isPartialExecutionAllowed")]
            isPartialExecutionAllowed,
            [System.Xml.Serialization.XmlEnumAttribute("isFeeCalculation")]
            isFeeCalculation,
            [System.Xml.Serialization.XmlEnumAttribute("isAbandonRemainingQty")]
            isAbandonRemainingQty,
            unknown
        }
        #endregion CciEnum

        #region members
        private readonly string _prefix;
        private readonly PosRequestCustomCaptureInfos _ccis;
        /// <summary>
        /// 
        /// </summary>
        private readonly IPosRequestPositionDocument _posRequestDocument;
        /// <summary>
        /// 
        /// </summary>
        private readonly CciFixInstrument _cciFixInstrument;
        #endregion members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public PosRequestCustomCaptureInfos Ccis => _ccis;

        /// <summary>
        /// Obtient posRequestInput
        /// </summary>
        public PosRequestPositionInput PosRequestInput
        {
            get { return _ccis.PosRequestInput; }
        }


        /// <summary>
        /// 
        /// </summary>
        public CciFixInstrument CciFixInstrument => _cciFixInstrument;


        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCcis"></param>
        public CciPosRequestPosition(PosRequestCustomCaptureInfos pCcis)
        {
            _ccis = pCcis;
            _prefix = string.Empty;

            _posRequestDocument = this.PosRequestInput.DataDocument;

            IFixInstrument fixInstrument = _posRequestDocument.Instrmt;
            _cciFixInstrument = new CciFixInstrument(this, "Instrmt", fixInstrument);

        }
        #endregion

        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
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
            return (pClientId_WithoutPrefix.StartsWith(_prefix));
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members

        #region IContainerCciFactory Membres

        void IContainerCciFactory.Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _posRequestDocument);

            _cciFixInstrument.Initialize_FromCci();
        }

        void IContainerCciFactory.AddCciSystem()
        {

            if (false == Ccis.Contains(CciClientId(CciEnum.actorEntityDealer)))
                Ccis.Add(new CustomCaptureInfo(Cst.TXT + CciClientId(CciEnum.actorEntityDealer), true, TypeData.TypeDataEnum.@string));

            if (false == Ccis.Contains(CciClientId(CciEnum.actorDealer)))
                Ccis.Add(new CustomCaptureInfo(Cst.TXT + CciClientId(CciEnum.actorDealer), true, TypeData.TypeDataEnum.@string));

            if (false == Ccis.Contains(CciClientId(CciEnum.actorClearer)))
                Ccis.Add(new CustomCaptureInfo(Cst.TXT + CciClientId(CciEnum.actorClearer), true, TypeData.TypeDataEnum.@string));

            _cciFixInstrument.AddCciSystem();
        }

        void IContainerCciFactory.Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        case CciEnum.requestMode:
                            data = _posRequestDocument.RequestMode.ToString();
                            break;

                        case CciEnum.requestType:
                            data = _posRequestDocument.RequestType.ToString();
                            break;

                        case CciEnum.clearingBusinessDate:
                            data = _posRequestDocument.ClearingBusinessDate.Value;
                            break;

                        case CciEnum.qty:
                            data = _posRequestDocument.Qty.Value;
                            break;

                        case CciEnum.actorDealer:
                            if (_posRequestDocument.ActorDealerSpecified && _posRequestDocument.ActorDealer.OTCmlId > 0)
                            {
                                sql_Table = (SQL_Table)LoadActor(CSCacheOn(), _posRequestDocument.ActorDealer.OTCmlId);
                                if (null != sql_Table)
                                    data = ((SQL_Actor)sql_Table).Identifier;
                            }
                            break;

                        case CciEnum.bookDealer:
                            if (_posRequestDocument.BookDealerSpecified && _posRequestDocument.BookDealer.OTCmlId > 0)
                            {
                                sql_Table = (SQL_Table)LoadBook(CSCacheOn(), _posRequestDocument.BookDealer.OTCmlId);
                                if (null != sql_Table)
                                    data = ((SQL_Book)sql_Table).Identifier;
                            }
                            break;

                        case CciEnum.actorClearer:
                            if (_posRequestDocument.ActorDealer.OTCmlId > 0)
                            {
                                sql_Table = (SQL_Table)LoadActor(CSCacheOn(), _posRequestDocument.ActorDealer.OTCmlId);
                                if (null != sql_Table)
                                    data = ((SQL_Actor)sql_Table).Identifier;
                            }
                            break;

                        case CciEnum.bookClearer:
                            if (_posRequestDocument.BookClearer.OTCmlId > 0)
                            {
                                sql_Table = (SQL_Table)LoadBook(CSCacheOn(), _posRequestDocument.BookClearer.OTCmlId);
                                if (null != sql_Table)
                                    data = ((SQL_Book)sql_Table).Identifier;
                            }
                            break;

                        case CciEnum.actorEntityDealer:
                            if (_posRequestDocument.ActorEntity.OTCmlId > 0)
                            {
                                sql_Table = (SQL_Table)LoadActor(CSCacheOn(), _posRequestDocument.ActorEntity.OTCmlId);
                                if (null != sql_Table)
                                    data = ((SQL_Actor)sql_Table).Identifier;
                            }
                            break;

                        case CciEnum.notes:
                            if (_posRequestDocument.NotesSpecified)
                                data = _posRequestDocument.Notes.Value;
                            break;

                        case CciEnum.isPartialExecutionAllowed:
                            data = _posRequestDocument.IsPartialExecutionAllowed.Value;
                            break;

                        case CciEnum.isFeeCalculation:
                            data = _posRequestDocument.IsFeeCalculation.Value;
                            break;

                        case CciEnum.isAbandonRemainingQty:
                            data = _posRequestDocument.IsAbandonRemainingQty.Value;
                            break;


                        default:
                            isSetting = false;
                            break;

                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
            _cciFixInstrument.Initialize_FromDocument();
        }

        void IContainerCciFactory.Dump_ToDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                //  
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset Variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    bool isSetting = true;
                    #endregion Reset Variables

                    switch (cciEnum)
                    {
                        case CciEnum.requestType:
                            DumpRequestType(cci);
                            break;
                        case CciEnum.requestMode:
                            DumpRequestMode(cci);
                            break;
                        case CciEnum.clearingBusinessDate:
                            DumpDtBusiness(cci);
                            break;
                        case CciEnum.qty:
                            DumpQty(cci);
                            break;
                        case CciEnum.actorEntityDealer:
                        case CciEnum.actorDealer:
                        case CciEnum.actorClearer:
                            DumpActor(cci);
                            break;
                        case CciEnum.bookDealer:
                        case CciEnum.bookClearer:
                            DumpBook(cci);
                            // afin de récupérer l'acteur à partir du book
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.notes:
                            DumpNotes(cci);
                            break;
                        case CciEnum.isPartialExecutionAllowed:
                            _posRequestDocument.IsPartialExecutionAllowed.Value = cci.NewValue;
                            break;
                        case CciEnum.isFeeCalculation:
                            _posRequestDocument.IsFeeCalculation.Value = cci.NewValue;
                            break;
                        case CciEnum.isAbandonRemainingQty:
                            _posRequestDocument.IsAbandonRemainingQty.Value = cci.NewValue;
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            _cciFixInstrument.Dump_ToDocument();
        }

        void IContainerCciFactory.ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                CciEnum key = CciEnum.unknown;
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    case CciEnum.bookDealer:
                        SearchActorbyBook(pCci);
                        SearchEntityBookDealer(pCci);
                        break;
                    case CciEnum.bookClearer:
                        SearchActorbyBook(pCci);
                        break;
                }
            }
            _cciFixInstrument.ProcessInitialize(pCci);

            Ccis.Finalize(pCci.ClientId_WithoutPrefix, CustomCaptureInfosBase.ProcessQueueEnum.None);

        }

        void IContainerCciFactory.ProcessExecute(CustomCaptureInfo pCci)
        {
            _cciFixInstrument.ProcessExecute(pCci);
        }

        void IContainerCciFactory.ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            _cciFixInstrument.ProcessExecuteAfterSynchronize(pCci);
        }

        bool IContainerCciFactory.IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return _cciFixInstrument.IsClientId_PayerOrReceiver(pCci);
        }

        void IContainerCciFactory.CleanUp()
        {
            _cciFixInstrument.CleanUp();
        }

        void IContainerCciFactory.RefreshCciEnabled()
        {
            _cciFixInstrument.RefreshCciEnabled();
        }

        void IContainerCciFactory.SetDisplay(CustomCaptureInfo pCci)
        {
            _cciFixInstrument.SetDisplay(pCci);
        }

        void IContainerCciFactory.Initialize_Document()
        {
            _cciFixInstrument.Initialize_Document();
        }

        #endregion

        #region Method

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void DumpRequestType(CustomCaptureInfo pCci)
        {
            pCci.ErrorMsg = string.Empty;
            _posRequestDocument.RequestType = Cst.PosRequestTypeEnum.None;
            if (StrFunc.IsFilled(pCci.NewValue))
            {
                Nullable<Cst.PosRequestTypeEnum> valueEnum = (Cst.PosRequestTypeEnum)ReflectionTools.EnumParse(_posRequestDocument.RequestType, pCci.NewValue);
                if (valueEnum.HasValue)
                    _posRequestDocument.RequestType = valueEnum.Value;
                else
                    pCci.ErrorMsg = CciTools.BuildCciErrMsg("RequestType value is not valid", pCci.NewValue);
            }
            else
            {
                pCci.ErrorMsg = CciTools.BuildCciErrMsg("RequestType value is empty", pCci.NewValue);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void DumpRequestMode(CustomCaptureInfo pCci)
        {
            pCci.ErrorMsg = string.Empty;
            _posRequestDocument.RequestMode = SettlSessIDEnum.EndOfDay;
            if (StrFunc.IsFilled(pCci.NewValue))
            {
                Nullable<SettlSessIDEnum> valueEnum = (SettlSessIDEnum)ReflectionTools.EnumParse(_posRequestDocument.RequestMode, pCci.NewValue);
                if (valueEnum.HasValue)
                    _posRequestDocument.RequestMode = valueEnum.Value;
                else
                    pCci.ErrorMsg = CciTools.BuildCciErrMsg("RequestMode value is not valid", pCci.NewValue);
            }
            else
            {
                pCci.ErrorMsg = CciTools.BuildCciErrMsg("RequestMode value is empty", pCci.NewValue);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void DumpDtBusiness(CustomCaptureInfo pCci)
        {
            pCci.ErrorMsg = string.Empty;
            _posRequestDocument.ClearingBusinessDate.DateValue = DateTime.MinValue;

            if (StrFunc.IsFilled(pCci.NewValue))
                _posRequestDocument.ClearingBusinessDate.Value = pCci.NewValue;
            else
                pCci.ErrorMsg = CciTools.BuildCciErrMsg("Clearing business date is empty", pCci.NewValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void DumpQty(CustomCaptureInfo pCci)
        {
            pCci.ErrorMsg = string.Empty;
            _posRequestDocument.Qty.Value = string.Empty;

            if (StrFunc.IsFilled(pCci.NewValue))
                _posRequestDocument.Qty.Value = pCci.NewValue;
            else
                pCci.ErrorMsg = CciTools.BuildCciErrMsg("Quantity is empty", pCci.NewValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void DumpActor(CustomCaptureInfo pCci)
        {

            RoleActor[] roleActor;
            if (IsCci(CciEnum.actorEntityDealer, pCci))
                roleActor = new RoleActor[] { RoleActor.ENTITY };
            else if (IsCci(CciEnum.actorDealer, pCci))
                roleActor = new RoleActor[] { RoleActor.COUNTERPARTY };
            else if (IsCci(CciEnum.actorClearer, pCci))
                roleActor = new RoleActor[] { RoleActor.CSS, RoleActor.CLEARER };
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pCci.ClientId_WithoutPrefix));

            SQL_Actor sql_Actor = null;
            bool isLoaded = false;
            bool isFound = false;

            if (StrFunc.IsFilled(pCci.NewValue))
                CciTools.IsActorValid(CSCacheOn(), pCci.NewValue, out sql_Actor, out isLoaded, out isFound, false, roleActor, Ccis.User, Ccis.SessionId);

            if (isFound)
            {
                pCci.NewValue = sql_Actor.Identifier;
                pCci.Sql_Table = sql_Actor;
                pCci.ErrorMsg = string.Empty;

                if (IsCci(CciEnum.actorEntityDealer, pCci))
                {
                    Tools.SetActorId(_posRequestDocument.ActorEntity, sql_Actor);
                }
                else if (IsCci(CciEnum.actorDealer, pCci))
                {
                    Tools.SetActorId(_posRequestDocument.ActorDealer, sql_Actor);
                }
                else if (IsCci(CciEnum.actorClearer, pCci))
                {
                    Tools.SetActorId(_posRequestDocument.ActorClearer, sql_Actor);
                }
            }
            else
            {
                pCci.ErrorMsg = string.Empty;
                pCci.Sql_Table = null;

                if (IsCci(CciEnum.actorEntityDealer, pCci))
                {
                    Tools.SetActorId(_posRequestDocument.ActorEntity, null);
                }
                else if (IsCci(CciEnum.actorDealer, pCci))
                {
                    Tools.SetActorId(_posRequestDocument.ActorDealer, null);
                }
                else if (IsCci(CciEnum.actorClearer, pCci))
                {
                    Tools.SetActorId(_posRequestDocument.ActorClearer, null);
                }

                if (pCci.IsFilled || (pCci.IsEmpty && pCci.IsMandatory))
                {
                    if (isLoaded)
                        pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorNotUnique"), pCci.NewValue);
                    else
                        pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorNotFound"), pCci.NewValue);
                }
            }

            if (IsCci(CciEnum.actorDealer, pCci))
            {
                _posRequestDocument.ActorDealerSpecified = (null != pCci.Sql_Table);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void DumpBook(CustomCaptureInfo pCci)
        {
            SQL_Book sql_book = null;
            bool isLoaded = false;
            bool isFound = false;
            int rowsCount = 0;

            int idA = 0;
            SQL_Actor sql_Actor;
            if (IsCci(CciEnum.bookDealer, pCci))
                sql_Actor = ((SQL_Actor)Ccis[CciClientId(CciEnum.actorDealer)].Sql_Table);
            else if (IsCci(CciEnum.bookClearer, pCci))
                sql_Actor = ((SQL_Actor)Ccis[CciClientId(CciEnum.actorClearer)].Sql_Table);
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pCci.ClientId_WithoutPrefix));

            if (StrFunc.IsFilled(pCci.NewValue))
            {
                if (null != sql_Actor)
                    idA = sql_Actor.Id;
                CciTools.IsBookValid(CSCacheOn(), pCci.NewValue, out sql_book, out isLoaded, out isFound, idA, Ccis.User, Ccis.SessionId);
            }

            pCci.ErrorMsg = string.Empty;
            if (isFound)
            {
                pCci.NewValue = sql_book.Identifier;
                pCci.Sql_Table = sql_book;

                if (!sql_book.IsEnabled)
                    pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_BookDisabled"), pCci.NewValue);

                if (IsCci(CciEnum.bookDealer, pCci))
                    Tools.SetBookId(_posRequestDocument.BookDealer, sql_book);
                else if (IsCci(CciEnum.bookClearer, pCci))
                    Tools.SetBookId(_posRequestDocument.BookClearer, sql_book);

            }
            else
            {
                pCci.ErrorMsg = string.Empty;
                pCci.Sql_Table = null;


                if (IsCci(CciEnum.bookDealer, pCci))
                    Tools.SetBookId(_posRequestDocument.BookDealer, null);
                else if (IsCci(CciEnum.bookClearer, pCci))
                    Tools.SetBookId(_posRequestDocument.BookClearer, null);

                if (pCci.IsFilled || (pCci.IsEmpty && pCci.IsMandatory))
                {
                    if (isLoaded && (rowsCount > 1))
                        pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_BookNotUnique"), pCci.NewValue);
                    else
                        pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_BookNotFound"), pCci.NewValue);
                }
            }

            if (IsCci(CciEnum.bookDealer, pCci))
            {
                _posRequestDocument.BookDealerSpecified = (null != pCci.Sql_Table);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void DumpNotes(CustomCaptureInfo pCci)
        {
            pCci.ErrorMsg = string.Empty;
            _posRequestDocument.NotesSpecified = StrFunc.IsFilled(pCci.NewValue);
            if (_posRequestDocument.NotesSpecified)
                _posRequestDocument.Notes.Value = pCci.NewValue;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        private static SQL_Actor LoadActor(string pCS, int pIdA)
        {
            SQL_Actor ret = null;
            SQL_Actor sql_Actor = null;
            if (pIdA > 0)
                sql_Actor = new SQL_Actor(pCS, pIdA);

            if ((null != sql_Actor) && sql_Actor.IsLoaded)
                ret = sql_Actor;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        private static SQL_Book LoadBook(string pCS, int pIdB)
        {
            SQL_Book ret = null;
            SQL_Book sql_Book = null;
            if (pIdB > 0)
                sql_Book = new SQL_Book(pCS, pIdB);

            if ((null != sql_Book) && sql_Book.IsLoaded)
                ret = sql_Book;

            return ret;
        }

        /// <summary>
        /// Initialise l'acteur associé au book s'il est non renseigné
        /// </summary>
        /// <param name="pCci">Représente book</param>
        private void SearchActorbyBook(CustomCaptureInfo pCci)
        {
            if (null != pCci.Sql_Table)
            {
                SQL_Actor sql_Actor;
                if (IsCci(CciEnum.bookDealer, pCci))
                    sql_Actor = Cci(CciEnum.actorDealer).Sql_Table as SQL_Actor;
                else
                    sql_Actor = Cci(CciEnum.actorClearer).Sql_Table as SQL_Actor;

                if (null == sql_Actor)
                {
                    sql_Actor = CciTools.SearchCounterPartyActorOfBook(CSCacheOn(), (SQL_Book)pCci.Sql_Table, Ccis.User, Ccis.SessionId);
                    if ((sql_Actor != null) && StrFunc.IsFilled(sql_Actor.Identifier))
                    {
                        if (IsCci(CciEnum.bookDealer, pCci))
                            Cci(CciEnum.actorDealer).NewValue = sql_Actor.Identifier;
                        else
                            Cci(CciEnum.actorClearer).NewValue = sql_Actor.Identifier;
                    }
                }
            }
        }

        /// <summary>
        /// Initialise l'acteur entité associé au book s'il est non renseigné
        /// </summary>
        /// <param name="pCci"></param>
        private void SearchEntityBookDealer(CustomCaptureInfo pCci)
        {
            if (!(Cci(CciEnum.actorEntityDealer).Sql_Table is SQL_Actor))
            {
                SQL_Book sql_book = null;
                if (IsCci(CciEnum.bookDealer, pCci))
                    sql_book = Cci(CciEnum.bookDealer).Sql_Table as SQL_Book;

                if (null != sql_book)
                {
                    int idAEntity = EFS.Book.BookTools.GetEntityBook(CSCacheOn(), sql_book.Id);
                    if (idAEntity > 0)
                    {
                        SQL_Entity sqlEntity = new SQL_Entity(CSCacheOn(), idAEntity);
                        if (sqlEntity.IsLoaded)
                            Cci(CciEnum.actorEntityDealer).NewValue = sqlEntity.Identifier;
                    }
                }
            }
        }

        /// <summary>
        /// Retourne la connexion string cache true
        /// </summary>
        public string CSCacheOn()
        {
            return CSTools.SetCacheOn(Ccis.CS);
        }

        #endregion
    }

    /// <summary>
    /// Pilote l'alimentation d'un CciFixInstrument à partir d'une collection de ccis
    /// </summary>
    /// TODO FI 20130325 [] il faudrait essayer de crééer une classe de base 
    /// entre EFS.PosRequestInformation.CciFixInstrument et EFS.TradeInformation.CciFixInstrument
    public class CciFixInstrument : IContainerCciFactory, IContainerCci
    {
        #region Enums
        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            Exch,
            ID,
            MMY,
            PutCall,
            StrkPx,
            Sym,
            ExchangeSymbol,
            unknown,
        }

        #endregion Enums

        #region Members
        /// <summary>
        /// Poiteur pour accéder à la collection Cci
        /// </summary>
        private readonly PosRequestCustomCaptureInfos _ccis;
        /// <summary>
        ///pointeur pour accéder aux éléments du posrequest
        /// </summary>
        private readonly CciPosRequestPosition _cciPosRequest;

        /// <summary>
        /// 
        /// </summary>
        private readonly string _prefix;
        /// <summary>
        /// 
        /// </summary>
        private readonly FixInstrumentContainer _fixInstrument;

        /// <summary>
        ///  
        /// </summary>
        readonly Nullable<CfiCodeCategoryEnum> _category;
        /// <summary>
        /// True si exchangeTradedDerivative
        /// </summary>
        private readonly bool _isETD;
        /// <summary>
        /// True si equitySecurityTransaction
        /// </summary>
        private readonly bool _isEST;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public PosRequestCustomCaptureInfos Ccis => _ccis;

        /// <summary>
        /// 
        /// </summary>
        public CciPosRequestPosition CciPosRequest => _cciPosRequest;

        /// <summary>
        /// 
        /// </summary>
        public string Prefix
        {
            get { return _prefix; }
        }


        /// <summary>
        /// 
        /// </summary>
        public FixInstrumentContainer FixInstrument => _fixInstrument;


        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciPosRequest"></param>
        /// <param name="pIsETD"></param>
        /// <param name="pIsEST"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pFixInstrument"></param>
        public CciFixInstrument(CciPosRequestPosition pCciPosRequest, string pPrefix, IFixInstrument pFixInstrument)
        {
            _cciPosRequest = pCciPosRequest;
            _ccis = _cciPosRequest.Ccis;

            pCciPosRequest.PosRequestInput.GetMainInstrumentInfo(out _isETD, out _isEST, out _category);
            //
            _fixInstrument = new FixInstrumentContainer(pFixInstrument);
            //
            _prefix = pPrefix;
            //
            if (StrFunc.IsFilled(_prefix))
                _prefix += '_';
        }
        #endregion Constructors

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        public void Dump_ToDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                //  
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset Variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    #endregion Reset Variables

                    switch (cciEnum)
                    {
                        #region Security Exchange
                        case CciEnum.Exch:
                            cci.ErrorMsg = string.Empty;
                            cci.Sql_Table = null;
                            FixInstrument.SecurityExchange = null;
                            if (StrFunc.IsFilled(data))
                            {
                                SQL_Market sqlMarket = new SQL_Market(CciPosRequest.CSCacheOn(), SQL_TableWithID.IDType.FIXML_SecurityExchange, data, 
                                    SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, Ccis.User, Ccis.SessionId);
                                if (sqlMarket.IsLoaded)
                                {
                                    cci.Sql_Table = sqlMarket;
                                    FixInstrument.SecurityExchange = sqlMarket.FIXML_SecurityExchange;
                                }
                                else
                                {
                                    cci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_MarketNotFound"), data);
                                }
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Security Exchange
                        #region Security ID
                        case CciEnum.ID:
                            cci.Sql_Table = null;
                            if (cci.IsFilledValue)
                            {
                                if (_isETD)
                                    DumpFixInstrument_ToDocument_ETD(cci, data);
                                else if (_isEST)
                                    DumpFixInstrument_ToDocument_Equity(cci, data);
                            }
                            else if (cci.IsEmptyValue && cci.IsLastInputByUser)
                            {
                                //on supprime toutes les infos de l'asset lorsque l'utilisateur efface volontairement ID 
                                ClearFixInstrument(false);
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Security ID
                        #region Maturity Month Year
                        case CciEnum.MMY:
                            DumpMaturityMonthYear_ToDocument(cci, data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;

                            break;
                        #endregion Maturity Month Year
                        #region Put Or Call
                        case CciEnum.PutCall:
                            FixInstrument.PutOrCallSpecified = cci.IsFilledValue;
                            if (FixInstrument.PutOrCallSpecified)
                            {
                                PutOrCallEnum putOrCallEnum = (PutOrCallEnum)ReflectionTools.EnumParse(FixInstrument.PutOrCall, data);
                                FixInstrument.PutOrCall = putOrCallEnum;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Put Or Call
                        #region Strike Price
                        case CciEnum.StrkPx:
                            FixInstrument.StrikePriceSpecified = cci.IsFilledValue;
                            if (FixInstrument.StrikePriceSpecified)
                                FixInstrument.StrikePrice.Value = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Strike Price
                        #region Contract Symbol
                        case CciEnum.Sym:
                            cci.ErrorMsg = string.Empty;
                            cci.Sql_Table = null;
                            FixInstrument.Symbol = null;
                            //
                            if (StrFunc.IsFilled(data))
                            {
                                //FI 20100429 use LoadSqlMarketFromFixInstrument
                                int idM = 0;
                                SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(CciPosRequest.CSCacheOn(), null,
                                    this.FixInstrument, SQL_Table.ScanDataDtEnabledEnum.No);
                                if (null != sqlMarket)
                                    idM = sqlMarket.Id;
                                //
                                SQL_DerivativeContract derivativeContract = new SQL_DerivativeContract(CciPosRequest.CSCacheOn(), data, idM, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                if (derivativeContract.IsLoaded)
                                {
                                    cci.Sql_Table = derivativeContract;
                                    FixInstrument.Symbol = derivativeContract.Identifier;
                                }
                                else
                                    cci.ErrorMsg = Ressource.GetString("Msg_DerivativeContractNotFound");
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Contract Symbol

                        #region  Asset Exchange Symbol
                        case CciEnum.ExchangeSymbol:
                            FixInstrument.ExchangeSymbol = data;
                            break;
                        #endregion Asset Exchange Symbol
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion default
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fixInstrument);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        public void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region Security Exchange
                        case CciEnum.Exch:
                            data = FixInstrument.SecurityExchange;
                            sql_Table = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciPosRequest.CSCacheOn(), null, 
                                _fixInstrument, SQL_Table.ScanDataDtEnabledEnum.No);
                            break;
                        #endregion Security Exchange

                        #region Security ID
                        case CciEnum.ID:
                            sql_Table = null;
                            data = string.Empty;
                            int idAsset = IntFunc.IntValue(FixInstrument.SecurityId);
                            if (idAsset > 0)
                            {
                                SQL_AssetBase sql_Asset = null;
                                if (_isETD)
                                    sql_Asset = new SQL_AssetETD(_cciPosRequest.CSCacheOn(), idAsset);
                                else if (_isEST)
                                    sql_Asset = new SQL_AssetEquity(_cciPosRequest.CSCacheOn(), idAsset);

                                if (sql_Asset.IsLoaded && (sql_Asset.RowsCount == 1))
                                {
                                    sql_Table = sql_Asset;
                                    data = sql_Asset.Identifier;
                                }
                            }
                            break;
                        #endregion Security ID
                        #region Maturity Month Year
                        case CciEnum.MMY:
                            data = FixInstrument.MaturityMonthYear;
                            break;
                        #endregion Maturity Month Year
                        #region Put Or Call
                        case CciEnum.PutCall:
                            if (FixInstrument.PutOrCallSpecified)
                                data = ReflectionTools.ConvertEnumToString<PutOrCallEnum>(FixInstrument.PutOrCall);
                            break;
                        #endregion Put Or Call
                        #region Strike Price
                        case CciEnum.StrkPx:
                            if (true == FixInstrument.StrikePriceSpecified)
                                data = FixInstrument.StrikePrice.Value;
                            break;
                        #endregion Strike Price
                        #region Contract Symbol
                        case CciEnum.Sym:
                            data = FixInstrument.Symbol;
                            // FI 20121004 [18172]
                            sql_Table = ExchangeTradedDerivativeTools.LoadSqlDerivativeContractFromFixInstrument(_cciPosRequest.CSCacheOn(), null, _fixInstrument);
                            break;
                        #endregion Contract Symbol

                        #region  Asset Exchange Symbol
                        case CciEnum.ExchangeSymbol:
                            data = FixInstrument.ExchangeSymbol;
                            break;
                        #endregion Asset Exchange Symbol
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            //
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                CciEnum key = CciEnum.unknown;
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    case CciEnum.Exch:
                        Ccis.Set(CciClientId(CciEnum.ID), "lastValue", "*");
                        break;

                    case CciEnum.ID:
                        if ((null != pCci.Sql_Table) || (pCci.IsEmptyValue && pCci.IsLastInputByUser))
                        {
                            //Spheres® rentre ici si 
                            // - asset renseigné et valise oi
                            // - asset mis à blanc volontairement par l'utilisateur
                            InitializeFromSecurityID();
                        }
                        break;
                }

                SearchAsset(pCci);

            }
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
        public void RefreshCciEnabled()
        {
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
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {

            if (IsCci(CciEnum.ID, pCci))
            {
                if (null != pCci.Sql_Table)
                {
                    string display = string.Empty;
                    try
                    {
                        if (_isETD)
                        {
                            SQL_AssetETD SqlAsset = (SQL_AssetETD)pCci.Sql_Table;
                            display = SqlAsset.DisplayName;
                            display += "  -  ";
                            bool isOption = StrFunc.IsFilled(SqlAsset.PutCall);
                            if (isOption)
                                display += SqlAsset.PutCall_EnglishString;
                            if (StrFunc.IsFilled(SqlAsset.Maturity_MaturityMonthYear))
                            {
                                display += " " + SqlAsset.Maturity_MaturityMonthYear;
                                if (DtFunc.IsDateTimeFilled(SqlAsset.Maturity_MaturityDate))
                                    display += " (" + DtFunc.DateTimeToString(SqlAsset.Maturity_MaturityDate, DtFunc.FmtShortDate) + ")";
                            }
                            if (isOption)
                                // FI 20190520 [XXXXX] usage de SqlAsset.StrikePrice.ToString(NumberFormatInfo.InvariantInfo) pour ne pas perdre des decimales
                                display += " " + StrFunc.FmtDecimalToCurrentCulture(SqlAsset.StrikePrice.ToString(NumberFormatInfo.InvariantInfo));
                            if (StrFunc.IsFilled(SqlAsset.ISINCode))
                                display += "  -  ISIN:" + SqlAsset.ISINCode;
                        }
                        else if (_isEST)
                        {
                            SQL_AssetEquity SqlAsset = (SQL_AssetEquity)pCci.Sql_Table;
                            display = SqlAsset.DisplayName;
                        }
                    }
                    catch { }
                    pCci.Display = display;
                }
            }
            else if (IsCci(CciEnum.MMY, pCci))
            {
                if (null != pCci.Sql_Table)
                {
                    SQL_Maturity sqlMaturity = (SQL_Maturity)pCci.Sql_Table;
                    pCci.Display = DtFunc.DateTimeToStringDateISO(sqlMaturity.MaturityDate);
                }
            }
            else if (IsCci(CciEnum.Exch, pCci))
            {
                if (null != pCci.Sql_Table)
                {

                    SQL_Market sqlMarket = (SQL_Market)pCci.Sql_Table;
                    string display = sqlMarket.DisplayName;
                    string tmp = Convert.ToString(sqlMarket.GetFirstRowColumnValue("URI"));
                    if (StrFunc.IsFilled(tmp))
                    {
                        string href = (tmp.StartsWith(@"http") ? string.Empty : @"http://") + tmp;
                        display += Cst.HTMLSpace2 + Cst.HTMLSpace2 + @"<a href=""" + href + @""" target=""_blank"" tabindex=""-1"" style=""color:gainsboro;font-size:xx-small"">" + tmp + @"</a>";
                    }
                    pCci.Display = display;
                }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "isEnabled", pIsEnabled);
        }

        #endregion IContainerCciFactory Members

        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return Prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(Prefix.Length);
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
            return (pClientId_WithoutPrefix.StartsWith(Prefix));
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsClearMarket"></param>
        /// FI 20131126 [19271] add MaturityDate
        private void ClearFixInstrument(bool pIsClearMarket)
        {
            //SecurityID
            FixInstrument.SetSecurityID(null, null);

            //Product
            FixInstrument.FixProductSpecified = false;
            //Exchange
            if (pIsClearMarket)
            {
                FixInstrument.SecurityExchange = null;
            }
            //Contract
            FixInstrument.Symbol = null;
            //Maturity
            FixInstrument.MaturityMonthYear = null;
            //StrikePrice
            FixInstrument.StrikePriceSpecified = false;
            FixInstrument.StrikePrice.DecValue = 0;
            //PutOrCall
            FixInstrument.PutOrCallSpecified = false;
            FixInstrument.PutOrCall = PutOrCallEnum.Call;
            //Attribute
            FixInstrument.OptAttribute = string.Empty;
            //ISINCode
            FixInstrument.ISINCode = string.Empty;
            //RICCode
            FixInstrument.RICCode = string.Empty;
            //BBGCode
            FixInstrument.BBGCode = string.Empty;
            //NSINCode
            FixInstrument.NSINCode = string.Empty;
            // CFICode
            FixInstrument.CFICode = string.Empty;
            // Issuer
            FixInstrument.Issuer = string.Empty;
            FixInstrument.IssuerSpecified = false;
            // IssueDate
            FixInstrument.IssueDate = new EFS_Date();
            FixInstrument.IssueDateSpecified = false;

            FixInstrument.MaturityDate = new EFS_Date();
            FixInstrument.MaturityDateSpecified = false;

            // CountryOfIssue
            FixInstrument.CountryOfIssue = string.Empty;
            FixInstrument.CountryOfIssueSpecified = false;
            // StateOrProvinceOfIssue
            FixInstrument.StateOrProvinceOfIssue = string.Empty;
            FixInstrument.StateOrProvinceOfIssueSpecified = false;
            // LocaleOfIssue
            FixInstrument.LocaleOfIssue = string.Empty;
            FixInstrument.LocaleOfIssueSpecified = false;
            //Asset Exchange Symbol
            FixInstrument.ExchangeSymbol = string.Empty;
        }

        /// <summary>
        /// Initialisation des ccis à partir des éléments de fixInstrument
        /// </summary>
        private void InitializeFromSecurityID()
        {

            //Exchange
            Ccis.SetNewValue(CciClientId(CciEnum.Exch), FixInstrument.SecurityExchange);
            //Contract
            Ccis.SetNewValue(CciClientId(CciEnum.Sym), FixInstrument.Symbol);
            //Maturity
            Ccis.SetNewValue(CciClientId(CciEnum.MMY), FixInstrument.MaturityMonthYear);
            //
            if (IsETDOption())
            {
                string data = string.Empty;
                //Strike
                if (FixInstrument.StrikePriceSpecified)
                    data = FixInstrument.StrikePrice.Value;
                Ccis.SetNewValue(CciClientId(CciEnum.StrkPx), data);
                //PutCall
                data = string.Empty;
                if (FixInstrument.PutOrCallSpecified)
                    data = ReflectionTools.ConvertEnumToString<PutOrCallEnum>(FixInstrument.PutOrCall);
                Ccis.SetNewValue(CciClientId(CciEnum.PutCall), data);
            }
        }

        /// <summary>
        /// Dump a Fix Instrument into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        private void DumpFixInstrument_ToDocument_ETD(CustomCaptureInfo pCci, string pData)
        {
            bool isLoaded = false;
            bool isFound = false;
            pCci.ErrorMsg = string.Empty;
            pCci.Sql_Table = null;

            SQL_AssetETD sqlAssetETD = null;

            if (StrFunc.IsFilled(pData))
            {
                int idI = CciPosRequest.PosRequestInput.SqlInstrument.IdI;
                //
                int idM = 0;
                SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciPosRequest.CSCacheOn(), null,
                    _fixInstrument, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (null != sqlMarket)
                    idM = sqlMarket.Id;
                //
                // FI 20121004 [18172] usage de la méthode ExchangeTradedDerivativeTools.LoadSqlDerivativeContract
                int idDC = 0;
                SQL_DerivativeContract sqlDerivativeContract = ExchangeTradedDerivativeTools.LoadSqlDerivativeContract(_cciPosRequest.CSCacheOn(), _fixInstrument.SecurityExchange, _fixInstrument.Symbol, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (null != sqlDerivativeContract)
                    idDC = sqlDerivativeContract.Id;
                //
                decimal strikePrice = decimal.Zero;
                if (_fixInstrument.StrikePriceSpecified)
                    strikePrice = _fixInstrument.StrikePrice.DecValue;
                //
                Nullable<PutOrCallEnum> putCall = null;
                if (_fixInstrument.PutOrCallSpecified)
                    putCall = _fixInstrument.PutOrCall;
                //
                string maturityMonthYear = _fixInstrument.MaturityMonthYear;
                //
                SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
                string searchAsset = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_assetETD", typeof(string), IDTypeSearch.ToString());
                string[] aSearchAsset = searchAsset.Split(";".ToCharArray());
                int searchCount = aSearchAsset.Length;
                for (int j = 0; j < searchCount; j += 1)
                {
                    try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearchAsset[j], true); }
                    catch { continue; }

                    for (int i = 0; i < 3; i++)
                    {
                        //string cs = CSTools.SetTimeOut(cciTrade.CSCacheOn, 60);
                        string dataToFind = pData.Replace("%", SQL_TableWithID.StringForPERCENT);
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        else if (i == 2)
                            dataToFind = "%" + pData.Replace(" ", "%") + "%";
                        //
                        //FI 20120523 Spheres® effectue un 1er passage où il ne considère que l'identifiant saisi, le marché et l'instrument   
                        //Cela permet de gérer le cas où idDC,putCall,strikePrice, maturityMonthYear sont déjà renseignés avec les infos d'un asset précédent
                        //Il ne faut pas tenir compte des éléments présents dans idDC,putCall,strikePrice, maturityMonthYear qui correspondent aux valeurs de l'asset précédent
                        sqlAssetETD = new SQL_AssetETD(_cciPosRequest.CSCacheOn(), IDTypeSearch, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes)
                        {
                            IdM_In = idM,
                            IdI_In = idI,
                            MaxRows = 2
                        };
                        isLoaded = sqlAssetETD.IsLoaded;
                        isFound = isLoaded && (1 == sqlAssetETD.RowsCount);

                        // FI 20121123 add isLoaded
                        //if (false == isFound)
                        if ((false == isFound) && isLoaded)
                        {
                            // nouvelle recherche plus fine 
                            sqlAssetETD = new SQL_AssetETD(_cciPosRequest.CSCacheOn(), IDTypeSearch, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes)
                            {
                                IdI_In = idI,
                                IdM_In = idM,
                                IdDC_In = idDC,
                                PutCall_In = putCall,
                                Strike_In = strikePrice,
                                MaturityMonthYear_In = maturityMonthYear,

                                MaxRows = 2 //NB: Afin de retourner au max 2 lignes (s'ignifiant qu'il y en a plus d'une)
                            };

                            isLoaded = sqlAssetETD.IsLoaded;
                            isFound = isLoaded && (1 == sqlAssetETD.RowsCount);
                        }
                        //
                        if (isLoaded)
                            break;
                    }
                    if (isLoaded)
                        break;
                }
            }
            //
            if (isFound)
            {
                #region isFound
                pCci.NewValue = sqlAssetETD.Identifier;
                pCci.Sql_Table = sqlAssetETD;
                pCci.ErrorMsg = string.Empty;
                #endregion isFound
            }
            else
            {
                if (pCci.IsFilled)
                {
                    if (isLoaded)
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetETDNotUnique");
                    else
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetETDNotFound");
                }
            }
            //
            if (null != pCci.Sql_Table)
            {
                SQL_AssetETD sql_AssetETD = (SQL_AssetETD)pCci.Sql_Table;
                ExchangeTradedDerivativeTools.SetFixInstrumentFromETDAsset(_cciPosRequest.CSCacheOn(), null, sql_AssetETD,
                   GetETDCategory(), FixInstrument, null);
            }
        }

        /// <summary>
        /// Dump a Fix Instrument into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        private void DumpFixInstrument_ToDocument_Equity(CustomCaptureInfo pCci, string pData)
        {
            bool isLoaded = false;
            bool isFound = false;
            pCci.ErrorMsg = string.Empty;
            pCci.Sql_Table = null;

            SQL_AssetEquity sqlAsset = null;

            if (StrFunc.IsFilled(pData))
            {
                int idI = CciPosRequest.PosRequestInput.SqlInstrument.IdI;
                //
                int idM = 0;
                SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciPosRequest.CSCacheOn(), null,
                    _fixInstrument, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (null != sqlMarket)
                    idM = sqlMarket.Id;
                //
                SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
                string searchAsset = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_equitySecurity", typeof(string), IDTypeSearch.ToString());
                string[] aSearchAsset = searchAsset.Split(";".ToCharArray());
                int searchCount = aSearchAsset.Length;
                for (int j = 0; j < searchCount; j += 1)
                {
                    try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearchAsset[j], true); }
                    catch { continue; }

                    for (int i = 0; i < 3; i++)
                    {
                        //string cs = CSTools.SetTimeOut(cciTrade.CSCacheOn, 60);
                        string dataToFind = pData.Replace("%", SQL_TableWithID.StringForPERCENT);
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        else if (i == 2)
                            dataToFind = "%" + pData.Replace(" ", "%") + "%";
                        //     
                        sqlAsset = new SQL_AssetEquity(_cciPosRequest.CSCacheOn(), IDTypeSearch, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes)
                        {
                            IdM_In = idM,
                            IdI_In = idI,

                            MaxRows = 2 //NB: Afin de retourner au max 2 lignes (s'ignifiant qu'il y en a plus d'une)
                        };

                        isLoaded = sqlAsset.IsLoaded;
                        isFound = isLoaded && (1 == sqlAsset.RowsCount);
                        if (isLoaded)
                            break;
                    }
                    if (isLoaded)
                        break;
                }
            }
            //
            if (isFound)
            {
                #region isFound
                pCci.NewValue = sqlAsset.Identifier;
                pCci.Sql_Table = sqlAsset;
                pCci.ErrorMsg = string.Empty;
                #endregion isFound
            }
            else
            {
                //	
                if (pCci.IsFilled)
                {
                    ClearFixInstrument(false);
                    InitializeFromSecurityID();
                    if (isLoaded)
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetEquityNotUnique");
                    else
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetEquityNotFound");
                }
            }
            //
            if (null != pCci.Sql_Table)
            {
                SQL_AssetEquity sql_AssetEquity = (SQL_AssetEquity)pCci.Sql_Table;
                EquitySecurityTransactionTools.SetFixInstrumentFromEquityAsset(_cciPosRequest.CSCacheOn(), sql_AssetEquity, null, null, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        /// FI 20100402 [16922] Interprétation des echéances abrégées
        private void DumpMaturityMonthYear_ToDocument(CustomCaptureInfo pCci, string pData)
        {

            CustomCaptureInfo cci = pCci;
            cci.Sql_Table = null;
            
            FixInstrument.SetMaturityMonthYear(CciPosRequest.CSCacheOn(), Ccis.FmtETDMaturityInput, pData);
            /* FI 20220601 [XXXXX] Mise en commentaire
            if (StrFunc.IsFilled(fixInstrument.fixInstrument.MaturityMonthYear))
            {
                SQL_Maturity sqlMaturity = new SQL_Maturity(ccis.CS, pData, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (null != Cci(CciEnum.Sym).Sql_Table)
                    sqlMaturity.idMaturityRuleIn = ((SQL_DerivativeContract)(Cci(CciEnum.Sym).Sql_Table)).IdMaturityRule;
                //
                if (sqlMaturity.IsLoaded)
                {
                    cci.Sql_Table = sqlMaturity;
                    fixInstrument.fixInstrument.MaturityMonthYear = sqlMaturity.Identifier;
                }
            }
            */
            
            //Spheres met à jour newValue parce puisque de la donnée saisie a peut être interprétée (ex Z15 pour 201512)
            //Dans ce cas on veut que le cci soit en phase avec la valeur présente dans fixInstrument.MaturityMonthYear
            pCci.NewValue = FixInstrument.FixInstrument.MaturityMonthYear;
        }

        /// <summary>
        /// Affecte newValue des ccis gérés par ce CciContainer avec string.Empty
        /// </summary>
        public void Clear()
        {
            //isLastInputByUser = true afin de bien supprimer toutes les infos de l'asset
            //Voir le Dump associé à CciEnum.ID
            Cci(CciEnum.ID).IsLastInputByUser = true;
            //
            CciTools.SetCciContainer(this, "CciEnum", "newValue", string.Empty);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsETDOption()
        {
            return _category.HasValue & (_category.Value == CfiCodeCategoryEnum.Option);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private CfiCodeCategoryEnum GetETDCategory()
        {
            CfiCodeCategoryEnum ret;
            if (IsETDOption())
                ret = CfiCodeCategoryEnum.Option;
            else
                ret = CfiCodeCategoryEnum.Future;
            return ret;
        }

        /// <summary>
        /// Recherche de l'asset suite à une modification de la donnée dans {pCci}
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        private void SearchAsset(CustomCaptureInfo pCci)
        {
            //Recherche de l'asset en fonction des éléments de la série, dès qu'un asset est trouvé il est chargé
            //En mode Importation, ne charger l'asset que si toutes les infos sont saisies
            bool isLoadAsset = Cst.Capture.IsModeInput(Ccis.CaptureMode);
            if (Ccis.IsModeIO)
                isLoadAsset &= _fixInstrument.IsAssetInfoFilled(_isETD, IsETDOption());

            if (isLoadAsset)
            {
                if (_isETD)
                {
                    isLoadAsset &= (IsCci(CciEnum.Sym, pCci) || IsCci(CciEnum.PutCall, pCci) || IsCci(CciEnum.StrkPx, pCci) || IsCci(CciEnum.MMY, pCci));
                    isLoadAsset &= pCci.IsLastInputByUser;
                    //
                    // RD 20100706 / en mode Importation, ne charger l'Asset que si toutes les Cci ne comporte pas d'erreur
                    if (isLoadAsset)
                    {
                        CustomCaptureInfo cciExch = Cci(CciEnum.Exch);
                        CustomCaptureInfo cciSym = Cci(CciEnum.Sym);
                        CustomCaptureInfo cciPutCall = Cci(CciEnum.PutCall);
                        CustomCaptureInfo cciStrkPx = Cci(CciEnum.StrkPx);
                        CustomCaptureInfo cciMMY = Cci(CciEnum.MMY);
                        //
                        if (null != cciExch)
                            isLoadAsset &= (false == cciExch.HasError);
                        if (null != cciSym)
                            isLoadAsset &= (false == cciSym.HasError);
                        if (null != cciPutCall)
                            isLoadAsset &= (false == cciPutCall.HasError);
                        if (null != cciStrkPx)
                            isLoadAsset &= (false == cciStrkPx.HasError);
                        if (null != cciMMY)
                            isLoadAsset &= (false == cciMMY.HasError);
                    }
                }
                else
                {
                    isLoadAsset &= IsCci(CciEnum.Exch, pCci);
                    if (isLoadAsset)
                    {
                        CustomCaptureInfo cciExch = Cci(CciEnum.Exch);
                        if (null != cciExch)
                            isLoadAsset &= (false == cciExch.HasError);
                    }
                }
            }
            //
            if (isLoadAsset)
            {
                //Rq Il ne faut pas appliquer de restriction sur DTENABLED,DTDISABLED, Ceci sera fait dans le DumpFixInstrument_ToDocument
                //Sinon lorsque l'asset est disabled, spheres annonce "à tort" qu'un nouvel asset sera créé alors qu'il existe en base
                SQL_AssetBase sqlAsset = null;
                int idI = CciPosRequest.PosRequestInput.SqlInstrument.IdI;
                if (_isETD)
                {
                    sqlAsset = AssetTools.LoadAssetETD(CciPosRequest.CSCacheOn(), null, idI, GetETDCategory(), _fixInstrument, this._cciPosRequest.PosRequestInput.DataDocument.ClearingBusinessDate.DateValue);
                }
                else if (_isEST)
                {
                    sqlAsset = AssetTools.LoadAssetEquity(null, CciPosRequest.CSCacheOn(), idI, _fixInstrument);
                }
                //
                if (null != sqlAsset)
                {
                    //pour forcer le dump de l'asset, on vide lastValue 
                    //exemple l'asset est déjà renseigné, je positionne contrat à blanc, si l'asset est à nouveau remonté après le select de LoadAssetETD alors il faut initialiser toutes les zones de la série 
                    Cci(CciEnum.ID).LastValue = string.Empty;
                    Cci(CciEnum.ID).NewValue = sqlAsset.Identifier;
                }
                else
                {
                    //RD 20100323 /on supprime l'idAsset lorsque l'Asset n'est pas trouvé                             
                    FixInstrument.SetSecurityID(null, null);
                    Cci(CciEnum.ID).NewValue = string.Empty;
                }
            }
        }

        #endregion
    }




}
