<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="EFS.Spheres.About" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
    <script type="text/javascript">
        $(document).ready(function () {
            $('[data-toggle="popover"]').popover({
                html: true,
                trigger: 'focus',
                placement: 'auto top',
                container: 'body'
            });
        });

        $("button[data-dismiss-modal=modalComponent]").click(function () {
            $('#modalComponent').modal('hide');
        });

    </script>
</head>
<body>
    <form id="frmAbout" role="form" runat="server">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
            <h3 id="H1" runat="server" class="modal-title">About</h3>
        </div>
        <div class="modal-body">
            <div class="panel sph-nav-tabs panel-primary">
                <div class="panel-heading">
                    <ul class="nav nav-tabs">
                        <li class="active"><a href="#main" data-toggle="tab">Spheres</a></li>
                        <li><a href="#browser" data-toggle="tab">Browser</a></li>
                        <li><a href="#system" data-toggle="tab">System</a></li>
                        <li><a href="#components" data-toggle="tab">Components</a></li>
                    </ul>
                </div>
                <div class="panel-body">
                    <div class="tab-content">
                        <div class="tab-pane fade in active" id="main">
                            <h3 id="lblSoftware" runat="server" class="modal-title">Spheres version</h3>
                            <hr />
                            <div class="container-fluid">
                                <div class="form-group small">
                                    <asp:Label ID="lblLicensee" CssClass="col-sm-6" runat="server">Licensee</asp:Label>
                                    <div class="col-sm-6"><strong><asp:Label ID="lblLicenseeResult" runat="server"></asp:Label></strong></div>
                                    <asp:Label ID="lblRdbms" CssClass="col-sm-6" runat="server">RDBMS</asp:Label>
                                    <div class="col-sm-6"><strong><asp:Label ID="lblRdbmsResult" runat="server"></asp:Label></strong></div>
                                    <asp:Label ID="lblDatabase" CssClass="col-sm-6" runat="server">Database</asp:Label>
                                    <div class="col-sm-6"><strong><asp:Label ID="lblDatabaseResult" runat="server"></asp:Label></strong></div>
                                </div>
                            </div>
                            <hr />
                            <div class="container-fluid small">
                                <div class="text-justify text-warning">
                                    <span class="label label-warning">Warning</span>
                                    This computer program is protected by copyright law and international treaties. 
                        Unauthorized reproduction or distribution of this program, or any portion of it, may result in severe civil and criminal penalties, and will be prosecured to the maximum extent possible under law.
                                </div>
                            </div>
                            <hr />
                            <div class="panel panel-info small">
                                <div class="panel-heading">The FpML License</div>
                                <div class="panel-body text-justify">
                                        The <b>FpML</b> Specifications of this document are subject to the FpML Public License
                        (the "License"); you may not use the FpML Specifications except in compliance with the License.<br />
                                        You may obtain a copy of the License at <a class="linkButton" href="http://www.fpml.org">http://www.FpML.org</a>.<br />
                                        The FpML Specifications distributed under the License are distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
                        See the License for the specific language governing rights and limitations under the License.<br />
                                        The Licensor of the FpML Specifications is the <a class="linkButton" href="http://www.isda.org">ISDA</a>, International Swaps and Derivatives Association, Inc. All Rights Reserved.
                                </div>
                            </div>
                            <div class="panel panel-info small">
                                <div class="panel-heading">The Fix Protocol</div>
                                <div class="panel-body text-justify">
                                        The <b>Financial Information eXchange (FIX) protocol</b> is a messaging standard developed specifically for the real-time electronic exchange of securities transactions. 
                        FIX is a public-domain specification owned and maintained by FIX Protocol, Ltd.<br />
                                        The <a class="linkButton" href="http://www.fixprotocol.org">FIX Website</a> serves as the central repository and authority for all specification documents, committee
                        calendars, discussion forums, presentations, and everything FIX.<br />
                                        "F.I.X Protocol" "Financial Information eXchange", "FIXML" and "F.I.X" are trademarks or service marks of FIX Protocol Limited. The marks "F.I.X Protocol" "FIXML" and
                        "F.I.X" are registered trademarks in the European Community.
                                </div>
                            </div>
                        </div>
                        <div class="tab-pane fade " id="browser">
                            <h3 id="lblBrowserType" runat="server" class="modal-title">Browser Type</h3>
                            <div id="displayBrowserInfo" class="table-responsive small">
                                <asp:PlaceHolder ID="browserContent" runat="server"></asp:PlaceHolder>
                            </div>
                        </div>
                        <div class="tab-pane fade " id="system">
                            <h3 class="modal-title">Web application settings</h3>
                            <div id="displayWebInfo" class="table-responsive small">
                                <asp:PlaceHolder ID="webappsettingsContent" runat="server"></asp:PlaceHolder>
                            </div>
                            <h3 class="modal-title">Systems</h3>
                            <div id="displaySystemInfo" class="table-responsive small">
                                <asp:PlaceHolder ID="systemsContent" runat="server"></asp:PlaceHolder>
                            </div>
                        </div>
                        <div class="tab-pane fade " id="components">
                            <h3 class="modal-title">Components</h3>
                            <div id="displayComponentInfo" class="table-responsive small">
                                <asp:PlaceHolder ID="componentsContent" runat="server"></asp:PlaceHolder>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal-footer">
            <button type="button" class="btn btn-default btn-sm" data-dismiss="modal">Close</button>
        </div>
    </form>
    <!-- Modal HTML -->

  <!-- Modal -->
  <div class="modal fade" id="modalComponent" role="dialog">
    <div class="modal-dialog">
    
      <!-- Modal content-->
      <div class="modal-content">
        <div class="modal-header">
          <button type="button" class="close" data-dismiss-modal="modalComponent">&times;</button>
          <h4 class="modal-title">Modal Header</h4>
        </div>
        <div class="modal-body">
          <p>Some text in the modal.</p>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-default" data-dismiss-modal="modalComponent">Close</button>
        </div>
      </div>
    </div>
  </div>
</body>
</html>
