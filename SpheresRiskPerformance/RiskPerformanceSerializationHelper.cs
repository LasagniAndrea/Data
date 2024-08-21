using EFS.ACommon;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML.Business;
using System;
using System.IO;
using System.Text;

namespace EFS.Common
{
    /// <summary>
    /// Helper class for XML serialization
    /// </summary>
    static public class RiskPerformanceSerializationHelper
    {
       
        /// <summary>
        /// Dump the object type T a XML text stream
        /// </summary>
        /// <typeparam name="T">type of the target object yDFou want to serialize/deserialize</typeparam>
        /// <param name="pObjectToSerialize">object to serialize</param>
        /// <param name="pSetErrorWarning"></param>
        /// <param name="pAddException"></param>
        /// <returns>a string representing the serialized XML object</returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        static public StringBuilder DumpObjectToString<T>(T pObjectToSerialize,  SetErrorWarning pSetErrorWarning, AddCriticalException pAddException)
        {
            StringBuilder res = null;
            try
            {
                EFS_SerializeInfo infoSerialize = new EFS_SerializeInfo(pObjectToSerialize);
                res = CacheSerializer.Serialize(infoSerialize);
            }
            catch (Exception ex)
            {
                pSetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);
                
                SpheresException2 otcmlEx = SpheresExceptionParser.GetSpheresException(String.Format(@"Serialization failed. Object type {0}.", typeof(T)), ex);
                pAddException.Invoke(otcmlEx);
                
                Logger.Log(new LoggerData(otcmlEx));
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1018), 0));
            }
            return res;
        }

        /// <summary>
        /// Serialize the input object into a file format XML
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="pObjectToSerialize">Object to serialize</param>
        /// <param name="pFileName">file name of the destination</param>
        /// <returns>the full filename</returns>
        /// EG 20190114 Add detail to ProcessLog Refactoring
        static public string DumpObjectToFile<T>(T pObjectToSerialize, string pFileName)
        {
            string fullpath = Path.Combine(SerializationHelper.SerializationDirectory, pFileName);
            try
            {
                EFS_SerializeInfo infoSerialize = new EFS_SerializeInfo(pObjectToSerialize);
                CacheSerializer.Serialize(infoSerialize, fullpath);
            }
            catch (Exception ex)
            {
                SpheresException2 otcmlEx = SpheresExceptionParser.GetSpheresException(String.Format(@"Serialization failed. Object type {0}.", typeof(T)), ex);
                
                Logger.Log(new LoggerData(otcmlEx));
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1018), 0));
            }
            return fullpath;
        }
    }
}