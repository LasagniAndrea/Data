<%@ Page language="c#" Inherits="EFS.Spheres.MSExcelPage" Codebehind="MSExcel.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
  <head runat="server">
    <title id="title" runat="server">MSExcel</title>
  </head>
  <body id="BodyID" >
    <form id="frmExcel" method="post" runat="server">
		<asp:DataGrid id="dgMSExcel" style="Z-INDEX: 101; LEFT: 7px; POSITION: absolute; TOP: 8px" runat="server"></asp:DataGrid>
     </form>
  </body>
</html>
