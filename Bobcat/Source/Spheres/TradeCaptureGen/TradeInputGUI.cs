#region Using Directives
using System;
using System.Data;
using System.IO;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Permission;
using EFS.Tuning;

#endregion Using Directives

namespace EFS.TradeInformation
{
    #region TradeInputGUI
    public class TradeInputGUI : TradeCommonInputGUI
    {
        #region Members
        private string m_TemplateIdentifier;
        private ActorRoleCollection m_ActorRole;
        private ActionTuning m_actionTuning;
        #endregion Members
        #region Accessors
        #region ActorRole
        public ActorRoleCollection ActorRole
        {
            get { return m_ActorRole; }
            set { m_ActorRole = value; }
        }
        #endregion ActorRole
        #region IdP
        public int IdP
        {
            get { return m_IdP; }
            set { m_IdP = value; }
        }
        #endregion IdP
        #region IdI
        public int IdI
        {
            get { return m_IdI; }
            set { m_IdI = value; }
        }
        #endregion IdI
        #region TemplateIdentifier
        public string TemplateIdentifier
        {
            get { return m_TemplateIdentifier; }
            set { m_TemplateIdentifier = value; }
        }
        #endregion TemplateIdentifier
        #region ActionTuning
        public ActionTuning ActionTuning
        {
            get { return m_actionTuning; }
        }
        #endregion
        #endregion
        #region Constructors
        public TradeInputGUI(string pAppPhysicalPath) : base(Cst.IdMenu.InputTrade, pAppPhysicalPath, Cst.SQLCookieGrpElement.SelADMProduct)
        {
        }
        #endregion Constructors
        #region Methods
        #region InitFirstTemplateUsedAsDefault
        public void InitFirstTemplateUsedAsDefault()
        {
            try
            {
                m_TemplateIdentifier = string.Empty;
                CurrentIdScreen = string.Empty;
                string cs = SessionTools.CS;
                //
                string SQLQuery = SQLCst.SELECT + "p.IDENTIFIER as Product, p.IDP as IDProduct, i.IDI as IDInstrument," + Cst.CrLf;
                SQLQuery += "t.IDENTIFIER as IDTemplate, ig.SCREENNAME as IDScreen" + Cst.CrLf;
                SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " i" + Cst.CrLf;
                SQLQuery += OTCmlHelper.GetSQLJoin(cs, Cst.OTCml_TBL.PRODUCT, true, "i.IDP", "p") + Cst.CrLf;
                SQLQuery += OTCmlHelper.GetSQLInstrRestriction("i.IDI", SessionTools.SessionID, SessionTools.IsSessionSysAdmin) + Cst.CrLf;
                SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_TRADE_INSTR_TEMPLATE.ToString() + " t on (t.IDPARENT = i.IDI)" + Cst.CrLf;
                SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENTGUI.ToString() + " ig on (ig.IDI = i.IDI and ig.GUITYPE != " + DataHelper.SQLString(Cst.Capture.GUIType.Full.ToString()) + ")" + Cst.CrLf;
                SQLQuery += SQLCst.WHERE + "(" + OTCmlHelper.GetSQLDataDtEnabled(cs, "i") + ")" + Cst.CrLf;
                //            
                if (0 < m_IdI)
                    SQLQuery += SQLCst.AND + "i.IDI=" + m_IdI.ToString() + Cst.CrLf;
                //
                SQLQuery += SQLCst.ORDERBY + " p.IDENTIFIER, i.IDENTIFIER, t.IDENTIFIER, ig.SCREENNAME" + Cst.CrLf;

                IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, SQLQuery);
                while (dr.Read())
                {
                    string product = Convert.ToString(dr["Product"]);
                    string xmlFile = m_AppPhysicalPath + Cst.CustomCapturePath + @"\" + "Screens" + @"\" + product + ".xml";
                    if (File.Exists(xmlFile) && SessionTools.License.IsLicProductAuthorised(product))
                    {
                        m_IdP = Convert.ToInt32(dr["IDProduct"]);
                        m_IdI = Convert.ToInt32(dr["IDInstrument"]);
                        m_TemplateIdentifier = Convert.ToString(dr["IDTemplate"]);
                        m_CurrentIdScreen = Convert.ToString(dr["IDScreen"]);
                        break;
                    }
                }
                dr.Close();

            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("InputTradeGUI.InitFirstTemplateUsedAsDefault", ex); }
        }
        #endregion InitFirstTemplateUsedAsDefault
        #region public InitializeActionTuning
        public void InitializeActionTuning(string pCs, int pIdA, ActorAncestor pActorActorAncestor)
        {
            int idPermission = Permission.GetIdPermission(PermissionTools.GetPermissionEnum(CaptureMode));
            m_actionTuning = new ActionTuning(pCs, m_IdI, idPermission, pIdA, pActorActorAncestor);
        }
        #endregion
        #endregion Methods
    }
    #endregion TradeInputGUI
}
