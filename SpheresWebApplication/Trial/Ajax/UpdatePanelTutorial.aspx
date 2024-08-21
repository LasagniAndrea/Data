

<%@ Page Language="C#" Inherits="UpdatePanelTutorial" ValidateRequest="true" Codebehind="UpdatePanelTutorial.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">



<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
    
    <style type="text/css">
    #UpdatePanel1 {   width:300px; height:400px;   }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <!--<div style="padding-top: 10px">-->
    <div class="UpdatePanel1">
        <asp:ScriptManager ID="ScriptManager1" runat="server"  >
        </asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode= "Always">
            <ContentTemplate>
                <asp:Label ID="Label1" runat="server" Text="Panel created."></asp:Label>
                <br/>
                <asp:Button ID="Button1" runat="server" Text="Button" />
                <br/>
                <asp:Button ID="Button3" runat="server" Text="Button3"   />
                <br/>
                <asp:TextBox  ID="TextBox1" runat="server" Text="Textbox1"  AutoPostBack = "true"  />
                <asp:TextBox  ID="TextBox2" runat="server" Text="Textbox2"  AutoPostBack = "true"  />
            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID ="Button3"/>
            </Triggers>
        </asp:UpdatePanel>
        <br />
        <hr />
        <asp:Button ID="Button2" runat="server" Text="Button" />
        <asp:Label ID="Label2" runat="server" Text="not in panel"></asp:Label>
        <asp:UpdateProgress runat="server" ID="UpdateProgress"  AssociatedUpdatePanelID="UpdatePanel1" Visible="true" >
            <ProgressTemplate  >
                Update in progress...
             </ProgressTemplate>
         </asp:UpdateProgress> 
        <br />
    </div>
    </form>
</body>
</html>
Copyright © 2005 - 2007 Microsoft Corporation. All rights reserved.
