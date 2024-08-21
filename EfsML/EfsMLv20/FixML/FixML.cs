#region Using Directives
using System;
using System.Reflection;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML.Enum;

using FixML.Enum;
using FixML.v44.Enum;
#endregion Using Directives

#region Revision
/// <revision>
///     <version>1.2.0</version><date>20071003</date><author>EG</author>
///     <comment>
///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent for all method DisplayArray (used to determine REGEX type for derived classes
///     </comment>
/// </revision>
#endregion Revision

namespace FixML.v44
{
    #region Abstract_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExecutionReport_message))]
    public class Abstract_message : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("Hdr", Namespace = "http://www.fixprotocol.org/FIXML-4-4",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Standard Header")]
        [DictionaryGUI(Page = "StandardHeader")]
        public MessageHeader Hdr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool HdrSpecified;
		#endregion Members
	}
    #endregion Abstract_message

    #region BaseHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageHeader))]
    public class BaseHeader : ItemGUI
    {
        #region Attributes accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SID
        {
            set { sID = new EFS_String(value); }
            get { return (null != sID) ? sID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TID
        {
            set { tID = new EFS_String(value); }
            get { return (null != tID) ? tID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OBID
        {
            set { obID = new EFS_String(value); }
            get { return (null != obID) ? obID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string D2ID
        {
            set { d2ID = new EFS_String(value); }
            get { return (null != d2ID) ? d2ID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SSub
        {
            set { sSub = new EFS_String(value); }
            get { return (null != sSub) ? sSub.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SLoc
        {
            set { sLoc = new EFS_String(value); }
            get { return (null != sLoc) ? sLoc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TSub
        {
            set { tSub = new EFS_String(value); }
            get { return (null != tSub) ? tSub.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TLoc
        {
            set { tLoc = new EFS_String(value); }
            get { return (null != tLoc) ? tLoc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OBSub
        {
            set { obSub = new EFS_String(value); }
            get { return (null != obSub) ? obSub.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OBLoc
        {
            set { obLoc = new EFS_String(value); }
            get { return (null != obLoc) ? obLoc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string D2Sub
        {
            set { d2Sub = new EFS_String(value); }
            get { return (null != d2Sub) ? d2Sub.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string D2Loc
        {
            set { d2Loc = new EFS_String(value); }
            get { return (null != d2Loc) ? d2Loc.Value : null; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SntSpecified
        {
            set { sntSpecified = value; }
            get { return sntSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime Snt
        {
            set { snt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != snt) ? snt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OrigSntSpecified
        {
            set { origSntSpecified = value; }
            get { return origSntSpecified; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime OrigSnt
        {
            set { origSnt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != origSnt) ? origSnt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MsgEncd
        {
            set { msgEncd = new EFS_String(value); }
            get { return (null != msgEncd) ? msgEncd.Value : null; }
        }
        #endregion Attributes accessors
        #region GUI
		[System.Xml.Serialization.XmlElementAttribute("Hop", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Hop")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Hop", MinItem = 0)]
        [DictionaryGUI(Page = "field_description", Anchor = "HopCompID")]
        public Hop[] Hop;
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "CompID", IsVisible = false, IsGroup = true)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [DictionaryGUI(Page = "field_description", Anchor = "SenderCompID")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Sender", LblWidth = 60, Width = 100)]
        public EFS_String sID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Target", LblWidth = 60, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "TargetCompID")]
        public EFS_String tID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "OnBehalfOf", LblWidth = 60, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "OnBehalfOfCompID")]
        public EFS_String obID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "DeliverTo", LblWidth = 60, Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "DeliverToCompID")]
        public EFS_String d2ID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "CompID")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "SubID", IsVisible = false, IsGroup = true)]
        [DictionaryGUI(Page = "field_description", Anchor = "SenderSubID")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Sender", LblWidth = 60, Width = 100)]
        public EFS_String sSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Target", LblWidth = 60, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "TargetSubID")]
        public EFS_String tSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "OnBehalfOf", LblWidth = 60, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "OnBehalfOfSubID")]
        public EFS_String obSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "DeliverTo", LblWidth = 50, Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "DeliverToSubID")]
        public EFS_String d2Sub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "SubID")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "LocationID", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Sender", LblWidth = 60, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "SenderLocationID")]
        public EFS_String sLoc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Target", LblWidth = 60, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "TargetLocationID")]
        public EFS_String tLoc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "OnBehalfOf", LblWidth = 60, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "OnBehalfOfLocationID")]
        public EFS_String obLoc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "DeliverTo", LblWidth = 60, Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "DeliverToLocationID")]
        public EFS_String d2Loc;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "LocationID")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Other characteristics of the message", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Encoding", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "MessageEncoding")]
        public EFS_String msgEncd;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Duplicate", Width = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "PossDupFlag")]
        public PossDupFlagEnum PosDup;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Resend", Width = 150, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "PossResend")]
        public PossResendEnum PosRsnd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sending Time")]
        [DictionaryGUI(Page = "field_description", Anchor = "SendingTime")]
        public EFS_Date snt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sntSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Original Sending Time")]
        [DictionaryGUI(Page = "field_description", Anchor = "OrigSendingTime")]
        public EFS_Date origSnt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool origSntSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Other characteristics of the message")]
        public bool FillerGUI;
        #endregion GUI
        #region Constructors
        public BaseHeader()
        {
            snt = new EFS_Date();
            origSnt = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion BaseHeader

    #region CommissionData
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class CommissionData : ItemGUI
    {
        #region Attributes accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Comm
        {
            set { comm = new EFS_Decimal(value); }
            get { return (null != comm) ? comm.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CommSpecified
        {
            set { commSpecified = value; }
            get { return commSpecified; }
        }
        #endregion Attributes accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "CommType")]
        public CommTypeEnum CommTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "Commission")]
        public EFS_Decimal comm;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "CommCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fund based renewal commission is to be waived")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "FundRenewWaiv")]
        public YesNoEnum FundRenewWaiv;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CommTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool FundRenewWaivSpecified;
        #endregion Specified fields

        #region Constructors
        public CommissionData()
        {
            Ccy = string.Empty;
        }
        #endregion Constructors

        public static object _Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
    }
    #endregion CommissionData
    #region ContAmtGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class ContAmtGrp : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal ContAmtValu
        {
            set { contAmtValu = new EFS_Decimal(value); }
            get { return (null != contAmtValu) ? contAmtValu.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContAmtValuSpecified
        {
            set { contAmtValuSpecified = value; }
            get { return contAmtValuSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "ContAmtTyp")]
        public ContAmtTypeEnum ContAmtTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
        [DictionaryGUI(Page = "field_description", Anchor = "ContAmtValue")]
        public EFS_Decimal contAmtValu;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "ContAmtCurr")]
        public string ContAmtCurr;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ContAmtTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contAmtValuSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ContAmtCurrSpecified;
        #endregion Specified fields
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array

        #region Constructors
        public ContAmtGrp()
        {
            ContAmtCurr = string.Empty;
        }
        #endregion Constructors

        public static object _ContAmtCurr(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
    }
    #endregion ContAmtGrp
    #region ContraGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class ContraGrp : ItemGUI, IEFS_Array
    {
        #region Attributes accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CntraBrkr
        {
            set { cntraBrkr = new EFS_String(value); }
            get { return (null != cntraBrkr) ? cntraBrkr.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CntraTrdr
        {
            set { cntraTrdr = new EFS_String(value); }
            get { return (null != cntraTrdr) ? cntraTrdr.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal CntraTrdQty
        {
            set { cntraTrdQty = new EFS_Decimal(value); }
            get { return (null != cntraTrdQty) ? cntraTrdQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CntraTrdQtySpecified
        {
            set { cntraTrdQtySpecified = value; }
            get { return cntraTrdQtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime CntraTrdTm
        {
            set { cntraTrdTm = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != cntraTrdTm) ? cntraTrdTm.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CntraTrdTmSpecified
        {
            set { cntraTrdTmSpecified = value; }
            get { return cntraTrdTmSpecified; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CntraLegRefID
        {
            set { cntraLegRefID = new EFS_String(value); }
            get { return (null != cntraLegRefID) ? cntraLegRefID.Value : null; }
        }
        #endregion Attributes accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Broker", Width = 200)]
        [DictionaryGUI(Page = "field_description", Anchor = "ContraBroker")]
        public EFS_String cntraBrkr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Trader (e.g badge number)", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "ContraTrader")]
        public EFS_String cntraTrdr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Leg Id", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "ContraLegRefID")]
        public EFS_String cntraLegRefID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Quantity", Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "ContraTradeQty")]
        public EFS_Decimal cntraTrdQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade time")]
        [DictionaryGUI(Page = "field_description", Anchor = "ContraTradeTime")]
        public EFS_Date cntraTrdTm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cntraTrdQtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cntraTrdTmSpecified;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
        #region Constructors
        public ContraGrp()
        {
            cntraTrdTm = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion ContraGrp

    #region DiscretionInstructions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class DiscretionInstructions : ItemGUI
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal OfstValu
        {
            set { ofstValu = new EFS_Decimal(value); }
            get { return (null != ofstValu) ? ofstValu.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OfstValuSpecified
        {
            set { ofstValuSpecified = value; }
            get { return ofstValuSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Relative to")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "DiscretionInst")]
        public DiscretionInstEnum DsctnInst;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount added to the related to")]
        [DictionaryGUI(Page = "field_description", Anchor = "DiscretionOffsetValue")]
        public EFS_Decimal ofstValu;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Discretionary price type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "DiscretionMoveType")]
        public MoveTypeEnum MoveTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Discretion Offset Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "DiscretionOffsetType")]
        public OffsetTypeEnum OfstTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Limit type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "DiscretionLimitType")]
        public LimitTypeEnum LmtTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "RoundDirection")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "DiscretionRoundDirection")]
        public PegRoundDirectionEnum RndDir;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Scope")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "DiscretionScope")]
        public ScopeEnum Scope;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DsctnInstSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ofstValuSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MoveTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OfstTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LmtTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RndDirSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ScopeSpecified;
        #endregion Specified fields
    }
    #endregion DiscretionInstructions

    #region EvntGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class EvntGrp : ItemGUI, IEFS_Array
    {
        #region Attribute accessors
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Dt
        {
            set { dt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != dt) ? dt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DtSpecified
        {
            set { dtSpecified = value; }
            get { return dtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Px
        {
            set { px = new EFS_Decimal(value); }
            get { return (null != px) ? px.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxSpecified
        {
            set { pxSpecified = value; }
            get { return pxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt
        {
            set { txt = new EFS_String(value); }
            get { return (null != txt) ? txt.Value : null; }
        }
        #endregion Attribute accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Type")]
        [DictionaryGUI(Page = "field_description", Anchor = "EventType")]
        public FixML.v44.Enum.EventTypeEnum EventTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Text", Width = 300)]
        [DictionaryGUI(Page = "field_description", Anchor = "EventText")]
        public EFS_String txt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "EventDate")]
        public EFS_Date dt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "EventPx")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
        #region Constructors
        public EvntGrp()
        {
            dt = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion EvntGrp
    #region ExecutionReport_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("ExecRpt", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class ExecutionReport_message : Abstract_message
    {
		[System.Xml.Serialization.XmlElementAttribute("Pty", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Partie", IsChild = true, IsChildVisible = true, MinItem = 0)]
        [DictionaryGUI(Page = "Parties")]
        public Parties[] Pty;
		[System.Xml.Serialization.XmlElementAttribute("Contra", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Counterparts brokers details")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Counterpart Broker details", IsChild = true, IsChildVisible = true, MinItem = 0)]
        [DictionaryGUI(Page = "field_description", Anchor = "ContraBroker")]
        public ContraGrp[] Contra;
		[System.Xml.Serialization.XmlElementAttribute("Instrmt", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Instrument", IsVisible = false)]
        [DictionaryGUI(Page = "Instrument")]
        public Instrument Instrmt;
		[System.Xml.Serialization.XmlElementAttribute("FinDetls", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Instrument")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Financing Details")]
        [DictionaryGUI(Page = "FinancingDetails")]
        public FinancingDetails FinDetls;
        [System.Xml.Serialization.XmlElementAttribute("Undly",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying instruments")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying instrument", MinItem = 0)]
        [DictionaryGUI(Page = "UnderlyingInstrument")]
        public UnderlyingInstrument[] Undly;
        [System.Xml.Serialization.XmlElementAttribute("Stip",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stipulations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stipulations", MinItem = 0)]
        [DictionaryGUI(Page = "Stipulations")]
        public Stipulations[] Stip;
		[System.Xml.Serialization.XmlElementAttribute("OrdQty", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quantity")]
        [DictionaryGUI(Page = "OrderQtyData")]
        public OrderQtyData OrdQty;
		[System.Xml.Serialization.XmlElementAttribute("PegInstr", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Peg price instructions")]
        [DictionaryGUI(Page = "PegInstructions")]
        public PegInstructions PegInstr;
		[System.Xml.Serialization.XmlElementAttribute("DiscInstr", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Specific discretion instructions")]
        [DictionaryGUI(Page = "DiscretionInstructions")]
        public DiscretionInstructions DiscInstr;
		[System.Xml.Serialization.XmlElementAttribute("Comm", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Commissions")]
        [DictionaryGUI(Page = "CommissionData")]
        public CommissionData Comm;
		[System.Xml.Serialization.XmlElementAttribute("SprdBnchmkCurve", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Associated price curves")]
        [DictionaryGUI(Page = "SpreadOrBenchmarkCurveData")]
        public SpreadOrBenchmarkCurveData SprdBnchmkCurve;
		[System.Xml.Serialization.XmlElementAttribute("Yield", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Yields")]
        [DictionaryGUI(Page = "YieldData")]
        public YieldData Yield;
        [System.Xml.Serialization.XmlElementAttribute("ContAmt",Order=13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contractual amounts")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contractual amount", MinItem = 0)]
        public ContAmtGrp[] ContAmt;
        [System.Xml.Serialization.XmlElementAttribute("Exec",Order=14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Legs")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Leg", IsChild = true, IsChildVisible = true, MinItem = 0)]
        [DictionaryGUI(Page = "InstrumentLeg")]
        public InstrmtLegExecGrp[] Exec;
        [System.Xml.Serialization.XmlElementAttribute("MiscFees",Order=15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Miscellaneous fees")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Miscellaneous fee", MinItem = 0)]
        public MiscFeesGrp[] MiscFees;
        #region Attributes accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID
        {
            set { ordID = new EFS_String(value); }
            get { return (null != ordID) ? ordID.Value : null; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID2
        {
            set { ordID2 = new EFS_String(value); }
            get { return (null != ordID2) ? ordID2.Value : null; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID2
        {
            set { id2 = new EFS_String(value); }
            get { return (null != id2) ? id2.Value : null; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecID2
        {
            set { execID2 = new EFS_String(value); }
            get { return (null != execID2) ? execID2.Value : null; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return (null != id) ? id.Value : null; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrigID
        {
            set { origID = new EFS_String(value); }
            get { return (null != origID) ? origID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LnkID
        {
            set { lnkID = new EFS_String(value); }
            get { return (null != lnkID) ? lnkID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RspID
        {
            set { rspID = new EFS_String(value); }
            get { return (null != rspID) ? rspID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StatReqID
        {
            set { statReqID = new EFS_String(value); }
            get { return (null != statReqID) ? statReqID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqID
        {
            set { reqID = new EFS_String(value); }
            get { return (null != reqID) ? reqID.Value : null; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TotNumRpts
        {
            set { totNumRpts = new EFS_Integer(value); }
            get { return (null != totNumRpts) ? totNumRpts.Value : null; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotNumRptsSpecified
        {
            set { totNumRptsSpecified = value; }
            get { return totNumRptsSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool LastRptReqed
        {
            set { lastRptReqed = new EFS_Boolean(value); }
            get { return (null != lastRptReqed) ? lastRptReqed.BoolValue : Convert.ToBoolean(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastRptReqedSpecified
        {
            set { lastRptReqedSpecified = value; }
            get { return lastRptReqedSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime OrignDt
        {
            set { orignDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != orignDt) ? orignDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrignDtSpecified
        {
            set { orignDtSpecified = value; }
            get { return orignDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ListID
        {
            set { listID = new EFS_String(value); }
            get { return (null != listID) ? listID.Value : null; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CrssID
        {
            set { crssID = new EFS_String(value); }
            get { return (null != crssID) ? crssID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrigCrssID
        {
            set { origCrssID = new EFS_String(value); }
            get { return (null != origCrssID) ? origCrssID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecID
        {
            set { execID = new EFS_String(value); }
            get { return (null != execID) ? execID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecRefID
        {
            set { execRefID = new EFS_String(value); }
            get { return (null != execRefID) ? execRefID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct
        {
            set { acct = new EFS_String(value); }
            get { return (null != acct) ? acct.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime SettlDt
        {
            set { settlDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != settlDt) ? settlDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDtSpecified
        {
            set { settlDtSpecified = value; }
            get { return settlDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Px
        {
            set { px = new EFS_Decimal(value); }
            get { return (null != px) ? px.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxSpecified
        {
            set { pxSpecified = value; }
            get { return pxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal StopPx
        {
            set { stopPx = new EFS_Decimal(value); }
            get { return (null != stopPx) ? stopPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StopPxSpecified
        {
            set { stopPxSpecified = value; }
            get { return stopPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal PeggedPx
        {
            set { peggedPx = new EFS_Decimal(value); }
            get { return (null != peggedPx) ? peggedPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PeggedPxSpecified
        {
            set { peggedPxSpecified = value; }
            get { return peggedPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal DsctnPx
        {
            set { dsctnPx = new EFS_Decimal(value); }
            get { return (null != dsctnPx) ? dsctnPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DsctnPxSpecified
        {
            set { dsctnPxSpecified = value; }
            get { return dsctnPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TgtStrategyParameters
        {
            set { tgtStrategyParameters = new EFS_String(value); }
            get { return (null != tgtStrategyParameters) ? tgtStrategyParameters.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal ParticipationRt
        {
            set { participationRt = new EFS_Decimal(value); }
            get { return (null != participationRt) ? participationRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ParticipationRtSpecified
        {
            set { participationRtSpecified = value; }
            get { return participationRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal TgtStrategyPerformance
        {
            set { tgtStrategyPerformance = new EFS_Decimal(value); }
            get { return (null != tgtStrategyPerformance) ? tgtStrategyPerformance.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TgtStrategyPerformanceSpecified
        {
            set { tgtStrategyPerformanceSpecified = value; }
            get { return tgtStrategyPerformanceSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ComplianceID
        {
            set { complianceID = new EFS_String(value); }
            get { return (null != complianceID) ? complianceID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime EfctvTm
        {
            set { efctvTm = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != efctvTm) ? efctvTm.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EfctvTmSpecified
        {
            set { efctvTmSpecified = value; }
            get { return efctvTmSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime ExpireDt
        {
            set { expireDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != expireDt) ? expireDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpireDtSpecified
        {
            set { expireDtSpecified = value; }
            get { return expireDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime ExpireTm
        {
            set { expireTm = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != expireTm) ? expireTm.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpireTmSpecified
        {
            set { expireTmSpecified = value; }
            get { return expireTmSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal LastQty
        {
            set { lastQty = new EFS_Decimal(value); }
            get { return (null != lastQty) ? lastQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastQtySpecified
        {
            set { lastQtySpecified = value; }
            get { return lastQtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal UndLastQty
        {
            set { undLastQty = new EFS_Decimal(value); }
            get { return (null != undLastQty) ? undLastQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UndLastQtySpecified
        {
            set { undLastQtySpecified = value; }
            get { return undLastQtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal LastPx
        {
            set { lastPx = new EFS_Decimal(value); }
            get { return (null != lastPx) ? lastPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastPxSpecified
        {
            set { lastPxSpecified = value; }
            get { return lastPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal UndLastPx
        {
            set { undLastPx = new EFS_Decimal(value); }
            get { return (null != undLastPx) ? undLastPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UndLastPxSpecified
        {
            set { undLastPxSpecified = value; }
            get { return undLastPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal LastParPx
        {
            set { lastParPx = new EFS_Decimal(value); }
            get { return (null != lastParPx) ? lastParPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastParPxSpecified
        {
            set { lastParPxSpecified = value; }
            get { return lastParPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal LastSpotRt
        {
            set { lastSpotRt = new EFS_Decimal(value); }
            get { return (null != lastSpotRt) ? lastSpotRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastSpotRtSpecified
        {
            set { lastSpotRtSpecified = value; }
            get { return lastSpotRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal LastFwdPnts
        {
            set { lastFwdPnts = new EFS_Decimal(value); }
            get { return (null != lastFwdPnts) ? lastFwdPnts.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastFwdPntsSpecified
        {
            set { lastFwdPntsSpecified = value; }
            get { return lastFwdPntsSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesID
        {
            set { sesID = new EFS_String(value); }
            get { return (null != sesID) ? sesID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesSub
        {
            set { sesSub = new EFS_String(value); }
            get { return (null != sesSub) ? sesSub.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TmBkt
        {
            set { tmBkt = new EFS_String(value); }
            get { return (null != tmBkt) ? tmBkt.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal LeavesQty
        {
            set { leavesQty = new EFS_Decimal(value); }
            get { return (null != leavesQty) ? leavesQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal CumQty
        {
            set { cumQty = new EFS_Decimal(value); }
            get { return (null != cumQty) ? cumQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal AvgPx
        {
            set { avgPx = new EFS_Decimal(value); }
            get { return (null != avgPx) ? avgPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal DayOrdQty
        {
            set { dayOrdQty = new EFS_Decimal(value); }
            get { return (null != dayOrdQty) ? dayOrdQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DayOrdQtySpecified
        {
            set { dayOrdQtySpecified = value; }
            get { return dayOrdQtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal DayCumQty
        {
            set { dayCumQty = new EFS_Decimal(value); }
            get { return (null != dayCumQty) ? dayCumQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DayCumQtySpecified
        {
            set { dayCumQtySpecified = value; }
            get { return dayCumQtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal DayAvgPx
        {
            set { dayAvgPx = new EFS_Decimal(value); }
            get { return (null != dayAvgPx) ? dayAvgPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DayAvgPxSpecified
        {
            set { dayAvgPxSpecified = value; }
            get { return dayAvgPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime TrdDt
        {
            set { trdDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != trdDt) ? trdDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdDtSpecified
        {
            set { trdDtSpecified = value; }
            get { return trdDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime TxnTm
        {
            set { txnTm = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != txnTm) ? txnTm.DateValue : Convert.ToDateTime(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TxnTmSpecified
        {
            set { txnTmSpecified = value; }
            get { return txnTmSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal GrossTrdAmt
        {
            set { grossTrdAmt = new EFS_Decimal(value); }
            get { return (null != grossTrdAmt) ? grossTrdAmt.DecValue : Convert.ToDecimal(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GrossTrdAmtSpecified
        {
            set { grossTrdAmtSpecified = value; }
            get { return grossTrdAmtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int NumDaysInt
        {
            set { numDaysInt = new EFS_Integer(value); }
            get { return (null != numDaysInt) ? numDaysInt.IntValue : Convert.ToInt32(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NumDaysIntSpecified
        {
            set { numDaysIntSpecified = value; }
            get { return numDaysIntSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime ExDt
        {
            set { exDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != exDt) ? exDt.DateValue : Convert.ToDateTime(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExDtSpecified
        {
            set { exDtSpecified = value; }
            get { return exDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal AcrdIntRt
        {
            set { acrdIntRt = new EFS_Decimal(value); }
            get { return (null != acrdIntRt) ? acrdIntRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AcrdIntRtSpecified
        {
            set { acrdIntRtSpecified = value; }
            get { return acrdIntRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal AcrdIntAmt
        {
            set { acrdIntAmt = new EFS_Decimal(value); }
            get { return (null != acrdIntAmt) ? acrdIntAmt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AcrdIntAmtSpecified
        {
            set { acrdIntAmtSpecified = value; }
            get { return acrdIntAmtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal IntAtMat
        {
            set { intAtMat = new EFS_Decimal(value); }
            get { return (null != intAtMat) ? intAtMat.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IntAtMatSpecified
        {
            set { intAtMatSpecified = value; }
            get { return intAtMatSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal EndAcrdIntAmt
        {
            set { endAcrdIntAmt = new EFS_Decimal(value); }
            get { return (null != endAcrdIntAmt) ? endAcrdIntAmt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EndAcrdIntAmtSpecified
        {
            set { endAcrdIntAmtSpecified = value; }
            get { return endAcrdIntAmtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal StartCsh
        {
            set { startCsh = new EFS_Decimal(value); }
            get { return (null != startCsh) ? startCsh.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartCshSpecified
        {
            set { startCshSpecified = value; }
            get { return startCshSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal EndCsh
        {
            set { endCsh = new EFS_Decimal(value); }
            get { return (null != endCsh) ? endCsh.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EndCshSpecified
        {
            set { endCshSpecified = value; }
            get { return endCshSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime BasisFeatureDt
        {
            set { basisFeatureDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != basisFeatureDt) ? basisFeatureDt.DateValue : Convert.ToDateTime(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BasisFeatureDtSpecified
        {
            set { basisFeatureDtSpecified = value; }
            get { return basisFeatureDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal BasisFeaturePx
        {
            set { basisFeaturePx = new EFS_Decimal(value); }
            get { return (null != basisFeaturePx) ? basisFeaturePx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BasisFeaturePxSpecified
        {
            set { basisFeaturePxSpecified = value; }
            get { return basisFeaturePxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Concession
        {
            set { concession = new EFS_Decimal(value); }
            get { return (null != concession) ? concession.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConcessionSpecified
        {
            set { concessionSpecified = value; }
            get { return concessionSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal TotTakedown
        {
            set { totTakedown = new EFS_Decimal(value); }
            get { return (null != totTakedown) ? totTakedown.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotTakedownSpecified
        {
            set { totTakedownSpecified = value; }
            get { return totTakedownSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal NetMny
        {
            set { netMny = new EFS_Decimal(value); }
            get { return (null != netMny) ? netMny.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NetMnySpecified
        {
            set { netMnySpecified = value; }
            get { return netMnySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal SettlCurrAmt
        {
            set { settlCurrAmt = new EFS_Decimal(value); }
            get { return (null != settlCurrAmt) ? settlCurrAmt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlCurrAmtSpecified
        {
            set { settlCurrAmtSpecified = value; }
            get { return settlCurrAmtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal SettlCurrFxRt
        {
            set { settlCurrFxRt = new EFS_Decimal(value); }
            get { return (null != settlCurrFxRt) ? settlCurrFxRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlCurrFxRtSpecified
        {
            set { settlCurrFxRtSpecified = value; }
            get { return settlCurrFxRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal MinQty
        {
            set { minQty = new EFS_Decimal(value); }
            get { return (null != minQty) ? minQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinQtySpecified
        {
            set { minQtySpecified = value; }
            get { return minQtySpecified; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal MaxFloor
        {
            set { maxFloor = new EFS_Decimal(value); }
            get { return (null != maxFloor) ? maxFloor.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaxFloorSpecified
        {
            set { maxFloorSpecified = value; }
            get { return maxFloorSpecified; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal MaxShow
        {
            set { maxShow = new EFS_Decimal(value); }
            get { return (null != maxShow) ? maxShow.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaxShowSpecified
        {
            set { maxShowSpecified = value; }
            get { return maxShowSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt
        {
            set { comment = new EFS_String(value); }
            get { return (null != comment) ? comment.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen
        {
            set { encTxtLen = new EFS_PosInteger(value); }
            get { return (null != encTxtLen) ? encTxtLen.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt
        {
            set { encTxt = new EFS_String(value); }
            get { return (null != encTxt) ? encTxt.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime SettlDt2
        {
            set { settlDt2 = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != settlDt2) ? settlDt2.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDt2Specified
        {
            set { settlDt2Specified = value; }
            get { return settlDt2Specified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Qty2
        {
            set { qty2 = new EFS_Decimal(value); }
            get { return (null != qty2) ? qty2.DecValue : Convert.ToDecimal(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Qty2Specified
        {
            set { qty2Specified = value; }
            get { return qty2Specified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal LastFwdPnts2
        {
            set { lastFwdPnts2 = new EFS_Decimal(value); }
            get { return (null != lastFwdPnts2) ? lastFwdPnts2.DecValue : Convert.ToDecimal(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastFwdPnts2Specified
        {
            set { lastFwdPnts2Specified = value; }
            get { return lastFwdPnts2Specified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RegistID
        {
            set { registID = new EFS_String(value); }
            get { return (null != registID) ? registID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Designation
        {
            set { designation = new EFS_String(value); }
            get { return (null != designation) ? designation.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime TransBkdTm
        {
            set { transBkdTm = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != transBkdTm) ? transBkdTm.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TransBkdTmSpecified
        {
            set { transBkdTmSpecified = value; }
            get { return transBkdTmSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime ExecValuationPoint
        {
            set { execValuationPoint = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != execValuationPoint) ? execValuationPoint.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecValuationPointSpecified
        {
            set { execValuationPointSpecified = value; }
            get { return execValuationPointSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal ExecPxAdjment
        {
            set { execPxAdjment = new EFS_Decimal(value); }
            get { return (null != execPxAdjment) ? execPxAdjment.DecValue : Convert.ToDecimal(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecPxAdjmentSpecified
        {
            set { execPxAdjmentSpecified = value; }
            get { return execPxAdjmentSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal PxImprvmnt
        {
            set { pxImprvmnt = new EFS_Decimal(value); }
            get { return (null != pxImprvmnt) ? pxImprvmnt.DecValue : Convert.ToDecimal(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxImprvmntSpecified
        {
            set { pxImprvmntSpecified = value; }
            get { return pxImprvmntSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool CopyMsgInd
        {
            set { copyMsgInd = new EFS_Boolean(value); }
            get { return (null != copyMsgInd) ? copyMsgInd.BoolValue : Convert.ToBoolean(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CopyMsgIndSpecified
        {
            set { copyMsgIndSpecified = value; }
            get { return copyMsgIndSpecified; }
        }
        #endregion Attributes accessors
        #region GUI
        #region Order characteristics and execution data
        #region Ids
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Order characteristics and execution data", IsVisible = false, IsGroup = true)]
        public bool FillerGUI;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "IDs", IsVisible = false, IsGroup = true)]
        [ControlGUI(Name = "Order ID", LblWidth = 100, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "OrderID")]
        public EFS_String ordID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Secondary Order", LblWidth = 140, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "SecondaryOrderID")]
        public EFS_String ordID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Secondary ExecOrder", LblWidth = 140, Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "SecondaryExecID")]
        public EFS_String execID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "ClOrder", LblWidth = 100, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "ClOrdID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Original ClOrder", LblWidth = 140, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "OrigClOrdID")]
        public EFS_String origID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Secondary ClOrder", LblWidth = 140, Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "SecondaryClOrdID")]
        public EFS_String id2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "ClOrder Link", LblWidth = 140, Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "ClOrdLinkID")]
        public EFS_String lnkID;
        #endregion Ids
        #region Message details
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "IDs")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Message details", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Reference for Quote response")]
        [DictionaryGUI(Page = "field_description", Anchor = "QuoteRespID")]
        public EFS_String rspID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Order Status Request")]
        [DictionaryGUI(Page = "field_description", Anchor = "OrdStatusReqID")]
        public EFS_String statReqID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Mass Status Request", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "MassStatusReqID")]
        public EFS_String reqID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total number of reports")]
        [DictionaryGUI(Page = "field_description", Anchor = "TotNumReports")]
        public EFS_Integer totNumRpts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Report Requested")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastRptRequested")]
        public EFS_Boolean lastRptReqed;
        #endregion Message details
        #region References
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Message details")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "References", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Origination Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "TradeOriginationDate")]
        public EFS_Date orignDt;
        #region Cross order
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cross order", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Cross order", Width = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "CrossID")]
        public EFS_String crssID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Original cross order", Width = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "OrigCrossID")]
        public EFS_String origCrssID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cross type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "CrossType")]
        public CrossTypeEnum CrssTyp;
        #endregion Cross order
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cross order")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "List ID", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "ListID")]
        public EFS_String listID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Execution message ID", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "ExecID")]
        public EFS_String execID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Execution message (Cancel or correct trade)", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "ExecRefID")]
        public EFS_String execRefID;
        #endregion References
        #region Status
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "References")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Status", IsVisible = false, IsGroup = true)]
        [ControlGUI(Name = "Execution type", Width = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "ExecType")]
        public ExecTypeEnum ExecTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Status order", Width = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "OrdStatus")]
        public OrdStatusEnum Stat;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Working Indicator")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "WorkingIndicator")]
        public YesNoEnum WorkingInd;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reason for order rejection")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "OrdRejReason")]
        public OrdRejReasonEnum RejRsn;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reason for restated message")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "ExecRestatementReason")]
        public ExecRestatementReasonEnum ExecRstmtRsn;
        #endregion Status
        #region Order description
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Status")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order description", IsVisible = false, IsGroup = true)]
        public bool Filler2GUI;
        #region Accounts
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accounts", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Mnemonic", Width = 300)]
        [DictionaryGUI(Page = "field_description", Anchor = "Account")]
        public EFS_String acct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account ID source")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "AcctIDSource")]
        public AcctIDSourceEnum AcctIDSrc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "AccountType")]
        public AccountTypeEnum AcctTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Day Booking")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "DayBookingInst")]
        public DayBookingInstEnum DayBkngInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Booking Unit")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "BookingUnit")]
        public BookingUnitEnum BkngUnit;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Preallocation method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PreallocMethod")]
        public PreallocMethodEnum PreallocMeth;
        #endregion Accounts
        #region Settlement data
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accounts")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement data", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "SettlType")]
        public SettlTypeEnum SettlTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "SettlDate")]
        public EFS_Date settlDt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Margin")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "CashMargin")]
        public CashMarginEnum CshMgn;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clearing Fee Indicator")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "ClearingFeeIndicator")]
        public ClearingFeeIndicatorEnum ClrFeeInd;
        #endregion Settlement data
        #region Order characteristics
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement data")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order characteristics", IsVisible = false, IsGroup = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side")]
        [DictionaryGUI(Page = "field_description", Anchor = "Side")]
        public SideEnum Side;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "QtyType")]
        public QtyTypeEnum QtyTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "OrdType")]
        public OrdTypeEnum Typ;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PriceType")]
        public PriceTypeEnum PxTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "Price")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stop price")]
        [DictionaryGUI(Page = "field_description", Anchor = "StopPx")]
        public EFS_Decimal stopPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Pegged price")]
        [DictionaryGUI(Page = "field_description", Anchor = "PeggedPrice")]
        public EFS_Decimal peggedPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Discretion price")]
        [DictionaryGUI(Page = "field_description", Anchor = "DiscretionPrice")]
        public EFS_Decimal dsctnPx;
        #endregion Order characteristics
        #region Strategy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order characteristics")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strategy", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Target strategy")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "TargetStrategy")]
        public TargetStrategyEnum TgtStrategy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parameters", Width = 300)]
        [DictionaryGUI(Page = "field_description", Anchor = "TargetStrategyParameters")]
        public EFS_String tgtStrategyParameters;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Participation Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "ParticipationRate")]
        public EFS_Decimal participationRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Target strategy performance")]
        [DictionaryGUI(Page = "field_description", Anchor = "TargetStrategyPerformance")]
        public EFS_Decimal tgtStrategyPerformance;
        #endregion Strategy
        #region Currency
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strategy")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "Currency")]
        public string Ccy;
        #endregion Currency
        #region External ID
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "External ID", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Compliance ID")]
        [DictionaryGUI(Page = "field_description", Anchor = "ComplianceID")]
        public EFS_String complianceID;
        #endregion External ID
        #region Times and dates data
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "External ID")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Times and dates data", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Solicited Flag")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "SolicitedFlag")]
        public YesNoEnum SolFlag;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time In Force")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "TimeInForce")]
        public TimeInForceEnum TmInForce;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Time")]
        [DictionaryGUI(Page = "field_description", Anchor = "EffectiveTime")]
        public EFS_Date efctvTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expire Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "ExpireDate")]
        public EFS_Date expireDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expire Time")]
        [DictionaryGUI(Page = "field_description", Anchor = "ExpireTime")]
        public EFS_Date expireTm;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exec Instruction")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "ExecInst")]
        public ExecInstEnum ExecInst;
        #endregion Times and dates data
        #region Capacity Data
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Times and dates data")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Capacity data", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Capacity")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "OrderCapacity")]
        public OrderCapacityEnum Cpcty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Restrictions")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "OrderRestrictions")]
        public OrderRestrictionsEnum Rstctions;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Customer Order Capacity")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "CustOrderCapacity")]
        public CustOrderCapacityEnum CustCpcty;
        #endregion Capacity Data
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Capacity Data")]
        public bool Filler3GUI;
        #endregion Order description
        #region Execution Details
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order description")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution details", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last quantity")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastQty")]
        public EFS_Decimal lastQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Last Quantity")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingLastQty")]
        public EFS_Decimal undLastQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastPx")]
        public EFS_Decimal lastPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Last Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingLastPx")]
        public EFS_Decimal undLastPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Price expressed in percent-of-par")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastParPx")]
        public EFS_Decimal lastParPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Spot Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastSpotRate")]
        public EFS_Decimal lastSpotRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Forward Points")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastForwardPoints")]
        public EFS_Decimal lastFwdPnts;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Capacity")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastCapacity")]
        public LastCapacityEnum LastCpcty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Market", Width = 500)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastMkt")]
        public string LastMkt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trading Session ID", LblWidth = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "TradingSessionID")]
        public EFS_String sesID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trading Session SubID", LblWidth = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "TradingSessionSubID")]
        public EFS_String sesSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time Bracket", LblWidth = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "TimeBracket")]
        public EFS_String tmBkt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leaves Qty", LblWidth = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "LeavesQty")]
        public EFS_Decimal leavesQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currently executed Qty", LblWidth = 150)]
        [DictionaryGUI(Page = "field_description", Anchor = "CumQty")]
        public EFS_Decimal cumQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "AveragePrice", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "AvgPx")]
        public EFS_Decimal avgPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Day Order Quantity")]
        [DictionaryGUI(Page = "field_description", Anchor = "DayOrdQty")]
        public EFS_Decimal dayOrdQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity traded today")]
        [DictionaryGUI(Page = "field_description", Anchor = "DayCumQty")]
        public EFS_Decimal dayCumQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Average price for quantity traded today")]
        [DictionaryGUI(Page = "field_description", Anchor = "DayAvgPx")]
        public EFS_Decimal dayAvgPx;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "GT Booking instruction")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "GTBookingInst")]
        public GTBookingInstEnum GTBkngInst;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade date")]
        [DictionaryGUI(Page = "field_description", Anchor = "TradeDate")]
        public EFS_Date trdDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time transaction")]
        [DictionaryGUI(Page = "field_description", Anchor = "TransacTime")]
        public EFS_Date txnTm;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "ReportToExch")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "ReportToExch")]
        public ReportToExchEnum RptToExch;
        #endregion Execution Details
        #region Trade Details
        #region Gross Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution details")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade details", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total amount traded")]
        [DictionaryGUI(Page = "field_description", Anchor = "GrossTradeAmt")]
        public EFS_Decimal grossTrdAmt;
        #endregion Gross Amount
        #region Fixed income trades
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed income trades", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number of Days of Interest")]
        [DictionaryGUI(Page = "field_description", Anchor = "NumDaysInterest")]
        public EFS_Integer numDaysInt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ex Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "ExDate")]
        public EFS_Date exDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued Interest Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "AccruedInterestRate")]
        public EFS_Decimal acrdIntRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued Interest Amount")]
        [DictionaryGUI(Page = "field_description", Anchor = "AccruedInterestAmt")]
        public EFS_Decimal acrdIntAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount of interest at maturity")]
        [DictionaryGUI(Page = "field_description", Anchor = "InterestAtMaturity")]
        public EFS_Decimal intAtMat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued Interest Amount on the end date")]
        [DictionaryGUI(Page = "field_description", Anchor = "EndAccruedInterestAmt")]
        public EFS_Decimal endAcrdIntAmt;
        #endregion Fixed income trades
        #region Repo trades
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed income trades")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo trades", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Starting dirty cash paid to the seller on the Start Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "StartCash")]
        public EFS_Decimal startCsh;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ending dirty cash reimbursed to the buyer on the End Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "EndCash")]
        public EFS_Decimal endCsh;
        #endregion Repo trades
        #region Alternate yields for fixed income trades
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo trades")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate yields for fixed income trades", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Traded Flat")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "TradedFlatSwitch")]
        public YesNoEnum TrddFlatSwitch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basis Feature Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "BasisFeatureDate")]
        public EFS_Date basisFeatureDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basis Feature Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "BasisFeaturePrice")]
        public EFS_Decimal basisFeaturePx;
        #endregion Alternate yields for fixed income trades
        #region Municipal trades
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate yields for fixed income trades")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Municipal trades", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Concession")]
        [DictionaryGUI(Page = "field_description", Anchor = "Concession")]
        public EFS_Decimal concession;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total Take down")]
        [DictionaryGUI(Page = "field_description", Anchor = "TotTakedown")]
        public EFS_Decimal totTakedown;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net Money")]
        [DictionaryGUI(Page = "field_description", Anchor = "NetMoney")]
        public EFS_Decimal netMny;
        #endregion Municipal trades
        #region Forex accomodated trades
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Municipal trades")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Forex accomodated trades", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Currency Amount")]
        [DictionaryGUI(Page = "field_description", Anchor = "SettlCurrAmt")]
        public EFS_Decimal settlCurrAmt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total Take down")]
        [DictionaryGUI(Page = "field_description", Anchor = "SettlCurrency")]
        public string SettlCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Foreign exchange rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "SettlCurrFxRate")]
        public EFS_Decimal settlCurrFxRt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Foreign exchange rate calculation")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "SettlCurrFxRateCalc")]
        public SettlCurrFxRateCalcEnum SettlCurrFxRtCalc;
        #endregion Forex accomodated trades
        #region Other order details
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Forex accomodated trades")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Other order details", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instructions for order handling on Broker trading floor")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "HandlInst")]
        public HandlInstEnum HandlInst;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Minimum quantity of an order to be executed")]
        [DictionaryGUI(Page = "field_description", Anchor = "MinQty")]
        public EFS_Decimal minQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Max Floor")]
        [DictionaryGUI(Page = "field_description", Anchor = "MaxFloor")]
        public EFS_Decimal maxFloor;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Position Effect")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PositionEffect")]
        public PositionEffectEnum PosEfct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Max Show")]
        [DictionaryGUI(Page = "field_description", Anchor = "MaxShow")]
        public EFS_Decimal maxShow;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Booking Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "BookingType")]
        public BookingTypeEnum BkngTyp;
        #endregion Other order details
        #region Comments
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Other order details")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Comments", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Width = 600, Name = "text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "Text")]
        public EFS_String comment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "encoded Text byte length", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedTextLen")]
        public EFS_PosInteger encTxtLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedText")]
        public EFS_String encTxt;
        #endregion Comments
        #region FX Swap trades
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Comments")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "FX Swap trades", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Value date for the future portion of a F/X swap")]
        [DictionaryGUI(Page = "field_description", Anchor = "SettlDate2")]
        public EFS_Date settlDt2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "OrderQty2")]
        public EFS_Decimal qty2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Forward points (added to LastSpotRate)")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastForwardPoints2")]
        public EFS_Decimal lastFwdPnts2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "FX Swap trades")]
        public bool Filler5GUI;
        #endregion FX Swap trades
        #endregion Trade Details
        #region Execution Report Type
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade details")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution Report Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "MultiLegReportingType")]
        public MultiLegReportingTypeEnum MLEGRptTyp;
        #endregion Execution Report Type
        #region CIV trades details
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "CIV trades details", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cancellation rights")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "CancellationRights")]
        public CancellationRightsEnum CxllationRights;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Money laundering status")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "MoneyLaunderingStatus")]
        public MoneyLaunderingStatusEnum MnyLaunderingStat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Registration details ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "RegistID")]
        public EFS_String registID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Designation")]
        [DictionaryGUI(Page = "field_description", Anchor = "Designation")]
        public EFS_String designation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CIV order booked time")]
        [DictionaryGUI(Page = "field_description", Anchor = "TransBkdTime")]
        public EFS_Date transBkdTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fund valuation point time")]
        [DictionaryGUI(Page = "field_description", Anchor = "ExecValuationPoint")]
        public EFS_Date execValuationPoint;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution Price calculation type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "ExecPriceType")]
        public ExecPriceTypeEnum ExecPxTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Adjustment")]
        [DictionaryGUI(Page = "field_description", Anchor = "ExecPriceAdjustment")]
        public EFS_Decimal execPxAdjment;
        #endregion CIV trades details
        #region Miscellaneous
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = " CIV trades details")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Miscellaneous", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Priority Indicator")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PriorityIndicator")]
        public PriorityIndicatorEnum PriInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Improvement")]
        [DictionaryGUI(Page = "field_description", Anchor = "PriceImprovement")]
        public EFS_Decimal pxImprvmnt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Liquidity Indicator")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LastLiquidityInd")]
        public LastLiquidityIndEnum LastLqdtyInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "this message is a drop copy of another message")]
        [DictionaryGUI(Page = "field_description", Anchor = "CopyMsgIndicator")]
        public EFS_Boolean copyMsgInd;
        #endregion Miscellaneous
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Miscellaneous")]
        public bool Filler7GUI;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Order characteristics and execution data")]
        public bool Filler8GUI;
        #endregion Order characteristics and execution data
        #region Specified Fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OrdQtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CommSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool FinDetlsSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PegInstrSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DiscInstrSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SprdBnchmkCurveSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool YieldSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool orignDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totNumRptsSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastRptReqedSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CrssTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool WorkingIndSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RejRsnSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExecRstmtRsnSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AcctIDSrcSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AcctTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DayBkngInstSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool BkngUnitSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PreallocMethSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlCcySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CshMgnSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ClrFeeIndSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool QtyTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PxTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stopPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool peggedPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dsctnPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TgtStrategySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool participationRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tgtStrategyParametersSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tgtStrategyPerformanceSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SolFlagSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TmInForceSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool efctvTmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expireDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expireTmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExecInstSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CpctySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RstctionsSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CustCpctySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastQtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool undLastQtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool undLastPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastParPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastSpotRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastFwdPntsSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LastMktSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LastCpctySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayOrdQtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCumQtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayAvgPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool GTBkngInstSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trdDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool txnTmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RptToExchSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool grossTrdAmtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numDaysIntSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool acrdIntRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool acrdIntAmtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intAtMatSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endAcrdIntAmtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startCshSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endCshSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrddFlatSwitchSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool basisFeatureDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool basisFeaturePxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool concessionSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totTakedownSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool netMnySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlCurrAmtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlCurrFxRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlCurrFxRtCalcSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool HandlInstSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minQtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maxFloorSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PosEfctSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maxShowSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool BkngTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlDt2Specified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qty2Specified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastFwdPnts2Specified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MLEGRptTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CxllationRightsSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MnyLaunderingStatSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool transBkdTmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExecPxTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool execValuationPointSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool execPxAdjmentSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxImprvmntSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PriIndSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LastLqdtyIndSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool copyMsgIndSpecified;
        #endregion Specified Fields
        #endregion GUI
        #region Constructors
        public ExecutionReport_message()
        {
            orignDt = new EFS_Date();
            settlDt = new EFS_Date();
            efctvTm = new EFS_Date();
            expireDt = new EFS_Date();
            expireTm = new EFS_Date();
            trdDt = new EFS_Date();
            txnTm = new EFS_Date();
            exDt = new EFS_Date();
            basisFeatureDt = new EFS_Date();
            settlDt2 = new EFS_Date();
            transBkdTm = new EFS_Date();
            execValuationPoint = new EFS_Date();

            leavesQty = new EFS_Decimal(0);
            cumQty = new EFS_Decimal(0);
            avgPx = new EFS_Decimal(0);

            LastMkt = string.Empty;
            Ccy = string.Empty;
            SettlCcy = string.Empty;
        }
        #endregion Constructors

        public static object _LastMkt(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _SettlCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

    }
    #endregion ExecutionReport_message

    #region FinancingDetails
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class FinancingDetails : ItemGUI
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AgmtDesc
        {
            set { agmtDesc = new EFS_String(value); }
            get { return (null != agmtDesc) ? agmtDesc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AgmtID
        {
            set { agmtID = new EFS_String(value); }
            get { return (null != agmtID) ? agmtID.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime AgmtDt
        {
            set { agmtDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != agmtDt) ? agmtDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AgmtDtSpecified
        {
            set { agmtDtSpecified = value; }
            get { return agmtDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime StartDt
        {
            set { startDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != startDt) ? startDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartDtSpecified
        {
            set { startDtSpecified = value; }
            get { return startDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime EndDt
        {
            set { endDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != endDt) ? endDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EndDtSpecified
        {
            set { endDtSpecified = value; }
            get { return endDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal MgnRatio
        {
            set { mgnRatio = new EFS_Decimal(value); }
            get { return (null != mgnRatio) ? mgnRatio.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MgnRatioSpecified
        {
            set { mgnRatioSpecified = value; }
            get { return mgnRatioSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Agreement", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Full name")]
        [DictionaryGUI(Page = "field_description", Anchor = "AgreementDesc")]
        public EFS_String agmtDesc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "reference")]
        [DictionaryGUI(Page = "field_description", Anchor = "AgreementDesc")]
        public EFS_String agmtID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "AgreementCurrency")]
        public string AgmtCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "AgreementDate")]
        public EFS_Date agmtDt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Agreement")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Financing termination Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "TerminationType")]
        public TerminationTypeEnum TrmTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Start Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "StartDate")]
        public EFS_Date startDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "End Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "EndDate")]
        public EFS_Date endDt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "DeliveryType")]
        public FixML.Enum.DeliveryTypeEnum DlvryTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Margin Ratio")]
        [DictionaryGUI(Page = "field_description", Anchor = "MarginRatio")]
        public EFS_Decimal mgnRatio;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool agmtDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AgmtCcySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrmTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DlvryTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mgnRatioSpecified;
        #endregion Specified fields
        #region Constructors
        public FinancingDetails()
        {
            agmtDt = new EFS_Date();
            startDt = new EFS_Date();
            endDt = new EFS_Date();
            AgmtCcy = string.Empty;
        }
        #endregion Constructors

        public static object _AgmtCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

    }
    #endregion FinancingDetails

    #region Hop
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class Hop : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")]
        public string Ref
        {
            set { _ref = new EFS_PosInteger(value); }
            get { return (null != _ref) ? _ref.Value : null; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SntSpecified
        {
            set { sntSpecified = value; }
            get { return sntSpecified; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime Snt
        {
            set { snt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != snt) ? snt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { efs_id = new EFS_Id(value); }
            get { return (null == efs_id ? null : efs_id.Value); }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Reference identifier", Width = 300)]
        [DictionaryGUI(Page = "field_description", Anchor = "HopRefID")]
        public EFS_PosInteger _ref;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "HopCompID")]
        [DictionaryGUI(Page = "field_description", Anchor = "HopCompID")]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sending Time")]
        [DictionaryGUI(Page = "field_description", Anchor = "HopSendingTime")]
        public EFS_Date snt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sntSpecified;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
        #region Constructors
        public Hop()
        {
            snt = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion Hop

    #region InstrmtLegExecGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class InstrmtLegExecGrp : ItemGUI, IEFS_Array
    {
        #region Attributes fields
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Qty
        {
            set { qty = new EFS_Decimal(value); }
            get { return (null != qty) ? qty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QtySpecified
        {
            set { qtySpecified = value; }
            get { return qtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RefID
        {
            set { refID = new EFS_String(value); }
            get { return (null != refID) ? refID.Value : null; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RefIDSpecified
        {
            set { refIDSpecified = value; }
            get { return refIDSpecified; }
        }
		[System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Px
        {
            set { px = new EFS_Decimal(value); }
            get { return (null != px) ? px.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxSpecified
        {
            set { pxSpecified = value; }
            get { return pxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime SettlDt
        {
            set { settlDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != settlDt) ? settlDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDtSpecified
        {
            set { settlDtSpecified = value; }
            get { return settlDtSpecified; }
        }
		[System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal LastPx
        {
            set { lastPx = new EFS_Decimal(value); }
            get { return (null != lastPx) ? lastPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastPxSpecified
        {
            set { lastPxSpecified = value; }
            get { return lastPxSpecified; }
        }
        #endregion Attributes fields
        #region GUI
		[System.Xml.Serialization.XmlElementAttribute("Leg", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg instrument", IsVisible = false)]
        public InstrumentLeg Leg;
        [System.Xml.Serialization.XmlElementAttribute("Stip",Order=2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stipulations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stipulation", MinItem = 0)]
        [DictionaryGUI(Page = "LegStipulations")]
        public LegStipulations[] Stip;
        [System.Xml.Serialization.XmlElementAttribute("Pty",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Partie", IsChild = true, IsChildVisible = true, MinItem = 0)]
        [DictionaryGUI(Page = "NestedParties")]
        public NestedParties[] Pty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Details...", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegQty")]
        public EFS_Decimal qty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Swap Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSwapType")]
        public LegSwapTypeEnum SwapTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Position Effect")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegPositionEffect")]
        public PositionEffectEnum PosEfct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cover/uncovered")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegCoveredOrUncovered")]
        public LegCoveredOrUncoveredEnum Cover;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Reference ID")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [DictionaryGUI(Page = "field_description", Anchor = "LegRefID")]
        public EFS_String refID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegPrice")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSettlType")]
        public SettlTypeEnum SettlTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSettlDate")]
        public EFS_Date settlDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegLastPx")]
        public EFS_Decimal lastPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Details...")]
        public bool FillerGUI;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SwapTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PosEfctSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CoverSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool refIDSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastPxSpecified;
        #endregion Specified fields
        #region Constructors
        public InstrmtLegExecGrp()
        {
            settlDt = new EFS_Date();
        }
        #endregion Constructors
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion InstrmtLegExecGrp
    #region Instrument
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class Instrument : ItemGUI
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { sym = new EFS_String(value); }
            get { return (null != sym) ? sym.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return (null == id ? null : id.Value); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CFI
        {
            set { cfi = new EFS_String(value); }
            get { return (null == cfi ? null : cfi.Value); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFISpecified
        {
            set { cfiSpecified = value; }
            get { return cfiSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SubTyp
        {
            set { subTyp = new EFS_String(value); }
            get { return (null != subTyp) ? subTyp.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MMY
        {
            set { mmy = new EFS_MonthYear(value); }
            get { return (null != mmy) ? mmy.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime MatDt
        {
            set { matDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != matDt) ? matDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MatDtSpecified
        {
            set { matDtSpecified = value; }
            get { return matDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime CpnPmt
        {
            set { cpnPmt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != cpnPmt) ? cpnPmt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CpnPmtSpecified
        {
            set { cpnPmtSpecified = value; }
            get { return cpnPmtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Issued
        {
            set { issued = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != issued) ? issued.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssuedSpecified
        {
            set { issuedSpecified = value; }
            get { return issuedSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int RepoTrm
        {
            set { repoTrm = new EFS_Integer(value); }
            get { return (null != repoTrm) ? repoTrm.IntValue : Convert.ToInt32(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoTrmSpecified
        {
            set { repoTrmSpecified = value; }
            get { return repoTrmSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal RepoRt
        {
            set { repoRt = new EFS_Decimal(value); }
            get { return (null != repoRt) ? repoRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoRtSpecified
        {
            set { repoRtSpecified = value; }
            get { return repoRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Fctr
        {
            set { fctr = new EFS_Decimal(value); }
            get { return (null != fctr) ? fctr.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FctrSpecified
        {
            set { fctrSpecified = value; }
            get { return fctrSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CrdRtg
        {
            set { crdRtg = new EFS_String(value); }
            get { return (null != crdRtg) ? crdRtg.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StPrv
        {
            set { stPrv = new EFS_String(value); }
            get { return (null != stPrv) ? stPrv.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Lcl
        {
            set { lcl = new EFS_String(value); }
            get { return (null != lcl) ? lcl.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Redeem
        {
            set { redeem = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != redeem) ? redeem.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RedeemSpecified
        {
            set { redeemSpecified = value; }
            get { return redeemSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Strk
        {
            set { strk = new EFS_Decimal(value); }
            get { return (null != strk) ? strk.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StrkSpecified
        {
            set { strkSpecified = value; }
            get { return strkSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptAt
        {
            set { optAt = new EFS_String(value); }
            get { return (null != optAt) ? optAt.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Mult
        {
            set { mult = new EFS_Decimal(value); }
            get { return (null != mult) ? mult.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MultSpecified
        {
            set { multSpecified = value; }
            get { return multSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal CpnRt
        {
            set { cpnRt = new EFS_Decimal(value); }
            get { return (null != cpnRt) ? cpnRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CpnRtSpecified
        {
            set { cpnRtSpecified = value; }
            get { return cpnRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Issr
        {
            set { issr = new EFS_String(value); }
            get { return (null != issr) ? issr.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncIssrLen
        {
            set { encIssrLen = new EFS_PosInteger(value); }
            get { return (null != encIssrLen) ? encIssrLen.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncIssr
        {
            set { encIssr = new EFS_String(value); }
            get { return (null != encIssr) ? encIssr.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Desc
        {
            set { desc = new EFS_String(value); }
            get { return (null != desc) ? desc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncSecDescLen
        {
            set { encSecDescLen = new EFS_PosInteger(value); }
            get { return (null != encSecDescLen) ? encSecDescLen.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncSecDesc
        {
            set { encSecDesc = new EFS_String(value); }
            get { return (null != encSecDesc) ? encSecDesc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Pool
        {
            set { pool = new EFS_String(value); }
            get { return (null != pool) ? pool.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CSetMo
        {
            set { cSetMo = new EFS_MonthYear(value); }
            get { return (null != cSetMo) ? cSetMo.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CPRegT
        {
            set { cPRegT = new EFS_String(value); }
            get { return (null != cPRegT) ? cPRegT.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Dated
        {
            set { dated = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != dated) ? dated.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DatedSpecified
        {
            set { datedSpecified = value; }
            get { return datedSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime IntAcrl
        {
            set { intAcrl = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != intAcrl) ? intAcrl.DateValue : Convert.ToDateTime(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IntAcrlSpecified
        {
            set { intAcrlSpecified = value; }
            get { return intAcrlSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ticker Symbol", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "Symbol")]
        public EFS_String sym;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol Sfx")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "SymbolSfx")]
        public SymbolSfxEnum Sfx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ticker Symbol")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Id", IsVisible = false)]
        [ControlGUI(Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "SecurityID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "SecurityIDSource")]
        public SecurityIDSourceEnum Src;
        [System.Xml.Serialization.XmlElementAttribute("AID",Order=1)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Id")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security alternate Ids", MinItem = 0)]
        public SecAltIDGrp[] AID;
        [System.Xml.Serialization.XmlElementAttribute("Evnt",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Events")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Events", IsChild = true, MinItem = 0)]
        public EvntGrp[] Evnt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Suite...", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Product")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "Product")]
        public ProductEnum Prod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CFI Code")]
        [DictionaryGUI(Page = "field_description", Anchor = "CFICode")]
        public EFS_String cfi;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "SecurityType")]
        public SecurityTypeEnum SecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Security Sub Type")]
        [DictionaryGUI(Page = "field_description", Anchor = "SecuritySubType")]
        public EFS_String subTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "MonthYear")]
        [DictionaryGUI(Page = "field_description", Anchor = "MaturityMonthYear")]
        public EFS_MonthYear mmy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "MaturityDate")]
        public EFS_Date matDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Payment Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "CouponPaymentDate")]
        public EFS_Date cpnPmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issue date")]
        [DictionaryGUI(Page = "field_description", Anchor = "IssueDate")]
        public EFS_Date issued;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo Collateral Security Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "RepoCollateralSecurityType")]
        public SecurityTypeEnum RepoCollSecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Term")]
        [DictionaryGUI(Page = "field_description", Anchor = "RepurchaseTerm")]
        public EFS_Integer repoTrm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "RepurchaseRate")]
        public EFS_Decimal repoRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amortization factor (for fixed income)")]
        [DictionaryGUI(Page = "field_description", Anchor = "Factor")]
        public EFS_Decimal fctr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Credit Rating")]
        [DictionaryGUI(Page = "field_description", Anchor = "CreditRating")]
        public EFS_String crdRtg;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Registry")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "InstrRegistry")]
        public InstrRegistryEnum Rgstry;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "CountryOfIssue")]
        public string IssuCtry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "State/Province")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [DictionaryGUI(Page = "field_description", Anchor = "StateOrProvinceOfIssue")]
        public EFS_String stPrv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Local/City")]
        [DictionaryGUI(Page = "field_description", Anchor = "LocaleOfIssue")]
        public EFS_String lcl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption date")]
        [DictionaryGUI(Page = "field_description", Anchor = "RedemptionDate")]
        public EFS_Date redeem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "StrikePrice")]
        public EFS_Decimal strk;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "Currency")]
        public string StrkCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option attribute")]
        [DictionaryGUI(Page = "field_description", Anchor = "OptAttribute")]
        public EFS_String optAt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Multiplier")]
        [DictionaryGUI(Page = "field_description", Anchor = "ContractMultiplier")]
        public EFS_Decimal mult;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "CouponRate")]
        public EFS_Decimal cpnRt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Exchange", Width = 500)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "SecurityExchange")]
        public string Exch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Width = 600, Name = "text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "Issuer")]
        public EFS_String issr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "encoded Issuer Byte length", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedIssuerLen")]
        public EFS_PosInteger encIssrLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedIssuer")]
        public EFS_String encIssr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security description", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Width = 600, Name = "text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "SecurityDesc")]
        public EFS_String desc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "encoded security Byte length", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedSecurityDescLen")]
        public EFS_PosInteger encSecDescLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedSecurityDesc")]
        public EFS_String encSecDesc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security description")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "MBS/ABS Pool", Width = 600, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "Pool")]
        public EFS_String pool;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Contract Settlement MonthYear")]
        [DictionaryGUI(Page = "field_description", Anchor = "ContractSettlMonth")]
        public EFS_MonthYear cSetMo;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "The program under which a commercial paper is issued")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "CPProgram")]
        public CPProgramEnum CPPgm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "The registration type of a commercial paper issuance", Width = 600, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "CPRegType")]
        public EFS_String cPRegT;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dated date")]
        [DictionaryGUI(Page = "field_description", Anchor = "DatedDate")]
        public EFS_Date dated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest accrual date")]
        [DictionaryGUI(Page = "field_description", Anchor = "InterestAccrualDate")]
        public EFS_Date intAcrl;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Suite...")]
        public bool FillerGUI;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SfxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ProdSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cfiSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnPmtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuedSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RepoCollSecTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoTrmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fctrSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RgstrySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool IssuCtrySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redeemSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StrkCcySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optAtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExchSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CPPgmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CPRegTSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool datedSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intAcrlSpecified;
        #endregion Specified fields
        #region Constructors
        public Instrument()
        {
            matDt = new EFS_Date();
            cpnPmt = new EFS_Date();
            issued = new EFS_Date();
            redeem = new EFS_Date();
            dated = new EFS_Date();
            intAcrl = new EFS_Date();
            Exch = string.Empty;
            StrkCcy = string.Empty;
            IssuCtry = string.Empty;
        }
        #endregion Constructors

        public static object _Exch(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _StrkCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _IssuCtry(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }


    }
    #endregion Instrument
    #region InstrumentLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class InstrumentLeg : ItemGUI
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { sym = new EFS_String(value); }
            get { return (null != sym) ? sym.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return (null == id ? null : id.Value); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CFI
        {
            set { cfi = new EFS_String(value); }
            get { return (null == cfi ? null : cfi.Value); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFISpecified
        {
            set { cfiSpecified = value; }
            get { return cfiSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SecSubTyp
        {
            set { secSubTyp = new EFS_String(value); }
            get { return (null != secSubTyp) ? secSubTyp.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MMY
        {
            set { mmy = new EFS_MonthYear(value); }
            get { return (null != mmy) ? mmy.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Mat
        {
            set { mat = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != mat) ? mat.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MatSpecified
        {
            set { matSpecified = value; }
            get { return matSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime CpnPmt
        {
            set { cpnPmt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != cpnPmt) ? cpnPmt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CpnPmtSpecified
        {
            set { cpnPmtSpecified = value; }
            get { return cpnPmtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Issued
        {
            set { issued = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != issued) ? issued.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssuedSpecified
        {
            set { issuedSpecified = value; }
            get { return issuedSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int RepoTrm
        {
            set { repoTrm = new EFS_Integer(value); }
            get { return (null != repoTrm) ? repoTrm.IntValue : Convert.ToInt32(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoTrmSpecified
        {
            set { repoTrmSpecified = value; }
            get { return repoTrmSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal RepoRt
        {
            set { repoRt = new EFS_Decimal(value); }
            get { return (null != repoRt) ? repoRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoRtSpecified
        {
            set { repoRtSpecified = value; }
            get { return repoRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Fctr
        {
            set { fctr = new EFS_Decimal(value); }
            get { return (null != fctr) ? fctr.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FctrSpecified
        {
            set { fctrSpecified = value; }
            get { return fctrSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CrdRtg
        {
            set { crdRtg = new EFS_String(value); }
            get { return (null != crdRtg) ? crdRtg.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StOrProvnc
        {
            set { stOrProvnc = new EFS_String(value); }
            get { return (null != stOrProvnc) ? stOrProvnc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Lcl
        {
            set { lcl = new EFS_String(value); }
            get { return (null != lcl) ? lcl.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Redeem
        {
            set { redeem = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != redeem) ? redeem.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RedeemSpecified
        {
            set { redeemSpecified = value; }
            get { return redeemSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Strk
        {
            set { strk = new EFS_Decimal(value); }
            get { return (null != strk) ? strk.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StrkSpecified
        {
            set { strkSpecified = value; }
            get { return strkSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptA
        {
            set { optA = new EFS_String(value); }
            get { return (null != optA) ? optA.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal CMult
        {
            set { cMult = new EFS_Decimal(value); }
            get { return (null != cMult) ? cMult.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CMultSpecified
        {
            set { cMultSpecified = value; }
            get { return cMultSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal CpnRt
        {
            set { cpnRt = new EFS_Decimal(value); }
            get { return (null != cpnRt) ? cpnRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CpnRtSpecified
        {
            set { cpnRtSpecified = value; }
            get { return cpnRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Issr
        {
            set { issr = new EFS_String(value); }
            get { return (null != issr) ? issr.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncLegIssrLen
        {
            set { encLegIssrLen = new EFS_PosInteger(value); }
            get { return (null != encLegIssrLen) ? encLegIssrLen.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncLegIssr
        {
            set { encLegIssr = new EFS_String(value); }
            get { return (null != encLegIssr) ? encLegIssr.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Desc
        {
            set { desc = new EFS_String(value); }
            get { return (null != desc) ? desc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncLegSecDescLen
        {
            set { encLegSecDescLen = new EFS_PosInteger(value); }
            get { return (null != encLegSecDescLen) ? encLegSecDescLen.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncLegSecDesc
        {
            set { encLegSecDesc = new EFS_String(value); }
            get { return (null != encLegSecDesc) ? encLegSecDesc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal RatioQty
        {
            set { ratioQty = new EFS_Decimal(value); }
            get { return (null != ratioQty) ? ratioQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RatioQtySpecified
        {
            set { ratioQtySpecified = value; }
            get { return ratioQtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Pool
        {
            set { pool = new EFS_String(value); }
            get { return (null != pool) ? pool.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Dated
        {
            set { dated = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != dated) ? dated.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DatedSpecified
        {
            set { datedSpecified = value; }
            get { return datedSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CSetMo
        {
            set { cSetMo = new EFS_MonthYear(value); }
            get { return (null != cSetMo) ? cSetMo.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime IntAcrl
        {
            set { intAcrl = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != intAcrl) ? intAcrl.DateValue : Convert.ToDateTime(null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IntAcrlSpecified
        {
            set { intAcrlSpecified = value; }
            get { return intAcrlSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ticker Symbol", IsVisible = false)]
        [ControlGUI(Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSymbol")]
        public EFS_String sym;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol Sfx")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSymbolSfx")]
        public SymbolSfxEnum Sfx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ticker Symbol")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Id", IsVisible = false)]
        [ControlGUI(Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSecurityID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSecurityIDSource")]
        public SecurityIDSourceEnum Src;
        [System.Xml.Serialization.XmlElementAttribute("LegAID",Order=1)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Id")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security alternate Ids", MinItem = 0)]
        public LegSecAltIDGrp[] LegAID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Suite...", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Product")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegProduct")]
        public ProductEnum Prod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CFI Code")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegCFICode")]
        public EFS_String cfi;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSecurityType")]
        public SecurityTypeEnum SecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Security Sub Type")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSecuritySubType")]
        public EFS_String secSubTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "MonthYear")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegMaturityMonthYear")]
        public EFS_MonthYear mmy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegMaturityDate")]
        public EFS_Date mat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Payment Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegCouponPaymentDate")]
        public EFS_Date cpnPmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issue date")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegIssueDate")]
        public EFS_Date issued;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo Collateral Security Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegRepoCollateralSecurityType")]
        public SecurityTypeEnum RepoCollSecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Term")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegRepurchaseTerm")]
        public EFS_Integer repoTrm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegRepurchaseRate")]
        public EFS_Decimal repoRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amortization factor (for fixed income)")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegFactor")]
        public EFS_Decimal fctr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Credit Rating")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegCreditRating")]
        public EFS_String crdRtg;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Registry")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegInstrRegistry")]
        public InstrRegistryEnum Rgstry;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegCountryOfIssue")]
        public string Ctry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "State/Province")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegStateOrProvinceOfIssue")]
        public EFS_String stOrProvnc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Local/City")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegLocaleOfIssue")]
        public EFS_String lcl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption date")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegRedemptionDate")]
        public EFS_Date redeem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegStrikePrice")]
        public EFS_Decimal strk;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike currency")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegStrikeCurrency")]
        public string StrkCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option attribute")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegOptAttribute")]
        public EFS_String optA;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Multiplier")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegContractMultiplier")]
        public EFS_Decimal cMult;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegCouponRate")]
        public EFS_Decimal cpnRt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Exchange", Width = 500)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSecurityExchange")]
        public string Exch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Width = 600, Name = "text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "LegIssuer")]
        public EFS_String issr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "encoded Issuer Byte length", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedLegIssuerLen")]
        public EFS_PosInteger encLegIssrLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedLegIssuer")]
        public EFS_String encLegIssr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security description", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Width = 600, Name = "text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSecurityDesc")]
        public EFS_String desc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "encoded security Byte length", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedLegSecurityDescLen")]
        public EFS_PosInteger encLegSecDescLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedLegSecurityDesc")]
        public EFS_String encLegSecDesc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "ratio of quantity for this individual leg")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegRatioQty")]
        public EFS_Decimal ratioQty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSide")]
        public SideEnum Side;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security description")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "MBS/ABS Pool", Width = 600, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "LegPool")]
        public EFS_String pool;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dated date")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegDatedDate")]
        public EFS_Date dated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Contract Settlement MonthYear")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegContractSettlMonth")]
        public EFS_MonthYear cSetMo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest accrual date")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegInterestAccrualDate")]
        public EFS_Date intAcrl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Suite...")]
        public bool FillerGUI;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SfxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ProdSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cfiSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnPmtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuedSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RepoCollSecTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoTrmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fctrSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RgstrySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CtrySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redeemSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StrkCcySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optASpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cMultSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExchSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ratioQtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SideSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool datedSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intAcrlSpecified;
        #endregion Specified fields
        #region Constructors
        public InstrumentLeg()
        {
            mat = new EFS_Date();
            cpnPmt = new EFS_Date();
            issued = new EFS_Date();
            redeem = new EFS_Date();
            dated = new EFS_Date();
            intAcrl = new EFS_Date();
            Exch = string.Empty;
            StrkCcy = string.Empty;
            Ccy = string.Empty;
            Ctry = string.Empty;
        }
        #endregion Constructors

        public static object _Exch(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _StrkCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _Ctry(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }


    }
    #endregion InstrumentLeg

    #region LegSecAltIDGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class LegSecAltIDGrp : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SecAltID
        {
            set { secAltID = new EFS_String(value); }
            get { return (null != secAltID) ? secAltID.Value : null; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "value", Width = 200)]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSecurityAltID")]
        public EFS_String secAltID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "LegSecurityAltIDSource")]
        public SecurityIDSourceEnum SecAltIDSrc;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion LegSecAltIDGrp
    #region LegStipulations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class LegStipulations : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StipVal
        {
            set { stipVal = new EFS_String(value); }
            get { return (null != stipVal) ? stipVal.Value : null; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "LegStipulationType")]
        public StipulationTypeEnum StipTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "value", Width = 300, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "LegStipulationValue")]
        public EFS_String stipVal;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion LegStipulations

    #region MessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class MessageHeader : BaseHeader
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")]
        public string SeqNum
        {
            set { seqNum = new EFS_PosInteger(value); }
            get { return (null != seqNum) ? seqNum.Value : null; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Integer message sequence number.", Width = 60)]
        [DictionaryGUI(Page = "field_description", Anchor = "MsgSeqNum")]
        public EFS_PosInteger seqNum;
        #endregion GUI
    }
    #endregion MessageHeader
    #region MiscFeesGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class MiscFeesGrp : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Amt
        {
            set { amt = new EFS_Decimal(value); }
            get { return (null != amt) ? amt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AmtSpecified
        {
            set { amtSpecified = value; }
            get { return amtSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
        [DictionaryGUI(Page = "field_description", Anchor = "MiscFeeAmt")]
        public EFS_Decimal amt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "MiscFeeCurr")]
        public string Curr;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "MiscFeeType")]
        public MiscFeeTypeEnum Typ;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basis")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "MiscFeeBasis")]
        public MiscFeeBasisEnum Basis;

        public static object _Curr(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool BasisSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CurrSpecified;
        #endregion Specified fields
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array

        #region Constructors
        public MiscFeesGrp()
        {
            Curr = string.Empty;
        }
        #endregion Constructors
    }
    #endregion MiscFeesGrp

    #region NestedParties
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class NestedParties : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string ID
        {
            set { efs_id = new EFS_Id(value); }
            get { return (null == efs_id ? null : efs_id.Value); }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlElementAttribute("Sub",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sub", IsMaster = true, IsClonable = false, MinItem = 1)]
        public NstdPtysSubGrp[] Sub;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Source")]
        [DictionaryGUI(Page = "field_description", Anchor = "NestedPartyIDSource")]
        public PartySourceEnum Src;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Role")]
        [DictionaryGUI(Page = "field_description", Anchor = "NestedPartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "NestedPartyID")]
        [DictionaryGUI(Page = "field_description", Anchor = "NestedPartyID")]
        public EFS_Id efs_id;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion NestedParties
    #region NstdPtysSubGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class NstdPtysSubGrp
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { efs_id = new EFS_Id(value); }
            get { return (null == efs_id ? null : efs_id.Value); }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Type")]
        [DictionaryGUI(Page = "field_description", Anchor = "NestedPartySubIDType")]
        public PartySubIDTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "NestedPartySubID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "NestedPartySubID")]
        public EFS_Id efs_id;
        #endregion GUI
    }
    #endregion NstdPtysSubGrp

    #region OrderQtyData
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class OrderQtyData : ItemGUI
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Qty
        {
            set { qty = new EFS_Decimal(value); }
            get { return (null != qty) ? qty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QtySpecified
        {
            set { qtySpecified = value; }
            get { return qtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Cash
        {
            set { cash = new EFS_Decimal(value); }
            get { return (null != cash) ? cash.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CashSpecified
        {
            set { cashSpecified = value; }
            get { return cashSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Pct
        {
            set { pct = new EFS_Decimal(value); }
            get { return (null != pct) ? pct.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PctSpecified
        {
            set { pctSpecified = value; }
            get { return pctSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal RndMod
        {
            set { rndMod = new EFS_Decimal(value); }
            get { return (null != rndMod) ? rndMod.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RndModSpecified
        {
            set { rndModSpecified = value; }
            get { return rndModSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity ordered")]
        [DictionaryGUI(Page = "field_description", Anchor = "OrderQty")]
        public EFS_Decimal qty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Quantity ordered")]
        [DictionaryGUI(Page = "field_description", Anchor = "CashOrderQty")]
        public EFS_Decimal cash;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Percentage")]
        [DictionaryGUI(Page = "field_description", Anchor = "OrderPercent")]
        public EFS_Decimal pct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rounding", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Direction")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "RoundingDirection")]
        public RoundDirectionEnum RndDir;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Modulus")]
        [DictionaryGUI(Page = "field_description", Anchor = "RoundingModulus")]
        public EFS_Decimal rndMod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rounding")]
        public bool FillerGUI;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pctSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RndDirSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rndModSpecified;
        #endregion Specified fields
    }
    #endregion OrderQtyData

    #region Parties
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class Parties : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string ID
        {
            set { efs_id = new EFS_Id(value); }
            get { return (null == efs_id ? null : efs_id.Value); }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlElementAttribute("Sub",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sub", IsMaster = true, IsClonable = false, MinItem = 1)]
        public PtysSubGrp[] Sub;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Source")]
        [DictionaryGUI(Page = "field_description", Anchor = "PartyIDSource")]
        public PartySourceEnum Src;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Role")]
        [DictionaryGUI(Page = "field_description", Anchor = "PartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "PartyID")]
        [DictionaryGUI(Page = "field_description", Anchor = "PartyID")]
        public EFS_Id efs_id;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion Parties
    #region PegInstructions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class PegInstructions : ItemGUI
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal OfstVal
        {
            set { ofstVal = new EFS_Decimal(value); }
            get { return (null != ofstVal) ? ofstVal.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OfstValSpecified
        {
            set { ofstValSpecified = value; }
            get { return ofstValSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Offset amount value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PegOffsetValue")]
        public EFS_Decimal ofstVal;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Move type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PegMoveType")]
        public MoveTypeEnum MoveTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Offset type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PegOffsetType")]
        public OffsetTypeEnum OfstTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Limit type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PegLimitType")]
        public LimitTypeEnum LmtTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "RoundDirection")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PegRoundDirection")]
        public PegRoundDirectionEnum RndDir;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Scope")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "PegScope")]
        public ScopeEnum Scope;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ofstValSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MoveTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OfstTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LmtTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RndDirSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ScopeSpecified;
        #endregion Specified fields
    }
    #endregion PegInstructions
    #region PtysSubGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class PtysSubGrp : IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { efs_id = new EFS_Id(value); }
            get { return (null == efs_id ? null : efs_id.Value); }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "Type")]
        [DictionaryGUI(Page = "field_description", Anchor = "PartySubIDType")]
        public PartySubIDTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "PartySubID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "PartySubID")]
        public EFS_Id efs_id;
        #endregion GUI
        #region Membres de IEFS_Array

        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion PtysSubGrp

    #region SecAltIDGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class SecAltIDGrp : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AltID
        {
            set { altID = new EFS_String(value); }
            get { return (null != altID) ? altID.Value : null; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "value", Width = 200)]
        [DictionaryGUI(Page = "field_description", Anchor = "SecurityAltID")]
        public EFS_String altID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "SecurityAltIDSource")]
        public SecurityIDSourceEnum AltIDSrc;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion SecAltIDGrp
    #region SpreadOrBenchmarkCurveData
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class SpreadOrBenchmarkCurveData : ItemGUI
    {
        #region Attributes accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Spread
        {
            set { spread = new EFS_Decimal(value); }
            get { return (null != spread) ? spread.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SpreadSpecified
        {
            set { spreadSpecified = value; }
            get { return spreadSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Point
        {
            set { point = new EFS_String(value); }
            get { return (null != point) ? point.Value : null; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PointSpecified
        {
            set { pointSpecified = value; }
            get { return pointSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Px
        {
            set { px = new EFS_Decimal(value); }
            get { return (null != px) ? px.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxSpecified
        {
            set { pxSpecified = value; }
            get { return pxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SecID
        {
            set { secID = new EFS_String(value); }
            get { return (null != secID) ? secID.Value : null; }
        }
        #endregion Attributes accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread")]
        [DictionaryGUI(Page = "field_description", Anchor = "Spread")]
        public EFS_Decimal spread;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "BenchmarkCurveCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Curve name")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "BenchmarkCurveName")]
        public CurveNameEnum Name;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Point")]
        [DictionaryGUI(Page = "field_description", Anchor = "BenchmarkCurvePoint")]
        public EFS_String point;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "BenchmarkPriceType")]
        public PriceTypeEnum PxTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "BenchmarkPrice")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark security ID")]
        [DictionaryGUI(Page = "field_description", Anchor = "BenchmarkSecurityID")]
        public EFS_String secID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark security source")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "BenchmarkSecurityIDSource")]
        public SecurityIDSourceEnum SecIDSrc;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pointSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool NameSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PxTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecIDSrcSpecified;
        #endregion Specified fields

        public SpreadOrBenchmarkCurveData()
        {
            Ccy = string.Empty;
        }

        public static object _Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

    }
    #endregion SpreadOrBenchmarkCurveData
    #region Stipulations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class Stipulations : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Val
        {
            set { val = new EFS_String(value); }
            get { return (null != val) ? val.Value : null; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "StipulationType")]
        public StipulationTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "value", Width = 300, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "StipulationValue")]
        public EFS_String val;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion Stipulations

    #region UnderlyingInstrument
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class UnderlyingInstrument : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { sym = new EFS_String(value); }
            get { return (null != sym) ? sym.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return (null == id ? null : id.Value); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CFI
        {
            set { cfi = new EFS_String(value); }
            get { return (null == cfi ? null : cfi.Value); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFISpecified
        {
            set { cfiSpecified = value; }
            get { return cfiSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SubTyp
        {
            set { subTyp = new EFS_String(value); }
            get { return (null != subTyp) ? subTyp.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MMY
        {
            set { mmy = new EFS_MonthYear(value); }
            get { return (null != mmy) ? mmy.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Mat
        {
            set { mat = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != mat) ? mat.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MatSpecified
        {
            set { matSpecified = value; }
            get { return matSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime CpnPmt
        {
            set { cpnPmt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != cpnPmt) ? cpnPmt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CpnPmtSpecified
        {
            set { cpnPmtSpecified = value; }
            get { return cpnPmtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Issued
        {
            set { issued = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != issued) ? issued.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssuedSpecified
        {
            set { issuedSpecified = value; }
            get { return issuedSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int RepoTrm
        {
            set { repoTrm = new EFS_Integer(value); }
            get { return (null != repoTrm) ? repoTrm.IntValue : Convert.ToInt32(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoTrmSpecified
        {
            set { repoTrmSpecified = value; }
            get { return repoTrmSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal RepoRt
        {
            set { repoRt = new EFS_Decimal(value); }
            get { return (null != repoRt) ? repoRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoRtSpecified
        {
            set { repoRtSpecified = value; }
            get { return repoRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Fctr
        {
            set { fctr = new EFS_Decimal(value); }
            get { return (null != fctr) ? fctr.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FctrSpecified
        {
            set { fctrSpecified = value; }
            get { return fctrSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CrdRtg
        {
            set { crdRtg = new EFS_String(value); }
            get { return (null != crdRtg) ? crdRtg.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StOrProvnc
        {
            set { stOrProvnc = new EFS_String(value); }
            get { return (null != stOrProvnc) ? stOrProvnc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Lcl
        {
            set { lcl = new EFS_String(value); }
            get { return (null != lcl) ? lcl.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime Redeem
        {
            set { redeem = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != redeem) ? redeem.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RedeemSpecified
        {
            set { redeemSpecified = value; }
            get { return redeemSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal StrkPx
        {
            set { strkPx = new EFS_Decimal(value); }
            get { return (null != strkPx) ? strkPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StrkPxSpecified
        {
            set { strkPxSpecified = value; }
            get { return strkPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptA
        {
            set { optA = new EFS_String(value); }
            get { return (null != optA) ? optA.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Mult
        {
            set { mult = new EFS_Decimal(value); }
            get { return (null != mult) ? mult.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MultSpecified
        {
            set { multSpecified = value; }
            get { return multSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal CpnRt
        {
            set { cpnRt = new EFS_Decimal(value); }
            get { return (null != cpnRt) ? cpnRt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CpnRtSpecified
        {
            set { cpnRtSpecified = value; }
            get { return cpnRtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Issr
        {
            set { issr = new EFS_String(value); }
            get { return (null != issr) ? issr.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncUndIssrLen
        {
            set { encUndIssrLen = new EFS_PosInteger(value); }
            get { return (null != encUndIssrLen) ? encUndIssrLen.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncUndIssr
        {
            set { encUndIssr = new EFS_String(value); }
            get { return (null != encUndIssr) ? encUndIssr.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Desc
        {
            set { desc = new EFS_String(value); }
            get { return (null != desc) ? desc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncUndSecDescLen
        {
            set { encUndSecDescLen = new EFS_PosInteger(value); }
            get { return (null != encUndSecDescLen) ? encUndSecDescLen.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncUndSecDesc
        {
            set { encUndSecDesc = new EFS_String(value); }
            get { return (null != encUndSecDesc) ? encUndSecDesc.Value : null; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CPRegTyp
        {
            set { cPRegTyp = new EFS_String(value); }
            get { return (null != cPRegTyp) ? cPRegTyp.Value : null; }
        }
		[System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Qty
        {
            set { qty = new EFS_Decimal(value); }
            get { return (null != qty) ? qty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QtySpecified
        {
            set { qtySpecified = value; }
            get { return qtySpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Px
        {
            set { px = new EFS_Decimal(value); }
            get { return (null != px) ? px.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxSpecified
        {
            set { pxSpecified = value; }
            get { return pxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal DirtPx
        {
            set { dirtPx = new EFS_Decimal(value); }
            get { return (null != dirtPx) ? dirtPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DirtPxSpecified
        {
            set { dirtPxSpecified = value; }
            get { return dirtPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal EndPx
        {
            set { endPx = new EFS_Decimal(value); }
            get { return (null != endPx) ? endPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EndPxSpecified
        {
            set { endPxSpecified = value; }
            get { return endPxSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal StartVal
        {
            set { startVal = new EFS_Decimal(value); }
            get { return (null != startVal) ? startVal.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartValSpecified
        {
            set { startValSpecified = value; }
            get { return startValSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal CurVal
        {
            set { curVal = new EFS_Decimal(value); }
            get { return (null != curVal) ? curVal.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CurValSpecified
        {
            set { curValSpecified = value; }
            get { return curValSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal EndVal
        {
            set { endVal = new EFS_Decimal(value); }
            get { return (null != endVal) ? endVal.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EndValSpecified
        {
            set { endValSpecified = value; }
            get { return endValSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ticker symbol", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSymbol")]
        public EFS_String sym;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol Sfx")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSymbolSfx")]
        public SymbolSfxEnum Sfx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ticker Symbol")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Id", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSecurityID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSecurityIDSource")]
        public SecurityIDSourceEnum Src;
        [System.Xml.Serialization.XmlElementAttribute("UndAID",Order=1)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Id")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security alternate Ids", MinItem = 0)]
        public UndSecAltIDGrp[] UndAID;
        [System.Xml.Serialization.XmlElementAttribute("Stip",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stipulations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stipulations", MinItem = 0)]
        [DictionaryGUI(Page = "UnderlyingStipulations")]
        public UnderlyingStipulations[] Stip;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Suite...", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Product")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingProduct")]
        public ProductEnum Prod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CFI Code")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCFICode")]
        public EFS_String cfi;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSecurityType")]
        public SecurityTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Security Sub Type")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSecuritySubType")]
        public EFS_String subTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "MonthYear")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingMaturityMonthYear")]
        public EFS_MonthYear mmy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingMaturityDate")]
        public EFS_Date mat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Payment Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCouponPaymentDate")]
        public EFS_Date cpnPmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issue date")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingIssueDate")]
        public EFS_Date issued;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo Collateral Security Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingRepoCollateralSecurityType")]
        public SecurityTypeEnum RepoCollSecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Term")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingRepurchaseTerm")]
        public EFS_Integer repoTrm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingRepurchaseRate")]
        public EFS_Decimal repoRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amortization factor (for fixed income)")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingFactor")]
        public EFS_Decimal fctr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Credit Rating")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCreditRating")]
        public EFS_String crdRtg;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Registry")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingInstrRegistry")]
        public InstrRegistryEnum Rgstry;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCountryOfIssue")]
        public string Ctry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "State/Province")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingStateOrProvinceOfIssue")]
        public EFS_String stOrProvnc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Local/City")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingLocaleOfIssue")]
        public EFS_String lcl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption date")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingRedemptionDate")]
        public EFS_Date redeem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingStrikePrice")]
        public EFS_Decimal strkPx;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike currency")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingStrikeCurrency")]
        public string StrkCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option attribute")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingOptAttribute")]
        public EFS_String optA;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Multiplier")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingContractMultiplier")]
        public EFS_Decimal mult;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCouponRate")]
        public EFS_Decimal cpnRt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Exchange", Width = 500)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSecurityExchange")]
        public string Exch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Width = 600, Name = "text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingIssuer")]
        public EFS_String issr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "encoded Issuer Byte length", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedUnderlyingIssuerLen")]
        public EFS_PosInteger encUndIssrLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedUnderlyingIssuer")]
        public EFS_String encUndIssr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security description", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Width = 600, Name = "text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSecurityDesc")]
        public EFS_String desc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "encoded security Byte length", Width = 100)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedUnderlyingSecurityDescLen")]
        public EFS_PosInteger encUndSecDescLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "EncodedUnderlyingSecurityDesc")]
        public EFS_String encUndSecDesc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security description")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "The program under which the underlying commercial paper is issued")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCPProgram")]
        public CPProgramEnum CPPgm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "The registration type of the underlying commercial paper issuance", Width = 600, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCPRegType")]
        public EFS_String cPRegTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingQty")]
        public EFS_Decimal qty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingPx")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dirty price")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingDirtyPrice")]
        public EFS_Decimal dirtPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "End price")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingEndPrice")]
        public EFS_Decimal endPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency value to this collateral at the start of the agreement")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingStartValue")]
        public EFS_Decimal startVal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency value currently to this collateral")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingCurrentValue")]
        public EFS_Decimal curVal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency value to this collateral at the end of the agreement")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingEndValue")]
        public EFS_Decimal endVal;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Suite...")]
        public bool FillerGUI;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SfxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ProdSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cfiSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnPmtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuedSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RepoCollSecTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoTrmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fctrSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RgstrySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CtrySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redeemSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StrkCcySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optASpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnRtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExchSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CPPgmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CPRegTypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qtySpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dirtPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startValSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool curValSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endValSpecified;
        #endregion Specified fields
        #region Constructors
        public UnderlyingInstrument()
        {
            mat = new EFS_Date();
            cpnPmt = new EFS_Date();
            issued = new EFS_Date();
            redeem = new EFS_Date();
            Exch = string.Empty;
            StrkCcy = string.Empty;
            Ccy = string.Empty;
            Ctry = string.Empty;
        }
        #endregion Constructors
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array

        public static object _Exch(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _StrkCcy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _Ccy(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        public static object _Ctry(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

    }
    #endregion UnderlyingInstrument
    #region UnderlyingStipulations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class UnderlyingStipulations : ItemGUI
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Val
        {
            set { val = new EFS_String(value); }
            get { return (null != val) ? val.Value : null; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingStipType")]
        public StipulationTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Stipulation value", Width = 300, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingStipValue")]
        public EFS_String val;
        #endregion GUI
    }
    #endregion UnderlyingStipulations
    #region UndSecAltIDGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class UndSecAltIDGrp : ItemGUI, IEFS_Array
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AltID
        {
            set { altID = new EFS_String(value); }
            get { return (null != altID) ? altID.Value : null; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "value", Width = 200)]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSecurityAltID")]
        public EFS_String altID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [ControlGUI(Name = "source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Page = "field_description", Anchor = "UnderlyingSecurityAltIDSource")]
        public SecurityIDSourceEnum AltIDSrc;
        #endregion GUI
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion UndSecAltIDGrp

    #region YieldData
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class YieldData
    {
        #region Attributes Accessors
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal Yld
        {
            set { yld = new EFS_Decimal(value); }
            get { return (null != yld) ? yld.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool YldSpecified
        {
            set { yldSpecified = value; }
            get { return yldSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime CalcDt
        {
            set { calcDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != calcDt) ? calcDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CalcDtSpecified
        {
            set { calcDtSpecified = value; }
            get { return calcDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime RedDt
        {
            set { redDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != redDt) ? redDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RedDtSpecified
        {
            set { redDtSpecified = value; }
            get { return redDtSpecified; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal RedPx
        {
            set { redPx = new EFS_Decimal(value); }
            get { return (null != redPx) ? redPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RedPxSpecified
        {
            set { redPxSpecified = value; }
            get { return redPxSpecified; }
        }
        #endregion Attributes Accessors
        #region GUI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "YieldType")]
        public FixML.Enum.YieldTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Yield percentage")]
        [DictionaryGUI(Page = "field_description", Anchor = "Yield")]
        public EFS_Decimal yld;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Page = "field_description", Anchor = "YieldCalcDate")]
        public EFS_Date calcDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date to which the yield has been calculated ")]
        [DictionaryGUI(Page = "field_description", Anchor = "YieldRedemptionDate")]
        public EFS_Date redDt;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [DictionaryGUI(Page = "field_description", Anchor = "YieldRedemptionPriceType")]
        public PriceTypeEnum RedPxTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price ")]
        [DictionaryGUI(Page = "field_description", Anchor = "YieldRedemptionPrice")]
        public EFS_Decimal redPx;
        #endregion GUI
        #region Specified fields
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool yldSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calcDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redDtSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redPxSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RedPxTypSpecified;
        #endregion Specified fields
        #region Constructors
        public YieldData()
        {
            calcDt = new EFS_Date();
            redDt = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion YieldData

    #region OrderCancelReject_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("OrdCxlRej", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class OrderCancelReject_message : Abstract_message
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LnkID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrigID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Stat;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string WorkingInd;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime OrigOrdModTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrigOrdModTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ListID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctIDSrc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctTyp;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime OrignDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrignDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime TrdDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TxnTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TxnTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CxlRejRspTo;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CxlRejRsn;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;
    }

    #endregion OrderCancelReject_message
    #region NewOrderSingle_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("Order", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class NewOrderSingle_message : Abstract_message
    {
        [System.Xml.Serialization.XmlElementAttribute("Pty",Order=1)]
        public Parties[] Pty;
        [System.Xml.Serialization.XmlElementAttribute("PreAll",Order=2)]
        public PreAllocGrp[] PreAll;
        [System.Xml.Serialization.XmlElementAttribute("TrdSes",Order=3)]
        public TrdgSesGrp[] TrdSes;
        public Instrument Instrmt;
        public FinancingDetails FinDetls;
        [System.Xml.Serialization.XmlElementAttribute("Undly",Order=4)]
        public UnderlyingInstrument[] Undly;
        [System.Xml.Serialization.XmlElementAttribute("Stip",Order=5)]
        public Stipulations[] Stip;
		[System.Xml.Serialization.XmlElementAttribute("OrdQty", Order = 6)]
        public OrderQtyData OrdQty;
		[System.Xml.Serialization.XmlElementAttribute("SprdBnchmkCurve", Order = 7)]
        public SpreadOrBenchmarkCurveData SprdBnchmkCurve;
		[System.Xml.Serialization.XmlElementAttribute("Yield", Order = 8)]
        public YieldData Yield;
		[System.Xml.Serialization.XmlElementAttribute("Comm", Order = 9)]
        public CommissionData Comm;
		[System.Xml.Serialization.XmlElementAttribute("PegInstr", Order = 10)]
        public PegInstructions PegInstr;
		[System.Xml.Serialization.XmlElementAttribute("DiscInstr", Order = 11)]
        public DiscretionInstructions DiscInstr;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LnkID;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime OrignDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrignDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime TrdDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctIDSrc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DayBkngInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BkngUnit;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PreallocMeth;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AllocID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SettlTyp;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime SettlDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CshMgn;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClrFeeInd;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string HandlInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal MinQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinQtySpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal MaxFloor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaxFloorSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExDest;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProcCode;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal PrevClsPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PrevClsPxSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Side;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LocReqd;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TxnTm;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string QtyTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Typ;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PxTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal Px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal StopPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StopPxSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Ccy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ComplianceID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SolFlag;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IOIID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string QID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TmInForce;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime EfctvTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EfctvTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime ExpireDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpireDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime ExpireTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpireTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GTBkngInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Cpcty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Rstctions;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int CustCpcty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CustCpctySpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ForexReq;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SettlCcy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BkngTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime SettlDt2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDt2Specified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal Qty2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Qty2Specified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal Px2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Px2Specified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PosEfct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Covered;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal MaxShow;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaxShowSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TgtStrategy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TgtStrategyParameters;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal ParticipationRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ParticipationRtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CxllationRights;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MnyLaunderingStat;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RegistID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Designation;
    }

    #endregion NewOrderSingle_message
    #region PreAllocGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class PreAllocGrp
    {
        [System.Xml.Serialization.XmlElementAttribute("Pty",Order=1)]
        public NestedParties[] Pty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ActIDSrc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AllocSettlCcy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IndAllocID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal Qty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QtySpecified;
    }
    #endregion PreAllocGrp
    #region TrdgSesGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class TrdgSesGrp
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesSub;
    }

    #endregion TrdgSesGrp
    #region OrderCancelRequest_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("OrdCxlReq", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class OrderCancelRequest_message : Abstract_message
    {
        [System.Xml.Serialization.XmlElementAttribute("Pty",Order=1)]
        public Parties[] Pty;
		[System.Xml.Serialization.XmlElementAttribute("Instrmt", Order = 2)]
        public Instrument Instrmt;
		[System.Xml.Serialization.XmlElementAttribute("FinDetls", Order = 3)]
        public FinancingDetails FinDetls;
        [System.Xml.Serialization.XmlElementAttribute("Undly",Order=4)]
        public UnderlyingInstrument[] Undly;
		[System.Xml.Serialization.XmlElementAttribute("OrdQty", Order = 5)]
        public OrderQtyData OrdQty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrigID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LnkID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ListID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime OrigOrdModTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrigOrdModTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctIDSrc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Side;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TxnTm;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ComplianceID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;
    }

    #endregion OrderCancelRequest_message
    #region OrderCancelReplaceRequest_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("OrdCxlRplcReq", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class OrderCancelReplaceRequest_message : Abstract_message
    {
        [System.Xml.Serialization.XmlElementAttribute("Pty",Order=1)]
        public Parties[] Pty;
        [System.Xml.Serialization.XmlElementAttribute("PreAll",Order=2)]
        public PreAllocGrp[] PreAll;
        [System.Xml.Serialization.XmlElementAttribute("TrdSes",Order=3)]
        public TrdgSesGrp[] TrdSes;
		[System.Xml.Serialization.XmlElementAttribute("Instrmt", Order = 4)]
        public Instrument Instrmt;
		[System.Xml.Serialization.XmlElementAttribute("FinDetls", Order = 5)]
        public FinancingDetails FinDetls;
        [System.Xml.Serialization.XmlElementAttribute("Undly",Order=6)]
        public UnderlyingInstrument[] Undly;
		[System.Xml.Serialization.XmlElementAttribute("OrdQty", Order = 7)]
        public OrderQtyData OrdQty;
		[System.Xml.Serialization.XmlElementAttribute("SprdBnchmkCurve", Order = 8)]
        public SpreadOrBenchmarkCurveData SprdBnchmkCurve;
		[System.Xml.Serialization.XmlElementAttribute("Yield", Order = 9)]
        public YieldData Yield;
		[System.Xml.Serialization.XmlElementAttribute("PegInstr", Order = 10)]
        public PegInstructions PegInstr;
		[System.Xml.Serialization.XmlElementAttribute("DiscInstr", Order = 11)]
        public DiscretionInstructions DiscInstr;
		[System.Xml.Serialization.XmlElementAttribute("Comm", Order = 12)]
        public CommissionData Comm;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime OrignDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrignDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime TrdDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrigID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LnkID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ListID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime OrigOrdModTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrigOrdModTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctIDSrc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DayBkngInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BkngUnit;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PreallocMeth;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AllocID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SettlTyp;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime SettlDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CshMgn;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClrFeeInd;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string HandlInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal MinQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinQtySpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal MaxFloor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaxFloorSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExDest;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Side;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TxnTm;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string QtyTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Typ;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PxTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal Px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal StopPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StopPxSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TgtStrategy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TgtStrategyParameters;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal ParticipationRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ParticipationRtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ComplianceID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SolFlag;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Ccy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TmInForce;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime EfctvTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EfctvTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime ExpireDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpireDtSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime ExpireTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpireTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GTBkngInst;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Cpcty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Rstctions;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int CustCpcty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CustCpctySpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ForexReq;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SettlCcy;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BkngTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime SettlDt2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDt2Specified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal Qty2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Qty2Specified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal Px2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Px2Specified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PosEfct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Covered;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal MaxShow;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaxShowSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LocReqd;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CxllationRights;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MnyLaunderingStat;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RegistID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Designation;
    }

    #endregion OrderCancelReplaceRequest_message
    #region OrderStatusRequest_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("OrdStatReq", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class OrderStatusRequest_message : Abstract_message
    {
        [System.Xml.Serialization.XmlElementAttribute("Pty",Order=1)]
        public Parties[] Pty;
		[System.Xml.Serialization.XmlElementAttribute("Instrmt", Order = 2)]
        public Instrument Instrmt;
		[System.Xml.Serialization.XmlElementAttribute("FinDetls", Order = 3)]
        public FinancingDetails FinDetls;
        [System.Xml.Serialization.XmlElementAttribute("Undly",Order=4)]
        public UnderlyingInstrument[] Undly;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LnkID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StatReqID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctIDSrc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Side;
    }
    #endregion OrderStatusRequest_message
    #region DontKnowTradeDK_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("DkTrd", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class DontKnowTradeDK_message : Abstract_message
    {
        public Instrument Instrmt;
        [System.Xml.Serialization.XmlElementAttribute("Undly",Order=1)]
        public UnderlyingInstrument[] Undly;
        [System.Xml.Serialization.XmlElementAttribute("Leg",Order=2)]
        public InstrumentLeg[] Leg;
		[System.Xml.Serialization.XmlElementAttribute("OrdQty", Order = 3)]
		public OrderQtyData OrdQty;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DkRsn;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Side;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal LastQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastQtySpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal LastPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastPxSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;
    }
    #endregion DontKnowTradeDK_message
    #region OrderMassCancelRequest_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("OrdMassCxlReq", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class OrderMassCancelRequest_message : Abstract_message
    {
		[System.Xml.Serialization.XmlElementAttribute("Instrmt", Order = 1)]
        public Instrument Instrmt;
		[System.Xml.Serialization.XmlElementAttribute("Undly", Order = 2)]
		public UnderlyingInstrument Undly;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesSub;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Side;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TxnTm;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;
    }

    #endregion OrderMassCancelRequest_message
    #region OrderMassCancelReport_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("OrdMassCxlRpt", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class OrderMassCancelReport_message : Abstract_message
    {
        [System.Xml.Serialization.XmlElementAttribute("AffectOrd",Order=1)]
        public AffectedOrdGrp[] AffectOrd;
		[System.Xml.Serialization.XmlElementAttribute("Instrmt", Order = 2)]
		public Instrument Instrmt;
		[System.Xml.Serialization.XmlElementAttribute("Undly", Order = 3)]
		public UnderlyingInstrument Undly;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID2;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Rsp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RejRsn;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int TotAffctdOrds;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotAffctdOrdsSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesSub;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Side;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TxnTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TxnTmSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;
    }

    #endregion OrderMassCancelReport_message
    #region AffectedOrdGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class AffectedOrdGrp
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrigID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AffctdOrdID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AffctdScndOrdID;
    }

    #endregion AffectedOrdGrp
    #region OrderMassStatusRequest_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("OrdMassStatReq", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class OrderMassStatusRequest_message : Abstract_message
    {
        [System.Xml.Serialization.XmlElementAttribute("Pty",Order=1)]
        public Parties[] Pty;
		[System.Xml.Serialization.XmlElementAttribute("Instrmt", Order = 2)]
		public Instrument Instrmt;
		[System.Xml.Serialization.XmlElementAttribute("Undly", Order = 3)]
		public UnderlyingInstrument Undly;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctIDSrc;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesSub;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Side;
    }

    #endregion OrderMassStatusRequest_message
    #region FIXML
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class FIXML
    {
        [System.Xml.Serialization.XmlElementAttribute("Batch",Order=1)]
        public Batch[] Items;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("4.4")]
        public string v = "4.4";
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("20030618")]
        public string r = "20030618";
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("20040109")]
        public string s = "20040109";
    }

    #endregion FIXML
    #region Batch
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class Batch
    {
		[System.Xml.Serialization.XmlElementAttribute("Hdr", Order = 1)]
        public BatchHeader Hdr;
		[System.Xml.Serialization.XmlElementAttribute("Item", Order = 2)]
        public object Item;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string ID;
    }

    #endregion Batch
    #region BatchHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class BatchHeader : BaseHeader
    {
    }

    #endregion BatchHeader
    #region BusinessMessageReject_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("BizMsgRej", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class BusinessMessageReject_message : Abstract_message
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")]
        public string RefSeqNum;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RefMsgTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BizRejRefID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BizRejRsn;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;
    }

    #endregion BusinessMessageReject_message
    #region NetworkCounterpartySystemStatusRequest_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("NtwkSysStatReq", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class NetworkCounterpartySystemStatusRequest_message : Abstract_message
    {
        [System.Xml.Serialization.XmlElementAttribute("CIDReq",Order=1)]
        public CompIDReqGrp[] CIDReq;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NtwkReqTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NtwkReqID;
    }

    #endregion NetworkCounterpartySystemStatusRequest_message
    #region CompIDReqGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class CompIDReqGrp
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RefCompID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RefSubID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LctnID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DeskID;
    }
    #endregion CompIDReqGrp
    #region NetworkCounterpartySystemStatusResponse_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("NtwkSysStatRsp", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class NetworkCounterpartySystemStatusResponse_message : Abstract_message
    {
        [System.Xml.Serialization.XmlElementAttribute("CIDStat",Order=1)]
        public CompIDStatGrp[] CIDStat;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NtwkStatRspTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NtwkReqID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NtwkRspID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LastNtwkRspID;
    }
    #endregion NetworkCounterpartySystemStatusResponse_message
    #region CompIDStatGrp
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public class CompIDStatGrp
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RefCompID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RefSubID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LctnID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DeskID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StatValu;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StatText;
    }
    #endregion CompIDStatGrp
    #region UserRequest_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("UserReq", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class UserRequest_message : Abstract_message
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UserReqID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UserReqTyp;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Username;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Password;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NewPassword;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string RawDataLength;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RawData;
    }
    #endregion UserRequest_message
    #region UserResponse_message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("UserRsp", Namespace = "http://www.fixprotocol.org/FIXML-4-4", IsNullable = false)]
    public class UserResponse_message : Abstract_message
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UserReqID;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Username;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UserStat;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UserStatText;
    }
    #endregion UserResponse_message
}