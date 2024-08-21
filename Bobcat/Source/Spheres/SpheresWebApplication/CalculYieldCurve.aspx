<%@ Page Language="c#" Inherits="EFS.Spheres.CalculYieldCurvePage" Codebehind="CalculYieldCurve.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title id="Title1" runat="server"/>
</head>
<body id="BodyID">
    <form id="YieldCurve" method="post" runat="server">
        <table id="Table0" style="width:936px;height:496px;border:none">
            <tr>
                <td>
                    <table id="Table1" style="width:928px;height:487px;border:none">
                        <tr>
                            <td style="width: 155px">
                                &nbsp;</td>
                            <td class="Header_TitleRight" style="width: 403px">
                                Yield Curve</td>
                            <td style="width: 1px">
                            </td>
                            <td class="Header_TitleRight">
                                Multi-dimensional Matrix</td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                Identifier Def</td>
                            <td style="width: 403px">
                                <asp:TextBox ID="txtIdYieldCurveDef" runat="server" Width="248px">CURVEKONDOR</asp:TextBox></td>
                            <td style="width: 1px">
                            </td>
                            <td>
                                <asp:TextBox ID="txtIdMatrixDef" runat="server" Width="248px">MATRIX_EG</asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 155px; height: 26px;">
                                Actor Payer</td>
                            <td style="height: 26px; width: 403px;">
                                <asp:TextBox ID="txtIdA_Pay1" runat="server" Width="152px">EFS-BANK</asp:TextBox>
                                <asp:CheckBox ID="chkUseActors" runat="server" 
                                    Text="Utilisation des payers/receivers" AutoPostBack="True" /></td>
                            <td style="width: 1px; height: 26px;">
                            </td>
                            <td style="height: 26px">
                                <asp:TextBox ID="txtIdA_Pay2" runat="server" Width="152px">EFS-BANK</asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                Book Payer</td>
                            <td style="width: 403px">
                                <asp:TextBox ID="txtIdB_Pay1" runat="server" Width="152px">IRS-Trd</asp:TextBox></td>
                            <td style="width: 1px">
                            </td>
                            <td>
                                <asp:TextBox ID="txtIdB_Pay2" runat="server" Width="152px">IRS-Trd</asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                Actor Receiver</td>
                            <td style="width: 403px">
                                <asp:TextBox ID="txtIdA_Rec1" runat="server" Width="152px">CLIENT-1</asp:TextBox></td>
                            <td style="width: 1px">
                            </td>
                            <td>
                                <asp:TextBox ID="txtIdA_Rec2" runat="server" Width="152px">CLIENT-1</asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                Book Receiver</td>
                            <td style="width: 403px">
                                <asp:TextBox ID="txtIdB_Rec1" runat="server" Width="152px">BK-CL1</asp:TextBox></td>
                            <td style="width: 1px">
                            </td>
                            <td>
                                <asp:TextBox ID="txtIdB_Rec2" runat="server" Width="152px">BK-CL1</asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                Currency</td>
                            <td style="width: 403px">
                                <asp:TextBox ID="txtIdC1" runat="server" Width="48px">EUR</asp:TextBox></td>
                            <td style="width: 1px">
                            </td>
                            <td>
                                <asp:TextBox ID="txtIdC2" runat="server" Width="48px">EUR</asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                Date courbe</td>
                            <td style="width: 403px">
                                <asp:TextBox ID="txtDtCurve1" runat="server" Width="80px">20/03/2006</asp:TextBox></td>
                            <td style="width: 1px">
                            </td>
                            <td style="width: 378px">
                                <asp:TextBox ID="txtDtCurve2" runat="server" Width="80px">20/03/2006</asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                Point(s)</td>
                            <td style="width: 403px">
                                <asp:TextBox ID="txtDtPoint" runat="server" Width="80px">01/04/2006</asp:TextBox></td>
                            <td style="width: 1px">
                            </td>
                            <td>
                                <asp:TextBox ID="txtDtExpiration" runat="server" Width="85px">03/06/2006</asp:TextBox><asp:TextBox
                                    ID="txtStrike" runat="server" Width="56px">25</asp:TextBox><asp:TextBox ID="txtDtTerm"
                                        runat="server" Width="88px">04/05/2006</asp:TextBox></td>
                        </tr>
                        
                        <tr>
                            <td style="width: 155px">
                                Type de point</td>
                            <td style="width: 403px">
                            <asp:DropDownList ID="ddlCurvePointType" runat="server" Width="272px" >
                                </asp:DropDownList>
                            <td style="width: 1px">
                            </td>
                            <td>
                            </td>
                        </tr>
                        
                        <tr>
                            <td style="width: 155px">
                                Interpolation</td>
                            <td style="width: 403px">
                                <asp:DropDownList ID="ddlInterpolationMethod" runat="server" Width="272px">
                                </asp:DropDownList>
                                <asp:Button ID="btnFind" runat="server" Width="114px" Text="Yield curve Find"
                                    OnClick="BtnFind_Click"></asp:Button></td>
                            <td style="width: 1px">
                            </td>
                            <td>
                                <asp:DropDownList ID="ddlMatrixInterpolationMethod" runat="server" Width="232px">
                                </asp:DropDownList><asp:Button ID="btnMatrixFind" runat="server" Text="Matrix Find"
                                    OnClick="BnMatrixFind_Click"></asp:Button></td>
                        </tr>
                        <tr>
                            <td colspan="4">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                &nbsp;</td>
                            <td class="Header_TitleRight" colspan="3">
                                Results</td>
                        </tr>
                        <tr>
                            <td style="width: 155px">
                                &nbsp;</td>
                            <td style="width: 403px">
                                <asp:TextBox ID="txtYieldCurveResult" runat="server" Width="415px" ReadOnly="True"
                                    TextMode="MultiLine" Height="260px"></asp:TextBox></td>
                            <td style="width: 1px">
                                &nbsp;&nbsp;&nbsp;</td>
                            <td colspan="2">
                                <asp:TextBox ID="txtMatrixResult" runat="server" Width="415px" ReadOnly="True" TextMode="MultiLine"
                                    Height="260px"></asp:TextBox></td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
