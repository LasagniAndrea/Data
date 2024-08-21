<%@ Page Title="" Language="C#" MasterPageFile="~/Spheres.Master" AutoEventWireup="true" CodeBehind="UserProfil.aspx.cs" Inherits="EFS.Spheres.UserProfil" %>

<asp:Content ID="userProfilContent" ContentPlaceHolderID="mc" runat="Server">
    <div id="divUserProfil" runat="server" class="body-content container-fluid startprint">
        <h2><%: Title %>.</h2>
        <h3>Your profil page.</h3>
        <div class="panel panel-primary">
            <div class="panel-heading">
                <asp:Label ID="lblPreferences" CssClass="panel-title" runat="server">Preferences</asp:Label>
            </div>

            <div class="panel-body">
                <%--Culture--%>
                <div class="row">
                <div id="divCulture" runat="server" class="col-lg-12">
                    <div class="col-lg-6">
                        <asp:Label runat="server" ID="lblCulture" AssociatedControlID="ddlCulture" CssClass="control-label">Culture</asp:Label>
                        <div class="input-group input-group-xs">
                            <span class="input-group-addon"><span class="glyphicon glyphicon-flag"></span></span>
                            <asp:DropDownList ID="ddlCulture" runat="server" CssClass="form-control input-xs" />
                        </div>
                    </div>
                </div>
                </div>
                <%--Preferences--%>
                <div class="row">
                <div id="divPreferences" runat="server" class="col-lg-6">
                    <div class="col-lg-12">
                        <asp:Label runat="server" ID="lblDefaultPage" AssociatedControlID="ddlDefaultPage" CssClass="control-label">Home page</asp:Label>
                        <div class="input-group input-group-xs">
                            <span class="input-group-addon"><span class="glyphicon glyphicon-modal-window"></span></span>
                            <asp:DropDownList ID="ddlDefaultPage" runat="server" CssClass="form-control input-xs" />
                        </div>
                    </div>
                    <div class="col-sm-6">
                        <asp:Label ID="lblETDMaturityFormat" AssociatedControlID="ddlETDMaturityFormat" CssClass="control-label" runat="server">Maturity format for Listed Derivative</asp:Label>
                        <div class="input-group input-group-xs">
                            <span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span>
                            <asp:DropDownList ID="ddlETDMaturityFormat" runat="server" CssClass="form-control input-xs" />
                            <div id="lblPerETD" runat="server" class="input-group-addon">for ETD</div>
                        </div>
                    </div>
                    <div class="col-sm-6">
                        <asp:Label ID="lblTradingTimestampZone" AssociatedControlID="ddlTradingTimestampZone" CssClass="control-label" runat="server">Trading Timestamp Zone</asp:Label>
                        <div class="input-group input-group-xs">
                            <span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span>
                            <asp:DropDownList ID="ddlTradingTimestampZone" runat="server" CssClass="form-control input-xs" />
                        </div>
                        <asp:Label ID="lblTradingTimestampPrecision" AssociatedControlID="ddlTradingTimestampPrecision" CssClass="control-label" runat="server">Trading Timestamp Precision</asp:Label>
                        <div class="input-group input-group-xs">
                            <span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span>
                            <asp:DropDownList ID="ddlTradingTimestampPrecision" runat="server" CssClass="form-control input-xs" />
                        </div>
                    </div>
                    <div class="col-sm-6">
                        <asp:Label ID="lblAuditTimestampZone" AssociatedControlID="ddlAuditTimestampZone" CssClass="control-label" runat="server">Audit Timestamp Zone</asp:Label>
                        <div class="input-group input-group-xs">
                            <span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span>
                            <asp:DropDownList ID="ddlAuditTimestampZone" runat="server" CssClass="form-control input-xs" />
                        </div>
                        <asp:Label ID="lblAuditTimestampPrecision" AssociatedControlID="ddlAuditTimestampPrecision" CssClass="control-label" runat="server">Audit Timestamp Precision</asp:Label>
                        <div class="input-group input-group-xs">
                            <span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span>
                            <asp:DropDownList ID="ddlAuditTimestampPrecision" runat="server" CssClass="form-control input-xs" />
                        </div>
                    </div>
                    <div class="col-sm-6">
                        <asp:Label ID="lblNumberRowByPage" AssociatedControlID="txtNumberRowByPage" CssClass="control-label" runat="server">Number of rows displayed per page</asp:Label>
                        <div class="input-group input-group-xs">
                            <asp:TextBox ID="txtNumberRowByPage" runat="server" CssClass="form-control input-xs">20</asp:TextBox>
                            <div id="lblPerPage" runat="server" class="input-group-addon">per page.</div>
                        </div>
                    </div>
                </div>
                <div id="divPreferences2" runat="server" class="col-lg-offset-1 col-lg-5">
                    <div class="col-lg-12">
                        <h1 />
                    </div>
                    <div class="col-lg-12">
                        <asp:CheckBox ID="chkUniquePage" runat="server" />
                        <asp:Label ID="lblUniquePage" AssociatedControlID="chkUniquePage" CssClass="control-label" runat="server">Open the pop-up pages within the same browser</asp:Label>
                    </div>
                    <div class="col-lg-12">
                        <asp:CheckBox ID="chkBackgroundWhite" runat="server" />
                        <asp:Label ID="lblBackgroundWhite" AssociatedControlID="chkBackgroundWhite" CssClass="control-label" runat="server">White background</asp:Label>
                    </div>
                    <div class="col-lg-12">
                        <asp:CheckBox ID="chkPagerPosition" runat="server" />
                        <asp:Label ID="lblPagerPosition" AssociatedControlID="chkPagerPosition" CssClass="control-label" runat="server">Display the list of pages in the header</asp:Label>
                    </div>
                </div>
                </div>
                <%--Reset--%>
                <div class="row">
                <div id="divReset" runat="server" class="col-lg-4 panel-body">
                    <asp:Label ID="lblReset" CssClass="sph-title" runat="server">Reset</asp:Label>
                    <div class="panel-body">
                        <div class="col-sm-6">
                            <asp:CheckBox ID="chkTrackerLog" runat="server" />
                            <asp:Label ID="lblTrackerLog" AssociatedControlID="chkTrackerLog" CssClass="control-label" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:CheckBox ID="chkProcessLog" runat="server" />
                            <asp:Label ID="lblProcessLog" AssociatedControlID="chkProcessLog" CssClass="control-label" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:CheckBox ID="chkSystemLog" runat="server" />
                            <asp:Label ID="lblSystemLog" AssociatedControlID="chkSystemLog" CssClass="control-label" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:CheckBox ID="chkRDBMSLog" runat="server" />
                            <asp:Label ID="lblRDBMSLog" AssociatedControlID="chkRDBMSLog" CssClass="control-label" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:CheckBox ID="chkDefault" runat="server" />
                            <asp:Label ID="lblDefault" AssociatedControlID="chkDefault" CssClass="control-label" runat="server" />
                        </div>
                        <div class="col-sm-4">
                            <asp:CheckBox ID="chkCache" runat="server" />
                            <asp:Label ID="lblCache" AssociatedControlID="chkCache" CssClass="control-label" runat="server" />
                        </div>
                        <div class="col-sm-2">
                            <button runat="server" id="btnReset" class="btn btn-xs btn-remove pull-right"></button>
                        </div>
                    </div>
                </div>
                <%--Tracker--%>
                <div id="divTracker" runat="server" class="col-sm-6 col-lg-4 panel-body">
                    <asp:Label ID="lblTracker" CssClass="sph-title" runat="server">Tracker</asp:Label>
                    <div>
                        <div class="col-sm-6">
                            <asp:Label ID="lblTrackerRefresh" AssociatedControlID="txtTrackerRefreshInterval" CssClass="control-label" runat="server">Refresh interval (in seconds)</asp:Label>
                            <div class="input-group input-group-xs">
                                <span class="input-group-addon"><span class="glyphicon glyphicon-time"></span></span>
                                <asp:TextBox ID="txtTrackerRefreshInterval" runat="server" CssClass="form-control input-xs"></asp:TextBox>
                                <div class="input-group-addon">sec.</div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <asp:Label ID="lblTrackerNbRow" AssociatedControlID="txtTrackerNbRowPerGroup" CssClass="control-label" runat="server">Number of rows displayed per group</asp:Label>
                            <div class="input-group input-group-xs">
                                <asp:TextBox ID="txtTrackerNbRowPerGroup" runat="server" CssClass="form-control input-xs"></asp:TextBox>
                                <div id="lblPerGroup" runat="server" class="input-group-addon">per group.</div>
                            </div>
                        </div>
                        <div class="col-lg-12">
                            <asp:CheckBox ID="chkTrackerAlert" runat="server" />
                            <asp:Label ID="lblTrackerAlert" AssociatedControlID="chkTrackerAlert" CssClass="control-label" runat="server">Notification at the end of each processing request</asp:Label>
                        </div>
                        <div class="col-lg-12">
                            <asp:CheckBox ID="chkTrackerVelocity" runat="server" />
                            <asp:Label ID="lblTrackerVelocity" AssociatedControlID="chkTrackerVelocity" CssClass="control-label" runat="server">Show the scrolling of status counters</asp:Label>
                        </div>
                    </div>
                </div>
                <%--Others--%>
                <div id="divActions" runat="server" class="col-sm-6 col-lg-4 panel-body">
                    <asp:Label ID="lblOthers" CssClass="sph-title" runat="server">Others</asp:Label>
                    <div>
                        <div class="col-lg-12">
                            <a runat="server" id="btnChangePassword" href="~/ChangePassword.aspx?FROMPROFIL=1" title="Change password"><span class="glyphicon glyphicon-lock"></span></a>
                            <asp:Label ID="lblChangePassword" AssociatedControlID="btnChangePassword" CssClass="control-label" runat="server">Password change</asp:Label>
                        </div>
                        <div class="col-lg-12">
                            <a runat="server" id="btnShowDataCache" href="#"><span class="glyphicon glyphicon-search"></span></a>
                            <asp:Label ID="lblShowDataCache" AssociatedControlID="btnShowDataCache2" CssClass="control-label" runat="server">Cache</asp:Label>
                        </div>
                        <div class="col-lg-12">
                            <a runat="server" id="btnLoginLog" href="#"><span class="glyphicon glyphicon-search"></span></a>
                            <asp:Label ID="lblLoginLog" AssociatedControlID="btnLoginLog" CssClass="control-label" runat="server">Login journal</asp:Label>
                        </div>
                        <div class="col-lg-12">
                            <a runat="server" id="btnUserLock" href="#"><span class="glyphicon glyphicon-search"></span></a>
                            <asp:Label ID="lblUserLock" AssociatedControlID="btnUserLock" CssClass="control-label" runat="server">User Lock</asp:Label>
                        </div>
                        <div class="col-lg-12">
                            <a runat="server" id="btnHostLock" href="#"><span class="glyphicon glyphicon-search"></span></a>
                            <asp:Label ID="lblHostLock" AssociatedControlID="btnHostLock" CssClass="control-label" runat="server">HostName Lock</asp:Label>
                        </div>
                    </div>
                </div>
                </div>
            </div>

            <div class="panel-footer">
                <asp:Button ID="btnOk" runat="server" CssClass="btn btn-xs btn-record" OnClick="Valid" Text="Ok" />
                <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-xs btn-cancel" OnClick="Cancel" Text="Cancel" />
                <asp:Button ID="btnApply" runat="server" CssClass="btn btn-xs btn-apply" OnClick="Apply" Text="Apply" />
            </div>
        </div>
    </div>
    <!-- Modal HTML -->
    <div id="userModal" class="modal fade" role="dialog" aria-labelledby="userModalLabel">
        <div class="modal-dialog modal-sm" role="document">
            <div class="modal-content">
                <!-- Content will be loaded here from remote.aspx file -->
            </div>
        </div>
    </div>
</asp:Content>
