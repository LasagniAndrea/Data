<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Postback.aspx.cs" Inherits="EFS.Spheres.Trial.Postback" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox runat="server" id="txt1"></asp:TextBox> 
            <asp:TextBox runat="server" id="txt2"></asp:TextBox>
            <asp:TextBox runat="server" id="txt3"></asp:TextBox>
            <br />
            <asp:DropDownList runat="server" ID ="DDL1" >
                <asp:ListItem Text="value1"  Value="value1"></asp:ListItem>
                <asp:ListItem Text="value2"  Value="value2"></asp:ListItem>
            </asp:DropDownList>
            <asp:DropDownList runat="server" ID ="DDL2" >
                <asp:ListItem Text="value1"  Value="value1"></asp:ListItem>
                <asp:ListItem Text="value2"  Value="value2"></asp:ListItem>
            </asp:DropDownList>
            <asp:TextBox runat="server" id="TextBox1"></asp:TextBox>
        </div>
    </form>
</body>
</html>
