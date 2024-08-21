<%@ Page Language="c#" Inherits="EFS.Spheres.FedAuthLogin" CodeBehind="FedAuthLogin.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Welcome</title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />

    <link id="linkFedAuth" href="Includes/fedAuth.min.css" rel="stylesheet" type="text/css" />

    <style type="text/css">
        #lblLoginMsg {
            font-size: larger;
            text-align: center;
            font-weight: bold;
        }
    </style>

</head>
<body id="fedauthbody" runat="server">

    <form id="frmFedAuthLogin" method="post" runat="server">
        <asp:Panel runat="server" ID="pnlFederate" CssClass="default grid">
            <asp:Panel ID="pnlHeader" runat="server" />
            <div class="contenth">
                <div>
                    <div id="divmsg" class="grid">
                        <asp:PlaceHolder ID="plhMessage" runat="server" />
                        <asp:Button ID="btnAccess" runat="server" />
                        <asp:Label ID="lblLoginMsg" runat="server" />
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
