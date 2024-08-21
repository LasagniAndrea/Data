#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web; 

using EFS.Referential;
using EFS.TradeInformation;
using EFS.Spheres;
#endregion Using Directives

namespace EFS.Spheres
{
    
    public partial class EventResetPage : EventZoomPage
    {
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20160804 [Migration TFS] Modify
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // FI 20160804 [Migration TFS] new m_XSLEvent
            //m_XSLEvent = SessionTools.NewAppInstance().SearchFile(SessionTools.CS, @"~\OTCML\XSL_Files\Event\EventReset.xslt");
            SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, @"~\GUIOutput\Event\EventReset.xslt", ref m_XSLEvent);
            
            m_TableNames[0] = "Reset";
            m_TableNames[1] = "SelfAverage";
            m_TableNames[2] = "SelfReset";
        }

        #endregion
    }
}
