<%@ Page language="c#" Inherits="EFS.Spheres.Trial.ConfirmPage" Codebehind="Confirm.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<head runat="server">
		<title>Confirm</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
	</head>
	<body class="Capture" >
		<form id="FrmCapture" runat="server">
			<TABLE id="TblToolbar" cellSpacing="1" cellPadding="1" width="100%" border="0" style="Z-INDEX: 101; LEFT: 8px; POSITION: absolute; TOP: 0px; HEIGHT: 32px"
				align="center">
				<tr>
					<td align="left" width="80%">
						<asp:Label id="lblIdT" runat="server" CssClass="frmLbl">Trade number</asp:Label>
						<asp:TextBox id="txtTradeIdentifier" runat="server" CssClass="txtCapture">0</asp:TextBox>
						&nbsp;&nbsp;&nbsp;
						<asp:Label id="lblXSLFile" runat="server" CssClass="frmLbl">XSL file</asp:Label>
						<asp:TextBox id="txtlXSLFile" runat="server" CssClass="txtCapture" Width="200px">IRD_HTML_Output.xsl</asp:TextBox>
					</td>
					<td align="right">
						<asp:button id="btnHtml" runat="server" CssClass="frmbtn" Text="Html"></asp:button>
						<asp:button id="btnPDF" runat="server" CssClass="frmbtn" Text="PDF"></asp:button>
					</td>
				</tr>
				<tr height="1">
					<td colspan="2">
						<hr color="cornflowerblue" width="100%" SIZE="2">
					</td>
				</tr>
			</TABLE>
		</form>
	</body>
</HTML>
