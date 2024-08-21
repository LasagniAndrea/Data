using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.SpheresService
{
    
    /// <summary>
    /// Classe 
    /// </summary>
    public class MQueueCount
    {
        #region Members
        private int m_CountQueueHigh;
        private int m_CountQueueNormal;
        private int m_CountQueueLow;
        #endregion Members

        #region Accessors
        public int CountQueueHigh
        {
            get { return m_CountQueueHigh; }
            set { m_CountQueueHigh = value; }
        }
        public int CountQueueNormal
        {
            get { return m_CountQueueNormal; }
            set { m_CountQueueNormal = value; }
        }
        public int CountQueueLow
        {
            get { return m_CountQueueLow; }
            set { m_CountQueueLow = value; }
        }
        #endregion Accessors

        #region Constuctors
        /// <summary>
        /// Constructeur
        /// </summary>
        public MQueueCount()
        {
            m_CountQueueHigh = 0;
            m_CountQueueNormal = 0;
            m_CountQueueLow = 0;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pCountQueueHigh"></param>
        /// <param name="pCountQueueNormal"></param>
        /// <param name="pCountQueueLow"></param>
        public MQueueCount(int pCountQueueHigh, int pCountQueueNormal, int pCountQueueLow)
        {
            m_CountQueueHigh = pCountQueueHigh;
            m_CountQueueNormal = pCountQueueNormal;
            m_CountQueueLow = pCountQueueLow;
        }
        #endregion Constuctors
    }
    
}
