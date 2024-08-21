//<script language="JavaScript">
//<!--
//
function EAR_ExpandCollapseAll(sender) {
    if (sender == null)
        return;
    // Conformite XHTML tous browser (class) / IE7 (className)
    var className = sender.getAttribute("class");
    if (null == className)
        className = sender.getAttribute("className");

    var div = document.getElementsByTagName("div");
    for (var i = 0; i < div.length; i++) {

        if ((-1 < div[i].className.indexOf("plus")) && (-1 < className.indexOf("plus")))
            div[i].click();
        if ((-1 < div[i].className.indexOf("minus")) && (-1 < className.indexOf("minus")))
            div[i].click();
    }
    if (-1 < className.indexOf("plus")) {
        sender.className = sender.className.replace("plus", "minus");
    }
    else if (-1 < className.indexOf("minus")) {
        sender.className = sender.className.replace("minus", "plus");
    }
}


function EAR_ExpandCollapse(sender)
{
	var imgGroup = sender;
	if (0 == imgGroup.id.indexOf("EARBOOK_"))
	{
		var table    = document.getElementsByTagName('TABLE');
		for(var i = table.length; i--;)
		{
			if (null != table[i].id)
				if (0 == table[i].id.indexOf(imgGroup.id))
					table[i].style.display = (table[i].style.display=="none"?"":"none");
		}
	}
	else
	{
		var tHead = imgGroup.parentNode.parentNode.parentNode; // THEAD
		var td = imgGroup.parentNode.nextSibling;
		while (td != null) {
		    if (td.nodeType != 3) { // Eviter le CR
		        td.style.display = (td.style.display == "none" ? "" : "none");
		    }
			td = td.nextSibling;
		}

		var tBody = tHead.nextSibling;
		while (tBody.nodeType == 3) { // Eviter le CR
		    tBody = tBody.nextSibling;
		}
		var tBodyChildren = tBody.children;
		for(var i = tBodyChildren.length; i--;)
		{
			tBodyChildren[i].style.display = (tBodyChildren[i].style.display=="none"?"":"none");
		}
	}
	if (-1 == imgGroup.className.indexOf("minus"))
	    imgGroup.className = imgGroup.className.replace("plus", "minus");
    else
        imgGroup.className = imgGroup.className.replace("minus", "plus");
	
}

function GUI_ShowHide()
{
	var divbody  = document.getElementById('divbody');
	divbody.style.display='none';

	var butDailyEar  = document.getElementById('BUTDAILYEAR');
	var butAllEar    = document.getElementById('BUTALLEAR');
	var butSelectEar = document.getElementById('BUTSELECTEAR');
	
	if (butDailyEar!=null)
	{
		if (butAllEar!=null)
		{					
			if (butSelectEar!=null)
			{
				var objDtStartEar    = document.getElementById('TXTDTSTARTEAR');
				var objDtEndEar      = document.getElementById('TXTDTENDEAR');
				var objPnlEventDates = document.getElementById('divtbear3');
				
				if (objDtStartEar!=null)
				{
					if (objDtEndEar!=null)
					{
						if (butDailyEar.checked || butAllEar.checked )
						{
							objPnlEventDates.style.display = 'none';
						}
						else
						{
							objPnlEventDates.style.display = '';
						}
					}
				}
			}
		}
	}
}
// -->
//</script>