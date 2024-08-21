function OnSubmit(callEvent, callArgument) {
    SaveActiveElement();
    Page_ClientValidate();
    if (Page_IsValid)
        javascript: __doPostBack(callEvent, callArgument);
}

function OpenerCallRefreshAndClose()
{
    window.opener.RefreshPage();
    self.close();
}

function OpenerCallReload()
{				
    if (null != window.opener)
        window.opener.RefreshPage();
}


// Initialisation des tooltips
var tooltipsforlabel = null;
var popoverforcontrol = null;

// Affichage ou non des colonnes SQL d'un référentiel sur le click  de la checkBox
function ShowColumnName(elem) {
    var div = document.getElementById('mc_pnlGridSystem');
    $(div).find("span[data-name]").each(function () {
        if ($(elem).is(":checked"))
            $(this).addClass('dsp-column');
        else
            $(this).removeClass('dsp-column');
    });
}


// Lecture de toutes les sessions (a[data-toggle="tab") qui sont actives 
// Stockage de l'identifiant (hRef) dans sessionStorage (Item = activeTab)
function SetActiveTabs()
{
    var activetabs = [];
    var i = 0;
    $('a[data-toggle="tab"]').each(function () {

        if ($(this).closest("li").hasClass("active")) {
            activetabs[i++] = $(this).attr('href');
        }
    });
    if (0 < activetabs.length)
        sessionStorage.setItem("activeTab", JSON.stringify(activetabs));
}


// Affichage ou non des popover d'un référentiel sur le click de la checkBox
function ShowPopover(elem) {

    if (null != popoverforcontrol)
        popoverforcontrol.popover("destroy");

    if ($(elem).is(":checked")) {

        // On récupère le template pour le popover s'il existe
        var currentTemplate = $("#popovertemplate").html();
        popoverforcontrol = $("[id=mc_pnlGridSystem] .input-xs[data-content]").popover({
            html: true,
            placement: "auto",
            trigger: "hover focus",
            //trigger: "manual", // used for debug
            container: "#tipcontainer", // Container des popovers construits par BS
            viewport: "#mc_pnlGridSystem", // viewport = container du grid system
            template: currentTemplate // template 
        });

        $("[id=mc_pnlGridSystem] .input-xs[data-content]").prevAll().filter(".control-label").css("font-weight", "bold");
        $("[id=mc_pnlGridSystem] .input-xs[data-content]").prevAll().filter(".control-label").addClass("help");
        $("[id=mc_pnlGridSystem] div.input-xs[data-content] label").css("font-weight", "bold");
        $("[id=mc_pnlGridSystem] div.input-xs[data-content]").addClass("help");
        $("[id=mc_pnlGridSystem] .input-xs.comment[data-content]").parent().filter(".comment").addClass("help");
    }
    else {
        $("[id=mc_pnlGridSystem] .input-xs[data-content]").prevAll().filter(".control-label").css("font-weight", "normal");
        $("[id=mc_pnlGridSystem] .input-xs[data-content]").prevAll().filter(".control-label").removeClass("help");
        $("[id=mc_pnlGridSystem] div.input-xs[data-content] label").css("font-weight", "normal");
        $("[id=mc_pnlGridSystem] div.input-xs[data-content]").removeClass("help");
        $("[id=mc_pnlGridSystem] .input-xs.comment[data-content]").parent().filter(".comment").removeClass("help");
    }
}


$().ready(function () {

    //Chargement des tooltips des labels (Ouverture par Hover (Trigger))
    tooltipsforlabel = $("[id=mc_pnlGridSystem] .control-label[title]").tooltip({
        title: this.title,
        html: true,
        placement: "auto left",
        trigger: "hover",
        container: "#tipcontainer" // Container des tooltips construits par BS
    });

    // Sur le resize de la fenêtre
    $(window).on('resize', function () {

        // S'il existe un popover actif ("div.popover.in") au moment du resize 
        // on le réaffiche ([data-content]:focus) pour recalculer ses coordonnées
        if ($("#tipcontainer").find("div.popover.in"))
            $('[data-content]:focus').popover("show");
    });


    $(function () {


        var repositories;

        $("[data-ta-kv]").each(function () {

            var keyvalues = $(this).attr("data-ta-kv");
            //var ctrlsource = $(this);
            var ctrlsourcevalue = document.getElementById($(this).attr('id').replace("TXT", "TMP"));


            $(this).typeahead({
                source: function (query, process) {
                    return $.ajax({
                        type: "POST",
                        url: "WebServices/WSDataService.asmx/GetData",
                        data: JSON.stringify({ pKeyValues: keyvalues, pFilter: query }),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success:
                        function (data) {
                            repositories = data.d;
                            var repositoryIds = $.map(repositories, function (item) {
                                return item.DisplayValue;
                            });
                            return process(repositoryIds);
                        }
                    });
                },
                items: 12,
                minLength: 1,
                autoSelect: true,
                fitToElement: false,
                matcher: function (item) {
                    return true;
                },
                sorter: function (items) {
                    return items;
                },

                showHintOnFocus : false,
                highlighter: function (identifier) {
                    var repository = $.grep(repositories, function (item) { return item.DisplayValue == identifier; })[0];
                    return repository.DisplayValue;

                },
                updater: function (identifier) {
                    var repository = $.grep(repositories, function (item) { return item.DisplayValue == identifier; })[0];
                    // Lecture du contrôle Hidden qui contient la valeur associé à l'autocomplete.
                    $(ctrlsourcevalue).val(repository.Id);
                    return repository.DisplayValue;
                }
            });
        });
    });


    // Pour garder la section active (tab) après un Reload ou post de page
    // Utilisation de sessionStorage (html5) pour stocker les dernières sections actives
    // Sur l'activation d'une section (tab) on appelle la fonction SetActiveTabs() qui se charge de stocker
    // l'ensemble des sections active dans la variable de session (sessionStorage avec Item = activeTab)
    var activetabs = [];
    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        SetActiveTabs();
    });

    activetabs = JSON.parse(sessionStorage.getItem("activeTab"));

    $.each(activetabs, function (index, value) {
        $('#mc_pnlGridSystem a[href="' + value + '"]').tab('show');
    });

    // Réinitialisation des validateurs non Valid (isValid = false) sur Focus d'un champ Texte
    // Permet de réactiver l'autocomplete
    //document.addEventListener("focus", RemoveHasErrorClass, true);

});
