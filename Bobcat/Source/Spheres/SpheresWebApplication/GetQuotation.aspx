<%@ Page Language="c#" Inherits="EFS.Spheres.GetQuotationPage" Codebehind="GetQuotation.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>GetQuotation</title>
</head>
<body id="BodyID">
    <form id="frmQuotation" method="post" runat="server">
        <table id="Table0" border="0" style="font-family: Arial;">
            <tr>
                <td>
                    <table id="Table1" border="1" style="background: lavender;">
                        <tr>
                            <td colspan="2" style="text-align:center;font-weight: bold;color: darkblue;background: steelblue;">REQUEST</td>
                        </tr>
                        <tr style="background: lightsteelblue;">
                            <td colspan="2">Asset</td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;AssetType</td>
                            <td><asp:DropDownList ID="ddlAssetType" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;Asset_Identifier</td>
                            <td><asp:TextBox ID="txtAsset_Identifier" runat="server" Width="100%"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;IdAsset</td>
                            <td><asp:TextBox ID="txtIdAsset" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="background: lightsteelblue;">
                            <td colspan="2">Characteristics</td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;MarketEnv</td>
                            <td><asp:DropDownList ID="ddlIdMarketEnv" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;ValScenario</td>
                            <td><asp:DropDownList ID="ddlIdValScenario" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;Availability</td>
                            <td><asp:DropDownList ID="ddlAvailability" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;QuoteSide</td>
                            <td><asp:DropDownList ID="ddlQuoteSide" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;QuoteTiming</td>
                            <td><asp:DropDownList ID="ddlQuoteTiming" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;Time</td>
                            <td><asp:DropDownList ID="ddlTimeOperator" runat="server"></asp:DropDownList><asp:TextBox ID="txtTime" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;AssetMeasure</td>
                            <td><asp:DropDownList ID="ddlAssetMeasure" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;CashFlowType</td>
                            <td><asp:DropDownList ID="ddlCashFlowType" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr style="background: gainsboro;">
                            <td colspan="2">FxRate asset constituent</td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;Currency1</td>
                            <td><asp:DropDownList ID="ddlIDC1" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;Currency2</td>
                            <td><asp:DropDownList ID="ddlIDC2" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;QuoteBasis</td>
                            <td><asp:DropDownList ID="ddlQuoteBasis" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr style="background: gainsboro;">
                            <td colspan="2">RateIndex asset constituent</td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;RateIndex</td>
                            <td><asp:TextBox ID="txtRateIndex" runat="server" Width="100%"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;PeriodMultiplier</td>
                            <td><asp:TextBox ID="txtPeriodMultiplier" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;Period</td>
                            <td><asp:DropDownList ID="ddlPeriod" runat="server"></asp:DropDownList></td>
                        </tr>
                        <tr style="background: gainsboro;">
                            <td colspan="2">Quote ID</td>
                        </tr>
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;IdQuote</td>
                            <td><asp:TextBox ID="txtIdQuote" runat="server"></asp:TextBox></td>
                        </tr>
                    </table>
                </td>
                <td style="vertical-align:top">&nbsp;</td>
                <td style="vertical-align:top">
                    <table id="Table3" border="1" style="background: ghostwhite;">
                        <tr>
                            <td colspan="2" style="text-align:center;font-weight: bold;color: black;background: silver;">RESPONSE</td>
                        </tr>
                        <tr>
                            <td>Status</td>
                            <td><asp:TextBox ID="txtOutStatus" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>Source</td>
                            <td><asp:TextBox ID="txtOutQuoteSource" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>Market (BC)</td>
                            <td><asp:TextBox ID="txtOutMarket_ISO10383_ALPHA4" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>MarketEnv</td>
                            <td><asp:TextBox ID="txtOutIdMarketEnv" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>ValScenario</td>
                            <td><asp:TextBox ID="txtOutIdValScenario" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>Availability</td>
                            <td><asp:TextBox ID="txtOutAvailability" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>QuoteSide</td>
                            <td><asp:TextBox ID="txtOutQuoteSide" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>QuoteTiming</td>
                            <td><asp:TextBox ID="txtOutQuoteTiming" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>Time</td>
                            <td><asp:TextBox ID="txtOutTime" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>Value</td>
                            <td><asp:TextBox ID="txtOutValue" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>Unit</td>
                            <td><asp:TextBox ID="txtOutQuoteUnit" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>AssetMeasure</td>
                            <td><asp:TextBox ID="txtOutAssetMeasure" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>CashFlowType</td>
                            <td><asp:TextBox ID="txtOutCashFlowType" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>IdAsset</td>
                            <td><asp:TextBox ID="txtOutIdAsset" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>IdQuote</td>
                            <td><asp:TextBox ID="txtOutIdQuote" runat="server" ReadOnly="true"></asp:TextBox></td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Button ID="btnSearch" runat="server" Text="btnSearch" OnClick="BtnSearch_Click" Width="100%"></asp:Button>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
