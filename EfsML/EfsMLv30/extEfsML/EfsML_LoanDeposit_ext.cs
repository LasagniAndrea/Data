#region using directives
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Interface;
using FpML.Interface;
using FpML.v44.Shared;
using System;
#endregion using directives


namespace EfsML.v30.LoanDeposit
{
    #region LoanDeposit
    public partial class LoanDeposit : IProduct, ILoanDeposit, IDeclarativeProvision
    {
        #region Constructors
        public LoanDeposit()
        {
            additionalPayment = new Payment[1] { new Payment() };
        }
        #endregion Constructors
        #region Accessors
        #region MinEffectiveDate
        // 20071015 EG Ticket 15858
        public EFS_EventDate MinEffectiveDate
        {
            get
            {
                EFS_EventDate dtEffective = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (IInterestRateStream stream in loanDepositStream)
                {
                    EFS_EventDate streamEffectiveDate = stream.EffectiveDate;
                    if ((DateTime.MinValue == dtEffective.unadjustedDate.DateValue) ||
                        (0 < dtEffective.unadjustedDate.DateValue.CompareTo(streamEffectiveDate.unadjustedDate.DateValue)))
                    {
                        dtEffective.unadjustedDate.DateValue = streamEffectiveDate.unadjustedDate.DateValue;
                        dtEffective.adjustedDate.DateValue = streamEffectiveDate.adjustedDate.DateValue;
                    }
                }
                return dtEffective;
            }
        }
        #endregion MinEffectiveDate
        #region MaxTerminationDate
        // 20071015 EG Ticket 15858
        public EFS_EventDate MaxTerminationDate
        {
            get
            {
                EFS_EventDate dtTermination = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (LoanDepositStream stream in loanDepositStream)
                {
                    EFS_EventDate streamTerminationDate = ((IInterestRateStream)stream).TerminationDate;
                    if (0 < streamTerminationDate.unadjustedDate.DateValue.CompareTo(dtTermination.unadjustedDate.DateValue))
                    {
                        dtTermination.unadjustedDate.DateValue = streamTerminationDate.unadjustedDate.DateValue;
                        dtTermination.adjustedDate.DateValue = streamTerminationDate.adjustedDate.DateValue;
                    }
                }
                return dtTermination;
            }
        }
        #endregion MaxTerminationDate
        #region Stream
        public LoanDepositStream[] Stream
        {
            get { return loanDepositStream; }
        }
        #endregion Stream
        #endregion Accessors

        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members

        #region ILoanDeposit Members
        IInterestRateStream[] ILoanDeposit.Stream { get { return this.loanDepositStream; } }
        bool ILoanDeposit.CancelableProvisionSpecified { get { return this.cancelableProvisionSpecified; } }
        ICancelableProvision ILoanDeposit.CancelableProvision { get { return this.cancelableProvision; } }
        bool ILoanDeposit.ExtendibleProvisionSpecified { get { return this.extendibleProvisionSpecified; } }
        IExtendibleProvision ILoanDeposit.ExtendibleProvision { get { return this.extendibleProvision; } }
        bool ILoanDeposit.EarlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
        IEarlyTerminationProvision ILoanDeposit.EarlyTerminationProvision { get { return this.earlyTerminationProvision; } }
        bool ILoanDeposit.StepUpProvisionSpecified { get { return this.stepUpProvisionSpecified; } }
        IStepUpProvision ILoanDeposit.StepUpProvision { get { return this.stepUpProvision; } }
        bool ILoanDeposit.AdditionalPaymentSpecified
        {
            set { this.additionalPaymentSpecified = value; }
            get { return this.additionalPaymentSpecified; }
        }
        IPayment[] ILoanDeposit.AdditionalPayment { get { return this.additionalPayment; } }
        bool ILoanDeposit.ImplicitProvisionSpecified { get { return this.implicitProvisionSpecified; } }
        IImplicitProvision ILoanDeposit.ImplicitProvision { get { return this.implicitProvision; } }
        bool ILoanDeposit.ImplicitCancelableProvisionSpecified
        {
            get { return (this.implicitProvisionSpecified && this.implicitProvision.cancelableProvisionSpecified); }
        }
        bool ILoanDeposit.ImplicitOptionalEarlyTerminationProvisionSpecified
        {
            get { return (this.implicitProvisionSpecified && this.implicitProvision.optionalEarlyTerminationProvisionSpecified); }
        }
        bool ILoanDeposit.ImplicitExtendibleProvisionSpecified
        {
            get { return (this.implicitProvisionSpecified && this.implicitProvision.extendibleProvisionSpecified); }
        }
        bool ILoanDeposit.ImplicitMandatoryEarlyTerminationProvisionSpecified
        {
            get { return (this.implicitProvisionSpecified && this.implicitProvision.mandatoryEarlyTerminationProvisionSpecified); }
        }
        EFS_EventDate ILoanDeposit.MaxTerminationDate { get { return MaxTerminationDate; } }
        #endregion ILoanDeposit Members
        #region IDeclarativeProvision Members
        bool IDeclarativeProvision.CancelableProvisionSpecified { get { return this.cancelableProvisionSpecified; } }
        ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return this.cancelableProvision; } }
        bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return this.extendibleProvisionSpecified; } }
        IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return this.extendibleProvision; } }
        bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
        IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return this.earlyTerminationProvision; } }
        bool IDeclarativeProvision.StepUpProvisionSpecified { get { return this.stepUpProvisionSpecified; } }
        IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return this.stepUpProvision; } }
        bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
        IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
        #endregion IDeclarativeProvision Members
    }
	#endregion LoanDeposit
    #region LoanDepositStream
    public partial class LoanDepositStream
    {
        #region Constructors
        public LoanDepositStream()
        {
            principalExchangesSpecified = true;
            principalExchanges = new PrincipalExchanges
            {
                initialExchange = new EFS_Boolean(true),
                intermediateExchange = new EFS_Boolean(true),
                finalExchange = new EFS_Boolean(true)
            };
        }
        #endregion Constructors
    }
    #endregion LoanDepositStream

}
