using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EFS.LoggerService
{
    public class LoggerTimer
    {
        #region Members
        private Timer m_WriteTimer;
        private readonly TimerCallback m_WriteCallBack;
        private readonly SemaphoreSlim m_TimerLock = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Nombre de milliseconde entre 2 écritures périodiques
        /// </summary>
        private readonly int m_TimerPeriod = 10000;
        private readonly int m_TimerDueTime = 10000;
        #endregion Members

        #region Constructors
        public LoggerTimer(TimerCallback pTimerCallback, int pDueTime, int pPeriod)
        {
            m_WriteCallBack = pTimerCallback;
            m_TimerDueTime = pDueTime;
            m_TimerPeriod = pPeriod;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Démarage du Timer
        /// </summary>
        public void StartTimer()
        {
            if (m_WriteTimer == default(Timer))
            {
                m_TimerLock.Wait();
                try
                {
                    m_WriteTimer = new Timer(m_WriteCallBack, null, m_TimerDueTime, m_TimerPeriod);
                }
                catch (Exception e)
                {
                    LoggerServiceAppTool.TraceManager.TraceError(e.Source, e.Message);
                    throw e;
                }
                finally
                {
                    m_TimerLock.Release();
                }
            }
        }

        /// <summary>
        /// Arrêt du Timer
        /// </summary>
        public void StopTimer()
        {
            m_TimerLock.Wait();
            try
            {
                if (m_WriteTimer != default(Timer))
                {
                    m_WriteTimer.Dispose();
                    m_WriteTimer = default;
                }
            }
            catch (Exception e)
            {
                LoggerServiceAppTool.TraceManager.TraceError(e.Source, e.Message);
                throw e;
            }
            finally
            {
                m_TimerLock.Release();
            }

        }
        #endregion Methods
    }
}
