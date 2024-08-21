#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Web;

using EFS.GUI.Attributes;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EfsML.Enum.Tools;
using FpML.Enum;

using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.GUI.ComplexControls
{
	#region Specify controls
	#region BookControl
	public sealed class BookControl
	{
		#region Book_Id
		public static void Book_Id(SchemeBox pSchemeBox)
		{
			string id         = pSchemeBox.Controls[0].UniqueID;
			string identifier = string.Empty;
			//
			if (StrFunc.IsFilled(pSchemeBox.Page.Request.Form[id]))
				identifier    = pSchemeBox.Page.Request.Form[id];
			object bookId        = pSchemeBox.FldCapture.GetValue(pSchemeBox.Capture);
			FieldInfo fldValue   = bookId.GetType().GetField("Value");
			FieldInfo fldOTCmlId = bookId.GetType().GetField("otcmlId");

            #region Get Actor Identifier
            FieldInfo fldPartyReference = pSchemeBox.Capture.GetType().GetField("partyReference");
            object partyReference = fldPartyReference.GetValue(pSchemeBox.Capture);
            FieldInfo fldReference = partyReference.GetType().GetField("href");
            object hRef = fldReference.GetValue(partyReference);
            SQL_Actor actor = null;
            if (null != hRef)
                actor = new SQL_Actor(SessionTools.CS, fldReference.GetValue(partyReference).ToString());
            #endregion Get Actor Identifier
            #region Search Book
            SQL_Book book;
            if ((null != actor) && actor.IsLoaded)
                book = new SQL_Book(SessionTools.CS, SQL_TableWithID.IDType.Identifier, identifier, SQL_Table.ScanDataDtEnabledEnum.Yes, actor.Id);
            else
                book = new SQL_Book(SessionTools.CS, SQL_TableWithID.IDType.Identifier, identifier);

			if (book.IsLoaded)
			{
				fldValue.SetValue(bookId,book.Identifier);
				fldOTCmlId.SetValue(bookId,book.Id.ToString());
			}
			else
			{
				fldValue.SetValue(bookId,string.Empty);
				fldOTCmlId.SetValue(bookId,string.Empty);
            }
            #endregion Search Book
        }
		#endregion Book_Id
	}
	#endregion BookControl
	#region FxRateControl
	public sealed class FxRateControl
	{
		#region GetAssetFxRate
		private static SQL_AssetFxRate GetAssetFxRate(SchemeBox pSchemeBox)
		{
			string id         = pSchemeBox.Controls[0].UniqueID;
			string identifier = string.Empty;
			if (StrFunc.IsFilled(pSchemeBox.Page.Request.Form[id]))
				identifier = pSchemeBox.Page.Request.Form[id];
            SQL_AssetFxRate asset_FxRate = new SQL_AssetFxRate(SessionTools.CS, SQL_TableWithID.IDType.Identifier, identifier, SQL_Table.ScanDataDtEnabledEnum.Yes);
			return asset_FxRate;
		}
		#endregion GetAssetFxRate

		#region GetIndexObject
		private static int GetIndexObject(Control pControl)
		{
			int i=0;
			if (pControl.HasControls() && pControl.Controls[0].GetType().Equals(typeof(PlaceHolder)))
				i++;
			return i;
		}
		#endregion GetIndexObject

		#region AssetFxRate_Id
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_Id(SchemeBox pSchemeBox)
		{
			try
			{
				object assetFxRateId = pSchemeBox.FldCapture.GetValue(pSchemeBox.Capture);
				FieldInfo fldValue   = assetFxRateId.GetType().GetField("Value");
				FieldInfo fldOTCmlId = assetFxRateId.GetType().GetField("otcmlId");

				#region QuotedCurrencyPair
				object quotedCurrencyPair       = null;
				object currency1                = null;
				object currency2                = null;
				FieldInfo fldQuotedCurrencyPair = null;
				FieldInfo fldCurrency1          = null;
				FieldInfo fldCurrency1Value     = null;
				FieldInfo fldCurrency2          = null;
				FieldInfo fldCurrency2Value     = null;
				FieldInfo fldQuoteBasis         = null;
				switch (pSchemeBox.CaptureParent.GetType().FullName)
				{
					case "FpML.v44.Fx.FxBarrier":
					case "FpML.v44.Fx.FxEuropeanTrigger":
					case "FpML.v44.Fx.FxAmericanTrigger":
					case "FpML.v44.Shared.FxFixing":
						fldQuotedCurrencyPair = pSchemeBox.CaptureParent.GetType().GetField("quotedCurrencyPair");
						quotedCurrencyPair    = fldQuotedCurrencyPair.GetValue(pSchemeBox.CaptureParent);
						fldCurrency1          = quotedCurrencyPair.GetType().GetField("currency1");
						currency1             = fldCurrency1.GetValue(quotedCurrencyPair);
						fldCurrency1Value     = currency1.GetType().GetField("Value");
						fldCurrency2          = quotedCurrencyPair.GetType().GetField("currency2");
						currency2             = fldCurrency2.GetValue(quotedCurrencyPair);
						fldCurrency2Value     = currency2.GetType().GetField("Value");
						fldQuoteBasis         = quotedCurrencyPair.GetType().GetField("quoteBasis");
						break;
					default:
						break;
				}
				#endregion QuotedCurrencyPair
				#region RateSource
				FieldInfo fldRateSource      = pSchemeBox.Capture.GetType().GetField("rateSource");
				object rateSource            = fldRateSource.GetValue(pSchemeBox.Capture);
				FieldInfo fldRateSourceValue = rateSource.GetType().GetField("Value");
				#endregion RateSource
				#region RateSourcePageSpecified
				FieldInfo fldRateSourcePageSpecified = pSchemeBox.Capture.GetType().GetField("rateSourcePageSpecified");
				//object rateSourcePageSpecified       = fldRateSourcePageSpecified.GetValue(pSchemeBox.Capture);
				#endregion RateSourcePageSpecified
				#region RateSourcePage
				FieldInfo fldRateSourcePage      = pSchemeBox.Capture.GetType().GetField("rateSourcePage");
				object rateSourcePage            = fldRateSourcePage.GetValue(pSchemeBox.Capture);
				FieldInfo fldRateSourcePageValue = rateSourcePage.GetType().GetField("Value");
				#endregion RateSourcePage
				#region RateSourcePageHeadingSpecified
				FieldInfo fldRateSourcePageHeadingSpecified = pSchemeBox.Capture.GetType().GetField("rateSourcePageHeadingSpecified");
				//object rateSourcePageHeadingSpecified       = fldRateSourcePageHeadingSpecified.GetValue(pSchemeBox.Capture);
				#endregion RateSourcePageSpecified
				#region RateSourceHeading
				FieldInfo fldRateSourcePageHeading      = pSchemeBox.Capture.GetType().GetField("rateSourcePageHeading");
				object rateSourcePageHeading            = fldRateSourcePageHeading.GetValue(pSchemeBox.Capture);
				FieldInfo fldRateSourcePageHeadingValue = rateSourcePageHeading.GetType().GetField("Value");
				#endregion RateSourceHeading
				#region OTCmlID
				FieldInfo fldParentOTCmlId = pSchemeBox.Capture.GetType().GetField("otcmlId");
				#endregion OTCmlID

				#region object setting
				SQL_AssetFxRate asset_FxRate = GetAssetFxRate(pSchemeBox);
				if (asset_FxRate.IsLoaded)
				{
					fldValue.SetValue(assetFxRateId,asset_FxRate.Identifier);
					fldOTCmlId.SetValue(assetFxRateId,asset_FxRate.Id.ToString());
					if ((null != fldCurrency1Value) && (null != currency1))
						fldCurrency1Value.SetValue(currency1,asset_FxRate.QCP_Cur1);
					if ((null != fldCurrency2Value) && (null != currency2))
						fldCurrency2Value.SetValue(currency2,asset_FxRate.QCP_Cur2);
					if ((null != fldQuoteBasis) && (null != quotedCurrencyPair))
						fldQuoteBasis.SetValue(quotedCurrencyPair,asset_FxRate.QCP_QuoteBasisEnum);

					fldRateSourceValue.SetValue(rateSource,asset_FxRate.PrimaryRateSrc);
					fldRateSourcePageValue.SetValue(rateSourcePage,asset_FxRate.PrimaryRateSrcPage);
					fldRateSourcePageHeadingValue.SetValue(rateSourcePageHeading,asset_FxRate.PrimaryRateSrcHead);
					fldRateSourcePageSpecified.SetValue(pSchemeBox.Capture,StrFunc.IsFilled(asset_FxRate.PrimaryRateSrcPage));
					fldRateSourcePageHeadingSpecified.SetValue(pSchemeBox.Capture,StrFunc.IsFilled(asset_FxRate.PrimaryRateSrcHead));
					fldParentOTCmlId.SetValue(pSchemeBox.Capture,asset_FxRate.Id.ToString());

					int i = GetIndexObject(pSchemeBox.Parent);
					AssetFxRate_InformationProvider_Render((Scheme)pSchemeBox.Parent.Controls[i+1]);
					AssetFxRate_RateSourcePage_Render((OptionalItem)pSchemeBox.Parent.Controls[i+2]);
					AssetFxRate_RateSourcePageHeading_Render((OptionalItem)pSchemeBox.Parent.Controls[i+3]);

					Item item = null;
					switch (pSchemeBox.CaptureParent.GetType().FullName)
					{
						case "FpML.v44.Fx.FxBarrier":
							if (pSchemeBox.NamingContainer.Parent.Controls[2].GetType().Equals(typeof(Item)))
								item = (Item)pSchemeBox.NamingContainer.Parent.Controls[2];
							else
								item = (Item)pSchemeBox.NamingContainer.Parent.Controls[3];
							break;
						case "FpML.v44.Fx.FxEuropeanTrigger":
							item = (Item)pSchemeBox.NamingContainer.Parent.Controls[4];
							break;
						case "FpML.v44.Fx.FxAmericanTrigger":
							item = (Item)pSchemeBox.NamingContainer.Parent.Controls[4];
							break;
						case "FpML.v44.Shared.FxFixing":
							// 
							if (pSchemeBox.NamingContainer.Parent.Controls[8].GetType().Equals(typeof(Item)))
								item = (Item)pSchemeBox.NamingContainer.Parent.Controls[8];
							else
								// Cas FxFixing Array
								item = (Item)pSchemeBox.NamingContainer.Parent.Controls[9];
							break;
						default:
							break;
					}
					if (null != item)
					{
						AssetFxRate_Currency_Render((Item)item.Controls[0].Controls[0]);
						AssetFxRate_Currency_Render((Item)item.Controls[0].Controls[1]);
						AssetFxRate_QuoteBasis_Render((Scheme)item.Controls[0].Controls[2]);
					}
					
				}
				else
				{
					if ((null != fldCurrency1Value) && (null != currency1))
						fldCurrency1Value.SetValue(currency1,null);
					if ((null != fldCurrency2Value) && (null != currency2))
						fldCurrency2Value.SetValue(currency2,null);

					fldRateSourcePageValue.SetValue(rateSourcePage,null);
					fldRateSourcePageHeadingValue.SetValue(rateSourcePageHeading,null);
					fldRateSourcePageSpecified.SetValue(pSchemeBox.Capture,false);
					fldRateSourcePageHeadingSpecified.SetValue(pSchemeBox.Capture,false);
					ResetAssetFxRate(pSchemeBox);
				}
				#endregion object setting
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_Id
		#region AssetFxRate_InformationProvider
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_InformationProvider(Scheme pScheme)
		{
			try
			{
				int i = GetIndexObject(pScheme.Parent);
				SchemeBox schemeBox = (SchemeBox)pScheme.Parent.Controls[i];
				SQL_AssetFxRate asset_FxRate = GetAssetFxRate(schemeBox);
				if (asset_FxRate.IsLoaded)
				{
					string id         = pScheme.Controls[0].UniqueID;
					string rateSource = string.Empty;
					if (StrFunc.IsFilled(pScheme.Page.Request.Form[id]))
						rateSource = pScheme.Page.Request.Form[id];

					if (asset_FxRate.PrimaryRateSrc != rateSource)
						ResetAssetFxRate(schemeBox);
				}
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_InformationProvider
		#region AssetFxRate_RateSourcePage
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_RateSourcePage(OptionalItem pOptionalItem)
		{
			try
			{
				if (null != pOptionalItem.Parent)
				{
					int i = GetIndexObject(pOptionalItem.Parent);
					SchemeBox schemeBox          = (SchemeBox)pOptionalItem.Parent.Controls[i];
					bool isSpecified             = ((CheckBox) pOptionalItem.Controls[1]).Checked;
					SQL_AssetFxRate asset_FxRate = GetAssetFxRate(schemeBox);
					if (asset_FxRate.IsLoaded)
					{
						if (StrFunc.IsFilled(asset_FxRate.PrimaryRateSrcPage) != isSpecified)
							ResetAssetFxRate(schemeBox);
					}
					FieldInfo fldRateSourcePageSpecified = pOptionalItem.Capture.GetType().GetField("rateSourcePageSpecified");
					fldRateSourcePageSpecified.SetValue(pOptionalItem.Capture,isSpecified);

				}
			}
			catch (Exception){throw;}
		}
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_RateSourcePage(SchemeText pSchemeText)
		{
			try
			{
				int i = GetIndexObject(pSchemeText.NamingContainer.Parent);
				SchemeBox schemeBox = (SchemeBox)pSchemeText.NamingContainer.Parent.Controls[i];
				SQL_AssetFxRate asset_FxRate = GetAssetFxRate(schemeBox);
				if (asset_FxRate.IsLoaded)
				{
					string id             = pSchemeText.Controls[0].UniqueID;
					string rateSourcePage = string.Empty;
					if (StrFunc.IsFilled(pSchemeText.Page.Request.Form[id]))
						rateSourcePage = pSchemeText.Page.Request.Form[id];

					if (asset_FxRate.PrimaryRateSrcPage != rateSourcePage)
						ResetAssetFxRate(schemeBox);
				}
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_RateSourcePage
		#region AssetFxRate_RateSourcePageHeading
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_RateSourcePageHeading(OptionalItem pOptionalItem)
		{
			try
			{
				if (null != pOptionalItem.Parent)
				{
					int i = GetIndexObject(pOptionalItem.Parent);
					SchemeBox schemeBox          = (SchemeBox)pOptionalItem.Parent.Controls[i];
					bool isSpecified             = ((CheckBox) pOptionalItem.Controls[1]).Checked;
					SQL_AssetFxRate asset_FxRate = GetAssetFxRate(schemeBox);
					if (asset_FxRate.IsLoaded)
					{
						if (StrFunc.IsFilled(asset_FxRate.PrimaryRateSrcHead) != isSpecified)
							ResetAssetFxRate(schemeBox);
					}
					FieldInfo fldRateSourcePageSpecified = pOptionalItem.Capture.GetType().GetField("rateSourcePageHeadingSpecified");
					fldRateSourcePageSpecified.SetValue(pOptionalItem.Capture,isSpecified);
				}
			}
			catch (Exception){throw;}

		}
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_RateSourcePageHeading(SimpleString pSimpleString)
		{
			try
			{
				int i = GetIndexObject(pSimpleString.NamingContainer.Parent);
				SchemeBox schemeBox = (SchemeBox)pSimpleString.NamingContainer.Parent.Controls[i];
				SQL_AssetFxRate asset_FxRate = GetAssetFxRate(schemeBox);
				if (asset_FxRate.IsLoaded)
				{
					string id                    = pSimpleString.Controls[0].UniqueID;
					string rateSourcePageHeading = string.Empty;
					if (StrFunc.IsFilled(pSimpleString.Page.Request.Form[id]))
						rateSourcePageHeading = pSimpleString.Page.Request.Form[id];

					if (asset_FxRate.PrimaryRateSrcHead != rateSourcePageHeading)
						ResetAssetFxRate(schemeBox);
				}
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_RateSourcePageHeading

		#region AssetFxRate_Render
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_Render(SchemeBox pSchemeBox)
		{
			try
			{
				#region QuotedCurrencyPairs
				/*
				Item item                         = (Item)pSchemeBox.NamingContainer.Parent.Controls[3];
				PlaceHolder  plhQCP               = (PlaceHolder) item.Controls[0];
				FpMLDropDownList ddlQCP_Currency1 = (FpMLDropDownList) plhQCP.Controls[0].Controls[0].Controls[1].Controls[0];
				FpMLDropDownList ddlQCP_Currency2 = (FpMLDropDownList) plhQCP.Controls[1].Controls[0].Controls[1].Controls[0];
				FpMLDropDownList ddlQCP_Basis     = (FpMLDropDownList) plhQCP.Controls[2].Controls[0];
				string currencyValue              = QuotedCurrencyPair_Currency(pSchemeBox.CaptureParent,"currency1");
				if (StrFunc.IsFilled(currencyValue))
					ddlQCP_Currency1.SelectedValue = currencyValue;
				currencyValue = QuotedCurrencyPair_Currency(pSchemeBox.CaptureParent,"currency2");
				if (StrFunc.IsFilled(currencyValue))
					ddlQCP_Currency2.SelectedValue = currencyValue;
				ddlQCP_Basis.SelectedValue = QuotedCurrencyPair_Basis(pSchemeBox.CaptureParent);
				*/
				#endregion QuotedCurrencyPairs

				#region RateSourcePage
				int i = GetIndexObject(pSchemeBox.Parent);
				OptionalItem oRateSourcePage  = (OptionalItem)pSchemeBox.Parent.Controls[i+2];
				CheckBox chkRateSourcePage    = (CheckBox) oRateSourcePage.Controls[1];
				PlaceHolder plhRateSourcePage = (PlaceHolder) oRateSourcePage.Controls[3];
				chkRateSourcePage.Checked     = RateSourcePageSpecified(pSchemeBox.Capture,"rateSourcePageSpecified");
				oRateSourcePage.IsSpecified   = chkRateSourcePage.Checked;
				plhRateSourcePage.Visible     = chkRateSourcePage.Checked;
				#endregion RateSourcePage
				#region RateSourcePageHeading
				OptionalItem oRateSourcePageHeading  = (OptionalItem)pSchemeBox.Parent.Controls[i+3];
				CheckBox chkRateSourcePageHeading    = (CheckBox) oRateSourcePageHeading.Controls[1];
				PlaceHolder plhRateSourcePageHeading = (PlaceHolder) oRateSourcePageHeading.Controls[3];
				chkRateSourcePageHeading.Checked     = RateSourcePageSpecified(pSchemeBox.Capture,"rateSourcePageHeadingSpecified");
				oRateSourcePageHeading.IsSpecified   = chkRateSourcePageHeading.Checked;
				plhRateSourcePageHeading.Visible     = chkRateSourcePageHeading.Checked;
				#endregion RateSourcePageHeading
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_Render
		#region AssetFxRate_InformationProvider_Render
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_InformationProvider_Render(Scheme pScheme)
		{
			try
			{
				#region RateSource
				FpMLDropDownList ddlRateSource    = (FpMLDropDownList) pScheme.Controls[0];
				string rateSource                 = InformationProvider_RateSource(pScheme.Capture);
				if (StrFunc.IsFilled(rateSource) && ddlRateSource.Items.Contains(new ListItem(rateSource)))
				{
					ddlRateSource.SelectedValue          = rateSource;
					ddlRateSource.Attributes["oldvalue"] = rateSource;
				}
				#endregion RateSource
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_InformationProvider_Render
		#region AssetFxRate_RateSourcePage_Render
		public static void AssetFxRate_RateSourcePage_Render(OptionalItem pOptionalItem)
		{
			CheckBox chkRateSourcePage = (CheckBox) pOptionalItem.Controls[1];
			string rateSourcePage      = RateSourcePage(pOptionalItem.Capture,"rateSourcePage");
			FieldInfo fldParentOTCmlId = pOptionalItem.Capture.GetType().GetField("otcmlId");
			string otcmlId             = (string) fldParentOTCmlId.GetValue(pOptionalItem.Capture);
			chkRateSourcePage.Checked  = StrFunc.IsFilled(otcmlId) && StrFunc.IsFilled(rateSourcePage);
			SchemeText schemeText = (SchemeText) pOptionalItem.Controls[3].Controls[0];
			AssetFxRate_RateSourcePage_Render(schemeText);
		}
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_RateSourcePage_Render(SchemeText pSchemeText)
		{
			try
			{
				#region RateSourcePage
				TextBox txtRateSourcePage = (TextBox) pSchemeText.Controls[0];
				string rateSourcePage     = RateSourcePage(pSchemeText.Capture,"rateSourcePage");
				if (StrFunc.IsFilled(rateSourcePage))
				{
					txtRateSourcePage.Text                   = rateSourcePage;
					txtRateSourcePage.Attributes["oldvalue"] = rateSourcePage;
				}
				#endregion RateSourcePage
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_RateSourcePage_Render
		#region AssetFxRate_RateSourcePageHeading_Render
        public static void AssetFxRate_RateSourcePageHeading_Render(OptionalItem pOptionalItem)
		{
			CheckBox chkRateSourcePageHeading = (CheckBox) pOptionalItem.Controls[1];
			string rateSourcePageHeading      = RateSourcePage(pOptionalItem.Capture,"rateSourcePageHeading");
			FieldInfo fldParentOTCmlId        = pOptionalItem.Capture.GetType().GetField("otcmlId");
			string otcmlId                    = (string) fldParentOTCmlId.GetValue(pOptionalItem.Capture);
			chkRateSourcePageHeading.Checked  = StrFunc.IsFilled(otcmlId) && StrFunc.IsFilled(rateSourcePageHeading);
			SimpleString simpleString         = (SimpleString) pOptionalItem.Controls[3].Controls[0];
			AssetFxRate_RateSourcePageHeading_Render(simpleString);
		}
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_RateSourcePageHeading_Render(SimpleString pSimpleString)
		{
			try
			{
				#region RateSourcePageHeading
				TextBox txtRateSourcePageHeading = (TextBox) pSimpleString.Controls[0];
				string rateSourcePageHeading     = RateSourcePage(pSimpleString.Capture,"rateSourcePageHeading");
				if (StrFunc.IsFilled(rateSourcePageHeading))
				{
					txtRateSourcePageHeading.Text                   = rateSourcePageHeading;
					txtRateSourcePageHeading.Attributes["oldvalue"] = rateSourcePageHeading;
				}
				#endregion RateSourcePageHeading
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_RateSourcePage_Render
		#region AssetFxRate_QuotedCurrency_Render
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_QuoteBasis_Render(Scheme pScheme)
		{
			try
			{
				#region QuoteBasis
				FpMLDropDownList ddlQuoteBasis = (FpMLDropDownList) pScheme.Controls[0];
				string quoteBasis              = ((QuoteBasisEnum) pScheme.FldCapture.GetValue(pScheme.Capture)).ToString();
				if (StrFunc.IsFilled(quoteBasis) && ddlQuoteBasis.Items.Contains(new ListItem(quoteBasis)))
				{
					ddlQuoteBasis.SelectedValue          = quoteBasis;
                    ddlQuoteBasis.Attributes["oldvalue"] = quoteBasis;
				}
				#endregion QuoteBasis
			}
			catch (Exception){throw;}
		}
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void AssetFxRate_Currency_Render(Item pItem)
		{
			try
			{
				#region Currency
				FpMLDropDownList ddlCurrency = (FpMLDropDownList) pItem.Controls[0].Controls[1].Controls[0];
				string currency              = QuotedCurrencyPair_Currency(pItem.Capture,pItem.FldCapture);
				if (StrFunc.IsFilled(currency) && ddlCurrency.Items.Contains(new ListItem(currency)))
				{
					ddlCurrency.SelectedValue          = currency;
                    ddlCurrency.Attributes["oldvalue"] = currency;
				}
				#endregion Currency
			}
			catch (Exception){throw;}
		}
		#endregion AssetFxRate_QuotedCurrency_Render
		
		
		#region InformationProvider_RateSource
        // EG 20180423 Analyse du code Correction [CA2200]
        public static string InformationProvider_RateSource(object pObject)
		{
			string rateSourceValue = string.Empty;
			try
			{
				FieldInfo fldRateSource = pObject.GetType().GetField("rateSource");
				object rateSource       = fldRateSource.GetValue(pObject);
				if (null != rateSource)
				{
					FieldInfo fldRateSourceValue = rateSource.GetType().GetField("Value");
					rateSourceValue              = (string)fldRateSourceValue.GetValue(rateSource);
				}
			}
			catch (Exception){throw;}
			return rateSourceValue;
		}
		#endregion InformationProvider_RateSource
		#region RateSourcePageSpecified
        // EG 20180423 Analyse du code Correction [CA2200]
        public static bool RateSourcePageSpecified(object pObject, string pFieldName)
		{
            bool rateSourcePageSpecified;
            try
			{
				FieldInfo fldSpecified  = pObject.GetType().GetField(pFieldName);
				rateSourcePageSpecified = (bool) fldSpecified.GetValue(pObject);
			}
			catch (Exception){throw;}
			return rateSourcePageSpecified;
		}
		#endregion RateSourcePageSpecified
		#region RateSourcePage
        // EG 20180423 Analyse du code Correction [CA2200]
        private static string RateSourcePage(object pObject, string pFieldName)
		{
			string rateSourcePageValue = string.Empty;
			try
			{
				FieldInfo fldRateSourcePage = pObject.GetType().GetField(pFieldName);
				object rateSourcePage       = fldRateSourcePage.GetValue(pObject);
				if (null != rateSourcePage)
				{
					FieldInfo fldRateSourcePageValue = rateSourcePage.GetType().GetField("Value");
					rateSourcePageValue              = (string)fldRateSourcePageValue.GetValue(rateSourcePage);
				}
			}
			catch (Exception){throw;}
			return rateSourcePageValue;
		}
		#endregion RateSourcePage
		#region QuotedCurrencyPair_Currency
        // EG 20180423 Analyse du code Correction [CA2200]
        private static string QuotedCurrencyPair_Currency(object pObject, FieldInfo pFieldInfo)
		{
			string currencyValue;
			try
			{
				object currency            = pFieldInfo.GetValue(pObject);
				FieldInfo fldCurrencyValue = currency.GetType().GetField("Value");
				currencyValue              = (string) fldCurrencyValue.GetValue(currency);
			}
			catch (Exception){throw;}
			return currencyValue;
		}
		#endregion QuotedCurrencyPair_Currency

		#region ResetAssetFxRate
        // EG 20180423 Analyse du code Correction [CA2200]
        private static void ResetAssetFxRate(SchemeBox pSchemeBox)
		{
			try
			{
				object assetFxRateId = pSchemeBox.FldCapture.GetValue(pSchemeBox.Capture);
				FieldInfo fldValue   = assetFxRateId.GetType().GetField("Value");
				FieldInfo fldOTCmlId = assetFxRateId.GetType().GetField("otcmlId");
				fldValue.SetValue(assetFxRateId,null);
				fldOTCmlId.SetValue(assetFxRateId,null);
				#region OTCmlID
				FieldInfo fldParentOTCmlId = pSchemeBox.Capture.GetType().GetField("otcmlId");
				fldParentOTCmlId.SetValue(pSchemeBox.Capture,null);
				#endregion OTCmlID
			}
			catch (Exception){throw;}
		}
		#endregion ResetAssetFxRate
	}
	#endregion FxRateControl
	#region PartyId
    /// <revision>
    ///     <version>1.2.0</version><date>20071003</date><author>EG</author>
    ///     <comment>
    ///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
    ///     </comment>
    /// </revision>
    /// EG 20170918 [23342] Refactoring
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class PartyId : ControlBase
    {
        #region Constructors
        /// EG 20170918 [23342]
        public PartyId(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
            : base(pCapture, pFldCapture, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {
            bool isFirstPartyId = IsFirstPartyId;



            string name = m_FldCapture.ReflectedType.Name;
            Validator validator = new Validator(name + (StrFunc.IsEmpty(pControlGUI.Name) ? string.Empty : "(" + pControlGUI.Name + ")"), true);
            string helpUrl = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);
            string id = m_FullCtor.GetUniqueID();

            FpMLTextBox txtBox;
            FpMLSchemeBox schemebox = null;
            if (isFirstPartyId)
            {
                schemebox = new FpMLSchemeBox(PartyId_Value, 200, name, pControlGUI, false, helpUrl, validator)
                {
                    ID = id
                };
                Controls.Add(schemebox);

                txtBox = new FpMLTextBox(m_FullCtor, Party_Name, 400, name, "name", string.Empty, true, string.Empty, helpUrl)
                {
                    ID = id + "Name"
                };
                Controls.Add(txtBox);

                txtBox = new FpMLTextBox(m_FullCtor, Party_OTCmlId, 50, name, "OTCml-Id", string.Empty, false, string.Empty, null)
                {
                    ID = id + "OTCml-Id",
                    Visible = false
                };
                Controls.Add(txtBox);
            }
            Controls.Add(new LiteralControl("<br/>"));
            txtBox = new FpMLTextBox(m_FullCtor, PartyId_Scheme, 450, name, "scheme", string.Empty, false, string.Empty, helpUrl)
            {
                ID = id + "Scheme"
            };
            Controls.Add(txtBox);

            txtBox = new FpMLTextBox(m_FullCtor, Party_Id, 150, name, "value", string.Empty, false, string.Empty, null)
            {
                ID = id + "BIC"
            };
            Controls.Add(txtBox);

            if (isFirstPartyId && (null != schemebox))
                OnIdChanged(schemebox, null);

            SetEventHandler();
        }
        #endregion Constructors
        #region Accessors
        #region IsFirstPartyId
        // EG 20170918 [23342] New
        // Test si 1ère ligne du PartyId[] pour afficher les info de la party en cours
        private bool IsFirstPartyId
        {
            get
            {
                bool isFirstPartyId = false;
                Type tCapture = m_Parent.GetType();
                PropertyInfo pty = tCapture.GetProperty("PartyId");
                if (null != pty)
                    isFirstPartyId = (m_Capture == pty.GetValue(m_Parent, null));
                return isFirstPartyId;
            }
        }
        #endregion IsFirstPartyId
        #region PartyId_Value
        // EG 20170918 [23342] Upd
        private string PartyId_Value
        {
            get
            {
                string partyId_Value = string.Empty;
                if (null != m_Capture)
                {
                    FieldInfo fld = m_Capture.GetType().GetField("Value");
                    if (null != fld)
                        partyId_Value = (string)fld.GetValue(m_Capture);
                }
                return partyId_Value;
            }
            set
            {
                if (null != m_Capture)
                {
                    FieldInfo fld = m_Capture.GetType().GetField("Value");
                    if (null != fld)
                        fld.SetValue(m_Capture, value);
                }
            }
        }
        #endregion PartyId_Value
        #region PartyId_Scheme
        // EG 20170918 [23342] Upd
        private string PartyId_Scheme
        {
            get
            {
                string partyId_Scheme = string.Empty;
                if (null != m_Capture)
                {
                    FieldInfo fld = m_Capture.GetType().GetField("partyIdScheme");
                    if (null != fld)
                        partyId_Scheme = (string)fld.GetValue(m_Capture);
                }
                return partyId_Scheme;
            }
            set
            {
                if (null != m_Capture)
                {
                    FieldInfo fld = m_Capture.GetType().GetField("partyIdScheme");
                    if (null != fld)
                        fld.SetValue(m_Capture, value);
                }
            }
        }
        #endregion PartyId_Scheme
        #region Party_OTCmlId
        // EG 20170918 [23342] Upd
        private string Party_OTCmlId
        {
            get
            {
                string otcmlId = string.Empty;
                FieldInfo fld = m_Parent.GetType().GetField("otcmlId");
                if (null != fld)
                    otcmlId = (string)fld.GetValue(m_Parent);
                return otcmlId;
            }
            set
            {
                FieldInfo fld = m_Parent.GetType().GetField("otcmlId");
                if (null != fld)
                    fld.SetValue(m_Parent, value);
            }
        }
        #endregion Party_OTCmlId
        #region Party_Id
        // EG 20170918 [23342] Upd
        private string Party_Id
        {
            get
            {
                string id = string.Empty;
                FieldInfo fld = m_Parent.GetType().GetField("id");
                if (null != fld)
                    id = (string)fld.GetValue(m_Parent);
                return id;
            }
            set
            {
                FieldInfo fld = m_Parent.GetType().GetField("id");
                if (null != fld)
                    fld.SetValue(m_Parent, value);
            }
        }
        #endregion Party_Id
        #region Party_Name
        // EG 20170918 [23342] Upd
        private string Party_Name
        {
            get
            {
                string party_Name = string.Empty;
                FieldInfo fld = m_Parent.GetType().GetField("partyName");
                if (null != fld)
                    party_Name = (string)fld.GetValue(m_Parent);
                return party_Name;
            }
            set
            {
                FieldInfo fld = m_Parent.GetType().GetField("partyName");
                if (null != fld)
                    fld.SetValue(m_Parent, value);
            }
        }
        #endregion Party_Name
        #endregion Accessors
        #region Events
        #region OnPreRender
        // EG 20170918 [23342] Upd
        protected override void OnPreRender(EventArgs e)
        {
            bool isFirstPartyId = IsFirstPartyId;
            string data = PartyId_Value;
            if (false == MethodsGUI.IsStEnvironment_Template(this.Page))
            {
                if (Cst.FpML_EntityOfUserIdentifier == data)
                {
                    ((TextBox)this.Controls[5]).Attributes["oldvalue"] = "0";
                    PartyId_Value = SessionTools.GetDefaultIdentifierEntityOfUser();
                    SearchParty();
                    OnIdChanged(this, null);
                }
            }

            if (isFirstPartyId)
            {
                ((TextBox)this.Controls[0]).Text = PartyId_Value;
                ((TextBox)this.Controls[1]).Text = Party_Name;
                ((TextBox)this.Controls[2]).Text = Party_OTCmlId;
                ((TextBox)this.Controls[5]).Text = Party_Id;
            }
            else
            {
                ((TextBox)this.Controls[2]).Text = Party_Id;

            }
            base.OnPreRender(e);
        }
        #endregion OnPreRender
        #region OnIdChanged
        // EG 20170918 [23342] Upd
        private void OnIdChanged(object sender, EventArgs e)
        {
            if (IsFirstPartyId)
            {
                string oldValue = ((TextBox)this.Controls[5]).Attributes["oldvalue"];
                string oldExtValue = ((TextBox)this.Controls[0]).Attributes["oldvalue"];
                string newValue = Party_OTCmlId;
                string newExtValue = Party_Id;

                m_FullCtor.LoadEnumObjectReference("PartyReference", oldValue, oldExtValue, newValue, newExtValue);
                ((TextBox)this.Controls[5]).Attributes["oldvalue"] = newValue;
                ((TextBox)this.Controls[0]).Attributes["oldvalue"] = newExtValue;
            }
        }
        #endregion OnIdChanged
        #endregion Events
        #region Methods
        #region SaveClass
        // EG 20170918 [23342] Upd
        public void SaveClass(Page pPage)
        {
            bool isFirstPartyId = IsFirstPartyId;
            string partyIdScheme_uniqueID = this.Controls[isFirstPartyId ? 4 : 1].UniqueID;
            string partyIdScheme = string.Empty;
            if (null != pPage.Request.Form[partyIdScheme_uniqueID])
                partyIdScheme = pPage.Request.Form[partyIdScheme_uniqueID].ToString();
            PartyId_Scheme = StrFunc.IsEmpty(partyIdScheme) ? null : partyIdScheme;

            if (isFirstPartyId)
            {
                string partyValue_uniqueID = this.Controls[0].UniqueID;
                if (null != pPage.Request.Form[partyValue_uniqueID])
                {
                    PartyId_Value = pPage.Request.Form[partyValue_uniqueID].ToString() + string.Empty;
                    SearchParty();
                }
            }
        }
        #endregion SaveClass
        #region SearchParty
        // EG 20170918 [23342] Upd
        private void SearchParty()
        {
            string dataToFindInitial = string.Empty;
            if (StrFunc.IsFilled(PartyId_Value))
                dataToFindInitial = PartyId_Value.Replace(" ", "%") + "%";

            SQL_Actor actor = null;
            // SYSTEM est une partie autorisée sur les debtSecurity
            // Pour l'instant il est autorisé sur tous les products
            if ("SYSTEM" == PartyId_Value.ToUpper())
                actor = new SQL_Actor(SessionTools.CS, PartyId_Value, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, SessionTools.User, SessionTools.SessionID);
            //
            if (null == actor)
            {
                actor = new SQL_Actor(SessionTools.CS, dataToFindInitial, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, SessionTools.User, SessionTools.SessionID);
                //20090403 FI [16559] Ajouts des roles CONTACTOFFICE et SETTLTOFFICE
                //20090403 PL Ajouts d'autres roles pour les titres
                //Ex Le CO ou le SO définis sous TRADESIDE doivent avoir un Item dans DataDocument.Party 
                actor.SetRoleRange(new RoleActor[] { RoleActor.COUNTERPARTY, RoleActor.BROKER, 
                                                 RoleActor.ISSUER, RoleActor.GUARANTOR, RoleActor.MANAGER, 
                                                 RoleActor.CONTACTOFFICE, RoleActor.SETTLTOFFICE, RoleActor.DECISIONOFFICE,
                                                 RoleActor.EXECUTION, RoleActor.INVESTDECISION, 
                                                 RoleActor.ENTITY});
            }
            //
            if (((SQL_Table)actor).IsLoaded)
            {
                if (actor.Id.ToString() != Party_OTCmlId)
                {
                    PartyId_Value = ((SQL_Table)actor).Key;
                    Party_OTCmlId = actor.Id.ToString();
                    Party_Name = actor.DisplayName;
                    Party_Id = actor.XmlId.ToString();
                }
            }
            else if (PartyId_Scheme == Cst.Market_Iso10383Scheme)
            {

            }
            else
            {
                //20090422 16300 FI En mode template lorsque l'acteur est inconnu 
                //Party_Name = valeur saisie
                //Party_Id = valeur saisie
                if (MethodsGUI.IsStEnvironment_Template(this.Page))
                {
                    Party_OTCmlId = "-1"; //valeur numérique nécessaire pour que l'acteur puisse être utilisé dans une référence (Payer/receiver)
                    Party_Name = PartyId_Value;
                    Party_Id = PartyId_Value;
                }
                else
                {
                    PartyId_Value = string.Empty;
                    Party_OTCmlId = string.Empty;
                    Party_Name = string.Empty;
                    Party_Id = string.Empty;
                }
            }
        }
        #endregion SearchParty
        #region public SetEventHandler
        // EG 20170918 [23342] Upd
        public void SetEventHandler()
        {
            if (IsFirstPartyId)
                ((TextBox)Controls[0]).TextChanged += new EventHandler(OnIdChanged);
        }
        #endregion public SetEvent
        #endregion Methods
    }
    #endregion PartyId
    #region PartyControl
    /// EG 20170918 [23342] New
    /// EG 20170926 [22374] Add Timezone
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
    public class PartyControl : ControlBase
    {
        #region Constructors
        public PartyControl(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
            : base(pCapture, pFldCapture, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {
            string name = m_FldCapture.ReflectedType.Name;
            Validator validator = new Validator(name + (StrFunc.IsEmpty(pControlGUI.Name) ? string.Empty : "(" + pControlGUI.Name + ")"), true);
            string helpUrl = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);
            string id = m_FullCtor.GetUniqueID();

            FpMLSchemeBox schemebox = new FpMLSchemeBox(PartyId_Value, 100, name, pControlGUI, false, helpUrl, validator)
            {
                ID = id
            };
            Controls.Add(schemebox);

            FpMLTextBox txtBox;
            txtBox = new FpMLTextBox(m_FullCtor, Party_Name, 180, name, "name", string.Empty, true, string.Empty, helpUrl)
            {
                ID = id + "Name"
            };
            Controls.Add(txtBox);

            txtBox = new FpMLTextBox(m_FullCtor, PartyId_Scheme, 250, name, "scheme", string.Empty, false, string.Empty, helpUrl)
            {
                ID = id + "Scheme"
            };
            Controls.Add(txtBox);

            txtBox = new FpMLTextBox(m_FullCtor, Party_OTCmlId, 50, name, "OTCml-Id", string.Empty, false, string.Empty, null)
            {
                ID = id + "OTCml-Id",
                Visible = false
            };
            Controls.Add(txtBox);

            txtBox = new FpMLTextBox(m_FullCtor, Party_Id, 150, name, "BIC", string.Empty, false, string.Empty, null)
            {
                ID = id + "BIC"
            };
            Controls.Add(txtBox);

            txtBox = new FpMLTextBox(m_FullCtor, Party_OTCmlId, 150, name, "Timezone", string.Empty, false, string.Empty, null)
            {
                ID = id + "tz-Id"
            };
            Controls.Add(txtBox);

            SetEventHandler();

            OnIdChanged(schemebox, null);
        }
        #endregion Constructors
        #region Accessors
        #region FpMLPartyId
        private object FpMLPartyId
        {
            get
            {
                return m_Capture;
            }
        }
        #endregion FpMLPartyId
        #region PartyId_Value
        private string PartyId_Value
        {
            get
            {
                string partyId_Value = string.Empty;
                object partyId = FpMLPartyId;
                if (null != partyId)
                {
                    FieldInfo fld = partyId.GetType().GetField("Value");
                    if (null != fld)
                        partyId_Value = (string)fld.GetValue(partyId);
                }
                return partyId_Value;
            }
            set
            {
                object partyId = FpMLPartyId;
                if (null != partyId)
                {
                    FieldInfo fld = partyId.GetType().GetField("Value");
                    if (null != fld)
                        fld.SetValue(partyId, value);
                }

            }
        }
        #endregion PartyId_Value
        #region PartyId_Scheme
        private string PartyId_Scheme
        {
            get
            {
                string partyId_Scheme = string.Empty;
                object partyId = FpMLPartyId;
                if (null != partyId)
                {
                    FieldInfo fld = partyId.GetType().GetField("partyIdScheme");
                    if (null != fld)
                        partyId_Scheme = (string)fld.GetValue(partyId);
                }
                return partyId_Scheme;
            }
            set
            {
                object partyId = FpMLPartyId;
                if (null != partyId)
                {
                    FieldInfo fld = partyId.GetType().GetField("partyIdScheme");
                    if (null != fld)
                        fld.SetValue(partyId, value);
                }

            }

        }
        #endregion PartyId_Scheme
        #region Party_OTCmlId
        private string Party_OTCmlId
        {
            get
            {
                string otcmlId = string.Empty;
                FieldInfo fld = m_Parent.GetType().GetField("otcmlId");
                if (null != fld)
                    otcmlId = (string)fld.GetValue(m_Parent);
                return otcmlId;
            }
            set
            {
                FieldInfo fld = m_Parent.GetType().GetField("otcmlId");
                if (null != fld)
                    fld.SetValue(m_Parent, value);
            }
        }
        #endregion Party_OTCmlId
        #region Party_Id
        private string Party_Id
        {
            get
            {
                string id = string.Empty;
                FieldInfo fld = m_Parent.GetType().GetField("id");
                if (null != fld)
                    id = (string)fld.GetValue(m_Parent);
                return id;
            }
            set
            {
                FieldInfo fld = m_Parent.GetType().GetField("id");
                if (null != fld)
                    fld.SetValue(m_Parent, value);
            }
        }
        #endregion Party_Id
        #region Party_Name
        private string Party_Name
        {
            get
            {
                string party_Name = string.Empty;
                FieldInfo fld = m_Parent.GetType().GetField("partyName");
                if (null != fld)
                    party_Name = (string)fld.GetValue(m_Parent);
                return party_Name;
            }
            set
            {
                FieldInfo fld = m_Parent.GetType().GetField("partyName");
                if (null != fld)
                    fld.SetValue(m_Parent, value);
            }
        }
        #endregion Party_Name
        #region Party_TzdbId
        /// EG 20170926 [22374] New
        private void SetParty_TzdbId(string value)
        {
            FieldInfo fld = m_Parent.GetType().GetField("tzdbid");
            if (null != fld)
                fld.SetValue(m_Parent, value);
        }
        #endregion Party_TzdbId
        #endregion Accessors
        #region Events
        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            string data = PartyId_Value;
            if (false == MethodsGUI.IsStEnvironment_Template(this.Page))
            {
                if (Cst.FpML_EntityOfUserIdentifier == data)
                {
                    ((TextBox)this.Controls[4]).Attributes["oldvalue"] = "0";
                    PartyId_Value = SessionTools.GetDefaultIdentifierEntityOfUser();
                    SearchParty();
                    OnIdChanged(this, null);
                }
            }
            ((TextBox)this.Controls[0]).Text = PartyId_Value;
            ((TextBox)this.Controls[1]).Text = Party_Name;
            ((TextBox)this.Controls[3]).Text = Party_OTCmlId;
            ((TextBox)this.Controls[4]).Text = Party_Id;
            base.OnPreRender(e);
        }
        #endregion OnPreRender
        #region OnIdChanged
        private void OnIdChanged(object sender, EventArgs e)
        {
            string oldValue = ((TextBox)this.Controls[3]).Attributes["oldvalue"];
            string oldExtValue = ((TextBox)this.Controls[0]).Attributes["oldvalue"];
            string newValue = Party_OTCmlId;
            string newExtValue = Party_Id;
            //
            m_FullCtor.LoadEnumObjectReference("PartyReference", oldValue, oldExtValue, newValue, newExtValue);
            ((TextBox)this.Controls[0]).Attributes["oldvalue"] = newExtValue;
            ((TextBox)this.Controls[3]).Attributes["oldvalue"] = newValue;
        }
        #endregion OnIdChanged
        #endregion Events
        #region Methods
        #region SaveClass
        public void SaveClass(Page pPage)
        {
            string partyValue_uniqueID = this.Controls[0].UniqueID;
            string partyIdScheme_uniqueID = this.Controls[2].UniqueID;
            string partyIdScheme = string.Empty;
            if (null != pPage.Request.Form[partyIdScheme_uniqueID])
                partyIdScheme = pPage.Request.Form[partyIdScheme_uniqueID].ToString();
            PartyId_Scheme = StrFunc.IsEmpty(partyIdScheme) ? null : partyIdScheme;
            if (null != pPage.Request.Form[partyValue_uniqueID])
            {
                PartyId_Value = pPage.Request.Form[partyValue_uniqueID].ToString() + string.Empty;
                SearchParty();
            }
        }
        #endregion SaveClass
        #region SearchParty
        /// EG 20170926 [22374] Add timezone.
        private void SearchParty()
        {
            string dataToFindInitial = string.Empty;
            if (StrFunc.IsFilled(PartyId_Value))
                dataToFindInitial = PartyId_Value.Replace(" ", "%") + "%";

            SQL_Actor actor = null;
            // SYSTEM est une partie autorisée sur les debtSecurity
            // Pour l'instant il est autorisé sur tous les products
            if ("SYSTEM" == PartyId_Value.ToUpper())
                actor = new SQL_Actor(SessionTools.CS, PartyId_Value, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, SessionTools.User, SessionTools.SessionID);
            //
            if (null == actor)
            {
                actor = new SQL_Actor(SessionTools.CS, dataToFindInitial, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, SessionTools.User, SessionTools.SessionID);
                //20090403 FI [16559] Ajouts des roles CONTACTOFFICE et SETTLTOFFICE
                //20090403 PL Ajouts d'autres roles pour les titres
                //Ex Le CO ou le SO définis sous TRADESIDE doivent avoir un Item dans DataDocument.Party 
                //actor.SetRoleRange(new RoleActor[] { RoleActor.COUNTERPARTY, RoleActor.BROKER, 
                //                                 RoleActor.ISSUER, RoleActor.GUARANTOR, RoleActor.MANAGER, 
                //                                 RoleActor.CONTACTOFFICE, RoleActor.SETTLTOFFICE });
                actor.SetRoleRange(new RoleActor[] { RoleActor.COUNTERPARTY, RoleActor.BROKER, 
                                                 RoleActor.ISSUER, RoleActor.GUARANTOR, RoleActor.MANAGER, 
                                                 RoleActor.CONTACTOFFICE, RoleActor.SETTLTOFFICE, RoleActor.DECISIONOFFICE,
                                                 RoleActor.EXECUTION, RoleActor.INVESTDECISION, 
                                                 RoleActor.ENTITY});
            }
            //
            if (((SQL_Table)actor).IsLoaded)
            {
                if (actor.Id.ToString() != Party_OTCmlId)
                {
                    PartyId_Value = ((SQL_Table)actor).Key;
                    Party_OTCmlId = actor.Id.ToString();
                    Party_Name = actor.DisplayName;
                    Party_Id = actor.XmlId.ToString();
                    SetParty_TzdbId(actor.TimeZone);
                }
            }
            else
            {
                //20090422 16300 FI En mode template lorsque l'acteur est inconnu 
                //Party_Name = valeur saisie
                //Party_Id = valeur saisie
                if (MethodsGUI.IsStEnvironment_Template(this.Page))
                {
                    Party_OTCmlId = "-1"; //valeur numérique nécessaire pour que l'acteur puisse être utilisé dans une référence (Payer/receiver)
                    Party_Name = PartyId_Value;
                    Party_Id = PartyId_Value;
                    SetParty_TzdbId(Tz.Tools.UniversalTimeZone);
                }
                else
                {
                    PartyId_Value = string.Empty;
                    Party_OTCmlId = string.Empty;
                    Party_Name = string.Empty;
                    Party_Id = string.Empty;
                    SetParty_TzdbId(string.Empty);
                }
            }
        }
        #endregion SearchParty
        #region public SetEventHandler
        public void SetEventHandler()
        {
            ((TextBox)Controls[0]).TextChanged += new EventHandler(OnIdChanged);
        }
        #endregion public SetEvent
        #endregion Methods
    }
    #endregion PartyControl
	#region RateIndexControl
	public sealed class RateIndexControl
	{
		#region FloatingRateIndex
		public static object FloatingRateIndex(object pObject,FieldInfo pFieldInfo)
		{
			return pFieldInfo.GetValue(pObject);
		}
		#endregion FloatingRateIndex
		#region FloatingRateIndexValue
		public static string FloatingRateIndexValue(object pObject,FieldInfo pFieldInfo)
		{
            string ret = string.Empty;
            object floatingRateIndex = FloatingRateIndex(pObject, pFieldInfo);
            FieldInfo fld = floatingRateIndex.GetType().GetField("Value");
            //
            if (null != fld)
                ret = (string)fld.GetValue(floatingRateIndex);
            //
            return ret;
		}
		public static void FloatingRateIndexValue(object pObject,FieldInfo pFieldInfo,string pValue,int pOTCmlId)
		{
            object floatingRateIndex = FloatingRateIndex(pObject, pFieldInfo);
            FieldInfo fldValue = floatingRateIndex.GetType().GetField("Value");
            FieldInfo fldOTCmlId = floatingRateIndex.GetType().GetField("otcmlId");
            if (null != fldValue)
                fldValue.SetValue(floatingRateIndex, pValue);
            if (null != fldOTCmlId)
                fldOTCmlId.SetValue(floatingRateIndex, pOTCmlId.ToString());
		}
		#endregion FloatingRateIndexValue
		#region FloatingRateIndexTenor
		public static object FloatingRateIndexTenor(object pObject)
		{
			FieldInfo fld = pObject.GetType().GetField("indexTenor");
			if (null != fld)
				return fld.GetValue(pObject);
			return null;
		}
		public static void FloatingRateIndexTenor(object pObject,string pPeriod,int pMultiplier)
		{
			object tenor                  = FloatingRateIndexTenor(pObject);
			FieldInfo fldPeriod           = tenor.GetType().GetField("period");
			FieldInfo fldPeriodMultiplier = tenor.GetType().GetField("periodMultiplier");
			if (null != fldPeriodMultiplier)
				fldPeriod.SetValue(tenor, StringToEnum.Period(pPeriod));
			if (null != fldPeriodMultiplier)
			{
				object periodMultiplier = fldPeriodMultiplier.GetValue(tenor);
				FieldInfo fld     = periodMultiplier.GetType().GetField("Value");
				fld.SetValue(periodMultiplier,pMultiplier.ToString());		
			}
		}
		#endregion FloatingRateIndexTenor
		#region FloatingRateIndexTenorSpecified
		public static bool FloatingRateIndexTenorSpecified(object pObject)
		{
			bool isSpecified = false;
			FieldInfo fld = pObject.GetType().GetField("indexTenor" + Cst.FpML_SerializeKeySpecified);
			if (null != fld)
				isSpecified = (bool) fld.GetValue(pObject);
			return isSpecified;
		}
		public static void FloatingRateIndexTenorSpecified(object pObject,bool pIsSpecified)
		{
			FieldInfo fld = pObject.GetType().GetField("indexTenor" + Cst.FpML_SerializeKeySpecified);
			if (null != fld)
				fld.SetValue(pObject,pIsSpecified);
		}
		#endregion FloatingRateIndexTenorSpecified
		#region TenorMultiplier
		public static int TenorMultiplier(object pObject)
		{
			int multiplier = 0;
			try
			{
				object tenor = FloatingRateIndexTenor(pObject);
				FieldInfo fldPeriodMultiplier = tenor.GetType().GetField("periodMultiplier");
				if (null != fldPeriodMultiplier)
				{
					object periodMultiplier = fldPeriodMultiplier.GetValue(tenor);
					FieldInfo fld           = periodMultiplier.GetType().GetField("Value");
					multiplier              = Convert.ToInt32(fld.GetValue(periodMultiplier));
				}
			}
			catch (Exception){}
			return multiplier;
		}
		#endregion TenorMultiplier
		#region TenorPeriod
		public static string TenorPeriod(object pObject)
		{
			string period = string.Empty;
			try
			{
				object tenor  = FloatingRateIndexTenor(pObject);
				FieldInfo fld = tenor.GetType().GetField("period");
				if (null != fld)
					period = fld.GetValue(tenor).ToString();
			}
			catch (Exception){}
			return period;
		}
		#endregion TenorPeriod

		#region RateIndex_Id
		#region RateIndex_Id (Acces by PeriodMultiplier / Period)
        public static void RateIndex_Id(Control pControl)
        {
            SchemeBox schemeBox;
            if (pControl.NamingContainer.NamingContainer.GetType().Equals(typeof(ObjectArray)))
                schemeBox = (SchemeBox)pControl.Parent.NamingContainer.Parent.Controls[1];
            else
                schemeBox = (SchemeBox)pControl.NamingContainer.NamingContainer.Controls[0].Controls[0];

            Control ctrlParent = pControl.Parent;
            int multiplier = IntFunc.IntValue((((TextBox)ctrlParent.Controls[0].Controls[0]).Text));
            string period = ((DropDownList)ctrlParent.Controls[1].Controls[0]).SelectedValue;

            string floatingRateIndex_Value = FloatingRateIndexValue(schemeBox.Capture, schemeBox.FldCapture);

            if (StrFunc.IsFilled(floatingRateIndex_Value))
                RateIndex_Id(schemeBox.Capture, schemeBox.FldCapture, floatingRateIndex_Value, multiplier, period);
        }
		#endregion RateIndex_Id (Acces by PeriodMultiplier / Period)
		#region RateIndex_Id (Acces by RateIndex)
		public static void RateIndex_Id(SchemeBox pSchemeBox) 
		{
			string id        = pSchemeBox.Controls[0].UniqueID;
			string rateIndex = string.Empty;
			if (StrFunc.IsFilled(pSchemeBox.Page.Request.Form[id]))
				rateIndex = pSchemeBox.Page.Request.Form[id];
			
			RateIndex_Id(pSchemeBox.Capture,pSchemeBox.FldCapture,rateIndex,0,null);
			
		}
		#endregion RateIndex_Id (Acces by RateIndex)
		#region RateIndex_Id (Common)
		
		public static void RateIndex_Id(object pCapture,FieldInfo pFldCapture,string pRateIndex,int pMultiplier,string pPeriod) 
		{
			//string otcmlId          = string.Empty;
			bool   isAssetWithTenor = false;

			#region FloatingRateIndex + Tenor objects
			object tenor            = FloatingRateIndexTenor(pCapture);
			bool isTenorSpecified   = FloatingRateIndexTenorSpecified(pCapture);
			#endregion FloatingRateIndex + Tenor objects
			#region Search RateIndex
			
			SQL_AssetRateIndex rate = new SQL_AssetRateIndex(SessionTools.CS, SQL_AssetRateIndex.IDType.RateIndex_IDISDA,pRateIndex,
				SQL_Table.ScanDataDtEnabledEnum.Yes);
			

			if (0 != pMultiplier && StrFunc.IsFilled(pPeriod))
			{
				rate.Asset_PeriodMltp_In = pMultiplier;
				rate.Asset_Period_In     = pPeriod;
			}
			else if (isTenorSpecified && (null != tenor))
			{
				rate.Asset_PeriodMltp_In = TenorMultiplier(pCapture);
				rate.Asset_Period_In     = TenorPeriod(pCapture);
			}

			if (rate.IsLoaded)
			{
				#region Rate exist
				#region RateIndex setting
				//string floatingRateIndex_Value   = string.Empty;
				//string floatingRateIndex_OTCmlId = string.Empty;
				FloatingRateIndexValue(pCapture,pFldCapture,
					StrFunc.IsFilled(rate.Idx_IdIsda)?rate.Idx_IdIsda:rate.Idx_Identifier,rate.IdAsset); 
				#endregion RateIndex setting
				#region Tenor setting

				isAssetWithTenor = rate.Asset_RateIndexWithTenor; 
				if (isAssetWithTenor)
					FloatingRateIndexTenor(pCapture,rate.Asset_Period_Tenor,rate.Asset_PeriodMltp_Tenor);
				#endregion Tenor setting
				#endregion Rate exist
			}
			else
			{
				#region Rate doesn't exist
				FloatingRateIndexValue(pCapture,pFldCapture,string.Empty,0);
				FloatingRateIndexTenor(pCapture,PeriodEnum.M.ToString(),0);
				#endregion Rate doesn't exist
			}
			#endregion Search RateIndex
			#region TenorSpecified Setting
			FloatingRateIndexTenorSpecified(pCapture,isAssetWithTenor);
			#endregion TenorSpecified Setting
		}
		
		#endregion RateIndex_Id (Common)
		
		#endregion RateIndex_Id
		#region RateIndex_Render (Acces by RateIndex)
		public static void RateIndex_Render(SchemeBox pSchemeBox)
		{
			try
			{
				OptionalItem ctrlTenor = null;
				if (pSchemeBox.NamingContainer.GetType().Equals(typeof(ObjectArray)))
					ctrlTenor = (OptionalItem) pSchemeBox.Parent.Controls[3];
				else
					ctrlTenor = (OptionalItem) pSchemeBox.NamingContainer.Controls[0].Controls[2];

				CheckBox     chkTenor       = (CheckBox) ctrlTenor.Controls[1];
				PlaceHolder  plhTenor       = (PlaceHolder) ctrlTenor.Controls[3];
				TextBox      txtMultiplier  = (TextBox) plhTenor.Controls[0].Controls[0];
				DropDownList ddlPeriod      = (DropDownList) plhTenor.Controls[1].Controls[0];
				chkTenor.Checked            = FloatingRateIndexTenorSpecified(pSchemeBox.Capture);
				ctrlTenor.IsSpecified       = chkTenor.Checked;
				plhTenor.Visible            = chkTenor.Checked;
				txtMultiplier.Text          = TenorMultiplier(pSchemeBox.Capture).ToString();
				ddlPeriod.SelectedValue     = TenorPeriod(pSchemeBox.Capture);
			}
			catch (Exception){}
		}
		#endregion RateIndex_Render (Acces by RateIndex)
		#region RateIndex_Render (Acces by PeriodMultiplier / Period)
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void RateIndex_Render(Control pControl)
		{
			try
			{
				SchemeBox schemeBox = null;
				if (pControl.NamingContainer.GetType().Equals(typeof(ObjectArray)))
					schemeBox = (SchemeBox)pControl.Parent.Controls[1];
				else
					schemeBox = (SchemeBox)pControl.NamingContainer.Controls[0].Controls[0];
				RateIndex_Render(schemeBox);
			}
			catch (Exception){throw;}
		}
		#endregion RateIndex_Render (Acces by PeriodMultiplier / Period)
		
	}
	#endregion RateIndexControl
	#region ReferenceBankControl
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class ReferenceBankControl : ControlBase
	{
		#region Constructors
		public ReferenceBankControl(object pCapture,FieldInfo pFldCapture,ControlGUI pControlGUI,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)
		{
			string name         = m_FldCapture.ReflectedType.Name;
			string helpUrl      = MethodsGUI.GetHelpLink(m_Capture,m_FldCapture);
			Validator validator = new Validator(name,true);
			string  id          = m_FullCtor.GetUniqueID();
            //
            Controls.Add(new FpMLTextBox(m_FullCtor, ReferenceBankId_Value, 150, name, pControlGUI, string.Empty, false, id + "Value" , null, validator));
			Controls.Add(new FpMLTextBox(m_FullCtor,ReferenceBank_Name,150,name,"name",string.Empty,false,id + "Name",helpUrl));
			Controls.Add(new FpMLTextBox(m_FullCtor,ReferenceBankId_Scheme,300,name,"scheme",string.Empty,false,id + "Scheme",helpUrl));
		}
		#endregion Constructors
		#region Accessors
		#region ReferenceBankId
		private object FpMLReferenceBankId
		{
			get
			{
				Type tCapture = m_Capture.GetType();
				FieldInfo fld = tCapture.GetField("referenceBankId");
				if (null != fld)
					return fld.GetValue(m_Capture);
				else
					return null;
			}
		}
		#endregion FpMLReferenceBankId
		#region ReferenceBankId_Value
		private string ReferenceBankId_Value
		{
			get
			{
				string referenceBankId_Value = string.Empty;
				object referenceId           = FpMLReferenceBankId;
				if (null!= referenceId)
				{
					FieldInfo fld = referenceId.GetType().GetField("Value");
					if (null != fld)
						referenceBankId_Value = (string) fld.GetValue(referenceId);
				}
				return referenceBankId_Value;
			}
			set
			{
				object referenceId = FpMLReferenceBankId;
				if (null!= referenceId)
				{
					FieldInfo fld = referenceId.GetType().GetField("Value");
					if (null != fld)
						fld.SetValue(referenceId,value);
				}

			}
		}
		#endregion ReferenceBankId_Value
		#region ReferenceBankId_Scheme
		private string ReferenceBankId_Scheme
		{
			get
			{
				string referenceBankId_Scheme = string.Empty;
				object referenceId            = FpMLReferenceBankId;
				if (null!= referenceId)
				{
					FieldInfo fld = referenceId.GetType().GetField("referenceBankIdScheme");
					if (null != fld)
						referenceBankId_Scheme = (string) fld.GetValue(referenceId);
				}
				return referenceBankId_Scheme;
			}
			set
			{
				object referenceId = FpMLReferenceBankId;
				if (null!= referenceId)
				{
					FieldInfo fld = referenceId.GetType().GetField("referenceBankIdScheme");
					if (null != fld)
						fld.SetValue(referenceId,value);
				}

			}
		}
		#endregion ReferenceBankId_Value
		#region ReferenceBank_Name
		private string ReferenceBank_Name
		{
			get
			{
				string referenceBank_Name = string.Empty;
				FieldInfo fld = m_Capture.GetType().GetField("referenceBankName");
				if (null != fld)
					referenceBank_Name = (string) fld.GetValue(m_Capture);
				return referenceBank_Name;
			}
			set
			{
				FieldInfo fld = m_Capture.GetType().GetField("referenceBankName");
				if (null != fld)
					fld.SetValue(m_Capture,value);
			}
		}
		#endregion Party_Name
		#endregion Accessors
		#region Methods
		#region SaveClass
		public void SaveClass(PageBase  pPage)
		{
			    
			string referenceBankIdValue_uniqueID  = this.Controls[0].UniqueID;
			string referenceBankName_uniqueID     = this.Controls[1].UniqueID;
			string referenceBankIdScheme_uniqueID = this.Controls[2].UniqueID;
			//
            string referenceBankName              = string.Empty;
			string referenceBankIdValue           = string.Empty;
			string referenceBankIdScheme          = string.Empty;
			//
            if (null != pPage.Request.Form[referenceBankIdValue_uniqueID])
				referenceBankIdValue = pPage.Request.Form[referenceBankIdValue_uniqueID].ToString();
			if (null != pPage.Request.Form[referenceBankName_uniqueID])
				referenceBankName = pPage.Request.Form[referenceBankName_uniqueID].ToString();
			if (null != pPage.Request.Form[referenceBankIdScheme_uniqueID])
				referenceBankIdScheme = pPage.Request.Form[referenceBankIdScheme_uniqueID].ToString();
            //
			ReferenceBankId_Value  = referenceBankIdValue;
			ReferenceBank_Name     = StrFunc.IsEmpty(referenceBankName)?null:referenceBankName;
			ReferenceBankId_Scheme = StrFunc.IsEmpty(referenceBankIdScheme)?null:referenceBankIdScheme;
		}
		#endregion SaveClass
		#endregion Methods
	}
	#endregion ReferenceBankControl
	#endregion Specify controls

	#region Global Objects Complex Controls Types
	#region ControlBase
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field m_GrandParent and m_FldGrandParent (used to determine REGEX type for derived classes
	///     and associated properties
	///     </comment>
	/// </revision>
	/// <revision>
	///     <version>1.2.0</version><date>20071101</date><author>EG</author>
	///     <comment>
	///     Ticket 15890 : Copy/Paste Toolbar of a business data object (FullScreen and Zoom)
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class ControlBase : Control, INamingContainer
	{
		#region Members
        protected object               m_Capture;
		protected object               m_Parent;
		protected object               m_GrandParent;
		protected FieldInfo            m_FldCapture;
		protected FieldInfo            m_FldParent;
		protected FieldInfo            m_FldGrandParent;
		protected FullConstructor      m_FullCtor;
		protected ControlGUI		   m_ControlGUI;
		#endregion Members

		#region Accessors
		#region Capture
		public object Capture {get {return m_Capture;}}
		#endregion Capture
		#region CaptureParent
		public object CaptureParent {get {return m_Parent;}}
		#endregion CaptureParent
		#region CaptureGrandParent
		public object CaptureGrandParent {get {return m_GrandParent;}}
		#endregion CaptureGrandParent

		#region CopyFieldName
		public string CopyFieldName {get {return m_FldCapture.Name;}}
		#endregion CopyFieldName
		#region CopyObjectName
		public string CopyObjectName {get {return m_Capture.GetType().Name;}}
		#endregion CopyObjectName
		#region CopyObjectValueName
		public string CopyObjectValueName 
		{
			get 
			{
				string objectValueName = CopyObjectName;
				object oChild = m_FldCapture.GetValue(m_Capture);
				if (null != oChild)
					objectValueName = oChild.GetType().Name;
				return objectValueName;
			}
		}
		#endregion CopyObjectValueName

		#region CopyTitleName
		public string CopyTitleName 
		{
			get {return CopyObjectValueName + " - " + CopyFieldName;}
		}
		#endregion CopyTitleName

		#region FldCapture
		public FieldInfo FldCapture {get {return m_FldCapture;}}
		#endregion FldCapture
		#region FldCaptureParent
		public FieldInfo FldCaptureParent {get {return m_FldParent;}}
		#endregion FldCaptureParent
		#region FldCaptureGrandParent
		public FieldInfo FldCaptureGrandParent {get {return m_FldGrandParent;}}
		#endregion FldCaptureGrandParent

		#region IsFullScreen
		public bool IsFullScreen {get {return null == this.Context.Request.QueryString["Object"];}}
		#endregion IsFullScreen
		#region IsFieldChoice
		public bool IsFieldChoice 
		{
			get 
			{
				return m_FldCapture.FieldType.Equals(typeof(EFS_RadioChoice)) ||
					   m_FldCapture.FieldType.Equals(typeof(EFS_DropDownChoice));
			}
		}
		#endregion IsFieldChoice

		#region ObjectCapture
		public object ObjectCapture 
		{
			get 
			{
				object oChild = m_FldCapture.GetValue(m_Capture);
				if ((null != oChild) && (false == oChild.GetType().IsArray))
					return oChild;
				return m_Capture;
			}
		}
		#endregion ObjectCapture
		#region TypeCapture
		public Type TypeCapture 
		{
			get {return ObjectCapture.GetType();}
		}
		#endregion TypeCapture
		#region TypeCaptureName
		public string TypeCaptureName 
		{
			get {return TypeCapture.Name;}
		}
		#endregion TypeCaptureName
		#endregion Accessors

		#region Constructors
		public ControlBase(){}
		public ControlBase(object pCapture,FieldInfo pFldCapture,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
            //
            
            m_Capture        = pCapture;
			m_Parent         = pParent;
			m_GrandParent    = pGrandParent;
			m_FldCapture     = pFldCapture;
			m_FldParent      = pFldParent;
			m_FldGrandParent = pFldGrandParent;
			m_FullCtor       = pFullCtor;
            ID               = m_FullCtor.GetUniqueID();  
		}
		#endregion Constructors

		#region Methods
		#region CopyPaste
		public void CopyPaste(Cst.OperatorType pOperatorType,int pIdClipBoard,string pDisplayName)
		{
			EFS_SerializeInfoBase serializeInfo;
			switch (pOperatorType)
			{
				case Cst.OperatorType.copy:
					#region Copy
					string objectID = GetObjectID();
                    serializeInfo = new EFS_SerializeInfoBase(TypeCapture, ObjectCapture)
                    {
                        Source = Cst.EFSmL_Name
                    };
                    StringBuilder sb = CacheSerializer.Serialize(serializeInfo);
					SessionTools.SessionClipBoard.Insert(TypeCaptureName,CopyFieldName,objectID,pDisplayName,sb.ToString());
					break;
					#endregion Copy
				case Cst.OperatorType.paste:
				case Cst.OperatorType.pastechoice:
					#region Paste
					// Read SessionClipboard table to get last or selected item copied
					string xml = SessionTools.SessionClipBoard.GetItemClipBoard(TypeCaptureName, pIdClipBoard);
					if (StrFunc.IsFilled(xml))
					{
						PlaceHolder plh  = new PlaceHolder();
                        #region Set to current object after deserialization
                        serializeInfo = new EFS_SerializeInfoBase(TypeCapture, xml)
                        {
                            Source = Cst.EFSmL_Name
                        };
                        object oCapture = CacheSerializer.Deserialize(serializeInfo);
						if (oCapture.GetType().Equals(m_FldCapture.DeclaringType) || oCapture.GetType().BaseType.Equals(m_FldCapture.DeclaringType))
						{
							object oPaste   = m_FldCapture.GetValue(oCapture);
							if (null != oPaste)
							{
								m_FldCapture.SetValue(m_Capture,oPaste);
								FieldInfo fldSpecified = TypeCapture.GetField(CopyFieldName + Cst.FpML_SerializeKeySpecified);
								if (null != fldSpecified)
									fldSpecified.SetValue(m_Capture,true);
								m_FullCtor.Display(m_Capture,m_FldCapture,ref plh,m_Parent,m_FldParent,m_GrandParent,m_FldGrandParent);
								PasteControl(plh,false);
							}
							else if (IsFieldChoice)
							{
								#region Choice Case

								bool isChoiceFound                 = false;
								FieldInfo[] fldsChoice             = oCapture.GetType().GetFields();
								FieldInfo fldSelectedSpecified     = null;
								FieldInfo fldSelectedNoneSpecified = null;
								FieldInfo fldSelected;
								foreach (FieldInfo fldChoice in fldsChoice)
								{
									if (0 == fldChoice.Name.IndexOf(m_FldCapture.Name))
									{
										if (-1 < fldChoice.Name.IndexOf(Cst.FpML_SerializeKeySpecified))
										{
											fldSelected = oCapture.GetType().GetField(fldChoice.Name.Replace(Cst.FpML_SerializeKeySpecified,string.Empty));
											if (null != fldSelected)
											{
												// Item du choice sélectionné sur la copie
												if ((bool)fldChoice.GetValue(oCapture))
												{
													fldSelectedSpecified = fldChoice;
													object oPasteItem = fldSelected.GetValue(oCapture);
													if (null != oPasteItem)
													{
														fldSelectedSpecified = fldChoice;
														fldSelected.SetValue(m_Capture,oPasteItem);
														isChoiceFound = true;
													}
												}
												else if (fldSelected.FieldType.Name.Equals("Empty"))
												{
													fldSelectedNoneSpecified = fldChoice;
													isChoiceFound        = true;
												}
												else
												{
													fldChoice.SetValue(m_Capture,false);
												}
											}
										}
									}
								}
								if (isChoiceFound)
								{
									if (null != fldSelectedNoneSpecified)
										fldSelectedNoneSpecified.SetValue(m_Capture,(null == fldSelectedSpecified));
									if (null != fldSelectedSpecified)
										fldSelectedSpecified.SetValue(m_Capture,true);
									m_FullCtor.Read(m_Capture,ref plh,m_Parent,m_FldParent,m_GrandParent,m_FldGrandParent);
									PasteControl(plh,true);
								}
								#endregion Choice Case
							}
							else if ((null != m_FldParent) && oCapture.GetType().Equals(m_FldParent.FieldType))
							{
								m_Capture = oCapture;
								m_FldParent.SetValue(m_Parent,m_Capture);
								FieldInfo fldSpecified = m_Parent.GetType().GetField(m_FldParent.Name + Cst.FpML_SerializeKeySpecified);
								if (null != fldSpecified)
									fldSpecified.SetValue(m_Parent,true);
								m_FullCtor.Read(m_Capture,ref plh,m_Parent,m_FldParent,m_GrandParent,m_FldGrandParent);
								PasteControl(plh,true);
							}
						}
						else if (oCapture.GetType().Equals(m_FldCapture.FieldType))
						{
							m_FldCapture.SetValue(m_Capture,oCapture);
							FieldInfo fldSpecified = m_Capture.GetType().GetField(CopyFieldName + Cst.FpML_SerializeKeySpecified);
							if (null != fldSpecified)
								fldSpecified.SetValue(m_Capture,true);
							m_FullCtor.Display(m_Capture,m_FldCapture,ref plh,m_Parent,m_FldParent,m_GrandParent,m_FldGrandParent);
							PasteControl(plh,false);
						}
						else if ((null != m_FldParent) && oCapture.GetType().Equals(m_FldParent.FieldType))
						{
							m_Capture = oCapture;
							m_FldParent.SetValue(m_Parent,m_Capture);
							FieldInfo fldSpecified = m_Parent.GetType().GetField(m_FldParent.Name + Cst.FpML_SerializeKeySpecified);
							if (null != fldSpecified)
								fldSpecified.SetValue(m_Parent,true);
							m_FullCtor.Read(m_Capture,ref plh,m_Parent,m_FldParent,m_GrandParent,m_FldGrandParent);
							PasteControl(plh,true);
						}
						#endregion Set to current object after deserialization
						#region Create Controls and subtitution
						//PasteControl(plh);
						#endregion Create Controls and subtitution
					}
					break;
					#endregion Paste
			}
		}
		#endregion CopyPaste
		#region CopyPasteItem
		public void CopyPasteItem(Cst.OperatorType pOperatorType,int pIdClipBoard,int pItemArray,string pDisplayName)
		{
			EFS_SerializeInfoBase serializeInfo;
			object objectItem = ((System.Array)m_FldCapture.GetValue(m_Capture)).GetValue(pItemArray);
			Type   tObjectItem = objectItem.GetType();
			switch (pOperatorType)
			{
				case Cst.OperatorType.copyitem:
					#region Copy
					// Insert into SessionClipboard table
					string objectID  = GetObjectID(objectItem);
                    serializeInfo = new EFS_SerializeInfoBase(tObjectItem, objectItem)
                    {
                        Source = Cst.EFSmL_Name
                    };
                    StringBuilder sb = CacheSerializer.Serialize(serializeInfo);
					SessionTools.SessionClipBoard.Insert(tObjectItem.Name,CopyFieldName,objectID,pDisplayName,sb.ToString());
					break;
					#endregion copy
				case Cst.OperatorType.pasteitem:
				case Cst.OperatorType.pastechoiceitem:
					#region Paste
					// Read SessionClipboard table to get last or selected item copied
					string xml = SessionTools.SessionClipBoard.GetItemClipBoard(tObjectItem.Name,pIdClipBoard);
					if (StrFunc.IsFilled(xml))
					{
						PlaceHolder plh = new PlaceHolder();
                        #region Set to current object after deserialization
                        serializeInfo = new EFS_SerializeInfoBase(tObjectItem, xml)
                        {
                            Source = Cst.EFSmL_Name,
                            NameSpace = Cst.EFSmL_Namespace_3_0
                        };
                        object oCapture = CacheSerializer.Deserialize(serializeInfo);
						((System.Array)m_FldCapture.GetValue(m_Capture)).SetValue(oCapture,pItemArray);
						m_FullCtor.Read(oCapture,ref plh,m_Capture,m_FldCapture,m_Capture,m_FldCapture);
						#endregion Set to current object after deserialization
						#region Create Controls and subtitution
						object[] attributes             = m_FldCapture.GetCustomAttributes(typeof(ArrayDivGUI),true);
						ArrayDivGUI m_ArrayDivGUI       = (ArrayDivGUI) attributes[0];
						ControlCollection ctrlContainer = this.Controls[pItemArray + (m_ArrayDivGUI.IsMaster?1:0)].Controls;
						int offsetChild = (m_ArrayDivGUI.IsChild?1:0);
						for (int i = 0;i<plh.Controls.Count;i++)
							ctrlContainer.RemoveAt(offsetChild);
						for (int i = plh.Controls.Count-1;0<=i;i--)
							ctrlContainer.AddAt(offsetChild,plh.Controls[i]);

						#endregion Create Controls and subtitution
					}
					break;
					#endregion Paste
			}
		}
		#endregion CopyPasteItem
		#region GetObjectID
		public string GetObjectID()
		{
			return GetObjectID(null);
		}
		public string GetObjectID(object pObjectContainsID)
		{
			object objectContainsID = pObjectContainsID;
			string id               = string.Empty;				
			if (null == pObjectContainsID)
			{
				if (IsFieldChoice)
				{
					#region Read objects Item of Choice
					FieldInfo[] fldsChoice = m_Capture.GetType().GetFields();
					foreach (FieldInfo fldChoice in fldsChoice)
					{
						if (0 == fldChoice.Name.IndexOf(m_FldCapture.Name))
						{
							if (-1 < fldChoice.Name.IndexOf(Cst.FpML_SerializeKeySpecified))
							{
								FieldInfo fldSelected = m_Capture.GetType().GetField(fldChoice.Name.Replace(Cst.FpML_SerializeKeySpecified,string.Empty));
								if (null != fldSelected)
								{
									
									if ((bool)fldChoice.GetValue(m_Capture))
									{
										// Item is found
										objectContainsID = fldSelected.GetValue(m_Capture);
										break;
									}
								}
							}
						}
					}
					#endregion Read objects Item of Choice
				}
				else
				{
					objectContainsID = m_FldCapture.GetValue(m_Capture);
				}
			}
			if (null != objectContainsID)
			{
				FieldInfo fldID = objectContainsID.GetType().GetField(Cst.FpML_EFSPrefixClass.ToLower() + Cst.FpML_IdAttribute);
				if (null != fldID)
					id = ((EFS_Id) fldID.GetValue(objectContainsID)).Value;
			}
			return id;
		}
		#endregion GetObjectID
		#region PasteControl
		private void PasteControl(PlaceHolder pPlhToPaste,bool pIsNamingContainer)
		{
			Type tObject = this.GetType();
			ControlCollection ctrlContainer;
			Control ctrlToPaste;
			if (pIsNamingContainer)
			{
				ctrlContainer = this.NamingContainer.Controls[0].Controls;
				ctrlToPaste   = pPlhToPaste;
				ctrlContainer.Clear();
				ctrlContainer.Add(ctrlToPaste);
			}
			else if (tObject.Equals(typeof(Item)) || tObject.Equals(typeof(OptionalItem)))
			{
				ctrlContainer = this.Controls;
				if (m_FldCapture.IsDefined(typeof(CloseDivGUI),true) && m_FldCapture.IsDefined(typeof(OpenDivGUI),true))
					ctrlToPaste = pPlhToPaste.Controls[2];
				else if (m_FldCapture.IsDefined(typeof(CloseDivGUI),true) || m_FldCapture.IsDefined(typeof(OpenDivGUI),true))
					ctrlToPaste = pPlhToPaste.Controls[1];
				else
					ctrlToPaste = pPlhToPaste;

				ctrlContainer.Clear();
				ctrlContainer.Add(ctrlToPaste);
			}
			else if (tObject.Equals(typeof(ObjectArray)))
			{
				ctrlContainer = this.NamingContainer.Controls;
				ctrlToPaste   = pPlhToPaste;
				ctrlContainer.Clear();
				ctrlContainer.Add(ctrlToPaste);
			}

			/*
			if (tObject.Equals(typeof(Item)) || tObject.Equals(typeof(OptionalItem)))
			{
				ctrlContainer = this.Controls;
				if (m_FldCapture.IsDefined(typeof(CloseDivGUI),true) && m_FldCapture.IsDefined(typeof(OpenDivGUI),true))
					ctrlToPaste = pPlhToPaste.Controls[2];
				else if (m_FldCapture.IsDefined(typeof(CloseDivGUI),true) || m_FldCapture.IsDefined(typeof(OpenDivGUI),true))
					ctrlToPaste = pPlhToPaste.Controls[1];
				else
					ctrlToPaste = pPlhToPaste;

				ctrlContainer.Clear();
				ctrlContainer.Add(ctrlToPaste);
			}
			else if (tObject.Equals(typeof(Choice)))
			{
				ctrlContainer = this.NamingContainer.Controls[0].Controls;
				ctrlToPaste   = pPlhToPaste;
				ctrlContainer.Clear();
				ctrlContainer.Add(ctrlToPaste);
			}
			else if (tObject.Equals(typeof(ObjectArray)))
			{
				ctrlContainer = this.NamingContainer.Controls;
				ctrlToPaste   = pPlhToPaste;
				ctrlContainer.Clear();
				ctrlContainer.Add(ctrlToPaste);
			}
			*/
		}
		#endregion GetContainerToPaste
        #endregion Methods
    }
	#endregion ControlBase
	#region Choice
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class Choice : ControlBase
	{
		#region Members
		private readonly int m_StyleChoice;
		private readonly MethodsGUI.LevelEnum m_Level;
		private readonly ArrayList m_aChoice;
		private readonly ArrayList m_aChoiceSpecified;
		private readonly string[]             m_LabelChoice;
		private string m_SelectedDropDown;
		private int m_SelectedRadioButton;
		private readonly bool m_IsExtendibleLevel;
		#endregion Members
		//
        #region Constructors
        public Choice(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, int pStyleChoice, FullConstructor pFullCtor)
            : base(pCapture, pFldCapture, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {

            #region private Members
            m_aChoice = new ArrayList();
            m_aChoiceSpecified = new ArrayList();
            m_SelectedRadioButton = -1;
            m_StyleChoice = pStyleChoice;
            m_Level = pControlGUI.Level;
            m_IsExtendibleLevel = MethodsGUI.IsExtendibleLevel(m_Level);
            string helpURL = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);
            string name = m_FldCapture.Name;
            #endregion
            //
            #region Fields of choice reading
            FieldInfo[] fieldInfos = m_Capture.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (0 == fieldInfo.Name.IndexOf(name))
                {
                    if (-1 < fieldInfo.Name.IndexOf(Cst.FpML_SerializeKeySpecified))
                        m_aChoiceSpecified.Add(fieldInfo);
                    else if (name != fieldInfo.Name)
                        m_aChoice.Add(fieldInfo);
                }
            }
            #endregion Fields of choice reading
            //
            #region Choice Selected value setting
            m_LabelChoice = new string[m_aChoice.Count];
            m_SelectedDropDown = ((FieldInfo)m_aChoice[0]).Name.Replace(name, string.Empty);
            m_SelectedRadioButton = 0;
            for (int i = 0; i < m_aChoice.Count; i++)
            {
                m_LabelChoice[i] = ((FieldInfo)m_aChoice[i]).Name.Replace(name, string.Empty);
                if ((bool)((FieldInfo)m_aChoiceSpecified[i]).GetValue(m_Capture))
                {
                    m_SelectedDropDown = m_LabelChoice[i];
                    m_SelectedRadioButton = i;
                }
            }
            #endregion Choice Selected value setting
            //
            #region OpenDivGUI
            if (m_IsExtendibleLevel)
                Controls.Add(MethodsGUI.MakeDiv(m_FullCtor, new OpenDivGUI(m_Level, pControlGUI.Name, pControlGUI.IsDisplay, helpURL, 0, pControlGUI.Color), true));
            #endregion OpenDivGUI
            //
            #region Control Choice Creating
            switch (m_StyleChoice)
            {
                case 1:
                    #region DropdownList
                    Dictionary<string, string> lstChoice = new Dictionary<string, string>();
                    for (int i = 0; i < m_aChoice.Count; i++)
                        lstChoice.Add(m_aChoice[i].ToString(), m_LabelChoice[i].ToString());

                    FpMLDropDownList ddl = new FpMLDropDownList(m_FullCtor, m_aChoice[m_SelectedRadioButton].ToString(), pControlGUI.Width, name, pControlGUI, helpURL, lstChoice)
                    {
                        ID = m_FullCtor.GetUniqueID(),
                        AutoPostBack = true
                    };
                    Controls.Add(ddl);
                    break;
                    #endregion DropdownList
                default:
                    #region RadioButton
                    FpMLRadioButton rbl = new FpMLRadioButton(m_FullCtor, pControlGUI, name, 0, m_LabelChoice)
                    {
                        ID = m_FullCtor.GetUniqueID(),
                        SelectedIndex = m_SelectedRadioButton
                    };
                    Controls.Add(rbl);
                    #endregion RadioButton
                    break;
            }
            #endregion Control Choice Creating
            //
            #region OpenDivGUI
            if (m_IsExtendibleLevel)
            {
                OpenDivGUI openDiv = new OpenDivGUI(MethodsGUI.GetLevelKey(m_Level), pControlGUI.Name, pControlGUI.IsDisplay, helpURL, 0, pControlGUI.Color)
                {
                    UniqueId = MethodsGUI.CreateUniqueID(m_FullCtor, m_Parent, m_FldParent, m_Capture, m_FldCapture, 0)
                };
                Controls.Add(MethodsGUI.MakeDiv(m_FullCtor, openDiv, true));
            }
            #endregion OpenDivGUI
            //
            #region Choice Item CreateControl Calling
            //object objChoice;
            PlaceHolder plh;
            FpMLCopyPasteButton btnCopyPaste;
            for (int i = 0; i < m_aChoice.Count; i++)
            {
                plh = new PlaceHolder();
                FieldInfo fldChoice = (FieldInfo)m_aChoice[i];
                ControlGUI controlGUI = MethodsGUI.GetControl(fldChoice);
                //
                if (fldChoice.IsDefined(typeof(OpenDivGUI), true))
                {
                    object[] attributes = fldChoice.GetCustomAttributes(typeof(OpenDivGUI), true);
                    OpenDivGUI openDiv = (OpenDivGUI)attributes[0];
                    openDiv.HelpURL = MethodsGUI.GetHelpLink(m_Capture, fldChoice);
                    openDiv.UniqueId = MethodsGUI.CreateUniqueID(m_FullCtor, m_Capture, m_FldCapture, m_Capture, fldChoice, 0);
                    plh.Controls.Add(MethodsGUI.MakeDiv(m_FullCtor, attributes[0], false));
                }
                //
                //objChoice = fldChoice.GetValue(m_Capture);
                if (fldChoice.FieldType.IsArray)
                    plh.Controls.Add(new ComplexControls.ObjectArray(m_Capture, fldChoice, controlGUI, m_Capture, m_FldCapture, m_Parent, m_FldParent, m_FullCtor));
                else if (fldChoice.FieldType.IsEnum || fldChoice.FieldType.BaseType.Equals(typeof(SchemeGUI)))
                    plh.Controls.Add(new ComplexControls.Scheme(m_Capture, fldChoice, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
                else if (-1 < fldChoice.FieldType.Name.IndexOf(Cst.FpML_EFSPrefixClass))
                    m_FullCtor.Display(m_Capture, fldChoice, ref plh, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, true, true, true);
                else if (fldChoice.FieldType.BaseType.Equals(typeof(HrefGUI)))
                    plh.Controls.Add(new ComplexControls.Href(m_Capture, fldChoice, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
                else if (fldChoice.FieldType.BaseType.Equals(typeof(SchemeBoxGUI)))
                    plh.Controls.Add(new ComplexControls.SchemeBox(m_Capture, fldChoice, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
                else
                    plh.Controls.Add(new ComplexControls.Item(m_Capture, fldChoice, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
                //
                if (fldChoice.IsDefined(typeof(OpenDivGUI), true))
                {
                    object[] attributes = fldChoice.GetCustomAttributes(typeof(OpenDivGUI), true);
                    // CopyPaste
                    OpenDivGUI openDiv = (OpenDivGUI)attributes[0];
                    if (openDiv.IsCopyPaste)
                    {
                        plh.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
                        btnCopyPaste = new FpMLCopyPasteButton(m_FullCtor.GetUniqueID(),openDiv.Name, TypeCaptureName, CopyFieldName, false, true);
                        plh.Controls.Add(btnCopyPaste);
                    }
                    plh.Controls.Add(MethodsGUI.MakeDiv(m_FullCtor, new CloseDivGUI(openDiv.Level)));
                }
                //
                Controls.Add(plh);
            }
            m_FullCtor.IsChildrenInclude = false;
            #endregion Choice Item CreateControl Calling
            //
            #region CopyPaste
            if (pControlGUI.IsCopyPaste)
            {
                Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
                btnCopyPaste = new FpMLCopyPasteButton(m_FullCtor.GetUniqueID(),pControlGUI.Name, TypeCaptureName, CopyFieldName, false, true);
                Controls.Add(btnCopyPaste);
            }
            #endregion
            //
            #region CloseDivGUI
            if (m_IsExtendibleLevel)
                Controls.Add(MethodsGUI.MakeDiv(new CloseDivGUI(m_Level)));
            #endregion CloseDivGUI
            //
            #region Display Choice SelectedValue
            switch (m_StyleChoice)
            {
                case 1:
                    DisplayChildSelectedList();
                    break;
                default:
                    DisplayChildRadio();
                    break;
            }
            #endregion
            //
            SetEventHandler(); 
        
        }
		#endregion Constructors
		//
        #region Events
		#region DDL_SelectedIndexChanged
		private void DDL_SelectedIndexChanged(object sender,EventArgs e) 
		{
			DisplayChildSelectedList();
		}
		#endregion DDL_SelectedIndexChanged
		#region RBL_SelectedIndexChanged
		private void RBL_SelectedIndexChanged(object sender,EventArgs e) 
		{
			DisplayChildRadio();
		}
		#endregion RBL_SelectedIndexChanged
        #endregion Events
		//
        #region Methods
		#region DisplayChildSelectedList
		private void DisplayChildSelectedList() 
		{
			int i = (m_IsExtendibleLevel?1:0);
			string selectedValue = ((FpMLDropDownList) Controls[i]).SelectedValue;
			if (StrFunc.IsFilled(selectedValue))
			{
				((FpMLDropDownList) Controls[i]).SetSelectedValue = selectedValue;
				m_SelectedDropDown = selectedValue;
			}
            //
			i = (m_IsExtendibleLevel?3:1);
			for (int k=0;k<m_aChoice.Count;k++)
			{
				FieldInfo fldChoice = (FieldInfo)m_aChoice[k];
				Controls[k+i].Visible = (m_SelectedDropDown == fldChoice.Name.Replace(m_FldCapture.Name,string.Empty));
			}
		}
		#endregion DisplayChildSelectedList
		#region DisplayChildRadio
		private void DisplayChildRadio() 
		{
			int i = (m_IsExtendibleLevel?1:0);
			int j = (m_IsExtendibleLevel?3:1);

			int lastChoice        = m_SelectedRadioButton;
			m_SelectedRadioButton = ((FpMLRadioButton) Controls[i]).SelectedIndex;
			for (int k=0;k<m_aChoice.Count;k++)
			{
				Controls[k+j].Visible=(k == m_SelectedRadioButton);
			}
			// Reset des éventuels id des childrens de même niveau (exemple : businessCenters.Id)
			// dans le cas où le radio bouton sélectionné est de type FpML.Shared.Empty
			MethodInfo methodToInvoke = m_Capture.GetType().GetMethod("ProcessReference_IdSibling");
			if ((null != methodToInvoke) && (lastChoice != m_SelectedRadioButton))
			{
				String [] argNames  = new String [] {"pFldPrevious","pFldNext","pFullCtor"};
				object[] argValues  = new object [] {(FieldInfo)m_aChoice[lastChoice],(FieldInfo)m_aChoice[m_SelectedRadioButton],m_FullCtor};
				m_Capture.GetType().InvokeMember(methodToInvoke.Name,BindingFlags.InvokeMethod,null,m_Capture,argValues,null,null,argNames);
			}
		}
		#endregion DisplayChildRadio
		#region SaveClass
        // EG 20180423 Analyse du code Correction [CA2200]
        public void SaveClass()
		{
			int i = (m_IsExtendibleLevel?1:0);
			try
			{
				switch (m_StyleChoice)
				{
					case 1:
						ListItem item = ((FpMLDropDownList)this.Controls[i]).SelectedItem;
						for (int j=0;j<m_aChoiceSpecified.Count;j++)
						{
							FieldInfo fldChoiceSpecified = (FieldInfo) m_aChoiceSpecified[j];
							FieldInfo fldChoice          = (FieldInfo) m_aChoice[j];
							fldChoiceSpecified.SetValue(m_Capture, fldChoice.Name.Replace(m_FldCapture.Name, string.Empty) == item.Text);
						}
						break;
					default:
						m_SelectedRadioButton = ((FpMLRadioButton) this.Controls[i]).SelectedIndex;
						for (int j=0;j<m_aChoiceSpecified.Count;j++)
						{
							((FieldInfo) m_aChoiceSpecified[j]).SetValue(m_Capture,(m_SelectedRadioButton==j));
						}
						break;
				}
			}
			catch(Exception) 
			{
				throw; 
			}
		}
		#endregion SaveClass
        #region public SetEventHandler
        public void SetEventHandler()
        {
            int i = (m_IsExtendibleLevel ? 1 : 0);
            switch (m_StyleChoice)
            {
                case 1:
                    #region DropdownList
                    FpMLDropDownList ddl = (FpMLDropDownList)Controls[i];
                    ddl.SelectedIndexChanged += new System.EventHandler(DDL_SelectedIndexChanged);
                    break;
                    #endregion DropdownList
                default:
                    #region RadioButton
                    FpMLRadioButton rbl = (FpMLRadioButton)Controls[i];
                    rbl.SelectedIndexChanged += new System.EventHandler(RBL_SelectedIndexChanged);
                    #endregion RadioButton
                    break;
            }
        }
        #endregion
        #endregion Methods
    }
	#endregion Choice
	#region DateCalendar
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    /// EG 20170918 [23342] Upd 
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class DateCalendar : ControlBase
	{
		#region Constructors
        // EG 20180423 Analyse du code Correction [CA2200]
        public DateCalendar(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)
		{
			try
			{
				//20071002 FI Ticket 15800 Refactoring
				DateTime dtValue         = DateTime.MinValue;
				object obj               = m_FldCapture.GetValue(m_Capture);
				EFSRegex.TypeRegex regex = EFSRegex.TypeRegex.RegexDate;
				//
				if (ReflectionTools.IsBaseType(m_FldCapture,typeof(DateCalendarGUI)))
				{
                    if (null != obj)
                    {
                        FieldInfo fldDate = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                        object objResult = fldDate.GetValue(obj);
                        if (null != objResult)
                        {
                            try
                            {
                                dtValue = new DtFunc().StringToDateTime(objResult.ToString());
                            }
                            catch { dtValue = DateTime.MinValue; }
                        }
                    }
				}
				else if (m_Capture.GetType().Equals(typeof(EFS_DateArray)))
					dtValue = ((EFS_DateBase)m_Capture).DateValue;
				else if (m_Capture.GetType().Equals(typeof(EFS_DateTimeArray)))
				{
					dtValue = ((EFS_DateBase)m_Capture).DateTimeValue;
					regex   = EFSRegex.TypeRegex.RegexDateTime;
				}
				else
					dtValue = (DateTime)obj;
				//
				string name    = MethodsGUI.GetValidationSummaryMessage(m_FldCapture,m_FldParent);
				string helpUrl = MethodsGUI.GetHelpLink(m_Capture,m_FldCapture);

                // EG 20170918 [23342]
                FpMLCalendarBox calendar = null;
                if (MethodsGUI.IsValidatorOptionalControl(m_FldCapture))
                    calendar = new FpMLCalendarBox(m_FullCtor, dtValue, m_FldCapture.Name, pControlGUI, helpUrl);
                else
                    calendar = new FpMLCalendarBox(m_FullCtor, dtValue, m_FldCapture.Name, pControlGUI, helpUrl,
                    new Validator(name, true), new Validator(regex, name, true), GetValidatorDateRange(name));

                calendar.ID = m_FullCtor.GetUniqueID();
                Controls.Add(calendar);
            }
			catch(Exception) 
			{
				throw; 
			}
		}
		#endregion Constructors 
		#region Events
		#region protected override Render
        // EG 20180423 Analyse du code Correction [CA2200]
        protected override void Render(HtmlTextWriter writer)
        {
            try
            {
                //20071002 FI Ticket 15800 => Refactoring 
                object obj = m_FldCapture.GetValue(m_Capture);
                DateTime dtValue = DateTime.MinValue;
                string date = string.Empty;
                //
                //FI 20091118 [16744] sur un template il y interprétation de la donnée si l'on sur est un zoom
                bool isUseInterpretation = true;
                if (MethodsGUI.IsStEnvironment_Template(this.Page))
                    isUseInterpretation = MethodsGUI.IsZoomOnFull(this.Page);
                //
                if (isUseInterpretation)
                {
                    CustomCaptureInfo cci = null;
                    //
                    if (ReflectionTools.IsBaseType(m_FldCapture, typeof(DateCalendarGUI))
                        ||
                        (m_Capture.GetType().Equals(typeof(EFS_DateArray))))
                    {
                        cci = new CustomCaptureInfo
                        {
                            DataType = TypeData.TypeDataEnum.date
                        };
                    }
                    else if (m_Capture.GetType().Equals(typeof(EFS_DateTimeArray)))
                    {
                        cci = new CustomCaptureInfo
                        {
                            DataType = TypeData.TypeDataEnum.datetime
                        };
                    }
                    else
                    {
                        cci = new CustomCaptureInfo
                        {
                            DataType = TypeData.TypeDataEnum.date
                        };
                    }
                    //
                    if (null != cci)
                    {
                        if (ReflectionTools.IsBaseType(m_FldCapture, typeof(DateCalendarGUI)))
                        {
                            if (null != obj)
                            {
                                FieldInfo fldDate = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                                object objResult = fldDate.GetValue(obj);
                                if (null != objResult)
                                    cci.NewValue = objResult.ToString();
                            }
                        }
                        else if (m_Capture.GetType().Equals(typeof(EFS_DateArray)) ||
                            m_Capture.GetType().Equals(typeof(EFS_DateTimeArray)))
                        {
                            cci.NewValue = ((EFS_DateBase)m_Capture).Value;
                        }
                        else
                        {
                            cci.NewValue = DtFunc.DateTimeToString(dtValue, DtFunc.FmtISODate);
                        }
                        //				
                        date = cci.NewValueFmtToCurrentCulture;
                        ((TextBox)Controls[0]).Text = date;
                    }
                }
                else
                {
                    if (null != obj)
                    {
                        FieldInfo fldDate = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                        if (null != fldDate)
                        {
                            object objresult = fldDate.GetValue(obj);
                            if (null != objresult)
                                ((TextBox)(Controls[0])).Text = objresult.ToString();
                        }
                    }
                }
                //
                base.Render(writer);
            }
            catch (Exception)
            {
                throw;
            }
        }
		#endregion Render
		#endregion Events
		#region Methods
		#region private GetValidatorDateRange
		private Validator GetValidatorDateRange(string pHeaderMessage)
		{
			return Validator.GetValidatorDateRange(pHeaderMessage,m_FldCapture.Name);
		}
		#endregion GetValidatorDateRange
		#region public SaveClass
        // EG 20180423 Analyse du code Correction [CA2200]
        public void SaveClass(PageBase page)
		{
			try
			{
				//20071002 FI Ticket 15800 => Refactoring  
				//Date value
				string Date_uniqueID = this.Controls[0].UniqueID;
				string val           = page.Request.Form[Date_uniqueID];
                //
                //FI 20091118 [16744] sur un template il y interprétation de la donnée si l'on sur est un zoom
                bool isUseInterpretation = true;
                if (MethodsGUI.IsStEnvironment_Template(this.Page))
                    isUseInterpretation = MethodsGUI.IsZoomOnFull(this.Page);
                //
                if (isUseInterpretation)
				{
					CustomCaptureInfo cci = new CustomCaptureInfo();
					//
					if (ReflectionTools.IsBaseType(m_FldCapture,typeof(DateCalendarGUI)) ||
						(m_Capture.GetType().Equals(typeof(EFS_DateArray))))
					{
                        cci = new CustomCaptureInfo
                        {
                            DataType = TypeData.TypeDataEnum.date
                        };
                    }
					else if (m_Capture.GetType().Equals(typeof(EFS_DateTimeArray)) )
					{
                        cci = new CustomCaptureInfo
                        {
                            DataType = TypeData.TypeDataEnum.datetime
                        };
                    }
					if (null != cci)
					{
						try
						{
							//Try cath en cas d'interpretation qui bug => string.Empty ds ce cas 
							cci.NewValueFromLiteral = val;
							val                 = cci.NewValue;   		 
						}
						catch{ val = string.Empty;}
					}
					//
				}
				//
				if (ReflectionTools.IsBaseType(m_FldCapture,typeof(DateCalendarGUI)))
				{
					object obj = m_FldCapture.GetValue(m_Capture);
					if (null != obj)
					{
						FieldInfo fldDate = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
						fldDate.SetValue(obj,val);
					}
				}
				else if (m_Capture.GetType().Equals(typeof(EFS_DateArray)))
					((EFS_DateBase)m_Capture).Value = val ;
				else if (m_Capture.GetType().Equals(typeof(EFS_DateTimeArray)) )
					((EFS_DateTimeArray)m_Capture).Value = val ;
				else 
				{
					DateTime dt          = DateTime.MinValue;
					if (StrFunc.IsFilled(val))
					{
						try 
						{
							dt = new DtFunc().StringToDateTime(val);  
						}
						catch (Exception){}
					}
					m_FldCapture.SetValue(m_Capture,dt);
				}
			
			}
			catch(Exception) 
			{
				throw; 
			}
		}
		#endregion SaveClass
		#endregion Methods
	}
	#endregion DateCalendar
	#region ElementId
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class ElementId : ControlBase
	{
		#region Constructors
		public ElementId(object pCapture,FieldInfo pFldCapture, object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)
		{
			FieldInfo[] fieldInfos = m_Capture.GetType().GetFields();
			ControlGUI controlGUI;
			int width;
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				if ((fieldInfo.FieldType.Namespace == "System") && (false == MethodsGUI.IsNoneControl(fieldInfo)))
				{
					controlGUI			= MethodsGUI.GetControl(fieldInfo);
					width				= (0 != controlGUI.Width) ? controlGUI.Width : 150;
					object obj			= fieldInfo.GetValue(m_Capture);
					string element		= (null == obj)?string.Empty:obj.ToString();
					string name         = MethodsGUI.GetValidationSummaryMessage(m_FldCapture,m_FldParent);
					Validator validator = new Validator(name,true);
					FpMLTextBox txt;
					if ((Cst.EFSmL_IdAttribute != fieldInfo.Name) && (Cst.FpML_OTCmlIdAttribute != fieldInfo.Name))
					{
						if (MethodsGUI.IsValidatorOptionalControl(fieldInfo))
							txt = new FpMLTextBox(m_FullCtor,element,width,name,controlGUI,string.Empty,false,null,null);
						else
							txt = new FpMLTextBox(m_FullCtor,element,width,name,controlGUI,string.Empty,false,null,null,validator);
						//txt.Visible = (Cst.FpML_OTCmlIdAttribute != fieldInfo.Name);
						txt.ID = m_FullCtor.GetUniqueID();  
						Controls.Add(txt);
					}
				}
			}
		}
		#endregion Constructors
        
        #region Methods
        #region SaveClass
        public void SaveClass(PageBase page)
		{
            int j = 0;
            FieldInfo[] fieldInfos = m_Capture.GetType().GetFields();
            string elementValue = string.Empty;
            string elementUniqueID;
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                if ((fieldInfos[i].FieldType.Namespace == "System") &&
                    (false == MethodsGUI.IsNoneControl(fieldInfos[i])))
                {
                    elementUniqueID = this.Controls[j].UniqueID;
                    if (null != page.Request.Form[elementUniqueID])
                        elementValue = page.Request.Form[elementUniqueID].ToString();
                    fieldInfos[i].SetValue(m_Capture, StrFunc.IsEmpty(elementValue) ? null : elementValue);
                    j++;
                }
            }
		}
		#endregion SaveClass
		#endregion Methods
	}
	#endregion ElementId
	#region Href
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class Href : ControlBase
	{
		#region Members
		private readonly string m_FpMLKeyReference;
		#endregion Variables
		#region Constructors
		public Href(object pCapture,FieldInfo pFldCapture,ControlGUI pControlGUI,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)
		{
			bool isHrefFldCapture = (Cst.FpML_hRefAttribute == m_FldCapture.Name);
			m_FpMLKeyReference    = MethodsGUI.GetReference(m_FldCapture);
			string href           = string.Empty;
			object obj            = m_FldCapture.GetValue(m_Capture);
			if (null != obj)
			{
				if (isHrefFldCapture)
					href = (string) obj;
				else
				{
					FieldInfo fld;
					if (obj.GetType().Equals(typeof(EFS_Href)))
						fld = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
					else
						fld = obj.GetType().GetField(Cst.FpML_hRefAttribute);
					href    = (fld.GetValue(obj)+string.Empty).ToString();
				}
			}
			int ctrlWidth = 150;
			if ((null != pControlGUI) && (0 != pControlGUI.Width))
				ctrlWidth = pControlGUI.Width;
			string helpUrl = MethodsGUI.GetHelpLink(m_Capture,m_FldCapture);
            //
            FpMLDropDownList ddl = new FpMLDropDownList(m_FullCtor, href, ctrlWidth, m_FpMLKeyReference, pControlGUI, helpUrl)
            {
                ID = m_FullCtor.GetUniqueID()
            };
            Controls.Add(ddl);
		}
		#endregion Constructors
		#region Methods
		#region SaveClass
		public void SaveClass(PageBase page)
		{
			string hRef_uniqueID = this.Controls[0].UniqueID;
			string hRef          = null;
			if (null != page.Request.Form[hRef_uniqueID])
			{
				hRef = page.Request.Form[hRef_uniqueID].ToString();
				DropDownList ddl = (DropDownList) this.Controls[0];
				ControlsTools.DDLSelectByValue(ddl,hRef);
				hRef = ddl.SelectedItem.Text;
				((FpMLDropDownList) this.Controls[0]).SetSelectedValue = hRef;
			}

			if (Cst.FpML_hRefAttribute == m_FldCapture.Name)
				m_FldCapture.SetValue(m_Capture,StrFunc.IsEmpty(hRef)?null:hRef);
			else
			{
				object obj = m_FldCapture.GetValue(m_Capture);
				if (null != obj)
				{
					FieldInfo fld;
					if (obj.GetType().Equals(typeof(EFS_Href)))
						fld = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
					else
						fld = obj.GetType().GetField(Cst.FpML_hRefAttribute);
					fld.SetValue(obj,StrFunc.IsEmpty(hRef)?null:hRef);
				}
			}
		}
		#endregion SaveClass
		#endregion Methods
	}
	#endregion Href
	#region Item
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class Item : ControlBase
	{
		public Item(object pCapture,FieldInfo pFldCapture,ControlGUI pControlGUI,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)
		{
			
			//System.Diagnostics.Debug.WriteLine(pCapture.ToString());
            //System.Diagnostics.Debug.WriteLine(pFldCapture.ToString());
			//
			PlaceHolder plh = new PlaceHolder();
			if ((null != pControlGUI) && (pControlGUI.IsLabel))
			{
				string helpUrl = MethodsGUI.GetHelpLink(m_Capture,m_FldCapture);
				plh.Controls.Add(new FpMLLabelOnly(pControlGUI,helpUrl));
			}
            //
            object oItem = m_FldCapture.GetValue(m_Capture);
            if (null == oItem)
                throw new NullReferenceException(StrFunc.AppendFormat("Field[{0}] is null in instance[{1}]", m_FldCapture.ToString(), pCapture.ToString() ));  
            //
            m_FullCtor.Read(oItem,ref plh,m_Capture,m_FldCapture,m_Parent,m_FldParent);
			m_FullCtor.IsChildrenInclude = false;
			Controls.Add(plh);
			// CopyPaste
			if ((null != pControlGUI) && (pControlGUI.IsCopyPaste))
			{
				FpMLCopyPasteButton btnCopyPaste = new FpMLCopyPasteButton(m_FullCtor.GetUniqueID(),pControlGUI.Name,TypeCaptureName,CopyFieldName,false,true);
				Controls.Add(btnCopyPaste);
			}
		}
    }



#endregion Item		
	#region Node
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class Node : ControlBase
	{
		#region Constructors
		public Node(object pCapture,FieldInfo pFldCapture,ControlGUI pControlGUI,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)
		{
			PlaceHolder plh = new PlaceHolder();
			string nodeValue = string.Empty;
			int m_NbItem; 
			object oItem = m_FldCapture.GetValue(m_Capture);
			if (null != oItem)
			{
				FieldInfo fldNodes = oItem.GetType().GetField("Any");
				if (null != fldNodes)
				{
					object nodes = fldNodes.GetValue(oItem);
					if (null != nodes)
					{
                        Array aNodes = (Array)nodes;
                        m_NbItem = aNodes.Length;
						for (int i=0;i<m_NbItem;i++)
						{
                            XmlNode node = (XmlNode)aNodes.GetValue(i);
							nodeValue   += node.OuterXml.Replace("xmlns=\"" + node.NamespaceURI + "\"",string.Empty) + Cst.CrLf;
							
						}
						nodeValue   = HttpContext.Current.Server.HtmlEncode(nodeValue);
					}
				}
			}
			FpMLTextBox txt = new FpMLTextBox(m_FullCtor,nodeValue,150,m_FldCapture.Name,pControlGUI,null,false,null,null);
			txt.SetMultilineAttribute(100,600);
            txt.ID = m_FullCtor.GetUniqueID();  
            plh.Controls.Add(txt);
			Controls.Add(plh);
		}
		#endregion Constructors
		#region Methods
		#region SaveClass
		public void SaveClass(PageBase page)
		{
			string node_uniqueID = Controls[0].Controls[0].UniqueID;
			string nodeValue     = null;
			if (null != page.Request.Form[node_uniqueID])
			{
				nodeValue = page.Request.Form[node_uniqueID].ToString();
			}
			object oItem = m_FldCapture.GetValue(m_Capture);
			if (null != oItem)
			{
				FieldInfo fldNodes = oItem.GetType().GetField("Any");
				if (null != fldNodes)
				{
					if (StrFunc.IsEmpty(nodeValue))
					{
                        fldNodes.SetValue(oItem,null);
					}
					else
					{
						nodeValue = "<Root>" + HttpContext.Current.Server.HtmlDecode(nodeValue) + "</Root>";
						XmlDocument document = new XmlDocument();
						document.LoadXml(nodeValue);
						XmlNodeList xmlNodeList = document.SelectNodes(@"/Root/*");
						XmlNode[] nodes         = new XmlNode[xmlNodeList.Count];
						for (int i=0;i<xmlNodeList.Count;i++)
							nodes[i] = xmlNodeList[i];
						fldNodes.SetValue(oItem,nodes);
					}
				}
			}
		}
		#endregion SaveClass
		#endregion Methods

	}
	#endregion Node
	#region ObjectArray
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class ObjectArray : ControlBase, INamingContainer 
	{
		#region Members
		private int         m_NbItem;
		private readonly ArrayDivGUI m_ArrayDivGUI       = null;
		private readonly Type m_ItemType;
		private readonly bool m_IsExtendibleLevel;
		#endregion Members
		#region Accessors
		#region OffsetButton
		private int OffsetButton
		{
			get {return (m_ArrayDivGUI.IsMaster && m_IsExtendibleLevel)?1:0;}
		}
		#endregion OffsetButton
		#region OffsetClonable
		private int OffsetClonable
		{
			get {return m_ArrayDivGUI.IsClonable?1:0;}
		}
		#endregion OffsetClonable
		#region OffsetCopyPaste
		private int OffsetCopyPaste
		{
			get {return m_ArrayDivGUI.IsCopyPaste?1:0;}
		}
		#endregion OffsetCopyPaste
		#region PosAddButton
		private int PosAddButton
		{
			get {return Controls.Count - (1 + OffsetButton + OffsetClonable + OffsetCopyPaste);}
		}
		#endregion PosAddButton
		#region PosSubButton
		private int PosSubButton
		{
			get {return this.Controls.Count - (2 + OffsetButton + OffsetClonable + OffsetCopyPaste);}
		}
		#endregion PosSubButton
		#region PosCloneButton
		private int PosCloneButton
		{
			get {return this.Controls.Count - (1 + OffsetButton + OffsetCopyPaste);}
		}
		#endregion PosCloneButton
		#region PosCopyPasteButton
		private int PosCopyPasteButton
		{
			get {return this.Controls.Count - (1 + OffsetButton);}
		}
		#endregion PosCopyPasteButton
		

		#region DisplaySubstractButton
		private bool DisplaySubstractButton
		{
			get {return (1 < m_NbItem) && (0 < (m_NbItem-m_ArrayDivGUI.MinItem));}
		}
		#endregion DisplaySubstractButton
		#region DisplayAddButton
		private bool DisplayAddButton
		{
			get {return (0 == m_ArrayDivGUI.MaxItem) || (m_NbItem < m_ArrayDivGUI.MaxItem);}
		}
		#endregion DisplayAddButton
		#region DisplayCloneButton
		private bool DisplayCloneButton
		{
			get {return 0 < m_NbItem;}
		}
		#endregion DisplayCloneButton
		#endregion Accessors
		#region Constructors
		public ObjectArray(object pCapture,FieldInfo pFldCapture,ControlGUI pControlGUI,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)
		{
			//m_LabelItem  = (null == pControlGUI?string.Empty:pControlGUI.Name);
			m_ItemType   = m_FldCapture.FieldType.GetElementType();

			int lastBalise = m_FullCtor.HistoryBalise.Count;
			#region OpenDivGUI/ArrayDivGUI
			if (m_FldCapture.IsDefined(typeof(ArrayDivGUI),true))
			{
				object[] attributes = m_FldCapture.GetCustomAttributes(typeof(ArrayDivGUI),true);
				m_ArrayDivGUI       = (ArrayDivGUI) attributes[0];
				if (m_ArrayDivGUI.IsMaster)
				{
                    OpenDivGUI openDiv = new OpenDivGUI
                    {
                        UniqueId = MethodsGUI.CreateUniqueID(m_FullCtor, m_Parent, m_FldParent, m_Capture, m_FldCapture, 0),
                        HelpURL = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture),
                        Color = m_ArrayDivGUI.Color
                    };
                    if ((null == pControlGUI) || MethodsGUI.IsNoneLevel(pControlGUI.Level) || (pControlGUI.Level==m_ArrayDivGUI.Level))
					{
						openDiv.Level     = m_ArrayDivGUI.Level;
						openDiv.Name      = m_ArrayDivGUI.Name;
						openDiv.IsVisible = m_ArrayDivGUI.IsMasterVisible;
						Controls.Add(MethodsGUI.MakeDiv(m_FullCtor,openDiv,false));
					}
					else
					{
						openDiv.Level     = pControlGUI.Level;
						openDiv.Name      = pControlGUI.Name;
						openDiv.IsVisible = pControlGUI.IsDisplay;
						Controls.Add(MethodsGUI.MakeDiv(m_FullCtor,openDiv,false));
					}
				}
			}
			else
			{
                m_ArrayDivGUI = new ArrayDivGUI
                {
                    Level = MethodsGUI.LevelEnum.None
                };
            }
			m_IsExtendibleLevel  = MethodsGUI.IsExtendibleLevel(m_ArrayDivGUI.Level);
			#endregion OpenDivGUI/ArrayDivGUI
			object obj = m_FldCapture.GetValue(m_Capture);
			
            #region Items Initialize
			if (null != obj)
				m_NbItem = ((System.Array)obj).Length;
			else if (0 < m_ArrayDivGUI.MinItem)
			{
				ArrayList aObjects = new ArrayList();
				for (int i=0;i<m_ArrayDivGUI.MinItem;i++)
					aObjects.Add(m_FldCapture.FieldType.GetElementType().InvokeMember(null, BindingFlags.CreateInstance,null,null,null));
				m_FldCapture.SetValue(m_Capture,aObjects.ToArray(m_FldCapture.FieldType.GetElementType()));
				m_NbItem = m_ArrayDivGUI.MinItem;
			}
			obj = m_FldCapture.GetValue(m_Capture);

			for (int i=0;i<m_NbItem;i++)
				CreateControlsItem(((System.Array)obj).GetValue(i),i + OffsetButton,i);
			#endregion Items Initialize
			
            #region Buttons Setting
			//if (m_ArrayDivGUI.IsCopyPasteItem)
			//	Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
			//
            string id = m_FullCtor.GetUniqueID();
            
            FpMLIncrButton btn;
            if (m_ArrayDivGUI.IsVariableArray)
            {
                // Substract
                btn = new FpMLIncrButton(id + "Substract",m_FldCapture.Name, Cst.OperatorType.substract, m_NbItem, false, DisplaySubstractButton);
                Controls.Add(btn);
                // Add
                btn = new FpMLIncrButton(id + "Add",m_FldCapture.Name, Cst.OperatorType.add, m_NbItem, false, DisplayAddButton);
                Controls.Add(btn);
            }
			// Clone
			if (m_ArrayDivGUI.IsClonable)
			{
				btn = new FpMLIncrButton(id + "Clone",m_FldCapture.Name,Cst.OperatorType.clone,m_NbItem,false,DisplayCloneButton);
				Controls.Add(btn);
			}
			// CopyPaste
            if (m_ArrayDivGUI.IsCopyPaste)
            {
                FpMLCopyPasteButton btnCopyPaste = new FpMLCopyPasteButton(id + "CopyPaste", m_ArrayDivGUI.Name, TypeCaptureName, CopyFieldName, false, true);
                Controls.Add(btnCopyPaste);
            }
			#endregion Buttons Setting

			#region CloseDivGUI
			// Fin Balise
			if (m_IsExtendibleLevel && m_ArrayDivGUI.IsMaster)
				this.Controls.Add(MethodsGUI.MakeDiv(new CloseDivGUI(m_ArrayDivGUI.Level)));
			#endregion CloseDivGUI

			m_FullCtor.DeleteHistoryBalise(lastBalise);
		}
		#endregion
		#region Events
		#region OnAddOrDeleteItem
		public void OnAddOrDeleteItem(object sender,CommandEventArgs e) 
		{
			ArrayList aObjects      = new ArrayList();
			Array obj               = (System.Array) m_FldCapture.GetValue(m_Capture);
			int nbItem              = (null != obj)? obj.Length:0;
			int itemsNumber         = nbItem;
			int itemRequestNumber   = 0;
			int itemRequestPosition = Math.Max(nbItem,1);
			bool isCloneRequest     = e.CommandName.Equals(Cst.OperatorType.clone.ToString());
			bool isAddRequest       = e.CommandName.Equals(Cst.OperatorType.add.ToString());
			bool isSubRequest       = e.CommandName.Equals(Cst.OperatorType.substract.ToString());
			bool isContinue         = false;
			//bool isInLastPosition   = false;

			#region ArrayList aObjects filling
			if (0 < m_NbItem)
			{
				for (int i=0;i<obj.Length;i++)
				{
					if (null != obj.GetValue(i))
						aObjects.Add(obj.GetValue(i));
				}
			}
			#endregion ArrayList aObjects filling

			#region Set btnCommand parameters
			FpMLIncrButton btn = null;
			if (isCloneRequest)
				btn = (FpMLIncrButton)Controls[PosCloneButton];
			else if (isAddRequest)
				btn = (FpMLIncrButton)Controls[PosAddButton];
			else if (isSubRequest)
				btn = (FpMLIncrButton)Controls[PosSubButton];

			if (null != btn)
			{
				try
				{
					itemRequestNumber   = btn.ItemNumber;
					itemRequestPosition = btn.ItemPosition;
					//isInLastPosition    = (itemsNumber < itemRequestPosition);
					if (isCloneRequest)
					{
						isContinue = (0 < itemRequestNumber) && (itemRequestNumber <= itemsNumber);
						isContinue = isContinue && (0 < itemRequestPosition) && (itemRequestPosition <= itemsNumber+1);
					}
					else if (isAddRequest)
					{
						isContinue = (0 < itemRequestNumber) && (0 < itemRequestPosition) && (itemRequestPosition <= itemsNumber+1);
					}
					else if (isSubRequest)
					{
						isContinue = (0 < itemRequestNumber) && (0 < itemRequestPosition) && (itemRequestPosition <= itemsNumber+1);
						isContinue = isContinue && ((itemRequestPosition - 1) <= (itemsNumber - itemRequestNumber));
					}
				}
				catch {isContinue = false;}
			}
			#endregion Set btnCommand parameters


			if (isContinue)
			{
				int realPosition = itemRequestPosition -1;
				if (isAddRequest || isCloneRequest)
				{
					#region Add or Clone object(s)
					object oObjectNews;
					if (isCloneRequest)
					{
						#region Clone
						EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(m_ItemType,aObjects[itemRequestNumber - 1]);
						serializeInfo.SetEfsMLTradeInfo(m_FullCtor.DocumentVersion);
						oObjectNews	= CacheSerializer.CloneDocument(serializeInfo);
						// Rename Id only if clone at end position
						ResetOrRenameIdClonedObject(oObjectNews,itemRequestNumber,itemRequestPosition,(aObjects.Count+1) == itemRequestPosition);
						// Insert clone into arrayList
						if (itemRequestPosition <= aObjects.Count)
							aObjects.Insert(itemRequestPosition-1,oObjectNews);
						else
							aObjects.Add(oObjectNews);
							
						// Save arrayList
						m_FldCapture.SetValue(m_Capture,aObjects.ToArray(m_ItemType));
						// Create GUI for clone
						obj = (System.Array) m_FldCapture.GetValue(m_Capture);
						CreateControlsItem(obj.GetValue(realPosition),realPosition + OffsetButton,m_NbItem++);
						#endregion Clone
					}
					else
					{
						#region Add
						// Insert new objects into arrayList
						itemsNumber = aObjects.Count;

						for (int i=0;i<itemRequestNumber;i++)
						{
							oObjectNews = m_ItemType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
							if (itemRequestPosition <= itemsNumber)
								aObjects.Insert(realPosition + i,oObjectNews);
							else
								aObjects.Add(oObjectNews);
						}
						// Save arrayList
						m_FldCapture.SetValue(m_Capture,aObjects.ToArray(m_ItemType));
						obj = (System.Array) m_FldCapture.GetValue(m_Capture);
						// Create GUI for new objects
						for (int i=0;i<itemRequestNumber;i++)
							CreateControlsItem(obj.GetValue(realPosition + i),realPosition + i + OffsetButton ,m_NbItem++);
						#endregion Add
					}
					#endregion Add or Clone object(s)
				}
				else if (isSubRequest)
				{
					#region Sub object(s)
					for (int i=itemRequestNumber-1;0<=i;i--)
					{
						MethodInfo methodToInvoke = m_ItemType.GetMethod("RemoveReference");
						if (null != methodToInvoke)
						{
							String [] argNames  = new String [] {"pFullCtor"};
							object[] argValues  = new object [] {m_FullCtor};
							m_ItemType.InvokeMember(methodToInvoke.Name,BindingFlags.InvokeMethod,null,obj.GetValue(realPosition + i),argValues,null,null,argNames);
						}
						this.Controls.RemoveAt(realPosition + i + OffsetButton);
						obj.SetValue(null,realPosition + i);
						m_NbItem--;
					}
					if (0 <= m_NbItem)
					{
						aObjects = new ArrayList();
						for (int i=0;i<obj.Length;i++)
						{
							if (null != obj.GetValue(i))
								aObjects.Add(obj.GetValue(i));
						}
						
						if (0 == m_NbItem)
						{
							m_FldCapture.SetValue(m_Capture,null);
							FieldInfo fldSpecified = m_Capture.GetType().GetField(m_FldCapture.Name + Cst.FpML_SerializeKeySpecified);
							if (null != fldSpecified)
								fldSpecified.SetValue(m_Capture,false);
						}
						else
							m_FldCapture.SetValue(m_Capture,aObjects.ToArray(m_ItemType));
					}
					#endregion Sub object(s)
				}
				((FpMLIncrButton)Controls[PosSubButton]).TextItem = aObjects.Count;
				((FpMLIncrButton)Controls[PosAddButton]).TextItem = aObjects.Count;
				if (m_ArrayDivGUI.IsClonable)
					((FpMLIncrButton)Controls[PosCloneButton]).TextItem = aObjects.Count;
			}
			Controls[PosSubButton].Visible = DisplaySubstractButton;
			Controls[PosAddButton].Visible = DisplayAddButton;
			if (m_ArrayDivGUI.IsClonable)
				Controls[PosCloneButton].Visible = DisplayCloneButton;
			if (m_ArrayDivGUI.IsCopyPaste)
				Controls[PosCopyPasteButton].Visible = true;
		}
		#endregion
		#region OnCopyPasteArray
		private void OnCopyPasteArray(object sender,CommandEventArgs e) 
		{
		}
		#endregion OnCopyPasteArray
		#endregion Events
		#region Methods
		#region CreateControlsItem
		private void CreateControlsItem(object pObj,int pPosition,int pIndex)
		{
			PlaceHolder plh = new PlaceHolder();

			int labelIndex = pIndex+1;
			string labelBalise;
			if (m_ArrayDivGUI.IsProduct)
				labelBalise = MethodsGUI.GetInstrumentName(pObj);
			else
				labelBalise = m_ArrayDivGUI.Name + " " + labelIndex;

			MethodsGUI.WriteBookMark(m_FldCapture,pIndex+1,m_FullCtor);
            OpenDivGUI openDiv = new OpenDivGUI
            {
                Name = labelBalise,
                UniqueId = MethodsGUI.CreateUniqueID(m_FullCtor, null, m_FldCapture, m_Capture, m_FldCapture, pIndex + 1)
            };
            if (m_ArrayDivGUI.IsChild)
			{
				openDiv.Level     = m_ArrayDivGUI.Level;
				openDiv.IsVisible = m_ArrayDivGUI.IsChildVisible;
			}
			else
			{
				openDiv.Level     = MethodsGUI.LevelEnum.HiddenKey;
				openDiv.IsVisible = false;
			}
			plh.Controls.Add(MethodsGUI.MakeDiv(m_FullCtor, openDiv, false));

			//
			m_FullCtor.Read(pObj,ref plh,m_Capture,m_FldCapture,m_Parent,m_FldParent);
			if (m_ArrayDivGUI.IsChild && m_IsExtendibleLevel)
				plh.Controls.Add(MethodsGUI.MakeDiv(m_FullCtor,new CloseDivGUI(m_ArrayDivGUI.Level,labelBalise)));
            //
			// CopyPaste
			if (m_ArrayDivGUI.IsCopyPasteItem)
			{
				FpMLCopyPasteButton btnCopyPaste = new FpMLCopyPasteButton(m_FullCtor.GetUniqueID(),labelBalise,pObj.GetType().Name,CopyFieldName,false,true,pIndex);
				plh.Controls.Add(btnCopyPaste);
			}
			Controls.AddAt(pPosition,plh);
		}
		#endregion
		#region DisplayFirstObjectArray
		public void DisplayFirstObjectArray()
		{
			for (int i=0;i<m_NbItem;i++)
			{
				this.Controls.RemoveAt(0);
			}
			ArrayList aObjects = new ArrayList();
			object obj         = m_ItemType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
			aObjects.Add(obj);
			m_FldCapture.SetValue(m_Capture,aObjects.ToArray(m_ItemType));
			m_NbItem = 1;
			obj      = m_FldCapture.GetValue(m_Capture);
			CreateControlsItem(((System.Array)obj).GetValue(0),0,0);
			this.Controls[PosSubButton].Visible = DisplaySubstractButton;
			((FpMLIncrButton)this.Controls[PosSubButton]).TextItem = m_NbItem;
			this.Controls[PosAddButton].Visible = DisplayAddButton;
			((FpMLIncrButton)this.Controls[PosAddButton]).TextItem = m_NbItem;
			if (m_ArrayDivGUI.IsClonable)
			{
				this.Controls[PosCloneButton].Visible = DisplayCloneButton;
				((FpMLIncrButton)this.Controls[PosCloneButton]).TextItem = m_NbItem;
			}
			if (m_ArrayDivGUI.IsCopyPaste)
				this.Controls[PosCopyPasteButton].Visible = true;
		}
		#endregion DisplayFirstObjectArray
		#region ResetOrRenameIdClonedObject
        // EG 20180423 Analyse du code Correction [CA2200]
        private void ResetOrRenameIdClonedObject(object pObj, int pItemNumberSource, int pItemNumberDest, bool pIsReplace)
		{
			try
			{
				string curValue         = string.Empty;
				string newValue         = string.Empty;
				string itemNumberSource = pItemNumberSource.ToString();
				string itemNumberDest   = pItemNumberDest.ToString();
				if (null != pObj)
				{
					Type tObj        = pObj.GetType();
					FieldInfo[] flds = tObj.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField);
					if (null != flds)
					{
						foreach (FieldInfo fld in flds)
						{

							object objChild = fld.GetValue(pObj);
							if ((null != objChild) && objChild.GetType().Equals(typeof(EFS_Id)))
							{
                                EFS_Id objId = (EFS_Id)objChild;
                                curValue = objId.Value;
								if (StrFunc.IsFilled(curValue) && pIsReplace)
								{
									newValue = curValue.Replace(itemNumberSource,itemNumberDest);
									if (newValue != curValue)
									{
                                        objId.Value = newValue;
										string[] m_FpMLKeyReference = MethodsGUI.GetReference(pObj,fld);
										if (null != m_FpMLKeyReference)
										{
											for (int i=0;i<m_FpMLKeyReference.Length;i++)
												m_FullCtor.LoadEnumObjectReference(m_FpMLKeyReference[i],null,newValue);
										}
									}
									else
                                        objId.Value = string.Empty;
								}
								else
                                    objId.Value = string.Empty;
							}
							else
								ResetOrRenameIdClonedObject(objChild,pItemNumberSource,pItemNumberDest,pIsReplace);
						}
					}
				}
			}
			catch (Exception) {throw;}
		}
		#endregion ResetOrRenameIdClonedObject
		#endregion Methods
	}
	#endregion ObjectArray
	#region OptionalItem
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class OptionalItem : ControlBase
    {
        #region Members
        private readonly MethodsGUI.LevelEnum m_Level;
        private bool m_IsSpecified;
        private readonly bool m_IsExtendibleLevel;
        #endregion

        #region Accessors
        public bool IsSpecified
        {
            get { return m_IsSpecified; }
            set { m_IsSpecified = value; }
        }
        #endregion Accessors

        #region Constructors
        public OptionalItem(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
            : base(pCapture, pFldCapture, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {
            #region Members Setting
            m_Level = pControlGUI.Level;
            m_IsSpecified = (null != m_FldCapture.GetValue(m_Capture));
            m_IsExtendibleLevel = MethodsGUI.IsExtendibleLevel(m_Level);
            string helpURL = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);
            #endregion

            #region CheckBox Control Setting
            FieldInfo fldSpecified = m_Capture.GetType().GetField(m_FldCapture.Name + Cst.FpML_SerializeKeySpecified);
            if (null != fldSpecified)
                m_IsSpecified = (bool)fldSpecified.GetValue(m_Capture);
            else if (m_FldParent.FieldType.Equals(m_Capture.GetType()))
            {
                fldSpecified = m_Parent.GetType().GetField(m_FldParent.Name + Cst.FpML_SerializeKeySpecified);
                if (null != fldSpecified)
                    m_IsSpecified = (bool)fldSpecified.GetValue(m_Parent);
            }
            #region OpenDivGUI
            if (m_IsExtendibleLevel)
                Controls.Add(MethodsGUI.MakeDiv(m_FullCtor, new OpenDivGUI(pControlGUI.Level, pControlGUI.Name, pControlGUI.IsDisplay, helpURL, 0, pControlGUI.Color), true));
            #endregion
            //
            FpMLCheckBox chk = new FpMLCheckBox(m_FullCtor, m_IsSpecified, TextAlign.Right, m_FldCapture.Name, pControlGUI)
            {
                ID = m_FullCtor.GetUniqueID()
            };
            Controls.Add(chk);
            #endregion ChechBox Control Setting
            //
            #region Item Control Setting
            #region OpenDivGUI
            if (m_IsExtendibleLevel)
            {
                OpenDivGUI openDiv = new OpenDivGUI(MethodsGUI.GetLevelKey(pControlGUI.Level),
                    pControlGUI.Name, pControlGUI.IsDisplay, helpURL)
                {
                    UniqueId = MethodsGUI.CreateUniqueID(m_FullCtor, m_Parent, m_FldParent, m_Capture, m_FldCapture, 0),
                    Color = pControlGUI.Color
                };
                Controls.Add(MethodsGUI.MakeDiv(m_FullCtor, openDiv, true));
            }
            #endregion OpenDivGUI
            #region Item Calling
            PlaceHolder plh = new PlaceHolder();
            ControlGUI controlGUI = MethodsGUI.GetControl(m_FldCapture, false);
            if (false == controlGUI.IsPrimary)
            {
                string helpUrl = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);
                plh.Controls.Add(new FpMLLabelOnly(controlGUI, helpUrl));
            }

            string labelControl = m_IsExtendibleLevel ? Cst.FpML_textAttribute : pControlGUI.Name;

            controlGUI = new ControlGUI
            {
                Name = labelControl,
                LblWidth = pControlGUI.LblWidth,
                LineFeed = pControlGUI.LineFeed,
                Width = pControlGUI.Width,
                Color = pControlGUI.Color
            };

            if (ReflectionTools.IsBaseType(m_FldCapture, typeof(StringGUI)))
            {
                plh.Controls.Add(new ComplexControls.SimpleString(m_Capture, m_FldCapture, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
            }
            else if (ReflectionTools.IsBaseType(m_FldCapture, typeof(BooleanGUI)))
            {
                controlGUI.Name = "checked if yes";
                plh.Controls.Add(new ComplexControls.SimpleBoolean(m_Capture, m_FldCapture, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
            }
            else if (ReflectionTools.IsBaseType(m_FldCapture, typeof(DateCalendarGUI)))
                plh.Controls.Add(new DateCalendar(m_Capture, m_FldCapture, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
            else if (m_FldCapture.FieldType.IsArray)
            {
                controlGUI.Name = pControlGUI.Name;
                plh.Controls.Add(new ObjectArray(m_Capture, m_FldCapture, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
            }
            else if (m_FldCapture.FieldType.IsEnum ||
                m_FldCapture.FieldType.Equals(typeof(System.String)) ||
                ReflectionTools.IsBaseType(m_FldCapture, typeof(SchemeGUI)))
            {
                controlGUI.Name = string.Empty;
                plh.Controls.Add(new Scheme(m_Capture, m_FldCapture, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
            }
            else if (ReflectionTools.IsBaseType(m_FldCapture, typeof(SchemeTextGUI)))
                plh.Controls.Add(new SchemeText(m_Capture, m_FldCapture, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
            else if (ReflectionTools.IsBaseType(m_FldCapture, typeof(HrefGUI)))
                plh.Controls.Add(new Href(m_Capture, m_FldCapture, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
            else if (ReflectionTools.IsBaseType(m_FldCapture, typeof(SchemeBoxGUI)))
                plh.Controls.Add(new SchemeBox(m_Capture, m_FldCapture, controlGUI, m_Parent, m_FldParent, m_GrandParent, m_FldGrandParent, m_FullCtor));
            else
            {
                object oItem;
                if (false == m_IsSpecified)
                {
                    oItem = m_FldCapture.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                    m_FldCapture.SetValue(m_Capture, oItem);
                }
                if (false == ReflectionTools.IsBaseType(m_FldCapture, typeof(StringGUI)))
                {
                    oItem = m_FldCapture.GetValue(m_Capture);
                    m_FullCtor.Read(oItem, ref plh, m_Capture, m_FldCapture, m_Parent, m_FldParent);
                    m_FullCtor.IsChildrenInclude = false;
                }
            }
            Controls.Add(plh);
            #endregion Item Calling
            #region CloseDivGUI
            if (m_IsExtendibleLevel)
                Controls.Add(MethodsGUI.MakeDiv(new CloseDivGUI(m_Level)));
            #endregion CloseDivGUI
            #region CopyPaste
            if ((null != pControlGUI) && (pControlGUI.IsCopyPaste))
            {
                FpMLCopyPasteButton btnCopyPaste = new FpMLCopyPasteButton(m_FullCtor.GetUniqueID(),pControlGUI.Name, TypeCaptureName, CopyFieldName, false, true);
                Controls.Add(btnCopyPaste);
            }
            #endregion
            #endregion Item Control Setting
            //            
            SetEventHandler();
            //
            DisplayChildCheck(false);
        }
        #endregion Constructors

        #region Events
        #region OnCheckedChanged
        private void OnCheckedChanged(object sender, EventArgs e)
        {
            DisplayChildCheck(true);
        }
        #endregion OnCheckedChanged
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            bool isConsult = MethodsGUI.IsModeConsult(this.Page);
            //
            if ((isConsult && m_IsSpecified) || false == isConsult)
                base.Render(writer);
        }
        #endregion Render
        #endregion Events

        #region Methods
        #region public SaveClass
        public void SaveClass()
        {
            FieldInfo fldSpecified = m_Capture.GetType().GetField(m_FldCapture.Name + Cst.FpML_SerializeKeySpecified);
            if (null != fldSpecified)
                fldSpecified.SetValue(m_Capture, m_IsSpecified);
            else if (false == m_IsSpecified)
                m_FldCapture.SetValue(m_Capture, null);
        }
		#endregion SaveClass
		#region public SetEventHandler
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		public void SetEventHandler()
        {
            int i = (m_IsExtendibleLevel ? 1 : 0);
			if (i < Controls.Count)
			{
                if (Controls[i] is CheckBox ctrl)
                    ctrl.CheckedChanged += new System.EventHandler(OnCheckedChanged);
            }
		}
        #endregion
        #region private DisplayChildCheck
        private void DisplayChildCheck(bool pIsUserAction)
        {
            int i = (m_IsExtendibleLevel ? 1 : 0);
            int j = (m_IsExtendibleLevel ? 3 : 1);
            //
            if (pIsUserAction && MethodsGUI.IsModeConsult(Page))
                ((CheckBox)Controls[i]).Checked = !((CheckBox)Controls[i]).Checked;
            else
            {
                if (m_Capture.GetType().Name.Equals("InformationSource"))
                {
                    if (m_FldCapture.Name.Equals("rateSourcePage"))
                        FxRateControl.AssetFxRate_RateSourcePage(this);
                    else if (m_FldCapture.Name.Equals("rateSourcePageHeading"))
                        FxRateControl.AssetFxRate_RateSourcePageHeading(this);
                }
                else
                {
                    m_IsSpecified = (this.Controls[i] as CheckBox).Checked;
                    if (m_IsSpecified && pIsUserAction)
                        (this.Controls[i] as CheckBox).Attributes.Add("data-redraw", "1");
                    else
                        (this.Controls[i] as CheckBox).Attributes.Remove("data-redraw");
                    Controls[j].Visible = m_IsSpecified;
                    if (m_FldCapture.FieldType.IsArray && m_IsSpecified && pIsUserAction)
                        ((ObjectArray)Controls[j].Controls[0]).DisplayFirstObjectArray();
                }
            }
        }
        #endregion DisplayChildCheck
        #endregion
    }
	#endregion OptionalItem
	#region SchemeBase
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SchemeBase : ControlBase
	{
		#region Variables
		protected string      m_EnumKey     = string.Empty;
		protected int         m_CtrlWidth   = 150;
		protected string      m_ValueScheme;
		protected string      m_HelpUrl;
        #endregion Variables

		#region Constructors
        public SchemeBase(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
            : base(pCapture, pFldCapture, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {
            m_ControlGUI = pControlGUI;
            m_FldParent = (pFldParent ?? pFldCapture);
            //
            if ((null != pControlGUI) && (0 != pControlGUI.Width))
                m_CtrlWidth = pControlGUI.Width;
            //
            if (null == m_FldCapture.GetValue(m_Capture))
            {
                //FI/EG 20091120 un string ne peut être instancié
                if (false == m_FldCapture.FieldType.Equals(typeof(System.String)))
                {
                    object o = m_FldCapture.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                    m_FldCapture.SetValue(m_Capture, o);
                }
            }
            //
            Object obj = m_FldCapture.GetValue(m_Capture);
            if (null != obj)
            {
                if (obj.GetType().IsEnum)
                {
                    // FI 20100621 [17009], 
                    //Pour les enums fpML, la combo est chargé avec les valeurs des enum c# et non les valeurs FpML associées
                    //Exemple sur la RollConventionEnum 20, m_ValueScheme doit être alimenté avec c# "DAY20" plutôt que la valeur FpML "20"
                    //Pour les enums FixML, la combo est chargé avec les valeurs des enum de fixML
                    string @namespace = string.Empty;
                    object[] attributes = obj.GetType().GetCustomAttributes(typeof(XmlTypeAttribute), true);
                    if (ArrFunc.IsFilled(attributes))
                    {
                        XmlTypeAttribute xmlAttribute = (XmlTypeAttribute)attributes[0];
                        if (StrFunc.IsFilled(xmlAttribute.Namespace))
                            @namespace = xmlAttribute.Namespace;
                    }
                    if (StrFunc.IsEmpty(@namespace))
                    {
                        @namespace = obj.GetType().Namespace;
                    }
                    //
                    bool isEnumCSharp = (false == (@namespace.ToUpper().Contains("FIXML")));
					//
					if (isEnumCSharp)
						m_ValueScheme = obj.ToString();
					else
						m_ValueScheme = ReflectionTools.ConvertEnumToString(obj as System.Enum);
				}
                else if ((Cst.FpML_OTCmlTextAttribute == m_FldCapture.Name))
                {
                    m_ValueScheme = obj.ToString();
                }
                else
                {
                    FieldInfo fieldInfo = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                    if (null != fieldInfo)
                    {
                        if (fieldInfo.FieldType.IsEnum)
                            m_ValueScheme = fieldInfo.GetValue(obj).ToString();
                        else
                            m_ValueScheme = (string)fieldInfo.GetValue(obj);
                    }
                    else
                        m_ValueScheme = obj.ToString();
                }
            }
        }
		#endregion Constructors
		
		#region Methods
		#region GetGUIValue
		public string GetGUIValue()
		{
			string value_uniqueID = Controls[0].UniqueID;
			string val            = string.Empty;
			if (null != Page.Request.Form[value_uniqueID])
				  val = Page.Request.Form[value_uniqueID];
			return val;
		}
		#endregion GetGUIValue
		#region GetValue
		protected string GetValue()
		{
			Object obj = m_FldCapture.GetValue(m_Capture);
			string val;
			if (Cst.FpML_OTCmlTextAttribute == m_FldCapture.Name)
				val = obj.ToString();
			else
			{
				FieldInfo fieldInfo = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
				if (null != fieldInfo)
					val = (string) fieldInfo.GetValue(obj);
				else
					val = obj.ToString();
			}
			return val;
		}
		#endregion GetValue
		#region SaveClass
		public void SaveClass(PageBase page)
		{
			string value_uniqueID = Controls[0].UniqueID;
			string val            = string.Empty;
			if (null != page.Request.Form[value_uniqueID])
				val = page.Request.Form[value_uniqueID];

			if (StrFunc.IsFilled(val))
				SetValue(val);
		}
		#endregion SaveClass
		#region SetValue
		protected void SetValue(string pValue)
		{
			object obj;
			if (null != m_FldCapture.GetValue(m_Capture))
			{
				if (Cst.FpML_OTCmlTextAttribute == m_FldCapture.Name)
					if (m_FldCapture.FieldType.IsEnum)
					{
						obj = m_FldCapture.GetValue(m_Capture);
						obj = ReflectionTools.EnumParse(obj,pValue);
						m_FldCapture.SetValue(m_Capture,obj);
					}
					else
						m_FldCapture.SetValue(m_Capture,pValue);
				else
				{
					obj = m_FldCapture.GetValue(m_Capture);
					if (obj.GetType().IsEnum)
					{
						obj = ReflectionTools.EnumParse(obj,pValue);
						m_FldCapture.SetValue(m_Capture,obj);
					}
					else
					{
						FieldInfo fieldInfo = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
						if (null != fieldInfo)
						{
							if (fieldInfo.FieldType.IsEnum)
								fieldInfo.SetValue(obj,ReflectionTools.EnumParse(obj,pValue));
							else
								fieldInfo.SetValue(obj,pValue);
						}
						else
							m_FldCapture.SetValue(m_Capture,pValue);
					}
				}
			}
		}
		#endregion SetValue
		#endregion Methods
	}
	#endregion SchemeBase
	#region Scheme
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class Scheme : SchemeBase
    {
        #region Constructors
        public Scheme(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
            : base(pCapture, pFldCapture, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {
            m_EnumKey = GetSchemeList(m_FldCapture);
            ControlGUI controlGUI = MethodsGUI.GetControl(m_FldParent, false);
            string helpUrl = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);
            //
            if (false == controlGUI.IsPrimary && (m_EnumKey.ToUpper() == controlGUI.Name.ToUpper()))
                Controls.Add(new FpMLLabelOnly(controlGUI, helpUrl));
            //
            FpMLDropDownList ddl;
            if (null != pControlGUI)
                ddl = new FpMLDropDownList(m_FullCtor, m_ValueScheme + String.Empty, m_CtrlWidth, m_EnumKey, pControlGUI, helpUrl);
            else
                ddl = new FpMLDropDownList(m_FullCtor, m_ValueScheme + String.Empty, m_CtrlWidth, m_EnumKey, string.Empty, helpUrl);
            //
            ddl.ID = m_FullCtor.GetUniqueID();
            ddl.AutoPostBack = true;
            Controls.Add(ddl);
            //
            SetEventHandler();
        }
        #endregion Constructors
        #region Events
        #region OnPeriodChanged
        private void OnPeriodChanged(object sender, EventArgs e)
        {
            RateIndexControl.RateIndex_Id(this);
        }
        #endregion OnPeriodChanged
        #region OnInformationProviderChanged
        private void OnInformationProviderChanged(object sender, EventArgs e)
        {
            string oldValue = ((FpMLDropDownList)this.Controls[0]).Attributes["oldvalue"];
            string newValue = ((FpMLDropDownList)this.Controls[0]).SelectedValue;
            if (oldValue != newValue)
            {
                FxRateControl.AssetFxRate_InformationProvider(this);
                ((FpMLDropDownList)this.Controls[0]).Attributes["oldvalue"] = newValue;
            }
        }
        #endregion OnInformationProviderChanged
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            if (null != m_Parent)
            {
                if (m_Parent.GetType().Name.Equals("FloatingRateCalculation") && m_Capture.GetType().Name.Equals("Interval"))
                    RateIndexControl.RateIndex_Render(this.NamingContainer);
                else if (m_Capture.GetType().Name.Equals("InformationSource") && m_FldCapture.FieldType.Name.Equals("InformationProvider"))
                    FxRateControl.AssetFxRate_InformationProvider_Render(this);
            }
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
        #region Methods
        #region GetSchemeList
        public static string GetSchemeList(FieldInfo pFldCapture)
        {
            string schemeName;
            if (pFldCapture.FieldType.IsEnum)
                schemeName = pFldCapture.FieldType.Name;
            else if (Cst.FpML_OTCmlTextAttribute == pFldCapture.Name)
                schemeName = pFldCapture.DeclaringType.Name;
            else if (pFldCapture.FieldType.Equals(typeof(System.String)))
                schemeName = pFldCapture.Name;
            else if (pFldCapture.FieldType.IsClass)
            {
                if ("EFS.GUI.Interface" == pFldCapture.FieldType.Namespace)
                    schemeName = pFldCapture.Name.Substring(0, 1).ToUpper() + pFldCapture.Name.Substring(1);
                else
                    schemeName = pFldCapture.FieldType.Name;
            }
            else
                schemeName = pFldCapture.Name;
            return schemeName;
        }
        #endregion GetSchemeList
        #region public override SetEventHandler
        public void SetEventHandler()
        {
            int i = 0;
            if (null != m_ControlGUI && (false == m_ControlGUI.IsPrimary && (m_EnumKey.ToUpper() == m_ControlGUI.Name.ToUpper())))
                i = 1;
            //
            DropDownList ddl = (DropDownList)Controls[i];
            if ((null != m_Parent) && m_Parent.GetType().Name.Equals("FloatingRateCalculation") && m_Capture.GetType().Name.Equals("Interval"))
                ddl.SelectedIndexChanged += new System.EventHandler(OnPeriodChanged);
            else if (m_Capture.GetType().Name.Equals("InformationSource") && m_FldCapture.FieldType.Name.Equals("InformationProvider"))
                ddl.SelectedIndexChanged += new System.EventHandler(OnInformationProviderChanged);
        }
        #endregion
        #endregion Methods
    }
	#endregion Scheme
	#region SchemeId
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SchemeId : Scheme
	{
		#region Constructors
		public SchemeId(object pCapture,FieldInfo pFldCapture,ControlGUI pControlGUI,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor) 
		{
			Object obj = pFldCapture.GetValue(pCapture);
			FieldInfo fld = obj.GetType().GetField("efs_id");
            Controls.Add(new SimpleString(obj,fld,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Constructors
	}
	#endregion SchemeId
	#region SchemeBox
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SchemeBox : SchemeBase
	{
		#region Constructors
		public SchemeBox(object pCapture,FieldInfo pFldCapture,ControlGUI pControlGUI,object pParent,FieldInfo pFldParent,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor) 
		{
			Validator validator     = new Validator(m_FldCapture.ReflectedType.Name + 
				(StrFunc.IsEmpty(pControlGUI.Name)?string.Empty:"(" + pControlGUI.Name + ")"),true);
			m_HelpUrl = MethodsGUI.GetHelpLink(m_Capture,m_FldCapture);
			
            FpMLSchemeBox schemebox;
			if (MethodsGUI.IsValidatorOptionalControl(m_FldCapture))
				schemebox = new FpMLSchemeBox(m_ValueScheme,m_CtrlWidth,m_FldCapture.Name,pControlGUI,true,m_HelpUrl);
			else
				schemebox = new FpMLSchemeBox(m_ValueScheme,m_CtrlWidth,m_FldCapture.Name,pControlGUI,true,m_HelpUrl,validator);

            schemebox.ID = m_FullCtor.GetUniqueID();  
			Controls.Add(schemebox);
            //
            SetEventHandler(); 
		}
		#endregion Constructors
		#region Events
		#region OnChanged
		private void OnChanged(object sender,EventArgs e) 
		{
			Type fieldType = m_FldCapture.FieldType;
			if (fieldType.Name.Equals("FloatingRateIndex"))
				RateIndexControl.RateIndex_Id(this);
			else if (fieldType.Name.Equals("BookId"))
				BookControl.Book_Id(this);
			else if (fieldType.Name.Equals("AssetFxRateId"))
				FxRateControl.AssetFxRate_Id(this);
		}
		#endregion OnChanged
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
			((TextBox) this.Controls[0]).Text = GetValue();
			if (m_FldCapture.FieldType.Name.Equals("FloatingRateIndex"))
				RateIndexControl.RateIndex_Render(this);
			else if (m_FldCapture.FieldType.Name.Equals("AssetFxRateId"))
				FxRateControl.AssetFxRate_Render(this);
			base.Render(writer);
		}
		#endregion Render
		#endregion Events
        #region Methods
        public void SetEventHandler()
        {
            ((TextBox)Controls[0]).TextChanged += new System.EventHandler(OnChanged);
        }
        #endregion Methods
    }
	#endregion SchemeBox
	#region SchemeText
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SchemeText : SchemeBase
	{
		#region Constructors
        public SchemeText(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
            : base(pCapture, pFldCapture, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {
            ControlGUI controlGUI = MethodsGUI.GetControl(m_FldParent, false);
            m_HelpUrl = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);
            if (false == controlGUI.IsPrimary && (m_EnumKey.ToUpper() == controlGUI.Name.ToUpper()))
                this.Controls.Add(new FpMLLabelOnly(controlGUI, m_HelpUrl));

            FpMLTextBox txt;
            if (MethodsGUI.IsMandatoryControl(m_FldCapture))
            {
                string name = MethodsGUI.GetValidationSummaryMessage(m_FldCapture, m_FldParent);
                txt = new FpMLTextBox(m_FullCtor, m_ValueScheme, m_CtrlWidth, m_FldCapture.Name, pControlGUI, null, false, null, m_HelpUrl, new Validator(name, true));
            }
            else
                txt = new FpMLTextBox(m_FullCtor, m_ValueScheme, m_CtrlWidth, m_FldCapture.Name, pControlGUI, null, false, null, m_HelpUrl);

            txt.ID = m_FullCtor.GetUniqueID();  

            if (null != m_Parent)
            {
                if (m_Capture.GetType().Name.Equals("InformationSource") && m_FldCapture.FieldType.Name.Equals("RateSourcePage"))
                {
                    #region RateSourcePage Case
                    txt.Attributes.Add("oldvalue", string.Empty);
                    txt.AutoPostBack = true;
                    #endregion RateSourcePage Case
                }
            }
            Controls.Add(txt);
            SetEventHandler();
        }
		#endregion Constructors
		#region Events
		#region OnRateSourcePageChanged
		private void OnRateSourcePageChanged(object sender,EventArgs e) 
		{
            string oldValue = ((TextBox)Controls[0]).Attributes["oldvalue"];
			string newValue    = ((TextBox) Controls[0]).Text;
			if (oldValue != newValue)
			{
				FxRateControl.AssetFxRate_RateSourcePage(this);
                ((TextBox)this.Controls[0]).Attributes["oldvalue"] = newValue;
			}
		}
		#endregion OnRateSourcePageChanged
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
			if (m_Capture.GetType().Name.Equals("InformationSource") && m_FldCapture.FieldType.Name.Equals("RateSourcePage"))
				FxRateControl.AssetFxRate_RateSourcePage_Render(this);
			base.Render(writer);
		}
		#endregion Render
        #endregion Events

        #region Methods
        public void SetEventHandler()
        {
            if (null != m_Parent)
            {
                if (m_Capture.GetType().Name.Equals("InformationSource") && m_FldCapture.FieldType.Name.Equals("RateSourcePage"))
                {
                    ((TextBox)Controls[0]).TextChanged += new System.EventHandler(OnRateSourcePageChanged);
                }
            }
        }
        #endregion

    }
	#endregion SchemeText
	#region SimpleBoolean
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SimpleBoolean : ControlBase
    {
        #region Constructors
        public SimpleBoolean(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor )
            : base( pCapture, pFldCapture, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {
            if (null == m_FldCapture.GetValue(m_Capture))
            {
                object o = m_FldCapture.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                m_FldCapture.SetValue(m_Capture, o);
            }
            object obj = m_FldCapture.GetValue(m_Capture);
            bool isChecked = ((EFS_Boolean)obj).Value == Cst.FpML_Boolean_True;
            //    
            FpMLCheckBox chkBox = new FpMLCheckBox(m_FullCtor, isChecked, TextAlign.Right, m_FldCapture.Name, pControlGUI)
            {
                ID = m_FullCtor.GetUniqueID()
            };
            Controls.Add(chkBox);
            SetEventHandler();
        }
        #endregion Constructors

        #region Event
        protected override void Render(HtmlTextWriter writer)
        {
            object obj = m_FldCapture.GetValue(m_Capture);
            if (null != obj)
            {
                FieldInfo fieldInfo = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                ((CheckBox)Controls[0]).Checked = BoolFunc.IsTrue(fieldInfo.GetValue(obj));
            }
            //
            base.Render(writer);
        }
        #region OnCheckedChanged
        private void OnCheckedChanged(object sender, EventArgs e)
        {
            SaveClass();
        }
        #endregion OnCheckedChanged

        #endregion


        #region Methods
        #region SaveClass
        public void SaveClass()
        {
            bool isSpecified = ((CheckBox)this.Controls[0]).Checked;
            object obj = m_FldCapture.GetValue(m_Capture);
            if (null != obj)
            {
                FieldInfo fieldInfo = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                fieldInfo.SetValue(obj, isSpecified ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False);
            }
        }
        #endregion SaveClass
        #region SetEventHandler
        public void SetEventHandler()
        {
            ((CheckBox)Controls[0]).CheckedChanged += new System.EventHandler(OnCheckedChanged);
        }
        #endregion SetEventHandler

        #endregion Methods
    }
	#endregion SimpleBoolean
	#region SimpleString
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SimpleString : ControlBase
	{
		#region Members
		private readonly string[]  m_FpMLKeyReference;
		#endregion
		#region Constructors
        // EG 20180423 Analyse du code Correction [CA2200]
        public SimpleString(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
			: base(pCapture,pFldCapture,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor) 
		{
			try
			{
				if (null == m_FldCapture.GetValue(m_Capture))
				{
					object o = m_FldCapture.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
					m_FldCapture.SetValue(m_Capture,o);
				}
				//
				object obj             = m_FldCapture.GetValue(m_Capture);
				Type tObj              = obj.GetType();
				bool isValueFldCapture = (Cst.FpML_OTCmlTextAttribute == m_FldCapture.Name);
				//
				//20071002 EG/FI Ticket 15800 => set m_ControlGUI 
				m_ControlGUI = pControlGUI;
				//
				string val             = string.Empty;
				if (isValueFldCapture)
				{
					val                 = (string)obj;
				}
				else
				{
					FieldInfo fieldInfo = tObj.GetField(Cst.FpML_OTCmlTextAttribute);
					val                 = (string)fieldInfo.GetValue(obj);
					val                 = GetValueByType(val,false).ToString();
				}
				//
				int ctrlWidth       = 150;
				if ((null != pControlGUI) && (0 != pControlGUI.Width))
					ctrlWidth       = pControlGUI.Width;
				//	
				if (tObj.Equals(typeof(EFS_Id)))
				{
					m_FpMLKeyReference  = MethodsGUI.GetReference(m_Capture,m_FldCapture);
					string pIdName      = pControlGUI.Name;
					if (StrFunc.IsEmpty(pIdName))
					{
						object[] attributes = null;
						if (null != m_FldParent)
							attributes = m_FldParent.GetCustomAttributes(typeof(XmlElementAttribute),true);
						else
							attributes = m_FldCapture.GetCustomAttributes(typeof(XmlElementAttribute),true);

						if (1 == attributes.GetLength(0))
							pIdName = ((XmlElementAttribute) attributes[0]).ElementName;
						else if (null != m_FldParent)
							pIdName = m_FldParent.FieldType.Name;
						else
							pIdName = m_FldCapture.DeclaringType.Name;

					}
                    FpMLTextBoxForId txt = new FpMLTextBoxForId(val, pIdName, pControlGUI.IsLocked)
                    {
                        ID = m_FullCtor.GetUniqueID()
                    };
                    Controls.Add(txt);
					OnIdChanged(txt,null);
				}
				else
				{
					//
					Validator validatorMandatory = null;
					Validator validator          = null;
					//
					if (MethodsGUI.IsMandatoryControl(m_FldCapture))
					{
						string name = MethodsGUI.GetValidationSummaryMessage(m_FldCapture,m_FldParent);
						validatorMandatory = new Validator(name,true);
					}
					//
					//20071002 FI Ticpublic EFS_Decimal stubTypeFixedRate;ket 15800 Sans Regex specifié Recherche du regex par défaut 
					if (tObj.Equals(typeof(EFS_Decimal))) 
					{
						if ((EFSRegex.TypeRegex.None == pControlGUI.Regex))
						{
							GetRegexDecimal(); 
							if (m_ControlGUI.Regex != EFSRegex.TypeRegex.RegexFixedRateExtend)
								validator = new Validator(m_ControlGUI.Regex ,m_FldCapture.Name,true); //Comme pour la saisie ligth => pas de Validator
						}
					}
					else if (tObj.Equals(typeof(EFS_Time)) || tObj.Name.Equals("HourMinuteTime"))
						validator = new Validator(EFSRegex.TypeRegex.RegexLongTime,m_FldCapture.Name,true);
					else if (tObj.Equals(typeof(EFS_NonNegativeInteger)))
						validator = new Validator(EFSRegex.TypeRegex.RegexNonNegativeInteger,m_FldCapture.Name,true);
					else if (tObj.Equals(typeof(EFS_NegativeInteger)))
						validator = new Validator(EFSRegex.TypeRegex.RegexNegativeInteger,m_FldCapture.Name,true);
					else if (tObj.Equals(typeof(EFS_PosInteger)))
						validator = new Validator(EFSRegex.TypeRegex.RegexPositiveInteger,m_FldCapture.Name,true);
					else if (tObj.Equals(typeof(EFS_Integer)))
						validator = new Validator(EFSRegex.TypeRegex.RegexInteger,m_FldCapture.Name,true);
					else if (tObj.Equals(typeof(EFS_MonthYear)))
						validator = new Validator(EFSRegex.TypeRegex.RegexMonthYear,m_FldCapture.Name,true);
					//
					string helpUrl = MethodsGUI.GetHelpLink(m_Capture,m_FldCapture);
					FpMLTextBox txt;
					//
					if (null == validator)
					{
						if (null == validatorMandatory)
							txt = new FpMLTextBox(m_FullCtor,val,ctrlWidth,m_FldCapture.Name,pControlGUI,null,false,null,helpUrl);
						else
							txt = new FpMLTextBox(m_FullCtor,val,ctrlWidth,m_FldCapture.Name,pControlGUI,null,false,null,helpUrl,validatorMandatory);
					}
					else if (null == validatorMandatory)
						txt = new FpMLTextBox(m_FullCtor,val,ctrlWidth,m_FldCapture.Name,pControlGUI,null,false,null,helpUrl,validator);
					else
						txt = new FpMLTextBox(m_FullCtor,val,ctrlWidth,m_FldCapture.Name,pControlGUI,null,false,null,helpUrl,validatorMandatory,validator);
					//
					if (tObj.Equals(typeof(EFS_MultiLineString)))
						txt.SetMultilineAttribute();
					//
                    txt.ID = m_FullCtor.GetUniqueID();   
                    //
                    if (null != m_Parent)
					{
						if ((m_Parent.GetType().Name.Equals("FloatingRateCalculation") || m_Parent.GetType().Name.Equals("FloatingRate")) && 
							m_Capture.GetType().Name.Equals("Interval"))
						{
							txt.AutoPostBack = true;   
						}
						else if (m_Capture.GetType().Name.Equals("InformationSource") && m_FldCapture.Name.Equals("rateSourcePageHeading"))
						{
                            txt.Attributes.Add("oldvalue", string.Empty);
							txt.AutoPostBack = true;
						}
					}
					//
					Controls.Add(txt);
				}
			}
			catch(Exception) 
			{
				throw;
			}
		}
		#endregion Constructors
		#region Events
		#region OnIdChanged
		private void OnIdChanged(object sender,EventArgs e) 
		{
            string oldValue = ((TextBox)this.Controls[0]).Attributes["oldvalue"];
			string newValue = ((TextBox) this.Controls[0]).Text;
			for (int i=0;i<m_FpMLKeyReference.Length;i++)
				m_FullCtor.LoadEnumObjectReference(m_FpMLKeyReference[i],oldValue,newValue);
            ((TextBox)this.Controls[0]).Attributes["oldvalue"] = newValue;
		}
		#endregion OnIdChanged
		#region OnIntervalChanged
		private void OnIntervalChanged(object sender,EventArgs e) 
		{
			RateIndexControl.RateIndex_Id(this);
		}
		#endregion OnIntervalChanged
		#region OnRateSourcePageHeadingChanged
		private void OnRateSourcePageHeadingChanged(object sender,EventArgs e) 
		{
            string oldValue = ((TextBox)this.Controls[0]).Attributes["oldvalue"];
			string newValue    = ((TextBox) this.Controls[0]).Text;
			if (oldValue != newValue)
			{
				FxRateControl.AssetFxRate_RateSourcePageHeading(this);
                ((TextBox)this.Controls[0]).Attributes["oldvalue"] = newValue;
			}
		}
		#endregion OnRateSourcePageHeadingChanged
        #region Render
        // EG 20180423 Analyse du code Correction [CA2200]
        protected override void Render(HtmlTextWriter writer)
        {
            try
            {
                if (null != m_Parent &&
                    (m_Parent.GetType().Name.Equals("FloatingRateCalculation") || m_Parent.GetType().Name.Equals("FloatingRate")) &&
                    m_Capture.GetType().Name.Equals("Interval"))
                {
                    RateIndexControl.RateIndex_Render(NamingContainer);
                }
                else if (m_Capture.GetType().Name.Equals("InformationSource") && m_FldCapture.Name.Equals("rateSourcePageHeading"))
                {
                    FxRateControl.AssetFxRate_RateSourcePageHeading_Render(this);
                }
                //
                //20071002 EG/FI Ticket 15800 Formatage de la donnée au Format station Si decimal
                string val = string.Empty;
                string value_uniqueID = Controls[0].UniqueID;
                object obj = m_FldCapture.GetValue(m_Capture);
                //
                if (Cst.FpML_OTCmlTextAttribute == m_FldCapture.Name)
                    val = (string)obj;
                else
                {
                    FieldInfo fieldInfo = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                    val = (string)fieldInfo.GetValue(obj);
                    if (EFSRegex.TypeRegex.None != m_ControlGUI.Regex)
                        val = GetValueByType(val, false).ToString();
                }
                //
                Control ctrl = Page.FindControl(value_uniqueID);
                if (null != ctrl)
                    ((TextBox)ctrl).Text = val;
                //
                base.Render(writer);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        #endregion Events
		
        #region Methods
		#region SaveClass
        // EG 20180423 Analyse du code Correction [CA2200]
        public void SaveClass(PageBase page)
		{
			try
			{
				string value_uniqueID = this.Controls[0].UniqueID;
				string val            = string.Empty;
				//
				if (null != page.Request.Form[value_uniqueID])
				{
					val                   = GetValueByType(page.Request.Form[value_uniqueID],true).ToString();
					object obj            = m_FldCapture.GetValue(m_Capture);
					if (null != obj)
					{
						if (Cst.FpML_OTCmlTextAttribute == m_FldCapture.Name)
							m_FldCapture.SetValue(m_Capture,StrFunc.IsFilled(val)?val:null);
						else
						{
							FieldInfo fieldInfo = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
							if (null != fieldInfo)
								fieldInfo.SetValue(obj,StrFunc.IsFilled(val)?val:null);
						}
					}
				}
				//
				FieldInfo fieldInfoSpecified = m_Capture.GetType().GetField(m_FldCapture.Name + Cst.FpML_SerializeKeySpecified);
				if (null != fieldInfoSpecified)
					fieldInfoSpecified.SetValue(m_Capture,StrFunc.IsFilled(val));
			}	
			catch(Exception) 
			{
				throw; 
			}
		}
		#endregion
		#region GetValueByType
		private object GetValueByType(string pValue,bool pIsSave)
		{
			object val            = string.Empty;
			try
			{
				if (Cst.FpML_OTCmlTextAttribute == m_FldCapture.Name)
					val = pValue;
				else
				{
					if (StrFunc.IsFilled(pValue))
					{
						//20071002 EG/FI Ticket 15800
						object obj = m_FldCapture.GetValue(m_Capture);
						Type tObj = obj.GetType();
						if (tObj.Equals(typeof(EFS_Decimal)))
						{
                            CustomCaptureInfo cci = new CustomCaptureInfo
                            {
                                Regex = m_ControlGUI.Regex,
                                DataType = TypeData.TypeDataEnum.@decimal
                            };
                            if (pIsSave)
							{
								cci.NewValueFromLiteral = pValue; 	
								val = cci.NewValue; 
							}
							else
							{
								cci.NewValue = ((EFS_Decimal) obj).Value; 
								val = cci.NewValueFmtToCurrentCulture;  
							}
						}
						else if (tObj.Equals(typeof(EFS_Time)) || tObj.Name.Equals("HourMinuteTime"))
						{
                            CustomCaptureInfo cci = new CustomCaptureInfo
                            {
                                Regex = m_ControlGUI.Regex,
                                DataType = TypeData.TypeDataEnum.time
                            };
                            if (pIsSave)
							{
								cci.NewValueFromLiteral = pValue; 	
								val                     = cci.NewValue; 
							}
							else
							{
								cci.NewValue = pValue;
								val          = cci.NewValueFmtToCurrentCulture;  
							}
						}
						else if (tObj.BaseType.Equals(typeof(EFS_NumberBase)) && (false == tObj.Equals(typeof(EFS_Decimal))))
							val = Int32.Parse(pValue);
						else if (tObj.BaseType.Equals(typeof(StringGUI)))
							val = pValue;
					}
				}
			}
			catch (Exception) {val = string.Empty;}
			return val;
		}
		#endregion GetValueByType
		#region GetRegexDecimal
        // 20090721 EG Modification des Regex : RegexAmountExtend -> RegexAmountSignedExtend
		private void GetRegexDecimal()
		{
			string captureName     = m_Capture.GetType().Name;
			string parentName      = string.Empty;
			string grandParentName = string.Empty;
			if (null != m_Parent)
				parentName = m_Parent.GetType().Name;
			if (null != m_GrandParent)
				grandParentName = m_GrandParent.GetType().Name;


			if (("Schedule" == captureName) && ("initialValue" == m_FldCapture.Name))
			{
				if (("FloatingRateCalculation" == parentName) && ("floatingRateMultiplierSchedule" == m_FldParent.Name))
                    m_ControlGUI.Regex = EFSRegex.TypeRegex.RegexAmountSignedExtend;
				else
					m_ControlGUI.Regex = EFSRegex.TypeRegex.RegexFixedRateExtend;
			}
			else if (("StrikeSchedule" == captureName) && ("initialValue" == m_FldCapture.Name))
				m_ControlGUI.Regex = EFSRegex.TypeRegex.RegexFixedRateExtend;
			
			else if (("StrikeSchedule" == parentName) && ("step" == m_FldParent.Name))
				m_ControlGUI.Regex = EFSRegex.TypeRegex.RegexFixedRateExtend;
			else if (("Schedule" == parentName) && ("step" == m_FldParent.Name))
			{
				if (("FloatingRateCalculation" == grandParentName) &&  ("floatingRateMultiplierSchedule" == m_FldGrandParent.Name))
                    m_ControlGUI.Regex = EFSRegex.TypeRegex.RegexAmountSignedExtend;
				else
					m_ControlGUI.Regex = EFSRegex.TypeRegex.RegexFixedRateExtend;
			}
			else
				m_ControlGUI.Regex = EFSRegex.TypeRegex.RegexAmountSignedExtend;
		}
		#endregion GetRegexDecimal
        #region public SetEventHandler
        public void SetEventHandler()
        {
            TextBox txt = (TextBox)Controls[0];
            object obj = m_FldCapture.GetValue(m_Capture);
            if ((null!=obj) && obj.GetType().Equals(typeof(EFS_Id)))
            {
                txt.TextChanged += new System.EventHandler(OnIdChanged);
            }
            else
            {
                if (null != m_Parent)
                {
                    if ((m_Parent.GetType().Name.Equals("FloatingRateCalculation") || m_Parent.GetType().Name.Equals("FloatingRate")) &&
                        m_Capture.GetType().Name.Equals("Interval"))
                    {
                        txt.TextChanged += new System.EventHandler(OnIntervalChanged);
                    }
                    else if (m_Capture.GetType().Name.Equals("InformationSource") && m_FldCapture.Name.Equals("rateSourcePageHeading"))
                    {
                        txt.TextChanged += new System.EventHandler(OnRateSourcePageHeadingChanged);
                    }
                }
            }
        }
        #endregion
        #endregion Methods
    }
	#endregion SimpleString
	#region SimpleEnum
	/// <summary>
	/// Classe pour gestion interface full FpML pour Array d'enum
	/// </summary>
	/// EG 20230808 [26454] New 
	[ComVisible(false)]
	public class SimpleEnum : ControlBase
	{
		public SimpleEnum(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
			: base(pCapture, pFldCapture, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
		{
			try
			{
				if (null == m_FldCapture.GetValue(m_Capture))
				{
					object o = m_FldCapture.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
					m_FldCapture.SetValue(m_Capture, o);
				}

				object obj = m_FldCapture.GetValue(m_Capture);
				Type tObj = obj.GetType();
				m_ControlGUI = pControlGUI;
				string val = (string)obj;

				int ctrlWidth = 150;
				if ((null != pControlGUI) && (0 != pControlGUI.Width))
					ctrlWidth = pControlGUI.Width;

				Validator validatorMandatory = null;
				Validator validator = null;

				if (MethodsGUI.IsMandatoryControl(m_FldCapture))
				{
					string name = MethodsGUI.GetValidationSummaryMessage(m_FldCapture, m_FldParent);
					validatorMandatory = new Validator(name, true);
				}

				string helpUrl = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);
				FpMLTextBox txt;

				if (null == validator)
				{
					if (null == validatorMandatory)
						txt = new FpMLTextBox(m_FullCtor, val, ctrlWidth, m_FldCapture.Name, pControlGUI, null, false, null, helpUrl);
					else
						txt = new FpMLTextBox(m_FullCtor, val, ctrlWidth, m_FldCapture.Name, pControlGUI, null, false, null, helpUrl, validatorMandatory);
				}
				else if (null == validatorMandatory)
					txt = new FpMLTextBox(m_FullCtor, val, ctrlWidth, m_FldCapture.Name, pControlGUI, null, false, null, helpUrl, validator);
				else
					txt = new FpMLTextBox(m_FullCtor, val, ctrlWidth, m_FldCapture.Name, pControlGUI, null, false, null, helpUrl, validatorMandatory, validator);

				txt.ID = m_FullCtor.GetUniqueID();

				Controls.Add(txt);
			}
			catch (Exception)
			{
				throw;
			}
		}
		protected override void Render(HtmlTextWriter writer)
		{
			try
			{
				string value_uniqueID = Controls[0].UniqueID;
				object obj = m_FldCapture.GetValue(m_Capture);
				string val = (string)obj;
				Control ctrl = Page.FindControl(value_uniqueID);
				if (null != ctrl)
					((TextBox)ctrl).Text = val;
				base.Render(writer);
			}
			catch (Exception)
			{
				throw;
			}
		}
		public void SaveClass(PageBase page)
		{
			try
			{
				string value_uniqueID = this.Controls[0].UniqueID;
				string val = string.Empty;

				if (null != page.Request.Form[value_uniqueID])
				{
					val = page.Request.Form[value_uniqueID].ToString();
					object obj = m_FldCapture.GetValue(m_Capture);
					if (null != obj)
						m_FldCapture.SetValue(m_Capture, StrFunc.IsFilled(val) ? val : null);
				}
				FieldInfo fieldInfoSpecified = m_Capture.GetType().GetField(m_FldCapture.Name + Cst.FpML_SerializeKeySpecified);
				if (null != fieldInfoSpecified)
					fieldInfoSpecified.SetValue(m_Capture, StrFunc.IsFilled(val));
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
	#endregion SimpleEnum

	#region DateTimeOffsetCalendar
	// EG 20171025 [23509] Upd
	// EG 20180423 Analyse du code Correction [CA1405]
	[ComVisible(false)]
    public class DateTimeOffsetCalendar : ControlBase
    {
        #region Constructors
        // EG 20180423 Analyse du code Correction [CA2200]
        public DateTimeOffsetCalendar(object pCapture, FieldInfo pFldCapture, ControlGUI pControlGUI, object pParent, FieldInfo pFldParent, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
            : base(pCapture, pFldCapture, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor)
        {
            try
            {
                string dtValue = string.Empty;
                object obj = m_FldCapture.GetValue(m_Capture);
                if (ReflectionTools.IsBaseType(m_FldCapture, typeof(DateTimeOffsetGUI)))
                {
                    if (null != obj)
                    {
                        FieldInfo fldDate = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                        object objResult = fldDate.GetValue(obj);
                        if (null != objResult)
                            dtValue = objResult.ToString();
                    }
                }
                else
                    dtValue = obj.ToString();

                string name = MethodsGUI.GetValidationSummaryMessage(m_FldCapture, m_FldParent);
                string helpUrl = MethodsGUI.GetHelpLink(m_Capture, m_FldCapture);

                FpMLTimestampBox timestamp = new FpMLTimestampBox(m_FullCtor, dtValue, m_FldCapture.Name, pControlGUI, helpUrl);
                Controls.Add(timestamp);
            }
            catch (Exception)
            {
                throw;
            }
        }
		#endregion Constructors
		#region Events
		#region protected override Render
		// EG 20180423 Analyse du code Correction [CA2200]
		// EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
		protected override void Render(HtmlTextWriter writer)
        {
            try
            {
				bool isTemplate = MethodsGUI.IsStEnvironment_Template(this.Page);
				object obj = m_FldCapture.GetValue(m_Capture);
                if (null != obj)
                {
                    FieldInfo fldDate = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                    object objResult = fldDate.GetValue(obj);

                    if (null != objResult)
                    {
                        string date = objResult.ToString();
						if (StrFunc.IsFilled(date))
                        {
							if (Controls[0] is FpMLTimestampBox tsBox)
							{
								if (Tz.Tools.IsDateFilled(date))
								{
									tsBox.ControlDatePicker.Zdt.Parse = date;
									tsBox.ControlDatePicker.SetZonedDateTime();
								}
								else if (isTemplate)
                                {
									tsBox.ControlDatePicker.Text = date;
								}
							}
						}
                    }
                }
                base.Render(writer);
            }
            catch (Exception)
            {
                throw;
            }
        }

		#endregion Render
		#endregion Events
		#region Methods
		#region public SaveClass
		// EG 20180423 Analyse du code Correction [CA2200]
		// EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
		public void SaveClass(PageBase page)
        {
            try
            {
                string valDate = string.Empty;
                if (Controls[0] is FpMLTimestampBox tsBox)
                {
                    string Date_uniqueID = tsBox.ControlDatePicker.UniqueID;
                    if (null != page.Request.Form[Date_uniqueID])
                    {
						valDate = page.Request.Form[Date_uniqueID];
						if (Tz.Tools.IsDateFilled(valDate))
                        {
							if (tsBox.ControlDatePicker.IsAltTime)
							{
								valDate = Tz.Tools.DateToStringISO(page.Request.Form[Date_uniqueID]);
								if (null != page.Request.Form[Date_uniqueID + "_Time"])
									valDate = Tz.Tools.AddTimeToDateReturnString(valDate, page.Request.Form[Date_uniqueID + "_Time"]);
							}
							else
							{
								valDate = Tz.Tools.DateTimeToStringISO(page.Request.Form[Date_uniqueID]);
							}
							valDate = DtFunc.AddEndUTCMarker(valDate);
						}
                    }
                }

				bool isTemplate = MethodsGUI.IsStEnvironment_Template(this.Page);
				bool isUseInterpretation = true;
                if (MethodsGUI.IsStEnvironment_Template(this.Page))
                    isUseInterpretation = MethodsGUI.IsZoomOnFull(this.Page);

                if (isUseInterpretation)
                {
                    CustomCaptureInfo cci = new CustomCaptureInfo
                    {
                        DataType = TypeData.TypeDataEnum.datetimeoffset
                    };

                    if (null != cci)
                    {
                        try
                        {
                            //Try cath en cas d'interpretation qui bug => string.Empty ds ce cas 
                            cci.NewValueFromLiteral = valDate;
                            valDate = cci.NewValue;
                        }
                        catch { if (!isTemplate) valDate = string.Empty; }
                    }
                }

                if (ReflectionTools.IsBaseType(m_FldCapture, typeof(DateTimeOffsetGUI)))
                {
                    PropertyInfo propTS = m_Capture.GetType().GetProperty(m_FldCapture.Name.Substring(1));
                    if (null != propTS)
                    {
                        propTS.SetValue(m_Capture, valDate);
                        FieldInfo fldTSSpecified = m_Capture.GetType().GetField(propTS.Name + Cst.FpML_SerializeKeySpecified);
                        if (null != fldTSSpecified)
                        {
                            fldTSSpecified.SetValue(m_Capture, Tz.Tools.IsDateFilled(valDate));
                        }
                    }
                    else
                    {
                        object obj = m_FldCapture.GetValue(m_Capture);
                        if (null != obj)
                        {
                            FieldInfo fldDate = obj.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
                            fldDate.SetValue(obj, valDate);
							FieldInfo fldTemplate = obj.GetType().GetField("isTemplate");
							fldTemplate.SetValue(obj, isTemplate);
						}
                    }

                }
                else
                {
                    m_FldCapture.SetValue(m_Capture, valDate);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion SaveClass
        #endregion Methods
    }
    #endregion DateTimeOffsetCalendar
	#endregion Global Objects Complex Controls Types
}
