<%@ Page Title="" Language="C#" MasterPageFile="~/Portal/Portal.master" AutoEventWireup="true" CodeBehind="ContactUs.aspx.cs" Inherits="EFS.Spheres.ContactUs" %>

<asp:Content ID="contactContent" ContentPlaceHolderID="mc" runat="server">
    <div class="container-fluid header">
        contact us
    </div>
    <div class="container portal">
        <div id="contactusmsg" class="col-sm-6 v-right">
            <div class="row">
                <div class="col-sm-6">
                    <asp:Label ID="lblFirstName" runat="server" Text="Prénom"/>
                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control input-xs" aria-invalid="false" placeholder="Prénom"></asp:TextBox>
                </div>
                <div class="col-sm-6">
                    <asp:Label ID="lblSurName" runat="server" Text="Nom *"/>
                    <asp:TextBox ID="txtSurName" runat="server" CssClass="form-control input-xs" aria-required="true" aria-invalid="false" placeholder="Nom"></asp:TextBox>
                </div>
                <div class="col-sm-6">
                    <asp:Label ID="lblEmail" runat="server" Text="Email *"/>
                    <asp:TextBox ID="txtEMail" runat="server" CssClass="form-control input-xs" aria-required="true" aria-invalid="false" placeholder="Email"></asp:TextBox>
                </div>
                <div class="col-sm-6">
                    <asp:Label ID="lblFunction" runat="server" Text="Fonction"/>
                    <asp:TextBox ID="txtFunction" runat="server" CssClass="form-control input-xs" aria-required="true" aria-invalid="false" placeholder="Fonction"></asp:TextBox>
                </div>
                <div class="col-sm-6">
                    <asp:Label ID="lblCompanyName" runat="server" Text="Société *"/>
                    <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-control input-xs" aria-required="true" aria-invalid="false" placeholder="Société"></asp:TextBox>
                </div>
                <div class="col-sm-6">
                    <asp:Label ID="lblPhoneNumber" runat="server" Text="Téléphone *"/>
                    <asp:TextBox ID="txtPhoneNumber" runat="server" CssClass="form-control input-xs" aria-required="true" aria-invalid="false" placeholder="Téléphone"></asp:TextBox>
                </div>
                <div class="col-sm-6">
                    <asp:Label ID="lblSector" runat="server" Text="Secteur d'activité"/>
                    <asp:DropDownList ID="ddlSector" runat="server" CssClass="form-control input-xs" aria-invalid="false"/>
                </div>
                <div class="col-sm-6">
                    <asp:Label ID="lblRequest" runat="server" Text="Demande"/>
                    <asp:DropDownList ID="ddlRequest" runat="server" CssClass="form-control input-xs" aria-invalid="false"/>
                </div>
                <div class="col-sm-12">
                    <asp:Label ID="lblMessage" class="control-label" runat="server" Text="Message"/>
                    <textarea id="txtMessage" cols="40" rows="15" class="form-control input-xs" aria-invalid="false" placeholder="Détail"></textarea>
                </div>
            </div>
            <div class="row">
                <div class="col-sm-12">
                    <asp:Button ID="btnSend" runat="server" class="btn btn-xs btn-primary pull-right" Text="Envoyer" />
                </div>
            </div>
        </div>
        <div class="col-sm-6">
            <div id="adress" class="col-sm-12">
                <address>
                    <p class="title">EURO FINANCE SYSTEMS</p>
                    <span class="glyphicon glyphicon-map-marker"></span>2 Boulevard Albert 1er<br />
                    94130 Nogent sur Marne<br />
                    FRANCE<br />
                    <span class="glyphicon glyphicon-phone"></span> +33 (0)1 48 71 44 44<br />
                    <span class="glyphicon glyphicon-envelope"></span><a href="mailto:spheres@euro-finance-systems.com"> spheres@euro-finance-systems.com</a><br />
                </address>
            </div>
            <div id="map-container" class="col-sm-12">
            </div>
        </div>
        <script src="http://maps.google.com/maps/api/js?key=AIzaSyAg92BVqkpxhnBDgu389-_j3If1Xht-Zjo"></script>
        <script src="../Scripts/contact.js" type="text/javascript" charset="utf-8"></script>
    </div>
</asp:Content>
