<%@ Page Title="" Language="C#" MasterPageFile="~/Portal/Portal.Master" AutoEventWireup="true" CodeBehind="SiteMap.aspx.cs" Inherits="EFS.Spheres.SiteMap" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mc" runat="server">
    <div class="container-fluid header">
        site map
    </div>
    <div id="sitemap" class="container-fluid">
        <asp:Repeater ID="repeat_sm_architecture" runat="server" DataSourceID="portal_smds" OnItemDataBound="OnItemDataBound"/>
    </div>
</asp:Content>
