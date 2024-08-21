#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;

#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Class chargee de l'amimentation du dataDocument via les ccis (contexte titre)
    /// </summary>
    public class DebtSecInput : TradeCommonInput
    {
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public new TradeCustomCaptureInfos CustomCaptureInfos
        {
            set { base.CustomCaptureInfos = value; }
            get { return (TradeCustomCaptureInfos)base.CustomCaptureInfos; }
        }
        #endregion

        #region Constructor
        public DebtSecInput() : base() { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Initialise les données non renseignées 
        /// <para>Alimente notament les données obligatoire vis à vis  du XSD</para>
        /// </summary>
        // EG 20190823 [FIXEDINCOME] Test  debtSecurity.debtSecurityType
        public override void SetDefaultValue(string pCS, IDbTransaction pDbTransaction)
        {
            if (Product.ProductBase.IsDebtSecurity)
            {
                IDebtSecurity debtSecurity = (IDebtSecurity)this.Product.Product;
                ICalculationPeriodDates calculationPeriodDates = debtSecurity.Stream[0].CalculationPeriodDates;

                #region Date transaction et Date de valeur
                DateTime dtTradeDate = DataDocument.TradeHeader.TradeDate.DateValue;
                DateTime dtValue = DateTime.MinValue;

                if (calculationPeriodDates.EffectiveDateAdjustableSpecified)
                    dtValue = calculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue;

                if (DtFunc.IsDateTimeFilled(dtTradeDate) || DtFunc.IsDateTimeFilled(dtValue))
                {
                    // si Dt Emission non renseignée => alimentée avec Dt Valeur
                    // si Dt Valeur non renseignée   => alimentée avec Dt Emission

                    if (DtFunc.IsDateTimeEmpty(dtTradeDate))
                    {
                        DataDocument.TradeHeader.TradeDate.Value = DtFunc.DateTimeToStringDateISO(dtValue);
                    }
                    else if (false == calculationPeriodDates.EffectiveDateRelativeSpecified)
                    {
                        calculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue = dtTradeDate;
                        calculationPeriodDates.EffectiveDateAdjustableSpecified = true;
                    }
                }
                else
                {
                    // Dt Emission et Dt Valeur non renseignées
                    // => alimentées avec Dt PreviousCouponDate si renseignée
                    // => alimentées avec Dt FirstPeriodStartDate si renseignée
                    // => alimentées avec Dt FirstRegularPeriodStartDate si renseignée
                    // sinon Erreur
                    if (debtSecurity.PrevCouponDateSpecified)
                    {
                        dtValue = debtSecurity.PrevCouponDate.DateValue;
                        calculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue = dtValue;
                        calculationPeriodDates.EffectiveDateAdjustableSpecified = true;
                    }
                    else if (calculationPeriodDates.FirstPeriodStartDateSpecified)
                    {
                        dtValue = calculationPeriodDates.FirstPeriodStartDate.UnadjustedDate.DateValue;
                        calculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue = dtValue;
                        calculationPeriodDates.EffectiveDateAdjustableSpecified = true;
                    }
                    else if (calculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                    {
                        dtValue = calculationPeriodDates.FirstRegularPeriodStartDate.DateValue;
                        calculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue = dtValue;
                        calculationPeriodDates.EffectiveDateAdjustableSpecified = true;
                    }
                    DataDocument.TradeHeader.TradeDate.Value = DtFunc.DateTimeToStringDateISO(dtValue);
                }
                #endregion

                if (debtSecurity.DebtSecurityType == DebtSecurityTypeEnum.Perpetual)
                {

                    #region add TerminationDate
                    bool isSetTerminationDate = (false == calculationPeriodDates.TerminationDateRelativeSpecified);
                    if (isSetTerminationDate)
                    {
                        isSetTerminationDate = (false == calculationPeriodDates.TerminationDateAdjustableSpecified) ||
                            (calculationPeriodDates.TerminationDateAdjustableSpecified && DtFunc.IsDateTimeEmpty(calculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue));
                    }
                    if (isSetTerminationDate)
                    {
                        calculationPeriodDates.TerminationDateAdjustableSpecified = true;
                        calculationPeriodDates.TerminationDateAdjustable = Product.ProductBase.CreateAdjustableDate(DateTime.MaxValue, BusinessDayConventionEnum.NONE, null);
                    }
                    #endregion add TerminationDate
                }

                //
                ICalculationPeriodAmount calculationPeriodAmount = debtSecurity.Stream[0].CalculationPeriodAmount;
                //
                #region calculationPerioAmount
                if ((false == calculationPeriodAmount.CalculationSpecified &&
                    false == calculationPeriodAmount.KnownAmountScheduleSpecified)
                    ||
                    (calculationPeriodAmount.KnownAmountScheduleSpecified &&
                    StrFunc.IsEmpty(calculationPeriodAmount.KnownAmountSchedule.InitialValue.Value))
                    ||
                    (calculationPeriodAmount.CalculationSpecified &&
                    calculationPeriodAmount.Calculation.NotionalSpecified &&
                    StrFunc.IsEmpty(calculationPeriodAmount.Calculation.Notional.StepSchedule.InitialValue.Value)))
                {
                    calculationPeriodAmount.CalculationSpecified = true;
                    calculationPeriodAmount.KnownAmountScheduleSpecified = false;
                    //
                    calculationPeriodAmount.Calculation.NotionalSpecified = true;
                    calculationPeriodAmount.Calculation.FxLinkedNotionalSpecified = false;
                    //
                    calculationPeriodAmount.Calculation.Notional.StepSchedule.InitialValue.DecValue = 0;
                    if (StrFunc.IsEmpty(calculationPeriodAmount.Calculation.Notional.StepSchedule.Currency.Value))
                        calculationPeriodAmount.Calculation.Notional.StepSchedule.Currency.Value = debtSecurity.Security.Currency.Value;
                    //
                    if (StrFunc.IsEmpty(calculationPeriodAmount.Calculation.Notional.Id))
                        calculationPeriodAmount.Calculation.Notional.Id = TradeCustomCaptureInfos.CCst.NOTIONALSCHEDULE_REFERENCE + "1";
                    if (StrFunc.IsEmpty(debtSecurity.Stream[0].CalculationPeriodAmount.Calculation.Notional.StepSchedule.Id))
                        calculationPeriodAmount.Calculation.Notional.StepSchedule.Id = TradeCustomCaptureInfos.CCst.INITIALVALUE_REFERENCE + "1";

                }
                #endregion
                //
                #region rate
                if (calculationPeriodAmount.CalculationSpecified)
                {
                    // Cas des titre precompté => le taux est non renseigné 
                    // rateFixedRate n'est pas spécifié et 
                    // soit rateFloatingRate n'est pas spécifié
                    // soit l'un ou l'autre sont spécifiés mais vide
                    if (
                        ((false == calculationPeriodAmount.Calculation.RateFixedRateSpecified) && (false == calculationPeriodAmount.Calculation.RateFloatingRateSpecified))
                        ||
                        (calculationPeriodAmount.Calculation.RateFloatingRateSpecified &&
                        StrFunc.IsEmpty(calculationPeriodAmount.Calculation.RateFloatingRate.FloatingRateIndex.Value))
                        ||
                        (calculationPeriodAmount.Calculation.RateFixedRateSpecified &&
                        StrFunc.IsEmpty(calculationPeriodAmount.Calculation.RateFixedRate.InitialValue.Value))
                       )
                    {
                        calculationPeriodAmount.Calculation.RateFixedRateSpecified = true;
                        calculationPeriodAmount.Calculation.RateFloatingRateSpecified = false;
                        calculationPeriodAmount.Calculation.RateFixedRate.InitialValue = new EFS_Decimal(0);

                        debtSecurity.Security.CouponType.Value = CouponTypeEnum.Fixed.ToString();
                    }
                }
                #endregion
            }
            //
            base.SetDefaultValue(pCS, pDbTransaction);

        }

        /// <summary>
        /// Définit une nouvelle instance de SQl_TRADE
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeIdentifier"></param>
        public override void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pTradeIdentifier)
        {
            InitializeSqlTrade(pCS, pDbTransaction, pTradeIdentifier, SQL_TableWithID.IDType.Identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        public override void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType)
        {
            m_SQLTrade = new SQL_TradeDebtSecurity(pCS, pIdType, pId)
            {
                DbTransaction = pDbTransaction
            };
        }

        /// <summary>
        /// Définit une nouvelle instance des ccis
        /// <para>
        /// Synchronize des pointeurs existants dans les CciContainers avec les éléments du dataDocument
        /// </para>
        /// </summary>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        /// FI 20141107 [20441] Modification de signature
        // EG 20180425 Analyse du code Correction [CA2214]
        public override void InitializeCustomCaptureInfos(string pCS, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
        {
            this.CustomCaptureInfos = new TradeCustomCaptureInfos(pCS, this, pUser, pSessionId, pIsGetDefaultOnInitializeCci);
            this.CustomCaptureInfos.InitializeCciContainer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        protected override bool CheckIsTradeFound(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, User pUser, string pSessionId)
        {
            Cst.StatusEnvironment stEnv = Cst.StatusEnvironment.UNDEFINED;
            bool isTemplate = TradeRDBMSTools.IsTradeTemplate(pCS, pDbTransaction, pId, pIdType);
            if (isTemplate)
                stEnv = Cst.StatusEnvironment.TEMPLATE;

            string sqlTradeId = pId.Replace("%", SQL_TableWithID.StringForPERCENT);

            SQL_TradeDebtSecurity sqlTrade = new SQL_TradeDebtSecurity(pCS, pIdType, sqlTradeId,
                                            stEnv, SQL_Table.RestrictEnum.Yes, pUser, pSessionId)
            {
                DbTransaction = pDbTransaction
            };
            return sqlTrade.IsFound;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class DebtSecInputGUI : TradeCommonInputGUI
    {
        #region Accessors
        /// <summary>
        ///  Retourne l'identifiant du regroupement des produits 
        /// </summary>
        public override Cst.SQLCookieGrpElement GrpElement
        {
            get
            {
                return Cst.SQLCookieGrpElement.SelDebtSecProduct;
            }
        }
        /// <summary>
        ///  Retoune la restriction sur la table PRODUCT
        /// </summary>
        /// <param name="pSqlAlias">alias de la table PRODUCT</param>
        public override string GetSQLRestrictProduct(string pSqlAlias)
        {
            string ret = pSqlAlias + ".GPRODUCT=" + DataHelper.SQLString(Cst.ProductGProduct_ASSET);
            ret += SQLCst.AND + pSqlAlias + ".FAMILY=" + DataHelper.SQLString(Cst.ProductFamily_DSE);
            return ret;

        }
        #region MainRessource
        /// <summary>
        /// Retourne titre en "fr-FR"
        /// </summary>
        public override string MainRessource
        {
            get
            {
                return Ressource.GetString2("DebtSecurity"); ;
            }
        }
        #endregion
        #endregion Accessors
        #region Constructors
        public DebtSecInputGUI(string pIdMenu, User pUser, string pXMLFilePath)
            : base(pIdMenu, pUser, pXMLFilePath)
        {
        }
        #endregion Constructors

    }
}