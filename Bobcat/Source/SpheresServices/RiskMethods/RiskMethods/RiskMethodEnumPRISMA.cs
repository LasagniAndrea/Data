using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Enum pour la donnée CurrencyTypeFlag de Prisma
    /// </summary>
    [DataContract(Name = "PrismaCurrencyTypeFlagEnum")]
    public enum PrismaCurrencyTypeFlagEnum
    {
        /// <summary>
        /// N/A
        /// </summary>
        [XmlEnum("")]
        [EnumMember]
        NA = 0,
        /// <summary>
        /// Clearing currency
        /// </summary>
        [XmlEnum("C")]
        [EnumMember(Value = "C")]
        ClearingCurrency = 1,
        /// <summary>
        /// Product currency
        /// </summary>
        [XmlEnum("P")]
        [EnumMember(Value = "P")]
        ProductCurrency = 2,
    }

    /// <summary>
    /// Enum pour la donnée HistoricalStressed de Prisma
    /// </summary>
    [DataContract(Name = "PrismaHistoricalStressedEnum")]
    public enum PrismaHistoricalStressedEnum
    {
        /// <summary>
        /// N/A
        /// </summary>
        [XmlEnum("")]
        [EnumMember]
        NA = 0,
        /// <summary>
        /// Historical scenarios
        /// </summary>
        [XmlEnum("H")]
        [EnumMember(Value = "H")]
        Historical = 1,
        /// <summary>
        /// Historical Without Sampling scenarios
        /// </summary>
        /// PM 20230614 [26180][WI656] Prisma 12.1 : Ajout
        [XmlEnum("I")]
        [EnumMember(Value = "I")]
        HistoricalWithoutSampling = 2,
        /// <summary>
        /// Filtered historical scenarios
        /// </summary>
        [XmlEnum("F")]
        [EnumMember(Value = "F")]
        Filtered = 3,
        /// <summary>
        /// Filtered historical Without Sampling scenarios
        /// </summary>
        /// PM 20230614 [26180][WI656] Prisma 12.1 : Ajout
        [XmlEnum("G")]
        [EnumMember(Value = "G")]
        FilteredWithoutSampling = 4,
        /// <summary>
        /// Stressed period scenarios
        /// </summary>
        [XmlEnum("S")]
        [EnumMember(Value = "S")]
        Stressed = 5,
        /// <summary>
        /// Stressed period Without Sampling scenarios
        /// </summary>
        /// PM 20230614 [26180][WI656] Prisma 12.1 : Ajout
        [XmlEnum("T")]
        [EnumMember(Value = "T")]
        StressedWithoutSampling = 6,
        /// <summary>
        /// Event risk scenarios
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout Event
        [XmlEnum("E")]
        [EnumMember(Value = "E")]
        Event = 7,
    }

    /// <summary>
    /// Enum pour la donnée MarginStyle de Prisma
    /// </summary>
    [DataContract(Name = "PrismaMarginStyleEnum")]
    public enum PrismaMarginStyleEnum
    {
        /// <summary>
        /// N/A
        /// </summary>
        [XmlEnum("")]
        [EnumMember]
        NA = 0,
        /// <summary>
        /// Futures style
        /// </summary>
        [XmlEnum("F")]
        [EnumMember(Value = "F")]
        FuturesStyle = 1,
        /// <summary>
        /// Filtered historical scenarios
        /// </summary>
        [XmlEnum("T")]
        [EnumMember(Value = "T")]
        Traditional = 2,
    }

    /// <summary>
    /// Enum pour la donnée AggregationMethod de Prisma
    /// </summary>
    [DataContract(Name = "PrismaAggregationMethod")]
    public enum PrismaAggregationMethod
    {
        /// <summary>
        /// N/A
        /// </summary>
        [XmlEnum("")]
        [EnumMember]
        NA = 0,
        /// <summary>
        /// Average (Moyenne)
        /// </summary>
        [XmlEnum("Avg")]
        [EnumMember(Value = "Avg")]
        Avg = 1,
        /// <summary>
        /// Maximum
        /// </summary>
        [XmlEnum("Max")]
        [EnumMember(Value = "Max")]
        Max = 2,
        /// <summary>
        /// Median (Médiane)
        /// </summary>
        [XmlEnum("Med")]
        [EnumMember(Value = "Med")]
        Med = 3,
        /// <summary>
        /// Minimum
        /// </summary>
        [XmlEnum("Min")]
        [EnumMember(Value = "Min")]
        Min = 4,
        /// <summary>
        /// Sum (Somme)
        /// </summary>
        [XmlEnum("Sum")]
        [EnumMember(Value = "Sum")]
        Sum = 5,
    }
}
