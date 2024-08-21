using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.ACommon
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20190718 [XXXXX] Add
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class UnderlyingAssetAttribute : Attribute
    {
        public UnderlyingAssetAttribute(Cst.UnderlyingAsset pUnderlyingAsset)
        {
            UnderlyingAsset = pUnderlyingAsset;
        }

        /// <summary>
        /// type d'asset
        /// </summary>
        public Cst.UnderlyingAsset UnderlyingAsset
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20220601 [XXXXX] Add
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class RegExAttribute : Attribute
    {
        /// <summary>
        /// Représente l'expression de la regular expression
        /// </summary>
        public String RegExPattern
        {
            get;
            set;
        }
        /// <summary>
        /// Nom de baptême de la RegEx (doit être renseignée si plusieurs regex sont définies)
        /// </summary>
        public String RegExName
        {
            get;
            set;
        }
    }
}
