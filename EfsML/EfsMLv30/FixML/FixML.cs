#region Using Directives
using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.v30.MarginRequirement;
using FixML.Enum;
using FixML.v50SP1.Enum;
using System;
using System.Reflection;

#endregion Using Directives


namespace FixML.v50SP1
{
    #region FixPartyReference
    public partial class FixPartyReference : HrefGUI
    {
        #region Members
        public string href;
        #endregion Members
    }
    #endregion FixPartyReference

    #region FIXML
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", IsNullable = false)]
    public partial class FIXML : ItemGUI
    {
        #region Members
        #region Abstract_message Message / Batch[] Batch
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Type de message FIXML")]
        public EFS_RadioChoice fixMsgType;
        #region Abstract_message Message
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixMsgTypeMessageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("Message", typeof(Abstract_message), Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("TrdCaptRpt", typeof(TradeCaptureReport_message), Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Message", IsVisible = true)]
        public Abstract_message fixMsgTypeMessage;
        #endregion Abstract_message Message
        #region Batch[] Batch
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixMsgTypeBatchSpecified;
        [System.Xml.Serialization.XmlElementAttribute("Batch", typeof(Batch), Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Batchs")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Batch", IsChild = true, MinItem = 0)]
        public Batch[] fixMsgTypeBatch;
        #endregion Batch[] Batch
        #endregion Abstract_message Message / Batch[] Batch
        #region EFS_String v
        [System.Xml.Serialization.XmlAttributeAttribute("v")]
        public string V
        {
            set { version = new EFS_String(value); }
            get { return version?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VSpecified
        {
            set { versionSpecified = value; }
            get { return versionSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version of FIX")]
        [DictionaryGUI(Tag = "", Anchor = "v")]
        public EFS_String version;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool versionSpecified;
        #endregion EFS_String v
        #region EFS_String r
        [System.Xml.Serialization.XmlAttributeAttribute("r")]
        public string R
        {
            set { releaseDate = new EFS_String(value); }
            get { return releaseDate?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RSpecified
        {
            set { releaseDateSpecified = value; }
            get { return releaseDateSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Release date of FIX")]
        [DictionaryGUI(Tag = "", Anchor = "r")]
        public EFS_String releaseDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool releaseDateSpecified;
        #endregion EFS_String r
        #region EFS_String xv
        [System.Xml.Serialization.XmlAttributeAttribute("xv")]
        public string Xv
        {
            set { cstmApplVerID = new EFS_String(value); }
            get { return cstmApplVerID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool XvSpecified
        {
            set { cstmApplVerIDSpecified = value; }
            get { return cstmApplVerIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Custom application version")]
        [DictionaryGUI(Tag = "", Anchor = "CstmApplVerID")]
        public EFS_String cstmApplVerID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cstmApplVerIDSpecified;
        #endregion EFS_String xv

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string xr;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ev;

        #region EFS_String s
        [System.Xml.Serialization.XmlAttributeAttribute("s")]
        public string S
        {
            set { schemaDate = new EFS_String(value); }
            get { return schemaDate?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SSpecified
        {
            set { schemaDateSpecified = value; }
            get { return schemaDateSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Release date of the Schema")]
        [DictionaryGUI(Tag = "", Anchor = "s")]
        public EFS_String schemaDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool schemaDateSpecified;
        #endregion EFS_String s
        #endregion Members
    }
    #endregion FIXML

    #region Components Block
    #region ApplicationSequenceControl_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class ApplicationSequenceControl_Block : ItemGUI
    {
        #region EFS_String ApplID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ApplID
        {
            set { applID = new EFS_String(value); }
            get { return applID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApplIDSpecified
        {
            set { applIDSpecified = value; }
            get { return applIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Application Id")]
        [DictionaryGUI(Tag = "1180", Anchor = "ApplID")]
        public EFS_String applID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool applIDSpecified;
        #endregion EFS_String ApplID
        #region EFS_PosInteger ApplSeqNum
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")]
        public string ApplSeqNum
        {
            set { applSeqNum = new EFS_PosInteger(value); }
            get { return applSeqNum?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApplSeqNumSpecified
        {
            set { applSeqNumSpecified = value; }
            get { return applSeqNumSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Application Sequence Number")]
        [DictionaryGUI(Tag = "1181", Anchor = "ApplSeqNum")]
        public EFS_PosInteger applSeqNum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool applSeqNumSpecified;
        #endregion EFS_PosInteger ApplSeqNum
        #region EFS_PosInteger ApplLastSeqNum
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")]
        public string ApplLastSeqNum
        {
            set { applLastSeqNum = new EFS_PosInteger(value); }
            get { return applLastSeqNum?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApplLastSeqNumSpecified
        {
            set { applLastSeqNumSpecified = value; }
            get { return applLastSeqNumSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Application Previous Sequence Number")]
        [DictionaryGUI(Tag = "1350", Anchor = "ApplLastSeqNum")]
        public EFS_PosInteger applLastSeqNum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool applLastSeqNumSpecified;
        #endregion EFS_PosInteger ApplLastSeqNum
        #region EFS_String ApplResendFlag
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ApplResendFlag
        {
            set { applResendFlag = new EFS_String(value); }
            get { return applResendFlag?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApplResendFlagSpecified
        {
            set { applResendFlagSpecified = value; }
            get { return applResendFlagSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Application Resend Flag")]
        [DictionaryGUI(Tag = "1352", Anchor = "ApplResendFlag")]
        public EFS_String applResendFlag;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool applResendFlagSpecified;
        #endregion EFS_String ApplResendFlag
    }
    #endregion ApplicationSequenceControl_Block
    #region AttrbGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class AttrbGrp_Block
    {

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Typ;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Val;
    }
    #endregion AttrbGrp_Block
    #region ClrInstGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class ClrInstGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region ClearingInstructionEnum ClrngInstrctn
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instruction")]
        [DictionaryGUI(Tag = "577", Anchor = "ClearingInstruction")]
        public ClearingInstructionEnum ClrngInstrctn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ClrngInstrctnSpecified;
        #endregion ClearingInstructionEnum ClrngInstrctn
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion ClrInstGrp_Block
    #region CommissionData_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class CommissionData_Block : ItemGUI
    {
        #region Members
        #region EFS_Decimal Comm
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Comm
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commission", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "12", Anchor = "Commission")]
        public EFS_Decimal comm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commSpecified;
        #endregion EFS_Decimal Comm
        #region CommTypeEnum CommTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commission Type")]
        [DictionaryGUI(Tag = "13", Anchor = "CommType")]
        public CommTypeEnum CommTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CommTypSpecified;
        #endregion CommTypeEnum CommTyp
        #region string Ccy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commission Currency")]
        [DictionaryGUI(Tag = "479", Anchor = "CommCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        #endregion string Ccy
        #region FundRenewWaivEnum FundRenewWaiv
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fund Renewal Waived")]
        [DictionaryGUI(Tag = "497", Anchor = "FundRenewWaiv")]
        public FundRenewWaivEnum FundRenewWaiv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool FundRenewWaivSpecified;
        #endregion FundRenewWaivEnum FundRenewWaiv
        #endregion Members
    }
    #endregion CommissionData_Block
    #region ContAmtGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class ContAmtGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region ContAmtTypeEnum ContAmtTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [DictionaryGUI(Tag = "519", Anchor = "ContAmtTyp")]
        public ContAmtTypeEnum ContAmtTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ContAmtTypSpecified;
        #endregion ContAmtTypeEnum ContAmtTyp
        #region EFS_Decimal ContAmtValu
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal ContAmtValu
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "520", Anchor = "ContAmtValue")]
        public EFS_Decimal contAmtValu;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contAmtValuSpecified;
        #endregion EFS_Decimal ContAmtValu
        #region string ContAmtCurr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [DictionaryGUI(Tag = "521", Anchor = "ContAmtCurr")]
        public string ContAmtCurr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ContAmtCurrSpecified;
        #endregion string ContAmtCurr
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion ContAmtGrp_Block
    #region EvntGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class EvntGrp_Block : ItemGUI, IEFS_Array
    {
        #region EventTypeEnum EventTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [DictionaryGUI(Tag = "865", Anchor = "EventType")]
        public FixML.v50SP1.Enum.EventTypeEnum EventTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool EventTypSpecified;
        #endregion EventTypeEnum EventTyp
        #region EFS_Date Dt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Tag = "866", Anchor = "EventDate")]
        public EFS_Date dt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dtSpecified;
        #endregion EFS_Date Dt
        #region EFS_Time Tm
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
        public DateTime Tm
        {
            set { tm = new EFS_Time(value.ToString(Cst.FixML_TimeFmt)); }
            get { return (null != tm) ? tm.TimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TmSpecified
        {
            set { tmSpecified = value; }
            get { return tmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time")]
        [DictionaryGUI(Tag = "1145", Anchor = "EventTime")]
        public EFS_Time tm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tmSpecified;
        #endregion EFS_Time Tm
        #region EFS_Decimal Px
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "867", Anchor = "EventPx")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        #endregion EFS_Decimal Px
        #region EFS_String Txt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt
        {
            set { txt = new EFS_String(value); }
            get { return txt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TxtSpecified
        {
            set { txtSpecified = value; }
            get { return txtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Text", Width = 300, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "868", Anchor = "EventText")]
        public EFS_String txt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool txtSpecified;
        #endregion EFS_String Txt

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion EvntGrp_Block
    #region FinancingDetails_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class FinancingDetails_Block : ItemGUI
    {
        #region Members
        #region EFS_String AgmtDesc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AgmtDesc
        {
            set { agmtDesc = new EFS_String(value); }
            get { return agmtDesc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AgmtDescSpecified
        {
            set { agmtDescSpecified = value; }
            get { return agmtDescSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Agreement", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Name", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "913", Anchor = "AgreementDesc")]
        public EFS_String agmtDesc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool agmtDescSpecified;
        #endregion EFS_String AgmtDesc
        #region EFS_String AgmtID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AgmtID
        {
            set { agmtID = new EFS_String(value); }
            get { return agmtID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AgmtIDSpecified
        {
            set { agmtIDSpecified = value; }
            get { return agmtIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "914", Anchor = "AgreementID")]
        public EFS_String agmtID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool agmtIDSpecified;
        #endregion EFS_String AgmtID
        #region EFS_Date AgmtDt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Tag = "915", Anchor = "AgreementDate")]
        public EFS_Date agmtDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool agmtDtSpecified;
        #endregion EFS_Date AgmtDt
        #region string AgmtCcy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [DictionaryGUI(Tag = "918", Anchor = "AgreementCurrency")]
        public string AgmtCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AgmtCcySpecified;
        #endregion AgmtCcy
        #region TerminationTypeEnum TrmTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Agreement")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Type")]
        [DictionaryGUI(Tag = "788", Anchor = "TerminationType")]
        public TerminationTypeEnum TrmTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrmTypSpecified;
        #endregion TerminationTypeEnum TrmTyp
        #region EFS_Date StartDt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Start Date")]
        [DictionaryGUI(Tag = "916", Anchor = "StartDate")]
        public EFS_Date startDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startDtSpecified;
        #endregion EFS_Date StartDt
        #region EFS_Date EndDt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "End Date")]
        [DictionaryGUI(Tag = "917", Anchor = "EndDate")]
        public EFS_Date endDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endDtSpecified;
        #endregion EFS_Date EndDt
        #region DeliveryTypeEnum DlvryTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Delivery Type")]
        [DictionaryGUI(Tag = "919", Anchor = "DeliveryType")]
        public FixML.Enum.DeliveryTypeEnum DlvryTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DlvryTypSpecified;
        #endregion DeliveryTypeEnum DlvryTyp
        #region EFS_Decimal MgnRatio
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Margin Ratio", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "898", Anchor = "MarginRatio")]
        public EFS_Decimal mgnRatio;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mgnRatioSpecified;
        #endregion EFS_Decimal MgnRatio
        #endregion Members
    }
    #endregion FinancingDetails_Block
    #region InstrumentBlock
    [System.SerializableAttribute()]
 //   [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class InstrumentBlock : ItemGUI
    {
        #region Members
        #region EFS_String Sym
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { sym = new EFS_String(value); }
            get { return sym?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SymSpecified
        {
            set { symSpecified = value; }
            get { return symSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sym", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "55", Anchor = "Symbol")]
        public EFS_String sym;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool symSpecified;
        #endregion EFS_String Sym
        #region SymbolSfxEnum Sfx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol Sfx")]
        [DictionaryGUI(Tag = "65", Anchor = "SymbolSfx")]
        public SymbolSfxEnum Sfx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SfxSpecified;
        #endregion SymbolSfxEnum Sfx
        #region EFS_String ID
        /// <summary>
        /// Security identifier value of SecurityIDSource (22) type (e.g. CUSIP, SEDOL, ISIN, etc).  Requires SecurityIDSource.
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { iD = new EFS_String(value); }
            get { return iD?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { iDSpecified = value; }
            get { return iDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Id", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "48", Anchor = "SecurityID")]
        public EFS_String iD;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool iDSpecified;
        #endregion EFS_String ID
        #region SecurityIDSourceEnum Src
        /// <summary>
        /// Identifies class or source of the SecurityID (48) value.  Required if SecurityID is specified.
        /// 100+ are reserved for private security identifications
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [DictionaryGUI(Tag = "22", Anchor = "SecurityIDSource")]
        public SecurityIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion SecurityIDSourceEnum Src
        #region SecAltIDGrp_Block[] AID
        [System.Xml.Serialization.XmlElementAttribute("AID", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Alternate Id", IsChild = true, MinItem = 0)]
        public SecAltIDGrp_Block[] AID;
        #endregion SecAltIDGrp_Block[] AID
        #region ProductEnum Prod
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Id")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Product")]
        [DictionaryGUI(Tag = "460", Anchor = "Product")]
        public ProductEnum Prod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ProdSpecified;
        #endregion ProductEnum Prod
        #region EFS_String ProdCmplx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProdCmplx
        {
            set { prodCmplx = new EFS_String(value); }
            get { return prodCmplx?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ProdCmplxSpecified
        {
            set { prodCmplxSpecified = value; }
            get { return prodCmplxSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Product Complex", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1227", Anchor = "ProdCmplx")]
        public EFS_String prodCmplx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool prodCmplxSpecified;
        #endregion EFS_String ProdCmplx
        #region EFS_String SecGrp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SecGrp
        {
            set { secGrp = new EFS_String(value); }
            get { return secGrp?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SecGrpSpecified
        {
            set { secGrpSpecified = value; }
            get { return secGrpSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Group", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1151", Anchor = "SecurityGroup")]
        public EFS_String secGrp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secGrpSpecified;
        #endregion EFS_String SecGrp
        #region EFS_String CFI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CFI
        {
            set { cfi = new EFS_String(value); }
            get { return cfi?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFISpecified
        {
            set { cfiSpecified = value; }
            get { return cfiSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CFI Code", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "461", Anchor = "CFICode")]
        public EFS_String cfi;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cfiSpecified;
        #endregion EFS_String CFI
        #region SecurityTypeEnum SecTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [DictionaryGUI(Tag = "1151", Anchor = "SecurityType")]
        public SecurityTypeEnum SecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecTypSpecified;
        #endregion SecurityTypeEnum SecTyp
        #region EFS_String SubTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SubTyp
        {
            set { subTyp = new EFS_String(value); }
            get { return subTyp?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubTypSpecified
        {
            set { subTypSpecified = value; }
            get { return subTypSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sub Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "762", Anchor = "SecuritySubType")]
        public EFS_String subTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool subTypSpecified;
        #endregion EFS_String SubTyp
        #region EFS_MonthYear MMY
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MMY
        {
            set { mmy = new EFS_MonthYear(value); }
            get { return mmy?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MMYSpecified
        {
            set { mmySpecified = value; }
            get { return mmySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "MonthYear", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "200", Anchor = "MaturityMonthYear")]
        public EFS_MonthYear mmy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mmySpecified;
        #endregion EFS_String MMY
        #region EFS_Date MatDt
        /// <summary>
        /// Obtient ou définie la date de maturité de l'asset
        /// </summary>
        /// FI 20131126 [19271] serialisation de type string
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MatDt
        {
            set { matDt = new EFS_Date(value); }
            get { return ((null != matDt) ? matDt.Value : Convert.ToString(null)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MatDtSpecified
        {
            set { matDtSpecified = value; }
            get { return matDtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        [DictionaryGUI(Tag = "541", Anchor = "MaturityDate")]
        public EFS_Date matDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matDtSpecified;
        #endregion EFS_Date MatDt
        #region EFS_Time MatTm
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
        public DateTime MatTm
        {
            set { matTm = new EFS_Time(value.ToString(Cst.FixML_TimeFmt)); }
            get { return (null != matTm) ? matTm.TimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MatTmSpecified
        {
            set { matTmSpecified = value; }
            get { return matTmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time")]
        [DictionaryGUI(Tag = "1079", Anchor = "MaturityTime")]
        public EFS_Time matTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matTmSpecified;
        #endregion EFS_Time MatTm
        #region EFS_String SettlOnOpenFlag
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SettlOnOpenFlag
        {
            set { settlOnOpenFlag = new EFS_String(value); }
            get { return settlOnOpenFlag?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlOnOpenFlagSpecified
        {
            set { settlOnOpenFlagSpecified = value; }
            get { return settlOnOpenFlagSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settle On Open", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "966", Anchor = "SettleOnOpenFlag")]
        public EFS_String settlOnOpenFlag;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlOnOpenFlagSpecified;
        #endregion EFS_String SettlOnOpenFlag
        #region EFS_String AsgnMeth
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AsgnMeth
        {
            set { asgnMeth = new EFS_String(value); }
            get { return asgnMeth?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AsgnMethSpecified
        {
            set { asgnMethSpecified = value; }
            get { return asgnMethSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Assignment Method", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1049", Anchor = "InstrmtAssignmentMethod")]
        public EFS_String asgnMeth;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool asgnMethSpecified;
        #endregion EFS_String AsgnMeth
        #region SecurityStatusEnum Status
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Status")]
        [DictionaryGUI(Tag = "965", Anchor = "SecurityStatus")]
        public SecurityStatusEnum Status;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StatusSpecified;
        #endregion SecurityStatusEnum Status
        #region EFS_Date CpnPmt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Payment Date")]
        [DictionaryGUI(Tag = "224", Anchor = "CouponPaymentDate")]
        public EFS_Date cpnPmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnPmtSpecified;
        #endregion EFS_Date CpnPmt
        #region EFS_Date Issued
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Issued
        {
            set
            {
                issued = new EFS_Date(value); 
            }
            get
            {
                return ((null != issued) ? issued.Value : Convert.ToString(null) );
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssuedSpecified
        {
            set { issuedSpecified = value; }
            get { return issuedSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issue date")]
        [DictionaryGUI(Tag = "225", Anchor = "Issuedate")]
        public EFS_Date issued;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuedSpecified;
        #endregion EFS_Date Issued
        #region SecurityTypeEnum RepoCollSecTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo Collateral Security Type")]
        [DictionaryGUI(Tag = "239", Anchor = "RepoCollateralSecurityType")]
        public SecurityTypeEnum RepoCollSecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RepoCollSecTypSpecified;
        #endregion SecurityTypeEnum RepoCollSecTyp
        #region EFS_Integer RepoTrm
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string RepoTrm
        {
            set { repoTrm = new EFS_Integer(value); }
            get { return repoTrm?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoTrmSpecified
        {
            set { repoTrmSpecified = value; }
            get { return repoTrmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "226", Anchor = "RepurchaseTerm")]
        public EFS_Integer repoTrm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoTrmSpecified;
        #endregion EFS_Integer RepoTrm
        #region EFS_Decimal RepoRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "227", Anchor = "RepurchaseRate")]
        public EFS_Decimal repoRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoRtSpecified;
        #endregion EFS_Decimal RepoRt
        #region EFS_Decimal Fctr
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amortization factor", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "228", Anchor = "Factor")]
        public EFS_Decimal fctr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fctrSpecified;
        #endregion EFS_Decimal Fctr
        #region EFS_String CrdRtg
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CrdRtg
        {
            set { crdRtg = new EFS_String(value); }
            get { return crdRtg?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CrdRtgSpecified
        {
            set { crdRtgSpecified = value; }
            get { return crdRtgSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit Rating", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "255", Anchor = "CreditRating")]
        public EFS_String crdRtg;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool crdRtgSpecified;
        #endregion EFS_String CrdRtg
        #region InstrRegistryEnum Rgstry
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Registry")]
        [DictionaryGUI(Tag = "543", Anchor = "InstrRegistry")]
        public InstrRegistryEnum Rgstry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RgstrySpecified;
        #endregion InstrRegistryEnum Rgstry
        #region string IssuCtry
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country Of Issue")]
        [DictionaryGUI(Tag = "470", Anchor = "CountryOfIssue")]
        public string IssuCtry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool IssuCtrySpecified;
        #endregion IssuCtry
        #region EFS_String StPrv
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StPrv
        {
            set { stPrv = new EFS_String(value); }
            get { return stPrv?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StPrvSpecified
        {
            set { stPrvSpecified = value; }
            get { return stPrvSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "State/Province Of Issue", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "471", Anchor = "StateOrProvinceOfIssue")]
        public EFS_String stPrv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stPrvSpecified;
        #endregion EFS_String StPrv
        #region EFS_String Lcl
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Lcl
        {
            set { lcl = new EFS_String(value); }
            get { return lcl?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LclSpecified
        {
            set { lclSpecified = value; }
            get { return lclSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Local/City Of Issue", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "472", Anchor = "LocaleOfIssue")]
        public EFS_String lcl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lclSpecified;
        #endregion EFS_String Lcl
        #region EFS_Date Redeem
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption date")]
        [DictionaryGUI(Tag = "240", Anchor = "RedemptionDate")]
        public EFS_Date redeem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redeemSpecified;
        #endregion EFS_Date Redeem
        #region EFS_Decimal StrkPx
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "decimal")]
        public decimal StrkPx
        {
            set
            {
                strkPx = new EFS_Decimal(value);
            }
            get { return (null != strkPx) ? strkPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StrkPxSpecified
        {
            set { strkPxSpecified = value; }
            get { return strkPxSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "202", Anchor = "StrikePrice")]
        public EFS_Decimal strkPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkPxSpecified;
        #endregion EFS_Decimal StrkPx
        #region string StrkCcy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike currency")]
        [DictionaryGUI(Tag = "947", Anchor = "StrikeCurrency")]
        public string StrkCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StrkCcySpecified;
        #endregion string StrkCcy
        #region EFS_Decimal StrkMult
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal StrkMult
        {
            set { strkMult = new EFS_Decimal(value); }
            get { return (null != strkMult) ? strkMult.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StrkMultSpecified
        {
            set { strkMultSpecified = value; }
            get { return strkMultSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Multiplier", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "967", Anchor = "StrikeMultiplier")]
        public EFS_Decimal strkMult;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkMultSpecified;
        #endregion EFS_Decimal StrkMult
        #region EFS_Decimal StrkValu
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal StrkValu
        {
            set { strkValu = new EFS_Decimal(value); }
            get { return (null != strkValu) ? strkValu.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StrkValuSpecified
        {
            set { strkValuSpecified = value; }
            get { return strkValuSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "968", Anchor = "StrikeValue")]
        public EFS_Decimal strkValu;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkValuSpecified;
        #endregion EFS_Decimal StrkValu
        #region EFS_String OptAt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptAt
        {
            set { optAt = new EFS_String(value); }
            get { return optAt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OptAtSpecified
        {
            set { optAtSpecified = value; }
            get { return optAtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option attribute", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "206", Anchor = "OptAttribute")]
        public EFS_String optAt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optAtSpecified;
        #endregion EFS_String OptAt
        #region EFS_Decimal Mult
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Multiplier", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "231", Anchor = "ContractMultiplier")]
        public EFS_Decimal mult;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multSpecified;
        #endregion EFS_Decimal Mult
        #region EFS_Decimal MinPxIncr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal MinPxIncr
        {
            set { minPxIncr = new EFS_Decimal(value); }
            get { return (null != minPxIncr) ? minPxIncr.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinPxIncrSpecified
        {
            set { minPxIncrSpecified = value; }
            get { return minPxIncrSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Min Price Increment", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "969", Anchor = "MinPriceIncrement")]
        public EFS_Decimal minPxIncr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minPxIncrSpecified;
        #endregion EFS_Decimal MinPxIncr
        #region EFS_Decimal MinPxIncrAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal MinPxIncrAmt
        {
            set { minPxIncrAmt = new EFS_Decimal(value); }
            get { return (null != minPxIncrAmt) ? minPxIncrAmt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinPxIncrAmtSpecified
        {
            set { minPxIncrAmtSpecified = value; }
            get { return minPxIncrAmtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Min Price Increment Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1146", Anchor = "MinPriceIncrementAmount")]
        public EFS_Decimal minPxIncrAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minPxIncrAmtSpecified;
        #endregion EFS_Decimal MinPxIncrAmt
        #region UnitOfMeasureEnum UOM
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unit Of Measure")]
        [DictionaryGUI(Tag = "996", Anchor = "UnitOfMeasure")]
        public UnitOfMeasureEnum UOM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool UOMSpecified;
        #endregion UnitOfMeasureEnum UOM
        #region EFS_Decimal UOMQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal UOMQty
        {
            set { uomQty = new EFS_Decimal(value); }
            get { return (null != uomQty) ? uomQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UOMQtySpecified
        {
            set { uomQtySpecified = value; }
            get { return uomQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unit Of Measure Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1147", Anchor = "UnitOfMeasureQty")]
        public EFS_Decimal uomQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool uomQtySpecified;
        #endregion EFS_Decimal UOMQty
        #region UnitOfMeasureEnum PxUOM
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Unit Of Measure")]
        [DictionaryGUI(Tag = "1191", Anchor = "PriceUnitOfMeasure")]
        public UnitOfMeasureEnum PxUOM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PxUOMSpecified;
        #endregion UnitOfMeasureEnum PxUOM
        #region EFS_Decimal PxUOMQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal PxUOMQty
        {
            set { pxUOMQty = new EFS_Decimal(value); }
            get { return (null != pxUOMQty) ? pxUOMQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxUOMQtySpecified
        {
            set { pxUOMQtySpecified = value; }
            get { return pxUOMQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Unit Of Measure Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1192", Anchor = "PriceUnitOfMeasureQty")]
        public EFS_Decimal pxUOMQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxUOMQtySpecified;
        #endregion EFS_Decimal PxUOMQty
        #region SettlMethodEnum SettlMeth
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Method")]
        [DictionaryGUI(Tag = "1193", Anchor = "SettlMethod")]
        public SettlMethodEnum SettlMeth;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlMethSpecified;
        #endregion SettlMethodEnum SettlMeth
        #region ExerciseStyleEnum ExerStyle
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Style")]
        [DictionaryGUI(Tag = "1194", Anchor = "ExerciseStyle")]
        public DerivativeExerciseStyleEnum ExerStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExerStyleSpecified;
        #endregion ExerciseStyleEnum ExerStyle
        #region EFS_Decimal OptPayAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal OptPayAmt
        {
            set { optPayAmt = new EFS_Decimal(value); }
            get { return (null != optPayAmt) ? optPayAmt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OptPayAmtSpecified
        {
            set { optPayAmtSpecified = value; }
            get { return optPayAmtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option Pay Out Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1195", Anchor = "OptPayAmount")]
        public EFS_Decimal optPayAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optPayAmtSpecified;
        #endregion EFS_Decimal OptPayAmt
        #region PriceQuoteMethodEnum PxQteMeth
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Quote Method")]
        [DictionaryGUI(Tag = "1196", Anchor = "PriceQuoteMethod")]
        public PriceQuoteMethodEnum PxQteMeth;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PxQteMethSpecified;
        #endregion PriceQuoteMethodEnum PxQteMeth
        #region FuturesValuationMethodEnum FutValMeth
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Futures Valuation Method")]
        [DictionaryGUI(Tag = "1197", Anchor = "FuturesValuationMethod")]
        public FuturesValuationMethodEnum FutValMeth;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool FutValMethSpecified;
        #endregion FuturesValuationMethodEnum FutValMeth
        #region ListMethodEnum ListMeth
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Listed Method")]
        [DictionaryGUI(Tag = "1198", Anchor = "ListMethod")]
        public ListMethodEnum ListMeth;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ListMethSpecified;
        #endregion ListMethodEnum ListMeth
        #region EFS_Decimal CapPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal CapPx
        {
            set { capPx = new EFS_Decimal(value); }
            get { return (null != capPx) ? capPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CapPxSpecified
        {
            set { capPxSpecified = value; }
            get { return capPxSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1199", Anchor = "CapPrice")]
        public EFS_Decimal capPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool capPxSpecified;
        #endregion EFS_Decimal CapPx
        #region EFS_Decimal FlrPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Decimal FlrPx
        {
            set { flrPx = new EFS_Decimal(value); }
            get { return (null != flrPx) ? flrPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FlrPxSpecified
        {
            set { flrPxSpecified = value; }
            get { return flrPxSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FloorPrice", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1200", Anchor = "FloorPrice")]
        public EFS_Decimal flrPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool flrPxSpecified;
        #endregion EFS_Decimal FlrPx
        #region PutOrCallEnum PutCall
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Put Or Call")]
        [DictionaryGUI(Tag = "201", Anchor = "PutOrCall")]
        public PutOrCallEnum PutCall;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PutCallSpecified;
        #endregion PutOrCallEnum PutCall
        #region EFS_String FlexInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FlexInd
        {
            set { flexInd = new EFS_String(value); }
            get { return flexInd?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FlexIndSpecified
        {
            set { flexIndSpecified = value; }
            get { return flexIndSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Flexible Indicator", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1244", Anchor = "FlexibleIndicator")]
        public EFS_String flexInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool flexIndSpecified;
        #endregion EFS_String FlexInd
        #region EFS_String FlexProdElig
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FlexProdElig
        {
            set { flexProdElig = new EFS_String(value); }
            get { return flexProdElig?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FlexProdEligSpecified
        {
            set { flexProdEligSpecified = value; }
            get { return flexProdEligSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Flexible Product Eligibility Indicator", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1242", Anchor = "FlexProductEligibilityIndicator")]
        public EFS_String flexProdElig;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool flexProdEligSpecified;
        #endregion EFS_String FlexProdElig
        #region TimeUnitEnum TmUnit
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time Unit")]
        [DictionaryGUI(Tag = "997", Anchor = "TimeUnit")]
        public TimeUnitEnum TmUnit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TmUnitSpecified;
        #endregion TimeUnitEnum TmUnit
        #region EFS_Decimal CpnRt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CpnRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "223", Anchor = "CouponRate")]
        public EFS_Decimal cpnRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnRtSpecified;
        #endregion EFS_Decimal CpnRt
        #region string Exch
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Exchange", Width = 500)]
        [DictionaryGUI(Tag = "207", Anchor = "SecurityExchange")]
        public string Exch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExchSpecified;
        #endregion string Exch
        #region EFS_Integer PosLmt
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string PosLmt
        {
            set { posLmt = new EFS_Integer(value); }
            get { return posLmt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PosLmtSpecified
        {
            set { posLmtSpecified = value; }
            get { return posLmtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Position Limit", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "970", Anchor = "PositionLimit")]
        public EFS_Integer posLmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool posLmtSpecified;
        #endregion EFS_Integer PosLmt
        #region EFS_Integer NTPosLmt
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string NTPosLmt
        {
            set { nTPosLmt = new EFS_Integer(value); }
            get { return nTPosLmt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NTPosLmtSpecified
        {
            set { nTPosLmtSpecified = value; }
            get { return nTPosLmtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Near-term Position Limit", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "971", Anchor = "NTPositionLimit")]
        public EFS_Integer nTPosLmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nTPosLmtSpecified;
        #endregion EFS_Integer NTPosLmt
        #region EFS_String Issr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Issr
        {
            set { issr = new EFS_String(value); }
            get { return issr?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssrSpecified
        {
            set { issrSpecified = value; }
            get { return issrSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "106", Anchor = "Issuer")]
        public EFS_String issr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issrSpecified;
        #endregion EFS_String Issr
        #region EFS_PosInteger EncIssrLen
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncIssrLen
        {
            set { encIssrLen = new EFS_PosInteger(value); }
            get { return encIssrLen?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncIssrLenSpecified
        {
            set { encIssrLenSpecified = value; }
            get { return encIssrLenSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Issuer Byte Length", Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "348", Anchor = "EncodedIssuerLen")]
        public EFS_PosInteger encIssrLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encIssrLenSpecified;
        #endregion EFS_PosInteger EncIssrLen
        #region EFS_String EncIssr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncIssr
        {
            set { encIssr = new EFS_String(value); }
            get { return encIssr?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncIssrSpecified
        {
            set { encIssrSpecified = value; }
            get { return encIssrSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Issuer", Width = 500, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "349", Anchor = "EncodedIssuer")]
        public EFS_String encIssr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encIssrSpecified;
        #endregion EFS_String EncIssr
        #region EFS_String Desc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Desc
        {
            set { desc = new EFS_String(value); }
            get { return desc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DescSpecified
        {
            set { descSpecified = value; }
            get { return descSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Description", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "107", Anchor = "SecurityDesc")]
        public EFS_String desc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descSpecified;
        #endregion EFS_String Desc
        #region EFS_PosInteger EncSecDescLen
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncSecDescLen
        {
            set { encSecDescLen = new EFS_PosInteger(value); }
            get { return encSecDescLen?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncSecDescLenSpecified
        {
            set { encSecDescLenSpecified = value; }
            get { return encSecDescLenSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Security Description Byte Length", Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "350", Anchor = "EncodedSecurityDescLen")]
        public EFS_PosInteger encSecDescLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encSecDescLenSpecified;
        #endregion EFS_PosInteger EncSecDescLen
        #region EFS_String EncSecDesc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncSecDesc
        {
            set { encSecDesc = new EFS_String(value); }
            get { return encSecDesc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncSecDescSpecified
        {
            set { encSecDescSpecified = value; }
            get { return encSecDescSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Security Description", Width = 500, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "351", Anchor = "EncodedSecurityDesc")]
        public EFS_String encSecDesc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encSecDescSpecified;
        #endregion EFS_String EncSecDesc
        #region SecurityXML_Block SecXML
        [System.Xml.Serialization.XmlElementAttribute("SecXML", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "SecurityXML")]
        public SecurityXML_Block SecXML;
        #endregion SecurityXML_Block SecXML
        #region EFS_String Pool
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Pool
        {
            set { pool = new EFS_String(value); }
            get { return pool?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PoolSpecified
        {
            set { poolSpecified = value; }
            get { return poolSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "MBS/ABS Pool", Width = 600, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "691", Anchor = "Pool")]
        public EFS_String pool;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool poolSpecified;
        #endregion EFS_String Pool
        #region EFS_String CSetMo
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CSetMo
        {
            set { cSetMo = new EFS_String(value); }
            get { return cSetMo?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CSetMoSpecified
        {
            set { cSetMoSpecified = value; }
            get { return cSetMoSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Settlement MonthYear", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "667", Anchor = "ContractSettlMonth")]
        public EFS_String cSetMo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cSetMoSpecified;
        #endregion EFS_String CSetMo
        #region CPProgramEnum CPPgm
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commercial Paper Program")]
        [DictionaryGUI(Tag = "875", Anchor = "CPProgram")]
        public CPProgramEnum CPPgm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CPPgmSpecified;
        #endregion CPProgramEnum CPPgm
        #region EFS_String CPRegT
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CPRegT
        {
            set { cPRegT = new EFS_String(value); }
            get { return cPRegT?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CPRegTSpecified
        {
            set { cPRegTSpecified = value; }
            get { return cPRegTSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commercial Paper Registration Type", Width = 600, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "876", Anchor = "CPRegType")]
        public EFS_String cPRegT;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cPRegTSpecified;
        #endregion EFS_String CPRegT
        #region EvntGrp_Block[] Evnt
        [System.Xml.Serialization.XmlElementAttribute("Evnt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Events")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event", IsChild = true, MinItem = 0)]
        public EvntGrp_Block[] Evnt;
        #endregion EvntGrp_Block[] Evnt
        #region EFS_Date Dated
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dated Date")]
        [DictionaryGUI(Tag = "873", Anchor = "DatedDate")]
        public EFS_Date dated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool datedSpecified;
        #endregion EFS_Date Dated
        #region EFS_Date IntAcrl
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Accrual Date")]
        [DictionaryGUI(Tag = "874", Anchor = "InterestAccrualDate")]
        public EFS_Date intAcrl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intAcrlSpecified;
        #endregion EFS_Date IntAcrl

        #region EFS_Date CntrctDt FI 20220209 [25699] Add
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime CntrctDt
        {
            set { cntrctDt = new EFS_Date(value.ToString(DtFunc.FmtISODate)); }
            get { return (null != cntrctDt) ? cntrctDt.DateValue : Convert.ToDateTime(null); }
        }


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CntrctDtSpecified
        {
            set { cntrctDtSpecified = value; }
            get { return cntrctDtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Date")]
        [DictionaryGUI(Tag = "30866", Anchor = "ContractDate")]
        public EFS_Date cntrctDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cntrctDtSpecified;
        #endregion EFS_Date CntrctDt 

        #region ContractFrequencyEnum CntrctFreq FI 20220209 [25699] Add
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Frequency")]
        [DictionaryGUI(Tag = "30867", Anchor = "ContractFrequency")]
        public ContractFrequencyEnum CntrctFreq;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CntrctFreqSpecified;
        #endregion ContractFrequencyEnum CntrctFreq

        #region InstrumentParties_Block[] Pty
        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Instrument Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Instrument Party", IsChild = true, MinItem = 0)]
        public InstrumentParties_Block[] Pty;
        #endregion InstrumentParties_Block[] Pty
        #endregion Members
    }
    #endregion InstrumentBlock
    #region InstrumentExtension_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class InstrumentExtension_Block
    {

        [System.Xml.Serialization.XmlElementAttribute("Attrb", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        public AttrbGrp_Block[] Attrb;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DeliveryFormEnum DlvryForm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DlvryFormSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal PctAtRisk;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PctAtRiskSpecified;
    }
    #endregion InstrumentExtension_Block
    #region InstrumentParties_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class InstrumentParties_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region FixPartyReference ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set {
                id = new FixPartyReference
                {
                    href = value
                };
            }
            get { return id?.href; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Instrument Party ID", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [DictionaryGUI(Tag = "1019", Anchor = "InstrumentPartyID")]
        public FixPartyReference id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion FixPartyReference ID
        #region PartyIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [DictionaryGUI(Tag = "1050", Anchor = "InstrumentPartyIDSource")]
        public PartyIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion PartyIDSourceEnum Src
        #region PartyRoleEnum R
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Role")]
        [DictionaryGUI(Tag = "1051", Anchor = "InstrumentPartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RSpecified;
        #endregion PartyRoleEnum R
        #region InstrumentPtysSubGrp_Block[] Sub
        [System.Xml.Serialization.XmlElementAttribute("Sub", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Id", MinItem = 0)]
        public InstrumentPtysSubGrp_Block[] Sub;
        #endregion InstrumentPtysSubGrp_Block[] Sub
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion InstrumentParties_Block
    #region InstrumentPtysSubGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class InstrumentPtysSubGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Party Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1053", Anchor = "InstrumentPartySubID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_String ID
        #region PartySubIDTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Party Sub ID Type")]
        [DictionaryGUI(Tag = "1054", Anchor = "InstrumentPartySubIDType")]
        public PartySubIDTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion PartySubIDTypeEnum Typ
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion InstrumentPtysSubGrp_Block
    #region InstrumentLeg_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class InstrumentLeg_Block : ItemGUI
    {
        #region Members
        #region EFS_String Sym
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { sym = new EFS_String(value); }
            get { return sym?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SymSpecified
        {
            set { symSpecified = value; }
            get { return symSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "600", Anchor = "LegSymbol")]
        public EFS_String sym;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool symSpecified;
        #endregion EFS_String Sym
        #region SymbolSfxEnum Sfx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol Suffix")]
        [DictionaryGUI(Tag = "601", Anchor = "LegSymbolSfx")]
        public SymbolSfxEnum Sfx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SfxSpecified;
        #endregion SymbolSfxEnum Sfx
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { iD = new EFS_String(value); }
            get { return iD?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { iDSpecified = value; }
            get { return iDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "602", Anchor = "LegSecurityID")]
        public EFS_String iD;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool iDSpecified;
        #endregion EFS_String ID
        #region SecurityIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security ID Source")]
        [DictionaryGUI(Tag = "603", Anchor = "LegSecurityIDSource")]
        public SecurityIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion SecurityIDSourceEnum Src
        #region LegSecAltIDGrp_Block[] LegAID
        [System.Xml.Serialization.XmlElementAttribute("LegAID", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Security Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Security Alternate Id", MinItem = 0)]
        public LegSecAltIDGrp_Block[] LegAID;
        #endregion LegSecAltIDGrp_Block[] LegAID
        #region ProductEnum Prod
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Product")]
        [DictionaryGUI(Tag = "607", Anchor = "LegProduct")]
        public ProductEnum Prod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ProdSpecified;
        #endregion ProductEnum Prod
        #region EFS_String CFI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CFI
        {
            set { cfi = new EFS_String(value); }
            get { return cfi?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFISpecified
        {
            set { cfiSpecified = value; }
            get { return cfiSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CFI Code", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "608", Anchor = "LegCFICode")]
        public EFS_String cfi;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cfiSpecified;
        #endregion EFS_String CFI
        #region SecurityTypeEnum SecTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Type")]
        [DictionaryGUI(Tag = "609", Anchor = "LegSecurityType")]
        public SecurityTypeEnum SecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecTypSpecified;
        #endregion SecurityTypeEnum SecTyp
        #region EFS_String SecSubTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SecSubTyp
        {
            set { secSubTyp = new EFS_String(value); }
            get { return secSubTyp?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SecSubTypSpecified
        {
            set { secSubTypSpecified = value; }
            get { return secSubTypSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Sub Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "764", Anchor = "LegSecuritySubType")]
        public EFS_String secSubTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secSubTypSpecified;
        #endregion EFS_String SecSubTyp
        #region EFS_String MMY
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MMY
        {
            set { mMY = new EFS_String(value); }
            get { return mMY?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MMYSpecified
        {
            set { mMYSpecified = value; }
            get { return mMYSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Month Year", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "610", Anchor = "LegMaturityMonthYear")]
        public EFS_String mMY;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mMYSpecified;
        #endregion EFS_String MMY
        #region EFS_Date Mat
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Date")]
        [DictionaryGUI(Tag = "611", Anchor = "LegMaturityDate")]
        public EFS_Date mat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matSpecified;
        #endregion EFS_Date Mat
        #region EFS_Time MatTm
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
        public DateTime MatTm
        {
            set { matTm = new EFS_Time(value.ToString(Cst.FixML_TimeFmt)); }
            get { return (null != matTm) ? matTm.TimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MatTmSpecified
        {
            set { matTmSpecified = value; }
            get { return matTmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Time")]
        [DictionaryGUI(Tag = "1212", Anchor = "LegMaturityTime")]
        public EFS_Time matTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matTmSpecified;
        #endregion EFS_Time MatTm
        #region EFS_Date CpnPmt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Payment Date")]
        [DictionaryGUI(Tag = "248", Anchor = "LegCouponPaymentDate")]
        public EFS_Date cpnPmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnPmtSpecified;
        #endregion EFS_Date CpnPmt
        #region EFS_Date Issued
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issue Date")]
        [DictionaryGUI(Tag = "249", Anchor = "LegIssueDate")]
        public EFS_Date issued;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuedSpecified;
        #endregion EFS_Date Issued
        #region EFS_Integer RepoCollSecTyp
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string RepoCollSecTyp
        {
            set { repoCollSecTyp = new EFS_Integer(value); }
            get { return repoCollSecTyp?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoCollSecTypSpecified
        {
            set { repoCollSecTypSpecified = value; }
            get { return repoCollSecTypSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo Collateral Security Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "250", Anchor = "LegRepoCollateralSecurityType")]
        public EFS_Integer repoCollSecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoCollSecTypSpecified;
        #endregion EFS_Integer RepoCollSecTyp
        #region EFS_Integer RepoTrm
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string RepoTrm
        {
            set { repoTrm = new EFS_Integer(value); }
            get { return repoTrm?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoTrmSpecified
        {
            set { repoTrmSpecified = value; }
            get { return repoTrmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "251", Anchor = "LegRepurchaseTerm")]
        public EFS_Integer repoTrm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoTrmSpecified;
        #endregion EFS_Integer RepoTrm
        #region EFS_Decimal RepoRt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal RepoRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "252", Anchor = "LegRepurchaseRate")]
        public EFS_Decimal repoRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoRtSpecified;
        #endregion EFS_Decimal RepoRt
        #region EFS_Decimal Fctr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Fctr
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Factor", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "253", Anchor = "LegFactor")]
        public EFS_Decimal fctr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fctrSpecified;
        #endregion EFS_Decimal Fctr
        #region EFS_String CrdRtg
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CrdRtg
        {
            set { crdRtg = new EFS_String(value); }
            get { return crdRtg?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CrdRtgSpecified
        {
            set { crdRtgSpecified = value; }
            get { return crdRtgSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CreditRating", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "257", Anchor = "LegCreditRating")]
        public EFS_String crdRtg;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool crdRtgSpecified;
        #endregion EFS_String CrdRtg
        #region EFS_String Rgstry
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Rgstry
        {
            set { rgstry = new EFS_String(value); }
            get { return rgstry?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RgstrySpecified
        {
            set { rgstrySpecified = value; }
            get { return rgstrySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Registry", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "599", Anchor = "LegInstrRegistry")]
        public EFS_String rgstry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rgstrySpecified;
        #endregion EFS_String Rgstry
        #region string Ctry
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country Of Issue")]
        [DictionaryGUI(Tag = "596", Anchor = "LegCountryOfIssue")]
        public string Ctry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CtrySpecified;
        #endregion string StrkCcy
        #region EFS_String StOrProvnc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StOrProvnc
        {
            set { stOrProvnc = new EFS_String(value); }
            get { return stOrProvnc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StOrProvncSpecified
        {
            set { stOrProvncSpecified = value; }
            get { return stOrProvncSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "State/Province Of Issue", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "597", Anchor = "LegStateOrProvinceOfIssue")]
        public EFS_String stOrProvnc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stOrProvncSpecified;
        #endregion EFS_String StOrProvnc
        #region EFS_String Lcl
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Lcl
        {
            set { lcl = new EFS_String(value); }
            get { return lcl?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LclSpecified
        {
            set { lclSpecified = value; }
            get { return lclSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Locale Of Issue", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "598", Anchor = "LegLocaleOfIssue")]
        public EFS_String lcl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lclSpecified;
        #endregion EFS_String Lcl
        #region EFS_Date Redeem
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption Date")]
        [DictionaryGUI(Tag = "254", Anchor = "LegRedemptionDate")]
        public EFS_Date redeem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redeemSpecified;
        #endregion EFS_Date Redeem
        #region EFS_Decimal Strk
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Strk
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "612", Anchor = "LegStrikePrice")]
        public EFS_Decimal strk;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkSpecified;
        #endregion EFS_Decimal Strk
        #region string StrkCcy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Currency")]
        [DictionaryGUI(Tag = "942", Anchor = "LegStrikeCurrency")]
        public string StrkCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StrkCcySpecified;
        #endregion string StrkCcy
        #region EFS_String OptA
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptA
        {
            set { optA = new EFS_String(value); }
            get { return optA?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OptASpecified
        {
            set { optASpecified = value; }
            get { return optASpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "OptionAttribute", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "613", Anchor = "LegOptAttribute")]
        public EFS_String optA;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optASpecified;
        #endregion EFS_String OptA
        #region EFS_Decimal Cmult
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Cmult
        {
            set { cmult = new EFS_Decimal(value); }
            get { return (null != cmult) ? cmult.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CmultSpecified
        {
            set { cmultSpecified = value; }
            get { return cmultSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Multiplier", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "614", Anchor = "LegContractMultiplier")]
        public EFS_Decimal cmult;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cmultSpecified;
        #endregion EFS_Decimal Cmult
        #region UnitOfMeasureEnum UOM
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unit Of Measure")]
        [DictionaryGUI(Tag = "999", Anchor = "LegUnitOfMeasure")]
        public UnitOfMeasureEnum UOM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool UOMSpecified;
        #endregion UnitOfMeasureEnum UOM
        #region EFS_Decimal UOMQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal UOMQty
        {
            set { uOMQty = new EFS_Decimal(value); }
            get { return (null != uOMQty) ? uOMQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UOMQtySpecified
        {
            set { uOMQtySpecified = value; }
            get { return uOMQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unit Of Measure Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1224", Anchor = "LegUnitOfMeasureQty")]
        public EFS_Decimal uOMQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool uOMQtySpecified;
        #endregion EFS_Decimal UOMQty
        #region UnitOfMeasureEnum PxUOM
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Unit Of Measure")]
        [DictionaryGUI(Tag = "1421", Anchor = "LegPriceUnitOfMeasure")]
        public UnitOfMeasureEnum PxUOM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PxUOMSpecified;
        #endregion UnitOfMeasureEnum PxUOM
        #region EFS_Decimal PxUOMQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal PxUOMQty
        {
            set { pxUOMQty = new EFS_Decimal(value); }
            get { return (null != pxUOMQty) ? pxUOMQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxUOMQtySpecified
        {
            set { pxUOMQtySpecified = value; }
            get { return pxUOMQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Unit Of Measure Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1422", Anchor = "LegPriceUnitOfMeasureQty")]
        public EFS_Decimal pxUOMQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxUOMQtySpecified;
        #endregion EFS_Decimal PxUOMQty
        #region TimeUnitEnum TmUnit
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time Unit")]
        [DictionaryGUI(Tag = "1001", Anchor = "LegTimeUnit")]
        public TimeUnitEnum TmUnit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TmUnitSpecified;
        #endregion TimeUnitEnum TmUnit
        #region DerivativeExerciseStyleEnum ExerStyle
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Style")]
        [DictionaryGUI(Tag = "1420", Anchor = "LegExerciseStyle")]
        public DerivativeExerciseStyleEnum ExerStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExerStyleSpecified;
        #endregion DerivativeExerciseStyleEnum ExerStyle
        #region EFS_Decimal CpnRt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CpnRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CouponRate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "615", Anchor = "LegCouponRate")]
        public EFS_Decimal cpnRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnRtSpecified;
        #endregion EFS_Decimal CpnRt
        #region string Exch
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Exchange")]
        [DictionaryGUI(Tag = "616", Anchor = "LegSecurityExchange")]
        public string Exch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExchSpecified;
        #endregion string Exch
        #region EFS_String Issr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Issr
        {
            set { issr = new EFS_String(value); }
            get { return issr?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssrSpecified
        {
            set { issrSpecified = value; }
            get { return issrSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "617", Anchor = "LegIssuer")]
        public EFS_String issr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issrSpecified;
        #endregion EFS_String Issr
        #region EFS_PosInteger EncLegIssrLen
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncLegIssrLen
        {
            set { encLegIssrLen = new EFS_PosInteger(value); }
            get { return encLegIssrLen?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncLegIssrLenSpecified
        {
            set { encLegIssrLenSpecified = value; }
            get { return encLegIssrLenSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Issuer Length", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "618", Anchor = "EncodedLegIssuerLen")]
        public EFS_PosInteger encLegIssrLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encLegIssrLenSpecified;
        #endregion EFS_PosInteger EncLegIssrLen
        #region EFS_String EncLegIssr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncLegIssr
        {
            set { encLegIssr = new EFS_String(value); }
            get { return encLegIssr?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncLegIssrSpecified
        {
            set { encLegIssrSpecified = value; }
            get { return encLegIssrSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Issuer", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "619", Anchor = "EncodedLegIssuer")]
        public EFS_String encLegIssr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encLegIssrSpecified;
        #endregion EFS_String EncLegIssr
        #region EFS_String Desc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Desc
        {
            set { desc = new EFS_String(value); }
            get { return desc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DescSpecified
        {
            set { descSpecified = value; }
            get { return descSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Description", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "620", Anchor = "LegSecurityDesc")]
        public EFS_String desc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descSpecified;
        #endregion EFS_String Desc
        #region EFS_PosInteger EncLegSecDescLen
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncLegSecDescLen
        {
            set { encLegSecDescLen = new EFS_PosInteger(value); }
            get { return encLegSecDescLen?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncLegSecDescLenSpecified
        {
            set { encLegSecDescLenSpecified = value; }
            get { return encLegSecDescLenSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Security Description Length", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "621", Anchor = "EncodedLegSecurityDescLen")]
        public EFS_PosInteger encLegSecDescLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encLegSecDescLenSpecified;
        #endregion EFS_PosInteger EncLegSecDescLen
        #region EFS_String EncLegSecDesc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncLegSecDesc
        {
            set { encLegSecDesc = new EFS_String(value); }
            get { return encLegSecDesc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncLegSecDescSpecified
        {
            set { encLegSecDescSpecified = value; }
            get { return encLegSecDescSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Security Description", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "622", Anchor = "EncodedLegSecurityDesc")]
        public EFS_String encLegSecDesc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encLegSecDescSpecified;
        #endregion EFS_String EncLegSecDesc
        #region EFS_Decimal RatioQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal RatioQty
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ratio Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "623", Anchor = "LegRatioQty")]
        public EFS_Decimal ratioQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ratioQtySpecified;
        #endregion EFS_Decimal RatioQty
        #region SideEnum Side
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side")]
        [DictionaryGUI(Tag = "624", Anchor = "LegSide")]
        public SideEnum Side;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SideSpecified;
        #endregion SideEnum Side
        #region string Ccy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [DictionaryGUI(Tag = "942", Anchor = "LegCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        #endregion string Ccy
        #region EFS_String Pool
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Pool
        {
            set { pool = new EFS_String(value); }
            get { return pool?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PoolSpecified
        {
            set { poolSpecified = value; }
            get { return poolSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Pool", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "740", Anchor = "LegPool")]
        public EFS_String pool;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool poolSpecified;
        #endregion EFS_String Pool
        #region EFS_Date Dated
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dated Date")]
        [DictionaryGUI(Tag = "739", Anchor = "LegDatedDate")]
        public EFS_Date dated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool datedSpecified;
        #endregion EFS_Date Dated
        #region EFS_String CSetMo
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CSetMo
        {
            set { cSetMo = new EFS_String(value); }
            get { return cSetMo?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CSetMoSpecified
        {
            set { cSetMoSpecified = value; }
            get { return cSetMoSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Settlement Month", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "955", Anchor = "LegContractSettlMonth")]
        public EFS_String cSetMo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cSetMoSpecified;
        #endregion EFS_String CSetMo
        #region EFS_Date IntAcrl
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Accrual Date")]
        [DictionaryGUI(Tag = "956", Anchor = "LegInterestAccrualDate")]
        public EFS_Date intAcrl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intAcrlSpecified;
        #endregion EFS_Date IntAcrl
        #region PutOrCallEnum PutCall
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Put Or Call")]
        [DictionaryGUI(Tag = "1358", Anchor = "LegPutOrCall")]
        public PutOrCallEnum PutCall;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PutCallSpecified;
        #endregion PutOrCallEnum PutCall
        #region EFS_Decimal LegOptionRatio
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LegOptionRatio
        {
            set { legOptionRatio = new EFS_Decimal(value); }
            get { return (null != legOptionRatio) ? legOptionRatio.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LegOptionRatioSpecified
        {
            set { legOptionRatioSpecified = value; }
            get { return legOptionRatioSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option Ratio", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1017", Anchor = "LegOptionRatio")]
        public EFS_Decimal legOptionRatio;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legOptionRatioSpecified;
        #endregion EFS_Decimal LegOptionRatio
        #region EFS_Decimal Px
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Px
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "566", Anchor = "LegPrice")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        #endregion EFS_Decimal Px
        #endregion Members
    }
    #endregion InstrumentLeg_Block
    #region InstrmtLegGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class InstrmtLegGrp_Block
    {
        #region InstrumentLeg_Block Leg
        [System.Xml.Serialization.XmlElementAttribute("Leg", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Leg")]
        public InstrumentLeg_Block Leg;
        #endregion InstrumentLeg_Block Leg
    }
    #endregion InstrmtLegGrp_Block
    #region LegSecAltIDGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class LegSecAltIDGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String SecAltID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SecAltID
        {
            set { secAltID = new EFS_String(value); }
            get { return secAltID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SecAltIDSpecified
        {
            set { secAltIDSpecified = value; }
            get { return secAltIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id", Width = 200)]
        [DictionaryGUI(Tag = "605", Anchor = "LegSecurityAltID")]
        public EFS_String secAltID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secAltIDSpecified;
        #endregion EFS_String SecAltID
        #region SecurityIDSourceEnum SecAltIDSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "606", Anchor = "LegSecurityAltIDSource")]
        public SecurityIDSourceEnum SecAltIDSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecAltIDSrcSpecified;
        #endregion SecurityIDSourceEnum SecAltIDSrc
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion LegSecAltIDGrp_Block
    #region LegStipulations_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class LegStipulations_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region StipulationTypeEnum StipTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stipulation Type")]
        [DictionaryGUI(Tag = "688", Anchor = "LegStipulationType")]
        public StipulationTypeEnum StipTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StipTypSpecified;
        #endregion StipulationTypeEnum StipTyp
        #region EFS_String StipVal
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StipVal
        {
            set { stipVal = new EFS_String(value); }
            get { return stipVal?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StipValSpecified
        {
            set { stipValSpecified = value; }
            get { return stipValSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stipulation Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "689", Anchor = "LegStipulationValue")]
        public EFS_String stipVal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stipValSpecified;
        #endregion EFS_String StipVal
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion LegStipulations_Block
    #region MiscFeesGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class MiscFeesGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_Decimal Amt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Amt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "137", Anchor = "MiscFeeAmt")]
        public EFS_Decimal amt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amtSpecified;
        #endregion EFS_Decimal Amt
        #region string Curr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [DictionaryGUI(Tag = "138", Anchor = "MiscFeeCurr")]
        public string Curr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CurrSpecified;
        #endregion string Curr
        #region MiscFeeTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [DictionaryGUI(Tag = "139", Anchor = "MiscFeeType")]
        public MiscFeeTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion MiscFeeTypeEnum Typ
        #region MiscFeeBasisEnum Basis
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basis")]
        [DictionaryGUI(Tag = "891", Anchor = "MiscFeeBasis")]
        public MiscFeeBasisEnum Basis;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool BasisSpecified;
        #endregion MiscFeeBasisEnum Basis
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion MiscFeesGrp_Block
    #region NestedParties_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class NestedParties_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region FixPartyReference ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set {
                id = new FixPartyReference
                {
                    href = value
                };
            }
            get { return id?.href; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Party ID", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [DictionaryGUI(Tag = "524", Anchor = "NestedPartyID")]
        public FixPartyReference id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion FixPartyReference ID
        #region PartyIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [DictionaryGUI(Tag = "525", Anchor = "NestedPartyIDSource")]
        public PartyIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion PartyIDSourceEnum Src
        #region PartyRoleEnum R
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Role")]
        [DictionaryGUI(Tag = "538", Anchor = "NestedPartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RSpecified;
        #endregion PartyRoleEnum R
        #region PtysSubGrp_Block[] Sub
        [System.Xml.Serialization.XmlElementAttribute("Sub", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Id", MinItem = 0)]
        public PtysSubGrp_Block[] Sub;
        #endregion PtysSubGrp_Block[] Sub
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion NestedParties_Block
    #region NestedParties2_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class NestedParties2_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_Id ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_Id(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "757", Anchor = "Nested2PartyID")]
        public EFS_Id id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_Id ID
        #region PartyIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [DictionaryGUI(Tag = "758", Anchor = "Nested2PartyIDSource")]
        public PartyIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion PartyIDSourceEnum Src
        #region PartyRoleEnum R
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Role")]
        [DictionaryGUI(Tag = "759", Anchor = "Nested2PartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RSpecified;
        #endregion PartyRoleEnum R
        #region NstdPtys2SubGrp_Block[] Sub
        [System.Xml.Serialization.XmlElementAttribute("Sub", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Id", MinItem = 0)]
        public NstdPtys2SubGrp_Block[] Sub;
        #endregion NstdPtys2SubGrp_Block[] Sub
        #endregion Members
 
        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion NestedParties2_Block
    #region NstdPtys2SubGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class NstdPtys2SubGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "760", Anchor = "Nested2PartySubID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_String ID
        #region PartySubIDTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Sub ID Type")]
        [DictionaryGUI(Tag = "807", Anchor = "Nested2PartySubIDType")]
        public PartySubIDTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion PartySubIDTypeEnum Typ
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion NstdPtys2SubGrp_Block
    #region NstdPtysSubGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class NstdPtysSubGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "545", Anchor = "NestedPartySubID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_String ID
        #region PartySubIDTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Sub ID Type")]
        [DictionaryGUI(Tag = "805", Anchor = "NestedPartySubIDType")]
        public PartySubIDTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion PartySubIDTypeEnum Typ
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion NstdPtysSubGrp_Block
    #region OrderQtyData_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class OrderQtyData_Block : ItemGUI
    {
        #region EFS_Decimal Qty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Qty
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "38", Anchor = "OrderQty")]
        public EFS_Decimal qty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qtySpecified;
        #endregion EFS_Decimal Qty
        #region EFS_Decimal Cash
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Cash
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Order Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "152", Anchor = "CashOrderQty")]
        public EFS_Decimal cash;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSpecified;
        #endregion EFS_Decimal Cash
        #region EFS_Decimal Pct
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Pct
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Percent", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "516", Anchor = "OrderPercent")]
        public EFS_Decimal pct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pctSpecified;
        #endregion EFS_Decimal Pct
        #region RoundingDirectionEnum RndDir
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rounding Direction")]
        [DictionaryGUI(Tag = "468", Anchor = "RoundingDirection")]
        public DerivativeRoundingDirectionEnum RndDir;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RndDirSpecified;
        #endregion RoundingDirectionEnum RndDir
        #region EFS_Decimal RndMod
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal RndMod
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rounding Modulus", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "469", Anchor = "RoundingModulus")]
        public EFS_Decimal rndMod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rndModSpecified;
        #endregion EFS_Decimal RndMod
    }
    #endregion OrderQtyData_Block
    #region Parties_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class Parties_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region FixPartyReference ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set {
                id = new FixPartyReference
                {
                    href = value
                };
            }
            get { return id?.href; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Party ID", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [DictionaryGUI(Tag = "448", Anchor = "PartyID")]
        public FixPartyReference id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion FixPartyReference ID
        #region PartyIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [DictionaryGUI(Tag = "447", Anchor = "PartyIDSource")]
        public PartyIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion PartyIDSourceEnum Src
        #region PartyRoleEnum R
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Role")]
        [DictionaryGUI(Tag = "452", Anchor = "PartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RSpecified;
        #endregion PartyRoleEnum R
        #region PtysSubGrp_Block[] Sub
        [System.Xml.Serialization.XmlElementAttribute("Sub", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Id", MinItem = 0)]
        public PtysSubGrp_Block[] Sub;
        #endregion PtysSubGrp_Block[] Sub
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion Parties_Block
    #region PositionAmountData_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class PositionAmountData_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region PosAmtTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Position Amount Type")]
        [DictionaryGUI(Tag = "707", Anchor = "PosAmtType")]
        public PosAmtTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion PosAmtTypeEnum Typ
        #region EFS_Decimal Amt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Amt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Position Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "708", Anchor = "PosAmt")]
        public EFS_Decimal amt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amtSpecified;
        #endregion EFS_Decimal Amt
        #region string Ccy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Position Currency")]
        [DictionaryGUI(Tag = "1055", Anchor = "PositionCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        #endregion string Ccy
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion PositionAmountData_Block
    #region PtysSubGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class PtysSubGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "523", Anchor = "PartySubID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_String ID
        #region PartySubIDTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Sub ID Type")]
        [DictionaryGUI(Tag = "803", Anchor = "PartySubIDType")]
        public PartySubIDTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion PartySubIDTypeEnum Typ
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion PtysSubGrp_Block
    #region RelatedPositionGrp_Block
    // PM 20160428 [22107] Ajout à partir de l'Extension Pack: FIX.5.0SP2 EP142
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class RelatedPositionGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set {
                id = new EFS_String
                {
                    Value = value
                };
            }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Related Position ID", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [DictionaryGUI(Tag = "1862", Anchor = "RelatedPositionID")]
        public EFS_String id;
        #endregion ID

        #region RelatedPositionIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Position ID Source")]
        [DictionaryGUI(Tag = "1863", Anchor = "RelatedPositionIDSource")]
        public RelatedPositionIDSourceEnum Src;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion RelatedPositionIDSourceEnum Src

        #region Dt
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Position Date")]
        [DictionaryGUI(Tag = "1864", Anchor = "RelatedPositionDate")]
        public System.DateTime Dt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DtSpecified;
        #endregion Dt
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion RelatedPositionGrp_Block

    /// <summary>
    /// Collection d'identifiants de transaction pour répondre aux éxigences règlementaires
    /// en matière de déclaration des transactions
    /// 
    /// Ajout  partir de l'Extension Pack: FIX.5.0SP2 EP275
    /// </summary>
    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class RegulatoryTradeIDGrp_Block : ItemGUI, IEFS_Array
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set
            {
                id = new EFS_String
                {
                    Value = value
                };
            }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Regulatory Trade ID", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [DictionaryGUI(Tag = "1903", Anchor = "RegulatoryTradeID")]
        public EFS_String id;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Regulatory Trade ID Source")]
        [DictionaryGUI(Tag = "1905", Anchor = "RegulatoryTradeIDSource")]
        public RegulatoryTradeIDSourceEnum Src;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Regulatory Trade ID Event")]
        [DictionaryGUI(Tag = "1904", Anchor = "RegulatoryTradeIDEvent")]
        public RegulatoryTradeIDEventEnum Evnt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool EvntSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Regulatory Trade ID Type")]
        [DictionaryGUI(Tag = "1906", Anchor = "RegulatoryTradeIDType")]
        public RegulatoryTradeIDTypeEnum Typ;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LegRefID
        {
            set
            {
                legRefId = new EFS_String
                {
                    Value = value
                };
            }
            get { return legRefId?.Value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LegRefIDSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Regulatory Trade ID Type")]
        [DictionaryGUI(Tag = "2411", Anchor = "RegulatoryTradeIDType")]
        public EFS_String legRefId;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Regulatory Trade ID Type")]
        [DictionaryGUI(Tag = "2397", Anchor = "RegulatoryTradeIDType")]
        public RegulatoryTradeIDScopeEnum Scope;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ScopeSpecified;

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }

    #region RootParties_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class RootParties_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region FixPartyReference ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set {
                id = new FixPartyReference
                {
                    href = value
                };
            }
            get { return id?.href; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Root Party ID", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [DictionaryGUI(Tag = "1117", Anchor = "RootPartyID")]
        public FixPartyReference id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion FixPartyReference ID
        #region PartyIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [DictionaryGUI(Tag = "1118", Anchor = "RootPartyIDSource")]
        public PartyIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion PartyIDSourceEnum Src
        #region PartyRoleEnum R
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Role")]
        [DictionaryGUI(Tag = "1119", Anchor = "RootPartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RSpecified;
        #endregion PartyRoleEnum R
        #region RootSubParties_Block[] Sub
        [System.Xml.Serialization.XmlElementAttribute("Sub", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Id", MinItem = 0)]
        public RootSubParties_Block[] Sub;
        #endregion RootSubParties_Block[] Sub
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion RootParties_Block
    #region RootSubParties_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class RootSubParties_Block : ItemGUI, IEFS_Array
    {
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Root Party Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1121", Anchor = "RootPartySubID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_String ID
        #region PartySubIDTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Root Party Sub ID Type")]
        [DictionaryGUI(Tag = "1122", Anchor = "RootPartySubIDType")]
        public PartySubIDTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion PartySubIDTypeEnum Typ

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion RootSubParties_Block
    #region SecAltIDGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class SecAltIDGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String AltID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AltID
        {
            set { altID = new EFS_String(value); }
            get { return altID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AltIDSpecified
        {
            set { altIDSpecified = value; }
            get { return altIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id", Width = 200)]
        [DictionaryGUI(Tag = "455", Anchor = "SecurityAltID")]
        public EFS_String altID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool altIDSpecified;
        #endregion EFS_String AltID
        #region SecurityIDSourceEnum AltIDSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "456", Anchor = "SecurityAltIDSource")]
        public SecurityIDSourceEnum AltIDSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AltIDSrcSpecified;
        #endregion SecurityIDSourceEnum AltIDSrc
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion SecAltIDGrp_Block
    #region SecurityXML_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class SecurityXML_Block : ItemGUI
    {
        #region XmlElement[] Any
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Security XML")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Security XML", IsChild = true, MinItem = 0)]
        public System.Xml.XmlElement[] Any;
        #endregion XmlElement[] Any
        #region EFS_String Schema
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Schema
        {
            set { schema = new EFS_String(value); }
            get { return schema?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SchemaSpecified
        {
            set { schemaSpecified = value; }
            get { return schemaSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schema", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1186", Anchor = "SecurityXMLSchema")]
        public EFS_String schema;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool schemaSpecified;
        #endregion EFS_String Schema
    }
    #endregion SecurityXML_Block
    #region SettlDetails_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class SettlDetails_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region SettlObligSourceEnum SettlSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Instructions Source")]
        [DictionaryGUI(Tag = "1164", Anchor = "SettlObligSource")]
        public SettlObligSourceEnum SettlSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlSrcSpecified;
        #endregion SettlObligSourceEnum SettlSrc
        #region SettlParties_Block[] Pty
        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Party", IsChild = true, MinItem = 0)]
        public SettlParties_Block[] Pty;
        #endregion SettlParties_Block[] Pty
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion SettlDetails_Block
    #region SettlParties_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class SettlParties_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region FixPartyReference ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set {
                id = new FixPartyReference
                {
                    href = value
                };
            }
            get { return id?.href; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Party ID", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [DictionaryGUI(Tag = "782", Anchor = "SettlPartyID")]
        public FixPartyReference id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion FixPartyReference ID
        #region PartyIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [DictionaryGUI(Tag = "783", Anchor = "SettlPartyIDSource")]
        public PartyIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion PartyIDSourceEnum Src
        #region PartyRoleEnum R
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Role")]
        [DictionaryGUI(Tag = "784", Anchor = "SettlPartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RSpecified;
        #endregion PartyRoleEnum R
        #region SettlPtysSubGrp_Block[] Sub
        [System.Xml.Serialization.XmlElementAttribute("Sub", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Id", MinItem = 0)]
        public SettlPtysSubGrp_Block[] Sub;
        #endregion SettlPtysSubGrp_Block[] Sub
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion SettlParties_Block
    #region SettlPtysSubGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class SettlPtysSubGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "785", Anchor = "SettlPartySubID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_String ID
        #region PartySubIDTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Sub ID Type")]
        [DictionaryGUI(Tag = "786", Anchor = "SettlPartySubIDType")]
        public PartySubIDTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion PartySubIDTypeEnum Typ
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion SettlPtysSubGrp_Block
    #region SideTrdRegTS_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class SideTrdRegTS_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_DateTime TS
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime TS
        {
            set { ts = new EFS_DateTime(value.ToString(Cst.FixML_DateTimeFmt)); }
            get { return (null != ts) ? ts.DateTimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TSSpecified
        {
            set { tsSpecified = value; }
            get { return tsSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Timestamp")]
        [DictionaryGUI(Tag = "1012", Anchor = "SideTrdRegTimestamp")]
        public EFS_DateTime ts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tsSpecified;
        #endregion EFS_DateTime TS
        #region EFS_Integer Typ
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string Typ
        {
            set { typ = new EFS_Integer(value); }
            get { return typ?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypSpecified
        {
            set { typSpecified = value; }
            get { return typSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1013", Anchor = "SideTrgRegTimestampType")]
        public EFS_Integer typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typSpecified;
        #endregion EFS_Integer Typ
        #region EFS_String Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Src
        {
            set { src = new EFS_String(value); }
            get { return src?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SrcSpecified
        {
            set { srcSpecified = value; }
            get { return srcSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1014", Anchor = "SideTrgRegTimestampSrc")]
        public EFS_String src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool srcSpecified;
        #endregion EFS_String Src
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion SideTrdRegTS_Block
    #region SpreadOrBenchmarkCurveData_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class SpreadOrBenchmarkCurveData_Block : ItemGUI
    {
        #region EFS_Decimal Spread
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Spread
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "218", Anchor = "Spread")]
        public EFS_Decimal spread;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadSpecified;
        #endregion EFS_Decimal Spread
        #region string Ccy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark Curve Currency")]
        [DictionaryGUI(Tag = "220", Anchor = "BenchmarkCurveCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        #endregion string Ccy
        #region BenchmarkCurveNameEnum Name
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark Curve Name")]
        [DictionaryGUI(Tag = "221", Anchor = "BenchmarkCurveName")]
        public BenchmarkCurveNameEnum Name;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool NameSpecified;
        #endregion BenchmarkCurveNameEnum Name
        #region EFS_String Point
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Point
        {
            set { point = new EFS_String(value); }
            get { return point?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PointSpecified
        {
            set { pointSpecified = value; }
            get { return pointSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark Curve Point", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "222", Anchor = "BenchmarkCurvePoint")]
        public EFS_String point;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pointSpecified;
        #endregion EFS_String Point
        #region EFS_Decimal Px
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Px
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "662", Anchor = "BenchmarkPrice")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        #endregion EFS_Decimal Px
        #region PriceTypeEnum PxTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark Price Type")]
        [DictionaryGUI(Tag = "663", Anchor = "BenchmarkPriceType")]
        public PriceTypeEnum PxTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PxTypSpecified;
        #endregion PriceTypeEnum PxTyp
        #region EFS_String SecID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SecID
        {
            set { secID = new EFS_String(value); }
            get { return secID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SecIDSpecified
        {
            set { secIDSpecified = value; }
            get { return secIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark Security ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "669", Anchor = "BenchmarkSecurityID")]
        public EFS_String secID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secIDSpecified;
        #endregion EFS_String SecID
        #region SecurityIDSourceEnum SecIDSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark Security ID Source")]
        [DictionaryGUI(Tag = "761", Anchor = "BenchmarkSecurityIDSource")]
        public SecurityIDSourceEnum SecIDSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecIDSrcSpecified;
        #endregion SecurityIDSourceEnum SecIDSrc
    }
    #endregion SpreadOrBenchmarkCurveData_Block
    #region Stipulations_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class Stipulations_Block : ItemGUI
    {
        #region Members
        #region StipulationTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [DictionaryGUI(Tag = "233", Anchor = "StipulationType")]
        public StipulationTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion StipulationTypeEnum Typ
        #region EFS_String Val
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Val
        {
            set { val = new EFS_String(value); }
            get { return val?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ValSpecified
        {
            set { valSpecified = value; }
            get { return valSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "234", Anchor = "StipulationValue")]
        public EFS_String val;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valSpecified;
        #endregion EFS_String Val
        #endregion Members
    }
    #endregion Stipulations_Block
    #region TradeCapLegUnderlyingsGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class TradeCapLegUnderlyingsGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region UnderlyingLegInstrument_Block Instrmt
        [System.Xml.Serialization.XmlElementAttribute("Instrmt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying Leg Instrument")]
        public UnderlyingLegInstrument_Block Instrmt;
        #endregion UnderlyingLegInstrument_Block Instrmt
        #endregion Members

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
    #endregion TradeCapLegUnderlyingsGrp_Block
    #region TrdAllocGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class TrdAllocGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String Acct
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct
        {
            set { acct = new EFS_String(value); }
            get { return acct?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AcctSpecified
        {
            set { acctSpecified = value; }
            get { return acctSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "79", Anchor = "AllocAccount")]
        public EFS_String acct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool acctSpecified;
        #endregion EFS_String Acct
        #region AllocAcctIDSourceEnum ActIDSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account Source")]
        [DictionaryGUI(Tag = "661", Anchor = "AllocAcctIDSrc")]
        public AllocAcctIDSourceEnum ActIDSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ActIDSrcSpecified;
        #endregion AllocAcctIDSourceEnum ActIDSrc
        #region string AllocSettlCcy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Currency")]
        [DictionaryGUI(Tag = "736", Anchor = "AllocSettlCurrency")]
        public string AllocSettlCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AllocSettlCcySpecified;
        #endregion string AllocSettlCcy
        #region EFS_String IndAllocID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IndAllocID
        {
            set { indAllocID = new EFS_String(value); }
            get { return indAllocID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IndAllocIDSpecified
        {
            set { indAllocIDSpecified = value; }
            get { return indAllocIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Individual Allocation ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "467", Anchor = "IndividualAllocID")]
        public EFS_String indAllocID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indAllocIDSpecified;
        #endregion EFS_String IndAllocID
        #region NestedParties2_Block[] Pty
        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Nested Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Nested Party", IsChild = true, MinItem = 0)]
        public NestedParties2_Block[] Pty;
        #endregion NestedParties2_Block[] Pty
        #region EFS_Decimal Qty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Qty
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "80", Anchor = "AllocQty")]
        public EFS_Decimal qty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qtySpecified;
        #endregion EFS_Decimal Qty
        #region EFS_String CustCpcty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CustCpcty
        {
            set { custCpcty = new EFS_String(value); }
            get { return custCpcty?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CustCpctySpecified
        {
            set { custCpctySpecified = value; }
            get { return custCpctySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Allocation Customer Capacity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "993", Anchor = "AllocCustomerCapacity")]
        public EFS_String custCpcty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool custCpctySpecified;
        #endregion EFS_String CustCpcty
        #region AllocMethodEnum Meth
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Allocation Method")]
        [DictionaryGUI(Tag = "1002", Anchor = "AllocMethod")]
        public AllocMethodEnum Meth;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MethSpecified;
        #endregion AllocMethodEnum Meth
        #region EFS_String IndAllocID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IndAllocID2
        {
            set { indAllocID2 = new EFS_String(value); }
            get { return indAllocID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IndAllocID2Specified
        {
            set { indAllocID2Specified = value; }
            get { return indAllocID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Individual Allocation ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "989", Anchor = "SecondaryIndividualAllocID")]
        public EFS_String indAllocID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indAllocID2Specified;
        #endregion EFS_String IndAllocID2
        #region ClearingFeeIndicatorEnum ClrFeeInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clearing Fee Indicator")]
        [DictionaryGUI(Tag = "1136", Anchor = "AllocClearingFeeIndicator")]
        public ClearingFeeIndicatorEnum ClrFeeInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ClrFeeIndSpecified;
        #endregion ClearingFeeIndicatorEnum ClrFeeInd
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion TrdAllocGrp_Block
    #region TrdCapDtGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class TrdCapDtGrp_Block
    {

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime TrdDt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdDtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime LastUpdateTm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastUpdateTmSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TxnTm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TxnTmSpecified;
    }
    #endregion TrdCapDtGrp_Block
    #region TrdCapRptAckSideGrp_Block
    [System.SerializableAttribute()]
//    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class TrdCapRptAckSideGrp_Block
    {

        [System.Xml.Serialization.XmlElementAttribute("Pty")]
        public Parties_Block[] Pty;

        [System.Xml.Serialization.XmlElementAttribute("ClrInst")]
        public ClrInstGrp_Block[] ClrInst;

        public CommissionData_Block Comm;

        [System.Xml.Serialization.XmlElementAttribute("ContAmt")]
        public ContAmtGrp_Block[] ContAmt;

        [System.Xml.Serialization.XmlElementAttribute("Stip")]
        public Stipulations_Block[] Stip;

        [System.Xml.Serialization.XmlElementAttribute("MiscFees")]
        public MiscFeesGrp_Block[] MiscFees;

        [System.Xml.Serialization.XmlElementAttribute("SettlDetails")]
        public SettlDetails_Block[] SettlDetails;

        [System.Xml.Serialization.XmlElementAttribute("Alloc")]
        public TrdAllocGrp_Block[] Alloc;

        [System.Xml.Serialization.XmlElementAttribute("TrdRegTS")]
        public SideTrdRegTS_Block[] TrdRegTS;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SideEnum Side;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClOrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClOrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ListID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AcctIDSrc;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AccountTypeEnum AcctTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AcctTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ProcessCodeEnum ProcCode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ProcCodeSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public OddLotEnum OddLot;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OddLotSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public LotTypeEnum LotTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LotTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InptSrc;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InptDev;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdInptDev;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ComplianceID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SolicitedFlagEnum SolFlag;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SolFlagSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public OrderCapacityEnum Cpcty;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CpctySpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public OrderRestrictionsEnum Rstctions;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RstctionsSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public CustOrderCapacityEnum CustCpcty;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CustCpctySpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public OrdTypeEnum OrdTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrdTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ExecInstEnum ExecInst;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecInstSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TransBkdTm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TransBkdTmSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesSub;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TmBkt;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public NetGrossIndEnum NetGrossInd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NetGrossIndSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Ccy;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SettlCcy;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string NumDaysInt;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime ExDt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExDtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal AcrdIntRt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AcrdIntRtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal AcrdIntAmt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AcrdIntAmtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal IntAtMat;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IntAtMatSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal EndAcrdIntAmt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EndAcrdIntAmtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal StartCsh;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartCshSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal EndCsh;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EndCshSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Concession;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConcessionSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal TotTakedown;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotTakedownSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal NetMny;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NetMnySpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal SettlCurrAmt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlCurrAmtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal SettlCurrFxRt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlCurrFxRtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SettlCurrFxRateCalcEnum SettlCurrFxRtCalc;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlCurrFxRtCalcSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PositionEffectEnum PosEfct;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PosEfctSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SideMultiLegReportingTypeEnum MLegRptTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MLegRptTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExchRule;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeAllocIndicatorEnum AllocInd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllocIndSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PreallocMethodEnum PreallocMeth;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PreallocMethSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AllocID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal SideGrossTradeAmt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SideGrossTradeAmtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AggressorIndicatorEnum AgrsrInd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AgrsrIndSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string SideQty;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FillStationCd;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RsnCD;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string RptSeq;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SideTrdSubTypEnum TrdSubTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdSubTypSpecified;
    }
    #endregion TrdCapRptAckSideGrp_Block
    #region TrdCapRptSideGrp_Block
    [System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class TrdCapRptSideGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region SideEnum Side
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side")]
        [DictionaryGUI(Tag = "54", Anchor = "Side")]
        public SideEnum Side;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SideSpecified;
        #endregion SideEnum Side
        #region EFS_String OrdID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID
        {
            set { ordID = new EFS_String(value); }
            get { return ordID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrdIDSpecified
        {
            set { ordIDSpecified = value; }
            get { return ordIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "37", Anchor = "OrderID")]
        public EFS_String ordID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ordIDSpecified;
        #endregion EFS_String OrdID
        #region EFS_String OrdID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID2
        {
            set { ordID2 = new EFS_String(value); }
            get { return ordID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrdID2Specified
        {
            set { ordID2Specified = value; }
            get { return ordID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Order ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "198", Anchor = "SecondaryOrderID")]
        public EFS_String ordID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ordID2Specified;
        #endregion EFS_String OrdID2
        #region EFS_String ClOrdID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClOrdID
        {
            set { clOrdID = new EFS_String(value); }
            get { return clOrdID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ClOrdIDSpecified
        {
            set { clOrdIDSpecified = value; }
            get { return clOrdIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unique Order ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "11", Anchor = "ClOrdID")]
        public EFS_String clOrdID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool clOrdIDSpecified;
        #endregion EFS_String ClOrdID
        #region EFS_String ExecRefID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecRefID
        {
            set { execRefID = new EFS_String(value); }
            get { return execRefID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecRefIDSpecified
        {
            set { execRefIDSpecified = value; }
            get { return execRefIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution Reference ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "19", Anchor = "ExecRefID")]
        public EFS_String execRefID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool execRefIDSpecified;
        #endregion EFS_String ExecRefID
        #region EFS_String ClOrdID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClOrdID2
        {
            set { clOrdID2 = new EFS_String(value); }
            get { return clOrdID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ClOrdID2Specified
        {
            set { clOrdID2Specified = value; }
            get { return clOrdID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Client Order ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "526", Anchor = "SecondaryClOrdID")]
        public EFS_String clOrdID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool clOrdID2Specified;
        #endregion EFS_String ClOrdID2
        #region EFS_String ListID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ListID
        {
            set { listID = new EFS_String(value); }
            get { return listID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ListIDSpecified
        {
            set { listIDSpecified = value; }
            get { return listIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "List ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "66", Anchor = "ListID")]
        public EFS_String listID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool listIDSpecified;
        #endregion EFS_String ListID
        #region EFS_Integer SideQty
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string SideQty
        {
            set { sideQty = new EFS_Integer(value); }
            get { return sideQty?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SideQtySpecified
        {
            set { sideQtySpecified = value; }
            get { return sideQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1009", Anchor = "SideQty")]
        public EFS_Integer sideQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sideQtySpecified;
        #endregion EFS_Integer SideQty
        #region EFS_String RptID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID
        {
            set { rptID = new EFS_String(value); }
            get { return rptID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RptIDSpecified
        {
            set { rptIDSpecified = value; }
            get { return rptIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Trade Report ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1005", Anchor = "SideTradeReportID")]
        public EFS_String rptID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rptIDSpecified;
        #endregion EFS_String RptID
        #region EFS_String FillStationCd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FillStationCd
        {
            set { fillStationCd = new EFS_String(value); }
            get { return fillStationCd?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillStationCdSpecified
        {
            set { fillStationCdSpecified = value; }
            get { return fillStationCdSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Fill Station Code", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1006", Anchor = "SideFillStationCd")]
        public EFS_String fillStationCd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fillStationCdSpecified;
        #endregion EFS_String FillStationCd
        #region EFS_String RsnCD
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RsnCD
        {
            set { rsnCD = new EFS_String(value); }
            get { return rsnCD?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RsnCDSpecified
        {
            set { rsnCDSpecified = value; }
            get { return rsnCDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multi-Sided Trade Report Reason", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1007", Anchor = "SideReasonCd")]
        public EFS_String rsnCD;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rsnCDSpecified;
        #endregion EFS_String RsnCD
        #region EFS_Integer RptSeq
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string RptSeq
        {
            set { rptSeq = new EFS_Integer(value); }
            get { return rptSeq?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RptSeqSpecified
        {
            set { rptSeqSpecified = value; }
            get { return rptSeqSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Report Sequence", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "83", Anchor = "RptSeq")]
        public EFS_Integer rptSeq;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rptSeqSpecified;
        #endregion EFS_Integer RptSeq
        #region SideTrdSubTypEnum TrdSubTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Trade Sub Type")]
        [DictionaryGUI(Tag = "1008", Anchor = "SideTrdSubTyp")]
        public SideTrdSubTypEnum TrdSubTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdSubTypSpecified;
        #endregion SideTrdSubTypEnum TrdSubTyp
        #region NetGrossIndEnum NetGrossInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net Gross Indicator")]
        [DictionaryGUI(Tag = "430", Anchor = "NetGrossInd")]
        public NetGrossIndEnum NetGrossInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool NetGrossIndSpecified;
        #endregion NetGrossIndEnum NetGrossInd
        #region string Ccy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Currency")]
        [DictionaryGUI(Tag = "1154", Anchor = "SideCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        #endregion string Ccy
        #region string SettlCcy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Settlement Currency")]
        [DictionaryGUI(Tag = "1155", Anchor = "SideSettlCurrency")]
        public string SettlCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlCcySpecified;
        #endregion string SettlCcy
        #region Parties_Block[] Pty
        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Party", IsChild = true, MinItem = 0)]
        public Parties_Block[] Pty;
        #endregion Parties_Block[] Pty
        #region FixPartyReference Acct
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Acct
        {
            set {
                acct = new FixPartyReference
                {
                    href = value
                };
            }
            get { return (acct?.href); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AcctSpecified
        {
            set { acctSpecified = value; }
            get { return acctSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Account", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [DictionaryGUI(Tag = "1", Anchor = "Account")]
        public FixPartyReference acct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool acctSpecified;
        #endregion FixPartyReference Acct
        #region AcctIDSourceEnum AcctIDSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account ID Source")]
        [DictionaryGUI(Tag = "660", Anchor = "AcctIDSource")]
        public AcctIDSourceEnum AcctIDSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AcctIDSrcSpecified;
        #endregion AcctIDSourceEnum AcctIDSrc
        #region AccountTypeEnum AcctTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account Type")]
        [DictionaryGUI(Tag = "581", Anchor = "AccountType")]
        public AccountTypeEnum AcctTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AcctTypSpecified;
        #endregion AccountTypeEnum AcctTyp
        #region ProcessCodeEnum ProcCode
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Processing Code")]
        [DictionaryGUI(Tag = "81", Anchor = "ProcessCode")]
        public ProcessCodeEnum ProcCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ProcCodeSpecified;
        #endregion ProcessCodeEnum ProcCode
        #region LotTypeEnum LotTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Lot Type")]
        [DictionaryGUI(Tag = "1093", Anchor = "LotType")]
        public LotTypeEnum LotTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LotTypSpecified;
        #endregion LotTypeEnum LotTyp
        #region OddLotEnum OddLot
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Odd Lot")]
        [DictionaryGUI(Tag = "575", Anchor = "OddLot")]
        public OddLotEnum OddLot;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OddLotSpecified;
        #endregion OddLotEnum OddLot
        #region ClrInstGrp_Block[] ClrInst
        [System.Xml.Serialization.XmlElementAttribute("ClrInst", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Clearing Instructions")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Clearing Instruction", IsChild = true, MinItem = 0)]
        public ClrInstGrp_Block[] ClrInst;
        #endregion ClrInstGrp_Block[] ClrInst
        #region EFS_String InptSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InptSrc
        {
            set { inptSrc = new EFS_String(value); }
            get { return inptSrc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InptSrcSpecified
        {
            set { inptSrcSpecified = value; }
            get { return inptSrcSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Input Source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "578", Anchor = "TradeInputSource")]
        public EFS_String inptSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool inptSrcSpecified;
        #endregion EFS_String InptSrc
        #region EFS_String InptDev
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InptDev
        {
            set { inptDev = new EFS_String(value); }
            get { return inptDev?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InptDevSpecified
        {
            set { inptDevSpecified = value; }
            get { return inptDevSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Input Device", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "579", Anchor = "TradeInputDevice")]
        public EFS_String inptDev;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool inptDevSpecified;
        #endregion EFS_String InptDev
        #region EFS_String OrdInptDev
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdInptDev
        {
            set { ordInptDev = new EFS_String(value); }
            get { return ordInptDev?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrdInptDevSpecified
        {
            set { ordInptDevSpecified = value; }
            get { return ordInptDevSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Input Device", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "821", Anchor = "OrderInputDevice")]
        public EFS_String ordInptDev;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ordInptDevSpecified;
        #endregion EFS_String OrdInptDev
        #region EFS_String ComplianceID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ComplianceID
        {
            set { complianceID = new EFS_String(value); }
            get { return complianceID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ComplianceIDSpecified
        {
            set { complianceIDSpecified = value; }
            get { return complianceIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Compliance ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "376", Anchor = "ComplianceID")]
        public EFS_String complianceID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool complianceIDSpecified;
        #endregion EFS_String ComplianceID
        #region SolicitedFlagEnum SolFlag
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Solicited Flag")]
        [DictionaryGUI(Tag = "377", Anchor = "SolicitedFlag")]
        public SolicitedFlagEnum SolFlag;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SolFlagSpecified;
        #endregion SolicitedFlagEnum SolFlag
        #region OrderCapacityEnum Cpcty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Capacity")]
        [DictionaryGUI(Tag = "528", Anchor = "OrderCapacity")]
        public OrderCapacityEnum Cpcty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CpctySpecified;
        #endregion OrderCapacityEnum Cpcty
        #region OrderRestrictionsEnum Rstctions
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Restrictions")]
        [DictionaryGUI(Tag = "529", Anchor = "OrderRestrictions")]
        public OrderRestrictionsEnum Rstctions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RstctionsSpecified;
        #endregion OrderRestrictionsEnum Rstctions
        #region CustOrderCapacityEnum CustCpcty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Customer Order Capacity")]
        [DictionaryGUI(Tag = "582", Anchor = "CustOrderCapacity")]
        public CustOrderCapacityEnum CustCpcty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CustCpctySpecified;
        #endregion CustOrderCapacityEnum CustCpcty
        #region OrdTypeEnum OrdTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Type")]
        [DictionaryGUI(Tag = "40", Anchor = "OrdType")]
        public OrdTypeEnum OrdTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OrdTypSpecified;
        #endregion OrdTypeEnum OrdTyp
        #region ExecInstEnum ExecInst
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution Instruction")]
        [DictionaryGUI(Tag = "18", Anchor = "ExecInst")]
        public ExecInstEnum ExecInst;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExecInstSpecified;
        #endregion ExecInstEnum ExecInst
        #region EFS_DateTime TransBkdTm
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime TransBkdTm
        {
            set { transBkdTm = new EFS_DateTime(value.ToString(Cst.FixML_DateTimeFmt)); }
            get { return (null != transBkdTm) ? transBkdTm.DateTimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TransBkdTmSpecified
        {
            set { transBkdTmSpecified = value; }
            get { return transBkdTmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transaction Booked Time")]
        [DictionaryGUI(Tag = "483", Anchor = "TransBkdTime")]
        public EFS_DateTime transBkdTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool transBkdTmSpecified;
        #endregion EFS_DateTime TransBkdTm
        #region EFS_String SesID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesID
        {
            set { sesID = new EFS_String(value); }
            get { return sesID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SesIDSpecified
        {
            set { sesIDSpecified = value; }
            get { return sesIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trading Session ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "336", Anchor = "TradingSessionID")]
        public EFS_String sesID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sesIDSpecified;
        #endregion EFS_String SesID
        #region EFS_String SesSub
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesSub
        {
            set { sesSub = new EFS_String(value); }
            get { return sesSub?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SesSubSpecified
        {
            set { sesSubSpecified = value; }
            get { return sesSubSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trading Session Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "625", Anchor = "TradingSessionSubID")]
        public EFS_String sesSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sesSubSpecified;
        #endregion EFS_String SesSub
        #region EFS_String TmBkt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TmBkt
        {
            set { tmBkt = new EFS_String(value); }
            get { return tmBkt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TmBktSpecified
        {
            set { tmBktSpecified = value; }
            get { return tmBktSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time Bracket", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "943", Anchor = "TimeBracket")]
        public EFS_String tmBkt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tmBktSpecified;
        #endregion EFS_String TmBkt
        #region CommissionData_Block Comm
        [System.Xml.Serialization.XmlElementAttribute("Comm", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Commission")]
        public CommissionData_Block Comm;
        #endregion CommissionData_Block Comm
        #region EFS_Integer NumDaysInt
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string NumDaysInt
        {
            set { numDaysInt = new EFS_Integer(value); }
            get { return numDaysInt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NumDaysIntSpecified
        {
            set { numDaysIntSpecified = value; }
            get { return numDaysIntSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number of Days of Interest", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "157", Anchor = "NumDaysInterest")]
        public EFS_Integer numDaysInt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numDaysIntSpecified;
        #endregion EFS_Integer NumDaysInt
        #region EFS_Date ExDt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ex-Date")]
        [DictionaryGUI(Tag = "230", Anchor = "ExDate")]
        public EFS_Date exDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exDtSpecified;
        #endregion EFS_Date ExDt
        #region EFS_Decimal AcrdIntRt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal AcrdIntRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued Interest Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "158", Anchor = "AccruedInterestRate")]
        public EFS_Decimal acrdIntRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool acrdIntRtSpecified;
        #endregion EFS_Decimal AcrdIntRt
        #region EFS_Decimal AcrdIntAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal AcrdIntAmt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued Interest Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "159", Anchor = "AccruedInterestAmt")]
        public EFS_Decimal acrdIntAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool acrdIntAmtSpecified;
        #endregion EFS_Decimal AcrdIntAmt
        #region EFS_Decimal IntAtMat
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal IntAtMat
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest At Maturity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "738", Anchor = "InterestAtMaturity")]
        public EFS_Decimal intAtMat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intAtMatSpecified;
        #endregion EFS_Decimal IntAtMat
        #region EFS_Decimal EndAcrdIntAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal EndAcrdIntAmt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ending Accrued Interest Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "920", Anchor = "EndAccruedInterestAmt")]
        public EFS_Decimal endAcrdIntAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endAcrdIntAmtSpecified;
        #endregion EFS_Decimal EndAcrdIntAmt
        #region EFS_Decimal StartCsh
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal StartCsh
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Starting Dirty Cash", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "921", Anchor = "StartCash")]
        public EFS_Decimal startCsh;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startCshSpecified;
        #endregion EFS_Decimal StartCsh
        #region EFS_Decimal EndCsh
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal EndCsh
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ending Dirty Cash", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "922", Anchor = "EndCash")]
        public EFS_Decimal endCsh;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endCshSpecified;
        #endregion EFS_Decimal EndCsh
        #region EFS_Decimal Concession
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Concession
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Concession", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "238", Anchor = "Concession")]
        public EFS_Decimal concession;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool concessionSpecified;
        #endregion EFS_Decimal Concession
        #region EFS_Decimal TotTakedown
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal TotTakedown
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total Takedown", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "237", Anchor = "TotalTakedown")]
        public EFS_Decimal totTakedown;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totTakedownSpecified;
        #endregion EFS_Decimal TotTakedown
        #region EFS_Decimal NetMny
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal NetMny
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net Money", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "118", Anchor = "NetMoney")]
        public EFS_Decimal netMny;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool netMnySpecified;
        #endregion EFS_Decimal NetMny
        #region EFS_Decimal SettlCurrAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal SettlCurrAmt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Currency Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "119", Anchor = "SettlCurrAmt")]
        public EFS_Decimal settlCurrAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlCurrAmtSpecified;
        #endregion EFS_Decimal SettlCurrAmt
        #region EFS_Decimal SettlCurrFxRt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal SettlCurrFxRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Currency Fx Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "155", Anchor = "SettlCurrFxRate")]
        public EFS_Decimal settlCurrFxRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlCurrFxRtSpecified;
        #endregion EFS_Decimal SettlCurrFxRt
        #region SettlCurrFxRateCalcEnum SettlCurrFxRtCalc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Currency Fx Rate Calculation")]
        [DictionaryGUI(Tag = "156", Anchor = "SettlCurrFxRateCalc")]
        public SettlCurrFxRateCalcEnum SettlCurrFxRtCalc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlCurrFxRtCalcSpecified;
        #endregion SettlCurrFxRateCalcEnum SettlCurrFxRtCalc
        #region PositionEffectEnum PosEfct
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Position Effect")]
        [DictionaryGUI(Tag = "77", Anchor = "PositionEffect")]
        public PositionEffectEnum PosEfct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PosEfctSpecified;
        #endregion PositionEffectEnum PosEfct
        #region EFS_String Txt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt
        {
            set { txt = new EFS_String(value); }
            get { return txt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TxtSpecified
        {
            set { txtSpecified = value; }
            get { return txtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "58", Anchor = "Text")]
        public EFS_String txt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool txtSpecified;
        #endregion EFS_String Txt
        #region EFS_PosInteger EncTxtLen
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen
        {
            set { encTxtLen = new EFS_PosInteger(value); }
            get { return encTxtLen?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncTxtLenSpecified
        {
            set { encTxtLenSpecified = value; }
            get { return encTxtLenSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Text Length", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "354", Anchor = "EncodedTextLen")]
        public EFS_PosInteger encTxtLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encTxtLenSpecified;
        #endregion EFS_PosInteger EncTxtLen
        #region EFS_String EncTxt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt
        {
            set { encTxt = new EFS_String(value); }
            get { return encTxt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncTxtSpecified
        {
            set { encTxtSpecified = value; }
            get { return encTxtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "EncodedText", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "355", Anchor = "EncodedText")]
        public EFS_String encTxt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encTxtSpecified;
        #endregion EFS_String EncTxt
        #region SideMultiLegReportingTypeEnum MLegRptTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Multileg Reporting Type")]
        [DictionaryGUI(Tag = "752", Anchor = "SideMultiLegReportingType")]
        public SideMultiLegReportingTypeEnum MLegRptTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MLegRptTypSpecified;
        #endregion SideMultiLegReportingTypeEnum MLegRptTyp
        #region ContAmtGrp_Block[] ContAmt
        [System.Xml.Serialization.XmlElementAttribute("ContAmt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contract Amounts")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contract Amount", IsChild = true, MinItem = 0)]
        public ContAmtGrp_Block[] ContAmt;
        #endregion ContAmtGrp_Block[] ContAmt
        #region Stipulations_Block[] Stip
        [System.Xml.Serialization.XmlElementAttribute("Stip", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stipulations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stipulation", IsChild = true, MinItem = 0)]
        public Stipulations_Block[] Stip;
        #endregion Stipulations_Block[] Stip
        #region MiscFeesGrp_Block[] MiscFees
        [System.Xml.Serialization.XmlElementAttribute("MiscFees", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Miscelaneous Fees")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Miscelaneous Fee", IsChild = true, MinItem = 0)]
        public MiscFeesGrp_Block[] MiscFees;
        #endregion MiscFeesGrp_Block[] MiscFees
        #region EFS_String ExchRule
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExchRule
        {
            set { exchRule = new EFS_String(value); }
            get { return exchRule?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExchRuleSpecified
        {
            set { exchRuleSpecified = value; }
            get { return exchRuleSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange Rule", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "825", Anchor = "ExchangeRule")]
        public EFS_String exchRule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchRuleSpecified;
        #endregion EFS_String ExchRule
        #region TradeAllocIndicatorEnum AllocInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Allocation Indicator")]
        [DictionaryGUI(Tag = "826", Anchor = "TradeAllocIndicator")]
        public TradeAllocIndicatorEnum AllocInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AllocIndSpecified;
        #endregion TradeAllocIndicatorEnum AllocInd
        #region PreallocMethodEnum PreallocMeth
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Preallocation Method")]
        [DictionaryGUI(Tag = "591", Anchor = "PreallocMeth")]
        public PreallocMethodEnum PreallocMeth;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PreallocMethSpecified;
        #endregion PreallocMethodEnum PreallocMeth
        #region EFS_String AllocID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AllocID
        {
            set { allocID = new EFS_String(value); }
            get { return allocID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllocIDSpecified
        {
            set { allocIDSpecified = value; }
            get { return allocIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Allocation ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "70", Anchor = "AllocID")]
        public EFS_String allocID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allocIDSpecified;
        #endregion EFS_String AllocID
        #region TrdAllocGrp_Block[] Alloc
        [System.Xml.Serialization.XmlElementAttribute("Alloc", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Allocations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Allocation", IsChild = true, MinItem = 0)]
        public TrdAllocGrp_Block[] Alloc;
        #endregion TrdAllocGrp_Block[] Alloc
        #region SideTrdRegTS_Block[] TrdRegTS
        [System.Xml.Serialization.XmlElementAttribute("TrdRegTS", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Side Timestamps")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Side Timestamp", IsChild = true, MinItem = 0)]
        public SideTrdRegTS_Block[] TrdRegTS;
        #endregion SideTrdRegTS_Block[] TrdRegTS
        #region SettlDetails_Block[] SettlDetails
        [System.Xml.Serialization.XmlElementAttribute("SettlDetails", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement Details")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement Detail", IsChild = true, MinItem = 0)]
        public SettlDetails_Block[] SettlDetails;
        #endregion SettlDetails_Block[] SettlDetails
        #region EFS_Decimal SideGrossTradeAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal SideGrossTradeAmt
        {
            set { sideGrossTradeAmt = new EFS_Decimal(value); }
            get { return (null != sideGrossTradeAmt) ? sideGrossTradeAmt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SideGrossTradeAmtSpecified
        {
            set { sideGrossTradeAmtSpecified = value; }
            get { return sideGrossTradeAmtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Gross Trade Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1072", Anchor = "SideGrossTradeAmt")]
        public EFS_Decimal sideGrossTradeAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sideGrossTradeAmtSpecified;
        #endregion EFS_Decimal SideGrossTradeAmt
        #region AggressorIndicatorEnum AgrsrInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Aggressor Indicator")]
        [DictionaryGUI(Tag = "1057", Anchor = "AggressorIndicator")]
        public AggressorIndicatorEnum AgrsrInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AgrsrIndSpecified;
        #endregion AggressorIndicatorEnum AgrsrInd
        #region EFS_String ExchSpeclInstr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExchSpeclInstr
        {
            set { exchSpeclInstr = new EFS_String(value); }
            get { return exchSpeclInstr?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExchSpeclInstrSpecified
        {
            set { exchSpeclInstrSpecified = value; }
            get { return exchSpeclInstrSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange Special Instructions", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1139", Anchor = "ExchangeSpecialInstructions")]
        public EFS_String exchSpeclInstr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchSpeclInstrSpecified;
        #endregion EFS_String ExchSpeclInstr
        #region RelatedPositionGrp_Block[] ReltdPos
        // PM 20160428 [22107] Ajout à partir de l'Extension Pack: FIX.5.0SP2 EP142
        [System.Xml.Serialization.XmlElementAttribute("ReltdPos", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Related Positions")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Related Position", IsChild = true, MinItem = 0)]
        public RelatedPositionGrp_Block[] ReltdPos;
        #endregion RelatedPositionGrp_Block[] ReltdPos
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion TrdCapRptSideGrp_Block
    #region TrdInstrmtLegGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class TrdInstrmtLegGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region InstrumentLeg_Block Leg
        [System.Xml.Serialization.XmlElementAttribute("Leg", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Instrument Leg")]
        public InstrumentLeg_Block Leg;

        #endregion InstrumentLeg_Block Leg
        #region EFS_Decimal Qty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Qty
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "687", Anchor = "LegQty")]
        public EFS_Decimal qty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qtySpecified;
        #endregion EFS_Decimal Qty
        #region LegSwapTypeEnum SwapTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Swap Type")]
        [DictionaryGUI(Tag = "690", Anchor = "LegSwapType")]
        public LegSwapTypeEnum SwapTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SwapTypSpecified;
        #endregion LegSwapTypeEnum SwapTyp
        #region EFS_String RptID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID
        {
            set { rptID = new EFS_String(value); }
            get { return rptID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RptIDSpecified
        {
            set { rptIDSpecified = value; }
            get { return rptIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Report ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "990", Anchor = "LegReportID")]
        public EFS_String rptID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rptIDSpecified;
        #endregion EFS_String RptID
        #region EFS_Integer LegNo
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string LegNo
        {
            set { legNo = new EFS_Integer(value); }
            get { return legNo?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LegNoSpecified
        {
            set { legNoSpecified = value; }
            get { return legNoSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Number", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1152", Anchor = "LegNumber")]
        public EFS_Integer legNo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legNoSpecified;
        #endregion EFS_Integer LegNo
        #region LegStipulations_Block[] Stip
        [System.Xml.Serialization.XmlElementAttribute("Stip", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Leg Stipulations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Leg Stipulation", IsChild = true, MinItem = 0)]
        public LegStipulations_Block[] Stip;
        #endregion LegStipulations_Block[] Stip
        #region PositionEffectEnum PosEfct
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Position Effect")]
        [DictionaryGUI(Tag = "564", Anchor = "LegPositionEffect")]
        public PositionEffectEnum PosEfct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PosEfctSpecified;
        #endregion PositionEffectEnum PosEfct
        #region LegCoveredOrUncoveredEnum Cover
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Covered Or Uncovered")]
        [DictionaryGUI(Tag = "565", Anchor = "LegCoveredOrUncovered")]
        public LegCoveredOrUncoveredEnum Cover;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CoverSpecified;
        #endregion LegCoveredOrUncoveredEnum Cover
        #region NestedParties_Block[] Pty
        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Nested Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Nested Party", IsChild = true, MinItem = 0)]
        public NestedParties_Block[] Pty;
        #endregion NestedParties_Block[] Pty
        #region EFS_String RefID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RefID
        {
            set { refID = new EFS_String(value); }
            get { return refID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RefIDSpecified
        {
            set { refIDSpecified = value; }
            get { return refIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Reference ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "654", Anchor = "LegRefID")]
        public EFS_String refID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool refIDSpecified;
        #endregion EFS_String RefID
        #region SettlTypeEnum SettlTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Settlement Type")]
        [DictionaryGUI(Tag = "587", Anchor = "LegSettlType")]
        public SettlTypeEnum SettlTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlTypSpecified;
        #endregion SettlTypeEnum SettlTyp
        #region EFS_Date SettlDt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Settlement Date")]
        [DictionaryGUI(Tag = "588", Anchor = "LegSettlDate")]
        public EFS_Date settlDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlDtSpecified;
        #endregion EFS_Date SettlDt
        #region EFS_Decimal LastPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastPx
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Last Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "637", Anchor = "LegLastPx")]
        public EFS_Decimal lastPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastPxSpecified;
        #endregion EFS_Decimal LastPx
        #region string SettlCcy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Settlement currency")]
        [DictionaryGUI(Tag = "675", Anchor = "LegSettlCurrency")]
        public string SettlCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlCcySpecified;
        #endregion string SettlCcy
        #region EFS_Decimal LegLastFwdPnts
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LegLastFwdPnts
        {
            set { legLastFwdPnts = new EFS_Decimal(value); }
            get { return (null != legLastFwdPnts) ? legLastFwdPnts.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LegLastFwdPntsSpecified
        {
            set { legLastFwdPntsSpecified = value; }
            get { return legLastFwdPntsSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Last Forward Points", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1073", Anchor = "LegLastForwardPoints")]
        public EFS_Decimal legLastFwdPnts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legLastFwdPntsSpecified;
        #endregion EFS_Decimal LegLastFwdPnts
        #region EFS_Decimal LegCalcCcyLastQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LegCalcCcyLastQty
        {
            set { legCalcCcyLastQty = new EFS_Decimal(value); }
            get { return (null != legCalcCcyLastQty) ? legCalcCcyLastQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LegCalcCcyLastQtySpecified
        {
            set { legCalcCcyLastQtySpecified = value; }
            get { return legCalcCcyLastQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Calculated Curency Last Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1074", Anchor = "LegCalculatedCcyLastQty")]
        public EFS_Decimal legCalcCcyLastQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legCalcCcyLastQtySpecified;
        #endregion EFS_Decimal LegCalcCcyLastQty
        #region EFS_Decimal LegGrossTrdAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LegGrossTrdAmt
        {
            set { legGrossTrdAmt = new EFS_Decimal(value); }
            get { return (null != legGrossTrdAmt) ? legGrossTrdAmt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LegGrossTrdAmtSpecified
        {
            set { legGrossTrdAmtSpecified = value; }
            get { return legGrossTrdAmtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Gross Trade Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1075", Anchor = "LegGrossTradeAmt")]
        public EFS_Decimal legGrossTrdAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legGrossTrdAmtSpecified;
        #endregion EFS_Decimal LegGrossTrdAmt
        #region EFS_Decimal LegVolatility
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LegVolatility
        {
            set { legVolatility = new EFS_Decimal(value); }
            get { return (null != legVolatility) ? legVolatility.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LegVolatilitySpecified
        {
            set { legVolatilitySpecified = value; }
            get { return legVolatilitySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Volatility", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1379", Anchor = "LegVolatility")]
        public EFS_Decimal legVolatility;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legVolatilitySpecified;
        #endregion EFS_Decimal LegVolatility
        #region EFS_Decimal LegDividendYield
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LegDividendYield
        {
            set { legDividendYield = new EFS_Decimal(value); }
            get { return (null != legDividendYield) ? legDividendYield.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LegDividendYieldSpecified
        {
            set { legDividendYieldSpecified = value; }
            get { return legDividendYieldSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Dividend Yield", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1381", Anchor = "LegDividendYield")]
        public EFS_Decimal legDividendYield;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legDividendYieldSpecified;
        #endregion EFS_Decimal LegDividendYield
        #region EFS_Decimal LegCurrencyRatio
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LegCurrencyRatio
        {
            set { legCurrencyRatio = new EFS_Decimal(value); }
            get { return (null != legCurrencyRatio) ? legCurrencyRatio.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LegCurrencyRatioSpecified
        {
            set { legCurrencyRatioSpecified = value; }
            get { return legCurrencyRatioSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Currency Ratio", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1383", Anchor = "LegCurrencyRatio")]
        public EFS_Decimal legCurrencyRatio;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legCurrencyRatioSpecified;
        #endregion EFS_Decimal LegCurrencyRatio
        #region ExecInstEnum LegExecInst
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg Execution Instruction")]
        [DictionaryGUI(Tag = "1384", Anchor = "LegExecInst")]
        public ExecInstEnum LegExecInst;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LegExecInstSpecified;
        #endregion ExecInstEnum LegExecInst
        #region EFS_Decimal LastQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastQty
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "LastQty", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1418", Anchor = "LegLastQty")]
        public EFS_Decimal lastQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastQtySpecified;
        #endregion EFS_Decimal LastQty
        #region TradeCapLegUnderlyingsGrp_Block[]
        [System.Xml.Serialization.XmlElementAttribute("TradeCapLegUndlyGrp", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying Instrument Legs")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying Instrument Leg", IsChild = true, MinItem = 0)]
        public TradeCapLegUnderlyingsGrp_Block[] TradeCapLegUndlyGrp;
        #endregion TradeCapLegUnderlyingsGrp_Block[]
        #endregion Members

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
    #endregion TrdInstrmtLegGrp_Block
    #region TrdRepIndicatorsGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class TrdRepIndicatorsGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region PartyRoleEnum PtyRole
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Reporting Party Role")]
        [DictionaryGUI(Tag = "1388", Anchor = "TrdRepPartyRole")]
        public PartyRoleEnum PtyRole;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PtyRoleSpecified;
        #endregion PartyRoleEnum PtyRole
        #region YesNoEnum TrdRepInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Report Trade Indicator")]
        [DictionaryGUI(Tag = "1389", Anchor = "TrdRepIndicator")]
        public YesNoEnum TrdRepInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdRepIndSpecified;
        #endregion YesNoEnum TrdRepInd
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion TrdRepIndicatorsGrp_Block
    #region TrdRegTimestamps_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    /// EG 20171004 [23452] Upd TS
    // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
    public partial class TrdRegTimestamps_Block : ItemGUI, IEFS_Array
    {
        #region EFS_DateTime TS
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TS
        {
            set { ts = new EFS_DateTimeOffset(value); }
            get { return (null != ts) ? ts.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TSSpecified
        {
            set { tsSpecified = value; }
            get { return EFS.ACommon.StrFunc.IsFilled(TS); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tsSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Regulatory Timestamp", LblWidth = 130)]
        [DictionaryGUI(Tag = "769", Anchor = "TrdRegTimestamp")]
        public EFS_DateTimeOffset ts;

        #endregion EFS_DateTime TS
        #region TrdRegTimestampTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Regulatory Timestamp Type")]
        [DictionaryGUI(Tag = "770", Anchor = "TrdRegTimestampType")]
        public TrdRegTimestampTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion TrdRegTimestampTypeEnum Typ
        #region EFS_String Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Src
        {
            set { src = new EFS_String(value); }
            get { return src?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SrcSpecified
        {
            set { srcSpecified = value; }
            get { return srcSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Regulatory Timestamp Origin", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "771", Anchor = "TrdRegTimestampTOrigin")]
        public EFS_String src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool srcSpecified;
        #endregion EFS_String Src
        #region DeskTypeEnum DskTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Desk Type")]
        [DictionaryGUI(Tag = "1033", Anchor = "DeskType")]
        public DeskTypeEnum DskTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DskTypSpecified;
        #endregion DeskTypeEnum DskTyp
        #region DeskTypeSourceEnum DskTypSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Desk Type Source")]
        [DictionaryGUI(Tag = "1034", Anchor = "DeskTypeSource")]
        public DeskTypeSourceEnum DskTypSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DskTypSrcSpecified;
        #endregion DeskTypeSourceEnum DskTypSrc
        #region DeskOrderHandlingInstEnum DskOrdHndlInst
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Desk Order Handling Instruction")]
        [DictionaryGUI(Tag = "1035", Anchor = "DeskOrderHandlingInst")]
        public DeskOrderHandlingInstEnum DskOrdHndlInst;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool DskOrdHndlInstSpecified;
        #endregion DeskOrderHandlingInstEnum DskOrdHndlInst

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion TrdRegTimestamps_Block
    #region YieldData_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class YieldData_Block : ItemGUI
    {
        #region YieldTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [DictionaryGUI(Tag = "235", Anchor = "YieldType")]
        public FixML.Enum.YieldTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion YieldTypeEnum Typ
        #region EFS_Decimal Yld
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Yld
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Yield", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "236", Anchor = "Yield")]
        public EFS_Decimal yld;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool yldSpecified;
        #endregion EFS_Decimal Yld
        #region EFS_Date CalcDt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Date")]
        [DictionaryGUI(Tag = "701", Anchor = "YieldCalcDate")]
        public EFS_Date calcDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calcDtSpecified;
        #endregion EFS_Date CalcDt
        #region EFS_Date RedDt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption Date")]
        [DictionaryGUI(Tag = "696", Anchor = "YieldRedemptionDate")]
        public EFS_Date redDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redDtSpecified;
        #endregion EFS_Date RedDt
        #region EFS_Decimal RedPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal RedPx
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "697", Anchor = "YieldRedemptionPrice")]
        public EFS_Decimal redPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redPxSpecified;
        #endregion EFS_Decimal RedPx
        #region PriceTypeEnum RedPxTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption Price Type")]
        [DictionaryGUI(Tag = "698", Anchor = "YieldRedemptionPriceType")]
        public PriceTypeEnum RedPxTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RedPxTypSpecified;
        #endregion PriceTypeEnum RedPxTyp
    }
    #endregion YieldData_Block
    #region UnderlyingInstrument_Block
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(UndInstrmtGrp_Block))]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class UnderlyingInstrument_Block : ItemGUI, IEFS_Array
    {
        #region EFS_String Sym
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { sym = new EFS_String(value); }
            get { return sym?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SymSpecified
        {
            set { symSpecified = value; }
            get { return symSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "311", Anchor = "UnderlyingSymbol")]
        public EFS_String sym;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool symSpecified;
        #endregion EFS_String Sym
        #region SymbolSfxEnum Sfx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol Suffix")]
        [DictionaryGUI(Tag = "312", Anchor = "UnderlyingSymbolSfx")]
        public SymbolSfxEnum Sfx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SfxSpecified;
        #endregion SymbolSfxEnum Sfx
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "309", Anchor = "UnderlyingSecurityID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_String ID
        #region SecurityIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security ID Source")]
        [DictionaryGUI(Tag = "305", Anchor = "UnderlyingSecurityIDSource")]
        public SecurityIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion SecurityIDSourceEnum Src
        #region UndSecAltIDGrp_Block[] UndAID
        [System.Xml.Serialization.XmlElementAttribute("UndAID", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Alternate Id", IsChild = true, MinItem = 0)]
        public UndSecAltIDGrp_Block[] UndAID;
        #endregion UndSecAltIDGrp_Block[] UndAID
        #region ProductEnum Prod
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Product")]
        [DictionaryGUI(Tag = "462", Anchor = "UnderlyingProduct")]
        public ProductEnum Prod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ProdSpecified;
        #endregion ProductEnum Prod
        #region EFS_String CFI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CFI
        {
            set { cfi = new EFS_String(value); }
            get { return cfi?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFISpecified
        {
            set { cfiSpecified = value; }
            get { return cfiSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CFI Code", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "463", Anchor = "UnderlyingCFICode")]
        public EFS_String cfi;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cfiSpecified;
        #endregion EFS_String CFI
        #region SecurityTypeEnum SecTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Type")]
        [DictionaryGUI(Tag = "310", Anchor = "UnderlyingSecurityType")]
        public SecurityTypeEnum SecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecTypSpecified;
        #endregion SecurityTypeEnum SecTyp
        #region EFS_String SubTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SubTyp
        {
            set { subTyp = new EFS_String(value); }
            get { return subTyp?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubTypSpecified
        {
            set { subTypSpecified = value; }
            get { return subTypSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Sub Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "763", Anchor = "UnderlyingSecuritySubType")]
        public EFS_String subTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool subTypSpecified;
        #endregion EFS_String SubTyp
        #region EFS_String MMY
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MMY
        {
            set { mMY = new EFS_String(value); }
            get { return mMY?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MMYSpecified
        {
            set { mMYSpecified = value; }
            get { return mMYSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Month Year", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "313", Anchor = "UnderlyingMaturityMonthYear")]
        public EFS_String mMY;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mMYSpecified;
        #endregion EFS_String MMY
        #region EFS_Date Mat
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Date")]
        [DictionaryGUI(Tag = "542", Anchor = "UnderlyingMaturityDate")]
        public EFS_Date mat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matSpecified;
        #endregion EFS_Date Mat
        #region EFS_Time MatTm
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
        public DateTime MatTm
        {
            set { matTm = new EFS_Time(value.ToString(Cst.FixML_TimeFmt)); }
            get { return (null != matTm) ? matTm.TimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MatTmSpecified
        {
            set { matTmSpecified = value; }
            get { return matTmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Time")]
        [DictionaryGUI(Tag = "1213", Anchor = "UnderlyingMaturityTime")]
        public EFS_Time matTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matTmSpecified;
        #endregion EFS_Time MatTm
        #region EFS_Date CpnPmt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Payment Date")]
        [DictionaryGUI(Tag = "241", Anchor = "UnderlyingCouponPaymentDate")]
        public EFS_Date cpnPmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnPmtSpecified;
        #endregion EFS_Date CpnPmt
        #region EFS_Date Issued
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issue Date")]
        [DictionaryGUI(Tag = "242", Anchor = "UnderlyingIssueDate")]
        public EFS_Date issued;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuedSpecified;
        #endregion EFS_Date Issued
        #region EFS_Integer RepoCollSecTyp
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string RepoCollSecTyp
        {
            set { repoCollSecTyp = new EFS_Integer(value); }
            get { return repoCollSecTyp?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoCollSecTypSpecified
        {
            set { repoCollSecTypSpecified = value; }
            get { return repoCollSecTypSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo Collateral Security Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "243", Anchor = "UnderlyingRepoCollateralSecurityType")]
        public EFS_Integer repoCollSecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoCollSecTypSpecified;
        #endregion EFS_Integer RepoCollSecTyp
        #region EFS_Integer RepoTrm
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string RepoTrm
        {
            set { repoTrm = new EFS_Integer(value); }
            get { return repoTrm?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RepoTrmSpecified
        {
            set { repoTrmSpecified = value; }
            get { return repoTrmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "244", Anchor = "UnderlyingRepurchaseTerm")]
        public EFS_Integer repoTrm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoTrmSpecified;
        #endregion EFS_Integer RepoTrm
        #region EFS_Decimal RepoRt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal RepoRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repurchase Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "245", Anchor = "UnderlyingRepurchaseRate")]
        public EFS_Decimal repoRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repoRtSpecified;
        #endregion EFS_Decimal RepoRt
        #region EFS_Decimal Fctr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Fctr
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Factor", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "246", Anchor = "UnderlyingFactor")]
        public EFS_Decimal fctr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fctrSpecified;
        #endregion EFS_Decimal Fctr
        #region EFS_String CrdRtg
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CrdRtg
        {
            set { crdRtg = new EFS_String(value); }
            get { return crdRtg?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CrdRtgSpecified
        {
            set { crdRtgSpecified = value; }
            get { return crdRtgSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit Rating", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "256", Anchor = "UnderlyingCreditRating")]
        public EFS_String crdRtg;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool crdRtgSpecified;
        #endregion EFS_String CrdRtg
        #region EFS_String Rgstry
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Rgstry
        {
            set { rgstry = new EFS_String(value); }
            get { return rgstry?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RgstrySpecified
        {
            set { rgstrySpecified = value; }
            get { return rgstrySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Registry", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "595", Anchor = "UnderlyingInstrRegistry")]
        public EFS_String rgstry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rgstrySpecified;
        #endregion EFS_String Rgstry
        #region string Ctry
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country Of Issue")]
        [DictionaryGUI(Tag = "592", Anchor = "UnderlyingCountryOfIssue")]
        public string Ctry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CtrySpecified;
        #endregion Ctry
        #region EFS_String StOrProvnc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StOrProvnc
        {
            set { stOrProvnc = new EFS_String(value); }
            get { return stOrProvnc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StOrProvncSpecified
        {
            set { stOrProvncSpecified = value; }
            get { return stOrProvncSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "State/Province Of Issue", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "593", Anchor = "UnderlyingStateOrProvinceOfIssue")]
        public EFS_String stOrProvnc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stOrProvncSpecified;
        #endregion EFS_String StOrProvnc
        #region EFS_String Lcl
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Lcl
        {
            set { lcl = new EFS_String(value); }
            get { return lcl?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LclSpecified
        {
            set { lclSpecified = value; }
            get { return lclSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Locale Of Issue", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "594", Anchor = "UnderlyingLocaleOfIssue")]
        public EFS_String lcl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lclSpecified;
        #endregion EFS_String Lcl
        #region EFS_Date Redeem
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption Date")]
        [DictionaryGUI(Tag = "247", Anchor = "UnderlyingRedemptionDate")]
        public EFS_Date redeem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redeemSpecified;
        #endregion EFS_Date Redeem
        #region EFS_Decimal StrkPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal StrkPx
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "316", Anchor = "UnderlyingStrikePrice")]
        public EFS_Decimal strkPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkPxSpecified;
        #endregion EFS_Decimal StrkPx
        #region string StrkCcy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike currency")]
        [DictionaryGUI(Tag = "941", Anchor = "UnderlyingStrikeCurrency")]
        public string StrkCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StrkCcySpecified;
        #endregion string StrkCcy
        #region EFS_String OptA
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptA
        {
            set { optA = new EFS_String(value); }
            get { return optA?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OptASpecified
        {
            set { optASpecified = value; }
            get { return optASpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option Attribute", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "317", Anchor = "UnderlyingOptAttribute")]
        public EFS_String optA;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optASpecified;
        #endregion EFS_String OptA
        #region EFS_Decimal Mult
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Mult
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Multiplier", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "436", Anchor = "UnderlyingContractMultiplier")]
        public EFS_Decimal mult;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multSpecified;
        #endregion EFS_Decimal Mult
        #region UnitOfMeasureEnum UOM
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unit Of Measure")]
        [DictionaryGUI(Tag = "998", Anchor = "UnderlyingUnitOfMeasure")]
        public UnitOfMeasureEnum UOM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool UOMSpecified;
        #endregion UnitOfMeasureEnum UOM
        #region EFS_Decimal UOMQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal UOMQty
        {
            set { uOMQty = new EFS_Decimal(value); }
            get { return (null != uOMQty) ? uOMQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UOMQtySpecified
        {
            set { uOMQtySpecified = value; }
            get { return uOMQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unit Of Measure Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1423", Anchor = "UnderlyingUnitOfMeasureQty")]
        public EFS_Decimal uOMQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool uOMQtySpecified;
        #endregion EFS_Decimal UOMQty
        #region UnitOfMeasureEnum PxUOM
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Unit Of Measure")]
        [DictionaryGUI(Tag = "1424", Anchor = "UnderlyingPriceUnitOfMeasure")]
        public UnitOfMeasureEnum PxUOM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PxUOMSpecified;
        #endregion UnitOfMeasureEnum PxUOM
        #region EFS_Decimal PxUOMQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal PxUOMQty
        {
            set { pxUOMQty = new EFS_Decimal(value); }
            get { return (null != pxUOMQty) ? pxUOMQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxUOMQtySpecified
        {
            set { pxUOMQtySpecified = value; }
            get { return pxUOMQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Unit Of Measure Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1425", Anchor = "UnderlyingPriceUnitOfMeasureQty")]
        public EFS_Decimal pxUOMQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxUOMQtySpecified;
        #endregion EFS_Decimal PxUOMQty
        #region TimeUnitEnum TmUnit
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time Unit")]
        [DictionaryGUI(Tag = "1000", Anchor = "UnderlyingTimeUnit")]
        public TimeUnitEnum TmUnit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TmUnitSpecified;
        #endregion TimeUnitEnum TmUnit
        #region DerivativeExerciseStyleEnum ExerStyle
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Style")]
        [DictionaryGUI(Tag = "1419", Anchor = "UnderlyingExerciseStyle")]
        public DerivativeExerciseStyleEnum ExerStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExerStyleSpecified;
        #endregion DerivativeExerciseStyleEnum ExerStyle
        #region EFS_Decimal CpnRt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CpnRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "435", Anchor = "UnderlyingCouponRate")]
        public EFS_Decimal cpnRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cpnRtSpecified;
        #endregion EFS_Decimal CpnRt
        #region string Exch
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Exchange", Width = 500)]
        [DictionaryGUI(Tag = "308", Anchor = "UnderlyingSecurityExchange")]
        public string Exch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExchSpecified;
        #endregion string Exch
        #region EFS_String Issr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Issr
        {
            set { issr = new EFS_String(value); }
            get { return issr?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssrSpecified
        {
            set { issrSpecified = value; }
            get { return issrSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "306", Anchor = "UnderlyingIssuer")]
        public EFS_String issr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issrSpecified;
        #endregion EFS_String Issr
        #region EFS_PosInteger EncUndIssrLen
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncUndIssrLen
        {
            set { encUndIssrLen = new EFS_PosInteger(value); }
            get { return encUndIssrLen?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncUndIssrLenSpecified
        {
            set { encUndIssrLenSpecified = value; }
            get { return encUndIssrLenSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Issuer Length", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "362", Anchor = "EncodedUnderlyingIssuerLen")]
        public EFS_PosInteger encUndIssrLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encUndIssrLenSpecified;
        #endregion EFS_PosInteger EncUndIssrLen
        #region EFS_String EncUndIssr
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncUndIssr
        {
            set { encUndIssr = new EFS_String(value); }
            get { return encUndIssr?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncUndIssrSpecified
        {
            set { encUndIssrSpecified = value; }
            get { return encUndIssrSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Issuer", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "363", Anchor = "EncodedUnderlyingIssuer")]
        public EFS_String encUndIssr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encUndIssrSpecified;
        #endregion EFS_String EncUndIssr
        #region EFS_String Desc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Desc
        {
            set { desc = new EFS_String(value); }
            get { return desc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DescSpecified
        {
            set { descSpecified = value; }
            get { return descSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Description", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "307", Anchor = "UnderlyingSecurityDesc")]
        public EFS_String desc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descSpecified;
        #endregion EFS_String Desc
        #region EFS_PosInteger EncUndSecDescLen
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncUndSecDescLen
        {
            set { encUndSecDescLen = new EFS_PosInteger(value); }
            get { return encUndSecDescLen?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncUndSecDescLenSpecified
        {
            set { encUndSecDescLenSpecified = value; }
            get { return encUndSecDescLenSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Security Description Length", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "364", Anchor = "EncodedUnderlyingSecurityDescLen")]
        public EFS_PosInteger encUndSecDescLen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encUndSecDescLenSpecified;
        #endregion EFS_PosInteger EncUndSecDescLen
        #region EFS_String EncUndSecDesc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncUndSecDesc
        {
            set { encUndSecDesc = new EFS_String(value); }
            get { return encUndSecDesc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EncUndSecDescSpecified
        {
            set { encUndSecDescSpecified = value; }
            get { return encUndSecDescSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Encoded Security Description", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "365", Anchor = "EncodedUnderlyingSecurityDesc")]
        public EFS_String encUndSecDesc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool encUndSecDescSpecified;
        #endregion EFS_String EncUndSecDesc
        #region EFS_String CPPgm
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CPPgm
        {
            set { cPPgm = new EFS_String(value); }
            get { return cPPgm?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CPPgmSpecified
        {
            set { cPPgmSpecified = value; }
            get { return cPPgmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commercial Paper Program", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "877", Anchor = "UnderlyingCPProgram")]
        public EFS_String cPPgm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cPPgmSpecified;
        #endregion EFS_String CPPgm
        #region EFS_String CPRegTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CPRegTyp
        {
            set { cPRegTyp = new EFS_String(value); }
            get { return cPRegTyp?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CPRegTypSpecified
        {
            set { cPRegTypSpecified = value; }
            get { return cPRegTypSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commercial Paper Registration Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "878", Anchor = "UnderlyingCPRegTyp")]
        public EFS_String cPRegTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cPRegTypSpecified;
        #endregion EFS_String CPRegTyp
        #region EFS_Decimal AllocPct
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal AllocPct
        {
            set { allocPct = new EFS_Decimal(value); }
            get { return (null != allocPct) ? allocPct.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllocPctSpecified
        {
            set { allocPctSpecified = value; }
            get { return allocPctSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Allocation Percent", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "972", Anchor = "UnderlyingAllocationPercent")]
        public EFS_Decimal allocPct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allocPctSpecified;
        #endregion EFS_Decimal AllocPct
        #region string Ccy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [DictionaryGUI(Tag = "318", Anchor = "UnderlyingCurrency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        #endregion string Ccy
        #region EFS_Decimal Qty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Qty
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "879", Anchor = "UnderlyingQty")]
        public EFS_Decimal qty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qtySpecified;
        #endregion EFS_Decimal Qty
        #region UnderlyingSettlementTypeEnum SettlTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Type")]
        [DictionaryGUI(Tag = "975", Anchor = "UnderlyingSettlementType")]
        public UnderlyingSettlementTypeEnum SettlTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlTypSpecified;
        #endregion UnderlyingSettlementTypeEnum SettlTyp
        #region EFS_Decimal CashAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CashAmt
        {
            set { cashAmt = new EFS_Decimal(value); }
            get { return (null != cashAmt) ? cashAmt.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CashAmtSpecified
        {
            set { cashAmtSpecified = value; }
            get { return cashAmtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "973", Anchor = "UnderlyingCashAmount")]
        public EFS_Decimal cashAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashAmtSpecified;
        #endregion EFS_Decimal CashAmt
        #region UnderlyingCashTypeEnum CashTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Type")]
        [DictionaryGUI(Tag = "974", Anchor = "UnderlyingCashType")]
        public UnderlyingCashTypeEnum CashTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CashTypSpecified;
        #endregion UnderlyingCashTypeEnum CashTyp
        #region EFS_Decimal Px
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Px
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "810", Anchor = "UnderlyingPx")]
        public EFS_Decimal px;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pxSpecified;
        #endregion EFS_Decimal Px
        #region EFS_Decimal DirtPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal DirtPx
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dirty Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "882", Anchor = "UnderlyingDirtyPrice")]
        public EFS_Decimal dirtPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dirtPxSpecified;
        #endregion EFS_Decimal DirtPx
        #region EFS_Decimal EndPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal EndPx
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "End Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "883", Anchor = "UnderlyingEndPrice")]
        public EFS_Decimal endPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endPxSpecified;
        #endregion EFS_Decimal EndPx
        #region EFS_Decimal StartVal
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal StartVal
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Start Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "884", Anchor = "UnderlyingStartValue")]
        public EFS_Decimal startVal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startValSpecified;
        #endregion EFS_Decimal StartVal
        #region EFS_Decimal CurVal
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CurVal
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Current Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "885", Anchor = "UnderlyingCurrentValue")]
        public EFS_Decimal curVal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool curValSpecified;
        #endregion EFS_Decimal CurVal
        #region EFS_Decimal EndVal
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal EndVal
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "End Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "886", Anchor = "UnderlyingEndValue")]
        public EFS_Decimal endVal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool endValSpecified;
        #endregion EFS_Decimal EndVal
        #region UnderlyingStipulations_Block[] Stip
        [System.Xml.Serialization.XmlElementAttribute("Stip", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying Stipulations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying Stipulation", IsChild = true, MinItem = 0)]
        public UnderlyingStipulations_Block[] Stip;
        #endregion UnderlyingStipulations_Block[] Stip
        #region EFS_Decimal AdjQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal AdjQty
        {
            set { adjQty = new EFS_Decimal(value); }
            get { return (null != adjQty) ? adjQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AdjQtySpecified
        {
            set { adjQtySpecified = value; }
            get { return adjQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1044", Anchor = "UnderlyingAdjustedQuantity")]
        public EFS_Decimal adjQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjQtySpecified;
        #endregion EFS_Decimal AdjQty
        #region EFS_Decimal FxRate
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal FxRate
        {
            set { fxRate = new EFS_Decimal(value); }
            get { return (null != fxRate) ? fxRate.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FxRateSpecified
        {
            set { fxRateSpecified = value; }
            get { return fxRateSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FX Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1045", Anchor = "UnderlyingFXRate")]
        public EFS_Decimal fxRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxRateSpecified;
        #endregion EFS_Decimal FxRate
        #region UnderlyingFXRateCalcEnum FxRateCalc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FX Rate Calculation")]
        [DictionaryGUI(Tag = "1046", Anchor = "UnderlyingFXRateCalc")]
        public UnderlyingFXRateCalcEnum FxRateCalc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool FxRateCalcSpecified;
        #endregion UnderlyingFXRateCalcEnum FxRateCalc
        #region EFS_Decimal CapValu
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CapValu
        {
            set { capValu = new EFS_Decimal(value); }
            get { return (null != capValu) ? capValu.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CapValuSpecified
        {
            set { capValuSpecified = value; }
            get { return capValuSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Capped Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1038", Anchor = "UnderlyingCapValue")]
        public EFS_Decimal capValu;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool capValuSpecified;
        #endregion EFS_Decimal CapValu
        #region UndlyInstrumentParties_Block[] Pty
        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying Instrument Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying Instrument Party", IsChild = true, MinItem = 0)]
        public UndlyInstrumentParties_Block[] Pty;
        #endregion UndlyInstrumentParties_Block[] Pty
        #region EFS_String SetMeth
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SetMeth
        {
            set { setMeth = new EFS_String(value); }
            get { return setMeth?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SetMethSpecified
        {
            set { setMethSpecified = value; }
            get { return setMethSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Method", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1039", Anchor = "UnderlyingSettlMethod")]
        public EFS_String setMeth;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool setMethSpecified;
        #endregion EFS_String SetMeth

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion UnderlyingInstrument_Block
    #region UnderlyingLegInstrument_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class UnderlyingLegInstrument_Block : ItemGUI
    {
        #region Members
        #region EFS_String Sym
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { sym = new EFS_String(value); }
            get { return sym?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SymSpecified
        {
            set { symSpecified = value; }
            get { return symSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1330", Anchor = "UnderlyingLegSymbol")]
        public EFS_String sym;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool symSpecified;
        #endregion EFS_String Sym
        #region EFS_String Sfx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sfx
        {
            set { sfx = new EFS_String(value); }
            get { return sfx?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SfxSpecified
        {
            set { sfxSpecified = value; }
            get { return sfxSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol Suffix", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1331", Anchor = "UnderlyingLegSymbolSfx")]
        public EFS_String sfx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sfxSpecified;
        #endregion EFS_String Sfx
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { iD = new EFS_String(value); }
            get { return iD?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { iDSpecified = value; }
            get { return iDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1332", Anchor = "UnderlyingLegSecurityID")]
        public EFS_String iD;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool iDSpecified;
        #endregion EFS_String ID
        #region EFS_String Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Src
        {
            set { src = new EFS_String(value); }
            get { return src?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SrcSpecified
        {
            set { srcSpecified = value; }
            get { return srcSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security ID Source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1333", Anchor = "UnderlyingLegSecurityIDSrc")]
        public EFS_String src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool srcSpecified;
        #endregion EFS_String Src
        #region UnderlyingLegSecurityAltIDGrp_Block[] AID
        [System.Xml.Serialization.XmlElementAttribute("AID", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Alternate Id", IsChild = true, MinItem = 0)]
        public UnderlyingLegSecurityAltIDGrp_Block[] AID;
        #endregion UnderlyingLegSecurityAltIDGrp_Block[] AID
        #region EFS_String CFI
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CFI
        {
            set { cFI = new EFS_String(value); }
            get { return cFI?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFISpecified
        {
            set { cFISpecified = value; }
            get { return cFISpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CFI Code", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1344", Anchor = "UnderlyingLegCFICode")]
        public EFS_String cFI;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cFISpecified;
        #endregion EFS_String CFI
        #region SecurityTypeEnum SecType
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Type")]
        [DictionaryGUI(Tag = "1337", Anchor = "UnderlyingLegSecurityType")]
        public SecurityTypeEnum SecType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SecTypeSpecified;
        #endregion SecurityTypeEnum SecType
        #region EFS_String SubType
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SubType
        {
            set { subType = new EFS_String(value); }
            get { return subType?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubTypeSpecified
        {
            set { subTypeSpecified = value; }
            get { return subTypeSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Sub Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1338", Anchor = "UnderlyingLegSecuritySubType")]
        public EFS_String subType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool subTypeSpecified;
        #endregion EFS_String SubType
        #region EFS_String MMY
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MMY
        {
            set { mMY = new EFS_String(value); }
            get { return mMY?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MMYSpecified
        {
            set { mMYSpecified = value; }
            get { return mMYSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Month Year", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1339", Anchor = "UnderlyingLegMaturityMonthYear")]
        public EFS_String mMY;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mMYSpecified;
        #endregion EFS_String MMY
        #region EFS_Date MatDt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Date")]
        [DictionaryGUI(Tag = "1345", Anchor = "UnderlyingLegMaturitydate")]
        public EFS_Date matDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matDtSpecified;
        #endregion EFS_Date MatDt
        #region EFS_Time MatTm
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
        public DateTime MatTm
        {
            set { matTm = new EFS_Time(value.ToString(Cst.FixML_TimeFmt)); }
            get { return (null != matTm) ? matTm.TimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MatTmSpecified
        {
            set { matTmSpecified = value; }
            get { return matTmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity Time")]
        [DictionaryGUI(Tag = "1405", Anchor = "UnderlyingLegMaturityTime")]
        public EFS_Time matTm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matTmSpecified;
        #endregion EFS_Time MatTm
        #region EFS_Decimal StrkPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal StrkPx
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1340", Anchor = "UnderlyingLegStrikePrice")]
        public EFS_Decimal strkPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strkPxSpecified;
        #endregion EFS_Decimal StrkPx
        #region EFS_String OptAt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptAt
        {
            set { optAt = new EFS_String(value); }
            get { return optAt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OptAtSpecified
        {
            set { optAtSpecified = value; }
            get { return optAtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option Attribute", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1391", Anchor = "UnderlyingLegOptAttribute")]
        public EFS_String optAt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optAtSpecified;
        #endregion EFS_String OptAt
        #region PutOrCallEnum PutCall
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Put Or Call")]
        [DictionaryGUI(Tag = "1343", Anchor = "UnderlyingLegPutOrCall")]
        public PutOrCallEnum PutCall;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PutCallSpecified;
        #endregion PutOrCallEnum PutCall
        #region string Exch
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Exchange", Width = 500)]
        [DictionaryGUI(Tag = "1341", Anchor = "UnderlyingLegSecurityExchange")]
        public string Exch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExchSpecified;
        #endregion string Exch
        #region EFS_String Desc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Desc
        {
            set { desc = new EFS_String(value); }
            get { return desc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DescSpecified
        {
            set { descSpecified = value; }
            get { return descSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security Description", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1392", Anchor = "UnderlyinLegSecurityDesc")]
        public EFS_String desc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descSpecified;
        #endregion EFS_String Desc
        #endregion Members
    }
    #endregion UnderlyingLegInstrument_Block
    #region UnderlyingLegSecurityAltIDGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class UnderlyingLegSecurityAltIDGrp_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String SecAltID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SecAltID
        {
            set { secAltID = new EFS_String(value); }
            get { return secAltID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SecAltIDSpecified
        {
            set { secAltIDSpecified = value; }
            get { return secAltIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id", Width = 200)]
        [DictionaryGUI(Tag = "1335", Anchor = "UnderlyingLegSecurityAltID")]
        public EFS_String secAltID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secAltIDSpecified;
        #endregion EFS_String SecAltID
        #region SecurityIDSourceEnum AltIDSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1336", Anchor = "UnderlyingLegSecurityAltIDSource")]
        public SecurityIDSourceEnum AltIDSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AltIDSrcSpecified;
        #endregion SecurityIDSourceEnum AltIDSrc
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }
    #endregion UnderlyingLegSecurityAltIDGrp_Block
    #region UnderlyingStipulations_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class UnderlyingStipulations_Block : ItemGUI, IEFS_Array
    {
        #region StipulationTypeEnum Typ
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [DictionaryGUI(Tag = "888", Anchor = "UnderlyingStipType")]
        public StipulationTypeEnum Typ;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TypSpecified;
        #endregion StipulationTypeEnum Typ
        #region EFS_String Val
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Val
        {
            set { val = new EFS_String(value); }
            get { return val?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ValSpecified
        {
            set { valSpecified = value; }
            get { return valSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "889", Anchor = "UnderlyingStipValue")]
        public EFS_String val;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valSpecified;
        #endregion EFS_String Val

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion UnderlyingStipulations_Block
    #region UndInstrmtGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class UndInstrmtGrp_Block : UnderlyingInstrument_Block
    {
    }
    #endregion UndInstrmtGrp_Block
    #region UndlyInstrumentPtysSubGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class UndlyInstrumentPtysSubGrp_Block
    {

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PartySubIDTypeEnum Typ;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypSpecified;
    }
    #endregion UndlyInstrumentPtysSubGrp_Block
    #region UndlyInstrumentParties_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class UndlyInstrumentParties_Block : ItemGUI, IEFS_Array
    {
        #region Members
        #region FixPartyReference ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set {
                id = new FixPartyReference
                {
                    href = value
                };
            }
            get { return id?.href; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "Instrument Party ID", LineFeed = MethodsGUI.LineFeedEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [DictionaryGUI(Tag = "1059", Anchor = "UndlyInstrumentPartyID")]
        public FixPartyReference id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion FixPartyReference ID
        #region PartyIDSourceEnum Src
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [DictionaryGUI(Tag = "1060", Anchor = "UndlyInstrumentPartyIDSource")]
        public PartyIDSourceEnum Src;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SrcSpecified;
        #endregion PartyIDSourceEnum Src
        #region PartyRoleEnum R
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Role")]
        [DictionaryGUI(Tag = "1061", Anchor = "UndlyInstrumentPartyRole")]
        public PartyRoleEnum R;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RSpecified;
        #endregion PartyRoleEnum R
        #region UndlyInstrumentPtysSubGrp_Block[] Sub
        [System.Xml.Serialization.XmlElementAttribute("Sub", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Alternate Id", MinItem = 0)]
        public UndlyInstrumentPtysSubGrp_Block[] Sub;
        #endregion UndlyInstrumentPtysSubGrp_Block[] Sub
        #endregion Members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion UndlyInstrumentParties_Block
    #region UndSecAltIDGrp_Block
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class UndSecAltIDGrp_Block
    {

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AltID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SecurityIDSourceEnum AltIDSrc;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AltIDSrcSpecified;
    }
    #endregion UndSecAltIDGrp_Block
    #endregion Components Block

    #region Header
    #region Abstract_message
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCaptureReportAck_message))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCaptureReportRequestAck_message))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCaptureReport_message))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCaptureReportRequest_message))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FixPositionReport))]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class Abstract_message : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("Hdr", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Standard Header")]
        public MessageHeader Hdr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool HdrSpecified;
        #endregion Members
    }
    #endregion Abstract_message
    #region BaseHeader
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BatchHeader))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageHeader))]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class BaseHeader : ItemGUI
    {
        #region Members
        #region EFS_String SID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SID
        {
            set { sid = new EFS_String(value); }
            get { return sid?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SIDSpecified
        {
            set { sidSpecified = value; }
            get { return sidSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sender Company ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "49", Anchor = "SenderCompID")]
        public EFS_String sid;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sidSpecified;
        #endregion EFS_String SID
        #region EFS_String TID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TID
        {
            set { tid = new EFS_String(value); }
            get { return tid?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TIDSpecified
        {
            set { tidSpecified = value; }
            get { return tidSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Target Company ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "56", Anchor = "TargetCompID")]
        public EFS_String tid;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tidSpecified;
        #endregion EFS_String TID
        #region EFS_String OBID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OBID
        {
            set { obid = new EFS_String(value); }
            get { return obid?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OBIDSpecified
        {
            set { obidSpecified = value; }
            get { return obidSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "On Behalf Of Company ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "115", Anchor = "OnBehalfOfCompID")]
        public EFS_String obid;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool obidSpecified;
        #endregion EFS_String OBID
        #region EFS_String D2ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string D2ID
        {
            set { d2ID = new EFS_String(value); }
            get { return d2ID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool D2IDSpecified
        {
            set { d2IDSpecified = value; }
            get { return d2IDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Deliver To Company ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "128", Anchor = "DeliverToCompID")]
        public EFS_String d2ID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool d2IDSpecified;
        #endregion EFS_String D2ID
        #region EFS_String SSub
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SSub
        {
            set { sSub = new EFS_String(value); }
            get { return sSub?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SSubSpecified
        {
            set { sSubSpecified = value; }
            get { return sSubSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sender Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "50", Anchor = "SenderSubID")]
        public EFS_String sSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sSubSpecified;
        #endregion EFS_String SSub
        #region EFS_String SLoc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SLoc
        {
            set { sLoc = new EFS_String(value); }
            get { return sLoc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SLocSpecified
        {
            set { sLocSpecified = value; }
            get { return sLocSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sender Location ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "142", Anchor = "SenderLocationID")]
        public EFS_String sLoc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sLocSpecified;
        #endregion EFS_String SLoc
        #region EFS_String TSub
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TSub
        {
            set { tSub = new EFS_String(value); }
            get { return tSub?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TSubSpecified
        {
            set { tSubSpecified = value; }
            get { return tSubSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Target Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "57", Anchor = "TargetSubID")]
        public EFS_String tSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tSubSpecified;
        #endregion EFS_String TSub
        #region EFS_String TLoc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TLoc
        {
            set { tLoc = new EFS_String(value); }
            get { return tLoc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TLocSpecified
        {
            set { tLocSpecified = value; }
            get { return tLocSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Target Location ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "143", Anchor = "TargetLocationID")]
        public EFS_String tLoc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tLocSpecified;
        #endregion EFS_String TLoc
        #region EFS_String OBSub
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OBSub
        {
            set { obSub = new EFS_String(value); }
            get { return obSub?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OBSubSpecified
        {
            set { obSubSpecified = value; }
            get { return obSubSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "On Behalf Of Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "116", Anchor = "OnBehalfOfSubID")]
        public EFS_String obSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool obSubSpecified;
        #endregion EFS_String OBSub
        #region EFS_String OBLoc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OBLoc
        {
            set { obLoc = new EFS_String(value); }
            get { return obLoc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OBLocSpecified
        {
            set { obLocSpecified = value; }
            get { return obLocSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "On Behalf Of Location ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "144", Anchor = "OnBehalfOfLocationID")]
        public EFS_String obLoc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool obLocSpecified;
        #endregion EFS_String OBLoc
        #region EFS_String D2Sub
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string D2Sub
        {
            set { d2Sub = new EFS_String(value); }
            get { return d2Sub?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool D2SubSpecified
        {
            set { d2SubSpecified = value; }
            get { return d2SubSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Deliver To Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "129", Anchor = "DeliverToSubID")]
        public EFS_String d2Sub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool d2SubSpecified;
        #endregion EFS_String D2Sub
        #region EFS_String D2Loc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string D2Loc
        {
            set { d2Loc = new EFS_String(value); }
            get { return d2Loc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool D2LocSpecified
        {
            set { d2LocSpecified = value; }
            get { return d2LocSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Deliver To Location ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "145", Anchor = "DeliverToLocationID")]
        public EFS_String d2Loc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool d2LocSpecified;
        #endregion EFS_String D2Loc
        #region PossDupFlagEnum PosDup
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Possible Dupplication Flag")]
        [DictionaryGUI(Tag = "43", Anchor = "PossDupFlag")]
        public PossDupFlagEnum PosDup;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PosDupSpecified;
        #endregion PossDupFlagEnum PosDup
        #region PossResendEnum PosRsnd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Possible Resend")]
        [DictionaryGUI(Tag = "97", Anchor = "PossResend")]
        public PossResendEnum PosRsnd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PosRsndSpecified;
        #endregion PossResendEnum PosRsnd
        #region EFS_DateTime Snt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime Snt
        {
            set { snt = new EFS_DateTime(value.ToString(Cst.FixML_DateTimeFmt)); }
            get { return (null != snt) ? snt.DateTimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SntSpecified
        {
            set { sntSpecified = value; }
            get { return sntSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "SendingTime")]
        [DictionaryGUI(Tag = "52", Anchor = "SendingTime")]
        public EFS_DateTime snt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sntSpecified;
        #endregion EFS_DateTime Snt
        #region EFS_DateTime OrigSnt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime OrigSnt
        {
            set { origSnt = new EFS_DateTime(value.ToString(Cst.FixML_DateTimeFmt)); }
            get { return (null != origSnt) ? origSnt.DateTimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrigSntSpecified
        {
            set { origSntSpecified = value; }
            get { return origSntSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Original Sending Time")]
        [DictionaryGUI(Tag = "122", Anchor = "OrigSendingTime")]
        public EFS_DateTime origSnt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool origSntSpecified;
        #endregion EFS_DateTime OrigSnt
        #region EFS_String MsgEncd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MsgEncd
        {
            set { msgEncd = new EFS_String(value); }
            get { return msgEncd?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MsgEncdSpecified
        {
            set { msgEncdSpecified = value; }
            get { return msgEncdSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Message Encoding", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "347", Anchor = "MessageEncoding")]
        public EFS_String msgEncd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool msgEncdSpecified;
        #endregion EFS_String MsgEncd
        #region Hop[] Hop
        [System.Xml.Serialization.XmlElementAttribute("Hop", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Hop")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Hop", IsChild = true, MinItem = 0)]
        public Hop[] Hop;
        #endregion Hop[] Hop
        #endregion Members
    }
    #endregion BaseHeader
    #region Batch
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class Batch : ItemGUI, IEFS_Array
    {
        [System.Xml.Serialization.XmlElementAttribute("Hdr", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Batch Header")]
        public BatchHeader Hdr;

        [System.Xml.Serialization.XmlElementAttribute("Message", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Messages")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Message", IsChild = true, MinItem = 0)]
        public Abstract_message[] Message;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string ID;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string TotMsg;

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
    #endregion Batch
    #region BatchHeader
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class BatchHeader : BaseHeader
    {
    }
    #endregion BatchHeader
    #region Hop
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class Hop : ItemGUI, IEFS_Array
    {
        #region Members
        #region EFS_String ID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            set { id = new EFS_String(value); }
            get { return id?.Value; }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool IDSpecified
        {
            set { idSpecified = value; }
            get { return idSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Hop ID", Width = 60)]
        [DictionaryGUI(Tag = "628", Anchor = "HopCompID")]
        public EFS_String id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idSpecified;
        #endregion EFS_String ID
        #region EFS_DateTime Snt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime Snt
        {
            set { snt = new EFS_DateTime(value.ToString(Cst.FixML_DateTimeFmt)); }
            get { return (null != snt) ? snt.DateTimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool SntSpecified
        {
            set { sntSpecified = value; }
            get { return sntSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sending Time")]
        [DictionaryGUI(Tag = "629", Anchor = "HopSendingTime")]
        public EFS_DateTime snt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sntSpecified;
        #endregion EFS_DateTime Snt
        #region EFS_PosInteger Ref
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")]
        public string Ref
        {
            set { refID = new EFS_PosInteger(value); }
            get { return refID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RefSpecified
        {
            set { refIDSpecified = value; }
            get { return refIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "630", Anchor = "HopRefID")]
        public EFS_PosInteger refID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool refIDSpecified;
        #endregion EFS_PosInteger Ref
        #endregion Members

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
    #endregion Hop
    #region MessageHeader
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public partial class MessageHeader : BaseHeader
    {
        #region EFS_PosInteger SeqNum
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")]
        public string SeqNum
        {
            set { seqNum = new EFS_PosInteger(value); }
            get { return seqNum?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SeqNumSpecified
        {
            set { seqNumSpecified = value; }
            get { return seqNumSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Integer message sequence number.", Width = 60, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "34", Anchor = "MegSeqNum")]
        public EFS_PosInteger seqNum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool seqNumSpecified;
        #endregion EFS_PosInteger SeqNum
    }
    #endregion MessageHeader
    #endregion Header

    #region Message
    #region TradeCaptureReport_message
    [System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    [System.Xml.Serialization.XmlRootAttribute("TrdCaptRpt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", IsNullable = false)]
    /// EG 20171004 [23452] Upd TxnTm, TrdRegTS, LastUpdateTm
    public partial class TradeCaptureReport_message : Abstract_message
    {
        #region Members
        #region ApplicationSequenceControl_Block ApplSeqCtrl
        [System.Xml.Serialization.XmlElementAttribute("ApplSeqCtrl", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Application Sequence Control")]
        public ApplicationSequenceControl_Block ApplSeqCtrl;
        #endregion ApplicationSequenceControl_Block ApplSeqCtrl
        #region EFS_String RptID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID
        {
            set { rptID = new EFS_String(value); }
            get { return rptID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RptIDSpecified
        {
            set { rptIDSpecified = value; }
            get { return rptIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "IDs", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Report ID", LblWidth = 140, Width = 100)]
        [DictionaryGUI(Tag = "571", Anchor = "TradeReportID")]
        public EFS_String rptID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rptIDSpecified;
        #endregion EFS_String RptID
        #region EFS_String TrdID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdID
        {
            set { trdID = new EFS_String(value); }
            get { return trdID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdIDSpecified
        {
            set { trdIDSpecified = value; }
            get { return trdIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade ID", LblWidth = 100, Width = 100)]
        [DictionaryGUI(Tag = "1003", Anchor = "TradeID")]
        public EFS_String trdID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trdIDSpecified;
        #endregion EFS_String TrdID
        #region EFS_String TrdID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdID2
        {
            set { trdID2 = new EFS_String(value); }
            get { return trdID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdID2Specified
        {
            set { trdID2Specified = value; }
            get { return trdID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Trade ID")]
        [DictionaryGUI(Tag = "1040", Anchor = "SecondaryTradeId")]
        public EFS_String trdID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trdID2Specified;
        #endregion EFS_String TrdID2
        #region EFS_String FirmTrdID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FirmTrdID
        {
            set { firmTrdID = new EFS_String(value); }
            get { return firmTrdID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FirmTrdIDSpecified
        {
            set { firmTrdIDSpecified = value; }
            get { return firmTrdIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Firm Trade ID")]
        [DictionaryGUI(Tag = "1041", Anchor = "FirmTradeID")]
        public EFS_String firmTrdID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firmTrdIDSpecified;
        #endregion EFS_String FirmTrdID
        #region EFS_String FirmTrdID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FirmTrdID2
        {
            set { firmTrdID2 = new EFS_String(value); }
            get { return firmTrdID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FirmTrdID2Specified
        {
            set { firmTrdID2Specified = value; }
            get { return firmTrdID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Firm Trade ID")]
        [DictionaryGUI(Tag = "1042", Anchor = "SecondaryFirmTradeID")]
        public EFS_String firmTrdID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firmTrdID2Specified;
        #endregion EFS_String FirmTrdID2
        #region TradeReportTransTypeEnum TransTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "IDs")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Report", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Report Transaction Typ")]
        [DictionaryGUI(Tag = "487", Anchor = "TradeReportTransType")]
        public TradeReportTransTypeEnum TransTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TransTypSpecified;
        #endregion TradeReportTransTypeEnum TransTyp
        #region TradeReportTypeEnum RptTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Report Type")]
        [DictionaryGUI(Tag = "856", Anchor = "TradeReportType")]
        public TradeReportTypeEnum RptTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RptTypSpecified;
        #endregion TradeReportTypeEnum RptTyp
        #region TrdRptStatusEnum TrdRptStat
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Report Status")]
        [DictionaryGUI(Tag = "939", Anchor = "TrdRptStatus")]
        public TrdRptStatusEnum TrdRptStat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdRptStatSpecified;
        #endregion TrdRptStatusEnum TrdRptStat
        #region EFS_String ReqID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqID
        {
            set { reqID = new EFS_String(value); }
            get { return reqID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ReqIDSpecified
        {
            set { reqIDSpecified = value; }
            get { return reqIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Report")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Request ID")]
        [DictionaryGUI(Tag = "568", Anchor = "TradeRequestID")]
        public EFS_String reqID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool reqIDSpecified;
        #endregion EFS_String ReqID
        #region TrdTypeEnum TrdTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Type")]
        [DictionaryGUI(Tag = "828", Anchor="TrdType")]
        public TrdTypeEnum TrdTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdTypSpecified;
        #endregion TrdTypeEnum TrdTyp
        #region TrdSubTypeEnum TrdSubTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Sub Type")]
        [DictionaryGUI(Tag = "829", Anchor = "TrdSubType")]
        public TrdSubTypeEnum TrdSubTyp;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdSubTypSpecified;
        #endregion EFS_String TrdSubTyp
        
        #region SecondaryTrdTypeEnum TrdTyp2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Trade Type")]
        [DictionaryGUI(Tag = "855", Anchor = "SecondaryTrdType")]
        public SecondaryTrdTypeEnum TrdTyp2;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdTyp2Specified;

        #endregion SecondaryTrdTypeEnum TrdTyp2
        #region TradeHandlingInstrEnum TrdHandlInst
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Handling Instruction")]
        [DictionaryGUI(Tag = "1123", Anchor = "TradeHandlingInstr")]
        public TradeHandlingInstrEnum TrdHandlInst;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdHandlInstSpecified;
        #endregion TradeHandlingInstrEnum TrdHandlInst
        #region TradeHandlingInstrEnum OrigTrdHandlInst
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Original Trade Handling Instruction")]
        [DictionaryGUI(Tag = "1124", Anchor = "OrigTradeHandlingInstr")]
        public TradeHandlingInstrEnum OrigTrdHandlInst;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OrigTrdHandlInstSpecified;
        #endregion TradeHandlingInstrEnum OrigTrdHandlInst
        #region EFS_DateTime OrigTrdDt
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime OrigTrdDt
        {
            set { origTrdDt = new EFS_Date(value.ToString(Cst.FixML_DateFmt)); }
            get { return (null != origTrdDt) ? origTrdDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrigTrdDtSpecified
        {
            set { origTrdDtSpecified = value; }
            get { return origTrdDtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Original Trade Date")]
        [DictionaryGUI(Tag = "1125", Anchor = "OrigTradeDate")]
        public EFS_Date origTrdDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool origTrdDtSpecified;
        #endregion EFS_DateTime OrigTrdDt
        #region EFS_String OrigTrdID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrigTrdID
        {
            set { origTrdID = new EFS_String(value); }
            get { return origTrdID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrigTrdIDSpecified
        {
            set { origTrdIDSpecified = value; }
            get { return origTrdIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Original Trade ID")]
        [DictionaryGUI(Tag = "1126", Anchor = "OrigTradeID")]
        public EFS_String origTrdID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool origTrdIDSpecified;
        #endregion EFS_String OrigTrdID
        #region EFS_String OrignTrdID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrignTrdID2
        {
            set { orignTrdID2 = new EFS_String(value); }
            get { return orignTrdID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrignTrdID2Specified
        {
            set { orignTrdID2Specified = value; }
            get { return orignTrdID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Orignal Secondary Trade ID")]
        [DictionaryGUI(Tag = "1127", Anchor = "OrigSecondaryTradeID")]
        public EFS_String orignTrdID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool orignTrdID2Specified;
        #endregion EFS_String OrignTrdID2
        #region EFS_String TrnsfrRsn
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrnsfrRsn
        {
            set { trnsfrRsn = new EFS_String(value); }
            get { return trnsfrRsn?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrnsfrRsnSpecified
        {
            set { trnsfrRsnSpecified = value; }
            get { return trnsfrRsnSpecified; }
        }
        [System.Xml.Serialization.XmlIgnore()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transfer Reason")]
        [DictionaryGUI(Tag = "830", Anchor = "TransferReason")]
        public EFS_String trnsfrRsn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trnsfrRsnSpecified;
        #endregion EFS_String TrnsfrRsn
        #region ExecTypeEnum ExecTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution Type")]
        [DictionaryGUI(Tag = "150", Anchor = "ExecType")]
        public ExecTypeEnum ExecTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ExecTypSpecified;
        #endregion ExecTypeEnum ExecTyp
        #region EFS_String TotNumTrdRpts
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string TotNumTrdRpts
        {
            set { totNumTrdRpts = new EFS_String(value); }
            get { return totNumTrdRpts?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotNumTrdRptsSpecified
        {
            set { totNumTrdRptsSpecified = value; }
            get { return totNumTrdRptsSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number of Trade Reports returned")]
        [DictionaryGUI(Tag = "748", Anchor = "TotNumTradeReports")]
        public EFS_String totNumTrdRpts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totNumTrdRptsSpecified;
        #endregion EFS_String TotNumTrdRpts
        #region LastRptRequestedEnum LastRptReqed
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Report Requested")]
        [DictionaryGUI(Tag = "912", Anchor = "LastRptRequested")]
        public LastRptRequestedEnum LastRptReqed;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LastRptReqedSpecified;
        #endregion LastRptRequestedEnum LastRptReqed
        #region UnsolicitedIndicatorEnum Unsol
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unsolicited Indicator")]
        [DictionaryGUI(Tag = "325", Anchor = "UnsolicitedIndicator")]
        public UnsolicitedIndicatorEnum Unsol;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool UnsolSpecified;
        #endregion UnsolicitedIndicatorEnum Unsol
        #region SubscriptionRequestTypeEnum SubReqTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Subscription Request Type")]
        [DictionaryGUI(Tag = "263", Anchor = "SubscriptionRequestType")]
        public SubscriptionRequestTypeEnum SubReqTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SubReqTypSpecified;
        #endregion SubscriptionRequestTypeEnum SubReqTyp
        #region EFS_String RptRefID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptRefID
        {
            set { rptRefID = new EFS_String(value); }
            get { return rptRefID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RptRefIDSpecified
        {
            set { rptRefIDSpecified = value; }
            get { return rptRefIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Report Referenced ID")]
        [DictionaryGUI(Tag = "572", Anchor = "TradeReportRefID")]
        public EFS_String rptRefID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rptRefIDSpecified;
        #endregion EFS_String RptRefID
        #region EFS_String RptRefID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptRefID2
        {
            set { rptRefID2 = new EFS_String(value); }
            get { return rptRefID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RptRefID2Specified
        {
            set { rptRefID2Specified = value; }
            get { return rptRefID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Trade Report Referenced ID")]
        [DictionaryGUI(Tag = "881", Anchor = "SecondaryTradeReportRefID")]
        public EFS_String rptRefID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rptRefID2Specified;
        #endregion EFS_String RptRefID2
        #region EFS_String RptID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID2
        {
            set { rptID2 = new EFS_String(value); }
            get { return rptID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RptID2Specified
        {
            set { rptID2Specified = value; }
            get { return rptID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Trade Report ID")]
        [DictionaryGUI(Tag = "818", Anchor = "SecondaryTradeReportID")]
        public EFS_String rptID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rptID2Specified;
        #endregion EFS_String RptID2
        #region EFS_String LinkID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LinkID
        {
            set { linkID = new EFS_String(value); }
            get { return linkID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LinkIDSpecified
        {
            set { linkIDSpecified = value; }
            get { return linkIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Link ID")]
        [DictionaryGUI(Tag = "820", Anchor = "TradeLinkID")]
        public EFS_String linkID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool linkIDSpecified;
        #endregion EFS_String LinkID
        #region EFS_String MtchID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MtchID
        {
            set { mtchID = new EFS_String(value); }
            get { return mtchID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MtchIDSpecified
        {
            set { mtchIDSpecified = value; }
            get { return mtchIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Match ID")]
        [DictionaryGUI(Tag = "880", Anchor = "TrdMatchID")]
        public EFS_String mtchID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mtchIDSpecified;
        #endregion EFS_String MtchID
        #region EFS_String ExecID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecID
        {
            set { execID = new EFS_String(value); }
            get { return execID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecIDSpecified
        {
            set { execIDSpecified = value; }
            get { return execIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution ID")]
        [DictionaryGUI(Tag = "17", Anchor = "ExecID")]
        public EFS_String execID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool execIDSpecified;
        #endregion EFS_String ExecID
        #region OrdStatusEnum OrdStat
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Status")]
        [DictionaryGUI(Tag = "39", Anchor = "OrdStatus")]
        public OrdStatusEnum OrdStat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OrdStatSpecified;
        #endregion OrdStatusEnum OrdStat
        #region EFS_String ExecID2
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecID2
        {
            set { execID2 = new EFS_String(value); }
            get { return execID2?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecID2Specified
        {
            set { execID2Specified = value; }
            get { return execID2Specified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Execution ID")]
        [DictionaryGUI(Tag = "527", Anchor = "SecondaryExecID")]
        public EFS_String execID2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool execID2Specified;
        #endregion EFS_String ExecID2
        #region EFS_String ExecRstmtRsn
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecRstmtRsn
        {
            set { execRstmtRsn = new EFS_String(value); }
            get { return execRstmtRsn?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecRstmtRsnSpecified
        {
            set { execRstmtRsnSpecified = value; }
            get { return execRstmtRsnSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution Restatement Reason")]
        [DictionaryGUI(Tag = "378", Anchor = "ExecRestatementReason")]
        public EFS_String execRstmtRsn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool execRstmtRsnSpecified;
        #endregion EFS_String ExecRstmtRsn
        #region PreviouslyReportedEnum PrevlyRpted
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "PreviouslyReported")]
        [DictionaryGUI(Tag = "570", Anchor = "PreviouslyReported")]
        public PreviouslyReportedEnum PrevlyRpted;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PrevlyRptedSpecified;
        #endregion PreviouslyReportedEnum PrevlyRpted
        #region PriceTypeEnum PxTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price Type")]
        [DictionaryGUI(Tag = "423", Anchor = "PriceType")]
        public PriceTypeEnum PxTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PxTypSpecified;
        #endregion PriceTypeEnum PxTyp
        #region RootParties_Block[] Pty
        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Root Parties")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Root Party", IsChild = true, MinItem = 0)]
        public RootParties_Block[] Pty;
        #endregion RootParties_Block[] Pty
        #region AsOfIndicatorEnum AsOfInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "As Of Indicator")]
        [DictionaryGUI(Tag = "1015", Anchor = "AsOfIndicator")]
        public AsOfIndicatorEnum AsOfInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AsOfIndSpecified;
        #endregion AsOfIndicatorEnum AsOfInd
        #region SettlSessIDEnum SetSesID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Session ID")]
        [DictionaryGUI(Tag = "716", Anchor = "SettlSessID")]
        public SettlSessIDEnum SetSesID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SetSesIDSpecified;
        #endregion SettlSessIDEnum SetSesID
        #region EFS_String SetSesSub
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SetSesSub
        {
            set { setSesSub = new EFS_String(value); }
            get { return setSesSub?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SetSesSubSpecified
        {
            set { setSesSubSpecified = value; }
            get { return setSesSubSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Session Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "717", Anchor = "SettlSessSubID")]
        public EFS_String setSesSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool setSesSubSpecified;
        #endregion EFS_String SetSesSub
        #region Instrument_Block Instrmt
        [System.Xml.Serialization.XmlElementAttribute("Instrmt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Instrument", IsVisible = false)]
        public InstrumentBlock Instrmt;
        #endregion Instrument_Block Instrmt
        #region FinancingDetails_Block FinDetls
        [System.Xml.Serialization.XmlElementAttribute("FinDetls", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Instrument")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Financing Details")]
        public FinancingDetails_Block FinDetls;
        #endregion FinancingDetails_Block FinDetls
        #region OrderQtyData_Block OrdQty
        [System.Xml.Serialization.XmlElementAttribute("OrdQty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quantity")]
        public OrderQtyData_Block OrdQty;
        #endregion OrderQtyData_Block OrdQty
        #region QtyTypeEnum QtyTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity Type")]
        [DictionaryGUI(Tag = "854", Anchor = "QtyType")]
        public QtyTypeEnum QtyTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool QtyTypSpecified;
        #endregion QtyTypeEnum QtyTyp
        #region YieldData_Block Yield
        [System.Xml.Serialization.XmlElementAttribute("Yield", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Yields")]
        [DictionaryGUI(Page = "YieldData")]
        public YieldData_Block Yield;
        #endregion YieldData_Block Yield
        #region UndInstrmtGrp_Block[] Undly
        [System.Xml.Serialization.XmlElementAttribute("Undly", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying instruments")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying instrument", IsChild = true, MinItem = 0)]
        public UndInstrmtGrp_Block[] Undly;
        #endregion UndInstrmtGrp_Block[] Undly
        #region EFS_String UndSesID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UndSesID
        {
            set { undSesID = new EFS_String(value); }
            get { return undSesID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UndSesIDSpecified
        {
            set { undSesIDSpecified = value; }
            get { return undSesIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Trading Session ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "822", Anchor = "UnderlyingTradingSessionID")]
        public EFS_String undSesID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool undSesIDSpecified;
        #endregion EFS_String UndSesID
        #region EFS_String UndSesSub
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UndSesSub
        {
            set { undSesSub = new EFS_String(value); }
            get { return undSesSub?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UndSesSubSpecified
        {
            set { undSesSubSpecified = value; }
            get { return undSesSubSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Trading Session Sub ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "823", Anchor = "UnderlyingTradingSessionSubID")]
        public EFS_String undSesSub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool undSesSubSpecified;
        #endregion EFS_String UndSesSub
        #region EFS_Decimal LastQty
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "32", Anchor = "LastQty")]
        public EFS_Decimal lastQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastQtySpecified;
        #endregion EFS_Decimal LastQty
        #region EFS_Decimal LastPx
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "31", Anchor = "LastPx")]
        public EFS_Decimal lastPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastPxSpecified;
        #endregion EFS_Decimal LastPx
        #region EFS_Decimal CalcCcyLastQty
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CalcCcyLastQty
        {
            set { calcCcyLastQty = new EFS_Decimal(value); }
            get { return (null != calcCcyLastQty) ? calcCcyLastQty.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CalcCcyLastQtySpecified
        {
            set { calcCcyLastQtySpecified = value; }
            get { return calcCcyLastQtySpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculated Curency Last Quantity", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1056", Anchor = "CalculatedCcyLastQty")]
        public EFS_Decimal calcCcyLastQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calcCcyLastQtySpecified;
        #endregion EFS_Decimal CalcCcyLastQty
        #region string Ccy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [DictionaryGUI(Tag = "15", Anchor = "Currency")]
        public string Ccy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool CcySpecified;
        #endregion string Ccy
        #region string SettlCcy
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Currency")]
        [DictionaryGUI(Tag = "120", Anchor = "SettlCurrency")]
        public string SettlCcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlCcySpecified;
        #endregion string SettlCcy
        #region EFS_Decimal LastParPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastParPx
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Percent-of-par Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "669", Anchor = "LastParPx")]
        public EFS_Decimal lastParPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastParPxSpecified;
        #endregion EFS_Decimal LastParPx
        #region EFS_Decimal LastSpotRt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastSpotRt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Spot Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "194", Anchor = "LastSpotRate")]
        public EFS_Decimal lastSpotRt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastSpotRtSpecified;
        #endregion EFS_Decimal LastSpotRt
        #region EFS_Decimal LastFwdPnts
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastFwdPnts
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Forward Points", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "195", Anchor = "LastForwardPoints")]
        public EFS_Decimal lastFwdPnts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastFwdPntsSpecified;
        #endregion EFS_Decimal LastFwdPnts
        #region EFS_Decimal LastSwapPnts
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastSwapPnts
        {
            set { lastSwapPnts = new EFS_Decimal(value); }
            get { return (null != lastSwapPnts) ? lastSwapPnts.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastSwapPntsSpecified
        {
            set { lastSwapPntsSpecified = value; }
            get { return lastSwapPntsSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Swap Points", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1071", Anchor = "LastSwapPoints")]
        public EFS_Decimal lastSwapPnts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastSwapPntsSpecified;
        #endregion EFS_Decimal LastSwapPnts
        #region string LastMkt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Market", Width = 500)]
        [DictionaryGUI(Tag = "30", Anchor = "LastMkt")]
        public string LastMkt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool LastMktSpecified;
        #endregion string LastMkt
        #region EFS_Date TrdDt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdDt
        {
            set
            {
                trdDt = new EFS_Date(value);
            }
            get
            {
                return (null != trdDt) ? trdDt.Value : Convert.ToString(null);
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdDtSpecified
        {
            set { trdDtSpecified = value; }
            get { return trdDtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Date")]
        [DictionaryGUI(Tag = "75", Anchor = "TradeDate")]
        public EFS_Date trdDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trdDtSpecified;
        #endregion EFS_Date TrdDt
        #region EFS_Date BizDt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BizDt
        {
            set
            {
                bizDt = new EFS_Date(value); 
            }
            get
            {
                return ((null != bizDt) ? bizDt.Value : Convert.ToString(null) );
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BizDtSpecified
        {
            set { bizDtSpecified = value; }
            get { return bizDtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clearing Business Date")]
        [DictionaryGUI(Tag = "715", Anchor = "ClearingBusinessDate")]
        public EFS_Date bizDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool bizDtSpecified;
        #endregion EFS_Date BizDt
        #region EFS_Decimal AvgPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal AvgPx
        {
            set { avgPx = new EFS_Decimal(value); }
            get { return (null != avgPx) ? avgPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AvgPxSpecified
        {
            set { avgPxSpecified = value; }
            get { return avgPxSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Average Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "6", Anchor = "AvgPx")]
        public EFS_Decimal avgPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool avgPxSpecified;
        #endregion EFS_Decimal AvgPx
        #region SpreadOrBenchmarkCurveData_Block SprdBnchmkCurve
        [System.Xml.Serialization.XmlElementAttribute("SprdBnchmkCurve", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Associated price curves")]
        public SpreadOrBenchmarkCurveData_Block SprdBnchmkCurve;
        #endregion SpreadOrBenchmarkCurveData_Block SprdBnchmkCurve
        #region AvgPxIndicatorEnum AvgPxInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Average Price Indicator")]
        [DictionaryGUI(Tag = "819", Anchor = "AvgPxIndicator")]
        public AvgPxIndicatorEnum AvgPxInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool AvgPxIndSpecified;
        #endregion AvgPxIndicatorEnum AvgPxInd
        #region PositionAmountData_Block[] Amt
        [System.Xml.Serialization.XmlElementAttribute("Amt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Amounts")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Amount", IsChild = true, IsChildVisible = true, MinItem = 0)]
        public PositionAmountData_Block[] Amt;
        #endregion PositionAmountData_Block[] Amt
        #region MultiLegReportingTypeEnum MLegRptTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multi-Leg Reporting Type")]
        [DictionaryGUI(Tag = "442", Anchor = "MultiLegReportingType")]
        public MultiLegReportingTypeEnum MLegRptTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MLegRptTypSpecified;
        #endregion MultiLegReportingTypeEnum MLegRptTyp
        #region EFS_String TrdLegRefID
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdLegRefID
        {
            set { trdLegRefID = new EFS_String(value); }
            get { return trdLegRefID?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdLegRefIDSpecified
        {
            set { trdLegRefIDSpecified = value; }
            get { return trdLegRefIDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Leg Reference ID", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "824", Anchor = "TradeLegRefID")]
        public EFS_String trdLegRefID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trdLegRefIDSpecified;
        #endregion EFS_String TrdLegRefID

        #region TrdInstrmtLegGrp_Block[] TrdLeg
        [System.Xml.Serialization.XmlElementAttribute("TrdLeg", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Leg")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Leg", IsChild = true, MinItem = 0)]
        public TrdInstrmtLegGrp_Block[] TrdLeg;
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20120625 [17864] add TrdLegSpecified
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdLegSpecified;

        #endregion TrdInstrmtLegGrp_Block[] TrdLeg
        #region EFS_DateTime TxnTm
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TxnTm
        {
            set { txnTm = new EFS_DateTimeOffset(value); }
            get { return TxnTmSpecified ? txnTm.ISODateTimeValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TxnTmSpecified
        {
            set { txnTmSpecified = value; }
            get { return txnTmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool txnTmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transaction time", LblWidth = 130)]
        [DictionaryGUI(Tag = "60", Anchor = "TransacTime")]
        public EFS_DateTimeOffset txnTm;
        #endregion EFS_DateTime TxnTm
        #region TrdRegTimestamps_Block[] TrdRegTS
        [System.Xml.Serialization.XmlElementAttribute("TrdRegTS", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Timestamps")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Timestamp", IsChild = true, MinItem = 0)]
        public TrdRegTimestamps_Block[] TrdRegTS;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdRegTSSpecified;

        #endregion TrdRegTimestamps_Block[] TrdRegTS
        #region SettlTypeEnum SettlTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Type")]
        [DictionaryGUI(Tag = "63", Anchor = "SettlType")]
        public SettlTypeEnum SettlTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool SettlTypSpecified;
        #endregion SettlTypeEnum SettlTyp
        #region EFS_Date SettlDt
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime SettlDt
        {
            set {
                settlDt = new EFS_Date
                {
                    DateValue = value
                };
            }
            get { return (null != settlDt) ? settlDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDtSpecified
        {
            set { settlDtSpecified = value; }
            get { return settlDtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Date")]
        [DictionaryGUI(Tag = "64", Anchor = "SettlDate")]
        public EFS_Date settlDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlDtSpecified;
        #endregion EFS_Date SettlDt
        #region EFS_Date StlDt
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime StlDt
        {
            set {
                stlDt = new EFS_Date
                {
                    DateValue = value
                };
            }
            get { return (null != stlDt) ? stlDt.DateValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StlDtSpecified
        {
            set { stlDtSpecified = value; }
            get { return stlDtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Settlement Date")]
        [DictionaryGUI(Tag = "987", Anchor = "UnderlyingSettlementDate")]
        public EFS_Date stlDt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stlDtSpecified;
        #endregion EFS_Date StlDt
        #region MatchStatusEnum MtchStat
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Match Status")]
        [DictionaryGUI(Tag = "573", Anchor = "MatchStatus")]
        public MatchStatusEnum MtchStat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MtchStatSpecified;
        #endregion MatchStatusEnum MtchStat
        #region MatchTypeEnum MtchTyp
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Match Type")]
        [DictionaryGUI(Tag = "574", Anchor = "MatchType")]
        public MatchTypeEnum MtchTyp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool MtchTypSpecified;
        #endregion MatchTypeEnum MtchTyp
        #region OrderCategoryEnum OrdCat
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order Category")]
        [DictionaryGUI(Tag = "1115", Anchor = "OrderCategory")]
        public OrderCategoryEnum OrdCat;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool OrdCatSpecified;
        #endregion OrderCategoryEnum OrdCat
        #region TrdCapRptSideGrp_Block[] RptSide
        [System.Xml.Serialization.XmlElementAttribute("RptSide", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Mandatory)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Sides")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Side", IsMaster = true, IsChild = true, MinItem = 1)]
        public TrdCapRptSideGrp_Block[] RptSide;
        #endregion TrdCapRptSideGrp_Block[] RptSide
        #region EFS_Decimal Vol
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Vol
        {
            set { vol = new EFS_Decimal(value); }
            get { return (null != vol) ? vol.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VolSpecified
        {
            set { volSpecified = value; }
            get { return volSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Volatility", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1188", Anchor = "Volatility")]
        public EFS_Decimal vol;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool volSpecified;
        #endregion EFS_Decimal Vol
        #region EFS_Decimal DividendYield
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal DividendYield
        {
            set { dividendYield = new EFS_Decimal(value); }
            get { return (null != dividendYield) ? dividendYield.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DividendYieldSpecified
        {
            set { dividendYieldSpecified = value; }
            get { return dividendYieldSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend Yield", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1380", Anchor = "DividendYield")]
        public EFS_Decimal dividendYield;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendYieldSpecified;
        #endregion EFS_Decimal DividendYield
        #region EFS_Decimal RFR
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal RFR
        {
            set { rFR = new EFS_Decimal(value); }
            get { return (null != rFR) ? rFR.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RFRSpecified
        {
            set { rFRSpecified = value; }
            get { return rFRSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Risk Free Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1190", Anchor = "RiskFreeRate")]
        public EFS_Decimal rFR;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rFRSpecified;
        #endregion EFS_Decimal RFR
        #region EFS_Decimal CurrencyRatio
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CurrencyRatio
        {
            set { currencyRatio = new EFS_Decimal(value); }
            get { return (null != currencyRatio) ? currencyRatio.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CurrencyRatioSpecified
        {
            set { currencyRatioSpecified = value; }
            get { return currencyRatioSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency Ratio", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1382", Anchor = "CurrencyRatio")]
        public EFS_Decimal currencyRatio;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencyRatioSpecified;
        #endregion EFS_Decimal CurrencyRatio
        #region EFS_String CopyMsgInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CopyMsgInd
        {
            set { copyMsgInd = new EFS_String(value); }
            get { return copyMsgInd?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CopyMsgIndSpecified
        {
            set { copyMsgIndSpecified = value; }
            get { return copyMsgIndSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Drop Copy Message Indicator", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "797", Anchor = "CopyMsgIndicator")]
        public EFS_String copyMsgInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool copyMsgIndSpecified;
        #endregion EFS_String CopyMsgInd
        #region TrdRepIndicatorsGrp_Block[]
        [System.Xml.Serialization.XmlElementAttribute("TrdRepIndicatorsGrp", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Indicators")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Indicator", IsChild = true, MinItem = 0)]
        public TrdRepIndicatorsGrp_Block[] TrdRepIndicatorsGrp;
        #endregion TrdRepIndicatorsGrp_Block[]
        #region PublishTrdIndicatorEnum PubTrdInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Publish Trade Indicator")]
        [DictionaryGUI(Tag = "852", Anchor = "PublishTrdIndicator")]
        public PublishTrdIndicatorEnum PubTrdInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool PubTrdIndSpecified;
        #endregion PublishTrdIndicatorEnum PubTrdInd
        #region TradePublishIndicatorEnum TrdPubInd
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Publish Indicator")]
        [DictionaryGUI(Tag = "1390", Anchor = "TradePublishIndicator")]
        public TradePublishIndicatorEnum TrdPubInd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool TrdPubIndSpecified;
        #endregion TradePublishIndicatorEnum TrdPubInd
        #region ShortSaleReasonEnum ShrtSaleRsn
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Short Sale Reason")]
        [DictionaryGUI(Tag = "853", Anchor = "ShortSaleReason")]
        public ShortSaleReasonEnum ShrtSaleRsn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ShrtSaleRsnSpecified;
        #endregion ShortSaleReasonEnum ShrtSaleRsn
        #region EFS_String TierCD
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TierCD
        {
            set { tierCD = new EFS_String(value); }
            get { return tierCD?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TierCDSpecified
        {
            set { tierCDSpecified = value; }
            get { return tierCDSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Tier Code", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "994", Anchor = "TierCode")]
        public EFS_String tierCD;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tierCDSpecified;
        #endregion EFS_String TierCD
        #region EFS_String MsgEvtSrc
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MsgEvtSrc
        {
            set { msgEvtSrc = new EFS_String(value); }
            get { return msgEvtSrc?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MsgEvtSrcSpecified
        {
            set { msgEvtSrcSpecified = value; }
            get { return msgEvtSrcSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Message Event Source", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1011", Anchor = "MessageEventSource")]
        public EFS_String msgEvtSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool msgEvtSrcSpecified;
        #endregion EFS_String MsgEvtSrc
        #region EFS_DateTime LastUpdateTm
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LastUpdateTm
        {
            set { lastUpdateTm = new EFS_DateTimeOffset(value); }
            get { return LastUpdateTmSpecified ? lastUpdateTm.ISODateTimeValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastUpdateTmSpecified
        {
            set { lastUpdateTmSpecified = value; }
            get { return lastUpdateTmSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastUpdateTmSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Update time", LblWidth = 130)]
        [DictionaryGUI(Tag = "779", Anchor = "LastUpdateTime")]
        public EFS_DateTimeOffset lastUpdateTm;

        #endregion EFS_DateTime LastUpdateTm
        #region EFS_Decimal RndPx
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal RndPx
        {
            set { rndPx = new EFS_Decimal(value); }
            get { return (null != rndPx) ? rndPx.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RndPxSpecified
        {
            set { rndPxSpecified = value; }
            get { return rndPxSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rounded Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "991", Anchor = "RndPx")]
        public EFS_Decimal rndPx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rndPxSpecified;
        #endregion EFS_Decimal RndPx
        #region EFS_DateTime TZTransactTime
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DateTime TZTransactTime
        {
            set { tZTransactTime = new EFS_DateTime(value.ToString(Cst.FixML_DateTimeFmt)); }
            get { return (null != tZTransactTime) ? tZTransactTime.DateTimeValue : Convert.ToDateTime(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TZTransactTimeSpecified
        {
            set { tZTransactTimeSpecified = value; }
            get { return tZTransactTimeSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time Zone Transaction Time")]
        [DictionaryGUI(Tag = "1132", Anchor = "TZTransactTime")]
        public EFS_DateTime tZTransactTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tZTransactTimeSpecified;
        #endregion EFS_DateTime TZTransactTime
        #region YesNoEnum ReportedPxDiff
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reported Price Difference")]
        [DictionaryGUI(Tag = "1134", Anchor = "ReportedPxDiff")]
        public YesNoEnum ReportedPxDiff;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ReportedPxDiffSpecified;
        #endregion YesNoEnum ReportedPxDiff
        #region EFS_Decimal GrossTrdAmt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal GrossTrdAmt
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Gross Trade Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "381", Anchor = "GrossTradeAmt")]
        public EFS_Decimal grossTrdAmt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool grossTrdAmtSpecified;
        #endregion EFS_Decimal GrossTrdAmt
        #region EFS_String RejTxt
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RejTxt
        {
            set { rejTxt = new EFS_String(value); }
            get { return rejTxt?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RejTxtSpecified
        {
            set { rejTxtSpecified = value; }
            get { return rejTxtSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reject Text", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1328", Anchor = "RejectText")]
        public EFS_String rejTxt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rejTxtSpecified;
        #endregion EFS_String RejTxt
        #region EFS_Decimal FeeMult
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal FeeMult
        {
            set { feeMult = new EFS_Decimal(value); }
            get { return (null != feeMult) ? feeMult.DecValue : Convert.ToDecimal(null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FeeMultSpecified
        {
            set { feeMultSpecified = value; }
            get { return feeMultSpecified; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Multiplier", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [DictionaryGUI(Tag = "1329", Anchor = "FeeMultiplier")]
        public EFS_Decimal feeMult;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool feeMultSpecified;
        #endregion EFS_Decimal FeeMult

        // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
        [System.Xml.Serialization.XmlElementAttribute("RegTrdID", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Regulatory Trade IDs")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Regulatory Trade ID", IsChild = true, MinItem = 0)]
        public RegulatoryTradeIDGrp_Block[] RegTrdID;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool RegTrdIDSpecified;

        #endregion Members
    }
    #endregion TradeCaptureReport_message
    #region TradeCaptureReportAck_message
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    [System.Xml.Serialization.XmlRootAttribute("TrdCaptRptAck", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", IsNullable = false)]
    public partial class TradeCaptureReportAck_message : Abstract_message
    {

        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        public RootParties_Block[] Pty;

        [System.Xml.Serialization.XmlElementAttribute("Instrmt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        public InstrumentBlock Instrmt;

        [System.Xml.Serialization.XmlElementAttribute("Undly", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        public UndInstrmtGrp_Block[] Undly;

        [System.Xml.Serialization.XmlElementAttribute("TrdRepIndicatorsGrp", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 4)]
        public TrdRepIndicatorsGrp_Block[] TrdRepIndicatorsGrp;

        [System.Xml.Serialization.XmlElementAttribute("TrdLeg", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 5)]
        public TrdInstrmtLegGrp_Block[] TrdLeg;

        [System.Xml.Serialization.XmlElementAttribute("TrdRegTS", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 6)]
        public TrdRegTimestamps_Block[] TrdRegTS;

        [System.Xml.Serialization.XmlElementAttribute("Amt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 7)]
        public PositionAmountData_Block[] Amt;

        [System.Xml.Serialization.XmlElementAttribute("RptSide", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 8)]
        public TrdCapRptAckSideGrp_Block[] RptSide;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FirmTrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FirmTrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeReportTransTypeEnum TransTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TransTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeReportTypeEnum RptTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RptTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdTyp;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdSubTyp;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SecondaryTrdTypeEnum TrdTyp2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdTyp2Specified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeHandlingInstrEnum TrdHandlInst;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdHandlInstSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeHandlingInstrEnum OrigTrdHandlInst;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrigTrdHandlInstSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime OrigTrdDt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrigTrdDtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrigTrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrignTrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrnsfrRsn;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ExecTypeEnum ExecTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptRefID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptRefID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TrdRptStatusEnum TrdRptStat;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdRptStatSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RejRsn;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SubscriptionRequestTypeEnum SubReqTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubReqTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LinkID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MtchID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public OrdStatusEnum OrdStat;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrdStatSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecRstmtRsn;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PreviouslyReportedEnum PrevlyRpted;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PrevlyRptedSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PriceTypeEnum PxTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PxTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UndSesID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UndSesSub;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SettlSessIDEnum SetSesID;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SetSesIDSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SetSesSub;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public QtyTypeEnum QtyTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QtyTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastQty;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastQtySpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastPx;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastPxSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastParPx;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastParPxSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CalcCcyLastQty;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CalcCcyLastQtySpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastSwapPnts;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastSwapPntsSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Ccy;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SettlCcy;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastSpotRt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastSpotRtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal LastFwdPnts;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastFwdPntsSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LastMkt;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime TrdDt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdDtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime BizDt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BizDtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal AvgPx;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AvgPxSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AvgPxIndicatorEnum AvgPxInd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AvgPxIndSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public MultiLegReportingTypeEnum MLegRptTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MLegRptTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdLegRefID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TxnTm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TxnTmSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SettlTyp;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public MatchStatusEnum MtchStat;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MtchStatSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public MatchTypeEnum MtchTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MtchTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CopyMsgInd;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PublishTrdIndicatorEnum PubTrdInd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PubTrdIndSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradePublishIndicatorEnum TrdPubInd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdPubIndSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ShortSaleReasonEnum ShrtSaleRsn;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ShrtSaleRsnSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ResponseTransportTypeEnum RspTransportTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RspTransportTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RspDest;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AsOfIndicatorEnum AsOfInd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AsOfIndSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ClearingFeeIndicatorEnum ClrFeeInd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ClrFeeIndSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TierCD;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MsgEvtSrc;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime LastUpdateTm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastUpdateTmSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal RndPx;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RndPxSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptSys;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal GrossTrdAmt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GrossTrdAmtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime SettlDt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SettlDtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal FeeMult;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FeeMultSpecified;
    }
    #endregion TradeCaptureReportAck_message
    #region TradeCaptureReportRequest_message
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    [System.Xml.Serialization.XmlRootAttribute("TrdCaptRptReq", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", IsNullable = false)]
    public partial class TradeCaptureReportRequest_message : Abstract_message
    {

        [System.Xml.Serialization.XmlElementAttribute("Pty", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        public Parties_Block[] Pty;

        [System.Xml.Serialization.XmlElementAttribute("Instrmt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        public InstrumentBlock Instrmt;

        [System.Xml.Serialization.XmlElementAttribute("InstrmtExt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        public InstrumentExtension_Block InstrmtExt;

        [System.Xml.Serialization.XmlElementAttribute("FinDetls", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 4)]
        public FinancingDetails_Block FinDetls;

        [System.Xml.Serialization.XmlElementAttribute("Undly", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 5)]
        public UndInstrmtGrp_Block[] Undly;

        [System.Xml.Serialization.XmlElementAttribute("Leg", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 6)]
        public InstrmtLegGrp_Block[] Leg;

        [System.Xml.Serialization.XmlElementAttribute("TrdCapDt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 7)]
        public TrdCapDtGrp_Block[] TrdCapDt;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FirmTrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FirmTrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeRequestTypeEnum ReqTyp;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SubscriptionRequestTypeEnum SubReqTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubReqTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RptID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExecID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ExecTypeEnum ExecTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClOrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public MatchStatusEnum MtchStat;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MtchStatSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdTyp;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdSubTyp;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeHandlingInstrEnum TrdHandlInst;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdHandlInstSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrnsfrRsn;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SecondaryTrdTypeEnum TrdTyp2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TrdTyp2Specified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LinkID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MtchID;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime BizDt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BizDtSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SesSub;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TmBkt;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SideEnum Side;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SideSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public MultiLegReportingTypeEnum MLegRptTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MLegRptTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InptSrc;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InptDev;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ResponseTransportTypeEnum RspTransportTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RspTransportTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RspDest;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MsgEvtSrc;
    }
    #endregion TradeCaptureReportRequest_message
    #region TradeCaptureReportRequestAck_message
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    [System.Xml.Serialization.XmlRootAttribute("TrdCaptRptReqAck", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", IsNullable = false)]
    public partial class TradeCaptureReportRequestAck_message : Abstract_message
    {

        [System.Xml.Serialization.XmlElementAttribute("Instrmt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 1)]
        public InstrumentBlock Instrmt;

        [System.Xml.Serialization.XmlElementAttribute("Undly", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        public UndInstrmtGrp_Block[] Undly;

        [System.Xml.Serialization.XmlElementAttribute("Leg", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        public InstrmtLegGrp_Block[] Leg;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FirmTrdID;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FirmTrdID2;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeRequestTypeEnum ReqTyp;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SubscriptionRequestTypeEnum SubReqTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubReqTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string TotNumTrdRpts;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReqRslt;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeRequestStatusEnum ReqStat;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public MultiLegReportingTypeEnum MLegRptTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MLegRptTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ResponseTransportTypeEnum RspTransportTyp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RspTransportTypSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RspDest;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Txt;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        public string EncTxtLen;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EncTxt;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MsgEvtSrc;
    }
    #endregion TradeCaptureReportRequestAck_message
    #endregion Message
}
