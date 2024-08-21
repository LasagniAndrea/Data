var txtSearch;

$(document).ready(function () {
    txtSearch = $("#txtSearch");
    $(txtSearch).autocomplete({
        source: function (request, response) {
            // EG 20210826 Encodage des caractères spéciaux qui plantent dans l'autocomplete
            var valueToSearch = request.term.replace('\'', '&#039;').replace('\\', '\\\\');
            $.ajax({
                url: "list.aspx/LoadSearchData",
                data: "{ 'request': '" + valueToSearch + "', 'guid':'" + $("#__GUID").val() + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
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
        minLength: 3,
        select: function (event, ui) {
            // event.type is autocompleteselect
            // Prevent the default operation applied (because selected value is replaced)
            event.preventDefault();
            let valItem = ui.item.value;
            valItem = "\"" + valItem + "\"";
            $(txtSearch).val(valItem);
            // FI 20200321 [XXXXX] call TXTSearchRefresh
            TXTSearchRefresh(event);
        },
        open: function (event, ui) {
            //FI 202002 [XXXXX] Remove onblur on onkeydown
            $(txtSearch).removeAttr("onblur"); //required  (If not, select (using mouse) is not working properly)
            $(txtSearch).removeAttr("onkeydown"); //required for prevent a double post
        },
        close: function (event, ui) {
            $(txtSearch).attr("onblur", "TXTSearchRefresh(event);");
            $(txtSearch).attr("onkeydown", "TXTSearchRefresh(event);");
        }
    });
});

function DDLSearchRefresh() {
    if ($(txtSearch).length) {
        if (IsCtrlChanged('ddlSearch')) {
            SaveActiveElement();
            RefreshPage('ddlSearch', 'SearchData');
        }
    }
}

function TXTSearchRefresh(evt) {
    if (evt.type == 'blur') {
        if (IsCtrlChanged('txtSearch')) {
            SaveActiveElement();
            RefreshPage('txtSearch', 'SearchData');
        }
    }
    else if (((evt.type == 'keydown') && (evt.keyCode == Sys.UI.Key.enter)) || evt.type == 'autocompleteselect') {
        if (((evt.type == 'keydown') && (evt.keyCode == Sys.UI.Key.enter))) {
            // Prevent the default operation applied (when the enter key is pressing (Refresh is called))
            event.preventDefault();
        }
        if (IsCtrlChanged('txtSearch')) {
            SaveActiveElement();
            RefreshPage('txtSearch', 'SearchData');
        }
    }
}