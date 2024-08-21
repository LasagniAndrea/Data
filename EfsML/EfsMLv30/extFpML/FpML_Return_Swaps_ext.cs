#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.Interface;

using FpML.Interface;

using FpML.v44.Doc;
using FpML.v44.Eq.Shared;
using FpML.v44.Shared;
#endregion using directives

namespace FpML.v44.ReturnSwaps
{
	#region EquitySwapTransactionSupplement
    // EG 20140702 Update Interface
	public partial class EquitySwapTransactionSupplement : IProduct, IEquitySwapTransactionSupplement, IDeclarativeProvision 
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		#endregion IProduct Members

        #region IEquitySwapTransactionSupplement Members
        bool IEquitySwapTransactionSupplement.MultipleExchangeIndexAnnexFallbackSpecified
        {
            set { this.multipleExchangeIndexAnnexFallbackSpecified = value; }
            get { return this.multipleExchangeIndexAnnexFallbackSpecified; }
        }
        EFS_Boolean IEquitySwapTransactionSupplement.MultipleExchangeIndexAnnexFallback
        {
            set { this.multipleExchangeIndexAnnexFallback = value; }
            get { return this.multipleExchangeIndexAnnexFallback; }
        }
        bool IEquitySwapTransactionSupplement.MutualEarlyTerminationSpecified
        {
            set { this.mutualEarlyTerminationSpecified = value; }
            get { return this.mutualEarlyTerminationSpecified; }
        }
        EFS_Boolean IEquitySwapTransactionSupplement.MutualEarlyTermination
        {
            set { this.mutualEarlyTermination = value; }
            get { return this.mutualEarlyTermination; }
        }
        bool IEquitySwapTransactionSupplement.LocalJurisdictionSpecified
        {
            set { this.localJurisdictionSpecified = value; }
            get { return this.localJurisdictionSpecified; }
        }
        IScheme IEquitySwapTransactionSupplement.LocalJurisdiction
        {
            set { this.localJurisdiction = (Country)value; }
            get { return this.localJurisdiction; }
        }
        bool IEquitySwapTransactionSupplement.RelevantJurisdictionSpecified
        {
            set { this.relevantJurisdictionSpecified = value; }
            get { return this.relevantJurisdictionSpecified; }
        }
        IScheme IEquitySwapTransactionSupplement.RelevantJurisdiction
        {
            set { this.relevantJurisdiction = (Country)value; }
            get { return this.relevantJurisdiction; }
        }
        #endregion IEquitySwapTransactionSupplement Members

		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion EquitySwapTransactionSupplement
}
