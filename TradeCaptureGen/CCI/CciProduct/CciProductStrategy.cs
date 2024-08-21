#region Using Directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;
using System.Xml.Serialization;


#endregion Using Directives
namespace EFS.TradeInformation
{
    public class CciProductStrategy : CciProductBase
    {
        

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Add
        public StrategyContainer Strategy { get; private set; }

        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected CciTrade CciTrade
        {
            get { return base.CciTradeCommon as CciTrade; }
        }
        #region cciProductGlogal
        public CciProductBase CciProductGlogal { get; private set; }
        #endregion cciProductGlogal
        #region SubProductLength
        /// <summary>
        /// Obtient le nbr de produit de la strategy
        /// </summary>
        public int SubProductLength
        {
            get { return ArrFunc.IsFilled(CciSubProduct) ? CciSubProduct.Length : 0; }
        }
        #endregion productLenght
        public CciProductBase[] CciSubProduct { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200116 [25141] Add
        public override CustomCaptureInfo CciExchange
        {
            get
            {
                return CciProductGlogal.CciExchange;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200116 [25141] Add
        public override string CciExchangeColumn
        {
            get
            {
                return CciProductGlogal.CciExchangeColumn;
            }
        }

        #endregion
        
        #region constructor
        public CciProductStrategy(CciTrade pCciTrade, IStrategy pStrategy, string pPrefix)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pStrategy, pPrefix)
        {
        }
        #endregion constructor
        
        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            InitializeProduct_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        public override void AddCciSystem()
        {
            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region public override Initialize_FromDocument
        public override void Initialize_FromDocument()
        {

            if (null != CciProductGlogal)
                CciProductGlogal.Initialize_FromDocument();
            //
            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].Initialize_FromDocument();
            //
            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
                Initialize_FromDefault();

        }
        #endregion Initialize_FromDocument
        #region public override Dump_ToDocument
        public override void Dump_ToDocument()
        {
            if (null != CciProductGlogal)
            {
                #region synchrozine _cciProductGlogal avec _cciProduct[0]
                // Permet de conserver en phase le product global et le product[0] pour les ccis existants dans les 2 objects
                // Ex il existe irs_payer et irs1.payer
                foreach (CustomCaptureInfo cci in CcisBase)
                {
                    if (cci.HasChanged && CciSubProduct[0].IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        string clientId_Key = CciSubProduct[0].CciContainerKey(cci.ClientId_WithoutPrefix);
                        string clientIdGlobal = CciProductGlogal.CciClientId(clientId_Key);
                        if (CcisBase.Contains(clientIdGlobal))
                            CciProductGlogal.Cci(clientId_Key).NewValue = cci.NewValue;
                    }
                }
                #endregion

                #region synchronize cciProduct[i] from  cciProductGlobal
                foreach (CustomCaptureInfo cci in CcisBase)
                {
                    if (cci.HasChanged && CciProductGlogal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        string clientId_Key = CciProductGlogal.CciContainerKey(cci.ClientId_WithoutPrefix);
                        for (int i = 0; i < SubProductLength; i++)
                        {
                            string clientId = CciSubProduct[i].CciClientId(clientId_Key);
                            if (CcisBase.Contains(clientId))
                                CciSubProduct[i].Cci(clientId_Key).NewValue = cci.NewValue;
                        }
                    }
                }
                #endregion
            }

            if (null != CciProductGlogal)
                CciProductGlogal.Dump_ToDocument();

            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].Dump_ToDocument();
        }
        #endregion Dump_ToDocument
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (null != CciProductGlogal)
                CciProductGlogal.ProcessInitialize(pCci);

            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].ProcessInitialize(pCci);

            SynchronizeFromMainProduct(pCci);

            //FI 20101108 [17205] 
            //Lorsque l'utilisateur renseigne le payer, Spheres® pré-alimente les ccis du _cciSubProduct avec les valeurs dites "globales"
            //Exemple sur les strategies ETD, La donnée marché est une donnée globale
            //Si l'utilisateur renseigne le payer d'un SubProduct, le marché de ce dernier est pré-alimenté avec le marché stocké dans la donnée globale 
            for (int i = 0; i < SubProductLength; i++)
            {
                if (CciSubProduct[i].CciClientIdPayer == pCci.ClientId_WithoutPrefix && pCci.IsFilledValue && pCci.IsLastEmpty)
                    SynchronizeSubProduct(i);
            }

            if ((pCci.ClientId_WithoutPrefix == CciClientIdPayer) || (pCci.ClientId_WithoutPrefix == CciClientIdReceiver))
                CciTrade.InitializePartySide();

        }
        #endregion ProcessInitialize
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            if (null != CciProductGlogal)
                isOk = CciProductGlogal.IsClientId_PayerOrReceiver(pCci);

            if (!isOk)
            {
                for (int i = 0; i < SubProductLength; i++)
                {
                    isOk = isOk || CciSubProduct[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            return isOk;
        }
        #endregion IsClientId_PayerOrReceiver
        #region public override CleanUp
        public override void CleanUp()
        {

            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].CleanUp();
            //
            for (int i = SubProductLength - 1; -1 < i; i--)
            {
                bool isRemove;
                if (CciSubProduct[i].Product.IsExchangeTradedDerivative)
                {
                    IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)CciSubProduct[i].Product.Product;
                    isRemove = (false == CaptureTools.IsDocumentElementValid(exchangeTradedDerivative.BuyerPartyReference));
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Current product {0} is not managed, please contact EFS", CciSubProduct[i].GetType().FullName));
                //
                if (isRemove)
                    ReflectionTools.RemoveItemInArray(Strategy.Product, "Item", i);
            }
            //
            //Si strategy il faut reaffecter le StrategyContainer afin de mettre à jour le membre _subProductContainer
            CciTrade.DataDocument.SetCurrentProduct();
        }
        #endregion
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            if (null != CciProductGlogal)
                CciProductGlogal.SetDisplay(pCci);

            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].SetDisplay(pCci);
        }
        #endregion
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            if (null != CciProductGlogal)
                CciProductGlogal.RefreshCciEnabled();
            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].RefreshCciEnabled();
        }
        #endregion
        #region public override Initialize_Document
        public override void Initialize_Document()
        {
            if (null != CciProductGlogal)
                CciProductGlogal.Initialize_Document();

            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].Initialize_Document();
        }
        #endregion
        #region public override SetButtonReferential
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            if (null != CciProductGlogal)
                CciProductGlogal.SetButtonReferential(pCci, pCo);

            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].SetButtonReferential(pCci, pCo);
        }
        #endregion
        #region public override DumpSpecific_ToGUI
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (null != CciProductGlogal)
                CciProductGlogal.DumpSpecific_ToGUI(pPage);

            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].DumpSpecific_ToGUI(pPage);
        }
        #endregion



        #endregion
        
        #region Membres de IContainerCciPayerReceiver
        #region public override CciClientIdPayer
        public override string CciClientIdPayer
        {
            get
            {
                //CciProductBase cciProductMain = GetCciProductMain(MainProductEnum.Side);
                CciProductBase cciProductMain = GetCciProductMain();
                return cciProductMain.CciClientIdPayer;
            }
        }
        #endregion CciClientIdPayer
        #region public override CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get
            {
                //CciProductBase cciProductMain = GetCciProductMain(MainProductEnum.Side);
                CciProductBase cciProductMain = GetCciProductMain();
                return cciProductMain.CciClientIdReceiver;
            }
        }
        #endregion CciClientIdReceiver
        #region public override SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion
        #endregion Membres de IContainerCciPayerReceiver
        
        #region Membres de ITradeCci
        #region public override RetSidePayer
        public override string RetSidePayer
        {
            get
            {
                //CciProductBase cciProductMain = GetCciProductMain(MainProductEnum.Side);
                CciProductBase cciProductMain = GetCciProductMain();
                return cciProductMain.RetSidePayer;
            }
        }
        #endregion RetSidePayer
        #region public override RetSideReceiver
        public override string RetSideReceiver
        {
            get
            {
                //CciProductBase cciProductMain = GetCciProductMain(MainProductEnum.Side);
                CciProductBase cciProductMain = GetCciProductMain();
                return cciProductMain.RetSideReceiver;
            }
        }
        #endregion RetSideReceiver
        #region public override GetMainCurrency
        /// <summary>
        /// Return the main currency for a product
        /// </summary>
        /// <returns></returns>
        public override string GetMainCurrency
        {
            get
            {
                CciProductBase cciProductMain = GetCciProductMain();
                return cciProductMain.GetMainCurrency;
            }
        }
        #endregion GetMainCurrency
        #region public override CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get
            {
                //CciProductBase cciProductMain = GetCciProductMain(MainProductEnum.Side);
                CciProductBase cciProductMain = GetCciProductMain();
                return cciProductMain.CciClientIdMainCurrency;
            }
        }
        #endregion CciClientIdMainCurrency

        #endregion  Membres de ITradeCci
        
        #region Membres de IContainerCciQuoteBasis
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            for (int i = 0; i < SubProductLength; i++)
            {
                isOk = CciSubProduct[i].IsClientId_QuoteBasis(pCci);
                if (isOk)
                    break;
            }
            //
            return isOk;

        }
        #endregion
        #region public override GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //

            for (int i = 0; i < SubProductLength; i++)
            {
                ret = CciSubProduct[i].GetCurrency1(pCci);
                if (StrFunc.IsFilled(ret))
                    break;
            }
            //
            return ret;

        }
        #endregion
        #region public override GetCurrency2
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //

            for (int i = 0; i < SubProductLength; i++)
            {
                ret = CciSubProduct[i].GetCurrency2(pCci);
                if (StrFunc.IsFilled(ret))
                    break;
            }
            //
            return ret;

        }
        #endregion
        #endregion Membres de IContainerCciQuoteBasis
        
        #region Methods
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            Strategy = null;
            if (null != pProduct)
                Strategy = new StrategyContainer((IStrategy)pProduct);
            base.SetProduct(pProduct);
        }
        #endregion
        #region public override ReplaceTradeDynamicConstantsWithValues
        public override string ReplaceTradeDynamicConstantsWithValues(CustomCaptureInfo pCci, string pInitialString)
        {

            string ret = pInitialString;
            //
            CciProductBase cciProductFind = null;
            foreach (CciProductBase cciProduct in CciSubProduct)
            {
                if (cciProduct.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    cciProductFind = cciProduct;
                    break;
                }
            }
            if (cciProductFind != null)
                ret = cciProductFind.ReplaceTradeDynamicConstantsWithValues(pCci, ret);
            //
            ret = base.ReplaceTradeDynamicConstantsWithValues(pCci, ret);
            //
            return ret;

        }
        #endregion
        //
        #region private CciProductMain
        /// <summary>
        /// Obtient le product représentatif d'une strategy (représentatif du side, du callPut, etc..)
        /// <para>Le produit représentatif est paramétré dans INSTRUMENTOF</para>
        /// </summary>
        //private CciProductBase GetCciProductMain(MainProductEnum pType)
        private CciProductBase GetCciProductMain()
        {

            CciProductBase ret = null;
            ProductContainer mainProduct = Strategy.MainProduct;
            if (null != mainProduct)
            {
                if (ArrFunc.IsFilled(CciSubProduct))
                {
                    foreach (CciProductBase cciProduct in CciSubProduct)
                    {
                        if (cciProduct.Product.ProductBase.Id == mainProduct.Product.ProductBase.Id)
                        {
                            ret = cciProduct;
                            break;
                        }
                    }
                }
            }
            return ret;
        }
        #endregion
        //
        #region private InitializeProduct_FromCci
        private void InitializeProduct_FromCci()
        {

            bool isAddNewproduct = false;
            bool isOk = true;
            int index = -1;
            //
            ArrayList lst = new ArrayList();
            lst.Clear();
            //
            while (isOk)
            {
                index += 1;
                CciProductBase cciCurrentProduct = null;
                isOk = false;
                //
                if (false == isOk)
                {
                    cciCurrentProduct = new CciProductFXOptionLeg(CciTrade, null, CciProductFXOptionLeg.FxProduct.FxOptionLeg, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fxSimpleOption, index + 1);
                    // FI 20180511 Analyse du code Correction [CA2214] 
                    cciCurrentProduct.SetProduct(null);
                    isOk = CcisBase.Contains(cciCurrentProduct.CciClientIdPayer);
                }
                //
                if (false == isOk)
                {
                    cciCurrentProduct = new CciProductEquityOption(CciTrade, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_equityOption, index + 1);
                    // FI 20180511 Analyse du code Correction [CA2214] 
                    cciCurrentProduct.SetProduct(null);
                    isOk = CcisBase.Contains(cciCurrentProduct.CciClientIdPayer);
                }
                //
                if (false == isOk)
                {
                    cciCurrentProduct = new CciProductExchangeTradedDerivative(CciTrade, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_exchangeTradedDerivative, index + 1);
                    isOk = CcisBase.Contains(cciCurrentProduct.CciClientIdPayer);
                    if (false == isOk)
                    {
                        CciProductExchangeTradedDerivative cciExchangeTradedDerivative = (CciProductExchangeTradedDerivative)cciCurrentProduct;
                        // FI 20180511 Analyse du code Correction [CA2214] 
                        cciExchangeTradedDerivative.SetProduct(null);
                        isOk = CcisBase.Contains(cciExchangeTradedDerivative.CciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.RptSide_Side));
                    }
                }
                //
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(Strategy.SubProduct) || (index == Strategy.SubProduct.Length))
                    {
                        isAddNewproduct = true; // Ajout d'un nouvel instrument 
                        object newItem = null;
                        //
                        string elementName = cciCurrentProduct.Prefix;
                        //A conserver dans le cas où le prefix strategy est renseigné
                        if (StrFunc.IsFilled(Prefix))
                            elementName = elementName.Replace(Prefix, string.Empty);
                        elementName = elementName.Replace(CustomObject.KEY_SEPARATOR.ToString(), string.Empty);
                        elementName = StrFunc.PutOffSuffixNumeric(elementName);
                        //
                        //Recherche de l'élément associé au cciProduct défini par le screen
                        FieldInfo fieldInfo = Strategy.Strategy.GetType().GetField("Item");
                        object[] attributes = fieldInfo.GetCustomAttributes(typeof(XmlElementAttribute), true);
                        if (ArrFunc.IsFilled(attributes))
                        {
                            XmlElementAttribute attribute = null;
                            bool isFind = false;
                            for (int i = 0; i < ArrFunc.Count(attributes); i++)
                            {
                                attribute = (XmlElementAttribute)attributes[i];
                                isFind = (attribute.ElementName == elementName);
                                if (isFind)
                                    break;
                            }
                            if (isFind)
                                newItem = attribute.Type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                        }
                        if (null == newItem)
                            throw new NotImplementedException(StrFunc.AppendFormat("element {0} is not is not managed, please contact EFS", elementName));
                        //
                        ArrayList aObjects = new ArrayList();
                        //Sauvegarde de l'array subProduct
                        if (null != Strategy.SubProduct)
                        {
                            Array aObj = (Array)Strategy.SubProduct;
                            for (int i = 0; i < aObj.Length; i++)
                            {
                                if (null != aObj.GetValue(i))
                                    aObjects.Add(aObj.GetValue(i));
                            }
                        }
                        //Ajout de l'object nouvellement instancié (newItem)
                        aObjects.Add(newItem);
                        //
                        Strategy.SubProduct = (object[])aObjects.ToArray(newItem.GetType());
                    }
                    //
                    cciCurrentProduct.SetProduct((IProduct)Strategy.SubProduct[index]);
                    lst.Add(cciCurrentProduct);
                }
            }

            CciSubProduct = (CciProductBase[])lst.ToArray(typeof(CciProductBase));
            if (ArrFunc.IsEmpty(CciSubProduct))
                throw new NotImplementedException(StrFunc.AppendFormat("clientid for product element is unknown, please contact EFS"));

            //Alimentation de product lorsque des subProduct sont ajoutés
            if (isAddNewproduct)
            {
                //La strategy ayant évoluée en fonction du screen, il convient de synchronizer le membre  _strategy (de type StrategyContainer);
                SetProduct(Strategy.Strategy);
            }

            //Alimentation à partir de paramétrage de INSTRUMENTOF
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode)))
                Tools.SetProduct(CciTrade.CSCacheOn, Strategy, CciTradeCommon.DataDocument.CurrentProduct.IdI);
            //
            if ((isAddNewproduct))
            {
                //Si INSTRUMENTOF ne décrit pas tous les instruments, alors productType est éventuellement alimenté avec données présente sur l'instrument précédent
                for (int i = 1; i < SubProductLength; i++)
                {
                    if (CciSubProduct[i].Product.ProductBase.ProductType == null)
                    {
                        if (CciSubProduct[i].GetType().Equals(CciSubProduct[i - 1].GetType()))
                        {
                            int idI = CciSubProduct[i - 1].Product.IdI;
                            string identifier = ((IScheme)CciSubProduct[i - 1].Product.ProductBase.ProductType).Value;
                            CciSubProduct[i].Product.ProductBase.SetProductType(idI.ToString(), identifier);
                            //
                            string id = CciSubProduct[i - 1].Product.ProductBase.Id;
                            int instrNum = StrFunc.GetSuffixNumeric2(id) + 1;
                            CciSubProduct[i].Product.ProductBase.SetId(instrNum);
                            //
                            if (CciSubProduct[i].Product.IsExchangeTradedDerivative)
                            {
                                IExchangeTradedDerivative exChangeTradedDerivative = (IExchangeTradedDerivative)CciSubProduct[i].Product.Product;
                                IExchangeTradedDerivative exChangeTradedDerivativePrev = (IExchangeTradedDerivative)CciSubProduct[i - 1].Product.Product;
                                exChangeTradedDerivative.Category = exChangeTradedDerivativePrev.Category;
                            }
                        }
                    }
                }
            }
            //
            //Synchronisation des éléments de INSTRUMENTOF spéciques aux strategies  
            if (ArrFunc.IsFilled(CciSubProduct))
            {
                ProductContainer[] subProduct = Strategy.GetSubProduct();
                for (int i = 0; i < ArrFunc.Count(CciSubProduct); i++)
                {
                    CciSubProduct[i].Product.SideValue = subProduct[i].SideValue;
                    CciSubProduct[i].Product.DerivativeContractValue = subProduct[i].DerivativeContractValue;
                    CciSubProduct[i].Product.PutCallValue = subProduct[i].PutCallValue;
                    CciSubProduct[i].Product.StrikeValue = subProduct[i].StrikeValue;
                    CciSubProduct[i].Product.MaturityValue = subProduct[i].MaturityValue;
                    CciSubProduct[i].Product.QuantityValue = subProduct[i].QuantityValue;
                    CciSubProduct[i].Product.PriceValue = subProduct[i].PriceValue;
                    CciSubProduct[i].Product.IsUnderlyerReference = subProduct[i].IsUnderlyerReference;

                }
            }

            #region init specifique des cciProduct
            for (int i = 0; i < SubProductLength; i++)
            {
                if (CciSubProduct[i].GetType().Equals(typeof(CciProductFXOptionLeg)))
                    ((CciProductFXOptionLeg)CciSubProduct[i]).IsInitPremiumPayerWithBuyer = true;
            }
            #endregion

            InitializeCciProductGlobal();

            if (null != CciProductGlogal)
            {
                #region génération ds chaque cciProduct des ccis du CciProductGlobal
                for (int i = 0; i < CcisBase.Count; i++)
                {
                    CustomCaptureInfo cci = CcisBase[i];
                    if (CciProductGlogal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        string clientId_Key = CciProductGlogal.CciContainerKey(cci.ClientId_WithoutPrefix);
                        for (int j = 0; j < SubProductLength; j++)
                            Ccis.CloneGlobalCci(clientId_Key, cci, CciSubProduct[j]);
                    }
                }
                #endregion
            }

            if (null != CciProductGlogal)
                CciProductGlogal.Initialize_FromCci();

            for (int i = 0; i < SubProductLength; i++)
                CciSubProduct[i].Initialize_FromCci();
        }
        #endregion
        #region private InitializeCciProductGlobal
        /// <summary>
        /// Initialisation de _cciProductGlogal, _cciProductGlogal n'existe que sur une strategy homogène
        /// </summary>
        private void InitializeCciProductGlobal()
        {
            CciProductGlogal = null;

            if (SubProductLength >= 1)
            {
                Hashtable al = new Hashtable();

                for (int i = 0; i < SubProductLength; i++)
                {
                    if (false == al.Contains(CciSubProduct[i].GetType().FullName))
                        al.Add(CciSubProduct[i].GetType().FullName, CciSubProduct[i].GetType().FullName);
                }

                if (1 == ArrFunc.Count(al))
                {
                    if (CciSubProduct[0].GetType().Equals(typeof(CciProductFXOptionLeg)))
                        CciProductGlogal = new CciProductFXOptionLeg(CciTrade, Strategy.SubProduct[0], CciProductFXOptionLeg.FxProduct.FxOptionLeg, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fxSimpleOption, 0);
                    else if (CciSubProduct[0].GetType().Equals(typeof(CciProductEquityOption)))
                        CciProductGlogal = new CciProductEquityOption(CciTrade, (IEquityOption)Strategy.SubProduct[0], Prefix + TradeCustomCaptureInfos.CCst.Prefix_equityOption, 0);
                    else if (CciSubProduct[0].GetType().Equals(typeof(CciProductExchangeTradedDerivative)))
                        CciProductGlogal = new CciProductExchangeTradedDerivative(CciTrade, (IExchangeTradedDerivative)Strategy.SubProduct[0], Prefix + TradeCustomCaptureInfos.CCst.Prefix_exchangeTradedDerivative, 0);
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("Current product {0} is not managed, please contact EFS", CciSubProduct[0].GetType().FullName));

                    // FI 20180511 Analyse du code Correction [CA2214] 
                    CciProductGlogal.SetProduct((IProduct)Strategy.SubProduct[0]);

                }
            }

        }
        #endregion
        #region private SynchronizeFromMainProduct
        /// <summary>
        /// Initialise les ccis en fonction des paramétrages DYNAMIC existants sous INSTRUMENTOF
        /// <para>
        /// L'initialisation s'effectue lorsque le pCCi appartient au subProduct de référence
        /// </para>
        /// </summary>
        /// <param name="pCci"></param>
        private void SynchronizeFromMainProduct(CustomCaptureInfo pCci)
        {
            #region Synchronize des payer/receiver
            CciProductBase cciProductMain = GetCciProductMain();
            if ((null != cciProductMain) && (cciProductMain.IsCciOfContainer(pCci.ClientId_WithoutPrefix)))
            {
                bool isCalcFromPutCall = false;
                bool isCalcFromPayerReceiver = cciProductMain.IsClientId_PayerOrReceiver(pCci);
                if (false == isCalcFromPayerReceiver)
                {
                    if (cciProductMain.Product.IsExchangeTradedDerivative)
                    {
                        CciProductExchangeTradedDerivative cciExchangeTradedDerivative = (CciProductExchangeTradedDerivative)cciProductMain;
                        if (cciExchangeTradedDerivative.ExchangeTradedDerivative.Category == CfiCodeCategoryEnum.Option)
                            isCalcFromPutCall = cciExchangeTradedDerivative.CciFixTradeCaptureReport.CciFixInstrument.IsCci(CciFixInstrument.CciEnum.PutCall, pCci);
                    }
                }
                if (isCalcFromPayerReceiver || isCalcFromPutCall)
                {
                    foreach (CciProductBase cciProductItem in this.CciSubProduct)
                    {
                        if (cciProductItem.Product.ProductBase.Id != cciProductMain.Product.ProductBase.Id)
                        {
                            if (null != cciProductItem.Product.SideValue)
                            {
                                if (isCalcFromPayerReceiver)
                                {
                                    if (cciProductItem.Product.SideValue == SideValueEnum.SIDE)
                                    {
                                        CcisBase.SetNewValue(cciProductItem.CciClientIdPayer, CcisBase[cciProductMain.CciClientIdPayer].NewValue);
                                        CcisBase.SetNewValue(cciProductItem.CciClientIdReceiver, CcisBase[cciProductMain.CciClientIdReceiver].NewValue);
                                    }
                                    else if (cciProductItem.Product.SideValue == SideValueEnum.REVERSESIDE)
                                    {
                                        CcisBase.SetNewValue(cciProductItem.CciClientIdPayer, CcisBase[cciProductMain.CciClientIdReceiver].NewValue);
                                        CcisBase.SetNewValue(cciProductItem.CciClientIdReceiver, CcisBase[cciProductMain.CciClientIdPayer].NewValue);
                                    }
                                }
                                //
                                if (isCalcFromPutCall || isCalcFromPayerReceiver)
                                {
                                    if (cciProductItem.Product.SideValue == SideValueEnum.DELTANEUTRALSIDE)
                                    {
                                        if ((Strategy.IsHomogeneous) && (cciProductMain.Product.IsExchangeTradedDerivative))
                                        {
                                            IExchangeTradedDerivative exchangeTradeDerivatideMain = (IExchangeTradedDerivative)cciProductMain.Product.Product;
                                            if (exchangeTradeDerivatideMain.Category == CfiCodeCategoryEnum.Option)
                                            {
                                                if (exchangeTradeDerivatideMain.TradeCaptureReport.Instrument.PutOrCallSpecified)
                                                {
                                                    if (exchangeTradeDerivatideMain.TradeCaptureReport.Instrument.PutOrCall == PutOrCallEnum.Put)
                                                    {
                                                        CcisBase.SetNewValue(cciProductItem.CciClientIdPayer, CcisBase[cciProductMain.CciClientIdPayer].NewValue);
                                                        CcisBase.SetNewValue(cciProductItem.CciClientIdReceiver, CcisBase[cciProductMain.CciClientIdReceiver].NewValue);
                                                    }
                                                    else if (exchangeTradeDerivatideMain.TradeCaptureReport.Instrument.PutOrCall == PutOrCallEnum.Call)
                                                    {
                                                        CcisBase.SetNewValue(cciProductItem.CciClientIdPayer, CcisBase[cciProductMain.CciClientIdReceiver].NewValue);
                                                        CcisBase.SetNewValue(cciProductItem.CciClientIdReceiver, CcisBase[cciProductMain.CciClientIdPayer].NewValue);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //
            if (Strategy.IsHomogeneous)
            {
                #region Synchronize des call/put
                cciProductMain = GetCciProductMain();
                if ((null != cciProductMain) && (cciProductMain.IsCciOfContainer(pCci.ClientId_WithoutPrefix)))
                {
                    if (cciProductMain.GetType().Equals(typeof(CciProductExchangeTradedDerivative)))
                    {
                        CciProductExchangeTradedDerivative cciProductExchangeTradedDerivative = (CciProductExchangeTradedDerivative)cciProductMain;
                        CciFixTradeCaptureReport cciFixTradeReport = cciProductExchangeTradedDerivative.CciFixTradeCaptureReport;
                        CciFixInstrument cciFixInstrument = cciFixTradeReport.CciFixInstrument;
                        //
                        foreach (CciProductExchangeTradedDerivative cciProductItem in CciSubProduct)
                        {
                            CciFixTradeCaptureReport cciFixTradeReportItem = cciProductItem.CciFixTradeCaptureReport;
                            CciFixInstrument cciFixInstrumentItem = cciFixTradeReportItem.CciFixInstrument;
                            //
                            if (cciProductItem.Product.ProductBase.Id != cciProductMain.Product.ProductBase.Id)
                            {
                                if (cciFixInstrument.IsCci(CciFixInstrument.CciEnum.Sym, pCci))
                                {
                                    if (null != cciProductItem.Product.DerivativeContractValue)
                                    {
                                        if (cciProductItem.Product.DerivativeContractValue == DerivativeContractValueEnum.IDENTICAL)
                                        {
                                            string clientId = cciFixInstrumentItem.CciClientId(CciFixInstrument.CciEnum.Sym);
                                            CcisBase.SetNewValue(clientId, cciFixInstrument.Cci(CciFixInstrument.CciEnum.Sym).NewValue);
                                            CcisBase.Set(clientId, "IsLastInputByUser", true);//Pour Charger l'asset 
                                        }
                                    }
                                }
                                else if (cciFixInstrument.IsCci(CciFixInstrument.CciEnum.PutCall, pCci))
                                {
                                    #region putCallValueEnum
                                    if (null != cciProductItem.Product.PutCallValue)
                                    {
                                        string clientId = cciFixInstrumentItem.CciClientId(CciFixInstrument.CciEnum.PutCall);
                                        if (cciProductItem.Product.PutCallValue == PutCallValueEnum.CALL)
                                        {
                                            CcisBase.SetNewValue(clientId, ReflectionTools.ConvertEnumToString<PutOrCallEnum>(PutOrCallEnum.Call));
                                            CcisBase.Set(clientId, "IsLastInputByUser", true);//Pour Charger l'asset
                                        }
                                        else if (cciProductItem.Product.PutCallValue == PutCallValueEnum.PUT)
                                        {
                                            CcisBase.SetNewValue(clientId, ReflectionTools.ConvertEnumToString<PutOrCallEnum>(PutOrCallEnum.Put));
                                            CcisBase.Set(clientId, "IsLastInputByUser", true);//Pour Charger l'asset
                                        }
                                        else if (cciProductItem.Product.PutCallValue == PutCallValueEnum.IDENTICAL)
                                        {
                                            CcisBase.SetNewValue(clientId, cciFixInstrument.Cci(CciFixInstrument.CciEnum.PutCall).NewValue);
                                            CcisBase.Set(clientId, "IsLastInputByUser", true);//Pour Charger l'asset
                                        }
                                        else if (cciProductItem.Product.PutCallValue == PutCallValueEnum.REVERSE)
                                        {
                                            if (cciFixInstrument.FixInstrument.PutOrCall == PutOrCallEnum.Put)
                                            {
                                                CcisBase.SetNewValue(clientId, ReflectionTools.ConvertEnumToString<PutOrCallEnum>(PutOrCallEnum.Call));
                                                CcisBase.Set(clientId, "IsLastInputByUser", true);
                                            }
                                            else if (cciFixInstrument.FixInstrument.PutOrCall == PutOrCallEnum.Call)
                                            {
                                                CcisBase.SetNewValue(clientId, ReflectionTools.ConvertEnumToString<PutOrCallEnum>(PutOrCallEnum.Put));
                                                CcisBase.Set(clientId, "IsLastInputByUser", true);//Pour Charger l'asset
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else if (cciFixInstrument.IsCci(CciFixInstrument.CciEnum.StrkPx, pCci))
                                {
                                    #region strikeValue
                                    if (null != cciProductItem.Product.StrikeValue)
                                    {
                                        string clientId = cciFixInstrumentItem.CciClientId(CciFixInstrument.CciEnum.StrkPx);
                                        if (cciProductItem.Product.StrikeValue == StrikeValueEnum.IDENTICAL)
                                        {
                                            CcisBase.SetNewValue(clientId, cciFixInstrument.Cci(CciFixInstrument.CciEnum.StrkPx).NewValue);
                                            CcisBase.Set(clientId, "IsLastInputByUser", true);//Pour Charger l'asset
                                        }
                                    }
                                    #endregion
                                }
                                else if (cciFixInstrument.IsCci(CciFixInstrument.CciEnum.MMY, pCci))
                                {
                                    #region maturityValue
                                    if (null != cciProductItem.Product.MaturityValue)
                                    {
                                        string clientId = cciFixInstrumentItem.CciClientId(CciFixInstrument.CciEnum.MMY);
                                        if (cciProductItem.Product.MaturityValue == MaturityValueEnum.IDENTICAL)
                                        {
                                            CcisBase.SetNewValue(clientId, cciFixInstrument.Cci(CciFixInstrument.CciEnum.MMY).NewValue);
                                            CcisBase.Set(clientId, "IsLastInputByUser", true); //Pour Charger l'asset
                                        }
                                    }
                                    #endregion
                                }
                                else if (cciFixTradeReport.IsCci(CciFixTradeCaptureReport.CciEnum.LastQty, pCci))
                                {
                                    #region quantityValue
                                    if (null != cciProductItem.Product.QuantityValue)
                                    {
                                        string clientId = cciFixTradeReportItem.CciClientId(CciFixTradeCaptureReport.CciEnum.LastQty);
                                        CustomCaptureInfo cciQuantityItem = CcisBase[clientId];
                                        if (null != cciQuantityItem)
                                        {
                                            decimal mainQuantityValue = DecFunc.DecValueFromInvariantCulture(cciFixTradeReport.Cci(CciFixTradeCaptureReport.CciEnum.LastQty).NewValue);
                                            //
                                            if (cciProductItem.Product.QuantityValue == QuantityValueEnum.IDENTICAL)
                                            {
                                                cciQuantityItem.NewValue = cciFixTradeReport.Cci(CciFixTradeCaptureReport.CciEnum.LastQty).NewValue;
                                            }
                                            else if (cciProductItem.Product.QuantityValue == QuantityValueEnum.DOUBLE)
                                            {
                                                cciQuantityItem.NewValue = StrFunc.FmtDecimalToInvariantCulture(mainQuantityValue * 2);
                                            }
                                            else if (cciProductItem.Product.QuantityValue == QuantityValueEnum.HALF)
                                            {
                                                cciQuantityItem.NewValue = StrFunc.FmtDecimalToInvariantCulture(mainQuantityValue / 2);
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else if (cciFixTradeReport.IsCci(CciFixTradeCaptureReport.CciEnum.LastPx, pCci))
                                {
                                    #region priceValue
                                    if (null != cciProductItem.Product.PriceValue)
                                    {
                                        string clientId = cciFixTradeReportItem.CciClientId(CciFixTradeCaptureReport.CciEnum.LastPx);
                                        if (cciProductItem.Product.PriceValue == PriceValueEnum.IDENTICAL)
                                        {
                                            CcisBase.SetNewValue(clientId, cciFixTradeReport.Cci(CciFixTradeCaptureReport.CciEnum.LastPx).NewValue);
                                        }
                                        else if (cciProductItem.Product.PriceValue == PriceValueEnum.ZERO)
                                        {
                                            CcisBase.SetNewValue(clientId, "0");
                                        }
                                    }
                                    #endregion
                                }


                            }
                        }
                    }
                }
                #endregion
            }

        }
        #endregion
        #region private Initialize_FromDefault
        /// <summary>
        /// Initialise les ccis en fonction des paramétrages STATIC existants sous INSTRUMENTOF
        /// <para>l'initialisation écrase les donnéees existantes dans le template</para>
        /// <para>l'initialisation s'eefectue en création uniqument</para>
        /// </summary>
        private void Initialize_FromDefault()
        {

            if (Cst.Capture.IsModeNewOrDuplicateOrReflect(CcisBase.CaptureMode))
            {
                foreach (CciProductBase cciProductItem in CciSubProduct)
                {
                    if ((cciProductItem.Product.PutCallValue == EfsML.Enum.PutCallValueEnum.CALL) ||
                        (cciProductItem.Product.PutCallValue == EfsML.Enum.PutCallValueEnum.PUT))
                    {
                        if (cciProductItem.Product.IsExchangeTradedDerivative)
                        {
                            CciProductExchangeTradedDerivative cciProductExChangeTradedDerivative = (CciProductExchangeTradedDerivative)cciProductItem;
                            string clientId = cciProductExChangeTradedDerivative.CciFixTradeCaptureReport.CciFixInstrument.CciClientId(CciFixInstrument.CciEnum.PutCall);
                            if (cciProductItem.Product.PutCallValue == EfsML.Enum.PutCallValueEnum.CALL)
                                CcisBase.SetNewValue(clientId, ReflectionTools.ConvertEnumToString<PutOrCallEnum>(PutOrCallEnum.Call), false);
                            else if (cciProductItem.Product.PutCallValue == EfsML.Enum.PutCallValueEnum.PUT)
                                CcisBase.SetNewValue(clientId, ReflectionTools.ConvertEnumToString<PutOrCallEnum>(PutOrCallEnum.Put), false);
                        }
                    }
                }
            }

        }
        #endregion
        #region private SynchronizeSubProduct
        /// <summary>
        /// Alimente le _cciSubProduct avec les valeurs présentes le _cciProductGlogal
        /// </summary>
        /// <param name="pSubProductIndex"></param>
        private void SynchronizeSubProduct(int pIndex)
        {

            if (null != CciProductGlogal)
            {
                for (int i = 0; i < CcisBase.Count; i++)
                {
                    CustomCaptureInfo cci = CcisBase[i];
                    if (CciProductGlogal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        string clientId_Key = CciProductGlogal.CciContainerKey(cci.ClientId_WithoutPrefix);
                        string clientId = CciSubProduct[pIndex].CciClientId(clientId_Key);
                        if (CcisBase.Contains(clientId))
                            CciSubProduct[pIndex].Cci(clientId_Key).NewValue = cci.NewValue;
                    }
                }
            }

        }

        /*
         FI 20240307 [WI862] Mise en commentaire (déjà effectué via l'appel à ProductContainerBase.SynchronizeFromDataDocument)
        /// <summary>
        /// Dump a clearedDate into DataDocument (FIXML => BizDt)
        /// </summary>
        /// <param name="pData"></param>
        /// FI 20180514 [23734] [23950]
        public override void DumpBizDt_ToDocument(string pData)
        {
            for (int i = 0; i < SubProductLength; i++)
            {
                if (CciSubProduct[i].GetType().Equals(typeof(CciProductExchangeTradedDerivative)))
                {
                    CciSubProduct[i].DumpBizDt_ToDocument(pData);
                }
            }
        }
        */

        #endregion
        #endregion
    }
}
