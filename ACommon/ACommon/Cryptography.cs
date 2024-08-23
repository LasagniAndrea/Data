using System;
using System.Security;
using System.Security.Cryptography;
using System.IO; 
using System.Xml; 
using System.Text; 

namespace EFS.ACommon
{
	public class Cryptography
	{
		const string DEF_ENCRYPTIONKEY = "Euro-Finance-Systems+33(0)148714444";
		//
		private static string GetEncryptionKey(string pEncryptionKey)
		{
			string encryptionKey = pEncryptionKey;
			//
			if (encryptionKey == null)
				encryptionKey = DEF_ENCRYPTIONKEY;
			encryptionKey = encryptionKey.Trim();
			if (encryptionKey.Length < 8)
				encryptionKey += DEF_ENCRYPTIONKEY;
			//
			return encryptionKey;
		}
        /// <summary>
        ///    Decrypts  a particular string with a specific Key
        /// </summary>
        /// EG 20180423 Analyse du code Correction [CA2202]
        /// FI 20201001 [XXXXX] usage de syntax using 
        public static string Decrypt(string pStringToDecrypt, string pEncryptionKey)
        {
            pEncryptionKey = GetEncryptionKey(pEncryptionKey);
            byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };

            string decrypt = string.Empty;
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(pEncryptionKey.Substring(0, 8));
                byte[] inputByteArray = new byte[pStringToDecrypt.Length];
                inputByteArray = Convert.FromBase64String(pStringToDecrypt);
                using (MemoryStream ms = new MemoryStream())
                {
                    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                    }
                    decrypt = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception) { }

            return decrypt;
        } 
		public static string Decrypt(string pStringToDecrypt) 
		{
			return Decrypt(pStringToDecrypt, null);
		}

        /// <summary>
        ///   Encrypts  a particular string with a specific Key
        /// </summary>
        /// EG 20180423 Analyse du code Correction [CA2202]
        /// FI 20201001 [XXXXX] usage de syntax using 
        public static string Encrypt(string pStringToEncrypt, string pEncryptionKey)
        {
            string decrypt = string.Empty;

            pEncryptionKey = GetEncryptionKey(pEncryptionKey);
            byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };

            try
            {
                byte[] key = Encoding.UTF8.GetBytes(pEncryptionKey.Substring(0, 8));
                byte[] inputByteArray = Encoding.UTF8.GetBytes(pStringToEncrypt);
                using (MemoryStream ms = new MemoryStream())
                {
                    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        decrypt = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception) { }
            return decrypt;
        } 
		public static string Encrypt(string pStringToEncrypt) 
		{
			return Encrypt(pStringToEncrypt, null);
		}
	} 
}