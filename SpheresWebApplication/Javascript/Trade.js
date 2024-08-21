// EG 2016-01-19 Refactoring
// EG 2016-01-19 Refactoring Footer
// EG 2020-07-20 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
function OnTradeChanged(tradeId){
    var txtTrade = document.getElementById(tradeId);
	if (null != txtTrade)
	{
	    var oAttrColl = txtTrade.attributes;
	    var oAttr = oAttrColl.getNamedItem("oldvalue");
		if (null != oAttr)
        {
            if (oAttr.value != txtTrade.value)
            {
                var tblStatus = document.getElementById('divdescandstatus');
                var pnlSummary = document.getElementById('pnlSummary');
                var tblDetail = document.getElementById('tblDetail');
                var pnlFooter = document.getElementById('divfooter');
                var pnlExtLLink = document.getElementById('divextllink');
                if (null != tblStatus)
                    tblStatus.removeNode(true);
                if (null != pnlSummary)
                    pnlSummary.removeNode(true);
                if (null != tblDetail)
                    tblDetail.removeNode(true);
                if (null != pnlExtlLink)
                    pnlExtlLink.removeNode(true);
                if (null != pnlFooter)
                    pnlFooter.removeNode(true);
            }
        }
    }
}

/* FI 2020-01-28 [25182] Add Method */
// EG 20210407 [25556] Message de confirmation (Record/Annul) avec Dialog JQuery (à la place de window.confirm)
// EG 20210928 [XXXXX] Test sur valeur pEnableValidators (Pb minification)
function SaveInputTrade(pTitle, pTarget, pMsgConfirm, pIdentifier, pEnableValidators) {
    var ret = true;

    if (pEnableValidators === "True") {
        EnableValidators();
        ret = Page_ClientValidate();
    }
    if (ret) {
        ConfirmInputTrade(pTitle, pTarget, pMsgConfirm, pIdentifier);
    }
    if (ret) {
        SetNoAutoPostbackCtrlChanged('tblDetail');
    }
    DisableValidators();

    return ret;
}


// EG 20210407 [25556] Message de confirmation (Record/Annul) avec Dialog JQuery (à la place de window.confirm)
function ConfirmInputTrade(pTitle, pTarget, pMsgConfirm, pIdentifier) {
    ConfirmInputTradeWithArgs(pTitle, pTarget, pMsgConfirm, pIdentifier, "TRUE", "FALSE");
}
// EG 20210623 [XXXXX] Appel à nouvelle fonction ConfirmInputTradeWithArgs
// pour entrer en modification avec message d'alerte demandant confirmation)
function ConfirmInputTradeWithArgs(pTitle, pTarget, pMsgConfirm, pIdentifier, pArgument_Ok, pArgument_Cancel) {
    pMsgConfirm = pMsgConfirm.replace("{0}", pIdentifier);
    OpenMainConfirm(pTitle, pMsgConfirm, "confirm", 0, 0, 300, 0, pTarget, pArgument_Ok, pArgument_Cancel);
}

// EG 20210407 [25556] Message de confirmation (Record/Annul) avec Dialog JQuery (à la place de window.confirm)
function ConfirmInputEvent(pTitle, pTarget, pMsgConfirm, pTradeidentifier, pIdentifier) {
    pMsgConfirm = pMsgConfirm.replace("{0}", pTradeidentifier);
    pMsgConfirm = pMsgConfirm.replace("{1}", pIdentifier);
    OpenMainConfirm(pTitle, pMsgConfirm, "confirm", 0, 0, 300, 0, pTarget, "TRUE", "FALSE");
}

function SetProduct(){
    var form = document.forms[0];
    javascript:__doPostBack('tblMenu$mnuConsult','SetProduct');
}

// OpenTradeAccDayBook
// PL 2016-07-25 Add Accounting:
function OpenTradeAccDayBook(pIdT, pSubTitle) {
    var urlToOpen = "List.aspx?" + "Trade=Accounting:ACCDAYBOOK";
	urlToOpen    += "&IDMenu=OTC_VIEW_ACCDAYBOOK";
	urlToOpen    += "&InputMode=2";
	urlToOpen    += "&CondApp=TRADE_CONSUL";
	urlToOpen    += "&DA=[[Data name='IDT' datatype='int']]" + pIdT + "[[/Data]]";
	urlToOpen    += "&SubTitle=" + pSubTitle; 
	ret = window.open(urlToOpen);
}
// OpenTradeCashFlows
function OpenTradeCashFlows(pIdT, pSubTitle) {
    var urlToOpen = "List.aspx?" + "Trade=CASHFLOWS";
    urlToOpen += "&IDMenu=CashFlows";
    urlToOpen += "&InputMode=2";
    urlToOpen += "&CondApp=TRADE_CONSUL";
    urlToOpen += "&DA=[[Data name='IDT' datatype='int']]" + pIdT + "[[/Data]]";
    urlToOpen += "&SubTitle=" + pSubTitle;
    ret = window.open(urlToOpen);
}
// OpenTradeClearingSpecific
// EG 2015-09-07 [21317] Gestion EquitySecurityTransaction et DebtSecurityTransaction
function OpenTradeClearingSpecific(pIdT, pIdentifier, pSide, pAvailableQuantity, pPrice, pEntity, pDtBusiness,
pCssCustodian, pMarket, pAsset, pDealer, pBookDealer, pClearer, pBookClearer)
{
    var urlToOpen = "List.aspx?" + "ProcessBase=POSKEEPING_SPEC";
    urlToOpen += "&ProcessType=ProcessCSharp";
    urlToOpen += "&ProcessName=EfsML.Business.PosKeepingTools|EFS.EfsML|AddPosRequestClearingSPEC";
    urlToOpen += "&InputMode=2";
    urlToOpen += "&IDMENU=OTC_INP_TRD_CLEARSPEC";
    urlToOpen += "&DA=[[Data name='IDENTIFIER' datatype='string']]" + pIdentifier + "[[/Data]];";
    urlToOpen += "[[Data name='SIDE' datatype='string']]" + pSide + "[[/Data]];";
    urlToOpen += "[[Data name='AVAILABLEQTY' datatype='int']]" + pAvailableQuantity + "[[/Data]];";
    urlToOpen += "[[Data name='PRICE' datatype='decimal']]" + pPrice + "[[/Data]];";
    urlToOpen += "[[Data name='ENTITY' datatype='int']]" + pEntity + "[[/Data]];";
    urlToOpen += "[[Data name='DTBUSINESS' datatype='date']]" + pDtBusiness + "[[/Data]];";
    urlToOpen += "[[Data name='CSSCUSTODIAN' datatype='int']]" + pCssCustodian + "[[/Data]];";
    urlToOpen += "[[Data name='MARKET' datatype='int']]" + pMarket + "[[/Data]];";
    urlToOpen += "[[Data name='ASSET' datatype='string']]" + pAsset + "[[/Data]];";
    urlToOpen += "[[Data name='IDT' datatype='int']]" + pIdT + "[[/Data]];";
    urlToOpen += "[[Data name='DEALER' datatype='string']]" + pDealer + "[[/Data]];";
    urlToOpen += "[[Data name='BOOKDEALER' datatype='string']]" + pBookDealer + "[[/Data]];";
    urlToOpen += "[[Data name='CLEARER' datatype='string']]" + pClearer + "[[/Data]];";
    urlToOpen += "[[Data name='BOOKCLEARER' datatype='string']]" + pBookClearer + "[[/Data]];";
    ret = window.open(urlToOpen);
}

// OpenTradePosAction
function OpenTradePosAction_ETD(pIdT, pSubTitle)
{
    var urlToOpen = "List.aspx?" + "Log=POSACTIONDET";
    urlToOpen += "&IDMenu=OTC_VIEW_FO_POSACTIONDET";
    urlToOpen += "&InputMode=2";
    urlToOpen += "&isLoadData=1";
    urlToOpen += "&DA=[[Data name='IDT' datatype='int']]" + pIdT + "[[/Data]]";
    urlToOpen += "&SubTitle=" + pSubTitle;
    ret = window.open(urlToOpen);
}

// OpenTradePosAction
function OpenTradePosAction_OTC(pIdT, pSubTitle) {
    var urlToOpen = "List.aspx?" + "Log=POSACTIONDET_OTC";
    urlToOpen += "&IDMenu=OTC_VIEW_FO_POSACTIONDET";
    urlToOpen += "&InputMode=2";
    urlToOpen += "&isLoadData=1";
    urlToOpen += "&DA=[[Data name='IDT' datatype='int']]" + pIdT + "[[/Data]]";
    urlToOpen += "&SubTitle=" + pSubTitle;
    ret = window.open(urlToOpen);
}

// OpenTradeAuditAction
function OpenTradeAuditAction(pIdT, pSubTitle)
{
    var urlToOpen = "List.aspx?" + "Log=ACTIONDET";
    urlToOpen += "&InputMode=2";
    urlToOpen += "&isLoadData=1";
    urlToOpen += "&DA=[[Data name='IDT' datatype='int']]" + pIdT + "[[/Data]]";
    urlToOpen += "&SubTitle=" + pSubTitle;
    ret = window.open(urlToOpen);
}

// EG 2020-09-30 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenTradeSplit(Guid) {ret = window.open("Split.aspx?GUID=" + Guid, "_blank");}

function OpenInstrumentSelection(GrpElement){
    ret=window.open("SelProduct.aspx?GrpElement=" + GrpElement,"ProductSelection",'fullscreen=no,height=800,width=600,location=no,scrollbars=no,status=no,resizable=yes');
}

// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenTradeEvents(Guid,action){ret=window.open("TradeEvents.aspx?GUID=" + Guid + '&Action=' + action ,"_blank");}
// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenTradeAdminEvents(Guid,action){ret=window.open("TradeAdminEvents.aspx?GUID=" + Guid + '&Action=' + action ,"_blank");}
// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenDebtSecurityEvents(Guid,action){ret=window.open("DebtSecEvents.aspx?GUID=" + Guid + '&Action=' + action ,"_blank");}
// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenTradeRiskEvents(Guid, action) {ret = window.open("TradeRiskEvents.aspx?GUID=" + Guid + '&Action=' + action, "_blank");}

// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenEars(Guid,action){ret=window.open("Ear.aspx?GUID=" + Guid + '&Action=' + action ,"_blank");}

// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenTradeTracks(Guid,action){ret=window.open("Tracks.aspx?GUID=" + Guid + '&Action=' + action ,"_blank");}

// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenTradeAction(Guid,action){ret=window.open("TradeAction.aspx?GUID=" + Guid + '&Action=' + action ,"_blank");}

// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenTradeAdminAction(Guid,action){ret=window.open("TradeAdminAction.aspx?GUID=" + Guid + '&Action=' + action ,"_blank");}

// EG 2020-10-02 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
function OpenObjectFpML(Guid,obj,elem,occur,copyTo,title,imgTitleRight,titleRight){
ret=window.open("CustomFpMLObject.aspx?GUID=" + Guid+ '&Object=' + obj + '&Element=' + elem + '&Occurence=' + occur + '&CopyTo=' + copyTo + '&Title=' + title + '&TitleRight=' + titleRight,"object");
}


// FI 2019-12-27 [25141] Add function LoadAutoCompleteData on trade
// FI 2020-01-07 [XXXXX] or-autocomplete (OpenRefential autocomplete)
function LoadAutoCompleteData() {
    $(".or-autocomplete").autocomplete({
        source: function (request, response) {
            var controlTxtId = $(this)[0].element[0].id; // Control with autocomplete 
            var controlBut = $get(controlTxtId.replace('TXT', 'BUT'));
            var openReferential = controlBut.attributes["onclick"].value;

            openReferential = OpenReferentialForAutocomplete(openReferential);

            $.ajax({
                url: "WebServices/ReferentialWebService.asmx/LoadAutoCompleteData",
                data: "{ 'request': '" + request.term + "','controlId': '" + controlTxtId + "','openReferential': '" + openReferential + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset = utf-8",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return {
                            value: item
                        }
                    }))
                },
                error: AutocompleteError
            });
        },
        minLength: 1,
        focus: function (event, ui) {
            //Triggered when focus is moved to an item(not selecting).
            //Text field's value is replace with the value of the focused item (event triggered by a keyboard interaction or mouse over).
            var controlTxtId = this.id; // Control with a autocomplete
            $("#" + controlTxtId).val(ui.item.value);
        }
    });
}
// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
// Mise à jour d'un libellé dans le Header d'un panel (toggle)
// - ctrl : contrôle source du libellé
// - target : contrôle cible 
function SetLinkIdToHeaderToggle(ctrl, target) {
    var ctrlvalue = ctrl.value;
    var color = "black";
    var bkgcolor = "white";
    // la source est un dropdown on récupère la valeur d'item sélectionné
    // et les styles associés (Forecolor et Background-color)
    if (ctrl.tagName.toUpperCase() == 'SELECT') {
        ctrlvalue = $(ctrl).find("option:selected").text();
        color = $(ctrl).find("option:selected").css("color");
        bkgcolor = $(ctrl).find("option:selected").css("background-color");
    }
    // la cible est le header du même panel que la source
    if ("default" == target) {
        var headerToggle = $(ctrl).parent().closest("div[class='contenth']").prev();
        if ($(headerToggle)) {
            var linkTitle = $(headerToggle).children(".headLinkInfo");
            if ($(linkTitle) == undefined) {
                var text = "<span class='headLinkInfo'>" + ctrlvalue + "</span>";
                $(headerToggle).append(text);
            }
            else {
                $(linkTitle).text(ctrlvalue);
            }
        }
    }
    // La cible est sur un autre panel (target = le nom de ce panel)
    else
    {
        SetStyleToLinkIdHeaderToggle(target, ctrlvalue, color, bkgcolor);
    }
}

// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
// Mise à jour d'un libellé et application des styles dans le Header d'un panel (toggle)
// - target : contrôle cible 
// - ctrlvalue : valeur du contrôle source
// - color : forecolor
// - bkgcolor : Backgroundcolor
function SetStyleToLinkIdHeaderToggle(target, ctrlvalue, color, bkgcolor) {
    var transparent = 'rgb(0, 0, 0)';
    var white = 'rgb(255, 255, 255)';
    var black = 'rgb(0, 0, 0)';
    var lblvalue = (ctrlvalue == 'RegularTrade' ? "" : ctrlvalue);

    if ((bkgcolor == transparent) && (color == transparent)) {
        bkgcolor = white;
        color = black;
    }
    if (bkgcolor == transparent) {
        bkgcolor = color;
        color = (color == white) ? black : white;
    }
    if (color == transparent) {
        color = (bkgcolor == black) ? white : black;
    }
    color = color + '!important';
    bkgcolor = bkgcolor + '!important';

    // Recherche de la cible et Mise à jour du libellé
    if ($("#" + target)) {
        $("#" + target).text(lblvalue);
        $("#" + target).attr("style", "background-color:" + bkgcolor + ";color:" + color + ";border-color:" + color + ";");
    }
}