#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.Process;
using EfsML.Business;
using EfsML.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
#endregion Using Directives

/* -------------------------------------------------------- */
/* ----- FONCTIONS UTILISEES POUR LA GESTION          ----- */
/* ----- DES CORPORATE ACTIONS (SAISIE et TRAITEMENT) ----- */
/* -------------------------------------------------------- */

namespace EfsML.CorporateActions
{

    public sealed partial class CATools
    {

        /// EG 20140506 [19913] New 
        public enum SQLRunTimeEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("C")]
            CONTROL,
            [System.Xml.Serialization.XmlEnumAttribute("E")]
            EMBEDDED,
            [System.Xml.Serialization.XmlEnumAttribute("P")]
            PRECEDING,
            [System.Xml.Serialization.XmlEnumAttribute("F")]
            FOLLOWING,
            [System.Xml.Serialization.XmlEnumAttribute("N")]
            NONE,
        }

        /// EG 20140506 [19913] New 
        public enum DOCTypeEnum
        {
            MSSQL,
            ORA,
            XML,
            LOG,
            HTML,
        }

        public const string AI_Undo = "##UNDO##";
        public const string SQLTEMPLATE_TITLE = "%%TITLE_CA%%";
        public const string SQLTEMPLATE_SUMMARY = "%%SUMMARY_CA%%";
        public const string SQLTEMPLATE_IDPROCESS_L = "%%IDPROCESS_L%%";
        public const string SQLTEMPLATE_PROCESSTYPE = "%%PROCESS%%";
        public const string SQLTEMPLATE_SCRIPTNAME = "%%SCRIPTNAME%%";
        public const string SQLTEMPLATE_PLACEHOLDER = "/* %%SCRIPT_CA%% */";
        // EG 20211109 [XXXXX] New constants
        public const string SQLTEMPLATE_HOSTNAME = "%%HOSTNAME%%";
        public const string SQLTEMPLATE_APPNAME = "%%APPNAME%%";
        public const string SQLTEMPLATE_APPVERSION = "%%APPVERSION%%";


        #region CAElementTypeEnum
        public enum CAElementTypeEnum
        {
            Cum,
            Ex,
            ExAdj,
            ExRecycled,
        }
        #endregion CAElementTypeEnum

        #region CAElementEnum
        public enum CAElementEnum
        {
            cmul,
            csize,
            dsn,
            desc,
            isin,
            qtymul,
            rsym,
            sym,
            unlisin,
        }
        #endregion CAElementEnum

        #region ResultValueEnum
        public enum ResultValueEnum
        {
            Gross,
            Rounded,
        }
        #endregion ResultValueEnum
        
        #region AdjStatusEnum
        public enum AdjStatusEnum
        {
            RFACTORCERTIFIED_RETAINED,
            RFACTOR_UNEVALUATED,
            EXCSIZE_UNEVALUATED,
            EXCMULTIPLIER_UNEVALUATED,
            EXSTRIKEPRICE_UNEVALUATED,
            EXDCLOSINGPRICE_UNEVALUATED,
            TAKEPLACE_UNEVALUATED,
            EVALUATED,
            FAILURE,
            EQUALPAYMENT_DCP_NOTFOUND,
            REQUESTED,
        }
        #endregion AdjStatusEnum

        #region ValidationGroupEnum
        /// <summary>
        /// Groupe de validation pour les validators de Saisie
        /// CA = Panel Corporate action
        /// CE = Panel Corporate event
        /// </summary>
        public enum ValidationGroupEnum
        {
            CA,
            CE,
        }
        #endregion ValidationGroupEnum

        #region Constants
        public const string pfxLBLComponent = "lblCE";
        public const string pfxTXTComponent = "txtCE";
        public const string pfxDDLComponent = "ddlCE";
        public const string pfxCHKComponent = "chkCE";
        public const string pfxLBLNoticeAdd = "lblRN_";
        public const string pfxTXTNoticeAdd = "txtRN_";
        public const string RFactorCertified_Id = "ratioFactorCertified";
        public const string RFactorCertified_Name = "RCertified";
        public const string RFactorCertified_Description = "Official R-Factor";
        #endregion Constants


        public class FormulaArgsComparer : IComparer<string>
        {
            public int Compare(string pArg1, string pArg2)
            {
                int ret = 0;
                if (StrFunc.IsFilled(pArg1) && StrFunc.IsFilled(pArg2))
                {
                    ret = pArg1.Length.CompareTo(pArg2.Length);
                    if (0 == ret)
                        ret = pArg1.CompareTo(pArg2);
                }
                return ret;
            }
        }


        /* -------------------------------------------------------- */
        /* ----- PATTERNS FICHIERS TEMPLATES                  ----- */
        /* -------------------------------------------------------- */

        #region GetRegularTemplateAIPattern
        /// <summary>
        /// Pattern pour fichiers Template d'Additional Infos 
        /// </summary>
        /// <returns></returns>
        public static string GetRegularTemplateAIPattern()
        {
            string regex = string.Empty;
            regex += @"^";
            regex += @"AI";
            regex += @"(_\w+)?"; // Libelle optionnel
            regex += @"$";
            return regex;
        }
        #endregion GetRegularTemplateAIPattern

        #region GetRegularTemplateAdditionalPattern
        /// <summary>
        /// Pattern pour fichiers Template additionels
        /// </summary>
        /// <returns></returns>
        public static string GetRegularTemplateAdditionalPattern()
        {
            string regex = string.Empty;
            regex += @"^";
            regex += @"AI|CT|MSSQL|ORA";
            regex += @"(_\w+)?"; // Libelle optionnel
            regex += @"$";
            return regex;
        }
        #endregion GetRegularTemplateAdditionalPattern

        #region GetRegularTemplatePattern
        /// <summary>
        /// Pattern pour fichiers Template d'ajustement des Corporate actions "simples"
        /// </summary>
        /// <returns></returns>
        public static string GetRegularTemplatePattern()
        {
            string regex = string.Empty;
            regex += @"^";
            regex += @"\w+";     // Corporate event GROUP
            regex += @"_\w+";     // Corporate event METHOD
            regex += @"_\w+";  // Corporate event TYPE
            regex += @"(_\w+)?"; // Libelle optionnel
            regex += @"$";
            return regex;
        }
        #endregion GetRegularTemplatePattern
        #region GetCombinedTemplatePattern
        /// <summary>
        /// Pattern pour fichiers Template d'ajustement des Corporate actions "combinés"
        /// </summary>
        /// <returns></returns>
        public static string GetCombinedTemplatePattern()
        {
            string regex = string.Empty;
            regex += @"^";
            regex += @"\w+";         // Corporate event GROUP
            regex += @"_\w+";   // Corporate event METHOD
            regex += @"_\w+";   // Corporate event TYPE
            regex += @"_FollowedBy|Together";   // Combination operand
            regex += @"_\w+";   // Corporate event TYPE 2
            regex += @"(_\w+)?";  // Libelle optionnel
            regex += @"$";
            return regex;
        }
        #endregion GetCombinedTemplatePattern

        #region GetRatioCertifiedTemplatePattern
        /// <summary>
        /// Pattern pour fichiers Template d'ajustement des Corporate actions avec ratio Certifié
        /// </summary>
        /// <returns></returns>
        public static string GetRatioCertifiedTemplatePattern()
        {
            string regex = string.Empty;
            regex += @"^";
            regex += @"All";     // Corporate event GROUP
            regex += @"_Ratio";     // Corporate event METHOD
            regex += @"_All";     // Corporate event TYPE
            regex += @"_RatioCertified";  // Libelle optionnel
            regex += @"$";
            return regex;
        }
        #endregion GetRatioCertifiedTemplatePattern

        /* -------------------------------------------------------- */
        /* ----- EXECUTION SCRIPT SQL                         ----- */
        /* ----- EMBEDDED, PRECEDING et FOLLOWING             ----- */
        /* -------------------------------------------------------- */
        #region ExecCorporateActionSQLScript
        // EG 20211109 [XXXXX] Changement de signature (usage de ProcessBase)
        public static Cst.ErrLevel ExecCorporateActionSQLScript(ProcessBase pProcessBase, string pProcessType, 
            CorporateAction pCorporateAction, CATools.SQLRunTimeEnum pRunTime, string pSQLTemplatePath)
        {
            return ExecCorporateActionSQLScript(pProcessBase, pProcessType, pCorporateAction, pRunTime, pSQLTemplatePath, null, null);
        }
        // EG 20211109 [XXXXX] Changement de signature (usage de ProcessBase)
        public static Cst.ErrLevel ExecCorporateActionSQLScript(ProcessBase pProcessBase,string pProcessType,
            CorporateAction pCorporateAction, CATools.SQLRunTimeEnum pRunTime, string pSQLTemplatePath, string pCAIdentifier, MQueueparameters pParameters)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (pCorporateAction.corporateDocsSpecified)
            {
                CATools.DOCTypeEnum _docType = DataHelper.IsDbOracle(pProcessBase.Cs) ? CATools.DOCTypeEnum.ORA : CATools.DOCTypeEnum.MSSQL;
                Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = new Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>(_docType, pRunTime);
                // Le script à exécuter existe
                if (pCorporateAction.corporateDocs.ContainsKey(_key))
                {
                    CorporateDoc _corporateDoc = pCorporateAction.corporateDocs[_key];
                    string _script = Encoding.ASCII.GetString(_corporateDoc.script, 0, _corporateDoc.script.Length);

                    if (StrFunc.IsFilled(_script))
                    {
                        string fileName = pSQLTemplatePath + _docType + "_Template.sql";
                        if (File.Exists(fileName))
                        {
                            Byte[] _SQLTemplateByte = FileTools.ReadFileToBytes(fileName);
                            string _SQLTemplateString = Encoding.ASCII.GetString(_SQLTemplateByte, 0, _SQLTemplateByte.Length);
                            if (StrFunc.IsFilled(_SQLTemplateString))
                            {
                                // Mise à jour du titre et du summary du script //
                                _SQLTemplateString = _SQLTemplateString.Replace(SQLTEMPLATE_TITLE, "Script (" + pRunTime + ")");
                                string _identifier = string.Empty;
                                string _effectiveDate = string.Empty;
                                if (ArrFunc.IsFilled(pCorporateAction.corporateEvent))
                                {
                                    _identifier = pCorporateAction.identifier;
                                    _effectiveDate = DtFunc.DateTimeToStringyyyyMMdd(pCorporateAction.corporateEvent[0].effectiveDate);
                                }
                                else if (null != pParameters)
                                {
                                    _identifier = pCAIdentifier;
                                    MQueueparameter parameter = pParameters["EFFECTIVEDATE"];
                                    if (null != parameter)
                                        _effectiveDate = parameter.Value;
                                }
                                _SQLTemplateString = _SQLTemplateString.Replace(SQLTEMPLATE_SUMMARY, _identifier + " (" + _effectiveDate + ")");

                                // Mise à jour des ids LOG + TRACKER et du PROCESSNAME //
                                _SQLTemplateString = _SQLTemplateString.Replace(SQLTEMPLATE_IDPROCESS_L, pProcessBase.IdProcess > 0 ? pProcessBase.IdProcess.ToString() : "null");
                                _SQLTemplateString = _SQLTemplateString.Replace(SQLTEMPLATE_PROCESSTYPE, pProcessType);
                                _SQLTemplateString = _SQLTemplateString.Replace(SQLTEMPLATE_SCRIPTNAME, _corporateDoc.identifier);
                                // EG 20211109 [XXXXX] Nouveaux paramètres
                                _SQLTemplateString = _SQLTemplateString.Replace(SQLTEMPLATE_HOSTNAME, pProcessBase.AppInstance.HostName);
                                _SQLTemplateString = _SQLTemplateString.Replace(SQLTEMPLATE_APPNAME, pProcessBase.AppInstance.AppNameInstance);
                                _SQLTemplateString = _SQLTemplateString.Replace(SQLTEMPLATE_APPVERSION, pProcessBase.AppInstance.AppVersion);

                                _script = _SQLTemplateString.Replace(SQLTEMPLATE_PLACEHOLDER, _script);
                            }
                        }
                        QueryParameters qryParameters = new QueryParameters(pProcessBase.Cs, _script, null);
                        DataHelper.ExecuteNonQuery(pProcessBase.Cs, CommandType.Text, qryParameters.Query);
                    }
                }
            }
            return codeReturn;
        }
        #endregion ExecCorporateActionSQLScript

        /* -------------------------------------------------------- */
        /* ----- EXTRACTION DES COMPOSANTS DANS UNE           ----- */
        /* ----- PROCEDURE D'AJUSTEMENT                       ----- */
        /* -------------------------------------------------------- */
        #region AllComponentsSimples
        /// <summary>
        /// Retourne tous les composants simples d'une CorporateEventProcedure
        /// </summary>
        /// <param name="pProcedure"></param>
        /// <returns></returns>
        public static List<ComponentSimple> AllComponentSimples(CorporateEventProcedure pProcedure)
        {
            List<ComponentSimple> _list = new List<ComponentSimple>();
            Dictionary<Pair<string, string>, ComponentBase> _dicComponents = new Dictionary<Pair<string, string>, ComponentBase>();
            AllComponents(pProcedure.underlyers, ref _dicComponents);
            AllComponents(pProcedure.adjustment, ref _dicComponents);
            _list.AddRange(_dicComponents.Values.OfType<ComponentSimple>());
            return _list;
        }
        /// <summary>
        /// Retourne tous les composants simples d'une CorporateEventProcedure
        /// </summary>
        /// <param name="pProcedure"></param>
        /// <returns></returns>
        public static List<ComponentSimple> AllComponentSimples<T>(T pSource)
        {
            List<ComponentSimple> _list = new List<ComponentSimple>();
            Dictionary<Pair<string, string>, ComponentBase> _dicComponents = new Dictionary<Pair<string, string>, ComponentBase>();

            if (pSource is CorporateEventProcedure)
            {
                CorporateEventProcedure _procedure = pSource as CorporateEventProcedure;
                AllComponents(_procedure.underlyers, ref _dicComponents);
                AllComponents(_procedure.adjustment, ref _dicComponents);
            }
            else if (pSource is AdditionalInfo)
            {
                AdditionalInfo _additionalInfo = pSource as AdditionalInfo;
                AllComponents(_additionalInfo, ref _dicComponents);
            }
            _list.AddRange(_dicComponents.Values.OfType<ComponentSimple>());
            return _list;
        }
        #endregion AllComponentsSimples
        #region AllComponents
        /// <summary>
        /// Charge dans un dictionnaire tous les composants présents et utiles à une évaluation
        /// pour une source donnée
        /// </summary>
        /// <typeparam name="T">Type de la source (Classes de Corporate actions: RFactor, CorprateEventUnderlyer ... )</typeparam>
        /// <param name="pSource">Source</param>
        /// <param name="poDicComponents">Dictionnaire des composantes</param>
        // EG [33415/33420] Gestion AdditionalInfo
        public static void AllComponents<T>(T pSource, ref Dictionary<Pair<string,string>,ComponentBase> poDicComponents )
        {
            Dictionary<Pair<string, string>, ComponentBase> _dicComponents = new Dictionary<Pair<string, string>, ComponentBase>();
            if ((null != poDicComponents) && (0 < poDicComponents.Count))
                _dicComponents = poDicComponents;
            string idParent = string.Empty;
            object[] components = null;
            if (pSource is Array)
            {
                #region ARRAY
                Array _array = pSource as Array;
                foreach (object _source in _array)
                    AllComponents(_source, ref _dicComponents);
                #endregion ARRAY
            }
            else if (pSource is CorporateEventUnderlyer)
            {
                #region SOUS-JACENT
                CorporateEventUnderlyer _source = pSource as CorporateEventUnderlyer;
                idParent = _source.id;
                if (_source.componentSpecified)
                    components = _source.component;
                #endregion SOUS-JACENT
            }
            else if (pSource is TakePlace)
            {
                #region TAKEPLACE (Condition d'application)
                TakePlace _source = pSource as TakePlace;
                components = new object[1] { _source.component };
                #endregion TAKEPLACE (Condition d'application)
            }
            else if (pSource is AdjustmentRatio)
            {
                #region RATIO
                AdjustmentRatio _source = pSource as AdjustmentRatio;
                if (null != _source.rFactor)
                    AllComponents(_source.rFactor, ref _dicComponents);
                if (_source.takePlaceSpecified)
                    AllComponents(_source.takePlace, ref _dicComponents);
                if (_source.contract.futureSpecified)
                    AllComponents(_source.contract.future, ref _dicComponents);
                if (_source.contract.optionSpecified)
                    AllComponents(_source.contract.option, ref _dicComponents);

                #endregion RATIO
            }
            else if (pSource is AdjustmentPackage)
            {
                // TBD : A développer
                #region PACKAGE
                #endregion PACKAGE
            }
            else if (pSource is AdjustmentFairValue)
            {
                // TBD : A développer
                #region FAIRVALUE
                #endregion FAIRVALUE
            }
            else if (pSource is AdjustmentNone)
            {
                #region NO ADJUSTMENT
                // EG [33415/33420] New
                AdjustmentNone _source = pSource as AdjustmentNone;
                if (_source.takePlaceSpecified)
                    AllComponents(_source.takePlace, ref _dicComponents);
                #endregion NO ADJUSTMENT
            }
            else if (pSource is AdjustmentFuture)
            {
                #region FUTURE
                AdjustmentFuture _source = pSource as AdjustmentFuture;
                if (_source.contractSizeSpecified)
                    AllComponents(_source.contractSize, ref _dicComponents);
                if (_source.contractMultiplierSpecified)
                    AllComponents(_source.contractMultiplier, ref _dicComponents);
                if (_source.priceSpecified)
                    AllComponents(_source.price, ref _dicComponents);
                #endregion FUTURE
            }
            else if (pSource is AdjustmentOption)
            {
                #region OPTION
                AdjustmentOption _source = pSource as AdjustmentOption;
                if (_source.contractSizeSpecified)
                    AllComponents(_source.contractSize, ref _dicComponents);
                if (_source.contractMultiplierSpecified)
                    AllComponents(_source.contractMultiplier, ref _dicComponents);
                if (_source.strikePriceSpecified)
                    AllComponents(_source.strikePrice, ref _dicComponents);
                if (_source.priceSpecified)
                    AllComponents(_source.price, ref _dicComponents);
                #endregion OPTION
            }
            else if (pSource is RFactor)
            {
                #region RFACTOR
                RFactor _source = pSource as RFactor;
                idParent = _source.id;
                components = _source.component;
                if (ArrFunc.IsFilled(_source.component) &&
                    (_source.component[0] is ComponentReference || _source.component[0] is ComponentFormula) && _source.rFactorCertifiedSpecified)
                    AllComponents(_source.rFactorCertified, ref _dicComponents);
                #endregion RFACTOR
            }
            else if (pSource is AdjustmentContractSize)
            {
                #region CONTRACTSIZE
                AdjustmentContractSize _source = pSource as AdjustmentContractSize;
                idParent = _source.id;
                components = _source.component;
                #endregion CONTRACTSIZE
            }
            else if (pSource is AdjustmentContractMultiplier)
            {
                #region CONTRACTSIZE
                AdjustmentContractMultiplier _source = pSource as AdjustmentContractMultiplier;
                idParent = _source.id;
                components = _source.component;
                #endregion CONTRACTSIZE
            }
            else if (pSource is AdjustmentPrice)
            {
                #region PRICE
                AdjustmentPrice _source = pSource as AdjustmentPrice;
                idParent = _source.id;
                components = _source.component;
                #endregion PRICE
            }
            else if (pSource is AdjustmentStrikePrice)
            {
                #region STRIKEPRICE
                AdjustmentStrikePrice _source = pSource as AdjustmentStrikePrice;
                idParent = _source.id;
                components = _source.component;
                #endregion STRIKEPRICE
            }
            else if (pSource is AdditionalInfo)
            {
                // EG [33415/33420] New
                #region ADDITIONAL INFO
                AdditionalInfo _source = pSource as AdditionalInfo;
                components = _source.component;
                #endregion ADDITIONAL INFO
            }
            else if (pSource is ComponentFormula)
            {
                #region Composant FORMULE
                ComponentFormula _source = pSource as ComponentFormula;
                idParent = _source.Id;
                if (_source.formula.componentSpecified)
                    components = _source.formula.component;
                #endregion Composant FORMULE
            }
            else if (pSource is ComponentMethod)
            {
                #region Composant METHOD
                ComponentMethod _source = pSource as ComponentMethod;
                idParent = _source.Id;
                if (_source.method.componentSpecified)
                    components = _source.method.component;
                #endregion Composant METHOD
            }
            else if (pSource is ComponentSimple)
            {
                #region Composant SIMPLE (used for RFactorCertified)
                ComponentSimple _source = pSource as ComponentSimple;
                Pair<string, string> _key = new Pair<string, string>(_source.Id, _source.GetType().Name);
                if (false == _dicComponents.ContainsKey(_key))
                    _dicComponents.Add(_key, _source);
                #endregion Composant SIMPLE (used for RFactorCertified)
            }
            if (StrFunc.IsEmpty(idParent))
                idParent = pSource.GetType().Name;

            if (ArrFunc.IsFilled(components))
            {
                // Alimentation du dictionnaire et 
                // appel récursif sur les composants FORMULE et METHOD
                foreach (ComponentBase component in components)
                {
                    Pair<string, string> _key = new Pair<string, string>(component.Id,idParent);
                    if (false == _dicComponents.ContainsKey(_key))
                    {
                        _dicComponents.Add(_key, component);
                        if ((component is ComponentFormula) || (component is ComponentMethod))
                            AllComponents(component, ref _dicComponents);
                    }
                }
            }
            poDicComponents = _dicComponents;
        }

        #endregion AllComponents

        /* -------------------------------------------------------- */
        /* ----- EVALUATION DES COMPOSANTS DANS UNE           ----- */
        /* ----- PROCEDURE D'AJUSTEMENT                       ----- */
        /* -------------------------------------------------------- */

        #region SetAdjStatus
        public static void SetAdjStatus<T1,T2>(T1 pSource, T2 pTarget, Cst.ErrLevel pErrLevel)
        {
            CATools.AdjStatusEnum adjStatus = AdjStatusEnum.REQUESTED;
            if (Cst.ErrLevel.SUCCESS != pErrLevel)
            {
                if (pSource is RFactor)
                {
                    if (pErrLevel == Cst.ErrLevel.RFACTOR_NOTCONFORM)
                        adjStatus = CATools.AdjStatusEnum.RFACTORCERTIFIED_RETAINED;
                    else
                        adjStatus = CATools.AdjStatusEnum.RFACTOR_UNEVALUATED;
                }
                else if (pSource is AdjustmentContractSize)
                    adjStatus = CATools.AdjStatusEnum.EXCSIZE_UNEVALUATED;
                else if (pSource is AdjustmentContractMultiplier)
                    adjStatus = CATools.AdjStatusEnum.EXCMULTIPLIER_UNEVALUATED;
                else if (pSource is AdjustmentPrice)
                    adjStatus = CATools.AdjStatusEnum.EXDCLOSINGPRICE_UNEVALUATED;
                else if (pSource is AdjustmentStrikePrice)
                    adjStatus = CATools.AdjStatusEnum.EXSTRIKEPRICE_UNEVALUATED;
                else if (pSource is TakePlace)
                    adjStatus = CATools.AdjStatusEnum.TAKEPLACE_UNEVALUATED;
            }
            else
            {
                adjStatus = AdjStatusEnum.EVALUATED;
                if (pTarget is CorporateEventContract)
                {
                    CorporateEventContract _target = pTarget as CorporateEventContract;
                    if (_target.adjStatus == AdjStatusEnum.RFACTORCERTIFIED_RETAINED)
                        adjStatus = _target.adjStatus;
                }
            }

            if (pTarget is CorporateEventContract)
            {
                CorporateEventContract _target = pTarget as CorporateEventContract;
                if (CATools.IsAdjStatusOK(_target.adjStatus))
                    _target.adjStatus = adjStatus;
            }
            else if (pTarget is CorporateEventDAttrib)
            {
                CorporateEventDAttrib _target = pTarget as CorporateEventDAttrib;
                if (CATools.IsAdjStatusOK(_target.adjStatus))
                    _target.adjStatus = adjStatus;
            }
            else if (pTarget is CorporateEventAsset)
            {
                CorporateEventAsset _target = pTarget as CorporateEventAsset;
                if (CATools.IsAdjStatusOK(_target.adjStatus))
                    _target.adjStatus = adjStatus;
            }
            else if (pTarget is RFactor)
            {
                RFactor _target = pTarget as RFactor;
                if (CATools.IsAdjStatusOK(_target.adjStatus))
                    _target.adjStatus = adjStatus;
            }
        }
        #endregion SetAdjStatus
        #region SetCodeReturnAfterAdjustment
        public static void SetCodeReturnAfterAdjustment<T>(T pSource, ref Cst.ErrLevel pCodeReturn)
        {
            if (pSource is CorporateEventContract)
            {
                CorporateEventContract _source = pSource as CorporateEventContract;
                if (_source.dAttribsSpecified)
                {
                    foreach (CorporateEventDAttrib _dAttrib in _source.dAttribs)
                        SetCodeReturnAfterAdjustment(_dAttrib, ref pCodeReturn);
                }
                else
                {
                    SetCodeReturnAfterAdjustment(_source.adjStatus, ref pCodeReturn);
                }
            }
            else if (pSource is CorporateEventDAttrib)
            {
                CorporateEventDAttrib _source = pSource as CorporateEventDAttrib;
                if (_source.assetsSpecified)
                {
                    foreach (CorporateEventAsset _asset in _source.assets)
                        SetCodeReturnAfterAdjustment(_asset, ref pCodeReturn);
                }
            }
            else if (pSource is CorporateEventAsset)
            {
                CorporateEventAsset _source = pSource as CorporateEventAsset;
                SetCodeReturnAfterAdjustment(_source.adjStatus, ref pCodeReturn);
            }
            else if (pSource is AdjStatusEnum)
            {
                Nullable<AdjStatusEnum> _status = pSource as Nullable<AdjStatusEnum>;
                if (_status.HasValue)
                {
                    switch (_status.Value)
                    {
                        case AdjStatusEnum.EVALUATED:
                            if ((pCodeReturn != Cst.ErrLevel.FAILURE) && (pCodeReturn != Cst.ErrLevel.DATANOTFOUND) && (pCodeReturn != Cst.ErrLevel.FAILUREWARNING))
                                pCodeReturn = Cst.ErrLevel.SUCCESS;
                            break;
                        case AdjStatusEnum.FAILURE:
                            pCodeReturn = Cst.ErrLevel.FAILURE;
                            break;
                        case AdjStatusEnum.EXDCLOSINGPRICE_UNEVALUATED:
                        case AdjStatusEnum.RFACTORCERTIFIED_RETAINED:
                            pCodeReturn = Cst.ErrLevel.FAILUREWARNING;
                            break;
                        default:
                            if (pCodeReturn != Cst.ErrLevel.FAILURE)
                                pCodeReturn = Cst.ErrLevel.DATANOTFOUND;
                            break;
                    }
                }
            }
        }
        #endregion SetCodeReturnAfterAdjustment
        #region IsAdjStatusOK
        public static bool IsAdjStatusOK(CATools.AdjStatusEnum pAdjStatus)
        {
            return (pAdjStatus == AdjStatusEnum.EVALUATED || pAdjStatus == AdjStatusEnum.REQUESTED || pAdjStatus == AdjStatusEnum.RFACTORCERTIFIED_RETAINED);
        }
        #endregion IsAdjStatusOK
        #region ApplyRounding
        /// <summary>
        // Application de l'arrondi après évaluation d'un composant final 
        /// </summary>
        // EG 20190121 [23249] Sauvegarde des règles d'arrondi 
        public static void ApplyRounding(AdjustmentElement pAdjElement, int? pContractPrecision)
        {
            ComponentBase _component = pAdjElement.component[0] as ComponentBase;
            Nullable<decimal> _result = _component.Result;
            if (_result.HasValue)
            {
                // Application de l'arrondi
                Rounding _rounding = pAdjElement.Rounding;
                EFS_Round round = new EFS_Round(_rounding.direction, (pContractPrecision ?? _rounding.precision), _result.Value);
                decimal _resultRounded = round.AmountRounded;

                switch (_component.result.itemsElementName)
                {
                    case ResultType.amount:
                        Money _money = _component.result.result as Money;
                        _money.amountRoundedSpecified = true;
                        _money.amountRounded = new EFS_Decimal(_resultRounded);
                        // EG 20190121 [23249]
                        _money.SaveRounding(_rounding, pContractPrecision);
                        break;
                    case ResultType.unit:
                        SimpleUnit _simpleUnit = _component.result.result as SimpleUnit;
                        _simpleUnit.valueRoundedSpecified = true;
                        _simpleUnit.valueRounded = new EFS_Decimal(_resultRounded);
                        // EG 20190121 [23249]
                        _simpleUnit.SaveRounding(_rounding, pContractPrecision);
                        break;
                }
            }
        }
        #endregion ApplyRounding
        #region Result
        /// <summary>
        /// Retourne le résultat d'un calcul
        /// </summary>
        /// <param name="pComponent"></param>
        /// <returns></returns>
        public static Nullable<decimal> Result(Result pComponent,CATools.ResultValueEnum pResultValue)
        {
            Nullable<decimal> ret = null;
            switch (pComponent.itemsElementName)
            {
                case ResultType.unit:
                    SimpleUnit _simpleUnit = (SimpleUnit)pComponent.result;
                    if (pResultValue == ResultValueEnum.Gross)
                        ret = _simpleUnit.value.DecValue;
                    else if (_simpleUnit.valueRoundedSpecified)
                        ret = _simpleUnit.valueRounded.DecValue;
                    break;
                case ResultType.amount:
                    Money _money = (Money)pComponent.result;
                    if (pResultValue == ResultValueEnum.Gross)
                        ret = _money.amount.DecValue;
                    else if (_money.amountRoundedSpecified)
                        ret = _money.amountRounded.DecValue;
                    break;
            }
            return ret;
        }
        #endregion Result

        #region EvaluateComponent (POINT D'ENTREE)
        /// <summary>
        /// Evaluation des composants d'une source (appel récursif)
        /// </summary>
        /// <typeparam name="T">Type de la source</typeparam>
        /// <param name="pSource">Source</param>
        /// <param name="pWorkingDataContainer">Classe paramètres (CS, dictionnaire des composants, gestion des messages retour, etc)</param>
        /// <returns>ErrLevel</returns>
        // EG [33415/33420] Gestion AdditionalInfo
        public static Cst.ErrLevel EvaluateComponent<T>(T pSource, CAWorkingDataContainer pWorkingDataContainer)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ComponentBase[] _components = null;
            if (pSource is Array srcArray)
            {
                #region ARRAY
                pWorkingDataContainer.SetMessageTitle(pSource);
                foreach (object _source in srcArray)
                {
                    ret = EvaluateComponent(_source, pWorkingDataContainer);
                    if (Cst.ErrLevel.SUCCESS != ret)
                        break;
                }
                #endregion ARRAY
            }
            else if (pSource is CorporateEventUnderlyer srcUnderlyer)
            {
                #region SOUS-JACENT
                srcUnderlyer.WorkingDataContainer = pWorkingDataContainer;
                pWorkingDataContainer.SetMessageTitle(srcUnderlyer);
                _components = srcUnderlyer.component;
                #endregion SOUS-JACENT
            }
            else if (pSource is TakePlace srcTakePlace)
            {
                #region TAKEPLACE (Condition d'application)
                if (false == srcTakePlace.component is ComponentFormula)
                    pWorkingDataContainer.SetMessageTitle(pSource);
                _components = new ComponentBase[1] { srcTakePlace.component };
                #endregion TAKEPLACE (Condition d'application)
            }
            else if (pSource is AdditionalInfo srcAdditionalInfo)
            {
                // EG [33415/33420] New
                #region ADDITIONAL INFO
                _components = srcAdditionalInfo.component;
                #endregion ADDITIONAL INFO
            }
            else if (pSource is AdjustmentRatio srcAdjustmentRatio)
            {
                #region RATIO
                if (null != srcAdjustmentRatio.rFactor)
                    ret = EvaluateComponent(srcAdjustmentRatio.rFactor, pWorkingDataContainer);
                if (srcAdjustmentRatio.takePlaceSpecified)
                    ret = EvaluateComponent(srcAdjustmentRatio.takePlace, pWorkingDataContainer);
                if (srcAdjustmentRatio.contract.futureSpecified)
                    ret = EvaluateComponent(srcAdjustmentRatio.contract.future, pWorkingDataContainer);
                if (srcAdjustmentRatio.contract.optionSpecified)
                    ret = EvaluateComponent(srcAdjustmentRatio.contract.option, pWorkingDataContainer);
                #endregion RATIO
            }
            else if (pSource is AdjustmentFuture srcAdjustmentFuture)
            {
                #region FUTURE
                if (srcAdjustmentFuture.contractSizeSpecified)
                    ret = EvaluateComponent(srcAdjustmentFuture.contractSize, pWorkingDataContainer);
                if (srcAdjustmentFuture.contractMultiplierSpecified)
                    ret = EvaluateComponent(srcAdjustmentFuture.contractMultiplier, pWorkingDataContainer);
                if (srcAdjustmentFuture.priceSpecified)
                    ret = EvaluateComponent(srcAdjustmentFuture.price, pWorkingDataContainer);
                #endregion FUTURE
            }
            else if (pSource is AdjustmentOption srcAdjustmentOption)
            {
                #region OPTION
                if (srcAdjustmentOption.contractSizeSpecified)
                    ret = EvaluateComponent(srcAdjustmentOption.contractSize, pWorkingDataContainer);
                if (srcAdjustmentOption.contractMultiplierSpecified)
                    ret = EvaluateComponent(srcAdjustmentOption.contractMultiplier, pWorkingDataContainer);
                if (srcAdjustmentOption.strikePriceSpecified)
                    ret = EvaluateComponent(srcAdjustmentOption.strikePrice, pWorkingDataContainer);
                if (srcAdjustmentOption.priceSpecified)
                    ret = EvaluateComponent(srcAdjustmentOption.price, pWorkingDataContainer);
                #endregion OPTION
            }
            else if (pSource is RFactor srcRFactor)
            {
                #region RFACTOR
                srcRFactor.WorkingDataContainer = pWorkingDataContainer;
                _components = srcRFactor.component;
                #endregion RFACTOR
            }
            else if (pSource is AdjustmentContractSize srcAdjustmentContractSize)
            {
                #region CONTRACTSIZE
                _components = srcAdjustmentContractSize.component;
                #endregion CONTRACTSIZE
            }
            else if (pSource is AdjustmentContractMultiplier srcAdjustmentContractMultiplier)
            {
                #region CONTRACTMULTIPLIER
                _components = srcAdjustmentContractMultiplier.component;
                #endregion CONTRACTMULTIPLIER
            }
            else if (pSource is AdjustmentPrice srcAdjustmentPrice)
            {
                #region PRICE
                _components = srcAdjustmentPrice.component;
                #endregion PRICE
            }
            else if (pSource is AdjustmentStrikePrice srcAdjustmentStrikePrice)
            {
                #region STRIKEPRICE
                _components = srcAdjustmentStrikePrice.component;
                #endregion STRIKEPRICE
            }
            else if (pSource is ComponentFormula srcComponentFormula)
            {
                #region Composant FORMULE
                if (srcComponentFormula.formula.componentSpecified)
                    _components = srcComponentFormula.formula.component;
                #endregion Composant FORMULE
            }
            else if (pSource is ComponentMethod srcComponentMethod)
            {
                #region Composant METHOD
                if (srcComponentMethod.method.componentSpecified)
                    _components = srcComponentMethod.method.component;
                #endregion Composant METHOD
            }
            else if (pSource is ComponentBase srcComponentBase)
            {
                _components = new ComponentBase[1]{ srcComponentBase};
            }

            if (ArrFunc.IsFilled(_components))
            {
                foreach (ComponentBase _component in _components)
                {
                    ret = EvaluateComponent(pSource, _component, pWorkingDataContainer, pSource);
                    if (Cst.ErrLevel.SUCCESS != ret)
                        break;
                }
            }
            return ret;
        }
        #endregion EvaluateComponent (POINT D'ENTREE)
        #region EvaluateComponent (BASE)
        /// <summary>
        /// Lecture d'un composant (BASE) aiguillage et appel de la méthode correspondant au type de composant
        /// </summary>
        /// <typeparam name="T">Type de la source</typeparam>
        /// <param name="pComponent">Composant</param>
        /// <param name="pWorkingParameters">Classe paramètres (CS, dictionnaire des composants, gestion des messages retour, etc)</param>
        /// <returns>ErrLevel</returns>
        public static Cst.ErrLevel EvaluateComponent<T,T1>(T pSource, ComponentBase pComponent, CAWorkingDataContainer pWorkingDataContainer, T1 pTarget)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pComponent is ComponentProperty cpProperty)
                ret = EvaluateComponent(pSource, cpProperty, pTarget);
            else if (pComponent is ComponentMethod cpMethod)
                ret = EvaluateComponent(pSource, cpMethod, pWorkingDataContainer, pTarget);
            else if (pComponent is ComponentSimple cpSimple)
                ret = EvaluateComponent(cpSimple);
            else if (pComponent is ComponentReference cpReference)
                ret = EvaluateComponent(pSource, cpReference, pWorkingDataContainer);
            else if (pComponent is ComponentFormula cpFormula)
                ret = EvaluateComponent(pSource, cpFormula, pWorkingDataContainer, pTarget);

            pWorkingDataContainer.SetMessageResult(pComponent, ret);

            if (false == pComponent.resultSpecified)
                ret = Cst.ErrLevel.DATANOTFOUND;
            return ret;
        }
        #endregion EvaluateComponent (BASE)
        #region EvaluateComponent (SIMPLE)
        /// <summary>
        /// Evaluation d'un composant SIMPLE (ici Lecture car composant renseigné à la saisie d'une CA)
        /// </summary>
        /// <param name="pComponent">Composant></param>
        /// <returns>ErrLevel</returns>
        public static Cst.ErrLevel EvaluateComponent(ComponentSimple pComponent)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (false == pComponent.resultSpecified)
                ret = Cst.ErrLevel.DATANOTFOUND;
            return ret;
        }
        #endregion EvaluateComponent (SIMPLE)
        #region EvaluateComponent (REFERENCE)
        /// <summary>
        ///Evaluation d'un composant REFERENCE (par évaluation (ou lecture) du composent référencé (hRef)
        /// </summary>
        /// <typeparam name="T">Type de la source</typeparam>
        /// <param name="pSource">Objet source</param>
        /// <param name="pComponent">Composant</param>
        /// <param name="pWorkingParameters">Classe paramètres (CS, dictionnaire des composants, gestion des messages retour, etc)</param>
        /// <returns>ErrLevel</returns>
        // EG 20210818 [XXXXX] Gestion de la valeur par défaut d'un composant en référence lorsque celui ci n'existe pas -->
        public static Cst.ErrLevel EvaluateComponent<T>(T pSource, ComponentReference pComponent, CAWorkingDataContainer pWorkingDataContainer)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            // On récupère le composant référencé
            IEnumerable<KeyValuePair<Pair<string, string>, ComponentBase>> _componentRef = pWorkingDataContainer.DicComponents.Where(elem => elem.Key.First == pComponent.href);
            if (0 < _componentRef.Count())
            {
                ComponentBase _component = _componentRef.First().Value;
                if (StrFunc.IsEmpty(_component.name))
                    _component.name = pComponent.name;
                else if (StrFunc.IsEmpty(pComponent.name))
                    pComponent.name = _component.name;

                // Evaluation
                if (false == _component.resultSpecified)
                    ret = CATools.EvaluateComponent(pComponent, _component, pWorkingDataContainer, pSource);
                else
                    ret = Cst.ErrLevel.SUCCESS;

                if (Cst.ErrLevel.SUCCESS == ret)
                    pComponent.SetResult(_component.result);
            }
            else if (pComponent.defaultResultSpecified)
            {
                ret = Cst.ErrLevel.SUCCESS;
                Nullable<decimal> defaultValue = Convert.ToDecimal(pComponent.defaultResult);
                pComponent.SetResult(defaultValue);
            }
            return ret;
        }
        #endregion EvaluateComponent (REFERENCE)
        #region EvaluateComponent (PROPERTY)
        /// <summary>
        /// Evaluation d'un composant PROPERTY
        /// La property à invoquée est soit:
        /// ● Déclarée et invoquée sur l'ASSEMBLY et la CLASSE définies pour le composant PROPERTY
        /// ● Déclarée et invoquée sur la source même dans le cas où l'ASSEMBLY et la CLASSE NE SONT PAS définies
        /// </summary>
        /// <typeparam name="T">Type de la source</typeparam>
        /// <param name="pSource">Objet source</param>
        /// <param name="pComponent">Composant</param>
        /// <param name="pWorkingDataContainer">Classe paramètres (CS, dictionnaire des composants, gestion des messages retour, etc)</param>
        /// <param name="pTarget">Source de la property</param>
        /// <returns>ErrLevel</returns>
        public static Cst.ErrLevel EvaluateComponent<T,T1>(T pSource, ComponentProperty pComponent, T1 pTarget)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                object _result = null;
                object _target = null;
                Type _tClass = null;

                if (StrFunc.IsFilled(pComponent.property.assembly) && StrFunc.IsFilled(pComponent.property.@class))
                {
                    // Récupération du nom de l'assembly et de la classe où chercher la méthode
                    _tClass = Type.GetType(pComponent.property.@class + "," + pComponent.property.assembly, true, false);
                    _target = pTarget;
                }
                else
                {
                    // Pas de nom d'assembly et de classe, la property est sur la source même
                    _tClass = pSource.GetType();
                    _target = pSource;
                }
                // Recherche de la property
                PropertyInfo _property = _tClass.GetProperty(pComponent.property.@value);
                if (null != _property)
                {
                    // Execution de la property
                    _result = _tClass.InvokeMember(_property.Name, BindingFlags.GetProperty, null, _target, null);
                    pComponent.SetResult(_result as Nullable<decimal>);
                    if (false == pComponent.Result.HasValue)
                        ret = Cst.ErrLevel.FAILURE;
                }
                else
                    ret = Cst.ErrLevel.FAILURE;
            }
            catch { ret = Cst.ErrLevel.FAILURE; }
            return ret;
        }
        #endregion EvaluateComponent (PROPERTY)
        #region EvaluateComponent (METHOD)
        /// <summary>
        /// Evaluation d'un composant METHOD
        /// La méthode à invoquée est soit:
        /// ● Déclarée et invoquée sur l'ASSEMBLY et la CLASSE définies pour le composant METHOD
        /// ● Déclarée et invoquée sur la source même dans le cas où l'ASSEMBLY et la CLASSE NE SONT PAS définies
        /// </summary>
        /// <typeparam name="T">Type de la source</typeparam>
        /// <param name="pSource">Objet source</param>
        /// <param name="pComponent">Composant</param>
        /// <param name="pWorkingParameters">Classe paramètres (CS, dictionnaire des composants, gestion des messages retour, etc)</param>
        /// <returns>ErrLevel</returns>
        public static Cst.ErrLevel EvaluateComponent<T,T1>(T pSource, ComponentMethod pComponent, CAWorkingDataContainer pWorkingDataContainer, T1 pTarget)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            // EG 20160404 Migration vs2013
            try
            {

                object _result = null;
                object _target = null;
                Type _tClass = null;

                if (StrFunc.IsFilled(pComponent.method.name.assembly) && StrFunc.IsFilled(pComponent.method.name.@class))
                {
                    // Récupération du nom de l'assembly et de la classe où chercher la méthode
                    _tClass = Type.GetType(pComponent.method.name.@class + "," + pComponent.method.name.assembly, true, false);
                    _target = pTarget;
                }
                else
                {
                    // Pas de nom d'assembly et de classe, la méthode est sur la source même
                    _tClass = pSource.GetType();
                    _target = pSource;
                }

                object[] _args = null;
                // EG 20160404 Migration vs2013
                string[] _sep = new string[] { "(",",",")" };
                string[] _methodInfo = pComponent.method.name.@value.Split(_sep,StringSplitOptions.RemoveEmptyEntries);
                // _methodInfo[0] : Nom de la méthode
                // _methodInfo[1.. n] : paramètres
                MethodInfo _method = _tClass.GetMethod(_methodInfo[0]);
                if (null != _method)
                {
                    if (1 < _methodInfo.Length)
                    {
                        _args = new object[_methodInfo.Length - 1];
                        for (int i = 0; i < _args.Length; i++)
                        {
                            IEnumerable<KeyValuePair<Pair<string, string>, ComponentBase>> _childComponents =
                                pWorkingDataContainer.DicComponents.Where(elem => elem.Key.First == _methodInfo[i + 1]);
                            if ((null != _childComponents) && (0 < _childComponents.Count()))
                            {
                                ComponentBase _par = _childComponents.First().Value;
                                _args[i] = null;
                                if (_par.resultSpecified)
                                    _args[i] = _par.ResultParameterMethod; 
                            }
                        }
                        
                    }
                    _result = _method.DeclaringType.InvokeMember(_method.Name, BindingFlags.InvokeMethod, null, _target, _args, null, null, null);
                    pComponent.SetResult(_result as Nullable<decimal>);
                    if (false == pComponent.Result.HasValue)
                        ret = Cst.ErrLevel.FAILURE;
                }
                else
                    ret = Cst.ErrLevel.FAILURE;
            }
            // EG 20160404 Migration vs2013
            catch (Exception) {ret = Cst.ErrLevel.FAILURE; }
            return ret;
        }
        #endregion EvaluateComponent  (METHOD)
        #region EvaluateComponent (FORMULE)
        /// <summary>
        /// Evaluation d'un composant FORMULA
        /// </summary>
        /// <typeparam name="T">Type de la source</typeparam>
        /// <param name="pSource">Objet source</param>
        /// <param name="pComponent">Composant</param>
        /// <param name="pWorkingParameters">Classe paramètres (CS, dictionnaire des composants, gestion des messages retour, etc)</param>
        /// <returns>ErrLevel</returns>
        public static Cst.ErrLevel EvaluateComponent<T,T1>(T pSource, ComponentFormula pComponent, CAWorkingDataContainer pWorkingDataContainer, T1 pTarget )
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            // Affichage de l'expression de la formule
            pWorkingDataContainer.SetMessageTitle(pComponent);
            // On récupèere dans un énumérateur l'ensemble des composants de la formule
            // le second élement de la clé (Pair) contient l'id 
            IEnumerable<KeyValuePair<Pair<string, string>, ComponentBase>> _childComponents =  pWorkingDataContainer.DicComponents.Where(elem => elem.Key.Second == pComponent.Id);

            pComponent.resultSpecified = false; // Force le recalcul
            if (true) //false == pComponent.resultSpecified)
            {
                #region Calcul de la formule
                ComponentBase _cs = null;

                string _mathExpression = pComponent.formula.mathExpression;
                // Tri des arguments de la formule par la taille de leur nom.
                List<string> lstArgs = _mathExpression.Split("/+-*()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                lstArgs = lstArgs.Distinct().OrderByDescending(_args => _args.Length).ToList();

                #region Traitement de composants de la formule
                foreach (KeyValuePair<Pair<string, string>, ComponentBase> _childComponent in _childComponents)
                {
                    // Evaluation d'un composent de la formule
                    if (Cst.ErrLevel.SUCCESS == CATools.EvaluateComponent(pSource, _childComponent.Value, pWorkingDataContainer, pTarget))
                    {
                        _cs = _childComponent.Value as ComponentBase;
                        if (null != _cs)
                        {
                            // EG 20140520 La valeur arrondi d'un composant si elle existe prévaut sur la valeur brute (ex : La valeur arrondi du RFActor doit être lue)
                            //Nullable<decimal> _value = _cs.Result;
                            Nullable<decimal> _value = _cs.ResultRounded;
                            if (false == _value.HasValue)
                                _value = _cs.Result;
                            if (_value.HasValue)
                            {
                                // Remplacement dans l'expression de la formule de toutes les références à ce composant 
                                // Component.name est remplacé par Component.result
                                lstArgs.ForEach(_args =>
                                {
                                    if ((_args != _cs.name) && _args.EndsWith(_cs.name))
                                        _mathExpression = _mathExpression.Replace(_args, (Convert.ToDecimal(_args.Replace(_cs.name, "")) * _value.Value).ToString());
                                    
                                });
                                _mathExpression = _mathExpression.Replace(_cs.name, _value.Value.ToString());
                            }
                        }
                    }
                    else
                        ret = Cst.ErrLevel.FAILURE;
                }
                #endregion Traitement de composants de la formule
                #region Traitement final de  l'évaluation
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    // Traitement des parenthèses et évaluation par bloc de la formule
                    ret = CATools.ParenthesisFormula(ref _mathExpression);
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        decimal _endResult = 0;
                        ret = ProcessOperation(_mathExpression, ref _endResult);
                        if (Cst.ErrLevel.SUCCESS == ret)
                        {
                            pComponent.resultSpecified = true;
                            pComponent.result = new Result
                            {
                                itemsElementName = ResultType.unit,
                                result = new SimpleUnit(_endResult)
                            };
                        }
                    }
                }
                #endregion Traitement final de  l'évaluation

                #endregion Calcul de la formule
            }
            return ret;
        }
        #endregion Calcul COMPOSANT FORMULE

        /* -------------------------------------------------------- */
        /* ----- FONCTIONS DE PARSING DE FORMULES              ----- */
        /* -------------------------------------------------------- */

        #region Operand
        /// <summary>
        /// Operateurs utilisés dans le parser de formules
        /// </summary>
        private static List<String> Operand
        {
            get
            {
                List<String> _operand = new List<string>
                {
                    "/",
                    "*",
                    "-",
                    "+"
                };
                return _operand;
            }
        }
        #endregion Operand

        #region ApplyOperand
        /// <summary>
        /// Calcul atomique entre deux nombres
        /// </summary>
        /// <param name="pValue1"></param>
        /// <param name="pValue2"></param>
        /// <param name="pOperand"></param>
        /// <returns></returns>
        private static Cst.ErrLevel ApplyOperand(decimal pValue1, decimal pValue2, string pOperand, ref decimal pResult)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                decimal _result = 0;
                if ("/" == pOperand)
                    _result = (pValue1 / pValue2);
                else if ("*" == pOperand)
                    _result = (pValue1 * pValue2);
                else if ("-" == pOperand)
                    _result = (pValue1 - pValue2);
                else if ("+" == pOperand)
                    _result = (pValue1 + pValue2);
                else
                    _result = 0;
                pResult = _result;
            }
            catch { ret = Cst.ErrLevel.FAILURE; }
            return ret;
        }
        #endregion ApplyOperand
        #region ParenthesisFormula
        /// <summary>
        /// Traitement des parenthèses de la formule
        /// </summary>
        /// <param name="pMathExpression"></param>
        /// <returns></returns>
        private static Cst.ErrLevel ParenthesisFormula(ref string pMathExpression)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                string _mathExpression = pMathExpression;
                // Position de la dernière parenthèse ouverte
                int _posLastOpenP = 0;
                // Position de la première parenthèse fermée après la dernière parenthèse ouverte
                int _posFirstCloseP = 0;
                List<String> _operand = Operand;

                while (_mathExpression.LastIndexOf("(") > -1)
                {
                    _posLastOpenP = _mathExpression.LastIndexOf("(");
                    _posFirstCloseP = _mathExpression.IndexOf(")", _posLastOpenP);
                    decimal _result = 0;
                    string _temp = string.Empty;
                    ret = ProcessOperation(_mathExpression.Substring(_posLastOpenP + 1, _posFirstCloseP - _posLastOpenP - 1), ref _result);
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        bool _addAsterix = false;
                        if (_posLastOpenP > 0)
                        {
                            _temp = _mathExpression.Substring(_posLastOpenP - 1, 1);
                            if (("(" != _temp) && (false == _operand.Contains(_temp)))
                                _addAsterix = true;
                        }
                        _mathExpression = _mathExpression.Substring(0, _posLastOpenP) + 
                            (_addAsterix ? "*" : "") + 
                            _result.ToString() +
                            _mathExpression.Substring(_posFirstCloseP + 1);
                    }
                    else
                    {
                        break;
                    }
                }
                if (Cst.ErrLevel.SUCCESS == ret)
                    pMathExpression = _mathExpression;
            }
            catch { ret = Cst.ErrLevel.FAILURE; }
            return ret;
        }
        #endregion ParenthesisFormula
        #region ProcessOperation
        /// <summary>
        /// Processus d'évaluation de la formule
        /// Découpage par type d'opérations (/,*,-,+) 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private static Cst.ErrLevel ProcessOperation(string pMathExpression, ref decimal pResult)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                string mathExpression = pMathExpression;
                List<String> _operand = Operand;
                ArrayList _blockExpression = new ArrayList();
                string _filler = string.Empty;

                // Construction de blocs (Equivalent à Split par opérateur)
                for (int i = 0; i < mathExpression.Length; i++)
                {
                    string _char = mathExpression.Substring(i, 1);
                    if (_operand.IndexOf(_char) > -1)
                    {
                        if (StrFunc.IsFilled(_filler))
                            _blockExpression.Add(_filler);
                        _blockExpression.Add(_char);
                        _filler = string.Empty;
                    }
                    else
                    {
                        _filler += _char;
                    }
                }
                _blockExpression.Add(_filler);

                foreach (string op in _operand)
                {
                    while (_blockExpression.IndexOf(op) > -1)
                    {
                        int _posOperand = _blockExpression.IndexOf(op);
                        decimal _result = 0;
                        decimal _valueBefore = 0;
                        decimal _valueAfter = 0;
                        if (_blockExpression[_posOperand + 1].ToString() == "-")
                        {
                            _blockExpression.RemoveAt(_posOperand + 1);
                            _valueAfter = Convert.ToDecimal(_blockExpression[_posOperand + 1]) * -1;
                        }
                        else
                        {
                            _valueAfter = Convert.ToDecimal(_blockExpression[_posOperand + 1]);
                        }

                        if (0 < _posOperand)
                        {
                            _valueBefore = Convert.ToDecimal(_blockExpression[_posOperand - 1]);
                            ret = ApplyOperand(_valueBefore, _valueAfter, op, ref _result);
                            if (Cst.ErrLevel.SUCCESS != ret)
                                break;
                            _blockExpression[_posOperand] = _result;
                            _blockExpression.RemoveAt(_posOperand - 1);
                        }
                        else
                        {
                            ret = ApplyOperand(_valueBefore, _valueAfter, op, ref _result);
                            if (Cst.ErrLevel.SUCCESS != ret)
                                break;
                            _blockExpression[_posOperand + 1] = _result;

                        }
                        _blockExpression.RemoveAt(_posOperand);
                    }
                    if (Cst.ErrLevel.SUCCESS != ret)
                        break;
                }
                if (Cst.ErrLevel.SUCCESS == ret)
                    pResult = Convert.ToDecimal(_blockExpression[0]);
            }
            catch { ret = Cst.ErrLevel.FAILURE; }
            return ret;
        }
        #endregion ProcessOperation

        /* -------------------------------------------------------- */
        /* ----- ENUMERATEURS                                 ----- */
        /* -------------------------------------------------------- */

        #region CAWhereMode
        /// <summary>
        /// Type de recherche utilisée pour lecture d'une CA dans les tables 
        /// ID = Recherche par IDCA / IDCAISSUE
        /// NOTICE = Recherche par IDM et NOTICE / CAMARKET et NOTICE
        /// </summary>
        public enum CAWhereMode
        {
            EFFECTIVEDATE,
            ID,
            NOTICE,
        }
        #endregion CAWhereMode
        #region DCWhereMode
        /// <summary>
        /// Type de recherche utilisée pour lecture d'un DC dans la table CORPOEVENTCONTRACT
        /// ID = Recherche par IDCEC
        /// IDDC = Recherche par IDCE, IDA_ENTITY, IDDC
        /// </summary>
        public enum DCWhereMode
        {
            ID,
            IDCE,
            IDCE_READYSTATE,
            IDDC,
        }
        #endregion DCWhereMode

        #region CAMarketType
        //public static Nullable<Cst.MarketTypeEnum> CAMarketType(string pValue)
        //{
        //    Nullable<Cst.MarketTypeEnum> _marketType = null;
        //    if (System.Enum.IsDefined(typeof(Cst.MarketTypeEnum), pValue))
        //        _marketType = (Cst.MarketTypeEnum)System.Enum.Parse(typeof(Cst.MarketTypeEnum), pValue, false);
        //    return _marketType;
        //}
        // PL 20171006 [23469] Original MARKETTYPE deprecated
        public static Cst.MarketTypeEnum CAMarketType(Nullable<int> pValue)
        {
            Cst.MarketTypeEnum _marketType = (pValue.HasValue && (pValue != 0) ? Cst.MarketTypeEnum.SEGMENT : Cst.MarketTypeEnum.OPERATING);
            return _marketType;
        }
        #endregion CAMarketType

        #region CADocType
        public static Nullable<CATools.DOCTypeEnum> CADocType(string pValue)
        {
            Nullable<CATools.DOCTypeEnum> _docType = (Nullable<CATools.DOCTypeEnum>)ReflectionTools.EnumParse(new CATools.DOCTypeEnum(), pValue);
            return _docType;
        }
        #endregion CADocType
        #region CADocRunTime
        public static Nullable<CATools.SQLRunTimeEnum> CADocRunTime(string pValue)
        {
            Nullable<CATools.SQLRunTimeEnum> _docRunTime = (Nullable<CATools.SQLRunTimeEnum>)ReflectionTools.EnumParse(new CATools.SQLRunTimeEnum(), pValue);
            return _docRunTime;
        }
        #endregion CADocRunTime


        #region CAReadyState
        public static Nullable<CorporateActionReadyStateEnum> CAReadyState(string pValue)
        {
            Nullable<CorporateActionReadyStateEnum> _readyState = null;
            if (System.Enum.IsDefined(typeof(CorporateActionReadyStateEnum), pValue))
                _readyState = (CorporateActionReadyStateEnum)System.Enum.Parse(typeof(CorporateActionReadyStateEnum), pValue, false);
            return _readyState;
        }
        #endregion CAReadyState
        #region CAEmbeddedState
        public static Nullable<CorporateActionEmbeddedStateEnum> CAEmbeddedState(string pValue)
        {
            Nullable<CorporateActionEmbeddedStateEnum> _embeddedState = null;
            if (System.Enum.IsDefined(typeof(CorporateActionEmbeddedStateEnum), pValue))
                _embeddedState = (CorporateActionEmbeddedStateEnum)System.Enum.Parse(typeof(CorporateActionEmbeddedStateEnum), pValue, false);
            return _embeddedState;
        }
        #endregion CAEmbeddedState
        #region CEGRoup
        public static Nullable<CorporateEventGroupEnum> CEGroup(string pValue)
        {
            Nullable<CorporateEventGroupEnum> _group = null;
            if (System.Enum.IsDefined(typeof(CorporateEventGroupEnum), pValue))
                _group = (CorporateEventGroupEnum)System.Enum.Parse(typeof(CorporateEventGroupEnum), pValue, false);
            return _group;
        }
        #endregion CEGRoup
        #region CEType
        public static Nullable<CorporateEventTypeEnum> CEType(string pValue)
        {
            Nullable<CorporateEventTypeEnum> _type = null;
            if (System.Enum.IsDefined(typeof(CorporateEventTypeEnum), pValue))
                _type = (CorporateEventTypeEnum)System.Enum.Parse(typeof(CorporateEventTypeEnum), pValue, false);
            return _type;
        }
        #endregion CEType
        #region CACfiCodeCategory
        public static Nullable<CfiCodeCategoryEnum> CACfiCodeCategory(string pValue)
        {
            Nullable<CfiCodeCategoryEnum> _cfiCodeCategory = (Nullable<CfiCodeCategoryEnum>)ReflectionTools.EnumParse(new CfiCodeCategoryEnum(), pValue);
            return _cfiCodeCategory;
        }
        #endregion CACfiCodeCategory
        #region CEOperand
        public static Nullable<CombinationOperandEnum> CEOperand(string pValue)
        {
            Nullable<CombinationOperandEnum> _oper = null;
            if (System.Enum.IsDefined(typeof(CombinationOperandEnum), pValue))
                _oper = (CombinationOperandEnum)System.Enum.Parse(typeof(CombinationOperandEnum), pValue, false);
            return _oper;
        }
        #endregion CEOperand
        #region CEMethod
        public static Nullable<AdjustmentMethodOfDerivContractEnum> CEMethod(string pValue)
        {
            Nullable<AdjustmentMethodOfDerivContractEnum> _method = null;
            if (System.Enum.IsDefined(typeof(AdjustmentMethodOfDerivContractEnum), pValue))
                _method = (AdjustmentMethodOfDerivContractEnum)System.Enum.Parse(typeof(AdjustmentMethodOfDerivContractEnum), pValue, false);
            return _method;
        }
        #endregion CEMethod
        #region CEMode
        public static Nullable<FixML.Enum.SettlSessIDEnum> CEMode(string pValue)
        {
            Nullable<FixML.Enum.SettlSessIDEnum> _mode = (Nullable<FixML.Enum.SettlSessIDEnum>)ReflectionTools.EnumParse(new FixML.Enum.SettlSessIDEnum(), pValue);
            return _mode;
        }
        #endregion CEMode
        #region IsCEGroupCombination
        public static bool IsCEGroupCombination(string pValue)
        {
            Nullable<CorporateEventGroupEnum> _group = CATools.CEGroup(pValue);
            return _group.HasValue && (_group.Value == CorporateEventGroupEnum.Combination);
        }
        #endregion IsCEGroupCombination
        #region CERenamingContractMethod
        public static CorpoEventRenamingContractMethodEnum CERenamingContractMethod(string pValue)
        {
            CorpoEventRenamingContractMethodEnum _method = CorpoEventRenamingContractMethodEnum.None;
            if (System.Enum.IsDefined(typeof(CorpoEventRenamingContractMethodEnum), pValue))
                _method = (CorpoEventRenamingContractMethodEnum)System.Enum.Parse(typeof(CorpoEventRenamingContractMethodEnum), pValue, false);
            return _method;
        }
        #endregion CERenamingContractMethod
        #region CERoundingDirection
        public static Cst.RoundingDirectionSQL CERoundingDirection(string pValue)
        {
            Cst.RoundingDirectionSQL _dir = Cst.RoundingDirectionSQL.N;
            if (System.Enum.IsDefined(typeof(Cst.RoundingDirectionSQL), pValue))
                _dir = (Cst.RoundingDirectionSQL)System.Enum.Parse(typeof(Cst.RoundingDirectionSQL), pValue, false);
            return _dir;
        }
        #endregion CERoundingDirection



        /* -------------------------------------------------------- */
        /* ----- CHECKEURS                                    ----- */
        /* -------------------------------------------------------- */

        #region RefMarketNoticeChecker
        /// <summary>
        /// Contrôle la validité d'une référence de notice/circulaire de Corporate Actions
        /// Les "Regular expressions" sont dépendantes du marché
        /// <example>
        /// ● EUREX
        /// = une numéro de notice + "/" suivi de l'année sur 4 digits 
        /// exemple: 1478/2013
        /// ● EURONEXT 
        /// = "CA" + "/" + ANNEE (4 digits) + "/" + N° NOTICE + "/" + ABBREV. PLACE (Amsterdam, Bruxelles, Lisbonne, Londres et Paris)
        /// exemple: CA/2013/231/Lo ou CA/2013/211/P
        /// ● EUREX 
        /// = "Info" + "_" + N° sur 3 digits (001,002,etc) + "_" + Code
        /// = Compteur sur 3 chiffres (CA en plusieurs étapes) + "_" + 
        /// ● IDEM 
        /// = N° NOTICE (8478)
        /// ● Autres
        /// Open, pas de Regular expressions restricitives
        /// </example>
        /// <code>Exemple</code>
        /// </summary>
        /// <param name="pMarketAcronym"></param>
        /// <param name="pRefNotice"></param>
        /// <returns></returns>
        /// FI 20160926 [XXXXX] Modify
        public static bool RefMarketNoticeChecker(string pMarketAcronym, string pRefNotice)
        {
            Regex REFNOTICE_PATTERN;
            switch (pMarketAcronym)
            {
                case "EURONEXT":
                    // EG 20140423 CA en plusieurs étapes
                    REFNOTICE_PATTERN = new Regex(@"^CA/\d{4}/\d+/(A|B|Li|Lo|P)(-\d{1})?$");
                    break;
                case "EUREX":
                    // = une référence de notice + "/" suivi de l'année sur 4 digits (1478/2013)    
                    // EG 20140127 CA en plusieurs étapes
                    //REFNOTICE_PATTERN = new Regex(@"^\d+/\d{4}$");
                    //REFNOTICE_PATTERN = new Regex(@"^\d+/\d{4}(-\d{1})?$");
                    // FI 20160926 [XXXXX] => Nouvelle regex
                    REFNOTICE_PATTERN = new Regex(@"^Info_\d{3}_[a-zA-Z]+$");
                    break;
                case "IDEM":
                    REFNOTICE_PATTERN = new Regex(@"^\d+(-)?\d+$");
                    break;
                default:
                    REFNOTICE_PATTERN = new Regex(".");
                    break;
            }
            bool isValid = StrFunc.IsFilled(pRefNotice);
            if (isValid)
            {
                isValid = REFNOTICE_PATTERN.IsMatch(pRefNotice);
            }
            return isValid;
        }
        #endregion RefMarketNoticeChecker
        #region ISINCodeChecker
        /// <summary>
        /// Contrôle la validité d'une saisie d'un CODE ISIN
        /// </summary>
        /// <param name="pISINCode">Code ISIN à valider</param>
        /// <returns></returns>
        public static bool ISINCodeChecker(string pISINCode)
        {
            Regex ISIN_PATTERN = new Regex(@"[A-Z]{2}([A-Z0-9]){9}[0-9]");
            if (null == pISINCode)
                return false;
            if (false == ISIN_PATTERN.IsMatch(pISINCode))
                return false;
 
            int[] digits = new int[22];
            int index = 0;
            for (int i = 0; i < 11; i++)
            {
                char c = pISINCode[i];
                if (c >= '0' && c <= '9')
                {
                    digits[index++] = c - '0';
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    int n = c - 'A' + 10;
                    int tens = n / 10;
                    if (tens != 0)
                    {
                        digits[index++] = tens;
                    }
                    digits[index++] = n % 10;
                }
                else
                {
                    // Not a digit or upper-case letter.
                    return false;
                }
            }
            int sum = 0;
            for (int i = 0; i < index; i++)
            {
                int digit = digits[index - 1 - i];
                if (i % 2 == 0)
                {
                    digit *= 2;
                }
                sum += digit / 10;
                sum += digit % 10;
            }

            int checkDigit = pISINCode[11] - '0';
            int tensComplement = (sum % 10 == 0) ? 0 : ((sum / 10) + 1) * 10 - sum;
            return (checkDigit == tensComplement);
        }
        #endregion ISINCodeChecker


        /* -------------------------------------------------------- */
        /* ----- MARKETRULES                                  ----- */
        /* -------------------------------------------------------- */

        #region MKTRulesQuery
        // EG 20140317 [19722]
        public class MKTRulesQuery
        {
            #region Members
            private DataParameter paramIdM;
            private readonly string _CS;
            #endregion Members
            #region Constructor
            public MKTRulesQuery(string pCS)
            {
                _CS = pCS;
                InitParameter();
            }
            #endregion
            #region Methods
            #region GetQueryExist
            public QueryParameters GetQueryExist()
            {
                DataParameters parameters = new DataParameters();
                StrBuilder sqlQuery = new StrBuilder();
                sqlQuery += SQLCst.SELECT + "1" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENTMKTRULES.ToString() + " mkr" + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "(mkr.IDM = @ID)" + Cst.CrLf;
                parameters.Add(paramIdM);
                QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
                return ret;
            }
            #endregion GetQueryExist
            #region GetQuerySelect
            public QueryParameters GetQuerySelect()
            {
                return GetQuerySelect(null);
            }
            // EG 20140317 [19722]
            public QueryParameters GetQuerySelect(Nullable<int> pIdM)
            {
                DataParameters parameters = new DataParameters();

                StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT + " mkr.IDM, ");
                sqlQuery += "mkr.RFACTOR_RNDDIR, mkr.RFACTOR_ISURNDDIR, mkr.RFACTOR_RNDPREC, mkr.RFACTOR_ISURNDPREC, " + Cst.CrLf;
                sqlQuery += "mkr.CMUL_RNDDIR, mkr.CMUL_ISURNDDIR, mkr.CMUL_RNDPREC, mkr.CMUL_ISURNDPREC, " + Cst.CrLf;
                sqlQuery += "mkr.CSIZE_RNDDIR, mkr.CSIZE_ISURNDDIR, mkr.CSIZE_RNDPREC, mkr.CSIZE_ISURNDPREC, " + Cst.CrLf;
                sqlQuery += "mkr.STRIKE_RNDDIR, mkr.STRIKE_ISURNDDIR, mkr.STRIKE_RNDPREC, mkr.STRIKE_ISURNDPREC, " + Cst.CrLf;
                sqlQuery += "mkr.PRICE_RNDDIR, mkr.PRICE_ISURNDDIR, mkr.PRICE_RNDPREC, mkr.PRICE_ISURNDPREC, " + Cst.CrLf;
                // PM 20170911 [23408] Le paramétrage des codes événements n'est plus possible
                //sqlQuery += "mkr.EQP_FUTISAUTHORIZED, mkr.EQP_OPTISAUTHORIZED, mkr.EQP_FUTEVENTCODE, mkr.EQP_FUTEVENTTYPE, mkr.EQP_OPTEVENTCODE, mkr.EQP_OPTEVENTTYPE, " + Cst.CrLf;
                sqlQuery += "mkr.EQP_FUTISAUTHORIZED, mkr.EQP_OPTISAUTHORIZED, " + Cst.CrLf;
                sqlQuery += DataHelper.SQLIsNull(_CS, "mkr.EQPAYMENT_RNDDIR", DataHelper.SQLString(Cst.RoundingDirectionSQL.N.ToString()), "EQPAYMENT_RNDDIR");
                sqlQuery += ", mkr.EQPAYMENT_ISURNDDIR, " + DataHelper.SQLIsNull(_CS, "mkr.EQPAYMENT_RNDPREC", "2", "EQPAYMENT_RNDPREC") + Cst.CrLf;
                sqlQuery += ", mkr.EQPAYMENT_ISURNDPREC, mkr.RENAMINGMETHOD, mkr.RENAMINGCATEGORY" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENTMKTRULES.ToString() + " mkr" + Cst.CrLf;
                if (pIdM.HasValue)
                {
                    parameters.Add(paramIdM);
                    paramIdM.Value = pIdM.Value;
                    sqlQuery += SQLCst.WHERE + "(mkr.IDM = @IDM)" + Cst.CrLf;
                }
                QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
                return ret;
            }
            #endregion GetQuerySelect
            #region InitParameter
            /// <summary>
            /// Initialisation des paramètres
            /// </summary>
            private void InitParameter()
            {
                paramIdM = new DataParameter(_CS, "IDM", DbType.AnsiString, 4);
            }
            #endregion
            #endregion Methods
        }
        #endregion MKTRulesQuery

        #region DefaultMktRoundingRules
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<Pair<AdjustmentElementEnum, Rounding>> DefaultMktRoundingRules()
        {
            List<Pair<AdjustmentElementEnum, Rounding>> _lst = new List<Pair<AdjustmentElementEnum, Rounding>>();
            Pair<AdjustmentElementEnum, Rounding> _pair =
                new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.RFactor, new Rounding(Cst.RoundingDirectionSQL.N, 8));
            _lst.Add(_pair);
            _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.ContractMultiplier, new Rounding(Cst.RoundingDirectionSQL.N, 0));
            _lst.Add(_pair);
            _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.ContractSize, new Rounding(Cst.RoundingDirectionSQL.N, 0));
            _lst.Add(_pair);
            _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.Price, new Rounding(Cst.RoundingDirectionSQL.N, 4));
            _lst.Add(_pair);
            _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.StrikePrice, new Rounding(Cst.RoundingDirectionSQL.N, 4));
            _lst.Add(_pair);
            _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.EqualisationPayment, new Rounding(Cst.RoundingDirectionSQL.N, 2));
            _lst.Add(_pair);
            return _lst;
        }
        #endregion DefaultMktRoundingRules

        #region GetMktRules
        // EG 20140317 [19722]
        /// EG 20141106 [20253] Equalisation payment
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        // EG 20180426 Analyse du code Correction [CA2202]
        public static CorporateEventMktRules GetMktRules(string pCS, IDbTransaction pDbTransaction, int pIdM)
        {
            CorporateEventMktRules _mktRules = new CorporateEventMktRules
            {
                idM = pIdM,
                renamingContractMethod = CorpoEventRenamingContractMethodEnum.None
            };
            List<Pair<AdjustmentElementEnum, Rounding>> _lstMarketRules = DefaultMktRoundingRules();

            MKTRulesQuery _mktQry = new MKTRulesQuery(pCS);
            QueryParameters qryParameters = _mktQry.GetQuerySelect(pIdM);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    _mktRules.renamingContractMethod = CATools.CERenamingContractMethod(dr["RENAMINGMETHOD"].ToString());
                    _mktRules.renamingCategorySpecified = (false == Convert.IsDBNull(dr["RENAMINGCATEGORY"]));
                    if (_mktRules.renamingCategorySpecified)
                        _mktRules.renamingCategory = CATools.CACfiCodeCategory(dr["RENAMINGCATEGORY"].ToString()).Value;

                    _mktRules.isEqualPaymentFutureAuthorized = Convert.ToBoolean(dr["EQP_FUTISAUTHORIZED"]);
                    // PM 20170911 [23408] Le paramétrage des codes événements n'est plus possible
                    //if (_mktRules.isEqualPaymentFutureAuthorized)
                    //{
                    //    _mktRules.equalPaymentFutureEventCodeSpecified = (false == Convert.IsDBNull(dr["EQP_FUTEVENTCODE"]));
                    //    if (_mktRules.equalPaymentFutureEventCodeSpecified)
                    //        _mktRules.equalPaymentFutureEventCode = Convert.ToString(dr["EQP_FUTEVENTCODE"]);
                    //    _mktRules.equalPaymentFutureEventTypeSpecified = (false == Convert.IsDBNull(dr["EQP_FUTEVENTTYPE"]));
                    //    if (_mktRules.equalPaymentFutureEventTypeSpecified)
                    //        _mktRules.equalPaymentFutureEventType = Convert.ToString(dr["EQP_FUTEVENTTYPE"]);
                    //}
                    _mktRules.isEqualPaymentOptionAuthorized = Convert.ToBoolean(dr["EQP_OPTISAUTHORIZED"]);
                    // PM 20170911 [23408] Le paramétrage des codes événements n'est plus possible
                    //if (_mktRules.isEqualPaymentOptionAuthorized)
                    //{
                    //    _mktRules.equalPaymentOptionEventCodeSpecified = (false == Convert.IsDBNull(dr["EQP_OPTEVENTCODE"]));
                    //    if (_mktRules.equalPaymentOptionEventCodeSpecified)
                    //        _mktRules.equalPaymentOptionEventCode = Convert.ToString(dr["EQP_OPTEVENTCODE"]);
                    //    _mktRules.equalPaymentOptionEventTypeSpecified = (false == Convert.IsDBNull(dr["EQP_OPTEVENTTYPE"]));
                    //    if (_mktRules.equalPaymentOptionEventTypeSpecified)
                    //        _mktRules.equalPaymentOptionEventType = Convert.ToString(dr["EQP_OPTEVENTTYPE"]);
                    //}

                    Rounding _rounding = new Rounding(Convert.ToString(dr["RFACTOR_RNDDIR"]), Convert.ToBoolean(dr["RFACTOR_ISURNDDIR"]),
                        Convert.ToInt32(dr["RFACTOR_RNDPREC"]), Convert.ToBoolean(dr["RFACTOR_ISURNDPREC"]));
                    _lstMarketRules.Find(match => match.First == AdjustmentElementEnum.RFactor).Second = _rounding;

                    _rounding = new Rounding(Convert.ToString(dr["CMUL_RNDDIR"]), Convert.ToBoolean(dr["CMUL_ISURNDDIR"]),
                        Convert.ToInt32(dr["CMUL_RNDPREC"]), Convert.ToBoolean(dr["CMUL_ISURNDPREC"]));
                    _lstMarketRules.Find(match => match.First == AdjustmentElementEnum.ContractMultiplier).Second = _rounding;

                    _rounding = new Rounding(Convert.ToString(dr["CSIZE_RNDDIR"]), Convert.ToBoolean(dr["CSIZE_ISURNDDIR"]),
                        Convert.ToInt32(dr["CSIZE_RNDPREC"]), Convert.ToBoolean(dr["CSIZE_ISURNDPREC"]));
                    _lstMarketRules.Find(match => match.First == AdjustmentElementEnum.ContractSize).Second = _rounding;

                    _rounding = new Rounding(Convert.ToString(dr["STRIKE_RNDDIR"]), Convert.ToBoolean(dr["STRIKE_ISURNDDIR"]),
                        Convert.ToInt32(dr["STRIKE_RNDPREC"]), Convert.ToBoolean(dr["STRIKE_ISURNDPREC"]));
                    _lstMarketRules.Find(match => match.First == AdjustmentElementEnum.StrikePrice).Second = _rounding;

                    _rounding = new Rounding(Convert.ToString(dr["PRICE_RNDDIR"]), Convert.ToBoolean(dr["PRICE_ISURNDDIR"]),
                        Convert.ToInt32(dr["PRICE_RNDPREC"]), Convert.ToBoolean(dr["PRICE_ISURNDPREC"]));
                    _lstMarketRules.Find(match => match.First == AdjustmentElementEnum.Price).Second = _rounding;
                }
            }
            _mktRules.rounding = _lstMarketRules;
            return _mktRules;
        }
        #endregion GetMarketRules

        #region ExRenaming
        // EG 20130716
        /// <summary>
        /// Calcul la version du contrat ajusté suite à CA en fonction
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pContract"></param>
        /// EG [33415/33420] Gestion exData optionnelles
        /// EG 20140317 [17922]
        public static void ExRenaming(CorporateEventMktRules pMktRules, CorporateEventContract pContract)
        {
            pContract.renamingMethod = pMktRules.renamingContractMethod;

            bool isIncrementalRenaming = (false == pMktRules.renamingCategorySpecified) || (pContract.category == pMktRules.renamingCategory);

            int _version;
            switch (pMktRules.renamingContractMethod)
            {
                case CorpoEventRenamingContractMethodEnum.ContractAttribute:
                    pContract.renamingMethod = CorpoEventRenamingContractMethodEnum.ContractAttribute;
                    pContract.cumData.renamingValueSpecified = true;
                    pContract.cumData.renamingValue = pContract.contractAttribute;
                    // EG [33415/33420]
                    if (pContract.exDataSpecified)
                    {
                        pContract.exData.renamingValueSpecified = true;
                        _version = Convert.ToInt32(pContract.contractAttribute);
                        if (isIncrementalRenaming)
                            _version++;
                        pContract.exData.renamingValue = _version.ToString();
                    }
                    break;
                case CorpoEventRenamingContractMethodEnum.SymbolSuffix:
                    string suffixPattern = @"\d+$";
                    Match match = Regex.Match(pContract.contractSymbol, suffixPattern);
                    pContract.cumData.renamingValueSpecified = true;
                    pContract.cumData.renamingValue = pContract.contractSymbol;
                    // EG [33415/33420]
                    if (pContract.exDataSpecified)
                    {
                        pContract.exData.renamingValueSpecified = true;
                        pContract.exData.renamingValue = pContract.contractSymbol + "1";
                        if (match.Success)
                        {
                            _version = Convert.ToInt32(match.Groups[0].Value);
                            if (isIncrementalRenaming)
                                _version++;
                            pContract.exData.renamingValue = pContract.contractSymbol.Replace(match.Groups[0].Value, _version.ToString());
                        }
                    }
                    break;
                default:
                    pContract.cumData.renamingValueSpecified = false;
                    // EG [33415/33420]
                    if (pContract.exDataSpecified)
                        pContract.exData.renamingValueSpecified = false;
                    break;
            }
        }
        #endregion ExRenaming

        #region RenamingNextVersion
        /// <summary>
        /// Donne le numéro de version du Contract (via Contract symbol suffix ou ContractAttribute
        /// </summary>
        /// <param name="pContract"></param>
        /// <returns></returns>
        /// EG 20130716 New
        /// EG 20140317 [17922] 
        //public static int RenamingNextVersion(CorporateEventMktRules pMktRules, CorporateEventContract pContract)
        public static int RenamingNextVersion(CorporateEventMktRules pCorporateEventMktRules, CorporateEventContract pContract)
        {
            int _version = 0;
            // Il y a insertion d'un DC Ex si pas de spécification de règle sur la catégory ou la catégorie du contrat est paramétrée.
            if ((false == pCorporateEventMktRules.renamingCategorySpecified) ||
                 (pCorporateEventMktRules.renamingCategorySpecified && (pContract.category == pCorporateEventMktRules.renamingCategory)))
            {
                _version = 1;

                switch (pContract.renamingMethod)
                {
                    case CorpoEventRenamingContractMethodEnum.ContractAttribute:
                        _version = Convert.ToInt32(pContract.contractAttribute) + 1;
                        break;
                    case CorpoEventRenamingContractMethodEnum.SymbolSuffix:
                        string suffixPattern = @"\d+$";
                        Match match = Regex.Match(pContract.contractSymbol, suffixPattern);
                        if (match.Success)
                            _version = Convert.ToInt32(match.Groups[0].Value) + 1;
                        break;
                }
            }
            return _version;
        }
        #endregion RenamingVersion

        /* -------------------------------------------------------- */
        /* ----- LE DC ADJ DOIT-IL ÊTRE CREE ?                ----- */
        /* -------------------------------------------------------- */

        #region IsNewDCForExAdj
        /// <summary>
        /// Determine s'il faut créer un DEX ExAdj sur la CA sur le contrat CUM
        /// </summary>
        /// <param name="pContrat"></param>
        /// <param name="pAdjustment"></param>
        /// <param name="pAdjMethod"></param>
        /// <param name="pRenamingContractMethod"></param>
        /// <param name="pListExContractSymbol"></param>
        /// <param name="pListExAdjContractSymbol"></param>
        /// <returns></returns>
        // EG 20141010 [XXXXX] Test sur Génération DC sur ajusté
        public static bool IsNewDCForExAdj(CorporateEventContract pContrat, Adjustment pAdjustment, CorpoEventRenamingContractMethodEnum pRenamingContractMethod)
        {
            string cumContractSymbol = pContrat.contractSymbol;
            string exContractSymbol = pAdjustment.GetStringAIContractValue(CATools.CAElementTypeEnum.Ex, CATools.CAElementEnum.sym, pContrat);
            string exAdjContractSymbol = pAdjustment.GetStringAIContractValue(CATools.CAElementTypeEnum.ExAdj, CATools.CAElementEnum.sym, pContrat);
            if (StrFunc.IsEmpty(exContractSymbol))
                exContractSymbol = pContrat.contractSymbol;
            //if (StrFunc.IsEmpty(exAdjContractSymbol))
            //    exAdjContractSymbol = pContrat.contractSymbol;

            bool isDCExADj = true;

            if (pRenamingContractMethod == CorpoEventRenamingContractMethodEnum.None)
            {
                // Normalement NO RENAMING sur le marché 
                // Exemple XUSS (Google) : Cum = GOOG Ex = GOOGL et Exadj = GOLG1

                if (StrFunc.IsFilled(exAdjContractSymbol) && (exContractSymbol != exAdjContractSymbol) && (exContractSymbol != cumContractSymbol))
                    isDCExADj = true;
                else
                    isDCExADj = false;
            }
            else
            {
                if (StrFunc.IsEmpty(exAdjContractSymbol))
                    exAdjContractSymbol = pContrat.contractSymbol;

                // Normalement RENAMING  sur le marché
                // Exemple XDMI (FIAT) : Cum = F Ex = FCA et Exadj = FCA (Pas de DC ExAdj créé les ASSETS ExAdj seront portés par DCEx)
                if ((exContractSymbol == exAdjContractSymbol) && (exContractSymbol != cumContractSymbol))
                    isDCExADj = false;
            }
            return isDCExADj;
        }
        #endregion RenamingVersion

    }


    #region CAEqualisationPaymentEvent
    /// <summary>
    /// Class de stockage du détail d'un événement Equalisation Payment
    /// </summary>
    // PM 20170911 [23408] New
    public class CAEqualisationPaymentEvent
    {
        #region Members
        private readonly int? m_IdA_Payer = null;
        private readonly int? m_IdB_Payer = null;
        private readonly int? m_IdA_Receiver = null;
        private readonly int? m_IdB_Receiver = null;
        private readonly decimal m_Amount;
        private readonly string m_IdC;
        private readonly DateTime m_DtEffectiveDate;
        private readonly DateTime m_DtSettlement;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Identifier interne du Payer
        /// </summary>
        public int? IdA_Payer
        { get { return m_IdA_Payer; } }
        /// <summary>
        /// Identifier interne du book du Payer
        /// </summary>
        public int? IdB_Payer
        { get { return m_IdB_Payer; } }
        /// <summary>
        /// Identifier interne du Receiver
        /// </summary>
        public int? IdA_Receiver
        { get { return m_IdA_Receiver; } }
        /// <summary>
        /// Identifier interne du book du Receiver
        /// </summary>
        public int? IdB_Receiver
        { get { return m_IdB_Receiver; } }
        /// <summary>
        /// Montant de l'Equalisation Payment
        /// </summary>
        public decimal Amount
        { get { return m_Amount; } }
        /// <summary>
        /// Devise de l'Equalisation Payment
        /// </summary>
        public string IdC
        { get { return m_IdC; } }
        /// <summary>
        /// Effective date de l'Equalisation Payment
        /// </summary>
        public DateTime DtEffectiveDate
        { get { return m_DtEffectiveDate; } }
        /// <summary>
        /// Settlement date de l'Equalisation Payment
        /// </summary>
        public DateTime DtSettlement
        { get { return m_DtSettlement; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdA_Payer"></param>
        /// <param name="pIdB_Payer"></param>
        /// <param name="pIdA_Receiver"></param>
        /// <param name="pIdB_Receiver"></param>
        /// <param name="pDtEffectiveDate"></param>
        /// <param name="pDtSettlement"></param>
        /// <param name="pAmount"></param>
        /// <param name="pIdC"></param>
        public CAEqualisationPaymentEvent(int? pIdA_Payer, int? pIdB_Payer, int? pIdA_Receiver, int? pIdB_Receiver,
            DateTime pDtEffectiveDate, DateTime pDtSettlement, decimal pAmount, string pIdC)
        {
            m_IdA_Payer = pIdA_Payer;
            m_IdB_Payer = pIdB_Payer;
            m_IdA_Receiver = pIdA_Receiver;
            m_IdB_Receiver = pIdB_Receiver;
            m_DtEffectiveDate = pDtEffectiveDate;
            m_DtSettlement = pDtSettlement;
            m_Amount = pAmount;
            m_IdC = pIdC;
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion CAEqualisationPaymentEvent

    #region CATradeEx
    /// <summary>
    /// Classe identifier les Trades Ex créées
    /// </summary>
    /// PM 20170911 [23408] New
    public class CATradeEx
    {
        #region Members
        private readonly int m_IdT;
        private readonly string m_Identifier;
        private readonly CAEqualisationPaymentEvent m_EqualisationPayment;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Identifier interne du trade Ex
        /// </summary>
        public int IdT
        { get { return m_IdT; } }
        /// <summary>
        /// Identifier du trade Ex
        /// </summary>
        public string Identifier
        { get { return m_Identifier; } }
        /// <summary>
        /// Données de l'Equalisation Payment
        /// </summary>
        public CAEqualisationPaymentEvent EqualisationPayment
        { get { return m_EqualisationPayment; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        public CATradeEx(int pIdT, string pIdentifier)
            : this(pIdT, pIdentifier, default) {}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pEqualisationPayment"></param>
        public CATradeEx( int pIdT, string pIdentifier, CAEqualisationPaymentEvent pEqualisationPayment)
        {
            m_IdT = pIdT;
            m_Identifier = pIdentifier;
            m_EqualisationPayment = pEqualisationPayment;
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion CATradeEx
}








