using System;

namespace EfsML.Notification
{
    /// <summary>
    /// Represente les différents type de document émis par la messagerie
    /// </summary>
    public enum DocTypeEnum
    {
        pdf,
        txt,
        html,
        xml,
    }

    /// <summary>
    /// Represente les règles d'affichage des frais
    /// </summary>
    public enum ReportFeeDisplayEnum
    {
        /// <summary>
        /// Aucune distinction.
        /// <para>Brokerage: Aucune distinction entre courtage de Trading et courtage de Clearing</para>
        /// <para>Fee: Aucune distinction entre commission de Trading et commission de Clearing</para>
        /// </summary>
        BRO_FEE,
        /// <summary>
        /// Distinction sur les Fees.
        /// <para>Brokerage: Aucune distinction entre courtage de Trading et courtage de Clearing</para>
        /// <para>Fee: Distinction entre commission de Trading et commission de Clearing</para>
        /// <para>Attention, cette règle inclus aussi le cas suivant:</para>
        /// <para>Brokerage:</para> 
        /// <para>- Distinction entre courtage de Trading et courtage de Clearing vis-à-vis du marché</para>
        /// <para>- Pas de distinction vis-à-vis du client</para>
        /// <para>Ainsi, uniquement les courtages vis-à-vis du client qui seront affichés sur les reports</para>
        /// <para>Fee: Distinction entre commission de Trading et commission de Clearing</para>
        /// </summary>
        BRO_TRDCLRFEE,
        /// <summary>
        /// Distinction sur les Brokerages.
        /// <para>Brokerage: Distinction entre courtage de Trading et courtage de Clearing</para>
        /// <para>Fee: Aucune distinction entre commission de Trading et commission de Clearing</para>
        /// <para>Attention, cette règle inclus aussi le cas suivant:</para>
        /// <para>Brokerage: Distinction entre courtage de Trading et courtage de Clearing</para>
        /// <para>Fee:</para> 
        /// <para>- Distinction entre commission de Trading et commission de Clearing vis-à-vis du marché</para>
        /// <para>- Pas de distinction vis-à-vis du client</para>
        /// <para>Ainsi, uniquement les commissions vis-à-vis du client qui seront affichées sur les reports</para>
        /// </summary>
        TRDCLRBRO_FEE,
        /// <summary>
        /// Distinction sur les Fees et sur les Brokerages.
        /// <para>Brokerage: Distinction entre courtage de Trading et courtage de Clearing</para>
        /// <para>Fee: Distinction entre commission de Trading et commission de Clearing</para>
        /// </summary>
        TRDCLRBRO_TRDCLRFEE,
        /// <summary>
        /// Aucune règle
        /// </summary>
        NA,
    }

    /// <summary>
    ///  Type de trade présents dans les éditions
    /// </summary>
    /// FI 20150623 [21149] Add  
    [Flags]
    public enum TradeReportTypeEnum
    {
        /// <summary>
        /// Trade du jour (Business date = @DTJ)
        /// </summary>
        tradeOfDay = 0x1,
        /// <summary>
        /// Trade non encore réglé  (DTBUSINESS &lt;= @DTJ) et (DTSETTLT &lt; @DTJ)
        /// </summary>
        unsetttledTrade = 0x2,
        /// <summary>
        /// Trade réglé (DTBUSINESS &lt;= @DTJ) et (DTSETTLT = @DTJ)
        /// </summary>
        setttledTrade = 0x4,
        /// <summary>
        /// Trade order (Trade d'un ordre)
        /// </summary>
        tradeOfOrder = 0x8,
        
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PostionType
    {
        /// <summary>
        /// 
        /// </summary>
        Business,
        /// <summary>
        /// 
        /// </summary>
        Settlement,
    }

}