using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.RiskMethods.Span2CoreAPI.v1_14;
using EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin;
using EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin.v1_0_39;
//
using FixML.Enum;
//
namespace EFS.SpheresRiskPerformance.RiskMethods.Span2CoreAPI
{
    /// <summary>
    /// Classe de gestion du SPAN 2 CME CORE Margin Service API
    /// </summary>
    public class SPAN2CoreAPI
    {
        #region Members
        /// Nom du paramètre SessionId du Header Http
        private readonly static string m_HeaderSessionId = "sessionId";
        /// Nom du paramètre Username du Header Http
        private readonly static string m_HeaderUsername = "username";
        /// Nom du paramètre Password du Header Http
        private readonly static string m_HeaderPassword = "password";

        // Client Http
        private readonly HttpClientHelper m_HttpClientHelper;

        // Nom des différents points d'accés des services (à ajouter à l'adresse racine)
        private const string m_UrlServicePortfolios = "portfolios/";
        private const string m_UrlServiceTransactions = "transactions/";
        private const string m_UrlServiceMargins = "margins/";
        private const string m_UrlServiceMarginsComplete = "margins?complete=true";
        private const string m_UrlServiceStatusFut = "margins/status/FUT";

        // Adresse de base des points d'accés des services 
        private string m_UrlServiceRoot;
        private string m_ApiUsername;
        private string m_ApiPassword;

        // Uniform Resource Identifier des différents points d'accès des services
        private Uri m_UriServicePortfolios;
        private Uri m_UriServiceTransactions;
        private Uri m_UriServiceMargins;
        private Uri m_UriServiceMarginsComplete;
        private Uri m_UriServiceStatusFut;

        // Ensemble des namespaces utilisés par le CME
        private readonly XmlSerializerNamespaces m_CmeNamespaces = new XmlSerializerNamespaces();

        // Serializer pour les différentes class de messages échangés
        private readonly XmlSerializer m_XsPortfolioReqMsg = new XmlSerializer(typeof(portfolioRequestMessage));
        private readonly XmlSerializer m_XsPortfolioRptMsg = new XmlSerializer(typeof(portfolioReportMessage));
        private readonly XmlSerializer m_XsTransactionReqMsg = new XmlSerializer(typeof(transactionRequestMessage));
        private readonly XmlSerializer m_XsTransactionRptMsg = new XmlSerializer(typeof(transactionReportMessage));
        private readonly XmlSerializer m_XsMarginReqMsg = new XmlSerializer(typeof(marginRequestMessage));
        private readonly XmlSerializer m_XsMarginRptMsg = new XmlSerializer(typeof(marginReportMessage));
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pHttpClient"></param>
        public SPAN2CoreAPI(HttpClientHelper pHttpClient)
        {
            m_HttpClientHelper = pHttpClient;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Renseigne le Header Http avec les paramètres necessaires à CME CORE API
        /// </summary>
        /// <param name="pSessionId">Id de la session</param>
        private void SetHtpHeader(string pSessionId = default)
        {
            RemoveHttpHeader();
            if (StrFunc.IsFilled(pSessionId))
            {

                m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Add(m_HeaderSessionId, pSessionId);
            }
            else
            {
                m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Add(m_HeaderUsername, m_ApiUsername);
                m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Add(m_HeaderPassword, m_ApiPassword);
            }
        }

        /// <summary>
        /// Supprime les paramètres CME CORE API du Http Header
        /// </summary>
        private void RemoveHttpHeader()
        {
            m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Remove(m_HeaderUsername);
            m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Remove(m_HeaderPassword);
            m_HttpClientHelper.HttpClient.DefaultRequestHeaders.Remove(m_HeaderSessionId);
        }

        /// <summary>
        /// Envoie une requête GET vers l'URI spécifié sous forme d'opération asynchrone aprés avoir renseigné l'entête Http
        /// </summary>
        /// <param name="pUri">Uri du web service</param>
        /// <param name="pSessionId">Id de la session</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> HttpGetAsync(Uri pUri, string pSessionId = default)
        {
            HttpResponseMessage responseMessage = default;

            // Autoriser une seule execution simultanée de la méthode
            await m_HttpClientHelper.HttpClientLock.WaitAsync();
            try
            {
                SetHtpHeader(pSessionId);

                Task<HttpResponseMessage> repMsgTask = m_HttpClientHelper.GetAsync(pUri);
                if (repMsgTask != default(Task<HttpResponseMessage>))
                {
                    repMsgTask.Wait();

                    responseMessage = repMsgTask.Result;
                }
            }
            finally
            {
                m_HttpClientHelper.HttpClientLock.Release();
            }

            return responseMessage;
        }

        /// <summary>
        /// Envoie une requête POST vers l'URI spécifié sous forme d'opération asynchrone aprés avoir renseigné l'entête Http
        /// </summary>
        /// <param name="pUri">Uri du web service</param>
        /// <param name="pContent">Contenu à poster</param>
        /// <param name="pSessionId">Id de la session</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> HttpPostAsync(Uri pUri, HttpContent pContent, string pSessionId = default)
        {
            HttpResponseMessage responseMessage = default;

            // Autoriser une seule execution simultanée de la méthode
            await m_HttpClientHelper.HttpClientLock.WaitAsync();
            try
            {
                SetHtpHeader(pSessionId);

                Task<HttpResponseMessage> repMsgTask = m_HttpClientHelper.PostAsync(pUri, pContent);
                if (repMsgTask != default(Task<HttpResponseMessage>))
                {
                    repMsgTask.Wait();

                    responseMessage = repMsgTask.Result;
                }
            }
            finally
            {
                m_HttpClientHelper.HttpClientLock.Release();
            }

            return responseMessage;
        }

        /// <summary>
        /// Envoie une requête DELETE vers l'URI spécifié sous forme d'opération asynchrone aprés avoir renseigné l'entête Http
        /// </summary>
        /// <param name="pUri">Uri du web service</param>
        /// <param name="pSessionId">Id de la session</param>
        /// <returns></returns>
        // PM 20230831 [XXXXX] New
        private async Task<HttpResponseMessage> HttpDeleteAsync(Uri pUri, string pSessionId = default)
        {
            HttpResponseMessage responseMessage = default;

            // Autoriser une seule execution simultanée de la méthode
            await m_HttpClientHelper.HttpClientLock.WaitAsync();
            try
            {
                SetHtpHeader(pSessionId);

                Task<HttpResponseMessage> repMsgTask = m_HttpClientHelper.DeleteAsync(pUri);
                if (repMsgTask != default(Task<HttpResponseMessage>))
                {
                    repMsgTask.Wait();

                    responseMessage = repMsgTask.Result;
                }
            }
            finally
            {
                m_HttpClientHelper.HttpClientLock.Release();
            }

            return responseMessage;
        }

        /// <summary>
        /// Envoie une requête PUT vers l'URI spécifié sous forme d'opération asynchrone aprés avoir renseigné l'entête Http
        /// </summary>
        /// <param name="pUri">Uri du web service</param>
        /// <param name="pContent">Contenu à envoyer</param>
        /// <param name="pSessionId">Id de la session</param>
        /// <returns></returns>
        // PM 20230831 [XXXXX] New
        private async Task<HttpResponseMessage> HttpPutAsync(Uri pUri, HttpContent pContent, string pSessionId = default)
        {
            HttpResponseMessage responseMessage = default;

            // Autoriser une seule execution simultanée de la méthode
            await m_HttpClientHelper.HttpClientLock.WaitAsync();
            try
            {
                SetHtpHeader(pSessionId);

                Task<HttpResponseMessage> repMsgTask = m_HttpClientHelper.PutAsync(pUri, pContent);
                if (repMsgTask != default(Task<HttpResponseMessage>))
                {
                    repMsgTask.Wait();

                    responseMessage = repMsgTask.Result;
                }
            }
            finally
            {
                m_HttpClientHelper.HttpClientLock.Release();
            }

            return responseMessage;
        }

        /// <summary>
        /// Envoie une requête GET vers l'URI spécifié avec gestion de reconnexion à une session
        /// </summary>
        /// <param name="pUri">Uri du web service</param>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        // PM 20230831 [XXXXX] New
        private HttpResponseMessage HttpGet(Uri pUri, SPAN2MarginData pRequestData)
        {
            HttpResponseMessage response = default;
            if ((pRequestData != default(SPAN2MarginData)) && (pUri != default))
            {
                try
                {
                    int tryCount = 1;
                    int tryCountMax = 3;
                    do
                    {
                        try
                        {
                            Task<HttpResponseMessage> repMsgTask = HttpGetAsync(pUri, pRequestData.ApiSessionId);
                            if (repMsgTask != default(Task<HttpResponseMessage>))
                            {
                                repMsgTask.Wait();

                                response = repMsgTask.Result;

                                if (response.IsSuccessStatusCode)
                                {
                                    // Ok -> Exit
                                    tryCount = tryCountMax;
                                }
                                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    // Re-authentification and retry
                                    NewSession(pRequestData);
                                    tryCount += 1;
                                }
                                else
                                {
                                    // Exit
                                    tryCount = tryCountMax;
                                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpGet: " + response.ReasonPhrase);
                                }
                            }
                            else
                            {
                                // Exit 
                                tryCount = tryCountMax;
                                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpGet: No response");
                            }
                        }
                        catch
                        {
                            // Re-authentification and retry
                            NewSession(pRequestData);
                            tryCount += 1;
                            if (tryCount >= tryCountMax)
                            {
                                throw;
                            }
                        }
                    }
                    while (tryCount < tryCountMax);
                }
                catch (AggregateException ae)
                {
                    foreach (Exception e in ae.Flatten().InnerExceptions)
                    {
                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpGet: " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpGet: " + e.Message);
                }
            }
            return response;
        }

        /// <summary>
        /// Envoie une requête POST vers l'URI spécifié avec gestion de reconnexion à une session
        /// </summary>
        /// <param name="pUri">Uri du web service</param>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <param name="pContent">Contenu à poster</param>
        /// <returns></returns>
        // PM 20230831 [XXXXX] New
        private HttpResponseMessage HttpPost(Uri pUri, SPAN2MarginData pRequestData, HttpContent pContent)
        {
            HttpResponseMessage response = default;
            if ((pRequestData != default(SPAN2MarginData)) && (pUri != default))
            {
                try
                {
                    int tryCount = 1;
                    int tryCountMax = 3;
                    do
                    {
                        try
                        {
                            Task<HttpResponseMessage> repMsgTask = HttpPostAsync(pUri, pContent, pRequestData.ApiSessionId);
                            if (repMsgTask != default(Task<HttpResponseMessage>))
                            {
                                repMsgTask.Wait();

                                response = repMsgTask.Result;

                                if (response.IsSuccessStatusCode)
                                {
                                    // Ok -> Exit
                                    tryCount = tryCountMax;
                                }
                                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    // Re-authentification and retry
                                    NewSession(pRequestData);
                                    tryCount += 1;
                                }
                                else
                                {
                                    // Exit
                                    tryCount = tryCountMax;
                                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpPost: " + response.ReasonPhrase);
                                }
                            }
                            else
                            {
                                // Exit
                                tryCount = tryCountMax;
                                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpPost: No response");
                            }
                        }
                        catch
                        {
                            // Re-authentification and retry
                            NewSession(pRequestData);
                            tryCount += 1;
                            if (tryCount >= tryCountMax)
                            {
                                throw;
                            }
                        }
                    }
                    while (tryCount < tryCountMax);
                }
                catch (AggregateException ae)
                {
                    foreach (Exception e in ae.Flatten().InnerExceptions)
                    {
                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpPost: " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpPost: " + e.Message);
                }
            }
            return response;
        }

        /// <summary>
        /// Envoie une requête DELETE vers l'URI spécifié avec gestion de reconnexion à une session
        /// </summary>
        /// <param name="pUri">Uri du web service</param>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        // PM 20230831 [XXXXX] New
        private HttpResponseMessage HttpDelete(Uri pUri, SPAN2MarginData pRequestData)
        {
            HttpResponseMessage response = default;
            if ((pRequestData != default(SPAN2MarginData)) && (pUri != default))
            {
                try
                {
                    int tryCount = 1;
                    int tryCountMax = 3;
                    do
                    {
                        try
                        {
                            Task<HttpResponseMessage> repMsgTask = HttpDeleteAsync(pUri, pRequestData.ApiSessionId);
                            if (repMsgTask != default(Task<HttpResponseMessage>))
                            {
                                repMsgTask.Wait();

                                response = repMsgTask.Result;

                                if (response.IsSuccessStatusCode)
                                {
                                    // Ok -> Exit
                                    tryCount = tryCountMax;
                                }
                                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    // Re-authentification and retry
                                    NewSession(pRequestData);
                                    tryCount += 1;
                                }
                                else
                                {
                                    // Exit
                                    tryCount = tryCountMax;
                                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpDelete: " + response.ReasonPhrase);
                                }
                            }
                            else
                            {
                                // Exit
                                tryCount = tryCountMax;
                                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpDelete: No response");
                            }
                        }
                        catch
                        {
                            // Re-authentification and retry
                            NewSession(pRequestData);
                            tryCount += 1;
                            if (tryCount >= tryCountMax)
                            {
                                throw;
                            }
                        }
                    }
                    while (tryCount < tryCountMax);
                }
                catch (AggregateException ae)
                {
                    foreach (Exception e in ae.Flatten().InnerExceptions)
                    {
                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpDelete: " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpDelete: " + e.Message);
                }
            }
            return response;
        }

        /// <summary>
        /// Envoie une requête PUT vers l'URI spécifié avec gestion de reconnexion à une session
        /// </summary>
        /// <param name="pUri">Uri du web service</param>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <param name="pContent">Contenu à envoyer</param>
        /// <returns></returns>
        // PM 20230831 [XXXXX] New
        private HttpResponseMessage HttpPut(Uri pUri, SPAN2MarginData pRequestData, HttpContent pContent)
        {
            HttpResponseMessage response = default;
            if ((pRequestData != default(SPAN2MarginData)) && (pUri != default))
            {
                try
                {
                    int tryCount = 1;
                    int tryCountMax = 3;
                    do
                    {
                        try
                        {
                            Task<HttpResponseMessage> repMsgTask = HttpPutAsync(pUri, pContent, pRequestData.ApiSessionId);
                            if (repMsgTask != default(Task<HttpResponseMessage>))
                            {
                                repMsgTask.Wait();

                                response = repMsgTask.Result;

                                if (response.IsSuccessStatusCode)
                                {
                                    // Ok -> Exit
                                    tryCount = tryCountMax;
                                }
                                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    // Re-authentification and retry
                                    NewSession(pRequestData);
                                    tryCount += 1;
                                }
                                else
                                {
                                    // Exit
                                    tryCount = tryCountMax;
                                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpPut: " + response.ReasonPhrase);
                                }
                            }
                            else
                            {
                                // Exit
                                tryCount = tryCountMax;
                                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpPut: No response");
                            }
                        }
                        catch
                        {
                            // Re-authentification and retry
                            NewSession(pRequestData);
                            tryCount += 1;
                            if (tryCount >= tryCountMax)
                            {
                                throw;
                            }
                        }
                    }
                    while (tryCount < tryCountMax);
                }
                catch (AggregateException ae)
                {
                    foreach (Exception e in ae.Flatten().InnerExceptions)
                    {
                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpPut: " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "HttpPut: " + e.Message);
                }
            }
            return response;
        }

        /// <summary>
        /// Initialisation de l'adresse de base du service et des identifiants de connexion
        /// </summary>
        /// <param name="pRootUrl">Url de base du web service</param>
        /// <param name="pUsername">Nom d'utilisateur</param>
        /// <param name="pPassword">Mot de passe</param>
        public void SetCoreServiceAccess(string pRootUrl, string pUsername, string pPassword, string pCMECoreNamespace)
        {
            // Ajout des namespaces utilisés par le CME
            if (StrFunc.IsEmpty(pCMECoreNamespace))
            {
                // Schema 1.12 par defaut
                pCMECoreNamespace = "http://cmegroup.com/schema/core/1.12";
            }
            m_CmeNamespaces.Add("ns2", pCMECoreNamespace);
            m_CmeNamespaces.Add("ns3", "http://www.w3.org/2000/09/xmldsig#");
            m_CmeNamespaces.Add("ns4", "http://www.fpml.org/FpML-5/confirmation");
            m_CmeNamespaces.Add("ns5", "http://www.cmegroup.com/otc-clearing/confirmation");
            m_CmeNamespaces.Add("ns6", "http://www.cmegroup.com/fixml50/1");
            //
            m_UrlServiceRoot = pRootUrl;
            if (m_UrlServiceRoot != default)
            {
                if (false == m_UrlServiceRoot.EndsWith("\\"))
                {
                    m_UrlServiceRoot += "\\";
                }
                m_UriServicePortfolios = new Uri(m_UrlServiceRoot + m_UrlServicePortfolios);
                m_UriServiceTransactions = new Uri(m_UrlServiceRoot + m_UrlServiceTransactions);
                m_UriServiceMargins = new Uri(m_UrlServiceRoot + m_UrlServiceMargins);
                m_UriServiceMarginsComplete = new Uri(m_UrlServiceRoot + m_UrlServiceMarginsComplete);
                m_UriServiceStatusFut = new Uri(m_UrlServiceRoot + m_UrlServiceStatusFut);
            }
            //
            m_ApiUsername = pUsername;
            m_ApiPassword = pPassword;
        }

        /// <summary>
        /// Requête permettant d'obtenir ou de récupérer le statut (heartbeat) de CME CORE par famille de produit, Futures est ici spécifié.
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns>Un HttpResponseMessage</returns>
        public HttpResponseMessage ResponseStatusFut(SPAN2MarginData pRequestData)
        {
            HttpResponseMessage response = default;
            try
            {
                RiskMethodSpan2Helper.WaitGetOther();
                // Attention ici il faut bien utiliser HttpGetAsync et pas HttpGet
                Task<HttpResponseMessage> statusRespMsg = HttpGetAsync(m_UriServiceStatusFut);
                statusRespMsg.Wait();

                response = statusRespMsg.Result;
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.Flatten().InnerExceptions)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "ResponseStatusFut: " + e.Message);
                }
            }
            catch (Exception e)
            {
                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "ResponseStatusFut: " + e.Message);
            }
            return response;
        }

        /// <summary>
        /// Requête permettant d'obtenir ou de récupérer le statut (heartbeat) de CME CORE par famille de produit, Futures est ici spécifié.
        /// Si la famille de produit est disponible pour le calcul de déposit, l'appel retourne une réponse 200 OK.
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns>
        /// 200	OK - Margining available	
        /// 401 Unauthorized
        /// 404	NotFound - Returned if GET /margins/status is sent and the Margin Services API is unavailable.
        /// 501	NotImplemented - Returned if Client enters a an Incorrect product.
        /// 503	ServiceUnavailable - Returned if GET /margins/status/{product} is sent and the product is unavailable.
        /// </returns>
        public HttpStatusCode StatusFut(SPAN2MarginData pRequestData)
        {
            HttpStatusCode status = HttpStatusCode.BadRequest;

            HttpResponseMessage response = ResponseStatusFut(pRequestData);
            if (response != default)
            {
                status = response.StatusCode;
            }
            return status;
        }

        /// <summary>
        /// Indique si le service de calcul de déposit est disponible pour les Futures
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns> True si disponible, sinon False</returns>
        public bool IsStatusFutOk(SPAN2MarginData pRequestData)
        {
            return (StatusFut(pRequestData) == HttpStatusCode.OK);
        }

        /// <summary>
        /// Envoie d'une requête de status pour obtenir une session
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        public bool NewSession(SPAN2MarginData pRequestData)
        {
            bool isOk = false;
            pRequestData.ApiSessionId = default;

            // Envoie d'une requête de status juste pour obtenir une session
            HttpResponseMessage response = ResponseStatusFut(pRequestData);
            if (response != default)
            {
                //
                HttpStatusCode status = response.StatusCode;
                isOk = (HttpStatusCode.OK == status);
                if (isOk)
                {
                    if (response.Headers != default(HttpResponseHeaders))
                    {
                        if (response.Headers.TryGetValues(m_HeaderSessionId, out IEnumerable<string> sessionIdValues))
                        {
                            pRequestData.ApiSessionId = sessionIdValues.FirstOrDefault();
                        }
                    }
                }
                else
                {
                    // Service unavailable
                    int statusCode = ((int)status);
                    // PM 20230830 [XXXXX] Ajout SysMsgCode
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), $"Span 2 Core Api: NewSession: Service unavailable : StatusCode {statusCode} {status}");
                }
            }
            else
            {
                // PM 20230830 [XXXXX] Ajout SysMsgCode
                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Span 2 Core Api: NewSession: No response to Status request");
            }
            return isOk;
        }

        /// <summary>
        /// Requête permettant de répertorier tous les portefeuilles qui ont été ajoutés à CME CORE
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns>Un message contenant tous les portefeuilles</returns>
        public portfolioReportMessage ListPortfolios(SPAN2MarginData pRequestData)
        {
            portfolioReportMessage portfolioRptMsg = default;
            try
            {
                // Get du message REST
                RiskMethodSpan2Helper.WaitGetPortfolio();
                HttpResponseMessage response = HttpGet(m_UriServicePortfolios, pRequestData);
                if (response != default)
                {
                    HttpContent content = response.Content;
                    Task<string> repString = content.ReadAsStringAsync();
                    repString.Wait();

                    string repMsg = repString.Result;

                    // Lecture des données en retour
                    StringReader reader = new StringReader(repMsg);
                    portfolioRptMsg = (portfolioReportMessage)m_XsPortfolioRptMsg.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "ListPortfolios: " + e.Message);
            }
            return portfolioRptMsg;
        }

        /// <summary>
        /// Requête permettant d'ajouter ou de créer un nouveau portefeuille dans CME CORE
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        public portfolioReportMessage AddPortfolios(SPAN2MarginData pRequestData)
        {
            portfolioReportMessage portfolioRptMsg = default;

            if (pRequestData != default(SPAN2MarginData))
            {
                // Alimentation des données nécessaire à la génération du message XML de la requête
                portfolioEntities entities = new portfolioEntities
                {
                    clrMbrFirmId = pRequestData.ActorId.ToString(),
                    clrOrgId = pRequestData.ActorId.ToString(),
                    custAcctId = pRequestData.ActorId.ToString(),
                    pbAcctId = pRequestData.ActorId.ToString(),
                };
                portfolio port = new portfolio
                {
                    id = pRequestData.BookId.ToString(),
                    desc = pRequestData.BookId.ToString(),
                    name = pRequestData.BookId.ToString(),
                    rptCcy = currency.EUR,
                    rptCcySpecified = true,
                    entities = entities,
                };
                portfolioRequestMessage portfolioReqMsg = new portfolioRequestMessage
                {
                    portfolio = port,
                };

                try
                {
                    // Génération du message XML de la requête
                    StringBuilder sb = new StringBuilder();
                    StringWriterWithEncoding writer = new StringWriterWithEncoding(sb, Encoding.UTF8);

                    m_XsPortfolioReqMsg.Serialize(writer, portfolioReqMsg, m_CmeNamespaces);

                    // Le contenu du message REST est au format XML
                    string xmlRequest = sb.ToString();
                    pRequestData.XmlRequestMessage.Add(xmlRequest);
                    HttpContent content = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");

                    // Postage du message REST
                    RiskMethodSpan2Helper.WaitPostPortfolio();
                    HttpResponseMessage response = HttpPost(m_UriServicePortfolios, pRequestData, content);
                    if (response != default)
                    {
                        HttpContent repContent = response.Content;
                        if (repContent != default(HttpContent))
                        {
                            Task<string> repString = repContent.ReadAsStringAsync();
                            repString.Wait();

                            string repMsg = repString.Result;

                            // Lecture des données en retour
                            StringReader reader = new StringReader(repMsg);
                            portfolioRptMsg = (portfolioReportMessage)m_XsPortfolioRptMsg.Deserialize(reader);
                        }
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "AddPortfolios: " + e.Message);
                }
            }
            return portfolioRptMsg;
        }

        /// <summary>
        /// Supprime tous les portefeuilles créés avant la date en paramètre
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <param name="pDateMax">Suppression des portefeuilles crées antérieurement à cette date</param>
        /// <returns></returns>
        // PM 20230831 [XXXXX] New
        public List<portfolioReportMessage> DeleteAllPortfolios(SPAN2MarginData pRequestData, DateTime pDateMax)
        {
            List<portfolioReportMessage> portfolioRptMsgList = new List<portfolioReportMessage>();
            if (pRequestData != default(SPAN2MarginData))
            {
                portfolioReportMessage portfolioList = ListPortfolios(pRequestData);

                if (portfolioList != default)
                {
                    portfolio[] portfolioArray = portfolioList.portfolio;

                    if ((portfolioArray != default(portfolio[])) && (portfolioArray.Count() > 0))
                    {
                        foreach (portfolio port in portfolioArray)
                        {
                            if (port.createTimeSpecified)
                            {
                                DateTime creationDate = port.createTime;
                                if (creationDate < pDateMax)
                                {
                                    try
                                    {
                                        Uri uriDelete = new Uri(m_UriServicePortfolios.OriginalString + port.id);

                                        // Envoie du message REST de delete
                                        RiskMethodSpan2Helper.WaitGetOther();
                                        HttpResponseMessage response = HttpDelete(uriDelete, pRequestData);

                                        if ((response != default) && response.IsSuccessStatusCode)
                                        {
                                            HttpContent repContent = response.Content;
                                            Task<string> repString = repContent.ReadAsStringAsync();
                                            repString.Wait();

                                            string repMsg = repString.Result;

                                            // Lecture des données en retour
                                            StringReader reader = new StringReader(repMsg);
                                            portfolioReportMessage portfolioRptMsg = (portfolioReportMessage)m_XsPortfolioRptMsg.Deserialize(reader);

                                            portfolioRptMsgList.Add(portfolioRptMsg);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "DeleteAllPortfolios: " + e.Message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return portfolioRptMsgList;
        }

        /// <summary>
        /// Supprime le portefeuille indiqué dans pMarginData
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        // PM 20230831 [XXXXX] New
        public portfolioReportMessage DeletePortfolio(SPAN2MarginData pRequestData)
        {
            portfolioReportMessage portfolioRptMsg = default;
            if ((pRequestData != default(SPAN2MarginData)) && (StrFunc.IsFilled(pRequestData.ApiPortfolioId)))
            {
                try
                {
                    Uri uriDelete = new Uri(m_UriServicePortfolios.OriginalString + pRequestData.ApiPortfolioId);

                    // Envoie du message REST de delete
                    RiskMethodSpan2Helper.WaitGetOther();
                    HttpResponseMessage response = HttpDelete(uriDelete, pRequestData);

                    if ((response != default) && response.IsSuccessStatusCode)
                    {
                        HttpContent repContent = response.Content;
                        Task<string> repString = repContent.ReadAsStringAsync();
                        repString.Wait();

                        string repMsg = repString.Result;

                        // Lecture des données en retour
                        StringReader reader = new StringReader(repMsg);
                        portfolioRptMsg = (portfolioReportMessage)m_XsPortfolioRptMsg.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "DeletePortfolio: " + e.Message);
                }
            }
            return portfolioRptMsg;
        }

        /// <summary>
        /// Suppression d'un trade (Transaction) d'un protefeuille
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <param name="pTransaction">Données de la transaction (trade)</param>
        public void DeleteTransaction(SPAN2MarginData pRequestData, transaction pTransaction)
        {
            if (pTransaction != default(transaction))
            {
                try
                {
                    string transId = pTransaction.id;
                    if (StrFunc.IsFilled(transId))
                    {
                        Uri uriDelete = new Uri(m_UriServiceTransactions.OriginalString + transId);

                        // Envoie du message REST de delete
                        RiskMethodSpan2Helper.WaitGetOther();
                        HttpResponseMessage response = HttpDelete(uriDelete, pRequestData);

                        if ((response != default) && response.IsSuccessStatusCode)
                        {
                            HttpContent repContent = response.Content;
                            Task<string> repString = repContent.ReadAsStringAsync();
                            repString.Wait();

                            string repMsg = repString.Result;

                            // Lecture des données en retour
                            StringReader reader = new StringReader(repMsg);
                            transactionReportMessage transactionRptMsg = (transactionReportMessage)m_XsTransactionRptMsg.Deserialize(reader);
                        }
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "DeleteTransaction: " + e.Message);
                }
            }
        }

        /// <summary>
        /// Requête permettant d'ajouter des trades au format CSV
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        // PM 20230929 [XXXXX] new
        public transactionReportMessage AddTransactionsCSV(SPAN2MarginData pRequestData)
        {
            transactionReportMessage transactionRptMsg = default;

            if (pRequestData != default(SPAN2MarginData))
            { 
                // Construction de l'ensemble des lignes CSV constituant la position
                string csvTransactions = pRequestData.CreateCSVTransactions();

                if (StrFunc.IsFilled(csvTransactions))
                {
                    // Alimentation des données nécessaire à la génération du message XML de la requête
                    transactionPayload transPayload = new transactionPayload
                    {
                        format = transactionFormat.CSV,
                        encoding = transactionEncoding.STRING,
                        encodingSpecified = true,
                        Item = csvTransactions
                    };
                    //
                    transaction trans = new transaction
                    {
                        portfolioId = "0",
                        type = transactionType.TRADE,
                        typeSpecified = true,
                        payload = transPayload
                    };
                    //
                    transactionRequestMessage transReqMsg = new transactionRequestMessage
                    {
                        transaction = new transaction[1] { trans }
                    };

                    try
                    {
                        // Génération du message XML de la requête
                        StringBuilder sb = new StringBuilder();
                        StringWriterWithEncoding writer = new StringWriterWithEncoding(sb, Encoding.UTF8);
                        m_XsTransactionReqMsg.Serialize(writer, transReqMsg, m_CmeNamespaces);

                        // Le contenu du message REST est au format XML
                        string xmlRequest = sb.ToString();
                        pRequestData.XmlRequestMessage.Add(xmlRequest);
                        HttpContent content = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");

                        // Postage du message REST
                        RiskMethodSpan2Helper.WaitPostTransaction();
                        HttpResponseMessage response = HttpPost(m_UriServiceTransactions, pRequestData, content);
                        if (response != default)
                        {
                            HttpContent repContent = response.Content;
                            if (repContent != default(HttpContent))
                            {
                                Task<string> repString = repContent.ReadAsStringAsync();
                                repString.Wait();

                                string xmlRepMsg = repString.Result;
                                pRequestData.XmlResponseMessage.Add(xmlRepMsg);

                                // Lecture des données en retour
                                StringReader reader = new StringReader(xmlRepMsg);
                                transactionRptMsg = (transactionReportMessage)m_XsTransactionRptMsg.Deserialize(reader);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "AddTransactionsCSV: " + e.Message);
                    }
                }
            }
            return transactionRptMsg;
        }

        /// <summary>
        /// Requête permettant d'ajouter des trades au format Json à un portefeuille
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        public transactionReportMessage AddTransactionsJson(SPAN2MarginData pRequestData)
        {
            transactionReportMessage transactionRptMsg = default;

            if (pRequestData != default(SPAN2MarginData))
            {
                string jsonString = pRequestData.CreateJsonRiskPortfolioRequestMessage();

                if (StrFunc.IsFilled(jsonString))
                {
                    // Alimentation des données nécessaire à la génération du message XML de la requête
                    transactionPayload transPayload = new transactionPayload
                    {
                        format = transactionFormat.RISK_API_JSON,
                        encoding = transactionEncoding.STRING,
                        encodingSpecified = true,
                        Item = jsonString,
                    };
                    transaction trans = new transaction
                    {
                        portfolioId = pRequestData.ApiPortfolioId,
                        type = transactionType.TRADE,
                        typeSpecified = true,
                        payload = transPayload,
                    };
                    transactionRequestMessage transReqMsg = new transactionRequestMessage
                    {
                        transaction = new transaction[1] { trans }
                    };

                    try
                    {
                        // Génération du message XML de la requête
                        StringBuilder sb = new StringBuilder();
                        StringWriterWithEncoding writer = new StringWriterWithEncoding(sb, Encoding.UTF8);
                        m_XsTransactionReqMsg.Serialize(writer, transReqMsg, m_CmeNamespaces);

                        // Le contenu du message REST est au format XML
                        string xmlRequest = sb.ToString();
                        pRequestData.XmlRequestMessage.Add(xmlRequest);
                        HttpContent content = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");

                        // Postage du message REST
                        RiskMethodSpan2Helper.WaitPostTransaction();
                        HttpResponseMessage response = HttpPost(m_UriServiceTransactions, pRequestData, content);
                        if (response != default)
                        {
                            HttpContent repContent = response.Content;
                            if (repContent != default(HttpContent))
                            {
                                Task<string> repString = repContent.ReadAsStringAsync();
                                repString.Wait();

                                string xmlRepMsg = repString.Result;
                                pRequestData.XmlResponseMessage.Add(xmlRepMsg);

                                // Lecture des données en retour
                                StringReader reader = new StringReader(xmlRepMsg);
                                transactionRptMsg = (transactionReportMessage)m_XsTransactionRptMsg.Deserialize(reader);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "AddTransactionsJson: " + e.Message);
                    }
                }
            }
            return transactionRptMsg;
        }

        /// <summary>
        /// Requête permettant de lancer le calcul de déposit sur un portefeuille
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <param name="pIsWithCycle">Indique s'il faut renseigner l'élément cycle</param>
        /// <returns></returns>
        public marginReportMessage CalcMargin(SPAN2MarginData pRequestData, bool pIsWithCycle = true)
        {
            marginReportMessage marginMsg = default;

            if (pRequestData != default(SPAN2MarginData))
            {
                // Alimentation des données nécessaire à la génération du message XML de la requête
                margin mrg = new margin
                {
                    portfolioId = pRequestData.ApiPortfolioId,
                    riskFramework = riskFramework.NEXT,
                    riskFrameworkSpecified = true
                };
                marginRequestMessage marginReqMsg;
                if (pIsWithCycle)
                {
                    cycle mrgCycle = new cycle
                    {
                        date = pRequestData.DtBusiness,
                        dateSpecified = true,
                        code = cycleCode.EOD,
                        codeSpecified = true
                    };
                    marginReqMsg = new marginRequestMessage
                    {
                        cycle = mrgCycle,
                        margin = mrg
                    };
                }
                else
                {
                    marginReqMsg = new marginRequestMessage
                    {
                        margin = mrg
                    };
                }

                try
                {
                    // Génération du message XML de la requête
                    StringBuilder sb = new StringBuilder();
                    StringWriterWithEncoding writer = new StringWriterWithEncoding(sb, Encoding.UTF8);
                    m_XsMarginReqMsg.Serialize(writer, marginReqMsg, m_CmeNamespaces);

                    // Le contenu du message REST est au format XML
                    string xmlRequest = sb.ToString();
                    pRequestData.XmlRequestMessage.Add(xmlRequest);
                    HttpContent content = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");

                    // Postage du message REST
                    RiskMethodSpan2Helper.WaitPostMargin();
                    HttpResponseMessage response = HttpPost(m_UriServiceMargins, pRequestData, content);
                    if (response != default)
                    {
                        HttpContent repContent = response.Content;
                        if (repContent != default(HttpContent))
                        {
                            Task<string> repString = repContent.ReadAsStringAsync();
                            repString.Wait();

                            string xmlRepMsg = repString.Result;
                            pRequestData.XmlResponseMessage.Add(xmlRepMsg);

                            // Lecture des données en retour
                            StringReader reader = new StringReader(xmlRepMsg);
                            marginMsg = (marginReportMessage)m_XsMarginRptMsg.Deserialize(reader);
                        }
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "CalcMargin: " + e.Message);
                }
            }
            return marginMsg;
        }

        /// <summary>
        /// Requête permettant d'obtenir l'état d'achevement d'un calcul de déposit
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        public marginReportMessage GetMargin(SPAN2MarginData pRequestData)
        {
            marginReportMessage marginMsg = default;
            if (pRequestData != default(SPAN2MarginData))
            {
                try
                {
                    Uri uriGetMargin = new Uri(m_UriServiceMargins.OriginalString + pRequestData.ApiMarginId);

                    // Envoie du message REST de get
                    RiskMethodSpan2Helper.WaitGetMargin();
                    HttpResponseMessage response = HttpGet(uriGetMargin, pRequestData);
                    if (response != default)
                    {
                        HttpContent repContent = response.Content;
                        Task<string> repString = repContent.ReadAsStringAsync();
                        repString.Wait();

                        pRequestData.GetMarginXmlResponseMessage = repString.Result;

                        // Lecture des données en retour
                        StringReader reader = new StringReader(pRequestData.GetMarginXmlResponseMessage);
                        marginMsg = (marginReportMessage)m_XsMarginRptMsg.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "GetMargin: " + e.Message);
                }
            }
            return marginMsg;
        }

        /// <summary>
        /// Attends l'achèvement d'un calcul de déposit
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        public marginReportMessage WaitMargin(SPAN2MarginData pRequestData)
        {
            marginReportMessage mrgRepMsg = default;
            if (pRequestData != default(SPAN2MarginData))
            {
                int loopGuard = 120; // Interrompre aprés 10 minutes max
                mrgRepMsg = GetMargin(pRequestData);
                while ((mrgRepMsg != default) && (mrgRepMsg.status == asyncReportStatus.PROCESSING) && (loopGuard > 0))
                {
                    // Attendre 5 secondes
                    Thread.Sleep(RiskMethodSpan2Helper.GetMarginDelay);

                    mrgRepMsg = GetMargin(pRequestData);
                    loopGuard -= 1;
                }
                if (loopGuard == 0)
                {
                    if (mrgRepMsg == default)
                    {
                        mrgRepMsg = new marginReportMessage();
                    }
                    mrgRepMsg.status = asyncReportStatus.ERROR;
                    mrgRepMsg.error = new error { code = string.Empty, msg = "Too many attempts to get the result of the initial margin." };
                }
                // Ajout de la dernière réponse XML reçue dans la liste des réponses XML
                pRequestData.XmlResponseMessage.Add(pRequestData.GetMarginXmlResponseMessage);
            }
            return mrgRepMsg;
        }

        /// <summary>
        /// Requête permettant d'obtenir le détail du résultat d'un calcul de déposit
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <param name="pDetailValue">Détail du calcul</param>
        /// <returns></returns>
        public MarginDetailResponseMessage GetRiskMarginDetail(SPAN2MarginData pRequestData, string pDetailValue)
        {
            MarginDetailResponseMessage detail = default;
            if (pDetailValue != default)
            {
                try
                {
                    detail = (MarginDetailResponseMessage)JsonSerializer.Deserialize<MarginDetailResponseMessage>(pDetailValue, SPAN2RiskMargin.JsonSpan2SerializerOptions);
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "GetRiskMarginDetail: " + e.Message);
                }
            }
            return detail;
        }

        /// <summary>
        /// Requête permettant d'obtenir la liste des calculs de déposit effectués ce jour
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        public marginReportMessage GetMarginList(SPAN2MarginData pRequestData)
        {
            marginReportMessage marginMsg = default;
            if (pRequestData != default(SPAN2MarginData))
            {
                try
                {
                    RiskMethodSpan2Helper.WaitGetMargin();
                    HttpResponseMessage response = HttpGet(m_UriServiceMargins, pRequestData);
                    if (response != default)
                    {
                        HttpContent repContent = response.Content;
                        Task<string> repString = repContent.ReadAsStringAsync();
                        repString.Wait();

                        string repMsg = repString.Result;

                        StringReader reader = new StringReader(repMsg);
                        marginMsg = (marginReportMessage)m_XsMarginRptMsg.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "GetMarginList: " + e.Message);
                }
            }
            return marginMsg;
        }

        /// <summary>
        /// Génération et envoie d'une requete de position et de calcul
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <param name="pIsWithCycle">Indique s'il faut renseigner l'élément cycle</param>
        /// <returns></returns>
        public marginReportMessage MarginCompleteJson(SPAN2MarginData pRequestData, bool pIsWithCycle = true)
        {
            marginReportMessage marginRptMsg = default;

            if (pRequestData != default(SPAN2MarginData))
            {
                // Génération du message Json contenant la position
                string jsonMessage = pRequestData.CreateJsonRiskPortfolioRequestMessage();

                if (StrFunc.IsFilled(jsonMessage))
                {
                    // Alimentation des données nécessaire à la génération du message XML de la requête
                    transactionPayload transPayload = new transactionPayload
                    {
                        format = transactionFormat.RISK_API_JSON,
                        encoding = transactionEncoding.STRING,
                        encodingSpecified = true,
                        Item = jsonMessage
                    };
                    //
                    transaction trans = new transaction
                    {
                        type = transactionType.TRADE,
                        typeSpecified = true,
                        payload = transPayload
                    };
                    //
                    margin mrg = new margin
                    {
                        riskFramework = riskFramework.NEXT,
                        riskFrameworkSpecified = true,
                        transactions = new transaction[1] { trans }
                    };
                    //
                    marginRequestMessage marginReqMsg;
                    if (pIsWithCycle)
                    {
                        cycle mrgCycle = new cycle
                        {
                            code = (pRequestData.Timing == SettlSessIDEnum.Intraday ? cycleCode.ITD : cycleCode.EOD),
                            codeSpecified = true,
                            date = pRequestData.DtBusiness,
                            dateSpecified = true
                        };
                        //
                        marginReqMsg = new marginRequestMessage
                        {
                            margin = mrg,
                            cycle = mrgCycle
                        };
                    }
                    else
                    {
                        marginReqMsg = new marginRequestMessage
                        {
                            margin = mrg
                        };
                    }
                    //
                    try
                    {
                        // Génération du message XML de la requête
                        StringBuilder sb = new StringBuilder();
                        StringWriterWithEncoding writer = new StringWriterWithEncoding(sb, Encoding.UTF8);
                        m_XsMarginReqMsg.Serialize(writer, marginReqMsg, m_CmeNamespaces);

                        // Le contenu du message REST est au format XML
                        string xmlRequest = sb.ToString();
                        pRequestData.XmlRequestMessage.Add(xmlRequest);
                        HttpContent content = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");

                        // Postage du message REST
                        RiskMethodSpan2Helper.WaitPostMargin();
                        HttpResponseMessage response = HttpPost(m_UriServiceMarginsComplete, pRequestData, content);
                        if (response != default)
                        {
                            HttpContent repContent = response.Content;
                            if (repContent != default(HttpContent))
                            {
                                Task<string> repString = repContent.ReadAsStringAsync();
                                repString.Wait();

                                string repMsg = repString.Result;

                                // Lecture des données en retour
                                StringReader reader = new StringReader(repMsg);
                                marginRptMsg = (marginReportMessage)m_XsMarginRptMsg.Deserialize(reader);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "MarginCompleteJson: " + e.Message);
                    }
                }
            }
            return marginRptMsg;
        }

        /// <summary>
        /// Attends l'achèvement du calcul de déposit et obtient le résultat
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <param name="pMarginArray"></param>
        /// <returns></returns>
        public bool WaitAndGetMargin(SPAN2CalcMethCom pMethodComObj, SPAN2MarginData pRequestData, margin[] pMarginArray)
        {
            bool isMarginAvailable = false;
            MarginDetailResponseMessage marginDetail = default;
            if (ArrFunc.IsFilled(pMarginArray))
            {
                if (pMarginArray.Length == 1)
                {
                    margin mrg = pMarginArray[0];
                    if (mrg != default(margin))
                    {
                        pRequestData.ApiMarginId = mrg.id;
                        pRequestData.ApiPortfolioId = mrg.portfolioId;
                        marginReportMessage mrgRepMsg = WaitMargin(pRequestData);

                        if (mrgRepMsg != default)
                        {
                            if ((mrgRepMsg.status == asyncReportStatus.ERROR) || (mrgRepMsg.status == asyncReportStatus.SUCCESS_WITH_ERRORS))
                            {
                                string msg = "Margin report status: " + mrgRepMsg.status.ToString();
                                if (mrgRepMsg.error != default(error))
                                {
                                    msg += $", Error {mrgRepMsg.error.code}: {mrgRepMsg.error.msg}";
                                }
                                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), msg);
                                pMethodComObj.Missing = true;
                                pMethodComObj.IsIncomplete = (mrgRepMsg.status == asyncReportStatus.ERROR);
                            }
                            if ((mrgRepMsg.status == asyncReportStatus.SUCCESS) || (mrgRepMsg.status == asyncReportStatus.SUCCESS_WITH_ERRORS))
                            {
                                if (mrgRepMsg.cycle != default(cycle))
                                {
                                    if (mrgRepMsg.cycle.codeSpecified)
                                    {
                                        pMethodComObj.CycleCode = mrgRepMsg.cycle.code.ToString();
                                    }
                                    if (mrgRepMsg.cycle.dateSpecified)
                                    {
                                        pMethodComObj.DtParameters = mrgRepMsg.cycle.date;
                                    }
                                }
                                if (ArrFunc.IsFilled(mrgRepMsg.margin))
                                {
                                    if (mrgRepMsg.margin.Length == 1)
                                    {
                                        margin mrgRep = mrgRepMsg.margin[0];

                                        // Maj de la date des paramètres de calcul (si encore égale à la date business)
                                        if ((mrgRep.asOfTimeSpecified) && (pMethodComObj.DtParameters == pRequestData.DtBusiness))
                                        {
                                            pMethodComObj.DtParameters = mrgRep.asOfTime;
                                        }

                                        if (ArrFunc.IsFilled(mrgRep.payload))
                                        {
                                            riskPayload payload = mrgRep.payload[0];
                                            if (mrgRep.payload.Count() > 1)
                                            {
                                                // Dans le cas où il y a plusieurs PayLoad, prendre celui du Framework NEXT (correspondant à SPAN 2)
                                                riskPayload payloadNext = mrgRep.payload.Where(p => p.riskFrameworkSpecified && p.riskFramework == riskFramework.NEXT).FirstOrDefault();
                                                if (payloadNext != default(riskPayload))
                                                {
                                                    payload = payloadNext;
                                                }
                                            }
                                            if (payload != default(riskPayload))
                                            {
                                                // Obtention du margin détail par deserialization Json de la valeur (value) du Payload
                                                marginDetail = GetRiskMarginDetail(pRequestData, payload.Value);

                                                // Réponse Json reçue
                                                pRequestData.JsonMarginDetailResponseMessage = payload.Value;
                                                pRequestData.MarginDetailResponseMessage = marginDetail;

                                                // Résultat disponible
                                                isMarginAvailable = true;
                                            }
                                            else
                                            {
                                                // PM 20230830 [XXXXX] Ajout SysMsgCode
                                                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Payload of margin detail message is empty");
                                                pMethodComObj.Missing = true;
                                                pMethodComObj.IsIncomplete = true;
                                            }
                                        }
                                        else
                                        {
                                            // PM 20230830 [XXXXX] Ajout SysMsgCode
                                            pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "There is no payload of margin detail message");
                                            pMethodComObj.Missing = true;
                                            pMethodComObj.IsIncomplete = true;
                                        }
                                    }
                                    else
                                    {
                                        // PM 20230830 [XXXXX] Ajout SysMsgCode
                                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Received more than one margin detail message");
                                        pMethodComObj.Missing = true;
                                        pMethodComObj.IsIncomplete = true;
                                    }
                                }
                                else
                                {
                                    // PM 20230830 [XXXXX] Ajout SysMsgCode
                                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "No margin detail message received");
                                    pMethodComObj.Missing = true;
                                    pMethodComObj.IsIncomplete = true;
                                }
                            }
                        }
                        else
                        {
                            // PM 20230830 [XXXXX] Ajout SysMsgCode
                            pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "No margin report message received from waiting margin detail");
                            pMethodComObj.Missing = true;
                            pMethodComObj.IsIncomplete = true;
                        }
                    }
                    else
                    {
                        // PM 20230830 [XXXXX] Ajout SysMsgCode
                        pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Received margin message is empty");
                        pMethodComObj.Missing = true;
                        pMethodComObj.IsIncomplete = true;
                    }
                }
                else
                {
                    // PM 20230830 [XXXXX] Ajout SysMsgCode
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Received more than one margin message");
                    pMethodComObj.Missing = true;
                    pMethodComObj.IsIncomplete = true;
                }
            }
            else
            {
                // PM 20230830 [XXXXX] Ajout SysMsgCode
                pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "No margin message received");
                pMethodComObj.Missing = true;
                pMethodComObj.IsIncomplete = true;
            }
            return isMarginAvailable;
        }

        /// <summary>
        /// Indique si l'envoie de la position pour calcul ne retourne pas d'erreur
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        public bool IsCalcMarginPossible(SPAN2MarginData pRequestData)
        {
            bool isOk = false;
            if (pRequestData != default(SPAN2MarginData))
            {
                // Génération et envoie de la requete de calcul
                marginReportMessage mrgCompleteRep = MarginCompleteJson(pRequestData, false);

                if (mrgCompleteRep != default)
                {
                    // Vérification du status
                    if ((mrgCompleteRep.error == default(error)) && (mrgCompleteRep.status != asyncReportStatus.ERROR) && (mrgCompleteRep.margin != default(margin[])))
                    {
                        isOk = true;
                    }
                    if ((mrgCompleteRep.margin != default(margin[])) && (mrgCompleteRep.margin.Length > 0))
                    {
                        // Suppression du portefuille de la demande de calcul
                        margin mrg = mrgCompleteRep.margin.FirstOrDefault();
                        if (mrg != default(margin))
                        {
                            pRequestData.ApiMarginId = mrg.id;
                            pRequestData.ApiPortfolioId = mrg.portfolioId;

                            DeletePortfolio(pRequestData);
                        }
                    }
                }
            }
            return isOk;
        }

        #region Test methods (TODO)
        /// <summary>
        /// Requête permettant d'ajouter des trades au format FixML à un portefeuille
        /// </summary>
        /// <param name="pRequestData">Données nécessaires à la création des requêtes</param>
        /// <returns></returns>
        /// TODO
        public transactionReportMessage TestAddTransactionsFixML(SPAN2MarginData pRequestData)
        {
            transactionReportMessage transactionRptMsg = default;

            if (pRequestData != default(SPAN2MarginData))
            {
                _ = RiskMethodSpan2TestTools.TestFixmlSerialized(pRequestData.ActorId, pRequestData.BookId, pRequestData.MarginPosition);

                string fixml =
                @"<![CDATA[<FIXML xmlns = ""www.cmegroup.com/fixml50/1"" v=""5.0 SP2"" xv=""109"" cv=""CME.0001"" s=""20090815"">
                         <TrdCaptRpt LastQty = ""4"" LastPx = ""3.75"">
                         <Instrmt ID=""CL"" Src=""H"" SecTyp=""FUT"" MMY=""202206""/><RptSide Side=""2"" InptDev=""API"">
                         <Pty ID=""118"" R=""4""/>
                         <Pty ID=""14"" R=""24""/>
                         </RptSide>
                         </TrdCaptRpt>
                         </FIXML>]]>";
                fixml = @"<![CDATA[
                <FIXML>
                    <TrdCaptRpt LastQty = ""4"">
                        <Instrmt Exch = ""NYMEX"" SecTyp = ""FUT"" ID = ""CL"" MMY = ""202206""/>
                            <RptSide Side = ""1"">
                                <Pty R = ""4"" ID = ""118"" />
                                <Pty R = ""24"" ID = ""14"" />
                            </RptSide>
                    </TrdCaptRpt>
                </FIXML>]]>";

                // Alimentation des données nécessaire à la génération du message XML de la requête
                transactionPayload transPayload = new transactionPayload
                {
                    format = transactionFormat.FIXML,
                    encoding = transactionEncoding.STRING,
                    encodingSpecified = true,
                    Item = fixml
                };
                //
                transaction trans = new transaction
                {
                    portfolioId = "0",
                    type = transactionType.TRADE,
                    typeSpecified = true,
                    payload = transPayload
                };
                //
                transactionRequestMessage transReqMsg = new transactionRequestMessage
                {
                    transaction = new transaction[1] { trans }
                };
                
                try
                {
                    // Génération du message XML de la requête
                    StringBuilder sb = new StringBuilder();
                    StringWriterWithEncoding writer = new StringWriterWithEncoding(sb, Encoding.UTF8);
                    m_XsTransactionReqMsg.Serialize(writer, transReqMsg, m_CmeNamespaces);

                    string xmlMsg = sb.ToString();
                    xmlMsg = xmlMsg.Replace("&lt;", "<");
                    xmlMsg = xmlMsg.Replace("&gt;", ">");

                    HttpContent content = new StringContent(xmlMsg, Encoding.UTF8, "application/xml");

                    RiskMethodSpan2Helper.WaitPostTransaction();
                    HttpResponseMessage response = HttpPost(m_UriServiceTransactions, pRequestData, content);
                    if (response != default)
                    {
                        HttpContent repContent = response.Content;
                        if (repContent != default(HttpContent))
                        {
                            Task<string> repString = repContent.ReadAsStringAsync();
                            repString.Wait();

                            string repMsg = repString.Result;

                            StringReader reader = new StringReader(repMsg);
                            transactionRptMsg = (transactionReportMessage)m_XsTransactionRptMsg.Deserialize(reader);
                        }
                    }
                }
                catch (Exception e)
                {
                    pRequestData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "AddTransactionsFixML: " + e.Message);
                }
            }
            return transactionRptMsg;
        }
        #endregion Test methods (TODO)
        #endregion Methods
    }
}
