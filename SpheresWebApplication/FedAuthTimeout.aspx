<%@ Page Language="c#" Inherits="EFS.Spheres.FedAuthTimeout" CodeBehind="FedAuthTimeout.aspx.cs" %>

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

        #frmFedAuthTimout {
            max-width: 200px;
            margin: 0 auto;
        }

        #pnlFederate {
            margin:10% 30%;
        }


        div.title {
            border-bottom: solid 2pt #036AB5;
            color:#036AB5;
        }

        h1{
            font-size:medium;
            font-weight:normal;
        }
    </style>

</head>
<body id="Body1" runat="server">

    <form id="frmFedAuthTimeout" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <asp:Panel runat="server" ID="pnlFederate" CssClass="default">
            <asp:Panel ID="pnlHeader" runat="server" CssClass="headh" />
            <div class="contenth fedauthopacity">
                <div>
                    <div id="divmsg grid">
                        <asp:PlaceHolder ID="plhMessage" runat="server" />
                        <asp:Button ID="btnContinue" runat="server" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </form>
</body>
</html>
