<%@ Page Language="c#" Inherits="EFS.Spheres.LstSavePage" CodeBehind="LstSave.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
</head>
<body class="center" runat="server">
    <form id="frmLstSave" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <asp:panel runat="server" CssClass="center" id="divmainlst">
            <asp:PlaceHolder ID="plhControl" runat="server"></asp:PlaceHolder>
        </asp:panel>
        <script type="text/javascript">
            $().ready(function () {
                window.resizeTo(815, 640);
            });
        </script>
    </form>
</body>
</html>
