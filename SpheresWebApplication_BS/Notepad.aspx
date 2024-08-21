<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Notepad.aspx.cs" Inherits="EFS.Spheres.Notepad" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
</head>
<body>
    <form id="frmNotePad" runat="server">
        <input id="hidTxtLoNote" type="hidden" runat="server" />
        <div class="modal-header">
            <h3 id="lblTitleNotePad" runat="server" class="modal-title">Bloc-notes</h3>
        </div>
        <div class="modal-body">
            <div class="col-sm-7">
                <asp:Label runat="server" ID="lblTitle" Text="Source title" />
            </div>
            <div class="col-sm-5">
                <div class="input-group input-group-sm">
                    <input id="txtSearch" runat="server" type="text" class="form-control" placeholder="Search for..."/>
                    <span class="input-group-btn">
                        <button id="btnSearch" onclick="Search('txtLoNote','txtSearch');return false;" class="btn btn-apply" type="button">
                            <span class="glyphicon glyphicon-zoom-in"></span></button>
                    </span>
                </div>
            </div>
            <asp:PlaceHolder ID="plhNotepad" runat="server" />
            <div id="txtLoNote" contenteditable="true" runat="server" class="col-sm-12 lonote" enableviewstate="true" />

        </div>
        <div class="modal-footer">
            <asp:Label runat="server" ID="lblLastAction" Text="NotePad actionTitle" />
            <button id="btnRecord" runat="server" class="btn btn-xs btn-record active" onclick="SetLonote('txtLoNote','hidTxtLoNote');" onserverclick="OnRecord">Ok</button>
            <button id="btnCancel" runat="server" class="btn btn-xs btn-cancel" onserverclick="OnCancel">Close</button>
        </div>
    </form>
</body>
</html>
