<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Task_Test.aspx.cs" Inherits="EFS.Spheres.Trial.Task_Test" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h3 runat ="server" >Insertion en parallèle dans 3 tables avec transaction partagée</h3>
            <div>
                <h5 runat ="server" >Paramètres</h5>
                <asp:Table ID="TableParam" runat="server">
                <asp:TableRow runat="server" VerticalAlign="Top">
                    <asp:TableCell>
                        <label>Nbr d'insert dans chaque table :</label>
                            <asp:TextBox ID="txtNbInsert" runat="server"></asp:TextBox>        
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow runat="server" VerticalAlign="Top">
                    <asp:TableCell>
                        <label>Pause post chaque insert (en seconde(s)) :</label>
                        <asp:TextBox ID="txtSleepTime" runat="server"></asp:TextBox>        
                    </asp:TableCell>
                    </asp:TableRow>

                    <asp:TableRow runat="server" VerticalAlign="Top">
                    <asp:TableCell>
                        <label>CS :</label>
                        <asp:TextBox ID="txtCS" runat="server"  Width="1250px"></asp:TextBox>        
                    </asp:TableCell>
                    </asp:TableRow>

                </asp:Table>
                
            </div>
            <div>
                <h5 runat="server" >Transaction Mode =&gt; ConnectionString ou DbTransaction </h5>
                <asp:Table ID="Table1" runat="server">
                <asp:TableRow runat="server" VerticalAlign="Top">
                    <asp:TableCell>
                        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="GO" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:CheckBox ID="chkTransaction" runat="server"  />
                        <label>sous DbTransaction</label>
                    </asp:TableCell>
                </asp:TableRow>
                </asp:Table>
            </div>
            <div>
            <h5 runat ="server" > Transaction Mode => TransactionScope</h5>
            <asp:Table ID="Table2" runat="server">
                <asp:TableRow runat="server" VerticalAlign="Top">
                    <asp:TableCell>
                    <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="GO" />
                    <label>Utilisation d'une TransactionScope</label>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow runat="server" VerticalAlign="Top">
                    <asp:TableCell>
                        <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="GO" />
                        <label>Utilisation de DependentTransaction</label>
                    </asp:TableCell>

                </asp:TableRow>
            </asp:Table>
            </div>
            <div>
            <h5 runat ="server" > TransactionScope Sample</h5>
            <asp:Table ID="Table3" runat="server">
                <asp:TableRow runat="server" VerticalAlign="Top">
                    <asp:TableCell>
                    <asp:Button ID="Button4" runat="server" OnClick="Button4_Click" Text="GO" />
                        </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            </div>
        </div>
    </form>
</body>
</html>
