<%@ Page Language="c#" Inherits="EFS.Spheres.LstSelectionPage" SmartNavigation="False" Codebehind="LstSelection.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title runat="server" id="titlePage" />
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
</head>
<body class="center" runat="server">
    <form id="frmLstSelection" method="post" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server">
    </asp:ScriptManager>
    <input id="hidSelection" type="hidden" name="hidSelection" runat="server" />
    <input id="hidGroupBy" type="hidden" name="hidGroupBy" runat="server" />
    <asp:panel runat="server" CssClass="center" id="divmainlst" style="width: 915px;">
        <asp:PlaceHolder ID="plhControl" runat="server"></asp:PlaceHolder>
    </asp:panel>

    <script type="text/javascript">
        $().ready(function() {
            window.resizeTo(1050, 600);
        });
        $(window).resize(function () {
            SetLstSelectionResize();
        });
    </script>

    </form>
</body>
</html>
