#region Using Directives
using System;
using System.Drawing;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.Security;

#endregion Using Directives

namespace EFS.ACommon
{
    // EG 20150923 New Use CSS Sprites mode = 1 image with selection by CSS (background-position) 
    // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
    public sealed class CstCSSColor
    {
        public const string
            black = "#000",
            blueDark = "#003082",
            blue = "#036AB5",
            blueMedium = "#3483C1",
            blueLight = "#84B6DB",
            blueLighter = "#D1DEE9",

            cyanDark = "#00ADC3",
            cyan = "#1CBCF0",
            cyanMedium = "#50C9F0",
            cyanLight = "#86B6DB",
            cyanLighter = "#E0F3F8",

            grayDark = "#251401",
            gray = "#545454",
            grayMedium = "#737373",
            grayLight = "#B4B4B4",
            grayLighter = "#DCDCDC",

            greenDark = "#3A7810",
            green = "#51AD26",
            greenMedium = "#77BC51",
            greenLight = "#C9DDAD",
            greenLighter = "#DBEAC8",

            marronDark = "#491F00",
            marron = "#673406",
            marronMedium = "#835324",
            marronLight = "#C19863",
            marronLighter = "#E3C7A1",

            orangeDark = "#C94300",
            orange = "#EA5A00",
            orangeMedium = "#EF7A27",
            orangeLight = "#F7BD77",
            orangeLighter = "#FBDFBD",

            redDark = "#AE0303",
            red = "#C00303",
            redMedium = "#D23838",
            redLight = "#F9ACAC",
            redLighter = "#FCD4D4",

            roseDark = "#CE5F92",
            rose = "#EA71A8",
            roseMedium = "#ED82AE",
            roseLight = "#F4BBC6",
            roseLighter = "#FDE3E3",

            violetDark = "#674275",
            violet = "#8B569F",
            violetMedium = "#9E75B0",
            violetLight = "#C6B5D3",
            violetLighter = "#E1D8E8",

            yellowDark = "#908516",
            yellow = "#D8C825",
            yellowMedium = "#F8EB66",
            yellowLight = "#F4EAAF",
            yellowLighter = "#FBF8E5";

        public static Color Convert(string pColor)
        {
            return Color.FromName(pColor);
        }
        public static Color Reverse(Color pColor)
        {
            string reverseColor = pColor.Name;
            switch (pColor.Name)
            {
                case blueMedium:
                    reverseColor = blueLight;
                    break;
                case cyanMedium:
                    reverseColor = cyanLight;
                    break;
                case grayMedium:
                    reverseColor = grayLight;
                    break;
                case greenMedium:
                    reverseColor = greenLight;
                    break;
                case marronMedium:
                    reverseColor = marronLight;
                    break;
                case orangeMedium:
                    reverseColor = orangeLight;
                    break;
                case redMedium:
                    reverseColor = redLight;
                    break;
                case roseMedium:
                    reverseColor = roseLight;
                    break;
                case violetMedium:
                    reverseColor = violetLight;
                    break;
                case yellowMedium:
                    reverseColor = yellowLight;
                    break;
            }
            return CstCSSColor.Convert(reverseColor);
        }
    }
    public sealed class CstCSS
    {
        public const string
            ActionMenu = "ActionMenu-",

            BannerMenu = "BannerMenu",
            BannerHeaderAbout = "about",
            BannerHeaderAdmin = "admin",
            BannerHeaderExternal = "external",
            BannerHeaderInput = "input",
            BannerHeaderInvoicing = "invoicing",
            BannerHeaderProcess = "process",
            BannerHeaderRepository = "repository",
            BannerHeaderUnknown = "external",
            BannerHeaderViews = "views",

            LblInfo = "LblInfo",
            LblError = "LblError",
            LblSuccess = "LblSuccess",
            LblWarning = "LblWarning",
            LblUnknown = "LblUnknown",

            Menu = "mnu-",
            MainMenu = "MainMenu-",

            SubMenu = "SubMenu-",
            SubMenuTxt = "SubMenuTxt-",

            TxtCapture = "txtCapture";
    }

    public sealed class CSS
    {
        public const string cssClassModel = "ui-{0} ui-{0}-{1}";
        public const string cssPrefixModel = "ui-{0}";
        public enum Sprites
        {
            main,
            tracker,
            poskeeping,
            flags,
            subevent,
        }

        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public enum Main
        {
            customer, 
            entity,
            external, 
        }

        public enum SubEvent
        {
            title, titleblue, titlegreen, titleorange, titleviolet, titlegray, titlered, titlecyan, titlerose,
        }
        public enum Trk
        {
            cptna, cptnone, cptprogress, cptpending, cptsuccess, cptwarning, cpterror,
            ledna, lednone, ledprogress, ledpending, ledsuccess, ledwarning, lederror,
            squarena, squarenone, squareprogress, squarepending, squaresuccess, squarewarning, squareerror,
            ledactive, ledrequested, ledterminated,
            squareactive, squarerequested, squareterminated,
            groupcontent, starttimer, stoptimer, refresh, parameters, detail, @float, noscroll, scroll, serviceobserver, monitoring,
            helpgroup, helpstatus, helpreadystate,
        }
        public static string SetCssClass<T>(T pValue)
        {
            string cssClass;
            if (pValue is Main)
                cssClass = String.Format(cssClassModel, Sprites.main, pValue);
            else if (pValue is Trk)
                cssClass = String.Format(cssClassModel, Sprites.tracker, pValue);
            else
            {
                string @value = pValue as string;
                if (Enum.IsDefined(typeof(Main), @value))
                    cssClass = String.Format(cssClassModel, Sprites.main, @value);
                else if (Enum.IsDefined(typeof(Trk), @value))
                    cssClass = String.Format(cssClassModel, Sprites.tracker, @value);
                else if (@value.StartsWith("pkeep-"))
                    cssClass = @value;
                else
                    cssClass = String.Format(cssClassModel, Sprites.main, @value.ToLower());
            }
            return cssClass;
        }
        public static string SetCssClassFlags(string pValue)
        {
            return String.Format(cssClassModel, Sprites.flags, StrFunc.IsFilled(pValue) ? pValue.ToLower() : "unlisted");
        }
        public static string SetCssClassTracker(string pValue)
        {
            return String.Format(cssClassModel, Sprites.tracker, pValue.ToLower());
        }
        public static bool IsCssClassMain(string pCssClass)
        {
            return pCssClass.StartsWith(String.Format(cssPrefixModel, Sprites.main.ToString()));
        }
    }
}