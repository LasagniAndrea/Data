using System;
using System.Collections.Generic;
using System.Linq;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public static class MimeMappingTools
    {
        /// <summary>
        /// Catégories de MimeType (telle que déclarées dans IIS)
        /// </summary>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        [FlagsAttribute]
        public enum TypeCategoryEnum
        {
            all = application | audio | image | message | text | video | other,
            defaultUpload = application | image | text | video,
            application = 1,
            audio = 2,
            image = 4,
            message = 8,
            text = 16,
            video = 32,
            other = 64,
        }

        // Dictionnaire de base de tous les mimeType
        private static readonly MimeMappingData _mimeMappingData = new MimeMappingData();

        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        private abstract class MimeMappingDataBase
        {
            private readonly List<MimeCategory> _lstMimeCategory = new List<MimeCategory>();
            private bool _isInitialized = false;

            /// <summary>
            /// Chargement du dictionnaire si non initialisé
            /// </summary>
            /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
            public void EnsureMapping()
            {
                // L'initialisation n'est opérée qu'une fois
                lock (this)
                {
                    if (!_isInitialized)
                    {
                        _lstMimeCategory.Add(new MimeCategoryApplication());
                        _lstMimeCategory.Add(new MimeCategoryAudio());
                        _lstMimeCategory.Add(new MimeCategoryImage());
                        _lstMimeCategory.Add(new MimeCategoryMessage());
                        _lstMimeCategory.Add(new MimeCategoryOther());
                        _lstMimeCategory.Add(new MimeCategoryText());
                        _lstMimeCategory.Add(new MimeCategoryVideo());
                        _isInitialized = true;
                    }
                }
            }

            /// <summary>
            /// Procédure principale de vérification d'un fichier à uploader
            /// </summary>
            /// <param name="pUploadFileInfo">Caractéristiques du fichier à uploader et paramètres (webCustom.config)</param>
            /// <returns></returns>
            /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
            /// EG 20240523 [WI940][26663] Security : Mime Type enhancement (IsEndOfVerify instead of IsSignatureVerified)
            public FileTypeVerifyResult FileTypeVerify(UploadFileInfo pUploadFileInfo)
            {
                EnsureMapping();

                // On filtre sur les catégories autorisées
                List<MimeCategory> categories = (from item in _lstMimeCategory
                                                 where pUploadFileInfo.Category.HasFlag(item.Category)
                                                 select item).ToList();

                FileTypeVerifyResult result = null;
                foreach (MimeCategory item in categories)
                {
                    // Vérification du fichier dans la liste des type de fichiers de la catégorie
                    result = item.FileTypeVerify(pUploadFileInfo);
                    // La signature est valide on sort de la boucle
                    if (result.IsEndOfVerify)
                        break;
                }
                return result;
            }
        }

        /// <summary>
        /// Classe de base de chargement des mimeType et de recherche d'un mimeType pour un nom de fichier donné (sur la base de son extension)
        /// </summary>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        private sealed class MimeMappingData : MimeMappingDataBase{}

        /// <summary>
        ///  Transformation d'une liste de catégories en Enum de catégories
        /// </summary>
        /// <param name="pLstMimeTypeCategory">Liste (chaine) des catégories</param>
        /// <returns></returns>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        public static MimeMappingTools.TypeCategoryEnum StringMimeTypeToEnum(string pLstMimeTypeCategory)
        {
            MimeMappingTools.TypeCategoryEnum ret = default;
            foreach (string category in pLstMimeTypeCategory.Split(",".ToCharArray()))
            {
                if (Enum.IsDefined(typeof(MimeMappingTools.TypeCategoryEnum), category.Trim()))
                    ret |= (MimeMappingTools.TypeCategoryEnum)Enum.Parse(typeof(MimeMappingTools.TypeCategoryEnum), category.Trim(), true);
            }
            return ret;

        }

        /// <summary>
        /// Méthode principale de vérification appelée par fileUpload.aspx
        /// </summary>
        /// <param name="pUploadFileInfo">Caractéristiques du fichier à uploader</param>
        /// <returns></returns>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        public static FileTypeVerifyResult FileTypeVerify(UploadFileInfo pUploadFileInfo)
        {
            return _mimeMappingData.FileTypeVerify(pUploadFileInfo);
        }

    }
}
