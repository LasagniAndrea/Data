<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListDisplay.ascx.cs" Inherits="EFS.ListControl.ListDisplay" %>
<div class="panel">
    <div class="panel-heading">
        <label id="title">Display</label>
        <div class="btn-group-xs pull-right">
            <button id="btnOkAndSave" runat="server" type="button" class="btn btn-xs btn-record" title="OkAndSave">
                <span class="glyphicon glyphicon-floppy-save"></span>
            </button>
            <button id="btnCancel" runat="server" type="button" class="btn btn-xs btn-cancel" title="Cancel">
                <span class="glyphicon glyphicon-remove"></span>
            </button>
            <button id="btnOk" runat="server" type="button" class="btn btn-xs btn-apply" title="Ok">
                <span class="glyphicon glyphicon-ok"></span>
            </button>
        </div>
    </div>
    <div class="panel-body">
        <div class="col-sm-4">
            <span id="lblSelectedColumnList" runat="server" class="title" />
            <asp:PlaceHolder ID="plhColumnsDisplayed" runat="server" EnableViewState="true" />
        </div>
        <div class="col-sm-4">
            <span id="lblAvailableColumnList" runat="server" class="title" />
            <asp:PlaceHolder ID="plhColumnsAvailable" runat="server" EnableViewState="true" />
        </div>
        <div class="col-sm-4">
            <span id="lblSortGroupColumnList" runat="server" class="title" />
            <asp:PlaceHolder ID="plhColumnsSorted" runat="server" EnableViewState="true" />
        </div>
    </div>
</div>
