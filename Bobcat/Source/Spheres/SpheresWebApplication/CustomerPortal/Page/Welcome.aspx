<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Welcome.aspx.cs" Inherits="EFS.Spheres.Welcome" %>

<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8" />
    <title>Welcome Page</title>

    <link href="https://fonts.googleapis.com/css?family=Poppins:100,400,300,700,100italic,300italic|Oswald:200,300,400,500,600,700|Raleway:100,200,300,400,500,600,700,100italic,300italic" rel="stylesheet" type="text/css">
    <link rel="preconnect" href="https://fonts.gstatic.com">
    <link href="https://fonts.googleapis.com/css2?family=Allerta+Stencil&family=Josefin+Sans:wght@600&family=Nova+Square&family=Zen+Dots&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="../../Includes/fontawesome-all.min.css" />
    <link rel="stylesheet" href="../content/Common.min.css" />
    <link rel="stylesheet" href="../Content/Welcome.min.css" />
    <link rel="stylesheet" href="../Content/policy.min.css" />
    <link id="linkCssAwesome" rel="stylesheet" type="text/css" href="../../Includes/fontawesome-all.min.css">
    <link id="linkCssCommon" rel="stylesheet" type="text/css" href="../../Includes/EFSThemeCommon.min.css">
    <link id="linkCssCustomCommon" rel="stylesheet" type="text/css" href="../../Includes/CustomThemeCommon.css">
    <link id="linkCssUISprites" rel="stylesheet" type="text/css" href="../../Includes/EFSUISprites.min.css">
    <link id="linkCss" rel="stylesheet" type="text/css" href="../../Includes/EFSTheme-vlight.min.css">
    <link id="linkCssJQueryUI" rel="stylesheet" type="text/css" href="../../Javascript/JQuery/jquery-ui-1.13.3.min.css">
    <link id="linkCssTimePicker" rel="stylesheet" type="text/css" href="../../Javascript/JQuery/jquery-ui-timepicker.min.css">
</head>
<body>
    <form id="welcome" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <input id="hidIsConnected" type="hidden" name="hidIsConnected" runat="server" />
        <input id="hidIDA" type="hidden" name="hidIDA" runat="server" />
        <section id="pg-welcome">
            <div id="pg-welcome-box">
                <div id="pg-welcome-content">
                    <div id="logoCustomer" runat="server"></div>
                    <div id="pg-welcome-title">
                        <h1 xct-sequence="cool88" id="lblWelcomeCustomerPortal" runat="server"></h1>
                        <h2 xct-sequence="cool88" id="lblWelcomeCustomerPortal2" runat="server"></h2>
                        <div id="infologin" class="user">
                            <small>
                                <asp:Label xct-sequence="cool88" ID="lastLogin" runat="server"></asp:Label></small>
                        </div>
                        <div id="infoCustomer" runat="server"></div>
                        <asp:Label ID="activeUser" runat="server"></asp:Label>
                    </div>
                </div>
            </div>
        </section>

        <script type="text/javascript" src="../scripts/plugin/tweenmax.min.js"></script>
        <script type="text/javascript" src="../scripts/plugin/cooltext.animation.min.js"></script>
        <script type="text/javascript" src="../scripts/plugin/cooltext.min.js"></script>

        <!--EG 20210908 [XXXXX] Cookies consent : Cookie consent instructions -->
        <!-- Cookie Consent by https://www.CookieConsent.com -->
        <script type="text/javascript" src="//www.cookieconsent.com/releases/4.0.0/cookie-consent.js" charset="UTF-8"></script>
        <script type="text/javascript" charset="UTF-8">
            document.addEventListener('DOMContentLoaded', function () {
                // Culture des pages de consentement
                var currentCulture = '<%= System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName %>';
            // Chemin d'accès aux poliques de consentement
            var urlPolicies = '<%= ConfigurationManager.AppSettings["EFSWebsitePath"]%>' + "/Pages/" + '<%= System.Globalization.CultureInfo.CurrentCulture.Name %>' + "/PrivacyPolicy.aspx";

            cookieconsent.run({
                "notice_banner_type": "simple", "consent_type": "express", "palette": "dark", "language": currentCulture,
                "page_load_consent_levels": ["strictly-necessary", "functionality", "tracking", "targeting"], "notice_banner_reject_button_hide": false,
                "preferences_center_close_button_hide": false, "website_privacy_policy_url": urlPolicies, "open_preferences_center_selector": "#cookies_preferences"
            });
        });
        </script>
        <noscript>ePrivacy and consent to cookies and GDPR by <a href="https://www.CookieConsent.com/" rel="nofollow noopener">Consent to cookies</a></noscript>
        <!-- End Cookie Consent by https://www.CookieConsent.com -->

    </form>
</body>
</html>
