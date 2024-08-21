<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>

<%@ Page Language="c#" Inherits="EFS.Spheres.SommairePage" CodeBehind="Sommaire.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title id="titlePage" runat="server" />
    <link id="linkCssSummary" href="Includes/Summary.min.css" rel="stylesheet" type="text/css" />
    <%--<base target="main" />--%>

    <script type="text/javascript">
        var banner = parent.document.getElementById("banner");
        var boardInfo = parent.document.getElementById('boardInfo');
        var mnu = parent.document.getElementById('mnu');
        var main = parent.document.getElementById('main');
    </script>

</head>
<body id="BodyID" class="EG-Sommaire" style="height: 100%;">
    <form id="Sommaire" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <div id="sommaireContainer">
            <input id="hidIDA" style="width: 55px; height: 22px" type="hidden" name="hidIDA"
                runat="server" />
            <input id="hidMaskMenu" style="width: 55px; height: 22px" type="hidden" name="hidMaskMenu"
                runat="server" />

            <script type="text/javascript">
                $(document).ready(function () {
                    var ctrl = document.forms[0].elements["__SOFTWARE"];
                    if (ctrl != null) {
                        if (ctrl.value == "Spheres" || ctrl.value == "OTCml" || ctrl.value == "Vision") {
                            if (typeof ($) != "undefined") {
                                ctrl = document.forms[0].elements["__ROLE"];
                                if (ctrl != null) {
                                    if (ctrl.value == "GUEST") {
                                        /* PL 2015-05-15 TEST */
                                        if (null != parent.$("#summaryFrame")) {
                                            var summary = parent.$("#summaryFrame").layout();
                                            if (null != summary) {
                                                summary.show("south", false);
                                                summary.removePane("south", true);
                                            }
                                        }
                                    }
                                    else {
                                        var iFrameBoardSrc;
                                        if (null != parent.$("#boardInfo")) {
                                            iFrameBoardSrc = $(parent.$("#boardInfo")).attr("src");
                                            if ((null == iFrameBoardSrc) || ("about:blank" == iFrameBoardSrc)) {
                                                $(parent.$("#boardInfo")).attr("src", "about:blank");
                                                $(parent.$("#boardInfo")).appendTo(parent.$("#viewTrackerFrame")).attr("src", "Tracker.aspx");
                                            }
                                        }

                                        if ((null != parent.$("#summaryFrame")) && (null != parent.$("#boardInfo"))) {
                                            var summary = parent.$("#summaryFrame").layout();
                                            if (null != summary) {
                                                summary.show("south", true);
                                            }
                                        }
                                        iFrameBoardSrc = null;
                                    }
                                }
                            }
                        }
                    }
                    if (null != parent.$("#main")) {
                        //var iFrameMainSrc = $(parent.$("#main")).attr("src");
                        //if ((null == iFrameMainSrc) || ("about:blank" == iFrameMainSrc))
                        $(parent.$("#main")).appendTo(parent.$("#mainFrame")).attr("src", "Welcome.aspx");
                    }
                    if (null != parent.$("#banner"))
                        $(parent.$("#banner")).appendTo(parent.$("#bannerFrame")).attr("src", "banner.aspx");
                });


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
                            mainPage = parent.document.getElementById('main');
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

            <div id="ShowMnu">
                <div id="TblLoginLogout" runat="server">
                    <efsc:WCToolTipLinkButton ID="btnLogout" CssClass="fa-icon" runat="server" OnClick="BtnLogout_Click" Text="<i class='fa fa-power-off'></i>"></efsc:WCToolTipLinkButton>
                    <efsc:WCToolTipButton ID="btnLogout2" runat="server" OnClick="BtnLogout_Click"></efsc:WCToolTipButton>
                    <efsc:WCToolTipLinkButton ID="btnMaskMenu" runat="server" CssClass="fa-icon" Text="<i class='fa fa-filter'></i>"></efsc:WCToolTipLinkButton>
                </div>
                <div id="TblShowMenu">
                    <div id="divMnu" style="overflow: auto; position: relative;">
                        <asp:PlaceHolder ID="phMenu" runat="server"></asp:PlaceHolder>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
