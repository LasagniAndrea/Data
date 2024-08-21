#region Using Directives
using System;
using System.Collections;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Xml.Serialization;
using System.Drawing;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.Enum.Tools;
using EfsML.Enum;



using FpML.Enum;
#endregion Using Directives

namespace EFS.GUI.CCI
{
    /// <summary>
    /// Interface que doivent respect�es les objets qui peutvent �tre g�r�s via des ccis 
    /// </summary>
    public interface ICustomCaptureInfos
    {
        /// <summary>
        /// Obtient la collection de cci
        /// </summary>
        CustomCaptureInfosBase CcisBase { get; }
    }
    
    /// <summary>
    /// Interface to implemented for class Trade[product] (ie: TradeFra)
    /// Warning: This methods are "virtual method" of the class "TradeBase"
    /// </summary>
    public interface IContainerCciFactory
    {

        /// <summary>
        /// Instanciations des objets du dataDocument en fonction de la pr�sence des CCIs
        /// <para>Exemple s'il existe des ccis appartenent � un 3�me otherPartyPayments, on g�n�re ici un 3�me otherPartyPayments dans le dataDocument</para>
        /// </summary>
        void Initialize_FromCci();


        /// <summary>
        /// Ajout des ccis dits "Systems" car syst�matiquement n�cessaires 
        /// (ie: Buyer et Seller du fra)
        /// <remarks>Par d�faut, seuls sont inject�s les ccis qui sont d�clar�s sur les fichiers descriptifs de l'�cran</remarks>
        /// </summary>
        void AddCciSystem();


        /// <summary>
        /// Initialisation des ccis � partir des donn�es pr�sentes dans le dataDocument
        /// </summary>
        void Initialize_FromDocument();

        /// <summary>
        /// Alimentation du dataDocument avec les donn�es pr�sentes dans les ccis
        /// </summary>
        void Dump_ToDocument();


        /// <summary>
        /// Pr�proposition de ccis en fonction du cci en entr�e
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        void ProcessInitialize(CustomCaptureInfo pCci);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        void ProcessExecute(CustomCaptureInfo pCci);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci);


        /// <summary>
        /// retourne true si le cci ne peut �tre aliment� qu'avec les contreparties
        /// </summary>
        bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci);


        /// <summary>
        /// CleanUp => Procedure appel�e afin de nettoyer le document Fpml avant validation ou avant chgt d'�cran 
        /// Ex OtherPartyPayment 
        /// s'il existe 3 OtherPartyPayment sur le screen objects => IL y aura 3 instances de Trade.otherpartyPayment
        /// Les instances non aliment�es seront supprim�es pour conformit�  vis � vis de l'xsd fpml
        /// </summary>
        void CleanUp();


        /// <summary>
        /// Affecte la propri�ti� Enabled des ccis
        /// </summary>
        void RefreshCciEnabled();


        /// <summary>
        /// Affecte la propri�ti� Display d'un cci
        /// </summary>
        void SetDisplay(CustomCaptureInfo pCci);


        /// <summary>
        ///  Initialisation du document pour que la saisie light soit operationnelle
        ///  Exemple En Cr�ation, il faut absolument que les payers receivers qui font r�f�rence aux COUNTERPARTY ne soit pas identiques
        ///  sinon Pb de synchronisation des payers/receivers
        /// </summary>
        void Initialize_Document();

    }
    
    /// <summary>
    /// 
    /// </summary>
    public interface IContainerCci
    {
        
        /// <summary>
        /// retourne le nom complet du cci (sans le prefix du control)  s'il appartient � la classe trade en cours ( Ex TradeFra ou TradeFxLeg......)
        /// Ex Fra => TradeFra.CciClientId( "toto") => Retourne le cci tel que ClientId_WithoutPrefix = fra_toto  
        /// </summary>
        string CciClientId(string pClientId_Key);
       
        /// <summary>
        /// retourne le cci s'il appartient � la classe trade en cours
        /// Ex Fra => TradeFra.CciTradeIdentifier("Toto") => Retourne le ccis[fra_toto] 
        /// </summary>
        CustomCaptureInfo Cci(string pClientId_Key);
      
        /// <summary>
        /// retourne si le cci accessible  depuis l'instance trade en cours
        /// parametre = nom complet (sans le prefix control)
        /// </summary>
        bool IsCciOfContainer(string pClientId_WithoutPrefix);
       
        /// <summary>
        /// retourne l'�lement � partir du nom complet (sans le prefix control)
        /// Ex Fra => TradeFra.CciContainerKey("fra_Toto") => Retourne "Toto"
        /// </summary>
        string CciContainerKey(string pClientId_WithoutPrefix);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IContainerArray
    {

        void RemoveLastItemInArray(string pPrefix);
    }

    /// <summary>
    /// Interface dedicated to CCiControl with special needs at the presentation level
    /// </summary>
    public interface ICciPresentation
    {
        void DumpSpecific_ToGUI(CciPageBase pPage);
    }

    
    
    /// <summary>
    ///  Classe permettant d'attribuer la priortit� � un des CCI d'une liste (array) de CCI
    /// </summary>
    public class CciCompare : IComparable
    {
        #region Members
        /// <summary>
        /// Nom de bapt�me du cci  
        /// </summary>
        public string key;
        /// <summary>
        /// aliment� avec cci.NewValue  
        /// </summary>
        private readonly string sValue;
        /// <summary>
        /// aliment� avec cci.IsInputByUser   
        /// </summary>
        private readonly bool isInputByUser;
        /// <summary>
        /// niveau de priorit�, le cciCompare de niveau le plus faible est prioritaire
        /// </summary>
        private readonly int order;
        /// <summary>
        /// aliment� avec cci.DataType   
        /// </summary>
        private readonly TypeData.TypeDataEnum dataType;
        #endregion Members
        #region accessor
        public bool IsFilledValue
        {
            get
            {
                bool ret;
                try
                {
                    //
                    if (TypeData.IsTypeDec(dataType))
                        ret = StrFunc.IsDecimalInvariantFilled(sValue);
                    else if (TypeData.IsTypeInt(dataType))
                        ret = StrFunc.IsIntegerInvariantFilled(sValue);
                    else if (TypeData.IsTypeDate(sValue))
                        ret = DtFunc.IsDateTimeFilled(new DtFunc().StringToDateTime(sValue, DtFunc.FmtISODate));
                    else
                        ret = StrFunc.IsFilled(sValue);
                }
                catch { ret = false; }
                return ret;
            }
        }
        public bool IsEmptyValue
        {
            get
            {
                return (false == IsFilledValue);
            }
        }
        #endregion
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pkey">Nom de bapt�me du cci</param>
        /// <param name="pCci">Repr�sente le cci</param>
        /// <param name="pOrder">priorit� du cci</param>
        public CciCompare(string pkey, CustomCaptureInfo pCci, int pOrder)
            : this(pkey, pCci.DataType, pCci.NewValue, pCci.IsInputByUser, pOrder)
        {
        }
        public CciCompare(string pkey, TypeData.TypeDataEnum pdataType, string pValue, bool pIsInputByUser, int pOrder)
        {
            key = pkey;
            dataType = pdataType;
            sValue = pValue;
            isInputByUser = pIsInputByUser;
            order = pOrder;
        }
        #endregion cosntructor

        #region Members de IComparable
        /// <summary>
        /// Est prioritaire (de poids plus faible)
        /// <para>
        /// - la donn�e non renseign�e (pour une donn�e numerique, on consid�re que 0 est une donn�e non renseign�e)  
        /// - sinon celle sp�cifi� comme prioritaire (order de poids le plus faible)
        /// - sinon la donn�e non saisie par l'utilisateur
        /// </para>
        /// </summary>
        /// <param name="pobj"></param>
        /// <returns></returns>
        public int CompareTo(object pObj)
        {
            if (pObj is CciCompare cci2)
            {
                int ret = 0; //Like Equal
                if ((ret == 0) && (dataType == cci2.dataType))
                {
                    if ((dataType == cci2.dataType))
                    {
                        if ((false == IsFilledValue) && cci2.IsFilledValue)
                            ret = -1; // cette instance est inf�rieure � pObj
                        if (IsFilledValue && (false == cci2.IsFilledValue))
                            ret = 1; // cette instance est sup�rieur � pObj
                    }
                    else
                    {
                        //20090609 FI On ne devrait jamais pass� ici
                        //Comparer des donn�es qui ne sont pas de m�me type, cela parait incoh�rent
                        //
                        if (StrFunc.IsEmpty(sValue) && StrFunc.IsFilled(cci2.sValue))
                            ret = -1; // cette instance est inf�rieure � pObj
                        if (StrFunc.IsFilled(sValue) && StrFunc.IsEmpty(cci2.sValue))
                            ret = 1; // cette instance est sup�rieur � pObj
                    }
                }
                //
                if ((IsEmptyValue) && (cci2.IsEmptyValue))
                {
                    if (ret == 0)
                        ret = order.CompareTo(cci2.order);   // cette instance est inf�rieure si son order et inf�rieur
                }
                //
                if (ret == 0)
                    ret = isInputByUser.CompareTo(cci2.isInputByUser);   // cette instance est inf�rieure si IsInputByUser =false 
                //
                if (ret == 0)
                    ret = order.CompareTo(cci2.order);   // cette instance est inf�rieure si son order et inf�rieur
                return ret;
            }
            throw new ArgumentException("object is not a CciCompare");
        }
        #endregion Members de IComparable
    }
    
    /// <summary>
    /// Description r�sum�e de RateTools.
    /// </summary>
    public sealed class RateTools
    {
        public const string
            FLOATING_RATE_TENOR_SEPERATOR = "/";

        public enum RateTypeEnum
        {
            RATE_FIXED,
            RATE_FLOATING,
        }
        #region constructor
        public RateTools() { }
        #endregion constructor
        #region GetTypeRate()
        //static RateTypeEnum GetTypeRate(string pRate)
        //{
        //    if (IsFixedRate(pRate))
        //        return RateTypeEnum.RATE_FIXED;
        //    else
        //        return RateTypeEnum.RATE_FLOATING;
        //}
        #endregion
        #region GetFloatingRateWithTenor() methods
        public static string GetFloatingRateWithTenor(string pFloatingRate, string pPeriodMultiplier, string pPeriod)
        {
            string ret = pFloatingRate;
            if (StrFunc.IsFilled(pPeriodMultiplier) && StrFunc.IsFilled(pPeriod))
            {
                ret += FLOATING_RATE_TENOR_SEPERATOR + pPeriodMultiplier + pPeriod;
            }
            return ret;
        }
        public static string GetFloatingRateWithTenor(string pFloatingRate, int pPeriodMultiplier, PeriodEnum pPeriod)
        {
            return GetFloatingRateWithTenor(pFloatingRate, pPeriodMultiplier.ToString(), pPeriod.ToString());
        }
        #endregion
        #region GetFloatingRateWithoutTenor()
        /// <summary>
        /// Return a floating string with tenor if exist (ie: EUR-EURIBOR-Telerate 3m, EUR-EONIA)
        /// </summary>
        /// <param name="pFloatingRateWithTenor"></param>
        /// <returns></returns>
        public static string GetFloatingRateWithoutTenor(string pFloatingRateWithTenor)
        {
            string ret = pFloatingRateWithTenor;
            int pos = pFloatingRateWithTenor.IndexOf(FLOATING_RATE_TENOR_SEPERATOR);
            if (pos > 0)
                ret = pFloatingRateWithTenor.Substring(0, pos);
            return ret;
        }
        #endregion

        #region IsFloatingRateWithTenor() methods
        /// <summary>
        /// Return true if the rate is a floating rate with tenor (ie: EUR-EURIBOR-Telerate 3M)
        /// </summary>
        /// <param name="pRate"></param>
        /// <returns></returns>
        public static bool IsFloatingRateWithTenor(string pFloatingRate)
        {
            return pFloatingRate.IndexOf(FLOATING_RATE_TENOR_SEPERATOR) > 0;
        }
        /// <summary>
        /// Return true if the rate is a floating rate with tenor (ie: EUR-EURIBOR-Telerate 3M)
        /// </summary>
        /// <param name="pFloatingRate"></param>
        /// <param name="opTenor">Return the string tenor (ie: 3M)</param>
        /// <returns></returns>
        public static bool IsFloatingRateWithTenor(string pFloatingRate, out string opTenor)
        {
            bool ret = IsFloatingRateWithTenor(pFloatingRate);
            if (ret)
                opTenor = pFloatingRate.Substring(pFloatingRate.IndexOf(FLOATING_RATE_TENOR_SEPERATOR) + 1).Trim();
            else
                opTenor = string.Empty;
            return ret;
        }
        public static bool IsFloatingRateWithTenor(string pFloatingRate, out int opTenor_periodMultiplier, out PeriodEnum opTenor_period)
        {
            bool ret = IsFloatingRateWithTenor(pFloatingRate, out string tenor);
            if (ret)
            {
                System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("\\d+");
                opTenor_periodMultiplier = Int32.Parse(re.Match(tenor).Value);
                string period = tenor.Substring(opTenor_periodMultiplier.ToString().Length).Trim();
                opTenor_period = StringToEnum.Period(period);
            }
            else
            {
                opTenor_periodMultiplier = 0;
                opTenor_period = PeriodEnum.D;
            }
            return ret;
        }
        #endregion IsFloatingRateWithTenor()
        #region IsFloatingRate()
        /// <summary>
        /// Return true if the rate is a floating rate
        /// </summary>
        /// <param name="pRate"></param>
        /// <returns></returns>
        public static bool IsFloatingRate(string pRate)
        {
            return (StrFunc.IsFilled(pRate)) && (!IsFixedRate(pRate));
        }
        #endregion
        #region IsFixedRate()
        public static bool IsFixedRate(string pRate)
        {
            bool isFixedRate = EFSRegex.IsMatch(pRate, EFSRegex.TypeRegex.RegexFixedRateExtend);
            return isFixedRate;
        }
        #endregion
    }

    
    /// <summary>
    /// 
    /// </summary>
    public class CaptureToolsBase
    {
        #region public IsDocumentElementValid
        public static bool IsDocumentElementValid(EFS_Decimal pData)
        {
            bool isOk = false;
            if (null != pData)
                isOk = StrFunc.IsFilled(pData.Value);
            return isOk;
        }
        public static bool IsDocumentElementValid(EFS_Date pData)
        {
            bool isOk = false;
            if (null != pData)
                isOk = !DtFunc.IsDateTimeEmpty(pData.DateValue);
            return isOk;
        }
        public static bool IsDocumentElementValid(string pData)
        {
            bool isOk = false;
            if (null != pData)
                isOk = StrFunc.IsFilled(pData);
            return isOk;
        }
        #endregion IsFpmlDocElementValid
    }
   
}
