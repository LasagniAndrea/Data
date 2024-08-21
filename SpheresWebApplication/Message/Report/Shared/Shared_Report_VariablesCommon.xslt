<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
					xmlns:dt="http://xsltsl.org/date-time"
					xmlns:fo="http://www.w3.org/1999/XSL/Format"
					xmlns:msxsl="urn:schemas-microsoft-com:xslt"
					version="1.0">

  <!--  
================================================================================================================
Summary : Spheres report - Shared - Common variables for all reports
          
File    : Shared_Report_VariablesCommon.xslt
================================================================================================================
Version : v3.7.5155                                           
Date    : 20140220
Author  : RD
Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking  
================================================================================================================
Version : v1.0.0.1 (Spheres 2.6.3.0)
Date    : 20120626
Author  : MF
Comment : added generic template variable for fee sorting  gcFeeDisplaySettings
================================================================================================================
Version : v1.0.0.2 (Spheres 2.6.3.0)
Date    : 20120003
Author  : MF
Comment :  
 - added gEndPeriodDate, end date of the account statement (saved into the //header/valueDate2 node)
 - added gReceiverBookNameWidthAccountStatement length for account statement header (account + period)
================================================================================================================
Version : v1.0.0." (Spheres 2.6.3.0)
Date    : 20120003
Author  : MF
Comment :  
 - added NA style into gcFeeDisplaySettings
 - new header book lenght gReceiverBookWidthAccountStatement for the account statement report
================================================================================================================
Version : v1.0.0.0 (Spheres 2.5.0.0)
Date    : 20100917
Author  : RD
Comment : First version
================================================================================================================    
  -->

  <!-- 20120625 MF moved from Shared_Report_BusinessETD.xslt -->
  <!-- ===== Payment types settings===== -->
  <!-- gcFeeDisplaySettings contains the sort order for each event fee type  -->
  <xsl:variable name="gcFeeDisplaySettings">
    <paymentType id="Bro" name="{$gcBro}" eventType="BRO" cuLabel="cuBro" sort="1"/>
    <paymentType id="BroTrd" name="{$gcBroTrd}" eventType="TBR" cuLabel="cuBroTrd" sort="2"/>
    <paymentType id="BroClr" name="{$gcBroClr}" eventType="CBR" cuLabel="cuBroClr" sort="3"/>
    <paymentType id="Fee" name="{$gcFee}" eventType="FEE" cuLabel="cuFee" sort="4"/>
    <paymentType id="FeeTrd" name="{$gcFeeTrd}" eventType="TFE" cuLabel="cuFeeTrd" sort="5"/>
    <paymentType id="FeeClr" name="{$gcFeeClr}" eventType="CFE" cuLabel="cuFeeClr" sort="6"/>

    <rule name="BRO_FEE">
      <paymentType id="Bro"/>
      <paymentType id="Fee"/>
    </rule>
    <rule name="BRO_TRDCLRFEE">
      <paymentType id="Bro"/>
      <paymentType id="FeeTrd"/>
      <paymentType id="FeeClr"/>
    </rule>
    <rule name="TRDCLRBRO_FEE">
      <paymentType id="BroTrd"/>
      <paymentType id="BroClr"/>
      <paymentType id="Fee"/>
    </rule>
    <rule name="TRDCLRBRO_TRDCLRFEE">
      <paymentType id="BroTrd"/>
      <paymentType id="BroClr"/>
      <paymentType id="FeeTrd"/>
      <paymentType id="FeeClr"/>
    </rule>
    <!-- MF 20120904 Ticket 18106 - the NA style must exist and it must be equals to an existent style
    if we want to build correctly the transaction report as well as the position actions report. We chose BRO_FEE as reference -->
    <rule name="NA">
      <paymentType id="Bro"/>
      <paymentType id="Fee"/>
    </rule>
  </xsl:variable>

  <!-- ================================================== -->
  <!--        Global Constantes                           -->
  <!-- ================================================== -->
  <xsl:variable name="gcIsBusinessDebugMode" select="false()"/>
  <xsl:variable name="gcIsDisplayDebugMode" select="false()"/>
  <xsl:variable name="gcIsModelMode" select="true()"/>
  <!--//-->
  <xsl:variable name="gcReportTypeALLOC" select="'ALLOC'"/>
  <xsl:variable name="gcReportTypeCSHBAL" select="'CSHBAL'"/>
  <xsl:variable name="gcReportTypePOS" select="'POS'"/>
  <xsl:variable name="gcReportTypePA" select="'PA'"/>
  <xsl:variable name="gcReportTypePOSSYNT" select="'POSSYNT'"/>
  <xsl:variable name="gcReportTypeACCSTAT" select="'ACCSTAT'"/>
  <xsl:variable name="gcReportTypeINTSTAT" select="'INTSTAT'"/>
  <xsl:variable name="gcReportTypeINV" select="'INV'"/>
  <xsl:variable name="gcReportTypeSYNTHESIS" select="'SYNTHESIS'"/>
  <!--//-->
  <xsl:variable name="gcNOkStatus" select="'*'"/>
  <xsl:variable name="gcPay" select="'p'"/>
  <xsl:variable name="gcRec" select="'r'"/>
  <xsl:variable name="gcCredit" select="'CR'"/>
  <xsl:variable name="gcDebit" select="'DR'"/>
  <!--//-->
  <xsl:variable name="gcBro" select="'Brokerage'"/>
  <xsl:variable name="gcBroTrd" select="'TradingBrokerage'"/>
  <xsl:variable name="gcBroClr" select="'ClearingBrokerage'"/>
  <xsl:variable name="gcFee" select="'Fee'"/>
  <xsl:variable name="gcFeeTrd" select="'TradingFee'"/>
  <xsl:variable name="gcFeeClr" select="'ClearingFee'"/>
  <!--//-->
  <xsl:variable name="gcNA" select="'X***'"/>
  <xsl:variable name="gcTableBorderDebug">
    <xsl:if test="$gcIsDisplayDebugMode=true()">
      <xsl:value-of select="'1pt solid blue'"/>
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="gcPageBorderDebug">
    <xsl:if test="$gcIsDisplayDebugMode=true()">
      <xsl:value-of select="'1pt solid red'"/>
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="gcTableBorderLegend" select="'0.5pt solid black'"/>
  <xsl:variable name="gcColorWhite">white</xsl:variable>
  <!-- Carriage return -->
  <xsl:variable name="gcLinefeed"> </xsl:variable>
  <!-- Espace character -->
  <xsl:variable name="gcEspace"> </xsl:variable>

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                         Global Variables                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->

  <!-- .................................. -->
  <!--   Common                           -->
  <!-- .................................. -->
  <xsl:variable name="gSendByRoutingIds" select="//sendBy/routingIdsAndExplicitDetails/routingIds"/>
  <xsl:variable name="gSendToRoutingIds" select="//sendTo/routingIdsAndExplicitDetails/routingIds"/>
  <xsl:variable name="gDataDocumentHeader" select="//header"/>
  <xsl:variable name="gCreationTimestamp" select="$gDataDocumentHeader/creationTimestamp" />
  <!--RD 20110908-->
  <!--Dans le cas du Report Positions détaillés, il nous faut la date de compensation à laquelle le traitement est demandé-->
  <!--En sachant que cette date (//header/valueDate) représente la date d'envoi, qui est égale à:
      Date de compensation à laquelle le traitement est demandé + un éventuel offset-->
  <xsl:variable name="gClearingDate" select="$gDataDocumentHeader/valueDate" />
  <!--<xsl:variable name="gClearingDate" select="//dataDocument/trade[1]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@BizDt"/>-->
  <!--//-->
  <!--RD 20120903 Ticket 18048 - end period date for the current account statement -->
  <xsl:variable name="gEndPeriodDate" select="$gDataDocumentHeader/valueDate2" />

  <xsl:variable name="gPrefixCurrencyId">CURRENCY.ISO4217_ALPHA3.</xsl:variable>

  <xsl:variable name="gDataDocumentParties" select="//dataDocument/party"/>
  <!--<xsl:variable name="gDataDocumentRepository" select="//dataDocument/repository"/>-->
  <xsl:variable name="gTradeSortingKeys">
    <xsl:choose>
      <xsl:when test="$gReportType = $gcReportTypeINV">
        <xsl:copy-of select="//invoiceTradeSorting/keys/key"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="//tradeSorting/keys/key"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gSortingKeys" select="msxsl:node-set($gTradeSortingKeys)/key"/>
  <xsl:variable name="gEvents" select="//events"/>

  <xsl:variable name="gMsgType" select="string($gDataDocumentHeader/confirmationMessageId[@confirmationMessageIdScheme='http://www.euro-finance-systems.fr/otcml/CNFMessageMSGType']/text())"/>
  <xsl:variable name="gIsMonoBook">
    <xsl:choose>
      <xsl:when test="$gReportType = $gcReportTypeINV">
        <xsl:value-of select="false()"/>
      </xsl:when>
      <xsl:when test="$gMsgType='MONO-TRADE'">
        <xsl:value-of select="true()"/>
      </xsl:when>
      <xsl:when test="$gMsgType='MULTI-TRADES'">
        <xsl:value-of select="true()"/>
      </xsl:when>
      <xsl:when test="$gMsgType='MULTI-PARTIES'">
        <xsl:value-of select="false()"/>
      </xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="gReportingCurrency">
    <xsl:choose>
      <xsl:when test="$gDataDocumentHeader/reportingCurrency">
        <xsl:value-of select="$gDataDocumentHeader/reportingCurrency"/>
      </xsl:when>
      <!-- JPN is for testing purpose, it will be replaced -->
      <xsl:otherwise>EUR</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gReportHeaderFooter" select="$gDataDocumentHeader/reportSettings/headerFooter"/>
  <xsl:variable name="gPosActions" select="//posActions/posAction"/>

  <!-- .................................. -->
  <!--   Entity                           -->
  <!-- .................................. -->
  <xsl:variable name="gEntityId">
    <xsl:value-of select="$gDataDocumentHeader/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/Actorid']/text()" />
  </xsl:variable>
  <!-- Get logo of the entity -->
  <xsl:variable name="gImgLogo">
    <xsl:value-of select="concat('sql(select IDENTIFIER, LOLOGO from dbo.ACTOR where IDA=', $gEntityId, ')')" />
  </xsl:variable>
  <!-- Phone of the entity -->
  <xsl:variable name="gEntityPhone">
    <xsl:value-of select="normalize-space($gSendByRoutingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telephoneNumber'])" />
  </xsl:variable>
  <!-- Fax of the entity -->
  <xsl:variable name="gEntityFax">
    <xsl:value-of select="normalize-space($gSendByRoutingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/faxNumber'])" />
  </xsl:variable>
  <!-- Telex of the entity -->
  <xsl:variable name="gEntityTelex">
    <xsl:value-of select="normalize-space($gSendByRoutingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telexNumber'])" />
  </xsl:variable>
  <!-- Mail of the entity -->
  <xsl:variable name="gEntityMail">
    <xsl:value-of select="normalize-space($gSendByRoutingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/email'])" />
  </xsl:variable>
  <xsl:variable name="gEntityIdentifier">
    <xsl:value-of select="normalize-space($gSendByRoutingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier'])" />
  </xsl:variable>
  <xsl:variable name="gEntityDisplayname">
    <xsl:value-of select="$gDataDocumentParties[@OTCmlId=$gEntityId]/partyName/text()"/>
  </xsl:variable>

  <!-- .................................. -->
  <!--   XSL Keys variables               -->
  <!-- .................................. -->
  <!-- keys -->
  <xsl:key name="kMarket" match="Instrmt" use="@Exch"/>
  <xsl:key name="kBookKey" match="BOOK" use="@data"/>
  <xsl:key name="kDC" match="Instrmt" use="@Sym"/>
  <xsl:key name="kBook" match="RptSide" use="@Acct"/>

  <xsl:key name="kMMY" match="Instrmt" use="@MMY"/>
  <xsl:key name="kPutCall" match="Instrmt" use="@PutCall"/>
  <xsl:key name="kStrkPx" match="Instrmt" use="@StrkPx"/>

  <xsl:key name="kDataValue" match="data" use="@value"/>

  <!-- ================================================== -->
  <!--   Statement page (A4 vertical) variables           -->
  <!-- ================================================== -->
  <!-- .................................. -->
  <!--   page caracteristics              -->
  <!-- .................................. -->
  <xsl:variable name="gPageA4VerticalPageHeight">297mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalPageWidth">210mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapePageHeight">210mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapePageWidth">297mm</xsl:variable>

  <xsl:variable name="gPageA4VerticalMarginTop">5mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalMarginBottom">5mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalMarginLeft">6mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalMarginRight">5mm</xsl:variable>

  <!-- .................................. -->
  <!--   page header                      -->
  <!-- .................................. -->
  <!--Size-->
  <xsl:variable name="gPageA4VerticalHeaderExtent">15mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalHeaderExtent0">0mm</xsl:variable>

  <xsl:variable name="gPageA4VerticalHeaderWidth">197mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapeHeaderWidth">284mm</xsl:variable>
  <!--Characteristics of the contents-->
  <xsl:variable name="gPageA4VerticalHeaderLeftWidth">60mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalHeaderCenterWidth">77mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalHeaderRightWidth">60mm</xsl:variable>

  <xsl:variable name="gPageA4LandscapeHeaderLeftWidth">60mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapeHeaderCenterWidth">164mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapeHeaderRightWidth">60mm</xsl:variable>

  <xsl:variable name="gPageA4VerticalHeaderLogoHeight">15mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalHeaderTextFontSize">16pt</xsl:variable>
  <xsl:variable name="gPageA4VerticalHeaderTextColor">gray</xsl:variable>
  <xsl:variable name="gPageA4VerticalHeaderDateFontSize">7pt</xsl:variable>
  <!-- .................................. -->
  <!--   page footer                      -->
  <!-- .................................. -->
  <!--Size-->
  <xsl:variable name="gPageA4VerticalFooterExtent">10mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalFooterExtentLarge1">15mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalFooterExtentLarge2">20mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalFooterExtentLarge3">25mm</xsl:variable>
  <!--Characteristics of the contents-->
  <xsl:variable name="gPageA4VerticalFooterWidth">197mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapeFooterWidth">284mm</xsl:variable>

  <xsl:variable name="gPageA4VerticalFooterPageNumberWidth">20mm</xsl:variable>

  <xsl:variable name="gPageA4VerticalFooterLeftWidth">50mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalFooterCenterWidth">97mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalFooterRightWidth">50mm</xsl:variable>

  <xsl:variable name="gPageA4LandscapeFooterLeftWidth">70mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapeFooterCenterWidth">144mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapeFooterRightWidth">70mm</xsl:variable>

  <xsl:variable name="gPageA4VerticalFooterTextFontSize">8</xsl:variable>
  <xsl:variable name="gPageA4VerticalFooterLegendTextFontSize">5</xsl:variable>
  <xsl:variable name="gPageA4VerticalFooterTextAlign">center</xsl:variable>
  <xsl:variable name="gPageA4VerticalFooterTextColor">black</xsl:variable>
  <!-- .................................. -->
  <!--   page body                        -->
  <!-- .................................. -->
  <!--Header Margins-->
  <xsl:variable name="gPageA4VerticalBodyMarginTop">20mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalBodyMarginTop0">5mm</xsl:variable>
  <!--Footer Margins-->
  <xsl:variable name="gPageA4VerticalBodyMarginBottom">16mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalBodyMarginBottomLarge1">21mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalBodyMarginBottomLarge2">26mm</xsl:variable>
  <xsl:variable name="gPageA4VerticalBodyMarginBottomLarge3">31mm</xsl:variable>
  <!--Other Margins-->
  <xsl:variable name="gPageA4VerticalBodyMargin">0mm</xsl:variable>
  <!-- Main Title -->
  <xsl:variable name="gMainTitleWidth">197mm</xsl:variable>
  <xsl:variable name="gMainTitlePaddingTop">5mm</xsl:variable>
  <xsl:variable name="gMainTitleLeftMargin">0mm</xsl:variable>
  <xsl:variable name="gMainTitleFontSize">16pt</xsl:variable>
  <xsl:variable name="gMainTitleFontWeight">bold</xsl:variable>
  <xsl:variable name="gMainTitleTextAlign">center</xsl:variable>
  <!-- receiver -->
  <xsl:variable name="gReceiverWidth">197mm</xsl:variable>
  <xsl:variable name="gReceiverPaddingTop">10mm</xsl:variable>
  <!-- receiver address -->
  <xsl:variable name="gReceiverAdressWidth">80mm</xsl:variable>
  <xsl:variable name="gReceiverAdressLeftMargin">0mm</xsl:variable>
  <!-- virtual variable -->
  <xsl:variable name="gReceiverAdressFontFamily">Courier</xsl:variable>
  <xsl:variable name="gReceiverAdressFontSize">11pt</xsl:variable>
  <xsl:variable name="gReceiverAdressTextAlign">left</xsl:variable>
  <!-- receiver Book -->
  <xsl:variable name="gReceiverBookWidth">117mm</xsl:variable>
  <xsl:variable name="gReceiverBookIdentWidth">40mm</xsl:variable>
  <xsl:variable name="gReceiverBookNameWidth">157mm</xsl:variable>

  <xsl:variable name="gReceiverBookPaddingTop">10mm</xsl:variable>
  <xsl:variable name="gReceiverBookLeftMargin">0mm</xsl:variable>
  <xsl:variable name="gReceiverBookFontSize">10pt</xsl:variable>
  <xsl:variable name="gReceiverBookFontSizeAccount">10pt</xsl:variable>
  <xsl:variable name="gReceiverBookFontSizeDisplayname">8pt</xsl:variable>
  <xsl:variable name="gReceiverBookFontSizeCurrency">8pt</xsl:variable>
  <xsl:variable name="gReceiverBookTextAlign">left</xsl:variable>
  <!-- Introduction Title -->
  <xsl:variable name="gPageA4VerticalFormulaWidth">197mm</xsl:variable>
  <xsl:variable name="gPageA4LandscapeFormulaWidth">284mm</xsl:variable>
  <xsl:variable name="gFormulaPaddingTop">5mm</xsl:variable>
  <xsl:variable name="gFormulaLeftMargin">0mm</xsl:variable>
  <xsl:variable name="gFormulaFontSize">7pt</xsl:variable>
  <xsl:variable name="gFormulaFontWeight">normal</xsl:variable>
  <xsl:variable name="gFormulaTextAlign">justify</xsl:variable>
  <!-- .................................. -->
  <!--   page body : Detailed Trade       -->
  <!-- .................................. -->
  <xsl:variable name="gDetTrdTablePaddingTop">2mm</xsl:variable>
  <xsl:variable name="gGroupPaddingTop">5mm</xsl:variable>
  <xsl:variable name="gGroupTotalPaddingTop">5mm</xsl:variable>
  <xsl:variable name="gTitlePaddingBottom">2mm</xsl:variable>
  <xsl:variable name="gDetTrdCellPadding">0.15mm</xsl:variable>
  <xsl:variable name="gDetTrdCellBorder">0.3pt solid black</xsl:variable>
  <xsl:variable name="gDetTrdFontSize">7pt</xsl:variable>
  <xsl:variable name="gDetTrdDetFontSize">5pt</xsl:variable>
  <xsl:variable name="gDetTrdDetFontSizeTime">4.5pt</xsl:variable>
  <xsl:variable name="gDetTrdStarFontSize">10pt</xsl:variable>
  <xsl:variable name="gDetTrdTotalBackgroundColor">#c3c3c3</xsl:variable>
  <xsl:variable name="gDetTrdRowAfter">2mm</xsl:variable>
  <!--Table Header-->
  <xsl:variable name="gDetTrdHdrFontSize">7pt</xsl:variable>
  <xsl:variable name="gDetTrdHdrFontWeight">normal</xsl:variable>
  <xsl:variable name="gDetTrdHdrTextAlign">center</xsl:variable>
  <xsl:variable name="gDetTrdHdrCellBorder">0.3pt solid black</xsl:variable>
  <xsl:variable name="gDetTrdHdrCellBorderTopStyle">solid</xsl:variable>
  <xsl:variable name="gDetTrdHdrCellBackgroundColor">#e3e3e3</xsl:variable>
  <!--Table Header Data-->
  <xsl:variable name="gDetTrdHdrDataFontSize">5pt</xsl:variable>
  <xsl:variable name="gDetTrdHdrDataFontWeight">normal</xsl:variable>
  <xsl:variable name="gDetTrdHdrDataCellBorderTopStyle">none</xsl:variable>
  <!--Table Header Group-->
  <xsl:variable name="gDetTrdHdrGrpTextAlign">left</xsl:variable>
  <!--Table Header Group Data-->
  <xsl:variable name="gDetTrdHdrGrpDataFontSize">7pt</xsl:variable>
  <xsl:variable name="gDetTrdHdrGrpDataFontWeight">bold</xsl:variable>
  <xsl:variable name="gDetTrdHdrGrpDataCellBorderTopStyle">solid</xsl:variable>
  <xsl:variable name="gDetTrdHdrGrpDataCellBackgroundColor">#ffffff</xsl:variable>

  <!--//-->
  <!-- Admitted values: Times Roman, Helvetica, and Courier -->
  <xsl:variable name="gFontFamily">Helvetica</xsl:variable>
  <xsl:variable name="gCopyrightPowerdBy">Powered by </xsl:variable>
  <xsl:variable name="gCopyrightSpheres">Spheres - © 2023 EFS</xsl:variable>
  <xsl:variable name="gCopyright">
    <xsl:value-of select="concat($gCopyrightPowerdBy,$gCopyrightSpheres)"/>
  </xsl:variable>

</xsl:stylesheet>