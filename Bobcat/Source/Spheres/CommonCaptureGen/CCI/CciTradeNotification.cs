#region Using Directives
using EFS.ACommon;
using EfsML;
using EfsML.Enum;
using System;

#endregion Using Directives

namespace EFS.TradeInformation
{
    #region public class CciTradeNotification
    /// <summary>
    /// Classe destinée à mettre à jour TradeNotification 
    /// </summary>
    public class CciTradeNotification
    {
        #region Member
        private readonly TradeNotification _tradeNotification;
        #endregion
        //
        #region accessor
        public TradeNotification TradeNotification
        {
            get { return _tradeNotification; }
        }
        #endregion
        //
        #region Constructor
        public CciTradeNotification(TradeNotification pTradeNotification)
            : base()
        {
            _tradeNotification = pTradeNotification;
        }
        #endregion
        //
        #region Accessor
        #endregion Accessor
        //
        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIndex"></param>
        /// <param name="pCciEnum"></param>
        /// <param name="pNewValue"></param>
        public void Dump_ToDocument(int pIndex, CciTradeParty.CciEnum pCciEnum, string pNewValue)
        {

            ActorNotification notificationActor = _tradeNotification.PartyNotification[pIndex];
            notificationActor.SetConfirmation(GetStepLifeEnum(pCciEnum), BoolFunc.IsTrue(pNewValue));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIndex"></param>
        /// <param name="pCciEnum"></param>
        /// <returns></returns>
        public string Initialize_FromDocument(int pIndex, CciTradeParty.CciEnum pCciEnum)
        {
            ActorNotification notificationActor = _tradeNotification.PartyNotification[pIndex];
            return notificationActor.GetConfirm(GetStepLifeEnum(pCciEnum)).ToString().ToLower();
        }
        #endregion
        
        #region private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciEnum"></param>
        /// <returns></returns>
        private NotificationStepLifeEnum GetStepLifeEnum(CciTradeParty.CciEnum pCciEnum)
        {

            NotificationStepLifeEnum ret = default;

            switch (pCciEnum)
            {
                case CciTradeParty.CciEnum.ISNCMINI:
                    ret = NotificationStepLifeEnum.INITIAL;
                    break;
                case CciTradeParty.CciEnum.ISNCMINT:
                    ret = NotificationStepLifeEnum.INTERMEDIARY;
                    break;
                case CciTradeParty.CciEnum.ISNCMFIN:
                    ret = NotificationStepLifeEnum.FINAL;
                    break;

                // EG 20160404 Migration vs2013
#if DEBUG
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pCciEnum.ToString()));
#else
                default:
                    break;
#endif
            }
            //
            return ret;
        }
        #endregion
    }
    #endregion
}