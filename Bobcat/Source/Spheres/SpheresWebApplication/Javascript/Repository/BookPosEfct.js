//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLTYPEINSTR").change(function () {
        $('#DDLIDINSTR').empty();
        let type = $(this).val();
        if (type.length > 0) {
            LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_BOOKPOSEFCTINSTR', [{ col: 'TYPEINSTR', value: type }], function (vw) {
                LoadDDL('DDLIDINSTR', vw);
            });
        }
    });

    //$("#DDLTYPECONTRACT").change(function () {
    //    $('#DDLIDCONTRACT').empty();
    //    let type = $(this).val();
    //    if (type.length > 0) {
    //        LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_BOOKPOSEFCTCONTRACT', [{ col: 'TYPECONTRACT', value: type }], function (vw) {
    //            LoadDDL('DDLIDCONTRACT', vw);
    //        });
    //    }
    //});

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