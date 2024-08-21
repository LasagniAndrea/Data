// Timer pour intervalle de raffraichissement du Tracker
var trackerinterval;

//------------------------------------------------- //
// Gestion de la touche contrôle pour ouverture     //
// d'un menu dans Main ou nouvel onglet             //
//------------------------------------------------- //
var cntrlIsPressed = false;
var hdlr_keydown = function (event) {
    if (event.ctrlKey)
        cntrlIsPressed = true;
};
var hdlr_keyup = function (event) {
    if (event.ctrlKey)
        cntrlIsPressed = false;
};

//============================================== //
// Evénements du tracker                         //
//============================================== //

//------------------------------------------------- //
// Activation d'un onglet du tracker (ACTIVATE TAB) //
//------------------------------------------------- //
var hdlr_tabs = function (event, ui) {
    var tabactive = $(this).tabs("option", "active");
    var elt = "#" + ui.newPanel.attr("id") + " > div";
    // Recherche du groupe actif
    var activeIndex = _GetAccordionActiveIndex() * 2;
    // Activation de son container
    $(elt).accordion({ active: activeIndex });
}

//----------------------------------------------- //
// Ouverture d'un groupe (ACTIVATE ACCORDION)     //
// Chargement des lignes du tracker le concernent //
// Appel à trackerContent.aspx                    //
//----------------------------------------------- //
var hdlr_accordion = function (event, ui) {

    var elt = "#" + $(this).attr("id");

    // Sauvegarde dans le cookie du nouvel accordéon actif (en fonction du mode : Détail ou Favori)
    var maskGroup = $("input[id$='hidMaskGroup']");
    if (maskGroup.attr('value') == "all")
        Cookies.set('TrkAll-ActiveIndex', ui.newHeader.parent("div[id^=group_]").prevAll().length, { expires: 1, sameSite: "strict" });
    else
        Cookies.set('TrkActiveIndex', ui.newHeader.parent("div[id^=group_]").prevAll().length, { expires: 1, sameSite: "strict" });

    // Chargement du détail du tracker pour le groupe actif
    _LoadTrackerContent(elt, ui);

    // Resize du container du groupe actif
    _ResizeActiveTrackerGroup(elt);
}

//----------------------------------------------- //
// Resize de la fenêtre Windows                   //
//----------------------------------------------- //
var hdlr_windowresize = function (event) {
    var doIt;
    clearTimeout(doIt);
    doIt = setTimeout(ResizeAll(), 500);
}

//----------------------------------------------- //
// Gestion du container du menu de la bannière    //
// Activation si click                            //
// Désactivation si on le quitte                  //
//----------------------------------------------- //
var hdlr_mainmenuopen = function (event, elt) {
    $("#user-box-content").toggleClass("active");
}
var hdlr_mainmenuleave = function (event, elt) {
    $("#user-box-content").removeClass("active");
}

//-------------------------------------------------------------------- //
// Gestion du chargement d'une page dans l'iFrame principal (main)     //
// Les règles de sécurité peuvent bloquer l'ouverture du page sur un   //
// iFrame.                                                             //
// C'est le cas lorsqu'un IdP cherche à rediriger une fenêtre de Login //
// après une fin de session                                            //
// et que ces paramètre de sécurité interdise l'ouverture dans un      //
// dans un iFrame dont il n'est pas propriétaire.                      //
// <add name="X-Frame-Options" value="sameorigin|DENY" />              //
//-------------------------------------------------------------------- //
// EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
$('#main').on('load', function () {
    try {
        ($(this)[0].contentDocument || $(this)[0].contentWindow).location.href;
    }
    catch (err) {
        if ("FedAuthLogout.aspx" != $(this).attr("src"))
            window.document.location.href = 'fedAuthTimeout.aspx'
        else
            window.document.location.href = 'Default.aspx'
    }
});

//-------------------------------------------------------------------- //
// Lecture et affichage des Notifications de fin de traitement         //
// des services                                                        //
//-------------------------------------------------------------------- //
function _GetAck() {
    
    GetMessageCompleted(function (result) {
        if ((null != result) && (null != result.Second) && (result.Second.length > 0)) {
            GetResource("REQUESTERRESPONSE", function (title) {
                $('#main')[0].contentWindow.OpenMainDialog(title, result.Second, result.First, "150", "250", "auto", "400");
            });
        }
    });
}
//-------------------------------------------------------------------------- //
// Lecture et affichage des messages éventuels générés lors de la connexion  //
//-------------------------------------------------------------------------- //
// EG 20221121 New : Affichage message géré via Default.js 
function _DisplayMessageAfterOnConnectOk() {

    GetMessageAfterOnConnectOk(function (msg) {
        if ((null != msg) && (null != msg.Item3) && (msg.Item3.length > 0)) {
            GetResource("REQUESTERRESPONSE", function (title) {
                $('#main')[0].contentWindow.OpenMainDialog(msg.Item1, msg.Item3, msg.Item2, "150", "250", "auto", "400");
            });
        }
    });
}

//-------------------------------------------------------------------- //
// Chargement du sommaire et du tracker une fois le document prêt      //
//  - pas de tracker en mode portail                                   //
//-------------------------------------------------------------------- //
// EG 20230123 [26235][WI543] Refactoring
// EG 20230130 [26235][WI543] Mise en commentaire _SlideContnr
function _LoadData() {
    // On masque les sections Summary et Tracker
//    _SlideContnr($("#btnSlidesummary"));
//    _SlideContnr($("#btnSlidetracker"));

    const contnr = $("#contnr");
    
    contnr.removeClass("portal");

    // Sommaire
    LoadSummary();
    var portal = $("input[id$='hidIsPortal']");

    // Tracker 
    if (portal.attr('value') == "true") {
        contnr.addClass("portal");
        _BindEvent()
    }
    else {
        // Affichage par groupe ou favori
        _DisplayTrackerGroupFavorite();
        // Chargement
        _LoadTracker();
        // Récupération des paramètres de rafraichissement du tracker pour application
        _SetTrackerAutoRefresh(false);
        // Gestion scrolling du bandeau EntityMarket
        _ScrollEntityMarket();
        // Lecture Réponses de services
        _GetAck();
    }
}

//-------------------------------------------------------------------- //
// Gestion des tooltip des lignes détails du Tracker                   //
//-------------------------------------------------------------------- //
// EG 20240619 [WI945] Security : New QTip2
function _TrackerQTip() {
    $('#tracker *[qtip-alt]').each(function () {
        var tt_content = $(this).attr('qtip-alt');
        var tt_title = false;
        var tt_style = 'qtip-light';

        var tt_button = false;

        if ($(this).attr('title'))
            tt_title = $(this).attr('title');
        if (tt_content == tt_title)
            tt_title = false;

        if ($(this).attr('qtip-button')) {
            tt_button = $(this).attr('qtip-button');
            $(this).qtip({
                content: {
                    attr: 'qtip-alt',
                    text: tt_content,
                    title: { text: tt_title, button: tt_button }
                },
                position: { my: 'center left', at: 'right center', adjust: { method: 'flip shift' }, viewport: $('#tabs') },
                show: { delay: 140, solo: true, ready: false, target: false, event: 'click', effect: { type: 'fade', length: 100 } },
                hide: { delay: 0, fixed: true, target: false, event: 'click', effect: { type: 'slide', length: 100 } },
                style: { classes: 'qtip-blue' }
            });
        }
        else {
            $(this).qtip({
                content: {
                    attr: 'qtip-alt',
                    text: tt_content,
                    title: { text: tt_title }
                },
                position: { my: 'center left', at: 'right center', adjust: { method: 'flip shift' }, viewport: $('#tabs') },
                style: { classes: tt_style }
            });
        }
        this.removeAttribute('title');
        this.removeAttribute('qtip-alt');
        this.removeAttribute('qtip-style');
        this.removeAttribute('qtip-button');
    });
};

//---------------------------------------------- //
// Resize du container du menu actif du sommaire //
//---------------------------------------------- //
function ResizeAll() {
    MenuResize("#" + $("div[id^=contnr] > div.active").attr("id"));
}

//------------------------------------------------------------- //
// Liaison des événements liés aux handlers déclarés plus haut  //
//------------------------------------------------------------- //
function _BindEvent() {
    const doc = $(document);

    doc.unbind("keydown", hdlr_keydown).bind("keydown", hdlr_keydown);
    doc.unbind("keyup", hdlr_keyup).bind("keyup", hdlr_keyup);

    $("#tabs").unbind("tabsactivate", hdlr_tabs).bind("tabsactivate", hdlr_tabs);
    $("div[id^='accordion_']").unbind("accordionactivate", hdlr_accordion).bind("accordionactivate", hdlr_accordion);
    $(window).unbind("resize", hdlr_windowresize).bind("resize", hdlr_windowresize);
    $("#user-box").unbind("click", hdlr_mainmenuopen).bind("click", hdlr_mainmenuopen);
    $("#mastermenu").unbind("mouseleave", hdlr_mainmenuleave).bind("mouseleave", hdlr_mainmenuleave);
}


//-------------------------------------------------------- //
// Ouverture d'un menu du sommaire                         //
// dans un nouvel onglet ou dans la section Main (iFrame)  //
//-------------------------------------------------------- //
function OM(evt, id) {
    if (cntrlIsPressed) {
        _target = "_blank";
        cntrlIsPressed = false;
    }
    else {

        var _target = "main";
        var mainPage = $('#main')[0];
        if (null == mainPage)
            mainPage = document.getElementById('main');
        if (null != mainPage) {
            if (mainPage.contentWindow.cntrlIsPressed) {
                _target = "_blank";
                mainPage.contentWindow.cntrlIsPressed = false;
            }
        }
    }
    AvailableSession(function (result) {
        if (result) {
            window.open("hyperlink.aspx?mnu=" + id, _target);
        }
    });
}

//-------------------------------------------------------- //
// Scrolling des données EntityMarket                      //
//-------------------------------------------------------- //
function _ScrollEntityMarket() {
    $(document).ready(function () {
        if ($("input[id$=__ISSCROLL]").val() == "true")
            $("ul[id$=entityMarket]").infiniteslide({ speed: 80 });
        else
            $("ul[id$=entityMarket]").stop();
    });
}

//-------------------------------------------------------- //
// Chargement du sommaire via Jquery                       //
// en ne récupérant que les données présentent dans le tag //
// TblShowMenu                                             //
// Affichage d'un menu normal ou allégé en fonction des    //
// paramètres                                              //
//-------------------------------------------------------- //
// EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
// EG 20221121 New : Affichage message géré via Default.js (Méthode _DisplayMessageAfterOnConnectOk)
// EG 20230123 [26235][WI543] Refactoring
function LoadSummary() {
    $("#summary").load("sommaire.aspx?default=1 #TblShowMenu");
    // Lecture de l'enum ActionOnConnect retourné à la connexion et gestion en conséquence  
    _DisplayActionOnConnect();
}

//-------------------------------------------------------- //
// Chargement du tracker                                   //
// en ne récupérant que les données présentent dans le tag //
// tabs                                                    //
// Un contrôle de session available est opéré avant        //
// si la session est morte retour à Login (IdP / RDBMS)    //
// Lecture et affichage des éventuelles notification de    //   
// fin de traitement par les services                      //
//-------------------------------------------------------- //
// EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
function _LoadTracker() {
    // Contrôle de session disponible
    AvailableSession(function (result) {
        if (result) {

            // Suppression des toolstip figés
            $("div[class^=qtip]").filter("[qtip]").css("display", "none");


            $("#tracker").empty().load("tracker.aspx?default=1 #tabs", function () {

                // Activation des liaisons événementielles
                _BindEvent();

                // Initialisation du tracker chargé
                _InitTracker();

                $("#tabs").attr("style", "visibility:visible");
                $.unblockUI();

            });
        }
    });
}

function _InitTracker() {
    // Création des onglets
    _SetTabs();
    // Création des accordéons
    _SetAccordion();
    // Activation du groupe pour chaque accordéon
    _ActivateTrackerGroup();
    // Empecher le click sur les compteur du tracker (evite ouverture/fermeture du panel)
    _DeactivatePropagationOnCpt();
    // Tooltip
    //TrackerQTip2();
    // Type d'affichage des groupes de trackers
    _DisplayMaskGroup();
}
// EG 20240523 [XXXXX] Tracker : Correction Tooltip manquant sur bouton(s) et Gestion icon sur Favorites/Groups
function _DisplayMaskGroup() {
    const maskGroup = $("input[id$=hidMaskGroup]");
    var ressource = maskGroup.attr('title').split(';');
    var isDisplayAll = (maskGroup.attr('value') == "all");
    $("a[id$=btnMaskGroup]").attr('title', isDisplayAll ? ressource[0] : ressource[1]);

    if (typeof ($) != "undefined") {

        const btn = $("a[id$=btnMaskGroup]");
        if (btn.data("qtip") != undefined) {
            var api = btn.qtip("api");
            if (null != api)
                api.updateContent(btn.attr('alt'));

        }

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

//------------------------------------------------------------------------ //
// Initialisation de tabs                                                  //
// Remplace appel CreateTabs("#tabs") (Tracker.js)                         //
//------------------------------------------------------------------------ //
function _SetTabs() {
    var activeTab = Cookies.get('Trk-activetab');
    if (null == activeTab) { activeTab = 2; }
    $("#tabs").tabs({
        active: activeTab,
        activate: function (event, ui) {
            Cookies.set('Trk-activetab', ui.newTab.index(), { expires: 1, sameSite: "strict" });
        }
    }).addClass("tabs-top");
}

//------------------------------------------------------------------------ //
// Initialisation des accordions                                           //
// Remplace les 3 appels CreateAccordion (Tracker.js)                      //
// ("#accordion_TERMINATED", "#accordion_REQUESTED", "#accordion_ACTIVE")  //
//------------------------------------------------------------------------ //
function _SetAccordion() {
    $('div[id^=accordion_]').accordion({
        header: "> div > div",
        collapsible: true,
        fillSpace: true,
        icons: false,
        active: false
    });
}

//------------------------------------------------------------------------ //
// Chargement du détail du tracker en fonction de GROUP/READYSTATE/STATUS  //
// stockés dans l'ID du titre de l'accordéon et l'attribut STATUS          //
//------------------------------------------------------------------------ //
// EG 20221104 Lecture des paramètres avant raffraichissement
function _LoadTrackerContent(elt, ui) {
    if ((-1 == elt.indexOf("HELP")) && (0 < ui.newHeader.length)) {
        AvailableSession(function (result) {
            if (result) {
                const _header = ui.newHeader.find("span[id^=lblgroup]");
                var histo = $("input[id$='hidHisto']").attr('value');

                TrackerRefreshParam(function (ret) {
                    histo = ret.Item3;
                    if ((null != _header) && (1 == _header.length)) {
                        // Chargement du détail du tracker en fonction de:
                        // - GROUP/READYSTATE stockée dans l'attribut [id]  et 
                        // - STATUS stocké dans l'attribut [status] 
                        var groupname = _header.attr("id").split("_");
                        var status = _header.attr("status");
                        // Container de réception des lignes détails
                        var _element = _GetAccordionActiveElement(elt).next("div");
                        if (null != _element) {
                            _element.load('TrackerContent.aspx?mode=GRPLOAD&group=' + groupname[2] + "&readystate=" + groupname[1] + "&status=" + status + "&histo=" + histo,
                                null, function () { _TrackerContentIsLoad(); });
                        }
                        _element = null;
                        status = null;
                        groupname = null;
                    }
                });
            }
        });
    }
}

function _TrackerContentIsLoad() {
    _TrackerQTip();
    $("li span").on("click", function (e) { e.stopPropagation(); });
}

//-------------------------------------------- //
// retourne l'accordéon actif (lecture cookie) //
//-------------------------------------------- //
function _GetAccordionActiveElement(elt) {
    // Lecture de l'index de l'accordéon actif dans le cookie
    var activeIndex = _GetAccordionActiveIndex();
    // On se récupère l'accordéon actif
    var parentActiveElement = $(elt + "> div[id^=group_]").eq(activeIndex);
    activeIndex = null;
    return parentActiveElement.children("div").first();
}

//-------------------------------------------- //
// Retourne l'index de l'accordion actif       //
//-------------------------------------------- //
function _GetAccordionActiveIndex() {
    // Lecture de l'index de l'accordéon actif dans le cookie
    var activeIndex = null;
    const maskGroup = $("input[id$=hidMaskGroup]");

    if (maskGroup.attr('value') == "all") {
        activeIndex = Cookies.get("TrkAll-ActiveIndex");
        if (null == activeIndex) activeIndex = 0;
    }
    else {
        activeIndex = Cookies.get("TrkActiveIndex");
        if (null == activeIndex) activeIndex = 5;
    }
    return activeIndex;
}

//-------------------------------------------- //
// Déactivation de l'ouverture de l'accordéon  //
// sur les boutons (compteurs) d'un élément de //
// l'accordéon                                 //
//-------------------------------------------- //
// EG 20221104 l'accès au détail d'un groupe n'est plus un <input> mais un <a>
function _DeactivatePropagationOnCpt() {
    $("a[id^=content_]").on("click", function (e) { e.preventDefault(); e.stopPropagation(); });
    $("span[id^=cpt_]").on("click", function (e) { e.stopPropagation(); });
    $("span[id^=xcpt_]").on("click", function (e) { e.stopPropagation(); });
}


//-------------------------------------------------------- //
// Masquer/Afficher le container du Sommaire et/ou Tracker //
//-------------------------------------------------------- //
function _SlideContnr(elt) {
    var id = "#" + elt.id;
    var current = $(elt).attr("id").replace("btnSlide", "");
    const contnr = $("#contnr");

    contnr.toggleClass("close" + current);
    var delsuffix = (contnr.hasClass("close" + current) ? "left" : "right");
    var addsuffix = (contnr.hasClass("close" + current) ? "right" : "left");
    $(id + " i").removeClass("fa-angle-double-" + delsuffix).addClass("fa-angle-double-" + addsuffix);
    ResizeAll();
}

//-------------------------------------------------------- //
// Raffraichissement du tracker après modification de ses  //
// paramètres depuis la forme TrackerParam.aspx            //
//-------------------------------------------------------- //
function _Refresh(eventTarget, eventArgument) {
    switch (eventArgument) {
        case "LoadTracker":
            _SetTrackerAutoRefresh(false);
            _LoadTracker();
            break;
        default: //        case "LoadParam":
            _SetTrackerAutoRefresh(false);
            _LoadTracker();
            _GetAck();
            break;

    }
}

//-------------------------------------------------------- //
// Affichage de la fenêtre de Monitoring                   //
//-------------------------------------------------------- //
function _OpenMonitoring() {
    ret = window.open('BusinessMonitoring2.aspx?IDMENU=OTC_VIEW_BUSINESSMONITORING', '_blank');
}

//-------------------------------------------------------- //
// Affichage de la fenêtre d'observation des services      //
//-------------------------------------------------------- //
function _OpenTrackerObserver() {
    ret = window.open('TrackerServiceObserver.aspx', '_blank', 'height=560,width=900,resizable=yes,status=yes');
}

//-------------------------------------------------------- //
// Affichage de la fenêtre des paramètres du tracker       //
//-------------------------------------------------------- //
function _OpenTrackerParam() {
    ret = window.open('TrackerParam.aspx', '_blank', 'height=750,width=1000,resizable=yes,status=yes');
}

//----------------------------------------------------------------- //
// Construction et ouverture de l'URL d'accès au détail du tracker  //
// en fonction des paramètres Groupe, ReadyState, Status            //
//----------------------------------------------------------------- //
// EG 20221104 New
// EG 20221030 Upd (ajout récupération de la date histo en cours)
function TrackerDetail(pIsLoadData, pURLParam1, pGroupTracker, pReadyState, pStatusTracker, pDtHistory, pSubTitle) {

    GetDateTrackerDetail(function (DtTrackerDet) {

        pDtHistory = DtTrackerDet;
        var urlToOpen = "List.aspx?" + "Log=FULLTRACKER_L";
        urlToOpen += "&IDMenu=OTC_VIEW_TRACKER";
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
        /* FI 2020-06-02 [25370] all DA are added on URL (null value allowed) */
        urlToOpen += "[[Data name='DTHISTORY' datatype='date']]" + ((null != pDtHistory) ? pDtHistory : "null") + "[[/Data]];";
        urlToOpen += "[[Data name='GROUPTRACKER' datatype='string']]" + ((null != pGroupTracker) ? pGroupTracker : "null") + "[[/Data]];";
        urlToOpen += "[[Data name='READYSTATE' datatype='string']]" + ((null != pReadyState) ? pReadyState : "null") + "[[/Data]];";
        urlToOpen += "[[Data name='STATUSTRACKER' datatype='string']]" + ((null != pStatusTracker) ? pStatusTracker : "null") + "[[/Data]];";
        urlToOpen += "&SubTitle=" + pSubTitle;

        DtTrackerDet = window.open(urlToOpen);

    });
}

//-------------------------------------------------------- //
// Ouverture/Fermeture d'un groupe de menu du sommaire     //
//-------------------------------------------------------- //
function _MenuToggle(id, level) {

    // Activation/Désactivation
    if (1 == level) {
        $("div[id^=contnr] > div[id^=mnu]:first-child").each(function () {
            if ($(this).attr("id") != $("#mnu" + id).attr("id"))
                $(this).removeClass("active");
        });
    }
    $("#mnu" + id).toggleClass("active");

    // Changement icone ouverture/fermeture
    if ($("#mnu" + id).hasClass("active"))
        $("#mnu" + id + " > span > i").toggleClass("fa-angle-right").addClass("fa-angle-down");
    else
        $("#mnu" + id + " > span > i").toggleClass("fa-angle-down").addClass("fa-angle-right");

    // Resize du groupe de menu ouvert
    MenuResize("#mnu" + id);
}

//-------------------------------------------------------- //
// Switch entre Menu allégé et menu complet                //
//-------------------------------------------------------- //
function _SwitchMenuToggle() {
    const hidmask = $("input[id$='hidMaskMenu']");
    var isout = (hidmask.attr("value") == "out");
    hidmask.attr("value", isout ? "in" : "out");

    // Changement icone menu allégé/complet
    $("#btnMaskMnu > i").removeClass(isout ? "fa-list" : "fa-filter").addClass(isout ? "fa-filter" : "fa-list");

    // Affichage des menus
    _DisplayMenuToggle();
}

//-------------------------------------------------------- //
// Affichage des items en fonction Menu allégé ou complet  //
//-------------------------------------------------------- //
function _DisplayMenuToggle() {
    const hidmask = $("input[id$='hidMaskMenu']");
    const btn = $('#btnMaskMnu');
    var isout = (hidmask.attr("value") == "out");
    var ressource = hidmask.attr("title").split(';');

    $("#btnMaskMnu > i").attr("title", ressource[isout ? 0 : 1]);

    if (btn.data("qtip") != undefined) {
        var api = btn.qtip("api");
        if (null != api)
            api.updateContent(btn).attr('alt');
    };

    $(".mask").removeClass(isout ? "off" : "on").addClass(isout ? "on" : "off");
}

//-------------------------------------------------------- //
// Switch entre Tracker par groupes ou favori              //
// Un contrôle de session available est opéré avant        //
// si la session est morte retour à Login (IdP / RDBMS)    //
// Lecture et affichage des éventuelles notification de    //   
// fin de traitement par les services                      //
//-------------------------------------------------------- //
function _SwitchTrackerGroupFavorite() {
    AvailableSession(function (result) {
        if (result) {
            const maskGroup = $("input[id$='hidMaskGroup']");
            maskGroup.attr('value', (maskGroup.attr('value') == "all") ? "detail" : 'all');
            // Affichage des containers du groupe de tracker en fonction de la demande
            _DisplayTrackerGroupFavorite();
            _ActivateTrackerGroup();
            SaveSQLCookie("MaskGroup", maskGroup.attr('value'));
        }
    });
}

//-------------------------------------------------------- //
// Affichage des containers du tracker en fonction de la   //
// demande groupes ou favori                               //
//-------------------------------------------------------- //
// EG 20240523 [XXXXX] Tracker : Correction Tooltip manquant sur bouton(s) et Gestion icon sur Favorites/Groups
function _DisplayTrackerGroupFavorite() {
    const maskGroup = $("input[id$=hidMaskGroup]");
    var ressource = maskGroup.attr('title').split(';');
    var isDisplayAll = (maskGroup.attr('value') == "all");

    $("a[id$=btnMaskGroup] > i").removeClass(isDisplayAll ? "fa-star" : "fa-layer-group").addClass(isDisplayAll ? "fa-layer-group" : "fa-star");
    $("a[id$=btnMaskGroup]").attr('title', isDisplayAll ? ressource[0] : ressource[1]);

    $("div[id^=group_]").each(function (index, value) {
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

//-------------------------------------------------------- //
// Activation du groupe du tracker demandé et              //
//-------------------------------------------------------- //
function _ActivateTrackerGroup() {
    var activeTab = ($("#tabs").tabs("option", "active") + 1).toString();
    $("#tabs > div:nth-of-type(" + activeTab + ") > div").each(function (index, elt) {
    //$("div[id^='accordion_']").each(function (index, elt) {
        var activeIndex = 0;
        // Recherche du groupe actif pour chaque onglet du tracker
        if (elt.id.endsWith("HELP"))
            activeIndex = Cookies.get("TrkHelp-ActiveIndex") * 2;
        else
            activeIndex = _GetAccordionActiveIndex() * 2;

        // Activation du groupe
        $(elt).accordion({ active: activeIndex });
    });
}

//-------------------------------------------------------- //
// Resize du container d'un groupe actif du tracker        //
// Concerne sa Hauteur                                     //
//-------------------------------------------------------- //
// EG 20221010 Gestion de l'onglet HELP
function _ResizeActiveTrackerGroup(elt) {
    var _element = _GetAccordionActiveElement(elt).next("div");
    if (null != _element) _element.height(HeightTracker(elt));
    _element = null;
}

//-------------------------------------------------------- //
// Resize d'un container du menu actif                     //
// Concerne sa Hauteur                                     //
// => entraine un Resize du tracker (sauf en mode Portail) //
//-------------------------------------------------------- //
function MenuResize(elt) {
    if ("divMnu" == $(elt).parent().parent().attr("id")) {
        $(elt + " + div").height(HeightMenu());
    }
    if (false == $("#contnr").hasClass("portal"))
        TrackerResize();
}

//-------------------------------------------------------- //
// Resize du container du groupe du tracker actif          //
// Concerne sa Hauteur                                     //
//-------------------------------------------------------- //
// EG 20221010 Gestion de l'onglet HELP
function TrackerResize() {
    var elt = $("div[id^='tabs_'][aria-hidden=false] > div[id^=accordion_]").attr("id");
    var _element = _GetAccordionActiveElement("#" + elt).next("div");
    _element.height(HeightTracker(elt));
}

//-------------------------------------------------------- //
// Calcul de la hauteur du menu actif                      //
//-------------------------------------------------------- //
function HeightMenu() {
    const contnr = $("#contnr");
    const mnu = $("div[id=divMnu]")
    var trackerTop = $(window).height() - (contnr.hasClass("closetracker") || contnr.hasClass("portal") ? 45 : 320);
    var summaryTop = mnu.offset().top;
    var offset = mnu.children().length * 20
    return trackerTop - summaryTop - offset;
}

//-------------------------------------------------------- //
// Calcul de la hauteur du groupe actif du tracker         //
//-------------------------------------------------------- //
// EG 20221010 Gestion de l'onglet HELP
// EG 20230123 [26235][WI543] Test valeur elt
function HeightTracker(elt) {
    if (elt != undefined) {
        var activeIndex = _GetAccordionActiveIndex();
        var offset = (activeIndex == 0 ? 20 : 156);
        if (-1 < elt.indexOf("HELP"))
            offset = 60;
        var trackerTop = $("#tabs > div[aria-hidden=false]").offset().top;
        var footerTop = $("#masterfooter").offset().top;
        return footerTop - trackerTop - offset;
    }
}

//-------------------------------------------------------- //
// Gestion du rafraichissement automatique du tracker      //
// Un contrôle de session available est opéré avant        //
// si la session est morte retour à Login (IdP / RDBMS)    //
//-------------------------------------------------------- //
function _SetTrackerAutoRefresh(isManual) {
    AvailableSession(function (result) {
        if (result) {

            TrackerRefreshParam(function (ret) {
                const btn = $("a[id$=btnAutoRefresh]");
                btn.removeAttr("disabled");

                // Histo du Tracker
                $("input[id$='hidHisto']").attr('value', ret.Item3);

                if ((ret.Item2 == 0) || (ret.Item1 == false))
                    btn.attr("disabled", "disabled");
                // Activation
                if (btn.hasClass("stop") && (ret.Item2 > 0) && ret.Item1) {
                    trackerinterval = setInterval(_Refresh, ret.Item2 * 1000);
                    btn.removeClass("stop").addClass("start");
                    GetResource("StopRefresh", function (result) {
                        btn.attr("title", result);
                    });
                }
                // Désactivation
                else if (btn.hasClass("start") && ((ret.Item2 == 0) || (false == ret.Item1))) {
                    clearInterval(trackerinterval);
                    btn.removeClass("start").addClass("stop");
                    GetResource("StartRefresh", function (result) {
                        btn.attr("title", result);
                    });
                }
                // Désactivation
                else if (isManual && btn.hasClass("start") && (ret.Item2 > 0) && ret.Item1) {
                    clearInterval(trackerinterval);
                    btn.removeClass("start").addClass("stop");
                    GetResource("StartRefresh", function (result) {
                        btn.attr("title", result);
                    });
                }
            });
        }
    });
}

//-------------------------------------------------------- //
// Gestion des notifications de fin de traitement de       //
// messages par les services                               //
//-------------------------------------------------------- //
function GetMessageCompleted(callback) {
    $.ajax({
        url: "WebServices/TrackerWebService.asmx/GetResponseRecipient",
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

//-------------------------------------------------------- //
// Gestion des notifications de fin de traitement de       //
// messages par les services                               //
//-------------------------------------------------------- //
// EG 20221121 New
function GetMessageAfterOnConnectOk(callback) {
    $.ajax({
        url: "WebServices/CommonWebService.asmx/GetMessageAfterOnConnectOk",
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

//-------------------------------------------------------------//
// Traitement en fonction de l'action retournée à la connexion //
//-------------------------------------------------------------//
// EG 20230123 [26235][WI543] New
// EG 20230130 [26235][WI543] Mise en commentaire _SlideContnr
function _DisplayActionOnConnect() {

    GetActionOnConnect(function (currentaction) {
        if (null != currentaction) {
            switch (currentaction) {
                case "CHANGEPWD":
                    // Appel à ChangePwd.aspx (la page remplace default.aspx)
                    window.location.href = "ChangePwd.aspx?PWDAUTOMODE=1";
                    break;
                case "EXPIREDPWD":
                    // Appel à ChangePwd.aspx (la page remplace default.aspx)
                    window.location.href = "ChangePwd.aspx?BEFOREEXP=1";
                    break;
                default:
                    // Dans les autres cas la connexion est Ok 
                    // Lecture d'un message éventuellement généré
                    _DisplayMessageAfterOnConnectOk();
                    // Design et affichage du menu
                    _DisplayMenuToggle();
                    // Réactivation (visible) des sections Summary et Tracker
                    //_SlideContnr($("#btnSlidesummary"));
                    //_SlideContnr($("#btnSlidetracker"));
                    break;
            }
        }
    });
}

//----------------------------------------------//
// Lecture de l'action retournée à la connexion //
//--------------------------------------------- //
// EG 20230123 [26235][WI543] New
function GetActionOnConnect(callback) {
    $.ajax({
        url: "WebServices/CommonWebService.asmx/GetActionOnConnect",
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

//--------------------------------------------------------- //
// Récupération des paramètres de Refresh du tracker        //
// timer + refresh actif                                    //
//--------------------------------------------------------- //
function TrackerRefreshParam(callback) {
    $.ajax({
        url: "WebServices/TrackerWebService.asmx/TrackerRefreshParam",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (typeof callback === "function") {
                callback(data.d);
            }
        },
    })
}

//----------------------------------------//
// Récupération de la date Histo en cours //
//-------------------------------------- //
// EG 20221030 New
function GetDateTrackerDetail(callback) {
    $.ajax({
        url: "WebServices/TrackerWebService.asmx/GetDateTrackerDetail",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset = utf-8",
        success: function (data) {
            if (typeof callback === "function") {
                callback(data.d);
            }
        },
    })
}
