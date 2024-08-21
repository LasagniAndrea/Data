function Init() {

    RemoveMaturityRuleAlreadyUsed();

    $("#TXTIDDC").change(function () {

        $('#DDLIDMATURITYRULE').empty();
        LoadMaturityRule(['IDMATURITYRULE', 'MATURITYRULE'], function (vw) {
            LoadDDL("DDLIDMATURITYRULE", vw);
        });

        RemoveMaturityRuleAlreadyUsed();
    });
}

/** Remove the Maturity Rule already used by de current DC */
function RemoveMaturityRuleAlreadyUsed() {

    let idDC = GetControlHiddenAutocompleteInput('TXTIDDC').val();
    if (!isNaN(parseInt(idDC)) && parseInt(idDC) > 0) {
        LoadDataTable(['IDMATURITYRULE'], 'VW_DRVCONTRACTMATRULE', [{ col: 'IDDC', value: parseInt(idDC) }], function (dc) {
            if (dc.length > 0) {
                dc.forEach(function (item) {
                    if (!$(`#DDLIDMATURITYRULE option[value='${item.IDMATURITYRULE}']`).is(':selected'))
                        $(`#DDLIDMATURITYRULE option[value='${item.IDMATURITYRULE}']`).remove();
                })
            }
        });
    }
}
    