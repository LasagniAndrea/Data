//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLTYPECONTRACT").change(function () {
        ClearAutocompleteInput("TXTIDCONTRACT");
    });

    $("#DDLTYPECONTRACT").change(function () {
        let controls = [
            { 'controlId': 'DDLTYPECONTRACT', 'value': $("#DDLTYPECONTRACT").val() }
        ];

        //Because TYPECONTRACT are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });


}
// -->
//</script>