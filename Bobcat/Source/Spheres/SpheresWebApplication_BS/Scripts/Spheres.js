// Fonction permettant de retourne si un élément est tronqué ou non
$.expr[':'].truncated = function (obj) {
    var $this = $(obj);
    var $c = $this
               .clone()
               .css({ display: 'inline', width: 'auto', visibility: 'hidden' })
               .appendTo('body');

    var c_width = $c.width();
    $c.remove();

    if (c_width > $this.width())
        return true;
    else
        return false;
};


function SaveActiveElement() {
    if (document.activeElement == null)
        return;

    var curAe = document.activeElement;
    document.forms[0].__ACTIVEELEMENT.value = curAe.id;
}

function PostBack(callEvent, callArgument) {
    SaveActiveElement();
    javascript: __doPostBack(callEvent, callArgument);
}


/***********************/
/* Gestion des cookies */
/***********************/
function SetCookie(pName, pValue, pExpires, pPath, pDomain, pSecure) {
    SetCookieForActor('', pName, pValue, pExpires, pPath, pDomain, pSecure);
}

//Expires par defaut : 100 jours
function SetCookieForActor(pIda, pName, pValue, pExpires, pPath, pDomain, pSecure) {
    var expiresDefaut = new Date();
    expiresDefaut.setTime(expiresDefaut.getTime() + (100 * 24 * 60 * 60 * 1000));
    pExpires = ((pExpires) ? pExpires : expiresDefaut);
    var curCookie = pIda + '_' + pName + '=' + escape(pValue) + ((pExpires) ? '; expires=' + pExpires.toGMTString() : '') +
        ((pPath) ? '; path=' + pPath : '') + ((pDomain) ? '; domain=' + pDomain : '') + ((pSecure) ? '; pSecure' : '');
    document.cookie = curCookie;
}

function GetCookie(pName) {
    GetCookieForActor('', pName);
}

function GetCookieForActor(pIda, pName) {
    var retValue;
    var dc = document.cookie;
    var prefix = pIda + '_' + pName + '=';
    var begin = dc.indexOf('; ' + prefix);

    if (begin == -1) {
        begin = dc.indexOf(prefix);
        if (begin != 0) {
            retValue = '!notFound!';
        }
    }
    else {
        begin += 2;
    }

    var end = document.cookie.indexOf(';', begin);
    if (end == -1) {
        end = dc.length;
    }
    if (retValue == '!notFound!') {
        retValue = null;
    }
    else {
        retValue = unescape(dc.substring(begin + prefix.length, end));
    }
    return retValue;
}

/***************************************/
/* Gestion des menus VISIBILITY = MASK */
/***************************************/
function DisplayMaskMenu() {

    var maskmenu = $('.maskmenu');
    var displayvalue = "none";

    $.each(maskmenu, function (index, value) {
        var id = value.id;
        if ($('#hidMaskMenu').val() == "out") {
            if ((null != id) && (-1 < id.indexOf("mastermnu"))) {
                var masksiblings = $(this).siblings('div');
                if (0 < masksiblings.length)
                    masksiblings[0].style.display = displayvalue;
            }
            $(this).css("display", displayvalue);
        }
        else {

            displayvalue = "";
            var maskparent = $(this).parent('.maskmenu');
            if (0 == maskparent.length)
                $(this).css("display", displayvalue);
            else
                $(this).css("display", maskparent[0].style.display);
        }
    });
}


function SwitchMaskMenu() {

    $('#btnMaskMenu span:first').removeClass("glyphicon-th");
    $('#btnMaskMenu span:first').removeClass("glyphicon-th-large");

    if ($('#hidMaskMenu').val() == "out") {
        $('#hidMaskMenu').val("in");
        $('#btnMaskMenu span:first').addClass("glyphicon-th");
    }
    else {
        $('#hidMaskMenu').val("out");
        $('#btnMaskMenu span:first').addClass("glyphicon-th-large");

    }
    DisplayMaskMenu();
}

/***************************************/
/* Impression                          */
/***************************************/
function PrintPage() {

    window.print();
    return false;
}

function RefreshPage() {
    __doPostBack('0', 'SELFRELOAD_');
}


/***************************************/
/* Confirmation modal                  */
/***************************************/

function OpenConfirmation() {
    $('#confirmationModal').modal('show');
}
function DisplayMessage() {
    $('#confirmationModal').modal('show');
}

function setModalMaxHeight(element) {
    this.$element = $(element);
    this.$content = this.$element.find('.modal-content');
    var borderWidth = this.$content.outerHeight() - this.$content.innerHeight();
    var dialogMargin = $(window).width() < 768 ? 20 : 60;
    var contentHeight = $(window).height() - (dialogMargin + borderWidth);
    var headerHeight = this.$element.find('.modal-header').outerHeight() || 0;
    var footerHeight = this.$element.find('.modal-footer').outerHeight() || 0;
    var maxHeight = contentHeight - (headerHeight + footerHeight);

    this.$content.css({
        'overflow': 'hidden'
    });

    this.$element
      .find('.modal-body').css({
          'max-height': maxHeight,
          'overflow-y': 'auto'
      });
}


/***************************************/
/* Search                              */
/***************************************/

$().ready(function () {
    $('.modal.printable').on('shown.bs.modal', function () {
        $('.modal-dialog', this).addClass('focused');
        $('body').addClass('modalprinter');

        if ($(this).hasClass('autoprint')) {
            window.print();
        }
    }).on('hidden.bs.modal', function () {
        $('.modal-dialog', this).removeClass('focused');
        $('body').removeClass('modalprinter');
    });

    $('.popover-markup > .trigger').popover({
        html: true,
        title: function () {
            return $('#myPopover').find('.head').html();
        },
        content: function () {
            return $('#myPopover').find('.content').html();
        },
        container: 'body',
        placement: 'bottom'
    });

});

/***************************************/
/* Date picker                         */
/***************************************/
$().ready(function () {

    var culture = $.trim($("a[href='UserProfil.aspx'").text()).substring(1, 3);

    $('div.input-group.date').each(function () {
        $(this).datetimepicker({
            locale: moment.locale(culture),
            format: $(this).attr('data-type'),
            debug: true,
        });
    });
});


/***************************************/
/* Common                              */
/***************************************/

//var originalValidatorUpdateDisplay = null;
$().ready(function () {
    $('#formModal').on('show.bs.modal', function (e) {
        var isLoadUrl = (false == $(e.relatedTarget).attr("id").endsWith("Model"))
        if (isLoadUrl) {
            var loadurl = $(e.relatedTarget).data('load-url');
            $(this).find('.modal-content').load(loadurl);
        }
    });

    // On efface le body de la forme modale
    // On réinitialise la forme principale pour réactiver le submit 
    // Sinon ERROR de soumission =  "Form submission canceled because the form is not connected"
    $('#formModal').on('hidden.bs.modal', function (e) {
        //alert("empty modal")
        $(this).data('bs.modal', null);
    });
});
//document.addEventListener("focus", RemoveHasErrorClass, true);

//// Remplace la méthode ValidatorUpdateDisplay   
//originalValidatorUpdateDisplay = window.ValidatorUpdateDisplay;
//window.ValidatorUpdateDisplay = extendedValidatorUpdateDisplay;


