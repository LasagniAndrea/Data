#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using FpML.Interface;
using System;
using System.Linq;
using System.Text.RegularExpressions;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciTrader
    /// <summary>
    /// Représente un trader ou un sale
    /// </summary>
    public class CciTrader : IContainerCciFactory, IContainerCci, IContainerCciSpecified, ICciPresentation
    {
        /// <summary>
        ///  Liste des données disponibles
        /// </summary>
        public enum CciEnum
        {
            /// <summary>
            /// Identifiant du trader/sales
            /// </summary>
            identifier,
            /// <summary>
            /// coef (disponible uniquement si sales)
            /// </summary>
            factor,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum TraderTypeEnum
        {
            /// <summary>
            /// Sales person
            /// </summary>
            sales,
            /// <summary>
            /// Trader
            /// </summary>
            trader
        }

        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly TraderTypeEnum _traderType;

        private readonly int _number;
        private readonly string _prefix;

        /// <summary>
        /// Représente la partie à laquelle est rattachée le trader/sales
        /// </summary>
        private readonly CciTradeParty _cciParty;
        private readonly TradeCommonCustomCaptureInfos _ccis;

        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la collections ccis
        /// </summary>
        public CustomCaptureInfosBase Ccis => _ccis;
        /// <summary>
        ///  Obtient la partie à laquelle est rattachée le trader/sales 
        /// </summary>
        public CciTradeParty CciParty
        {
            get { return _cciParty; }
        }
        /// <summary>
        ///  Obtient ou Définit l'élément trader alimenté via les ccis
        /// </summary>
        public ITrader Trader
        {
            get;
            set;
        }

        #endregion Accessors

        #region Constructors
        public CciTrader(CciTradeParty pCciTradeParty, int plNumber, string pPrefixParent, TraderTypeEnum pTraderType)
        {

            _traderType = pTraderType;
            _cciParty = pCciTradeParty;

            _ccis = pCciTradeParty.Ccis;

            _number = plNumber;

            string prefix;
            if (TraderTypeEnum.trader == _traderType)
                prefix = TradeCommonCustomCaptureInfos.CCst.Prefix_trader;
            else if (TraderTypeEnum.sales == _traderType)
                prefix = TradeCommonCustomCaptureInfos.CCst.Prefix_sales;
            else
                throw new NotImplementedException("traderType not implemented");

            _prefix = pPrefixParent + prefix + _number.ToString() + CustomObject.KEY_SEPARATOR;

        }
        #endregion Constructors

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
        {
            CciTools.CreateInstance(this, Trader);

        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, Trader);
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170718 [23326] Modify
        /// FI 20170214 [23629] Modify
        public void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[_prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    bool isToValidate = false;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.identifier:
                            #region identifier
                            SQL_Actor sql_Actor = null;
                            string traderValue = Trader.Identifier;

                            if (StrFunc.IsFilled(traderValue))
                            {
                                int idTrader;
                                if (traderValue.IndexOf(Cst.FpMLTraderSeparator) > -1)
                                {
                                    string[] arrTraderValue = traderValue.Split(Cst.FpMLTraderSeparator);
                                    idTrader = Convert.ToInt32(arrTraderValue[ArrFunc.Count(arrTraderValue) - 1]);
                                }
                                else
                                    idTrader = Trader.OTCmlId;

                                if (idTrader > 0)
                                    sql_Actor = new SQL_Actor(CciParty.cciTrade.CSCacheOn, idTrader);

                                if ((null != sql_Actor) && sql_Actor.IsLoaded)
                                {
                                    data = sql_Actor.Identifier;
                                    sql_Table = (SQL_Table)sql_Actor;

                                    Trader.Identifier = sql_Actor.Identifier;
                                    Trader.Name = sql_Actor.DisplayName;
                                    Trader.OTCmlId = sql_Actor.Id;
                                }
                                else
                                {
                                    // FI 20170718 [23326] Affichage du dealer même lorsqu'il n'existe pas dans la DB 
                                    // Trader importer avec un dealer inconnu dans Spheres®
                                    data = Trader.Identifier;
                                    // FI 20170214 [23629] isToValidate = true=> pour avoir du rouge à l'écran les traders inconnus
                                    isToValidate = true;
                                }
                            }
                            #endregion identifier
                            break;
                        case CciEnum.factor:
                            #region factor
                            // 20090610 RD pour n'afficher le 'factor' que si on a un Actor (Trader) de renseigné
                            if (StrFunc.IsFilled(Trader.StrFactor) && StrFunc.IsFilled(Trader.Identifier))
                                data = Trader.StrFactor;
                            #endregion factor
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                    {
                        Ccis.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }


        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                {
                    CciEnum key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);
                    switch (key)
                    {
                        case CciEnum.identifier:
                            if (pCci.IsEmpty)
                                Clear();
                            SetEnabled(pCci.IsFilled);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            Ccis.Set(CciClientId(CciEnum.factor), "IsEnabled", IsSpecified);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string _)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141119 [20505] Modify
        /// FI 20170404 [23039] Modify
        /// FI 20170718 [23326] Modify
        /// FI 20200421 [XXXXX] Usage de ccis.ClientId_DumpToDocument
        public void Dump_ToDocument()
        {
            foreach (string clientId in Ccis.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = Ccis[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.identifier:
                            #region identifier
                            SQL_Actor sql_Actor = null;
                            bool isLoaded = false;
                            bool isValid = false;
                            if (isFilled)
                            {
                                //Check if actor is a valid operator_fo (Scan ACTOR via IDENTIFIER)
                                sql_Actor = new SQL_Actor(CciParty.cciTrade.CSCacheOn, data, SQL_Table.RestrictEnum.Yes,
                                    SQL_Table.ScanDataDtEnabledEnum.Yes, Ccis.User, Ccis.SessionId);

                                // FI 20170404 [23039] call SetRole method
                                //if (TraderTypeEnum.trader == _traderType)
                                //    sql_Actor.SetRole(RoleActor.TRADER);
                                //else
                                //    sql_Actor.SetRole(RoleActor.SALES);
                                SetRole(sql_Actor);

                                isLoaded = sql_Actor.IsLoaded;

                            }
                            if (isLoaded)
                            {
                                cci.NewValue = sql_Actor.Identifier;
                                cci.Sql_Table = sql_Actor;
                                cci.ErrorMsg = string.Empty;

                                Trader.Identifier = sql_Actor.Identifier;
                                Trader.Name = sql_Actor.DisplayName;
                                Trader.OTCmlId = sql_Actor.Id;

                                //AL 20240620 [WI975] Only valid trader for the actor are allowed
                                var lst = ActorTools.LoadTraderAlgo(CciParty.cciTrade.CSCacheOn, CciParty.GetActorIda(), Ccis.User, Ccis.SessionId);
                                isValid = lst.Where(x => x.Item2 == sql_Actor.Identifier).Count() > 0;
                                if (!isValid)
                                    cci.ErrorMsg = CciTools.BuildCciErrMsg(
                                        string.Format(Ressource.GetString("Msg_ValidationRule_party_Trader_Unknown"), data, CciParty.GetParty().PartyName), cci.NewValue);
                            }
                            else
                            {
                                cci.Sql_Table = null;
                                if (isFilled)
                                {
                                    cci.ErrorMsg = Ressource.GetString("Msg_ActorNotFound");
                                    // FI 20170404 [23039] comportement spécifique si isAutoCreate sur trader 
                                    if (cci.IsAutoCreate && this._traderType == TraderTypeEnum.trader)
                                    {
                                        // si data ne se termine pas par " [xxxxx]" => alors ajout du suffix " [IDENTIFIER]" pour simplifier la suite
                                        // FI 20170718 [23326] chgt de l'expression regex 
                                        Regex regex = new Regex(@"^.+\s\[.+\]$");
                                        if (false == regex.IsMatch(data))
                                            data += " [IDENTIFIER]";
                                        // FI 20170718 [23326] chgt de l'expression regex 
                                        // data peut matcher avec la regex suivante si importation de trade 
                                        regex = new Regex(@"^(.+)\s\[(BIC|IBEI|ISO17442|IDA|IDENTIFIER|DISPLAYNAME|EXTLLINK|EXTLLINK2|MemberId-\w+)\]$");
                                        Match match = regex.Match(data);
                                        if (match.Success && match.Groups.Count == 3)
                                        {
                                            // IDENTIFIER, DISPLAYNAME et EXTLLINK => création du trader en automatique possible  
                                            // Avant d' "autoriser" la création automatique dernier contôle afin de vérifier que l'acteur n'existe vraiment pas 
                                            // Lecture de la table ACTEUR (sans usage de rôle et sans restriction) 
                                            // FI 20170718 [23326] correction d'une boulette => Utilisation de match.Groups[1].Value pour vérifier l'existence de l'acteur
                                            switch (match.Groups[2].Value)
                                            {
                                                case "IDENTIFIER":
                                                    sql_Actor = new SQL_Actor(CciParty.cciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, match.Groups[1].Value,
                                                            SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, Ccis.User, Ccis.SessionId);
                                                    break;
                                                case "DISPLAYNAME":
                                                    sql_Actor = new SQL_Actor(CciParty.cciTrade.CSCacheOn, SQL_TableWithID.IDType.Displayname, match.Groups[1].Value,
                                                        SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, Ccis.User, Ccis.SessionId);
                                                    break;
                                                case "EXTLLINK":
                                                    sql_Actor = new SQL_Actor(CciParty.cciTrade.CSCacheOn, SQL_TableWithID.IDType.ExtLink, match.Groups[1].Value,
                                                        SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, Ccis.User, Ccis.SessionId);
                                                    break;
                                                default:
                                                    sql_Actor = null;
                                                    break;
                                            }
                                        }

                                        if (null != sql_Actor)
                                        {
                                            //On renstre ici uniquement si l'identification en entrée est IDENTIFIER, DISPLAYNAME, EXTLLINK
                                            //Seules identifications admises pour l'auto-création d'un trader
                                            if (sql_Actor.IsFound)
                                            {
                                                // L'acteur existe mais l'utilisateur connecté n'a pas le droit de l'utiliser ou l'acteur (ou son rôle) est désactivé ou ...
                                                // On laisse le message d'erreur => pas de création du trader
                                                //=> le cci reste en erreur
                                            }
                                            else
                                            {
                                                // L'acteur n'existe pas et le type d'identification est IDENTIFIER, DISPLAYNAME, EXTLLINK (cas géré pour la génération d'un acteur)
                                                // => Dans de cas (puisque isAutoCreate) pas d'erreur le trader sera créé lors de l'enregistrement
                                                cci.ErrorMsg = string.Empty;
                                            }
                                        }
                                    }

                                    Trader.Identifier = cci.NewValue;
                                    Trader.Name = string.Empty;
                                    Trader.OtcmlId = string.Empty;
                                }
                                else
                                {
                                    cci.ErrorMsg = (cci.IsMandatory ? Ressource.GetString("Msg_ActorNotFound") : string.Empty);
                                    Trader.Identifier = string.Empty;
                                    Trader.Name = string.Empty;
                                    Trader.OtcmlId = string.Empty;
                                }
                            }


                            if (CciParty.IsInitFromPartyTemplate)
                            {
                                // FI 20180608 [XXXXX] Recette v7.2 
                                // =>  DumpSales_ToDocument_FromFirstTrader uniquement s'il n'y a pas d'erreur 
                                // le cci peut être alimenté avec un trader devenu disabled ou même un trader inconnu  (Initialize_FromDocument)
                                // Dans ce cas pas de DumpSales_ToDocument_FromFirstTrader
                                if ((TraderTypeEnum.trader == _traderType) &&
                                    (CciTradeParty.PartyType.party == CciParty.partyType) && _number == 1 && (false == cci.HasError))
                                {
                                    // FI 20141119 [20505] Appel à DumpSales_ToDocument_FromFirstTrader si le trader est renseigné ou vide 
                                    bool isOk = CaptureTools.DumpSales_ToDocument_FromFirstTrader(CciParty.cciTrade.CSCacheOn, CciParty.cciTrade.TradeCommonInput, CciParty.GetPartyId());
                                    if (isOk)
                                    {
                                        Ccis.IsToSynchronizeWithDocument = true;
                                        // FI 20240206 [WI840] la propriété IsToSynchronizeWithDocument n'est pas utlisée par IO
                                        // Il faut charger les ccis existants sales avec ce qui est remonté depuis PartyTemplate
                                        if (Ccis.IsModeIO)
                                        {
                                            CciParty.InitializeSales_FromCci();
                                            foreach (CciTrader item in CciParty.cciSales)
                                                item.Initialize_Document();
                                        }
                                    }
                                }
                            }

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion identifier
                            break;
                        case CciEnum.factor:
                            #region factor
                            Trader.Factor = 0;
                            if (StrFunc.IsFilled(data))
                                Trader.StrFactor = data;

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion factor
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);

                }
            }
        }

        #endregion IContainerCciFactory Members

        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnum)
        {
            return Cci(pEnum.ToString());
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
        }
        #endregion CciClientId
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(_prefix));
        }

        #endregion IContainerCci Members

        #region IContainerCciSpecified Members
        /// <summary>
        ///  Retourne true si une trader/sales est renseigné 
        /// </summary>
        /// FI 20170404 [23039] Modify
        /// FI 20170214 [23629] Modify
        public bool IsSpecified
        {
            get
            {
                // FI 20170214 [23629] true si Cci(CciEnum.identifier).IsFilled; 
                /*
                // FI 20170404 [23039] Modify
                //return (null != Cci(CciEnum.identifier).Sql_Table);
                // FI 20170404 [23039] Gestion isAutoCreate
                if (Cci(CciEnum.identifier).IsAutoCreate)
                {
                    return Cci(CciEnum.identifier).IsFilled;
                }
                else
                {
                    return (null != Cci(CciEnum.identifier).Sql_Table);
                }
                 */
                return Cci(CciEnum.identifier).IsFilled;
            }
        }
        #endregion IContainerCciSpecified Members


        /// <summary>
        /// Toutes les valeurs des ccis sont placés à string.Empty
        /// </summary>
        public void Clear()
        {
            CciTools.SetCciContainer(this, "CciEnum", "NewValue", string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            //Doit tjs être Enabled 
            Cci(CciEnum.identifier).IsEnabled = true;
        }

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = Cci(CciEnum.identifier);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ACTOR);
        }

        /// <summary>
        /// Applique le rôle à {sql_Actor}
        /// </summary>
        /// <param name="sql_Actor"></param>
        /// FI 20170404 [23039] Add method
        /// FI 20170928 [23452] Modify
        private void SetRole(SQL_Actor sql_Actor)
        {
            switch (_traderType)
            {

                case TraderTypeEnum.trader:// FI 20170928 [23452]  Ajout du rôle EXECUTION
                    sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.TRADER, RoleActor.EXECUTION });
                    break;
                case TraderTypeEnum.sales: // FI 20170928 [23452]  Ajout du rôle INVESTDECISION
                    sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.SALES, RoleActor.INVESTDECISION });
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Type (id:{0}) is not implemented", _traderType.ToString()));
            }
        }


        #endregion
    }
    #endregion CciTrader
}
