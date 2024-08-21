//<script language="JavaScript">
//<!--

var FmtEnum = { "YYYYMM": 0, "YYYYMMDD": 1, "YYYYMMwN": 2 };
var controlExpirationArray = ['DDLROLLCONVMMY', 'DDLMONTHREF', 'DDLBDC_ROLLCONV_MDT', 'TXTPERIODMLTPMDTOFFSET', 'DDLPERIODMDTOFFSET', 'DDLDAYTYPEMDTOFFSET', 'DDLBDC_MDT'];

function Init() {

    var enabledColor = $('#TXTIDENTIFIER').css('color');
    var disabledColor = "gray";

    let fmt = parseInt($("#DDLMMYFMT").val());
    if (false == isNaN(fmt)) {
        switch (fmt) {
            case FmtEnum.YYYYMM:
            case FmtEnum.YYYYMMwN:
                Clear('DDLMMYRELATIVETO');
                break;
            case FmtEnum.YYYYMMDD:
                SetEnabled('DDLMMYRELATIVETO')
                break;
        }
    }

    let rule = $("#DDLMMYRULE").val();
    if (rule == 'm=FGJKNQVX|q_lbd=HMUZ' || rule == 'w_fri|m=FGJKNQVX|q_lbd=HMUZ' || rule == 'w_fri|m_d=FGHJKMNQUVXZ') {
        SetFrequency('');

        $("#DDLMMYRELATIVETO").val('EXP');
        SetDisabled("DDLMMYRELATIVETO");
    }
    else if (rule == 'd') {
        ClearExpirationSetting();
    }

    $("#DDLMMYFMT").change(function () {
        let fmt = parseInt($("#DDLMMYFMT").val());

        Reset('TXTOPENSIMULTANEOUSLY');

        if (false == isNaN(fmt)) {
            switch (fmt) {
                case FmtEnum.YYYYMM:
                    SetMMYRULE('FGHJKMNQUVXZ');
                    break;
                case FmtEnum.YYYYMMDD:
                    SetMMYRULE('d');
                    break;
                case FmtEnum.YYYYMMwN:
                    SetMMYRULE('w');
                    break;
            }
        }
        else {
            SetMMYRULE('');
            Clear('DDLMMYRELATIVETO');
        }
    });

    $("#DDLMMYRELATIVETO").change(function () {
        let relativeTo = $("#DDLMMYRELATIVETO").val();
        if (relativeTo.length == 0) // mandatory when enabled
            $("#DDLMMYRELATIVETO").val("EXP");
    });

    $("#DDLMMYRULE").change(function () {
        let rule = $("#DDLMMYRULE").val();

        SetEnabled('DDLFREQUENCYMATURITY');
        Reset('TXTOPENSIMULTANEOUSLY');


        if (rule.length > 0) {
            if (rule == 'm=FGJKNQVX|q_lbd=HMUZ' || rule == 'w_fri|m=FGJKNQVX|q_lbd=HMUZ' || rule == 'w_fri|m_d=FGHJKMNQUVXZ') {
                
                // set Format
                SetFMT(FmtEnum.YYYYMMDD);
                // set Frequency
                SetFrequency('');
                SetEnabledExpirationSetting();

                $("#DDLMMYRELATIVETO").val('EXP');
                SetDisabled("DDLMMYRELATIVETO");
                
            }
            else if (rule == 'd') {
                // set Format
                SetFMT(FmtEnum.YYYYMMDD);
                SetFrequency('1D');
                SetDisabled('DDLFREQUENCYMATURITY');
                ClearExpirationSetting();

                SetEnabled("DDLMMYRELATIVETO");
                if ($("#DDLMMYRELATIVETO").val().length == 0)
                    $("#DDLMMYRELATIVETO").val('EXP');
            }
            else if (rule.startsWith('w')) {
                // set Format (when is not specified or different of YYYYMMDD)
                if (($("#DDLMMYFMT").val().length == 0) || (parseInt($("#DDLMMYFMT").val()) != FmtEnum.YYYYMMDD))
                    SetFMT(FmtEnum.YYYYMMwN);

                if ($("#DDLROLLCONVMMY").val().length == 0)
                    $("#DDLROLLCONVMMY").val('FRI');

                // set Frequency
                SetFrequency('1W');
                SetDisabled('DDLFREQUENCYMATURITY');

                SetEnabledExpirationSetting();
                if ($("#DDLROLLCONVMMY").val().length == 0)
                    $("#DDLROLLCONVMMY").val('3FRI');

                $("#DDLMONTHREF").val("MaturityMonth");
                SetDisabled('DDLMONTHREF');

                if (parseInt($("#DDLMMYFMT").val()) == FmtEnum.YYYYMMDD) {
                    SetEnabled("DDLMMYRELATIVETO");
                    if ($("#DDLMMYRELATIVETO").val().length == 0)
                        $("#DDLMMYRELATIVETO").val('EXP');
                }
                else
                    Clear("DDLMMYRELATIVETO");

            }
            else { // Monthly, Quarterly, Annually

                // set Format (when is not specified)
                if (($("#DDLMMYFMT").val().length == 0))
                    SetFMT(FmtEnum.YYYYMM);

                // Set Frequency
                if (rule.length == 4) //Ex HMUZ
                    SetFrequency('3M');
                else if (rule.length == 2) //Ex JV
                    SetFrequency('6M');
                else if (rule.length == 1)  //Ex Z
                    SetFrequency('1Y');
                else
                    SetFrequency('1M');

                SetEnabledExpirationSetting();
                
                if ($("#DDLROLLCONVMMY").val().length == 0)
                    $("#DDLROLLCONVMMY").val('3RDFRI');
                $("#DDLMONTHREF").val("MaturityMonth");

                if (parseInt($("#DDLMMYFMT").val()) == FmtEnum.YYYYMMDD) {
                    SetEnabled("DDLMMYRELATIVETO")
                    if ($("#DDLMMYRELATIVETO").val().length == 0)
                        $("#DDLMMYRELATIVETO").val('EXP');
                }
                else
                    Clear("DDLMMYRELATIVETO");
            }
        }
        else
            SetFrequency('');
    });

    $("#DDLFREQUENCYMATURITY").change(function () {
        let frequency = $("#DDLFREQUENCYMATURITY").val();
        SetEnabled('CHKISEOM');
        if (frequency.length > 0) {
            if (frequency == '1D' || frequency == '1W') {
                $('#CHKISEOM').prop("checked", false)
                SetDisabled('CHKISEOM');
            }
        }
    });

    function SetFMT(format) {
        if ($("#DDLMMYFMT").val() !== format) {
            $("#DDLMMYFMT").val(format);
            $("#DDLMMYFMT").change();
        }
    }

    function SetMMYRULE(rule) {
        if ($("#DDLMMYRULE").val() !== rule) {
            $("#DDLMMYRULE").val(rule);
            $("#DDLMMYRULE").change();
        }
    }

    function SetFrequency(freq) {
        if ($("#DDLFREQUENCYMATURITY").val() != freq) {
            $("#DDLFREQUENCYMATURITY").val(freq);
            $("#DDLFREQUENCYMATURITY").change();
        }
    }

    function SetEnabled(controlId) {
        if (controlId.startsWith('DDL')) {
            $(`#${controlId} option`).show();
            $(`#${controlId}`).css("color", enabledColor);
        }
        else if (controlId.startsWith('TXT')) {
            $(`#${controlId}`).removeAttr('readonly');
            $(`#${controlId}`).css("color", enabledColor);
        }
        else if (controlId.startsWith('CHK')) {
            $(`#${controlId}`).prop("disabled", false);
        }
        $(`#${controlId}`).removeAttr('tabindex');
    }

    function SetDisabled(controlId) {
        if (controlId.startsWith('DDL')) {
            $(`#${controlId} option:not(:selected)`).hide();
            $(`#${controlId}`).css("color", disabledColor);
        }
        else if (controlId.startsWith('TXT')) {
            $(`#${controlId}`).attr('readonly', 'true');
            $(`#${controlId}`).css("color", disabledColor);
        }
        else if (controlId.startsWith('CHK')) {
            $(`#${controlId}`).prop("disabled", true);
        }
        $(`#${controlId}`).attr('tabindex', '-1');
    }
    /**
     * Reset control 
     * @param {any} controlId
     */
    function Reset(controlId) {
        $(`#${controlId}`).val('');
    }

    /**
     * reset and disabled control
     * @param {any} controlId
     */
    function Clear(controlId) {
        Reset(controlId);
        SetDisabled(controlId);
    }

    /**
     * reset and disabled control used for expiry/maturitydate
     */
    function ClearExpirationSetting() {
        controlExpirationArray.forEach(function (item) {
            Clear(item);
        });
    }

    function SetEnabledExpirationSetting() {
        controlExpirationArray.forEach(function (item) {
            SetEnabled(item);
        });
    }


    
}


// -->
//</script>