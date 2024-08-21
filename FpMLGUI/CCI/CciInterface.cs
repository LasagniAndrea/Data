using System;

using EFS.Common.Web; 
using EFS.GUI;


namespace EFS.GUI.CCI
{
    public interface ITradeCci
    {
        #region GetMainCurrency
        string GetMainCurrency { get; }
        #endregion
        #region CciClientIdMainCurrency
        string CciClientIdMainCurrency { get; }
        #endregion
        #region RetSidePayer
        string RetSidePayer { get; }
        #endregion
        #region RetSideReceiver
        string RetSideReceiver { get; }
        #endregion
    }

    public interface IContainerCciPayerReceiver
    {
        #region CciClientIdPayer
        string CciClientIdPayer { get; }
        #endregion
        #region CciClientIdReceiver
        string CciClientIdReceiver { get; }
        #endregion
        #region SynchronizePayerReceiver
        /// <summary>
        /// Synchronisation des payers/receivers par rapport aux parties
        /// </summary>
        void SynchronizePayerReceiver(string pLastValue, string pNewValue);
        #endregion SynchronizePayerReceiver
    }

    public interface IContainerCciGetInfoButton
    {
        #region SetButtonZoom
        /// <summary>
        /// Retourne true s'il existe un bouton ZOOM associé au cci 
        /// <para>Si true alimente pCo afin d'ouvrir une feuille ZOOM de type saisie full, alimente pIsObjSpecified , alimente pIsEnabled  </para>
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsObjSpecified, ref bool pIsEnabled);
        #endregion SetButtonZoom
        //		
        #region SetButtonScreenBox
        /// <summary>
        /// Retourne true s'il existe un bouton ScreenBox associé au cci 
        /// <para>Si true alimente pCo afin d'ouvrir la feuille ZOOM de type saisie light, alimente pIsObjSpecified , alimente pIsEnabled  </para>
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled);
        #endregion SetButtonScreenBox
        //
        #region SetButtonReferential
        /// <summary>
        /// Permet la mise à jour du CustomObjectButtonReferential associé au cci, le CustomObjectButtonReferential est initialisé par le descriptif de l'écran
        /// <para>CustomObjectButtonReferential gère l'ouverture d'un référentiel/consultation</para>
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pDA"></param>
        /// <returns></returns>
        void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo);
        #endregion SetButtonReferential
    }
    
    /// <summary>
    ///  Retourne true lorsque le container de ccis est renseigné
    ///  <para>Généralement un container de ccis est renseigné si une donnée majeure est renseignée (ex le payer)</para>
    /// </summary>
    public interface IContainerCciSpecified
    {
        bool IsSpecified { get; }
    }

    /// <summary>
    /// Gestion des ccis associés aux éléments de type QuoteBasisEnum et StrikeQuoteBasisEnum 
    ///<para>QuoteBasisEnum est géré via 3 ccis, 1 pour la devise1, 1 pour la devise2, 1 pour l'enum QuoteBasisEnum</para>
    ///<para>StrikeQuoteBasisEnum est géré via 3 ccis, 1 pour la devise call, 1 pour la put, 1 pour l'enum StrikeQuoteBasisEnum</para>
    /// </summary>
    public interface IContainerCciQuoteBasis
    {
        #region  IsClientId_QuoteBasis
        /// <summary>
        /// Retourne true si pCci gère un élément de type QuoteBasisEnum ou StrikeQuoteBasisEnum
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        bool IsClientId_QuoteBasis(CustomCaptureInfo pCci);
        #endregion
        #region GetCurrency1
        /// <summary>
        /// Retourne la property cci.NewValue du cci rattaché à la devise1 du quotedCurrencyPair
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        string GetCurrency1(CustomCaptureInfo pCci);
        #endregion
        #region GetCurrency2
        /// <summary>
        /// Retourne la property cci.NewValue du cci rattaché à la devise2 du quotedCurrencyPair
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        string GetCurrency2(CustomCaptureInfo pCci);
        #endregion
        #region GetBaseCurrency
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        //201001 FI je ne sais pas à koi ça sert ??
        string GetBaseCurrency(CustomCaptureInfo pCci);
        #endregion
    }

}
