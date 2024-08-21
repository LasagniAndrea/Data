#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.Common.Web;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.GUI;
using EFS.GUI.CCI;
using EFS.GUI.Interface;




using EFS.Status;
using EFS.Tuning;
using EFS.Permission;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives


public partial class Trial_LoadSwapswire : System.Web.UI.Page
{

    // EG 20231127 [WI752] Exclusion de FpML 4.2
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            ///Chargement d'un doc FpML dans FpML.v42.Doc.DataDocument
            string fileName1 = Server.MapPath(@"~\Trial\SwapsWire\Files\Fpml_FRA.xml");
            XmlSerializer fpMLSerializer = new System.Xml.Serialization.XmlSerializer(typeof(FpML.v44.Doc.DataDocument));
            XmlDocument xml1 = new XmlDocument();
            xml1.Load(fileName1);
            StringReader reader1 = new StringReader(xml1.OuterXml);
            IDataDocument fpMLDataDoc = (IDataDocument) fpMLSerializer.Deserialize(reader1);


            ///Chargement d'un doc FpML dans EfsML.v20.EfsDocument
            string fileName2 = Server.MapPath(@"~\Trial\SwapsWire\Files\EfsML_FRA.xml");
            XmlSerializer efsMLSerializer = new System.Xml.Serialization.XmlSerializer(typeof(EfsML.v30.Doc.EfsDocument));
            XmlDocument xml2 = new XmlDocument();
            xml2.Load(fileName2);
            StringReader reader2 = new StringReader(xml2.OuterXml);
            IDataDocument EfsMLMLDataDoc = (IDataDocument)efsMLSerializer.Deserialize(reader2);
            //
            CacheSerializer.Clear(); 
            EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(xml2.OuterXml);
            IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
            //
        }
        catch (Exception ) { };
    }

}
