<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChildCheckList.ascx.cs" Inherits="EFS.TrackerControl.ChildCheckList" %>
<%@ Register Src="~/UserControl/SubChildCheckList.ascx" TagPrefix="efs" TagName="SubChildCheckList" %>
<div id="container" runat="server">
    <div class="input-group popover-markup">
        <span runat="server" id="help" class="input-group-addon process trigger"><span id="icon" runat="server"></span></span>
        <input type="text" runat="server" class="form-control" id="title" readonly aria-label="..." />
        <span class="input-group-addon">
            <input runat="server" id="mainCheck" type="checkbox" aria-label="..." />
        </span>
    </div>
    <div class="checkList">
        <asp:CheckBoxList ID="chkChildList" RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Left" OnSelectedIndexChanged="OnChildChange" runat="server" />
        <asp:PlaceHolder ID="plhListItem" EnableViewState="false" runat="server"></asp:PlaceHolder>
    </div>
</div>
