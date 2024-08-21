// OpenReferential
// FI 2019-12-27 [XXXXX] Warning Toute évolution de cette méthode a des impactes sur le web service ReferentialWebService.LoadAutoCompleteData
function OpenReferential(XMLName, title, subTitle, listType, isFiltered, type_KeyField, clientIdForDumpKeyField, sqlColumn, clientIdForDumpSqlColumn, condApp, valueFK, listParam, dynamicArg){
    
    var form = document.forms[0];
	var clientIdForDump = clientIdForDumpKeyField;
	//
	var urlToOpen = "List.aspx?" + listType + "=" + XMLName;
	urlToOpen    += "&InputMode=1";
	urlToOpen    += "&OpenerKeyId=" + clientIdForDumpKeyField;
	urlToOpen    += "&TypeKeyField=" + type_KeyField;
        
	if (valueFK != null)
		urlToOpen += "&FK=" + valueFK;
		
	if (condApp != null)
		urlToOpen += "&CondApp=" + condApp;
		
	if (title != null)
		urlToOpen += "&TitleMenu=" + title;
		
	if (subTitle != null)
		urlToOpen += "&SubTitle=" + subTitle;
		
    //if (clientIdForDumpSqlColumn != null && sqlColumn != null)
    //{
    //	urlToOpen      += "&OpenerSpecifiedId=" + clientIdForDumpSqlColumn;
    //	urlToOpen      += "&OpenerSpecifiedSQLField=" + sqlColumn;
    //	clientIdForDump = clientIdForDumpSqlColumn;
    //}
    //2009-04-30 PL Refactoring
	if (sqlColumn != null)
	{
		if (clientIdForDumpSqlColumn != null)
		    clientIdForDump = clientIdForDumpSqlColumn;
        urlToOpen += "&OpenerSpecifiedId=" + clientIdForDump;
        urlToOpen += "&OpenerSpecifiedSQLField=" + sqlColumn;
	}
	//
	if (isFiltered != null && isFiltered == true)
	{
		var form    = document.forms[0];
		var ctrlKey = form.elements[clientIdForDump];
		if (ctrlKey != null)
		{
			var ctrlValue = ctrlKey.value;
			urlToOpen    += "&ValueForFilter=" + ctrlValue;
		}
	}
	//	
	if (listParam != null)
	{
		var paramArray = listParam.split(';');
		for(i=0;i<paramArray.length;i++)
		{
			urlToOpen += "&P" + (i+1).toString() + "=" + paramArray[i];
		}
	}
	//
	if (dynamicArg != null)
	{
	    urlToOpen += "&DA=" + dynamicArg;
	}
	// FI 2023 [XXXXX] use paramters target and features
	//ret = window.open(urlToOpen, "referential" + listType + XMLName ,  "fullscreen=no,menubar=no,resizable=yes,scrollbars=yes,status=no,toolbar=no,location=no");
	ret = window.open(urlToOpen, "referential" + listType + XMLName ,  "popup=true");
}			
		
// OpenerGetReferential
function OpenerGetReferential(openerCtrlIdKeyField, KeyFieldValue,openerCtrlIdSpecifiedField, SpecifiedFieldValue){
	if (openerCtrlIdKeyField != null)
		OpenerGetReferential_SetDataToControl(openerCtrlIdKeyField, KeyFieldValue);

	if (openerCtrlIdSpecifiedField != null)
		OpenerGetReferential_SetDataToControl(openerCtrlIdSpecifiedField, SpecifiedFieldValue);
}

// OpenerGetReferential_SetDataToControl
// FI 2018-01-30 [23749] Modify
function OpenerGetReferential_SetDataToControl(CtrlIdField, FieldValue) {
    var form = document.forms[0];   
    var ctrlKey = form.elements[CtrlIdField];
    if (ctrlKey != null) {
        ctrlKey.focus();
        var ctrlKeyValue = ctrlKey.value;
        if ((ctrlKeyValue.charAt(ctrlKeyValue.length - 1) != ';') &&
		    (ctrlKeyValue.length > 1) &&
		    (ctrlKeyValue.lastIndexOf(';', ctrlKeyValue.Length - 2) >= 0)) {
            ctrlKeyValue = ctrlKeyValue.substr(0, ctrlKeyValue.lastIndexOf(';', ctrlKeyValue.length - 2) + 1);
        }

        if ((ctrlKeyValue.length > 0) && (ctrlKeyValue.charAt(ctrlKeyValue.length - 1) == ';')) {
            ctrlKey.value = ctrlKeyValue +FieldValue;
        }
        else {
            if (ctrlKey.value != FieldValue) {
                ctrlKey.value = FieldValue;
                // FI 2018-01-30 [23749] ctrlKey.onchange() not called 
                //ctrlKey.onchange(); //PL 20170628
                if (form.name == "CCIPage") {
                    // FI 2018-01-30 [23749] call OnCtrlChanged on cciPage
                    OnCtrlChanged(CtrlIdField,'');
                }
             }
        }
    }
}


function OpenReferentialForAutocomplete(openReferentialclick) {
    //$.ajax call doesn't support ' and \x
    //Suppress of '
    var ret = openReferentialclick.replace(new RegExp("'", 'g'), '');
    //Replace of code hex \x22 par "
    while (ret.indexOf('\\x22')>-1) {
        ret = ret.replace('\\x22', '"');
    }
    ret = ret.replace(';return false;', '')
    return ret;
}

function OpenUrl(url, openerKeyId, openerKeyIdentifier) {
	var form = document.forms[0];
	var ctrlKey = form.elements[openerKeyId];
	var ctrlIdentifier = form.elements[openerKeyIdentifier];

	if (ctrlKey != null) {
		url = url.replace('{0}', ctrlKey.value);
		if (ctrlIdentifier != null)
			url = url.replace('{1}', ctrlIdentifier.value);
		else
			url = url.replace('{1}', ctrlKey.value);
	}
	window.open(url, '_blank');
}

function OpenAttachedDoc(url, openerKeyId, openerKeyIdentifier) { OpenUrl(url, openerKeyId, openerKeyIdentifier); }
function OpenNotepad(url, openerKeyId, openerKeyIdentifier) { OpenUrl(url, openerKeyId, openerKeyIdentifier); }

function GetReferential(openerCtrlIdKeyField, KeyFieldValue, openerCtrlIdSpecifiedField, SpecifiedFieldValue) {
	if (null != window.opener)
		window.opener.OpenerGetReferential(openerCtrlIdKeyField,KeyFieldValue,openerCtrlIdSpecifiedField, SpecifiedFieldValue);
	self.close();
}
