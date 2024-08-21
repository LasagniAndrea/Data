<%@ Page language="c#" Inherits="OTC.Pascal_Test" Codebehind="Pascal_Test.aspx.cs" %>
<%@ Register Assembly="EFS.EfsMLGUI" Namespace="EFS.GUI.ComplexControls" TagPrefix="cc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
	<head runat="server">
		<title>Pascal_Test</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR"/>
		<meta content="C#" name="CODE_LANGUAGE"/>
		<meta content="JavaScript" name="vs_defaultClientScript"/>
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema"/>
		<link id="linkCss" href="/OTC/Includes/EFSThemeBlue.css" rel="stylesheet" runat="server"/>
		<script type="text/javascript">
        function changer_couleurs() 
        {
            document.bgColor=document.Form1.bgColor.value;
            document.Form1.HiddenValue.value=document.Form1.bgColor.value;
        }
		</script>
	</head>
	<body style="background-color:white;">
		<form id="Form1" method="post" runat="server">
			<table style="height:100%;width:100%">
				<tr>
					<td style="font-size:xx-small;color:Blue;font-style:italic;height:82px;" colspan="2">
						<br/>
						<asp:textbox id="txtXYZ" runat="server" AutoPostBack="True"></asp:textbox><asp:requiredfieldvalidator id="RequiredFieldValidatorXYZ" runat="server" ErrorMessage="XYZ" ControlToValidate="txtXYZ"></asp:requiredfieldvalidator>&nbsp;
						<asp:Button id="Button1" runat="server" Text="Button" onclick="Button1_Click"></asp:Button><asp:textbox id="TextBox1" runat="server"></asp:textbox><asp:button id="Button2" runat="server" Text="Button" CausesValidation="False" onclick="Button2_Click"></asp:button>
						<br/>
                        <asp:MultiView ID="MultiView1" runat="server"></asp:MultiView>
						<table id="Table1" cellspacing="1" cellpadding="1" style="width:300px;" border="1">
							<tr>
								<td title="ddd">DDD</td>
								<td></td>
								<td></td>
							</tr>
							<tr>
								<td></td>
								<td></td>
								<td></td>
							</tr>
							<tr>
								<td></td>
								<td></td>
								<td></td>
							</tr>
						</table>
					</td>
				</tr>
			</table>
		</form>
	</body>
</html>
