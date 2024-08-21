<%@ Page Title="" Language="C#" MasterPageFile="~/Portal/Portal.Master" AutoEventWireup="true" CodeBehind="Career.aspx.cs" Inherits="EFS.Spheres.Career" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mc" runat="server">
    <div class="container-fluid header">
        career
    </div>
    <div class="container portal">
        <div class="row">
            <div class="col-sm-12">
                <h2 class="title">Nos métiers</h2>
            </div>
            <div class="col-sm-12">
                <p>Les ressources humaines sont à la base de notre réussite : c’est pourquoi nous y prêtons la plus grande attention.</p>
                <p>Notre environnement de travail, notre secteur d’activité, la technologie déployée et la vocation internationale de nos produits sont autant d’arguments qui permettent aux personnes qui nous rejoignent d’exprimer leur potentiel tout en se forgeant une solide expérience.</p>
                <p>Chaque nouveau recrutement est abordé comme un investissement à long terme: motivation, rigueur et compétence sont les traits communs de nos collaborateurs.</p>
                <p>Pour l'ensemble des postes une culture financière et une connaissance de l'anglais restent souhaitées.</p>
                <hr />
            </div>
        </div>
        <div class="row">
            <div id="carousel-career" class="carousel slide" data-ride="carousel">
                <!-- Indicators -->
                <ol class="carousel-indicators">
                    <li data-target="#carousel-career" data-slide-to="0" class="active"></li>
                    <li data-target="#carousel-career" data-slide-to="1"></li>
                    <li data-target="#carousel-career" data-slide-to="2"></li>
                </ol>

                <!-- Wrapper for slides -->
                <div class="carousel-inner" role="listbox">
                    <div class="item active" style="background-image: url('../Images/rd.jpg');">
                        <div class="carousel-caption">
                            <div class="row cadre rd">
                                <h1>Recherche et développement</h1>
                                <div class="col-sm-12">
                                    <p>
                                        Notre équipe de développement doit régulièrement adapter nos solutions en réponse aux évolutions métiers, 
                                        règlementaires et techniques.
                                    </p>
                                </div>
                                <div class="col-sm-6">
                                    <div class="col-sm-12">
                                        <p class="title">MISSIONS</p>
                                        <p>Analyse, modélisation conceptuelle et physique des processus.</p>
                                        <p>Développement du framework et des progiciels.</p>
                                        <p>Veille technologique.</p>
                                    </div>
                                </div>
                                <div class="col-sm-6">
                                    <div class="col-sm-12">
                                        <p class="title">DOMAINES DE COMPETENCES</p>
                                        <p>C#, Asp.Net, ADO.Net, Framework.Net, Mono, Java, C++</p>
                                        <p>HTML, Ajax, Javascript, JQuery, XML, XSL, XSL-FO, XSD, CSS, Less, Bootstrap</p>
                                        <p>Oracle®, MS SqlServer®</p>
                                        <p>MSMQ®, MQSeries®</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="item" style="background-image: url('../Images/consulting.jpg');">
                        <div class="carousel-caption">
                            <div class="row cadre consulting">
                                <h1>Consulting</h1>
                                <div class="col-sm-12">
                                    <p>
                                        En travaillant en étroite collaboration avec l'équipe de développement, nos consultants contribuent à l'innovation de produit,
                                        à la livraison et au support de logiciels, en fournissant des conseils techniques et fonctionnels aux clients, 
                                        aux intégrateurs et aux autres équipes internes.
                                    </p>
                                </div>
                                <div class="col-sm-6">
                                    <div class="col-sm-12">
                                        <p class="title">GESTION DE PROJET</p>
                                        <p>Maîtrise d'ouvrage et maîtrise d'oeuvre.</p>
                                        <p>Rédaction de cahier des charges.</p>
                                        <p>Participation à l’analyse des nouvelles fonctionnalités.</p>
                                    </div>
                                </div>
                                <div class="col-sm-6">
                                    <div class="col-sm-12">
                                        <p class="title">ACCOMPAGNEMENT et FORMATION</p>
                                        <p>Mise à jour des documentations produits: utilisation, fonctionnalités, ...</p>
                                        <p>Formation des utilisateurs: métiers, produits, ...</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="item" style="background-image: url('../Images/helpdesk.jpg');">
                        <div class="carousel-caption">
                            <div class="row cadre helpdesk">
                                <h1>Help-desk</h1>
                                <div class="col-sm-12">
                                    <p>
                                        Prise en charge des demandes d’assistance des utilisateurs de nos solutions. 
                                    </p>
                                </div>
                                <div class="col-sm-6">
                                    <div class="col-sm-12">
                                        <p class="title">ASSISTANCE</p>
                                        <p>Conseil et assistance dans l’utilisation de nos produits.</p>
                                        <p>Résolution et suivi des anomalies.</p>
                                        <p>Mise en place de solutions de contournement.</p>
                                        <p>Constitution de dossiers pour transmission au service R&D.</p>
                                    </div>
                                </div>
                                <div class="col-sm-6">
                                    <div class="col-sm-12">
                                        <p class="title">ADMINISTRATION & RECETTE</p>
                                        <p>Administration et mise à jour des référentiels métiers.</p>
                                        <p>Participation aux phases de recette des correctifs et releases.</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Controls -->
                <a class="left carousel-control" href="#carousel-career" role="button" data-slide="prev">
                    <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>
                    <span class="sr-only">Previous</span>
                </a>
                <a class="right carousel-control" href="#carousel-career" role="button" data-slide="next">
                    <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>
                    <span class="sr-only">Next</span>
                </a>
            </div>
        </div>
    </div>
</asp:Content>
