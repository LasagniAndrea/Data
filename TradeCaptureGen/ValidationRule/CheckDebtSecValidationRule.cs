#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Business;
using EfsML.Interface;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Validation de la saisie des titres
    /// </summary>
    public class CheckDebtSecValidationRule : CheckTradeInputValidationRuleBase
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly DebtSecInput _Input;


        #region Constructors
        // EG 20171115 Upd Add CaptureSessionInfo
        public CheckDebtSecValidationRule(DebtSecInput pDebtSecInput, Cst.Capture.ModeEnum pCaptureModeEnum, User pUser)
            : base(pDebtSecInput, pCaptureModeEnum, pUser)
        {
            _Input = pDebtSecInput;
        }
        
        #endregion constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCheckMode"></param>
        /// <returns></returns>
        /// FI 20160517 [22148] Modify
        public override bool ValidationRules(string pCS, IDbTransaction pDbTransaction, CheckModeEnum pCheckMode)
        {

            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();

            // FI 20160517 [22148] Add if puisque désormais un cas d'annulation il y a appel aux validations rules
            if (Cst.Capture.IsModeNewCapture(CaptureMode) ||
                Cst.Capture.IsModeUpdate(CaptureMode))
            {
                CheckValidationRule_Party();
                CheckValidationRule_CodeIsin(pCS);
                CheckValidationRule_CodeIsinUnique(pCS);
            }

            return ArrFunc.IsEmpty(m_CheckConformity);
        }


        /// <summary>
        /// 
        /// </summary>
        private void CheckValidationRule_CodeIsin(string pCS)
        {
            string codeIsin = GetCodeIsin(pCS);
            //
            bool isToCheck = IsToCheck("VRDSEISIN");
            if (isToCheck && StrFunc.IsFilled((String)SqlInstrument.GetFirstRowColumnValue("VRDSEISINLST")))
            {
                string[] codeIsinIgnore = StrFunc.QueryStringData.StringListToStringArray((String)SqlInstrument.GetFirstRowColumnValue("VRDSEISINLST"));
                isToCheck = (false == ArrFunc.ExistInArray(codeIsinIgnore, codeIsin));
            }
            //
            if (isToCheck)
            {
                if (StrFunc.IsEmpty(codeIsin))
                {
                    SetValidationRuleError("Msg_ValidationRule_VRDSEISIN");
                }
                else
                {

                    bool isOk;
                    // Verification de la taile du CODEISIN
                    if (BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("VRDSEISINLEN")))
                    {
                        isOk = StrFunc.IsFilled(codeIsin) && (codeIsin.Length == 12);
                        if (false == isOk)
                            SetValidationRuleError("Msg_ValidationRule_VRDSEISINLEN", new string[] { codeIsin });
                    }
                    // Verification du pays du CODEISIN
                    if (BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("VRDSEISINCOUNTRY")))
                    {
                        isToCheck = (false == (StrFunc.IsFilled(codeIsin) && codeIsin.StartsWith("X")));
                        if (isToCheck)
                        {
                            string country = string.Empty;
                            isOk = (2 <= codeIsin.Length);
                            if (isOk)
                            {
                                country = codeIsin.Substring(0, 2);
                                SQL_Country sqlCountry = new SQL_Country(pCS, SQL_Country.IDType.Iso3166Alpha2, country, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                isOk = sqlCountry.IsFound;
                            }
                            //
                            if (false == isOk)
                                SetValidationRuleError("Msg_ValidationRule_VRDSEISINCOUNTRY", new string[] { country, codeIsin });
                        }
                    }
                    // Verification avec la clef 
                    if (BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("VRDSEISINKEY")))
                    {
                        isOk = true;
                        if (false == isOk)
                            SetValidationRuleError("Msg_ValidationRule_VRDSEISINKEY", new string[] { codeIsin });
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20120614 [17904] Utilisation de la table TRADE_ASSET
        /// EG 20150327 [20513] BANCAPERTA Options sur titre ISINCODE remplace ISIN
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void CheckValidationRule_CodeIsinUnique(string pCS)
        {
            string codeIsin = GetCodeIsin(pCS);

            bool isToCheck = IsToCheck("VRDSEISINUNIQUE");
            if (isToCheck && StrFunc.IsFilled((String)SqlInstrument.GetFirstRowColumnValue("VRDSEISINUNIQUELST")))
            {
                string[] codeIsinIgnore = StrFunc.QueryStringData.StringListToStringArray((String)SqlInstrument.GetFirstRowColumnValue("VRDSEISINUNIQUELST"));
                isToCheck = (false == ArrFunc.ExistInArray(codeIsinIgnore, codeIsin));
                //droits non "isinés" à regrouper sous FR0000880007
                //fonds communs de placement résidents non "isinés" à regrouper sous FR0007999990
                if (isToCheck)
                    isToCheck = ((codeIsin != "FR0000880007") && (codeIsin != "FR00007999990"));
            }
            //
            bool isNew = Cst.Capture.IsModeNewCapture(CaptureMode);
            bool isUpd = Cst.Capture.IsModeUpdateOrUpdatePostEvts(CaptureMode);
            //
            if (isToCheck)
                isToCheck = (isNew || isUpd);
            //                
            if (isToCheck)
            {
                DataParameters parameters = new DataParameters();
                /// EG 20150327 [20513]
                parameters.Add(new DataParameter(pCS, "ISINCODE", DbType.AnsiString, 255), codeIsin);
                parameters.Add(new DataParameter(pCS, "FAMILY", DbType.AnsiString, 255), "DSE");
                parameters.Add(new DataParameter(pCS, "DEACTIV", DbType.AnsiString, 255), Cst.StatusActivation.DEACTIV.ToString());
                if (isUpd)
                    parameters.Add(new DataParameter(pCS, "@IDT", DbType.Int32), _Input.IdT);

                //FI 20120614 [17904] 
                string sqlSelect = @"select tr.IDENTIFIER
                from dbo.TRADEASSET tasset
                inner join dbo.TRADE tr on (tr.IDT = tasset.IDT)
                inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.FAMILY = @FAMILY)
                where (tasset.ISINCODE = @ISINCODE) and (tr.IDSTACTIVATION = @DEACTIV)";
                if (isUpd)
                    sqlSelect += " and (tr.IDT != @IDT)" + Cst.CrLf;

                QueryParameters qry = new QueryParameters(pCS, sqlSelect, parameters);
                object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(qry.Cs, 1, null), CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                bool isOk = (null == obj);
                if (false == isOk)
                {
                    string identifier = Convert.ToString(obj);
                    SetValidationRuleError("Msg_ValidationRule_VRDSEISINUNIQUE", new string[] { codeIsin, identifier });
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetCodeIsin(string pCS)
        {
            IDebtSecurity debtSecurity = (IDebtSecurity)_Input.DataDocument.CurrentProduct.Product;
            DebtSecurityContainer debtSec = new DebtSecurityContainer(debtSecurity);
            return debtSec.GetCodeIsin(pCS);
        }
    }
}