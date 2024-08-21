
using System;
using System.Collections;
using System.Reflection;  
using System.Data;
using System.Diagnostics;
using System.IO;
// 20091029 RD / PrintPDF, à reprendre plus tard
using System.Text;
using System.Threading;
using System.ComponentModel;

using EFS.Common;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EfsML.DynamicData;



namespace EFS.SpheresIO
{

    public abstract class ProcessShellBase
    {
        #region Members
        private readonly AppInstance _appInstance;
        protected string m_Cs;
        protected string m_IdShell;
        protected string m_SIFileName;
        protected string m_SIArguments;
        protected string m_SIWorkingDirectory;
        protected string m_SIStyle;
        protected string m_SIConnection;
        protected int m_IsSynchMode;
        protected int m_TimeOut;
        protected int m_ExitCodeSuccess;
        protected int m_ExitCodeWarning;
        protected int m_ExitCodeError;
        protected int m_ExitCodeTimeOut;

        protected Cst.StartInfoStyle _siStyle;
        protected ArrayList m_SIArgumentsParamData;

        #endregion Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdShell"></param>
        /// <param name="pAppInstance"></param>
        public ProcessShellBase(string pCS, string pIdShell, AppInstance pAppInstance)
        {
            m_Cs = pCS;
            m_IdShell = pIdShell;
            _appInstance = pAppInstance;

            Load();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Cst.ErrLevel Process()
        {
            // 20091029 RD / PrintPDF, à reprendre plus tard
            const int ERROR_FILE_NOT_FOUND = 2;
            const int ERROR_ACCESS_DENIED = 5;
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (_siStyle != Cst.StartInfoStyle.STOREDPROCEDURE)
            {
                #region verify shell file pathname
                if (StrFunc.IsEmpty(m_SIFileName) || m_SIFileName.ToLower().Contains("please insert"))
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                            @"<b>You forgot to specify the full pathname for the shell (" + m_IdShell + " [" + m_SIStyle + @"]).</b>
                                    Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                    (eg: \\server\share\directory\mybatch.bat or S:\directory\myscript.sql)",
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FILENOTFOUND));
                //
                if (false == File.Exists(m_SIFileName))
                {
                    m_SIFileName = _appInstance.GetFilepath(m_SIFileName);
                    //
                    if (false == File.Exists(m_SIFileName))
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                            @"<b>File (" + m_SIFileName + ") does not exist for the shell (" + m_IdShell + @" [" + m_SIStyle + @"]).</b>
                                        Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                        (eg: \\server\share\directory\mybatch.bat or S:\directory\myscript.sql)",
                            new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FILENOTFOUND));
                }
                #endregion
            }

            switch (_siStyle)
            {
                case Cst.StartInfoStyle.COMMANDFILE:
                case Cst.StartInfoStyle.EXECUTABLEFILE:
                    #region COMMANDFILE, EXECUTABLEFILE

                    #region verify shell Working Directory pathname
                    if (StrFunc.IsFilled(m_SIWorkingDirectory))
                    {
                        if (m_SIWorkingDirectory.ToLower().Contains("please insert"))
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                    @"<b>You forgot to specify the full pathname of working directory for the shell (" + m_IdShell + " [" + m_SIStyle + @"]).</b>
                                            Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                            (eg: \\server\share\directory or S:\directory)",
                                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FILENOTFOUND));
                        //
                        DirectoryInfo dInfo = new DirectoryInfo(m_SIWorkingDirectory);
                        if (false == dInfo.Exists)
                        {
                            m_SIWorkingDirectory = _appInstance.GetFilepath(m_SIWorkingDirectory);
                            //
                            dInfo = new DirectoryInfo(m_SIWorkingDirectory);
                            if (false == dInfo.Exists)
                                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                        @"<b>Working directory (" + m_SIWorkingDirectory + ") does not exist for the shell (" + m_IdShell + " [" + m_SIStyle + @"]).</b>
                                                Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                                (eg: \\server\share\directory or S:\directory)",
                                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FILENOTFOUND));
                        }
                    }
                    #endregion

                    // 20091029 RD / PrintPDF, à reprendre plus tard
                    // 20091028 RD Pour garder une compatibilité ascendante
                    bool isPrintPDF = m_IdShell.ToLower().Contains("print") && m_IdShell.ToLower().Contains("pdf");
                    //
                    System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
                    myProcess.StartInfo.WorkingDirectory = m_SIWorkingDirectory;
                    myProcess.StartInfo.FileName = m_SIFileName;
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    myProcess.StartInfo.CreateNoWindow = false;
                    //
                    // 20091029 RD / PrintPDF, à reprendre plus tard
                    myProcess.StartInfo.RedirectStandardOutput = true;
                    myProcess.StartInfo.RedirectStandardError = true;
                    //
                    if (isPrintPDF)
                    {
                        myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        myProcess.StartInfo.Verb = "Print"; ;
                    }
                    //20101104 FI 
                    //Avant UseShellExecute est positionne à false si  PDF
                    //Apres UseShellExecute est positionne à false si  PDF ou COMMANDFILE
                    //Cela permet de ne pas planter lorsque que le bat contient des instructions d'entrées/sortie (command Echo par ex)
                    if ((isPrintPDF) || (Cst.StartInfoStyle.COMMANDFILE == _siStyle))
                        myProcess.StartInfo.UseShellExecute = false;
                    //
                    myProcess.StartInfo.Arguments = GetDataParameter();
                    //
                    // 20091029 RD / PrintPDF, à reprendre plus tard
                    // 20091028 RD Pour un EXE les paramétres sont séparés par des blanc
                    if (_siStyle == Cst.StartInfoStyle.EXECUTABLEFILE && StrFunc.IsFilled(myProcess.StartInfo.Arguments))
                        myProcess.StartInfo.Arguments = myProcess.StartInfo.Arguments.Replace(";", " ");
                    //
                    string processLogInfo = "[Shell: " + m_IdShell + " (" + _siStyle.ToString() + ")]";
                    processLogInfo += Cst.CrLf + "[FileName: " + myProcess.StartInfo.FileName + "]";
                    processLogInfo += Cst.CrLf + "[Arguments: " + myProcess.StartInfo.Arguments + "]";
                    processLogInfo += Cst.CrLf + "[WorkingDirectory: " + myProcess.StartInfo.WorkingDirectory + "]";
                    //
                    try
                    {
                        myProcess.Start();
                    }
                    catch (Exception ex)
                    {

                        string exMsg = "<b>Error to start process</b>";
                        //
                        exMsg += processLogInfo + Cst.CrLf;
                        //
                        if (ex.GetType() == typeof(Win32Exception))
                        {
                            if (((Win32Exception)ex).NativeErrorCode == ERROR_FILE_NOT_FOUND)
                                exMsg += " - Check the path.";
                            else if (((Win32Exception)ex).NativeErrorCode == ERROR_ACCESS_DENIED)
                                exMsg += " - You do not have permission to access this file.";
                        }
                        throw new Exception(exMsg, ex);
                    }
                    // EG 20160404 Migration vs2013
                    // #warning 20101104 FI Spheres® peut finir en infinite Loop, il faudrait définir un timeout même si le paramétrage n'en dispose pas
                    if (false == myProcess.HasExited)
                    {
                        if (m_TimeOut == 0)
                            myProcess.WaitForExit();
                        else
                            myProcess.WaitForExit(m_TimeOut * 1000);
                    }
                    //
                    if (false == myProcess.HasExited)
                    {
                        // 20091029 RD / PrintPDF, à reprendre plus tard
                        if (isPrintPDF)
                            myProcess.CloseMainWindow();
                        else
                            ret = Cst.ErrLevel.TIMEOUT;
                        //
                        myProcess.Kill();
                    }
                    // 20091029 RD / PrintPDF, à reprendre plus tard
                    else
                    {
                        StringBuilder buffer = new StringBuilder();
                        //
                        using (StreamReader reader = myProcess.StandardOutput)
                        {
                            if (false == reader.EndOfStream)
                            {
                                buffer.Append("Output: ");
                                //
                                string line = reader.ReadLine();
                                while (line != null)
                                {
                                    buffer.Append(line);
                                    buffer.Append(Environment.NewLine);
                                    line = reader.ReadLine();
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        //
                        using (StreamReader reader = myProcess.StandardError)
                        {
                            if (false == reader.EndOfStream)
                            {
                                buffer.Append("Error: ");
                                //
                                string line = reader.ReadLine();
                                while (line != null)
                                {
                                    buffer.Append(line);
                                    buffer.Append(Environment.NewLine);
                                    line = reader.ReadLine();
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        //
                        if (myProcess.ExitCode != 0)
                        {
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                string.Format(@"Process exited with ExitCode: {0}" + Cst.CrLf + "{1}" + Cst.CrLf + processLogInfo,
                                myProcess.ExitCode, buffer.ToString()),
                                new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE));
                        }
                    }
                    //
                    myProcess.Close();
                    #endregion
                    break;

                case Cst.StartInfoStyle.SQLFILE:
                    //PL 20110916 NON TESTE
                    //string sqlQuery = string.Empty; //GetSqlQueryFromFile();
                    string sqlQuery = File.ReadAllText(m_SIFileName);
                    DataHelper.ExecuteNonQuery(m_Cs, CommandType.Text, sqlQuery, GetSqlParameter().GetArrayDbParameter());
                    break;

                case Cst.StartInfoStyle.STOREDPROCEDURE:
                    DataHelper.ExecuteNonQuery(m_Cs, CommandType.StoredProcedure, m_SIFileName, GetSqlParameter().GetArrayDbParameter());
                    break;
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual DataParameters GetSqlParameter()
        {
            DataParameters ret = new DataParameters();
            if (null != m_SIArgumentsParamData)
            {
                foreach (ParamData paramData in m_SIArgumentsParamData)
                    ret.Add(paramData.GetDataParameter(m_Cs, null, GetSqlCommandType()));
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDataParameter()
        {
            ArrayList retList = new ArrayList();
            if (null != m_SIArgumentsParamData)
            {
                foreach (ParamData paramData in m_SIArgumentsParamData)
                    retList.Add(paramData.GetDataParameter(m_Cs, null, CommandType.Text).Value);
            }
            return ArrFunc.GetStringList(retList, ";");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected CommandType GetSqlCommandType()
        {
            CommandType ret;
            switch (_siStyle)
            {
                case Cst.StartInfoStyle.STOREDPROCEDURE:
                    ret = CommandType.StoredProcedure;
                    break;
                case Cst.StartInfoStyle.SQLFILE:
                    ret = CommandType.Text;
                    break;
                default:
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Incorrect CommandType");
            }
            return ret;
        }




        /// <summary>
        /// 
        /// </summary>
        /// EG 20180426 Analyse du code Correction [CA2202]
        private void Load()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.IDENTIFIER), m_IdShell);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "IDIOSHELL,DISPLAYNAME, DESCRIPTION," + Cst.CrLf;
            sqlQuery += "SIFILENAME, SIARGUMENTS, SIWORKINGDIRECTORY,SISTYLE, SICONNECTION," + Cst.CrLf;
            sqlQuery += "ISSYNCHMODE, TIMEOUT," + Cst.CrLf;
            sqlQuery += "EXITCODESUCCESS, EXITCODEWARNING, EXITCODEERROR, EXITCODETIMEOUT" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOSHELL + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + " IDIOSHELL = @IDENTIFIER" + Cst.CrLf;
            sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Cs, Cst.OTCml_TBL.IOSHELL);

            using (IDataReader drData = DataHelper.ExecuteReader(m_Cs, CommandType.Text, sqlQuery.ToString(), dataParameters.GetArrayDbParameter()))
            {
                if (drData.Read())
                {
                    #region Préparer les données de Shell

                    m_SIFileName = drData["SIFILENAME"].ToString();
                    m_SIConnection = (Convert.IsDBNull(drData["SICONNECTION"]) ? null : drData["SICONNECTION"].ToString());
                    m_Cs = (StrFunc.IsFilled(m_SIConnection) ? m_SIConnection : m_Cs);

                    m_SIArguments = (Convert.IsDBNull(drData["SIARGUMENTS"]) ? null : drData["SIARGUMENTS"].ToString());

                    m_SIWorkingDirectory = (Convert.IsDBNull(drData["SIWORKINGDIRECTORY"]) ? null : drData["SIWORKINGDIRECTORY"].ToString());
                    m_SIStyle = drData["SISTYLE"].ToString();
                    m_IsSynchMode = Convert.ToInt32(drData["ISSYNCHMODE"]);
                    m_TimeOut = Convert.ToInt32(drData["TIMEOUT"]);
                    m_ExitCodeSuccess = (Convert.IsDBNull(drData["EXITCODESUCCESS"]) ? 0 : Convert.ToInt32(drData["EXITCODESUCCESS"]));
                    m_ExitCodeWarning = (Convert.IsDBNull(drData["EXITCODEWARNING"]) ? 0 : Convert.ToInt32(drData["EXITCODEWARNING"]));
                    m_ExitCodeError = (Convert.IsDBNull(drData["EXITCODEERROR"]) ? 0 : Convert.ToInt32(drData["EXITCODEERROR"]));
                    m_ExitCodeTimeOut = (Convert.IsDBNull(drData["EXITCODETIMEOUT"]) ? 0 : Convert.ToInt32(drData["EXITCODETIMEOUT"]));
                    #endregion

                    _siStyle = Cst.StartInfoStyle.COMMANDFILE;
                    if (StrFunc.IsFilled(m_SIStyle))
                        _siStyle = (Cst.StartInfoStyle)System.Enum.Parse(typeof(Cst.StartInfoStyle), m_SIStyle, true);
                    else
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Shell style not specified");

                    LoadArgumentParamData();

                }
                else
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, m_IdShell);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void LoadArgumentParamData()
        {
            if (StrFunc.IsFilled(m_SIArguments))
            {
                m_SIArgumentsParamData = new ArrayList();
                string[] arguments = m_SIArguments.Split(';');
                for (int i = 0; i < arguments.Length; i++)
                {
                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(ParamData), arguments[i].Trim());
                    ParamData data = (ParamData)CacheSerializer.Deserialize(serializeInfo);
                    m_SIArgumentsParamData.Add(data);
                }
            }
        }

    }

}
