#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{


    /// <summary>
    /// 
    /// </summary>
    /// FI 20140206 [19564] add Class 
    public sealed class RegulatoryTools
    {

        /// <summary>
        /// Retourne le Regulatory Office associé à l'acteur {pIdA}
        /// <para>Retourne le 1er acteur dans la hierarchie avec le role RegulatoryOffice</para>
        /// <para>Retourne 0 s'il n'existe pas</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public static int GetActorRegulatoryOffice(string pCS, int pIdA)
        {
            return GetActorRegulatoryOffice(pCS, null, pIdA);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public static int GetActorRegulatoryOffice(string pCS, IDbTransaction pDbTransaction, int pIdA)
        {
            int ret = 0;

            RegulatoryOffices offices = new RegulatoryOffices(pCS, pDbTransaction, pIdA);
            if (offices.Count > 0)
                ret = offices[0];

            return ret;
        }

        /// <summary>
        ///  Retourne les règles UTI,PUTI et l'émetteur spécifié sur un datarow de la table REGULATORY pour la filière ETD
        /// </summary>
        /// <param name="row"></param>
        /// <param name="oUTIRule">Retourne la règle pour les trades, Retourne null si non renseigné</param>
        /// <param name="oPUTIRule">Retourne la règle pour les positions, null si non renseigné</param>
        /// <param name="oIssuer">Retourne le namespace, Retourne null si non renseigné</param>
        public static void GetETDRuleAndIssuer(DataRow row, out Nullable<UTIRule> oUTIRule, out  Nullable<UTIRule> oPUTIRule, out Pair<UTIIssuerIdent, string> oIssuer)
        {
            oUTIRule = null;
            oPUTIRule = null;
            oIssuer = null;

            string utiRuleRow = (Convert.IsDBNull(row["EMIRUTIETDRULE"]) ? string.Empty : row["EMIRUTIETDRULE"].ToString());
            if (StrFunc.IsFilled(utiRuleRow))
                oUTIRule = (UTIRule)Enum.Parse(typeof(UTIRule), utiRuleRow);

            string putiRuleRow = (Convert.IsDBNull(row["EMIRPUTIETDRULE"]) ? string.Empty : row["EMIRPUTIETDRULE"].ToString());
            if (StrFunc.IsFilled(putiRuleRow))
                oPUTIRule = (UTIRule)Enum.Parse(typeof(UTIRule), putiRuleRow);

            string issuerRow = (Convert.IsDBNull(row["EMIRETDISSUER"]) ? string.Empty : row["EMIRETDISSUER"].ToString());
            if (StrFunc.IsFilled(issuerRow))
                oIssuer = new Pair<UTIIssuerIdent, string>(UTIIssuerIdent.ISSUER, issuerRow);
        }

        /// <summary>
        /// Retourne la règle et l'émetteur par défaut associés à la chambre {pIdA}
        /// <para>Retourne true si la chambre est gérée (si la chambre dispose dans Spheres d'un algorithme de calcul des UTI)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUTIRule"></param>
        /// <param name="pIdA">Représente une chambre</param>
        /// <param name="opUTIRule"></param>
        /// <param name="opIssuer"></param>
        /// <returns></returns>
        ///FI 20140605 [XXXXX] modification 
        ///PL 20151029 add L2
        ///PL 20160427 [22107] EUREX C7 3.0 Release - New signature, Nullable<UTIRule> pUTIRule
        /// EG 20180307 [23769] Gestion dbTransaction
        public static Boolean GetDefaultCCPUTIRule(string pCS, IDbTransaction pDbTransaction, Nullable<UTIRule> pUTIRule, int pIdA, out Nullable<UTIRule> opUTIRule, out Pair<UTIIssuerIdent, string> opIssuer)
        {
            if (pIdA <= 0)
                throw new ArgumentException(StrFunc.AppendFormat("Value:{0} for argument pIdA is not valid", pIdA.ToString()));

            opUTIRule = null;
            opIssuer = null;

            Boolean isOk = false;
            SQL_Actor sqlActor = new SQL_Actor(pCS, pIdA)
            {
                DbTransaction = pDbTransaction
            };

            if (sqlActor.LoadTable(new string[] { "BIC" }))
            {
                isOk = true; //PL 20160427 Add (A priori ligne de code manquante... eu égard au Summary)

                //NB: On considère par défault la règle la plus récente en vigueur sur la Clearing House.
                switch (sqlActor.BIC)
                {
                    case UTIBuilder.BIC_CCeG:
                        //opUTIRule = (pUTIRule == UTIRule.CCP ? UTIRule.CCEG : UTIRule.CCEG_L2);
                        if (!pUTIRule.HasValue)
                        {
                            // FI 20240425[26593] UTI / PUTI REFIT => default use of REFIT
                            //GetDefaultCCPUTIRule(UTIBuilder.BIC_CCeG, out opUTIRule, UTIRule.CCEG_L2);
                            GetDefaultCCPUTIRule(pCS, UTIBuilder.BIC_CCeG, out opUTIRule, UTIRule.CCEG_REFIT);
                        }
                        else if (pUTIRule == UTIRule.CCP_L2)
                            opUTIRule = UTIRule.CCEG_L2;
                        else if (pUTIRule == UTIRule.CCP_REFIT) // FI 20240425[26593] UTI / PUTI REFIT => règle REFIT 
                            opUTIRule = UTIRule.CCEG_REFIT;
                        else
                            opUTIRule = UTIRule.CCEG;

                        opIssuer = new Pair<UTIIssuerIdent, string>(UTIIssuerIdent.BIC, sqlActor.BIC);
                        break;
                    case UTIBuilder.BIC_EUREX:
                        //PL 20160426 [22107] EUREX C7 3.0 Release
                        //opUTIRule = (pUTIRule == UTIRule.CCP ? UTIRule.EUREX : UTIRule.EUREX_L2); 
                        if (!pUTIRule.HasValue)
                        {
                            // FI 20240425[26593] UTI / PUTI REFIT => default use of REFIT
                            //GetDefaultCCPUTIRule(UTIBuilder.BIC_EUREX, out opUTIRule, UTIRule.EUREX_L2_C7_3);
                            GetDefaultCCPUTIRule(pCS, UTIBuilder.BIC_EUREX, out opUTIRule, UTIRule.EUREX_REFIT);
                        }
                        else if (pUTIRule == UTIRule.CCP_L2)
                            opUTIRule = UTIRule.EUREX_L2;
                        else if (pUTIRule == UTIRule.CCP_REFIT) // FI 20240425[26593] UTI / PUTI REFIT => règle REFIT 
                            opUTIRule = UTIRule.EUREX_REFIT;
                        else
                            opUTIRule = UTIRule.EUREX;

                        opIssuer = new Pair<UTIIssuerIdent, string>(UTIIssuerIdent.BIC, sqlActor.BIC);

                        break;
                    case UTIBuilder.BIC_LCHClearnetSA:
                        //opUTIRule = (pUTIRule == UTIRule.CCP ? UTIRule.LCHCLEARNETSA : UTIRule.LCHCLEARNETSA_L2);
                        if (!pUTIRule.HasValue)
                        {
                            // FI 20240425[26593] UTI / PUTI REFIT => default use of REFIT
                            //GetDefaultCCPUTIRule(UTIBuilder.BIC_LCHClearnetSA, out opUTIRule, UTIRule.LCHCLEARNETSA_L2);
                            GetDefaultCCPUTIRule(pCS, UTIBuilder.BIC_LCHClearnetSA, out opUTIRule, UTIRule.LCHCLEARNETSA_REFIT);
                        }
                        else if (pUTIRule == UTIRule.CCP_L2)
                            opUTIRule = UTIRule.LCHCLEARNETSA_L2;
                        else if (pUTIRule == UTIRule.CCP_REFIT) // FI 20240425[26593] UTI / PUTI REFIT => règle REFIT 
                            opUTIRule = UTIRule.LCHCLEARNETSA_REFIT;
                        else
                            opUTIRule = UTIRule.LCHCLEARNETSA;

                        opIssuer = new Pair<UTIIssuerIdent, string>(UTIIssuerIdent.BIC, sqlActor.BIC);
                        break;

                    case UTIBuilder.BIC_EURONEXTCLEARING: // FI 20240704 [WI987] Add
                        if (!pUTIRule.HasValue)
                            GetDefaultCCPUTIRule(pCS, UTIBuilder.BIC_EURONEXTCLEARING, out opUTIRule, UTIRule.EURONEXTCLEARING_REFIT);
                        else
                            opUTIRule = UTIRule.EURONEXTCLEARING_REFIT;

                        opIssuer = new Pair<UTIIssuerIdent, string>(UTIIssuerIdent.BIC, sqlActor.BIC);
                        break;

                    default:
                        isOk = false;
                        break;
                }
            }
            return isOk;
        }
        /// <summary>
        /// Retourne la règle par défaut associés à la chambre {pBIC}
        /// </summary>
        /// <param name="pBIC"></param>
        /// <param name="opUTIRule"></param>
        /// <param name="pDefaultUTIRule"></param>
        /// <returns></returns>
        //PL 20160427 [22107] EUREX C7 3.0 Release - New Method 
        public static Boolean GetDefaultCCPUTIRule(string pCS, string pBIC, out Nullable<UTIRule> opUTIRule, UTIRule pDefaultUTIRule)
        {
            Boolean isOk = false; 
            opUTIRule = pDefaultUTIRule;
            
            string prefix = string.Empty;
            switch (pBIC)
            {
                case UTIBuilder.BIC_CCeG:
                    prefix = "CCEG";
                    break;
                case UTIBuilder.BIC_EUREX:
                    prefix = "EUREX";
                    break;
                case UTIBuilder.BIC_LCHClearnetSA:
                    prefix = "LCHCLEARNETSA";
                    break;
                case UTIBuilder.BIC_EURONEXTCLEARING:  // FI 20240704 [WI987] Add
                    prefix = "EURONEXTCLEARING";
                    break;
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                //NB: On considère comme règle la plus récente, celle "classée" en dernier.
                string lastRule = null;
                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                //ExtendEnum extendEnums = ExtendEnumsTools.ListEnumsSchemes["EMIRUTIETDRulesEnum"];
                ExtendEnum extendEnums = DataEnabledEnumHelper.GetDataEnum(pCS, "EMIRUTIETDRulesEnum");
                if ((null != extendEnums) && (null != extendEnums.item))
                {
                    foreach (ExtendEnumValue extendEnumValue in extendEnums.Sort("Value"))
                    {
                        if ((null != extendEnumValue) && (extendEnumValue.Value.StartsWith(prefix)))
                        {
                            isOk = true;
                            lastRule = extendEnumValue.Value;
                        }
                    }
                }
                if (isOk)
                    opUTIRule = (UTIRule)Enum.Parse(typeof(UTIRule), lastRule);
            }

            return isOk;
        }
        /// <summary>
        /// Retoune le paramétrage associé à un acteur REGULATORYOFFICE
        /// <para>Retourne null s'il n'existe aucun paramétrage</para>
        /// <para>Lecture de la table REGULATORY</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        /// FI 20140206 [19564] add Method 
        /// EG 20180307 [23769] Gestion dbTransaction
        public static DataRow GetDataRowRegulatory(string pCS, IDbTransaction pDbTransaction, int pIdA)
        {
            DataRow dr = null;

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            string query = @"select EMIRTRETD,IDIOTASK_ETD,EMIRUTIETDRULE,EMIRPUTIETDRULE,EMIRETDISSUER, EMIRTROTC,IDIOTASK_OTC,EMIRUTIOTCRULE,EMIRPUTIOTCRULE,EMIROTCISSUER 
            from dbo.REGULATORY where IDA = @IDA";
            QueryParameters queryParameters = new QueryParameters(pCS, query, parameters);

            DataSet dsResult = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            DataTable dtTable = dsResult.Tables[0];
            if (ArrFunc.IsFilled(dtTable.Rows))
                dr = dtTable.Rows[0];

            return dr;
        }


        /// <summary>
        /// Retourne true s'il existe un enregistrement dans la table REGULATORY pour l'acteur <paramref name="pIdARegulatoryOffice"/>
        /// <para>Obtient la règle et l'issuer associés à un acteur REGULATORYOFFICE</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdARegulatoryOffice"></param>
        /// <param name="pUtiType"></param>
        /// <param name="pRule"></param>
        /// <param name="pIssuer"></param>
        public static Boolean GetAttributesRegulatory(string pCS, int pIdARegulatoryOffice, UTIType pUtiType, out Nullable<UTIRule> pRule, out Pair<UTIIssuerIdent, string> pIssuer)
        {
            return GetAttributesRegulatory(pCS, null, pIdARegulatoryOffice, pUtiType, out pRule, out pIssuer);
        }
        /// <summary>
        /// Retourne true s'il existe un enregistrement dans la table REGULATORY pour l'acteur <paramref name="pIdARegulatoryOffice"/>
        /// <para>Obtient la règle et l'issuer associés à un acteur REGULATORYOFFICE</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdARegulatoryOffice"></param>
        /// <param name="pUtiType"></param>
        /// <param name="pRule"></param>
        /// <param name="pIssuer"></param>
        public static Boolean GetAttributesRegulatory(string pCS, IDbTransaction pDbTransaction, int pIdARegulatoryOffice, UTIType pUtiType, out Nullable<UTIRule> pRule, out Pair<UTIIssuerIdent, string> pIssuer)
        {
            pRule = null;
            pIssuer = null;

            DataRow row = RegulatoryTools.GetDataRowRegulatory(pCS, pDbTransaction, pIdARegulatoryOffice);
            Boolean isOk = (null != row);
            if (isOk)
            {
                GetETDRuleAndIssuer(row, out UTIRule? rowUTIRule, out UTIRule? rowPUTIRule, out Pair<UTIIssuerIdent, string> rowIssuer);
                switch (pUtiType)
                {
                    case UTIType.UTI:
                        pRule = rowUTIRule;
                        break;
                    case UTIType.PUTI:
                        pRule = rowPUTIRule;
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("UTIType :{0} is not implemented", pUtiType.ToString()));
                }
                pIssuer = rowIssuer;
            }

            return isOk;
        }
    }
}