<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TrackerMain.ascx.cs" Inherits="EFS.TrackerControl.TrackerMain" %>
<ul class="trkgrid list-group col-sm-4">
    <li id="lst" runat="server" class="list-group-item">
        <div runat="server" class="panel-heading">
            <asp:Label runat="server" id="Title" EnableViewState="false"  />
            <asp:PlaceHolder ID="plhTrackerMainCounter" EnableViewState="false" runat="server"></asp:PlaceHolder>
        </div>
        <div class="panel-body panel-body-sm">
            <asp:PlaceHolder ID="trkMain" EnableViewState="false" runat="server"></asp:PlaceHolder>
        </div>
    </li>
</ul>

