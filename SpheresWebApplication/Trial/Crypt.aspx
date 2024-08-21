<%@ Page language="c#" Inherits="EFS.Spheres.Trial.CryptPage" Codebehind="Crypt.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml" >
	<head runat="server">
		<title id="title">Crypt</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	</head>
	<body>
		<form id="Form1" method="post" runat="server">
			<asp:TextBox id="txtInput" style="Z-INDEX: 100; LEFT: 10px; POSITION: absolute; TOP: 57px" runat="server"
				width="100%" height="108"></asp:TextBox>
			<asp:TextBox id="txtId" style="Z-INDEX: 118; LEFT: 208px; POSITION: absolute; TOP: 312px" runat="server"
				height="24px" width="112px"></asp:TextBox>
			<asp:CheckBox id="chkIO" style="Z-INDEX: 117; LEFT: 24px; POSITION: absolute; TOP: 376px" runat="server"
				Text="IO"></asp:CheckBox>
			<asp:CheckBox id="chkSIGEN" style="Z-INDEX: 116; LEFT: 24px; POSITION: absolute; TOP: 352px" runat="server"
				Text="SIGEN"></asp:CheckBox>
			<asp:CheckBox id="chkEVENTSVAL" style="Z-INDEX: 115; LEFT: 24px; POSITION: absolute; TOP: 328px"
				runat="server" Text="EVENTSVAL"></asp:CheckBox>
			<asp:Button id="btnPeekAsync" style="Z-INDEX: 113; LEFT: 400px; POSITION: absolute; TOP: 440px"
				runat="server" Text="Peek Async" Height="23px" Width="106px" onclick="btnPeekAsync_Click"></asp:Button>
			<asp:Button id="btnPeek" style="Z-INDEX: 112; LEFT: 312px; POSITION: absolute; TOP: 440px" runat="server"
				Text="Peek" Height="23px" Width="71px" onclick="btnPeek_Click"></asp:Button>
			<asp:Button id="btnReceiveAsync" style="Z-INDEX: 111; LEFT: 180px; POSITION: absolute; TOP: 440px"
				runat="server" Text="Receive Async" Height="23px" Width="106px" onclick="btnReceiveAsync_Click"></asp:Button><asp:Button id="btnAsync" style="Z-INDEX: 110; LEFT: 180px; POSITION: absolute; TOP: 440px"
				runat="server" Width="106px" Height="23px" Text="Receive Async"></asp:Button><asp:Button id="btnReceive" style="Z-INDEX: 109; LEFT: 98px; POSITION: absolute; TOP: 440px"
				runat="server" Width="71px" Height="23px" Text="Receive" onclick="btnReceive_Click"></asp:Button><asp:Button id="btnSend" style="Z-INDEX: 108; LEFT: 15px; POSITION: absolute; TOP: 439px" runat="server"
				Width="71px" Height="23px" Text="Send" onclick="btnSend_Click"></asp:Button><asp:TextBox id="txtBody" style="Z-INDEX: 107; LEFT: 13px; POSITION: absolute; TOP: 513px" runat="server"
				height="108" width="100%"></asp:TextBox><asp:TextBox id="txtObject" style="Z-INDEX: 106; LEFT: 13px; POSITION: absolute; TOP: 472px"
				runat="server" height="31px" width="100%"></asp:TextBox><asp:TextBox id="TextBox1" style="Z-INDEX: 102; LEFT: 10px; POSITION: absolute; TOP: 178px" runat="server"
				Width="100%" Height="108px"></asp:TextBox><asp:Button id="btnUncrypt" style="Z-INDEX: 105; LEFT: 96px; POSITION: absolute; TOP: 16px"
				runat="server" Width="71px" Height="23px" Text="Uncrypt" onclick="BtnUncrypt_Click"></asp:Button><asp:TextBox id="txtOutput" style="Z-INDEX: 103; LEFT: 10px; POSITION: absolute; TOP: 178px"
				runat="server" Width="100%" Height="108px"></asp:TextBox>&nbsp;<asp:Button id="btnCrypt" style="Z-INDEX: 104; LEFT: 16px; POSITION: absolute; TOP: 16px" runat="server"
				Width="71px" Height="23px" Text="Crypt" onclick="BtnCrypt_Click"></asp:Button>
			<asp:CheckBox id="chkEVENTSGEN" style="Z-INDEX: 114; LEFT: 24px; POSITION: absolute; TOP: 304px"
				runat="server" Text="EVENTSGEN"></asp:CheckBox>
		</form>
	</body>
</html>
