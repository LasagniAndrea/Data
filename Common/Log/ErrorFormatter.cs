using System;
using System.Text;

using EFS.ACommon;  

namespace EFS.Common.Log
{
	/// <summary>
	/// Classe ErrorFormatter used to format text to be sent to various output devices
	/// </summary>
	public class ErrorFormatter : ErrorLogFormatter
	{
		#region Constructors
		/// <summary>
		/// Constructeur
		/// </summary>
		/// <param name="pErrorBlock"></param>
		public ErrorFormatter(ErrorBlock pErrorBlock)
		{
			if (pErrorBlock != default(ErrorBlock))
			{
				m_ErrorLogInfo = new ErrorLogInfo();
				if (pErrorBlock.Session != default(AppSession))
				{
					m_ErrorLogInfo.AppName = pErrorBlock.Session.AppInstance.AppName;
					m_ErrorLogInfo.AppVersion = pErrorBlock.Session.AppInstance.AppVersion;
					m_ErrorLogInfo.BrowserInfo = pErrorBlock.Session.BrowserInfo;
					m_ErrorLogInfo.IdA_Identifier = pErrorBlock.Session.IdA_Identifier;
				}
				m_ErrorLogInfo.DtError = pErrorBlock.DtError;
				m_ErrorLogInfo.Message = pErrorBlock.Message;
				m_ErrorLogInfo.Method = pErrorBlock.Method;
				m_ErrorLogInfo.Severity = pErrorBlock.Severity;
				m_ErrorLogInfo.Source = pErrorBlock.Source;
				m_ErrorLogInfo.URL = pErrorBlock.URL;
			}
		}
		#endregion Constructors
	}
}
