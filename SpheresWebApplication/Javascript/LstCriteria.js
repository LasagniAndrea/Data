
$(document).ready(function () {
    LoadAutoCompleteData();
});


function LoadAutoCompleteData() {
    $(".or-autocomplete").autocomplete({
        source: function (request, response) {
            let controlTxtId = $(this)[0].element[0].id; // Control with autocomplete 
            let controlBut = $("#btn_PtsForList" + GetSuffixNumeric(controlTxtId));
            let openReferential = OpenReferentialForAutocomplete(controlBut.attr("onclick"));
            let requestVal = ExtractLast(request.term);
            $.ajax({
                url: "WebServices/ReferentialWebService.asmx/LoadAutoCompleteData",
                data: "{ 'request': '" + requestVal + "','controlId': '" + controlTxtId + "','openReferential': '" + openReferential + "'}",
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
        minLength: 3,
        focus: function (event, ui) {
            // prevent value inserted on focus when value already contains ';' 
            let controlTxtId = $(this).attr("id"); // Control with autocomplete
            if ($("#" + controlTxtId).val().indexOf(';') > -1) {
                return false;
            }
        },
        select: function (event, ui) {
            let controlTxtId = $(this).attr("id"); // Control with autocomplete
            // specific behavior value already contains ';' 
            if ($("#" + controlTxtId).val().indexOf(';') > -1) {
                let terms = this.value.split(';');
                // remove the current input
                terms.pop();
                // add the selected item
                terms.push(ui.item.value);
                this.value = terms.join(";");

                OperatorToEqual(controlTxtId);
                return false;
            }
            else {
                OperatorToEqual(controlTxtId);
            }
        },
        search: function (event, ui) {
            // custom minLength
            let  term = ExtractLast(this.value);
            if (term.length < 3) {
                return false;
            }
        }
    });
}

//Set Equal Operator to DDLOperator
function OperatorToEqual(controlTxtId) {
    $("#ddlOperator" + GetSuffixNumeric(controlTxtId)).val('=');
}

function GetSuffixNumeric(controlId) {
    let ret = controlId.match(new RegExp('\\d+$'))[0];
    return ret;
}

//Retrieve last word when several words exists separated by ';'
//Retrieve inpout when input doesn't contains ';'
function ExtractLast(input) {
    return input.split(';').pop();
}


   