//<script language="JavaScript">
//<!--
//
function Init() {

    LoadDCMarket();

    $("#TXTIDDC").change(function () {

        //TXTIDASSET depends on TXTIDDC
        ClearAutocompleteInput("TXTIDASSET");

        let controlHidden = GetControlHiddenAutocompleteInput('TXTIDDC');
        SyncAutoCompleteReferentialColumnRelation([{ 'controlId': `${controlHidden.prop('id')}`, 'value': controlHidden.val() }]);

        $("#DDLIDM").empty();
        $("#DDLIDMATURITYRULE").empty();
        $("#DDLIDMATURITY").empty();

        InitFromFromDeriveContract();
    });

    $("#DDLIDMATURITYRULE").change(function () {
        InitFromMaturityRule();
    });


    $("#DDLIDMATURITY").change(function () {
        InitFromMaturity();
    });
}

function LoadDCMarket() {
    let idDC = GetIdAutocompleteInput("TXTIDDC");

    if (!isNaN(parseInt(idDC)) && parseInt(idDC) > 0) {
        LoadDataTable(['IDM'], 'DERIVATIVECONTRACT', [{ col: 'IDDC', value: parseInt(idDC) }], function (dc) {
            if (dc.length > 0) {
                if (dc[0].IDM > 0) {
                    LoadDataTable(['IDM', 'SHORT_ACRONYM'], 'VW_MARKET_IDENTIFIER', [{ col: 'IDM', value: dc[0].IDM }], function (m) {
                        LoadDDL("DDLIDM", m);
                    });
                }
            }
        });
    }
}

function InitFromFromDeriveContract() {

    LoadDCMarket();

    let idDC = GetIdAutocompleteInput("TXTIDDC");
    if (!isNaN(parseInt(idDC)) && parseInt(idDC) > 0) {
        LoadDataTable(['IDM'], 'DERIVATIVECONTRACT', [{ col: 'IDDC', value: parseInt(idDC) }], function (dc) {
            if (dc.length > 0) {
                LoadDataTable(['IDMATURITYRULE', 'MATURITYRULE'], 'VW_DRVCONTRACTMATRULE', [{ col: 'IDDC', value: parseInt(idDC) }], function (mr) {
                    if (mr.length > 0) {
                        LoadDDL("DDLIDMATURITYRULE", mr);
                        InitFromMaturityRule();
                    }
                });
            }
        });
    }
}

function InitFromMaturity() {

    $("#TXTLASTTRADINGDAY").val("");
    $("#TXTLASTTRADINGTIME").val("");

    $("#TXTMATURITYDATE").val("");
    $("#TXTMATURITYDATESYS").val("");
    $("#TXTMATURITYTIME").val("");

    $("#TXTDELIVERYDATE").val("");

    $("#TXTPERIODMLTPDELIVERY").val("");
    $("#DDLPERIODDELIVERY").val("");
    $("#DDLDAYTYPEDELIVERY").val("");
    $("#DDLROLLCONVDELIVERY").val("");

    $("#TXTFIRSTDELIVERYDATE").val("");
    $("#TXTLASTDELIVERYDATE").val("");
    $("#TXTDELIVERYTIMESTART").val("");
    $("#TXTDELIVERYTIMEEND").val("");
    $("#DDLDELIVERYTIMEZONE").val("");

    $("#TXTFIRSTDLVSETTLTDATE").val("");
    $("#TXTLASTDLVSETTLTDATE").val("");

    $("input:radio[name='CHKISAPPLYSUMMERTIME']")[0].checked = false;
    $("input:radio[name='CHKISAPPLYSUMMERTIME']")[1].checked = false;

    $("#TXTPERIODMLTPDLVSETTLTOFFSET").val("");
    $("#DDLPERIODDLVSETTLTOFFSET").val("");
    $("#DDLDAYTYPEDLVSETTLTOFFSET").val("");
    $("#DDLSETTLTOFHOLIDAYDLVCONVENTION").val("");

    $("#TXTFIRSTNOTICEDAY").val("");
    $("#TXTLASTNOTICEDAY").val("");

    let idMaturity = $("#DDLIDMATURITY").val();
    if (!isNaN(parseInt(idMaturity)) && parseInt(idMaturity) > 0) {
        LoadDataTable(['LASTTRADINGDAY', 'LASTTRADINGTIME',
            'MATURITYDATE', 'MATURITYDATESYS', 'MATURITYTIME',
            'DELIVERYDATE',
            'PERIODMLTPDELIVERY', 'PERIODDELIVERY', 'DAYTYPEDELIVERY', 'ROLLCONVDELIVERY',
            'FIRSTDELIVERYDATE', 'LASTDELIVERYDATE', 'DELIVERYTIMESTART', 'DELIVERYTIMEEND', 'DELIVERYTIMEZONE',
            'ISAPPLYSUMMERTIME',
            'FIRSTDLVSETTLTDATE', 'LASTDLVSETTLTDATE',
            'PERIODMLTPDLVSETTLTOFFSET', 'PERIODDLVSETTLTOFFSET', 'DAYTYPEDLVSETTLTOFFSET', 'SETTLTOFHOLIDAYDLVCONVENTION',
            'FIRSTNOTICEDAY', 'LASTNOTICEDAY'], 'MATURITY', [{ col: 'IDMATURITY', value: parseInt(idMaturity) }], function (mat) {
                for (const [key, value] of Object.entries(mat[0])) {
                    let controlId = `TXT${key}`;
                    if (value != null) {
                        switch (key) {
                            case "LASTTRADINGDAY":
                            case "MATURITYDATE":
                            case "MATURITYDATESYS":
                            case "DELIVERYDATE":
                            case "FIRSTNOTICEDAY":
                            case "FIRSTDELIVERYDATE":
                            case "LASTDELIVERYDATE":
                            case "FIRSTDLVSETTLTDATE":
                            case "LASTDLVSETTLTDATE":
                            case "FIRSTNOTICEDAY":
                            case "LASTNOTICEDAY":
                                controlId = `TXT${key}`;
                                SetJSonDateToControl(controlId, value);
                                break;
                            case "PERIODDELIVERY":
                            case "DAYTYPEDELIVERY":
                            case "ROLLCONVDELIVERY":
                            case "DELIVERYTIMEZONE":
                            case "PERIODDLVSETTLTOFFSET":
                            case "DAYTYPEDLVSETTLTOFFSET":
                            case "SETTLTOFHOLIDAYDLVCONVENTION":
                                controlId = `DDL${key}`;
                                $(`#${controlId}`).val(value);
                                break;
                            case "ISAPPLYSUMMERTIME":
                                $("input:radio[name='CHKISAPPLYSUMMERTIME']")[0].checked = value;
                                $("input:radio[name='CHKISAPPLYSUMMERTIME']")[1].checked = !value;
                                break;
                            default:
                                $(`#${controlId}`).val(value);
                                break;
                        }
                    }
                }
            });
    }
}

function InitFromMaturityRule() {

    let idMaturityRule = $("#DDLIDMATURITYRULE").val();
    if (!isNaN(parseInt(idMaturityRule)) && parseInt(idMaturityRule) > 0) {
        LoadDataTable(['IDMATURITY', 'MATURITYMONTHYEAR'], 'MATURITY', [{ col: 'IDMATURITYRULE', value: idMaturityRule }], function (mat) {
            LoadDDL("DDLIDMATURITY", mat);
            InitFromMaturity();
        });
    }
}


// -->
//</script>