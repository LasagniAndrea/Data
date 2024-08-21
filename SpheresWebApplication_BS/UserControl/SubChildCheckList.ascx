<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SubChildCheckList.ascx.cs" Inherits="EFS.TrackerControl.SubChildCheckList" %>
<div class="panel-group" id="subContainer" runat="server" >
    <div class="panel panel-primary">
        <div class="panel-heading">
            <asp:Label runat="server" ID="title" ></asp:Label>
            <input runat="server" id="check" type="checkbox" />
        </div>
        <div class="panel-body">
            <asp:PlaceHolder ID="plhListItem" EnableViewState="false" runat="server"></asp:PlaceHolder>
        </div>
    </div>
</div>
