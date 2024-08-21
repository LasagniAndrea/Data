//<script language="JavaScript">
//<!--
//
function Init() {

    $("#DDLTYPEINSTR").change(function () {
        $('#DDLIDINSTR').empty();
        let type = $(this).val();
        if (type.length > 0) {
            LoadDataTable(['ID_', 'DISPLAYNAME'], 'VW_EXTENDINSTR', [{ col: 'TYPEINSTR', value: type }], function (vw) {
                LoadDDL('DDLIDINSTR', vw);
            });
        }
    });
}
// -->
//</script>