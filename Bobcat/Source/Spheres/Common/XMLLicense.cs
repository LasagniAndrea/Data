using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.Collections;
using System.Linq;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

namespace EFS.Common
{
    #region public enum LimitationFunctionalityEnum
    // EG 20220826 [XXXXX][WI413]Gestion licence sur usage de Shibboleth SP avec Spheres
    public enum LimitationFunctionalityEnum
	{
		CUSTOMIZEWELCOME,	//True/False
		CUSTOMIZEBANNER,	//True/False
		MODELDAYHOUR,		//True/False
		CONSULTATION,		//True/False
		MENU,MENUOF,		//Add
        TRADEIMPORT,        //True/False
        //CC 20140110 
        PRISMAONLY,         //True/False
        SHIBBOLETHSP,       //True/False
    }
	#endregion
	//	
	#region public enum LimitationProductEnum
    // EG 20140526 New build FpML4.4 VarianceSwapOption removed
	public enum LimitationProductEnum
	{
        //BO (Bond option)
        bondOption,
        //CD (Credit default)
        creditDefaultSwap,
        //DSE (Debentures securities)
        buyAndSellBack, debtSecurity, securityLending, debtSecurityTransaction, repo,
        //EQD (Equity derivative)
        brokerEquityOption, equityOption,
		//EQF (Equity forward)
		equityForward,
        //EQS 
        equitySwap, equitySwapTransactionSupplement, returnSwap,
        //EQVS 
        equityOptionTransactionSupplement,
        //ESE (Equity securities)
        //share, //PL 20180620 Mise en commentaire
        equitySecurityTransaction,
        //FX (Foreign exchange)
        fxAverageRateOption, fxBarrierOption, fxDigitalOption, fxSimpleOption, fxSingleLeg, fxSwap, termDeposit,
        //INV
        additionalInvoice, credit, invoice, invoiceSettlement,
        //IRD (Interest rate derivative)
        bulletPayment, capFloor, loanDeposit, fra, swap, swaption,
        //COMD (Commodity derivative)
        commoditySpot, commoditySwap,
        //LSD (Listed derivative) 
        exchangeTradedDerivative, STGexchangeTradedDerivative,
        //STRATEGY                                                         
        strategy,
        // ---------------------------------------------
        //COS (Correlation swap)
		correlationSwap,
		//VAS (Variance swap)
		varianceSwap,
        /*varianceSwapOption,*/
        // ---------------------------------------------
        //MARGIN & CASHBALANCE
        marginRequirement, cashBalance, cashPayment, collateral, cashBalanceInterest
	}
	#endregion
	//
	#region public enum LimitationSystemEnum
	public enum LimitationSystemEnum
	{
        IBMDB2,
        //ORACL9I2, ORACL10G, ORACL11G, 
        ORACL12C, ORACL18C, ORACL19C,
        //SQLSRV2K, SQLSRV2K5, 
        //SQLSRV2K8, 
        SQLSRV2K12, SQLSRV2K14, SQLSRV2K16, SQLSRV2K17, SQLSRV2K19, SQLSRV2K22,
        //SYBASE12,
        SAPASE,
		FileWatcher, MSMQ, MQSeries
	}
    #endregion
    //
    #region public class License
    /// <summary>
    /// Classe de gestion des licenses
    /// </summary>
    // PM 20240524 [WI947] Ajout LicInitialMargin / InitialMarginMethods
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
	public class License 
	{
		#region Members
		#region private LimitationTypeEnum
		private enum LimitationTypeEnum
		{
			LicFunctionality, LicProduct, LicService, LicSystem, LicInitialMargin
        }
		#endregion
		//
		[System.Xml.Serialization.XmlArrayItemAttribute("LicProduct", typeof(LicProduct), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public LicProduct[] Products;
		//	
		[System.Xml.Serialization.XmlArrayItemAttribute("LicService", typeof(LicService), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public LicService[] Services;
		//
		[System.Xml.Serialization.XmlArrayItemAttribute("LicSystem", typeof(LicSystem), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public LicSystem[] Systems;
		//
		[System.Xml.Serialization.XmlArrayItemAttribute("LicFunctionality", typeof(LicFunctionality), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public LicFunctionality[] Functionalities;
        //
        [System.Xml.Serialization.XmlArrayItemAttribute("LicInitialMargin", typeof(LicInitialMargin), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public LicInitialMargin[] InitialMarginMethods;
        //
        [System.Xml.Serialization.XmlAttributeAttribute()]
		public string softwarename;
		//
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string softwareversion;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int entity;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int user;
        //
        [System.Xml.Serialization.XmlAttributeAttribute()]
		public string licensee;
		//
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int trial;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool trialSpecified;
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int trialRemaining;
		//
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string registration;
		#endregion
		//
		#region constructor
		public License()
		{
		}
		#endregion constructor
		//
		#region public IsValidRegistration
		public bool IsValidRegistration(ref int opMaxSimultaneousLoginAuthorized, ref int opMaxEntityAuthorized)
		{
			string registraytionKey = registration;
            bool isOk = (!String.IsNullOrEmpty(licensee)) && (!String.IsNullOrEmpty(registraytionKey));

			if (isOk)
			{
                string licAndSoft = Software.Name + "-" + Software.Major + "-" + licensee;

                if (trialSpecified)
                    licAndSoft += "-" + trial;

                string licAndSoftHashed = StrFunc.HashData(licAndSoft, Cst.HashAlgorithm.MD5);
                //Check license / registration
                isOk = registraytionKey.StartsWith(licAndSoftHashed);
                    
                if (isOk)
                {
                    registraytionKey = registraytionKey.Remove(0, licAndSoftHashed.Length);//remove licenseHashed

                    if (!String.IsNullOrEmpty(registraytionKey))
                    {
                        #region
                        string delimStr = "-";
                        isOk = registraytionKey.StartsWith(delimStr);
                        if (isOk)
                        {
                            isOk = (registraytionKey.Length > 1);
                            if (isOk)
                            {
                                registraytionKey = registraytionKey.Remove(0, 1);//remove -

                                char[] delimiter = delimStr.ToCharArray();
                                string[] split = registraytionKey.Split(delimiter);

                                //Get max login
                                //for (int i=1;i<1000;i++) PL 20120327
                                for (int i = 1; i < 10000; i++)
                                {
                                    if (split[0] == StrFunc.HashData(licAndSoft.Substring(0, 1) + i.ToString() + licAndSoftHashed.Substring(0, 1), Cst.HashAlgorithm.MD5).Substring(0, 5))
                                    {
                                        opMaxSimultaneousLoginAuthorized = i;
                                        break;
                                    }
                                }

                                //Get max entity
                                for (int i = 1; i < 1000; i++)
                                {
                                    if (split[1] == StrFunc.HashData(licAndSoft.Substring(0, 1) + i.ToString() + licAndSoftHashed.Substring(0, 1), Cst.HashAlgorithm.MD5).Substring(0, 5))
                                    {
                                        opMaxEntityAuthorized = i;
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
 			}
			return isOk;
		}
		#endregion
		#region public IsLicFunctionalityAuthorised
		public bool IsLicFunctionalityAuthorised(string pLicFunctionality, bool pDefaultReturn)
		{
			if (System.Enum.IsDefined(typeof(LimitationFunctionalityEnum), pLicFunctionality))
				return IsLicFunctionalityAuthorised((LimitationFunctionalityEnum)System.Enum.Parse(typeof(LimitationFunctionalityEnum), pLicFunctionality));
			else
				//Functionalité inconnue
				return pDefaultReturn;
		}
		public bool IsLicFunctionalityAuthorised(LimitationFunctionalityEnum pLicFunctionality)
		{
			return IsAuthorised(LimitationTypeEnum.LicFunctionality, pLicFunctionality.ToString());
		}
		#endregion
		#region public IsLicFunctionalityAuthorised_Add
		public bool IsLicFunctionalityAuthorised_Add(string pLicFunctionality, bool pDefaultReturn)
		{
			if (System.Enum.IsDefined(typeof(LimitationFunctionalityEnum), pLicFunctionality))
				return IsLicFunctionalityAuthorised_Add((LimitationFunctionalityEnum)System.Enum.Parse(typeof(LimitationFunctionalityEnum), pLicFunctionality));
			else
				//Functionalité inconnue
				return pDefaultReturn;
		}
		public bool IsLicFunctionalityAuthorised_Add(LimitationFunctionalityEnum pLicFunctionality)
		{
			return IsAddAuthorised(LimitationTypeEnum.LicFunctionality, pLicFunctionality.ToString());
		}
		
		#endregion IsLicFunctionalityAuthorised_Add
		//
		#region public IsLicProductAuthorised
		public bool IsLicProductAuthorised(LimitationProductEnum pLicProduct)
		{
			return IsAuthorised(LimitationTypeEnum.LicProduct, pLicProduct.ToString());
		}
		public bool IsLicProductAuthorised(string pProduct)
		{
			return IsAuthorised(LimitationTypeEnum.LicProduct, pProduct);
		}
		#endregion
		#region public IsLicProductAuthorised_Add
        /// <summary>
        /// Return true si le produit est autorisé pour la "Création" de trade. 
        /// </summary>
        /// <param name="pLicProduct"></param>
        /// <returns></returns>
		public bool IsLicProductAuthorised_Add(LimitationProductEnum pLicProduct)
		{
			return IsAddAuthorised(LimitationTypeEnum.LicProduct, pLicProduct.ToString());
		}
		/// <summary>
		/// Return true si le produit est autorisé pour la "Création" de trade. 
		/// </summary>
		/// <param name="pProduct"></param>
		/// <returns></returns>
        public bool IsLicProductAuthorised_Add(string pProduct)
		{
			return IsAddAuthorised(LimitationTypeEnum.LicProduct, pProduct);
		}
        /// <summary>
        /// Return true si seul le produit "exchangeTradedDerivative" est autorisé pour la "Création" de trade. 
        /// </summary>
        /// <returns></returns>
        public bool IsLicProductAuthorised_Add_ALLOCETDOnly()
        {
            return IsLicProductAuthorised_AddOnly(LimitationProductEnum.exchangeTradedDerivative);
        }
        /// <summary>
        /// Retourne true si la license autorise uniquement {productEnum}
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public Boolean IsLicProductAuthorised_AddOnly(LimitationProductEnum product)
        {
            Boolean ret = IsLicProductAuthorised_Add(product);
            if (ret)
            {
                foreach (string item in Enum.GetNames(typeof(LimitationProductEnum)).Where(x => x != product.ToString()))
                {
                    if (IsLicProductAuthorised_Add((LimitationProductEnum)Enum.Parse(typeof(LimitationProductEnum), item)))
                    {
                        ret = false;
                        break;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Return true si le produit "exchangeTradedDerivative" n'est pas autorisé pour la "Création" de trade. 
        /// </summary>
        /// <returns></returns>
        public bool IsLicProductAuthorised_Add_ALLOCETDNotAllowed()
        {
            bool ret = (!IsLicProductAuthorised_Add(LimitationProductEnum.exchangeTradedDerivative));
            return ret;
        }
        #endregion
		//		
		#region public IsLicServiceAuthorised
		public bool IsLicServiceAuthorised(Cst.ServiceEnum pLicService)
		{
			return IsAuthorised(LimitationTypeEnum.LicService, pLicService.ToString());
		}
		public bool IsLicSystemAuthorised(LimitationSystemEnum pLicSystem)
		{
			return IsAuthorised(LimitationTypeEnum.LicSystem, pLicSystem.ToString());
		}
        #endregion
        //
        #region public IsLicInitialMarginMethodAuthorised
        /// <summary>
        /// Teste si une méthode de calcul de déposit est autorisée ou pas par la license
        /// </summary>
        /// <param name="pLicInitialMargin"></param>
        /// <returns>True si méthode autorisée, sinon false</returns>
        // PM 20240524 [WI947] Ajout
        public bool IsLicInitialMarginMethodAuthorised(EfsML.Enum.InitialMarginMethodEnum pLicInitialMargin)
        {
            return IsAuthorised(LimitationTypeEnum.LicInitialMargin, pLicInitialMargin.ToString());
        }
        #endregion
        //		
        #region private IsTrueAuthorised
        private bool IsTrueAuthorised(LimitationTypeEnum pLimitationType, string pData)
		{
			bool ret = false;
//#if DEBUG
//            ret = true;
//#else
			try
			{
				string dataCrypted     = Encrypt(pData);
				string dataValueHashed = string.Empty;
				if (ReadValue(pLimitationType, dataCrypted, out dataValueHashed))
				{
					string getValueHashed = Get_True_ValueHashed(dataCrypted);
					if (dataValueHashed == getValueHashed)
					{
						ret = true;
					}
				}
			}
			catch
			{
				ret = false;
			}
//#endif
			return ret;
		}
		#endregion
		#region private IsAddAuthorised
		private bool IsAddAuthorised(LimitationTypeEnum pLimitationType, string pData)
		{
			bool ret = false;

//#if DEBUG
//            ret = true;
//#else
			try
			{
                string dataCrypted = Encrypt(pData);
				string dataValueHashed = string.Empty;
				if (ReadValue(pLimitationType, dataCrypted, out dataValueHashed))
				{
					string getValueHashed = Get_Add_ValueHashed(dataCrypted);

					ret = (dataValueHashed == getValueHashed);
				}
			}
			catch
			{
				ret = false;
			}
//#endif 
			return ret;
		}
		
		#endregion
		#region private IsAuthorised
		private bool IsAuthorised(LimitationTypeEnum pLimitationType, string pData)
		{
			return  IsTrueAuthorised(pLimitationType,pData) || IsAddAuthorised(pLimitationType,pData);   
		}
		#endregion
			
		#region private Encrypt
		private string Encrypt(string pData)
		{
			//Cryptage via la licence
            string dataEncrypted = Cryptography.Encrypt(pData, licensee);
        
            //New from v8
            dataEncrypted = dataEncrypted.Replace("+", "P");
            dataEncrypted = dataEncrypted.Replace("-", "L");
            dataEncrypted = dataEncrypted.Replace("=", "E"); 
            dataEncrypted = dataEncrypted.Replace("/", "G");

            return dataEncrypted;
		}
		#endregion
		#region private Get_True_ValueHashed
		private string Get_True_ValueHashed(string pData)
		{
			return Get_True_ValueHashed(pData, false);
		}
		private string Get_True_ValueHashed(string pData, bool pIsClear)
		{
			return Get_True_ValueHashed(pData, pIsClear, string.Empty);
		}
		private string Get_True_ValueHashed(string pData, bool pIsClear, string pTrue)
		{
			//Hachage de la licence + "true" + dataCrypted	
			const string True = "true";
			//
			pTrue = (StrFunc.IsEmpty(pTrue)? True : pTrue);
			//
            return Get_ValueHashed(pData, pIsClear, pTrue);
		}
		#endregion Get_Add_ValueHashed
		#region private Get_Add_ValueHashed
		private string Get_Add_ValueHashed(string pData)
		{
			return Get_Add_ValueHashed(pData, false);
		}
		private string Get_Add_ValueHashed(string pData, bool pIsClear)
		{
			return Get_Add_ValueHashed(pData, pIsClear, string.Empty);
		}
		private string Get_Add_ValueHashed(string pData, bool pIsClear, string pAdd)
		{
			//Hachage de la licence + "add" + dataCrypted 
			const string Add = "add";	
			//
			pAdd = (StrFunc.IsEmpty(pAdd)? Add : pAdd);
			//
			return Get_ValueHashed(pData,pIsClear,pAdd);			
		}
		#endregion Get_Add_ValueHashed
		#region private Get_ValueHashed
		private string Get_ValueHashed(string pData, bool pIsClear, string pValue)
		{
			if (pIsClear)
				return pValue;
			else
				return StrFunc.HashData(licensee + pValue + pData, Cst.HashAlgorithm.MD5);
		}

        #endregion Get_ValueHashed
        #region private GetRegistration

        private string GetRegistration_Hashed()
		{
            //string maxLogin = "maxLogin=";
            //string maxEntity = "maxEntity=";
            //string version = "sofwareVersion=";

            ////ex.: sofwareVersion=8-maxLogin=10-maxEntity=1
            //string registrationKey = version + softwareversion + "-" + maxLogin + user.ToString() + "-" + maxEntity + entity.ToString();
           
            string licAndSoft = softwarename + "-" + softwareversion + "-" + licensee;
            if (trialSpecified)
                licAndSoft += "-" + trial;
			
			string hash_licAndSoft = StrFunc.HashData(licAndSoft, Cst.HashAlgorithm.MD5);

            string tmp = licAndSoft.Substring(0, 1) + user.ToString() + hash_licAndSoft.Substring(0, 1);
            string hash_maxLogin = StrFunc.HashData(tmp, Cst.HashAlgorithm.MD5).Substring(0, 5);

            tmp = licAndSoft.Substring(0, 1) + entity.ToString() + hash_licAndSoft.Substring(0, 1);
            string hash_maxEntity = StrFunc.HashData(tmp, Cst.HashAlgorithm.MD5).Substring(0, 5);

			return (hash_licAndSoft + "-" + hash_maxLogin + "-" + hash_maxEntity);
		}

        #endregion
        #region private GetItemHashed
        private void GetItemHashed2(LicBase pItem)
        {
            string nameClear = pItem.name;
            //opName = Encrypt(nameClear);
            if (pItem.name.StartsWith("Spheres"))
                pItem.name = pItem.name.Substring(7);
            pItem.name = pItem.name.Replace("ORACL", "Oracle ");
            pItem.name = pItem.name.Replace("SQLSRV2K8", "SQL Server 2008");
            pItem.name = pItem.name.Replace("SQLSRV2K", "SQL Server 20");

            pItem.key = Encrypt(nameClear);

            if (pItem.Value == Get_True_ValueHashed(nameClear, true))
            {
                pItem.Value = Get_True_ValueHashed(pItem.key);
            }
            else if (pItem.Value == Get_Add_ValueHashed(nameClear, true))
            {
                pItem.Value = Get_Add_ValueHashed(pItem.key);
            }
            else
            {
                //pItem.Value = Get_ValueHashed(pItem.key, false, pItem.Value);
                pItem.key = null;
                pItem.Value = null;
            }
        }
        #endregion
        #region private HashValue
        // PM 20240524 [WI947] Ajout LicInitialMargin / InitialMarginMethods
        private void HashValue(LimitationTypeEnum pLimitationType)
		{
			switch (pLimitationType)
			{
				case LimitationTypeEnum.LicFunctionality:
					for (int index=0;index<Functionalities.Length;index++)
					{
						LicFunctionality item = Functionalities[index];
						//GetItemHashed(ref item.name, ref item.Value);
                        GetItemHashed2(item);
					}
					break;
				case LimitationTypeEnum.LicProduct:
					for (int index=0;index<Products.Length;index++)
					{
						LicProduct item = Products[index];
						//GetItemHashed(ref item.name, ref item.Value);
                        GetItemHashed2(item);
					}	
					break;
				case LimitationTypeEnum.LicService:
					for (int index=0;index<Services.Length;index++)
					{
						LicService item = Services[index];
                        //GetItemHashed(ref item.name, ref item.Value);
                        GetItemHashed2(item);
					}
					break;
				case LimitationTypeEnum.LicSystem:
					for (int index=0;index<Systems.Length;index++)
					{
						LicSystem item = Systems[index];
                        //GetItemHashed(ref item.name, ref item.Value);
                        GetItemHashed2(item);
					}
					break;
                case LimitationTypeEnum.LicInitialMargin:
                    for (int index = 0; index < InitialMarginMethods.Length; index++)
                    {
                        LicInitialMargin item = InitialMarginMethods[index];
                        GetItemHashed2(item);
                    }
                    break;

            }
		}
        #endregion private HashValue

        #region private ReadValue
        // PM 20240524 [WI947] Ajout LicInitialMargin / InitialMarginMethods
        private bool ReadValue(LimitationTypeEnum pLimitationType, string pData, out string opValue)
        {
            bool ret = false;
            opValue = string.Empty;
            try
            {
                switch (pLimitationType)
                {
                    case LimitationTypeEnum.LicFunctionality:
                        if (ArrFunc.IsFilled(Functionalities))
                        {
                            for (int index = 0; index < Functionalities.Length; index++)
                            {
                                LicFunctionality item = Functionalities[index];
                                //if (item.name == pData)
                                if (item.key == pData)
                                {
                                    opValue = item.Value;
                                    ret = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case LimitationTypeEnum.LicProduct:
                        if (ArrFunc.IsFilled(Products))
                        {
                            for (int index = 0; index < Products.Length; index++)
                            {
                                LicProduct item = Products[index];
                                //if (item.name == pData)
                                if (item.key == pData)
                                {
                                    opValue = item.Value;
                                    ret = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case LimitationTypeEnum.LicService:
                        if (ArrFunc.IsFilled(Services))
                        {
                            for (int index = 0; index < Services.Length; index++)
                            {
                                LicService item = Services[index];
                                //if (item.name == pData)
                                if (item.key == pData)
                                {
                                    opValue = item.Value;
                                    ret = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case LimitationTypeEnum.LicSystem:
                        if (ArrFunc.IsFilled(Systems))
                        {
                            for (int index = 0; index < Systems.Length; index++)
                            {
                                LicSystem item = Systems[index];
                                //if (item.name == pData)
                                if (item.key == pData)
                                {
                                    opValue = item.Value;
                                    ret = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case LimitationTypeEnum.LicInitialMargin:
                        if (ArrFunc.IsFilled(InitialMarginMethods))
                        {
                            foreach (LicInitialMargin item in InitialMarginMethods)
                            {
                                if (item.key == pData)
                                {
                                    opValue = item.Value;
                                    ret = true;
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }
		#endregion 
		
		#region public static methods
        public static License Load(string pCs, string pSoftwareName)
        {
            License license = null;
            string registrationXML = string.Empty;
            //
            string SQLSelect = SQLCst.SELECT + "REGISTRATIONXML" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LICENSEE.ToString() + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + "IDEFSSOFTWARE = " + DataHelper.SQLString(pSoftwareName);
            //
            object obj = DataHelper.ExecuteScalar(pCs, CommandType.Text, SQLSelect);
            if (null != obj)
                registrationXML = Convert.ToString(obj);
            //
            if (StrFunc.IsFilled(registrationXML))
            {
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(License), registrationXML.Trim());
                license = (License)CacheSerializer.Deserialize(serializeInfo);
            }
            //
            return license;
        }
		public static void Save (License pLicense, bool pIsCrypted, string pPath)
		{
            string filename = "License" + (pIsCrypted ? string.Empty : "_Uncrypted");
            //
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(License), pLicense)
            {
                IsXMLTrade = false
            };
            string path = pPath;
            if (StrFunc.IsEmpty(path))
            {
                //path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\SpheresLicense_";
                //PL  path = path.Replace(@"c:", @"c:\inetpub\wwwroot\SpheresWebSite\Temporary\");
                path = System.Web.HttpContext.Current.Server.MapPath(@"~\Temporary");
            }
            if (!path.EndsWith(@"\"))
                path += @"\";
            //
            if (!pIsCrypted)
            {
                if (File.Exists(path + @"License_Uncrypted.xml"))
                    File.Delete(path + @"License_Uncrypted.xml");
            }
            if (File.Exists(path + @"License.xml"))
                File.Delete(path + @"License.xml");
            //
            CacheSerializer.Serialize(serializeInfo, path + filename + ".xml");										
		}
        /// <summary>
        /// Génération d'un fichier de licence non crypté.
        /// <para>NB: Méthode utilisée uniquement depuis la solution "GenerateLicense" </para>
        /// </summary>
        /// <param name="pLicensee"></param>
        /// <param name="pSoftwareName"></param>
        /// <param name="pVersion"></param>
        /// <param name="pMaxLogin"></param>
        /// <param name="pMaxEntity"></param>
        /// <param name="pTrial"></param>
        /// <param name="pPath"></param>
        // PM 20240524 [WI947] Ajout LicInitialMargin / InitialMarginMethods
        public static void GenerateUncrypted(string pLicensee, string pSoftwareName, int pVersion,
            int pMaxLogin, int pMaxEntity, int pTrial, 
            string pPath)
		{
			ArrayList limitationList;
            License license = new License
            {
                licensee = pLicensee,
                softwarename = pSoftwareName,
                softwareversion = pVersion.ToString(),
                entity = pMaxEntity,
                user = pMaxLogin,
                //registration = license.GetRegistration_UnHashed(pVersion, pMaxLogin, pMaxEntity);

                trialSpecified = true,
                trial = (pTrial <= 0 ? -1 : pTrial)
            };

            try
			{
				limitationList = GetLimitationList(license, typeof(LimitationProductEnum), LimitationTypeEnum.LicProduct);
				license.Products = (LicProduct[])limitationList.ToArray(typeof(LicProduct));
				
				limitationList = GetLimitationList(license, typeof(LimitationFunctionalityEnum), LimitationTypeEnum.LicFunctionality);
				license.Functionalities = (LicFunctionality[])limitationList.ToArray(typeof(LicFunctionality));
				
				limitationList = GetLimitationList(license, typeof(LimitationSystemEnum), LimitationTypeEnum.LicSystem);
				license.Systems = (LicSystem[])limitationList.ToArray(typeof(LicSystem));
				
				limitationList = GetLimitationList(license, typeof(Cst.ServiceEnum), LimitationTypeEnum.LicService);
				license.Services = (LicService[])limitationList.ToArray(typeof(LicService));

                limitationList = GetLimitationList(license, typeof(EfsML.Enum.InitialMarginMethodEnum), LimitationTypeEnum.LicInitialMargin);
                license.InitialMarginMethods = ((LicInitialMargin[])limitationList.ToArray(typeof(LicInitialMargin))).OrderBy(l => l.name).ToArray();
            }
            catch (Exception )
			{
				throw ;
			}
			
            Save(license, false, pPath);	
		}

        /// <summary>
        /// Génération d'un fichier de licence crypté.
        /// <para>NB: Méthode utilisée uniquement depuis la solution "GenerateLicense" </para>
        /// </summary>
        /// <param name="pSoftwareName"></param>
        /// <param name="pPath"></param>
        // PM 20240524 [WI947] Ajout LicInitialMargin / InitialMarginMethods
        public static void GenerateCrypted(string pPath)
		{
			License license;
			
			XmlDocument xmlFile = new XmlDocument();
            string path = pPath;
            if (StrFunc.IsEmpty(path))
            {
                path = System.Web.HttpContext.Current.Server.MapPath(@"~\Temporary");
            }
            if (!path.EndsWith(@"\"))
            {
                path += @"\";
            }

            xmlFile.Load(path + @"\License_Uncrypted.xml");

			StringBuilder sbXMLResult    = new StringBuilder();
			XmlTextWriter xmlFileWriter  = new XmlTextWriter(new StringWriter(sbXMLResult));

			xmlFile.Save(xmlFileWriter);
			EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(License), sbXMLResult.ToString());
			license = (License)CacheSerializer.Deserialize(serializeInfo);	
			
            //license.softwarename = pSoftwareName;
            //license.softwareversion = pSoftwareVersion.ToString();
            //license.registration = license.GetRegistration_UnHashed(pSoftwareVersion, license.user, license.entity);

            license.trialSpecified = (license.trial > 0);
            license.registration = license.GetRegistration_Hashed();
						
			license.HashValue(LimitationTypeEnum.LicProduct);
			license.HashValue(LimitationTypeEnum.LicFunctionality);
			license.HashValue(LimitationTypeEnum.LicService);
			license.HashValue(LimitationTypeEnum.LicSystem);
            license.HashValue(LimitationTypeEnum.LicInitialMargin);

            Save(license, true, pPath);				
		}
        #endregion

        #region private static methods
        // EG 20220826 [XXXXX][WI413]Gestion licence sur usage de Shibboleth SP avec Spheres
        // PM 20240524 [WI947] Ajout LicInitialMargin / InitialMarginMethods
        private static ArrayList GetLimitationList(License pLicense, Type pEnumType, LimitationTypeEnum pLimitationType)
		{
			ArrayList arrayList  = new ArrayList();
            foreach (string limitation in Enum.GetNames(pEnumType))
			{			
				switch (pLimitationType)
				{
					case LimitationTypeEnum.LicFunctionality:
                        LicFunctionality licFunctionality = new LicFunctionality
                        {
                            name = limitation
                        };

                        if ( limitation == LimitationFunctionalityEnum.CONSULTATION.ToString())
                        {
                            licFunctionality.Value = pLicense.Get_True_ValueHashed(limitation,true);
                        }
                        else if (limitation == LimitationFunctionalityEnum.PRISMAONLY.ToString())
                        {
                            //PL/CC 20160727 PRISMAONLY default value to "false"
                            licFunctionality.Value = pLicense.Get_ValueHashed(limitation, true, "false");
                        }
                        else if (limitation == LimitationFunctionalityEnum.SHIBBOLETHSP.ToString())
                        {
                            licFunctionality.Value = pLicense.Get_ValueHashed(limitation, true, "false");
                        }
                        else
                        {
                            licFunctionality.Value = pLicense.Get_Add_ValueHashed(limitation, true);
                        }
													
						arrayList.Add(licFunctionality);
						break;
					case LimitationTypeEnum.LicProduct:
                        LicProduct licProduct = new LicProduct
                        {
                            name = limitation,
                            Value = pLicense.Get_Add_ValueHashed(limitation, true)
                        };
                        arrayList.Add(licProduct);
						break;
					case LimitationTypeEnum.LicService:
                        LicService licService = new LicService
                        {
                            name = limitation,
                            Value = pLicense.Get_True_ValueHashed(limitation, true)
                        };
                        arrayList.Add(licService);
						break;
					case LimitationTypeEnum.LicSystem:
                        LicSystem licSystem = new LicSystem
                        {
                            name = limitation,
                            Value = pLicense.Get_True_ValueHashed(limitation, true)
                        };
                        arrayList.Add(licSystem);
						break;
                    case LimitationTypeEnum.LicInitialMargin:
                        LicInitialMargin licInitialMargin = new LicInitialMargin
                        {
                            name = limitation,
                            Value = pLicense.Get_True_ValueHashed(limitation, true)
                        };
                        arrayList.Add(licInitialMargin);
                        break;
                }
			}

			return arrayList;
		}		
		#endregion
	}
	#endregion
	//
	#region public class LicBase 
	public class LicBase
    {
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string name;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string key;
        [System.Xml.Serialization.XmlTextAttribute()]
		public string Value;
	}
	#endregion
	//
	#region public class LicProduct 
	public class LicProduct : LicBase
	{
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public string name;
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public string key;
        //[System.Xml.Serialization.XmlTextAttribute()]
        //public string Value;
	}
	#endregion
	//
	#region public class LicFunctionality 
    public class LicFunctionality : LicBase
	{
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public string name;
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public string key;
        //[System.Xml.Serialization.XmlTextAttribute()]
        //public string Value;
	}
	#endregion
	//
	#region public class LicSystem 
	public class LicSystem : LicBase
	{
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public string name;
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public string key;
        //[System.Xml.Serialization.XmlTextAttribute()]
        //public string Value;
	}
	#endregion
	//
	#region public class LicService 
    public class LicService : LicBase
	{
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public string name;
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //public string key;
        //[System.Xml.Serialization.XmlTextAttribute()]
        //public string Value;
	}
    #endregion
    //
    #region public class LicInitialMargin
    // PM 20240524 [WI947] Ajout LicInitialMargin
    public class LicInitialMargin : LicBase
    {
    }
    #endregion
}