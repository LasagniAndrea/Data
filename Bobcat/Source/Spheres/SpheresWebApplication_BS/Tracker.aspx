<%@ Page Title="" Language="C#" MasterPageFile="~/Spheres.Master" AutoEventWireup="true" CodeBehind="Tracker.aspx.cs" Inherits="EFS.Spheres.Tracker" %>
<%@ Import Namespace="EFS.ACommon" %>

<%@ Register Src="~/UserControl/TrackerMain.ascx" TagPrefix="efs" TagName="TrackerMain" %>
<%@ Register Src="~/UserControl/TrackerItem.ascx" TagPrefix="efs" TagName="TrackerItem" %>
<%@ Register Src="~/UserControl/MainCheckList.ascx" TagPrefix="efs" TagName="MainCheckList" %>
<%@ Register Src="~/UserControl/ChildCheckList.ascx" TagPrefix="efs" TagName="ChildCheckList" %>





<asp:Content ID="trackerContent" ContentPlaceHolderID="mc" runat="Server">
    <div id="trkContainer" class="container body-content">
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-heading">
                <asp:Timer ID="timerRefresh" runat="server" OnTick="Refresh"></asp:Timer>

                <a id="btnRefresh" runat="server" href="#" class="btn btn-xs btn-apply" title="Refresh"><span class="glyphicon glyphicon-refresh"></span></a>
                <a id="btnAutorefresh" runat="server" href="#" class="btn btn-xs btn-apply" title="AutoRefresh"><span class="glyphicon glyphicon-time"></span></a>
                <a id="btnParam" runat="server" href="#" class="btn btn-xs btn-apply" title="Settings tracker" data-toggle="modal" data-target="#trackerParam"><span class="glyphicon glyphicon-wrench"></span></a>
                <a id="collapse-init" href="#" class="btn btn-xs btn-apply" title="Toggle"><span class="glyphicon glyphicon-collapse-down"></span></a>
                <a id="container-init" href="#" class="btn btn-xs btn-apply" title="Container"><span class="glyphicon glyphicon-fullscreen"></span></a>
                <div class="pull-right">
                    <div id="rbDisplayMode" class="sph-radio btn-group" data-toggle="buttons">
                        <asp:RadioButton GroupName="displayMode" ID="rb_readyState" runat="server" CssClass="btn btn-xs btn-apply" AutoPostBack="true" OnCheckedChanged="DisplayTracker" Text="ReadyState" />
                        <asp:RadioButton GroupName="displayMode" ID="rb_groupTracker" runat="server" CssClass="btn btn-xs btn-apply"  AutoPostBack="true" OnCheckedChanged="DisplayTracker" Text="GroupTracker" />
                        <asp:RadioButton GroupName="displayMode" ID="rb_statusTracker" runat="server" CssClass="btn btn-xs btn-apply" AutoPostBack="true" OnCheckedChanged="DisplayTracker" Text="StatusTracker" />
                    </div>

                    <div class="btn-group">
                        <button id="lblModel" type="button" class="btn btn-xs btn-apply">Model 1</button>
                        <button type="button" class="btn btn-xs btn-apply dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            <span class="caret"></span>
                            <span class="sr-only">Toggle Dropdown</span>
                        </button>
                        <ul class="dropdown-menu">
                            <li><a id="grid-model1-init" href="#">Model 1</a></li>
                            <li><a id="grid-model2-init" href="#">Model 2</a></li>
                            <li><a id="grid-model3-init" href="#">Model 3</a></li>
                        </ul>
                    </div>
                </div>
            </div>
            <div id="trkBody" class="panel-body panel-body-sm">
                <div class="col-sm-12">
                    <div class="form-group form-group-xs">
                        <asp:PlaceHolder ID="plhTrackerContent" runat="server"></asp:PlaceHolder>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- Modal HTML -->
    <div id="trackerModal" class="modal fade">
        <div class="modal-dialog modal-md">
            <div class="modal-content">
                <!-- Content will be loaded here from remote.aspx file -->
            </div>
        </div>
    </div>

    <div id="trackerParam" class="modal animated fadeInRight printable" tabindex="-1" aria-labelledby="trackerParam" role="dialog" data-backdrop="static" data-keyboard="false" style="display: none;">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div>
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                    </div>
                    <h3 id="H1" runat="server" class="modal-title">Tracker : Paramètres utilisateur </h3>

                </div>
                <div class="modal-body">
                    <div class="panel sph-nav-tabs panel-primary">
                        <div class="panel-heading">
                            <ul class="nav nav-tabs">
                                <li class="active"><a href="#main" data-toggle="tab">Généralités</a></li>
                                <li><a href="#userInterface" data-toggle="tab">Interface utilisateur</a></li>
                                <li><a href="#processNotify" data-toggle="tab">Notification de traitement</a></li>
                                <li><a href="#trackerHelp" data-toggle="tab">Aide</a></li>
                            </ul>
                        </div>
                        <div class="panel-body">
                            <div id="trkTab" class="tab-content">
                                <div class="tab-pane fade in active" id="main">
                                    <div class="form-group">
                                        <asp:Label ID="lblTrackerRefresh" AssociatedControlID="txtTrackerRefreshInterval" CssClass="col-sm-3 control-label" runat="server">Refresh interval (in seconds)</asp:Label>
                                        <div class="col-sm-2 input-group input-group-sm">
                                            <span class="input-group-addon"><span class="glyphicon glyphicon-time"></span></span>
                                            <asp:TextBox ID="txtTrackerRefreshInterval" runat="server" CssClass="form-control"></asp:TextBox>
                                            <div class="input-group-addon">sec.</div>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <asp:Label ID="lblTrackerNbRow" AssociatedControlID="txtTrackerNbRowPerGroup" CssClass="col-sm-3 control-label" runat="server">Number of rows displayed per group</asp:Label>
                                        <div class="col-sm-2 input-group input-group-sm">
                                            <asp:TextBox ID="txtTrackerNbRowPerGroup" runat="server" CssClass="form-control"></asp:TextBox>
                                            <div id="lblPerGroup" runat="server" class="input-group-addon">per group.</div>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <asp:Label ID="lblTrackerHistoric" AssociatedControlID="chkHistoToDay" CssClass="col-sm-3 control-label" runat="server">TrackerHistoric</asp:Label>

                                        <div id="rbHistoric" class="sph-radio btn-group" data-toggle="buttons">
                                            <asp:RadioButton GroupName="historic" ID="chkHistoToDay" runat="server" CssClass="btn btn-default btn-sm" Text="TrackerToDay" />
                                            <asp:RadioButton GroupName="historic" ID="chkHisto1D" runat="server" CssClass="btn btn-default btn-sm"  Text="Tracker1D" />
                                            <asp:RadioButton GroupName="historic" ID="chkHisto7D" runat="server" CssClass="btn btn-default btn-sm"  Text="Tracker7D" />
                                            <asp:RadioButton GroupName="historic" ID="chkHisto1M" runat="server" CssClass="btn btn-default btn-sm"  Text="Tracker1M" />
                                            <asp:RadioButton GroupName="historic" ID="chkHisto3M" runat="server" CssClass="btn btn-default btn-sm"  Text="Tracker3M" />
                                            <asp:RadioButton GroupName="historic" ID="chkHistoBeyond" runat="server" CssClass="btn btn-default btn-sm" Text="TrackerBeyond" />
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <div class="col-sm-offset-3">
                                            <asp:CheckBox ID="chkTrackerAlert" runat="server" />
                                            <asp:Label ID="lblTrackerAlert" AssociatedControlID="chkTrackerAlert" CssClass="control-label" runat="server">Notification at the end of each processing request</asp:Label>
                                        </div>
                                    </div>
                                </div>
                                <div class="tab-pane fade " id="userInterface">
                                    <div class="form-group">
                                        <asp:Label ID="trkDisplayMode" AssociatedControlID="rb_readyState" CssClass="control-label" runat="server">Display mode</asp:Label>
                                        <div id="rbDisplayModeParam" class="sph-radio btn-group" data-toggle="buttons">
                                            <asp:RadioButton GroupName="displayModeParam" ID="rb_readyStateParam" runat="server" CssClass="btn btn-xs btn-apply" Text="ReadyState" />
                                            <asp:RadioButton GroupName="displayModeParam" ID="rb_groupTrackerParam" runat="server" CssClass="btn btn-xs btn-apply"  Text="GroupTracker" />
                                            <asp:RadioButton GroupName="displayModeParam" ID="rb_statusTrackerParam" runat="server" CssClass="btn btn-xs btn-apply" Text="StatusTracker" />
                                        </div>
                                        <span class="pull-right">
                                        <asp:CheckBox ID="chkSelectAll2" runat="server" Text="Select all" TextAlign="Left"/>
                                        </span>
                                    </div>
                                    <div id="uitrk_car" class="carousel slide" data-ride="carousel" data-interval="0">
                                        <!-- Indicators -->
                                        <ol class="carousel-indicators">
                                            <li data-target="#uitrk_car" data-slide-to="0" class="active"></li>
                                            <li data-target="#uitrk_car" data-slide-to="1"></li>
                                            <li data-target="#uitrk_car" data-slide-to="2"></li>
                                            <li data-target="#uitrk_car" data-slide-to="3"></li>
                                            <li data-target="#uitrk_car" data-slide-to="4"></li>
                                            <li data-target="#uitrk_car" data-slide-to="5"></li>
                                            <li data-target="#uitrk_car" data-slide-to="6"></li>
                                            <li data-target="#uitrk_car" data-slide-to="7"></li>
                                            <li data-target="#uitrk_car" data-slide-to="8"></li>
                                            <li data-target="#uitrk_car" data-slide-to="9"></li>
                                        </ol>
                                        <!-- Wrapper for slides -->
                                        <div class="carousel-inner" role="listbox">
                                            <!-- Controls -->
                                            <div class="item active">
                                                <div class="ucMainChecklist readystate">
                                                    <asp:PlaceHolder ID="plhReadyState1" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>
                                            <div class="item">
                                                <div class="ucMainChecklist readystate">
                                                    <asp:PlaceHolder ID="plhReadyState2" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>
                                            <div class="item">
                                                <div class="ucMainChecklist readystate">
                                                    <asp:PlaceHolder ID="plhReadyState3" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>

                                            <div class="item">
                                                <div class="ucMainChecklist groupTracker">
                                                    <asp:PlaceHolder ID="plhGroupTracker1" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>
                                            <div class="item">
                                                <div class="ucMainChecklist groupTracker">
                                                    <asp:PlaceHolder ID="plhGroupTracker2" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>
                                            <div class="item">
                                                <div class="ucMainChecklist groupTracker">
                                                    <asp:PlaceHolder ID="plhGroupTracker3" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>

                                            <div class="item">
                                                <div class="ucMainChecklist statusTracker">
                                                    <asp:PlaceHolder ID="plhStatusTracker1" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>
                                            <div class="item">
                                                <div class="ucMainChecklist statusTracker">
                                                    <asp:PlaceHolder ID="plhStatusTracker2" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>
                                            <div class="item">
                                                <div class="ucMainChecklist statusTracker">
                                                    <asp:PlaceHolder ID="plhStatusTracker3" EnableViewState="false"  runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>
                                            <div class="item">
                                                <div class="ucMainChecklist statusTracker">
                                                    <asp:PlaceHolder ID="plhStatusTracker4" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                                </div>
                                            </div>
                                        </div>
                                        <a class="left carousel-control" href="#uitrk_car" role="button" data-slide="prev">
                                            <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>
                                            <span class="sr-only">Previous</span>
                                        </a>
                                        <a class="right carousel-control" href="#uitrk_car" role="button" data-slide="next">
                                            <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>
                                            <span class="sr-only">Next</span>
                                        </a>
                                    </div>
                                </div>
                                <div class="tab-pane fade " id="processNotify">
                                    <div class="ucMainChecklist service">
                                        <efs:MainCheckList runat="server" CheckListType="Service" ID="service" IsWithCheckAll="true" />
                                    </div>
                                </div>
                                <div class="tab-pane fade " id="trackerHelp">
                                    <div id="uitrkhelp-car" class="carousel slide" data-ride="carousel" data-interval="0">
                                        <!-- Indicators -->
                                        <ol class="carousel-indicators">
                                            <li data-target="#uitrkhelp-car" data-slide-to="0" class="active"></li>
                                            <li data-target="#uitrkhelp-car" data-slide-to="1"></li>
                                        </ol>
                                        <!-- Wrapper for slides -->
                                        <div class="carousel-inner" role="listbox">
                                            <!-- Controls -->
                                            <div class="item active">
                                                <asp:PlaceHolder ID="plhTrackerHelp1" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                            </div>
                                            <div class="item">
                                                <asp:PlaceHolder ID="plhTrackerHelp2" EnableViewState="false" runat="server"></asp:PlaceHolder>
                                            </div>
                                        </div>
                                        <a class="left carousel-control" href="#uitrkhelp-car" role="button" data-slide="prev">
                                            <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>
                                            <span class="sr-only">Previous</span>
                                        </a>
                                        <a class="right carousel-control" href="#uitrkhelp-car" role="button" data-slide="next">
                                            <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>
                                            <span class="sr-only">Next</span>
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnOk" runat="server" CssClass="btn btn-xs btn-record active" OnClick="OnValidation" UseSubmitBehavior="false" data-dismiss="modal" Text="Ok" />
                        <button type="button" class="btn btn-xs btn-cancel" data-dismiss="modal">Close</button>
                        <button type="button" class="btn btn-xs btn-default" onclick="window.print();">Print</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

