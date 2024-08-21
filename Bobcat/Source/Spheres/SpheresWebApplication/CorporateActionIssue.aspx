<%@ Page Language="C#" AutoEventWireup="true" Inherits="EFS.Spheres.CorporateActionIssuePage" Codebehind="CorporateActionIssue.aspx.cs" %>
<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title id="titlePage" runat="server"></title>

    <script type="text/javascript">
        function openSQLScript(pRDBMS, pServer, pDbName, pFileName) {
            WshShell = new ActiveXObject("WScript.Shell");
            if ("MSSQL" == pRDBMS)
                WshShell.Run("Ssms.exe -S " + pServer + " -d " + pDbName + " -U sa -P efs98* " + pFileName, 1, false);
            else if ("ORA" == pRDBMS)
                alert("Unavailable NOW !!!");
        }
    </script>

</head>
<body runat="server">
    <form id="frmCorporateAction" method="post" runat="server">
    <asp:PlaceHolder ID="plhHeader" runat="server"></asp:PlaceHolder>
    <asp:ScriptManager ID="ScriptManager" runat="server" />
    <div id="divalltoolbar">
        <asp:Panel ID="tblist" runat="server">
            <div>
                <efsc:WCToolTipLinkButton ID="btnRefresh" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
                <efsc:WCToolTipLinkButton ID="btnRecord" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
                <efsc:WCToolTipLinkButton ID="btnCancel" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
                <efsc:WCToolTipLinkButton ID="btnRemove" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
                <efsc:WCToolTipLinkButton ID="btnDuplicate" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
            </div>
            <div>
                <efsc:WCToolTipLinkButton ID="btnSend" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
                <efsc:WCToolTipLinkButton ID="btnTestRatio" runat="server" Visible="false" CssClass="fa-icon" OnCommand="OnAdjustmentByRatio" />
                <efsc:WCToolTipLinkButton ID="btnNotepad" runat="server" CssClass="fa-icon" />
                <efsc:WCToolTipLinkButton ID="btnAttachedNotice" runat="server" CssClass="fa-icon" />
            </div>
            <div>
                <efsc:WCToolTipLinkButton ID="btnSeeFinalResult" runat="server" CssClass="fa-icon" OnCommand="GetFinalResult" />
                <efsc:WCToolTipLinkButton ID="btnSeeContractResult" runat="server" CssClass="fa-icon"/>
                <efsc:WCToolTipLinkButton ID="btnSeeAssetResult" runat="server" CssClass="fa-icon" />
                <efsc:WCToolTipLinkButton ID="btnSeeMsg" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
                <efsc:WCToolTipLinkButton ID="btnSeeProcedure" runat="server" CssClass="fa-icon" OnCommand="OnAction" />
            </div>
            <div></div>

        </asp:Panel>
    </div>
    <asp:Panel id="divbody" runat="server">

        <asp:Panel runat="server" ID="pnlCorporateAction" Style="margin: 2px; height: 96%;min-width: 870px">
            <div class="headh">
                <efsc:WCTooltipLabel CssClass="size3" runat="server" ID="lblCACharacteristics" />
                <efsc:WCTooltipLabel runat="server" ID="lblIDCA" />
            </div>
            <div class="contenth">
                <div>
                    <!-- LEFT -->
                    <div>
                        <efsc:WCTooltipLabel runat="server" ID="lblCAMarket" Width="100px" />
                        <efsc:WCTextBox runat="server" ID="txtCAMarket" CssClass="ca-autocomplete txtCapture" AutoPostBack="true"  OnTextChanged="OnMarketChanged" />
                        <efsc:WCTooltipLabel runat="server" ID="roCAMarket" Visible="false" />
                        <asp:CustomValidator ID="txtCAMarketValidator" runat="server" OnServerValidate="ControlToValidate" ErrorMessage="*" ControlToValidate="txtCAMarket" ValidationGroup="CA" ValidateEmptyText="true" Display="None" />
                        <efsc:WCTooltipLabel runat="server" ID="lblCAEmbeddedState" />
                        <br />
                        <efsc:WCTooltipLabel runat="server" ID="lblCAIdentifier" Width="100px" />
                        <efsc:WCTextBox runat="server" ID="txtCAIdentifier" onblur="Copy(this,'txtCADisplayName','new');" />
                        <efsc:WCTooltipLabel runat="server" ID="roCAIdentifier" Visible="false" />
                        <efsc:WCTooltipLabel runat="server" ID="lblCAReadyState" Width="110px" />
                        <efsc:WCDropDownList2 runat="server" ID="ddlCAReadyState" Width="90px" />
                        <br />
                        <efsc:WCTooltipLabel runat="server" ID="lblCADisplayName" Width="100px" />
                        <efsc:WCTextBox runat="server" ID="txtCADisplayName" />
                        <efsc:WCTooltipLabel runat="server" ID="lblCACfiCodeCategory" Width="110px" />
                        <efsc:WCDropDownList2 runat="server" ID="ddlCACfiCodeCategory" Width="90px" AutoPostBack="True" OnSelectedIndexChanged="OnCACfiCodeCategoryChanged" />
                        <br />
                        <br />
                        <efsc:WCTooltipLabel runat="server" ID="lblCADocumentation" Width="100px" Style="vertical-align: top;" />
                        <efsc:WCTextBox runat="server" ID="txtCADocumentation" Height="100px" />
                    </div>
                    <!-- FILLER -->
                    <div>
                    </div>
                    <!-- RIGHT -->
                    <div>
                        <!-- Validation summary -->
                        <asp:ValidationSummary ID="validSummaryCA" runat="server" DisplayMode="List" ShowSummary="true"
                            ShowMessageBox="true" Visible="true" />
                        <!-- Notices -->
                        <div id="pnlNotices" runat="server">
                            <div class="headh" >
                                <efsc:WCTooltipLabel runat="server" CssClass="size3" ID="lblNotices" />
                            </div>
                            <div class="contenth">
                                <div>
                                    <efsc:WCTooltipLabel runat="server" ID="lblCAURLNotice" Width="100px"/>
                                    <efsc:WCTextBox runat="server" ID="txtCAURLNotice"  />
                                    <br />
                                    <efsc:WCTooltipLabel runat="server" ID="lblCARefNotice" Width="100px" />
                                    <efsc:WCTextBox runat="server" ID="txtCARefNotice" Width="150px" />
                                    <efsc:WCTooltipLabel runat="server" ID="roCARefNotice" Width="150px" Visible="false" />
                                    <asp:CustomValidator ID="txtCARefNoticeValidator" runat="server" OnServerValidate="GlobalValidate"
                                        ErrorMessage="*" ControlToValidate="txtCARefNotice" ValidationGroup="CA" Display="None" />
                                    <br />
                                    <efsc:WCTooltipLabel runat="server" ID="lblCAPubDate" Width="100px" />
                                    <efsc:WCTextBox runat="server" ID="txtCAPubDate" Width="60px"></efsc:WCTextBox>
                                    <efsc:WCTooltipLabel runat="server" ID="roCAPubDate" Width="60px" Visible="false" />
                                    <asp:CustomValidator ID="txtCAPubDateValidator" runat="server" OnServerValidate="GlobalValidate"
                                        ErrorMessage="*" ControlToValidate="txtCAPubDate" ValidationGroup="CA" Display="None" />
                                    <br />
                                    <efsc:WCCheckBox2 runat="server" ID="chkCAUseURLNotice" Width="100px"/>
                                    <efsc:WCTextBox runat="server" ID="txtCARefNoticeFileName"  />
                                    <efsc:WCToolTipLinkButton runat="server" ID="btnAddRefNotice" CssClass="fa-icon" CommandArgument="add" OnCommand="OnAddDelRefNotice" />
                                    <efsc:WCToolTipLinkButton runat="server" ID="btnDelRefNotice" CssClass="fa-icon" CommandArgument="substract" OnCommand="OnAddDelRefNotice" />
                                    <br />
                                    <asp:PlaceHolder ID="plhCARefNoticeAdds" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel runat="server" ID="pnlCorporateEvent" Style="margin: 2px; height: 96%; min-width: 870px">
            <div class="headh" >
                <efsc:WCTooltipLabel runat="server" CssClass="size3" ID="lblCorporateEvent" />
                <efsc:WCTooltipLabel runat="server" ID="lblIDCE" />
            </div>
            <div class="contenth">
                <!-- Classification / Type -->
                <div id="pnlCEMain">
                    <div>
                        <efsc:WCTooltipLabel runat="server" ID="lblCEGroup" Width="100px" />
                        <efsc:WCDropDownList2 runat="server" ID="ddlCEGroup" Width="200px" OnSelectedIndexChanged="OnCEGroupChanged" AutoPostBack="True" />
                        <efsc:WCToolTipPanel runat="server" ID="ttipCEGroup" />
                        <efsc:WCTooltipLabel runat="server" ID="roCEGroup" Width="200px" Visible="false" />
                        <br />
                        <efsc:WCTooltipLabel runat="server" ID="lblCEType" Width="100px" />
                        <efsc:WCDropDownList2 runat="server" ID="ddlCEType" Width="200px" OnSelectedIndexChanged="OnCETypeChanged"
                            AutoPostBack="True" />
                        <efsc:WCDropDownList2 runat="server" ID="ddlCECombinedOperand" OnSelectedIndexChanged="OnEnumsChanged"
                            AutoPostBack="True" />
                        <efsc:WCDropDownList2 runat="server" ID="ddlCECombinedType" Width="200px" OnSelectedIndexChanged="OnEnumsChanged"
                            AutoPostBack="True" />
                        <efsc:WCToolTipPanel runat="server" ID="ttipCEType" />
                        <efsc:WCTooltipLabel runat="server" ID="roCEType" Visible="false" />
                        <asp:CustomValidator ID="ddlCETypeValidator" runat="server" OnServerValidate="ControlToValidate"
                            ErrorMessage="*" ControlToValidate="ddlCEType" ValidationGroup="CE" Display="None"
                            ValidateEmptyText="true" />
                        <asp:CustomValidator ID="ddlCECombinedOperandValidator" runat="server" OnServerValidate="ControlToValidate"
                            ErrorMessage="*" ControlToValidate="ddlCECombinedOperand" ValidationGroup="CE"
                            Display="None" ValidateEmptyText="true" />
                        <asp:CustomValidator ID="ddlCECombinedTypeValidator" runat="server" OnServerValidate="ControlToValidate"
                            ErrorMessage="*" ControlToValidate="ddlCECombinedType" ValidationGroup="CE" Display="None"
                            ValidateEmptyText="true" />
                    </div>
                    <!-- FILLER -->
                    <div>
                    </div>
                    <!-- Validation summary -->
                    <div>
                        <asp:ValidationSummary ID="validSummaryCE" runat="server" DisplayMode="List" ValidationGroup="CE" />
                    </div>
                </div>
                <div>
                    <!--LEFT DIV-->
                    <div>
                        <div id="pnlCEDescription" runat="server">
                            <div class="headh" >
                                <efsc:WCTooltipLabel runat="server" CssClass="size3" ID="lblDESCRIPTION" />
                            </div>
                            <div class="contenth">
                                <!--Caractéristiques générales événement de CA-->
                                <div id="pnlCEDescriptionDates">
                                    <efsc:WCTooltipLabel runat="server" ID="lblCEIdentifier" Width="100px" />
                                    <efsc:WCTextBox runat="server" ID="txtCEIdentifier"  />
                                    <efsc:WCTooltipLabel runat="server" ID="roCEIdentifier"  Visible="false" />
                                    <br />
                                    <efsc:WCTooltipLabel runat="server" ID="lblCEMode" Width="100px" />
                                    <efsc:WCDropDownList2 runat="server" Width="140px" ID="ddlCEMode" />
                                    <br />
                                    <efsc:WCTooltipLabel runat="server" ID="lblCEExDate" Width="100px" />
                                    <efsc:WCTextBox runat="server" ID="txtCEExDate" Width="60px" onblur="Copy(this,'txtCEEffectiveDate','new');" />
                                    <asp:CustomValidator ID="txtCEExDateValidator" runat="server" OnServerValidate="GlobalValidate"
                                        ErrorMessage="*" ControlToValidate="txtCEExDate" ValidationGroup="CE" Display="None" />
                                    <br />
                                    <efsc:WCTooltipLabel runat="server" ID="lblCEEffectiveDate" Width="100px" />
                                    <efsc:WCTextBox runat="server" ID="txtCEEffectiveDate" Width="60px" />
                                    <asp:CustomValidator ID="txtCEEffectiveDateValidator" runat="server" OnServerValidate="GlobalValidate"
                                        ErrorMessage="*" ControlToValidate="txtCEEffectiveDate" ValidationGroup="CE"
                                        Display="None" />
                                </div>
                                <!--Caractéristiques sous-jacent(s)-->
                                <div id="pnlUnderlyers" runat="server">
                                    <div class="headhR" >
                                        <efsc:WCTooltipLabel runat="server" CssClass="size4" ID="lblCEUnderlyer" />
                                    </div>
                                    <div class="contenth">
                                        <div>
                                            <efsc:WCTooltipLabel runat="server" ID="lblCEUNLCategory" Width="100px" />
                                            <efsc:WCDropDownList2 runat="server" ID="ddlCEUNLCategory" />
                                            <efsc:WCTooltipLabel runat="server" ID="roCEUNLCategory" Visible="false" />
                                            <br />
                                            <efsc:WCTooltipLabel runat="server" ID="lblCEUNLMarket" Width="100px" />
                                            <efsc:WCTextBox runat="server" ID="txtCEUNLMarket" AutoPostBack="True"  CssClass="ca-autocomplete txtCapture" />
                                            <efsc:WCTooltipLabel runat="server" ID="roCEUNLMarket"  Visible="false" />
                                            <asp:CustomValidator ID="txtCEUNLMarketValidator" runat="server" OnServerValidate="ControlToValidate"
                                                ErrorMessage="*" ControlToValidate="txtCEUNLMarket" ValidationGroup="CE" ValidateEmptyText="true"
                                                Display="None" />
                                            <br />
                                            <efsc:WCTooltipLabel runat="server" ID="lblCEUNLCode" Width="100px" />
                                            <efsc:WCTextBox runat="server" ID="txtCEUNLCode"  />
                                            <efsc:WCTooltipLabel runat="server" ID="roCEUNLCode" Visible="false" />
                                            <asp:CustomValidator ID="txtCEUNLCodeValidator" runat="server" OnServerValidate="ControlToValidate"
                                                ErrorMessage="*" ControlToValidate="txtCEUNLCode" ValidationGroup="CE" ValidateEmptyText="true"
                                                Display="None" />
                                            <div id="pnlUnderlyer1" runat="server">
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEUNLIdentifier" Width="100px" />
                                                <efsc:WCTextBox runat="server" ID="txtCEUNLIdentifier"  Visible="false" />
                                                <efsc:WCTooltipLabel runat="server" ID="roCEUNLIdentifier" Visible="false" />
                                            </div>
                                            <div id="pnlUnderlyer2" runat="server">
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEUNLCategory2" Width="100px" />
                                                <efsc:WCDropDownList2 runat="server" ID="ddlCEUNLCategory2" />
                                                <efsc:WCTooltipLabel runat="server" ID="roCEUNLCategory2" Visible="false" />
                                                <br />
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEUNLMarket2" Width="100px" />
                                                <efsc:WCTextBox runat="server" ID="txtCEUNLMarket2" AutoPostBack="True"  CssClass="ca-autocomplete txtCapture" />
                                                <efsc:WCTooltipLabel runat="server" ID="roCEUNLMarket2" Visible="false" />
                                                <asp:CustomValidator ID="txtCEUNLMarketValidator2" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="txtCEUNLMarket2" ValidationGroup="CE" ValidateEmptyText="true"
                                                    Display="None" />
                                                <br />
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEUNLCode2" Width="100px" />
                                                <efsc:WCTextBox runat="server" ID="txtCEUNLCode2" />
                                                <efsc:WCTooltipLabel runat="server" ID="roCEUNLCode2" Visible="false" />
                                                <asp:CustomValidator ID="txtCEUNLCode2Validator" runat="server" OnServerValidate="ControlToValidate"
                                                    ErrorMessage="*" ControlToValidate="txtCEUNLCode2" ValidationGroup="CE" ValidateEmptyText="true"
                                                    Display="None" />
                                                <br />
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEUNLIdentifier2" Width="100px" />
                                                <efsc:WCTextBox runat="server" ID="txtCEUNLIdentifier2" />
                                                <efsc:WCTooltipLabel runat="server" ID="roCEUNLIdentifier2" Visible="false" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!--Informations additionnelles-->
                        <div id="pnlCEComponents_AI" runat="server">
                            <div class="headh" >
                                <efsc:WCTooltipLabel runat="server" CssClass="size3" ID="lblCEComponents_AI" />
                            </div>
                            <div class="contenth">
                                <div>
                                    <efsc:WCTooltipLabel runat="server" ID="lblCETemplate_AI" Width="130px" />
                                    <efsc:WCDropDownList2 runat="server" ID="ddlCETemplate_AI" Width="220px" AutoPostBack="True"
                                        OnSelectedIndexChanged="OnCETemplate_AIChanged" />
                                    <efsc:WCTooltipLabel runat="server" ID="roCETemplate_AI" Width="220px" Visible="false" />
                                    <asp:CustomValidator ID="ddlCETemplate_AIValidator" runat="server" OnServerValidate="ControlToValidate"
                                        ErrorMessage="*" ControlToValidate="ddlCETemplate_AI" ValidationGroup="CE" Display="None"
                                        ValidateEmptyText="true" />
                                    <efsc:WCToolTipLinkButton ID="btnCETemplate_AI" runat="server" CausesValidation="true" TabIndex="-1" CssClass="fa-icon orange" OnCommand="OnAction" />
                                </div>
                                <div>
                                    <asp:PlaceHolder ID="plhCEComponents_AI" runat="server" />
                                </div>
                            </div>
                        </div>
                   </div>

                    <!-- FILLER -->
                    <div>
                    </div>

                    <!--RIGHT DIV-->
                    <div>
                        <div id="pnlCEAdjustment" runat="server">
                            <div class="headh" >
                                <efsc:WCTooltipLabel runat="server" CssClass="size3" ID="ADJ" />
                            </div>
                            <div class="contenth">
                                <!--Méthode ajustement -->
                                <div style="line-height:2">
                                    <efsc:WCTooltipLabel runat="server" ID="lblCEAdjMethod" Width="130px" />
                                    <efsc:WCDropDownList2 runat="server" ID="ddlCEAdjMethod" AutoPostBack="True" Width="220px" OnSelectedIndexChanged="OnCEMethodChanged" />
                                    <efsc:WCTooltipLabel runat="server" ID="roCEAdjMethod" Width="220px" Visible="false" />
                                    <efsc:WCToolTipPanel runat="server" ID="ttipCEAdjMethod" />
                                    <br />
                                    <efsc:WCTooltipLabel runat="server" ID="lblCETemplate" Width="130px" />
                                    <efsc:WCDropDownList2 runat="server" ID="ddlCETemplate" Width="220px" AutoPostBack="True"
                                        OnSelectedIndexChanged="OnCETemplateChanged" />
                                    <efsc:WCTooltipLabel runat="server" ID="roCETemplate" Width="220px" Visible="false" />
                                    <asp:CustomValidator ID="ddlCETemplateValidator" runat="server" OnServerValidate="ControlToValidate"
                                        ErrorMessage="*" ControlToValidate="ddlCETemplate" ValidationGroup="CE" Display="None"
                                        ValidateEmptyText="true" />
                                    <efsc:WCToolTipLinkButton ID="btnCETemplate" runat="server" CausesValidation="true" TabIndex="-1" CssClass="fa-icon orange" OnCommand="OnAction" />

                                    <p><efsc:WCTooltipLabel runat="server" ID="lblCEAdjContract" /></p>
                                    <div id="pnlAdjFuture" runat="server">
                                        <efsc:WCTooltipLabel runat="server" ID="lblCEAdjFuture" Style="display:inline-block;width:60px" />
                                        <efsc:WCCheckBox2 runat="server" ID="chkCEAdjFutureCSize" Width="75" />
                                        <efsc:WCCheckBox2 runat="server" ID="chkCEAdjFuturePrice" Width="75" />
                                        <efsc:WCCheckBox2 runat="server" ID="chkCEAdjFutureEqualisationPayment" Width="100" AutoPostBack="true" OnCheckedChanged="OnCheckedChanged_EqualisationPayment" />
                                        <br />
                                    </div>
                                    <div id="pnlAdjOption" runat="server">
                                        <efsc:WCTooltipLabel runat="server" ID="lblCEAdjOption" Style="display:inline-block;width:60px" />
                                        <efsc:WCCheckBox2 runat="server" ID="chkCEAdjOptionCSize" Width="75" />
                                        <efsc:WCCheckBox2 runat="server" ID="chkCEAdjOptionPrice" Width="75" />
                                        <efsc:WCCheckBox2 runat="server" ID="chkCEAdjOptionStrikePrice" Width="100" />
                                        <efsc:WCCheckBox2 runat="server" ID="chkCEAdjOptionEqualisationPayment" Width="100" AutoPostBack="true" OnCheckedChanged="OnCheckedChanged_EqualisationPayment" />
                                    </div>

                                </div>

                                <!--Composants de la formule d'ajustement -->
                                <div id="pnlCEComponents" style="margin-top:10px;" runat="server">
                                    <div class="headhR" >
                                        <efsc:WCTooltipLabel runat="server" CssClass="size4" ID="lblCEComponents" />
                                    </div>
                                    <div class="contenth">
                                        <asp:PlaceHolder ID="plhCEComponents" runat="server" />
                                    </div>
                                </div>

                                <!--Règles d'arrondi -->
                                <div id="pnlCERoundingRules" style="margin-top:10px;" runat="server">
                                    <div class="headhR" >
                                        <efsc:WCTooltipLabel runat="server" CssClass="size4" ID="lblCERoundingRules" />
                                    </div>
                                    <div class="contenth">
                                        <div style="display:flex;flex-wrap:wrap;line-height:2">
                                            <div style="min-width:220px;">
                                                <efsc:WCTooltipLabel runat="server" ID="lblCERatioRoundingRules" Width="100px" />
                                                <efsc:WCDropDownList2 ID="ddlCERFactorRoundingDir" runat="server" Width="70px" />
                                                <efsc:WCDropDownList2 ID="ddlCERFactorRoundingPrec" runat="server" Width="50px" />
                                                <br />
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEContractSizeRoundingRules" Width="100px" />
                                                <efsc:WCDropDownList2 ID="ddlCEContractSizeRoundingDir" runat="server" Width="70px" />
                                                <efsc:WCDropDownList2 ID="ddlCEContractSizeRoundingPrec" runat="server" Width="50px" />
                                                <br />
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEContractMultiplierRoundingRules" Width="100px" />
                                                <efsc:WCDropDownList2 ID="ddlCEContractMultiplierRoundingDir" runat="server" Width="70px" />
                                                <efsc:WCDropDownList2 ID="ddlCEContractMultiplierRoundingPrec" runat="server" Width="50px" />
                                            </div>
                                            <div style="width:2%">

                                            </div>
                                            <div style="min-width:220px;">
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEStrikePriceRoundingRules" Width="100px" />
                                                <efsc:WCDropDownList2 ID="ddlCEStrikePriceRoundingDir" runat="server" Width="70px" />
                                                <efsc:WCDropDownList2 ID="ddlCEStrikePriceRoundingPrec" runat="server" Width="50px" />
                                                <br />
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEPriceRoundingRules" Width="100px" />
                                                <efsc:WCDropDownList2 ID="ddlCEPriceRoundingDir" runat="server" Width="70px" />
                                                <efsc:WCDropDownList2 ID="ddlCEPriceRoundingPrec" runat="server" Width="50px" />
                                                <br />
                                                <efsc:WCTooltipLabel runat="server" ID="lblCEEqualisationPaymentRoundingRules" Width="100px" />
                                                <efsc:WCDropDownList2 ID="ddlCEEqualisationPaymentRoundingDir" runat="server" Width="70px" />
                                                <efsc:WCDropDownList2 ID="ddlCEEqualisationPaymentRoundingPrec" runat="server" Width="50px" />
                                             </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!--Scripts SQL -->
                        <div id="pnlSqlScripts" runat="server">
                            <div class="headh" >
                                <efsc:WCTooltipLabel runat="server" CssClass="size3" ID="lblCEScript" />
                            </div>
                            <div class="contenth">
                                <div>
                                    <efsc:WCTooltipLabel runat="server" ID="lblFilename" Width="100px" />
                                    <asp:FileUpload ID="fileUpload" runat="server" />
                                    <asp:HiddenField ID="hidNFI" runat="server" />
                                    <div id="pnlScripts2">
                                        <!-- Labels Scripts -->
                                        <div style="width:100px">
                                            <p>Exécution</p>
                                            <efsc:WCTooltipLabel runat="server" ID="lblCEMSSQL_Script_C" Width="100px" />
                                            <br />
                                            <efsc:WCTooltipLabel runat="server" ID="lblCEMSSQL_Script_E" Width="100px" />
                                            <br />
                                            <efsc:WCTooltipLabel runat="server" ID="lblCEMSSQL_Script_P" Width="100px" />
                                            <br />
                                            <efsc:WCTooltipLabel runat="server" ID="lblCEMSSQL_Script_F" Width="100px" />
                                        </div>
                                        <!-- Scripts MS SQLServer -->
                                        <div style="width:45%">
                                            <p>MS SQLServer</p>
                                            <efsc:WCTextBox runat="server" ID="txtMSSQL_File_C" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Upload_C" runat="server" CssClass="fa-icon blue" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnUpload" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Delete_C" runat="server" CssClass="fa-icon red" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnDeleteFile" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Open_C" runat="server" CssClass="fa-icon green" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnOpenFile"  />
                                            <br />
                                            <efsc:WCTextBox runat="server" ID="txtMSSQL_File_E" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Upload_E" runat="server" CssClass="fa-icon blue" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnUpload" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Delete_E" runat="server" CssClass="fa-icon red" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnDeleteFile" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Open_E" runat="server" CssClass="fa-icon green" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnOpenFile"  />
                                            <br />
                                            <efsc:WCTextBox runat="server" ID="txtMSSQL_File_P" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Upload_P" runat="server" CssClass="fa-icon blue" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnUpload" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Delete_P" runat="server" CssClass="fa-icon red" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnDeleteFile" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Open_P" runat="server" CssClass="fa-icon green" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnOpenFile"  />
                                            <br />
                                            <efsc:WCTextBox runat="server" ID="txtMSSQL_File_F" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Upload_F" runat="server" CssClass="fa-icon blue" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnUpload" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Delete_F" runat="server" CssClass="fa-icon red" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnDeleteFile" />
                                            <efsc:WCToolTipLinkButton ID="btnMSSQL_Open_F" runat="server" CssClass="fa-icon green" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnOpenFile" />
                                        </div>
                                        <!-- Scripts ORACLE -->
                                        <div style="width:45%">
                                            <p>Oracle</p>
                                            <efsc:WCTextBox runat="server" ID="txtORA_File_C" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Upload_C" runat="server" CssClass="fa-icon blue" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnUpload" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Delete_C" runat="server" CssClass="fa-icon red" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnDeleteFile" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Open_C" runat="server" CssClass="fa-icon green" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnOpenFile" />
                                            <br />
                                            <efsc:WCTextBox runat="server" ID="txtORA_File_E" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Upload_E" runat="server" CssClass="fa-icon blue" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnUpload" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Delete_E" runat="server" CssClass="fa-icon red" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnDeleteFile" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Open_E" runat="server" CssClass="fa-icon green" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnOpenFile" />
                                            <br />
                                            <efsc:WCTextBox runat="server" ID="txtORA_File_P" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Upload_P" runat="server" CssClass="fa-icon blue" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnUpload" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Delete_P" runat="server" CssClass="fa-icon red" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnDeleteFile" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Open_P" runat="server" CssClass="fa-icon green" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnOpenFile" />
                                            <br />
                                            <efsc:WCTextBox runat="server" ID="txtORA_File_F" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Upload_F" runat="server" CssClass="fa-icon blue" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnUpload" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Delete_F" runat="server" CssClass="fa-icon red" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnDeleteFile" />
                                            <efsc:WCToolTipLinkButton ID="btnORA_Open_F" runat="server" CssClass="fa-icon green" CausesValidation="true"
                                                TabIndex="-1" OnCommand="OnOpenFile" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        
        </asp:Panel>

    </asp:Panel>
    <script src="./Javascript/JQuery/jquery.nicefileinput.min.js" type="text/javascript"></script>

    <script type="text/javascript">
        $(document).ready(function() {
            $("input[type=file]").nicefileinput({ label: '&nbsp;', fullPath:true, defValue:true, doPostBack:true });
        });
    </script>
    </form>
    <script type="text/javascript">
        $(document).ready(function () {
            $(".ca-autocomplete").autocomplete({
                source: function (request, response) {
                    let currentCtrlId = $(this)[0].element[0].id; // Control with autocomplete 

                    $.ajax({
                        url: "CorporateActionIssue.aspx/LoadDataControl",
                        data: "{ 'currentCtrlId': '" + currentCtrlId + "', 'identifier': '" + request.term + "'}",
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
