#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Permission;
using EfsML.Business;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Classe chargé de sauvegarder un titre (debtsecurity)
    /// </summary>
    public sealed partial class DebtSecCaptureGen : TradeCommonCaptureGen
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private DebtSecInput m_Input;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public DebtSecInput Input
        {
            set { m_Input = value; }
            get { return m_Input; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20130204 [] propertie set utilisée pour l'importation 
        public override TradeCommonInput TradeCommonInput
        {
            get
            {
                return (TradeCommonInput)m_Input;
            }
            set
            {
                this.Input = (DebtSecInput)value;
            }
        }

        /// <summary>
        /// Obtient TRADEDEBTSEC
        /// </summary>
        public override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.TRADEDEBTSEC.ToString();
            }
        }
        #endregion Accessors

        #region Constructor
        public DebtSecCaptureGen() {
            m_Input = new DebtSecInput();
        }
        
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Contrôle que l'identifier du titre {pIdentifier} existe déjà dans le référentiel
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdentifier"></param>
        /// <exception cref="TradeCommonCaptureGenException[SECURITYIDENTIFIER_DUPLICATE] si identifiant existe déjà"></exception>
        /// FI 202120301 [18465]  pIdentifier n'est plus de type ref
        protected override void CheckIdentifier(string pCS, Cst.Capture.ModeEnum pCaptureMode, string pIdentifier)
        {
            bool isNew = Cst.Capture.IsModeNewCapture(pCaptureMode);
            bool isUpd = Cst.Capture.IsModeUpdateOrUpdatePostEvts(pCaptureMode);

            bool isToCheck = (isNew || isUpd);
            //
            if (isToCheck)
            {
                StrBuilder sql = new StrBuilder("");
                sql += SQLCst.SELECT + "1" + SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " t" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i on i.IDI=t.IDI" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p on p.IDP=i.IDP" + Cst.CrLf;
                sql += SQLCst.WHERE + "p.FAMILY=@FAMILY" + SQLCst.AND + "p.GPRODUCT=@GPRODUCT" + Cst.CrLf;
                sql += SQLCst.AND + "t.IDENTIFIER=@IDENTIFIER" + Cst.CrLf;
                //
                if (isUpd)
                    sql += SQLCst.AND + @"t.IDT!=@IDT" + Cst.CrLf;
                //
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "FAMILY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), "DSE");
                parameters.Add(new DataParameter(pCS, "GPRODUCT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), "ASSET");
                parameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdentifier);
                //
                if (isUpd)
                    parameters.Add(new DataParameter(pCS, "@IDT", DbType.Int32), m_Input.IdT);
                //
                QueryParameters qry = new QueryParameters(pCS, sql.ToString(), parameters);
                //
                object obj = DataHelper.ExecuteScalar(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                bool isOk = (null == obj);
                if (false == isOk)
                {
                    string msg = Ressource.GetString2("Msg_Identifier_Detail", pIdentifier);
                    throw new TradeCommonCaptureGenException("DebtSecCaptureGen.CheckIdentifier", msg, TradeCaptureGen.ErrorLevel.SECURITYIDENTIFIER_DUPLICATE);
                }
            }
        }

        ///<summary>
        ///Contrôle des validationRules
        ///</summary>
        ///<param name="pDbTransaction"></param>
        ///<param name="pCaptureMode"></param>
        ///<param name="pCheckMode"></param>
        ///<exception cref="TradeCommonCaptureGenException[VALIDATIONRULE_ERROR] lorsque les règles ne sont pas respectées"></exception>
        // EG 20171115 Upd Add CaptureSessionInfo
        public override void CheckValidationRule(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CheckTradeValidationRule.CheckModeEnum pCheckMode, User pUser)
        {
            CheckDebtSecValidationRule chk = new CheckDebtSecValidationRule(m_Input, pCaptureMode, pUser);
            if (false == chk.ValidationRules(CSTools.SetCacheOn(pCS), pDbTransaction, pCheckMode))
            {
                throw new TradeCommonCaptureGenException("DebtSecCaptureGen.CheckValidationRule", chk.GetConformityMsg(),
                    TradeCaptureGen.ErrorLevel.VALIDATIONRULE_ERROR);
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUSer"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSetNewCustomcapturesInfos"></param>
        /// <returns></returns>
        public override bool Load(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, Cst.Capture.ModeEnum pCaptureMode, User pUSer, string pSessionId, bool pIsSetNewCustomcapturesInfos)
        {
            return base.Load(pCS, pDbTransaction, pId, pIdType, pCaptureMode, pUSer, pSessionId, pIsSetNewCustomcapturesInfos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdMenu"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pRecordSettings"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifierUnderlying"></param>
        /// <param name="pIdTUnderlying"></param>
        /// <param name="pIdTRK_L"></param>
        /// <param name="pProcessLog"></param>
        /// <exception cref="TradeCommonCaptureGenException"></exception>
        /// FI 20170404 [23039] Cht de signature (oUnderlying, oTrader) 
        public override void CheckAndRecord(string pCS, IDbTransaction pDbTransaction, string pIdMenu, Cst.Capture.ModeEnum pCaptureMode,
            CaptureSessionInfo pSessionInfo, TradeRecordSettings pRecordSettings,
            ref string pIdentifier, ref int pIdT,
            out Pair<int, string>[] oUnderlying,
            out Pair<int, string>[] oTrader)
        {
            bool removeCache = false;
            try
            {
                // FI 20170404 [23039] oUnderlying et oTrader
                base.CheckAndRecord(pCS, pDbTransaction, pIdMenu, pCaptureMode, pSessionInfo, pRecordSettings, ref pIdentifier, ref pIdT,
                    out oUnderlying, out oTrader);

                removeCache = true;
            }
            catch (TradeCommonCaptureGenException ex)
            {
                if (TradeCaptureGen.IsRecordInSuccess(ex.ErrLevel))
                    removeCache = true;
                throw;
            }
            finally
            {
                if (removeCache)
                {
                    try
                    {
                        DataHelper.queryCache.Remove(Cst.OTCml_TBL.VW_TRADEDEBTSEC.ToString(), pCS);
                        DataEnabledHelper.ClearCache(pCS, Cst.OTCml_TBL.VW_TRADEDEBTSEC);
                    }
                    catch
                    {
                        //pas la peine de planter s'il existe un pb lors de la purge du cache
                    }
                }
            }
        }

        /// <summary>
        /// Contrôle si le titre n'est pas déjà utilisé par un trade "classique"
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        /// <exception cref="TradeCommonCaptureGenException"></exception>
        /// FI 20120601 Ajout de dtReference afin de réduire le scope des trades traités (amélioration des perfs) 
        /// FI 20120614 [17890] Utilisation de la colonne TRADEINSTRUMENT.IDASSET
        /// EG 20140106 [19344] Add pRecordSettings
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override void CheckSpecific(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, DateTime pDtRefForDtEnabled)
        {
            bool isCaptureUpd = Cst.Capture.IsModeUpdate(pCaptureMode);
            bool isRemove = Cst.Capture.IsModeRemove(pCaptureMode);
            bool isOk = isCaptureUpd || isRemove;

            if (isOk)
            {
                DateTime dtReference = DateTime.MinValue;
                SQL_LastTrade_L create = new SQL_LastTrade_L(CSTools.SetCacheOn(pCS), TradeCommonInput.IdT, new PermissionEnum[] { PermissionEnum.Create });
                if (create.LoadTable(new string[] { "DTSYS" }))
                    dtReference = create.DtSys.AddDays(-1);

                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
                parameters.Add(new DataParameter(pCS, "TEMPLATE", DbType.AnsiString, 255), Cst.StatusEnvironment.TEMPLATE.ToString());
                parameters.Add(new DataParameter(pCS, "DEACTIV", DbType.AnsiString, 255), Cst.StatusActivation.DEACTIV.ToString());
                parameters.Add(new DataParameter(pCS, "DSE", DbType.AnsiString, 255), Cst.ProductFamily_DSE);
                parameters.Add(new DataParameter(pCS, "SEC", DbType.AnsiString, 255), Cst.ProductGProduct_SEC);
                parameters.Add(new DataParameter(pCS, "TRD", DbType.AnsiString, 3), "TRD");
                parameters.Add(new DataParameter(pCS, "DAT", DbType.AnsiString, 3), "DAT");
                if (dtReference != DateTime.MinValue)
                    parameters.Add(DataParameter.GetParameter(pCS,DataParameter.ParameterEnum.DTSYS), dtReference);  // FI 20201006 [XXXXX] Call GetParameter

                //Le titre ne peut être modifié ou annulé, il est utilisé par des transactions qui possèdent des évènements
                string sqlSelect = @"select tr.IDENTIFIER, tr.DTTRADE, ns.IDENTIFIER as NS_IDENTIFIER
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = @TRD) and (ev.EVENTTYPE = @DAT)
                where (tr.IDASSET = @IDT) and (tr.IDSTENVIRONMENT != @TEMPLATE) and (tr.IDSTACTIVATION != @DEACTIV) and (pr.FAMILY = @DSE) and (pr.GPRODUCT = @SEC)" + Cst.CrLf;
                if (dtReference != DateTime.MinValue)
                    sqlSelect += " and (tr.DTSYS > @DTSYS)";

                QueryParameters qryParameters = new QueryParameters(pCS, DataHelper.GetSelectTop(pCS, sqlSelect, 3), parameters);

                bool isFound = false;
                string msg = null;
                try
                {
                    using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                    {
                        if (dr.Read())
                        {
                            isFound = true;
                            msg = Ressource.GetString("Msg_SecurityUseError_Trade");
                            msg += Ressource.GetString2("Msg_SecurityUseError_Trade_Detail",
                                Convert.ToString(dr["IDENTIFIER"]), DtFunc.DateTimeToString(Convert.ToDateTime(dr["DTTRADE"]), DtFunc.FmtShortDate), Convert.ToString(dr["NS_IDENTIFIER"]));
                            if (dr.Read())
                                msg += Ressource.GetString2("Msg_SecurityUseError_Trade_Detail",
                                    Convert.ToString(dr["IDENTIFIER"]), DtFunc.DateTimeToString(Convert.ToDateTime(dr["DTTRADE"]), DtFunc.FmtShortDate), Convert.ToString(dr["NS_IDENTIFIER"]));
                            if (dr.Read())
                                msg += "...";
                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw ex; 
                    // RD 20100115 [16792] Utiliser SQLErrorEnum 
                    bool isSQLException = DataHelper.AnalyseSQLException(pCS, ex, out _, out SQLErrorEnum sqlErrorCode);
                    if (isSQLException && (sqlErrorCode == SQLErrorEnum.Timeout))
                    {
                        //20090917 PL Temporaire (cf ci-dessus)
                        msg = ex.Message;
                        throw new TradeCommonCaptureGenException("DebtSecCaptureGen.CheckSpecific", msg, ErrorLevel.SECURITYSUSE_ERROR2);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (isFound)
                    throw new TradeCommonCaptureGenException("DebtSecCaptureGen.CheckSpecific", msg, ErrorLevel.SECURITYSUSE_ERROR);
            }
        }

        /// <summary>
        /// Modification des trades qui portent sur le titre modifié
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <exception cref="TradeCommonCaptureGenException"></exception> 
        /// EG 20100401 Add new [SQL_TableWithID.IDType] parameter
        /// FI 20120601 Ajout de dtReference afin de réduire le scope des trades traités (amélioration des perfs) 
        /// FI 20120614 [17890] Modification de la requête => passage par la table TRADEINSTRUMENT. Cette colonne contient la colonne IDASSET  
        /// EG 20150422 [20513] BANCAPERTA add issuerReferenceSpecified
        /// FI 20170404 [23039] Modify
        /// EG 20180423 Analyse du code Correction [CA2200]
        protected override void AfterRecord(string pCS, int pIdT, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo)
        {
            //Lors d'une modification de titre on met à jour les datadocuments des trades qui s'appuient sur ce titre
            //pour Info en modification de titre on passe ici uniquement si les trades associés n'ont pas leur évènements de générés
            
            string msgError = string.Empty;
            //FI 20091211 [16795] pas de mise à jour si modification d'un TEMPLATE ou d'un PRE-TRADE 
            bool isOk = Cst.Capture.IsModeUpdate(pCaptureMode) || Cst.Capture.IsModeUpdateOrUpdatePostEvts(pCaptureMode);
            if (isOk)
                isOk = (false == IsInputIncompleteAllow(pCaptureMode));
            //
            if (isOk)
            {
                DateTime dtReference = DateTime.MinValue;
                SQL_LastTrade_L create = new SQL_LastTrade_L(CSTools.SetCacheOn(pCS), TradeCommonInput.IdT, new PermissionEnum[] { PermissionEnum.Create });
                if (create.LoadTable(new string[] { "DTSYS" }))
                    dtReference = create.DtSys.AddDays(-1);

                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "DSE", DbType.AnsiString, 255), Cst.ProductFamily_DSE);
                parameters.Add(new DataParameter(pCS, "SEC", DbType.AnsiString, 255), Cst.ProductGProduct_SEC);
                parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
                if (dtReference != DateTime.MinValue)
                    parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTSYS), dtReference); // FI 20201006 [XXXXX] Call GetDataParamter

                string sqlQuery = @"select t.IDT, t.IDENTIFIER
                from dbo.TRADE t
                inner join dbo.INSTRUMENT i on (i.IDI = t.IDI)
                inner join dbo.PRODUCT p on (p.IDP = i.IDP)
                where (p.FAMILY = @DSE) and (p.GPRODUCT = @SEC) and (t.IDASSET = @IDT)";
                if (dtReference != DateTime.MinValue)
                    sqlQuery += " and (t.DTSYS > @DTSYS)";
                QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
                DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                //
                if ((null != dt) && ArrFunc.IsFilled(dt.Rows))
                {
                    //Chargement de l'asset qui vient dêtre modifié
                    ISecurityAsset securityAsset = Tools.LoadSecurityAsset(pCS, null, pIdT);
                    //securityAsset.issuerSpecified = false;
                    securityAsset.IssuerReferenceSpecified = false;
                    securityAsset.IsNewAssetSpecified = false;
                    securityAsset.IdTTemplateSpecified = false;
                    ((IProduct)securityAsset.DebtSecurity).ProductBase.Id = null;
                    //
                    for (int i = 0; i < ArrFunc.Count(dt.Rows); i++)
                    {
                        string identifier = Convert.ToString(dt.Rows[i]["IDENTIFIER"]);
                        int idT = Convert.ToInt32(dt.Rows[i]["IDT"]);
                        //
                        TradeCaptureGen captureGen = new TradeCaptureGen();
                        //
                        User userAdmin = new User(1, null, RoleActor.SYSADMIN);
                        string sessionId = pSessionInfo.session.SessionId;
                        captureGen.Load(pCS,null, idT, Cst.Capture.ModeEnum.Consult, userAdmin, sessionId, false);
                        ArrayList al = new ArrayList();

                        IDebtSecurityTransaction debtSecurityTransaction;

                        if (captureGen.Input.DataDocument.CurrentProduct.IsDebtSecurityTransaction)
                        {
                            debtSecurityTransaction = (IDebtSecurityTransaction)captureGen.Input.DataDocument.CurrentProduct.Product;
                            al.Add(debtSecurityTransaction);
                        }
                        else if (captureGen.Input.DataDocument.CurrentProduct.IsSaleAndRepurchaseAgreement)
                        {
                            ISaleAndRepurchaseAgreement saleAndRepurchaseAgreement = (ISaleAndRepurchaseAgreement)captureGen.Input.DataDocument.CurrentProduct.Product;
                            for (int j = 0; j < ArrFunc.Count(saleAndRepurchaseAgreement.SpotLeg); j++)
                            {
                                if (saleAndRepurchaseAgreement.SpotLeg[j].DebtSecurityTransaction.SecurityAssetSpecified)
                                {
                                    debtSecurityTransaction = saleAndRepurchaseAgreement.SpotLeg[j].DebtSecurityTransaction;
                                    al.Add(debtSecurityTransaction);
                                }
                            }
                            //
                            for (int j = 0; j < ArrFunc.Count(saleAndRepurchaseAgreement.ForwardLeg); j++)
                            {
                                if (saleAndRepurchaseAgreement.ForwardLeg[j].DebtSecurityTransaction.SecurityAssetSpecified)
                                {
                                    debtSecurityTransaction = saleAndRepurchaseAgreement.ForwardLeg[j].DebtSecurityTransaction;
                                    al.Add(debtSecurityTransaction);
                                }
                            }
                        }
                        else
                        {
                            throw new NotImplementedException("Current product is not managed, please contact EFS");
                        }
                        //
                        bool isToSave = ArrFunc.IsFilled(al);
                        if (isToSave)
                        {
                            for (int j = 0; j < ArrFunc.Count(al); j++)
                            {
                                debtSecurityTransaction = (IDebtSecurityTransaction)al[j];
                                string savId = debtSecurityTransaction.SecurityAsset.Id;
                                debtSecurityTransaction.SecurityAsset = securityAsset;
                                debtSecurityTransaction.SecurityAsset.Id = savId;
                            }
                        }
                        //
                        if (isToSave)
                        {
                            string screenName = captureGen.TradeCommonInput.SQLLastTradeLog.ScreenName;
                            string displayname = captureGen.TradeCommonInput.Identification.Displayname;
                            string description = captureGen.TradeCommonInput.Identification.Description;
                            string extlLink = captureGen.TradeCommonInput.Identification.Extllink;

                            //mise à jour du DataDocument
                            //Sauvegarde
                            TradeRecordSettings recordSettings = new TradeRecordSettings
                            {
                                isCheckValidationRules = false,
                                idScreen = screenName,
                                displayName = displayname,
                                description = description,
                                extLink = extlLink,
                                // RD 20121031 ne pas vérifier la license pour les services pour des raisons de performances
                                isCheckLicense = RecordSettings.isCheckLicense
                            };

                            //Utilisation de l'utilisateur SYSADM pour avoir tous les pouvoirs
                            CaptureSessionInfo sessionInfo = new CaptureSessionInfo
                            {
                                user = new User(pSessionInfo.session.IdA, null, RoleActor.SYSADMIN),
                                session = pSessionInfo.session,
                                licence = pSessionInfo.licence,
                                idTracker_L = pSessionInfo.idTracker_L,
                                idProcess_L = pSessionInfo.idProcess_L,
                            };

                            // FI 20170404 [23039] gestion de underlying et trader
                            Pair<int, string>[] underlying = null;
                            Pair<int, string>[] trader = null;
                            try
                            {
                                captureGen.CheckAndRecord(pCS, null, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), pCaptureMode, sessionInfo, recordSettings,
                                    ref identifier, ref idT, out underlying, out trader);
                            }
                            catch (TradeCommonCaptureGenException ex)
                            {
                                if (false == TradeCommonCaptureGen.IsRecordInSuccess(ex.ErrLevel))
                                {
                                    if (StrFunc.IsFilled(msgError))
                                        msgError += Cst.CrLf;


                                    // FI 20170404 [23039] underlying, trader
                                    string msg = captureGen.GetResultMsgAfterCheckAndRecord(pCS, ex, pCaptureMode, identifier, underlying, trader, false, out string msgDetail);

                                    msgError += msg;
                                }
                            }
                            catch (Exception) { throw; }
                        }
                    }
                }
                base.AfterRecord(pCS, pIdT, pCaptureMode, pSessionInfo);
                //
                if (StrFunc.IsFilled(msgError))
                    throw new TradeCommonCaptureGenException("DebtSecCaptureGen.AfterCommit", msgError, ErrorLevel.AFTER_RECORD_ERROR);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class DebtSecCaptureGen
    {
        #region Methods
        /// <summary>
        /// Alimente les tables  TRADEINSTURMENT et TRADESTREAM 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pProduct"></param>
        /// <param name="pIsUpdateOnly_TradeInstrument"></param>
        /// RD 20130109 [18314] update only asset if trade included on invoice or partial modification mode
        protected override void InsertTradeInstrument(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct, bool pIsUpdateOnly_TradeInstrument)
        {
            int streamNo = 1;
            int instrumentNo = Convert.ToInt32(pProduct.ProductBase.Id.Replace(Cst.FpML_InstrumentNo, string.Empty));
            base.InsertTradeInstrument(pCS, pDbTransaction, pIdT, pProduct, pIsUpdateOnly_TradeInstrument);
            if (pIsUpdateOnly_TradeInstrument)
                return;

            if (pProduct.ProductBase.IsDebtSecurity)
            {
                #region InterestRateStreams
                IInterestRateStream[] streams = ((IDebtSecurity)pProduct.Product).Stream;
                TradeStreamTools.InsertInterestRateStreams(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, streams);
                #endregion InterestRateStreams
            }
            else
            {
                throw new Exception("Error, Current product is not managed, please contact EFS");
            }

        }
        protected override void InsertTradeStream(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct)
        {
            int streamNo = 1;
            int instrumentNo = Convert.ToInt32(pProduct.ProductBase.Id.Replace(Cst.FpML_InstrumentNo, string.Empty));

            if (pProduct.ProductBase.IsDebtSecurity)
            {
                #region InterestRateStreams
                IInterestRateStream[] streams = ((IDebtSecurity)pProduct.Product).Stream;
                TradeStreamTools.InsertInterestRateStreams(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, streams);
                #endregion InterestRateStreams
            }
            else
            {
                throw new Exception("Error, Current product is not managed, please contact EFS");
            }

        }

        /// <summary>
        /// Alimentation de la table TRADEASSET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pProduct"></param>
        /// FI 20120619 [17904] Alimentation de la table TRADEASSET
        /// TRADEASSET est une redondance des informations présentes dans le flux XML
        /// Elle est alimentée sur les titres REGULAR ou TEMPLATE
        protected override void SaveTradeAsset(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            base.SaveTradeAsset(pCS, pDbTransaction, pIdT);

            InsertTradeAsset(pCS, pDbTransaction, pIdT);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pProduct"></param>
		///CC 20160920 [22477]
        ///FI 20120618 [17904] Alimentation de la table TRADEASSET
        // EG 20190926 [Maturity Redemption] Upd Nouvelles colonnes : ISSUEPRICEPCT|REDEMPTIONPRICEPCT|INTACCRUALDATE|DATEDDATE|MATURITYDATE|PERIODMLTPPAYMENTDT|PERIODPAYMENTDT
        // EG 20230505 [XXXXX] [WI617] data optional => controls for Trade template
        private void InsertTradeAsset(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            ProductContainer product = TradeCommonInput.Product;

            if (product.ProductBase.IsDebtSecurity)
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);

                parameters.Add(new DataParameter(pCS, "IDM", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "ISINCODE", DbType.AnsiString, 32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "RICCODE", DbType.AnsiString, 32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "CUSIP", DbType.AnsiString, 32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "SEDOL", DbType.AnsiString, 32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "BBGCODE", DbType.AnsiString, 32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "SICOVAM", DbType.AnsiString, 32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "COMMON", DbType.AnsiString, 32), Convert.DBNull);

                parameters.Add(new DataParameter(pCS, "SECURITYCLASS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "CFICODE", DbType.AnsiString, SQLCst.UT_CFICODE_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "PRODUCTTYPECODE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "FIPRODUCTTYPECODE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);

                parameters.Add(new DataParameter(pCS, "IDC_ISSUE", DbType.AnsiString, SQLCst.UT_CURR_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDCOUNTRY_ISSUE", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), Convert.DBNull);

                parameters.Add(new DataParameter(pCS, "COUPONTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "FIXEDRATE", DbType.Decimal), Convert.DBNull);

                parameters.Add(new DataParameter(pCS, "PARVALUE", DbType.Decimal), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDC_PARVALUE", DbType.AnsiString, SQLCst.UT_CURR_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "DCF", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);

                parameters.Add(new DataParameter(pCS, "ISSUEPRICEPCT", DbType.Decimal), 1);
                parameters.Add(new DataParameter(pCS, "REDEMPTIONPRICEPCT", DbType.Decimal), 1);
                parameters.Add(new DataParameter(pCS, "INTACCRUALDATE", DbType.Date), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "DATEDDATE", DbType.Date), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "MATURITYDATE", DbType.Date), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "PERIODMLTPPAYMENTDT", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "PERIODPAYMENTDT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);

                DebtSecurityContainer debSecurity = new DebtSecurityContainer((IDebtSecurity)product.Product);
                //
                //Code
                string code = debSecurity.GetCodeValue(pCS, "ISIN");
                if (StrFunc.IsFilled(code))
                    parameters["ISINCODE"].Value = code;
                //
                code = debSecurity.GetCodeValue(pCS, "CUSIP");
                if (StrFunc.IsFilled(code))
                    parameters["CUSIP"].Value = code;
                //
                code = debSecurity.GetCodeValue(pCS, "SEDOL");
                if (StrFunc.IsFilled(code))
                    parameters["SEDOL"].Value = code;
                //
                code = debSecurity.GetCodeValue(pCS, "RIC");
                if (StrFunc.IsFilled(code))
                    parameters["RICCODE"].Value = code;
                //
                code = debSecurity.GetCodeValue(pCS, "Blmbrg");
                if (StrFunc.IsFilled(code))
                    parameters["BBGCODE"].Value = code;
                //
                code = debSecurity.GetCodeValue(pCS, "Sicovam");
                if (StrFunc.IsFilled(code))
                    parameters["SICOVAM"].Value = code;
                //
                code = debSecurity.GetCodeValue(pCS, "Common");
                if (StrFunc.IsFilled(code))
                    parameters["COMMON"].Value = code;
                //
                ISecurity security = debSecurity.DebtSecurity.Security;
                if (null != security)
                {
                    if (security.ClassificationSpecified)
                    {
                        if (security.ClassificationSpecified)
                        {
                            if (security.Classification.DebtSecurityClassSpecified)
                                parameters["SECURITYCLASS"].Value = security.Classification.DebtSecurityClass.Value;
                            if (security.Classification.CfiCodeSpecified)
                                parameters["CFICODE"].Value = security.Classification.CfiCode.Value;
                            if (security.Classification.ProductTypeCodeSpecified)
                                parameters["PRODUCTTYPECODE"].Value = security.Classification.ProductTypeCode.ToString();
                            if (security.Classification.FinancialInstrumentProductTypeCodeSpecified)
                                parameters["FIPRODUCTTYPECODE"].Value = security.Classification.FinancialInstrumentProductTypeCode.ToString();
                        }
                    }
                    if (security.CouponTypeSpecified)
                        parameters["COUPONTYPE"].Value = security.CouponType.Value;
                    //
                    //Issue
                    if (security.CurrencySpecified)
                        parameters["IDC_ISSUE"].Value = Tools.GetIdC(CSTools.SetCacheOn(pCS), security.Currency.Value);
                    if ((security.LocalizationSpecified) && (security.Localization.CountryOfIssueSpecified))
                    {
                        SQL_Country sQLCountry = new SQL_Country(CSTools.SetCacheOn(pCS), SQL_Country.IDType.Iso3166Alpha2, security.Localization.CountryOfIssue.Value, SQL_Table.ScanDataDtEnabledEnum.Yes);
                        if (sQLCountry.LoadTable(new string[] { "IDCOUNTRY" }))
                            parameters["IDCOUNTRY_ISSUE"].Value = sQLCountry.IDCOUNTRY;
                    }

                    if (security.PriceSpecified)
                    {
                        if (security.Price.IssuePricePercentageSpecified)
                            parameters["ISSUEPRICEPCT"].Value = security.Price.IssuePricePercentage.DecValue;
                        if (security.Price.RedemptionPricePercentageSpecified)
                            parameters["REDEMPTIONPRICEPCT"].Value = security.Price.RedemptionPricePercentage.DecValue;
                    }
                }
                //IDM
                int idM = debSecurity.GetIdMarket();
                if (idM > 0)
                    parameters["IDM"].Value = idM;
                //
                //Nominal
                IMoney nominal = debSecurity.GetNominal(product.ProductBase);
                if (nominal.Amount.DecValue > 0)
                    parameters["PARVALUE"].Value = nominal.Amount.DecValue;
                if (StrFunc.IsFilled(nominal.Currency))
                    parameters["IDC_PARVALUE"].Value = Tools.GetIdC(CSTools.SetCacheOn(pCS), nominal.Currency);

                //
                //IInterestRateStream
                if (ArrFunc.IsFilled(debSecurity.DebtSecurity.Stream))
                {
                    InterestRateStreamContainer stream = new InterestRateStreamContainer(debSecurity.DebtSecurity.Stream[0]);
                    if (null != stream.FixedRate)
                        parameters["FIXEDRATE"].Value = stream.FixedRate.InitialValue.DecValue;
                    else if (null != stream.FloatingRateCalculation)
                        parameters["IDASSET"].Value = stream.FloatingRateCalculation.FloatingRateIndex.OTCmlId;

                    if (StrFunc.IsFilled(stream.DayCountFraction))
                    {
                        // FI 20190724 [XXXXX] l'appel à ReflectionTools.ConvertStringToEnum était une erreur pour l'alimentation de TRADEASSET.DCF
                        // => Boulette introduite dans le code main le 23/11/2017 (TFS 26681)
                        //parameters["DCF"].Value = ReflectionTools.ConvertStringToEnum<DayCountFractionEnum>(stream.DayCountFraction);
                        DayCountFractionEnum dcf = (DayCountFractionEnum) Enum.Parse(typeof(DayCountFractionEnum), stream.DayCountFraction);
                        parameters["DCF"].Value = ReflectionTools.ConvertEnumToString<DayCountFractionEnum>(dcf);
                    }

                    if (stream.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                    {
                        // EG 20230505 [XXXXX] [WI617] data optional => controls for Trade template
                        DateTime maturityDate = stream.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue;
                        parameters["MATURITYDATE"].Value = (maturityDate != DateTime.MinValue)? maturityDate:Convert.DBNull;
                    }

                    if (stream.CalculationPeriodDates.FirstPeriodStartDateSpecified)
                    {
                        // EG 20230505 [XXXXX] [WI617] data optional => controls for Trade template
                        DateTime intAccrualDate = stream.CalculationPeriodDates.FirstPeriodStartDate.UnadjustedDate.DateValue;
                        parameters["INTACCRUALDATE"].Value = (intAccrualDate != DateTime.MinValue) ? intAccrualDate : Convert.DBNull;
                    }

                    if (stream.CalculationPeriodDates.EffectiveDateAdjustableSpecified)
                    {
                        // EG 20230505 [XXXXX] [WI617] data optional => controls for Trade template
                        DateTime datedDate = stream.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue;
                        parameters["DATEDDATE"].Value = (datedDate != DateTime.MinValue) ? datedDate : Convert.DBNull;
                    }

                    parameters["PERIODMLTPPAYMENTDT"].Value = stream.PaymentDates.PaymentFrequency.PeriodMultiplier.IntValue;
                    parameters["PERIODPAYMENTDT"].Value = stream.PaymentDates.PaymentFrequency.Period.ToString();

                }

                string sqlInsert = @"insert into dbo.TRADEASSET (
                IDT, IDM, ISINCODE, CUSIP, SEDOL, RICCODE, BBGCODE, SICOVAM, COMMON, SECURITYCLASS, CFICODE, PRODUCTTYPECODE , FIPRODUCTTYPECODE , IDC_ISSUE, IDCOUNTRY_ISSUE, COUPONTYPE, 
                IDASSET, FIXEDRATE, PARVALUE, IDC_PARVALUE, DCF, ISSUEPRICEPCT, REDEMPTIONPRICEPCT, 
                INTACCRUALDATE, DATEDDATE, MATURITYDATE, PERIODMLTPPAYMENTDT, PERIODPAYMENTDT)
                values (
                @IDT, @IDM, @ISINCODE, @CUSIP, @SEDOL, @RICCODE, @BBGCODE, @SICOVAM, @COMMON, @SECURITYCLASS, @CFICODE, @PRODUCTTYPECODE , @FIPRODUCTTYPECODE , @IDC_ISSUE, @IDCOUNTRY_ISSUE, @COUPONTYPE, 
                @IDASSET, @FIXEDRATE, @PARVALUE, @IDC_PARVALUE, @DCF, @ISSUEPRICEPCT, @REDEMPTIONPRICEPCT,
                @INTACCRUALDATE, @DATEDDATE, @MATURITYDATE, @PERIODMLTPPAYMENTDT, @PERIODPAYMENTDT)";
                //
                DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlInsert, parameters.GetArrayDbParameter());
            }
            else
            {
                throw new Exception("Error, Current product is not managed, please contact EFS");
            }
        }

        /// <summary>
        /// Delete de la table TRADEASSET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// FI 20140415 [XXXXX] add Method
        protected override void DeleteTradeAsset(string pCs, IDbTransaction pDbTransaction, int pIdT)
        {
            base.DeleteTradeAsset(pCs, pDbTransaction, pIdT);

            DataParameter paramIdT = new DataParameter(pCs, "IDT", DbType.Int32)
            {
                Value = pIdT
            };
            
            try
            {
                StrBuilder sqlQuery = new StrBuilder(string.Empty);
                sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADEASSET.ToString() + Cst.CrLf;
                sqlQuery += new SQLWhere("IDT=@IDT").ToString();
                DataHelper.ExecuteNonQuery(pCs, pDbTransaction, CommandType.Text, sqlQuery.ToString(), paramIdT.DbDataParameter);
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.query,
                        StrFunc.AppendFormat("Error on delete table TRADEASSET for asset (id:{0})", pIdT.ToString()), ex);
            }
        }
        #endregion
    }
}
