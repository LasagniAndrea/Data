#region using directives
using EFS.ACommon;
using EFS.GUI.Interface;
using EfsML.Interface;
using EfsML.Repository;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
#endregion using directives


namespace EfsML.v30.Repository
{
    /// <summary>
    /// 
    /// </summary>
    // PL 20181001 [24212] RICCODE/BBGCODE
    public partial class DerivativeContractRepository : IDerivativeContractRepository
    {
        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion

        #region IDerivativeContractRepository Membres
        bool IDerivativeContractRepository.AssetCategorySpecified
        {
            set { this.assetCategorySpecified = value; }
            get { return this.assetCategorySpecified; }
        }
        string IDerivativeContractRepository.AssetCategory
        {
            set { this.assetCategory = value; }
            get { return this.assetCategory; }
        }
        bool IDerivativeContractRepository.IdDC_UnlSpecified
        {
            set { this.idDC_UnlSpecified = value; }
            get { return this.idDC_UnlSpecified; }
        }
        string IDerivativeContractRepository.IdDC_Unl
        {
            set { this.idDC_Unl = value; }
            get { return this.idDC_Unl; }
        }
        bool IDerivativeContractRepository.IdAsset_UnlSpecified
        {
            set { this.idAsset_UnlSpecified = value; }
            get { return this.idAsset_UnlSpecified; }
        }
        string IDerivativeContractRepository.IdAsset_Unl
        {
            set { this.idAsset_Unl = value; }
            get { return this.idAsset_Unl; }
        }
        bool IDerivativeContractRepository.IdC_PriceSpecified
        {
            set { this.idC_PriceSpecified = value; }
            get { return this.idC_PriceSpecified; }
        }
        string IDerivativeContractRepository.IdC_Price
        {
            set { this.idC_Price = value; }
            get { return this.idC_Price; }
        }
        bool IDerivativeContractRepository.IdC_NominalSpecified
        {
            set { this.idC_NominalSpecified = value; }
            get { return this.idC_NominalSpecified; }
        }
        string IDerivativeContractRepository.IdC_Nominal
        {
            set { this.idC_Nominal = value; }
            get { return this.idC_Nominal; }
        }
        bool IDerivativeContractRepository.CategorySpecified
        {
            set { this.categorySpecified = value; }
            get { return this.categorySpecified; }
        }
        string IDerivativeContractRepository.Category
        {
            set { this.category = value; }
            get { return this.category; }
        }
        bool IDerivativeContractRepository.ExerciseStyleSpecified
        {
            set { this.exerciseStyleSpecified = value; }
            get { return this.exerciseStyleSpecified; }
        }
        string IDerivativeContractRepository.ExerciseStyle
        {
            set { this.exerciseStyle = value; }
            get { return this.exerciseStyle; }
        }
        bool IDerivativeContractRepository.ContractSymbolSpecified
        {
            set { this.contractSymbolSpecified = value; }
            get { return this.contractSymbolSpecified; }
        }
        string IDerivativeContractRepository.ContractSymbol
        {
            set { this.contractSymbol = value; }
            get { return this.contractSymbol; }
        }
        bool IDerivativeContractRepository.FutValuationMethodSpecified
        {
            set { this.futValuationMethodSpecified = value; }
            get { return this.futValuationMethodSpecified; }
        }
        string IDerivativeContractRepository.FutValuationMethod
        {
            set { this.futValuationMethod = value; }
            get { return this.futValuationMethod; }
        }
        bool IDerivativeContractRepository.SettltMethodSpecified
        {
            set { this.settltMethodSpecified = value; }
            get { return this.settltMethodSpecified; }
        }
        string IDerivativeContractRepository.SettltMethod
        {
            set { this.settltMethod = value; }
            get { return this.settltMethod; }
        }
        bool IDerivativeContractRepository.ContractMultiplierSpecified
        {
            set { this.contractMultiplierSpecified = value; }
            get { return this.contractMultiplierSpecified; }
        }
        decimal IDerivativeContractRepository.ContractMultiplier
        {
            set { this.contractMultiplier = value; }
            get { return this.contractMultiplier; }
        }
        int IDerivativeContractRepository.InstrumentDen
        {
            set { this.instrumentDen = value; }
            get { return this.instrumentDen; }
        }
        bool IDerivativeContractRepository.FactorSpecified
        {
            set { this.factorSpecified = value; }
            get { return this.factorSpecified; }
        }
        decimal IDerivativeContractRepository.Factor
        {
            set { this.factor = value; }
            get { return this.factor; }
        }
        bool IDerivativeContractRepository.IdMSpecified
        {
            set { this.idMSpecified = value; }
            get { return this.idMSpecified; }
        }
        int IDerivativeContractRepository.IdM
        {
            set { this.idM = value; }
            get { return this.idM; }
        }
        bool IDerivativeContractRepository.PriceFmtStyleSpecified
        {
            set { this.priceFmtStyleSpecified = value; }
            get { return this.priceFmtStyleSpecified; }
        }
        /// <summary>
        ///  format d'affichage du prix
        /// </summary>
        /// FI 20150218 [20275] add
        string IDerivativeContractRepository.PriceFmtStyle
        {
            set { this.priceFmtStyle = value; }
            get { return this.priceFmtStyle; }
        }
        bool IDerivativeContractRepository.StrikeFmtStyleSpecified
        {
            set { this.strikeFmtStyleSpecified = value; }
            get { return this.strikeFmtStyleSpecified; }
        }
        /// <summary>
        ///  format d'affichage du strike
        /// </summary>
        /// FI 20150218 [20275] add
        string IDerivativeContractRepository.StrikeFmtStyle
        {
            set { this.strikeFmtStyle = value; }
            get { return this.strikeFmtStyle; }
        }
        bool IDerivativeContractRepository.RICCodeSpecified
        {
            set { this.RICCodeSpecified = value; }
            get { return this.RICCodeSpecified; }
        }
        string IDerivativeContractRepository.RICCode
        {
            set { this.RICCode = value; }
            get { return this.RICCode; }
        }
        bool IDerivativeContractRepository.BBGCodeSpecified
        {
            set { this.BBGCodeSpecified = value; }
            get { return this.BBGCodeSpecified; }
        }
        string IDerivativeContractRepository.BBGCode
        {
            set { this.BBGCode = value; }
            get { return this.BBGCode; }
        }

        bool IDerivativeContractRepository.ExtlDescSpecified
        {
            set { this.extlDescSpecified = value; }
            get { return this.extlDescSpecified; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220906 [XXXXX] Add 
        string IDerivativeContractRepository.ExtlDesc
        {
            set { this.extlDesc = value; }
            get { return this.extlDesc; }
        }


        bool IDerivativeContractRepository.AttribSpecified
        {
            set { this.attribSpecified = value; }
            get { return this.attribSpecified; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220908 [XXXXX] Add 
        string IDerivativeContractRepository.Attrib
        {
            set { this.attrib = value; }
            get { return this.attrib; }
        }

        bool IDerivativeContractRepository.ETDIdentifierFormatSpecified
        {
            set { this.ETDIdentifierFormatSpecified = value; }
            get { return this.ETDIdentifierFormatSpecified; }
        }
        string IDerivativeContractRepository.ETDIdentifierFormat
        {
            set { this.ETDIdentifierFormat = value; }
            get { return this.ETDIdentifierFormat; }
        }

        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    public partial class ExtendRepository : IExtendRepository
    {
        #region IExtendRepository Members
        bool IExtendRepository.ExtendDetSpecified
        {
            get { return this.extendDetSpecified; }
            set { this.extendDetSpecified = value; }
        }
        IExtendDetRepository[] IExtendRepository.ExtendDet
        {
            get { return this.extendDet; }
            set { this.extendDet = (ExtendDetRepository[])value; }
        }
        #endregion IExtendRepository Members
        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion
        //
        #region Methods
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ExtendDetRepository : IExtendDetRepository
    {
        #region IExtendDetRepository Members
        string IExtendDetRepository.DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }
        #endregion IExtendRepository Members
        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion
        //
        #region Methods
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150304 [XXPOC] Modidy
    public partial class CommonRepository : ICommonRepository
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set
            {
                // FI 20150304 [XXPOC] Alimentation de otcmlIdSpecified
                otcmlId = value.ToString();
                this.otcmlIdSpecified = true;
            }
        }
        #endregion Accessors
        #region ICommonRepository Members
        // FI 20150304 [XXPOC] Add
        Boolean ICommonRepository.OtcmlIdSpecified
        {
            get { return this.otcmlIdSpecified; }
            set { this.otcmlIdSpecified = value; }
        }

        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class EnumsRepository : IEnumsRepository
    {
        #region IEnumsRepository Members
        string IEnumsRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string IEnumsRepository.Code
        {
            get { return this.code; }
            set { this.code = value; }
        }
        string IEnumsRepository.ExtCode
        {
            get { return this.extCode; }
            set { this.extCode = value; }
        }
        bool IEnumsRepository.EnumsDetSpecified
        {
            get { return this.enumsDetSpecified; }
            set { this.enumsDetSpecified = value; }
        }
        IEnumRepository[] IEnumsRepository.EnumsDet
        {
            get { return this.enumsDet; }
            set { this.enumsDet = (EnumRepository[])value; }
        }
        #endregion IEnumsRepository Members
        //
        #region Methods
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class EnumRepository : IEnumRepository
    {
        #region IEnumRepository Members
        string IEnumRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string IEnumRepository.Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        string IEnumRepository.ExtValue
        {
            get { return this.extValue; }
            set { this.extValue = value; }
        }
        string IEnumRepository.ExtAttrb
        {
            get { return this.extAttrb; }
            set { this.extAttrb = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        // FI 20170216 [21916] add
        Boolean IEnumRepository.ExtAttrbSpecified
        {
            get { return this.extAttrbSpecified; }
            set { this.extAttrbSpecified = value; }
        }

        #endregion IEnumsRepository Members
        //
        #region Methods
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class InvoicingScopeRepository : IInvoicingScopeRepository
    {
        #region IInvoicingScopeRepository Members

        bool IInvoicingScopeRepository.IdC_FeeSpecified
        {
            set { this.idC_FeeSpecified = value; }
            get { return this.idC_FeeSpecified; }
        }
        string IInvoicingScopeRepository.IdC_Fee
        {
            set { this.idC_Fee = value; }
            get { return this.idC_Fee; }
        }
        bool IInvoicingScopeRepository.EventTypeSpecified
        {
            set { this.eventTypeSpecified = value; }
            get { return this.eventTypeSpecified; }
        }
        string IInvoicingScopeRepository.EventType
        {
            set { this.eventType = value; }
            get { return this.eventType; }
        }
        bool IInvoicingScopeRepository.PaymentTypeSpecified
        {
            set { this.paymentTypeSpecified = value; }
            get { return this.paymentTypeSpecified; }
        }
        string IInvoicingScopeRepository.PaymentType
        {
            set { this.paymentType = value; }
            get { return this.paymentType; }
        }
        #endregion
        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    // PL 20181001 [24211] RICCODE/BBGCODE
    public partial class MarketRepository : IMarketRepository
    {
        #region IMarketRepository Members
        bool IMarketRepository.ISO10383_ALPHA4Specified
        {
            set { this.ISO10383_ALPHA4Specified = value; }
            get { return this.ISO10383_ALPHA4Specified; }
        }
        string IMarketRepository.ISO10383_ALPHA4
        {
            set { this.ISO10383_ALPHA4 = value; }
            get { return this.ISO10383_ALPHA4; }
        }
        bool IMarketRepository.AcronymSpecified
        {
            set { this.acronymSpecified = value; }
            get { return this.acronymSpecified; }
        }
        string IMarketRepository.Acronym
        {
            set { this.acronym = value; }
            get { return this.acronym; }
        }
        bool IMarketRepository.CitySpecified
        {
            set { this.citySpecified = value; }
            get { return this.citySpecified; }
        }
        string IMarketRepository.City
        {
            set { this.city = value; }
            get { return this.city; }
        }
        bool IMarketRepository.ExchangeSymbolSpecified
        {
            set { this.exchangeSymbolSpecified = value; }
            get { return this.exchangeSymbolSpecified; }
        }
        string IMarketRepository.ExchangeSymbol
        {
            set { this.exchangeSymbol = value; }
            get { return this.exchangeSymbol; }
        }
        bool IMarketRepository.ShortIdentifierSpecified
        {
            set { this.shortIdentifierSpecified = value; }
            get { return this.shortIdentifierSpecified; }
        }
        string IMarketRepository.ShortIdentifier
        {
            set { this.shortIdentifier = value; }
            get { return this.shortIdentifier; }
        }
        bool IMarketRepository.Fixml_SecurityExchangeSpecified
        {
            set { this.fixml_SecurityExchangeSpecified = value; }
            get { return this.fixml_SecurityExchangeSpecified; }
        }
        string IMarketRepository.Fixml_SecurityExchange
        {
            set { this.fixml_SecurityExchange = value; }
            get { return this.fixml_SecurityExchange; }
        }
        bool IMarketRepository.RICCodeSpecified
        {
            set { this.RICCodeSpecified = value; }
            get { return this.RICCodeSpecified; }
        }
        string IMarketRepository.RICCode
        {
            set { this.RICCode = value; }
            get { return this.RICCode; }
        }
        bool IMarketRepository.BBGCodeSpecified
        {
            set { this.BBGCodeSpecified = value; }
            get { return this.BBGCodeSpecified; }
        }
        string IMarketRepository.BBGCode
        {
            set { this.BBGCode = value; }
            get { return this.BBGCode; }
        }

        bool IMarketRepository.ETDIdentifierFormatSpecified
        {
            set { this.ETDIdentifierFormatSpecified = value; }
            get { return this.ETDIdentifierFormatSpecified; }
        }
        string IMarketRepository.ETDIdentifierFormat
        {
            set { this.ETDIdentifierFormat = value; }
            get { return this.ETDIdentifierFormat; }
        }

        #endregion

        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class BookRepository : IBookRepository
    {
        #region IBookRepository Members

        bool IBookRepository.IdCSpecified
        {
            set { this.idcSpecified = value; }
            get { return this.idcSpecified; }
        }
        string IBookRepository.IdC
        {
            set { this.idc = value; }
            get { return this.idc; }
        }

        IBookOwnerRepository IBookRepository.Owner
        {
            set { this.owner = (BookOwnerRepository)value; }
            get { return this.owner; }
        }

        #endregion
        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion
        
        public BookRepository()
        {
            owner = new BookOwnerRepository();
        }

    }

    public partial class TradeLinkRepository : ITradeLinkRepository
    {
        #region ITradeLinkRepository Members
        string ITradeLinkRepository.Link
        {
            set { this.link = value; }
            get { return this.link; }
        }
        int ITradeLinkRepository.IdT
        {
            set { this.idt = value; }
            get { return this.idt; }
        }
        bool ITradeLinkRepository.ExecutionIdSpecified
        {
            set { this.executionIdSpecified = value; }
            get { return this.executionIdSpecified; }
        }
        string ITradeLinkRepository.ExecutionId
        {
            set { this.executionId = value; }
            get { return this.executionId; }
        }
        int ITradeLinkRepository.IdT_a
        {
            set { this.idt_a = value; }
            get { return this.idt_a; }
        }
        string ITradeLinkRepository.Identifier_a
        {
            set { this.identifier_a = value; }
            get { return this.identifier_a; }
        }
        #endregion
        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class Repository : IRepository
    {
        #region IRepository Members
        bool IRepository.EnumsSpecified
        {
            set { this.enumsSpecified = value; }
            get { return this.enumsSpecified; }
        }
        IEnumsRepository[] IRepository.Enums
        {
            get { return this.enums; }
            set { this.enums = (EnumsRepository[])value; }
        }
        bool IRepository.DerivativeContractSpecified
        {
            set { this.derivativeContractSpecified = value; }
            get { return this.derivativeContractSpecified; }
        }
        IDerivativeContractRepository[] IRepository.DerivativeContract
        {
            get { return this.derivativeContract; }
            set { this.derivativeContract = (DerivativeContractRepository[])value; }
        }
        bool IRepository.AssetETDSpecified
        {
            set { this.assetETDSpecified = value; }
            get { return this.assetETDSpecified; }
        }
        IAssetETDRepository[] IRepository.AssetETD
        {
            get { return this.assetETD; }
            set { this.assetETD = (AssetETDRepository[])value; }
        }
        bool IRepository.MarketSpecified
        {
            set { this.marketSpecified = value; }
            get { return this.marketSpecified; }
        }
        IMarketRepository[] IRepository.Market
        {
            get { return this.market; }
            set { this.market = (MarketRepository[])value; }
        }
        bool IRepository.BookSpecified
        {
            set { this.bookSpecified = value; }
            get { return this.bookSpecified; }
        }
        IBookRepository[] IRepository.Book
        {
            get { return this.book; }
            set { this.book = (BookRepository[])value; }
        }
        bool IRepository.AssetRateIndexSpecified
        {
            set { this.assetRateIndexSpecified = value; }
            get { return this.assetRateIndexSpecified; }
        }
        IAssetRateIndexRepository[] IRepository.AssetRateIndex
        {
            get { return this.assetRateIndex; }
            set { this.assetRateIndex = (AssetRateIndexRepository[])value; }
        }
        bool IRepository.ExtendSpecified
        {
            set { this.extendSpecified = value; }
            get { return this.extendSpecified; }
        }
        IExtendRepository[] IRepository.Extend
        {
            get { return this.extend; }
            set { this.extend = (ExtendRepository[])value; }
        }
        bool IRepository.CurrencySpecified
        {
            set { this.currencySpecified = value; }
            get { return this.currencySpecified; }
        }
        ICurrencyRepository[] IRepository.Currency
        {
            get { return this.currency; }
            set { this.currency = (CurrencyRepository[])value; }
        }
        bool IRepository.InvoicingScopeSpecified
        {
            set { this.invoicingScopeSpecified = value; }
            get { return this.invoicingScopeSpecified; }
        }
        IInvoicingScopeRepository[] IRepository.InvoicingScope
        {
            get { return this.invoicingScope; }
            set { this.invoicingScope = (InvoicingScopeRepository[])value; }
        }
        bool IRepository.TradeLinkSpecified
        {
            set { this.tradeLinkSpecified = value; }
            get { return this.tradeLinkSpecified; }
        }
        ITradeLinkRepository[] IRepository.TradeLink
        {
            get { return this.tradeLink; }
            set { this.tradeLink = (TradeLinkRepository[])value; }
        }

        bool IRepository.AssetEquitySpecified
        {
            set { this.assetEquitySpecified = value; }
            get { return this.assetEquitySpecified; }
        }
        IAssetEquityRepository[] IRepository.AssetEquity
        {
            get { return this.assetEquity; }
            set { this.assetEquity = (AssetEquityRepository[])value; }
        }


        bool IRepository.AssetDebtSecuritySpecified
        {
            set { this.assetDebtSecuritySpecified = value; }
            get { return this.assetDebtSecuritySpecified; }
        }
        IAssetDebtSecurityRepository[] IRepository.AssetDebtSecurity
        {
            get { return this.assetDebtSecurity; }
            set { this.assetDebtSecurity = (AssetDebtSecurityRepository[])value; }
        }


        bool IRepository.AssetIndexSpecified
        {
            set { this.assetIndexSpecified = value; }
            get { return this.assetIndexSpecified; }
        }
        IAssetIndexRepository[] IRepository.AssetIndex
        {
            get { return this.assetIndex; }
            set { this.assetIndex = (AssetIndexRepository[])value; }
        }

        bool IRepository.AssetFxRateSpecified
        {
            set { this.assetFxRateSpecified = value; }
            get { return this.assetFxRateSpecified; }
        }
        IAssetFxRateRepository[] IRepository.AssetFxRate
        {
            get { return this.assetFxRate; }
            set { this.assetFxRate = (AssetFxRateRepository[])value; }
        }
        bool IRepository.InstrumentSpecified
        {
            set { this.instrumentSpecified = value; }
            get { return this.instrumentSpecified; }
        }
        IInstrumentRepository[] IRepository.Instrument
        {
            get { return this.instrument; }
            set { this.instrument = (InstrumentRepository[])value; }
        }


        bool IRepository.ActorSpecified
        {
            set { this.actorSpecified = value; }
            get { return this.actorSpecified; }
        }
        IActorRepository[] IRepository.Actor
        {
            get { return this.actor; }
            set { this.actor = (ActorRepository[])value; }
        }



        bool IRepository.BusinessCenterSpecified
        {
            set { this.businessCenterSpecified = value; }
            get { return this.businessCenterSpecified; }
        }
        IBusinessCenterRepository[] IRepository.BusinessCenter
        {

            get { return this.businessCenter; }
            set { this.businessCenter = (BusinessCenterRepository[])value; }
        }

        bool IRepository.AssetCashSpecified
        {
            set { this.assetCashSpecified = value; }
            get { return this.assetCashSpecified; }
        }

        IAssetCashRepository[] IRepository.AssetCash
        {
            get { return this.assetCash; }
            set { this.assetCash = (AssetCashRepository[])value; }
        }

        bool IRepository.AssetCommoditySpecified
        {
            set { this.assetCommoditySpecified = value; }
            get { return this.assetCommoditySpecified; }
        }


        IAssetCommodityRepository[] IRepository.AssetCommodity
        {
            get { return this.assetCommodity; }
            set { this.assetCommodity = (AssetCommodityRepository[])value; }
        }



        #endregion IRepository Members
        /// <summary>
        /// Constructor
        /// </summary>
        /// FI 20140903 [20275] add
        /// FI 20160530 [21885] Modify
        /// FI 20161214 [21916] Modify
        public Repository()
        {
            //FI 20140903 [20275] Mise en place de array vide pour usage de requête Link
            this.assetEquity = new AssetEquityRepository[] { };
            this.assetETD = new AssetETDRepository[] { };
            this.assetIndex = new AssetIndexRepository[] { };
            this.assetRateIndex = new AssetRateIndexRepository[] { };
            this.assetFxRate = new AssetFxRateRepository[] { };
            this.assetDebtSecurity = new AssetDebtSecurityRepository[] { };
            this.assetCash = new AssetCashRepository[] { };  // FI 20160530 [21885] Add
            this.assetCommodity = new AssetCommodityRepository[] { };  // FI 20161214 [21916] Add
            this.actor = new ActorRepository[] { };
            this.book = new BookRepository[] { };
            this.currency = new CurrencyRepository[] { };
            this.derivativeContract = new DerivativeContractRepository[] { };
            this.enums = new EnumsRepository[] { };
            this.extend = new ExtendRepository[] { };
            this.invoicingScope = new InvoicingScopeRepository[] { };
            this.market = new MarketRepository[] { };
            this.tradeLink = new TradeLinkRepository[] { };
            this.instrument = new InstrumentRepository[] { };
            this.businessCenter = new BusinessCenterRepository[] { };
            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class AssetRateIndexRepository : IAssetRateIndexRepository
    {
        #region IRateIndexRepository Members
        bool IAssetRateIndexRepository.InformationSourceSpecified
        {
            get { return this.informationSourceSpecified; }
            set { this.informationSourceSpecified = value; }
        }
        IInformationSource IAssetRateIndexRepository.InformationSource
        {
            get { return this.informationSource; }
            set { this.informationSource = (InformationSource)value; }
        }
        bool IAssetRateIndexRepository.RateTypeSpecified
        {
            get { return this.rateTypeSpecified; }
            set { this.rateTypeSpecified = value; }
        }
        string IAssetRateIndexRepository.RateType
        {
            get { return this.rateType; }
            set { this.rateType = value; }
        }
        bool IAssetRateIndexRepository.CalculationRuleSpecified
        {
            get { return this.calculationRuleSpecified; }
            set { this.calculationRuleSpecified = value; }
        }
        string IAssetRateIndexRepository.CalculationRule
        {
            get { return this.calculationRule; }
            set { this.calculationRule = value; }
        }
        #endregion IRateIndexRepository Members

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150708 [XXXXX] Add
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get
            {
                return Cst.UnderlyingAsset.RateIndex;
            }
        }


        #region Methods
        IInformationSource IAssetRateIndexRepository.CreateInformationSource()
        {
            return new InformationSource();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class FxRateRepository : IFxRateRepository
    {
        #region IFxRateRepository Members
        // EG 20240216 [WI850][26600] Ajout Request Date pour édition des confirmation sur facture Migration MAREX
        DateTime IFxRateRepository.RequestDate
        {
            get { return this.requestDate; }
            set { this.requestDate = value; }
        }

        EFS_Date IFxRateRepository.FixingDate
        {
            get { return this.fixingDate; }
            set { this.fixingDate = value; }
        }
        #endregion IFxRateRepository Members


        #region Methods

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CurrencyRepository : ICurrencyRepository
    {
        #region ICurrencyRepository Members
        bool ICurrencyRepository.SymbolSpecified
        {
            get { return this.symbolSpecified; }
            set { this.symbolSpecified = value; }
        }
        string ICurrencyRepository.Symbol
        {
            get { return this.symbol; }
            set { this.symbol = value; }
        }
        bool ICurrencyRepository.SymbolalignSpecified
        {
            get { return this.symbolalignSpecified; }
            set { this.symbolalignSpecified = value; }
        }
        string ICurrencyRepository.Symbolalign
        {
            get { return this.symbolalign; }
            set { this.symbolalign = value; }
        }
        bool ICurrencyRepository.ISO4217_num3Specified
        {
            get { return this.ISO4217_num3Specified; }
            set { this.ISO4217_num3Specified = value; }
        }
        string ICurrencyRepository.ISO4217_num3
        {
            get { return this.ISO4217_num3; }
            set { this.ISO4217_num3 = value; }
        }
        bool ICurrencyRepository.FactorSpecified
        {
            get { return this.factorSpecified; }
            set { this.factorSpecified = value; }
        }
        int ICurrencyRepository.Factor
        {
            get { return this.factor; }
            set { this.factor = value; }
        }
        bool ICurrencyRepository.RounddirSpecified
        {
            get { return this.rounddirSpecified; }
            set { this.rounddirSpecified = value; }
        }
        RoundingDirectionEnum ICurrencyRepository.Rounddir
        {
            get { return this.rounddir; }
            set { this.rounddir = value; }
        }
        bool ICurrencyRepository.RoundprecSpecified
        {
            get { return this.roundprecSpecified; }
            set { this.roundprecSpecified = value; }
        }
        int ICurrencyRepository.Roundprec
        {
            get { return this.roundprec; }
            set { this.roundprec = value; }
        }

        bool ICurrencyRepository.FxrateSpecified
        {
            get { return this.fxrateSpecified; }
            set { this.fxrateSpecified = value; }
        }
        // RD 20131015 [19067] Extrait de compte / un taux de change par jour        
        IFxRateRepository[] ICurrencyRepository.Fxrate
        {
            get { return this.fxrate; }
            set { this.fxrate = (FxRateRepository[])value; }
        }
        #endregion ICurrencyRepository Members
        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion
    }

    /// FI 20150513 [XXXXX] Add 
    public partial class AssetRepository : IAssetRepository
    {
        #region IAssetRepository Membres
        bool IAssetRepository.AltIdentifierSpecified
        {
            get { return this.altIdentifierSpecified; }
            set { this.altIdentifierSpecified = value; }
        }
        string IAssetRepository.AltIdentifier
        {
            get { return this.altIdentifier; }
            set { this.altIdentifier = value; }
        }

        
        bool IAssetRepository.IdMSpecified
        {
            get { return this.idMSpecified; }
            set { this.idMSpecified = value; }
        }
        int IAssetRepository.IdM
        {
            get { return this.idM; }
            set { this.idM = value; }
        }

        bool IAssetRepository.IdCSpecified
        {
            get { return this.idCSpecified; }
            set { this.idCSpecified = value; }
        }
        string IAssetRepository.IdC
        {
            get { return this.idC; }
            set { this.idC = value; }
        }

        /// FI 20150708 [XXXXX] Add
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get
            {
                throw new NotSupportedException("assetCategory is not supported"); 
            }
        }
        
        
        #endregion
    
    }

    /// <summary>
    /// 
    /// </summary>
    // PM 20140516 [19970][19259] Ajout de idC et idCSpecified
    // FI 20150513 [XXXXX] add  IAssetRepository.asset
    // FI 20150708 [XXXXX] Modify
    // PL 20181001 [24213] RICCODE/BBGCODE
    public partial class AssetETDRepository : IAssetETDRepository
    {
        #region IETDRepository Membres
        int IAssetETDRepository.IdDC
        {
            set { this.idDC = value; }
            get { return this.idDC; }
        }
        bool IAssetETDRepository.AssetSymbolSpecified
        {
            set { this.assetSymbolSpecified = value; }
            get { return this.assetSymbolSpecified; }
        }
        string IAssetETDRepository.AssetSymbol
        {
            set { this.assetSymbol = value; }
            get { return this.assetSymbol; }
        }

        bool IAssetETDRepository.CFICodeSpecified
        {
            set { this.CFICodeSpecified = value; }
            get { return this.CFICodeSpecified; }
        }
        string IAssetETDRepository.CFICode
        {
            set { this.CFICode = value; }
            get { return this.CFICode; }
        }
        bool IAssetETDRepository.ISINCodeSpecified
        {
            set { this.ISINCodeSpecified = value; }
            get { return this.ISINCodeSpecified; }
        }
        string IAssetETDRepository.ISINCode
        {
            set { this.ISINCode = value; }
            get { return this.ISINCode; }
        }
        bool IAssetETDRepository.RICCodeSpecified
        {
            set { this.RICCodeSpecified = value; }
            get { return this.RICCodeSpecified; }
        }
        string IAssetETDRepository.RICCode
        {
            set { this.RICCode = value; }
            get { return this.RICCode; }
        }
        bool IAssetETDRepository.BBGCodeSpecified
        {
            set { this.BBGCodeSpecified = value; }
            get { return this.BBGCodeSpecified; }
        }
        string IAssetETDRepository.BBGCode
        {
            set { this.BBGCode = value; }
            get { return this.BBGCode; }
        }

        bool IAssetETDRepository.ContractMultiplierSpecified
        {
            set { this.contractMultiplierSpecified = value; }
            get { return this.contractMultiplierSpecified; }
        }
        decimal IAssetETDRepository.ContractMultiplier
        {
            set { this.contractMultiplier = value; }
            get { return this.contractMultiplier; }
        }
        bool IAssetETDRepository.StrikePriceSpecified
        {
            set { this.strikePriceSpecified = value; }
            get { return this.strikePriceSpecified; }
        }
        RepositoryPrice IAssetETDRepository.StrikePrice
        {
            set { this.strikePrice = value; }
            get { return this.strikePrice; }
        }
        bool IAssetETDRepository.PutCallSpecified
        {
            set { this.putCallSpecified = value; }
            get { return this.putCallSpecified; }
        }
        string IAssetETDRepository.PutCall
        {
            set { this.putCall = value; }
            get { return this.putCall; }
        }

        bool IAssetETDRepository.AiiSpecified
        {
            set { this.aiiSpecified = value; }
            get { return this.aiiSpecified; }
        }
        string IAssetETDRepository.Aii
        {
            set { this.aii = value; }
            get { return this.aii; }
        }
        bool IAssetETDRepository.FactorSpecified
        {
            set { this.factorSpecified = value; }
            get { return this.factorSpecified; }
        }
        decimal IAssetETDRepository.Factor
        {
            set { this.factor = value; }
            get { return this.factor; }
        }
        bool IAssetETDRepository.MaturityDateSpecified
        {
            set { this.maturityDateSpecified = value; }
            get { return this.maturityDateSpecified; }
        }
        /// <summary>
        /// 
        /// </summary>
        DateTime IAssetETDRepository.MaturityDate
        {
            set { this.maturityDate = value; }
            get { return this.maturityDate; }
        }
        bool IAssetETDRepository.MaturityMonthYearSpecified
        {
            set { this.maturityMonthYearSpecified = value; }
            get { return this.maturityMonthYearSpecified; }
        }
        string IAssetETDRepository.MaturityMonthYear
        {
            set { this.maturityMonthYear = value; }
            get { return this.maturityMonthYear; }
        }
        bool IAssetETDRepository.ExpIndSpecified
        {
            set { this.expIndSpecified = value; }
            get { return this.expIndSpecified; }
        }
        int IAssetETDRepository.ExpInd
        {
            set { this.expInd = value; }
            get { return this.expInd; }
        }
        bool IAssetETDRepository.ConvertedPricesSpecified
        {
            set { this.ConvertedPricesSpecified = value; }
            get { return this.ConvertedPricesSpecified; }
        }
        IConvertedPrices IAssetETDRepository.ConvertedPrices
        {
            set { this._ConvertedPrices = (ConvertedPrices)value; }
            get { return this._ConvertedPrices; }
        }
        #endregion
        
        /// <summary>
        ///  Obtient ExchangeTradedContract
        /// </summary>
        /// FI 20150708 [XXXXX] Add
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get { return Cst.UnderlyingAsset.ExchangeTradedContract; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20140818 [20275] Add AssetEquityRepository
    /// FI 20150513 [XXXXX] Modify
    public partial class AssetEquityRepository : IAssetEquityRepository
    {
        #region IAssetEquityRepository Membres
        bool IAssetEquityRepository.AssetSymbolSpecified
        {
            set { this.assetSymbolSpecified = value; }
            get { return this.assetSymbolSpecified; }
        }
        string IAssetEquityRepository.AssetSymbol
        {
            set { this.assetSymbol = value; }
            get { return this.assetSymbol; }
        }

        bool IAssetEquityRepository.BBGCodeSpecified
        {
            set { this.BBGCodeSpecified = value; }
            get { return this.BBGCodeSpecified; }
        }
        string IAssetEquityRepository.BBGCode
        {
            set { this.BBGCode = value; }
            get { return this.BBGCode; }
        }

        bool IAssetEquityRepository.RICCodeSpecified
        {
            set { this.RICCodeSpecified = value; }
            get { return this.RICCodeSpecified; }
        }
        string IAssetEquityRepository.RICCode
        {
            set { this.RICCode = value; }
            get { return this.RICCode; }
        }

        bool IAssetEquityRepository.ISINCodeSpecified
        {
            set { this.ISINCodeSpecified = value; }
            get { return this.ISINCodeSpecified; }
        }
        string IAssetEquityRepository.ISINCode
        {
            set { this.ISINCode = value; }
            get { return this.ISINCode; }
        }

        bool IAssetEquityRepository.CFICodeSpecified
        {
            set { this.CFICodeSpecified = value; }
            get { return this.CFICodeSpecified; }
        }
        string IAssetEquityRepository.CFICode
        {
            set { this.CFICode = value; }
            get { return this.CFICode; }
        }
        #endregion

        #region IAssetRepository Membres
        bool IAssetRepository.AltIdentifierSpecified
        {
            set { this.altIdentifierSpecified = value; }
            get { return this.altIdentifierSpecified; }
        }
        string IAssetRepository.AltIdentifier
        {
            set { this.altIdentifier = value; }
            get { return this.altIdentifier; }
        }
        bool IAssetRepository.IdMSpecified
        {
            set { this.idMSpecified = value; }
            get { return this.idMSpecified; }
        }
        int IAssetRepository.IdM
        {
            set { this.idM = value; }
            get { return this.idM; }
        }
        bool IAssetRepository.IdCSpecified
        {
            set { this.idCSpecified = value; }
            get { return this.idCSpecified; }
        }
        string IAssetRepository.IdC
        {
            set { this.idC = value; }
            get { return this.idC; }
        }
        #endregion IAssetEquityRepository Membres

        /// <summary>
        /// Obtient EquityAsset
        /// </summary>
        /// FI 20150708 [XXXXX] Add
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get
            {
                return Cst.UnderlyingAsset.EquityAsset;
            }
        }

    }
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AssetDebtSecurityRepository : IAssetDebtSecurityRepository
    {
        #region IAssetDebtSecurityRepository Membres


        bool IAssetDebtSecurityRepository.BBGCodeSpecified
        {
            set { this.BBGCodeSpecified = value; }
            get { return this.BBGCodeSpecified; }
        }
        string IAssetDebtSecurityRepository.BBGCode
        {
            set { this.BBGCode = value; }
            get { return this.BBGCode; }
        }

        bool IAssetDebtSecurityRepository.RICCodeSpecified
        {
            set { this.RICCodeSpecified = value; }
            get { return this.RICCodeSpecified; }
        }
        string IAssetDebtSecurityRepository.RICCode
        {
            set { this.RICCode = value; }
            get { return this.RICCode; }
        }

        bool IAssetDebtSecurityRepository.ISINCodeSpecified
        {
            set { this.ISINCodeSpecified = value; }
            get { return this.ISINCodeSpecified; }
        }
        string IAssetDebtSecurityRepository.ISINCode
        {
            set { this.ISINCode = value; }
            get { return this.ISINCode; }
        }

        bool IAssetDebtSecurityRepository.CFICodeSpecified
        {
            set { this.CFICodeSpecified = value; }
            get { return this.CFICodeSpecified; }
        }
        string IAssetDebtSecurityRepository.CFICode
        {
            set { this.CFICode = value; }
            get { return this.CFICode; }
        }

        bool IAssetDebtSecurityRepository.SEDOLCodeSpecified
        {
            set { this.SEDOLCodeSpecified = value; }
            get { return this.SEDOLCodeSpecified; }
        }
        string IAssetDebtSecurityRepository.SEDOLCode
        {
            set { this.SEDOLCode = value; }
            get { return this.SEDOLCode; }
        }


        bool IAssetDebtSecurityRepository.ParValueSpecified
        {
            set { this.parValueSpecified = value; }
            get { return this.parValueSpecified; }
        }

        EfsML.Notification.ReportAmount IAssetDebtSecurityRepository.ParValue
        {
            set { this.parValue = value; }
            get { return this.parValue; }
        }

        #endregion

        #region IAssetRepository Membres
        bool IAssetRepository.AltIdentifierSpecified
        {
            set { this.altIdentifierSpecified = value; }
            get { return this.altIdentifierSpecified; }
        }
        string IAssetRepository.AltIdentifier
        {
            set { this.altIdentifier = value; }
            get { return this.altIdentifier; }
        }
        bool IAssetRepository.IdMSpecified
        {
            set { this.idMSpecified = value; }
            get { return this.idMSpecified; }
        }
        int IAssetRepository.IdM
        {
            set { this.idM = value; }
            get { return this.idM; }
        }
        bool IAssetRepository.IdCSpecified
        {
            set { this.idCSpecified = value; }
            get { return this.idCSpecified; }
        }
        string IAssetRepository.IdC
        {
            set { this.idC = value; }
            get { return this.idC; }
        }
        #endregion IAssetEquityRepository Membres

        /// <summary>
        /// Obtient Bond
        /// </summary>
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get
            {
                return Cst.UnderlyingAsset.Bond;
            }
        }

    }
    
    /// <summary>
    /// 
    /// </summary>
    /// FI 20140818 [20275] Add AssetIndexRepository
    public partial class AssetIndexRepository : IAssetIndexRepository
    {
        #region IAssetIndexRepository
        bool IAssetIndexRepository.AssetSymbolSpecified
        {
            set { this.assetSymbolSpecified = value; }
            get { return this.assetSymbolSpecified; }
        }
        string IAssetIndexRepository.AssetSymbol
        {
            set { this.assetSymbol = value; }
            get { return this.assetSymbol; }
        }

        bool IAssetIndexRepository.BBGCodeSpecified
        {
            set { this.BBGCodeSpecified = value; }
            get { return this.BBGCodeSpecified; }
        }
        string IAssetIndexRepository.BBGCode
        {
            set { this.BBGCode = value; }
            get { return this.BBGCode; }
        }

        bool IAssetIndexRepository.RICCodeSpecified
        {
            set { this.RICCodeSpecified = value; }
            get { return this.RICCodeSpecified; }
        }
        string IAssetIndexRepository.RICCode
        {
            set { this.RICCode = value; }
            get { return this.RICCode; }
        }
        #endregion

        /// <summary>
        /// Obtient Index
        /// </summary>
        /// FI 20150708 [XXXXX] Add
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get
            {
                return Cst.UnderlyingAsset.Index;
            }
        }

    }

    /// <summary>
    /// Représente un asset de change
    /// </summary>
    /// FI 20150331 [XXPOC] Modify
    public partial class AssetFxRateRepository : IAssetFxRateRepository
    {
        #region IAssetFxRateRepository Members
        IInformationSource IAssetFxRateRepository.PrimaryRateSrc
        {
            get { return this.primaryRateSrc; }
            set { this.primaryRateSrc = (InformationSource)value; }
        }
        IQuotedCurrencyPair IAssetFxRateRepository.QuotedCurrencyPair
        {
            get { return this.quotedCurrencyPair; }
            set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
        }
        IBusinessCenterTime IAssetFxRateRepository.FixingTime
        {
            get { return this.fixingTime; }
            set { this.fixingTime = (BusinessCenterTime)value; }
        }
        #endregion IAssetFxRateRepository Members

        #region IAssetRepository Members
        bool IAssetRepository.IdMSpecified
        {
            get { return this.idMSpecified; }
            set { this.idMSpecified = value; }
        }
        int IAssetRepository.IdM
        {
            get { return this.idM; }
            set { this.idM = value; }
        }

        bool IAssetRepository.IdCSpecified
        {
            get { return this.idCSpecified; }
            set { this.idCSpecified = value; }
        }
        string IAssetRepository.IdC
        {
            get { return this.idC; }
            set { this.idC = value; }
        }
        #endregion IAssetRepository Members

        /// <summary>
        /// Obtient FxRateAsset
        /// </summary>
        /// FI 20150708 [XXXXX] Add
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get
            {
                return Cst.UnderlyingAsset.FxRateAsset;
            }
        }

        #region constructor
        public AssetFxRateRepository() :
            base()
        {
            quotedCurrencyPair = new QuotedCurrencyPair();
            primaryRateSrc = new InformationSource();
            fixingTime = new BusinessCenterTime();
        }
        #endregion constructor

    }


    /// <summary>
    /// Réprésente le référentiel d'un asset Cash
    /// </summary>
    /// FI 20160530 [21885] Add
    public partial class AssetCashRepository : IAssetCashRepository
    {

        #region IAssetRepository Membres
        bool IAssetRepository.AltIdentifierSpecified
        {
            set { this.altIdentifierSpecified = value; }
            get { return this.altIdentifierSpecified; }
        }
        string IAssetRepository.AltIdentifier
        {
            set { this.altIdentifier = value; }
            get { return this.altIdentifier; }
        }
        bool IAssetRepository.IdMSpecified
        {
            set { this.idMSpecified = value; }
            get { return this.idMSpecified; }
        }
        int IAssetRepository.IdM
        {
            set { this.idM = value; }
            get { return this.idM; }
        }
        bool IAssetRepository.IdCSpecified
        {
            set { this.idCSpecified = value; }
            get { return this.idCSpecified; }
        }
        string IAssetRepository.IdC
        {
            set { this.idC = value; }
            get { return this.idC; }
        }
        #endregion IAssetEquityRepository Membres

        /// <summary>
        ///  Obtient Cash
        /// </summary>
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get
            {
                return Cst.UnderlyingAsset.Cash;
            }
        }


        #region constructor
        public AssetCashRepository() :
            base()
        {

        }
        #endregion constructor


    }

    /// <summary>
    /// Réprésente le référentiel d'un asset commodity
    /// </summary>
    /// FI 20161214 [21916] Add
    /// FI 20170116 [21916] Modify
    /// FI 20170201 [21916] Modify
    public partial class AssetCommodityRepository : IAssetCommodityRepository
    {

        /// <summary>
        ///  Obtient Commodity
        /// </summary>
        Cst.UnderlyingAsset IAssetRepository.AssetCategory
        {
            get
            {
                return Cst.UnderlyingAsset.Commodity;
            }
        }

        #region constructor
        public AssetCommodityRepository() :
            base()
        {

        }
        #endregion constructor

        string IAssetCommodityRepository.AssetSymbol
        {
            get { return this.assetSymbol; }
            set { this.assetSymbol = value; }
        }
        Boolean IAssetCommodityRepository.AssetSymbolSpecified
        {
            get { return this.assetSymbolSpecified; }
            set { this.assetSymbolSpecified = value; }
        }

        Boolean IAssetCommodityRepository.ContractSymbolSpecified
        {
            get { return this.contractSymbolSpecified; }
            set { this.contractSymbolSpecified = value; }
        }
        string IAssetCommodityRepository.ContractSymbol
        {
            get { return this.contractSymbol; }
            set { this.contractSymbol = value; }
        }

        Boolean IAssetCommodityRepository.ExchContractSymbolSpecified
        {
            get { return this.exchContractSymbolSpecified; }
            set { this.exchContractSymbolSpecified = value; }
        }
        string IAssetCommodityRepository.ExchContractSymbol
        {
            get { return this.exchContractSymbol; }
            set { this.exchContractSymbol = value; }
        }

        Boolean IAssetCommodityRepository.QtyUnitSpecified
        {
            get { return this.qtyUnitSpecified; }
            set { this.qtyUnitSpecified = value; }
        }

        string IAssetCommodityRepository.QtyUnit
        {
            get { return this.qtyUnit; }
            set { this.qtyUnit = value; }
        }

        Boolean IAssetCommodityRepository.DeliveryPointSpecified
        {
            get { return this.deliveryPointSpecified; }
            set { this.deliveryPointSpecified = value; }
        }
        string IAssetCommodityRepository.DeliveryPoint
        {
            get { return this.deliveryPoint; }
            set { this.deliveryPoint = value; }
        }


        Boolean IAssetCommodityRepository.DeliveryTimezoneSpecified
        {
            get { return this.deliveryTimezoneSpecified; }
            set { this.deliveryTimezoneSpecified = value; }
        }
        string IAssetCommodityRepository.DeliveryTimezone
        {
            get { return this.deliveryTimezone; }
            set { this.deliveryTimezone = value; }
        }

        Boolean IAssetCommodityRepository.DurationSpecified
        {
            get { return this.durationSpecified; }
            set { this.durationSpecified = value; }
        }
        string IAssetCommodityRepository.Duration
        {
            get { return this.duration; }
            set { this.duration = value; }
        }
        
        Boolean IAssetCommodityRepository.QtyScaleSpecified
        {
            get { return this.QtyScaleSpecified; }
            set { this.QtyScaleSpecified = value; }
        }

        /// <summary>
        /// Nombre de digit utilisés pour la partie décimale
        /// </summary>
        // FI 20170201 [21916] Modify
        int IAssetCommodityRepository.QtyScale
        {
            get { return this.QtyScale; }
            set { this.QtyScale = value; }
        }
    }

    // EG 20220324 [XXXXX] New
    public partial class ContractRepository : IContractRepository
    {
        Boolean IContractRepository.ContractCategorySpecified
        {
            get { return this.contractCategorySpecified; }
            set { this.contractCategorySpecified = value; }
        }
        string IContractRepository.ContractCategory
        {
            get { return this.contractCategory; }
            set { this.contractCategory = value; }
        }
    }

    /// <summary>
    /// Représente un instrument
    /// </summary>
    /// FI 20150218 [20275] Modify
    /// FI 20150625 [21149] Modify
    public partial class InstrumentRepository : IInstrumentRepository
    {
        #region ICommonRepository Members
        int ICommonRepository.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string ICommonRepository.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        string ICommonRepository.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        string ICommonRepository.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string ICommonRepository.Displayname
        {
            set { this.displayname = value; }
            get { return this.displayname; }
        }
        bool ICommonRepository.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        string ICommonRepository.Description
        {
            set { this.description = value; }
            get { return this.description; }
        }
        bool ICommonRepository.ExtllinkSpecified
        {
            get { return this.extllinkSpecified; }
            set { this.extllinkSpecified = value; }
        }
        string ICommonRepository.Extllink
        {
            set { this.extllink = value; }
            get { return this.extllink; }
        }
        #endregion

        #region IInstrumentRepository Members
        string IInstrumentRepository.Product
        {
            set { this.product = value; }
            get { return this.product; }
        }
        string IInstrumentRepository.GProduct
        {
            set { this.gProduct = value; }
            get { return this.gProduct; }
        }
        Boolean IInstrumentRepository.IsMargining
        {
            set { this.isMargining = value; }
            get { return this.isMargining; }
        }
        Boolean IInstrumentRepository.IsFunding
        {
            set { this.isFunding = value; }
            get { return this.isFunding; }
        }
        #endregion
    }

    /// <summary>
    /// Représente une référence vers le repository Actor
    /// </summary>
    /// FI 20150603 [XXXXX] Add
    public partial class ActorRepositoryReference : IReference
    {
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }

    }

    /// <summary>
    ///  Représente le propriétaire d'un book
    /// </summary>
    /// FI 20150603 [XXXXX] Add
    /// FI 20150310 [XXXXX] Modify 
    public partial class BookOwnerRepository : IBookOwnerRepository
    {
        // FI 20150310 [XXXXX] OTCmlId en commentaire
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public int OTCmlId
        //{
        //    get { return Convert.ToInt32(otcmlId); }
        //    set { otcmlId = value.ToString(); }
        //}

        //int ISpheresId.OTCmlId
        //{
        //    set { this.OTCmlId = value; }
        //    get { return this.OTCmlId; }
        //}
        //string ISpheresId.otcmlId
        //{
        //    get { return this.otcmlId; }
        //    set { this.otcmlId = value; }
        //}

        string IReference.HRef
        {
            get { return this.href; }
            set { this.href = value; }
        }
    }

    /// <summary>
    /// Représente un acteur
    /// </summary>
    /// FI 20150603 [XXXXX] Add
    /// FI 20160530 [21885] Modify
    public partial class ActorRepository : IActorRepository
    {
        #region IActorRepository Members
        bool IActorRepository.ISO10383_ALPHA4Specified
        {
            get { return this.ISO10383_ALPHA4Specified; }
            set { this.ISO10383_ALPHA4Specified = value; }
        }
        string IActorRepository.ISO10383_ALPHA4
        {
            get { return this.ISO10383_ALPHA4; }
            set { this.ISO10383_ALPHA4 = value; }
        }

        bool IActorRepository.ISO17442Specified
        {
            set { this.ISO17442Specified = value; }
            get { return this.ISO17442Specified; }

        }
        string IActorRepository.ISO17442
        {
            get { return this.ISO17442; }
            set { this.ISO17442 = value; }
        }

        bool IActorRepository.TelephoneNumberSpecified
        {
            get { return this.telephoneNumberSpecified; }
            set { this.telephoneNumberSpecified = value; }
        }

        string IActorRepository.TelephoneNumber
        {
            get { return this.telephoneNumber; }
            set { this.telephoneNumber = value; }
        }

        bool IActorRepository.MobileNumberSpecified
        {
            get { return this.mobileNumberSpecified; }
            set { this.mobileNumberSpecified = value; }
        }

        string IActorRepository.MobileNumber
        {
            get { return this.mobileNumber; }
            set { this.mobileNumber = value; }
        }

        bool IActorRepository.EmailSpecified
        {
            get { return this.emailSpecified; }
            set { this.emailSpecified = value; }
        }

        string IActorRepository.Email
        {
            get { return this.email; }
            set { this.email = value; }
        }

        bool IActorRepository.WebSpecified
        {
            get { return this.webSpecified; }
            set { this.webSpecified = value; }
        }

        string IActorRepository.Web
        {
            get { return this.web; }
            set { this.web = value; }
        }
        bool IActorRepository.AddressSpecified
        {
            get { return this.addressSpecified; }
            set { this.addressSpecified = value; }
        }
        IAddress IActorRepository.Address
        {
            get { return this.address; }
            set { this.address = (Address)value; }
        }
        #endregion
    }
    /// <summary>
    /// Représente un businessCenter
    /// </summary>
    public partial class BusinessCenterRepository : IBusinessCenterRepository
    {
        
    }
}
