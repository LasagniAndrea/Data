<%@ Page Language="C#" AutoEventWireup="true" Inherits="Trial_grid" Codebehind="grid.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>DataGrid Custom Paging Example </title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    <h3> DataGrid Custom Paging Example </h3>

      <asp:DataGrid id="MyDataGrid" 
           AllowCustomPaging="True" 
           AllowPaging="True" 
           PageSize="10" 
           OnPageIndexChanged="MyDataGrid_Page" 
           runat="server">

         <HeaderStyle BackColor="Navy" 
                      ForeColor="White" 
                      Font-Bold="True" />

         <PagerStyle Mode="NumericPages" 
                     HorizontalAlign="Right" />

      </asp:DataGrid>


    </div>
    </form>
</body>
</html>
