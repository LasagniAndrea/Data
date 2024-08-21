using System;
using System.Collections.Generic;
using System.Linq;

namespace EFS.Common.Web
{
    public class AutocompleteTools
    {
        /// <summary>
        ///  Retourne La liste des valeurs qui matchent avec la données saisie dans un ordre pertinent
        /// </summary>
        /// <param name="pLstDataAutocomplete">Représente les données autocomplete qui matchent avec {pRequest}</param>
        /// <param name="pRequest">Représente la données saisie par l'utilsateur dans un contrôle autocompete</param>
        /// <returns></returns>
        public static IEnumerable<string> OrderAutocompletata(IEnumerable<string> pLstDataAutocomplete, string pRequest)
        {
            if (null == pLstDataAutocomplete)
                throw new ArgumentNullException("pLstDataAutocomplete");

            // FI 20200107 [XXXXX] priorité d'affichage  
            // 1/ enregistrements exactement identiques à request, 
            // 2/ enregistrements exactement identiques à request (case insensitive) 
            // 3/ enregistrements qui commencent par request (case insensitive). ces enregistrements sont triés par ordre alphabétique
            // 4/ enregistrements qui contiennent request (case insensitive) . ces enregistrements sont triés par ordre alphabétique
            // EG 20210826 [XXXXX] Decode de la chaine
            string valueToSearch = pRequest.Replace("&#039;","'").Replace("\\\\", "\\");
            IEnumerable<string> ret =
                    ((from item in pLstDataAutocomplete.Where(x => x == valueToSearch)
                      select item).Concat(
                      from item in pLstDataAutocomplete.Where(x => x.ToUpper() == valueToSearch.ToUpper())
                      select item).Concat(
                      (from item in pLstDataAutocomplete.Where(x => x.ToUpper().StartsWith(valueToSearch.ToUpper()))
                       select item).OrderBy(x => x.ToString())).Concat(
                      (from item in pLstDataAutocomplete.Where(x => x.ToUpper().Contains(valueToSearch.ToUpper()))
                       select item).OrderBy(x => x.ToString()))).Distinct();

            return ret;
        }

        /// <summary>
        ///  Retourne La liste des valeurs qui matchent avec la données saisie dans un ordre pertient
        /// </summary>
        /// <param name="pLstDataAutocomplete">Représente les données autocomplete qui matchent avec {pRequest}</param>
        /// <param name="pRequest">Représente la données saisie par l'utilsateur dans un contrôle autocompete</param>
        /// <returns></returns>
        /// <typeparam name="T">donnée complexe</typeparam>
        /// <param name="pLstDataAutocomplete"></param>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        /// FI 20210202 [XXXXX] Add
        public static IEnumerable<T> OrderAutocompletata<T>(IEnumerable<T> pLstDataAutocomplete, string pRequest) where T : IAutocompleteItem
        {

            if (null == pLstDataAutocomplete)
                throw new ArgumentNullException("pLstDataAutocomplete");

            // FI 20200107 [XXXXX] priorité d'affichage  
            // 1/ enregistrements exactement identiques à request, 
            // 2/ enregistrements exactement identiques à request (case insensitive) 
            // 3/ enregistrements qui commencent par request (case insensitive). ces enregistrements sont triés par ordre alphabétique
            // 4/ enregistrements qui contiennent request (case insensitive) . ces enregistrements sont triés par ordre alphabétique
            IEnumerable<T> ret =
                ((from item in pLstDataAutocomplete.Where(x => x.GetPropertyForOrder == pRequest)
                  select item).Concat(
                  from item in pLstDataAutocomplete.Where(x => x.GetPropertyForOrder.ToUpper() == pRequest.ToUpper())
                  select item).Concat(
                  (from item in pLstDataAutocomplete.Where(x => x.GetPropertyForOrder.ToUpper().StartsWith(pRequest.ToUpper()))
                   select item).OrderBy(x => x.ToString())).Concat(
                  (from item in pLstDataAutocomplete.Where(x => x.GetPropertyForOrder.ToUpper().Contains(pRequest.ToUpper()))
                   select item).OrderBy(x => x.ToString()))).Distinct();


            return ret;
        }
    }
}
