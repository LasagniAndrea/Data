// EG 20210401 [25556] Recalcul de la taille des ListBox au resize de la forme lstSelection.aspx
// - lstAvailable, lstSelected et lstGroupBy
function SetLstSelectionResize() {
	// Calcul du viewportHeight
	var vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);
	// Récupération de l'offset top d'une des listes (elles sont alignées)
	var offsetTop = $("#lstAvailable")[0].offsetTop;
	// Calcul de la hauteur d'un item
	var itemsize = $("#lstAvailable")[0][0].clientHeight;
	itemsize = Math.trunc(itemsize);
	// Calcul du nombre d'items visibles dans la liste avant overflow 
	// => Partie entière((Hauteur du client - top de la liste) / itemsize)
	var newsize = Math.trunc((vh - offsetTop) / itemsize) - 5; // Marge pour éviter Scroll de page
	// Mise en application
	$("#lstAvailable").attr("size", newsize);
	$("#lstSelected").attr("size", newsize);
	$("#lstGroupBy").attr("size", newsize);
}


// IsContainOption
function IsContainOption(pList,pOption)
{
    isContain = false;
    //
	for (indexContain=0; indexContain < pList.options.length ; indexContain++)
	{
		if (pList.options[indexContain].text == pOption.text)
		{
		    isContain = true;
		}
	}
	//
	return isContain;
}
//
// IsContainOptionByValue
function IsContainOptionByValue(pList,pOptionValue,pPrefixLength)
{
    isContain = false;
    //
	for (indexContain=0; indexContain < pList.options.length ; indexContain++)
	{
	    var containOptionWithoutPrefix = pList.options[indexContain].value.substr(pPrefixLength,pList.options[indexContain].value.length);
	    if (containOptionWithoutPrefix == pOptionValue)
		{
		    isContain = true;
		}
	}
	//
	return isContain;
}
//
// AddOptGroupChild
function AddOptGroupChild(pList,pOptGroupIndex,pOptChild)
{
    var addOptGrps = pList.getElementsByTagName("optgroup");
	addOptGrps[pOptGroupIndex].appendChild(pOptChild)
}
//
// GetOptionIndex
function GetOptionIndex(pList,pOption)
{
    foundOptionIndex = -1;
    //
	for (indexGet=0; indexGet < pList.options.length ; indexGet++)
	{
		if (pList.options[indexGet].text == pOption.text)
		{
		    foundOptionIndex = indexGet;
		}
	}
	//
	return foundOptionIndex;
}

// Move
// Deplace la(les) ligne(s) sélectionnée(s) du ListBox l1 dans le ListBox l2
function Move(l1,l2)
{
	for (i=0; i < l1.options.length ; i++)
	{
		if (l1.options[i].selected && l1.options[i]!= '')
		{
			o = new Option(l1.options[i].text,l1.options[i].value);
			o.ClassName="ddlCaptureLight";
			l2.options[l2.options.length] = o;
			l1.options[i]                 = null;
			i                             = i -1;
		}
		else
		{
			//alert('Aucun élément sélectionné');
		}
	}
}

// MoveWithPrefix
function MoveWithPrefix(l1,l2,prefix)
{
	var optGrps = l1.getElementsByTagName("optgroup");
    
    for (var i = 0; i < optGrps.length; i++) 
    {
        if(optGrps[i].hasChildNodes)
        {
            var options = optGrps[i].childNodes;            
                
            for (j=0; j < options.length ; j++)
            {	
                var isAlreadyExist = IsContainOptionByValue(l2,options[j].value,prefix.length);
               
                if (options[j].selected && options[j]!= '' && false == isAlreadyExist)
                {      
                    if (1 < optGrps.length)
    	                o = new Option(prefix + '[' + optGrps[i].label + '] ' + options[j].text,prefix + options[j].value);
    	            else    
    	                o = new Option(prefix + ' ' + options[j].text,prefix + options[j].value);
    	            o.Class = "txtCaptureConsult";
	                l2.options[l2.options.length] = o;
	                j = j -1;
                }
                else
                {
	                //alert('Aucun élément sélectionné');
                }
            }
        }
    }
}

// MoveWithoutPrefix
function MoveWithoutPrefix(l1,l2,l3,prefixLength)
{
	for (i=0; i < l1.options.length ; i++)
	{
		if (l1.options[i].selected && l1.options[i]!= '')
		{
		    textWithoutPrefix = l1.options[i].text.substr(prefixLength,l1.options[i].text.length);
		    valueWithoutPrefix = l1.options[i].value.substr(prefixLength,l1.options[i].value.length);
			//
			l1.options[i] = null;
			//
			if(null != l3)
			{
			    opt = new Option(textWithoutPrefix,valueWithoutPrefix);
			    i3 = GetOptionIndex(l3,opt);
			    if (i3 >= 0)
			    {
			        l3.options[i3] = null;
			    }
			}
			//
			i = i -1;
		}
		else
		{
			//alert('Aucun élément sélectionné');
		}
	}
}

// MoveAll
// Deplace toutes les lignes du ListBox l1 dans le ListBox l2
function MoveAll(l1,l2)
{
	for (i=0; i < l1.options.length ; i++)
	{
		o = new Option(l1.options[i].text,l1.options[i].value);
		o.ClassName = "ddlCaptureLight";
		l2.options[l2.options.length] = o;
		l1.options[i] = null;
		i = i -1;
	}
}

// MoveAllWithPrefix
function MoveAllWithPrefix(l1,l2,prefix)
{
    var optGrps = l1.getElementsByTagName("optgroup");
    
    for (var i = 0; i < optGrps.length; i++) 
    {
        if(optGrps[i].hasChildNodes)
        {
            var options = optGrps[i].childNodes;            
                
            for (j=0; j < options.length ; j++)
            {	
                var isAlreadyExist = IsContainOptionByValue(l2,options[j].value,prefix.length);
               
                if (options[j].value !=null && options[j].value !='' && false == isAlreadyExist)
                {                    
                    if (1 < optGrps.length)
	                    o = new Option(prefix + '[' + optGrps[i].label + '] ' + options[j].text,prefix + options[j].value);
	                else
	                    o = new Option(prefix + ' ' + options[j].text,prefix + options[j].value);
	                o.ClassName = "ddlCaptureLight";
	                l2.options[l2.options.length] = o;
	                j = j -1;
                }
            }
        }
    }
}

// MoveAllWithoutPrefix
function MoveAllWithoutPrefix(l1,l2,l3,prefixLength)
{
    for (i=0; i < l1.options.length ; i++)
	{		
	    textWithoutPrefix = l1.options[i].text.substr(prefixLength,l1.options[i].text.length);
	    valueWithoutPrefix = l1.options[i].value.substr(prefixLength,l1.options[i].value.length);
		//
		l1.options[i] = null;
		//
		if(null != l3)
		{
		    opt = new Option(textWithoutPrefix,valueWithoutPrefix);
		    i3 = GetOptionIndex(l3,opt);
		    if (i3 >= 0)
		    {
		        l3.options[i3] = null;
		    }
		}
		//
		i = i -1;
	}
}

// Remove 
// Enleve la(les) ligne(s) selectionnée(s) du ListBox l1 
function Remove(l1)
{
	for ( i=l1.options.length -1; i>=0 ; i--)
	{
		if (l1.options[i].selected && l1.options[i]!= '' )
		{
			l1.options[i] = null;
		}
		else
		{
			//alert('Aucun élément sélectionné');
		}
	}
}

// RemoveAll 
// Toute les lignes du ListBox l1 
function RemoveAll(l1)
{
	for ( i=l1.options.length -1; i>=0 ; i--)
	{
		l1.options[i] = null;
	}
}

// Copy
// Copie la(les) ligne(s) selectionnée(s) du ListBox l1 dans le ListBox l2
function CopyOld(l1,l2)
{
	for (i=0; i < l1.options.length ; i++)
	{
		if (l1.options[i].selected && l1.options[i]!= '')
		{
			o = new Option(l1.options[i].text,l1.options[i].value);
			l2.options[l2.options.length] = o;
		}
		else
		{
			//alert('Aucun élément sélectionné');
		}
	}
}
function Copy(l1,l2)
{
    var optGrps = l1.getElementsByTagName("optgroup");
    
    for (var i = 0; i < optGrps.length; i++) 
    {
        if(optGrps[i].hasChildNodes)
        {
            var options = optGrps[i].childNodes;            
                
            for (j=0; j < options.length ; j++)
            {	
                if (options[j].selected && options[j]!= '')
                {
                    if (1 < optGrps.length)
	                    o = new Option('[' + optGrps[i].label + '] ' + options[j].text,options[j].value);
                    else
                        o = new Option(options[j].text,options[j].value);

                    o.ClassName = "ddlCaptureLight";
	                l2.options[l2.options.length] = o;
                }
                else
                {
	                //alert('Aucun élément sélectionné');
                }
            }
	    }
    }
}
function CopyOptGrp(l1,l2)
{
    var optGrps = l1.getElementsByTagName("optgroup");
    
    for (var i = 0; i < optGrps.length; i++) 
    {
        if(optGrps[i].hasChildNodes)
        {
            var l2OptGroupIndex;
            var isNewOptGroup = false;
            var isOptGroupWithNewChild = false;
            var optGroup = IsContainOptGroup(l2,optGrps[i]);
            
            if(null == optGroup)
            {
                optGroup = document.createElement('optgroup');
                optGroup.label = optGrps[i].label;
                isNewOptGroup = true;
            }
            //
            var options = optGrps[i].childNodes;            
            
	        for (j=0; j < options.length ; j++)
	        {	            
		        if (options[j].selected && options[j]!= '')
		        {
    		        var opt = document.createElement('option');
                    opt.innerHTML = options[j].text;
                    opt.value = options[j].value;
			        optGroup.appendChild(opt);
			        isOptGroupWithNewChild = true;
		        }
		        else
		        {
			        //alert('Aucun élément sélectionné');
		        }
	        }
	        //
	        if(isNewOptGroup && isOptGroupWithNewChild)
	        {
	            l2.appendChild(optGroup);
	        }
	    }
	}
}

// CopyWithoutPrefix
// Copie la(les) ligne(s) selectionnée(s) du ListBox l1 dans le ListBox l2
function CopyWithoutPrefix(l1,l2,prefixLength)
{
    for (i=0; i < l1.options.length ; i++)
	{
	    if (l1.options[i].selected && l1.options[i]!= '')
		{
			o = new Option(l1.options[i].text.substr(prefixLength,l1.options[i].text.length),l1.options[i].value.substr(prefixLength,l1.options[i].value.length));
			o.ClassName = "ddlCaptureLight";
			if(IsContainOption(l2,o) == false)
			{
			    l2.options[l2.options.length] = o;
			}
		}
		else
		{
			//alert('Aucun élément sélectionné');
		}
	}
}

// CopyAll
// Copie toute les lignes du ListBox l1 dans le ListBox l2
function CopyAll(l1,l2)
{
    var optGrps = l1.getElementsByTagName("optgroup");
    
    for (var i = 0; i < optGrps.length; i++) 
    {
        if(optGrps[i].hasChildNodes)
        {
            var options = optGrps[i].childNodes;            
                
            for (j=0; j < options.length ; j++)
            {	    
                if (options[j].value !=null && options[j].value !='')                  
                {  
                    if (1 < optGrps.length)          
                        o = new Option('[' + optGrps[i].label + '] ' + options[j].text,options[j].value);
                    else
                        o = new Option(options[j].text,options[j].value);

                    o.ClassName = "ddlCaptureLight";
                    l2.options[l2.options.length] = o;
                }
            }
	    }
    }
}

// CopyAllWithoutPrefix
// Copie toute les lignes du ListBox l1 dans le ListBox l2
function CopyAllWithoutPrefix(l1,l2,prefixLength)
{
	for (i=0; i < l1.options.length ; i++)
	{
		o = new Option(l1.options[i].text.substr(prefixLength,l1.options[i].text.length),l1.options[i].value.substr(prefixLength,l1.options[i].value.length));
		o.ClassName = "ddlCaptureLight";
		if(IsContainOption(l2,o) == false)
		{
		    l2.options[l2.options.length] = o;
		}
	}
}

// Switch
// Permute l'ordre de la ligne selectionnée au sein du ListBox
function Switch(menu, way)
{
	// Init
	var menumax = menu.length -2;
	var menusel = menu.selectedIndex;
	// Debordement
	if ((menusel < 0) || (menusel < 1 && way == -1) || (menusel > menumax && way == 1)) 
	{ 
		return false;
	}
	// Permutation
	tmpopt                             = new Option( menu.options[menusel+way].text, menu.options[menusel+way].value );
	menu.options[menusel+way].text     = menu.options[menusel].text;
	menu.options[menusel+way].value    = menu.options[menusel].value;
	menu.options[menusel+way].selected = true;
	menu.options[menusel].text         = tmpopt.text;
	menu.options[menusel].value        = tmpopt.value;
	menu.options[menusel].selected     = false;
	return true;
}

// Record
// Enregistre les éléments selectionnés dans le ListBox l1 dans un control h sous la forme : "<value1>|<value2>|<value3>|..."
function Record(l,h)
{
	var selection = '';
	for (i=0; i < l.options.length ; i++)
	{
		selection += l.options[i].value + '|';
	}
	h.value = selection;
}

// SetAscDesc
// Ajoute un prefixe ASC ou DESC
function SetAscDesc(l1,ascDesc)
{
	for (i=0; i < l1.options.length ; i++)
	{
		if (l1.options[i].selected && l1.options[i]!= '')
		{
			var prefix      = 'DESC-';
			var optionText  = l1.options[i].text;
			var optionValue = l1.options[i].value;
			optionText      = optionText.substr(5,optionText.length);
			optionValue     = optionValue.substr(5,optionValue.length);
			if (ascDesc != 'DESC')
			{
				prefix = 'ASC -';
			}
			l1.options[i].text  = prefix + optionText;
			l1.options[i].value = prefix + optionValue;
		}
		else
		{
			// alert('Aucun élément sélectionné');
		}
	}
}
