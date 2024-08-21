#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EfsML.Business;
using EfsML.Enum;
using FixML.Enum;
using System;
using System.Data;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Classe contenant les informations élémentaires nécessaires au calcul des UTI/PUTI
    /// </summary>
    /// FI 20140916 [20353] Modify (add DC_StrikeDecLocator,Asset_MaturityMonthYear,Clearer_Compartment_Code)
    public class UTIComponents
    {
        #region Members
        /// <summary>
        /// Valeurs possibles MISSING,INVALID,OK
        /// </summary>
        public string StatusUTI_Dealer;
        /// <summary>
        /// Valeurs possibles MISSING,INVALID,OK
        /// </summary>
        public string StatusUTI_Clearer;
        /// <summary>
        /// Valeurs possibles MISSING,INVALID,OK
        /// </summary>
        public string StatusPUTI_Dealer;
        /// <summary>
        /// Valeurs possibles MISSING,INVALID,OK
        /// </summary>
        public string StatusPUTI_Clearer;
        #region Position
        /// <summary>
        /// Id POSUTI
        /// </summary>
        public int PosUti_IdPosUti;
        /// <summary>
        /// IDT du trade ayant ouvert la position (souvent identique à <see cref="Trade_id"/>)
        /// </summary>
        /// FI 20240627 [WI983] add
        public int PosUti_OpeningTrade_Trade_id;
        /// <summary>
        /// Sens du trade ayant ouvert la position (souvent identique à <see cref="Trade_tradeSide"/>)
        /// </summary>
        /// FI 20240627 [WI983] add
        public string PosUti_OpeningTrade_TradeSide;
        /// <summary>
        /// Date d’ouverture de la position (souvent identique à <see cref="Trade_tradeDate"/>)
        /// </summary>
        /// FI 20240627 [WI983] add
        public DateTime PosUti_OpeningTrade_TradeDate;
        #endregion Position

        #region Trade
        /// <summary>
        /// IDT du trade
        /// </summary>
        public int Trade_id;
        /// <summary>
        /// Identifier du trade
        /// </summary>
        public string Trade_Identifier;
        /// <summary>
        /// Date de transaction du trade
        /// </summary>
        public DateTime Trade_tradeDate;
        /// <summary>
        /// Date de clearing du trade
        /// </summary>
        public DateTime Trade_businessDate;
        /// <summary>
        /// "Buyer","Seller"
        /// </summary>
        public string Trade_tradeSide;
        /// <summary>
        /// executionID
        /// </summary>
        public string Trade_executionID;
        /// <summary>
        /// positionID
        /// </summary>
        //PL 20160428 [22107] Newness EUREX C7 3.0 Release
        public string Trade_positionID;
        /// <summary>
        /// orderID
        /// </summary>
        public string Trade_orderID;
        /// <summary>
        /// Valeur numerique assocée à une des valeur de l'enum TrdTypeEnum
        /// </summary>
        public string Trade_trdType;
        #endregion Trade

        #region CSS
        /// <summary>
        /// IDA de la chambre de compensation
        /// </summary>
        public int Css_Id;
        /// <summary>
        /// Identifier de la chambre de compensation
        /// </summary>
        public string Css_Identifier;
        /// <summary>
        /// code BIC de la chambre de compensation
        /// </summary>
        public string Css_BIC;

        /// <summary>
        /// code LEI de la chambre de compensation
        /// </summary>
        // FI 20240425 [26593] UTI/PUTI REFIT
        public string css_LEI;

        #endregion CSS

        #region Market
        /// <summary>
        /// IDM du marché
        /// </summary>
        public int Market_Id;
        /// <summary>
        /// Code MIC du marché
        /// </summary>
        public string Market_MIC;
        #endregion

        #region Derivative Contract
        /// <summary>
        /// IDI du DC
        /// </summary>
        public int DC_IdI;
        /// <summary>
        ///  Type de contract du DC (STD ou FLEX)
        /// </summary>
        public string DC_ContractType;
        /// <summary>
        /// "O" (Option), "F"(Future)
        /// </summary>
        public string DC_Category;
        /// <summary>
        /// Symbol du DC
        /// </summary>
        public string DC_Symbol;
        /// <summary>
        /// Version du DC
        /// </summary>
        public string DC_Attribute;
        /// <summary>
        /// Nbre de decimal du strike 
        /// </summary>
        public Nullable<int> DC_StrikeDecLocator;
        #endregion Derivative Contract

        #region Asset
        /// <summary>
        /// IDASSET de l'asset
        /// </summary>
        public int Asset_Id;
        /// <summary>
        /// Code ISIN de l'asset
        /// </summary>
        public string Asset_ISIN;
        /// <summary>
        /// "0" (Put) or "1" (Call)
        /// </summary>
        public string Asset_PutCall;
        /// <summary>
        /// Strike de l'asset
        /// </summary>
        public decimal Asset_StrikePrice;
        /// <summary>
        /// MaturityDate de l'asset
        /// <para>doit être renseigné avec isnull(MATURITYDATESYS,MATURITYDATE)</para>
        /// </summary>
        public DateTime Asset_MaturityDate;
        /// <summary>
        /// Format de l'échéance selon le format défini sur la règle d'échéance
        /// </summary>
        /// FI 20140916 [20353] add Member
        public string Asset_MaturityMonthYear;
        /// <summary>
        /// Asset CFI Code
        /// </summary>
        /// LP 20240625 [WI936] UTI/PUTI REFIT
        public string Asset_CfiCode;
        #endregion Asset

        #region Entité
        /// <summary>
        /// Id non significatif de l'entité
        /// </summary>
        public int Entity_id;
        /// <summary>
        /// Code LEI de l'acteur entité
        /// </summary>
        public string Entity_LEI;
        /// <summary>
        /// Code membre lorsque l'entité est adhérent de la chambre
        /// </summary>
        public string Entity_CSSMemberCode;
        /// <summary>
        /// Id non significatif de l'acteur REGULATORYOFFICE rattaché à l'entité
        /// </summary>
        public int Entity_RegulatoryOffice_Id;
        #endregion Entité

        #region Dealer
        /// <summary>
        /// Id non significatif de l'acteur dealer (IDA)
        /// </summary>
        public int Dealer_Actor_id;
        /// <summary>
        ///  True si l'acteur est un client
        /// </summary>
        public bool Dealer_Actor_IsCLIENT;
        /// <summary>
        /// Code BIC de l'acteur dealer
        /// </summary>
        public string Dealer_Actor_BIC;
        /// <summary>
        /// Code LEI de l'acteur dealer
        /// </summary>
        public string Dealer_Actor_LEI;
        /// <summary>
        /// Id non significatif du book dealer
        /// </summary>
        public int Dealer_Book_Id;
        #endregion Dealer

        #region Clearer
        /// <summary>
        /// Id non significatif de l'acteur clearer (IDA)
        /// </summary>
        public int Clearer_Actor_Id;
        /// <summary>
        ///  True si l'acteur clearer est une chambre
        /// </summary>
        public bool Clearer_Actor_IsCCP;
        /// <summary>
        /// Code BIC de l'acteur clearer
        /// </summary>
        public string Clearer_Actor_BIC;
        /// <summary>
        /// Code LEI de l'acteur clearer
        /// </summary>
        public string Clearer_Actor_LEI;
        /// <summary>
        /// Id non significatif du book de l'acteur clearer
        /// </summary>
        public int Clearer_Book_Id;
        /// <summary>
        /// Code du compartiment au sein de la chambre (exploitation de la table CLEARINGCOMPART)
        /// </summary>
        /// FI 20140916 [20353] add Member
        public string Clearer_Compartment_Code;
        /// <summary>
        /// Id non significatif de l'acteur REGULATORYOFFICE rattaché au clearer
        /// </summary>
        public int Clearer_RegulatoryOffice_Id;
        #endregion Clearer

        #endregion

        #region properties
        /// <summary>
        ///  Obtient true lorsque le statut est différent de "OK"
        /// </summary>
        public bool IsRecalculUTI_Dealer
        {
            get { return this.StatusUTI_Dealer != "OK"; }
        }
        /// <summary>
        ///  Obtient true lorsque le statut est différent de "OK"
        /// </summary>
        public bool IsRecalculUTI_Clearer
        {
            get { return this.StatusUTI_Clearer != "OK"; }
        }
        /// <summary>
        ///  Obtient true lorsque le statut est différent de "OK"
        /// </summary>
        public bool IsRecalculPUTI_Dealer
        {
            get { return this.StatusPUTI_Dealer != "OK"; }
        }
        /// <summary>
        ///  Obtient true lorsque le statut est différent de "OK"
        /// </summary>
        public bool IsRecalculPUTI_Clearer
        {
            get { return this.StatusPUTI_Clearer != "OK"; }
        }

        /// <summary>
        ///  Obtient un clé unique de posititon constituée avec ID du dealeur, clearer , asset  
        /// </summary>
        public string PositionKey
        {
            get
            {
                StrBuilder strBuiler = new StrBuilder();
                strBuiler.Append(Dealer_Actor_id);
                strBuiler.Append(Dealer_Book_Id);
                strBuiler.Append(Clearer_Actor_Id);
                strBuiler.Append(Clearer_Book_Id);
                strBuiler.Append(DC_IdI);
                strBuiler.Append(Asset_Id);
                return strBuiler.ToString();
            }
        }

        /// <summary>
        ///  Obtient le parsing de Trade_trdType 
        /// </summary>
        /// FI 20140919 [XXXXX] 
        // EG 20171113 Upd
        public Nullable<TrdTypeEnum> Trade_TrdTypeEnum
        {
            get
            {
                return ReflectionTools.ConvertStringToEnumOrNullable<TrdTypeEnum>(this.Trade_trdType);
            }
        }
        /// <summary>
        ///  Obtient le parsing de DC_ContractType 
        /// </summary>
        // EG 20171113 Upd
        public Nullable<DerivativeContractTypeEnum> DC_ContractTypeEnum
        {
            get
            {
                return ReflectionTools.ConvertStringToEnumOrNullable<DerivativeContractTypeEnum>(DC_ContractType);
            }
        }
        /// <summary>
        ///  Obtient le parsing de Asset_PutCall
        /// </summary>
        // EG 20171113 Upd
        public Nullable<PutOrCallEnum> Asset_PutCallEnum
        {
            get
            {
                return ReflectionTools.ConvertStringToEnumOrNullable<PutOrCallEnum>(Asset_PutCall);
            }
        }
        #endregion

        /// <summary>
        /// set properties idPosUti, PosUti_Trade_id, PosUti_TradeDate, PosUti_TradeSide
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="idPosUti"></param>
        /// <param name="idTPosOpening"></param>
        public void InitPUTIComponents(string cs, IDbTransaction dbTransaction, int idPosUti, int idTPosOpening)
        {
            if (Trade_id == 0)
                throw new InvalidOperationException("Trade_id==0.");

            PosUti_IdPosUti = idPosUti;
            PosUti_OpeningTrade_Trade_id = idTPosOpening;

            if (Trade_id == PosUti_OpeningTrade_Trade_id)
            {
                PosUti_OpeningTrade_TradeDate = Trade_tradeDate;
                PosUti_OpeningTrade_TradeSide = Trade_tradeSide;
            }
            else
            {
                SQL_TradeCommon sql_TradeCommon = new SQL_TradeCommon(cs, idTPosOpening)
                {
                    DbTransaction = dbTransaction
                };
                if (sql_TradeCommon.LoadTable(new String[] { "DTTRADE", "SIDE" }))
                {
                    PosUti_OpeningTrade_TradeDate = sql_TradeCommon.DtTrade;
                    PosUti_OpeningTrade_TradeSide = (sql_TradeCommon.Side == "1") ? SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.BUYER) : SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.SELLER);
                }
            }
        }
    }
}
