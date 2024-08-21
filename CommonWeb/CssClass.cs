using System;


// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    // EG 20100125 Add Css for Events
    public sealed class EFSCssClass
    {
        // Constants Event
        public const string Event_Subevent = "event-subevent";
        public const string Event_Deactiv = " event-deactiv";
        public const string Event_Locked = " event-locked";

        //Cosntante utilisé pour générer la CssClass d'un control (voir méthode GetCssClass)
        public const string Multiline = "Multiline";
        public const string Optional = "Optional";
        public const string Consult = "Consult";
        public const string Numeric = "Numeric";
        //
        public const string Msg_Alert = "Msg_Alert";
        public const string Msg_Warning = "Msg_Warning";
        public const string Msg_Information = "Msg_Information";
        public const string Msg_Success = "Msg_Success";
        //
        #region public Enum CssClassEnum
        public static bool IsUnknown(string pCssClass)
        {
            return (pCssClass == CssClassEnum.UNKNOWN.ToString());
        }

        public static bool IsUnknown(CssClassEnum pCssClass)
        {
            return (pCssClass == CssClassEnum.UNKNOWN);
        }
        // EG 20180525 [23979] IRQ Processing
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Suppression des classes CSS et méthodes obsolètes 
        public enum CssClassEnum
        {
            UNKNOWN,

            chkCapture,
            txtCapture,
            txtCaptureOptional,
            txtCaptureConsult,

            txtCaptureMultiline,
            txtCaptureMultilineOptional,
            txtCaptureMultilineConsult,

            txtCaptureNumeric,
            txtCaptureNumericOptional,
            txtCaptureNumericConsult,

            txtTotal,
            txtTotalOptional,
            txtTotalConsult,
            txtTotalConsult2,

            txtTotalMultiline,
            txtTotalMultilineOptional,
            txtTotalMultilineConsult,

            txtTotalNumeric,
            txtTotalNumericOptional,
            txtTotalNumericConsult,

            txtNoBorder,
            ddlCapture,
            ddlCaptureLight,
            ddlOptionGroup,
            ddlJQOptionGroup,
            label,
            lblCapture,
            lblCaptureTitle,
            lblCaptureTitleBold,
            lblDisplay,
            lblDisplayFK,
            lblDisplayCustomObject,
            frmbtn,
            DataGrid_Payer,
            DataGrid_Receiver,
            DataGrid_PayerReceiver,
            DataGrid_Buyer,
            DataGrid_Seller,
            DataGrid_Buyer_Light,
            DataGrid_Seller_Light,
            DataGrid_BuyerSeller,
            DataGrid_txtSetPageNumber,
            DataGrid_SuccessBackColor,
            DataGrid_ErrorBackColor,
            DataGrid_NaBackColor,
            DataGrid_InfoBackColor,
            // EG 20091110
            DataGrid_ItemStyle,
            DataGrid_AlternatingItemStyle,
            DataGrid_AlternatingTotalErrorStyle,
            DataGrid_WarningBackColor,
            DataGrid_TotalStyle,
            DataGrid_TotalErrorStyle,
            DataGrid_GroupStyle,
            DataGrid_GroupStyle1,
            DataGrid_GroupStyle2,

            PnlRoundedColor2,

            leftFixedCol,
            rightFixedCol,

            StatusSuccess,
            StatusSuccessWarning,
            StatusError,
            StatusInterrupt,
            StatusUnXxxx,
            StatusNotfound,
            StatusInfo,
            StatusNA,
            StatusOthers,
            StatusProgress,
            StatusPending,
            //
            LevelOtherMsg,
            LevelAlertMsg,
            LevelInfoMsg,
            LevelWarningMsg,
            //
            ReadyStateActiveMsg,
            ReadyStateRequestedMsg,
            ReadyStateTerminatedMsg,
            // EG 20160216 New
            dg_ask,
            dg_bid,
            dg_mid,
            dg_officialclose,
            dg_officialsettlement,
            dg_quotetimingclose,
            dg_quotetimingopen,
            dg_na,
            dg_otm,
            dg_itm,
            dg_atm,
            // CC/PL 20170307 [22916] 
            dg_atm_itm,
            dg_atm_otm,
            dg_payer,
            dg_receiver,
            dg_payerreceiver,
            dg_buyer,
            dg_seller,
            dg_buyer_light,
            dg_seller_light,
            dg_buyerseller,
        }
        #endregion
        //
        #region Accessor
        public static string Capture
        {
            get { return CssClassEnum.txtCapture.ToString(); }
        }
        public static string CaptureCheck
        {
            get { return CssClassEnum.chkCapture.ToString(); }
        }
        public static string CaptureOptional
        {
            get { return CssClassEnum.txtCaptureOptional.ToString(); }
        }
        public static string CaptureConsult
        {
            get { return CssClassEnum.txtCaptureConsult.ToString(); }
        }
        public static string CaptureMultiline
        {
            get { return CssClassEnum.txtCaptureMultiline.ToString(); }
        }
        public static string CaptureMultilineOptional
        {
            get { return CssClassEnum.txtCaptureMultilineOptional.ToString(); }
        }
        public static string CaptureMultilineConsult
        {
            get { return CssClassEnum.txtCaptureMultilineConsult.ToString(); }
        }
        public static string CaptureNumeric
        {
            get { return CssClassEnum.txtCaptureNumeric.ToString(); }
        }
        public static string CaptureNumericOptional
        {
            get { return CssClassEnum.txtCaptureNumericOptional.ToString(); }
        }
        public static string CaptureNumericConsult
        {
            get { return CssClassEnum.txtCaptureConsult.ToString(); }
        }
        public static string Total
        {
            get { return CssClassEnum.txtTotal.ToString(); }
        }
        public static string TotalOptional
        {
            get { return CssClassEnum.txtTotalOptional.ToString(); }
        }
        public static string TotalConsult
        {
            get { return CssClassEnum.txtTotalConsult.ToString(); }
        }
        // EG 20091110
        public static string TotalConsult2
        {
            get { return CssClassEnum.txtTotalConsult2.ToString(); }
        }
        public static string TotalMultiline
        {
            get { return CssClassEnum.txtTotalMultiline.ToString(); }
        }
        public static string TotalMultilineOptional
        {
            get { return CssClassEnum.txtTotalMultilineOptional.ToString(); }
        }
        public static string TotalMultilineConsult
        {
            get { return CssClassEnum.txtTotalMultilineConsult.ToString(); }
        }
        public static string TotalNumeric
        {
            get { return CssClassEnum.txtTotalNumeric.ToString(); }
        }
        public static string TotalNumericOptional
        {
            get { return CssClassEnum.txtTotalNumericOptional.ToString(); }
        }
        public static string TotalNumericConsult
        {
            get { return CssClassEnum.txtTotalConsult.ToString(); }
        }

        public static string DropDownListCapture
        {
            get { return CssClassEnum.ddlCapture.ToString(); }
        }
        public static string DropDownListOptionGroup
        {
            get { return CssClassEnum.ddlOptionGroup.ToString(); }
        }
        public static string DropDownListJQOptionGroup
        {
            get { return CssClassEnum.ddlJQOptionGroup.ToString(); }
        }
        
        public static string DropDownListCaptureLight
        {
            get { return CssClassEnum.ddlCaptureLight.ToString(); }
        }
        public static string NoBorder
        {
            get { return CssClassEnum.txtNoBorder.ToString(); }
        }
        public static string Label
        {
            get { return CssClassEnum.label.ToString(); }
        }
        public static string LabelCapture
        {
            get { return CssClassEnum.lblCapture.ToString(); }
        }
        public static string LabelDisplay
        {
            get { return CssClassEnum.lblDisplay.ToString(); }
        }
        public static string LabelDisplayForeignKey
        {
            get { return CssClassEnum.lblDisplayFK.ToString(); }
        }
        public static string LabelDisplayCustomObject
        {
            get { return CssClassEnum.lblDisplayCustomObject.ToString(); }
        }
        public static string DataGrid_txtSetPageNumber
        {
            get { return CssClassEnum.DataGrid_txtSetPageNumber.ToString(); }
        }
        public static string DataGrid_ItemStyle
        {
            get { return CssClassEnum.DataGrid_ItemStyle.ToString(); }
        }
        public static string DataGrid_AlternatingItemStyle
        {
            get { return CssClassEnum.DataGrid_AlternatingItemStyle.ToString(); }
        }
        // EG 20091110
        public static string DataGrid_AlternatingTotalErrorStyle
        {
            get { return CssClassEnum.DataGrid_AlternatingTotalErrorStyle.ToString(); }
        }
        public static string DataGrid_TotalErrorStyle
        {
            get { return CssClassEnum.DataGrid_TotalErrorStyle.ToString(); }
        }
        public static string DataGrid_GroupStyle
        {
            get { return CssClassEnum.DataGrid_GroupStyle.ToString(); }
        }
        public static string DataGrid_GroupStyle1
        {
            get { return CssClassEnum.DataGrid_GroupStyle1.ToString(); }
        }
        public static string DataGrid_GroupStyle2
        {
            get { return CssClassEnum.DataGrid_GroupStyle2.ToString(); }
        }
        public static string LeftFixedCol
        {
            get { return CssClassEnum.leftFixedCol.ToString(); }
        }
        public static string RightFixedCol
        {
            get { return CssClassEnum.rightFixedCol.ToString(); }
        }
        #endregion Accessor
        //
        #region public GetCssClass
        /// <summary>
        /// Genere la classe à appliquer à un contrôle,
        /// pClass est la classe de base
        ///<para>
        /// Paramètre valide pour pClass (txtCapture ou txtTotal)
        ///</para>
        ///  
        /// </summary>
        /// <param name="pIsNumeric"></param>
        /// <param name="pIsMandatory"></param>
        /// <param name="pIsMultiline"></param>
        /// <param name="pIsConsult"></param>
        /// <example>
        ///   si la classe de base est txtCapture
        ///   alors la fonction retourne 
        /// 
        ///    txtCapture ou
        ///    txtCaptureOptional ou
        ///    txtCaptureConsult ou
        /// 
        ///    txtCaptureMultiline ou
        ///    txtCaptureMultilineOptional ou
        ///    txtCaptureMultilineConsult ou
        ///
        ///    txtCaptureNumeric ou
        ///    txtCaptureNumericOptional ou
        ///    txtCaptureNumericConsult
        /// </example> 
        /// <returns></returns>
        public static string GetCssClass(bool pIsNumeric, bool pIsMandatory, bool pIsMultiline, bool pIsConsult)
        {
            return GetCssClass(pIsNumeric, pIsMandatory, pIsMultiline, pIsConsult, EFSCssClass.Capture);

        }
        public static string GetCssClass(bool pIsNumeric, bool pIsMandatory, bool pIsMultiline, bool pIsConsult, string pClass)
        {

            string css = pClass;
            
            if (pIsNumeric)
                css += Numeric;
            else if (pIsMultiline)
                css += Multiline;
            
            if (pIsConsult)
            {
                // Mode Consultation
                css += Consult;
            }
            else
            {
                // Mode Saisie
                if (!pIsMandatory)
                    css += Optional;
            }
            
            if (Enum.IsDefined(typeof(EFSCssClass.CssClassEnum), css))
            {
                CssClassEnum ret = (EFSCssClass.CssClassEnum)Enum.Parse(typeof(EFSCssClass.CssClassEnum), css, true);
                return ret.ToString();
            }
            else
            {
                return pClass;
            }
        }
        #endregion
        #region public GetCssClassBase
        /// <summary>
        /// Retourne la class de base
        /// <para>Ex retourne txtCapture si {pClass} vaut txtCaptureMultilineOptional</para>
        /// <para>Retourne string empty si pClass n'est pas une valeur présente dans l'enum EFSCssClass.CssClassEnum</para>
        /// </summary>
        /// <param name="pClass"></param>
        /// <returns></returns>
        public static string GetCssClassBase(string pClass)
        {
            string ret = string.Empty;
            bool isEfsCssClass = Enum.IsDefined(typeof(EFSCssClass.CssClassEnum), pClass);
            if (isEfsCssClass)
            {
                ret = pClass;
                ret = ret.Replace(Multiline, string.Empty);
                ret = ret.Replace(Numeric, string.Empty);
                ret = ret.Replace(Optional, string.Empty);
                ret = ret.Replace(Consult, string.Empty);
            }
            return ret;
        }
        #endregion
    }
}
