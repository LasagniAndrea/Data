#region Using Directives
using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Interface;
using EfsML.StrategyMarker;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using FpML.Interface;
using System;
using System.Linq;
using System.Reflection;
#endregion Using Directives

namespace FixML.v50SP1
{

    #region FixPartyReference
    public partial class FixPartyReference : ICloneable, IReference
    {
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            FixPartyReference clone = new FixPartyReference
            {
                href = this.href
            };
            return clone;
        }
        #endregion Clone
        #endregion IClonable Members
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion FixPartyReference

    #region FIXML
    public partial class FIXML
    {
        #region Constructors
        public FIXML()
        {
            #region Message / Batch
            fixMsgTypeMessage = new TradeCaptureReport_message();
            fixMsgTypeMessageSpecified = true;
            fixMsgTypeBatch = new Batch[1] { new Batch() };
            //fixMsgTypeBatchSpecified = false;
            #endregion Message / Batch

            V = "5.0";
            VSpecified = true;
            S = "20080115";
            SSpecified = true;
        }
        #endregion Constructors
    }
    #endregion FIXML

    #region Header
    #region Abstract_message
    public partial class Abstract_message
    {
        #region Constructors
        public Abstract_message()
        {
        }
        #endregion Constructors

        public bool IsTradeCaptureReportAck { get { return this.GetType().Equals(typeof(TradeCaptureReportAck_message)); } }
        public bool IsTradeCaptureReportRequestAck { get { return this.GetType().Equals(typeof(TradeCaptureReportRequestAck_message)); } }
        public bool IsTradeCaptureReport { get { return this.GetType().Equals(typeof(TradeCaptureReport_message)); } }
        public bool IsTradeCaptureReportRequest { get { return this.GetType().Equals(typeof(TradeCaptureReportRequest_message)); } }
    }
    #endregion Abstract_message
    #region Batch
    public partial class Batch
    {
        #region Constructors
        public Batch()
        {
        }
        #endregion Constructors
    }
    #endregion Batch
    #region Hop
    public partial class Hop
    {
        #region Constructors
        public Hop()
        {
            snt = new EFS_DateTime();
        }
        #endregion Constructors
    }
    #endregion Hop
    #endregion Header

    #region ClrInstGrp_Block
    public partial class ClrInstGrp_Block : IFixClrInstGrp
    {
        #region IFixClrInstGrp Members
        ClearingInstructionEnum IFixClrInstGrp.ClearingInstruction
        {
            set { this.ClrngInstrctn = value; }
            get { return this.ClrngInstrctn; }
        }
        bool IFixClrInstGrp.ClearingInstructionSpecified
        {
            set { this.ClrngInstrctnSpecified = value; }
            get { return this.ClrngInstrctnSpecified; }
        }
        #endregion IFixClrInstGrp Members
    }
    #endregion ClrInstGrp_Block
    #region CommissionData_Block
    public partial class CommissionData_Block
    {
        #region Constructors
        public CommissionData_Block()
        {
            Ccy = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion CommissionData_Block
    #region ContAmtGrp_Block
    public partial class ContAmtGrp_Block
    {
        #region Constructors
        public ContAmtGrp_Block()
        {
            ContAmtCurr = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_ContAmtCurr(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion ContAmtGrp_Block
    #region EvntGrp_Block
    public partial class EvntGrp_Block
    {
        #region Constructors
        public EvntGrp_Block()
        {
            dt = new EFS_Date();
            tm = new EFS_Time();
        }
        #endregion Constructors
    }
    #endregion EvntGrp_Block
    #region FinancingDetails_Block
    public partial class FinancingDetails_Block
    {
        #region Constructors
        public FinancingDetails_Block()
        {
            agmtDt = new EFS_Date();
            AgmtCcy = string.Empty;
            startDt = new EFS_Date();
            endDt = new EFS_Date();
        }
        #endregion Constructors
        #region Methods
        public static object INIT_AgmtCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion FinancingDetails_Block
    #region Instrument_Block
    /// <summary>
    /// 
    /// </summary>
    /// FI 20131126 [19271] Add IFixInstrument.MaturityDate implementation
    /// EG 20140702 [XXXXX] Upd Interface SrcSpecified|Src
    public partial class InstrumentBlock : IFixInstrument
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private Nullable<SecurityIDSourceEnum> _NSINTypeCode;
        #endregion Members
        #region Constructors
        public InstrumentBlock()
        {
            sym = new EFS_String();
            iD = new EFS_String();
            mmy = new EFS_MonthYear();
            strkPx = new EFS_Decimal();
            optAt = new EFS_String();

            matDt = new EFS_Date();
            matTm = new EFS_Time();
            cpnPmt = new EFS_Date();
            issued = new EFS_Date();
            IssuCtry = string.Empty;
            redeem = new EFS_Date();
            StrkCcy = string.Empty;
            Exch = string.Empty;
            dated = new EFS_Date();
            intAcrl = new EFS_Date();
            cntrctDt = new EFS_Date();
        }
        #endregion Constructors

        #region IFixInstrument Members
        string IFixInstrument.Symbol
        {
            set { this.Sym = value; this.SymSpecified = StrFunc.IsFilled(value); }
            get { return this.Sym; }
        }
        string IFixInstrument.SecurityId
        {
            set { this.ID = value; this.IDSpecified = StrFunc.IsFilled(value); }
            get { return this.ID; }
        }
        IFixAlternateAssetId[] IFixInstrument.AlternateId
        {
            set { this.AID = (SecAltIDGrp_Block[])value; }
            get { return this.AID; }
        }
        string IFixInstrument.CFICode
        {
            set { this.CFI = value; this.CFISpecified = StrFunc.IsFilled(value); }
            get { return this.CFI; }
        }
        string IFixInstrument.MaturityMonthYear
        {
            set { this.MMY = value; this.MMYSpecified = StrFunc.IsFilled(value); }
            get { return this.MMY; }
        }

        bool IFixInstrument.MaturityDateSpecified
        {
            set { matDtSpecified = value; }
            get { return matDtSpecified; }
        }
        EFS_Date IFixInstrument.MaturityDate
        {
            set { this.matDt = value; }
            get { return this.matDt; }
        }

        EFS_Decimal IFixInstrument.StrikePrice
        {
            set { this.strkPx = value; }
            get { return this.strkPx; }
        }
        bool IFixInstrument.StrikePriceSpecified
        {
            set { this.strkPxSpecified = value; }
            get { return this.strkPxSpecified; }
        }
        string IFixInstrument.OptAttribute
        {
            set { this.OptAt = value; this.OptAtSpecified = StrFunc.IsFilled(value); }
            get { return this.OptAt; }
        }
        PutOrCallEnum IFixInstrument.PutOrCall
        {
            set { this.PutCall = value; }
            get { return this.PutCall; }
        }
        bool IFixInstrument.PutOrCallSpecified
        {
            set { this.PutCallSpecified = value; }
            get { return this.PutCallSpecified; }
        }
        string IFixInstrument.SecurityExchange
        {
            set { this.Exch = value; this.ExchSpecified = StrFunc.IsFilled(value); }
            get { return this.Exch; }
        }
        string IFixInstrument.ISINCode
        {
            set
            {
                SetAlternateId(value, SecurityIDSourceEnum.ISIN);
            }
            get
            {
                IFixAlternateAssetId alternateAssetId = GetAlternateId(SecurityIDSourceEnum.ISIN);
                return alternateAssetId?.AlternateId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200216 [25699] add
        string IFixInstrument.MarketAssignedIdentifier
        {
            set
            {
                SetAlternateId(value, SecurityIDSourceEnum.MarketplaceAssignedIdentifier);
            }
            get
            {
                IFixAlternateAssetId alternateAssetId = GetAlternateId(SecurityIDSourceEnum.MarketplaceAssignedIdentifier);
                return alternateAssetId?.AlternateId;
            }
        }


        ProductEnum IFixInstrument.FixProduct
        {
            set { this.Prod = value; }
            get { return this.Prod; }
        }
        bool IFixInstrument.FixProductSpecified
        {
            set { this.ProdSpecified = value; }
            get { return this.ProdSpecified; }
        }

        string IFixInstrument.RICCode
        {
            set
            {
                SetAlternateId(value, SecurityIDSourceEnum.RICCode);
            }
            get
            {
                IFixAlternateAssetId alternateAssetId = GetAlternateId(SecurityIDSourceEnum.RICCode);
                return alternateAssetId?.AlternateId;
            }
        }
        string IFixInstrument.BBGCode
        {
            set
            {
                SetAlternateId(value, SecurityIDSourceEnum.BloombergSymbol);
            }
            get
            {
                IFixAlternateAssetId alternateAssetId = GetAlternateId(SecurityIDSourceEnum.BloombergSymbol);
                return alternateAssetId?.AlternateId;
            }
        }
        string IFixInstrument.NSINCode
        {
            get
            {
                IFixAlternateAssetId alternateAssetId = null;
                if (_NSINTypeCode.HasValue)
                    alternateAssetId = GetAlternateId(_NSINTypeCode.Value);
                return alternateAssetId?.AlternateId;
            }
            set
            {
                if (_NSINTypeCode.HasValue)
                    SetAlternateId(value, _NSINTypeCode.Value);
            }
        }
        // EG 20171113 Upd
        string IFixInstrument.NSINTypeCode
        {
            set
            {
                Nullable<SecurityIDSourceEnum> enumValue = ReflectionTools.ConvertStringToEnumOrNullable<SecurityIDSourceEnum>(value); 
                if (enumValue.HasValue)
                    _NSINTypeCode = enumValue.Value;
                else if (_NSINTypeCode.HasValue)
                    RemoveAlternateId(_NSINTypeCode.Value);

            }
            get
            {
                string _code = string.Empty;
                if (_NSINTypeCode.HasValue)
                    _code = ReflectionTools.ConvertEnumToString<SecurityIDSourceEnum>(_NSINTypeCode.Value);
                return _code;

            }
        }
        string IFixInstrument.NSINTypeCodeText
        {
            get
            {
                string _value = string.Empty;
                if (_NSINTypeCode.HasValue)
                    _value = _NSINTypeCode.Value.ToString();
                return _value;
            }
        }
        bool IFixInstrument.IssuerSpecified
        {
            set { this.issrSpecified = value; }
            get { return this.issrSpecified; }
        }
        string IFixInstrument.Issuer
        {
            set { Issr = value; }
            get { return this.Issr; }
        }
        bool IFixInstrument.IssueDateSpecified
        {
            set { issuedSpecified = value; }
            get { return issuedSpecified; }
        }
        EFS_Date IFixInstrument.IssueDate
        {
            set { this.issued = value; }
            get { return this.issued; }
        }
        bool IFixInstrument.CountryOfIssueSpecified
        {
            set { IssuCtrySpecified = value; }
            get { return IssuCtrySpecified; }
        }
        string IFixInstrument.CountryOfIssue
        {
            set { IssuCtry = value; }
            get { return IssuCtry; }
        }
        bool IFixInstrument.StateOrProvinceOfIssueSpecified
        {
            set { stPrvSpecified = value; }
            get { return stPrvSpecified; }
        }
        string IFixInstrument.StateOrProvinceOfIssue
        {
            set { StPrv = value; }
            get { return StPrv; }
        }
        bool IFixInstrument.LocaleOfIssueSpecified
        {
            set { lclSpecified = value; }
            get { return lclSpecified; }
        }
        string IFixInstrument.LocaleOfIssue
        {
            set { Lcl = value; }
            get { return Lcl; }
        }
        string IFixInstrument.ExchangeSymbol
        {
            set
            {
                SetAlternateId(value, SecurityIDSourceEnum.ExchangeSymbol);
            }
            get
            {
                IFixAlternateAssetId alternateAssetId = GetAlternateId(SecurityIDSourceEnum.ExchangeSymbol);
                return alternateAssetId?.AlternateId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        Nullable<SecurityIDSourceEnum> IFixInstrument.Src
        {
            set
            {
                this.SrcSpecified = value.HasValue;
                if (this.SrcSpecified)
                    this.Src = value.Value;
            }
            get
            {
                Nullable<SecurityIDSourceEnum> ret = default(SecurityIDSourceEnum);
                if (this.SrcSpecified)
                    ret = this.Src;
                return ret;

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220209 [25699] Add
        bool IFixInstrument.CntrctDtSpecified
        {
            set { cntrctDtSpecified = value; }
            get { return cntrctDtSpecified; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220209 [25699] Add
        EFS_Date IFixInstrument.CntrctDt
        {
            set { cntrctDt = value; }
            get { return cntrctDt; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220209 [25699] Add
        bool IFixInstrument.CntrctFreqSpecified
        {
            set { CntrctFreqSpecified = value; }
            get { return CntrctFreqSpecified; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220209 [25699] Add
        ContractFrequencyEnum IFixInstrument.CntrctFreq
        {
            set { CntrctFreq = value; }
            get { return CntrctFreq; }
        }

        // EG 20171113 Upd
        IFixAlternateAssetId IFixInstrument.GetAlternateId(string pIDSource)
        {
            IFixAlternateAssetId AAID = null;
            Nullable<SecurityIDSourceEnum> enumValue = ReflectionTools.ConvertStringToEnumOrNullable<SecurityIDSourceEnum>(pIDSource);
            if (enumValue.HasValue)
                AAID = GetAlternateId(enumValue.Value);
            return AAID;
        }
        // EG 20171113 Upd
        void IFixInstrument.SetAlternateId(string pID, string pIDSource)
        {
            Nullable<SecurityIDSourceEnum> enumValue = ReflectionTools.ConvertStringToEnumOrNullable<SecurityIDSourceEnum>(pIDSource); 
            if (enumValue.HasValue)
                SetAlternateId(pID, enumValue.Value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="pIDSource"></param>
        void IFixInstrument.SetSecurityID(string pID, Nullable<SecurityIDSourceEnum> pIDSource)
        {
            IDSpecified = StrFunc.IsFilled(pID);
            ID = pID;

            Src = default;
            SrcSpecified = (pIDSource.HasValue);
            if (SrcSpecified)
                Src = pIDSource.Value;
        }
        #endregion IFixInstrument Members

        #region Methods
        public static object INIT_IssuCtry(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_StrkCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_Exch(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Static Methods
        #region Methods
        private IFixAlternateAssetId GetAlternateId(SecurityIDSourceEnum pSource)
        {
            return GetAlternateId(pSource, true);
        }
        private IFixAlternateAssetId GetAlternateId(SecurityIDSourceEnum pSource, bool pCheckSourceSpecified)
        {
            IFixAlternateAssetId alternateId = null;
            if (null != AID)
            {
                if (AID.Length > 0)
                    foreach (IFixAlternateAssetId currentAlternateId in AID)
                    {
                        if (pSource == currentAlternateId.AlternateIdSource)
                        {
                            if (currentAlternateId.AlternateIdSourceSpecified || (false == pCheckSourceSpecified))
                                alternateId = currentAlternateId;
                            break;
                        }
                    }
            }

            return (alternateId);
        }
        /// <summary>
        ///  Suppression de AlternateId dont la source est {pSource}
        /// </summary>
        /// <param name="pSource"></param>
        /// FI 20160916 [XXXXX] Modify
        private void RemoveAlternateId(SecurityIDSourceEnum pSource)
        {
            // FI 20160916 [XXXXX] Usage d'une instruction LINK
            if (ArrFunc.IsFilled(AID))
                AID = AID.Where(x => ((IFixAlternateAssetId)x).AlternateIdSourceSpecified && ((IFixAlternateAssetId)x).AlternateIdSource != pSource).ToArray();
       }

        /// <summary>
        ///  Ajoute/Supprime un AlternateId
        /// </summary>
        /// <param name="pId">
        /// <para>si renseigné ajoute l'AlternateId associé à la source {pSource}</para>
        /// <para>si non renseigné supprime l'AlternateId associé à la source {pSource}</para>
        /// </param>
        /// <param name="pSource"></param>
        /// FI 20160916 [XXXXX] Modify
        private void SetAlternateId(string pId, SecurityIDSourceEnum pSource)
        {
            Boolean isAdd = StrFunc.IsFilled(pId);
            Boolean isRemove = !isAdd;

            IFixAlternateAssetId alternateId = GetAlternateId(pSource, false);

            if (isAdd)
            {

                if (null == alternateId)
                {
                    if ((null != AID) && (AID.Length > 0))
                    {
                        int newPos = AID.Length;
                        ReflectionTools.AddItemInArray(this, "AID", newPos);
                        alternateId = AID[newPos];
                    }
                    else
                    {
                        AID = new SecAltIDGrp_Block[1] { new SecAltIDGrp_Block() };
                        alternateId = AID[0];
                    }
                }

                alternateId.AlternateId = pId;
                alternateId.AlternateIdSource = pSource;
                alternateId.AlternateIdSourceSpecified = true;
            }
            else if (isRemove) 
            {
                RemoveAlternateId(pSource);
            }
            else
            {
                throw new NotSupportedException("Not supported exception in SetAlternateId Method"); 
            }
        }
        #endregion Methods
    }
    #endregion Instrument_Block
    #region InstrumentLeg_Block
    public partial class InstrumentLeg_Block
    {
        #region Constructors
        public InstrumentLeg_Block()
        {
            mat = new EFS_Date();
            cpnPmt = new EFS_Date();
            issued = new EFS_Date();
            Ctry = string.Empty;
            redeem = new EFS_Date();
            StrkCcy = string.Empty;
            Exch = string.Empty;
            Ccy = string.Empty;
            dated = new EFS_Date();
            intAcrl = new EFS_Date();
        }
        #endregion Constructors
        #region Methods
        public static object INIT_Ctry(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_StrkCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_Exch(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion InstrumentLeg_Block
    #region InstrumentParties_Block
    public partial class InstrumentParties_Block : IFixParty
    {
        #region IFixParty Members
        FixPartyReference IFixParty.PartyId
        {
            set { this.id = value; }
            get { return this.id; }
        }
        bool IFixParty.PartyIdSpecified
        {
            set { this.idSpecified = value; }
            get { return this.idSpecified; }
        }
        PartyIDSourceEnum IFixParty.PartyIdSource
        {
            set { this.Src = value; }
            get { return this.Src; }
        }
        bool IFixParty.PartyIdSourceSpecified
        {
            set { this.SrcSpecified = value; }
            get { return this.SrcSpecified; }
        }
        PartyRoleEnum IFixParty.PartyRole
        {
            set { this.R = value; }
            get { return this.R; }
        }
        bool IFixParty.PartyRoleSpecified
        {
            set { this.RSpecified = value; }
            get { return this.RSpecified; }
        }
        IFixPartySubGrp[] IFixParty.PtysSubGrp
        {
            set { this.Sub = (InstrumentPtysSubGrp_Block[])value; }
            get { return this.Sub; }
        }
        #endregion IFixParty Members
    }
    #endregion InstrumentParties_Block
    #region InstrumentPtysSubGrp_Block
    public partial class InstrumentPtysSubGrp_Block : IFixPartySubGrp
    {
        #region IFixPartySubGrp Members
        string IFixPartySubGrp.PartySubId
        {
            set { this.ID = value; }
            get { return this.ID; }
        }
        PartySubIDTypeEnum IFixPartySubGrp.PartySubIdType
        {
            set { this.Typ = value; }
            get { return this.Typ; }
        }
        #endregion IFixPartySubGrp Members
    }
    #endregion InstrumentPtysSubGrp_Block
    #region MiscFeesGrp_Block
    public partial class MiscFeesGrp_Block
    {
        #region Constructors
        public MiscFeesGrp_Block()
        {
            Curr = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_Curr(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion MiscFeesGrp_Block
    #region Parties_Block
    public partial class Parties_Block : IFixParty
    {
        #region Constructors
        public Parties_Block()
        {
            id = new FixPartyReference();
        }
        #endregion Constructors
        #region IFixParty Members
        FixPartyReference IFixParty.PartyId
        {
            set { this.id = value; }
            get { return this.id; }
        }
        bool IFixParty.PartyIdSpecified
        {
            set { this.idSpecified = value; }
            get { return this.idSpecified; }
        }
        PartyIDSourceEnum IFixParty.PartyIdSource
        {
            set { this.Src = value; }
            get { return this.Src; }
        }
        bool IFixParty.PartyIdSourceSpecified
        {
            set { this.SrcSpecified = value; }
            get { return this.SrcSpecified; }
        }
        PartyRoleEnum IFixParty.PartyRole
        {
            set { this.R = value; }
            get { return this.R; }
        }
        bool IFixParty.PartyRoleSpecified
        {
            set { this.RSpecified = value; }
            get { return this.RSpecified; }
        }
        IFixPartySubGrp[] IFixParty.PtysSubGrp
        {
            set { this.Sub = (PtysSubGrp_Block[])value; }
            get { return this.Sub; }
        }
        #endregion IFixParty Members
    }
    #endregion Parties_Block
    #region PtysSubGrp_Block
    public partial class PtysSubGrp_Block : IFixPartySubGrp
    {
        #region IFixPartySubGrp Members
        string IFixPartySubGrp.PartySubId
        {
            set { this.ID = value; }
            get { return this.ID; }
        }
        PartySubIDTypeEnum IFixPartySubGrp.PartySubIdType
        {
            set { this.Typ = value; }
            get { return this.Typ; }
        }
        #endregion IFixPartySubGrp Members
    }
    #endregion PtysSubGrp_Block
    #region RelatedPositionGrp_Block
    // PM 20160428 [22107] Ajout à partir de l'Extension Pack: FIX.5.0SP2 EP142
    public partial class RelatedPositionGrp_Block : IFixRelatedPositionGrp
    {
        #region IFixRelatedPositionGrp Members
        string IFixRelatedPositionGrp.ID
        {
            set { this.ID = value; }
            get { return this.ID; }
        }
        //
        RelatedPositionIDSourceEnum IFixRelatedPositionGrp.Src
        {
            set { this.Src = value; }
            get { return this.Src; }
        }
        bool IFixRelatedPositionGrp.SrcSpecified
        {
            set { this.SrcSpecified = value; }
            get { return this.SrcSpecified; }
        }
        //
        System.DateTime IFixRelatedPositionGrp.Dt
        {
            set { this.Dt = value; }
            get { return this.Dt; }
        }
        bool IFixRelatedPositionGrp.DtSpecified
        {
            set { this.DtSpecified = value; }
            get { return this.DtSpecified; }
        }
        #endregion IFixRelatedPositionGrp Members
    }
    #endregion RelatedPositionGrp_Block

    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
    public partial class RegulatoryTradeIDGrp_Block : IFixRegulatoryTradeIDGrp
    {
        string IFixRegulatoryTradeIDGrp.ID
        {
            set { this.ID = value; }
            get { return this.ID; }
        }
        RegulatoryTradeIDSourceEnum IFixRegulatoryTradeIDGrp.Src
        {
            set { this.Src = value; }
            get { return this.Src; }
        }
        bool IFixRegulatoryTradeIDGrp.SrcSpecified
        {
            set { this.SrcSpecified = value; }
            get { return this.SrcSpecified; }
        }
        RegulatoryTradeIDTypeEnum IFixRegulatoryTradeIDGrp.Typ
        {
            set { this.Typ = value; }
            get { return this.Typ; }
        }
        bool IFixRegulatoryTradeIDGrp.TypSpecified
        {
            set { this.TypSpecified = value; }
            get { return this.TypSpecified; }
        }
        RegulatoryTradeIDEventEnum IFixRegulatoryTradeIDGrp.Evnt
        {
            set { this.Evnt = value; }
            get { return this.Evnt; }
        }
        bool IFixRegulatoryTradeIDGrp.EvntSpecified
        {
            set { this.EvntSpecified = value; }
            get { return this.EvntSpecified; }
        }
        RegulatoryTradeIDScopeEnum IFixRegulatoryTradeIDGrp.Scope
        {
            set { this.Scope = value; }
            get { return this.Scope; }
        }
        bool IFixRegulatoryTradeIDGrp.ScopeSpecified
        {
            set { this.ScopeSpecified = value; }
            get { return this.ScopeSpecified; }
        }
        string IFixRegulatoryTradeIDGrp.LegRefID
        {
            set { this.LegRefID = value; }
            get { return this.LegRefID; }
        }
        bool IFixRegulatoryTradeIDGrp.LegRefIDSpecified
        {
            set { this.LegRefIDSpecified = value; }
            get { return this.LegRefIDSpecified; }
        }
    }
    #region RootParties_Block
    public partial class RootParties_Block : IFixParty
    {
        #region IFixParty Members
        FixPartyReference IFixParty.PartyId
        {
            set { this.id = value; }
            get { return this.id; }
        }
        bool IFixParty.PartyIdSpecified
        {
            set { this.idSpecified = value; }
            get { return this.idSpecified; }
        }
        PartyIDSourceEnum IFixParty.PartyIdSource
        {
            set { this.Src = value; }
            get { return this.Src; }
        }
        bool IFixParty.PartyIdSourceSpecified
        {
            set { this.SrcSpecified = value; }
            get { return this.SrcSpecified; }
        }
        PartyRoleEnum IFixParty.PartyRole
        {
            set { this.R = value; }
            get { return this.R; }
        }
        bool IFixParty.PartyRoleSpecified
        {
            set { this.RSpecified = value; }
            get { return this.RSpecified; }
        }
        IFixPartySubGrp[] IFixParty.PtysSubGrp
        {
            set { this.Sub = (RootSubParties_Block[])value; }
            get { return this.Sub; }
        }
        #endregion IFixParty Members
    }
    #endregion RootParties_Block
    #region RootSubParties_Block
    public partial class RootSubParties_Block : IFixPartySubGrp
    {
        #region IFixPartySubGrp Members
        string IFixPartySubGrp.PartySubId
        {
            set { this.ID = value; }
            get { return this.ID; }
        }
        PartySubIDTypeEnum IFixPartySubGrp.PartySubIdType
        {
            set { this.Typ = value; }
            get { return this.Typ; }
        }
        #endregion IFixPartySubGrp Members
    }
    #endregion RootSubParties_Block
    #region SecAltIDGrp_Block
    public partial class SecAltIDGrp_Block : IFixAlternateAssetId
    {
        #region Accessors
        string IFixAlternateAssetId.AlternateId
        {
            set
            {
                this.AltID = value;
                this.AltIDSpecified = (null != value);
            }
            get { return this.AltID; }
        }
        SecurityIDSourceEnum IFixAlternateAssetId.AlternateIdSource
        {
            set { this.AltIDSrc = value; }
            get { return this.AltIDSrc; }
        }
        bool IFixAlternateAssetId.AlternateIdSourceSpecified
        {
            set { this.AltIDSrcSpecified = value; }
            get { return this.AltIDSrcSpecified; }
        }
        #endregion Accessors
    }
    #endregion SecAltIDGrp_Block
    #region PositionAmountData_Block
    public partial class PositionAmountData_Block
    {
        #region Constructors
        public PositionAmountData_Block()
        {
            Ccy = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion PositionAmountData_Block
    #region SideTrdRegTS_Block
    public partial class SideTrdRegTS_Block
    {
        #region Constructors
        public SideTrdRegTS_Block()
        {
            ts = new EFS_DateTime();
        }
        #endregion Constructors
    }
    #endregion SideTrdRegTS_Block
    #region SpreadOrBenchmarkCurveData_Block
    public partial class SpreadOrBenchmarkCurveData_Block
    {
        #region Constructors
        public SpreadOrBenchmarkCurveData_Block()
        {
            Ccy = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion SpreadOrBenchmarkCurveData_Block
    #region TrdAllocGrp_Block
    public partial class TrdAllocGrp_Block
    {
        #region Constructors
        public TrdAllocGrp_Block()
        {
            AllocSettlCcy = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_AllocSettlCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion TrdAllocGrp_Block
    #region TrdCapRptSideGrp_Block
    public partial class TrdCapRptSideGrp_Block : IFixTrdCapRptSideGrp
    {
        #region Constructors
        public TrdCapRptSideGrp_Block()
        {
            Ccy = string.Empty;
            SettlCcy = string.Empty;
            transBkdTm = new EFS_DateTime();
            exDt = new EFS_Date();
            //Pty = new Parties_Block[1] { new Parties_Block() };
        }
        #endregion Constructors

        #region IFixTrdCapRptSideGrp Members
        bool IFixTrdCapRptSideGrp.SideSpecified
        {
            set { this.SideSpecified = value; }
            get { return this.SideSpecified; }
        }
        SideEnum IFixTrdCapRptSideGrp.Side
        {
            set { this.Side = value; }
            get { return this.Side; }
        }
        //
        bool IFixTrdCapRptSideGrp.OrderIdSpecified
        {
            set { this.OrdIDSpecified   = value; }
            get { return this.OrdIDSpecified  ; }
        }
        string IFixTrdCapRptSideGrp.OrderId
        {
            set { this.OrdID = value; }
            get { return this.OrdID; }
        }
        //
        bool IFixTrdCapRptSideGrp.ExecRefIdSpecified
        {
            set { this.ExecRefIDSpecified = value; }
            get { return this.ExecRefIDSpecified; }
        }
        string IFixTrdCapRptSideGrp.ExecRefId
        {
            set { this.ExecRefID = value; }
            get { return this.ExecRefID; }
        }
        //
        IFixParty[] IFixTrdCapRptSideGrp.Parties
        {
            set { this.Pty = (Parties_Block[])value; }
            get { return this.Pty; }
        }
        bool IFixTrdCapRptSideGrp.AccountSpecified
        {
            set { AcctSpecified = value; }
            get { return  this.AcctSpecified; }
        }
        string IFixTrdCapRptSideGrp.Account
        {
            set { this.Acct = value; }
            get { return this.Acct; }
        }
        bool IFixTrdCapRptSideGrp.AcctIdSourceSpecified 
        {
            set { AcctIDSrcSpecified = value; }
            get { return this.AcctIDSrcSpecified; }
        }
        AcctIDSourceEnum IFixTrdCapRptSideGrp.AcctIdSource
        {
            set { this.AcctIDSrc = value; }
            get { return this.AcctIDSrc; }
        }
        bool IFixTrdCapRptSideGrp.AccountTypeSpecified
        {
            set { this.AcctTypSpecified = value; }
            get { return this.AcctTypSpecified; }
        }
        AccountTypeEnum IFixTrdCapRptSideGrp.AccountType
        {
            set { this.AcctTyp = value; }
            get { return this.AcctTyp; }
        }
        IFixClrInstGrp[] IFixTrdCapRptSideGrp.ClrInstGrp
        {
            set { this.ClrInst = (ClrInstGrp_Block[])value; }
            get { return this.ClrInst; }
        }
        string IFixTrdCapRptSideGrp.TradeInputSource
        {
            set { this.InptSrc = value; }
            get { return this.InptSrc; }
        }
        bool IFixTrdCapRptSideGrp.TradeInputSourceSpecified
        {
            set { this.InptSrcSpecified = value; }
            get { return this.InptSrcSpecified; }
        }
        string IFixTrdCapRptSideGrp.TradeInputDevice
        {
            set { this.InptDev = value; }
            get { return this.InptDev; }
        }
        bool IFixTrdCapRptSideGrp.TradeInputDeviceSpecified
        {
            set { this.InptDevSpecified = value; }
            get { return this.InptDevSpecified; }
        }
        string IFixTrdCapRptSideGrp.OrderInputDevice
        {
            set { this.OrdInptDev = value; }
            get { return this.OrdInptDev; }
        }
        bool IFixTrdCapRptSideGrp.OrderInputDeviceSpecified
        {
            set { this.OrdInptDevSpecified = value; }
            get { return this.OrdInptDevSpecified; }
        }
        OrdTypeEnum IFixTrdCapRptSideGrp.OrdType
        {
            set { this.OrdTyp = value; }
            get { return this.OrdTyp; }
        }
        bool IFixTrdCapRptSideGrp.OrdTypeSpecified
        {
            set { this.OrdTypSpecified = value; }
            get { return this.OrdTypSpecified; }
        }
        string IFixTrdCapRptSideGrp.TradingSessionId
        {
            set { this.SesID = value; }
            get { return this.SesID; }
        }
        bool IFixTrdCapRptSideGrp.TradingSessionIdSpecified
        {
            set { this.SesIDSpecified = value; }
            get { return this.SesIDSpecified; }
        }
        bool IFixTrdCapRptSideGrp.PositionEffectSpecified
        {
            set { this.PosEfctSpecified = value; }
            get { return this.PosEfctSpecified; }
        }
        PositionEffectEnum IFixTrdCapRptSideGrp.PositionEffect
        {
            set { this.PosEfct = value; }
            get { return this.PosEfct; }
        }
        string IFixTrdCapRptSideGrp.Text
        {
            set { this.Txt = value; }
            get { return this.Txt; }
        }
        bool IFixTrdCapRptSideGrp.TextSpecified
        {
            set { this.TxtSpecified = value; }
            get { return this.TxtSpecified; }
        }
        // PM 20160428 [22107] Ajout à partir de l'Extension Pack: FIX.5.0SP2 EP142
        IFixRelatedPositionGrp[] IFixTrdCapRptSideGrp.ReltdPos
        {
            set { this.ReltdPos = (RelatedPositionGrp_Block[])value; }
            get { return this.ReltdPos; }
        }
        #endregion IFixTrdCapRptSideGrp Members

        #region Methods
        public static object INIT_Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_SettlCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion TrdCapRptSideGrp_Block
    #region TrdInstrmtLegGrp_Block
    /// <summary>
    /// 
    /// </summary>
    /// FI 20180214 [23774] Modify
    public partial class TrdInstrmtLegGrp_Block
    {
        #region Constructors
        public TrdInstrmtLegGrp_Block()
        {
            // FI 20180214 [23774] 
            Leg = new InstrumentLeg_Block();
            settlDt = new EFS_Date();
            SettlCcy = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_SettlCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion TrdInstrmtLegGrp_Block
    #region TrdRegTimestamps_Block
    /// EG 20171004 [23452] Upd
    public partial class TrdRegTimestamps_Block : IFixTrdRegTimestamps
    {
        #region Constructors
        public TrdRegTimestamps_Block()
        {
            ts = new EFS_DateTimeOffset();
        }
        #endregion Constructors

        #region IFixTrdRegTimestamps Members
        bool IFixTrdRegTimestamps.TSSpecified
        {
            get {return this.TSSpecified;}
            set { this.TSSpecified = value; }
        }

        EFS_DateTimeOffset IFixTrdRegTimestamps.TS
        {
            set { this.ts = value; }
            get { return this.ts; }
        }

        bool IFixTrdRegTimestamps.TypSpecified
        {
            get {return this.TypSpecified;}
            set { this.TypSpecified = value; }
        }

        TrdRegTimestampTypeEnum IFixTrdRegTimestamps.Typ
        {
            set { this.Typ = value; }
            get { return this.Typ; }
        }
        #endregion IFixTrdRegTimestamps Members
    }
    #endregion TrdRegTimestamps_Block
    #region UnderlyingInstrument_Block
    public partial class UnderlyingInstrument_Block
    {
        #region Constructors
        public UnderlyingInstrument_Block()
        {
            mat = new EFS_Date();
            matTm = new EFS_Time();
            cpnPmt = new EFS_Date();
            issued = new EFS_Date();
            Ctry = string.Empty;
            redeem = new EFS_Date();
            StrkCcy = string.Empty;
            Exch = string.Empty;
            Ccy = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_Ctry(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_StrkCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_Exch(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion UnderlyingInstrument_Block
    #region UnderlyingLegInstrument_Block
    public partial class UnderlyingLegInstrument_Block
    {
        #region Constructors
        public UnderlyingLegInstrument_Block()
        {
            matDt = new EFS_Date();
            matTm = new EFS_Time();
            Exch = string.Empty;
        }
        #endregion Constructors
        #region Methods
        public static object INIT_Exch(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion UnderlyingLegInstrument_Block
    #region UndInstrmtGrp_Block
    public partial class UndInstrmtGrp_Block
    {
    }
    #endregion UndInstrmtGrp_Block
    #region YieldData_Block
    public partial class YieldData_Block
    {
        #region Constructors
        public YieldData_Block()
        {
            calcDt = new EFS_Date();
            redDt = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion YieldData_Block

    #region TradeCaptureReport_message
    // EG 20171004 [23452] Upd
    // EG 20171025 [23509] Add RemoveTradeRegulatoryTimeStamp
    // EG 20190730 New interface ITradeTypeReport (shared with DebtSecurityTransaction)
    public partial class TradeCaptureReport_message : IFixTradeCaptureReport
    {
        #region Constructors
        // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
        public TradeCaptureReport_message()
        {
            trdID = new EFS_String();
            rptID = new EFS_String();
            lastQty = new EFS_Decimal();
            lastPx = new EFS_Decimal();
            Ccy = string.Empty;
            SettlCcy = string.Empty;
            LastMkt = string.Empty;
            trdDt = new EFS_Date();
            bizDt = new EFS_Date();
            settlDt = new EFS_Date();
            stlDt = new EFS_Date();
            tZTransactTime = new EFS_DateTime();
            trnsfrRsn = new EFS_String();

            Instrmt = new InstrumentBlock();
            RptSide = new TrdCapRptSideGrp_Block[1] { new TrdCapRptSideGrp_Block() };
            // Réceptionne le TradeDateTime
            txnTm = new EFS_DateTimeOffset();
            lastUpdateTm = new EFS_DateTimeOffset();
            // Réceptionne le ExecutionDateTime de la plateforme
            TrdRegTS = new TrdRegTimestamps_Block[1] { new TrdRegTimestamps_Block() };
            // EG 20240227 [WI855] New
            RegTrdID = new RegulatoryTradeIDGrp_Block[1] { new RegulatoryTradeIDGrp_Block()};
        }
        #endregion Constructors

        #region IFixTradeCaptureReport Members
        bool IFixTradeCaptureReport.TradeIdSpecified
        {
            set { this.TrdIDSpecified = value; }
            get { return this.TrdIDSpecified; }
        }
        string IFixTradeCaptureReport.TradeId
        {
            set { this.TrdID = value; }
            get { return this.TrdID; }
        }
        //
        bool IFixTradeCaptureReport.FirmTradeIdSpecified
        {
            set { this.FirmTrdIDSpecified = value; }
            get { return this.FirmTrdIDSpecified; }
        }
        string IFixTradeCaptureReport.FirmTradeId
        {
            set { this.FirmTrdID = value; }
            get { return this.FirmTrdID; }
        }
        //
        bool IFixTradeCaptureReport.TradeReportTransTypeSpecified
        {
            set { this.TransTypSpecified = value; }
            get { return this.TransTypSpecified; }
        }
        TradeReportTransTypeEnum IFixTradeCaptureReport.TradeReportTransType
        {
            set { this.TransTyp = value; }
            get { return this.TransTyp; }
        }
        //
        bool ITradeTypeReport.TrdTypeSpecified
        {
            set { this.TrdTypSpecified = value; }
            get { return this.TrdTypSpecified; }
        }
        TrdTypeEnum ITradeTypeReport.TrdType
        {
            set { this.TrdTyp = value; }
            get { return this.TrdTyp; }
        }
        //
        bool ITradeTypeReport.SecondaryTrdTypeSpecified
        {
            set { this.TrdTyp2Specified = value; }
            get { return this.TrdTyp2Specified; }
        }
        SecondaryTrdTypeEnum ITradeTypeReport.SecondaryTrdType
        {
            set { this.TrdTyp2 = value; }
            get { return this.TrdTyp2; }
        }
        //
        bool ITradeTypeReport.TrdSubTypeSpecified
        {
            set { this.TrdSubTypSpecified = value; }
            get { return this.TrdSubTypSpecified; }
        }
        TrdSubTypeEnum ITradeTypeReport.TrdSubType
        {
            set { this.TrdSubTyp = value; }
            get { return this.TrdSubTyp; }
        }
        //
        bool IFixTradeCaptureReport.ExecTypeSpecified
        {
            set { this.ExecTypSpecified = value; }
            get { return this.ExecTypSpecified; }
        }
        ExecTypeEnum IFixTradeCaptureReport.ExecType
        {
            set { this.ExecTyp = value; }
            get { return this.ExecTyp; }
        }
        //
        bool IFixTradeCaptureReport.TradeLinkIdSpecified
        {
            set { this.LinkIDSpecified = value; }
            get { return this.LinkIDSpecified; }
        }
        string IFixTradeCaptureReport.TradeLinkId
        {
            set { this.LinkID = value; }
            get { return this.LinkID; }
        }
        //
        bool IFixTradeCaptureReport.TrdMatchIdSpecified
        {
            set { this.MtchIDSpecified = value; }
            get { return this.MtchIDSpecified; }
        }
        string IFixTradeCaptureReport.TrdMatchId
        {
            set { this.MtchID = value; }
            get { return this.MtchID; }
        }
        //
        bool IFixTradeCaptureReport.ExecIdSpecified
        {
            set { this.ExecIDSpecified = value; }
            get { return this.ExecIDSpecified; }
        }
        string IFixTradeCaptureReport.ExecId
        {
            set { this.ExecID = value; }
            get { return this.ExecID; }
        }
        //
        bool IFixTradeCaptureReport.OrdStatusSpecified
        {
            set { this.OrdStatSpecified = value; }
            get { return this.OrdStatSpecified; }
        }
        OrdStatusEnum IFixTradeCaptureReport.OrdStatus
        {
            set { this.OrdStat = value; }
            get { return this.OrdStat; }
        }
        //
        IFixInstrument IFixTradeCaptureReport.Instrument
        {
            set { this.Instrmt = (InstrumentBlock)value; }
            get { return this.Instrmt; }
        }
        // EG 20171031 [23509] Upd
        bool  IFixTradeCaptureReport.LastPxSpecified
        {
            set { this.lastPxSpecified = value; }
            get { return this.lastPxSpecified; }
        }
        EFS_Decimal IFixTradeCaptureReport.LastPx
        {
            set { this.lastPx = value; }
            get { return this.lastPx; }
        }
        // EG 20171031 [23509] Upd
        bool  IFixTradeCaptureReport.LastQtySpecified
        {
            set { this.lastQtySpecified = value; }
            get { return this.lastQtySpecified; }
        }
        EFS_Decimal IFixTradeCaptureReport.LastQty
        {
            set { this.lastQty = value; }
            get { return this.lastQty; }
        }
        // EG 20171031 [23509] Upd
        bool IFixTradeCaptureReport.CcySpecified
        {
            set { this.CcySpecified = value; }
            get { return this.CcySpecified; }
        }
        string IFixTradeCaptureReport.Ccy
        {
            set { this.Ccy = value; }
            get { return this.Ccy; }
        }
        // EG 20171031 [23509] Upd
        bool IFixTradeCaptureReport.TradeDateSpecified
        {
            set { this.trdDtSpecified = value; }
            get { return this.trdDtSpecified; }
        }
        EFS_Date IFixTradeCaptureReport.TradeDate
        {
            set { this.trdDt = value; }
            get { return this.trdDt; }
        }
        // EG 20171031 [23509] Upd
        bool IFixTradeCaptureReport.ClearingBusinessDateSpecified
        {
            set { this.bizDtSpecified = value; }
            get { return this.bizDtSpecified; }
        }
        EFS_Date IFixTradeCaptureReport.ClearingBusinessDate
        {
            set { this.bizDt = value; }
            get { return this.bizDt; }
        }
        //         
        bool IFixTradeCaptureReport.TransactTimeSpecified
        {
            set { this.txnTmSpecified = value; }
            get { return this.txnTmSpecified; }
        }

        EFS_DateTimeOffset IFixTradeCaptureReport.TransactTime
        {
            set { this.txnTm = value; }
            get { return this.txnTm; }
        }

        bool IFixTradeCaptureReport.LastUpdateTimeSpecified
        {
            set { this.lastUpdateTmSpecified = value; }
            get { return this.lastUpdateTmSpecified; }
        }

        EFS_DateTimeOffset IFixTradeCaptureReport.LastUpdateTime
        {
            set { this.lastUpdateTm = value; }
            get { return this.lastUpdateTm; }
        }

        //
        bool IFixTradeCaptureReport.OrderCategorySpecified
        {
            set { this.OrdCatSpecified = value; }
            get { return this.OrdCatSpecified; }
        }
        OrderCategoryEnum IFixTradeCaptureReport.OrderCategory
        {
            set { this.OrdCat = value; }
            get { return this.OrdCat; }
        }
        
        IFixTrdCapRptSideGrp[] IFixTradeCaptureReport.TrdCapRptSideGrp
        {
            set { this.RptSide = (TrdCapRptSideGrp_Block[])value; }
            get { return this.RptSide; }
        }

        bool IFixTradeCaptureReport.TransferReasonSpecified
        {
            set { this.trnsfrRsnSpecified = value; }
            get { return this.trnsfrRsnSpecified; }
        }
        EFS_String IFixTradeCaptureReport.TransferReason
        {
            set { this.trnsfrRsn = value; }
            get { return this.trnsfrRsn; }
        }

        /// <summary>
        /// Get/Set the type of strategy, which the current trade is making part of
        /// </summary>
        /// <remarks>
        /// http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag762.html
        /// </remarks>
        /// <value>
        /// Not null just for trade ALLOC provided with an electronic order and marked as strategy.
        /// Null in any other cases, included strategy ORDER/EXECUTION.
        /// </value>
        /// MF XXXXXXXXX [17864] Add Get
        /// FI 20180214 [23774] Add Set
        string IFixTradeCaptureReport.SecSubTyp
        {
            get
            {
                if (ArrFunc.IsFilled(this.TrdLeg))
                {
                    // TrdLeg[0] -> Just one TrdLeg item is allowed for trade ALLOC
                    return this.TrdLeg[0].Leg.SecSubTyp;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (StrFunc.IsFilled(value))
                {
                    this.TrdLegSpecified = true;
                    if (ArrFunc.IsEmpty(TrdLeg))
                        TrdLeg = new TrdInstrmtLegGrp_Block[1] { new TrdInstrmtLegGrp_Block() };

                    this.TrdLeg[0].Leg.SecTypSpecified = true;
                    this.TrdLeg[0].Leg.SecTyp = SecurityTypeEnum.MultilegInstrument;

                    this.TrdLeg[0].Leg.secSubTypSpecified = true;
                    this.TrdLeg[0].Leg.SecSubTyp = value;
                }
                else
                {
                    this.TrdLegSpecified = false;
                    this.TrdLeg = null;
                }
            }
        }

        // ticket 17864
        /// <summary>
        /// Get the type of strategy, which the current trade is making part of. 
        /// Expressed as value of the StrategyEnum Spheres enumeration
        /// </summary>
        /// <value>
        /// Returns a valid value just for trade ALLOC provided with an electronic order and marked as strategy.
        /// 'Unknown' in any other cases, included strategy ORDER/EXECUTION.
        /// </value>
        StrategyEnumRepository.StrategyEnum IFixTradeCaptureReport.StrategyTyp
        {
            get
            {
                if (
                    ((IFixTradeCaptureReport)this).SecSubTyp != null
                    &&
                    System.Enum.IsDefined(typeof(StrategyEnumRepository.StrategyEnum), ((IFixTradeCaptureReport)this).SecSubTyp)
                    )
                {
                    return (StrategyEnumRepository.StrategyEnum)
                        System.Enum.Parse(typeof(StrategyEnumRepository.StrategyEnum), ((IFixTradeCaptureReport)this).SecSubTyp);
                }
                else
                {
                    // default is "unknown" actually
                    return default;
                }
            }
        }

        /// <summary>
        /// Get the current leg number for the current trade
        /// </summary>
        /// <remarks>
        /// http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag1152.html
        /// </remarks>
        /// <value>
        /// the current leg number for for trade ALLOC provided with an electronic order and marked as strategy.
        /// 0 in any other cases, included strategy ORDER/EXECUTION.
        /// </value>
        /// MF XXXXXXXXX [17864] Add Get
        /// FI 20180214 [23774] Add Set
        int IFixTradeCaptureReport.LegNo
        {
            get
            {
                if (ArrFunc.IsFilled(this.TrdLeg))
                {
                    // TrdLeg[0] -> Just one TrdLeg item is allowed for trade ALLOC
                    int.TryParse(this.TrdLeg[0].LegNo, out int legNo);
                    return legNo;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (value > 0)
                {
                    if (ArrFunc.IsEmpty(TrdLeg))
                        TrdLeg = new TrdInstrmtLegGrp_Block[1] { new TrdInstrmtLegGrp_Block() };

                    this.TrdLeg[0].legNoSpecified = true;
                    this.TrdLeg[0].LegNo = value.ToString();
                }
                else
                {
                    if (ArrFunc.Count(this.TrdLeg) == 1)
                    {
                        this.TrdLeg[0].legNoSpecified = false;
                        this.TrdLeg[0].LegNo = "0";
                    }
                }
            }
        }

        IFixTrdRegTimestamps[] IFixTradeCaptureReport.TradedRegulatoryTimestamps
        {
            set { this.TrdRegTS = value.Cast<TrdRegTimestamps_Block>().ToArray();}
            get { return this.TrdRegTS; }
        }
        // EG 20171028 [23509] Upd
        bool IFixTradeCaptureReport.TradedRegulatoryTimestampsSpecified
        {
            set { this.TrdRegTSSpecified = value; }
            get { return this.TrdRegTSSpecified; }
        }

        // EG 20240227 [WI855] New
        IFixRegulatoryTradeIDGrp[] IFixTradeCaptureReport.RegulatoryTradeIDGrp
        {
            set { this.RegTrdID = value.Cast<RegulatoryTradeIDGrp_Block>().ToArray(); }
            get { return this.RegTrdID; }
        }
        // EG 20240227 [WI855] New
        bool IFixTradeCaptureReport.RegulatoryTradeIDGrpSpecified
        {
            set { this.RegTrdIDSpecified = value; }
            get { return this.RegTrdIDSpecified; }
        }
        /// <summary>
        /// Supprime les informations qui font référence à une stratégie
        /// </summary>
        /// FI 20120625 [17864] Add ResetStrategyLeg
        void IFixTradeCaptureReport.ResetStrategyLeg()
        {
            if (this.TrdLegSpecified)
            {
                for (int i = ArrFunc.Count(this.TrdLeg) - 1; -1 < i; i--)
                {
                    if (this.TrdLeg[i].Leg.SecTyp == SecurityTypeEnum.MultilegInstrument)
                    {
                        ReflectionTools.RemoveItemInArray(this, "TrdLeg", i);
                    }
                }
            }
            TrdLegSpecified = ArrFunc.IsFilled(this.TrdLeg);

            if (false == TrdLegSpecified)
            {
                MLegRptTypSpecified = false;
                MLegRptTyp = MultiLegReportingTypeEnum.Single; //valeur sans importance puisque MLegRptTypSpecified = false;
            }
        }
        // EG 20171016 [23452][23509] New
        void IFixTradeCaptureReport.SetTradeRegulatoryTimeStamp(TrdRegTimestampTypeEnum pTrdRegTimestampType, string pValue)
        {
            bool isMustBeApplied = (null == TrdRegTS) || (0 == TrdRegTS.Where(item => item.TypSpecified && (item.Typ == pTrdRegTimestampType) &&
                                                                                        item.tsSpecified && item.ts.Value.Equals(pValue)).Count());
            if (isMustBeApplied)
            {
                TrdRegTimestamps_Block newTimestamp = new TrdRegTimestamps_Block
                {
                    TypSpecified = true,
                    Typ = pTrdRegTimestampType,
                    tsSpecified = true,
                    ts = new EFS_DateTimeOffset(pValue)
                };

                if (ArrFunc.IsFilled(TrdRegTS))
                    TrdRegTS = TrdRegTS.Where(ts => ts.Typ != pTrdRegTimestampType).ToArray();

                if (ArrFunc.IsFilled(TrdRegTS))
                    TrdRegTS = (from item in TrdRegTS select (TrdRegTimestamps_Block)item).Concat(new TrdRegTimestamps_Block[] { newTimestamp }).ToArray();
                else
                    TrdRegTS = new TrdRegTimestamps_Block[] { newTimestamp };

                TrdRegTSSpecified = ArrFunc.IsFilled(TrdRegTS);
            }
        }
        // EG 20171028 [23509] New
        void IFixTradeCaptureReport.RemoveTradeRegulatoryTimeStamp(TrdRegTimestampTypeEnum pTrdRegTimestampType)
        {
            if (null != TrdRegTS)
                TrdRegTS = TrdRegTS.Where(item => (item.Typ != pTrdRegTimestampType)).ToArray();
            TrdRegTSSpecified = ArrFunc.IsFilled(TrdRegTS);
        }

        /// <summary>
        /// Alimentation de la collection RegulatoryTradeIDGrp
        /// avec un ID pour un type donné (RegulatoryTradeIDTypeEnum)
        /// </summary>
        /// <param name="pRegulatoryTradeIDType">Type</param>
        /// <param name="pValue">Valeur</param>
        // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
        void IFixTradeCaptureReport.SetRegulatoryTradeIDGrp(RegulatoryTradeIDTypeEnum pRegulatoryTradeIDType, string pValue)
        {
            bool isMustBeApplied = (null == RegTrdID) || (0 == RegTrdID.Where(item => (item.Typ == pRegulatoryTradeIDType) && item.ID.Equals(pValue)).Count());
            if (isMustBeApplied)
            {
                RegulatoryTradeIDGrp_Block newRegTrdID = new RegulatoryTradeIDGrp_Block
                {
                    TypSpecified = true,
                    Typ = pRegulatoryTradeIDType,
                    ID = pValue
                };

                if (ArrFunc.IsFilled(RegTrdID))
                    RegTrdID = RegTrdID.Where(item => item.Typ != pRegulatoryTradeIDType).ToArray();

                if (ArrFunc.IsFilled(RegTrdID))
                    RegTrdID = (from item in RegTrdID select (RegulatoryTradeIDGrp_Block)item).Concat(new RegulatoryTradeIDGrp_Block[] { newRegTrdID }).ToArray();
                else
                    RegTrdID = new RegulatoryTradeIDGrp_Block[] { newRegTrdID };

                RegTrdIDSpecified = ArrFunc.IsFilled(RegTrdID);
            }
        }
        /// <summary>
        /// Suppression d'un élément de la collection RegulatoryTradeIDGrp
        /// pour un type donné  (RegulatoryTradeIDTypeEnum)
        /// </summary>
        /// <param name="pRegulatoryTradeIDType">Type</param>
        // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
        void IFixTradeCaptureReport.RemoveRegulatoryTradeIDGrp(RegulatoryTradeIDTypeEnum pRegulatoryTradeIDType)
        {
            if (null != RegTrdID)
                RegTrdID = RegTrdID.Where(item => (item.Typ != pRegulatoryTradeIDType)).ToArray();
            RegTrdIDSpecified = ArrFunc.IsFilled(RegTrdID);
        }

        // EG 20240227 [WI858] Trade input : New data TRDID (Market Transaction ID)
        void IFixTradeCaptureReport.SetMarketTransactionId(string pValue)
        {
            TrdID = pValue;
            TrdIDSpecified = StrFunc.IsFilled(pValue);
        }
        #endregion IFixTradeCaptureReport Members
        #region Methods
        public static object INIT_Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_SettlCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        public static object INIT_LastMkt(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion TradeCaptureReport_message
}
