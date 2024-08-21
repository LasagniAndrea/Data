<%@ Page language="c#" Inherits="EFS.Spheres.FileUploadPage" smartNavigation="False" Codebehind="FileUpload.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title id="title" runat="server"></title>
    </head>
    <body class="center" id="BodyID" onload="window.resizeTo(740,280);">
        <form id="fileUpload" method="post" runat="server">
            <asp:panel runat="server" CssClass="center" id="divmain" style="width:650px;">
                <asp:PlaceHolder ID="plhMain" runat="server"></asp:PlaceHolder>
            </asp:panel>
        </form>
    </body>
</html>
