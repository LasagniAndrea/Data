using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization; 


using EFS.ACommon;
using EFS.Common;
using EFS.Actor;

using EFS.ApplicationBlocks.Data;
using EFS.Common.MQueue;


namespace EFS.Common.Log
{
    /// <summary>
    /// Définie les méthodes nécessaires aux objets chargés d'alimenter le journal des actions utilisateurs
    /// </summary>
    /// FI 20140519 [19923] add interface
    public interface IRequestTrackBuilder
    {
        /// <summary>
        ///  Génération du document destiné à alimenter le journal des actions utilisateurs
        /// </summary>
        void BuildRequestTrack();
        /// <summary>
        /// Envoi du document au journal des actions utilisateurs
        /// </summary>
        void SendRequestTrack();
    }

    /// <summary>
    /// Classe chargée d'alimenter le journal des actions utilisateurs dans un thread indépendant
    /// </summary>
    /// FI 20140519 [19923] add class 
    public class ThreadRequestTrack : ThreadTaskBase
    {
        /// <summary>
        /// Objet chargé d'alimenter le journal des actions utilisateurs
        /// </summary>
        public IRequestTrackBuilder trackBuilder;

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public ThreadRequestTrack() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRequestTrack"></param>
        public ThreadRequestTrack(IRequestTrackBuilder pRequestTrack)
        {
            trackBuilder = pRequestTrack;
        }
        #endregion

        /// <summary>
        /// Alimentation et envoi d'un flux destiné à l'alimentation du journal des actions utilisateurs 
        /// </summary>
        public override void ExecuteTask()
        {
            trackBuilder.BuildRequestTrack();
            trackBuilder.SendRequestTrack();
        }
    }
}