<%@ Page Language="C#" AutoEventWireup="true" Inherits="Trial_Default" Codebehind="GetObjectByName.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Load element by name</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <br />
        <asp:Label ID="LblTitle" runat="server" Text="Label">Serialization d'un element du dataDocument</asp:Label>
        <br />
        <br />
        <asp:Table ID="tbl1" runat="server" GridLines="Both" >
            <asp:TableRow runat="server">
                <asp:TableCell runat="server">
                    <asp:Label ID="lblIdentifier" runat="server"  Text="Entrez l'identifier du trade"/>
                </asp:TableCell>
                <asp:TableCell runat="server" ColumnSpan ="2">
                    <asp:TextBox ID="txtIdentifier" runat="server" Width ="100%" />
                </asp:TableCell>
            </asp:TableRow>
            
            <asp:TableRow runat="server">
                <asp:TableCell runat="server">
                    <asp:Label runat="server" Text="Entrez l'élément parent [et son occurence]"/>
                </asp:TableCell>
                <asp:TableCell runat="server" >
                    <asp:TextBox ID="txtElementParent" runat="server" />
                </asp:TableCell>
                <asp:TableCell  runat="server">
                    <asp:TextBox ID="txtElementParentOccurence" runat="server"/>
                </asp:TableCell>
                
            </asp:TableRow>
            <asp:TableRow  runat="server">
                <asp:TableCell  runat="server">
                    <asp:Label  runat="server" Text="Entrez l'élément"/>
                </asp:TableCell>
                <asp:TableCell runat="server" ColumnSpan ="2">
                    <asp:TextBox ID="txtElement" runat="server" Width ="100%" />
                </asp:TableCell>
            </asp:TableRow>
            
        </asp:Table>
        
        <br />
        <br />
        <asp:Button ID="btnLoad" runat="server" Text="Load" OnClick="BtnLoad_Click"  />
        <br />
        <br />
          <asp:Panel ID="pnl1" runat="server"  BorderWidth="2px" Height ="600px"  Width="600px">
                <asp:TextBox ID="txtResult" runat="server"    TextMode ="MultiLine" Width="592px" Height ="592px"   ></asp:TextBox> 
         </asp:Panel>
    </div>
    </form>
</body>
</html>
