#region using directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Restriction;
using EFS.SpheresIO;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

using SpheresMenu = EFS.Common.Web.Menu;


using System.Web.ApplicationServices;
#endregion using directives

namespace EFS.GridViewProcessor
{
    /// <summary>
    /// Classe regroupant les methodes statiques employées pour le referenciel (classe referential) (en mode form ou list)
    /// </summary>
    public sealed class RepositoryTools
    {
        #region const
        public const string SuffixEdit = "EDIT";
        public const string SuffixAggregate = "AGGREGATE";
        #endregion

        #region Methods
        #region public DeserializeXML_ForModeRW
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdMenu"></param>
        /// <param name="pType"></param>
        /// <param name="pObjectName"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="opReferentialsReferential"></param>
        /// Use by SQLExport 
        public static void DeserializeXML_ForModeRW_25(string pCS, string pIdMenu, Cst.ListType pType, string pObjectName,
            string pCondApp, string[] pParam, Dictionary<string, StringDynamicData> pDynamicArgs, out Referential opReferentialsReferential)
        {
            DeserializeXML_ForModeRW(pCS, pIdMenu, pType, pObjectName, pCondApp, pParam, pDynamicArgs, out opReferentialsReferential);

            // Add CmptLevel = "2.5", afin de forcer le chargement des colonnes.
            opReferentialsReferential.CmptLevelSpecified = true;
            opReferentialsReferential.CmptLevel = "2.5";
        }
        /// <summary>
        /// Renseigne une classe referential à partir d'un fichier XML de type "Repository"
        /// </summary>
        /// <param name="pCS">chaine pour la connexion SGBD</param>
        /// <param name="pIdMenu">Id du menu appelant</param>
        /// <param name="pType">type de consultation</param>
        /// <param name="pObjectName">object xml utilisé pour la Deserialization</param>
        /// <param name="pConditionApplicationForSQLWhere">condition d'application si nécessaire</param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="opReferentialsReferential">OUT classe referential à renseigner</param>
        /// FI 20141211 [20563] Modification Siganture add pDynamicArgs
        public static void DeserializeXML_ForModeRW(string pCS, string pIdMenu, Cst.ListType pType, string pObjectName,
            string pCondApp, string[] pParam, Dictionary<string, StringDynamicData> pDynamicArgs, out Referential opReferentialsReferential)
        {
            List<string> lstObject = new List<string>();
            lstObject.Add(pObjectName);
            DeserializeXML_ForModeRW(pCS, pIdMenu, pType, lstObject, pCondApp, pParam, pDynamicArgs, out opReferentialsReferential);
        }


        /// <summary>
        /// Renseigne une classe referential à partir d'un fichier XML de type "Repository"
        /// </summary>
        /// <param name="pCS">chaine pour la connexion SGBD</param>
        /// <param name="pIdMenu">Id du menu appelant</param>
        /// <param name="pType">type de consultation</param>
        /// <param name="pObjectName">Liste des object xml utilisés pour la Deserialization, priorité est donné au 1er item pour lequel il existe un fichier xml </param>
        /// <param name="pCondApp">condition d'application si nécessaire</param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="opReferentialsReferential">OUT classe referential à renseigner</param>
        // EG 20180423 Analyse du code Correction [CA2202]
        public static void DeserializeXML_ForModeRW(string pCS, string pIdMenu, Cst.ListType pType, List<string> pObjectName,
             string pCondApp, string[] pParam, Dictionary<string, StringDynamicData> pDynamicArgs, out Referential opReferentialsReferential)
        {
            string xmlFile = string.Empty;
            string objectName = pObjectName[0];

            //Identification du fichier XML contenant le descriptif du référentiel 
            for (int i = 0; i < ArrFunc.Count(pObjectName); i++)
            {
                string folder = pType.ToString();
                string filename = pObjectName[i];
                if (filename.IndexOf(":") > 0)
                {
                    // Gestion du cas particulier d'un fichier XML utilisé sur plusieurs consultation, donc où le Folder n'est pas équivalent au "Type".
                    // ex. ListViewer.aspx?Trade=Accounting:ACCDAYBOOK
                    folder = filename.Substring(0, filename.IndexOf(":"));
                    filename = filename.Substring(filename.IndexOf(":") + 1);
                }


                if (SessionTools.NewAppInstance().SearchFile2(pCS, StrFunc.AppendFormat(@"~\PDIML\{0}\{1}.XML", folder, filename), ref xmlFile))
                {
                    objectName = filename;
                    break;
                }
            }

            bool isTablePreviousImage = objectName.EndsWith("_P");
            if (isTablePreviousImage)
                objectName = GetTableNameForReferential(objectName);

            StreamReader streamReader = null;
            StreamWriter streamWriter = null;
            bool isFound = false;
            if (File.Exists(xmlFile))
            {
                streamReader = new StreamReader(xmlFile);
                isFound = true;
            }

            if ((!isFound) && DataHelper.isDbSqlServer(pCS))
            {
                #region MS SQLServer: Call Stored Procedure for auto generate the xml file
                IDataReader drSQLTOXML = null;
                try
                {
                    xmlFile = HttpContext.Current.Server.MapPath(@"Temporary/" + objectName + ".xml");

                    DataParameters parameters = new DataParameters();
                    parameters.Add(new DataParameter(pCS, "OBJECTNAME", DbType.AnsiString, 255), objectName);
                    DataHelper.ExecuteNonQuery(pCS, CommandType.StoredProcedure, "dbo.SQLTOXML", parameters.GetArrayDbParameter());
                    drSQLTOXML = DataHelper.ExecuteReader(pCS, CommandType.Text, "select SQLTOXML from dbo.TMP_SQLTOXML order by IDSQLTOXML");
                    FileStream fileStream = new FileStream(xmlFile, FileMode.OpenOrCreate);
                    streamWriter = new StreamWriter(fileStream);
                    while (drSQLTOXML.Read())
                    {
                        isFound = true;
                        streamWriter.WriteLine(drSQLTOXML.GetValue(0).ToString());
                    }
                    //streamWriter.Close();
                    //fileStream.Close();
                    //streamReader = new StreamReader(xmlFile);
                }
                finally
                {
                    if (null != streamWriter)
                        streamWriter.Close();

                    if (null != drSQLTOXML)
                    {
                        drSQLTOXML.Close();
                        drSQLTOXML.Dispose();
                    }
                }
                #endregion
            }
            if (!isFound)
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                                        String.Format("XML file not found: {0} [{1}]", xmlFile, pObjectName[0]),
                                        new Exception());
            }

            try
            {
                streamReader = new StreamReader(xmlFile);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Referentials));
                Referentials referentials = (Referentials)xmlSerializer.Deserialize(streamReader);
                if (isTablePreviousImage)
                {
                    // S'il s'agit d'un référentiel "_P" (previous image), alors on utilise le fichier XML de la table principale 
                    // auquel on rajoutera dynamiquement les N colonnes d'audit (ACTION_, DTSYS_, xxx_, ...)
                    referentials[objectName].TableName = referentials[objectName].TableName + "_P";
                }

                // FI 20141211 [20563] paramètre pDynamicArgs
                referentials[objectName].Initialize(false, pType, pCondApp, pParam, pDynamicArgs);
                opReferentialsReferential = referentials[objectName];
            }
            finally
            {
                if (null != streamReader)
                    streamReader.Close();
            }

            string sqlSelect = string.Empty;

            #region Add Additionnal Column (EXTLID, ROLE, ITEM)
            List<ReferentialColumn> lstRc = new List<ReferentialColumn>();

            bool isAdditionalDataCreating = (false == opReferentialsReferential.HideExtlId) ||
                                            (opReferentialsReferential.RoleTableNameSpecified) ||
                                            (opReferentialsReferential.ItemsSpecified);
            if (isAdditionalDataCreating)
            {
                lstRc.AddRange(opReferentialsReferential.Column.ToList());

                // Add Additionnal Column from DEFINEEXTLID
                if (false == opReferentialsReferential.HideExtlId)
                    CreateExternalDataColumn(pCS, lstRc, opReferentialsReferential);
                // Add Additionnal Column from ROLE
                if (opReferentialsReferential.RoleTableNameSpecified)
                    CreateRoleDataColumn(pCS, lstRc, opReferentialsReferential);
                // Add Additionnal Column from ITEM
                if (opReferentialsReferential.ItemsSpecified)
                    CreateItemDataColumn(pCS, lstRc, opReferentialsReferential);
            }

            // Recréation des colonnes à partir de l'ArrayList créé et enrichi précédemment.
            if (0 < lstRc.Count)
            {
                opReferentialsReferential.HasMultiTable = true;
                opReferentialsReferential.Column = lstRc.ToArray();
            }
            #endregion Add Additionnal Column (EXTLID, ROLE, ITEM)
        }
        #endregion

        #region CreateExternalDataColumn
        /// <summary>
        /// Création des colonnes de données externes
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstRc">Liste des colonnes sources</param>
        /// <param name="pReferential">Referentiel source</param>
        /// EG 20170401 New
        private static void CreateExternalDataColumn(string pCS, List<ReferentialColumn> pLstRc, Referential pReferential)
        {
            string sqlSelect = @"select TABLENAME, IDENTIFIER, DISPLAYNAME, ISMANDATORY, REGULAREXPRESSION, LISTRETRIEVAL, DATATYPE, DEFAULTVALUE, ISHIDEINDATAGRID,
                                    HELPTYPE, HELPDISPLAY, HELPMESSAGE
                                    from dbo.DEFINEEXTLID
                                    where TABLENAME = " + DataHelper.SQLString("{0}") + " order by SEQUENCENO";

            sqlSelect = String.Format(sqlSelect, (pReferential.TableName == Cst.OTCml_TBL.VW_BOOK_VIEWER.ToString()) ?
                Cst.OTCml_TBL.BOOK.ToString() : pReferential.TableName);

            IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, sqlSelect);

            int i = 0;
            while (dr.Read())
            {
                #region While
                ReferentialColumn rrc = new ReferentialColumn();
                if (0 == i)
                {
                    rrc.html_BLOCK = new ReferentialColumnhtml_BLOCK[1] { new ReferentialColumnhtml_BLOCK() };
                    rrc.html_BLOCK[0].columnbyrowSpecified = true;
                    rrc.html_BLOCK[0].columnbyrow = 1;
                    rrc.html_BLOCK[0].title = Ressource.GetString("ExternalInfos", true);
                    rrc.html_BLOCK[0].InformationSpecified = false;
                }

                rrc.IsExternal = true;
                rrc.ExternalFieldID = i;
                rrc.ExternalTableName = pReferential.TableName;
                rrc.ExternalIdentifier = dr["IDENTIFIER"].ToString(); ;
                rrc.ColumnName = "VALUE";
                rrc.AliasTableNameSpecified = true;
                rrc.AliasTableName = "e_" + dr["TABLENAME"].ToString().Substring(0, 1).ToLower() + dr["IDENTIFIER"].ToString(); ;
                rrc.DataField = rrc.ColumnName;
                rrc.Ressource = dr["DISPLAYNAME"].ToString();
                rrc.RessourceSpecified = true;
                rrc.RegularExpression = (dr["REGULAREXPRESSION"].ToString().Length > 0 ? dr["REGULAREXPRESSION"].ToString() : rrc.RegularExpression);
                rrc.IsMandatorySpecified = true;
                rrc.IsMandatory = Convert.ToBoolean(dr["ISMANDATORY"]);
                rrc.DataType = new ReferentialColumnDataType();
                rrc.DataType.value = dr["DATATYPE"].ToString();
                rrc.Length = 64;
                rrc.AlignSpecified = true;
                rrc.Align = "left";
                rrc.Scale = (TypeData.IsTypeDec(rrc.DataType.value) ? 10 : 0);
                rrc.Default = new ReferentialColumnDefault[1] { new ReferentialColumnDefault() };
                rrc.Default[0].Value = dr["DEFAULTVALUE"].ToString();
                rrc.IsHideSpecified = true;
                rrc.IsHide = false;
                rrc.IsHideInDataGridSpecified = true;
                rrc.IsHideInDataGrid = Convert.ToBoolean(dr["ISHIDEINDATAGRID"]);
                rrc.IsKeyFieldSpecified = true;
                rrc.IsKeyField = false;
                rrc.IsDataKeyFieldSpecified = true;
                rrc.IsDataKeyField = false;
                rrc.IsIdentitySpecified = true;
                rrc.IsIdentity.Value = false;

                rrc.IsUpdatable = new ReferentialColumnIsUpdatable();
                rrc.IsUpdatableSpecified = true;
                rrc.IsUpdatable.Value = true;

                rrc.Relation = new ReferentialColumnRelation[1];
                rrc.Relation[0] = new ReferentialColumnRelation();

                DispatchListRetrieval(ref rrc, dr["LISTRETRIEVAL"].ToString());

                #region rrc.Information
                string helpMessage = (dr["HELPMESSAGE"] is DBNull ? string.Empty : dr["HELPMESSAGE"].ToString());
                if (StrFunc.IsFilled(helpMessage))
                {
                    string helpType = (dr["HELPTYPE"] is DBNull ? string.Empty : dr["HELPTYPE"].ToString());
                    string helpDisplay = (dr["HELPDISPLAY"] is DBNull ? string.Empty : dr["HELPDISPLAY"].ToString());
                    rrc.InformationSpecified |= GetInformationCustomization(helpType, helpDisplay, helpMessage, out rrc.Information);
                }
                #endregion

                pLstRc.Add(rrc);
                #endregion While
                i++;
            }

            if (null != dr)
            {
                dr.Close();
                dr.Dispose();
            }
        }
        #endregion CreateExternalDataColumn
        #region CreateRoleDataColumn
        /// <summary>
        /// Création des colonnes de données ROLE
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstRc">Liste des colonnes sources</param>
        /// <param name="pReferential">Referentiel source</param>
        /// EG 20170401 New
        private static void CreateRoleDataColumn(string pCS, List<ReferentialColumn> pLstRc, Referential pReferential)
        {
            SpheresMenu.Menu menuRole = SessionTools.Menus.SelectByURL("=" + pReferential.RoleTableName.Value + "&");
            bool isUserRightEnabledOnMenu = ((null != menuRole) && menuRole.IsEnabled);

            // Le menu d'accès direct aux items est accessible par l'utilisateur connecté 
            // --> Affichage des items pour consultation/modification directement depuis le référentiel en cours. 
            if (isUserRightEnabledOnMenu)
            {
                //Modification autorisée uniquement lorsque toutes les permissions sur le menu correspondant aux rôles sont autorisées (Create, Modify et Remove)
                bool isUpdatable = RepositoryTools.IsAllPermissionEnabled(SessionTools.CS, menuRole.IdMenu, SessionTools.User);

                string tbl = pReferential.RoleTableName.Value;               //ex.:  "ACTORROLE"
                string tblRole = "ROLE" + tbl.Substring(0, tbl.Length - 4);  //ex.:  "ROLEACTOR"
                string idcol = "ID" + tblRole;                               //ex.:  "IDROLEACTOR"
                string concat = DataHelper.GetConcatOperator(SessionTools.CS);

                //  Select des ROLES triés par ROLETYPE
                string sqlSelect = @"select {0}, {1} as NAME, ROLETYPE from dbo.{2} order by ROLETYPE, {0}";
                sqlSelect = String.Format(sqlSelect, idcol,
                    DataHelper.SQLConcat(SessionTools.CS, idcol,
                    " case when (" + idcol + "!=DESCRIPTION) then " + DataHelper.SQLString(" - "),
                    " DESCRIPTION else ' ' end"), tblRole);

                IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, sqlSelect);
                int iStart = pLstRc.Count(column => column.IsExternal);
                int i = iStart;
                string lastRoleType = string.Empty;
                while (dr.Read())
                {
                    #region Column IDROLExxx
                    ReferentialColumn rrc = new ReferentialColumn();
                    string identifier = dr[idcol].ToString().Trim();
                    string roleType = dr["ROLETYPE"].ToString().Trim();
                    string defaultValue = string.Empty;

                    if (i == iStart)
                    {
                        int columnByRow = (pReferential.RoleTableName.columnbyrowSpecified ? pReferential.RoleTableName.columnbyrow : 2);
                        rrc.html_BLOCK = new ReferentialColumnhtml_BLOCK[1] { new ReferentialColumnhtml_BLOCK() };
                        rrc.html_BLOCK[0].SetBlock(columnByRow, "Role", "Msg_ROLExForxROLE");
                    }
                    if (lastRoleType != roleType)
                    {
                        lastRoleType = roleType;

                        rrc.html_HR = new ReferentialColumnhtml_HR[1] { new ReferentialColumnhtml_HR() };
                        rrc.html_HR[0].columnbyrowSpecified = false;
                        rrc.html_HR[0].size = "1";
                        rrc.html_HR[0].title = Ressource.GetString(roleType, true);
                    }
                    rrc.SetColumnIDRole(i, tbl, identifier, idcol, dr["ROLETYPE"].ToString(), dr["NAME"].ToString(), defaultValue, isUpdatable);

                    if (tblRole == Cst.OTCml_TBL.ROLEACTOR.ToString())
                    {
                        if (
                            ((identifier == RoleActor.SYSADMIN.ToString()) && !SessionTools.IsSessionSysAdmin)
                            ||
                            ((identifier == RoleActor.SYSOPER.ToString()) && !SessionTools.IsSessionSysAdmin && !SessionTools.IsSessionSysOper)
                        )
                        {
                            rrc.IsUpdatable.Value = false;
                            rrc.IsUpdatable.isupdatableincreation = false;
                            rrc.IsUpdatable.isupdatableincreationSpecified = true;
                        }
                    }
                    #endregion Column IDROLExxx

                    #region Column IDAxxx
                    bool isGinstrRole = (pReferential.RoleTableName.Value == "GINSTRROLE");
                    bool isActorRole = (pReferential.RoleTableName.Value == "ACTORROLE");
                    bool isGContractRole = (pReferential.RoleTableName.Value == "GCONTRACTROLE");
                    rrc.IsAutoPostBackSpecified = true;
                    rrc.IsAutoPostBack = isActorRole;
                    pLstRc.Add(rrc);

                    if (pReferential.RoleTableName.IsRoleWithIDA)
                    {
                        rrc = new ReferentialColumn();
                        rrc.SetColumnIDARole(pReferential.RoleTableName.IsActorRole, i, tbl, identifier, idcol);
                        rrc.IsUpdatable.Value = isUpdatable &&
                            ((false == pReferential.RoleTableName.IsGInstrRole) && (pReferential.RoleTableName.IsGContractRole)) || (identifier == "INVOICING");
                        rrc.IsUpdatable.isupdatableincreationSpecified = true;
                        rrc.IsUpdatable.isupdatableincreation = rrc.IsUpdatable.Value;
                        pLstRc.Add(rrc);
                    }

                    i++;
                    #endregion Column IDAxxx
                }
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }
        #endregion CreateRoleDataColumn
        #region CreateItemDataColumn
        /// <summary>
        /// Création des colonnes de données ITEMS 
        /// GACTOR,ACTORG|GBOOK,BOOKG|GCONTRACT,CONTRACTG|INSTRUMENT,INTRG|MARKET,MARKETG|GINSTR,INSTRG|GMARKET,MARKETG|TAX,ACTORTAX)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstRc">Liste des colonnes sources</param>
        /// <param name="pReferential">Referentiel source</param>
        /// EG 20170401 New
        private static void CreateItemDataColumn(string pCS, List<ReferentialColumn> pLstRc, Referential pReferential)
        {
            EFS.Common.Web.Menu.Menu menuItem = SessionTools.Menus.SelectByURL("=" + pReferential.Items.tablename + "&");//rctablename="GINSTR" tablename="INSTRG" 

            bool isUserRightEnabledOnMenu = ((menuItem != null) && (menuItem.IsEnabled));
            //Menu d'accès direct aux items, autorisé pour l'utilisateur connecté --> Affichage des items pour consultation/modification directement depuis le référentiel en cours. 
            if (isUserRightEnabledOnMenu)
            {
                // Modification autorisée uniquement 
                // - lorsqu'aucun mode n'est précisé (tous les modes sont des modes Read Only)
                // - lorsque toutes les permissions sont autorisées sur le menu correspondant aux items (Create, Modify et Remove)
                bool isUpdatable = (pReferential.Items.mode == "RW") && RepositoryTools.IsAllPermissionEnabled(SessionTools.CS, menuItem.IdMenu, SessionTools.User);

                #region Select des ITEMS triés (spécifiquement pour certains référentiels)
                //ex.: "IDI" pour le référentiel "GINSTR" 
                string sqlSelect = @"select tblmain." + pReferential.Items.columnname + @" as ID, tblmain.IDENTIFIER || 
                case when (tblmain.IDENTIFIER <> tblmain.DISPLAYNAME) then ' - ' || tblmain.DISPLAYNAME else ' ' end as NAME, ";

                string itemDataType = TypeData.TypeDataEnum.@bool.ToString();
                if (pReferential.Items.datatypeSpecified)
                    itemDataType = pReferential.Items.datatype;

                switch (pReferential.Items.srctablename)//ex.: "INSTRUMENT" pour le référentiel "GINSTR"
                {
                    case "INSTRUMENT":
                        #region
                        sqlSelect += @"p.GPRODUCT || '|' || p.FAMILY || '|' || case when p.CLASS='STRATEGY' and p.CLASS!=p.FAMILY then p.CLASS else ' ' end as Title
                        from dbo.INSTRUMENT tblmain
                        inner join dbo.PRODUCT p on (p.IDP = tblmain.IDP)
                        order by p.GPRODUCT,p.FAMILY,p.CLASS,tblmain.IDENTIFIER";
                        break;
                        #endregion
                    case "MENU":
                        #region
                        sqlSelect = @"select tblmain." + pReferential.Items.columnname + @" as ID, tblmain.IDMENU as NAME, tblmain.IDMENU as Title
                                from dbo.MENU tblmain" + Cst.CrLf;
                        if (DataHelper.isDbOracle(pCS))
                            sqlSelect += SQLCst.WHERE + DataHelper.SQLLength(pCS, "tblmain.IDMENU") + "< 28" + Cst.CrLf;
                        sqlSelect += SQLCst.ORDERBY + "tblmain.IDMENU";
                        itemDataType = "string";
                        break;
                        #endregion
                    case "MARKET":
                        #region
                        sqlSelect += DataHelper.SQLIsNull(pCS, "c.DESCRIPTION", DataHelper.SQLIsNullChar(pCS, "tblmain.IDCOUNTRY", " "))
                                    + " || '|' || " + DataHelper.SQLIsNull(pCS, "bc.DESCRIPTION", DataHelper.SQLIsNullChar(pCS, "bc.DISPLAYNAME", " ")) + " as Title" + Cst.CrLf;
                        sqlSelect += @"from dbo.MARKET tblmain
                                left outer join dbo.BUSINESSCENTER bc on (bc.IDBC = tblmain.IDBC)
                                left outer join dbo.COUNTRY c on (c.IDCOUNTRY = tblmain.IDCOUNTRY)
                                order by tblmain.IDCOUNTRY, bc.DESCRIPTION, tblmain.IDBC, tblmain.IDENTIFIER";
                        break;
                        #endregion
                    case "DERIVATIVECONTRACT":
                        #region
                        sqlSelect += DataHelper.SQLIsNull(pCS, "c.DESCRIPTION", DataHelper.SQLIsNullChar(pCS, "m.IDCOUNTRY", " "))
                                    + " || '|' || "
                                    + DataHelper.SQLIsNull(pCS, "bc.DESCRIPTION", DataHelper.SQLIsNullChar(pCS, "bc.DISPLAYNAME", " "))
                                    + " || '|' || " + DataHelper.SQLIsNullChar(pCS, "m.DISPLAYNAME", " ") + " as Title" + Cst.CrLf;
                        sqlSelect += @"from dbo.DERIVATIVECONTRACT tblmain
                                left outer join dbo.MARKET m on (m.IDM = tblmain.IDM)
                                left outer join dbo.BUSINESSCENTER bc on (bc.IDBC = tblmain.IDBC)
                                left outer join dbo.COUNTRY c on (c.IDCOUNTRY = tblmain.IDCOUNTRY)
                                order by m.IDCOUNTRY, bc.DESCRIPTION, m.IDBC, m.IDENTIFIER, tblmain.IDENTIFIER";
                        break;
                        #endregion
                    default:
                        #region
                        sqlSelect += @"null as Title" + Cst.CrLf;
                        sqlSelect += SQLCst.FROM_DBO + pReferential.Items.srctablename + " tblmain" + Cst.CrLf;
                        sqlSelect += SQLCst.ORDERBY + "IDENTIFIER";
                        break;
                        #endregion
                }
                #endregion
                IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, sqlSelect);

                int iStart = pLstRc.Count(column => column.IsExternal);
                int nbRole = pLstRc.Count(column => column.IsRole);
                if (0 < nbRole)
                    iStart += (nbRole / (pReferential.RoleTableName.IsRoleWithIDA ? 2 : 1));
                int i = iStart;

                string lastTitle = null;
                // Création d'une colonne (à l'image des colonnes ExtlId) pour chaque valeur disponible (ex. pour chaque "INSTRUMENT" pour le référentiel "GINSTR").
                while (dr.Read())
                {
                    #region Column IDxxx
                    ReferentialColumn rrc = new ReferentialColumn();
                    string identifier = dr["ID"].ToString().Trim();
                    string displayName = dr["NAME"].ToString().Trim();
                    string title = dr["Title"].ToString().Trim();
                    //string dataType = TypeData.TypeDataEnum.@bool.ToString();
                    // EG 20170327 Add dataType
                    //if (pReferential.Items.datatypeSpecified)
                    //    dataType = pReferential.Items.datatype;
                    if (i == iStart)
                    {
                        #region html_BLOCK - Création d'un nouveau BLOC sur la première colonne
                        rrc.html_BLOCK = new ReferentialColumnhtml_BLOCK[1];
                        if (pReferential.Items.html_BLOCKSpecified)
                        {
                            rrc.html_BLOCK[0] = pReferential.Items.html_BLOCK;
                            if (String.IsNullOrEmpty(rrc.html_BLOCK[0].title))
                            {
                                rrc.html_BLOCK[0].title = Ressource.GetString(menuItem.IdMenu, true);
                            }
                            else
                            {
                                rrc.html_BLOCK[0].title = Ressource.GetString(rrc.html_BLOCK[0].title, true);
                            }
                        }
                        else
                        {
                            rrc.html_BLOCK[0] = new ReferentialColumnhtml_BLOCK();
                            rrc.html_BLOCK[0].columnbyrowSpecified = false;
                            rrc.html_BLOCK[0].title = Ressource.GetString(menuItem.IdMenu, true);
                            rrc.html_BLOCK[0].InformationSpecified = true;
                            rrc.html_BLOCK[0].Information = new ReferentialInformation();
                            rrc.html_BLOCK[0].Information.TypeSpecified = true;
                            rrc.html_BLOCK[0].Information.Type = Cst.TypeInformationMessage.Information;
                            rrc.html_BLOCK[0].Information.LabelMessageSpecified = true;
                            rrc.html_BLOCK[0].Information.LabelMessage = "Msg_ITEMxForxITEM";
                        }
                        #endregion
                    }

                    if (lastTitle != title)
                    {
                        #region html_HR - Création éventuelle d'un nouveau HR sur la base d'une rupure sur "Title"
                        // Mettre un exemple afin de faciliter la Compréhension
                        if (lastTitle == null)
                        {
                            lastTitle = @"#" + title.Replace(@"|", @"|#"); //Tip
                        }
                        string[] aLastTitle = lastTitle.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] aTitle = title.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        rrc.html_HR = new ReferentialColumnhtml_HR[aTitle.Length];
                        for (int nbHR = 0; nbHR < aTitle.Length; nbHR++)
                        {
                            if ((nbHR == aLastTitle.Length) || (aTitle[nbHR] != aLastTitle[nbHR]))
                            {
                                if ((nbHR == 0) || (StrFunc.IsFilled(aTitle[nbHR])))
                                {
                                    rrc.html_HR[nbHR] = new ReferentialColumnhtml_HR();
                                    rrc.html_HR[nbHR].columnbyrowSpecified = false;
                                    rrc.html_HR[nbHR].size = (aTitle.Length - nbHR).ToString();
                                    rrc.html_HR[nbHR].title = aTitle[nbHR].Replace(@",", @"," + Cst.HTMLBreakLine + Cst.HTMLSpace2);

                                    if (pReferential.Items.srctablename == "INSTRUMENT")
                                    {
                                        if (0 == nbHR)
                                        {
                                            // Groupe de produit
                                            ProductTools.GroupProductEnum groupProduct = (ProductTools.GroupProductEnum)
                                                ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), aTitle[nbHR]);
                                            rrc.html_HR[nbHR].title += " - " + groupProduct.ToString();

                                        }
                                        else if (1 == nbHR)
                                        {
                                            // Famille
                                            ProductTools.FamilyEnum family = (ProductTools.FamilyEnum)
                                                ReflectionTools.EnumParse(new ProductTools.FamilyEnum(), aTitle[nbHR]);
                                            rrc.html_HR[nbHR].title += " - " + family.ToString();

                                        }

                                    }
                                }
                            }
                        }
                        lastTitle = title;
                        #endregion
                    }
                    rrc.IsItem = true;
                    rrc.ExternalFieldID = i;
                    rrc.ExternalTableName = pReferential.Items.srctablename;
                    rrc.ExternalIdentifier = identifier;
                    rrc.ColumnName = pReferential.Items.columnname;
                    rrc.AliasTableNameSpecified = true;
                    // Important *********************************************************************************************************************
                    // rrc.AliasTableName: Constitué de 3 données (see also GetSQL())
                    // ex.: ii99 avec "i" en dur pour "Item", premier caractère lower() de la table (ex. "i" pour INSTRUMENT), valeur de la donnée (ex. 99 pour un IDI)
                    rrc.AliasTableName = "i" + pReferential.Items.srctablename.Substring(0, 1).ToLower() + identifier.Replace("-", "_");
                    //*******************************************************************************************************************************
                    rrc.DataField = rrc.ColumnName;
                    rrc.Ressource = displayName;
                    rrc.RessourceSpecified = true;
                    rrc.ColspanSpecified = false;
                    rrc.Colspan = 1;//NB: Bien que ColspanSpecified soit false, il est nécessaire de valoriser Colspan
                    rrc.IsMandatorySpecified = true;
                    rrc.IsMandatory = true;
                    rrc.DataType = new ReferentialColumnDataType();
                    rrc.DataType.value = itemDataType;
                    rrc.Length = 1;
                    rrc.IsHideInDataGridSpecified = true;
                    rrc.IsHideInDataGrid = true;

                    rrc.IsUpdatable = new ReferentialColumnIsUpdatable();
                    rrc.IsUpdatableSpecified = true;
                    rrc.IsUpdatable.Value = isUpdatable;

                    rrc.Relation = new ReferentialColumnRelation[1];
                    rrc.Relation[0] = new ReferentialColumnRelation();

                    //Ajout de la colonne dans la liste.
                    pLstRc.Add(rrc);

                    i++;
                    #endregion Column IDxxx

                }
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }
        #endregion CreateItemDataColumn

        #region DeserializeXML_ForModeRO
        /// <summary> 
        /// Renseigne une classe referential à partir d'un fichier XML
        /// </summary>
        public static void DeserializeXML_ForModeRO(string pFileName, out Referential opReferential)
        {
            DeserializeXML_ForModeRO(pFileName, null, null, null, out opReferential);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="opReferentialsReferential"></param>
        /// FI 20141211 [20563] Modification de signature add pDynamicArgs
        public static void DeserializeXML_ForModeRO(string pFileName, string pCondApp, string[] pParam, Dictionary<string, StringDynamicData> pDynamicArgs, out Referential opReferential)
        {
            StreamReader streamReader = null;
            try
            {
                string XML_File = pFileName;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Referentials));
                streamReader = new StreamReader(XML_File);
                Referentials referentials = (Referentials)xmlSerializer.Deserialize(streamReader);
                referentials.Items[0].Initialize(false, null, pCondApp, pParam, pDynamicArgs);
                opReferential = referentials.Items[0];
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
            }
        }
        #endregion

        /// <summary>
        /// Initialise la classe Referential pour un usage de type Grid
        /// </summary>
        /// <param name="pPage">page appelante</param>
        /// <param name="pReferential">REF classe referential</param>
        /// <param name="pValueForeignKeyField">valeur pour la foreignKey</param>
        public static void InitializeReferentialForGrid(PageBase pPage, ref Referential pReferential, string pValueForeignKeyField)
        {
            pReferential.valueForeignKeyField = pValueForeignKeyField;
            pReferential.IsGrid = true;
            pReferential.IsForm = false;
            GetExtlLinkCustomization(ref pReferential);
            InitializeID(ref pReferential);
        }


        /// <summary>
        /// Initialise la classe Referential pour un usage de type FORM 
        /// </summary>
        /// <param name="pPage">page appelante</param>
        /// <param name="pReferential">REF classe referential</param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void InitializeReferentialForForm(RepositoryPageBase pRepositoryPage)
        {
            Referential referential = pRepositoryPage.referential;
            referential.IsGrid = false;
            referential.IsForm = true;
            referential.isNewRecord = pRepositoryPage.isNewRecord && (false == pRepositoryPage.isDuplicateRecord);
            referential.consultationMode = pRepositoryPage.consultationMode;

            pRepositoryPage.referential.isLookReadOnly = ((Cst.ConsultationMode.Normal != pRepositoryPage.consultationMode) &&
                                           (Cst.ConsultationMode.PartialReadOnly != pRepositoryPage.consultationMode)) ||
                                           ((false == referential.Create) & pRepositoryPage.isNewRecord) ||
                                           ((false == referential.Modify) & (false == pRepositoryPage.isNewRecord));

            referential.valueForeignKeyField = pRepositoryPage.valueForeignKeyField;

            if (false == pRepositoryPage.IsPostBack)
            {
                DataSet ds = GetDsDataForm(pRepositoryPage);
                pRepositoryPage.dataReferentiel = ds;
            }
            // Entre 2 post de page le dataset est sauvegardé dans une variable session
            referential.dataSet = pRepositoryPage.dataReferentiel;
            pRepositoryPage.SetRepositoryTitle();

            bool isOk = true;
            DataRow rowSource = null;
            if ((false == pRepositoryPage.isNewRecord) || pRepositoryPage.isDuplicateRecord)
            {
                // Obtention du DataRow correspondant depuis le DataSet
                rowSource = pRepositoryPage.referential.GetRow(0, null, pRepositoryPage.valueDataKeyField);

                #region DataRow non trouvé
                if (rowSource == null)
                {
                    isOk = false;

                    // La page "RepositoryPageBase" nécessite l'existence d'un "pReferential.dataRow"
                    referential.dataRow = referential.dataSet.Tables[0].NewRow();

                    // Message utilisateur de type SelfClose, afin de fermer le formulaire.
                    JavaScript.DialogStartUpImmediate(pRepositoryPage, Ressource.GetString("Msg_DataRowNotFound"), true, ProcessStateTools.StatusWarningEnum);
                }
                #endregion
            }

            if (isOk)
            {
                if (pRepositoryPage.isNewRecord || pRepositoryPage.isDuplicateRecord)
                {
                    #region
                    // New ou Duplicate Record: Création d'un nouveau DataRow puis initialisation de celui-ci.
                    referential.dataRow = referential.dataSet.Tables[0].NewRow();

                    if (referential.HasMultiTable)
                    {
                        referential.drExternal = new DataRow[referential.dataSet.Tables.Count - 1];
                        referential.isNewDrExternal = new bool[referential.dataSet.Tables.Count - 1];
                        for (int i = 1; i < referential.dataSet.Tables.Count; i++)
                        {
                            referential.drExternal[i - 1] = referential.dataSet.Tables[i].NewRow();
                            // En mode "pIsDuplicateRecord, initialisation de "isNewDrExternal" plus bas après CreateControls(), afin de ne pas considérer 
                            // ces lignes comme des lignes nouvelles et ainsi permettre la duplication de leur données respectives.
                            if (false == pRepositoryPage.isDuplicateRecord)
                            {
                                referential.isNewDrExternal[i - 1] = true;
                            }
                        }
                    }

                    if (pRepositoryPage.isDuplicateRecord)
                    {
                        // NAV:New Actor Value
                        string actorIdentifier = (pRepositoryPage.Request.QueryString["NAV"]);
                        referential.DuplicateRow(ref referential.dataRow, rowSource, actorIdentifier);

                        // Duplication des données externes...
                        for (int i = 1; i < referential.dataSet.Tables.Count; i++)
                        {
                            DataRow extlRowSource = referential.GetRow(i, "ID", pRepositoryPage.valueDataKeyField);//La colonne "clé" est ici en toujours "ID"
                            if (extlRowSource != null)
                            {
                                // Une ligne existe, uniquement lorsque la donnée est "cochée".
                                for (int index = 0; index < extlRowSource.ItemArray.Length; index++)
                                {
                                    try
                                    {
                                        if (extlRowSource.Table.Columns[index].ColumnName == "ID")
                                        {
                                            // Colonne clé, on réinitialise sa valeur afin de pas impacter le référentiel servant de base à la duplication
                                            referential.drExternal[i - 1][index] = Convert.DBNull;
                                        }
                                        else
                                        {
                                            referential.drExternal[i - 1][index] = extlRowSource[index];
                                        }
                                    }
                                    catch
                                    {
                                        referential.drExternal[i - 1][index] = Convert.DBNull;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        referential.InitializeNewRow(ref referential.dataRow);
                    }
                    #endregion
                }
                else
                {
                    #region
                    referential.dataRow = rowSource;
                    if (referential.consultationMode == Cst.ConsultationMode.PartialReadOnly)
                    {
                        referential.InitializeUpdateRow(referential.dataRow);
                    }
                    try
                    {
                        // Affichage de la PK (si not IDENTITY)
                        if (referential.ExistsColumnIDENTITY && (false == referential.Column[referential.IndexIDENTITY].IsIdentity.sourceSpecified))
                            pRepositoryPage.SetRepositoryTitle();

                        if (referential.ExistsColumnROWATTRIBUT && (referential.dataRow[referential.IndexColSQL_ROWATTRIBUT].ToString() == Cst.RowAttribut_Deleted))
                        {
                            string test = string.Empty;
                            //pRepositoryPage.AddFooterLeft(Ressource.GetString("Msg_ErasedItem"));
                        }

                        //20071102 PL Add ExistsColumnsDateValidity et test sur DTENABLED
                        if (referential.ExistsColumnsDateValidity)
                        {
                            if ((Convert.ToDateTime(referential.dataRow["DTENABLED"]).CompareTo(DateTime.Today) > 0)
                                ||
                                (!(referential.dataRow["DTDISABLED"] is DBNull) && (Convert.ToDateTime(referential.dataRow["DTDISABLED"]).CompareTo(DateTime.Today) <= 0)))
                            {
                                string test = string.Empty;
                                //pRepositoryPage.AddFooterLeft(Ressource.GetString("Msg_DisabledItem"));
                            }
                        }

                    }
                    catch (Exception)
                    {
                        // Ajout des msg en debug afin de déteter des pbs
#if DEBUG
                        throw;
#endif
                    }

                    if (referential.HasMultiTable)
                    {
                        string filter = "ID=";
                        if (referential.IsDataKeyField_String)
                            filter += DataHelper.SQLString(pRepositoryPage.valueDataKeyField);
                        else
                            filter += pRepositoryPage.valueDataKeyField;

                        referential.drExternal = new DataRow[referential.dataSet.Tables.Count - 1];
                        referential.isNewDrExternal = new bool[referential.dataSet.Tables.Count - 1];
                        for (int i = 1; i < referential.dataSet.Tables.Count; i++)
                        {
                            // Add: CaseSensitive = true pour Oracle et la gestion du ROWBERSION (ROWID) 
                            referential.dataSet.Tables[i].CaseSensitive = true;
                            DataRow[] modifyExternalRows = referential.dataSet.Tables[i].Select(filter);
                            if (modifyExternalRows.GetLength(0) > 0)
                            {
                                referential.drExternal[i - 1] = modifyExternalRows[0];
                                referential.isNewDrExternal[i - 1] = false;
                            }
                            else
                            {
                                referential.drExternal[i - 1] = referential.dataSet.Tables[i].NewRow();
                                referential.isNewDrExternal[i - 1] = true;
                            }
                        }
                    }
                    #endregion
                }

                // Substituer la valeur de la colonne DTHOLIDAYVALUE, par la prochaine date du jour férié correspondant                
                if ((false == pRepositoryPage.IsPostBack) && referential.ExistsColumnDTHOLIDAYVALUE)
                {
                    RepositoryTools.ValuateDTHOLIDAYVALUE(referential.dataRow, null);
                }

                GetExtlLinkCustomization(ref referential);

                // En mode "pIsDuplicateRecord, initialisation ici de "isNewDrExternal" afin de considérer ces lignes comme des lignes nouvelles.
                if (referential.HasMultiTable && pRepositoryPage.isDuplicateRecord)
                {
                    for (int i = 1; i < referential.dataSet.Tables.Count; i++)
                    {
                        referential.isNewDrExternal[i - 1] = true;
                    }
                }

                // Warning: on repositionne ici isNewRecord sans tenir compe de pIsDuplicateRecord
                referential.isNewRecord = pRepositoryPage.isNewRecord;

                // Affichage Date Insert et Update
                pRepositoryPage.SetActionDates();

            }
        }

        /// <summary>
        /// Retourne le nom de la table qui pilote l'automate en fonction du nom de la table
        /// </summary>
        /// <param name="pTableName">nom de la table</param>
        /// <returns>nom de la table pour le referentiel</returns>
        public static string GetTableNameForReferential(string pTableName)
        {
            string retTableName = pTableName;

            if (pTableName.ToUpper().EndsWith("_P"))
            {
                // Table Previous Image for repository
                retTableName = retTableName.Substring(0, retTableName.Length - 2);
            }
            return retTableName;
        }

        #region public GetNewAlias
        /// <summary>
        /// Renvoie un alias numéroté si plusieurs alias existants
        /// </summary>
        /// <param name="pTableName">alias</param>
        /// <param name="pTableName">Liste des alias</param>
        /// <returns>nouveau alias numéroté</returns>
        public static string GetNewAlias(string pAlias, ref ArrayList pListAlias)
        {
            int aliasNumber = 0;
            string newAlias = pAlias;
            //
            while (pListAlias.Contains(newAlias))
            {
                aliasNumber++;
                newAlias = pAlias + aliasNumber.ToString();
            }
            //
            pListAlias.Add(newAlias);
            //
            return newAlias;
        }

        #endregion GetNewAlias
        #region GetExtlLinkCustomization
        /// <summary>
        /// Ajoute les éventuelles customisations paramétrées pour les colonnes: EXTLLINK, EXTLLINK2 et EXTLATTRB
        /// </summary>
        /// <param name="pReferential">REF la classe referential à renseigner</param>
        private static void GetExtlLinkCustomization(ref Referential pReferential)
        {
            IDataReader dr = null;
            try
            {
                if (pReferential.ExistsColumnEXTL)
                {
                    string SQLSelect = SQLCst.SELECT + @"DISPLAYNAME,ISMANDATORY,REGULAREXPRESSION,LISTRETRIEVAL,DATATYPE,ISHIDEINDATAGRID,DEFAULTVALUE,EXTLTYPE,";
                    //if (AspTools.IsDBRevisionEqualOrGreater(3))
                    SQLSelect += @"HELPTYPE,HELPDISPLAY,HELPMESSAGE" + Cst.CrLf;
                    //else
                    //  SQLSelect += @"null as HELPTYPE,null as HELPDISPLAY,null as HELPMESSAGE" + Cst.CrLf;  
                    SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.DEFINEEXTLLINK.ToString() + Cst.CrLf;
                    SQLSelect += SQLCst.WHERE + "TABLENAME=" + DataHelper.SQLString(pReferential.TableName);
                    //SQLSelect += SQLCst.AND + "SEQUENCENO in (1,2)";
                    dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, SQLSelect);
                    while (dr.Read())
                    {
                        //int sequenceNo = Convert.ToInt32(dr["SEQUENCENO"]);
                        string extlType = dr["EXTLTYPE"].ToString();
                        string helpType = (dr["HELPTYPE"] is DBNull ? string.Empty : dr["HELPTYPE"].ToString());
                        string helpDisplay = (dr["HELPDISPLAY"] is DBNull ? string.Empty : dr["HELPDISPLAY"].ToString());
                        string helpMessage = (dr["HELPMESSAGE"] is DBNull ? string.Empty : dr["HELPMESSAGE"].ToString());

                        ReferentialColumn rrc = null;
                        //if (pReferential.ExistsColumnEXTLLINK && sequenceNo == 1)
                        if (pReferential.ExistsColumnEXTLLINK && (extlType == Cst.ExtlType.EXTLLINK.ToString()))
                            rrc = pReferential.Column[pReferential.IndexEXTLLINK];
                        //else if (pReferential.ExistsColumnEXTLLINK2 && sequenceNo == 2)
                        else if (pReferential.ExistsColumnEXTLLINK && (extlType == Cst.ExtlType.EXTLLINK2.ToString()))
                            rrc = pReferential.Column[pReferential.IndexEXTLLINK2];
                        else if (pReferential.ExistsColumnEXTLLINK && (extlType == Cst.ExtlType.EXTLATTRB.ToString()))
                            rrc = pReferential.Column[pReferential.IndexEXTLATTRB];

                        rrc.Ressource = dr["DISPLAYNAME"].ToString();
                        rrc.RessourceSpecified = true;
                        string regularExpression = dr["REGULAREXPRESSION"].ToString();
                        if (regularExpression.Length > 0)
                            rrc.RegularExpression = regularExpression;
                        rrc.IsMandatorySpecified = true;
                        rrc.IsMandatory = Convert.ToBoolean(dr["ISMANDATORY"]);
                        rrc.DataType = new ReferentialColumnDataType();
                        rrc.DataType.value = dr["DATATYPE"].ToString();
                        rrc.IsHideInDataGridSpecified = true;
                        rrc.IsHideInDataGrid = Convert.ToBoolean(dr["ISHIDEINDATAGRID"]);

                        if (rrc.Default == null)
                            rrc.Default = new ReferentialColumnDefault[1];
                        if (rrc.Default[0] == null)
                            rrc.Default[0] = new ReferentialColumnDefault();
                        rrc.Default[0].Value = dr["DEFAULTVALUE"].ToString();

                        DispatchListRetrieval(ref rrc, dr["LISTRETRIEVAL"].ToString());

                        //PL 20120618 New features
                        #region rrc.Information
                        if (StrFunc.IsFilled(helpMessage))
                            rrc.InformationSpecified |= GetInformationCustomization(helpType, helpDisplay, helpMessage, out rrc.Information);
                        #endregion
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }
        #endregion GetExtlLinkCustomization

        #region GetInformationCustomization
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferential"></param>
        private static bool GetInformationCustomization(string pHelpType, string pHelpDisplay, string pHelpMessage,
                                                        out ReferentialInformation pInformation)
        {
            bool ret = false;
            ReferentialInformation Information = null;

            if (StrFunc.IsFilled(pHelpMessage))
            {
                Information = new ReferentialInformation();
                Information.Type = StrFunc.IsEmpty(pHelpType) ? Cst.TypeInformationMessage.Information : pHelpType.ToLower();
                Information.TypeSpecified = true;
                pHelpDisplay = pHelpDisplay.ToLower();
                if (pHelpDisplay.IndexOf("label") >= 0)
                {
                    ret = true;
                    Information.LabelMessageSpecified = true;
                    if (pHelpMessage.Length > 50)
                    {
                        Information.LabelMessage = pHelpMessage.Substring(0, 47) + "...";
                        pHelpDisplay += "tooltip";
                    }
                    else
                    {
                        Information.LabelMessage = pHelpMessage;
                    }
                }
                if (pHelpDisplay.IndexOf("popup") >= 0)
                {
                    ret = true;
                    Information.PopupMessageSpecified = true;
                    Information.PopupMessage = pHelpMessage;
                }
                if (pHelpDisplay.IndexOf("tooltip") >= 0)
                {
                    ret = true;
                    Information.ToolTipMessageSpecified = true;
                    Information.ToolTipMessage = new ReferentialInformationToolTipMessage();
                    Information.ToolTipMessage.Value = pHelpMessage;
                }
            }

            pInformation = Information;
            return ret;
        }
        #endregion

        /// <summary>
        /// Enregistre dans la database, les modifications apportées à la classe Referential
        /// </summary>
        /// <param name="pPage">Page source</param>
        /// <param name="pDbTransaction">DbTransaction</param>
        /// <param name="pIdMenu">ID du menu</param>
        /// <param name="pReferential">Classe referential</param>
        /// <param name="opRowsAffected">OUT nb de lignes affectées</param>
        /// <param name="opMessage">OUT message d'erreur (traduit)</param>
        /// <param name="opError">OUT erreur (brute, non traduite)</param>
        /// <param name="opDataKeyField">OUT nom de colonne du DataKeyField (dans le cas d'un nouvel enregistrement)</param>
        /// <param name="opDataKeyFieldNewValue">OUT nouvelle valeur de la colonne DataKeyField (dans le cas d'un nouvel enregistrement)</param>
        /// <param name="opDataKeyFieldOldValue">OUT ancienne valeur de la colonne DataKeyField (dans le cas d'un nouvel enregistrement)</param>
        /// <returns>(int)ErrLevel</returns>
        public static Cst.ErrLevel SaveReferential(PageBase pPage, IDbTransaction pDbTransaction, string pIdMenu, Referential pReferential,
            out int opRowsAffected, out string opMessage, out string opError,
            out string opDataKeyField, out string opDataKeyFieldNewValue, out string opDataKeyFieldOldValue)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;
            opRowsAffected = 0;
            opDataKeyField = opDataKeyFieldNewValue = opDataKeyFieldOldValue = string.Empty;

            #region Add new row(s) in datable(s)
            if (pReferential.isNewRecord)
            {
                pReferential.dataSet.Tables[0].Rows.Add(pReferential.dataRow);
            }

            if ((pReferential.drExternal != null) && (pReferential.dataRow.RowState != DataRowState.Deleted))
            {
                for (int i = 1; i <= pReferential.drExternal.GetLength(0); i++)
                {
                    if (pReferential.isNewDrExternal[i - 1])
                    {
                        pReferential.dataSet.Tables[i].Rows.Add(pReferential.drExternal[i - 1]);
                    }
                }
            }
            #endregion

            MQueueRequester qRequester = new MQueueRequester(new TrackerRequest(OTCmlHelper.GetDateSys(SessionTools.CS), SessionTools.NewAppInstance()));

            ret = SQLReferentialData.ApplyChangesInSQLTable(SessionTools.CS, pDbTransaction, pReferential, pReferential.dataSet,
                out opRowsAffected, out opMessage, out opError, qRequester, pIdMenu);

            // New record --> Récupération de la valeur de la colonne DataKeyField 
            if ((ret == Cst.ErrLevel.SUCCESS) && (pReferential.isNewRecord))
            {
                if (pReferential.ExistsColumnDataKeyField)
                {
                    DataRow newCreatedRow = pReferential.dataSet.Tables[0].Rows[pReferential.dataSet.Tables[0].Rows.Count - 1];
                    ReferentialColumn rrcDataKeyField = pReferential.Column[pReferential.IndexDataKeyField];
                    // DataKeyField de type "IsIdentity"
                    // S'il existe un KeyField différent de DataKeyField --> on l'utilise pour identifier le record et ainsi récupérer la valeur de l'identity généré
                    if ((rrcDataKeyField.IsIdentitySpecified) && (rrcDataKeyField.IsIdentity.Value) && (!rrcDataKeyField.IsIdentity.sourceSpecified)
                        && (pReferential.IndexKeyField != -1) && (pReferential.IndexKeyField != pReferential.IndexDataKeyField))
                    {
                        #region DataKeyfield de type IDENTITY (Oracle SEQUENCE)
                        ret = Cst.ErrLevel.ABORTED;
                        try
                        {
                            ReferentialColumn rrcKeyField = pReferential.Column[pReferential.IndexKeyField];

                            object dataKeyFieldValue = GetDataKeyFieldNewValue(SessionTools.CS, pDbTransaction,
                                rrcDataKeyField.ColumnName, pReferential.TableName,
                                rrcKeyField.ColumnName, rrcKeyField.DataType.value, newCreatedRow[rrcKeyField.ColumnName],
                                newCreatedRow["IDAINS"], newCreatedRow["DTINS"], true);

                            if (dataKeyFieldValue != null)
                            {
                                newCreatedRow.BeginEdit();
                                opDataKeyFieldOldValue = newCreatedRow[rrcDataKeyField.ColumnName].ToString();
                                newCreatedRow[rrcDataKeyField.ColumnName] = dataKeyFieldValue;
                                newCreatedRow.EndEdit();

                                opDataKeyField = rrcDataKeyField.ColumnName;
                                opDataKeyFieldNewValue = dataKeyFieldValue.ToString();
                                ret = Cst.ErrLevel.SUCCESS;
                            }
                        }
                        catch
                        {
                            ret = Cst.ErrLevel.FAILURE;
                        }
                        #endregion
                    }
                    // DataKeyField saisisable
                    else if ((!rrcDataKeyField.IsIdentitySpecified) || (!rrcDataKeyField.IsIdentity.Value) || (rrcDataKeyField.IsIdentity.sourceSpecified))
                    {
                        #region DataKeyField saisi
                        opDataKeyField = rrcDataKeyField.ColumnName;
                        opDataKeyFieldNewValue = newCreatedRow[rrcDataKeyField.ColumnName].ToString();
                        opDataKeyFieldOldValue = null; //TBD
                        #endregion
                    }
                }
            }

            // Purge du cache Spheres®  
            if (ret == Cst.ErrLevel.SUCCESS && opRowsAffected >= 1)
            {
                RepositoryTools.RemoveCache(pReferential.dataSet.Tables[0].TableName);
            }

            // Maj de Sessionrestrict (Si TableName est une OTCml_TBL) 
            if ((ret == Cst.ErrLevel.SUCCESS) && (opRowsAffected >= 1))
            {
                if (Enum.IsDefined(typeof(Cst.OTCml_TBL), pReferential.TableName))
                    RepositoryTools.AddItemSessionRestrict(pReferential);
            }
            return ret;
        }


        /// <summary>
        /// Ajoute un enregistrement dans SESSIONRESTRICT lorsqu'il y a ajout d'un nouvel élément via le référentiel
        /// </summary>
        /// FI 20150529 [20982] Add method
        /// FI 20150720 [20982] Modify
        public static void AddItemSessionRestrict(Referential pReferential)
        {

            if (pReferential.isNewRecord && SessionTools.User.IsApplySessionRestrict())
            {
                if (Enum.IsDefined(typeof(Cst.OTCml_TBL), pReferential.TableName))
                {
                    Cst.OTCml_TBL tbl = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pReferential.TableName);

                    RestrictionUserBase restricUser = null;
                    switch (tbl)
                    {
                        case Cst.OTCml_TBL.ACTOR:
                            restricUser = new RestrictionActor();
                            break;
                        case Cst.OTCml_TBL.INSTRUMENT:
                            restricUser = new RestrictionInstrument();
                            break;
                        case Cst.OTCml_TBL.MARKET:
                            restricUser = new RestrictionMarket();
                            break;
                        case Cst.OTCml_TBL.IOTASK:
                            restricUser = new RestrictionIOTask();
                            break;
                        default:
                            break;
                    }

                    if (null != restricUser)
                    {
                        DataRow row = pReferential.dataSet.Tables[0].Rows[(pReferential.dataSet.Tables[0].Rows.Count - 1)];

                        string columnId = OTCmlHelper.GetColunmID(pReferential.TableName);
                        if (row[columnId] == Convert.DBNull)
                            throw new NotSupportedException(StrFunc.AppendFormat("column ({0}): value is DBNull. This value is not Supported", columnId));
                        if (row["IDENTIFIER"] == Convert.DBNull)
                            throw new NotSupportedException(StrFunc.AppendFormat("column ({0}): value is DBNull. This value is not Supported", "IDENTIFIER"));

                        restricUser.SetItemRestriction(Convert.ToInt32(row[columnId]), Convert.ToString(row["IDENTIFIER"]), true);

                        SqlSessionRestrict sqlSessionRestrict = new SqlSessionRestrict(SessionTools.CS, SessionTools.NewAppInstance());
                        sqlSessionRestrict.SetRestrictUseSelectUnion(restricUser);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataKeyFieldColumnName"></param>
        /// <param name="pTableName"></param>
        /// <param name="pKeyFieldColumnName"></param>
        /// <param name="pKeyFieldDataType"></param>
        /// <param name="pKeyfieldValue"></param>
        /// <param name="pIDAValue"></param>
        /// <param name="pDTValue"></param>
        /// <param name="pIsUseINS"></param>
        /// <returns></returns>
        private static object GetDataKeyFieldNewValue(string pCS, IDbTransaction pDbTransaction,
            string pDataKeyFieldColumnName, string pTableName, string pKeyFieldColumnName, string pKeyFieldDataType,
            object pKeyfieldValue, object pIDAValue, object pDTValue, bool pIsUseINS)
        {
            IDbDataParameter sqlParam_KEYFIELD = null;
            if (TypeData.IsTypeInt(pKeyFieldDataType))
                sqlParam_KEYFIELD = new EFSParameter(pCS, "KEYFIELD", DbType.Int64).DataParameter;
            else if (TypeData.IsTypeBool(pKeyFieldDataType))
                sqlParam_KEYFIELD = new EFSParameter(pCS, "KEYFIELD", DbType.Boolean).DataParameter;
            else if (TypeData.IsTypeDec(pKeyFieldDataType))
                sqlParam_KEYFIELD = new EFSParameter(pCS, "KEYFIELD", DbType.Decimal).DataParameter;
            else if (TypeData.IsTypeDateOrDateTime(pKeyFieldDataType))
                sqlParam_KEYFIELD = new EFSParameter(pCS, "KEYFIELD", DbType.DateTime).DataParameter;
            else
                sqlParam_KEYFIELD = new EFSParameter(pCS, "KEYFIELD", DbType.AnsiString, 64).DataParameter;

            IDbDataParameter sqlParam_IDAxxx = new EFSParameter(pCS, "IDAxxx", DbType.Int32).DataParameter;
            IDbDataParameter sqlParam_DTxxx = new EFSParameter(pCS, "DTxxx", DbType.DateTime).DataParameter;

            sqlParam_KEYFIELD.Value = pKeyfieldValue;
            sqlParam_IDAxxx.Value = pIDAValue;
            sqlParam_DTxxx.Value = pDTValue;

            string columnNameIDAxxx;
            string columnNameDTxxx;
            if (pIsUseINS)
            {
                columnNameIDAxxx = "IDAINS";
                columnNameDTxxx = "DTINS";
            }
            else
            {
                columnNameIDAxxx = "IDAUPD";
                columnNameDTxxx = "DTUPD";
            }

            string SQLQuery;
            SQLQuery = SQLCst.SELECT + pDataKeyFieldColumnName + " as DATAKEYFIELDVALUE " + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + pTableName + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "(" + pKeyFieldColumnName + "=@KEYFIELD)";
            SQLQuery += SQLCst.AND + "(" + columnNameIDAxxx + "=@IDAxxx)";
            SQLQuery += SQLCst.AND + "(" + columnNameDTxxx + "=@DTxxx)";

            object ret = null;
            if (pDbTransaction == null)
                ret = DataHelper.ExecuteScalar(pCS, CommandType.Text, SQLQuery, sqlParam_KEYFIELD, sqlParam_IDAxxx, sqlParam_DTxxx);
            else
                ret = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, SQLQuery, sqlParam_KEYFIELD, sqlParam_IDAxxx, sqlParam_DTxxx);
            return ret;
        }

        #region public UpdateDataRowFromControls
        /// <revision>
        ///     <version>1.0.4</version><date>20041202</date><author>OD</author>
        ///     <comment>
        ///     Utilisation de la methode GetStringDefaultForReferencialColumn() pour recuperer le defaultValue
        ///     </comment>
        /// </revision>
        /// <revision>
        ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
        ///     <comment>
        ///     [To insert here a description of the made modifications ]
        ///     </comment>
        /// </revision>        
        /// <summary>
        /// Mets à jour les données contenu dans le datarow de la classe Referential en fonction des valeurs dans les controls de la page appelante
        /// </summary>
        /// <param name="pPage">page appelante</param>
        /// <param name="pReferential">REF classe referential à mettre à jour</param>
        public static void UpdateDataRowFromControls(RepositoryPageBase pPage, IDbTransaction pDbTransaction)
        {

            string defaultBusinessCenter = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_businesscenter");

            pPage.referential.dataRow.BeginEdit();
            DataRow currentRow = null;

            for (int i = 0; i < pPage.referential.Column.Length; i++)
            {
                ReferentialColumn rrc = pPage.referential.Column[i];
                bool isColumnMandatory = (rrc.IsMandatorySpecified ? rrc.IsMandatory : false);
                Control ctrlRef;
                bool isVisible = (false == (rrc.IsIdentity.Value || rrc.IsHide));
                bool isEnabled = false;

                #region External DR BeginEdit
                if (rrc.IsAdditionalData)
                {
                    currentRow = pPage.referential.drExternal[rrc.ExternalFieldID];
                    if (currentRow.RowState == DataRowState.Deleted)
                    {
                        // NB: Dans le cas des rôles, la ligne se trouve deleted lorsque le rôle et décoché.
                        //     On ne peut donc pas mettre à jour l'acteur associé, ...
                        continue;
                    }
                    else
                    {
                        currentRow.BeginEdit();
                    }
                }
                else
                {
                    currentRow = pPage.referential.dataRow;
                }
                #endregion External DR BeginEdit

                if (pPage.referential.isNewRecord)
                {
                    if (rrc.IsUpdatableSpecified && rrc.IsUpdatable.isupdatableincreationSpecified)
                        isEnabled = rrc.IsUpdatable.isupdatableincreation;
                    else
                        isEnabled = true;
                }
                else if (rrc.IsUpdatableSpecified && rrc.IsUpdatable.Value)
                {
                    isEnabled = true;
                }

                #region Cas particulier d'un Identity avec Source (eg. GETID) PL 20130403
                if (pPage.referential.ExistsColumnIDENTITY && rrc.IsIdentity.Value && rrc.IsIdentity.sourceSpecified)
                {
                    if (pPage.referential.isNewRecord)
                    {
                        //MAJ uniquement en cas d'insertion
                        if (rrc.IsIdentity.source.ToUpper().Contains("SRC=TRADEID") || rrc.IsIdentity.source.ToUpper().Contains("SRC=IDTRADE"))
                        {
                            #region TRADEID/IDTRADE
                            //NB: La source est de type Src=TRADEID;Instr=<InstrumentIdentifier>;Book=<BookColumn>;TrdDt=<TradeDateColumn>;[BizDt=<BusinessDateColumn>;]
                            //    ex. Src=TRADEID;Instr=ExchangeTradedFuture;Book=IDB_PAY;TrdDt=DTBUSINESS;

                            SQL_Instrument sqlInstrument = null;
                            int idAEntity = 0;
                            DateTime tradeDate = DateTime.MinValue;
                            DateTime businessDate = DateTime.MinValue;

                            string[] srcs = rrc.IsIdentity.source.Split(";".ToCharArray());
                            foreach (string src in srcs)
                            {
                                if (src.ToUpper().StartsWith("INSTR"))
                                {
                                    string instrument = src.Remove(0, "INSTR".Length + 1);
                                    sqlInstrument = new SQL_Instrument(pPage.CS, instrument);
                                }
                                else if (src.ToUpper().StartsWith("BOOK"))
                                {
                                    #region Entity
                                    string columnName = src.Remove(0, "BOOK".Length + 1);
                                    if (null != pPage.referential[columnName])
                                    {
                                        ReferentialColumn rrcBD = pPage.referential[columnName];
                                        string controlID = ControlID.getID(rrcBD.ColumnName, rrcBD.DataType.value, Cst.DDL);
                                        ctrlRef = pPage.MasterPage_ContentPlaceHolder.FindControl(controlID);

                                        string data = (ctrlRef as DropDownList).SelectedValue;
                                        if (IntFunc.IsPositiveInteger(data))
                                        {
                                            SQL_Book sqlBook = new SQL_Book(pPage.CS, Convert.ToInt32(data));
                                            if (sqlBook.IsLoaded)
                                            {
                                                idAEntity = sqlBook.IdA_Entity;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else if (src.ToUpper().StartsWith("TRDDT"))
                                {
                                    #region Trade date
                                    string columnName = src.Remove(0, "TRDDT".Length + 1);
                                    if (null != pPage.referential[columnName])
                                    {
                                        ReferentialColumn rrcBD = pPage.referential[columnName];
                                        string controlID = ControlID.getID(rrcBD.ColumnName, rrcBD.DataType.value, Cst.TXT);
                                        ctrlRef = pPage.MasterPage_ContentPlaceHolder.FindControl(controlID);

                                        string data = (ctrlRef as TextBox).Text.Trim();
                                        if (!String.IsNullOrEmpty(data))
                                            tradeDate = new DtFuncML(pPage.CS, defaultBusinessCenter, SessionTools.User.entity_IdA, 0, 0, null).StringToDateTime(data);
                                    }
                                    #endregion
                                }
                                else if (src.ToUpper().StartsWith("BIZDT"))
                                {
                                    #region Business date
                                    string columnName = src.Remove(0, "BIZDT".Length + 1);
                                    if (null != pPage.referential[columnName])
                                    {
                                        ReferentialColumn rrcBD = pPage.referential[columnName];
                                        string controlID = ControlID.getID(rrcBD.ColumnName, rrcBD.DataType.value, Cst.TXT);
                                        ctrlRef = pPage.MasterPage_ContentPlaceHolder.FindControl(controlID);

                                        string data = (ctrlRef as TextBox).Text.Trim();
                                        if (!String.IsNullOrEmpty(data))
                                            businessDate = new DtFuncML(pPage.CS, defaultBusinessCenter, SessionTools.User.entity_IdA, 0, 0, null).StringToDateTime(data);
                                    }
                                    #endregion
                                }
                            }
                            if (DtFunc.IsDateTimeEmpty(businessDate))
                            {
                                businessDate = tradeDate;
                            }
                            else if (DtFunc.IsDateTimeEmpty(tradeDate))
                            {
                                tradeDate = businessDate;
                            }

                            string newIdentifier, prefix, suffix;
                            TradeRDBMSTools.BuildTradeIdentifier(pPage.CS, pDbTransaction,
                                sqlInstrument, idAEntity, null, tradeDate, businessDate,
                                out newIdentifier, out prefix, out suffix);

                            //Constitution du nouvel identifiant 
                            currentRow[rrc.DataField] = prefix + newIdentifier + suffix;
                            #endregion
                        }
                        else if (rrc.IsIdentity.source.ToUpper().Contains("SRC=GETID") || rrc.IsIdentity.source.ToUpper().Contains("SRC=ID"))
                        {
                            #region GETID/ID
                            currentRow[rrc.DataField] = Convert.DBNull;//TBD
                            #endregion
                        }
                    }
                }
                #endregion

                if (isVisible)
                {
                    #region isVisible
                    if (isEnabled)
                    {
                        string columnName = rrc.ColumnName + (rrc.IsAdditionalData ? rrc.ExternalFieldID.ToString() : string.Empty);

                        string controlID = ControlID.getID(columnName, rrc.DataType.value,
                            (rrc.TypeAheadEnabled ? Cst.TXT : (RepositoryTools.IsDataForDDL(rrc) ? Cst.DDL : null)));

                        if (rrc.IsAdditionalData)
                        {
                            if (rrc.IsRole || rrc.IsItem)
                                controlID = controlID.Replace(Cst.TXT, Cst.CHK);
                        }

                        ctrlRef = pPage.MasterPage_ContentPlaceHolder.FindControl(controlID);

                        if (null != ctrlRef)
                        {
                            if (ctrlRef is CheckBox)
                            {
                                #region CheckBox
                                if (rrc.IsExternal2)
                                    currentRow[rrc.DataField] = ((ctrlRef as CheckBox).Checked ? "TRUE" : "FALSE"); //le champ est de type string
                                else
                                    currentRow[rrc.DataField] = ((ctrlRef as CheckBox).Checked ? 1 : 0);
                                #endregion CheckBox
                            }
                            else if (ctrlRef is TextBox)
                            {
                                #region TextBox
                                string data = (ctrlRef as TextBox).Text.Trim();

                                switch (rrc.DataTypeEnum)
                                {
                                    case TypeData.TypeDataEnum.date:
                                    case TypeData.TypeDataEnum.datetime:

                                        #region Date|DateTime
                                        if (StrFunc.IsEmpty(data))
                                        {
                                            currentRow[rrc.DataField] = isColumnMandatory ? DateTime.MinValue : Convert.DBNull;
                                        }
                                        else
                                        {
                                            string fmtDate = null;
                                            if (rrc.IsExternal2)
                                                fmtDate = TypeData.IsTypeDate(rrc.DataType.value) ? DtFunc.FmtISODate : DtFunc.FmtISODateTime;

                                            DateTime dtResult = new DtFuncML(pPage.CS, defaultBusinessCenter,
                                                SessionTools.User.entity_IdA, 0, 0, null).StringToDateTime(data, fmtDate);
                                            if (DtFunc.IsDateTimeFilled(dtResult))
                                                currentRow[rrc.DataField] = dtResult;
                                        }
                                        #endregion Date|DateTime

                                        break;

                                    case TypeData.TypeDataEnum.time:

                                        #region Time
                                        if (StrFunc.IsEmpty(data) && (false == isColumnMandatory))
                                        {
                                            currentRow[rrc.DataField] = Convert.DBNull;
                                        }
                                        else
                                        {
                                            DateTime dtMinDefaultValue = new DtFunc().StringyyyyMMddToDateTime("19000101");
                                            currentRow[rrc.DataField] = DtFunc.AddTimeToDate(dtMinDefaultValue,
                                                new DtFunc().StringToDateTime(data, DtFunc.FmtISOShortTime));
                                        }
                                        #endregion Time

                                        break;

                                    default:

                                        #region Autres type de données
                                        if (rrc.TypeAheadEnabled)
                                        {
                                            #region TypeAhead (AutoComplete)
                                            string idTxt = ControlID.getID(rrc.ColumnName + (rrc.IsAdditionalData ? rrc.ExternalFieldID.ToString() : string.Empty), null, "TMP");
                                            HtmlInputHidden taheadRef = pPage.MasterPage_ContentPlaceHolder.FindControl(idTxt) as HtmlInputHidden;
                                            if (null != taheadRef)
                                            {
                                                currentRow[rrc.DataField] = (StrFunc.IsEmpty(taheadRef.Value) ? Convert.DBNull : taheadRef.Value);
                                            }
                                            else
                                                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                                                    String.Format("No valid control: {0} - ID: {1}",
                                                        ctrlRef.GetType(), idTxt), new Exception());
                                            #endregion TypeAhead (AutoComplete)
                                        }
                                        else
                                        {
                                            #region Text
                                            string resAUTOMATIC_COMPUTE = Ressource.GetString(Cst.AUTOMATIC_COMPUTE.ToString());
                                            if (data.StartsWith(resAUTOMATIC_COMPUTE))
                                            {
                                                string defaultValue = rrc.GetStringDefaultValue(pPage.referential.TableName);
                                                if (Cst.AUTOMATIC_COMPUTE.ToString() == defaultValue)
                                                    data = Cst.AUTOMATIC_COMPUTE.ToString() + data.Remove(0, resAUTOMATIC_COMPUTE.Length);
                                            }

                                            if (String.IsNullOrEmpty(data) && (rrc.ColumnName == "DISPLAYNAME") && (pPage.referential["IDENTIFIER"] != null))
                                            {
                                                currentRow[rrc.DataField] = currentRow["IDENTIFIER"];
                                            }
                                            else if (StrFunc.IsEmpty(data) && !isColumnMandatory)
                                            {
                                                currentRow[rrc.DataField] = Convert.DBNull;
                                            }
                                            else
                                            {
                                                bool setInDataRow = true;
                                                #region Password
                                                if (rrc.TextMode == TextBoxMode.Password.ToString())
                                                {
                                                    if (data == Cst.StringForTextBoxModePassword)
                                                    {
                                                        //Data not modify --> no update in database
                                                        setInDataRow = false;
                                                    }
                                                    else
                                                    {
                                                        //Applic hash
                                                        string algorithm = rrc.Coding;
                                                        if (StrFunc.IsEmpty(algorithm))
                                                            algorithm = SystemSettings.GetAppSettings_Software("_Hash");
                                                        if (StrFunc.IsEmpty(algorithm))
                                                            algorithm = Cst.HashAlgorithm.MD5.ToString(); //Default value

                                                        data = StrFunc.HashData(data, algorithm);
                                                    }
                                                }
                                                #endregion Password

                                                if (setInDataRow)
                                                {
                                                    if (TypeData.IsTypeDec(rrc.DataType.value))
                                                        currentRow[rrc.DataField] = DecFunc.DecValue(data);
                                                    else
                                                        currentRow[rrc.DataField] = data;
                                                }
                                            }
                                            #endregion Text
                                        }
                                        #endregion Autres type de données

                                        break;
                                }
                                #endregion TextBox
                            }
                            else if (ctrlRef is HtmlSelect)
                            {
                                #region HtmlSelect
                                HtmlSelect ddl = ctrlRef as HtmlSelect;
                                if (((0 == ddl.SelectedIndex) || StrFunc.IsEmpty(ddl.Value.Trim())) && (false == isColumnMandatory))
                                    currentRow[rrc.DataField] = Convert.DBNull;
                                else if (TypeData.IsTypeInt(rrc.DataType.value))
                                    currentRow[rrc.DataField] = Convert.ToInt64(ddl.Value);
                                else
                                    currentRow[rrc.DataField] = ddl.Value;
                                #endregion HtmlSelect
                            }
                            else if (ctrlRef is DropDownList)
                            {
                                #region DropDownList
                                bool isFK = rrc.IsForeignKeyField || (StrFunc.IsFilled(pPage.referential.valueForeignKeyField));
                                if (isFK)
                                {
                                    #region ForeignKey
                                    if ((ctrlRef as BS_DropDownList).hasViewer)
                                    {
                                        TextBox txtViewer = (ctrlRef as BS_DropDownList).txtViewer as TextBox;
                                        currentRow[rrc.DataField] = txtViewer.Attributes["data-value"].ToString();
                                    }
                                    #endregion ForeignKey
                                }
                                else
                                {
                                    if (rrc.IsAutoPostBack && StrFunc.IsEmpty(pPage.Request.Form[controlID]))
                                    {
                                        // Ajout du if (rrc.IsAutoPostBack...) 
                                        // Rq. Sur une DDL AutoPostBack mise à jour et disabled via JS, ASP.NET ne récupère pas la nouvelle valeur initialisée par JS. 
                                        // Utilisation ici, faute de mieux, de IsAutoPostBack et de Request.Form[], pour identifier cette DDL particulière...
                                        // Ex.: DDL ASSETCATEGORY dans COLLATERALPRIORITY
                                        currentRow[rrc.DataField] = Convert.DBNull;
                                    }
                                    else
                                    {
                                        DropDownList ddl = ctrlRef as DropDownList;

                                        if ((0 == ddl.Items.Count) || StrFunc.IsEmpty(ddl.SelectedValue))
                                        {
                                            currentRow[rrc.DataField] = Convert.DBNull;

                                        }
                                        else
                                        {
                                            if (TypeData.IsTypeInt(rrc.DataType.value))
                                                currentRow[rrc.DataField] = Convert.ToInt64(ddl.SelectedValue);
                                            else
                                                currentRow[rrc.DataField] = ddl.SelectedValue;
                                        }
                                    }
                                }
                                #endregion DropDownList
                            }
                        }
                    }
                    #endregion isVisible
                }
                else
                {
                    #region not isVisible
                    //Update data may be not captured (data not visibled on the screen invisibles à l'écran)
                    if ((rrc.IsAdditionalData && pPage.referential.isNewDrExternal[rrc.ExternalFieldID]))
                    {
                        try
                        {
                            string defaultValue = rrc.GetStringDefaultValue(pPage.referential.TableName);
                            currentRow[rrc.DataField] = StrFunc.IsEmpty(defaultValue) ? Convert.DBNull : defaultValue;
                        }
                        catch
                        {
                            currentRow[rrc.DataField] = Convert.DBNull;
                        }
                    }
                    #endregion not isVisible
                }

                #region External DR EndEdit
                if (rrc.IsAdditionalData)
                {
                    bool isNewDr = (pPage.referential.isNewDrExternal[rrc.ExternalFieldID]);
                    if (rrc.IsExternal)
                    {
                        #region rrc.IsExternal
                        currentRow["TABLENAME"] = rrc.ExternalTableName;
                        currentRow["IDENTIFIER"] = rrc.ExternalIdentifier;
                        currentRow["ID"] = pPage.referential.dataRow[pPage.referential.IndexColSQL_DataKeyField];
                        currentRow[(pPage.referential.isNewDrExternal[rrc.ExternalFieldID] ? "DTINS" : "DTUPD")] = OTCmlHelper.GetDateSys(pPage.CS);
                        currentRow[(pPage.referential.isNewDrExternal[rrc.ExternalFieldID] ? "IDAINS" : "IDAUPD")] = SessionTools.Collaborator_IDA;
                        #endregion rrc.IsExternal
                    }
                    else if (rrc.IsRole || rrc.IsItem)
                    {
                        #region rrc.IsRole or rrc.IsItem
                        string controlID = null;
                        string data = null;
                        //
                        bool isSet = ((rrc.IsRole && rrc.DataField.StartsWith("IDROLE")) || (rrc.IsItem));
                        if (isSet)
                        {
                            if (currentRow[rrc.DataField].ToString() == "1")
                            {
                                //"1" --> Checked, donc rôle ou élément accordé (nouvellement ou déjà en place).
                                currentRow[rrc.DataField] = rrc.AliasTableName.Substring(2);
                                if (isNewDr)
                                {
                                    //Nouvelle ligne, donc le rôle ou l'élément vient d'être accordé (sinon RAS)
                                    if (currentRow.Table.Columns.IndexOf("DTENABLED") >= 0)
                                    {
                                        controlID = ControlID.getID("DTENABLED", TypeData.TypeDataEnum.date.ToString());
                                        ctrlRef = pPage.MasterPage_ContentPlaceHolder.FindControl(controlID);
                                        data = ((TextBox)ctrlRef).Text.Trim();
                                        currentRow["DTENABLED"] = new DtFuncML(pPage.CS, defaultBusinessCenter, SessionTools.User.entity_IdA, 0, 0, null).StringToDateTime(data);
                                    }
                                    currentRow["ID"] = pPage.referential.dataRow[pPage.referential.IndexColSQL_DataKeyField];
                                    currentRow[(pPage.referential.isNewDrExternal[rrc.ExternalFieldID] ? "DTINS" : "DTUPD")] = OTCmlHelper.GetDateSys(pPage.CS);
                                    currentRow[(pPage.referential.isNewDrExternal[rrc.ExternalFieldID] ? "IDAINS" : "IDAUPD")] = SessionTools.Collaborator_IDA;
                                }
                            }
                            else
                            {
                                // "0" --> Unchecked, donc rôle ou l'élément non accordé (nouvellement ou jamais en place).
                                if (isNewDr)
                                    // Nouvelle ligne, donc le rôle ou l'élément n'était pas accordé
                                    pPage.referential.isNewDrExternal[rrc.ExternalFieldID] = false;
                                else
                                {
                                    // Non nouvelle ligne, donc le rôle ou l'élément vient d'être retiré
                                    currentRow.Delete();
                                }
                            }
                        }
                        #endregion rrc.IsRole or rrc.IsItem
                    }
                    currentRow.EndEdit();
                }
                #endregion External DR EndEdit
            }

            currentRow = pPage.referential.dataRow;

            #region Update DTINS, IDAINS and DTUPD, IDAUPD, ROWATTRIBUT
            if ((pPage.referential.isNewRecord && (pPage.referential.ExistsColumnsINS)) || (!pPage.referential.isNewRecord && (pPage.referential.ExistsColumnsUPD)))
            {
                currentRow[(pPage.referential.isNewRecord ? "DTINS" : "DTUPD")] = OTCmlHelper.GetDateSys(pPage.CS);
                currentRow[(pPage.referential.isNewRecord ? "IDAINS" : "IDAUPD")] = SessionTools.Collaborator_IDA;
            }
            if (pPage.referential.isNewRecord)
            {
                try
                {
                    // En cas de duplication (= création) réinitialisation des colonnes UPD
                    if (pPage.referential.ExistsColumnsUPD)
                    {
                        currentRow["DTUPD"] = Convert.DBNull;
                        currentRow["IDAUPD"] = Convert.DBNull;
                    }
                    currentRow["ROWATTRIBUT"] = Convert.DBNull;
                }
                catch
                { }
            }
            #endregion

            pPage.referential.dataRow.EndEdit();
        }
        #endregion UpdateDataRowFromControls

        #region public ValuateDTHOLIDAYVALUE
        /// <summary>
        /// Pour toutes les lignes du DataTable {pDt}, Valorise la colonne DTHOLIDAYVALUE avec la date du prochain jour férié correspondant
        /// </summary>
        /// <param name="pDt"></param>
        /// FI 20190509 [24661] Add date de reference
        public static void ValuateDTHOLIDAYVALUE(DataTable pDt, Nullable<DateTime> pDtREf)
        {
            int totalRows = pDt.Rows.Count;
            int currentRow = 0;
            while (currentRow < totalRows)
            {
                ValuateDTHOLIDAYVALUE(pDt.Rows[currentRow], pDtREf);
                currentRow++;
            }
        }
        
        /// <summary>
        /// Valorisation, à partir de DTHOLIDAYVALUE, de la colonne fictive DTHOLIDAYNEXTDATE avec la date du prochain jour férié.
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pEfs_BusinessCenters"></param>
        /// FI 20190509 [24661] Add Date de reference
        public static void ValuateDTHOLIDAYVALUE(DataRow pRow,  Nullable<DateTime> pDtRef)
        {

            string data = pRow[Cst.OTCml_COL.DTHOLIDAYVALUE.ToString()].ToString();
            if (StrFunc.IsFilled(data))
            {
                EFS_BusinessCenters bcs = new EFS_BusinessCenters(SessionTools.CS);
                DateTime holidayDate = bcs.GetDate(data, pDtRef);
                if (DtFunc.IsDateTimeFilled(holidayDate))
                {
                    //data = DtFunc.DateTimeToString(holidayDate, DtFunc.FmtShortDate);
                    pRow.BeginEdit();
                    pRow[Cst.OTCml_COL.DTHOLIDAYNEXTDATE.ToString()] = holidayDate;
                    pRow.EndEdit();
                }
            }
        }
        #endregion ValuateDTHOLIDAYVALUE

        /// <summary>
        /// Initialise les champs IDxxx (IDForItemTemplate et IDForItemTemplateRelation) d'une classe referential
        /// IDForItemTemplate et IDForItemTemplateRelation: 
        ///     Champs utilisé pour l'affectation de l'ID du control dans le cas de templateColumn qui nécessite plusieurs ID 
        ///     car le control pour l'affichage et le control pour la modification des données sont deux controles distincts 
        ///     donc nécessité d'un ID supplémentaire
        /// Rappel: ItemTemplate est un element de TemplateColumn
        /// </summary>
        /// <param name="pReferential">REF la classe referential à initialiser</param>
        public static void InitializeID(ref Referential pReferential)
        {
            for (int index = 0; index < pReferential.Column.Length; index++)
            {
                ReferentialColumn rrc = pReferential.Column[index];
                if (rrc.IsAdditionalData)
                    rrc.IDForItemTemplate = "Col" + rrc.ExternalIdentifier;
                else
                    rrc.IDForItemTemplate = rrc.DataField;

                if (rrc.ExistsRelation)
                    rrc.IDForItemTemplateRelation = rrc.Relation[0].AliasTableName + "_" + rrc.Relation[0].ColumnSelect[0].ColumnName;
            }
        }

        /// <summary>
        ///  Retourne un array à partir des paramètres de la requête http
        ///  P1=> array[0], P2=> array[1], P3=> array[1]
        /// </summary>
        /// <param name="pPage"></param>
        /// <returns></returns>
        public static string[] GetQueryStringParam(Page pPage)
        {
            NameValueCollection nvcQueryString = pPage.Request.QueryString;
            string[] ret = null;
            int length = 0;
            //
            if (ArrFunc.IsFilled(nvcQueryString))
            {
                for (int i = 0; i < 32; i++) // 32 params Max
                {
                    if (null == nvcQueryString["P" + (i + 1).ToString()])
                    {
                        length = i;
                        break;
                    }
                }
                //
                if (length > 0)
                    ret = new string[length];
                //
                if (ArrFunc.IsFilled(ret))
                {
                    for (int i = 0; i < ret.Length; i++)
                    {
                        ret[i] = nvcQueryString["P" + (i + 1).ToString()];
                    }
                }
            }
            //
            return ret;
        }

        #region GetID
        public static string GetID(ReferentialColumn pRc)
        {
            return GetID(pRc, Cst.TXT);
        }
        public static string GetID(ReferentialColumn pRc, string pPrefix)
        {
            return ControlID.getID(pRc.ColumnName + (pRc.IsAdditionalData ? pRc.ExternalFieldID.ToString() : string.Empty), pRc.DataType.value, pPrefix);
        }
        #endregion GetID

        #region GetCurrentValue
        public static string GetCurrentValue(Referential pReferential, ReferentialColumn pRc)
        {
            string currentValue = string.Empty;
            if (pRc.IsAdditionalData)
                currentValue = (pReferential.drExternal[pRc.ExternalFieldID])[pRc.ColumnName].ToString();
            else
                currentValue = pReferential.dataRow[pRc.DataField].ToString();
            return currentValue;
        }
        #endregion GetCurrentValue


        #region public GetCheckBoxValue
        /// <summary>
        /// Obtient la valeur à renseigner pour une colonne de type bool 
        /// </summary>
        /// <param name="pReferential">classe referential</param>
        /// <param name="pRrc">la colonne concernée</param>
        /// <returns>valur de la checkBox</returns>
        public static bool GetCheckBoxValue(Referential pReferential, ReferentialColumn pRc)
        {
            bool ret = false;
            if ((pRc.IsAdditionalData && pReferential.isNewDrExternal[pRc.ExternalFieldID]))
            {
                if (pRc.ExistsDefaultValue)
                    ret = BoolFunc.IsTrue(pRc.Default[0].Value);
            }
            else
            {
                string data;
                if (pRc.IsAdditionalData)
                    data = (pReferential.drExternal[pRc.ExternalFieldID])[pRc.ColumnName].ToString();
                else
                    data = pReferential.dataRow[pRc.DataField].ToString();
                ret = BoolFunc.IsTrue(data);
            }
            return ret;
        }
        #endregion GetCheckBoxValue

        #region public ConstructColumnOrderBy
        public static string ConstructColumnOrderBy(string pAliasTable, string pColumnName, string pSqlOrder, bool pIsWithColumnAlias)
        {
            // 20091021 RD DYNAMIC_ALIASTABLE déplacé dans la classe Cst pour pouvoir le partager
            string ret = pColumnName;
            //
            if (StrFunc.IsFilled(pSqlOrder))
            {
                if (pSqlOrder.IndexOf(Cst.DYNAMIC_ALIASTABLE) >= 0)
                    //ex.: replicate('' '', 64-len(<aliasTable>.IDENTIFIER)) + <aliasTable>.IDENTIFIER
                    ret = pSqlOrder.Replace(Cst.DYNAMIC_ALIASTABLE, pAliasTable);
                else
                    ret = pAliasTable + "." + pSqlOrder;
            }
            //
            if (pIsWithColumnAlias)
                ret = ret.Replace(pAliasTable + ".", pAliasTable + "_"); // 20090831 RD / Il peut y avoir un problème si la colonne contient un alias     
            //
            return ret;
        }
        #endregion ConstructColumnOrderBy




        #region GetDataTRIM
        public static string GetDataTRIM(string pData, out string opCulture)
        {
            const string TODOTOTRANSLATE = @"TODO/TOTRANSLATE";
            string Template = @"<Info lang=""EN"">" + Cst.CrLf + TODOTOTRANSLATE + Cst.CrLf + Cst.CrLf;
            Template += @"<Info lang=""FR"">" + Cst.CrLf + TODOTOTRANSLATE + Cst.CrLf + Cst.CrLf;
            Template += @"<Info lang=""IT"">" + Cst.CrLf + TODOTOTRANSLATE + Cst.CrLf + Cst.CrLf;
            Template += @"<Info lang=""ES"">" + Cst.CrLf + TODOTOTRANSLATE + Cst.CrLf + Cst.CrLf;
            string culture = SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper();
            //
            bool isFindCulture = false;
            int start;
            string search;
            string ret = string.Empty;
            //
            if (pData == Template)
                pData = string.Empty;
            //
            if (StrFunc.IsFilled(pData))
            {
                int guard = 0;
                while (!isFindCulture && (culture != "**") && (guard < 5))
                {
                    guard++;
                    //
                    search = @"<Info lang=""" + culture + @""">";
                    start = pData.IndexOf(search);
                    if (start >= 0)
                    {
                        //Culture courante trouvée
                        ret = pData.Substring(start + search.Length + 1).TrimStart();
                        //Vérification que la culture trouvée ne contient pas TODOTOTRANSLATE
                        isFindCulture = (ret.IndexOf(TODOTOTRANSLATE) != 0);
                    }
                    #region Recherche d'une autre cuture
                    if (!isFindCulture)
                    {
                        switch (SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                        {
                            case "EN":
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "FR";
                                else if (culture == "FR")
                                    culture = "IT";
                                else if (culture == "IT")
                                    culture = "ES";
                                else
                                    culture = "**";
                                break;
                            case "FR":
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "EN";
                                else if (culture == "EN")
                                    culture = "IT";
                                else if (culture == "IT")
                                    culture = "ES";
                                else
                                    culture = "**";
                                break;
                            case "IT":
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "EN";
                                else if (culture == "EN")
                                    culture = "FR";
                                else if (culture == "FR")
                                    culture = "ES";
                                else
                                    culture = "**";
                                break;
                            case "ES":
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "EN";
                                else if (culture == "EN")
                                    culture = "FR";
                                else if (culture == "FR")
                                    culture = "IT";
                                else
                                    culture = "**";
                                break;
                            default:
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "EN";
                                else
                                    culture = "**";
                                break;
                        }
                    }
                    #endregion
                }
                if (isFindCulture)
                {
                    search = @"<Info lang=""";
                    start = ret.IndexOf(search);
                    if (start >= 0)
                    {
                        //Suppression des culture suivantes
                        ret = ret.Substring(0, start);
                    }
                }
                else
                {
                    //Aucune culture trouvée --> le message doit être en English...
                    ret = pData;
                    ret = ret.Replace(TODOTOTRANSLATE, "...");
                }
            }
            //
            opCulture = culture;
            return ret;
        }
        #endregion GetDataTRIM

        #region GetCssClassForXXXXValue
        public static EFSCssClass.CssClassEnum GetCssClassForTradeSideValue(string pData, bool pIsLight)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.DataGrid_BuyerSeller;

            switch (pData)
            {
                // ------------------------------------------------------------
                // BLUE
                // ------------------------------------------------------------
                case "A"://Achat
                case "B"://Buyer
                case "Buyer":
                case "Buy":
                case "1":
                case "Long":
                case "Ask":
                case "P":
                    if (pIsLight)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_Buyer_Light;
                    else
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_Buyer;
                    break;
                // ------------------------------------------------------------
                // RED
                // ------------------------------------------------------------
                case "V"://Vente
                case "S"://Seller
                case "Seller":
                case "Sell":
                case "2":
                case "Short":
                case "Bid":
                case "R":
                    if (pIsLight)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_Seller_Light;
                    else
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_Seller;
                    break;
            }

            return cssClass;
        }

        // EG 20160216 New
        public static EFSCssClass.CssClassEnum GetCssClassForQuoteSideValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;
            switch (pData)
            {
                case "Ask":
                    cssClass = EFSCssClass.CssClassEnum.dg_ask;
                    break;
                case "Bid":
                    cssClass = EFSCssClass.CssClassEnum.dg_bid;
                    break;
                case "Mid":
                    cssClass = EFSCssClass.CssClassEnum.dg_mid;
                    break;
                case "OfficialClose":
                    cssClass = EFSCssClass.CssClassEnum.dg_officialclose;
                    break;
                case "OfficialSettlement":
                    cssClass = EFSCssClass.CssClassEnum.dg_officialsettlement;
                    break;
            }
            return cssClass;
        }
        // EG 20160216 New
        public static EFSCssClass.CssClassEnum GetCssClassForQuoteTimingValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;
            switch (pData)
            {
                case "Close":
                    cssClass = EFSCssClass.CssClassEnum.dg_quotetimingclose;
                    break;
                case "Open":
                    cssClass = EFSCssClass.CssClassEnum.dg_quotetimingopen;
                    break;
            }
            return cssClass;
        }

        // EG 20160224 New Gestion CSS complex sur Cellule (REQUESTTYPE)
        // EG 20160304 New Gestion pour les consultations des LOG avec POSREQUESTTYPE
        public static void SetCssClassForLogPosRequestTypeValue(string pPrefixCssClass, Control pControl, string pVersion, string pData)
        {
            // REQUESTYPE-STATUS
            string[] _datas = pData.Split('-');
            string _data = _datas[0];
            string _status = _datas[1];
            string _level = _datas[2];

            WCToolTipPanel pnl = SetCssClassForPosRequestTypeValue(pPrefixCssClass, pControl, pVersion, _data);
            if (null != pnl)
                pnl.CssClass += "-" + _status + "-" + _level;
        }
        // EG 20160304 Upd Gestion pour les consultations des LOG avec POSREQUESTTYPE
        // EG 20171113 Upd
        public static WCToolTipPanel SetCssClassForPosRequestTypeValue(string pPrefixCssClass, Control pControl, string pVersion, string pData)
        {
            WCToolTipPanel pnl = null;

            string _data = pData;
            string _resData = Ressource.GetString(pData);
            if (StrFunc.IsFilled(_data))
            {
                Nullable<Cst.PosRequestTypeEnum> posRequestType = ReflectionTools.ConvertStringToEnumOrNullable<Cst.PosRequestTypeEnum>(_data);
                if (false == posRequestType.HasValue)
                {
                    if (Enum.IsDefined(typeof(Cst.PosRequestTypeEnum), _data))
                        posRequestType = (Cst.PosRequestTypeEnum)Enum.Parse(typeof(Cst.PosRequestTypeEnum), _data);
                    else
                        posRequestType = StringToEnum.ConvertToPosRequestTypeEnum(_data);

                    _data = ReflectionTools.GetXmlEnumAttributName(typeof(Cst.PosRequestTypeEnum), posRequestType.ToString());

                }
                if (posRequestType.HasValue || (pData == "RECLEARING"))
                {
                    if (pControl is Label)
                    {
                        pnl = new WCToolTipPanel();
                        pControl.Parent.Controls.Add(pnl);
                        pControl.Parent.Controls.Remove(pControl);
                        pnl.CssClass = pPrefixCssClass + "-" + (StrFunc.IsFilled(pVersion) ? pVersion : "h") + "-" + _data;
                        pnl.Attributes.Add("prt-data", _data);
                        pnl.Attributes.Add("prt-exdata", Ressource.GetString(posRequestType.HasValue ? posRequestType.Value.ToString() : "ReClearing", _resData));
                        if (pData == "RECLEARING")
                            pnl.Pty.TooltipContent = Ressource.GetString("Qtip_UNCLEARINGREMAIN");
                        else
                            pnl.Pty.TooltipContent = Ressource.GetString("Qtip_" + _data);
                    }
                }
            }
            return pnl;
        }

        // EG 20160216 New
        public static EFSCssClass.CssClassEnum GetCssClassForMoneyPositionEnumValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            switch (pData)
            {
                case "OTM":
                case "Out the money":
                    cssClass = EFSCssClass.CssClassEnum.dg_otm;
                    break;
                case "ITM":
                case "In the money":
                    cssClass = EFSCssClass.CssClassEnum.dg_itm;
                    break;
                case "ATM":
                case "At the money":
                    cssClass = EFSCssClass.CssClassEnum.dg_atm;
                    break;
                // CC/PL 20170307 [22916] 
                case "ATM_ITM":
                case "At the money (ITM)":
                    cssClass = EFSCssClass.CssClassEnum.dg_atm_itm;
                    break;
                case "ATM_OTM":
                case "At the money (OTM)":
                    cssClass = EFSCssClass.CssClassEnum.dg_atm_otm;
                    break;
                case "N/A":
                    cssClass = EFSCssClass.CssClassEnum.dg_na;
                    break;
            }
            return cssClass;
        }
        /// <summary>
        /// Donne La class qui colorise la cellule en rouge ou en vert en fonction de la valeur {pData} 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20120605 place GetCssClassForSide  
        public static EFSCssClass.CssClassEnum GetCssClassForGreenRed(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.DataGrid_PayerReceiver;

            switch (pData)
            {
                // ------------------------------------------------------------
                // RED
                // ------------------------------------------------------------
                case "A"://Achat
                case "B"://Buyer
                case "Buyer":
                case "Buy":
                case "1":
                case "Long":
                case "Ask":
                case "P":
                case "Pay":
                case "Payer":
                case "Debit":
                case "DEBIT":
                case "ERROR":
                case "Out":
                case "Override":
                    cssClass = EFSCssClass.CssClassEnum.DataGrid_Payer;
                    break;
                // ------------------------------------------------------------
                // GREEN
                // ------------------------------------------------------------
                case "V"://Vente
                case "S"://Seller
                case "Seller":
                case "Sell":
                case "2":
                case "Short":
                case "Bid":
                case "R":
                case "Receive":
                case "Receiver":
                case "Credit":
                case "CREDIT":
                case "SUCCESS":
                case "In":
                case "Standard":
                case "Beneficiary":
                    cssClass = EFSCssClass.CssClassEnum.DataGrid_Receiver;
                    break;
                default:
                    //Warning: A REVOIR: Utilisation en DUR pour les écritures comptables (PL)
                    //PL DISPLAY_RUPTURE_TOTAL Code ajouté pour géré en dur une rupture avec sous-total pour les écritures comptables
                    if (pData.IndexOf("Unbalanced") >= 0)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_ErrorBackColor;
                    else if (pData.IndexOf("Well-balanced") >= 0)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_SuccessBackColor;
                    //Warning: A REVOIR: Utilisation en DUR pour les logs (CC)
                    else if (pData.IndexOf("INFO") >= 0)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_InfoBackColor;
                    else if ((pData.IndexOf("NA") >= 0) || (pData.IndexOf("N/A") >= 0))
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_NaBackColor;
                    else if (pData.IndexOf("WARNING") >= 0)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_WarningBackColor;
                    break;
            }

            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForQuantityValue(string pData, bool pIsLight)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.DataGrid_BuyerSeller;

            if (StrFunc.IsFilled(pData))
            {
                decimal decdata = Convert.ToDecimal(pData);
                if (decdata > 0)
                    cssClass = GetCssClassForTradeSideValue("1", pIsLight);
                else if (decdata < 0)
                    cssClass = GetCssClassForTradeSideValue("2", pIsLight);
            }
            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForAmountValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.DataGrid_PayerReceiver;

            if (StrFunc.IsFilled(pData))
            {
                decimal decdata = Convert.ToDecimal(pData);
                if (decdata > 0)
                    cssClass = EFSCssClass.CssClassEnum.DataGrid_Receiver;
                else if (decdata < 0)
                    cssClass = EFSCssClass.CssClassEnum.DataGrid_Payer;
            }
            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForReverseAmountValue(string pData)
        {
            if (StrFunc.IsFilled(pData))
                pData = (Convert.ToDecimal(pData) * (-1)).ToString();

            return GetCssClassForAmountValue(pData);
        }
        public static Color GetColorForStatusValue(string pData)
        {
            //Warning: see aso GetCssClassForStatusValue()
            Color color = Color.Empty;

            if (StrFunc.IsFilled(pData))
            {
                pData = pData.Replace(Cst.HTMLBold, string.Empty);
                pData = pData.Replace(Cst.HTMLEndBold, string.Empty);

                if (ProcessStateTools.IsStatusSuccess(pData))
                    color = CstCSSColor.Convert(CstCSSColor.green);
                else if (ProcessStateTools.IsStatusError(pData))
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (ProcessStateTools.IsStatusWarning(pData))
                    color = CstCSSColor.Convert(CstCSSColor.orange);
                else if (ProcessStateTools.IsStatusNone(pData))
                    color = CstCSSColor.Convert(CstCSSColor.blue);
                else if (pData == LevelStatusTools.StatusEnum.INFO.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.blue);
                else if (pData == Cst.ErrLevel.FAILURE.ToString() || pData == Cst.ErrLevel.BREAK.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData == Cst.ErrLevel.TIMEOUT.ToString() || pData == Cst.ErrLevel.DEADLOCK.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData == Cst.ErrLevel.UNDEFINED.ToString() || pData == Cst.ErrLevel.ABORTED.ToString() || pData == Cst.ErrLevel.LOCKUNSUCCESSFUL.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData == Cst.ErrLevel.DATAUNMATCH.ToString() || pData == Cst.ErrLevel.DATADISABLED.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.gray);
                else if (ProcessStateTools.IsStatusProgress(pData))
                    color = CstCSSColor.Convert(CstCSSColor.violet);
                else if (ProcessStateTools.IsStatusPending(pData))
                    color = CstCSSColor.Convert(CstCSSColor.marron);
                else if (pData.StartsWith("ERROR") || pData.StartsWith("ALERT"))
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData.StartsWith("WARNING"))
                    color = CstCSSColor.Convert(CstCSSColor.orange);
                else if (pData.StartsWith("MATCH"))
                    color = CstCSSColor.Convert(CstCSSColor.green);
                else if (pData.StartsWith("UNMATCH"))
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData.EndsWith("NOTFOUND"))
                    color = CstCSSColor.Convert(CstCSSColor.yellow);
                else if (ProcessStateTools.IsStatusUnknown(pData))
                    color = CstCSSColor.Convert(CstCSSColor.gray);
            }
            return color;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForStatusValue(string pData)
        {
            //Warning: see aso GetColorForStatusValue()
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            if (StrFunc.IsFilled(pData))
            {
                pData = pData.Replace(Cst.HTMLBold, string.Empty);
                pData = pData.Replace(Cst.HTMLEndBold, string.Empty);

                if (ProcessStateTools.IsStatusSuccess(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusSuccess;
                else if (ProcessStateTools.IsStatusError(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusError;
                else if (ProcessStateTools.IsStatusWarning(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusSuccessWarning;
                else if (ProcessStateTools.IsStatusNone(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusInfo;

                else if (pData == LevelStatusTools.StatusEnum.INFO.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusInfo;
                else if (pData == Cst.ErrLevel.FAILURE.ToString() || pData == Cst.ErrLevel.BREAK.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusError;
                else if (pData == Cst.ErrLevel.TIMEOUT.ToString() || pData == Cst.ErrLevel.DEADLOCK.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusError;
                else if (pData == Cst.ErrLevel.UNDEFINED.ToString() || pData == Cst.ErrLevel.ABORTED.ToString() || pData == Cst.ErrLevel.LOCKUNSUCCESSFUL.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusUnXxxx;
                else if (pData == Cst.ErrLevel.DATAUNMATCH.ToString() || pData == Cst.ErrLevel.DATADISABLED.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusOthers;

                else if (ProcessStateTools.IsStatusUnknown(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusNA;
                else if (ProcessStateTools.IsStatusProgress(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusProgress;
                else if (ProcessStateTools.IsStatusPending(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusPending;

                else if (pData.StartsWith("ERROR"))
                    cssClass = EFSCssClass.CssClassEnum.StatusError;
                else if (pData.StartsWith("WARNING"))
                    cssClass = EFSCssClass.CssClassEnum.StatusSuccessWarning;
                else if (pData.StartsWith("MATCH"))
                    cssClass = EFSCssClass.CssClassEnum.StatusSuccess;
                else if (pData.StartsWith("UNMATCH"))
                    cssClass = EFSCssClass.CssClassEnum.StatusUnXxxx;

                else if (pData.EndsWith("NOTFOUND"))
                    //Cst.ErrLevel.DATANOTFOUND, Cst.ErrLevel.FILENOTFOUND, Cst.ErrLevel.FOLDERNOTFOUND
                    cssClass = EFSCssClass.CssClassEnum.StatusNotfound;
            }

            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForLevelValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            if (LevelStatusTools.IsLevelAlert(pData))
                cssClass = EFSCssClass.CssClassEnum.LevelAlertMsg;
            else if (LevelStatusTools.IsLevelInfo(pData))
                cssClass = EFSCssClass.CssClassEnum.LevelInfoMsg;
            else if (LevelStatusTools.IsLevelWarning(pData))
                cssClass = EFSCssClass.CssClassEnum.LevelWarningMsg;
            else if (LevelStatusTools.IsLevelUnknown(pData))
                cssClass = EFSCssClass.CssClassEnum.LevelOtherMsg;

            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForReadyStateValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            if (ProcessStateTools.IsReadyStateActive(pData))
                cssClass = EFSCssClass.CssClassEnum.ReadyStateActiveMsg;
            else if (ProcessStateTools.IsReadyStateRequested(pData))
                cssClass = EFSCssClass.CssClassEnum.ReadyStateRequestedMsg;
            else if (ProcessStateTools.IsReadyStateTerminated(pData))
                cssClass = EFSCssClass.CssClassEnum.ReadyStateTerminatedMsg;
            return cssClass;
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="pModel"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static EFSCssClass.CssClassEnum GetCssClassForModel(string pModel, string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            if ("status" == pModel)
                cssClass = RepositoryTools.GetCssClassForStatusValue(pData);
            else if ("level" == pModel)
                cssClass = RepositoryTools.GetCssClassForLevelValue(pData);
            else if ("readystate" == pModel)
                cssClass = RepositoryTools.GetCssClassForReadyStateValue(pData);
            else if ("side" == pModel)
                cssClass = RepositoryTools.GetCssClassForGreenRed(pData);
            else if ("tradeside" == pModel)
                cssClass = RepositoryTools.GetCssClassForTradeSideValue(pData, false);
            else if ("tradesideLong" == pModel)
                cssClass = RepositoryTools.GetCssClassForTradeSideValue("1", false);
            else if ("tradesideShort" == pModel)
                cssClass = RepositoryTools.GetCssClassForTradeSideValue("2", false);
            else if ("tradeside_light" == pModel)
                cssClass = RepositoryTools.GetCssClassForTradeSideValue(pData, true);
            else if ("tradesideLong_light" == pModel)
                cssClass = RepositoryTools.GetCssClassForTradeSideValue("1", true);
            else if ("tradesideShort_light" == pModel)
                cssClass = RepositoryTools.GetCssClassForTradeSideValue("2", true);
            else if ("quantity" == pModel)
                cssClass = RepositoryTools.GetCssClassForQuantityValue(pData, false);
            else if ("quantity_light" == pModel)
                cssClass = RepositoryTools.GetCssClassForQuantityValue(pData, true);
            else if ("amount" == pModel)
                cssClass = RepositoryTools.GetCssClassForAmountValue(pData);
            else if ("reverseamount" == pModel)
                cssClass = RepositoryTools.GetCssClassForReverseAmountValue(pData);
            else if ("moneyposition" == pModel)
                cssClass = RepositoryTools.GetCssClassForMoneyPositionEnumValue(pData);
            else if ("quoteside" == pModel)
                cssClass = RepositoryTools.GetCssClassForQuoteSideValue(pData);
            else if ("quotetiming" == pModel)
                cssClass = RepositoryTools.GetCssClassForQuoteTimingValue(pData);
            else if ("greenRed" == pModel)
            {
                //FI 20120605 add greenRed
                cssClass = RepositoryTools.GetCssClassForGreenRed(pData);
            }
            return cssClass;
        }
        // EG 20160224 New Gestion CSS complex sur Cellule (REQUESTTYPE)
        public static void SetCssClassForComplexModel(string pComplexModel, Control pControl, string pVersion, string pData)
        {
            switch (pComplexModel)
            {
                case "posRequestType":
                    RepositoryTools.SetCssClassForPosRequestTypeValue("prt", pControl, pVersion, pData);
                    break;
                case "logPosRequestType":
                    RepositoryTools.SetCssClassForLogPosRequestTypeValue("log-prt", pControl, pVersion, pData);
                    break;
            }
        }
        public static string GetCssClassFromDataValue(ReferentialColumnStyleBase pStyle, string pData)
        {
            string cssClass = string.Empty;
            try
            {
                bool isVerify = false;
                //
                if (pStyle.WhenSpecified)
                {
                    foreach (ReferentialColumnStyleWhen when in pStyle.When)
                    {
                        if (StrFunc.IsFilled(when.test))
                        {
                            string[] test = when.test.Split(";".ToCharArray());
                            string comparator = test[0].Substring(0, 1);
                            //
                            for (int pass = 1; pass <= 2; pass++)
                            {
                                if (pass == 2)
                                    comparator = test[0].Substring(0, 2);
                                //
                                if ((comparator == "=") || (comparator == "!="))
                                {
                                    for (int i = 0; i < test.Length; i++)
                                    {
                                        string testItem = test[i];
                                        if (i == 0)
                                            testItem = test[i].Substring(comparator.Length);
                                        //
                                        isVerify = ((StrFunc.IsEmpty(pData) && StrFunc.IsEmpty(testItem)));
                                        //
                                        if (false == isVerify && StrFunc.IsFilled(pData))
                                            isVerify = (pData.CompareTo(testItem) == 0);
                                        //
                                        if (isVerify)
                                            break;
                                    }
                                    //
                                    if (comparator == "!=")
                                        isVerify = (!isVerify);
                                }
                            }
                            //
                            if (isVerify)
                            {
                                cssClass = when.Value;
                                break;
                            }
                        }
                    }
                }
                //
                if (false == isVerify && pStyle.OtherwiseSpecified)
                    cssClass = pStyle.Otherwise.Value;
            }
            catch { }
            return cssClass;
        }
        #endregion

        #region FormatLongData
        public static string FormatLongData(string pData)
        {
            return FormatLongData(pData, 0);
        }
        public static string FormatLongData(string pData, int pMaxLength)
        {
            pData = pData.Replace(Cst.CrLf, Cst.HTMLBreakLine);
            pData = pData.Replace(Cst.Lf, Cst.HTMLBreakLine);
            pData = pData.Replace(Cst.Tab, Cst.HTMLSpace4);

            if ((pMaxLength > 0) && (pData.Length > pMaxLength))
            {
                pData = pData.Substring(0, pMaxLength) + " ...";
            }

            if (pData.StartsWith(@"[Code Return:"))
            {
                int pos = pData.IndexOf(@"]");
                if (pos > 0)
                {
                    string code = pData.Substring(13, pos - 13).Trim();
                    EFSCssClass.CssClassEnum cssClass = GetCssClassForStatusValue(code);
                    if (!EFSCssClass.IsUnknown(cssClass))
                        pData = string.Format(@"[Code Return: <span class=""{0}""><b>{1}</b></span>]{2}",
                            cssClass.ToString(), code, pData.Substring(pos + 1));
                }
            }
            else if (pData.StartsWith(@"Result:"))
            {
                string code = pData.Substring(7).Trim();
                int pos = pData.IndexOf(Cst.HTMLBreakLine);
                if (pos > 0)
                    code = pData.Substring(7, pos - 7).Trim();
                EFSCssClass.CssClassEnum cssClass = GetCssClassForStatusValue(code);
                if (!EFSCssClass.IsUnknown(cssClass))
                    pData = string.Format(@"Result: <span class=""{0}""><b>{1}</b></span>{2}",
                        cssClass.ToString(), code, (pos >= 0) ? pData.Substring(pos) : string.Empty);
            }
            else if (pData.StartsWith(@"<b>Warning"))
            {
                string data = pData;
                int pos = pData.IndexOf(Cst.HTMLBreakLine);
                if (pos > 0)
                    data = pData.Substring(0, pos).Trim();
                pData = string.Format(@"<span class=""{0}"">{1}</span>{2}",
                    EFSCssClass.CssClassEnum.StatusSuccessWarning.ToString(), data, (pos >= 0) ? pData.Substring(pos) : string.Empty);
            }
            else if (pData.StartsWith(@"<b>Data imported successfully") || pData.StartsWith(@"<b>Data matched successfully"))
            {
                string data = pData;
                int pos = pData.IndexOf(Cst.HTMLBreakLine);
                if (pos > 0)
                    data = pData.Substring(0, pos).Trim();
                pData = string.Format(@"<span class=""{0}"">{1}</span>{2}",
                    EFSCssClass.CssClassEnum.StatusSuccess.ToString(), data, (pos >= 0) ? pData.Substring(pos) : string.Empty);
            }
            return pData;
        }
        #endregion FormatLongData
        #region FoundValue
        public static bool FoundValue(bool pIsHTMLSelect, DropDownList pDdlRef, HtmlSelect pHtmlddlRef, string pSelectedValue,
            ReferentialColumn pRc, ref string opTextSelectedValue, ref string opWarningMsg)
        {

            bool ret = false;
            bool isHideWarning = false;
            if (pIsHTMLSelect)
                ret = ControlsTools.DDLSelectByValue(pHtmlddlRef, pSelectedValue);
            else
                ret = ControlsTools.DDLSelectByValue(pDdlRef, pSelectedValue);

            if ((!ret) && (StrFunc.IsFilled(pSelectedValue)))
            {
                opTextSelectedValue = pSelectedValue;
                if (StrFunc.IsEmpty(opWarningMsg))
                {
                    opWarningMsg = Ressource.GetString("Msg_DataUnavailableOrRemoved", "Warning: Data disabled or removed !") + Cst.CrLf;
                    if (pRc.Relation[0].TableName == Cst.OTCml_TBL.ACTOR.ToString())
                    {
                        if (pSelectedValue == 0.ToString())
                        {
                            //PL 20110819 Newness
                            //On est ici sur le cas particulier d'un acteur non renseigné (ex. SYSTEM_L)
                            isHideWarning = true;
                        }
                        else
                        {
                            //On est ici sur un acteur "indisponible" dans la DDL
                            opWarningMsg = Ressource.GetString("Msg_ActorUnavailableOrRemoved", "Warning: Actor disabled or removed !") + Cst.CrLf;
                            //Recherche de l'identifier/displayname pour affichage
                            try
                            {
                                SQL_Actor sql_Actor = new SQL_Actor(SessionTools.CS, Convert.ToInt32(pSelectedValue));
                                if (sql_Actor.IsLoaded)
                                {
                                    opTextSelectedValue = sql_Actor.Identifier + " - " + sql_Actor.DisplayName;
                                    if (sql_Actor.IsEnabled)
                                        opWarningMsg = Ressource.GetString("Msg_ActorHasNotRole", "Warning: Actor has not the necessary role !") + Cst.CrLf;
                                }
                                else
                                {
                                    opWarningMsg = Ressource.GetString("Msg_ActorRemoved", "Warning: Actor removed !") + Cst.CrLf;
                                }
                                sql_Actor = null;
                            }
                            catch { }
                        }
                    }
                }
                if (isHideWarning)
                {
                    opWarningMsg = string.Empty;
                    opTextSelectedValue = Ressource.GetString("Msg_UnavailableOrRemoved", "[unavailable]");
                }
                else
                {
                    opWarningMsg += Cst.CrLf + pRc.Label + ": " + opTextSelectedValue;
                    opTextSelectedValue += " " + Ressource.GetString("Msg_UnavailableOrRemoved", "[unavailable]");
                }

                ListItem liUnavailable = new ListItem(opTextSelectedValue, pSelectedValue);
                liUnavailable.Attributes.Add("style", "color:#FFFFFF;background-color:#AE0303");
                if (pIsHTMLSelect)
                {
                    pHtmlddlRef.Items.Add(liUnavailable);
                    ret = ControlsTools.DDLSelectByValue(pHtmlddlRef, pSelectedValue);
                }
                else
                {
                    pDdlRef.Items.Add(liUnavailable);
                    ret = ControlsTools.DDLSelectByValue(pDdlRef, pSelectedValue);
                }
            }
            return ret;

        }
        #endregion FoundValue

        #region GetTypeAheadKeyValues
        private static Pair<string, string> GetTypeAheadKeyValues(ReferentialColumn pRc)
        {
            Pair<string, string> typeAheadKeyValues = null;
            if (pRc.TypeAheadEnabled)
                typeAheadKeyValues = new Pair<string, string>(pRc.TypeAhead.Table, pRc.TypeAhead.Values);
            return typeAheadKeyValues;
        }
        #endregion GetTypeAheadKeyValues

        #region private DispatchListRetrieval
        private static void DispatchListRetrieval(ref ReferentialColumn opRrc, string pListRetrieval)
        {
            Cst.ListRetrievalEnum listRetrievalType = Cst.ListRetrievalEnum.PREDEF;
            string listRetrievalData = string.Empty;
            //
            if (ControlsTools.DecomposeListRetrieval(pListRetrieval, out listRetrievalType, out listRetrievalData))
            {
                opRrc.ListRetrievalType = listRetrievalType.ToString().ToLower();
                //
                switch (listRetrievalType)
                {
                    case Cst.ListRetrievalEnum.PREDEF:
                        //PL 20101020 Debug --> Add new ReferentialsReferentialColumnRelationDDLType()
                        if (opRrc.Relation[0].DDLType == null)
                            opRrc.Relation[0].DDLType.Value = listRetrievalData;
                        break;
                    default:
                        opRrc.ListRetrievalData = listRetrievalData;
                        break;
                }
            }
        }
        #endregion
        #region private SetJavascriptToColumnControl
        /// <creation>
        ///     <version>1.0.?</version><date>2004????</date><author>OD</author>
        /// </creation>
        /// <revision>
        ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
        ///     <comment>
        ///     [To insert here a description of the made modifications ]
        ///     </comment>
        /// </revision>
        /// <summary>
        /// Ajoute du javascript au control d'une colonne en fonction de ce qui est paramétré dans l'element <Help> et/ou <JavaScript> de cette colonne
        /// </summary>
        /// <param name="pPage">page appelante</param>
        /// <param name="pCtrlNew">control à implémenter</param>
        /// <param name="pReferential">classe referentiel</param>
        /// <param name="pRrc">colonne à traiter</param>
        private static void SetJavascriptToColumnControl(PageBase pPage, Control pCtrlNew, Referential pReferential, ReferentialColumn pRc)
        {
            if (pRc.ExistsHelp)
            {
                XMLJavaScript.setHelp(pCtrlNew, pRc.Help);
                //20080825 FI Les script sont dans la library PageReferential.js  
                //JavaScript.AddScript(pPage, "EFS_StatusHelp");
            }

            if (pRc.ExistsJavaScript)
            {
                JavaScript.JavaScriptScript jss;
                for (int i = 0; i <= pRc.JavaScript.Script.GetLength(0) - 1; i++)
                {
                    jss = pRc.JavaScript.Script[i];

                    XMLJavaScript.Interpret(jss);

                    //dans le cas de EnabledDisabled on obtient l'ID du(des) control(s) dans la page
                    if (JavaScript.IsScriptTypeEnabledDisabled(jss.name)
                        || JavaScript.IsScriptTypeEFS_Copy(jss.name)
                        || JavaScript.IsScriptTypeEFS_Reset(jss.name)
                        || JavaScript.IsScriptTypeEFS_Set(jss.name)
                        || JavaScript.IsScriptTypeEFS_ApplyOffset(jss.name)
                        )
                    {
                        for (int ni = 0; ni < jss.aControl.Length; ni++)
                        {
                            ReferentialColumn rccolID = new ReferentialColumn();
                            rccolID = pReferential[jss.aControl[ni]];
                            // when the current control is not defined inside of the XML referential signature (no column node),
                            // then une exception is thrown on rrccolID, because of that is null,
                            // also when the control is just there but child of one main referential controls.
                            // Adding condition rrccolID != null.
                            if (jss.aControl[ni] != "this" && rccolID != null)
                            {
                                string prefix = RepositoryTools.IsDataForDDL(rccolID) ? Cst.DDL : null;
                                jss.aControl[ni] = RepositoryTools.GetID(rccolID, prefix);
                            }
                        }
                        if (JavaScript.IsScriptTypeEnabledDisabled(jss.name))
                            XMLJavaScript.InterpretEnabledDisabled(jss);
                        else if (JavaScript.IsScriptTypeEFS_Copy(jss.name))
                            XMLJavaScript.InterpretCopy(jss);
                        else if (JavaScript.IsScriptTypeEFS_Reset(jss.name) || JavaScript.IsScriptTypeEFS_Set(jss.name))
                            XMLJavaScript.InterpretSet(jss);
                        else if (JavaScript.IsScriptTypeEFS_ApplyOffset(jss.name))
                            XMLJavaScript.InterpretApplyOffset(jss);
                    }

                    //on ajoute le script sur l'attribut
                    XMLJavaScript.AddScriptToCtrl(pCtrlNew, jss);
                }
            }
        }
        #endregion SetJavascriptToColumnControl

        /// <summary>
        /// Verifie si la colonne de l'index passé en arg est le premier d'une serie de controls liés (1 seul libellé)
        /// </summary>
        /// <param name="pIndex">index de la colonne à verifier</param>
        /// <param name="pReferential">REF classe referential</param>
        /// <returns>bool</returns>
        public static bool IsFirstControlLinked(int pIndex, ref Referential pReferential)
        {
            bool ret = false;
            if ((pIndex + 1 < pReferential.Column.Length) &&
                (StrFunc.IsEmpty(pReferential.Column[pIndex + 1].Ressource)) &&
                (StrFunc.IsFilled(pReferential.Column[pIndex].Ressource)))
            {
                pReferential.NbControlLinked = 2;
                int index = pIndex + 1;
                while (index + 1 < pReferential.Column.Length && StrFunc.IsEmpty(pReferential.Column[index + 1].Ressource))
                {
                    pReferential.NbControlLinked++;
                    index++;
                }
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Verifie si la colonne de l'index passé en arg est dans une serie de controls liés (1 seul libellé) sans etre ni le premier ni le dernier de cette serie
        /// </summary>
        /// <param name="pIndex">index de la colonne à verifier</param>
        /// <param name="pReferential">REF classe referential</param>
        /// <returns>bool</returns>
        public static bool IsMiddleControlLinked(int pIndex, ref Referential pReferential)
        {
            if ((pIndex - 1 >= 0) && (pIndex + 1 < pReferential.Column.Length))
                return (IsFirstControlLinked(pIndex - 1, ref pReferential) &&
                    StrFunc.IsEmpty(pReferential.Column[pIndex + 1].Ressource) &&
                    StrFunc.IsEmpty(pReferential.Column[pIndex].Ressource));
            else
                return false;
        }

        /// <summary>
        /// Verifie si la colonne de l'index passé en arg est la dernière d'une serie de controls liés (1 seul libellé)
        /// </summary>
        /// <param name="pIndex">index de la colonne à verifier</param>
        /// <param name="pReferential">classe referential</param>
        /// <returns>bool</returns>
        public static bool IsLastControlLinked(int pIndex, Referential pReferential)
        {
            if (pIndex + 1 < pReferential.Column.Length)
                return (StrFunc.IsEmpty(pReferential.Column[pIndex].Ressource) && StrFunc.IsFilled(pReferential.Column[pIndex + 1].Ressource));
            else
                return (StrFunc.IsEmpty(pReferential.Column[pIndex].Ressource));
        }

        /// <summary>
        /// Renvoie true si la colonne spécifié est de type DDL
        /// </summary>
        /// <param name="pRrc">la colonne à verifier</param>
        /// <returns>bool</returns>
        public static bool IsDataForDDL(ReferentialColumn pRc)
        {

            bool isDDL = pRc.ExistsRelation && (false == pRc.AutoCompleteEnabled);
            if (false == isDDL)
            {
                if (pRc.ExistsDDLType)
                {
                    if (pRc.Relation[0].DDLType == null)
                        isDDL = true;
                    else
                        isDDL = (false == Cst.IsDDLTypeSumAmount(pRc.Relation[0].DDLType.Value)) && (false == Cst.IsDDLTypePeriodMulti(pRc.Relation[0].DDLType.Value));
                }
            }
            if (false == isDDL)
            {
                isDDL = IsDataForDDLParticular(pRc);
            }
            return isDDL;
        }

        /// <creation>
        ///     <version>1.0.?</version><date>????????</date><author>??</author>
        /// </creation>
        /// <revision>
        ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
        ///     <comment>
        ///     [To insert here a description of the made modifications ]
        ///     </comment>
        /// </revision>        
        /// <summary>
        /// Renvoie true si la colonne spécifié est de type DDL cas particulier
        /// </summary>
        /// <param name="pRrc">la colonne à verifier</param>
        /// <returns>bool</returns>
        public static bool IsDataForDDLParticular(ReferentialColumn pRc)
        {
            bool isDDL = false;
#warning 20030101 PL (Not Urgent) Codage en dur à revoir
            isDDL |= (pRc.ColumnName == "MTMMETHOD");
            isDDL |= (pRc.ColumnName == "ACCRUEDINTMETHOD");
            isDDL |= (pRc.ColumnName == "ACCRUEDINTPERIOD");
            isDDL |= (pRc.ColumnName == "LINEARDEPPERIOD");
            isDDL |= (pRc.ColumnName == "GPRODUCT");
            isDDL |= (pRc.ColumnName == "FAMILY");
            isDDL |= (pRc.ColumnName == "CLASS");
            return isDDL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc"></param>
        /// <param name="pDDLRef"></param>
        /// <param name="pValueForeignKeyField"></param>
        /// <param name="pCurrentNonUpdatableValue"></param>
        /// <param name="pIsNewRecord"></param>
        public static void LoadDDL(PageBase pPage, Referential pReferential, ReferentialColumn pRc, DropDownList pDDLRef,
            string pValueForeignKeyField, string pCurrentNonUpdatableValue, bool pIsNewRecord)
        {
            string source = pPage.CS;

            if ((pRc.Relation[0].DDLType != null) && (pRc.Relation[0].DDLType.Value != null))
            {
                bool isResource = (pRc.IsResourceSpecified && pRc.IsResource.IsResource);
                bool isWithEmpy = !pRc.IsMandatory;
                bool isDynamicArguments = pRc.Relation[0].DDLType.Value.Contains(Cst.DA_START);
                string misc = ((pRc.MiscSpecified) ? pRc.Misc : string.Empty);

                //
                if (StrFunc.IsFilled(pValueForeignKeyField))
                {
                    if (misc.Length > 0)
                        misc += ";";
                    misc += "FK:" + pValueForeignKeyField;
                }
                if (pRc.Relation[0].DDLType.miscSpecified)
                {
                    if (misc.Length > 0)
                        misc += ";";
                    misc += pRc.Relation[0].DDLType.misc;
                }
                //
                string objectsPath = string.Empty;
                string ddlType = pRc.Relation[0].DDLType.Value;
                if (isDynamicArguments)
                    ddlType = pReferential.ReplaceDynamicArgument(source, ddlType);

                //
                if (Cst.IsDDLTypeWebControlType(ddlType))
                    objectsPath = pPage.Server.MapPath(Cst.CustomTradePath + @"\Objects\");
                //

                ControlsTools.DDLLoad_FromListRetrieval(pPage, pDDLRef, source, Cst.ListRetrievalEnum.PREDEF, ddlType,
                    isWithEmpy, isResource, misc, objectsPath);
            }
            else if (StrFunc.IsFilled(pRc.ListRetrievalType))
            {
                bool isResource = (pRc.IsResourceSpecified && pRc.IsResource.IsResource);
                bool isWithEmpy = !pRc.IsMandatory;
                string misc = ((pRc.MiscSpecified) ? pRc.Misc : string.Empty);
                Cst.ListRetrievalEnum listRetrieval = (Cst.ListRetrievalEnum)Enum.Parse(typeof(Cst.ListRetrievalEnum), pRc.ListRetrievalType, true);
                //
                ControlsTools.DDLLoad_FromListRetrieval(pDDLRef, source, listRetrieval, pRc.ListRetrievalData,
                    isWithEmpy, isResource, misc);
            }
            //-----------------------------------------------------------------------
            //20081118 A priori inutile maintenat ...
            else if (pRc.ColumnName.StartsWith("IDSTENVIRONMENT"))
                ControlsTools.DDLLoad_StatusEnvironment(pPage.CS, pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName.StartsWith("IDSTACTIVATION"))
                ControlsTools.DDLLoad_StatusActivation(pPage.CS, pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName.StartsWith("IDSTPRIORITY"))
                ControlsTools.DDLLoad_StatusPriority(pPage.CS, pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName.StartsWith("IDSTCHECK"))
                ControlsTools.DDLLoad_StatusCheck(pPage.CS, pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName.StartsWith("IDSTMATCH"))
                ControlsTools.DDLLoad_StatusMatch(pPage.CS, pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName.StartsWith("IDSTPROCESS"))
                ControlsTools.DDLLoad_StatusProcess(pPage.CS, pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName.StartsWith("BUYER_SELLER"))
                ControlsTools.DDLLoad_BuyerSellerType(pDDLRef, !pRc.IsMandatory);
            //else if (pRrc.ColumnName.EndsWith("POS"))
            //    ControlsTools.DDLLoad_PositionType(pDDLRef, !pRrc.IsMandatory);
            //-----------------------------------------------------------------------
            // EG 20160308 Migration vs2013
            //#warning 20030101 PL (Not Urgent) Codage en dur à revoir
            //-----------------------------------------------------------------------
            else if (pRc.ColumnName == "MTMMETHOD")
                ControlsTools.DDLLoad_FxMTMMethod(pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName == "ACCRUEDINTMETHOD")
                ControlsTools.DDLLoad_AccruedInterest(pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName == "ACCRUEDINTPERIOD")
                ControlsTools.DDLLoad_AccruedInterestPeriod(pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName == "LINEARDEPPERIOD")
                ControlsTools.DDLLoad_LinearDepreciationPeriod(pDDLRef, !pRc.IsMandatory);
            //else if (pRrc.ColumnName=="SOURCE" || pRrc.ColumnName=="DEFINED")
            else if (pRc.ColumnName == "DEFINED")
                ControlsTools.DDLLoad_Source(pDDLRef);
            else if (pRc.ColumnName == "GPRODUCT")
                ControlsTools.DDLLoad_ProductGProduct(pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName == "FAMILY")
                ControlsTools.DDLLoad_ProductFamily(pDDLRef, !pRc.IsMandatory);
            else if (pRc.ColumnName == "CLASS")
                ControlsTools.DDLLoad_ProductClass(pDDLRef, !pRc.IsMandatory);
            //-----------------------------------------------------------------------

            else
            {
                #region Relation Loading
                string SQLWhere = string.Empty;
                System.Data.IDbDataParameter sqlParam = null;
                //20040317 PL if (valueForeignKeyField != null)

                if ((pRc.IsForeignKeyField && StrFunc.IsFilled(pValueForeignKeyField)) || (StrFunc.IsFilled(pCurrentNonUpdatableValue)))
                {
                    //The zone is disabled or has FK defined, loading only data for display
                    SQLWhere = SQLCst.TBLMAIN + "." + pRc.Relation[0].ColumnRelation[0].ColumnName + "=" + DataHelper.GetVarPrefix(source) + "PARAM";

                    if (TypeData.IsTypeInt(pRc.DataType.value))
                    {
                        // EG 20150920 [21314] Int (int32) to Long (Int64) 
                        sqlParam = new EFSParameter(source, "PARAM", DbType.Int64).DataParameter;
                    }
                    else if (TypeData.IsTypeBool(pRc.DataType.value))
                        sqlParam = new EFSParameter(source, "PARAM", DbType.Boolean).DataParameter;
                    else if (TypeData.IsTypeDec(pRc.DataType.value))
                        sqlParam = new EFSParameter(source, "PARAM", DbType.Decimal).DataParameter;
                    else if (TypeData.IsTypeDateOrDateTime(pRc.DataType.value))
                        sqlParam = new EFSParameter(source, "PARAM", DbType.DateTime).DataParameter;
                    else
                        sqlParam = new EFSParameter(source, "PARAM", DbType.AnsiString, 64).DataParameter;

                    //if CurrentNonUpdatableValue is not empty: use it instead of ValueForeignKeyField
                    if (StrFunc.IsEmpty(pCurrentNonUpdatableValue))
                        sqlParam.Value = pValueForeignKeyField;
                    else
                        sqlParam.Value = pCurrentNonUpdatableValue;
                }

                string aliasColumnSelect;
                bool isColor = pRc.Relation[0].iscolor;
                string SQLQuery = GetDDLSQLSelect(pPage, pReferential, pRc, SQLWhere, false, isColor, pIsNewRecord, pValueForeignKeyField,
                    out aliasColumnSelect);
                bool isResource = (pRc.IsResourceSpecified && pRc.IsResource.IsResource);
                bool isSort = true;
                //PL 20141017 pPage
                ControlsTools.DDLLoad(pPage, pDDLRef, aliasColumnSelect, pRc.Relation[0].ColumnRelation[0].ColumnName, source, SQLQuery,
                    !pRc.IsMandatory, isSort, isResource, null, sqlParam);
                #endregion
            }
        }

        /// <summary>
        /// Renvoie la requete SQL pour un DDL de type <Relation>
        /// </summary>
        /// <param name="pRrc">colonne concernée</param>
        /// <param name="pSQLWhere">condition additionnelle si necessaire</param>
        /// <param name="pIsScalar"></param>
        /// <param name="pValueForeignKeyField">valeur pour la foreignKey</param>
        /// <param name="opAliasColumnSelect">OUT nom d'alias de la colonne dans la requete SQL générée</param>
        /// <returns></returns>
        public static string GetDDLSQLSelect(PageBase pPage,
            Referential pReferential, ReferentialColumn pRc,
            string pSQLWhere, bool pIsScalar, bool pIsColor, bool pIsNewRecord,
            string pValueForeignKeyField, out string opAliasColumnSelect)
        {
            //PL 20100211 Add tableNameForDDL
            string tableNameForDDL = pRc.Relation[0].TableNameForDDLSpecified ? pRc.Relation[0].TableNameForDDL : pRc.Relation[0].TableName;
            string sqlQuery;
            SQLWhere sqlWhere = new SQLWhere(pSQLWhere);

            if (pIsScalar)
            {
                sqlQuery = SQLCst.SELECT + " 1" + Cst.CrLf;
                opAliasColumnSelect = string.Empty;
            }
            else
            {
                opAliasColumnSelect = pRc.Relation[0].ColumnSelect[0].ColumnName;

                // SQLCst.SELECT --> SQLCst.SELECT_DISTINCT
                string columnSelect = SQLCst.TBLMAIN + "." + pRc.Relation[0].ColumnSelect[0].ColumnName;
                sqlQuery = SQLCst.SELECT_DISTINCT + SQLCst.TBLMAIN + "." + pRc.Relation[0].ColumnRelation[0].ColumnName + ",";
                sqlQuery += columnSelect;
                //
                if (ArrFunc.IsFilled(pRc.Relation[0].ColumnLabel))
                {
                    string concat = DataHelper.GetConcatOperator(SessionTools.CS);
                    string columnLabel = SQLCst.TBLMAIN + "." + pRc.Relation[0].ColumnLabel[0].ColumnName;
                    //					
                    sqlQuery += concat + "case when (not " + columnLabel + " is null) and (" + columnLabel + "!=" + columnSelect + ") then "
                        + DataHelper.SQLString(" - ") + concat + columnLabel
                        + " else ' ' end";
                    //
                    if (opAliasColumnSelect == pRc.Relation[0].ColumnSelect[0].ColumnName)
                        opAliasColumnSelect += "_";
                }
                sqlQuery += " as " + opAliasColumnSelect + Cst.CrLf;
                //
                if (pIsColor)
                    sqlQuery += "," + SQLCst.TBLMAIN + ".FORECOLOR," + SQLCst.TBLMAIN + ".BACKCOLOR";
            }
            sqlQuery += SQLCst.FROM_DBO + tableNameForDDL + " " + SQLCst.TBLMAIN + Cst.CrLf;

            #region Restriction
            string tmp_SQLJoin = string.Empty;

            if (pReferential.IsSQLWhereWithSessionRestrict() && (false == SessionTools.IsSessionSysAdmin))
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, false);

                if (tableNameForDDL == Cst.OTCml_TBL.GINSTR.ToString())
                    tmp_SQLJoin = srh.GetSQLGroupeInstr(string.Empty, SQLCst.TBLMAIN + ".IDGINSTR");

                if (tableNameForDDL == Cst.OTCml_TBL.BOOK.ToString() ||
                    tableNameForDDL == Cst.OTCml_TBL.VW_BOOK_VIEWER.ToString())
                {
                    string where = string.Empty;
                    srh.GetSQLBook(string.Empty, SQLCst.TBLMAIN + ".IDB", out tmp_SQLJoin, out where);
                    sqlWhere.Append(where);
                }

                if (tableNameForDDL == Cst.OTCml_TBL.ACTOR.ToString())
                {
                    if (false == ((pRc.ColumnName == "IDAINS") || (pRc.ColumnName == "IDAUPD")))
                        tmp_SQLJoin = srh.GetSQLActor(string.Empty, SQLCst.TBLMAIN + ".IDA");
                }

                if (tableNameForDDL == Cst.OTCml_TBL.PRODUCT.ToString())
                {
                    if (false == (pReferential.TableName == Cst.OTCml_TBL.INSTRUMENT.ToString()))
                        tmp_SQLJoin = srh.GetSQLProduct(String.Empty, SQLCst.TBLMAIN + ".IDP");
                }

                if (tableNameForDDL == Cst.OTCml_TBL.INSTRUMENT.ToString())
                    tmp_SQLJoin = srh.GetSQLInstr(string.Empty, SQLCst.TBLMAIN + ".IDI");

                if (tableNameForDDL == Cst.OTCml_TBL.MARKET.ToString())
                    tmp_SQLJoin = srh.GetSQLMarket(string.Empty, SQLCst.TBLMAIN + ".IDM");
                sqlQuery += tmp_SQLJoin;
            }
            #endregion

            if (pRc.Relation[0].Condition != null)
            {
                string where = string.Empty;
                for (int nbCondition = 0; nbCondition < pRc.Relation[0].Condition.Length; nbCondition++)
                {
                    where = pRc.Relation[0].Condition[nbCondition].SQLWhere;
                    //
                    if (nbCondition == 0)
                    {
                        #region Condition[nbCondition].TableName
                        if (pRc.Relation[0].Condition[nbCondition].TableName != null)
                        {
                            string tableName = pRc.Relation[0].Condition[nbCondition].TableName;
                            if (tableName.IndexOf("|") > 0)
                            {
                                //Il existe une table pour les Création et une pour les Modifications (eg: TBLINS|TBLUPD)
                                string[] aTableName = tableName.Split("|".ToCharArray());
                                tableName = (pIsNewRecord ? aTableName[0] : aTableName[1]);
                            }
                            string aliasTableName = tableName;
                            if ((pRc.Relation[0].Condition[nbCondition].AliasTableNameSpecified) && (pRc.Relation[0].Condition[nbCondition].AliasTableName != null))
                                aliasTableName = pRc.Relation[0].Condition[nbCondition].AliasTableName;
                            //
                            sqlQuery += Cst.CrLf + SQLCst.INNERJOIN_DBO + tableName;
                            if (aliasTableName != tableName)
                                sqlQuery += " " + aliasTableName;
                            sqlQuery += " on (";
                            sqlQuery += aliasTableName + "." + pRc.Relation[0].Condition[nbCondition].ColumnRelation[0].ColumnName;
                            sqlQuery += " = ";
                            sqlQuery += SQLCst.TBLMAIN + "." + pRc.Relation[0].ColumnRelation[0].ColumnName;
                            sqlQuery += ")";
                        }
                        #endregion
                    }
                    if (pIsScalar)
                    {
                        //On est ici sur le test d'existence des référentiels enfants, on utilise donc la PK
                        where = string.Empty;
                    }
                    else
                    //On est ici sur le référentiel lui même , on utilise donc la FK
                    {
                        //20081008 PL Newness valueForeignKey
                        string valueForeignKey = (StrFunc.IsFilled(pValueForeignKeyField) ? pValueForeignKeyField : SQLCst.NULL);
                        where = where.Replace(Cst.FOREIGN_KEY, valueForeignKey);
                    }
                    //
                    where = where.Replace(Cst.MODE_NEWRECORD, (pIsNewRecord ? "1" : "0"));
                    //
                    #region Condition[nbCondition].ColumnName
                    //20060424 PL GLOPTEST
                    bool isColumnName = pRc.Relation[0].Condition[nbCondition].ColumnNameSpecified;
                    bool isColumnValue = where.IndexOf(Cst.COLUMN_VALUE) > 0;
                    while (isColumnName || isColumnValue)
                    {
                        string currentvalue = string.Empty;
                        string constantID = null;
                        string clientID = null;
                        string aliasTableName = null;
                        if (isColumnName)
                        {
                            constantID = Cst.COLUMN_VALUE;
                            clientID = pRc.Relation[0].Condition[nbCondition].ColumnName;
                        }
                        else
                        {
                            int startConstant = where.IndexOf(Cst.COLUMN_VALUE);
                            int endConstant = startConstant + Cst.COLUMN_VALUE.Length;
                            int posDblPercent = where.IndexOf("%%", endConstant);
                            clientID = where.Substring(endConstant, posDblPercent - endConstant);
                            constantID = Cst.COLUMN_VALUE + clientID + "%%";
                            //20090305 PL Si un "." est présent on a alors: aliasTableName.columnName
                            if (clientID.IndexOf(".") > 0)
                            {
                                int posDot = clientID.IndexOf(".");
                                aliasTableName = clientID.Substring(0, posDot);
                                clientID = clientID.Substring(posDot + 1);
                            }
                        }
                        //
                        ReferentialColumn rrc = null;
                        try
                        {
                            string suffix_ClientID = string.Empty;
                            rrc = pReferential[clientID, aliasTableName];
                            if (rrc.IsAdditionalData)
                                suffix_ClientID = rrc.ExternalFieldID.ToString();
                            //
                            bool isEnabled = ((!rrc.IsForeignKeyField) || (StrFunc.IsEmpty(pReferential.valueForeignKeyField)));
                            bool isFK = !isEnabled;

                            //PL 20120117 Add test on !IsTypeBool
                            if ((pReferential.isLookReadOnly && !TypeData.IsTypeBool(rrc.DataType.value)) || isFK)
                            {
                                //FI 20120305 Add cas TextBox
                                //En mode ReadOnly ControlMain n'est pas nécessairement une Table 
                                //C'est une table uniquement sur les données chargé via DDL ds le formulaire
                                //
                                if (rrc.ControlMainSpecified)
                                {
                                    if (rrc.ControlMain is BS_TextBox)
                                    {
                                        BS_TextBox txt = rrc.ControlMain as BS_TextBox;
                                        currentvalue = txt.Attributes["data-value"];
                                        if (StrFunc.IsEmpty(currentvalue))
                                            currentvalue = txt.Text;
                                    }
                                    else if (rrc.ControlMain is BS_DropDownList)
                                    {
                                        BS_DropDownList ddl = rrc.ControlMain as BS_DropDownList;
                                        currentvalue = ddl.SelectedValue;
                                    }
                                    else if (rrc.AutoCompleteEnabled)
                                    {
                                        //HiddenField hdn = rrc.ControlMain as HiddenField;
                                        //currentvalue = hdn.Value;
                                    }
                                }
                            }
                            else if (pPage.IsPostBack)
                            {
                                #region IsPostBack
                                //Get value from Page
                                if (TypeData.IsTypeBool(rrc.DataType.value))
                                {
                                    clientID = Cst.CHK + clientID + suffix_ClientID;
                                }
                                else if (RepositoryTools.IsDataForDDL(rrc))
                                {
                                    clientID = Cst.DDL + clientID + suffix_ClientID;
                                }
                                else
                                {
                                    //PL 20130516 Test
                                    if (rrc.ExistsRelation && (rrc.Relation[0].AutoComplete != null))
                                    {
                                        clientID = Cst.HDN + clientID + suffix_ClientID;
                                    }
                                    else
                                    {
                                        clientID = Cst.TXT + clientID + suffix_ClientID;
                                    }
                                }
                                // EG 20150923 Test null
                                if (null != pPage.Request.Form[clientID])
                                {
                                    currentvalue = pPage.Request.Form[clientID].ToString();
                                    if (TypeData.IsTypeBool(rrc.DataType.value))
                                        currentvalue = DataHelper.SQLBoolean(BoolFunc.IsTrue(currentvalue));
                                }
                                #endregion IsPostBack
                            }
                            else
                            {
                                //Get value from Referential
                                if (TypeData.IsTypeBool(rrc.DataType.value))
                                {
                                    BS_CheckBox chk = rrc.ControlMain as BS_CheckBox;
                                    currentvalue = DataHelper.SQLBoolean(chk.Checked);
                                }
                                else if (RepositoryTools.IsDataForDDL(rrc))
                                {
                                    BS_DropDownList ddl = rrc.ControlMain as BS_DropDownList;
                                    currentvalue = ddl.SelectedValue;
                                }
                                else
                                {
                                    BS_TextBox txt = rrc.ControlMain as BS_TextBox;
                                    currentvalue = txt.Attributes["data-value"];
                                    if (StrFunc.IsEmpty(currentvalue))
                                        currentvalue = txt.Text;
                                }
                            }
                        }
                        catch
                        {
                            #region catch
                            try
                            {
                                //20080829 PL En mode "Duplication" on est en "IsPostBack" mais étrangement Request.Form est vide...
                                //            On recherche donc dans la calsse "rrc"
                                if (pPage.IsPostBack)
                                {
                                    //Get value from Referential
                                    if (TypeData.IsTypeBool(rrc.DataType.value))
                                    {
                                        BS_CheckBox chk = rrc.ControlMain as BS_CheckBox;
                                        currentvalue = DataHelper.SQLBoolean(chk.Checked);
                                    }
                                    else if (RepositoryTools.IsDataForDDL(rrc))
                                    {
                                        BS_DropDownList ddl = rrc.ControlMain as BS_DropDownList;
                                        currentvalue = ddl.SelectedValue;
                                    }
                                    else
                                    {
                                        BS_TextBox txt = rrc.ControlMain as BS_TextBox;
                                        currentvalue = txt.Text;
                                    }
                                }
                            }
                            catch
                            {
                                currentvalue = string.Empty;
                            }
                            #endregion catch
                        }
                        //20080922 PL Add if()
                        if ((currentvalue.Length == 0) && (TypeData.IsTypeNumeric(rrc.DataType.value) || TypeData.IsTypeBool(rrc.DataType.value)))
                            currentvalue = SQLCst.NULL;
                        //
                        where = where.Replace(constantID, currentvalue);
                        //
                        if (isColumnName)
                            isColumnName = false;
                        isColumnValue = where.IndexOf(Cst.COLUMN_VALUE) > 0;
                    }
                    #endregion
                    //
                    sqlWhere.Append(where);
                }
            }
            string sql_Select = sqlQuery + sqlWhere.ToString(true);
            Debug.WriteLine(sql_Select);
            //
            return sql_Select;
        }

        /// <summary>
        /// Execute la commande SQLPreSelect définie dans referential
        /// <remarks>Remplace %%DA:PK%% et %%DA:FK%% pouvant exister ds la commande</remarks>
        /// <remarks>Remplace @PK et @FK pouvant exister ds la commande</remarks>
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pDataKeyField"></param>
        /// <param name="pValueDataKeyField"></param>
        private static void ExecutePreSelect(Referential pReferential, string pValueDataKeyField, string pValueForeigKeyField)
        {
            if (pReferential.SQLPreSelectSpecified)
            {
                QueryParameters[] queryParameters = pReferential.GetSqlPreSelectCommand(SessionTools.CS);
                //
                for (int i = 0; i < ArrFunc.Count(queryParameters); i++)
                {

                    if (pReferential.IndexDataKeyField > -1)
                    {
                        //Replace du %%DA:PK%% pouvant exister ds la query
                        StringDynamicData sdPk = new StringDynamicData();
                        sdPk.name = "PK";
                        sdPk.datatype = pReferential[pReferential.IndexDataKeyField].DataType.value;
                        if (StrFunc.IsFilled(pValueDataKeyField))
                        {
                            sdPk.value = pValueDataKeyField;
                        }
                        else
                        {
                            if (TypeData.IsTypeString(sdPk.datatype))
                                sdPk.value = "N/A";
                            if (TypeData.IsTypeInt(sdPk.datatype))
                                sdPk.value = "-1";
                            else
                                throw new NotImplementedException(StrFunc.AppendFormat("dataType [{0}] is not implemented", sdPk.datatype));
                        }
                        queryParameters[i].query = sdPk.ReplaceInString(SessionTools.CS, queryParameters[i].query);
                        //
                        if ((pReferential.IsUseSQLParameters) && queryParameters[i].query.Contains("@PK"))
                        {
                            DataParameter pkParameter = sdPk.GetDataParameter(SessionTools.CS, null, CommandType.Text, 0, ParameterDirection.Input);
                            queryParameters[i].parameters.Add(pkParameter);
                        }

                    }
                    //    
                    //Replace du %%DA:FK%% pouvant exister ds la query
                    if (pReferential.IndexForeignKeyField > -1)
                    {
                        //Replace du %%DA:PK%% pouvant exister ds la query
                        StringDynamicData sdFk = new StringDynamicData();
                        sdFk.name = "FK";
                        sdFk.datatype = pReferential[pReferential.IndexForeignKeyField].DataType.value;
                        if (StrFunc.IsFilled(pValueForeigKeyField))
                        {
                            sdFk.value = pValueForeigKeyField;
                        }
                        else
                        {
                            if (TypeData.IsTypeString(sdFk.datatype))
                                sdFk.value = "N/A";
                            if (TypeData.IsTypeInt(sdFk.datatype))
                                sdFk.value = "-1";
                            else
                                throw new NotImplementedException(StrFunc.AppendFormat("dataType [{0}] is not implemented", sdFk.datatype));
                        }
                        queryParameters[i].query = sdFk.ReplaceInString(SessionTools.CS, queryParameters[i].query);

                        if ((pReferential.IsUseSQLParameters) && queryParameters[i].query.Contains("@FK"))
                        {
                            DataParameter fkParameter = sdFk.GetDataParameter(SessionTools.CS, null, CommandType.Text, 0, ParameterDirection.Input);
                            queryParameters[i].parameters.Add(fkParameter);
                        }
                    }
                    DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, queryParameters[i].query, queryParameters[i].parameters.GetArrayDbParameter());
                }
            }
        }
        /* FI 20200205 [XXXXX]  Méthode mis en commentaire puisque non utilisée
        /// <summary>
        /// Obtient un array de DynamicArgument formattés à partir d'un paramètre {pDA} issu d'une requête Http 
        /// <para>Chaque item de l'array est au format de serialization d'un StringDynamicData </para>
        /// </summary>
        /// <param name="pQueryStringDA"></param>
        /// <returns></returns>
        /// EG 201306026 Appel à la méthode de Désérialisation d'un StringDynamicData en chaine
        public static string[] CalcDynamicArgumentFromHttpParameter(string pDA)
        {
            string[] ret = StrFunc.StringArrayList.StringListToStringArray(pDA);
            //
            #region cas particulier lorsque DA n'est pas une string XML correspondant à la serialisation d'un StringDynamicData
            if (1 <= ArrFunc.Count(ret))
            {
                try
                {
                    EFS_SerializeInfoBase serializerInfo = null;
                    if (1 == ArrFunc.Count(ret))
                    {
                        //serializerInfo = new EFS_SerializeInfoBase(typeof(StringDynamicData), ret[0]);
                        //StringDynamicData sDa = (StringDynamicData)CacheSerializer.Deserialize(serializerInfo);
                        StringDynamicData sDa = RepositoryTools.DeserializeDA(ret[0]);

                    }
                    else
                    {
                        serializerInfo = new EFS_SerializeInfoBase(typeof(StringDynamicDatas), "<Datas>" + pDA + "</Datas>");
                        StringDynamicDatas sDas = (StringDynamicDatas)CacheSerializer.Deserialize(serializerInfo);
                    }
                }
                catch
                {
                    //Lorsque URL contient un DA au format non XML (donc non deserializable en StringDynamicData)  
                    //Spheres Applique un format defaut
                    ret[0] = @"<Data name=""DATA"" datatype=""string"">" + ret[0] + "</Data>";
                }
            }
            #endregion
            //
            return ret;
        }
        */

        /// Désérialisation d'une chaine en StringDynamicData avec encodage et décodage du "& commercial"
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// EG 201300626 New
        public static StringDynamicData DeserializeDA(string pData)
        {
            StringDynamicData _dd = null;
            string _data = pData;
            string etcom = HttpUtility.UrlEncode("&", System.Text.Encoding.Default);
            _data = _data.Replace("&", etcom);
            EFS_SerializeInfoBase serializerInfo = serializerInfo = new EFS_SerializeInfoBase(typeof(StringDynamicData), _data);
            _dd = (StringDynamicData)CacheSerializer.Deserialize(serializerInfo);
            if (StrFunc.IsFilled(_dd.value))
                _dd.value = _dd.value.Replace(etcom, "&");
            return _dd;

        }
        /// <summary>
        /// Sérialisation d'unStringDynamicData en string avec encodage et décodage du "& commercial"
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// EG 201300626 New
        public static string SerializeDA(StringDynamicData pDD)
        {
            string etcom = HttpUtility.UrlEncode("&", System.Text.Encoding.Default);
            bool isValueSpecified = StrFunc.IsFilled(pDD.value);
            if (isValueSpecified)
                pDD.value = pDD.value.Replace("&", etcom);
            string _data = pDD.Serialize();
            if (isValueSpecified)
                pDD.value = pDD.value.Replace(etcom, "&");
            return _data;
        }

        /// <summary>
        /// Retourne les DynamicArgument formattés à partir d'un paramètre {pDA} issu d'une requête Http 
        /// <para>Chaque item du dictionnaire  est au format par serialization d'un StringDynamicData</para>
        /// <para>Retourne null si {pDA} est vide</para>
        /// </summary>
        /// <param name="pQueryStringDA"></param>
        /// <returns></returns>
        /// EG 201306026 Appel à la méthode de Désérialisation d'un StringDynamicData en chaine
        public static Dictionary<string, StringDynamicData> CalcDynamicArgumentFromHttpParameter2(string pDA)
        {
            Dictionary<string, StringDynamicData> ret = null;
            //
            if (StrFunc.IsFilled(pDA))
            {
                string[] arrayDA = StrFunc.StringArrayList.StringListToStringArray(pDA);
                if (ArrFunc.IsFilled(arrayDA))
                {
                    ret = new Dictionary<string, StringDynamicData>();
                    for (int i = 0; i < arrayDA.Length; i++)
                    {
                        StringDynamicData sDD = null;
                        try
                        {

                            /// EG 201300626 
                            /// EFS_SerializeInfoBase serializerInfo = serializerInfo = new EFS_SerializeInfoBase(typeof(StringDynamicData), arrayDA[i]);
                            //sDD = (StringDynamicData)CacheSerializer.Deserialize(serializerInfo);
                            sDD = DeserializeDA(arrayDA[i]);
                            ret.Add(sDD.name, sDD);
//ret[sDD.name].isSettingByRequestHTTP = true;
                        }
                        catch
                        {
                            sDD = new StringDynamicData("string", "DATA_" + i.ToString(), arrayDA[i]);
                            ret.Add(sDD.name, sDD);
                        }
                    }
                }
            }
            return ret;
        }



        /// <summary>
        /// Retournne l'URL qui ouvre le formulaire de saisie/consultation d'un référentiel
        /// </summary>
        /// <param name="pType">Valeurs possibles (Referential,Cconsultation,Log,etc...)</param>
        /// <param name="pObject">Nom du référentiel</param>
        /// <param name="pMode"></param>
        /// <param name="pNew"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArg"></param>
        /// <param name="pFormId">Représente le formulaire appelant</param>
        /// <param name="pDS">Représente l'identifiant du datasetCourant (permet la navigation vers les autres lignes à partir du formulaire)</param>
        /// <param name="pPK">Représente la colonne PK</param>
        /// <param name="pPKValue">Repésente la valeur de la PK</param>
        /// <param name="pFK">Représente la colonne FK (utilisé pour les référentiels enfants)</param>
        /// <param name="pFKValue">Représente la colonne FK (utilisé pour les référentiels enfants)</param>
        /// <param name="pIdMenu">Représente le menu associé au référentiel</param>
        /// <param name="pTitleMenu"></param>
        /// <returns></returns>
        public static string GetURLOpenRepository(string pType, string pObject, Cst.ConsultationMode pMode, bool pNew,
            string pCondApp, string[] pParam, string[] pDynamicArg,
            string pFormId, string pDS,
            string pPK, string pPKValue, string pFK, string pFKValue,
            string pIdMenu, string pTitleMenu)
        {
            //
            OpenReferentialFormArguments arg = new OpenReferentialFormArguments();
            arg.type = pType;
            arg.@object = pObject;
            arg.mode = pMode;
            arg.@IsnewRecord = pNew;
            arg.condApp = pCondApp;
            arg.param = pParam;
            arg.dynamicArg = pDynamicArg;
            arg.PK = pPK;
            arg.PKValue = pPKValue;
            arg.FK = pFK;
            arg.FKValue = pFKValue;
            arg.idMenu = pIdMenu;
            arg.titleMenu = pTitleMenu;
            return arg.GetURLOpenFormRepository();
        }

        /// <summary>
        /// Retourne les commandes SQL rattachées à l'identifiant {pName}
        /// <para>Ouverture de la ressource incorporée pour récupérer les ordres SQL</para>
        /// </summary>
        /// <param name="pName"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException si la ressource incorporée qui contient la commande est non chargée"></exception>
        public static ReferentialSQLSelects LoadReferentialsReferentialSQLSelect(string pName)
        {
            //
            string streamName = Assembly.GetExecutingAssembly().GetName().Name + ".XML_Files." + "sqlCommand.xml";
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(streamName);

            //FI 20120516 En mode Debug lecture du fichier
#if DEBUG
            //En debug Spheres® cherche à lire le fichier xml sui se trouve Sous Referentiel\XML_Files
            //Cela permet de ne pas compiler à chaque modification du fichier sqlCommand
            string path = HttpContext.Current.Request.PhysicalApplicationPath;
            if (path.EndsWith(@"\"))
                path = path.Substring(0, path.Length - 1);
            try
            {
                DirectoryInfo dirInfo = Directory.GetParent(path);
                //path = dirInfo.FullName + @"\" + AspTools.GetReferentialProject() + @"\XML_Files\sqlCommand.xml";
                path = dirInfo.FullName + @"\Referentiel\XML_Files\sqlCommand.xml";
                stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch (DirectoryNotFoundException)
            {
                //Spheres® abandonne

            }
#endif

            if (null == stream)
                throw new NullReferenceException("SqlCommand.xml Ressource not found");

            StreamReader reader = new StreamReader(stream);
            string xml = reader.ReadToEnd();
            reader.Close();
            //
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            //
            string xPath = StrFunc.AppendFormat(@"sqlCommands/item[@name='{0}']", pName);
            //
            XmlNodeList list = xmlDoc.SelectNodes(xPath);
            if (false == (list.Count > 0))
                throw new Exception(StrFunc.AppendFormat("tag sqlCommands/item['{0}'] not Found", pName));
            //
            string xmlItem = StrFunc.AppendFormat(@"<sqlCommands>{0}</sqlCommands>", list.Item(0).InnerXml);
            //
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(ReferentialSQLSelects), xmlItem);
            ReferentialSQLSelects ret = (ReferentialSQLSelects)CacheSerializer.Deserialize(serializeInfo);
            //
            return ret;
        }

        /// <summary>
        /// Retourne true lorsque l'utilisateur a les droits de créer, modifier et supprimer sur le référentiel associé au menu {pIdMenu}
        /// <para>Cette méthode ne tient pas compte des drois directement définis sur le référentiel</para>
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        /// FI 20120116 Un admin n'a pas nécessairement les droits Create,Modify,Delete etc.. (c'est le cas lorsque la Database est de type READONLY)
        public static bool IsAllPermissionEnabled(string pCS, string pIdMenu, User pUser)
        {
            bool ret = true;
            if (StrFunc.IsFilled(pIdMenu))
            {
                if ((pIdMenu == "ATTACHEDDOC") || (pIdMenu == "ATTACHEDDOCS"))
                {
                    //Note: 
                    //Il n'existe pas pour l'instant de gestion des permissions sur l'accès aux pièces jointes
                    //Cela peut être gênant, car un user qui n'aurait pas le droit de modifié/supprimé un item 
                    //pourra malgré tout ajouter/supprimer une pièce jointe (cf. TRIM n°3230)
                }
                else
                {
                    RestrictionPermission restrictPermission = new RestrictionPermission(pIdMenu, pUser);
                    restrictPermission.Initialize(pCS);
                    //
                    if (!restrictPermission.isCreate)
                        ret = false;
                    if (!restrictPermission.isModify)
                        ret = false;
                    if (!restrictPermission.isRemove)
                        ret = false;
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne un dataset alimenté avec l'enregistrement ouvert par le formulaire
        /// <para>Le dataset est vide en mode pIsNewRecord</para>
        /// </summary>
        /// <param name="pPage">page qui contient le formuliare</param>
        private static DataSet GetDsDataForm(RepositoryPageBase pPage)
        {
            // Dans le cas d'un appel direct au formulaire (sans passer par l'étape du DataGrid), Spheres joue le script contenu dans la PreSelectCommand
            // Ce cas se produit lorsque l'utilisateur ouvre un acteur depuis le masque de saisie
            // En mode isOpenFromGrid, si le dataset du grid associé est non chargé Spheres® lance le SQLPreSelect. 
            // Cet dernier doit absolument être joué avant de charger le dataset  
            bool isPreCommand = true; // par défaut
            if ((pPage.isOpenFromGrid) & (pPage.isDatasetGridFilled))
                isPreCommand = false;

            if (isPreCommand)
                ExecutePreSelect(pPage.referential, pPage.valueDataKeyField, pPage.valueForeignKeyField);

            // Dans le cas d'un appel direct au formulaire (sans passer par l'étape du DataGrid), le DataSet est null
            // Spheres® le recharge ici
            string column = pPage.dataKeyField;
            string value = pPage.valueDataKeyField;
            if (pPage.isNewRecord && StrFunc.IsEmpty(column))
            {
                //20070529 PL Astuce pour que l'alimentation du Dataset ne retourne aucune ligne en mode "création" (cf. GetSQLSelect() )
                column = "'TIP'";
                value = "'NewRecord'";
            }
            bool isSelectDistinct = (pPage.consultationMode == Cst.ConsultationMode.Select);

            DataSet ds = SQLReferentialData.RetrieveDataFromSQLTable(SessionTools.CS,
                                    pPage.referential, column, value, true, false, isSelectDistinct, pPage.valueForeignKeyField);

            // CaseSensitive = true pour Oracle et la gestion du ROWBERSION (ROWID) 
            ds.Tables[0].CaseSensitive = true;
            return ds;
        }

        /// <summary>
        /// Suppression des données présentes dans le cache dépendantes ds la table {pTable}
        /// </summary>
        /// <param name="pTable"></param>
        public static void RemoveCache(string pTable)
        {
            //Cache WEB
            if (Enum.IsDefined(typeof(Cst.OTCml_TBL), pTable))
            {
                Cst.OTCml_TBL tableEnum = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pTable);
                switch (tableEnum)
                {
                    case Cst.OTCml_TBL.ENUM:
                    case Cst.OTCml_TBL.ENUMS:
                    case Cst.OTCml_TBL.EVENTENUMS:
                    case Cst.OTCml_TBL.EVENTENUM:
                    case Cst.OTCml_TBL.STCHECK:
                    case Cst.OTCml_TBL.STMATCH:
                        ExtendEnumsTools.ResetFpMLEnumsAndSchemes();
                        ExtendEnumsTools.LoadFpMLEnumsAndSchemes(SessionTools.CS);
                        break;
                }
            }
            //Cache SQL
            DataHelper.queryCache.Remove(pTable, SessionTools.CS);
        }

        /// <summary>
        /// Retourne le chemin du fichier xml descriptif retenu pour afficher {pObjectName}
        /// </summary>
        /// <param name="pObjectType"></param>
        /// <param name="pObjectName"></param>
        public static string GetObjectXMLFile(Cst.ListType pObjectType, string pObjectName)
        {
            //FI 20160804 [Migration TFS] Call SessionTools.NewAppInstance().SearchFile (cette méthode effectue une lecture de FileConfig)
            //return GetXMLFile(pObjectType.ToString(), pObjectName);
            string fileName = string.Empty;
            SessionTools.NewAppInstance().SearchFile2(SessionTools.CS, StrFunc.AppendFormat(@"~\PDIML\{0}\{1}.XML", pObjectType.ToString(), pObjectName), ref fileName);
            return fileName;
        }

        /// <summary>
        /// Retourne un titre titre specifique pour un menu en fonction de l'object
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <returns></returns>
        public static string ParticularTitle(string pIdMenu, string pObjectName)
        {
            string ret = string.Empty;
            //
            Nullable<IdMenu.Menu> idMenu = IdMenu.ConvertToMenu(pIdMenu);
            if (idMenu.HasValue && idMenu.Value == IdMenu.Menu.IOTRACKCOMPARE)
            {
                if (StrFunc.ContainsIn(pObjectName, "IOTRACKCOMPARE_"))
                {
                    string shortName = pObjectName.Replace("IOTRACKCOMPARE_", string.Empty);
                    //
                    Nullable<EFS.SpheresIO.CompareOptions> compareOption = CompareOptionsAttribute.Parse(shortName, true);
                    if (false == compareOption.HasValue)
                        compareOption = (EFS.SpheresIO.CompareOptions)Enum.Parse(typeof(EFS.SpheresIO.CompareOptions), shortName, true);
                    //
                    ret = Ressource.GetString(StrFunc.AppendFormat("CompareOptions_{0}", compareOption.Value.ToString()), compareOption.Value.ToString());
                }
            }
            //
            return ret;
        }

        /// <summary>
        /// Retourne la liste des objects pouvait être utilisé pour déserializer la consultation 3 pts
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <param name="pObjectName"></param>
        /// <returns></returns>
        public static List<string> GetObjectNameForDeserialize(string pIdMenu, string pObjectName)
        {
            List<String> lstObjectName = new List<String>();
            lstObjectName.Add(pObjectName);
            //
            Nullable<IdMenu.Menu> idMenu = IdMenu.ConvertToMenu(pIdMenu);
            if (idMenu.HasValue)
            {
                switch (idMenu.Value)
                {
                    case IdMenu.Menu.IOTRACKCOMPARE:
                        if (pObjectName.Contains("IOTRACKCOMPARE_"))
                        {
                            string compareType = pObjectName.Replace("IOTRACKCOMPARE_", string.Empty);
                            Nullable<EFS.SpheresIO.CompareOptions> compareOptions = EFS.SpheresIO.CompareOptionsAttribute.Parse(compareType, true);
                            if (compareOptions.HasValue)
                            {
                                switch (compareOptions.Value)
                                {
                                    case EFS.SpheresIO.CompareOptions.Trades:

                                        lstObjectName.Add("IOTRACKCOMPARE_TRADE");

                                        break;

                                    case EFS.SpheresIO.CompareOptions.TradesInPosition:
                                    case EFS.SpheresIO.CompareOptions.Positions:

                                        //MF 20120402 - ticket 9928 , résolution : interface IOTRACKCOMPARE_POS additionelle position pour Vision
                                        if (Software.IsSoftwareVision())
                                        {
                                            lstObjectName.Add("IOTRACKCOMPARE_POS");
                                        }
                                        else
                                        {
                                            lstObjectName.Add("IOTRACKCOMPARE_TRADE");
                                        }

                                        break;

                                    case EFS.SpheresIO.CompareOptions.CashFlows:

                                        lstObjectName.Add("IOTRACKCOMPARE_AMT");

                                        break;

                                    case EFS.SpheresIO.CompareOptions.CashFlowsInstr:

                                        lstObjectName.Add("IOTRACKCOMPARE_AMI");

                                        break;
                                }
                            }
                        }
                        lstObjectName.Add("IOTRACKCOMPARE");
                        break;
                    default:
                        break;
                }
            }
            //
            return lstObjectName;
        }


        /// <summary>
        /// Mise en place link sur le control {pControl} qui ouvre un formulaire qui permet de consulter la donnée en rapport avec {pRrc} 
        /// </summary>
        /// <param name="pPage">Page qui contient le control</param>
        /// <param name="pControl"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc">Caractéristiques de la colonne</param>
        /// <param name="pData">Donnée à consulter</param>
        /// <param name="pMode"></param>
        /// FI 20140911 [XXXXX] add Method
        private static void SetControlHyperLinkColumn(PageBase pPage, WebControl pControl, Referential pReferential, ReferentialColumn pRc, string pData, Cst.ConsultationMode pMode)
        {
            if (IsHyperLinkAvailable(pRc))
            {
                // EG 20170125 [Refactoring URL] New
                Nullable<IdMenu.Menu> idMenu = null;

                string formId = string.Empty;
                if (pPage.Form != null)
                    formId = pPage.Form.ID;

                string url = string.Empty;

                if (pRc.ExistsHyperLinkColumn)
                {
                    string columName = pRc.IsHyperLink.type;
                    string colNameId = pRc.IsHyperLink.data;
                    if (IsHyperLinkColumn(colNameId))
                    {
                        int indexColId = pReferential.GetIndexColSQL(colNameId);
                        if (-1 == indexColId)
                            throw new Exception(StrFunc.AppendFormat("Column [name:{0}] doesn't exist", colNameId));

                        string value = pReferential.dataRow[indexColId].ToString();
                        // EG 20170125 [Refactoring URL] Upd
                        idMenu = SpheresURL.GetMenu_Repository(colNameId, value);
                    }
                }
                else if (pRc.ExistsRelation)
                {
                    ReferentialColumnRelation colRelation = pRc.Relation[0];
                    string columName = GetHyperLinkColumnName(colRelation);

                    if (StrFunc.IsFilled(columName) && IsHyperLinkColumn(columName))
                    {
                        if ((columName == "IDASSET") && (pReferential.dataRow.Table.Columns.Contains("ASSETCATEGORY")))
                        {
                            Cst.UnderlyingAsset assetCategory = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), pReferential.dataRow["ASSETCATEGORY"].ToString());
                            idMenu = SpheresURL.GetMenu_Asset(columName, null, assetCategory);
                        }
                        else
                        {
                            idMenu = SpheresURL.GetMenu_Repository(columName, pData);
                        }
                    }
                }
                else if (IsHyperLinkColumn(pRc.ColumnName))
                {
                    // EG 20170125 [Refactoring URL] Upd
                    idMenu = SpheresURL.GetMenu_Repository(pRc.ColumnName, pData);

                }

                // EG 20170125 [Refactoring URL] Upd
                if (idMenu.HasValue)
                {
                    url = SpheresURL.GetURL(idMenu, pData, pMode, formId);
                    if (StrFunc.IsFilled(url))
                        ControlsTools.SetHyperLinkOpenFormReferential(pControl, url);

                }
            }
        }


        /// <summary>
        /// Retourne true si Spheres® est en mesure d'ouvrir un lien hypertext vers le référentiel associé à la colonne {pColumnName}
        /// </summary>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        public static bool IsHyperLinkColumn(string pColumnName)
        {
            bool ret =
                       (pColumnName == "IDA")
                    || (pColumnName == "IDB")
                    || (pColumnName == "IDI")
                    || (pColumnName == "IDP")
                    || (pColumnName == "IDBC")
                    || (pColumnName == "IDRX")
                    || (pColumnName == "IDM")
                    || (pColumnName == "IDDC")
                    || (pColumnName == "IDASSET")
                    || (pColumnName == "IDASSET_UNL")
                    || (pColumnName == "IDASSET_ETD")
                    || (pColumnName == "IDASSET_EQUITY")
                    || (pColumnName == "IDASSET_FXRATE")
                    || (pColumnName == "IDASSET_COMMODITY")
                    || (pColumnName == "IDASSET_INDEX")
                    || (pColumnName == "IDASSET_RATEINDEX")
                    || (pColumnName == "IDASSET_EXTRDFUND")
                    || (pColumnName == "IDMATURITYRULE")
                    || (pColumnName == "IDCAISSUE")
                    || (pColumnName == "IDT")
                    || (pColumnName == "IDT_ADMIN")
                    || (pColumnName == "IDT_RISK")
                    || (pColumnName == "IDFEEMATRIX")
                    || (pColumnName == "IDFEESCHEDULE")
                    || (pColumnName == "IDCNFMESSAGE")
                    || (pColumnName == "IDIOTASK")
                    || (pColumnName == "IDIOTASKDET")
                    || (pColumnName == "IDIOELEMENT")
                    || (pColumnName == "IDIOINPUT")
                    || (pColumnName == "IDIOOUTPUT")
                    || (pColumnName == "IDIOPARSING")
                    || (pColumnName == "IDIOPARAM")
                    || (pColumnName == "IDIOSHELL")
                    ;
            return ret;
        }

        /// <summary>
        /// Retourne true si les caractéristiques de la colonne {pRrc} permettent à Spheres de produire un URL Hyperlink qui permet l'ouverture d'un formulaire de consultation
        /// </summary>
        /// <param name="pRrc"></param>
        /// <returns></returns>
        public static Boolean IsHyperLinkAvailable(ReferentialColumn pRc)
        {
            Boolean ret = false;

            if ((pRc.ExistsHyperLinkDocument) || (pRc.ExistsHyperLinkColumn) || (pRc.ExistsHyperLinkExternal))
            {
                ret = true;
            }
            else if (pRc.ExistsRelation)
            {
                if (IsHyperLinkColumn(pRc.Relation[0].ColumnRelation[0].ColumnName))
                    ret = true;
            }
            else if (pRc.ColumnName == "URL")
            {
                ret = true;
            }

            return ret;
        }

        /// <summary>
        /// Retourne le nom de colonne utilisé pour piloter les hyperLinks
        /// </summary>
        /// <param name="pRRCR"></param>
        /// <returns></returns>
        /// FI 20140812 [XXXXX] Add
        public static string GetHyperLinkColumnName(ReferentialColumnRelation pRRCR)
        {
            string ret = string.Empty;
            if (ArrFunc.IsFilled(pRRCR.ColumnRelation))
            {
                ret = pRRCR.ColumnRelation[0].ColumnName;
                if (ret == "IDASSET")
                {
                    switch (pRRCR.TableName)
                    {
                        case "ASSET_ETD":
                        case "ASSET_INDEX":
                        case "ASSET_RATEINDEX":
                        case "ASSET_COMMODITY":
                        case "ASSET_EQUITY":
                        case "ASSET_EXTRDFUND":
                            ret = "ID" + pRRCR.TableName;
                            break;
                        default:
                            break;
                    }
                }
            }
            return ret;
        }

        #region GetTooltipByType
        private static string GetTooltipByType(string pType)
        {
            string imgType = string.Empty;
            switch (pType.ToLower())
            {
                case Cst.TypeInformationMessage.Warning:
                    imgType = CSS.Main.tooltipwarning.ToString();
                    break;
                case Cst.TypeInformationMessage.Alert:
                    imgType = CSS.Main.tooltipalert.ToString();
                    break;
                case Cst.TypeInformationMessage.Success:
                    imgType = CSS.Main.tooltipsuccess.ToString();
                    break;
                case Cst.TypeInformationMessage.Information:
                default:
                    imgType = CSS.Main.tooltipinfo.ToString();
                    break;
            }
            return imgType;
        }
        #endregion GetTooltipByType

        #region SetPopover
        /// <summary>
        /// Construction du popover rattaché au contrôle de la colonne
        /// </summary>
        /// <param name="pPage">Formulaire du référentiel</param>
        /// <param name="pRrc">Colonne</param>
        /// <param name="pDefaultTitle">titre du label de la colonne</param>
        /// <returns></returns>
        public static void SetPopover(RepositoryPageBase pPage, ReferentialInformation pInformation, string pTitle, Control pAssociatedControl)
        {
            if (pInformation.ToolTipMessageSpecified || pInformation.LabelMessageSpecified)
            {
                string popover_Title = pTitle;
                string popover_Content = string.Empty;

                #region Type Message (Information|Warning|Alert|Success)
                string popover_Type = Cst.TypeInformationMessage.Information;
                if (pInformation.TypeSpecified)
                    popover_Type = pInformation.Type;
                #endregion Type Message

                if (pInformation.LabelMessageSpecified)
                    popover_Content = Ressource.GetString(pInformation.LabelMessage, true);

                if (pInformation.ToolTipMessageSpecified)
                {
                    if (StrFunc.IsFilled(popover_Content))
                        popover_Content += Cst.HTMLBreakLine;
                    popover_Content += Ressource.GetString(pInformation.ToolTipMessage.Value, true);

                    if (pInformation.ToolTipMessage.titleSpecified)
                        popover_Title = Ressource.GetString(pInformation.ToolTipMessage.title, true);
                }

                WebControl associatedControl = null;
                if (pAssociatedControl is PlaceHolder)
                {
                    if ((pAssociatedControl as PlaceHolder).HasControls())
                    {
                        associatedControl = pAssociatedControl.Controls[0] as WebControl;
                    }

                }
                else
                {
                    associatedControl = pAssociatedControl as WebControl;
                    if (pAssociatedControl is BS_ClassicCheckBox)
                    {
                        associatedControl = (pAssociatedControl as BS_ClassicCheckBox).div;
                    }
                    else if (pAssociatedControl is BS_RadioButton)
                    {
                        associatedControl = (pAssociatedControl as BS_RadioButton).div;
                    }
                    else
                    {
                        // TODO
                    }
                }

                if (null != associatedControl)
                {
                    if (StrFunc.IsFilled(popover_Content))
                        associatedControl.Attributes.Add("data-Content", popover_Content);

                    if (StrFunc.IsFilled(popover_Title))
                    {
                        associatedControl.Attributes.Add("title", popover_Title);
                        associatedControl.Attributes.Add("data-theme", popover_Type);
                    }
                }
            }
        }
        #endregion SetPopover

        /*
        NEWS NEWS NEWS NEWS NEWS NEWS NEWS NEWS NEWS NEWS
        */

        #region AddColumnNameInfo
        private static void AddColumnNameInfo(ReferentialColumn pRc, Control pControl)
        {
            if (pControl is Label)
            {
                Label label = pControl as Label;
                string keyType = string.Empty;
                if (pRc.IsDataKeyFieldSpecified && pRc.IsDataKeyField)
                    keyType = "(PK)";
                else if (pRc.IsKeyFieldSpecified && pRc.IsKeyField)
                    keyType = "(UK)";

                if (pRc.IsForeignKeyFieldSpecified && pRc.IsForeignKeyField)
                    keyType += @"(FK)";

                label.Attributes.Add("data-name", pRc.ColumnName + keyType);
            }
        }
        #endregion AddColumnNameInfo


        #region InitializeID
        /// <summary>
        /// Initialise les champs IDxxx (IDForItemTemplate et IDForItemTemplateRelation) d'une classe referential
        /// IDForItemTemplate et IDForItemTemplateRelation: 
        ///     Champs utilisé pour l'affectation de l'ID du control dans le cas de templateColumn qui nécessite plusieurs ID 
        ///     car le control pour l'affichage et le control pour la modification des données sont deux controles distincts 
        ///     donc nécessité d'un ID supplémentaire
        /// Rappel: ItemTemplate est un element de TemplateColumn
        /// </summary>
        private static void InitializeID(Referential pReferential, ReferentialColumn pRc)
        {
            if (pRc.IsAdditionalData)
                pRc.IDForItemTemplate = "Col" + pRc.ExternalIdentifier;
            else
                pRc.IDForItemTemplate = pRc.DataField;

            if (pRc.ExistsRelation)
            {
                pRc.IDForItemTemplateRelation = pRc.Relation[0].AliasTableName + "_" + pRc.Relation[0].ColumnSelect[0].ColumnName;
            }
            pReferential.HasAggregateColumns = pReferential.HasAggregateColumns ||
                (pRc.GroupBySpecified && pRc.GroupBy.AggregateSpecified);
        }
        #endregion InitializeID
        #region InitializeRegEx
        /// <summary>
        /// Initialisation de la Regex d'une colonne
        /// </summary>
        private static void InitializeRegEx(ReferentialColumn pRc)
        {
            if (0 > pRc.Scale)
                pRc.Scale = 0;

            string msgError = string.Empty;
            string regularExpression = pRc.RegularExpression;
            string minOccurence = (pRc.IsMandatorySpecified && pRc.IsMandatory ? "1" : "0");
            string maxOccurence = Math.Max((pRc.IsMandatorySpecified && pRc.IsMandatory ? 1 : 0), pRc.Length).ToString();

            #region default RegEx
            if (StrFunc.IsEmpty(regularExpression))
            {
                switch (pRc.DataTypeEnum)
                {
                    case TypeData.TypeDataEnum.integer:
                    case TypeData.TypeDataEnum.@int:
                        //[+-]? : + or - (1 char), optionnal ( -> ? )
                        //([\d]{0,x}) : any decimal char : [\d]   from 0 to x occurences max  {0,x}                        
                        regularExpression = @"[-+]?([\d]{" + minOccurence + "," + maxOccurence + "})";
                        msgError = EFSRegex.ErrorMessage(EFSRegex.TypeRegex.RegexInteger);
                        msgError += " (" + ACommon.Ressource.GetString2("RegexMaxLengthError", maxOccurence) + " )";
                        break;
                    case TypeData.TypeDataEnum.dec:
                    case TypeData.TypeDataEnum.@decimal:
                        System.Globalization.NumberFormatInfo nfi = System.Globalization.CultureInfo.CurrentUICulture.NumberFormat;
                        //[+-]? : + or - (1 char), optionnal ( -> ? )
                        //([\d]{0,x}) : any decimal char : [\d]   from 0 to x occurences max  {0,x}      
                        regularExpression = @"([-+]?([\d]{0," + (pRc.Length - pRc.Scale).ToString() + @"}))";
                        if (0 < pRc.Scale)
                        {
                            //? -> first part (int) is optionnal : so we can type .05 for the value 0.05
                            // ( : begin group
                            // nfi.NumberDecimalSeparator : decimal separator
                            //([\d]{0,x}) : any decimal char : [\d]   from 0 to x occurences max  {0,x}    
                            // ) : end group (group is separator + dec)
                            //? -> second part (group as separator + dec) is also optionnal
                            regularExpression += @"?(";
                            regularExpression += "[" + nfi.NumberDecimalSeparator + ".]";
                            regularExpression += @"[\d]{0," + pRc.Scale.ToString() + @"}";
                            regularExpression += @")?";
                        }
                        msgError = EFS.ACommon.Ressource.GetString2("RegexDecimalFormatError", (pRc.Length - pRc.Scale).ToString(), nfi.NumberDecimalSeparator, pRc.Scale.ToString());
                        break;

                    case TypeData.TypeDataEnum.date:
                    case TypeData.TypeDataEnum.datetime:
                    case TypeData.TypeDataEnum.time:
                        {

                            EFSRegex.TypeRegex tRegex = EFSRegex.TypeRegex.None;
                            if (TypeData.IsTypeDateTime(pRc.DataType.value))
                            {
                                tRegex = EFSRegex.TypeRegex.RegexDateTime;
                                regularExpression = EFSRegex.RegularExpression(tRegex);
                                regularExpression += "|" + EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDateRelativeOffset);
                                msgError = EFSRegex.ErrorMessage(tRegex);
                            }
                            else if (TypeData.IsTypeDate(pRc.DataType.value))
                            {
                                tRegex = EFSRegex.TypeRegex.RegexDate;
                                regularExpression = EFSRegex.RegularExpression(tRegex);
                                regularExpression += "|" + EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDateRelativeOffset);
                                msgError = EFSRegex.ErrorMessage(tRegex);
                            }
                            else if (TypeData.IsTypeTime(pRc.DataType.value))
                            {
                                tRegex = EFSRegex.TypeRegex.RegexShortTime;
                                regularExpression = EFSRegex.RegularExpression(tRegex);
                                msgError = EFSRegex.ErrorMessage(tRegex);
                            }
                            break;
                        }
                    default:
                        {
                            if (!TypeData.IsTypeText(pRc.DataType.value))
                            {
                                regularExpression = @"([\w|\W]{" + minOccurence + "," + maxOccurence + @"})";
                                msgError = " (" + ACommon.Ressource.GetString2("RegexMaxLengthError", maxOccurence) + " )";
                            }
                            break;
                        }
                }
            }

            if (StrFunc.IsFilled(regularExpression) && regularExpression.ToUpper().StartsWith("REGEX"))
            {
                string newMsgError = string.Empty;
                string newRegularExpression = ACommon.Ressource.GetString(regularExpression, @"[\w|\W]+");

                switch (regularExpression)
                {
                    case "RegexStringAlphaNumUpper":
                    case "RegexStringAlphaNum":
                        newRegularExpression = newRegularExpression.Replace(@"+$", @"{" + minOccurence + @"," + maxOccurence + @"}$");
                        newMsgError = ACommon.Ressource.GetString(regularExpression + "Error", string.Empty);
                        if (StrFunc.IsFilled(newMsgError))
                            msgError = newMsgError;
                        msgError += " (" + ACommon.Ressource.GetString2("RegexMaxLengthError", maxOccurence) + " )";
                        break;
                }
                regularExpression = newRegularExpression;
            }
            #endregion

            pRc.RegularExpression = regularExpression;
            pRc.MsgErr = msgError;
            pRc.MsgErrRequiredField = EFS.ACommon.Ressource.GetString("ISMANDATORY");
        }
        #endregion InitializeRegEx

        #region CreateControl
        /// <summary>
        /// A DEPLACER DANS REPOSITORYTOOLS
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pMode"></param>
        /// <param name="pIsNewRecord"></param>
        /// <param name="pIsDuplicateRecord"></param>
        public static PlaceHolder CreateControl(RepositoryPageBase pPage, ReferentialColumn pRc)
        {
            PlaceHolder plhControl = new PlaceHolder();

            string warningMsg = string.Empty;
            bool isDisplayColumnName = pPage.isDisplayColumn;
            bool isShowAllData = pPage.IsShowAllData;
            bool isIdentity = (pRc.IsIdentitySpecified && pRc.IsIdentity.Value);
            bool isForeignKey = (pRc.IsForeignKeyField && StrFunc.IsFilled(pPage.referential.valueForeignKeyField)); ;

            //Initialise les champs IDxxx (IDForItemTemplate et IDForItemTemplateRelation)
            InitializeID(pPage.referential, pRc);

            //RegularExpression, MsgErr & MsgErrRequiredField
            if (false == RepositoryTools.IsDataForDDL(pRc))
                InitializeRegEx(pRc);

            bool isEnabled = (false == isForeignKey);
            if (isEnabled && pRc.IsUpdatableSpecified)
            {
                // Enabled if IsUpdatable
                if (pPage.referential.isNewRecord || pPage.isDuplicateRecord)
                {
                    if (pRc.IsUpdatable.isupdatableincreationSpecified)
                    {
                        isEnabled = pRc.IsUpdatable.isupdatableincreation;
                    }
                }
                else
                {
                    isEnabled = pRc.IsUpdatable.Value;
                }
            }

            #region Gestion ToolTip
            string firstControlLinkedColumnName = string.Empty;
            string toolTip = string.Empty;
            string toolTipLabel = string.Empty;
            //Si un tooltip est renseigné
            if (pRc.ToolTipSpecified)
            {
                toolTip = Ressource.GetString(pRc.ToolTip, true);
                toolTipLabel = toolTip;
            }
            //Si pas de tooltip renseigné, on utilise les ressources
            else
            {
                //Cas d'un control lié : ressource du control 'père' + ( ressource du 'fils')
                if (pRc.IsFirstControlLinked || pRc.IsMiddleControlLinked || pRc.IsLastControlLinked)
                {
                    //PLl 20110228 Add Test on firstControlLinkedColumnName
                    if (StrFunc.IsFilled(firstControlLinkedColumnName))
                    {
                        toolTip = Ressource.GetMulti((pPage.referential[firstControlLinkedColumnName]).Ressource, 2);
                        toolTipLabel = toolTip;
                        toolTip += " (" + Ressource.GetString(pRc.ColumnName, true) + ")";
                    }
                }
                //sinon, on affecte simplement la ressource au tooltip
                else
                {
                    toolTip = Ressource.GetMulti(pRc.Ressource, 2, 0, null);
                    toolTipLabel = toolTip;
                }
            }
            #endregion Gestion ToolTip

            #region Construction du control
            Control control = null;
            TypeData.TypeDataEnum dataType = TypeData.GetTypeDataEnum(pRc.DataType.value);

            Control label = null;
            if (pRc.IsItem)
            {
                control = RepositoryTools.CreateControl_BooleanToCheckBox(pPage, pRc, isEnabled);
            }
            else
            {
                label = RepositoryTools.CreateControl_Label(pPage, pRc, isEnabled, toolTipLabel);
                pRc.ControlLabel = (WebControl)label;

                if (isIdentity)
                {
                    control = RepositoryTools.CreateControl_Identity(pPage, pRc, isEnabled);
                }
                else if (TypeData.IsTypeBool(dataType))
                {
                    if (pRc.DataType.value.StartsWith("bool2"))
                        control = RepositoryTools.CreateControl_BooleanToRadioButton(pPage, pRc, isEnabled);
                    else
                        control = RepositoryTools.CreateControl_BooleanToCheckBox(pPage, pRc, isEnabled);
                }
                else if (pRc.TypeAheadEnabled)
                {
                    control = RepositoryTools.CreateControl_TypeAhead(pPage, pRc, isEnabled);
                }
                else if (RepositoryTools.IsDataForDDL(pRc))
                {
                    control = RepositoryTools.CreateControl_DropDownList(pPage, pRc, isEnabled);
                }
                else
                {
                    bool isTextBoxModeMultiLine = (pRc.TextMode == TextBoxMode.MultiLine.ToString()) ||
                                                  (TypeData.IsTypeString(pRc.DataTypeEnum) && (1000 <= pRc.Length));
                    if (isTextBoxModeMultiLine)
                        control = RepositoryTools.CreateControl_TextArea(pPage, pRc, isEnabled);
                    else
                        control = RepositoryTools.CreateControl_TextBox(pPage, pRc, isEnabled);
                }
                plhControl.Controls.Add(label);

            }
            #endregion Construction du control

            if (null != control)
            {
                // InformationControls (popover)
                if (pRc.InformationSpecified)
                    SetPopover(pPage, pRc.Information, pRc.Label, control);

                plhControl.Controls.Add(control);
            }
            return plhControl;
        }
        #endregion CreateControl


        #region CreateControl_TypeAhead
        /// <summary>
        /// Création d'un controle de vtype autocomplete (typeAhead avec Bootstrap)
        /// - Un champ BS_TextBox qui reçoit la donnée visible (IDENTIFIER, DISPLAYNAME, ...)
        /// - Un champ HtmlInputHidden qui reçoit la donnée value pour mise à jour Datarow
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pRc"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public static Control CreateControl_TypeAhead(RepositoryPageBase pPage, ReferentialColumn pRc, bool pIsEnabled)
        {
            Referential referential = pPage.referential;
            PlaceHolder plhTypeAhead = new PlaceHolder();

            bool isColumnReadOnly = pPage.IsColumnReadOnly(pRc) || (false == pIsEnabled);
            isColumnReadOnly |= pRc.IsForeignKeyFieldSpecified && pRc.IsForeignKeyField && StrFunc.IsFilled(pPage.referential.valueForeignKeyField);

            List<Validator> lstValidators = null;
            if (false == isColumnReadOnly)
                lstValidators = ControlsTools.GetValidators(pRc.IsMandatory, pRc.MsgErrRequiredField);

            string controlID = RepositoryTools.GetID(pRc, Cst.TXT);
            BS_TextBox txt = new BS_TextBox(controlID, isColumnReadOnly, lstValidators);

            //txt.Attributes.Add("DDLVALUE", "0");

            HtmlInputHidden txtvalue = new HtmlInputHidden();
            txtvalue.ID = RepositoryTools.GetID(pRc, "TMP");

            #region Data
            string currentValue = RepositoryTools.GetCurrentValue(referential, pRc);
            bool isLoadDDL = StrFunc.IsFilled(referential.valueForeignKeyField) || StrFunc.IsFilled(currentValue);
            if (isLoadDDL)
            {
                DropDownList ddl = new DropDownList();
                LoadDDL(pPage, referential, pRc, ddl, referential.valueForeignKeyField, currentValue, pPage.isNewRecord);

                string warningMsg = string.Empty;
                string textValue = string.Empty;
                bool isFoundValue = RepositoryTools.FoundValue(false, ddl, null, currentValue, pRc, ref textValue, ref warningMsg);

                // Set the text value, using the selected item label from the temporary DDL
                if (ddl.SelectedItem != null)
                {
                    txt.Text = ddl.SelectedItem.Text;
                    if (StrFunc.IsEmpty(currentValue))
                        currentValue = ddl.SelectedValue;

                    txt.Attributes.Add("data-value", currentValue);
                    txtvalue.Value = currentValue;
                }
                ddl.Dispose();
            }

            #endregion Data

            // Create the autocomplete server-controls set
            if (false == isColumnReadOnly)
            {
                Pair<string, string> typeAheadKeyValues = RepositoryTools.GetTypeAheadKeyValues(pRc);
                if (null != typeAheadKeyValues)
                {
                    string dataKeyValues = typeAheadKeyValues.First;
                    if (StrFunc.IsFilled(typeAheadKeyValues.Second))
                        dataKeyValues += "|" + typeAheadKeyValues.Second;
                    txt.Attributes.Add("data-ta-kv", dataKeyValues);
                }
                // control hosting the actual selected value description (and the autocomplete functionality)
                if (pRc.IsAutoPostBack)
                    txt.Attributes.Add("onBlur", "__doPostBack(\'" + txt.ClientID + "\',\'\');");
            }
            else if (StrFunc.IsFilled(currentValue))
            {
                SetControlHyperLinkColumn(pPage, txt, referential, pRc, currentValue, Cst.ConsultationMode.ReadOnly);
            }
            pRc.ControlMain = txt;
            plhTypeAhead.Controls.Add(txt);
            plhTypeAhead.Controls.Add(txtvalue);
            return plhTypeAhead;
        }
        #endregion CreateControl_TypeAhead

        #region CreateControl_BooleanToCheckBox
        public static Control CreateControl_BooleanToCheckBox(RepositoryPageBase pPage, ReferentialColumn pRc, bool pIsEnabled)
        {
            bool isColumnReadOnly = pPage.IsColumnReadOnly(pRc) || (false == pIsEnabled);
            string ctrlID = RepositoryTools.GetID(pRc, Cst.CHK);

            BS_ClassicCheckBox ctrl = new BS_ClassicCheckBox(ctrlID, (false == isColumnReadOnly), pRc.Label, pRc.IsAutoPostBack);
            //SetJavascriptToColumnControl(pPage, chkRef, pReferential, pRrc);
            if (false == pPage.IsPostBack)
                ctrl.chk.Checked = RepositoryTools.GetCheckBoxValue(pPage.referential, pRc);
            return ctrl;
        }
        #endregion CreateControl_BooleanToCheckBox
        #region CreateControl_BooleanToRadioButton
        public static Control CreateControl_BooleanToRadioButton(RepositoryPageBase pPage, ReferentialColumn pRc, bool pIsEnabled)
        {
            bool isColumnReadOnly = pPage.IsColumnReadOnly(pRc) || (false == pIsEnabled);
            string ctrlID = RepositoryTools.GetID(pRc, Cst.CHK);

            RepeatDirection direction = pRc.DataType.value.EndsWith("v") ? RepeatDirection.Vertical : RepeatDirection.Horizontal;
            // GLOP ligne à retirer
            direction = RepeatDirection.Horizontal;
            BS_RadioButton ctrl = new BS_RadioButton(ctrlID, (false == isColumnReadOnly), pRc.Label, pRc.IsAutoPostBack, direction);
            SetJavascriptToColumnControl(pPage, ctrl, pPage.referential, pRc);
            if (false == pPage.IsPostBack)
            {
                bool isChecked = RepositoryTools.GetCheckBoxValue(pPage.referential, pRc);
                ctrl.rbPrimary.Checked = isChecked;
                ctrl.rbSecondary.Checked = (false == isChecked);
            }
            pRc.ControlMain = ctrl.rbPrimary;
            return ctrl;
        }
        #endregion CreateControl_BooleanToRadioButton
        #region CreateControl_Comment
        public static Control CreateControl_Comment(string pID)
        {
            string[] comments = pID.Split(new char[] { ';' },StringSplitOptions.RemoveEmptyEntries);
            Control hgcComment = null;
            if (ArrFunc.IsFilled(comments))
            {
                if (1 == comments.Length)
                {
                    hgcComment = new HtmlGenericControl("div");
                    (hgcComment as HtmlGenericControl).Attributes.Add("class", "comment");
                    (hgcComment as HtmlGenericControl).InnerHtml = Ressource.GetString(comments[0]);
                }
                else
                {
                    hgcComment = new Panel();
                    (hgcComment as Panel).Attributes.Add("class", "comment");
                    BS_TextBox txtComment = new BS_TextBox(Cst.TXT + comments[0], true, null);
                    txtComment.Attributes.Add("data-theme", Cst.TypeInformationMessage.Information);
                    txtComment.Text = Ressource.GetString(comments[0]);
                    txtComment.CssClass += " comment";
                    txtComment.Attributes.Add("data-content", Ressource.GetString(comments[1]));
                    if (3 == comments.Length)
                        txtComment.Attributes.Add("title", Ressource.GetString(comments[2]));
                    hgcComment.Controls.Add(txtComment);
                }
            }
            return hgcComment;
        }
        #endregion CreateControl_Comment

        #region CreateControl_DropDownList
        public static Control CreateControl_DropDownList(RepositoryPageBase pPage, ReferentialColumn pRc, bool pIsEnabled)
        {
            PlaceHolder plhDropDownList = new PlaceHolder();

            Referential referential = pPage.referential;

            string argCurrentNonUpdatableValue = string.Empty;

            bool isForeignKey = pRc.IsForeignKeyField && StrFunc.IsFilled(referential.valueForeignKeyField);
            bool isColumnReadOnly = pPage.IsColumnReadOnly(pRc) || (false == pIsEnabled) || isForeignKey;
            string ctrlID = RepositoryTools.GetID(pRc, Cst.DDL);
            string currentValue = RepositoryTools.GetCurrentValue(referential, pRc);


            BS_DropDownList ddl = null;
            //if (pRrc.ColumnName == "ACTION" || pRrc.ColumnName == "STRATEGYTYPE")
            //{
            //    // TO DO
            //    //OptionGroupDropDownList optionGroupDDLRef = new OptionGroupDropDownList();
            //    //ddlRef = (WCDropDownList2)optionGroupDDLRef;
            //}
            //else
            if (true)
            {
                ddl = new BS_DropDownList(isColumnReadOnly);
                ddl.ID = ctrlID;
                ddl.EnableViewState = true;
                ddl.AutoPostBack = pRc.IsAutoPostBack;
                if (pRc.IsAutoPostBack)
                {
                    ddl.AutoPostBack = false;
                    JavaScript.PostBack(pPage);
                    JavaScript.OnCtrlChanged(pPage);
                    ddl.Attributes.Add("onchange", "OnCtrlChanged('" + ddl.UniqueID + "','');return false;");
                }
            }

            if (false == pIsEnabled)
                argCurrentNonUpdatableValue = currentValue;

            #region LoadDDL
            if (((pRc.ColumnName != "IDAINS") && (pRc.ColumnName != "IDAUPD")) || StrFunc.IsFilled(argCurrentNonUpdatableValue))
                RepositoryTools.LoadDDL(pPage, referential, pRc, ddl, referential.valueForeignKeyField, argCurrentNonUpdatableValue, pPage.isNewRecord);
            #endregion LoadDDL

            bool isFoundValue = pPage.IsPostBack;
            string selectedValue = currentValue;
            string warningMsg = string.Empty;
            string textSelectedValue = currentValue;


            if (isColumnReadOnly)
            {
                ddl.txtViewer.ID = ctrlID;

                if (((pRc.ColumnName != "IDAINS") && (pRc.ColumnName != "IDAUPD")) || StrFunc.IsFilled(argCurrentNonUpdatableValue))
                {
                    isFoundValue = FoundValue(false, ddl, null, selectedValue, pRc, ref textSelectedValue, ref warningMsg);
                    ddl.txtViewer.Text = (isFoundValue ? ddl.SelectedItem.Text : selectedValue);
                    ddl.txtViewer.Attributes.Add("data-value", isFoundValue ? ddl.SelectedItem.Value : selectedValue);
                }

                // Add HyperLink on control
                if (StrFunc.IsFilled(selectedValue))
                    SetControlHyperLinkColumn(pPage, ddl.txtViewer, referential, pRc, selectedValue, Cst.ConsultationMode.ReadOnly);
            }
            else
            {
                #region Init Value
                if (false == pPage.IsPostBack)
                {
                    if (referential.isNewRecord || (pRc.IsAdditionalData && referential.isNewDrExternal[pRc.ExternalFieldID]))
                    {
                        #region Default Value
                        if (pRc.IsForeignKeyField)
                            selectedValue = referential.valueForeignKeyField;
                        else
                        {
                            string defaultValue = string.Empty;
                            if (pRc.ExistsDefaultValue)
                                defaultValue = SessionTools.ReplaceDynamicConstantsWithValues(pRc.GetStringDefaultValue(referential.TableName));
                            else if (pRc.ExistsDefaultColumnName)
                            {
                                try
                                {
                                    //Default Value renseignée dans la méthode InitializeNewRow()
                                    defaultValue = referential.dataRow[pRc.DataField].ToString();
                                }
                                catch { }
                            }
                            if (StrFunc.IsEmpty(defaultValue) && pRc.IsMandatory && pRc.ExistsDDLType)
                            {
                                if (pRc.Relation[0].DDLType != null)
                                    defaultValue = ConfigurationManager.AppSettings["Spheres_ReferentialDefault_" + pRc.Relation[0].DDLType.Value];
                            }
                            selectedValue = defaultValue;
                        }
                        #endregion Default Value
                    }

                    if (((pRc.ColumnName != "IDAINS") && (pRc.ColumnName != "IDAUPD")) || StrFunc.IsFilled(argCurrentNonUpdatableValue))
                    {
                        isFoundValue = FoundValue(false, ddl, null, selectedValue, pRc, ref textSelectedValue, ref warningMsg);
                    }
                }
                #endregion Init Value

                #region Init JS
                RepositoryTools.SetJavascriptToColumnControl(pPage, ddl, referential, pRc);
                #endregion Init JS
            }

            pRc.ControlMain = ddl;
            plhDropDownList.Controls.Add(ddl);
            return plhDropDownList;
        }
        #endregion CreateControl_DropDownList
        #region CreateControl_Identity
        public static Control CreateControl_Identity(RepositoryPageBase pPage, ReferentialColumn pRc, bool pIsEnabled)
        {
            BS_TextBox txt = new BS_TextBox();
            txt.ID = RepositoryTools.GetID(pRc, Cst.TXT);
            txt.Enabled = pIsEnabled;
            txt.Text = string.Empty;
            if (false == pPage.IsPostBack)
            {
                if (pRc.ExistsRelation)
                    txt.Text = pPage.referential.dataRow[pRc.IndexColSQL + 1].ToString();
                else
                    txt.Text = pRc.GetStringValue(pPage.referential.dataRow[pRc.DataField]);
            }
            pRc.ControlMain = txt;
            return txt;
        }
        #endregion CreateControl_Identity
        #region CreateControl_Label
        public static Control CreateControl_Label(RepositoryPageBase pPage, ReferentialColumn pRc, bool pIsEnabled, string pToolTip)
        {
            BS_Label lbl = new BS_Label();
            lbl.ID = RepositoryTools.GetID(pRc, Cst.LBL);
            lbl.Enabled = pIsEnabled;

            bool isBoolean = TypeData.IsTypeBool(pRc.DataType.value);
            if (false == isBoolean)
                lbl.Text = pRc.Label;
            else
                lbl.Text = " ";
            if (StrFunc.IsEmpty(pRc.Label.Trim()) || isBoolean)
                lbl.CssClass += " empty";

            if (false == pToolTip.Trim().ToLower().Equals(pRc.Label.ToLower()))
                lbl.ToolTip = pToolTip;

            // Utilisation pour affichage du nom de la colonne SQL (via JS)
            AddColumnNameInfo(pRc, lbl);
            return lbl;
        }
        #endregion CreateControl_Label
        #region CreateControl_TextBox
        public static Control CreateControl_TextBox(RepositoryPageBase pPage, ReferentialColumn pRc, bool pIsEnabled)
        {
            Referential referential = pPage.referential;
            bool isColumnReadOnly = pPage.IsColumnReadOnly(pRc) || (false == pIsEnabled);

            bool isTextBoxModeMultiLine = (pRc.TextMode == TextBoxMode.MultiLine.ToString()) || (TypeData.IsTypeString(pRc.DataTypeEnum) && (1000 <= pRc.Length));

            string ctrlID = RepositoryTools.GetID(pRc, Cst.TXT);


            #region Validator
            List<Validator> lstValidators = null;
            if (false == isColumnReadOnly)
                lstValidators = GetValidators(pRc);
            #endregion Validator

            Control ctrl;
            BS_TextBox txt;
            if (TypeData.IsTypeDateOrDateTime(pRc.DataTypeEnum))
            {
                ctrl = new BS_TextBoxDate(ctrlID, isColumnReadOnly, pRc.DataTypeEnum.ToString(), lstValidators);
                txt = (ctrl as BS_TextBoxDate).Date;
            }
            else
            {
                ctrl = new BS_TextBox(ctrlID, isColumnReadOnly, lstValidators);
                txt = ctrl as BS_TextBox;
            }

            #region Data
            string data = string.Empty;
            bool isReadData = false;
            if (false == pPage.IsPostBack)
            {
                isReadData = true;
            }
            else
            {
                isReadData = (referential.isLookReadOnly);
                //Lors d'un postback, lorsque qu'un contrôle est readOnly alors ASP affecte .text avec la valeur existante dans le viewstate (ASP écrase notamment la valeur lu sur le client) 
                //Sauf que le viewState ne contient rien puisque les propriétés du contrôles sont affectés avant même que le contrôle soit ajouté à la page
                //=> donc il faut relire la donnée systématiquement
                if (false == isReadData)
                {
                    isReadData = (false == pIsEnabled);
                    //Lors d'un postback, lorsque qu'un contrôle est disabled alors ASP affecte .text avec  la valeur existante dans le viewstate, 
                    // ASP ne lit même pas la valeur existante sur le client 
                    //Sauf que le viewState ne contient rien puisque les propriétés du contrôles sont affectés avant même que le contrôle soit ajouté à la page
                    //=> donc il faut relire la donnée systématiquement
                }

                if (false == isReadData)
                {
                    // RD 20130322 [18515] 
                    // Lors d'un postback, lorsque qu'un control est "caché en mode Light" alors le .text n'est pas valorisé:
                    // - Soit comme ci-dessus, le viewstate ne contient pas la valeur du contol !!!
                    // - Soit pour une autre raison que je n'arrive pas à expliquer
                    // => donc il faut relire la donnée systématiquement
                    isReadData = (pRc.IsHideOnLightDisplaySpecified && pRc.IsHideOnLightDisplay);
                }
            }
            if (isReadData)
            {
                if (referential.isNewRecord || (pRc.IsAdditionalData && referential.isNewDrExternal[pRc.ExternalFieldID]))
                {
                    //Default Value
                    data = pRc.GetStringDefaultValue(referential.TableName);
                }
                else
                {
                    Object objData;
                    if (pRc.IsAdditionalData)
                        objData = (referential.drExternal[pRc.ExternalFieldID])[pRc.ColumnName].ToString();
                    else
                        objData = referential.dataRow[pRc.DataField];

                    data = pRc.GetStringValue(objData);

                    if (StrFunc.IsEmpty(data))
                    {
                        string defaultValue = pRc.GetStringDefaultValue(referential.TableName);
                        if (Cst.AUTOMATIC_COMPUTE.ToString() == defaultValue)
                            data = defaultValue;
                    }
                }
            }
            #endregion Data

            txt.EnableViewState = true;
            txt.AutoPostBack = pRc.IsAutoPostBack;
            txt.ReadOnly = isColumnReadOnly;

            //SetJavascriptToColumnControl(pPage, txtRef, pReferential, pRrc);

            #region Ressource
            if ((pRc.IsResourceSpecified && pRc.IsResource.IsResource) || (Cst.AUTOMATIC_COMPUTE.ToString() == data))
                data = Ressource.GetString(data, true);
            #endregion

            txt.Text = data;

            #region TextBoxMode.Password
            if (pRc.TextMode == TextBoxMode.Password.ToString())
            {
                txt.TextMode = TextBoxMode.Password;
                data = Cst.StringForTextBoxModePassword;
                txt.Attributes["value"] = data;
            }
            #endregion TextBoxMode.Password

            #region TextBoxMode.MultiLine
            if (isTextBoxModeMultiLine)
            {
                txt.TextMode = TextBoxMode.MultiLine;
                int txtHeight = 50;
                try
                {
                    txtHeight = Convert.ToInt32(pRc.InputHeight);
                }
                catch
                { }
                txt.Height = Unit.Point(txtHeight);
            }
            #endregion

            pRc.ControlMain = txt;
            return ctrl;
        }
        #endregion CreateControl_TextBox
        #region CreateControl_TextArea
        public static Control CreateControl_TextArea(RepositoryPageBase pPage, ReferentialColumn pRc, bool pIsEnabled)
        {
            Referential referential = pPage.referential;
            bool isColumnReadOnly = pPage.IsColumnReadOnly(pRc) || (false == pIsEnabled);
            string ctrlID = RepositoryTools.GetID(pRc, Cst.TXT);

            Control ctrl = new BS_TextArea(ctrlID, (false == isColumnReadOnly));

            #region Data
            string data = string.Empty;
            bool isReadData = false;
            if (false == pPage.IsPostBack)
            {
                isReadData = true;
            }
            else
            {
                isReadData = referential.isLookReadOnly || (false == pIsEnabled) || (pRc.IsHideOnLightDisplaySpecified && pRc.IsHideOnLightDisplay);
            }
            if (isReadData)
            {
                if (referential.isNewRecord || (pRc.IsAdditionalData && referential.isNewDrExternal[pRc.ExternalFieldID]))
                {
                    //Default Value
                    data = pRc.GetStringDefaultValue(referential.TableName);
                }
                else
                {
                    Object objData;
                    if (pRc.IsAdditionalData)
                        objData = (referential.drExternal[pRc.ExternalFieldID])[pRc.ColumnName].ToString();
                    else
                        objData = referential.dataRow[pRc.DataField];

                    data = pRc.GetStringValue(objData);

                    if (StrFunc.IsEmpty(data))
                    {
                        string defaultValue = pRc.GetStringDefaultValue(referential.TableName);
                        if (Cst.AUTOMATIC_COMPUTE.ToString() == defaultValue)
                            data = defaultValue;
                    }
                }
            }
            #endregion Data

            ctrl.EnableViewState = true;

            #region Ressource
            if ((pRc.IsResourceSpecified && pRc.IsResource.IsResource) || (Cst.AUTOMATIC_COMPUTE.ToString() == data))
                data = Ressource.GetString(data, true);
            #endregion

            (ctrl as BS_TextArea).text = data;
            int height = 50;
            try
            {
                height = Convert.ToInt32(pRc.InputHeight);
            }
            catch
            { }
            (ctrl as BS_TextArea).height = height;
            pRc.ControlMain = ctrl as Panel;
            return ctrl;
        }
        #endregion CreateControl_TextBox



        #region GetValidators
        public static List<Validator> GetValidators(ReferentialColumn pRc)
        {
            return ControlsTools.GetValidators(pRc.IsMandatory, pRc.MsgErrRequiredField, pRc.RegularExpression, pRc.MsgErr, pRc.DataType.value);
        }
        #endregion GetValidators

        /// <summary>
        ///  Modification de la connexion {pCs} en fonction de {pReferential} 
        ///  <para>par exemple prise en compte du timeout lorsqu'il est renseigné</para>
        /// </summary>
        /// <param name="pCs">ConectionString en entrée</param>
        /// <param name="pReferential"></param>
        /// <returns></returns>
        // FI 20170531 [23206] Add Method
        public static string AlterConnectionString(string pCs, Referential pReferential)
        {
            string ret = pCs;
            if (pReferential.TimeoutSpecified && (0 < pReferential.Timeout))
            {
                int timeout = CSTools.GetTimout(ret);
                timeout = Math.Max(timeout, pReferential.Timeout);
                ret = CSTools.SetTimeOut(ret, timeout);
            }
            return ret;
        }

        #endregion Methods
    }

    #region GroupByExtensions
    /// <summary>
    /// helper class containing all the extensions targeting the group by sql command, for the referentiel namespace.
    /// These extentions use a group by string formatted with special characters. Any group by field must be encapsulated with {}. 
    /// Ex: group by {testfield}, {testfield2}
    /// </summary>
    public static class GroupByExtensions
    {
        static Regex parseSpecialCharacters = new Regex(@"\s*{([\s\w\d:_\.,""'-@/\(\)\[\]]+)}\s*", RegexOptions.IgnoreCase);
        // EG 20160308 Migration vs2013
        //static string replace = "$1";

        public static string GetNormalizedGroupByString(this string pSQLGroupBy)
        {
            string[] fields = GetGroupByColumn(pSQLGroupBy);

            return fields.Aggregate((curr, next) => next != null ? String.Concat(curr, ",", next) : curr);
        }

        public static string[] GetGroupByColumn(this string pSQLGroupBy)
        {
            MatchCollection matches = parseSpecialCharacters.Matches(pSQLGroupBy);

            string[] fieldsgroupby = new string[matches.Count];

            for (int idx = 0; idx < matches.Count; idx++)
            {
                fieldsgroupby[idx] = matches[idx].Groups[1].Value;
            }

            return fieldsgroupby;
        }

    }
    #endregion GroupByExtensions

}
