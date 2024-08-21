<%@ Page language="c#" Inherits="EFS.Spheres.UnavailablePage" Codebehind="Unavailable.aspx.cs" %>
<!DOCTYPE html>
<html id="HtmlCenter" xmlns="http://www.w3.org/1999/xhtml">
  <head runat="server">
		<title runat="server" id="titlePage"/>
  </head>
<body class="PageCenter" style="background-color:white;" id="BodyID" >
    <form id="Unavailable" method="post" runat="server">
        <table class="PageCenter" id="MainTable" style="Z-INDEX: 101; LEFT: 0px; POSITION: absolute; TOP: 0px; height:100%;width:100%" border="0">
            <tr>
                <td class="PageCenter">
                    <div class="PageCenter" style="width:700px;">    
                        <table style="width:100%" border="0">
                            <tr style="height:20%">
                                <td colspan="2">&nbsp;</td>
                            </tr>
                            <tr>
                                <td style="font-weight:bold;font-size:small;color:#cc3333;font-family:Verdana;font-variant:small-caps;text-align:center;" colspan="2">
                                    Sorry, the page you requested is not available, please retry later !<br/><br/>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" style="height:1px;">
                                    <hr class="size1"/>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align:top">&nbsp;Page:&nbsp;</td>
                                <td style="vertical-align:top;width:100%">
                                    <asp:label id="lblErrorPage" runat="server" cssclass="ErrorPage"></asp:label>
                                    <br/>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align:top">&nbsp;Error:&nbsp;</td>
                                <td style="vertical-align:top;width:100%">
                                    <asp:label id="lblErrorMessage" runat="server" cssclass="Error"></asp:label>
                                    <br/>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" style="height:1px;">
                                    <hr class="size1"/>
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align:right" colspan="2">
                                    For additional assistance, contact the editor <a class="linkButton" href="MAILTO:support@euro-finance-systems.com" >Euro Finance Systems</a>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
            <tr>
			    <td>&nbsp;</td>
			</tr>
            <tr style="height:10px">
                <td>&nbsp;</td>
			</tr>
        </table>
    </form>
</body>
</html>
