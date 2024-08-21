<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AutoCompleteDemo2.aspx.cs" Inherits="EFS.Spheres.Trial.Jquery.AutoCompleteDemo2" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>

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
    
    <script type="text/javascript">
        //AutoComplete en txt control
        function LoadAutoCompleteData() {
            $(".autocomplete").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "AutoCompleteDemo2.aspx/LodaData",
                        data: "{'identifier': '" + request.term + "'}",
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset = utf-8",
                        dataFilter: function (data) { return data; },
                        success: function (data) {
                            response($.map(data.d, function (item) {
                                return {
                                    value: item.value,
                                    label: item.label,
                                    id: item.id,
                                }
                            }))
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            alert(textStatus);
                        }
                    });
                },
                focus: function (event, ui) {
                    var controlTxtId = $(this)[0].id; // Control with autocomplete 
                    var controlHDN = controlTxtId.replace('TXT', 'HDN');
                    $("#" + controlHDN).val('');
                },
                select: function (event, ui) {
                    var controlTxtId = $(this)[0].id; // Control with autocomplete 
                    var controlHDN = controlTxtId.replace('TXT', 'HDN');
                    $("#" + controlHDN).val(ui.item.id);
                    return false;
                },
                minLength: 1
            })
            .autocomplete("instance")._renderItem = function (ul, item) {
                return $("<li>")
                    .append("<div>" + item.value + "<br>" + item.label + "</div>")
                    .appendTo(ul);
            };
        };
    </script>

</head>

<body>
    <form id="form1" runat="server">
        <div style="margin:20px">
            <div>
                <h1>AUTOCOMPLETE DEMO</h1>
                <h2>l'autocomplete jQuery  retour une collection de Value, Label, Id</h2>
                <asp:Label runat="server" CssClass ="label" Text="Saisir un language (java,c++ etc..)"></asp:Label>
                <efsc:WCTextBox2 id="TXT_autocomplete" runat="server" CssClass="autocomplete txtCapture" ></efsc:WCTextBox2>
            </div>
            <div>
                <hr />
                <h2>Id est déversé dans le contrôle c-idessous</h2>
                <h3>A terme celui ci devrait être un asp:hidden enfant de WCTextBox2 (evolution de WCTextBox2 nécessaire)</h3>
                <asp:TextBox  id="HDN_autocomplete" runat="server"></asp:TextBox>
            </div>
            <div>
                <hr />
                <asp:Button ID="btnPost"  Text="Post" runat="server"/>
            </div>
        </div>
        
        <script  type="text/javascript">
        <!--
        if (typeof ($) != 'undefined') $().ready(function () {
            LoadAutoCompleteData();
        });
        // -->
        </script>
    </form>
</body>

</html>
