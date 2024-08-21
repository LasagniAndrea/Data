<%@ Page Title="" Language="C#" MasterPageFile="~/Spheres.master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="EFS.Spheres.ChangePassword" %>
<asp:Content ID="userProfilContent" ContentPlaceHolderID="mc" runat="Server">
    <div class="container body-content">
        <div class="col-sm-6">
            <h2><%: Title %>.</h2>
            <h3>Change you password.</h3>
            <div class="panel panel-primary">
                <div class="panel-heading">
                    <div class="panel-title">
                        <asp:Label ID="lblPasswordTitle" runat="server">Password</asp:Label>
                    </div>
                </div>
                <div class="panel-body">
                    <div id="alertPwd" runat="server">
                        <div id="alertPwdMsg" class="alert alert-danger" runat="server"/>
                    </div>
                    <div class="form-group form-group-xs">
                        <asp:Label runat="server" ID="lblOldPassword" AssociatedControlID="txtOldPassword" CssClass="col-sm-3 control-label input-xs">Old Password</asp:Label>
                        <div class="col-sm-9">
                            <div class="input-group">
                                <span class="input-group-addon input-xs"><span class="glyphicon glyphicon-lock"></span></span>
                                <asp:TextBox runat="server" ID="txtOldPassword" TextMode="Password" CssClass="form-control input-xs" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group form-group-xs">
                        <asp:Label runat="server" ID="lblNewPassword" AssociatedControlID="txtNewPassword" CssClass="col-sm-3 control-label input-xs">New Password</asp:Label>
                        <div class="col-sm-9">
                            <div class="input-group">
                                <span class="input-group-addon input-xs"><span class="glyphicon glyphicon-lock"></span></span>
                                <asp:TextBox runat="server" ID="txtNewPassword" TextMode="Password" CssClass="form-control input-xs" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group form-group-xs">
                        <asp:Label runat="server" ID="lblConfirmPassword" AssociatedControlID="txtConfirmPassword" CssClass="col-sm-3 control-label input-xs">Confirm New Password</asp:Label>
                        <div class="col-sm-9">
                            <div class="input-group">
                                <span class="input-group-addon input-xs"><span class="glyphicon glyphicon-lock"></span></span>
                                <asp:TextBox runat="server" ID="txtConfirmPassword" TextMode="Password" CssClass="form-control input-xs" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="panel-footer">
                    <asp:Button runat="server" ID="btnRecord" OnClick="Record" Text="Record" CssClass="btn btn-xs btn-record" />
                    <asp:Button runat="server" ID="btnCancel" OnClick="Cancel" Text="Cancel" CssClass="btn btn-xs btn-cancel" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
