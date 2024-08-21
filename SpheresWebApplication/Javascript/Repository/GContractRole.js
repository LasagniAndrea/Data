//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLIDROLEGCONTRACT").change(function () {
        ClearAutocompleteInput("TXTIDA");
    });

    $("#DDLIDROLEGCONTRACT").change(function () {
        let controls = [
            { 'controlId': 'DDLIDROLEGCONTRACT', 'value': $("#DDLIDROLEGCONTRACT").val() }
        ];

        //Because IDROLEGCONTRACT are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });


}
// -->
//</script>