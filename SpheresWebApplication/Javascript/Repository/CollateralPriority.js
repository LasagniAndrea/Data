//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLASSETCATEGORY").change(function () {
        ClearAutocompleteInput('TXTIDASSET');
    });

    $("#DDLASSETCATEGORY").change(function () {
        let controls = [
            { 'controlId': 'DDLASSETCATEGORY', 'value': $('#DDLASSETCATEGORY').val() }
        ];
        SyncAutoCompleteReferentialColumnRelation(controls);
    });
}
// -->
//</script>