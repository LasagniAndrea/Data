#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.GUI.CCI;
using EFS.GUI.ComplexControls;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CaptureTools.
    /// </summary>
    public sealed class CaptureTools : CaptureToolsBase
    {
        #region constructor
        public CaptureTools()
        {
        }
        #endregion constructor

        /// <summary>Mise à jour desc instances de classe FpML<br/>
        /// Utilisation de la collection de contrôles du placeHolder afin d'invoquer l'éventuelle méthode SaveClass
        /// d'un contrôle de type 'FpML.ComplexControls' 
        /// Chaque méthode SaveClass mettant à jour les instances FpML (pour les objets FpML concernés par le control) : FpMLDocReader
        /// </summary>
        /// <param name="page">Page asp</param>
        /// <param name="ctrlcollection">ControlCollection d'un PlaceHolder d'un écran de saisie</param>
        public static void CompleteCollection(PageBase page, ControlCollection pCtrlcollection)
        {

            //
            foreach (Control ctrl in pCtrlcollection)
            {
                //string ctrlIdTest = ctrl.UniqueID ;
                //ctrl.ID = ctrlIdTest;
                //
                Type t = ctrl.GetType();
                if ((t.Namespace == "EFS.GUI.ComplexControls") && (false == MethodsGUI.IsModeConsult(page)))
                {
                    MethodInfo m = t.GetMethod("SaveClass");
                    ParameterInfo[] pi;
                    if (m != null)
                    {
                        object[] argValues;
                        String[] argNames;
                        pi = m.GetParameters();
                        // Invocation de la méthode SaveClass en fonction du nombre de ses paramètres
                        switch (pi.Length)
                        {
                            case 0:
                                t.InvokeMember(m.Name, BindingFlags.InvokeMethod, null, ctrl, null, null, null, null);
                                break;
                            case 1:
                                argValues = new object[] { page };
                                argNames = new String[] { pi[0].Name };
                                t.InvokeMember(m.Name, BindingFlags.InvokeMethod, null, ctrl, argValues, null, null, argNames);
                                break;
                        }
                    }
                }
                else if (t.Equals(typeof(HtmlInputHidden)))
                {
                    string ctrlId = ctrl.UniqueID;

                    HtmlInputHidden ctrlHidden = (HtmlInputHidden)ctrl;
                    string valueServer = ctrlHidden.Value;
                    AttributeCollection attributeServer = ctrlHidden.Attributes;
                    bool isVirtual;
                    if ((null == attributeServer) || (StrFunc.IsEmpty(attributeServer["virtual"])))
                        //On est ici dans le cas de la saisie "Customized" qui ne dispose pas d'attribut.
                        isVirtual = true;
                    else
                        isVirtual = (ctrlHidden.Attributes["virtual"] == "1");
                    if (page.Request.Form[ctrlId] != null && (!isVirtual))
                    {
                        string valueClient = page.Request.Form[ctrlId];
                        string cssClassBtnServer = (valueServer == "0" ? "banniereButtonOpen" : "banniereButton");
                        string cssClassBtnClient = (valueClient == "0" ? "banniereButtonOpen" : "banniereButton");
                        string cssClassServer = (valueServer == "0" ? "banniereCapture" : "banniereCaptureOpen");
                        string cssClassClient = (valueClient == "0" ? "banniereCapture" : "banniereCaptureOpen");
                        string displayServer = (valueServer == "0" ? "display:none" : "display:block");
                        string displayClient = (valueClient == "0" ? "display:none" : "display:block");
                        Type tyGrandParent = ctrlHidden.Parent.Parent.GetType();
                        LiteralControl ctrlBalise;
                        if (tyGrandParent.Equals(typeof(OptionalItem)) || tyGrandParent.Equals(typeof(Choice)))
                            ctrlBalise = ctrlHidden.Parent.Parent.Controls[0].Controls[0] as LiteralControl;
                        else
                            ctrlBalise = ctrlHidden.Parent.Parent.Controls[0] as LiteralControl;


                        string txtBaliseBefore = ctrlBalise.Text;
                        LiteralControl ctrlPrevious = ctrl.Parent.Controls[ctrl.Parent.Controls.Count - 1] as LiteralControl;
                        string txtBaliseAfter = ctrlPrevious.Text;
                        ctrlBalise.Text = txtBaliseBefore.Replace(cssClassBtnServer, cssClassBtnClient);
                        ctrlBalise.Text = txtBaliseBefore.Replace(cssClassServer, cssClassClient);
                        ctrlPrevious.Text = txtBaliseAfter.Replace(displayServer, displayClient);
                    }
                }

                if (ctrl.Controls.GetType() == typeof(ControlCollection))
                    // Appel récursif pour les contrôles enfants 
                    CompleteCollection(page, ctrl.Controls);
            }

        }

        #region public IsDocumentElementValid
        /// <summary>
        /// Retourne true si l'élément IReference est correctement renseigné
        /// <para>Méthode notamment utilisée par CleanUp, tout élément incomplet est supprimé</para>
        /// </summary>
        /// <param name="pPartyRef"></param>
        /// <returns></returns>
        public static bool IsDocumentElementValid(IReference pPartyRef)
        {

            // utilisation pour CleanUp 
            // Ex si PartyPayer d'un frais est absent ou non interpréter => L'objet doit être supprimé pour obtenir un doc Fpml Propre
            // FpML_EntityOfUserIdentifier peut être présent lorsque il provient d'un template et qu'il n'existe aucune zone associé ds le screen 
            bool isOk = false;
            if (null != pPartyRef)
                isOk = (StrFunc.IsFilled(pPartyRef.HRef) && (pPartyRef.HRef != Cst.FpML_EntityOfUserIdentifier));
            return isOk;

        }
        /// <summary>
        /// Retourne true si l'élément IPartyTradeIdentifier est correctement renseigné
        /// <para>Méthode notamment utilisée par CleanUp, tout élément incomplet est supprimé</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pIsOnlyControlPartyReference"></param>
        /// <returns></returns>
        public static bool IsDocumentElementValid(IPartyTradeIdentifier pData, bool pIsOnlyControlPartyReference)
        {
            bool isOk = null != pData;
            // Attention S'il y a book le document pData est valide même s'il n'a pas de tradeId
            // Le tradeId  sera alimenté par la procedure checkAndRecord
            // 20070928 FI [15789] Add Test sur linkIdSpecified
            //
            // 20090930 FI [16770] add pIsOnlyControlPartyReference 
            // Car pour la sasisie des titres, tradeId est alimenté par la procedure checkAndRecord
            // il ne faut surtout pas supprimer le PartyTradeIdentifier de l'émetteur 
            // Spheres ne vérifie dans ce cas que l'existence de partyReference)   
            //
            if ((isOk) && (false == pIsOnlyControlPartyReference))
                isOk = (StrFunc.IsFilled(pData.PartyReference.HRef)) && (ArrFunc.IsFilled(pData.TradeId) || pData.LinkIdSpecified || pData.BookIdSpecified);
            return isOk;

        }
        /// <summary>
        /// Retourne true si l'élément IPartyTradeInformation est correctement renseigné
        /// <para>Méthode notamment utilisée par CleanUp, tout élément incomplet est supprimé</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pIsOnlyControlPartyReference"></param>
        /// <returns></returns>
        public static bool IsDocumentElementValid(IPartyTradeInformation pData)
        {

            // EG 20170918 [23342] Add executionDateTimeSpecified
            // EG 20171122 [23342] Add orderEnteredSpecified et sales
            // FI 20190408 [XXXXX] Add isMIFirSpecified


            bool ret = (null != pData);
            if (ret)
            {

                Boolean isMIFirSpecified = pData.RelatedPartySpecified || pData.RelatedPersonSpecified || pData.AlgorithmSpecified || pData.CategorySpecified ||
                    pData.TradingWaiverSpecified || pData.ShortSaleSpecified ||
                    pData.OtcClassificationSpecified || pData.IsCommodityHedgeSpecified || pData.IsSecuritiesFinancingSpecified;

                ret = (StrFunc.IsFilled(pData.PartyReference)) &&
                    (ArrFunc.IsFilled(pData.Trader) || ArrFunc.IsFilled(pData.BrokerPartyReference)
                    || ArrFunc.IsFilled(pData.Sales) || pData.ExecutionDateTimeSpecified || pData.OrderEnteredSpecified || isMIFirSpecified);

            }
            return ret;
        }
        /// <summary>
        /// Retourne true si l'élément ISettlementInput est correctement renseigné
        /// <para>Méthode notamment utilisée par CleanUp, tout élément incomplet est supprimé</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static bool IsDocumentElementValid(ISettlementInput pData)
        {
            bool isOk = null != pData && null != pData.SettlementContext && null != pData.SettlementInputInfo;
            if (isOk)
                isOk = (null != pData.SettlementInputInfo.SettlementInformation);
            if (isOk)
                isOk = (pData.SettlementInputInfo.SettlementInformation.InstructionSpecified) ||
                    (pData.SettlementInputInfo.SettlementInformation.StandardSpecified);

            if (pData.SettlementInputInfo.SettlementInformation.StandardSpecified)
            {
                isOk = (null != pData.SettlementInputInfo.CssCriteria);
                if (isOk)
                {
                    if (pData.SettlementInputInfo.CssCriteria.CssSpecified)
                        isOk = IsDocumentElementValid(pData.SettlementInputInfo.CssCriteria.Css.OtcmlId);
                    else if (pData.SettlementInputInfo.CssCriteria.CssInfoSpecified)
                        isOk = IsDocumentElementValid(pData.SettlementInputInfo.CssCriteria.CssInfo.Country.Value) ||
                            IsDocumentElementValid(pData.SettlementInputInfo.CssCriteria.CssInfo.Type.Value) ||
                            IsDocumentElementValid(pData.SettlementInputInfo.CssCriteria.CssInfo.SettlementType.Value) ||
                            IsDocumentElementValid(pData.SettlementInputInfo.CssCriteria.CssInfo.PaymentType.Value);
                }
            }
            return isOk;

        }
        /// <summary>
        /// Retourne true si l'élément INettingInformationInput est correctement renseigné
        /// <para>Méthode notamment utilisée par CleanUp, tout élément incomplet est supprimé</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static bool IsDocumentElementValid(INettingInformationInput pData)
        {
            bool isOk = null != pData;
            if (isOk)
            {
                if ((pData.NettingMethod == NettingMethodEnum.Designation))
                    isOk = StrFunc.IsFilled(pData.NettingDesignation.Value);
            }
            return isOk;
        }
        /// <summary>
        /// Retourne true si l'élément IMoney est correctement renseigné
        /// <para>Méthode notamment utilisée par CleanUp, tout élément incomplet est supprimé</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static bool IsDocumentElementValid(IMoney pData)
        {
            bool isOk = null != pData;
            if (isOk)
                isOk = StrFunc.IsFilled(pData.Amount.Value);
            return isOk;
        }
        /// <summary>
        /// Retourne true si l'élément IPayment est correctement renseigné
        /// <para>Méthode notamment utilisée par CleanUp, tout élément incomplet est supprimé</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static bool IsDocumentElementValid(IPayment pData)
        {
            IPayment payment = (IPayment)pData;
            bool isOk = null != pData;
            if (isOk)
                isOk = IsDocumentElementValid(payment.PayerPartyReference);
            if (isOk)
                isOk = IsDocumentElementValid(payment.PaymentAmount);
            return isOk;
        }
        /// <summary>
        /// Retourne true si l'élément IPayment est correctement renseigné
        /// <para>Méthode notamment utilisée par CleanUp, tout élément incomplet est supprimé</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static bool IsDocumentElementValid(IFxOptionPremium pData)
        {
            IFxOptionPremium premium = (IFxOptionPremium)pData;
            bool isOk = null != pData;
            if (isOk)
                isOk = IsDocumentElementValid(premium.PayerPartyReference);
            if (isOk)
                isOk = IsDocumentElementValid(premium.PremiumAmount);
            return isOk;

        }
        #endregion public IsDocumentElementValid

        /// <summary>
        /// Retourne true si l'élément principal de pData est renseigné
        /// <para>Utilisé par la saisie afin de contrôler qu'un élément n'est pas vide lorsque l'utilisateur click sur le bouton Remove</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20161114 [RATP] Modify
        public static bool IsDocumentElementInCapture(Object pData)
        {

            bool isOk;
            if (Tools.IsInterfaceOf(pData, InterfaceEnum.IPayment))
                isOk = StrFunc.IsFilled(((IPayment)pData).PayerPartyReference.HRef);
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.ITrader))
                isOk = StrFunc.IsFilled(((ITrader)pData).Identifier);
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IReference))
                isOk = StrFunc.IsFilled(((IReference)pData).HRef);
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IScheme))
                isOk = StrFunc.IsFilled(((IScheme)pData).Value);
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IFxOptionPremium))
                isOk = IsDocumentElementInCapture(((IFxOptionPremium)pData).PayerPartyReference);
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IFxBarrier))
                isOk = ((IFxBarrier)pData).FxBarrierTypeSpecified;
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IFxAmericanTrigger))
            {
                isOk = (null != ((IFxAmericanTrigger)pData).TriggerRate);
                if (isOk)
                    isOk = (((IFxAmericanTrigger)pData).TriggerRate.DecValue > 0);
            }
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IFxEuropeanTrigger))
            {
                isOk = (null != ((IFxEuropeanTrigger)pData).TriggerRate);
                if (isOk)
                    isOk = (((IFxEuropeanTrigger)pData).TriggerRate.DecValue > 0);
            }
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IInterestRateStream))
            {
                IReference reference = ((IInterestRateStream)pData).PayerPartyReference;
                isOk = (null != reference);
                if (isOk)
                    isOk = IsDocumentElementInCapture(reference);
            }
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IExchangeTradedDerivative))
            {
                IReference reference = ((IExchangeTradedDerivative)pData).BuyerPartyReference;
                isOk = (null != reference);
                if (isOk)
                    isOk = IsDocumentElementInCapture(reference);
            }
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IFxOptionLeg))
            {
                IReference reference = ((IFxOptionLeg)pData).BuyerPartyReference;
                isOk = (null != reference);
                if (isOk)
                    isOk = IsDocumentElementInCapture(reference);
            }
            else if (Tools.IsInterfaceOf(pData, InterfaceEnum.IFxLeg)) // FI 20161114 [RATP] add IFxLeg
            {
                isOk = ((IFxLeg)pData).FxDateValueDateSpecified;
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("Object {0} is not implemented", pData.GetType().ToString()));

            return isOk; ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pPartyTradeInfo"></param>
        /// <param name="pIdPartyTemplate"></param>
        /// EG 20180425 Analyse du code Correction [CA2202]
        private static void SetSalesFromPartyTemplate(string pCs, IPartyTradeInformation pPartyTradeInfo, int pIdPartyTemplate)
        {
            if (null != pPartyTradeInfo)
            {
                pPartyTradeInfo.Sales = null;
                pPartyTradeInfo.SalesSpecified = false;

                if (pIdPartyTemplate > 0)
                {
                    DataParameters sqlParam = new DataParameters();
                    sqlParam.Add(new DataParameter(pCs, "IDPARTYTEMPLATE", DbType.Int32), pIdPartyTemplate);

                    SQLWhere sqlWhere = new SQLWhere();
                    sqlWhere.Append("(ptd.IDPARTYTEMPLATE=@IDPARTYTEMPLATE)");
                    sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCs, "ptd"));

                    StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                    sqlSelect += "ptd.IDA,ac.IDENTIFIER,ac.DISPLAYNAME,ptd.FACTOR" + Cst.CrLf;
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.PARTYTEMPLATEDET.ToString() + " ptd" + Cst.CrLf;
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " ac on ac.IDA= ptd.IDA" + Cst.CrLf;

                    sqlSelect += sqlWhere.ToString() + Cst.CrLf;

                    //PL 20100205 Add "Order By"
                    sqlSelect += SQLCst.ORDERBY + "ptd.SEQUENCENO, ptd.FACTOR, ac.IDENTIFIER";

                    QueryParameters queryParameters = new QueryParameters(pCs, sqlSelect.ToString(), sqlParam);

                    DataTable dt = DataHelper.ExecuteDataTable(queryParameters.Cs, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

                    using (IDataReader dr = dt.CreateDataReader())
                    {
                        while (dr.Read())
                        {
                            ReflectionTools.AddItemInArray(pPartyTradeInfo, "sales", 0);
                            pPartyTradeInfo.Sales[pPartyTradeInfo.Sales.Length - 1].Identifier = Convert.ToString(dr["IDENTIFIER"]);
                            pPartyTradeInfo.Sales[pPartyTradeInfo.Sales.Length - 1].Name = Convert.ToString(dr["DISPLAYNAME"]);
                            pPartyTradeInfo.Sales[pPartyTradeInfo.Sales.Length - 1].OTCmlId = Convert.ToInt32(dr["IDA"]);
                            pPartyTradeInfo.Sales[pPartyTradeInfo.Sales.Length - 1].Factor = (Convert.DBNull == dr["FACTOR"]) ? 0 : Convert.ToDecimal(dr["FACTOR"]) / 100;
                            pPartyTradeInfo.SalesSpecified = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Alimentation d'un ISecurityAsset inclus dans dataDocument de type transaction (Repo,BSB, etc..) un à partir de l'IDT d'un asset 
        /// </summary>
        /// <param name="pSecurityAsset"></param>
        /// <param name="pDataDocument">DataDocument qui contient le ISecurityAsset </param>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdTAsset">IDT de l'asset à charger</param>
        // EG 20150422 [20513] BANCAPERTA
        public static void SetSecurityAssetInSecurityAsset(ISecurityAsset pSecurityAsset, DataDocumentContainer pDataDocument, string pCs, IDbTransaction pDbTransaction, int pIdTAsset)
        {

            //
            ISecurityAsset securityAsset = pSecurityAsset;
            ISecurityAsset newSecurityAsset = Tools.LoadSecurityAsset(pCs, pDbTransaction, pIdTAsset);
            //
            if (null != newSecurityAsset)
            {
                //
                securityAsset.SecurityId = newSecurityAsset.SecurityId;
                securityAsset.OTCmlId = newSecurityAsset.OTCmlId;
                //
                securityAsset.SecurityNameSpecified = newSecurityAsset.SecurityNameSpecified;
                if (securityAsset.SecurityNameSpecified)
                    securityAsset.SecurityName = newSecurityAsset.SecurityName;
                //
                securityAsset.SecurityDescriptionSpecified = newSecurityAsset.SecurityDescriptionSpecified;
                if (securityAsset.SecurityDescriptionSpecified)
                    securityAsset.SecurityDescription = newSecurityAsset.SecurityDescription;
                //
                //20091002 FI Alimentation de securityIssueDate.Value car lorsque l'on vient d'un template il peut y avoir un mot clef (ex TODAY)
                securityAsset.SecurityIssueDateSpecified = newSecurityAsset.SecurityIssueDateSpecified;
                if (securityAsset.SecurityIssueDateSpecified)
                    securityAsset.SecurityIssueDate.Value = newSecurityAsset.SecurityIssueDate.Value;
                //
                securityAsset.DebtSecuritySpecified = newSecurityAsset.DebtSecuritySpecified;
                if (securityAsset.DebtSecuritySpecified)
                    securityAsset.DebtSecurity = newSecurityAsset.DebtSecurity;
                // EG 20150422 [20513] BANCAPERTA
                //securityAsset.issuerSpecified = newSecurityAsset.issuerSpecified;
                //if (securityAsset.issuerSpecified)
                //{
                //    securityAsset.issuer = newSecurityAsset.issuer;
                //    securityAsset.issuerReferenceSpecified = true;
                //    securityAsset.issuerReference = pDataDocument.currentProduct.product.productBase.CreatePartyReference(newSecurityAsset.issuer.id);
                //}
                //if (newSecurityAsset.issuerReferenceSpecified)
                //{
                //    securityAsset.issuer = newSecurityAsset.issuer;
                //}

                if (null != newSecurityAsset.Issuer)
                {
                    securityAsset.Issuer = newSecurityAsset.Issuer;
                }
                else
                {
                    SecurityAssetContainer secAsset = new SecurityAssetContainer(newSecurityAsset);
                    secAsset.SetIssuer(new DataDocumentContainer(pDataDocument.DataDocument));
                }


                //
                ((IProduct)securityAsset.DebtSecurity).ProductBase.Id = null;
                securityAsset.Id = pDataDocument.GenerateId(securityAsset, "securityAsset", true);
                //
            }

        }

        /// <summary>
        /// Set LinkId (pScheme,pValue) in all PartyTradeIdentifier
        /// <para>Si LinkId n'existe pas alors création d'un nouveau LinkId</para>        
        /// </summary>
        /// <param name="pPartyTradeIdentifier"></param>
        /// <param name="pScheme"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public static bool SetLinkIdToPartyTradeIdentifier(IPartyTradeIdentifier[] pPartyTradeIdentifier, string pScheme, string pValue)
        {

            bool ret = false;
            //
            if (ArrFunc.IsFilled(pPartyTradeIdentifier))
            {
                for (int i = 0; i < pPartyTradeIdentifier.Length; i++)
                {
                    IPartyTradeIdentifier partyTradeIdentifier = pPartyTradeIdentifier[i];
                    bool isOk = CaptureTools.IsDocumentElementValid(partyTradeIdentifier.PartyReference);
                    //
                    if (isOk)
                    {
                        ILinkId linkId = partyTradeIdentifier.GetLinkIdFromScheme(pScheme);
                        //
                        // RD 20111114 / Pour utiliser un éventuel LinkId vide
                        if (null == linkId)
                            linkId = partyTradeIdentifier.GetLinkIdWithNoScheme();
                        //
                        if (null == linkId)
                        {
                            ReflectionTools.AddItemInArray(partyTradeIdentifier, "linkId", 0);
                            linkId = partyTradeIdentifier.LinkId[partyTradeIdentifier.LinkId.Length - 1];
                        }
                        //
                        if (null != linkId)
                        {
                            partyTradeIdentifier.LinkIdSpecified = true;
                            linkId.LinkIdScheme = pScheme;
                            linkId.Value = pValue;
                            //
                            ret = true;
                        }
                    }
                }
            }
            //
            return ret;

        }
        /// <summary>
        /// Génère n messages queue pour process EVENTGEN
        /// <para>Les messages queue sont réellement instanciés si l'instrument est paramétré pour générer des évènements</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIsDelEvent"></param>
        /// <param name="pRequester"></param>
        /// <returns></returns>
        // EG 20131024 Add pCaptureMode parameter
        // FI 20140415 Modification de la signature de la fonction, parameter pIsDelEvent
        // EG 20181127 PERF Post RC (Step 3)
        public static EventsGenMQueue[] GetMQueueForEventProcess(string pCs, int[] pIdT, Boolean pIsDelEvent, MQueueRequester pRequester)
        {
            return GetMQueueForEventProcess(pCs, pIdT, pIsDelEvent, false, pRequester);
        }
        // EG 20181127 PERF Post RC (Step 3)
        public static EventsGenMQueue[] GetMQueueForEventProcess(string pCs, int[] pIdT, Boolean pIsDelEvent, Boolean pIsNoLockCurrentId, MQueueRequester pRequester)
        {
            EventsGenMQueue[] ret = null;
            ArrayList al = new ArrayList();
            for (int i = 0; i < ArrFunc.Count(pIdT); i++)
            {
                SQL_TradeCommon sqlTradeCommon = new SQL_TradeCommon(pCs, pIdT[i]);
                sqlTradeCommon.LoadTable(new string[] { "IDI" });
                //
                SQL_Instrument sqlInstr = new SQL_Instrument(pCs, sqlTradeCommon.IdI);
                sqlInstr.LoadTable(new string[] { "ISEVENTS,IDI,GPRODUCT" });
                //
                if (sqlInstr.IsEvents)
                {
                    MQueueAttributes mQueueAttributes = new MQueueAttributes()
                    {
                        connectionString = pCs,
                        id = pIdT[i],
                        idInfo = GetMqueueIdInfo(pCs, pIdT[i])
                    };

                    EventsGenMQueue eventQueue = new EventsGenMQueue(mQueueAttributes);

                    eventQueue.header.requesterSpecified = (null != pRequester);
                    eventQueue.header.requester = pRequester;
                    // EG 20131024 Les queries de suppressions des événements ne seront plus lancée si mode = New (Création trade)
                    //bool isDelEvents = (null == pCaptureMode) || (pCaptureMode.HasValue && (pCaptureMode.Value != Cst.Capture.ModeEnum.New));
                    eventQueue.parameters[EventsGenMQueue.PARAM_DELEVENTS].SetValue(pIsDelEvent);
                    eventQueue.parameters[EventsGenMQueue.PARAM_NOLOCKCURRENTID].SetValue(pIsNoLockCurrentId);
                    al.Add(eventQueue);
                }
            }
            if (ArrFunc.IsFilled(al))
                ret = (EventsGenMQueue[])al.ToArray(typeof(EventsGenMQueue));
            return ret;
        }
        /// <summary>
        /// Génère 1 message queue pour process EVENTGEN
        /// <para>le message queue est réellement instancié si l'instrument est paramétré pour générer des évènements</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIsDelEvent"></param>
        /// <param name="pRequester"></param>
        /// <returns></returns>
        // EG 20131024 Add pCaptureMode parameter
        // FI 20140415 Modification de la signature de la fonction, parameter pIsDelEvent
        // EG 20181127 PERF Post RC (Step 3)
        public static EventsGenMQueue GetMQueueForEventProcess(string pCs, int pIdT, Boolean pIsDelEvent, MQueueRequester pRequester)
        {
            return GetMQueueForEventProcess(pCs, pIdT, pIsDelEvent, false, pRequester);
        }
        // EG 20181127 PERF Post RC (Step 3)
        public static EventsGenMQueue GetMQueueForEventProcess(string pCs, int pIdT, Boolean pIsDelEvent, Boolean pIsNoLockCurrentId, MQueueRequester pRequester)
        {
            EventsGenMQueue ret = null;
            EventsGenMQueue[] mQueueEvent = GetMQueueForEventProcess(pCs, new int[] { pIdT }, pIsDelEvent, pIsNoLockCurrentId, pRequester);
            if (ArrFunc.IsFilled(mQueueEvent))
                ret = mQueueEvent[0];
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        public static IdInfo GetMqueueIdInfo(string pCs, int pIdT)
        {
            SQL_TradeCommon sqlTradeCommon = new SQL_TradeCommon(pCs, pIdT);
            sqlTradeCommon.LoadTable(new string[] { "IDI" });

            SQL_Instrument sqlInstr = new SQL_Instrument(pCs, sqlTradeCommon.IdI);
            sqlInstr.LoadTable(new string[] { "IDI,GPRODUCT" });

            IdInfo idInfo = new IdInfo()
            {
                id = pIdT,
                idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", sqlInstr.GProduct) }
            };

            return idInfo;
        }

        /// <summary>
        /// Retroune true si la saisie est complete pour un ISaleAndRepurchaseAgreement
        /// </summary>
        public static bool IsInputFilled(ISaleAndRepurchaseAgreement pSaleAndRepurchaseAgreement)
        {

            bool ret = false;
            //
            bool isCalculationSpecified = pSaleAndRepurchaseAgreement.CashStream[0].CalculationPeriodAmount.CalculationSpecified;
            if (isCalculationSpecified)
            {
                ICalculation calculation = pSaleAndRepurchaseAgreement.CashStream[0].CalculationPeriodAmount.Calculation;
                //                
                if (calculation.RateFixedRateSpecified)
                    ret = (null != calculation.RateFixedRate.InitialValue) && (calculation.RateFixedRate.InitialValue.DecValue > decimal.Zero);
                //
                ret = ret && (pSaleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue > decimal.Zero);
                //
                if (pSaleAndRepurchaseAgreement.ForwardLegSpecified)
                    ret = ret && (pSaleAndRepurchaseAgreement.ForwardLeg[0].DebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue > decimal.Zero);
            }
            //
            return ret;

        }

        /// <summary>
        /// Retourne le membre de pObject qui se nomme pElementName ou qui possède un attribut de serialization  qui se nomme pElementName
        /// <para>Retourne null si l'élément est inexistant</para>
        /// <para>Remarque: Retourne null si l'élément vaut null</para>
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pElementName"></param>
        /// <returns></returns>
        public static object GetElementByName(object pObject, string pElementName)
        {

            return ReflectionTools.GetElementByName(pObject, pElementName);

        }

        /// <summary>
        /// Retourne true si l'acteur pIdA est membre compensateur de la chambre associée à un marché (CLEARINGMEMBERTYPE in (GCM,DCM))
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdM"></param>
        /// <param name="pUseDtEnabled"></param>
        /// <returns></returns>
        public static bool IsActorClearingMember(string pCS, int pIdA, int pIdM, bool pUseDtEnabled)
        {
            return IsActorClearingMember(pCS, pIdA, pIdM, pUseDtEnabled, out _);
        }

        /// <summary>
        /// Retourne true si l'acteur pIdA est membre compensateur de la chambre associée à un marché (CLEARINGMEMBERTYPE in (GCM,DCM))
        /// </summary>
        /// <param name="pIdA"></param>
        /// <param name="pIdMarket"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        public static bool IsActorClearingMember(string pCS, int pIdA, int pIdM, bool pUseDtEnabled, out bool pIsMArketMarker)
        {
            bool ret = false;
            pIsMArketMarker = false;

            SQL_Market sqlMarket = new SQL_Market(pCS, pIdM, SQL_Table.ScanDataDtEnabledEnum.No);
            if (false == sqlMarket.LoadTable(new string[] { "IDA" }))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("Market not found [Id:{0}]", pIdM));

            int idACss = sqlMarket.IdA;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDM), pIdM);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ID), idACss);

            StrBuilder sqlBase = new StrBuilder();
            sqlBase += SQLCst.SELECT + "{0} as COLORDER, cssm.ISMARKETMAKER" + Cst.CrLf;
            sqlBase += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
            sqlBase += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CSMID + " cssm on cssm.IDA=a.IDA" + Cst.CrLf;
            if (pUseDtEnabled)
                sqlBase += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "cssm").ToString() + Cst.CrLf;
            sqlBase += SQLCst.WHERE + "a.IDA=@IDA";
            if (pUseDtEnabled)
                sqlBase += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "a").ToString();
            sqlBase += SQLCst.AND + "cssm.IDA_CSS=@ID and {1}" + Cst.CrLf;
            sqlBase += SQLCst.AND + DataHelper.SQLColumnIn(pCS, "cssm.CLEARINGMEMBERTYPE", new string[] { Cst.ClearingMemberTypeEnum.GCM.ToString(), Cst.ClearingMemberTypeEnum.DCM.ToString() }, TypeData.TypeDataEnum.@string) + Cst.CrLf;

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += StrFunc.AppendFormat(sqlBase.ToString(), "1", "cssm.IDM=@IDM") + Cst.CrLf;
            //PL 20111221 Add UNIONALL 
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += StrFunc.AppendFormat(sqlBase.ToString(), "2", "cssm.IDM is null") + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "COLORDER";

            QueryParameters qry = new QueryParameters(pCS, sqlSelect.ToString(), dp);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                ret = dr.Read();
                if (ret)
                    pIsMArketMarker = BoolFunc.IsTrue(dr["ISMARKETMAKER"]);
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si l'entité doit venir en tant que gestionnaire alors qu'elle est déjà contrepartie 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20121002 [18165] add method IsAddEntityPartyInBroker
        public static bool IsAddEntityPartyInBroker(string pCS)
        {
            bool ret = false;
            StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT);
            sqlQuery += "ISADDENTITYPARTYINBROKER" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EFSSOFTWARE.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "IDEFSSOFTWARE=" + DataHelper.SQLString(Software.Name);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlQuery.ToString());
            if (null != obj)
                ret = Convert.ToBoolean(obj);

            return ret;
        }

        /// <summary>
        /// <para>Exploitation de PARTYTEMPLATE afin de rechercher un trader et éventuellement des sales</para>
        /// <para>Retourne true lorsque le dataDocument est mis à jour</para>
        /// <param name="pCs"></param>
        /// <param name="pTradeInput">Reprédente le trade</param>
        /// <param name="pPartyId">id de la party sur laquelle sont appliquées les recherches et les éventuelles mise à jour</param>
        /// </summary>
        /// <returns></returns>
        /// FI 20230927 [XXXXX][WI714] Refactoring
        /// FI 20240206 [WI841] Spheres® doit faire des pré-propositions et mettre à jour le dataDocument uniquement lorsqu'une instruction PartyTemplate peut être appliquée. 
        public static bool DumpTraderAndSales_ToDocument(string pCs, TradeCommonInput pTradeInput, string pPartyId)
        {
            if (null == pTradeInput)
                throw new ArgumentNullException(nameof(pTradeInput));
            if (null == pTradeInput.DataDocument)
                throw new NullReferenceException("DataDocument is null");

            DataDocumentContainer dataDocument = pTradeInput.DataDocument;

            IPartyTradeInformation partyTradeInfo = dataDocument.GetPartyTradeInformation(pPartyId);
            bool isOk = (null != partyTradeInfo);

            // INSTRUMENT
            int idI = dataDocument.CurrentProduct.IdI;

            //ACTOR
            int idA = 0;
            if (isOk)
            {
                // EG 20150706 [21021]
                int? idAParty = dataDocument.GetOTCmlId_Party(pPartyId);
                isOk = (idAParty.HasValue);
                if (isOk)
                    idA = idAParty.Value;
            }

            //BOOK et ENTITY
            int idB = 0;
            int idAEntity = 0;
            if (isOk)
            {
                int? idBook = dataDocument.GetOTCmlId_Book(pPartyId);
                isOk = (idBook.HasValue);
                if (isOk)
                {
                    idB = idBook.Value;
                    idAEntity = BookTools.GetEntityBook(pCs, idBook);
                    isOk = (idAEntity > 0);
                }
            }

            //MARKET 
            int idM = 0;
            if (isOk)
            {
                if (pTradeInput.SQLProduct.IsLSD || pTradeInput.SQLProduct.IsESE || pTradeInput.SQLProduct.IsCOMS)
                {
                    dataDocument.CurrentProduct.GetMarket(pCs, null, out SQL_Market sqlMarket);
                    if (null != sqlMarket)
                        idM = sqlMarket.Id;
                }
            }

            if (isOk)
            {
                isOk = false;

                PartyTemplate partyTemplateFind = PartyTemplates.FindTrader(pCs, idA, idI, idM, idB, idAEntity);
                if (null != partyTemplateFind)
                {
                    isOk = true;
                    if (partyTemplateFind.idATraderSpecified)
                    {
                        // Alimentation du trader
                        partyTradeInfo.Trader = null;
                        partyTradeInfo.TraderSpecified = false;

                        ReflectionTools.AddItemInArray(partyTradeInfo, "trader", 0);
                        partyTradeInfo.Trader[0].OTCmlId = partyTemplateFind.idATrader;
                        partyTradeInfo.Trader[0].Identifier = partyTemplateFind.traderIdentifier;
                        partyTradeInfo.Trader[0].Name = partyTemplateFind.traderDisplayName;
                        partyTradeInfo.TraderSpecified = true;
                    }
                    SetSalesFromPartyTemplate(pCs, partyTradeInfo, partyTemplateFind.idPartyTemplate);
                }
                else
                {
                    partyTemplateFind = PartyTemplates.FindSalesNoTrader(pCs, idA, idI, idM, idB, idAEntity);
                    if (null != partyTemplateFind)
                    {
                        isOk = true;
                        SetSalesFromPartyTemplate(pCs, partyTradeInfo, partyTemplateFind.idPartyTemplate);
                    }
                }
            }

            return isOk;
        }
        /// <summary>
        /// Alimentation du dataDocument avec des sales lorsqu'il existe un paramétrage approprié dans PARTYTEMPLATE
        /// <para>Recherche uniquement si acteur, book renseigné et si book est géré</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pTradeInput"></param>
        /// <param name="pPartyId"></param>
        /// <returns></returns>
        /// FI 20230927 [XXXXX][WI714] Refactoring
        /// FI 20240206 [WI841] Spheres® doit faire des pré-propositions et mettre à jour le dataDocument uniquement lorsqu'une instruction PartyTemplate peut être appliquée. 
        public static Boolean DumpSales_ToDocument_FromFirstTrader(string pCs, TradeCommonInput pTradeInput, string pPartyId)
        {
            if (null == pTradeInput)
                throw new ArgumentNullException(nameof(pTradeInput));
            if (null == pTradeInput.DataDocument)
                throw new NullReferenceException("DataDocument is null");

            DataDocumentContainer dataDocument = pTradeInput.DataDocument;

            IPartyTradeInformation partyTradeInfo = dataDocument.GetPartyTradeInformation(pPartyId);
            bool isOk = (null != partyTradeInfo);

            // INSTRUMENT
            int idI = dataDocument.CurrentProduct.IdI;

            //ACTOR
            int idA = 0;
            if (isOk)
            {
                // EG 20150706 [21021]
                int? idAParty = dataDocument.GetOTCmlId_Party(pPartyId);
                isOk = (idAParty.HasValue);
                if (isOk)
                    idA = idAParty.Value;
            }

            //BOOK et ENTITY
            int idB = 0;
            int idAEntity = 0;
            if (isOk)
            {
                int? idBook = dataDocument.GetOTCmlId_Book(pPartyId);
                isOk = (idBook.HasValue);
                if (isOk)
                {
                    idB = idBook.Value;
                    idAEntity = BookTools.GetEntityBook(pCs, idBook);
                    isOk = (idAEntity > 0);
                }
            }

            //MARKET 
            int idM = 0;
            if (isOk)
            {
                if (pTradeInput.SQLProduct.IsLSD || pTradeInput.SQLProduct.IsESE || pTradeInput.SQLProduct.IsCOMS)
                {
                    dataDocument.CurrentProduct.GetMarket(pCs, null, out SQL_Market sqlMarket);
                    if (null != sqlMarket)
                        idM = sqlMarket.Id;
                }
            }

            //TRADER
            int idATrader = 0;
            if (isOk)
            {
                //FI 20141119 [20505] le trader n'est plus obligatoire
                if (ArrFunc.Count(partyTradeInfo.Trader) > 0)
                    idATrader = partyTradeInfo.Trader[0].OTCmlId;
            }

            if (isOk)
            {
                isOk = false;

                PartyTemplate partyTemplateFind = PartyTemplates.FindSales(pCs, idA, idI, idM, idB, idAEntity, idATrader);
                if (null != partyTemplateFind)
                {
                    isOk = true;
                    SetSalesFromPartyTemplate(pCs, partyTradeInfo, partyTemplateFind.idPartyTemplate);
                }
            }

            return isOk;
        }
    }
}