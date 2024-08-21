<%@ Page Language="c#" Inherits="EFS.Spheres.ErrorPage" Codebehind="Error.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title runat="server" id="titlePage" />
</head>
<body id="BodyID" class="white">
    <form id="Form1" method="post" runat="server">
        <table id="MainTable" style="z-index: 101; left: 0px; position: absolute; top: 0px;height:100%;width:100%" border="0">
            <tr style="height:50px">
                <td>&nbsp;</td>
            </tr>
            <tr style="width:100%">
                <td style="vertical-align:middle;text-align:center">
                    <table style="height:100%;width:100%" border="0">
                        <tr style="height:20%;">
                            <td colspan="2">&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="font-weight:bold;font-size:small;color: #cc3333;font-family:Verdana;font-variant:small-caps;text-align:center;" colspan="2">
                                AN ERROR HAS OCCURED&nbsp;!
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; font-size: x-small; color:#a9a9a9; font-family: Verdana; font-variant: small-caps;text-align:left" colspan="2">
                                <br/>
                                <br/>
                                &nbsp;DESCRIPTION:&nbsp;
                                <br/>
                                <br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Page:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorPage" runat="server" CssClass="ErrorPage"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Error:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorMessage" runat="server" CssClass="Error"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Source:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorSource" runat="server" CssClass="ErrorSource"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Detail:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorDetail" runat="server" CssClass="ErrorDetail"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; font-size: x-small; color: #a9a9a9; font-family: Verdana; font-variant: small-caps;text-align:left" colspan="2">
                                <br/>
                                <br/>
                                &nbsp;ENVIRONMENT:&nbsp;
                                <br/>
                                <br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Date:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorDate" runat="server" CssClass="ErrorPage"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Application:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorApplication" runat="server" CssClass="ErrorSource"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Database:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorRDBMS" runat="server" CssClass="ErrorPage"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Browser:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorBrowser" runat="server" CssClass="ErrorPage"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;Culture:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorCulture" runat="server" CssClass="ErrorPage"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:top;">&nbsp;User:&nbsp;</td>
                            <td style="vertical-align:top;width:100%">
                                <asp:Label ID="lblErrorUser" runat="server" CssClass="ErrorPage"></asp:Label><br/>
                            </td>
                        </tr>
                        <tr style="height:80%;">
                            <td colspan="2">&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="text-align:center" colspan="2">
                                <br/>
                                <br/>
                                For more information, please contact the editor <a class="linkButton" href="MAILTO:support@euro-finance-systems.com">
                                    Euro Finance Systems</a></td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
            </tr>
            <tr style="height:10px;">
                <td>&nbsp;</td>
            </tr>
        </table>
    </form>
</body>
</html>
