using System;

namespace EFS.Controls
{
    #region DataGridLoadErrorEventHandler
    /// <summary>
    /// Représente l'évènement généré lorsqu'une erreur se produit durant le chargement du jeu de données
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void LoadDataErrorEventHandler(object sender, LoadDataErrorEventArgs e);
    #endregion

    #region LoadDataErrorEventArgs
    /// <summary>
    /// Représente les arguments associées à l'évènement LoadDataErrorEventHandler
    /// </summary>
    public class LoadDataErrorEventArgs : EventArgs
    {
        #region Membres
        #region errorType
        /// <summary>
        /// Représente un code erreur
        /// </summary>
        public string errorCode;
        #endregion
        #region message
        /// <summary>
        /// Représente le Message d'erreur
        /// </summary>
        public string message;
        #endregion
        #endregion
        //
        #region constructor
        public LoadDataErrorEventArgs()
        {
        }
        public LoadDataErrorEventArgs(string pErrorCode, string pMessage)
        {
            errorCode = pErrorCode;
            message = pMessage;
        }
        #endregion constructor
        //
        #region Methodes
        #region GetEventArgument
        public string GetEventArgument()
        {
            return errorCode + "{-}" + message;
        }
        #endregion
        #endregion Methodes
    }
    #endregion
}