using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Classe d'accés au HttpClient
    /// </summary>
    public class HttpClientHelper
    {
        #region members
        /// <summary>
        ///  Client Http
        /// </summary>
        private static readonly HttpClient m_HttpClient = new HttpClient();

        /// <summary>
        /// Semaphore pour l'accés au Client Http
        /// </summary>
        private static readonly SemaphoreSlim m_SemaphoreHttpLock = new SemaphoreSlim(1, 1);
        #endregion Members

        #region Accessors
        /// <summary>
        /// Client Http
        /// </summary>
        public HttpClient HttpClient
        {
            get { return m_HttpClient; }
        }

        /// <summary>
        /// Semaphore à utiliser pour l'accés au Client Http
        /// </summary>
        public SemaphoreSlim HttpClientLock
        {
            get { return m_SemaphoreHttpLock; }
        }
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Envoie une requête GET vers l'URI spécifié sous forme d'opération asynchrone
        /// </summary>
        /// <param name="pUri">URI auquel la requête est envoyée</param>
        /// <returns>Objet de tâche représentant l'opération asynchrone</returns>
        public async Task<HttpResponseMessage> GetAsync(Uri pUri)
        {
            return await m_HttpClient.GetAsync(pUri);
        }

        /// <summary>
        /// Envoie une requête POST vers l'URI spécifié sous forme d'opération asynchrone
        /// </summary>
        /// <param name="pUri">URI auquel la requête est envoyée</param>
        /// <param name="pContent">Contenu HTTP à envoyer</param>
        /// <returns>Objet de tâche représentant l'opération asynchrone</returns>
        public async Task<HttpResponseMessage> PostAsync(Uri pUri, HttpContent pContent)
        {
            return await m_HttpClient.PostAsync(pUri, pContent);
        }

        /// <summary>
        /// Envoie une requête DELETE vers l'URI spécifié sous forme d'opération asynchrone
        /// </summary>
        /// <param name="pUri">URI auquel la requête est envoyée</param>
        /// <returns>Objet de tâche représentant l'opération asynchrone</returns>
        // PM 20230831 [XXXXX] New
        public async Task<HttpResponseMessage> DeleteAsync(Uri pUri)
        {
            return await m_HttpClient.DeleteAsync(pUri);
        }

        /// <summary>
        /// Envoie une requête PUT vers l'URI spécifié sous forme d'opération asynchrone
        /// </summary>
        /// <param name="pUri">URI auquel la requête est envoyée</param>
        /// <param name="pContent">Contenu HTTP à envoyer</param>
        /// <returns>Objet de tâche représentant l'opération asynchrone</returns>
        // PM 20230831 [XXXXX] New
        public async Task<HttpResponseMessage> PutAsync(Uri pUri, HttpContent pContent)
        {
            return await m_HttpClient.PutAsync(pUri, pContent);
        }
        #endregion Methods
    }
}
