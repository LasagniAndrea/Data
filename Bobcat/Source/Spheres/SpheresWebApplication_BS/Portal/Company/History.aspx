<%@ Page Title="" Language="C#" MasterPageFile="~/Portal/Portal.Master" AutoEventWireup="true" CodeBehind="History.aspx.cs" Inherits="EFS.Spheres.History" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mc" runat="server">
    <div class="container-fluid header">
        history
    </div>
    <div class="container portal">
        <div class="row">
            <div class="col-sm-12">
                <h2 class="title">Nos dates clés</h2>
            </div>
            <div class="col-sm-12">
                <p>
                    Bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla  bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                Bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla  bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla.
                </p>
                <hr />
            </div>
            <div class="col-sm-8">
                <ul class="date">
                    <li>
                        <span>2015 ...</span>
                        <ul>
                            <li>
                                <span>2017</span>
                                <span>Développement de la seconde génération de Spheres.</span>
                            </li>
                        </ul>
                    </li>
                    <li>
                        <span>2010 à 2014</span>
                        <ul>
                            <li>
                                <span>2013</span>
                                <p>Grâce à l'accord signé avec la société Xchanging Italy, les progiciels d'EFS deviennent les progiciels de back-office spécialisés pour la gestion des dérivés listés les plus utilisés en Italie.</p>
                            </li>
                            <li>
                                <span>2011</span>
                                <span>Arrêt de la commercialisation des produits de la gamme Eurosys® disponibles depuis 1995. 
                                    La gamme Eurosys® est intégralement remplacée par la gamme Spheres®.</span>
                            </li>
                        </ul>
                    </li>
                    <li>
                        <span>2004 à 2009</span>
                        <ul>
                            <li>
                                <span>2009</span>
                                <span>EFS termine le développement des fonctionnalités concernant les ETD sur Spheres® et les outils de migration de Eurosys® vers Spheres®; 
                                    Eurosys Futures® n’est plus commercialisé; Spheres® offre désormais une gestion intégrée des instruments OTC et ETD.</span>
                            </li>
                            <li>
                                <span>2008</span>
                                <span>Spheres® est choisi par OTCex pour la gestion de l’ensemble des opérations traitées dans le cadre de ses activités de broker. 
                                    Le périmètre instrumental est complet : dérivés listés, dérivés OTC & titres. 
                                    C’est la première fois que Spheres® est choisi pour couvrir un périmètre aussi large. 
                                    Il est par ailleurs prévu que toutes les filiales l’utilisent : Londres, Genève, Hong Kong, etc.</span>
                            </li>
                            <li>
                                <span>2005</span>
                                <span>EFS séduit trois institutions bancaires avec Spheres OTCml®, reposant sur la nouvelle plateforme de développement en environnement Web qui donne naissance à la troisième génération des produits Eurosys® rebaptisés à cette occasion Spheres®.</span>
                            </li>
                        </ul>
                    </li>
                    <li>
                        <span>1992 à 2003</span>
                        <ul>
                            <li>
                                <span>2003</span>
                                <span>EFS recrute une équipe provenant de chez un autre éditeur et ayant 15 années d’expérience dans le domaine du Back-Office des opérations sur les marchés de capitaux, en particulier sur les produits OTC.</span>
                            </li>
                            <li>
                                <span>2000</span>
                                <span>EFS rachète à DIAGRAM son activité autour du progiciel Norma®, concurrent direct de Eurosys Futures®.</span>
                            </li>
                            <li>
                                <span>1998</span>
                                <span>EFS rachète à TICK France son activité de fourniture de fichiers de paramètres de calcul concurrente de son service MDFS.</span>
                            </li>
                            <li>
                                <span>1996</span>
                                <span>Lancement commercial de la seconde génération d’Eurosys Futures®; application Windows® en architecture client/serveur et multi-SGBD/R.</span>
                            </li>
                            <li>
                                <span>1993</span>
                                <span>20 sociétés ont déjà choisi Eurosys Futures®.</span>
                            </li>
                            <li>
                                <span>1992</span>
                                <span>Création de EURO FINANCE SYSTEMS S.A. avec la participation à hauteur de 33% du Crédit Agricole d’Ile de France par l’intermédiaire de sa filiale de capital risque SOCADIF.</span>
                            </li>
                        </ul>
                    </li>
                </ul>
            </div>
            <div class="col-sm-offset-1 col-sm-3">
                <p class="title">Solution Euro Finance Systems</p>

                <div id="carousel-solution" class="carousel" data-ride="carousel">
                    <!-- Indicators -->
                    <ol class="carousel-indicators">
                        <li data-target="#carousel-solution" data-slide-to="0" class="active"></li>
                        <li data-target="#carousel-solution" data-slide-to="1"></li>
                        <li data-target="#carousel-solution" data-slide-to="2"></li>
                        <li data-target="#carousel-solution" data-slide-to="3"></li>
                        <li data-target="#carousel-solution" data-slide-to="4"></li>
                    </ol>

                    <!-- Wrapper for slides -->
                    <div class="carousel-inner" role="listbox">
                        <div class="item active" style="background-image: url('../Images/home.jpg');">
                        </div>
                        <div class="item" style="background-image: url('../Images/solutions.jpg');">
                        </div>
                        <div class="item" style="background-image: url('../Images/services.jpg');">
                        </div>
                        <div class="item" style="background-image: url('../Images/company.jpg');">
                        </div>
                        <div class="item" style="background-image: url('../Images/partners.jpg');">
                        </div>
                    </div>

                    <!-- Controls -->
                    <a class="left carousel-control" href="#carousel-solution" role="button" data-slide="prev">
                        <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>
                        <span class="sr-only">Previous</span>
                    </a>
                    <a class="right carousel-control" href="#carousel-solution" role="button" data-slide="next">
                        <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>
                        <span class="sr-only">Next</span>
                    </a>
                </div>
            </div>

        </div>
    </div>
</asp:Content>
