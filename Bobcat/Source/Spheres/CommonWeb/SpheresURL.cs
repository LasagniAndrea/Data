using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web.Menu;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using SpheresMenu = EFS.Common.Web.Menu;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class SpheresURL
    {
        /// <summary>
        /// Retourne l'URL du Formulaire de saisie des trades de marché
        /// </summary>
        /// <param name="pPKName"></param>
        /// <param name="pPKValue"></param>
        /// <returns></returns>
        public static string GetUrlFormTrade(string pPKName, string pPKValue, SpheresMenu.Menus pMenus)
        {
            return pMenus.GetMenu_AddPKArgument(pPKName, pPKValue, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));
        }
        /// <summary>
        /// Retourne l'URL du Formulaire de saisie des trades admin
        /// </summary>
        /// <param name="pPKName"></param>
        /// <param name="pPKValue"></param>
        /// <returns></returns>
        public static string GetUrlFormTradeAdmin(string pPKName, string pPKValue, SpheresMenu.Menus pMenus)
        {
            return pMenus.GetMenu_AddPKArgument(pPKName, pPKValue, IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin));
        }
        /// <summary>
        /// Retourne l'URL du Formulaire de saisie des trades représentatifs de RISK (Deposit, Cash-Balance, ...)
        /// </summary>
        /// <param name="pPKName"></param>
        /// <param name="pPKValue"></param>
        /// <param name="pFamily"></param>
        /// <param name="pMenus"></param>
        /// <returns></returns>
        // EG 20160404 Migration vs2013
        public static string GetUrlFormTradeRisk(string pPKName, string pPKValue, string pFamily, SpheresMenu.Menus pMenus)
        {
            // EG 20160404 Migration vs2013
            //bool isOk = true;
            Nullable<IdMenu.Menu> inputTradeRisk = null;
            switch (pFamily)
            {
                case Cst.ProductFamily_MARGIN:
                    inputTradeRisk = IdMenu.Menu.InputTradeRisk_InitialMargin;
                    break;
                case Cst.ProductFamily_CASHBALANCE:
                    inputTradeRisk = IdMenu.Menu.InputTradeRisk_CashBalance;
                    break;
                case Cst.ProductFamily_CASHPAYMENT:
                    inputTradeRisk = IdMenu.Menu.InputTradeRisk_CashPayment;
                    break;
                case Cst.ProductFamily_CASHINTEREST:
                    inputTradeRisk = IdMenu.Menu.InputTradeRisk_CashInterest;
                    break;
            }

            if (inputTradeRisk.HasValue)
                return pMenus.GetMenu_AddPKArgument(pPKName, pPKValue, IdMenu.GetIdMenu(inputTradeRisk.Value));
            else
                return StrFunc.AppendFormat(StrFunc.AppendFormat("TradeRiskCapturePage.aspx?PK={0}&PKV={1}", pPKName, pPKValue));
        }
        /// <summary>
        /// Obtient l'URL du Formulaire de saisie des titres 
        /// <para></para>
        /// </summary>
        /// <param name="pPKName"></param>
        /// <param name="pPKValue"></param>
        /// <returns></returns>
        public static string GetUrlFormTradeDebtSecurity(string pPKName, string pPKValue)
        {
            return StrFunc.AppendFormat("DebtSecCapturePage.aspx?IDMenu={0}&PK={1}&PKV={2}", IdMenu.GetIdMenu(IdMenu.Menu.InputDebtSec), pPKName, pPKValue);
        }
        /// <summary>
        /// Obtient l'URL du Formulaire de saisie des titres 
        /// <para></para>
        /// </summary>
        /// <param name="pPKName"></param>
        /// <param name="pPKValue"></param>
        /// <returns></returns>
        public static string GetUrlFormTradeDebtSecurity(string pPKName, string pPKValue, string pFKName, string pFKValue)
        {
            string ret = GetUrlFormTradeDebtSecurity(pPKName, pPKValue);
            if (pFKName != null)
                ret = StrFunc.AppendFormat(ret + "&FK={0}&FKV={1}", pFKName, pFKValue);
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPKValue"></param>
        /// <param name="pFkValue"></param>
        /// <returns></returns>
        public static string GetUrlFormEvent(string pPKValue, string pFkValue, SpheresMenu.Menus pMenus)
        {
            string ret = pMenus.GetMenu_Url(IdMenu.GetIdMenu(IdMenu.Menu.InputEvent));
            if (StrFunc.IsFilled(ret))
            {
                ret = AspTools.AddIdMenuOnUrl(ret, IdMenu.GetIdMenu(IdMenu.Menu.InputEvent));
                if (pPKValue != null)
                    ret = StrFunc.AppendFormat(ret + "&PK={0}&FKV={1}", pPKValue, pFkValue);
            }
            return ret;
        }

        /// <summary>
        /// Retourne l'URL du Formulaire de saisie des corporates action
        /// </summary>
        /// <param name="pPKName"></param>
        /// <param name="pPKValue"></param>
        /// <returns></returns>
        public static string GetUrlFormCorporateAction(string pPKName, string pPKValue, SpheresMenu.Menus pMenus)
        {
            string URL = string.Empty;
            string idMenu = string.Empty;
            if (pPKName == "IDCAISSUE")
                idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputCorporateActionIssue);
            if (pPKName == "IDCA")
                idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputCorporateActionEmbedded);
            if (StrFunc.IsFilled(idMenu))
                URL = pMenus.GetMenu_AddPKArgument(pPKName, pPKValue, idMenu);
            return URL;
        }



        /// <summary>
        /// Retourne URL qui permet d'ouvrir un formulaire de type Referential ou Trade afin de consulter la donnée {pPKValue} stockée dans la colonne {pColumn}
        /// </summary>
        /// <param name="pColumn">Represente un colonne
        /// <para>○ Couple constitué d'un nom de colonne et, si besoin d'un nom de table( les colonnes où le nom de table est obligatoire sont IDASSET et IDIOELEMENT, IDT...)</para>
        /// </param>
        /// <param name="pPKValue">Représente la donnée à consulter</param>
        /// <param name="pMode">Type de consultation du formulaire (Mode saisi, ReadOnly, etc...)</param>
        /// <param name="pFormId">Id du Formulaire Parent</param>
        /// <returns></returns>
        /// FI 20140911 [XXXXX] Add Method 
        public static string GetURLHyperLink(Pair<string, Nullable<Cst.OTCml_TBL>> pColumn, string pPKValue, Cst.ConsultationMode pMode, string pFormId)
        {
            string ret = string.Empty;
            string pageName = string.Empty;

            string columnName = pColumn.First;

            bool isOpenReferential = false;
            switch (columnName)
            {
                case "IDT":
                    if (null != pColumn.Second)
                        pageName = GetTradeURLByObjectName(pColumn.Second.ToString(), pPKValue);
                    else
                        pageName = GetTradeURLByObjectName("TRADE", pPKValue);
                    break;
                case "IDT_ADMIN":
                    pageName = GetTradeURLByObjectName("TRADEADMIN", pPKValue);
                    break;
                case "IDT_RISK":
                    pageName = GetTradeURLByObjectName("TRADERISK", pPKValue);
                    break;
                case "IDT_DEBSEC":
                    pageName = GetTradeURLByObjectName("DEBTSECURITY", pPKValue);
                    break;
                case "IDT_TRADE":
                    pageName = GetTradeURLByObjectName("TRADE", pPKValue);
                    break;
                case "IDASSET":
                case "IDASSET_UNL":
                    if (null != pColumn.Second)
                    {
                        // EG 20150601 Add Cst.OTCml_TBL.VW_TRADEDEBTSEC
                        if ((pColumn.Second.Value == Cst.OTCml_TBL.DEBTSECURITY) ||
                            (pColumn.Second.Value == Cst.OTCml_TBL.VW_TRADEDEBTSEC))
                        {
                            pageName = GetTradeURLByObjectName("DEBTSECURITY", pPKValue);
                        }
                        else
                        {
                            columnName = GetColumnAssetForOpenReferential(pColumn.Second.Value);
                            isOpenReferential = StrFunc.IsFilled(columnName);
                        }
                    }
                    break;
                case "IDIOELEMENT":
                    if (null != pColumn.Second)
                    {
                        columnName = GetColumnIOForOpenReferential(pColumn.Second.Value);
                        isOpenReferential = StrFunc.IsFilled(columnName);
                    }
                    break;

                default:
                    isOpenReferential = true;
                    break;
            }

            if (isOpenReferential)
            {
                
                GetReferentialInfoForHyperLink(columnName, pPKValue, out string @objectItem, out string idMenu, out  string columnPk, out string cond);
                if (StrFunc.IsFilled(@objectItem))
                    pageName = GetReferentialURLForHyperLink(@objectItem, idMenu, columnPk, pPKValue, pMode, pFormId, cond);
            }

            if (StrFunc.IsFilled(pageName))
                ret = pageName;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <param name="pPKValue"></param>
        /// <returns></returns>
        /// FI 20140911 [XXXXX]
        /// FI 20140930 [XXXXX] Modify
        public static string GetTradeURLByObjectName(string pObjectName, string pPKValue)
        {
            string pkValue = HttpUtility.UrlEncode(pPKValue, System.Text.Encoding.Default);

            string url;
            switch (pObjectName)
            {
                case "TRADE":
                    url = SpheresURL.GetUrlFormTrade("IDT", pkValue, SessionTools.Menus);
                    break;
                case "TRADEADMIN":
                    url = SpheresURL.GetUrlFormTradeAdmin("IDT", pkValue, SessionTools.Menus);
                    break;
                case "DEBTSECURITY":
                    url = SpheresURL.GetUrlFormTradeDebtSecurity("IDT", pkValue);
                    break;
                case "TRADERISK":
                    // Ouverture du tout type de trade RISK 
                    url = SpheresURL.GetUrlFormTradeRisk("IDT", pkValue, null, SessionTools.Menus);
                    break;
                case "RISKPERFORMANCE":
                    // Ouverture du tout type de trade deposit
                    url = SpheresURL.GetUrlFormTradeRisk("IDT", pkValue, Cst.ProductFamily_MARGIN, SessionTools.Menus);
                    break;
                case "CASHBALANCE":
                case "CASHINTEREST":
                case "CASHPAYMENT":
                    string family = pObjectName;
                    url = SpheresURL.GetUrlFormTradeRisk("IDT", pkValue, family, SessionTools.Menus);
                    break;
                default:
                    url = SpheresURL.GetUrlFormTrade("IDT", pkValue, SessionTools.Menus);
                    break;
            }

            return url;
        }

        /// <summary>
        /// Retourne un nom de colonne suffisamment explicite pour permettre l'ouverture d'un formulaire de type référentiel Asset
        /// </summary>
        /// <param name="assetCategoryEnum"></param>
        /// <returns></returns>
        /// FI 20140911 [XXXXX] Add Method
        private static string GetColumnAssetForOpenReferential(Cst.OTCml_TBL pTable)
        {
            string ret;
            switch (pTable)
            {
                case Cst.OTCml_TBL.ASSET_ETD:
                case Cst.OTCml_TBL.ASSET_EQUITY:
                case Cst.OTCml_TBL.ASSET_COMMODITY:
                case Cst.OTCml_TBL.ASSET_INDEX:
                case Cst.OTCml_TBL.ASSET_RATEINDEX:
                case Cst.OTCml_TBL.ASSET_EXTRDFUND:
                case Cst.OTCml_TBL.ASSET_CASH:
                case Cst.OTCml_TBL.ASSET_FXRATE:
                    ret = StrFunc.AppendFormat("ID{0}", pTable.ToString());
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("table {0} is not implemented", pTable.ToString()));
            }
            return ret;
        }

        /// <summary>
        /// Retourne un nom de colonne suffisamment explicite pour permettre l'ouverture d'un formulaire de type référentiel IO
        /// </summary>
        /// <param name="pTable"></param>
        /// <returns></returns>
        private static string GetColumnIOForOpenReferential(Cst.OTCml_TBL pTable)
        {
            string ret = string.Empty;
            switch (pTable)
            {
                case Cst.OTCml_TBL.IOCOMPARE:
                case Cst.OTCml_TBL.IOINPUT:
                case Cst.OTCml_TBL.IOOUTPUT:
                case Cst.OTCml_TBL.IOSHELL:
                case Cst.OTCml_TBL.IOPARSING:
                    ret = StrFunc.AppendFormat("ID{0}", pTable.ToString());
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Retourne le nom de référentiel, la colonne PK du référentiel, le menu d'un référentiel en fonction d'un nom de colonne
        /// </summary>
        /// <param name="pColumn">Nom de colonne source</param>
        /// <param name="pColumnValue">Valeur de la colonne</param>
        /// <param name="objectItem">Nom de référentiel</param>
        /// <param name="idMenu">Menu associé au référentiel</param>
        /// <param name="pColumnPK">Colonne PK du référentiel</param>
        /// <param name="pCondApp"></param>
        /// FI 20140911 [XXXXX] Add Method 
        /// FI 20150928 [XXXXX] Modify 
        /// FI 20170313 [22225] Modify
        /// FI 20170928 [23452] Modify
        private static void GetReferentialInfoForHyperLink(string pColumn, string pColumnValue, out string @objectItem, out string idMenu, out string pColumnPK, out string pCondApp)
        {
            string cs = CSTools.SetCacheOn(SessionTools.CS);
            @objectItem = string.Empty;
            idMenu = string.Empty;
            pColumnPK = pColumn;
            pCondApp = string.Empty;

            IdMenu.Menu? menu;
            switch (pColumn)
            {
                case "IDA":
                    // FI 20170928 [23452] Appel à OpenReferentialFormArguments.GetObject
                    //@objectItem = Cst.OTCml_TBL.ACTOR.ToString();
                    //idMenu = IdMenu.GetIdMenu(IdMenu.Menu.Actor);
                    OpenReferentialFormArguments.GetObject(cs, Cst.OTCml_TBL.ACTOR, pColumnValue, out @objectItem, out menu);
                    idMenu = IdMenu.GetIdMenu(menu.Value);
                    break;
                case "IDB":
                    @objectItem = Cst.OTCml_TBL.BOOK.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.Book);
                    break;
                case "IDI": // FI 20240703 [WI989] Refactoring
                    OpenReferentialFormArguments.GetObject(cs, Cst.OTCml_TBL.INSTRUMENT, pColumnValue, out @objectItem, out menu);
                    switch (menu)
                    {
                        case IdMenu.Menu.Instrument:
                            idMenu = IdMenu.GetIdMenu(IdMenu.Menu.Instrument);
                            pCondApp = "NOSTRATEGY";
                            break;
                        case IdMenu.Menu.Strategy:
                            idMenu = IdMenu.GetIdMenu(IdMenu.Menu.Strategy);
                            pCondApp = "STRATEGY";
                            break;
                    }
                    break;
                case "IDGINSTR":
                    @objectItem = Cst.OTCml_TBL.GINSTR.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.GrpInstrument);
                    break;
                case "IDP":
                    @objectItem = Cst.OTCml_TBL.PRODUCT.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.Product);
                    break;
                case "IDRX":
                    @objectItem = Cst.OTCml_TBL.RATEINDEX.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetRateIndex);
                    break;
                case "IDM":
                    @objectItem = Cst.OTCml_TBL.MARKET.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.Market);
                    break;
                case "IDDC":
                    @objectItem = Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.DrvContract);
                    break;
                case "IDDERIVATIVEATTRIB": // FI 20170313 [22225] Add
                    @objectItem = Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.DrvAttrib);
                    break;
                case "IDCC":
                    @objectItem = Cst.OTCml_TBL.COMMODITYCONTRACT.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.CommodityContract);
                    break;
                // FI 20150928 [XXXXX] add IDASSET_CASH
                case "IDASSET_CASH":
                    @objectItem = Cst.OTCml_TBL.ASSET_CASH.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetCash);
                    pColumnPK = "IDASSET";
                    break;
                case "IDASSET_ETD":
                    @objectItem = Cst.OTCml_TBL.ASSET_ETD.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetETD);
                    pColumnPK = "IDASSET";
                    break;
                case "IDASSET_EQUITY":
                    @objectItem = Cst.OTCml_TBL.ASSET_EQUITY.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetEquity);
                    pColumnPK = "IDASSET";
                    break;
                case "IDASSET_FXRATE":
                    @objectItem = Cst.OTCml_TBL.ASSET_FXRATE.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetFxRate);
                    pColumnPK = "IDASSET";
                    break;
                case "IDASSET_COMMODITY":
                    @objectItem = Cst.OTCml_TBL.ASSET_COMMODITY.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetCommodity);
                    pColumnPK = "IDASSET";
                    break;
                case "IDASSET_RATEINDEX":
                    @objectItem = Cst.OTCml_TBL.ASSET_RATEINDEX.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetRateIndex);
                    pColumnPK = "IDASSET";
                    break;
                case "IDASSET_INDEX":
                    @objectItem = Cst.OTCml_TBL.ASSET_INDEX.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetIndex);
                    pColumnPK = "IDASSET";
                    break;
                case "IDASSET_EXTRDFUND":
                    @objectItem = Cst.OTCml_TBL.ASSET_EXTRDFUND.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.AssetExchangeTradedFund);
                    pColumnPK = "IDASSET";
                    break;
                case "IDMATURITYRULE":
                    @objectItem = Cst.OTCml_TBL.MATURITYRULE.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.MATURITYRULE);
                    pColumnPK = "IDMATURITYRULE";
                    break;
                case "IDFEE":
                    @objectItem = Cst.OTCml_TBL.FEE.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.Fee);
                    break;
                case "IDFEEMATRIX":
                    @objectItem = Cst.OTCml_TBL.FEEMATRIX.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.FeeMatrix);
                    break;
                case "IDFEESCHEDULE":
                    @objectItem = Cst.OTCml_TBL.FEESCHEDULE.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.FeeSchedule);
                    break;
                case "IDCNFMESSAGE":
                    OpenReferentialFormArguments.GetObject(cs, Cst.OTCml_TBL.CNFMESSAGE, pColumnValue, out @objectItem, out menu);
                    idMenu = IdMenu.GetIdMenu(menu.Value);
                    break;
                case "IDSTLMESSAGE":
                    @objectItem = Cst.OTCml_TBL.STLMESSAGE.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.STLMESSAGE);
                    break;
                case "IDIOTASK":
                    @objectItem = Cst.OTCml_TBL.IOTASK.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IOTASK);
                    break;
                case "IDIOTASKDET":
                    @objectItem = Cst.OTCml_TBL.IOTASKDET.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IOTASKDET);
                    break;
                case "IDIOINPUT":
                    @objectItem = Cst.OTCml_TBL.IOINPUT.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IOINPUT);
                    break;
                case "IDIOOUTPUT":
                    @objectItem = Cst.OTCml_TBL.IOOUTPUT.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IOOUTPUT);
                    break;
                case "IDIOCOMPARE":
                    @objectItem = Cst.OTCml_TBL.IOCOMPARE.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IOCOMPARE);
                    break;
                case "IDIOSHELL":
                    @objectItem = Cst.OTCml_TBL.IOSHELL.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IOSHELL);
                    break;
                case "IDIOPARSING":
                    @objectItem = Cst.OTCml_TBL.IOPARSING.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IOPARSING);
                    break;
                case "IDIOPARAM":
                    @objectItem = Cst.OTCml_TBL.IOPARAM.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IOPARAM);
                    break;
                case "IDBC":
                    @objectItem = Cst.OTCml_TBL.BUSINESSCENTER.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.BusinessCenter);
                    break;
                case "IDINVOICINGRULES":
                    @objectItem = Cst.OTCml_TBL.INVOICINGRULES.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InvoicingRules);
                    break;
                case "IDMODELSAFETY":
                    @objectItem = Cst.OTCml_TBL.MODELSAFETY.ToString();
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.MODELSAFETY);
                    break;
            }
        }

        /// <summary>
        /// Retourne l'URL pour ouvrir un lien vers un referentiel
        /// </summary>
        /// <param name="pObject">Nom du référentiel</param>
        /// <param name="pIdMenu">Menu</param>
        /// <param name="pPkColumn">Nom de la colonne PK</param>
        /// <param name="pPkValue">Valeur de la colonne PK</param>
        /// <param name="pMode"></param>
        /// <param name="pFormId">Représente le formulaire appelant</param>
        /// <param name="pCondApp"></param>
        /// <returns></returns>
        /// FI 20140911 [XXXXX] Add Method
        /// FI 20160804 [Migration TFS] Modify
        private static string GetReferentialURLForHyperLink(string pObject, string pIdMenu, string pPkColumn, string pPkValue, Cst.ConsultationMode pMode, string pFormId, string pCondApp)
        {
            OpenReferentialFormArguments arg = new OpenReferentialFormArguments
            {
                // FI 20160804 [Migration TFS] 
                //arg.type = "Referential";
                Type = Cst.ListType.Repository.ToString(),
                @Object = pObject,
                IdMenu = pIdMenu,
                PK = pPkColumn,
                PKValue = pPkValue,
                Mode = pMode,
                CondApp = pCondApp,
                FormId = pFormId
            };
            return arg.GetURLOpenFormReferential();
        }

        /* ------------------------------------- */
        /* NOUVELLE VERSION DE CONSTRUCTION URL  */
        /* ------------------------------------- */
        // EG 20170125 [Refactoring URL] 
        /// <summary>
        /// hRef = URL sur Hyperlink
        /// ondblclick = URL sur ondblclick (dans window.open)
        /// </summary>
        // EG 20170125 [Refactoring URL] New
        public enum LinkEvent
        {
            href,
            ondblclick,
        }

        /// <summary>
        /// Type de link
        /// </summary>
        // EG 20170125 [Refactoring URL] New
        // EG 20190308 Add InputClosingReopeningPosition
        public enum LinkType
        {
            [System.Xml.Serialization.XmlEnumAttribute("CAI")]
            [PageLinkAttribute(Name = "CorporateActionIssue.aspx", Menu = IdMenu.Menu.InputCorporateActionIssue, PK = "IDCAISSUE", Mode = Cst.ConsultationMode.Normal)]
            InputCorporateActionIssue,
            [System.Xml.Serialization.XmlEnumAttribute("CAE")]
            [PageLinkAttribute(Name = "CorporateActionIssue.aspx", Menu = IdMenu.Menu.InputCorporateActionEmbedded, PK = "IDCA", Mode = Cst.ConsultationMode.Normal)]
            InputCorporateActionEmbedded,
            [System.Xml.Serialization.XmlEnumAttribute("CRP")]
            [PageLinkAttribute(Name = "ClosingReopeningPosition.aspx", Menu = IdMenu.Menu.InputClosingReopeningPosition, PK = "IDARQ", Mode = Cst.ConsultationMode.Normal)]
            InputClosingReopeningPosition,
            [System.Xml.Serialization.XmlEnumAttribute("IEV")]
            [PageLinkAttribute(Name = "EventCapturePage.aspx", Menu = IdMenu.Menu.InputEvent, PK = "IDE", FK = "IDT", Mode = Cst.ConsultationMode.Normal)]
            InputEvent,
            [System.Xml.Serialization.XmlEnumAttribute("ITR")]
            [PageLinkAttribute(Name = "TradeCapturePage.aspx", Menu = IdMenu.Menu.InputTrade, PK = "IDT", Mode = Cst.ConsultationMode.Normal)]
            InputTrade,
            [System.Xml.Serialization.XmlEnumAttribute("ITA")]
            [PageLinkAttribute(Name = "TradeAdminCapturePage.aspx", Menu = IdMenu.Menu.InputTradeAdmin, PK = "IDT", Mode = Cst.ConsultationMode.Normal)]
            InputTradeAdmin,
            [System.Xml.Serialization.XmlEnumAttribute("IDS")]
            [PageLinkAttribute(Name = "DebtSecCapturePage.aspx", Menu = IdMenu.Menu.InputDebtSec, PK = "IDT", Mode = Cst.ConsultationMode.Normal)]
            InputDebtSecurity,
            [System.Xml.Serialization.XmlEnumAttribute("ICB")]
            [PageLinkAttribute(Name = "TradeRiskCapturePage.aspx", Menu = IdMenu.Menu.InputTradeRisk_CashBalance, PK = "IDT", Mode = Cst.ConsultationMode.Normal)]
            InputCashBalance,
            [System.Xml.Serialization.XmlEnumAttribute("ICI")]
            [PageLinkAttribute(Name = "Referential.aspx", Menu = IdMenu.Menu.InputTradeRisk_CashInterest, PK = "IDT", Mode = Cst.ConsultationMode.Normal)]
            InputCashInterest,
            [System.Xml.Serialization.XmlEnumAttribute("ICP")]
            [PageLinkAttribute(Name = "TradeRiskCapturePage.aspx", Menu = IdMenu.Menu.InputTradeRisk_CashPayment, PK = "IDT", Mode = Cst.ConsultationMode.Normal)]
            InputCashPayment,
            [System.Xml.Serialization.XmlEnumAttribute("IIM")]
            [PageLinkAttribute(Name = "TradeRiskCapturePage.aspx", Menu = IdMenu.Menu.InputTradeRisk_InitialMargin, PK = "IDT", Mode = Cst.ConsultationMode.Normal)]
            InputInitialMargin,
            [System.Xml.Serialization.XmlEnumAttribute("REP")]
            [PageLinkAttribute(Name = "Referential.aspx", Mode = Cst.ConsultationMode.Normal)]
            Repository,
            [System.Xml.Serialization.XmlEnumAttribute("LOG")]
            [PageLinkAttribute(Name = "List.aspx", ListType = Cst.ListType.Log, Event = LinkEvent.ondblclick, Mode = Cst.ConsultationMode.Normal)]
            Log,
            [System.Xml.Serialization.XmlEnumAttribute("LST")]
            [PageLinkAttribute(Name = "List.aspx", ListType = Cst.ListType.Consultation, Event = LinkEvent.ondblclick, Mode = Cst.ConsultationMode.Normal)]
            List,
            //[System.Xml.Serialization.XmlEnumAttribute("PRI")]
            //[PageLinkAttribute(Name = "Referential.aspx", ListType = Cst.ListType.Price, Event = LinkEvent.ondblclick, PK = "IDQUOTE_H", Mode = Cst.ConsultationMode.Normal)]
            //Price,
        }



        // EG 20170125 [Refactoring URL] New
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        public static string GetURL(Nullable<IdMenu.Menu> pIdMenu, OpenReferentialFormArguments pArgs)
        {
            string url = "hyperlink.aspx?args=" + ReflectionTools.ConvertEnumToString<LinkType>(LinkType.Repository) + "|" + pIdMenu.Value.ToString() + "|";

            if (StrFunc.IsFilled(pArgs.PKValue))
                url += pArgs.PKValue + "|";

            if (StrFunc.IsFilled(pArgs.FKValue))
                url += pArgs.FKValue;

            if (url.EndsWith("|"))
                url = url.Substring(0, url.Length - 1);

            if (ArrFunc.IsFilled(pArgs.DynamicArg))
            {
                url += "&DA=";
                pArgs.DynamicArg.ToList().ForEach(item => { url += HttpUtility.UrlEncode(Ressource.EncodeDA(item)) + StrFunc.StringArrayList.LIST_SEPARATOR; });
            }

            if (StrFunc.IsFilled(pArgs.CondApp))
                url += StrFunc.AppendFormat("&CondApp={0}", pArgs.CondApp);

            if (ArrFunc.IsFilled(pArgs.Param))
            {
                for (int i = 0; i < pArgs.Param.Length; i++)
                {
                    url += "&P" + (i + 1).ToString() + "=" + HttpUtility.UrlEncode(pArgs.Param[i]);
                }
            }

            if (Cst.ConsultationMode.Normal != pArgs.Mode)
                url += "&M=" + Convert.ToInt32(pArgs.Mode).ToString();

            if (StrFunc.IsFilled(pArgs.DS))
                url += "&DS=" + HttpUtility.UrlEncode(pArgs.DS);

            if (StrFunc.IsFilled(pArgs.FormId))
                url += "&F=" + HttpUtility.UrlEncode(pArgs.FormId);

            return JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_FormReferential);
        }
        // EG 20170125 [Refactoring URL] New
        public static string GetURL(Nullable<IdMenu.Menu> pIdMenu, string pPKValue)
        {
            return GetURL(pIdMenu, pPKValue, LinkEvent.href, Cst.ConsultationMode.Normal);
        }
        // EG 20170125 [Refactoring URL] New
        public static string GetURL(Nullable<IdMenu.Menu> pIdMenu, string pPKValue, Cst.ConsultationMode pMode)
        {
            return GetURL(pIdMenu, pPKValue, LinkEvent.href, pMode);
        }
        // EG 20170125 [Refactoring URL] New
        public static string GetURL(Nullable<IdMenu.Menu> pIdMenu, string pPKValue, LinkEvent pLinkEvent, Cst.ConsultationMode pMode)
        {
            return GetURL(pIdMenu, pPKValue, null, pLinkEvent, pMode, string.Empty, string.Empty);
        }
        /// EG 20170125 [Refactoring URL] New
        /// FI 20170906 [23401] Modify
        // EG 20190308 Add InputClosingReopeningPosition
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        public static string GetURL(Nullable<IdMenu.Menu> pIdMenu, string pPKValue, string pFKValue,
            LinkEvent pLinkEvent, Cst.ConsultationMode pMode, string pDynamicArgs, string pDataCacheKeyDsData)
        {
            string url = string.Empty;

            bool isMenu = false;
            string pkValue = HttpUtility.UrlEncode(pPKValue, Encoding.Default);
            string fkValue = HttpUtility.UrlEncode(pFKValue, Encoding.Default);
            if (pIdMenu.HasValue)
            {
                LinkType linkType;
                switch (pIdMenu.Value)
                {
                    case IdMenu.Menu.InputDebtSec: // FI 20170906 [23401] add
                        linkType = LinkType.InputDebtSecurity;
                        break;
                    case IdMenu.Menu.InputCorporateActionEmbedded:
                        linkType = LinkType.InputCorporateActionEmbedded;
                        break;
                    case IdMenu.Menu.InputCorporateActionIssue:
                        linkType = LinkType.InputCorporateActionIssue;
                        break;
                    case IdMenu.Menu.InputClosingReopeningPosition:
                        linkType = LinkType.InputClosingReopeningPosition;
                        break;
                    case IdMenu.Menu.InputEvent:
                        linkType = LinkType.InputEvent;
                        break;
                    case IdMenu.Menu.InputTrade:
                        linkType = LinkType.InputTrade;
                        break;
                    case IdMenu.Menu.InputTradeAdmin:
                        linkType = LinkType.InputTradeAdmin;
                        break;
                    case IdMenu.Menu.InputTradeRisk_CashBalance:
                        linkType = LinkType.InputCashBalance;
                        break;
                    case IdMenu.Menu.InputTradeRisk_CashInterest:
                        linkType = LinkType.InputCashInterest;
                        break;
                    case IdMenu.Menu.InputTradeRisk_CashPayment:
                        linkType = LinkType.InputCashPayment;
                        break;
                    case IdMenu.Menu.InputTradeRisk_InitialMargin:
                        linkType = LinkType.InputInitialMargin;
                        break;
                    case IdMenu.Menu.LogEndOfDay:
                    case IdMenu.Menu.LogClosingDay:
                    case IdMenu.Menu.TrackerPosRequest:
                    case IdMenu.Menu.SERVICE_L:
                    // FI 20210308 [XXXXX] Add IOTRACK et IOTRACKCOMPARE
                    case IdMenu.Menu.IOTRACK:
                    case IdMenu.Menu.IOTRACKCOMPARE:
                        isMenu = true;
                        linkType = LinkType.Log;
                        break;
                    case IdMenu.Menu.PosSynthesis:
                    case IdMenu.Menu.PosSynthesis_OTC:
                    case IdMenu.Menu.PosSynthesisDet:
                    case IdMenu.Menu.PosSynthesisDet_OTC:
                    case IdMenu.Menu.EntityMarket:
                        isMenu = true;
                        linkType = LinkType.List;
                        break;
                    case IdMenu.Menu.Repository:
                    default:
                        isMenu = true;
                        linkType = LinkType.Repository;
                        break;
                }

                url = "hyperlink.aspx?args=" + ReflectionTools.ConvertEnumToString<LinkType>(linkType) + "|";

                if (isMenu)
                    url += pIdMenu.Value.ToString() + "|";

                if (StrFunc.IsFilled(pkValue))
                    url += pkValue + "|";

                if (StrFunc.IsFilled(fkValue))
                    url += fkValue;

                if (url.EndsWith("|"))
                    url = url.Substring(0, url.Length - 1);

                if (Cst.ConsultationMode.Normal != pMode)
                    url += "&M=" + Convert.ToInt32(pMode).ToString();

                if (StrFunc.IsFilled(pDataCacheKeyDsData))
                    url += "&DS=" + HttpUtility.UrlEncode(pDataCacheKeyDsData, Encoding.Default); ;

                if (StrFunc.IsFilled(pDynamicArgs))
                    url += pDynamicArgs;

                if (pLinkEvent == LinkEvent.ondblclick)
                    url = JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_FormReferential);
            }
            return url;
        }


        /// <summary>
        /// Obtient le menu en fonction du nom de colonne (Exemple IDA=> Menu ACTOR)
        /// </summary>
        /// <param name="pColumn"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// EG 20170125 [Refactoring URL] New
        /// FI 20170313 [22225] Modify
        /// FI 20170908 [23409] Modify
        /// FI 20170928 [23452] Modify
        /// EG 20210330 [XXXXX] Gestion link ligne de Tracker sur POSCOLLATERAL
        public static Nullable<IdMenu.Menu> GetMenu_Repository(string pColumn, string pData)
        {
            Nullable<IdMenu.Menu> idMenu = null;
            string cs = CSTools.SetCacheOn(SessionTools.CS);
            switch (pColumn)
            {
                case "IDA":
                    // FI 20170928 [23452] Appel a OpenReferentialFormArguments.GetObject
                    //idMenu = IdMenu.Menu.Actor;
                    OpenReferentialFormArguments.GetObject(cs, Cst.OTCml_TBL.ACTOR, pData, out _, out idMenu);
                    break;
                case "IDGACTOR":
                    idMenu = IdMenu.Menu.GrpActor;
                    break;
                case "IDB":
                    idMenu = IdMenu.Menu.Book;
                    break;
                case "IDGBOOK":
                    idMenu = IdMenu.Menu.GrpBook;
                    break;
                case "IDI":
                    // FI 20240703 [WI989] Call GetObject
                    OpenReferentialFormArguments.GetObject(cs, Cst.OTCml_TBL.INSTRUMENT, pData, out _, out idMenu);
                    break;
                case "IDGINSTR":
                    idMenu = IdMenu.Menu.GrpInstrument;
                    break;
                case "IDP":
                    idMenu = IdMenu.Menu.Product;
                    break;
                case "IDRX":
                    // FI 20190404 [XXXXX] 
                    //idMenu = IdMenu.Menu.AssetRateIndex;
                    idMenu = IdMenu.Menu.RateIndex;
                    break;
                case "IDM":
                    idMenu = IdMenu.Menu.Market;
                    break;
                case "IDGMARKET":
                    idMenu = IdMenu.Menu.GrpMarket;
                    break;
                case "IDDC":
                    idMenu = IdMenu.Menu.DrvContract;
                    break;
                case "IDCC":
                    idMenu = IdMenu.Menu.CommodityContract;
                    break;
                case "IDGCONTRACT": // FI 20170908 [23409] Add
                    idMenu = IdMenu.Menu.GrpContract;
                    break;
                case "IDDERIVATIVEATTRIB": // FI 20170313 [22225]  add
                    idMenu = IdMenu.Menu.DrvAttrib;
                    break;
                case "IDASSET_CASH":
                    idMenu = IdMenu.Menu.AssetCash;
                    break;
                case "IDASSET_ETD":
                    idMenu = IdMenu.Menu.AssetETD;
                    break;
                case "IDASSET_EQUITY":
                    idMenu = IdMenu.Menu.AssetEquity;
                    break;
                case "IDASSET_FXRATE":
                    idMenu = IdMenu.Menu.AssetFxRate;
                    break;
                case "IDASSET_COMMODITY":
                    idMenu = IdMenu.Menu.AssetCommodity;
                    break;
                case "IDASSET_RATEINDEX":
                    idMenu = IdMenu.Menu.AssetRateIndex;
                    break;
                case "IDASSET_INDEX":
                    idMenu = IdMenu.Menu.AssetIndex;
                    break;
                case "IDASSET_EXTRDFUND":
                    idMenu = IdMenu.Menu.AssetExchangeTradedFund;
                    break;
                case "IDMATURITYRULE":
                    idMenu = IdMenu.Menu.MATURITYRULE;
                    break;
                case "IDFEE":
                    idMenu = IdMenu.Menu.Fee;
                    break;
                case "IDFEEMATRIX":
                    idMenu = IdMenu.Menu.FeeMatrix;
                    break;
                case "IDFEESCHEDULE":
                    idMenu = IdMenu.Menu.FeeSchedule;
                    break;
                case "IDCNFMESSAGE":
                    OpenReferentialFormArguments.GetObject(cs, Cst.OTCml_TBL.CNFMESSAGE, pData, out _, out idMenu);
                    break;
                case "IDSTLMESSAGE":
                    idMenu = IdMenu.Menu.STLMESSAGE;
                    break;
                case "IDIOTASK":
                    idMenu = IdMenu.Menu.IOTASK;
                    break;
                case "IDIOTASKDET":
                    idMenu = IdMenu.Menu.IOTASKDET;
                    break;
                case "IDIOINPUT":
                    idMenu = IdMenu.Menu.IOINPUT;
                    break;
                case "IDIOOUTPUT":
                    idMenu = IdMenu.Menu.IOOUTPUT;
                    break;
                case "IDIOCOMPARE":
                    idMenu = IdMenu.Menu.IOCOMPARE;
                    break;
                case "IDIOSHELL":
                    idMenu = IdMenu.Menu.IOSHELL;
                    break;
                case "IDIOPARSING":
                    idMenu = IdMenu.Menu.IOPARSING;
                    break;
                case "IDIOPARAM":
                    idMenu = IdMenu.Menu.IOPARAM;
                    break;
                case "IDBC":
                    idMenu = IdMenu.Menu.BusinessCenter;
                    break;
                case "IDINVOICINGRULES":
                    idMenu = IdMenu.Menu.InvoicingRules;
                    break;
                case "IDPR":
                    idMenu = IdMenu.Menu.LogPosRequest;
                    break;
                case "IDMODELACTOR":
                    idMenu = IdMenu.Menu.FilterActor;
                    break;
                case "IDMODELINSTRUMENT":
                    idMenu = IdMenu.Menu.FilterInstrument;
                    break;
                case "IDMODELMARKET":
                    idMenu = IdMenu.Menu.FilterMarket;
                    break;
                case "IDMODELMENU":
                    idMenu = IdMenu.Menu.FilterMenu;
                    break;
                case "IDMODELPERMISSION":
                    idMenu = IdMenu.Menu.FilterPermission;
                    break;
                case "IDPOSCOLLATERAL":
                    idMenu = IdMenu.Menu.POSCOLLATERAL;
                    break;
                case "IDMODELSAFETY":
                    idMenu = IdMenu.Menu.MODELSAFETY;
                    break;
                case "IDDEFINEEXTEND":
                    idMenu = IdMenu.Menu.DefineExtend;
                    break;
            }
            return idMenu;
        }

        /// <summary>
        /// Retourne la valeur de la colonne ELEMENTTYPE
        /// </summary>
        /// <param name="pDataGridItem"></param>
        /// <returns></returns>
        // EG 20170125 [Refactoring URL] New
        public static Nullable<IdMenu.Menu> GetMenu_IO(DataRow pRow, int pIndexElementType)
        {
            Nullable<IdMenu.Menu> idMenu = null;
            if ((null != pRow) && (-1 < pIndexElementType))
            {
                string elementType = pRow.ItemArray[pIndexElementType].ToString();
                if (Enum.IsDefined(typeof(Cst.IOElementType), elementType))
                {
                    Cst.IOElementType IOElementType = (Cst.IOElementType)Enum.Parse(typeof(Cst.IOElementType), elementType);
                    switch (IOElementType)
                    {
                        case Cst.IOElementType.INPUT:
                            idMenu = IdMenu.Menu.IOINPUT;
                            break;
                        case Cst.IOElementType.OUTPUT:
                            idMenu = IdMenu.Menu.IOOUTPUT;
                            break;
                        case Cst.IOElementType.COMPARE:
                            idMenu = IdMenu.Menu.IOCOMPARE;
                            break;
                        case Cst.IOElementType.SHELL:
                            idMenu = IdMenu.Menu.IOSHELL;
                            break;
                    }
                }
            }
            return idMenu;
        }
        // EG 20170125 [Refactoring URL] New
        public static Nullable<IdMenu.Menu> GetMenu_IO(string pTableName)
        {
            Nullable<IdMenu.Menu> idMenu = null;
            try
            {
                Cst.OTCml_TBL tableName = (Cst.OTCml_TBL)System.Enum.Parse(typeof(Cst.OTCml_TBL), pTableName, true);
                switch (tableName)
                {
                    case Cst.OTCml_TBL.IOTASK:
                        idMenu = IdMenu.Menu.IOTASK;
                        break;
                    case Cst.OTCml_TBL.IOTASKDET:
                        idMenu = IdMenu.Menu.IOTASKDET;
                        break;
                    case Cst.OTCml_TBL.IOINPUT:
                        idMenu = IdMenu.Menu.IOINPUT;
                        break;
                    case Cst.OTCml_TBL.IOOUTPUT:
                        idMenu = IdMenu.Menu.IOOUTPUT;
                        break;
                    case Cst.OTCml_TBL.IOCOMPARE:
                        idMenu = IdMenu.Menu.IOCOMPARE;
                        break;
                    case Cst.OTCml_TBL.IOSHELL:
                        idMenu = IdMenu.Menu.IOSHELL;
                        break;
                    case Cst.OTCml_TBL.IOPARSING:
                        idMenu = IdMenu.Menu.IOPARSING;
                        break;
                    case Cst.OTCml_TBL.IOPARAM:
                        idMenu = IdMenu.Menu.IOPARAM;
                        break;
                }
            }
            catch { }
            return idMenu;
        }

        // EG 20170125 [Refactoring URL] New
        // EG 20200923 [XXXXX] Correction Mauvais lien Hyperlink sur Trade Risk (ajout INDENTIFIER du PRODUCT)
        public static Nullable<IdMenu.Menu> GetMenu_Trade(string pColumn, string pTableName)
        {
            return GetMenu_Trade(pColumn, pTableName, null, -1, -1);
        }
        // EG 20170125 [Refactoring URL] New
        // EG 20200923 [XXXXX] Correction Mauvais lien Hyperlink sur Trade Risk (ajout INDENTIFIER du PRODUCT)
        public static Nullable<IdMenu.Menu> GetMenu_Trade(string pColumn, string pTableName, DataRow pRow, int pIndexGProduct, int pIndexProduct)
        {
            Nullable<IdMenu.Menu> idMenu = null;
            switch (pColumn)
            {
                case "IDT":
                    idMenu = GetMenu_TradeByProduct(pRow, pIndexGProduct, pIndexProduct, pTableName);
                    break;
                case "IDT_ADMIN":
                    idMenu = IdMenu.Menu.InputTradeAdmin;
                    break;
                case "IDT_RISK":
                    idMenu = IdMenu.Menu.InputTradeRisk;
                    break;
                case "IDT_DEBSEC":
                    idMenu = IdMenu.Menu.InputDebtSec;
                    break;
                case "IDT_TRADE":
                    idMenu = IdMenu.Menu.InputTrade;
                    break;
            }
            return idMenu;
        }

        // EG 20170125 [Refactoring URL] New
        public static Nullable<IdMenu.Menu> GetMenu_Asset(string pColumn, string pTableName)
        {
            return GetMenu_Asset(pColumn, pTableName, null);
        }
        // EG 20170125 [Refactoring URL] New
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public static Nullable<IdMenu.Menu> GetMenu_Asset(string pColumn, string pTableName, Nullable<Cst.UnderlyingAsset> pAssetCategory)
        {
            Nullable<IdMenu.Menu> idMenu = null;
            if (false == pAssetCategory.HasValue)
                pAssetCategory = GetAssetCategory2(pTableName);

            if (pAssetCategory.HasValue)
            {
                switch (pAssetCategory)
                {
                    case Cst.UnderlyingAsset.Cash:
                        idMenu = IdMenu.Menu.AssetCash;
                        break;

                    case Cst.UnderlyingAsset.Commodity:
                        idMenu = (pColumn == "IDCONTRACT") ? IdMenu.Menu.CommodityContract : IdMenu.Menu.AssetCommodity;
                        break;
                    case Cst.UnderlyingAsset.ExchangeTradedContract:
                    case Cst.UnderlyingAsset.Future:
                        idMenu = (pColumn == "IDCONTRACT") ? IdMenu.Menu.DrvContract : IdMenu.Menu.AssetETD;
                        break;

                    case Cst.UnderlyingAsset.EquityAsset:
                        idMenu = IdMenu.Menu.AssetEquity;
                        break;

                    case Cst.UnderlyingAsset.ExchangeTradedFund:
                        idMenu = IdMenu.Menu.AssetExchangeTradedFund;
                        break;

                    case Cst.UnderlyingAsset.Index:
                        idMenu = IdMenu.Menu.AssetIndex;
                        break;

                    case Cst.UnderlyingAsset.FxRateAsset:
                        idMenu = IdMenu.Menu.AssetFxRate;
                        break;

                    case Cst.UnderlyingAsset.RateIndex:
                        idMenu = IdMenu.Menu.AssetRateIndex;
                        break;
                    case Cst.UnderlyingAsset.Bond:
                    case Cst.UnderlyingAsset.ConvertibleBond:
                        idMenu = IdMenu.Menu.InputDebtSec;
                        break;
                    case Cst.UnderlyingAsset.Deposit:
                    case Cst.UnderlyingAsset.MutualFund:
                    case Cst.UnderlyingAsset.SimpleFra:
                    //case Cst.UnderlyingAsset.Bond:
                    //case Cst.UnderlyingAsset.ConvertibleBond:
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pAssetCategory));
                }
            }
            return idMenu;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableName"></param>
        /// <returns></returns>
        /// FI 20190718 [XXXXX] Add Method
        private static Nullable<Cst.UnderlyingAsset> GetAssetCategory2(string pTableName)
        {
            Nullable<Cst.UnderlyingAsset> assetCategory = null;

            try
            {
                Cst.OTCml_TBL tableName = (Cst.OTCml_TBL)System.Enum.Parse(typeof(Cst.OTCml_TBL), pTableName, true);
                UnderlyingAssetAttribute attrib = ReflectionTools.GetAttribute<UnderlyingAssetAttribute>(tableName);
                if (null != attrib)
                    assetCategory = attrib.UnderlyingAsset;

            }
            catch { }
            return assetCategory;

        }



        /// <summary>
        /// Retourne les menus qui affichent des contracts 
        /// </summary>
        /// <param name="pColumn"></param>
        /// <param name="pContractCategory">Doit être renseignée si le nom de colonne est IDXC</param>
        /// <returns></returns>
        /// FI 20170908 [23409] Add
        public static Nullable<IdMenu.Menu> GetMenu_Contract(string pColumn, Nullable<Cst.ContractCategory> pContractCategory)
        {
            Nullable<IdMenu.Menu> idMenu = null;
            switch (pColumn)
            {
                case "IDCC":
                    idMenu = IdMenu.Menu.CommodityContract;
                    break;
                case "IDDC":
                    idMenu = IdMenu.Menu.DrvContract;
                    break;
                case "IDXC":
                    if (pContractCategory.HasValue)
                    {
                        switch (pContractCategory.Value)
                        {
                            case Cst.ContractCategory.DerivativeContract:
                                idMenu = IdMenu.Menu.DrvContract;
                                break;
                            case Cst.ContractCategory.CommodityContract:
                                idMenu = IdMenu.Menu.CommodityContract;
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Contract Category:{0} is not implemented", pContractCategory.Value.ToString()));
                        }
                    }
                    break;
            }
            return idMenu;
        }



        // EG 20170125 [Refactoring URL] New
        //public static Nullable<IdMenu.Menu> GetMenu_Zoom(DataGridItem pDataGridItem, string pObjectName, string pColumn, string pTableName, int pIndexGProduct)
        // EG 20200923 [XXXXX] Correction Mauvais lien Hyperlink sur Trade Risk (ajout INDENTIFIER du PRODUCT)
        public static Nullable<IdMenu.Menu> GetMenu_Zoom(DataRow pRow, string pObjectName, string pTableName, int pIndexGProduct, int pIndexProduct)
        {
            Nullable<IdMenu.Menu> idMenu = null;
            switch (pObjectName)
            {
                case "TRADE":
                    idMenu = IdMenu.Menu.InputTrade;
                    break;
                case "TRADEADMIN":
                    idMenu = IdMenu.Menu.InputTradeAdmin;
                    break;
                case "CASHBALANCE":
                    idMenu = IdMenu.Menu.InputTradeRisk_CashBalance;
                    break;
                case "CAHINTEREST":
                    idMenu = IdMenu.Menu.InputTradeRisk_CashInterest;
                    break;
                case "CASHPAYMENT":
                    idMenu = IdMenu.Menu.InputTradeRisk_CashPayment;
                    break;
                case "DEBTSECURITY":
                    idMenu = IdMenu.Menu.InputDebtSec;
                    break;
                case "RISKPERFORMANCE":
                    idMenu = IdMenu.Menu.InputTradeRisk_InitialMargin;
                    break;
                case "TRADERISK":
                    idMenu = IdMenu.Menu.InputTradeRisk;
                    break;
                case "POSSYNT_ALLOC":
                    idMenu = IdMenu.Menu.PosSynthesisDet;
                    break;
                case "POSSYNT_OTC_ALLOC":
                    idMenu = IdMenu.Menu.PosSynthesisDet_OTC;
                    break;
                case "POSDET":
                case "POSDETOTC":
                case "POSOPTDET":
                    break;
                default:
                    idMenu = GetMenu_TradeByProduct(pRow, pIndexGProduct, pIndexProduct, pTableName);
                    break;
            }
            return idMenu;
        }

        // EG 20170125 [Refactoring URL] New
        //private static Nullable<ProductTools.GroupProductEnum> GetGroupProduct(DataGridItem pDataGridItem, int pIndexGProduct)
        private static Nullable<ProductTools.GroupProductEnum> GetGroupProduct(DataRow pRow, int pIndexGProduct)
        {
            Nullable<ProductTools.GroupProductEnum> groupProduct = null;
            //if ((null != pDataGridItem) && (-1 < pIndexGProduct))
            //{
            //    DataRow row = ((DataRowView)(pDataGridItem.DataItem)).Row;
            //    string product = row.ItemArray[pIndexGProduct].ToString();
            //    groupProduct = (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), product);
            //}
            if ((null != pRow) && (-1 < pIndexGProduct))
            {
                string product = pRow.ItemArray[pIndexGProduct].ToString();
                if (StrFunc.IsFilled(product))
                    groupProduct = (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), product);
            }
            return groupProduct;
        }

        // EG 20170125 [Refactoring URL] New
        // EG 20200923 [XXXXX] Correction Mauvais lien Hyperlink sur Trade Risk (ajout INDENTIFIER du PRODUCT)
        private static Nullable<IdMenu.Menu> GetMenu_TradeByProduct(DataRow pRow, int pIndexGProduct, int pIndexProduct, string pTableName)
        {
            // EG 20170215 Set idMenu = IdMenu.Menu.InputTrade (byDefault)
            Nullable<IdMenu.Menu> idMenu = IdMenu.Menu.InputTrade;
            //Nullable<ProductTools.GroupProductEnum> groupProduct = GetGroupProduct(pDataGridItem, pIndexGProduct);
            Nullable<ProductTools.GroupProductEnum> groupProduct = GetGroupProduct(pRow, pIndexGProduct);
            if (groupProduct.HasValue)
            {
                switch (groupProduct.Value)
                {
                    case ProductTools.GroupProductEnum.Risk:
                        idMenu = IdMenu.Menu.InputTradeRisk_InitialMargin;
                        if ((null != pRow) && (-1 < pIndexProduct))
                        {
                            string productIdentifier = pRow.ItemArray[pIndexProduct].ToString().ToLower();
                            switch (productIdentifier)
                            {
                                case "cashbalance":
                                    idMenu = IdMenu.Menu.InputTradeRisk_CashBalance;
                                    break;
                                case "cashbalanceinterest":
                                    idMenu = IdMenu.Menu.InputTradeRisk_CashInterest;
                                    break;
                                case "cashpayment":
                                    idMenu = IdMenu.Menu.InputTradeRisk_CashPayment;
                                    break;
                                case "marginrequirement":
                                    idMenu = IdMenu.Menu.InputTradeRisk_InitialMargin;
                                    break;
                            }
                        }
                        break;
                    case ProductTools.GroupProductEnum.Asset:
                        idMenu = IdMenu.Menu.InputDebtSec;
                        break;
                    case ProductTools.GroupProductEnum.Administrative:
                        idMenu = IdMenu.Menu.InputTradeAdmin;
                        break;
                    default:
                        idMenu = IdMenu.Menu.InputTrade;
                        break;
                }
            }
            else
            {
                try
                {
                    if (Enum.IsDefined(typeof(Cst.OTCml_TBL), pTableName))
                    {
                        Cst.OTCml_TBL tableName = (Cst.OTCml_TBL)System.Enum.Parse(typeof(Cst.OTCml_TBL), pTableName, true);
                        switch (tableName)
                        {
                            case Cst.OTCml_TBL.QUOTE_DEBTSEC_H:
                            case Cst.OTCml_TBL.TRADEDEBTSEC:
                            case Cst.OTCml_TBL.TRADEASSET:
                                idMenu = IdMenu.Menu.InputDebtSec;
                                break;
                            case Cst.OTCml_TBL.TRADEADMIN:
                                idMenu = IdMenu.Menu.InputTradeAdmin;
                                break;
                            default:
                                idMenu = IdMenu.Menu.InputTrade;
                                break;
                        }
                    }
                }
                catch { }
            }
            return idMenu;
        }

        // EG 20170125 [Refactoring URL] New
        private static string[] GetQueryStringParam(NameValueCollection pQueryString)
        {
            string[] param = null;
            int length = 0;
            if (ArrFunc.IsFilled(pQueryString))
            {
                for (int i = 0; i < 32; i++) // 32 params Max
                {
                    if (null == pQueryString["P" + (i + 1).ToString()])
                    {
                        length = i;
                        break;
                    }
                }

                if (0 < length)
                {
                    param = new string[length];
                    for (int i = 0; i < param.Length; i++)
                    {
                        param[i] = pQueryString["P" + (i + 1).ToString()];
                    }
                }
            }
            return param;
        }

        // EG 20170125 [Refactoring URL] New
        public static string PageToCall(NameValueCollection pQueryString)
        {
            string url = PageToCall(pQueryString["args"], pQueryString["M"], pQueryString["DA"], pQueryString["CondApp"], pQueryString["DS"],
                GetQueryStringParam(pQueryString), pQueryString["F"]);
            return url;
        }
        // EG 20170125 [Refactoring URL] New
        // FI 20170906 [23401] Modify
        // EG 20190308 Add InputClosingReopeningPosition
        public static string PageToCall(string pArgs, string pMode, string pDynamicArgs, string pCondApp, string pDataSetCache, string[] pParam, string pFormId)
        {
            string[] args = pArgs.Split('|');
            string url = string.Empty;

            if (ArrFunc.IsFilled(args))
            {
                LinkType linkType = (LinkType)ReflectionTools.EnumParse(new LinkType(), args[0]);
                PageLinkAttribute attribute = ReflectionTools.GetAttribute<PageLinkAttribute>(linkType);
                if (null != attribute)
                {
                    switch (linkType)
                    {
                        case LinkType.Repository:
                            if (Enum.IsDefined(typeof(IdMenu.Menu), args[1]))
                            {
                                attribute.Menu = (IdMenu.Menu)ReflectionTools.EnumParse(new IdMenu.Menu(), args[1]);
                                attribute.PKV = args[2];
                                attribute.SetRepositoryInfo();
                            }
                            else
                            {
                                attribute.PKV = args[1];
                            }
                            break;
                        case LinkType.InputCashBalance:
                        case LinkType.InputCashInterest:
                        case LinkType.InputCashPayment:
                        case LinkType.InputDebtSecurity:
                        case LinkType.InputInitialMargin:
                        case LinkType.InputTrade:
                        case LinkType.InputTradeAdmin:
                            attribute.PK = "IDT";
                            // FI 20170906 [23401] Add if
                            if (args.Length >= 2)
                                attribute.PKV = args[1];
                            break;
                        case LinkType.InputCorporateActionEmbedded:
                            attribute.PK = "IDCA";
                            attribute.PKV = args[1];
                            break;
                        case LinkType.InputCorporateActionIssue:
                            attribute.PK = "IDCAISSUE";
                            if (args.Length >= 2)
                                attribute.PKV = args[1];
                            break;
                        case LinkType.InputClosingReopeningPosition:
                            attribute.PK = "IDARQ";
                            if (args.Length >= 2)
                                attribute.PKV = args[1];
                            break;
                        case LinkType.Log:
                        case LinkType.List:
                            attribute.Menu = (IdMenu.Menu)ReflectionTools.EnumParse(new IdMenu.Menu(), args[1]);
                            attribute.FK = args[2];
                            if (attribute.Menu == IdMenu.Menu.TrackerPosRequest)
                            {
                                attribute.PK = "IDPR";
                                attribute.PKV = args[3];
                            }
                            attribute.SetListInfo();
                            break;
                    }
                    url = attribute.URL(linkType, pMode, pDynamicArgs, pCondApp, pDataSetCache, pParam, pFormId);
                }
            }
            return url;
        }

        /// <summary>
        /// Construction de lien final d'ouverture de page via le menu Summary
        /// La source est par exemple : hyperlink.aspx?mnu=4 
        /// où 4 est l'id système retourné par la query de chargement des menus.
        /// 1. On recherche la ligne de menu dans la variable de session : SessionTools.Menus
        /// 2. On récupère l'URL de lien et on la complète
        /// </summary>
        /// <param name="pArgs"></param>
        /// <returns></returns>
        // EG 20201116 [25556] New Réécriture du lien sur Hyperlink (Pb self.Close) 
        public static string MenuToCall(string pArgs)
        {
            string[] args = pArgs.Split('=');
            string ret = string.Empty;

            Menus menus = SessionTools.Menus;
            if (ArrFunc.IsFilled(args) && ArrFunc.IsFilled(menus))
            {
                string id = args[0];

                Menu.Menu currentMenu = menus.SelectByID(id);
                if (null != currentMenu)
                {
                    ret = currentMenu.Url;

                    if (currentMenu.IsAction)
                    {
                        string addingForUrlMenu = string.Empty;
                        bool isAddIDMenu = true;
                        if (currentMenu.IsExternal)
                        {
                            if (ret.ToUpper().StartsWith("LIST.ASPX"))
                                addingForUrlMenu = "&TitleMenu=" + System.Web.HttpUtility.UrlEncode(currentMenu.Description, System.Text.Encoding.UTF8);
                            else
                                isAddIDMenu = false;
                        }
                        if (isAddIDMenu)
                        {
                            ret += ((ret.IndexOf("?") < 0) ? "?" : "&") + "IDMenu=" + currentMenu.IdMenu + addingForUrlMenu;
                            if (ret.Contains("%%IDMENUSYS%%"))
                                ret = ret.Replace("%%IDMENUSYS%%", id);
                            ret += "&IDMenuSys=" + currentMenu.Id;
                        }
                    }
                }
            }
            return ret;
        }
    }


    #region PageLinkAttribute
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    // EG 20170125 [Refactoring URL] New
    public sealed class PageLinkAttribute : Attribute
    {
        #region Members
        private SpheresURL.LinkEvent m_Event;
        private string m_Name;
        private Nullable<Cst.ListType> m_Type;
        private Nullable<IdMenu.Menu> m_Menu;
        private string m_Object;
        private string m_PK;
        private string m_PKV;
        private string m_FK;
        private string m_FKV;
        private Cst.ConsultationMode m_Mode;
        private string m_Condition;
        private string m_InputMode;
        private string m_AdditionalInfo;
        private string m_DynamicArgs;

        private OpenReferentialArguments m_Args;

        #endregion Members

        #region Accessors
        public SpheresURL.LinkEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        public string Object
        {
            get { return m_Object; }
            set { m_Object = value; }
        }
        public Nullable<Cst.ListType> Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }
        public Cst.ListType ListType
        {
            get { return m_Type.Value; }
            set { m_Type = value; }
        }
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
        public IdMenu.Menu Menu
        {
            get { return m_Menu.Value; }
            set { m_Menu = value; }
        }
        public string PK
        {
            get { return m_PK; }
            set { m_PK = value; }
        }
        public string PKV
        {
            get { return m_PKV; }
            set { m_PKV = value; }
        }
        public string FK
        {
            get { return m_FK; }
            set { m_FK = value; }
        }
        public string FKV
        {
            get { return m_FKV; }
            set { m_FKV = value; }
        }
        public string AdditionalInfo
        {
            get { return m_AdditionalInfo; }
            set { m_AdditionalInfo = value; }
        }
        public string DynamicArgs
        {
            get { return m_DynamicArgs; }
            set { m_DynamicArgs = value; }
        }
        public Cst.ConsultationMode Mode
        {
            get { return m_Mode; }
            set { m_Mode = value; }
        }
        public string Condition
        {
            get { return m_Condition; }
            set { m_Condition = value; }
        }
        public string InputMode
        {
            get { return m_InputMode; }
            set { m_InputMode = value; }
        }

        public OpenReferentialArguments Args
        {
            get { return m_Args; }
            set { m_Args = value; }
        }

        // EG 20210308 [XXXXX] Modification Ordre Construction URL via Hyperlink.aspx pour Contrôle CheckURL
        public string URL(SpheresURL.LinkType pLinkType, string pMode, string pDynamicArgs, string pCondApp, string pDataSetCache, string[] pParam, string pFormId)
        {
            string url = Name;

            if ((pLinkType == SpheresURL.LinkType.List) || (pLinkType == SpheresURL.LinkType.Log))
            {
                if (Type.HasValue)
                    url += "?" + Type.Value.ToString() + "=" + Object;
            }
            else
            {
                if (Type.HasValue)
                    url += "?T=" + Type.Value;
                if (StrFunc.IsFilled(Object))
                    url += "&O=" + Object;
            }

            if (StrFunc.IsFilled(AdditionalInfo))
                url += AdditionalInfo;

            if (ArrFunc.IsFilled(pParam))
            {
                for (int i = 0; i < pParam.Length; i++)
                {
                    url += "&P" + (i + 1).ToString() + "=" + pParam[i];
                }
            }


            url = AspTools.AddIdMenuOnUrl(url, IdMenu.GetIdMenu(Menu));

            if (StrFunc.IsFilled(PKV))
                url += "&PK=" + PK + "&PKV=" + PKV;

            if (StrFunc.IsFilled(FK))
                url += "&FK=" + FK + "&FKV=" + FKV;

            if (StrFunc.IsFilled(pMode))
                url += "&M=" + pMode;
            else if (m_Mode != Cst.ConsultationMode.Normal)
                url += "&M=" + Convert.ToInt32(m_Mode).ToString();

            if (StrFunc.IsFilled(pDynamicArgs))
                url += "&DA=" + pDynamicArgs;

            // FI 20240703 [WI989] Prise en compte de la propriété Condition lorsque renseignnée
            if (StrFunc.IsEmpty(pCondApp) && StrFunc.IsFilled(Condition))
                url += "&CondApp=" + Condition;
            else if (StrFunc.IsFilled(pCondApp))
                url += "&CondApp=" + pCondApp;


            if (StrFunc.IsFilled(pDataSetCache))
                url += "&DS=" + pDataSetCache;

            if (StrFunc.IsFilled(pFormId))
                url += "&F=" + pFormId;

            return url;
        }
        #endregion Accessors
        #region Constructors
        public PageLinkAttribute() { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Mode Repository : Appel à Referential.aspx
        /// Détermination de la table et de sa PK pour un menu donné 
        /// </summary>
        /// EG 20170125 [Refactoring URL] New
        /// FI 20170313 [22225] Modify
        /// FI 20170928 [23452] Modify
        /// EG 20210330 [XXXXX] Gestion link ligne de Tracker sur POSCOLLATERAL
        public void SetRepositoryInfo()
        {
            if (("Referential.aspx" == m_Name) && m_Menu.HasValue)
            {
                m_Type = Cst.ListType.Repository;
                switch (m_Menu.Value)
                {
                    case IdMenu.Menu.Actor:
                        m_PK = "IDA";
                        m_Object = Cst.OTCml_TBL.ACTOR.ToString();
                        break;
                    case IdMenu.Menu.GrpActor:
                        m_PK = "IDGACTOR";
                        m_Object = Cst.OTCml_TBL.GACTOR.ToString();
                        break;
                    case IdMenu.Menu.Algorithm: // FI 20170928 [23452] Add
                        m_PK = "IDA";
                        m_Object = "ALGORITHM";
                        break;
                    case IdMenu.Menu.Book:
                        m_PK = "IDB";
                        m_Object = Cst.OTCml_TBL.BOOK.ToString();
                        break;
                    case IdMenu.Menu.GrpBook:
                        m_PK = "IDGBOOK";
                        m_Object = Cst.OTCml_TBL.GBOOK.ToString();
                        break;
                    case IdMenu.Menu.AssetCash:
                        m_PK = "IDASSET";
                        m_Object = Cst.OTCml_TBL.ASSET_CASH.ToString();
                        break;
                    case IdMenu.Menu.AssetCommodity:
                        m_PK = "IDASSET";
                        m_Object = Cst.OTCml_TBL.ASSET_COMMODITY.ToString();
                        break;
                    case IdMenu.Menu.AssetETD:
                        m_PK = "IDASSET";
                        m_Object = Cst.OTCml_TBL.ASSET_ETD.ToString();
                        break;
                    case IdMenu.Menu.AssetEquity:
                        m_PK = "IDASSET";
                        m_Object = Cst.OTCml_TBL.ASSET_EQUITY.ToString();
                        break;
                    case IdMenu.Menu.AssetExchangeTradedFund:
                        m_PK = "IDASSET";
                        m_Object = Cst.OTCml_TBL.ASSET_EXTRDFUND.ToString();
                        break;
                    case IdMenu.Menu.AssetFxRate:
                        m_PK = "IDASSET";
                        m_Object = Cst.OTCml_TBL.ASSET_FXRATE.ToString();
                        break;
                    case IdMenu.Menu.AssetIndex:
                        m_PK = "IDASSET";
                        m_Object = Cst.OTCml_TBL.ASSET_INDEX.ToString();
                        break;
                    case IdMenu.Menu.AssetRateIndex:
                        m_PK = "IDASSET";
                        m_Object = Cst.OTCml_TBL.ASSET_RATEINDEX.ToString();
                        break;
                    case IdMenu.Menu.RateIndex: // FI 20190404 [XXXXX] Add
                        m_PK = "IDRX";
                        m_Object = Cst.OTCml_TBL.RATEINDEX.ToString();
                        break;
                    case IdMenu.Menu.BusinessCenter:
                        m_PK = "IDBC";
                        m_Object = Cst.OTCml_TBL.BUSINESSCENTER.ToString();
                        break;
                    case IdMenu.Menu.CommodityContract:
                        m_PK = "IDCC";
                        m_Object = Cst.OTCml_TBL.COMMODITYCONTRACT.ToString();
                        break;
                    case IdMenu.Menu.DrvContract:
                        m_PK = "IDDC";
                        m_Object = Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString();
                        break;
                    case IdMenu.Menu.DrvAttrib: // FI 20170313 [22225] Add
                        m_PK = "IDDERIVATIVEATTRIB";
                        m_Object = Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString();
                        break;
                    case IdMenu.Menu.GrpContract: // FI 20170908 [23409] add
                        m_PK = "IDGCONTRACT";
                        m_Object = Cst.OTCml_TBL.GCONTRACT.ToString();
                        break;
                    case IdMenu.Menu.GrpMarket: // FI 20170908 [23409] add
                        m_PK = "IDGMARKET";
                        m_Object = Cst.OTCml_TBL.GMARKET.ToString();
                        break;
                    case IdMenu.Menu.FilterActor:
                        m_PK = "IDMODELACTOR";
                        m_Object = "MODELDATA";
                        m_AdditionalInfo += "&P1=MODELACTOR";
                        break;
                    case IdMenu.Menu.FilterInstrument:
                        m_PK = "IDMODELINSTRUMENT";
                        m_Object = "MODELDATA_ITEM";
                        m_AdditionalInfo += "&P1=MODELINSTRUMENT&P2=INSTRUMENT";
                        break;
                    case IdMenu.Menu.FilterMarket:
                        m_PK = "IDMODELMARKET";
                        m_Object = "MODELDATA_ITEM";
                        m_AdditionalInfo += "&P1=MODELMARKET&P2=MARKET";
                        break;
                    case IdMenu.Menu.FilterMenu:
                        m_PK = "IDMODELMENU";
                        m_Object = "MODELDATA_ITEM";
                        m_AdditionalInfo += "&P1=MODELMENU&P2=MENU";
                        break;
                    case IdMenu.Menu.FilterPermission:
                        m_PK = "IDMODELPERMISSION";
                        m_Object = "MODELDATA";
                        m_AdditionalInfo += "&P1=MODELPERMISSION";
                        break;
                    case IdMenu.Menu.Fee:
                        m_PK = "IDFEE";
                        m_Object = Cst.OTCml_TBL.FEE.ToString();
                        break;
                    case IdMenu.Menu.FeeMatrix:
                        m_PK = "IDFEEMATRIX";
                        m_Object = Cst.OTCml_TBL.FEEMATRIX.ToString();
                        break;
                    case IdMenu.Menu.FeeSchedule:
                        m_PK = "IDFEESCHEDULE";
                        m_Object = Cst.OTCml_TBL.FEESCHEDULE.ToString();
                        break;
                    case IdMenu.Menu.GrpInstrument:
                        m_PK = "IDGINSTR";
                        m_Object = Cst.OTCml_TBL.GINSTR.ToString();
                        break;
                    case IdMenu.Menu.Instrument:
                        m_PK = "IDI";
                        m_Object = Cst.OTCml_TBL.INSTRUMENT.ToString();
                        m_Condition = "NOSTRATEGY";
                        break;
                    case IdMenu.Menu.Strategy: // FI 20240703 [WI989] Add
                        m_PK = "IDI";
                        m_Object = Cst.OTCml_TBL.INSTRUMENT.ToString();
                        m_Condition = "STRATEGY";
                        break;
                    case IdMenu.Menu.InvoicingRules:
                        m_PK = "IDINVOICINGRULES";
                        m_Object = Cst.OTCml_TBL.INVOICINGRULES.ToString();
                        break;

                    case IdMenu.Menu.IOCOMPARE:
                        m_PK = "IDIOCOMPARE";
                        m_Object = Cst.OTCml_TBL.IOCOMPARE.ToString();
                        break;

                    case IdMenu.Menu.IOINPUT:
                        m_PK = "IDIOINPUT";
                        m_Object = Cst.OTCml_TBL.IOINPUT.ToString();
                        break;
                    case IdMenu.Menu.IOOUTPUT:
                        m_PK = "IDIOOUTPUT";
                        m_Object = Cst.OTCml_TBL.IOOUTPUT.ToString();
                        break;
                    case IdMenu.Menu.IOPARAM:
                        m_PK = "IDIOPARAM";
                        m_Object = Cst.OTCml_TBL.IOPARAM.ToString();
                        break;
                    case IdMenu.Menu.IOPARSING:
                        m_PK = "IDIOPARSING";
                        m_Object = Cst.OTCml_TBL.IOPARSING.ToString();
                        break;
                    case IdMenu.Menu.IOSHELL:
                        m_PK = "IDIOSHELL";
                        m_Object = Cst.OTCml_TBL.IOSHELL.ToString();
                        break;
                    case IdMenu.Menu.IOTASK:
                        m_PK = "IDIOTASK";
                        m_Object = Cst.OTCml_TBL.IOTASK.ToString();
                        break;
                    case IdMenu.Menu.IOTASKDET:
                        m_PK = "IDIOTASKDET";
                        m_Object = Cst.OTCml_TBL.IOTASKDET.ToString();
                        break;

                    case IdMenu.Menu.Market:
                        m_PK = "IDM";
                        m_Object = Cst.OTCml_TBL.MARKET.ToString();
                        break;
                    case IdMenu.Menu.MATURITYRULE:
                        m_PK = "IDMATURITYRULE";
                        m_Object = Cst.OTCml_TBL.MATURITYRULE.ToString();
                        break;
                    case IdMenu.Menu.Product:
                        m_PK = "IDP";
                        m_Object = Cst.OTCml_TBL.PRODUCT.ToString();
                        break;
                    case IdMenu.Menu.STLMESSAGE:
                        m_PK = "IDSTLMESSAGE";
                        m_Object = Cst.OTCml_TBL.STLMESSAGE.ToString();
                        break;
                    case IdMenu.Menu.LogPosRequest:
                        m_PK = "IDPR";
                        m_Type = Cst.ListType.Log;
                        m_Object = Cst.OTCml_TBL.POSREQUEST.ToString();
                        break;
                    case IdMenu.Menu.TrackerPosRequest:
                        m_PK = "IDPR";
                        m_Type = Cst.ListType.Log;
                        m_Object = Cst.OTCml_TBL.TRACKER_POSREQUEST.ToString();
                        break;
                    case IdMenu.Menu.IOTRACK:
                        m_PK = "IDIOTRACK";
                        m_Type = Cst.ListType.Log;
                        m_Object = Cst.OTCml_TBL.IOTRACK.ToString();
                        break;
                    case IdMenu.Menu.TRACKER_L:
                        m_PK = "IDTRK_L";
                        m_Type = Cst.ListType.Log;
                        m_Object = Cst.OTCml_TBL.TRACKER_L.ToString();
                        m_Mode = Cst.ConsultationMode.PartialReadOnly;
                        break;
                    case IdMenu.Menu.CNFMessage:
                        m_PK = "IDCNFMESSAGE";
                        m_Object = "CNFMESSAGE";
                        break;
                    case IdMenu.Menu.TSMessage:
                        m_PK = "IDCNFMESSAGE";
                        m_Object = "TS_CNFMESSAGE";
                        break;
                    case IdMenu.Menu.POSCOLLATERAL:
                        m_PK = "IDPOSCOLLATERAL";
                        m_Object = "POSCOLLATERAL";
                        break;
                    case IdMenu.Menu.MODELSAFETY:
                        m_PK = "IDMODELSAFETY";
                        m_Object = "MODELSAFETY";
                        break;
                    case IdMenu.Menu.DefineExtend:
                        m_PK = "IDDEFINEEXTEND";
                        m_Object = "DEFINEEXTEND";
                        break;
                    default:
                        if (m_Menu.Value.ToString().StartsWith("QUOTE_"))
                        {
                            m_PK = "IDQUOTE_H";
                            m_Type = Cst.ListType.Price;
                            m_Object = m_Menu.ToString();
                            m_Mode = Cst.ConsultationMode.ReadOnly;

                            if ((m_Menu.Value == IdMenu.Menu.QUOTE_EQUITY_H) ||
                                 (m_Menu.Value == IdMenu.Menu.QUOTE_EXTRDFUND_H) ||
                                 (m_Menu.Value == IdMenu.Menu.QUOTE_INDEX_H) ||
                                 (m_Menu.Value == IdMenu.Menu.QUOTE_RATEINDEX_H))
                            {
                                m_Object = "QUOTE_xxx_H";
                                m_AdditionalInfo += "&P1=" + m_Menu.ToString().Replace("QUOTE_", string.Empty).Replace("_H", string.Empty);
                            }
                        }
                        else
                        {
                            m_PK = string.Empty;
                        }

                        break;
                }
            }
        }
        /// <summary>
        /// Mode List|Log|Consultation : Appel à List.aspx
        /// Détermination de l'objet associé à la consultation du grid et arguments complémentaires spécifiques
        /// </summary>
        // EG 20170125 [Refactoring URL] New
        public void SetListInfo()
        {
            if (("List.aspx" == m_Name) && m_Menu.HasValue)
            {
                m_AdditionalInfo = "&InputMode=2";
                switch (m_Menu.Value)
                {
                    case IdMenu.Menu.LogClosingDay:
                        m_Object = "EOD_POSREQUEST";
                        m_AdditionalInfo += "&P1=CLOSINGDAY";
                        break;
                    case IdMenu.Menu.LogEndOfDay:
                        m_Object = "EOD_POSREQUEST";
                        m_AdditionalInfo += "&P1=EOD";
                        break;
                    case IdMenu.Menu.PosSynthesisDet:
                        m_Object = "POSDET_ALLOC";
                        break;
                    case IdMenu.Menu.PosSynthesisDet_OTC:
                        m_Object = "POSDETOTC_ALLOC";
                        break;
                    case IdMenu.Menu.TrackerPosRequest:
                        m_Object = "TRACKER_POSREQUEST";
                        break;
                    case IdMenu.Menu.IOTRACK:
                        m_Object = "IOTRACK";
                        break;
                    case IdMenu.Menu.SERVICE_L:
                        m_Object = "SERVICE_L";
                        break;
                    case IdMenu.Menu.PROCESS_L:
                        m_Object = "IOTRACK";
                        if (false == Software.IsSoftwareOTCmlOrFnOml())
                            m_AdditionalInfo += "&P1=ONLYSIO";
                        break;
                    case IdMenu.Menu.EntityMarket:
                        m_Type = Cst.ListType.Repository;
                        m_Object = Cst.OTCml_TBL.ENTITYMARKET.ToString();
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion Methods
    }
    #endregion PageLinkAttribute

}
