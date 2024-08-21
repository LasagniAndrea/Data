<%@ Register TagPrefix="EFS" Namespace="EFS.Controls" Assembly="EFS.WebControlLibrary" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<%@ Page Async="true"  Language="c#" Inherits="EFS.Spheres.LstReferentialPage" Codebehind="List.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title runat="server" id="titlePage"></title>
</head>
<body id="BodyID" runat="server">
    <form id="frmConsult" method="post" runat="server">
    <asp:Timer ID="timerRefresh" runat="server"  OnTick="OnTimerRefresh"/> 
    <input type="hidden" runat="server" id="exportToken_value"/>
    <asp:ScriptManager ID="ScriptManager" runat="server" />
    <div id="divHeader" runat="server">
        <asp:PlaceHolder ID="plhHeader" runat="server"></asp:PlaceHolder>
    </div>
    <div id="divalltoolbar" class="vdark" runat="server">
        <div id="tblist" runat="server" >
            <div runat="server" id="divBtn">
                <asp:PlaceHolder ID="plhClose" runat="server"></asp:PlaceHolder>
                <efsc:WCToolTipLinkButton ID="imgRefresh" CssClass="fa-icon" runat="server" Text=" <i class='fas fa-sync-alt'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgPrintAll" CssClass="fa-icon" runat="server" Text="<i class='fas fa-print'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgMSExcel" CssClass="fa-icon" runat="server" Text="<i class='fas fa-file-excel'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgCSV" CssClass="fa-icon" runat="server" Text="<i class='fas fa-file-csv'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgSQL" CssClass="fa-icon" runat="server" Text="<i class='fas fa-file-export'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgZIP" CssClass="fa-icon" runat="server" Text="<i class='fas fa-file-archive'></i>"></efsc:WCToolTipLinkButton>
            </div>
            <div id="divtoolbar" runat="server">
                <efsc:WCToolTipLinkButton ID="imgRunProcess" CssClass="fa-icon" runat="server" Text="<i class='fa fa-caret-square-right'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgRunProcessIO" CssClass="fa-icon" runat="server" Text="<i class='fa fa-caret-square-right'></'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgRunProcessAndIO" CssClass="fa-icon" runat="server" Text="<i class='fa fa-caret-square-right'></'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgAddNew" CssClass="fa-icon" runat="server" Text="<i class='fa fa-plus-circle'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgRecord" CssClass="fa-icon" runat="server" OnClick="OnRecord_Click" Text="<i class='fa fa-save'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgCancel" CssClass="fa-icon" runat="server" OnClick="OnCancel_Click" Text="<i class='fa fa-times-circle'></i>"></efsc:WCToolTipLinkButton>
            </div>
            <div id="tbPDF" runat="server"/>
            <div id="tbViewer" runat="server">
                <asp:Label ID="lblViewerModel" runat="server"></asp:Label>
                <efsc:WCToolTipLinkButton ID="imgViewerOpen" CssClass="fa-icon" runat="server" Text="<i class='fa fa-folder-open'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgViewerSave" CssClass="fa-icon" runat="server" Text="<i class='fa fa-save'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgViewerDisplay" CssClass="fa-icon" runat="server" Text="<i class='fas fa-columns'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgViewerCriteria" CssClass="fa-icon" runat="server" Text="<i class='fa fa-filter'></i>"></efsc:WCToolTipLinkButton>
                <efsc:WCToolTipLinkButton ID="imgViewerSort" CssClass="fa-icon" runat="server" Text="<i class='fas fa-sort-amount-down'></i>"></efsc:WCToolTipLinkButton>
            </div>
            <div id="tbCheck" runat="server">
                <efsc:WCDropDownList2 ID="ddlValidityData" runat="server" />
            </div>
            <div id="tbSearch" runat="server"/>
            <asp:Panel ID="tbFiller" runat="server"/>
            <div id="tbTrace"  runat="server">
                <div>
                    <efsc:WCCheckBox2 ID="chkSQLTrace" runat="server" Text="TraceSql"/>
                </div>
            </div>
        </div>
    </div>

    <asp:Panel ID="InformationSummary" CssClass="InformationSummary" runat="server">
        <efsc:WCToolTipLinkButton CssClass="fa-icon" runat="server" Text="<i class='fa fa-info'></i>"></efsc:WCToolTipLinkButton>
        <asp:Literal ID="litMsgInformation" runat="server" />
    </asp:Panel>
    <asp:Panel ID="WarningSummary" CssClass="WarningSummary" runat="server">
        <efsc:WCToolTipLinkButton CssClass="fa-icon" runat="server" Text="<i class='fa fa-exclamation-triangle'></i>"></efsc:WCToolTipLinkButton>
        <asp:Literal ID="litMsgWarning" runat="server" />
    </asp:Panel>
    <asp:Panel ID="ErrorSummary" CssClass="ErrorSummary" runat="server">
        <efsc:WCToolTipLinkButton CssClass="fa-icon" runat="server" Text="<i class='fa fa-exclamation-circle'></i>"></efsc:WCToolTipLinkButton>
        <asp:Literal ID="litMsgErr" runat="server" />
    </asp:Panel>
    <div id="divbody" class="vdark" runat="server">
        <asp:Panel ID="divInfos" runat="server"></asp:Panel>
        <asp:PlaceHolder ID="plhParamProcess" runat="server"></asp:PlaceHolder>
        <asp:Panel id="divDtg" runat="server">
            <EFS:TemplateDataGrid ID="efsdtgRefeferentiel" runat="server" AutoGenerateColumns="false"></EFS:TemplateDataGrid>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        $(window).on ("load", function() {
            $.unblockUI();
        });

        var fileExportCheckTimer;
        function BlockUIForExport(message, exportType, objectName) {
            var token = new Date().getTime();
            $('#exportToken_value').val(token);
            $.blockUI(message);
            fileExportCheckTimer = window.setInterval(function () {
                var cookieValue = Cookies.get(exportType + 'fileToken');
                if (cookieValue == token)
                    finishDownload(exportType);
            }, 1000);
        }


        function finishDownload(exportType) {
            window.clearInterval(fileExportCheckTimer);
            Cookies.remove(exportType + 'fileToken'); //clears this cookie value
            $.unblockUI();
        }
    </script>
    </form>
    
</body>
</html>
