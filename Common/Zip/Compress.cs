using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;

using EFS.ACommon;
using EFS.Common;

using Ionic.Zip;

//FI 29102009 [download File]  add class GZipCompress
/// <summary>
/// Classe de gestion de la compression GZIP
/// </summary>
public class GZipCompress
{
    //
    public static void Compressfolder(string pFolderSource, string pFolderDestination)
    {

        DirectoryInfo dir = new DirectoryInfo(pFolderSource);
        foreach (FileInfo fl in dir.GetFiles())
            CompressFile(fl.FullName, pFolderDestination);

    }
    //
    public static void CompressFile(string pFileName, string pFolderDestination)
    {

        FileInfo fl = new FileInfo(pFileName);
        //Open the file as a FileStream object.    
        FileStream infile = new FileStream(fl.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] buffer = new byte[infile.Length];
        //Read the file to ensure it is readable.   
        int count = infile.Read(buffer, 0, buffer.Length);
        if (count != buffer.Length)
        {
            infile.Close();
            Console.WriteLine("Test Failed: Unable to read data from file");
            return;
        }
        infile.Close();

        string FileDestination = pFolderDestination + @"\" + fl.Name + ".gz";
        Stream fs = File.Create(FileDestination);
        // Use the newly created stream for the compressed data.   
        GZipStream gZip = new GZipStream(fs, CompressionMode.Compress, true);
        gZip.Write(buffer, 0, buffer.Length);
        gZip.Close();


    }
}

//FI 29102009 [download File]  add class ZipCompress
/// <summary>
/// Classe de gestion de la compression ZIP
/// </summary>
public class ZipCompress
{
    public static string CompressFolder(string pSourceFolder, string pTargetTarget, string pTargerFileName)
    {
        string ret = pTargetTarget + pTargerFileName + ".zip";
        
        using (ZipFile zipFile = new ZipFile(ret, Encoding.UTF8))
        {
            
            zipFile.AddDirectory(pSourceFolder);
            zipFile.Save();
        }
        return ret;
    }

}