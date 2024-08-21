<%@ Page language="c#" Inherits="EFS.Spheres.UnderConstructionPage" Codebehind="UnderConstruction.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
	<head runat="server">
		<title runat="server" id="titlePage"/>
	</head>
	<body style="background-color:white" id="BodyID">
		<form id="Form1" method="post" runat="server">
			<table id="MainTable" style="Z-INDEX: 101; LEFT: 0px; POSITION: absolute; TOP: 0px;height:100%;width:100%" border="0">
				<tr style="height:50px">
					<td>&nbsp;</td>
				</tr>
				<tr style="width:100%">
				    <td style="vertical-align:middle;text-align:center">
						<table style="height:100%;width:100%" border="0">
							<tbody>
								<tr>
									<td style="vertical-align:middle;text-align:center;width:100%">
										<asp:label id="lblTitlePage" runat="server" CssClass="ErrorPage"></asp:label>
										<br/>
									</td>
								</tr>
								<tr>
									<td style="vertical-align:middle;text-align:center;width:100%">
									    <br/>
										<br/>
										<asp:label id="lblUnderConstruction" runat="server" CssClass="Error" Width="100%"></asp:label>
										<br/>
										<br/>
									</td>
								</tr>
								<tr>
									<td style="text-align:center">
									    <br/>
										<br/>
										<asp:label id="lblUnderConstructionContact" runat="server"></asp:label>
										<a class="linkButton" href="MAILTO:support@euro-finance-systems.com">Euro Finance Systems</a>
									</td>
								</tr>
							</tbody>
						</table>
					</td>
				</tr>
				<tr>
					<td>&nbsp;</td>
				</tr>
				<tr style="height:10px;">
					<td>&nbsp;</td>
				</tr>
			</table>
		</form>
	</body>
</html>
