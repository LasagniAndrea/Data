using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ThreadTasks = System.Threading.Tasks;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Enum;
using EFS.ACommon;
using FixML.Enum;
using FixML.v50SP1.Enum;
using FpML.Enum;
using EFS.Common.IO;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// Gestion des CM des DerivativeContrat
    /// </summary>
    internal class MarketDerivativeContractCM : DataTableToUpdateBase
    {
        #region Members
        /// <summary>
        /// Représente le(s) marché(s).
        /// <para>Généralement 1 marché mais pas toujours (exemple sur EUREX CLEARING AG avec 2 marchés => EUREX FRANKFURT (XEUR) et XHEX HELSINKI(XEUR))</para>
        /// </summary>
        private readonly Tuple<MarketColumnIdent, string> m_Market;

        /// <summary>
        /// Identifiant du Css de rattachement des marchés des DC
        /// </summary>
        // PM 20240122 [WI822] New
        private readonly string m_CssIdentifier;

        private readonly DateTime m_DtStart;
        private readonly int m_IdA;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Liste des Derivative Contracts chargés
        /// </summary>
        public List<DerivativeContractIdent> DerivativeContract
        {
            get;
            private set;
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMarket"></param>
        /// <param name="pDtStart"></param>
        /// <param name="pIdA"></param>
        public MarketDerivativeContractCM(Tuple<MarketColumnIdent, string> pMarket, DateTime pDtStart, int pIdA) : base()
        {
            m_Market = pMarket;
            m_DtStart = pDtStart;
            m_IdA = pIdA;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCssIdentifier"></param>
        /// <param name="pDtStart"></param>
        /// <param name="pIdA"></param>
        // PM 20240122 [WI822] New
        public MarketDerivativeContractCM(string pCssIdentifier, DateTime pDtStart, int pIdA) : base()
        {
            m_CssIdentifier = pCssIdentifier;
            m_DtStart = pDtStart;
            m_IdA = pIdA;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Charge les Derivative Contract dont le CM est null 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtBusiness"></param>
        // PM 20240122 [WI822] Rename
        public void LoadDCByMarket(string pCs, DateTime pDtBusiness)
        {
            QueryParameters queryParameters = GetQuerySelectDERIVATIVECONTRACT(pCs, m_Market, pDtBusiness, out string queryForAdapter);

            base.Load(pCs, Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString(), queryParameters, queryForAdapter);

            SetDerivativeContract();
        }

        /// <summary>
        /// Charge les Derivative Contract dont le CM est null 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        // PM 20240122 [WI822] Rename
        public async ThreadTasks.Task LoadDCByMarketAsync(string pCs, DateTime pDtBusiness)
        {
            QueryParameters queryParameters = GetQuerySelectDERIVATIVECONTRACT(pCs, m_Market, pDtBusiness, out string queryForAdapter);

            await base.LoadAsync(pCs, Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString(), queryParameters, queryForAdapter);

            SetDerivativeContract();
        }

        /// <summary>
        /// Charge les Derivative Contract en autosetting d'une chambre
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        // PM 20240122 [WI822] New
        public async ThreadTasks.Task LoadDCByCssAsync(string pCs, DateTime pDtBusiness)
        {
            QueryParameters queryParameters = GetQuerySelectDERIVATIVECONTRACTByCss(pCs, m_CssIdentifier, pDtBusiness, out string queryForAdapter);

            await base.LoadAsync(pCs, Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString(), queryParameters, queryForAdapter);

            SetDerivativeContract();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetDerivativeContract()
        {
            DerivativeContract = (from dr in GetRows()
                                  select new DerivativeContractIdent()
                                  {
                                      IdDC = Convert.ToInt32(dr["IDDC"]),
                                      IdM = Convert.ToInt32(dr["IDM"]),
                                      ContractSymbol = Convert.IsDBNull(dr["CONTRACTSYMBOL"]) ? string.Empty : dr["CONTRACTSYMBOL"].ToString(),
                                      ElectronicContractSymbol = Convert.IsDBNull(dr["ELECCONTRACTSYMBOL"]) ? string.Empty : dr["ELECCONTRACTSYMBOL"].ToString(),
                                      ContractType = ReflectionTools.ConvertStringToEnum<DerivativeContractTypeEnum>(dr["CONTRACTTYPE"].ToString()),
                                      ContractCategory = dr["CATEGORY"].ToString(),
                                      ContractAttribute = Convert.IsDBNull(dr["CONTRACTATTRIBUTE"]) ? string.Empty : dr["CONTRACTATTRIBUTE"].ToString(),
                                      SettlementMethod = ReflectionTools.ConvertStringToEnum<SettlMethodEnum>(dr["SETTLTMETHOD"].ToString()),
                                      ExerciseStyle = ReflectionTools.ConvertStringToEnumOrNullable<DerivativeExerciseStyleEnum>(Convert.IsDBNull(dr["EXERCISESTYLE"]) ? string.Empty : dr["EXERCISESTYLE"].ToString()),
                                  }).ToList();
        }

        /// <summary>
        /// Mise à jour en mémoire d'un Derivative Contract
        /// </summary>
        /// <param name="pIdDC"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pMinPriceAmount"></param>
        /// <param name="pMinPriceIncr"></param>
        public Boolean UpdateDC(int pIdDC, decimal pContractMultiplier, decimal pMinPriceAmount, decimal pMinPriceIncr)
        {
            Boolean ret = false;
            DataRow dr = GetRows().FirstOrDefault(x => Convert.ToInt32(x["IDDC"]) == pIdDC);
            if (dr != default(DataRow) && IsDCUpdatable(dr))
            {
                ret = ((Convert.IsDBNull(dr["CONTRACTMULTIPLIER"]) ||  (Convert.ToDecimal(dr["CONTRACTMULTIPLIER"]) != pContractMultiplier))
                    || (Convert.IsDBNull(dr["MINPRICEINCRAMOUNT"]) ||   (Convert.ToDecimal(dr["MINPRICEINCRAMOUNT"]) != pMinPriceAmount))
                    || (Convert.IsDBNull(dr["MINPRICEINCR"]) ||  (Convert.ToDecimal(dr["MINPRICEINCR"]) != pMinPriceIncr)));

                if (ret)
                {
                    dr["CONTRACTMULTIPLIER"] = pContractMultiplier;
                    dr["MINPRICEINCRAMOUNT"] = pMinPriceAmount;
                    dr["MINPRICEINCR"] = pMinPriceIncr;
                    dr["IDAUPD"] = m_IdA;
                    dr["DTUPD"] = m_DtStart;
                }
            }
            return ret;
        }

        /// <summary>
        /// Mise à jour du Contract Multiplier
        /// </summary>
        /// <param name="pIdDC"></param>
        /// <param name="pContractMultiplier"></param>
        /// <returns></returns>
        // PM 20240122 [WI822] New
        public Boolean UpdateDCMultiplier(int pIdDC, decimal pContractMultiplier)
        {
            Boolean ret = false;
            DataRow dr = GetRows().FirstOrDefault(x => Convert.ToInt32(x["IDDC"]) == pIdDC);
            if (dr != default(DataRow) && IsDCUpdatable(dr))
            {
                ret = (Convert.IsDBNull(dr["CONTRACTMULTIPLIER"]) || (Convert.ToDecimal(dr["CONTRACTMULTIPLIER"]) != pContractMultiplier));

                if (ret)
                {
                    dr["CONTRACTMULTIPLIER"] = pContractMultiplier;
                    dr["IDAUPD"] = m_IdA;
                    dr["DTUPD"] = m_DtStart;
                }
            }
            return ret;
        }

        /// <summary>
        /// Mise à jour en mémoire d'un Derivative Contract
        /// </summary>
        /// <param name="pIdDC"></param>
        /// <param name="pMinPriceIncr"></param>
        public Boolean UpdateDC(int pIdDC, decimal pMinPriceIncr)
        {
            Boolean ret = false;
            DataRow dr = GetRows().FirstOrDefault(x => Convert.ToInt32(x["IDDC"]) == pIdDC);
            if (dr != default(DataRow) && IsDCUpdatable(dr))
            {
                ret = (Convert.IsDBNull(dr["MINPRICEINCR"]) || (Convert.ToDecimal(dr["MINPRICEINCR"]) != pMinPriceIncr));
                if (ret)
                {
                    dr["MINPRICEINCR"] = pMinPriceIncr;
                    dr["IDAUPD"] = m_IdA;
                    dr["DTUPD"] = m_DtStart;
                }
            }
            return ret;
        }

        /// <summary>
        /// Rerourne true si le DC n'a pas déjà été mis à jour
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        private Boolean IsDCUpdatable(DataRow pDr)
        {
            Boolean isOk = (pDr.RowState == DataRowState.Unchanged); // Spheres® ne change pas dc déjà modifié en mémoire
            if (isOk)
            {
                DateTime dtUpd = Convert.IsDBNull(pDr["DTUPD"]) ? DateTime.MinValue : Convert.ToDateTime(pDr["DTUPD"]);
                if (dtUpd == m_DtStart)
                {
                    /// Spheres® ne change pas une modification de DC déjà validée (cad ajoutée/modifiée en base de données) par l'import courant (test sur les dates pour vérifier qu'on est bien sur l'import courant). 
                    /// pour information, lorsque la base est mise à jour le status des lignes est Unchanged <seealso cref="DataTableToUpdateBase.AcceptChanges"/>   
                    isOk = false;
                }
            }
            return isOk;
        }

        /// <summary>
        /// selection des Derivative Contract sans CM et tels que ISAUTOSETTING = 1
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pMarket"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="opQueryForAdapter"></param>
        /// <returns></returns>
        private QueryParameters GetQuerySelectDERIVATIVECONTRACT(string pCs, Tuple<MarketColumnIdent, string> pMarket, DateTime pDtBusiness, out string opQueryForAdapter)
        {
            string query = $@"select dc.IDDC, dc.IDENTIFIER, dc.IDM, 
dc.CONTRACTSYMBOL, dc.ELECCONTRACTSYMBOL, dc.CONTRACTTYPE, dc.CATEGORY, dc.CONTRACTATTRIBUTE, dc.SETTLTMETHOD, dc.EXERCISESTYLE,
dc.CONTRACTMULTIPLIER, dc.MINPRICEINCRAMOUNT, dc.MINPRICEINCR, 
dc.IDAUPD, dc.DTUPD
from dbo.DERIVATIVECONTRACT dc
#joinMarket#
where (dc.ISAUTOSETTING = 1)
#predicatMarket#
and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE), pDtBusiness);

            opQueryForAdapter = query.Replace($"#joinMarket#{Cst.CrLf}", string.Empty);
            opQueryForAdapter = opQueryForAdapter.Replace($"#predicatMarket#{Cst.CrLf}", string.Empty);
            QueryParameters qryParameters = new QueryParameters(pCs, opQueryForAdapter.ToString(), dataParameters);
            opQueryForAdapter = qryParameters.GetQueryReplaceParameters();

            string querycmd = query.Replace("#joinMarket#", "inner join dbo.MARKET mk on (mk.IDM = dc.IDM)");
            querycmd = querycmd.Replace("#predicatMarket#", $"and (mk.{pMarket.Item1} = '{pMarket.Item2}')");
            qryParameters = new QueryParameters(pCs, querycmd.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Selection des Derivative Contract d'une chambre tels que ISAUTOSETTING = 1
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCssIdentifier"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="opQueryForAdapter"></param>
        /// <returns></returns>
        // PM 20240122 [WI822] New
        private QueryParameters GetQuerySelectDERIVATIVECONTRACTByCss(string pCs, string pCssIdentifier, DateTime pDtBusiness, out string opQueryForAdapter)
        {
            string query = $@"select dc.IDDC, dc.IDENTIFIER, dc.IDM, 
dc.CONTRACTSYMBOL, dc.ELECCONTRACTSYMBOL, dc.CONTRACTTYPE, dc.CATEGORY, dc.CONTRACTATTRIBUTE, dc.SETTLTMETHOD, dc.EXERCISESTYLE,
dc.CONTRACTMULTIPLIER, dc.MINPRICEINCRAMOUNT, dc.MINPRICEINCR, 
dc.IDAUPD, dc.DTUPD
from dbo.DERIVATIVECONTRACT dc
#joinCss#
where (dc.ISAUTOSETTING = 1)
#predicatCss#
and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE), pDtBusiness);

            opQueryForAdapter = query.Replace($"#joinCss#{Cst.CrLf}", string.Empty);
            opQueryForAdapter = opQueryForAdapter.Replace($"#predicatCss#{Cst.CrLf}", string.Empty);
            QueryParameters qryParameters = new QueryParameters(pCs, opQueryForAdapter.ToString(), dataParameters);
            opQueryForAdapter = qryParameters.GetQueryReplaceParameters();

            string querycmd = query.Replace("#joinCss#", $"inner join dbo.MARKET mk on (mk.IDM = dc.IDM){Cst.CrLf}inner join dbo.ACTOR a on (a.IDA = mk.IDA)");
            querycmd = querycmd.Replace("#predicatCss#", $"and (a.IDENTIFIER = '{pCssIdentifier}')");
            qryParameters = new QueryParameters(pCs, querycmd.ToString(), dataParameters);

            return qryParameters;
        }
        #endregion Methods
    }
}
 
