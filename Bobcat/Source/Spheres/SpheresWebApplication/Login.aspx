<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>

<%@ Page Language="c#" Inherits="EFS.Spheres.LoginPage" CodeBehind="Login.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title id="titlePage" runat="server" />
    <link id="linkFedAuth" href="Includes/fedAuth.min.css" rel="stylesheet" type="text/css" />

    <style type="text/css">
        #frmLogin {
            max-width: 400px;
            margin: 10% auto;
        }

        #divlogin {
            border: 1pt solid #000;
            background-color:transparent!important;
        }

        #divbody > span,
        #divbody > input,
        #divbody > select {
            font-size: initial;
        }

        #divbody {
            padding: 2px;
            opacity: 90%;
        }

        input[type=checkbox] {
            width: auto;
        }

        input[type=checkbox] + label {
            padding: 0px 8px;
        }

        input {
            color: #fff;
            width: 100%;
            padding: 0px;
            text-align: initial;
            border-radius:0px;
        }

        input:hover{
            border-radius:4px;
        }

        #btnCaptcha {
            width:180px;
            line-height: 34px;
            background-color: #C00303;
            background-image: url(images/png/captcha.png);
            background-repeat:no-repeat;
            background-position:5% center;
            margin: auto;
            padding-left:20px;
            font-size: small;
            font-weight: bold;
            color: #fff !important;
            text-align: center;
        }

    </style>

    <script type="text/javascript">
        var banner = parent.document.getElementById("banner");
        var boardInfo = parent.document.getElementById('boardInfo');
        var requesterInfo = parent.document.getElementById('requesterInfo');
        var mnu = parent.document.getElementById('mnu');
        var main = parent.document.getElementById('main');
    </script>
</head>
<body id="fedauthbody">

    <form id="frmLogin" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <asp:Panel ID="divlogin" runat="server">
            <asp:Panel ID="pnlHeader" runat="server">
                <asp:Label ID="lblIdentification" runat="server">Identification</asp:Label>
                <asp:Label ID="lblSpheresVersion" runat="server">Version</asp:Label>
                <asp:Timer ID="timerReset" runat="server" OnTick="OnReset" />
            </asp:Panel>
            <asp:Panel ID="divbody" CssClass="fedauthopacity" runat="server">
                <asp:Label ID="lblLoginMsg" runat="server"></asp:Label>
                <asp:Panel ID="divErrLockout" runat="server">
                    <asp:Label ID="lblLockoutMsg" runat="server"></asp:Label>
                </asp:Panel>
                <asp:Label ID="lblIdentifier" runat="server">Identifiant</asp:Label>
                <asp:TextBox ID="txtCollaborator" runat="server" CssClass="txtCapture"></asp:TextBox>
                <asp:Label ID="lblPassword" runat="server">Password</asp:Label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="txtCapture" TextMode="Password"></asp:TextBox>
                <asp:CheckBox ID="chkAutoLogin" runat="server" Text="Remember" AutoPostBack="false" OnCheckedChanged="ChkAutoLogin_CheckedChanged"></asp:CheckBox>
                <efsc:WCToolTipLinkButton ID="btnLogin" runat="server" CssClass="fa-icon" OnClick="BtnLogin_Click"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="btnCaptcha" runat="server" OnClick="BtnCaptcha_Click"></efsc:WCToolTipLinkButton>
                <div class="hr"></div>
                <asp:LinkButton ID="btnDetail" runat="server" CssClass="fa-icon" OnClick="BtnDetail_Click">DataBase</asp:LinkButton>
                <asp:PlaceHolder ID="plhRDBMS" runat="server" Visible="False">
                    <asp:Label ID="lblRdbms" runat="server">RDBMS</asp:Label>
                    <asp:DropDownList ID="ddlRdbmsName" runat="server" CssClass="ddlCapture" Width="100%"></asp:DropDownList>
                    <asp:Label ID="lblServer" runat="server">Server</asp:Label>
                    <asp:TextBox ID="txtServerName" runat="server" CssClass="txtCapture" Width="100%"></asp:TextBox>
                    <asp:Label ID="lblDatabase" runat="server">DataBase</asp:Label>
                    <asp:TextBox ID="txtDatabaseName" runat="server" CssClass="txtCapture" Width="100%"></asp:TextBox>
                </asp:PlaceHolder>
            </asp:Panel>
        </asp:Panel>
    </form>
    <script type="text/javascript">
        if (parent.frames.length > 1) {
            var ctrl = document.forms[0].elements["__SOFTWARE"];
            if (ctrl != null) {
                if (typeof ($) != "undefined") {
                    var iFrameBoardSrc;
                    if (null != parent.$("#boardInfo")) {
                        iFrameBoardSrc = $(parent.$("#boardInfo")).attr("src");
                        if ((null == iFrameBoardSrc) || ("about:blank" != iFrameBoardSrc))
                            $(parent.$("#boardInfo")).appendTo(parent.$("#viewTrackerFrame")).attr("src", "about:blank");

                        //$('#boardInfo').load('Tracker.aspx #tracker', function(){});
                    }

                    if (null != parent.$("#summaryFrame")) {
                        var summary = parent.$("#summaryFrame").layout();
                        if (null != summary)
                            summary.hide("south");
                    }
                    //2009-09-22 PL Add Following if()
                    if ((this.name != "mnu")) {
                        //if ((this.name != "main"))
                        //    alert('Your session has expired. Please reconnect.\r\nVotre session a expiré. Veuillez vous reconnecter.\r\nSu sesión ha caducado. Por favor, vuelva a conectar.\r\nLa vostra sessione è scaduta. Vogliate riconnettervi.');
                        if (null != parent.$("#mnu"))
                            $(parent.$("#mnu")).appendTo(parent.$("#menuFrame")).attr("src", "sommaire.aspx");
                    }
                    if (typeof ($) != "undefined") {
                        if (null != parent.$("#main"))
                            $(parent.$("#main")).appendTo(parent.$("#mainFrame")).attr("src", "Welcome.aspx");
                    }
                    if (typeof ($) != "undefined") {
                        if (null != parent.$("#banner"))
                            $(parent.$("#banner")).appendTo(parent.$("#bannerFrame")).attr("src", "banner.aspx");
                    }
                }
            }
        }

        function stopResetTimer() {
            var timer = $find("timerReset");
            timer._stopTimer();
        }
    </script>

</body>
</html>
