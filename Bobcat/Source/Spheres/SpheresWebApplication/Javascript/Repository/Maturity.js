
var FmtEnum = { "YYYYMM": 0, "YYYYMMDD": 1, "YYYYMMwN": 2 };

function Init() {
    ApplyRegExOnMATURITYMONTHYEAR();
}

function ApplyRegExOnMATURITYMONTHYEAR() {

    if ((typeof (Page_Validators) != "undefined") && (Page_Validators != null)) {
        var i;
        for (i = 0; i < Page_Validators.length; i++) {
            if (Page_Validators[i].controltovalidate == "TXTMATURITYMONTHYEAR" && Page_Validators[i].evaluationfunction.name == "RegularExpressionValidatorEvaluateIsValid") {
                SetFormatValidationExpession(Page_Validators[i]);
                break;
            }
        }
    }
}


function SetFormatValidationExpession(validator) {
    let idMR = $("#DDLIDMATURITYRULE").attr("ddlvalue");

    if (!isNaN(parseInt(idMR)) && parseInt(idMR) > 0) {
        LoadDataTable(['MMYFMT'], 'MATURITYRULE', [{ col: 'IDMATURITYRULE', value: parseInt(idMR) }], function (mr) {
            if (mr.length > 0) {
                let expression = '';
                let fmt = parseInt(mr[0].MMYFMT);
                switch (fmt) {
                    case FmtEnum.YYYYMM:
                        expression = "^\\d{6}$";
                        break;
                    case FmtEnum.YYYYMMDD:
                        expression = "^\\d{8}$";
                        break;
                    case FmtEnum.YYYYMMwN:
                        expression = "^\\d{6}w\\d$";
                        break;
                }
                if (expression.length > 0)
                    validator.validationexpression = expression;
            }
        });
    }
}