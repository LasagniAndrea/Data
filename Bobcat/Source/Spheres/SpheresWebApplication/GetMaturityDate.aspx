<%@ Page Language="c#" AutoEventWireup="true" Culture="en-US" Inherits="EFS.Spheres.GetMaturityDate" Codebehind="GetMaturityDate.aspx.cs" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Maturities Calculator</title>
</head>
<body style="background: #EEEEEE; font-family: Calibri;">
    <div style="text-align:center">
        <form id="frmMaturityDate" method="post" runat="server">
        <table>
            <tr style="text-align:center">
                <td style="text-align:center">
                    <h2>
                        <a href="GetMaturityDate.aspx" style="color: #36393D;">Maturities Calculator</a></h2>
                    <asp:Label runat="server" Style="color: #36393D;">Date format: </asp:Label>
                    <asp:DropDownList ID="ddl_DateFormat" runat="server" AutoPostBack="true" />
                    <br />
                    <table style="background: #C3D9FF; border: 1px solid #36393D;">
                        <asp:Panel ID="DCChoose" runat="server" DefaultButton="btn_Validate">
                            <tr style="text-align:center;font-weight: bold; color: #C3D9FF; background: #36393D;">
                                <td>
                                    Deriv. Contract <span style="font-size:small">(DERIVATIVECONTRACT)</span>
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">Market <span style="font-size:small">(IDM)</span></asp:Label>
                                    <asp:DropDownList ID="ddl_Market" runat="server" Width="500px" AutoPostBack="true" OnSelectedIndexChanged="MarketChanged" />
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">Identifier <span style="font-size:small">(IDENTIFIER)</span></asp:Label>
                                    <asp:TextBox ID="txt_DcIdentifier" CssClass="or-autocomplete" runat="server" Width="500px" OnClick="this.value=''"  />
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">ID System <span style="font-size:small">(IDDC)</span></asp:Label>
                                    <asp:TextBox ID="txt_IdDc" runat="server" Width="500px" OnClick="this.value=''" />
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Button ID="btn_Validate" Text="Validate" runat="server" Width="100%" OnClick="ClickOnValidateButton" />
                                </td>
                            </tr>
                        </asp:Panel>
                        <asp:Panel ID="MaturityChoose" runat="server" DefaultButton="btn_Calculate">
                            <tr style="text-align:center">
                                <td style="text-align:center;font-weight: bold; color: #C3D9FF; background: #36393D;">
                                    Maturity Rule <span style="font-size:small">(MATURITYRULE)</span>
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">Identifier <span style="font-size:small">(IDENTIFIER)</span></asp:Label>
                                    <asp:DropDownList ID="ddl_IdMaturityRule" runat="server" Width="500px" AutoPostBack="true"
                                        OnSelectedIndexChanged="MaturityRuleChanged" />
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">ID System <span style="font-size:small">(IDMATURITYRULE)</span></asp:Label>
                                    <asp:TextBox ID="txt_IdMaturityRule" runat="server" Width="500px" ReadOnly="true"
                                        Style="background: #E5E5E5;" />
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">Frequency <span style="font-size:small">(MMYRULE)</span></asp:Label>
                                    <asp:TextBox ID="txt_Frequency" runat="server" Width="500px" ReadOnly="true" Style="background: #E5E5E5;" />
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align:center;font-weight: bold; color: #C3D9FF; background: #36393D;">
                                    Maturities <span style="font-size:small">(MATURITY)</span>
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">Maturity <span style="font-size:small">(MATURITYMONTHYEAR)</span></asp:Label>
                                    <asp:DropDownList ID="ddl_MaturityMontYear" runat="server" Width="500px" AutoPostBack="true"
                                        OnSelectedIndexChanged="SearchMaturity" />
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">Maturity Date <span style="font-size:small">(MATURITYDATE)</span></asp:Label>
                                    <asp:TextBox ID="txt_MaturityDate" runat="server" Width="500px" ReadOnly="true" Style="background: #E5E5E5;" />
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Label runat="server" Width="300px" Style="color: #36393D;">New Maturity (<i>Format: YYYY or YYYYmm</i>)</asp:Label>
                                    <asp:TextBox ID="txt_NewMaturityMonthYear" Width="500px" runat="server" OnClick="this.value=''" />
                                </td>
                            </tr>
                            <tr style="text-align:center;">
                                <td>
                                    <asp:Button ID="btn_Calculate" Text="Calculate" runat="server" Width="100%" OnClick="ClickOnCalculateButton" />
                                </td>
                            </tr>
                        </asp:Panel>
                    </table>
                    <br />
                    <asp:Table ID="CalculatedMaturities" Style="background: #C3D9FF; border: 1px solid #36393D;"
                        runat="server" Visible="false">
                        <asp:TableHeaderRow Style="font-weight: bold; color: #C3D9FF; background: #36393D;">
                            <asp:TableHeaderCell HorizontalAlign="Center" ColumnSpan="10">Calculated Maturities</asp:TableHeaderCell></asp:TableHeaderRow>
                        <asp:TableRow HorizontalAlign="Center" Style="color: #36393D; background: #C3D9FF;
                            font-weight: bold;">
                            <asp:TableCell>Maturity</asp:TableCell>
                            <asp:TableCell>Maturity Date<br /><span style="font-size:small">(MATURITYDATE)</span></asp:TableCell>
                            <asp:TableCell>Last Trading Date<br /><span style="font-size:small">(LASTTRADINGDAY)</span></asp:TableCell>
                            <asp:TableCell>Non Periodic<br />Settl./Deliv.<br /><span style="font-size:small">(DELIVERYDATE)</span></asp:TableCell>
                            <asp:TableCell>Periodic<br />First Deliv. Date<br /><span style="font-size:small">(FIRSTDELIVERYDATE)</span></asp:TableCell>
                            <asp:TableCell>Periodic<br />First Settlt. Date<br /><span style="font-size:small">(FIRSTDLVSETTLTDATE)</span></asp:TableCell>
                            <asp:TableCell>Periodic<br />Last Deliv. Date<br /><span style="font-size:small">(LASTDELIVERYDATE)</span></asp:TableCell>
                            <asp:TableCell>Periodic<br />Last Settlt. Date<br /><span style="font-size:small">(LASTDLVSETTLTDATE)</span></asp:TableCell>
                            <asp:TableCell>First Notice Day<br /><span style="font-size:small">(FIRSTNOTICEDAY)</span></asp:TableCell>
                            <asp:TableCell>Last Notice Day<br /><span style="font-size:small">(LASTNOTICEDAY)</span></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow ID="Month" runat="server">
                            <asp:TableCell>
                                <asp:TextBox ID="txt_Month" runat="server" ReadOnly="true" Style="font-weight: bold;
                                    text-align: center; background: #E5E5E5;" Width="98%" />
                            </asp:TableCell>
                            <asp:TableCell>
                                <asp:TextBox ID="txt_CalcMaturityDate" runat="server" ReadOnly="true" Width="96%"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                            <asp:TableCell>
                                <asp:TextBox ID="txt_CalcLastTradingDate" runat="server" ReadOnly="true" Width="96%"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                            <asp:TableCell>
                                <asp:TextBox ID="txt_CalcDeliveryDate" runat="server" ReadOnly="true" Width="96%"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                            <asp:TableCell>
                                <asp:TextBox ID="txt_CalcFirstDeliveryDate" runat="server" ReadOnly="true" Width="96%"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                            <asp:TableCell>
                                <asp:TextBox ID="txt_CalcFirstDlvSettltDate" runat="server" ReadOnly="true" Width="96%"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                            <asp:TableCell>
                                <asp:TextBox ID="txt_CalcLastDeliveryDate" runat="server" ReadOnly="true" Width="96%"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                            <asp:TableCell>
                                <asp:TextBox ID="txt_CalcLastDlvSettltDate" runat="server" ReadOnly="true" Width="96%"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                            <asp:TableCell Visible="false">
                                <asp:TextBox ID="txt_CalcFirstNoticeDay" runat="server" ReadOnly="true" Width="96%" Visible="false"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                            <asp:TableCell Visible="false">
                                <asp:TextBox ID="txt_CalcLastNoticeDay" runat="server" ReadOnly="true" Width="96%" Visible="false"
                                    Style="text-align: center; background: #E5E5E5;" />
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </td>
            </tr>
        </table>
        </form>
    </div>

    <script type="text/javascript">
        $(function () {
            $(".or-autocomplete").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "GetMaturityDate.aspx/FetchDerivativeContractIdentifier",
                        data: "{ 'identifier': '" + request.term + "' }",
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        dataFilter: function (data) { return data; },
                        success: function (data) {
                            response($.map(data.d, function (item) {
                                return {
                                    value: item.Identifier
                                }
                            }))
                        },
                        error: AutocompleteError
                    });
                },
                minLength: 1
            });
        });
    </script>

</body>
</html>
