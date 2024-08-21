<%@ Page Language="c#" Inherits="EFS.Spheres.WelcomePage" Codebehind="Welcome.aspx.cs" %>
<!DOCTYPE html>
<html id="HtmlCenter" xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Welcome</title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />

    <script type="text/javascript">
        function CallAlert() {
            alert("TEST");
        }
    </script>
    <style type="text/css">
    #welcomeContainer
    {
       margin: 10% auto;
       width:800px;
       height:200px;
       text-align: left;
    }
    
    div.default .headh
    {
        background-color:#036AB5;
        border:1pt solid #036AB5;
    }
    div.default .contenth {border:1pt solid lightgray;}

    #pnlWelcomeHeader
    {
        background-repeat:no-repeat;
        background-position:center;  
        height:80px;
    }
    #pnlWelcomeBody
    {
        text-align:center;
        background-repeat:no-repeat;
        background-image:none;
    }

    #pnlWelcomeFooter
    {
        text-align:right;
        background-repeat:no-repeat;
        background-image:none;
    }
    #pnlWelcome p {border-bottom:2pt solid #036AB5;}
    #pnlWelcome.blue p {border-bottom:2pt solid #036AB5;}
    #pnlWelcome.cyan p {border-bottom: 2pt solid #1CBCF0;}
    #pnlWelcome.green p {border-bottom: 2pt solid #51AD26;}
    #pnlWelcome.gray p {border-bottom: 2pt solid #545454;}
    #pnlWelcome.marron p {border-bottom: 2pt solid #673406;}
    #pnlWelcome.orange p {border-bottom:2pt solid #EA5A00;}
    #pnlWelcome.red p {border-bottom:2pt solid #C00303;}
    #pnlWelcome.rose p {border-bottom:2pt solid #EA71A8;}
    #pnlWelcome.violet p {border-bottom:2pt solid #8B569F;}
    #pnlWelcome.yellow p {border-bottom:2pt solid #D8C825;}
   
    #pnlWelcome.blue a[id=hlkFooter] {color:#036AB5;}
    #pnlWelcome.cyan a[id=hlkFooter] {color:#1CBCF0;}
    #pnlWelcome.green a[id=hlkFooter] {color:#51AD26;}
    #pnlWelcome.gray a[id=hlkFooter] {color:#545454;}
    #pnlWelcome.marron a[id=hlkFooter] {color:#673406;}
    #pnlWelcome.orange a[id=hlkFooter] {color:#EA5A00;}
    #pnlWelcome.red a[id=hlkFooter] {color:#C00303;}
    #pnlWelcome.rose a[id=hlkFooter] {color:#EA71A8;}
    #pnlWelcome.violet a[id=hlkFooter] {color:#8B569F;}
    #pnlWelcome.yellow a[id=hlkFooter] {color:#D8C825;}
    </style>

</head>
<body id="Body1" runat="server" style=" width: 100%; /*background-image: -webkit-gradient(linear,left top,right bottom,color-stop(0, #000000),color-stop(1, #2A64A3));*/ /*background-image: -webkit-gradient(linear,left top,right bottom,color-stop(0, #000000),color-stop(1, #FA5858));
background-image: -ms-linear-gradient(45deg, #000000, #FA5858);
background-image: -o-linear-gradient(-45deg, #000000, #FA5858);
background-image: -moz-linear-gradient(-45deg, #000000, #FA5858);
background-image: -webkit-linear-gradient(-45deg, #000000, #FA5858);
background-image: linear-gradient(to -45deg, #000000, #FA5858);*/">
    <form id="Welcome" method="post" runat="server">
    <div id="welcomeContainer">
        <asp:Panel runat="server" ID="pnlWelcome" CssClass="default">
            <asp:Panel ID="pnlWelcomeHeader" runat="server" CssClass="headh" />
            <div class="contenth">
                <div style="height: 100%">
                    <asp:Panel ID="pnlWelcomeBody" runat="server">
                        <asp:Image ID="imgWelcomeBody" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="pnlWelcomeFooter" runat="server">
                        <asp:Image ID="imgLogoPub" runat="server" ImageUrl="Images/Partners/DotNetFramework.png" />
                        <asp:Image ID="imgWelcomeFooter" runat="server" ImageUrl="Images/Partners/DotNetFramework.png" Visible="false" />
                        <p> </p>
                        <asp:HyperLink ID="hlkFooter" runat="server" CssClass="hyperlink" Target="_blank">Copyright EFS</asp:HyperLink>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
    </div>
    </form>

    <script type="text/javascript">

        var cntrlIsPressed = false;
        // [25556] Gestion de la touche Control sur lien hyperlink du Menu - Ouverture sur main ou _blank - solution de contournement pour le self.Close
        $(document).on("keydown" , function (event) {
            if (event.ctrlKey)
                cntrlIsPressed = true;
        });

        $(document).on("keyup" , function () {
            cntrlIsPressed = false;
        });

        var compteur;
        compteur = 0;

        function Go() {
            var texte = "Images/Partners/" + Affiche(compteur);
            Filtre(texte);
            compteur += 1;
            window.setTimeout("Imbrique()", 3500);
        }

        function Imbrique() {
            var texte = "Images/Partners/" + Affiche(compteur);
            Filtre(texte);
            compteur += 1;
            window.setTimeout("Go()", 3500);
        }

        function Filtre(texte) {
            var ctrl = document.getElementById["imgLogoPub"];
            if (null != ctrl) {
                ctrl.style.filter = "progid:DXImageTransform.Microsoft.Fade(overlap=1,duration=0.25)";
                ctrl.filters[0].apply();
                ctrl.src = texte;
                ctrl.filters[0].play();
            }
        }

        function Affiche(valeur) {
            var image = new Array("SqlServer.png", "Oracle.png", "IBMDB2.png", "Sybase.png", "DotNetFramework.png");
            var ctrl = document.forms[0].elements["__SOFTWARE"];
            if (ctrl != null) {
                if (ctrl.value == "OTCml")
                    image = new Array("ISDA.png", "FpML.png", "FixML.png", "SqlServer.png", "Oracle.png", "IBMDB2.png", "Sybase.png", "DotNetFramework.png");
            }

            if (valeur < image.length)
                return image[valeur];
            else {
                compteur = 0;
                return image[0];
            }
        }
    </script>
</body>
</html>
