#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Status;
using EFS.Tuning;
//
using EfsML.Business;
using EfsML.EarAcc;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.v30.Fix;
//
using FpML.Interface;
#endregion Using Directives

// RD 20120809 [18070] Optimisation
namespace EFS.Process.EarGen
{
    /// <summary>
    /// Fonction mathématique gérées pour les EarCalc
    /// </summary>
    /// RD 20150814 [21263] Add PROD and PRO2
    /// FI 20151027 [21513] Modify
    public enum AGFuncEnum
    {
        /// <summary>
        /// Moyenne
        /// <para>C’est la moyenne des différents montants avec une notion de netting entre montant payé et montant reçu</para>
        /// </summary>
        AVG,
        /// <summary>
        /// Différence
        /// <para>C’est la différence entre DEUX (et seulement DEUX) montants avec une notion de netting entre montant payé et montant reçu.</para>
        /// <para>Ainsi seul les deux premiers N-uplets sont pris en compte.</para>        
        /// <para>NB: Si un seul N-uplet est spécifié, le calcul se fait entre ce N-uplet et son homologue disposant d’un EarCode de type xxx-1.</para>
        /// </summary>
        DIFF,
        /// <summary>
        /// Différence
        /// <para>De fonctionnement identique à la fonction DIFF avec:</para>
        /// <para>- Recherche du 2ème N-uplet sur le stream suivant de même sens</para>
        /// <para>- et en croisant les montants payés et les montants reçus.</para>
        /// </summary>
        DIF2,
        /// <summary>
        /// Produit mathématique
        /// <para>Cette fonction opère un produit mathématique entre DEUX (et seulement DEUX) montants:</para>
        /// <para>- Le montant payé correspondant au premier N-uplets est multiplié par le montant payé correspondant au deuxième N-uplets.</para>
        /// <para>- Le montant reçu correspondant au premier N-uplets est multiplié par le montant reçu correspondant au deuxième N-uplets.</para>
        /// <para>NB: Si un seul N-uplet est spécifié, le produit se fait entre ce N-uplet et lui même pour simuler la fonction mathématique CARREE (puissance 2).</para>
        /// </summary>
        PROD,
        /// <summary>
        /// Produit mathématique
        /// <para>De fonctionnement identique à la fonction PROD avec:</para>
        /// <para>- Pour le montant 2 elle croise les montants payé et reçu</para>
        /// <para>- Le montant payé correspondant au premier N-uplets est multiplié par le montant reçu correspondant au deuxième N-uplets.</para>
        /// <para>- Le montant reçu correspondant au premier N-uplets est multiplié par le montant payé correspondant au deuxième N-uplets.</para>
        /// </summary>
        PRO2,
        /// <summary>
        /// Somme
        /// <para>C’est la somme des différents montants avec une notion de netting entre montant payé et montant reçu</para>
        /// </summary>
        SUM,
        /// <summary>
        /// PRORATA
        /// </summary>
        /// FI 20151027 [21513] Add
        PRORATA,
        /// <summary>
        /// Aucun
        /// </summary>
        None,
    }

    #region public class EarQuote
    /// <revision>
    ///     <version>1.1.5</version><date>20070412</date><author>EG</author>
    ///     <EurosysSupport>N° 15435</EurosysSupport>
    ///     <comment>
    ///     Gestion du cours inverse, Méthode ObservedRate muni d'un paramètre pIsInverse pour appliquer
    ///     un cours inverse à la cotation d'asset
    ///     Mise en private de tous les membres de la classe
    ///		</comment>
    /// </revision>
    public class EarQuote
    {
        #region Members
        private readonly int m_IdQuote;
        private readonly int m_IdAsset;
        private readonly decimal m_ObservedRate;
        private readonly FpML.Enum.QuoteBasisEnum m_QuoteBasis;
        private readonly string m_IdMarketEnv;
        private readonly string m_IdValScenario;
        private readonly ProcessStateTools.StatusEnum m_IdStProcess;
        private readonly DateTime m_AdjustedDate;
        #endregion Members
        #region Accessors
        #region IdQuote
        public int IdQuote
        {
            get { return m_IdQuote; }
        }
        #endregion IdQuote
        #region IdAsset
        public int IdAsset
        {
            get { return m_IdAsset; }
        }
        #endregion IdAsset
        #region IdMarketEnv
        public string IdMarketEnv
        {
            get { return m_IdMarketEnv; }
        }
        #endregion IdMarketEnv
        #region IdValScenario
        public string IdValScenario
        {
            get { return m_IdValScenario; }
        }
        #endregion IdValScenario
        #region IdStProcess
        public ProcessStateTools.StatusEnum IdStProcess
        {
            get { return m_IdStProcess; }
        }
        #endregion IdStProcess
        #region ObservedRate
        public decimal ObservedRate
        {
            get { return m_ObservedRate; }
        }
        #endregion ObservedRate
        #region AdjustedDate
        public DateTime AdjustedDate
        {
            get { return m_AdjustedDate; }
        }
        #endregion AdjustedDate
        #endregion Accessors

        #region Constructors
        public EarQuote(DateTime pAdjustedDate, int pIdquote, decimal pObservedRate, FpML.Enum.QuoteBasisEnum pQuoteBasis)
        {
            m_IdQuote = pIdquote;
            m_ObservedRate = pObservedRate;
            m_QuoteBasis = pQuoteBasis;
            m_IdStProcess = ProcessStateTools.StatusSuccessEnum;
            m_AdjustedDate = pAdjustedDate;
        }

        public EarQuote(DateTime pAdjustedDate, int pIdquote, int pIdAsset, decimal pObservedRate, string pIdMarketEnv, string pIdValScenario, FpML.Enum.QuoteBasisEnum pQuoteBasis)
        {
            m_IdQuote = pIdquote;
            m_ObservedRate = pObservedRate;
            m_QuoteBasis = pQuoteBasis;
            m_IdStProcess = ProcessStateTools.StatusSuccessEnum;
            m_AdjustedDate = pAdjustedDate;
            m_IdAsset = pIdAsset;
            m_IdMarketEnv = pIdMarketEnv;
            m_IdValScenario = pIdValScenario;
        }

        public EarQuote(DateTime pAdjustedDate, int pIdquote, decimal pObservedRate, FpML.Enum.QuoteBasisEnum pQuoteBasis, ProcessStateTools.StatusEnum pIdStProcess)
        {
            m_IdQuote = pIdquote;
            m_ObservedRate = pObservedRate;
            m_QuoteBasis = pQuoteBasis;
            m_IdStProcess = pIdStProcess;
            m_AdjustedDate = pAdjustedDate;
        }
        #endregion Constructors

        #region Methods
        #region QuoteBasis
        public FpML.Enum.QuoteBasisEnum QuoteBasis(bool pIsInverse)
        {
            if (pIsInverse)
            {
                if (FpML.Enum.QuoteBasisEnum.Currency1PerCurrency2 == m_QuoteBasis)
                    return FpML.Enum.QuoteBasisEnum.Currency2PerCurrency1;
                else
                    return FpML.Enum.QuoteBasisEnum.Currency1PerCurrency2;
            }
            else
                return m_QuoteBasis;
        }
        #endregion QuoteBasis
        #endregion Methods
    }
    #endregion EarQuote

    #region public class EarCalcAmountQuery
    public class EarCalcAmountQuery
    {
        /// <summary>
        /// Query d'insert dans TMPEARCALCAMOUNT
        /// </summary>
        public string SQL_Insert;
        /// <summary>
        /// Query d'insert dans TMPEARCALCDET
        /// </summary>
        public string SQL_Insert_Det;

        public string SQL_Amount1;
        public string SQL_Select_Amount2;
        public string SQL_Inner_Amount2;
        public string SQL_Where_Amount2;

        /// <summary>
        /// Query for insert all details involved in calculation of result
        /// <para>Insert into TMPEARCALCDET</para>
        /// </summary>
        public string SQL_AmountDet;
        /// <summary>
        /// Requête de lecture de la table TMPEARCALCAMOUNT
        /// </summary>
        public string SQL_Select_EarCalcAmount;
        public string SQL_Select_EarCalcEvent;
        
        /// <summary>
        ///  Constitution des requêtes
        /// </summary>
        /// FI 20160523 [22188] Modify
        public EarCalcAmountQuery()
        {
            // Query for insert result of calculated amount
            SQL_Insert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TMPEARCALCAMOUNT + Cst.CrLf;
            SQL_Insert += "(IDEAR, EARCODE, EVENTCLASS, EXCHANGETYPE, IDC, PAID, RECEIVED, IDSTPROCESS, CALCTYPE, DTACCOUNT {PARAMETERS})" + Cst.CrLf;
            SQL_Insert += @"
select IDEAR, {EARCODE}, {EVENTCLASS}, EXCHANGETYPE, IDC, 
       {PAID},{RECEIVED}, IDSTPROCESS, @CALCTYPE, {DTACCOUNT} {PARAMETERS}
from dbo.VW_EARALLAMOUNT
where ({AMOUNTTYPE}) and (IDEAR = @IDEAR) and (IDSTPROCESS = @IDSTPROCESS)and (EXCHANGETYPE = @EXCHANGETYPE)
group by IDEAR {BYEARCODE} {BYEVENTCLASS}, EXCHANGETYPE, IDC, IDSTPROCESS {BYDTACCOUNT} {PARAMETERS}";


            // Query for insert all details involved in calculation of result
            SQL_Insert_Det = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TMPEARCALCDET + Cst.CrLf;
            SQL_Insert_Det += "(IDEARCALC, IDEARTYPE, EARTYPE)" + Cst.CrLf;

            // FI 20160523 [22188] Mise en commentaire 
            // SQL_Insert_Det += SQLCst.SELECT_DISTINCT + Cst.CrLf; 
            SQL_Insert_Det += @"
select distinct teca.IDEARCALC, veaa.IDEARTYPE, veaa.EARTYPE
from dbo.VW_EARALLAMOUNT veaa
inner join dbo.TMPEARCALCAMOUNT teca on (teca.IDEAR = veaa.IDEAR) and ({INSTRUMENTNO}) and ({STREAMNO}) 
                                            and (teca.EARCODE = {EARCODE}) 
                                            and (teca.EVENTCLASS = {EVENTCLASS})
                                            and (teca.EXCHANGETYPE = veaa.EXCHANGETYPE) 
                                            and (teca.IDC = veaa.IDC) 
                                            and (teca.CALCTYPE = @CALCTYPE)
                                            and (teca.IDSTPROCESS = veaa.IDSTPROCESS) 
                                            and (teca.DTACCOUNT = {DTACCOUNT})
where 
({AMOUNTTYPE})
and (veaa.IDEAR=@IDEAR)
and (veaa.IDSTPROCESS  = @IDSTPROCESS)
and (veaa.EXCHANGETYPE = @EXCHANGETYPE)";



            SQL_Amount1 = SQLCst.SELECT + "earAmount.PAID, earAmount.RECEIVED, " + Cst.CrLf;
            SQL_Amount1 += "earAmount.DTACCOUNT, earAmount.IDC, earAmount.INSTRUMENTNO, earAmount.STREAMNO, " + Cst.CrLf;
            SQL_Amount1 += "earAmount.IDEARTYPE, earAmount.EARTYPE" + Cst.CrLf;
            SQL_Amount1 += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EARALLAMOUNT.ToString() + " earAmount" + Cst.CrLf;
            SQL_Amount1 += SQLCst.WHERE + "earAmount.AMOUNTTYPE = @AMOUNTTYPE" + Cst.CrLf;
            SQL_Amount1 += SQLCst.AND + "earAmount.EARCODE      = @EARCODE" + Cst.CrLf;
            SQL_Amount1 += SQLCst.AND + "earAmount.EVENTCLASS   = @EVENTCLASS" + Cst.CrLf;
            SQL_Amount1 += SQLCst.AND + "earAmount.IDEAR        = @IDEAR " + Cst.CrLf;
            SQL_Amount1 += SQLCst.AND + "earAmount.IDSTPROCESS  = @IDSTPROCESS" + Cst.CrLf;
            SQL_Amount1 += SQLCst.AND + "earAmount.EXCHANGETYPE = @EXCHANGETYPE" + Cst.CrLf;
            SQL_Amount1 += SQLCst.ORDERBY + "earAmount.INSTRUMENTNO, earAmount.STREAMNO, earAmount.EXCHANGETYPE" + Cst.CrLf;

            SQL_Select_Amount2 = SQLCst.SELECT + Cst.CrLf;
            SQL_Select_Amount2 += "vear.RECEIVED, vear.PAID, vear.IDEARTYPE, vear.EARTYPE" + Cst.CrLf;
            SQL_Select_Amount2 += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EARALLAMOUNT.ToString() + " vear" + Cst.CrLf;
            SQL_Select_Amount2 += "{BYEVENT_JOIN}";

            SQL_Inner_Amount2 = SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EAR.ToString() + " ear on ear.IDEAR=vear.IDEAR";
            SQL_Inner_Amount2 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTREAM.ToString() + " ts1 on ts1.IDT=ear.IDT and ts1.INSTRUMENTNO=vear.INSTRUMENTNO";
            SQL_Inner_Amount2 += " and ts1.STREAMNO=@STREAMNO";
            SQL_Inner_Amount2 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTREAM.ToString() + " ts2 on ts2.IDT=ear.IDT and ts2.INSTRUMENTNO=vear.INSTRUMENTNO";
            SQL_Inner_Amount2 += " and ts2.STREAMNO!=ts1.STREAMNO  and ts2.STREAMNO=vear.STREAMNO";
            SQL_Inner_Amount2 += " and ts2.IDA_PAY=ts1.IDA_PAY and ts2.IDA_REC=ts1.IDA_REC";

            SQL_Where_Amount2 = SQLCst.WHERE + "vear.AMOUNTTYPE   = @AMOUNTTYPE" + Cst.CrLf;
            SQL_Where_Amount2 += SQLCst.AND + "vear.EARCODE      = @EARCODE" + Cst.CrLf;
            SQL_Where_Amount2 += SQLCst.AND + "vear.EVENTCLASS   = @EVENTCLASS" + Cst.CrLf;
            SQL_Where_Amount2 += SQLCst.AND + "vear.IDEAR        = @IDEAR " + Cst.CrLf;
            SQL_Where_Amount2 += SQLCst.AND + "vear.IDSTPROCESS  = @IDSTPROCESS" + Cst.CrLf;
            SQL_Where_Amount2 += SQLCst.AND + "vear.EXCHANGETYPE = @EXCHANGETYPE" + Cst.CrLf;
            SQL_Where_Amount2 += "{BYINSTRUMENTNO}";
            SQL_Where_Amount2 += "{BYSTREAMNO}";
            SQL_Where_Amount2 += "{BYDTEVENT}";
            SQL_Where_Amount2 += "{BYEVENT_WHERE}";

            // 
            SQL_AmountDet = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TMPEARCALCDET + Cst.CrLf;
            SQL_AmountDet += "(IDEARCALC, IDEARTYPE, EARTYPE)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.SELECT_DISTINCT + Cst.CrLf;
            SQL_AmountDet += "teca.IDEARCALC, @IDEARTYPE, @EARTYPE" + Cst.CrLf;
            SQL_AmountDet += SQLCst.FROM_DBO + Cst.OTCml_TBL.TMPEARCALCAMOUNT.ToString() + " teca" + Cst.CrLf;
            SQL_AmountDet += SQLCst.WHERE + "(teca.IDEAR = @IDEAR)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.INSTRUMENTNO = @INSTRUMENTNO)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.STREAMNO = @STREAMNO)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.EARCODE = @EARCODE)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.EVENTCLASS = @EVENTCLASS)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.EXCHANGETYPE = @EXCHANGETYPE)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.IDC = @IDC)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.IDSTPROCESS = @IDSTPROCESS)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.CALCTYPE = @CALCTYPE)" + Cst.CrLf;
            SQL_AmountDet += SQLCst.AND + "(teca.DTACCOUNT = @DTACCOUNT)" + Cst.CrLf;


            SQL_Select_EarCalcAmount = @"
select teca.IDEAR, teca.IDEARCALC, teca.DTACCOUNT, teca.INSTRUMENTNO, teca.STREAMNO,
teca.EARCODE, teca.CALCTYPE, teca.EVENTCLASS, teca.EXCHANGETYPE, teca.IDC,
teca.PAID, teca.RECEIVED, teca.IDSTPROCESS, defec.AGFUNC, defec.AGAMOUNTS
from dbo.TMPEARCALCAMOUNT teca
inner join dbo.DEFINEEARCALC defec on defec.CALCTYPE = teca.CALCTYPE
                                        and defec.SEQUENCENO = @SEQUENCENO
                                        and defec.CALCTYPE = @CALCTYPE
where teca.IDEAR = @IDEAR and teca.EXCHANGETYPE = @EXCHANGETYPE";


            SQL_Select_EarCalcEvent = @"
select distinct vee.IDE
from dbo.TMPEARCALCDET tecd
inner join dbo.VW_EAREVENT vee on vee.IDEARTYPE = tecd.IDEARTYPE and vee.EARTYPE = tecd.EARTYPE
where (tecd.IDEARCALC = @IDEARCALC)";

        }
    }
    #endregion public class EarCalcAmountQuery
    #region public class DefineEarCalc
    public class DefinedEarCalc
    {
        #region Members
        public string calcType;
        public string[] agLstAmounts;
        public AGFuncEnum agFunction;
        public int ida;

        private readonly string _agAmounts;
        private readonly bool _isByInstrument;
        private readonly bool _isByStream;
        private readonly bool _isByDtEvent;
        private readonly bool _isByEvent;
        private string _tableAlias;
        #endregion Members
        #region Accessors
        #region AmountType
        private static bool IsParamExistInList(string pParamName, ArrayList pParamList)
        {
            foreach (IDbDataParameter idbParam in pParamList)
            {
                if (idbParam.ParameterName.Substring(1) == pParamName.Substring(1))
                    return true;
            }
            //
            return false;
        }
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        private string GetAmountTypeList(string pCs, string pAgsAmountType, ref ArrayList pParamList, int pItemNO)
        {
            const string AmountTypeLike = "AMOUNTTYPE" + SQLCst.LIKE;
            //
            string retAmountType = string.Empty;
            string[] agsAmountType = pAgsAmountType.Split(',');
            int itemNO = 0;

            foreach (string item1 in agsAmountType)
            {
                string paramName = "AMOUNTTYPE" + pItemNO.ToString() + itemNO.ToString();

                if (false == IsParamExistInList("@" + paramName, pParamList))
                {
                    IDbDataParameter param = new DataParameter(pCs, paramName, DbType.AnsiString, SQLCst.UT_AMOUNTTYPE_LEN).DbDataParameter;
                    param.Value = item1.Trim();
                    pParamList.Add(param);
                }
                retAmountType += _tableAlias + AmountTypeLike + "@" + paramName + SQLCst.OR;
                itemNO++;
            }
            //
            if (retAmountType.EndsWith(SQLCst.OR))
                retAmountType = "(" + retAmountType.Substring(0, retAmountType.Length - SQLCst.OR.ToString().Length) + ")" + SQLCst.AND;
            //
            return retAmountType;
        }
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        public string GetSqlAmount(string pCs, ref ArrayList pParamList)
        {
            int itemNO = 0;
            string amount = string.Empty;

            if (StrFunc.IsFilled(agLstAmounts.ToString()))
            {
                foreach (string item in agLstAmounts)
                {
                    string itemAmount = string.Empty;
                    //
                    string[] agsAmount = item.Split(';');
                    //
                    if (agsAmount.Length == 1)
                    {
                        if (StrFunc.IsFilled(agsAmount[0]))
                            itemAmount += GetAmountTypeList(pCs, agsAmount[0], ref pParamList, itemNO);
                        //
                        if (itemAmount.EndsWith(SQLCst.AND))
                            itemAmount = itemAmount.Substring(0, itemAmount.Length - SQLCst.AND.ToString().Length);
                    }
                    else
                    {
                        IDbDataParameter param;
                        string paramName;
                        if (StrFunc.IsFilled(agsAmount[0]))
                        {
                            paramName = "EARCODE" + itemNO.ToString();
                            //
                            if (false == IsParamExistInList("@" + paramName, pParamList))
                            {
                                param = new DataParameter(pCs, paramName, DbType.AnsiString, SQLCst.UT_EARCODE_LEN).DbDataParameter;
                                param.Value = agsAmount[0].Trim();
                                //
                                pParamList.Add(param);
                            }
                            //
                            itemAmount += _tableAlias + "EARCODE = @" + paramName + SQLCst.AND;
                        }
                        //
                        if (StrFunc.IsFilled(agsAmount[1]))
                            itemAmount += GetAmountTypeList(pCs, agsAmount[1], ref pParamList, itemNO);
                        //
                        if (agsAmount.Length > 2 && StrFunc.IsFilled(agsAmount[2]))
                        {
                            paramName = "EVENTCLASS" + itemNO.ToString();
                            //
                            if (false == IsParamExistInList("@" + paramName, pParamList))
                            {
                                param = new DataParameter(pCs, paramName, DbType.AnsiString, SQLCst.UT_EVENT_LEN).DbDataParameter;
                                param.Value = agsAmount[2].Trim();
                                //
                                pParamList.Add(param);
                            }
                            //
                            itemAmount += _tableAlias + "EVENTCLASS = @" + paramName + SQLCst.AND;
                        }
                        //
                        if (itemAmount.EndsWith(SQLCst.AND))
                            itemAmount = "(" + itemAmount.Substring(0, itemAmount.Length - SQLCst.AND.ToString().Length) + ")";
                    }
                    //
                    if (StrFunc.IsFilled(itemAmount))
                        amount += itemAmount + SQLCst.OR;
                    //
                    itemNO++;
                }
            }
            //
            if (amount.EndsWith(SQLCst.OR))
                amount = amount.Substring(0, amount.Length - SQLCst.OR.ToString().Length);
            //
            return amount;
        }
        /*
        public string[] DiffAgsAmount
        {
            get
            {
                string[] agsAmount = new string[1] { " " };

                if (StrFunc.IsFilled(agLstAmounts.ToString()))
                {
                    string item = agLstAmounts[0];
                    //
                    if (StrFunc.IsFilled(item))
                        agsAmount = item.Split(';');
                }
                return agsAmount;
            }
        }
        */
        #endregion AmountType
        #region Parameters
        public string Parameters
        {
            get
            {
                string parameters = string.Empty;
                if (_isByInstrument)
                    parameters = ", INSTRUMENTNO";
                if (_isByStream)
                    parameters += ", STREAMNO";
                //				if (_isByIsPayment)
                //					parameters += ", ISPAYMENT";
                return parameters;
            }
        }
        #endregion Parameters
        #region IsByInstrument
        public bool IsByInstrument
        {
            get { return _isByInstrument; }
        }
        #endregion IsByInstrument
        #region IsByStream
        public bool IsByStream
        {
            get { return _isByStream; }
        }
        #endregion IsByStream
        #region IsByDtEvent
        public bool IsByDtEvent
        {
            get { return _isByDtEvent; }
        }
        #endregion IsByDtEvent
        #region IsByEvent
        public bool IsByEvent
        {
            get { return _isByEvent; }
        }
        #endregion IsByEvent

        #region Function
        /// <summary>
        /// 
        /// </summary>
        // FI 20151027 [21513] Modify
        public string Function
        {
            get
            {
                string function = string.Empty;
                switch (agFunction)
                {
                    case AGFuncEnum.DIF2://20070726 PL Temporary...
                    case AGFuncEnum.DIFF:
                    case AGFuncEnum.PRO2:
                    case AGFuncEnum.PROD:
                    case AGFuncEnum.PRORATA: // FI 20151027 [21513] 
                        break;
                    case AGFuncEnum.SUM:
                    case AGFuncEnum.AVG:
                        function = "case when " + agFunction.ToString() + "(RECEIVED-PAID)>=0 then @CASETRUE else @CASEFALSE end as @LABEL";
                        break;
                    default:
                        break;
                }
                return function;
            }
        }
        #endregion Function
        #region Amount
        public string Amount
        {
            get
            {
                string amount = string.Empty;
                switch (agFunction)
                {
                    case AGFuncEnum.DIF2://20070726 PL Temporary...
                    case AGFuncEnum.DIFF:
                    case AGFuncEnum.PRO2:
                    case AGFuncEnum.PROD:
                        break;
                    case AGFuncEnum.SUM:
                    case AGFuncEnum.AVG:
                        amount = "ABS(" + agFunction.ToString() + "(RECEIVED-PAID))";
                        break;
                    default:
                        break;
                }
                return amount;
            }
        }
        #endregion Amount
        #region Paid
        public string Paid
        {
            get { return Function.Replace("@CASETRUE", "0").Replace("@CASEFALSE", Amount).Replace("@LABEL", "PAID"); }
        }
        #endregion Paid
        #region Received
        public string Received
        {
            get { return Function.Replace("@CASETRUE", Amount).Replace("@CASEFALSE", "0").Replace("@LABEL", "RECEIVED"); }
        }
        #endregion Received
        #region TableAlias
        public string TableAlias
        {
            set { _tableAlias = value; }
            get { return _tableAlias; }
        }
        #endregion TableAlias
        public string Data
        {
            get
            {
                return " = " + agFunction.ToString() + "(" +
                    agLstAmounts[0] + (agLstAmounts.Length > 1 ? "&" +
                    agLstAmounts[1] : string.Empty) + ")";
            }
        }
        #endregion Accessors
        #region Constructors
        public DefinedEarCalc() { }
        public DefinedEarCalc(string pCalcType, string pAgFunction, string pAgAmounts, int pIda,
            bool pIsByInstrument, bool pIsByStream, bool pIsByDtEvent, bool pIsByEvent)
        {
            calcType = pCalcType;

            agFunction = AGFuncEnum.None;
            if (Enum.IsDefined(typeof(AGFuncEnum), pAgFunction.Trim()))
                agFunction = (AGFuncEnum)Enum.Parse(typeof(AGFuncEnum), pAgFunction, true);

            _agAmounts = pAgAmounts;
            if (_agAmounts.IndexOf(@"&") > 0)
                agLstAmounts = _agAmounts.Split('&');
            else
                //20070718 PL Pour compatibilité ascendante
                agLstAmounts = _agAmounts.Split('|');

            ida = pIda;
            _isByInstrument = pIsByInstrument;
            _isByStream = pIsByStream;
            _isByDtEvent = pIsByDtEvent;
            _isByEvent = pIsByEvent;

            _tableAlias = string.Empty;
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] DiffAgsAmount()
        {
            string[] agsAmount = new string[1] { " " };
            if (StrFunc.IsFilled(agLstAmounts.ToString()))
            {
                string item = agLstAmounts[0];
                //
                if (StrFunc.IsFilled(item))
                    agsAmount = item.Split(';');
            }
            return agsAmount;
        }
        #endregion Methods
    }
    #endregion DefineEarCalc

    /// <summary>
    /// Informations générales sur le trade
    /// </summary>
    public class EarTradeInfo
    {
        private readonly string m_Cs;
        private readonly DataSet m_DsTrade;
        private readonly SQL_Product m_SQL_Product;
        private readonly ExchangeTradedDerivative m_UnknownProduct;

        public DataTable DtTrade
        {
            get { return m_DsTrade.Tables["Trade"]; }
        }

        public DataTable DtTradeActor
        {
            get { return m_DsTrade.Tables["TradeActor"]; }
        }

        public DataTable DtTradeInstrument
        {
            get { return m_DsTrade.Tables["TradeInstrument"]; }
        }

        public int IdI
        {
            get { return Convert.ToInt32(DtTrade.Rows[0]["IDI"]); }
        }

        public int IdP
        {
            get { return Convert.ToInt32(DtTrade.Rows[0]["IDP"]); }
        }

        public SQL_Product Product
        {
            get { return m_SQL_Product; }
        }

        public ExchangeTradedDerivative UnknownProduct
        {
            get { return m_UnknownProduct; }
        }

        public bool IsAlloc
        {
            get { return (DtTrade.Rows[0]["IDSTBUSINESS"].ToString() == Cst.StatusBusiness.ALLOC.ToString()); }
        }

        public bool IsEarExchange
        {
            get { return (DtTrade.Rows[0]["EAREXCHANGE"].ToString() == Cst.EarExchangeEnum.YES.ToString()); }
        }

        public bool IsDeactiv
        {
            get { return (DtTrade.Rows[0]["IDSTACTIVATION"].ToString() == Cst.StatusActivation.DEACTIV.ToString()); }
        }

        public bool IsETDAlloc
        {
            get { return m_SQL_Product.IsExchangeTradedDerivative && IsAlloc; }
        }
        /// <summary>
        /// Obtient si produit ExchangeTradedDerivative ou GPRODUCT= 'RISK'
        /// </summary>
        public bool IsETDContext
        {
            get { return (Product.IsExchangeTradedDerivative || Product.IsRiskProduct); }
        }

        public bool IsOTCContext
        {
            get { return (Product.GProduct == Cst.ProductGProduct_OTC); }
        }

        public EarTradeInfo(string pCs, int pIdt)
        {
            m_Cs = pCs;

            // Load trade infos
            DataParameter paramIdT = new DataParameter(m_Cs, "IDT", DbType.Int32)
            {
                Value = pIdt
            };

            StrBuilder sqlSelect = new StrBuilder(string.Empty);
            sqlSelect += GetSelectTradeColumn() + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += GetSelectTradeActorColumn() + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += GetSelectTradeInstrumentColumn() + SQLCst.SEPARATOR_MULTISELECT;

            m_DsTrade = DataHelper.ExecuteDataset(m_Cs, CommandType.Text, sqlSelect.ToString(), paramIdT.DbDataParameter);

            m_DsTrade.DataSetName = "TradeInfo";
            m_DsTrade.Tables[0].TableName = "Trade";
            m_DsTrade.Tables[1].TableName = "TradeActor";
            m_DsTrade.Tables[2].TableName = "TradeInstrument";

            // Load product
            m_SQL_Product = new SQL_Product(CSTools.SetCacheOn(m_Cs), IdP);
            m_SQL_Product.LoadTable(new string[] { "IDP", "IDENTIFIER", "FAMILY", "GPRODUCT" });

            // Unknown product
            m_UnknownProduct = new ExchangeTradedDerivative();
        }

        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private string GetSelectTradeColumn()
        {
            return @"select t.IDI, t.IDENTIFIER, i.EAREXCHANGE, i.IDP, t.IDSTACTIVATION, t.IDSTBUSINESS
            from dbo.TRADE t
            inner join dbo.INSTRUMENT i on (i.IDI = t.IDI)
            where (t.IDT = @IDT)";
        }

        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private string GetSelectTradeActorColumn()
        {
            return @"select a.IDA,a.IDENTIFIER,t.IDB,t.BUYER_SELLER,t.FIXPARTYROLE,b.IDENTIFIER as BOOK_IDENTIFIER, b.IDA_ENTITY
            from dbo.ACTOR a
            inner join dbo.TRADEACTOR t on (t.IDT = @IDT) and (t.IDA = a.IDA)
            inner join dbo.BOOK b on (b.IDB = t.IDB)
            where (b.IDA_ENTITY is not null)";
        }

        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private string GetSelectTradeInstrumentColumn()
        {
            return @"select t.IDI,t.INSTRUMENTNO,i.IDP
            from dbo.VW_ALLTRADEINSTRUMENT t
            inner join dbo.INSTRUMENT i on (i.IDI = t.IDI)
            where (t.IDT = @IDT)";
        }

        public bool IsFx(int pInstrumentNo)
        {
            DataRow[] currentTradeInstrument = DtTradeInstrument.Select("INSTRUMENTNO=" + pInstrumentNo.ToString());
            int currentIDP = Convert.ToInt32(currentTradeInstrument[0]["IDP"]);

            SQL_Product sql_Product = new SQL_Product(CSTools.SetCacheOn(m_Cs), currentIDP);
            sql_Product.LoadTable(new string[] { "IDP", "IDENTIFIER", "FAMILY", "GPRODUCT" });

            return (sql_Product.Family.Trim() == Cst.ProductFamily_FX);
        }
    }

    /// <summary>
    /// Informations générales sur les évènements trade (évènements inclus)
    /// </summary>
    public class EarEventInfo : DataSetEventTrade
    {
        private readonly ProcessTuningOutput m_Tuning;
        private readonly string m_GProduct;

        public ProcessTuningOutput Tuning
        {
            get { return m_Tuning; }
        }
        public EarEventInfo(string pConnectionString, int pIdT, string pGProduct, ProcessTuningOutput pTuning)
            : base()
        {
            m_cs = pConnectionString;
            m_DbTransaction = null;
            m_Tuning = pTuning;
            m_GProduct = pGProduct;
            m_IdT = pIdT;
            Load();
        }

        public override void Load()
        {
            DataParameters dataParams = new DataParameters();
            SQLWhere sqlWhere = new SQLWhere();
            string sqlWhereGProduct = string.Empty;

            bool isWithStCheck = ((m_Tuning != null) && m_Tuning.IdStSpecified(StatusEnum.StatusCheck));
            bool isWithStMatch = ((m_Tuning != null) && m_Tuning.IdStSpecified(StatusEnum.StatusMatch));

            if (m_IdT > 0)
            {
                dataParams.Add(new DataParameter(m_cs, "IDT", DbType.Int32), m_IdT);
                // EG 20121227 Use for VW_EAREVENTENUM (to get EAR_DAY and EAR_COMMON values)
                dataParams.Add(new DataParameter(m_cs, "GPRODUCT", DbType.String), m_GProduct);
                sqlWhere.Append(@"(ev.IDT = @IDT)");
                if (DataHelper.IsDbOracle(m_cs))
                    sqlWhereGProduct = SQLCst.AND + "(@GPRODUCT = @GPRODUCT)" + Cst.CrLf;
            }


            StrBuilder SQLSelect = new StrBuilder();
            SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENT, Cst.ProcessTypeEnum.EARGEN) + sqlWhere.ToString() + Cst.CrLf + SQLCst.ORDERBY + "ev.IDE";
            SQLSelect += SQLCst.SEPARATOR_MULTISELECT;
            SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTCLASS, Cst.ProcessTypeEnum.EARGEN) + sqlWhere.ToString() + sqlWhereGProduct;
            SQLSelect += SQLCst.SEPARATOR_MULTISELECT;
            SQLSelect += GetSelectEventClassWithEarDayColumn() + sqlWhere.ToString() + sqlWhereGProduct;
            SQLSelect += SQLCst.SEPARATOR_MULTISELECT;

            if (isWithStCheck)
                SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTSTCHECK) + sqlWhere.ToString() + sqlWhereGProduct + SQLCst.SEPARATOR_MULTISELECT;
            if (isWithStMatch)
                SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTSTMATCH) + sqlWhere.ToString() + sqlWhereGProduct + SQLCst.SEPARATOR_MULTISELECT;

            string sqlSelectEvent = SQLSelect.ToString();
            m_dsEvents = DataHelper.ExecuteDataset(m_cs, CommandType.Text, sqlSelectEvent, dataParams.GetArrayDbParameter());

            m_dsEvents.DataSetName = "Events";
            m_dsEvents.Tables[0].TableName = "Event";
            m_dsEvents.Tables[1].TableName = "EventClass";
            m_dsEvents.Tables[2].TableName = "EventClassWithEarDay";
            if (isWithStCheck)
                m_dsEvents.Tables[3].TableName = "EventStCheck";
            if (isWithStMatch)
                m_dsEvents.Tables[4].TableName = "EventStMatch";

            if (null != DtEvent)
            {
                DataRelation relChildEvent = new DataRelation("ChildEvent", DtEvent.Columns["IDE"], DtEvent.Columns["IDE_EVENT"], false);
                m_dsEvents.Relations.Add(relChildEvent);
            }
            if ((null != DtEvent) && (null != DtEventClass))
            {
                DataRelation relEventClass = new DataRelation("Event_EventClass", DtEvent.Columns["IDE"], DtEventClass.Columns["IDE"], false);
                m_dsEvents.Relations.Add(relEventClass);
            }
            if ((null != DtEvent) && (null != DtEventStCheck))
            {
                DataRelation relEventStCheck = new DataRelation("Event_EventStCheck", DtEvent.Columns["IDE"], DtEventStCheck.Columns["IDE"], false);
                m_dsEvents.Relations.Add(relEventStCheck);
            }
            if ((null != DtEvent) && (null != DtEventStMatch))
            {
                DataRelation relEventStMatch = new DataRelation("Event_EventStMatch", DtEvent.Columns["IDE"], DtEventStMatch.Columns["IDE"], false);
                m_dsEvents.Relations.Add(relEventStMatch);
            }
        }

        private string GetSelectEventClassWithEarDayColumn()
        {
            return @"select distinct ec.IDEC, ec.IDE, ec.EVENTCLASS, 
            ear.IDEAR, ear.IDB, ear.IDSTACTIVATION, ear.IDB, ear.DTEAR, 
            bk.IDENTIFIER as BOOK_IDENTIFIER
            from dbo.EVENT ev
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
            inner join dbo.EVENTENUM en on (en.CODE = 'EventClass') and (en.VALUE = ec.EVENTCLASS)
            inner join dbo.EARDAY ed on (ed.IDEC = ec.IDEC)
            inner join dbo.EAR ear on (ear.IDEAR = ed.IDEAR)
            inner join dbo.BOOK bk on (bk.IDB = ear.IDB)" + Cst.CrLf;
        }

        /// <summary>
        /// Obtient DataTable nommé EventClassWithEarDay
        /// </summary>
        public DataTable DtEventClassWithEarDay
        {
            get { return DsEvent.Tables["EventClassWithEarDay"]; }
        }

        /// <summary>
        /// Obtient DataTable nommé EventToEarDay
        /// </summary>
        public DataTable DtEventToEarDay
        {
            get { return DsEvent.Tables["EventToEarDay"]; }
        }

        /// <summary>
        /// Obtient DataTable nommé EventToEarDay_EventClassToEarDay
        /// </summary>
        public DataRelation ChildEventClassToEarDay
        {
            get { return DsEvent.Relations["EventToEarDay_EventClassToEarDay"]; }
        }
    }
    public class EarQuery
    {
        public IDbDataParameter IdEAR;
        public IDbDataParameter IdT;
        public IDbDataParameter IdB;
        public IDbDataParameter DtEar;
        public IDbDataParameter DtEvent;
        public IDbDataParameter DtRemoved;
        public IDbDataParameter DtAccount;
        public IDbDataParameter IdAIns;
        public IDbDataParameter Source;
        public IDbDataParameter ExtLink;
        public IDbDataParameter IdStActivation;

        public IDbDataParameter InstrumentNo;
        public IDbDataParameter StreamNo;

        public IDbDataParameter IdEARDAY;
        public IDbDataParameter IdE;
        public IDbDataParameter IdEC;
        public IDbDataParameter EventCode;
        public IDbDataParameter EventType;
        public IDbDataParameter AmountType;
        public IDbDataParameter EventClass;

        public IDbDataParameter IdEARCOMMON;
        public IDbDataParameter EarCode;

        public IDbDataParameter IdEARNOM;
        public IDbDataParameter Amount;

        public IDbDataParameter ExchangeType;
        public IDbDataParameter IdC;
        public IDbDataParameter Paid;
        public IDbDataParameter Received;
        public IDbDataParameter IdQuote_H;
        public IDbDataParameter IdQuote_H2;
        public IDbDataParameter IdStProcess;

        public IDbDataParameter IdEARCALC;
        public IDbDataParameter CalcType;
        public IDbDataParameter SequenceNo;

        public IDbDataParameter ConstEventCode;
        public IDbDataParameter ConstEventType;
        public IDbDataParameter ConstEventClass;

        public IDbDataParameter ConstRemoved;

        public IDbDataParameter ConstALL;
        public IDbDataParameter ConstOTC;
        public IDbDataParameter ConstETD;
        public IDbDataParameter ConstTrue;

        public IDbDataParameter GProduct;

        // Ear Insert Query
        public string m_SQLQueryEar = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EAR.ToString() + @"
(IDEAR, IDT, IDB, DTEAR, DTEVENT, DTREMOVED, DTSYS, DTINS, IDAINS, SOURCE, IDSTACTIVATION, EXTLLINK)
values 
(@IDEAR, @IDT, @IDB, @DTEAR, @DTEVENT, @DTREMOVED, @DTSYS, @DTINS, @IDAINS, @SOURCE, @IDSTACTIVATION, @EXTLLINK)";
        // EarCalcTemp Insert Query
        public string m_SQLQueryTempEarCalc = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TMPEARCALCAMOUNT.ToString() + @"
(IDEAR, DTACCOUNT, INSTRUMENTNO, STREAMNO, EARCODE, CALCTYPE, EVENTCLASS,EXCHANGETYPE,IDC, PAID, RECEIVED, IDSTPROCESS)
values
(@IDEAR, @DTACCOUNT, @INSTRUMENTNO, @STREAMNO, @EARCODE, @CALCTYPE, @EVENTCLASS,@EXCHANGETYPE,@IDC, @PAID, @RECEIVED, @IDSTPROCESS)";
        // EarCalc Insert Query
        public string m_SQLQueryEarCalc = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARCALC.ToString() + @"
(IDEARCALC, IDEAR, INSTRUMENTNO, STREAMNO, EARCODE, CALCTYPE, EVENTCLASS, DTACCOUNT) 
values 
(@IDEARCALC, @IDEAR, @INSTRUMENTNO, @STREAMNO, @EARCODE, @CALCTYPE, @EVENTCLASS, @DTACCOUNT)";
        // EarCalcEvent Insert Query
        public string m_SQLQueryEarCalcEvent = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARCALC_EVENT.ToString() + @"
(IDEARCALC, IDE, DTINS, IDAINS) 
values 
(@IDEARCALC, @IDE, @DTINS, @IDAINS)";
        // EarCalcAmount Insert Query
        public string m_SQLQueryEarCalcAmount = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARCALCAMOUNT.ToString() + @"
(IDEARCALC, EXCHANGETYPE, IDC, PAID, RECEIVED, IDQUOTE_H, IDQUOTE_H2, IDSTPROCESS)
values 
(@IDEARCALC, @EXCHANGETYPE, @IDC, @PAID, @RECEIVED, @IDQUOTE_H, @IDQUOTE_H2, @IDSTPROCESS)";
        // EarDet Insert Query
        public string m_SQLQueryEarDet = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARDET.ToString() + @"
(IDEAR, INSTRUMENTNO, STREAMNO) 
values 
(@IDEAR, @INSTRUMENTNO, @STREAMNO)";
        // EarDay Insert Query
        public string m_SQLQueryEarDay = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARDAY.ToString() + @"
(IDEARDAY, IDEAR, INSTRUMENTNO, STREAMNO, IDEC,EARCODE, EVENTCODE, EVENTTYPE, EVENTCLASS)
values 
(@IDEARDAY, @IDEAR, @INSTRUMENTNO, @STREAMNO, @IDEC, @EARCODE, @EVENTCODE, @EVENTTYPE, @EVENTCLASS)";
        // EarDayAmount Insert Query
        public string m_SQLQueryEarDayAmount = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARDAYAMOUNT.ToString() + @"
(IDEARDAY, EXCHANGETYPE, IDC, PAID, RECEIVED, IDQUOTE_H, IDQUOTE_H2, IDSTPROCESS)
values 
(@IDEARDAY, @EXCHANGETYPE, @IDC, @PAID, @RECEIVED, @IDQUOTE_H, @IDQUOTE_H2, @IDSTPROCESS)";
        // EarCommon Insert Query
        public string m_SQLQueryEarCommon = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARCOMMON.ToString() + @"
(IDEARCOMMON, IDEAR, INSTRUMENTNO, STREAMNO, IDEARDAY,EARCODE, EVENTCODE, EVENTTYPE, EVENTCLASS)
values
(@IDEARCOMMON, @IDEAR, @INSTRUMENTNO, @STREAMNO, @IDEARDAY,@EARCODE, @EVENTCODE, @EVENTTYPE, @EVENTCLASS)";
        // EarCommonAmount Insert Query
        public string m_SQLQueryEarCommonAmount = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARCOMMONAMOUNT.ToString() + @"
(IDEARCOMMON, EXCHANGETYPE, IDC, PAID, RECEIVED, IDQUOTE_H, IDQUOTE_H2, IDSTPROCESS) 
values 
(@IDEARCOMMON, @EXCHANGETYPE, @IDC, @PAID, @RECEIVED, @IDQUOTE_H, @IDQUOTE_H2, @IDSTPROCESS)";
        // EarNom Insert Query
        public string m_SQLQueryEarNom = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARNOM.ToString() + @"
(IDEARNOM, IDEAR, INSTRUMENTNO, STREAMNO, EVENTCODE, EVENTTYPE, EVENTCLASS,DTACCOUNT)
values
(@IDEARNOM, @IDEAR, @INSTRUMENTNO, @STREAMNO, @EVENTCODE, @EVENTTYPE, @EVENTCLASS, @DTACCOUNT)";
        // EarNomEvent Insert Query
        public string m_SQLQueryEarNomEvent = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARNOM_EVENT.ToString() + @"
(IDEARNOM, IDE, DTINS, IDAINS)
values 
(@IDEARNOM, @IDE, @DTINS, @IDAINS)";
        // EarNomAmount Insert Query
        public string m_SQLQueryEarNomAmount = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EARNOMAMOUNT.ToString() + @"
(IDEARNOM, EXCHANGETYPE, IDC, AMOUNT, IDQUOTE_H, IDQUOTE_H2, IDSTPROCESS)
values 
(@IDEARNOM, @EXCHANGETYPE, @IDC, @AMOUNT, @IDQUOTE_H, @IDQUOTE_H2, @IDSTPROCESS)";
    }

    public class EarGenProcess : ProcessTradeBase
    {
        #region Members
        private readonly EarGenMQueue earGenMQueue;
        readonly List<Pair<string, string>> m_LstMasterData = new List<Pair<string, string>>();
        private readonly ProcessBase m_Process;

        readonly private DateTime m_EarDate;
        readonly private string m_EarFlow;

        private EarTradeInfo m_TradeInfo;
        private EarEventInfo m_EventsInfo;

        private DataTable m_DtEarCommon;
        private DataTable m_DtEarCLO;
        private DataTable m_DtEarLPC;

        private bool m_IsCLOEarDay;
        /// <summary>
        /// Si Gestion native de l'annulation comptable d'un événement
        /// <para>Si True un EAR annulé sera généré, et les écritures seront inversées (sens Débit/Crédit)</para>
        /// <para>Sinon un EARDAY xxx/yyy/RMV sera généré, et il faudrait disposer de schémas comptables pour générer les écritures d'annulation</para>
        /// </summary>
        private bool m_IsNativeAccRemove;

        private Hashtable m_CacheBook;
        private List<Pair<string, object>> m_CacheQuote;
        private Hashtable m_CacheCurrency;

        private List<Pre_EarBook> m_EarBooks;
        private Pre_EarAmounts m_Pre_EarAmounts;
        private DateTime m_TransactDate;
        private EarQuery m_EarQuery;

        private List<AccVariable> m_AccVariables;

        #endregion Members
        #region Accessors
        #region DtPRSDate
        public DateTime DtPRSDate
        {
            get
            {
                if (RowPRSEventClass != null)
                    return Convert.ToDateTime(RowPRSEventClass["DTEVENT"]);
                else
                    return DateTime.MinValue;
            }
        }
        #endregion DtPRSDate
        #region RowPRSEventClass
        public DataRow RowPRSEventClass
        {
            get
            {
                if (RowEvent != null)
                {
                    DataRow[] rowEventClass = RowEvent.GetChildRows(m_EventsInfo.ChildEventClass);
                    foreach (DataRow item in rowEventClass)
                    {
                        if (item["EVENTCLASS"].ToString() == "PRS")
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion RowPRSEventClass
        #region RowEventClass
        public DataRow RowEventClass
        {
            get
            {
                DataRow[] rows = m_EventsInfo.DtEventClass.Select("IDEC=" + m_EarQuery.IdEC.Value.ToString());
                if ((null != rows) && (0 < rows.Length))
                    return rows[0];
                else
                    return null;
            }
        }
        #endregion RowEventClass
        #region RowEvent
        public DataRow RowEvent
        {
            get
            {
                DataRow[] rows = m_EventsInfo.DtEvent.Select("IDE=" + m_EarQuery.IdE.Value.ToString());
                if ((null != rows) && (0 < rows.Length))
                    return rows[0];
                else
                    return null;
            }
        }
        #endregion RowEvent

        #endregion Accessors
        #region Constructor
        // EG 20180423 Analyse du code Correction [CA2200]
        public EarGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
#if DEBUG
            diagnosticDebug.Write("Ear generation for trade: " + pMQueue.identifier + " (" + DtFunc.DateTimeToStringISO(DateTime.Now) + ") *****************");
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            try
            {
                earGenMQueue = (EarGenMQueue)pMQueue;
                m_Process = this;

                if (false == m_Process.IsProcessObserver)
                {
                    //PL 20091224 PL Bug Lorsque Message généré par le STP ProcessTuning car ce paramètre n'existe pas
                    //m_EarFlow = earGenMQueue.GetStringValueParameterById(EarGenMQueue.PARAM_CLASS).ToString();
                    m_EarFlow = earGenMQueue.GetStringValueParameterById(EarGenMQueue.PARAM_CLASS);
                    bool isEarFlowSpecified = StrFunc.IsFilled(m_EarFlow);
                    if (!isEarFlowSpecified)
                        m_EarFlow = Cst.FlowTypeEnum.ALL.ToString();

                    // EG 20121003
                    m_LstMasterData = new List<Pair<string, string>>();
                    Pair<string, string> trade = new Pair<string, string>("TRADE", "N/A");
                    if (earGenMQueue.idSpecified && earGenMQueue.identifierSpecified)
                        trade.Second = LogTools.IdentifierAndId(earGenMQueue.identifier, earGenMQueue.id);
                    else if (earGenMQueue.identifierSpecified)
                        trade.Second = LogTools.IdentifierAndId(earGenMQueue.identifierSpecified ? earGenMQueue.identifier : "N/A");
                    m_LstMasterData.Add(trade);

                    if (earGenMQueue.IsMasterDateSpecified)
                    {
                        m_EarDate = earGenMQueue.EarDate;
                        m_LstMasterData.Add(new Pair<string, string>("PROCESSDATE", DtFunc.DateTimeToStringDateISO(m_EarDate)));
                    }
                    else
                    {
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-05342",
                            new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND),
                            m_LstMasterData.Find(match => match.First == "TRADE").Second);
                    }
                    m_LstMasterData.Add(new Pair<string, string>("FLOWSCLASS", m_EarFlow + (!isEarFlowSpecified ? "[default]" : string.Empty)));
                }
            }
            catch (SpheresException2) { throw; }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-00010", ex);
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }
        #endregion Constructor
        #region Methods

        /// <summary>
        /// Annulation des EARs du trade
        /// </summary>
        /// <returns></returns>
        // EG 20180606 [23979] IRQ (EARGEN)
        private Cst.ErrLevel EarCancellation()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.DATANOTFOUND;
            DateTime dtEventRemove = DateTime.MinValue;

            if (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn))
            {
                if (m_EventsInfo.IsTradeRemoved(m_EarDate, ref dtEventRemove))
                {
                    #region Process Cancellation
                    #region SelectEar query
                    StrBuilder sqlSelect = new StrBuilder();
                    sqlSelect += SQLCst.SELECT + "e.IDEAR, e.IDSTACTIVATION, e.DTEVENTCANCEL, e.DTEARCANCEL, e.DTSYSCANCEL" + Cst.CrLf;
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EAR + " e" + Cst.CrLf;
                    string sqlWhere = SQLCst.WHERE + "(e.IDT = @IDT)" + Cst.CrLf;
                    #endregion SelectEar query
                    DataSet dsEar = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlSelect.ToString() + sqlWhere, m_EarQuery.IdT);
                    DataTable dtEar = dsEar.Tables[0];

                    if (null != dtEar.Rows)
                    {
                        string dtMinString = DtFunc.DateTimeToStringDateISO(DateTime.MinValue);
                        string dtEarcancelIsNull = "ISNULL(DTEARCANCEL,#" + dtMinString + "#)=#" + dtMinString + "#";
                        //
                        DataRow[] rowsEarCancelled = dtEar.Select("NOT(" + dtEarcancelIsNull + ")");
                        DataRow[] rowsEarNotCancelled = dtEar.Select(dtEarcancelIsNull);
                        int nbEarCancelled = (rowsEarCancelled == null ? 0 : rowsEarCancelled.Length);

                        if (dtEar.Rows.Count == 0)
                        {
                            // Log Warning
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "LOG-05335",
                                new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND));
                        }
                        else
                        {
                            if (nbEarCancelled == dtEar.Rows.Count)
                            {
                                // Log Warning
                                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "LOG-05336",
                                    new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND));
                            }
                            else
                            {
                                if (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn))
                                {

                                    foreach (DataRow rowEarToCancel in rowsEarNotCancelled)
                                    {
                                        if (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn))
                                            break;
                                        rowEarToCancel.BeginEdit();
                                        rowEarToCancel["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV;
                                        rowEarToCancel["DTEVENTCANCEL"] = dtEventRemove;
                                        rowEarToCancel["DTEARCANCEL"] = m_EarDate;
                                        // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                                        //rowEarToCancel["DTSYSCANCEL"] = processLog.GetDate();
                                        rowEarToCancel["DTSYSCANCEL"] = OTCmlHelper.GetDateSysUTC(Cs);
                                        rowEarToCancel.EndEdit();
                                    }


                                    if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                                    {
                                        _ = DataHelper.ExecuteDataAdapter(Cs, sqlSelect.ToString(), dtEar);
                                        codeReturn = Cst.ErrLevel.SUCCESS;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    // Log Warning
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "LOG-05337",
                        new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND));
                }
            }
            return codeReturn;
        }

        /// <summary>
        /// Calculer les EARs du jour (EARDAY)
        /// </summary>
        /// <returns></returns>
        // EG 20180606 [23979] IRQ (EARGEN)
        private Cst.ErrLevel CalcEarDay()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " ...............................");
#endif
            Cst.ErrLevel codeReturn = Cst.ErrLevel.DATANOTFOUND;


            m_EarBooks = new List<Pre_EarBook>();
            m_CacheQuote = new List<Pair<string, object>>();
            m_CacheCurrency = new Hashtable();

            // EarDay calculation
            // RD 20121026 [18201] Gestion des événements annulés
            bool isCompatibility_Event_Found = false;

            foreach (DataRow rowEvent in m_EventsInfo.DtEventToEarDay.Rows)
            {
                if (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn))
                    break;

                int ide_Current = Convert.ToInt32(rowEvent["IDE"]);
                DateTime dtRemoved = DateTime.MinValue;
                bool isEventRemoved = m_IsNativeAccRemove && CheckEventRemoved(ide_Current, out dtRemoved);
                Cst.StatusActivation idStActivation = (isEventRemoved ? Cst.StatusActivation.REMOVED : Cst.StatusActivation.REGULAR);

                bool isCompatibility_Event = (Cst.ErrLevel.SUCCESS == ScanCompatibility_Event(ide_Current));
                if (isCompatibility_Event)
                {
                    isCompatibility_Event_Found = true;

                    bool isEarPayer = (false == Convert.IsDBNull(rowEvent["IDB_PAY"]) && m_CacheBook.ContainsKey(rowEvent["IDB_PAY"].ToString()));
                    bool isEarReceiver = (false == Convert.IsDBNull(rowEvent["IDB_REC"]) && m_CacheBook.ContainsKey(rowEvent["IDB_REC"].ToString()));

                    IEnumerable<DataRow> rowsEventClass = rowEvent.GetChildRows(m_EventsInfo.ChildEventClassToEarDay);

                    if (isEarPayer)
                        AddEarDay(rowEvent, rowsEventClass, true, idStActivation, dtRemoved);
                    if (isEarReceiver)
                        AddEarDay(rowEvent, rowsEventClass, false, idStActivation, dtRemoved);
                }

                codeReturn = Cst.ErrLevel.SUCCESS;
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                if (isCompatibility_Event_Found == false)
                    codeReturn = Cst.ErrLevel.TUNING_IGNORE;
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "..........................................");
#endif
            return codeReturn;
        }

        /// <summary>
        /// Calculer les EARs du jour (Paid/Received)
        /// </summary>
        /// <param name="pRow">Repésente un évènement</param>
        /// <param name="pRowsEventClass">Représente les EVENTCLASS de l'évènement</param>
        /// <param name="pIsPaid">true si l'évènement est payé</param>
        /// <param name="pIdStActivation"></param>
        /// <param name="pDtRemoved">Date d'annulation si évènement annulé</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddEarDay(DataRow pRow, IEnumerable<DataRow> pRowsEventClass, bool pIsPaid,
            Cst.StatusActivation pIdStActivation, DateTime pDtRemoved)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            // RD 20121026 [18201] Gestion des événements annulés
            Pre_EarBook partyBook = (Pre_EarBook)m_CacheBook[pRow["IDB_" + (pIsPaid ? "PAY" : "REC")].ToString()];

            decimal amount = Convert.IsDBNull(pRow["VALORISATION"]) ? 0 : Convert.ToDecimal(pRow["VALORISATION"]);
            int idE = Convert.ToInt32(pRow["IDE"]);
            int instrumentNo = Convert.IsDBNull(pRow["INSTRUMENTNO"]) ? 0 : Convert.ToInt32(pRow["INSTRUMENTNO"]);
            int streamNo = Convert.IsDBNull(pRow["STREAMNO"]) ? 0 : Convert.ToInt32(pRow["STREAMNO"]);
            string eventCode = pRow["EVENTCODE"].ToString();
            string eventType = pRow["EVENTTYPE"].ToString();
            string idC = pRow["UNIT"].ToString();

            List<Pair<string, string>> lstData = new List<Pair<string, string>>
            {
                new Pair<string, string>("PAYREC", (pIsPaid ? "PAID" : "RECEIVED")),
                new Pair<string, string>("EVENT", LogTools.IdentifierAndId(eventCode + " / " + eventType, idE)),
                new Pair<string, string>("INSTRSTREAMNO", instrumentNo.ToString() + " / " + streamNo.ToString()),
                new Pair<string, string>("BOOK", LogTools.IdentifierAndId(partyBook.IdB_Identifier, partyBook.IdB))
            };

            // GetEarCode
            m_EarQuery.IdE.Value = idE;
            DataRow rowEvent = RowEvent;

            string earCode = eventCode;
            if (null != rowEvent)
                earCode = GetEarCode(rowEvent, lstData);

            lstData.Add(new Pair<string, string>("EARCODE", earCode));

            // EarDet setting
            Pre_EarDet earDet = new Pre_EarDet(instrumentNo, streamNo);

            // EarAmounts settings
            DateTime dtPRSDate = DateTime.MinValue;
            DateTime dtValue = DateTime.MinValue;

            if ((m_TradeInfo.Product.IsLSD == false) && (m_TradeInfo.Product.IsRiskProduct == false))
                dtPRSDate = DtPRSDate;

            if (eventCode == "OPP")
                dtValue = Convert.ToDateTime(pRow["DTSTARTADJ"]);

            // Set currencies
            SetCounterValueData(earDet, idC, partyBook, dtPRSDate, dtValue);

            // Process all counter values except EventDate one	
            NewEarAmounts(pIsPaid, amount);

            // EarDay and EarDayAmount settings


            foreach (DataRow rowEventClass in pRowsEventClass)
            {
                int idEC = Convert.ToInt32(rowEventClass["IDEC"]);
                DateTime dtEvent = Convert.ToDateTime(rowEventClass["DTEVENT"]);
                string eventClass = rowEventClass["EVENTCLASS"].ToString();

                lstData.Add(new Pair<string, string>("EVENTCLASS", LogTools.IdentifierAndId(eventClass, idEC)));
                lstData.Add(new Pair<string, string>("DTEVENT", DtFunc.DateTimeToStringDateISO(dtEvent)));

                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5361), 2,
                        new LogParam(lstData.Find(match => match.First == "EVENTCLASS").Second),
                        new LogParam(lstData.Find(match => match.First == "DTEVENT").Second),
                        new LogParam(lstData.Find(match => match.First == "PAYREC").Second),
                        new LogParam(lstData.Find(match => match.First == "EARCODE").Second),
                        new LogParam(lstData.Find(match => match.First == "EVENT").Second),
                        new LogParam(lstData.Find(match => match.First == "INSTRSTREAMNO").Second),
                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                        new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second),
                        new LogParam(lstData.Find(match => match.First == "BOOK").Second)));

                // EarDay setting
                Pre_EarBook earBook = m_EarBooks.Find(book => ((book.IdB == partyBook.IdB) &&
                    (book.IdStActivation == pIdStActivation) &&
                    (book.DtEvent == dtEvent) &&
                    (book.DtRemoved == pDtRemoved)));

                if (earBook == null)
                {
                    earBook = new Pre_EarBook(partyBook, pIdStActivation, dtEvent, pDtRemoved);
                    m_EarBooks.Add(earBook);
                }

                earDet = earBook.EarDets.Find(det => (det.instrumentNo == instrumentNo) && (det.streamNo == streamNo));
                if (earDet == null)
                {
                    earDet = new Pre_EarDet(instrumentNo, streamNo);
                    earBook.EarDets.Add(earDet);
                }

                Pre_EarDay earDay = earDet[earCode, eventCode, eventType, eventClass, idEC];
                if (earDay == null)
                {
                    earDay = new Pre_EarDay(earCode, eventCode, eventType, eventClass, idEC, idE, dtEvent);
                    earDet.EarDays.Add(earDay);
                }

                // Add all amounts to EarDay
                // Process EventDate counter value for all currencies
                AddEventDateAmount(dtEvent);
                // Add all counter values to EarDay
                AddAmountsToEar(earDay);

                #region Add EarDay with EventClass LDR for each LDP EventCalss
                string eventClassLDP = EventClassFunc.LinearDepreciation;
                string eventClassLDR = EventClassFunc.LinearDepRemaining;
                //
                if (eventClass == eventClassLDP || eventClass == eventClassLDR)
                {
                    try
                    {
                        string eventClassNew = (eventClass == eventClassLDP ? eventClassLDR : eventClassLDP);
                        Pre_EarDay earDayNew = earDet[earCode, eventCode, eventType, eventClassNew, idEC];
                        if (earDayNew == null)
                        {
                            earDayNew = new Pre_EarDay(earCode, eventCode, eventType, eventClassNew, idEC, idE, dtEvent);
                            earDet.EarDays.Add(earDayNew);
                        }

                        DataRow rowEventParent = rowEvent.GetParentRow(m_EventsInfo.ChildEvent);

                        if ((null != rowEventParent))
                        {
                            decimal valoParent = (Convert.IsDBNull(rowEventParent["VALORISATION"]) ? 0 : Convert.ToDecimal(rowEventParent["VALORISATION"]));
                            decimal amountNew = (amount > 0 ? ((valoParent - amount) > 0 ? (valoParent - amount) : 0) : 0);
                            //
                            Pre_EarAmounts earAmountsNew = m_Pre_EarAmounts.Clone();

                            // Add all amounts to EarDay
                            // Process all counter values except EventDate one	
                            NewEarAmounts(pIsPaid, amountNew, ref earAmountsNew);
                            // Process EventDate counter value for all currencies
                            AddEventDateAmount(dtEvent, ref earAmountsNew);
                            // Add all counter values to EarDay
                            AddAmountsToEar(earDayNew, earAmountsNew);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, "SYS-05362",
                            new ProcessState(ProcessStateTools.StatusEnum.ERROR, ProcessStateTools.CodeReturnFailureEnum), ex,
                                lstData.Find(match => match.First == "EVENTCLASS").Second,
                                lstData.Find(match => match.First == "DTEVENT").Second,
                                lstData.Find(match => match.First == "PAYREC").Second,
                                lstData.Find(match => match.First == "EARCODE").Second,
                                lstData.Find(match => match.First == "EVENT").Second,
                                lstData.Find(match => match.First == "INSTRSTREAMNO").Second,
                                m_LstMasterData.Find(match => match.First == "TRADE").Second,
                                m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                                m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second,
                                lstData.Find(match => match.First == "BOOK").Second);
                    }
                }
                #endregion
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// Calculer les EARs représentant le nominal (EARNOM) selon l'instrument 
        /// </summary>
        /// <returns></returns>
        // EG 20180606 [23979] IRQ (EARGEN)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel CalcEarNOM()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

            List<Pair<string, string>> lstData = new List<Pair<string, string>>();
            foreach (Pre_EarBook earBook in m_EarBooks)
            {
                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || 
                    (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                    break;

                foreach (Pre_EarDet earDet in earBook.EarDets)
                {
                    if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || 
                        (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                        break;

                    m_EarQuery.InstrumentNo.Value = earDet.instrumentNo;
                    m_EarQuery.StreamNo.Value = earDet.streamNo;

                    foreach (Pre_EarDay earDay in earDet.EarDays)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || 
                            (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                            break;

                        lstData.Clear();
                        lstData.Add(new Pair<string, string>("EVENTCLASS", LogTools.IdentifierAndId(earDay.eventClass, earDay.idEC)));
                        lstData.Add(new Pair<string, string>("EVENT", LogTools.IdentifierAndId(earDay.eventCode + " / " + earDay.eventType, earDay.idE)));
                        lstData.Add(new Pair<string, string>("DTEVENT", DtFunc.DateTimeToStringDateISO(earDay.dtEvent)));
                        lstData.Add(new Pair<string, string>("INSTRSTREAMNO", earDet.instrumentNo.ToString() + " / " + earDet.streamNo.ToString()));
                        lstData.Add(new Pair<string, string>("BOOK", LogTools.IdentifierAndId(earBook.IdB_Identifier, earBook.IdB)));

                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5363), 2,
                                new LogParam(lstData.Find(match => match.First == "EVENTCLASS").Second),
                                new LogParam(lstData.Find(match => match.First == "DTEVENT").Second),
                                new LogParam(lstData.Find(match => match.First == "EVENT").Second),
                                new LogParam(lstData.Find(match => match.First == "INSTRSTREAMNO").Second),
                                new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second),
                                new LogParam(lstData.Find(match => match.First == "BOOK").Second)));
                        
                        m_EarQuery.IdE.Value = earDay.idE;
                        m_EarQuery.IdEC.Value = earDay.idEC;

                        DataRow rowEvent = RowEvent;

                        if (null != rowEvent)
                        {
                            try
                            {
                                DateTime dtAccount = DateTime.MinValue;
                                DataRow rowEventClass = RowEventClass;
                                if (null != rowEventClass)
                                    dtAccount = Convert.ToDateTime(rowEventClass["DTEVENT"]);

                                DataRow[] rowsNominal = null;
                                decimal amount = 0;
                                string idC = string.Empty;
                                string eventCode = EventCodeFunc.NominalStep;
                                string eventType = string.Empty;
                                string eventClass = EventClassFunc.Date;
                                int[] earNomEvent = null;

                                if (m_TradeInfo.Product.IsIRD)
                                {
                                    DateTime dtStartPeriod = Convert.ToDateTime(rowEvent["DTSTARTADJ"]);
                                    rowsNominal = GetIRDCurrentNominal(dtStartPeriod);
                                }
                                else if (m_TradeInfo.Product.IsFx)
                                    rowsNominal = GetFxCurrentNominal();
                                // EG 20160404 Migration vs2013
                                // #warning EARNOM uniquement sur Fx et IRD, il faut gérer les autres familles
                                //
                                if (ArrFunc.IsFilled(rowsNominal))
                                {
                                    foreach (DataRow rowNominal in rowsNominal)
                                    {

                                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || 
                                            (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                            break;

                                        eventType = rowNominal["EVENTTYPE"].ToString();
                                        amount = GetNominalAmount(rowNominal, dtAccount, ref earNomEvent);
                                        idC = rowNominal["UNIT"].ToString();

                                        #region EarNom and EarNomAmount settings
                                        #region EarNom settings
                                        Pre_EarNom earNom = earDet[eventCode, eventType, dtAccount];
                                        if (earNom == null)
                                        {
                                            earNom = new Pre_EarNom(eventCode, eventType, eventClass, dtAccount, earNomEvent);
                                            earDet.EarNoms.Add(earNom);
                                        }
                                        #endregion EarNom settings
                                        #region EarNomAmount settings
                                        // Set currencies
                                        SetCounterValueData(earDet, idC, earBook);
                                        // Process all counter values												
                                        NewEarAmounts(amount, dtAccount);
                                        // Add all counter values to EarNom																								
                                        AddAmountsToEar(earNom);
                                        #endregion EarNomAmount settings
                                        #endregion EarNom and EarNomAmount settings
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, "SYS-05363",
                                    new ProcessState(ProcessStateTools.StatusEnum.ERROR, ProcessStateTools.CodeReturnFailureEnum), ex,
                                        lstData.Find(match => match.First == "EVENTCLASS").Second,
                                        lstData.Find(match => match.First == "DTEVENT").Second,
                                        lstData.Find(match => match.First == "EVENT").Second,
                                        lstData.Find(match => match.First == "INSTRSTREAMNO").Second,
                                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                                        m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second,
                                        lstData.Find(match => match.First == "BOOK").Second);
                            }
                        }
                    }
                }
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return codeReturn;
        }

        /// <summary>
        /// Ajoute des EARCOMMON à partir des EARDAY qui sont flagués comme donnant lieu à des EARCOMMON, via la table EVENTENUM
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <returns></returns>
        // EG 20121227 Gestion Nouvelle colonne EARCOMMON (Puissance de 2)
        // EG 20180606 [23979] IRQ (EARGEN)
        protected Cst.ErrLevel CalcEarCommon(DataDbTransaction pEfsTransaction)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            List<Pair<string, string>> lstData = new List<Pair<string, string>>();
            // RD 20121024 [18201] Calculer les EARCOMMON uniquement sur les EAR non annulés
            foreach (Pre_EarBook earBook in m_EarBooks.FindAll(ear => ear.IdStActivation == Cst.StatusActivation.REGULAR))
            {
                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ||
                    (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                    break;

                // Add previous EarDay to EarCommon
                lstData.Add(new Pair<string, string>("EAR", LogTools.IdentifierAndId(DtFunc.DateTimeToStringDateISO(m_EarDate), earBook.IdEAR)));
                lstData.Add(new Pair<string, string>("DTEVENT", DtFunc.DateTimeToStringDateISO(earBook.DtEvent)));
                lstData.Add(new Pair<string, string>("IDSTACTIVATION", earBook.IdStActivation.ToString()));
                lstData.Add(new Pair<string, string>("BOOK", LogTools.IdentifierAndId(earBook.IdB_Identifier, earBook.IdB)));

                if (Cst.ErrLevel.SUCCESS == GetPreviousEarToEarCommon(pEfsTransaction, earBook.IdB, earBook.DtEvent, lstData))
                {
                    if (null != m_DtEarCommon)
                    {
                        decimal paid = 0;
                        decimal received = 0;
                        int instrumentNo = 0;
                        int streamNo = 0;
                        string earCode = string.Empty;
                        string eventCode = string.Empty;
                        string eventType = string.Empty;
                        string eventClass = string.Empty;
                        string idC = string.Empty;
                        int idEARDAY = 0;
                        int idPreviousEAR = 0;

                        foreach (DataRow row in m_DtEarCommon.Rows)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ||
                                (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                break;

                            idPreviousEAR = Convert.ToInt32(row["IDEAR"]);
                            //
                            if (idPreviousEAR != earBook.IdEAR)
                            {
                                Pre_EarDet earDet = null;
                                Pre_EarCommon earCommon = null;
                                //
                                paid = Convert.ToDecimal(row["PAID"]);
                                received = Convert.ToDecimal(row["RECEIVED"]);
                                instrumentNo = Convert.ToInt32(row["INSTRUMENTNO"]);
                                streamNo = Convert.ToInt32(row["STREAMNO"]);
                                eventCode = row["EVENTCODE"].ToString();
                                earCode = row["EARCODE"].ToString();
                                eventType = row["EVENTTYPE"].ToString();
                                eventClass = row["EVENTCLASS"].ToString();
                                idC = row["IDC"].ToString();
                                m_EarQuery.IdE.Value = Convert.ToInt32(row["IDE"]);

                                earDet = new Pre_EarDet(instrumentNo, streamNo);

                                // EarDet setting
                                earDet = earBook.EarDets.Find(det => (det.instrumentNo == instrumentNo) && (det.streamNo == streamNo));
                                if (earDet == null)
                                {
                                    earDet = new Pre_EarDet(instrumentNo, streamNo);
                                    earBook.EarDets.Add(earDet);
                                }

                                // EarCommon setting
                                idEARDAY = Convert.ToInt32(row["IDEARDAY"]);
                                earCommon = earDet[idEARDAY, earCode, eventCode, eventType, eventClass];
                                if (earCommon == null)
                                {
                                    earCommon = new Pre_EarCommon(idEARDAY, earCode, eventCode, eventType, eventClass, earBook.DtEvent);
                                    earDet.EarCommons.Add(earCommon);
                                }

                                // Set Currencies
                                SetCounterValueData(earDet, idC, earBook);
                                // Process all counter values
                                NewEarAmounts(paid, received, earBook.DtEvent, DtPRSDate);
                                // Add counter values to Ear COMMON
                                AddAmountsToEar(earCommon);
                            }
                        }
                    }
                }
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return codeReturn;
        }

        /// <summary>
        /// Ajoute des EARCOMMON à partir des EARDAY passés, en suffixant l'EVENTCODE par '-1'.
        /// <para>Exemple: </para>
        /// <para>Un EARDAY passé avec EVENTCODE='CLO', donne lieu un EARCOMMON avec EARCODE='CLO-1'</para>
        /// <para>Un EARDAY passé avec EVENTCODE='LPC', donne lieu un EARCOMMON avec EARCODE='LPC-1'</para>
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <returns></returns>
        // EG 20180606 [23979] IRQ (EARGEN)
        protected Cst.ErrLevel CalcEarCommonForLastEar(DataDbTransaction pEfsTransaction)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            CheckCLOIsEarDay(pEfsTransaction);

            List<Pair<string, string>> lstData = new List<Pair<string, string>>();
            // RD 20140328 [19726]
            List<Pre_EarDet> earDetsNew = new List<Pre_EarDet>();

            // RD 20121024 [18201] Calculer les EARCOMMON uniquement sur les EAR non annulés
            foreach (Pre_EarBook earBook in m_EarBooks.FindAll(ear => ear.IdStActivation == Cst.StatusActivation.REGULAR))
            {
                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ||
                    (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                    break;

                lstData.Clear();
                lstData.Add(new Pair<string, string>("EAR", LogTools.IdentifierAndId(DtFunc.DateTimeToStringDateISO(m_EarDate), earBook.IdEAR)));
                lstData.Add(new Pair<string, string>("BOOK", LogTools.IdentifierAndId(earBook.IdB_Identifier, earBook.IdB)));
                lstData.Add(new Pair<string, string>("IDSTACTIVATION", earBook.IdStActivation.ToString()));
                lstData.Add(new Pair<string, string>("DTEVENT", DtFunc.DateTimeToStringDateISO(earBook.DtEvent)));

                foreach (Pre_EarDet earDet in earBook.EarDets)
                {
                    if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ||
                        (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                        break;

                    if (lstData.Exists(match => match.First == "INSTRSTREAMNO"))
                        lstData.Find(match => match.First == "INSTRSTREAMNO").Second =
                            earDet.instrumentNo.ToString() + " / " + earDet.streamNo.ToString();
                    else
                        lstData.Add(new Pair<string, string>("INSTRSTREAMNO", earDet.instrumentNo.ToString() + " / " + earDet.streamNo.ToString()));

                    List<DataRow> rowsToEarCommon = new List<DataRow>();

                    // EarCommon CLO-1
                    if (Cst.ErrLevel.SUCCESS == GetPreviousEarCLOToEarCommon(pEfsTransaction, earBook.IdB, earBook.DtEvent, earDet.instrumentNo, earDet.streamNo))
                    {
                        if (null != m_DtEarCLO)
                            rowsToEarCommon.AddRange((from row in m_DtEarCLO.Rows.Cast<DataRow>() select row));
                    }

                    if (IsTradeRisk)
                    {
                        string streamFCU =
                            (from earday in earDet.EarDays
                             from eardayAmount in earday.earAmounts
                             where eardayAmount.ExchangeType == ExchangeTypeFunc.FlowCurrency
                             select eardayAmount.IdC)
                            .Distinct().First();

                        // EarCommon LPC-1
                        if (Cst.ErrLevel.SUCCESS == GetPreviousEarLPCToEarCommon(pEfsTransaction, earBook.IdB, earDet.instrumentNo, earDet.streamNo, streamFCU))
                        {
                            if (null != m_DtEarLPC)
                                rowsToEarCommon.AddRange((from row in m_DtEarLPC.Rows.Cast<DataRow>() select row));
                        }
                    }

                    if (ArrFunc.IsFilled(rowsToEarCommon))
                    {
                        decimal paid = 0;
                        decimal received = 0;
                        int instrumentNo = 0;
                        int streamNo = 0;
                        string earCode = string.Empty;
                        string eventCode = string.Empty;
                        string eventType = string.Empty;
                        string eventClass = string.Empty;
                        string idC = string.Empty;
                        int idEARDAY = 0;
                        DateTime dtEvent;
                        Pre_EarCommon earCommon = null;

                        string eventTypeOld = string.Empty;
                        string eventClassOld = string.Empty;
                        string eventCodeOld = string.Empty;

                        // Set currencies
                        SetCounterValueData(earDet, earBook);

                        foreach (DataRow row in rowsToEarCommon)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ||
                                (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                break;

                            paid = Convert.ToDecimal(row["PAID"]);
                            received = Convert.ToDecimal(row["RECEIVED"]);
                            instrumentNo = Convert.ToInt32(row["INSTRUMENTNO"]);
                            streamNo = Convert.ToInt32(row["STREAMNO"]);
                            eventCode = row["EVENTCODE"].ToString();
                            earCode = eventCode + "-1"; //ex.: CLO-1, LPC-1
                            eventType = row["EVENTTYPE"].ToString();
                            eventClass = row["EVENTCLASS"].ToString();
                            idC = row["IDC"].ToString();
                            dtEvent = Convert.ToDateTime(row["DTEVENT"]);

                            idEARDAY = Convert.ToInt32(row["IDEARDAY"]);

                            // EarCommon and EarCommonAmount settings
                            if ((eventCodeOld != eventCode) || (eventTypeOld != eventType) || (eventClassOld != eventClass))
                            {
                                eventCodeOld = eventCode;
                                eventTypeOld = eventType;
                                eventClassOld = eventClass;

                                m_EarQuery.IdE.Value = Convert.ToInt32(row["IDE"]);

                                // EarDet setting
                                // RD 20140328 [19726]
                                Pre_EarDet earDetNew = earBook.EarDets.Find(det => (det.instrumentNo == instrumentNo) && (det.streamNo == streamNo));
                                if (earDetNew == null)
                                {
                                    earDetNew = earDetsNew.Find(det => (det.instrumentNo == instrumentNo) && (det.streamNo == streamNo));
                                    if (earDetNew == null)
                                    {
                                        earDetNew = new Pre_EarDet(instrumentNo, streamNo);
                                        earDetsNew.Add(earDetNew);
                                    }
                                }

                                // EarCommon setting CLO-1
                                earCommon = earDetNew[idEARDAY, earCode, eventCode, eventType, eventClass];
                                if (earCommon == null)
                                {
                                    earCommon = new Pre_EarCommon(idEARDAY, earCode, eventCode, eventType, eventClass, earBook.DtEvent);
                                    earDetNew.EarCommons.Add(earCommon);
                                }

                                // Process all counter values
                                NewEarAmounts(paid, received, idC, dtEvent, DtPRSDate);
                                // Add all counter values to EarCommon
                                AddAmountsToEar(earCommon);
                            }
                        }
                    }
                }

                if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                {
                    // RD 20140328 [19726]
                    earBook.EarDets.AddRange(earDetsNew);
                    earDetsNew.Clear();
                }
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return codeReturn;
        }

        /// <summary>
        /// Calculer les EARs calculés (EARCALC)
        /// </summary>
        /// <returns></returns>
        /// FI 20151027 [21513] Modify 
        // EG 20180606 [23979] IRQ (EARGEN)
        private Cst.ErrLevel CalcEarCalc(DataDbTransaction pEfsTransaction)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (ArrFunc.IsFilled(m_EarBooks))
            {
#if DEBUG
                diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
                m_AccVariables = new List<AccVariable>();

                // FI 20151027 [21513] Appel Delete_TMPEARCALCAMOUNT
                Delete_TMPEARCALCAMOUNT(pEfsTransaction);

                // FI 20151027 [21513] Appel LoadDefineEarCalc
                DataTable dt = LoadDefineEarCalc();

                if (ArrFunc.IsFilled(dt.Rows))
                {
                    IEnumerable<DataRow> allSequenceNo = from row in dt.Rows.Cast<DataRow>() select row;
#if DEBUG
                    diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
                    if (allSequenceNo.Count() > 0)
                    {
                        bool isToDeleteTmpTable = false;
                        int sequenceNoMax = allSequenceNo.Max(row => Convert.ToInt32(row["SEQUENCENO"]));
                        for (int sequenceNo = 0; sequenceNo <= sequenceNoMax; sequenceNo++)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ||
                                (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                break;

                            IEnumerable<DataRow> sequenceNoRows =
                                (from row in allSequenceNo
                                 where Convert.ToInt32(row["SEQUENCENO"]) == sequenceNo
                                 select row);

                            if (0 < sequenceNoRows.Count())
                            {
                                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                                    codeReturn = CalcEarCalcAmount(pEfsTransaction, sequenceNo, sequenceNoRows, ref isToDeleteTmpTable);

                                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                                    codeReturn = WriteEarCalc(pEfsTransaction, sequenceNo);
                            }
                        }

                        if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                            (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            if (isToDeleteTmpTable)
                                Delete_TMPEARCALCAMOUNT(pEfsTransaction); // FI 20151027 [21513] Appel Delete_TMPEARCALCAMOUNT
                        }
                    }
                }
            }
            return codeReturn;
        }

        /// <summary>
        /// Charger la liste des variables comptables à partir de la table ACCVARIABLE
        /// </summary>
        /// <param name="pDefinedEarCalc"></param>
        private void LoadAccVariable(List<DefinedEarCalc> pDefinedEarCalc)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

            try
            {
                // 1- Extraire toutes les Variables comptables contenues dans les arguments des montants calculés
                ArrayList stringsWithVariable = new ArrayList();

                foreach (DefinedEarCalc definedEarCalc in pDefinedEarCalc)
                {
                    foreach (string agAmounts in definedEarCalc.agLstAmounts)
                        stringsWithVariable.Add(agAmounts);
                }

                List<string> strAccVariables = AccVariableTools.InitVariables(stringsWithVariable);

                // 2- Charger la liste des variables comptables à partir de la table ACCVARIABLE
                AccVariableTools.LoadAccVariable(Cs, strAccVariables, ref m_AccVariables);

                // 3- Vérifier si toutes les variables comptables existent bien
                foreach (string stringVar in strAccVariables)
                {
                    if (false == m_AccVariables.Exists(var => var.Regex.Match(stringVar).Success))
                    {
                        AccLog.FireException( null,
                            new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05387",
                                new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                                stringVar,
                                m_LstMasterData.Find(match => match.First == "TRADE").Second,
                                m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                                m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second));
                    }
                }
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05386",
                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                        m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second));
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }

        /// <summary>
        /// Alimentation de EarCalcs pour la sequence {pSequenceNo}
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pSequenceNo">Représente la séquence</param>
        /// <param name="pSequenceNoRows">Représente les enregistrements présents dans DEFINEEARCALC en rapport avec la sequence</param>
        /// <param name="pIsToDeleteTmpTable"></param>
        /// <returns></returns>
        /// RD 20150814 [21263] Modify
        /// FI 20151027 [21513] Modify 
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel CalcEarCalcAmount(DataDbTransaction pEfsTransaction, int pSequenceNo,
            IEnumerable<DataRow> pSequenceNoRows, ref bool pIsToDeleteTmpTable)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            int rowAffected = 0;

            List<DefinedEarCalc> aDefinedEarCalc = new List<DefinedEarCalc>();
            foreach (DataRow row in pSequenceNoRows)
            {
                DefinedEarCalc defineEarCalc = new DefinedEarCalc(row["CALCTYPE"].ToString(),
                    row["AGFUNC"].ToString(),
                    row["AGAMOUNTS"].ToString(),
                    (Convert.IsDBNull(row["IDA"]) ? 0 : Convert.ToInt32(row["IDA"])),
                    Convert.ToBoolean(row["BYINSTRUMENT"]),
                    Convert.ToBoolean(row["BYSTREAM"]),
                    Convert.ToBoolean(row["BYDTEVENT"]),
                    Convert.ToBoolean(row["BYEVENT"]));

                aDefinedEarCalc.Add(defineEarCalc);
            }

            LoadAccVariable(aDefinedEarCalc);

            // Construct query for insert/select
            EarCalcAmountQuery earCalcAmountQuery = new EarCalcAmountQuery();

            m_EarQuery.SequenceNo.Value = pSequenceNo;

            foreach (Pre_EarBook pre_EarBook in m_EarBooks)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5370), 2,
                    new LogParam(pre_EarBook.IdEAR),
                    new LogParam(LogTools.IdentifierAndId(pre_EarBook.IdB_Identifier, pre_EarBook.IdB)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_EarDate)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(pre_EarBook.DtEvent)),
                    new LogParam(pre_EarBook.IdStActivation)));

                m_EarQuery.IdEAR.Value = pre_EarBook.IdEAR;

                foreach (DefinedEarCalc defineEarCalc in aDefinedEarCalc)
                {
                    if (defineEarCalc.ida == 0 || pre_EarBook.IdAEntity == defineEarCalc.ida)
                    {
                        rowAffected = 0;
                        m_EarQuery.CalcType.Value = defineEarCalc.calcType;

                        //if (defineEarCalc.agFunction == "DIFF")
                        //20070726 PL Temporary...
                        //Attention: DIF2 a été créé pour palier de façon temporaire à un besoin URGENT/
                        //           Cette fonction est à finalisé pour être plu sstandard, il faudra reprendre les paramétrages existants (ex. BANCAPERTA)
                        //           Cette fonction opère un DIFF mais à la diffrénece du DIFF, elle recherche le montant 2 
                        //           sur le stream suivant de même sens et en croisant les montants PAY / REC.
                        //           Elle est utilisée à ce jour pour "sommer" des TEI/NOM avec des STI/NOM afin de simuler un flux de variation de capital.
                        // RD 20150814 [21263] Add PROD and PRO2
                        //Attention: PRO2, cette fonction opère un PROD mais à la différence du PROD, pour le montant 2 elle croise PAY / REC.


                        switch (defineEarCalc.agFunction)
                        {
                            case AGFuncEnum.DIFF:
                            case AGFuncEnum.DIF2:
                            case AGFuncEnum.PROD:
                            case AGFuncEnum.PRO2:
                                // DIFF/PROD functions
                                rowAffected = CalcEarCalcDiffProd(pEfsTransaction, earCalcAmountQuery, pre_EarBook, defineEarCalc);
                                break;
                            case AGFuncEnum.SUM:
                            case AGFuncEnum.AVG:
                                rowAffected = CalcEarCalcAvgSum(pEfsTransaction, earCalcAmountQuery, pre_EarBook, defineEarCalc);
                                break;
                            case AGFuncEnum.PRORATA:// FI 20151027 [21513] Modify
                                rowAffected = CalcEarCalcProrata(pEfsTransaction, pre_EarBook, defineEarCalc);
                                break;
                        }

                        if (rowAffected > 0)
                        {
                            pIsToDeleteTmpTable = true;
                            AddEarCalc(pEfsTransaction, pre_EarBook, earCalcAmountQuery, pSequenceNo);
                        }
                    }
                }
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return ret;
        }

        /// <summary>
        /// Alimentation de TMPEARCALCAMOUNT et TMPEARCALCDET
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pEarCalcAmountQuery"></param>
        /// <param name="pPre_EarBook"></param>
        /// <param name="pDefinedEarCalc"></param>
        /// <returns></returns>
        // RD 20150814 [21280] Modify
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        private int CalcEarCalcDiffProd(DataDbTransaction pEfsTransaction,
            EarCalcAmountQuery pEarCalcAmountQuery, Pre_EarBook pPre_EarBook, DefinedEarCalc pDefinedEarCalc)
        {
            int rowAffected = 0;
            string tmpDebug;
            string SQL_Amount2 = pEarCalcAmountQuery.SQL_Select_Amount2;
            if (pDefinedEarCalc.agFunction == AGFuncEnum.DIF2)
                SQL_Amount2 += pEarCalcAmountQuery.SQL_Inner_Amount2;

            SQL_Amount2 += pEarCalcAmountQuery.SQL_Where_Amount2;

            SQL_Amount2 = SQL_Amount2.Replace("{BYINSTRUMENTNO}", pDefinedEarCalc.IsByInstrument ? SQLCst.AND + "vear.INSTRUMENTNO=@INSTRUMENTNO" + Cst.CrLf : string.Empty);
            SQL_Amount2 = SQL_Amount2.Replace("{BYSTREAMNO}", pDefinedEarCalc.IsByStream ? SQLCst.AND + "vear.STREAMNO=@STREAMNO" + Cst.CrLf : string.Empty);
            SQL_Amount2 = SQL_Amount2.Replace("{BYDTEVENT}", pDefinedEarCalc.IsByDtEvent ? SQLCst.AND + "vear.DTACCOUNT=@DTACCOUNT" + Cst.CrLf : string.Empty);
            //
            if (pDefinedEarCalc.IsByEvent)
            {
                SQL_Amount2 = SQL_Amount2.Replace("{BYEVENT_JOIN}", SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.VW_EAREVENT.ToString() + " earEvent1" + Cst.CrLf +
                                                                    SQLCst.ON + "earEvent1.IDEARTYPE=@IDEARTYPE" + Cst.CrLf +
                                                                    SQLCst.AND + "earEvent1.EARTYPE=@EARTYPE" + Cst.CrLf +
                                                                    SQLCst.AND + "earEvent1.EARTYPE in (" + DataHelper.SQLString("EARDAY") + "," + DataHelper.SQLString("EARCOMMON") + ")" + Cst.CrLf +
                                                                    SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.EVENT.ToString() + " event1" + Cst.CrLf +
                                                                    SQLCst.ON + "event1.IDE=earEvent1.IDE" + Cst.CrLf +
                                                                    SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.VW_EAREVENT.ToString() + " earEvent2" + Cst.CrLf +
                                                                    SQLCst.ON + "earEvent2.IDEARTYPE=vear.IDEARTYPE" + Cst.CrLf +
                                                                    SQLCst.AND + "earEvent2.EARTYPE=vear.EARTYPE" + Cst.CrLf +
                                                                    SQLCst.AND + "earEvent2.EARTYPE in (" + DataHelper.SQLString("EARDAY") + "," + DataHelper.SQLString("EARCOMMON") + ")" + Cst.CrLf +
                                                                    SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.EVENT.ToString() + " event2" + Cst.CrLf +
                                                                    SQLCst.ON + "event2.IDE=earEvent2.IDE" + Cst.CrLf);
                //
                SQL_Amount2 = SQL_Amount2.Replace("{BYEVENT_WHERE}", SQLCst.AND + "(event1.IDE_EVENT=event2.IDE_EVENT" + Cst.CrLf +
                                                                    SQLCst.OR + "earEvent1.IDEARTYPE" + SQLCst.IS_NULL + Cst.CrLf +
                                                                    SQLCst.OR + "earEvent2.IDEARTYPE" + SQLCst.IS_NULL + ")");
            }
            else
            {
                SQL_Amount2 = SQL_Amount2.Replace("{BYEVENT_JOIN}", string.Empty);
                SQL_Amount2 = SQL_Amount2.Replace("{BYEVENT_WHERE}", string.Empty);
            }

            IDbDataParameter paramIDEARTYPE = new DataParameter(Cs, "IDEARTYPE", DbType.Int32).DbDataParameter;
            IDbDataParameter paramEARTYPE = new DataParameter(Cs, "EARTYPE", DbType.AnsiString).DbDataParameter;
            IDbDataParameter paramNoZero = new DataParameter(Cs, "NOZERO", DbType.Int32) { Value = 0 }.DbDataParameter;

            string[] funcArguments1 = pDefinedEarCalc.agLstAmounts[0].Split(';');
            //
            m_EarQuery.EarCode.Value = funcArguments1[0].Trim();
            m_EarQuery.AmountType.Value = funcArguments1[1].Trim();
            m_EarQuery.EventClass.Value = funcArguments1[2].Trim();

            if ((funcArguments1.Length > 3) && StrFunc.IsFilled(funcArguments1[3]))
                m_EarQuery.ExchangeType.Value = funcArguments1[3].Trim();
            else
                m_EarQuery.ExchangeType.Value = ExchangeTypeFunc.FlowCurrency;
            //											
            #region tmpDebug
            tmpDebug = m_EarQuery.CalcType.Value.ToString() + " - " + m_EarQuery.EarCode.Value.ToString() + " - " + m_EarQuery.AmountType.Value.ToString();
            tmpDebug += " - " + m_EarQuery.EventClass.Value.ToString() + " - " + m_EarQuery.ExchangeType.Value.ToString();
            tmpDebug += " - " + m_EarQuery.IdStProcess.Value.ToString() + " - " + m_EarQuery.IdEAR.Value.ToString();
            #endregion tmpDebug

            DataSet dsEarCalcAmount1 = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text,
            pEarCalcAmountQuery.SQL_Amount1,
            m_EarQuery.AmountType, m_EarQuery.EarCode, m_EarQuery.EventClass, m_EarQuery.IdEAR,
            m_EarQuery.IdStProcess, m_EarQuery.ExchangeType);

            //
            if (null != dsEarCalcAmount1)
            {
                #region Calculate difference (First Amount - Second Amount) or product (First Amount * Second Amount)
                DataTable dtEarCalcAmount1 = dsEarCalcAmount1.Tables[0];
                if (0 < dtEarCalcAmount1.Rows.Count)
                {
                    foreach (DataRow rowAmount1 in dtEarCalcAmount1.Rows)
                    {
                        decimal receivedAmount1 = Convert.ToDecimal(rowAmount1["RECEIVED"]);
                        decimal paidAmount1 = Convert.ToDecimal(rowAmount1["PAID"]);
                        int idEarType1 = Convert.ToInt32(rowAmount1["IDEARTYPE"]);
                        string earType1 = rowAmount1["EARTYPE"].ToString();
                        //
                        decimal receivedAmount2 = 0;
                        decimal paidAmount2 = 0;
                        int idEarType2 = 0;
                        string earType2 = string.Empty;

                        string strAccVariable = string.Empty;
                        string strAccVariableValue = string.Empty;

                        #region Get Second Amount
                        /* Si le deuxième argument existe on le prend sinon:
                         * - Pour les fonctions DIFF et DIFF2 on considère l'événement précédent (eg. CLO --> CLO-1  )
                         * - Pour la fonction PROD on considère l'événement lui même pour simuler la fonction mathématique CARREE (puissance 2)
                         */
                        if ((pDefinedEarCalc.agLstAmounts.Length > 1) && StrFunc.IsFilled(pDefinedEarCalc.agLstAmounts[1]))
                        {
                            // RD 20150814 [21280] Use acc variable
                            List<string> strAccVariables = AccVariableTools.InitVariables(pDefinedEarCalc.agLstAmounts[1]);
                            if (ArrFunc.IsFilled(strAccVariables))
                                strAccVariable = strAccVariables[0];

                            if (StrFunc.IsEmpty(strAccVariable))
                            {
                                string[] funcArguments2 = pDefinedEarCalc.agLstAmounts[1].Split(';');
                                m_EarQuery.EarCode.Value = funcArguments2[0].Trim();
                                m_EarQuery.AmountType.Value = funcArguments2[1].Trim();
                                m_EarQuery.EventClass.Value = funcArguments2[2].Trim();

                                if ((funcArguments2.Length > 3) && StrFunc.IsFilled(funcArguments2[3]))
                                    m_EarQuery.ExchangeType.Value = funcArguments2[3].Trim();
                                else
                                    m_EarQuery.ExchangeType.Value = ExchangeTypeFunc.FlowCurrency;
                            }
                        }
                        else if ((pDefinedEarCalc.agFunction == AGFuncEnum.DIFF) || (pDefinedEarCalc.agFunction == AGFuncEnum.DIF2))
                        {
                            m_EarQuery.EarCode.Value = funcArguments1[0] + "-1"; //eg. CLO --> CLO-1
                        }

                        m_EarQuery.InstrumentNo.Value = Convert.ToInt32(rowAmount1["INSTRUMENTNO"]);
                        m_EarQuery.StreamNo.Value = Convert.ToInt32(rowAmount1["STREAMNO"]);
                        m_EarQuery.IdC.Value = rowAmount1["IDC"].ToString();
                        m_EarQuery.DtAccount.Value = Convert.ToDateTime(rowAmount1["DTACCOUNT"]);
                        //                                                        
                        if (pDefinedEarCalc.IsByEvent || StrFunc.IsFilled(strAccVariable))
                        {
                            paramIDEARTYPE.Value = idEarType1;
                            paramEARTYPE.Value = earType1;
                        }
                        //														
                        #region tmpDebug
                        tmpDebug = m_EarQuery.CalcType.Value.ToString() + " - " + m_EarQuery.EarCode.Value.ToString() + " - " + m_EarQuery.AmountType.Value.ToString();
                        tmpDebug += " - " + m_EarQuery.EventClass.Value.ToString() + " - " + m_EarQuery.ExchangeType.Value.ToString();
                        tmpDebug += " - " + m_EarQuery.IdStProcess.Value.ToString() + " - " + m_EarQuery.IdEAR.Value.ToString();
                        tmpDebug += " - " + m_EarQuery.InstrumentNo.Value.ToString() + " - " + m_EarQuery.StreamNo.Value.ToString();
                        #endregion tmpDebug

                        // RD 20150814 [21280] Use acc variable
                        if (StrFunc.IsFilled(strAccVariable))
                        {
                            AccVariable accVariable = m_AccVariables.Find(item => item.Regex.Match(strAccVariable).Success);

                            try
                            {
                                List<Pair<string, IDbDataParameter>> lParameters = new List<Pair<string, IDbDataParameter>>
                                {
                                    new Pair<string, IDbDataParameter>("IdEAR", m_EarQuery.IdEAR),
                                    new Pair<string, IDbDataParameter>("EarCode", m_EarQuery.EarCode),
                                    new Pair<string, IDbDataParameter>("AmountType", m_EarQuery.AmountType),
                                    new Pair<string, IDbDataParameter>("EventClass", m_EarQuery.EventClass),
                                    new Pair<string, IDbDataParameter>("NoZero", paramNoZero),
                                    new Pair<string, IDbDataParameter>("InstrumentNo", m_EarQuery.InstrumentNo),
                                    new Pair<string, IDbDataParameter>("StreamNo", m_EarQuery.StreamNo),
                                    new Pair<string, IDbDataParameter>("IDEARTYPE", paramIDEARTYPE),
                                    new Pair<string, IDbDataParameter>("EARTYPE", paramEARTYPE)
                                };

                                strAccVariableValue = AccVariableTools.ValuateAccVariable(pEfsTransaction, accVariable, lParameters);

                                if (StrFunc.IsEmpty(strAccVariableValue))
                                {
                                    // Unable to evaluate variable
                                    AccLog.FireException( null,
                                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05388",
                                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                                        accVariable.Data,
                                        pDefinedEarCalc.calcType, pDefinedEarCalc.Data,
                                        LogTools.IdentifierAndId(DtFunc.DateTimeToStringDateISO(m_EarDate), pPre_EarBook.IdEAR),
                                        m_EarQuery.InstrumentNo.Value.ToString(), m_EarQuery.StreamNo.Value.ToString(),
                                        LogTools.IdentifierAndId(pPre_EarBook.IdB_Identifier, pPre_EarBook.IdB),
                                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                                        m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second));
                                }
                            }
                            catch (Exception ex)
                            {
                                AccLog.FireException(ex,
                                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05389",
                                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                                        accVariable.Data,
                                        pDefinedEarCalc.calcType, pDefinedEarCalc.Data,
                                        m_EarQuery.InstrumentNo.Value.ToString(), m_EarQuery.StreamNo.Value.ToString(),
                                        LogTools.IdentifierAndId(DtFunc.DateTimeToStringDateISO(m_EarDate), pPre_EarBook.IdEAR),
                                        LogTools.IdentifierAndId(pPre_EarBook.IdB_Identifier, pPre_EarBook.IdB),
                                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                                        m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second));
                            }
                        }
                        else
                        {
                            List<IDbDataParameter> lParameters = new List<IDbDataParameter>
                            {
                                m_EarQuery.AmountType,
                                m_EarQuery.EarCode,
                                m_EarQuery.EventClass,
                                m_EarQuery.IdEAR,
                                m_EarQuery.IdStProcess,
                                m_EarQuery.ExchangeType
                            };
                            if (pDefinedEarCalc.IsByInstrument)
                                lParameters.Add(m_EarQuery.InstrumentNo);
                            if (pDefinedEarCalc.IsByStream || (pDefinedEarCalc.agFunction == AGFuncEnum.DIF2))
                                lParameters.Add(m_EarQuery.StreamNo);
                            if (pDefinedEarCalc.IsByDtEvent)
                                lParameters.Add(m_EarQuery.DtAccount);
                            if (pDefinedEarCalc.IsByEvent)
                            {
                                lParameters.Add(paramIDEARTYPE);
                                lParameters.Add(paramEARTYPE);
                            }

                            DataSet dsEarCalcAmount2 = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text, SQL_Amount2, lParameters.ToArray());


                            if (null != dsEarCalcAmount2)
                            {
                                DataTable dtEarCalcAmount2 = dsEarCalcAmount2.Tables[0];
                                switch (dtEarCalcAmount2.Rows.Count)
                                {
                                    case 0:
                                        /* Si le Montant correspondant au deuxième Argument n'existe pas on considère zéro */
                                        receivedAmount2 = 0;
                                        paidAmount2 = 0;
                                        break;
                                    case 1:
                                        if (pDefinedEarCalc.agFunction == AGFuncEnum.DIF2)
                                        {
                                            //Warning: On inverse ici les montants (20070726 PL)
                                            receivedAmount2 = Convert.ToDecimal(dtEarCalcAmount2.Rows[0]["PAID"]);
                                            paidAmount2 = Convert.ToDecimal(dtEarCalcAmount2.Rows[0]["RECEIVED"]);
                                        }
                                        else
                                        {
                                            receivedAmount2 = Convert.ToDecimal(dtEarCalcAmount2.Rows[0]["RECEIVED"]);
                                            paidAmount2 = Convert.ToDecimal(dtEarCalcAmount2.Rows[0]["PAID"]);
                                        }
                                        //
                                        idEarType2 = Convert.ToInt32(dtEarCalcAmount2.Rows[0]["IDEARTYPE"]);
                                        earType2 = dtEarCalcAmount2.Rows[0]["EARTYPE"].ToString();
                                        break;
                                    default:
                                        //20070725 PL Add throw
                                        //Il ne devrait jamais y avoir plus d'une ligne dans le jeu de résultats
                                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05341",
                                            new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE),
                                            pDefinedEarCalc.calcType, pDefinedEarCalc.Data,
                                            LogTools.IdentifierAndId(DtFunc.DateTimeToStringDateISO(m_EarDate), pPre_EarBook.IdEAR),
                                            LogTools.IdentifierAndId(pPre_EarBook.IdB_Identifier, pPre_EarBook.IdB),
                                            m_LstMasterData.Find(match => match.First == "TRADE").Second,
                                            m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                                            m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second);
                                }
                            }
                        }
                        #endregion	Get Second Amount

                        decimal paidAmountResult = 0;

                        // RD 20150814 [21280] Use acc variable for DIFF, DIFF2, PROD et PRO2
                        if (StrFunc.IsFilled(strAccVariable))
                        {
                            decimal netAmount1 = (paidAmount1 - receivedAmount1);
                            decimal amount2 = DecFunc.DecValue(strAccVariableValue);

                            if (pDefinedEarCalc.agFunction == AGFuncEnum.PROD)
                            {
                                /* Pour la fonction PROD
                                 * - Le montant net (montant payé - montant reçu) du premier argument est multiplié par le montant correspondant à la variable comptable.
                                 * - Si le résultat final est positif alors il est considéré comme étant un montant Payé
                                 * - Si le résultat final est négatif alors il est considéré comme étant un montant Reçu
                                 */

                                paidAmountResult = (netAmount1 * amount2);
                            }
                            else if (pDefinedEarCalc.agFunction == AGFuncEnum.PRO2)
                            {
                                /* Pour la fonction PRO2
                                 * - Le montant net (montant payé - montant reçu) du premier argument est divisé (pour simuler la fonction de DIVISION) par le montant correspondant à la variable comptable.
                                 * - Si le résultat final est positif alors il est considéré comme étant un montant Payé
                                 * - Si le résultat final est négatif alors il est considéré comme étant un montant Reçu
                                 */

                                paidAmountResult = (netAmount1 / amount2);
                            }
                            else if (pDefinedEarCalc.agFunction == AGFuncEnum.DIFF)
                            {
                                /* Pour la fonction DIFF
                                 * - Le montant net (montant payé - montant reçu) du premier argument est déminué en valeur absolue par le montant correspondant à la variable comptable.
                                 * - Si le résultat final est positif alors il est considéré comme étant un montant Payé
                                 * - Si le résultat final est négatif alors il est considéré comme étant un montant Reçu
                                 */

                                paidAmountResult = (netAmount1 > 0 ? netAmount1 - amount2 : netAmount1 + amount2);
                            }
                            else if (pDefinedEarCalc.agFunction == AGFuncEnum.DIF2)
                            {
                                /* Pour la fonction DIFF/DIFF2
                                 * - Le montant net (montant payé - montant reçu) du premier argument est augmenté (pour simuler la fonction de ADDITION) en valeur absolue par le montant correspondant à la variable comptable.
                                 * - Si le résultat final est positif alors il est considéré comme étant un montant Payé
                                 * - Si le résultat final est négatif alors il est considéré comme étant un montant Reçu
                                 */

                                paidAmountResult = (netAmount1 > 0 ? netAmount1 + amount2 : netAmount1 - amount2);
                            }

                            m_EarQuery.Paid.Value = (paidAmountResult > 0 ? paidAmountResult : 0);
                            m_EarQuery.Received.Value = (paidAmountResult > 0 ? 0 : -paidAmountResult);
                        }
                        else
                        {
                            if (pDefinedEarCalc.agFunction == AGFuncEnum.PROD)
                            {
                                /* Pour la fonction PROD
                                 * - Le montant payé correspondant au premier Argument est multiplié par le montant payé correspondant au deuxième Argument
                                 * - Le montant reçu correspondant au premier Argument est multiplié par le montant reçu correspondant au deuxième Argument
                                 */

                                m_EarQuery.Paid.Value = paidAmount1 * paidAmount2;
                                m_EarQuery.Received.Value = receivedAmount1 * receivedAmount2;
                            }
                            else if (pDefinedEarCalc.agFunction == AGFuncEnum.PRO2)
                            {
                                /* Pour la fonction PRO2
                                 * - Le montant payé correspondant au premier Argument est multiplié par le montant reçu correspondant au deuxième Argument
                                 * - Le montant reçu correspondant au premier Argument est multiplié par le montant payé correspondant au deuxième Argument
                                 */

                                m_EarQuery.Paid.Value = paidAmount1 * receivedAmount2;
                                m_EarQuery.Received.Value = receivedAmount1 * paidAmount2;
                            }
                            else
                            {
                                /* Pour la fonction DIFF/DIFF2
                                 * - On fait d'abord la différence entre le montant payé correspondant au premier Argument et le montant payé correspondant au deuxième Argument
                                 * - On fait ensuiet la différence entre le montant reçu correspondant au premier Argument et le montant reçu correspondant au deuxième Argument
                                 * - On fait enfin la différence entre le premier résultat et le deuxième résultat
                                 * - Si le résultat final est positif alors il est considéré comme étant un montant Payé
                                 * - Si le résultat final est négatif alors il est considéré comme étant un montant Reçu
                                 */

                                paidAmountResult = (paidAmount1 - paidAmount2) - (receivedAmount1 - receivedAmount2);

                                m_EarQuery.Paid.Value = (paidAmountResult > 0 ? paidAmountResult : 0);
                                m_EarQuery.Received.Value = (paidAmountResult > 0 ? 0 : -paidAmountResult);
                            }
                        }

                        m_EarQuery.EarCode.Value = funcArguments1[0].Trim();
                        m_EarQuery.EventClass.Value = funcArguments1[2].Trim();
                        m_EarQuery.ExchangeType.Value = ExchangeTypeFunc.FlowCurrency;

                        rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                            m_EarQuery.m_SQLQueryTempEarCalc,
                            m_EarQuery.IdEAR, m_EarQuery.DtAccount, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo,
                            m_EarQuery.EarCode, m_EarQuery.CalcType, m_EarQuery.EventClass, m_EarQuery.ExchangeType,
                            m_EarQuery.IdC, m_EarQuery.Paid, m_EarQuery.Received, m_EarQuery.IdStProcess);

                        //
                        #region Write EarCalcDet
                        if (rowAffected > 0)
                        {
                            paramIDEARTYPE.Value = idEarType1;
                            paramEARTYPE.Value = earType1;
                            //
                            rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                                pEarCalcAmountQuery.SQL_AmountDet,
                                m_EarQuery.IdEAR, m_EarQuery.DtAccount, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo,
                                m_EarQuery.EarCode, m_EarQuery.CalcType, m_EarQuery.EventClass, m_EarQuery.ExchangeType,
                                m_EarQuery.IdC, m_EarQuery.IdStProcess, paramIDEARTYPE, paramEARTYPE);
                            //
                            if (idEarType2 > 0)
                            {
                                paramIDEARTYPE.Value = idEarType2;
                                paramEARTYPE.Value = earType2;
                                //
                                rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                                    pEarCalcAmountQuery.SQL_AmountDet,
                                    m_EarQuery.IdEAR, m_EarQuery.DtAccount,
                                    m_EarQuery.InstrumentNo, m_EarQuery.StreamNo,
                                    m_EarQuery.EarCode, m_EarQuery.CalcType, m_EarQuery.EventClass,
                                    m_EarQuery.ExchangeType, m_EarQuery.IdC, m_EarQuery.IdStProcess,
                                    paramIDEARTYPE, paramEARTYPE);
                            }
                        }
                        #endregion Write EarCalcDet
                    }
                }
                #endregion
            }

            return rowAffected;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pEarCalcAmountQuery"></param>
        /// <param name="pPre_EarBook"></param>
        /// <param name="pDefinedEarCalc"></param>
        /// <returns></returns>
        private int CalcEarCalcAvgSum(DataDbTransaction pEfsTransaction,
            EarCalcAmountQuery pEarCalcAmountQuery, Pre_EarBook pPre_EarBook, DefinedEarCalc pDefinedEarCalc)
        {
            pDefinedEarCalc.TableAlias = string.Empty;
            m_EarQuery.ExchangeType.Value = ExchangeTypeFunc.FlowCurrency;

            ArrayList paramList = new ArrayList();
            string sqlAmount = pDefinedEarCalc.GetSqlAmount(Cs, ref paramList);

            string SQLQueryAmount = pEarCalcAmountQuery.SQL_Insert;
            SQLQueryAmount = SQLQueryAmount.Replace("{PARAMETERS}", pDefinedEarCalc.Parameters);
            SQLQueryAmount = SQLQueryAmount.Replace("{AMOUNTTYPE}", sqlAmount);
            SQLQueryAmount = SQLQueryAmount.Replace("{PAID}", pDefinedEarCalc.Paid);
            SQLQueryAmount = SQLQueryAmount.Replace("{RECEIVED}", pDefinedEarCalc.Received);

            pDefinedEarCalc.TableAlias = "veaa.";
            sqlAmount = pDefinedEarCalc.GetSqlAmount(Cs, ref paramList);

            string SQLQueryAmountDet = pEarCalcAmountQuery.SQL_Insert_Det;

            if (pDefinedEarCalc.IsByInstrument)
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{INSTRUMENTNO}", "teca.INSTRUMENTNO=" + pDefinedEarCalc.TableAlias + "INSTRUMENTNO");
            else
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{INSTRUMENTNO}", "teca.INSTRUMENTNO" + SQLCst.IS_NULL + SQLCst.OR + "teca.INSTRUMENTNO=0");

            if (pDefinedEarCalc.IsByStream)
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{STREAMNO}", "teca.STREAMNO=" + pDefinedEarCalc.TableAlias + "STREAMNO");
            else
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{STREAMNO}", "teca.STREAMNO" + SQLCst.IS_NULL + SQLCst.OR + "teca.STREAMNO=0");

            SQLQueryAmountDet = SQLQueryAmountDet.Replace("{AMOUNTTYPE}", sqlAmount);
            //
            if (pDefinedEarCalc.agLstAmounts.Length == 1)
            {
                SQLQueryAmount = SQLQueryAmount.Replace("{EARCODE}", "EARCODE");
                SQLQueryAmount = SQLQueryAmount.Replace("{BYEARCODE}", ",EARCODE");
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{EARCODE}", pDefinedEarCalc.TableAlias + "EARCODE");
                //
                SQLQueryAmount = SQLQueryAmount.Replace("{EVENTCLASS}", "EVENTCLASS");
                SQLQueryAmount = SQLQueryAmount.Replace("{BYEVENTCLASS}", ",EVENTCLASS");
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{EVENTCLASS}", pDefinedEarCalc.TableAlias + "EVENTCLASS");
            }
            else
            {
                m_EarQuery.EarCode.Value = "CALC";
                m_EarQuery.EventClass.Value = pDefinedEarCalc.agFunction.ToString().Substring(0, SQLCst.UT_EVENT_LEN);
                //
                SQLQueryAmount = SQLQueryAmount.Replace("{EARCODE}", m_EarQuery.EarCode.ParameterName);
                SQLQueryAmount = SQLQueryAmount.Replace("{BYEARCODE}", string.Empty);
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{EARCODE}", m_EarQuery.EarCode.ParameterName);
                //
                SQLQueryAmount = SQLQueryAmount.Replace("{EVENTCLASS}", m_EarQuery.EventClass.ParameterName);
                SQLQueryAmount = SQLQueryAmount.Replace("{BYEVENTCLASS}", string.Empty);
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{EVENTCLASS}", m_EarQuery.EventClass.ParameterName);
                //
                paramList.Add(m_EarQuery.EarCode);
                paramList.Add(m_EarQuery.EventClass);
            }
            //
            paramList.Add(m_EarQuery.CalcType);
            paramList.Add(m_EarQuery.IdEAR);
            paramList.Add(m_EarQuery.IdStProcess);
            paramList.Add(m_EarQuery.ExchangeType);
            //
            if (pDefinedEarCalc.IsByDtEvent)
            {
                SQLQueryAmount = SQLQueryAmount.Replace("{DTACCOUNT}", "DTACCOUNT");
                SQLQueryAmount = SQLQueryAmount.Replace("{BYDTACCOUNT}", ",DTACCOUNT");
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{DTACCOUNT}", pDefinedEarCalc.TableAlias + "DTACCOUNT");
            }
            else
            {
                SQLQueryAmount = SQLQueryAmount.Replace("{DTACCOUNT}", "@DTACCOUNT");
                SQLQueryAmount = SQLQueryAmount.Replace("{BYDTACCOUNT}", string.Empty);
                SQLQueryAmountDet = SQLQueryAmountDet.Replace("{DTACCOUNT}", "@DTACCOUNT");
                m_EarQuery.DtAccount.Value = pPre_EarBook.DtEvent;
                //	
                paramList.Add(m_EarQuery.DtAccount);
            }

            IDbDataParameter[] paramsEarCalcSum = (IDbDataParameter[])paramList.ToArray(typeof(IDbDataParameter));
            int rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, SQLQueryAmount, paramsEarCalcSum);
            if (rowAffected > 0)
                rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, SQLQueryAmountDet, paramsEarCalcSum);


            return rowAffected;
        }

        /// <summary>
        ///  Alimentation de EarCalcs à partir de TMPEARCALCAMOUNT
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pPre_EarBook"></param>
        /// <param name="pEarCalcAmountQuery"></param>
        /// <param name="pSequenceNo"></param>
        /// FI 20151027 [21513] Modify
        private void AddEarCalc(DataDbTransaction pEfsTransaction, Pre_EarBook pPre_EarBook,
            EarCalcAmountQuery pEarCalcAmountQuery, int pSequenceNo)
        {
            ProcessStateTools.StatusEnum idstprocessenum;
            m_EarQuery.ExchangeType.Value = ExchangeTypeFunc.FlowCurrency;

            string querySelect_EarCalcAmount = pEarCalcAmountQuery.SQL_Select_EarCalcAmount.ToString();

            DataSet dsEarCalcAmount = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text,
                  querySelect_EarCalcAmount,
                  m_EarQuery.SequenceNo, m_EarQuery.CalcType, m_EarQuery.IdEAR, m_EarQuery.ExchangeType);


            if (null != dsEarCalcAmount)
            {
                // EarCalcAmount to Pre_EarCalc and Pre_EarCalcAmount
                DataTable dtEarCalcAmount = dsEarCalcAmount.Tables[0];
                if (0 < dtEarCalcAmount.Rows.Count)
                {
                    foreach (DataRow row in dtEarCalcAmount.Rows)
                    {
                        DateTime dtAccount = Convert.ToDateTime(row["DTACCOUNT"]);
                        string earCode = row["EARCODE"].ToString();
                        string eventClass = row["EVENTCLASS"].ToString();
                        string calcType = row["CALCTYPE"].ToString();
                        int instrumentNo = Convert.ToInt32(row["INSTRUMENTNO"]);
                        int streamNo = Convert.ToInt32(row["STREAMNO"]);
                        string idC = row["IDC"].ToString();
                        decimal paid = Convert.ToDecimal(row["PAID"]);
                        decimal received = Convert.ToDecimal(row["RECEIVED"]);
                        string idstprocess = row["IDSTPROCESS"].ToString();
                        string agFunc = row["AGFUNC"].ToString();
                        string agAmounts = row["AGAMOUNTS"].ToString();
                        int idEarCalc = Convert.ToInt32(row["IDEARCALC"]);
                        int[] earCalcEvent = null;

                        // Get Events wich compose this EarCalc
                        m_EarQuery.IdEARCALC.Value = idEarCalc;


                        DataSet dsEarCalcEvent = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text,
                           pEarCalcAmountQuery.SQL_Select_EarCalcEvent.ToString(), m_EarQuery.IdEARCALC);


                        if (null != dsEarCalcEvent)
                        {
                            DataTable dtEarCalcEvent = dsEarCalcEvent.Tables[0];
                            if (0 < dtEarCalcEvent.Rows.Count)
                            {
                                foreach (DataRow rowIde in dtEarCalcEvent.Rows)
                                    AddEarTypeEvent(ref earCalcEvent, Convert.ToInt32(rowIde["IDE"]));
                            }
                        }

                        idstprocessenum = (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), idstprocess);

                        Pre_EarDet earDet = pPre_EarBook.EarDets.Find(det => (det.instrumentNo == instrumentNo) && (det.streamNo == streamNo));
                        if (earDet == null)
                        {
                            earDet = new Pre_EarDet(instrumentNo, streamNo);
                            pPre_EarBook.EarDets.Add(earDet);
                        }

                        Pre_EarCalc pre_EarCalc = null;
                        // FI 20151027 [21513]
                        // Il est désormais possible d'avoir n fois un même montant calculé en concordance avec le MPD
                        //Pre_EarCalc pre_EarCalc = earDet[earCode, calcType, eventClass, dtAccount];
                        //if (pre_EarCalc == null)
                        //{
                        pre_EarCalc = new Pre_EarCalc(earCode, calcType, eventClass, dtAccount, agFunc, agAmounts, pSequenceNo, earCalcEvent);
                        earDet.EarCalcs.Add(pre_EarCalc);
                        //}

                        // Set currencies
                        SetCounterValueData(earDet, idC, pPre_EarBook);

                        // Process all counter values
                        NewEarAmounts(paid, received, dtAccount);

                        // Add counter values to EarCalc
                        AddAmountsToEar(pre_EarCalc);
                    }
                }
            }
        }

        /// <summary>
        /// Vérification si l'EVENTCODE 'CLO' est candidat à génération d'EARDAY via EVENTENUM.EAR_DAY
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// EG 20121227 Gestion Nouvelle colonne EAR_DAY (Puissance de 2) 
        /// FI 20170607 [23221] Modify
        private void CheckCLOIsEarDay(DataDbTransaction pEfsTransaction)
        {
            // RD 20120809 [18070] Optimisation
            /*
            m_IsCLOEarDay = (false == m_TradeInfo.IsETDContext);
            if (m_IsCLOEarDay)
            {
                m_IsCLOEarDay = false;
                string SQLSelect = SQLCst.SELECT + "en.ISEARDAY" + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENTENUM + " en" + Cst.CrLf;
                SQLSelect += SQLCst.WHERE + "en.CODE = @CONSTEVENTCODE" + Cst.CrLf;
                SQLSelect += SQLCst.AND + "en.VALUE = @EVENTCODE" + Cst.CrLf;
                m_EarQuery.EventCode.Value = EventCodeFunc.DailyClosing;
                object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(pEfsTransaction.Cs), CommandType.Text, SQLSelect,
                    m_EarQuery.ConstEventCode, m_EarQuery.EventCode);
                if (null != obj)
                    m_IsCLOEarDay = BoolFunc.IsTrue(obj);
            }
            */

            // EG 20121227
            // FI 20170607 [23221] Prise en considération de GPRODUCT= 'COM'
            m_IsCLOEarDay = false;
            string SQLSelect = SQLCst.SELECT + "case @GPRODUCT when 'ADM' then en.EAR_DAY_ADM" + Cst.CrLf;
            SQLSelect += "when 'ASSET' then en.EAR_DAY_ASSET" + Cst.CrLf;
            SQLSelect += "when 'FUT' then en.EAR_DAY_FUT" + Cst.CrLf;
            SQLSelect += "when 'COM' then en.EAR_DAY_COM" + Cst.CrLf;
            SQLSelect += "when 'FX' then en.EAR_DAY_FX" + Cst.CrLf;
            SQLSelect += "when 'OTC' then en.EAR_DAY_OTC" + Cst.CrLf;
            SQLSelect += "when 'RISK' then en.EAR_DAY_RISK" + Cst.CrLf;
            SQLSelect += "when 'SEC' then en.EAR_DAY_SEC end as ISEARDAY" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EAREVENTENUM + " en" + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + "(en.CODE = @CONSTEVENTCODE)" + SQLCst.AND + "(en.VALUE = @EVENTCODE)" + Cst.CrLf;

            m_EarQuery.EventCode.Value = EventCodeFunc.DailyClosing;

            object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(pEfsTransaction.Cs), CommandType.Text, SQLSelect, m_EarQuery.GProduct, m_EarQuery.ConstEventCode, m_EarQuery.EventCode);
            if (null != obj)
                m_IsCLOEarDay = BoolFunc.IsTrue(obj);
        }

        /// <summary>
        /// Retourne true si l'événement est annulé:
        /// <para>- statut d'activation = DECATIV</para>
        /// <para>- présence d'un EventClass enfant avec code RMV</para>
        /// </summary>
        /// <param name="pIdE">Représente l'identifiant non significatif de l'évènement (IDE)</param>
        /// <returns></returns>
        /// <exception cref="SpheresException2 s'il n'existe pas "></exception>  
        private bool CheckEventRemoved(DataRow pRowEvent)
        {
            return CheckEventRemoved(pRowEvent, out _);
        }

        /// <summary>
        /// Retourne true si l'événement est annulé:
        /// <para>- statut d'activation = DECATIV</para>
        /// <para>- présence d'un EventClass enfant avec code RMV</para>
        /// </summary>
        /// <param name="pIdE">Représente l'identifiant non significatif de l'évènement (IDE)</param>
        /// <param name="pDtRemoved">Retourne la date d'annulation</param>
        /// <returns></returns>
        /// <exception cref="SpheresException2 s'il n'existe pas "></exception>  
        private bool CheckEventRemoved(int pIdE, out DateTime pDtRemoved)
        {
            DataRow rowEvent = m_EventsInfo.DtEvent.Select().Where(row => Convert.ToInt32(row["IDE"]) == pIdE).First();
            return CheckEventRemoved(rowEvent, out pDtRemoved);
        }

        /// <summary>
        /// Retourne true si l'événement est annulé:
        /// <para>- statut d'activation = DECATIV</para>
        /// <para>- présence d'un EventClass enfant avec code RMV</para>
        /// </summary>
        /// <param name="pRowEvent">Représente l'évènement</param>
        /// <param name="pDtRemoved">Retourne la date d'annulation</param>
        /// <returns></returns>
        /// <exception cref="SpheresException2 s'il n'existe pas "></exception>  
        private bool CheckEventRemoved(DataRow pRowEvent, out DateTime pDtRemoved)
        {
            pDtRemoved = DateTime.MinValue;
            bool isRemoved = (pRowEvent["IDSTACTIVATION"].ToString() == Cst.StatusActivation.DEACTIV.ToString());

            if (isRemoved)
            {
                IEnumerable<DataRow> rowsEventClass = pRowEvent.GetChildRows(m_EventsInfo.ChildEventClass);
                DataRow rowEventClassRMV = null;

                IEnumerable<DataRow> rowsEventClassRMV =
                    from row in rowsEventClass
                    where row["EVENTCLASS"].ToString() == EventClassFunc.RemoveEvent
                    select row;

                if (rowsEventClassRMV.Count() > 0)
                    rowEventClassRMV = rowsEventClassRMV.First();

                isRemoved = (null != rowEventClassRMV);

                if (isRemoved)
                {
                    pDtRemoved = Convert.ToDateTime(rowEventClassRMV["DTEVENT"]);
                }
                else
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-05385",
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                        m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second,
                        LogTools.IdentifierAndId(pRowEvent["EVENTCODE"].ToString() + "/" + pRowEvent["EVENTTYPE"].ToString(),
                            pRowEvent["IDE"].ToString()));
                }
            }

            return isRemoved;
        }

        #region GetPreviousEarToEarCommon
        /// <summary>
        /// Charger tous les EARDAY, du jour ou passés, qui sont flagués comme donnant lieu à des EARCOMMON, via la table EVENTENUM
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pIdB"></param>
        /// <param name="pDtEvent"></param>
        /// <param name="pOtcmlData"></param>
        /// <returns></returns>
        /// EG 20121227 Gestion Nouvelle colonne EAR_COMMON (Puissance de 2)
        /// FI 20170607 [23221] Modify
        protected Cst.ErrLevel GetPreviousEarToEarCommon(DataDbTransaction pEfsTransaction,
            int pIdB, DateTime pDtEvent, List<Pair<string, string>> pLstData)
        {
            try
            {
                // RD 20121024 [18201] Ne pas considérer les EAR annulés
                m_EarQuery.DtEvent.Value = pDtEvent;
                m_EarQuery.IdB.Value = pIdB;
                m_EarQuery.ExchangeType.Value = ExchangeTypeFunc.FlowCurrency;
                m_EarQuery.IdStActivation.Value = Cst.StatusActivation.REGULAR;
                m_EarQuery.GProduct.Value = m_TradeInfo.Product.GProduct;
                //
                #region Select previous EAR with "ISEARCOMMON = 1"
                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT + "ear.IDEAR, ear.DTEAR, ear.DTEVENT, ear.IDB, ed.INSTRUMENTNO, ed.STREAMNO," + Cst.CrLf);
                sqlSelect += "ed.IDEARDAY, ed.IDEC, ed.EARCODE, ed.EVENTCODE, ed.EVENTTYPE, ed.EVENTCLASS," + Cst.CrLf;
                sqlSelect += "ea.EXCHANGETYPE, ea.IDC, ea.PAID, ea.RECEIVED, ea.IDQUOTE_H, ec.IDE" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EAR + " ear" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAY + " ed" + SQLCst.ON + "(ed.IDEAR=ear.IDEAR)" + Cst.CrLf;
                /*
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTENUM + " en_code" + SQLCst.ON + "(en_code.CODE=@CONSTEVENTCODE)";
                sqlSelect += SQLCst.AND + "(en_code.VALUE=ed.EVENTCODE)" + Cst.CrLf;
                //
                sqlSelect += SQLCst.AND + "((en_code.EARCOMMON=@CONSTALL)";
                if (m_TradeInfo.IsETDContext)
                    sqlSelect += SQLCst.OR + "(en_code.EARCOMMON=@CONSTETD)";
                else if (m_TradeInfo.IsOTCContext)
                    sqlSelect += SQLCst.OR + "(en_code.EARCOMMON=@CONSTOTC)";
                sqlSelect += ")" + Cst.CrLf;
                //
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTENUM + " en_type" + SQLCst.ON + "(en_type.CODE=@CONSTEVENTTYPE)";
                sqlSelect += SQLCst.AND + "(en_type.VALUE=ed.EVENTTYPE)" + Cst.CrLf;
                //
                sqlSelect += SQLCst.AND + "((en_type.EARCOMMON=@CONSTALL)";
                if (m_TradeInfo.IsETDContext)
                    sqlSelect += SQLCst.OR + "(en_type.EARCOMMON=@CONSTETD)";
                else if (m_TradeInfo.IsOTCContext)
                    sqlSelect += SQLCst.OR + "(en_type.EARCOMMON=@CONSTOTC)";
                sqlSelect += ")" + Cst.CrLf;
                */

                // EG 20121227
                // FI 20170607 [23221] Prise en considération de GPRODUCT= 'COM'
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_EAREVENTENUM + " en_code" + SQLCst.ON + "(en_code.CODE = @CONSTEVENTCODE)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(en_code.VALUE=ed.EVENTCODE)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(case @GPRODUCT when 'ADM' then en_code.EAR_COMMON_ADM" + Cst.CrLf;
                sqlSelect += @"when 'ASSET' then en_code.EAR_COMMON_ASSET" + Cst.CrLf;
                sqlSelect += @"when 'FUT' then en_code.EAR_COMMON_FUT" + Cst.CrLf;
                sqlSelect += @"when 'COM' then en_code.EAR_COMMON_COM" + Cst.CrLf;
                sqlSelect += @"when 'FX' then en_code.EAR_COMMON_FX" + Cst.CrLf;
                sqlSelect += @"when 'OTC' then en_code.EAR_COMMON_OTC" + Cst.CrLf;
                sqlSelect += @"when 'RISK' then en_code.EAR_COMMON_RISK" + Cst.CrLf;
                sqlSelect += @"when 'SEC' then en_code.EAR_COMMON_SEC end = 1)" + Cst.CrLf;

                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_EAREVENTENUM + " en_type" + SQLCst.ON + "(en_type.CODE = @CONSTEVENTTYPE)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(en_type.VALUE=ed.EVENTTYPE)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(case @GPRODUCT when 'ADM' then en_type.EAR_COMMON_ADM" + Cst.CrLf;
                sqlSelect += @"when 'ASSET' then en_type.EAR_COMMON_ASSET" + Cst.CrLf;
                sqlSelect += @"when 'COM' then en_type.EAR_COMMON_COM" + Cst.CrLf;
                sqlSelect += @"when 'FUT' then en_type.EAR_COMMON_FUT" + Cst.CrLf;
                sqlSelect += @"when 'FX' then en_type.EAR_COMMON_FX" + Cst.CrLf;
                sqlSelect += @"when 'OTC' then en_type.EAR_COMMON_OTC" + Cst.CrLf;
                sqlSelect += @"when 'RISK' then en_type.EAR_COMMON_RISK" + Cst.CrLf;
                sqlSelect += @"when 'SEC' then en_type.EAR_COMMON_SEC end = 1)" + Cst.CrLf;


                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + SQLCst.ON + "(ec.IDEC=ed.IDEC)" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAYAMOUNT + " ea" + SQLCst.ON + "(ea.IDEARDAY=ed.IDEARDAY)" + Cst.CrLf;
                //
                sqlSelect += SQLCst.WHERE + "(ear.IDT=@IDT)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ear.IDB=@IDB)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ear.DTEVENT<=@DTEVENT)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ea.EXCHANGETYPE=@EXCHANGETYPE)";
                // Uniquement les EARs avec un statut d'activation REGULAR
                sqlSelect += SQLCst.AND + "(ear.IDSTACTIVATION=@IDSTACTIVATION)" + Cst.CrLf;
                // Uniquement les EARDAY issus d'événements non annulés. 
                // Un événement annulé contient un EvantClass RMV, 
                // et donne naissence à deux EARs, un avec un statut REGULAR et l'autre avec un statut REMOVED
                sqlSelect += SQLCst.AND + SQLCst.NOT_EXISTS_SELECT + Cst.OTCml_TBL.EARDAY + " ed1" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EAR + " ear1" + SQLCst.ON + "(ear1.IDEAR=ed1.IDEAR)" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "(ear1.IDT=ear.IDT) and (ear1.IDSTACTIVATION=@REMOVED) and (ear1.DTREMOVED is not null) and (ed1.IDEC=ec.IDEC))";

                DataSet dsEar = null;
                /*
                if (m_TradeInfo.IsETDContext)
                    dsEar = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text, sqlSelect.ToString(),
                        m_EarQuery.IdT, m_EarQuery.IdB, m_EarQuery.IdStActivation, m_EarQuery.DtEvent, m_EarQuery.ExchangeType, 
                        m_EarQuery.ConstRemoved, m_EarQuery.ConstEventCode, m_EarQuery.ConstEventType, m_EarQuery.ConstALL, 
                        m_EarQuery.ConstETD);
                else if (m_TradeInfo.IsOTCContext)
                    dsEar = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text, sqlSelect.ToString(),
                        m_EarQuery.IdT, m_EarQuery.IdB, m_EarQuery.IdStActivation, m_EarQuery.DtEvent, m_EarQuery.ExchangeType, 
                        m_EarQuery.ConstRemoved, m_EarQuery.ConstEventCode, m_EarQuery.ConstEventType, m_EarQuery.ConstALL, 
                        m_EarQuery.ConstOTC);
                else
                    dsEar = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text, sqlSelect.ToString(),
                        m_EarQuery.IdT, m_EarQuery.IdB, m_EarQuery.IdStActivation, m_EarQuery.DtEvent, m_EarQuery.ExchangeType,
                        m_EarQuery.ConstRemoved, m_EarQuery.ConstEventCode, m_EarQuery.ConstEventType, m_EarQuery.ConstALL);
                */
                // EG 20121227
                dsEar = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text, sqlSelect.ToString(),
                    m_EarQuery.IdT, m_EarQuery.IdB, m_EarQuery.IdStActivation, m_EarQuery.DtEvent, m_EarQuery.ExchangeType,
                    m_EarQuery.ConstRemoved, m_EarQuery.ConstEventCode, m_EarQuery.ConstEventType, m_EarQuery.GProduct);

                m_DtEarCommon = dsEar.Tables[0];
                #endregion

                return Cst.ErrLevel.SUCCESS;
            }
            catch (SpheresException2) { throw; }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, "SYS-05364",
                    new ProcessState(ProcessStateTools.StatusEnum.ERROR, ProcessStateTools.CodeReturnFailureEnum), ex,
                        pLstData.Find(match => match.First == "EAR").Second,
                        pLstData.Find(match => match.First == "DTEVENT").Second,
                        pLstData.Find(match => match.First == "IDSTACTIVATION").Second,
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                        m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second,
                        pLstData.Find(match => match.First == "BOOK").Second);
            }
        }
        #endregion GetPreviousEarToEarCommon
        #region GetPreviousEarCLOToEarCommon
        /// <summary>
        /// Charger tous les EARDAY passés, avec EARCODE='CLO'
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pIdB"></param>
        /// <param name="pDtEvent"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pEarDetLogMsg"></param>
        /// <returns></returns>
        protected Cst.ErrLevel GetPreviousEarCLOToEarCommon(DataDbTransaction pEfsTransaction, int pIdB, DateTime pDtEvent, int pInstrumentNo, int pStreamNo)
        {
            // RD 20121024 [18201] Ne pas considérer les EAR annulés
            if (m_IsCLOEarDay)
            {
                m_EarQuery.EventCode.Value = EventCodeFunc.DailyClosing;
                m_EarQuery.IdT.Value = CurrentId;
                m_EarQuery.IdB.Value = pIdB;
                m_EarQuery.IdStActivation.Value = Cst.StatusActivation.REGULAR;
                m_EarQuery.DtEvent.Value = pDtEvent;
                m_EarQuery.InstrumentNo.Value = pInstrumentNo;
                m_EarQuery.StreamNo.Value = pStreamNo;
                m_EarQuery.ExchangeType.Value = ExchangeTypeFunc.FlowCurrency;

                #region Select previous Daily Closing EAR (CLO)
                StrBuilder sqlSelect = new StrBuilder();
                //
                //Query Fils: Retourne tous les flux d'arrêtés antérieurs (CLO), dont l'événement PARENT a sa date de flux égale à DTEVENT.
                //         On remonte ainsi les derniers CLO ENFANTS de l'événement disposant d'une date de flux égale à la date traitée.
                //
                //         Cela permet de remonter entre autre:
                //           - le dernier arrêté (CLO/AIN) du coupon dont on traite actuellement "le" règlement (INT/INT):
                //             ex.: Coupon du 25/12 au 25/02, le 25/02 lors du traitement de INT/INT en date du 25/02, on remonte le CLO/AIN 
                //                  en date du 31/01 enfant de cet INT/INT en date du 25/02
                //
                #region Query Fils
                sqlSelect += SQLCst.SELECT_DISTINCT + "ear.IDEAR,ear.DTEAR,ear.DTEVENT as DTEVENT,ear.IDB," + Cst.CrLf;
                sqlSelect += "earday.INSTRUMENTNO,earday.STREAMNO,earday.IDEARDAY,earday.IDEC," + Cst.CrLf;
                sqlSelect += "earday.EVENTCODE as EVENTCODE,earday.EVENTTYPE as EVENTTYPE,earday.EVENTCLASS as EVENTCLASS," + Cst.CrLf;
                sqlSelect += "eardayamount.EXCHANGETYPE,eardayamount.IDC,eardayamount.PAID,eardayamount.RECEIVED,eardayamount.IDQUOTE_H," + Cst.CrLf;
                sqlSelect += "ec_child.IDE" + Cst.CrLf;
                //
                // Tous les Events :
                //    1- du même StreamNO, 
                //    2- du même InstrumentNO
                //    3- ayant un flux jour (en date: @DTEVENT)
                //
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(ec.IDE=ev.IDE) and (ec.DTEVENT=@DTEVENT)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ev.INSTRUMENTNO=@INSTRUMENTNO) and (ev.STREAMNO=@STREAMNO)" + Cst.CrLf;
                //
                // Tous les Events :
                //    1- FILS
                //    2- 'CLO'
                //    3- Anciens
                //
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev_child" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(ev_child.IDE_EVENT=ev.IDE) and (ev_child.EVENTCODE=@EVENTCODE)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ev_child.IDT=@IDT)" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec_child" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(ec_child.IDE=ev_child.IDE) and (ec_child.DTEVENT<@DTEVENT)" + Cst.CrLf;
                //
                // Tous les flux EARDAY ( donc les CLO) des EARs :
                //     1- du même Trade, 
                //     2- du même Book
                //
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAY + " earday" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(earday.IDEC=ec_child.IDEC)" + Cst.CrLf;
                sqlSelect += OTCmlHelper.GetSQLJoin(Cs, Cst.OTCml_TBL.EAR, true, "earday.IDEAR", "ear") + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ear.IDT=@IDT)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ear.IDB=@IDB)" + Cst.CrLf;
                //
                // Uniquement les EARs avec un statut d'activation REGULAR
                sqlSelect += SQLCst.AND + "(ear.IDSTACTIVATION=@IDSTACTIVATION)" + Cst.CrLf;
                //
                // Uniquement les EARDAY issus d'événements non annulés. 
                // Un événement annulé contient un EvantClass RMV, 
                // et donne naissence à deux EARs, un avec un statut REGULAR et l'autre avec un statut REMOVED
                sqlSelect += SQLCst.AND + SQLCst.NOT_EXISTS_SELECT + Cst.OTCml_TBL.EARDAY + " ed1" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EAR + " ear1" + SQLCst.ON + "(ear1.IDEAR=ed1.IDEAR)" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "(ear1.IDT=ear.IDT) and (ear1.IDSTACTIVATION=@REMOVED) and (ear1.DTREMOVED is not null) and (ed1.IDEC=ec_child.IDEC))";

                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAYAMOUNT + " eardayamount" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(eardayamount.IDEARDAY=earday.IDEARDAY)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(eardayamount.EXCHANGETYPE=@EXCHANGETYPE)" + Cst.CrLf;
                //
                sqlSelect += SQLCst.WHERE + "(ev.IDT=@IDT)" + Cst.CrLf;
                //
                #endregion
                //
                //Query Freres: Retourne tous les flux d'arrêtés antérieurs (CLO), dont un événement FRERE a sa date de flux égale à DTEVENT.
                //         On remonte ainsi les derniers CLO FRERES de l'événement disposant d'une date de flux égale à la date traitée.
                //
                //         Cela permet de remonter entre autre:
                //           - le dernier arrêté Mark to Market(CLO/MTM), si je suis entrain de traiter "le" règlement (INT/INT), 
                //             en supposant que les événements INT/INT et CLO/MTM sont enfants de STREAM (donc FRERE)
                //
                //             ex.: Coupon du 25/12 au 25/02, le 25/02 lors du traitement de INT/INT en date du 25/02, on remonte le CLO/MTM 
                //                  en date du dd/MM enfant du STREAM en cours
                //
                #region Query Freres

                sqlSelect += SQLCst.UNIONALL;
                //
                sqlSelect += SQLCst.SELECT_DISTINCT + "ear.IDEAR,ear.DTEAR,ear.DTEVENT as DTEVENT,ear.IDB," + Cst.CrLf;
                sqlSelect += "earday.INSTRUMENTNO,earday.STREAMNO,earday.IDEARDAY,earday.IDEC," + Cst.CrLf;
                sqlSelect += "earday.EVENTCODE as EVENTCODE,earday.EVENTTYPE as EVENTTYPE,earday.EVENTCLASS as EVENTCLASS," + Cst.CrLf;
                sqlSelect += "eardayamount.EXCHANGETYPE,eardayamount.IDC,eardayamount.PAID,eardayamount.RECEIVED,eardayamount.IDQUOTE_H," + Cst.CrLf;
                sqlSelect += "ec_brother.IDE" + Cst.CrLf;
                //
                // Tous les Events :
                //    1- du même StreamNO, 
                //    2- du même InstrumentNO
                //    3- ayant un flux jour (en date: @DTEVENT)
                //
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(ec.IDE=ev.IDE) and (ec.DTEVENT=@DTEVENT)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ev.INSTRUMENTNO=@INSTRUMENTNO) and (ev.STREAMNO=@STREAMNO)" + Cst.CrLf;
                //
                // Tous les Events :
                //    1- FRERES
                //    2- 'CLO'
                //    3- Anciens
                //
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev_brother" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(ev_brother.IDE_EVENT=ev.IDE_EVENT) and (ev_brother.EVENTCODE=@EVENTCODE) and not(ev_brother.IDE=ev.IDE)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ev_brother.IDT=@IDT)" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec_brother" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(ec_brother.IDE=ev_brother.IDE) and (ec_brother.DTEVENT< @DTEVENT)" + Cst.CrLf;
                //
                // Tous les flux EARDAY ( donc les CLO) des EARs :
                //     1- du même Trade, 
                //     2- du même Book
                //
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAY + " earday" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(earday.IDEC=ec_brother.IDEC)" + Cst.CrLf;
                sqlSelect += OTCmlHelper.GetSQLJoin(Cs, Cst.OTCml_TBL.EAR, true, "earday.IDEAR", "ear");
                sqlSelect += SQLCst.AND + "(ear.IDT=@IDT)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(ear.IDB=@IDB)" + Cst.CrLf;
                //
                // Uniquement les EARs avec un statut d'activation REGULAR
                sqlSelect += SQLCst.AND + "(ear.IDSTACTIVATION=@IDSTACTIVATION)" + Cst.CrLf;
                //
                // Uniquement les EARDAY issus d'événements non annulés. 
                // Un événement annulé contient un EvantClass RMV, 
                // et donne naissence à deux EARs, un avec un statut REGULAR et l'autre avec un statut REMOVED
                sqlSelect += SQLCst.AND + SQLCst.NOT_EXISTS_SELECT + Cst.OTCml_TBL.EARDAY + " ed1" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EAR + " ear1" + SQLCst.ON + "(ear1.IDEAR=ed1.IDEAR)" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "(ear1.IDT=ear.IDT) and (ear1.IDSTACTIVATION=@REMOVED) and (ear1.DTREMOVED is not null) and (ed1.IDEC=ec_brother.IDEC))";

                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAYAMOUNT + " eardayamount" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "(eardayamount.IDEARDAY=earday.IDEARDAY)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(eardayamount.EXCHANGETYPE=@EXCHANGETYPE)" + Cst.CrLf;
                //
                sqlSelect += SQLCst.WHERE + "(ev.IDT=@IDT)" + Cst.CrLf;
                //
                #endregion
                // 
                //Query Query ONCLE ( et ONCLE des ANCETRES): Retourne tous les flux d'arrêtés antérieurs (CLO), dont un événement NEVEU a sa date de flux égale à DTEVENT.
                //         On remonte ainsi les derniers CLO ONCLES de l'événement disposant d'une date de flux égale à la date traitée.
                //
                //         Cela permet de remonter entre autre:
                //           - le dernier arrêté Mark to Market(CLO/MTM), si je suis entrain de traiter un Réescompte (CLO/AIN) du coupon en cours (INT/INT), 
                //             en sachant que CLO/AIN est enfant de INT/INT et en supposant que les événements INT/INT et CLO/MTM sont enfants de STREAM (donc FRERE)
                //
                //             ex.: Coupon du 25/12 au 25/02, le 31/01 lors du traitement du réescompte CLO/AIN en date du 31/01, on remonte le CLO/MTM 
                //                  en date du dd/MM enfant du STREAM en cours
                //
                #region Query ONCLE ( et ONCLE des ANCETRES)
                const int deepTree = 4;
                //
                // deepTree représente le nombre de parent sur lesquels il faudrait remonter
                //
                // 0: aucun, ainsi ou aura pas les CLO-1 du PERE de l'event
                // 1: on aura seulement les CLO-1 FRERES du PERE (Donc ONCLE)
                // 2: on aura et le cas 1, et les CLO-1 FRERE du GRAND PERE
                // 3: on aura et le cas 2, et les CLO-1 FRERE de L'ARRIERE GRAND PERE
                // ....
                for (int parentNO = 1; parentNO <= deepTree; parentNO++)
                {
                    sqlSelect += SQLCst.UNIONALL;

                    sqlSelect += SQLCst.SELECT_DISTINCT + "ear.IDEAR,ear.DTEAR,ear.DTEVENT as DTEVENT,ear.IDB," + Cst.CrLf;
                    sqlSelect += "earday.INSTRUMENTNO,earday.STREAMNO,earday.IDEARDAY,earday.IDEC," + Cst.CrLf;
                    sqlSelect += "earday.EVENTCODE as EVENTCODE,earday.EVENTTYPE as EVENTTYPE,earday.EVENTCLASS as EVENTCLASS," + Cst.CrLf;
                    sqlSelect += "eardayamount.EXCHANGETYPE,eardayamount.IDC,eardayamount.PAID,eardayamount.RECEIVED,eardayamount.IDQUOTE_H," + Cst.CrLf;
                    sqlSelect += "ec_oncle.IDE" + Cst.CrLf;
                    //
                    // Tous les Events :
                    //    1- du même StreamNO, 
                    //    2- du même InstrumentNO
                    //    3- ayant un flux jour (en date: @DTEVENT)
                    //
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + Cst.CrLf;
                    sqlSelect += SQLCst.ON + "(ec.IDE=ev.IDE) and (ec.DTEVENT=@DTEVENT)" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "(ev.INSTRUMENTNO=@INSTRUMENTNO) and (ev.STREAMNO=@STREAMNO)" + Cst.CrLf;
                    //
                    // Tous les Events :
                    //    1- ONCLE ( et ONCLE des ANCETRES)
                    //    2- 'CLO'
                    //    3- Anciens
                    //
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev_father0" + Cst.CrLf;
                    sqlSelect += SQLCst.ON + "(ev_father0.IDE=ev.IDE_EVENT)" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "(ev_father0.IDT=@IDT)" + Cst.CrLf;
                    //
                    for (int j = 1; j < parentNO; j++)
                    {
                        sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev_father" + j.ToString() + Cst.CrLf;
                        sqlSelect += SQLCst.ON + "(ev_father" + j.ToString() + ".IDE=ev_father" + ((int)(j - 1)).ToString() + ".IDE_EVENT)" + Cst.CrLf;
                        //SQLEar += SQLCst.AND + "(ev_father" + j.ToString() + ".IDT = @IDT)" + Cst.CrLf;
                    }
                    //
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev_oncle" + Cst.CrLf;
                    sqlSelect += SQLCst.ON + "(ev_oncle.IDE_EVENT=ev_father" + ((int)(parentNO - 1)).ToString() + ".IDE_EVENT)" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "not(ev_oncle.IDE=ev_father" + ((int)(parentNO - 1)).ToString() + ".IDE)" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "(ev_oncle.EVENTCODE=@EVENTCODE)" + Cst.CrLf;
                    //SQLEar += SQLCst.AND + "(ev_oncle.IDT = @IDT)" + Cst.CrLf;
                    //
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec_oncle" + Cst.CrLf;
                    sqlSelect += SQLCst.ON + "(ec_oncle.IDE=ev_oncle.IDE)" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "(ec_oncle.DTEVENT<@DTEVENT)" + Cst.CrLf;
                    //
                    // Tous les flux EARDAY ( donc les CLO) des EARs :
                    //     1- du même Trade, 
                    //     2- du même Book
                    //
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAY + " earday" + Cst.CrLf;
                    sqlSelect += SQLCst.ON + "(earday.IDEC = ec_oncle.IDEC)" + Cst.CrLf;
                    sqlSelect += OTCmlHelper.GetSQLJoin(Cs, Cst.OTCml_TBL.EAR, true, "earday.IDEAR", "ear");
                    sqlSelect += SQLCst.AND + "(ear.IDT=@IDT)" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "(ear.IDB=@IDB)" + Cst.CrLf;
                    //
                    // Uniquement les EARs avec un statut d'activation REGULAR
                    sqlSelect += SQLCst.AND + "(ear.IDSTACTIVATION=@IDSTACTIVATION)" + Cst.CrLf;
                    //
                    // Uniquement les EARDAY issus d'événements non annulés. 
                    // Un événement annulé contient un EvantClass RMV, 
                    // et donne naissence à deux EARs, un avec un statut REGULAR et l'autre avec un statut REMOVED
                    sqlSelect += SQLCst.AND + SQLCst.NOT_EXISTS_SELECT + Cst.OTCml_TBL.EARDAY + " ed1" + Cst.CrLf;
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EAR + " ear1" + SQLCst.ON + "(ear1.IDEAR=ed1.IDEAR)" + Cst.CrLf;
                    sqlSelect += SQLCst.WHERE + "(ear1.IDT=ear.IDT) and (ear1.IDSTACTIVATION=@REMOVED) and (ear1.DTREMOVED is not null) and (ed1.IDEC=ec_oncle.IDEC))";

                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAYAMOUNT + " eardayamount" + Cst.CrLf;
                    sqlSelect += SQLCst.ON + "(eardayamount.IDEARDAY=earday.IDEARDAY)" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "(eardayamount.EXCHANGETYPE=@EXCHANGETYPE)" + Cst.CrLf;
                    //
                    sqlSelect += SQLCst.WHERE + "(ev.IDT=@IDT)" + Cst.CrLf;
                }
                #endregion

                sqlSelect += SQLCst.ORDERBY + "EVENTTYPE, EVENTCLASS, DTEVENT" + SQLCst.DESC;
                #endregion

                DataSet dsEar = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text, sqlSelect.ToString(),
                    m_EarQuery.IdT, m_EarQuery.IdB, m_EarQuery.IdStActivation, m_EarQuery.DtEvent, m_EarQuery.ExchangeType,
                    m_EarQuery.EventCode, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo,
                    m_EarQuery.ConstRemoved);

                m_DtEarCLO = dsEar.Tables[0];
            }

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion GetPreviousEarCLOToEarCommon
        #region GetPreviousEarLPCToEarCommon
        /// <summary>
        /// Charger tous les EARDAY avec EARCODE='LPC', du Trade précédent, et ceci uniquement pour un Trade Cash-Balance
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pIdB"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamCurrency"></param>
        /// <param name="pLstData"></param>
        /// <returns></returns>
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        protected Cst.ErrLevel GetPreviousEarLPCToEarCommon(DataDbTransaction pEfsTransaction, int pIdB, int pInstrumentNo, int pStreamNo, string pStreamFCU)
        {
            // RD 20121024 [18201] Ne pas considérer les EAR annulés
            // RD 20121121 [17953] Considérer tous les flux précédents dans la même devise, 
            // quelque soit le StreamNo, car sur un trade CashBalance chaque Stream correspond à une seule devise
            // Et pour éviter le problème en cas ou une devise donnée n'a pas le même StreamNO sur le trade jour et sur le trade veille
            IDbDataParameter paramLink = new DataParameter(Cs, "LINK", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN) { Value = EFS.TradeLink.TradeLinkType.PrevCashBalance }.DbDataParameter;
            IDbDataParameter paramEventTypeCSB = new DataParameter(Cs, "EVENTTYPE_CSB", DbType.AnsiString, SQLCst.UT_EVENT_LEN) { Value = "CSB" }.DbDataParameter;
            IDbDataParameter paramEventTypePCB = new DataParameter(Cs, "EVENTTYPE_PCB", DbType.AnsiString, SQLCst.UT_EVENT_LEN) { Value = "PCB" }.DbDataParameter;

            m_EarQuery.EventCode.Value = EventCodeFunc.LinkedProductClosing;
            m_EarQuery.IdT.Value = CurrentId;
            m_EarQuery.IdB.Value = pIdB;
            m_EarQuery.InstrumentNo.Value = pInstrumentNo;
            m_EarQuery.StreamNo.Value = pStreamNo;
            m_EarQuery.IdC.Value = pStreamFCU;
            m_EarQuery.ExchangeType.Value = ExchangeTypeFunc.FlowCurrency;
            m_EarQuery.IdStActivation.Value = Cst.StatusActivation.REGULAR;

            #region Select previous Linked to Product Closing EAR (LPC)
            StrBuilder sqlSelect = new StrBuilder();

            sqlSelect += SQLCst.SELECT_DISTINCT + "ear.IDEAR,ear.DTEAR,ear.DTEVENT as DTEVENT,ear.IDB," + Cst.CrLf;
            sqlSelect += "earday.INSTRUMENTNO,@STREAMNO as STREAMNO,earday.IDEARDAY,earday.IDEC," + Cst.CrLf;
            sqlSelect += "earday.EVENTCODE,earday.EVENTTYPE,earday.EVENTCLASS," + Cst.CrLf;
            sqlSelect += "eardayamount.EXCHANGETYPE,eardayamount.IDC,eardayamount.PAID,eardayamount.RECEIVED," + Cst.CrLf;
            sqlSelect += "eardayamount.IDQUOTE_H,ec.IDE" + Cst.CrLf;

            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " t" + Cst.CrLf;

            // Trade précédent via TRADELINK
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADELINK + " tl";
            sqlSelect += SQLCst.ON + "(tl.IDT_A=@IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(tl.IDT_B=t.IDT) and (tl.LINK=@LINK)" + Cst.CrLf;

            // Uniquement si LPC est flagué comme étant EARDAY
            /*
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTENUM + " en";
            sqlSelect += SQLCst.ON + "(en.CODE=@CONSTEVENTCODE)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(en.VALUE=@EVENTCODE) and (en.ISEARDAY=@CONSTTRUE)" + Cst.CrLf;
            */

            // EG 20121227
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_EAREVENTENUM + " en" + SQLCst.ON + "(en.CODE = @CONSTEVENTCODE)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(en.VALUE = @EVENTCODE) and (en.EAR_DAY_RISK=@CONSTTRUE)" + Cst.CrLf;

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EAR + " ear";
            sqlSelect += SQLCst.ON + "(ear.IDT=t.IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ear.IDB=@IDB) and (ear.DTEVENT=t.DTBUSINESS)" + Cst.CrLf;
            // Uniquement les EARs avec un statut d'activation REGULAR
            sqlSelect += SQLCst.AND + "(ear.IDSTACTIVATION=@IDSTACTIVATION)" + Cst.CrLf;

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAY + " earday";
            sqlSelect += SQLCst.ON + "(earday.IDEAR=ear.IDEAR)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(earday.EVENTCODE=@EVENTCODE) and (earday.INSTRUMENTNO=@INSTRUMENTNO)" + Cst.CrLf;
            // Pour ne pas charger l'event CSB et PCB:
            // CSB :  car il existe un event à part entière qui repérsente le Solde précédent, à savoir PCB (Previous cash Balance)
            // PCB :  pour ne pas charger le Solde précédent du trade précédent
            sqlSelect += SQLCst.AND + "(earday.EVENTTYPE not in (@EVENTTYPE_CSB, @EVENTTYPE_PCB))" + Cst.CrLf;

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EARDAYAMOUNT + " eardayamount";
            sqlSelect += SQLCst.ON + "(eardayamount.IDEARDAY=earday.IDEARDAY)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(eardayamount.EXCHANGETYPE=@EXCHANGETYPE)";
            // Considérer tous les flux précédents dans la même devise, 
            sqlSelect += SQLCst.AND + "(eardayamount.IDC=@IDC)" + Cst.CrLf;

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec";
            sqlSelect += SQLCst.ON + "(ec.IDEC=earday.IDEC)" + Cst.CrLf;

            // Uniquement les EARDAY issus d'événements non annulés. 
            // Un événement annulé contient un EvantClass RMV, 
            // et donne naissence à deux EARs, un avec un statut REGULAR et l'autre avec un statut REMOVED
            // Attention:
            //  Actuellment on ne peut pas annuler un trade CashBalance, pour des raisons de perfs, je met le code suivant en commentaire:
            //sqlSelect += SQLCst.WHERE + SQLCst.NOT_EXISTS_SELECT + Cst.OTCml_TBL.EARDAY + " ed1" + Cst.CrLf;
            //sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EAR + " ear1" + SQLCst.ON + "(ear1.IDEAR=ed1.IDEAR)" + Cst.CrLf;
            //sqlSelect += SQLCst.WHERE + "(ear1.IDT=ear.IDT) and (ear1.IDSTACTIVATION=@REMOVED) and (ear1.DTREMOVED is not null) and (ed1.IDEC=ec.IDEC))";

            sqlSelect += SQLCst.ORDERBY + "EVENTTYPE, EVENTCLASS, DTEVENT" + SQLCst.DESC;
            #endregion

            DataSet dsEar = DataHelper.ExecuteDataset(pEfsTransaction, CommandType.Text, sqlSelect.ToString(),
                m_EarQuery.IdT, m_EarQuery.IdB, m_EarQuery.IdStActivation, m_EarQuery.DtEvent,
                m_EarQuery.ExchangeType, m_EarQuery.EventCode, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo, m_EarQuery.IdC,
                paramLink, m_EarQuery.ConstRemoved, m_EarQuery.ConstEventCode, m_EarQuery.ConstTrue,
                paramEventTypeCSB, paramEventTypePCB);

            m_DtEarLPC = dsEar.Tables[0];

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion GetPreviousEarCLOToEarCommon

        #region GetIRDCurrentNominal
        public DataRow[] GetIRDCurrentNominal(DateTime pDate)
        {
            ArrayList aIRDNominals = new ArrayList();
            DataRow[] rowsIRDNominal = GetRowIRDNominal();
            foreach (DataRow rowNominal in rowsIRDNominal)
            {
                if ((Convert.ToDateTime(rowNominal["DTSTARTADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowNominal["DTENDADJ"])))
                    aIRDNominals.Add(rowNominal);
                else if ((Convert.ToDateTime(rowNominal["DTSTARTUNADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowNominal["DTENDUNADJ"])))
                    aIRDNominals.Add(rowNominal);
            }
            //
            if (aIRDNominals.Count == 0)
            {
                int nbRow = rowsIRDNominal.Length;
                if (0 < nbRow)
                {
                    DataRow rowTerminationNominal = rowsIRDNominal[nbRow - 1];
                    if (pDate == Convert.ToDateTime(rowTerminationNominal["DTENDADJ"]))
                        aIRDNominals.Add(rowTerminationNominal);
                }
            }
            DataRow[] drIRDNominals = (DataRow[])aIRDNominals.ToArray(typeof(DataRow));
            return drIRDNominals;
        }
        public DataRow[] GetIRDCurrentNominal(DateTime pDate, DateTime pDateUnadjusted)
        {
            DataRow[] rowNominal = GetIRDCurrentNominal(pDate);
            if (null == rowNominal)
                rowNominal = GetIRDCurrentNominal(pDateUnadjusted);
            return rowNominal;
        }
        #endregion GetIRDCurrentNominal
        #region GetFxCurrentNominal
        public DataRow[] GetFxCurrentNominal()
        {
            ArrayList aFxNominals = new ArrayList();
            DataRow[] rowsFxStartNominal = GetRowFxStartNominal();
            foreach (DataRow rowNominal in rowsFxStartNominal)
            {
                DataRow rowRECEventClass = GetRowEventClass(rowNominal, EventClassFunc.Recognition);
                if (rowRECEventClass != null)
                {
                    if (Convert.ToDateTime(rowRECEventClass["DTEVENT"]) == m_TransactDate)
                        aFxNominals.Add(rowNominal);
                }
            }
            //
            DataRow[] drFxNominals = (DataRow[])aFxNominals.ToArray(typeof(DataRow));
            //
            return drFxNominals;
        }
        #endregion GetFxCurrentNominal
        #region AddEarTypeEvent
        public static void AddEarTypeEvent(ref int[] pEarTypeEvent, int pIde)
        {
            ArrayList aEarEvent = new ArrayList();
            //
            if (ArrFunc.IsFilled(pEarTypeEvent))
            {
                foreach (int ide in pEarTypeEvent)
                    aEarEvent.Add(ide);
            }
            //
            aEarEvent.Add(pIde);
            //
            pEarTypeEvent = (int[])aEarEvent.ToArray(typeof(int));
        }
        #endregion AddEarTypeEvent

        #region GetNominalAmount
        public decimal GetNominalAmount(DataRow pRowNominal, DateTime pDtAccount, ref int[] pEarNomEvent)
        {
            decimal nominalAmount = Convert.ToDecimal(pRowNominal["VALORISATION"]);
            AddEarTypeEvent(ref pEarNomEvent, Convert.ToInt32(pRowNominal["IDE"]));

            if (m_TradeInfo.Product.IsFx)
            {
                DataRow[] drFxTerNominals = GetRowFxIntTerNominal(pRowNominal["EVENTTYPE"].ToString());

                foreach (DataRow rowTerNominal in drFxTerNominals)
                {
                    DataRow rowRECEventClass = GetRowEventClass(rowTerNominal, EventClassFunc.Recognition);

                    if (rowRECEventClass != null)
                    {
                        if (Convert.ToDateTime(rowRECEventClass["DTEVENT"]) <= pDtAccount)
                        {
                            nominalAmount -= Convert.ToDecimal(rowTerNominal["VALORISATION"]);
                            AddEarTypeEvent(ref pEarNomEvent, Convert.ToInt32(rowTerNominal["IDE"]));
                        }
                    }
                }
            }

            return nominalAmount;
        }
        #endregion GetNominalAmount

        #region GetRowIRDNominal
        public DataRow[] GetRowIRDNominal()
        {
            return m_EventsInfo.DtEvent.Select("EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.NominalStep) +
                " and INSTRUMENTNO=" + m_EarQuery.InstrumentNo.Value.ToString() +
                " and STREAMNO=" + m_EarQuery.StreamNo.Value.ToString());
        }
        #endregion GetRowIRDNominal
        #region GetRowFxStartNominal
        public DataRow[] GetRowFxStartNominal()
        {
            return m_EventsInfo.DtEvent.Select("EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.Start) +
                " and ( EVENTTYPE=" + DataHelper.SQLString(EventTypeFunc.CallCurrency) +
                " or EVENTTYPE=" + DataHelper.SQLString(EventTypeFunc.PutCurrency) +
                "  ) and INSTRUMENTNO=" + m_EarQuery.InstrumentNo.Value.ToString() +
                " and STREAMNO=" + m_EarQuery.StreamNo.Value.ToString());
        }
        #endregion GetRowFxStartNominal

        #region GetRowFxIntTerNominal
        public DataRow[] GetRowFxIntTerNominal(string pEventType)
        {
            return m_EventsInfo.DtEvent.Select("( EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.Termination) +
                " or EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.Intermediary) + " ) " +
                " and EVENTTYPE=" + DataHelper.SQLString(pEventType) +
                " and INSTRUMENTNO=" + m_EarQuery.InstrumentNo.Value.ToString() +
                " and STREAMNO=" + m_EarQuery.StreamNo.Value.ToString());
        }
        #endregion GetRowFxIntTerNominal
        //
        #region GetRowEventClass
        public DataRow GetRowEventClass(DataRow pRowEvent, string pEventClass)
        {
            DataRow[] rowEventClass = pRowEvent.GetChildRows(m_EventsInfo.ChildEventClass);
            foreach (DataRow item in rowEventClass)
            {
                if (item["EVENTCLASS"].ToString() == pEventClass)
                    return item;
            }
            return null;
        }
        #endregion GetRowEventClass
        #region GetEarCode
        /// <summary>
        /// Classe donnant une valeur à EARCODE éventuellement différente de EVENTCODE
        /// <para>Par exemple, pour les évènements d'ABANDON/EXERCISE/OUT 'TER' CCU/PCU/PAO/REB où EVENTCODE vaut 'TER' et EARCODE vaudra ABN/EXE/OUT</para>
        /// </summary>
        // RD 20140910 [20336] Gérer les deux cas suivants:
        // - Assignation: Manuelle et Automatique
        // - Exercice/Abandon: Automatique
        // PL 20180315 Also manage Exercise of Optional Early Termination. Necessary for FX Options.
        // EG 20180514 [23812] Report 
        private string GetEarCode(DataRow pRow, List<Pair<string, string>> pLstData)
        {
            try
            {
                string earCode = pRow["EVENTCODE"].ToString();
                DataRow rowParent = pRow.GetParentRow(m_EventsInfo.ChildEvent);
                if (null != rowParent)
                {
                    string eventCode = rowParent["EVENTCODE"].ToString();
                    if (EventCodeFunc.IsAbandon(eventCode) || EventCodeFunc.IsAutomaticAbandon(eventCode))
                        earCode = EventCodeFunc.Abandon;
                    else if (EventCodeFunc.IsExercise(eventCode) || EventCodeFunc.IsAutomaticExercise(eventCode))
                        earCode = EventCodeFunc.Exercise;
                    else if (EventCodeFunc.IsAssignment(eventCode) || EventCodeFunc.IsAutomaticAssignment(eventCode))
                        earCode = EventCodeFunc.Assignment;
                    else if (EventCodeFunc.IsOut(eventCode))
                        earCode = EventCodeFunc.Out;
                    // EG 20180514 [23812] Report 
                    else if (EventCodeFunc.IsExerciseOptionalEarlyTermination(eventCode))
                        earCode = EventCodeFunc.ExerciseOptionalEarlyTermination;
                }
                return earCode;
            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05360",
                    new ProcessState(ProcessStateTools.StatusEnum.ERROR, ProcessStateTools.CodeReturnFailureEnum), ex,
                    m_LstMasterData.Find(match => match.First == "TRADE").Second,
                    m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                    m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second,
                    pLstData.Find(match => match.First == "PAYREC").Second,
                    pLstData.Find(match => match.First == "EVENT").Second,
                    pLstData.Find(match => match.First == "INSTRSTREAMNO").Second,
                    pLstData.Find(match => match.First == "BOOK").Second);
            }
        }
        #endregion GetEarCode
        #region GetEventsToEarDay
        /// <summary>
        /// Collecter la liste des événements du trade, candidats à la génération des EAR du jour (EARDAY)
        /// <para>ALGORITHME</para>
        /// <para>- Evénement du jour et EAR déjà généré (quelque soit le statut)</para>
        /// <para>  . Supprimer les EARs déjà générés</para>
        /// <para>  . Considérer comme candidats à Earday tous les événements issus de l'Ear supprimé et non annulé (Removed)</para>
        /// <para>- Evénement du jour et Evénement non annulé:</para>
        /// <para>  . Re-Générer l'EAR</para>
        /// <para>- Evénement du jour et Evénement annulé</para>
        /// <para>  . Ne pas généré l'EAR d'un Evénement annulé</para>
        /// 
        /// <para>- Evénement passé, EAR déjà généré, Evénement non annulé</para>
        /// <para>  . Ne pas ré-généré l'EAR d'un Evénement passé non annulé</para>
        /// <para>- Evénement passé, EAR déjà généré, Evénement annulé</para>
        /// <para>  . Générer l'EAR Removed</para>
        /// <para>- Evénement passé, EAR non généré, Evénement non annulé</para>
        /// <para>  . Générer l'EAR</para>
        /// <para>- Evénement passé, EAR non généré, Evénement annulé</para>
        /// <para>  . Ne pas généré l'EAR Romeved d'un Evénement annulé sans EAR</para>
        /// </summary>
        /// <param name="pIsEventToEarDayFilled"></param>
        /// <returns></returns>
        // EG 20121227 Gestion Nouvelle colonne EAR_DAY (Puissance de 2)
        // EG 20180606 [23979] IRQ (EARGEN)
        protected Cst.ErrLevel SetEventsCandidateToEarDay()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            DataTable dtEventToEarDay = m_EventsInfo.DtEvent.Clone();
            dtEventToEarDay.TableName = "EventToEarDay";

            DataTable dtEventClassToEarDay = m_EventsInfo.DtEventClass.Clone();
            dtEventClassToEarDay.TableName = "EventClassToEarDay";

            // RD 20121026 [18201] Gestion des événements annulés
            // EG 20121227 Gestion Nouvelle colonne EARDAY (Puissance de 2)
            IEnumerable<DataRow> rowsEventToEarDay = null;
            if (m_EarFlow.ToUpper() == Cst.FlowTypeEnum.CLOSING.ToString().ToUpper())
            {
                rowsEventToEarDay = m_EventsInfo.DtEvent.Select()
                    .Where(row => BoolFunc.IsTrue(row["ISEARDAY"]) && row["EVENTCODE"].ToString() == EventCodeFunc.DailyClosing);
            }
            else if (m_EarFlow.ToUpper() == Cst.FlowTypeEnum.CASH_FLOWS.ToString().ToUpper())
            {
                rowsEventToEarDay = m_EventsInfo.DtEvent.Select()
                    .Where(row => BoolFunc.IsTrue(row["ISEARDAY"]) && row["EVENTCODE"].ToString() != EventCodeFunc.DailyClosing);
            }
            else
            {
                rowsEventToEarDay = m_EventsInfo.DtEvent.Select()
                    .Where(row => BoolFunc.IsTrue(row["ISEARDAY"]));
            }


            List<int> lstDeletedEar = new List<int>();


            foreach (DataRow rowEvent in rowsEventToEarDay)
            {
                if (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn))
                    break;

                #region Check event by event
                bool isEventRemoved = CheckEventRemoved(rowEvent);
                int idCurrentEvent = Convert.ToInt32(rowEvent["IDE"]);

                foreach (DataRow rowEventClass in rowEvent.GetChildRows(m_EventsInfo.ChildEventClass)
                    .Where(row => BoolFunc.IsTrue(row["ISEARDAY"])))
                {
                    int idCurrentEventClass = Convert.ToInt32(rowEventClass["IDEC"]);
                    m_EarQuery.IdEC.Value = idCurrentEventClass;

                    IEnumerable<DataRow> rowsEarDayOfCurrentEventClass = m_EventsInfo.DtEventClassWithEarDay
                        .Select("IDEC=" + idCurrentEventClass.ToString());
                    bool isEventClassWithEarDay = (rowsEarDayOfCurrentEventClass.Count() > 0);

                    int compareValue = Convert.ToDateTime(rowEventClass["DTEVENT"]).CompareTo(m_EarDate);

                    // 1- EventClass du jour
                    // ---------------------                                    
                    if (0 == compareValue)
                    {
                        #region Today EventClass
                        // EventClass n'est pas encore traité
                        if (dtEventClassToEarDay.Select("IDEC=" + idCurrentEventClass.ToString()).Count() == 0)
                        {
                            // 1.1- Earday déjà généré pour cet EventClass
                            // -------------------------------------------
                            // - Supprimer tous les EARs du jour parents des EarDays générés par cet EventClass
                            // et 
                            // - Considérer comme candidats à Earday tous les événements 
                            // . issus de l'Ear supprimé 
                            // . et non annulé (Removed)
                            if (isEventClassWithEarDay)
                            {
                                int idCurrentEarToDelete = 0;
                                int idBCurrentEarToDelete = 0;
                                int idEventClassOfDeletedEar = 0;

                                // Ne considérer que les EARs du jour
                                foreach (DataRow rowEarDayOfCurrentEventClass in rowsEarDayOfCurrentEventClass
                                    .Where(row => Convert.ToDateTime(row["DTEAR"]).CompareTo(m_EarDate) == 0))
                                {
                                    idCurrentEarToDelete = Convert.ToInt32(rowEarDayOfCurrentEventClass["IDEAR"]);
                                    idBCurrentEarToDelete = Convert.ToInt32(rowEarDayOfCurrentEventClass["IDB"]);

                                    // Chercher pour tous les EventClass frères (donc EarDay du même EAR), les autres éventuels EARs (différents de l'EAR en cours)
                                    IEnumerable<DataRow> rowsOtherEar =
                                        from rowEventClassOfEarToDelete in m_EventsInfo.DtEventClassWithEarDay.Rows.Cast<DataRow>()
                                        where Convert.ToInt32(rowEventClassOfEarToDelete["IDEAR"]) == idCurrentEarToDelete
                                        from rowOtherEar in m_EventsInfo.DtEventClassWithEarDay.Rows.Cast<DataRow>()
                                        where (Convert.ToInt32(rowOtherEar["IDEC"]) == Convert.ToInt32(rowEventClassOfEarToDelete["IDEC"]))
                                        && (Convert.ToInt32(rowOtherEar["IDEAR"]) != idCurrentEarToDelete)
                                        select rowOtherEar;

                                    // Considérer les EventClass qui ne sont inclus dans aucun autre EAR.
                                    bool isEventToAdd = (rowsOtherEar.Count() == 0);

                                    if (false == isEventToAdd)
                                    {
                                        // Existe-t-il un autre EAR d'un Book différent du Book en cours?
                                        // Exemple: le Book de la contrepartie qui est lui également un book géré
                                        //          le Book de l'entité qui est lui également un book géré
                                        #region
                                        IEnumerable<DataRow> rowsOtherBookEar =
                                            from rowEventClassOfOtherBookEar in rowsOtherEar
                                            where (Convert.ToInt32(rowEventClassOfOtherBookEar["IDB"]) != idBCurrentEarToDelete)
                                            select rowEventClassOfOtherBookEar;

                                        // Tous les autres EARs sont sur des Books différents du Book en cours
                                        if (rowsOtherBookEar.Count() == rowsOtherEar.Count())
                                        {
                                            isEventToAdd = true;

                                            // Pour tous les autres EARs des autres Books, vérifier si les EventClass ne sont pas inclus dans d'autres EARs
                                            foreach (DataRow rowOtherBookEar in rowsOtherBookEar)
                                            {
                                                int idCurrentOtherBookEar = Convert.ToInt32(rowOtherBookEar["IDEAR"]);
                                                int idBCurrentOtherBookEar = Convert.ToInt32(rowOtherBookEar["IDB"]);

                                                // Chercher pour tous les EventClass frères (donc EarDay du même EAR), les autres éventuels EARs (différent de l'EAR en cours)
                                                IEnumerable<DataRow> rowsOtherEarOfOtherBook =
                                                    from rowEventClassOfEarToDelete in m_EventsInfo.DtEventClassWithEarDay.Rows.Cast<DataRow>()
                                                    where Convert.ToInt32(rowEventClassOfEarToDelete["IDEAR"]) == idCurrentOtherBookEar
                                                    from rowOtherEar in m_EventsInfo.DtEventClassWithEarDay.Rows.Cast<DataRow>()
                                                    where (Convert.ToInt32(rowOtherEar["IDEC"]) == Convert.ToInt32(rowEventClassOfEarToDelete["IDEC"]))
                                                    && (Convert.ToInt32(rowOtherEar["IDEAR"]) != idCurrentEarToDelete)
                                                    && (Convert.ToInt32(rowOtherEar["IDEAR"]) != idCurrentOtherBookEar)
                                                    select rowOtherEar;

                                                // Considérer les EventClass qui ne sont inclus dans aucun autre EAR.
                                                bool isOtherEventToAdd = (rowsOtherEarOfOtherBook.Count() == 0);

                                                if (false == isOtherEventToAdd)
                                                {
                                                    // Existe-t-il un autre EAR d'un Book différent du Book en cours?
                                                    // Exemple: le Book de la contrepartie qui est lui également un book géré
                                                    //          le Book de l'entité qui est lui également un book géré
                                                    // Ici, on aurait pu faire un traitement récursif
                                                    IEnumerable<DataRow> rowsOtherBookEar1 =
                                                        from rowEventClassOfOtherBookEar in rowsOtherEarOfOtherBook
                                                        where (Convert.ToInt32(rowEventClassOfOtherBookEar["IDB"]) != idBCurrentOtherBookEar)
                                                        select rowEventClassOfOtherBookEar;

                                                    // Tous les autres EARs sont sur des Books différents du Book en cours
                                                    if (rowsOtherBookEar1.Count() == rowsOtherEarOfOtherBook.Count())
                                                    {
                                                        isOtherEventToAdd = true;

                                                        // Pour tous les autres EARs des autres Books, vérifier si les EventClass ne sont pas inclus dans d'autres EARs
                                                        foreach (DataRow rowOtherBookEar1 in rowsOtherBookEar1)
                                                        {
                                                            int idCurrentOtherBookEar1 = Convert.ToInt32(rowOtherBookEar1["IDEAR"]);
                                                            int idBCurrentOtherBookEar1 = Convert.ToInt32(rowOtherBookEar1["IDB"]);

                                                            // Il existe un autre EAR sur le Book de la première contrepartie
                                                            if (idBCurrentOtherBookEar1 == idBCurrentEarToDelete)
                                                            {
                                                                isOtherEventToAdd = false;
                                                                break;
                                                            }

                                                            // Chercher pour tous les EventClass frères (donc EarDay du même EAR), les autres éventuels EARs (différent de l'EAR en cours)
                                                            IEnumerable<DataRow> rowsOtherEarOfOtherBook1 =
                                                                from rowEventClassOfEarToDelete in m_EventsInfo.DtEventClassWithEarDay.Rows.Cast<DataRow>()
                                                                where Convert.ToInt32(rowEventClassOfEarToDelete["IDEAR"]) == idCurrentOtherBookEar1
                                                                from rowOtherEar in m_EventsInfo.DtEventClassWithEarDay.Rows.Cast<DataRow>()
                                                                where (Convert.ToInt32(rowOtherEar["IDEC"]) == Convert.ToInt32(rowEventClassOfEarToDelete["IDEC"]))
                                                                && (Convert.ToInt32(rowOtherEar["IDEAR"]) != idCurrentEarToDelete)
                                                                && (Convert.ToInt32(rowOtherEar["IDEAR"]) != idCurrentOtherBookEar)
                                                                && (Convert.ToInt32(rowOtherEar["IDEAR"]) != idCurrentOtherBookEar1)
                                                                select rowOtherEar;

                                                            // Considérer les EventClass qui ne sont inclus dans aucun autre EAR.
                                                            if (rowsOtherEarOfOtherBook1.Count() > 0)
                                                            {
                                                                isOtherEventToAdd = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                // Considérer les EventClass qui ne sont inclus dans aucun autre EAR.
                                                if (false == isOtherEventToAdd)
                                                {
                                                    isEventToAdd = false;
                                                    break;
                                                }
                                            }
                                        }
                                        #endregion
                                    }

                                    // Ne pas considérer les EventClass inclus dans d'autres EARs, en plus de l'EAR du jour en cours
                                    if (isEventToAdd)
                                    {
                                        idEventClassOfDeletedEar = Convert.ToInt32(rowEarDayOfCurrentEventClass["IDEC"]);

                                        // Supprimer l'EAR du jour s'il n'a pas été déjà supprimé 
                                        if (false == lstDeletedEar.Contains(idCurrentEarToDelete))
                                        {
                                            DeleteTodayEar(rowEarDayOfCurrentEventClass);
                                            lstDeletedEar.Add(idCurrentEarToDelete);
                                        }

                                        // Chercher tous les EventClass (donc EarDay), issus de l'EAR du jour supprimé 
                                        IEnumerable<DataRow> rowsEarDayOfDeletedEar =
                                            from row in m_EventsInfo.DtEventClassWithEarDay.Rows.Cast<DataRow>()
                                            where Convert.ToInt32(row["IDEAR"]) == idCurrentEarToDelete
                                            select row;

                                        foreach (DataRow rowEarDayOfDeletedEar in rowsEarDayOfDeletedEar)
                                        {
                                            DataRow rowEventClassOfDeletedEarDay = m_EventsInfo.DtEventClass.Select("IDEC=" + rowEarDayOfDeletedEar["IDEC"].ToString()).First();
                                            DataRow rowEventOfDeletedEarDay = rowEventClassOfDeletedEarDay.GetParentRow(m_EventsInfo.ChildEventClass);
                                            bool isEventOfDeletedEarRemoved = CheckEventRemoved(rowEventOfDeletedEarDay);

                                            // Considérer comme candidats à Earday tous les événements:
                                            // - issus de l'EAR du jour supprimé 
                                            // - et non annulé (Removed)
                                            if (isEventOfDeletedEarRemoved == false)
                                                AddEventCandidate(ref dtEventToEarDay, ref dtEventClassToEarDay, rowEventOfDeletedEarDay, rowEventClassOfDeletedEarDay, true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // 1.2- Evénement non annulé
                                // ---------------------------
                                if (isEventRemoved == false)
                                    AddEventCandidate(ref dtEventToEarDay, ref dtEventClassToEarDay, rowEvent, rowEventClass, true);

                                // 1.3- Evénement annulé
                                // -----------------------
                                // - Ne pas généré l'EAR d'un Evénement jour annulé
                            }
                        }
                        #endregion
                    }
                    // 2- Evénement passé
                    // ------------------
                    else if (0 > compareValue)
                    {
                        #region Past EventClass
                        // 2.1- Earday déjà généré pour cet EventClass
                        // -------------------------------------------
                        if (isEventClassWithEarDay)
                        {
                            // 2.1.1- Evénement non annulé
                            // -------------------------                                    
                            // - Ne pas ré-généré l'EAR d'un Evénement passé non annulé

                            // 2.1.2- Evénement annulé
                            // -------------------------                                    
                            if (m_IsNativeAccRemove && isEventRemoved)
                            {
                                var rowsEventClassWithRemovedEar = rowsEarDayOfCurrentEventClass.Where(row => (row["IDSTACTIVATION"].ToString() == Cst.StatusActivation.REMOVED.ToString()));
                                // - Générer l'EAR Removed, s'il n'a pas été déjà généré
                                if (rowsEventClassWithRemovedEar.Count() == 0)
                                {
                                    var rowsEventClassRMV = m_EventsInfo.DtEventClassWithEarDay.Select()
                                        .Where(row => (row["EVENTCLASS"].ToString() == EventClassFunc.RemoveEvent)
                                        && Convert.ToInt32(row["IDE"]) == idCurrentEvent);

                                    // - Générer l'EAR Removed, si l'EARDAY xxx/yyy/RMV n'a pas été déjà généré (en mode annulation externe)
                                    if (rowsEventClassRMV.Count() == 0)
                                        AddEventCandidate(ref dtEventToEarDay, ref dtEventClassToEarDay, rowEvent, rowEventClass, false);
                                }
                            }
                        }
                        // 2.2- Earday non généré pour cet EventClass
                        // ------------------------------------------
                        else
                        {
                            // 2.2.1- Evénement non annulé
                            // -------------------------                                    
                            // - Générer l'EAR
                            if (isEventRemoved == false)
                                // RD 20140205 [19577] Set the "pIsWithEventValidation" value to "true"
                                // in order to verify if the Event is already valorised, otherwise, an error is raised
                                AddEventCandidate(ref dtEventToEarDay, ref dtEventClassToEarDay, rowEvent, rowEventClass, true);
                            else
                            {
                                // EventClass d'annulation, donc pas de gestion native de l'annulation des évenements
                                if (rowEventClass["EVENTCLASS"].ToString() == EventClassFunc.RemoveEvent)
                                {
                                    var rowsEventClassBrother = m_EventsInfo.DtEventClassWithEarDay.Select()
                                        .Where(row => (row["EVENTCLASS"].ToString() != EventClassFunc.RemoveEvent)
                                        && Convert.ToInt32(row["IDE"]) == idCurrentEvent);

                                    // - Générer l'EARDAY xxx/yyy/RMV, s'il existe des EARDAY frères déjà générés
                                    if (rowsEventClassBrother.Count() > 0)
                                    {
                                        var rowsEventClassWithRemovedEar = rowsEventClassBrother.Where(row => (row["IDSTACTIVATION"].ToString() == Cst.StatusActivation.REMOVED.ToString()));

                                        // - Générer l'EARDAY xxx/yyy/RMV, si un EAR REMOVED n'est pas généré (en mode annulation native)
                                        if (rowsEventClassWithRemovedEar.Count() == 0)
                                            AddEventCandidate(ref dtEventToEarDay, ref dtEventClassToEarDay, rowEvent, rowEventClass, false);
                                    }
                                }
                            }

                            // 2.2.2- Evénement annulé
                            // ------------------------- 
                            // - Ne pas généré l'EAR Romeved d'un Evénement annulé sans EAR
                        }
                        #endregion
                    }
                }
                #endregion
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                if (dtEventClassToEarDay.Rows.Count == 0 || dtEventToEarDay.Rows.Count == 0)
                {
                    // Log d'un Warning, si des Ear du jour ont été delétés (i.e: les EARs ont été générés une première fois et ensuite annulés dans la foulée)
                    if (lstDeletedEar.Count() > 0)
                    {
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "LOG-05396",
                            new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND),
                            m_LstMasterData.Find(match => match.First == "TRADE").Second,
                            m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                            m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second);
                    }
                    else
                    {
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-05336",
                             new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                             m_LstMasterData.Find(match => match.First == "TRADE").Second,
                             m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                             m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second);
                    }
                }

                m_EventsInfo.DsEvent.Tables.Add(dtEventClassToEarDay);
                m_EventsInfo.DsEvent.Tables.Add(dtEventToEarDay);

                // Add relation between EventToEarDay & EventClassToEarDay
                DataRelation relEventClassToEarDay = new DataRelation("EventToEarDay_EventClassToEarDay",
                    dtEventToEarDay.Columns["IDE"], dtEventClassToEarDay.Columns["IDE"], false);
                m_EventsInfo.DsEvent.Relations.Add(relEventClassToEarDay);

            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return codeReturn; ;
        }
        #endregion GetEventsToEarDay

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDtEventToEarDay"></param>
        /// <param name="pDtEventClassToEarDay"></param>
        /// <param name="pDrEventToAdd"></param>
        /// <param name="pDrEventClassToAdd"></param>
        /// <param name="pIsWithEventValidation"></param>
        protected void AddEventCandidate(ref DataTable pDtEventToEarDay, ref DataTable pDtEventClassToEarDay,
            DataRow pDrEventToAdd, DataRow pDrEventClassToAdd, bool pIsWithEventValidation)
        {
            if (pIsWithEventValidation && Convert.IsDBNull(pDrEventToAdd["VALORISATION"]))
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-05335",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE),
                    m_LstMasterData.Find(match => match.First == "TRADE").Second,
                    m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                    m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second,
                    LogTools.IdentifierAndId(pDrEventToAdd["EVENTCODE"].ToString() + "/" + pDrEventToAdd["EVENTTYPE"].ToString(),
                    Convert.ToInt32(pDrEventToAdd["IDE"])));
            }

            // Evénement n'est pas encore traité
            if (pDtEventToEarDay.Select("IDE=" + pDrEventToAdd["IDE"].ToString()).Count() == 0)
                pDtEventToEarDay.ImportRow(pDrEventToAdd);

            // EventClass n'est pas encore traité
            if (pDtEventClassToEarDay.Select("IDEC=" + pDrEventClassToAdd["IDEC"].ToString()).Count() == 0)
                pDtEventClassToEarDay.ImportRow(pDrEventClassToAdd);
        }

        /// <summary>
        /// Obtenir le mode de gestion de l'annulation comptable des événements via l'EventClass RMV
        /// <para>Gestion native: RMV NON candidat à EARDAY (EVENTENUM.EAR_DAY=0)</para>
        /// <para>Gestion externe: RMV candidat à EARDAY (EVENTENUM.EAR_DAY>1)</para>
        /// </summary>
        /// <returns></returns>
        protected void SetIsNativeAccRemove()
        {
            m_IsNativeAccRemove = false;

            IEnumerable<DataRow> rowsRMV =
                from row in m_EventsInfo.DtEventClass.Select().Cast<DataRow>()
                where row["EVENTCLASS"].ToString() == EventClassFunc.RemoveEvent
                select row;

            if (rowsRMV.Count() > 0)
                m_IsNativeAccRemove = (false == (Convert.ToBoolean(rowsRMV.First()["ISEARDAY"])));
        }

        /// <summary>
        /// Ecriture des EAR dans la DB
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <returns></returns>
        // EG 20180606 [23979] IRQ (EARGEN)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel WriteEarAndEarDayAndEarNom(DataDbTransaction pEfsTransaction)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " ...............");
#endif
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (ArrFunc.IsFilled(m_EarBooks))
            {
                int numberOfTokenEar = m_EarBooks.Count;
                int numberOfTokenEarDay = (
                        from pre_EarBook in m_EarBooks
                        from earDet in pre_EarBook.EarDets
                        select earDet.EarDays.Count).Sum();
                int numberOfTokenEarNom = (
                        from earBook in m_EarBooks
                        from earDet in earBook.EarDets
                        select earDet.EarNoms.Count).Sum();

                int newIdEARDAY = 0;
                int newIdEARNOM = 0;

                codeReturn = SQLUP.GetId(out int newIdEAR, pEfsTransaction.Transaction, SQLUP.IdGetId.EAR, SQLUP.PosRetGetId.First, numberOfTokenEar);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (numberOfTokenEarDay > 0)
                        codeReturn = SQLUP.GetId(out newIdEARDAY, pEfsTransaction.Transaction, SQLUP.IdGetId.EARDAY, SQLUP.PosRetGetId.First,
                        numberOfTokenEarDay);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        if (numberOfTokenEarNom > 0)
                            codeReturn = SQLUP.GetId(out newIdEARNOM, pEfsTransaction.Transaction, SQLUP.IdGetId.EARNOM, SQLUP.PosRetGetId.First,
                            numberOfTokenEarNom);

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            int rowAffected = 0;

                            foreach (Pre_EarBook pre_EarBook in m_EarBooks)
                            {
                                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || 
                                    (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                    break;

                                pre_EarBook.IdEAR = newIdEAR;

                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5391), 2,
                                    new LogParam("EAR"),
                                    new LogParam(pre_EarBook.IdEAR),
                                    new LogParam(LogTools.IdentifierAndId(pre_EarBook.IdB_Identifier, pre_EarBook.IdB)),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(pre_EarBook.DtEvent))));

                                m_EarQuery.IdEAR.Value = pre_EarBook.IdEAR;

                                m_EarQuery.IdB.Value = pre_EarBook.IdB;
                                m_EarQuery.DtEar.Value = m_EarDate;
                                m_EarQuery.DtEvent.Value = pre_EarBook.DtEvent;
                                m_EarQuery.DtRemoved.Value = (DtFunc.IsDateTimeFilled(pre_EarBook.DtRemoved) ? pre_EarBook.DtRemoved : Convert.DBNull);
                                m_EarQuery.IdAIns.Value = Session.IdA;
                                // FI 20140401 [19806] utilisation de la property serviceName
                                //m_EarQuery.Source.Value = appInstance.AppNameVersion;
                                m_EarQuery.Source.Value = AppInstance.ServiceName;
                                m_EarQuery.ExtLink.Value = Convert.DBNull;
                                m_EarQuery.IdStActivation.Value = pre_EarBook.IdStActivation.ToString();

                                rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, m_EarQuery.m_SQLQueryEar,
                                    m_EarQuery.IdEAR, m_EarQuery.IdT, m_EarQuery.IdB, m_EarQuery.DtEar, m_EarQuery.DtEvent, m_EarQuery.DtRemoved,
                                    m_EarQuery.IdAIns, m_EarQuery.Source, m_EarQuery.IdStActivation, m_EarQuery.ExtLink);

                                if (0 <= rowAffected)
                                {
                                    foreach (Pre_EarDet pre_EarDet in pre_EarBook.EarDets)
                                    {

                                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || 
                                            (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                            break;

                                        m_EarQuery.InstrumentNo.Value = pre_EarDet.instrumentNo;
                                        m_EarQuery.StreamNo.Value = pre_EarDet.streamNo;

                                        rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, m_EarQuery.m_SQLQueryEarDet,
                                            m_EarQuery.IdEAR, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo);

                                        if (0 <= rowAffected)
                                        {
                                            WriteEarNom(pEfsTransaction, pre_EarDet.EarNoms, ref newIdEARNOM, pre_EarBook, pre_EarDet);
                                            WriteEarAndEarDayAndEarNom(pEfsTransaction, pre_EarDet.EarDays, ref newIdEARDAY, pre_EarBook, pre_EarDet);
                                        }
                                    }
                                }

                                newIdEAR++;
                            }

                            if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) && 
                                (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                            {
                                // RD 20120809 [18070] Optimisation
                                if (ProcessTuningSpecified)
                                {
#if DEBUG
                                    diagnosticDebug.Start("ProcessTuning: UpdateEventStatus");
#endif
                                    m_EventsInfo.Update(pEfsTransaction.Transaction, Cst.OTCml_TBL.EVENTSTCHECK);
                                    m_EventsInfo.Update(pEfsTransaction.Transaction, Cst.OTCml_TBL.EVENTSTMATCH);
                                    m_EventsInfo.Update(pEfsTransaction.Transaction, Cst.OTCml_TBL.EVENT);
#if DEBUG
                                    diagnosticDebug.End("ProcessTuning: UpdateEventStatus");
#endif
                                }

#if DEBUG
                                diagnosticDebug.Start("WriteEventProcess");
#endif
                                List<int> events =
                                    (from earBook in m_EarBooks
                                     from earDet in earBook.EarDets
                                     from earDay in earDet.EarDays
                                     select earDay.idE).Distinct().ToList();

                                // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                                //EventProcess.Write(pEfsTransaction.Cs, pEfsTransaction.Transaction, events,
                                //        Cst.ProcessTypeEnum.EARGEN, ProcessStateTools.StatusSuccessEnum, processLog.GetDate(),
                                //        string.Empty, tracker.idTRK_L, 0);
                                EventProcess.Write(pEfsTransaction.Cs, pEfsTransaction.Transaction, events,
                                        Cst.ProcessTypeEnum.EARGEN, ProcessStateTools.StatusSuccessEnum, OTCmlHelper.GetDateSysUTC(Cs),
                                        string.Empty, Tracker.IdTRK_L, 0);
#if DEBUG
                                diagnosticDebug.End("WriteEventProcess");
#endif
                            }
                        }
                    }
                }
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "..........................................");
#endif
            return codeReturn;
        }

        // EG 20180606 [23979] IRQ (EARGEN)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel WriteEarCommon(DataDbTransaction pEfsTransaction)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (ArrFunc.IsFilled(m_EarBooks))
            {
                int numberOfTokenEarCommon = (
                    from earBook in m_EarBooks
                    from earDet in earBook.EarDets
                    select earDet.EarCommons.Count).Sum();

                int newIdEARCOMMON = 0;

                if (numberOfTokenEarCommon > 0)
                    codeReturn = SQLUP.GetId(out newIdEARCOMMON, pEfsTransaction.Transaction, SQLUP.IdGetId.EARCOMMON, SQLUP.PosRetGetId.First,
                    numberOfTokenEarCommon);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    foreach (Pre_EarBook pre_EarBook in m_EarBooks)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ||
                            (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                            break;

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5391), 2,
                            new LogParam("EARCOMMON"),
                            new LogParam(pre_EarBook.IdEAR),
                            new LogParam(LogTools.IdentifierAndId(pre_EarBook.IdB_Identifier, pre_EarBook.IdB)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(pre_EarBook.DtEvent))));

                        m_EarQuery.IdEAR.Value = pre_EarBook.IdEAR;

                        foreach (Pre_EarDet pre_EarDet in pre_EarBook.EarDets)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ||
                                (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                break;

                            m_EarQuery.InstrumentNo.Value = pre_EarDet.instrumentNo;
                            m_EarQuery.StreamNo.Value = pre_EarDet.streamNo;

                            if (0 == pre_EarDet.EarDays.Count)
                            {
                                DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, m_EarQuery.m_SQLQueryEarDet,
                                    m_EarQuery.IdEAR, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo);
                            }

                            WriteEarCommon(pEfsTransaction, pre_EarDet.EarCommons, ref newIdEARCOMMON, pre_EarBook, pre_EarDet);
                        }
                    }
                }
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return codeReturn;
        }
        /// <summary>
        /// Alimentation de la table EARCALC pour une sequence donnée
        /// <para>Alimentation en fonction du contenu des earDet.EarCalcs</para>
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pSequenceNo">N° de sequence </param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel WriteEarCalc(DataDbTransaction pEfsTransaction, int pSequenceNo)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            string otcmlData = string.Empty;
            string otcmlDataDet = string.Empty;

            if (ArrFunc.IsFilled(m_EarBooks))
            {
                int numberOfTokenEarCalc = (
                        from pre_EarBook in m_EarBooks
                        from earDet in pre_EarBook.EarDets
                        from earCalc in earDet.EarCalcs
                        where earCalc.sequenceNo == pSequenceNo
                        select earCalc).Count();

                int newIdEARCALC = 0;

                if (numberOfTokenEarCalc > 0)
                    codeReturn = SQLUP.GetId(out newIdEARCALC, pEfsTransaction.Transaction, SQLUP.IdGetId.EARCALC, SQLUP.PosRetGetId.First,
                    numberOfTokenEarCalc);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    foreach (Pre_EarBook pre_EarBook in m_EarBooks)
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5391), 2,
                            new LogParam("EARCALC"),
                            new LogParam(pre_EarBook.IdEAR),
                            new LogParam(LogTools.IdentifierAndId(pre_EarBook.IdB_Identifier, pre_EarBook.IdB)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(pre_EarBook.DtEvent))));

                        m_EarQuery.IdEAR.Value = pre_EarBook.IdEAR;

                        foreach (Pre_EarDet pre_EarDet in pre_EarBook.EarDets)
                        {
                            m_EarQuery.InstrumentNo.Value = pre_EarDet.instrumentNo;
                            m_EarQuery.StreamNo.Value = pre_EarDet.streamNo;

                            List<Pre_EarCalc> currentSeqEarCalc = (
                                from earCalc in pre_EarDet.EarCalcs
                                where earCalc.sequenceNo == pSequenceNo
                                select earCalc).ToList();

                            List<Pre_EarCalc> previousSeqEarCalc = (
                                from earCalc in pre_EarDet.EarCalcs
                                where earCalc.sequenceNo < pSequenceNo
                                select earCalc).ToList();

                            if (0 < currentSeqEarCalc.Count)
                            {
                                // Alimentation de EARDET si non encore existant
                                if ((0 == pre_EarDet.EarDays.Count) &&
                                    (0 == pre_EarDet.EarCommons.Count) &&
                                    (0 == previousSeqEarCalc.Count))
                                {
                                    DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, m_EarQuery.m_SQLQueryEarDet,
                                        m_EarQuery.IdEAR, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo);
                                }

                                WriteEarCalc(pEfsTransaction, currentSeqEarCalc, ref newIdEARCALC, pre_EarBook, pre_EarDet);
                            }
                        }
                    }
                }
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return codeReturn;
        }

        /// <summary>
        /// Ecrire dans la DB les EARDAY 
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pEarDet"></param>
        /// <param name="pIdEARDAY"></param>
        /// <param name="pOTCmlData"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private int WriteEarAndEarDayAndEarNom(DataDbTransaction pEfsTransaction, List<Pre_EarDay> pEarDays,
            ref int pIdEARDAY, Pre_EarBook pPre_EarBook, Pre_EarDet pPre_EarDet)
        {
            int rowAffected = 0;

            foreach (Pre_EarDay earDay in pEarDays)
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5393), 3,
                    new LogParam("EARDAY"),
                    new LogParam(pPre_EarBook.IdEAR),
                    new LogParam(pIdEARDAY),
                    new LogParam(LogTools.IdentifierAndId(pPre_EarBook.IdB_Identifier, pPre_EarBook.IdB)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(pPre_EarBook.DtEvent)),
                    new LogParam(pPre_EarDet.instrumentNo.ToString() + "/" + pPre_EarDet.streamNo.ToString()),
                    new LogParam(earDay.earCode),
                    new LogParam(earDay.eventClass),
                    new LogParam(earDay.eventCode + " / " + earDay.eventType),
                    new LogParam(DtFunc.DateTimeToStringDateISO(earDay.dtEvent))));

                earDay.idEARDay = pIdEARDAY;
                m_EarQuery.IdEARDAY.Value = earDay.idEARDay;
                m_EarQuery.EarCode.Value = earDay.earCode;
                m_EarQuery.EventCode.Value = earDay.eventCode;
                m_EarQuery.EventType.Value = earDay.eventType;
                m_EarQuery.EventClass.Value = earDay.eventClass;
                m_EarQuery.IdEC.Value = earDay.idEC;

                rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, m_EarQuery.m_SQLQueryEarDay,
                    m_EarQuery.IdEARDAY, m_EarQuery.IdEAR, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo, m_EarQuery.IdEC,
                    m_EarQuery.EarCode, m_EarQuery.EventCode, m_EarQuery.EventType, m_EarQuery.EventClass);

                if (0 <= rowAffected)
                {
                    foreach (Pre_EarAmount earAmount in earDay.earAmounts)
                    {
                        m_EarQuery.ExchangeType.Value = earAmount.ExchangeType.ToString();
                        // RD 20100420 [16956]
                        if (StrFunc.IsFilled(earAmount.IdC))
                            m_EarQuery.IdC.Value = earAmount.IdC;
                        else
                            m_EarQuery.IdC.Value = DBNull.Value;

                        m_EarQuery.Paid.Value = earAmount.Paid;
                        m_EarQuery.Received.Value = earAmount.Received;
                        m_EarQuery.IdQuote_H.Value = (0 == earAmount.IdQuote_H ? Convert.DBNull : earAmount.IdQuote_H);
                        m_EarQuery.IdQuote_H2.Value = (0 == earAmount.IdQuote_H2 ? Convert.DBNull : earAmount.IdQuote_H2);
                        m_EarQuery.IdStProcess.Value = earAmount.IdStProcess;

                        rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                            m_EarQuery.m_SQLQueryEarDayAmount, m_EarQuery.IdEARDAY, m_EarQuery.ExchangeType, m_EarQuery.IdC,
                            m_EarQuery.Paid, m_EarQuery.Received, m_EarQuery.IdQuote_H, m_EarQuery.IdQuote_H2, m_EarQuery.IdStProcess);
                    }
                }

                pIdEARDAY++;

                if (ProcessTuningSpecified)
                {
                    // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                    //m_EventsInfo.SetEventStatus(earDay.idE, m_EventsInfo.Tuning, this.appInstance.IdA, processLog.GetDate());
                    // FI 20200820 [25468] dates systemes en UTC
                    m_EventsInfo.SetEventStatus(earDay.idE, m_EventsInfo.Tuning, this.Session.IdA, OTCmlHelper.GetDateSysUTC(Cs));
                }
            }
            return rowAffected;
        }
        /// <summary>
        /// Ecrire dans la DB les EARNOM
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pEarNoms"></param>
        /// <param name="pIdEARNOM"></param>
        /// <param name="pPre_EarBook"></param>
        /// <param name="pPre_EarDet"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private int WriteEarNom(DataDbTransaction pEfsTransaction, List<Pre_EarNom> pEarNoms,
            ref int pIdEARNOM, Pre_EarBook pPre_EarBook, Pre_EarDet pPre_EarDet)
        {
            int rowAffected = 0;

            foreach (Pre_EarNom earNom in pEarNoms)
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5393), 3,
                    new LogParam("EARNOM"),
                    new LogParam(pPre_EarBook.IdEAR),
                    new LogParam(pIdEARNOM),
                    new LogParam(LogTools.IdentifierAndId(pPre_EarBook.IdB_Identifier, pPre_EarBook.IdB)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(pPre_EarBook.DtEvent)),
                    new LogParam(pPre_EarDet.instrumentNo.ToString() + "/" + pPre_EarDet.streamNo.ToString()),
                    new LogParam(earNom.earCode),
                    new LogParam(earNom.eventClass),
                    new LogParam(earNom.eventCode + " / " + earNom.eventType),
                    new LogParam(DtFunc.DateTimeToStringDateISO(earNom.dtAccount))));

                earNom.idEARNom = pIdEARNOM;
                m_EarQuery.IdEARNOM.Value = earNom.idEARNom;
                m_EarQuery.EventCode.Value = earNom.eventCode;
                m_EarQuery.EventType.Value = earNom.eventType;
                m_EarQuery.EventClass.Value = earNom.eventClass;
                m_EarQuery.DtAccount.Value = earNom.dtAccount;

                rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                    m_EarQuery.m_SQLQueryEarNom,
                    m_EarQuery.IdEARNOM, m_EarQuery.IdEAR, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo,
                    m_EarQuery.EventCode, m_EarQuery.EventType, m_EarQuery.EventClass, m_EarQuery.DtAccount);

                if (0 <= rowAffected)
                {
                    foreach (int ide in earNom.earNomEvent)
                    {
                        m_EarQuery.IdAIns.Value = Session.IdA;
                        m_EarQuery.IdE.Value = ide;

                        rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                            m_EarQuery.m_SQLQueryEarNomEvent,
                            m_EarQuery.IdEARNOM, m_EarQuery.IdE, m_EarQuery.IdAIns);
                    }
                }

                if (0 <= rowAffected)
                {
                    foreach (Pre_EarAmount earAmount in earNom.earAmounts)
                    {
                        m_EarQuery.ExchangeType.Value = earAmount.ExchangeType.ToString();
                        // RD 20100420 [16956]
                        if (StrFunc.IsFilled(earAmount.IdC))
                            m_EarQuery.IdC.Value = earAmount.IdC;
                        else
                            m_EarQuery.IdC.Value = DBNull.Value;
                        //
                        m_EarQuery.Amount.Value = earAmount.Paid;
                        m_EarQuery.IdQuote_H.Value = (0 == earAmount.IdQuote_H ? Convert.DBNull : earAmount.IdQuote_H);
                        m_EarQuery.IdQuote_H2.Value = (0 == earAmount.IdQuote_H2 ? Convert.DBNull : earAmount.IdQuote_H2);
                        m_EarQuery.IdStProcess.Value = earAmount.IdStProcess;

                        rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                            m_EarQuery.m_SQLQueryEarNomAmount,
                            m_EarQuery.IdEARNOM, m_EarQuery.ExchangeType, m_EarQuery.IdC, m_EarQuery.Amount,
                            m_EarQuery.IdQuote_H, m_EarQuery.IdQuote_H2, m_EarQuery.IdStProcess);
                    }
                }
                pIdEARNOM++;
            }

            return rowAffected;
        }
        /// <summary>
        /// Ecrire dans la DB les EARCOMMON 
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pEarCommons"></param>
        /// <param name="pIdEARCOMMON"></param>
        /// <param name="pPre_EarBook"></param>
        /// <param name="pPre_EarDet"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private int WriteEarCommon(DataDbTransaction pEfsTransaction, List<Pre_EarCommon> pEarCommons,
            ref int pIdEARCOMMON, Pre_EarBook pPre_EarBook, Pre_EarDet pPre_EarDet)
        {
            int rowAffected = 0;

            foreach (Pre_EarCommon earCommon in pEarCommons)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5393), 4,
                    new LogParam("EARCOMMON"),
                    new LogParam(pPre_EarBook.IdEAR),
                    new LogParam(pIdEARCOMMON),
                    new LogParam(LogTools.IdentifierAndId(pPre_EarBook.IdB_Identifier, pPre_EarBook.IdB)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(pPre_EarBook.DtEvent)),
                    new LogParam(pPre_EarDet.instrumentNo.ToString() + "/" + pPre_EarDet.streamNo.ToString()),
                    new LogParam(earCommon.earCode),
                    new LogParam(earCommon.eventClass),
                    new LogParam(earCommon.eventCode + " / " + earCommon.eventType),
                    new LogParam(DtFunc.DateTimeToStringDateISO(earCommon.dtEvent))));

                earCommon.idEARCommon = pIdEARCOMMON;
                m_EarQuery.IdEARCOMMON.Value = earCommon.idEARCommon;
                m_EarQuery.EarCode.Value = earCommon.earCode;
                m_EarQuery.EventCode.Value = earCommon.eventCode;
                m_EarQuery.EventType.Value = earCommon.eventType;
                m_EarQuery.EventClass.Value = earCommon.eventClass;
                m_EarQuery.IdEARDAY.Value = (earCommon.idEARDay == 0 ? Convert.DBNull : earCommon.idEARDay);

                rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                    m_EarQuery.m_SQLQueryEarCommon,
                    m_EarQuery.IdEARCOMMON, m_EarQuery.IdEAR, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo,
                    m_EarQuery.IdEARDAY, m_EarQuery.EarCode, m_EarQuery.EventCode, m_EarQuery.EventType, m_EarQuery.EventClass);

                if (0 <= rowAffected)
                {
                    foreach (Pre_EarAmount earAmount in earCommon.earAmounts)
                    {
                        m_EarQuery.ExchangeType.Value = earAmount.ExchangeType.ToString();
                        // RD 20100420 [16956]
                        if (StrFunc.IsFilled(earAmount.IdC))
                            m_EarQuery.IdC.Value = earAmount.IdC;
                        else
                            m_EarQuery.IdC.Value = DBNull.Value;

                        m_EarQuery.Paid.Value = earAmount.Paid;
                        m_EarQuery.Received.Value = earAmount.Received;
                        m_EarQuery.IdQuote_H.Value = (0 == earAmount.IdQuote_H ? Convert.DBNull : earAmount.IdQuote_H);
                        m_EarQuery.IdQuote_H2.Value = (0 == earAmount.IdQuote_H2 ? Convert.DBNull : earAmount.IdQuote_H2);
                        m_EarQuery.IdStProcess.Value = earAmount.IdStProcess;

                        rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                            m_EarQuery.m_SQLQueryEarCommonAmount,
                            m_EarQuery.IdEARCOMMON, m_EarQuery.ExchangeType, m_EarQuery.IdC, m_EarQuery.Paid, m_EarQuery.Received,
                            m_EarQuery.IdQuote_H, m_EarQuery.IdQuote_H2, m_EarQuery.IdStProcess);
                    }
                }
                pIdEARCOMMON++;
            }
            return rowAffected;
        }
        /// <summary>
        /// Ecrire dans la DB dans la table EARCALC, EARCALCAMOUNT,  EARCALEVENT
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pEarCalcs">Liste des montants calculés</param>
        /// <param name="pIdEARCALC">Jeton </param>
        /// <param name="pPre_EarBook"></param>
        /// <param name="pPre_EarDet"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private int WriteEarCalc(DataDbTransaction pEfsTransaction, List<Pre_EarCalc> pEarCalcs,
            ref int pIdEARCALC, Pre_EarBook pPre_EarBook, Pre_EarDet pPre_EarDet)
        {
            int rowAffected = 0;

            foreach (Pre_EarCalc earCalc in pEarCalcs)
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5394), 4,
                    new LogParam(pPre_EarBook.IdEAR),
                    new LogParam(pIdEARCALC),
                    new LogParam(LogTools.IdentifierAndId(pPre_EarBook.IdB_Identifier, pPre_EarBook.IdB)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(pPre_EarBook.DtEvent)),
                    new LogParam(pPre_EarDet.instrumentNo.ToString() + "/" + pPre_EarDet.streamNo.ToString()),
                    new LogParam(earCalc.earCode),
                    new LogParam(earCalc.calcType),
                    new LogParam(earCalc.eventClass),
                    new LogParam(DtFunc.DateTimeToStringDateISO(earCalc.dtAccount))));

                earCalc.idEARCalc = pIdEARCALC;
                m_EarQuery.IdEARCALC.Value = earCalc.idEARCalc;
                m_EarQuery.EarCode.Value = earCalc.earCode;
                m_EarQuery.CalcType.Value = earCalc.calcType;
                m_EarQuery.EventClass.Value = earCalc.eventClass;
                m_EarQuery.DtAccount.Value = earCalc.dtAccount;

                rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                    m_EarQuery.m_SQLQueryEarCalc,
                    m_EarQuery.IdEARCALC, m_EarQuery.IdEAR, m_EarQuery.InstrumentNo, m_EarQuery.StreamNo,
                    m_EarQuery.DtAccount, m_EarQuery.EarCode, m_EarQuery.CalcType, m_EarQuery.EventClass);

                // RD 20120308 /Pour l'EARCALC NETINTNOM, earCalc.earCalcEvent est NULL, à revoir !?
                if ((0 <= rowAffected) && ArrFunc.IsFilled(earCalc.earCalcEvent))
                {
                    foreach (int ide in earCalc.earCalcEvent)
                    {
                        m_EarQuery.IdAIns.Value = Session.IdA;
                        m_EarQuery.IdE.Value = ide;

                        rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                            m_EarQuery.m_SQLQueryEarCalcEvent, m_EarQuery.IdEARCALC, m_EarQuery.IdE, m_EarQuery.IdAIns);
                    }
                }

                if (0 <= rowAffected)
                {
                    foreach (Pre_EarAmount earAmount in earCalc.earAmounts)
                    {
                        m_EarQuery.ExchangeType.Value = earAmount.ExchangeType.ToString();
                        // RD 20100420 [16956]
                        if (StrFunc.IsFilled(earAmount.IdC))
                            m_EarQuery.IdC.Value = earAmount.IdC;
                        else
                            m_EarQuery.IdC.Value = DBNull.Value;

                        m_EarQuery.Paid.Value = earAmount.Paid;
                        m_EarQuery.Received.Value = earAmount.Received;
                        m_EarQuery.IdQuote_H.Value = (0 == earAmount.IdQuote_H ? Convert.DBNull : earAmount.IdQuote_H);
                        m_EarQuery.IdQuote_H2.Value = (0 == earAmount.IdQuote_H2 ? Convert.DBNull : earAmount.IdQuote_H2);
                        m_EarQuery.IdStProcess.Value = earAmount.IdStProcess;


                        rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text,
                            m_EarQuery.m_SQLQueryEarCalcAmount,
                            m_EarQuery.IdEARCALC, m_EarQuery.ExchangeType, m_EarQuery.IdC, m_EarQuery.Paid, m_EarQuery.Received,
                            m_EarQuery.IdQuote_H, m_EarQuery.IdQuote_H2, m_EarQuery.IdStProcess);

                    }
                }

                pIdEARCALC++;
            }

            return rowAffected;
        }
        /// <summary>
        /// Obtenir la date de transaction du trade, via l'événement TRD/DAT/GRP
        /// </summary>
        /// <returns></returns>
        protected void SetTradeDtTransaction()
        {
            m_TransactDate = DateTime.MinValue;

            DataRow eventClassGRP = null;
            DataRow[] eventTRDDAT = m_EventsInfo.DtEvent.Select("EVENTCODE=" +
                DataHelper.SQLString(EventCodeFunc.Trade) + " and EVENTTYPE=" +
                DataHelper.SQLString(EventTypeFunc.Date));


            if (eventTRDDAT != null && eventTRDDAT.Length == 1)
            {
                DataRow[] rowEventClass = eventTRDDAT[0].GetChildRows(m_EventsInfo.ChildEventClass);

                IEnumerable<DataRow> rowsGRP =
                    from row in rowEventClass.Cast<DataRow>()
                    where row["EVENTCLASS"].ToString() == EventClassFunc.GroupLevel
                    select row;

                if (rowsGRP.Count() > 0)
                {
                    eventClassGRP = rowsGRP.First();
                    if (eventClassGRP != null)
                        m_TransactDate = Convert.ToDateTime(eventClassGRP["DTEVENT"]);
                }
            }
            m_LstMasterData.Add(new Pair<string, string>("TRANSACDATE", DtFunc.DateTimeToStringDateISO(m_TransactDate)));
        }
        /// <summary>
        /// Initialiser les données nécessaires au calcul des contrevaleurs
        /// <para>- Date transaction (m_TransactDate)</para>
        /// <para>- Date traitement (m_EarDate)</para>
        /// </summary>
        protected void SetCounterValueData()
        {
            m_Pre_EarAmounts = new Pre_EarAmounts(m_TransactDate, m_EarDate);
        }
        /// <summary>
        /// Initialiser les données nécessaires au calcul des contrevaleurs
        /// <para>- Devise de flux {pIdFCU}</para>
        /// <para>- Devise comptable {pBook.idCAccount}</para>
        /// </summary>
        /// <param name="pIdFCU"></param>
        /// <param name="pBook"></param>
        protected void SetCounterValueData(string pIdFCU, Pre_EarBook pBook)
        {
            SetCounterValueData(null, pIdFCU, pBook);
        }
        /// <summary>
        /// Initialiser les données nécessaires au calcul des contrevaleurs
        /// <para>- Devise comptable {pBook.idCAccount}</para>
        /// <para>- Devise 1 {Currency 1}</para>
        /// <para>- Devise 2 {Currency 2}</para>
        /// </summary>
        /// <param name="pEarDet"></param>
        /// <param name="pBook"></param>
        protected void SetCounterValueData(Pre_EarDet pEarDet, Pre_EarBook pBook)
        {
            SetCounterValueData(pEarDet, string.Empty, pBook);
        }
        /// <summary>
        /// Initialiser les données nécessaires au calcul des contrevaleurs
        /// </summary>
        /// <param name="pEarDet"></param>
        /// <param name="pIdFCU">Devise de flux</param>
        /// <param name="pBook"></param>
        protected void SetCounterValueData(Pre_EarDet pEarDet, string pIdFCU, Pre_EarBook pBook)
        {
            SetCounterValueData(pEarDet, pIdFCU, pBook, DateTime.MinValue);
        }
        /// <summary>
        /// Initialiser les données nécessaires au calcul des contrevaleurs
        /// </summary>
        /// <param name="pEarDet"></param>
        /// <param name="pIdFCU">Devise de flux</param>
        /// <param name="pBook"></param>
        /// <param name="pDtPRSDate">Date de pré-règlement</param>
        protected void SetCounterValueData(Pre_EarDet pEarDet, string pIdFCU, Pre_EarBook pBook, DateTime pDtPRSDate)
        {
            SetCounterValueData(pEarDet, pIdFCU, pBook, pDtPRSDate, DateTime.MinValue);
        }
        /// <summary>
        /// Initialiser les données nécessaires au calcul des contrevaleurs
        /// </summary>
        /// <param name="pEarDet"></param>
        /// <param name="pIdFCU">Devise de flux</param>
        /// <param name="pBook"></param>
        /// <param name="pDtPRSDate">Date de pré-règlement</param>
        /// <param name="pDtValue">Date valeur</param>
        protected void SetCounterValueData(Pre_EarDet pEarDet, string pIdFCU, Pre_EarBook pBook, DateTime pDtPRSDate, DateTime pDtValue)
        {
            m_Pre_EarAmounts.ResetAmounts();
            m_Pre_EarAmounts.IdMarketEnv = pBook.IdMarketEnv;

            string idCU1 = string.Empty;
            string idCU2 = string.Empty;
            DateTime dtValue = pDtValue;
            DateTime dtPRSDate = pDtPRSDate;

            if (pEarDet != null)
            {
                // dtValue
                if (DtFunc.IsDateTimeFilled(dtValue) == false)
                {
                    if (m_TradeInfo.Product.IsFx ||
                        m_TradeInfo.IsETDContext)
                    {
                        dtValue = m_Pre_EarAmounts.DtTransact;
                    }
                    else
                    {
                        DataRow[] eventPRDDAT = m_EventsInfo.DtEvent.Select("EVENTCODE=" +
                            DataHelper.SQLString(EventCodeFunc.Product) + " and EVENTTYPE=" +
                            DataHelper.SQLString(EventTypeFunc.Date) + " and INSTRUMENTNO=" +
                            pEarDet.instrumentNo + " and STREAMNO=0");

                        if (eventPRDDAT != null && eventPRDDAT.Length == 1)
                        {
                            if (m_TradeInfo.Product.IsIRD)
                                dtValue = Convert.ToDateTime(eventPRDDAT[0]["DTSTARTADJ"]);
                            else if (m_TradeInfo.Product.IsFxSwap)
                            {
                                DataRow[] rowEventFirstChild = eventPRDDAT[0].GetChildRows(m_EventsInfo.ChildEvent);
                                if (rowEventFirstChild != null && rowEventFirstChild.Length > 0)
                                    dtValue = Convert.ToDateTime(rowEventFirstChild[0]["DTENDADJ"]);
                            }
                        }
                    }
                }

                // CU1 et CU2
                if (false == m_TradeInfo.IsETDContext)
                    GetCurrencyForFXLeg(pEarDet, out idCU1, out idCU2);
            }

            m_Pre_EarAmounts.SetCounterValuesData(pIdFCU, pBook.IdCAccount, idCU1, idCU2, dtPRSDate, dtValue);
        }
        /// <summary>
        /// Initialiser les données nécessaires au calcul des contrevaleurs
        /// <para>- Calcul de contre valeur ou pas {pIsDtPRSDateToProcess} pour la date de pré-règlement </para>
        /// <para>- Calcul des contres valeurs pour: Date événement, Date transaction, Date valeur, Date de traitement</para>
        /// </summary>
        /// <param name="pIsDtPRSDateToProcess"></param>
        protected void SetCounterValueData(bool pIsDtPRSDateToProcess)
        {
            // RD 20120809 [18070] Optimisation
            if (m_TradeInfo.IsEarExchange)
                m_Pre_EarAmounts.SetCounterValuesData(true, true, true, pIsDtPRSDateToProcess, true);
            else
                m_Pre_EarAmounts.SetCounterValuesData(false, false, false, false, false);
        }
        #region ReadQuote_FXRate
        /// <revision>
        ///     <version>1.1.5</version><date>20070412</date><author>EG</author>
        ///     <EurosysSupport>N° 15435</EurosysSupport>
        ///     <comment>
        ///     Gestion du cours inverse, passage d'un paramètre pIsInverse en référence utilisée par la suite pour appliquer
        ///     un cours inverse à une cotation d'asset
        ///		</comment>
        /// </revision>        
        public EarQuote ReadQuote_FXRate(string pIdC1, string pIdC2, DateTime pFixingDate, string pIdMarketEnv,
            ref bool pIsInverse, bool pWithlog, ref string pKeyException, out bool pIsAlreadySearched)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            pIsAlreadySearched = false;

            EarQuote earQuote = null;
            SpheresException2 quoteException = null;

            pKeyException = string.Empty;
            string key = pIdC1 + pIdC2 + DtFunc.DateTimeToString(pFixingDate.Date, DtFunc.FmtISODate);
            string keyReverse = pIdC2 + pIdC1 + DtFunc.DateTimeToString(pFixingDate.Date, DtFunc.FmtISODate);
            string keyException = key + "EXC";

            pIsInverse = false;
            if (m_CacheQuote.Exists(match => match.First == key))
            {
                earQuote = (EarQuote)m_CacheQuote.Find(match => match.First == key).Second;
                pIsAlreadySearched = true;
            }
            else if (m_CacheQuote.Exists(match => match.First == keyReverse))
            {
                earQuote = (EarQuote)m_CacheQuote.Find(match => match.First == keyReverse).Second;
                pIsAlreadySearched = true;
                keyException = keyReverse + "EXC";
                pIsInverse = true;
            }
            else if (pIdC1 == pIdC2)
            {
                earQuote = new EarQuote(DateTime.MinValue, 0, 1, FpML.Enum.QuoteBasisEnum.Currency1PerCurrency2);
                if (DtFunc.IsDateTimeFilled(pFixingDate))
                {
                    Pair<string, object> item = new Pair<string, object>(key, earQuote);
                    m_CacheQuote.Add(item);
                }
            }
            else
            {
                KeyQuote keyQuote = new KeyQuote(CSTools.SetCacheOn(Cs), pFixingDate, pIdMarketEnv, null, null, QuoteTimingEnum.Close);

                KeyAssetFxRate keyAsset = new KeyAssetFxRate
                {
                    IdC1 = pIdC1,
                    IdC2 = pIdC2
                };
                keyAsset.SetQuoteBasis(true);

                if (DtFunc.IsDateTimeFilled(pFixingDate))
                {
                    // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                    //if (DateTime.Compare(keyQuote.Time.Date, processLog.GetDate()) <= 0)
                    if (DateTime.Compare(keyQuote.Time.Date, OTCmlHelper.GetDateSys(Cs)) <= 0)
                    {
                        SQL_Quote quote = new SQL_Quote(CSTools.SetCacheOn(Cs), QuoteEnum.FXRATE, AvailabilityEnum.NA, m_TradeInfo.UnknownProduct, keyQuote, keyAsset);
                        bool isLoaded = quote.IsLoaded;

                        codeReturn = quote.QuoteValueCodeReturn;

                        if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                            earQuote = new EarQuote(quote.AdjustedTime, quote.IdQuote, quote.IdAsset, quote.QuoteValue,
                                            quote.IdMarketEnv, quote.IdValScenario, keyAsset.QuoteBasis);
                        else
                            earQuote = new EarQuote(quote.AdjustedTime, 0, 0, keyAsset.QuoteBasis, ProcessStateTools.StatusErrorEnum);

                        m_CacheQuote.Add(new Pair<string, object>(key, earQuote));

                        if (ProcessStateTools.IsCodeReturnUnsuccessful(codeReturn))
                        {
                            // Log error message
                            quoteException = new SpheresException2(MethodInfo.GetCurrentMethod().Name, quote.SystemMsgInfo.Identifier,
                                new ProcessState(ProcessStateTools.StatusWarningEnum, codeReturn),
                                quote.SystemMsgInfo.datas);

                            if (pWithlog)
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                                SystemMSGInfo msgInfo = quote.SystemMsgInfo;
                                msgInfo.processState.Status = ProcessStateTools.StatusWarningEnum;
                                
                                Logger.Log(msgInfo.ToLoggerData(2));
                            }

                            m_CacheQuote.Add(new Pair<string, object>(keyException, quoteException));
                            pKeyException = keyException;
                        }
                    }
                    else
                    {
                        // Fixing date in the future
                        earQuote = new EarQuote(DateTime.MinValue, 0, 0, keyAsset.QuoteBasis, ProcessStateTools.StatusErrorEnum);
                        m_CacheQuote.Add(new Pair<string, object>(key, earQuote));
                    }
                }
                else
                {
                    // Fixing date missing
                    earQuote = new EarQuote(DateTime.MinValue, 0, 0, keyAsset.QuoteBasis, ProcessStateTools.StatusErrorEnum);
                    m_CacheQuote.Add(new Pair<string, object>(key, earQuote));
                    pIsAlreadySearched = true;
                }
            }

            return earQuote;
        }
        #endregion ReadQuote_FXRate
        #region GetCurrencyForFXLeg
        /// <summary>
        /// Valoriser les deux devises {pCurrency1} {pCurrency2} selon l'instrument
        /// </summary>
        /// <param name="pEarDet"></param>
        /// <param name="pCurrency1"></param>
        /// <param name="pCurrency2"></param>
        protected void GetCurrencyForFXLeg(Pre_EarDet pEarDet, out string pCurrency1, out string pCurrency2)
        {
            pCurrency1 = string.Empty;
            pCurrency2 = string.Empty;

            // if productMaster is Strategy then no exchange amount in dev1/dev2
            if (0 == pEarDet.instrumentNo)
                return;

            if (m_TradeInfo.IsFx(pEarDet.instrumentNo))
            {
                EFS_TradeLibrary tradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId);
                IProduct currentProduct = (IProduct)ReflectionTools.GetObjectById(tradeLibrary.CurrentTrade, Cst.FpML_InstrumentNo + pEarDet.instrumentNo);

                if (currentProduct.ProductBase.IsFxLeg)
                {
                    IFxLeg product = (IFxLeg)currentProduct;
                    pCurrency1 = product.ExchangedCurrency1.PaymentCurrency;
                    pCurrency2 = product.ExchangedCurrency2.PaymentCurrency;
                }
                else if (currentProduct.ProductBase.IsFxOptionLeg)
                {
                    IFxOptionLeg product = (IFxOptionLeg)currentProduct;
                    pCurrency1 = product.PutCurrencyAmount.Currency;
                    pCurrency2 = product.CallCurrencyAmount.Currency;
                }
                else if (currentProduct.ProductBase.IsFxAverageRateOption)
                {
                    IFxAverageRateOption product = (IFxAverageRateOption)currentProduct;
                    pCurrency1 = product.PutCurrencyAmount.Currency;
                    pCurrency2 = product.CallCurrencyAmount.Currency;
                }
                else if (currentProduct.ProductBase.IsFxBarrierOption)
                {
                    IFxBarrierOption product = (IFxBarrierOption)currentProduct;
                    pCurrency1 = product.PutCurrencyAmount.Currency;
                    pCurrency2 = product.CallCurrencyAmount.Currency;
                }
                else if (currentProduct.ProductBase.IsFxDigitalOption)
                {
                    IFxDigitalOption product = (IFxDigitalOption)currentProduct;
                    pCurrency1 = product.QuotedCurrencyPair.Currency1;
                    pCurrency2 = product.QuotedCurrencyPair.Currency2;
                }
                else if (currentProduct.ProductBase.IsFxSwap)
                {
                    IFxLeg product = ((IFxSwap)currentProduct).FxSingleLeg[pEarDet.streamNo - 1];
                    pCurrency1 = product.ExchangedCurrency1.PaymentCurrency;
                    pCurrency2 = product.ExchangedCurrency2.PaymentCurrency;
                }
            }
        }
        #endregion GetCurrencyForFXLeg
        #region ApplyCounterValue
        // EG 20190114 Add detail to ProcessLog Refactoring
        private EarQuote ApplyCounterValue(string pIdMarketEnv, string pIdC1, string pIdC2, DateTime pFixingDate, decimal pAmountPaid, decimal pAmountReceived,
            ref decimal pExchangeAmountPaid, ref decimal pExchangeAmountReceived, string pIdACU, ref EarQuote pEarQuote1, ref EarQuote pEarQuote2, string pExchangeType)
        {
            EarQuote earQuote = null;
            EarQuote earQuote1 = null;
            EarQuote earQuote2 = null;

            bool isToUsePivotCurrency = (StrFunc.IsFilled(pIdACU) && (pIdC1 != pIdACU) && (pIdC2 != pIdACU));

            bool isInverse = false;
            SpheresException2 retException = null;
            string retExceptionKey = string.Empty;

            earQuote = ReadQuote_FXRate(pIdC1, pIdC2, pFixingDate, pIdMarketEnv, ref isInverse, !isToUsePivotCurrency, ref retExceptionKey, out bool isAlreadySearched);

            if (StrFunc.IsFilled(retExceptionKey))
                retException = (SpheresException2)m_CacheQuote.Find(match => match.First == retExceptionKey).Second;

            if (ProcessStateTools.IsStatusSuccess(earQuote.IdStProcess))
            {
                #region Real Cotation
                GetExchangeAmounts(pIdC1, pIdC2, earQuote.ObservedRate, earQuote.QuoteBasis(isInverse),
                    pAmountPaid, pAmountReceived, ref pExchangeAmountPaid, ref pExchangeAmountReceived, pExchangeType);
                #endregion
            }
            else
            {
                if (isToUsePivotCurrency)
                {
                    #region Pivot cotation
                    string keyExc = pIdC1 + pIdACU + pIdC2 + DtFunc.DateTimeToString(pFixingDate.Date, DtFunc.FmtISODate) + "EXC";
                    string keyExcReverse = pIdC2 + pIdACU + pIdC1 + DtFunc.DateTimeToString(pFixingDate.Date, DtFunc.FmtISODate) + "EXC";
                    //		
                    SpheresException2 retC1Exception = null;
                    string retC1ExceptionKey = string.Empty;
                    //
                    earQuote1 = ReadQuote_FXRate(pIdC1, pIdACU, pFixingDate, pIdMarketEnv, ref isInverse, false, ref retC1ExceptionKey, out isAlreadySearched);
                    //
                    if (ProcessStateTools.IsStatusSuccess(earQuote1.IdStProcess))
                    {
                        #region Cotation of 1st Currency with Accouting Currency
                        GetExchangeAmounts(pIdC1, pIdACU, earQuote1.ObservedRate, earQuote1.QuoteBasis(isInverse),
                            pAmountPaid, pAmountReceived, ref pExchangeAmountPaid, ref pExchangeAmountReceived, pExchangeType);
                        #endregion
                        //		
                        SpheresException2 retC2Exception = null;
                        string retC2ExceptionKey = string.Empty;
                        //
                        earQuote2 = ReadQuote_FXRate(pIdACU, pIdC2, pFixingDate, pIdMarketEnv, ref isInverse, false, ref retC2ExceptionKey, out isAlreadySearched);
                        //
                        if (ProcessStateTools.IsStatusSuccess(earQuote2.IdStProcess))
                        {
                            #region Cotation of Accouting Currency with 2nd Currency
                            GetExchangeAmounts(pIdACU, pIdC2, earQuote2.ObservedRate, earQuote2.QuoteBasis(isInverse),
                                ref pExchangeAmountPaid, ref pExchangeAmountReceived, pExchangeType);
                            #endregion
                        }
                        else if (StrFunc.IsFilled(retC2ExceptionKey))
                        {
                            #region log message
                            retException = (SpheresException2)m_CacheQuote.Find(match => match.First == retExceptionKey).Second;
                            if ((false == m_CacheQuote.Exists(match => match.First == keyExc)) &&
                                (false == m_CacheQuote.Exists(match => match.First == keyExcReverse)))
                            {
                                if ((retException != null) || (retC2Exception != null))
                                    m_CacheQuote.Add(new Pair<string, object>(keyExc, null));

                                if (retException != null)
                                {

                                    // FI 20200623 [XXXXX] AddCriticalException
                                    ProcessState.AddCriticalException(retException);
                                    
                                    Logger.Log(new LoggerData(retException));
                                }

                                retC2Exception = (SpheresException2)m_CacheQuote.Find(match => match.First == retC2ExceptionKey).Second;

                                if (retC2Exception != null)
                                {
                                    // FI 20200623 [XXXXX] AddCriticalException
                                    ProcessState.AddCriticalException(retC2Exception);
                                    
                                    Logger.Log(new LoggerData(retC2Exception));
                                }

                                if (isAlreadySearched == false)
                                {
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5340), 2,
                                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                        new LogParam(pIdACU),
                                        new LogParam(pExchangeType)));
                                }
                            }
                            #endregion
                        }
                    }
                    else if (StrFunc.IsFilled(retC1ExceptionKey))
                    {
                        #region log message
                        if ((false == m_CacheQuote.Exists(match => match.First == keyExc)) &&
                            (false == m_CacheQuote.Exists(match => match.First == keyExcReverse)))
                        {
                            if ((retException != null) || (retC1Exception != null))
                                m_CacheQuote.Add(new Pair<string, object>(keyExc, null));

                            if (retException != null)
                            {
                                // FI 20200623 [XXXXX] AddCriticalException
                                ProcessState.AddCriticalException(retException);
                                Logger.Log(new LoggerData(retException));
                            }

                            retC1Exception = (SpheresException2)m_CacheQuote.Find(match => match.First == retC1ExceptionKey).Second;

                            if (retC1Exception != null)
                            {
                                // FI 20200623 [XXXXX] AddCriticalException
                                ProcessState.AddCriticalException(retC1Exception);

                                Logger.Log(new LoggerData(retException));
                            }

                            if (isAlreadySearched == false)
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5340), 2,
                                    new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                    new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                    new LogParam(pIdACU),
                                    new LogParam(pExchangeType)));
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                else if (isAlreadySearched == false)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5339), 2,
                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                        new LogParam(pExchangeType)));
                    
                }
            }
            //
            pEarQuote1 = earQuote1;
            pEarQuote2 = earQuote2;
            //
            return earQuote;
        }
        //
        private void GetExchangeAmounts(string pCurrency1, string pCurrency2, Decimal pObservedRate, FpML.Enum.QuoteBasisEnum pQuoteBasis,
            ref decimal pAmountPaid, ref decimal pAmountReceived, string pExchangeType)
        {
            GetExchangeAmounts(pCurrency1, pCurrency2, pObservedRate, pQuoteBasis,
            pAmountPaid, pAmountReceived, ref pAmountPaid, ref pAmountReceived, pExchangeType);
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        private void GetExchangeAmounts(string pCurrency1, string pCurrency2, Decimal pObservedRate, FpML.Enum.QuoteBasisEnum pQuoteBasis,
            decimal pAmountPaid, decimal pAmountReceived, ref decimal pExchangeAmountPaid, ref decimal pExchangeAmountReceived, string pExchangeType)
        {
            CurrencyCashInfo currency1CashInfo = null;
            CurrencyCashInfo currency2CashInfo = null;

            try
            {
                if (m_CacheCurrency.ContainsKey(pCurrency1))
                    currency1CashInfo = (CurrencyCashInfo)m_CacheCurrency[pCurrency1];
                else
                {
                    currency1CashInfo = new CurrencyCashInfo(Cs, pCurrency1);
                    m_CacheCurrency.Add(pCurrency1, currency1CashInfo);
                }

                if (m_CacheCurrency.ContainsKey(pCurrency2))
                    currency2CashInfo = (CurrencyCashInfo)m_CacheCurrency[pCurrency2];
                else
                {
                    currency2CashInfo = new CurrencyCashInfo(Cs, pCurrency2);
                    m_CacheCurrency.Add(pCurrency2, currency2CashInfo);
                }
            }
            catch (SpheresException2) { throw; }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05351", ex,
                            pExchangeType, pCurrency1 + " / " + pCurrency2,
                            m_LstMasterData.Find(match => match.First == "TRADE").Second,
                            m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                            m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second);
            }
            //
            GetExchangeAmounts(currency1CashInfo, currency2CashInfo, pObservedRate, pQuoteBasis,
                pAmountPaid, pAmountReceived, ref pExchangeAmountPaid, ref pExchangeAmountReceived);
        }
        private void GetExchangeAmounts(CurrencyCashInfo pCurrency1CashInfo, CurrencyCashInfo pCurrency2CashInfo, Decimal pObservedRate, FpML.Enum.QuoteBasisEnum pQuoteBasis,
            decimal pAmountPaid, decimal pAmountReceived, ref decimal pExchangeAmountPaid, ref decimal pExchangeAmountReceived)
        {
            EFS_Cash cash = null;
            pExchangeAmountPaid = 0;
            pExchangeAmountReceived = 0;

            if (pCurrency1CashInfo.Currency != pCurrency2CashInfo.Currency)
            {
                if (pAmountPaid != 0)
                {
                    cash = new EFS_Cash(Cs, pCurrency1CashInfo, pCurrency2CashInfo, pAmountPaid, pObservedRate, pQuoteBasis);
                    pExchangeAmountPaid = cash.ExchangeAmountRounded;
                }

                if (pAmountReceived != 0)
                {
                    if (pAmountPaid != pAmountReceived)
                        cash = new EFS_Cash(Cs, pCurrency1CashInfo, pCurrency2CashInfo, pAmountReceived, pObservedRate, pQuoteBasis);
                    pExchangeAmountReceived = cash.ExchangeAmountRounded;
                }
            }
            else
            {
                if (pAmountPaid != 0)
                {
                    cash = new EFS_Cash(Cs, pAmountPaid, pCurrency1CashInfo);
                    pExchangeAmountPaid = cash.AmountRounded;
                }

                if (pAmountReceived != 0)
                {
                    if (pAmountPaid != pAmountReceived)
                        cash = new EFS_Cash(Cs, pAmountReceived, pCurrency2CashInfo);
                    pExchangeAmountReceived = cash.AmountRounded;
                }
            }
        }
        #endregion ApplyCounterValue

        #region GetPreEarAmount
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchangeType"></param>
        /// <param name="pPre_EarAmounts"></param>
        /// <returns></returns>
        private Pre_EarAmount GetPreEarAmount(ExchangeTypeEnum pExchangeType, Pre_EarAmounts pPre_EarAmounts)
        {
            string exchangeTypeIdC = pPre_EarAmounts.GetExchangeTypeIDC(pExchangeType);
            Pre_EarAmount ret = new Pre_EarAmount(pExchangeType.ToString(), exchangeTypeIdC);
            //
            if (ExchangeTypeFunc.IsFlowCurrency(pExchangeType.ToString()))
            {
                ret.IdStProcess = ProcessStateTools.StatusSuccessEnum;

                // RD 20121108 / Conserver dans les EARxxxAMOUNT toutes les décimales significatives pour les FCU
                // 1- Mise en commentaire du code ci-dessous
                // -------------------------------------------------------------------------------------------------
                //if (StrFunc.IsFilled(exchangeTypeIdC))
                //{
                //    // RD 20111024 Application de la règle d’arrondi de la devise  
                //    // PL 20111026 Refactoring 
                //    if (pPre_EarAmounts.Paid > Decimal.Zero)
                //    {
                //        EFS_Cash cash = new EFS_Cash(CSTools.SetCacheOn(Cs), pPre_EarAmounts.Paid, exchangeTypeIdC);
                //        earAmount.paid = cash.AmountRounded;
                //    }
                //    if (pPre_EarAmounts.Received > Decimal.Zero)
                //    {
                //        EFS_Cash cash = new EFS_Cash(CSTools.SetCacheOn(Cs), pPre_EarAmounts.Received, exchangeTypeIdC);
                //        earAmount.received = cash.AmountRounded;
                //    }
                //}
                //else
                //{
                //    earAmount.paid = pPre_EarAmounts.Paid;
                //    earAmount.received = pPre_EarAmounts.Received;
                //}
                // -------------------------------------------------------------------------------------------------
                // 2- Utiliser les montants avec toutes les décimales significatives pour les FCU
                ret.Paid = pPre_EarAmounts.Paid;
                ret.Received = pPre_EarAmounts.Received;
            }
            else
            {
                // Pour ne pas calculer de contre valeur, si la devise du flux principal est vide.
                string flowCurrency = pPre_EarAmounts.GetExchangeTypeIDC(ExchangeTypeEnum.FCU);
                if (StrFunc.IsEmpty(flowCurrency))
                    return null;

                // Pour ne pas calculer de contrevaleur, si la date est vide.
                // RD 20150806 - calculer la contrevaleur si le devise du flux est égale à à la devise de contrevaleur
                DateTime exchangeTypeDate = pPre_EarAmounts.GetExchangeTypeDate(pExchangeType);
                if (DtFunc.IsDateTimeEmpty(exchangeTypeDate) && exchangeTypeIdC != pPre_EarAmounts.IdFCU)
                    return null;

                // Pour calculer la contre valeur qui est pertinente
                if ((pPre_EarAmounts.IsExchangeTypeDateToProcess(pExchangeType) == false))
                    return null;

                decimal accountingAmountPaid = 0;
                decimal accountingAmountReceived = 0;
                EarQuote earQuote1 = null;
                EarQuote earQuote2 = null;
                //
                EarQuote earQuote = ApplyCounterValue(pPre_EarAmounts.IdMarketEnv, pPre_EarAmounts.IdFCU,
                    exchangeTypeIdC, exchangeTypeDate.Date, pPre_EarAmounts.Paid, pPre_EarAmounts.Received,
                    ref accountingAmountPaid, ref accountingAmountReceived, pPre_EarAmounts.IdACU,
                    ref earQuote1, ref earQuote2, pExchangeType.ToString());
                //
                ret.IdStProcess = earQuote.IdStProcess;
                ret.FxDate = earQuote.AdjustedDate;
                ret.ExchangeDate = exchangeTypeDate.Date;
                ret.IdMarketEnv = pPre_EarAmounts.IdMarketEnv;
                //							
                if (earQuote1 == null)
                {
                    if (ProcessStateTools.IsStatusSuccess(ret.IdStProcess))
                    {
                        ret.Paid = accountingAmountPaid;
                        ret.Received = accountingAmountReceived;
                        //
                        ret.IdQuote_H = earQuote.IdQuote;
                        ret.IdMarketEnv = earQuote.IdMarketEnv;
                        ret.IdValScenario = earQuote.IdValScenario;
                        ret.IdAsset = earQuote.IdAsset;
                        ret.FxValue = earQuote.ObservedRate;
                    }
                }
                else
                {
                    ret.IdStProcess = earQuote1.IdStProcess;
                    //
                    if (ProcessStateTools.IsStatusSuccess(earQuote1.IdStProcess))
                    {
                        ret.IdQuote_H = earQuote1.IdQuote;
                        ret.IdMarketEnv = earQuote1.IdMarketEnv;
                        ret.IdValScenario = earQuote1.IdValScenario;
                        ret.IdAsset = earQuote1.IdAsset;
                        ret.FxValue = earQuote1.ObservedRate;
                        //
                        if (earQuote2 != null)
                        {
                            ret.IdStProcess = earQuote2.IdStProcess;
                            //
                            if (ProcessStateTools.IsStatusSuccess(earQuote2.IdStProcess))
                            {
                                ret.Paid = accountingAmountPaid;
                                ret.Received = accountingAmountReceived;
                                //
                                ret.IdQuote_H2 = earQuote2.IdQuote;
                                ret.IdMarketEnv2 = earQuote2.IdMarketEnv;
                                ret.IdValScenario2 = earQuote2.IdValScenario;
                                ret.IdAsset2 = earQuote2.IdAsset;
                                ret.FxValue2 = earQuote2.ObservedRate;
                            }
                        }
                        else
                            ret.IdStProcess = ProcessStateTools.StatusErrorEnum;
                    }
                }
            }
            //
            return ret;
        }
        #endregion
        #region AddEventDateAmount
        public void AddEventDateAmount(DateTime pDtEvent)
        {
            AddEventDateAmount(pDtEvent, ref m_Pre_EarAmounts);
        }
        public void AddEventDateAmount(DateTime pDtEvent, ref Pre_EarAmounts pPre_EarAmounts)
        {
            pPre_EarAmounts.DtEvent = pDtEvent;

            // ACUEventDateAmount
            pPre_EarAmounts.ACUEventDateAmount = GetPreEarAmount(ExchangeTypeEnum.ACU_EVENTDATE, pPre_EarAmounts);

            if (StrFunc.IsFilled(pPre_EarAmounts.IdCU1) && StrFunc.IsFilled(pPre_EarAmounts.IdCU2))
            {
                // CU1EventDateAmount
                pPre_EarAmounts.CU1EventDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU1_EVENTDATE, pPre_EarAmounts);
                // CU2EventDateAmount
                pPre_EarAmounts.CU2EventDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU2_EVENTDATE, pPre_EarAmounts);
            }
        }
        #endregion AddEventDateAmount
        #region NewEarAmounts
        public void NewEarAmounts(bool pIsPaid, decimal pAmount)
        {
            NewEarAmounts(pIsPaid, pAmount, ref m_Pre_EarAmounts);
        }
        public void NewEarAmounts(bool pIsPaid, decimal pAmount, ref Pre_EarAmounts pPre_EarAmounts)
        {
            if (pIsPaid)
                NewEarAmounts(pAmount, 0, ref pPre_EarAmounts);
            else
                NewEarAmounts(0, pAmount, ref pPre_EarAmounts);
        }
        public void NewEarAmounts(decimal pAmount, DateTime pDtEvent)
        {
            NewEarAmounts(pAmount, 0, pDtEvent);
        }
        public void NewEarAmounts(decimal pAmountPaid, decimal pAmountReceived, string pIdFCU)
        {
            NewEarAmounts(pAmountPaid, pAmountReceived, pIdFCU, DateTime.MinValue);
        }
        public void NewEarAmounts(decimal pAmountPaid, decimal pAmountReceived, string pIdFCU, DateTime pDtEvent, DateTime pDtPRSDate)
        {
            m_Pre_EarAmounts.DtPRSDate = pDtPRSDate;
            //
            NewEarAmounts(pAmountPaid, pAmountReceived, pIdFCU, pDtEvent);
        }
        public void NewEarAmounts(decimal pAmountPaid, decimal pAmountReceived, string pIdFCU, DateTime pDtEvent)
        {
            m_Pre_EarAmounts.IdFCU = pIdFCU;
            //
            NewEarAmounts(pAmountPaid, pAmountReceived, pDtEvent);
        }
        public void NewEarAmounts(decimal pAmountPaid, decimal pAmountReceived, DateTime pDtEvent, DateTime pDtPRSDate)
        {
            m_Pre_EarAmounts.DtPRSDate = pDtPRSDate;
            //
            NewEarAmounts(pAmountPaid, pAmountReceived, pDtEvent);
        }
        public void NewEarAmounts(decimal pAmountPaid, decimal pAmountReceived, DateTime pDtEvent)
        {
            NewEarAmounts(pAmountPaid, pAmountReceived);
            //
            if (DtFunc.IsDateTimeFilled(pDtEvent))
                AddEventDateAmount(pDtEvent);
        }
        public void NewEarAmounts(decimal pAmountPaid, decimal pAmountReceived)
        {
            NewEarAmounts(pAmountPaid, pAmountReceived, ref m_Pre_EarAmounts);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmountPaid"></param>
        /// <param name="pAmountReceived"></param>
        /// <param name="pPre_EarAmounts"></param>
        public void NewEarAmounts(decimal pAmountPaid, decimal pAmountReceived, ref Pre_EarAmounts pPre_EarAmounts)
        {
            pPre_EarAmounts.SetCounterValuesData(pAmountPaid, pAmountReceived);
            //			
            #region FCU amount
            pPre_EarAmounts.FCUAmount = GetPreEarAmount(ExchangeTypeEnum.FCU, pPre_EarAmounts);
            #endregion
            #region ACU amounts
            // ACUEarDateAmount
            pPre_EarAmounts.ACUEarDateAmount = GetPreEarAmount(ExchangeTypeEnum.ACU_EARDATE, pPre_EarAmounts);
            // ACUTransactDateAmount
            pPre_EarAmounts.ACUTransactDateAmount = GetPreEarAmount(ExchangeTypeEnum.ACU_TRANSACTDATE, pPre_EarAmounts);
            // ACUPRSDateAmount
            pPre_EarAmounts.ACUPRSDateAmount = GetPreEarAmount(ExchangeTypeEnum.ACU_PRSDATE, pPre_EarAmounts);
            // ACUValueDateAmount
            pPre_EarAmounts.ACUValueDateAmount = GetPreEarAmount(ExchangeTypeEnum.ACU_VALUEDATE, pPre_EarAmounts);
            #endregion ACU amounts
            if (StrFunc.IsFilled(pPre_EarAmounts.IdCU1) && StrFunc.IsFilled(pPre_EarAmounts.IdCU2))
            {
                #region CU1 amounts
                // CU1EarDateAmount
                pPre_EarAmounts.CU1EarDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU1_EARDATE, pPre_EarAmounts);
                // CU1TransactDateAmount
                pPre_EarAmounts.CU1TransactDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU1_TRANSACTDATE, pPre_EarAmounts);
                // CU1PRSDateAmount
                pPre_EarAmounts.CU1PRSDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU1_PRSDATE, pPre_EarAmounts);
                // CU1ValueDateAmount
                pPre_EarAmounts.CU1ValueDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU1_VALUEDATE, pPre_EarAmounts);
                #endregion
                #region CU2 amounts
                // CU2EarDateAmount
                pPre_EarAmounts.CU2EarDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU2_EARDATE, pPre_EarAmounts);
                // CU2TransactDateAmount
                pPre_EarAmounts.CU2TransactDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU2_TRANSACTDATE, pPre_EarAmounts);
                // CU2PRSDateAmount
                pPre_EarAmounts.CU2PRSDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU2_PRSDATE, pPre_EarAmounts);
                // CU2ValueDateAmount
                pPre_EarAmounts.CU2ValueDateAmount = GetPreEarAmount(ExchangeTypeEnum.CU2_VALUEDATE, pPre_EarAmounts);
                #endregion
            }
        }
        #endregion NewEarAmounts
        #region AddAmountsToEar
        /// <summary>
        /// Ajoute les montans présents dans  m_Pre_EarAmounts dans {pEarSource}
        /// </summary>
        public void AddAmountsToEar(Pre_EarBase pEarSource)
        {
            AddAmountsToEar(pEarSource, m_Pre_EarAmounts);
        }
        /// <summary>
        /// Ajoute les montans présents dans  pPre_EarAmounts dans {pEarSource}
        /// </summary>
        /// <param name="pEarSource"></param>
        /// <param name="pPre_EarAmounts"></param>
        private static void AddAmountsToEar(Pre_EarBase pEarSource, Pre_EarAmounts pPre_EarAmounts)
        {

            pEarSource.Add(pPre_EarAmounts.FCUAmount);

            if (false == pEarSource.IsAgAmountsWithCounterValue)
            {
                // ACUEarDateAmount
                pEarSource.Add(pPre_EarAmounts.ACUEarDateAmount);

                // ACUEventDateAmount
                pEarSource.Add(pPre_EarAmounts.ACUEventDateAmount);

                //ACUTransactDateAmount
                pEarSource.Add(pPre_EarAmounts.ACUTransactDateAmount);

                //ACUPRSDateAmount
                pEarSource.Add(pPre_EarAmounts.ACUPRSDateAmount);

                //ACUValueDateAmount
                pEarSource.Add(pPre_EarAmounts.ACUValueDateAmount);

                if (StrFunc.IsFilled(pPre_EarAmounts.IdCU1) && StrFunc.IsFilled(pPre_EarAmounts.IdCU2))
                {
                    #region Add Currency1 amount
                    // CU1EarDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU1EarDateAmount);

                    // CU1EventDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU1EventDateAmount);

                    // CU1TransactDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU1TransactDateAmount);

                    // CU1PRSDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU1PRSDateAmount);

                    // CU1ValueDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU1ValueDateAmount);
                    #endregion  Add Currency1 amount
                    #region Add Currency2 amount
                    // CU2EarDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU2EarDateAmount);

                    // CU2EventDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU2EventDateAmount);

                    // CU2TransactDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU2TransactDateAmount);

                    // CU2PRSDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU2PRSDateAmount);

                    // CU2ValueDateAmount
                    pEarSource.Add(pPre_EarAmounts.CU2ValueDateAmount);
                    #endregion Add Currency2 amount
                }
            }
        }
        #endregion AddAmountsToEar

        /// <summary>
        /// Initialiser les paramètres à utiliser dans le process de la génération des EARs
        /// </summary>
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        protected void InitEarQueries()
        {
            m_EarQuery = new EarQuery();

            #region Ear
            m_EarQuery.IdEAR = new DataParameter(Cs, "IDEAR", DbType.Int32).DbDataParameter;
            m_EarQuery.IdT = new DataParameter(Cs, "IDT", DbType.Int32).DbDataParameter;
            m_EarQuery.IdT.Value = CurrentId;
            m_EarQuery.IdB = new DataParameter(Cs, "IDB", DbType.Int32).DbDataParameter;
            m_EarQuery.DtEar = new DataParameter(Cs, "DTEAR", DbType.Date).DbDataParameter; // FI 20201006 [XXXXX] DbType.Date
            m_EarQuery.DtEvent = new DataParameter(Cs, "DTEVENT", DbType.Date).DbDataParameter; // FI 20201006 [XXXXX] DbType.Date
            m_EarQuery.DtRemoved = new DataParameter(Cs, "DTREMOVED", DbType.Date).DbDataParameter; // FI 20201006 [XXXXX] DbType.Date
            m_EarQuery.IdAIns = new DataParameter(Cs, "IDAINS", DbType.Int32).DbDataParameter;
            m_EarQuery.Source = new DataParameter(Cs, "SOURCE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN).DbDataParameter;
            m_EarQuery.ExtLink = new DataParameter(Cs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN).DbDataParameter;
            m_EarQuery.DtAccount = new DataParameter(Cs, "DTACCOUNT", DbType.Date).DbDataParameter; // FI 20201006 [XXXXX] DbType.Date
            m_EarQuery.IdStActivation = new DataParameter(Cs, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN).DbDataParameter;
            #endregion Ear
            #region EarDet
            m_EarQuery.InstrumentNo = new DataParameter(Cs, "INSTRUMENTNO", DbType.Int32).DbDataParameter;
            m_EarQuery.StreamNo = new DataParameter(Cs, "STREAMNO", DbType.Int32).DbDataParameter;
            #endregion EarDet
            #region EarDay
            m_EarQuery.IdEARDAY = new DataParameter(Cs, "IDEARDAY", DbType.Int32).DbDataParameter;
            m_EarQuery.IdE = new DataParameter(Cs, "IDE", DbType.Int32).DbDataParameter;
            m_EarQuery.IdEC = new DataParameter(Cs, "IDEC", DbType.Int32).DbDataParameter;
            m_EarQuery.EarCode = new DataParameter(Cs, "EARCODE", DbType.AnsiString, SQLCst.UT_EARCODE_LEN).DbDataParameter;
            m_EarQuery.EventCode = new DataParameter(Cs, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN).DbDataParameter;
            m_EarQuery.EventClass = new DataParameter(Cs, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN).DbDataParameter;
            m_EarQuery.EventType = new DataParameter(Cs, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN).DbDataParameter;
            m_EarQuery.AmountType = new DataParameter(Cs, "AMOUNTTYPE", DbType.AnsiString, SQLCst.UT_AMOUNTTYPE_LEN).DbDataParameter;
            #endregion EarDay
            #region EarCommon
            m_EarQuery.IdEARCOMMON = new DataParameter(Cs, "IDEARCOMMON", DbType.Int32).DbDataParameter;
            #endregion EarCommon
            #region EarCalc
            m_EarQuery.IdEARCALC = new DataParameter(Cs, "IDEARCALC", DbType.Int32).DbDataParameter;
            m_EarQuery.CalcType = new DataParameter(Cs, "CALCTYPE", DbType.AnsiString).DbDataParameter;
            m_EarQuery.SequenceNo = new DataParameter(Cs, "SEQUENCENO", DbType.Int32).DbDataParameter;
            #endregion EarCalc
            #region EarNom
            m_EarQuery.IdEARNOM = new DataParameter(Cs, "IDEARNOM", DbType.Int32).DbDataParameter;
            #endregion EarNom
            #region EarNomAmount
            m_EarQuery.Amount = new DataParameter(Cs, "AMOUNT", DbType.Decimal).DbDataParameter;
            #endregion EarNomAmount
            #region EarAmount
            m_EarQuery.ExchangeType = new DataParameter(Cs, "EXCHANGETYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN).DbDataParameter;
            m_EarQuery.IdC = new DataParameter(Cs, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN).DbDataParameter;
            m_EarQuery.Paid = new DataParameter(Cs, "PAID", DbType.Decimal).DbDataParameter;
            m_EarQuery.Received = new DataParameter(Cs, "RECEIVED", DbType.Decimal).DbDataParameter;
            m_EarQuery.IdQuote_H = new DataParameter(Cs, "IDQUOTE_H", DbType.Int32).DbDataParameter;
            m_EarQuery.IdQuote_H2 = new DataParameter(Cs, "IDQUOTE_H2", DbType.Int32).DbDataParameter;
            m_EarQuery.IdStProcess = new DataParameter(Cs, "IDSTPROCESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN).DbDataParameter;
            #endregion EarAmount

            

            m_EarQuery.ConstEventCode = new DataParameter(Cs, "CONSTEVENTCODE", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN) { Value = "EventCode"}.DbDataParameter;
            m_EarQuery.ConstEventType = new DataParameter(Cs, "CONSTEVENTTYPE", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN) { Value = "EventType" }.DbDataParameter;
            m_EarQuery.ConstEventClass = new DataParameter(Cs, "CONSTEVENTCLASS", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN) { Value = "EventClass" }.DbDataParameter;
            m_EarQuery.ConstALL = new DataParameter(Cs, "CONSTALL", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN) { Value = Cst.EarCommonEnum.ALL.ToString() }.DbDataParameter;
            m_EarQuery.ConstOTC = new DataParameter(Cs, "CONSTOTC", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN) { Value = Cst.EarCommonEnum.OTC.ToString() }.DbDataParameter;
            m_EarQuery.ConstETD = new DataParameter(Cs, "CONSTETD", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN) { Value = Cst.EarCommonEnum.ETD.ToString() }.DbDataParameter;
            m_EarQuery.ConstTrue = new DataParameter(Cs, "CONSTTRUE", DbType.Boolean) { Value = true }.DbDataParameter;
            m_EarQuery.ConstRemoved = new DataParameter(Cs, "REMOVED", DbType.AnsiString, SQLCst.UT_STATUS_LEN) { Value = Cst.StatusActivation.REMOVED }.DbDataParameter;
            m_EarQuery.GProduct = new DataParameter(Cs, "GPRODUCT", DbType.String) { Value = m_TradeInfo.Product.GProduct }.DbDataParameter;

            // FI 20200820 [25468] dates systemes en UTC
            string dtSys = DataHelper.SQLGetDate(Cs,true);

            m_EarQuery.m_SQLQueryEar = m_EarQuery.m_SQLQueryEar.Replace("@DTSYS", dtSys);
            m_EarQuery.m_SQLQueryEar = m_EarQuery.m_SQLQueryEar.Replace("@DTINS", dtSys);

            m_EarQuery.m_SQLQueryEarCalcEvent = m_EarQuery.m_SQLQueryEarCalcEvent.Replace("@DTINS", dtSys);
            m_EarQuery.m_SQLQueryEarNomEvent = m_EarQuery.m_SQLQueryEarNomEvent.Replace("@DTINS", dtSys);
        }

        /// <summary>
        /// Supprime de la DB l'EAR 
        /// </summary>
        /// <param name="pRowEar"></param>
        private void DeleteTodayEar(DataRow pRowEar)
        {
            int idEar = Convert.ToInt32(pRowEar["IDEAR"]);
            int idb = Convert.ToInt32(pRowEar["IDB"]);
            string bookIdentifier = (Convert.IsDBNull(pRowEar["BOOK_IDENTIFIER"]) ? string.Empty : pRowEar["BOOK_IDENTIFIER"].ToString());

            try
            {
                string sqlQuery = SQLCst.DELETE_DBO + Cst.OTCml_TBL.EAR.ToString() + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "IDEAR = " + idEar.ToString() + Cst.CrLf;
                int nbRows = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlQuery);

                if (nbRows > 0)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 5395), 1,
                        new LogParam(idEar),
                        new LogParam(LogTools.IdentifierAndId(bookIdentifier, idb)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_EarDate)),
                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second)));
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Number == 547) // integrity constraint
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05337",
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.MESSAGE_DEL_ERROR),
                        idEar.ToString(),
                        LogTools.IdentifierAndId(bookIdentifier, idb),
                        DtFunc.DateTimeToStringDateISO(m_EarDate),
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second);

                }
                else
                {
                    SpheresException2 exLog = SpheresExceptionParser.GetSpheresException(null, ex);

                    // FI 20200623 [XXXXX] AddCriticalException
                    ProcessState.AddCriticalException(exLog);
                    Logger.Log(new LoggerData(exLog));

                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05338",
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE),
                        idEar.ToString(),
                        LogTools.IdentifierAndId(bookIdentifier, idb),
                        DtFunc.DateTimeToStringDateISO(m_EarDate),
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("ORA-02292")) // integrity constraint
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05337",
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.MESSAGE_DEL_ERROR),
                        idEar.ToString(),
                        LogTools.IdentifierAndId(bookIdentifier, idb),
                        DtFunc.DateTimeToStringDateISO(m_EarDate),
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second);
                }
                else
                {
                    SpheresException2 exLog = SpheresExceptionParser.GetSpheresException(null, ex);

                    ProcessState.AddCriticalException(exLog);
                    
                    Logger.Log(new LoggerData(exLog));

                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05338",
                        idEar.ToString(),
                        LogTools.IdentifierAndId(bookIdentifier, idb),
                        DtFunc.DateTimeToStringDateISO(m_EarDate),
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second);
                }
            }
        }

        /// <summary>
        /// Collecter la liste des Books candidats à génération d'EARs
        /// <para>- Ne pas générer d'EAR pour les Books de l'Acheteur ou Vendeur, Non Dealer (FixParty différent de 27)</para>
        /// <para>- Na pas générer d'EAR pour les Books non gérés (Entité comptable avec une devise comptable)</para>
        /// </summary>
        // EG 20180606 [23979] IRQ (EARGEN)
        protected Cst.ErrLevel SetBookCandidateToEar()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

            bool isETDAlloc = m_TradeInfo.IsETDAlloc;
            int idB = 0;
            m_CacheBook = new Hashtable();

            foreach (DataRow party in m_TradeInfo.DtTradeActor.Rows)
            {
                if (IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn))
                    break;

                bool isBookToAccount = true;

                if (isETDAlloc && (false == Convert.IsDBNull(party["BUYER_SELLER"])))
                {
                    string fixPartyRole = (Convert.IsDBNull(party["FIXPARTYROLE"]) ? string.Empty : party["FIXPARTYROLE"].ToString());

                    if (fixPartyRole != "27")
                        isBookToAccount = false;
                }

                if (isBookToAccount)
                {
                    idB = Convert.ToInt32(party["IDB"]);
                    // Vérifier si le Book n'a pas été déjà recensé
                    if (m_CacheBook.ContainsKey(idB.ToString()))
                        isBookToAccount = false;
                }

                // Vérifier si le Book est géré                
                if (isBookToAccount)
                {
                    string book_Identifier = party["BOOK_IDENTIFIER"].ToString();
                    int book_IdA_Entity = Convert.ToInt32(party["IDA_ENTITY"]);

                    SQL_Actor sql_ActorEntity = new SQL_Actor(CSTools.SetCacheOn(Cs), book_IdA_Entity)
                    {
                        WithInfoEntity = true
                    };
                    if (sql_ActorEntity.LoadTable(new string[] { "ACTOR.IDENTIFIER", "ent.IDA as IDA1", "ent.IDMARKETENV", "ent.IDCACCOUNT" }))
                    {
                        if (sql_ActorEntity.IsEntityExist)
                        {
                            if (StrFunc.IsFilled(sql_ActorEntity.IdCAccount))
                            {
                                m_CacheBook.Add(idB.ToString(),
                                    new Pre_EarBook(idB, book_Identifier, book_IdA_Entity,
                                        sql_ActorEntity.Identifier, sql_ActorEntity.IdCAccount, sql_ActorEntity.IdMarketEnv,
                                        DateTime.MinValue, party["IDENTIFIER"].ToString()));
                            }
                            else
                            {
                                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05333",
                                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                                    LogTools.IdentifierAndId(sql_ActorEntity.Identifier, book_IdA_Entity),
                                    LogTools.IdentifierAndId(book_Identifier, idB));

                            }
                        }
                        else
                        {
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05334",
                                new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                                    LogTools.IdentifierAndId(sql_ActorEntity.Identifier, book_IdA_Entity),
                                    LogTools.IdentifierAndId(book_Identifier, idB));
                        }
                    }
                }
            }

            if (0 == m_CacheBook.Count)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05332",
                    new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.NOBOOKMANAGED),
                            m_LstMasterData.Find(match => match.First == "TRADE").Second,
                            m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second,
                            m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second);
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return codeReturn;
        }

        protected override void ProcessPreExecute()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " ------------------------");
#endif

            ProcessState.CodeReturn = Cst.ErrLevel.SUCCESS;

            // RD 20120809 [18070] Optimisation
            //CheckLicense();

            if (false == IsProcessObserver)
            {
#if DEBUG
                diagnosticDebug.Start("TradeInfo");
#endif
                m_TradeInfo = new EarTradeInfo(Cs, CurrentId);
#if DEBUG
                diagnosticDebug.End("TradeInfo");
#endif

                if (m_TradeInfo.DtTrade == null)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        StrFunc.AppendFormat("Trade n°{0} not found", CurrentId.ToString()),
                        new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum));
                }
                //
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn) && (false == NoLockCurrentId))
                    ProcessState.CodeReturn = LockCurrentObjectId();
                //
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                    ProcessState.CodeReturn = ScanCompatibility_Trade(CurrentId);
#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "------------------------------------------");
#endif
            }
        }


        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180606 [23979] IRQ (EARGEN)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " -------------------");
#endif

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            try
            {
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5331), 0,
                    new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                    new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                    new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                if (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn))
                {
                    InitEarQueries();

#if DEBUG
                    diagnosticDebug.Start("LoadTradeEvents");
#endif
                    if (ProcessTuningSpecified)
                        m_EventsInfo = new EarEventInfo(Cs, CurrentId, m_TradeInfo.Product.GProduct, ProcessTuning.GetProcessTuningOutput(Tuning.TuningOutputTypeEnum.OES));
                    else
                        m_EventsInfo = new EarEventInfo(Cs, CurrentId, m_TradeInfo.Product.GProduct, null);
#if DEBUG
                    diagnosticDebug.End("LoadTradeEvents");
#endif
                }

                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                    (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                {
                    if (m_TradeInfo.IsDeactiv)
                    {
                        // Annulation des EARs du Trade                       
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5397), 1,
                            new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                            new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                            new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                        codeReturn = EarCancellation();
                    }
                    else
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5340), 1,
                            new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                            new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                            new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                        SetTradeDtTransaction();
                        codeReturn = SetBookCandidateToEar();

                        if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) && 
                            (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            SetIsNativeAccRemove();
                            SetCounterValueData();
                            codeReturn = SetEventsCandidateToEarDay();
                        }

                        // Phase 1 - Calc EarDay                        
                        if (ProcessStateTools.IsCodeReturnSuccess(codeReturn) && 
                            (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5360), 1,
                                new LogParam("EARDAY"),
                                new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                            // PRSDATE counter value is to process for EarDay
                            SetCounterValueData(m_TradeInfo.IsEarExchange == true);
                            codeReturn = CalcEarDay();
                        }

                        // Phase 2 - Calc EarNOM
                        if (ProcessStateTools.IsCodeReturnSuccess(codeReturn) &&
                            (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            // RD 20120809 [18070] Optimisation
                            if (false == m_TradeInfo.IsETDContext)
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5360), 1,
                                    new LogParam("EARNOM"),
                                    new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                    new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                    new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                                // PRSDATE counter value is not to process for EarNom
                                SetCounterValueData(false);
                                codeReturn = CalcEarNOM();
                            }
                        }

                        if (ProcessStateTools.IsCodeReturnSuccess(codeReturn) &&
                            (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            IDbConnection dbConnection = null;
                            DataDbTransaction transaction = null;

                            try
                            {
                                dbConnection = DataHelper.OpenConnection(Cs);
                                transaction = new DataDbTransaction(Cs, DataHelper.BeginTran(Cs));

                                // Phase 3 - Write to DB Ear/EarDet/EarDay/EarNom
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5390), 1,
                                    new LogParam("EAR / EARDAY / EARNOM"),
                                    new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                    new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                    new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                                codeReturn = WriteEarAndEarDayAndEarNom(transaction);

                                // Phase 4 - Calc EarCommon
                                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn) && 
                                    (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                {
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5360), 1,
                                        new LogParam("EARCOMMON"),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                                    // PRSDATE counter value is to process for EarCommon
                                    SetCounterValueData(true);
                                    codeReturn = CalcEarCommon(transaction);
                                }

                                // Phase 5 - Calc EarCommon for LastEar (CLO-1,LPC-1)
                                // EG 20091110 IsTradeAdmin Test
                                if ((ProcessStateTools.IsCodeReturnSuccess(codeReturn) && (false == IsTradeAdmin)) && 
                                    (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                {
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5360), 1,
                                        new LogParam("EARCOMMON-1"),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                                    codeReturn = CalcEarCommonForLastEar(transaction);
                                }

                                // Phase 6 - Write to DB EarCommon
                                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn) &&
                                    (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                {
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5390), 1,
                                        new LogParam("EARCOMMON"),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                                    codeReturn = WriteEarCommon(transaction);
                                }

                                // Phase 7 - Calc and Write EARCALC
                                // PRSDATE counter value is not to process for EarCalc
                                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn) &&
                                    (false == IRQTools.IsIRQRequested(this, this.IRQNamedSystemSemaphore, ref codeReturn)))
                                {
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5360), 1,
                                        new LogParam("EARCALC"),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                                        new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));

                                    SetCounterValueData(false);

                                    codeReturn = CalcEarCalc(transaction);
                                }

                                if (transaction.Transaction != null)
                                {
                                    if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                                        DataHelper.CommitTran(transaction.Transaction, false);
                                    else
                                        DataHelper.RollbackTran(transaction.Transaction, false);
                                }
                            }
                            catch (Exception)
                            {
                                if (transaction.Transaction != null)
                                    DataHelper.RollbackTran(transaction.Transaction, false);

                                throw;
                            }
                            finally
                            {
                                transaction.Transaction = null;
                                DataHelper.CloseConnection(dbConnection);
                            }
                        }

                        // Phase ultime - Contrôle des Status sur les cotations 
                        if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                        {
                            Pair<string, object> quote =
                                m_CacheQuote.Find(match => match.Second.GetType().Equals(typeof(SpheresException2)));
                            if (null != quote)
                            {
                                SpheresException2 quoteException = (SpheresException2)quote.Second;
                                codeReturn = quoteException.ProcessState.CodeReturn;
                            }
                        }
                    }
                }

                if (codeReturn != Cst.ErrLevel.SUCCESS)
                    ProcessState.CodeReturn = codeReturn;
            }
            catch (Exception ex)
            {
                // FI 20220803 [XXXXX] Trace déjà alimentée par le logger
                if (false == LoggerManager.IsEnabled)
                {
                    // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                    Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                }

                SpheresException2 exLog = SpheresExceptionParser.GetSpheresException(null, ex);
                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(exLog);
                ProcessState.CodeReturn = exLog.ProcessState.CodeReturn;

                Logger.Log(new LoggerData(exLog));

                // FI 20200916 [XXXXX] pas de throw pour eviter double ajout dans le log  de l'exception
                //throw new SpheresException2(exLog.ProcessState);
            }
            finally
            {
                // RD 20120809 [18070] Optimisation
#if DEBUG || DEBUGDEV || RELEASEDEV
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5332), 0,
                    new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                    new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second),
                    new LogParam(m_LstMasterData.Find(match => match.First == "FLOWSCLASS").Second)));
#endif
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, ("------------------------------------------"));
#endif
            return ProcessState.CodeReturn;
        }

        /// <summary>
        /// Purge of TMPEARCALCAMOUNT
        /// </summary>
        /// FI 20151027 [21513] Add Method
        private void Delete_TMPEARCALCAMOUNT(DataDbTransaction pEfsTransaction)
        {
            // Purge of TMPEARCALCAMOUNT
            List<int> lstEar = (from pre_EarBook in m_EarBooks select pre_EarBook.IdEAR).ToList();
            string sqlLstEar = DataHelper.SQLCollectionToSqlList(pEfsTransaction.Cs, lstEar, TypeData.TypeDataEnum.@int);
            string SQL_Delete_TmpTable = SQLCst.DELETE_DBO + Cst.OTCml_TBL.TMPEARCALCAMOUNT.ToString() + Cst.CrLf;
            SQL_Delete_TmpTable += SQLCst.WHERE + "IDEAR" + SQLCst.IN + "(" + sqlLstEar + ")";

            DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, SQL_Delete_TmpTable);

        }

        /// <summary>
        /// Retourne true si le contexte est favorable à la génération du montant calculé "PRORATAPRM"
        /// <para>Montant calculé sur les ETD alloc de type option lorsqu'il existe au minimum 1 EAR day issu d'une clôture/compensation</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20151027 [21513] Add Method
        private Boolean IsCalcProrataPRM()
        {
            Boolean ret = false;

            string filterOption = StrFunc.AppendFormat("EVENTCODE = '{0}' or EVENTCODE = '{1}'", EventCodeEnum.AED.ToString(), EventCodeEnum.EED.ToString());
            if (this.m_TradeInfo.IsETDAlloc && ArrFunc.IsFilled(m_EventsInfo.DtEvent.Select(filterOption)))
            {
                //Liste des EARDay LPC, RMG, REC
                List<Pre_EarDay> lstEarDay = (from itemEarBook in m_EarBooks
                                              from itemEarDet in itemEarBook.EarDets
                                              from item in itemEarDet.EarDays.Where(x => x.eventCode == EventCodeEnum.LPC.ToString() &&
                                                                                            x.eventType == EventTypeEnum.RMG.ToString() &&
                                                                                             x.eventClass == EventClassEnum.REC.ToString())
                                              select item).ToList();

                //Recherche si eles EARDay LPC, RMG, REC précédents sont enfants d'une clôture/compensation
                foreach (Pre_EarDay item in lstEarDay)
                {
                    DataRow dr = m_EventsInfo.RowEvent(item.idE);
                    DataRow rowParent = dr.GetParentRow(m_EventsInfo.ChildEvent);
                    ret = (rowParent["EVENTCODE"].ToString() == EventCodeEnum.OFS.ToString());
                    if (ret)
                        break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Calcul d'un prorata
        /// </summary>
        /// <param name="pEfsTransaction"></param>
        /// <param name="pPre_EarBook"></param>
        /// <param name="pDefinedEarCalc"></param>
        /// <returns></returns>
        /// FI 20151027 [21513] Add Method
        private int CalcEarCalcProrata(DataDbTransaction pEfsTransaction, Pre_EarBook pPre_EarBook, DefinedEarCalc pDefinedEarCalc)
        {
            if (ArrFunc.Count(pDefinedEarCalc.agLstAmounts) != 1)
                throw new NotSupportedException(StrFunc.AppendFormat("Unexpected agLstAmounts dimension({0})", ArrFunc.Count(pDefinedEarCalc.agLstAmounts).ToString()));

            string[] funcArguments1 = pDefinedEarCalc.agLstAmounts[0].Split(';');
            if (ArrFunc.Count(funcArguments1) != 3)
                throw new NotSupportedException(StrFunc.AppendFormat("Unexpected arguments for method (Name:{0})", pDefinedEarCalc.Function.ToString()));

            string earCode = funcArguments1[0];
            string eventType = funcArguments1[1];
            string eventClass = funcArguments1[2];

            int ret;
            if (pDefinedEarCalc.calcType == "PRORATAPRM")
            {
                if ((earCode == EventCodeFunc.LinkedProductClosing.ToString()) &&
                    (eventType == EventTypeEnum.RMG.ToString()) &&
                    (eventClass == EventClassEnum.REC.ToString()))
                {
                    ret = CalcEarCalcProrataPrime(pEfsTransaction, pPre_EarBook, pDefinedEarCalc);
                }
                else
                {
                    throw new NotImplementedException(
                        StrFunc.AppendFormat("PRORATA method is not implemented for (EventCode:{0}, EventType:{1}, EventClass :{2})", earCode, eventType, eventClass));
                }
            }
            else
            {
                throw new NotImplementedException("PRORATA method is not implemented");

            }
            return ret;
        }

        /// <summary>
        ///  Calcul du Montant de la prime initiale au prorata de la qté clôturée lorsqu'il y a clôture/compensation 
        /// </summary>
        /// <returns></returns>
        /// FI 20151027 [21513] Add Method
        // EG 20180425 Analyse du code Correction [CA2202]
        private int CalcEarCalcProrataPrime(DataDbTransaction pEfsTransaction, Pre_EarBook pPre_EarBook, DefinedEarCalc pDefinedEarCalc)
        {
            int ret = 0;

            string[] funcArguments1 = pDefinedEarCalc.agLstAmounts[0].Split(';');

            string earCode = funcArguments1[0];
            string eventType = funcArguments1[1];
            string eventClass = funcArguments1[2];


            StrBuilder SQL_Insert = new StrBuilder();
            SQL_Insert += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TMPEARCALCAMOUNT + Cst.CrLf;
            SQL_Insert += "(IDEAR, EARCODE, EVENTCLASS, EXCHANGETYPE, IDC, PAID, RECEIVED, IDSTPROCESS, CALCTYPE, DTACCOUNT, INSTRUMENTNO, STREAMNO)" + Cst.CrLf;
            SQL_Insert += @"select @IDEAR, @EARCODE, @EVENTCLASS, @EXCHANGETYPE, @IDC, @PAID, @RECEIVED, @IDSTPROCESS, @CALCTYPE, @DTACCOUNT, @INSTRUMENTNO, @STREAMNO from DUAL";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(Cs, "IDEAR", DbType.Int32), pPre_EarBook.IdEAR);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.EARCODE), earCode);
            dp.Add(new DataParameter(Cs, "CALCTYPE", DbType.AnsiString, 16), pDefinedEarCalc.calcType);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.EVENTCLASS), eventClass);
            dp.Add(new DataParameter(Cs, "EXCHANGETYPE", DbType.AnsiString, 64), ExchangeTypeFunc.FlowCurrency.ToString());
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDC));
            dp.Add(new DataParameter(Cs, "PAID", DbType.Decimal), 0);
            dp.Add(new DataParameter(Cs, "RECEIVED", DbType.Decimal), 0);
            dp.Add(new DataParameter(Cs, "IDSTPROCESS", DbType.AnsiString, 8), Cst.ErrLevel.SUCCESS.ToString());
            dp.Add(new DataParameter(Cs, "DTACCOUNT", DbType.DateTime), pPre_EarBook.DtEvent);
            dp.Add(new DataParameter(Cs, "INSTRUMENTNO", DbType.Int32), 1);
            dp.Add(new DataParameter(Cs, "STREAMNO", DbType.Int32), 1);

            QueryParameters qryParameters = new QueryParameters(Cs, SQL_Insert.ToString(), dp);

            List<Pre_EarDay> lstEarDay = (from itemEarDet in pPre_EarBook.EarDets
                                          from item in itemEarDet.EarDays.Where(x => x.earCode == earCode &&
                                                                                    x.eventType == eventType &&
                                                                                    x.eventClass == eventClass)
                                          select item).ToList();

            foreach (Pre_EarDay item in lstEarDay)
            {
                DataRow row = m_EventsInfo.RowEvent(item.idE);

                DataRow rowParent = row.GetParentRow(m_EventsInfo.ChildEvent);
                if (null == rowParent)
                    throw new Exception(StrFunc.AppendFormat("IdE:{0}. GetParentRow not found", item.idE.ToString()));

                if ((rowParent["EVENTCODE"].ToString() == EventCodeEnum.OFS.ToString()))
                {
                    // EG 20170127 Qty Long To Decimal
                    LoadInitialPremium(Cs, CurrentId, pPre_EarBook.IdB, pPre_EarBook.DtEvent, out decimal qtyPrime, out decimal prime, out string side);

                    //Recherche de la quantité relative au flux LPC/RMG concerné
                    DataRow rowOFSQty = (from itemRow in rowParent.GetChildRows(m_EventsInfo.ChildEvent)
                                             .Where(x => Convert.ToString(x["EVENTTYPE"]) == "QTY")
                                         select itemRow).FirstOrDefault();
                    if (null == rowOFSQty)
                        throw new Exception(StrFunc.AppendFormat("OFFSETTING child row QTY not found"));

                    // EG 20170127 Qty Long To Decimal
                    decimal ofsQty = Convert.ToDecimal(rowOFSQty["VALORISATION"]);
                    decimal amount = prime * ofsQty / qtyPrime;

                    string idC = row["UNIT"].ToString();
                    dp["IDC"].Value = idC;
                    dp["PAID"].Value = (side == "PAID") ? amount : 0;
                    dp["RECEIVED"].Value = (side == "RECEIVED") ? amount : 0;

                    int rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    ret += rowAffected;

                    // Lecture de l'enregistrement qui vient tout juste d'être ajouté
                    int lastIDEARCALC = 0;
                    
                    string query = "select max(IDEARCALC) as LASTID from dbo.TMPEARCALCAMOUNT";
                    using (IDataReader dr = DataHelper.ExecuteReader(pEfsTransaction, CommandType.Text, query))
                    {
                        if (dr.Read())
                            lastIDEARCALC = Convert.ToInt32(dr["LASTID"]);
                    }

                    string SQL_AmountDet = StrFunc.AppendFormat(@"
                    insert into dbo.TMPEARCALCDET(IDEARCALC, IDEARTYPE, EARTYPE)
                    select {0}, {1},'EARDAY' from DUAL", lastIDEARCALC.ToString(), item.idEARDay);

                    rowAffected = DataHelper.ExecuteNonQuery(pEfsTransaction, CommandType.Text, SQL_AmountDet);
                    ret += rowAffected;
                }
            }
            return ret;
        }

        /// <summary>
        /// Lecture de DEFINEEARCALC
        /// <para>Mise en place des filtres afin de charger uniquement les enregistrements valables pour le contexte</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20151027 [21513] Add Method
        // EG 20180307 [23769] Gestion dbTransaction
        private string GetQueryLoadTableDEFINEEARCALC()
        {
            SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(CSTools.SetCacheOn(Cs), null, m_TradeInfo.IdI, false, SQL_Table.ScanDataDtEnabledEnum.Yes);

            StrBuilder select = new StrBuilder(@"select 
dec.SEQUENCENO, dec.CALCTYPE, dec.AGFUNC, dec.AGAMOUNTS, dec.IDA,dec.GPRODUCT, dec.IDP,
dec.IDGINSTR, dec.IDI, dec.BYINSTRUMENT, dec.BYSTREAM, dec.BYDTEVENT, dec.BYISPAYMENT, dec.BYEVENT
from dbo.DEFINEEARCALC dec") + Cst.CrLf;
            select += SQLCst.WHERE + StrFunc.AppendFormat("{0}", OTCmlHelper.GetSQLDataDtEnabled(Cs, "dec")) + Cst.CrLf;
            select += SQLCst.AND + StrFunc.AppendFormat("{0}", sqlInstrCriteria.GetSQLRestriction(CSTools.SetCacheOn(Cs), "dec", RoleGInstr.TUNING)) + Cst.CrLf;

            if (false == IsCalcProrataPRM())
                select += SQLCst.AND + StrFunc.AppendFormat("CALCTYPE != '{0}'", "PRORATAPRM");

            return select.ToString();
        }

        /// <summary>
        /// Chargement de la table DEFINEEARCALC
        /// </summary>
        /// <returns></returns>
        /// FI 20151027 [21513] Add
        private DataTable LoadDefineEarCalc()
        {
            string query = GetQueryLoadTableDEFINEEARCALC();
            DataTable ret = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(Cs), query);
            return ret;
        }

        /// <summary>
        ///Recherche 
        ///<para>-- la somme signée des flux PRM/HPR et des quantités associées </para>
        ///<para>-- du sens de la prime initiale</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdBook"></param>
        /// <param name="pDt"></param>
        /// <param name="pQty"></param>
        /// <param name="pPrime"></param>
        /// <param name="pSide"></param>
        /// FI 20151027 [21513] Add (Utilisation d'une requête SQL car l'usage de linq serait trop complexe)
        /// FI 20160303 [XXXXX] Modify (isnull à la place de nvl)
        // EG 20170127 Qty Long To Decimal 
        // EG 20180425 Analyse du code Correction [CA2202]
        private static void LoadInitialPremium(string pCs, int pIdT, int pIdBook, DateTime pDt, out decimal pQty, out decimal pPrime, out string pSide)
        {
            pQty = 0;
            pPrime = decimal.Zero;
            pSide = "N/A";

            string qry = @"select t.IDT, case when ep.IDB_PAY = @IDB then 'PAID'  else 'RECEIVED' end as SIDE,
            abs(case when eq.IDB_PAY = @IDB then -1 * eq.VALORISATION else eq.VALORISATION end + isnull(poc.QTY,0)) as QTY, 
            abs(case when ep.IDB_PAY = @IDB then -1 * ep.VALORISATION else ep.VALORISATION end + isnull(poc.PRM,0)) as PRM  
            from dbo.TRADE t
            inner join dbo.EVENT eq on eq.IDT = t.IDT and eq.EVENTCODE = 'STA' and eq.EVENTTYPE = 'QTY' 
            inner join dbo.EVENT ep on ep.IDT = t.IDT and ep.EVENTCODE = 'LPP' and ep.EVENTTYPE in ('PRM','HPR')
            inner join dbo.EVENT eMaster on eMaster.IDT = t.IDT and eMaster.EVENTCODE in ('AED','EED') and eMaster.IDE = eq.IDE_EVENT and eMaster.IDE= ep.IDE_EVENT
            left outer join 
            (   select  t.IDT, 
                sum(case when eq.IDB_PAY = @IDB  then -1 * eq.VALORISATION else eq.VALORISATION end) as QTY, 
                sum(case when ep.IDB_PAY = @IDB  then -1 * ep.VALORISATION else ep.VALORISATION end) as PRM  
                from dbo.TRADE t
                inner join dbo.EVENT eq on eq.IDT = t.IDT and eq.EVENTCODE in ('INT', 'TER') and eq.EVENTTYPE = 'QTY'
                inner join dbo.EVENT ep on ep.IDT = t.IDT and ep.EVENTCODE = 'LPC' and ep.EVENTTYPE in ('PRM','HPR') 
                inner join dbo.EVENT eMaster on eMaster.IDT = t.IDT and eMaster.EVENTCODE in ('POC','POT') and eMaster.EVENTTYPE in('PAR','TOT') and 
                                 eMaster.IDE = eq.IDE_EVENT and eMaster.IDE= ep.IDE_EVENT
                inner join dbo.EVENTCLASS ec on ec.IDE = eMaster.IDE and ec.EVENTCLASS = 'GRP' and ec.DTEVENT <= @DT 
                where t.IDT = @IDT
                group by t.IDT 
            ) poc on poc.IDT = t.IDT    
            where t.IDT = @IDT";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDT), pIdT);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDB), pIdBook);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DT), pDt);

            QueryParameters qryParameters = new QueryParameters(pCs, qry, dp);

            using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    pSide = Convert.ToString(dr["SIDE"]);
                    // EG 20170127 Qty Long To Decimal
                    pQty = Convert.ToDecimal(dr["QTY"]);
                    pPrime = Convert.ToDecimal(dr["PRM"]);
                }
            }
        }
        #endregion Methods
    }
}
