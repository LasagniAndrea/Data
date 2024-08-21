<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CustomerPortal.aspx.cs" Inherits="EFS.Spheres.CustomerPortal" %>

<!DOCTYPE html>
<html lang="fr">
<head runat="server">
    <meta charset="utf-8" />
    <title>Main Page</title>

    <link href="https://fonts.googleapis.com/css?family=Poppins:100,400,300,700,100italic,300italic|Oswald:200,300,400,500,600,700|Raleway:100,200,300,400,500,600,700,100italic,300italic" rel="stylesheet" type="text/css">
    <link rel="preconnect" href="https://fonts.gstatic.com">
    <link href="https://fonts.googleapis.com/css2?family=Allerta+Stencil&family=Josefin+Sans:wght@600&family=Nova+Square&family=Zen+Dots&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="Includes/fontawesome-all.min.css" />
    <link rel="stylesheet" href="CustomerPortal/Content/Common.min.css" />
    <link rel="stylesheet" href="CustomerPortal/Content/CustomerPortal.min.css" />

</head>
<body>
    <form id="frmCustomerPortal" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <input id="hidIsConnected" type="hidden" name="hidIsConnected" runat="server" />
        <input id="hidIDA" type="hidden" name="hidIDA" runat="server" />
        <input id="hidMaskMenu" style="width: 55px; height: 22px" type="hidden" name="hidMaskMenu" runat="server" />

        <header id="banner">
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
                <div class="banner title">
                    <div>
                        EURO FINANCE SYSTEMS | <small>Provider of Capitals Markets solutions</small>
                    </div>
                </div>

                <div class="banner mnu">
                    <asp:LinkButton CssClass="mnu" runat="server" ID="lnkHome">Home</asp:LinkButton><small> | </small>
                    <asp:LinkButton CssClass="mnu" runat="server" ID="lnkHelp"><i class="fa fa-question"></i></asp:LinkButton><small> | </small>
                      <div id="user-box" class="mnu dropdown">
                        <button onclick="return false;" runat="server" id="dduser" class="dropbtn"></button>
                        <div class="dropdown-content">
                          <asp:LinkButton CssClass="mnu" runat="server" ID="lnkProfile"></asp:LinkButton>
                          <asp:LinkButton CssClass="mnu" runat="server" ID="lnkDisconnect" OnClientClick="HideMenu();"></asp:LinkButton>
                        </div>
                      </div>
                </div>

            </div>
            <div>
                <div id="infologin" class="user">
                    <small><asp:Label ID="lastLogin" runat="server"></asp:Label></small>
                </div>
            </div>
        </header>
        <section id="pg-body">
            <section id="summary" runat="server">
                <div id="ShowMnu">
                    <div id="TblLoginLogout" runat="server">
                        <efsc:WCToolTipLinkButton ID="btnMaskMenu" runat="server" CssClass="fa-icon" Text="<i class='fa fa-filter'></i>"></efsc:WCToolTipLinkButton>
                        <efsc:WCToolTipLinkButton ID="btnSlideMenu" runat="server" CssClass="fa-icon" Text="<i class='fa fa-angle-double-left'></i>"></efsc:WCToolTipLinkButton>
                    </div>
                    <div id="TblShowMenu">
                        <div id="divMnu" style="overflow: auto; position: relative;">
                            <asp:PlaceHolder ID="phMenu" runat="server"></asp:PlaceHolder>
                        </div>
                    </div>
                </div>
            </section>
            <section id="pg-main">
                <iframe id="main" title="Main" name="main" src="CustomerPortal/Page/Welcome.aspx"></iframe>
            </section>
        </section>
        <footer>
            Powered by Euro Finance Systems | Copyright <i class="fas fa-copyright"></i>2024
        </footer>

        <script type="text/javascript">
            // [25556] Gestion de la touche Control sur lien hyperlink du Menu - Ouverture sur main ou _blank - solution de contournement pour le self.Close
            var cntrlIsPressed = false;

            $(document).on("keydown", function (event) {
                if (event.ctrlKey)
                    cntrlIsPressed = true;
            });

            $(document).on("keyup", function () {
                cntrlIsPressed = false;
            });

            function OM(evt, id) {
                if (cntrlIsPressed) {
                    _target = "_blank";
                }
                else {

                    var _target = "main";
                    var mainPage = $('#main')[0];
                    if (null == mainPage)
                        mainPage = document.getElementById('main');
                    if (null != mainPage) {
                        if (mainPage.contentWindow.cntrlIsPressed) {
                            _target = "_blank";
                            mainPage.contentWindow.cntrlIsPressed = false;
                        }
                    }
                }
                window.open("hyperlink.aspx?mnu=" + id, _target);
            }
        </script>
    </form>
</body>
</html>
