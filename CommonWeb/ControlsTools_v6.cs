#region Using Directives
using EFS.ACommon;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{

    /// <summary>
    /// Utilisée par le TypeAhead via WSDataService 
    /// </summary>
    public class DataTypeAhead
    {
        public string Id { get; set; }
        public string Identifier { get; set; }
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }
    }

    public sealed partial class ControlsTools
    {
        public static List<T> GetControls<T>(ControlCollection Controls)
        where T : Control
        {
            List<T> results = new List<T>();
            foreach (Control c in Controls)
            {
                if (c is T t) results.Add(t);
                if (c.HasControls()) results.AddRange(GetControls<T>(c.Controls));
            }
            return results;
        }

        /// <summary>
        /// Finds a Control recursively. Note finds the first match and exists
        /// </summary>
        /// <param name="pRoot">Racine de la recherche</param>
        /// <param name="pId">Id</param>
        /// <returns></returns>
        public static Control FindControlRecursive(Control pRoot, string pId)
        {
            if (pRoot.ID == pId)
                return pRoot;

            foreach (Control control in pRoot.Controls)
            {
                Control foundControl = FindControlRecursive(control, pId);
                if (foundControl != null)
                    return foundControl;
            }
            return null;
        }

        #region GetValidators
        public static List<Validator> GetValidators(bool pIsMandatory, string pMsgErrRequiredField)
        {
            return GetValidators(pIsMandatory, pMsgErrRequiredField, null, null, null);
        }
        public static List<Validator> GetValidators(bool pIsMandatory,
            string pMsgErrRequiredField, string pRegularExpression, string pMsgError, string pDataType)
        {
            List<Validator> lstValidators = new List<Validator>();

            EFSRegex.TypeRegex regexValue = EFSRegex.TypeRegex.None;
            if (StrFunc.IsFilled(pRegularExpression) && System.Enum.IsDefined(typeof(EFSRegex.TypeRegex), pRegularExpression))
                regexValue = (EFSRegex.TypeRegex)System.Enum.Parse(typeof(EFSRegex.TypeRegex), pRegularExpression);

            #region RequireField
            if (pIsMandatory)
                lstValidators.Add(new Validator(pMsgErrRequiredField, true, false));
            #endregion RequireField

            #region Regular Expression
            if (regexValue != EFSRegex.TypeRegex.None)
                lstValidators.Add(new Validator(regexValue, pMsgError, true, false));
            else if (StrFunc.IsFilled(pRegularExpression))
            {
                lstValidators.Add(new Validator(pMsgError, true, pRegularExpression, false));
            }
            else if (StrFunc.IsFilled(pDataType))
            {
                #region ValidationDataType
                ValidationDataType validationType = ValidationDataType.String;
                if (TypeData.IsTypeDate(pDataType))
                    validationType = ValidationDataType.Date;
                else if (TypeData.IsTypeDec(pDataType))
                    validationType = ValidationDataType.Double;
                else if (TypeData.IsTypeInt(pDataType))
                    validationType = ValidationDataType.Integer;
                else if (TypeData.IsTypeString(pDataType))
                    validationType = ValidationDataType.String;
                lstValidators.Add(new Validator(validationType, pMsgError, true, false));
                #endregion ValidationDataType
            }
            #endregion Regular Expression

            #region CustomValidator
            if (StrFunc.IsFilled(pDataType) && TypeData.IsTypeDate(pDataType))
                lstValidators.Add(Validator.GetValidatorDateRange(Ressource.GetString("Msg_InvalidDate"), "Date"));
            #endregion CustomValidator

            return lstValidators;
        }
        #endregion GetValidators

    }
}
