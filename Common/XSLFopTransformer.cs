using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Reflection;
using System.IO;
using System.Data;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml.Serialization;
// EG 20160404 Migration vs2013
//using org.apache.fop.apps;  
//using ApacheFop;
using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common.Log;

// EG 20160404 Migration vs2013
using Fonet;
using Fonet.Render;
using Fonet.Render.Pdf;

namespace EFS.Common
{
    // EG 20160404 Migration vs2013 (Accès à FOP C# ()
    /// <summary>
    /// Classe scellée pour génération PDF via FOP.Net (C#)
    /// => Remplace FOP (J#)
    /// </summary>
    public sealed class FopEngine
    {
        #region Constructor
        public FopEngine() { }
        #endregion Constructor
        
        #region Methods
        /// <summary>
        /// Retourne le résultat binaire d'un fichier (Génération d'un fichier binaire de type pdf et lecture dans la foulée)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pXml">flux XML de type fo:xml</param>
        /// <param name="pTemporaryPath">Répertoire de travail dans lequel sera généré le fichier pdf</param>
        /// <returns></returns>
        public static byte[] TransformToByte(string pCS, string pXml, string pTemporaryPath)
        {
            byte[] _binaryResult = null;
            
            string pdfFilePath = pTemporaryPath + @"\" + FileTools.GetUniqueName("Result", "PDF", "yyyyMMddHHmmssfff") + ".pdf";
            
            if (Cst.ErrLevel.SUCCESS == WritePDF(pCS, pXml, pTemporaryPath, pdfFilePath))
                _binaryResult = FileTools.ReadFileToBytes(pdfFilePath);
            
            return _binaryResult;
        }

        /// <summary>
        /// Génération d'un fichier pdf à partir d'un flux fo:xml
        /// <para></para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pXml"></param>
        /// <param name="pTemporaryPath"></param>
        /// <param name="pPdfFilePath"></param>
        /// <returns></returns>
        public static Cst.ErrLevel WritePDF(string pCS, string pXml, string pTemporaryPath, string pPdfFilePath)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            string xml = XMLTools.ReplaceXslFoTagImage(pCS, pXml, pTemporaryPath);
            xml = XMLTools.ReplaceTagImage(pCS, xml, "fo:block", pTemporaryPath, string.Empty);
#if DEBUG
            string write_File = StrFunc.AppendFormat(@"{0}\TransformResult.xml", pTemporaryPath);
            FileTools.WriteStringToFile(xml, write_File);
#endif
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            FonetDriver driver = FonetDriver.Make();
            // FI 20190701 [XXXXX] Alimentation de la trace avec les infos issus de FoNet
            if (null != AppInstance.TraceManager)
            {
                driver.OnError += new FonetDriver.FonetEventHandler(FonEventOnError);
                driver.OnWarning += new FonetDriver.FonetEventHandler(FonEventOnWarnign);
                driver.OnInfo += new FonetDriver.FonetEventHandler(FonEventOnInfo);
            }
            driver.CloseOnExit = true;
            // FI 20210701 [XXXXX] using instruction pour appel sous-jacent à dispose() 
            using (FileStream fs = new FileStream(pPdfFilePath, FileMode.Create))
            {
                driver.Render(xmlDoc, fs);
            }

            // FI 20210701 [XXXXX]
            // Pour empêcher les fuites de ressources, désabonnement aux événements 
            if (null != AppInstance.TraceManager)
            {
                driver.OnInfo -= FonEventOnError;
                driver.OnWarning -= FonEventOnWarnign;
                driver.OnInfo -= FonEventOnInfo;
            }

            ret = Cst.ErrLevel.SUCCESS;

            return ret;
        }
    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Driver"></param>
        /// <param name="e"></param>
        /// FI 20190701 [XXXXX] Add Method
        private static void FonEventOnInfo(object Driver, FonetEventArgs e)
        {
            FonEventTrace(Driver, TraceEventType.Information, e.GetMessage());
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Driver"></param>
        /// <param name="e"></param>
        /// FI 20190701 [XXXXX] Add Method
        private static void FonEventOnError(object Driver, FonetEventArgs e)
        {
            FonEventTrace(Driver, TraceEventType.Error, e.GetMessage());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Driver"></param>
        /// <param name="e"></param>
        /// FI 20190701 [XXXXX] Add Method
        private static void FonEventOnWarnign(object Driver, FonetEventArgs e)
        {
            FonEventTrace(Driver, TraceEventType.Warning, e.GetMessage());
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Driver"></param>
        /// <param name="e"></param>
        /// FI 20190701 [XXXXX] Add Method
        private static void FonEventTrace(object Driver, TraceEventType pTraceEventType, string pMessage)
        {
            if (null != AppInstance.TraceManager)
            {
                TraceEvent traceEvent = new TraceEvent(pTraceEventType, pMessage);
                AppInstance.TraceManager.Trace(Driver, traceEvent);
            }
        }
        #endregion Methods
    }
}
