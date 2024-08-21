<%@ Page Language="c#" Inherits="EFS.Spheres.ProfilePage" CodeBehind="Profile.aspx.cs" %>

<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>

<!DOCTYPE html>
<html id="HtmlCenter" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Profile</title>
</head>
<body id="divprofile" runat="server">
    <form id="frmProfile" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <div class="PageCenter" style="width: 700px;">
            <div>
                <asp:PlaceHolder ID="plhMain" runat="server"></asp:PlaceHolder>
                <asp:panel id="divalltoolbar" runat="server">
                    <div id="divtoolbar" runat="server">
                        <efsc:WCToolTipLinkButton ID="imgOk" CssClass="fa-icon" runat="server" OnClick="BtnOk_Click" Text="<i class='fa fa-check-square'></i>"></efsc:WCToolTipLinkButton>
                        <efsc:WCToolTipLinkButton ID="imgCancel" CssClass="fa-icon" runat="server" OnClick="BtnCancel_Click" Text="<i class='fa fa-times-circle'></i>"></efsc:WCToolTipLinkButton>
                        <efsc:WCToolTipLinkButton ID="imgApply" CssClass="fa-icon" runat="server" OnClick="BtnApply_Click" Text="<i class='fa fa-save'></i>"></efsc:WCToolTipLinkButton>
                    </div>
                </asp:panel>
            </div>
            <asp:Panel ID="divbody" runat="server">
                <asp:Panel runat="server" ID="divculture" Style="margin-bottom: 4px;">
                    <div class="headh">
                        <span id="lblCultureTitle" class="size4" runat="server">Culture</span>
                    </div>
                    <div class="contenth">
                        <div>
                            <table style="border-collapse: collapse; width: 100%; margin-top: 1em;">
                                <tr>
                                    <td>
                                        <asp:Label ID="lblIdentifier" runat="server">Identifier</asp:Label>
                                    </td>
                                    <td>
                                        <div style="float: right;">
                                            <asp:Label ID="lblIdentifierInfo" runat="server"></asp:Label>
                                            <asp:Panel runat="server" CssClass="fa-icon" ID="imgUserLink" Style="display: inline;">
                                                <i class="fas fa-search"></i>
                                            </asp:Panel>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <p>
                                            <asp:Label ID="lblDeptOrDesk" runat="server">DeptOrDesk</asp:Label> 
                                        </p>
                                    </td>
                                    <td>
                                        <div style="float: right;">
                                            <asp:Label ID="lblDeptOrDeskInfo" runat="server"></asp:Label>
                                            <asp:Panel runat="server" CssClass="fa-icon" ID="imgDeptOrDeskLink" Style="display: inline;">
                                                <i class="fas fa-search"></i>
                                            </asp:Panel>
                                        </div>

                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblEntity" runat="server">Entity</asp:Label>                               
                                    </td>
                                    <td>
                                        <div style="float: right;">
                                            <asp:Label ID="lblEntityInfo" runat="server"></asp:Label>
                                            <asp:Panel runat="server" CssClass="fa-icon" ID="imgEntityLink" Style="display: inline;">
                                                <i class="fas fa-search"></i>
                                            </asp:Panel>
                                        </div>

                                    </td>
                                </tr>
                            </table> 
                                <p style="border-bottom: solid 1pt #C00303;"></p>
                            <div>
                                <p>
                                    <asp:Label ID="lblCulture" runat="server">Culture</asp:Label>
                                    <asp:DropDownList ID="ddlCulture" runat="server" Style="float: right;" CssClass="ddlCapture" />
                                </p>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel runat="server" ID="divpreferences" Style="margin-bottom: 4px;">
                    <div class="headh">
                        <span class="size4" id="lblPreferences" runat="server">Preferences</span>
                    </div>
                    <div class="contenth">
                        <div>
                            <p>
                                <asp:Label ID="lblDefaultPage" runat="server">DefaultPage</asp:Label>
                                <asp:DropDownList ID="ddlDefaultPage" runat="server" Style="float: right;" CssClass="ddlCapture" />
                            </p>
                            <p style="border-bottom: solid 1pt #C00303;"></p>
                            <p>
                                <asp:Label ID="lblETDMaturityFormat" runat="server">ETDMaturityFormat</asp:Label>
                                <asp:DropDownList ID="ddlETDMaturityFormat" runat="server" Style="float: right;" CssClass="ddlCapture" />
                            </p>
                            <p style="border-bottom: solid 1pt #C00303;"></p>
                            <p>
                                <asp:Label ID="lblTradingTimestampZone" runat="server">lblTradingTimestampZone</asp:Label>
                                <asp:DropDownList ID="ddlTradingTimestampZone" runat="server" Style="float: right;" CssClass="ddlCapture" />
                            </p>
                            <p>
                                <asp:Label ID="lblTradingTimestampPrecision" runat="server">lblTradingTimestampPrecision</asp:Label>
                                <asp:DropDownList ID="ddlTradingTimestampPrecision" runat="server" Style="float: right;" CssClass="ddlCapture" />
                            </p>
                            <p style="border-bottom: solid 1pt #C00303;"></p>
                            <p>
                                <asp:Label ID="lblAuditTimestampZone" runat="server">lblAuditTimestampZone</asp:Label>
                                <asp:DropDownList ID="ddlAuditTimestampZone" runat="server" Style="float: right;" CssClass="ddlCapture" />
                            </p>
                            <p>
                                <asp:Label ID="lblAuditTimestampPrecision" runat="server">lblAuditTimestampPrecision</asp:Label>
                                <asp:DropDownList ID="ddlAuditTimestampPrecision" runat="server" Style="float: right;" CssClass="ddlCapture" />
                            </p>



                            <p style="border-bottom: solid 1pt #C00303;"></p>
                            <p>
                                <asp:Label ID="lblCSSMode" runat="server">BackgroundWhite</asp:Label>
                                <asp:DropDownList ID="ddlCSSMode" runat="server" Style="float: right;" CssClass="ddlCapture" />
                            </p>
                            <p style="border-bottom: solid 1pt #C00303;"></p>
                            <p>
                                <asp:Label ID="lblNumberRowByPage" runat="server">NumberRowByPage</asp:Label>
                                <asp:TextBox ID="txtNumberRowByPage" runat="server" CssClass="txtCaptureNumeric" Style="float: right;">20</asp:TextBox>
                            </p>
                            <p>
                                <asp:Label ID="lblPagerPosition" runat="server">PagerPosition</asp:Label>
                                <asp:CheckBox ID="chkPagerPosition" runat="server" Style="float: right;"></asp:CheckBox>
                            </p>
                            <p>
                                <asp:Label ID="lblValidityData" runat="server">Données visibles dans les référentiels</asp:Label>
                                <asp:DropDownList ID="ddlProfilValidityData" runat="server" Style="float: right;" CssClass="ddlCapture" />
                            </p>
                            <p style="border-bottom: solid 1pt #C00303;"></p>
                            <p>
                                <asp:Label ID="lblTrackerRefreshInterval" runat="server">TrackerRefreshInterval</asp:Label>
                                <asp:TextBox ID="txtTrackerRefreshInterval" runat="server" Width="159px" CssClass="txtCaptureNumeric" Style="float: right;"></asp:TextBox>
                            </p>
                            <p>
                                <asp:Label ID="lblTrackerNbRowPerGroup" runat="server">TrackerNbRowPerGroup</asp:Label>
                                <asp:TextBox ID="txtTrackerNbRowPerGroup" runat="server" Width="159px" CssClass="txtCaptureNumeric" Style="float: right;"></asp:TextBox>
                            </p>
                            <p>
                                <asp:Label ID="lblTrackerAlert" runat="server">TrackerAlert</asp:Label>
                                <asp:CheckBox ID="chkTrackerAlert" runat="server" Style="float: right;"></asp:CheckBox>
                            </p>
                            <p>
                                <asp:Label ID="lblTrackerVelocity" runat="server">TrackerVelocity</asp:Label>
                                <asp:CheckBox ID="chkTrackerVelocity" runat="server" Style="float: right;"></asp:CheckBox>
                            </p>
                        </div>
                    </div>
                </asp:Panel>
                    <asp:Panel runat="server" ID="divreset" Style="margin-right: 2px; float: left; width: 50%">
                        <div class="headh">
                            <span class="size4" id="lblReset" runat="server">Reset</span>
                        </div>
                        <div class="contenth">
                            <table style="border-collapse: collapse; width: 100%">
                                <tr>
                                    <td>
                                        <asp:CheckBox ID="chkTrackerLog" runat="server"></asp:CheckBox></td>
                                    <td>
                                        <asp:CheckBox ID="chkProcessLog" runat="server"></asp:CheckBox></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:CheckBox ID="chkSystemLog" runat="server"></asp:CheckBox></td>
                                    <td>
                                        <asp:CheckBox ID="chkRDBMSLog" runat="server"></asp:CheckBox></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:CheckBox ID="chkCache" runat="server"></asp:CheckBox></td>
                                    <td>
                                        <asp:CheckBox ID="chkDefault" runat="server"></asp:CheckBox></td>
                                    <td>
                                        <efsc:WCToolTipLinkButton ID="imgReset" CssClass="fa-icon" runat="server" Style="float: right;" OnClick="BtnReset_Click"></efsc:WCToolTipLinkButton></td>
                                </tr>
                            </table>
                        </div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="divactions" Style="overflow: auto;">
                        <div class="headh">
                            <span class="size4" id="lblOthers" runat="server">Autres</span>
                        </div>
                        <div class="contenth">
                            <div id="pnlAction">
                                <table style="border-collapse: collapse; width: 100%">
                                    <tr>
                                        <td style="border-bottom: solid 1pt #C00303;">
                                            <asp:Label ID="lblChangePassword" runat="server">Password</asp:Label>&nbsp;</td>
                                        <td style="border-bottom: solid 1pt #C00303;">
                                            <asp:LinkButton runat="server" CssClass="fa-icon" ID="btnChangePassword" OnClick="BtnChangePassword_Click" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblCache" runat="server">Cache</asp:Label></td>
                                        <td>
                                            <asp:LinkButton runat="server" CssClass="fa-icon" ID="btnShowDataCache" OnClick="BtnShowDataCache_Click" /></td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblLoginLog" runat="server">Login journal</asp:Label></td>
                                        <td>
                                            <asp:Panel runat="server" CssClass="fa-icon" ID="imgLoginLog"><i class="fas fa-search"></i></asp:Panel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblUserLock" runat="server">User Lock</asp:Label></td>
                                        <td>
                                            <asp:Panel runat="server" CssClass="fa-icon" ID="imgUserLock"><i class="fas fa-search"></i></asp:Panel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblHostLock" runat="server">HostName Lock</asp:Label></td>
                                        <td>
                                            <asp:Panel runat="server" CssClass="fa-icon" ID="imgHostLock"><i class="fas fa-search"></i></asp:Panel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblImpersonate" runat="server">User Impersonation</asp:Label>

                                        </td>
                                        <td>
                                            <asp:Panel runat="server" CssClass="fa-icon" ID="imgImpersonate"><i class="fas fa-search"></i></asp:Panel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblImpersonatedByMe" runat="server">User I can impersonate</asp:Label>

                                        </td>
                                        <td>
                                            <asp:Panel runat="server" CssClass="fa-icon" ID="imgImpersonatedByMe"><i class="fas fa-search"></i></asp:Panel>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </asp:Panel>
            </asp:Panel>
        </div>
    </form>
</body>
</html>
