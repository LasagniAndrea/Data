function GridViewCheckedChanged(pGvID, pCBSelectIDAll, pCBSelectID) {
    var boolAllChecked = true;
    // GridView
    var gv = document.getElementById(pGvID);
    // Without Header
    $(gv).find("input[type='checkbox']").each(function () {
        if (-1 < $(this).attr("id").indexOf(pCBSelectID)) {
            if (false == $(this).is(":checked"))
                boolAllChecked = false;
        }
    });
    //// Only Header
    $(gv).find("input[type='checkbox']").each(function () {
        if (-1 < $(this).attr("id").indexOf(pCBSelectIDAll)) {
            $(this).prop("checked", boolAllChecked);
        }
    });
}

function GridViewSelectAll(pCBSelectAll, pGvID, pCBSelectID) {
    // GridView
    var gv = document.getElementById(pGvID);
    $(gv).find("input[type='checkbox']").each(function () {
        if (-1 < $(this).attr("id").indexOf(pCBSelectID))
            $(this).prop("checked", pCBSelectAll.checked);
    });
}

function DDLSetTextOnToolTip(e) {
    try {
        var obj = e;
        if (null == obj)
            obj = window.event;
        if (null != obj) {
            if (obj.target)
                targ = obj.target;
            else if (obj.srcElement)
                targ = obj.srcElement;

            if (null != targ) {
                var ddl = targ;
                ddl.title = "";
                if (ddl.selectedIndex >= 0)
                    ddl.title = ddl.options[ddl.selectedIndex].text;
            }
        }
    }
    catch (err) {
        // Throw a generic error with a message
        throw Error.create("DDLSetTextOnToolTip : " + err.message)
    }
}


// OpenReferential
function OpenReferential(pXMLName, pTitle, pSubTitle, pListType, pIsFiltered, pTypeKeyField, pClientIdForDumpKeyField, pSqlColumn, pClientIdForDumpSqlColumn,
    pCondApp, pValueFK, pListParam, pDynamicArg) {

    var form = document.forms[0];
    var clientIdForDump = clientIdForDumpKeyField;
    //
    var urlToOpen = "ListViewer.aspx?" + pListType + "=" + pXMLName + "&InputMode=1" + "&OpenerKeyId=" + pClientIdForDumpKeyField + "&TypeKeyField=" + pTypeKeyField;

    if (pValueFK != null)
        urlToOpen += "&FK=" + pValueFK;

    if (pCondApp != null)
        urlToOpen += "&CondApp=" + pCondApp;

    if (pTitle != null)
        urlToOpen += "&TitleMenu=" + pTitle;

    if (pSubTitle != null)
        urlToOpen += "&SubTitle=" + pSubTitle;

    if (pSqlColumn != null) {
        if (pClientIdForDumpSqlColumn != null)
            clientIdForDump = pClientIdForDumpSqlColumn;
        urlToOpen += "&OpenerSpecifiedId=" + clientIdForDump;
        urlToOpen += "&OpenerSpecifiedSQLField=" + pSqlColumn;
    }

    if (isFiltered != null && isFiltered == true) {
        var ctrlKey = document.getElementById(clientIdForDump);
        if (ctrlKey != null) {
            urlToOpen += "&ValueForFilter=" + ctrlKey.value;
        }
    }

    if (pListParam != null) {
        var paramArray = pListParam.split(';');
        for (i = 0; i < paramArray.length; i++) {
            urlToOpen += "&P" + (i + 1).toString() + "=" + paramArray[i];
        }
    }

    if (pDynamicArg != null) {
        urlToOpen += "&DA=" + pDynamicArg;
    }
    ret = window.open(urlToOpen);
}

// GetReferential
function GetReferential(pOpenerCtrlIdKeyField, pKeyFieldValue, pOpenerCtrlIdSpecifiedField, pSpecifiedFieldValue) {
    if (null != window.opener)
        window.opener.OpenerGetReferential(pOpenerCtrlIdKeyField, pKeyFieldValue, pOpenerCtrlIdSpecifiedField, pSpecifiedFieldValue);
    self.close();
}

// OpenerGetReferential
function OpenerGetReferential(pOpenerCtrlIdKeyField, pKeyFieldValue, pOpenerCtrlIdSpecifiedField, pSpecifiedFieldValue) {
    if (null != pOpenerCtrlIdKeyField)
        OpenerGetReferential_SetDataToControl(pOpenerCtrlIdKeyField, pKeyFieldValue);

    if (null != pOpenerCtrlIdSpecifiedField)
        OpenerGetReferential_SetDataToControl(pOpenerCtrlIdSpecifiedField, pSpecifiedFieldValue);
}

// OpenerGetReferential_SetDataToControl
function OpenerGetReferential_SetDataToControl(pCtrlIdField, pFieldValue) {
    var ctrlKey = document.getElementById(pCtrlIdField);
    if (null != ctrlKey) {
        ctrlKey.focus();
        var ctrlKeyValue = ctrlKey.value;
        if ((ctrlKeyValue.charAt(ctrlKeyValue.length - 1) != ';') &&
		    (ctrlKeyValue.length > 1) &&
		    (ctrlKeyValue.lastIndexOf(';', ctrlKeyValue.Length - 2) >= 0)) {
            ctrlKeyValue = ctrlKeyValue.substr(0, ctrlKeyValue.lastIndexOf(';', ctrlKeyValue.length - 2) + 1);
        }

        if ((ctrlKeyValue.length > 0) && (ctrlKeyValue.charAt(ctrlKeyValue.length - 1) == ';')) {
            ctrlKey.value = ctrlKeyValue + pFieldValue;
        }
        else if (ctrlKey.value != pFieldValue) {
                ctrlKey.value = pFieldValue;
                ctrlKey.onchange(); //PL 20170628
        }
    }
}

function OnCtrlChanged(callEvent, callArgument) {
    var ctrl = document.getElementById(callEvent);
    if (null != ctrl) {
        var newValue = null;
        if ((ctrl.type == 'select-one') && (ctrl.selectedIndex != -1)) {
            newValue = ctrl.options[ctrl.selectedIndex].text;
        }
        else {
            ctrlText = ctrl.value;
        }
        var oldValue = $(ctrl).attr("oldValue");
        if ((oldValue == null) || (oldValue != newValue)) {
            PostBack(callEvent, callArgument);
        }
    }
}


function RefreshControlClassBuySell(pControlId, pValue) {
    try {
        var css = null;
        if (pValue == "BUY")
            css = "PnlRoundedBuyer";
        else if (pValue == "SELL")
            css = "PnlRoundedSeller";

        if (null != css) {
            var control = $get(pControlId);
            if (null != table)
                control.className = css;
        }
    }
    catch (err) {
        // Throw a generic error with a message
        throw Error.create("RefreshTableClassBuySell : " + err.message)
    }
}

// RecordDisplay
// Enregistre les éléments selectionnés dans la bulletedList (ul/li) dans le control h sous la forme : "<value1>|<value2>|<value3>|..."
function RecordDisplay(sourceID, targetID) {
    var selection = '';

    if (-1 < sourceID.toLowerCase().indexOf("sorted")) {
        $(sourceID).find("li").each(function () {
            selection += $(this).attr("value") + ';'; // + $(this).attr("sort") + ";";
            if ($(this).find("input[type='checkbox'][id^='blColumnSorted_sort']").is(':checked')) {
                selection += 'true';
            }
            else {
                selection += 'false';
            }
            selection += ';';
            if ($(this).find("input[type='checkbox'][id^='blColumnSorted_groupby']").is(':checked')) {
                selection += 'true';
            }
            else {
                selection += 'false';
            }
            selection += '|';
        });
    }
    else {
        $(sourceID).find("li").each(function () {
            selection += $(this).attr("value") + '|';
        });

    }
    $(targetID).val(selection);
}

// RecordFilter
// Enregistre les colonnes sélectionnée du filtre xxx_ddlCol0..n dans le control targetID sous la forme : "<grp><value><data-type>|<grp><value><data-type>" de xxx_ddlCol0..n
// Si isSqlSource alors <value> = mandatory;tableName;columnName;alias;datatype
// Si isXMLSource alors <value> = mandatory;-1;alias;datatype;tableName;columnName;tableRelation;columnRelation;columnSelect;typeRelation
function RecordFilter(targetID) {

    var selection = '';

    $('[id^="mc_uc_lstfilter_ddlCol"]').each(function (e) {
        $(this).find("option:selected").each(function () {
            if ($(this).attr("data-type")) {
                var grp = "ALL";
                if ($(this).attr("grp"))
                    grp = $(this).attr("grp");
                selection += grp + ";" + $(this).attr("value") + ";" + $(this).attr("data-type");
            }
        });
        selection += '|';
    });
    $(targetID).val(selection);
}

function ResetFilter() {

    $("#mc_lstCriteria_Filter .row").each(function (e) {
        $(this).find("button[data-toggle='dropdown']").removeAttr("value").html('');
        $(this).find("li").removeClass("active");
        $(this).find("option:selected").prop("selected", false);
        $(this).find("input[type='text']").val('');
    });
}


// Redimensionnemnt du container du GridView (gvContent)
function GridViewHeight() {

    // Calcul Hauteur du container du gridView (#gvContent) :
    //    = Hauteur de son container (#lstBody) - Hauteur du container des critères (#mc_lstInfo) - 10
    // NB : les containers invisibles ont position : absolute
    $("#gvContent").height($("#lstBody").height() - $("#mc_lstInfo").height() - 10);
}




// Affichage des custom objects, critères de filtre , colonnes affichées et tri (mode CONSULT)
function DisplayListInfo(suffixID) {
    if ($("#mc_lstCriteria_" + suffixID).hasClass("invisible")) {
        $("#mc_lstInfo > div").addClass("invisible");
        $("#mc_lstCriteria_" + suffixID).removeClass("invisible");
    }
    else {
        $("#mc_lstInfo > div").addClass("invisible");
        $("#mc_lstCustomObjects").removeClass("invisible");
        $("#mc_lstFilterReadOnly").removeClass("invisible");
    }
    $("#mc_lstInfo").removeClass("invisible");
    if (0 == $("#mc_lstInfo > div:not(.invisible)").length)
        $("#mc_lstInfo").addClass("invisible");

    GridViewHeight();
}

function DisplayColumnByGroup() {
    var group = $("#mc_uc_lstdisplay_ddlGroupAvailable option:selected").val();
    $('#mc_uc_lstdisplay_blColumnAvailable > li').each(function () {
        if ($(this).attr("grp") == group) {
            $(this).removeClass("hidden");
        }
        else {
            $(this).addClass("hidden");
        }
    });
}

function IsOperandEnabled(operand, datatype) {
    var isEnabled = true;
    switch (operand) {
        case "checked":
        case "unchecked":
            isEnabled = (datatype == "bool") || (datatype == "bool2v") || (datatype == "bool2h") || (datatype == "boolean");
            break;
        case "contains":
        case "notcontains":
        case "startswith":
        case "endswith":
            isEnabled = (datatype == "string") || (datatype == "text");
            break;
        case "like":
        case "notlike":
            isEnabled = (datatype != "date") && (datatype != "datetime") &&
                        (datatype != "bool") && (datatype != "bool2v") && (datatype != "bool2h") && (datatype != "boolean");
            break;
        case "equalto":
        case "notequalto":
        case "lessthan":
        case "greaterthan":
        case "lessorequalto":
        case "greaterorequalto":
            isEnabled = (datatype != "text") &&
                        (datatype != "bool") && (datatype != "bool2v") && (datatype != "bool2h") && (datatype != "boolean");
            break;
    }
    return isEnabled;

}

$(document).ready(function () {

    DisplayColumnByGroup();
    GridViewHeight();
    $(window).on('resize', function () {

        GridViewHeight();

        if ($('.modal.in').length != 0) {
            setModalMaxHeight($('.modal.in'));
        }

    });

    $('#mc_btnDisplay').click(function () {
        DisplayListInfo("Display");
    });

    $('#btnFilter').click(function () {
        DisplayListInfo("Filter");
    });

    $('#mc_uc_lstdisplay_ddlGroupAvailable').change(function () {
        DisplayColumnByGroup();
    });

    $('#mc_uc_lstfilter_btnReset').click(function () {
        ResetFilter();
    });

    $(function () {

        var available = $("#mc_uc_lstdisplay_blColumnAvailable");
        var displayed = $("#mc_uc_lstdisplay_blColumnDisplayed");
        var sorted = $("#mc_uc_lstdisplay_blColumnSorted");

        available.on('click', 'li', function (e) {
            if (e.ctrlKey || e.metaKey) {
                $(this).toggleClass("selected");
            } else {
                $(this).addClass("selected").siblings().removeClass('selected');
            }
        });

        $("#mc_uc_lstdisplay_blColumnAvailable > li").draggable({
            connectToSortable: "#mc_uc_lstdisplay_blColumnDisplayed, #mc_uc_lstdisplay_blColumnSorted",
            helper: "clone",
            start: function (event, ui) {
                $(ui.helper).width('100%');
            }
        });

        displayed.on('click', 'li', function (e) {
            if (e.ctrlKey || e.metaKey) {
                $(this).toggleClass("selected");
            } else {
                $(this).addClass("selected").siblings().removeClass('selected');
            }
        }).sortable({
            connectWith: "#mc_uc_lstdisplay_blColumnDisplayed",
            placeholder: "sortable-placeholder",
            receive: function (event, ui) {
                $(this).find(".placeholder").remove();
                var link = $("<a href='#'>x</a>");
                $(ui.helper).addClass("list-group-item ui-sortable-handle");
                $(ui.helper).removeAttr("style");
                $(ui.helper).append(link);
            }
        }).on("click", "a", function (event) {
            event.preventDefault();
            $(this).parent().remove();
        });

        sorted.on('click', 'li', function (e) {
            if (e.ctrlKey || e.metaKey) {
                $(this).toggleClass("selected");
            } else {
                $(this).addClass("selected").siblings().removeClass('selected');
            }
        }).sortable({
            connectWith: "#mc_uc_lstdisplay_blColumnSorted",
            placeholder: "sortable-placeholder",
            receive: function (event, ui) {
                var i = $("#mc_uc_lstdisplay_blColumnSorted").children("li").length;
                $(this).find(".placeholder").remove();
                var link = $("<a href='#'>x</a>");
                var groupByid = "blColumnSorted_groupby" + i.toString();
                var groupBy = $("<input type='checkbox' id ='" + groupByid + "'/>");
                $(ui.helper).removeClass("ui-draggable ui-draggable-handle").addClass("list-group-item ui-sortable-handle");
                $(ui.helper).removeAttr("style");
                $(ui.helper).append(groupBy);
                $(ui.helper).append(link);
                if (null == $(ui.helper).attr("sort")) {
                    // Avant
                    //$(ui.helper).attr("sort", "ASC");
                    // Après
                    var sortid = "blColumnSorted_sort" + i.toString();
                    var sort = $("<div class='switch ascdesc'><input type='checkbox' id ='" + sortid + "'/><label for='" + sortid + "' class='label-info'/></div>");
                    $(ui.helper).append(sort);
                }
            }
        }).on("click", "a", function (event) {
            event.preventDefault();
            $(this).parent().remove();
        });

    });


    $("#mc_lstCriteria_Filter").on('click', 'ul li a', function (e) {

        var btn = $(this).parents(".input-group-btn").find('.btn');
        $(btn).html($(this).text());
        $(btn).val($(this).attr('value'));
    });

    $("#mc_lstCriteria_Filter ul li.active").each(function () {
        $(this).parents(".input-group-btn").find('.btn').html($(this).find('a').text());
        $(this).parents(".input-group-btn").find('.btn').val($(this).find('a').attr('value'));
    });

    $('[id^="mc_uc_lstfilter_ddlGrp"]').on('click', 'li a', function (e) {

        var group = $(this).attr('value');
        var ddlColumn = $('#' + $(this).parents("ul").attr('id').replace("Grp", "Col"));

        $(ddlColumn).empty().append($("<option></option>").val("").html(""));
        $('#mc_uc_lstdisplay_blColumnAvailable > li').each(function () {

            if ($(this).attr("grp") == group && (null == $(this).attr("nocrit"))) {

                var _value = ($(this).attr("mdty") ? "1" : "0") + ";" + $(this).attr("value");
                var option = $("<option></option>")
                    .attr("grp", group)
                    .val(_value)
                    .html($(this).text());

                if ($(this).attr("data-type"))
                    $(option).attr("data-type", $(this).attr("data-type"));

                $(ddlColumn).append($(option));
            }
        });
    });

    $('[id^="mc_uc_lstfilter_ddlCol"]').change(function (e) {
        $(this).find("option:selected").each(function () {
            $(this).prop("selected", true);
            var dataType = $(this).attr('data-type');
            var ddlOperand = $('#' + $(this).parents("select").attr('id').replace("Col", "Operand"));

            $(ddlOperand).find("option").each(function () {
                var operand = $(this).attr("value");
                if (false == IsOperandEnabled(operand, dataType)) {
                    $(this).addClass("hidden");
                }
                else {
                    $(this).removeClass("hidden");
                }
            });
            $(ddlOperand).find("option").first().prop("selected", true);
        });
    });

    $('[id^="mc_uc_lstfilter_ddlOperand"]').on('click', 'li a', function (e) {
        var operand = $(this).attr('value');
        var ddlColumn = $('#' + $(this).parents("select").attr('id').replace("Operand", "Col"));
        $(ddlColumn).find("option:selected").each(function () {
            $(this).attr("operand", operand);
        });
    });


    var tooltipsElement = null;
    //Chargement des tooltips des labels (Ouverture par Hover (Trigger))
    tooltipsElement = $("[title]").tooltip({
        title: this.title,
        html: true,
        placement: "auto",
        trigger: "hover",
        container: "#tipcontainer" // Container des tooltips construits par BS
    });
});





