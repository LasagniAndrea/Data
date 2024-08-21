using EFS.ACommon;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using System;


namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// Classe de base pour l'importation des fichiers Eurex PRISMA
    /// </summary>
    /// FI 20220325 [XXXXX] Add
    /// <remarks>Certaines méthodes sont partagées avec EurosysMarketDataImportEurexPrisma (obsolete)</remarks>
    internal partial class MarketDataImportEurexPrismaBase : MarketDataImportEurexBase
    {

        // PM 20150804 [21236] Ajout type delegué pour correction mauvais format de date suite à la modif pour le ticket [21224]
        delegate DateTime StringToDateTime(string pStringDate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        public MarketDataImportEurexPrismaBase(Task pTask, string pDataName, string pDataStyle) :
            base(pTask, pDataName, pDataStyle, false)
        {

        }

        /// <summary>
        /// Initialisation à effectuer lors de l'importation des fichiers PRISMA
        /// <para>Démarrage de l'audit, Ouverture du fichier, Récupération de la date de fichier</para>
        /// </summary>
        /// <param name="pInputParsing"></param>
        protected virtual void StartPrismaImport(InputParsing pInputParsing)
        {
            string fileDescription = GetFileDesciption();

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Start importation:{0}", fileDescription), 3));

            StartAudit();

            InitDirectImport(pInputParsing);

            SetDateFile();

            OpenInputFileName();
        }

        /// <summary>
        /// Fin d'importation d'un fichier PRISMA
        /// <para>Fermeture des fichiers, Purge du cache SQL</para>
        /// </summary>
        protected virtual void EndPrismaImport()
        {
            CloseAllFiles();
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "End importation", 3));
        }

        /// <summary>
        /// Ecriture dans System.Diagnostics.Debug
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="currentLine"></param>
        /// <param name="guard"></param>
        protected void AuditLine(int lineNumber, string currentLine, int guard)
        {
            if ((lineNumber % 10000) == 0)
            {
                System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                audit.AddStep("Line number: " + lineNumber.ToString());
            }

            if (currentLine == null)
            {
                System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                System.Diagnostics.Debug.WriteLine("Guard: " + guard.ToString());
                System.Diagnostics.Debug.WriteLine("ENDED");
                audit.AddStep("ENDED");
                audit.WriteDebug();
            }
        }

        /// <summary>
        /// Retourne les caractères présents entre le début de la ligne et le 1er ";" 
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        protected static string GetFirstElement(string pLine)
        {
            int index = pLine.IndexOf(";");
            if (false == index > 0)
                throw new Exception("Missing ';' char");

            string ret = pLine.Substring(0, index);
            return ret;
        }

        /// <summary>
        /// Récupération du "Record Type" de la ligne dans le fichier
        /// </summary>
        /// <param name="pLine">Représnte une ligne du fichier</param>
        /// <returns></returns>
        protected static string GetRecordType(string pLine)
        {
            return GetFirstElement(pLine);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="pParsingRowProduct"></param>
        /// <param name="pParsingRowExpiration"></param>
        /// <param name="pParsingRowSerie"></param>
        /// <returns></returns>
        // PM 20221014 [XXXXX] Refactorisation et Rename
        protected static MarketAssetETDRequest GetAssetRequestContractMonthYear(string cs, IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowExpiration, IOTaskDetInOutFileRow pParsingRowSerie)
        {
            Tuple<string, PrismaExpiryDateComponent, PrismaSerieMainComponent> prismaComponent = GetPrismaComponent(cs, pParsingRowProduct, pParsingRowExpiration, pParsingRowSerie);

            //RowProduct
            string ProductID = prismaComponent.Item1;

            //Row Expiration
            PrismaExpiryDateComponent prismaExpiry = prismaComponent.Item2;

            //Row serie
            PrismaSerieMainComponent prismaSerie = prismaComponent.Item3;

            MarketAssetETDRequest request = PrismaTools.GetAssetRequestContractMonthYear(ProductID, prismaExpiry, prismaSerie);

            return request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="pParsingRowProduct"></param>
        /// <param name="pParsingRowExpiration"></param>
        /// <param name="pParsingRowSerie"></param>
        /// <returns></returns>
        // PM 20221014 [XXXXX] Add
        protected static MarketAssetETDRequest GetAssetRequestContractDate(string cs, IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowExpiration, IOTaskDetInOutFileRow pParsingRowSerie)
        {
            Tuple<string, PrismaExpiryDateComponent, PrismaSerieMainComponent> prismaComponent = GetPrismaComponent(cs, pParsingRowProduct, pParsingRowExpiration, pParsingRowSerie);

            //RowProduct
            string ProductID = prismaComponent.Item1;

            //Row Expiration
            PrismaExpiryDateComponent prismaExpiry = prismaComponent.Item2;

            //Row serie
            PrismaSerieMainComponent prismaSerie = prismaComponent.Item3;

            MarketAssetETDRequest request = PrismaTools.GetAssetRequestContractDate(ProductID, prismaExpiry, prismaSerie);

            return request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="pParsingRowProduct"></param>
        /// <param name="pParsingRowExpiration"></param>
        /// <param name="pParsingRowSerie"></param>
        /// <returns></returns>
        // PM 20221014 [XXXXX] Add
        private static Tuple<string, PrismaExpiryDateComponent, PrismaSerieMainComponent> GetPrismaComponent(string cs, IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowExpiration, IOTaskDetInOutFileRow pParsingRowSerie)
        {
            //RowProduct
            string ProductID = pParsingRowProduct.GetRowDataValue(cs, "Product ID");

            //Row Expiration
            PrismaExpiryDateComponent PrismaExpiry = new PrismaExpiryDateComponent()
            {
                ContractYear = pParsingRowExpiration.GetRowDataValue(cs, "Contract Year"),
                ContractMonth = pParsingRowExpiration.GetRowDataValue(cs, "Contract Month"),
                ExpirationYear = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Year"),
                ExpirationMonth = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Month"),
                ExpirationDay = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Day"),
                ContractDate = pParsingRowExpiration.GetRowDataValue(cs, "Contract Date")
            };

            //Row serie
            PrismaSerieMainComponent PrismaSerie = new PrismaSerieMainComponent()
            {
                CallPut = pParsingRowSerie.GetRowDataValue(cs, "Call Put Flag"),
                StrikePrice = pParsingRowSerie.GetRowDataValue(cs, "Exercise Price"),
                Version = pParsingRowSerie.GetRowDataValue(cs, "Series Version Number"),
                // PM 20180903 [24015] Prisma v8.0 : add SettlementType
                SettlementType = pParsingRowSerie.GetRowDataValue(cs, "Settlement Type"),
                ExerciseStyle = pParsingRowSerie.GetRowDataValue(cs, "Series exercise style flag"),
                FlexProductID = pParsingRowSerie.GetRowDataValue(cs, "Flex Product ID"),
                FlexFlag = pParsingRowSerie.GetRowDataValue(cs, "Flex Series Flag"),
                UniqueContractID = pParsingRowSerie.GetRowDataValue(cs, "Unique Contract ID"),
                ContractMnemonic = pParsingRowSerie.GetRowDataValue(cs, "Contract Mnemonic"),
                ContractFrequency = pParsingRowSerie.GetRowDataValue(cs, "Contract Frequency"),
            };

            return new Tuple<string, PrismaExpiryDateComponent, PrismaSerieMainComponent> (ProductID, PrismaExpiry, PrismaSerie);
        }

        /// <summary>
        /// Alimente la date du fichier (date de traitement)
        /// <para>Récupère la date sur la dernière ligne du fichier</para>
        /// </summary>
        /// FI 20150727 [21224] Modify 
        private void SetDateFile()
        {
            try
            {
                IOTaskDetInOutFileRow parsingRow = null;

                string lastline = FileTools.GetLastLine(dataName);
                LoadLine(lastline, ref parsingRow);

                // PM 20150804 [21236] Ajout dtFunc et stringToDateTime pour correction mauvais format de date suite à la modif pour le ticket [21224]
                DtFunc dtFunc = new DtFunc();
                StringToDateTime stringToDateTime;

                string date = string.Empty;
                if (null == parsingRow) // la ligne n'est pas systématiquement parsée (cas du fichier  LIQUIDITY FACTORS par exemple)
                {
                    if (lastline.StartsWith("*EOF*"))
                    {
                        string[] lineValues = lastline.Split(';');
                        date = lineValues[3];
                        // PM 20150804 [21236] Correction mauvais format de date suite à la modif pour le ticket [21224]
                        stringToDateTime = dtFunc.StringyyyyMMddToDateTime;
                    }
                    else
                    {
                        throw new Exception("Last line doesn't start with *EOF*");
                    }
                }
                else
                {
                    date = parsingRow.GetRowDataValue(Cs, "Current business day");
                    // PM 20150804 [21236] Correction mauvais format de date suite à la modif pour le ticket [21224]
                    stringToDateTime = dtFunc.StringDateISOToDateTime;
                }

                // FI 20150727 [21224] garde fou Exception avec message d'erreur clair lorsque la date est vide 
                if (StrFunc.IsEmpty(date))
                {
                    throw new Exception("Current business day is empty.");
                }

                // PM 20150804 [21236] Correction mauvais format de date suite à la modif pour le ticket [21224]
                //m_dtFile = new DtFunc().StringyyyyMMddToDateTime(date);
                m_dtFile = stringToDateTime(date);
            }
            catch (Exception e)
            {
                throw new Exception("Error while initializing the current business day.", e);
            }
        }
    }
}
