//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLPAYTYPEPARTY").change(function () {
        ClearAutocompleteInput('TXTIDPAY');
    });

    $("#DDLRECTYPEPARTY").change(function () {
        ClearAutocompleteInput('TXTIDREC');
    });

    $("#DDLASSETCATEGORY").change(function () {
        ClearAutocompleteInput('TXTIDASSET');
    });


    $("#DDLPAYTYPEPARTY,#DDLRECTYPEPARTY,#DDLASSETCATEGORY").change(function () {
        let controls = [
            { 'controlId': 'DDLPAYTYPEPARTY', 'value': $('#DDLPAYTYPEPARTY').val() },
            { 'controlId': 'DDLRECTYPEPARTY', 'value': $('#DDLRECTYPEPARTY').val() },
            { 'controlId': 'DDLASSETCATEGORY', 'value': $('#DDLASSETCATEGORY').val() }
        ];
        SyncAutoCompleteReferentialColumnRelation(controls);
    });
}
// -->
//</script>