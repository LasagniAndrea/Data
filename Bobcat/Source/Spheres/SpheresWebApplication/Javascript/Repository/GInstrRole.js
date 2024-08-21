//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLIDROLEGINSTR").change(function () {
        ClearAutocompleteInput("TXTIDA");
    });

    $("#DDLIDROLEGINSTR").change(function () {
        let controls = [
            { 'controlId': 'DDLIDROLEGINSTR', 'value': $("#DDLIDROLEGINSTR").val() }
        ];

        //Because IDROLEGCONTRACT are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });


}
// -->
//</script>