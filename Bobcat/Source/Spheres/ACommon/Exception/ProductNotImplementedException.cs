using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace EFS.ACommon
{


    /// <summary>
    /// Exception lorsque le produit n'est pas géré
    /// </summary>
    // EG 20180425 Analyse du code Correction [CA2237]
    [Serializable]
    public class ProductNotImplementedException : NotImplementedException
    {
        private readonly string _productName;

        public ProductNotImplementedException()
            : base()
        {
        }

        public ProductNotImplementedException(string productName, string pMsg)
            : base(pMsg)
        {
            _productName = productName;
        }

        // EG 20180425 Analyse du code Correction [CA2240]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("productName", _productName);
            base.GetObjectData(info, context);
        }

    }
}
