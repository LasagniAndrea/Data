var model = 1;
var isContainerFluid = $('#trkContainer').hasClass("container-fluid");
display = $('#mc_rb_readyState').prop("checked") ? "readystate" : $('#mc_rb_groupTracker').prop("checked") ? "group" : "status";

SetModel1();

$(function () {
    var active = true;
    isContainerFluid = $('#trkContainer').hasClass("container-fluid");
    display = $('#mc_rb_readyState').prop("checked") ? "readystate" : $('#mc_rb_groupTracker').prop("checked") ? "group" : "status";

    $('#collapse-init').click(function () {

        if (active) {
            active = false;
            $('.panel-collapse').collapse('show');
            $('.panel-title').attr('data-toggle', '');
            $('#collapse-init span:first').switchClass("glyphicon-collapse-down", "glyphicon-collapse-up");
        } else {
            active = true;
            $('.panel-collapse').collapse('hide');
            $('.panel-title').attr('data-toggle', 'collapse');
            $('#collapse-init span:first').switchClass("glyphicon-collapse-up", "glyphicon-collapse-down");
        }
    });

    $("[id$='accordion']").on('show.bs.collapse', function (e) {
        if (active) $("[id$='accordion'] .in").collapse('hide');
    });

    $('#container-init').click(function () {
        if ($('#trkContainer').hasClass("container")) {
            $('#trkContainer').switchClass("container", "container-fluid");
            $('#container-init span:first').switchClass("glyphicon-fullscreen", "glyphicon-resize-small");
            SetModel();
        }
        else if ($('#trkContainer').hasClass("container-fluid")) {
            $('#trkContainer').switchClass("container-fluid", "container");
            $('#container-init span:first').switchClass("glyphicon-resize-small", "glyphicon-fullscreen");
            SetModel();
        }
    });

    $('#grid-model1-init').click(function () {
        SetModel1();
    });

    $('#grid-model2-init').click(function () {
        SetModel2();
    });

    $('#grid-model3-init').click(function () {
        SetModel3();
    });

    $('a[data-toggle="collapse"]').on('click', function () {

        var objectID = $(this).attr('href');

        if ($(objectID).hasClass('in')) {
            $(objectID).collapse('hide');
        }

        else {
            $(objectID).collapse('show');
        }
    });

    $(function () {
        $('.trkgrid .collapse').on('show.bs.collapse', function (event) {
            LoadTrackerContent(display, event)
        })
    });

});


// Chargement du détail du tracker en fonction de GROUP/READYSTATE/STATUS stockée dans l'ID du titre de l'accordéon et l'attribut STATUS
function LoadTrackerContent(display, event) {

    var currentTarget = event.currentTarget;
    if (null != currentTarget) {
        var criteria = $(currentTarget).attr("id").split("_");
        var group;
        var readystate;
        var status;
        if (display == 'readystate'){
            readystate = criteria[2];
            group = criteria[3];
        }
        else if (display == 'group') {
            group = criteria[2];
            readystate = criteria[3];
        }
        else if (display == 'status') {
            status = criteria[2];
            group = criteria[3];
        }
        $(currentTarget).load('TrackerContent.aspx?mode=GRPLOAD&group=' + group + "&readystate=" + readystate + "&status=" + status, null, null);
        criteria = null;
        group = null;
        readyState = null;
        status = null;
    }
}

// Ouverture de la page liée (TRADE, TRADEADMIN...)
function TrackerLink(elt) {
    // L'éventuel appel  de la page est construit dans TrackerContent.aspx
    // en fonction de l'attribut [args] qui contient (IDDAIDENT;IDDATA)
    var args = $(elt).attr("args")
    // EG 20170125 [Refactoring URL]
    if (null != args)
        window.open('Hyperlink.aspx?&args=' + args, '_blank', 'center=true, resizable=yes,scrollbars=yes,status=yes,location=yes,menubar=yes,toolbar=yes')
    args = null;
    return false;
}


function OnTrackerRefresh() {
    $().empty().load('Tracker.aspx');
}

function SetModel() {
    if (model == 1) SetModel1();
    else if (model == 2) SetModel2();
    else if (model == 3) SetModel3();
}


function SetModel1() {
    model = 1;
    $('#lblModel').text("Model " + model);
    $('#trkBody div.form-group ul.trkgrid div.trkgrid').removeClass('col-sm-1 col-sm-2 col-sm-3 col-sm-4 col-sm-5 col-sm-6 col-sm-7 col-sm-8 col-sm-9 col-sm-10 col-sm-11 col-sm-12');
    $('#trkBody div.form-group ul.trkgrid').removeClass('col-sm-1 col-sm-2 col-sm-3 col-sm-4 col-sm-5 col-sm-6 col-sm-7 col-sm-8 col-sm-9 col-sm-10 col-sm-11 col-sm-12');

    if (display == "readystate") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-12');
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-4');
    }
    else if (display == "group") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-12');
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-3');
    }
    else if (display == "status") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-12');
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-3');
    }
}

function SetModel2() {
    model = 2;
    $('#lblModel').text("Model " + model);
    $('#trkBody div.form-group ul.trkgrid div.trkgrid').removeClass('col-sm-1 col-sm-2 col-sm-3 col-sm-4 col-sm-5 col-sm-6 col-sm-7 col-sm-8 col-sm-9 col-sm-10 col-sm-11 col-sm-12');
    $('#trkBody div.form-group ul.trkgrid').removeClass('col-sm-1 col-sm-2 col-sm-3 col-sm-4 col-sm-5 col-sm-6 col-sm-7 col-sm-8 col-sm-9 col-sm-10 col-sm-11 col-sm-12');

    if (display == "readystate") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-6')
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-4')
    }
    else if (display == "group") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-4')
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-6')
    }
    else if (display == "status") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-4')
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-6')
    }
}

function SetModel3() {
    model = 3;
    $('#lblModel').text("Model " + model);
    $('#trkBody div.form-group ul.trkgrid div.trkgrid').removeClass('col-sm-1 col-sm-2 col-sm-3 col-sm-4 col-sm-5 col-sm-6 col-sm-7 col-sm-8 col-sm-9 col-sm-10 col-sm-11 col-sm-12');
    $('#trkBody div.form-group ul.trkgrid').removeClass('col-sm-1 col-sm-2 col-sm-3 col-sm-4 col-sm-5 col-sm-6 col-sm-7 col-sm-8 col-sm-9 col-sm-10 col-sm-11 col-sm-12');

    if (display == "readystate") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-3')
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-12')
    }
    else if (display == "group") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-4')
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-4')
    }
    else if (display == "status") {
        $('#trkBody div.form-group ul.trkgrid div.trkgrid').addClass('col-sm-4')
        $('#trkBody div.form-group ul.trkgrid').addClass('col-sm-4')
    }
}


/* Changement des checks d'une List en fonction d'un Check principal 
tous les checks de la liste prennent la valeur du check principal */
function AllTrackerCheckChange(elem) {
    var carrouselElem = document.getElementById('uitrk_car');
    $(carrouselElem).find("div.item").each(function () {
        $(this).find("div[id$='_container']").each(function () {
            $(this).find("input[type='checkbox']").each(function () {
                $(this).prop("checked", $(elem).prop("checked"));
            });
        })
    })
}


/* Changement des checks d'une List en fonction d'un Check principal 
tous les checks de la liste prennent la valeur du check principal */
function AllCheckChange(elem, containerId) {
    var containerElem = document.getElementById(containerId);
    $(containerElem.parentElement).children("div[id$='_container']").each(function () {
        $(this).find("input[type='checkbox']").each(function () {
            $(this).prop("checked", $(elem).prop("checked"));
        });
    })
}

function MainCheckChange(elem, containerId) {
    $("#" + containerId).children("div.checkList").find("input[type='checkbox']").each(function () {
        $(this).prop("checked", $(elem).prop("checked"));
    });
}
function ChildCheckChange(elem, containerId, parentCheckId) {

    $("#" + containerId).find("div.panel-body").each(function () {
        $(this).find("input[type='checkbox']").each(function () {
            $(this).prop("checked", $(elem).prop("checked"));
        });
    });
    var containerElem = document.getElementById(containerId);
    if (0 == $(containerElem.parentElement).find("input[type='checkbox']:checked").length) {
        $("#" + parentCheckId).prop("checked", false);
    }
    else {
        $("#" + parentCheckId).prop("checked", true);
    }
}
function SubChildCheckChange(elem, parentCheckId, containerId, grandParentCheckId) {
    if ($(elem).prop("checked")) {
        $("#" + parentCheckId).prop("checked", $(elem).prop("checked"));
        $("#" + grandParentCheckId).prop("checked", $(elem).prop("checked"));
    }
    else if (0 == $(elem.parentElement.parentElement).find("input[type='checkbox']:checked").length) {
        $("#" + parentCheckId).prop("checked", false);
        if (0 == $("#" + containerId).children("div.checkList").find("input[type='checkbox']:checked").length) {
            $("#" + grandParentCheckId).prop("checked", false);
        }
    }
}

$(document).ready(function () {

    $('.carousel').on('slide.bs.carousel', function () {
        $holder = $("ol.carousel-indicators li.active");
        $holder.removeClass("active");
        if ($holder.is(':last-child')) {
            $("ol.carousel-indicators li:first").addClass("active");
        }
        else {
            $holder.next("li").addClass("active");
        }
    });

    $("input[type='radio']").on("change", function () {
        var $input = $(this);
        if ($input.is(":checked"))
            $input.parent().first().addClass("active");
        else
            $input.parent().first().removeClass("active");
    });


    /* TRACKER */
    $(".sph-radio").each(function () {
        $(this).find("input[type='radio']").each(function () {
            if ($(this).prop("checked"))
                $(this).parent().first().addClass("active");
            else
                $(this).parent().first().removeClass("active");
        });
    });

});

