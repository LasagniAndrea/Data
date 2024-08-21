function Init() {

    // InitFromDeriveAttrib necessary when "asset_ETD" is called as child of "DeriveAttrib"
    InitFromDeriveAttrib();

    $("#DDLIDDERIVATIVEATTRIB").change(function () {

        InitFromDeriveAttrib();

        let idDerivativeAttrib = $("#DDLIDDERIVATIVEATTRIB").val();
        SyncAutoCompleteReferentialColumnRelation([{ 'controlId': 'DDLIDDERIVATIVEATTRIB', 'value': idDerivativeAttrib }]);

        ClearAutocompleteInput("TXTIDPREVIOUSASSET");
        
    });
}

function InitFromDeriveAttrib() {
    let idDerivativeAttrib;
    if ($("#DDLIDDERIVATIVEATTRIB").is('span')) {
        idDerivativeAttrib = $("#DDLIDDERIVATIVEATTRIB").attr("ddlvalue"); //child of "DeriveAttrib"
    } else {
        idDerivativeAttrib = $("#DDLIDDERIVATIVEATTRIB").val(); //"asset_ETD" is called as Master
    }

    if (!isNaN(parseInt(idDerivativeAttrib)) && parseInt(idDerivativeAttrib)> 0) {
        LoadDataTable(['IDDC', 'IDMATURITY'], 'DERIVATIVEATTRIB', [{ col: 'IDDERIVATIVEATTRIB', value: parseInt(idDerivativeAttrib) }], function (dvAttrib) {
            if (dvAttrib.length > 0) {
                LoadDataTable(['IDM'], 'DERIVATIVECONTRACT', [{ col: 'IDDC', value: dvAttrib[0].IDDC }], function (dc) {
                    if (dc.length > 0) {
                        LoadDataTable(['IDM', 'SHORT_ACRONYM'], 'VW_MARKET_IDENTIFIER', [{ col: 'IDM', value: dc[0].IDM }], function (m) {
                            LoadDDL("DDLIDM", m);
                        });
                    }
                });

                let idMaturity = dvAttrib[0].IDMATURITY
                LoadDataTable(['LASTTRADINGDAY', 'LASTTRADINGTIME',
                    'MATURITYDATE', 'MATURITYDATESYS', 'MATURITYTIME',
                    'DELIVERYDATE',
                    'FIRSTNOTICEDAY', 'LASTNOTICEDAY'], 'MATURITY', [{ col: 'IDMATURITY', value: idMaturity }], function (mat) {
                        for (const [key, value] of Object.entries(mat[0])) {
                            let controlId = `TXT${key}`;
                            if (value != null) {
                                switch (key) {
                                    case "LASTTRADINGDAY":
                                    case "MATURITYDATE":
                                    case "MATURITYDATESYS":
                                    case "DELIVERYDATE":
                                    case "FIRSTNOTICEDAY":
                                    case "LASTNOTICEDAY":
                                        SetJSonDateToControl(controlId, value);
                                        break;
                                    default:
                                        $(`#${controlId}`).val(value);
                                        break;
                                }
                            }
                        }
                    });
            }
        });
    }

}