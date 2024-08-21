using EFS.ACommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace EFS.Common.Web
{
    /// <summary>
    /// Description résumée de FpMLValidator.
    /// </summary>
    /// EG 20234429 [WI756] Spheres Core : Refactoring Code Analysis
    public class Validator
	{
		#region Variables
		private readonly Cst.TypeValidator  m_TypeValidator     = Cst.TypeValidator.None;
		private string             m_Text              = "*";
		private string             m_ErrorMessage      = "*";
		private string             m_RegularExpression = string.Empty;
		private bool               m_IsShowMessage     = true;
		private bool               m_IsSummaryOnly     = true;

		private readonly EFSRegex.TypeRegex m_TypeRegex         = EFSRegex.TypeRegex.None;
		private readonly ValidationDataType m_DataType          = ValidationDataType.String;
		private readonly string             m_MinValue          = string.Empty;
		private readonly string             m_MaxValue          = string.Empty;
		private ValidatorDisplay   m_Display           = ValidatorDisplay.Dynamic;
		private readonly string             m_ClientFunction    = string.Empty;

		#endregion Variables

		#region Accessors
		public Cst.TypeValidator TypeValidator
		{
			get {return m_TypeValidator;}
		}
		public string ErrorMessage 
		{
			get {return m_ErrorMessage;}
            set {m_ErrorMessage = value;}
		}
		public bool IsShowMessage 
		{
			get {return m_IsShowMessage;}
            set { m_IsShowMessage = value; }
		}
		public bool IsSummaryOnly 
		{
			get {return m_IsSummaryOnly;}
            set { m_IsSummaryOnly = value; }
		}
		public EFSRegex.TypeRegex RegexTemplate 
		{
			get {return m_TypeRegex;}
		}
		public string Text 
		{
			get {return m_Text;}
		}
		public string RegularExpression 
		{
			get {return m_RegularExpression;}
		}
		public ValidationDataType RangeDataType 
		{
			get {return m_DataType;}
		}
		public string MinValue 
		{
			get {return m_MinValue;}
		}
		public string MaxValue 
		{
			get {return m_MaxValue;}
		}
        public ValidatorDisplay Display
        {
            get { return m_Display; }
            set { m_Display = value; }
        }
		#endregion Accessors
		
		#region constructor
		public Validator():this("*",true,true){}
		public Validator(bool pIsSummaryOnly):this("*",true,pIsSummaryOnly){}
		public Validator(string pErrorMessage, bool pIsShowMessage):this(pErrorMessage,pIsShowMessage,true){}
		public Validator(string pErrorMessage, bool pIsShowMessage,string pRegularExpression)
			:this(pErrorMessage,pIsShowMessage,pRegularExpression,true){}
		public Validator(EFSRegex.TypeRegex pTypeRegEx,string pErrorMessage,bool pIsShowMessage)
			:this(pTypeRegEx,pErrorMessage,pIsShowMessage,true){}
		public Validator(string pErrorMessage, bool pIsShowMessage,ValidationDataType pRangeDataType,string pMinValue,string pMaxValue)
			:this(pErrorMessage,pIsShowMessage,true,pRangeDataType,pMinValue,pMaxValue){}

		public Validator(string pErrorMessage, bool pIsShowMessage,bool pIsSummaryOnly)
		{
			m_TypeValidator = Cst.TypeValidator.RequireFieldNavigator;
			m_ErrorMessage  = pErrorMessage;
			m_IsShowMessage = pIsShowMessage;
			m_IsSummaryOnly = pIsSummaryOnly;
		}

		public Validator(string pErrorMessage, bool pIsShowMessage,string pRegularExpression,bool pIsSummaryOnly)
		{
			m_TypeValidator     = Cst.TypeValidator.ExpressionValidator;
			m_ErrorMessage      = pErrorMessage;
			m_IsShowMessage     = pIsShowMessage;
			m_RegularExpression = pRegularExpression;
			m_IsSummaryOnly     = pIsSummaryOnly;
		}

		public Validator(EFSRegex.TypeRegex pTypeRegex,string pErrorMessage,bool pIsShowMessage,bool pIsSummaryOnly)
		{
			m_TypeValidator = Cst.TypeValidator.ExpressionValidator;
			m_ErrorMessage  = pErrorMessage;
			m_IsShowMessage = pIsShowMessage;
			m_IsSummaryOnly = pIsSummaryOnly;
			m_TypeRegex     = pTypeRegex;
		}

		public Validator(string pErrorMessage, bool pIsShowMessage,bool pIsSummaryOnly,ValidationDataType pRangeDataType,
			string pMinValue,string pMaxValue)
		{
			m_TypeValidator = Cst.TypeValidator.RangeValidator;
			m_ErrorMessage  = pErrorMessage;
			m_IsShowMessage = pIsShowMessage;
			m_IsSummaryOnly = pIsSummaryOnly;
			m_DataType      = pRangeDataType;
			m_MinValue      = pMinValue;
			m_MaxValue      = pMaxValue;
		}

		public Validator(ValidationDataType pDataCheckType, string pErrorMessage, bool pIsShowMessage,bool pIsSummaryOnly)
		{
			m_TypeValidator = Cst.TypeValidator.CompareValidator;
			m_ErrorMessage  = pErrorMessage;
			m_IsShowMessage = pIsShowMessage;
			m_IsSummaryOnly = pIsSummaryOnly;
			m_DataType      = pDataCheckType;
		}

		public Validator(string pClientFunction, string pErrorMessage, bool pIsShowMessage,bool pIsSummaryOnly)
		{
			m_TypeValidator  = Cst.TypeValidator.CustomValidator;
			m_ErrorMessage   = pErrorMessage;
			m_IsShowMessage  = pIsShowMessage;
			m_IsSummaryOnly  = pIsSummaryOnly;
			m_ClientFunction = pClientFunction;
		}

		#endregion constructor

		#region public CreateControl
		public WebControl CreateControl(string pId)
		{
			WebControl ctrl = null;
			try
			{
				DateTimeFormatInfo dtfi = CultureInfo.CurrentUICulture.DateTimeFormat;
				NumberFormatInfo nbfi   = CultureInfo.CurrentUICulture.NumberFormat;
				switch (m_TypeValidator)
				{
					case Cst.TypeValidator.RequireFieldNavigator:
						ctrl = new RequiredFieldValidator();
						break;
					case Cst.TypeValidator.ExpressionValidator:
						ctrl = new RegularExpressionValidator();

                        if (m_TypeRegex != EFSRegex.TypeRegex.None)
						    m_RegularExpression =  EFSRegex.RegularExpression( m_TypeRegex); 
						ConstructErrorMessage(EFSRegex.ErrorMessage(m_TypeRegex), true);

                        ((RegularExpressionValidator)ctrl).ValidationExpression = m_RegularExpression;
						break;
					case Cst.TypeValidator.RangeValidator:
						//RangeValidator
						ctrl                                = new RangeValidator();
						((RangeValidator)ctrl).Type         = m_DataType;
						((RangeValidator)ctrl).MinimumValue = m_MinValue;
						((RangeValidator)ctrl).MaximumValue = m_MaxValue;
						break;
					case Cst.TypeValidator.CompareValidator:
						//CompareValidator
						ctrl = new CompareValidator();
						if (ValidationDataType.Double == m_DataType)
							ConstructErrorMessage(Ressource.GetString2("RegexDecimalError", nbfi.NumberGroupSeparator, 	nbfi.NumberDecimalSeparator), true);
						((CompareValidator)ctrl).Operator = ValidationCompareOperator.DataTypeCheck;
						((CompareValidator)ctrl).Type = m_DataType;
						break;
					case Cst.TypeValidator.CustomValidator:
						//CustomValidator
						ctrl = new CustomValidator();
						((CustomValidator)ctrl).ClientValidationFunction = m_ClientFunction;
						break;
					default:
						return null;
				}
                if (null != ctrl)
                {
                    ctrl.CssClass = "vtor";
                    ((BaseValidator)ctrl).ControlToValidate = pId;
                    if (!m_IsSummaryOnly)
                        m_Text = m_ErrorMessage;
                    ((BaseValidator)ctrl).ErrorMessage = m_ErrorMessage;
                    ((BaseValidator)ctrl).Text = m_Text;
                    ((BaseValidator)ctrl).Display = m_Display;

                }
			}
			catch (Exception ex)
			{System.Diagnostics.Debug.Write("Error: " + ex.Message);}

			return (ctrl);
		}
		#endregion

		#region private ConstructErrorMessage
		private void ConstructErrorMessage(string pErrorMessage, bool pAddBR)
		{
			if (pErrorMessage.StartsWith( m_ErrorMessage.Trim() )) 
				m_ErrorMessage = pErrorMessage;
			else
			{
				if (pAddBR)
					m_ErrorMessage += Cst.HTMLBreakLine;
				m_ErrorMessage += pErrorMessage;
			}
		}
		#endregion private ConstructErrorMessage
		
		#region public static GetValidatorDateRange
        // EG 20180423 Analyse du code Correction [CA2200]
        public static Validator GetValidatorDateRange(string pHeaderMessage, string pFieldName)
		{
            Validator rangeValidator;
            try
			{
				DateTime dtMinDefaultValue = new DtFunc().StringyyyyMMddToDateTime("19000101");
                DateTime dtMaxDefaultValue = new DtFunc().StringyyyyMMddToDateTime("21991231");
				DateTime dtMinConfigValue  = dtMinDefaultValue;
				DateTime dtMaxConfigValue  = dtMaxDefaultValue;
				string minConfigValue = SystemSettings.GetAppSettings("Spheres_MinDate");
				string maxConfigValue = SystemSettings.GetAppSettings("Spheres_MaxDate");
				//
				try
				{
					dtMinConfigValue  = new DtFunc().StringToDateTime(minConfigValue);
					if ((DtFunc.IsDateTimeEmpty(dtMinConfigValue) && (StrFunc.IsFilled(minConfigValue))))
						dtMinConfigValue  = new DtFunc().StringyyyyMMddToDateTime(minConfigValue);
				}
				catch
				{}
				try
				{
					dtMaxConfigValue  = new DtFunc().StringToDateTime(maxConfigValue);
					if ((DtFunc.IsDateTimeEmpty(dtMaxConfigValue) && (StrFunc.IsFilled(maxConfigValue))))
						dtMaxConfigValue  = new DtFunc().StringyyyyMMddToDateTime(maxConfigValue);
				}
				catch
				{}

                if (0 < dtMinDefaultValue.CompareTo(dtMinConfigValue))
					minConfigValue = DtFunc.DateTimeToString(dtMinDefaultValue,DtFunc.FmtShortDate);
				else
					minConfigValue = DtFunc.DateTimeToString(dtMinConfigValue,DtFunc.FmtShortDate);

				if (0 < dtMaxDefaultValue.CompareTo(dtMaxConfigValue))
					maxConfigValue = DtFunc.DateTimeToString(dtMaxConfigValue,DtFunc.FmtShortDate);
				else
					maxConfigValue = DtFunc.DateTimeToString(dtMaxDefaultValue,DtFunc.FmtShortDate);

                StrBuilder message = new StrBuilder(pHeaderMessage);
				message.AppendFormat(" [{0}&lt;={1}&lt;={2}]",minConfigValue,pFieldName,maxConfigValue);
				rangeValidator = new Validator(message.ToString(),true,false,ValidationDataType.Date,minConfigValue,maxConfigValue);
			}
			catch (Exception)
			{
				throw;
			}
			return rangeValidator;
		}
		#endregion GetValidatorDateRange
	
	}
    // EG 20190308 New (used by ClosingReopeningPosition)
    public sealed partial class ValidatorTools
    {
        #region ValidationGroupEnum
        /// <summary>
        /// Main = Panel principal 
        /// Pnl1,Pnl2,Pnl3 = Autres Panel
        /// </summary>
        public enum ValidationGroupEnum
        {
            MAIN, 
            PNL1,
            PNL2,
            PNL3,
        }
        #endregion ValidationGroupEnum

        #region SetValidators
        /// <summary>
        /// Initialisation des validateurs dynamiques
        /// </summary>
        public static void SetValidators(TextBox pControl, Nullable<ValidationGroupEnum> pValidationGroup)
        {
            SetValidators(pControl, EFSRegex.TypeRegex.None, true, EFSCssClass.Capture, pValidationGroup);
        }
        public static void SetValidators(TextBox pControl, bool pIsMandatory, Nullable<ValidationGroupEnum> pValidationGroup)
        {
            SetValidators(pControl, EFSRegex.TypeRegex.None, pIsMandatory, pIsMandatory ? EFSCssClass.Capture : EFSCssClass.CaptureOptional, pValidationGroup);
        }
        public static void SetValidators(TextBox pControl, EFSRegex.TypeRegex pTypeRegex, bool pIsMandatory, string pCssClass, Nullable<ValidationGroupEnum> pValidationGroup,
            params string[] pLabelMessage)
        {
            List<Validator> validators = new List<Validator>();
            Validator validator = null;
            if (StrFunc.IsFilled(pCssClass))
                pControl.CssClass = pCssClass;
            #region RequireField
            if (pIsMandatory)
            {
                validator = new Validator(Ressource.GetString("MissingData"), false, true);
                validators.Add(validator);
            }
            #endregion RequireField
            #region Regular Expression
            if (EFSRegex.TypeRegex.None != pTypeRegex)
            {
                validator = new Validator(pTypeRegex, Ressource.GetString("InvalidData"), false, true);
                validators.Add(validator);
            }
            #endregion Regular Expression
            if (0 < validators.Count)
            {
                string errMessage = SetValidatorMessage(pControl.Page, ArrFunc.IsFilled(pLabelMessage) ? pLabelMessage[0] : pControl.ID);
                validators.ForEach(item =>
                {
                    string lastErr = item.ErrorMessage.StartsWith("<div>") ? item.ErrorMessage.Substring(6, item.ErrorMessage.Length - 11) : item.ErrorMessage;
                    item.ErrorMessage = "<div>" + errMessage + Cst.HTMLBreakLine + "[" + lastErr + "]" + "</div>" ;
                    //item.ErrorMessage = "<div>" + errMessage + Cst.HTMLBreakLine + "[" + item.ErrorMessage + "]" + "</div>" ;
                    WebControl wcValidator = item.CreateControl(pControl.ID);
                    BaseValidator bv = (BaseValidator)wcValidator;
                    if (pValidationGroup.HasValue)
                        bv.ValidationGroup = pValidationGroup.Value.ToString();
                    bv.Display = ValidatorDisplay.None;
                    bv.SetFocusOnError = true;
                    pControl.Controls.Add(wcValidator);
                });
            }
        }
        #endregion SetValidators

        #region SetValidatorMessage
        /// <summary>
        /// Construction du titre du message d'un validateur en fonction du contrôle
        /// </summary>
        /// <param name="pPage">Page</param>
        /// <param name="pControlID">Identifiant du control</param>
        /// <param name="pDefaultMessage">Message par défaut</param>
        /// <returns></returns>
        public static string SetValidatorMessage(Page pPage, string pControlID, params string[] pDefaultMessage)
        {
            string errMessage = Cst.HTMLBold + pControlID + Cst.HTMLEndBold;
            string id = string.Empty;
            if (pControlID.StartsWith(Cst.TXT, StringComparison.OrdinalIgnoreCase) ||
                pControlID.StartsWith(Cst.DDL, StringComparison.OrdinalIgnoreCase))
                id = pControlID.Substring(Cst.TXT.Length);
            if (!(pPage.FindControl(Cst.LBL + id) is WCTooltipLabel lbl))
                lbl = pPage.FindControl(Cst.LBL.ToLower() + id) as WCTooltipLabel;
            if (null != lbl)
                errMessage = Cst.HTMLBold + lbl.Text + Cst.HTMLEndBold;
            else if (StrFunc.IsFilled(id))
                errMessage = Cst.HTMLBold + Ressource.GetString(Cst.LBL.ToLower() + id, false) + Cst.HTMLEndBold;
            if (ArrFunc.IsFilled(pDefaultMessage))
                errMessage += Cst.HTMLBreakLine + "[" + pDefaultMessage[0] + "]";
            return errMessage;
        }
        #endregion SetValidatorMessage
    }
}
