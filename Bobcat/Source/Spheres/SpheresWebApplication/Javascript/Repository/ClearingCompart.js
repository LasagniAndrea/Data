//<script language="JavaScript">
//<!--
//
function Init() {

    $("#TXTIDA_CSS").change(function () {
        ClearAutocompleteInput('TXTIDA');
    });

    $("#TXTIDA_CSS").change(function () {
        let controls = [
            { 'controlId': 'TXTIDA_CSS', 'value': GetControlHiddenAutocompleteInput('TXTIDA_CSS').val() },
        ];
        SyncAutoCompleteReferentialColumnRelation(controls);
    });
}
// -->
//</script>