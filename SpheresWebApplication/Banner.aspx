<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Banner.aspx.cs" Inherits="EFS.Spheres.Banner" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title id="titlePage" runat="server" />
</head>
<body id="BodyID" class="White">
    <form id="idBanner" runat="server">
        <input id="hidIsConnected" type="hidden" name="hidIsConnected" runat="server" />
        <input id="hidIDA" type="hidden" name="hidIDA" runat="server" />
    
        <div id="bannermain" style="width:100%">
            <!-- Gauche -->
            <div id="bl">
                <asp:Image ID="imgLogoCompany" runat="server" ImageUrl="Images/Logo_Entity/EuroFinanceSystems/TransparentLogoSmall_EuroFinanceSystems.gif"></asp:Image>
            </div>
            <!-- Centre c8e7fe -->
            <div id="bm" runat="server">
                <div id="bm_row1">
                    <div id="bm_company" runat="server">
                        <asp:Label ID="lblCompanyDisplayname" runat="server"></asp:Label>
                        <asp:Label ID="lblCompanyDescription" runat="server"></asp:Label>
                    </div>
                    <div id="bm_guititle">
                        <asp:Label ID="lblArchive" runat="server" CssClass="labelGUITitle"></asp:Label>
                        <asp:Label ID="lblGUITitle" runat="server" CssClass="labelGUITitle"></asp:Label>
                    </div>
                    <div id="bm_button">
                        <asp:LinkButton ID="btnCulture" runat="server" CssClass="fa-icon" Text="<i class='fas fa-flag'></i>" />
                        <asp:LinkButton ID="btnHelp" runat="server" CssClass="fa-icon" Text="<i class='fas fa-question-circle'></i>" />
                    </div>
                    <div id="bm_logosoftware">
                        <efs:WCToolTipImageButton TabIndex="-1" ID="imgLogoSoftware" runat="server" CausesValidation="False" />
                    </div>
                </div>
                <div id="bm_row2" runat="server">
                    <div id="bm_user">
                        <efs:WCToolTipLinkButton ID="btnUserName" runat="server" CssClass="fa-icon" />
                        <asp:Label ID="lblLastLogin" runat="server">LastConnection</asp:Label>                                            
                        <efs:WCToolTipLinkButton ID="imgRefresh" CssClass="fa-icon" runat="server" OnClientClick="RefreshEntityMarket();return false; "  Text=" <i class='fas fa-sync-alt'></i>"></efs:WCToolTipLinkButton>
                    </div>
                    <asp:Panel ID="bm_entitymarket" runat="server">
                        <iframe id="entityMarketFrame" name="entityMarketFrame" src="EntityMarket.aspx"></iframe>
                    </asp:Panel>
                    <div id="bm_cs">
                        <efs:WCTooltipLabel ID="lblConnectionString" runat="server" CssClass="ConnectionString" BackColor="Transparent">ConnectionString</efs:WCTooltipLabel>
                    </div>
                </div>
            </div>
            <!-- Droit -->
            <div class="br">
                <asp:ImageButton ID="imgBanner" runat="server" Height="55px" ImageUrl="Portal/Images/Gif/BannerPortal.gif" CausesValidation="false" Visible="false"></asp:ImageButton>
            </div>
        </div>

    <script type="text/javascript">
        if (document.forms[0].hidIsConnected.value == 'logout') {
            var mnu = parent.document.getElementById('mnu');
            if (null != mnu)
                mnu.src = "Sommaire.aspx?connectionstate=logout";
        }
    </script>
    <script type="text/javascript">
        function RefreshEntityMarket() {
            var em = document.getElementById('entityMarketFrame');
            if (null != em) {
                if (em.contentWindow)
                    em.contentWindow.location.reload(true);
                else if (em.contentDocument)
                    em.contentDocument.location.reload(true);
            }
        }
    </script>
    </form>
</body>
</html>

