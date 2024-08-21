//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLCONTRACTCATEGORY").change(function () {
        let controls = [
            { 'controlId': 'DDLCONTRACTCATEGORY', 'value': $("#DDLCONTRACTCATEGORY").val() } 
        ];
        SyncAutoCompleteReferentialColumnRelation(controls);
        ClearAutocompleteInput('TXTIDXC');
    }); 
    
}
// -->
//</script>