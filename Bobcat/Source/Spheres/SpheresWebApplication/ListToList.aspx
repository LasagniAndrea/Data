<%@ Page Language="c#" Inherits="EFS.Spheres.ListToListPage" Codebehind="ListToList.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>ListToList</title>
        
        <script type="text/javascript">
	    function Move(l1,l2)
	    {
		    for (i=0; i < l1.options.length ; i++)
		    {
			    if (l1.options(i).selected && l1.options(i)!= "" )
			    {
				    o=new Option(l1.options(i).text,l1.options(i).value);
				    l2.options[l2.options.length]=o;
				    l1.options[i]=null;
				    i = i -1 ;
			    }
			    else
			    {
				    //alert("Aucun élément sélectionné");
			    }
		    }
	    }
	    function MoveAll(l1,l2)
	    {
		    for (i=0; i < l1.options.length ; i++)
		    {
			    o=new Option(l1.options(i).text,l1.options(i).value);
			    l2.options[l2.options.length]=o;
			    l1.options[i]=null;
			    i = i -1 ;
		    }
	    }
	    function Switch(menu, way)
	    {
		    // Init
		    var menumax = menu.length -2;
		    var menusel = menu.selectedIndex;
    	    
		    // Debordement
		    if ((menusel < 0) || (menusel < 1 && way == -1) || (menusel > menumax && way == 1)) { return false; }

		    // Permutation
		    tmpopt = new Option( menu.options[menusel+way].text, menu.options[menusel+way].value );
    	    
		    menu.options[menusel+way].text = menu.options[menusel].text; 
		    menu.options[menusel+way].value = menu.options[menusel].value; 
		    menu.options[menusel+way].selected = true;
    	    
		    menu.options[menusel].text = tmpopt.text;                         
		    menu.options[menusel].value = tmpopt.value;
		    menu.options[menusel].selected = false;
		    return true;
	    }
	    function Record(l,h)
	    {
		    var selection = "";
		    for (i=0; i < l.options.length ; i++)
		    {
			    selection += l.options(i).value + '|';
		    }
		    h.value = selection;
	    }
	    //-->
        </script>
</head>
<body id="BodyID">
    <form id="frmListToList" method="post" runat="server">
        &nbsp;<asp:PlaceHolder ID="plhMain" runat="server"></asp:PlaceHolder>
        <asp:TextBox ID="TextBox1" Style="z-index: 101; left: 8px; position: absolute; top: 48px" runat="server"></asp:TextBox>
        <asp:Button ID="btnPostBack" Style="z-index: 102; left: 16px; position: absolute; top: 88px" runat="server" Text="POSTBACK" OnClick="BtnPostBack_Click"></asp:Button>
    </form>
</body>
</html>
