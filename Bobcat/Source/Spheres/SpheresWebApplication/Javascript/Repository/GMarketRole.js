//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLIDROLEGMARKET").change(function () {
        ClearAutocompleteInput("TXTIDA");
    });

    $("#DDLIDROLEGMARKET").change(function () {
        let controls = [
            { 'controlId': 'DDLIDROLEGMARKET', 'value': $("#DDLIDROLEGMARKET").val() }
        ];

        //Because DDLIDROLEGMARKET are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });


}
// -->
//</script>