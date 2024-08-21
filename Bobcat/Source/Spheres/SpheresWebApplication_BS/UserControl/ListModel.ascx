<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListModel.ascx.cs" Inherits="EFS.ListControl.ListModel" %>

<div id="ContentListOpen">
    <script src="Scripts/Spheres-Confirmation.js" type="text/javascript" charset="utf-8"></script>
    <script src="Scripts/ListModel.js" type="text/javascript" charset="utf-8"></script>

    <div class="modal-header btn-primary">
        <%-- Title --%>
        <h3 id="lblModelTitle" runat="server" class="modal-title" />
    </div>
    <div class="modal-body">
        <%-- Choix de l'action sur Modèle --%>
        <div id="divOpenChoice" runat="server" class="col-sm-4 sph-radio btn-group" data-toggle="buttons">
            <asp:RadioButton GroupName="main" ID="rbOpen" runat="server" OnCheckedChanged="OnRequestModeChanged" CssClass="btn btn-xs btn-apply" EnableViewState="true" />
            <asp:RadioButton GroupName="main" ID="rbSave" runat="server" OnCheckedChanged="OnRequestModeChanged" CssClass="btn btn-xs btn-apply" EnableViewState="true" />
            <asp:RadioButton GroupName="main" ID="rbRemove" runat="server" OnCheckedChanged="OnRequestModeChanged" CssClass="btn btn-xs btn-apply" EnableViewState="true" />
        </div>
        <asp:UpdatePanel ID="updSubTitle" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Label ID="lblSubTitle" runat="server" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="rbOpen" EventName="CheckedChanged" />
                <asp:AsyncPostBackTrigger ControlID="rbRemove" EventName="CheckedChanged" />
                <asp:AsyncPostBackTrigger ControlID="rbSave" EventName="CheckedChanged" />
            </Triggers>
        </asp:UpdatePanel>

        <div class="col-sm-12">
            <hr />
        </div>

        <%-- Update panel pour PostBackPartiel sur action CheckedChanged des radio boutons --%>
        <%-- Raffraichissement des données gestion de l'affichage des panels en fonction de l'action demandée --%>
        <asp:UpdatePanel ID="updRequestMode" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
            <ContentTemplate>
                <%-- Panel pour création ou sauvegarde d'un modèle --%>
                <div id="divSave" runat="server">
                    <asp:TextBox ID="txtInitialIdentifier" CssClass="hidden" runat="server" />
                    <asp:TextBox ID="txtIdLstConsult" CssClass="hidden" runat="server" />
                    <div class="col-sm-12">
                        <asp:Label ID="lblIdentifier" runat="server" />
                        <asp:RequiredFieldValidator ControlToValidate="txtIdentifier" Display="Dynamic" ID="rqvIdentifier" CssClass="vtor" runat="server"></asp:RequiredFieldValidator>
                        <asp:TextBox ID="txtIdentifier" CssClass="form-control input-xs" runat="server" />
                    </div>
                    <div class="col-sm-12">
                        <asp:Label ID="lblDisplayName" runat="server" />
                        <asp:RequiredFieldValidator ControlToValidate="txtDisplayName" Display="Dynamic" ID="rqvDisplayName" CssClass="vtor" runat="server"></asp:RequiredFieldValidator>
                        <asp:TextBox ID="txtDisplayName" CssClass="form-control input-xs" runat="server" />
                    </div>
                    <div class="col-sm-12">
                        <asp:Label ID="lblDescription" runat="server" />
                        <asp:TextBox ID="txtDescription" CssClass="form-control input-xs" TextMode="MultiLine" Rows="2" Columns="20" runat="server" />
                    </div>
                    <div class="col-sm-12">
                        <asp:Label ID="lblRefreshInterval" runat="server" />

                    </div>
                    <div class="col-sm-6">
                        <asp:TextBox ID="txtRefreshInterval" CssClass="form-control input-xs" runat="server" />
                    </div>
                    <div class="col-sm-6">
                        <div class="checkbox">
                            <div class="form-control input-xs">
                                <asp:CheckBox ID="chkTemplateDefault" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="col-sm-12">
                        <asp:Label ID="lblRowByPage" runat="server" />
                    </div>
                    <div class="col-sm-6">
                        <asp:TextBox ID="txtRowByPage" CssClass="form-control input-xs" runat="server" />
                    </div>
                    <div class="col-sm-6">
                        <div class="checkbox">
                            <div class="form-control input-xs">
                                <asp:CheckBox ID="chkIsLoadOnStart" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="col-sm-12">
                        <asp:Label ID="lblExtlLink" runat="server" />
                        <asp:TextBox ID="txtExtlLink" CssClass="form-control input-xs" runat="server" />
                    </div>

                    <div class="col-sm-12">
                        <span class="sph-title" id="lblRightModel" runat="server" />
                        <div class="col-sm-12">
                            <div class="col-sm-6">
                                <asp:Label ID="lblRightPublic" runat="server" />
                                <asp:DropDownList ID="ddlRightPublic" CssClass="form-control input-xs" runat="server" />
                            </div>
                            <div class="col-sm-6">
                                <asp:Label ID="lblRightEntity" runat="server" />
                                <asp:DropDownList ID="ddlRightEntity" CssClass="form-control input-xs" runat="server" />
                            </div>
                            <div class="col-sm-6">
                                <asp:Label ID="lblRightDepartment" runat="server" />
                                <asp:DropDownList ID="ddlRightDepartment" CssClass="form-control input-xs" runat="server" />
                            </div>
                            <div class="col-sm-6">
                                <asp:Label ID="lblRightDesk" runat="server" />
                                <asp:DropDownList ID="ddlRightDesk" CssClass="form-control input-xs" runat="server" />
                            </div>
                        </div>
                    </div>


                    <asp:TextBox ID="txtIdA" CssClass="hidden" runat="server" />
                    <asp:TextBox ID="txtDtUpd" CssClass="hidden" runat="server" />
                    <asp:TextBox ID="txtIdAUpd" CssClass="hidden" runat="server" />
                    <asp:TextBox ID="txtDtIns" CssClass="hidden" runat="server" />
                    <asp:TextBox ID="txtIdAIns" CssClass="hidden" runat="server" />
                    <asp:TextBox ID="txtRowAttribut" CssClass="hidden" runat="server" />
                    <asp:TextBox ID="txtRowVersion" CssClass="hidden" runat="server" />
                </div>
                <%-- Panel pour sélection d'un modèle parmi ceux disponibles --%>
                <div id="divOpen" runat="server">
                    <asp:UpdatePanel ID="updOwnerOpen" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Label ID="lblOwnerOpen" runat="server" />
                            <asp:DropDownList ID="ddlOwnerOpen" CssClass="form-control input-xs" runat="server" OnSelectedIndexChanged="OnActorChanged" />
                            <asp:Label ID="lblModelOpen" runat="server" />
                            <asp:RequiredFieldValidator ControlToValidate="ddlModelOpen" Display="Dynamic" ID="rqvModelOpen" CssClass="vtor" runat="server"></asp:RequiredFieldValidator>
                            <asp:DropDownList ID="ddlModelOpen" CssClass="form-control input-xs" runat="server" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="ddlOwnerOpen" EventName="SelectedIndexChanged" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
                <%-- Panel pour suppression d'un modèle parmi ceux disponibles --%>
                <div id="divRemove" runat="server">
                    <asp:UpdatePanel ID="updOwnerRemove" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Label ID="lblOwnerRemove" runat="server" />
                            <asp:DropDownList ID="ddlOwnerRemove" CssClass="form-control input-xs" runat="server" OnSelectedIndexChanged="OnActorChanged" />
                            <asp:Label ID="lblModelRemove" runat="server" />
                            <asp:RequiredFieldValidator ControlToValidate="ddlModelRemove" Display="Dynamic" ID="rqvModelRemove" CssClass="vtor" runat="server"></asp:RequiredFieldValidator>
                            <asp:DropDownList ID="ddlModelRemove" CssClass="form-control input-xs" runat="server" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="ddlOwnerRemove" EventName="SelectedIndexChanged" />
                            <asp:AsyncPostBackTrigger ControlID="btnRecord" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </ContentTemplate>
            <Triggers>
                <%--Trigger de déclenchement du postback partiel et mise à jour des controles présent de l'update panel : updRequestMode--%>
                <asp:AsyncPostBackTrigger ControlID="rbOpen" EventName="CheckedChanged" />
                <asp:AsyncPostBackTrigger ControlID="rbRemove" EventName="CheckedChanged" />
                <asp:AsyncPostBackTrigger ControlID="rbSave" EventName="CheckedChanged" />
            </Triggers>
        </asp:UpdatePanel>

    </div>
    <div class="modal-footer">
        <asp:UpdatePanel ID="updFooter" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
            <ContentTemplate>
                <div runat="server" id="divUserInfo" class="col-sm-8">
                    <asp:Label runat="server" ID="lblInsertDate" />
                    <asp:Label runat="server" ID="lblUpdateDate" />
                </div>
                <div class="col-sm-4">
                    <asp:Button ID="btnRecord" runat="server" xdata-toggle="confirmation" CssClass="btn btn-xs btn-record" OnClick="OnValid" />
                    <button id="btnCancel" runat="server" class="btn btn-xs btn-cancel active" data-dismiss="modal" />
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="rbOpen" EventName="CheckedChanged" />
                <asp:AsyncPostBackTrigger ControlID="rbRemove" EventName="CheckedChanged" />
                <asp:AsyncPostBackTrigger ControlID="rbSave" EventName="CheckedChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</div>
