using System;
using EFS.ApplicationBlocks.Data;
using IOCompareCommon.DataContracts;
using IOCompareCommon.Interfaces;

namespace IOCompareCommon
{
    /// <summary>
    /// Query collection for internal datas
    /// </summary>
    /// <typeparam name="TInternal"></typeparam>
    public class IOCompareQueryInternal<TInternal>
        where TInternal : ISpheresComparer
    {

        private readonly string m_CS;
        private readonly bool m_IsMatchingOnlyClearer;

        /// <summary>
        /// Build a new Query getter for internal Spheres/Eurosys datas
        /// </summary>
        /// <param name="pCS">the connection string, it should not be null</param>
        // RD 20190718 [24580] Add parameter pIsMatchingOnlyClearer
        public IOCompareQueryInternal(string pCS, bool pIsMatchingOnlyClearer)
        {
            m_CS = pCS;
            m_IsMatchingOnlyClearer = pIsMatchingOnlyClearer;
        }

        /// <summary>
        /// Get the query core relative to the INTERNAL dataset
        /// </summary>
        /// <remarks>
        /// Rules list
        /// <list type="">
        /// <item>It is MANDATORY to provide an alias for any table inside the query, IFF you want the columns of the table are binded with
        /// some IoCompare parameters (ConfigurationParameter object). 
        /// If an alias is missing the columns defined for the table without alias will not be rattached to any IoCompare parameter</item>
        /// <item>For each table column matching an EXCEPT parameter 
        /// it is MANDATORY to provide a UNIQUE alias equals to the relative parameter ID.</item>
        /// <item>It is mandatory to provide an IDDATA field that identifies the row or a 0 IDDATA field.</item>
        /// <item>It is mandatory to provide a NULL DATAROWNUMBER field.</item>
        /// <item>The query core may contain WHERE conditions, but any condition must reference one field only owned by the main table 
        /// (see the BuildKeys regexp).</item>
        /// <item>Any WHERE condition should not contain sub-query or mathematical expressions (see the m_regExParseWhereCondition regexp).</item>
        /// <item>When any of the WHERE conditions are not compliant with the given rules, all of them must be incapsulated inside of the special 
        /// IoCompare tags 
        /// IOCOMPARE_COMPLEXCONDITION_START ... your not compliant WHERE conditions ... IOCOMPARE_COMPLEXCONDITION_END.</item>
        /// <item>The query field order is up to you.</item>
        /// <item>any WHERE conditions must be placed at the very end of the SQL request, 
        /// if you need to anticipate them then you need to place the IOCOMPARE_NOTCOMPLIANTFILTERS special tag 
        /// inside your request where you want that the custom filters will be added</item>
        /// <item>the query can contain SQL parameters expressed in the TSQL syntax (@xxxx).
        /// the SQL parameter value will be assigned from the Compare parameter having the same name</item>
        /// </list>
        /// </remarks>
        public string QueryCoreInternal
        {
            get
            {
                if (typeof(TInternal) == typeof(TradeFixCompareData))
                {

                    return QueryInternalTrade();

                }
                else if (typeof(TInternal) == typeof(PositionFixCompareData))
                {

                    return QueryInternalPosition(m_IsMatchingOnlyClearer);

                }
                else if (typeof(TInternal) == typeof(TradesInPositionFixCompareData))
                {

                    return QueryInternalTradesInPosition();

                }
                else if (typeof(TInternal) == typeof(CashFlowsCompareData))
                {

                    return QueryInternalCashFlow();

                }
                else if (typeof(TInternal) == typeof(CashFlowsInstrCompareData))
                {

                    return QueryInternalCashFlowInstr();

                }
                else if (typeof(TInternal) == typeof(TradeFixCompareDataEurosys))
                {

                    return QueryInternalTradeEurosys(m_CS);

                }
                else if (typeof(TInternal) == typeof(PositionFixCompareDataEurosys))
                {

                    return QueryInternalPositionEurosys(m_CS);

                }
                else if (typeof(TInternal) == typeof(TradesInPositionFixCompareDataEurosys))
                {

                    return QueryInternalTradesInPositionEurosys(m_CS);

                }
                else if (typeof(TInternal) == typeof(CashFlowsCompareDataEurosys))
                {

                    return QueryInternalCashFlowEurosys(m_CS);

                }
                else if (typeof(TInternal) == typeof(CashFlowsInstrCompareDataEurosys))
                {

                    return QueryInternalCashFlowInstrEurosys(m_CS);

                }
                else
                {
                    throw new NotSupportedException(String.Format("No sql request for the input type {0}", typeof(TInternal).ToString()));
                }
            }
        }

        private static string QueryInternalTradesInPosition()
        {
            // UNDONE MF 20120215  pas d'entité sur le filtre par défaut, rajouter le au plus tôt...
            // RD à voir avec MF / IOCOMPARE_COMPLEXCONDITION_START n'est pas géré, est-ce cette requête est utilisée?
            // RD 20210304 Use TRADEXML.TRADEXML instead of TRADE.TRADEXML            
            return @"
                select 
                alloc.IDT               as IDDATA,
                ''						as COLLECTEDVALUES,
                TRADEXML.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@TxnTm)[1]','UT_ENUM_OPTIONAL') 
                                        as TXNTM,
                alloc.SIDE              as SIDE,
                asset.CONTRACTIDENTIFIER	as SYM,
                asset.MATURITYMONTHYEAR as MMY,
                asset.STRIKEPRICE		as STRKPX,
                TRADEXML.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@LastPx)[1]','UT_PRICE') 
						                as LASTPX,		
                alloc.ISO10383_ALPHA4	as EXCH, 
                asset.PUTCALL			as PUTCALL,
                b.IDENTIFIER 
						                as ACCT,
                TRADEXML.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@AcctTyp)[1]','UT_ENUMCHAR_OPTIONAL') 
						                as ACCTTYP,	
                TRADEXML.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@PosEfct)[1]','UT_ENUMCHAR_OPTIONAL') 
						                as POSEFCT,																
                alloc.QTY 
                - isnull(pab.QTY,0) 
                - isnull(pas.QTY,0)  
						                as LASTQTY,
                0						as DATAROWNUMBER
                from dbo.VW_TRADE_POSETD alloc 
                inner join dbo.VW_ASSET_ETD_EXPANDED asset 
                on asset.IDASSET = alloc.IDASSET
                inner join dbo.BOOK b 
                on b.IDB = alloc.IDB_DEALER
                inner join dbo.TRADEXML 
                on TRADEXML.IDT = alloc.IDT
                left outer join (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY
                from dbo.POSACTION pa
                inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                where (pa.DTBUSINESS <= @DTBUSINESS)
                group by pad.IDT_BUY
                ) pab  on (pab.IDT = alloc.IDT)
                left outer join (
                select pad.IDT_SELL as IDT, sum(isnull(pad.QTY,0)) as QTY
                from dbo.POSACTION pa
                inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                where (pa.DTBUSINESS <= @DTBUSINESS)
                group by pad.IDT_SELL
                ) pas  on (pas.IDT = alloc.IDT)
                where
                /*IOCOMPARE_COMPLEXCONDITION_START*/
                (alloc.QTY - isnull(pab.QTY,0) - isnull(pas.QTY,0)) > 0
                /*IOCOMPARE_COMPLEXCONDITION_END*/
                and
                alloc.DTBUSINESS <= @DTBUSINESS
            ";
        }

        /// <summary>
        /// Get internal Position query
        /// </summary>
        /// <param name="pIsMatchingOnlyClearer"></param>
        /// <returns></returns>
        // RD 20190718 [24580] Add parameter pIsMatchingOnlyClearer
        private static string QueryInternalPosition(bool pIsMatchingOnlyClearer)
        {
            //PL 20110829
            //MF 20120124 - modifications à valider
            // UNDONE MF 20120215  pas d'entité sur le filtre par défaut, rajouter le au plus tôt...
            // RD à voir avec MF / IOCOMPARE_COMPLEXCONDITION_START n'est pas géré, est-ce cette requête est utilisée?
            // RD 20131129 [19272] Compare clearer data
            // - Add join on Clearer actor           
            // RD 20190718 [24580] Compare clearer data
            // - Add only Clearer actor query
            if (pIsMatchingOnlyClearer)
            {
                return @"
                    select 
                    min(alloc.IDT)			                                    as IDDATA,
                    ''						                                    as COLLECTEDVALUES,
                    asset.CONTRACTIDENTIFIER	                                as SYM,
                    asset.MATURITYMONTHYEAR                                     as MMY,
                    asset.STRIKEPRICE		                                    as STRKPX,	
                    alloc.ISO10383_ALPHA4	                                    as EXCH, 
                    asset.PUTCALL			                                    as PUTCALL,
                    b.IDENTIFIER                                                as ACCT,
                    case when b.ISPOSKEEPING = 1 then '2' else '8' end          as ACCTTYP,																				
                    sum(case when alloc.SIDE = 1 then (alloc.QTY - ISNULL(pab.QTY, 0)) else 0 end) 
                                                                                as ""LONG"",
                    sum(case when alloc.SIDE = 2 then (alloc.QTY - ISNULL(pas.QTY, 0)) else 0 end) 
                                                                                as ""SHORT"",
                    0						                                    as DATAROWNUMBER
                    from dbo.VW_TRADE_POSETD alloc 
                    inner join dbo.VW_ASSET_ETD_EXPANDED asset on (asset.IDASSET=alloc.IDASSET)
                    inner join dbo.BOOK b on (b.IDB=alloc.IDB_CLEARER)
                    inner join dbo.ACTOR a on (a.IDA=alloc.IDA_CLEARER)
                    left outer join (
                        select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY
                        from dbo.POSACTION pa
                        inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                        where (pa.DTBUSINESS <= @DTBUSINESS)
                        group by pad.IDT_BUY
                    ) pab  on (pab.IDT = alloc.IDT)
                    left outer join (
                        select pad.IDT_SELL as IDT, sum(isnull(pad.QTY,0)) as QTY
                        from dbo.POSACTION pa
                        inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                        where (pa.DTBUSINESS <= @DTBUSINESS)
                        group by pad.IDT_SELL
                    ) pas  on (pas.IDT = alloc.IDT)
                    where
                    /*IOCOMPARE_COMPLEXCONDITION_START*/
                    ((alloc.SIDE = 1 and (alloc.QTY - ISNULL(pab.QTY, 0)) > 0)
                        or 
                    (alloc.SIDE = 2 and (alloc.QTY - ISNULL(pas.QTY, 0)) > 0))
                    /*IOCOMPARE_COMPLEXCONDITION_END*/
                    and (alloc.DTBUSINESS <= @DTBUSINESS)
                    /*IOCOMPARE_NOTCOMPLIANTFILTERS*/
                    group by asset.CONTRACTIDENTIFIER,asset.MATURITYMONTHYEAR,asset.STRIKEPRICE,alloc.ISO10383_ALPHA4,asset.PUTCALL,
                        b.IDENTIFIER,a.IDA,b.ISPOSKEEPING,a.IDA
                    having 
                        (sum(case when alloc.SIDE = 1 then (alloc.QTY - ISNULL(pab.QTY, 0)) else 0 end) != 0) 
                        or 
                        (sum(case when alloc.SIDE = 2 then (alloc.QTY - ISNULL(pas.QTY, 0)) else 0 end) != 0)
                    ";
            }
            else
            {
                return @"
                select 
                min(alloc.IDT)			as IDDATA,
                ''						as COLLECTEDVALUES,
                asset.CONTRACTIDENTIFIER	as SYM,
                asset.MATURITYMONTHYEAR as MMY,
                asset.STRIKEPRICE		as STRKPX,	
                alloc.ISO10383_ALPHA4	as EXCH, 
                asset.PUTCALL			as PUTCALL,
                b.IDENTIFIER
						                as ACCT,
                case when ar.IDA is not null 
							                then '1'
	                when b.ISPOSKEEPING = 1 then '2'
							                else '8' 
                end						as ACCTTYP,																				
                sum(case when alloc.SIDE = 1 then (alloc.QTY - ISNULL(pab.QTY, 0)) else 0 end) as ""LONG"",
                sum(case when alloc.SIDE = 2 then (alloc.QTY - ISNULL(pas.QTY, 0)) else 0 end) as ""SHORT"",
                0						as DATAROWNUMBER
                from dbo.VW_TRADE_POSETD alloc 
                inner join dbo.VW_ASSET_ETD_EXPANDED asset 
                on asset.IDASSET = alloc.IDASSET
                inner join dbo.BOOK b on b.IDB = alloc.IDB_DEALER
                left outer join dbo.ACTOR aclearing on aclearing.IDA=alloc.IDA_CLEARER
                inner join dbo.ACTOR a on a.IDA = alloc.IDA_DEALER 
                left outer join dbo.ACTORROLE ar on ar.IDA = a.IDA and ar.IDROLEACTOR='CLIENT'
                left outer join (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY
                from dbo.POSACTION pa
                inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                where (pa.DTBUSINESS <= @DTBUSINESS)
                group by pad.IDT_BUY
                ) pab  on (pab.IDT = alloc.IDT)
                left outer join (
                select pad.IDT_SELL as IDT, sum(isnull(pad.QTY,0)) as QTY
                from dbo.POSACTION pa
                inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                where (pa.DTBUSINESS <= @DTBUSINESS)
                group by pad.IDT_SELL
                ) pas  on (pas.IDT = alloc.IDT)
                where
                /*IOCOMPARE_COMPLEXCONDITION_START*/
                (
                (alloc.SIDE = 1 and (alloc.QTY - ISNULL(pab.QTY, 0)) > 0)
                or 
                (alloc.SIDE = 2 and (alloc.QTY - ISNULL(pas.QTY, 0)) > 0)
                )
                /*IOCOMPARE_COMPLEXCONDITION_END*/
                and
                alloc.DTBUSINESS <= @DTBUSINESS
                /*IOCOMPARE_NOTCOMPLIANTFILTERS*/
                group by 
                asset.CONTRACTIDENTIFIER, 
                asset.MATURITYMONTHYEAR, 
                asset.STRIKEPRICE, 
                alloc.ISO10383_ALPHA4, 
                asset.PUTCALL,
                b.IDENTIFIER,
                a.IDA,
                ar.IDA,
                b.ISPOSKEEPING,
                aclearing.IDA
                having 
                (sum(case when alloc.SIDE = 1 then (alloc.QTY - ISNULL(pab.QTY, 0)) else 0 end) != 0) 
                or 
                (sum(case when alloc.SIDE = 2 then (alloc.QTY - ISNULL(pas.QTY, 0)) else 0 end) != 0)
            ";
            }
        }


        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // RD 20220624 [26058] Add INSTRUMENT and MARKET joins
        private static string QueryInternalTrade()
        {
            //PL 20101209
            //t.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@TxnTm)[1]','UT_ENUMCHAR_OPTIONAL') as TXNTM,
            // RD 20131129 [19272] Add join on Clearer actor 
            // RD 20131129 [19287] Exclude deactiv trades
            // EG 20170127 Qty Long To Decimal (UT_QTY)
            return @"select t.IDT as IDDATA,
                    '' as COLLECTEDVALUES,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@LastQty)[1]','UT_QTY') as LASTQTY,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@Side)[1]','UT_ENUMCHAR_OPTIONAL') as SIDE,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@LastPx)[1]','UT_PRICE') as LASTPX,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:Instrmt/@MMY)[1]','UT_ENUM_OPTIONAL') as MMY,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:Instrmt/@Sym)[1]','UT_LINKCODE') as SYM,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:Instrmt/@Exch)[1]','char(4)') as EXCH,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:Instrmt/@StrkPx)[1]','UT_PRICE') as STRKPX,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:Instrmt/@PutCall)[1]','UT_ENUMCHAR_OPTIONAL') as PUTCALL,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@TxnTm)[1]','UT_ENUM_OPTIONAL') as TXNTM,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@Acct)[1]','UT_IDENTIFIER') as ACCT,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@AcctTyp)[1]','UT_ENUMCHAR_OPTIONAL') as ACCTTYP,
                    trx.TRADEXML.value('(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@PosEfct)[1]','UT_ENUMCHAR_OPTIONAL') as POSEFCT, 
                    0 as DATAROWNUMBER
                    from dbo.TRADE t
                    inner join dbo.TRADEXML trx on (trx.IDT = t.IDT)
                    inner join dbo.INSTRUMENT i on (i.IDI=t.IDI)
                    inner join dbo.MARKET m on (m.IDM=t.IDM)
                    left outer join dbo.TRADEACTOR taclearing on (taclearing.IDT=t.IDT) and (taclearing.IDROLEACTOR='COUNTERPARTY') and (taclearing.FIXPARTYROLE in ('4','21'))
                    left outer join dbo.ACTOR aclearing on (aclearing.IDA=taclearing.IDA)
                    where (t.IDSTACTIVATION!='DEACTIV')";
        }

        private static string QueryInternalTradesInPositionEurosys(string pCS)
        {
            // 20110905 MF - Ticket 17443 
            // 21020403 MF - Ticket 9928 - add DATE_TRAIT for key
            return @"select 
                    0 as IDDATA,
                    '' as COLLECTEDVALUES,
                    " + DataHelper.SQLFormatColumnDateTimeInIso(pCS, "pos.HRE_NEGO") + @" as TXNTM,
                    case pos.SENS_OP when 'A' then '1' when 'V' then '2' else null end as SIDE,
                    pos.QUANT_OP as LASTQTY,
                    pos.COURS_OP as LASTPX,
                    " + DataHelper.SQLSubstring(pCS, DataHelper.SQLConcat(pCS, " pos.MARCHE", "'    '"), 1, 4) + @" as EXCH,
                    pos.PRODUIT as SYM,
                    case p.TYP_ECHE
                         when 'J' then '20' ||" + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 7, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 4, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 1, 2) + @"
                         when 'M' then '20' ||" + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 4, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 1, 2) + @"
                         else null
                         end as MMY,
                    case pos.INSTRT when 'C' then '1' when 'P' then '0' else null end as PUTCALL,
                    case pos.INSTRT when 'F' then null else pos.STRK_OP end as STRKPX,
                    pos.NUM_COMPTE as ACCT,
                    case " + DataHelper.SQLSubstring(pCS, "pos.TYP_COMPTE", 1, 1) + @" when 'C' then '1' when 'M' then '2' else '8' end as ACCTTYP,
                    pos.TYP_OP as POSEFCT,
                    pos.AGE_ENREG as AGE_ENREG,pos.TIME_ENREG as TIME_ENREG,pos.TYP_COMPTE as TYP_COMPTE,
                    pos.DATE_TRAIT as DATE_TRAIT,
                    0 as DATAROWNUMBER
                    from dbo.VUE_ARCH_POS_OUV pos
                    inner join dbo.PRODUIT p on (p.PRODUIT=pos.PRODUIT)
                    left outer join dbo.BANQUE bq on (bq.NUM_PROPRIET=pos.NUM_COMPTE)
                    where pos.TYP_COMPTE not in ('BROKER')";
        }

        private static string QueryInternalPositionEurosys(string pCS)
        {
            //GS 20110801 This join handle the source (valorized into BANQUE table). This is a ICBPI specification (to do: find a standard rule)
            //PL 20110829 On garde la jointure externe sur la table BANQUE car, dixit Fabrice, sans risque pour les autres clients Eurosys®.
            //PL 20110831 Utilisation du PRODUIT directement depuis VUE_ARCH_POS_OUV, à l'image de ce qui était en place sur les trades + harmonisation avec la query des trades
            //MF 21020403 Ticket 9928 - add DATE_TRAIT for key
            return @"select 
                    0 as IDDATA,
                    case pos.SENS_OP when 'A' then pos.QUANT_OP else 0 end as ""LONG"",
                    case pos.SENS_OP when 'V' then pos.QUANT_OP else 0 end as ""SHORT"",
                    " + DataHelper.SQLSubstring(pCS, DataHelper.SQLConcat(pCS, "pos.MARCHE", "'    '"), 1, 4) + @" as EXCH,
                    pos.PRODUIT as SYM,
                    case p.TYP_ECHE
                         when 'J' then '20' ||" + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 7, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 4, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 1, 2) + @"
                         when 'M' then '20' ||" + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 4, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "pos.NOM_ECHE", 1, 2) + @"
                         else null
                         end as MMY,
                    case pos.INSTRT when 'C' then '1' when 'P' then '0' else null end as PUTCALL,
                    case pos.INSTRT when 'F' then null else pos.STRK_OP end as STRKPX,
                    pos.NUM_COMPTE as ACCT,
                    case " + DataHelper.SQLSubstring(pCS, "pos.TYP_COMPTE", 1, 1) + @" when 'C' then '1' when 'M' then '2' else '8' end as ACCTTYP,
                    pos.AGE_ENREG as AGE_ENREG,pos.TIME_ENREG as TIME_ENREG,pos.TYP_COMPTE as TYP_COMPTE,
                    pos.DATE_TRAIT as DATE_TRAIT,
                    0 as DATAROWNUMBER
                    from dbo.VUE_ARCH_POS_OUV pos
                    inner join dbo.PRODUIT p on (p.PRODUIT = pos.PRODUIT)
                    left outer join dbo.BANQUE bq on (bq .NUM_PROPRIET=pos.NUM_COMPTE)
                    where pos.TYP_COMPTE not in ('BROKER')";
        }

        private static string QueryInternalTradeEurosys(string pCS)
        {
            //PL 20110831 Harmonisation avec la query des Positions
            return @"select 
                    0 as IDDATA,
                    '' as COLLECTEDVALUES,
                    " + DataHelper.SQLFormatColumnDateTimeInIso(pCS, "t.HRE_NEGO") + @" as TXNTM,
                    case t.SENS_OP when 'A' then '1' when 'V' then '2' else null end as SIDE,
                    t.QUANT_OP as LASTQTY,
                    t.COURS_OP as LASTPX,
                    " + DataHelper.SQLSubstring(pCS, DataHelper.SQLConcat(pCS, "t.MARCHE", "'    '"), 1, 4) + @" as EXCH,
                    t.PRODUIT as SYM,
                    case p.TYP_ECHE
                         when 'J' then '20' ||" + DataHelper.SQLSubstring(pCS, "t.NOM_ECHE", 7, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "t.NOM_ECHE", 4, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "t.NOM_ECHE", 1, 2) + @"
                         when 'M' then '20' ||" + DataHelper.SQLSubstring(pCS, "t.NOM_ECHE", 4, 2) + @" || "
                                        + DataHelper.SQLSubstring(pCS, "t.NOM_ECHE", 1, 2) + @"
                         else null
                         end as MMY,
                    case t.INSTRT when 'C' then '1' when 'P' then '0' else null end as PUTCALL,
                    case t.INSTRT when 'F' then null else t.STRK_OP end as STRKPX,
                    t.NUM_COMPTE as ACCT,
                    case " + DataHelper.SQLSubstring(pCS, "t.TYP_COMPTE", 1, 1) + @" when 'C' then '1' when 'M' then '2' else '8' end as ACCTTYP,
                    t.TYP_OP as POSEFCT,
                    t.AGE_ENREG as AGE_ENREG,t.TIME_ENREG as TIME_ENREG,t.TYP_COMPTE as TYP_COMPTE,
                    0 as DATAROWNUMBER
                    from dbo.VUE_ARCH_NEGO t
                    inner join dbo.PRODUIT p on (p.PRODUIT=t.PRODUIT)
                    left outer join dbo.BANQUE bq on (bq.NUM_PROPRIET=t.NUM_COMPTE)
                    where t.TYP_COMPTE not in ('BROKER','INTER')";
            //PL For test only
            //and t.COURS_OP = 20660";
        }

        // GP 20120612, ticket 17596 item 34, 35
        //
        // Dans le flux de JPM intégré par ICBPI le montant Pending Incremental peut être comparé
        // soit avec les Mouvements (FLX_DEV.MONT_MOUV) soit avec le solde (FLX_DEV.MONT_SOLDE).
        //
        // Pour gérer cela nous avons introduit le parametre PENDINGINCRCB. 
        //
        // Si PENDINGINCRCB = 'CashBalance' le montant externe est comparé avec:
        // 1. les movements si le solde n'est pas géré (ID_GEST_SOLDE = 'N') 
        // 2. les soldes si le solde est géré (ID_GEST_SOLDE = 'O')
        // Si le parametre PENDINGINCRCB est valorisé avec une valeur autre que CashBalance 
        // ou il est null, le montant externe est comparé avec les mouvements (besoin initial de ICBPI).

        private static string QueryInternalCashFlowEurosys(string pCS)
        {
            // Ticket 17596 item 34, 35
            return @"select 
                    0 as IDDATA,
                    '' as COLLECTEDVALUES,
                    fdev.NUM_PROPRIET as ACCT,
                    case " + DataHelper.SQLSubstring(pCS, "fdev.CAT_PROPRIET", 1, 1) + @" when 'C' then '1' when 'M' then '2' else '8' end as ACCTTYP,
                    case 
                        when upper (@EXCEPTFEES) = upper ('true') then 0
                        else fdev.MONT_FRAIS 
                    end as TAXCOMAMT, 
                    fdev.CD_DEV as TAXCOMCCY,
                    fdev.MONT_DEPOSIT as COLLAMT, 
                    fdev.CD_DEV as COLLCCY, 
                    case
                    when upper (@PENDINGINCRCB) = upper ('CashBalance')
                    then
                        case 
                            when rg.ID_GEST_SOLDE = 'N' or rg.ID_GEST_SOLDE is null then fdev.MONT_MOUV /*as PMTAMT*/
                            when rg.ID_GEST_SOLDE = 'O' then fdev.MONT_SOLDE
                            else fdev.MONT_MOUV 
                        end 
                        else fdev.MONT_MOUV
                    end as PMTAMT, 
                    fdev.CD_DEV as PMTCCY,
                    fdev.CD_DEV as CD_DEV,
                    fdev.NUM_PROPRIET as NUM_PROPRIET,
                    fdev.CAT_PROPRIET as CAT_PROPRIET,
                    fdev.DATE_TRAIT as DATE_TRAIT, 
                    0 as DATAROWNUMBER
                    from dbo.FLX_DEV fdev
                    left outer join dbo.BANQUE bq on bq.NUM_PROPRIET = fdev.NUM_PROPRIET
                    left outer join REGLEMNT rg on rg.NUM_PROPRIET = fdev.NUM_PROPRIET and rg.CD_DEV = fdev.CD_DEV"
                    ;
        }

        private static string QueryInternalCashFlowInstrEurosys(string pCS)
        {
            return @"select 
                    0 as IDDATA,
                    '' as COLLECTEDVALUES,
                    fprod.NUM_PROPRIET as ACCT,
                    case " + DataHelper.SQLSubstring(pCS, "fprod.CAT_PROPRIET", 1, 1) + @" when 'C' then '1' when 'M' then '2' else '8' end as ACCTTYP,
                    fprod.PRODUIT as SYM,
                    fprod.MARCHE as MRK,
                    case fprod.INSTRT when 'C' then '1' when 'P' then '0' else null end as PUTCALL,
                    fprod.MONT_POTENT as UMGAMT, 
                    fprod.CD_DEV as UMGCCY,
                    fprod.CD_DEV as CD_DEV,
                    fprod.NUM_PROPRIET as NUM_PROPRIET,
                    fprod.CAT_PROPRIET as CAT_PROPRIET,
                    fprod.DATE_TRAIT as DATE_TRAIT,
                    fprod.PRODUIT as PRODUIT,
                    fprod.INSTRT as INSTRT, 
                    0 as DATAROWNUMBER
                    from dbo.FLX_PROD fprod
                    left outer join dbo.BANQUE bq on bq.NUM_PROPRIET = fprod.NUM_PROPRIET";
        }

        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string QueryInternalCashFlow()
        {
            // RD 20121022 [18112] 
            // - Modify value of RptAmt
            // - Add new amount CSBAMT
            return @"select min(tr.IDT) as IDDATA, '' as COLLECTEDVALUES, b.IDENTIFIER as ACCT, 1 as ACCTTYP, ta_ent.IDA as IDA_ENTITY,
                    sum(case when e_mgr.IDA_PAY = e_opp.IDA_PAY then -1 else + 1 end * (isnull(e_opp.VALORISATION,0) - isnull(e_opp_rec.VALORISATION,0))) as TAXCOMAMT, unit.UNIT as TAXCOMCCY, 1 as TAXCOMACT,
                    sum( isnull(case when e_mgr.IDA_PAY = e_mgr_.IDA_PAY then +1 else -1 end * e_mgr_.VALORISATION, 0) ) as COLLAMT, unit.UNIT as COLLCCY, 1 as COLLACT,
                    sum( isnull(case when e_mgr.IDA_PAY = e_cbp.IDA_PAY then +1 else -1 end * e_cbp.VALORISATION, 0) ) as PMTAMT, unit.UNIT	as PMTCCY, 1 as PMTACT,
                    sum( isnull(case when e_mgr.IDA_PAY = e_prm.IDA_PAY then -1 else +1 end * e_prm.VALORISATION, 0) ) as PRMAMT, unit.UNIT	as PRMCCY, 1 as PRMACT,
                    sum( isnull(case when e_mgr.IDA_PAY = e_vmg.IDA_PAY then -1 else +1 end * e_vmg.VALORISATION, 0) ) +
                    sum( isnull(case when e_mgr.IDA_PAY = e_scu.IDA_PAY then -1 else +1 end * e_scu.VALORISATION, 0) ) as VRMRGNAMT, unit.UNIT as VRMRGNCCY, 1 as VRMRGNACT,
                    sum(case when e_mgr.IDA_PAY = e_opp.IDA_PAY then -1 else + 1 end * (isnull(e_opp.VALORISATION,0) - isnull(e_opp_rec.VALORISATION,0))) as TAXCOMBRKAMT, unit.UNIT as TAXCOMBRKCCY, 1 as TAXCOMBRKACT,
                    sum( isnull(case when e_mgr.IDA_PAY = e_mgc.IDA_PAY then -1 else +1 end * e_mgc.VALORISATION, 0) ) as CALLAMT, unit.UNIT as CALLCCY, 1 as CALLACT,
                    sum( isnull(case when e_mgr.IDA_PAY = e_csb.IDA_PAY then -1 else +1 end * e_csb.VALORISATION, 0) ) -
                    sum( isnull(case when e_mgr.IDA_PAY = e_csu.IDA_PAY then -1 else +1 end * e_csu.VALORISATION, 0) ) as RPTAMT, unit.UNIT as RPTCCY, 1 as RPTACT,
                    sum( isnull(case when e_mgr.IDA_PAY = e_csb.IDA_PAY then -1 else +1 end * e_csb.VALORISATION, 0) ) as CSBAMT, 1 as CSBACT, 
                    0 as DATAROWNUMBER
                    from  dbo.TRADE tr   
                    inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)        
                    inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER='cashBalance')     
                    inner join 
                    ( 
                         select distinct IDT, UNIT 
                         from  dbo.EVENT 
                         where (UNITTYPE = 'Currency') and (IDSTACTIVATION = 'REGULAR') 
                    ) unit on ( unit.IDT = tr.IDT )
                    inner join 
                    ( 
                        select distinct IDT, IDA_PAY, IDA_REC, IDB_PAY, IDB_REC 
                        from  dbo.EVENT 
                        where (EVENTCODE = 'LPC') and (EVENTTYPE = 'MGR') and (IDSTACTIVATION = 'REGULAR' ) 
                    ) e_mgr on (e_mgr.IDT = tr.IDT)
                    inner join dbo.BOOK b on (b.IDB = e_mgr.IDB_PAY )  
                    inner join dbo.TRADEACTOR ta_cbo on (ta_cbo.IDT = tr.IDT) and (ta_cbo.IDROLEACTOR = 'CSHBALANCEOFFICE')  
                    inner join dbo.ACTOR on ACTOR.IDA = ta_cbo.IDA         
                    inner join dbo.TRADEACTOR ta_ent on (ta_ent.IDT = tr.IDT) and (ta_ent.IDROLEACTOR = 'ENTITY')  
                    inner join dbo.ACTOR ae on (ae.IDA = ta_ent.IDA)               

                    left outer join dbo.EVENT e_mgr_ on (e_mgr_.IDT = tr.IDT) and (e_mgr_.EVENTCODE = 'LPC') and (e_mgr_.EVENTTYPE = 'MGR') and (e_mgr_.IDSTACTIVATION = 'REGULAR' ) and (e_mgr_.UNIT = unit.UNIT)  

                    left outer join dbo.EVENT e_prm on (e_prm.IDT = tr.IDT) and (e_prm.EVENTCODE = 'LPC') and (e_prm.EVENTTYPE = 'PRM') and (e_prm.IDSTACTIVATION = 'REGULAR' ) and (e_prm.UNIT = unit.UNIT) 

                    left outer join dbo.EVENT e_vmg on (e_vmg.IDT = tr.IDT) and (e_vmg.EVENTCODE = 'LPC') and (e_vmg.EVENTTYPE = 'VMG') and (e_vmg.IDSTACTIVATION = 'REGULAR' ) and (e_vmg.UNIT = unit.UNIT)   

                    left outer join dbo.EVENT e_scu on (e_scu.IDT = tr.IDT) and (e_scu.EVENTCODE = 'LPC') and (e_scu.EVENTTYPE = 'SCU') and (e_scu.IDSTACTIVATION = 'REGULAR' ) and (e_scu.UNIT = unit.UNIT)      

                    left outer join 
                    (
                        /* Cumul des flux OPP payé  + TAX */
                        select sum(VALORISATION) as VALORISATION, UNIT, IDT, IDA_PAY, IDA_REC            
                        from dbo.EVENT     
                        where (EVENTCODE = 'OPP') and (IDSTACTIVATION = 'REGULAR')
                        group by IDT, UNIT, IDA_PAY, IDA_REC
                    ) e_opp on (e_opp.IDT = tr.IDT) and (e_opp.UNIT = unit.UNIT) and (e_opp.IDA_PAY = e_mgr.IDA_PAY)            
                            
                    left outer join            
                    (
                        /* Cumul des flux OPP reçu  + TAX */
                        select sum(VALORISATION) as VALORISATION, UNIT, IDT, IDA_PAY, IDA_REC            
                        from dbo.EVENT 
                        where (EVENTCODE = 'OPP') and (IDSTACTIVATION = 'REGULAR')            
                        group by IDT, UNIT, IDA_PAY, IDA_REC            
                    ) e_opp_rec on (e_opp_rec.IDT = tr.IDT) and (e_opp_rec.UNIT = unit.UNIT) and (e_opp_rec.IDA_REC = e_mgr.IDA_PAY)
                                
                    left outer join dbo.EVENT e_mgc on (e_mgc.IDT = tr.IDT) and (e_mgc.EVENTCODE = 'LPC') and (e_mgc.EVENTTYPE = 'MGC') and (e_mgc.IDSTACTIVATION = 'REGULAR' ) and (e_mgc.UNIT = unit.UNIT)            

                    left outer join dbo.EVENT e_cbp on (e_cbp.IDT = tr.IDT) and (e_cbp.EVENTCODE = 'LPC') and (e_cbp.EVENTTYPE = 'CBP') and (e_cbp.IDSTACTIVATION = 'REGULAR' ) and (e_cbp.UNIT = unit.UNIT)

                    left outer join dbo.EVENT e_csb on (e_csb.IDT = tr.IDT) and (e_csb.EVENTCODE = 'LPC') and (e_csb.EVENTTYPE = 'CSB') and (e_csb.IDSTACTIVATION = 'REGULAR' ) and (e_csb.UNIT = unit.UNIT)            

                    left outer join dbo.EVENT e_csu on (e_csu.IDT = tr.IDT) and (e_csu.EVENTCODE = 'LPC') and (e_csu.EVENTTYPE = 'CSU') and (e_csu.IDSTACTIVATION = 'REGULAR' ) and (e_csu.UNIT = unit.UNIT)            
                     
                    where (tr.IDSTACTIVATION = 'REGULAR') and (ae.IDENTIFIER = @ENTITYIDENTIFIER) and (tr.DTBUSINESS = @DTBUSINESS)

                    /*IOCOMPARE_NOTCOMPLIANTFILTERS*/

                    group by tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS, 
                    unit.UNIT, 
                    ta_cbo.IDA, ta_ent.IDA, 
                    e_mgr.IDA_PAY, e_mgr.IDA_REC, e_mgr.IDB_PAY, e_mgr.IDB_REC,
                    b.IDENTIFIER";
        }

        /// <summary>
        /// Template SQL product/instrument cash flows 
        /// </summary>
        /// <remarks>
        /// Extracted from the original request "Consultation: Flux financiers agrégés par actif".
        /// Just the unrealized amount is evaluated.
        /// </remarks>
        /// <returns>the SQL request to select all the cash flows grouped by book/product/instrument</returns>
        private static string QueryInternalCashFlowInstr()
        {
            //  UNDONE MF 20120214 le filtrage par marché est désactivé, VW_TRADE_POSETD n'a plus d'alias pour résoudre l'erreur
            //      soulevée lors du traitement du paramètre ACCTTYP
            // RD 20141104 [20461] Chargement des flux LPC/VMG et LPC/PRM
            return @"select 
                    min(VW_TRADE_POSETD.IDT) 
                                            as IDDATA,
                    ''                      as COLLECTEDVALUES,
                    b.IDENTIFIER            as ACCT,
                    case when ta.FIXPARTYROLE= '27' then 1 else 2 end         
                                            as ACCTTYP,
                    asset.CONTRACTSYMBOL	as SYM,
                    m.ISO10383_ALPHA4	
                                            as MRK, 
                    asset.PUTCALL			as PUTCALL,
                    sum(isnull(e_prm.PRM_SIGNED, 0)) 
                                            as PRMAMT,
                    unit.UNIT               as PRMCCY,
                    1                       as PRMACT,
                    sum(isnull(e_scu.SCU_SIGNED, 0) + isnull(e_vmg.VMG_SIGNED, 0)) 
                                            as VRMRGNAMT,
                    unit.UNIT               as VRMRGNCCY,
                    1                       as VRMRGNACT,
                    sum(isnull(e_umg.UMG_SIGNED, 0)) 
                                            as UMGAMT,
                    unit.UNIT               as UMGCCY,
                    1                       as UMGACT,
                    sum(isnull(e_lov.LOV_SIGNED, 0)) 
                                            as LOVAMT,
                    unit.UNIT               as LOVCCY,
                    1                       as LOVACT,
                    sum(isnull(e_rmg.RMG_SIGNED, 0)) 
                                            as RMGAMT,
                    unit.UNIT               as RMGCCY,
                    1                       as RMGACT,
                    sum(isnull(e_opp.FEE_SIGNED, 0)) 
                                            as TAXCOMBRKAMT,
                    unit.UNIT               as TAXCOMBRKCCY,
                    1                       as TAXCOMBRKACT,
                    0                       as DATAROWNUMBER

                    from  dbo.VW_TRADE_POSETD 
                    inner join dbo.VW_ASSET_ETD_EXPANDED asset 
                      on asset.IDASSET = VW_TRADE_POSETD.IDASSET
                    inner join dbo.ACTOR ae 
                      on ae.IDA = VW_TRADE_POSETD.IDA_ENTITYDEALER 
                    inner join dbo.TRADEACTOR ta 
                      on ta.IDT = VW_TRADE_POSETD.IDT and ta.IDA in (VW_TRADE_POSETD.IDA_DEALER, VW_TRADE_POSETD.IDA_CLEARER) 
                      and  ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                    inner join dbo.BOOK b 
                      on 
                        (b.IDB = VW_TRADE_POSETD.IDB_DEALER and ta.FIXPARTYROLE = '27') or 
                        (b.IDB = VW_TRADE_POSETD.IDB_CLEARER and ta.FIXPARTYROLE in ('21', '4')) 
                    inner join dbo.MARKET m on (m.IDM=VW_TRADE_POSETD.IDM)

                    inner join
                    (     
                      select e.UNIT as UNIT, e.IDT as IDT, e.IDA_PAY as IDA
                      from  dbo.EVENT e
                      inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE 
                      where e.UNITTYPE='Currency' 
                      and e.IDSTACTIVATION='REGULAR'
                      and nullif(e.UNIT,'') is not null 
                      and
                      (
                         (
                           e.EVENTCODE='OPP'
                           or 
                           (e.EVENTCODE='STA') 
                           or
                           (e.EVENTCODE in ('LPC','LPI','LPP') and e.EVENTTYPE in ('LOV','PRM','RMG','UMG','VMG'))
                         )
                         and  ec.DTEVENT = @DTBUSINESS 
                        )
                      union 
                      select e.UNIT as UNIT, e.IDT as IDT, e.IDA_REC as IDA
                      from  dbo.EVENT e
                      inner join  dbo.EVENTCLASS ec on ec.IDE=e.IDE 
                      where e.UNITTYPE='Currency' 
                      and e.IDSTACTIVATION='REGULAR'
                      and nullif(e.UNIT,'') is not null 
                      and
                      (
                        (
                        e.EVENTCODE='OPP'
                        or 
                        (e.EVENTCODE='STA') 
                        or
                        (e.EVENTCODE in ('LPC','LPI','LPP') and e.EVENTTYPE in ('LOV','PRM','RMG','UMG','VMG'))
                        )
                        and 
                        ec.DTEVENT = @DTBUSINESS
                      )
                    ) unit on unit.IDT = VW_TRADE_POSETD.IDT and unit.IDA in (VW_TRADE_POSETD.IDA_CLEARER)

                    left outer join 
                    (
                      /* Cumul des flux PRM */
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as PRM_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from dbo.EVENT e
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT= @DTBUSINESS 
                      where e.EVENTCODE in ('LPP','LPC') and e.EVENTTYPE='PRM' and e.IDSTACTIVATION='REGULAR'
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                    ) e_prm on (e_prm.IDT=VW_TRADE_POSETD.IDT) and e_prm.UNIT=unit.UNIT and e_prm.FIXPARTYROLE=ta.FIXPARTYROLE

                    left outer join 
                    (
                      /* Cumul des flux SCU*/
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as SCU_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE  
                      from dbo.EVENT e
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT= @DTBUSINESS
                      where e.EVENTCODE in ('TER','INT') and e.EVENTTYPE='SCU' and e.IDSTACTIVATION='REGULAR'
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE 
                    ) e_scu on (e_scu.IDT=VW_TRADE_POSETD.IDT) and e_scu.UNIT=unit.UNIT and e_scu.FIXPARTYROLE=ta.FIXPARTYROLE

                    left outer join 
                    (
                      /* Cumul des flux VMG, LPP,LPC s’il en existe (donc flux EOD), sinon LPI (donc flux Intraday)  */
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as VMG_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from dbo.EVENT e
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT=@DTBUSINESS
                      where e.EVENTCODE in ('LPP','LPC') and e.EVENTTYPE='VMG' and e.IDSTACTIVATION='REGULAR'
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                      union all 
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as VMG_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from dbo.EVENT e
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='STL' and ec.ISPAYMENT=1 and ec.DTEVENT=@DTBUSINESS
                      left outer join dbo.EVENT e2 on e2.IDT=e.IDT and e2.EVENTCODE in ('LPP','LPC') and e2.EVENTTYPE='VMG' and e2.IDSTACTIVATION='REGULAR'
                      left outer join dbo.EVENTCLASS ec2 on ec2.IDE=e2.IDE and ec2.EVENTCLASS='REC' and ec2.DTEVENT=@DTBUSINESS
                      where e.EVENTCODE='LPI' and e.EVENTTYPE='VMG' and e.IDSTACTIVATION='REGULAR'
                      and ec2.IDE is null 
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                    ) e_vmg on (e_vmg.IDT=VW_TRADE_POSETD.IDT) and e_vmg.UNIT=unit.UNIT and e_vmg.FIXPARTYROLE=ta.FIXPARTYROLE 

                    left outer join 
                    (
                      /* Cumul des flux UMG, LPC s’il en existe (donc flux EOD), sinon LPI (donc flux Intraday)*/
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as UMG_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from  dbo.EVENT e 
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join  dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT = @DTBUSINESS
                      where e.EVENTCODE='LPC' and e.EVENTTYPE='UMG' and e.IDSTACTIVATION='REGULAR'
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                      union all
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as UMG_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from  dbo.EVENT e 
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join  dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT = @DTBUSINESS
                      left outer join  dbo.EVENT e2 on e2.IDT=e.IDT and e2.EVENTCODE='LPC' and e2.EVENTTYPE='UMG' and e2.IDSTACTIVATION='REGULAR'
                      left outer join  dbo.EVENTCLASS ec2 on ec2.IDE=e2.IDE and ec2.EVENTCLASS='REC' and ec2.DTEVENT=@DTBUSINESS
                      where e.EVENTCODE='LPI' and e.EVENTTYPE='UMG' and e.IDSTACTIVATION='REGULAR'
                      and ec2.IDE is null
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                    ) e_umg on (e_umg.IDT = VW_TRADE_POSETD.IDT) and e_umg.UNIT=unit.UNIT and e_umg.FIXPARTYROLE=ta.FIXPARTYROLE   

                    left outer join 
                    (
                      /*  Cumul des flux LOV, LPC s’il en existe (donc flux EOD), sinon LPI (donc flux Intraday) */
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as LOV_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from dbo.EVENT e 
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT=@DTBUSINESS
                      where e.EVENTCODE='LPC' and e.EVENTTYPE='LOV' and e.IDSTACTIVATION='REGULAR'
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                      union all
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as LOV_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from dbo.EVENT e 
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT=@DTBUSINESS
                      left outer join dbo.EVENT e2 on e2.IDT=e.IDT and e2.EVENTCODE='LPC' and e2.EVENTTYPE='LOV' and e2.IDSTACTIVATION='REGULAR'
                       left outer join dbo.EVENTCLASS ec2 on ec2.IDE=e2.IDE and ec2.EVENTCLASS='REC' and ec2.DTEVENT=@DTBUSINESS
                       where e.EVENTCODE='LPI' and e.EVENTTYPE='LOV' and e.IDSTACTIVATION='REGULAR'
                      and ec2.IDE is null
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                    ) e_lov on (e_lov.IDT=VW_TRADE_POSETD.IDT) and e_lov.UNIT=unit.UNIT and e_lov.FIXPARTYROLE=ta.FIXPARTYROLE   

                    left outer join 
                    (
                      /* Cumul des flux RMG (affichage du flux LPC/RMG uniquement sur les trades clôturants) */
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as RMG_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from dbo.EVENT e
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT=@DTBUSINESS
                      inner join dbo.EVENTPOSACTIONDET ev_pad on ev_pad.IDE=e.IDE 
                      inner join dbo.POSACTIONDET pad on pad.IDPADET=ev_pad.IDPADET and pad.IDT_CLOSING=e.IDT
                      where e.EVENTCODE='LPC' and e.EVENTTYPE='RMG' and e.IDSTACTIVATION='REGULAR'
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                    ) e_rmg on (e_rmg.IDT=VW_TRADE_POSETD.IDT) and e_rmg.UNIT=unit.UNIT and e_rmg.FIXPARTYROLE=ta.FIXPARTYROLE  

                    left outer join 
                    (
                      /* Cumul des flux OPP  + TAX */
                      select 
                      sum(
                          case
                          when e.IDA_PAY=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then -1 else +1 end
                          when e.IDA_REC=ta.IDA then 
                          case when ta.FIXPARTYROLE= '27' then +1 else -1 end
                          else 0 end
                          * e.VALORISATION) as FEE_SIGNED, 
                      e.IDT, e.UNIT, ta.FIXPARTYROLE
                      from dbo.EVENT e
                      inner join  dbo.TRADEACTOR ta on ta.IDT=e.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE in ('27', '21', '4')
                      inner join dbo.EVENTCLASS ec 
                      on ec.IDE=e.IDE and ec.EVENTCLASS='REC' and ec.DTEVENT=@DTBUSINESS and e.EVENTCODE='OPP' and e.IDSTACTIVATION='REGULAR'
                      group by e.IDT, e.UNIT, ta.FIXPARTYROLE
                    )  e_opp on (e_opp.IDT=VW_TRADE_POSETD.IDT) and e_opp.UNIT=unit.UNIT and e_opp.FIXPARTYROLE=ta.FIXPARTYROLE 


                    where 
                    (VW_TRADE_POSETD.POSKEEPBOOK_DEALER=1)
                    and ae.IDENTIFIER = @ENTITYIDENTIFIER

                    /*IOCOMPARE_NOTCOMPLIANTFILTERS*/

                    group by 

                    unit.UNIT, asset.CONTRACTSYMBOL, asset.PUTCALL,
                    VW_TRADE_POSETD.IDA_DEALER, VW_TRADE_POSETD.IDB_DEALER,
                    VW_TRADE_POSETD.IDA_CLEARER, VW_TRADE_POSETD.IDB_CLEARER,
                    b.IDENTIFIER, 
                    m.ISO10383_ALPHA4, VW_TRADE_POSETD.IDM, ta.FIXPARTYROLE
                    ";
        }

    }

    /// <summary>
    /// Query collection for external datas (stocked inside of the table EXTLDATADET)
    /// </summary>
    /// <typeparam name="TExternal"></typeparam>
    public static class IOCompareQueryExternal<TExternal>
        where TExternal : ISpheresComparer
    {

        /// <summary>
        /// Get the query core relative to the EXTERNAL dataset
        /// </summary>
        /// <remarks>
        /// Rules list
        /// <list type="">
        /// <item>It is MANDATORY to provide an alias for any table inside the query, IFF you want the columns of the table are binded with
        /// some IoCompare parameters (ConfigurationParameter object). 
        /// If an alias is missing the columns defined for the table without alias will not be rattached to any IoCompare parameter</item>
        /// <item>For each table column matching an EXCEPT parameter 
        /// it is MANDATORY to provide a UNIQUE alias equals to the relative parameter ID.</item>
        /// <item>It is mandatory to provide an IDDATA field that identifies the row </item>
        /// <item>It is mandatory to provide a valid DATAROWNUMBER field.</item>
        /// <item>The query core may contain WHERE conditions, but any condition must reference one field only owned by the main table 
        /// (see the BuildKeys regexp).</item>
        /// <item>Any WHERE condition should not contain sub-query or mathematical expressions (see the m_regExParseWhereCondition regexp).</item>
        /// <item>When any of the WHERE conditions are not compliant with the given rules, all of them must be incapsulated inside of the special 
        /// IoCompare tags 
        /// IOCOMPARE_COMPLEXCONDITION_START ... your not compliant WHERE conditions ... IOCOMPARE_COMPLEXCONDITION_END.</item>
        /// <item>The query field order is up to you.</item>
        /// <item>any WHERE conditions must be placed at the very end of the SQL request, 
        /// if you need to place them before then you need to place the IOCOMPARE_NOTCOMPLIANTFILTERS special tag 
        /// inside your request where you want that the custom filters will be added</item>
        /// <item>the query can contain SQL parameters expressed in the TSQL syntax (@xxxx).
        /// the SQL parameter value will be assigned from the Compare parameter having the same name</item>
        /// </list>
        /// </remarks>
        public static string QueryCoreExternal
        {
            get
            {
                if (typeof(TExternal) == typeof(TradeFixCompareData))
                {

                    return QueryExternalTrade();

                }
                else if (typeof(TExternal) == typeof(PositionFixCompareData))
                {

                    return QueryExternalPosition();

                }
                else if (typeof(TExternal) == typeof(TradesInPositionFixCompareData))
                {

                    return QueryExternalTradesInPosition();

                }
                else if (typeof(TExternal) == typeof(CashFlowsCompareData))
                {

                    return QueryExternalCashFlow();

                }
                else if (typeof(TExternal) == typeof(CashFlowsInstrCompareData))
                {

                    return QueryExternalCashFlowInstr();

                }
                else
                {
                    throw new NotSupportedException(String.Format("No sql request for the input type {0}", typeof(TExternal).ToString()));
                }
            }
        }
        // EG 20170127 Qty Long To Decimal (UT_QTY)
        private static string QueryExternalTradesInPosition()
        {
            return @"select ed.IDEXTLDATA,
                    edd.IDEXTLDATADET as IDDATA,
                    '' as COLLECTEDVALUES,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/@LastQty)[1]','UT_QTY') as LASTQTY,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:RptSide/@Side)[1]','UT_ENUMCHAR_OPTIONAL') as SIDE,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/@LastPx)[1]','UT_PRICE') as LASTPX,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@MMY)[1]','UT_ENUM_OPTIONAL') as MMY,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@Sym)[1]','UT_LINKCODE') as SYM,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@Exch)[1]','char(4)') as EXCH,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@StrkPx)[1]','UT_PRICE') as STRKPX,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@PutCall)[1]','UT_ENUMCHAR_OPTIONAL') as PUTCALL,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/@TxnTm)[1]','UT_ENUM_OPTIONAL') as TXNTM,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:RptSide/@Acct)[1]','UT_IDENTIFIER') as ACCT,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:RptSide/@AcctTyp)[1]','UT_ENUMCHAR_OPTIONAL') as ACCTTYP,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:RptSide/@PosEfct)[1]','UT_ENUMCHAR_OPTIONAL') as POSEFCT, 
                    edd.DATAROWNUMBER
                    from dbo.EXTLDATA ed 
                    inner join dbo.EXTLDATADET edd on (edd.IDEXTLDATA=ed.IDEXTLDATA)
                    where ed.BUSINESSTYPE = 'TradesInPositionFIXml'";
        }
        // EG 20170127 Qty Long To Decimal (UT_QTY)
        private static string QueryExternalPosition()
        {
            return @"select ed.IDEXTLDATA,
                    edd.IDEXTLDATADET as IDDATA,
                    edd.DATAXML.value('(fixml:PosRpt/fixml:Qty/@Long)[1]','UT_QTY') as ""LONG"",
                    edd.DATAXML.value('(fixml:PosRpt/fixml:Qty/@Short)[1]','UT_QTY') as ""SHORT"",
                    edd.DATAXML.value('(fixml:PosRpt/fixml:Qty/@Typ)[1]','UT_NUMBER10') as TYP,
                    edd.DATAXML.value('(fixml:PosRpt/fixml:Instrmt/@MMY)[1]','UT_ENUM_OPTIONAL') as MMY,
                    edd.DATAXML.value('(fixml:PosRpt/fixml:Instrmt/@Sym)[1]','UT_LINKCODE') as SYM,
                    edd.DATAXML.value('(fixml:PosRpt/fixml:Instrmt/@Exch)[1]','char(4)') as EXCH,                    
                    edd.DATAXML.value('(fixml:PosRpt/fixml:Instrmt/@StrkPx)[1]','UT_PRICE') as STRKPX,
                    edd.DATAXML.value('(fixml:PosRpt/fixml:Instrmt/@PutCall)[1]','UT_ENUMCHAR_OPTIONAL') as PUTCALL,
                    edd.DATAXML.value('(fixml:PosRpt/@Acct)[1]','UT_IDENTIFIER') as ACCT,
                    edd.DATAXML.value('(fixml:PosRpt/@AcctTyp)[1]','UT_ENUMCHAR_OPTIONAL') as ACCTTYP,
                    edd.DATAROWNUMBER
                    from dbo.EXTLDATA ed 
                    inner join dbo.EXTLDATADET edd on (edd.IDEXTLDATA=ed.IDEXTLDATA)
                    where ed.BUSINESSTYPE = 'PositionFIXml'";
        }
        // EG 20170127 Qty Long To Decimal (UT_QTY)
        private static string QueryExternalTrade()
        {
            //PL 20101209
            return @"select ed.IDEXTLDATA,
                    edd.IDEXTLDATADET as IDDATA,
                    '' as COLLECTEDVALUES,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/@LastQty)[1]','UT_QTY') as LASTQTY,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:RptSide/@Side)[1]','UT_ENUMCHAR_OPTIONAL') as SIDE,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/@LastPx)[1]','UT_PRICE') as LASTPX,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@MMY)[1]','UT_ENUM_OPTIONAL') as MMY,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@Sym)[1]','UT_LINKCODE') as SYM,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@Exch)[1]','char(4)') as EXCH,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@StrkPx)[1]','UT_PRICE') as STRKPX,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:Instrmt/@PutCall)[1]','UT_ENUMCHAR_OPTIONAL') as PUTCALL,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/@TxnTm)[1]','UT_ENUM_OPTIONAL') as TXNTM,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:RptSide/@Acct)[1]','UT_IDENTIFIER') as ACCT,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:RptSide/@AcctTyp)[1]','UT_ENUMCHAR_OPTIONAL') as ACCTTYP,
                    edd.DATAXML.value('(fixml:TrdCaptRpt/fixml:RptSide/@PosEfct)[1]','UT_ENUMCHAR_OPTIONAL') as POSEFCT, 
                    edd.DATAROWNUMBER
                    from dbo.EXTLDATA ed 
                    inner join dbo.EXTLDATADET edd on (edd.IDEXTLDATA=ed.IDEXTLDATA)
                    where ed.BUSINESSTYPE = 'TradeFIXml'";
        }

        private static string QueryExternalCashFlow()
        {
            // RD 20121022 [18112] Add new amount CSBAMT
            // RD 20160606 [22232] Column XXXACT: Use UT_NUMBER10 type instead of UT_VALUE
            return @"select ed.IDEXTLDATA,
                    edd.IDEXTLDATADET as IDDATA,
                    '' as COLLECTEDVALUES,
                    edd.DATAXML.value('(efs:AcctRpt/fixml:RptSide/@Acct)[1]','UT_IDENTIFIER') as ACCT,
                    edd.DATAXML.value('(efs:AcctRpt/fixml:RptSide/@AcctTyp)[1]','UT_ENUMCHAR_OPTIONAL') as ACCTTYP,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComAmt/@amt)[1]','UT_VALUE') as TAXCOMAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComAmt/@ccy)[1]','char(3)') as TAXCOMCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComAmt/@act)[1]','UT_NUMBER10') as TAXCOMACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:CollAmt/@amt)[1]','UT_VALUE') as COLLAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:CollAmt/@ccy)[1]','char(3)') as COLLCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:CollAmt/@act)[1]','UT_NUMBER10') as COLLACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PmtAmt/@amt)[1]','UT_VALUE') as PMTAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PmtAmt/@ccy)[1]','char(3)') as PMTCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PmtAmt/@act)[1]','UT_NUMBER10') as PMTACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PrmAmt/@amt)[1]','UT_VALUE') as PRMAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PrmAmt/@ccy)[1]','char(3)') as PRMCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PrmAmt/@act)[1]','UT_NUMBER10') as PRMACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:VrMrgnAmt/@amt)[1]','UT_VALUE') as VRMRGNAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:VrMrgnAmt/@ccy)[1]','char(3)') as VRMRGNCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:VrMrgnAmt/@act)[1]','UT_NUMBER10') as VRMRGNACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComBrkAmt/@amt)[1]','UT_VALUE') as TAXCOMBRKAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComBrkAmt/@ccy)[1]','char(3)') as TAXCOMBRKCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComBrkAmt/@act)[1]','UT_NUMBER10') as TAXCOMBRKACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:CallAmt/@amt)[1]','UT_VALUE') as CALLAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:CallAmt/@ccy)[1]','char(3)') as CALLCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:CallAmt/@act)[1]','UT_NUMBER10') as CALLACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:RptAmt/@amt)[1]','UT_VALUE') as RPTAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:RptAmt/@ccy)[1]','char(3)') as RPTCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:RptAmt/@act)[1]','UT_NUMBER10') as RPTACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:RptAmt/@amt)[1]','UT_VALUE') as CSBAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:RptAmt/@act)[1]','UT_NUMBER10') as CSBACT,
                    edd.DATAROWNUMBER
                    from dbo.EXTLDATA ed 
                    inner join dbo.EXTLDATADET edd on (edd.IDEXTLDATA=ed.IDEXTLDATA)
                    where ed.BUSINESSTYPE = 'CashFlows'";
        }

        private static string QueryExternalCashFlowInstr()
        {
            // RD 20160606 [22232] Column XXXACT: Use UT_NUMBER10 type instead of UT_VALUE
            return @"select ed.IDEXTLDATA,
                    edd.IDEXTLDATADET as IDDATA,
                    '' as COLLECTEDVALUES,
                    edd.DATAXML.value('(efs:AcctRpt/fixml:RptSide/@Acct)[1]','UT_IDENTIFIER') as ACCT,
                    edd.DATAXML.value('(efs:AcctRpt/fixml:RptSide/@AcctTyp)[1]','UT_ENUMCHAR_OPTIONAL') as ACCTTYP,
                    edd.DATAXML.value('(efs:AcctRpt/fixml:Instrmt/@Sym)[1]','UT_LINKCODE') as SYM,
                    edd.DATAXML.value('(efs:AcctRpt/@Mrk)[1]','varchar(32)') as MRK,
                    edd.DATAXML.value('(efs:AcctRpt/fixml:Instrmt/@PutCall)[1]','UT_ENUMCHAR_OPTIONAL') as PUTCALL,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:UMgAmt/@amt)[1]','UT_VALUE') as UMGAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:UMgAmt/@ccy)[1]','char(3)') as UMGCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:UMgAmt/@act)[1]','UT_NUMBER10') as UMGACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PrmAmt/@amt)[1]','UT_VALUE') as PRMAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PrmAmt/@ccy)[1]','char(3)') as PRMCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:PrmAmt/@act)[1]','UT_NUMBER10') as PRMACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:VrMrgnAmt/@amt)[1]','UT_VALUE') as VRMRGNAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:VrMrgnAmt/@ccy)[1]','char(3)') as VRMRGNCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:VrMrgnAmt/@act)[1]','UT_NUMBER10') as VRMRGNACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:LovAmt/@amt)[1]','UT_VALUE') as LOVAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:LovAmt/@ccy)[1]','char(3)') as LOVCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:LovAmt/@act)[1]','UT_NUMBER10') as LOVACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:RMgAmt/@amt)[1]','UT_VALUE') as RMGAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:RMgAmt/@ccy)[1]','char(3)') as RMGCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:RMgAmt/@act)[1]','UT_NUMBER10') as RMGACT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComBrkAmt/@amt)[1]','UT_VALUE') as TAXCOMBRKAMT,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComBrkAmt/@ccy)[1]','char(3)') as TAXCOMBRKCCY,
                    edd.DATAXML.value('(efs:AcctRpt/efs:Amts/efs:TaxComBrkAmt/@act)[1]','UT_NUMBER10') as TAXCOMBRKACT,
                    edd.DATAROWNUMBER
                    from dbo.EXTLDATA ed 
                    inner join dbo.EXTLDATADET edd on (edd.IDEXTLDATA=ed.IDEXTLDATA)
                    where ed.BUSINESSTYPE = 'CashFlowsInstr'";
        }
    }
}
