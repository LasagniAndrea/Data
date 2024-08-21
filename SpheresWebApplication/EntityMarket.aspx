<%@ Page Language="C#" AutoEventWireup="true" Inherits="EntityMarket" Codebehind="EntityMarket.aspx.cs" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title id="titlePage" runat="server" />
    <link id="linkCssTracker" href="Includes/EntityMarket.min.css" rel="stylesheet" type="text/css"/>
</head>

<body id="entityMarketbody">
    <form id="idEntityMarket" runat="server">
        <asp:ScriptManager ID="scriptManagerMain" runat="server"/>
        <asp:UpdatePanel ID="UpdEntityMarket" runat="server">
            <ContentTemplate>
                <asp:PlaceHolder runat="server" ID="plhEntityMarket" >
                    <asp:HiddenField ID="__ISSCROLL"  runat="server" Value="true"/>
                </asp:PlaceHolder>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="timerEntityMarket" EventName="Tick" />
            </Triggers>
        </asp:UpdatePanel>
        <asp:Timer ID="timerEntityMarket" runat="server"/>   
        <script type="text/javascript" src="./Javascript/JQuery/jquery.infiniteslidev2.min.js"></script>
        <script type="text/javascript">
            /* FI 2020-06-18 [XXXXX] add ScrollEntityMarket */
            function ScrollEntityMarket() {
                $(document).ready(function () {
                    if ($("#__ISSCROLL").val() == "true")
                        $("ul#entityMarket").infiniteslide({ speed: 80 });
                    else
                        $("ul#entityMarket").stop();
                });
            }
        </script>
        <script type="text/javascript"> 
            /* FI 2020-06-18 [XXXXX] add pageLoad */
            function pageLoad(sender, args) {
                if (args.get_isPartialLoad()) {
                    ScrollEntityMarket;
                }
            }
        </script>
    </form>
</body>
</html>
