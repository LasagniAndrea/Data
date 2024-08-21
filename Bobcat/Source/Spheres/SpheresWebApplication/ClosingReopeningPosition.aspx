<%@ Page Language="C#" AutoEventWireup="true" Inherits="EFS.Spheres.ClosingReopeningPositionPage" CodeBehind="ClosingReopeningPosition.aspx.cs" %>

<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title id="titlePage" runat="server"></title>
</head>
<body class="EG-Sommaire" runat="server">
    <form id="frmClosingReopeningPosition" method="post" runat="server">
        <asp:PlaceHolder ID="plhHeader" runat="server"></asp:PlaceHolder>
        <asp:ScriptManager ID="ScriptManager" runat="server" />

        <div id="divalltoolbar">
            <asp:Panel ID="tblist" runat="server">
                <div>
                    <efsc:WCToolTipLinkButton ID="btnRefresh" CssClass="fa-icon" runat="server" OnCommand="OnAction" />
                    <efsc:WCToolTipLinkButton ID="btnRecord" CssClass="fa-icon" runat="server" OnCommand="OnAction" />
                    <efsc:WCToolTipLinkButton ID="btnCancel" CssClass="fa-icon" runat="server" OnCommand="OnAction" />
                    <efsc:WCToolTipLinkButton ID="btnRemove" CssClass="fa-icon" runat="server" OnCommand="OnAction" />
                    <efsc:WCToolTipLinkButton ID="btnDuplicate" CssClass="fa-icon" runat="server" OnCommand="OnAction" />
                    <efsc:WCToolTipLinkButton ID="btnAttachedResults" runat="server" CssClass="fa-icon" />
                    <efsc:WCToolTipLinkButton ID="btnSend" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
                    <efsc:WCToolTipLinkButton ID="btnSeeMsg" CssClass="fa-icon" runat="server" OnCommand="OnAction" />
                </div>
            </asp:Panel>
        </div>

        <%-- Action request characteristics --%>
        <asp:Panel id="divbody" runat="server">
            <asp:Panel runat="server" ID="pnlCharacteristicsGen" Style="margin: 2px; height: 96%;">
                <div class="headh">
                    <efsc:WCTooltipLabel runat="server" CssClass="size3" ID="lblCharacteristics" />
                    <span id="CRPState" style="float: left;"></span>
                </div>
                <div class="contenth">
                    <efsc:WCTooltipLabel runat="server" ID="lblIDARQ" />
                    <table class="main">
                        <tr>
                            <td>
                                <efsc:WCTooltipLabel runat="server" ID="lblReadyState" Width="40px" />
                                <efsc:WCDropDownList2 runat="server" ID="ddlReadyState" Width="100px" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <efsc:WCTooltipLabel runat="server" ID="lblIdentifier" Width="100px" />
                                <efsc:WCTextBox runat="server" ID="txtIdentifier" Width="520px" onblur="Copy(this,'txtDisplayName','new');" />
                                <efsc:WCTooltipLabel runat="server" ID="roIdentifier" Width="520px" Visible="false" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <efsc:WCTooltipLabel runat="server" ID="lblDisplayName" Width="100px" />
                                <efsc:WCTextBox runat="server" ID="txtDisplayName" Width="520px" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <efsc:WCTooltipLabel runat="server" ID="lblDescription" Width="100px" Style="vertical-align: top;" />
                                <efsc:WCTextBox runat="server" ID="txtDescription" Height="50px" Width="520px" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <efsc:WCTooltipLabel runat="server" ID="lblRequestType" Width="100px" />
                                <efsc:WCDropDownList2 runat="server" ID="ddlRequestType" OnSelectedIndexChanged="OnRequestTypeChanged" AutoPostBack="true" Width="180px" />
                                <efsc:WCToolTipPanel runat="server" ID="ttipRequestType" />
                                <efsc:WCTooltipLabel runat="server" ID="roRequestType" Width="180px" Visible="false" />
                                <asp:CustomValidator ID="ddlRequestTypeValidator" runat="server" OnServerValidate="ControlToValidate"
                                    ErrorMessage="*" ControlToValidate="ddlRequestType" ValidationGroup="MAIN" ValidateEmptyText="true" Display="None" />

                                <efsc:WCTooltipLabel runat="server" ID="lblTiming" Width="35px" />
                                <efsc:WCDropDownList2 runat="server" ID="ddlTiming" Width="150px" />
                                <efsc:WCToolTipPanel runat="server" ID="ttipTiming" />
                                <asp:CustomValidator ID="ddlTimingValidator" runat="server" OnServerValidate="ControlToValidate"
                                    ErrorMessage="*" ControlToValidate="ddlTiming" ValidationGroup="MAIN" ValidateEmptyText="true" Display="None" />

                            </td>
                        </tr>
                        <tr>
                            <td>
                                <efsc:WCTooltipLabel runat="server" ID="lblEffectiveDate" Width="100px" />
                                <efsc:WCTextBox runat="server" ID="txtEffectiveDate" Width="80px"></efsc:WCTextBox>
                                <asp:CustomValidator ID="txtEffectiveDateValidator" runat="server" OnServerValidate="ControlToValidate"
                                    ErrorMessage="*" ControlToValidate="txtEffectiveDate" ValidationGroup="MAIN" ValidateEmptyText="true" Display="None" />
                                <efsc:WCTooltipLabel runat="server" ID="lblEffectiveEndDate" Width="60px" />
                                <efsc:WCTextBox runat="server" ID="txtEffectiveEndDate" Width="80px"></efsc:WCTextBox>
                                <asp:CustomValidator ID="txtEffectiveEndDateValidator" runat="server" OnServerValidate="ControlToValidate"
                                    ErrorMessage="*" ControlToValidate="txtEffectiveEndDate" ValidationGroup="MAIN" ValidateEmptyText="true" Display="None" />
                            </td>
                        </tr>
                    </table>
                    <table>
                        <tr>
                            <td>
                                <asp:ValidationSummary ID="validGen" runat="server" DisplayMode="List" ShowSummary="true" Visible="true" />
                            </td>
                        </tr>
                    </table>
                </div>
            </asp:Panel>

            <%--Instructions pour environnement Instrumental--%>

            <asp:Panel runat="server" ID="pnlInstrumentalEnvironnment" Style="margin: 2px; height: 96%;">
                <div class="headh">
                    <efsc:WCTooltipLabel runat="server" CssClass="size3"  ID="lblInstrumentalEnvironnment" />
                </div>
                <div class="contenth">
                    <table class="main">
                        <tr>
                            <td>
                                <efsc:WCTooltipLabel runat="server" ID="lblTypeInstr" Width="100px" />
                                <efsc:WCDropDownList2 runat="server" ID="ddlTypeInstr" AutoPostBack="true" OnSelectedIndexChanged="OnGProductTypeInstrChanged" Width="160px" />
                                <efsc:WCTooltipLabel runat="server" ID="lblIDInstr" Width="90px" />
                                <efsc:WCDropDownList2 runat="server" ID="ddlIDInstr" Width="268px" />
                                <asp:CustomValidator ID="ddlIDInstrValidator" runat="server" OnServerValidate="ControlToValidate"
                                    ErrorMessage="*" ControlToValidate="ddlIDInstr" ValidationGroup="PNL1" ValidateEmptyText="true" Display="None" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <efsc:WCTooltipLabel runat="server" ID="lblTypeContract" Width="100px" />
                                <efsc:WCDropDownList2 runat="server" ID="ddlTypeContract" AutoPostBack="true" OnSelectedIndexChanged="OnTypeContractChanged" Width="160px" />
                                <efsc:WCTooltipLabel runat="server" ID="lblIDContract" Width="90px" />
                                <efsc:WCTextBox ID="txtIDContract" runat="server" CssClass="crp-autocomplete txtCapture" Width="268px"></efsc:WCTextBox>
                                <asp:CustomValidator ID="ddlIDContractValidator" runat="server" OnServerValidate="ControlToValidate"
                                    ErrorMessage="*" ControlToValidate="txtIDContract" ValidationGroup="PNL1" ValidateEmptyText="true" Display="None" />
                            </td>
                        </tr>
                    </table>
                    <table>
                        <tr>
                            <td>
                                <asp:ValidationSummary ID="validInstr" runat="server" DisplayMode="List" ValidationGroup="PNL1" />
                            </td>
                        </tr>
                    </table>
                </div>
            </asp:Panel>

            <%--Instructions de positions--%>

            <asp:Panel runat="server" ID="pnlClosingReopeningPositions" Style="margin: 2px; height: 96%;">
                <div class="headh">
                    <efsc:WCTooltipLabel runat="server" CssClass="size3" ID="lblClosingReopeningPositions" />
                    <efsc:WCTooltipLabel runat="server" ID="lblEntity" />
                    <efsc:WCDropDownList2 runat="server" ID="ddlEntity" AutoPostBack="true" Width="160px" />
                </div>
                <div class="contenth">
                    <table id="tblClosingPositions" class="main">
                        <tr>
                            <td class="arqfilter">
                                <efsc:WCTooltipLabel runat="server" ID="lblARQFilter" Width="240px" />
                                <efsc:WCDropDownList2 runat="server" ID="ddlARQFilter" Width="300px" />
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align: top;">
                                <%--Fermeture de positions--%>
                                <h3 runat="server">
                                    <efsc:WCTooltipLabel runat="server" ID="lblClosingPositions" />
                                </h3>
                                <div style="height: 100%">
                                    <table style="width: 100%">
                                        <tr>
                                            <td>
                                                <asp:ValidationSummary ID="validClosingPositions" runat="server" DisplayMode="List" ValidationGroup="PNL2" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblTypeDealer_C" Width="135px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlTypeDealer_C" AutoPostBack="true" OnSelectedIndexChanged="OnTypeDealerClearerCssCustodianChanged" Width="160px" />
                                                <asp:CustomValidator ID="ddlTypeDealer_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlTypeDealer_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />

                                                <efsc:WCTooltipLabel runat="server" ID="lblIDDealer_C" Width="135px" />
                                                <efsc:WCTextBox ID="txtIDDealer_C" runat="server" CssClass="crp-autocomplete txtCapture" Width="260px"></efsc:WCTextBox>
                                                <asp:CustomValidator ID="ddlIDDealer_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="txtIDDealer_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />
                                                <efsc:WCToolTipPanel runat="server" ID="ttipIDDealer_C" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblTypeClearer_C" Width="135px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlTypeClearer_C" AutoPostBack="true" OnSelectedIndexChanged="OnTypeDealerClearerCssCustodianChanged" Width="160px" />
                                                <asp:CustomValidator ID="ddlTypeClearer_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlTypeClearer_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />

                                                <efsc:WCTooltipLabel runat="server" ID="lblIDClearer_C" Width="135px" />
                                                <efsc:WCTextBox ID="txtIDClearer_C" runat="server" CssClass="crp-autocomplete txtCapture" Width="260px"></efsc:WCTextBox>
                                                <asp:CustomValidator ID="ddlIDClearer_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="txtIDClearer_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />
                                                <efsc:WCToolTipPanel runat="server" ID="ttipIDClearer_C" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblTypeCssCustodian_C" Width="135px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlTypeCssCustodian_C" AutoPostBack="true" OnSelectedIndexChanged="OnTypeDealerClearerCssCustodianChanged" Width="160px" />
                                                <asp:CustomValidator ID="ddlTypeCssCustodian_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlTypeCssCustodian_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />

                                                <efsc:WCTooltipLabel runat="server" ID="lblIDCssCustodian_C" Width="135px" />
                                                <efsc:WCTextBox ID="txtIDCssCustodian_C" runat="server" CssClass="crp-autocomplete txtCapture" Width="260px"></efsc:WCTextBox>
                                                <asp:CustomValidator ID="ddlIDCssCustodian_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="txtIDCssCustodian_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />
                                                <efsc:WCToolTipPanel runat="server" ID="ttipIDCssCustodian_C" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <hr />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblMode_C" Width="100px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlMode_C" AutoPostBack="true" OnSelectedIndexChanged="OnMethodClosingChanged" Width="160px" />
                                                <efsc:WCToolTipPanel runat="server" ID="ttipMode_C" />
                                                <efsc:WCCheckBox2 runat="server" ID="chkIsSumClosingAmount" />
                                                <efsc:WCCheckBox2 runat="server" ID="chkIsDelisting" />
                                                <asp:CustomValidator ID="ddlMode_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlMode_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />

                                            </td>
                                        </tr>
                                        <tr class="lblprice">
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblEQTYPrice_C" Width="160px" />
                                                <efsc:WCTooltipLabel runat="server" ID="lblFUTPrice_C" Width="140px" />
                                                <efsc:WCTooltipLabel runat="server" ID="lblOtherPrice_C" Width="140px" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>

                                                <efsc:WCTooltipLabel runat="server" ID="lblChoicePrice_C" Width="100px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlEQTYPrice_C" Width="160px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlFUTPrice_C" Width="140px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlOtherPrice_C" Width="140px" />
                                                <efsc:WCToolTipPanel runat="server" ID="ttipOtherPrice_C" />

                                                <asp:CustomValidator ID="ddlEQTYPrice_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlEQTYPrice_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />
                                                <asp:CustomValidator ID="ddlFUTPrice_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlFUTPrice_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />
                                                <asp:CustomValidator ID="ddlOtherPrice_CValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlOtherPrice_C" ValidationGroup="PNL2" ValidateEmptyText="true" Display="None" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <hr />

                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblFeeAction_C" Width="100px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlFeeAction_C" Width="303px" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>

                    <table id="tblReopeningPositions" runat="server">
                        <tr>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="vertical-align: top;">
                                <%--Ouverture de positions--%>
                                <h3 runat="server">
                                    <efsc:WCTooltipLabel runat="server" ID="lblReopeningPositions" />
                                </h3>
                                <div style="height: 100%">
                                    <table style="width: 100%">
                                        <tr>
                                            <td>
                                                <asp:ValidationSummary ID="validReopeningPositions" runat="server" DisplayMode="List" ValidationGroup="PNL3" />
                                            </td>

                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblLinkDealer_O" Width="150px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlLinkDealer_O" Width="303px" />
                                                <asp:CustomValidator ID="ddlLinkDealer_OValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlLinkDealer_O" ValidationGroup="PNL3" ValidateEmptyText="true" Display="None" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblLinkClearer_O" Width="150px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlLinkClearer_O" Width="303px" />
                                                <asp:CustomValidator ID="ddlLinkClearer_OValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlLinkClearer_O" ValidationGroup="PNL3" ValidateEmptyText="true" Display="None" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblLinkCssCustodian_O" Width="150px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlLinkCssCustodian_O" Width="303px" />
                                                <asp:CustomValidator ID="ddlLinkCssCustodian_OValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlLinkCssCustodian_O" ValidationGroup="PNL3" ValidateEmptyText="true" Display="None" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <hr />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblMode_O" Width="100px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlMode_O" AutoPostBack="true" OnSelectedIndexChanged="OnMethodOpeningChanged" Width="160px" />
                                                <efsc:WCToolTipPanel runat="server" ID="ttipMode_O" />
                                                <asp:CustomValidator ID="ddlMode_OValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlMode_O" ValidationGroup="PNL3" ValidateEmptyText="true" Display="None" />
                                            </td>
                                        </tr>
                                        <tr class="lblprice">
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblEQTYPrice_O" Width="160px" />
                                                <efsc:WCTooltipLabel runat="server" ID="lblFUTPrice_O" Width="140px" />
                                                <efsc:WCTooltipLabel runat="server" ID="lblOtherPrice_O" Width="140px" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>

                                                <efsc:WCTooltipLabel runat="server" ID="lblChoicePrice_O" Width="100px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlEQTYPrice_O" Width="160px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlFUTPrice_O" Width="140px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlOtherPrice_O" Width="140px" />
                                                <efsc:WCToolTipPanel runat="server" ID="ttipOtherPrice_O" />

                                                <asp:CustomValidator ID="ddlEQTYPrice_OValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlEQTYPrice_O" ValidationGroup="PNL3" ValidateEmptyText="true" Display="None" />
                                                <asp:CustomValidator ID="ddlFUTPrice_OValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlFUTPrice_O" ValidationGroup="PNL3" ValidateEmptyText="true" Display="None" />
                                                <asp:CustomValidator ID="ddlOtherPrice_OValidator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="ddlOtherPrice_O" ValidationGroup="PNL3" ValidateEmptyText="true" Display="None" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <hr />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <efsc:WCTooltipLabel runat="server" ID="lblFeeAction_O" Width="100px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlFeeAction_O" Width="303px" />
                                            </td>
                                        </tr>

                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
            </asp:Panel>
        </asp:Panel>
    </form>
    <script type="text/javascript">
        $(document).ready(function () {
            $(".crp-autocomplete").autocomplete({
                source: function (request, response) {
                    let currentCtrlId = $(this)[0].element[0].id; // Control with autocomplete 
                    let typeAssociated = $get(currentCtrlId.replace('txtID', 'ddlType')).value;

                    $.ajax({
                        url: "ClosingReopeningPosition.aspx/LoadDataControl",
                        data: "{ 'currentCtrlId': '" + currentCtrlId + "', 'typeAssociated': '" + typeAssociated + "', 'identifier': '" + request.term + "'}",
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        dataFilter: function (data) { return data; },
                        success: function (data) {
                            response($.map(data.d, function (item) {
                                return { value: item.Value }
                            }))
                        },
                        error: AutocompleteError
                    });
                },
                minLength: 1
            });
        });
    </script>    
</body>
</html>
