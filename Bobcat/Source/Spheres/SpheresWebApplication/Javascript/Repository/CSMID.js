//<script language="JavaScript">
//<!--
//
function Init() {

    $("#TXTIDA_CSS").change(function () {
        ClearAutocompleteInput('TXTIDM');
    });

    $("#TXTIDA_CSS").change(function () {
        let controls = [
            { 'controlId': 'HDNIDA_CSS', 'value': GetControlHiddenAutocompleteInput('TXTIDA_CSS').val() },
        ];
        SyncAutoCompleteReferentialColumnRelation(controls);
    });
}
// -->
//</script>