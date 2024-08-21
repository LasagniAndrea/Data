#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Interface;
using EfsML.v30.Shared;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Reflection;
#endregion using directives


namespace EfsML.v30.AssetDef
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ShortAsset : IShortAsset
    {
        #region IShortAsset Membres
        ISpheresIdScheme IShortAsset.Instrument
        {
            get { return this.instrument; }
            set { this.instrument = (ProductType)value; }
        }
        //
        //FI 20091223 [16471] add instrumentSpecified
        bool IShortAsset.InstrumentSpecified
        {
            get { return this.instrumentSpecified; }
            set { this.instrumentSpecified = value; }
        }
        //FI 20091223 [16471] add notionalAmountSpecified
        bool IShortAsset.NotionalAmountSpecified
        {
            get { return this.notionalAmountSpecified; }
            set { this.notionalAmountSpecified = value; }
        }
        //FI 20091223 [16471] add notionalAmount
        IMoney IShortAsset.NotionalAmount
        {
            get { return this.notionalAmount; }
            set { this.notionalAmount = (Money)value; }
        }

        //FI 20091223 [16471] add periodNumberOfDaysSpecified
        bool IShortAsset.PeriodNumberOfDaysSpecified
        {
            get { return this.periodNumberOfDaysSpecified; }
            set { this.periodNumberOfDaysSpecified = value; }
        }
        //
        //FI 20091223 [16471] add periodNumberOfDays
        EFS_Integer IShortAsset.PeriodNumberOfDays
        {
            get { return this.periodNumberOfDays; }
            set { this.periodNumberOfDays = value; }
        }
        #endregion

        #region IAsset Membres
        IScheme[] IAsset.InstrumentId
        {
            get { return this.instrumentId; }
            set { this.instrumentId = (InstrumentId[])value; }
        }
        bool IAsset.DescriptionSpecified
        {
            get { return this.descriptionSpecified; }
            set { this.descriptionSpecified = value; }
        }
        EFS_String IAsset.Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class IdentifiedFxRate : IEFS_Array
    {
        #region accessor
        /// <summary>
        /// Représente l'id non signification sous Spheres® de la quotation
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion

        #region Membres de IEFS_Array
        public new object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array

    }

    /// <summary>
    /// 
    /// </summary>
    public partial class UnderlyerCappedFlooredPrice : IUnderlyerCappedFlooredPrice
    {
        #region Constructor
        public UnderlyerCappedFlooredPrice()
        {
            cappedFlooredPrice = new EFS_Decimal();
            cappedFlooredPriceRelativeTo = new PriceReference();
        }
        #endregion Constructor

        #region IUnderlyerCappedFlooredPrice Members
        bool IUnderlyerCappedFlooredPrice.PriceSpecified
        {
            set { this.cappedFlooredPriceSpecified = value; }
            get { return this.cappedFlooredPriceSpecified; }
        }
        EFS_Decimal IUnderlyerCappedFlooredPrice.Price
        {
            set { this.cappedFlooredPrice = value; }
            get { return this.cappedFlooredPrice; }
        }
        //
        bool IUnderlyerCappedFlooredPrice.PriceRelativeToSpecified
        {
            set { this.cappedFlooredPriceRelativeToSpecified = value; }
            get { return this.cappedFlooredPriceRelativeToSpecified; }
        }
        IReference IUnderlyerCappedFlooredPrice.PriceRelativeTo
        {
            set { this.cappedFlooredPriceRelativeTo = (PriceReference)value; }
            get { return this.cappedFlooredPriceRelativeTo; }
        }
        #endregion IUnderlyerCappedFlooredPrice Members
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class FxRateReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
}
