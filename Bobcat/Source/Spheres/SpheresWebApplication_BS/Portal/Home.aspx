<%@ Page Title="" Language="C#" MasterPageFile="~/Portal/Portal.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="EFS.Spheres.Home" %>

<asp:Content ID="homePage" ContentPlaceHolderID="mc" runat="server">
    <div id="carousel-home" class="carousel slide" data-ride="carousel">
        <!-- Indicators -->
        <ol class="carousel-indicators">
            <li data-target="#carousel-home" data-slide-to="0" class="active"></li>
            <li data-target="#carousel-home" data-slide-to="1"></li>
            <li data-target="#carousel-home" data-slide-to="2"></li>
            <li data-target="#carousel-home" data-slide-to="3"></li>
            <li data-target="#carousel-home" data-slide-to="4"></li>
        </ol>

        <!-- Wrapper for slides -->
        <div class="carousel-inner" role="listbox">
            <div class="item active" style="background-image: url('../Portal/Images/home.jpg');">
                <div class="carousel-caption">
                    <div class="cadre">
                        <h1>Euro Finance Systems</h1>
                        <h3>Provider of Capital Markets Solutions</h3>
                        <p>
                            Bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                            bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                            bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                            bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                            <a class="more" href="Company/Company.aspx">En savoir +</a><br />
                        </p>
                    </div>
                </div>
            </div>
            <div class="item" style="background-image: url('../Portal/Images/solutions.jpg');">
                <div class="carousel-caption">
                    <div class="cadre">
                        <h1>Solutions</h1>
                        <p>
                            Bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                            bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                            <a class="more" href="Solutions/Solutions.aspx">En savoir +</a><br />
                        </p>
                    </div>
                </div>
            </div>
            <div class="item" style="background-image: url('../Portal/Images/services.jpg');">
                <div class="carousel-caption">
                    <div class="cadre">
                        <h1>Services</h1>
                        <p>
                            Bla bla bla bla, bla bla bla bla, bla bla bla bla, bla bla 
                            bla bla bla bla bla bla bla bla bla bla bla bla bla bla,.<br />
                            <a class="more" href="Services/Services.aspx">En savoir +</a><br />
                        </p>
                    </div>
                </div>
            </div>
            <div class="item" style="background-image: url('../Portal/Images/company.jpg');">
                <div class="carousel-caption">
                    <div class="cadre">
                        <h1>Company</h1>
                        <p>Bla bla bla bla, bla bla bla bla, bla bla bla bla, bla bla bla bla,.</p>
                    </div>
                </div>
            </div>
            <div class="item" style="background-image: url('../Portal/Images/partners.jpg');">
                <div class="carousel-caption">
                    <div class="cadre">
                        <h1>Partners</h1>
                        <p>Bla bla bla bla, bla bla bla bla, bla bla bla bla, bla bla bla bla,.</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Controls -->
        <a class="left carousel-control" href="#carousel-home" role="button" data-slide="prev">
            <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>
            <span class="sr-only">Previous</span>
        </a>
        <a class="right carousel-control" href="#carousel-home" role="button" data-slide="next">
            <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>
            <span class="sr-only">Next</span>
        </a>
    </div>
    <div class="container portal home">
        <div class="row">
            <div class="col-sm-4">
                <h2 class="title">Solutions</h2>
                <p>
                    Bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                    bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                    <a class="more" href="Solutions/Solutions.aspx">En savoir +</a><br />
                </p>
            </div>
            <div class="col-sm-4">
                <h2 class="title">Services</h2>
                <p>
                    Bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                    bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                    <a class="more" href="Services/Services.aspx">En savoir +</a><br />
                </p>
            </div>
            <div class="col-sm-4">
                <h2 class="title">Resources</h2>
                <p>
                    Bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                    bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla
                bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla bla<br />
                    <a class="more" href="Resources/Resources.aspx">En savoir +</a><br />
                </p>
            </div>
        </div>
    </div>
</asp:Content>
