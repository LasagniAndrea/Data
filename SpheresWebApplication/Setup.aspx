<%@ Page Language="c#" Inherits="EFS.Spheres.SetupPage" Codebehind="Setup.aspx.cs" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<!DOCTYPE html>
<html id="HtmlCenter" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title runat="server">Setup</title>

    <style type="text/css">
    #divRdbms > div.contenth  {
        padding:5px;
        display:inline-flex;
    }

    #divRdbms > div.contenth .contenth  {
        padding:5px;
    }
    span.lblCapture {
        cursor:pointer;
    }

    div[id^="tblist"] > div {
        vertical-align:baseline!important;
        }

    #divSphereSystem > div.contenth {
        padding:5px;
    }
    #divSphereSystem > div.contenth > select {
        margin-bottom:5px;
    }
    </style>

</head>
<body class="PageCenter" id="BodyID" runat="server">
    <form id="Setup" method="post" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server" />
    <table class="PageCenter" id="TblSetup" border="0">
        <tr>
            <td class="PageCenter">
                <div class="PageCenter" style="width: 500px;">
                    <asp:PlaceHolder ID="plhHeader" runat="server"></asp:PlaceHolder>
                    <div id="divlogo" style="margin: 4px">
                        <asp:HyperLink ID="imglogosoftware" Text="Software" runat="server" ImageUrl="Images\Logo_Software\Spheres_Banner_v9999.png"
                            NavigateUrl="http://www.euro-finance-systems.com"></asp:HyperLink>
                        <asp:HyperLink ID="imglogocompany" Text="Company"  runat="server" ImageUrl="Images\Logo_Entity\EuroFinanceSystems\EuroFinanceSystems_Banner.gif"
                            NavigateUrl="http://www.euro-finance-systems.com"></asp:HyperLink>
                    </div>
                    <div id="divbody" runat="server">
                            <div id="divSphereSystem" runat="server">
                                <div class="headh">
                                    <asp:Label ID="lblTitleSystem" class="size4" runat="server">Spheres® System</asp:Label>
                                </div>
                                <div class="contenth">
                                    <asp:Label ID="lblSourceDisplay" runat="server" CssClass="lblCapture" Width="100%">Data access</asp:Label>
                                    <asp:DropDownList ID="ddlSourceDisplay" runat="server" CssClass="ddlCapture" Width="50%"/>
                                    <br /><br />
                                    <asp:Label ID="lblHash" runat="server" CssClass="lblCapture" Width="30%">New Hash algorithm</asp:Label>
                                    <asp:Label ID="lblOldHash" runat="server" CssClass="lblCapture" Width="30%">Old Hash algorithm</asp:Label>
                                    <asp:Label ID="lblDeprecatedOldHash" runat="server" CssClass="lblCapture" Width="30%">Deprecated date</asp:Label>
                                    <asp:DropDownList ID="ddlHash" runat="server" CssClass="ddlCapture" Width="30%"/>
                                    <asp:DropDownList ID="ddlOldHash" runat="server" CssClass="ddlCapture" Width="30%"/>
                                    <efsc:WCTextBox2 ID="txtDtDeprecatedOldHash" runat="server" CssClass="DtPicker" Width="20%"></efsc:WCTextBox2>
                                </div>
                            </div>
                            <br />
                            <div id="divRdbms" runat="server">
                                <div class="headh">
                                    <asp:Label ID="lblTitleRdbms" class="size4" runat="server">Relational DataBase Management System</asp:Label>
                                </div>
                                <div class="contenth">
                                    <div id="divData" runat="server">
                                        <div class="headhR">
                                            <asp:Label ID="lblData" class="size5" runat="server">Data</asp:Label>
                                        </div>
                                        <div class="contenth">
                                            <asp:Label ID="lblRdbmsName" runat="server" CssClass="lblCapture">RdbmsName</asp:Label>
                                            <asp:DropDownList ID="ddlRdbmsName" runat="server" CssClass="ddlCapture" Width="100%"/>
                                            <br /><br />
                                            <asp:Label ID="lblServerName" runat="server" CssClass="lblCapture">ServerName</asp:Label>
                                            <asp:TextBox ID="txtServerName" runat="server" CssClass="txtCapture" Width="97%"></asp:TextBox>
                                            <asp:Label ID="lblDatabaseName" runat="server" CssClass="lblCapture">DatabaseName</asp:Label>
                                            <asp:TextBox ID="txtDatabaseName" runat="server" CssClass="txtCapture" Width="97%"></asp:TextBox>
                                            <br /><br />
                                            <asp:Label ID="lblUserName" runat="server" CssClass="lblCapture">UserName</asp:Label>
                                            <asp:TextBox ID="txtUserName" runat="server" CssClass="txtCaptureOptional" Width="97%"></asp:TextBox>
                                            <asp:Label ID="lblPwd" runat="server" CssClass="lblCapture">Password</asp:Label>
                                            <asp:TextBox ID="txtPwd" runat="server" CssClass="txtCaptureOptional" Width="97%" TextMode="Password"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div id="divLog" runat="server">
                                        <div class="headhR">
                                            <asp:Label ID="lblLog" class="size5" runat="server">Log</asp:Label>
                                        </div>
                                        <div class="contenth">
                                            <asp:Label ID="lblRdbmsNameLog" runat="server" CssClass="lblCapture">RdbmsName</asp:Label>
                                            <asp:DropDownList ID="ddlRdbmsNameLog" runat="server" CssClass="ddlCapture" Width="100%"/>
                                            <br /><br />
                                            <asp:Label ID="lblServerNameLog" runat="server" CssClass="lblCapture">ServerName</asp:Label>
                                            <asp:TextBox ID="txtServerNameLog" runat="server" CssClass="txtCapture" Width="97%"></asp:TextBox>
                                            <asp:Label ID="lblDatabaseNameLog" runat="server" CssClass="lblCapture">DatabaseName</asp:Label>
                                            <asp:TextBox ID="txtDatabaseNameLog" runat="server" CssClass="txtCapture" Width="97%"></asp:TextBox>
                                            <br /><br />
                                            <asp:Label ID="lblUserNameLog" runat="server" CssClass="lblCapture">UserName</asp:Label>
                                            <asp:TextBox ID="txtUserNameLog" runat="server" CssClass="txtCaptureOptional" Width="97%"></asp:TextBox>
                                            <asp:Label ID="lblPwdLog" runat="server" CssClass="lblCapture">Password</asp:Label>
                                            <asp:TextBox ID="txtPwdLog" runat="server" CssClass="txtCaptureOptional" Width="97%" TextMode="Password"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                            </div>
                    </div>

                    <asp:Panel id="divalltoolbar" runat="server">
                        <div id="tblist" style="display: initial;">
                            <div>
                                <asp:LinkButton ID="btnOk" runat="server" CssClass="fa-icon" OnClick="BtnOk_Click"></asp:LinkButton>
                                <asp:LinkButton ID="btnCancel" runat="server" CssClass="fa-icon" OnClick="BtnCancel_Click"></asp:LinkButton>
                                <asp:LinkButton ID="btnApply" runat="server" CssClass="fa-icon" OnClick="BtnApply_Click"></asp:LinkButton>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
