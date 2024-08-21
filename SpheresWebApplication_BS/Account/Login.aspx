<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="EFS.Spheres.Login" MasterPageFile="../Spheres.Master" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="mc">
    <script src="../Scripts/Validator.js" type="text/javascript" charset="utf-8"></script>
    <div class="container body-content">
        
        <div class="row">
            <div class="col-sm-offset-4 col-sm-4">
                <h2><%: Title %></h2>
                <div class="panel panel-primary">
                    <div class="panel-heading">
                        <h3 class="panel-title">
                            <asp:Label ID="lblIdentification" runat="server">Identification</asp:Label></h3>
                    </div>
                    <div class="panel-body">
                        <div id="alertLogin" class="col-lg-12" runat="server">
                            <div class="alert alert-danger" runat="server">
                                <asp:Label runat="server" ID="alertLoginMsg" CssClass="text-danger"></asp:Label>
                            </div>
                        </div>
                        <div id="alertLockout" class="col-lg-12" runat="server">
                            <div class="alert alert-danger" runat="server">
                                <asp:Label runat="server" ID="alertLockoutMsg" CssClass="text-danger"></asp:Label>
                            </div>
                        </div>

                        <!-- Identification -->
                        <div class="col-lg-12">
                            <asp:Label runat="server" ID="lblIdentifier" AssociatedControlID="txtCollaborator" CssClass="control-label">Identifiant</asp:Label>
                            <asp:RequiredFieldValidator runat="server" ID="rqvIdentifier" ControlToValidate="txtCollaborator" CssClass="vtor" ErrorMessage="The collaborator field is required." />
                            <div class="input-group input-group-xs">
                                <span class="input-group-addon"><span class="glyphicon glyphicon-user"></span></span>
                                <asp:TextBox runat="server" ID="txtCollaborator" CssClass="form-control input-xs"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-lg-12">
                            <asp:Label runat="server" ID="lblPassword" AssociatedControlID="txtPassword" CssClass="control-label">Password</asp:Label>
                            <asp:RequiredFieldValidator ID="rqvPassword" runat="server" ControlToValidate="txtPassword" CssClass="vtor" ErrorMessage="The password field is required." />
                            <div class="input-group input-group-xs">
                                <span class="input-group-addon"><span class="glyphicon glyphicon-lock"></span></span>
                                <asp:TextBox runat="server" ID="txtPassword" TextMode="Password" CssClass="form-control input-xs" />
                            </div>
                        </div>
                        <div class="col-sm-12">
                            <asp:CheckBox ID="chkRemember" runat="server" />
                            <asp:Label ID="lblRemember" AssociatedControlID="chkRemember" CssClass="control-label" runat="server">Se souvenir du profil</asp:Label>
                        </div>
                        <div class="col-sm-12">
                            <asp:Button runat="server" ID="btnLogin" OnClick="LogIn" Text="Log in" CssClass="btn btn-xs btn-primary pull-right" />
                        </div>
                        <div class="col-sm-12">
                            <hr />
                        </div>


                        <!-- Database -->
                        <div class="col-lg-12">
                            <div id="pnlDetail" runat="server" class="panel-group">
                            <div class="panel panel-info">
                                <div class="panel-heading panel-heading-collapsing">
                                    <h4 class="panel-title">
                                        <a data-toggle="collapse" href="#databaseinfo" class="collapsed">Database</a>
                                    </h4>
                                </div>
                                <div id="databaseinfo" class="panel-collapse collapse">
                                    <div class="panel-body">
                                        <div class="col-lg-12">
                                            <asp:Label runat="server" ID="lblRdbms" AssociatedControlID="ddlRdbmsName" CssClass="control-label">RDBMS</asp:Label>
                                            <asp:DropDownList runat="server" ID="ddlRdbmsName" CssClass="form-control input-xs" />
                                        </div>
                                        <div class="col-lg-12">
                                            <asp:Label runat="server" ID="lblServer" AssociatedControlID="txtServerName" CssClass="control-label">Server</asp:Label>
                                            <asp:TextBox runat="server" ID="txtServerName" CssClass="form-control input-xs" />
                                        </div>
                                        <div class="col-lg-12">
                                            <asp:Label runat="server" ID="lblDatabase" AssociatedControlID="txtDatabaseName" CssClass="control-label">DataBase</asp:Label>
                                            <asp:TextBox runat="server" ID="txtDatabaseName" CssClass="form-control input-xs" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
