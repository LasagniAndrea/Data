function Refresh(eventTarget, eventArgument) {
    javascript: __doPostBack(eventTarget, eventArgument);
}

// Initialisation principale du Tracker
// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
function InitTracker() {
    if (0 == parent.frames.length)
        $("#btnfloatbar").attr("style", "visibility:hidden");

    $("#toolsbar").addClass("summarytitle");
    // Création des onglets
    CreateTabs("#tabs");
    // Création des accordéons
    CreateAccordion("#accordion_TERMINATED");
    CreateAccordion("#accordion_ACTIVE");
    CreateAccordion("#accordion_REQUESTED");
    CreateAccordionHelp("#accordion_HELP");
    // Tooltip
    TrackerQTip();
    // Type d'affichage des groupes de trackers
    DisplayMaskGroup();
}

// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
function TrackerParam() {
    ret = window.open('TrackerParam.aspx', '_blank', 'height=750,width=1000,resizable=yes,status=yes');
}
function Monitoring() {
    ret = window.open('BusinessMonitoring2.aspx?IDMENU=OTC_VIEW_BUSINESSMONITORING', '_blank');
}

// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 2020-09-22 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Nouveaux Boutons Favori/Groupes
// EG 20220613 [XXXXX] Refactoring Javascript/Jquery
function SwitchMaskGroup() {
    var maskGroup = $("input[id$=hidMaskGroup]");
    maskGroup.attr('value', (maskGroup.attr('value') == "all") ? "detail" : 'all');
    DisplayMaskGroup();
}

// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 20220613 [XXXXX] Refactoring Javascript/Jquery
function DisplayMaskGroup() {
    var maskGroup = $("input[id$=hidMaskGroup]");
    var ressource = maskGroup.attr('title').split(';');
    maskGroup.attr('title', (maskGroup.attr('value') == "all") ? ressource[0] : ressource[1]);

    if (typeof ($) != "undefined") {
        if ($('#btnMaskGroup').data("qtip") != undefined) {
            var api = $('#btnMaskGroup').qtip("api");
            if (null != api)
                api.updateContent($('#btnMaskGroup').attr('alt'));

        }

        var isDisplayAll = (maskGroup.attr('value') == "all");
        $("div[id^='group_']").each(function (index, value) {
            if (0 <= value.id.indexOf("HELP")) {
                value.style.display = 'block';
            }
            else {
                var isAll = value.id.endsWith('_ALL');
                if (isDisplayAll) {
                    value.style.display = isAll ? 'block' : 'none';
                }
                else {
                    value.style.display = isAll ? 'none' : 'block';
                }
            }
        });
    }
}

function TrackerObserver() {
    ret = window.open('TrackerServiceObserver.aspx', '_blank', 'height=560,width=900,resizable=yes,status=yes');
}

// EG 2020-09-22 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Taille fenêtre
// EG 2021-01-28 [XXXXX] Ajout paramètre sur l'ouverture du tracker pour spécifier qu'il est détaché de la Main.
function TrackerFloat() {
    ret = window.open('Tracker.aspx?float=1', '_blank', 'center=yes,height=430,width=350,resizable=yes,status=yes');
}
/* FI 2020-06-02 [25370] add parameter pIsLoadData */
function TrackerDetail(pIsLoadData, pURLParam1, pGroupTracker, pReadyState, pStatusTracker, pDtHistory, pSubTitle) {
    var urlToOpen = "List.aspx?" + "Log=FULLTRACKER_L";
    urlToOpen += "&IDMenu=OTC_VIEW_TRACKER";
    // RD 2013-01-18 [18349] Ajout "&P1=ONLYSIO" pour Vision et Portail
    if (null != pURLParam1)
        urlToOpen += "&P1=" + pURLParam1;
    urlToOpen += "&TF=Tracker";
    if (null != pGroupTracker)
        urlToOpen += "_" + pGroupTracker
    if (null != pReadyState)
        urlToOpen += "_" + pReadyState
    if (null != pStatusTracker)
        urlToOpen += "_" + pStatusTracker
    if (pIsLoadData)
        urlToOpen += "&isLoadData=1";
    else
        urlToOpen += "&isLoadData=0";
    urlToOpen += "&M=4";
    urlToOpen += "&InputMode=2";

    urlToOpen += "&DA=";
    //PL 2016-09-14
    /* FI 2020-06-02 [25370] all DA are added on URL (null value allowed) */
    if (null != pDtHistory)
        urlToOpen += "[[Data name='DTHISTORY' datatype='date']]" + pDtHistory + "[[/Data]];";
    else
        urlToOpen += "[[Data name='DTHISTORY' datatype='date']]null[[/Data]];";
    if (null != pGroupTracker)
        urlToOpen += "[[Data name='GROUPTRACKER' datatype='string']]" + pGroupTracker + "[[/Data]];";
    else
        urlToOpen += "[[Data name='GROUPTRACKER' datatype='string']]null[[/Data]];";
    if (null != pReadyState)
        urlToOpen += "[[Data name='READYSTATE' datatype='string']]" + pReadyState + "[[/Data]];";
    else
        urlToOpen += "[[Data name='READYSTATE' datatype='string']]null[[/Data]];";
    if (null != pStatusTracker)
        urlToOpen += "[[Data name='STATUSTRACKER' datatype='string']]" + pStatusTracker + "[[/Data]];";
    else
        urlToOpen += "[[Data name='STATUSTRACKER' datatype='string']]null[[/Data]];";

    urlToOpen += "&SubTitle=" + pSubTitle;

    ret = window.open(urlToOpen);
}

function OnTrackerRefresh() {
    $("#boardInfo").empty().load('Tracker.aspx #trackerContainer', function () { InitTracker(); });
}

// Initialisation des onglets du tracker (3 appels pour: ACTIVE, REQUESTED et TERMINATED)
// Sauvegarde de l'onglet actif dans le cookie
// Modification de la position des titres des onglets (TOP)
// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 2021-01-20 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
// EG 2021-01-20 [25556] Nouvelle gestion du cookie (attribut JQuery DEPRECATED)
// EG 2021-01-26 [25556] Gestion Samesite (Strict) sur cookie
function CreateTabs(elt) {
    var activeTab = Cookies.get('Trk-activetab');
    if (null == activeTab) { activeTab = 2;}
    $(elt).tabs({
        active: activeTab,
        activate: function (event, ui) {
            Cookies.set('Trk-activetab', ui.newTab.index(), {expires: 1, sameSite: "strict"});
        }
    });
    $(elt).addClass("tabs-top");
}
// Fermeture de tous les accordéons
function CollapseAccordion() {
    $('.ui-accordion-header').removeClass('ui-accordion-header-active ui-state-active ui-corner-top').addClass('ui-corner-all').attr({
        'aria-selected': 'false',
        'tabindex': '-1'
    });
    $('.ui-accordion-content').removeClass('ui-accordion-content-active').attr({
        'aria-expanded': 'false',
        'aria-hidden': 'true'
    }).hide();
}

// Initialisation des accordéons du tracker (3 appels pour: ACTIVE, REQUESTED et TERMINATED)
// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 2021-01-20 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
// EG 2021-01-20 [25556] Nouvelle gestion de l'accordéon actif (multiplicateur de 2) pour une raison inexpliquée
// EG 2021-01-20 [25556] Evénement d'activation de l'accordéon (activate à la place de change)
// EG 2021-01-26 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
// EG 2021-01-26 [25556] Gestion Samesite (Strict) sur cookie
// EG 20220613 [XXXXX] Refactoring Javascript/Jquery
function CreateAccordion(elt) {
    // On se récupère l'accordéon actif
    var activeIndex = GetAccordionActiveIndex() * 2;

    // Paramètre par défaut de l'accordéon
    $(elt).accordion({
        header: "> div > div",
        collapsible: true,
        fillSpace: true,
        icons: false,
        active: false
    });

    // Déactivation de l'ouverture de l'accordéon
    // sur le bouton (image) d'accès au détail d'un item de l'accordéon
    $("input[id^=content_]").on("click", function (e) { e.preventDefault(); e.stopPropagation(); });
    // sur le bouton (image) d'accès au tooltip d'affichage des totaux d'un item de l'accordéon
    $("span[id^=cpt_]").on("click", function (e) { e.stopPropagation(); });
    $("span[id^=xcpt_]").on("click", function (e) { e.stopPropagation(); });

    // Ouverture du group = Chargement des lignes TRACKER le concernant (appel à TrackerContent.aspx)
    $(elt).on("accordionactivate", function (event, ui) {
        // Sauvegarde dans le cookie du nouvel accordéon actif
        var maskGroup = $("input[id$=hidMaskGroup]");

        if (maskGroup.attr('value') == "all")
            Cookies.set('TrkAll-ActiveIndex', ui.newHeader.parent("div[id^=group_]").prevAll().length, { expires: 1, sameSite: "strict" });
        else
            Cookies.set('TrkActiveIndex', ui.newHeader.parent("div[id^=group_]").prevAll().length, { expires: 1, sameSite: "strict" });
        // Chargement du détail du tracker
        LoadTrackerContent(elt, ui);
        ResizeActiveAccordion(elt);
    });

    // Activation de l'accordéon
    $(elt).accordion({ active: activeIndex });
}

// Initialisation des accordéons du tracker (3 appels pour: ACTIVE, REQUESTED et TERMINATED)
// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 2021-01-20 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
// EG 2021-01-20 [25556] Nouvelle gestion de l'accordéon actif (multiplicateur de 2) pour une raison inexpliquée
// EG 2021-01-20 [25556] Evénement d'activation de l'accordéon (activate à la place de change)
// EG 2021-01-26 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
// EG 2021-01-26 [25556] Gestion Samesite (Strict) sur cookie
function CreateAccordionHelp(elt) {
    // On se récupère l'accordéon actif
    var activeIndex = Cookies.get("TrkHelp-ActiveIndex")*2;

    // Paramètre par défaut de l'accordéon
    $(elt).accordion({
        header: "> div > div",
        collapsible: true,
        fillSpace: true,
        icons: false,
        active: false
    });

    // Ouverture du group = Chargement des lignes TRACKER le concernant (appel à TrackerContent.aspx)
    $(elt).on("accordionactivate", function (event, ui) {
        // Sauvegarde dans le cookie du nouvel accordéon actif
        Cookies.set('TrkHelp-ActiveIndex', ui.newHeader.parent("div[id^=group_]").prevAll().length, { expires: 1, sameSite: "strict" });
    });

    // Activation de l'accordéon
    $(elt).accordion({ active: activeIndex });
}

// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 20220613 [XXXXX] Refactoring Javascript/Jquery
function ResizeActiveAccordion(elt) {
    var _element = GetAccordionActiveElement(elt).next("div");
    if (null != _element) {
        var nextheight = _element.parent().offset().top;
        _element.parent().nextAll().each(function (index, value) {
            nextheight = nextheight + value.clientHeight + 5;
        });
        var x = Math.min(360, 360 - nextheight);
        _element.height(x);
    }
    _element = null;

}
// Chargement du détail du tracker en fonction de GROUP/READYSTATE/STATUS stockée dans l'ID du titre de l'accordéon et l'attribut STATUS
// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 2020-08-18 [XXXXX] Réduction Accès ReadSQLCookie (TrackerHistoric) sur Tracker utilisation Contrôle Hidden pour stockage valeur
// EG 20220613 [XXXXX] Refactoring Javascript/Jquery
function LoadTrackerContent(elt, ui) {
    var _header = ui.newHeader.find("span[id^=lblgroup]");
    var histo = $("input[id$='hidHisto']").attr('value');

    //var histo = document.forms[0].hidHisto.value;
    if ((null != _header) && (1 == _header.length)) {
        // Chargement du détail du tracker en fonction de:
        // GROUP/READYSTATE stockée dans l'attribut [id]  et 
        // STATUS stocké dans l'attribut [status] 
        var groupname = _header.attr("id").split("_");
        var status = _header.attr("status");
        // Container de réception des lignes détails
        var _element = GetAccordionActiveElement(elt).next("div");
        if (null != _element) {
            _element.load('TrackerContent.aspx?mode=GRPLOAD&group=' + groupname[2] + "&readystate=" + groupname[1] + "&status=" + status + "&histo=" + histo,
                null, function () { TrackerContentIsLoad(); });
        }
        _element = null;
        status = null;
        groupname = null;
        _header = null;
    }
}

// retourne l'accordéon actif (lecture cookie)
// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 2021-01-26 [25556] Gestion Samesite (Strict) sur cookie
// EG 20220613 [XXXXX] Refactoring Javascript/Jquery
function GetAccordionActiveElement(elt) {
    // Lecture de l'index de l'accordéon actif dans le cookie
    var activeIndex = null;
    var maskGroup = $("input[id$=hidMaskGroup]");

    if (maskGroup.attr('value') == "all") {
        activeIndex = Cookies.get("TrkAll-ActiveIndex");
        if (null == activeIndex) activeIndex = 0;
    }
    else {
        activeIndex = Cookies.get("TrkActiveIndex");
        if (null == activeIndex) activeIndex = 5;
    }
    // On se récupère l'accordéon actif
    var parentActiveElement = $(elt + "> div[id^=group_]").eq(activeIndex);
    activeIndex = null;
    return parentActiveElement.children("div").first();
}
// EG 2021-01-26 [25556] Gestion Samesite (Strict) sur cookie
function GetAccordionActiveIndex() {
    // Lecture de l'index de l'accordéon actif dans le cookie
    var activeIndex = null;
    var maskGroup = $("input[id$=hidMaskGroup]");

    if (maskGroup.attr('value') == "all") {
//    if (document.forms[0].hidMaskGroup.value == "all") {
        activeIndex = Cookies.get("TrkAll-ActiveIndex");
        if (null == activeIndex) activeIndex = 0;
    }
    else {
        activeIndex = Cookies.get("TrkActiveIndex");
        if (null == activeIndex) activeIndex = 5;
    }
    return activeIndex;
}

// Construction des Tooltips de la page détail fraichement chargée
// EG 2021-01-26 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
function TrackerContentIsLoad() {
    TrackerQTip();
    $("li span").on("click", function (e) { e.stopPropagation(); });
}
// EG 2020-09-22 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Look Tooltip
function TrackerQTip() {
    if (typeof ($) != "undefined") {
        $.fn.qtip.styles.QTip_na =
        {
            name: 'light', width: { min: 100, max: 180 }, border: { width: 1, radius: 0, color: '#A9A9A9' },
            tip: { corner: 'topLeft', color: '#A9A9A9', size: { x: 8, y: 8 } }
        };
    };

    $('*[alt]').each(function () {
        var tt_content = false;
        var tt_title = false;
        var tt_style = 'QTip_na';
        var tt_button = false;
        if ($(this).attr('title'))
            tt_title = $(this).attr('title');
        if ($(this).attr('qtiplevel'))
            tt_style = 'QTip_' + $(this).attr('qtiplevel');
        if ($(this).attr('alt'))
            tt_content = $(this).attr('alt');
        if (tt_content == tt_title)
            tt_title = false;

        if ($(this).attr('qtipbutton')) {
            tt_button = $(this).attr('qtipbutton');
            $(this).qtip({
                content: {
                    text: tt_content,
                    title: { text: tt_title, button: tt_button }
                },
                position: {
                    target: 'mouse', type: 'fixed', container: $(document.body),
                    corner: { target: 'bottomRight', tooltip: 'topLeft' },
                    adjust: { x: 0, y: 0, mouse: true, screen: true, scroll: true, resize: true }
                },
                show: { delay: 140, solo: true, ready: false, when: { target: false, event: 'click' }, effect: { type: 'fade', length: 100 } },
                hide: { delay: 0, fixed: true, when: { target: false, event: 'unfocus' }, effect: { type: 'slide', length: 100 } },
                style: {
                    name: 'light', width: { min: 0, max: 150 }, border: { width: 1, color: '#A9A9A9' },
                    tip: { corner: 'leftMiddle', color: '#000000', size: { x: 15, y: 15 } }
                }
            });
        }
        else {
            $(this).qtip({
                content: {
                    text: tt_content,
                    title: { text: tt_title }
                },
                position: {
                    target: 'mouse', type: 'fixed', container: $(document.body),
                    corner: { target: 'bottomRight', tooltip: 'topLeft' },
                    adjust: { x: 0, y: 0, mouse: true, screen: true, scroll: true, resize: true }
                },
                style: tt_style
            });
        }
        this.removeAttribute('title');
        this.removeAttribute('alt');
        this.removeAttribute('qtiplevel');
        this.removeAttribute('qtipbutton');
    });
};

