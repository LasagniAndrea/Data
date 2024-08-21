#region using directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;


using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Notification;
using EfsML.Settlement;
using EfsML.Settlement.Message;

using FixML.Enum;
using FixML.Interface;

using FpML.Enum;
using FpML.Interface;
#endregion using directives

namespace EfsML.Interface
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20140818 [20275] Modify
    /// FI 20150218 [20275] Modify
    /// FI 20150304 [XXPOC] Modify
    /// FI 20151019 [21317] Modify
    /// FI 20161214 [21916] Modify
    public interface IRepository
    {
        #region Accessors
        bool EnumsSpecified { set; get; }
        /// <summary>
        /// Représente les enums
        /// </summary>
        IEnumsRepository[] Enums { get; set; }

        bool MarketSpecified { set; get; }
        /// <summary>
        /// Représente les marchés
        /// </summary>
        IMarketRepository[] Market { get; set; }

        bool DerivativeContractSpecified { set; get; }
        /// <summary>
        /// Représente les DerivativeContrat
        /// </summary>
        IDerivativeContractRepository[] DerivativeContract { set; get; }

        bool AssetETDSpecified { set; get; }
        /// <summary>
        /// Représente les assets ETD
        /// </summary>
        IAssetETDRepository[] AssetETD { set; get; }

        bool AssetRateIndexSpecified { set; get; }
        /// <summary>
        /// Représente les indice de taux
        /// </summary>
        IAssetRateIndexRepository[] AssetRateIndex { get; set; }

        bool AssetEquitySpecified { set; get; }
        /// <summary>
        /// Représente les assets actions
        /// </summary>
        /// FI 20140818 [20275] add
        IAssetEquityRepository[] AssetEquity { get; set; }

        bool AssetIndexSpecified { set; get; }
        /// <summary>
        /// Représente les indices
        /// </summary>
        /// FI 20140818 [20275] add
        IAssetIndexRepository[] AssetIndex { get; set; }

        bool AssetFxRateSpecified { set; get; }
        /// <summary>
        /// Représente les indices
        /// </summary>
        /// EG 20150222 add
        IAssetFxRateRepository[] AssetFxRate { get; set; }


        bool AssetDebtSecuritySpecified { set; get; }
        /// <summary>
        /// Représente les assets DebtSecurity
        /// </summary>
        /// FI 20151019 [21317]
        IAssetDebtSecurityRepository[] AssetDebtSecurity { set; get; }


        bool InstrumentSpecified { set; get; }
        /// <summary>
        /// Représente les instruments
        /// </summary>
        // FI 20150218 [20275] add
        IInstrumentRepository[] Instrument { get; set; }

        bool BookSpecified { set; get; }
        /// <summary>
        /// Représente les books
        /// </summary>
        IBookRepository[] Book { get; set; }

        bool ExtendSpecified { set; get; }
        IExtendRepository[] Extend { get; set; }

        bool CurrencySpecified { set; get; }
        /// <summary>
        /// Représente les devices
        /// </summary>
        ICurrencyRepository[] Currency { get; set; }

        bool InvoicingScopeSpecified { set; get; }
        IInvoicingScopeRepository[] InvoicingScope { get; set; }

        bool TradeLinkSpecified { set; get; }
        ITradeLinkRepository[] TradeLink { get; set; }


        bool ActorSpecified { set; get; }
        /// <summary>
        /// Représente les acteurs
        /// </summary>
        // FI 20150218 [20275] add
        IActorRepository[] Actor { get; set; }


        bool BusinessCenterSpecified { set; get; }
        /// <summary>
        /// Représente les businessCenter
        /// </summary>
        /// FI 20150304 [XXPOC] Add
        IBusinessCenterRepository[] BusinessCenter { get; set; }



        bool AssetCashSpecified { set; get; }
        /// <summary>
        /// Représente les assets de type Cash
        /// </summary>
        /// FI 20160530 [21885] 
        IAssetCashRepository[] AssetCash { get; set; }


        bool AssetCommoditySpecified { set; get; }
        /// <summary>
        /// Représente les assets de type commodity
        /// </summary>
        /// FI 20161214 [21916] add
        IAssetCommodityRepository[] AssetCommodity { get; set; }



        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IExtendRepository : ICommonRepository
    {
        #region Accessors
        bool ExtendDetSpecified { set; get; }
        IExtendDetRepository[] ExtendDet { get; set; }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IExtendDetRepository : ICommonRepository
    {
        #region Accessors
        string DataType { set; get; }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IEnumsRepository
    {
        #region Accessors
        string Id { set; get; }
        string Code { set; get; }
        string ExtCode { set; get; }
        bool EnumsDetSpecified { set; get; }
        IEnumRepository[] EnumsDet { get; set; }
        #endregion Accessors

    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20170206 [21916] Modify
    public interface IEnumRepository
    {
        #region Accessors
        string Id { set; get; }
        string Value { set; get; }
        string ExtValue { set; get; }
        // FI 20170206 [21916] add extAttrbSpecified
        Boolean ExtAttrbSpecified { set; get; }
        string ExtAttrb { set; get; }
        #endregion Accessors
    }
    // EG 20220324 [XXXXX] New 
    public interface IContractRepository : ICommonRepository
    {
        bool ContractCategorySpecified { set; get; }
        string ContractCategory { set; get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public interface ICommonRepository
    {
        #region Accessors
        /// <summary>
        /// Représente la valeur unique dans un flux XML 
        /// </summary>
        string Id { set; get; }

        Boolean OtcmlIdSpecified { set; get; }
        /// <summary>
        /// Identifiant non significatif du repository
        /// </summary>
        string OtcmlId { set; get; }

        /// <summary>
        /// Identifiant non significatif du repository
        /// </summary>
        int OTCmlId { set; get; }

        /// <summary>
        /// Identifiant
        /// </summary>
        string Identifier { set; get; }

        /// <summary>
        /// nom affiché 
        /// </summary>
        string Displayname { set; get; }

        bool DescriptionSpecified { set; get; }
        /// <summary>
        /// Description
        /// </summary>
        string Description { set; get; }

        bool ExtllinkSpecified { set; get; }
        /// <summary>
        /// Identifiant externe
        /// </summary>
        string Extllink { set; get; }
        #endregion Accessors
    }

    /// <summary>
    ///  Représente les données communes à tous les assets
    /// </summary>
    /// FI 20140903 [20275] Add  
    /// FI 20150708 [XXXXX] Modify 
    public interface IAssetRepository : ICommonRepository
    {
        bool IdMSpecified { set; get; }
        /// <summary>
        /// Marché (Id non significatif d'un marché)
        /// </summary>
        int IdM { set; get; }

        bool IdCSpecified { set; get; }
        /// <summary>
        /// Devise de l'asset
        /// </summary>
        string IdC { set; get; }

        bool AltIdentifierSpecified { set; get; }
        /// <summary>
        /// Alternative Identifier (en générale => nom affiché de l'asset, du quel on supprime la devise, le code ISIN, le CodeISO du marché)
        /// </summary>
        string AltIdentifier { set; get; }

        /// <summary>
        /// Obtient l'asset Category
        /// </summary>
        /// FI 20150708 [XXXXX] Add
        Cst.UnderlyingAsset AssetCategory { get; }
    }
}
