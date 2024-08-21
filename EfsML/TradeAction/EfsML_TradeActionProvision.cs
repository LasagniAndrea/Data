#region Debug Directives
//      ---------------------------------------------------------------------------------------------------------------------
//      EfsML_TradeActionProvision : Contient l'ensemble des classe utilisées par les actions diverses liées aux provisions
//      ---------------------------------------------------------------------------------------------------------------------
//      * CurrentTradeAction                       : Cette classe est partialisée, stocke l'événement de base en cours de traitement
//                                                   vous y trouvez ici les Accesseurs/méthodes propres aux provisions
//      * TradeActionEvent                         : Cette classe est partialisée, vous y trouvez ici les Accesseurs/méthodes propres 
//                                                   aux provisions
//      Classes Action
//      * CancelableProvisionEvents                : Résilitation avec Commission optionnelle
//      * ExtendibleProvisionEvents                : Prorogation
//      * MandatoryEarlyTerminationProvisionEvents : Résilitation obligatoire avec CashSettlement
//      * OptionalEarlyTerminationProvisionEvents  : Résilitation optionnelle avec CashSettlement
//      * ProvisionEvents                          : classe commune à toutes les provisions
//      * StepUpProvisionEvents                    : Augmentation de capital

//      Classes de travail
//      * CashSettlementAmountProvision            : CashSettlement sur provisions
//      * FeeAmountInfo et FeeAmountProvision      : Commissions sur provisions
//      * PartyInfoProvision                       : Parties sur Provsions
//      * NotionalAmountProvision                  : Notionnel sur provisions

#endregion Debug Directives

#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives
namespace EfsML
{
    #region CancelableProvisionEvents
    public class CancelableProvisionEvents : ProvisionEvents
    {
        #region Members
        ICancelableProvision cancelableProvision;
        #endregion Members
        #region Accessors
        #region Resource in Form
        #region ResFormValueDate
        protected override string ResFormValueDate { get { return Ressource.GetString("ProvisionRelevantUnderlyingDate"); } }
        #endregion ResFormValueDate
        #endregion Resource in Form
        #endregion Accessors

        #region Override Accessors
        // EG 20150428 [20513] New
        protected override bool IsImplicitProvision
        {
            get
            {
                IProduct product = (IProduct)CurrentProduct;
                return product.ProductBase.ImplicitProvisionSpecified && product.ProductBase.ImplicitCancelableProvisionSpecified;
            }
        }
        // EG 20150428 [20513] New
        protected override IAmericanExercise AmericanExerciseProvision
        {
            get { return cancelableProvision.American;}
        }
        // EG 20150428 [20513] New
        protected override IEuropeanExercise EuropeanExerciseProvision
        {
            get { return cancelableProvision.European; }
        }
        // EG 20150428 [20513] New
        protected override IBermudaExercise BermudaExerciseProvision
        {
            get { return cancelableProvision.Bermuda; }
        }
        // EG 20150428 [20513] New
        public override IBusinessCenterTime ExpirationTimeProvision
        {
            get
            {
                IBusinessCenterTime expirationTime = null;
                if (cancelableProvision.AmericanSpecified)
                    expirationTime = cancelableProvision.American.ExpirationTime;
                else if (cancelableProvision.BermudaSpecified)
                    expirationTime = cancelableProvision.Bermuda.ExpirationTime;
                else if (cancelableProvision.EuropeanSpecified)
                    expirationTime = cancelableProvision.European.ExpirationTime;
                return expirationTime;
            }
        }
        #endregion Override Accessors

        #region Constructors
        public CancelableProvisionEvents() { }
        public CancelableProvisionEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent) : base(pEvent, pEventParent) 
        {
            // EG 20150605 [21087] Remove Setting with SetProvision()
            //cancelableProvision = ((IProduct)CurrentProduct).productBase.cancelableProvision;
        }
        #endregion Constructors
        #region Methods
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            ProvisionElementsToPostMessage();
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.CancelableProvision) + "_" + m_Event.eventCode;
            return new CancelableProvisionMsg(idE, ActionDateTime, m_ExerciseType, ValueDate.DateValue, m_PostNotionalProvisionMsg, m_PostFeeProvisionMsg, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
                isOk = base.Save(pPage);
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region SetProvision
        // EG 20150605 [21087] New
        public override void SetProvision()
        {
            cancelableProvision = ((IProduct)CurrentProduct).ProductBase.CancelableProvision;
        }
        #endregion SetProvision

        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            return base.ValidationRules(pPage);
        }
        #endregion ValidationRules
        #endregion Methods
    }
    #endregion CancelableProvisionEvents
    #region ExtendibleProvisionEvents
    public class ExtendibleProvisionEvents : ProvisionEvents
    {
        #region Members
        IExtendibleProvision extendibleProvision;
        #endregion Members

        #region Override Accessors
        // EG 20150428 [20513] New
        protected override bool IsImplicitProvision
        {
            get
            {
                IProduct product = (IProduct)CurrentProduct;
                return product.ProductBase.ImplicitProvisionSpecified && product.ProductBase.ImplicitExtendibleProvisionSpecified;
            }
        }
        // EG 20150428 [20513] New
        protected override IAmericanExercise AmericanExerciseProvision
        {
            get { return extendibleProvision.American; }
        }
        // EG 20150428 [20513] New
        protected override IEuropeanExercise EuropeanExerciseProvision
        {
            get { return extendibleProvision.European; }
        }
        // EG 20150428 [20513] New
        protected override IBermudaExercise BermudaExerciseProvision
        {
            get { return extendibleProvision.Bermuda; }
        }
        // EG 20150428 [20513] New
        public override IBusinessCenterTime ExpirationTimeProvision
        {
            get
            {
                IBusinessCenterTime expirationTime = null;
                if (extendibleProvision.AmericanSpecified)
                    expirationTime = extendibleProvision.American.ExpirationTime;
                else if (extendibleProvision.BermudaSpecified)
                    expirationTime = extendibleProvision.Bermuda.ExpirationTime;
                else if (extendibleProvision.EuropeanSpecified)
                    expirationTime = extendibleProvision.European.ExpirationTime;
                return expirationTime;
            }
        }
        #endregion Override Accessors

        #region Constructors
        public ExtendibleProvisionEvents() { }
        public ExtendibleProvisionEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent) : base(pEvent, pEventParent) 
        {
            // EG 20150605 [21087] Remove Setting with SetProvision()
            //extendibleProvision = ((IProduct)CurrentProduct).productBase.extendibleProvision;
        }
        #endregion Constructors
        #region Methods
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            ProvisionElementsToPostMessage();
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.ExtendibleProvision) + "_" + m_Event.eventCode;
            return new ExtendibleProvisionMsg(idE, ActionDateTime, m_ExerciseType, ValueDate.DateValue, m_PostNotionalProvisionMsg, m_PostFeeProvisionMsg, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
                isOk = base.Save(pPage);
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region SetProvision
        // EG 20150605 [21087] New
        public override void SetProvision()
        {
            extendibleProvision = ((IProduct)CurrentProduct).ProductBase.ExtendibleProvision;
        }
        #endregion SetProvision

        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            return base.ValidationRules(pPage);
        }
        #endregion ValidationRules
        #endregion Methods

    }
    #endregion ExtendibleProvisionEvents
    #region MandatoryEarlyTerminationProvisionEvents
    public class MandatoryEarlyTerminationProvisionEvents : ProvisionEvents
    {
        #region Members
        IMandatoryEarlyTermination mandatoryEarlyTerminationProvision;
        #endregion Members

        #region Override Accessors
        // EG 20150428 [20513] New
        protected override bool IsImplicitProvision
        {
            get
            {
                IProduct product = (IProduct)CurrentProduct;
                return product.ProductBase.ImplicitProvisionSpecified && product.ProductBase.ImplicitMandatoryEarlyTerminationProvisionSpecified;
            }
        }
        // EG 20150428 [20513] New
        protected override bool IsMandatoryEarlyTermination { get { return true; } }
        // EG 20150428 [20513] New
        protected override ICashSettlement CashSettlementProvision
        {
            get { return mandatoryEarlyTerminationProvision.CashSettlement; }
        }

        #endregion Override Accessors

        #region Constructors
        public MandatoryEarlyTerminationProvisionEvents() { }
        public MandatoryEarlyTerminationProvisionEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent) : base(pEvent, pEventParent) 
        {
            // EG 20150605 [21087] Remove Setting with SetProvision()
            // mandatoryEarlyTerminationProvision = ((IProduct)CurrentProduct).productBase.mandatoryEarlyTerminationProvision;
        }
        #endregion Constructors
        #region Methods
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            ProvisionElementsToPostMessage();
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.MandatoryEarlyTerminationProvision) + "_" + m_Event.eventCode;
            return new MandatoryEarlyTerminationProvisionMsg(idE, ActionDateTime, m_ExerciseType, ValueDate.DateValue,
                m_PostNotionalProvisionMsg, m_PostCashSettlementProvisionMsg, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
                isOk = base.Save(pPage);
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region SetProvision
        // EG 20150605 [21087] New
        public override void SetProvision()
        {
            mandatoryEarlyTerminationProvision = ((IProduct)CurrentProduct).ProductBase.MandatoryEarlyTerminationProvision;
        }
        #endregion SetProvision

        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            return base.ValidationRules(pPage);
        }
        #endregion ValidationRules
        #endregion Methods
    }
    #endregion MandatoryEarlyTerminationProvisionEvents
    #region OptionalEarlyTerminationProvisionEvents
    public class OptionalEarlyTerminationProvisionEvents : ProvisionEvents
    {
        #region Members
        IOptionalEarlyTermination optionalEarlyTerminationProvision;
        #endregion Members

        #region Override Accessors
        // EG 20150428 [20513] New
        protected override bool IsImplicitProvision
        {
            get
            {
                IProduct product = (IProduct)CurrentProduct;
                return product.ProductBase.ImplicitProvisionSpecified && product.ProductBase.ImplicitOptionalEarlyTerminationProvisionSpecified;
            }
        }
        // EG 20150428 [20513] New
        protected override IAmericanExercise AmericanExerciseProvision
        {
            get { return optionalEarlyTerminationProvision.American; }
        }
        // EG 20150428 [20513] New
        protected override IEuropeanExercise EuropeanExerciseProvision
        {
            get { return optionalEarlyTerminationProvision.European; }
        }
        // EG 20150428 [20513] New
        protected override IBermudaExercise BermudaExerciseProvision
        {
            get { return optionalEarlyTerminationProvision.Bermuda; }
        }
        // EG 20150428 [20513] New
        protected override ICashSettlement CashSettlementProvision
        {
            get { return optionalEarlyTerminationProvision.CashSettlement; }
        }
        // EG 20150428 [20513] New
        public override IBusinessCenterTime ExpirationTimeProvision
        {
            get
            {
                IBusinessCenterTime expirationTime = null;
                if (optionalEarlyTerminationProvision.AmericanSpecified)
                    expirationTime = optionalEarlyTerminationProvision.American.ExpirationTime;
                else if (optionalEarlyTerminationProvision.BermudaSpecified)
                    expirationTime = optionalEarlyTerminationProvision.Bermuda.ExpirationTime;
                else if (optionalEarlyTerminationProvision.EuropeanSpecified)
                    expirationTime = optionalEarlyTerminationProvision.European.ExpirationTime;
                return expirationTime;
            }
        }
        #endregion Override Accessors

        #region Constructors
        public OptionalEarlyTerminationProvisionEvents() { }
        public OptionalEarlyTerminationProvisionEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent) : base(pEvent, pEventParent) 
        {
            // EG 20150605 [21087] Remove Setting with SetProvision()
            //optionalEarlyTerminationProvision = ((IProduct)CurrentProduct).productBase.optionalEarlyTerminationProvision;
        }
        #endregion Constructors

        #region Methods
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            ProvisionElementsToPostMessage();
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.OptionalEarlyTerminationProvision) + "_" + m_Event.eventCode;
            return new OptionalEarlyTerminationProvisionMsg(idE, ActionDateTime, m_ExerciseType, ValueDate.DateValue,
                m_PostNotionalProvisionMsg, m_PostFeeProvisionMsg, m_PostCashSettlementProvisionMsg, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
                isOk = base.Save(pPage);
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region SetProvision
        // EG 20150605 [21087] New
        public override void SetProvision()
        {
            optionalEarlyTerminationProvision = ((IProduct)CurrentProduct).ProductBase.OptionalEarlyTerminationProvision;
        }
        #endregion SetProvision

        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            return base.ValidationRules(pPage);
        }
        #endregion ValidationRules
        #endregion Methods
    }
    #endregion OptionalEarlyTerminationProvisionEvents
    #region StepUpProvisionEvents
    public class StepUpProvisionEvents : ProvisionEvents
    {
        #region Members
        // EG 20160404 Migration vs2013
        //IStepUpProvision stepUpProvision;
        #endregion Members

        #region Accessors
        #region Resource in Form
        #region ResFormProvisionNotionalAmounts
        protected override string ResFormTitleNotionalAmounts { get { return Ressource.GetString("ProvisionStepUpNotionalAmounts"); } }
        #endregion ResFormProvisionNotionalAmounts
        #region ResFormProvisionNotionalAmountsDispo
        protected override string ResFormTitleNotionalAmountsDispo { get { return Ressource.GetString("ProvisionStepUpNewAmounts"); } }
        #endregion ResFormProvisionNotionalAmountsDispo
        #region ResFormValueDate
        protected override string ResFormValueDate { get { return Ressource.GetString("ProvisionDate"); } }
        #endregion ResFormValueDate
        #endregion Resource in Form
        #endregion Accessors

        #region Override Accessors
        // EG 20150428 [20513] New
        protected override bool IsImplicitProvision
        {
            get
            {
                IProduct product = (IProduct)CurrentProduct;
                return product.ProductBase.ImplicitProvisionSpecified && product.ProductBase.ImplicitProvision.StepUpProvisionSpecified;
            }
        }
        #endregion Override Accessors

        #region Constructors
        public StepUpProvisionEvents() { }
        public StepUpProvisionEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            //stepUpProvision = ((IProduct)CurrentProduct).productBase.implicitProvision.stepUpProvision;
        }
        #endregion Constructors
        #region Methods
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            ProvisionElementsToPostMessage();
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.StepUpProvision) + "_" + m_Event.eventCode;
            return new StepUpProvisionMsg(idE, ActionDateTime, m_ExerciseType, ValueDate.DateValue, m_PostNotionalProvisionMsg, m_PostFeeProvisionMsg, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
                isOk = base.Save(pPage);
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            return base.ValidationRules(pPage);
        }
        #endregion ValidationRules
        #endregion Methods
    }
    #endregion StepUpProvisionEvents

    #region ProvisionEvents
    // 20080416 EG Ticket 16173
    public abstract class ProvisionEvents : ActionEvents
    {
        #region Members
        protected string m_ExerciseType;
        // EG 20150605 [21087] Change Type of m_LstExerciseType
        //protected List<Pair<string,string>> m_LstExerciseType;
        protected Dictionary<string, string> m_LstExerciseType;
        protected TradeActionEvent[] m_ExerciseDates;
        protected IBusinessCenterTime m_ExpirationTime;
        protected NotionalAmountProvision[] m_NotionalAmountProvision;
        protected NotionalAmountProvision[] m_NotionalAmountProvisionWork;
        protected FeeAmountProvision[] m_FeeAmountProvision;
        protected FeeAmountProvision[] m_FeeAmountProvisionWork;
        protected Dictionary<string, string> m_LstExerciseDate;
        protected IPartialExercise m_PartialExercise;
        protected bool m_PartialExerciseSpecified;
        protected IMultipleExercise m_MultipleExercise;
        protected bool m_MultipleExerciseSpecified;
        protected string m_StreamAmountSelected;
        protected string m_Currency;
        protected Dictionary<string, string> m_LstRelevantUnderlyingDate;
        protected bool m_RelevantUnderlyingDateListSpecified;
        protected Dictionary<string, string> m_LstCashSettlementPaymentDate;
        protected bool m_CashSettlementPaymentDateListSpecified;
        protected CashSettlementAmountProvision m_CashSettlement;
        protected bool m_CashSettlementSpecified;

        #region PostElements
        protected NotionalProvisionMsg[] m_PostNotionalProvisionMsg;
        protected FeeProvisionMsg m_PostFeeProvisionMsg;
        protected CashSettlementProvisionMsg m_PostCashSettlementProvisionMsg;
        #endregion PostElements
        #endregion Members

        #region Virtual Accessors
        // EG 20150428 [20513] New
        protected virtual bool IsProvisionExecuted { set; get; }
        // EG 20150428 [20513] New
        protected virtual bool IsImplicitProvision { get { return false; } }
        // EG 20150428 [20513] New
        protected virtual bool IsMandatoryEarlyTermination { set; get; }
        // EG 20150428 [20513] New
        protected virtual object GetObjectProvision { set; get; }
        // EG 20150428 [20513] New
        protected virtual IAmericanExercise AmericanExerciseProvision { get { return null; } }
        protected virtual IBermudaExercise BermudaExerciseProvision { get { return null; } }
        protected virtual IEuropeanExercise EuropeanExerciseProvision { get { return null; } }
        protected virtual ICashSettlement CashSettlementProvision { get { return null; } }
        public virtual IBusinessCenterTime ExpirationTimeProvision { get { return null; } }
        // EG 20150428 [20513] New
        public virtual IExerciseFee ExerciseFee
        {
            get
            {
                IExerciseFee exerciseFee = null;
                IEuropeanExercise exercise = EuropeanExerciseProvision;
                if (null != exercise && exercise.ExerciseFeeSpecified)
                    exerciseFee = exercise.ExerciseFee;
                return exerciseFee;
            }
        }
        // EG 20150428 [20513] New
        public virtual IExerciseFeeSchedule ExerciseFeeSchedule
        {
            get
            {
                IExerciseFeeSchedule exerciseFeeSchedule = null;
                if (IsAmerican)
                {
                    IAmericanExercise americanExercise = AmericanExerciseProvision;
                    if ((null != americanExercise) && americanExercise.ExerciseFeeScheduleSpecified)
                        exerciseFeeSchedule = americanExercise.ExerciseFeeSchedule;
                }
                else if (IsBermuda)
                {
                    IBermudaExercise bermudaExercise = BermudaExerciseProvision;
                    if ((null != bermudaExercise) && bermudaExercise.ExerciseFeeScheduleSpecified)
                        exerciseFeeSchedule = bermudaExercise.ExerciseFeeSchedule;
                }
                return exerciseFeeSchedule;
            }
        }
        // EG 20150428 [20513] New
        public virtual IPartialExercise PartialExerciseProvision
        {
            get
            {
                IPartialExercise partialExercise = null;
                if (IsEuropean)
                {
                    IEuropeanExercise europeanExercise = EuropeanExerciseProvision;
                    if ((null != europeanExercise) && europeanExercise.PartialExerciseSpecified)
                        partialExercise = europeanExercise.PartialExercise;
                }
                return partialExercise;
            }
        }
        // EG 20150428 [20513] New
        public IMultipleExercise MultipleExerciseProvision
        {
            get
            {
                IMultipleExercise multipleExercise = null;
                if (IsAmerican)
                {
                    IAmericanExercise americanExercise = AmericanExerciseProvision;
                    if ((null != americanExercise) && americanExercise.MultipleExerciseSpecified)
                        multipleExercise = americanExercise.MultipleExercise;
                }
                else if (IsBermuda)
                {
                    IBermudaExercise bermudaExerciseExercise = BermudaExerciseProvision;
                    if ((null != bermudaExerciseExercise) && bermudaExerciseExercise.MultipleExerciseSpecified)
                        multipleExercise = bermudaExerciseExercise.MultipleExercise;
                }
                return multipleExercise;
            }
        }

        #endregion Virtual Accessors

        #region Accessors
        #region AddCells_Capture
        protected virtual TableCell[] AddCells_Capture
        {
            get
            {
                ArrayList aCell = new ArrayList
                {
                    TableTools.AddCell(ActionDate, HorizontalAlign.Center, 80, UnitEnum.Pixel),
                    TableTools.AddCell(actionTime.Value, HorizontalAlign.Center, 60, UnitEnum.Pixel),
                    TableTools.AddCell(ValueDate.Value, HorizontalAlign.Center, 80, UnitEnum.Pixel),
                    AddPanelOfNotionalAmountSelected(true)
                };
                if (null != m_FeeAmountProvision)
                    aCell.Add(AddPanelOfFeeAmountSelected());
                if (m_CashSettlementSpecified)
                    aCell.Add(AddPanelOfCashSettlement());

                aCell.Add(TableTools.AddCell(note, HorizontalAlign.NotSet, 100, UnitEnum.Percentage, true, false, false));
                aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel));

                return (TableCell[])aCell.ToArray(typeof(TableCell));
            }
        }
        #endregion AddCells_Capture
        #region AddCells_Static
        public virtual TableCell[] AddCells_Static(TradeActionEvent pEvent, TradeActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList
            {
                AddCellEventCode(pEvent.eventCode),
                AddCellEventType(pEvent.eventType),
                AddPanelOfNotionalAmountSelected(false)
            };
            aCell.AddRange(AddCells_Capture);

            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
        #region AddPanelOfCashSettlement
        protected TableCell AddPanelOfCashSettlement()
        {
            TableCell td = new TableCell();
            Table tableResult = new Table
            {
                // EG 20150605 [21087]
                CssClass = "subActionDataGrid",
                CellPadding = 0,
                CellSpacing = 0,
                Height = Unit.Percentage(100)
            };
            //string label = string.Empty;
            if (m_CashSettlementSpecified)
            {
                string dtPayment = DtFunc.DateTimeToString(m_CashSettlement.resultPaymentDate.DateValue, DtFunc.FmtShortDate);
                TableRow tr = new TableRow
                {
                    CssClass = m_Event.GetDefaultRowClass
                };
                tableResult.Rows.Add(tr);
                #region CashSettlementAmount
                // EG 20150605 [21087]
                tableResult.CssClass = "subActionDataGrid";
                tableResult.Width = Unit.Percentage(100);
                tableResult.CellPadding = 0;
                tableResult.CellSpacing = 0;
                string resultAmount = m_CashSettlement.cashSettlementAmount.Currency;
                resultAmount += " " + StrFunc.FmtDecimalToCurrentCulture(m_CashSettlement.cashSettlementAmount.Amount.DecValue);
                tr = new TableRow
                {
                    CssClass = m_Event.GetDefaultRowClass
                };
                tr.Cells.Add(TableTools.AddCell(ResFormAmount, true));
                tr.Cells.Add(TableTools.AddCell(resultAmount, HorizontalAlign.Left));
                tableResult.Rows.Add(tr);
                #endregion CashSettlementAmount
                #region CashSettlement details
                tr = new TableRow
                {
                    CssClass = "DataGrid_AlternatingItemStyle"
                };
                TableCell tdInfo = TableTools.AddCell(ResFormTitleDetail, HorizontalAlign.Left);
                tdInfo.ColumnSpan = 2;
                tr.Cells.Add(tdInfo);
                tableResult.Rows.Add(tr);

                tr = new TableRow();
                tdInfo = new TableCell
                {
                    ColumnSpan = 2
                };
                tr.Cells.Add(tdInfo);
                tableResult.Rows.Add(tr);

                Table tableinfo = new Table
                {
                    Height = Unit.Percentage(100),
                    Width = Unit.Percentage(100),
                    CellSpacing = 0,
                    CellPadding = 1
                };

                Panel panel = new Panel
                {
                    // EG 20150605 [21087]
                    Height = Unit.Pixel(70)
                };
                panel.Style[HtmlTextWriterStyle.Overflow] = "auto";
                panel.Controls.Add(tableinfo);
                tdInfo.Controls.Add(panel);


                #region CashSettlementPaymentDate
                TableRow trInfo = new TableRow
                {
                    CssClass = m_Event.GetDefaultRowClass
                };
                trInfo.Cells.Add(TableTools.AddCell(ResFormPaymentDate, true));
                trInfo.Cells.Add(TableTools.AddCell(dtPayment, HorizontalAlign.Left, 80, UnitEnum.Pixel));
                tableinfo.Rows.Add(trInfo);
                #endregion CashSettlementPaymentDate

                #region CashSettlement Payer/Receiver
                if ((null != m_CashSettlement.payerReference) && (null != m_CashSettlement.receiverReference))
                {
                    string payer = m_CashSettlement.payerReference.partyReference;
                    string receiver = m_CashSettlement.receiverReference.partyReference;

                    if (StrFunc.IsFilled(m_CashSettlement.payerReference.bookIdentifier))
                        payer += " [" + m_CashSettlement.payerReference.bookIdentifier + "]";
                    if (StrFunc.IsFilled(m_CashSettlement.receiverReference.bookIdentifier))
                        receiver += " [" + m_CashSettlement.receiverReference.bookIdentifier + "]";

                    trInfo = new TableRow
                    {
                        CssClass = m_Event.GetDefaultRowClass
                    };
                    trInfo.Cells.Add(TableTools.AddCell(ResFormPayer, true));
                    trInfo.Cells.Add(TableTools.AddCell(payer, HorizontalAlign.Left));
                    tableinfo.Rows.Add(trInfo);

                    trInfo = new TableRow
                    {
                        CssClass = m_Event.GetDefaultRowClass
                    };
                    trInfo.Cells.Add(TableTools.AddCell(ResFormReceiver, true));
                    trInfo.Cells.Add(TableTools.AddCell(receiver, HorizontalAlign.Left));
                    tableinfo.Rows.Add(trInfo);
                }
                #endregion CashSettlement Payer/Receiver

                #region CashSettlement Rate
                if (m_CashSettlement.cashSettlementRateSpecified)
                {
                    trInfo = new TableRow
                    {
                        CssClass = m_Event.GetDefaultRowClass
                    };
                    trInfo.Cells.Add(TableTools.AddCell(ResFormRate, true));
                    trInfo.Cells.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(m_CashSettlement.cashSettlementRate.DecValue, 5), HorizontalAlign.Left));
                    tableinfo.Rows.Add(trInfo);
                }
                #endregion CashSettlement Rate
                #endregion Fee Amount details
                td.Controls.Add(tableResult);
            }
            else
            {
                // Display No CashSettlement
            }
            return td;
        }
        #endregion AddPanelOfCashSettlement
        #region AddPanelOfFeeAmountSelected
        protected TableCell AddPanelOfFeeAmountSelected()
        {
            TableCell td = new TableCell();
            Table tableResult = new Table
            {
                // EG 20150605 [21087]
                CssClass = "subActionDataGrid",
                CellPadding = 0,
                CellSpacing = 0,
                Height = Unit.Percentage(100)
            };
            //string label = string.Empty;
            if (null != m_FeeAmountProvision)
            {
                foreach (FeeAmountProvision item in m_FeeAmountProvision)
                {
                    if (item.isSelected)
                    {
                        string dtPayment = DtFunc.DateTimeToString(item.feeAmount.resultPaymentDate.AdjustedEventDate.DateValue, DtFunc.FmtShortDate);
                        TableRow tr = new TableRow
                        {
                            CssClass = m_Event.GetDefaultRowClass
                        };
                        tableResult.Rows.Add(tr);
                        #region fee Amount
                        // EG 20150605 [21087]
                        tableResult.CssClass = "subActionDataGrid";
                        tableResult.Width = Unit.Percentage(100);
                        tableResult.CellPadding = 0;
                        tableResult.CellSpacing = 0;
                        string resultAmount = item.nominalStep.Currency;
                        resultAmount += " " + StrFunc.FmtDecimalToCurrentCulture(item.feeAmount.resultAmount.DecValue);
                        tr = new TableRow
                        {
                            CssClass = m_Event.GetDefaultRowClass
                        };
                        tr.Cells.Add(TableTools.AddCell(ResFormTitleFee, true));
                        tr.Cells.Add(TableTools.AddCell(resultAmount, HorizontalAlign.Left));
                        tableResult.Rows.Add(tr);
                        #endregion fee Amount
                        #region Fee Amount details
                        tr = new TableRow
                        {
                            CssClass = "DataGrid_AlternatingItemStyle"
                        };
                        TableCell tdInfo = TableTools.AddCell(ResFormTitleDetail, HorizontalAlign.Left);
                        tdInfo.ColumnSpan = 2;
                        tr.Cells.Add(tdInfo);
                        tableResult.Rows.Add(tr);

                        tr = new TableRow();
                        tdInfo = new TableCell
                        {
                            ColumnSpan = 2
                        };
                        tr.Cells.Add(tdInfo);
                        tableResult.Rows.Add(tr);

                        Table tableinfo = new Table
                        {
                            Height = Unit.Percentage(100),
                            Width = Unit.Percentage(100),
                            CellSpacing = 0,
                            CellPadding = 1
                        };

                        Panel panel = new Panel
                        {
                            Height = Unit.Pixel(30)
                        };
                        panel.Style[HtmlTextWriterStyle.Overflow] = "auto";
                        panel.Controls.Add(tableinfo);
                        tdInfo.Controls.Add(panel);


                        #region Fee Date
                        TableRow trInfo = new TableRow
                        {
                            CssClass = m_Event.GetDefaultRowClass
                        };
                        trInfo.Cells.Add(TableTools.AddCell(ResFormPaymentDate, true));
                        trInfo.Cells.Add(TableTools.AddCell(dtPayment, HorizontalAlign.Left, 80, UnitEnum.Pixel));
                        tableinfo.Rows.Add(trInfo);
                        #endregion Fee Date

                        #region Fee Payer/Receiver
                        string payer = item.payerReference.partyReference;
                        string receiver = item.receiverReference.partyReference;

                        if (StrFunc.IsFilled(item.payerReference.bookIdentifier))
                            payer += " [" + item.payerReference.bookIdentifier + "]";
                        if (StrFunc.IsFilled(item.receiverReference.bookIdentifier))
                            receiver += " [" + item.receiverReference.bookIdentifier + "]";

                        trInfo = new TableRow
                        {
                            CssClass = m_Event.GetDefaultRowClass
                        };
                        trInfo.Cells.Add(TableTools.AddCell(ResFormPayer, true));
                        trInfo.Cells.Add(TableTools.AddCell(payer, HorizontalAlign.Left));
                        tableinfo.Rows.Add(trInfo);

                        trInfo = new TableRow
                        {
                            CssClass = m_Event.GetDefaultRowClass
                        };
                        trInfo.Cells.Add(TableTools.AddCell(ResFormReceiver, true));
                        trInfo.Cells.Add(TableTools.AddCell(receiver, HorizontalAlign.Left));
                        tableinfo.Rows.Add(trInfo);
                        #endregion Fee Payer/Receiver
                        #region Fee NotionalReference
                        if (item.nominalStepSpecified)
                        {
                            string notionalReference = item.nominalStep.Currency;
                            notionalReference += " " + StrFunc.FmtDecimalToCurrentCulture(item.nominalStep.Amount.DecValue);
                            trInfo = new TableRow
                            {
                                CssClass = m_Event.GetDefaultRowClass
                            };
                            trInfo.Cells.Add(TableTools.AddCell(ResFormNotionalReference, true));
                            trInfo.Cells.Add(TableTools.AddCell(notionalReference, HorizontalAlign.Left));
                            tableinfo.Rows.Add(trInfo);
                        }
                        #endregion Fee NotionalReference
                        #region Fee Amount/Rate
                        trInfo = new TableRow
                        {
                            CssClass = m_Event.GetDefaultRowClass
                        };
                        if (item.feeAmount.amountSpecified)
                        {
                            trInfo.Cells.Add(TableTools.AddCell(ResFormAmount, true));
                            trInfo.Cells.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(item.feeAmount.amount.DecValue), HorizontalAlign.Left));
                        }
                        else if (item.feeAmount.rateSpecified)
                        {
                            trInfo.Cells.Add(TableTools.AddCell(ResFormRate, true));
                            trInfo.Cells.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(item.feeAmount.rate.DecValue, 5), HorizontalAlign.Left));
                        }
                        tableinfo.Rows.Add(trInfo);
                        #endregion Fee Amount/Rate
                        #endregion Fee Amount details
                        td.Controls.Add(tableResult);
                    }
                }
            }
            else
            {
                // Display No Commission
            }
            return td;
        }
        #endregion AddPanelOfFeeAmountSelected
        #region AddPanelOfNotionalAmountSelected
        // EG 20180514 [23812] Report
        protected TableCell AddPanelOfNotionalAmountSelected(bool pDisplayCancelableAmount)
        {
            TableCell td = new TableCell();
            Panel panel = new Panel
            {
                Height = Unit.Pixel(40)
            };
            panel.Style[HtmlTextWriterStyle.Overflow] = "auto";

            Table table = new Table
            {
                // EG 20150605 [21087]
                CssClass = "subActionDataGrid",
                CellPadding = 0,
                CellSpacing = 0,
                Height = Unit.Percentage(100)
            };
            if (null != m_NotionalAmountProvision)
            {
                foreach (NotionalAmountProvision item in m_NotionalAmountProvision)
                {
                    if (item.isSelected)
                    {
                        TableRow tr = new TableRow
                        {
                            CssClass = m_Event.GetDefaultRowClass
                        };
                        tr.Cells.Add(TableTools.AddCell(Ressource.GetString2("StreamNumber", item.streamNo), true));
                        if (pDisplayCancelableAmount)
                        {
                            if (IsProvisionExecuted && item.variationAmountAtEndPeriodSpecified)
                            {
                                tr.Cells.Add(TableTools.AddCell(item.variationAmountAtEndPeriod.Currency, HorizontalAlign.Center));
                                tr.Cells.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(item.variationAmountAtEndPeriod.Amount.DecValue), HorizontalAlign.Right));
                            }
                            else
                            {
                                tr.Cells.Add(TableTools.AddCell(item.provisionNotionalStepAmount.Currency, HorizontalAlign.Center));
                                tr.Cells.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(item.provisionNotionalStepAmount.Amount.DecValue), HorizontalAlign.Right));
                            }
                        }
                        else
                        {
                            tr.Cells.Add(TableTools.AddCell(item.notionalStepAmount.Currency, HorizontalAlign.Center));
                            tr.Cells.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(item.notionalStepAmount.Amount.DecValue), HorizontalAlign.Right));
                        }
                        table.Rows.Add(tr);
                    }
                }
            }
            panel.Controls.Add(table);
            td.Controls.Add(panel);
            return td;
        }
        #endregion AddPanelOfNotionalAmountSelected
        #region CashSettlementPaymentDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected EFS_AdjustableDates CashSettlementPaymentDate(DateTime pDtAction)
        {
            EFS_AdjustableDates adjustedPaymentDate = new EFS_AdjustableDates();
            ICashSettlement cashSettlement = CashSettlementProvision;
            if ((null != cashSettlement) && (cashSettlement.PaymentDateSpecified))
            {
                if (cashSettlement.PaymentDate.AdjustableDatesSpecified)
                {
                    #region AdjustableDate
                    IAdjustableDates adjustableDates = cashSettlement.PaymentDate.AdjustableDates;
                    adjustedPaymentDate.adjustableDates = new EFS_AdjustableDate[adjustableDates.UnadjustedDate.Length];
                    IBusinessDayAdjustments bda = adjustableDates.DateAdjustments;
                    for (int i = 0; i < adjustableDates.UnadjustedDate.Length; i++)
                    {
                        adjustedPaymentDate.adjustableDates[i] = new EFS_AdjustableDate(SessionTools.CS, adjustableDates[i], bda,
                            CurrentEvent.CurrentTradeAction.dataDocumentContainer);
                    }
                    #endregion AdjustableDate
                }
                else if (cashSettlement.PaymentDate.BusinessDateRangeSpecified)
                {
                    DateTime dtFirst = cashSettlement.PaymentDate.BusinessDateRange.UnadjustedFirstDate.DateValue;
                    DateTime dtLast = cashSettlement.PaymentDate.BusinessDateRange.UnadjustedLastDate.DateValue;
                    IBusinessDayAdjustments bda = cashSettlement.PaymentDate.BusinessDateRange.GetAdjustments;
                    adjustedPaymentDate.adjustableDates = new EFS_AdjustableDate[1];
                    if (0 < dtFirst.CompareTo(pDtAction))
                        adjustedPaymentDate.adjustableDates[0] = new EFS_AdjustableDate(SessionTools.CS, dtFirst, bda,
                            CurrentEvent.CurrentTradeAction.dataDocumentContainer);
                    else if (0 <= dtLast.CompareTo(pDtAction))
                        adjustedPaymentDate.adjustableDates[0] = new EFS_AdjustableDate(SessionTools.CS, pDtAction, bda,
                            CurrentEvent.CurrentTradeAction.dataDocumentContainer);
                }
                else if (cashSettlement.PaymentDate.RelativeDateSpecified)
                {
                    #region RelativeDateOffset
                    IRelativeDateOffset relativeDateOffset = cashSettlement.PaymentDate.RelativeDate;
                    Tools.OffSetDateRelativeTo(SessionTools.CS, relativeDateOffset, out DateTime[] offsetDates, CurrentEvent.CurrentTradeAction.dataDocumentContainer);
                    adjustedPaymentDate.adjustableDates = new EFS_AdjustableDate[offsetDates.Length];
                    for (int i = 0; i < offsetDates.Length; i++)
                    {
                        adjustedPaymentDate.adjustableDates[i] = new EFS_AdjustableDate(SessionTools.CS, offsetDates[i], 
                            relativeDateOffset.GetAdjustments, CurrentEvent.CurrentTradeAction.dataDocumentContainer);
                    }
                    #endregion RelativeDateOffset
                }
            }
            return adjustedPaymentDate;
        }
        #endregion CashSettlementPaymentDate
        #region CloneFeeAmount
        // EG 20180514 [23812] Report
        private static FeeAmountProvision[] CloneFeeAmount(FeeAmountProvision[] pFeeAmountSource)
        {
            if (null != pFeeAmountSource)
            {
                ArrayList aFeeAmount = new ArrayList();
                foreach (FeeAmountProvision item in pFeeAmountSource)
                {
                    FeeAmountProvision clone = (FeeAmountProvision)item.Clone();
                    aFeeAmount.Add(clone);
                }
                if (0 < aFeeAmount.Count)
                    return (FeeAmountProvision[])aFeeAmount.ToArray(typeof(FeeAmountProvision));
            }
            return null;
        }
        #endregion CloneFeeAmount
        #region CloneNotionalAmount
        // EG 20180514 [23812] Report
        private static NotionalAmountProvision[] CloneNotionalAmount(NotionalAmountProvision[] pNotionalAmountSource)
        {
            if (null != pNotionalAmountSource)
            {
                ArrayList aNotionalAmount = new ArrayList();
                foreach (NotionalAmountProvision item in pNotionalAmountSource)
                {
                    NotionalAmountProvision clone = (NotionalAmountProvision)item.Clone();
                    aNotionalAmount.Add(clone);
                }
                if (0 < aNotionalAmount.Count)
                    return (NotionalAmountProvision[])aNotionalAmount.ToArray(typeof(NotionalAmountProvision));
            }
            return null;
        }
        #endregion CloneNotionalAmount
        #region CreateControlActionDate
        protected new TableRow CreateControlActionDate
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormActionDate);
                StringBuilder sb = new StringBuilder();

                if (IsAmerican)
                {
                    DateTime dtLastExerciseProvision = GetDtLastExerciseProvision;
                    if (0 < m_ExerciseDates[0].dtStartPeriod.DateValue.CompareTo(dtLastExerciseProvision))
                        dtLastExerciseProvision = m_ExerciseDates[0].dtStartPeriod.DateValue;

                    string startPeriod = DtFunc.DateTimeToString(dtLastExerciseProvision.Date, DtFunc.FmtShortDate);
                    string endPeriod = DtFunc.DateTimeToString(m_ExerciseDates[0].dtEndPeriod.DateValue, DtFunc.FmtShortDate);
                    string errorMess = Ressource.GetString2("Failure_RangeDate", startPeriod, endPeriod);
                    Validator validator = new Validator(errorMess, true, false, ValidationDataType.Date, startPeriod, endPeriod);
                    controlGUI.Regex = EFSRegex.TypeRegex.RegexDate;
                    controlGUI.LblWidth = 105;
                    FpMLCalendarBox txtDate = new FpMLCalendarBox(null, actionDate.DateValue, "ActionDate", controlGUI, null, "TXTACTIONDATE_" + idE,
                        new Validator("ActionDate", true), new Validator(EFSRegex.TypeRegex.RegexDate, "ActionDate", true, false),
                        validator);
                    sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtDate.ID, TradeActionMode.TradeActionModeEnum.CalculProvisionExercise);
                    txtDate.Attributes.Add("onchange", sb.ToString());
                    td.Controls.Add(txtDate);
                }
                else
                {
                    controlGUI.LblWidth = 102;
                    controlGUI.Regex = EFSRegex.TypeRegex.RegexDate;
                    FpMLDropDownList ddlActionDate = new FpMLDropDownList(null, actionDate.Value, "DDLACTIONDATE_" + idE, 105, controlGUI, m_LstExerciseDate);
                    sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", ddlActionDate.ID, TradeActionMode.TradeActionModeEnum.CalculProvisionExercise);
                    ddlActionDate.Attributes.Add("onchange", sb.ToString());
                    td.Controls.Add(ddlActionDate);
                }
                if (false == IsMandatoryEarlyTermination)
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + "[" + TradeAction.GetEventTypeTitle(m_ExerciseDates[0].eventType) + "]"));
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlActionDate
        #region CreateControlCashSettlement
        protected TableRow[] CreateControlCashSettlement
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                aTableRow.Add(CreateControlTitleSeparator(ResFormTitleCashSettlement, false));
                #region CashSettlementPaymentDate
                aTableRow.Add(CreateControlCashSettlementPaymentDate);
                #endregion CashSettlementPaymentDate
                #region CashSettlementAmount
                aTableRow.Add(CreateControlCashSettlementAmount);
                #endregion CashSettlementAmount
                #region CashSettlementRate
                aTableRow.Add(CreateControlCashSettlementRate);
                #endregion CashSettlementRate
                #region Payer/Receiver
                aTableRow.Add(CreateControlCashSettlementPayer);
                aTableRow.Add(CreateControlCashSettlementReceiver);
                #endregion Payer/Receiver
                return ((TableRow[])aTableRow.ToArray(typeof(TableRow)));
            }
        }
        #endregion CreateControlCashSettlement
        #region CreateControlCashSettlementAmount
        // 20080416 EG Ticket 16173
        protected TableRow CreateControlCashSettlementAmount
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormAmount)
                {
                    LblWidth = 105
                };
                string key = "CashSettlementAmount";
                FpMLDropDownList ddlCurrency = new FpMLDropDownList(null, m_CashSettlement.cashSettlementAmount.Currency, 50, "Currency", controlGUI, null)
                {
                    ID = "DDLCSHCURRENCY_" + idE.ToString()
                };

                controlGUI = new ControlGUI(false, ResFormAmount)
                {
                    Regex = EFSRegex.TypeRegex.RegexAmountExtend
                };
                FpMLTextBox txtAmount = new FpMLTextBox(null, m_CashSettlement.cashSettlementAmount.Amount.CultureValue, 150, key, controlGUI, null, false, "TXTCSHAMOUNT_" + idE, null,
                    new Validator(key, true),
                    new Validator(EFSRegex.TypeRegex.RegexAmountExtend, key, true, false));
                GetFormatControlAttribute(txtAmount);
                td.Controls.Add(ddlCurrency);
                td.Controls.Add(txtAmount);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlCashSettlementAmount
        #region CreateControlCashSettlementPaymentDate
        protected TableRow CreateControlCashSettlementPaymentDate
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormPaymentDate)
                {
                    Regex = EFSRegex.TypeRegex.RegexDate,
                    LblWidth = 105
                };
                FpMLCalendarBox txtDate = new FpMLCalendarBox(null, m_CashSettlement.resultPaymentDate.DateValue, "CashSettlementPaymentDate", controlGUI, null, "TXTCSHPAYMENTDATE_" + idE,
                    new Validator("CashSettlementPaymentDate", true),
                    new Validator(EFSRegex.TypeRegex.RegexDate, "PaymentDate", true, false));
                GetFormatControlAttribute(txtDate);
                td.Controls.Add(txtDate);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlCashSettlementPaymentDate
        #region CreateControlCashSettlementPayer
        // 20080416 EG Ticket 16173
        protected TableRow CreateControlCashSettlementPayer
        {
            get
            {
                return CreateControlCashSettlementParty("Payer", m_CashSettlement.payerReferenceWork.Identifier);
            }
        }
        #endregion CreateControlCashSettlementPayer
        #region CreateControlCashSettlementParty
        // 20080416 EG Ticket 16173
        protected TableRow CreateControlCashSettlementParty(string pLabel, string pIdentifier)
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, pLabel)
            {
                LblWidth = 105
            };
            FpMLDropDownList ddlParty = new FpMLDropDownList(null, pIdentifier, "DDLCSH" + pLabel + "_" + idE.ToString(), 350, controlGUI, m_CashSettlement.partyInfoGUI)
            {
                FpMLKey = "PartyReference"
            };
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", ddlParty.ID, TradeActionMode.TradeActionModeEnum.SynchronizePayerReceiver);
            ddlParty.Attributes.Add("onchange", sb.ToString());
            td.Controls.Add(ddlParty);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlCashSettlementParty
        #region CreateControlCashSettlementRate
        protected TableRow CreateControlCashSettlementRate
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                 tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlCashSettlementRate
        #region CreateControlCashSettlementReceiver
        // 20080416 EG Ticket 16173
        protected TableRow CreateControlCashSettlementReceiver
        {
            get
            {
                return CreateControlCashSettlementParty("Receiver", m_CashSettlement.receiverReferenceWork.Identifier);
            }
        }
        #endregion CreateControlCashSettlementReceiver
        #region CreateControlCurrentAction
        // EG 20180514 [23812] Report
        public virtual TableRow[] CreateControlCurrentAction
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                aTableRow.Add(CreateControlTitleSeparator(ResFormTitleExerciseEvents, false));
                #region ActionDate
                aTableRow.Add(CreateControlActionDate);
                #endregion ActionDate
                #region ActionTime
                aTableRow.Add(base.CreateControlActionTime(IsEuropean));
                #endregion ActionTime
                #region ValueDate
                aTableRow.Add(CreateControlValueDate);
                #endregion ValueDate
                #region ExerciseType
                if (false == this.GetType().Equals(typeof(StepUpProvisionEvents)))
                {
                    aTableRow.Add(CreateControlExerciseType);
                }
                #endregion ExerciseType
                #region NotionalAmount
                if (null != m_NotionalAmountProvisionWork)
                {
                    aTableRow.Add(CreateControlTitleSeparator(ResFormTitleNotionalAmountsDispo, false));
                    foreach (NotionalAmountProvision item in m_NotionalAmountProvisionWork)
                    {
                        if (item.isSelected)
                        {
                            aTableRow.Add(CreateControlNotionalAmount(item, EventTypeFunc.IsTotal(m_ExerciseType)));
                        }
                    }
                }
                #endregion NotionalAmount
                #region Fee
                if (null != m_FeeAmountProvisionWork)
                {
                    aTableRow.Add(CreateControlTitleSeparator(ResFormTitleFee, true));
                    foreach (FeeAmountProvision item in m_FeeAmountProvisionWork)
                    {
                        if (item.isSelected)
                        {
                            aTableRow.Add(CreateControlFeePaymentDate(item));
                            aTableRow.Add(CreateControlFeeAmount(item));
                            break;
                        }
                    }
                }
                #endregion Fee
                #region CashSettlement
                if (m_CashSettlementSpecified)
                    aTableRow.AddRange(CreateControlCashSettlement);
                #endregion CashSettlement
                aTableRow.AddRange(base.CreateControlDescription);
                return ((TableRow[])aTableRow.ToArray(typeof(TableRow)));
            }
        }
        #endregion CreateControlCurrentAction
        #region CreateControlExerciseType
        protected TableRow CreateControlExerciseType
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormExerciseType)
                {
                    LblWidth = 105
                };
                // EG 20150605 [21087] use m_LstExerciseType
                FpMLDropDownList ddlExerciseType = new FpMLDropDownList(null, m_ExerciseType, "DDLEXERCISETYPE_" + idE, 150, controlGUI, m_LstExerciseType);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", ddlExerciseType.ID, TradeActionMode.TradeActionModeEnum.CalculProvisionExercise);
                ddlExerciseType.Attributes.Add("onchange", sb.ToString());
                td.Controls.Add(ddlExerciseType);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlExerciseType
        #region CreateControlValueDate
        protected TableRow CreateControlValueDate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormValueDate)
                {
                    Regex = EFSRegex.TypeRegex.RegexDate,
                    LblWidth = 105
                };
                if (DtFunc.IsDateTimeEmpty(ValueDate.DateValue))
                    ValueDate.DateTimeValue = ExpiryDate.AddDays(2);
                FpMLCalendarBox txtDate = new FpMLCalendarBox(null, ValueDate.DateValue, "ValueDate", controlGUI, null, "TXTVALUEDATE_" + idE,
                    new Validator("ValueDate", true), new Validator(EFSRegex.TypeRegex.RegexDate, "ValueDate", true, false));
                sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtDate.ID, TradeActionMode.TradeActionModeEnum.CalculProvisionExercise);
                txtDate.Attributes.Add("onchange", sb.ToString());
                td.Controls.Add(txtDate);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlValueDate
        #region CreateHeaderCells_Capture
        protected virtual TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleAbandonExerciseDate, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormValueDate, false, 0, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell((IsProvisionExecuted ? ResFormTitleNotionalAmounts : ResFormTitleNotionalAmountsDispo), false, 0, UnitEnum.Pixel, 0, false)
                };
                if (null != m_FeeAmountProvision)
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleFee, false, 0, UnitEnum.Pixel, 0, false));
                if (m_CashSettlementSpecified)
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleCashSettlement, false, 0, UnitEnum.Pixel, 0, false));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false));
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Capture
        #region CreateHeaderCells_Static
        protected virtual TableHeaderCell[] CreateHeaderCells_Static
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleEventCode, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitleCurrentNotionalAmounts, false, 0, UnitEnum.Pixel, 0, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Static
        #region GetCashSettlementAmount
        protected CashSettlementAmountProvision GetCashSettlementAmount
        {
            get
            {
                IProduct product = (IProduct)CurrentProduct;
                ICashSettlement cashSettlement = CashSettlementProvision;
                if (null != cashSettlement)
                {
                    CashSettlementAmountProvision cashSettlementProvision = new CashSettlementAmountProvision(m_Event, cashSettlement);
                    if (null != cashSettlementProvision)
                    {
                        if (product.ProductBase.IsSwap)
                        {
                            foreach (IInterestRateStream item in ((ISwap)product).Stream)
                            {
                                if (null == cashSettlementProvision[item.PayerPartyReference])
                                    cashSettlementProvision.Add(item.PayerPartyReference, m_Event);
                                if (null == cashSettlementProvision[item.ReceiverPartyReference])
                                    cashSettlementProvision.Add(item.ReceiverPartyReference, m_Event);
                            }
                        }
                        else if (product.ProductBase.IsCapFloor)
                        {
                            IInterestRateStream stream = ((ICapFloor)product).Stream;
                            if (null == cashSettlementProvision[stream.PayerPartyReference])
                                cashSettlementProvision.Add(stream.PayerPartyReference, m_Event);
                            if (null == cashSettlementProvision[stream.ReceiverPartyReference])
                                cashSettlementProvision.Add(stream.ReceiverPartyReference, m_Event);
                        }
                        return cashSettlementProvision; ;
                    }
                }
                return null;
            }
        }
        #endregion GetCashSettlementAmount
        #region GetCashSettlementAmountExecuted
        // 20090403 EG Ticket : 16539 Add line 
        protected CashSettlementAmountProvision GetCashSettlementAmountExecuted
        {
            get
            {
                CashSettlementAmountProvision cashSettlementProvision = null;
                TradeActionEvent eventCashSettlement = CurrentEvent.EventExerciseCashSettlement;
                if (null != eventCashSettlement)
                {
                    // 20090403 EG Ticket : 16539 Add next line 
                    eventCashSettlement.currentTradeAction = m_Event.CurrentTradeAction;
                    cashSettlementProvision = new CashSettlementAmountProvision(eventCashSettlement);
                }
                return cashSettlementProvision;
            }
        }
        #endregion GetCashSettlementAmountExecuted
        #region GetCurrentNominalStepForFee
        protected IMoney GetCurrentNominalStepForFee(string pNotionalReference, DateTime pDate)
        {
            IMoney nominalStep = null;
            IProduct product = (IProduct)CurrentProduct;
            if (product.ProductBase.IsSwap)
            {
                ISwap swap = (ISwap)product;
                foreach (IInterestRateStream item in swap.Stream)
                {
                    if (null != ReflectionTools.GetObjectById(item, pNotionalReference))
                    {
                        string streamNo = item.Id.Replace("swapStream", string.Empty);
                        NotionalAmountProvision amountProvision = (NotionalAmountProvision)this[typeof(NotionalAmountProvision), m_Event.instrumentNo, streamNo, pDate];
                        if (null != item)
                        {
                            nominalStep = amountProvision.notionalStepAmount;
                            break;
                        }
                    }
                }
            }
            else if (product.ProductBase.IsCapFloor)
            {
                ICapFloor capFloor = (ICapFloor)product;
                if (null != ReflectionTools.GetObjectById(capFloor.Stream, pNotionalReference))
                {
                    string streamNo = capFloor.Stream.Id.Replace("capfloorStream", string.Empty);
                    NotionalAmountProvision amountProvision = (NotionalAmountProvision)this[typeof(NotionalAmountProvision), m_Event.instrumentNo, streamNo, pDate];
                    if (null != capFloor.Stream)
                        nominalStep = amountProvision.notionalStepAmount;
                }
            }
            return nominalStep;
        }
        #endregion GetCurrentNominalStepForFee
        #region GetFeeAmount
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected FeeAmountProvision[] GetFeeAmount
        {
            get
            {
                List<FeeAmountProvision> lstFeeAmountProvision = new List<FeeAmountProvision>();
                FeeAmountInfo feeAmount;
                PartyInfoProvision feeReceiver;
                PartyInfoProvision feePayer;
                FeeAmountProvision feeAmountProvision;
                if (IsEuropean)
                {
                    IExerciseFee fee = ExerciseFee;
                    if (null != fee)
                    {
                        feePayer = new PartyInfoProvision(fee.PayerPartyReference, m_Event);
                        feeReceiver = new PartyInfoProvision(fee.ReceiverPartyReference, m_Event);
                        feeAmount = new FeeAmountInfo(fee, m_Event.CurrentTradeAction.dataDocumentContainer);
                        feeAmountProvision = new FeeAmountProvision(feePayer, feeReceiver, fee.NotionalReference.HRef, feeAmount);
                        lstFeeAmountProvision.Add(feeAmountProvision);
                    }
                }
                else if (IsAmerican || IsBermuda)
                {
                    IProduct product = (IProduct)CurrentProduct;
                    IExerciseFeeSchedule fee = ExerciseFeeSchedule;
                    if (null != fee)
                    {
                        feePayer = new PartyInfoProvision(fee.PayerPartyReference, m_Event);
                        feeReceiver = new PartyInfoProvision(fee.ReceiverPartyReference, m_Event);
                        IExerciseFee feeItem;
                        if (fee.FeeAmountSpecified)
                        {
                            #region Amount schedule
                            feeItem = product.ProductBase.CreateExerciseFee();
                            feeItem.FeeAmountSpecified = fee.FeeAmountSpecified;
                            feeItem.FeeAmount = new EFS_Decimal(fee.FeeAmount.InitialValue.DecValue);
                            feeItem.FeePaymentDate = fee.FeePaymentDate;
                            feeAmount = new FeeAmountInfo(feeItem, m_Event.CurrentTradeAction.dataDocumentContainer);
                            feeAmountProvision = new FeeAmountProvision(feePayer, feeReceiver, fee.NotionalReference.HRef, feeAmount);
                            lstFeeAmountProvision.Add(feeAmountProvision);

                            if (fee.FeeAmount.StepSpecified)
                            {
                                #region Step Amount
                                foreach (IStep step in fee.FeeAmount.Step)
                                {
                                    feeItem = product.ProductBase.CreateExerciseFee();
                                    feeItem.FeeAmountSpecified = fee.FeeAmountSpecified;
                                    feeItem.FeeAmount = new EFS_Decimal(step.StepValue.DecValue);
                                    feeItem.FeePaymentDate = fee.FeePaymentDate;
                                    feeAmount = new FeeAmountInfo(feeItem, step.StepDate.DateValue, m_Event.CurrentTradeAction.dataDocumentContainer);
                                    feeAmountProvision = new FeeAmountProvision(feePayer, feeReceiver, fee.NotionalReference.HRef, feeAmount);
                                    lstFeeAmountProvision.Add(feeAmountProvision);
                                }
                                #endregion Step Amount
                            }
                            #endregion Amount schedule
                        }
                        else if (fee.FeeRateSpecified)
                        {
                            #region Rate schedule
                            feeItem = product.ProductBase.CreateExerciseFee();
                            feeItem.FeeRateSpecified = fee.FeeRateSpecified;
                            feeItem.FeeRate = new EFS_Decimal(fee.FeeRate.InitialValue.DecValue);
                            feeItem.FeePaymentDate = fee.FeePaymentDate;
                            feeAmount = new FeeAmountInfo(feeItem, m_Event.CurrentTradeAction.dataDocumentContainer);
                            feeAmountProvision = new FeeAmountProvision(feePayer, feeReceiver, fee.NotionalReference.HRef, feeAmount);
                            lstFeeAmountProvision.Add(feeAmountProvision);
                            if (fee.FeeRate.StepSpecified)
                            {
                                #region Step Rate
                                foreach (IStep step in fee.FeeRate.Step)
                                {
                                    feeItem = product.ProductBase.CreateExerciseFee();
                                    feeItem.FeeRateSpecified = fee.FeeRateSpecified;
                                    feeItem.FeeRate = new EFS_Decimal(step.StepValue.DecValue);
                                    feeItem.FeePaymentDate = fee.FeePaymentDate;
                                    feeAmount = new FeeAmountInfo(feeItem, step.StepDate.DateValue, m_Event.CurrentTradeAction.dataDocumentContainer);
                                    feeAmountProvision = new FeeAmountProvision(feePayer, feeReceiver, fee.NotionalReference.HRef, feeAmount);
                                    lstFeeAmountProvision.Add(feeAmountProvision);
                                }
                                #endregion Step Rate
                            }
                            #endregion Rate schedule
                        }
                    }
                }
                if (0 < lstFeeAmountProvision.Count)
                    return (FeeAmountProvision[])lstFeeAmountProvision.ToArray();
                return null;
            }
        }
        #endregion GetFeeAmount
        #region GetFeeAmountExecuted
        protected FeeAmountProvision[] GetFeeAmountExecuted
        {
            get
            {
                IProduct product = (IProduct)CurrentProduct;
                ArrayList aFeeAmountProvision = new ArrayList();
                TradeActionEvent eventFee = CurrentEvent.EventExerciseFee;
                IMoney nominalStep = product.ProductBase.CreateMoney();
                if (null != eventFee)
                {
                    PartyInfoProvision feePayer = new PartyInfoProvision(eventFee.idPayer, eventFee.idPayerBookSpecified ? eventFee.idPayerBook : 0, eventFee);
                    PartyInfoProvision feeReceiver = new PartyInfoProvision(eventFee.idReceiver, eventFee.idReceiverBookSpecified ? eventFee.idReceiverBook : 0, eventFee);
                    FeeAmountInfo feeAmount = new FeeAmountInfo(eventFee);
                    if (eventFee.detailsSpecified && eventFee.details.notionalReferenceSpecified)
                        nominalStep.Amount = new EFS_Decimal(eventFee.details.notionalReference.DecValue);

                    nominalStep.Currency = eventFee.unit;
                    FeeAmountProvision feeAmountProvision = new FeeAmountProvision(feePayer, feeReceiver, feeAmount, nominalStep)
                    {
                        isSelected = true
                    };
                    aFeeAmountProvision.Add(feeAmountProvision);
                }
                if (0 < aFeeAmountProvision.Count)
                    return (FeeAmountProvision[])aFeeAmountProvision.ToArray(typeof(FeeAmountProvision));
                return null;
            }
        }
        #endregion GetFeeAmountExecuted
        #region GetDtLastExerciseProvision
        private DateTime GetDtLastExerciseProvision
        {
            get
            {
                DateTime dtLastExerciseProvision = DateTime.MinValue;
                if (null != m_Event.dtEndPeriod)
                    dtLastExerciseProvision = m_Event.dtStartPeriod.DateValue;
                TradeActionEvent[] exerciseProvisions = CurrentEventParent.EventExerciseProvisionEffect(m_Event.eventCode);
                if (null != exerciseProvisions)
                {
                    foreach (TradeActionEvent item in exerciseProvisions)
                    {
                        if (null != item.eventClass)
                        {
                            foreach (EventClass itemClass in item.eventClass)
                            {
                                if (0 < itemClass.dtEvent.DateValue.CompareTo(dtLastExerciseProvision))
                                    dtLastExerciseProvision = itemClass.dtEvent.DateValue;
                            }
                        }
                    }
                }
                return dtLastExerciseProvision;
            }
        }
        #endregion GetDtLastExerciseProvision
        #region GetNumberOfNotionalAmountSelected
        public int GetNumberOfNotionalAmountSelected
        {
            get
            {
                int i = 0;
                foreach (NotionalAmountProvision item in m_NotionalAmountProvision)
                    i += item.isSelected ? 1 : 0;
                return i;
            }
        }
        #endregion GetNumberOfNotionalAmountSelected
        #region GetNotionalAmount
        // EG 20180514 [23812] Report
        protected NotionalAmountProvision[] GetNotionalAmount
        {
            get
            {
                List<NotionalAmountProvision> lstNotionalAmountProvision = new List<NotionalAmountProvision>();
                TradeActionEvent eventGrandParent = CurrentTradeAction2.events.GetEvent(m_EventParent.idEParent);
                if (null != eventGrandParent)
                {
                    TradeActionEvent[] eventStreams = eventGrandParent.EventStreamLeg;

                    if (null != eventStreams)
                    {
                        foreach (TradeActionEvent eventStream in eventStreams)
                        {
                            TradeActionEvent[] eventNominalSteps = eventStream.EventNominalStep;
                            if (null != eventNominalSteps)
                            {
                                foreach (TradeActionEvent item in eventNominalSteps)
                                {
                                    NotionalAmountProvision notionalAmountProvision = new NotionalAmountProvision(item);
                                    lstNotionalAmountProvision.Add(notionalAmountProvision);
                                }
                            }

                        }
                    }
                }
                if (0 < lstNotionalAmountProvision.Count)
                    return (NotionalAmountProvision[])lstNotionalAmountProvision.ToArray();
                return null;
            }
        }
        #endregion GetNotionalAmount
        #region IsAuthorizedToExerciseProvision
        private bool IsAuthorizedToExerciseProvision(DateTime pDate)
        {
            TradeActionEvent[] exerciseProvisions = CurrentEventParent.EventExerciseProvisionEffect(m_Event.eventCode);
            if (null != exerciseProvisions)
            {
                foreach (TradeActionEvent item in exerciseProvisions)
                {
                    if (null != item.eventClass)
                    {
                        foreach (EventClass itemClass in item.eventClass)
                        {
                            if (0 <= itemClass.dtEvent.DateValue.CompareTo(pDate))
                                return false;
                        }
                    }
                }
                return true;
            }
            return true;
        }
        #endregion IsAuthorizedToExerciseProvision
        #region RelevantUnderlyingDates
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected EFS_AdjustableDates RelevantUnderlyingDates
        {
            get
            {
                IProduct product = (IProduct)CurrentProduct;
                IAdjustableOrRelativeDates relevantUnderlyingDate = null;
                EFS_AdjustableDates adjustedRelevantDate = null;
                object exercise = null;
                #region Is RelevantUnderlyingDate specified
                if (IsEuropean)
                {
                    IEuropeanExercise europeanExercise = EuropeanExerciseProvision;
                    if ((null != exercise) && europeanExercise.RelevantUnderlyingDateSpecified)
                        relevantUnderlyingDate = europeanExercise.RelevantUnderlyingDate;
                }
                else if (IsAmerican)
                {
                    IAmericanExercise americanExercise = AmericanExerciseProvision;
                    if ((null != exercise) && americanExercise.RelevantUnderlyingDateSpecified)
                        relevantUnderlyingDate = americanExercise.RelevantUnderlyingDate;
                }
                else if (IsBermuda)
                {
                    IBermudaExercise bermudaExercise = BermudaExerciseProvision;
                    if ((null != exercise) && bermudaExercise.RelevantUnderlyingDateSpecified)
                        relevantUnderlyingDate = bermudaExercise.RelevantUnderlyingDate;
                }
                #endregion Is RelevantUnderlyingDate specified

                if (null != relevantUnderlyingDate)
                {
                    adjustedRelevantDate = new EFS_AdjustableDates();

                    IBusinessDayAdjustments bda;
                    if (relevantUnderlyingDate.AdjustableDatesSpecified)
                    {
                        #region AdjustableDates
                        IAdjustableDates adjustableRelevantDates = relevantUnderlyingDate.AdjustableDates;
                        adjustedRelevantDate.adjustableDates = new EFS_AdjustableDate[adjustableRelevantDates.UnadjustedDate.Length];
                        bda = adjustableRelevantDates.DateAdjustments;
                        for (int i = 0; i < adjustableRelevantDates.UnadjustedDate.Length; i++)
                        {
                            adjustedRelevantDate.adjustableDates[i] = new EFS_AdjustableDate(SessionTools.CS, adjustableRelevantDates[i],
                                bda, m_Event.CurrentTradeAction.dataDocumentContainer);
                        }
                        #endregion AdjustableDates
                    }
                    else if (relevantUnderlyingDate.RelativeDatesSpecified)
                    {
                        #region RelativeDates
                        DateTime[] offsetDates;
                        IRelativeDates adjustableRelevantDates = relevantUnderlyingDate.RelativeDates;
                        IOffset offset = adjustableRelevantDates.GetOffset;
                        bda = adjustableRelevantDates.GetAdjustments;
                        if (IsImplicitProvision && m_CashSettlementPaymentDateListSpecified)
                        {
                            adjustedRelevantDate.adjustableDates = new EFS_AdjustableDate[m_LstCashSettlementPaymentDate.Count];
                            offsetDates = new DateTime[m_LstCashSettlementPaymentDate.Count];
                            int i = 0;

                            foreach (string item in m_LstCashSettlementPaymentDate.Values)
                            {
                                DateTime dtCashSettlementPaymentDate = new DtFunc().StringToDateTime(item).Date;
                                offsetDates[i] = Tools.ApplyOffset(SessionTools.CS, dtCashSettlementPaymentDate, offset, bda, m_Event.CurrentTradeAction.dataDocumentContainer);
                                adjustedRelevantDate.adjustableDates[i] = new EFS_AdjustableDate(SessionTools.CS, m_Event.CurrentTradeAction.dataDocumentContainer)
                                {
                                    adjustedDate = product.ProductBase.CreateAdjustedDate((DateTime)offsetDates[i])
                                };
                            }
                        }
                        else
                        {
                            if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(SessionTools.CS, adjustableRelevantDates, out offsetDates, m_Event.CurrentTradeAction.dataDocumentContainer))
                            {
                                DateTime startBound = offsetDates[0];
                                DateTime endBound = offsetDates[offsetDates.Length - 1];
                                if (adjustableRelevantDates.ScheduleBoundsSpecified)
                                {
                                    EFS_AdjustableDate adjustableStartBound = new EFS_AdjustableDate(SessionTools.CS, adjustableRelevantDates.ScheduleBoundsUnadjustedFirstDate,
                                        adjustableRelevantDates.GetAdjustments, m_Event.CurrentTradeAction.dataDocumentContainer);
                                    EFS_AdjustableDate adjustableEndBound = new EFS_AdjustableDate(SessionTools.CS, adjustableRelevantDates.ScheduleBoundsUnadjustedLastDate,
                                        adjustableRelevantDates.GetAdjustments, m_Event.CurrentTradeAction.dataDocumentContainer);
                                    startBound = adjustableStartBound.adjustedDate.DateValue;
                                    endBound = adjustableEndBound.adjustedDate.DateValue;
                                }
                                ArrayList aFinalDtOffset = new ArrayList();
                                foreach (DateTime dt in offsetDates)
                                {
                                    if ((0 <= dt.CompareTo(startBound)) && (0 <= endBound.CompareTo(dt)))
                                    {
                                        aFinalDtOffset.Add(Tools.ApplyOffset(SessionTools.CS, dt, offset, bda, m_Event.CurrentTradeAction.dataDocumentContainer));
                                    }
                                }
                                adjustedRelevantDate.adjustableDates = new EFS_AdjustableDate[aFinalDtOffset.Count];
                                for (int i = 0; i < aFinalDtOffset.Count; i++)
                                {
                                    adjustedRelevantDate.adjustableDates[i] = new EFS_AdjustableDate(SessionTools.CS, m_Event.CurrentTradeAction.dataDocumentContainer)
                                    {
                                        adjustedDate = product.ProductBase.CreateAdjustedDate((DateTime)aFinalDtOffset[i])
                                    };
                                }
                            }
                        }
                        #endregion RelativeDates
                    }
                }
                return adjustedRelevantDate;
            }
        }
        #endregion CalculRelevantUnderlyingDates
        #region ReadAmountAuthorization
        protected void ReadAmountAuthorization()
        {
            IProduct product = (IProduct)CurrentProduct;
            if (product.ProductBase.IsSwap)
                ReadAmountAuthorization(((ISwap)product).Stream, "swapStream");
            else if (product.ProductBase.IsCapFloor)
                ReadAmountAuthorization(((ICapFloor)product).StreamInArray, "capFloorStream");
            else if (product.ProductBase.IsLoanDeposit)
                ReadAmountAuthorization(((ILoanDeposit)product).Stream, "loanDepositStream");
        }
        protected void ReadAmountAuthorization(IInterestRateStream[] pStreams, string pStreamName)
        {
            IReference[] notionalReference = null;
            decimal integralMultipleAmount = 1;
            decimal minimumNotionalAmount = 0;
            decimal maximumNotionalAmount = 0;

            #region ReadNotionalReference and affect amount authorization to NotionalAmountProvision attached
            if (m_PartialExerciseSpecified)
            {
                notionalReference = m_PartialExercise.NotionalReference;
                minimumNotionalAmount = m_PartialExercise.MinimumNotionalAmount.DecValue;
                if (m_PartialExercise.IntegralMultipleAmountSpecified)
                    integralMultipleAmount = m_PartialExercise.IntegralMultipleAmount.DecValue;
            }
            else if (m_MultipleExerciseSpecified)
            {
                notionalReference = m_MultipleExercise.NotionalReference;
                minimumNotionalAmount = m_MultipleExercise.MinimumNotionalAmount.DecValue;
                if (m_MultipleExercise.IntegralMultipleAmountSpecified)
                    integralMultipleAmount = m_MultipleExercise.IntegralMultipleAmount.DecValue;
                if (m_MultipleExercise.MaximumNotionalAmountSpecified)
                    maximumNotionalAmount = m_MultipleExercise.MaximumNotionalAmount.DecValue;
            }
            foreach (IReference notional in notionalReference)
            {
                foreach (IInterestRateStream item in pStreams)
                {
                    if (null != ReflectionTools.GetObjectById(item, notional.HRef))
                    {
                        if (StrFunc.IsFilled(item.Id))
                            m_StreamAmountSelected = item.Id.Replace(pStreamName, string.Empty);
                        else
                            m_StreamAmountSelected = "1";
                        foreach (NotionalAmountProvision nap in m_NotionalAmountProvision)
                        {
                            if ((nap.instrumentNo == m_Event.instrumentNo) && (nap.streamNo == m_StreamAmountSelected))
                            {
                                nap.SetAmountAuthorization(this, integralMultipleAmount, minimumNotionalAmount, maximumNotionalAmount);
                                nap.SetAmountReference(ValueDate.DateValue, m_StreamAmountSelected);
                            }
                        }
                    }
                }
            }
            #endregion ReadNotionalReference and affect amount authorization to NotionalAmountProvision attached

        }
        #endregion ReadAmountAuthorization
        #region Resource in Form
        #region ResFormTitleNotionalAmounts
        protected virtual string ResFormTitleNotionalAmounts { get { return Ressource.GetString("ProvisionNotionalAmounts"); } }
        #endregion ResFormTitleNotionalAmounts
        #region ResFormTitleNotionalAmountsDispo
        protected virtual string ResFormTitleNotionalAmountsDispo { get { return Ressource.GetString("ProvisionNotionalAmountsDispo"); } }
        #endregion ResFormTitleNotionalAmountsDispo
        #region ResFormValueDate
        protected override string ResFormValueDate { get { return Ressource.GetString("ProvisionDate"); } }
        #endregion ResFormValueDate
        #endregion Resource in Form
        #region SetCashSettlementInfo
        // 20080416 EG Ticket 16173
        protected void SetCashSettlementInfo()
        {
            SetCashSettlementInfo(actionDate.DateValue);
        }
        protected void SetCashSettlementInfo(DateTime pDate)
        {
            DateTime dtPayment = DateTime.MinValue;
            DateTime dtCurrent = pDate.Date;
            if (m_CashSettlementPaymentDateListSpecified)
            {
                foreach (string item in m_LstCashSettlementPaymentDate.Values)
                {
                    DateTime paymentDate = new DtFunc().StringToDateTime(item).Date;
                    if (DtFunc.IsDateTimeEmpty(dtPayment))
                    {
                        if (0 <= paymentDate.CompareTo(dtCurrent))
                            dtPayment = paymentDate;
                    }
                    else if ((0 <= paymentDate.CompareTo(dtCurrent)) && (0 <= dtPayment.CompareTo(paymentDate)))
                    {
                        dtPayment = paymentDate;
                    }
                };
            }
            SetCashSettlementInfo(m_CashSettlement, dtPayment);
        }
        protected static void SetCashSettlementInfo(CashSettlementAmountProvision pCashSettlementAmountProvision, DateTime pDate)
        {
            if (null != pCashSettlementAmountProvision)
            {
                pCashSettlementAmountProvision.resultPaymentDate = new EFS_Date
                {
                    DateValue = pDate
                };
            }
        }
        #endregion SetNotionalAmountSelected
        #region SetFeeAmountSelected
        protected void SetFeeAmountSelected()
        {
            SetFeeAmountSelected(m_FeeAmountProvision, ValueDate.DateValue, null);
        }
        protected void SetFeeAmountSelected(FeeAmountProvision[] pFeeAmountProvision, DateTime pDate, FpMLTextBox pControl)
        {
            DateTime dtCurrent = pDate.Date;
            if (null != pFeeAmountProvision)
            {
                FeeAmountProvision itemSelected = null;
                foreach (FeeAmountProvision item in pFeeAmountProvision)
                {
                    item.isSelected = false;
                    if ((null == itemSelected) ||
                        (item.feeAmount.feeScheduleDateSpecified && (0 <= DateTime.Compare(dtCurrent, item.feeAmount.feeScheduleDate.DateValue))))
                        itemSelected = item;
                }
                if (null != itemSelected)
                {
                    itemSelected.nominalStep = GetCurrentNominalStepForFee(itemSelected.notionalReference, dtCurrent);
                    itemSelected.nominalStepSpecified = (null != itemSelected.nominalStep);
                    itemSelected.isSelected = true;
                    if (itemSelected.nominalStepSpecified)
                    {
                        if (itemSelected.feeAmount.rateSpecified)
                            itemSelected.feeAmount.resultAmount.DecValue = itemSelected.nominalStep.Amount.DecValue * itemSelected.feeAmount.rate.DecValue;
                        else
                            itemSelected.feeAmount.resultAmount.DecValue = itemSelected.feeAmount.amount.DecValue;

                        if (null != pControl)
                        {
                            pControl.Text = itemSelected.feeAmount.resultAmount.CultureValue;
                        }
                    }
                }
            }
        }
        #endregion SetFeeAmountSelected
        #region SetNotionalAmountSelected
        protected void SetNotionalAmountSelected()
        {
            SetNotionalAmountSelected(false);
        }
        protected void SetNotionalAmountSelected(bool pIsEndIncluded)
        {
            SetNotionalAmountSelected(m_NotionalAmountProvision, ValueDate.DateValue, null, pIsEndIncluded);
        }
        protected void SetNotionalAmountSelected(NotionalAmountProvision[] pNotionalAmountProvision, DateTime pDate, Page pPage)
        {
            SetNotionalAmountSelected(pNotionalAmountProvision, pDate, pPage, false);
        }
        protected void SetNotionalAmountSelected(NotionalAmountProvision[] pNotionalAmountProvision, DateTime pDate, Page pPage, bool pIsEndIncluded)
        {
            DateTime dtCurrent = pDate.Date;
            if (null != pNotionalAmountProvision)
            {
                foreach (NotionalAmountProvision item in pNotionalAmountProvision)
                {
                    bool isOriginalSelected = item.isSelected;
                    //bool isOriginalReference = item.isNotionalReference;
                    item.SetAmountSelected(dtCurrent, pIsEndIncluded);
                    item.SetAmountReference(dtCurrent, m_StreamAmountSelected);

                    #region Update Control in page
                    if (null != pPage)
                    {
                        if ((isOriginalSelected != item.isSelected) && item.isSelected)
                        {
                            Control ctrl = pPage.FindControl("TXTAMOUNT_" + idE + "STREAM_" + item.streamNo);
                            if (null != ctrl)
                            {
                                ((FpMLTextBox)ctrl).Text = item.notionalStepAmount.Amount.CultureValue;
                            }
                        }
                    }
                    #endregion Update Control in page
                }
            }
        }
        #endregion SetNotionalAmountSelected

        #region SetProvision
        // EG 20150605 [21087] New
        public virtual void SetProvision()
        {
        }
        #endregion SetProvision
        #endregion Accessors
        #region Constructors
        public ProvisionEvents() { }
        // EG 20180205 [23769] Del EFS_TradeLibray 
        public ProvisionEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            
            //Affecte EFS_CURRENT
            //new EFS_TradeLibrary(TradeAction.DataDocument);
            // EG 20150605 [21087] use m_LstExerciseType
            SetProvision();
            IsProvisionExecuted = (null != m_Event.EventClassGroupLevel);

            #region Exercise dates
            ExerciseDateList();
            #endregion Exercise dates
            #region Partial/Multiple Exercise
            m_PartialExercise = PartialExerciseProvision;
            m_PartialExerciseSpecified = (null != m_PartialExercise); 
            m_MultipleExercise = MultipleExerciseProvision;
            m_MultipleExerciseSpecified = (null != m_MultipleExercise);
            #endregion Partial/Multiple Exercise
            #region Get NotionalAmount
            m_NotionalAmountProvision = GetNotionalAmount;
            #endregion Get NotionalAmount
            #region ExerciseType authorization
            m_ExerciseType = m_Event.eventType;
            // EG 20150605 [21087]
            // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
            //ExtendEnum extendEnums = ExtendEnumsTools.ListEnumsSchemes["ExerciseTypeEnum"];
            ExtendEnum extendEnums = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "ExerciseTypeEnum");
            if ((null != extendEnums) && (null != extendEnums.item))
            {
                m_LstExerciseType = new Dictionary<string, string>();
                foreach (ExtendEnumValue extendEnumValue in extendEnums.Sort("ExtValue"))
                {
                    if (null != extendEnumValue)
                    {
                        // EG 20150605 [21087] Add to dictionary
                        if (EventTypeFunc.IsPartiel(extendEnumValue.Value) && m_PartialExerciseSpecified)
                            m_LstExerciseType.Add(extendEnumValue.Value, extendEnumValue.ExtValue);
                        else if (EventTypeFunc.IsMultiple(extendEnumValue.Value) && m_MultipleExerciseSpecified)
                            m_LstExerciseType.Add(extendEnumValue.Value, extendEnumValue.ExtValue);
                        else if (EventTypeFunc.IsTotal(extendEnumValue.Value) && (null == CurrentEventParent.EventExerciseProvisionTotalEffect(pEvent.eventCode)))
                            m_LstExerciseType.Add(extendEnumValue.Value, extendEnumValue.ExtValue);
                    }
                }
            }
            #endregion ExerciseType authorization

            ValueDate = new EFS_Date();
            if (IsProvisionExecuted)
            {
                #region Fee/FeeSchedule
                m_FeeAmountProvision = GetFeeAmountExecuted;
                #endregion Fee/FeeSchedule
                #region Exercise Date & Time
                actionDate.DateValue = m_Event.dtEndPeriod.DateValue;
                actionTime.TimeValue = m_Event.dtEndPeriod.DateValue;
                #endregion Exercise Date & Time
                #region ValueDate
                ValueDate.DateValue = m_Event.EventClassGroupLevel.dtEvent.DateValue;
                #endregion ValueDate
                #region NotionalAmountCandidat
                SetNotionalAmountSelected(true);
                #endregion NotionalAmountCandidat
                #region Get CashSettlementAmount
                m_CashSettlement = GetCashSettlementAmountExecuted;
                m_CashSettlementSpecified = (null != m_CashSettlement);
                #endregion Get NotionalAmount
            }
            else
            {
                if (this.GetType().Equals(typeof(StepUpProvisionEvents)))
                {
                    if (m_MultipleExerciseSpecified)
                        m_ExerciseType = IsEuropean ? EventTypeFunc.Partiel : EventTypeFunc.Multiple;
                    else
                        m_Event.eventType = EventTypeFunc.Total;
                    m_Event.eventType = m_ExerciseType;
                }
                #region Fee/FeeSchedule
                m_FeeAmountProvision = GetFeeAmount;
                #endregion Fee/FeeSchedule
                #region ExerciseDate (ActionDate)
                ExpiryDate = m_EventParent.dtEndPeriod.DateValue;
                if (IsMandatoryEarlyTermination)
                {
                    #region Like Exercise European at Termination Date
                    actionDate.DateValue = ExpiryDate;
                    actionTime.TimeValue = actionDate.DateTimeValue;
                    #endregion Like Exercise European at Termination Date
                }
                else
                {
                    m_ExpirationTime = ExpirationTimeProvision;
                    #region Exercise Date & Time
                    if (IsEuropean)
                    {
                        #region European
                        actionDate.DateTimeValue = ExpiryDate;
                        actionTime.TimeValue = m_ExpirationTime.HourMinuteTime.TimeValue;
                        #endregion European
                    }
                    else
                    {
                        // FI 20200904 [XXXXX] Call OTCmlHelper.GetDateSys
                        //DateTime dtSysBusiness = OTCmlHelper.GetDateBusiness(SessionTools.CS);
                        DateTime dtSys = OTCmlHelper.GetDateSys(SessionTools.CS);
                        if (IsAmerican)
                        {
                            #region American
                            DateTime dtLastExerciseProvision = GetDtLastExerciseProvision;
                            if (0 < dtLastExerciseProvision.CompareTo(dtSys))
                                actionDate.DateTimeValue = dtLastExerciseProvision.AddDays(1);
                            else
                                actionDate.DateTimeValue = dtSys;
                            actionTime.TimeValue = actionDate.DateTimeValue;
                            if (0 < actionDate.DateValue.CompareTo(ExpiryDate))
                            {
                                actionDate.DateValue = ExpiryDate;
                                actionTime.TimeValue = m_ExpirationTime.HourMinuteTime.TimeValue;
                            }
                            #endregion American
                        }
                        else if (IsBermuda)
                        {
                            #region Bermuda
                            if ((null != m_LstExerciseDate) && (0 < m_LstExerciseDate.Count))
                            {
                                DateTime dtExercise = DateTime.MinValue;
                                foreach (string item in m_LstExerciseDate.Values)
                                {
                                    DateTime dtTemp = new DtFunc().StringToDateTime(item).Date;
                                    if (0 <= dtTemp.CompareTo(dtSys))
                                    {
                                        if (DtFunc.IsDateTimeEmpty(dtExercise) || (0 <= dtExercise.CompareTo(dtTemp)))
                                            dtExercise = dtTemp.Date;
                                    }
                                }
                                actionDate.DateValue = dtExercise;
                                actionTime.TimeValue = m_ExpirationTime.HourMinuteTime.TimeValue;
                            }
                            #endregion Bermuda
                        }
                    }
                    #endregion Exercise Date & Time
                }
                #endregion ExerciseDate (ActionDate)

                if (IsImplicitProvision)
                {
                    #region CashSettlementPaymentDate
                    CashSettlementPaymentDateList();
                    #endregion CashSettlementPaymentDate
                    #region RelevantUnderlyingDates (ValueDate)
                    RelevantUnderlyingDatesList();
                    ValueDate.DateValue = InitializeValueDate();
                    #endregion RelevantUnderlyingDates (ValueDate)
                }
                else
                {
                    #region RelevantUnderlyingDates (ValueDate)
                    RelevantUnderlyingDatesList();
                    ValueDate.DateValue = InitializeValueDate();
                    #endregion RelevantUnderlyingDates (ValueDate)
                    #region CashSettlementPaymentDate
                    CashSettlementPaymentDateList();
                    #endregion CashSettlementPaymentDate
                }
                #region Get CashSettlementAmount
                m_CashSettlement = GetCashSettlementAmount;
                m_CashSettlementSpecified = (null != m_CashSettlement);
                #endregion Get NotionalAmount

                #region NotionalAmountCandidat
                SetNotionalAmountSelected();
                #endregion NotionalAmountCandidat
                #region FeeAmountCandidat
                SetFeeAmountSelected();
                #endregion FeeAmountCandidat
                #region Amounts authorization
                if (m_PartialExerciseSpecified || m_MultipleExerciseSpecified)
                    ReadAmountAuthorization();
                #endregion Amounts autorisation

                #region SetCashSettlementInfo initialize
                SetCashSettlementInfo();
                #endregion SetCashSettlementInfo initialize
                #region Save for working
                m_NotionalAmountProvisionWork = CloneNotionalAmount(m_NotionalAmountProvision);
                if (null != m_FeeAmountProvision)
                    m_FeeAmountProvisionWork = CloneFeeAmount(m_FeeAmountProvision);
                #endregion Save for working
            }
        }
        #endregion Constructors
        #region Indexors
        public object this[Type pType, string pInstrumentNo, string pStreamNo, DateTime pDate]
        {
            get
            {
                if (pType.Equals(typeof(NotionalAmountProvision)))
                {
                    if (null != m_NotionalAmountProvision)
                    {
                        foreach (NotionalAmountProvision item in m_NotionalAmountProvision)
                        {
                            if ((item.instrumentNo == pInstrumentNo) && (item.streamNo == pStreamNo))
                            {
                                DateTime dtStart = item.dtStartPeriod.DateValue;
                                DateTime dtEnd = item.dtEndPeriod.DateValue;
                                if ((0 <= DateTime.Compare(pDate, dtStart)) && (0 < DateTime.Compare(dtEnd, pDate)))
                                    return item;
                            }
                        }
                    }
                }
                return null;
            }
        }
        #endregion Indexors
        #region Methods
        #region CalculProvisionExercise
        // 200900403 Eg Ticket 16540 : Gestion IsLock sur les Nominaux de stream exerçable (Exercice partiel)
        public virtual void CalculProvisionExercise(Page pPage, string pControlId)
        {
            FormatControl(pPage, pControlId);
            DateTime dtAction = DateTime.MinValue;
            DateTime dtValue = DateTime.MinValue;
            bool isReadOnly = true;
            Control ctrlId = pPage.FindControl(pControlId);
            TextBox txtCtrlId = ctrlId as TextBox;

            Control ctrl;
            #region GetExerciseType value
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
            {
                string exerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];
                isReadOnly = EventTypeFunc.IsTotal(exerciseType);
                // 200900403 Eg Ticket 16540 : Gestion IsLock sur les Nominaux de stream exerçable (Exercice partiel)
                #region Notional Amount are disabled if exercise size is Total
                foreach (NotionalAmountProvision item in m_NotionalAmountProvisionWork)
                {
                    if (item.isSelected)
                    {
                        ctrl = pPage.FindControl("TXTAMOUNT_" + idE + "STREAM_" + item.streamNo);
                        if (null != ctrl)
                        {
                            if (item.isNotionalReference)
                            {
                                FpMLTextBox txtCtrl = (FpMLTextBox)ctrl;
                                txtCtrl.IsLocked = isReadOnly;
                                txtCtrl.CssClass = isReadOnly ? EFSCssClass.CaptureConsult : EFSCssClass.Capture;
                            }
                        }
                    }
                }
                #endregion Notional Amount are disabled if exercise size is Total
            }
            #endregion GetExerciseType value
            #region ExerciseDate value
            if (null != pPage.Request.Form["DDLACTIONDATE_" + idE])
                dtAction = new DtFunc().StringToDateTime(pPage.Request.Form["DDLACTIONDATE_" + idE]);
            else if (null != pPage.Request.Form["TXTACTIONDATE_" + idE])
            {
                if (("TXTACTIONDATE_" + idE) == pControlId)
                {
                    if (null != txtCtrlId)
                        dtAction = new DtFunc().StringToDateTime(txtCtrlId.Text);
                }
                else
                    dtAction = new DtFunc().StringToDateTime(pPage.Request.Form["TXTACTIONDATE_" + idE]);
            }
            #endregion ExerciseDate value
            #region ValueDate
            if (null != pPage.Request.Form["TXTVALUEDATE_" + idE])
            {
                if (("TXTVALUEDATE_" + idE) == pControlId)
                {
                    if (null != txtCtrlId)
                        dtValue = new DtFunc().StringToDateTime(txtCtrlId.Text);
                }
                else
                    dtValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTVALUEDATE_" + idE]); ;
            }
            #endregion ValueDate

            if (("DDLEXERCISETYPE_" + idE) == pControlId)
            {
                #region Notional Amount are disabled if exercise size is Total
                foreach (NotionalAmountProvision item in m_NotionalAmountProvisionWork)
                {
                    if (item.isSelected)
                    {
                        ctrl = pPage.FindControl("TXTAMOUNT_" + idE + "STREAM_" + item.streamNo);
                        if (null != ctrl)
                        {
                            FpMLTextBox txtCtrl = (FpMLTextBox)ctrl;
                            txtCtrl.Text = item.provisionNotionalStepAmount.Amount.CultureValue;
                            if (item.isNotionalReference)
                            {
                                txtCtrl.IsLocked = isReadOnly;
                                txtCtrl.CssClass = isReadOnly ? EFSCssClass.CaptureConsult : EFSCssClass.Capture;
                            }
                        }
                    }
                }
                #endregion Notional Amount are disabled if exercise size is Total
            }
            else if ((("DDLACTIONDATE_" + idE) == pControlId) || (("TXTACTIONDATE_" + idE) == pControlId))
            {
                #region Exercise date modification (-> search associated nominalStep & Fee)
                if (null != pPage.Request.Form[pControlId])
                {
                    #region ValueDate
                    if (IsImplicitProvision)
                    {
                        m_LstCashSettlementPaymentDate.Clear();
                        CashSettlementPaymentDateList(dtAction);
                        m_LstRelevantUnderlyingDate.Clear();
                        RelevantUnderlyingDatesList();
                    }
                    dtValue = InitializeValueDate(dtAction);
                    ctrl = pPage.FindControl("TXTVALUEDATE_" + idE);
                    if (null != ctrl)
                        ((FpMLCalendarBox)ctrl).Text = DtFunc.DateTimeToString(dtValue, DtFunc.FmtShortDate);
                    #endregion ValueDate
                    #region Notional Amount dependance
                    SetNotionalAmountSelected(m_NotionalAmountProvisionWork, dtValue, pPage);
                    #endregion Notional Amount dependance
                    #region Fee Amount
                    ctrl = pPage.FindControl("TXTFEEAMOUNT_" + idE);
                    SetFeeAmountSelected(m_FeeAmountProvisionWork, dtValue, (FpMLTextBox)ctrl);
                    #endregion Fee Amount dependance
                    #region CashSettlementPaymentDate
                    SetCashSettlementInfo(dtAction);
                    ctrl = pPage.FindControl("TXTCSHPAYMENTDATE_" + idE);
                    if (null != ctrl)
                        ((FpMLCalendarBox)ctrl).Text = DtFunc.DateTimeToString(m_CashSettlement.resultPaymentDate.DateValue, DtFunc.FmtShortDate);
                    #endregion CashSettlementPaymentDate

                }
                #endregion Exercise date modification (-> search associated nominalStep & Fee)
            }
            else if (("TXTVALUEDATE_" + idE) == pControlId)
            {
                #region Value date modification (-> search associated nominalStep & Fee)
                if (null != pPage.Request.Form[pControlId])
                {
                    #region Notional Amount dependance
                    SetNotionalAmountSelected(m_NotionalAmountProvisionWork, dtValue, pPage);
                    #endregion Notional Amount dependance
                    #region Fee Amount
                    ctrl = pPage.FindControl("TXTFEEAMOUNT_" + idE);
                    SetFeeAmountSelected(m_FeeAmountProvisionWork, dtValue, (FpMLTextBox)ctrl);
                    #endregion Fee Amount dependance
                }
                #endregion Value date modification (-> search associated nominalStep & Fee)
            }
            else if (pControlId.StartsWith("TXTAMOUNT_" + idE + "STREAM_"))
            {
                #region Notional Amount dependant modification (prorata)
                if (null != pPage.Request.Form[pControlId])
                {
                    EFS_Decimal amount = new EFS_Decimal(DecFunc.DecValue(txtCtrlId.Text, Thread.CurrentThread.CurrentCulture));
                    string streamNo = pControlId.Replace("TXTAMOUNT_" + idE + "STREAM_", string.Empty);
                    NotionalAmountProvision currentItem = (NotionalAmountProvision)this[typeof(NotionalAmountProvision),
                        m_Event.instrumentNo, streamNo, dtValue];

                    ctrl = pPage.FindControl(pControlId);
                    if ((null != ctrl) && (null != currentItem))
                    {
                        FpMLTextBox txtCtrl = (FpMLTextBox)ctrl;
                        txtCtrl.IsLocked = isReadOnly;
                        txtCtrl.CssClass = isReadOnly ? EFSCssClass.CaptureConsult : EFSCssClass.Capture;

                        foreach (NotionalAmountProvision item in m_NotionalAmountProvisionWork)
                        {
                            if (item.isSelected)
                            {
                                Control ctrl2 = pPage.FindControl("TXTAMOUNT_" + idE + "STREAM_" + item.streamNo);
                                if ((null != ctrl2) && (currentItem.streamNo != item.streamNo))
                                {
                                    EFS_Decimal amount2 = new EFS_Decimal(item.notionalStepAmount.Amount.DecValue * (amount.DecValue / currentItem.notionalStepAmount.Amount.DecValue));
                                    ((FpMLTextBox)ctrl2).Text = amount2.CultureValue;
                                }
                            }
                        }
                    }
                }
                #endregion Notional Amount dependant modification (prorata)
            }
        }
        #endregion CalculProvisionExercise
        #region CashSettlementPaymentDateList
        protected void CashSettlementPaymentDateList()
        {
            CashSettlementPaymentDateList(actionDate.DateValue);
        }
        // EG 20180514 [23812] Report
        protected void CashSettlementPaymentDateList(DateTime pDtAction)
        {
            m_LstCashSettlementPaymentDate = new Dictionary<string, string>();
            DateTime DtCashSettlementPaymentDate;
            if (null != m_ExerciseDates)
            {
                #region Is CashSettlementPaymentDate specified
                foreach (TradeActionEvent events in m_ExerciseDates)
                {
                    if (null != events.eventClass)
                    {
                        foreach (EventClass item in events.eventClass)
                        {
                            if (EventClassFunc.IsCashSettlementPaymentDate(item.code))
                            {
                                DtCashSettlementPaymentDate = item.dtEvent.DateValue;
                                if (false == m_LstCashSettlementPaymentDate.ContainsKey(DtFunc.DateTimeToStringDateISO(DtCashSettlementPaymentDate)))
                                    m_LstCashSettlementPaymentDate.Add(DtFunc.DateTimeToStringDateISO(DtCashSettlementPaymentDate),
                                                                        DtFunc.DateTimeToString(DtCashSettlementPaymentDate, DtFunc.FmtShortDate));
                            }
                        }
                    }
                }
                // 20081107 EG CashSettlementValuationDate reading
                if (0 == m_LstCashSettlementPaymentDate.Count)
                {
                    foreach (TradeActionEvent events in m_ExerciseDates)
                    {
                        if (null != events.eventClass)
                        {
                            foreach (EventClass item in events.eventClass)
                            {
                                if (EventClassFunc.IsCashSettlementValuationDate(item.code))
                                {
                                    DtCashSettlementPaymentDate = item.dtEvent.DateValue;
                                    if (false == m_LstCashSettlementPaymentDate.ContainsKey(DtFunc.DateTimeToStringDateISO(DtCashSettlementPaymentDate)))
                                        m_LstCashSettlementPaymentDate.Add(DtFunc.DateTimeToStringDateISO(DtCashSettlementPaymentDate),
                                                                            DtFunc.DateTimeToString(DtCashSettlementPaymentDate, DtFunc.FmtShortDate));
                                }
                            }
                        }
                    }
                }
                #endregion Is CashSettlementPaymentDate specified
            }

            if (0 == m_LstCashSettlementPaymentDate.Count)
            {
                #region Calcul (there aren't no eventclass CSP/CSV)
                EFS_AdjustableDates adjustedPaymentDate = CashSettlementPaymentDate(pDtAction);
                if ((null != adjustedPaymentDate) &&
                    (null != adjustedPaymentDate.adjustableDates) &&
                    (0 < adjustedPaymentDate.adjustableDates.Length))
                {
                    for (int i = 0; i < adjustedPaymentDate.adjustableDates.Length; i++)
                    {
                        if (null != adjustedPaymentDate.adjustableDates[i])
                        {
                            DtCashSettlementPaymentDate = adjustedPaymentDate.adjustableDates[i].AdjustedEventDate.DateValue;
                            m_LstCashSettlementPaymentDate.Add(DtFunc.DateTimeToStringDateISO(DtCashSettlementPaymentDate),
                                                             DtFunc.DateTimeToString(DtCashSettlementPaymentDate, DtFunc.FmtShortDate));
                        }
                    }
                }
                #endregion Calcul (there aren't no eventclass CSP)
            }
            m_CashSettlementPaymentDateListSpecified = ((null != m_LstCashSettlementPaymentDate) && (0 < m_LstCashSettlementPaymentDate.Count));
        }
        #endregion CashSettlementPaymentDateList
        #region CreateControlFeeAmount
        protected TableRow CreateControlFeeAmount(FeeAmountProvision pItem)
        {
            bool isReadOnly = (false == pItem.isSelected);
            string key = "Amount";
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, ResFormAmount)
            {
                Regex = EFSRegex.TypeRegex.RegexAmountExtend,
                LblWidth = 100
            };

            FpMLTextBox txtAmount = new FpMLTextBox(null, pItem.feeAmount.resultAmount.CultureValue, 150, key,
                controlGUI, null, isReadOnly, "TXTFEEAMOUNT_" + idE, null,
                new Validator(key, true), new Validator(EFSRegex.TypeRegex.RegexAmountExtend, key, true, false));
            GetFormatControlAttribute(txtAmount);
            td.Controls.Add(txtAmount);
            td.Controls.Add(new LiteralControl(Cst.HTMLSpace + pItem.nominalStep.GetCurrency));
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlFeeAmount
        #region CreateControlFeePaymentDate
        protected TableRow CreateControlFeePaymentDate(FeeAmountProvision pItem)
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, ResFormPaymentDate)
            {
                Regex = EFSRegex.TypeRegex.RegexDate,
                LblWidth = 105
            };
            FpMLCalendarBox txtDate = new FpMLCalendarBox(null, pItem.feeAmount.resultPaymentDate.AdjustedEventDate.DateValue, "FeePaymentDate", controlGUI, null, "TXTFEEPAYMENTDATE_" + idE,
                new Validator("FeePaymentDate", true), new Validator(EFSRegex.TypeRegex.RegexDate, "FeePaymentDate", true, false));
            GetFormatControlAttribute(txtDate);
            td.Controls.Add(txtDate);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlFeePaymentDate
        #region CreateControlNotionalAmount
        protected TableRow CreateControlNotionalAmount(NotionalAmountProvision pItem, bool pIsReadOnly)
        {
            bool isReadOnly = (false == pItem.isNotionalReference) || pIsReadOnly;
            string key = "ProvisionNotionalAmountsDispo";
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, Ressource.GetString2("StreamNumber", pItem.streamNo))
            {
                Regex = EFSRegex.TypeRegex.RegexAmountExtend,
                LblWidth = 100
            };

            FpMLTextBox txtAmount = new FpMLTextBox(null, pItem.provisionNotionalStepAmount.Amount.CultureValue, 150, key,
                controlGUI, null, isReadOnly, "TXTAMOUNT_" + idE + "STREAM_" + pItem.streamNo, null,
                new Validator(key, true),
                new Validator(EFSRegex.TypeRegex.RegexAmountExtend, controlGUI.Name, true, false));
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtAmount.ID, TradeActionMode.TradeActionModeEnum.CalculProvisionExercise);
            txtAmount.Attributes.Add("onchange", sb.ToString());
            txtAmount.CssClass = isReadOnly ? EFSCssClass.CaptureConsult : EFSCssClass.Capture;
            td.Controls.Add(txtAmount);
            td.Controls.Add(new LiteralControl(Cst.HTMLSpace + pItem.provisionNotionalStepAmount.GetCurrency.Value));
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlNotionalAmount
        #region CreateTableHeader
        public virtual ArrayList CreateTableHeader(TradeActionEvent pEvent)
        {
            ArrayList aTableHeader = new ArrayList
            {
                pEvent.CurrentTradeAction.GetEventCodeTitle(pEvent.eventCode),
                CreateHeaderCells_Static,
                CreateHeaderCells_Capture,
                ResFormTitleComplementary
            };
            return aTableHeader;
        }
        #endregion CreateTableHeader
        #region ExerciseDateList
        protected void ExerciseDateList()
        {
            #region Exercise dates
            m_ExerciseDates = CurrentEventParent.EventExerciseDates;
            m_LstExerciseDate = new Dictionary<string,string>();
            if (null != m_ExerciseDates)
            {
                foreach (TradeActionEvent events in m_ExerciseDates)
                {
                    IsAmerican = EventTypeFunc.IsAmerican(events.eventType);
                    IsEuropean = EventTypeFunc.IsEuropean(events.eventType);
                    IsBermuda = EventTypeFunc.IsBermuda(events.eventType);

                    if (null != events.eventClass)
                    {
                        foreach (EventClass item in events.eventClass)
                        {
                            if (EventClassFunc.IsGroupLevel(item.code) && IsAuthorizedToExerciseProvision(item.dtEvent.DateValue))
                            {
                                EFS_Date dateExercise = new EFS_Date
                                {
                                    DateValue = item.dtEvent.DateValue
                                };
                                m_LstExerciseDate.Add(DtFunc.DateTimeToStringDateISO(dateExercise.DateValue), DtFunc.DateTimeToString(dateExercise.DateValue, DtFunc.FmtShortDate));
                            }
                        }
                    }
                }
            }
            #endregion Exercise dates
        }
        #region FormatControl
        // 200900403 Eg Ticket 16540 : Gestion IsLock sur les Nominaux de stream exerçable (Exercice partiel)
        public override void FormatControl(Page pPage, string pControlId)
        {
            base.FormatControl(pPage, pControlId);
            LockedNotional(pPage);
        }
        #endregion FormatControl
        #endregion ExerciseDateList
        #region InitializeValueDate
        protected DateTime InitializeValueDate()
        {
            return InitializeValueDate(actionDate.DateValue);
        }
        protected DateTime InitializeValueDate(DateTime pDate)
        {
            DateTime terminationDate = DateTime.MinValue;
            if (m_RelevantUnderlyingDateListSpecified)
            {
                foreach (string item in m_LstRelevantUnderlyingDate.Values)
                {
                    DateTime relevantDate = new DtFunc().StringToDateTime(item).Date;
                    if (DtFunc.IsDateTimeEmpty(terminationDate))
                    {
                        if (0 <= DateTime.Compare(relevantDate, pDate.Date))
                            terminationDate = relevantDate;
                    }
                    else if ((0 <= DateTime.Compare(relevantDate, pDate.Date)) && (0 <= DateTime.Compare(terminationDate, relevantDate.Date)))
                    {
                        terminationDate = relevantDate;
                    }
                }
            }
            else
                terminationDate = pDate;
            return terminationDate;
        }
        #endregion InitializeValueDate
        #region IsEventChanged
        //20090129 PL/EG Il faudra voir si on peut affiner pour voir une donnée a réellement été modifié par le user...
        public override bool IsEventChanged(TradeActionEventBase pEvent)
        {
            bool isEventChanged = base.IsEventChanged(pEvent);
            if (false == isEventChanged)
            {
                isEventChanged = true;
            }
            return isEventChanged;
        }
        #endregion IsEventChanged
        #region LockedNotional
        // 200900403 Eg Ticket 16540 : Gestion IsLock sur les Nominaux de stream exerçable (Exercice partiel)
        public virtual void LockedNotional(Page pPage)
        {
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
            {
                string exerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];
                bool isReadOnly = EventTypeFunc.IsTotal(exerciseType);
                foreach (NotionalAmountProvision item in m_NotionalAmountProvisionWork)
                {
                    if (item.isSelected)
                    {
                        Control ctrl = pPage.FindControl("TXTAMOUNT_" + idE + "STREAM_" + item.streamNo);
                        if (null != ctrl)
                        {
                            if (item.isNotionalReference)
                            {
                                FpMLTextBox txtCtrl = (FpMLTextBox)ctrl;
                                txtCtrl.IsLocked = isReadOnly;
                                txtCtrl.CssClass = isReadOnly ? EFSCssClass.CaptureConsult : EFSCssClass.Capture;
                            }
                        }
                    }
                }
            }
        }
        #endregion LockedNotional
        #region ProvisionElementsToPostMessage
        // EG 20150706 [21021] Refactoring Nullable<int> idA|idB
        protected void ProvisionElementsToPostMessage()
        {
            #region NotionalAmountProvision
            if (null != m_NotionalAmountProvision)
            {
                List<NotionalProvisionMsg> lstNotionalProvisionMsg = new List<NotionalProvisionMsg>();
                foreach (NotionalAmountProvision item in m_NotionalAmountProvision)
                {
                    if (item.isSelected)
                    {
                        NotionalProvisionMsg notionalProvisionEvent = new NotionalProvisionMsg
                        {
                            instrumentNo = item.instrumentNo,
                            streamNo = item.streamNo,
                            dtStartPeriod = item.dtStartPeriod.DateValue,
                            dtEndPeriod = item.dtEndPeriod.DateValue,
                            originalAmount = item.notionalStepAmount.Amount.DecValue,
                            originalCurrency = item.notionalStepAmount.Currency,
                            provisionAmount = item.provisionNotionalStepAmount.Amount.DecValue,
                            provisionCurrency = item.provisionNotionalStepAmount.Currency
                        };
                        lstNotionalProvisionMsg.Add(notionalProvisionEvent);
                    }
                }
                if (0 < lstNotionalProvisionMsg.Count)
                    m_PostNotionalProvisionMsg = (NotionalProvisionMsg[])lstNotionalProvisionMsg.ToArray();
            }
            #endregion NotionalAmountProvision
            #region FeeAmountProvision
            if (null != m_FeeAmountProvision)
            {
                foreach (FeeAmountProvision item in m_FeeAmountProvision)
                {
                    if (item.isSelected)
                    {
                        FeeProvisionMsg feeProvisionEvent = new FeeProvisionMsg
                        {
                            feePaymentDate = item.feeAmount.resultPaymentDate.AdjustedEventDate.DateValue,
                            amount = item.feeAmount.resultAmount.DecValue,
                            notionalReferenceSpecified = item.nominalStepSpecified,
                            feeRateSpecified = item.feeAmount.rateSpecified,
                            idA_PayerSpecified = item.payerReference.idA.HasValue,
                            idB_PayerSpecified = item.payerReference.idB.HasValue,
                            idA_ReceiverSpecified = item.receiverReference.idA.HasValue,
                            idB_ReceiverSpecified = item.receiverReference.idB.HasValue
                        };

                        if (feeProvisionEvent.notionalReferenceSpecified)
                        {
                            feeProvisionEvent.notionalReference = item.nominalStep.Amount.DecValue;
                            feeProvisionEvent.currency = item.nominalStep.Currency;
                            feeProvisionEvent.currencyReference = item.nominalStep.Currency;
                        }
                        if (feeProvisionEvent.feeRateSpecified)
                            feeProvisionEvent.feeRate = item.feeAmount.rate.DecValue;

                        if (feeProvisionEvent.idA_PayerSpecified)
                            feeProvisionEvent.idA_Payer = item.payerReference.idA.Value;

                        if (feeProvisionEvent.idB_PayerSpecified)
                            feeProvisionEvent.idB_Payer = item.payerReference.idB.Value;

                        if (feeProvisionEvent.idA_ReceiverSpecified)
                            feeProvisionEvent.idA_Receiver = item.receiverReference.idA.Value;

                        if (feeProvisionEvent.idB_ReceiverSpecified)
                            feeProvisionEvent.idB_Receiver = item.receiverReference.idB.Value;

                        m_PostFeeProvisionMsg = feeProvisionEvent;
                    }
                }
            }
            #endregion NotionalAmountProvision
            #region CashSettlementProvision
            if (m_CashSettlementSpecified)
            {
                m_PostCashSettlementProvisionMsg = new CashSettlementProvisionMsg
                {
                    cashSettlementPaymentDate = m_CashSettlement.resultPaymentDate.DateValue,
                    amount = m_CashSettlement.cashSettlementAmount.Amount.DecValue,
                    currency = m_CashSettlement.cashSettlementAmount.Currency,
                    idA_PayerSpecified = m_CashSettlement.payerReference.idA.HasValue,
                    idB_PayerSpecified = m_CashSettlement.payerReference.idB.HasValue,
                    idA_ReceiverSpecified = m_CashSettlement.receiverReference.idA.HasValue,
                    idB_ReceiverSpecified = m_CashSettlement.receiverReference.idB.HasValue
                };

                if (m_PostCashSettlementProvisionMsg.idA_PayerSpecified)
                    m_PostCashSettlementProvisionMsg.idA_Payer = m_CashSettlement.payerReference.idA.Value;

                if (m_PostCashSettlementProvisionMsg.idB_PayerSpecified)
                    m_PostCashSettlementProvisionMsg.idB_Payer = m_CashSettlement.payerReference.idB.Value;

                if (m_PostCashSettlementProvisionMsg.idA_ReceiverSpecified)
                    m_PostCashSettlementProvisionMsg.idA_Receiver = m_CashSettlement.receiverReference.idA.Value;

                if (m_PostCashSettlementProvisionMsg.idB_ReceiverSpecified)
                    m_PostCashSettlementProvisionMsg.idB_Receiver = m_CashSettlement.receiverReference.idB.Value;
                
            }
            #endregion NotionalAmountProvision
        }
        #endregion ProvisionElementsToPostMessage
        #region RelevantUnderlyingDatesList
        protected void RelevantUnderlyingDatesList()
        {
            #region RelevantUnderlyingDatesList
            m_LstRelevantUnderlyingDate = new Dictionary<string, string>();
            DateTime relevantUnderlyingDate;
            if (null != m_ExerciseDates)
            {
                #region Is RelevantUnderlyingDate specified
                foreach (TradeActionEvent events in m_ExerciseDates)
                {
                    if (null != events.eventClass)
                    {
                        foreach (EventClass item in events.eventClass)
                        {
                            if (EventClassFunc.IsRelevantUnderlyingDate(item.code))
                            {
                                relevantUnderlyingDate = item.dtEvent.DateValue;
                                // 20081107 EG 
                                string dtRUD = DtFunc.DateTimeToStringDateISO(relevantUnderlyingDate);
                                if (false == m_LstRelevantUnderlyingDate.ContainsKey(dtRUD))
                                    m_LstRelevantUnderlyingDate.Add(dtRUD, DtFunc.DateTimeToString(relevantUnderlyingDate, DtFunc.FmtShortDate));
                            }
                        }
                    }
                }
                #endregion Is RelevantUnderlyingDate specified
            }

            if (0 == m_LstRelevantUnderlyingDate.Count)
            {
                #region Calcul (there aren't no eventclass RUD)
                EFS_AdjustableDates adjustedRelevantDate = RelevantUnderlyingDates;
                if (null != adjustedRelevantDate && (0 < adjustedRelevantDate.adjustableDates.Length))
                {
                    for (int i = 0; i < adjustedRelevantDate.adjustableDates.Length; i++)
                    {
                        relevantUnderlyingDate = adjustedRelevantDate.adjustableDates[i].AdjustedEventDate.DateValue;
                        m_LstRelevantUnderlyingDate.Add(DtFunc.DateTimeToStringDateISO(relevantUnderlyingDate),
                                                         DtFunc.DateTimeToString(relevantUnderlyingDate, DtFunc.FmtShortDate));
                    }
                }
                #endregion Calcul (there aren't no eventclass RUD)
            }
            m_RelevantUnderlyingDateListSpecified = ((null != m_LstRelevantUnderlyingDate) && (0 < m_LstRelevantUnderlyingDate.Count));
            #endregion RelevantUnderlyingDatesList
        }
        #endregion RelevantUnderlyingDatesList
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = base.Save(pPage);
            if (isOk)
            {
                #region Save VALUE PROVISION
                if (null != pPage.Request.Form["TXTVALUEDATE_" + idE])
                    ValueDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTVALUEDATE_" + idE]);
                #endregion Save VALUE PROVISION
                #region Save EXERCISE TYPE (TOT/MUL/PAR)
                if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
                {
                    m_ExerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];
                    m_Event.eventType = m_ExerciseType;
                }
                //else if (this.GetType().Equals(typeof(StepUpProvisionEvents)))
                //{
                //	if (m_MultipleExerciseSpecified)
                //		m_ExerciseType = m_IsEuropean?EventTypeFunc.Partiel:EventTypeFunc.Multiple;
                //	else
                //		m_Event.eventType = EventTypeFunc.Total;
                //	m_Event.eventType = m_ExerciseType;
                //}
                #endregion Save EXERCISE TYPE (TOT/MUL/PAR)
                #region Save Notional provision
                foreach (NotionalAmountProvision item in m_NotionalAmountProvisionWork)
                {
                    if (item.isSelected)
                    {
                        string id = "TXTAMOUNT_" + idE + "STREAM_" + item.streamNo;
                        if (null != pPage.Request.Form[id])
                            item.provisionNotionalStepAmount.Amount.DecValue = DecFunc.DecValue(pPage.Request.Form[id], Thread.CurrentThread.CurrentCulture);
                    }
                }
                m_NotionalAmountProvision = CloneNotionalAmount(m_NotionalAmountProvisionWork);
                #endregion Save Notional provision
                #region Save Fee provision
                if (null != m_FeeAmountProvisionWork)
                {
                    foreach (FeeAmountProvision item in m_FeeAmountProvisionWork)
                    {
                        if (item.isSelected)
                        {
                            string id = "TXTFEEAMOUNT_" + idE;
                            if (null != pPage.Request.Form[id])
                                item.feeAmount.resultAmount.DecValue = DecFunc.DecValue(pPage.Request.Form[id], Thread.CurrentThread.CurrentCulture);
                            id = "TXTFEEPAYMENTDATE_" + idE;
                            if (null != pPage.Request.Form[id])
                            {
                                IProduct product = (IProduct)CurrentProduct;
                                DateTime dt = new DtFunc().StringToDateTime(pPage.Request.Form[id]);
                                item.feeAmount.resultPaymentDate.adjustedDate = product.ProductBase.CreateAdjustedDate(dt);
                            }
                        }
                    }
                    m_FeeAmountProvision = CloneFeeAmount(m_FeeAmountProvisionWork);
                }
                #endregion Save Fee provision
                #region Save CashSettlement provision
                if (m_CashSettlementSpecified)
                {
                    #region Date
                    string id = "TXTCSHPAYMENTDATE_" + idE;
                    if (null != pPage.Request.Form[id])
                        m_CashSettlement.resultPaymentDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form[id]);
                    #endregion Date
                    #region Currency
                    id = "DDLCSHCURRENCY_" + idE;
                    if (null != pPage.Request.Form[id])
                        m_CashSettlement.cashSettlementAmount.Currency = pPage.Request.Form[id].ToString();
                    #endregion Currency
                    #region Amount
                    id = "TXTCSHAMOUNT_" + idE;
                    if (null != pPage.Request.Form[id])
                        m_CashSettlement.cashSettlementAmount.Amount.DecValue = DecFunc.DecValue(pPage.Request.Form[id], Thread.CurrentThread.CurrentCulture);
                    #endregion Amount
                    #region Payer/Receiver
                    id = "DDLCSHPayer_" + idE;
                    if (null != pPage.Request.Form[id])
                        m_CashSettlement.payerReference = (PartyInfoProvision)m_CashSettlement[pPage.Request.Form[id]];
                    id = "DDLCSHReceiver_" + idE;
                    if (null != pPage.Request.Form[id])
                        m_CashSettlement.receiverReference = (PartyInfoProvision)m_CashSettlement[pPage.Request.Form[id]];
                    #endregion Payer/Receiver
                }
                #endregion Save Fee provision
            }
            return isOk;
        }
        #endregion Save
        #region SynchronizePayerReceiver
        // 20080416 EG Ticket 16173
        public void SynchronizePayerReceiver(Page pPage, string pControlId)
        {
            Control ctrl = pPage.FindControl(pControlId);
            if (null != ctrl)
            {
                string identifier = pPage.Request.Form[pControlId];
                string idPayer = "DDLCSHPayer_" + idE.ToString();
                string idReceiver = "DDLCSHReceiver_" + idE.ToString();
                PartyInfoProvision newParty = (PartyInfoProvision)m_CashSettlement[identifier];
                ((FpMLDropDownList)ctrl).SetSelectedValue = identifier;
                Control ctrl2;
                if (idPayer == pControlId)
                {
                    m_CashSettlement.receiverReferenceWork = (PartyInfoProvision)m_CashSettlement.payerReferenceWork.Clone();
                    m_CashSettlement.payerReferenceWork = newParty;
                    ctrl2 = pPage.FindControl(idReceiver);
                    ((FpMLDropDownList)ctrl2).SetSelectedValue = m_CashSettlement.receiverReferenceWork.Identifier;

                }
                else if (idReceiver == pControlId)
                {
                    m_CashSettlement.payerReferenceWork = (PartyInfoProvision)m_CashSettlement.receiverReferenceWork.Clone();
                    m_CashSettlement.receiverReferenceWork = newParty;
                    ctrl2 = pPage.FindControl(idPayer);
                    ((FpMLDropDownList)ctrl2).SetSelectedValue = m_CashSettlement.payerReferenceWork.Identifier;
                }
            }
        }
        #endregion SynchronizePayerReceiver
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = true;

            #region ExerciseType
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
                m_ExerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];
            string id;
            #endregion ExerciseType

            if (IsAmerican)
            {
                #region ActionDate
                id = "TXTACTIONDATE_" + idE;
                DateTime dtAction = new DtFunc().StringToDateTime(pPage.Request.Form[id]);
                EFS_Date actionMinDate = new EFS_Date
                {
                    DateValue = GetDtLastExerciseProvision
                };
                EFS_Date actionMaxDate = new EFS_Date
                {
                    DateValue = ExpiryDate
                };
                isOk = (0 <= dtAction.CompareTo(actionMinDate.DateValue)) && (0 <= actionMaxDate.DateValue.CompareTo(dtAction));
                if (false == isOk)
                    m_Event.validationRulesMessages.Add(Ressource.GetString2("Msg_IncorrectProvisionActionDate", actionMinDate.Value, actionMaxDate.Value));

                #endregion ActionDate
            }
            else if (IsBermuda)
            {
                #region ActionDate

                isOk = (0 < m_LstExerciseDate.Count);
                if (false == isOk)
                    m_Event.validationRulesMessages.Add("Msg_NoProvisionAuthorized");
                #endregion ActionDate
            }
            if (isOk)
            {
                #region NotionalAmountProvision
                foreach (NotionalAmountProvision item in m_NotionalAmountProvisionWork)
                {
                    if (item.isNotionalReference && (false == EventTypeFunc.IsTotal(m_ExerciseType)))
                    {
                        id = "TXTAMOUNT_" + idE + "STREAM_" + item.streamNo;
                        if (null != pPage.Request.Form[id])
                        {
                            decimal amount = DecFunc.DecValue(pPage.Request.Form[id], Thread.CurrentThread.CurrentCulture);
                            isOk = (item.minimumNotionalAmount.DecValue <= amount);
                            if (item.maximumNotionalAmountSpecified)
                                isOk = isOk && (amount <= item.maximumNotionalAmount.DecValue);
                            isOk = isOk && (0 == System.Math.IEEERemainder(Convert.ToDouble(amount), Convert.ToDouble(item.integralMultipleAmount.DecValue)));
                            if (false == isOk)
                            {
                                if (1 < item.integralMultipleAmount.DecValue)
                                {
                                    if (item.maximumNotionalAmountSpecified)
                                    {
                                        m_Event.validationRulesMessages.Add(
                                            Ressource.GetString2("Msg_IncorrectProvisionNotionalAmountSupp",
                                            item.minimumNotionalAmount.CultureValue, item.maximumNotionalAmount.CultureValue, item.integralMultipleAmount.CultureValue));
                                    }
                                    else
                                    {
                                        m_Event.validationRulesMessages.Add(
                                            Ressource.GetString2("Msg_IncorrectProvisionNotionalStepUpAmountSupp",
                                            item.minimumNotionalAmount.CultureValue, item.maximumNotionalAmount.CultureValue, item.integralMultipleAmount.CultureValue));
                                    }
                                }
                                else
                                {
                                    if (item.maximumNotionalAmountSpecified)
                                    {
                                        m_Event.validationRulesMessages.Add(
                                            Ressource.GetString2("Msg_IncorrectProvisionNotionalAmount",
                                            item.minimumNotionalAmount.CultureValue, item.maximumNotionalAmount.CultureValue));
                                    }
                                    else
                                    {
                                        m_Event.validationRulesMessages.Add(
                                            Ressource.GetString2("Msg_IncorrectProvisionNotionalStepUpAmount", item.minimumNotionalAmount.CultureValue));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion NotionalAmountProvision
            }
            if (isOk && (null != m_FeeAmountProvisionWork))
            {
                #region FeeAmountProvision
                foreach (FeeAmountProvision item in m_FeeAmountProvisionWork)
                {
                    if (item.isSelected)
                    {
                        id = "TXTFEEAMOUNT_" + idE;
                        if (null != pPage.Request.Form[id])
                            isOk = (0 < DecFunc.DecValue(pPage.Request.Form[id], Thread.CurrentThread.CurrentCulture));
                        else
                            isOk = (0 < item.feeAmount.resultAmount.DecValue);

                        if (false == isOk)
                            m_Event.validationRulesMessages.Add(Ressource.GetString2("Msg_IncorrectProvisionFeeAmount"));
                        break;
                    }
                }
                #endregion FeeAmountProvision
            }
            if (isOk)
            {
                #region CashSettlementAmountProvision
                id = "TXTCSHAMOUNT_" + idE;
                if (null != pPage.Request.Form[id])
                    isOk = (0 < DecFunc.DecValue(pPage.Request.Form[id], Thread.CurrentThread.CurrentCulture));
                else if (m_CashSettlementSpecified)
                    isOk = (0 < m_CashSettlement.cashSettlementAmount.Amount.DecValue);
                if (false == isOk)
                    m_Event.validationRulesMessages.Add(Ressource.GetString2("Msg_IncorrectProvisionCashSettlementAmount"));
                #endregion CashSettlementAmountProvision
            }
            if (isOk)
            {
                #region CashSettlementCurrencyProvision
                id = "DDLCSHCURRENCY_" + idE;
                if (null != pPage.Request.Form[id])
                    isOk = (StrFunc.IsFilled(pPage.Request.Form[id].ToString()));
                else if (m_CashSettlementSpecified)
                    isOk = (StrFunc.IsFilled(m_CashSettlement.cashSettlementAmount.Currency));
                if (false == isOk)
                    m_Event.validationRulesMessages.Add(Ressource.GetString2("Msg_IncorrectProvisionCashSettlementCurrency"));
                #endregion CashSettlementCurrencyProvision
            }
            if (isOk)
                isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules
        #endregion Methods


        #region Virtual Methods
        #endregion Virtual Methods 
    }
    #endregion ProvisionEvents

    #region CashSettlementAmountProvision
    // 20080416 EG Ticket 16173
    public class CashSettlementAmountProvision
    {
        #region Members
        public string instrumentNo;
        public PartyInfoProvision[] partyInfo;
        public Dictionary<string, string> partyInfoGUI;
        public PartyInfoProvision payerReference;
        public PartyInfoProvision receiverReference;
        public EFS_Decimal cashSettlementRate;
        public bool cashSettlementRateSpecified;
        public IMoney cashSettlementAmount;
        public ICashSettlement cashSettlement;
        public EFS_Date resultPaymentDate;

        public EFS_AdjustableDate[] cashSettlementPaymentDate;
        public bool cashSettlementPaymentDateSpecified;
        public EFS_AdjustableDate[] cashSettlementValuationDate;
        public IBusinessCenterTime cashSettlementValuationTime;
        // 20080416 EG Ticket 16173
        public PartyInfoProvision payerReferenceWork;
        public PartyInfoProvision receiverReferenceWork;

        #endregion Members
        #region Constructors
        public CashSettlementAmountProvision() { }
        public CashSettlementAmountProvision(TradeActionEventBase pEvent, ICashSettlement pCashSettlement)
        {
            instrumentNo = pEvent.instrumentNo;
            cashSettlement = pCashSettlement;
            Calc(pEvent);
        }
        public CashSettlementAmountProvision(TradeActionEventBase pEventCashSettlement)
        {
            instrumentNo = pEventCashSettlement.instrumentNo;
            EventClass eventClass = pEventCashSettlement.EventClassSettlement;
            if (null != eventClass)
            {
                resultPaymentDate = new EFS_Date
                {
                    DateValue = eventClass.dtEvent.DateValue
                };
            }
            payerReference = new PartyInfoProvision(pEventCashSettlement.idPayer, pEventCashSettlement.idPayerBook, pEventCashSettlement);
            receiverReference = new PartyInfoProvision(pEventCashSettlement.idReceiver, pEventCashSettlement.idReceiverBook, pEventCashSettlement);
            IProduct product = (IProduct)pEventCashSettlement.CurrentProduct(instrumentNo);
            cashSettlementAmount = product.ProductBase.CreateMoney(pEventCashSettlement.valorisation.DecValue, pEventCashSettlement.unit);
        }
        #endregion Constructors
        #region Indexors
        public object this[IReference pPartyReference]
        {
            get
            {
                if (null != partyInfo)
                {
                    foreach (PartyInfoProvision item in partyInfo)
                    {
                        if (item.partyReference == pPartyReference.HRef)
                            return item;
                    }
                }
                return null;
            }
        }
        // 20080416 EG Ticket 16173
        public object this[string pIdentifier]
        {
            get
            {
                if (null != partyInfo)
                {
                    foreach (PartyInfoProvision item in partyInfo)
                    {
                        if (item.Identifier == pIdentifier)
                            return item;
                    }
                }
                return null;
            }
        }

        #endregion Indexors
        #region Methods
        #region Add
        // 20080416 EG Ticket 16173
        public void Add(IReference pPartyReference, TradeActionEventBase pEvent)
        {
            ArrayList aPartyInfo = new ArrayList();
            if (null != partyInfo)
            {
                foreach (PartyInfoProvision item in partyInfo)
                {
                    aPartyInfo.Add(item);
                }
            }
            #region New Party
            PartyInfoProvision newParty = new PartyInfoProvision(pPartyReference, pEvent);
            aPartyInfo.Add(newParty);
            if (null == partyInfoGUI)
                partyInfoGUI = new Dictionary<string, string>();
            partyInfoGUI.Add(newParty.Identifier, newParty.Identifier);
            if (null == payerReference)
            {
                payerReference = (PartyInfoProvision)newParty.Clone();
                payerReferenceWork = (PartyInfoProvision)newParty.Clone();
            }
            else if (null == receiverReference)
            {
                receiverReference = (PartyInfoProvision)newParty.Clone();
                receiverReferenceWork = (PartyInfoProvision)newParty.Clone();
            }
            #endregion New Party
            if (0 < aPartyInfo.Count)
                partyInfo = (PartyInfoProvision[])aPartyInfo.ToArray(typeof(PartyInfoProvision));
        }
        #endregion Add
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel Calc(TradeActionEventBase pEvent)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            DateTime[] offsetDate;
            #region CashSettlementPaymentDate
            if (cashSettlement.PaymentDateSpecified)
            {
                if (cashSettlement.PaymentDate.AdjustableDatesSpecified)
                {
                    #region AdjustableDates
                    IAdjustableDates adjustableDates = cashSettlement.PaymentDate.AdjustableDates;
                    cashSettlementPaymentDate = new EFS_AdjustableDate[adjustableDates.UnadjustedDate.Length];
                    for (int i = 0; i < adjustableDates.UnadjustedDate.Length; i++)
                    {
                        cashSettlementPaymentDate[i] = new EFS_AdjustableDate(SessionTools.CS, adjustableDates[i], 
                            adjustableDates.DateAdjustments, pEvent.CurrentTradeAction.dataDocumentContainer);
                    }
                    #endregion AdjustableDates
                }
                else if (cashSettlement.PaymentDate.RelativeDateSpecified)
                {
                    #region RelativeDate
                    IRelativeDateOffset relativeDateOffset = cashSettlement.PaymentDate.RelativeDate;
                    if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(SessionTools.CS, relativeDateOffset, out offsetDate, pEvent.CurrentTradeAction.dataDocumentContainer))
                    {
                        cashSettlementPaymentDate = new EFS_AdjustableDate[1];
                        cashSettlementPaymentDate[0] = new EFS_AdjustableDate(SessionTools.CS, offsetDate[0], 
                            relativeDateOffset.GetAdjustments, pEvent.CurrentTradeAction.dataDocumentContainer);
                    }
                    #endregion RelativeDate
                }
                else if (cashSettlement.PaymentDate.BusinessDateRangeSpecified)
                {
                    #region BusinessDateRange
                    IBusinessDateRange dateRange = cashSettlement.PaymentDate.BusinessDateRange;
                    IBusinessDayAdjustments bda = dateRange.GetAdjustments;
                    cashSettlementPaymentDate = new EFS_AdjustableDate[2];
                    cashSettlementPaymentDate[0] = new EFS_AdjustableDate(SessionTools.CS, dateRange.UnadjustedFirstDate.DateValue, 
                        bda, pEvent.CurrentTradeAction.dataDocumentContainer);
                    cashSettlementPaymentDate[1] = new EFS_AdjustableDate(SessionTools.CS, dateRange.UnadjustedLastDate.DateValue, 
                        bda, pEvent.CurrentTradeAction.dataDocumentContainer);
                    #endregion BusinessDateRange
                }
            }
            cashSettlementPaymentDateSpecified = cashSettlement.PaymentDateSpecified;
            #endregion CashSettlementPaymentDate
            #region CashSettlementValuationDate
            if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(SessionTools.CS, cashSettlement.ValuationDate, out offsetDate, pEvent.CurrentTradeAction.dataDocumentContainer))
            {
                cashSettlementValuationDate = new EFS_AdjustableDate[offsetDate.Length];
                for (int i = 0; i < offsetDate.Length; i++)
                {
                    cashSettlementValuationDate[i] = new EFS_AdjustableDate(SessionTools.CS, offsetDate[i], 
                        cashSettlement.ValuationDate.GetAdjustments, pEvent.CurrentTradeAction.dataDocumentContainer);
                }
            }
            #endregion CashSettlementValuationDate
            #region CashSettlementValuationTime
            cashSettlementValuationTime = cashSettlement.ValuationTime;
            #endregion CashSettlementValuationTime

            #region ResultPaymentDate
            resultPaymentDate = new EFS_Date();
            #endregion ResultPaymentDate
            #region Amount
            IProduct product = (IProduct)pEvent.CurrentProduct(instrumentNo);
            cashSettlementAmount = product.ProductBase.CreateMoney();
            #endregion ResultPaymentDate
            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion CashSettlementAmountProvision
    #region FeeAmountInfo
    public class FeeAmountInfo
    {
        #region Members
        public EFS_Decimal amount;
        public bool amountSpecified;
        public EFS_Decimal rate;
        public bool rateSpecified;
        public IRelativeDateOffset feePaymentDate;
        public EFS_Date feeScheduleDate;
        public bool feeScheduleDateSpecified;

        public EFS_Decimal resultAmount;
        public EFS_AdjustableDate resultPaymentDate;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public FeeAmountInfo(IExerciseFee pExerciseFee, DataDocumentContainer pDataDocument) : this(pExerciseFee, DateTime.MinValue, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public FeeAmountInfo(IExerciseFee pExerciseFee, DateTime pScheduleDate, DataDocumentContainer pDataDocument)
        {
            amountSpecified = pExerciseFee.FeeAmountSpecified;
            rateSpecified = pExerciseFee.FeeRateSpecified;
            feeScheduleDateSpecified = DtFunc.IsDateTimeFilled(pScheduleDate);

            if (amountSpecified)
                amount = pExerciseFee.FeeAmount;
            if (rateSpecified)
                rate = pExerciseFee.FeeRate;

            if (feeScheduleDateSpecified)
            {
                feeScheduleDate = new EFS_Date
                {
                    DateValue = pScheduleDate
                };
            }
            resultAmount = new EFS_Decimal();

            feePaymentDate = pExerciseFee.FeePaymentDate;
            if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(SessionTools.CS, feePaymentDate, out DateTime offsetDate, pDataDocument))
                resultPaymentDate = new EFS_AdjustableDate(SessionTools.CS, offsetDate, feePaymentDate.GetAdjustments, pDataDocument);
            else
                resultPaymentDate = new EFS_AdjustableDate(SessionTools.CS, pDataDocument);
        }
        public FeeAmountInfo(TradeActionEvent pTradeActionEvent)
        {
            IProduct product = (IProduct)pTradeActionEvent.CurrentProduct(pTradeActionEvent.instrumentNo);
            rateSpecified = (pTradeActionEvent.detailsSpecified && pTradeActionEvent.details.rateSpecified);
            if (rateSpecified)
                rate = new EFS_Decimal(pTradeActionEvent.details.rate.DecValue);

            amountSpecified = (false == rateSpecified);
            if (amountSpecified)
                amount = new EFS_Decimal(pTradeActionEvent.valorisation.DecValue);

            resultAmount = new EFS_Decimal(pTradeActionEvent.valorisation.DecValue);
            resultPaymentDate = new EFS_AdjustableDate(SessionTools.CS, pTradeActionEvent.currentTradeAction.dataDocumentContainer)
            {
                adjustedDate = product.ProductBase.CreateAdjustedDate(pTradeActionEvent.dtEndPeriod.DateValue)
            };
        }
        #endregion Constructors
    }
    #endregion FeeAmountInfo
    #region FeeAmountProvision
    public class FeeAmountProvision : ICloneable
    {
        #region Members
        public bool isSelected;
        public PartyInfoProvision payerReference;
        public PartyInfoProvision receiverReference;
        public string notionalReference;
        public bool notionalReferenceSpecified;
        public IMoney nominalStep;
        public bool nominalStepSpecified;
        public FeeAmountInfo feeAmount;
        #endregion Members
        #region Constructors
        public FeeAmountProvision() { }
        public FeeAmountProvision(PartyInfoProvision pFeePayer, PartyInfoProvision pFeeReceiver, string pNotionalReference, FeeAmountInfo pFeeAmountInfo)
        {
            payerReference = pFeePayer;
            receiverReference = pFeeReceiver;
            notionalReference = pNotionalReference;
            feeAmount = pFeeAmountInfo;
        }
        public FeeAmountProvision(PartyInfoProvision pFeePayer, PartyInfoProvision pFeeReceiver, FeeAmountInfo pFeeAmountInfo, IMoney pNominalStep)
        {
            payerReference = pFeePayer;
            receiverReference = pFeeReceiver;
            feeAmount = pFeeAmountInfo;
            nominalStep = pNominalStep.Clone();
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            FeeAmountProvision clone = new FeeAmountProvision
            {
                isSelected = isSelected,
                payerReference = (PartyInfoProvision)payerReference.Clone(),
                receiverReference = (PartyInfoProvision)receiverReference.Clone(),
                notionalReference = notionalReference,
                feeAmount = feeAmount,
                nominalStepSpecified = nominalStepSpecified
            };
            if (nominalStepSpecified)
                clone.nominalStep = nominalStep.Clone();
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion FeeAmountProvision
    #region PartyInfoProvision
    // 20080416 EG Ticket 16173
    // EG 20150706 [21021] Nullable<int> (idA|idB)
    public class PartyInfoProvision : ICloneable
    {
        #region Members
        public string partyReference;
        // EG 20150706 [21021]
        public Nullable<int> idA;
        public string partyIdentifier;
        public string partyDisplayName;
        // EG 20150706 [21021]
        public Nullable<int> idB;
        public string bookIdentifier;
        public bool bookIdentifierSpecified;
        public string bookDisplayName;
        #endregion Members
        #region Accessors
        #region Identifier
        // 20080416 EG Ticket 16173
        public string Identifier
        {
            get
            {
                string identifier = partyIdentifier;
                if (bookIdentifierSpecified)
                    identifier += " [" + bookIdentifier + "]";
                return identifier;
            }
        }
        #endregion Identifier
        #endregion Accessors
        #region Constructors
        public PartyInfoProvision() { }
        // EG 20150706 [21021] Refactoring Call SetPartyAndBook
        public PartyInfoProvision(IReference pPartyReference, TradeActionEventBase pEvent)
        {
            idA = pEvent.CurrentTradeAction.dataDocumentContainer.GetOTCmlId_Party(pPartyReference.HRef);
            idB = pEvent.CurrentTradeAction.dataDocumentContainer.GetOTCmlId_Book(pPartyReference.HRef);
            SetPartyAndBook();
        }
        // EG 20150706 [21021] Refactoring Call SetPartyAndBook
        public PartyInfoProvision(Nullable<int> pIdA, Nullable<int> pIdB, TradeActionEventBase pEvent)
        {
            idA = pIdA;
            idB = pIdB;
            SetPartyAndBook();
        }
        #endregion Constructors
        #region Methods
        #region Clone
        // 20080416 EG Ticket 16173
        public object Clone()
        {
            PartyInfoProvision clone = new PartyInfoProvision
            {
                partyReference = partyReference,
                idA = idA,
                partyIdentifier = partyIdentifier,
                partyDisplayName = partyDisplayName,
                idB = idB,
                bookIdentifier = bookIdentifier,
                bookIdentifierSpecified = bookIdentifierSpecified,
                bookDisplayName = bookDisplayName
            };
            return clone;
        }
        #endregion Clone
        #region SetPartyAndBook
        // EG 20150706 [21021] New
        // RD 20150814 [21253] Modify
        private void SetPartyAndBook()
        {
            if (idA.HasValue)
            {
                SQL_Actor actor = new SQL_Actor(SessionTools.CS, idA.Value);
                if (actor.IsLoaded)
                {
                    partyIdentifier = actor.Identifier;
                    partyDisplayName = actor.DisplayName;
                    partyReference = actor.XmlId;
                }
            }
            if (idB.HasValue)
            {
                SQL_Book book = new SQL_Book(SessionTools.CS, idB.Value);
                if (book.IsLoaded)
                {
                    bookIdentifier = book.Identifier;
                    bookDisplayName = book.FullName;
                }
            }
            bookIdentifierSpecified = StrFunc.IsFilled(bookIdentifier);
        }
        #endregion SetPartyAndBook
        #endregion Methods
    }
    #endregion PartyInfoProvision
    #region NotionalAmountProvision
    public class NotionalAmountProvision : ICloneable
    {
        #region Members
        public bool isSelected;
        public bool isNotionalReference;
        public string instrumentNo;
        public string streamNo;
        public EFS_Date dtStartPeriod;
        public EFS_Date dtEndPeriod;
        public IMoney notionalStepAmount;
        public IMoney provisionNotionalStepAmount;
        public IMoney variationAmountAtEndPeriod;
        public bool variationAmountAtEndPeriodSpecified;
        public bool isNominalVariationEventProvision;

        public EFS_Decimal integralMultipleAmount;
        public EFS_Decimal minimumNotionalAmount;
        public EFS_Decimal maximumNotionalAmount;
        public bool maximumNotionalAmountSpecified;


        #endregion Members
        #region Constructors
        public NotionalAmountProvision() { }
        public NotionalAmountProvision(TradeActionEvent pNominalStep)
        {
            //isSelected = false;
            //isNotionalReference = false;
            instrumentNo = pNominalStep.instrumentNo;
            streamNo = pNominalStep.streamNo;
            dtStartPeriod = new EFS_Date
            {
                DateValue = pNominalStep.dtStartPeriod.DateValue
            };
            dtEndPeriod = new EFS_Date
            {
                DateValue = pNominalStep.dtEndPeriod.DateValue
            };

            IProduct product = (IProduct)pNominalStep.CurrentProduct(instrumentNo);

            notionalStepAmount = product.ProductBase.CreateMoney(pNominalStep.valorisation.DecValue, pNominalStep.unit);
            if ((null != pNominalStep.m_Action) && (pNominalStep.m_Action.GetType().Equals(typeof(NominalStepEvents))))
            {
                TradeActionEvent nse = ((NominalStepEvents)pNominalStep.m_Action).nominalVariationEvent;
                if ((null != nse) && (nse.valorisationSpecified))
                {
                    isNominalVariationEventProvision = ((NominalStepEvents)pNominalStep.m_Action).isNominalVariationEventProvision;
                    variationAmountAtEndPeriod = product.ProductBase.CreateMoney(nse.valorisation.DecValue, nse.unit);
                    variationAmountAtEndPeriodSpecified = true;
                }
            }
            provisionNotionalStepAmount = product.ProductBase.CreateMoney(pNominalStep.valorisation.DecValue, pNominalStep.unit);
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            NotionalAmountProvision clone = new NotionalAmountProvision
            {
                isSelected = isSelected,
                isNotionalReference = isNotionalReference,
                instrumentNo = instrumentNo,
                streamNo = streamNo,
                dtStartPeriod = new EFS_Date() { DateValue = dtStartPeriod.DateValue },
                dtEndPeriod = new EFS_Date() { DateValue = dtEndPeriod.DateValue },
                notionalStepAmount = notionalStepAmount.Clone(),
                provisionNotionalStepAmount = provisionNotionalStepAmount.Clone(),
                isNominalVariationEventProvision = isNominalVariationEventProvision,
                variationAmountAtEndPeriodSpecified = variationAmountAtEndPeriodSpecified
            };
            if (null != integralMultipleAmount)
                clone.integralMultipleAmount = new EFS_Decimal(integralMultipleAmount.DecValue);
            if (null != minimumNotionalAmount)
                clone.minimumNotionalAmount = new EFS_Decimal(minimumNotionalAmount.DecValue);
            if (null != maximumNotionalAmount)
                clone.maximumNotionalAmount = new EFS_Decimal(maximumNotionalAmount.DecValue);
            if (variationAmountAtEndPeriodSpecified)
                clone.variationAmountAtEndPeriod = variationAmountAtEndPeriod.Clone();
            return clone;
        }
        #endregion Clone
        #region SetAmountAuthorization
        public void SetAmountAuthorization(ProvisionEvents pProvisionEvents, decimal pIntegralMultipleAmount, decimal pMinimumNotionalAmount, decimal pMaximumNotionalAmount)
        {
            integralMultipleAmount = new EFS_Decimal(pIntegralMultipleAmount);
            minimumNotionalAmount = new EFS_Decimal(System.Math.Min(pMinimumNotionalAmount, notionalStepAmount.Amount.DecValue));
            if (false == pProvisionEvents.GetType().Equals(typeof(StepUpProvisionEvents)))
                maximumNotionalAmount = new EFS_Decimal(notionalStepAmount.Amount.DecValue);
            if (0 < pMaximumNotionalAmount)
                maximumNotionalAmount = new EFS_Decimal(System.Math.Min(pMaximumNotionalAmount, maximumNotionalAmount.DecValue));
        }
        #endregion SetAmountAuthorization
        #region SetAmountReference
        public void SetAmountReference(DateTime pDate, string pStreamAmountSelected)
        {
            isNotionalReference = isSelected && (pStreamAmountSelected == streamNo);
        }
        #endregion SetAmountReference
        #region SetAmountSelected
        public void SetAmountSelected(DateTime pDate)
        {
            SetAmountSelected(pDate, false);
        }
        public void SetAmountSelected(DateTime pDate, bool pIsEndIncluded)
        {
            if (pIsEndIncluded)
                isSelected = ((0 < DateTime.Compare(pDate, dtStartPeriod.DateValue)) && (0 <= DateTime.Compare(dtEndPeriod.DateValue, pDate)));
            else
                isSelected = ((0 <= DateTime.Compare(pDate, dtStartPeriod.DateValue)) && (0 < DateTime.Compare(dtEndPeriod.DateValue, pDate)));
        }
        #endregion SetAmountSelected
        #endregion Methods
    }
    #endregion NotionalAmountProvision

}
