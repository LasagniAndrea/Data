<%@ Page language="c#" Inherits="EFS.Spheres.Trial.HelpCryptagePage" Codebehind="HelpCryptage.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<head runat="server">
		<title>HelpCryptage</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio 7.0">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	</head>
	<body>
		<form id="HelpCryptage" method="post" runat="server">
			<table width="352" cellpadding="5" cellspacing="0" style="WIDTH: 352px; HEIGHT: 152px">
				<tr bgcolor="#6699cc">
					<td align="left" height="25" style="BORDER-RIGHT:black 1px solid; BORDER-TOP:black 1px solid; BORDER-LEFT:black 1px solid; BORDER-BOTTOM:black 1px solid">
						<font face="Arial" color="white"><b>Cryptage</b></font>
					</td>
				</tr>
				<tr bgcolor="#eeeeee">
					<td align="middle" height="25" style="BORDER-RIGHT:black 1px solid; BORDER-TOP:1px; BORDER-LEFT:black 1px solid; BORDER-BOTTOM:black 1px solid">
						<table width="100%">
							<tr>
								<td style="WIDTH: 160px"><font face="Arial" size="-1">Caractères à crypter :</font></td>
								<td><b><asp:textbox id="txtToCrypt" size="14" runat="server" Width="168px" BorderStyle="Solid" BorderWidth="1px" /></b>
								</td>
							</tr>
							<tr>
								<td style="WIDTH: 160px"><font face="Arial" size="-1">Type de cryptage :</font></td>
								<td>
									<asp:DropDownList id="DdwTypeCrypt" runat="server" Width="168px">
										<asp:ListItem Value="MD5">MD5</asp:ListItem>
										<asp:ListItem Value="SHA1">SHA1</asp:ListItem>
										<asp:ListItem Value="CLEAR">CLEAR</asp:ListItem>
									</asp:DropDownList></td>
							</tr>
							<tr>
								<td style="WIDTH: 160px"><font face="Arial" size="-1">Caractères cryptés :</font></td>
								<td><b><asp:textbox id="TxtCrypt" size="14" runat="server" Width="168px" BorderStyle="Solid" BorderWidth="1px" /></b>
								</td>
							</tr>
							<tr>
								<td style="WIDTH: 160px"></td>
								<td align="right">
									<asp:Button id="btnCrypt" BorderWidth="1px" BorderStyle="Solid" runat="server" Text="Valider" BorderColor="Black" BackColor="LightSteelBlue" onclick="BtnCrypt_Click"></asp:Button>
								</td>
							</tr>
						</table>
					</td>
				</tr>
			</table>
		</form>
	</body>
</HTML>
