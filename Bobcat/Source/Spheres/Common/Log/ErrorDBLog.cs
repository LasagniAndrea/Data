#region Using Directives
using System;
using System.Data;
using System.Text;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
#endregion Using Directives

namespace EFS.Common.Log
{
    /// <summary>
    /// 
    /// </summary>
    public class ErrorDBLog2
    {
        private readonly string _cs;


        /// <summary>
        /// 
        /// </summary>
        /// FI 20210715 [XXXXX] Même si la colonne est dimmensionnée avec 4000 caractères, Spheres® n'alimente que les 3500 caractères   
        private int MaxLenghtMessage
        {
            get
            {
                int ret = SQLCst.UT_MESSAGE_LEN;
                if (DataHelper.IsDbOracle(_cs))
                    ret -= 500;

                return ret;
            }
        }

        public ErrorDBLog2(string pConnectionString)
        {
            _cs = pConnectionString;
        }
        public int Write(ErrorBlock pErrorBlock)
        {
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTSYS)); // FI 20201006 [XXXXX] Appel à GetParameter
            dp.Add(new DataParameter(_cs, "SEVERITY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            dp.Add(new DataParameter(_cs, "FUNCTIONNAME", DbType.AnsiString, 128));
            dp.Add(new DataParameter(_cs, "SOURCE", DbType.AnsiString, 64));
            dp.Add(new DataParameter(_cs, "MESSAGE", DbType.AnsiString, SQLCst.UT_MESSAGE_LEN));
            dp.Add(new DataParameter(_cs, "IDA", DbType.Int32));
            dp.Add(new DataParameter(_cs, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN));
            dp.Add(new DataParameter(_cs, "APPNAME", DbType.AnsiString, SQLCst.UT_APPNAME_LEN));
            dp.Add(new DataParameter(_cs, "APPVERSION", DbType.AnsiString, SQLCst.UT_APPVERSION_LEN));
            dp.Add(new DataParameter(_cs, "APPBROWSER", DbType.AnsiString, 64));
            dp.Add(new DataParameter(_cs, "URL", DbType.AnsiString, 1000));

            dp["DTSYS"].Value = pErrorBlock.DtError;
            dp["SEVERITY"].Value = pErrorBlock.Severity.ToString();
            dp["FUNCTIONNAME"].Value = pErrorBlock.Method;
            dp["SOURCE"].Value = pErrorBlock.Source;
            // FI 20210308 [XXXXX] Correction dans l'alimentation de SYSTEM_L
            //dp["MESSAGE"].Value = StrFunc.IsFilled(pErrorBlock.Message) ? pErrorBlock.Message.Substring(System.Math.Min(4000, pErrorBlock.Message.Length)) : Convert.DBNull;
            dp["MESSAGE"].Value = StrFunc.IsFilled(pErrorBlock.Message) ? pErrorBlock.Message.Substring(0, Math.Min(MaxLenghtMessage, pErrorBlock.Message.Length)) : @"N/A";
            dp["IDA"].Value = pErrorBlock.Session.IdA;
            dp["HOSTNAME"].Value = pErrorBlock.Session.AppInstance.HostName;
            dp["APPNAME"].Value = pErrorBlock.Session.AppInstance.AppName;
            dp["APPVERSION"].Value = pErrorBlock.Session.AppInstance.AppVersion;
            dp["APPBROWSER"].Value = pErrorBlock.Session.BrowserInfo;
            dp["URL"].Value = pErrorBlock.URL;

            string _SQLQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.SYSTEM_L.ToString() + @"(
							DTSYS, SEVERITY, SOURCE, MESSAGE, FUNCTIONNAME, IDA, 
							HOSTNAME, APPNAME, APPVERSION, APPBROWSER, URL) 
                            values 
                            (@DTSYS, @SEVERITY, @SOURCE, @MESSAGE, @FUNCTIONNAME, @IDA,
							@HOSTNAME, @APPNAME, @APPVERSION, @APPBROWSER, @URL)";

            int retValue = DataHelper.ExecuteNonQuery(_cs, CommandType.Text, _SQLQuery, dp.GetArrayDbParameter());

            return retValue;
        }
    }
}
