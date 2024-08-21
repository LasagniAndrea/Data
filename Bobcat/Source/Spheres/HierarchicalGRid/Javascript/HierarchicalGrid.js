<script type="text/javascript">
//<![CDATA[
function HierarchicalGrid_ExpandCollapseAll(sender)
{
	if(sender == null)
		return;
	// Conformite XHTML tous browser (class) / IE7 (className)
	var childNode = sender.childNodes[0];
	var className = childNode.getAttribute("class");

	var div = document.getElementsByTagName("div");
	for (var i=0;i<div.length;i++)
	{
        if ((-1 < div[i].className.indexOf("plus")) && (-1 < className.indexOf("plus")))
                HierarchicalGrid_toggleRow(div[i]);
 		if ((-1 < div[i].className.indexOf("minus")) && (-1 < className.indexOf("minus")))
	   	        HierarchicalGrid_toggleRow(div[i]);
	}
   
	if (-1 < className.indexOf("plus"))
	{
		childNode.className = childNode.className.replace("plus","minus")
	}
	else if (-1 < className.indexOf("minus"))
	{
		childNode.className = childNode.className.replace("minus", "plus")
	}
	
}

function HierarchicalGrid_toggleRow(sender)
{
	if(sender == null)
		return;

	var state = 1;
	//if the hidden row has not already been generated, clone the panel into a new row
	var existingRow = window.document.getElementById(sender.id + "showRow");
	if (existingRow==null)
	{
		//getting a reference to the table
		var table = GetParentElementByTagName(sender, "table");
		var index = GetParentElementByTagName(sender, "tr").sectionRowIndex;
		//concatenate name of hidden panel => replace "Expand" from sender.id with "Panel"\n
		rowDivName = HierarchicalGrid_ReplaceStr(sender.id, "Expand", "pnl");
		var rowDiv = window.document.getElementById(rowDivName);
		//adding new row to table
		var newRow = table.insertRow(index+1);
		newRow.id = sender.id + "showRow";
		//adding new cell to row
		var newTD=document.createElement("td");
		if(table.rows[index].cells[0].colSpan > 1)
			newTD.colSpan = table.rows[index].cells[0].colSpan;
		else
			newTD.colSpan = table.rows[index].cells.length;	
		var myTD = newRow.appendChild(newTD);
		//clone Panel into new cell
		var copy = rowDiv.cloneNode(false);
		copy.innerHTML = rowDiv.innerHTML;
		copy.style.display = "";
		myTD.innerHTML = copy.innerHTML;
		rowDiv.parentNode.removeChild(rowDiv);
		sender.className = sender.className.replace("plus", "minus");
		state = 1;
	}
	else
	{
		if (existingRow.style.display=="none")
		{
			existingRow.style.display = "";
			sender.className = sender.className.replace("plus", "minus");
			state = 1;
		}
		else
		{
			existingRow.style.display = "none";
			sender.className = sender.className.replace("minus", "plus");
			state = 0;
		}
	}
	ChangeRowState(sender, state);
}

function HierarchicalGrid_ReplaceStr(orgString, findString, replString)
{
	pos = orgString.lastIndexOf(findString);
	return orgString.substr(0, pos) + replString + orgString.substr(pos + findString.length);
}

function GetParentElementByTagName(element, tagName)
{
	var element=element;
	while(element.tagName.toLowerCase() != tagName)
	{
		element = element.parentNode;
	}
	return element;
}

function ChangeRowState(sender, state)
{
	var table = GetParentElementByTagName(sender, "table");
	var hiddenfield = table.getAttribute("Expand_ClientIDs");
	var rowStates = document.getElementsByName(hiddenfield)[0].value;


	if(state == 1)
	{
		if(rowStates.indexOf(sender.id) == -1)
			rowStates += ", " + sender.id;
	}
	else if(state == 0)
	{
		rowStates = rowStates.replace(sender.id, "");
	}
	document.getElementsByName(hiddenfield)[0].value = rowStates;
}

function OnPrint()
{
	SetDivStyleOverflow("auto","visible");
	window.print();
	SetDivStyleOverflow("visible","auto");		
}	

function SetDivStyleOverflow(before,after)
{
	var table = document.getElementById("tblHgMaster");
	var div   = table.getElementsByTagName("div");
	for (var i=0;i<div.length;i++)
	{
		if (div[i].style.overflow == before)
			div[i].style.overflow = after;
	}
}	
//]]>
</script>

