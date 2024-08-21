using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Common;
using EFS.SpheresRiskPerformance.External;
//
using EfsML.v30.MarginRequirement;
//
using FixML.Enum;
using FixML.v50SP1;
using FixML.v50SP1.Enum;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// Complément pour la gestion du log
    /// </summary>
    public sealed partial class CalculationSheetRepository
    {
        #region Members
        private Dictionary<int, AssetETDInfo> m_AssetInfo = default;
        private Dictionary<int, string> m_ActorRepository = new Dictionary<int, string>();
        #endregion Members
        #region Accessors
        [XmlIgnore]
        public Dictionary<int, AssetETDInfo> AssetInfo
        {
            get { return m_AssetInfo; }
            set { m_AssetInfo = value; }
        }
        [XmlIgnore]
        public Dictionary<int, string> ActorRepository
        {
            get { return m_ActorRepository; }
            set { m_ActorRepository = value; }
        }
        #endregion Accessors

        /// <summary>
        /// Alimentation d'un FixPositionReport 
        /// </summary>
        /// <param name="pLogsRepository"></param>
        /// <param name="pPositions"></param>
        /// <returns></returns>
        public FixPositionReport[] BuildExternalPositionReport(DateTime pDtBusiness, SettlSessIDEnum pTiming, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            FixPositionReport[] positionReports = new FixPositionReport[pPositions.Count()];
            int idxPosition = 0;
            foreach (Pair<PosRiskMarginKey, RiskMarginPosition> position in pPositions)
            {
                PosRiskMarginKey keyPos = position.First;
                RiskMarginPosition valuePos = position.Second;
                FixPositionReport reportPosition = new FixPositionReport();

                if (pDtBusiness > DateTime.MinValue)
                {
                    reportPosition.BizDt = pDtBusiness;
                    reportPosition.BizDtSpecified = true;
                    
                    // FI 20171025 [23533] Appel à ConvertEnumToString
                    //reportPosition.SetSesID = ReflectionTools.GetAttribute<XmlEnumAttribute>(
                    //    (typeof(SettlSessIDEnum), Enum.GetName(typeof(SettlSessIDEnum), pTiming), true);
                    reportPosition.SetSesID = ReflectionTools.ConvertEnumToString < SettlSessIDEnum>(pTiming); 
                    reportPosition.SetSesIDSpecified = true;
                }
                // Acteur
                if ((m_ActorRepository != null) && m_ActorRepository.TryGetValue(keyPos.idA_Dealer, out string actorIdentifier))
                {
                    reportPosition.Acct = actorIdentifier;
                    reportPosition.AcctSpecified = true;
                }
                // Asset
                SetInstrumentBlockFromAssetETDInfo(keyPos, reportPosition);
                reportPosition.InstrmtSpecified = true;
                // Quantité
                SetQuantity(keyPos, valuePos, null, null, reportPosition);
                //
                positionReports[idxPosition] = reportPosition;
                idxPosition += 1;
            }
            return positionReports;
        }

        /// <summary>
        /// Alimente la partie InstrumentBlock d'un FixPositionReport
        /// </summary>
        /// <param name="keyPos"></param>
        /// <param name="positionReport"></param>
        private void SetInstrumentBlockFromAssetETDInfo(PosRiskMarginKey keyPos, FixPositionReport positionReport)
        {
            InstrumentBlock instrmt = positionReport.Instrmt;
            instrmt.Src = SecurityIDSourceEnum.Proprietary;
            instrmt.SrcSpecified = true;

            if ((m_AssetInfo != null) && m_AssetInfo.TryGetValue(keyPos.idAsset, out AssetETDInfo asset))
            {
                instrmt.Exch = asset.MarketIdentifier;
                instrmt.ExchSpecified = true;

                instrmt.MMY = asset.MaturityMonthYear;
                instrmt.MMYSpecified = true;

                instrmt.Sym = asset.ContractSymbol;
                instrmt.SymSpecified = true;

                if (StrFunc.IsFilled(asset.ContractAttribute))
                {
                    instrmt.OptAt = asset.ContractAttribute;
                    instrmt.OptAtSpecified = true;
                }

                if (StrFunc.IsFilled(asset.PutCall))
                {
                    instrmt.StrkPx = asset.StrikePrice;
                    instrmt.StrkPxSpecified = true;

                    instrmt.PutCall = (FixML.Enum.PutOrCallEnum)System.Enum.Parse(typeof(FixML.Enum.PutOrCallEnum), asset.PutCall);
                    instrmt.PutCallSpecified = true;
                }
            }
        }
    }
}
