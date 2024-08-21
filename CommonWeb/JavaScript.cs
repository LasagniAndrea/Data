using EFS.ACommon;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    /// <summary>
    /// Description résumée de JavaScript.
    /// </summary>
    /// EG 20161122 Add partial
    // EG 20200914 [XXXXX] Suppression de méthodes obsolètes
    // EG 20201014 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
    public sealed partial class JavaScript
	{
		#region Constants
		public const string
			JS_NULL = @"null",
			JS_CrLf = @"\r\n",
			JS_AND = @" && ",
			JS_OR = @" || ";
		#endregion Constants

		#region DefautHeight
		public static Pair<int?, int?> DefautHeight
		{
			get { return new Pair<int?, int?>(0, 0); }
		}
		#endregion DefautHeight
		#region DefautWidth
		public static Pair<int?, int?> DefautWidth
		{
			get { return new Pair<int?, int?>(0, 0); }
		}
		#endregion DefautWidth

		#region Constructor
		public JavaScript() { }
		#endregion Constructor

		#region Type de valeurs pour <Script> name
		public static bool IsScriptTypeEFS_Enabled(string pType)
		{
			return (pType == "EFS_Enabled");
		}
		public static bool IsScriptTypeEFS_Disabled(string pType)
		{
			return (pType == "EFS_Disabled");
		}
		public static bool IsScriptTypeEnabledDisabled(string pType)
		{
			return (IsScriptTypeEFS_Enabled(pType) || IsScriptTypeEFS_Disabled(pType));
		}
		public static bool IsScriptTypeEFS_Copy(string pType)
		{
			return (pType == "EFS_Copy");
		}
		public static bool IsScriptTypeEFS_Reset(string pType)
		{
			return (pType == "EFS_Reset");
		}
		public static bool IsScriptTypeEFS_Set(string pType)
		{
			return (pType == "EFS_Set");
		}
		public static bool IsScriptTypeEFS_Upper(string pType)
		{
			return (pType == "EFS_Upper");
		}
		public static bool IsScriptTypeEFS_Lower(string pType)
		{
			return (pType == "EFS_Lower");
		}
		public static bool IsScriptTypeEFS_1stUpper(string pType)
		{
			return (pType == "EFS_1stUpper");
		}
		public static bool IsScriptTypeEFS_Status(string pType)
		{
			return (pType == "EFS_Status");
		}
		public static bool IsScriptTypeEFS_StatusHelp(string pType)
		{
			return (pType == "EFS_StatusHelp");
		}
		public static bool IsScriptTypeEFS_ApplyOffset(string pType)
		{
			return (pType.StartsWith("EFS_ApplyOffset"));
		}
		#endregion

		#region HTMLBlockUIMessage
		//EG 20120613 BlockUI New
		public static string HTMLBlockUIMessage(Page pPage, string pMessage)
		{
			string message = @"<h3 class=""blockUI""><img src=""" + pPage.Request.ApplicationPath + @"/images/blockUI_loader.gif"" />" + pMessage + "</h3>";
			return HTMLString(message);
		}
		#endregion HTMLBlockUIMessage

		#region HTMLString
		public static string HTMLString(string pString)
		{
			string ret = pString;
			ret = ret.Replace(Cst.CrLf, Cst.HTMLBreakLine);
			ret = ret.Replace(JS_CrLf, Cst.HTMLBreakLine);
			ret = ret.Replace(@"'", @"\x27");     //Replace simple-quote par son code hex
			return "'" + ret + "'";
		}
		public static string HTMLStringCrLf(string pString)
		{
			string ret = pString;
			ret = ret.Replace(Cst.CrLf, Cst.HTMLBreakLine);
			ret = ret.Replace(Cst.Lf, Cst.HTMLBreakLine);
			ret = ret.Replace(JS_CrLf, Cst.HTMLBreakLine);
			return ret;
		}

		#endregion HTMLString

		#region JSValidString
		public static string JSValidString(string pString)
		{
			string ret = pString;
			ret = ret.Replace(@"\", @"\x5C");     //Replace anti-slash par son code hex
			ret = ret.Replace(@"""", @"\x22");    //Replace double-quote par son code hex
			ret = ret.Replace(Cst.CrLf, JS_CrLf); //Replace crlf C# par son équivalent JS
			ret = ret.Replace(Cst.HTMLBreakLine, JS_CrLf); //Replace Html <br/> par son équivalent JS
														   //20051011 PL ajout de la ligne suivante
			ret = ret.Replace(@"'", @"\x27");     //Replace simple-quote par son code hex
			return ret;
		}
		#endregion JSValidString
		#region JSString
		public static string JSString(string pString)
		{
			return "'" + JSValidString(pString) + "'";
		}
		#endregion JSString
		#region JSDblQuote
		public static string JSDblQuote(string pString)
		{
			return @"""" + pString.Replace(@"""", @"""""") + @"""";
		}
		#endregion JSDblQuote
		#region JSNoQuote
		public static string JSNoQuote(string pString)
		{
			return "'" + pString.Replace("'", " ") + "'";
		}
		#endregion JSNoQuote

		#region JSStringBuilder
		public class JSStringBuilder
		{
			#region Members
			private readonly StringBuilder _sbScript;
			#endregion Members
			#region Constructor
			public JSStringBuilder()
				: this("javascript")
			{
			}
			public JSStringBuilder(string pScriptLanguage)
			{
				_sbScript = new StringBuilder();
				_sbScript.Append("\n<script type=\"text/" + pScriptLanguage + "\">\n");
				_sbScript.Append("<!--\n");
			}
			#endregion Constructor
			#region Methods
			#region AppendLine
			public void AppendLine(string line)
			{
				this.Append(line + "\n");
			}
			#endregion AppendLine
			#region Append
			public void Append(string line)
			{
				_sbScript.Append(line);
			}
			#endregion Append
			#region ToString
			public override string ToString()
			{
				string ret;
				ret = string.Empty;
				if (_sbScript.Length > 0)
				{
					_sbScript.Append("\n// -->");
					_sbScript.Append("\n</script>");
					ret = _sbScript.ToString();
				}
				return ret;
			}
			#endregion ToString
			#endregion Methods
		}
		#endregion

		#region JavaScriptScript
		// Class Javascript pour la serialization utilisée dans XMLReferencial.cs
		public class JavaScriptScript
		{
			#region Members
			[System.Xml.Serialization.XmlTextAttribute()]
			public string Script;
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string name;
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string attribut;
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string control;
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string data;
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string condition;
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string resdata;
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string rescondition;
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public string[] Jscript;
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public string[] aAttribut;
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public string[] aControl;
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public string[] aData;
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public string[] aCondition;
			#endregion Members
		}
		#endregion

		#region ConfirmOnLoad,ConfirmOnStartUp
		/// <summary>
		/// Enregistre un script fait appel au confirm natif de Javascript, la page sera publiée via un __doPostBack
		/// <para>EventArg du __doPostBack est TRUE si confirmation</para>
		/// <para>EventArg du __doPostBack est FALSE si non confirmation</para>
		/// </summary>
		public static void ConfirmOnLoad(PageBase pPage, string pEventTarget, string pMessage, bool pPostbaskOnConfirmOnly)
		{
			ConfirmMain(pPage, pEventTarget, pMessage, "ConfirmOnLoad", false, pPostbaskOnConfirmOnly);
		}
		/// <summary>
		/// Enregistre un script fait appel au confirm natif de Javascript, la page sera publiée via un __doPostBack
		/// <para>EventArg du __doPostBack est TRUE si confirmation</para>
		/// <para>EventArg du __doPostBack est FALSE si non confirmation</para>
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pEventTarget"></param>
		/// <param name="pMessage"></param>
		public static void ConfirmOnLoad(PageBase pPage, string pEventTarget, string pMessage)
		{
			ConfirmMain(pPage, pEventTarget, pMessage, "ConfirmOnLoad", false, false);
		}
		/// <summary>
		/// Enregistre un script fait appel au confirm natif de Javascript, la page sera publiée via un __doPostBack
		/// <para>EventArg du __doPostBack est TRUE si confirmation</para>
		/// <para>EventArg du __doPostBack est FALSE si non confirmation</para>
		/// <para>Script exécuté en OnStartUp</para>
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pEventTarget"></param>
		/// <param name="pMessage"></param>
		/// <param name="pPostbaskOnConfirmOnly"></param>
		public static void ConfirmOnStartUp(PageBase pPage, string pEventTarget, string pMessage, bool pPostbaskOnConfirmOnly)
		{
			ConfirmMain(pPage, pEventTarget, pMessage, "ConfirmOnStartUp", true, pPostbaskOnConfirmOnly);
		}
		/// <summary>
		/// Enregistre un script fait appel au confirm natif de Javascript, la page sera publiée via un __doPostBack
		/// <para>EventArg du __doPostBack est TRUE si confirmation</para>
		/// <para>EventArg du __doPostBack est FALSE si non confirmation</para>
		/// <para>Script exécuté en OnStartUp</para>
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pEventTarget"></param>
		/// <param name="pMessage"></param>
		public static void ConfirmOnStartUp(PageBase pPage, string pEventTarget, string pMessage)
		{
			ConfirmMain(pPage, pEventTarget, pMessage, "ConfirmOnStartUp", true, false);
		}
		/// <summary>
		/// Enregistre un script fait appel au confirm natif de Javascript, la page sera publiée via un __doPostBack
		/// <para>EventArg du __doPostBack est TRUE si confirmation</para>
		/// <para>EventArg du __doPostBack est FALSE si non confirmation</para>
		/// <para>Script exécuté en OnStartUp ou enregistré</para>
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pEventTarget">EventTarget du __doPostBack</param>
		/// <param name="pMessage">Message assoicé à la confirmation</param>
		/// <param name="pNameFunction"></param>
		/// <param name="pIsOnStartUp">Script exécuté en OnStartUp ou inscrit uniquement</param>
		/// <param name="pPostbaskOnConfirmOnly"></param>
		private static void ConfirmMain(PageBase pPage, string pEventTarget, string pMessage, string pNameFunction, bool pIsOnStartUp, bool pPostbaskOnConfirmOnly)
		{
			string script;
			JSStringBuilder sb = new JSStringBuilder();
			//
			sb.AppendLine("function " + pNameFunction + "()");
			sb.AppendLine("{");
			sb.AppendLine("  var ret=confirm(" + JSString(pMessage) + ");");
			sb.AppendLine("  if(ret)");
			sb.AppendLine("    {javascript:__doPostBack('" + pEventTarget + "','TRUE');}");
			if (false == pPostbaskOnConfirmOnly)
			{
				sb.AppendLine("  else");
				sb.AppendLine("  {javascript:__doPostBack('" + pEventTarget + "','FALSE');}");
			}
			sb.AppendLine("}");
			if (pIsOnStartUp)
				sb.AppendLine(pNameFunction + "();");
			else
				sb.AppendLine("window.onload  = function(){" + pNameFunction + "();}");
			script = sb.ToString();
			pPage.RegisterScript(pNameFunction, script, pIsOnStartUp);
		}
		#endregion

		#region DoPostBackImmediate
		//public static void DoPostBackImmediate(PageBase pPage, string pTarget, string pArgument)
		//{
		//	string script;
		//	string nameFunction = "DoPostBackImmediate";
		//	JSStringBuilder sb = new JSStringBuilder();
		//	//
		//	sb.AppendLine("function " + nameFunction + "()");
		//	sb.AppendLine("{				");
		//	sb.AppendLine("javascript:__doPostBack('" + pTarget + "','" + pArgument + "');");
		//	sb.AppendLine("}");
		//	sb.AppendLine(nameFunction + "();");
		//	script = sb.ToString();
		//	//
		//	pPage.RegisterScript(nameFunction, script);
		//}
		#endregion

		#region ConfirmImmediate
		private const string
			KEYCONFIRM_UPIMMEDIATE = "ConfirmStartUpImmediate",
			KEYCONFIRM_IMMEDIATE = "ConfirmImmediate";

		public static void ConfirmImmediate(PageBase pPage, string pMessage)
		{
			ConfirmImmediate(pPage, pMessage, string.Empty, string.Empty);
		}
		public static void ConfirmImmediate(PageBase pPage, string pMessage, string pPostBack_Target, string pPostBack_Argument)
		{
			string script = GetConfirmScript(pPage.PageTitle, JSValidString(pMessage),
				pPostBack_Target, pPostBack_Argument, string.Empty);
			pPage.RegisterScript(GetKey(false), script);
		}
		public static void ConfirmStartUpImmediate(PageBase pPage, string pMessage)
		{
			ConfirmStartUpImmediate(pPage, pMessage, string.Empty, string.Empty, string.Empty);
		}
		public static void ConfirmStartUpImmediate(PageBase pPage, string pMessage, string pPostBack_Target, string pPostBack_Argument)
		{
			ConfirmStartUpImmediate(pPage, pMessage, pPostBack_Target, pPostBack_Argument, string.Empty);
		}
		public static void ConfirmStartUpImmediate(PageBase pPage, string pMessage, string pPostBack_Target, string pPostBack_ArgumentOk, string pPostBack_ArgumentCancel)
		{
			string script = GetConfirmScript(pPage.PageTitle, JSValidString(pMessage),
				pPostBack_Target, pPostBack_ArgumentOk, pPostBack_ArgumentCancel);
			pPage.RegisterScript(GetKey(true), script, true);
		}
		public static void ConfirmStartUpImmediate(PageBase pPage, string pMessage, string pStatus, string pPostBack_Target, string pPostBack_ArgumentOk, string pPostBack_ArgumentCancel)
		{
			string script = GetConfirmScript(pPage.PageTitle, JSValidString(pMessage), pStatus,
				pPostBack_Target, pPostBack_ArgumentOk, pPostBack_ArgumentCancel);
			pPage.RegisterScript(GetKey(true), script, true);
		}

		public static void TransferStartUpImmediate(PageBase pPage, string pMessage, string pPostBack_Target, string pPostBack_ArgumentOk)
		{
			TransferStartUpImmediate(pPage, pMessage, pPostBack_Target, pPostBack_ArgumentOk, string.Empty);
		}
		public static void TransferStartUpImmediate(PageBase pPage, string pMessage, string pPostBack_Target, string pPostBack_ArgumentOk, string pPostBack_ArgumentCancel)
		{
			string script = GetTransferScript(pPage.PageTitle, JSValidString(pMessage),
				pPostBack_Target, pPostBack_ArgumentOk, pPostBack_ArgumentCancel);
			pPage.RegisterScript(GetKey(true), script, true);
		}

		public static void ReloadImmediate(PageBase pPage, string pMessage, string pPostBack_Target, string pPostBack_Argument)
		{

			JSStringBuilder sbjss = new JSStringBuilder();

			sbjss.AppendLine(String.Format(@"OpenDialogAndReload({0}, {1}, {2}, 0, 0, {3}, {4});",
				JSString(pPage.PageTitle), HTMLString(pMessage), JSString("confirm"), JSString(pPostBack_Target), JSString(pPostBack_Argument)));

			string script = sbjss.ToString();

			pPage.RegisterScript(GetKey(false), script);
		}

		private static string GetKey(bool pIsStartUp)
		{
			string ret;
			if (pIsStartUp)
				ret = KEYCONFIRM_UPIMMEDIATE;
			else
				ret = KEYCONFIRM_IMMEDIATE;
			return ret;
		}

        #region private GetConfirmScript
        private static string GetConfirmScript(string pTitle, string pMessage, string pTarget, string pArgument_Ok, string pArgument_Cancel)
		{
			return GetConfirmScript(pTitle, pMessage, "confirm", pTarget, pArgument_Ok, pArgument_Cancel);
		}
		private static string GetConfirmScript(string pTitle, string pMessage, string pStatus, string pTarget, string pArgument_Ok, string pArgument_Cancel)
		{
			string ret;
			JSStringBuilder sbjss = new JSStringBuilder();
			sbjss.AppendLine("OpenMainConfirm(" + JSString(pTitle) + "," + HTMLString(pMessage) + "," +
				JSString(pStatus.ToLower()) + ",0,0,0,0," + JSString(pTarget) + "," + JSString(pArgument_Ok) + "," + JSString(pArgument_Cancel) + ");");
			ret = sbjss.ToString();
			return ret;
		}
		#endregion
		#region GetTransferScript
		// EG 20150716 [21103] Add IsReversalSafekeeping
		private static string GetTransferScript(string pTitle, string pMessage, string pTarget, string pArgument_Ok, string pArgument_Cancel)
		{
			string ret;
			JSStringBuilder sbjss = new JSStringBuilder();
			#region Transfert de masse
			//string message = @"<div><table id=""tblActorTransfer"">";
			//message += @"<tr><td colspan=""3""><span class='formula'>" + pMessage + @"</span></td></tr>";
			string message = @"<div><span class='formula'>" + pMessage + @"</span></div>";
			message += @"<div><table id=""tblActorTransfer"">";
			message += @"<tr><td colspan=""3""><span class='lblCaptureTitleBold'>" + Ressource.GetString("SelectDealerTransfer") + @"</span></td></tr>";
			message += @"<tr><td><span name=""lblDealer"" id=""lblDealer"" >" + Cst.HTMLSpace2 + Ressource.GetString("ActorIdentifier") + @"</span></td>";
			message += @"<td>&nbsp;</td>";
			message += @"<td><input type=""text"" name=""txtDealer"" id=""txtDealer"" class=""txtCapture"" width=""100%""  /></td></tr>";
			message += @"<tr><td><span name=""lblBookDealer"" id=""lblBookDealer"" >" + Cst.HTMLSpace2 + Ressource.GetString("BookIdentifier") + @"</span></td>";
			message += @"<td>&nbsp;</td>";
			message += @"<td><input type=""text"" name=""txtBookDealer"" id=""txtBookDealer"" class=""txtCapture"" width=""100%""  /></td></tr>";

			message += @"<tr><td colspan=""3""><span class='lblCaptureTitleBold'>" + Ressource.GetString("SelectClearerOrCustodianTransfer") + @"</span></td></tr>";
			message += @"<tr><td><span name=""lblClearer"" id=""lblClearer"" >" + Cst.HTMLSpace2 + Ressource.GetString("ActorIdentifier") + @"</span></td>";
			message += @"<td>&nbsp;</td>";
			message += @"<td><input type=""text"" name=""txtClearer"" id=""txtClearer"" class=""txtCapture"" width=""100%""  /></td></tr>";
			message += @"<tr><td><span name=""lblBookClearer"" id=""lblBookClearer"" >" + Cst.HTMLSpace2 + Ressource.GetString("BookIdentifier") + @"</span></td>";
			message += @"<td>&nbsp;</td>";
			message += @"<td><input type=""text"" name=""txtBookClearer"" id=""txtBookClearer"" class=""txtCapture"" width=""100%""  /></td></tr>";

			message += @"<tr><td colspan=""3""><span class='lblCaptureTitleBold'>" + Ressource.GetString("otherPartyPayment") + @"</span></td></tr>";
			message += @"<tr><td><label for=""chkReversalFees"" class=""lblCapture"">" + Cst.HTMLSpace2 + Ressource.GetString("isReversalFees") + @"</label></td>";
			message += @"<td>&nbsp;</td>";
			message += @"<td><input type=""checkbox"" name=""chkReversalFees"" id=""chkReversalFees"" class=""txtCapture"" width=""100%""  /></td></tr>";
			message += @"<tr><td><label for=""chkCalcNewFees"" class=""lblCapture"">" + Cst.HTMLSpace2 + Ressource.GetString("isNewCalculationFees") + @"</label></td>";
			message += @"<td>&nbsp;</td>";
			message += @"<td><input type=""checkbox"" name=""chkCalcNewFees"" id=""chkCalcNewFees"" class=""txtCapture"" width=""100%""  /></td></tr>";

			// EG 20150716 [21103]
			message += @"<tr><td colspan=""3""><span class='lblCaptureTitleBold'>" + Ressource.GetString("safekeepingPayment") + @"</span></td></tr>";
			message += @"<tr><td><label for=""chkReversalSafekeeping"" class=""lblCapture"">" + Cst.HTMLSpace2 + Ressource.GetString("isReversalSafekeeping") + @"</label></td>";
			message += @"<td>&nbsp;</td>";
			message += @"<td><input type=""checkbox"" name=""chkReversalSafekeeping"" id=""chkReversalSafekeeping"" class=""txtCapture"" width=""100%""  /></td></tr>";

			message += @"</table></div>";
			sbjss.AppendLine("OpenActorTransferConfirm(" + JSString(pTitle) + "," + HTMLString(message) + "," +
				JSString("actortransfer") + ",0,0,0,0," + JSString(pTarget) + "," + JSString(pArgument_Ok) + "," + JSString(pArgument_Cancel) + ");");
			ret = sbjss.ToString();

			#endregion Transfert de masse

			return ret;
		}
		#endregion
		#endregion

		#region AlertImmediate
		#region public AlertImmediate
		/// <summary>
		/// Javascript natif (utilisation de la méthode alert)
		/// </summary>
		public static void AlertImmediate(PageBase pPage, string pMessage)
		{
			AlertImmediate(pPage, pMessage, false);
		}
		/// <summary>
		/// Javascript natif (utilisation de la méthode alert)
		/// </summary>
		public static void AlertImmediate(PageBase pPage, string pMessage, bool pIsClose)
		{
			string script = GetAlertScript(pMessage, pIsClose);
			pPage.RegisterScript("AlertImmediate", script, false);
		}
		#endregion
		#region public AlertStartUpImmediate
		/// <summary>
		/// Javascript natif (utilisation de la méthode alert)
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pMessage"></param>
		public static void AlertStartUpImmediate(PageBase pPage, string pMessage)
		{
			AlertStartUpImmediate(pPage, pMessage, false);
		}
		/// <summary>
		/// Javascript natif (utilisation de la méthode alert)
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pMessage"></param>
		/// <param name="pIsClose"></param>
		public static void AlertStartUpImmediate(PageBase pPage, string pMessage, bool pIsClose)
		{
			string script = GetAlertScript(pMessage, pIsClose);
			pPage.RegisterScript("AlertStartUpImmediate", script, true);
		}
		#endregion
		/// <summary>
		/// Utlisation de la méthode Alert de javascript
		/// </summary>
		/// <param name="psMessage"></param>
		/// <param name="pIsClose"></param>
		/// <returns></returns>
		private static string GetAlertScript(string psMessage, bool pIsClose)
		{
			string ret;
			string smg;
			JSStringBuilder sbjss = new JSStringBuilder();
			//
			smg = JSString(psMessage);
			sbjss.AppendLine("alert(" + smg + ");");
			if (pIsClose)
				sbjss.AppendLine("self.close();");
			ret = sbjss.ToString();
			return ret;
		}
		#endregion

		#region DialogImmediate
		#region public DialogImmediate
		public static void DialogImmediate(PageBase pPage, string pMessage)
		{
			DialogImmediate(pPage, pMessage, false, ProcessStateTools.StatusNoneEnum);
		}
		public static void DialogImmediate(PageBase pPage, string pMessage, bool pIsClose)
		{
			DialogImmediate(pPage, pMessage, pIsClose, ProcessStateTools.StatusNoneEnum);
		}
		public static void DialogImmediate(PageBase pPage, string pMessage, bool pIsClose, ProcessStateTools.StatusEnum pStatus)
		{
			string script = GetDialogScript(pPage.PageTitle, JSValidString(pMessage), pIsClose, pStatus, DefautHeight, DefautWidth);
			pPage.RegisterScript("DialogImmediate", script, false);
		}
		#endregion
		#region public DialogStartUpImmediate
		/// <summary>
		/// jQuery Dialog
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pMessage"></param>
		public static void DialogStartUpImmediate(PageBase pPage, string pMessage)
		{
			DialogStartUpImmediate(pPage, pPage.PageTitle, pMessage, false, ProcessStateTools.StatusNoneEnum, DefautHeight, DefautWidth);
		}
		public static void DialogStartUpImmediate(PageBase pPage, string pMessage, bool pIsClose)
		{
			DialogStartUpImmediate(pPage, pPage.PageTitle, pMessage, pIsClose, ProcessStateTools.StatusNoneEnum, DefautHeight, DefautWidth);
		}
		/// <summary>
		/// jQuery Dialog 
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pMessage"></param>
		/// <param name="pIsClose"></param>
		/// <param name="pLevel"></param>
		public static void DialogStartUpImmediate(PageBase pPage, string pMessage, bool pIsClose, ProcessStateTools.StatusEnum pStatus)
		{
			DialogStartUpImmediate(pPage, pPage.PageTitle, pMessage, pIsClose, pStatus, DefautHeight, DefautWidth);
		}
		/// <summary>
		/// jQuery Dialog
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pMessage"></param>
		/// <param name="pIsClose"></param>
		/// <param name="pLevel"></param>
		/// <param name="pMaxHeight"></param>
		/// <param name="pMaxWidth"></param>
		public static void DialogStartUpImmediate(PageBase pPage, string pMessage, bool pIsClose,
			ProcessStateTools.StatusEnum pStatus, Pair<Pair<int?, int?>, Pair<int?, int?>> pSize)
		{
			DialogStartUpImmediate(pPage, pPage.PageTitle, pMessage, pIsClose, pStatus, pSize.First, pSize.Second);
		}
		/// <summary>
		/// jQuery Dialog
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pTitle"></param>
		/// <param name="pMessage"></param>
		/// <param name="pIsClose"></param>
		/// <param name="pLevel"></param>
		/// <param name="pMaxHeight"></param>
		/// <param name="pMaxWidth"></param>
		public static void DialogStartUpImmediate(PageBase pPage, string pTitle, string pMessage, bool pIsClose,
			ProcessStateTools.StatusEnum pStatus, Pair<int?, int?> pHeight, Pair<int?, int?> pWidth)
		{
			string script = GetDialogScript(pTitle, JSValidString(pMessage), pIsClose, pStatus, pHeight, pWidth);
			pPage.RegisterScript("DialogStartUpImmediate", script, true);
		}
		#endregion
		#region private GetDialogScript
		private static string GetDialogScript(string pTitle, string pMessage, bool pIsClose,
			ProcessStateTools.StatusEnum pStatus, Pair<int?, int?> pHeight, Pair<int?, int?> pWidth)
		{
			string ret;
			JSStringBuilder sbjss = new JSStringBuilder();
			string _height = "0";
			string _maxHeight = "0";
			if (null != pHeight)
			{
				if (pHeight.First.HasValue && (0 < pHeight.First.Value))
					_height = pHeight.First.Value.ToString();
				if (pHeight.Second.HasValue && (0 < pHeight.Second.Value))
					_maxHeight = pHeight.Second.Value.ToString();
			}
			string _width = "0";
			string _maxWidth = "0";
			if (null != pWidth)
			{
				if (pWidth.First.HasValue && (0 < pWidth.First.Value))
					_width = pWidth.First.Value.ToString();
				if (pWidth.Second.HasValue && (0 < pWidth.Second.Value))
					_maxWidth = pWidth.Second.Value.ToString();
			}


			sbjss.AppendLine("OpenMainDialog(" + JSString(pTitle) + "," + HTMLString(pMessage) + "," + JSString(pStatus.ToString().ToLower()) + "," +
				_height + "," + _maxHeight + "," + _width + "," + _maxWidth + "," + JSString(pIsClose.ToString().ToLower()) + ");");
			ret = sbjss.ToString();
			return ret;
		}
		#endregion
		#endregion

		#region ExecuteImmediate
		public static void ExecuteImmediate(PageBase pPage, string pCommand, bool pIsClose)
		{
			ExecuteImmediate(pPage, pCommand, pIsClose, false);
		}
		public static void ExecuteImmediate(PageBase pPage, string pCommand, bool pIsClose, bool pIsStartup)
		{
			string script;
			string nameFunction = "ExecuteImmediate";
			JSStringBuilder sb = new JSStringBuilder();
			//
			if (!pCommand.EndsWith(";"))
				pCommand += ";";
			//
			sb.AppendLine("function " + nameFunction + "()");
			sb.AppendLine("{");
			sb.AppendLine(pCommand);
			if (pIsClose)
				sb.AppendLine("self.close();");
			sb.AppendLine("}");
			sb.AppendLine(nameFunction + "();");
			script = sb.ToString();
			//
			pPage.RegisterScript(nameFunction, script, pIsStartup);
		}
		#endregion

		#region ScriptOnStartUp
		public static void ScriptOnStartUp(PageBase pPage, string pscript, string pkey)
		{
			string script;
			JSStringBuilder sb = new JSStringBuilder();
			sb.AppendLine(pscript);
			script = sb.ToString();
			pPage.RegisterScript(pkey, script, true);
		}
		#endregion ScriptOnStartUp

		#region SetInitialFocus
		public static void SetInitialFocus(Control pControl)
		{
			SetInitialFocus(pControl, string.Empty);
		}
		public static void SetInitialFocus(Control pControl, string pAddScript)
		{
			string script;
			string nameFunction = "SetInitialFocus";
			//
			if (pControl.Page == null)
			{
				throw new ArgumentException(
					"The Control must be added to a Page before you can set the IntialFocus to it.");
			}
			//
			bool bSelect = true;

			// Create JavaScript
			JSStringBuilder sb = new JSStringBuilder();
			sb.Append("function " + nameFunction + "()\n");
			sb.Append("{\n");
			sb.Append("   document.");

			// Find the Form
			Control p = pControl.Parent;
			while (!(p is System.Web.UI.HtmlControls.HtmlForm))
				p = p.Parent;
			sb.Append(p.ClientID);

			sb.Append("['");
			sb.Append(pControl.UniqueID);

            // Set Focus on the selected item of a RadioButtonList
            if (pControl is RadioButtonList rbl)
            {
                bSelect = false;
                string suffix = "_0";
                int t = 0;
                foreach (ListItem li in rbl.Items)
                {
                    if (li.Selected)
                    {
                        suffix = "_" + t.ToString();
                        break;
                    }
                    t++;
                }
                sb.Append(suffix);
            }

            // Set Focus on the first item of a CheckBoxList
            if (pControl is CheckBoxList)
			{
				bSelect = false;
				sb.Append("_0");
			}

			if (pControl is Button)
				bSelect = false;
			if (pControl is ListBox)
				bSelect = false; //GlopPL Rajouter le 16/10/2003
			if (pControl is DropDownList)
				bSelect = false; //GlopPL Rajouter le 16/10/2003

			sb.Append("'].focus();\n");

			//*****
			if (bSelect)
			{
				sb.Append("   document.");
				sb.Append(p.ClientID);
				sb.Append("['");
				sb.Append(pControl.UniqueID);
				sb.Append("'].select();\n");
			}
			sb.Append("}\n");
			// EG 20160404 Migration vs2013
			//if (pControl.Page.SmartNavigation)
			if (pControl.Page.MaintainScrollPositionOnPostBack)
				sb.Append("window.setTimeout(" + nameFunction + ", 500);\n");
			else
				sb.Append("window.onload = " + nameFunction + ";" + pAddScript + "\n");
			//
			script = sb.ToString();

			((PageBase)pControl.Page).RegisterScript(nameFunction, script);
		}

		#endregion SetInitialFocus

		#region SubmitOpenerAndSelfClose
		public static void SubmitOpenerAndSelfClose(PageBase pPage, string pOpener)
		{
			string script;
			string nameFunction = "SubmitOpenerAndSelfClose";
			JSStringBuilder sb = new JSStringBuilder();
			if (pOpener != null)
				sb.AppendLine("	window.opener." + pOpener + ".submit();");
			sb.AppendLine("	self.close();");
			//
			script = sb.ToString();
			pPage.RegisterScript(nameFunction, script);
		}
		public static void SubmitOpenerAndSelfClose(PageBase pPage, string pEvent, string pArgument)
		{
			SubmitOpenerAndSelfClose(string.Empty, pPage, pEvent, pArgument);
		}
		public static void SubmitOpenerAndSelfClose(string pNameFunction, PageBase pPage, string pEvent, string pArgument)
		{
            if (StrFunc.IsEmpty(pNameFunction))
				pNameFunction = "SubmitOpenerAndSelfClose";

			JSStringBuilder sb = new JSStringBuilder();
			sb.AppendLine("window.opener.__doPostBack('" + pEvent + "','" + pArgument + "');");
			sb.AppendLine("	self.close();");

			pPage.RegisterScript(pNameFunction, sb.ToString());
		}
		#endregion SubmitOpenerAndSelfClose

		#region public OpenCodeEvent
		public static void OpenCodeEvent(PageBase pPage, string pGUID)
		{
			string nameFunction = "OpenCodeEvent";
			JSStringBuilder sb = new JSStringBuilder();
			sb.Append("function " + nameFunction + "()\n");
			sb.Append("{\n");
			sb.Append("ret=window.open('CodeEvent.aspx?GUID=" + pGUID + "','_blank','width=750,location=no,scrollbars=no,resizable=yes,status=no');\n");
			sb.Append("}\n");
			pPage.RegisterScript(nameFunction, sb.ToString());
		}
		#endregion OpenCodeEvent

		#region public OpenEventSi
		public static void OpenEventSi(PageBase pPage, string pGUID)
		{
			OpenEventsChildrens1(pPage, pGUID, "OpenEventSi", "EventSi.aspx");
		}
		#endregion OpenEventSi
		#region public OpenEventSiMsg
		public static void OpenEventSiMsg(PageBase pPage, string pGUID)
		{
			OpenEventsChildrens2(pPage, pGUID, "OpenEventSiMsg", "EventSiMsg.aspx");
		}
		#endregion OpenEventSiMsg
		#region public OpenEventReset
		public static void OpenEventReset(PageBase pPage, string pGUID)
		{
			OpenEventsChildrens1(pPage, pGUID, "OpenEventReset", "EventReset.aspx");
		}
		#endregion OpenEventReset
		#region public OpenEventExerciseDates
		public static void OpenEventExerciseDates(PageBase pPage, string pGUID)
		{
			OpenEventsChildrens1(pPage, pGUID, "OpenEventExerciseDates", "EventExerciseDates.aspx");
		}
		#endregion OpenEventExerciseDates
		#region public OpenEventUnderlyerValuationDates
		public static void OpenEventUnderlyerValuationDates(PageBase pPage, string pGUID)
		{
			OpenEventsChildrens1(pPage, pGUID, "OpenEventUnderlyerValuationDates", "EventUnderlyerValuationDates.aspx");
		}
		#endregion OpenEventUnderlyerValuationDates
		#region public OpenEventInvoiceDetailFee
		public static void OpenEventInvoiceDetailFee(PageBase pPage, string pGUID)
		{
			OpenEventsChildrens1(pPage, pGUID, "OpenEventInvoiceDetailFee", "EventInvoiceDetailFee.aspx");
		}
		#endregion OpenEventInvoiceDetailFee
		#region public OpenSubDebtSecEvents
		public static void OpenSubDebtSecEvents(PageBase pPage, string pGUID)
		{
			OpenSubEvents(pPage, pGUID, "OpenSubDebtSecEvents", "DebtSecEvents.aspx");
		}
		#endregion OpenSubDebtSecEvents

		#region public OpenSubTradeEvents
		public static void OpenSubTradeEvents(PageBase pPage, string pGUID)
		{
			OpenSubEvents(pPage, pGUID, "OpenSubTradeEvents", "TradeEvents.aspx");
		}
		#endregion OpenSubTradeEvents
		#region public OpenSubTradeAdminEvents
		public static void OpenSubTradeAdminEvents(PageBase pPage, string pGUID)
		{
			OpenSubEvents(pPage, pGUID, "OpenSubTradeAdminEvents", "TradeAdminEvents.aspx");
		}
		#endregion OpenSubTradeAdminEvents
		#region public OpenSubTradeRiskEvents
		public static void OpenSubTradeRiskEvents(PageBase pPage, string pGUID)
		{
			OpenSubEvents(pPage, pGUID, "OpenSubTradeRiskEvents", "TradeRiskEvents.aspx");
		}
		#endregion OpenSubTradeRiskEvents
		#region public OpenSubEvents
		private static void OpenSubEvents(PageBase pPage, string pGUID, string pNameFunction, string pPageToOpen)
		{
			JSStringBuilder sb = new JSStringBuilder();
			sb.Append("function " + pNameFunction + "(idE,title)\n");
			sb.Append("{\n");
			sb.Append("ret=window.open('" + pPageToOpen + "?GUID=" + pGUID + "&IdE=' + idE + '&Title=' + title,'_blank');");
			sb.Append("}\n");
			pPage.RegisterScript(pNameFunction, sb.ToString());
		}
		#endregion OpenSubEvents
		#region public OpenEventsChildrens
		private static void OpenEventsChildrens1(PageBase pPage, string pGUID, string pNameFunction, string pPageToOpen)
		{
			JSStringBuilder sb = new JSStringBuilder();
			sb.Append("function " + pNameFunction + "(idE,title)\n");
			sb.Append("{\n");
			sb.Append("ret=window.open('" + pPageToOpen + "?GUID=" + pGUID + "&IdE=' + idE + '&Title=' + title,'_blank');");
			sb.Append("}\n");
			pPage.RegisterScript(pNameFunction, sb.ToString());
		}
		private static void OpenEventsChildrens2(PageBase pPage, string pGUID, string pNameFunction, string pPageToOpen)
		{
			JSStringBuilder sb = new JSStringBuilder();
			sb.Append("function " + pNameFunction + "(idE, Payer_Receiver)\n");
			sb.Append("{\n");
			sb.Append("ret=window.open('" + pPageToOpen + "?GUID=" + pGUID + "&IdE=' + idE + '&P_R=' + Payer_Receiver,'_blank');");
			sb.Append("}\n");
			pPage.RegisterScript(pNameFunction, sb.ToString());
		}
		#endregion OpenEventsChildrens

		#region public DisplayDescription
		// EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		public static void DisplayDescription(PageBase pPage)
		{
			string script;
			string nameFunction = "DisplayDescription";
			// Create JavaScript
			JSStringBuilder sb = new JSStringBuilder();
			sb.AppendLine("function " + nameFunction + "( isChecked )");
			sb.AppendLine("{");
			sb.AppendLine("var chkDescription = document.getElementById(\"chkDescription\");");
			sb.AppendLine("var description = document.getElementsByTagName(\"textarea\");");
			sb.AppendLine("for (var i=0;i<description.length;i++)");
			sb.AppendLine("{");
			sb.AppendLine("\t description[i].parentNode.style.display=(isChecked?\"block\":\"none\");");
			sb.AppendLine("}");
			sb.AppendLine("window.resizeTo(600,isChecked?600:400);");
			sb.AppendLine("}");
			//
			sb.AppendLine("function getAscendingTops(elem)");
			sb.AppendLine("{");
			sb.AppendLine("if (elem == null)");
			sb.AppendLine("return 0;");
			sb.AppendLine("else");
			sb.AppendLine("return elem.offsetTop + (getAscendingTops(elem.offsetParent));");
			sb.AppendLine("}");
			//
			script = sb.ToString();
			//
			pPage.RegisterScript(nameFunction, script);
		}
		#endregion DisplayDescription
		#region public DisplayDescriptionStartUp
		// EG 20160404 Migration vs2013
		public static void DisplayDescriptionStartUp(PageBase pPage, bool isChecked)
		{
			string nameFunction;
			nameFunction = "DisplayDescriptionStartUp";
			JSStringBuilder sb = new JSStringBuilder();
			sb.AppendLine("DisplayDescription(" + isChecked.ToString().ToLower() + ");");
			// EG 20160404 Migration vs2013
			//pPage.RegisterStartupScript(nameFunction, sb.ToString());
			if (false == pPage.ClientScript.IsStartupScriptRegistered(nameFunction))
				pPage.ClientScript.RegisterStartupScript(pPage.GetType(), nameFunction, sb.ToString());
		}
		#endregion

		#region public PostBackOnLoad
		// EG 20210126 [25556] MAJ JQuery (3.5.1)
		public static void PostBackOnLoad(PageBase pPage, bool pIsSetScrollPos, string pAddJS)
		{
			if (pPage.IsPostBack)
			{
				string nameFunction = "PostBackOnLoad";
				string scrollTop = String.Empty;
				if (pIsSetScrollPos)
					scrollTop = $@"document.body.scrollTop ='{pPage.Request.Form["__SCROLLPOS"]}';";

				string script = String.Format(@"function {0}() {{
                    {1}
                    var activeElementId = '{2}';
                    if ('' != activeElementId) {{
                        var activeElement = document.getElementById(activeElementId);
                        if (null != activeElement) {{
                            try {{activeElement.focus();}}
                            catch(e){{}}
                            finally{{document.forms[0].__ACTIVEELEMENT.value = '';}};

                            {3}
                        }}
                    }}
                }}
				document.body.onload = {0}();", nameFunction, scrollTop, pPage.Request.Form["__ACTIVEELEMENT"], pAddJS);

				JSStringBuilder sb = new JSStringBuilder();
				sb.AppendLine(script);

				pPage.RegisterScript(nameFunction, sb.ToString(), true);
			}
		}
		#endregion PostBackOnLoad
		#region public FirstPostOnLoad
		// EG 20210126 [25556] MAJ JQuery (3.5.1)
		public static void FirstPostOnLoad(PageBase pPage, string pJsScript)
		{
			if ((false == pPage.IsPostBack) && StrFunc.IsFilled(pJsScript))
			{
				string nameFunction = "OnLoad";
				JSStringBuilder sb = new JSStringBuilder();
				sb.AppendLine(String.Format(@"function {0}() {{
                    {1}
	                document.body.onload = {0}();
                }}", nameFunction, pJsScript));

				pPage.RegisterScript(nameFunction, sb.ToString(), true);
			}
		}
		#endregion FirstPostOnLoad

		#region OpenObjectFpMLImmediate
		public static void OpenObjectFpMLImmediate(PageBase pPage,
												   string pObject, string pObjectOccurence, string pElement, string pOccurence,
												   string pCopyTo, string pTitle, string pReadOnly, string pTitleRight)
		{
			//string nameFunction = "OpenObjectFpMLImmediate";
			// Create JavaScript
			string script = "window.open(\"CustomFpMLObject.aspx?GUID=" + pPage.GUID;
			script += "&Object=" + pObject + "&ObjectOccurence=" + pObjectOccurence + "&Element=" + pElement;
			script += "&Occurence=" + pOccurence + "&CopyTo=" + pCopyTo + "&Title=" + pTitle;
			script += "&ReadOnly=" + pReadOnly + "&TitleRight=" + pTitleRight + "\"";
			script += ",\"object\",\"fullscreen=no,resizable=yes,location=no,scrollbars=yes,status=yes\");";
			//20060907 PL 
			//pPage.RegisterScript(nameFunction, script, true);
			pPage.WindowOpen = script;
		}
		#endregion OpenObjectFpMLImmediate

		#region OpenScreenBoxImmediate
		public static void OpenScreenBoxImmediate(PageBase pPage, string pIdMenu, string pScreenParent, string pScreen, string pDialogStyle, string pImage)
		{
			string script;
			string nameFunction = "OpenScreenBoxImmediate";
			// Create JavaScript
			string nameSubForm = string.Empty;
			if (IdMenu.GetIdMenu(IdMenu.Menu.InputTrade) == pIdMenu)
				nameSubForm = "SubTrade";
			else if (IdMenu.GetIdMenu(IdMenu.Menu.InputEvent) == pIdMenu)
				nameSubForm = "SubTradeEvents";
			else if (IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin) == pIdMenu)
				nameSubForm = "SubTradeAdmin";
			JavaScript.JSStringBuilder sb = new JavaScript.JSStringBuilder();
			sb.Append("window.open(\"" + nameSubForm + ".aspx?GUID=" + pPage.GUID);
			sb.Append("&IDMenu=" + pIdMenu + "&ScreenParent=" + pScreenParent + "&Screen=" + pScreen + "&Image=" + pImage + "\"");
			sb.AppendLine(",\"" + pScreen + "\",\"" + pDialogStyle + "\");");
			script = sb.ToString();
			pPage.RegisterScript(nameFunction, script, true);
		}
		#endregion OpenScreenBoxImmediate


		#region GetWindowOpen
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUrl"></param>
		/// <returns></returns>
		public static string GetWindowOpen(string pUrl)
		{
			return GetWindowOpen(pUrl, Cst.HyperLinkTargetEnum._blank.ToString(), WindowOpenAttribut.ModeEnum.defaultMode);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUrl"></param>
		/// <param name="pMode"></param>
		/// <returns></returns>
		public static string GetWindowOpen(string pUrl, WindowOpenAttribut.ModeEnum pMode)
		{
			return GetWindowOpen(pUrl, Cst.HyperLinkTargetEnum._blank.ToString(), pMode);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUrl"></param>
		/// <param name="pTarget"></param>
		/// <returns></returns>
		public static string GetWindowOpen(string pUrl, Cst.WindowOpenStyle pTarget)
		{
			return GetWindowOpen(pUrl, pTarget, WindowOpenAttribut.ModeEnum.defaultMode);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUrl"></param>
		/// <param name="pTarget"></param>
		/// <param name="pMode"></param>
		/// <returns></returns>
		// EG 20200924 [XXXXX] Suppression du paramètre d'ouverture de page IsOpenInUniquePage
		/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
		public static string GetWindowOpen(string pUrl, Cst.WindowOpenStyle pTarget, WindowOpenAttribut.ModeEnum pMode)
		{
			WindowOpenAttribut woa = WindowOpenAttribut.GetWindowOpenAttribut(pTarget);
			if (pMode != WindowOpenAttribut.ModeEnum.defaultMode)
				woa.mode = pMode;

			string target;
			switch (pTarget)
			{
				case Cst.WindowOpenStyle._help:
				case Cst.WindowOpenStyle.FpML_Help:
				case Cst.WindowOpenStyle.EfsML_Help:
					target = pTarget.ToString();
					break;
				case Cst.WindowOpenStyle.EfsML_Main:
					target = "main";
					break;
				case Cst.WindowOpenStyle.EfsML_Status:
					target = "Status";
					break;
				case Cst.WindowOpenStyle.EfsML_FormReferential:
				case Cst.WindowOpenStyle.EfsML_ListReferential:
				default:
					target = Cst.HyperLinkTargetEnum._blank.ToString();
					break;
			}
			return GetWindowOpen(pUrl, target, woa);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUrl"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string GetWindowOpen(string pUrl, string target)
		{
			return GetWindowOpen(pUrl, target, WindowOpenAttribut.ModeEnum.defaultMode);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUrl"></param>
		/// <param name="target"></param>
		/// <param name="pMode"></param>
		/// <returns></returns>
		public static string GetWindowOpen(string pUrl, string target, WindowOpenAttribut.ModeEnum pMode)
		{
			WindowOpenAttribut woa = new WindowOpenAttribut();
			if (pMode != WindowOpenAttribut.ModeEnum.defaultMode)
				woa.mode = pMode;
			return GetWindowOpen(pUrl, target, woa);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUrl"></param>
		/// <param name="target"></param>
		/// <param name="pWoa"></param>
		/// <returns></returns>
		/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
		private static string GetWindowOpen(string pUrl, string target, WindowOpenAttribut pWoa)
		{
			StringBuilder sb = new StringBuilder();
			switch (pWoa.mode)
			{
				case WindowOpenAttribut.ModeEnum.help:
					sb.Append("window.showHelp(");
					sb.Append(JSDblQuote(pUrl));
					sb.Append(");");
					break;
				
				case WindowOpenAttribut.ModeEnum.open:
					sb.Append("window.open(");
					sb.Append(JSDblQuote(pUrl));
					sb.Append("," + JSDblQuote(target));
					sb.Append("," + JSDblQuote(pWoa.GetwindowFeatures()));
					sb.Append(");");
					break;
				case WindowOpenAttribut.ModeEnum.openRequestedPopup:
					sb.Append("OpenRequestedPopup(");
					sb.Append(JSDblQuote(pUrl));
					sb.Append("," + JSDblQuote(target));
					sb.Append("," + JSDblQuote(pWoa.GetwindowFeatures()));
					sb.Append(");");
					break;
				case WindowOpenAttribut.ModeEnum.opentab:
					sb.Append("window.open(");
					sb.Append(JSDblQuote(pUrl));
					sb.Append("," + JSDblQuote(target));
					sb.Append(");");
					break;
			}
			return sb.ToString();
		}
		#endregion GetWindowOpen

		#region class WindowOpenAttribut
		public class WindowOpenAttribut
		{
			public WindowOpenAttribut()
			{
			}

			/// <summary>
			/// type d'ouverture de page
			/// </summary>
			/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
			public enum ModeEnum
			{
				/// <summary>
				/// 
				/// </summary>
				defaultMode,
				/// <summary>
				/// Appel à la méthode window.showHelp
				/// </summary>
				help,
				/// <summary>
				/// Appel à la méthode window.Open
				/// </summary>
				open,
				/// <summary>
				/// Appel à la méthode window.Open sans arguments complémentaire (hauteur,largeur, location etc) 
				/// </summary>
				opentab,
				/// <summary>
				/// Appel à la méthode openRequestedPopup (Voir pagebase.js)
				/// </summary>
				openRequestedPopup,
			}

			/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
			public ModeEnum mode = ModeEnum.opentab;
			public bool center = false;

			public bool fullscreen = false;
			public int height = -1;
			public int inneHeight = -1;
			public int innerWidth = -1;
			public int left = -1;
			public bool location = false;
			public bool menubar = false;
			public int outerHeight = -1;
			public int outerWidth = -1;
			public bool resizable = true;
			//public int  screenX     = -1;
			//public int  screenY     = -1;
			public bool scrollbars = true;
			public bool status = false;
			public bool toolbar = false;
			public int top = -1;
			public int width = -1;

			/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
			public string GetwindowFeatures()
			{
				string ret = string.Empty;

				if (mode != ModeEnum.opentab)
				{
					ret += "fullscreen=" + (fullscreen ? "yes" : "no");
					ret += ",location=" + (location ? "yes" : "no");
					ret += ",menubar=" + (menubar ? "yes" : "no");
					ret += ",resizable=" + (resizable ? "yes" : "no");
					ret += ",scrollbars=" + (scrollbars ? "yes" : "no");
					ret += ",status=" + (status ? "yes" : "no");
					ret += ",toolbar=" + (toolbar ? "yes" : "no");
					ret += ",location=" + (location ? "yes" : "no");

					if (fullscreen)
					{
						height = -1;
						left = -1;
						top = -1;
						width = -1;
					}
					else if (center)
					{
						if (width > 0)
						{
							left = -1; //TODO
							ret += ",left=((screen.width-" + width.ToString() + ")/2)";
						}
						if (height > 0)
						{
							top = -1;
							ret += ",top=((screen.height-" + height.ToString() + ")/2)";
						}
					}

					if (height >= 0)
						ret += ",height=" + height.ToString();
					if (inneHeight >= 0)
						ret += ",inneHeight=" + inneHeight.ToString();
					if (innerWidth >= 0)
						ret += ",innerWidth=" + innerWidth.ToString();
					if (left >= 0)
						ret += ",left=" + left.ToString();
					if (outerHeight >= 0)
						ret += ",outerHeight=" + outerHeight.ToString();
					if (outerWidth >= 0)
						ret += ",outerWidth=" + outerWidth.ToString();
					if (top >= 0)
						ret += ",top=" + top.ToString();
					if (width >= 0)
						ret += ",width=" + width.ToString();
				}
				return ret;
			}


			/// <summary>
			/// Retourne les attributs d'une page en fonction de {pTarget}
			/// </summary>
			/// <param name="pTarget"></param>
			/// <returns></returns>
			/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
			public static WindowOpenAttribut GetWindowOpenAttribut(Cst.WindowOpenStyle pTarget)
			{
				WindowOpenAttribut ret;
				switch (pTarget)
				{
					case Cst.WindowOpenStyle._help:
					case Cst.WindowOpenStyle.FpML_Help:
					case Cst.WindowOpenStyle.EfsML_Help:
						ret = new WindowOpenAttribut()
						{
							mode = ModeEnum.help
						};
						break;
					case Cst.WindowOpenStyle.EfsML_FormReferential:
					case Cst.WindowOpenStyle.EfsML_ListReferential:
					case Cst.WindowOpenStyle.EfsML_Main:
					case Cst.WindowOpenStyle.EfsML_Status:
					default:
						ret = new WindowOpenAttribut();
						break;
				}
				return ret;
			}
		}
		#endregion class WindowOpenAttribut

		#region Notepad
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pTN"></param>
		/// <param name="pID"></param>
		/// <param name="pT1"></param>
		/// <param name="pT2"></param>
		/// <param name="pConsultationMode"></param>
		/// <param name="pS"></param>
		/// <param name="pT"></param>
		/// <param name="pParentGUID"></param>
		/// <returns></returns>
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		public static string GetUrlNotepad(string pTN, string pID, string pT1, string pT2,
				Cst.ConsultationMode pConsultationMode, string pS, string pT, string pParentGUID)
		{
            //PL 20120210 
            //url = System.Web.HttpContext.Current.Request.ApplicationPath + "/Notepad.aspx";
            string url = (System.Web.HttpContext.Current.Request.ApplicationPath == @"/" ? string.Empty : System.Web.HttpContext.Current.Request.ApplicationPath) + @"/Notepad.aspx";
            url += "?TN=" + pTN;
			url += "&ID=" + pID;
			url += "&T1=" + pT1;
			url += "&T2=" + pT2;
			url += "&M=" + Convert.ToInt32(pConsultationMode).ToString();
			url += "&S=" + pS;
			url += "&T=" + pT;

			// PM 20240604 [XXXXX] Ajout pParentGUID 
			if (StrFunc.IsFilled(pParentGUID))
			{
				url += "&GUID=" + pParentGUID;
			}

			return url;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pTN"></param>
		/// <param name="pID"></param>
		/// <param name="pT1"></param>
		/// <param name="pT2"></param>
		/// <param name="pConsultationMode"></param>
		/// <param name="pS"></param>
		/// <param name="pT"></param>
		/// <param name="pParentGUID"></param>
		/// <returns></returns>
		/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		public static string GetWindowOpenNotepad(string pTN, string pID, string pT1, string pT2, Cst.ConsultationMode pConsultationMode, string pS, string pT, string pParentGUID)
		{
			string url = GetUrlNotepad(pTN, pID, pT1, pT2, pConsultationMode, pS, pT, pParentGUID);
			return GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_FormReferential);
		}
		#endregion Notepad

		#region AttachedDoc
		/// <summary>
		/// Retourne l'URL qui permet d'ouvrir les documents attachés à un référentiel (Lecture de la table ATTACHEDDOC ou ATTACHEDDOCS)
		/// </summary>
		/// <param name="pTableAttachedDoc"></param>
		/// <param name="pValueDataKeyField"></param>
		/// <param name="pValueKeyField"></param>
		/// <param name="pValueDescription"></param>
		/// <param name="pIdMenu"></param>
		/// <param name="pTableName"></param>
		/// <param name="pParentGUID">GUID</param>
		/// <returns></returns>
		/// FI 20191002 [XXXXX] Add Method
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		public static string GetUrlAttachedDoc(string pTableAttachedDoc, string pValueDataKeyField, string pValueKeyField, string pValueDescription, string pIdMenu, string pTableName, string pParentGUID)
		{
			return BuildUrlAttachedDoc(pTableAttachedDoc, pValueDataKeyField, pValueKeyField, pValueDescription, pIdMenu, pTableName, null, pParentGUID);
		}
		/// <summary>
		/// Retourne l'URL qui permet d'ouvrir les documents attachés à un référentiel (Lecture d'une view VW_ATTACHEDDOC_XXXX dont le nom est à présicer)
		/// </summary>
		/// <param name="pTableAttachedDoc"></param>
		/// <param name="pValueDataKeyField"></param>
		/// <param name="pValueKeyField"></param>
		/// <param name="pValueDescription"></param>
		/// <param name="pIdMenu"></param>
		/// <param name="pTableName"></param>
		/// <param name="pViewAttachedDoc"></param>
		/// <param name="pParentGUID">GUID</param>
		/// <returns></returns>
		/// FI 20191002 [XXXXX] Add Method
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		public static string GetUrlAttachedDoc(string pTableAttachedDoc, string pValueDataKeyField, string pValueKeyField, string pValueDescription, string pIdMenu, string pTableName, string pViewAttachedDoc, string pParentGUID)
		{
			return BuildUrlAttachedDoc(pTableAttachedDoc, pValueDataKeyField, pValueKeyField, pValueDescription, pIdMenu, pTableName, pViewAttachedDoc, pParentGUID);
		}

		/// <summary>
		/// Javascript pour ouverture de URL qui permet d'ouvrir les documents attachés à un référentiel 
		/// </summary>
		/// <param name="pTableAttachedDoc"></param>
		/// <param name="pValueDataKeyField"></param>
		/// <param name="pValueKeyField"></param>
		/// <param name="pValueDescription"></param>
		/// <param name="pIdMenu"></param>
		/// <param name="pTableName"></param>
		/// <param name="pViewAttachedDoc">A renseigner si lecture d'une view VW_ATTACHEDDOC_XXXX</param>
		/// <param name="pParentGUID">GUID</param>
		/// <returns></returns>
		/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		public static string GetWindowOpenAttachedDoc(string pTableAttachedDoc, string pValueDataKeyField, string pValueKeyField, string pValueDescription, string pIdMenu, string pTableName, string pViewAttachedDoc, string pParentGUID)
		{
			string url = GetUrlAttachedDoc(pTableAttachedDoc, pValueDataKeyField, pValueKeyField, pValueDescription, pIdMenu, pTableName, pViewAttachedDoc, pParentGUID);
			return GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_FormReferential);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pTableAttachedDoc">ATTACHEDDOC ou ATTACHEDDOCS</param>
		/// <param name="pValueDataKeyField"></param>
		/// <param name="pValueKeyField"></param>
		/// <param name="pValueDescription"></param>
		/// <param name="pIdMenu"></param>
		/// <param name="pTableName"></param>
		/// <param name="pParam1">A renseigné si Lecture des données depuis une vue</param>
		/// <param name="pParentGUID">GUID</param>
		/// <returns></returns>
		/// FI 20160804 [Migration TFS] Modify
		/// FI 20191002 [XXXXX] Rename method, modifion de la signature, private  
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		private static string BuildUrlAttachedDoc(string pTableAttachedDoc, string pValueDataKeyField, string pValueKeyField, string pValueDescription, string pIdMenu, string pTableName, string pParam1, string pParentGUID)
		{
            //PL 20120210 
            //url  = System.Web.HttpContext.Current.Request.ApplicationPath + "/List.aspx";
            // EG 20161122 Add AspTools.SetListOrListViewer
            //url = (System.Web.HttpContext.Current.Request.ApplicationPath == @"/" ? string.Empty : System.Web.HttpContext.Current.Request.ApplicationPath) + @"/List.aspx";
            string url = (System.Web.HttpContext.Current.Request.ApplicationPath == @"/" ? string.Empty : System.Web.HttpContext.Current.Request.ApplicationPath) + "/List.aspx";

            url += StrFunc.AppendFormat("?{0}={1}", Cst.ListType.Repository.ToString(), pTableAttachedDoc);

            //url += "&InputMode=2";
            url += "&InputMode=" + ((int)Cst.DataGridMode.FormInput).ToString();

			// EG 20160404 Migration vs2013
			// #warning  RD 20100512 à revoir par EG
			// RD 20100512 Correction pour ne pas afficher les sous-menus à la consultation d'une pièce jointe. 
			url += "&IDMenu=" + pTableAttachedDoc;
			url += "&FK=" + pValueDataKeyField;
			//PL 20160914
			//string subTitle = Ressource.GetString(pIdMenu);
			string subTitle = Ressource.GetMenu_Shortname2(pIdMenu, pIdMenu);
			if (!String.IsNullOrEmpty(pValueKeyField))
			{
				subTitle += ": " + pValueKeyField;
				if (!String.IsNullOrEmpty(pValueDescription))
					subTitle += " - " + pValueDescription;
			}
			url += "&SubTitle=" + System.Web.HttpUtility.UrlEncode(subTitle, System.Text.Encoding.UTF8);
			url += "&DA=" + pTableName; //Dynamic Arg

			//FI 20191002 Gestion de P1
			if (pTableAttachedDoc == Cst.OTCml_TBL.ATTACHEDDOC.ToString())
			{
				string param1 = StrFunc.IsFilled(pParam1) ? pParam1 : pTableAttachedDoc;
				url += "&P1=" + param1;
			}

			// PM 20240604 [XXXXX] Ajout pParentGUID 
			if (StrFunc.IsFilled(pParentGUID))
			{
				url += "&GUID=" + pParentGUID;
			}

			return url;
		}


		#endregion AttachedDoc

		#region GetUrl_TestPascal
		private static string GetUrl_TestPascal(string pTableName,
			string pColumnName_Data, string pColumnName_Type, string pColumnName_FileName,
			string[] pKeyColumns, string[] pKeyValues, string[] pKeyDataTypes)
		{
            //PL 20120210 
            //url = System.Web.HttpContext.Current.Request.ApplicationPath + "/ViewDoc.aspx?O=" + pTableName;
            string url = (System.Web.HttpContext.Current.Request.ApplicationPath == @"/" ? string.Empty : System.Web.HttpContext.Current.Request.ApplicationPath) + @"/ViewDoc.aspx?O=" + pTableName;

            if (StrFunc.IsFilled(pColumnName_Data))
				url += @"&d=" + pColumnName_Data;
			if (StrFunc.IsFilled(pColumnName_Type))
				url += @"&t=" + pColumnName_Type;
			if (StrFunc.IsFilled(pColumnName_FileName))
				url += @"&f=" + pColumnName_FileName;
			//
			for (int i = 0; i < pKeyColumns.GetLength(0); i++)
			{
				url += @"&kc" + i.ToString() + @"=" + pKeyColumns[i];
				url += @"&kv" + i.ToString() + @"=" + pKeyValues[i];
				url += @"&kd" + i.ToString() + @"=" + pKeyDataTypes[i];
			}

			return url;
		}
		#endregion GetUrl_TestPascal
		#region GetWindowOpen_TestPascal
		/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
		public static string GetWindowOpen_TestPascal(string pTableName,
			string pColumnName_Data, string pColumnName_Type, string pColumnName_FileName,
			string[] pKeyColumns, string[] pKeyValues, string[] pKeyDataTypes)
		{
			string url = GetUrl_TestPascal(pTableName,
											pColumnName_Data, pColumnName_Type, pColumnName_FileName,
											pKeyColumns, pKeyValues, pKeyDataTypes);
			return GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_FormReferential);
		}
		#endregion GetWindowOpen_TestPascal

		#region GetWindowOpenUpload & GetWindowOpenDBImageViewer
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUploadTag"></param>
		/// <param name="pTableName"></param>
		/// <param name="pColumnName"></param>
		/// <param name="pKeyColumns"></param>
		/// <param name="pKeyValues"></param>
		/// <param name="pKeyDatatypes"></param>
		/// <param name="pDisplayName"></param>
		/// <param name="pColumnIDAUPD"></param>
		/// <param name="pColumnDTUPD"></param>
		/// <param name="pParentGUID"></param>
		/// <returns></returns>
		/// EG 20210204 [25590] Nouvelle gestion des mimeTypes autorisés dans l'upload d'un fichier
		/// EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		public static string GetWindowOpenDBImageViewer(string pUploadTag, string pTableName, string pColumnName, string[] pKeyColumns, string[] pKeyValues, string[] pKeyDatatypes, string pDisplayName, string pColumnIDAUPD, string pColumnDTUPD, string pParentGUID)
		{
			return GetWindowOpenUploadScript("DBImageViewer", pUploadTag, pTableName, pColumnName, pKeyColumns, pKeyValues, pKeyDatatypes, pDisplayName, string.Empty, pColumnIDAUPD, pColumnDTUPD, pParentGUID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUploadTag"></param>
		/// <param name="pTableName"></param>
		/// <param name="pColumnName"></param>
		/// <param name="pKeyColumns"></param>
		/// <param name="pKeyValues"></param>
		/// <param name="pKeyDatatypes"></param>
		/// <param name="pDisplayName"></param>
		/// <param name="pColumnFileName"></param>
		/// <param name="pColumnIDAUPD"></param>
		/// <param name="pColumnDTUPD"></param>
		/// <param name="pParentGUID"></param>
		/// <returns></returns>
		/// EG 20210204 [25590] Nouvelle gestion des mimeTypes autorisés dans l'upload d'un fichier
		/// EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		public static string GetWindowOpenUpload(string pUploadTag, string pTableName, string pColumnName, string[] pKeyColumns, string[] pKeyValues, string[] pKeyDatatypes, string pDisplayName, string pColumnFileName, string pColumnIDAUPD, string pColumnDTUPD, string pParentGUID)
		{
			return GetWindowOpenUploadScript("FileUpload", pUploadTag, pTableName, pColumnName, pKeyColumns, pKeyValues, pKeyDatatypes, pDisplayName, pColumnFileName, pColumnIDAUPD, pColumnDTUPD, pParentGUID);
		}
		// EG 20210204 [25590] Nouvelle gestion des mimeTypes autorisés dans l'upload d'un fichier
		/// EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
		/// PM 20240604 [XXXXX] Ajout paramètre pParentGUID
		private static string GetWindowOpenUploadScript(string pAspxName, string pUploadTag, string pTableName, string pColumnName, string[] pKeyColumns, string[] pKeyValues, string[] pKeyDatatypes, string pDisplayName, string pColumnFileName, string pColumnIDAUPD, string pColumnDTUPD, string pParentGUID)
		{
			string listKeyColumns = StrFunc.StringArrayList.StringArrayToStringList(pKeyColumns);
			string listKeyValues = StrFunc.StringArrayList.StringArrayToStringList(pKeyValues);
			string listKeyDatatypes = StrFunc.StringArrayList.StringArrayToStringList(pKeyDatatypes);

			string url = (System.Web.HttpContext.Current.Request.ApplicationPath == @"/" ? string.Empty : System.Web.HttpContext.Current.Request.ApplicationPath) + $"/{pAspxName}.aspx";
			url += $"?UploadTag={pUploadTag}";
			if (StrFunc.IsFilled(pTableName))
				url += "&TableName=" + pTableName;
			if (StrFunc.IsFilled(pColumnName))
				url += "&ColumnName=" + pColumnName;
			if (StrFunc.IsFilled(listKeyColumns))
				url += "&KeyColumns=" + listKeyColumns;
			if (StrFunc.IsFilled(listKeyValues))
				url += "&KeyValues=" + listKeyValues;
			if (StrFunc.IsFilled(listKeyDatatypes))
				url += "&KeyDatatypes=" + listKeyDatatypes;
			if (StrFunc.IsFilled(pDisplayName))
				url += "&DN=" + pDisplayName;
			if (StrFunc.IsFilled(pColumnFileName))
				url += "&ColumnFileName=" + pColumnFileName;
			if (StrFunc.IsFilled(pColumnIDAUPD))
				url += "&ColumnIDAUPD=" + pColumnIDAUPD;
			if (StrFunc.IsFilled(pColumnDTUPD))
				url += "&ColumnDTUPD=" + pColumnDTUPD;

			// PM 20240604 [XXXXX] Ajout pParentGUID 
			if (StrFunc.IsFilled(pParentGUID))
			{
				url += "&GUID=" + pParentGUID;
			}
			
			return $"window.open('{url}','_blank','location=no,scrollbars=no,status=yes');\n";
		}
		#endregion GetWindowOpenUpload & GetWindowOpenDBImageViewer

		#region Manifest resource Javascript registration

		/// <summary>
		/// Enregistre dans la page le script JS présent la resssource incorporée de l'assembly ayant appelée la méthode 
		/// </summary>
		/// <param name="pPage"></param>
		/// <param name="pResourceName">Nom de la ressource</param>
		/// <param name="pKey">Clé du script à inscrire dans la page</param>
		/// <param name="pIsRegisterStartup">True si script de démarrage</param>
		/// <param name="pReplaceParameters"></param>
		public static void RegisterManifestResource(Page pPage, string pResourceName, string pKey, bool pIsRegisterStartup, params string[] pReplaceParameters)
		{
			Type csType = pPage.GetType();

			bool isRegistered;
			if (pIsRegisterStartup)
				isRegistered = pPage.ClientScript.IsStartupScriptRegistered(csType, pKey);
			else
				isRegistered = pPage.ClientScript.IsClientScriptBlockRegistered(csType, pKey);

			if (false == isRegistered)
			{
				Assembly assembly = Assembly.GetCallingAssembly();
				string assemblyName = assembly.GetName().Name;

				Stream stream = assembly.GetManifestResourceStream(assemblyName + "." + pResourceName);
				if (null == stream)
					throw new Exception("Script not found :" + assemblyName + " - " + pResourceName + " - " + assembly.FullName);

				using (StreamReader reader = new StreamReader(stream))
				{
					string script = reader.ReadToEnd();
					#region Replace dynamic variable parameters in script Source
					if ((null != pReplaceParameters) && (0 < pReplaceParameters.Length))
					{
						for (int i = 0; i < pReplaceParameters.Length; i += 2)
						{
							script = script.Replace("{--" + pReplaceParameters[i] + "--}", pReplaceParameters[i + 1]);
						}
					}
					#endregion Replace dynamic variable parameters in script Source

					if (pIsRegisterStartup)
						pPage.ClientScript.RegisterStartupScript(csType, pKey, script);
					else
						pPage.ClientScript.RegisterClientScriptBlock(csType, pKey, script);
				}
			}
		}
		#endregion Manifest resource Javascript registration

		public static void CallFunction(PageBase pPage, string pScript)
		{
			JSStringBuilder sb = new JSStringBuilder();
			sb.AppendLine(pScript);
			string script = sb.ToString();
			pPage.RegisterScript("CallJS", script, true);
		}

	}
}
