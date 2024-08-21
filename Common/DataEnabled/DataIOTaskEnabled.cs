using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common.Log;

namespace EFS.Common
{
    /// <summary>
    /// Représente, en mémoire, les tâche IO enabled
    /// <para>Chargement via IOTASK</para>
    /// </summary>

    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.IOTASK)]
    public class DataIOTaskEnabled : DataEnabledReaderBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetKey()
        {
            return new DataIOTaskEnabled().GetType().Name;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string Key => GetKey();


        /// <summary>
        /// 
        /// </summary>
        public DataIOTaskEnabled()
        {

        }


        /// <summary>
        /// Représente, en mémoire, tous les tâches IO
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        public DataIOTaskEnabled(string cs, IDbTransaction dbTransaction) : this(cs, dbTransaction, DateTime.MinValue)
        {
        }

        /// <summary>
        /// Représente, en mémoire, tous les tâches IO enabled en <paramref name="dtReference"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="dtReference"></param>
        public DataIOTaskEnabled(string cs, IDbTransaction dbTransaction, DateTime dtReference) : base(cs, dbTransaction, dtReference)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override QueryParameters GetQuery()
        {
            string query = $@"select 
IDIOTASK, IDENTIFIER, DISPLAYNAME, DESCRIPTION, 
IN_OUT, IDA, 
COMMITMODE, LOGLEVEL, 
ISNOTIF_SMTP,
SENDON_SMTP,
IDSMTPSERVER,
ORIGINADRESS,
IDA_SMTP,ADRESSIDENT_SMTP,
MAILADRESS,
PRIORITYONSUCCESS,PRIORITYONERROR,
OBJECT_SMTP,SIGNATURE_SMTP,
CSSFILENAME
from dbo.IOTASK iot
{GetWhereDtReference("iot")}";

            DataParameters dp = new DataParameters();
            QueryParameters qry = new QueryParameters(CS, query, dp);

            return qry;
        }

        /// <summary>
        ///  Retourne tous les enregistrements sous le type <seealso cref="DataIOTASK"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataIOTASK> GetDataIOTask()
        {
            //Remarque L'usage de this.Rows.Cast<DataIOTASK>() ne fonctionne pas ? (invalidcastException)
            return this.GetData().Select(x => (DataIOTASK)x);
        }


        /// <summary>
        /// Suppresssion des tâches IO du cache en relation avec la base de donnée <paramref name="cs"/> 
        /// </summary>
        /// <param name="cs"></param>
        public static new int ClearCache(string cs)
        {
            return ClearCache(cs, GetKey());
        }

    }

    /// <summary>
    /// Représente les élements de la vue VW_MARKET_IDENTIFIER
    /// </summary>
    public class DataIOTASK
    {
        /// <summary>
        /// 
        /// </summary>
        public Int32 IdIOTask { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///  type de tâche (Input, Output, comparason
        /// </summary>
        public string InOut { get; set; }


        /// <summary>
        /// owner
        /// </summary>
        public int IdA { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CommitMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Cst.CommitMode CommitModeEnum
        {
            get
            {
                Cst.CommitMode ret;
                if (Enum.IsDefined(typeof(Cst.CommitMode), CommitMode))
                {
                    ret = (Cst.CommitMode)Enum.Parse(typeof(Cst.CommitMode), CommitMode);
                }
                else
                {
                    //Pour compatibilité ascendante, si besoin... 
                    ret = Cst.CommitMode.INHERITED;
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// 
        /// 
        /// </summary>
        public string LogLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Common.Log.LogLevelDetail LogLevelEnum
        {
            get
            {
                Common.Log.LogLevelDetail ret;
                if (Enum.IsDefined(typeof(LogLevelDetail), LogLevel))
                {
                    ret = (LogLevelDetail)Enum.Parse(typeof(LogLevelDetail), LogLevel);
                }
                else
                {
                    //Pour compatibilité ascendante, si besoin... 
                    switch (LogLevel)
                    {
                        case "FULL":
                            ret = Common.Log.LogLevelDetail.LEVEL4;
                            break;
                        case "NONE":
                            ret = Common.Log.LogLevelDetail.LEVEL2;
                            break;
                        default:
                            ret = Common.Log.LogLevelDetail.LEVEL3;
                            break;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Notification par email
        /// </summary>
        public bool IsNotif_SMTP { get; set; }

        /// <summary>
        ///  status expected to send Email
        /// </summary>
        public string SendON_SMTP { get; set; }

        /// <summary>
        ///  status expected to send Email
        /// </summary>
        public Cst.StatusTask SendON_SMTPEnum
        {
            get
            {
                if (StrFunc.IsFilled(SendON_SMTP))
                    return (Cst.StatusTask)Enum.Parse(typeof(Cst.StatusTask), SendON_SMTP);
                else
                    return Cst.StatusTask.ONTERMINATED;
            }
        }

        /// <summary>
        /// SMTP Server used to send Email
        /// </summary>
        public int IdSmtpServer { get; set; }

        /// <summary>
        /// Sender Email adress
        /// </summary>
        public string OriginAdress { get; set; }

        /// <summary>
        /// Receiver of the email
        /// </summary>
        public int IDA_SMTP { get; set; }

        /// <summary>
        /// Receiver Adress Ident
        /// </summary>
        public string AddressIdent_SMTP { get; set; }

        /// <summary>
        /// Free email Adress
        /// </summary>
        public string MailAdress { get; set; }

        /// <summary>
        /// Importance in case of succes
        /// </summary>
        public string PriorityOnSuccess { get; set; }

        /// <summary>
        /// Importance in case of succes
        /// </summary>
        public Cst.StatusPriority PriorityOnSuccessEnum
        {
            get
            {
                if (StrFunc.IsFilled(PriorityOnSuccess))
                    return (Cst.StatusPriority)Enum.Parse(typeof(Cst.StatusPriority), PriorityOnSuccess);
                else
                    return Cst.StatusPriority.HIGH;
            }
        }

        /// <summary>
        /// Importance in case of error
        /// </summary>
        public string PriorityOnError { get; set; }

        /// <summary>
        /// Importance in case of error
        /// </summary>
        public Cst.StatusPriority PriorityOnErrorEnum
        {
            get
            {
                if (StrFunc.IsFilled(PriorityOnError))
                    return (Cst.StatusPriority)Enum.Parse(typeof(Cst.StatusPriority), PriorityOnError);
                else
                    return Cst.StatusPriority.HIGH;
            }
        }

        /// <summary>
        /// Email object
        /// </summary>
        public string Object_SMTP { get; set; }

        /// <summary>
        /// Email signature
        /// </summary>
        public string Signature_SMTP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CSSFilename { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public static explicit operator DataIOTASK(MapDataReaderRow item)
        {
            DataIOTASK ret = new DataIOTASK
            {
                IdIOTask = Convert.ToInt32(item["IDIOTASK"].Value),
                Identifier = Convert.ToString(item["IDENTIFIER"].Value),
                DisplayName = Convert.ToString(item["DISPLAYNAME"].Value),
                Description = Convert.ToString(item["DESCRIPTION"].Value),
                InOut = Convert.ToString(item["IN_OUT"].Value),
                CommitMode = Convert.ToString(item["COMMITMODE"].Value),
                LogLevel = Convert.ToString(item["LOGLEVEL"].Value),
                IdA = Convert.ToInt32(item["IDA"].Value),

                //Email
                IsNotif_SMTP = Convert.ToBoolean(item["ISNOTIF_SMTP"].Value),

                //Send
                SendON_SMTP = Convert.ToString(item["SENDON_SMTP"].Value),
                IdSmtpServer = (item["IDSMTPSERVER"].Value != Convert.DBNull) ? Convert.ToInt32(item["IDSMTPSERVER"].Value) : 0,
                OriginAdress = Convert.ToString(item["ORIGINADRESS"].Value),

                //Receiver
                IDA_SMTP = (item["IDA_SMTP"].Value != Convert.DBNull) ? Convert.ToInt32(item["IDA_SMTP"].Value) : 0,
                AddressIdent_SMTP = Convert.ToString(item["ADRESSIDENT_SMTP"].Value),
                MailAdress = Convert.ToString(item["MAILADRESS"].Value),

                //Message
                PriorityOnSuccess = Convert.ToString(item["PRIORITYONSUCCESS"].Value),
                PriorityOnError = Convert.ToString(item["PRIORITYONERROR"].Value),
                Object_SMTP = Convert.ToString(item["OBJECT_SMTP"].Value),
                Signature_SMTP = Convert.ToString(item["SIGNATURE_SMTP"].Value),

                CSSFilename = Convert.ToString(item["CSSFILENAME"].Value),

            };
            return ret;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public static class DataIOTaskEnabledHelper
    {

        /// <summary>
        ///  Retourne la tâche <paramref name="idIOTask"/>. Retoure null si le tâche n'existe pas
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="idIOTask"></param>
        /// <returns></returns>
        public static DataIOTASK GetDataIoTask(string cs, IDbTransaction dbTransaction, int idIOTask)
        {
            return GetDataIoTask(cs, dbTransaction, DateTime.MinValue, idIOTask);
        }

        /// <summary>
        ///  Retourne la tâche <paramref name="idIOTask"/>. Retoure null si le tâche n'existe pas ou n'est pas active en date <paramref name="dtRefence"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="dtRefence">Date de réfrence</param>
        /// <param name="idIOTask"></param>
        /// <returns></returns>
        public static DataIOTASK GetDataIoTask(string cs, IDbTransaction dbTransaction, DateTime dtRefence, int idIOTask)
        {
            IEnumerable<DataIOTASK> dataIOTask = new DataIOTaskEnabled(cs, dbTransaction, dtRefence).GetDataIOTask();
            DataIOTASK ret = dataIOTask.FirstOrDefault(x => x.IdIOTask == idIOTask);
            return ret;
        }
    }
    
}
