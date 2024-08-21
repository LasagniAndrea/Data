// FI 20210218 [XXXXX] add var culture
var culture = $("#__CULTURE").val();

// EG 20201116 [25556] Gestion de la touche Control pour target window.open sur lien des menus de Summary
// EG 20210126 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
var cntrlIsPressed = false;
$(document).on ("keydown", function (event) {
    if (event.ctrlKey)
        cntrlIsPressed = true;
});

$(document).on ("keyup", function () {
    cntrlIsPressed = false;
});

function RemoveAudit() {
    var data = document.getElementById('pnlTracks');
    if (null != data)
        $("#pnlTracks").html("");
    var data = document.getElementById('pnlDiffXMLTracks');
    if (null != data)
        $("#pnlDiffXMLTracks").html("");
}

var isAddEndRequestHandler = false;
function pageLoad(sender, args) {

    let isPartialLoad = args.get_isPartialLoad();
    if (isPartialLoad) {
        // EG 2010-08-26 Fonctions JQuery a rappeler
        // Avec JQuery l'exécution des scripts est faite une fois la page entièrement chargée via l'utilisation de la méthode $().ready()
        // Malheureusement cette méthode n'est pas appelée dans le cas d'un updatepanel (AJAX) et les scripts ne sont éventuellement 
        // plus enregistrés
        RecallFunctions();
        if (false == isAddEndRequestHandler) {
            /* FI 20231004 [WI720] call add_endRequest for AJAX request*/
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
            isAddEndRequestHandler = true;
        }
    }
}
/**
 * Méthode appelée en fin de requête asynchrone (AJAX). Si une erreur => une alerte est affiché avec le messsage d'erreur
 * @param {any} sender
 * @param {any} args
 * FI 20231004 [WI720] Add
 */
function EndRequestHandler(sender, args) {
    if (args.get_error() != undefined) {
        let msg = 'An error occurred while processing your request.\r\n\r\n.';
        msg += args.get_error().message;
        alert(msg);
        console.error(msg);
    }
}


var WindowObjectReference = []; // variable globale
function OpenRequestedPopup(strUrl, strWindowName, strfeatures) {
    if (WindowObjectReference.length == 0) {
        var WindowReference = window.open(strUrl, strWindowName, strfeatures);
        WindowObjectReference[0] = { url: strUrl, ref: WindowReference }
    }
    else if (WindowObjectReference.length > 0) {
        //findIndex is not supported by internet Explorer (Use of some method)
        //var index = WindowObjectReference.findIndex(Element => Element.url == strUrl);
        var index = -1;
        WindowObjectReference.some(function (el, i) {
            if (el.url == strUrl) {
                index = i;
                return true;
            }
        });

        if (index > -1) {
            var WindowReference = WindowObjectReference[index].ref;
            if (WindowReference == null || WindowReference.closed) {
                var WindowReference = window.open(strUrl, strWindowName, strfeatures);
                WindowObjectReference[index] = { url: strUrl, ref: WindowReference }
            }
            else {
                WindowReference.focus();
            }
        }
        else {
            var WindowReference = window.open(strUrl, strWindowName, strfeatures);
            WindowObjectReference.push({ url: strUrl, ref: WindowReference });
        }
    };
}


function ClickOnNode(evt) {
    try {
        var elem;
        if (evt.srcElement) elem = evt.srcElement;
        else if (evt.target) elem = evt.target;

        if (null != elem) {

            if ($(elem).hasClass("full-margin"))
                DisplayNode(elem);
            else {
                if (false == $(elem).hasClass("full-capture"))
                    elem = $(elem).parent(".full-capture");

                if ($(elem).hasClass("full-capture"))
                    elem = $(elem).parent(".full-margin");

                if (false == $(elem).hasClass("full-margin"))
                    elem = $(elem).parent(".full-margin");

                if ($(elem).hasClass("full-margin"))
                    DisplayNode(elem[0]);
            }
        }
    }
    catch (err) {
        // Throw a generic error with a message
        throw Error.create("ClickOnNode : " + err.message)
    }
}

// DisplayNode
// EG 2020-08-28 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
// EG 20211217 [XXXXX] Gestion icon (Font Awesome) ouverture/Fermeture (identique à un toggle)
function DisplayNode(e) {
    try {
        // Balise SPAN
        span = $(e.children[0]).children(".full-capture-textsupplement")[0];

        if ("undefined" != span) {
            // InputHidden avec ID = "TXT" + N°Balise
            txt = span.children[0];
            txt.value = (txt.value == "0" ? "1" : "0");
            // Balise DIV avec Bouton
            divbtn = e.children[0].children[0];
            var isCollapse = $(divbtn).hasClass("fa-angle-down");
            $(divbtn).removeClass('fa-angle-right fa-angle-down').addClass(isCollapse ? "fa-angle-right" : "fa-angle-down");

            redraw = 0;
            if ($(e.children[0]).children(".full-title").attr("data-redraw"))
                redraw = 1;


            $(e).children().slice(1).each(function () {
                $(this).toggle();
                if (redraw)
                    $(this)[0].innerHTML = $(this)[0].innerHTML;
            });
        }
    }
    catch (err) {
        // Throw a generic error with a message
        throw Error.create("DisplayNode : " + err.message)
    }
}

//FI 2014-10-06 [XXXXX] Modify
function PostBackOnKeyEnter(evt, callEvent, callArgument) {
    if (evt.keyCode == Sys.UI.Key.enter) {
        //2009-09-23 FI l'appel à event.returnValue doit être effectué après le __doPostBack  
        //sinon le le bonton enter reste actif
        //__doPostBack(callEvent, callArgument);

        //FI 2014-10-06 call PostBack method (SaveActiveElement) 
        PostBack(callEvent, callArgument);

        if (evt.preventDefault)
            evt.preventDefault();
        else
            evt.returnValue = false;
    }
}

// EG 2021-01-20 [XXXXX] Correction gestion de la touche entrée sur TEXTAREA
// EG 2021-04-07 [25556] Gestion de la touche Entrée sur DialogBox JQuery
function CheckKey(evt) {

    try {
        var elt;

        if (!evt.target)
            elt = evt.srcElement;
        else
            elt = evt.target;

        /* EG 2015-12-15 [21667] (false == elt.isTextEdit) && (false == elt.isContentEditable) && (null == elt.parentTextEdit)*/
        if (evt.keyCode == Sys.UI.Key.backspace) {
            if ((false == elt.isTextEdit) && (false == elt.isContentEditable) && (null == elt.parentTextEdit)) {
                if (evt.preventDefault)
                    evt.preventDefault();
                else
                    evt.returnValue = false;
            }
        }
        else if (evt.keyCode == Sys.UI.Key.enter) {
            var isTrapEnter = true;
            //PL 2012-10-10 Evolution pour textarea (TRIM 18186)
            isTrapEnter = (elt.tagName.toLowerCase() != "textarea");
            if (isTrapEnter)
                isTrapEnter = (null == elt.offsetParent) || (elt.offsetParent.className.indexOf("ui-dialog") == -1);

            if (isTrapEnter) {
                isTrapEnter = (elt.tagName.toLowerCase() != "select");
                if (isTrapEnter) {
                    if ((elt.className.indexOf("or-autocomplete") > -1) ||
                        (elt.className.indexOf("rcr-autocomplete") > -1) ||
                        (elt.className.indexOf("rc-autocomplete") > -1)) {
                        /* FI 2020-01-07 [XXXXX] or-autocomplete (OpenReferential-autocomplete) isTrapEnter = false */
                        /* FI 2021-02-02 [XXXXX] rcr-autocomplete (ReferentialColumnRelation-autocomplete) isTrapEnter = false */
                        /* FI 2021-02-08 [XXXXX] rc-autocomplete (ReferentialColumn-autocomplete) isTrapEnter = false */
                        isTrapEnter = false;
                    }
                    if (isTrapEnter) {
                        var id = null;

                        if (null == id) {
                            if (null != $get("__idDefaultControlOnEnter")) {
                                //2009-09-23 FI => Activation du control défini comme Default sur la touche Entrée
                                //Le control default est spécifié à travers input de type hidden
                                id = $get("__idDefaultControlOnEnter").value;
                            }
                        }

                        if ((null != id) && (id != "") && (null != $get(id))) {
                            $get(id).click();
                            if (evt.preventDefault)
                                evt.preventDefault();
                            else
                                evt.returnValue = false;
                        }
                        else {
                            //La Touche entrée est sans effet
                            if (evt.preventDefault)
                                evt.preventDefault();
                            else
                                evt.returnValue = false;
                        }
                    }
                    else {
                        //La Touche entrée est sans effet
                        if (evt.preventDefault)
                            evt.preventDefault();
                        else
                            evt.returnValue = false;
                    }
                }
            }
        }
    }
    catch (err) {
        throw Error.create("CheckKey : " + err.message);
    }
}

function ChangeOverflow(element, before, after) {
    var div = element.getElementsByTagName("div");
    if (null != div) {
        for (var i = 0; i < div.length; i++) {
            if (div[i].style.overflow == before)
                div[i].style.overflow = after;
        }
    }
}

function ChangeRowScrollPosition(element, before, after) {
    var tr = element.getElementsByTagName("tr");
    if (null != tr) {
        for (var i = 0; i < tr.length; i++) {
            if ((null != tr[i].name) && (-1 < tr[i].id.indexOf("scrollTopPosition"))) {
                if (tr[i].style.position == before)
                    tr[i].style.position = after;
            }
        }
    }
}
// EG 2018-05-25 [23979] IRQ Processing
// EG 2021-01-20 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
// EG 2021-01-20 [25556] utilisation de outerHTML à la place de innerHTML et fermeture de la page
function CallPrint(headerId, bodyId) {
    var prtTitle = "Spheres®";
    var frmTitle = document.title;
    if (null != frmTitle) {
        prtTitle = frmTitle;
    }
    var prtLink = "Includes/EFSTheme-vlight.min.css";
    var frmLink = document.getElementById('linkCss');
    if (null != frmLink) {
        prtLink = frmLink.getAttribute('href');
    }
    var prtHeader;
    if (null != headerId) {
        prtHeader = document.getElementById(headerId);
        ChangeRowScrollPosition(prtHeader, "relative", "static");
    }
    var prtBody;
    if (null != bodyId) {
        prtBody = document.getElementById(bodyId);
        ChangeRowScrollPosition(prtBody, "relative", "static");
    }
    var winPrint = window.open('', '', 'left=0,top=0,toolbar=0,scrollbars=1,resizable=1,status=0');
    winPrint.document.write("<html>");
    winPrint.document.write("<head>");
    winPrint.document.write("  <title>" + prtTitle + " [Print]</title>");
    winPrint.document.write('  <style type="text/css">');
    winPrint.document.write("    thead {display: table-header-group;}");
    winPrint.document.write("    tfoot {display: table-footer-group;}");
    winPrint.document.write("  </style>");
    winPrint.document.write('  <link rel="stylesheet" href="Includes/fontawesome-all.min.css"/>');
    winPrint.document.write('  <link rel="stylesheet" href="Includes/EFSThemeCommon.min.css"/>');
    winPrint.document.write('  <link rel="stylesheet" href="Includes/EFSUISprites.min.css"/>');
    winPrint.document.write('  <link rel="stylesheet" href="' + prtLink + '"/>');
    winPrint.document.write('  <link type="image/x-ico" rel="shortcut icon" href="Images/ico/Spheres-Blue.ico" />');
    winPrint.document.write('  <link type="image/x-png" rel="shortcut icon" href="Images/png/Spheres-Blue.png" />');
    winPrint.document.write("</head>");
    winPrint.document.write("<body>");
    if (null != prtHeader) {
        winPrint.document.write(prtHeader.outerHTML);
        ChangeRowScrollPosition(prtHeader, "static", "relative");
    }
    if (null != prtBody) {
        winPrint.document.write(prtBody.outerHTML);
        ChangeRowScrollPosition(prtBody, "static", "relative");
    }
    winPrint.document.write("</body>");
    winPrint.document.write("</html>");
    ChangeOverflow(winPrint.document, "auto", "visible");
    winPrint.focus();
    winPrint.print();
    winPrint.close();
    return true;
}


function ExpandCollapse(evt) {
    var imgGroup;
    if (!evt.target)
        imgGroup = evt.srcElement;
    else
        imgGroup = evt.target;

    var trChild = document.getElementsById(imgGroup.id);
    for (var j = 0; j < trChild.length; j++) {
        if (trChild[j].id != imgGroup.id)
            trChild[j].style.display = trChild[j].style.display == "none" ? "" : "none";
        else if (-1 == imgGroup.src.indexOf("collapse"))
            imgGroup.src = imgGroup.src.replace("expand", "collapse");
        else
            imgGroup.src = imgGroup.src.replace("collapse", "expand");
    }
}


function ExpandCollapseAll(sender) {
    if (sender == null)
        return;
    // Conformite XHTML tous browser (class) / IE7 (className)
    var childNode = sender.childNodes[0];
    var className = childNode.getAttribute("class");

    var div = document.getElementsByTagName("div");
    for (var i = 0; i < div.length; i++) {

        if ((-1 < div[i].className.indexOf("plus")) && (-1 < className.indexOf("plus")))
            div[i].click();
        if ((-1 < div[i].className.indexOf("minus")) && (-1 < className.indexOf("minus")))
            div[i].click();
    }
    if (-1 < className.indexOf("plus")) {
        childNode.className = childNode.className.replace("plus", "minus")
    }
    else if (-1 < className.indexOf("minus")) {
        childNode.className = childNode.className.replace("minus", "plus")
    }
}

// EG 2020-08-25 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
function ClickOnLetter(sender) {
    var imgGroupId = sender.id.split("_", 3);
    var table = document.getElementById(imgGroupId[1] + "_TBL");
    var trChild = table.getElementsByTagName("tr");
    for (var j = 0; j < trChild.length; j++) {
        if (-1 < trChild[j].id.indexOf("_MAIN"))
            j = j;
        else if (-1 < trChild[j].id.indexOf(imgGroupId[0] + "_" + imgGroupId[1]))
            trChild[j].style.display = (trChild[j].style.display == "none" ? "" : "none");
    }
    if (-1 == sender.className.indexOf("minus"))
        sender.className = sender.className.replace("plus", "minus");
    else
        sender.className = sender.className.replace("minus", "plus");
}

function SaveActiveElement() {
    if (document.activeElement == null)
        return;

    var curAe = document.activeElement;
    document.forms[0].__ACTIVEELEMENT.value = curAe.id;
}

function ClosePage() {
    var main = parent.document.getElementById('main');
    if (null != main)
        main.src = 'Welcome.aspx';
    else
        self.close();
    return false;
}

function PostBack(callEvent, callArgument) {
    SaveActiveElement();
    javascript: __doPostBack(callEvent, callArgument);
}


function OnCtrlChanged(callEvent, callArgument) {
    // FI 2018-07-13 [XXXXX] calling IsCtrlChanged
    if (IsCtrlChanged(callEvent)) {
        PostBack(callEvent, callArgument);
    }
}

// FI 2018-07-13 [XXXXX] Add Method
function IsCtrlChanged(callEvent) {
    var ret = false;
    var ctrl = document.getElementById(callEvent);
    if (null != ctrl) {
        var newValue = null;
        if ((ctrl.type == 'select-one') && (ctrl.selectedIndex != -1)) {
            newValue = ctrl.options[ctrl.selectedIndex].text;
        }
        else {
            newValue = ctrl.value;
        }
        var oldValue = $(ctrl).attr("oldValue");
        if ((oldValue == null) || (oldValue != newValue)) {
            ret = true;
        }
    }
    return ret;
}

/* FI 2020-01-28 [25182] Add Method
 Scan each input with oldvalue attribute. 
 If value have changed set __idLastNoAutoPostbackCtrlChanged Hidden Control
*/
function SetNoAutoPostbackCtrlChanged(pContainerId) {
    var _ctrl = $get('__idLastNoAutoPostbackCtrlChanged');
    if (null != _ctrl) {
        $('#' + pContainerId + ' input[oldvalue] , ' + '#' + pContainerId + ' select[oldvalue]').each(
            function (index) {
                var inputId = $(this).attr('id');
                if (IsCtrlChanged(inputId)) {
                    _ctrl.value += inputId +';';
                }
            }
        );
    }
}


// Get a resource using $ajax. Callback function is called with the retrieve result 
// FI 2021-02-10 Add Method
function GetResource(input, callback) {
    $.ajax({
        url: "WebServices/CommonWebService.asmx/GetResource",
        data: "{'res': '" + input + "'}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (typeof callback === "function") {
                callback(data.d);
            } else {
                throw 'callback is not a function!';
            }
        },
        error: AlertAJaxError
    })
}

// Get array resource using $ajax. Callback function is called with the retrieve result 
// FI 2022-10-24 Add Method
function GetResourceArray(inputArray, callback) {
    let JSONInput = JSON.stringify(inputArray);
    $.ajax({
        url: "WebServices/CommonWebService.asmx/GetResourceArray",
        data: `{'res':${JSONInput}}`,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (typeof callback === "function") {
                callback(data.d);
            } else {
                throw 'callback is not a function!';
            }
        },
        error: AlertAJaxError
    })
}


/**
 * Load Enabled Maturity Rule (without 'Default Rule') . Callback function is called with the retrieve results
 * @param {any} colArray
 * @param {any} callback
 */
function LoadMaturityRule(colArray,  callback) {
    let JSONCol = JSON.stringify(colArray);
    
    $.ajax({
        url: "WebServices/CommonWebService.asmx/LoadMaturityRule",
        data: `{'col':${JSONCol}}`,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (typeof callback === "function") {
                callback(data.d);
            }
        },
        error: AlertAJaxError
    })
}



/**
 * Load data using $ajax. Callback function is called with the retrieve results
 * @param {any} colArray
 * @param {any} table
 * @param {any} restrictArray
 * @param {any} callback
 */
function LoadDataTable(colArray, table, restrictArray, callback) {
    let JSONCol = JSON.stringify(colArray);
    let JSONTable = JSON.stringify(table);
    let JSONRestrict = JSON.stringify(restrictArray);

    $.ajax({
        url: "WebServices/CommonWebService.asmx/LoadDataTable",
        data: `{'col':${JSONCol}, 'table':'${table}', 'restrict': ${JSONRestrict}}`,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (typeof callback === "function") {
                callback(data.d);
            }
        },
        error: AlertAJaxError
    })
}

// FI 20220530 [XXXXX] Add Method
function AlertAJaxError(jqXHR, textStatus, errorThrown) {
    let msg = 'An error occurred while processing your request.\r\n\r\n';

    if (typeof textStatus === 'string' && textStatus.length > 0)
        msg += "Status : " + textStatus + "\r\n";

    if (typeof errorThrown === 'string' && errorThrown.length > 0)
        msg += "HTTP error : " + errorThrown + "\r\n";

    if (typeof (jqXHR.responseJSON) !== 'undefined' && typeof (jqXHR.responseJSON.Message) !== 'undefined') {
        msg += "Message : " + jqXHR.responseJSON.Message + "\r\n";
    }
    else {
        msg += jqXHR.responseText
    }

    // StackTrace is not displayed (StackTrace is on Temporary/Trc_Spheres.txt)
    //if (typeof (jqXHR.responseJSON) !== 'undefined' && typeof (jqXHR.responseJSON.StackTrace) !== 'undefined') {
    //    msg += jqXHR.responseJSON.StackTrace + "\r\n";
    //}

    alert(msg);
    console.error(msg);
}

// FI 20220530 [XXXXX] Add Method
function AutocompleteError(jqXHR, textStatus, errorThrown) {
    if (jqXHR.status == 500 && typeof (jqXHR.responseJSON) !== 'undefined' && typeof (jqXHR.responseJSON.Message) !== 'undefined' && jqXHR.responseJSON.Message.indexOf("maxJsonLength") > -1) {
        console.log(jqXHR.responseJSON.Message);
    }
    else {
        AlertAJaxError(jqXHR, textStatus, errorThrown);
    }
}

// FI 20210218 [XXXXX] Add
function SetJSonDateToControl(controlid, jsonDate) {
    let dt = new Date(parseInt(jsonDate.substr(6)));
    let sdt = new Intl.DateTimeFormat(culture).format(dt);
    $("#" + controlid).val(sdt);
}

// FI 20210218 [XXXXX] Add
function SetJSonDateToControl(controlid, jsonDate) {
    let dt = new Date(parseInt(jsonDate.substr(6)));
    let sdt = new Intl.DateTimeFormat(culture).format(dt);
    $("#" + controlid).val(sdt);
}

function RecallFunctions() {
    if (typeof Toggle === "function") Toggle();
    if (typeof DatePicker === "function") DatePicker();
    if (typeof DateTimePicker === "function") DateTimePicker();
    if (typeof DateTimeOffsetPicker === "function") DateTimeOffsetPicker();
    if (typeof TimePicker === "function") TimePicker();
    if (typeof TimeOffsetPicker === "function") TimeOffsetPicker();
    if (typeof GlobalQTip === "function") GlobalQTip();
    /* FI 2019-12-27 [XXXXX] add LoadAutoCompleteData */
    if (typeof LoadAutoCompleteData === "function") LoadAutoCompleteData();
}

// EG 2021-02-22 New En provenance de Javascript.cs
function DoPostBackImmediate(pEventTarget, pEventArgument) {
    javascript: __doPostBack(pEventTarget, pEventArgument);
}

// EG 2021-02-22 New En provenance de Javascript.cs
function OpenerRefresh(pEventTarget, pEventArgument) {
    try {window.opener.RefreshPage(pEventTarget, "SELFRELOAD_");}
    catch (e) { }
}

// EG 2021-02-22 New En provenance de Javascript.cs
function OpenerReload() {
    try { window.opener.ReloadPage(); }
    catch (e) { }
}

// EG 2021-02-22 New En provenance de Javascript.cs
// EG 20220907 [XXXXX] Test pour raffraichissement automatique du tracker après marquage d'un ligne
function OpenerRefreshAndClose(pEventTarget, pEventArgument)
{
    try {
        if (pEventArgument == 'LoadTracker')
            window.opener._Refresh(pEventTarget, pEventArgument);
        else
            window.opener.RefreshPage(pEventTarget, pEventArgument);
    }
    catch (e) { }
    finally {
        self.close();
    }
}

// EG 20210225 Lien Hyperlink sur Grid
function HL(evt) {
    //window.open("hyperlink.aspx?args=" + evt.args, evt.target);
    // L'appel  de la page est construit en fonction de l'attribut [args]
    var args = $(evt).attr("args");
    var target = $(evt).attr("args");
    if (null != args)
        window.open('hyperlink.aspx?&args=' + args, target);
    args = null;
    target = null;
    return false;

}

// EG 2021-02-22 New En provenance de Javascript.cs
// FI 20210324 [XXXXX] Add Page_ClientValidate
function RefreshPage(pEventTarget, pEventArgument) {
    let isOk = true;

    if (typeof (Page_ClientValidate) == 'function')
        isOk = Page_ClientValidate();

    if (isOk) {
        GetResource("Msg_WaitingRefresh", function (result) {
            Block("<h3 class='blockUI'>" + result + "</h3>");
            if (pEventTarget == '' || pEventTarget == null) {
                pEventTarget = '0';
            }
            if (pEventArgument == '' || pEventArgument == null) {
                pEventArgument = 'SELFRELOAD_';
            }
            __doPostBack(pEventTarget, pEventArgument);
        });
    }
}

// EG 2021-02-22 New En provenance de Javascript.cs
function ReloadPage() {
    var url = window.location.href;
    if (url.search('OCR=1') == -1) {
        url = url + '&OCR=1';
    }
    window.location = url;
}

// EG 2021-02-22 New En provenance de Javascript.cs
function SetColor(pColor, pControlID, pTypeColor) {
    var form = document.forms[0];
    var ctrlSearch_ = form.elements[pControlID];
    if (ctrlSearch_ != null) {
        if (pTypeColor == 'textcolor') {
            ctrlSearch_.style.color = pColor;
        }
        if (pTypeColor == 'backcolor') {
            ctrlSearch_.style.backgroundColor = pColor;
        }
        if (pTypeColor == 'bordercolor') {
            ctrlSearch_.style.borderColor = pColor;
        }
    }
}

// EG 2021-02-22 New En provenance de Capture.js
function OnSubmit(callEvent, callArgument) {
    SaveActiveElement();
    Page_ClientValidate();
    if (Page_IsValid)
        javascript: __doPostBack(callEvent, callArgument);
}

// EG 2021-02-22 New En provenance de Capture.js
function CopyTo(ctrlSource, ctrlTarget, isAlways) {
    try {
        var form = document.forms[0];
        var ctrlTarget_ = form.elements[ctrlTarget];
        if (ctrlTarget_ != null) {
            if (ctrlTarget_.type == 'checkbox')
                ctrlTarget_.status = ctrlSource.status;
            else if (isAlways || (ctrlTarget_.value == ''))
                ctrlTarget_.value = ctrlSource.value;
        }
        else
            alert('CopyTo(), control Target not found: ' + ctrlTarget);
    }
    catch (error) {
        alert('CopyTo(), error: ' + error);
    }
}

// EG 2021-02-22 New En provenance de Javascript.cs
function SetFocus(pControlID) {
    try {
        var form = document.forms[0];
        var ctrl = form.elements[pControlID];
        if (ctrl != null) {
            ctrl.focus();
        }
    }
    catch(error) {}
}

// EG 2021-02-22 New En provenance de Javascript.cs
function EnabledDisabledChecked(pObjCaller, pObjRefIDName, pCompValue, pIsEnabled) {
    var objRef = document.getElementById(pObjRefIDName);
    var enabled = false;
    if (objRef != null) {
        enabled = (pObjCaller.checked == pCompValue);
        if (pIsEnabled) 
            objRef.disabled = !enabled;
        else
            objRef.disabled = enabled;
    }
    return enabled;
}

// EG 2021-02-22 New En provenance de Javascript.cs
function SaveScrollPos() {
    document.forms[0].__SCROLLPOS.value = document.body.scrollTop;
}

// EG 2021-02-22 New En provenance de Validator.js
function ExecuteValidators() {
    Page_ClientValidate();
}

// EG 2021-02-22 New En provenance de Validator.js
function DisableValidators() {
    var i;
    if (null != Page_Validators) {
        for (i = 0; i < Page_Validators.length; i++)
            Page_Validators[i].enabled = false;
    }
}
// EG 2021-02-22 New En provenance de Validator.js
function EnableValidators() {
    var i;
    for (i = 0; i < Page_Validators.length; i++)
        Page_Validators[i].enabled = true;
}

// EG 2021-02-22 New En provenance de FpmlCopyPaste.js
function FpmlCopyPaste(Guid, copyPastePanelID, objectName, fieldName, subTitle) {
    ret = window.open("FpmlCopyPaste.aspx?GUID=" + Guid + '&CopyPastePanelID=' + copyPastePanelID + '&ObjectName=' + objectName + '&FieldName=' + fieldName + '&SubTitle=' + subTitle, "_blank", 'height=300,width=900,location=no,scrollbars=no,status=yes,menubar=no,toolbar=no');
}

// EG 2021-02-22 New En provenance de FpmlCopyPaste.js
function ConfirmDelCopy(msgConfirm, infoCopy1, infoCopy2, infoCopy3, infoCopy4) {
    msgConfirm = msgConfirm.replace("{0}", infoCopy1);
    msgConfirm = msgConfirm.replace("{1}", infoCopy2);
    msgConfirm = msgConfirm.replace("{2}", infoCopy3);
    msgConfirm = msgConfirm.replace("{3}", infoCopy4);
    return window.confirm(msgConfirm);
}

// EG 2021-02-22 New En provenance de FpmlCopyPaste.js
function SetPasteChoice(copyPastePanelID, IDClipBoard) {
    var form = document.forms[0];
    var ctrl = form.elements[copyPastePanelID];
    javascript: __doPostBack(copyPastePanelID, IDClipBoard);
}

// EG 20211217 [XXXXX] Maintien de cet état (Toggle) avant un post de la page.
// Sauvegarde dans le champ __TOGGLESTATUS des état de tous les panels (avec ID) de type toggle
function SaveToggleStatus() {
    if (typeof ($) != 'undefined') {
        if ($("#__TOGGLESTATUS")) {
            var status = "";
            $("div[class*= 'headh']").each(function () {
                var parent = $(this).parent().closest("div");
                if ($(parent) && $(parent).attr("id")) {
                    status += $(parent).attr("id") + "|" + $(this).hasClass("closed") + ";"
                }
            });
            $("#__TOGGLESTATUS").val(status.slice(0,-1));
        }
    }
}
// EG 20211217 [XXXXX] Maintien de cet état (Toggle) avant un post de la page.
// Restauration depuis lecture du champ __TOGGLESTATUS des état de tous les panels (avec ID) de type toggle
function SetToggleStatus() {
    if (typeof ($) != 'undefined') {
        if ($("#__TOGGLESTATUS") && ($("#__TOGGLESTATUS").val() != undefined) && ($("#__TOGGLESTATUS").val() != "")) {
            var arrToggle = $("#__TOGGLESTATUS").val().split(';');
            $.each(arrToggle, function (index, toggle) {
                var toggleValue = toggle.split("|");
                if (toggleValue != undefined) {
                    var isclosed = ("true" == toggleValue[1])
                    if ($("#" + toggleValue[0])) {
                        $("#" + toggleValue[0]).find("div[class*='headh']").toggleClass('closed', isclosed)
                        $("#" + toggleValue[0]).find("div[class='contenth']").css('display', isclosed? "none" : "block");
                    }
                }
            });
        }
    }
}
// Save in Cookie table using $ajax. Callback function is called with the retrieve result 
function SaveSQLCookie(element, value, callback) {
    $.ajax({
        url: "WebServices/CommonWebService.asmx/SaveSQLCookie",
        data: `{'element':'${element}', 'value':'${value}'}`,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        error: AlertAJaxError
    })
}
// EG 20220715 Déplacé de Default.js
// FI 20221118 add alert when Session has expired
// FI 20230613 Session expired and 401 Unauthorized error have now distinct message
//-------------------------------------------------------- //
// Contrôle Session active                                 //
//-------------------------------------------------------- //
// EG 20230309 [26257] Réouverture de la page default sur Retour Session Available = false
function AvailableSession(callback) {
    $.ajax({
        url: "WebServices/CommonWebService.asmx/IsSessionAvailable",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (false == data.d) {
                let msg = 'Your session has expired and you must be re-authenticate.\r\n\r\n';
                alert(msg);
                window.location.href = "Default.aspx";
            }
            else if (typeof callback === "function") {
                callback(JSON.parse(data.d));
            }
        },
        statusCode: {
            401: function () {
                /* 401 Unauthorized occurs after an authentication timeout  */
                let msg = 'Your authentication credentials are not valid and you must be re-authenticate.\r\n\r\n';
                alert(msg);
                window.location.href = "Default.aspx";
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status != 401) {
                AlertAJaxError(jqXHR, textStatus, errorThrown);
            }
        }
    })
}
// EG 20220715 Déplacé de Default.js
//-------------------------------------------------------- //
// Message d'erreur si session expirée                     //
//-------------------------------------------------------- //
// EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
function SessionError(jqXHR, textStatus, errorThrown) {
    let msg = 'Your session has expired and you must be re-authenticate.\r\n\r\n';

    alert(msg);
    window.location.href = "Default.aspx";
}
// Gestion ouverture/Fermeture des groupes de composants (Applée depuis la fenêtre A Propos) 
var hdlr_about = function (event, elt) {
    var isclosed = $(event.target).hasClass("closed");
    $(event.target).next('div').stop(true, true).toggle('fade', '', 'fast');
    $(event.target).toggleClass('closed', (false == isclosed))
}

/** 
 *  localStorage get, set, remove, clear
 */
const storage = {
    get: (key, defaultValue = null) => {
        const value = localStorage.getItem(key);
        return value ? JSON.parse(value) : defaultValue;
    },
    set: (key, value) => localStorage.setItem(key, JSON.stringify(value)),
    remove: (key) => localStorage.removeItem(key),
    clear: () => localStorage.clear(),
};


/**
 * Handling the click, mousemove and other events are mostly implemented with the addEventListener() method.
 * Example : listen(buttonElem, "click", () => console.log("Clicked!"));
 * @param {any} target
 * @param {any} event
 * @param {any} callback
 * @param {...any} options
 */
const listen = (target, event, callback, ...options) => {
    return target.addEventListener(event, callback, ...options);
};