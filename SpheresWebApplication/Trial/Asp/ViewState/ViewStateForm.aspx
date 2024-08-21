<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewStateForm.aspx.cs" Inherits="EFS.Spheres.Trial.Asp.ViewStateForm"  %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    
    <style type="text/css">
        h1 {
        margin-top: unset;
        margin-bottom: unset;
        }
    </style>
</head>
    
<body>
    <form id="form1" runat="server">
        <asp:Panel  runat="server" EnableViewState="false">
            <h1>Enableviewstate: false</h1>
            <asp:label runat ="server" ID="LBL_div1"/>
            <asp:TextBox runat ="server" ID="TXT_div1"/>
        </asp:Panel>
        <asp:Panel  runat="server" EnableViewState="true">
            <h1>Enableviewstate: true</h1>
            <asp:label runat ="server" ID="LBL_div2" />
            <asp:TextBox runat ="server" ID="TXT_div2"/>
        </asp:Panel>
        <div>
            <hr />
            <asp:Button runat="server" ID="BTN_id1" Text="Post" EnableViewState="false" /> 
        </div>
    </form>
</body>
</html>
