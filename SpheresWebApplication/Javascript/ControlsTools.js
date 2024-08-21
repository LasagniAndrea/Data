// Fichier JScript

function DDLSetTextOnToolTip (e)
{
    try
    {
        var obj = e;
        if ( null == obj)
            obj = window.event;
        if (null != obj)
        {
            if (obj.target) 
                targ = obj.target;
            else if (obj.srcElement) 
                targ = obj.srcElement;
            //
            if (null!=targ)
            {
                var ddl = targ;
                ddl.title = "";
                if (ddl.selectedIndex>=0)
                    ddl.title= ddl.options[ddl.selectedIndex].text;
            }
        }
    }
    catch(err)
    {
        // Throw a generic error with a message
        throw Error.create("DDLSetTextOnToolTip : " + err.message)
    }
}

function RefreshControlClassBuySell(pControlId, pValue)
{
    try
    {
        var css = null;
        if (pValue == "BUY")
            css = "PnlRoundedBuyer";
        else if (pValue == "SELL")
            css = "PnlRoundedSeller";
        //
        if (null != css)
        {
            var control = $get(pControlId);
            //
            if (null!=table)
                control.className = css;
        }
    }
    catch(err)
    {
        // Throw a generic error with a message
        throw Error.create("RefreshTableClassBuySell : " + err.message)
    }
}

// EG 2019-01-23 New
function DataGridSelectAll(pCBSelectAll, pDgID, pCBSelectID) {
    // DataGrid
    var dg = document.getElementById(pDgID);

    $(dg).find("input[type='checkbox']:first").prop("indeterminate", pCBSelectAll.name.indexOf('cbSelectAllOnCurrentPage') > 0);
    $(dg).find("input[type='checkbox']").eq(1).prop("indeterminate", false);

    $(dg).find("input[type='checkbox']:not(:first)").each(function () {
        if (-1 < $(this).attr("id").indexOf(pCBSelectID))
            $(this).prop("checked", pCBSelectAll.checked);
    });
}

// EG 2019-01-23 New
function DataGridCheckedChanged(pDgID, pCBSelectIDAll, pCBSelectID) {
    // DataGrid
    var dg = document.getElementById(pDgID);
    var boolAllChecked = true;
    var countChecked = 0;
    var countUnchecked = 0;

    // Check Header Exclude (Slice(2)
    $(dg).find("input[type='checkbox']").slice(2).each(function () {
        if (-1 < $(this).attr("id").indexOf(pCBSelectID)) {
            if (false == $(this).is(":checked"))
                countChecked++;
            else
                countUnchecked++;

            if ((countChecked > 0) && (countUnchecked > 0))
                return false;
        }
    });

    if ($(dg).find("input[type='checkbox']:eq(1)").attr("id").indexOf(pCBSelectIDAll) > -1) {
        $(dg).find("input[type='checkbox']:eq(1)").prop("indeterminate", (countChecked > 0) && (countUnchecked > 0));
        $(dg).find("input[type='checkbox']:eq(1)").prop("checked", (countChecked > 0) && (countUnchecked == 0));
    }

    if ($(dg).find("input[type='checkbox']:eq(0)").attr("id").indexOf(pCBSelectIDAll.replace('OnCurrentPage', 'OnAllPages')) > -1) {
        $(dg).find("input[type='checkbox']:eq(0)").prop("indeterminate", true);
    }
}


