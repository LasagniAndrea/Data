<%@ Page Language="c#" Inherits="EFS.Spheres.FedAuthLogout" CodeBehind="FedAuthLogout.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Welcome</title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
    <link id="linkFedAuth" href="Includes/fedAuth.min.css" rel="stylesheet" type="text/css" />
</head>
<body id="fedauthbody" runat="server">

    <form id="frmFedAuthLogout" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <script type="text/javascript" src="./Javascript/Tracker.min.js"></script>
        <asp:Panel runat="server" ID="pnlFederate" CssClass="default grid">
            <asp:Panel ID="pnlHeader" runat="server" />
            <div class="contenth">
                <div>
                    <div id="divmsg" class="grid">
                        <asp:PlaceHolder ID="plhMessage" runat="server" />
                        <asp:Button ID="btnLogout" runat="server" />
                    </div>
                    <div id="divdetail" class="grid">
                        <asp:PlaceHolder ID="plhDetail" runat="server" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </form>
</body>
</html>
