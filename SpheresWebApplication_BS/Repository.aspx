<%@ Page Title="" Language="C#" MasterPageFile="~/Spheres.Master" AutoEventWireup="true" CodeBehind="Repository.aspx.cs" Inherits="EFS.Spheres.Repository" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mc" runat="server">

    <script src="Scripts/Notepad.js" type="text/javascript" charset="utf-8"></script>
    <script src="Scripts/Repository.js" type="text/javascript" charset="utf-8"></script>
    <script src="Scripts/Validator.js" type="text/javascript" charset="utf-8"></script>

    <nav id="nvb" runat="server" role="navigation" class="navbar navbar-default navbar-fixed-top">
        <div class="container-fluid">
            <div class="navbar-header">
                <button type="button" class="navbar-toggle" data-target="#nvbcollapse" data-toggle="collapse">
                    <span class="sr-only">Toggle navigation</span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                </button>
            </div>
            <div id="nvbcollapse" class="collapse navbar-collapse">
                <div runat="server" id="pnlaction" class="col-sm-1">
                    <asp:Label runat="server" ID="titleaction"></asp:Label>
                </div>
                <div class="col-sm-8">
                    <ul class="nav navbar-nav navbar-right">
                        <li class="sph-v-divider"></li>
                        <asp:PlaceHolder ID="plhMenuDetail" runat="server" />
                        <asp:PlaceHolder ID="plhMenuDetailOthers" runat="server" />
                        <li class="sph-v-divider"></li>
                        <li>
                            <button id="btnLogo" runat="server" type="button" class="btn btn-xs btn-logo" title="logo"><span class="glyphicon glyphicon-picture"></span></button>
                            <button id="btnAttacheDoc" runat="server" type="button" class="btn btn-xs btn-attacheddocempty" title="Attache doc"><span class="glyphicon glyphicon-paperclip"></span></button>
                            <button id="btnNotepad" runat="server" onclick="return false;" data-load-url="Notepad.aspx" class="btn btn-xs btn-notepadempty" title="Notepad" data-toggle="modal" data-target="#formModal" data-backdrop="static"><span class="glyphicon glyphicon-paste"></span></button>
                        </li>
                        
                        <li class="sph-v-divider"></li>
                        <li>
                            <button id="btnFirstRecord" runat="server" type="button" class="btn btn-xs btn-apply" onserverclick="OnConsult" title="1st record"><span class="glyphicon glyphicon-fast-backward"></span></button>
                            <button id="btnPreviousRecord" runat="server" type="button" class="btn btn-xs btn-apply" onserverclick="OnConsult" title="Previous record"><span class="glyphicon glyphicon-backward"></span></button>
                            <button id="btnNextRecord" runat="server" type="button" class="btn btn-xs btn-apply" onserverclick="OnConsult" title="Next record"><span class="glyphicon glyphicon-forward"></span></button>
                            <button id="btnLastRecord" runat="server" type="button" class="btn btn-xs btn-apply" onserverclick="OnConsult" title="Last record"><span class="glyphicon glyphicon-fast-forward"></span></button>
                        </li>
                        <li class="sph-v-divider"></li>
                        <li>
                            <button id="btnRecord" runat="server" type="button" class="btn btn-xs btn-record" title="Record" onserverclick="OnRecord"><span class="glyphicon glyphicon-floppy-save"></span></button>
                            <button id="btnCancel" runat="server" type="button" class="btn btn-xs btn-cancel" title="Annul"><span class="glyphicon glyphicon-remove"></span></button>
                            <button id="btnApply" runat="server" type="button" class="btn btn-xs btn-apply" title="Apply" onserverclick="OnRecord"><span class="glyphicon glyphicon-ok"></span></button>
                            <button id="btnRemove" runat="server" type="button" class="btn btn-xs btn-remove" title="Remove" onserverclick="OnRemove"><span class="glyphicon glyphicon-floppy-remove"></span></button>
                            <button id="btnDuplicate" runat="server" type="button" class="btn btn-xs btn-duplicate" title="Duplicate" onserverclick="OnDuplicate"><span class="glyphicon glyphicon-duplicate"></span></button>
                        </li>
                    </ul>
                </div>
                <div class="col-sm-3" style="padding-top:4px;">
                    <asp:PlaceHolder ID="plhCheckColumn" runat="server"></asp:PlaceHolder>
                </div>
            </div>

        </div>
    </nav>

    <div class="container-fluid body-content repository">

        <div runat="server" id="rowtitle" class="col-sm-8">
            <asp:Label runat="server" ID="lblMnuTitle" Text="Repository title" />
            <asp:Label runat="server" CssClass="idsystem" prt-data="Id Name" ID="lblIdSystem" Text="Id System" />
        </div>
        <div runat="server" id="rowupdate" class="col-sm-4">
            <asp:Label runat="server" ID="lblInsertDate" />
            <asp:Label runat="server" ID="lblUpdateDate"/>
        </div>
        <asp:Panel runat="server" ID="pnlGridSystem">
            <asp:PlaceHolder ID="plhGridSystem" runat="server" />
            <br />
        </asp:Panel>
    </div>

    <!-- Modal HTML -->
    <div id="formModal" class="modal fade">
        <div class="modal-dialog">
            <div class="modal-content">
                <!-- Content will be loaded here from remote.aspx file -->
            </div>
        </div>
    </div>

    <!-- Modal HTML -->
    <div id="confirmationModal" class="modal fade">
        <div class="modal-dialog modal-md">
            <div class="modal-content">
                <div id="divConfirmHeader" runat="server" class="modal-header bg-primary">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">
                        <span id="confirmTitle" runat="server"></span>
                    </h4>
                </div>
                <div class="modal-body">
                    <p>
                        <span id="confirmMsg" runat="server"></span>                                .
                    </p>
                </div>
                <div class="modal-footer">
                    <button type="button" id="btnConfirmCancel" runat="server" class="btn btn-xs btn-cancel" data-dismiss="modal">Annuler</button>
                    <button type="button" id="btnConfirmSubmit" runat="server" class="btn btn-xs btn-record">Lancer le traitement</button>
                </div>
            </div>
        </div>
    </div>

    <div id="popovertemplate">
        <div class="popover">
            <div class="arrow"></div>
            <div class="popover-title">
                <span class="glyphicon glyphicon-remove"></span>
            </div>
            <div class="popover-content"></div>
        </div>
    </div>

    <div id="tipcontainer">

    </div>

</asp:Content>
