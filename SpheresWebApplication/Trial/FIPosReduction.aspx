<%@ Page Language="C#" AutoEventWireup="true" Inherits="Trial_FIPosReduction" Codebehind="FIPosReduction.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Page sans titre</title>
    <style type="text/css">
        #form1
        {
            height: 298px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div style="height: 274px">
        <asp:Button runat="server" Text ="OK" />
        <asp:Repeater id="Repeater1" runat="server">
          <HeaderTemplate>
             <table border="1">
                <tr>
                   <td><b>Company</b></td>
                   <td><b>Symbol</b></td>
                </tr>
          </HeaderTemplate>

          <ItemTemplate>
             <asp:TableRow runat="server" >
                <asp:TableCell> 
                <asp:TextBox runat ="server" EnableViewState ="true" text= '<%# DataBinder.Eval(Container.DataItem, "Name") %>' />
                <!--<%# DataBinder.Eval(Container.DataItem, "Name") %> -->
                </asp:TableCell>
                <asp:TableCell> <%# DataBinder.Eval(Container.DataItem, "Ticker") %> </asp:TableCell>
             </asp:TableRow>
          </ItemTemplate>

      <FooterTemplate>
             </table>
          </FooterTemplate>

       </asp:Repeater>
    </div>
    </form>
</body>
</html>
