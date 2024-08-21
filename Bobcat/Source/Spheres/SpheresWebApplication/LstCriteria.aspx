<%@ Page Language="c#" Inherits=" EFS.Spheres.LstCriteriaPage" CodeBehind="LstCriteria.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
    <!-- FI 20200107 [XXXX] override ui-autocomplete properties  -->
    <style>
        .ui-autocomplete {
            max-height: 190px;
            overflow-y: auto;
            /* prevent horizontal scrollbar */
            overflow-x: hidden;
            text-align: left;
        }
    </style>
</head>
<body class="center" runat="server">
    <form id="frmLstCriteria" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <asp:panel runat="server" CssClass="center" id="divmainlst">
            <asp:PlaceHolder ID="plhControl" runat="server"></asp:PlaceHolder>
        </asp:panel>
    </form>
</body>
</html>
