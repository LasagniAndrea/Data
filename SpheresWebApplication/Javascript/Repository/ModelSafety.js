//<script language="JavaScript">
//<!--


function Init() {

    var lblEnabledColor = $('#LBLDESCRIPTION').css('color');
    var txtEnabledColor = $('#TXTDESCRIPTION').css('color');
    var disabledColor = "gray";

    if ($('#CHKPWDMODPERIODIC').is(":checked")) {
        SetEnabled('NBDAYVALID');
        SetEnabled('ALLOWEDGRACE');
        SetEnabled('NBDAYWARNING');
    }
    else {
        SetDisabled('NBDAYVALID');
        SetDisabled('ALLOWEDGRACE');
        SetDisabled('NBDAYWARNING');
    }

    $("#CHKPWDMODPERIODIC").change(function () {

        if ($('#CHKPWDMODPERIODIC').is(":checked")) {
            SetEnabled('NBDAYVALID');
            SetEnabled('ALLOWEDGRACE');
            SetEnabled('NBDAYWARNING');
        }
        else {
            SetDisabled('NBDAYVALID');
            SetDisabled('ALLOWEDGRACE');
            SetDisabled('NBDAYWARNING');
        }
    });

    function SetEnabled(controlId) {
        
        $(`#${'TXT' + controlId}`).prop("disabled", false);
        $(`#${'TXT' + controlId}`).css("color", txtEnabledColor);
        $(`#${'LBL' + controlId}`).css("color", lblEnabledColor);
    }

    function SetDisabled(controlId) {
        $(`#${'TXT' + controlId}`).prop("disabled", true);
        $(`#${'TXT' + controlId}`).css("color", disabledColor);
        $(`#${'LBL' + controlId}`).css("color", disabledColor);
        //Reset('TXT' + controlId);
    }

    function Reset(controlId) {
        $(`#${controlId}`).val('');
    }

}


// -->
//</script>