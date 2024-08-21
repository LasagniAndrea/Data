using System;
using System.Reflection;  


namespace EFS.SpheresIO
{
    /// <summary>
    /// Comparison result types.
    /// </summary>
    /// <remarks>
    /// Constraint: higher is the number higher is the error criticity.
    /// </remarks>
    /// <seealso cref="ToolDeRapprochement-CDCv1.0.1"/>
    public enum MatchStatus
    {
        #region amounts results

        /// <summary>
        /// Raised when at least one amount does not match among two elements having same comparison key
        /// (UNMATCH_PRMAMT | UNMATCH_VRMRGNAMT | UNMATCH_LOVAMT | UNMATCH_RMGAMT | UNMATCH_TAXCOMBRKAMT | 
        /// UNMATCH_PMTAMT | UNMATCH_COLLAMT | UNMATCH_UMGAMT | UNMATCH_TAXCOMAMT | UNMATCH_CALLAMT | UNMATCH_RPTAMT )
        /// </summary>
        UNMATCH_AMT = 8188,
        /// <summary>
        /// Raised when amounts on result do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_RPTAMT = 4096,
        /// <summary>
        /// Raised when amounts on AP do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_CALLAMT = 2048,
        /// <summary>
        /// Raised when amounts on premium do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_PRMAMT = 1024,
        /// <summary>
        /// Raised when amounts on variation margin plus cash settlement do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_VRMRGNAMT = 512,
        /// <summary>
        /// Raised when amounts on liquidative (options) value do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_LOVAMT = 256,
        /// <summary>
        /// Raised when amounts on realized margin do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_RMGAMT = 128,
        /// <summary>
        /// Raised when amounts on other party payments (including VAT and other taxes) do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_TAXCOMBRKAMT = 64,
        /// <summary>
        /// Raised when amounts on payments do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_PMTAMT = 32,
        /// <summary>
        /// Raised when amounts on initial margin (aka deposit) do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_COLLAMT = 16,
        /// <summary>
        /// Raised when amounts on unrealized margin do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_UMGAMT = 8,
        /// <summary>
        /// Raised when amounts on commission do not match among two elements having same comparison key
        /// </summary>
        UNMATCH_TAXCOMAMT = 4,

        #endregion amounts results

        #region Positions/Trades results

        /// <summary>
        /// When two elements are equals but they do not have the same quantity (UNMATCH_LONGQTY | UNMATCH_SHORTQTY )
        /// </summary>
        UNMATCH_QTY = 3,
        /// <summary>
        /// When two elements are equals but they do not have the same long quantity
        /// </summary>
        UNMATCH_LONGQTY = 2,
        /// <summary>
        /// When two elements are equals but they do not have the same short quantity
        /// </summary>
        UNMATCH_SHORTQTY = 1,

        #endregion Positions/Trades results

        #region Common results

        ///<summary>
        /// Raised in any case we have something different. 
        /// </summary>
        /// <remarks>
        /// Must be the bit-wise OR of all the defined elements, but MATCH : UNMATCH_MISSING + 1
        /// </remarks>
        UNMATCH = 8193,
        /// <summary>
        /// Raised when an element does not found a related element into the other set, its own comparison key does not match.
        /// </summary>
        /// <remarks>
        /// Must be greater than any other enumeration element (it must use the greater power of 2), but UNMATCH.
        /// </remarks>
        UNMATCH_MISSING = 8192,
        /// <summary>
        /// When two elements are equal
        /// </summary>
        MATCH = 0,

        #endregion Common results


    }

    /// <summary>
    /// Compare Options
    /// </summary>
    /// <remarks>
    /// Add other elements for each new comparison interface
    /// </remarks>
    public enum CompareOptions
    {
        Unknown = 0,
        Spheres = 1,
        Eurosys = 2,
        FIXml = 16,
        FpML = 32,
        [CompareOptionsAttribute(ShortName = "TRD")]
        Trades = 128,
        [CompareOptionsAttribute(ShortName = "POS")]
        Positions = 256,
        [CompareOptionsAttribute(ShortName = "TIP")]
        TradesInPosition = 512,
        [CompareOptionsAttribute(ShortName = "AMT")]
        CashFlows = 1024,
        [CompareOptionsAttribute(ShortName = "AMI")]
        CashFlowsInstr = 2048,
        [CompareOptionsAttribute(ShortName = "MIC")]
        Miscellanous = 4096,
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CompareOptionsAttribute : Attribute
    {
        /// <summary>
        /// Nom court de substitution d'une valeur de l'enum CompareOptions
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Convertie une string en CompareOptions  
        /// <para>Les valeurs reconnues sont les enums ou leurs noms courts</para>
        /// </summary>
        /// <param name="pShortName"></param>
        /// <returns></returns>
        public static Nullable<CompareOptions> Parse(string pValue, bool pIgnoreCase)
        {
            Nullable<CompareOptions> ret = null;

            FieldInfo[] Flds = typeof(CompareOptions).GetFields();
            if (null != Flds && Flds.Length > 0)
            {
                for (int i = 0; i < Flds.Length; i++)
                {
                    CompareOptionsAttribute[] attributes = (CompareOptionsAttribute[])Flds[i].GetCustomAttributes(typeof(CompareOptionsAttribute), false);
                    if ((null != attributes) && (attributes.Length > 0))
                    {
                        string shortName = attributes[0].ShortName;
                        string value = pValue;
                        if (pIgnoreCase)
                        {
                            shortName = shortName.ToUpper();
                            value = pValue.ToUpper();
                        }
                        //
                        if (shortName.CompareTo(value) == 0)
                        {
                            ret = (CompareOptions)Enum.Parse(typeof(CompareOptions), Flds[i].Name);
                            break;
                        }
                    }
                }
                //
                if (null == ret)
                {
                    if (pIgnoreCase)
                    {
                        for (int i = 0; i < Flds.Length; i++)
                        {
                            if (Flds[i].Name.ToUpper() == pValue.ToUpper())
                            {
                                ret = (CompareOptions)Enum.Parse(typeof(CompareOptions), Flds[i].Name);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (Enum.IsDefined(typeof(CompareOptions), pValue))
                            ret = (CompareOptions)Enum.Parse(typeof(CompareOptions), pValue);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// <para>
        /// Retourne le nom court associé à {pCompareOptions} 
        /// </para>
        /// <para>
        /// Retourne {pCompareOptions}.toString() lorsque le nom court n'existe pas
        /// </para>
        /// </summary>
        /// <param name="pCompareOptions"></param>
        /// <returns></returns>
        public static string ConvertToString(CompareOptions pCompareOptions)
        {
            string ret = pCompareOptions.ToString();
            //
            FieldInfo[] Flds = typeof(CompareOptions).GetFields();
            if (null != Flds)
            {
                for (int i = 0; i < Flds.Length; i++)
                {
                    if (Flds[i].Name == pCompareOptions.ToString())
                    {
                        CompareOptionsAttribute[] attributes = (CompareOptionsAttribute[])Flds[i].GetCustomAttributes(typeof(CompareOptionsAttribute), false);
                        if (null != attributes && attributes.Length > 0)
                            ret = attributes[0].ShortName;
                        else
                            ret = pCompareOptions.ToString();
                        break;
                    }
                }
            }
            //
            return ret;
        }
    }

}