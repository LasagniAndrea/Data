using System.IO;

namespace EFS.ACommon
{
    /// <summary>
    /// Class d'encapsulation de l'utilisation des class de System.IO
    /// </summary>
    public static class SystemIOTools
    {
        #region Encapsulation des class de base
        /// <summary>
        ///  Création du folder s'il n'existe pas 
        /// </summary>
        /// <param name="pPath"></param>
        public static void CreateDirectory(string pPath)
        {
            if (false == Directory.Exists(pPath))
            {
                Directory.CreateDirectory(pPath);
            }
        }
        #endregion Encapsulation des class de base

        #region Outils supplémentaires
        /// <summary>
        /// Ajoute le suffixe {pSuffixe} à un nom de fichier.
        /// <para>Le nom de fichier peut contenir une extension (ex filename.txt)</para>
        /// <para>Le nom de fichier peut contenir un folder (ex c:\filename.txt)</para>
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pSuffixe"></param>
        /// <returns></returns>
        public static string AddFileNameSuffixe(string pFileName, string pSuffixe)
        {
            string ret = pFileName;
            if ((null != pFileName) && (null != pSuffixe))
            {
                int dotPos = pFileName.LastIndexOf('.');
                if (dotPos > -1)
                {
                    ret = pFileName.Remove(dotPos) + pSuffixe + pFileName.Substring(dotPos);
                }
                else
                {
                    ret += pSuffixe;
                }
            }
            return ret;
        }
        #endregion Outils supplémentaires
    }
}
