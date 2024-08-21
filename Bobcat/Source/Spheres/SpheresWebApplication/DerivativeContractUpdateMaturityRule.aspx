<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DerivativeContractUpdateMaturityRule.aspx.cs" Inherits="EFS.Spheres.DerivativeContractMaturityRuleUpdate" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="HeadDerivativeContractUpdateMaturityRule" runat="server">
    <title id="titlePage" runat="server"></title>
</head>
<body>
    <form id="formMRUpdate" method="post" runat="server">
        <asp:PlaceHolder ID="plhHeader" runat="server"/>
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <div id="frmContainer">
            <asp:Panel ID="divalltoolbar" runat="server">
                <efsc:WCToolTipLinkButton ID="btnRecord" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
                <efsc:WCToolTipLinkButton ID="btnCancel" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
            </asp:Panel>
        </div>
        <asp:Panel ID="divbody" runat="server">
            <asp:Panel runat="server" ID="pnlCharacteristicsGen">
                <div class="headh">
                    <h2 runat="server">
                        <efsc:WCTooltipLabel runat="server" ID="lblCharacteristics" />
                    </h2>
                    <span id="CRPState" style="float: left;"></span>
                </div>
                <div class="contenth">
                    <div class="container">
						<div class="container-child">
							<efsc:WCTooltipLabel ID="lblIdentifier" runat="server" />
							<efsc:WCTextBox ID="txtIdentifier" runat="server" CssClass="txtCapture" ReadOnly="true" disabled="disabled"/>
						</div>
						<div class="container-child">
							<efsc:WCTooltipLabel ID="lblMarket" runat="server" />
							<efsc:WCTextBox ID="txtMarket" runat="server" CssClass="txtCapture"  ReadOnly="true"  disabled="disabled"/>
						</div>	
					</div>
					<div class="container">
						<div class="container-child">
							<efsc:WCTooltipLabel ID="lblDISPLAYNAME" runat="server" />
							<efsc:WCTextBox ID="txtDisplayName" runat="server" CssClass="txtCapture"  disabled="disabled" />
						</div>
					</div>
					<div class="container">
						<div class="container-child">
							<efsc:WCTooltipLabel ID="lblDESCRIPTION" runat="server" />
							<efsc:WCTextBox ID="txtDescription" runat="server" CssClass="txtCapture"  disabled="disabled" />
						</div>
					</div>
					<asp:Panel ID="divMainMR" runat="server" CssClass="container" >
						<div class="container-child">
							<efsc:WCTooltipLabel ID="lblMainMR" runat="server" />
							<efsc:WCTextBox ID="txtMainMR" runat="server" CssClass="txtCapture" disabled="disabled"  />
						</div>
					</asp:Panel>
					<div class="container">
						<div class="container-child">
							<efsc:WCTooltipLabel ID="lblCurrentMR" runat="server" />
							<efsc:WCTextBox ID="txtCurrentMR" runat="server" CssClass="txtCapture" disabled="disabled" />
						</div>					
						<div class="container-child">
							<efsc:WCTooltipLabel ID="lblNewMR" runat="server"/>
							<efsc:WCDropDownList2 ID="ddlNewMR" runat="server" CssClass="ddlCapture"  />
						</div>
					</div>
                </div>
            </asp:Panel>
        </asp:Panel>
    </form>
</body>
</html>
