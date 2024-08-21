function Init() {
    $("#TXTIDA,#TXTIDA_ACTORIMP").change(function () {

        let idA = GetIdAutocompleteInput('TXTIDA');
        let idA2 = GetIdAutocompleteInput('TXTIDA_ACTORIMP');

        if (idA == idA2) {
            let controlActorId = $(this).prop('id');
            if (controlActorId == 'TXTIDA')
                ClearAutocompleteInput('TXTIDA_ACTORIMP');
            else
                ClearAutocompleteInput('TXTIDA');
        }
    });
}