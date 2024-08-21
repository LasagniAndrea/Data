//<script language="JavaScript">
//<!--
//
function Init() {
    
    $("#TXTIDDC").change(function () {
        ClearAutocompleteInput("TXTIDDC_CASC");
    });

    $("#TXTIDDC").change(function () {
        let controls = [
            { 'controlId': 'HDNIDDC', 'value': GetControlHiddenAutocompleteInput('TXTIDDC').val() },
        ];

        //Because TYPECONTRACT are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });


}
// -->
//</script>