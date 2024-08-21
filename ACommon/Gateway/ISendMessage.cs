using System;

namespace EFS.Gateway
{
    /// <summary>
    /// Fournit l'interface de base pour envoyer un message reçu par une gateway vers SpheresIO®
    /// </summary>
    public interface ISendIOMessage
    {
        /// <summary>
        /// Envoie une message vers Spheres® IO
        /// </summary>
        /// <param name="pMessage">Représente le message</param>
        /// <param name="pMessageClass">Represente le type de message</param>
        SendIOResult Send(string pMessage, string pMessageClass);

        /// <summary>
        /// Envoie un message vers Spheres® IO avec indication d'une tâche spécifique
        /// </summary>
        /// <param name="pMessage">Représente la donnée</param>
        /// <param name="pIoTaskIdentifier">Represente la tâche IO</param>
        SendIOResult SendTask(string pMessage, string pIoTaskIdentifier);
    }


    /// <summary>
    /// Représente les retours possibles pour l'envoi des messages reçu par une gateway vers SpheresIO®
    /// </summary>
    /// FI 20131003 [] Add enum SendIOResult
    public enum SendIOResult
    {
        /// <summary>
        ///  Envoi du message vers SpheresIO en succès
        /// </summary>
        Ok,
        /// <summary>
        ///  Envoi du message vers SpheresIO en erreur
        /// </summary>
        NOk,
        /// <summary>
        /// Aucune tâche dans le fichier de configuration pour le message reçu
        /// </summary>
        NoTask
    }

}
