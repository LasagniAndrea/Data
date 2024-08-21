<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebServiceClient.aspx.cs" Inherits="EFS.Spheres.Trial.Jquery.WebForm1" %>

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
    <style>
        h3 {
            margin: 15px;
        }

        .divMaster {
            display: flex;
            flex-direction : row;
            justify-content: flex-start;
            border: 2px solid #444;
        }


        .divContainer {
            display: flex;
            justify-content: flex-start;
        }

        .div1 {
            margin: 15px;
        }
        .div2 {
            margin: 15px;
        }
    </style>

    <script type="text/javascript">
        
        function Init() {

            // load resource on change
            $("#DDLResource").change(function () {
                GetResource($("#DDLResource").val(), function (res) {
                    $("#LBLResource").text(res);
                });
            });

            var queryResult;
            var isQueryLoaded = false;
            // load dataset first
            $("#BTNRefresh").click(function (e) {
                e.preventDefault();
                $("#DDLRow")[0].selectedIndex = 0;
                ExecuteSelect(" select top 5 a.IDENTIFIER, ac.IDROLEACTOR from dbo.ACTORROLE ac inner join dbo.ACTOR a on a.IDA = ac.IDA", function (result) {
                    queryResult = result;
                    isQueryLoaded = true;
                });
            });

            // Display ACTOR/ROLE on change
            $("#DDLRow").change(function () {
                let selectedIndex = $("#DDLRow")[0].selectedIndex;
                $("#LBLValue").text("");
                if (isQueryLoaded && selectedIndex > 0) {
                    $("#LBLValue").text("Actor: " + queryResult[selectedIndex].IDENTIFIER + " has role " + queryResult[selectedIndex].IDROLEACTOR);
                }
            });
        }
    </script>
</head>

<body>
    <form id="form1" runat="server">
        <div class ="divMaster">
        <h3>GetResource : Sample (Get ressource using web service) </h3>
            <div class="divContainer">
                <div class="div1">
                    <asp:DropDownList ID="DDLResource" runat="server" AutoPostBack ="false" />
                </div>
                <div class="div2">
                    <asp:Label ID="LBLResource" runat="server" Text=""></asp:Label>
                </div>
           </div>
       </div>
        <div class ="divMaster">
            <h3>ExecuteDataTable : Sample (Get Query result using web service)</h3> 
            <div class="divContainer">
                <div class="div1">
                    <asp:Button ID="BTNRefresh" runat="server" Text="Refresh" />
                </div>
               <div class="div1">
                    <asp:DropDownList ID="DDLRow" runat="server" AutoPostBack =" false" >
                        <asp:ListItem Text=""  Selected="True"></asp:ListItem>
                        <asp:ListItem Text="FirstRow" ></asp:ListItem>
                        <asp:ListItem Text="SecondRow"></asp:ListItem>
                        <asp:ListItem Text="ThirdRow"></asp:ListItem>
                        <asp:ListItem Text="FourthRow"></asp:ListItem>
                        <asp:ListItem Text="FifthRow"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="div2">
                    <asp:Label ID="LBLValue" runat="server" Text=""></asp:Label>
                </div>
           </div>
        </div>
        
        <script  type="text/javascript">
        <!--
        if (typeof ($) != 'undefined') $().ready(function () {
            Init();
        });
        // -->
        </script>
    </form>
</body>
</html>
