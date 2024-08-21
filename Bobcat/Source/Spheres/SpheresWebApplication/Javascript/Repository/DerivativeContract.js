//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLASSETCATEGORY").change(function () {
        ClearAutocompleteInput("TXTIDASSET_UNL");
    });

    $("#TXTIDM,#TXTIDENTIFIER").change(function () {
        $("#DDLIDDC_SHIFT").val('');

        let idM = GetIdAutocompleteInput("TXTIDM");
        if (!isNaN(parseInt(idM)) && parseInt(idM) > 0) {
            LoadDCShift(parseInt(idM), $("#TXTIDENTIFIER").val(), function (dc) {
                LoadDDL("DDLIDDC_SHIFT", dc, true);
            });
        }
    });

    $("#DDLASSETCATEGORY,#TXTIDM,#TXTIDENTIFIER").change(function () {
        ClearAutocompleteInput("TXTIDDC_UNL");
    });

    $("#DDLASSETCATEGORY,#TXTIDM,#TXTIDENTIFIER").change(function () {
        let controls = [
            { 'controlId': 'DDLASSETCATEGORY', 'value': $("#DDLASSETCATEGORY").val() },
            { 'controlId': 'HDNIDM', 'value': GetControlHiddenAutocompleteInput('TXTIDM').val() },
            { 'controlId': 'TXTIDENTIFIER', 'value': $("#TXTIDENTIFIER").val() }
        ];
        //Because ASSETCATEGORY, IDM, IDENTIFIER are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });


    function LoadDCShift(idM, currentDCIdentifier, callback) {
        let JSONCol = JSON.stringify(['IDDC', 'IDENTIFIER']);

        $.ajax({
            url: "WebServices/CommonWebService.asmx/LoadDCShift",
            data: `{'col':${JSONCol}, 'idM':'${idM}', 'currentDCIdentifier':'${currentDCIdentifier}'}`,
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset = utf-8",
            success: function (data) {
                if (typeof callback === "function") {
                    callback(data.d);
                }
            },
            error: AlertAJaxError
        })

    }


}
// -->
//</script>