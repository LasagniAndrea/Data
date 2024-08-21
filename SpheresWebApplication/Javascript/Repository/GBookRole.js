//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLIDROLEGBOOK").change(function () {
        ClearAutocompleteInput("TXTIDA");
    });

    $("#DDLIDROLEGBOOK").change(function () {
        let controls = [
            { 'controlId': 'DDLIDROLEGBOOK', 'value': $("#DDLIDROLEGBOOK").val() }
        ];

        //Because TYPECONTRACT are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });


}
// -->
//</script>