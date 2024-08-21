using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
//
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin.v1_0_39;

//
namespace EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin
{
    /// <summary>
    /// Convertion de DateTime en DateBusiness pour Json
    /// </summary>
    public class SPAN2BusinessDateJsonConverter : JsonConverter<DateTime>
    {
        #region Methods
        /// <summary>
        /// Lecture d'un DateTime
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        /// <summary>
        /// Ecriture d'un DateTime
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de gestion du SPAN 2 Deployable Margin Software
    /// </summary>
    public class SPAN2RiskMargin
    {
        #region Members
        // Client Http
        private readonly HttpClientHelper m_HttpClientHelper;

        // Nom du point d'accés du service (à ajouter à l'adresse racine)
        private const string m_UrlServiceMargins = "margins/";

        // Adresse de base du point d'accés du service
        private string m_UrlServiceRoot;

        // Uniform Resource Identifier du point d'accès du service
        private Uri m_UriServiceMargins;

        // Options de serialization JSON pous SPAN2
        public static JsonSerializerOptions JsonSpan2SerializerOptions = new JsonSerializerOptions
        {
            //IgnoreNullValues = true,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pHttpClient"></param>
        public SPAN2RiskMargin(HttpClientHelper pHttpClient)
        {
            m_HttpClientHelper = pHttpClient;
            m_HttpClientHelper.HttpClient.BaseAddress = new Uri("http://127.0.0.1:8082/");
            m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Add("username", "API_EFS_TEST");
            m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Add("password", "APIefs.2021");
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Initialisation de l'adresse de base du service et des identifiants de connexion
        /// </summary>
        /// <param name="pRootUrl"></param>
        /// <param name="pUsername"></param>
        /// <param name="pPassword"></param>
        public void SetRiskMarginServiceAccess(string pRootUrl, string pUsername, string pPassword)
        {
            m_UrlServiceRoot = pRootUrl;
            if (m_UrlServiceRoot != default)
            {
                if (false == m_UrlServiceRoot.EndsWith("\\"))
                {
                    m_UrlServiceRoot += "\\";
                }
                m_UriServiceMargins = new Uri(m_UrlServiceRoot + m_UrlServiceMargins);

                m_HttpClientHelper.HttpClient.BaseAddress = new Uri(m_UrlServiceRoot);
            }
            m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Add("username", pUsername);
            m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Add("password", pPassword);
        }

        /// <summary>
        /// Calcul du deposit
        /// </summary>
        /// <param name="pMarginData"></param>
        /// <returns></returns>
        public MarginDetailResponseMessage CalcMargin(SPAN2MarginData pMarginData)
        {
            MarginDetailResponseMessage marginRespMsg = default;

            string jsonMessage = pMarginData.CreateJsonRiskPortfolioRequestMessage();

            // Le contenu du message REST est au format Json
            HttpContent content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

            #if DEBUG
                Task<string> contentString = content.ReadAsStringAsync();
                contentString.Wait();
            #endif

            // Postage du message REST
            Task<HttpResponseMessage> mrgResponseMsg = m_HttpClientHelper.HttpClient.PostAsync("margins", content);
            mrgResponseMsg.Wait();

            // Attente et deserialization de la réponse
            HttpResponseMessage response = mrgResponseMsg.Result;
            HttpContent repContent = response.Content;
            Task<string> repString = repContent.ReadAsStringAsync();
            repString.Wait();
            string repMsg = repString.Result;

            try
            {
                marginRespMsg = (MarginDetailResponseMessage)JsonSerializer.Deserialize<MarginDetailResponseMessage>(repMsg, JsonSpan2SerializerOptions);
            }
            catch (Exception e)
            {
                // PM 20230830 [XXXXX] Ajout SysMsgCode
                pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "CalcMargin: " + e.Message );
            }

            // Réponse Json reçue
            pMarginData.JsonMarginDetailResponseMessage = repMsg;
            pMarginData.MarginDetailResponseMessage = marginRespMsg;

            return marginRespMsg;
        }
        #endregion Methods
    }
}
