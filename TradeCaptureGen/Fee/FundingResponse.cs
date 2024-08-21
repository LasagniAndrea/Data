using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;

namespace EFS.TradeInformation
{
    public abstract class ResponseBase
    {
        #region Members
        private readonly string _cs;
        private readonly ProcessingBase _parent;
        // EG 20160404 Migration vs2013
        protected LevelStatusTools.LevelEnum _levelEnum;
        protected LevelStatusTools.StatusEnum _statusEnum;
        protected string _auditMessage;
        protected string _errorMessage;
        protected string _infoMessage;

        private IDataReader _dr;
        // EG 20150309 POC -BERKELEY
        //private string _shiftFormula, _longShiftFormula, _shortShiftFormula;
        private string _shiftFormula;
        #endregion Members

        #region Accessors
        public string CS
        {
            get { return _cs; }
        }
        public string AuditMessage
        {
            get { return _auditMessage; }
            set { _auditMessage = value; }
        }
        public bool AuditMessageSpecified
        {
            get { return StrFunc.IsFilled(_auditMessage); }
        }
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
        public bool ErrorMessageSpecified
        {
            get { return StrFunc.IsFilled(_errorMessage); }
        }
        public string InfoMessage
        {
            get { return _infoMessage; }
        }
        public bool InfoMessageSpecified
        {
            get { return StrFunc.IsFilled(_infoMessage); }
        }
        public LevelStatusTools.LevelEnum LevelEnum
        {
            get { return _levelEnum; }
        }
        public LevelStatusTools.StatusEnum StatusEnum
        {
            get { return _statusEnum; }
        }
        public ProcessingBase Parent
        {
            get { return _parent; }
        }
        protected IDataReader Dr
        {
            get { return _dr; }
        }
        protected string ShiftFormula
        {
            get { return _shiftFormula; }
        }

        protected Nullable<decimal> ShiftFormulaConverted
        {
            get 
            {
                Nullable<decimal> ret = null;
                try 
                {
                    EFS_Decimal convertedFormula = new EFS_Decimal(StrFunc.FmtDecimalToInvariantCulture(_shiftFormula));
                    if (0 != convertedFormula.DecValue)
                        ret = convertedFormula.DecValue;
                }
                catch { }
                return ret; 
            }
        }

        // EG 20150309 POC - BERKELEY Remove
        //protected string LongShiftFormula
        //{
        //    get { return _longShiftFormula; }
        //}
        //protected string ShortShiftFormula
        //{
        //    get { return _shortShiftFormula; }
        //}
        #endregion Accessors

        #region Constructors
        public ResponseBase(ProcessingBase pProcessing)
        {
            _parent = pProcessing;
            _cs = _parent.ScheduleRequest.CS;
            _dr = null;
        }
        #endregion Constructors    

        #region Methods
        /// <summary>
        /// Retourne l'enregistrement le "plus fin*", compatible avec l'environement ScheduleRequest (FeeRequest)
        /// <para>* voir Order By</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20141104 [20466] Modify
        /// FI 20170908 [23409] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180423 Analyse du code Correction [CA2200]
        protected bool LoadData(Cst.OTCml_TBL pTableName)
        {
            bool ret = false;

            FeeRequest scheduleRequest = Parent.ScheduleRequest;
            scheduleRequest.CS = CS;

            int savPartyB_Ida = scheduleRequest.PartyB.m_Party_Ida;
            int savPartyB_Idb = scheduleRequest.PartyB.m_Party_Idb;
            bool isRenewCustodian = false;

            DataParameters parameters;
            DataParameters fundingTypePamrameters = null;
            QueryParameters qry;
            StrBuilder sqlSelect;
            try
            {
                bool isFundingSearch = (pTableName == Cst.OTCml_TBL.FUNDINGSCHEDULE);
                bool isMarginSearch = (pTableName == Cst.OTCml_TBL.MARGINSCHEDULE);

                // EG 20150320 (POC] Filtre sur FundingType (si Borrowing PartyA doit être Seller)
                string sqlWhereFundingType = string.Empty;
                if (isFundingSearch)
                {
                    Cst.FundingType fundingType = ((FundingProcessing)Parent).FundingType;
                    fundingTypePamrameters = new DataParameters();
                    fundingTypePamrameters.Add(new DataParameter(CS, "FUNDINGTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),fundingType.ToString());
                    fundingTypePamrameters.Add(new DataParameter(CS, "FUNDINGTYPEALL", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Cst.FundingType.FundingAndBorrowing.ToString());
                    sqlWhereFundingType += OTCmlHelper.GetSQLComment("Funding Type");
                    sqlWhereFundingType += " and ({0}.FUNDINGTYPE in (@FUNDINGTYPE, @FUNDINGTYPEALL))" + Cst.CrLf;
                    if ((Cst.FundingType.Borrowing == fundingType) && (scheduleRequest.PartyA.m_Party_Side == TradeSideEnum.Buyer))
                    {
                        fundingTypePamrameters["FUNDINGTYPE"].Value = "NA";
                        fundingTypePamrameters["FUNDINGTYPEALL"].Value = "NA";
                    }
                }

                int idI = scheduleRequest.Product.IdI;
                int idI_Unl = (null != scheduleRequest.Product.GetUnderlyingAssetIdI()) ? (int)scheduleRequest.Product.GetUnderlyingAssetIdI() : 0;
                
                //int idDC = (null != scheduleRequest.sqlDerivativeContract) ? (int)scheduleRequest.sqlDerivativeContract.Id : 0;
                Pair <Cst.ContractCategory,int> contractId= null;
                if (null != scheduleRequest.Contract)
                    contractId = new Pair<Cst.ContractCategory, int>(scheduleRequest.Contract.First, scheduleRequest.Contract.Second.Id); 
  

                
                int idM = (null != scheduleRequest.SqlMarket) ? (int)scheduleRequest.SqlMarket.Id : 0;
                int idAsset = 0;
                
                //PL 20140924 Newness
                //------------------------------------------------------------------------------------------------------------------------------------------------------
                //WARNING: La méthode GetCriteriaJoinOnMATRIX() valorise le parameters @IDC sur la base de GetMainCurrencyAmount()
                //         Or, ici la devise utile est-celle de l'actif. Celle-ci est théoriquement par la suite également déversée dans la donnée utilisée par 
                //         GetMainCurrencyAmount(), mais cela est opéré après l'appel à la méthode courante.
                //         On écrase donc ici la valeur du parameters @IDC par la bonne donnée.
                string idC = scheduleRequest.Product.GetUnderlyingAssetIdC(CS, null);

                // FI 20141104 [20466] idC est écrasé par la devise cotée (cas du GBX qui est remplacé par GBP)
                if (StrFunc.IsFilled(idC))
                {
                    Tools.GetQuotedCurrency(CSTools.SetCacheOn(CS), null, SQL_Currency.IDType.IdC, idC, out string idcQuoted, out int? factor);
                    if (StrFunc.IsFilled(idcQuoted))
                        idC = idcQuoted;
                }
                // EG 20150310 _idAsset commun à FUNDING|MARGIN
                Nullable<int> _idAsset = scheduleRequest.Product.GetUnderlyingAssetId(CS);
                if (_idAsset.HasValue)
                    idAsset = _idAsset.Value;

                SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(CS, null, idI, false, SQL_Table.ScanDataDtEnabledEnum.Yes);
                // FI 20170908 [23409] use sqlContractCriteria
                //SQLDerivativeContractCriteria sqlDerivativeCriteria = new SQLDerivativeContractCriteria(CS, idDC, idM, SQL_Table.ScanDataDtEnabledEnum.Yes);
                SQLContractCriteria sqlContractCriteria = new SQLContractCriteria(CS, null, contractId, idM, SQL_Table.ScanDataDtEnabledEnum.Yes);

                SQLInstrCriteria sqlInstrCriteriaUnl = new SQLInstrCriteria(CS, null, idI_Unl, true, SQL_Table.ScanDataDtEnabledEnum.Yes);

                #region STEP 1: RELATIVE TO
                parameters = new DataParameters();
                if (null != fundingTypePamrameters)
                {
                    parameters.Add(fundingTypePamrameters["FUNDINGTYPE"]);
                    parameters.Add(fundingTypePamrameters["FUNDINGTYPEALL"]);
                }

                #region Select/Where
                string tblMATRIXRELTO = pTableName.ToString().Replace("SCHEDULE", "MATRIXRELTO");
                string colSHIFTFORMULA = (isFundingSearch ? ",mrt.LONG_SHIFTFORMULA,mrt.SHORT_SHIFTFORMULA" : ",mrt.SHIFTFORMULA");

                sqlSelect = new StrBuilder(SQLCst.SELECT);
                sqlSelect += "mrt.IDA_PARTYB_OUT,mrt.IDB_PARTYB_OUT" + colSHIFTFORMULA + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + tblMATRIXRELTO + " mrt" + Cst.CrLf;

                // FI 20170908 [23409] sqlContractCriteria à la place de sqlDerivativeCriteria
                // sqlSelect += scheduleRequest.GetCriteriaJoinOnMATRIX("mrt", sqlInstrCriteria, sqlInstrCriteriaUnl, sqlDerivativeCriteria, parameters);
                sqlSelect += scheduleRequest.GetCriteriaJoinOnMATRIX("mrt", sqlInstrCriteria, sqlInstrCriteriaUnl, sqlContractCriteria, parameters);
                sqlSelect += String.Format(sqlWhereFundingType, "mrt");
                #endregion

                #region Order
                sqlSelect += SQLCst.ORDERBY + Cst.CrLf;
                sqlSelect += "case mrt.TYPEPARTYB when 'Book' then 4 when 'GrpBook' then 3 when 'Actor' then 2 when 'GrpActor' then 1 else 0 end desc, " + Cst.CrLf;
                sqlSelect += "case mrt.TYPEPARTYA when 'Book' then 4 when 'GrpBook' then 3 when 'Actor' then 2 when 'GrpActor' then 1 else 0 end desc, " + Cst.CrLf;

                sqlSelect += "mrt.IDC desc, mrt.ASSETCATEGORY desc, " + Cst.CrLf;
                sqlSelect += "case mrt.TYPEINSTR_UNL when 'Instr' then 2 when 'GrpInstr' then 1 else 0 end desc, " + Cst.CrLf;
                // FI 20170908 [23409] gestion des valeurs DerivativeContract et CommodityContract
                sqlSelect += @"case mrt.TYPECONTRACT 
                                when 'DerivativeContract' then 4 
                                when 'CommodityContract' then 4 
                                when 'GrpContract' then 3 
                                when 'Market' then 2 
                                when 'GrpMarket' then 1 else 0 end desc, " + Cst.CrLf;
                sqlSelect += "case mrt.TYPEINSTR when 'Instr' then 2 when 'GrpInstr' then 1 else 0 end desc, " + Cst.CrLf;
                sqlSelect += "mrt.GPRODUCT desc";
                if (isFundingSearch)
                    sqlSelect += ", case mrt.FUNDINGTYPE when 'FundingBorrowing' then 0 else 1 end desc" + Cst.CrLf;
                #endregion

                parameters["IDC"].Value = idC;
                qry = new QueryParameters(CS, sqlSelect.ToString(), parameters);
                _dr = DataHelper.ExecuteReader(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                if (_dr.Read())
                {
                    //NB: Il existe un paramétrage de "Reconduction" d'un barème d'un Custodian externe au trade. 
                    //    On subsitue l'actuel Custodian (PartyB) par ce Custodian, afin d'effectue la recherche de barème sur la base de ce dernier.
                    isRenewCustodian = true;
                    scheduleRequest.PartyB.m_Party_Ida = Convert.ToInt32(_dr["IDA_PARTYB_OUT"]);
                    if (_dr["IDB_PARTYB_OUT"] != Convert.DBNull)
                    {
                        scheduleRequest.PartyB.m_Party_Idb = Convert.ToInt32(_dr["IDB_PARTYB_OUT"]);
                    }
                    else
                    {
                        scheduleRequest.PartyB.m_Party_Idb = 0;
                    }
                    // EG 20150309 Test Long/Short
                    if (isFundingSearch)
                    {
                        //_longShiftFormula = _dr["LONG_SHIFTFORMULA"].ToString();
                        //_shortShiftFormula = _dr["SHORT_SHIFTFORMULA"].ToString();
                        if (scheduleRequest.PartyA.m_Party_Side == TradeSideEnum.Buyer)
                            _shiftFormula = _dr["LONG_SHIFTFORMULA"].ToString();
                        else
                            _shiftFormula = _dr["SHORT_SHIFTFORMULA"].ToString();
                    }
                    else
                    {
                        _shiftFormula = _dr["SHIFTFORMULA"].ToString();
                    }
                }
                CloseDataReader();
                #endregion STEP 1

                #region STEP 2: SCHEDULE
                #region Select/Where
                string tblMATRIX = pTableName.ToString().Replace("SCHEDULE", "MATRIX");
                

                parameters = new DataParameters();
                if (null != fundingTypePamrameters)
                {
                    parameters.Add(fundingTypePamrameters["FUNDINGTYPE"]);
                    parameters.Add(fundingTypePamrameters["FUNDINGTYPEALL"]);
                }
                parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), idAsset);

                sqlSelect = new StrBuilder(SQLCst.SELECT);
                sqlSelect += "s." + OTCmlHelper.GetColunmID(pTableName.ToString()) + ", s.IDENTIFIER, s.DISPLAYNAME, " + Cst.CrLf;
                sqlSelect += "s.IDA, " + Cst.CrLf;
                //sqlSelect += "s.GPRODUCT, s.TYPEINSTR, s.IDINSTR, s.TYPECONTRACT, s.IDCONTRACT, " + Cst.CrLf;
                //sqlSelect += "s.ASSETCATEGORY, s.IDC, s.TYPEINSTR_UNL s.IDINSTR_UNL, " + Cst.CrLf;

                if (isFundingSearch)
                {
                    //Long/Short values
                    string sqlLongShort = "'{0}' as PREFIX," + Cst.CrLf;
                    sqlLongShort += "s.{0}_FIXEDRATE, s.{0}_IDASSET, s.{0}_MULTIPLIER, s.{0}_SPREAD, isnull(s.{0}_DCF,ri_{1}.DCF) as {0}_DCF, " + Cst.CrLf;
                    sqlLongShort += "isnull(s.{0}_PERIODMLTPFIXINGOFFSET,ri_{1}.PERIODMLTPFIXINGOFFSET) as {0}_PERIODMLTPFIXINGOFFSET,isnull(s.{0}_PERIODFIXINGOFFSET,ri_{1}.PERIODFIXINGOFFSET) as {0}_PERIODFIXINGOFFSET, " + Cst.CrLf;
                    sqlLongShort += "isnull(s.{0}_DAYTYPEFIXINGOFFSET,ri_{1}.DAYTYPEFIXINGOFFSET) as {0}_DAYTYPEFIXINGOFFSET,isnull(s.{0}_RELATIVETOFIXINGDT,ri_{1}.RELATIVETOFIXINGDT) as {0}_RELATIVETOFIXINGDT, " + Cst.CrLf;
                    //sqlLongShort += "ri_{1}.IDISDA as {0}_IDISDA, ari_{1}.PERIOD as {0}_TENOR, ari_{1}.PERIODMLTP as {0}_TENORMLTP, " + Cst.CrLf;

                    if (scheduleRequest.PartyA.m_Party_Side == TradeSideEnum.Buyer)
                        sqlSelect += String.Format(sqlLongShort, "LONG", "l");
                    else
                        sqlSelect += String.Format(sqlLongShort, "SHORT", "s");

                    sqlSelect += "m.ISNOTIONALRESET, m.PERIOD, m.PERIODMLTP" + Cst.CrLf;
                }
                else if (isMarginSearch)
                {
                    sqlSelect += "s.MARGINTYPE,s.MARGINVALUE,s.CROSSMARGINVALUE, s.ISAPPLYMIN" + Cst.CrLf;
                    //TBD: Reste à gérer les colonnes m.LOWHIGHBASIS, m.LOWVALUE et m.HIGHVALUE
                }

                sqlSelect += SQLCst.FROM_DBO + tblMATRIX + " m" + Cst.CrLf;
                /*
                //Jointure sur xxxxxSCHEDULE via IDxxxxxGSCHEDULE (alias s_id)
                sqlSelect += SQLCst.INNERJOIN_DBO + pTableName.ToString() + " s_id on (s_id." + colPK + "=m." + colPK + ")" + Cst.CrLf;
                //Jointure sur xxxxxSCHEDULE via IDENTIFIER (alias s)
                sqlSelect += SQLCst.INNERJOIN_DBO + pTableName.ToString() + " s on (s.IDENTIFIER=s_id.IDENTIFIER)" + Cst.CrLf;
                 */

                //FI 20180426 [23929] modification de la jointure utilisation de la colonne  SCHEDULELIBRARY
                sqlSelect += SQLCst.INNERJOIN_DBO + pTableName.ToString() + " s on (s.SCHEDULELIBRARY=m.SCHEDULELIBRARY)" + Cst.CrLf;

                
                // FI 20170908 [23409] Utilisation de sqlContractCriteria
                sqlSelect += "      and " + scheduleRequest.GetCriteriaJoinOnSCHEDULE("s", false, sqlInstrCriteria, sqlInstrCriteriaUnl, sqlContractCriteria, null);
                //PL 20150310 Add IDASSET filter (NB: à déplacer dans GetCriteriaJoinOnSCHEDULE() lorsque cette colonne sera également disponible dans FEESCHEDULE)
                if (isFundingSearch || isMarginSearch)
                {
                    sqlSelect += OTCmlHelper.GetSQLComment("Asset filter");
                    sqlSelect += "      and ((s.IDASSET is null) or (s.IDASSET=@IDASSET))" + Cst.CrLf;
                }

                sqlSelect += OTCmlHelper.GetSQLComment("Currency filter");
                //CC/PL 20150429 TRIM 20993
                //sqlSelect += "      and ( (s.IDC is null) or (s.IDC=@IDC) )" + Cst.CrLf;
                sqlSelect += "      and ( (m.IDC is null) or (m.IDC=@IDC) )" + Cst.CrLf;
                sqlSelect += "      and ( (s.IDC is null) or (s.IDC=@IDC) )" + Cst.CrLf;

                if (isFundingSearch)
                {
                    sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ASSET_RATEINDEX.ToString() + " ari_l on ari_l.IDASSET=s.LONG_IDASSET" + Cst.CrLf;
                    sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.RATEINDEX.ToString() + " ri_l on ri_l.IDRX=ari_l.IDRX" + Cst.CrLf;
                    sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ASSET_RATEINDEX.ToString() + " ari_s on ari_s.IDASSET=s.SHORT_IDASSET" + Cst.CrLf;
                    sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.RATEINDEX.ToString() + " ri_s on ri_s.IDRX=ari_s.IDRX" + Cst.CrLf;
                }
                else if (isMarginSearch)
                {
                    //Nullable<int> _idAsset = scheduleRequest.Product.GetUnderlyingAssetId();
                    //if (_idAsset.HasValue)
                    //    idAsset = _idAsset.Value;

                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_INSTR_PRODUCT.ToString() + " vip on vip.IDI=@IDI" + Cst.CrLf;

                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_ASSET.ToString() + " v_asset on v_asset.ASSETCATEGORY=vip.ASSETCATEGORY" + Cst.CrLf;
                    sqlSelect += "      and v_asset.IDASSET=@IDASSET" + Cst.CrLf;
                    //sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.CUSTODIAN.ToString() + " cust on cust.IDA=@IDA_CUSTODIAN" + Cst.CrLf;

                    sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ASSETRATING.ToString() + " arating on arating.IDASSETREF=@IDASSET" + Cst.CrLf;
                    sqlSelect += "      and arating.ASSETCATEGORY=vip.ASSETCATEGORY" + Cst.CrLf;
                    sqlSelect += "      and arating.IDA=@IDA_CUSTODIAN" + Cst.CrLf;
                    //sqlSelect += "      and arating.RATINGCODE=case vip.ASSETCATEGORY" + Cst.CrLf;
                    //sqlSelect += "          when 'Commodity' then cust.COMMODITYRATINGCODE" + Cst.CrLf;
                    //sqlSelect += "          when 'Future' then cust.FUTURERATINGCODE" + Cst.CrLf;
                    //sqlSelect += "          when 'EquityAsset' then cust.EQUITYRATINGCODE" + Cst.CrLf;
                    //sqlSelect += "          when 'Index' then cust.INDEXRATINGCODE" + Cst.CrLf;
                    //sqlSelect += "          else null end" + Cst.CrLf;
                    sqlSelect += "      and arating.RATINGVALUE=s.RATINGVALUE" + Cst.CrLf;

                    parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), idI);
                    //parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), idAsset);
                    parameters.Add(new DataParameter(CS, "IDA_CUSTODIAN", DbType.Int32), scheduleRequest.GetIdAPartyB());
                }

                // FI 20170908 [23409] use sqlContractCriteria
                //sqlSelect += scheduleRequest.GetCriteriaJoinOnMATRIX("m", sqlInstrCriteria, sqlInstrCriteriaUnl, sqlDerivativeCriteria, parameters);
                sqlSelect += scheduleRequest.GetCriteriaJoinOnMATRIX("m", sqlInstrCriteria, sqlInstrCriteriaUnl, sqlContractCriteria, parameters);
                sqlSelect += String.Format(sqlWhereFundingType,"m");

                //Critère sur Dealer (PartyA)
                sqlSelect += OTCmlHelper.GetSQLComment("Dealer filter");
                sqlSelect += SQLCst.AND + "( (s.IDA is null) or (s.IDA=@IDA) )" + Cst.CrLf;
                parameters.Add(new DataParameter(CS, "IDA", DbType.Int32), scheduleRequest.GetIdAPartyA());

                if (isMarginSearch)
                {
                    //Critère sur Rating
                    sqlSelect += OTCmlHelper.GetSQLComment("Rating filter");
                    sqlSelect += SQLCst.AND + "( (s.RATINGVALUE is null) or (s.RATINGVALUE=arating.RATINGVALUE) )" + Cst.CrLf;
                }
                #endregion

                #region Order
                sqlSelect += SQLCst.ORDERBY + "s.IDA desc, " + Cst.CrLf;

                //if (isMarginSearch)
                //{
                //    //sqlSelect += "case when s.RATINGVALUE is not null then 1 else 0 end desc, " + Cst.CrLf;
                //    sqlSelect += "case when arating.RATINGVALUE is not null then 1 else 0 end desc, " + Cst.CrLf;
                //}

                sqlSelect += "case m.TYPEPARTYB when 'Book' then 4 when 'GrpBook' then 3 when 'Actor' then 2 when 'GrpActor' then 1 else 0 end desc, " + Cst.CrLf;
                sqlSelect += "case m.TYPEPARTYA when 'Book' then 4 when 'GrpBook' then 3 when 'Actor' then 2 when 'GrpActor' then 1 else 0 end desc, " + Cst.CrLf;
                //CC/PL 20150429 TRIM 20993
                // EG 20150310 Add sort IDASSET desc
                //sqlSelect += "isnull(s.IDC,m.IDC) desc, s.IDASSET desc, isnull(s.ASSETCATEGORY,m.ASSETCATEGORY) desc, " + Cst.CrLf;
                sqlSelect += "m.IDC desc, s.IDC desc, s.IDASSET desc, isnull(s.ASSETCATEGORY,m.ASSETCATEGORY) desc, " + Cst.CrLf;
                sqlSelect += "case isnull(s.TYPEINSTR_UNL,m.TYPEINSTR_UNL) when 'Instr' then 2 when 'GrpInstr' then 1 else 0 end desc, " + Cst.CrLf;
                // FI 20170908 [23409] gestion des valeurs DerivativeContract et CommodityContract
                sqlSelect += @"case isnull(s.TYPECONTRACT,m.TYPECONTRACT) 
                                    when 'DerivativeContract' then 4 
                                    when 'CommodityContract' then 4 
                                    when 'GrpContract' then 3 
                                    when 'Market' then 2 
                                    when 'GrpMarket' then 1 else 0 end desc, " + Cst.CrLf;
                sqlSelect += "case isnull(s.TYPEINSTR,m.TYPEINSTR) when 'Instr' then 2 when 'GrpInstr' then 1 else 0 end desc, " + Cst.CrLf;
                sqlSelect += "isnull(s.GPRODUCT,s.GPRODUCT) desc";
                if (isFundingSearch)
                    sqlSelect += ", case m.FUNDINGTYPE when 'FundingBorrowing' then 0 else 1 end desc" + Cst.CrLf;

                #endregion

                parameters["IDC"].Value = idC;
                qry = new QueryParameters(CS, sqlSelect.ToString(), parameters);
                _dr = DataHelper.ExecuteReader(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                ret = (_dr.Read());
                #endregion STEP 2
            }
            catch (Exception)
            {
                ret = false;

                throw;
            }
            finally
            {
                if (isRenewCustodian)
                {
                    scheduleRequest.PartyB.m_Party_Ida = savPartyB_Ida;
                    scheduleRequest.PartyB.m_Party_Idb = savPartyB_Idb;
                }

                //NB: Si ret=true, le DataReader est fermé ultérieurement.
                if (!ret)
                {
                    CloseDataReader();
                }
            }

            return ret;
        }
        protected void CloseDataReader()
        {
            if (_dr != null)
            {
                _dr.Close();
                _dr.Dispose();
            }
        }
        #endregion Methods
    }

    public class FundingResponse : ResponseBase
    {
        #region Members
        private IInterestCalculation _interestCalculation;
        private IRelativeDateOffset _fixingDatesRDO;
        private IInterval _interestFrequency;
        private bool _isNotionalReset;
        private bool _isNotionalResetSpecified;
        //private FundingProcessing _parent; 
        #endregion Members

        #region Accessors
        public IInterestCalculation InterestCalculation
        {
            get { return _interestCalculation; }
        }
        public bool InterestCalculationSpecified
        {
            get { return (_interestCalculation != null); }
        }
        public IRelativeDateOffset FixingDatesRDO
        {
            get { return _fixingDatesRDO; }
        }
        public bool FixingDatesRDOSpecified
        {
            get { return (_fixingDatesRDO != null); }
        }
        public IInterval InterestFrequency
        {
            get { return _interestFrequency; }
        }
        public bool InterestFrequencySpecified
        {
            get { return (_interestFrequency != null); }
        }
        // EG 20160404 Migration vs2013
        public bool IsNotionalReset
        {
            //get { return _isNotionalResetSpecified; }
            get { return _isNotionalReset; }
        }
        public bool IsNotionalResetSpecified
        {
            get { return _isNotionalResetSpecified; }
        }
        //public FundingProcessing Parent
        //{
        //    get { return _parent; }
        //}
        #endregion Accessors

        #region Constructors
        public FundingResponse(FundingProcessing pFundingProcessing)
            : base(pFundingProcessing) { }
        #endregion Constructors    

        #region Methods
        #region public Calc
        public void Calc()
        {
            FeeRequest scheduleRequest = Parent.ScheduleRequest;
            // EG 20160404 Migration vs2013
            _errorMessage = null;
            _infoMessage = null;
            _levelEnum = LevelStatusTools.LevelEnum.NA;
            _statusEnum = LevelStatusTools.StatusEnum.NA;


            if (scheduleRequest.Product.IsReturnSwap && ((IReturnSwap)scheduleRequest.Product.Product).InterestLegSpecified)
            {
                _interestCalculation = ((IReturnSwap)scheduleRequest.Product.Product).InterestLeg[0].CreateInterestCalculation;
                _fixingDatesRDO = ((IReturnSwap)scheduleRequest.Product.Product).InterestLeg[0].CreateRelativeDateOffset;
                _interestFrequency = ((IProductBase)scheduleRequest.Product.Product).CreateInterval(PeriodEnum.D, 1);
                _isNotionalResetSpecified = false;

                LoadFundingRate();
            }
        }
        #endregion public Calc

        /// <summary>
        /// Retourne l'enregistrement le "plus fin*", compatible avec l'environement ScheduleRequest (FeeRequest)
        /// <para>* voir Order By</para>
        /// </summary>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        private void LoadFundingRate() 
        {
            try
            {
                if (LoadData(Cst.OTCml_TBL.FUNDINGSCHEDULE))
                {
                    string data = null;
                    string prefix = Dr["PREFIX"].ToString() + "_";

                    _interestCalculation.FloatingRateSpecified = false;
                    _interestCalculation.FixedRateSpecified = false;

                    #region FLOATINGRATE
                    data = Dr[prefix + "IDASSET"].ToString();
                    if ((!string.IsNullOrEmpty(data)))
                    {
                        _interestCalculation.FloatingRateSpecified = true;
                        _interestCalculation.FloatingRate.FloatingRateIndex.OTCmlId = Convert.ToInt32(data);
                        _interestCalculation.FloatingRate.NegativeInterestRateTreatmentSpecified = true;
                        _interestCalculation.FloatingRate.NegativeInterestRateTreatment = NegativeInterestRateTreatmentEnum.NegativeInterestRateMethod;
                        SQL_AssetRateIndex sql_AssetRateIndex = new SQL_AssetRateIndex(CS, SQL_AssetRateIndex.IDType.IDASSET, _interestCalculation.FloatingRate.FloatingRateIndex.OTCmlId);
                        if (sql_AssetRateIndex.IsLoaded)
                        {
                            _interestCalculation.SqlAsset = sql_AssetRateIndex;
                            _interestCalculation.FloatingRate.FloatingRateIndex.Value = sql_AssetRateIndex.Idx_IdIsda;
                            _interestCalculation.FloatingRate.IndexTenorSpecified = sql_AssetRateIndex.Asset_RateIndexWithTenor;
                            if (_interestCalculation.FloatingRate.IndexTenorSpecified)
                            {
                                _interestCalculation.FloatingRate.IndexTenor.Period = StringToEnum.Period(sql_AssetRateIndex.Asset_Period_Tenor);
                                _interestCalculation.FloatingRate.IndexTenor.PeriodMultiplier = new EFS_Integer(sql_AssetRateIndex.Asset_PeriodMltp_Tenor.ToString());
                            }
                            //Multiplier
                            data = Dr[prefix + "MULTIPLIER"].ToString();
                            _interestCalculation.FloatingRate.FloatingRateMultiplierScheduleSpecified = !string.IsNullOrEmpty(data);
                            if (_interestCalculation.FloatingRate.FloatingRateMultiplierScheduleSpecified)
                            {
                                _interestCalculation.FloatingRate.FloatingRateMultiplierSchedule.InitialValue.DecValue = Convert.ToDecimal(data);
                                //_interestCalculation.floatingRate.floatingRateMultiplierSchedule.stepSpecified = false;
                            }
                            //Spread
                            // EG 20150309 POC - BERKELEY Refactoring (Add MarkUp)
                            ArrayList aSpreadSchedule = new ArrayList();

                            data = Dr[prefix + "SPREAD"].ToString();
                            _interestCalculation.FloatingRate.SpreadScheduleSpecified = !string.IsNullOrEmpty(data);
                            if (_interestCalculation.FloatingRate.SpreadScheduleSpecified)
                            {
                                //_interestCalculation.floatingRate.spreadSchedule.initialValue.DecValue = Convert.ToDecimal(data);
                                //_interestCalculation.floatingRate.spreadSchedule.stepSpecified = false;
                                ISpreadSchedule spreadSchedule = _interestCalculation.FloatingRate.CreateSpreadSchedule();
                                spreadSchedule.InitialValue.DecValue = Convert.ToDecimal(data);
                                spreadSchedule.CreateSpreadScheduleType("Spread");
                                aSpreadSchedule.Add((ISpreadSchedule)spreadSchedule);
                            }

                            Nullable<decimal> formula = ShiftFormulaConverted;
                            if (formula.HasValue)
                            {
                                _interestCalculation.FloatingRate.SpreadScheduleSpecified = true;
                                ISpreadSchedule additionalSpread = _interestCalculation.FloatingRate.CreateSpreadSchedule();
                                additionalSpread.InitialValue.DecValue = formula.Value;
                                additionalSpread.CreateSpreadScheduleType("MarkUp");
                                aSpreadSchedule.Add((ISpreadSchedule)additionalSpread);
                            }
                            // EG 20150309 La ligne suivante ne fonctionne pas !!!
                            //_interestCalculation.floatingRate.lstSpreadSchedule = (ISpreadSchedule[])aSpreadSchedule.ToArray(typeof(ISpreadSchedule));
                            _interestCalculation.FloatingRate.LstSpreadSchedule = 
                                _interestCalculation.FloatingRate.CreateSpreadSchedules((ISpreadSchedule[])aSpreadSchedule.ToArray(typeof(ISpreadSchedule)));

                            //Fixing (Rq. RELATIVETOFIXINGDT is unused) 
                            _fixingDatesRDO.DayTypeSpecified = true;
                            _fixingDatesRDO.DayType = StringToEnum.DayType(Dr[prefix + "DAYTYPEFIXINGOFFSET"].ToString());
                            _fixingDatesRDO.Period = StringToEnum.Period(Dr[prefix + "PERIODFIXINGOFFSET"].ToString());
                            _fixingDatesRDO.PeriodMultiplier = new EFS_Integer(Dr[prefix + "PERIODMLTPFIXINGOFFSET"].ToString());
                        }
                    }
                    #endregion FLOATINGRATE
                    #region FIXEDRATE
                    if (!_interestCalculation.FloatingRateSpecified)
                    {
                        data = Dr[prefix + "FIXEDRATE"].ToString();
                        _interestCalculation.FixedRateSpecified = !string.IsNullOrEmpty(data);
                        if (_interestCalculation.FixedRateSpecified)
                        {
                            _interestCalculation.FixedRate.DecValue = Convert.ToDecimal(data);
                        }
                        else
                        {
                            //Set ZERO
                            _interestCalculation.FixedRateSpecified = true;
                            _interestCalculation.FixedRate.IntValue = 0;
                        }
                    }
                    #endregion FIXEDRATE
                    #region DCF
                    data = Dr[prefix + "DCF"].ToString();
                    _interestCalculation.DayCountFraction = StringToEnum.DayCountFraction(data);
                    #endregion DCF

                    #region ISNOTIONALRESET
                    data = Dr["ISNOTIONALRESET"].ToString();
                    if (!String.IsNullOrEmpty(data))
                    {
                        // EG 20160404 Migration vs2013
                        _isNotionalResetSpecified = true;
                        //_isNotionalResetSpecified = BoolFunc.IsTrue(data);
                        _isNotionalReset = BoolFunc.IsTrue(data);
                    }
                    #endregion ISNOTIONALRESET
                    #region PERIOD
                    data = Dr["PERIOD"].ToString();
                    if (!String.IsNullOrEmpty(data))
                    {
                        _interestFrequency.Period = StringToEnum.Period(data);
                    }
                    data = Dr["PERIODMLTP"].ToString();
                    if (!String.IsNullOrEmpty(data))
                    {
                        _interestFrequency.PeriodMultiplier = new EFS_Integer(data);
                    }
                    #endregion PERIOD
                }
                else
                {
                    _interestCalculation = null; 
                }
            }
            catch (Exception)
            {
                _interestCalculation = null; 

                throw;
            }
            finally
            {
                CloseDataReader();
            }
        }
        #endregion Methods
    }

    public class MarginResponse : ResponseBase
    {
        #region Members
        #endregion Members

        #region Accessors
        public Nullable<decimal> MarginRatioAmount
        {
            get;
            private set;
        }
        public Boolean MarginRatioAmountSpecified
        {
            get { return (this.MarginRatioAmount != null) && (this.MarginRatioAmount.HasValue); }
        }
        public Nullable<decimal> SpreadMarginRatioAmount
        {
            get;
            private set;
        }
        public Boolean SpreadMarginRatioAmountSpecified
        {
            get { return (this.SpreadMarginRatioAmount != null) && (this.SpreadMarginRatioAmount.HasValue); }
        }
        public Nullable<decimal> CrossMarginRatioAmount
        {
            get;
            private set;
        }
        public Boolean CrossMarginRatioAmountSpecified
        {
            get { return (this.CrossMarginRatioAmount != null) && (this.CrossMarginRatioAmount.HasValue); }
        }
        public bool IsApplyMinMargin
        {
            get;
            private set;
        }
        #endregion Accessors

        #region Constructors
        public MarginResponse(MarginProcessing pMarginProcessing)
            : base(pMarginProcessing) { }
        #endregion Constructors

        #region Methods
        #region public Calc
        /// EG 20150306 [POC-BERKELEY] : 
        public void Calc()
        {
            // EG 20160404 Migration vs2013
            _errorMessage = null;
            _infoMessage = null;
            _levelEnum = LevelStatusTools.LevelEnum.NA;
            _statusEnum = LevelStatusTools.StatusEnum.NA;

            SetMargin();
        }
        #endregion public Calc

        /// <summary>
        /// Retourne l'enregistrement le "plus fin*", compatible avec l'environement ScheduleRequest (FeeRequest)
        /// <para>* voir Order By</para>
        /// </summary>
        /// <returns></returns>
        /// EG 20150309 POC - BERKELEY Add ShiftFormula (Reconduction)
        private void SetMargin()
        {
            try
            {
                MarginRatioAmount = CrossMarginRatioAmount = null;
                if (LoadData(Cst.OTCml_TBL.MARGINSCHEDULE))
                {
                    string data = Dr["MARGINVALUE"].ToString(); 
                    if (StrFunc.IsFilled(data))
                    {
                        MarginRatioAmount = Convert.ToDecimal(data);

                        data = Dr["CROSSMARGINVALUE"].ToString();
                        if (StrFunc.IsFilled(data))
                        {
                            CrossMarginRatioAmount = Convert.ToDecimal(data);
                        }

                        data = Dr["MARGINTYPE"].ToString();
                        if (data.ToUpper() == Cst.MarginType.LEVERAGE.ToString())
                        {
                            // EG 20150408 Test MarginRatioAmountSpecified|CrossMarginRatioAmountSpecified
                            if (MarginRatioAmountSpecified)
                                MarginRatioAmount = (1 / MarginRatioAmount.Value);
                            if (CrossMarginRatioAmountSpecified)
                                CrossMarginRatioAmount = (1 / CrossMarginRatioAmount.Value);
                        }
                        IsApplyMinMargin = Convert.ToBoolean(Dr["ISAPPLYMIN"]);
                    }
                    Nullable<decimal> formula = ShiftFormulaConverted;
                    if (formula.HasValue)
                    {
                        SpreadMarginRatioAmount = formula.Value;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CloseDataReader();
            }
        }
        #endregion Methods
    }
}