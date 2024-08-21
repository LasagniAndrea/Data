using System;
using System.Diagnostics;
using System.ServiceModel;
//
using EFS.ACommon;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
//
namespace EFS.LoggerClient
{
    /// <summary>
    /// Gestion du Client du service de Log
    /// </summary>
    /// FI 20240111 [WI793] Add
    public class LoggerManager
    {
        /// <summary>
        /// Gestionnaire de la trace 
        /// </summary>
        internal static SpheresTraceManager TraceManager { get; private set; }

        /// <summary>
        /// Application qui utilise le client
        /// </summary>
        internal static (string Name, string Version, string FullName, int Ida) AppInstance { get; private set; }

        /// <summary>
        /// Processus actif qui utilise le client
        /// </summary>
        internal static (string Name, int Id) CurrentProcess { get; private set; }


        private static bool m_IsInitialized = false;
        /// <summary>
        /// Indicateur d'initialisation du LoggerClient
        /// </summary>
        public static bool IsInitialized { get => m_IsInitialized; }


        private static bool m_IsEnabled = false;
        /// <summary>
        /// Indique si le Client du service de Log est actif
        /// </summary>
        public static bool IsEnabled { get => m_IsEnabled; }

        /// <summary>
        /// Client du service de Log
        /// </summary>
        public static SpheresLoggerServiceClient LoggerClient { get; private set; }

        #region Methods
        /// <summary>
        /// Active le Logger
        /// </summary>
        /// <returns></returns>
        public static void Enable()
        {
            m_IsEnabled = true;
        }

        /// <summary>
        /// Désactive le Logger
        /// </summary>
        /// <returns></returns>
        public static void Disable()
        {
            m_IsEnabled = false;
            m_IsInitialized = false;
            if (LoggerClient != default(SpheresLoggerServiceClient))
            {
                CloseLoggerClient();
                LoggerClient = default;
            }
        }

        /// <summary>
        /// Initialise le client du service de log. Lorsque le service n'est pas vivant la propriété <see cref="IsInitialized"/> retourne false.
        /// </summary>
        /// <param name="pInstance"></param>
        /// <param name="pTraceManager"></param>
        public static void Initialize((string Name, string Version, string FullName, int Ida) pInstance, SpheresTraceManager pTraceManager)
        {
            if (m_IsEnabled)
            {
                try
                {
                    TraceManager = pTraceManager;
                    if (TraceManager == default(SpheresTraceManager))
                    {
                        TraceManager = new SpheresTraceManager(pInstance.Name);
                        TraceManager.NewTrace();
                    }

                    AppInstance = pInstance;

                    Process currentProcess = Process.GetCurrentProcess();
                    CurrentProcess = (currentProcess.ProcessName, currentProcess.Id);

                    LoggerClient = new SpheresLoggerServiceClient();
                    TraceManager.TraceInformation(ErrorLogTools.GetMethodName(), $"Logger client state: {LoggerClient.State}");

                    // PM 20240612 [WI542] Ajout log de l'adresse de connexion
                    Uri loggerEndpoint = LoggerClient.Endpoint.Address.Uri;
                    TraceManager.TraceInformation(ErrorLogTools.GetMethodName(), $"Logger endpoint address: {loggerEndpoint.OriginalString}");

                    m_IsInitialized = LoggerClient.IsAlive();

                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e), "Unable to initialize Spheres Logger Client");
                    CloseLoggerClient();
                }
            }
        }

        /// <summary>
        // Fermeture de LoggerClient
        /// </summary>
        private static void CloseLoggerClient()
        {
            if ((LoggerClient != default(SpheresLoggerServiceClient)) && (LoggerClient.State == CommunicationState.Opened))
            {
                LoggerClient.Close();
            }
            LoggerClient = null;
        }

        /// <summary>
        /// Ecriture des erreurs dans fichier trace et observateur d'événement. Fermeture de <see cref="LoggerClient"/>
        /// </summary>
        /// <param name="pSpheresException"></param>
        /// <param name="pData"></param>
        internal static void WriteEventLogSystemError(SpheresException2 pSpheresException, params string[] pData)
        {
            // Ecriture dans la trace
            if (LoggerClient != default(SpheresLoggerServiceClient))
            {
                TraceManager.TraceInformation(ErrorLogTools.GetMethodName(), string.Format("Logger client state: {0}", LoggerClient.State));
            }
            TraceManager.TraceError(pSpheresException.Method, ExceptionTools.GetMessageAndStackExtended(pSpheresException));

            // Ecriture dans le journal des évènements
            EventLogTools.WriteEventLogSystemError(AppInstance.FullName, pSpheresException, pData);
        }
        #endregion
    }

    
}
