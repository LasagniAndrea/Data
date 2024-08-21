using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EFS.Common
{
    /// <summary>
    /// 
    /// </summary>
    class IFileSystemCompareBySize : IComparer
    {
        public int Compare(object _Info1, object _Info2)
        {
            // Only FILES
            long fileSize1 = !(_Info1 is FileInfo fileInfo1) ? -1 : fileInfo1.Length;
            long fileSize2 = !(_Info2 is FileInfo fileInfo2) ? -1 : fileInfo2.Length;
            if (fileSize1 > fileSize2) return 1;
            if (fileSize1 < fileSize2) return -1;
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class IFileSystemCompareByName : IComparer
    {
        public int Compare(object _Info1, object _Info2)
        {
            FileSystemInfo fileInfo1 = _Info1 as FileSystemInfo;
            FileSystemInfo fileInfo2 = _Info2 as FileSystemInfo;

            return (fileInfo1.Name.CompareTo(fileInfo2.Name));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class FileTools
    {
        /// <summary>
        /// Pilote l'écriture d'un fichier lorsque le fichier existe déjà 
        /// </summary>
        /// FI 20230209 [XXXXX] Add
        public enum WriteFileOverrideMode
        {
            /// <summary>
            /// Ecrase un fichier
            /// </summary>
            Override,
            /// <summary>
            /// Ajoute nouveau fichier avec un suffixe (Exemple File.txt, File(1).txt, File(2).txt)
            /// </summary>
            New,
            /// <summary>
            /// Aucune action
            /// </summary>
            None,
        }
        
        /// <summary>
        /// Maximum pass in loop
        /// </summary>
        public static int MaxLoopPass = 10;

        /// <summary>
        /// Thread sleep in millisecondes befor looping
        /// </summary>
        public static int ThreadSleepBeforLoop = 5000;

        #region public ErrLevel
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160503 [XXXXX] suppression de UNDEFINED
        public enum ErrLevel
        {
            /// <summary>
            /// 
            /// </summary>
            SUCCESS,
            /// <summary>
            /// Le fichier est introuvable.
            /// </summary>
            FILENOTFOUND,
            /// <summary>
            /// Une erreur d'E/S s'est produite lors de l'ouverture du fichier.
            /// </summary>
            IOEXCEPTION
        }
        #endregion ErrLevel

        /// <summary>
        /// Retourne la position de début de la dernière ligne dans un fichier.
        /// <para>See also: GetLastLine()</para>
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2202]
        public static long FindLastLinePosition(string pPath)
        {
            long index = 0;
            //PL 20130710 Add FileAccess.Read and , FileShare.ReadWrite
            //using (BinaryReader br = new BinaryReader(File.Open(pPath, FileMode.Open)))
            using (BinaryReader br = new BinaryReader(File.Open(pPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                index = br.BaseStream.Length - 2;
                char repere = '\n';
                br.BaseStream.Position = index;
                char current = br.ReadChar();
                while (!(current.Equals(repere)))
                {
                    index -= 1;
                    br.BaseStream.Position = index;
                    current = br.ReadChar();
                }
                //br.Close();
            }
            return index + 1;
        }

        /// <summary>
        /// Retourne la dernière ligne d'un fichier.
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        public static string GetLastLine(string pPath)
        {
            long position = FindLastLinePosition(pPath);
            return GetLine(pPath, position);
        }

        /// <summary>
        /// Retourne une ligne d'un fichier.
        /// </summary>
        /// <param name="pPath"></param>
        /// <param name="pPosition"></param>
        /// <returns></returns>
        public static string GetLine(string pPath, long pPosition)
        {
            string lastLine = null;
            //PL 20130710 Add FileAccess.Read and FileShare.ReadWrite
            //using (StreamReader sr = new StreamReader(File.Open(pPath, FileMode.Open)))
            using (StreamReader sr = new StreamReader(File.Open(pPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                sr.BaseStream.Position = pPosition;
                lastLine = sr.ReadLine();
            }
            return lastLine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        public static void CopyDir(string sourceDir, string destDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            if (dir.Exists)
            {
                string realDestDir;
                if (dir.Root.Name != dir.Name)
                {
                    realDestDir = Path.Combine(destDir, dir.Name);
                    if (!Directory.Exists(realDestDir))
                        Directory.CreateDirectory(realDestDir);
                }
                else
                {
                    realDestDir = destDir;
                }
                foreach (string d in Directory.GetDirectories(sourceDir))
                {
                    CopyDir(d, realDestDir);
                }
                foreach (string file in Directory.GetFiles(sourceDir))
                {

                    string fileNameDest = Path.Combine(realDestDir, Path.GetFileName(file));
                    try
                    {
                        File.Copy(file, fileNameDest, true);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Attribut l'extension .bak au fichier spécifié
        /// <para>Supprime aau préalable le fichier .bak s'il est déjà présent</para>
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        public static ErrLevel FileRenameToDotBak(string pPath)
        {
            string pPathBak = pPath + ".bak";
            FileDelete2(pPathBak);
            return FileMove2(pPath, ref pPathBak);
        }

        /// <summary>
        /// Affecte un nouvel emplacement au fichier {pPath}
        /// </summary>
        /// <param name="pPath">Fichier source</param>
        /// <param name="pPath2">Nouveau nom du fichier</param>
        /// <returns></returns>
        public static ErrLevel FileMove2(string pPath, ref string pPath2)
        {
            return FileMove2(pPath, ref pPath2, true);
        }
        /// <summary>
        /// Affecte un nouvel emplacement au fichier {pPath}
        /// </summary>
        /// <param name="pPath">Fichier source</param>
        /// <param name="pPath2">Nouveau nom du fichier</param>
        /// <param name="pUseNativMove">si true, usage de la méthode File.Move; si false, usage de la méthode FileCopy2</param>
        /// <returns>FILENOTFOUND si le fichier n'existe pas</returns>
        private static ErrLevel FileMove2(string pPath, ref string pPath2, bool pUseNativMove)
        {
            ErrLevel ret = ErrLevel.SUCCESS;
            //
            if (pUseNativMove)
            {
                pPath2 = GetNotExistingFileName(pPath2);
                try
                {
                    File.Move(pPath, pPath2);
                }
                catch (FileNotFoundException)
                {
                    ret = ErrLevel.FILENOTFOUND;
                }
            }
            else
            {
                ret = FileCopy2(pPath, ref pPath2);
                if (ErrLevel.SUCCESS == ret)
                    FileDelete2(pPath);
            }
            //
            return ret;
        }

        /// <summary>
        /// Déplacement d'un fichier.
        /// <para>Cette méthode est une évolution de la méthode FileMove2, l'exception IOException est trappée</para>
        /// </summary>
        /// <param name="pPath">Source</param>
        /// <param name="pPath2">Destination</param>
        /// <returns>FILENOTFOUND si le fichier n'existe pas (FileNotFoundException trappée),IOEXCEPTION si une IOException se produit (IOException trappée)</returns>
        public static ErrLevel FileMove3(string pPath, ref string pPath2)
        {
            ErrLevel ret = ErrLevel.SUCCESS;

            try
            {
                pPath2 = GetNotExistingFileName(pPath2);

                using (FileStream fs = File.Open(pPath, FileMode.Open, FileAccess.Read, FileShare.Delete))
                {
                    File.Move(pPath, pPath2);
                }

            }
            catch (FileNotFoundException)
            {
                ret = ErrLevel.FILENOTFOUND;
            }
            catch (IOException)
            {
                //EG/PL Catch added
                ret = ErrLevel.IOEXCEPTION;
            }
            catch
            {
                throw;
            }

            return ret;
        }

        /// <summary>
        /// Supprime le fichier spécifié lorsqu'il existe
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        public static void FileDelete2(string pPath)
        {
            FileDelete2(pPath, false);
        }

        /// <summary>
        /// Supprime le fichier spécifié lorsqu'il existe
        /// </summary>
        /// <param name="pPath"></param>
        /// <param name="pIsSetNormal">si true, applique au fichier l'attribut FileAttributes.Normal </param>
        /// <returns></returns>
        public static void FileDelete2(string pPath, bool pIsSetNormal)
        {
            try
            {
                if (File.Exists(pPath))//20081022 PL Add test File.Exists()
                {
                    if (pIsSetNormal)
                        File.SetAttributes(pPath, FileAttributes.Normal);
                    File.Delete(pPath);
                }
            }
            catch (FileNotFoundException)
            {
            }
        }

        /// <summary>
        /// Copie un fichier existant vers un autre fichier 
        /// </summary>
        /// <param name="pPath"></param>
        /// <param name="pPath2"></param>
        /// <returns>FILENOTFOUND si le fichier n'existe pas</returns>
        public static ErrLevel FileCopy2(string pPath, ref string pPath2)
        {
            ErrLevel ret = ErrLevel.SUCCESS;
            try
            {
                pPath2 = GetNotExistingFileName(pPath2);
                File.Copy(pPath, pPath2, true);
            }
            catch (FileNotFoundException)
            {
                ret = ErrLevel.FILENOTFOUND;
            }
            return ret;
        }

        // PM 20200601 [XXXXX] Déplacé dans la classe SystemIOTools (ACommon/SystemIO.cs) 
        ///// <summary>
        /////  Création du folder s'il n'existe pas 
        ///// </summary>
        ///// <param name="pPath"></param>
        //public static void CreateDirectory(string pPath)
        //{
        //    bool isExist = Directory.Exists(pPath);
        //    if (false == isExist)
        //        Directory.CreateDirectory(pPath);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPath"></param>
        public static void CleanDirectory(string pPath)
        {
            CleanDirectory(pPath, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPath"></param>
        /// <param name="pIsDeleteSubDirectories"></param>
        public static void CleanDirectory(string pPath, bool pIsDeleteSubDirectories)
        {
            if (Directory.Exists(pPath))
            {
                DirectoryInfo pathInfo = new DirectoryInfo(pPath);
                //
                FileInfo[] fileInfos = pathInfo.GetFiles();
                foreach (FileInfo fileInfo in fileInfos)
                    fileInfo.Delete();
                //
                if (pIsDeleteSubDirectories)
                {
                    DirectoryInfo[] directoryInfos = pathInfo.GetDirectories();
                    foreach (DirectoryInfo directoryInfo in directoryInfos)
                        directoryInfo.Delete(true);
                }
            }
        }

        /// <summary>
        /// Lecture du fichier {pPath}
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        /// EG 20180423 Analyse du code Correction [CA2202]
        /// FI 20200608 [XXXXX] using syntax
        public static byte[] ReadFileToBytes(string pPath)
        {
            using (FileStream fsFile = new FileStream(pPath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader brFile = new BinaryReader(fsFile))
                {
                    byte[] ret = brFile.ReadBytes((int)fsFile.Length);
                    return ret;
                }
            }
        }

        /// <summary>
        /// Tri {pFileInfo} (Utilisation de IFileSystemCompareByName pour la comparaison)
        /// </summary>
        /// <param name="_aFileInfo"></param>
        /// <returns></returns>
        public static FileInfo[] SortFilesByName(FileInfo[] pFileInfo)
        {
            FileInfo[] ret = pFileInfo;
            if (ArrFunc.IsFilled(ret))
            {
                IComparer nameComparer = new IFileSystemCompareByName();
                Array.Sort(ret, nameComparer);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_aDirectoryInfo"></param>
        /// <returns></returns>
        public static DirectoryInfo[] SortDirectoriesByName(DirectoryInfo[] _aDirectoryInfo)
        {
            DirectoryInfo[] aInfos = _aDirectoryInfo;
            IComparer nameComparer = new IFileSystemCompareByName();
            if (ArrFunc.IsFilled(aInfos))
                Array.Sort(aInfos, nameComparer);
            return aInfos;
        }

        /// <summary>
        /// Ecrire d'un fichier
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pPath"></param>
        /// <param name="pMode"></param>
        /// <returns>FAILURE si le fichier existe déjà (uniquement lorsque <paramref name="pMode"/> vaut <see cref="WriteFileOverrideMode.None"/>) </returns>
        /// FI 20230209 [XXXXX] Nouvelle signature
        public static Cst.ErrLevel WriteBytesToFile(byte[] pData, string pPath, WriteFileOverrideMode pMode)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (File.Exists(pPath))
            {
                switch (pMode)
                {
                    case WriteFileOverrideMode.Override:
                        if ((File.GetAttributes(pPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            FileDelete2(pPath, true);
                        break;
                    case WriteFileOverrideMode.New:
                        pPath = GetNextFileName(pPath);
                        break;
                    case WriteFileOverrideMode.None:
                        ret = Cst.ErrLevel.FAILURE;
                        break;
                    default:
                        throw new NotSupportedException($"Mode: {pMode} not supported");
                }
            }

            if ((ret != Cst.ErrLevel.FAILURE) && (pData != null))
            {
                using (FileStream fsFile = File.Create(pPath))
                {
                    fsFile.Write(pData, 0, pData.Length);
                }
                ret = Cst.ErrLevel.SUCCESS;
            }

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// FI 20230209 [XXXXX] Add Method
        private static string GetNextFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            int i = 0;
            while (File.Exists(fileName))
            {
                if (i == 0)
                    fileName = fileName.Replace(extension, $"({++i}){extension}");
                else
                    fileName = fileName.Replace($"({i}){extension}", $"({++i}){extension}");
            }

            return fileName;
        }

        /// <summary>
        ///  Ecriture d'une d'une string dans un fichier 'Si le fichier existe déjà il est écrasé)
        ///  <para>Le fichier est encodé en UTF-8</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pPath"></param>
        // EG 20180423 Analyse du code Correction [CA2202]
        public static void WriteStringToFile(string pData, string pPath)
        {
            using (FileStream fs = new FileStream(pPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(pData);
                    sw.Flush();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObj"></param>
        private static void ScaleImages(object pObj)
        {
            #region Jpeg
            Jpeg img = null;
            try
            {
                img = (Jpeg)pObj;
            }
            catch { }
            #endregion
            //
            if (null != img)
                img.ScaleToFit(img.Width * 0.75F, img.Height * 0.75F);
            else
            {
                #region ImgRaw
                iTextSharp.text.ImgRaw imgRaw = null;
                try
                {
                    imgRaw = (iTextSharp.text.ImgRaw)pObj;
                }
                catch { }
                #endregion
                //
                if (null != imgRaw)
                    imgRaw.ScaleToFit(imgRaw.Width * 0.75F, imgRaw.Height * 0.75F);
                else
                {
                    #region Rectangle
                    iTextSharp.text.pdf.PdfRectangle rectangle = null;
                    try
                    {
                        rectangle = (iTextSharp.text.pdf.PdfRectangle)pObj;
                    }
                    catch { }
                    //
                    if (rectangle != null)
                    {
                        //if (null != rectangle.Rectangle.Chunks[0].CompositeElements)
                        //{
                        //    for (int k = 0; k < rectangle.CompositeElements.Count; k++)
                        //        ScaleImages(rectangle.CompositeElements[k]);
                        //}
                        //else
                        ScaleImages(rectangle);
                    }
                    #endregion
                    #region Table
                    iTextSharp.text.pdf.PdfPTable table = null;
                    try
                    {
                        table = (iTextSharp.text.pdf.PdfPTable)pObj;
                    }
                    catch { }
                    //
                    if (table != null)
                    {
                        if (null != table.Rows)
                        {
                            for (int iRow = 0; iRow < table.Rows.Count; iRow++)
                            {
                                iTextSharp.text.pdf.PdfPRow row = (iTextSharp.text.pdf.PdfPRow)table.Rows[iRow];
                                //
                                if (null != row && null != row.Cells)
                                {
                                    for (int iCell = 0; iCell < row.Cells.Length; iCell++)
                                    {
                                        iTextSharp.text.pdf.PdfPCell cell = (iTextSharp.text.pdf.PdfPCell)row.Cells[iCell];
                                        //
                                        if (null != cell)
                                        {
                                            if (null != cell.CompositeElements)
                                            {
                                                for (int k = 0; k < cell.CompositeElements.Count; k++)
                                                    ScaleImages(cell.CompositeElements[k]);
                                            }
                                            else
                                                ScaleImages(cell);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHTMLData"></param>
        /// <param name="pPDFPath"></param>
        public static void WriteHTMLToPDF(string pHTMLData, string pPDFPath)
        {

            #region declarations
            Document documentPDF = new Document(PageSize.A4, 26F, 25.95F, 18F, 18F);
            //
            string titlePDF = string.Empty;
            HeaderFooter footerPDF = null;
            Font footerHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            int footerHeaderAlign = 0;
            iTextSharp.text.Color footerHeaderBorderColor = new iTextSharp.text.Color(System.Drawing.Color.SteelBlue);
            #endregion
            //
            #region Get Title
            XmlDocument docHTML = new XmlDocument();
            docHTML.LoadXml(pHTMLData);
            //
            XmlNode nodeHead = docHTML.SelectSingleNode("html/head");
            if (null != nodeHead)
            {
                XmlNode nodeTitle = nodeHead.SelectSingleNode("title");
                if (null != nodeTitle)
                    titlePDF = nodeTitle.InnerText;
            }
            #endregion
            //
            try
            {
                Encoding dataEncoding = Encoding.UTF8;
                //
                string encodedData = dataEncoding.GetString(dataEncoding.GetBytes(pHTMLData));

                ArrayList objects;
                using (StringReader documentStream = new StringReader(encodedData))
                {
                    objects = iTextSharp.text.html.simpleparser.HTMLWorker.ParseToList(documentStream, null);
                }
                //
                //Faire une première Passe:
                // 1 - pour corriger les dimensions et la position du Logo
                // 2 - et récupérer le Footer
                //
                #region Pass 1
                object obj = objects[0];
                objects.Remove(obj);
                //
                for (int k = 0; k < objects.Count; k++)
                {
                    obj = objects[k];
                    //
                    #region Logo
                    ScaleImages(obj);
                    #endregion
                    //
                    #region Document Footer
                    if (k + 1 == objects.Count)
                    {
                        iTextSharp.text.pdf.PdfPTable lastTable = null;
                        //
                        try
                        {
                            lastTable = (iTextSharp.text.pdf.PdfPTable)obj;
                            //
                            iTextSharp.text.pdf.PdfPRow lastRow = null;
                            iTextSharp.text.pdf.PdfPCell lastCell = null;
                            iTextSharp.text.pdf.PdfPTable footerTable = null;
                            //
                            if (null != lastTable.Rows)
                                lastRow = lastTable.GetRow(lastTable.Rows.Count - 1);
                            //
                            if (null != lastRow.Cells)
                                lastCell = lastRow.Cells[lastRow.Cells.Length - 1];
                            //
                            if (null != lastCell.CompositeElements)
                                footerTable = (iTextSharp.text.pdf.PdfPTable)lastCell.CompositeElements[0];
                            //
                            iTextSharp.text.Paragraph footerPara = null;
                            //
                            if (null != footerTable && null != footerTable.Rows)
                            {
                                #region footerTable
                                footerPara = new Paragraph();
                                //
                                for (int iRow = 0; iRow < footerTable.Rows.Count; iRow++)
                                {
                                    iTextSharp.text.pdf.PdfPRow footerRow = (iTextSharp.text.pdf.PdfPRow)footerTable.Rows[iRow];
                                    //
                                    if (null != footerRow.Cells)
                                    {
                                        for (int iCell = 0; iCell < footerRow.Cells.Length; iCell++)
                                        {
                                            iTextSharp.text.pdf.PdfPCell footerCell = (iTextSharp.text.pdf.PdfPCell)footerRow.Cells[iCell];
                                            //
                                            if (iRow == 0 && iCell == 0)
                                                footerHeaderAlign = footerCell.Column.Alignment;
                                            //
                                            if (null != footerCell.CompositeElements)
                                            {
                                                foreach (iTextSharp.text.Paragraph para in footerCell.CompositeElements)
                                                {
                                                    footerPara.Add(para);
                                                    footerPara.Alignment = footerCell.Column.Alignment;
                                                }
                                            }
                                        }
                                    }
                                }
                                //
                                lastTable.DeleteRow(lastTable.Rows.Count - 1);
                                #endregion
                            }
                            //
                            if (null != footerPara)
                            {
                                footerPDF = new HeaderFooter(footerPara, false)
                                {
                                    Alignment = footerHeaderAlign,
                                    Border = Rectangle.TOP_BORDER,
                                    BorderWidthTop = 1.5F,
                                    BorderColorTop = footerHeaderBorderColor
                                };


                                if (footerPara.Chunks[0] is Chunk textFooter)
                                {
                                    footerHeaderFont = textFooter.Font;
                                    footerHeaderAlign = footerPara.Alignment;
                                }
                            }
                        }
                        catch
                        { }
                    }
                    #endregion
                }
                #endregion
                using (FileStream fs = new FileStream(pPDFPath, FileMode.Create))
                {
                    PdfWriter.GetInstance(documentPDF, fs);
                }
                //
                #region Préparer le document
                documentPDF.AddTitle(titlePDF);
                documentPDF.AddCreationDate();
                //
                documentPDF.Footer = footerPDF;
                //
                documentPDF.Open();
                documentPDF.NewPage();
                //
                #region Header
                HeaderFooter headerPDF = new HeaderFooter(new Phrase("Page : ", footerHeaderFont), true);
                headerPDF.SetAlignment("right");
                headerPDF.Border = Rectangle.BOTTOM_BORDER;
                headerPDF.BorderWidthBottom = 1.5F;
                headerPDF.BorderColorBottom = footerHeaderBorderColor;
                documentPDF.Header = headerPDF;
                #endregion
                #endregion
                //
                // La deuxième Passe pour écrire dans le document les différents objets
                //
                #region Pass 2
                for (int k = 0; k < objects.Count; k++)
                {
                    obj = objects[k];
                    //
                    documentPDF.Add((IElement)obj);
                }
                #endregion
            }
            finally { documentPDF.Close(); }

        }

        private static string GetSQLWhereAndParametersForGetFile(string pCS, string pSoftware, string pCategory, string pFileType, string pFolderName, ref DataParameters pDataParameters)
        {
            string fileName = null;
            string fileExtension = null;
            return GetSQLWhereAndParametersForGetFile(pCS, pSoftware, pCategory, pFileType, pFolderName, fileName, fileExtension, ref pDataParameters);
        }

        private static string GetSQLWhereAndParametersForGetFile(string pCS, string pSoftware, string pCategory, string pFileType, string pFolderName, string pFileName, string pFileExtension,
            ref DataParameters pDataParameters)
        {
            //PL TODO Voir pour utiliser ou supprimer la colonne SOFTWARE
            if (DataHelper.IsDbOracle(pCS))
                pSoftware = " ";
            else
                pSoftware = string.Empty;

            string sqlWhere = SQLCst.WHERE + "SOFTWARE=@SOFTWARE";
            sqlWhere += SQLCst.AND + "CATEGORY=@CATEGORY";
            sqlWhere += SQLCst.AND + "FILETYPE=@FILETYPE";
            sqlWhere += SQLCst.AND + "FOLDERNAME like @FOLDERNAME";
            if (pFileName != null)
                sqlWhere += SQLCst.AND + "FILENAME=@FILENAME";
            if (pFileExtension != null)
                sqlWhere += SQLCst.AND + "FILEEXTENSION=@FILEEXTENSION";
            pDataParameters = new DataParameters();
            pDataParameters.Add(new DataParameter(pCS, "SOFTWARE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pSoftware);
            pDataParameters.Add(new DataParameter(pCS, "CATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pCategory);
            pDataParameters.Add(new DataParameter(pCS, "FILETYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pFileType);
            pDataParameters.Add(new DataParameter(pCS, "FOLDERNAME", DbType.AnsiString, SQLCst.UT_UNC_LEN), pFolderName);
            if (pFileName != null)
                pDataParameters.Add(new DataParameter(pCS, "FILENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN), pFileName);
            if (pFileExtension != null)
                pDataParameters.Add(new DataParameter(pCS, "FILEEXTENSION", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pFileExtension);
            return sqlWhere;
        }

        private static bool SaveToDBForGetFile(string pCS, string pSoftware, string pCategory, string pFileType, string pFolderName, string pFileName, string pFileExtension, string pFullPathName)
        {
            bool ret;
            try
            {
                //byte[] fileContents = FileTools.ReadFileToBytes(pFullPathName);
                //string contents = StrFunc.UTF8ByteArrayToString(fileContents);
                //
                XmlDocument xmlDoc = new XmlDocument
                {
                    //20090729 FI 16643 pour que <xsl:text> </xsl:text> ne se transforme pas en <xsl:text/>
                    PreserveWhitespace = true
                };
                xmlDoc.Load(pFullPathName);
                XMLTools.RemoveXmlDeclaration(xmlDoc);
                string contents = xmlDoc.InnerXml;
                xmlDoc = null;

                DataParameters parameters = new DataParameters();

                string sqlWhere = GetSQLWhereAndParametersForGetFile(pCS, pSoftware, pCategory, pFileType,
                                        pFolderName, pFileName, pFileExtension,
                                        ref parameters);
                #region SQL Delete
                string sqlDelete = SQLCst.DELETE_DBO + "FILECONFIG" + Cst.CrLf;
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlDelete + sqlWhere, parameters.GetArrayDbParameter());
                #endregion
                #region SQL Insert
                //string sqlInsert = SQLCst.INSERT_INTO_DBO + "FILECONFIG";
                //sqlInsert += "(SOFTWARE,CATEGORY,FILETYPE,FOLDERNAME,FILENAME,FILEEXTENSION,FILECONTENTS,DTINS)";
                //sqlInsert += " values ";
                //sqlInsert += "(@SOFTWARE,@CATEGORY,@FILETYPE,@FOLDERNAME,@FILENAME,@FILEEXTENSION,@FILECONTENTS," + DataHelper.SQLGetDate(pCS) + ")";
                ////glop parameters.Add(new DataParameter(pCS, "FILECONTENTS", DbType.Xml, contents.Length), xslDoc);
                //if (ConnectionStringCache.GetConnectionStringState(pCS) == ConnectionStringCacheState.isOracle)
                //{
                //    System.Data.SqlTypes.SqlXml sqlXml = new System.Data.SqlTypes.SqlXml(new XmlTextReader(pFullPathName));
                //    parameters.Add(new DataParameter(pCS, "FILECONTENTS", DbType.Xml), sqlXml);
                //    //parameters.Add(new DataParameter(pCS, "FILECONTENTS", DbType.Guid), contents);
                //    //parameters.Add(new DataParameter(pCS, "FILECONTENTS", DbType.Xml), xmlDoc);
                //    //parameters.Add(new DataParameter(pCS, "FILECONTENTS", DbType.AnsiString, contents.Length), contents);
                //}
                //else
                //    parameters.Add(new DataParameter(pCS, "FILECONTENTS", DbType.AnsiString, contents.Length), contents);
                //int nRows = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlInsert, parameters.GetArrayDbParameter());
                //
                string sqlSelect = SQLCst.SELECT + "SOFTWARE,CATEGORY,FILETYPE,FOLDERNAME,FILENAME,FILEEXTENSION,FILECONTENTS,DTINS" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + "FILECONFIG" + Cst.CrLf;
                DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, sqlSelect + sqlWhere, parameters.GetArrayDbParameter());
                DataTable dt = ds.Tables[0];
                //
                DataRow row = dt.NewRow();
                //DataParameter[] arrayParameters = parameters.GetArrayParameter();
                row.BeginEdit();
                //PL TODO Voir pour utiliser ou supprimer la colonne SOFTWARE
                if (DataHelper.IsDbOracle(pCS))
                    pSoftware = " ";
                else
                    pSoftware = string.Empty;
                row["SOFTWARE"] = pSoftware;
                row["CATEGORY"] = pCategory;
                row["FILETYPE"] = pFileType;
                row["FOLDERNAME"] = pFolderName;
                row["FILENAME"] = pFileName;
                row["FILEEXTENSION"] = pFileExtension;
                row["FILECONTENTS"] = contents;
                // FI 20200820 [25468] Dates systèmes en UTC
                row["DTINS"] = OTCmlHelper.GetDateSysUTC(pCS);
                row.EndEdit();
                dt.Rows.Add(row);
                //
                int nRows = DataHelper.ExecuteDataAdapter(pCS, sqlSelect, dt);
                #endregion
                ret = (nRows == 1);
            }
            catch (Exception)
            {
                throw;
            }
            return ret;
        }

        public static bool GetFile3(string pCS, string pRootPath, string pFileType, string pFolder, string pObjectName, ref string opFilePath)
        {
            string subFolder = null;
            bool isCallRecursive = false;
            return GetFile3(pCS, pRootPath, pFileType, pFolder, subFolder, pObjectName, isCallRecursive, ref opFilePath);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool GetFile3(string pCS, string pRootPath, string pFileType, string pFolder, string pSubFolder, string pObjectName, ref string opFilePath)
        {
            return GetFile3(pCS, null, pRootPath, pFileType, pFolder, pSubFolder, pObjectName, ref opFilePath);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool GetFile3(string pCS, IDbTransaction pDbTransaction, string pRootPath, string pFileType, string pFolder, string pSubFolder, string pObjectName, ref string opFilePath)
        {
            bool isCallRecursive = false;
            return GetFile3(pCS, pDbTransaction, pRootPath, pFileType, pFolder, pSubFolder, pObjectName, isCallRecursive, ref opFilePath);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool GetFile3(string pCS, string pRootPath, string pFileType, string pFolder, string pSubFolder, string pObjectName, bool pIsCallRecursive, ref string opFilePath)
        {
            return GetFile3(pCS, null, pRootPath, pFileType, pFolder, pSubFolder, pObjectName, pIsCallRecursive, ref opFilePath);
        }
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Upd Datatable instead of DataReader
        // EG 20210916 [XXXXX] Si SoftwarePortal alors suppression du de "CustomerPortal/Page dans pRootPath
        public static bool GetFile3(string pCS, IDbTransaction pDbTransaction, string pRootPath, string pFileType, string pFolder, string pSubFolder, string pObjectName, bool pIsCallRecursive, ref string opFilePath)
        {

            #region Variables
            bool isXSL_Files = false;

            if (Software.IsSoftwarePortal())
                pRootPath = pRootPath.Replace(@"\CustomerPortal\Page", string.Empty);

            CSManager csManager = new CSManager(pCS);
            string serverName = csManager.GetSvrName();
            // FI 20210416 [XXXXX] usage de serverName.Replace(@"\", ".") pour correctement géré les Instance sql Server (Exemple : SVR-DB01\SQL2019 )
            // FI 20210715 [XXXXX] Appel à FileTools.ReplaceFilenameInvalidChar
            string pathDB = FileTools.ReplaceFilenameInvalidChar("SVR-" + (serverName == "." ? "(Local)" : serverName) + $".DB-{csManager.GetDbName()}", ".");
            string software = string.Empty;
            string filePath = pFolder;
            if (StrFunc.IsFilled(pSubFolder))
                filePath += @"\" + pSubFolder;
            string category = pFolder;

            string fileExtension;
            string fileType;
            #endregion

            #region FileType
            switch (pFileType)
            {
                case "XSL":
                    isXSL_Files = true;
                    fileType = "XSL_Files"; //Deprecated PL 20170123 New GetFile3
                    fileExtension = ".xsl";
                    break;
                case "XSLT":
                    isXSL_Files = true;
                    fileType = "XSL_Files"; //Deprecated PL 20170123 New GetFile3
                    fileExtension = ".xslt";
                    break;
                case "XML":
                default:
                    _ = (pFileType == "XML");
                    fileType = "XML_Files"; //Deprecated PL 20170123 New GetFile3
                    fileExtension = ".xml";
                    break;
            }

            #endregion
            #region isCustomFile
            if (isXSL_Files)
            {
                if (pFolder.ToLower() == "message")
                {
                    //------------------------------------------------------------------------------------------------------------------------------------------------
                    //Messagerie - Notification/Règlement
                    //------------------------------------------------------------------------------------------------------------------------------------------------
                    //Dès lors que le nom du fichier ne matche pas avec une des valeurs ci-dessous on considère celui-ci en tant que fichier CUSTOM (donc appartenant au client).
                    //NB: on remarquera que tout fichier se terminant par REPORT est considéré comme NON CUSTOM !
                    //------------------------------------------------------------------------------------------------------------------------------------------------
                    string objectNameUpper = pObjectName.ToUpper();
                    _ = (!objectNameUpper.EndsWith("REPORT"))
                        && (!objectNameUpper.EndsWith("_ISDA_HTML")) && (!objectNameUpper.EndsWith("_ISMA_HTML")) && (!objectNameUpper.EndsWith("_ICMA_HTML"))
                        && (!objectNameUpper.EndsWith("_ISDA_PDF")) && (!objectNameUpper.EndsWith("_ISMA_PDF")) && (!objectNameUpper.EndsWith("_ICMA_PDF"))
                        && (objectNameUpper != "MT202_SWIFT") && (objectNameUpper != "MT210_SWIFT")
                        && (objectNameUpper != "MT300_SWIFT") && (objectNameUpper != "MT305_SWIFT") && (objectNameUpper != "MT360_SWIFT");
                }
                //PL 20170123 New GetFile3
                //else if ((pFolder.ToLower() == "input") || (pFolder.ToLower() == "output"))
                else if ((pFolder.ToLower() == "ioinput") || (pFolder.ToLower() == "iooutput"))
                {
                    //------------------------------------------------------------------------------------------------------------------------------------------------
                    //Spheres IO - Importation/Exportation
                    //------------------------------------------------------------------------------------------------------------------------------------------------
                    //Pour rappel, les fichiers XSL utiles à SIO sont livrés à titre d'exemple. Ils peuvent donc faire l'objet d'une CUSTOMISATION de la part des clients.
                    //Bien que déconseillé, lors de la customisation les clients peuvent opter pour conserver le nom d'origine du fichier mis à disposition par EFS.  
                    //On considère par défaut tout fichier XSL utile à SIO comme CUSTOM, c'est pourquoi on privilégie ici une lecture de la table FILECONFIG 
                    //à la recherche d'un éventuel fichier CUSTOM.
                    //NB: exception est faite pour les fichiers relatifs à la gestion de données externes (IOCompare)... PL 20141124 je ne sais plus pourquoi cetet exception !
                    //------------------------------------------------------------------------------------------------------------------------------------------------
                    if (!pSubFolder.ToLower().StartsWith("extldata"))
                        _ = true;
                }
            }
            #endregion

            #region [Step 1] Search: ".Release" files or ".Debug" files
            string objectName = pObjectName;
            #region Search ".Release" file from Temporary\{DB} folder
            string fileName = objectName + ".Release" + fileExtension;
            //PL 20170123 New GetFile3
            //getFileName = pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\" + filePath + @"\" + fileName;
            string getFileName = pRootPath + @"\Temporary\" + pathDB + @"\" + filePath + @"\" + fileName;
            bool isFound = File.Exists(getFileName);
            #endregion
            if (isFound)
            {

                #region Save ".Release" file to Database and Rename
                try
                {
                    //Fichier Release --> Save in DB
                    string newFileName = objectName;
                    //PL 20170123 New GetFile3
                    if (SaveToDBForGetFile(pCS, software, category, fileType, filePath, newFileName, fileExtension.Substring(1), getFileName))
                    {
                        //Rename file 
                        FileRenameToDotBak(getFileName);
                        //If necessary, delete "no release" existing file
                        FileDelete2(getFileName.Replace(".Release" + fileExtension, fileExtension));
                        //
                        isFound = false; //Afin de lire plus bas depuis la DB les données qui viennet d'y être insérées et ainsi extraire d'éventuel autres fichiers (ex.: #include)
                        //
                        #region If Exist in subfolder other files ".Release", save all and rename
                        //PL 20170123 New GetFile3 
                        //DirectoryInfo dir = new DirectoryInfo(pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\" + filePath);
                        DirectoryInfo dir = new DirectoryInfo(pRootPath + @"\Temporary\" + pathDB + @"\" + filePath);
                        FileInfo[] infos = dir.GetFiles(@"*.Release" + fileExtension, SearchOption.AllDirectories);
                        if (infos != null)
                        {
                            try
                            {
                                newFileName = null;
                                foreach (FileInfo info in infos)
                                {
                                    newFileName = info.Name.Replace(".Release" + fileExtension, string.Empty);
                                    string newFolderName = info.FullName;
                                    newFolderName = newFolderName.Replace(info.Name, string.Empty);
                                    //PL 20170123 New GetFile3 
                                    //newFolderName = newFolderName.Replace(pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\", string.Empty);
                                    newFolderName = newFolderName.Replace(pRootPath + @"\Temporary\" + pathDB + @"\", string.Empty);
                                    newFolderName = newFolderName.Remove(newFolderName.Length - 1, 1);
                                    //PL 20170123 New GetFile3
                                    if (SaveToDBForGetFile(pCS, software, category, fileType, newFolderName, newFileName, fileExtension.Substring(1), info.FullName))
                                    {
                                        //Rename file 
                                        FileRenameToDotBak(info.FullName);
                                        //If necessary, delete "no release" existing file
                                        FileDelete2(info.FullName.Replace(".Release" + fileExtension, fileExtension));
                                    }
                                    else
                                    {
                                        throw new Exception("Impossibility to save the file: " + info.Name);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Impossibility to save the file: " + (newFileName ?? @"*.Release") + fileExtension + Cst.CrLf2 + ex.Message);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        isFound = false;
                        throw new Exception("Impossibility to save the file: " + fileName);
                    }
                }
                catch (Exception ex)
                {
                    // FI 20200910 [XXXXX] Alimentation de inner exception
                    throw new Exception($"Impossibility to save the file: {fileName}", ex);
                }
                #endregion
            }
            else
            {
                #region Search ".Debug" file from Temporary\{DB} folder
                fileName = objectName + ".Debug" + fileExtension;
                //PL 20170123 New GetFile3 
                //getFileName = pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\" + filePath + @"\" + fileName;
                getFileName = pRootPath + @"\Temporary\" + pathDB + @"\" + filePath + @"\" + fileName;
                isFound = File.Exists(getFileName);
                #endregion
                #region Search file from Temporary\{DB} folder
                if (!isFound)
                {
                    fileName = objectName + fileExtension;
                    //PL 20170123 New GetFile3 
                    //getFileName = pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\" + filePath + @"\" + fileName;
                    getFileName = pRootPath + @"\Temporary\" + pathDB + @"\" + filePath + @"\" + fileName;
                    isFound = File.Exists(getFileName);
                    if (isFound)
                    {
                        //Si fichier vieux de plus de 10mn on le réextrait... (isFound=false)
                        if (FileTools.IsOutdatedFile(getFileName, -1))
                            isFound = false;
                    }
                }
                #endregion
            }
            #endregion [Step 1] Search: ".Release" files or ".Debug" files

            #region [Step 2] Search: Database
            if (!isFound)
            {
                #region Search file from FILECONFIG table
                try
                {
                    fileName = objectName;
                    #region SQL Select
                    DataParameters parameters = new DataParameters();
                    string sqlSelect = SQLCst.SELECT + "IDFILECONFIG, FILECONTENTS" + Cst.CrLf;
                    sqlSelect += SQLCst.FROM_DBO + "FILECONFIG" + Cst.CrLf;
                    //PL 20170123 New GetFile3
                    sqlSelect += GetSQLWhereAndParametersForGetFile(pCS, software, category, fileType, filePath, fileName, fileExtension.Substring(1),
                                                    ref parameters);
                    sqlSelect += SQLCst.AND + "ISENABLED=1";
                    #endregion
                    byte[] fileContents = null;

                    // RD 20140127 [19530] Add loop and try cach section
                    int countLoop = 0;
                    bool isLoop = false;
                    do
                    {
                        int idFileConfig = 0;
                        string contents = string.Empty;
                        DataTable dt1 = DataHelper.ExecuteDataTable(pCS, pDbTransaction, sqlSelect, parameters.GetArrayDbParameter());
                        if ((null != dt1) && (0 < dt1.Rows.Count))
                        {
                            #region Cas particulier pour MESSAGE et BUILDIDENTIFIER
                            string sourceDir, destDir;
                            if (category == "Message")
                            {
                                //Si Category "Message", on copie alors tous les xslts messages de OTCml, ceci au cas où le fichier XSL en question y ferait référence.
                                try
                                {
                                    sourceDir = pRootPath + @"Message";
                                    destDir = pRootPath + @"\Temporary\" + pathDB;
                                    CopyDir(sourceDir, destDir);
                                }
                                catch (Exception) { }
                            }
                            if ((category == "Message") || (category == "BuildIdentifier"))
                            {
                                try
                                {
                                    sourceDir = pRootPath + @"\Library";
                                    destDir = pRootPath + @"\Temporary\" + pathDB;
                                    CopyDir(sourceDir, destDir);
                                }
                                catch (Exception) { }
                                try
                                {
                                    sourceDir = pRootPath + @"\Resource";
                                    destDir = pRootPath + @"\Temporary\" + pathDB;
                                    CopyDir(sourceDir, destDir);
                                }
                                catch (Exception) { }
                            }
                            #endregion Cas particulier pour MESSAGE et BUILDIDENTIFIER

                            idFileConfig = Convert.ToInt32(dt1.Rows[0]["IDFILECONFIG"]);
                            contents = dt1.Rows[0]["FILECONTENTS"].ToString();

                        }

                        if (idFileConfig != 0)
                        {
                            fileContents = StrFunc.StringToUTF8ByteArray(contents);
                            //Extract on Temporary\{DB}
                            fileName += fileExtension;
                            //PL 20170123 New GetFile3 
                            //getFileName = pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\" + filePath + @"\" + fileName;
                            //FileTools.CreateDirectory(pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\" + filePath);
                            getFileName = pRootPath + @"\Temporary\" + pathDB + @"\" + filePath + @"\" + fileName;
                            SystemIOTools.CreateDirectory(pRootPath + @"\Temporary\" + pathDB + @"\" + filePath);

                            try
                            {
                                if (FileTools.WriteBytesToFile(fileContents, getFileName, FileTools.WriteFileOverrideMode.Override) == Cst.ErrLevel.SUCCESS)
                                {
                                    isFound = true;

                                    #region Extract all other files from Database
                                    parameters = new DataParameters();
                                    sqlSelect = SQLCst.SELECT + "FOLDERNAME, FILENAME, FILEEXTENSION, FILECONTENTS" + Cst.CrLf;
                                    sqlSelect += SQLCst.FROM_DBO + "FILECONFIG" + Cst.CrLf;
                                    //PL 20170123 New GetFile3
                                    sqlSelect += GetSQLWhereAndParametersForGetFile(pCS, software, category, fileType, filePath + "%",
                                                                    ref parameters);
                                    sqlSelect += SQLCst.AND + "ISENABLED=1";
                                    sqlSelect += SQLCst.AND + "IDFILECONFIG!=" + idFileConfig.ToString();

                                    DataTable dt2 = DataHelper.ExecuteDataTable(pCS, pDbTransaction, sqlSelect, parameters.GetArrayDbParameter());
                                    if (null != dt2)
                                    {
                                        foreach (DataRow row in dt2.Rows)
                                        {
                                            contents = row["FILECONTENTS"].ToString();
                                            fileContents = StrFunc.StringToUTF8ByteArray(contents);
                                            //Extract on Temporary\{DB}
                                            string folderName = row["FOLDERNAME"].ToString();
                                            //PL 20170123 New GetFile3 
                                            //string fullPathName = pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\"
                                            string fullPathName = pRootPath + @"\Temporary\" + pathDB + @"\"
                                                + folderName + @"\"
                                                + row["FILENAME"].ToString() + "." + row["FILEEXTENSION"].ToString();

                                            //PL 20170123 New GetFile3 
                                            //FileTools.CreateDirectory(pPath + @"\Temporary\" + sourceDB + @"\Common\" + fileType + @"\" + folderName);
                                            SystemIOTools.CreateDirectory(pRootPath + @"\Temporary\" + pathDB + @"\" + folderName);
                                            FileTools.WriteBytesToFile(fileContents, fullPathName, FileTools.WriteFileOverrideMode.Override);
                                        }
                                    }

                                    //FI 20190612 [XXXXX] le fichier n'est pas bloqué par un autre process => pas besoin de boucler 
                                    isLoop = false;
                                    #endregion
                                }

                                //FI 20190612 [XXXXX] le fichier n'est pas bloqué par un autre process => pas besoin de boucler 
                                isLoop = false;
                            }
                            catch (Exception ex)
                            {
                                if (FileTools.IsFileUsedException(ex))
                                {
                                    //Pause de 5 sec.
                                    Thread.Sleep(FileTools.ThreadSleepBeforLoop);

                                    if (FileTools.IsOutdatedFile(getFileName, -1))
                                    {
                                        isFound = false;
                                        // On retente le cycle complet de mise à jour depuis la table FILECONFIG, au maximum 10 fois, au delà on remonte l'erreur.
                                        isLoop = true;
                                        countLoop++;
                                        if (countLoop > FileTools.MaxLoopPass)
                                            throw;
                                    }
                                    else
                                    {
                                        // On utilise le fichier disponible qui vient a priori d’être mis par un autre processus.
                                        isFound = true;
                                        //FI 20190617 [XXXXX] On utilise le fichier disponible => pas besoin de boucler 
                                        isLoop = false;
                                    }
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    } while (isLoop);
                }
                catch (Exception) { }
                #endregion
            }
            #endregion [Step 2] Search: Database

            #region [Step 3] Search: "Spheres files" or "Old Specific files"
            if ((!isFound) && (!pIsCallRecursive))
            {
                //List<string> softwareFolder = SoftwareFolder();
                List<String> softwareFolder = new List<string>
                {
                    string.Empty
                };
                if (Software.IsSoftwarePortal())
                    softwareFolder.Add("CustomerPortal"); //PL 20170303 Add

                for (int i = 0; i < ArrFunc.Count(softwareFolder); i++)
                {
                    software = softwareFolder[i];
                    if (StrFunc.IsFilled(software))
                        software += @"\";

                    #region Search (without prefix) from XML_Files (or XSL_Files) folder (for compatibility asc)
                    //Pour "fichiers Spheres" ou pour compatibilité ascendante avec "vieux fichiers spécifiques"
                    fileName = pObjectName + fileExtension;
                    //PL 20170123 New GetFile3 
                    //getFileName = pPath + @"\" + software + fileType + @"\" + filePath + @"\" + fileName;
                    getFileName = pRootPath + @"\" + software + filePath + @"\" + fileName;
                    isFound = File.Exists(getFileName);
                    #endregion
                    //
                    //PL 20170123 New GetFile3 
                    //if ((!isFound) && isXML_Files)
                    //{
                    //    #region Search (XMLReferential prefix) from old XML_Files folder (for compatibility asc)
                    //    //Pour compatibilité ascendante "anciens fichiers Spheres" ou pour avec "anciens fichiers spécifiques"
                    //    fileName = @"XMLReferential" + pObjectName + fileExtension;
                    //    getFileName = pPath + @"\" + software + fileType + @"\" + filePath + @"\" + fileName;
                    //    isFound = File.Exists(getFileName);
                    //    #endregion
                    //}
                    //
                    if (isFound)
                    {
                        //PL 20170123 New GetFile3 
                        ////******************************************************
                        ////NB: Code temporaire à supprimer ultérieurement... (PL)
                        ////******************************************************
                        ////Si fichier spécifique --> Rename (remove "XMLReferential" prefix and add ".Custom" suffix) and Save in DB
                        //if (isCustomFile || fileName.StartsWith("XMLReferential"))
                        //{
                        //    #region Save to Database
                        //    string newFileName = null;
                        //    try
                        //    {
                        //        newFileName = pObjectName;
                        //        if (SaveToDBForGetFile(pCS, software, category, fileType, filePath, newFileName, fileExtension.Substring(1), getFileName))
                        //        {
                        //            //Le fichier n'est par renommer en .bak
                        //            //En effet Les services comme l'appli web peuvent fonctionner sur plusieurs databases simultanée
                        //            //Il faut conserver le fichier pour la 1er utilisation avec une nouvelle database
                        //            //Rename file 
                        //            //FileRenameToDotBak(getFileName);
                        //            //Appel récursif afin d'exploiter le fichier depuis la DB et ainsi extraire d'éventuel autres fichiers (ex.: #include)
                        //            string newObjectName = newFileName;
                        //            getFileName = GetFile2(pCS, pPath, pFileType, pFolder, pSubFolder, newObjectName, true);
                        //            isFound = StrFunc.IsFilled(getFileName);
                        //        }
                        //        else
                        //        {
                        //            isFound = false;
                        //            throw new Exception("Impossibility to save the file: " + fileName);
                        //        }
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        throw new Exception("Impossibility to save the file: " + fileName + Cst.CrLf2 + ex.Message);
                        //    }
                        //    #endregion
                        //}
                    }
                    if (isFound)
                        break;
                }//end for
            }
            #endregion [Step 3] Search: "Spheres files" or "Old Specific files"

            if (isFound)
                opFilePath = getFileName;
            else
                opFilePath = string.Empty;

            return isFound;
        }

        /// <summary>
        /// Retourne un nom de fichier unique dans le Temps
        /// le nom généré = {pKey}_{pIdentifier}_{DateTime.Now.ToString("yyyyMMddHHmmssfffff")}
        /// <para>les caractères non autorisés (%,\,:,? etc..) sont supprimés</para>
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pIdentifier"></param>
        /// <returns></returns>
        public static string GetUniqueName(string pKey, string pIdentifier)
        {
            return GetUniqueName(pKey, pIdentifier, "yyyyMMddHHmmssfffff");
        }

        /// <summary>
        /// Retourne un nom de fichier unique dans le Temps
        /// le nom généré = {pKey}_{pIdentifier}_{DateTime.Now.ToString(pFormat)}
        /// <para>les caractères non autorisés (%,\,:,? etc..) sont supprimés</para>
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pIdentifier"></param>
        /// <returns></returns>
        public static string GetUniqueName(string pKey, string pIdentifier, string pFormat)
        {
            if (StrFunc.IsEmpty(pFormat))
                throw new ArgumentNullException();
            string ret = pKey + "_" + pIdentifier + "_" + DateTime.Now.ToString(pFormat);
            // FI 20200409 [XXXXX] Call ReplaceFilenameInvalidChar
            ret = ReplaceFilenameInvalidChar(ret);
            return ret;

        }
        /// <summary>
        ///  Remplace les caractères interdits par string.Empty
        /// </summary>
        /// <param name="pFilename">Nom de fichier</param>
        /// <returns></returns>
        /// FI 20200409 [XXXXX] Add Method
        public static string ReplaceFilenameInvalidChar(string pFilename)
        {
            return ReplaceFilenameInvalidChar(pFilename, string.Empty);
        }


        /// <summary>
        ///  Remplace les caractères interdits par {pNewValue} 
        /// </summary>
        /// <param name="pFilename">Nom de fichier</param>
        /// <param name="pNewValue">Chaîne permettant de remplacer tous les caractères spéciaux</param>
        /// <returns></returns>
        /// FI 20200409 [XXXXX] Add Method
        public static string ReplaceFilenameInvalidChar(string pFilename, string pNewValue)
        {
            if (pFilename == null)
                throw new ArgumentNullException();

            string ret = pFilename;
            ret = ret.Replace("%", pNewValue);
            ret = ret.Replace(@"\", pNewValue);
            ret = ret.Replace("/", pNewValue);
            ret = ret.Replace(@":", pNewValue);
            ret = ret.Replace(@"*", pNewValue);
            ret = ret.Replace(@"|", pNewValue);
            ret = ret.Replace(@"?", pNewValue);
            ret = ret.Replace(@"<", pNewValue);
            ret = ret.Replace(@">", pNewValue);
            return ret;
        }


        /// <summary>
        /// Retourne la "Cible" d'un fichier "Raccourci" (Shortcut).
        /// </summary>
        /// <param name="pShortcutFilename"></param>
        /// <returns></returns>
        public static string GetShortcutTargetFile(string pShortcutFilename)
        {
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(pShortcutFilename);
            return link.TargetPath;
        }

        /// <summary>
        /// Contrôle l'existence du folder associé à un path de fichier
        /// </summary>
        /// <exception cref="SpheresException2[FOLDERNOTFOUND] si path non accessible, l'exception donne les informations suivantes: folder,nbr de tentatives,timeout en secondes "/>
        /// <param name="pFilePath"></param>
        ///<param name="pTimeOut">Timeout au delà duquel Speheres® ne cherche plus à accéder au folder (choisir 0 pour n'opérer qu'une tentative)</param>
        ///<param name="pAttemps">Obtient le nombre de tentatives effectuées pour accéder au répertoire (si le folder est détecté comme existant) </param>
        public static void CheckFolderFromFilePath(string pFilePath, double pTimeOut, out int pAttemps)
        {
            CheckFolderFromFilePath(pFilePath, pTimeOut, out pAttemps, 0);
        }
        public static void CheckFolderFromFilePath(string pFilePath, double pTimeOut, out int pAttemps, int pLevelOrder)
        {
            FileTools.GetFilenameAndFoldername(pFilePath, out string fileName, out string folderName);

            bool isFileNameWithWildcards = FileTools.IsFilenameWithWildcards(fileName);

            int nbAttemps;

            try
            {
                CheckFolder(folderName, pTimeOut, out nbAttemps, pLevelOrder);
            }
            catch (SpheresException2 ex)
            {
                if (ProcessStateTools.CodeReturnFolderNotFoundEnum == ex.ProcessState.CodeReturn)
                {
                    string[] infoEx = ex.GetLogInfo();
                    //
                    string fileSample = "sample.txt";
                    if (isFileNameWithWildcards)
                        fileSample = "sample*.txt";
                    //
                    string[] infoFolder = new string[3] { string.Empty, string.Empty, string.Empty };
                    //
                    // RD 20110920 / FI 20110920
                    // Réduire les index, car GetLogInfo(), "[" + Method + "]" est à la fin de la liste
                    infoFolder[0] = infoEx[1]; //Nom du folder
                    infoFolder[1] = infoEx[2]; //Nbr de tentatives pour accéder au folder
                    infoFolder[2] = infoEx[3]; //Timeout utilise pour limiter les tentatives 
                    //
                    string errorMsg = @"<b>Folder does not exist or is not accessible</b>" + Cst.CrLf;
                    errorMsg += @"- Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path." + Cst.CrLf;
                    errorMsg += StrFunc.AppendFormat(@"(eg: \\server\share\directory\{0} or S:\directory\{0})", fileSample) + Cst.CrLf;
                    errorMsg += "- If the folder exist, contact your system administrator to verify that the acccount has rights and to make sure there are no problems with the network or its configurations." + Cst.CrLf;
                    errorMsg += @"<b>" + infoFolder[0] + "</b>" + Cst.CrLf;
                    errorMsg += @"[File: " + fileName + "]" + Cst.CrLf;
                    errorMsg += infoFolder[1] + Cst.CrLf;
                    errorMsg += infoFolder[2] + Cst.CrLf;
                    //
                    //Attention c'est bien l'inner exception qui est injectée afin d'avoir l'erreur source
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, pLevelOrder, errorMsg,
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FOLDERNOTFOUND), ex.InnerException, infoFolder);
                }
                else
                    throw;
            }
            catch { throw; }

            pAttemps = nbAttemps;
        }

        /// <summary>
        /// Contrôle l'existence du folder
        /// </summary>
        /// <exception cref="SpheresException2[FOLDERNOTFOUND] si path non accessible, l'exception donne les infos suivantes: folder,nbr de tentatives,timeout en secondes "/>
        /// <param name="pPath">Chemin du Folder</param>
        ///<param name="pTimeOut">Timeout au delà duquel Speheres® ne cherche plus à accéder au folder (choisir 0 pour n'opérer qu'1 seule tentative)</param>
        ///<param name="pAttemps">Obtient le nombre de tentatives effectuées pour accéder au répertoire (si le folder est détecté comme existant) </param>
        public static void CheckFolder(string pPath, double pTimeOut, out int opAttemps)
        {
            CheckFolder(pPath, pTimeOut, out opAttemps, 0);
        }
        /// <summary>
        /// Contrôle l'existence du folder
        /// </summary>
        /// <exception cref="SpheresException2[FOLDERNOTFOUND] si path non accessible, l'exception donne les infos suivantes: folder,nbr de tentatives,timeout en secondes "/>
        /// <param name="pPath">Chemin du Folder</param>
        /// <param name="pTimeOut">Timeout au delà duquel Speheres® ne cherche plus à accéder au folder (choisir 0 pour n'opérer qu'1 seule tentative)</param>
        /// <param name="pAttemps">nombre de tentatives effectuées pour accéder au répertoire (si le folder est détecté comme existant)</param>
        /// <param name="pLevelOrder">Level order appliqué à l'exception lorsque le folder est non accessible</param>
        public static void CheckFolder(string pPath, double pTimeOut, out int opAttemps, int pLevelOrder)
        {

            double timeOut = pTimeOut;
            int count = 0;
            bool isFolderExist = false;
            string currentDir = Directory.GetCurrentDirectory();
            //
            DatetimeProfiler dtProfiler = new DatetimeProfiler(DateTime.Now);
            while ((dtProfiler.GetTimeSpan().TotalSeconds.CompareTo(timeOut) == -1) && (false == isFolderExist) && (count < Int32.MaxValue))
            {
                //Note: Appels multiples à SetCurrentDirectory afin d'essayer d'activer un folder existant 
                //mais détecté comme "not exists"
                try
                {
                    count++;
                    Directory.SetCurrentDirectory(pPath);
                    isFolderExist = true;
                }
                catch
                {
                    Thread.Sleep(500); //Pause de 0.5 sec.
                }
                finally
                {
                    Directory.SetCurrentDirectory(currentDir);
                }
            }
            //
            if (false == isFolderExist)
            {
                try
                {
                    count++;
                    //Note: Nouvel appel à SetCurrentDirectory afin de tenter de récupérer une erreur "précise"
                    Directory.SetCurrentDirectory(pPath);
                }
                catch (Exception ex)
                {
                    //
                    string[] info = new string[3] { string.Empty, string.Empty, string.Empty };
                    info[0] = StrFunc.AppendFormat(@"[Folder: {0}]", pPath);
                    info[1] = StrFunc.AppendFormat(@"[Failed attempts to access: {0}]", count.ToString());
                    info[2] = StrFunc.AppendFormat(@"[Attempts timeout: {0} seconds]", timeOut.ToString());
                    //
                    string errorMsg = @"<b>Folder does not exist or is not accessible</b>" + Cst.CrLf;
                    errorMsg += "- If the folder exist, contact your system administrator to verify that the acccount has rights and to make sure there are no problems with the network or its configurations." + Cst.CrLf;
                    errorMsg += StrFunc.AppendFormat(@"<b>{0}</b>", info[0]) + Cst.CrLf;
                    errorMsg += info[1] + Cst.CrLf;
                    errorMsg += info[2] + Cst.CrLf;
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, pLevelOrder, errorMsg,
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FOLDERNOTFOUND), ex.InnerException, info);
                }
                finally
                {
                    Directory.SetCurrentDirectory(currentDir);
                }
            }
            opAttemps = count;
        }

        /// <summary>
        /// Récupère le folder et le fichier (avec l'extension) à partir d'un chemin complet
        /// </summary>
        /// <param name="pFullpathname">Représente le chemin complet</param>
        /// <param name="opFilename"></param>
        /// <param name="opFoldername"></param>
        public static void GetFilenameAndFoldername(string pFullpathname, out string opFilename, out string opFoldername)
        {
            opFilename = Path.GetFileName(pFullpathname);
            opFoldername = Path.GetDirectoryName(pFullpathname);
        }

        /// <summary>
        /// Retourne true s'il existe  "?" ou "*" dans le nom de fichier {pFilename}
        /// </summary>
        /// <param name="pFilename"></param>
        /// <returns></returns>
        public static bool IsFilenameWithWildcards(string pFilename)
        {
            return (pFilename.IndexOf(@"*") >= 0) || (pFilename.IndexOf(@"?") >= 0);
        }

        /// <summary>
        /// Retourne true si temps écoulé depuis la création du fichier a dépassé {pLifeTimeInMinutes}
        /// </summary>
        /// <param name="pFile"></param>
        /// <param name="pLifeTimeInMinutes"></param>
        /// <returns></returns>
        public static bool IsOutdatedFile(string pFile, int pLifeTimeInMinutes)
        {
            bool ret = false;

            if (pLifeTimeInMinutes <= 0)
                pLifeTimeInMinutes = 10;

            FileInfo file = new FileInfo(pFile);
            DateTime dtCreation = file.CreationTimeUtc;
            TimeSpan timeSpan = (DateTime.UtcNow - dtCreation);
            if (timeSpan.TotalMinutes >= pLifeTimeInMinutes)
                ret = true;

            return ret;
        }

        /// <summary>
        /// If exception is that "The process cannot access the file 'xxxxxx’ because it is being used by another process"
        /// </summary>
        /// <param name="pException"></param>
        /// <returns></returns>
        public static bool IsFileUsedException(Exception pException)
        {
            // FI 20210628 [XXXXX] test sur l'erreur HResult = -2147024864
            //return (pException.Message.Contains("The process cannot access the file") &&
            //    pException.Message.Contains("because it is being used by another process"));

            return
                (pException is IOException)
                &&
                (pException.HResult == -2147024864); // https://windows-hexerror.linestarve.com/0x80070020 The process cannot access the file because it is being used by another process
        }

        /// <summary>
        /// Ouvre un fichier dans le mode spécifié, avec accès en lecture, en écriture ou en lecture/écriture
        /// <para>et avec gestion de l'accès concurrentiel au fichier: si le fichier est bloqué par un autre process, alors boucler jusqu'à ce que le fichier se libère</para>
        /// </summary>
        /// <param name="pFileInfo">Objet FileInfo du fichier à ouvrir</param>
        /// <param name="pFileMode">Constante System.IO.FileMode spécifiant le mode (Open ou Append, par exemple)dans lequel ouvrir le fichier.</param>
        /// <param name="pFileAccess">Constante System.IO.FileAccess spécifiant si le fichier doit être ouvert avec l'accès Read, Write ou ReadWrite.</param>
        /// <returns></returns>
        // RD 20171013 [23506] Add method
        public static FileStream OpenFile(FileInfo pFileInfo, FileMode pFileMode, FileAccess pFileAccess)
        {
            FileStream fileStream = null;
            int countLoop = 1;
            bool isToLoop = true;

            while (isToLoop)
            {
                try
                {
                    fileStream = pFileInfo.Open(pFileMode, pFileAccess);
                    // Fichier n'est pas bloqué par un autre process, donc pas besoin de boucler
                    isToLoop = false;
                }
                catch (Exception ex)
                {
                    if (FileTools.IsFileUsedException(ex))
                    {
                        //Pause de 5 sec.
                        Thread.Sleep(FileTools.ThreadSleepBeforLoop);

                        // On tente l'utilisation du fichier au maximum 10 fois
                        if (countLoop < FileTools.MaxLoopPass)
                            // Nombre de boucles incrémenté
                            countLoop++;
                        else
                            throw;
                    }
                    else
                        throw;
                }
            }

            return fileStream;
        }

        /// <summary>
        /// Retourne un nom de fichier qui n'existe pas
        /// <para>Si le fichier {pPath} n'existe pas, retourne {pPath}</para>
        /// <para>Si le fichier {pPath} existe, retourne {pPath}_{n} n vaut de 1 à n</para>
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        private static string GetNotExistingFileName(string pPath)
        {
            string ret = pPath;
            //
            string pathExtension = Path.GetExtension(pPath); // ex : .XML
            string path2WithoutExtension = Path.ChangeExtension(pPath, null);
            int i = 0;
            while (File.Exists(ret))
            {
                i++;
                ret = path2WithoutExtension + "_" + i.ToString() + pathExtension;
            }
            return ret;
        }

       

        /// <summary>
        /// Génère un fichier à partir de n fichiers
        /// </summary>
        /// <param name="pPath">Liste des fichiers source</param>
        /// <param name="pPathDestination">Nom du fichier généré</param>
        /// <param name="pIsUseNewLine">True, s'il est nécessaire d'ajouter \r\n entre 2 fichiers</param>
        /// FI 20131113 [19081] Concatenation de n fichier
        public static void FileConcat(string[] pPath, ref string pPath2, Boolean pIsUseNewLine)
        {
            pPath2 = GetNotExistingFileName(pPath2);

            using (FileStream fsFile = File.Create(pPath2))
            {
                for (int i = 0; i < ArrFunc.Count(pPath); i++)
                {
                    if (pIsUseNewLine && (i > 0))
                    {
                        byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                        fsFile.Write(newline, 0, newline.Length);
                    }
                    byte[] read = FileTools.ReadFileToBytes(pPath[i]);
                    fsFile.Write(read, 0, read.Length);
                }
            }
        }
    }
}
