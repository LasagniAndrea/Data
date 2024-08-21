<%@ Page Language="c#" Inherits="EFS.Spheres.StatusCapturePage" Codebehind="Status.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title id="title">TradeStatus</title>
</head>
<body class="center" id="BodyID" onload="window.resizeTo(960,750);">
    <form id="frmStatus" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <asp:panel runat="server" CssClass="center" id="divmain">
        <asp:PlaceHolder ID="plhHeader" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="plhToolBar" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="plhBody" runat="server"></asp:PlaceHolder>
</asp:panel>
    </form>
</body>
</html>
