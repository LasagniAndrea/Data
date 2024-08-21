using System;

namespace EFS.Common.Web
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20210202 [XXXXX] Add
    public interface IAutocompleteItem
    {
        /// <summary>
        /// Ontient la propriété utilisée pour proposer les items dans l'ordre
        /// </summary>
        /// <returns></returns>
        string GetPropertyForOrder { get; }
    }

    /// <summary>
    ///  Représente un item autocomplete (doit être utilisé lorsque les données retour d'un autocomplete sont complexes)
    /// </summary>
    /// FI 20210202 [XXXXX] Add
    public class AutoCompleteDataItem : IAutocompleteItem, IEquatable<AutoCompleteDataItem>
    {
        /// <summary>
        /// Id non significatif 
        /// </summary>
        public string id;
        /// <summary>
        /// Identifiant (généralement unique) 
        /// </summary>
        public string identifier;
        /// <summary>
        /// Description
        /// </summary>
        public string description;

        /// <summary>
        /// Ontient la propriété utilisée pour proposer les items dans l'ordre
        /// </summary>
        /// <returns></returns>
        public string GetPropertyForOrder
        {
            get
            {
                return identifier;
            }
        }

        public bool Equals(AutoCompleteDataItem other)
        {
            bool ret = false;
            if (null != other)
            {
                if (
                    (((other.id != null) && (this.id != null) && (other.id.Equals(this.id))) || (other.id is null && this.id is null)) ||
                    (((other.identifier != null) && (this.identifier != null) && (other.identifier.Equals(this.identifier))) || (other.identifier is null && this.identifier is null)) ||
                    (((other.description != null) && (this.description != null) && (other.description.Equals(this.description))) || (other.description is null && this.description is null))
                    )
                {
                    ret = true;
                }
            }
            return ret;

        }
    }
}
