<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html  >
<head runat="server">
    <title>MultiView ActiveViewIndex Example</title>
<script runat="server">

  protected void NextButton_Command(object sender, EventArgs e)
  {
    // Determine which button was clicked
    // and set the ActiveViewIndex property to
    // the view selected by the user.
    if (DevPollMultiView.ActiveViewIndex > -1 & DevPollMultiView.ActiveViewIndex < 3)
    {
      // Increment the ActiveViewIndex property 
      // by one to advance to the next view.
      DevPollMultiView.ActiveViewIndex += 1;
    }
    else if (DevPollMultiView.ActiveViewIndex == 3)
    {
      // This is the final view.
      // The user wants to save the survey results.
      // Insert code here to save survey results.
      // Disable the navigation buttons.
      Page4Save.Enabled = false;
      Page4Restart.Enabled = false;
    }
    else
    {
      throw new Exception("An error occurred.");
    }
  }

  protected void BackButton_Command(object sender, EventArgs e)
  {
    if (DevPollMultiView.ActiveViewIndex > 0 & DevPollMultiView.ActiveViewIndex <= 2)
    {
      // Decrement the ActiveViewIndex property
      // by one to return to the previous view.
      DevPollMultiView.ActiveViewIndex -= 1;
    }
    else if (DevPollMultiView.ActiveViewIndex == 3)
    {
      // This is the final view.
      // The user wants to restart the survey.
      // Return to the first view.
      DevPollMultiView.ActiveViewIndex = 0;
    }
    else
    {
      throw new Exception("An error occurred.");
    }
  }

  </script>

</head>
<body>
    <form id="Form1" runat="Server">

        <h3>MultiView ActiveViewIndex Example</h3>

        <asp:Panel id="Page1ViewPanel" 
            Width="330px" 
            Height="150px"
            HorizontalAlign="Left"
            Font-size="12" 
            BackColor="#C0C0FF" 
            BorderColor="#404040"
            BorderStyle="Double"                     
            runat="Server">  

            <asp:MultiView id="DevPollMultiView"
                ActiveViewIndex="0"
                runat="Server">

                <asp:View id="Page1" 
                    runat="Server">   

                    <asp:Label id="Page1Label" 
                        Font-bold="true"                         
                        Text="What kind of applications do you develop?"
                        runat="Server"
                        AssociatedControlID="Page1">
                    </asp:Label><br /><br />

                    <asp:RadioButton id="Page1Radio1"
                         Text="Web Applications" 
                         Checked="False" 
                         GroupName="RadioGroup1" 
                         runat="server" >
                    </asp:RadioButton><br />

                    <asp:RadioButton id="Page1Radio2"
                         Text="Windows Forms Applications" 
                         Checked="False" 
                         GroupName="RadioGroup1" 
                         runat="server" >
                     </asp:RadioButton><br /><br /><br />                                       

                    <asp:Button id="Page1Next"
                        Text = "Next"
                        OnClick="NextButton_Command"
                        Height="25"
                        Width="70"
                        runat= "Server">
                    </asp:Button>     

                </asp:View>

                <asp:View id="Page2" 
                    runat="Server">

                    <asp:Label id="Page2Label" 
                        Font-bold="true"                        
                        Text="How long have you been a developer?"
                        runat="Server"
                        AssociatedControlID="Page2">                    
                    </asp:Label><br /><br />

                    <asp:RadioButton id="Page2Radio1"
                         Text="Less than five years" 
                         Checked="False" 
                         GroupName="RadioGroup1" 
                         runat="Server">
                     </asp:RadioButton><br />

                    <asp:RadioButton id="Page2Radio2"
                         Text="More than five years" 
                         Checked="False" 
                         GroupName="RadioGroup1" 
                         runat="Server">
                     </asp:RadioButton><br /><br /><br />

                    <asp:Button id="Page2Back"
                        Text = "Previous"
                        OnClick="BackButton_Command"
                        Height="25"
                        Width="70"
                        runat= "Server">
                    </asp:Button> 

                    <asp:Button id="Page2Next"
                        Text = "Next"
                        OnClick="NextButton_Command"
                        Height="25"
                        Width="70"
                        runat="Server">
                    </asp:Button> 

                </asp:View>

                <asp:View id="Page3" 
                    runat="Server">

                    <asp:Label id="Page3Label1" 
                        Font-bold="true"                        
                        Text= "What is your primary programming language?"                        
                        runat="Server"
                        AssociatedControlID="Page3">                    
                    </asp:Label><br /><br />

                    <asp:RadioButton id="Page3Radio1"
                         Text="Visual Basic .NET" 
                         Checked="False" 
                         GroupName="RadioGroup1" 
                         runat="Server">
                     </asp:RadioButton><br />

                    <asp:RadioButton id="Page3Radio2"
                         Text="C#" 
                         Checked="False" 
                         GroupName="RadioGroup1" 
                         runat="Server">
                     </asp:RadioButton><br />

                    <asp:RadioButton id="Page3Radio3"
                         Text="C++" 
                         Checked="False" 
                         GroupName="RadioGroup1" 
                         runat="Server">
                     </asp:RadioButton><br /><br />

                     <asp:Button id="Page3Back"
                        Text = "Previous"
                        OnClick="BackButton_Command"
                        Height="25"
                        Width="70"
                        runat="Server">
                    </asp:Button> 

                    <asp:Button id="Page3Next"
                        Text = "Next"
                        OnClick="NextButton_Command"
                        Height="25"
                        Width="70"
                        runat="Server">
                    </asp:Button><br />

                </asp:View>     

                <asp:View id="Page4"
                    runat="Server">

                    <asp:Label id="Label1"
                        Font-bold="true"                                           
                        Text = "Thank you for taking the survey."
                        runat="Server"
                        AssociatedControlID="Page4">
                    </asp:Label>

                    <br /><br /><br /><br /><br /><br />              

                    <asp:Button id="Page4Save"
                        Text = "Save Responses"
                        OnClick="NextButton_Command"
                        Height="25"
                        Width="110"
                        runat="Server">
                    </asp:Button>

                    <asp:Button id="Page4Restart"
                        Text = "Retake Survey"
                        OnClick="BackButton_Command"
                        Height="25"
                        Width="110"
                        runat= "Server">
                    </asp:Button>                    

                </asp:View>  

            </asp:MultiView>

        </asp:Panel> &nbsp;
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CellPadding="4"
            DataKeyNames="IDA" DataSourceID="SqlDataSource1" ForeColor="#333333" GridLines="None" AllowPaging="True" AllowSorting="True" EnableSortingAndPagingCallbacks="True" ShowFooter="True">
            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <RowStyle BackColor="#E3EAEB" />
            <Columns>
                <asp:BoundField DataField="IDA" HeaderText="IDA" InsertVisible="False" ReadOnly="True"
                    SortExpression="IDA" />
                <asp:BoundField DataField="IDMODELSAFETY" HeaderText="IDMODELSAFETY" SortExpression="IDMODELSAFETY" />
                <asp:BoundField DataField="IDBC" HeaderText="IDBC" SortExpression="IDBC" />
                <asp:BoundField DataField="IDENTIFIER" HeaderText="IDENTIFIER" SortExpression="IDENTIFIER" />
                <asp:BoundField DataField="DISPLAYNAME" HeaderText="DISPLAYNAME" SortExpression="DISPLAYNAME" />
                <asp:BoundField DataField="DESCRIPTION" HeaderText="DESCRIPTION" SortExpression="DESCRIPTION" />
                <asp:BoundField DataField="TELEPHONENUMBER" HeaderText="TELEPHONENUMBER" SortExpression="TELEPHONENUMBER" />
                <asp:BoundField DataField="MOBILEPHONENUMBER" HeaderText="MOBILEPHONENUMBER" SortExpression="MOBILEPHONENUMBER" />
                <asp:BoundField DataField="FAXNUMBER" HeaderText="FAXNUMBER" SortExpression="FAXNUMBER" />
                <asp:BoundField DataField="TELEXNUMBER" HeaderText="TELEXNUMBER" SortExpression="TELEXNUMBER" />
                <asp:BoundField DataField="MAIL" HeaderText="MAIL" SortExpression="MAIL" />
                <asp:BoundField DataField="WEB" HeaderText="WEB" SortExpression="WEB" />
                <asp:BoundField DataField="ADDRESS1" HeaderText="ADDRESS1" SortExpression="ADDRESS1" />
                <asp:BoundField DataField="ADDRESS2" HeaderText="ADDRESS2" SortExpression="ADDRESS2" />
                <asp:BoundField DataField="ADDRESS3" HeaderText="ADDRESS3" SortExpression="ADDRESS3" />
                <asp:BoundField DataField="ADDRESS4" HeaderText="ADDRESS4" SortExpression="ADDRESS4" />
                <asp:BoundField DataField="ADDRESS5" HeaderText="ADDRESS5" SortExpression="ADDRESS5" />
                <asp:BoundField DataField="ADDRESS6" HeaderText="ADDRESS6" SortExpression="ADDRESS6" />
                <asp:BoundField DataField="CULTURE" HeaderText="CULTURE" SortExpression="CULTURE" />
                <asp:BoundField DataField="CSSFILENAME" HeaderText="CSSFILENAME" SortExpression="CSSFILENAME" />
                <asp:BoundField DataField="IDCOUNTRYRESIDENCE" HeaderText="IDCOUNTRYRESIDENCE" SortExpression="IDCOUNTRYRESIDENCE" />
                <asp:BoundField DataField="REGIONRESIDENCE" HeaderText="REGIONRESIDENCE" SortExpression="REGIONRESIDENCE" />
                <asp:BoundField DataField="IDCOUNTRYTAX" HeaderText="IDCOUNTRYTAX" SortExpression="IDCOUNTRYTAX" />
                <asp:BoundField DataField="REGIONTAX" HeaderText="REGIONTAX" SortExpression="REGIONTAX" />
                <asp:BoundField DataField="IDCOUNTRYNATIONAL" HeaderText="IDCOUNTRYNATIONAL" SortExpression="IDCOUNTRYNATIONAL" />
                <asp:BoundField DataField="REGIONNATIONAL" HeaderText="REGIONNATIONAL" SortExpression="REGIONNATIONAL" />
                <asp:BoundField DataField="SUBSIDIARYCODE" HeaderText="SUBSIDIARYCODE" SortExpression="SUBSIDIARYCODE" />
                <asp:BoundField DataField="RESIDENCECODE" HeaderText="RESIDENCECODE" SortExpression="RESIDENCECODE" />
                <asp:BoundField DataField="ECONOMICAREACODE" HeaderText="ECONOMICAREACODE" SortExpression="ECONOMICAREACODE" />
                <asp:BoundField DataField="NCBRATING" HeaderText="NCBRATING" SortExpression="NCBRATING" />
                <asp:BoundField DataField="INTERNALRATING" HeaderText="INTERNALRATING" SortExpression="INTERNALRATING" />
                <asp:BoundField DataField="BIC" HeaderText="BIC" SortExpression="BIC" />
                <asp:BoundField DataField="LTCODE" HeaderText="LTCODE" SortExpression="LTCODE" />
                <asp:BoundField DataField="NCBNUMBER" HeaderText="NCBNUMBER" SortExpression="NCBNUMBER" />
                <asp:BoundField DataField="BUSINESSNUMBER" HeaderText="BUSINESSNUMBER" SortExpression="BUSINESSNUMBER" />
                <asp:BoundField DataField="ECONOMICAGENTCODE" HeaderText="ECONOMICAGENTCODE" SortExpression="ECONOMICAGENTCODE" />
                <asp:BoundField DataField="PERSONALNUMBER" HeaderText="PERSONALNUMBER" SortExpression="PERSONALNUMBER" />
                <asp:BoundField DataField="TAXNUMBER" HeaderText="TAXNUMBER" SortExpression="TAXNUMBER" />
                <asp:BoundField DataField="IDCCAPITAL" HeaderText="IDCCAPITAL" SortExpression="IDCCAPITAL" />
                <asp:BoundField DataField="CAPITAL" HeaderText="CAPITAL" SortExpression="CAPITAL" />
                <asp:CheckBoxField DataField="ISENABLED" HeaderText="ISENABLED" SortExpression="ISENABLED" />
                <asp:BoundField DataField="DTENABLED" HeaderText="DTENABLED" SortExpression="DTENABLED" />
                <asp:BoundField DataField="DTDISABLED" HeaderText="DTDISABLED" SortExpression="DTDISABLED" />
                <asp:CheckBoxField DataField="ISRDBMSUSER" HeaderText="ISRDBMSUSER" SortExpression="ISRDBMSUSER" />
                <asp:BoundField DataField="PWD" HeaderText="PWD" SortExpression="PWD" />
                <asp:CheckBoxField DataField="ISPWDMODNEXTLOGON" HeaderText="ISPWDMODNEXTLOGON" SortExpression="ISPWDMODNEXTLOGON" />
                <asp:BoundField DataField="DTPWDEXPIRATION" HeaderText="DTPWDEXPIRATION" SortExpression="DTPWDEXPIRATION" />
                <asp:BoundField DataField="PWDREMAININGGRACE" HeaderText="PWDREMAININGGRACE" SortExpression="PWDREMAININGGRACE" />
                <asp:BoundField DataField="SIMULTANEOUSLOGIN" HeaderText="SIMULTANEOUSLOGIN" SortExpression="SIMULTANEOUSLOGIN" />
                <asp:BoundField DataField="FREE1" HeaderText="FREE1" SortExpression="FREE1" />
                <asp:BoundField DataField="FREE2" HeaderText="FREE2" SortExpression="FREE2" />
                <asp:BoundField DataField="FREE3" HeaderText="FREE3" SortExpression="FREE3" />
                <asp:BoundField DataField="FREE4" HeaderText="FREE4" SortExpression="FREE4" />
                <asp:BoundField DataField="FREE5" HeaderText="FREE5" SortExpression="FREE5" />
                <asp:BoundField DataField="FREE6" HeaderText="FREE6" SortExpression="FREE6" />
                <asp:BoundField DataField="DTUPD" HeaderText="DTUPD" SortExpression="DTUPD" />
                <asp:BoundField DataField="IDAUPD" HeaderText="IDAUPD" SortExpression="IDAUPD" />
                <asp:BoundField DataField="DTINS" HeaderText="DTINS" SortExpression="DTINS" />
                <asp:BoundField DataField="IDAINS" HeaderText="IDAINS" SortExpression="IDAINS" />
                <asp:BoundField DataField="EXTLLINK" HeaderText="EXTLLINK" SortExpression="EXTLLINK" />
                <asp:BoundField DataField="ROWATTRIBUT" HeaderText="ROWATTRIBUT" SortExpression="ROWATTRIBUT" />
                <asp:BoundField DataField="CULTURE_CNF" HeaderText="CULTURE_CNF" SortExpression="CULTURE_CNF" />
            </Columns>
            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Left" />
            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <EditRowStyle BackColor="#7C6F57" />
            <AlternatingRowStyle BackColor="White" />
            <PagerSettings Position="TopAndBottom" />
        </asp:GridView>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="Data Source=POSTE-VISTA-91;Initial Catalog=OTCml_HPC;User ID=sa;Password=sa2005"
            ProviderName="System.Data.SqlClient" SelectCommand="SELECT * FROM [ACTOR] WHERE (([IDA] > @IDA) AND ([BIC] <> @BIC)) ORDER BY [IDENTIFIER]">
            <SelectParameters>
                <asp:FormParameter DefaultValue="0" FormField="Ida" Name="IDA" Type="Decimal" />
                <asp:Parameter DefaultValue="123" Name="BIC" Type="String" />
            </SelectParameters>
        </asp:SqlDataSource>

    </form>
</body>
</html>