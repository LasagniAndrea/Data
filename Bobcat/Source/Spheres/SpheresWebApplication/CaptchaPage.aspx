<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CaptchaPage.aspx.cs" Inherits="EFS.Spheres.CaptchaPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title id="titlePage" runat="server" />
    <link id="linkFedAuth" href="Includes/fedAuth.min.css" rel="stylesheet" type="text/css" />

    <style type="text/css">
        #captchabody {
            background-image: url(images/logo_software/WelcomeBody_v2024.png);
            background-image: url(images/Logo_Entity/EuroFinanceSystems/Spheres-Bkg-EFSBDX.png);
            background-position: center center;
            background-repeat: no-repeat;
            background-size: cover;
        }

        #frmCaptcha {
            max-width: 350px;
            margin: 10% auto;
        }

        #divCaptcha {
            display: flex;
            flex-direction: column;
            border: 1pt solid #000;
            background-color: transparent !important;
        }

        #divbody {
            display: flex;
            flex-direction: column;
            opacity: 90%;
        }

            #divbody > div {
                display: grid;
                grid-template-columns: 70px auto 30px 30px;
                align-items: center;
                justify-items:center;
                border-top: 1pt solid black;
                padding: 5px;
            }

        a[id^=btn]:focus-visible {
            color: white;
            background-color: #036ab5;
            outline:1pt solid #036ab5;
            border-radius:3px;

        }
        a[id^=btn]:focus-visible i::before {
            color: white;
        }

        a[id^=btn] i::before {
            font-size: large;
            color: #036ab5;
        }

        #txtCaptchaInput {
            background-color: white;
            color: black;
            width: 180px;
            padding: 2px;
            text-align:left;
        }

        #divFooter {
            background-color: black;
            line-height: 25px;
            color: white;
            background-image: none !important;
        }

            #divFooter.error {
                background-color: #C00303;
            }

            #divFooter.success {
                background-color: #51AD26;
            }
    </style>

</head>
<body id="captchabody">
    <form id="frmCaptcha" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <asp:Panel ID="divCaptcha" runat="server">
            <asp:Panel ID="pnlHeader" runat="server">
                <asp:Label ID="lblCaptchaTitle" runat="server">Captcha Security check</asp:Label>
                <asp:Label ID="lblSpheresVersion" runat="server">Version</asp:Label>
                <asp:Timer ID="timerReset" runat="server" OnTick="OnReset" />
            </asp:Panel>
            <asp:Panel ID="divbody" runat="server">
                <asp:Image ID="imgCaptcha" runat="server" />
                <div>
                    <asp:Label ID="lblCaptchaCode" runat="server">Entrez le code CAPTCHA :</asp:Label>
                    <asp:TextBox ID="txtCaptchaInput" runat="server"></asp:TextBox>
                    <asp:LinkButton ID="btnRegenerate" runat="server" CssClass="fa-icon" OnClick="OnRegenerate">Regenerate</asp:LinkButton>
                    <asp:LinkButton ID="btnValidate" runat="server" CssClass="fa-icon" OnClick="OnValidate">Validate</asp:LinkButton>
                </div>
            </asp:Panel>
            <asp:Panel ID="divFooter" runat="server">
                <asp:Label ID="lblCaptchaMessage" runat="server"></asp:Label>
            </asp:Panel>
        </asp:Panel>
    </form>
</body>
</html>
