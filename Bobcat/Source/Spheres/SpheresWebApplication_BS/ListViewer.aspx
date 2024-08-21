<%@ Page Title="" Language="C#" MasterPageFile="~/Spheres.master" AutoEventWireup="true" CodeBehind="ListViewer.aspx.cs" Inherits="EFS.Spheres.ListViewer" %>
<%@ Register Src="~/UserControl/ListFilter.ascx" TagPrefix="efs" TagName="ListFilter" %>
<%@ Register Src="~/UserControl/ListDisplay.ascx" TagPrefix="efs" TagName="ListDisplay" %>
<%@ Register Src="~/UserControl/ListModel.ascx" TagPrefix="efs" TagName="ListModel" %>
<%@ Register TagPrefix="efs" Namespace="EFS.Controls" Assembly="EFS.GridViewLibrary"%>

<asp:Content ID="lstContent" ContentPlaceHolderID="mc" runat="Server">


    <script src="Scripts/ListViewer.js" type="text/javascript" charset="utf-8"></script>
    <script src="Scripts/Validator.js" type="text/javascript" charset="utf-8"></script>
<%--    <script src="Scripts/ListModel.js" type="text/javascript" charset="utf-8"></script>--%>


    <input id="hidLstDisplay" type="hidden" runat="server" />
    <input id="hidLstSorted" type="hidden" runat="server" />
    <input id="hidLstFilter" type="hidden" runat="server" />

    <%--class="container-fluid body-content"--%>
    <div id="lstContainer" class="row body-content">
        <div runat="server" id="viewertitle" data-menu="input">
            <asp:Label runat="server" ID="lblMnuTitle" Text="Viewer title" />
            <asp:Label runat="server" ID="lblMnuSubTitle" Text="Viewer subTitle" />
        </div>
        <div class="panel panel-default">
            <div class="panel-heading">
                <div class="row">
                    <div class="col-sm-5">
                        <button id="btnRefresh" runat="server" type="button" class="btn btn-xs btn-apply" title="Refresh"><span class="glyphicon glyphicon-refresh"></span></button>
                        <button id="btnAutorefresh" runat="server" type="button" class="btn btn-xs btn-apply" title="Refresh" onclick="OnAutoRefresh"><span class="glyphicon glyphicon-time"></span></button>

                        <button id="btnMSExcel" runat="server" type="button" class="btn btn-xs btn-apply" onserverclick="OnToolbar_Click"><span class="fa fa-file-excel-o"></span></button>
                        <button id="btnZip"  runat="server" type="button" class="btn btn-xs btn-apply"><span class="glyphicon glyphicon-compressed"></span></button>
                        <button id="btnSql" runat="server" type="button" class="btn btn-xs btn-apply"><span class="fa fa-database"></span></button>

                        <asp:Button ID="btnRunProcess" CssClass="btn btn-xs btn-process" runat="server" Text="Button"></asp:Button>
                        <asp:Button ID="btnRunProcessIO" CssClass="btn btn-xs btn-process" runat="server" Text="Button"></asp:Button>
                        <asp:Button ID="btnRunProcessAndIO" CssClass="btn btn-xs btn-process" runat="server" Text="Button"></asp:Button>

                        <asp:Button ID="btnAddNew" CssClass="btn btn-xs btn-new" runat="server" Text="Button"></asp:Button>
                        <asp:Button ID="btnRecord" CssClass="btn btn-xs btn-record" runat="server" Text="Button"></asp:Button>
                        <asp:Button ID="btnCancel" CssClass="btn btn-xs btn-cancel" runat="server" Text="Button"></asp:Button>
                    </div>
                    <div class="col-sm-6">
                        <asp:PlaceHolder ID="plhCheckSelected" runat="server"></asp:PlaceHolder>
                    </div>
                    <div class="col-sm-1">
                        <div class="btn-group-xs pull-right">
                            <button id="btnOpenSaveModel" runat="server" onclick="return false;" class="btn btn-xs btn-apply" data-toggle="modal" 
                                data-target="#formModal" data-backdrop="static"><span class="glyphicon glyphicon-folder-close"></span></button>
                            <button id="btnDisplay" runat="server" type="button" class="btn btn-xs btn-apply" title="Display Sort and Group by"><span class="glyphicon glyphicon-th"></span></button>
                            <button id="btnFilter" type="button" class="btn btn-xs btn-apply" title="Criteria"><span class="glyphicon glyphicon-filter"></span></button>
                        </div>
                    </div>
                </div>
            </div>
            <div id="lstBody" class="panel-body panel-body-sm">

                <asp:Panel ID="pnlMsgSummary" runat="server">
                    <asp:Literal ID="litMsg" runat="server" />
                </asp:Panel>

                <div id="lstInfo" runat="server" class="col-sm-12">

                    <%-- Container READONLY : CustomObjects --%>
                    <div id="lstCustomObjects" runat="server" class="form-inline col-sm-12">
                        <div class="panel">
                            <div id="lstCustomObjectsTitle" runat="server" class="panel-heading">Characteristics</div>
                            <div class="panel-body">
                                <asp:PlaceHolder ID="plhCustomObject" runat="server"></asp:PlaceHolder>
                            </div>
                        </div>
                    </div>

                    <%-- Container READONLY : Critères de filtre --%>
                    <div id="lstFilterReadOnly" runat="server" class="col-sm-12">
                        <div class="panel">
                            <div id="lstFilterReadOnlyTitle" runat="server" class="panel-heading" />
                            <div class="panel-body">
                                <asp:PlaceHolder ID="plhFilterReadOnly" runat="server"></asp:PlaceHolder>
                            </div>
                        </div>
                    </div>

                    <%-- Container EDIT : Sélection des colonnes d'affichage --%>
                    <div id="lstCriteria_Display"  runat="server" class="col-sm-12 invisible">
                        <efs:ListDisplay ID="uc_lstdisplay" runat="server" />
                    </div>

                    <%-- Container EDIT : Critères de filtre --%>
                    <div id="lstCriteria_Filter"  runat="server" class="col-sm-12 invisible">
                        <efs:ListFilter ID="uc_lstfilter" runat="server" />
                    </div>

                </div>

                <%-- Container du GridView --%>
                <div id="gvContent">
                    <efs:GridViewTemplate ID="gvTemplate" runat="server" AutoGenerateColumns="false" />
                </div>

            </div>
            <div id="lstFooter" runat="server" class="panel-footer panel-footer-sm">
                <div class="row">
                    <div class="col-sm-7">
                        <%-- DataPager lié au GridView --%>
                        <efs:UnorderedListDataPager ID="gvPager" runat="server" class="pagination pagination-xs" PagedControlID="gvTemplate" />

                    </div>
                    <div class="col-sm-2 col-sm-offset-3">
                        <div class="input-group">
                            <span class="input-group-addon input-xs" title="Go to page"><i class="glyphicon glyphicon-arrow-right"></i></span>
                            <asp:TextBox ID="txtGoToPage" runat="server" AutoPostBack="true" CssClass="form-control input-xs" ToolTip="Go to page" />
                            <span class="input-group-addon input-xs" title="Rows per page"><i class="glyphicon glyphicon-th-list"></i></span>
                            <asp:TextBox ID="txtRowsPerPage" runat="server" Text="10" AutoPostBack="true" CssClass="form-control input-xs" ToolTip="Rows per page" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>



    <!-- Modal HTML -->
    <div id="formModal" class="modal fade">
        <div class="modal-dialog">
            <div class="modal-content">
                <!-- Content will be loaded here from remote.aspx file -->
                <efs:ListModel ID="uc_lstmodel" runat="server" />
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

    <div id="tipcontainer">

    </div>

</asp:Content>

