<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>

<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="EFS.Spheres.Default" %>

<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link id="linkCssDefault" href="Includes/Default.min.css" rel="stylesheet" type="text/css" />
    <link id="linkCssTracker" href="Includes/EntityMarket.min.css" rel="stylesheet" type="text/css" />
</asp:Content>

<%--Banniere partie centrale--%>
<asp:Content ID="Content2" ContentPlaceHolderID="cphContentInfo" runat="server">
    <div id="divcompany">
        <asp:Label ID="lblCompanyDisplayname" runat="server"></asp:Label>
        <asp:Label ID="lblCompanyDescription" runat="server"></asp:Label>
    </div>
    <div id="divguititle">
        <asp:Label ID="lblArchive" runat="server" CssClass="labelGUITitle"></asp:Label>
        <asp:Label ID="lblGUITitle" runat="server" CssClass="labelGUITitle"></asp:Label>
    </div>

    <div id="diventitymarket">
        <asp:HiddenField ID="__ISSCROLL" runat="server" Value="true" />
        <asp:PlaceHolder runat="server" ID="plhEntityMarket">
        </asp:PlaceHolder>
    </div>
    <div id="divcs">
        <efs:WCTooltipLabel ID="lblConnectionString" runat="server" CssClass="ConnectionString" BackColor="Transparent">ConnectionString</efs:WCTooltipLabel>
    </div>
</asp:Content>

<%--Bannière menu--%>
<asp:Content ID="Content4" ContentPlaceHolderID="cphContentMenu" runat="server">
    <div id="user-box" class="mnu dropdown">
        <div id="imgLogin">
            <span id="imgstart" runat="server"></span>
            <span id="imgDisconnect" runat="server"></span>
        </div>
        <div>
            <efs:WCToolTipLinkButton ID="btnUserName" runat="server" CssClass="fa-icon" />
            <span id="imgend" runat="server"></span>
        </div>
    </div>
    <div>
    </div>
    <div id="user-box-content">
        <div class="outer-arrow-up theme-up-outer-arrow">
            <div class="inner-arrow-up theme-up-inner-arrow"></div>
        </div>
        <asp:LinkButton CssClass="mnu" runat="server" ID="lnkHome"></asp:LinkButton>
        <asp:LinkButton CssClass="mnu" runat="server" ID="lnkProfile"></asp:LinkButton>
        <asp:LinkButton CssClass="mnu" runat="server" ID="lnkHelp"></asp:LinkButton>
        <asp:LinkButton CssClass="mnu" runat="server" ID="lnkDisconnect"></asp:LinkButton>
        <asp:Label ID="lblLastLogin" runat="server">LastConnection</asp:Label>
    </div>
</asp:Content>

<%--Body : Sommaire , Tracker et Main --%>
<asp:Content ID="Content5" ContentPlaceHolderID="cphContentPage" runat="server">
    <input id="hidIsPortal" type="hidden" name="hidIsPortal" runat="server" />
    <input id="hidIsConnected" type="hidden" name="hidIsConnected" runat="server" />
    <input id="hidIDA" type="hidden" name="hidIDA" runat="server" />
    <input id="hidMaskMenu" style="width: 55px; height: 22px" type="hidden" name="hidMaskMenu" runat="server" />

    <div id="contnr">
        <section id="tbsummary">
            <a onclick="_SwitchMenuToggle();return false;" id="btnMaskMnu" class="fa-icon" href="#btnMaskMnu">
                <i class="fa fa-filter" title="Afficher le menu allégé"></i>
            </a>
            <a onclick="_SlideContnr(this);return false;" id="btnSlidesummary" class="fa-icon" title="Masquer/Afficher le menu" href="#btnSlidesummary">
                <i class="fa fa-angle-double-left"></i>
            </a>
        </section>
        <section id="summary">
        </section>
        <section id="tbtracker">
            <input id="hidMaskGroup" style="width: 55px; height: 22px" type="hidden" name="hidGMaskroup" runat="server" />
            <input id="hidHisto" style="width: 55px; height: 22px" type="hidden" name="hidHisto" runat="server" />

            <a id="btnDetail" class="fa-icon" runat="server" href="#btnDetail"><i class="fas fa-search"></i></a>
            <a onclick="_Refresh();return false;" id="btnRefresh" class="fa-icon" runat="server" href="#btnRefresh"><i class="fa fa-sync-alt"></i></a>
            <a id="btnAutoRefresh" class="fa-icon" runat="server" href="#btnAutoRefresh"><i class='fas fa-timer'></i></a>
            <a onclick="_OpenTrackerParam();return false;" id="btnParam" runat="server" class="fa-icon" href="#btnParam"><i class="fab fa-process"></i></a>
            <a onclick="_OpenTrackerObserver();return false;" id="btnObserver" class="fa-icon" runat="server" href="#btnObserver"><i class="fab fa-process"></i><i class='fas fa-process-help'></i></a>
            <a onclick="_OpenMonitoring();return false;" id="btnMonitoring" class="fa-icon" runat="server" href="#btnMonitoring"><i class="fas fa-list-alt"></i></a>
            <a onclick="_SwitchTrackerGroupFavorite();return false;" id="btnMaskGroup" class="fa-icon" runat="server" href="#btnMaskGroup"><i class="fas fa-star"></i></a>
            <a onclick="_SlideContnr(this);return false;" id="btnSlidetracker" class="fa-icon" title="Masquer/Afficher le tracker" href="#btnSlidetracker"><i class="fa fa-angle-double-left"></i></a>

        </section>
        <section id="tracker"></section>
        <section id="mainframe">
            <iframe id="main" title="Main" name="main" src="Welcome.aspx"></iframe>
        </section>
    </div>

    <script type="text/javascript" src="./Javascript/Default.min.js "></script>
    <script type="text/javascript" src="./Javascript/JQuery/jquery.infiniteslidev2.min.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            _LoadData();
        });
    </script>


</asp:Content>
