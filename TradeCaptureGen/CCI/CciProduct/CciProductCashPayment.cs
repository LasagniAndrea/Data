#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.CCI;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Linq;
using Tz = EFS.TimeZone;

#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Description résumée de CciProductCashPayment.
    /// </summary>
    /// EG 20171109 [23509] New
    public class CciProductCashPayment : CciProductBulletPayment
    {
        #region CciEnum
        /// <summary>
        /// 
        /// </summary>
        /// EG 20171204 [23509] Add orderEntered
        /// EG 20190308 Upd [VCL migration] Add CCI Group
        public enum CciEnum
        {
            [CciGroup(name = "MarketTimeZone")]
            orderEntered,
            unknown,
        }
        #endregion CciEnum

        #region Members
        /// <summary>
        /// 
        /// </summary>
        // EG 20180608 New
        private ICashPayment _cashPayment;
        // EG 20180608 New
        private IParty _partyEntity;
        #endregion Members

        #region constructor
        public CciProductCashPayment(CciTradeBase pCciTrade, IBulletPayment pBulletPayment, string pPrefix)
            : this(pCciTrade, pBulletPayment, pPrefix, -1)
        { }
        public CciProductCashPayment(CciTradeBase pCciTrade, IBulletPayment pBulletPayment, string pPrefix, int pNumber)
            : base(pCciTrade, pBulletPayment, pPrefix, pNumber)
        {
        }
        #endregion constructor

        #region public override SetProduct
        // EG 20180608 New
        public override void SetProduct(IProduct pProduct)
        {
            _cashPayment = (ICashPayment)pProduct;
            _partyEntity = CciTradeCommon.DataDocument.GetParty(_cashPayment.EntityPartyReference.HRef);
            base.SetProduct(pProduct);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// EG 20171204 [23509] Add orderEntered
        /// EG 20180608 Upd (use _partyEntity)
        public override void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    bool isToValidate = false;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables

                    
                    switch (cciEnum)
                    {
                        case CciEnum.orderEntered:
                            #region OrderEntered
                            if (null != _partyEntity)
                            {
                                IPartyTradeInformation partyTradeInformation = CciTradeCommon.DataDocument.GetPartyTradeInformation(_partyEntity.Id);
                                if ((null != partyTradeInformation) && (partyTradeInformation.TimestampsSpecified && partyTradeInformation.Timestamps.OrderEnteredSpecified))
                                    data = partyTradeInformation.Timestamps.OrderEntered;
                            }
                            if (StrFunc.IsEmpty(data))
                            {
                                data = Tz.Tools.ToString(OTCmlHelper.GetDateSysUTC(CciTrade.CS));
                            }
                            SynchronizeTimeZone();
                            #endregion OrderEntered
                            break;
                        default:
                            #region Default
                            isSetting = false;
                            #endregion Default
                            break;
                    }
                    if (isSetting)
                    {
                        CcisBase.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }

                }
            }

            base.Initialize_FromDocument();
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20171122 [23509] Add orderEntered
        public override void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    #region Reset variables
                    string data = cci.NewValue;
                    Boolean isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.orderEntered:
                            #region OrderEntered
                            DumpOrderEntered_ToDocument(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion OrderEntered
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            base.Dump_ToDocument();
        }
        
        /// <summary>
        /// Dump a orderEntered into DataDocument
        /// </summary>
        /// <param name="pData"></param>
        /// EG 20171122 [23509] New
        /// EG 20180608 Upd (use _partyEntity)
        private void DumpOrderEntered_ToDocument(string pData)
        {
            if (null != _partyEntity)
            {
                IPartyTradeInformation partyTradeInformation = CciTradeCommon.DataDocument.GetPartyTradeInformation(_partyEntity.Id);
                if (null != partyTradeInformation)
                {
                    if (Tz.Tools.IsDateFilled(pData))
                    {
                        bool isSetOrderEntered = (null != partyTradeInformation) && (false == partyTradeInformation.OrderEnteredSpecified);
                        if (isSetOrderEntered)
                            partyTradeInformation.Timestamps = CciTrade.DataDocument.CurrentProduct.ProductBase.CreateTradeProcessingTimestamps();

                        partyTradeInformation.Timestamps.OrderEntered = pData;
                        partyTradeInformation.Timestamps.OrderEnteredSpecified = true;
                        Nullable<DateTimeOffset> dtOrderEnteredInTimeZone = Tz.Tools.FromTimeZone(partyTradeInformation.Timestamps.OrderEnteredDateTimeOffset.Value, GetTimeZoneValue());
                        CciTrade.DataDocument.TradeHeader.TradeDate.Value = DtFunc.DateTimeToStringDateISO(dtOrderEnteredInTimeZone.Value.Date);
                    }
                    else
                    {
                        partyTradeInformation.Timestamps.OrderEntered = string.Empty;
                        partyTradeInformation.Timestamps.OrderEnteredSpecified = false;
                        CciTrade.DataDocument.TradeHeader.TradeDate.Value = string.Empty;
                    }
                    partyTradeInformation.TimestampsSpecified = partyTradeInformation.Timestamps.OrderEnteredSpecified;
                }
            }
        }
        
        /// <summary>
        /// Retourne le timezone.
        /// </summary>
        /// <returns></returns>
        /// EG 20171122 [23509] New
        private string GetTimeZoneValue()
        {
            string timezone = string.Empty;
            CustomCaptureInfo cciOrderEntered = Cci(CciEnum.orderEntered);

            // 1. Recherche du Timezone associé à la date d'exécution
            if (null != cciOrderEntered)
            {
                CustomCaptureInfo cciZone = CcisBase[cciOrderEntered.ClientIdZone, false];
                if ((null != cciZone) && StrFunc.IsFilled(cciZone.NewValue))
                    timezone = cciZone.NewValue;
            }
            // 2. Recherche du Timezone associé à la plateforme puis à l'entité et Universal timezone si non trouvé
            if (StrFunc.IsEmpty(timezone))
            {
                timezone = CciTrade.DataDocument.GetTradeTimeZone(CciTrade.CSCacheOn, CcisBase.User.Entity_IdA, Tz.Tools.UniversalTimeZone);
            }
            return timezone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20180608 Upd (use _partyEntity)
        private void SynchronizeTimeZone()
        {
            string timeZone;
            if ((null != _partyEntity) && StrFunc.IsFilled(_partyEntity.Tzdbid))
                timeZone = _partyEntity.Tzdbid;
            else
                timeZone = CciTrade.DataDocument.GetTradeTimeZone(CciTrade.CSCacheOn, CcisBase.User.Entity_IdA);

            string clientIdZone = Cci(CciEnum.orderEntered).ClientId.Replace(Cst.TMS, Cst.TMZ);
            CustomCaptureInfo cciZone = CcisBase[clientIdZone, false];
            if (cciZone.NewValue != timeZone)
                CcisBase.SetNewValue(cciZone.ClientId, false, timeZone, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// EG 20180608 New (to update _partyEntity)
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            base.ProcessInitialize(pCci);

            for (int i = 0; i < CciTrade.PartyLength; i++)
            {
                if (CciTrade.cciParty[i].IsCci(CciTradeParty.CciEnum.actor, pCci))
                {
                    SQL_Actor sql_Actor = (SQL_Actor)pCci.Sql_Table;
                    if (sql_Actor != null)
                    {
                        if (ActorTools.IsActorWithRole(CciTradeCommon.CSCacheOn, sql_Actor.Id, new RoleActor[] { RoleActor.ENTITY }, 0))
                        {
                            _cashPayment.EntityPartyReference.HRef = sql_Actor.XmlId;
                            _cashPayment.EntityPartyReferenceSpecified = true;
                            _partyEntity = CciTradeCommon.DataDocument.GetParty(_cashPayment.EntityPartyReference.HRef);
                            CustomCaptureInfo cciOrderEntered = CcisBase[Cci(CciEnum.orderEntered).ClientId, false];
                            DumpOrderEntered_ToDocument(cciOrderEntered.NewValue);
                        }
                    }
                }
            }
        }
        
    }
    
}
