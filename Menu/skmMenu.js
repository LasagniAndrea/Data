<script type="text/JavaScript">
//<![CDATA[
// MousedOverMenu
function MousedOverMenu(elem, parent, displayedVertically, displayedDown, mnuNum, enabled)
{
	StopTick(mnuNum);
	// only close the subMenus if we are mousing over a top-level menu
	CloseSubMenus(elem, mnuNum);
	if (elem.onclick != null)
		elem.style.cursor = 'pointer';
		
	// Display child menu if needed
	ResetClassName(mnuNum,enabled);
	var elemwithoutprefix = elem.id.split("-X-");
	if ((elemwithoutprefix != null) && (2==elemwithoutprefix.length))
	{
	    var parentMenu = elemwithoutprefix[1].split("-sm");
	    if (parentMenu != null)
	    {
	        for (var i=parentMenu.length-2;i>0;i--)
	        {
		        SetClassName(document.getElementById(parentMenu[i-1] + "-sm" + parentMenu[i]),mnuNum,enabled);
	        }
	        SetClassName(document.getElementById(parentMenu[0]),mnuNum,enabled);
        }	    
	}
	//
	var childID = elemwithoutprefix[1] + "-sm";
	var eltChildID = document.getElementById(childID);
	if (eltChildID != null)
	{
		// make the child menu visible & specify that its position is specified in absolute coordinates
		eltChildID.style.display = 'block';
		eltChildID.style.position = 'absolute';
		if (displayedVertically)
		{
			// Set the child menu's left and top attributes according to the menu's offsets
			var leftPos = (GetAscendingLefts(parent) + parent.offsetWidth - 1);
			if (leftPos + elem.parentNode.offsetWidth>document.body.clientWidth)
			    leftPos = elem.offsetParent.offsetLeft - eltChildID.offsetWidth + 1;
            eltChildID.style.left = leftPos + 'px';
				
			var topPos = GetAscendingTops(elem.parentNode);
			if (displayedDown)
			    topPos += 1;
			else
			    topPos += 1 - (eltChildID.offsetHeight-elem.parentNode.offsetHeight);
		    eltChildID.style.top = topPos + 'px';
		}
		else
		{
			// Set the child menu's left and top attributes according to the menu's offsets
			var leftPos = GetAscendingLefts(elem.parentNode)-1;
			eltChildID.style.left = leftPos + 'px';
			var topPos = GetAscendingTops(parent) + parent.offsetHeight;
			if (displayedDown)
			    topPos -= 1;
			else
			    topPos += 1 - (eltChildID.offsetHeight-elem.parentNode.offsetHeight);
			eltChildID.style.top =  topPos + 'px';
				
			if (eltChildID.offsetWidth < elem.parentNode.offsetWidth)
				eltChildID.style.width = elem.parentNode.offsetWidth + 'px';
			else
			{
				var temp = eltChildID.offsetWidth-elem.parentNode.offsetWidth;
				if (eltChildID.offsetLeft + eltChildID.offsetWidth>document.body.clientWidth)
				{
				    var temp = eltChildID.offsetLeft - (eltChildID.offsetWidth-elem.parentNode.offsetWidth);
					eltChildID.style.left = temp + 'px';
				}
			}
		}
	}
	// set the selected-item style properties, if needed here
	if (enabled)
		elem.parentNode.className=mnuClassItemSelected[mnuNum];
	else
		elem.parentNode.className=mnuClassItemDisabled[mnuNum];
}

// MousedOutMenu
function MousedOutMenu(elem,mnuNum,enabled)
{
	ResetClassName(mnuNum,enabled);
	DoTick(mnuNum);
	// set the unselected-item style properties, if needed
	if (enabled)
		//elem.parentElement.className=mnuClassItem[mnuNum];
		elem.parentNode.className=mnuClassItem[mnuNum];
	else
		//elem.parentElement.className=mnuClassItemDisabled[mnuNum];
		elem.parentNode.className=mnuClassItemDisabled[mnuNum];
}

// CloseSubMenus
function CloseSubMenus(parent,mnuNum)
{
	// Hide **all** lower-ordered submenus
	var parentid = parent.id;
	var elemwithoutprefix = parent.id.split("-X-");
	if ((elemwithoutprefix != null) && (2 == elemwithoutprefix.length))
	    parentid = elemwithoutprefix[1];
    if (null != subMenuIDs[mnuNum])
    {
	    for (var i = 0; i < subMenuIDs[mnuNum].length; i++)
	    {
    		if (subMenuIDs[mnuNum][i].length > parentid.length)
		    {
    			document.getElementById(subMenuIDs[mnuNum][i]).style.display = 'none';
		    }
	    }
    }
}

// GetAscendingLefts
function GetAscendingLefts(obj)
{
    if (obj == null)
        return 0;
        
    var curleft = 0;
    if (obj.offsetParent)
        while (1)
        {
            curleft += obj.offsetLeft;
            if (! obj.offsetParent)
                break ;
            obj = obj.offsetParent;
        }
    else if (obj.x)
        curleft += obj.x;
    return curleft;

//	if (obj == null)
//		return 0;
//	else
//		return obj.offsetLeft + (GetAscendingLefts(obj.offsetParent));
}

// GetAscendingTops
function GetAscendingTops(obj)
{
    if (obj == null)
        return 0;
        
    var curtop = 0;
    if (obj.offsetParent)
        while (1)
        {
            curtop += obj.offsetTop;
            if (! obj.offsetParent)
                break ;
            obj = obj.offsetParent;
        }
    else if (obj.y)
        curtop += obj.y;
    return curtop;
    
//    if (obj == null)
//	    return 0;
//    else
//	    return obj.offsetTop + (GetAscendingTops(obj.offsetParent));
}

// DoTick
function DoTick(mnuNum)
{
	if (clockValues[mnuNum] > delaybounds[mnuNum])
	{
		StopTick(mnuNum);
		CloseSubMenus(document.getElementById(mnuMains[mnuNum]),mnuNum);
	}
	else
	{
		clockValues[mnuNum]++;
		tickers[mnuNum] = setTimeout("DoTick(" + mnuNum + ")", 100);
	}
}

				
// StopTick
function StopTick(mnuNum)
{
	clockValues[mnuNum] = 0;
	clearTimeout(tickers[mnuNum]);
}

// SetClassName
function SetClassName(elemName,mnuNum,enabled)
{
	var elem = document.getElementById(elemName);
	if (elem!=null)
	{
		if (enabled)
		{
			if (elem.tagName=="TABLE")
				elem.children(0).children(0).className=mnuClassItemSelected[mnuNum];
			else if (elem.tagName=="TD")
				elem.parentNode.className=mnuClassItemSelected[mnuNum];
		}
		else
		{
			if (elem.tagName=="TABLE")
				elem.children(0).children(0).className=mnuClassItemDisabled[mnuNum];
			else if (elem.tagName=="TD")
				elem.parentNode.className=mnuClassItemDisabled[mnuNum];
		}
	}
}
				
// ResetClassName
function ResetClassName(mnuNum,enabled)
{
	//var oTR = document.all.tags("TR");
	var oTR = document.getElementsByTagName("TR");
	if (oTR != null)
	{
		for (var i=0;i<oTR.length;i++)
		{
			if (enabled)
			{
				if (oTR[i].className==mnuClassItemSelected[mnuNum])
					oTR[i].className=mnuClassItem[mnuNum];
			}
			else
			{
				if (oTR[i].className==mnuClassItemDisabled[mnuNum])
					oTR[i].className=mnuClassItemDisabled[mnuNum];
			}
		}
	}
}
//]]>
</script>


