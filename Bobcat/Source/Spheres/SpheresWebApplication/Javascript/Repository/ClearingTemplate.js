//<script language="JavaScript">
//<!--
//
function Init() {

    $("#TXTIDA_CLEARER,#TXTIDA_BROKER").change(function () {
        let controlActorId = $(this).prop('id');
        let idA = GetIdAutocompleteInput(controlActorId);
        let controlBookId = controlActorId.replace('IDA', 'IDB').replace('TXT', 'DDL');
        $(`#${controlBookId}`).empty();
        if (!isNaN(parseInt(idA)) && parseInt(idA) > 0) {
            LoadDataTable(['IDB', 'column:IDENTIFIER,columnDisplay:DISPLAYNAME'], 'VW_BOOK_VIEWER', [{ col: 'IDA', value: parseInt(idA) }], function (b) {
                LoadDDL(controlBookId, b, true);
            });
        }
    });

    $("#DDLTYPEINSTR").change(function () {
        $('#DDLIDINSTR').empty();
        let type = $(this).val();
        if (type.length > 0) {
            LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_CLEARINGTPINSTR', [{ col: 'TYPEINSTR', value: type }], function (vw) {
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