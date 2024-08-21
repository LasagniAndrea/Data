<%@ Page Language="C#" AutoEventWireup="true" Inherits="ChangePwd" CodeBehind="ChangePwd.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title id="titlePage" runat="server" />
    <link id="linkCssSummary" href="Includes/Summary.min.css" rel="stylesheet" type="text/css" />
    <link id="linkFedAuth" href="Includes/fedAuth.min.css" rel="stylesheet" type="text/css" />

    <style type="text/css">
        #frmChangePwd {
            max-width: 350px;
            margin: 10% auto;
        }

        #divpwd {
            border: 1pt solid #000;
            background-color:transparent!important;
        }

        h4 {
            margin-block-start : 0px;
            margin-block-end : 0px;
            background-color:#c00303;
            color:#fff;
            text-align:center;
            font-size:14pt;
            font-weight:normal;
        }

        #divbody span,
        #divbody input {
            font-size: initial;
            margin:5px
        }

        #divbody {
            /*padding: 2px;*/
            opacity: 90%;
            /*margin:5px;*/
        }

        #pnlFooter {
            background-color: #000!important;
            color: #fff;
            line-height: 25px;
            font-size: 11pt;
            padding: 1px 10px;     
            margin-top:5px;
        }

        #divPwdMsg {
            display:flex;
        }

        #lblPwdMsg {
            display:block;
            font-size: larger!important;
            text-align: justify;
            font-weight:500;
        	padding: 4px 0px;
	        color: #C00303 !important;        
        }

        input {
            width: 95%;
            padding: 0px;
            text-align: initial;
            border-radius:0px;
        }

        #btnPostpone {
            float:right;
        }

    </style>

</head>
<body id="fedauthbody">
    <form id="frmChangePwd" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <asp:panel id="divpwd" runat="server">
            <asp:Panel ID="pnlHeader" runat="server">
                <asp:Label ID="lblPassword" runat="server">Mot de passe</asp:Label>
            </asp:Panel>
            <h4 id="PasswordTitle" runat="server">PasswordTitle</h4>
            <asp:Panel ID="divbody" CssClass="fedauthopacity" runat="server">
                <div id="divPwdMsg">
                    <asp:Label ID="lblPwdMsg" runat="server"></asp:Label>
                </div>
                <div>
                <asp:Label ID="lblOldPassword" runat="server">PWOld</asp:Label>
                <asp:TextBox ID="txtOldPassword" runat="server" CssClass="txtCapture" TextMode="Password"></asp:TextBox>
                </div>
                <div>
                    <asp:Label ID="lblNewPassword" runat="server">PWNew</asp:Label>
                    <asp:TextBox ID="txtNewPassword" runat="server" CssClass="txtCapture" TextMode="Password"></asp:TextBox>
                </div>
                <div>
                    <asp:Label ID="lblConfirmPassword" runat="server">PWConfirmNew</asp:Label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="txtCapture" TextMode="Password"></asp:TextBox>
                </div>
            </asp:Panel>
            <div id="pnlFooter">
                <asp:LinkButton ID="btnRecord" runat="server" CssClass="fa-icon" Text="<i class='fa fa-check'></i>" OnClick="BtnRecord_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancel" runat="server" CssClass="fa-icon" Text="<i class='fa fa-check'></i>" OnClick="BtnCancel_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnPostpone" runat="server" CssClass="fa-icon" Text="<i class='fa fa-check'></i>" OnClick="BtnPostpone_Click"></asp:LinkButton>
            </div>
        </asp:panel>
    </form>
</body>
</html>




