<%@ Page Language="C#" AutoEventWireup="true" Inherits="EFS.Spheres.BusinessMonitoring2Page" Codebehind="BusinessMonitoring2.aspx.cs" %>
<%@ Register TagPrefix="EFS" Namespace="EFS.Controls" Assembly="EFS.WebControlLibrary" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title id="titlePage" runat="server"></title>
</head>
<body class="Monitoring">
    <form id="frmBusinessMonitoring" method="post" runat="server">
    <asp:PlaceHolder ID="plhHeader" runat="server"></asp:PlaceHolder>
    <asp:ScriptManager ID="ScriptManager" runat="server" />
    <div id="businessMonitoringContainer">
        <asp:Timer ID="timerRefresh" runat="server" OnTick="OnRefresh" />

        <script type="text/javascript" src="./Javascript/Monitoring.min.js"></script>

        <asp:Panel ID="divalltoolbar" runat="server">
            <table style="width:100%">
                <tr>
                    <td>
                        <efsc:WCToolTipLinkButton ID="btntbrefresh" CssClass="fa-icon" runat="server" OnCommand="OnAction" Text=" <i class='fas fa-sync-alt'></i>"></efsc:WCToolTipLinkButton>
                        <efsc:WCToolTipLinkButton ID="btntbautorefresh" CssClass="fa-icon" runat="server" OnCommand="OnAction" Text=" <i class='fas fa-timer'></i>"></efsc:WCToolTipLinkButton>
                        <efsc:WCToolTipLinkButton ID="btntbparam" CssClass="fa-icon" runat="server" OnClientClick="MonitoringParam();return false;" Text=" <i class='fab fa-process'></i>"></efsc:WCToolTipLinkButton>
                    </td>
                    <td style="width: 20px">
                        &nbsp;
                    </td>
                    <td>
                        <efsc:WCTooltipLabel runat="server" ID="lblBMEntity" Style="vertical-align: middle;" />
                        <efsc:WCDropDownList2 runat="server" ID="ddlBMEntity" OnSelectedIndexChanged="OnEntityChanged"
                            AutoPostBack="true" Width="250px" />
                    </td>
                    <td>
                        <efsc:WCTooltipLabel runat="server" ID="lblBMCssCustodian" Style="vertical-align: middle;" />
                        <efsc:WCDropDownList2 CssClass="ddlJQOptionGroup" runat="server" ID="ddlBMCssCustodian" AutoPostBack="true" Width="400px" />
                    </td>
                    <td style="width:100%">
                        <efsc:WCCheckBox2 runat="server" ID="chkovf_EnLargeAll" AutoPostBack="true" OnCheckedChanged="OnCheckOverFlowChanged" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </div>

    <div id="divbody" runat="server">
    <!-- ENTITYMARKET -->
    <asp:Panel runat="server" ID="pnlBMEntityMarket">
        <div class="headh" >
            <efsc:WCCheckBox2 runat="server" ID="chkovf_EntityMarket" AutoPostBack="true" OnCheckedChanged="OnCheckOverFlowChanged" />
            <h2 id="H1" runat="server">
                <efsc:WCTooltipLabel runat="server" ID="lblBMEntityMarket" />
            </h2>
        </div>
        <div class="contenth">
            <div style="height: 100%">
                <div id="divDtg"  style="z-index: 6" class="admin">
                    <asp:PlaceHolder ID="plhBMEntityMarket" runat="server" />
                </div>
            </div>
        </div>
    </asp:Panel>    
    
    <asp:Panel runat="server" ID="pnlBMResults">
        <asp:Panel runat="server" ID="pnlBMIO" Style="margin: 5px 2px 2px 2px;">
            <div class="headh">
                <efsc:WCCheckBox2 runat="server" ID="chkovf_IO" AutoPostBack="true" OnCheckedChanged="OnCheckOverFlowChanged" />
                <h2 runat="server">
                    <efsc:WCTooltipLabel runat="server" ID="lblBMIO" />
                </h2>
            </div>
            <div class="contenth">
                <div style="height: 100%">
                    <div id="divDtgIO" style="z-index: 6" class="views">
                        <asp:PlaceHolder ID="plhBMIO" runat="server" />
                    </div>
                </div>
            </div>
        </asp:Panel>
        
        <asp:Panel runat="server" ID="pnlBMQuote" Style="margin: 5px 2px 2px 2px;">
            <div class="headh">
                <efsc:WCCheckBox2 runat="server" ID="chkovf_Quote" AutoPostBack="true" OnCheckedChanged="OnCheckOverFlowChanged" />
                <h2 runat="server">
                    <efsc:WCTooltipLabel runat="server" ID="lblBMQuote" />
                </h2>
            </div>
            <div class="contenth">
                <div style="height: 100%">
                    <div id="divDtgQuote" style="z-index: 6" class="about">
                        <asp:PlaceHolder ID="plhBMQuote" runat="server" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel runat="server" ID="pnlBMEOD" Style="margin: 5px 2px 2px 2px;">
            <div class="headh">
                <efsc:WCCheckBox2 runat="server" ID="chkovf_EOD" AutoPostBack="true" OnCheckedChanged="OnCheckOverFlowChanged" />
                <h2 runat="server">
                    <efsc:WCTooltipLabel runat="server" ID="lblBMEOD" />
                </h2>
            </div>
            <div class="contenth">
                <div style="height: 100%">
                    <div id="divDtgEOD" style="z-index: 6" class="process">
                        <asp:PlaceHolder ID="plhBMEOD" runat="server" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel runat="server" ID="pnlBMCashBalance" Style="margin: 5px 2px 2px 2px;" CssClass="violet">
            <div class="headh">
                <efsc:WCCheckBox2 runat="server" ID="chkovf_CashBalance" AutoPostBack="true" OnCheckedChanged="OnCheckOverFlowChanged" />
                <h2 runat="server">
                    <efsc:WCTooltipLabel runat="server" ID="lblBMCashBalance" />
                </h2>
            </div>
            <div class="contenth">
                <div style="height: 100%">
                    <div id="divDtgCB" style="z-index: 6" class="process">
                        <asp:PlaceHolder ID="plhBMCashBalance" runat="server" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </asp:Panel>   
        <asp:Panel ID="Panel1" style="padding-right:10px;" runat="server" >
            <efsc:WCTooltipLabel ID="lblLastUpdate" runat="server" />    
        </asp:Panel>
    </div>    
    </form>
</body>
</html>
