#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Xml;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Common.Web;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;



using EFS.Status;
using EFS.Tuning;
using EFS.Permission;


using EfsML;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;

   
#endregion Using Directives

namespace EFS.TradeInformation.Export
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20130321 [] Cette classe est-elle tjs utilisée ??
    public class ExportTradeCommonCustomCaptureInfos
    {
        #region Members
        [System.Xml.Serialization.XmlArray("customCaptureInfos")]
        [System.Xml.Serialization.XmlArrayItem("customCaptureInfo", typeof(CustomCaptureInfoDynamicData))]
        public TradeCommonCustomCaptureInfos ccis;
        #endregion

        #region Constructor
        public ExportTradeCommonCustomCaptureInfos()
        {
            ccis = new TradeCommonCustomCaptureInfos();
        }
        #endregion
    }
}
