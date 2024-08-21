using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace EFS.ACommon
{

    /// <summary>
    /// Helper class to parse a generic CLR exception and build the relative SpheresException
    /// </summary>
    public static class SpheresExceptionParser
    {
        /// <summary>
        /// RegEx finding all the spheres namespaces
        /// </summary>
        /// <remarks>in case you want to add more prefixes, you can add another pipe condition in the first capturing group</remarks>
        static readonly Regex regEFSExNameSpaces = new Regex(@"^(?:EFS|EfsML|SpheresProcessBase|SpheresServiceBase){1}(?:\.\w+)+$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Get the SpheresException instance which boxes the exception input parameter
        /// </summary>
        /// <param name="pMessage">Custom exception message, it could be empty</param>
        /// <param name="pEx">Exception to parse, not null</param>
        public static SpheresException2 GetSpheresException(string pMessage, Exception pEx)
        {
            if (null == pEx)
                throw new ArgumentNullException("Exception paramter is null");

            SpheresException2 ret;

            if (pEx is SpheresException2 exception)
            {
                ret = exception;
            }
            else
            {
                string methodName = GetFirstEFSMethodFromStack(pEx);

                if (StrFunc.IsFilled(pMessage))
                    ret = new SpheresException2(methodName, pMessage, pEx);
                else
                    ret = new SpheresException2(methodName, pEx);
            }

            return ret;
        }

        /// <summary>
        /// Get the first EFS method in the stacktrace of the given exception 
        /// </summary>
        /// <param name="pEx">Exception to parse, not null</param>
        /// <returns>the first method  of the exceptions stacktrace</returns>
        private static string GetFirstEFSMethodFromStack(Exception pEx)
        {
            string methodName = Cst.NotAvailable;
            StackTrace stack = new StackTrace(pEx);
            StackFrame[] frames = stack.GetFrames();
            // FI 20120911 add if (ArrFunc.IsFilled(frames))
            if (ArrFunc.IsFilled(frames))
            {
                foreach (StackFrame frame in frames)
                {
                    MethodBase method = frame.GetMethod();
                    if (null != method.ReflectedType)
                    {
                        string methodNameSpace = method.ReflectedType.Namespace;
                        // FI 20210112 [XXXXX] add StrFunc.IsFilled(methodNameSpace) pour ne pas palnter dans IsEFSNameSpace
                        bool ok = StrFunc.IsFilled(methodNameSpace) && IsEFSNameSpace(methodNameSpace);
                        if (ok)
                        {
                            string @className = method.ReflectedType.FullName.Replace(method.ReflectedType.Namespace + ".", string.Empty);
                            methodName = String.Format("{0}.{1}", @className, method.Name);
                            break;
                        }
                    }
                }
            }
            return methodName;
        }

        private static bool IsEFSNameSpace(string methodNameSpace)
        {
            Match match = regEFSExNameSpaces.Match(methodNameSpace);

            return match.Success;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// EG 20180423 Analyse du code Correction [CA2240]
    [Serializable]
    public class SpheresException2 : SystemException
    {
        #region Members
        private readonly ProcessState m_ProcessState;
        private readonly string m_method;
        private readonly int m_LevelOrder;
        private readonly string[] m_data = new string[10] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                                   string.Empty, string.Empty, string.Empty, string.Empty, string.Empty};

        private string m_messageExtended;

        #endregion Members
        #region Accessors

        /// <summary>
        /// Obtient le niveau d'indentation dans le log
        /// </summary>
        public int LevelOrder { get { return m_LevelOrder; } }

        /// <summary>
        /// Obtient la méthode qui a généré l'exception
        /// </summary>
        public string Method { get { return m_method; } }

        /// <summary>
        /// Obtient true si l'exception a pour source une exception
        /// </summary>
        public bool IsInnerException { get { return (null != InnerException); } }
        /// <summary>
        /// 
        /// </summary>
        public ProcessState ProcessState { get { return m_ProcessState; } }
        /// <summary>
        /// La ligne est en erreur
        /// </summary>
        public bool IsStatusError { get { return ProcessStateTools.IsStatusError(m_ProcessState.Status); } }

        /// <summary>
        ///  Obtient un message étendu  (=> Message + Messages de toutes les InnerExceptions + Stack + Stack de toutes les InnerExceptions)
        /// </summary>
        /// FI 20200910 [XXXXX] Add
        public string MessageExtended
        {
            get
            {
                if (StrFunc.IsEmpty(m_messageExtended))
                    m_messageExtended = BuidExtendMessage();
                return m_messageExtended;
            }
        }
        /// <summary>
        ///  Obtient les datas associés à l'exeption
        /// </summary>
        /// FI 20220719 [XXXXX] Add
        public string[] ParamData
        {
            get { return m_data; }
        }
        #endregion Accessors

        #region Constructors
        #region SpheresException à partir d'une exception
        /// <summary>
        ///  Nouvelle exception avec propriété ProcessState (Status:Error et CodeReturn:Failure)
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pException"></param>
        public SpheresException2(string pMethod, Exception pException)
            : this(pMethod, 0, null, new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum), pException) { }

        /// <summary>
        ///  Nouvelle exception avec propriété ProcessState (Status:Error et CodeReturn:Failure)
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pMessage"></param>
        /// <param name="pException"></param>
        public SpheresException2(string pMethod, string pMessage, Exception pException)
            : this(pMethod, 0, pMessage, new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum), pException) { }

        /// <summary>
        ///  Nouvelle exception avec propriété ProcessState (Status:Error et CodeReturn:Failure)
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pException"></param>
        /// <param name="pData"></param>
        public SpheresException2(string pMethod, string pMessage, Exception pException, params object[] pData)
            : this(pMethod, 0, pMessage, new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum), pException, pData) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pMessage"></param>
        /// <param name="pProcessState"></param>
        /// <param name="pException"></param>
        /// <param name="pData"></param>
        /// FI 20111125 Modification de l'alimentation de m_method
        public SpheresException2(string pMethod, string pMessage, ProcessState pProcessState, Exception pException, params object[] pData)
            : this(pMethod, 0, pMessage, pProcessState, pException, pData) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pLevelOrder"></param>
        /// <param name="pMessage"></param>
        /// <param name="pProcessState"></param>
        /// <param name="pException"></param>
        /// <param name="pData"></param>
        /// FI 20180320 [XXXXX] Modify
        public SpheresException2(string pMethod, int pLevelOrder, string pMessage, ProcessState pProcessState, Exception pException, params object[] pData)
            : base(pMessage, pException)
        {
            m_ProcessState = pProcessState;
            m_method = pMethod;

            if (pException is SpheresException2 exception)
            {
                if (0 == pLevelOrder)
                {
                    m_LevelOrder = exception.LevelOrder;
                }
                // PM 20210503 [XXXXX] Gestion message SysCode passé en ajout dans la string Message sur une SpheresException2
                m_data = exception.m_data;
            }
            else
            {
                m_LevelOrder = pLevelOrder;
            }

            if (null != pData)
            {
                SetData(pData);
            }
        }
        #endregion

        #region SpheresException simple (les éventuels arguments, de type string, rentrent dans la constitution du message de l'exception)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pMessage"></param>
        /// <param name="pProcessState"></param>
        public SpheresException2(string pMethod, string pMessage, ProcessState pProcessState)
            : this(pMethod, 0, pMessage, pProcessState, string.Empty) { }
        
        /// <summary>
        ///  Nouvelle exception avec propriété ProcessState (Status:Error et CodeReturn:Failure)
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pMessage"></param>
        /// <param name="pArgs"></param>
        public SpheresException2(string pMethod, string pMessage, params string[] pArgs)
            : this(pMethod, 0, pMessage, new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum), pArgs) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pLevelOrder"></param>
        /// <param name="pMessage"></param>
        /// <param name="pArgs"></param>
        public SpheresException2(string pMethod, int pLevelOrder, string pMessage, params string[] pArgs)
            : this(pMethod, pLevelOrder, pMessage, new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum), pArgs) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pProcessState"></param>
        /// <param name="pMessage"></param>
        /// <param name="pArgs"></param>
        public SpheresException2(string pMethod, string pMessage, ProcessState pProcessState, params string[] pArgs)
            : this(pMethod, 0, pMessage, pProcessState, pArgs) { }
        public SpheresException2(string pMethod, int pLevelOrder, string pMessage, ProcessState pProcessState, params string[] pArgs)
            : base((ArrFunc.IsFilled(pArgs)) ? new StringBuilder().AppendFormat(pMessage, pArgs).ToString() : pMessage)
        {
            m_ProcessState = pProcessState;
            m_method = pMethod;
            m_LevelOrder = pLevelOrder;

            if (ArrFunc.IsFilled(pArgs))
                SetData(pArgs);
        }
        #endregion

        #region SpheresException simple (les éventuels arguments, de type object, ne rentrent dans la constitution du message de l'exception)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pProcessState"></param>
        /// <param name="pMessage"></param>
        /// <param name="pData"></param>
        public SpheresException2(string pMethod, string pMessage, ProcessState pProcessState, params object[] pData)
            : base(pMessage)
        {
            // Alimenation de message + SetData  
            m_ProcessState = pProcessState;
            m_method = pMethod;
            if (null != pData)
                SetData(pData);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessState"></param>
        public SpheresException2(ProcessState pProcessState)
        {
            m_ProcessState = pProcessState;
        }

        #endregion Constructors
        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        private void SetData(params object[] pData)
        {
            for (int i = 0; i < ArrFunc.Count(pData); i++)
            {
                object data = pData.GetValue(i);
                if (null != data)
                {
                    Type tData = data.GetType();
                    if (tData.IsArray)
                    {
                        Array aData = (Array)data;
                        string dataMsg = (string)aData.GetValue(0);
                        object[] dataArgs = new object[aData.Length - 1];
                        for (int j = 1; j < aData.Length; j++)
                            dataArgs.SetValue(aData.GetValue(j), j - 1);

                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat(dataMsg, dataArgs);
                        m_data.SetValue(sb.ToString(), i);
                    }
                    else
                    {
                        if (null != data)
                            m_data.SetValue(data.ToString(), i);
                    }
                }
            }
        }

        /// <summary>
        /// Retourne un array avec les infomations sur l'exception
        /// <para>[0]    GetMsgHeader + Cst.CrLf2 + Message étendu</para>
        /// <para>[1..n] les infos datas (facultatif)</para>
        /// <para>[n+1]    "[" + Method + "]"  (obligatoire)</para>
        /// </summary>
        /// <returns></returns>
        public string[] GetLogInfo()
        {
            // RD/FI mettre "[" + Method + "]" à la fin de la liste
            ArrayList arrLog = new ArrayList();

            string msg = string.Empty;
            msg += GetMsgHeader() + Cst.CrLf2;
            // FI 20200910 [XXXXX] Appel à MessageExtended
            msg += MessageExtended;

            /* FI 20200910 [XXXXX] Il ne faut pas faire appel à ExceptionTools.GetStackExtended sur une SpheresException car cette 
            // RD 20190917 [24948] Pour avoir plus de détails dans le message de Log
            if (this.IsStatusError)
                msg += Cst.CrLf2 + ExceptionTools.GetStackExtended(this);
            */

            arrLog.Add(msg);                        //data[0]

            // RD 20180108 [23705] Pour ne pas ajouter la Méthode dans l'élément n+1
            //for (int i = 0; i < m_data.Length; i++) //data[1 à n]
            //    arrLog.Add(m_data.GetValue(i));
            for (int i = 0; i < m_data.Length; i++) //data[1 à n]
            {
                if (StrFunc.IsFilled(m_data[i]))
                    arrLog.Add(m_data[i]);
            }

            arrLog.Add("Method [" + Method + "]");  //data[n+1]            

            return (string[])arrLog.ToArray(typeof(string));
        }

        /// <summary>
        /// Retourne "[" + CodeReturn + "]"
        /// </summary>
        /// <returns></returns>
        protected virtual string GetMsgHeader()
        {
            return "[Code Return:" + Cst.Space + m_ProcessState.CodeReturn + Cst.Space + "]";
        }

        // EG 20180425 Analyse du code Correction [CA2240]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("processState", m_ProcessState);
            info.AddValue("method", m_method);
            info.AddValue("MessageExtended", MessageExtended);
            info.AddValue("levelOrder", m_LevelOrder);
            info.AddValue("data", m_data);

            base.GetObjectData(info, context);
        }

        /// <summary>
        /// retourne un message étendu  (Message + Messages de toutes les Exceptions inner + Stack + Stack de tous les Exceptions inner)
        /// </summary>
        /// <returns></returns>
        private string BuidExtendMessage()
        {
            return ExceptionTools.GetMessageAndStackExtended(this);
        }
        #endregion
    }
}
