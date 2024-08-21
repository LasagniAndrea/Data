//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLTYPEINSTR").change(function () {
        let typeInstr = $("#DDLTYPEINSTR").val();

        $('#DDLIDINSTR').empty();
        if (typeInstr.length > 0) {
            switch (typeInstr) {
                case 'GrpInstr':
                    LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_STGIDENTRULESINSTR', [{ col: 'TYPEINSTR', value: typeInstr }], function (vw) {
                        LoadDDL("DDLIDINSTR", vw);
                    });
                    break;
                default:
                    let restrict = [{ col: 'TYPEINSTR', value: typeInstr }, { col: 'GPRODUCT', value: 'FUT' }];

                    LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_STGIDENTRULESINSTR', restrict, function (vw) {
                        LoadDDL("DDLIDINSTR", vw);
                    });
                    break;
            }
        }
    });

    $("#DDLTYPECONTRACT").change(function () {
        ClearAutocompleteInput("TXTIDCONTRACT");
    });


    $("#DDLTYPECONTRACT").change(function () {
        let controls = [
            { 'controlId': 'DDLTYPECONTRACT', 'value': $("#DDLTYPECONTRACT").val() }
        ];

        //Because DDLTYPECONTRACT is used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });

}
// -->
//</script>