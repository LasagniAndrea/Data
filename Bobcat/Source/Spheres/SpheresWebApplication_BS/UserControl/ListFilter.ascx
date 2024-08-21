<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListFilter.ascx.cs" Inherits="EFS.ListControl.ListFilter" %>
<div class="panel">
    <div class="panel-heading">
        <label id="title">Filter</label>
        <%--<span id="lblIsEnabledLstWhere" class="Msg_Information">Cocher cette case pour appliquer le ou les filtres sur la consultation. Attention, bien que cette case soit décochée, s'il en existe, les critères obligatoires restent appliqués.</span>--%>
        <div class="btn-group-xs pull-right">
            <input id="chkIsEnabledLstWhere" runat="server" type="checkbox" class="chkCapture" checked="checked">
            <label for="chkIsEnabledLstWhere">Appliquer le(s) filtre(s) optionnel(s)</label>
            <button id="btnOkAndSave" runat="server" type="button" class="btn btn-xs btn-record" title="OkAndSave">
                <span class="glyphicon glyphicon-floppy-save"></span>
            </button>
            <button id="btnCancel" runat="server" type="button" class="btn btn-xs btn-cancel" title="Cancel">
                <span class="glyphicon glyphicon-remove"></span>
            </button>
            <button id="btnOk" runat="server" type="button" class="btn btn-xs btn-apply" title="Ok">
                <span class="glyphicon glyphicon-ok"></span>
            </button>
            <button id="btnReset" runat="server" type="button" class="btn btn-xs btn-apply" title="Reset">
                <span class="glyphicon glyphicon-erase"></span>
            </button>
        </div>
    </div>
    <div class="panel-body">
        <asp:PlaceHolder ID="plhFilter" runat="server"></asp:PlaceHolder>
    </div>
</div>
