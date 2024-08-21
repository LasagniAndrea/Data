#region using directives
//
//
using EFS.GUI.Interface;
using EfsML.Business;
//
using EfsML.Enum;
using EfsML.Interface;
//
using FpML.Interface;
using FpML.v44.Shared;
using System;
//
#endregion using directives

namespace EfsML.v30.CashBalanceInterest
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CashBalanceInterest : IProduct, ICashBalanceInterest
    {
        #region accessors
        #region Stream
        public CashBalanceInterestStream[] Stream
        {
            get { return cashBalanceInterestStream; }
        }
        #endregion Stream
        #region MinEffectiveDate
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
                foreach (IInterestRateStream stream in cashBalanceInterestStream)
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
                foreach (IInterestRateStream stream in cashBalanceInterestStream)
                {
                    EFS_EventDate streamTerminationDate = stream.TerminationDate;
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
        #endregion
        #region Constructor
        public CashBalanceInterest()
        {
        }
        #endregion
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
        #region ICashBalanceInterest Members
        /// <summary>
        /// Type de montant sur lequel portent les intérêts
        /// </summary>
        Nullable<InterestAmountTypeEnum> ICashBalanceInterest.InterestAmountType
        {
            set { this.interestAmountType = value.Value; }
            get { return this.interestAmountType; }
        }
        /// <summary>
        /// Entité
        /// </summary>
        IReference ICashBalanceInterest.EntityPartyReference
        {
            get { return (IReference)this.entityPartyReference; }
            set { entityPartyReference = (PartyReference)value; }
        }
        /// <summary>
        /// Eléments de calcul des intérêts
        /// </summary>
        IInterestRateStream[] ICashBalanceInterest.Stream
        {
            get { return this.cashBalanceInterestStream; }
        }
        #endregion
    }

    #region CashBalanceInterestStream
    /// <summary>
    /// 
    /// </summary>
    public partial class CashBalanceInterestStream : IEFS_Array, IInterestRateStream
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public CashBalanceInterestStream() : base()
        {
            minimumThresholdSpecified = false;
            minimumThreshold = new Money();
        }
        #endregion Constructors
    }
    #endregion CashBalanceInterestStream
}
