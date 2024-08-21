(function ($) {

    $.fn.freezegrid = function () {
        // Le référentiel a-t-il un attribut Freeze ?
        var nbFrz = parseInt(this.attr("frz-col")) || 0;
        if (0 < nbFrz) {
            // Offset des colonnes à conserver/supprimer
            var nbFrz2 = nbFrz + 1;
            var nbFrz3 = nbFrz + 2;
            var nbFrz4 = nbFrz2;
            var nbColSpan = 0;

            // Clone du Datagrid d'origine
            var tblScrl = $("#efsdtgRefeferentiel").clone(true); // Grid avec scrolling
            var tblFrz = $("#efsdtgRefeferentiel").clone(true); // Grid avec colonness fixes
            var tblHpg = $("#efsdtgRefeferentiel").clone(true); // Pager Header
            var tblFpg = $("#efsdtgRefeferentiel").clone(true); // Page Footer

            // Définition des nouveaux Ids (l'id du datagrid d'origine est conservé sur le datagrid Freeze)
            $(tblScrl).attr("id", "efsdtg-scrl");
            $(tblHpg).attr("id", "efsdtg-hpg");
            $(tblFpg).attr("id", "efsdtg-fpg");

            tblFrz.find("thead tr.DataGrid_HeaderStyle td[colspan]").each(function () {
                if ($(this).index() < nbFrz)
                    nbColSpan += 1;
            });

            nbFrz = nbFrz - nbColSpan;
            nbFrz2 = nbFrz + 1;

            // Suppression des lignes inutiles (Page header)
            tblHpg.find("thead tr.DataGrid_HeaderStyle").remove();
            tblHpg.find("tbody").remove();
            tblHpg.find("tfoot").remove();

            // Suppression des colonnes inutiles coté DataGrid avec colonnes fixes
            tblFrz.find("thead tr.DataGrid_PagerStyle").remove();
            tblFrz.find("thead tr.DataGrid_HeaderStyle > td:gt(" + nbFrz + ")").remove();
            tblFrz.find("tfoot").remove();
            tblFrz.find("tbody > tr > td:nth-child(n+" + nbFrz3 + ")").remove();

            // Suppression des colonnes inutiles coté DataGrid scroll
            tblScrl.find("thead tr.DataGrid_PagerStyle").remove();
            tblScrl.find("thead tr.DataGrid_HeaderStyle > td:nth-child(-n+" + nbFrz2 + ")").remove();
            tblScrl.find("tfoot").remove();
            tblScrl.find("tbody > tr > td:nth-child(-n+" + nbFrz4 + ")").remove();

            // Suppression des lignes inutiles (Page footer)
            tblFpg.find("thead").remove();
            tblFpg.find("tbody").remove();

            // Préparation du container pour stockage du résultat
            var frzgrid = $('<table></table>').addClass("freezegrid").attr({ "id": "frzContnr", "border": "0", "cellpadding": "0", "cellspacing": "0" });

            var hpgcontnr = $('<td></td>').attr("colspan", "2").append($('<div></div>').attr("id", "Hpg")); // réceptionne Grid Pager Header
            var frzcontnr = $('<td></td>').attr("valign", "top").append($('<div></div>').attr("id", "Frz")); // réceptionne Grid colonnes fixes
            var scrlcontnr = $('<td></td>').attr("valign", "top").append($('<div></div>').attr("id", "Scrl").css("overflow", "auto"));  // réceptionne Grid scroll (d'où la présence overflow)
            var fpgcontnr = $('<td></td>').attr("colspan", "2").append($('<div></div>').attr("id", "Fpg")); // réceptionne Page Footer

            // table Freeze container principal
            $(frzgrid).append($('<tr></tr>').append(hpgcontnr), $('<tr></tr>').append(frzcontnr, scrlcontnr), $('<tr></tr>').append(fpgcontnr));
            // Alilentation du container
            $("#Hpg", frzgrid).html($(tblHpg));
            $("#Frz", frzgrid).html($(tblFrz));
            $("#Scrl", frzgrid).html($(tblScrl));
            $("#Fpg", frzgrid).html($(tblFpg));
            // Suppression grid d'origine et remplacement par le container
            $(this).html('').append(frzgrid);

            // Alignement hauteur Header
            var frzHeight = Math.ceil(tblFrz.find("thead tr.DataGrid_HeaderStyle td:first-child").height());
            var scrlHeight = Math.ceil(tblScrl.find("thead tr.DataGrid_HeaderStyle td:first-child").height());
            tblFrz.find("thead tr.DataGrid_HeaderStyle td:first-child").height(Math.max(frzHeight, scrlHeight));
            tblScrl.find("thead tr.DataGrid_HeaderStyle td:first-child").height(Math.max(frzHeight, scrlHeight));

            // Alignement hauteur Body
            frzHeight = Math.ceil(tblFrz.find("tbody tr td:first-child").height());
            scrlHeight = Math.ceil(tblScrl.find("tbody tr td:first-child").height());
            tblFrz.find("tbody tr td:first-child").height(Math.max(frzHeight, scrlHeight));
            tblScrl.find("tbody tr td:first-child").height(Math.max(frzHeight, scrlHeight));

            $("tbody tr", tblFrz).on("mouseleave", function (event) {
                $("div[qtip]").triggerHandler("unfocus");
                $("div[qtip]").css("display", "none");
                $("div[qtip]").removeClass("qtip-active");
            });

            // Evénement MouseOver/MouseOut pour synchronisation des déplacements de la souris
            $("tbody tr", tblFrz).on("mouseover", function (event) {
                $("div[qtip]").triggerHandler("mouseover");
                $("div[qtip]").css("display", "none");
                $("div[qtip]").removeClass("qtip-active");
                var index = $(this).index() + 1;
                $("#efsdtg-scrl tr:eq(" + index + ")").addClass("DataGrid_SelectedItemStyle");
            }).on("mouseout", function (event) {
                $("div[qtip]").css("display", "none");
                $("div[qtip]").removeClass("qtip-active");
                var index = $(this).index() + 1;
                $("#efsdtg-scrl tr:eq(" + index + ")").removeClass("DataGrid_SelectedItemStyle");
            });

            $("tbody tr", tblScrl).on("mouseover", function () {
                $("div[qtip]").css("display", "none");
                $("div[qtip]").removeClass("qtip-active");
                var index = $(this).index() + 1;
                $("#efsdtgRefeferentiel tr:eq(" + index + ")").addClass("DataGrid_SelectedItemStyle");
            }).on("mouseout", function () {
                $("div[qtip]").css("display", "none");
                $("div[qtip]").removeClass("qtip-active");
                var index = $(this).index() + 1;
                $("#efsdtgRefeferentiel tr:eq(" + index + ")").removeClass("DataGrid_SelectedItemStyle");
            });

            // Resize 
            $(window).on("resize", function () {
                // Offset de 17px si existence d'une scrollbar verticale
                var offset = 0;

                if ($('html').hasScrollBar())
                    offset = 17;
                var vw = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
                var freezeWidth = $("#Frz").width();
                $("#Scrl").width(vw - freezeWidth - offset);
            });

            // Forcer le Resize à l'initialisation
            $(window).triggerHandler("resize");
        }
        return this;
    }

})(jQuery);

// Il existe une scrollbar verticale 
(function ($) {
    $.fn.hasScrollBar = function () {
        var y = this.get(0).scrollHeight;
        var z = this.get(0).clientHeight;
        //alert(y);
        //alert(z);
        //alert(y >= z);
        return y > z;
    }
})(jQuery);