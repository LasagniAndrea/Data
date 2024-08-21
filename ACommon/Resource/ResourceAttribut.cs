using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.ACommon
{
    /// <summary>
    /// Attribut qui permet d'attibuer une resource 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field , AllowMultiple = false)]
    public class ResourceAttribut : Attribute
    {
        /// <summary>
        /// Nom de la resource
        /// </summary>
        public string Resource
        {
            get;
            set;
        }
    }
}
