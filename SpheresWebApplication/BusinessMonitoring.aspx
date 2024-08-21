<%@ Page Language="C#" AutoEventWireup="true" Inherits="EFS.Spheres.BusinessMonitoringPage" Codebehind="BusinessMonitoring.aspx.cs" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="refresh" content="60" />
    <title id="titlePage" runat="server"></title>
    <style type="text/css">
        .style1
        {
            width: 277px;
        }
    </style>
</head>
<body id="BodyID" runat="server">
    <form id="frmBusinessMonitoring" method="post" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server" />
    <asp:PlaceHolder ID="plhHeader" runat="server" />
    <asp:PlaceHolder ID="pnlParameters" runat="server" />
    <asp:Panel ID="pnlLineSeparator" runat="server">
        <table style="width: 100%;" border="0">
            <tr style="height: 15px;">
                <td colspan="2" class="fixedText" style="width: 99%;">
                    <div class="hr" style="width: 100%; height: 2px;" />
                </td>
            </tr>
            <tr>
                <td style="white-space: nowrap; height: 100%; padding: 5px;vertical-align:bottom;text-align:left;" />
            </tr>
        </table>
    </asp:Panel>
    <%--    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
        <ContentTemplate>--%>
    <asp:Panel runat="server" ID="pnlInputs" Style="margin: 2px; height: 96%; min-width: 870px">
        <div style="margin: 2px 2px;">
            <table class="TABLE" border="0">
                <tr>
                    <td>
                        <div class="headh">
                            <span class="size3">
                                <efsc:WCTooltipLabel runat="server" Text="Importazioni" />
                            </span>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:PlaceHolder ID="gvInputs" runat="server" />
                    </td>
                </tr>
                <tr style="padding: 5px">
                    <td style="width: 100%;vertical-align:top;text-align:left;">
                        <asp:Table ID="TblInfos" runat="server" CellPadding="0" Style="border: 10px; width: 100%;">
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell></asp:TableHeaderRow>
                        </asp:Table>
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>
    <asp:Panel runat="server" ID="pnlOutputs" Style="margin: 2px; height: 96%; min-width: 870px">
        <div style="margin: 2px 2px;">
            <table class="TABLE" border="0">
                <tr>
                    <td>
                        <div class="headh">
                            <span class="size3">
                                <efsc:WCTooltipLabel runat="server" Text="Esportazioni" />
                            </span>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:PlaceHolder ID="gvOutputs" runat="server" />
                    </td>
                </tr>
                <tr style="padding: 5px">
                    <td style="width: 100%;vertical-align:top;text-align:left;">
                        <asp:Table ID="Table4" runat="server" CellPadding="0" Style="border: 10px; width: 100%;">
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell></asp:TableHeaderRow>
                        </asp:Table>
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>
    <asp:Panel runat="server" ID="pnlClearingParameters" Style="margin: 2px; height: 96%;
        min-width: 870px">
        <div style="margin: 2px 2px;">
            <table class="TABLE" border="0">
                <tr>
                    <td>
                        <div class="headh">
                            <span class="size3">
                                <efsc:WCTooltipLabel runat="server" Text="Importazione delle quotazioni di fine giornata" />
                            </span>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:PlaceHolder ID="gvClearingParameters" runat="server" />
                    </td>
                </tr>
                <tr style="padding: 5px">
                    <td style="width: 100%;vertical-align:top;text-align:left;">
                        <asp:Table ID="Table1" runat="server" CellPadding="0" Style="border: 10px; width: 100%;">
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell></asp:TableHeaderRow>
                        </asp:Table>
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>
    <asp:Panel runat="server" ID="pnlEndOfDay" Style="margin: 2px; height: 96%; min-width: 870px">
        <div style="margin: 2px 2px;">
            <table class="TABLE" border="0">
                <tr>
                    <td>
                    <div class="headh">
                        <span class="size3">
                            <efsc:WCTooltipLabel runat="server" Text="Trattamento di fine giornata" />
                        </span>
                    </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:PlaceHolder ID="gvEndOfDay" runat="server" />
                    </td>
                </tr>
                <tr style="padding: 5px">
                    <td style="width:100%;vertical-align:top;text-align:left;">
                        <asp:Table ID="Table2" runat="server" CellPadding="0" Style="border: 10px; width: 100%;">
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell></asp:TableHeaderRow>
                        </asp:Table>
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>
    <asp:Panel runat="server" ID="CashBalance" Style="margin: 2px; height: 96%; min-width: 870px">
        <div style="margin: 2px 2px;">
            <table class="TABLE" border="0">
                <tr>
                    <td>
                        <div class="headh">
                            <span class="size3">
                                <efsc:WCTooltipLabel runat="server" Text="Trattamento garanzie/saldi" />
                            </span>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:PlaceHolder ID="gvCashBalance" runat="server" />
                    </td>
                </tr>
                <tr style="padding: 5px">
                    <td style="width: 100%;vertical-align:top;text-align:left;">
                        <asp:Table ID="Table3" runat="server" CellPadding="0" Style="border: 10px; width: 100%;">
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell></asp:TableHeaderRow>
                        </asp:Table>
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>
    <%-- </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="imgBtnRefresh1" EventName="SelectedIndexChanged" />
        </Triggers>
    </asp:UpdatePanel>--%>
    </form>
</body>
</html>
<%--<asp:Panel ID="pnlReload" runat="server">
            <table cellspacing="0" cellpadding="0" style="width: 100%;" border="0">
                <tr style="height: 15px;">
                    <td colspan="2" class="fixedText" style="width: 99%;">
                        <div class="hr" style="width: 100%; height: 2px;" />
                    </td>
                </tr>
                <tr>
                    <td valign="bottom" align="left" style="white-space: nowrap; height: 100%; padding: 5px;">
                        <div id="reload" style="padding: 4px;">
                            <efsc:WCToolTipImageButton ID="WCToolTipImageButton1" runat="server" ImageUrl="Images/PNG/Reload.png" />
                        </div>
                    </td>
                </tr>
            </table>
        </asp:Panel>--%>