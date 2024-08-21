//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLIDFEE").change(function () {
        let idFee = $(this).val();

        $('#DDLSCHEDULELIBRARY').empty();
        if (!isNaN(parseInt(idFee)) && parseInt(idFee) > 0) {
            LoadDataTable(['SCHEDULELIBRARY', 'SCHEDULELIBRARY'], 'VW_FEESCHEDLIBRARY', [{ col: 'IDFEE', value: parseInt(idFee) }], function (vw) {
                LoadDDL("DDLSCHEDULELIBRARY", vw);
            });
        }
    });


    $("#DDLGPRODUCT,#DDLTYPEINSTR").change(function () {
        let typeInstr = $("#DDLTYPEINSTR").val();

        $('#DDLIDINSTR').empty();
        if (typeInstr.length > 0) {
            switch (typeInstr) {
                case 'GrpInstr':
                    LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FEEMATRIXINSTR', [{ col: 'TYPEINSTR', value: typeInstr }], function (vw) {
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

                    LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FEEMATRIXINSTR', restrict, function (vw) {
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
            LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_FEEMATRIXINSTR_UNL', [{ col: 'TYPEINSTR_UNL', value: type }], function (vw) {
                LoadDDL("DDLIDINSTR_UNL", vw);
            });
        }
    });

    $("#DDLTYPECONTRACT").change(function () {
        ClearAutocompleteInput("TXTIDCONTRACT");
    });

    $("#DDLTYPECONTRACTEXCEPT").change(function () {
        ClearAutocompleteInput("TXTIDCONTRACTEXCEPT");
    });

    $("#DDLFEETYPEPARTYA").change(function () {
        ClearAutocompleteInput("TXTIDPARTYA");
    });

    $("#DDLFEETYPEPARTYB").change(function () {
        ClearAutocompleteInput("TXTIDPARTYB");
    });

    $("#DDLFEETYPEOTHERPARTY1").change(function () {
        ClearAutocompleteInput("TXTIDOTHERPARTY1");
    });

    $("#DDLFEETYPEOTHERPARTY2").change(function () {
        ClearAutocompleteInput("TXTIDOTHERPARTY2");
    });

    $("#DDLFEETYPEPARTYA,#DDLFEETYPEPARTYB,#DDLFEETYPEOTHERPARTY1,#DDLFEETYPEOTHERPARTY2,#DDLTYPECONTRACT,#DDLTYPECONTRACTEXCEPT").change(function () {
        let controls = [
            { 'controlId': 'DDLFEETYPEPARTYA', 'value': $("#DDLFEETYPEPARTYA").val() },
            { 'controlId': 'DDLFEETYPEPARTYB', 'value': $("#DDLFEETYPEPARTYB").val() },
            { 'controlId': 'DDLFEETYPEOTHERPARTY1', 'value': $("#DDLFEETYPEOTHERPARTY1").val() },
            { 'controlId': 'DDLFEETYPEOTHERPARTY2', 'value': $("#DDLFEETYPEOTHERPARTY2").val() },
            { 'controlId': 'DDLTYPECONTRACT', 'value': $("#DDLTYPECONTRACT").val() },
            { 'controlId': 'DDLTYPECONTRACTEXCEPT', 'value': $("#DDLTYPECONTRACTEXCEPT").val() }
        ];

        //Because DDLFEETYPEPARTYA/B, DDLFEETYPEOTHERPARTY1/2 , DDLTYPECONTRACT, DDLTYPECONTRACTEXCEPT are used in an autocompleteRelation
        SyncAutoCompleteReferentialColumnRelation(controls);
    });
}
// -->
//</script>