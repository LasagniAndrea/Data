using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;

namespace EFS.Common
{
    /// <summary>
    /// Helper class to make easy serialization procedures
    /// </summary>
    static public class SerializationHelper
    {
        static string m_SerializationDirectory = Directory.GetCurrentDirectory();

        /// <summary>
        /// Get/Set the directory where the serialized files will be saved
        /// </summary>
        public static string SerializationDirectory
        {
            get { return SerializationHelper.m_SerializationDirectory; }
            set { SerializationHelper.m_SerializationDirectory = value; }
        }

        /// <summary>
        /// Clone the input parameter object 
        /// </summary>
        /// <typeparam name="T">type of the target object you want to serialize/deserialize</typeparam>
        /// <param name="pToClone">the object to be cloned</param>
        /// <returns>a copy of the object to be cloned</returns>
        static public T Clone<T>(object pToClone)
        {
            T ret = default;

            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                serializer.Serialize(stream, pToClone);

                XmlSerializer deserializer = new XmlSerializer(typeof(T));

                ret = (T)deserializer.Deserialize(stream);
            }

            return ret;
        }

        public static T DeepClone<T>(T pToClone)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "pToClone");
            }

            if (pToClone == null)
            {
                return default;
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, pToClone);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Load a new instance of the given class type
        /// </summary>
        /// <typeparam name="T">class type</typeparam>
        /// <param name="pStream">source stream</param>
        /// <returns>a new instance of the given class type</returns>
        static public T LoadObjectFromFileStream<T>(FileStream pStream)
        {
            T ret;
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(T));

                ret = (T)deserializer.Deserialize(pStream);
            }
            finally { }

            return ret;
        }

        /// <summary>
        /// Serialize the input object into a file format XML
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="pObjectToSerialize">Object to serialize</param>
        /// <param name="pExeFolder"></param>
        /// <param name="pFileName"></param>
        /// <param name="pExtraTypes"></param>
        /// <param name="pAddCriticalException"></param>
        // /FI 20200623 [XXXXX] Add pAddException
        static public string DumpObjectToFile<T>(T pObjectToSerialize, SysMsgCode pSysMsgCode, string pExeFolder, string pFileName, Type[] pExtraTypes, AddCriticalException pAddCriticalException)
        {
            string path = SerializationHelper.SerializationDirectory;
            string fullpath = Path.Combine(path, pFileName);
            string temporaryPath = SerializationHelper.SerializationDirectory.Replace(pExeFolder, string.Empty);

            try
            {
                XmlSerializer serializer = null;

#if DEBUGDEV
                XmlSerializer deserializer = null;
#endif

                if (pExtraTypes != null && pExtraTypes.Length > 0)
                {
                    serializer = new XmlSerializer(typeof(T), pExtraTypes);
#if DEBUGDEV
                    deserializer = new XmlSerializer(typeof(T), pExtraTypes);
#endif
                }
                else
                {
                    serializer = new XmlSerializer(typeof(T));
#if DEBUGDEV
                    deserializer = new XmlSerializer(typeof(T));
#endif
                }

                using (FileStream stream = File.Create(fullpath))
                {
                    serializer.Serialize(stream, pObjectToSerialize);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, pSysMsgCode, 2, new LogParam(pExeFolder), new LogParam(temporaryPath), new LogParam(pFileName)));
                }

#if DEBUGDEV

                // Loading test
                using (FileStream stream = File.Open(fullpath, FileMode.Open))
                {
                    T objectToSerialize = default(T);

                    objectToSerialize = (T)deserializer.Deserialize(stream);
                }

#endif
            }
            catch (Exception ex)
            {
                string message = String.Format(Ressource.GetString("SerialisationException"), fullpath, typeof(T));
                SpheresException2 sEX = SpheresExceptionParser.GetSpheresException(message, ex);

                //FI 20200623 [XXXXX] call AddException
                pAddCriticalException.Invoke(sEX);

                
                Logger.Log(new LoggerData(sEX));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1018), 2));
            }
            return fullpath;

        }
    }
}