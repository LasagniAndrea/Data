function MainMenuDisplay(objRef, objcaller, level, mnuName, visibility) { 
    try {
        var mnuObj = document.getElementById(objRef);
        var othermnuObj, othermnuid;
        var offsetimg = ((level - 1) * 5) + ((level - 2) * 5);
        if (null != mnuObj) {
            var tmpLevel = Math.min(level, 2);
            if (mnuObj.style.display == "none") {
                if (2 == tmpLevel)
                    objcaller.className = "SubMenu-" + mnuName + "-Selected";
            }
            else {
                if (2 == tmpLevel)
                    objcaller.className = "SubMenu-" + mnuName + "-Over";
            }
            if ((visibility == 'MASK') && (2 == tmpLevel))
                objcaller.className = objcaller.className + " MaskMenu";

            mnuObj.style.display = (mnuObj.style.display == "none") ? "block" : "none";
        }
        if (1 == level) {
            var aDivs = document.body.getElementsByTagName("DIV");
            var i;
            for (i = 0; i < aDivs.length; i++) {
                if (aDivs[i].id.indexOf("Mastermnu") > -1 && aDivs[i].id != objcaller.id) {
                    othermnuid = aDivs[i].id.substr(6) + "sub";
                    othermnuObj = document.getElementById(othermnuid);
                    othermnuObj.style.display = "none";
                    aDivs[i].className = aDivs[i].className.replace("-Selected", "");
                }
            }
            objcaller.className = "MainMenu-" + mnuName + "-Selected";
            if (visibility == 'MASK')
                objcaller.className = objcaller.className + " MaskMenu";
        }
    }
    catch (error) { }
}

function MenuOver(objRef, level, mnuName, visibility) {
    MenuOutOver(objRef, level, mnuName, visibility, "-Over");
}

function MenuOut(objRef, level, mnuName, visibility) {
    MenuOutOver(objRef, level, mnuName, visibility, " ");
}

function MenuOutOver(objRef, level, mnuName, visibility, suffix) {
    var sUrl;
    var classname = objRef.className;
    if (1 == level) {
        if (-1 == classname.indexOf("MainMenu-" + mnuName + "-Selected")) {
            objRef.className = "MainMenu-" + mnuName + suffix;
            if (visibility == 'MASK')
                objRef.className = objRef.className + " MaskMenu";
        }
    }
    else {
        objRef.className = "SubMenu-" + mnuName + suffix;
        if (visibility == 'MASK')
            objRef.className = objRef.className + " MaskMenu";
    }

    var mnuText = document.getElementById("td_" + objRef.id);
    if (mnuText != null) {
        mnuText.className = "SubMenuTxt-" + mnuName + suffix;
        if (visibility == 'MASK')
            mnuText.className = mnuText.className + " MaskMenu";
    }
    level = Math.min(level, 2);

}
// EG 20220613 [XXXXX] Refactoring Javascript/Jquery
function SwitchMaskMenu() {
    var hidmask = $("input[id$='hidMaskMenu']");

    if (hidmask.attr("value") == "out") {
        hidmask.attr("value") = "in";
        $("#btnMaskMenu i")[0].className = "fa fa-filter";
    }
    else {
        hidmask.attr("value") = "out";
        $("#btnMaskMenu i")[0].className = "fa fa-list";
    }
    DisplayMaskMenu();
}

// EG 20220613 [XXXXX] Refactoring Javascript/Jquery
function DisplayMaskMenu() {
    var hidmask = $("input[id$='hidMaskMenu']");
    var ressource = hidmask.attr("title").split(';');
    if (hidmask.attr("value") == "out")
        $("#btnMaskMenu i")[0].title = ressource[0];
    else
        $("#btnMaskMenu i")[0].title = ressource[1];

    //
    if (typeof ($) != "undefined") {
        if ($('#btnMaskMenu').data("qtip") != undefined) {
            var api = $('#btnMaskMenu').qtip("api");
            if (null != api)
                api.updateContent($('#btnMaskMenu').attr('alt'));

        }
        //
        var maskMenu = $('.MaskMenu');
        var displayValue = "none";
        $.each(maskMenu, function (index, value) {
            var id = value.id;
            if (hidmask.attr("value") == "out") {
                if (null != id && id.indexOf("Mastermnu") > -1) {
                    var maskNext = $(this).next('div');
                    if (0 < maskNext.length)
                        maskNext[0].style.display = displayValue;
                }
                $(this).css("display", displayValue);
            }
            else {
                displayValue = "";
                var maskParent = $(this).parent('div.MaskMenu');
                if (0 == maskParent.length)
                    $(this).css("display", displayValue);
                else
                    $(this).css("display", maskParent[0].style.display);
            }
            if (null != id && id.indexOf("Mastermnu") > -1) {
                var maskTDParent = $(this).parent('td');
                if (0 < maskTDParent.length) {
                    if (null != maskTDParent[0].parentElement)
                        maskTDParent[0].parentElement.style.display = displayValue;
                }
            }
        });
    }
}

function SlideMenu() {
    $("#summary").toggleClass("close");
    $("#TblLoginLogout").toggleClass("close");
    $("#pg-main").toggleClass("full");
}

function HideMenu() {
    $("#summary").toggleClass("disconnect");
    $("#pg-main").toggleClass("disconnect");
}
