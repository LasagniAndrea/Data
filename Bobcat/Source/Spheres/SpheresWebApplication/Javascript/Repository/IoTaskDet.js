//<script language="JavaScript">
//<!--
//
function Init() {
    $("#DDLELEMENTTYPE").change(function () {
        let elementType = $(this).val();

        $('#DDLIDIOELEMENT').empty();
        if (elementType.length > 0) {
            LoadDataTable(['IDIOELEMENT', 'column:IDIOELEMENT,columnDisplay:DISPLAYNAME'], 'VW_IOELEMENT', [{ col: 'ELEMENTTYPE', value: elementType }], function (e) {
                LoadDDL("DDLIDIOELEMENT", e);
            });
        }
    });
}
// -->
//</script>