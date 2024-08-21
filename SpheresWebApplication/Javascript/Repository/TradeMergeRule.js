//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLTYPEINSTR").change(function () {
        $('#DDLIDINSTR').empty();
        let type = $(this).val();
        if (type.length > 0) {
            LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_TRADEMERGERULEINSTR', [{ col: 'TYPEINSTR', value: type }], function (vw) {
                LoadDDL('DDLIDINSTR', vw);
            });
        }
    });

    $("#DDLTYPEPARTY").change(function () {
        ClearAutocompleteInput("TXTIDPARTY");
    });

    $("#DDLTYPECONTRACT").change(function () {
        ClearAutocompleteInput("TXTIDCONTRACT");
    });

    $("#DDLTYPEPARTY,#DDLTYPECONTRACT").change(function () {
        let controls = [
            { 'controlId': 'DDLTYPEPARTY', 'value': $("#DDLTYPEPARTY").val() },
            { 'controlId': 'DDLTYPECONTRACT', 'value': $("#DDLTYPECONTRACT").val() }
        ];

        //Because TYPEPARTY, TYPECONTRACT are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });
}
// -->
//</script>