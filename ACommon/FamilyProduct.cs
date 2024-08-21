using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EFS.ACommon
{
    /// EG 20161122 New Commodity Derivative (NOT USED)
    public sealed class ProductTools
    {
        public enum FamilyEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("N/A")]
            NotAvailable,
            [System.Xml.Serialization.XmlEnumAttribute("BO")]
            BondOption,
            [System.Xml.Serialization.XmlEnumAttribute("CD")]
            CreditDerivative,
            [System.Xml.Serialization.XmlEnumAttribute("COMD")]
            CommoditiyDerivative,
            [System.Xml.Serialization.XmlEnumAttribute("COMS")]
            CommoditySpot,
            [System.Xml.Serialization.XmlEnumAttribute("DSE")]
            DebtSecurity,
            [System.Xml.Serialization.XmlEnumAttribute("EQD")]
            EquityDerivative,
            [System.Xml.Serialization.XmlEnumAttribute("EQF")]
            EQF,
            [System.Xml.Serialization.XmlEnumAttribute("RTS")]
            ReturnSwap,
            [System.Xml.Serialization.XmlEnumAttribute("EQVS")]
            EQVS,         // Equity variance swap
            [System.Xml.Serialization.XmlEnumAttribute("ESE")]
            EquitySecurity,
            [System.Xml.Serialization.XmlEnumAttribute("FIX")]
            Fix,
            [System.Xml.Serialization.XmlEnumAttribute("FX")]
            ForeignExchange,
            [System.Xml.Serialization.XmlEnumAttribute("INV")]
            Invoicing,
            [System.Xml.Serialization.XmlEnumAttribute("IRD")]
            InterestRateDerivative,
            [System.Xml.Serialization.XmlEnumAttribute("LSD")]
            ListedDerivative,
            [System.Xml.Serialization.XmlEnumAttribute("STRATEGY")]
            Strategy,
            [System.Xml.Serialization.XmlEnumAttribute("MARGIN")]
            Margin,
            [System.Xml.Serialization.XmlEnumAttribute("CASHBALANCE")]
            CashBalance,
            [System.Xml.Serialization.XmlEnumAttribute("CASHPAYMENT")]
            CashPayment,
            [System.Xml.Serialization.XmlEnumAttribute("CASHINTEREST")]
            CashInterest,
        }

        public enum GroupProductEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("N/A")]
            NotAvailable,
            [System.Xml.Serialization.XmlEnumAttribute("ADM")]
            Administrative,
            [System.Xml.Serialization.XmlEnumAttribute("ASSET")]
            Asset,
            [ProductAttribute(IsTrading = true)]
            [System.Xml.Serialization.XmlEnumAttribute("COM")]
            Commodity,
            [System.Xml.Serialization.XmlEnumAttribute("FX")]
            [ProductAttribute(IsTrading = true)]
            ForeignExchange,
            [System.Xml.Serialization.XmlEnumAttribute("FUT")]
            [ProductAttribute(IsTrading = true)]
            ExchangeTradedDerivative,
            [System.Xml.Serialization.XmlEnumAttribute("MTM")]
            MarkToMarket,
            [System.Xml.Serialization.XmlEnumAttribute("OTC")]
            [ProductAttribute(IsTrading = true)]
            OverTheCounter,
            [System.Xml.Serialization.XmlEnumAttribute("RISK")]
            Risk,
            [System.Xml.Serialization.XmlEnumAttribute("SEC")]
            [ProductAttribute(IsTrading = true)]
            Security,
        }

        // EG 20170918 [23342] Add ESMA
        public enum SourceEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("EFS")]
            EFS,
            [System.Xml.Serialization.XmlEnumAttribute("EfsML")]
            [SourceAttribute(IsProduct = true)]
            EfsML,
            [System.Xml.Serialization.XmlEnumAttribute("ESMA")]
            ESMA,
            [System.Xml.Serialization.XmlEnumAttribute("ECC")]
            EuropeanCommodityClearing,
            [System.Xml.Serialization.XmlEnumAttribute("Exchange")]
            Exchange,
            [System.Xml.Serialization.XmlEnumAttribute("External")]
            External,
            [System.Xml.Serialization.XmlEnumAttribute("IANA")]
            IANA,
            [System.Xml.Serialization.XmlEnumAttribute("Internal")]
            Internal,
            [System.Xml.Serialization.XmlEnumAttribute("ISDA")]
            ISDA,
            [System.Xml.Serialization.XmlEnumAttribute("ISO")]
            ISO,
            [System.Xml.Serialization.XmlEnumAttribute("ISO15022")]
            ISO15022,
            [System.Xml.Serialization.XmlEnumAttribute("ISO20022")]
            ISO20022,
            [System.Xml.Serialization.XmlEnumAttribute("F&Oml")]
            [SourceAttribute(IsProduct = true)]
            FOml,
            [System.Xml.Serialization.XmlEnumAttribute("FIX")]
            [SourceAttribute(IsProduct = true)]
            FIX,
            [System.Xml.Serialization.XmlEnumAttribute("FixML")]
            FixML,
            [System.Xml.Serialization.XmlEnumAttribute("FpML")]
            [SourceAttribute(IsProduct = true)]
            FpML,
            [System.Xml.Serialization.XmlEnumAttribute("N/A")]
            [SourceAttribute(IsProduct = true)]
            NotAvailable,
            [System.Xml.Serialization.XmlEnumAttribute("OTCml")]
            [SourceAttribute(IsProduct = true)]
            OTCml,
        }

        public static bool IsProductTrading(GroupProductEnum pEnum)
        {
            bool isTrading = false;
            FieldInfo fld = pEnum.GetType().GetField(pEnum.ToString());
            if (null != fld)
            {
                object[] attributes = fld.GetCustomAttributes(typeof(ProductAttribute), true);
                if (0 != attributes.GetLength(0))
                    isTrading = ((ProductAttribute)attributes[0]).IsTrading;
            }
            return isTrading;
        }
        public static bool IsProductSource(SourceEnum pEnum)
        {
            bool isProduct = false;
            FieldInfo fld = pEnum.GetType().GetField(pEnum.ToString());
            if (null != fld)
            {
                object[] attributes = fld.GetCustomAttributes(typeof(SourceAttribute), true);
                if (0 != attributes.GetLength(0))
                    isProduct = ((SourceAttribute)attributes[0]).IsProduct;
            }
            return isProduct;
        }


        private static object EnumValue(Type pEnumType, string pValue)
        {
            object objFind = null;
            string @value = pValue.Trim();
            if (false == System.Enum.IsDefined(pEnumType, @value))
            {
                object obj = pEnumType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                if (null != obj)
                {
                    FieldInfo[] flds = obj.GetType().GetFields();
                    foreach (FieldInfo fld in flds)
                    {
                        object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                        if ((0 != attributes.GetLength(0)) && (@value == ((XmlEnumAttribute)attributes[0]).Name))
                            objFind = fld.GetValue(obj);
                    }
                }
            }
            return objFind;
        }
        // Relative to ProductEnum
        public static bool IsCommodity(string pGroup)
        {
            object objFind = EnumValue(typeof(GroupProductEnum), pGroup);
            return (null != objFind) && (GroupProductEnum.Commodity == (GroupProductEnum)objFind);
        }
        public static bool IsMarkToMarket(string pGroup)
        {
            object objFind = EnumValue(typeof(GroupProductEnum), pGroup);
            return (null != objFind) && (GroupProductEnum.MarkToMarket == (GroupProductEnum)objFind);
        }
        public static bool IsProductFX(string pGroup)
        {
            object objFind = EnumValue(typeof(GroupProductEnum), pGroup);
            return (null != objFind) && (GroupProductEnum.ForeignExchange == (GroupProductEnum)objFind);
        }
        // Relative to FamilyEnum
        public static bool IsCommoditySpot(string pFamily)
        {
            object objFind = EnumValue(typeof(FamilyEnum), pFamily);
            return (null != objFind) && (FamilyEnum.CommoditySpot == (FamilyEnum)objFind);
        }
        public bool IsListedDerivative(string pFamily)
        {
            object objFind = EnumValue(typeof(FamilyEnum), pFamily);
            return (null != objFind) && (FamilyEnum.ListedDerivative == (FamilyEnum)objFind);
        }
        public bool IsInterestRateDerivative(string pFamily)
        {
            object objFind = EnumValue(typeof(FamilyEnum), pFamily);
            return (null != objFind) && (FamilyEnum.InterestRateDerivative == (FamilyEnum)objFind);
        }
        public bool IsBondOption(string pFamily)
        {
            object objFind = EnumValue(typeof(FamilyEnum), pFamily);
            return (null != objFind) && (FamilyEnum.BondOption == (FamilyEnum)objFind);
        }
        public bool IsEquitySecurity(string pFamily)
        {
            object objFind = EnumValue(typeof(FamilyEnum), pFamily);
            return (null != objFind) && (FamilyEnum.EquitySecurity == (FamilyEnum)objFind);
        }
        public bool IsReturnSwap(string pFamily)
        {
            object objFind = EnumValue(typeof(FamilyEnum), pFamily);
            return (null != objFind) && (FamilyEnum.ReturnSwap == (FamilyEnum)objFind);
        }
        public bool IsForeignExchange(string pFamily)
        {
            object objFind = EnumValue(typeof(FamilyEnum), pFamily);
            return (null != objFind) && (FamilyEnum.ForeignExchange == (FamilyEnum)objFind);
        }
    }

    #region ProductAttribute Attribute
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProductAttribute : BooleanAttribute
    {
        #region Accessors
        public bool IsTrading
        {
            get { return Value; }
            set { Value = value; }
        }
        #endregion Accessors
        #region Constructors
        public ProductAttribute() : this(false) { }
        public ProductAttribute(bool pValue) : base (pValue)
        {
        }
        #endregion Constructors
    }
    #endregion ProductTrading Attribute
    #region SourceAttribute Attribute
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SourceAttribute : BooleanAttribute
    {
        #region Accessors
        public bool IsProduct
        {
            get { return Value; }
            set { Value = value; }
        }
        #endregion Accessors
        #region Constructors
        public SourceAttribute() : this(false) { }
        public SourceAttribute(bool pValue) : base (pValue)
        {
        }
        #endregion Constructors
    }
    #endregion SourceAttribute Attribute
    #region BooleanAttribute Attribute
    public abstract class BooleanAttribute : Attribute
    {
        #region Variables
        private bool m_Value;
        #endregion Variables
        #region Accessors
        protected bool Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
        #endregion Accessors
        #region Constructors
        public BooleanAttribute() : this(false) { }
        public BooleanAttribute(bool pValue)
        {
            m_Value = pValue;
        }
        #endregion Constructors
    }
    #endregion BooleanAttribute Attribute
}
