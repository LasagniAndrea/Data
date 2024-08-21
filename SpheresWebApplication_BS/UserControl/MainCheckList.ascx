<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainCheckList.ascx.cs" Inherits="EFS.TrackerControl.MainCheckList" %>
<%@ Register Src="~/UserControl/ChildCheckList.ascx" TagPrefix="efs" TagName="ChildCheckList" %>

<div id="ucStart" runat="server" class="form-group col-xs-12">
    <span id="ucTitle" runat="server">TITRE</span>
    <span class="pull-right">
    <asp:CheckBox ID="chkSelectAll" runat="server" Text="Select all" TextAlign="Left"/>
    </span>
</div>
<asp:PlaceHolder ID="mainContent" EnableViewState="false" runat="server"></asp:PlaceHolder>
