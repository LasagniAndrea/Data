<%@ Page Language="c#" Inherits="EFS.Spheres.DBImageViewerPage" SmartNavigation="False" Codebehind="DBImageViewer.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title id="title" runat="server"></title>
</head>
<body class="center" id="BodyID" onload="window.resizeTo(740,430);">
    <form id="frmImageViewer" method="post" runat="server">
        <asp:panel runat="server" CssClass="center" id="divmain" style="width:650px;">
            <asp:PlaceHolder ID="plhMain" runat="server"></asp:PlaceHolder>
        </asp:panel>
    </form>
</body>
</html>
