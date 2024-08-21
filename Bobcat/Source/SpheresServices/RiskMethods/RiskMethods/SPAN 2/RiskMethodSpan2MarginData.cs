using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
//
using EFS.ACommon;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin.v1_0_39;
using EFS.SpheresRiskPerformance.RiskMethods.Span2CoreAPI.v1_14;
//
using FixML.Enum;

namespace EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin
{
    /// <summary>
    /// Classe des données nécessaires à la création des messages des requêtes SPAN2
    /// et de génération des requêtes JSON et XML
    /// </summary>
    public class SPAN2MarginData
    {
        #region Members
        private DateTime m_DtBusiness;
        private string m_CssAcronym;
        private int m_ActorId;
        private int m_BookId;
        private bool m_IsMaintenance = true;
        private SettlSessIDEnum m_Timing;

        private readonly IEnumerable<AssetExpandedParameter> m_AssetExpandedParameters;
        private IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> m_GlobalPosition;
        private List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> m_MarginPosition;
        private readonly List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> m_DiscartedPosition;
        private readonly List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> m_GoodPosition;

        private OriginType m_OriginType = OriginType.CUSTOMER;
        private Currency m_Currency = Currency.USD;
        private CustomerAccountType m_CustomerAccountType = CustomerAccountType.HEDGE;
        private YesNoIndicator m_OmnibusInd = YesNoIndicator.NO;

        // PM 20230929 [XXXXX] Changement de type : string => List<string>
        private List<string> m_XmlRequestMessage;
        // PM 20230929 [XXXXX] Changement de type : string => List<string>
        private List<string> m_XmlResponseMessage;
        private string m_GetMarginXmlResponseMessage;

        private RiskPortfolioRequestMessage m_RiskPortfolioRequestMessage;
        private string m_JsonRiskPortfolioRequestMessage;
        private long m_NbSpanRiskPosition;

        private MarginDetailResponseMessage m_MarginDetailResponseMessage;
        private string m_JsonMarginDetailResponseMessage;

        private string m_ApiSessionId;
        private string m_ApiMarginId;
        // PM 20230831 [XXXXX] Add m_ApiPortfolioId
        private string m_ApiPortfolioId;

        private readonly List<SPAN2Error> m_errors;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Date Business
        /// </summary>
        public DateTime DtBusiness
        {
            get { return m_DtBusiness; }
            set { m_DtBusiness = value; }
        }

        /// <summary>
        /// Acronym de la Clearing house
        /// </summary>
        public string CssAcronym
        {
            get { return m_CssAcronym; }
            set { m_CssAcronym = value; }
        }
        /// <summary>
        /// Id interne de l'acteur Spheres
        /// </summary>
        public int ActorId
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }

        /// <summary>
        /// Id interne du book Spheres
        /// </summary>
        public int BookId
        {
            get { return m_BookId; }
            set { m_BookId = value; }
        }

        /// <summary>
        /// Indicateur d'utilisation du montant de maintenance
        /// </summary>
        public bool IsMaintenance
        {
            get { return m_IsMaintenance; }
            set { m_IsMaintenance = value; }
        }

        /// <summary>
        /// Timing: Intraday,EndOfDay
        /// </summary>
        public SettlSessIDEnum Timing
        {
            get { return m_Timing; }
            set { m_Timing = value; }
        }

        /// <summary>
        /// Position globale du book
        /// </summary>
        public IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> GlobalPosition
        {
            get { return m_GlobalPosition; }
            set { m_GlobalPosition = value; }
        }

        /// <summary>
        /// Position partielle du book
        /// </summary>
        public List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> MarginPosition
        {
            get { return m_MarginPosition; }
            set { m_MarginPosition = value; }
        }

        /// <summary>
        /// Position écartée du book
        /// </summary>
        public List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> DiscartedPosition
        {
            get { return m_DiscartedPosition; }
        }

        /// <summary>
        /// Position valide du book
        /// </summary>
        public List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> GoodPosition
        {
            get { return m_GoodPosition; }
        }

        /// <summary>
        /// Caractèristiques des assets
        /// </summary>
        public IEnumerable<AssetExpandedParameter> AssetExpandedParameters
        {
            get { return m_AssetExpandedParameters; }
        }

        /// <summary>
        /// Cycle de calcul
        /// </summary>
        public CycleCode JsonCycleCode
        {
            get { return (m_Timing == SettlSessIDEnum.Intraday ? CycleCode.ITD : CycleCode.EOD); }
        }

        /// <summary>
        /// Origine de la position
        /// </summary>
        public OriginType OriginType
        {
            get { return m_OriginType; }
            set { m_OriginType = value; }
        }

        /// <summary>
        /// Portfolio Currency
        /// </summary>
        public Currency Currency
        {
            get { return m_Currency; }
            set { m_Currency = value; }
        }

        /// <summary>
        /// Customer Account Type: MEMBER / HEDGE / SPECULATOR
        /// </summary>
        public CustomerAccountType CustomerAccountType
        {
            get { return m_CustomerAccountType; }
            set { m_CustomerAccountType = value; }
        }

        /// <summary>
        /// Omnibus Indicator: YES / NO
        /// </summary>
        public YesNoIndicator OmnibusInd
        {
            get { return m_OmnibusInd; }
            set { m_OmnibusInd = value; }
        }

        /// <summary>
        /// Indique s'il s'agit d'un calcul Omnibus
        /// </summary>
        public bool IsOmnibus
        {
            get { return (m_OmnibusInd == YesNoIndicator.YES); }
        }

        /// <summary>
        /// Génération d'un Id de la requête en concaténant ActorId et BookId
        /// </summary>
        public string RequestId
        {
            get { return ("A" + m_ActorId.ToString() + "_B" + m_BookId.ToString()); }
        }

        /// <summary>
        /// Génération d'un Id pour le portfolio
        /// </summary>
        public string PortfolioId
        {
            get { return ("PB" + m_BookId.ToString()); }
        }

        /// <summary>
        /// Id du compte
        /// </summary>
        public string AccountId
        {
            get { return ("B" + m_BookId.ToString()); }
        }

        /// <summary>
        /// Nom du compte
        /// </summary>
        public string AccountName
        {
            get { return ("B" + m_BookId.ToString()); }
        }

        /// <summary>
        /// Id de la société
        /// </summary>
        public string FirmId
        {
            get { return ("A" + m_ActorId.ToString()); }
        }

        /// <summary>
        /// Requête XML générée
        /// </summary>
        // PM 20230929 [XXXXX] Changement de type : string => List<string>
        public List<string> XmlRequestMessage
        {
            get { return m_XmlRequestMessage; }
            set { m_XmlRequestMessage = value; }
        }

        /// <summary>
        /// Message XML reçue en réponse
        /// </summary>
        // PM 20230929 [XXXXX] Changement de type : string => List<string>
        public List<string> XmlResponseMessage
        {
            get { return m_XmlResponseMessage; }
            set { m_XmlResponseMessage = value; }
        }

        /// <summary>
        /// Dernier message XML reçue en réponse d'un GET Margin
        /// </summary>
        // PM 20230929 [XXXXX] Ajout
        public string GetMarginXmlResponseMessage
        {
            get { return m_GetMarginXmlResponseMessage; }
            set { m_GetMarginXmlResponseMessage = value; }
        }

        /// <summary>
        /// Classe pour la génération de la requete
        /// </summary>
        public RiskPortfolioRequestMessage RiskPortfolioRequestMessage
        {
            get { return m_RiskPortfolioRequestMessage; }
        }

        /// <summary>
        /// Requête RiskPortfolioRequest Json générée
        /// </summary>
        public string JsonRiskPortfolioRequestMessage
        {
            get { return m_JsonRiskPortfolioRequestMessage; }
        }

        /// <summary>
        /// Compteur de position du message de calcul
        /// </summary>
        public long NbSpanRiskPosition
        {
            get { return m_NbSpanRiskPosition; }
        }

        /// <summary>
        /// Classe du message de réponse reçue
        /// </summary>
        public MarginDetailResponseMessage MarginDetailResponseMessage
        {
            get { return m_MarginDetailResponseMessage; }
            set { m_MarginDetailResponseMessage = value; }
        }

        /// <summary>
        /// Réponse MarginDetailResponse Json reçue
        /// </summary>
        public string JsonMarginDetailResponseMessage
        {
            get { return m_JsonMarginDetailResponseMessage; }
            set { m_JsonMarginDetailResponseMessage = value; }
        }

        /// <summary>
        /// SessionId reçue de CME CORE API
        /// </summary>
        public string ApiSessionId
        {
            get { return m_ApiSessionId; }
            set { m_ApiSessionId = value; }
        }

        /// <summary>
        /// MarginId reçue du calcul par CME CORE API
        /// </summary>
        public string ApiMarginId
        {
            get { return m_ApiMarginId; }
            set { m_ApiMarginId = value; }
        }

        /// <summary>
        /// PortfolioId reçue du calcul par CME CORE API
        /// </summary>
        // PM 20230831 [XXXXX] Add ApiPortfolioId
        public string ApiPortfolioId
        {
            get { return m_ApiPortfolioId; }
            set { m_ApiPortfolioId = value; }
        }

        /// <summary>
        /// Ensemble des erreurs
        /// </summary>
        public List<SPAN2Error> Errors
        {
            get { return m_errors; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pPosition"></param>
        /// <param name="pAssetExpandedParameters"></param>
        public SPAN2MarginData(IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> pPosition, IEnumerable<AssetExpandedParameter> pAssetExpandedParameters)
        {
            m_errors = new List<SPAN2Error>();
            m_XmlRequestMessage = new List<string>();
            m_XmlResponseMessage = new List<string>();
            //
            m_DiscartedPosition = new List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>>();
            m_GoodPosition = new List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>>();
            //
            m_AssetExpandedParameters = pAssetExpandedParameters;
            //
            if (pPosition != default(IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>>))
            {
                m_MarginPosition = pPosition.ToList();
            }
            else
            {
                m_MarginPosition = new List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>>();
            }
            m_GlobalPosition = m_MarginPosition;
        }

        #endregion Constructors

        #region Methods
        /// <summary>
        /// Convertion et affectation de la devise
        /// </summary>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        public Currency SetCurrency(string pCurrency)
        {
            if (System.Enum.TryParse<Currency>(pCurrency, out Currency parsedCurrency))
            {
                m_Currency = parsedCurrency;
            }
            return m_Currency;
        }

        /// <summary>
        /// Convertion et affectation des paramètres de type de compte Span
        /// </summary>
        /// <param name="pSpanAccountType"></param>
        /// <returns></returns>
        public CustomerAccountType SetAccountType(SpanAccountType pSpanAccountType)
        {
            switch (pSpanAccountType)
            {
                case SpanAccountType.OmnibusSpeculator:
                    m_CustomerAccountType = CustomerAccountType.SPECULATOR;
                    m_OmnibusInd = YesNoIndicator.YES;
                    break;
                case SpanAccountType.OmnibusHedger:
                    m_CustomerAccountType = CustomerAccountType.HEDGE;
                    m_OmnibusInd = YesNoIndicator.YES;
                    break;
                case SpanAccountType.Member:
                    m_CustomerAccountType = CustomerAccountType.MEMBER;
                    m_OmnibusInd = YesNoIndicator.NO;
                    break;
                case SpanAccountType.Default:
                case SpanAccountType.Hedger:
                case SpanAccountType.Normal:
                    m_CustomerAccountType = CustomerAccountType.HEDGE;
                    m_OmnibusInd = YesNoIndicator.NO;
                    break;
                case SpanAccountType.Speculator:
                    m_CustomerAccountType = CustomerAccountType.SPECULATOR;
                    m_OmnibusInd = YesNoIndicator.NO;
                    break;
            }
            return CustomerAccountType;
        }

        /// <summary>
        /// Construction de la classe contenant la position autres informations pour le calcul
        /// </summary> 
        /// <returns></returns>
        public RiskPortfolioRequestMessage CreateRiskPortfolioRequestMessage()
        {
            // Convertion de la position au foramt SPAN2
            IEnumerable<RiskPosition> spanRiskPosition = RiskMethodSpan2Helper.BuildRiskPosition(CssAcronym, AssetExpandedParameters, MarginPosition, IsOmnibus);

            // Compteur de position du message de calcul
            m_NbSpanRiskPosition = spanRiskPosition.Count();

            // Création de la classe nécessaire à la génération du message et alimentation des données de celle-ci
            RequestHeader reqHeader = new RequestHeader
            {
                RequestId = RequestId,
                SentTime = DateTime.UtcNow,
                Version = "1.0"
            };
            //
            PointInTime pointInTime = new PointInTime
            {
                BusinessDt = DtBusiness,
                CycleCode = JsonCycleCode,
                RunNumber = 1
            };
            //
            RiskPortfolioEntities portfolioEntities = new RiskPortfolioEntities
            {
                AccountId = AccountId,
                FirmId = FirmId,
                OriginType = OriginType
            };
            //
            RiskPortfolio riskPortfolio = new RiskPortfolio
            {
                Id = PortfolioId,
                Currency = m_Currency,
                CustomerAccountType = CustomerAccountType,
                OmnibusInd = OmnibusInd,
                //
                Entities = portfolioEntities,
                //
                Positions = spanRiskPosition.ToArray()
            };
            //
            RiskPortfolioRequest portfolioReq = new RiskPortfolioRequest
            {
                PointInTime = pointInTime,
                Portfolios = new RiskPortfolio[1] { riskPortfolio }
            };
            //
            m_RiskPortfolioRequestMessage = new RiskPortfolioRequestMessage
            {
                header = reqHeader,
                payload = portfolioReq
            };
            //
            return m_RiskPortfolioRequestMessage;
        }

        /// <summary>
        /// Construction de la requête JSON contenant la position et autres informations pour le calcul
        /// </summary>
        /// <returns></returns>
        public string CreateJsonRiskPortfolioRequestMessage()
        {
            // Alimentation des class
            CreateRiskPortfolioRequestMessage();

            try
            {
                // Serialization JSON
                m_JsonRiskPortfolioRequestMessage = JsonSerializer.Serialize<RiskPortfolioRequestMessage>(m_RiskPortfolioRequestMessage, SPAN2RiskMargin.JsonSpan2SerializerOptions);
            }
            catch (Exception e)
            {
                // PM 20230830 [XXXXX] Ajout SysMsgCode
                AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "CreateJsonRiskPortfolioRequestMessage: " + e.Message);
            }

            return m_JsonRiskPortfolioRequestMessage;
        }

        /// <summary>
        /// Construction de l'ensemble des lignes CSV constituant la position
        /// </summary>
        /// <returns></returns>
        public string CreateCSVTransactions()
        {
            string posTransaction = string.Empty;

            List<string> allTransaction = CSVTransaction.CreateAllCSVTransactions(this);
            if ((allTransaction != default(List<string>)) && (allTransaction.Count() > 1))
            {
                m_NbSpanRiskPosition = allTransaction.Count() - 1;

                posTransaction = string.Join(Cst.CrLf, allTransaction);
            }

            return posTransaction;
        }

        /// <summary>
        /// Ajout d'une erreur avec un message
        /// </summary>
        /// <param name="pErrMsg"></param>
        /// <param name="pSysMsgCode"></param>
        /// <returns>L'objet erreur</returns>
        // PM 20230830 [XXXXX] Modification signature
        // public void AddError(string pErrMsg, SysMsgCode pSysMsgCode = default)
        public SPAN2Error AddError(SysMsgCode pSysMsgCode, string pErrMsg = default)
        {
            SPAN2Error error = new SPAN2Error(pSysMsgCode, pErrMsg);
            m_errors.Add(error);
            return error;
        }

        /// <summary>
        /// Ajout d'une erreur Span 2 Core
        /// </summary>
        /// <param name="pError"></param>
        public void AddError(error pError)
        {
            if (pError != default(error))
            {
                AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), $"Error {pError.code}: {pError.msg}");
            }
        }

        /// <summary>
        /// Ajout d'un tableau d'erreurs Span 2 Margin
        /// </summary>
        /// <param name="pErrors"></param>
        public void AddError(Error[] pErrors)
        {
            if ((pErrors != default(Error[])) && (pErrors.Length > 0))
            {
                foreach (Error err in pErrors)
                {
                    AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), $"Error {err.Code}: {err.Message}");
                }
            }
        }

        /// <summary>
        /// Retourne une string contenant tous les messages d'erreurs
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            string msg;
            if (m_errors.Count != 0)
            {
                //msg = m_errors.Select(m => m.Message).Aggregate((m1, m2) => $"{m1}{Cst.CrLf}{m2}");
                msg = String.Join(Cst.CrLf, m_errors.Select(m => m.Message));
            }
            else
            {
                msg = default;
            }
            return msg;
        }

        /// <summary>
        /// Retourne une liste d'erreur pour alimenter le log
        /// </summary>
        /// <returns></returns>
        public List<(SysMsgCode, List<LogParam>)> GetErrorListForLog()
        {
            List<(SysMsgCode, List<LogParam>)> errorList = new List<(SysMsgCode, List<LogParam>)>();
            foreach (SPAN2Error error in m_errors)
            {
                errorList.Add(error.GetErrorForLog());
            }
            return errorList;
        }

        /// <summary>
        /// Ajout de la position partielle à la position écartée
        /// </summary>
        public void AddToDiscardPosition()
        {
            m_DiscartedPosition.AddRange(m_MarginPosition);
        }

        /// <summary>
        /// Ajout de la position partielle à la position correcte
        /// </summary>
        public void AddToGoodPosition()
        {
            m_GoodPosition.AddRange(m_MarginPosition);
        }
        #endregion Methods
    }
}
