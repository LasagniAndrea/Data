//<script language="JavaScript">
//<!--
function Init() {

    $("#DDLGPRODUCT,#DDLTYPEINSTR").change(function () {
        let typeInstr = $("#DDLTYPEINSTR").val();

        $('#DDLIDINSTR').empty();
        if (typeInstr.length > 0) {
            switch (typeInstr) {
                case 'GrpInstr':
                    LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FEESCHEDINSTR', [{ col: 'TYPEINSTR', value: typeInstr }], function (vw) {
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

                    LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FEESCHEDINSTR', restrict, function (vw) {
                        LoadDDL("DDLIDINSTR", vw);
                    });
                    break;
            }
        }
    });

    $("#DDLTYPEINSTR_UNL").change(function () {
        let typeInstr = $(this).val();
        
        $('#DDLIDINSTR_UNL').empty();
        if (typeInstr.length > 0) {
            LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FEESCHEDINSTR', [{ col: 'TYPEINSTR_UNL', value: typeInstr }], function (vw) {
                LoadDDL("DDLIDINSTR_UNL", vw);
            });
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