//<script language="JavaScript">
//<!--
//
function Init() {

    $("#TXTIDA_PAY,#TXTIDA_REC").change(function () {
        let controlActorId = $(this).prop('id');
        let idA = GetIdAutocompleteInput(controlActorId);
        let controlBookId = controlActorId.replace('IDA', 'IDB').replace('TXT', 'DDL');
        $(`#${controlBookId}`).empty();
        if (!isNaN(parseInt(idA)) && parseInt(idA) > 0) {
            LoadDataTable(['IDB', 'column:IDENTIFIER,columnDisplay:DISPLAYNAME'], 'VW_BOOK_VIEWER', [{ col: 'IDA', value: parseInt(idA) }], function (b) {
                LoadDDL(controlBookId, b);
            });
        }
    });

    $("#DDLASSETCATEGORY").change(function () {
        let controls = [
            { 'controlId': 'DDLASSETCATEGORY', 'value': $("#DDLASSETCATEGORY").val() }
        ];

        SyncAutoCompleteReferentialColumnRelation(controls);
        ClearAutocompleteInput('TXTIDASSET');
    });

}
// -->
//</script>