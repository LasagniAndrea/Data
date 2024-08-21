<%@ Page Title="" Language="C#" MasterPageFile="~/Spheres.master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="EFS.Spheres.Contact" %>
<asp:Content ID="contactContent" ContentPlaceHolderID="mc" runat="server">
    <div class="container">
    <style>
      /*#map-info {
          background-image:url(Images/Logo_Entity/EuroFinanceSystems_Banner.gif);
          background-repeat:no-repeat;
          background-position:center center; 
      }*/
      #adress {
          margin-top:150px;
      }  
      #map-container 
      { 
          margin-top:100px;
          height: 300px; 
          border:solid 1.5pt #000;
          border-radius:10px;
      }
    </style>
        <div id="adress" class="col-sm-4">
        <h1>Contact us</h1>
        <address>
            <strong>EURO FINANCE SYSTEMS</strong><br/>
            <span class="glyphicon glyphicon-map-marker"></span>  2 Boulevard Albert 1er<br/>
            94130 Nogent sur Marne<br/>
            FRANCE<br/>
            <span class="glyphicon glyphicon-phone"></span> +33 (0)1 48 71 44 44<br/>
            <span class="glyphicon glyphicon-envelope"></span> spheres@euro-finance-systems.com<br/>
        </address>
        </div>
        <div id="map-container" class="col-sm-8"></div>
        <script src="http://maps.google.com/maps/api/js?key=AIzaSyAg92BVqkpxhnBDgu389-_j3If1Xht-Zjo"></script>
        <script src="Scripts/Contact.js" type="text/javascript" charset="utf-8"></script>
    </div>
</asp:Content>
