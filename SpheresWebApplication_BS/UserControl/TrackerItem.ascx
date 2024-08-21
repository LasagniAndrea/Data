<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TrackerItem.ascx.cs" Inherits="EFS.TrackerControl.TrackerItem" %>
<div class="trkgrid col-sm-12">
    <div class="panel panel-default">
        <div runat="server" class="panel-heading" role="tab" id="Heading">
            <asp:HyperLink runat="server" CssClass="accordion-toggle" data-toggle="collapse" data-parent="#accordion" href="#Collapse" ID="Title" 
                aria-expanded="false" aria-controls="collapse" />
            <asp:PlaceHolder ID="plhTrackerItemCounter" EnableViewState="false" runat="server"></asp:PlaceHolder>
        </div>
        <div id="Collapse" runat="server" class="panel-collapse collapse" role="tabpanel" aria-labelledby="Heading" enableviewstate="false">
<%--                            <ul class="list-group small">
                <li class="list-group-item">1. item</li>
                <li class="list-group-item">2. item</li>
                <li class="list-group-item">3. item</li>
                <li class="list-group-item">4. item</li>
                <li class="list-group-item">5. item</li>
                <li class="list-group-item">6. item</li>
                <li class="list-group-item">7. item</li>
                <li class="list-group-item">8. item</li>
            </ul>
--%>
        </div>
    </div>
</div>