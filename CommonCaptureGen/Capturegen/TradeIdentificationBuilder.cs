#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Xml;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Classe chargée de générer les identifiants displayName(identifier,displayname,etc..) 
    /// </summary>
    ///20091016 FI [Rebuild identification] nouvelle classe
    public class TradeIdentificationBuilder
    {
        #region const
        public const string ConstDefault = "{default}";
        #endregion Members
        //
        #region Members
        private readonly TradeRecordSettings _recordSettings;
        /// <summary>
        /// Définit si l'identifier d'un trade est à calculer via l'utilisation de GetId ou non.
        ///<para>Exemple: true pour les "Trades de marchés" (eg. DebSecurityTransaction, Swap...), false pour les "Titres" (eg. DebSecurity)</para>
        /// </summary>
        //private bool _isUseGetIdForIdentifier;
        /// <summary>
        /// Définit les valeurs initiales, toute valeur initiale non vide et différente de {default} n'est pas écrasée 
        /// </summary>
        private readonly SpheresIdentification _initialIdentification;
        /// <summary>
        /// Trade courant utilisé pour générer les identifiants
        /// </summary>
        private readonly TradeCommonInput _tradeInput;
        /// <summary>
        /// retourne true si le XSL de transformation a été trouvé lors du dernier Appel à BuildIdentification
        /// </summary>
        private bool _isXslFind;
        #endregion
        //
        #region property
        /// <summary>
        /// retourne true si le XSL de transformation a été trouvé lors du dernier Appel à BuildIdentification
        /// </summary>
        public bool IsXslFind
        {
            get { return _isXslFind; }
        }
        #endregion
        //
        #region constructor
        //PL 20130422
        //public TradeIdentificationBuilder(bool pIsUseGetIdForIdentifier, TradeCommonInput pTradeInput, SpheresIdentification pInitialValue)
        public TradeIdentificationBuilder(TradeRecordSettings pRecordSettings, TradeCommonInput pTradeInput, SpheresIdentification pInitialValue)
        {
            _isXslFind = false;
            //_isUseGetIdForIdentifier = pIsUseGetIdForIdentifier;
            _recordSettings = pRecordSettings;
            _tradeInput = pTradeInput;

            _initialIdentification = new SpheresIdentification
            {
                Identifier = pInitialValue.Identifier,
                Description = pInitialValue.Description,
                Displayname = pInitialValue.Displayname,
                Extllink = pInitialValue.Extllink
            };
        }
        #endregion
        //
        #region Methodes
        /// <summary>
        /// Retourne les identifiants d'un trade [Identifier,Displayname, Description, etc..] par application du XSL uniquement
        /// </summary>
        /// <param name="pSessionInfo"></param>
        /// <returns></returns>
        public SpheresIdentification BuildIdentification(string pCs, CaptureSessionInfo pSessionInfo)
        {
            return BuildIdentification(pCs, null, Cst.Capture.ModeEnum.New, pSessionInfo);
        }

        /// <summary>
        /// Retourne les identifiants d'un trade [Identifier, Displayname, Description...] 
        /// <para>L'identifier d'un trade est éventuellement obtenu via l'usage de la procédure GETID</para>
        /// <para>L'identifier et les autres identifiants sont obtenus par application du XSL (lorsque l'appel à GETID n'est pas effectué)</para>
        /// 
        /// 
        /// La méthode de construction de l’identifiant a été découpée en plusieurs micro-méthodes (EG 20210421 [25723])
        /// --------------------------------------------------------
        /// Synoptique en cas de création d’un nouvel identifiant
        /// -------------------------------------------------------- 
        /// 1.	Lecture des préfixes / Suffixes
        ///     Si CRP
        ///      Pair(prefix, suffix) = Recherche des préfixe/suffixe CRP en fonction de l’instrument.
        ///     Si CA
        ///      Pair(prefix, suffix) = Recherche des préfixe/suffixe CA en fonction de l’instrument.
        ///     Sinon
        ///      Pair(prefix, suffix) = Recherche des préfixe/suffixe (IDSTENVIRONNEMENT/IDSTBUSINESS) en fonction de l’instrument et des statuts du trade(GetPrefixSuffixForStatus)
        ///      NB : (Sinon peut aisément être supprimé dans le code, dans ce cas les préfixes/Suffixes propres aux statuts sont ajoutés au CRP ou à la CA)
        ///      
        /// 2.	Pair(finalPrefix, finalSuffix) = Evaluation des variables présentes dans préfixes / Suffixes(ReplaceVariableOnPrefixSuffix)
        /// 3.	Identifier = Calcul du nouvel identifiant(GetNewIdentifier)
        /// 4.	Gestion de l’éventuel dépassement de capacité du nouvel identifiant finalPrefix + Identifier + finalSuffix(CheckColumnDataCapacity)
        /// 5.	Gestion de l’éventuel dépassement de capacité pour la donnée DESCRIPTION créée dynamiquement dans le code(CA, CRP)
        /// -------------------------------------------------------- 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify
        // EG 20180205 [23769] Use pDbTransaction  
        // EG 20180606 [23980] Set missing dbTransaction (parallelism)
        // EG 20190613 [24683] Use Prefix|Suffix for Closing/Reopening positions
        // EG 20210420 [25723] Contrôle -Garde fou dépassement de capacité de l'identifant d'un trade (CheckIdentifierCapacity)
        // EG 20210421 [25723] Refactoring complet (découpage en appel de micro-méthodes, Regroupement traitement Trade classique, CA et CRP)
        // EG 20220617 [26073] Correction sur Identifiant d'un trade EX sur CA
        public SpheresIdentification BuildIdentification(string pCs, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo)
        {
            _isXslFind = false;

            SpheresIdentification ret = new SpheresIdentification
            {
                Identifier = _initialIdentification.Identifier,
                Displayname = _initialIdentification.Displayname,
                Description = _initialIdentification.Description,
                Extllink = _initialIdentification.Extllink
            };

            #region Identifier via GetId
            //if (_isUseGetIdForIdentifier)
            if ((_recordSettings != null) && _recordSettings.isGetNewIdForIdentifier)
            {
                // _recordSettings.isGetNewIdForIdentifier: Identifier du trade à calculer via l'utilisation de GetId ou non.
                ///ex: true pour les "Trades de marchés" (eg. DebSecurityTransaction, Swap...), false pour les "Titres" (eg. DebSecurity)
                string newIdentifier = "0";
                string prefix = string.Empty;
                string suffix = string.Empty;
                // FI 20200424 [XXXXX] cacheOn
                int idAEntity = _tradeInput.DataDocument.GetFirstEntity(CSTools.SetCacheOn(pCs), pDbTransaction);
                DateTime tradeDate = _tradeInput.DataDocument.TradeDate;
                DateTime businessDate = _tradeInput.Product.GetBusinessDate2();
                
                bool isIdentifierToModify = false;
                string old_prefix = string.Empty;
                string old_suffix = string.Empty;

                //WARNING: Le calcul d'un nouvel identifier s'effectue uniquement:
                //         - s'il y a création d'un nouveau trade 
                //         - ou s'il y a modification d'une donnée spécifique 
                bool isGetNewIdForIdentifier = false;
                if (Cst.Capture.IsModeNewCapture(pCaptureMode))
                {
                    //Création --> Calcul d'un nouvel identifier
                    isGetNewIdForIdentifier = true;
                }
                else if (Cst.Capture.IsModeUpdateGen(pCaptureMode))
                {
                    //Modification --> Calcul d'un nouvel identifier si IsStEnvironmentChanged ou IsEntityChanged
                    if (_tradeInput.TradeStatus.stEnvironment.IsChanged)
                    {
                        if (_tradeInput.SQLInstrument.IsTradeIdByStEnv)
                        {
                            isGetNewIdForIdentifier = true;
                        }
                        else
                        {
                            #region EntityChanged
                            if (idAEntity <= 0)
                            {
                                // FI 20200424 [XXXXX] cacheOn
                                idAEntity = _tradeInput.DataDocument.GetFirstEntity(CSTools.SetCacheOn(pCs), pDbTransaction);
                            }
                            bool isEntityChanged = (_tradeInput.EntityOnLoad != idAEntity);
                            if (isEntityChanged && _tradeInput.SQLInstrument.IsTradeIdByEntity)
                            {
                                isGetNewIdForIdentifier = true;
                            }
                            #endregion

                            if (!isGetNewIdForIdentifier)
                            {
                                #region Get Old Prefix and Old Suffix
                                if (_tradeInput.TradeStatus.IsCurrentStEnvironment_Regular)
                                {
                                    old_prefix = _tradeInput.SQLInstrument.TradeIdPrefixReg;
                                    old_suffix = _tradeInput.SQLInstrument.TradeIdSuffixReg;
                                }
                                //20100311 PL-StatusBusiness
                                //else if (_tradeInput.IsCurrentStEnvironment_PreTrade)
                                //{
                                //    old_prefix = _tradeInput.SQLInstrument.TradeIdPrefixPre;
                                //    old_suffix = _tradeInput.SQLInstrument.TradeIdSuffixPre;
                                //}
                                else if (_tradeInput.TradeStatus.IsCurrentStEnvironment_Simul)
                                {
                                    old_prefix = _tradeInput.SQLInstrument.TradeIdPrefixSim;
                                    old_suffix = _tradeInput.SQLInstrument.TradeIdSuffixSim;
                                }
                                //PL 20100623 Newness
                                bool isExistold_PrefixOrSuffix = StrFunc.IsFilled(old_prefix) || StrFunc.IsFilled(old_suffix);
                                if (isExistold_PrefixOrSuffix)
                                {
                                    if ((old_prefix.IndexOf("_ENTITY%%") > 0) || (old_suffix.IndexOf("_ENTITY%%") > 0))
                                    {
                                        if (_tradeInput.EntityOnLoad > 0)
                                        {
                                            // FI 20200424 [XXXXX] cacheOn
                                            SQL_Actor sql_ActorEntityOnLoad = new SQL_Actor(CSTools.SetCacheOn(pCs), _tradeInput.EntityOnLoad)
                                            {
                                                DbTransaction = pDbTransaction
                                            };
                                            // FI 20200424 [XXXXX] cacheOn
                                            SQL_Entity sql_EntityOnLoad = new SQL_Entity(CSTools.SetCacheOn(pCs), sql_ActorEntityOnLoad.Id)
                                            {
                                                DbTransaction = pDbTransaction
                                            };

                                            old_prefix = TradeRDBMSTools.ReplaceDynamicConstantsWithEntityInfo(old_prefix, sql_ActorEntityOnLoad, sql_EntityOnLoad);
                                            old_suffix = TradeRDBMSTools.ReplaceDynamicConstantsWithEntityInfo(old_suffix, sql_ActorEntityOnLoad, sql_EntityOnLoad);
                                        }
                                    }
                                    if ((old_prefix.IndexOf(Cst.BUSINESSDATE.Substring(0, Cst.BUSINESSDATE.Length - 2)) >= 0)
                                        || (old_prefix.IndexOf(Cst.TRANSACTDATE.Substring(0, Cst.TRANSACTDATE.Length - 2)) >= 0)
                                        || (old_suffix.IndexOf(Cst.BUSINESSDATE.Substring(0, Cst.BUSINESSDATE.Length - 2)) >= 0)
                                        || (old_suffix.IndexOf(Cst.TRANSACTDATE.Substring(0, Cst.TRANSACTDATE.Length - 2)) >= 0))
                                    {
                                        //WARNING: Il faudra utiliser ici les dates en vigueur lors du chargement du trade (TBD)
                                        //old_prefix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(old_prefix, tradeDate, businessDate);
                                        //old_suffix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(old_suffix, tradeDate, businessDate);
                                        old_prefix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(old_prefix, Cst.TRANSACTDATE, tradeDate);
                                        old_prefix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(old_prefix, Cst.BUSINESSDATE, tradeDate);
                                        old_suffix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(old_suffix, Cst.TRANSACTDATE, businessDate);
                                        old_suffix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(old_suffix, Cst.BUSINESSDATE, businessDate);
                                    }
                                }
                                #endregion Get Old Prefix and Old Suffix
                            }
                        }
                    }
                }

                if (isGetNewIdForIdentifier)
                {
                    // EG 20210421 [25723] Refactoring complet (découpage en micro-méthodes)
                    DateTime dtCorpoAction = DateTime.MinValue;
                    string CRPPosition= string.Empty;
                    Pair<string, string> sourcePrefixSuffix = new Pair<string, string>();

                    if (_recordSettings.typeForClosingReopeningPosition.HasValue)
                    {
                        CRPPosition = _recordSettings.typeForClosingReopeningPosition.Value.ToString();
                        // ClosingReopening position : Cas particulier de l'obtention du nouvel identifiant d'un trade ayant subit une fermeture/Réouverture de position                        
                        if (_recordSettings.typeForClosingReopeningPosition.Value == FixML.v50SP1.Enum.PositionEffectEnum.Close)
                        {
                            sourcePrefixSuffix.First = _tradeInput.SQLInstrument.TradeIdPrefixClosingPosition;
                            sourcePrefixSuffix.Second = _tradeInput.SQLInstrument.TradeIdSuffixClosingPosition;
                        }
                        else if (_recordSettings.typeForClosingReopeningPosition.Value == FixML.v50SP1.Enum.PositionEffectEnum.Open)
                        {
                            sourcePrefixSuffix.First = _tradeInput.SQLInstrument.TradeIdPrefixReopeningPosition;
                            sourcePrefixSuffix.Second = _tradeInput.SQLInstrument.TradeIdSuffixReopeningPosition;
                        }
                    }
                    else if (DtFunc.IsDateTimeFilled(_recordSettings.dtCorpoAction))
                    {
                        dtCorpoAction = _recordSettings.dtCorpoAction;
                        // Corporate Action: Cas particulier de l'obtention du nouvel identifiant d'un trade ayant subit une Corporate Action
                        sourcePrefixSuffix.First = _tradeInput.SQLInstrument.TradeIdPrefixCorpoAction;
                        sourcePrefixSuffix.Second = _tradeInput.SQLInstrument.TradeIdSuffixCorpoAction;
                        // EG 20220617 [26073] Add
                        newIdentifier = _tradeInput.Identifier;
                    }

                    // EG 20220617 [26073] Correction sur Identifiant d'un trade EX sur CA
                    // EG 20220616 on lit lit pas les préfixes des statuts d'environnement et business sur une CA
                    if (DtFunc.IsDateTimeEmpty(dtCorpoAction))
                    {
                        // Si ni préfixe/Suffixe alors on va lire ceux des statuts d'environnement et business
                        if (String.IsNullOrEmpty(sourcePrefixSuffix.First) && String.IsNullOrEmpty(sourcePrefixSuffix.Second))
                            sourcePrefixSuffix = TradeRDBMSTools.GetPrefixSuffixForStatus(_tradeInput.SQLInstrument, _tradeInput.TradeStatus, sourcePrefixSuffix.First, sourcePrefixSuffix.Second);
                    }

                    // Remplacement des variables présentes dans le coule préfixe/suffixe
                    if (StrFunc.IsFilled(sourcePrefixSuffix.First) || StrFunc.IsFilled(sourcePrefixSuffix.Second))
                    {
                        Pair<string, string> finalPrefixSuffix = TradeRDBMSTools.ReplaceVariableOnPrefixSuffix(pCs, pDbTransaction, sourcePrefixSuffix.First, sourcePrefixSuffix.Second, 
                            idAEntity, tradeDate, businessDate, _tradeInput.Identifier, dtCorpoAction, CRPPosition);
                        prefix = finalPrefixSuffix.First;
                        suffix = finalPrefixSuffix.Second;
                    }

                    // EG 20220617 [26073] Correction sur Identifiant d'un trade EX sur CA
                    if (DtFunc.IsDateTimeEmpty(dtCorpoAction))
                    {
                        // Calcul nouvel identifiant
                        newIdentifier = TradeRDBMSTools.GetNewIdentifier(pCs, pDbTransaction, _tradeInput.SQLInstrument, idAEntity, _tradeInput.TradeStatus, tradeDate, businessDate);
                    }
                }
                else if ((Cst.Capture.IsModeUpdateGen(pCaptureMode)) && (_tradeInput.TradeStatus.stEnvironment.IsChanged))
                {
                    //Calcul d'un éventuel nouvel identifiant

                    //Rq.: Appel sans alimentation de pDbTransaction, afin de valoriser uniquement les paramètres opPrefix et opSuffix.
                    TradeRDBMSTools.BuildTradeIdentifier(pCs, null, _tradeInput.SQLInstrument, idAEntity, _tradeInput.TradeStatus, tradeDate, businessDate, 
                        out _, out prefix, out suffix);

                    bool isPrefixDifferent = (StrFunc.IsFilled(old_prefix) || StrFunc.IsFilled(prefix)) && (old_prefix != prefix);
                    bool isSuffixDifferent = (StrFunc.IsFilled(old_suffix) || StrFunc.IsFilled(suffix)) && (old_suffix != suffix);
                    isIdentifierToModify = isPrefixDifferent || isSuffixDifferent;
                    if (isIdentifierToModify)
                    {
                        newIdentifier = _tradeInput.Identifier.TrimStart(old_prefix.ToCharArray());
                        newIdentifier = newIdentifier.TrimEnd(old_suffix.ToCharArray());
                    }
                }

                if (isGetNewIdForIdentifier || isIdentifierToModify)
                {
                    //Constitution du nouvel identifiant 
                    // EG 20210420 [25723] New
                    ret.Identifier = CheckColumnDataCapacity(64, prefix, newIdentifier, suffix);
                }
                else
                {
                    //Conservation de l'identifiant d'origine
                    ret.Identifier = _tradeInput.Identifier;
                }
            }
            #endregion

            #region Identifier, DisplayName, Description, ExtlLink via XSL transformation
            if (StrFunc.IsEmpty(ret.Identifier) || (ret.Identifier.ToLower() == ConstDefault) ||
                StrFunc.IsEmpty(ret.Displayname) || (ret.Displayname.ToLower() == ConstDefault) ||
                StrFunc.IsEmpty(ret.Description) || (ret.Description.ToLower() == ConstDefault) ||
                StrFunc.IsEmpty(ret.Extllink) || (ret.Extllink.ToLower() == ConstDefault))
            {
                
                AppInstance appInstance = pSessionInfo.session.AppInstance;
                /// FI 20160804 [Migration TFS] Modify
                //string xsltPath = @"~\OTCml\XSL_Files\Message\Identifier\" + _tradeInput.Product.productBase.ProductName + "_Identifier.xslt";
                string xsltPath = @".\BuildIdentifier\" + _tradeInput.Product.ProductBase.ProductName + "_Identifier.xslt";

                _isXslFind = appInstance.SearchFile2(pCs, pDbTransaction, xsltPath, ref xsltPath);
                if (IsXslFind)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    try
                    {
                        #region Create xsl param
                        Hashtable xsltParamList = new Hashtable
                        {
                            { "pProduct", _tradeInput.Product.ProductBase.ProductName },
                            { "pInstrument", _tradeInput.SQLInstrument.Identifier },
                            { "pIdStEnvironment", _tradeInput.TradeStatus.stEnvironment.NewSt }
                        };
                        #endregion Create xsl param
                        //
                        #region Serialize and Transform
                        Encoding encoding = new UTF8Encoding();
                        EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(_tradeInput.DataDocument.DataDocument.GetType(), _tradeInput.DataDocument.DataDocument);
                        StringBuilder serializeDoc = CacheSerializer.Serialize(serializeInfo, encoding);
                        //
                        StrBuilder resultRemoveXmlnsAlias = new StrBuilder(XSLTTools.RemoveXmlnsAlias(serializeDoc, encoding));
                        StringBuilder serializeDocWithoutXmlns = resultRemoveXmlnsAlias.StringBuilder;
                        //
                        string resultTransformXml = XSLTTools.TransformXml(serializeDocWithoutXmlns, xsltPath, xsltParamList, null);
                        #endregion Serialize and Transform
                        //
                        xmldoc.LoadXml(resultTransformXml);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(StrFunc.AppendFormat("Error on xsl transformation" + Cst.CrLf + "{0}", ex.Message));
                    }
                    //identifier
                    try
                    {
                        string identifier = ret.Identifier;
                        SetIdentification(xmldoc, "TradeIdentification/Identifier", ref identifier);
                        ret.Identifier = identifier;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(StrFunc.AppendFormat("Error on set identifier" + Cst.CrLf + "{0}", ex.Message));
                    }
                    //displayName
                    try
                    {
                        string displayName = ret.Displayname;
                        SetIdentification(xmldoc, "TradeIdentification/DisplayName", ref displayName);
                        ret.Displayname = displayName;
                        //
                        if (StrFunc.IsEmpty(ret.Displayname) || (ret.Displayname.ToLower() == ConstDefault))
                            ret.Displayname = ret.Identifier;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(StrFunc.AppendFormat("Error on set displayName" + Cst.CrLf + "{0}", ex.Message));
                    }
                    //description
                    try
                    {
                        string description = ret.Description;
                        SetIdentification(xmldoc, "TradeIdentification/Description", ref description);
                        ret.Description = description;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(StrFunc.AppendFormat("Error on set description" + Cst.CrLf + "{0}", ex.Message));
                    }
                    //extllink
                    try
                    {
                        string extllink = ret.Extllink;
                        SetIdentification(xmldoc, "TradeIdentification/ExtlLink", ref extllink);
                        ret.Extllink = extllink;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(StrFunc.AppendFormat("Error on set ExtlLink" + Cst.CrLf + "{0}", ex.Message));
                    }
                }
            }
            #endregion

            return ret;
        }

        /// <summary>
        /// <summary>
        /// Gestion (Garde-fou) du dépassement de capacité d'une colonne avec l'usage du préfixe et suffixe
        /// </summary>
        /// <param name="pMaxLength">Taille maximale de la donnée à traiter</param>
        /// <param name="pPrefix">Préfixe après transformation</param>
        /// <param name="pNewValue">Valeur générée de base</param>
        /// <param name="pSuffix">Suffixe après transformation</param>
        /// <returns>Data adaptée à la taille</returns>
        public static string CheckColumnDataCapacity(int pMaxLength, string pPrefix, string pNewValue, string pSuffix)
        {
            string newValue = pNewValue;
            _ = newValue.Length;

            string point = "...";
            int maxLength = pMaxLength;

            // Gestion du Prefixe et contrôle de la longueur de la donnée
            if (StrFunc.IsFilled(pPrefix) && (false == newValue.StartsWith(pPrefix)))
            {
                // La donnée est amputée s'il y a dépassement de capacité (avec ajout du préfixe)
                // => notifié par l'ajout de ... (à gauche)
                if (maxLength < (newValue.Length + pPrefix.Length))
                    newValue = point + newValue.Substring(pPrefix.Length + 3 + newValue.Length - maxLength);

                // ajout du préfixe
                newValue = pPrefix + newValue;
            }

            // Gestion du Suffixe et contrôle de la longueur de la donnée
            if (StrFunc.IsFilled(pSuffix) && (false == newValue.EndsWith(pSuffix)))
            {
                // L'identifiant (déjà préfixé) est amputé s'il y a dépassement de capacité (avec ajout du suffixe)
                // => notifié par l'ajout de ... (à droite)
                if (maxLength < (newValue.Length + pSuffix.Length))
                    newValue = newValue.Substring(0, maxLength - pSuffix.Length - 3) + point;

                // ajout du suffixe
                newValue += pSuffix;
            }
            return newValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmldoc"></param>
        /// <param name="pXPath"></param>
        /// <param name="pField"></param>
        private static void SetIdentification(XmlDocument pXmldoc, string pXPath, ref string pField)
        {
            if (StrFunc.IsEmpty(pField) || (pField.ToLower() == ConstDefault))
            {
                XmlNode nodeIdentification = pXmldoc.SelectSingleNode(pXPath);
                //
                if ((null != nodeIdentification) && StrFunc.IsFilled(nodeIdentification.InnerText))
                {
                    pField = nodeIdentification.InnerText.Trim(); // 20091006 RD Ticket 16608
                    pField = pField.Replace(Cst.NonBreakSpace, " ");
                }
            }
        }
        #endregion
    }
}
