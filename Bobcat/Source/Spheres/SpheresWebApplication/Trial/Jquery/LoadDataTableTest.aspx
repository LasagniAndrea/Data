<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoadDataTableTest.aspx.cs" Inherits="EFS.Spheres.Trial.Jquery.WebForm2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">


    <head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>

    <meta content="text/html; charset=UTF-8" http-equiv="Content-Type" name="metaText" />
    <meta content="text/javascript" http-equiv="Content-Script-Type" name="metaScript" />
    <meta content="text/css" http-equiv="Content-Style-Type" name="metaStyle" />
    
    <link id="linkCssAwesome" rel="stylesheet" type="text/css" href="../../Includes/fontawesome-all.min.css" />
    <link id="linkCssCommon" rel="stylesheet" type="text/css" href="../../Includes/EFSThemeCommon.min.css" />
    <link id="linkCssCustomCommon" rel="stylesheet" type="text/css" href="../../Includes/CustomThemeCommon.css" />
    <link id="linkCssUISprites" rel="stylesheet" type="text/css" href="../../Includes/EFSUISprites.min.css" />
    <link id="linkCss" rel="stylesheet" type="text/css" href="../../Includes/EFSTheme-vlight.min.css" />
    
    <link id="linkCssJQueryUI" rel="stylesheet" type="text/css" href="../../Javascript/JQuery/jquery-ui-1.13.3.min.css" />

    <link id="linkIcon" rel="shortcut icon" type="image/x-ico" href="../../Images/ico/Spheres-Marron.ico" />
    <link id="linkIcon2" rel="shortcut icon" type="image/x-png" href="../../Images/png/Spheres-Marron.png" />
    <style type="text/css">THEAD { DISPLAY: table-header-group }  TFOOT { DISPLAY: table-footer-group }</style>
    
    <script type="text/javascript" src="../../Javascript/JQuery/jquery-3.7.1.min.js"></script>
    <script type="text/javascript" src="../../Javascript/JQuery/jquery-ui-1.13.3.min.js"></script>

    <script type="text/javascript" src="../../Javascript/pagebase.min.js"></script>
    
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    

    <script type="text/javascript">

        function Init() {
            //Appel correct 1
            LoadDataTable(['IDM', 'SHORT_ACRONYM'], 'VW_MARKET_IDENTIFIER', [{ col: 'IDM', value: 10 }], function (m) {
                console.log('succes');
                //Appel correct 2
                LoadDataTable(['IDM', 'SHORT_ACRONYM'], 'VW_MARKET_IDENTIFIER', [{ col: 'IDM', value: 10, operator: '=' }], function (m) {
                    console.log('succes');

                    /*********************************************
                    Avec les appels suivants Spheres doit planter  
                    voir http://svr-db01:8080/tfs/DefaultCollection/SpheresProject/_workitems/edit/562
                    *************************************************/

                    //Error expected  => Aucune restriction 
                    // ici l'appel à LoadDataTable plante "invalid json primitive" puisqu'il manque le paramètre restrictArray invalid json primitive
                    LoadDataTable(['IDM', 'SHORT_ACRONYM'], 'VW_MARKET_IDENTIFIER'/*, [{ col: 'IDM', value: 10 }]*/, function (m) {

                    });

                    //Error expected  => la table ACTOR n'est pas acceptée à ce jour 
                    LoadDataTable(['IDM', 'SHORT_ACRONYM'], 'ACTOR', [{ col: 'IDA', value: 10 }], function (m) {

                    });


                    //Error expected  => Colonne (col au format incorrect dans restriction)
                    LoadDataTable(['IDM', 'SHORTIDENTIFIER'], 'VW_MARKET_IDENTIFIER', [{ col: 'IDM is not null--', value: 10 }], function (m) {
                        
                    });

                    //Error expected  => Colonne (col au format incorrect dans le select )
                    LoadDataTable(['IDM TOT', 'SHORT_ACRONYM'], 'VW_MARKET_IDENTIFIER', [{ col: 'IDM', value: 10 }], function (m) {

                    });

                    //Error expected  => colonne NULL interdit (afin d'éviter le where null is null)
                    LoadDataTable(['IDM', 'SHORT_ACRONYM'], 'VW_MARKET_IDENTIFIER', [{ col:'null',value: null }], function (m) {

                    });

                    //Error expected  4 => opérator interdit
                    LoadDataTable(['IDM', 'SHORT_ACRONYM'], 'VW_MARKET_IDENTIFIER', [{ col: 'IDM', value: 0, operator: '>' }], function (m) {

                    });

                    //Error expected  6 => jointure non autorisé
                    LoadDataTable(['IDM', 'BIC'], 'VW_MARKET_IDENTIFIER inner join dbo.ACTOR on 1=1', [{ col: 'IDM', value: 10 }], function (m) {

                    });
                });
            });

        }
    </script>
</head>


<body>
    <form id="form1" runat="server">
        <div>
        </div>
    </form>
    <script  type="text/javascript">
        <!--
    if (typeof ($) != 'undefined') $().ready(function () {
        Init();
    });
        // -->
    </script>
</body>
</html>


