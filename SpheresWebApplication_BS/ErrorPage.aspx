<%@ Page Title="" Language="C#" MasterPageFile="~/Spheres.master" AutoEventWireup="true" CodeBehind="ErrorPage.aspx.cs" Inherits="EFS.Spheres.ErrorPage" %>
<asp:Content ID="ErrorContent" ContentPlaceHolderID="mc" runat="Server">
    <div class="container body-content">
        <div id="pnlError" class="startprint">
            <div class="col-sm-12">
                <div class="panel panel-danger">
                    <div class="panel-heading">
                        Error
                    </div>
                    <div class="panel-body panel-body">
                        <div class="col-sm-12">
                            <asp:Label ID="friendlyErrorMsg" runat="server"></asp:Label>
                        </div>
                        <div id="detailedErrorPanel" runat="server" class="col-sm-12">
                            <h3><span class="glyphicon glyphicon-chevron-right"></span>Error type</h3>
                            <p>
                                <asp:Label ID="errorDetailedMsg" runat="server" />
                            </p>
                            <h3><span class="glyphicon glyphicon-chevron-right"></span>Error source</h3>
                            <p>
                                <asp:Label ID="innerTarget" runat="server" />
                            </p>
                            <h3><span class="glyphicon glyphicon-chevron-right"></span>Detailed Error Message</h3>
                            <blockquote>
                                <p class="message">
                                    <asp:Label ID="innerMessage" runat="server" />
                                </p>
                                <pre id="innerTrace" runat="server"/>
                            </blockquote>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
