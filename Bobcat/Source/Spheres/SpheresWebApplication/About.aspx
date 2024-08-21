<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="EFS.Spheres.AboutPage" %>

<!DOCTYPE html>
<html id="HtmlCenter" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>About</title>
</head>
<body class="PageCenter" id="BodyID" runat="server">
    <form id="About" method="post" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <table class="PageCenter" id="TblPreference" border="0">
            <tr>
                <td class="PageCenter">
                    <div class="PageCenter" style="width: 500px;">
                        <asp:PlaceHolder ID="plhHeader" runat="server"></asp:PlaceHolder>
                        <div id="divlogo" style="margin: 4px;">
                            <asp:HyperLink ID="imglogosoftware" Text="Software" runat="server" ImageUrl="Images\Logo_Software\Spheres_Banner_v9999.png"
                                NavigateUrl="http://www.euro-finance-systems.com"></asp:HyperLink>
                            <asp:HyperLink ID="imglogocompany" Text="Company"  runat="server" ImageUrl="Images\Logo_Entity\EuroFinanceSystems\LogoEFS.png"
                                NavigateUrl="http://www.euro-finance-systems.com"></asp:HyperLink>
                        </div>
                        <div id="divbody" runat="server">
                            <div>
                                <div id="divabout" runat="server">
                                    <div class="headh">
                                        <asp:Label ID="lblSoftware" class="size4" runat="server">SPHERES</asp:Label>
                                    </div>
                                    <div class="contenth">
                                        <div style="height: 100%">
                                            <div style="z-index: 6">
                                                <table runat="server" >
                                                    <tr>
                                                        <td>
                                                            <asp:Label ID="lblLicensee1" runat="server">Licensee</asp:Label>
                                                            <asp:Label ID="lblLicensee2" runat="server">Licensee</asp:Label>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:Label ID="lblDatabase1" runat="server">Database</asp:Label>
                                                            <asp:Label ID="lblDatabase2" runat="server">Database</asp:Label>
                                                            <asp:Label ID="lblDatabase2b" runat="server">Database</asp:Label>
                                                            <asp:Label ID="lblArchive" runat="server">Archive</asp:Label>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:Label ID="lblRDBMS1" runat="server">RDBMS</asp:Label>
                                                            <asp:Label ID="lblRDBMS2" runat="server">RDBMS</asp:Label>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:Label ID="lblProvider1" runat="server">Provider</asp:Label>
                                                            <asp:Label ID="lblProvider2" runat="server">Provider</asp:Label>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>&nbsp;</td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:Label ID="lblWarning" runat="server"><b>Warning:</b> This computer program is protected by copyright law and international treaties. Unauthorized reproduction or distribution of this program, or any portion of it, may result in severe civil and criminal penalties, and will be prosecured to the maximum extent possible under law.</asp:Label></td>
                                                    </tr>
                                                    <tr style="height: 15px;">
                                                        <td class="fixedText" style="width: 99%;">
                                                            <div class="hr" style="width: 100%; height: 2px;" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="title">The FpML License</td>
                                                    </tr>
                                                    <tr>
                                                        <td>&nbsp;</td>
                                                    </tr>
                                                    <tr>
                                                        <td>The <b>FpML</b> Specifications of this document are subject to the FpML Public License
                                                    (the "License"); you may not use the FpML Specifications except in compliance with
                                                    the License.
                                                    <br />
                                                            You may obtain a copy of the License at <a href="http://www.fpml.org">http://www.FpML.org</a>.
                                                    <br />
                                                            The FpML Specifications distributed under the License are distributed on an "AS
                                                    IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
                                                    for the specific language governing rights and limitations under the License.
                                                    <br />
                                                            The Licensor of the FpML Specifications is the <a href="http://www.isda.org">International Swaps and Derivatives Association, Inc</a>. All Rights Reserved.
                                                        </td>
                                                    </tr>
                                                    <tr style="height: 15px;">
                                                        <td class="fixedText" style="width: 100%;">
                                                            <div class="hr" style="width: 100%; height: 2px;" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="title">The Fix Protocol</td>
                                                    </tr>
                                                    <tr>
                                                        <td>&nbsp;</td>
                                                    </tr>
                                                    <tr>
                                                        <td>The <b>Financial Information eXchange (FIX) protocol</b> is a messaging standard
                                                    developed specifically for the real-time electronic exchange of securities transactions.
                                                    FIX is a public-domain specification owned and maintained by FIX Protocol, Ltd.
                                                    <br />
                                                            The <a href="http://www.fixprotocol.org">FIX Website</a> serves
                                                    as the central repository and authority for all specification documents, committee
                                                    calendars, discussion forums, presentations, and everything FIX.
                                                    <br />
                                                            "F.I.X Protocol" "Financial Information eXchange", "FIXML" and "F.I.X" are trademarks
                                                    or service marks of FIX Protocol Limited. The marks "F.I.X Protocol" "FIXML" and
                                                    "F.I.X" are registered trademarks in the European Community.
                                                    <br />
                                                        </td>
                                                    </tr>
                                                    <tr style="height: 15px;">
                                                        <td class="fixedText" style="width: 100%;">
                                                            <div class="hr" style="width: 100%; height: 2px;" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="title">The SPAN® framework</td>
                                                    </tr>
                                                    <tr>
                                                        <td>&nbsp;</td>
                                                    </tr>
                                                    <tr>
                                                        <td>SPAN® is a registered trademark of Chicago Mercantile Exchange Inc., used herein under license.
                                                        Chicago Mercantile Exchange Inc. assumes no liability in connection with the use of the SPAN® by any person or entity.
                                                        <br/>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <asp:Panel id="divalltoolbar" runat="server">
                            <div id="tblist">
                                <div>
                                    <asp:LinkButton ID="imgOk" runat="server" CssClass="fa-icon" OnClientClick="ClosePage();" OnClick="OnOk_Click"></asp:LinkButton>
                                    </div>
                                <div>
                                    <asp:LinkButton ID="imgBrowserInfo" runat="server" CssClass="fa-icon" OnClick="OnBrowserInfo_Click"></asp:LinkButton>
                                    <asp:LinkButton ID="imgSystemInfo" runat="server" CssClass="fa-icon" OnClick="OnSystemInfo_Click"></asp:LinkButton>
                                    <asp:LinkButton ID="imgComponents" runat="server" CssClass="fa-icon" OnClick="OnComponents_Click"></asp:LinkButton>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                </td>
            </tr>
        </table>
    </form>
</body>

    <script type="text/javascript">
        $(document).ready(function () {
            $("h2.header").unbind("click", hdlr_about);
            $("h2.header").bind("click", hdlr_about);
        });
    </script>
</html>
