function Set(objTargetID, svalue, condition)
{
 if (condition)
  {
    var objRef = document.getElementById(objTargetID);
    if(objRef != null)
    {
      if (objRef.type=='checkbox' || objRef.type=='radio' )
      {
		   objRef.checked = (svalue=='true');
      }
      else
      {
          objRef.value = svalue;
      }
    }
  }
}

function Reset(objTargetID, condition)
{
  Set(objTargetID, '', condition);
}

function IsEqualValue(objRef, svalue, condition)
{
  var ret = false;
  if (condition)
  {
    if (objRef.type=='checkbox' || objRef.type=='radio' )
    {
      ret = (objRef.checked==(svalue=='true'));
    }
    else
    {
      ret = (objRef.value.replace(/(^\s*)|(\s*$)/g,'') == svalue);
    }
  }
  return ret;
}

function EnabledDisabled(objCaller,objRefIDName,compValue,isEnabled,condition)
{
	var objRef  = document.getElementById(objRefIDName);
	var enabled = false;
	if (objRef!=null)
	{
	    if (objCaller.type=='checkbox')
	    {
		    enabled=(objCaller.checked && 'true'==compValue) || condition;
	    }
	    else
	    {
		    var data = objCaller.value;
		    data = data.replace(/(^\s*)|(\s*$)/g,'');
		    enabled=(data==compValue) || condition;
	    }
		if (isEnabled)
			objRef.disabled= !(enabled);
		else
			objRef.disabled= enabled;
		
	    if (objRef.type=='radio')
	    {
	        var objRef2 = document.getElementById(objRefIDName+'2');
	        if (objRef2!=null)
			    objRef2.disabled=objRef.disabled;
	    }
	}
	return (enabled);
}

function UpperLower(objRef,svalue)
{				
	if(objRef!=null)
	{
		if (svalue == 'upper')
			objRef.value = objRef.value.toUpperCase();
		else if (svalue=='lower')
			objRef.value = objRef.value.toLowerCase();
		else if (svalue=='1stupper')
			objRef.value = objRef.value.substr(0,1).toUpperCase() + objRef.value.substring(1,objRef.value.length);
		else
			objRef.value = objRef.value.toUpperCase();
	}		
	return false;
}

function SetStatus(msg)
{
	window.status = msg;
	return false;
}

function Copy(objCaller,objTargetID,args)
{
    if (objCaller.value == null)
	    return false;
    var objRef = document.getElementById(objTargetID);
    if(objRef!=null)
    {
		if ((objRef.value.length >= 1) && (args=='new'))
			return false;
		if (args=='append')
			objRef.value += objCaller.value;
		else
			objRef.value = objCaller.value;
    }
    return false;
} 
// FI 20210202 [XXXXX] add
// Autocomplete on Referential Column Relation
function LoadAutoCompleteReferentialColumnRelation() {
    $(".rcr-autocomplete").autocomplete({
        source: function (request, response) {
            var controlTXTId = $(this)[0].element[0].id; // Control with autocomplete
            $.ajax({
                url: "Referential.aspx/LoadAutoCompleteReferentialColumnRelation",
                data: "{ 'request': '" + request.term + "','guid': '" + $("#__GUID").val() + "','controlId': '" + controlTXTId + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset = utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            id: item.id,
                            value: item.identifier
                        }
                    }))
                },
                error: AutocompleteError
            });
        },
        select: function (event, ui) {
            var controlTXTId = $(this)[0].id; // Control with autocomplete 
            SetReferentialColumnRelation(controlTXTId, ui.item.id, ui.item.value);
        },
        minLength: 1
    });
}


// FI 20210202 [XXXXX] add
// Clear when inconsistent values are detected (Protection against copy/paste user actions)
function CheckAutoCompleteReferentialColumnRelation() {
    $(".rcr-autocomplete").change(function () {
        var controlTXTId = $(this)[0].id; // Control with autocomplete 
        var controlHDN = controlTXTId.replace('TXT', 'HDN');
        if ($("#" + controlTXTId).val().length == 0) {
            ClearAutocompleteInput(controlTXTId);
        }
        else if ($("#" + controlHDN).val().length == 0) {
            ClearAutocompleteInput(controlTXTId);
        }
        else {
            $.ajax({
                url: "Referential.aspx/CheckAutoCompleteReferentialColumnRelation",
                data: "{'guid': '" + $("#__GUID").val() + "','controlId': '" + controlTXTId + "','id': '" + $("#" + controlHDN).val() + "','identifier': '" + $("#" + controlTXTId).val() + "'}",
                dataType: "json",
                type: "POST",
                async: false, // needed because some input are autopostback
                contentType: "application/json; charset = utf-8",
                success: function (data) {
                    if (data.d == false) {
                        ClearAutocompleteInput(controlTXTId);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert("Error: " + jqXHR.responseText);
                }
            })
        }
    });
}

// FI 20210202 [XXXXX] add
function ClearAutocompleteInput(controlTXTId) {
    SetReferentialColumnRelation(controlTXTId, '', '');
}

// FI 20210219 [XXXXX] add
function GetIdAutocompleteInput(controlTXTId) {
    return GetControlHiddenAutocompleteInput(controlTXTId).val();
}

// FI 20210219 [XXXXX] add
function GetControlHiddenAutocompleteInput(controlTXTId) {
    let controlHDN = controlTXTId.replace('TXT', 'HDN');
    return $(`#${controlHDN}`)
}

// FI 20210202 [XXXXX] add
function SetReferentialColumnRelation(controlTXTId, id, identifier) {
    var controlHDN = controlTXTId.replace('TXT', 'HDN');
    $("#" + controlHDN).val(id);
    $("#" + controlTXTId).val(identifier);
}

// FI 20210208 [XXXXX] add
// Autocomplete on Referential Column
function LoadAutoCompleteReferentialColumn() {
    $(".rc-autocomplete").autocomplete({
        source: function (request, response) {
            var controlTXTId = $(this)[0].element[0].id; // Control with autocomplete
            var array = [];
            $("input[id^='TXT'],input[id^='CHK'],select[id^='DDL'],span[id^='DDL'][ddlvalue]").each(function () {
                let val;
                if ($(this).is('span')) {
                    val = $(this).attr("ddlvalue");
                }
                else {
                    val = $(this).val();
                }
                array.push({
                    id: $(this)[0].id,
                    value: val
                });
            });
            var controlValues = JSON.stringify(array); // Controls of the form

            $.ajax({
                url: "Referential.aspx/LoadAutoCompleteReferentialColumn",
                data: "{ 'request': '" + request.term + "','guid': '" + $("#__GUID").val() + "','controlId': '" + controlTXTId + "','controlValues': " + controlValues + "}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset = utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            value: item
                        }
                    }))
                },
                error: AutocompleteError
            });
        },
        select: function (event, ui) {
            var controlTXTId = $(this)[0].id; // Control with autocomplete 
            $("#" + controlTXTId).val(ui.value);
        },
        minLength: 1
    });
}

// FI 20210116 [XXXXX] add
// Add Method (Must be called when AutoCompleteReferentialColumnRelation depends on some input data (controlArray))
function SyncAutoCompleteReferentialColumnRelation(controlArray) {
    let jsonControls = JSON.stringify(controlArray);
    $.ajax({
        url: "Referential.aspx/SyncAutoCompleteReferentialColumnRelation",
        data: "{'guid': '" + $("#__GUID").val() + "','controls': " + jsonControls + "}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        }
    });
}

// FI 20210116 [XXXXX] add
// arrayJSON is the result of LoadDataTable (First column: Value, Second column: Text )
function LoadDDL(controlId, arrayJSON, isAddEmpty = false) {
    $(`#${controlId}`).empty();
    if (Array.isArray(arrayJSON) && arrayJSON.length > 0) {
        if (isAddEmpty) {
            $(`#${controlId}`).append(new Option('', ''));
        }

        arrayJSON.forEach(function (item) {
            let itemValue = Object.entries(item)[0][1];
            let itemText = Object.entries(item)[1][1];
            $(`#${controlId}`).append(new Option(itemText, itemValue));
        });

        OrderDDL(controlId);
    }
}

// FI 20220503 [XXXXX] add
function OrderDDL(controlId) {
    var options = $(`#${controlId} option`)
    var arr = options.map(function (_, o) { return { t: $(o).text(), v: o.value }; }).get();
    arr.sort(function (o1, o2) { return o1.t > o2.t ? 1 : o1.t < o2.t ? -1 : 0; });
    options.each(function (i, o) {
        o.value = arr[i].v;
        $(o).text(arr[i].t);
    })
}

// -->
//</script>