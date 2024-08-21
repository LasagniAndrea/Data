<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="EFS.Spheres.Login" %>

<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8" />
    <title>Login to customers portal</title>

    <link href="https://fonts.googleapis.com/css?family=Poppins:100,400,300,700,100italic,300italic|Oswald:200,300,400,500,600,700|Raleway:100,200,300,400,500,600,700,100italic,300italic" rel="stylesheet" type="text/css">
    <link rel="preconnect" href="https://fonts.gstatic.com">
    <link href="https://fonts.googleapis.com/css2?family=Allerta+Stencil&family=Josefin+Sans:wght@600&family=Nova+Square&family=Zen+Dots&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="../../Includes/fontawesome-all.min.css" />
    <link rel="stylesheet" href="../content/Common.min.css" />
    <link rel="stylesheet" href="../Content/Login.min.css" />
    <link rel="stylesheet" href="../Content/policy.min.css" />
</head>
<body>
    <header id="hdrLogin" runat="server">
        <div>
            <div id="efsLogo">
                <div>
                    <div>e</div>
                    <div>
                        <div><span>f</span>inance</div>
                        <div><span>s</span>ystems</div>
                    </div>
                </div>
            </div>
            <div id="title"><i class="fa fa-user"></i>Access  | <small>Customer Portal</small></div>
        </div>
    </header>
    <section id="body">
        <section id="pg-login" class="page pg-common">
            <div id="pg-login-box">
                <form id="portalLogin" runat="server">
                    <asp:ScriptManager ID="ScriptManager" runat="server" />
                    <asp:Timer ID="timerReset" runat="server" OnTick="OnReset"/>   
                    <h1 id="loginTitle" runat="server">Sign in to your account</h1>
                    <div>
                        <asp:Label ID="lblIdentifier" runat="server">Identifier</asp:Label>
                        <asp:TextBox autocomplete="username" ID="txtIdentifier" runat="server"></asp:TextBox><br />
                    </div>
                    <div>
                        <asp:Label ID="lblPassword" runat="server">Password</asp:Label>
                        <asp:TextBox autocomplete="current-password" TextMode="Password" ID="txtPassword" runat="server"></asp:TextBox>
                    </div>
                    <div id="error">
                        <asp:Label ID="lblLoginMsg" runat="server"></asp:Label>
                        <asp:Panel ID="divErrLockout" runat="server">
                            <asp:Label ID="lblLockoutMsg" runat="server"></asp:Label>
                        </asp:Panel>
                    </div>
                    <asp:Button runat="server" ID="btnSignIn" OnClick="btnLogin_Click" />
                </form>
            </div>
        </section>
    </section>
    <footer id="ftrLogin" runat="server">
        Powered by Euro Finance Systems | Copyright <i class="fas fa-copyright"></i>2023
    </footer>

    <script type="text/javascript">
        $(document).ready(function () {
            $(parent.document.getElementById('infologin')).html('');
            $(parent.document.getElementById('user-name')).text('');
            $(parent.document.getElementById('dduser')).removeClass("connected");
        });
    </script>

	<script type="text/javascript">
        function stopResetTimer() {
            var timer = $find("timerReset");
            timer._stopTimer();
        }
    </script>

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

</body>
</html>
