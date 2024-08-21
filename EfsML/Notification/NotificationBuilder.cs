using EFS.ACommon;
using EFS.Common;
using EfsML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;


namespace EfsML.Notification
{
    /// <summary>
    /// Permer de générer les notications
    /// </summary>
    public class NotificationBuilder
    {

        /// <summary>
        /// 
        /// </summary>
        /// FI 2020623 [XXXXX] Add
        private SetErrorWarning _setErrorWarning = null;

        /// <summary>
        /// AttachedDoc delegate Method
        /// </summary>
        /// FI 20191007 [XXXXX] Add
        private AddAttachedDoc _addAttachedDoc = null;


        #region Members
        /// <summary>
        /// IdMCO
        /// </summary>
        private int _idMCO;

        /// <summary>
        /// 
        /// </summary>
        private readonly Encoding _encoding;
        /// <summary>
        /// 
        /// </summary>
        private readonly AppSession _session;
        /// <summary>
        /// 
        /// </summary>
        private readonly CnfMessage _cnfMessage;

        /// <summary>
        /// Format du message de sortie par défaut
        /// </summary>
        private readonly DocTypeEnum _outputMsgDocTypeDefault;

        /// <summary>
        /// Format du message de sortie, ce format est déterminer par le xsl (utilisation de xsl:output method="xxx")
        /// </summary>
        private DocTypeEnum _outputMsgDocType;
        //
        /// <summary>
        /// Format output method défini sous xsl:output method="xxx"
        /// </summary>
        /// Attention si génération d'un pdf xxx vaut xml, le résultat de la transformation est un XML dont l'interprétation génére un pdf 
        private string _outputXslDocType;

        /// <summary>
        /// DocType en cas de sortie binaire (pdf, doc, etc...)  
        /// </summary>
        private string _outputBinDocType;
        /// <summary>
        /// 
        /// </summary>
        private NotificationDocumentContainer _notificationDocument;

        /// <summary>
        /// Flux XML resultat de la serialization du message 
        /// </summary>
        private String _serializeDoc;
        //
        /// <summary>
        /// Flux XML resultat sans namespace ( les transformations xsl s'applique sur ce flux) 
        /// </summary>
        private String _serializeDocWithoutXmlns;

        /// <summary>
        /// Flux string  resultat de la transformation XSL (si xsloutput = in (txt,xml,html))     
        /// </summary>
        private String _result;

        /// <summary>
        /// Flux Binaire resultat de la transformation PDF (si xsloutput = in (pdf,....))     
        /// </summary>
        private byte[] _binaryResult;

        /// <summary>
        /// 
        /// </summary>
        private string _xslFile;


        #endregion Members

        #region properties
        /// <summary>
        /// 
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding; }
        }
        /// <summary>
        /// 
        /// </summary>
        public CnfMessage CnfMessage
        {
            get
            {
                return _cnfMessage;
            }
        }
        /// <summary>
        /// Obtient la notification
        /// </summary>
        public NotificationDocumentContainer NotificationDocument
        {
            get { return _notificationDocument; }
        }
        /// <summary>
        /// Obtient la serialization de la notification
        /// </summary>
        public string SerializeNotificationDoc
        {
            get { return _serializeDoc; }
        }
        /// <summary>
        /// Obtient la serialization de la notification sans les espaces de nom
        /// </summary>
        public string SerializeDocWithoutXmlns
        {
            get { return _serializeDocWithoutXmlns; }
        }

        /// <summary>
        /// Obtient le résultat de la transformation xsl 
        /// </summary>
        public string Result
        {
            get { return _result; }
        }
        /// <summary>
        /// 
        /// </summary>
        public byte[] BinaryResult
        {
            get { return _binaryResult; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string OutputBinDocType
        {
            get { return _outputBinDocType; }
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public string OutputBinDocTypeMIME
        {
            get
            {
                string ret;
                switch (OutputBinDocType)
                {
                    case "pdf":
                        ret = Cst.TypeMIME.Application.Pdf;
                        break;
                    default:
                        throw new InvalidOperationException(StrFunc.AppendFormat(" Type MIME {0} is unknown", OutputBinDocType));
                }
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string OutputXslDocType
        {
            get { return _outputXslDocType; }
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public string OutputXslDocTypeMIME
        {
            get
            {
                string ret;
                switch (OutputXslDocType)
                {
                    case "xml":
                        ret = Cst.TypeMIME.Text.Xml;
                        break;
                    case "txt":
                        ret = Cst.TypeMIME.Text.Plain;
                        break;
                    case "html":
                        ret = Cst.TypeMIME.Text.Html;
                        break;
                    default:
                        throw new InvalidOperationException(OutputXslDocTypeMIME + " Type MIME unknown");
                }
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public DocTypeEnum OutputMsgDocType
        {
            get { return _outputMsgDocType; }
        }
        /// <summary>
        /// Obtient le fichier xsl de la transformation
        /// </summary>
        public string XslFile
        {
            get { return _xslFile; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAppInstance"></param>
        /// <param name="pCnfMessage"></param>
        /// <param name="pDefaultDocType"></param>
        /// <param name="pEncoding"></param>
        public NotificationBuilder(AppSession pSession, CnfMessage pCnfMessage, DocTypeEnum pDefaultDocType, Encoding pEncoding)
        {
            _outputMsgDocTypeDefault = pDefaultDocType;
            _session = pSession;
            _cnfMessage = pCnfMessage;
            _encoding = pEncoding;
        }
        /// <summary>
        /// Sortie par défaut est html
        /// </summary>
        /// <param name="pSession"></param>
        /// <param name="pCnfMessage"></param>
        public NotificationBuilder(AppSession pSession, CnfMessage pCnfMessage)
            : this(pSession, pCnfMessage, DocTypeEnum.html, new UnicodeEncoding())
        {
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// Génère le message de notification
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdMCO">IdMCO</param>
        /// <param name="pNcs">Système de communication</param>
        /// <param name="pIdInci_SendBy"></param>
        /// <param name="pConfirmationChain">Chaîne de confirmation</param>
        /// <param name="pInciIems">Liste des destinataires</param>
        /// <param name="pIdT">Liste des trades</param>
        /// <param name="pProductBase"></param>
        /// FI 20120829 [18048] modification de la signature ajout de la colonne pDate2
        /// FI 20150520 [XXXXX] Modify
        public void SetNotificationDocument(string pCS, DateTime pDate, Nullable<DateTime> pDate2, int pIdMCO, NotificationConfirmationSystem pNcs,
            int pIdInci_SendBy, ConfirmationChain pConfirmationChain, InciItems pInciIems, int[] pIdT, IProductBase pProductBase)
        {

            _idMCO = pIdMCO;

            //Chargement du document
            _notificationDocument = new NotificationDocumentContainer(((IProductBase)pProductBase).CreateConfirmationMessageDocument());

            // FI 20150520 [XXXXX] Cal lInitializeDelegate
            _notificationDocument.InitializeDelegate( this._setErrorWarning);


            _notificationDocument.Initialize(pCS, pIdMCO, _cnfMessage, pNcs, pConfirmationChain, pInciIems.inciItem,
                pIdT, pDate, pDate2, pIdInci_SendBy, _session);
        }

        /// <summary>
        /// Serialize le message de notification
        /// </summary>
        /// FI 20120720 Refactoring _serializeDoc est de type string désormais 
        public void SerializeDoc()
        {
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(_notificationDocument.NotificationDocument.GetType(), _notificationDocument.NotificationDocument);
            SetSerializeOverrides(_notificationDocument.NotificationDocument.EfsMLversion, serializeInfo);

            StringBuilder _doc = CacheSerializer.Serialize(serializeInfo, _encoding);

            //
            // EG 20100618 Verrue de merde pour pouvoit visualiser les colonnes de type XML (ex EVENTSI)
            _doc = _doc.Replace("&lt;", "<");
            _doc = _doc.Replace("&gt;", ">");
            // FI 20230103 [26204] Suppression des replace _serializeDoc et _serializeDocWithoutXmlns commencent par une déclaration xml
            //_doc = _doc.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", string.Empty);
            //_doc = _doc.Replace(@"<?xml version=""1.0"" encoding=""UTF-16""?>", string.Empty);
            //_doc = _doc.Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>", string.Empty);
            //_doc = _doc.Replace(@"<?xml version=""1.0"" encoding=""UTF-8""?>", string.Empty);

            _serializeDoc = _doc.ToString();
            _serializeDocWithoutXmlns = XSLTTools.RemoveXmlnsAlias(new StringBuilder(_serializeDoc), _encoding);
        }

        /// <summary>
        /// Transforme le document 
        /// <para>Alimente les membres _result et_binaryResult</para>
        /// <para>Lorsque le résultat de la transformation est de type HTML, génère un pdf (iTextSharp)</para>
        /// </summary>
        /// <param name="pProductBase">Product du trade (doit être renseigné sur les notification MONOTRADE)</param>
        /// <param name="pIdT">IdT du trade (doit être renseigné sur les notification MONOTRADE)</param>
        /// <param name="pIdProcess">Doit être supérieur à zéro pour alimentation de attachedDoc en cas d'erreur</param>
        /// FI 20120720 Refactoring (nouveaux paramètres pProductBase et pIdT)
        public void TransForm(string pCs, IProductBase pProductBase, Nullable<int> pIdT, int pIdProcess)
        {
            // Si le résulat est un HTML, spheres Génère également et systématiquement un pdf
            // Le pdf est généré sous le TemporaryPath
            TransForm(pCs, true, _session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True), pProductBase, pIdT, pIdProcess);
        }

        /// <summary>
        /// Transforme le document 
        /// <para>Alimente les membres _result et_binaryResult</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIsConvertHtmlToPdf">Si true, lorsque le résultat de la transformation est un HTML, Spheres®  génère également un Pdf dans le répertoire temporaire</param>
        /// <param name="pTempPath"></param>
        /// <param name="pProductBase">Product du trade (doit être renseigné sur les notification MONOTRADE)</param>
        /// <param name="pIdT">IdT du trade (doit être renseigné sur les notification MONOTRADE)</param>
        /// <param name="pIdProcess">Doit être supérieur à zéro pour alimentation de attachedDoc en cas d'erreur</param>
        /// FI 20120720 Refactoring (nouveaux paramètres pProductBase et pIdT), utilisatio de la méthode GetXslParam
        public void TransForm(string pCS, bool pIsConvertHtmlToPdf, string pTempPath, IProductBase pProductBase, Nullable<int> pIdT, int pIdProcess)
        {
            //isConvertHtmlToPdf devrait être paramétrable dans CNFMESSAGE

            string srcFile = _cnfMessage.xsltFile;
            string tgtFile = string.Empty;
            bool isFound = File.Exists(srcFile);
            if (isFound)
            {
                tgtFile = srcFile;
            }
            else
            {
                isFound = _session.AppInstance.SearchFile2(pCS, srcFile, ref tgtFile);
            }
            if (isFound)
            {
                _xslFile = tgtFile;
            }
            else
            {
                string errInfo = "XSL/XSLT: File not found! [File: " + srcFile + "][Full File: " + tgtFile + "][Current Directory: " + Directory.GetCurrentDirectory() + "]";
                throw new FileNotFoundException(errInfo, _cnfMessage.xsltFile);
            }
            
            string defaultXslDocType;
            if (DocTypeEnum.pdf == _outputMsgDocTypeDefault)
                defaultXslDocType = "xml";
            else
                defaultXslDocType = _outputMsgDocTypeDefault.ToString();
                            
            _outputXslDocType = XSLTTools.GetXslOutputStreamType(XslFile, defaultXslDocType);
            if ("xml" == _outputXslDocType)
                _outputBinDocType = XSLTTools.GetOutputStreamType(XslFile, "pdf");
            //
            if (StrFunc.IsFilled(_outputBinDocType))
                _outputMsgDocType = (DocTypeEnum)System.Enum.Parse(typeof(DocTypeEnum), _outputBinDocType);
            else
                _outputMsgDocType = (DocTypeEnum)System.Enum.Parse(typeof(DocTypeEnum), _outputXslDocType);
            //
            //Recherche des paramètres de la transformation Xsl
            Hashtable xsltParamList = GetXslParam(pCS, pProductBase, pIdT);

            try
            {
                //Transformation du flux
                _result = XSLTTools.TransformXml(new StringBuilder(_serializeDocWithoutXmlns), XslFile, xsltParamList, null);
            }
            catch (XsltException)// FI 20191007 [XXXXX] Add  XsltException
            {
                try
                {
                    if (pIdProcess > 0)
                        AddXmlFlowAttachedDoc(pCS, pIdProcess, _session.IdA, pTempPath);
                }
                catch { }
                throw;
            }

            // 20081013 RD - Produire le PDF à partir du flux HTML (Via iTextSharp)
            //
            if (DocTypeEnum.html == _outputMsgDocType)
            {
                //
                _result = _result.Replace(@"<META http-equiv=""Content-Type"" content=""text/html; charset=utf-16"">", @"<META http-equiv=""Content-Type"" content=""text/html; charset=utf-16""/>");
                _result = _result.Replace(@"<br>", @"<br/>");
                _result = _result.Replace(@"endelement=""true"">", @"/>");
                //
                // 20081015 RD - Produire le document PDF à partir du flux HTML (Via iTextSharp)
                // iTextSharp a besoin d'un Stream (FileStream par exemple) pour pouvoir générer le PDF destination
                // Je lui passe le chemin temporaire
                //
                if (pIsConvertHtmlToPdf)
                {
                    string tempPDFPath = pTempPath + @"/TempPDF" + SystemTools.GetNewGUID() + ".pdf";
                    try
                    {
                        StringBuilder pdfSb = new StringBuilder(XMLTools.ReplaceHtmlTagImage(pCS, _result.ToString(), pTempPath, pTempPath));
                        //
                        FileTools.WriteHTMLToPDF(pdfSb.ToString(), tempPDFPath);
                        //
                        _binaryResult = FileTools.ReadFileToBytes(tempPDFPath);
                        _outputBinDocType = DocTypeEnum.pdf.ToString();
                        //   
                    }
                    catch
                    {
                        //20081022 FI add try catch sans gestion d'erreur
                        //SI generation auto de PDF depuis un HTML, on ne plante pas, c'est le résultat HTML qui est important
                        //Ce principe pourra sera peut-être à revoir dans l'avenir
                        //Aujourd'hui nous sommes dans ce cas lors de la génération des message par les services 
                    }
                    finally { FileTools.FileDelete2(tempPDFPath); }
                }
            }
            //20090213 [16505] Appel au moteur Fop si sortie pdf 
            else if (DocTypeEnum.pdf == _outputMsgDocType)
            {
                // EG 20160404 Migration vs2013
                //_binaryResult = FopEngine_V2.TransformToByte(pCS, _result, pTempPath);
                _binaryResult = FopEngine.TransformToByte(pCS, _result, pTempPath);
            }
        }

        /// <summary>
        /// Retourne les paramètres par défaut de la transformation xsl
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase">Product du trade (doit être renseigné sur les notification MONOTRADE)</param>
        /// <param name="pIdT">IdT du trade (doit être renseigné sur les notification MONOTRADE)</param>
        /// FI 20120720 Add GetDefaultXslParam
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private StringData[] GetDefaultXslParam(string pCS, IProductBase pProductBase, Nullable<int> pIdT)
        {
            StringData strDataDefaultLanguage = new StringData("pCurrentCulture", TypeData.TypeDataEnum.@string, _notificationDocument.Culture, string.Empty);
            StringData[] ret;
            if (_cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE)
            {
                if (null == pIdT)
                    throw new ArgumentException("null value for Parameter {pIdT} is not allowed");
                if (null == pProductBase)
                    throw new ArgumentException("null value for Parameter {pProductBase} is not allowed");

                int idT = pIdT.Value;

                SQL_TradeCommon sqlTrade = new SQL_TradeCommon(pCS, idT);
                sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDI", "EXTLLINK" });
                SQL_Instrument sqlInstr = new SQL_Instrument(pCS, sqlTrade.IdI);
                sqlInstr.LoadTable(new string[] { "DISPLAYNAME", "DESCRIPTION" });
                //
                StringData strDataProduct = new StringData("pProduct", TypeData.TypeDataEnum.@string, pProductBase.GetType().Name, string.Empty);
                StringData strDataInstr = new StringData("pInstrument", TypeData.TypeDataEnum.@string, sqlInstr.DisplayName, string.Empty);
                StringData strDataInstrDescription = new StringData("pInstrumentDescription", TypeData.TypeDataEnum.@string, sqlInstr.Description, string.Empty);
                StringData strDataExtlLink = new StringData("pTradeExtlLink", TypeData.TypeDataEnum.@string, sqlTrade.ExtlLink, string.Empty);
                //
                ret = new StringData[] {
                        strDataDefaultLanguage,
                        strDataProduct,
                        strDataInstr,
                        strDataInstrDescription,
                        strDataExtlLink
                    };
            }
            else
                ret = new StringData[] { strDataDefaultLanguage };

            return ret;
        }

        /// <summary>
        /// Retourne les paramètres de la transformation xsl
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        /// FI 20120720 Add GetXslParam
        private Hashtable GetXslParam(string pCS, IProductBase pProductBase, Nullable<int> pIdT)
        {
            Hashtable ret = new Hashtable();
            //
            if (_cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE)
            {
                if (null == pIdT)
                    throw new ArgumentException("null value for Parameter {pIdT} is not allowed");
                if (null == pProductBase)
                    throw new ArgumentException("null value for Parameter {pProductBase} is not allowed");
            }

            //Paramètre par défaut
            StringData[] defaultXslParam;
            if (_cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE)
                defaultXslParam = GetDefaultXslParam(pCS, pProductBase, pIdT);
            else
                defaultXslParam = GetDefaultXslParam(pCS, null, null);

            // Add Param Default
            if (ArrFunc.IsFilled(defaultXslParam))
            {
                for (int i = 0; i < ArrFunc.Count(defaultXslParam); i++)
                    ret.Add(defaultXslParam[i].name, defaultXslParam[i].value);
            }
            //
            //Paramètres propres au message 
            if (_cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE)
            {
                //Ne pas charger  si _cnfMessage.xsltTransformParam existe déjà (cas des transformations depuis la saisie)
                if (null == _cnfMessage.xsltTransformParam && _cnfMessage.idGParamSpecified)
                    _cnfMessage.LoadXsltTransformParam(pCS, pIdT.Value);
                //
                if (ArrFunc.IsFilled(_cnfMessage.xsltTransformParam))
                {
                    for (int i = 0; i < _cnfMessage.xsltTransformParam.Length; i++)
                        ret.Add(_cnfMessage.xsltTransformParam[i].name, _cnfMessage.xsltTransformParam[i].value);
                }
            }
            return ret;
        }
        /// <summary>
        /// Mise en place des informations de serialization en fonction de la version du document
        /// </summary>
        /// <param name="EfsMLversion"></param>
        /// <param name="serializeInfo"></param>
        /// FI 20140821 [20275] Modify
        /// FI 20151019 [21317] Modify
        /// FI 20170116 [21916] Modify
        // EG 20180523 Order de serialisation de RptSide sur FxOptionLeg (suite à report et extension Provisions sur FX)
        private static void SetSerializeOverrides(EfsMLDocumentVersionEnum EfsMLversion, EFS_SerializeInfoBase serializeInfo)
        {
            if (EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
            {
                XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();

                //IdPosActionDet devient OTCmlId
                XmlAttributes xmlAttributes = new XmlAttributes
                {
                    //xmlAttributes.XmlAttribute = new XmlAttributeAttribute("SpheresId");
                    XmlAttribute = new XmlAttributeAttribute("OTCmlId")
                };
                xmlAttributeOverrides.Add(typeof(PosAction), "IdPosActionDet", xmlAttributes);

                //Il faudrait que tous les OTCmlId soient remplacés par des SpheresId
                //Pour l'instant on conserve OTCmlId
                //xmlAttributes = new XmlAttributes();
                //xmlAttributes.XmlAttribute = new XmlAttributeAttribute("SpheresId");
                //xmlAttributeOverrides.Add(typeof(TradeIdentification), "otcmlId", xmlAttributes);

                //tradeIdentifier devient tradeId
                xmlAttributes = new XmlAttributes
                {
                    XmlAttribute = new XmlAttributeAttribute("tradeId")
                };
                xmlAttributeOverrides.Add(typeof(TradeIdentification), "tradeIdentifier", xmlAttributes);

                //idb devient idB
                xmlAttributes = new XmlAttributes
                {
                    XmlAttribute = new XmlAttributeAttribute("idB")
                };
                xmlAttributeOverrides.Add(typeof(PosSynthetic), "idb", xmlAttributes);

                //dailyQty devient qty
                xmlAttributes = new XmlAttributes
                {
                    XmlAttribute = new XmlAttributeAttribute("qty")
                };
                xmlAttributeOverrides.Add(typeof(PosSynthetic), "qty", xmlAttributes);

                //avgPrice devient avgPx
                xmlAttributes = new XmlAttributes
                {
                    XmlAttribute = new XmlAttributeAttribute("avgPx")
                };
                xmlAttributeOverrides.Add(typeof(PosSynthetic), "avgPrice", xmlAttributes);

                // RD 20130722 [18745] Optimiser les éléments ConvertedPrices
                // RD 20140114 [18600] les éléments de prix peuvent contenir des sous éléments destinés à l'affichage en fraction (75/100, ...) 

                // L'élément "ConvertedStrikePrice" devient un élément "strk"
                xmlAttributes = new XmlAttributes();
                XmlElementAttribute element = new XmlElementAttribute("strk");
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(ConvertedPrices), "ConvertedStrikePrice", xmlAttributes);

                // L'élément "ConvertedLongAveragePrice" devient un élément "longAvgPx"
                xmlAttributes = new XmlAttributes();
                element = new XmlElementAttribute("longAvgPx");
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(ConvertedPrices), "ConvertedLongAveragePrice", xmlAttributes);

                // L'élément "ConvertedShortAveragePrice" devient un élément "shortAvgPx"
                xmlAttributes = new XmlAttributes();
                element = new XmlElementAttribute("shortAvgPx");
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(ConvertedPrices), "ConvertedShortAveragePrice", xmlAttributes);

                // L'élément "ConvertedSynthPositionPrice" devient un élément "avgPx"
                xmlAttributes = new XmlAttributes();
                element = new XmlElementAttribute("avgPx");
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(ConvertedPrices), "ConvertedSynthPositionPrice", xmlAttributes);

                // L'élément "ConvertedClearingPrice" devient un élément "clrPx"
                xmlAttributes = new XmlAttributes();
                element = new XmlElementAttribute("clrPx");
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(ConvertedPrices), "ConvertedClearingPrice", xmlAttributes);

                // FI 20140821 [20275] RptSide est serialisé
                // FI 20170116 [21916] RptSide (R majuscule)
                xmlAttributes = new XmlAttributes();
                element = new XmlElementAttribute("RptSide")
                {
                    Order = 6
                };
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(FpML.v44.Eq.Shared.ReturnSwapBase), "RptSide", xmlAttributes);

                // FI 20150331 [XXPOC] RptSide est serialisé
                // FI 20170116 [21916] RptSide (R majuscule)
                xmlAttributes = new XmlAttributes();
                element = new XmlElementAttribute("RptSide")
                {
                    Order = 10
                };
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(FpML.v44.Fx.FxLeg), "RptSide", xmlAttributes);

                // FI 20150331 [XXPOC] RptSide est serialisé
                // FI 20170116 [21916] RptSide (R majuscule)
                xmlAttributes = new XmlAttributes();
                element = new XmlElementAttribute("RptSide")
                {
                    Order = 18
                };
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(FpML.v44.Fx.FxOptionLeg), "RptSide", xmlAttributes);

                // FI 20151019 [21317] RptSide est serialisé
                // FI 20170116 [21916] RptSide (R majuscule)
                xmlAttributes = new XmlAttributes();
                element = new XmlElementAttribute("RptSide")
                {
                    Order = 8
                };
                xmlAttributes.XmlElements.Add(element);
                xmlAttributeOverrides.Add(typeof(EfsML.v30.Security.DebtSecurityTransaction), "RptSide", xmlAttributes);


                serializeInfo.XmlAttributeOverrides = xmlAttributeOverrides;
            }
        }

        /// <summary>
        /// Initialise le delegue chargé d'écrire dans le log
        /// </summary>
        /// <param name="pLog"></param>
        /// FI 20150520 [XXXXX] Add Method
        /// FI 20190201 [24495] Add Method
        public void InitializeDelegate(AddAttachedDoc pAddAttachedDoc, SetErrorWarning pSetErrorWarning)
        {
            _addAttachedDoc = pAddAttachedDoc;
            _setErrorWarning = pSetErrorWarning;
        }

        /// <summary>
        /// Alimente AttachedDoc avec le flux XML de la messagerie
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdProcess"></param>
        /// <param name="pIdA"></param>
        /// <param name="pTempPath"></para
        /// FI 20191007 [XXXXX] Add Method
        private void AddXmlFlowAttachedDoc(string pCS, int pIdProcess, int pIdA, string pTempPath)
        {
            if (pIdProcess <= 0)
                throw new ArgumentException($"{nameof(pIdProcess)} value is not valid");

            string folder = pTempPath + @"/AttachedDocMCO";
            SystemIOTools.CreateDirectory(folder);

            XmlDocument doc = new XmlDocument();
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true
            };

            string fileXml = StrFunc.AppendFormat("MCO_XML_{0}.xml", _idMCO.ToString());

            //Write xml
            using (XmlWriter xmlWritter = XmlTextWriter.Create(folder + @"\" + fileXml, xmlWriterSettings))
            {
                doc.LoadXml(SerializeDocWithoutXmlns);
                doc.Save(xmlWritter);
                xmlWritter.Close();
            }

            byte[] data = FileTools.ReadFileToBytes(folder + @"\" + fileXml);
            _addAttachedDoc.Invoke(pCS, pIdProcess, pIdA, data, fileXml, "xml");
        }

        #endregion
    }
}
