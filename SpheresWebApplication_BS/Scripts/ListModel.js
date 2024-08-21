// Désactiver les tabIndex du background en présence d'une forme modal ACTIVE (paramètre)
// => Principe récupérer dans un vecteur la liste des éléments visible du container de la forme modale passé en paramètre
//    et boucler sur cette liste lorsque la touche tab est utilisée.
function disableTabModalShown(divModalContent) {

    var focusableChildren = divModalContent.find('a[href], a[data-dismiss], area[href], input, select, textarea, button, *[tabindex], *[contenteditable]').filter(":visible");
    var numElements = focusableChildren.length;
    var currentIndex = 0;

    $(document.activeElement).blur();

    var focus = function () {
        var focusableElement = focusableChildren[currentIndex];
        if (focusableElement)
            focusableElement.focus();
    };

    var focusPrevious = function () {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = numElements - 1;

        focus();

        return false;
    };

    var focusNext = function () {
        currentIndex++;
        if (currentIndex >= numElements)
            currentIndex = 0;

        focus();

        return false;
    };

    $(document).on('keydown', function (e) {

        if (e.keyCode == 9 && e.shiftKey) {
            e.preventDefault();
            focusPrevious();
        }
        else if (e.keyCode == 9) {
            e.preventDefault();
            focusNext();
        }
    });

    $(this).focus();
};

// 
function clearPostBackModel() {
    $get('__EVENTTARGET').value = $get('__EVENTARGUMENT').value = '';
    Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(clearPostBackModel);

    // Le postBack est Terminé 

    // 1. on resize la forme Modale 
    setModalMaxHeight($('.modal.in'));
    // 2. on désactive les tabIndex du background de la forme Modale
    disableTabModalShown($('.modal.in'));
}

function PostBackAndWaitExecution(eventTargetUniqueId, eventArgument) {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(clearPostBackModel);
    __doPostBack(eventTargetUniqueId, eventArgument);
}

$(document).ready(function () {

    //alert("document.ready");
    $('input[type="radio"][data-toggle="confirmation"]').on("click", function() {
        
        $(this).confirmation({
            rootSelector: '[data-toggle=confirmation]',
        });
    });

    //$('[data-toggle="confirmation"]').confirmation({
    //    rootSelector: '[data-toggle=confirmation]',
    //});

    $('#formModal').on('shown.bs.modal', function () {
        // Fenêtre modale en format large
        // alert("shown.bs.modal");
        // Evite le déclenchement multiple de l'événement
        //$(this).off('shown.bs.modal');
        $(this).find('.modal-dialog').addClass("modal-lg");
        setModalMaxHeight($('.modal.in'));
        disableTabModalShown($('.modal.in'));
        $(this).find('#mc_uc_lstmodel_btnCancel').focus();
    });

    $('#formModal').on('hidden.bs.modal', function () {
        //$(document).unbind('keydown');
    });

    // Gestion des contrôles visibles en fonction de la demande (Open|Modify|Remove Model)
    $("#formModal input[type='radio']").on("change", function (event) {
        if ($(this).is(":checked")) {
            $(this).closest("span").addClass("active");
            //event.preventDefault();
            PostBackAndWaitExecution($(this).attr("id"), "");
        }
        else {
            $(this).closest("span").removeClass("active");
        }
    });
});

