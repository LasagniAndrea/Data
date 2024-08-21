var divInfosWhereDataRow;
var idLstTemplateSav;
var idATemplateSav;


$(document).ready(function () {
    $("span[id^='imgDisabledFilter']:not([onclick]),span[id^='lnkDisabledFilter']:not([onclick])").on("click", function (event) {

        divInfosWhereDataRow = document.querySelector("#divInfosWhereDataRow");
        ClearGrid();
        AvailableSession(function (result) {
            if (result) {
                GetCurrentTemplate(function (result) {
                    //idLstTemplateSav : Name of the current template when a DisabledFilter action occured
                    idLstTemplateSav = result.IDLSTTEMPLATE;
                    idATemplateSav = result.IDA;
                    DisabledFilter(event.target.parentElement.id); // event.target.parentElement.id => span Control
                });
            }
        });
    });
});

function ClearGrid() {
    $("#divDtg table>tbody").hide();
    let td = $("#divDtg .DataGrid_PagerStyle td");
    GetResourceArray(["Page", "of"], function (res) {
        td.html(`<span style='font-weight:bold;'>${res[0]} 1 ${res[1]} 1</span>`);
    });
    
}

function HideFilter(spanControlId) {
    $(`#${spanControlId}`).parent().hide(); // hide div (parent of span/label)
    var i = parseInt(spanControlId.charAt(spanControlId.length - 1));
    if ($(divInfosWhereDataRow).find(".infosAnd")[i - 1] != undefined)
        $($(divInfosWhereDataRow).find(".infosAnd")[i - 1]).hide();
}

function DisabledFilter(spanControlId) {

    // Spheres® create a new temporary template is the current template (idLstTemplateSav) is not temporary
    $.ajax({
        url: "list.aspx/DisabledFilter",
        data: "{'disabledFilterControlId': '" + spanControlId + "', 'guid':'" + $("#__GUID").val() + "'}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            OnDisabledFilterSuccess(spanControlId);
        },
        error: AlertAJaxError
    })

}

function OnDisabledFilterSuccess(spanControlId) {
    if (spanControlId.startsWith("lnk")) {
        //Hide of all optional Filter
        let arrayFilter = $(divInfosWhereDataRow).find("span[id^='imgDisabledFilter']");
        if ($(arrayFilter).length) {
            $(arrayFilter).each(function () {
                HideFilter($(this).attr("id"));
            });
        }
        $(`#${spanControlId}`).hide(); // hide label
    }
    else if (spanControlId.startsWith("img")) {
        //Hide of courrent optional Filter
        HideFilter(spanControlId);

        // if all optional Filter are hidden => hide of lnkDisabledFilter
        let optFilter = $(divInfosWhereDataRow).find("div>span>i");
        let optFilterDisabled = $(divInfosWhereDataRow).find("div:hidden");
        if (optFilter.length == optFilterDisabled.length)
            $("#lnkDisabledFilter").hide(); // hide label
    }

    if (!idLstTemplateSav.startsWith('*')) {
        GetCurrentTemplate(function (result) {
            let idLstTemplate = result.IDLSTTEMPLATE;
            let idA = result.IDA;
            
            let arrayAnchor = $("#tbViewer>a[onclick]");
            if ($(arrayAnchor).length) {
                $(arrayAnchor).each(function () {
                    let onClickAttrib = $(this).attr("onclick").replace(`&T=${idLstTemplateSav}`, `&T=${idLstTemplate}`).replace(`&A=${idATemplateSav}`, `&A=${idA}`);
                    $(this).attr("onclick", onClickAttrib);
                })
            }
        });
    }

    GetCurrentTemplateSubTitle(function (result) {
        $("#subTitleLeft").html(result.Item1);
        $("#lblRightSubtitle").html(result.Item2);
    });
    
    $("#WarningSummary").hide();
}

function GetCurrentTemplate(callback) {
    $.ajax({
        url: "list.aspx/GetCurrentTemplate",
        data: "{'guid':'" + $("#__GUID").val() + "'}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (typeof callback === "function") {
                callback(data.d);
            } else {
                throw 'callback is not a function!';
            }
        },
        error: AlertAJaxError
    });
}

function GetCurrentTemplateSubTitle(callback) {
    $.ajax({
        url: "list.aspx/GetCurrentTemplateSubTitle",
        data: "{'guid':'" + $("#__GUID").val() + "'}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (typeof callback === "function") {
                callback(data.d);
            } else {
                throw 'callback is not a function!';
            }
        },
        error: AlertAJaxError
    });
}
    




