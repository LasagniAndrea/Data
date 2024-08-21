//<script language="JavaScript">
//<!--


function Init() {

    var enabledColor = $('#TXTIDENTIFIER').css('color');
    var disabledColor = "gray";

    SetDisabled('CHKISIMMAINTENANCE');
    SetDisabled('CHKISIMMAINTENANCEBis');
    SetDisabled('TXTNBMONTHLONGCALL');
    SetDisabled('TXTNBMONTHLONGPUT');
    SetDisabled('CHKISIMINTERCOMSPRD');
    SetDisabled('DDLIMWEIGHTEDRISKMETH');
    SetDisabled('DDLIMROUNDINGDIR');
    SetDisabled('DDLIMROUNDINGPREC');
    SetDisabled('CHKISIMINTERCOMSPRD');
    SetDisabled('CHKISCALCECCCONR');
    SetDisabled('TXTECCCONRADDON');
    SetDisabled('TXTBASEURL');
    SetDisabled('TXTUSERID');
    SetDisabled('TXTPWD');
    SetDisabled('TXTCMECORESCHEME');
    SetDisabled('DDLCURRENCYTYPE');
    SetDisabled('CHKISEXCLUDEWRONGPOS');
    SetDisabled('CHKISCESMONLY');
    SetDisabled('CHKISWITHHOLIDAYADJ');
    SetDisabled('TXTALPHAFACTOR');
    SetDisabled('TXTBETAFACTOR');
    SetDisabled('TXTEWMAFACTOR');
    SetDisabled('TXTWINDOWSIZEFORMAX');
    SetDisabled('TXTSTATWINDOWSIZE');
    SetDisabled('TXTMINIMUMAMOUNTFIRST');
    SetDisabled('TXTMINAMTFIRSTTERM');
    SetDisabled('TXTMINIMUMAMOUNT');

    let meth = $("#DDLINITIALMARGINMETH").val();

    switch (meth) {
        case "CBOE_Margin":
            SetEnabled('CHKISIMMAINTENANCE');
            SetEnabled('CHKISIMMAINTENANCEBis');
            SetEnabled('TXTNBMONTHLONGCALL');
            SetEnabled('TXTNBMONTHLONGPUT');
            break;
        case "IMSM":
            SetEnabled('CHKISCESMONLY');
            SetEnabled('CHKISWITHHOLIDAYADJ');
            SetEnabled('TXTALPHAFACTOR');
            SetEnabled('TXTBETAFACTOR');
            SetEnabled('TXTEWMAFACTOR');
            SetEnabled('TXTWINDOWSIZEFORMAX');
            SetEnabled('TXTSTATWINDOWSIZE');
            SetEnabled('TXTMINIMUMAMOUNTFIRST');
            SetEnabled('TXTMINAMTFIRSTTERM');
            SetEnabled('TXTMINIMUMAMOUNT');
            break;
        case "SPAN_CME":
        case "SPAN_C21":
            SetEnabled('CHKISIMMAINTENANCE');
            SetEnabled('CHKISIMMAINTENANCEBis');
            SetEnabled('DDLIMWEIGHTEDRISKMETH');
            SetEnabled('DDLIMROUNDINGDIR');
            SetEnabled('DDLIMROUNDINGPREC');
            SetEnabled('CHKISIMINTERCOMSPRD');
            SetEnabled('CHKISCALCECCCONR');
            SetEnabled('TXTECCCONRADDON');
            break;
        case "SPAN_2_CORE":
            SetEnabled('CHKISIMMAINTENANCE');
            SetEnabled('CHKISIMMAINTENANCEBis');
            SetEnabled('TXTBASEURL');
            SetEnabled('TXTUSERID');
            SetEnabled('TXTPWD');
            SetEnabled('TXTCMECORESCHEME');
            SetEnabled('DDLCURRENCYTYPE');
            SetEnabled('CHKISEXCLUDEWRONGPOS');
            break;
    }

    $("#DDLINITIALMARGINMETH").change(function () {

        SetDisabled('CHKISIMMAINTENANCE');
        SetDisabled('CHKISIMMAINTENANCEBis');
        SetDisabled('TXTNBMONTHLONGCALL');
        SetDisabled('TXTNBMONTHLONGPUT');
        SetDisabled('CHKISIMINTERCOMSPRD');
        SetDisabled('DDLIMWEIGHTEDRISKMETH');
        SetDisabled('DDLIMROUNDINGDIR');
        SetDisabled('DDLIMROUNDINGPREC');
        SetDisabled('CHKISIMINTERCOMSPRD');
        SetDisabled('CHKISCALCECCCONR');
        SetDisabled('TXTECCCONRADDON');
        SetDisabled('TXTBASEURL');
        SetDisabled('TXTUSERID');
        SetDisabled('TXTPWD');
        SetDisabled('TXTCMECORESCHEME');
        SetDisabled('DDLCURRENCYTYPE');
        SetDisabled('CHKISEXCLUDEWRONGPOS');
        SetDisabled('CHKISCESMONLY');
        SetDisabled('CHKISWITHHOLIDAYADJ');
        SetDisabled('TXTALPHAFACTOR');
        SetDisabled('TXTBETAFACTOR');
        SetDisabled('TXTEWMAFACTOR');
        SetDisabled('TXTWINDOWSIZEFORMAX');
        SetDisabled('TXTSTATWINDOWSIZE');
        SetDisabled('TXTMINIMUMAMOUNTFIRST');
        SetDisabled('TXTMINAMTFIRSTTERM');
        SetDisabled('TXTMINIMUMAMOUNT');

        let meth = $("#DDLINITIALMARGINMETH").val();

        switch (meth) {
            case "CBOE_Margin":
                SetEnabled('CHKISIMMAINTENANCE');
                SetEnabled('CHKISIMMAINTENANCEBis');
                SetEnabled('TXTNBMONTHLONGCALL');
                SetEnabled('TXTNBMONTHLONGPUT');
                break;
            case "IMSM":
                SetEnabled('CHKISCESMONLY');
                SetEnabled('CHKISWITHHOLIDAYADJ');
                SetEnabled('TXTALPHAFACTOR');
                SetEnabled('TXTBETAFACTOR');
                SetEnabled('TXTEWMAFACTOR');
                SetEnabled('TXTWINDOWSIZEFORMAX');
                SetEnabled('TXTSTATWINDOWSIZE');
                SetEnabled('TXTMINIMUMAMOUNTFIRST');
                SetEnabled('TXTMINAMTFIRSTTERM');
                SetEnabled('TXTMINIMUMAMOUNT');
                break;
            case "SPAN_CME":
            case "SPAN_C21":
                SetEnabled('CHKISIMMAINTENANCE');
                SetEnabled('CHKISIMMAINTENANCEBis');
                SetEnabled('DDLIMWEIGHTEDRISKMETH');
                SetEnabled('DDLIMROUNDINGDIR');
                SetEnabled('DDLIMROUNDINGPREC');
                SetEnabled('CHKISIMINTERCOMSPRD');
                SetEnabled('CHKISCALCECCCONR');
                SetEnabled('TXTECCCONRADDON');
                break;
            case "SPAN_2_CORE":
                SetEnabled('CHKISIMMAINTENANCE');
                SetEnabled('CHKISIMMAINTENANCEBis');
                SetEnabled('TXTBASEURL');
                SetEnabled('TXTUSERID');
                SetEnabled('TXTPWD');
                SetEnabled('TXTCMECORESCHEME');
                SetEnabled('DDLCURRENCYTYPE');
                SetEnabled('CHKISEXCLUDEWRONGPOS');
                break;
        }
    });

    function SetEnabled(controlId) {
        if (controlId.startsWith('DDL')) {
            $(`#${controlId} option`).show();
            $(`#${controlId}`).css("color", enabledColor);
        }
        else if (controlId.startsWith('TXT')) {
            $(`#${controlId}`).attr('readonly', false);
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
            $(`#${controlId}`).attr('readonly', true);
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

}