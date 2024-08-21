using EFS.ACommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{
    /// <summary>
    /// Caractéristiques du fichier à uploader
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public class UploadFileInfo
    {
        /// <summary>
        ///  Contenu du fichier 
        /// </summary>
        public Stream Stream { get; set; }
        /// <summary>
        /// Mime type du fichier (sur la base de son extension)
        /// </summary>
        public string MimeType { get; set; }
        /// <summary>
        /// Nom du fichier
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///  Longueur du fichier
        /// </summary>
        public int ContentLength { get; set; }
        /// <summary>
        ///  Extension du fichier
        /// </summary>
        public string Extension => Path.GetExtension(Name);
        /// <summary>
        /// Booléen indiquant upload d'un logo d'acteur
        /// </summary>
        public bool Logo { get; set; }
        /// <summary>
        /// Catégories autorisées
        /// </summary>
        public MimeMappingTools.TypeCategoryEnum Category;
        /// <summary>
        /// Liste des fichiers (Extension) non autorisés à être uploadé
        /// => Lecture de la clé "UploadFile_ExtensionFilesExcluded" dans le fichier de configuration 
        ///    ou  de la clé "UploadFile_LogoExtensionFilesExcluded" dans le fichier de configuration 
        /// => les éléments de cette liste correspondent à la propriété "Name" de la classe MimeFile
        /// </summary>
        public List<string> mimeFileNameExcluded = new List<string>();
        /// <summary>
        /// Liste des fichiers (Extension) autorisés à être uploadé
        /// => Lecture de la clé "UploadFile_ExtensionFilesAuthorized" dans le fichier de configuration 
        ///    ou  de la clé "UploadFile_LogoExtensionFilesAuthorized" dans le fichier de configuration 
        /// => les éléments de cette liste correspondent à la propriété "Name" de la classe MimeFile
        /// </summary>
        public List<string> mimeFileNameAuthorized = new List<string>();
        /// <summary>
        /// Il existe des exclusions sur les types de fichiers
        /// </summary>
        public bool IsMimeFileNameExcluded => (0 < mimeFileNameExcluded.Count);
        /// <summary>
        /// Il existe des restrictions (autorisation sur les types de fichiers
        /// </summary>
        public bool IsMimeFileNameAuthorized => (0 < mimeFileNameAuthorized.Count);
        /// <summary>
        /// Chargement des paramètres de l'upload présent dans le webCustom.config
        /// </summary>
        public void SetParameters()
        {
            // Lecture des paramètres (LOGO ou ATTACHEDDOC)
            Category = Logo ? MimeMappingTools.TypeCategoryEnum.image:SessionTools.UploadFile_CategoryAuthorized;

            string prefix = Logo ? "Logo" : "";

            // Typde fichiers exclus
            string _excluded = SessionTools.GetUploadFile_ExtensionFiles(prefix, "Excluded");
            if (StrFunc.IsFilled(_excluded))
                mimeFileNameExcluded = _excluded.Split(',').Select(mf => mf.Trim().ToUpper()).ToList();

            // Typde fichiers autorisés
            string _authorized = SessionTools.GetUploadFile_ExtensionFiles(prefix, "Authorized");
            if (StrFunc.IsFilled(_authorized))
                mimeFileNameAuthorized = _authorized.Split(',').Select(mf => mf.Trim().ToUpper()).ToList();
        }
    }

    /// <summary>
    /// Classe contenant le résultat d'une vérication d'un fichier à uploader
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public class FileTypeVerifyResult
    {
        /// <summary>
        /// Nom du MimeFile
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Descriptif du MimeFile
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// La signature est-elle valide?
        /// </summary>
        public bool IsSignatureVerified { get; set; }
        /// <summary>
        /// Aucune signature pour ce type de fichier ?
        /// </summary>
        public bool IsWithoutSignature { get; set; }
        /// <summary>
        /// La taille est-elle valide?
        /// </summary>
        public bool IsSizeVerified { get; set; }
        /// <summary>
        /// Le mimeType est-il valide?
        /// </summary>
        public bool IsMimeTypeVerified { get; set; }
        /// <summary>
        /// Ressource du message d'erreur
        /// </summary>
        public string ErrMessage { get; set; }
        /// <summary>
        /// Caractéristiques du fichier à uploader
        /// </summary>
        public UploadFileInfo UploadFileInfo { get; set; }

        /// <summary>
        /// Le fichier est VALIDE : Signature vérifiée + MimeType vérifié + Taille vérifiée + Type autorisé
        /// </summary>
        public bool IsVerified => IsSignatureVerified && IsSizeVerified & IsMimeTypeVerified;
        /// <summary>
        /// Le fichier est VALIDE : Signature vérifiée + MimeType vérifié + Taille vérifiée + Type autorisé
        /// </summary>
        public bool IsEndOfVerify => IsSignatureVerified && IsMimeTypeVerified;
    }

    /// <summary>
    /// ----------------------------------------------
    /// Classe abstraite d'une signature de fichier
    /// ----------------------------------------------
    /// - Nom et description du fichier
    /// - Category Mime
    /// - Type de mime
    /// - Extensions
    /// - Signatures en Hexa
    /// </summary>
    /// <seealso cref="https://www.wikiwand.com/en/List_of_file_signatures"/>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public abstract class MimeFile
    {
        /// <summary>
        /// Identifiant du type de fichier
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description du type de fichier
        /// </summary>
        protected string Description { get; set; }
        /// <summary>
        /// Catégorie d'appartenance
        /// </summary>
        protected MimeMappingTools.TypeCategoryEnum Category { get; set; }
        /// <summary>
        /// Liste des mime type possibles pour ce type de fichier
        /// </summary>
        public List<string> Type { get; }
            = new List<string>();
        /// <summary>
        /// Liste des extensions possibles pour ce type de fichier
        /// </summary>
        public List<string> Extensions { get; }
            = new List<string>();
        /// <summary>
        /// Offset de démarrage du contrôle de signature
        /// </summary>
        public int OffsetSignatures { get; protected set; }
        /// <summary>
        /// Liste des signatures pour ce type de fichier
        /// </summary>
        public List<byte[]> Signatures { get; }
            = new List<byte[]>();
        /// <summary>
        /// Longueur de la plus grand signature
        /// </summary>
        public int SignatureLength => (0 < Signatures.Count)?Signatures.Max(m => m.Length):0;
        /// <summary>
        /// Construction du MimeType complet (IIS) = Categorie + "/" + MimeType du fichier
        /// </summary>
        public string PrefixMimeType => (Category != MimeMappingTools.TypeCategoryEnum.other) ? $"{Category}/" : string.Empty;
        /// <summary>
        /// Alimentation de la liste des extensions
        /// </summary>
        protected MimeFile AddExtensions(params string[] extensions)
        {
            Extensions.AddRange(extensions);
            return this;
        }
        /// <summary>
        /// Alimentation de la liste des extensions
        /// </summary>
        protected MimeFile AddMimeType(params string[] type)
        {
            Type.AddRange(type);
            return this;
        }
        /// <summary>
        /// Alimentation de la liste des signatures
        /// </summary>
        protected MimeFile AddSignatures(params byte[][] bytes)
        {
            Signatures.AddRange(bytes);
            return this;
        }

        /// <summary>
        /// Vérification du fichier dont l'upload est demandé
        /// - Signature
        /// - MimeType et Extensions
        /// - Taille
        /// </summary>
        /// EG 20240523 [WI940][26663] Security : Mime Type enhancement
        /// EG 20240523 [WI940][26663] Security : Virtualisation de la méthode (Gestion particulière pour Mime Type XML - Gestion Offset de signature incrémental)
        public virtual FileTypeVerifyResult FileTypeVerify(UploadFileInfo pUploadFileInfo, int pCategoryContentMaxLength)
        {
            pUploadFileInfo.Stream.Position = OffsetSignatures;
            var reader = new BinaryReader(pUploadFileInfo.Stream);
            int length = SignatureLength;
            var headerBytes = reader.ReadBytes(length);

            FileTypeVerifyResult result = new FileTypeVerifyResult
            {
                Name = Name,
                Description = Description,
                IsSignatureVerified = (0 == length) || Signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature)),
                IsWithoutSignature = (0 == length),
                IsMimeTypeVerified = Type.Select(x => PrefixMimeType + x).Contains(pUploadFileInfo.MimeType) && 
                                     Extensions.Select(x => x).Contains(pUploadFileInfo.Extension),
                IsSizeVerified = (pUploadFileInfo.ContentLength <= (pCategoryContentMaxLength * 1000)) || (0 == pCategoryContentMaxLength)
            };
            return result;
        }

    }

    // ------------------------------------------------
    // Signatures des Mime Type : Catégorie APPLICATION
    // ------------------------------------------------

    /// <summary>
    /// Microsoft cabinet file (CAB)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Cab : MimeFile
    {
        public MimeFile_Cab()
        {
            Name = "CAB";
            Description = "Microsoft cabinet file";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("vnd.ms-cab-compressed");
            AddExtensions(".cab");
            AddSignatures(new byte[] { 0x4D, 0x53, 0x43, 0x46});
        }
    }
    /// <summary>
    /// Microsoft Windows HtmlHelp Data (CHM)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Chm : MimeFile
    {
        public MimeFile_Chm()
        {
            Name = "CHM";
            Description = "MS Windows HtmlHelp Data";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("octet-stream");
            AddExtensions(".chm");
            AddSignatures(new byte[] { 0x49, 0x54, 0x53, 0x46, 0x03, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00, 0x00 });
        }
    }

    /// <summary>
    /// Extendible Markup Language format (XML, XSD, XSL, XSLT)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Config : MimeFile
    {
        public MimeFile_Config()
        {
            Name = "CONFIG";
            Description = "Extendible Markup Language format";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("xml");
            AddExtensions(".config");
            AddSignatures(new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C, 0x20, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x22, 0x31, 0x2E, 0x30, 0x22, 0x3F, 0x3E },
                            new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C, 0x20 },
                            new byte[] { 0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00, 0x6C, 0x00, 0x20 },
                            new byte[] { 0x00, 0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00, 0x6C, 0x00, 0x20 },
                            new byte[] { 0x3C, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x6D, 0x00, 0x00, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00 },
                            new byte[] { 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x6D, 0x00, 0x00, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x20 });
        }
    }

    /// <summary>
    /// PEM encoded X.509 certificate (CRT)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Crt : MimeFile
    {
        public MimeFile_Crt()
        {
            Name = "CRT";
            Description = "PEM encoded X.509 certificate";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("x-x509-ca-cert");
            AddExtensions(".crt");
            AddSignatures(new byte[] { 0x2D, 0x2D, 0x2D, 0x2D, 0x2D, 0x42, 0x45, 0x47, 0x49, 0x4E, 0x20,
                                        0x43, 0x45, 0x52, 0x54, 0x49, 0x46, 0x49, 0x43, 0x41, 0x54, 0x45,
                                        0x2D, 0x2D, 0x2D, 0x2D, 0x2D});
        }
    }
    /// <summary>
    /// DER encoded X.509 certificate (DER)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Der : MimeFile
    {
        public MimeFile_Der()
        {
            Name = "DER";
            Description = "DER encoded X.509 certificate";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("x-x509-ca-cert");
            AddExtensions(".der");
            AddSignatures(new byte[] { 0x30, 0x82 });
        }
    }
    /// <summary>
    /// DOS MZ Executable and its descendants (EXE/DLL)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_DllExe : MimeFile
    {
        public MimeFile_DllExe()
        {
            Name = "EXE";
            Description = "DOS MZ Executable and its descendants";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("octet-stream", "x-msdownload");
            AddExtensions(".dll", ".exe");
            AddSignatures(new byte[] { 0x4D, 0x5A }, new byte[] { 0x5A, 0x4D });
        }
    }
    /// <summary>
    /// MSWord file (DOC)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Doc : MimeFile
    {
        public MimeFile_Doc()
        {
            Name = "DOC";
            Description = "MSWord file";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("msword");
            AddExtensions(".doc");
            AddSignatures(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0xA1, 0xE1 });
        }
    }
    /// <summary>
    /// MSWord - Zip File Format and formats based on it (DOCX)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Docx : MimeFile
    {
        public MimeFile_Docx()
        {
            Name = "DOCX";
            Description = "MSWord - Zip File Format and formats based on it";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("vnd.openxmlformats-officedocument.wordprocessingml.document");
            AddExtensions(".docx");
            AddSignatures(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 });
        }
    }
    /// <summary>
    /// Encapsulated PostScript file version 3.0 and 3.1 (EPS/EPSF)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Eps : MimeFile
    {
        public MimeFile_Eps()
        {
            Name = "EPS";
            Description = "Encapsulated PostScript file version 3.0 and 3.1";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("postscript");
            AddExtensions(".eps",".epsf");
            AddSignatures(new byte[] { 0x25, 0x21, 0x50, 0x53, 0x2D, 0x41, 0x64, 0x6F, 0x62, 0x65, 0x2D, 0x33, 0x2E, 0x30, 0x20, 0x45, 0x50, 0x53, 0x46, 0x2D, 0x33, 0x2E, 0x30 },
                          new byte[] { 0x25, 0x21, 0x50, 0x53, 0x2D, 0x41, 0x64, 0x6F, 0x62, 0x65, 0x2D, 0x33, 0x2E, 0x31, 0x20, 0x45, 0x50, 0x53, 0x46, 0x2D, 0x33, 0x2E, 0x30 });
        }
    }
    /// <summary>
    /// Lempel Ziv Huffman archive (GZ)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Gz : MimeFile
    {
        public MimeFile_Gz()
        {
            Name = "GZ";
            Description = "Lempel Ziv Huffman archive";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("x-gzip");
            OffsetSignatures = 0;
            AddExtensions(".gz");
            AddSignatures(new byte[] { 0x1F, 0x8B });
        }
    }
    /// <summary>
    /// Old Windows Help file (HLP)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Hlp : MimeFile
    {
        public MimeFile_Hlp()
        {
            Name = "HLP";
            Description = "Old Windows Help file";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("winhlp");
            AddExtensions(".hlp");
            AddSignatures(new byte[] { 0x3F, 0x5F });
        }
    }
    /// <summary>
    /// Lempel Ziv Huffman archive (LZH)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Lzh : MimeFile
    {
        public MimeFile_Lzh()
        {
            Name = "LZH";
            Description = "Lempel Ziv Huffman archive";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("octet-stream");
            OffsetSignatures = 2;
            AddExtensions(".lzh");
            AddSignatures(new byte[] { 0x2D, 0x68, 0x6C, 0x30, 0x2D }, new byte[] { 0x2D, 0x68, 0x6C, 0x35, 0x2D });
        }
    }
    /// <summary>
    /// File binary format for windows installer (MSI)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Msi : MimeFile
    {
        public MimeFile_Msi()
        {
            Name = "MSI";
            Description = "File binary format for windows installer";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("octet-stream");
            OffsetSignatures = 0;
            AddExtensions(".msi");
            AddSignatures(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0xA1, 0xE1 });
        }
    }
    /// <summary>
    /// Pdf document format (PDF)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Pdf : MimeFile
    {
        public MimeFile_Pdf()
        {
            Name = "PDF";
            Description = "Pdf document";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("pdf");
            AddExtensions(".pdf");
            AddSignatures(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D });
        }
    }
    /// <summary>
    /// MSPowerpoint file format (PPT)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Ppt : MimeFile
    {
        public MimeFile_Ppt()
        {
            Name = "PPT";
            Description = "MSPowerpoint file";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("vnd.ms-powerpoint");
            AddExtensions(".ppt");
            AddSignatures(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0xA1, 0xE1 });
        }
    }
    /// <summary>
    /// MSPowerpoint - Zip File Format and formats based on it (PPTX)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Pptx : MimeFile
    {
        public MimeFile_Pptx()
        {
            Name = "PPTX";
            Description = "MSPowerpoint - Zip File Format and formats based on it";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("vnd.openxmlformats-officedocument.presentationml.presentation");
            AddExtensions(".pptx");
            AddSignatures(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 });
        }
    }
    /// <summary>
    /// PostScript file (PS)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Ps : MimeFile
    {
        public MimeFile_Ps()
        {
            Name = "PS";
            Description = "PostScript file";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("postscript");
            AddExtensions(".ps");
            AddSignatures(new byte[] { 0x25, 0x21, 0x50, 0x53 });
        }
    }
    /// <summary>
    /// Photoshop document format (PSD)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Psd : MimeFile
    {
        public MimeFile_Psd()
        {
            Name = "PSD";
            Description = "Photoshop document format";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("octet-stream");
            AddExtensions(".psd");
            AddSignatures(new byte[] { 0x38, 0x42, 0x50, 0x53 });
        }
    }
    /// <summary>
    /// Roshal Archive compressed archive onwards (RAR)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Rar : MimeFile
    {
        public MimeFile_Rar()
        {
            Name = "RAR";
            Description = "Roshal Archive compressed archive onwards";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("rar");
            AddExtensions(".rar");
            AddSignatures(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 }, new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 });
        }
    }
    /// <summary>
    /// Richtext format (RTF)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Rtf : MimeFile
    {
        public MimeFile_Rtf()
        {
            Name = "RTF";
            Description = "Richtext format";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("rtf","msword");
            AddExtensions(".rtf");
            AddSignatures(new byte[] { 0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31});
        }
    }
    /// <summary>
    /// Adobe flash (SWF)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Swf : MimeFile
    {
        public MimeFile_Swf()
        {
            Name = "SWF";
            Description = "Adobe flash";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("x-shockwave-flash");
            AddExtensions(".swf");
            AddSignatures(new byte[] { 0x43, 0x57, 0x53 }, new byte[] { 0x46, 0x57, 0x53 });
        }
    }
    /// <summary>
    /// WebAssembly binary format (WASM)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Wasm : MimeFile
    {
        public MimeFile_Wasm()
        {
            Name = "WASM";
            Description = "WebAssembly binary format";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("wasm");
            AddExtensions(".wasm");
            AddSignatures(new byte[] { 0x00, 0x61, 0x73, 0x6D });
        }
    }
    /// <summary>
    /// Windows metafile (WMF)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Wmf : MimeFile
    {
        public MimeFile_Wmf()
        {
            Name = "WMF";
            Description = "Windows metafile";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("x-msmetafile");
            AddExtensions(".wmf");
            AddSignatures(new byte[] { 0xD7, 0xCD, 0xC6, 0x9A });
        }
    }
    /// <summary>
    /// MSExcel file (XLS)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Xls : MimeFile
    {
        public MimeFile_Xls()
        {
            Name = "XLS";
            Description = "MSExcel file";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("vnd.ms-excel");
            AddExtensions(".xls");
            AddSignatures(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0xA1, 0xE1 });
        }
    }
    /// <summary>
    /// MSExcel - Zip File Format and formats based on it (XLSX)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Xlsx : MimeFile
    {
        public MimeFile_Xlsx()
        {
            Name = "XLSX";
            Description = "MSExcel - Zip File Format and formats based on it";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            AddExtensions(".xlsx");
            AddSignatures(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 });
        }
    }

    /// <summary>
    /// Lempel Ziv Huffman archive (Z)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Z : MimeFile
    {
        public MimeFile_Z()
        {
            Name = "Z";
            Description = "Lempel Ziv Huffman archive";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("octet-stream");
            AddExtensions(".z","tar.z");
            AddSignatures(new byte[] { 0x1F, 0x9D }, new byte[] { 0x1F, 0xA0 });
        }
    }
    /// <summary>
    /// Zip file format (ZIP)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Zip : MimeFile
    {
        public MimeFile_Zip()
        {
            Name = "ZIP";
            Description = "Zip file format";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("x-zip-compressed");
            AddExtensions(".zip");
            AddSignatures(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                            new byte[] { 0x50, 0x4B, 0x07, 0x08 });
        }
    }

    // ------------------------------------------------
    // Signatures des Mime Type : Catégorie AUDIO
    // ------------------------------------------------

    /// <summary>
    /// Midi sound format (MID, MIDI)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Mid : MimeFile
    {
        public MimeFile_Mid()
        {
            Name = "MID";
            Description = "Midi sound format";
            Category = MimeMappingTools.TypeCategoryEnum.audio;
            AddMimeType("mid");
            AddExtensions(".mid", ".midi");
            AddSignatures(new byte[] { 0x4D, 0x54, 0x68, 0x64 });
        }
    }
    /// <summary>
    /// MP3 file with ID3v2 container (MP3)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Mp3 : MimeFile
    {
        public MimeFile_Mp3()
        {
            Name = "MP3";
            Description = "MP3 file with ID3v2 container";
            Category = MimeMappingTools.TypeCategoryEnum.audio;
            AddMimeType("mpeg");
            AddExtensions(".mp3");
            AddSignatures(new byte[] { 0xFF, 0xFB }, new byte[] { 0xFF, 0xF3 }, new byte[] { 0xFF, 0xF2 }, new byte[] { 0x49, 0x44, 0x33 });
        }
    }
    /// <summary>
    /// Oga, an open source media container format (OGA)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Oga : MimeFile
    {
        public MimeFile_Oga()
        {
            Name = "OGA";
            Description = "Oga, an open source media container format";
            Category = MimeMappingTools.TypeCategoryEnum.audio;
            AddMimeType("ogg");
            AddExtensions(".oga");
            AddSignatures(new byte[] { 0x4F, 0x67, 0x67, 0x53 });
        }
    }
    /// <summary>
    /// RedHat Package Manager (RPM)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Rpm : MimeFile
    {
        public MimeFile_Rpm()
        {
            Name = "RPM";
            Description = "RedHat Package Manager";
            Category = MimeMappingTools.TypeCategoryEnum.audio;
            AddMimeType("x-pn-realaudio-plugin");
            AddExtensions(".rpm");
            AddSignatures(new byte[] { 0xED, 0xAB, 0xEE, 0xDB });
        }
    }
    /// <summary>
    /// Audio file format (SND)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Snd : MimeFile
    {
        public MimeFile_Snd()
        {
            Name = "SND";
            Description = "Audio file format";
            Category = MimeMappingTools.TypeCategoryEnum.audio;
            AddMimeType("basic");
            AddExtensions(".au", ".snd");
            AddSignatures(new byte[] { 0x2E, 0x73, 0x6E, 0x64 });
        }
    }
    /// <summary>
    /// Advanced Systems Format (WMA)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Wma : MimeFile
    {
        public MimeFile_Wma()
        {
            Name = "WMA";
            Description = "Advanced Systems Format";
            Category = MimeMappingTools.TypeCategoryEnum.audio;
            AddMimeType("x-ms-wma");
            AddExtensions(".wma");
            AddSignatures(new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C });
        }
    }


    // ------------------------------------------------
    // Signatures des Mime Type : Catégorie IMAGE
    // ------------------------------------------------

    /// <summary>
    /// Bitmap file format (BMP)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Bmp : MimeFile
    {
        public MimeFile_Bmp()
        {
            Name = "BMP";
            Description = "Bitmap file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("bmp");
            AddExtensions(".bmp", ".dib");
            AddSignatures(new byte[] { 0x42, 0x40 });
        }
    }
    /// <summary>
    /// Graphics Interchange file format (GIF)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Gif : MimeFile
    {
        public MimeFile_Gif()
        {
            Name = "GIF";
            Description = "Graphics Interchange file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("gif");
            AddExtensions(".gif");
            AddSignatures(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 });
        }
    }
    /// <summary>
    /// Icon file format (ICON)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Icon : MimeFile
    {
        public MimeFile_Icon()
        {
            Name = "ICON";
            Description = "Icon file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("x-icon");
            AddExtensions(".ico");
            AddSignatures(new byte[] { 0x00, 0x00, 0x01, 0x00 });
        }
    }
    /// <summary>
    /// Joint Photographic Experts Group file format (JPEG)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Jpeg : MimeFile
    {
        public MimeFile_Jpeg()
        {
            Name = "JPEG";
            Description = "Joint Photographic Experts Group file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("jpeg");
            AddExtensions(".jpg", ".jpeg");
            AddSignatures(new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, new byte[] { 0xFF, 0xD8, 0xFF, 0xEE }, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        }
    }
    /// <summary>
    /// Portable Bitmap file format (PBM)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Pbm : MimeFile
    {
        public MimeFile_Pbm()
        {
            Name = "PBM";
            Description = "Portable Bitmap file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("x-portable-bitmap");
            AddExtensions(".pbm");
            AddSignatures(new byte[] { 0x50, 0x31, 0x0A }, new byte[] { 0x50, 0x34, 0x0A });
        }
    }
    /// <summary>
    /// Portable Graymap file format (PGM)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Pgm : MimeFile
    {
        public MimeFile_Pgm()
        {
            Name = "PGM";
            Description = "Portable Graymap file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("x-portable-graymap");
            AddExtensions(".pgm");
            AddSignatures(new byte[] { 0x50, 0x32, 0x0A }, new byte[] { 0x50, 0x35, 0x0A });
        }
    }
    /// <summary>
    /// Portable Network Graphic file format (PNG)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Png : MimeFile
    {
        public MimeFile_Png()
        {
            Name = "PNG";
            Description = "Portable Network Graphic file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("png");
            AddExtensions(".png", ".pnz");
            AddSignatures(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        }
    }
    /// <summary>
    /// Portable Pixmap file format (PPM)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Ppm : MimeFile
    {
        public MimeFile_Ppm()
        {
            Name = "PPM";
            Description = "Portable Pixmap file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("x-portable-pixmap");
            AddExtensions(".ppm");
            AddSignatures(new byte[] { 0x50, 0x33, 0x0A }, new byte[] { 0x50, 0x36, 0x0A });
        }
    }
    /// <summary>
    /// Tagged Image file format (TIFF)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Tiff : MimeFile
    {
        public MimeFile_Tiff()
        {
            Name = "TIFF";
            Description = "Tagged Image file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("tiff");
            AddExtensions(".tif", ".tiff");
            AddSignatures(new byte[] { 0x49, 0x49, 0x2A, 0x00 }, new byte[] { 0x4D, 0x4D, 0x00, 0x2A });
        }
    }
    /// <summary>
    /// X Bitmap file format (XBM)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Xbm : MimeFile
    {
        public MimeFile_Xbm()
        {
            Name = "XBM";
            Description = "X Bitmap file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("x-xbitmap");
            AddExtensions(".xbm");
            AddSignatures(new byte[] { 0x23, 0x64, 0x65, 0x66, 0x69, 0x6E, 0x65, 0x20 });
        }
    }
    /// <summary>
    /// X Pixmap file format (XPM)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Xpm : MimeFile
    {
        public MimeFile_Xpm()
        {
            Name = "XPM";
            Description = "X Pixmap file format";
            Category = MimeMappingTools.TypeCategoryEnum.image;
            AddMimeType("x-xpixmap");
            AddExtensions(".xpm");
            AddSignatures(new byte[] { 0x2F, 0x2A, 0x20, 0x58, 0x50, 0x4D, 0x20, 0x2A, 0x2F });
        }
    }


    // ------------------------------------------------
    // Signatures des Mime Type : Catégorie MESSAGE
    // ------------------------------------------------


    /// <summary>
    /// Email file format (EML)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Eml : MimeFile
    {
        public MimeFile_Eml()
        {
            Name = "EML";
            Description = "Email Message";
            Category = MimeMappingTools.TypeCategoryEnum.message;
            AddMimeType("rfc822");
            AddExtensions(".eml");
            AddSignatures(new byte[] { 0x52, 0x65, 0x63, 0x65, 0x69, 0x76, 0x65, 0x64, 0x3A });
        }
    }


    // ------------------------------------------------
    // Signatures des Mime Type : Catégorie TEXT
    // ------------------------------------------------

    /// <summary>
    /// Comma-Separated Values format (CSV)
    /// ATTENTION PAS DE SIGNATURES
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Csv : MimeFile
    {
        public MimeFile_Csv()
        {
            Name = "CSV";
            Description = "Comma-Separated Values format";
            Category = MimeMappingTools.TypeCategoryEnum.text;
            AddMimeType("csv");
            AddExtensions(".csv");
        }
    }

    /// <summary>
    /// Text file format (TXT)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Txt : MimeFile
    {
        public MimeFile_Txt()
        {
            Name = "TXT";
            Description = "Text file format";
            Category = MimeMappingTools.TypeCategoryEnum.text;
            AddMimeType("plain");
            AddExtensions(".txt");
            AddSignatures(new byte[] { 0xEF, 0xBB, 0xBF },
                            new byte[] { 0xFF, 0xFE },
                            new byte[] { 0xFE, 0xFF },
                            new byte[] { 0xFF, 0xFE, 0x00, 0x00 },
                            new byte[] { 0x00, 0x00, 0xFF, 0xFE },
                            new byte[] { 0x2B, 0x2F, 0x76, 0x38 },
                            new byte[] { 0x2B, 0x2F, 0x76, 0x39 },
                            new byte[] { 0x2B, 0x2F, 0x76, 0x2B },
                            new byte[] { 0x2B, 0x2F, 0x76, 0x2F },
                            new byte[] { 0x0E, 0xFE, 0xFF },
                            new byte[] { 0x22, 0x53, 0x70, 0x68, 0x65, 0x72, 0x65, 0x73, 0x54, 0x72, 0x61, 0x63, 0x65, 0x22, 0x3B /*"SpheresTrace;"*/ });
        }
    }
    /// <summary>
    /// Classe de base pour : Extendible Markup Language format (XML, XSL, XSLT)
    /// </summary>
    /// EG 20240523 [WI940][26663] Security : Mime Type enhancement : New abstract class
    public abstract class MimeFile_XmlBase : MimeFile
    {
        public MimeFile_XmlBase()
        {
            Description = "Extendible Markup Language format";
            AddSignatures(new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C, 0x20, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x22, 0x31, 0x2E, 0x30, 0x22, 0x3F, 0x3E },
                            new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C, 0x20 },
                            new byte[] { 0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00, 0x6C, 0x00, 0x20 },
                            new byte[] { 0x00, 0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00, 0x6C, 0x00, 0x20 },
                            new byte[] { 0x3C, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x6D, 0x00, 0x00, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00 },
                            new byte[] { 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x6D, 0x00, 0x00, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x20 });
        }
        /// <summary>
        /// Verification du fichier de type XML
        /// Cas particulier  avec un gestion Offset de signature par incrémentation
        /// </summary>
        /// <param name="pUploadFileInfo">Caractéristiques du fichier</param>
        /// <param name="pCategoryContentMaxLength">Taille maximal autorisée à l'upload pour ce type de fichier</param>
        /// <returns></returns>
        // EG 20240523 [WI940][26663] Security : Mime Type enhancement
        public override FileTypeVerifyResult FileTypeVerify(UploadFileInfo pUploadFileInfo, int pCategoryContentMaxLength)
        {
            FileTypeVerifyResult result = null;
            while (OffsetSignatures < 4)
            {
                result = base.FileTypeVerify(pUploadFileInfo, pCategoryContentMaxLength);
                if (result.IsVerified)
                    break;
                else
                    OffsetSignatures++;
            }
            OffsetSignatures = 0;
            return result;
        }
    }
    /// <summary>
    /// Extendible Markup Language format (XSLT)
    /// </summary>
    /// EG 20240523 [WI940][26663] Security : Mime Type enhancement / New
    public sealed class MimeFile_Xslt : MimeFile_XmlBase
    {
        public MimeFile_Xslt() : base()
        {
            Name = "XSLT";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("xslt+xml");
            AddExtensions(".xslt");
        }
    }

    /// <summary>
    /// Extendible Markup Language format (XML : MimeType application/xml)
    /// </summary>
    /// EG 20240523 [WI940][26663] Security : Mime Type enhancement / New
    public sealed class MimeFile_Xml : MimeFile_XmlBase
    {
        public MimeFile_Xml() : base()
        {
            Name = "XML";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("xml");
            AddExtensions(".xml");
        }
    }
    /// <summary>
    /// Extendible Markup Language format (XML, XSL, XSLT)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    /// EG 20240523 [WI940][26663] Security : Mime Type enhancement / New
    public sealed class MimeFile_TextXml : MimeFile_XmlBase 
    {
        public MimeFile_TextXml() : base()
        {
            Name = "XML";
            Category = MimeMappingTools.TypeCategoryEnum.text;
            AddMimeType("xml");
            AddExtensions(".xml", ".xsl", ".xslt");
        }
    }
    /// <summary>
    /// Extendible Markup Language format (XSD)
    /// </summary>
    /// EG 20240523 [WI940] Security : Mime Type enhancement / New
    public sealed class MimeFile_Xsd : MimeFile_XmlBase
    {
        public MimeFile_Xsd()
        {
            Name = "XML";
            Category = MimeMappingTools.TypeCategoryEnum.application;
            AddMimeType("xml");
            AddExtensions(".xsd");
        }
    }
    // ------------------------------------------------
    // Signatures des Mime Type : Catégorie VIDEO
    // ------------------------------------------------

    /// <summary>
    /// Audio video Interleave format (AVI)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Avi : MimeFile
    {
        public MimeFile_Avi()
        {
            Name = "AVI";
            Description = "Audio Video Interleave format";
            Category = MimeMappingTools.TypeCategoryEnum.video;
            AddMimeType("avi");
            AddExtensions(".avi");
            AddSignatures(new byte[] { 0x52, 0x49, 0x46, 0x46 });
        }
    }
    /// <summary>
    /// Flash video file format (FLV)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Flv : MimeFile
    {
        public MimeFile_Flv()
        {
            Name = "FLV";
            Description = "Flash video file";
            Category = MimeMappingTools.TypeCategoryEnum.video;
            AddMimeType("x-flv");
            AddExtensions(".flv");
            AddSignatures(new byte[] { 0x46, 0x4C, 0x56 });
        }
    }
    /// <summary>
    /// MP4  file ISO base Media file format (MP4)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Mp4 : MimeFile
    {
        public MimeFile_Mp4()
        {
            Name = "MP4";
            Description = "MP4 file ISO base Media file";
            Category = MimeMappingTools.TypeCategoryEnum.video;
            AddMimeType("mpeg");
            AddExtensions(".mp4", ".mp4v", ".m4v");
            AddSignatures(new byte[] { 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D });
        }
    }
    /// <summary>
    /// MPEG Program video file format (MPG)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Mpg : MimeFile
    {
        public MimeFile_Mpg()
        {
            Name = "MPG";
            Description = "MPEG Program video";
            Category = MimeMappingTools.TypeCategoryEnum.video;
            AddMimeType("mpeg");
            AddExtensions(".mpg", ".mpeg", "mp2", ".mpv2", ".mpe", ".m1v");
            AddSignatures(new byte[] { 0x00, 0x00, 0x01, 0xBA }, new byte[] { 0x00, 0x00, 0x01, 0xB3 });
        }
    }
    /// <summary>
    /// Ogg media container file format (OGG)
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeFile_Ogg : MimeFile
    {
        public MimeFile_Ogg()
        {
            Name = "OGG";
            Description = "Ogg, an open source media container format";
            Category = MimeMappingTools.TypeCategoryEnum.video;
            AddMimeType("ogg");
            AddExtensions(".ogg",".ogv");
            AddSignatures(new byte[] { 0x4F, 0x67, 0x67, 0x53 });
        }
    }

    // ------------------------------------------------
    // Signatures des Mime Type : Catégorie OTHERS
    // ------------------------------------------------
    // NOTHING



    // -----------------------------------------
    // Regroupement des signatures par Categorie
    // -----------------------------------------

    /// <summary>
    /// Classe abstraite de gestion des catégories de Mime
    /// - Application, Audio, Image, Message, Text, Video et Other
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public abstract class MimeCategory
    {
        /// <summary>
        /// Nom de la catégorie
        /// </summary>
        public MimeMappingTools.TypeCategoryEnum Category { get; set; }
        /// <summary>
        /// Retourne true si la catégorie est autorisée à être lue pour un upload de fichiers (ATTACHEDOC)
        /// => Lecture de la clé "UploadFile_CategoryAuthorized" dans le fichier de configuration 
        ///    Si pas de spécification, les catégories autorisées par défaut sont = defaultUpload (application | image | text | video)
        /// => seule la catégorie "Image" est en dur autorisée pour l'upload d'un fichier pour alimenter le LOGO d'un acteur
        /// </summary>
        public bool IsAuthorized { get; set; }
        /// <summary>
        /// Liste des signatures de fichiers managés appartenant à la catégorie 
        /// (NB : toute extension de fichier non présente dans cette liste est considérée comme non managée)
        /// </summary>
        protected List<MimeFile> lstMimeFiles = new List<MimeFile>();
        /// <summary>
        /// Longueur maximale autorisée d'un fichier de cette catégorie pour l'upload 
        /// => si non spécifié dans le fichier de configuration alors :
        ///     - Longueur maximale autorisée d'un fichier pour l'upload (toute catégorie) 
        ///     - si non spécifiée dans le fichier de configuration alors aucune restriction
        /// </summary>
        protected int categoryContentMaxLength = 0;
        /// <summary>
        /// Chargement des types de fichiers pour la catégorie
        /// </summary>
        /// <param name="mimeFiles">Liste de MimeFiles</param>
        /// <returns></returns>
        protected List<MimeFile> AddMimeFiles(params MimeFile[] mimeFiles)
        {
            lstMimeFiles.AddRange(mimeFiles);
            lstMimeFiles.OrderByDescending(x => x.SignatureLength);
            return this.lstMimeFiles;
        }


        /// <summary>
        /// Initialisation d'une catégorie 
        /// - Nom de la catégorie
        /// - Setting des paramètres de restriction par lecture du fichier de configuration
        /// - Alimentation de la laiste des fichiers managés (extensions et signatures)
        /// </summary>
        /// <param name="pCategory"></param>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        public MimeCategory(MimeMappingTools.TypeCategoryEnum pCategory)
        {
            Category = pCategory;
            // Taille maximale autorisée
            categoryContentMaxLength = SessionTools.GetUploadFile_CategoryContentMaxLength(Category);
        }

        /// <summary>
        /// Méthode de vérification (autorisation) d'upload d'un fichier
        /// - Filtre sur Mimefile autorisés et interdits
        /// - Contrôle de signature
        /// - Contrôle du Mime type et de l'extension du fichier
        /// - Contrôle de la taille
        /// </summary>
        /// <param name="pUploadFileInfo">Caractéristiques du fichier à uploader et paramètres (webCustom.config)</param>
        /// <returns></returns>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        public FileTypeVerifyResult FileTypeVerify(UploadFileInfo pUploadFileInfo)
        {
            FileTypeVerifyResult result = null;
            List<MimeFile> mimeFiles = lstMimeFiles;

            // On filtre sur les MimeFiles autorisés
            if (pUploadFileInfo.IsMimeFileNameAuthorized)
                mimeFiles = (from item in mimeFiles
                                where pUploadFileInfo.mimeFileNameAuthorized.Contains(item.Name)
                                    select item).ToList();

            // On Exclue les MimeFiles interdits
            if (pUploadFileInfo.IsMimeFileNameExcluded)
                mimeFiles = (from item in mimeFiles
                            where (false == pUploadFileInfo.mimeFileNameExcluded.Contains(item.Name))
                                select item).ToList();

            foreach (MimeFile item in mimeFiles)
            {
                // Vérification
                result = item.FileTypeVerify(pUploadFileInfo, categoryContentMaxLength);

                if ((null != result) && result.IsEndOfVerify) // Fin de vérification
                    break;
            }
            return SetVerificationMessage(result, pUploadFileInfo);
        }

        /// <summary>
        /// Affichage du message de retour après vérification.
        /// </summary>
        /// <param name="pResult">Résultat de la vérication</param>
        /// <param name="pUploadFileInfo">Caractéristiques du fichier à uploader et paramètres (webCustom.config)</param>
        /// <returns></returns>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        private FileTypeVerifyResult SetVerificationMessage(FileTypeVerifyResult pResult, UploadFileInfo pUploadFileInfo)
        {
            FileTypeVerifyResult result = pResult;
            if (null == result)
            {
                // Aucun Mapping, l'upload est rejeté 
                result = new FileTypeVerifyResult()
                {
                    Name = "Unmanaged",
                    Description = "Unmanaged File Type for upload",
                    ErrMessage = String.Format(Ressource.GetString("lblFileUploadedUnmanagedType"), pUploadFileInfo.Name)
                };
            }
            else if (false == result.IsSignatureVerified)
            {
                // Mapping mais signature non valide ou fichier non autorisé (via config), l'upload est rejeté 
                result.ErrMessage = String.Format(Ressource.GetString("lblFileUploadedRejectedType"), pUploadFileInfo.Name);
            }
            else if (false == result.IsMimeTypeVerified)
            {
                // Mapping et signature valides mais mimeType non valide (cas d'un fichier dont l'extension a été modifiée manuellement)
                result.ErrMessage = String.Format(Ressource.GetString(result.IsWithoutSignature?"lblFileUploadedCorruptedType": "lblFileUploadedUnmanagedType"), 
                    pUploadFileInfo.Name, pUploadFileInfo.MimeType);
            }
            else if (false == result.IsSizeVerified)
            {
                // Mapping, signature et mimeType valides mais la taille du fichier ne respecte pas les paramètres du fichier de configuration
                result.ErrMessage = String.Format(Ressource.GetString("lblFileUploadedIllegalLength"), pUploadFileInfo.Name, pUploadFileInfo.ContentLength);
            }
            else
            {
                // Upload autorisé
                result.ErrMessage = Ressource.GetString("lblFileUploadedSuccess");
            }
            return result;
        }

    }
    /// <summary>
    /// Classe de mapping de la catégorie APPLICATION
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    /// EG 20240523 [WI940][26663] Security : Mime Type enhancement (add MimeFile_Xml,MimeFile_Xslt,MimeFile_Xsd)
    public sealed class MimeCategoryApplication : MimeCategory
    {
        /// <summary>
        /// Initialisation de la catégorie et des signatures de fichiers gérés pour celle-ci
        /// </summary>
        public MimeCategoryApplication() : base(MimeMappingTools.TypeCategoryEnum.application)
        {
            AddMimeFiles(new MimeFile_Cab(), new MimeFile_Chm(), new MimeFile_Config(), new MimeFile_Crt(), 
                         new MimeFile_Der(), new MimeFile_DllExe(), new MimeFile_Doc(), new MimeFile_Docx(),
                         new MimeFile_Eps(), 
                         new MimeFile_Gz(),
                         new MimeFile_Hlp(), 
                         new MimeFile_Lzh(),
                         new MimeFile_Msi(),
                         new MimeFile_Pdf(), new MimeFile_Ppt(), new MimeFile_Pptx(), new MimeFile_Ps(), new MimeFile_Psd(),
                         new MimeFile_Rar(), new MimeFile_Rtf(), 
                         new MimeFile_Swf(), 
                         new MimeFile_Wasm(), new MimeFile_Wmf(),
                         new MimeFile_Xls(), new MimeFile_Xlsx(), new MimeFile_Xml(), new MimeFile_Xslt(), new MimeFile_Xsd(),
                         new MimeFile_Z(), new MimeFile_Zip());
        }
    }
    /// <summary>
    /// Classe de mapping de la catégorie AUDIO
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeCategoryAudio : MimeCategory
    {
        /// <summary>
        /// Initialisation de la catégorie et des signatures de fichiers gérés pour celle-ci
        /// </summary>
        public MimeCategoryAudio() : base(MimeMappingTools.TypeCategoryEnum.audio)
        {
            AddMimeFiles(new MimeFile_Mid(), new MimeFile_Mp3(),
                         new MimeFile_Oga(), 
                         new MimeFile_Rpm(), 
                         new MimeFile_Snd(),
                         new MimeFile_Wma());
        }
    }
    /// <summary>
    /// Classe de mapping de la catégorie IMAGE
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeCategoryImage : MimeCategory
    {
        /// <summary>
        /// Initialisation de la catégorie et des signatures de fichiers gérés pour celle-ci
        /// </summary>
        public MimeCategoryImage() : base(MimeMappingTools.TypeCategoryEnum.image)
        {
            AddMimeFiles(new MimeFile_Bmp(), 
                         new MimeFile_Gif(), 
                         new MimeFile_Icon(), 
                         new MimeFile_Jpeg(), 
                         new MimeFile_Pbm(), new MimeFile_Pgm(), new MimeFile_Png(), new MimeFile_Ppm(), 
                         new MimeFile_Tiff(),
                         new MimeFile_Xbm(), new MimeFile_Xpm());
        }
    }
    /// <summary>
    /// Classe de mapping de la catégorie MESSAGE
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeCategoryMessage : MimeCategory
    {
        /// <summary>
        /// Initialisation de la catégorie et des signatures de fichiers gérés pour celle-ci
        /// </summary>
        public MimeCategoryMessage() : base(MimeMappingTools.TypeCategoryEnum.message)
        {
            AddMimeFiles(new MimeFile_Eml());
        }
    }
    /// <summary>
    /// Classe de mapping de la catégorie TEXT
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    /// EG 20240523 [WI940][26663] Security : Mime Type enhancement (Remove MimeFile_Xml, add MimeFile_TextXml) 
    public sealed class MimeCategoryText : MimeCategory
    {
        /// <summary>
        /// Initialisation de la catégorie et des signatures de fichiers gérés pour celle-ci
        /// </summary>
        public MimeCategoryText() : base(MimeMappingTools.TypeCategoryEnum.text)
        {
            AddMimeFiles(new MimeFile_Txt(), new MimeFile_TextXml(), new MimeFile_Csv() /* CSV At the end of list because no Signature */);
        }
    }
    /// <summary>
    /// Classe de mapping de la catégorie VIDEO
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeCategoryVideo : MimeCategory
    {
        /// <summary>
        /// Initialisation de la catégorie et des signatures de fichiers gérés pour celle-ci
        /// </summary>
        public MimeCategoryVideo() : base(MimeMappingTools.TypeCategoryEnum.video)
        {
            AddMimeFiles(new MimeFile_Avi(), 
                         new MimeFile_Flv(), 
                         new MimeFile_Mp4(), new MimeFile_Mpg(), 
                         new MimeFile_Ogg());
        }
    }
    /// <summary>
    /// Classe de mapping de la catégorie OTHER
    /// </summary>
    /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
    public sealed class MimeCategoryOther : MimeCategory
    {
        /// <summary>
        /// Initialisation de la catégorie et des signatures de fichiers gérés pour celle-ci
        /// </summary>
        public MimeCategoryOther() : base(MimeMappingTools.TypeCategoryEnum.other)
        {
        }
    }
}
