function OpenDateSelection(ctrlBound, pageTitle)
{
	WindowOpenDate(ctrlBound,pageTitle,'D');		
}

function OpenDateTimeSelection(ctrlBound, pageTitle)
{
	WindowOpenDate(ctrlBound,pageTitle,'DT');		
}

function WindowOpenDate(ctrlBound, pageTitle, mode)
{
	var ctrlName  = ctrlBound.name;
	var dateValue = ctrlBound.value;
	var x         = "left=" + event.screenX;
	var y         = "top="  + event.screenY;
	
	if (screen.width < (event.screenX+211))
		x = "left=" + (screen.width-250);
		
	if (screen.height < (event.screenY+165))
		y= "top=" + (screen.height-200);
		
	ret = window.open("{--ApplicationPath--}/DateSelection.aspx?MODE=" + mode + "&ID=" + ctrlName + "&Value=" + dateValue + "&Title=" + pageTitle ,"DateSelection","fullscreen=no,location=no,menubar=no,toolbar=no,width=272,height=175," + x + "," + y );
}

function SetDate(ctrlBound, dateValue)
{
	var form = document.forms[0];
	var ctrl = form.elements[ctrlBound];
	if (ctrl != null)
	{
		ctrl.value = dateValue;
		ctrl.focus();
	}
}

function OnKeyDown(callEvent,callArgument)
{
	if (event.keyCode == Sys.UI.Key.enter)
	{
		event.returnValue = false;
		event.cancel      = true;
		__doPostBack(callEvent,callArgument);
	}
	else
	{
	    event.returnValue = false;
	    event.cancel      = true;
	}
}

function OldOnKeyDown(callEvent,callArgument)
{
	if (document.all)
	{
		if (event.keyCode == Sys.UI.Key.enter)
		{
			event.returnValue = false;
			event.cancel      = true;
			__doPostBack(callEvent,callArgument);
		}
	}
}

function CheckKey(event)
{
	if (document.all)
	{
		if (event.keyCode == Sys.UI.Key.backspace)
		{			
			elt = event.srcElement;
			if ((false == elt.isTextEdit) | (false == elt.isContentEditable) | (null == elt.parentTextEdit))
				event.returnValue = false;
		}
		else if (event.keyCode ==  Sys.UI.Key.enter)
		{			
   			event.returnValue = false;
		}
	}
}

function ChangeOverflow(element,before,after)
{
    var div = element.getElementsByTagName("div");
    if (null != div)
    {
        for (var i=0;i<div.length;i++)
        {
            if (div[i].style.overflow == before)
		        div[i].style.overflow = after;
	    }
	}
}	

function ChangeRowScrollPosition(element,before,after)
{
    var tr = element.getElementsByTagName("tr");
    if (null != tr)
    {
        for (var i=0;i<tr.length;i++)
        {
            if ((null != tr[i].name) && (-1 < tr[i].id.indexOf("scrollTopPosition")))
            {
                if (tr[i].style.position == before)
    		        tr[i].style.position = after;
    		}
	    }
	}
}	

function CallPrint(headerId,bodyId)
{
     var prtTitle = "Spheres®";
     var frmTitle = document.title;
     if (null != frmTitle)
     {
        prtTitle = frmTitle;
     }
     var prtLink = "Includes/EFSThemeBlue.css";
     var frmLink = document.getElementById('linkCss');
     if (null != frmLink)
     {
        prtLink = frmLink.getAttribute('href');
     }
     var prtHeader;
     if (null != headerId)
     {
        prtHeader = document.getElementById(headerId);
        ChangeRowScrollPosition(prtHeader,"relative","static");
     }
     var prtBody;
     if (null != bodyId)
     {
        prtBody = document.getElementById(bodyId);
        ChangeRowScrollPosition(prtBody,"relative","static");
     }
     var winPrint = window.open('','','left=0,top=0,width=1,height=1,toolbar=0,scrollbars=1,resizable=1,status=0');
     winPrint.document.write("<html>");
     winPrint.document.write("<head>");
     winPrint.document.write("  <title>" + prtTitle + " [Print]</title>");
     winPrint.document.write('  <style type="text/css">');
     winPrint.document.write('    thead {display: table-header-group;}');
     winPrint.document.write('    tfoot {display: table-footer-group;}');
     winPrint.document.write('  </style>');
     winPrint.document.write('  <link rel="stylesheet" href="Includes/EFSThemeCommon.css"/>');
     winPrint.document.write('  <link rel="stylesheet" href="' + prtLink + '"/>');
     winPrint.document.write('  <link type="image/x-icon" rel="shortcut icon" href="Images/ico/Spheres.ico" />');
     winPrint.document.write("</head>");
     winPrint.document.write("<body>");
     if (null != prtHeader)
     {
        winPrint.document.write(prtHeader.innerHTML);
        ChangeRowScrollPosition(prtHeader,"static","relative");
     }
     if (null != prtBody)
     {
        winPrint.document.write(prtBody.innerHTML);
        ChangeRowScrollPosition(prtBody,"static","relative");
     }
     winPrint.document.write("</body>");
     winPrint.document.write("</html>");
     ChangeOverflow(winPrint.document,"auto","visible");
     winPrint.document.close();
     winPrint.focus();
     winPrint.print();
     return true;
}

function ExpandCollapse2()
{
    var imgGroup = window.event.srcElement;
    var trChild	 = document.getElementsByName(imgGroup.name);
    for(var j=0;j<trChild.length;j++)
    {
      if (trChild[j].id != imgGroup.id)
        trChild[j].style.display = trChild[j].style.display=="none"?"inline":"none";
      else if (-1 == imgGroup.src.indexOf("collapse"))
        imgGroup.src = imgGroup.src.replace("expand","collapse");
      else
        imgGroup.src = imgGroup.src.replace("collapse","expand");
    }
}

function ExpandCollapseAll2(sender)
{
    alert("1a");
	if(sender == null)
		return;
    alert("1b");
	var mode = sender.getAttribute("mode");		
	var img = document.getElementsByTagName("img");
    alert("2");	
	for (var i=0;i<img.length;i++)
	{
	    alert(i);	
        if ((-1 < img[i].src.indexOf("expand")) && (mode=="expand"))
			    img[i].click();
		if ((-1 < img[i].src.indexOf("collapse")) && (mode=="collapse"))
			    img[i].click();
	}
	if (mode =="expand")
	{
		sender.setAttribute("mode","collapse");
		sender.setAttribute("src",sender.getAttribute("collapse"));
		sender.setAttribute("title",sender.getAttribute("titlecollapse"));			
	}
	else if (mode =="collapse")
	{
		sender.setAttribute("mode","expand");
		sender.setAttribute("src",sender.getAttribute("expand"));
		sender.setAttribute("title",sender.getAttribute("titleexpand"));			
	}
}
