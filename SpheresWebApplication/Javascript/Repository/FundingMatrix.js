//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLGPRODUCT,#DDLTYPEINSTR").change(function () {
        let typeInstr = $("#DDLTYPEINSTR").val();

        $('#DDLIDINSTR').empty();
        if (typeInstr.length > 0) {
            switch (typeInstr) {
                case 'GrpInstr':
                    LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FUNDINGMATRIXINSTR', [{ col: 'TYPEINSTR', value: typeInstr }], function (vw) {
                        LoadDDL("DDLIDINSTR", vw);
                    });
                    break;
                default:
                    let gProduct = $("#DDLGPRODUCT").val();

                    let restrict;
                    if (gProduct.length > 0)
                        restrict = [{ col: 'TYPEINSTR', value: typeInstr }, { col: 'GPRODUCT', value: gProduct }];
                    else
                        restrict = [{ col: 'TYPEINSTR', value: typeInstr }];

                    LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FUNDINGMATRIXINSTR', restrict, function (vw) {
                        LoadDDL("DDLIDINSTR", vw);
                    });
                    break;
            }
        }
    });



    $("#DDLTYPEINSTR_UNL").change(function () {
        let type = $(this).val();

        $('#DDLIDINSTR_UNL').empty();
        if (type.length > 0) {
            LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FUNDINGMATRIXINSTR', [{ col: 'TYPEINSTR_UNL', value: type }], function (vw) {
                LoadDDL("DDLIDINSTR_UNL", vw);
            });
        }
    });

    $("#DDLTYPECONTRACT").change(function () {
        ClearAutocompleteInput("TXTIDCONTRACT");
    });

    $("#DDLTYPEPARTYA").change(function () {
        ClearAutocompleteInput("TXTIDPARTYA");
    });

    $("#DDLTYPEPARTYB").change(function () {
        ClearAutocompleteInput("TXTIDPARTYB");
    });


    $("#DDLTYPEPARTYA,#DDLTYPEPARTYB,#DDLTYPECONTRACT").change(function () {
        let controls = [
            { 'controlId': 'DDLTYPEPARTYA', 'value': $("#DDLTYPEPARTYA").val() },
            { 'controlId': 'DDLTYPEPARTYB', 'value': $("#DDLTYPEPARTYB").val() },
            { 'controlId': 'DDLTYPECONTRACT', 'value': $("#DDLTYPECONTRACT").val()}
        ];

        SyncAutoCompleteReferentialColumnRelation(controls);
    });
}
// -->
//</script>