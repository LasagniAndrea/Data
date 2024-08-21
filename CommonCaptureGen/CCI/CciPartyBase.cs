#region Using Directives
using EFS.ACommon;
using EFS.GUI.CCI;
using FpML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20200504 [XXXXX] CciPartyBase hérite de ContainerCciBase
    public class CciPartyBase : ContainerCciBase
    {
        /// <summary>
        /// Le trade
        /// </summary>
        public CciTradeCommonBase cciTrade;

        /// <summary>
        /// Numéro de la party (index commence à 1)
        /// </summary>
        public int number;


        /// <summary>
        /// Obtiens la collection ccis 
        /// </summary>
        public TradeCommonCustomCaptureInfos Ccis
        {
            get { return cciTrade.Ccis; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pNumber"></param>
        /// <param name="pPrefix"></param>
        /// FI 20200504 [XXXXX] CciPartyBase hérite de ContainerCciBase
        public CciPartyBase(CciTradeCommonBase pTrade, int pNumber, string pPrefix) :
            base(pPrefix, pNumber, pTrade.Ccis)
        {
            cciTrade = pTrade;
            number = pNumber;
        }

        /// <summary>
        /// Retourne l'attribut id attribué à l'élément Party
        /// <para>
        /// Retourne string.Empty si la donnée du cci est non valide
        /// </para>
        /// </summary>
        /// <returns></returns>
        public virtual string GetPartyId()
        {
            return GetPartyId(false);
        }

        /// <summary>
        /// Retourne l'attribut id de la partie
        /// </summary>
        /// <param name="pIsGetNewValueWhenDataNoValid">si true retourne un Id lorsque la donnée renseignée est non valide</param>
        /// <returns></returns>
        public virtual string GetPartyId(bool pIsGetNewValueWhenDataNoValid)
        {
            throw new Exception("To override");
        }

        /// <summary>
        /// Retourne le PartyTradeIdentifier associé à la partie
        /// </summary>
        /// <returns></returns>
        public IPartyTradeIdentifier GetPartyTradeIdentifier()
        {
            return cciTrade.DataDocument.GetPartyTradeIdentifier(GetPartyId());
        }
        /// <summary>
        /// Retourne le PartyTradeInformation associé à la partie
        /// </summary>
        /// <returns></returns>
        public IPartyTradeInformation GetPartyTradeInformation()
        {
            return cciTrade.DataDocument.GetPartyTradeInformation(GetPartyId());
        }

        /// <summary>
        /// Retourne le Party associé à la partie
        /// </summary>
        /// <returns></returns>
        /// FI 20170928 [23452] Add
        public IParty GetParty()
        {
            return cciTrade.DataDocument.GetParty(GetPartyId());
        }



        /// <summary>
        /// Retoune lde PartyTradeIdentifier associé à la party
        /// <para>Ajoute un PartyTradeIdentifier s'il en existe pas</para>
        /// </summary>
        /// <returns></returns>
        protected IPartyTradeIdentifier PartyTradeIdentifier()
        {
            return PartyTradeIdentifier(GetPartyId());
        }
        /// <summary>
        /// Retoune la PartyTradeIdentifier pour référence {pPartyId}
        /// <para>Ajoute un PartyTradeIdentifier s'il en existe pas</para>
        /// </summary>
        /// <returns></returns>
        /// <param name="pPartyId"></param>
        /// <returns></returns>
        protected IPartyTradeIdentifier PartyTradeIdentifier(string pPartyId)
        {
            IPartyTradeIdentifier ret = null;
            if (StrFunc.IsFilled(pPartyId))
                ret = cciTrade.DataDocument.AddPartyTradeIndentifier(pPartyId, true);

            return ret;
        }
        /// <summary>
        /// Retoune le PartyTradeInformation associé à la party lorsque la party est valide
        /// <para>Ajoute un PartyTradeInformation s'il en existe pas</para>
        /// </summary>
        /// <returns></returns>
        protected IPartyTradeInformation PartyTradeInformation()
        {
            return PartyTradeInformation(GetPartyId());
        }
        /// <summary>
        /// Retoune le PartyTradeInformation pour référence {pPartyId}
        /// <para>Ajoute un PartyTradeInformation s'il en existe pas</para>
        /// </summary>
        /// <returns></returns>
        /// EG 20171016 [23509] Upd 
        protected IPartyTradeInformation PartyTradeInformation(string pPartyId)
        {
            IPartyTradeInformation ret = null;
            if (StrFunc.IsFilled(pPartyId))
            {
                ret = cciTrade.DataDocument.GetPartyTradeInformation(pPartyId);
                if (null == ret)
                    ret = cciTrade.DataDocument.AddPartyTradeInformation(pPartyId);
            }
            return ret;
        }

        /// <summary>
        ///  purge de partyTradeIdentifier && partyTradeInformation
        ///  <para>Suppression dans la foulée du partyTradeInformation s'il est vide</para>
        /// </summary>
        // EG 20171122 IsDocumentElementValid
        public virtual void CleanUp()
        {
            IPartyTradeInformation partyTradeInformation = GetPartyTradeInformation();
            if (null != partyTradeInformation)
            {
                cciTrade.DataDocument.CleanUpPartyTradeInformation(partyTradeInformation);

                //Suppression de partyTradeInformation s'il est vide 
                if (false ==CaptureTools.IsDocumentElementValid(partyTradeInformation))
                {
                    int posInArray = cciTrade.DataDocument.GetPartyTradeInformationPosInArray(GetPartyId());
                    if (posInArray > -1)
                        ReflectionTools.RemoveItemInArray(cciTrade.DataDocument.TradeHeader, "partyTradeInformation", posInArray);
                }
            }

            IPartyTradeIdentifier partyTradeIdentifier = GetPartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {
                cciTrade.DataDocument.CleanUpPartyTradeIdentifier(partyTradeIdentifier);
                //Rq pas de suppression d'un éventuel partyTradeIdentifier
            }
        }
    }
}
