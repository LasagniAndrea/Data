using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.Common;

namespace EFS.SpheresIO.MarketData
{

    /// <summary>
    /// Représente un asset ETD dont le cours est requis
    /// </summary>
    /// PM 20180219 [23824] New
    internal class MarketAssetETDToImport : AssetETDIdent
    {
        /// <summary>
        /// Mnémonique de l'asset sous Spheres®
        /// </summary>
        public string AssetIdentifier { get; set; }

        /// <summary>
        /// Category de l'asset sous-jacent
        /// </summary>
        public string UnderlyingAssetCategory { get; set; }

        /// <summary>
        /// Id interne de l'asset sous-jacent
        /// </summary>
        public int IdAssetUnl { get; set; }

        /// <summary>
        /// Id interne de l'asset sous-jacent Future (Options sur future)
        /// </summary>
        public int IdAssetUnlFuture { get; set; }

    }
}
