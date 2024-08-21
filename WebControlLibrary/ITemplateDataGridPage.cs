using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.Common.Web;



namespace EFS.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20200602 [25370]  Add
    public interface ITemplateDataGridPage
    {
        /// <summary>
        /// Obtient les CutomObjects de la page
        /// </summary>
        CustomObjects CustomObjects { get; }

        /// <summary>
        /// Obtient l'identifiant du contrôle si publication de la page à partir d'un contrôle généré depuis un CustomObjects
        /// </summary>
        String ClientIdCustomObjectChanged { get; }

        /// <summary>
        /// Retourne true si publication de la page suite à désactivation des critères optionnels  
        /// </summary>
        /// FI 20200602 [25370] Refactoring
        bool IsOptionalFilterDisabled { get; }


        /// <summary>
        /// Retourne la position du critère optionnel désactivé si publication de la page suite à désactivation d'un critère optionnel
        /// <para>Retourne -1 dans les autres cas</para>
        /// </summary>
        /// FI 20200602 [25370] Refactoring
        int PositionFilterDisabled { get; }

        /// <summary>
        /// 
        /// </summary>
        Boolean IsNoFailure { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEx"></param>
        void WriteLogException(Exception pEx);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileType"></param>
        /// <param name="pFileName"></param>
        /// <param name="pPath"></param>
        void ResponseWriteFile(string pFileType, string pFileName, string pPath);
    }

}
