#region using directives
using System;
#endregion using directives

namespace EFS.Controls
{
    #region GridViewDataErrorEventHandler
    /// <summary>
    /// Représente l'évènement généré lorsqu'une erreur se produit durant le chargement du jeu de données
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void GridViewDataErrorEventHandler(object sender, GridViewDataErrorEventArgs e);
    #endregion

    #region LoadDataErrorEventArgs
    /// <summary>
    /// Représente les arguments associées à l'évènement GridViewDataErrorEventHandler
    /// </summary>
    public class GridViewDataErrorEventArgs : EventArgs
    {
        #region Membres
        #region errorCode
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
        public GridViewDataErrorEventArgs()
        {
        }
        public GridViewDataErrorEventArgs(string pErrorCode, string pMessage)
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