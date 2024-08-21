<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:fn="http://www.w3.org/2005/xpath-functions"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres reports - OTC Synthesis Report                                               -->
  <!-- File    : \Report_v2\OTCSynthesisReport.xsl                                                    -->
  <!-- ============================================================================================== -->
  <!-- Version : v5.1.6101                                                                            -->
  <!-- Date    : 20160914                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : [224553] Affichage de l'identifiant du trade avec une petite police en fonction de sa longeur  -->
  <!-- ============================================================================================== -->
  <!-- Version : v5.0.5738                                                                            -->
  <!-- Date    : 20150917                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : [21376] Correction bug affichage "Expiry Indicator" (voir appel template "GetExpiry-color")  -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.5.5491                                                                            -->
  <!-- Date    : 20150116                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Settings                                          -->
  <!-- ============================================================================================== -->
  <!--Business-->
  <xsl:import href="Shared\Shared_Report_v2_Business.xslt" />

  <xsl:import href="ESE\EST_Report_v2_Business.xslt" />
  <xsl:import href="LSD\ETD_Report_v2_Business.xslt" />
  <xsl:import href="DSE\DST_Report_v2_Business.xslt" />
  <xsl:import href="RTS\RTS_Report_v2_Business.xslt" />
  <xsl:import href="FX\FxLeg_Report_v2_Business.xslt" />
  <xsl:import href="FX\FxOptionLeg_Report_v2_Business.xslt" />
  <xsl:import href="COMS\CommoditySpot_Report_v2_Business.xslt" />

  <!--Report Format-->
  <xsl:import href="Shared\Shared_Report_v2_A4Vertical.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_UK.xslt" />

  <!--Tools-->
  <xsl:import href="Shared\Shared_Report_v2_Tools.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_FixmlTools.xslt" />

  <xsl:import href="LSD\ETD_Report_v2_Tools.xslt" />
  <xsl:import href="ESE\EST_Report_v2_Tools.xslt" />
  <xsl:import href="DSE\DST_Report_v2_Tools.xslt" />
  <xsl:import href="RTS\RTS_Report_v2_Tools.xslt" />
  <xsl:import href="FX\FxLeg_Report_v2_Tools.xslt" />
  <xsl:import href="FX\FxOptionLeg_Report_v2_Tools.xslt" />
  <xsl:import href="COMS\CommoditySpot_Report_v2_Tools.xslt" />

  <xsl:import href="Shared\Shared_Report_v2_Variables.xslt" />

  <!--Parameters-->
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>
  <!--<xsl:param name="pPathResource" select="'..\..\..\..\Resource'" />-->
  <xsl:param name="pCurrentCulture" select="'en-GB'" />
  <xsl:param name="pDebugMode" select="'None'"/>
  <!-- FI 20150921 [21311] => Paramètre qui permet de réduire l'affichage à certaines sections  
  Exemple:TRD,ACC => Ds ce cas affichage uniquement des trades jour et de la situation financière   -->
  <xsl:param name="pBizSection" select="'ALL'"/>
  <!--<xsl:param name="pBizSection" select="$gcReportSectionKey_ABN"/>-->

  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gReportType" select="$gcReportTypeSYNTHESIS"/>
  <!--To add Spécific Timezone-->
  <!--<xsl:variable name="gHeaderTimezone" select="$gSendBy-routingAddress/city"/>-->

  <!--TODO: manage all sections and "sort" attribute-->
  <!--  FI 20160530 [21885] Modify -->
  <xsl:variable name="gReportSections_Node">
    <section name="tradesConfirmations" sort="1" optional="{false()}"/>
    <section name="amendmentTransfers" sort="2"/>
    <section name="liquidations" sort="3"/>
    <section name="exeAss" sort="4"/>
    <section name="abandonments" sort="5"/>
    <section name="purchaseSale" sort="6"/>
    <section name="unsettledTrades" sort="7"/>
    <section name="settledTrades" sort="8"/>
    <section name="closedPositions" sort="9">
      <!--VCL: don't display section "closedPositions" for "Equity"-->
      <!--<product name="equitySecurityTransaction"/>-->
    </section>
    <section name="openPosition" sort="10" optional="{false()}"/>
    <section name="stlClosedPositions" sort="11">
      <!--VCL: don't display section "stlClosedPositions" for "Equity"-->
      <!--<product name="equitySecurityTransaction"/>-->
    </section>
    <section name="stlOpenPosition" sort="12" optional="{false()}">
      <product name="equitySecurityTransaction"/>
      <product name="debtSecurityTransaction"/>
    </section>
    <section name="journalEntries" sort="13"/>
    <!-- FI 20160530 [21885] Add-->
    <section name="collaterals" sort="14"/>
    <!-- FI 20160613 [22256] Add-->
    <section name="underlyingStocks" sort="15"/>
    <section name="accountSummary" sort="16"/>
  </xsl:variable>
  <xsl:variable name="gReportSections" select="msxsl:node-set($gReportSections_Node)/section"/>

  <!-- ============================================================================================== -->
  <!--                                              Template                                          -->
  <!-- ============================================================================================== -->
  <!-- .......................................................................... -->
  <!--              Main                                                          -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Main Template                                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  First called template                           -->
  <!-- ................................................ -->
  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <!-- .......................................................................... -->
      <!--              Page models                                                   -->
      <!-- .......................................................................... -->
      <fo:layout-master-set>
        <!--Define "A4Vertical" model -->
        <xsl:call-template name="A4Vertical"/>

        <!--Define "A4Vertical-Synthesis" page sequence -->
        <fo:page-sequence-master master-name="A4Vertical-Synthesis">
          <fo:repeatable-page-master-alternatives>
            <fo:conditional-page-master-reference master-reference="A4Vertical"/>
          </fo:repeatable-page-master-alternatives>
        </fo:page-sequence-master>
      </fo:layout-master-set>

      <!-- .......................................................................... -->
      <!--              Page Content                                                   -->
      <!-- .......................................................................... -->
      <fo:page-sequence master-reference="A4Vertical-Synthesis" font-family="{$gFontFamily}">
        <!-- .......................................................................... -->
        <!--              Page Header                                                   -->
        <!-- .......................................................................... -->
        <fo:static-content flow-name="A4VerticalHeader" display-align="before" >
          <xsl:call-template name="Debug_border-red"/>

          <fo:block display-align="before" linefeed-treatment="preserve">
            <xsl:call-template name="A4VerticalHeader"/>
          </fo:block>
        </fo:static-content>
        <!-- .......................................................................... -->
        <!--              Page Footer                                                   -->
        <!-- .......................................................................... -->
        <fo:static-content flow-name="A4VerticalFooter" display-align="after">
          <xsl:call-template name="Debug_border-red"/>

          <fo:block display-align="after" linefeed-treatment="preserve">
            <xsl:call-template name="A4VerticalFooter"/>
          </fo:block>
        </fo:static-content>
        <!-- .......................................................................... -->
        <!--              Page Content                                                  -->
        <!-- .......................................................................... -->
        <fo:flow flow-name="A4VerticalBody">
          <xsl:call-template name="Debug_border-red"/>
          <xsl:call-template name="Synthesis_Content"/>
        </fo:flow>
      </fo:page-sequence>
    </fo:root>
  </xsl:template>


  <!-- .......................................................................... -->
  <!--              Synthesis                                                     -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_Content                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis content                       -->
  <!-- ................................................ -->
  <!-- FI 20160530 [21885] Modify -->
  <xsl:template name="Synthesis_Content">

    <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
    <!--              Business Data                     -->
    <!--Ordre d’affichage (en dur pour l’instant):
    -	ETD
    -	EQUITY
    - DEBTSECURITY
    -	CFD EQUITY
    -	CFD FOREX
    -	FXOPTION
    -	FXFORWARD NDF
    -	FXFORWARD
    -	COMMODITY SPOT-->
    <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->

    <!--1/ Section: TradesConfirmations -->
    <xsl:variable name ="vBizTRD">
      <xsl:choose>
        <xsl:when test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_TRD)">
          <xsl:value-of select ="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vTradesConfirmations_subTotal-predicates" select="$gDailyTrades[subTotal[predicates/predicate/@trdTyp]]"/>

    <xsl:variable name="vBizETDTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$vTradesConfirmations_subTotal-predicates" mode="BizETD_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vBizESTTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$vTradesConfirmations_subTotal-predicates" mode="BizEST_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vBizDSTTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$vTradesConfirmations_subTotal-predicates" mode="BizDST_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vBizCFDEquityTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$vTradesConfirmations_subTotal-predicates" mode="BizCFDEquity_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCFDForexTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$vTradesConfirmations_subTotal-predicates" mode="BizCFDForex_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vBizFxNDOTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$gDailyTrades" mode="BizFxOptionLeg_Trade"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizFxNDFTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$gDailyTrades" mode="BizFxNDF_Trade"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizFxForwardTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$gDailyTrades" mode="BizFxForward_Trade"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizFxBullionTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$gDailyTrades" mode="BizFxBullion_Trade"/>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vBizCOMSTradesConfirmations_Node">
      <xsl:if  test ="$vBizTRD = 1">
        <xsl:apply-templates select="$vTradesConfirmations_subTotal-predicates" mode="BizCOMS_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>


    <!--2/ Section: AmendmentTransfers -->
    <!-- 
      - POC : Position correction
      - POT : Position transfer
      - LateTrade(4): Trades saisis en retard
      - PortFolioTransfer(42): Trades issus d'un transfert
      - TechnicalTrade(63): Trades généralement générés lorsque POSREQUEST = 'CLOSINGPOS'
        ................................................ -->
    <xsl:variable name ="vBizAMT">
      <xsl:choose>
        <xsl:when test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_AMT)">
          <xsl:value-of select ="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- FI 20231222 [WI788] add TechnicalTrade Trade  -->
    <xsl:variable name="vAmendmentTransfers" select="$gPosActions[posAction[contains(',POC,POT,',concat(',',@requestType,','))]] | 
                  $gDailyTrades[trade[contains(',4,42,',concat(',',@trdTyp,','))]] |
                  $gDailyTrades[trade[@trdTyp='63' and (@posResult='Close' or @posResult='Partial Close')]]"/>

    <xsl:variable name="vBizETDAmendmentTransfers_Node">
      <xsl:if  test ="$vBizAMT = 1">
        <xsl:apply-templates select="$vAmendmentTransfers" mode="BizETD_AmendmentTransfers"/>
      </xsl:if>
    </xsl:variable>
    <!-- FI 20150827 [21287] add vBizESTAmendmentTransfers_Node -->
    <xsl:variable name="vBizESTAmendmentTransfers_Node">
      <xsl:if  test ="$vBizAMT = 1">
        <xsl:apply-templates select="$vAmendmentTransfers" mode="BizEST_AmendmentTransfers"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizDSTAmendmentTransfers_Node">
      <xsl:if  test ="$vBizAMT = 1">
        <xsl:apply-templates select="$vAmendmentTransfers" mode="BizDST_AmendmentTransfers"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCFDEquityAmendmentTransfers_Node">
      <xsl:if  test ="$vBizAMT = 1">
        <xsl:apply-templates select="$vAmendmentTransfers" mode="BizCFDEquity_AmendmentTransfers"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCFDForexAmendmentTransfers_Node">
      <xsl:if  test ="$vBizAMT = 1">
        <xsl:apply-templates select="$vAmendmentTransfers" mode="BizCFDForex_AmendmentTransfers"/>
      </xsl:if>
    </xsl:variable>

    <!--3/ Section: Cascading -->
    <!-- CAS : Cascading-->
    <!-- SHI : Shifting -->
    <xsl:variable name="vBizETDCascading_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_CAS)">
        <xsl:apply-templates select="$gPosActions[posAction[contains(',CAS,SHI,',concat(',',@requestType,','))]]"
                             mode="BizETD_Cascading"/>
      </xsl:if>
    </xsl:variable>

    <!--4/ Section: Liquidations -->
    <!-- LIQ : Liquidation de future -->
    <xsl:variable name="vBizETDLiquidations_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_LIQ)">
        <xsl:apply-templates select="$gPosActions[posAction[@requestType='MOF']]"
                             mode="BizETD_Liquidations"/>
      </xsl:if>
    </xsl:variable>

    <!--5/ Section: Deliveries -->
    <xsl:variable name="vBizETDDeliveries_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_DLV)">
        <xsl:apply-templates select="$gDlvTrades" mode="BizETD_Deliveries"/>
      </xsl:if>
    </xsl:variable>

    <!--6/ Section: CorporateAction -->
    <xsl:variable name="vBizETDCorporateAction_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_CA)">
        <xsl:apply-templates select="$gPosActions[posAction[contains(',CA,',concat(',',@requestType,','))]]"
                             mode="BizETD_CorporateAction"/>
      </xsl:if>
    </xsl:variable>

    <!--7/ Section: ExeAss -->
    <!-- EXE, AUTOEXE, ASS et AUTOASS -->
    <xsl:variable name="vBizETDExeAss_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_EXA)">
        <xsl:apply-templates select="$gPosActions[posAction[contains(',EXE,AUTOEXE,ASS,AUTOASS,',concat(',',@requestType,','))]]"
                             mode="BizETD_ExeAss"/>
      </xsl:if>
    </xsl:variable>

    <!--8/ Section: Abandonments -->
    <!-- RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
    <!-- ABN, NEX, NAS, AUTOABN -->
    <xsl:variable name="vBizETDAbandonments_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_ABN)">
        <xsl:apply-templates select="$gPosActions[posAction[contains(',ABN,NEX,NAS,AUTOABN,',concat(',',@requestType,','))]]"
                             mode="BizETD_Abandonments"/>
      </xsl:if>
    </xsl:variable>

    <!--9/ Section: PurchaseSale -->
    <!-- - UNCLEARING: Compensation générée suite à une décompensation partielle 
        - CLEARSPEC : Compensation  Spécifique 
        - CLEARBULK : Compensation globale 
        - CLEAREOD : Compensation fin de journée 
        - ENTRY : Clôture STP 
        - UPDENTRY : Clôture Fin de journée 
        - Hors Décompensation  -->
    <xsl:variable name ="vBizPSS">
      <xsl:choose>
        <xsl:when test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_PSS)">
          <xsl:value-of select ="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vBizETDPurchaseSales_Node">
      <xsl:if  test ="$vBizPSS = 1">
        <xsl:apply-templates select="$gPosActions" mode="BizETD_PurchaseSale"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCFDEquityPurchaseSales_Node">
      <xsl:if  test ="$vBizPSS = 1">
        <xsl:apply-templates select="$gPosActions" mode="BizCFDEquity_PurchaseSale"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCFDForexPurchaseSales_Node">
      <xsl:if  test ="$vBizPSS = 1">
        <xsl:apply-templates select="$gPosActions" mode="BizCFDForex_PurchaseSale"/>
      </xsl:if>
    </xsl:variable>

    <!--10/ Section: UnsettledTrades -->
    <xsl:variable name="vBizESTUnsettledTrades_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_UNS)">
        <xsl:apply-templates select="$gUnsettledTrades" mode="BizEST_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_UNS"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizDSTUnsettledTrades_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_UNS)">
        <xsl:apply-templates select="$gUnsettledTrades" mode="BizDST_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_UNS"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCOMSUnsettledTrades_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_UNS)">
        <xsl:apply-templates select="$gUnsettledTrades" mode="BizCOMS_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_UNS"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>


    <!--11/ Section: SettledTrades -->
    <xsl:variable name="vBizESTSettledTrades_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_SET)">
        <xsl:apply-templates select="$gSettledTrades" mode="BizEST_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_SET"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizDSTSettledTrades_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_SET)">
        <xsl:apply-templates select="$gSettledTrades" mode="BizDST_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_SET"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCOMSSettledTrades_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_SET)">
        <xsl:apply-templates select="$gSettledTrades" mode="BizCOMS_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_SET"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>

    <!--12/ Section: ClosedPositions -->
    <xsl:variable name="vBizESTClosedPositions_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_CLO)">
        <xsl:variable name="vSection_ClosedPositions" select="$gReportSections[@name='closedPositions']"/>
        <xsl:if test="$vSection_ClosedPositions[product[@name='equitySecurityTransaction']]">
          <xsl:apply-templates select="$gPosActions" mode="BizEST_PurchaseSales"/>
        </xsl:if>
      </xsl:if>
    </xsl:variable>

    <!--13 Section: OpenPosition -->
    <xsl:variable name ="vBizPOS">
      <xsl:choose>
        <xsl:when test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_POS)">
          <xsl:value-of select ="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vBizETDOpenPosition_Node">
      <xsl:if  test ="$vBizPOS = 1">
        <xsl:apply-templates select="$gPosTrades" mode="BizETD_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vBizESTOpenPosition_Node">
      <xsl:if  test ="$vBizPOS = 1">
        <!-- FI 20150716 [20892] pas de position business pour les EST (Uniquement des positions en settlement)=> Mise en commentaire
      <xsl:apply-templates select="$gPosTrades" mode="BizEST_TradeSubTotal">
        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
      </xsl:apply-templates>
      -->
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCFDEquityOpenPosition_Node">
      <xsl:if  test ="$vBizPOS = 1">
        <xsl:apply-templates select="$gPosTrades" mode="BizCFDEquity_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizCFDForexOpenPosition_Node">
      <xsl:if  test ="$vBizPOS = 1">
        <xsl:apply-templates select="$gPosTrades" mode="BizCFDForex_TradeSubTotal">
          <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizFxNDOOpenPosition_Node">
      <xsl:if  test ="$vBizPOS = 1">
        <xsl:apply-templates select="$gPosTrades" mode="BizFxOptionLeg_Trade"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizFxNDFOpenPosition_Node">
      <xsl:if  test ="$vBizPOS = 1">
        <xsl:apply-templates select="$gPosTrades" mode="BizFxNDF_Trade"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizFxForwardOpenPosition_Node">
      <xsl:if  test ="$vBizPOS = 1">
        <xsl:apply-templates select="$gPosTrades" mode="BizFxForward_Trade"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizFxBullionOpenPosition_Node">
      <xsl:if  test ="$vBizPOS = 1">
        <xsl:apply-templates select="$gPosTrades" mode="BizFxBullion_Trade"/>
      </xsl:if>
    </xsl:variable>

    <!--14/ Section: StlClosedPositions -->
    <xsl:variable name="vBizESTStlClosedPositions_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_CST)">
        <xsl:variable name="vSection_StlClosedPositions" select="$gReportSections[@name='closedPositionsStl']"/>
        <xsl:if test="$vSection_StlClosedPositions[product[@name='equitySecurityTransaction']]">
          <xsl:apply-templates select="$gStlPosActions" mode="BizEST_PurchaseSalesx"/>
        </xsl:if>
      </xsl:if>
    </xsl:variable>

    <!--15/ Section: StlOpenPosition -->
    <xsl:variable name="vBizESTStlOpenPosition_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_STL)">
        <xsl:variable name="vSection_StlOpenPosition" select="$gReportSections[@name='stlOpenPosition']"/>
        <xsl:if test="$vSection_StlOpenPosition[product[@name='equitySecurityTransaction']]">
          <xsl:apply-templates select="$gStlPosTrades" mode="BizEST_TradeSubTotal">
            <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_STL"/>
          </xsl:apply-templates>
        </xsl:if>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vBizDSTStlOpenPosition_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_STL)">
        <xsl:variable name="vSection_StlOpenPosition" select="$gReportSections[@name='stlOpenPosition']"/>
        <xsl:if test="$vSection_StlOpenPosition[product[@name='debtSecurityTransaction']]">
          <xsl:apply-templates select="$gStlPosTrades" mode="BizDST_TradeSubTotal">
            <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_STL"/>
          </xsl:apply-templates>
        </xsl:if>
      </xsl:if>
    </xsl:variable>


    <!--16/ Section JournalEntries -->
    <xsl:variable name="vBizJournalEntries_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_JNL)">
        <xsl:call-template name="Business_JournalEntries-book"/>
      </xsl:if>
    </xsl:variable>

    <!-- FI 20160530 [21885] Add -->
    <!--17/ Section Collateral (securities on deposit) -->
    <xsl:variable name="vBizCollateral_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_SOD)">
        <xsl:call-template name="Business_Collaterals-book"/>
      </xsl:if>
    </xsl:variable>

    <!-- FI 20160613 [22256] Add -->
    <!--18/ Section UnderlyingStocks (underlying stocks on deposit) -->
    <xsl:variable name="vBizUnderlyingStock_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_UOD)">
        <xsl:call-template name="Business_UnderlyingStocks-book"/>
      </xsl:if>
    </xsl:variable>

    <!--19/ Section: AccountSummary -->
    <xsl:variable name="vBizAccountSummary_Node">
      <xsl:if test ="$pBizSection= 'ALL' or contains($pBizSection,$gcReportSectionKey_ACC)">
        <xsl:call-template name="Business_AccountSummary-group"/>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vBizETDTradesConfirmations" select="msxsl:node-set($vBizETDTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizESTTradesConfirmations" select="msxsl:node-set($vBizESTTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizDSTTradesConfirmations" select="msxsl:node-set($vBizDSTTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizCFDEquityTradesConfirmations" select="msxsl:node-set($vBizCFDEquityTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizCFDForexTradesConfirmations" select="msxsl:node-set($vBizCFDForexTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizFxNDOTradesConfirmations" select="msxsl:node-set($vBizFxNDOTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizFxNDFTradesConfirmations" select="msxsl:node-set($vBizFxNDFTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizFxForwardTradesConfirmations" select="msxsl:node-set($vBizFxForwardTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizFxBullionTradesConfirmations" select="msxsl:node-set($vBizFxBullionTradesConfirmations_Node)/date"/>
    <xsl:variable name="vBizCOMSTradesConfirmations" select="msxsl:node-set($vBizCOMSTradesConfirmations_Node)/date"/>

    <xsl:variable name="vBizETDAmendmentTransfers" select="msxsl:node-set($vBizETDAmendmentTransfers_Node)/date"/>
    <xsl:variable name="vBizESTAmendmentTransfers" select="msxsl:node-set($vBizESTAmendmentTransfers_Node)/date"/>
    <xsl:variable name="vBizDSTAmendmentTransfers" select="msxsl:node-set($vBizDSTAmendmentTransfers_Node)/date"/>
    <xsl:variable name="vBizCFDEquityAmendmentTransfers" select="msxsl:node-set($vBizCFDEquityAmendmentTransfers_Node)/date"/>
    <xsl:variable name="vBizCFDForexAmendmentTransfers" select="msxsl:node-set($vBizCFDForexAmendmentTransfers_Node)/date"/>

    <xsl:variable name="vBizETDCascading" select="msxsl:node-set($vBizETDCascading_Node)/date"/>
    <xsl:variable name="vBizETDLiquidations" select="msxsl:node-set($vBizETDLiquidations_Node)/date"/>
    <xsl:variable name="vBizETDDeliveries" select="msxsl:node-set($vBizETDDeliveries_Node)/date"/>
    <xsl:variable name="vBizETDCorporateAction" select="msxsl:node-set($vBizETDCorporateAction_Node)/date"/>
    <xsl:variable name="vBizETDExeAss" select="msxsl:node-set($vBizETDExeAss_Node)/date"/>
    <xsl:variable name="vBizETDAbandonments" select="msxsl:node-set($vBizETDAbandonments_Node)/date"/>

    <xsl:variable name="vBizETDPurchaseSales" select="msxsl:node-set($vBizETDPurchaseSales_Node)/date"/>
    <xsl:variable name="vBizCFDEquityPurchaseSales" select="msxsl:node-set($vBizCFDEquityPurchaseSales_Node)/date"/>
    <xsl:variable name="vBizCFDForexPurchaseSales" select="msxsl:node-set($vBizCFDForexPurchaseSales_Node)/date"/>

    <xsl:variable name="vBizESTUnsettledTrades" select="msxsl:node-set($vBizESTUnsettledTrades_Node)/date"/>
    <xsl:variable name="vBizDSTUnsettledTrades" select="msxsl:node-set($vBizDSTUnsettledTrades_Node)/date"/>
    <xsl:variable name="vBizCOMSUnsettledTrades" select="msxsl:node-set($vBizCOMSUnsettledTrades_Node)/date"/>

    <xsl:variable name="vBizESTSettledTrades" select="msxsl:node-set($vBizESTSettledTrades_Node)/date"/>
    <xsl:variable name="vBizDSTSettledTrades" select="msxsl:node-set($vBizDSTSettledTrades_Node)/date"/>
    <xsl:variable name="vBizCOMSSettledTrades" select="msxsl:node-set($vBizCOMSSettledTrades_Node)/date"/>

    <xsl:variable name="vBizESTClosedPositions" select="msxsl:node-set($vBizESTClosedPositions_Node)/date"/>

    <xsl:variable name="vBizETDOpenPosition" select="msxsl:node-set($vBizETDOpenPosition_Node)/date"/>
    <xsl:variable name="vBizESTOpenPosition" select="msxsl:node-set($vBizESTOpenPosition_Node)/date"/>
    <xsl:variable name="vBizCFDEquityOpenPosition" select="msxsl:node-set($vBizCFDEquityOpenPosition_Node)/date"/>
    <xsl:variable name="vBizCFDForexOpenPosition" select="msxsl:node-set($vBizCFDForexOpenPosition_Node)/date"/>
    <xsl:variable name="vBizFxNDOOpenPosition" select="msxsl:node-set($vBizFxNDOOpenPosition_Node)/date"/>
    <xsl:variable name="vBizFxNDFOpenPosition" select="msxsl:node-set($vBizFxNDFOpenPosition_Node)/date"/>
    <xsl:variable name="vBizFxForwardOpenPosition" select="msxsl:node-set($vBizFxForwardOpenPosition_Node)/date"/>
    <xsl:variable name="vBizFxBullionOpenPosition" select="msxsl:node-set($vBizFxBullionOpenPosition_Node)/date"/>

    <xsl:variable name="vBizESTStlClosedPositions" select="msxsl:node-set($vBizESTStlClosedPositions_Node)/date"/>

    <xsl:variable name="vBizESTStlOpenPosition" select="msxsl:node-set($vBizESTStlOpenPosition_Node)/date"/>
    <xsl:variable name="vBizDSTStlOpenPosition" select="msxsl:node-set($vBizDSTStlOpenPosition_Node)/date"/>

    <xsl:variable name="vBizJournalEntries" select="msxsl:node-set($vBizJournalEntries_Node)/book"/>
    <!-- FI 20160530 [21885] Add vBizCollateral-->
    <xsl:variable name="vBizCollateral" select="msxsl:node-set($vBizCollateral_Node)/book"/>
    <!-- FI 20160613 [22256] Add vBizUnderlyingStock-->
    <xsl:variable name="vBizUnderlyingStock" select="msxsl:node-set($vBizUnderlyingStock_Node)/book"/>

    <xsl:variable name="vBizAccountSummary" select="msxsl:node-set($vBizAccountSummary_Node)/group"/>

    <!--Activity Stat variables -->
    <xsl:variable name="vActivityCount_TradesConfirmations" select="count($vBizETDTradesConfirmations[1] | $vBizESTTradesConfirmations[1] | $vBizDSTTradesConfirmations[1] |
                  $vBizCFDEquityTradesConfirmations[1] | $vBizCFDForexTradesConfirmations[1] | 
                  $vBizFxNDOTradesConfirmations[1] | $vBizFxNDFTradesConfirmations[1] | $vBizFxForwardTradesConfirmations[1] | $vBizFxBullionTradesConfirmations[1] | $vBizCOMSTradesConfirmations[1])"/>
    <xsl:variable name="vActivityCount_OpenPosition" select="count($vBizETDOpenPosition[1] | $vBizESTOpenPosition[1] | 
                  $vBizCFDEquityOpenPosition[1] | $vBizCFDForexOpenPosition[1] | 
                  $vBizFxNDOOpenPosition[1] | $vBizFxNDFOpenPosition[1] | $vBizFxForwardOpenPosition[1] | $vBizFxBullionOpenPosition[1])"/>

    <xsl:variable name="vActivityCount_StlOpenPosition" select="count($vBizESTStlOpenPosition[1] | $vBizDSTStlOpenPosition[1])"/>

    <xsl:variable name="vActivity_ETD">
      <xsl:choose>
        <xsl:when test="count($vBizETDTradesConfirmations[1] | 
                  $vBizETDAmendmentTransfers[1] | $vBizETDCascading[1] | 
                  $vBizETDLiquidations[1] | $vBizETDDeliveries[1] | $vBizETDExeAss[1] | $vBizETDAbandonments[1] | 
                  $vBizETDCorporateAction[1] | $vBizETDPurchaseSales[1] | 
                  $vBizETDOpenPosition[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vActivity_EST">
      <xsl:choose>
        <xsl:when test="count($vBizESTTradesConfirmations[1] | $vBizESTUnsettledTrades[1] | $vBizESTSettledTrades[1] | 
                  $vBizESTAmendmentTransfers[1] |
                  $vBizESTClosedPositions[1] | 
                  $vBizESTOpenPosition[1] | $vBizESTStlOpenPosition[1] |
                  $vBizESTStlClosedPositions[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vActivity_DST">
      <xsl:choose>
        <xsl:when test="count($vBizDSTTradesConfirmations[1] | $vBizDSTUnsettledTrades[1] | $vBizDSTSettledTrades[1] |
                  $vBizDSTAmendmentTransfers[1] | $vBizDSTStlOpenPosition[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>


    <xsl:variable name="vActivity_CFDEquity">
      <xsl:choose>
        <xsl:when test="count($vBizCFDEquityTradesConfirmations[1] | $vBizCFDEquityAmendmentTransfers[1] | $vBizCFDEquityPurchaseSales[1] | $vBizCFDEquityOpenPosition[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vActivity_CFDForex">
      <xsl:choose>
        <xsl:when test="count($vBizCFDForexTradesConfirmations[1] | $vBizCFDForexAmendmentTransfers[1] | $vBizCFDForexPurchaseSales[1] | $vBizCFDForexOpenPosition[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vActivity_FxNDO">
      <xsl:choose>
        <xsl:when test="count($vBizFxNDOTradesConfirmations[1] | $vBizFxNDOOpenPosition[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vActivity_FxNDF">
      <xsl:choose>
        <xsl:when test="count($vBizFxNDFTradesConfirmations[1] | $vBizFxNDFOpenPosition[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vActivity_FxForward">
      <xsl:choose>
        <xsl:when test="count($vBizFxForwardTradesConfirmations[1] | $vBizFxForwardOpenPosition[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vActivity_FxBullion">
      <xsl:choose>
        <xsl:when test="count($vBizFxBullionTradesConfirmations[1] | $vBizFxBullionOpenPosition[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vActivity_COMS">
      <xsl:choose>
        <xsl:when test="count($vBizCOMSTradesConfirmations[1] | $vBizCOMSUnsettledTrades[1] | $vBizCOMSSettledTrades[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vActivityCount" select="$vActivity_ETD + 
                  $vActivity_EST + $vActivity_DST + 
                  $vActivity_CFDEquity + $vActivity_CFDForex + 
                  $vActivity_FxNDO + $vActivity_FxNDF + $vActivity_FxForward + $vActivity_FxBullion + 
                  $vActivity_COMS"/>

    <xsl:variable name="vIsMultiActivity" select="$vActivityCount > 1"/>

    <xsl:choose>
      <xsl:when test="$gcIsBusinessDebugMode=true()">
        <settings>
          <xsl:copy-of select="$gBlockSettingsNode"/>
        </settings>
        <tradesConfirmations activityCount="{$vActivityCount_TradesConfirmations}">
          <etd>
            <xsl:copy-of select="$vBizETDTradesConfirmations"/>
          </etd>
          <est>
            <xsl:copy-of select="$vBizESTTradesConfirmations"/>
          </est>
          <dst>
            <xsl:copy-of select="$vBizDSTTradesConfirmations"/>
          </dst>
          <cfdEquity>
            <xsl:copy-of select="$vBizCFDEquityTradesConfirmations"/>
          </cfdEquity>
          <cfdForex>
            <xsl:copy-of select="$vBizCFDForexTradesConfirmations"/>
          </cfdForex>
          <fxNDO>
            <xsl:copy-of select="$vBizFxNDOTradesConfirmations"/>
          </fxNDO>
          <fxNDF>
            <xsl:copy-of select="$vBizFxNDFTradesConfirmations"/>
          </fxNDF>
          <fxForward>
            <xsl:copy-of select="$vBizFxForwardTradesConfirmations"/>
          </fxForward>
          <fxBullion>
            <xsl:copy-of select="$vBizFxBullionTradesConfirmations"/>
          </fxBullion>
          <commodity>
            <xsl:copy-of select="$vBizCOMSTradesConfirmations"/>
          </commodity>
        </tradesConfirmations>
        <amendmentTransfers>
          <etd>
            <xsl:copy-of select="$vBizETDAmendmentTransfers"/>
          </etd>
          <!-- FI 20150827 [21287] AmendmentTransfers sur EquitySecurityTransaction -->
          <est>
            <xsl:copy-of select="$vBizESTAmendmentTransfers"/>
          </est>
          <dst>
            <xsl:copy-of select="$vBizDSTAmendmentTransfers"/>
          </dst>
          <cfdEquity>
            <xsl:copy-of select="$vBizCFDEquityAmendmentTransfers"/>
          </cfdEquity>
          <cfdForex>
            <xsl:copy-of select="$vBizCFDForexAmendmentTransfers"/>
          </cfdForex>
        </amendmentTransfers>
        <cascading>
          <etd>
            <xsl:copy-of select="$vBizETDCascading"/>
          </etd>
        </cascading>
        <liquidations>
          <etd>
            <xsl:copy-of select="$vBizETDLiquidations"/>
          </etd>
        </liquidations>
        <Deliveries>
          <etd>
            <xsl:copy-of select="$vBizETDDeliveries"/>
          </etd>
        </Deliveries>
        <CorporateAction>
          <etd>
            <xsl:copy-of select="$vBizETDCorporateAction"/>
          </etd>
        </CorporateAction>
        <exeAss>
          <etd>
            <xsl:copy-of select="$vBizETDExeAss"/>
          </etd>
        </exeAss>
        <abandonments>
          <etd>
            <xsl:copy-of select="$vBizETDAbandonments"/>
          </etd>
        </abandonments>
        <purchaseSale>
          <etd>
            <xsl:copy-of select="$vBizETDPurchaseSales"/>
          </etd>
          <cdfEquity>
            <xsl:copy-of select="$vBizCFDEquityPurchaseSales"/>
          </cdfEquity>
          <cfdForex>
            <xsl:copy-of select="$vBizCFDForexPurchaseSales"/>
          </cfdForex>
        </purchaseSale>
        <unsettledTrades>
          <est>
            <xsl:copy-of select="$vBizESTUnsettledTrades"/>
          </est>
          <dst>
            <xsl:copy-of select="$vBizDSTUnsettledTrades"/>
          </dst>
          <commodity>
            <xsl:copy-of select="$vBizCOMSUnsettledTrades"/>
          </commodity>
        </unsettledTrades>
        <settledTrades>
          <est>
            <xsl:copy-of select="$vBizESTSettledTrades"/>
          </est>
          <dst>
            <xsl:copy-of select="$vBizDSTSettledTrades"/>
          </dst>
          <commodity>
            <xsl:copy-of select="$vBizCOMSSettledTrades"/>
          </commodity>
        </settledTrades>
        <closedPositions>
          <est>
            <xsl:copy-of select="$vBizESTClosedPositions"/>
          </est>
        </closedPositions>
        <openPosition activityCount="{$vActivityCount_OpenPosition}">
          <etd>
            <xsl:copy-of select="$vBizETDOpenPosition"/>
          </etd>
          <est>
            <xsl:copy-of select="$vBizESTOpenPosition"/>
          </est>
          <cfdEquity>
            <xsl:copy-of select="$vBizCFDEquityOpenPosition"/>
          </cfdEquity>
          <cfdForex>
            <xsl:copy-of select="$vBizCFDForexOpenPosition"/>
          </cfdForex>
          <fxNDO>
            <xsl:copy-of select="$vBizFxNDOOpenPosition"/>
          </fxNDO>
          <fxNDF>
            <xsl:copy-of select="$vBizFxNDFOpenPosition"/>
          </fxNDF>
          <fxForward>
            <xsl:copy-of select="$vBizFxForwardOpenPosition"/>
          </fxForward>
          <fxBullion>
            <xsl:copy-of select="$vBizFxBullionOpenPosition"/>
          </fxBullion>
        </openPosition>
        <stlClosedPositions>
          <est>
            <xsl:copy-of select="$vBizESTStlClosedPositions"/>
          </est>
        </stlClosedPositions>
        <stlOpenPosition activityCount="{$vActivityCount_StlOpenPosition}">
          <est>
            <xsl:copy-of select="$vBizESTStlOpenPosition"/>
          </est>
          <dst>
            <xsl:copy-of select="$vBizDSTStlOpenPosition"/>
          </dst>
        </stlOpenPosition>
        <journalEntries>
          <xsl:copy-of select="$vBizJournalEntries"/>
        </journalEntries>
        <!-- FI 20160530 [21885] Add -->
        <collateral>
          <xsl:copy-of select="$vBizCollateral"/>
        </collateral>
        <!-- FI 20160613 [22256] Add -->
        <underlyingStock>
          <xsl:copy-of select="$vBizUnderlyingStock"/>
        </underlyingStock>
        <accountSummary>
          <xsl:copy-of select="$vBizAccountSummary"/>
        </accountSummary>
      </xsl:when>
      <xsl:otherwise>
        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <!--              Content Header                    -->
        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <xsl:call-template name="ReportTitle"/>
        <xsl:call-template name="ReportSendDetails"/>

        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <!--              Content Details                   -->
        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <!--Space-->
        <fo:block space-before="{$gDisplaySettings-BusinessDetail/@space-before}"/>

        <fo:table table-layout="fixed" table-omit-header-at-break="false"
                  display-align="center" keep-together.within-page="always" width="{$gSection_width}">

          <fo:table-column column-number="01" column-width="{$gSection_width}">
            <xsl:call-template name="Debug_border-red"/>
          </fo:table-column>
          <fo:table-header>
            <fo:table-row>
              <fo:table-cell>
                <xsl:call-template name="ReportBusinessDetails"/>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-header>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell>
                <xsl:call-template name="ReportIntroduction"/>
                <!-- FI 20161027 [22151] Call NonProcessedExchanges -->
                <!--<xsl:call-template name ="NonProcessedExchanges"/>-->
                <!-- FI 20170213 [22151] Call ProcessedExchanges -->
                <xsl:call-template name ="ProcessedExchanges"/>
                <!--1/ Section: TradesConfirmations / Mandatory -->
                <xsl:choose>
                  <xsl:when test="$vActivityCount_TradesConfirmations = 0">
                    <xsl:call-template name="Synthesis_Section">
                      <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:if test="$vBizETDTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizETDTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizESTTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizESTTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizDSTTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizDSTTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizCFDEquityTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizCFDEquityTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizCFDForexTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizCFDForexTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizFxNDOTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizFxNDOTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizFxNDFTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizFxNDFTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizFxForwardTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizFxForwardTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizFxBullionTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizFxBullionTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizCOMSTradesConfirmations">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizCOMSTradesConfirmations"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                  </xsl:otherwise>
                </xsl:choose>

                <!--2/ Section: AmendmentTransfers / Optional -->
                <xsl:if test="$vBizETDAmendmentTransfers">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_AMT"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDAmendmentTransfers"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!-- FI 20150827 [21287] AmendmentTransfers sur EquitySecurityTransaction -->
                <xsl:if test="$vBizESTAmendmentTransfers">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_AMT"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizESTAmendmentTransfers"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:if test="$vBizDSTAmendmentTransfers">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_AMT"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizDSTAmendmentTransfers"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:if test="$vBizCFDEquityAmendmentTransfers">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_AMT"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizCFDEquityAmendmentTransfers"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$vBizCFDForexAmendmentTransfers">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_AMT"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizCFDForexAmendmentTransfers"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--3/ Section: Cascading / Optional -->
                <xsl:if test="$vBizETDCascading">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_CAS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDCascading"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--4/ Section: Liquidations / Optional -->
                <xsl:if test="$vBizETDLiquidations">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_LIQ"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDLiquidations"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--5/ Section: Deliveries / Optional -->
                <xsl:if test="$vBizETDDeliveries">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_DLV"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDDeliveries"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--6/ Section: CorporateAction / Optional -->
                <xsl:if test="$vBizETDCorporateAction">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_CA"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDCorporateAction"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--7/ Section: ExeAss / Optional -->
                <xsl:if test="$vBizETDExeAss">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_EXA"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDExeAss"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>
                <!--8/ Section: Abandonments / Optional -->
                <xsl:if test="$vBizETDAbandonments">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_ABN"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDAbandonments"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--9/ Section: PurchaseSale / Optional -->
                <xsl:if test="$vBizETDPurchaseSales">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_PSS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDPurchaseSales"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$vBizCFDEquityPurchaseSales">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_PSS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizCFDEquityPurchaseSales"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$vBizCFDForexPurchaseSales">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_PSS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizCFDForexPurchaseSales"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--10/ Section: UnsettledTrades / Optional -->
                <xsl:if test="$vBizESTUnsettledTrades">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_UNS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizESTUnsettledTrades"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$vBizDSTUnsettledTrades">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_UNS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizDSTUnsettledTrades"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$vBizCOMSUnsettledTrades">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_UNS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizCOMSUnsettledTrades"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--11/ Section: SettledTrades / Optional -->
                <xsl:if test="$vBizESTSettledTrades">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_SET"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizESTSettledTrades"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$vBizCOMSSettledTrades">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_SET"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizCOMSSettledTrades"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:if test="$vBizDSTSettledTrades">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_SET"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizDSTSettledTrades"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:variable name="vESTDetailBiz">
                  <xsl:if test="$vActivity_EST = 1">
                    <xsl:value-of select="$gBlockSettings_Section/section[@key=$gcReportSectionKey_POS]/data[@name='BizDate']/@resource"/>
                  </xsl:if>
                </xsl:variable>

                <!--12/ Section: ClosedPositions / Optional -->
                <xsl:if test="$vBizESTClosedPositions">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_CLO"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizESTClosedPositions"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                    <xsl:with-param name="pDetail" select="$vESTDetailBiz"/>
                  </xsl:call-template>
                </xsl:if>

                <!--13/ Section: OpenPosition / Optional -->
                <!-- FI 20150716 [20892] OpenPosition devient optionel si activité EST ou DST uniquement-->
                <!-- RD 20161018 [22548] OpenPosition est optionel si activité EST et/ou DST et/ou Commodity uniquement-->
                <!--<xsl:variable name ="isPOSMandatory"
                              select ="not(($vIsMultiActivity=false()) and ($vActivity_EST=1 or $vActivity_DST=1))"/>-->
                <xsl:variable name ="isPOSMandatory"
                              select ="not(($vActivityCount > 0) and ($vActivityCount = $vActivity_EST + $vActivity_DST + $vActivity_COMS))"/>
                <xsl:choose>
                  <xsl:when test="$vActivityCount_OpenPosition=0 and $isPOSMandatory=true()">
                    <xsl:call-template name="Synthesis_Section">
                      <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:if test="$vBizETDOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizETDOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizESTOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizESTOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                        <xsl:with-param name="pDetail" select="$vESTDetailBiz"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizCFDEquityOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizCFDEquityOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizCFDForexOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizCFDForexOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizFxNDOOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizFxNDOOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizFxNDFOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizFxNDFOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizFxForwardOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizFxForwardOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizFxBullionOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizFxBullionOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                  </xsl:otherwise>
                </xsl:choose>

                <!--14/ Section: StlClosedPositions / Optional -->
                <xsl:if test="$vBizESTStlClosedPositions">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_CST"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizESTStlClosedPositions"/>
                    <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                  </xsl:call-template>
                </xsl:if>

                <!--15/ Section: StlOpenPosition / Mandatory for Equity -->
                <!-- FI 20150716 [20892] Add isSTLMandatory -->
                <xsl:variable name ="isSTLMandatory" select="$vActivity_EST=1 or $vActivity_DST=1"/>
                <xsl:choose>
                  <xsl:when test="$vActivityCount_StlOpenPosition=0 and $isSTLMandatory=true()">
                    <xsl:call-template name="Synthesis_Section">
                      <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_STL"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:if test="$vBizESTStlOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_STL"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizESTStlOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="$vBizDSTStlOpenPosition">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_STL"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizDSTStlOpenPosition"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                  </xsl:otherwise>
                </xsl:choose>

                <!--16/ Section: JournalEntries / Optional -->
                <xsl:if test="$vBizJournalEntries">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_JNL"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizJournalEntries"/>
                  </xsl:call-template>
                </xsl:if>

                <!--17/ Section: Collateral / Optional -->
                <!-- FI 20160530 [21885] Add-->
                <xsl:if test="$vBizCollateral">
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_SOD"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizCollateral"/>
                  </xsl:call-template>
                </xsl:if>

                <!--18/ Section: Collateral / Optional -->
                <!-- FI 20160613 [22256] Add-->
                <xsl:if test="$vBizUnderlyingStock">
                  <xsl:variable name="vDetailHeader">
                    <xsl:value-of select="$gBlockSettings_Section/section[@key=$gcReportSectionKey_UOD]/@resourceAdditional"/>
                  </xsl:variable>
                  <xsl:call-template name="Synthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_UOD"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizUnderlyingStock"/>
                    <xsl:with-param name="pDetail" select="$vDetailHeader"/>
                  </xsl:call-template>
                </xsl:if>

                <!--19/ Section: AccountSummary / Mandatory / Page break -->
                <fo:block break-after='page'/>
                <xsl:call-template name="Synthesis_Section">
                  <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_ACC"/>
                  <xsl:with-param name="pBusiness_Section" select="$vBizAccountSummary"/>
                </xsl:call-template>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>

        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <!--              Content Footer                    -->
        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <xsl:call-template name="ReportLastMessage"/>
        <!--Standard Legend-->
        <xsl:call-template name="A4VerticalLegend">
          <xsl:with-param name="pWithOrderType" select="true()"/>
          <xsl:with-param name="pWithTradeType" select="true()"/>
          <xsl:with-param name="pWithPriceType" select="true()"/>
        </xsl:call-template>

        <!--Report Legend-->
        <xsl:if test="$vBizETDTradesConfirmations or $vBizESTTradesConfirmations or $vBizCFDEquityTradesConfirmations or $vBizCFDForexTradesConfirmations or
                $vBizFxNDOTradesConfirmations or $vBizFxNDFTradesConfirmations or $vBizFxForwardTradesConfirmations or $vBizFxBullionTradesConfirmations or $vBizCOMSTradesConfirmations or
                $vBizETDAmendmentTransfers or $vBizCFDEquityAmendmentTransfers or $vBizCFDForexAmendmentTransfers or
                $vBizETDCascading or $vBizETDLiquidations or $vBizETDDeliveries or $vBizETDExeAss or $vBizETDAbandonments or 
                $vBizETDCorporateAction or $vBizETDPurchaseSales or $vBizCFDEquityPurchaseSales or $vBizCFDForexPurchaseSales or
                $vBizESTUnsettledTrades or $vBizCOMSUnsettledTrades or $vBizESTSettledTrades or $vBizCOMSSettledTrades or 
                $vBizESTOpenPosition or $vBizESTStlOpenPosition">

          <fo:block font-size="{$gData_font-size}" padding="{$gBlockSettings_Legend/@padding}"
                    display-align="{$gData_display-align}" keep-together.within-page="always" space-before="{$gLegend_space-before}">

            <!--Legend_FeesExclTax-->
            <xsl:if test="$vBizETDTradesConfirmations or $vBizESTTradesConfirmations or $vBizCFDEquityTradesConfirmations or $vBizCFDForexTradesConfirmations or 
                    $vBizFxNDOTradesConfirmations or $vBizFxNDFTradesConfirmations or $vBizFxForwardTradesConfirmations or $vBizFxBullionTradesConfirmations or $vBizCOMSTradesConfirmations or
                    $vBizETDAmendmentTransfers or $vBizCFDEquityAmendmentTransfers or $vBizCFDForexAmendmentTransfers or
                    $vBizETDCascading or $vBizETDLiquidations or $vBizETDDeliveries or $vBizETDExeAss or $vBizETDAbandonments or 
                    $vBizETDCorporateAction or $vBizETDPurchaseSales or
                    $vBizESTUnsettledTrades or $vBizCOMSUnsettledTrades or $vBizESTSettledTrades or $vBizCOMSSettledTrades or 
                    $vBizESTOpenPosition or $vBizESTStlOpenPosition">

              <xsl:call-template name="UKDisplay_Legend">
                <xsl:with-param name="pLegend" select="$gcLegend_FeesExclTax"/>
                <xsl:with-param name="pResource" select="'Report-LegendFeesExclTax'"/>
              </xsl:call-template>
            </xsl:if>

            <!--Legend_RealisedPnL-->
            <xsl:if test="$vBizETDLiquidations or $vBizETDExeAss or $vBizETDAbandonments or
                    $vBizETDPurchaseSales or $vBizCFDEquityPurchaseSales or $vBizCFDForexPurchaseSales">

              <xsl:call-template name="UKDisplay_Legend">
                <xsl:with-param name="pLegend" select="$gcLegend_RealisedPnL"/>
                <xsl:with-param name="pResource" select="'Report-Legend_RealisedPnL'"/>
              </xsl:call-template>
            </xsl:if>
          </fo:block>
        </xsl:if>

        <fo:block id="LastPage"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>


  <!-- .......................................................................... -->
  <!--  Section: TRADES CONFIRMATIONS / UNSETTLED TRADES / SETTLED TRADES         -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_TradesConfirmationsData              -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display TradesConfirmations Content             -->
  <!-- ................................................ -->
  <!-- FI 20150716 [20892] Add parameter pSectionKey -->
  <!-- FI 20151019 [21317] Modify (Gestion de la famille DSE) -->
  <xsl:template name="Synthesis_TradesConfirmationsData">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <xsl:variable name="vQtyModel">
      <xsl:choose>
        <xsl:when test="contains(',ESE,DSE,COMS,',concat(',',$pBizBook/asset/@family,','))">
          <xsl:value-of select="'2DateSideQty'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Date2Qty'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">

      <xsl:choose>
        <xsl:when test="$vQtyModel='Date2Qty'">
          <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
          <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
          <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
          <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
          <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
          <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
          <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
          <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
          <fo:table-column column-number="11" column-width="proportional-column-width(1)"  />
          <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
          <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
        </xsl:when>
        <xsl:when test="$vQtyModel='2DateSideQty'">
          <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
          <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
          <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Side']/@column-width}" />
          <fo:table-column column-number="06" column-width="{$gBlockSettings_Data/column[@name='SideQty2']/@column-width}" />
          <fo:table-column column-number="07" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
          <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
          <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
          <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
          <fo:table-column column-number="12" column-width="proportional-column-width(1)"  />
          <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
          <fo:table-column column-number="17" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="21" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
        </xsl:when>
      </xsl:choose>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <xsl:choose>
            <xsl:when test="$vQtyModel='2DateSideQty'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/asset/@family='COMS'">
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='PaymentDate']/@resource"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='SettltDate']/@resource"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/asset/@family='ESE' or $pBizBook/asset/@family='DSE'">
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='SettltDate']/@resource"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Type-->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>

          <xsl:choose>
            <xsl:when test="$vQtyModel='Date2Qty'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
														 text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Side']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Side']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='SideQty2']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='SideQty2']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>

          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family='LSD'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O'">
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
																 display-align="{$gData_display-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
																 display-align="{$gData_display-align}"
                                 text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="2">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name ="UnderlyingHeader">
                    <xsl:with-param name ="pFamily" select ="$pBizBook/asset/@family"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:choose>
                <xsl:when test="$pBizBook/asset/@family='COMS'">
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Commissions']/@resource"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Fee']/@resource"/>
                </xsl:otherwise>
              </xsl:choose>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_FeesExclTax"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family='LSD'">
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O' and $pBizDC/@futValuationMethod = 'EQTY'">
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
										             display-align="{$gData_display-align}" number-columns-spanned="4">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Premium']/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
								             display-align="{$gData_display-align}" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='ContractValue']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-row>

        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>

      <fo:table-body>
        <xsl:variable name="vDailyTrades" select="$gDailyTrades[(string-length($pBizBook/@sectionKey)=0 or $pBizBook/@sectionKey=$gcReportSectionKey_TRD) and @bizDt=$pBizDt]
                      | $gSettledTrades[$pBizBook/@sectionKey=$gcReportSectionKey_SET and (@bizDt=$pBizDt)]
                      | $gUnsettledTrades[$pBizBook/@sectionKey=$gcReportSectionKey_UNS and (@bizDt=$pBizDt)]"/>

        <xsl:variable name="vBookTradeList" select="$gTrade[@tradeId=$pBizBook/asset/trade/@tradeId]"/>
        <xsl:variable name="vBookSubTotal" select="$vDailyTrades/subTotal[
                      (($pBizBook/@sectionKey=$gcReportSectionKey_TRD and predicates/predicate/@trdTyp) or ($pBizBook/@sectionKey!=$gcReportSectionKey_TRD and predicates=false())) 
                      and @idB=$pBizBook/@OTCmlId and @idI=$pBizBook/asset/@idI]"/>
        <xsl:variable name="vBookEfsmlTradeList" select="$vDailyTrades/trade[@tradeId=$vBookTradeList/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>
          <xsl:sort select="$gRepository/*[name()=current()/@assetName and @OTCmlId=current()/@OTCmlId]/displayname" data-type="text"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset/trade"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!-- RD 20190619 [23912] Display asset details -->
          <!--Display asset details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}">

            <fo:table-cell text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                           font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="20">
              <!-- Header -->
              <xsl:call-template name="UKAsset_Header">
                <xsl:with-param name="pRepository-Asset" select="$vRepository_Asset"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>
          </fo:table-row>

          <!--Display Trades Details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()[$vQtyModel!='2DateSideQty']/@stlDt" data-type="text"/>
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@trdNum" data-type="text"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <!--Only one EfsmlTrade-->
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId][1]"/>
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <!--Data row, with first Fee and amount1-->
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}"
                          keep-together.within-page="always">

              <!-- Pas de trade orphelin-->
              <xsl:if test="position()=1">
                <xsl:attribute name="keep-with-previous">
                  <xsl:value-of select="'always'"/>
                </xsl:attribute>
              </xsl:if>

              <xsl:choose>
                <xsl:when test="$vQtyModel='2DateSideQty'">
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@stlDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:choose>
                        <xsl:when test="$vBizAsset/@family='ESE' or $vBizAsset/@family='DSE'">
                          <xsl:call-template name="DisplayData_Format">
                            <xsl:with-param name="pData" select="$vBizTrade/@stlDt"/>
                            <xsl:with-param name="pDataType" select="'Date'"/>
                          </xsl:call-template>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:call-template name="DisplayData_Format">
                            <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                            <xsl:with-param name="pDataType" select="'Date'"/>
                          </xsl:call-template>
                        </xsl:otherwise>
                      </xsl:choose>
                    </fo:block>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@trdNum"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Type']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='Type']/data/@font-weight}"
                             text-align="{$gBlockSettings_Data/column[@name='Type']/data/@text-align}"
                             padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='LSD'">
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="'TrdTyp'"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Type']/data/@length"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="contains(',ESE,DSE,COMS,',concat(',',$vBizAsset/@family,','))"/>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:call-template name="DisplayData_Efsml">
                        <xsl:with-param name="pDataName" select="'PosResult'"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Type']/data/@length"/>
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <xsl:if test="$vQtyModel='Date2Qty'">
                  <xsl:attribute name="text-align">
                    <xsl:value-of select="$gData_Number_text-align"/>
                  </xsl:attribute>
                </xsl:if>
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:variable name="vDataName">
                    <xsl:choose>
                      <xsl:when test="$vQtyModel='Date2Qty'">
                        <xsl:value-of select="'BuyQty'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'Side'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:call-template name="DisplayData_ReturnSwap">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='DSE'">
                      <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='COMS'">
                      <xsl:call-template name="DisplayData_CommoditySpot">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='LSD' or $vBizAsset/@family='ESE'">
                      <!-- FI 20150825 [21287] Passage du paramètre pDataEfsml -->
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:variable name="vDataName">
                    <xsl:choose>
                      <xsl:when test="$vQtyModel='Date2Qty'">
                        <xsl:value-of select="'SellQty'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'Qty'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:call-template name="DisplayData_ReturnSwap">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='DSE'">
                      <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='COMS'">
                      <xsl:call-template name="DisplayData_CommoditySpot">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                      <xsl:if test="string-length($vRepository_Asset/qtyUnit) > 0">
                        <fo:inline font-size="{$gData_Det_font-size}">
                          <xsl:value-of select="concat($gcSpace,$vRepository_Asset/qtyUnit)"/>
                        </fo:inline>
                      </xsl:if>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='LSD' or $vBizAsset/@family='ESE'">
                      <!-- FI 20150825 [21287] Passage du paramètre pDataEfsml -->
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>

              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='LSD'">
                  <fo:table-cell padding-top="{$gData_padding}"
                                 padding-bottom="{$gData_padding}"
                                 text-align="{$gBlockSettings_Data/column[@name='Maturity']/data/@text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'Maturity'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <xsl:choose>
                    <xsl:when test="$pBizDC/@category = 'O'">
                      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayData_RepositoryEtd">
                            <xsl:with-param name="pDataName" select="'PC'"/>
                            <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayData_RepositoryEtd">
                            <xsl:with-param name="pDataName" select="'FmtStrike'"/>
                            <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                            <xsl:with-param name="pNumberPattern" select="$pBizDC/pattern/@strike" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell number-columns-spanned="2">
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <!--RD 20200818 [25456] Faire le cumul des frais par trade -->
                  <xsl:variable name="vFeeCount" select="count($vBizTrade/fee)"/>
                  <xsl:variable  name ="vNumberRowsSpanned">
                    <xsl:choose>
                      <xsl:when test ="$vBizTrade/amount/amount4">
                        <xsl:call-template name="Maximum">
                          <xsl:with-param name="pNbr1" select="$vFeeCount"/>
                          <xsl:with-param name="pNbr2" select="4"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:when test ="$vBizTrade/amount/amount3">
                        <xsl:call-template name="Maximum">
                          <xsl:with-param name="pNbr1" select="$vFeeCount"/>
                          <xsl:with-param name="pNbr2" select="3"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:when test ="$vBizTrade/amount/amount2">
                        <xsl:call-template name="Maximum">
                          <xsl:with-param name="pNbr1" select="$vFeeCount"/>
                          <xsl:with-param name="pNbr2" select="2"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:choose>
                          <xsl:when test ="$gIsDisplayTimestamp=true()">
                            <xsl:call-template name="Maximum">
                              <xsl:with-param name="pNbr1" select="$vFeeCount"/>
                              <xsl:with-param name="pNbr2" select="2"/>
                            </xsl:call-template>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:call-template name="Maximum">
                              <xsl:with-param name="pNbr1" select="$vFeeCount"/>
                              <xsl:with-param name="pNbr2" select="1"/>
                            </xsl:call-template>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:variable name ="vRepository_AdditionalInfo">
                    <xsl:if test ="$vBizAsset/@family='DSE' and string-length($vBizTrade/@accIntDays)>0">
                      <xsl:value-of select ="concat('Acc. Days: ',$vBizTrade/@accIntDays)"/>
                    </xsl:if>
                  </xsl:variable>

                  <!-- FI 20151019 [21317] call GetCellUnderlying -->
                  <xsl:call-template name ="GetCellUnderlying">
                    <xsl:with-param name ="pRepository_Asset" select ="$vRepository_Asset"/>
                    <xsl:with-param name ="pAdditionalInfo" select ="$vRepository_AdditionalInfo"/>
                    <xsl:with-param name ="pBizTrade" select ="$vBizTrade"/>
                    <xsl:with-param name ="pNumberRowsSpanned" select ="$vNumberRowsSpanned"/>
                  </xsl:call-template>

                </xsl:otherwise>
              </xsl:choose>

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@lastPx" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>

              <!--First Fee -->
              <!--RD 20200818 [25456] Faire le cumul des frais par trade -->
              <xsl:call-template name="UKDisplayTradeDet_FirstFee">
                <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
              </xsl:call-template>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!-- amount1 -->
              <xsl:variable name="vAmount1" select ="$vBizTrade/amount/amount1"/>
              <xsl:choose>
                <xsl:when test ="$vAmount1/*[1]">
                  <xsl:choose>
                    <xsl:when test ="$vAmount1/@det">
                      <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                                     font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                                     padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayType_Short">
                            <xsl:with-param name="pType" select="$vAmount1/@det"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell>
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vAmount1/*[1]"/>
                    <xsl:with-param name="pWithSide" select="$vAmount1/@withside=1"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>

            <!--Other Fee rows, Timestamp, ...-->
            <xsl:variable name="vDisplayTimestamp">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'TimeStamp'"/>
                <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
              </xsl:call-template>
            </xsl:variable>
            <!-- FI 20151019 [21317]-->
            <xsl:variable name="vDisplayOtherPrice">
              <xsl:if test ="$vBizAsset/@family='DSE'">
                <xsl:call-template name="DisplayFmtNumber">
                  <xsl:with-param name="pFmtNumber" select="$vBizTrade/@accIntRt"/>
                  <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@accIntRt" />
                </xsl:call-template>
              </xsl:if>
            </xsl:variable>

            <!--RD 20200818 [25456] Faire le cumul des frais par trade -->
            <xsl:call-template name="UKDisplayRow_TradeDet">
              <xsl:with-param name="pOtherPrice" select="$vDisplayOtherPrice"/>
              <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
              <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
              <xsl:with-param name="pOtherAmount2" select="$vBizTrade/amount/amount2"/>
              <xsl:with-param name="pOtherAmount3" select="$vBizTrade/amount/amount3"/>
              <xsl:with-param name="pOtherAmount4" select="$vBizTrade/amount/amount4"/>
              <xsl:with-param name="pQtyPattern" select="$vBizAsset/pattern/@qty" />
              <xsl:with-param name="pIsQtyModel" select="$vQtyModel='2DateSideQty'"/>
            </xsl:call-template>
          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vSubTotal" select="$vBookSubTotal[@idAsset=$vBizAsset/@OTCmlId and @assetCategory=$vBizAsset/@assetCategory and @idI=$vBizAsset/@idI][1]"/>
          <xsl:variable name="vBizSubTotal" select="$vBizAsset/subTotal[1]"/>
          <xsl:variable name="vBizSubTotalAmount" select="$vBizAsset/subTotalAmount"/>

          <!--Subtotal row, with first Fee-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">

            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$vQtyModel='Date2Qty'">
                <fo:table-cell number-columns-spanned="2">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block >
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                    </xsl:call-template>
                    <xsl:if test="$vBizAsset/@family='COMS' and string-length($vRepository_Asset/qtyUnit) > 0">
                      <fo:inline font-size="{$gData_Det_font-size}">
                        <xsl:value-of select="concat($gcSpace,$vRepository_Asset/qtyUnit)"/>
                      </fo:inline>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                    </xsl:call-template>
                    <xsl:if test="$vBizAsset/@family='COMS' and string-length($vRepository_Asset/qtyUnit) > 0">
                      <fo:inline font-size="{$gData_Det_font-size}">
                        <xsl:value-of select="concat($gcSpace,$vRepository_Asset/qtyUnit)"/>
                      </fo:inline>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="3">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Side']/data/@background-color}">

                  <xsl:if test="$vQtyModel='Date2Qty'">
                    <xsl:attribute name="text-align">
                      <xsl:value-of select="$gData_Number_text-align"/>
                    </xsl:attribute>
                  </xsl:if>
                  <fo:block >
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'BuySide'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      <xsl:with-param name="pQtyModel" select="$vQtyModel"/>
                    </xsl:call-template>
                    <xsl:if test="$vBizAsset/@family='COMS' and string-length($vRepository_Asset/qtyUnit) > 0">
                      <fo:inline font-size="{$gData_Det_font-size}">
                        <xsl:value-of select="concat($gcSpace,$vRepository_Asset/qtyUnit)"/>
                      </fo:inline>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>

            <xsl:choose>
              <xsl:when test="$vBizAsset/@family='LSD'">
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                               font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               number-columns-spanned="2">
                  <xsl:call-template name="Display_AddAttribute-color">
                    <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                  </xsl:call-template>
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <!-- 20160229 [XXXXX] Add if pour ne pas afficher NaN-->
                    <xsl:if test ="string-length($vBizAsset/@expDt)>0">
                      <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                      <xsl:value-of select="$gcSpace"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizAsset/@expDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <!-- FI 20150716 [20892] Add settl. price -->
              <!-- FI 20151019 [21317] Add closing AccIntDays on DSE (add no-wrap) -->
              <xsl:when test="($vBizAsset/@family='ESE' or $vBizAsset/@family='DSE') and $pSectionKey=$gcReportSectionKey_UNS">
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-weight}"
                           text-align="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="2" wrap-option="no-wrap">
                  <xsl:call-template name="Display_AddAttribute-color">
                    <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@color"/>
                  </xsl:call-template>
                  <fo:block>
                    <xsl:call-template name="Debug_border-blue"/>
                    <xsl:if test ="$vBizAsset/closing">
                      <!-- EG 20190730 Upd : AssetMeasure -->
                      <xsl:call-template name="DisplaySubTotalColumnAssetMeasureClosePrice">
                        <xsl:with-param name="pFmtPrice" select="$vBizAsset/closing/@clrPx"/>
                        <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@clrPx" />
                        <xsl:with-param name="pResName" select="$vBizAsset/closing/@meaClrPx"/>
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="2">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>

            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-weight}"
                           text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="2" wrap-option="no-wrap">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="UKDisplay_SubTotal_AvgPx">
                  <xsl:with-param name="pDataName" select="'AvgBuy'"/>
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                  <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@avgPx"/>
                  <xsl:with-param name="pIsPos" select="false()"/>
                  <xsl:with-param name="pQtyModel" select="$vQtyModel"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!--First Subtotal Fee row-->
            <xsl:call-template name="UKDisplay_SubTotal_OneFee">
              <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
            </xsl:call-template>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- Subtotal amount 1-->
            <xsl:variable name="vAmount1" select ="$vBizSubTotalAmount/amount1"/>
            <xsl:choose>
              <xsl:when test ="$vAmount1/*[1]">
                <xsl:variable name="vTotal"  select="$vAmount1/*[1]/@amt"/>
                <xsl:variable name="vTotalCcy" select="$vAmount1/*[1]/@ccy"/>
                <xsl:choose>
                  <xsl:when test ="$vAmount1/@det">
                    <xsl:variable name="vTotalColor">
                      <xsl:if test="$gIsColorMode and $vAmount1/@withside=1">
                        <xsl:call-template name="GetAmount-color">
                          <xsl:with-param name="pAmount" select="$vTotal"/>
                        </xsl:call-template>
                      </xsl:if>
                    </xsl:variable>
                    <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                               font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding}">

                      <xsl:if test ="string-length($vTotalColor)>0">
                        <xsl:call-template name="Display_AddAttribute-color">
                          <xsl:with-param name="pColor" select="$vTotalColor"/>
                        </xsl:call-template>
                      </xsl:if>
                      <fo:block>
                        <xsl:call-template name="Debug_border-green"/>
                        <xsl:call-template name="DisplayType_Short">
                          <xsl:with-param name="pType" select="$vAmount1/@det"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </xsl:when>
                  <xsl:otherwise>
                    <fo:table-cell>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </xsl:otherwise>
                </xsl:choose>

                <xsl:call-template name="UKDisplay_SubTotal_Amount">
                  <xsl:with-param name="pAmount" select="$vTotal" />
                  <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                  <xsl:with-param name="pWithSide" select="$vAmount1/@withside=1"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-row>

          <xsl:variable name="vColumns-spanned">
            <xsl:choose>
              <xsl:when test="$vQtyModel='2DateSideQty'">
                <xsl:value-of select="'12'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'11'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:choose>
            <xsl:when test="$vQtyModel!='Date2Qty' and $vSubTotal/long/@qty > 0 and $vSubTotal/short/@qty > 0">
              <!--Subtotal row, with second Fee-->
              <fo:table-row font-size="{$gData_font-size}"
                            font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                            text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">

                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Side']/data/@background-color}">
                  <fo:block >
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'SellSide'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                    </xsl:call-template>
                    <xsl:if test="$vBizAsset/@family='COMS' and string-length($vRepository_Asset/qtyUnit) > 0">
                      <fo:inline font-size="{$gData_Det_font-size}">
                        <xsl:value-of select="concat($gcSpace,$vRepository_Asset/qtyUnit)"/>
                      </fo:inline>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-size}"
                               font-weight="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-weight}"
                               text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               number-columns-spanned="3">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="UKDisplay_SubTotal_AvgPx">
                      <xsl:with-param name="pDataName" select="'AvgSell'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@avgPx"/>
                      <xsl:with-param name="pIsPos" select="false()"/>
                      <xsl:with-param name="pQtyModel" select="$vQtyModel"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!--Second Subtotal Fee row-->
                <xsl:call-template name="UKDisplay_SubTotal_OneFee">
                  <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
                  <xsl:with-param name="pFeeNumber" select="'2'" />
                </xsl:call-template>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <xsl:choose>
                  <!-- FI 20150716 [20892] call UKDisplay_SubTotal_OtherAmount-->
                  <xsl:when test="$vBizSubTotalAmount/amount2">
                    <xsl:call-template name ="UKDisplay_SubTotal_OtherAmount">
                      <xsl:with-param name ="pOtherAmount" select="$vBizSubTotalAmount/amount2"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <fo:table-cell number-columns-spanned="5">
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:table-row>
              <!--Other Subtotal Fee rows-->
              <xsl:call-template name="UKDisplayRow_SubTotalDet">
                <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
                <xsl:with-param name="pFeeStartNumber" select="'3'" />
                <xsl:with-param name="pOtherAmount3" select="$vBizSubTotalAmount/amount3"/>
                <xsl:with-param name="pOtherAmount4" select="$vBizSubTotalAmount/amount4"/>
                <xsl:with-param name="pColumns-spanned" select="$vColumns-spanned" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <!--Other Subtotal Fee rows-->
              <xsl:call-template name="UKDisplayRow_SubTotalDet">
                <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
                <xsl:with-param name="pOtherAmount2" select="$vBizSubTotalAmount/amount2"/>
                <xsl:with-param name="pOtherAmount3" select="$vBizSubTotalAmount/amount3"/>
                <xsl:with-param name="pOtherAmount4" select="$vBizSubTotalAmount/amount4"/>
                <xsl:with-param name="pColumns-spanned" select="$vColumns-spanned" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <!--Display underline-->
          <!-- EG 20160308 Migration vs2013 number('21') -->
          <xsl:choose>
            <xsl:when test="$vQtyModel='2DateSideQty'">
              <xsl:call-template name="Display_SubTotalSpace">
                <xsl:with-param name="pPosition" select="position()"/>
                <xsl:with-param name="pNumber-columns" select="number('21')"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="Display_SubTotalSpace">
                <xsl:with-param name="pPosition" select="position()"/>
                <xsl:with-param name="pNumber-columns" select="number('20')"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>


  <!-- ................................................ -->
  <!-- Synthesis_TradesConfirmationsFx                -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Fx TradesConfirmations Content          -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_TradesConfirmationsFx">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizCurrency"/>
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="08" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
      <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
      <fo:table-column column-number="12" column-width="proportional-column-width(1)"  />
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
      <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
      <fo:table-column column-number="17" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
      <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
      <fo:table-column column-number="21" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
						             display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Ccy']/header[@name='Dealt']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:choose>
                <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='Expiry']/@resource"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='ValueDate']/@resource"/>
                </xsl:otherwise>
              </xsl:choose>
            </fo:block>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="2">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Rate']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Fee']/@resource"/>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_FeesExclTax"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:choose>
                <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Premium']/@resource"/>
                </xsl:when>
                <xsl:when test="$pBizBook/dealt/@family='FxNDF'">
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='RefAmount']/@resource"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='NetAmount']/@resource"/>
                </xsl:otherwise>
              </xsl:choose>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gTrade[@tradeId=$pBizBook/dealt/trade/@tradeId]"/>
        <xsl:variable name="vBookEfsmlTradeList" select="$gDailyTrades[@bizDt=$pBizDt]/trade[@tradeId=$vBookTradeList/@tradeId]"/>

        <xsl:for-each select="$pBizBook/dealt">
          <xsl:sort select="current()/@ccy" data-type="text"/>

          <xsl:variable name="vBizDealt" select="current()"/>

          <xsl:variable name="vDealtBizTradeList" select="$vBizDealt/trade"/>
          <xsl:variable name="vDealtEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vDealtBizTradeList/@tradeId]"/>
          <xsl:variable name="vDealtTradeList" select="$vBookTradeList[@tradeId=$vDealtBizTradeList/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vDealtBizTradeList">
            <xsl:sort select="current()/@valDt" data-type="text"/>
            <xsl:sort select="current()/@expDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <!--Only one EfsmlTrade-->
            <xsl:variable name="vEfsmlTrade" select="$vDealtEfsmlTradeList[@tradeId=$vBizTrade/@tradeId][1]"/>
            <xsl:variable name="vTrade" select="$vDealtTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <!-- 1/ Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}" font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}" display-align="{$gData_display-align}">

              <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                             padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">

                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Efsml">
                    <xsl:with-param name="pDataName" select="'TrdDt'"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                             padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@tradeId"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                             padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                      <xsl:call-template name="DisplayData_FxOptionLeg">
                        <xsl:with-param name="pDataName" select="'BuyQty'"/>
                        <xsl:with-param name="pFxOptionLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_FxLeg">
                        <xsl:with-param name="pDataName" select="'BuyQty'"/>
                        <xsl:with-param name="pFxLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                             padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                      <xsl:call-template name="DisplayData_FxOptionLeg">
                        <xsl:with-param name="pDataName" select="'SellQty'"/>
                        <xsl:with-param name="pFxOptionLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_FxLeg">
                        <xsl:with-param name="pDataName" select="'SellQty'"/>
                        <xsl:with-param name="pFxLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
														 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">

                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$vBizDealt/@ccy"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
														 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@expDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@valDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
														 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                      <xsl:call-template name="DisplayData_FxOptionLeg">
                        <xsl:with-param name="pDataName" select="'Strike'"/>
                        <xsl:with-param name="pFxOptionLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_FxLeg">
                        <xsl:with-param name="pDataName" select="'CurrencyPair'"/>
                        <xsl:with-param name="pFxLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
														 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizDealt/pattern/@lastPx" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!--First Fee -->
              <!--RD 20200818 [25456] Faire le cumul des frais par trade -->
              <xsl:call-template name="UKDisplayTradeDet_FirstFee">
                <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
              </xsl:call-template>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vEfsmlTrade/prm"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="Display_NetAmount_FxLeg">
                    <xsl:with-param name="pFxLegTrade" select="$vTrade" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
            <!--RD 20200818 [25456] Faire le cumul des frais par trade -->
            <xsl:choose>
              <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                <xsl:call-template name="DisplayTradeDetRow_FxOptionLeg">
                  <xsl:with-param name="pFamily" select="$pBizBook/dealt/@family"/>
                  <xsl:with-param name="pFxOptionLegTrade" select="$vTrade"/>
                  <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="DisplayTradeDetRow_FxLeg">
                  <xsl:with-param name="pFamily" select="$pBizBook/dealt/@family"/>
                  <xsl:with-param name="pFxLegTrade" select="$vTrade"/>
                  <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vBizSubTotal" select="$vBizDealt/subTotal[1]"/>

          <!--Subtotal row, with first Fee-->
          <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="2">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                    <xsl:call-template name="DisplaySubtotal_FxOptionLeg">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pFxOptionLegTrades" select="$vDealtTradeList"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="DisplaySubtotal_FxLeg">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pFxLegTrades" select="$vDealtTradeList"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                    <xsl:call-template name="DisplaySubtotal_FxOptionLeg">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pFxOptionLegTrades" select="$vDealtTradeList"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="DisplaySubtotal_FxLeg">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pFxLegTrades" select="$vDealtTradeList"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$vBizDealt/@ccy"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="4">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!--First Subtotal Fee row-->
            <xsl:call-template name="UKDisplay_SubTotal_OneFee">
              <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
            </xsl:call-template>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                <xsl:call-template name="UKDisplay_SubTotal_Amount">
                  <xsl:with-param name="pAmount" select="$vDealtEfsmlTradeList/prm" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="DisplaySubtotal_NetAmount_FxLeg">
                  <xsl:with-param name="pFxLegTrades" select="$vDealtTradeList" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-row>
          <!--Other Subtotal Fee rows-->
          <xsl:call-template name="UKDisplayRow_SubTotalDet">
            <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
            <xsl:with-param name="pColumns-spanned" select="'12'" />
          </xsl:call-template>
        </xsl:for-each>

        <!--Subtotal row, for main Currency-->
        <xsl:if test="$pBizBook/dealt/@family!='FxNDO'">
          <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="concat($gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource,$gcSpace,$pBizCurrency/@ccy)"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="17">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:call-template name="DisplaySubtotal_NetAmount_FxLeg">
              <xsl:with-param name="pFxLegTrades" select="$vBookTradeList" />
            </xsl:call-template>
          </fo:table-row>
        </xsl:if>
        <!--Display space-->
        <xsl:call-template name="Display_SubTotalSpace">
          <xsl:with-param name="pNumber-columns" select="number('21')"/>
          <xsl:with-param name="pPosition" select="last()"/>
        </xsl:call-template>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--  Section: AMENDMENT / TRANSFER / CASCADING                                 -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_AmendmentTransfersCascadingData        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
       Display AmendmentTransfers/Cascading Content              
       ................................................ -->
  <!-- FI 20150827 [21287] Modify -->
  <xsl:template name="Synthesis_AmendmentTransfersCascadingData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <xsl:variable name="vQtyModel">
      <xsl:choose>
        <xsl:when test="$pBizBook/asset/@family='ESE' or $pBizBook/asset/@family='DSE'">
          <xsl:value-of select="'2DateSideQty'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Date2Qty'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">

      <xsl:choose>
        <xsl:when test="$vQtyModel='Date2Qty'">
          <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
          <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
          <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
          <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
          <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
          <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
          <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
          <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
          <fo:table-column column-number="11" column-width="proportional-column-width(1)"  />
          <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
          <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
        </xsl:when>
        <xsl:when test="$vQtyModel='2DateSideQty'">
          <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
          <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
          <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Side']/@column-width}" />
          <fo:table-column column-number="06" column-width="{$gBlockSettings_Data/column[@name='SideQty2']/@column-width}" />
          <fo:table-column column-number="07" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
          <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
          <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
          <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
          <fo:table-column column-number="12" column-width="proportional-column-width(1)"  />
          <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
          <fo:table-column column-number="17" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="21" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
        </xsl:when>
      </xsl:choose>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <xsl:choose>
            <xsl:when test="$vQtyModel='2DateSideQty'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='SettltDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/asset/@family='ESE'">
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='SettltDate']/@resource"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>

          <xsl:choose>
            <xsl:when test="$vQtyModel='Date2Qty'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Side']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Side']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='SideQty2']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='SideQty2']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>

          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family='LSD'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O'">
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
																 display-align="{$gData_display-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
																 display-align="{$gData_display-align}"
                                 text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="2">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name ="UnderlyingHeader">
                    <xsl:with-param name ="pFamily" select ="$pBizBook/asset/@family"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
												 text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Fee']/@resource"/>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_FeesExclTax"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family='LSD'">
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O' and $pBizDC/@futValuationMethod = 'EQTY'">
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
																 display-align="{$gData_display-align}" number-columns-spanned="4">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Premium']/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='ContractValue']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>

      <fo:table-body>
        <xsl:variable name="vDailyTrades" select="$gDailyTrades[@bizDt=$pBizDt]"/>
        <xsl:variable name="vBookTradeList" select="$gTrade[@tradeId=$pBizBook/asset//trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>
          <xsl:sort select="$gRepository/*[name()=current()/@assetName and @OTCmlId=current()/@OTCmlId]/displayname" data-type="text"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizPosActionList" select="$vBizAsset/posAction"/>
          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset//trade"/>
          <xsl:variable name="vAssetPosActionList" select="$gPosActions[@bizDt=$pBizDt]/posAction[@OTCmlId=$vAssetBizPosActionList/@OTCmlId]"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$vDailyTrades/trade[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!-- RD 20190619 [23912] Display asset details -->
          <!--Display asset details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                           font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="20">
              <!-- Header -->
              <xsl:call-template name="UKAsset_Header">
                <xsl:with-param name="pRepository-Asset" select="$vRepository_Asset"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>

          </fo:table-row>

          <!--Display Trades details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@trdNum" data-type="text"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <xsl:variable name="vBizPosAction" select="$vBizTrade/parent::node()[name()='posAction']"/>
            <xsl:variable name="vPosAction" select="$vAssetPosActionList[@OTCmlId=$vBizPosAction/@OTCmlId]"/>
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId]"/>
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>
            <!-- FI 20231222 [WI788] trade2 existe également sur les trades techniques (trdtype:63) générés par l'action CLOSINGPOS -->
            <xsl:variable name="vBizTrade2" select="$vBizPosAction/trade2 | $vBizTrade/tradeSrc | $vBizTrade/trade2"/>

            <!--On considère les frais sur PosAction s'ils existent sinon on considère les frais sur le trade-->
            <xsl:variable name="vFee" select="$vPosAction/fee | $vEfsmlTrade/fee[$vPosAction=false()]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}" font-weight="{$gData_font-weight}" text-align="{$gData_text-align}" display-align="{$gData_display-align}">
              <xsl:choose>
                <xsl:when test="$vQtyModel='2DateSideQty'">
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:if test ="$vBizTrade/@stlDt">
                        <xsl:call-template name="DisplayData_Format">
                          <xsl:with-param name="pData" select="$vBizTrade/@stlDt"/>
                          <xsl:with-param name="pDataType" select="'Date'"/>
                        </xsl:call-template>
                      </xsl:if>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:variable name="vtrdNumItem">
                    <xsl:choose>
                      <!-- FI 20231222 [WI788] si trade techique Spheres affiche le(s) trade(s) clôturé(s) -->
                      <xsl:when test="$pBizBook/@sectionKey=$gcReportSectionKey_AMT and $vEfsmlTrade/@trdTyp='63'">
                        <xsl:choose>
                          <xsl:when test="$vBizTrade/@trade2Count=1">
                            <xsl:value-of select="$vBizTrade/trade2/@trdNum"/>
                          </xsl:when>
                          <xsl:when test="$vBizTrade/@trade2Count>1">
                            <xsl:value-of select="$vBizTrade/@trade2TrdNumList"/>
                          </xsl:when>
                        </xsl:choose>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$vBizTrade/@trdNum"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vtrdNumItem"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizBook/@sectionKey=$gcReportSectionKey_AMT">
                  <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Type']/data/@font-size}"
                                 font-weight="{$gBlockSettings_Data/column[@name='Type']/data/@font-weight}"
                                 text-align="{$gBlockSettings_Data/column[@name='Type']/data/@text-align}"
                                 padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayAmendmentTransfersData_ActionTyp">
                        <xsl:with-param name="pDataName" select="'ActionTyp'"/>
                        <xsl:with-param name="pFamily" select="$vBizAsset/@family"/>
                        <xsl:with-param name="pPosAction" select="$vPosAction"/>
                        <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTrade"/>
                        <!--RD 20200407 [xxxx] Afficher un trade "Trs.i" s'il existe le trade source, sinon l'afficher en "Late"-->
                        <xsl:with-param name="pTrade2" select="$gTrade[@tradeId=$vBizTrade2/@tradeId]"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:variable name="vDataName">
                    <xsl:choose>
                      <xsl:when test="$vQtyModel='Date2Qty'">
                        <xsl:value-of select="'BuyQty'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'Side'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <!-- RD 20180503 [xxxx] $vPosAction à la place de $vEfsmlTrade -->
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:call-template name="DisplayData_ReturnSwap">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='DSE'">
                      <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='LSD' or $vBizAsset/@family='ESE'">
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:variable name="vDataName">
                    <xsl:choose>
                      <xsl:when test="$vQtyModel='Date2Qty'">
                        <xsl:value-of select="'SellQty'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'Qty'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:call-template name="DisplayData_ReturnSwap">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='DSE'">
                      <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <!-- RD 20160915 [22329][22461] @family='LSD' à la place @family='ETD' -->
                    <xsl:when test="$vBizAsset/@family='LSD' or $vBizAsset/@family='ESE'">
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='LSD'">
                  <fo:table-cell padding-top="{$gData_padding}"
                                 padding-bottom="{$gData_padding}"
                                 text-align="{$gBlockSettings_Data/column[@name='Maturity']/data/@text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'Maturity'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <xsl:choose>
                    <xsl:when test="$pBizDC/@category = 'O'">
                      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayData_RepositoryEtd">
                            <xsl:with-param name="pDataName" select="'PC'"/>
                            <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayData_RepositoryEtd">
                            <xsl:with-param name="pDataName" select="'FmtStrike'"/>
                            <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                            <xsl:with-param name="pNumberPattern" select="$pBizDC/pattern/@strike" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell number-columns-spanned="2">
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <!-- FI 20151019 [21317] call GetCellUnderlying -->
                  <xsl:call-template name ="GetCellUnderlying">
                    <xsl:with-param name ="pRepository_Asset" select ="$vRepository_Asset"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@lastPx" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!--First Fee -->
              <xsl:call-template name="UKDisplayTradeDet_FirstFee">
                <xsl:with-param name="pFee" select="$vFee"/>
              </xsl:call-template>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!-- amount1 -->
              <xsl:variable name="vAmount1" select ="$vBizTrade/amount/amount1 | $vBizPosAction/amount/amount1"/>
              <xsl:choose>
                <xsl:when test ="$vAmount1/*[1]">
                  <xsl:choose>
                    <xsl:when test ="$vAmount1/@det">
                      <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                                 font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                                 padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayType_Short">
                            <xsl:with-param name="pType" select="$vAmount1/@det"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell>
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vAmount1/*[1]"/>
                    <xsl:with-param name="pWithSide" select="$vAmount1/@withside=1"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
            <xsl:variable name="vDisplayTimestamp">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'TimeStamp'"/>
                <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vDisplayTrade2">
              <!-- FI 20231222 [WI788] Exlusion des trades techniques (Le report affiche le(s) trade(s) clôturé(s) sur la 1er ligne et auncune référence du trade technique) -->
              <xsl:if test="$vBizTrade2 and $vEfsmlTrade/@trdTyp!='63'">
                <fo:table-cell font-style="{$gBlockSettings_Data/column[@name='TrdNum']/data[@name='trade2']/@font-style}"
                               color="{$gBlockSettings_Data/column[@name='TrdNum']/data[@name='trade2']/@color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_Format">
                      <xsl:with-param name="pData" select="$vBizTrade2/@trdNum"/>
                      <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell font-style="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@font-style}"
                               text-align="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@text-align}"
                               color="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayAmendmentTransfersData_ActionTyp">
                      <xsl:with-param name="pDataName" select="'ActionTyp2'"/>
                      <xsl:with-param name="pFamily" select="$vBizAsset/@family"/>
                      <xsl:with-param name="pPosAction" select="$vPosAction"/>
                      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTrade"/>
                      <xsl:with-param name="pTrade2" select="$gTrade[@tradeId=$vBizTrade2/@tradeId]"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </xsl:if>
            </xsl:variable>

            <!--Other Fee rows, Timestamp, ...-->
            <xsl:call-template name="UKDisplayRow_TradeDet">
              <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
              <xsl:with-param name="pTrade2" select="$vDisplayTrade2"/>
              <xsl:with-param name="pFee" select="$vFee"/>
              <!-- FI 20180126 [XXXXX] variable pIsQtyModel valorisée avec $vQtyModel='2DateSideQty'-->
              <xsl:with-param name="pIsQtyModel" select="$vQtyModel='2DateSideQty'"/>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:for-each>
        <!--Display underline-->
        <xsl:call-template name="Display_SubTotalSpace">
          <xsl:with-param name="pPosition" select="1"/>
        </xsl:call-template>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplayAmendmentTransfersData_ActionTyp          -->
  <!-- ................................................ -->
  <!-- Summary :                                        
       ................................................ -->
  <!-- FI 20150827 [21287] Add -->
  <!-- FI 20151019 [21317] Modify (Gestion des DebtSecurityTransaction) -->
  <xsl:template name="DisplayAmendmentTransfersData_ActionTyp">
    <!-- Représente l'information à restituer -->
    <xsl:param name="pDataName"/>
    <!-- Représente La famille -->
    <xsl:param name="pFamily"/>
    <!-- pPosAction est renseigné lorsque le trade subi une POC,POT -->
    <xsl:param name="pPosAction"/>
    <!-- pEfsmlTrade est renseigné lorsque le trade est le résultat d'un transfert ou un late trade (Type:trade issu de gDailyTrade) -->
    <xsl:param name="pEfsmlTrade"/>
    <!-- pTrade2 => trade source d'un transfert ou trade résultat d'un transfert (Type: trade issu de datadocument) -->
    <xsl:param name="pTrade2"/>
    <xsl:param name="pDataLength" select ="number('0')"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='ActionTyp'" >
          <xsl:choose>
            <!-- POC : Position correction-->
            <xsl:when test="$pPosAction[@requestType='POC']">
              <xsl:value-of select="'Corr.'"/>
            </xsl:when>
            <!-- POT : Position transfer -->
            <xsl:when test="$pPosAction[@requestType='POT']">
              <xsl:choose>
                <xsl:when test="$pPosAction/trades/trade2">
                  <!--1: InternalTransferOrAdjustment-->
                  <xsl:value-of select="'Trs.o'"/>
                </xsl:when>
                <!--2: ExternalTransferOrTranferOfAccount-->
                <xsl:otherwise>
                  <xsl:value-of select="'Corr.'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <!--4: LateTrade-->
            <xsl:when test="$pEfsmlTrade/@trdTyp = '4'">
              <xsl:value-of select="'Late'" />
            </xsl:when>
            <!-- FI 20231222 [WI788] gestion de TechnicalTrade -->
            <!--63: TechnicalTrade-->
            <xsl:when test="$pEfsmlTrade/@trdTyp = '63'">
              <xsl:value-of select="'Corr.'" />
            </xsl:when>
            <!--42: PortFolioTransfer-->
            <xsl:when test="$pEfsmlTrade/@trdTyp = '42'">
              <!--InternalTransferOrAdjustment: pour les CFD et les DST, par défaut il s'agit de transfert interne-->
              <!--RD 20200407 [xxxx] Afficher un trade "Trs.i" s'il existe le trade source, sinon l'afficher en "Late"-->
              <!--<xsl:value-of select="'Trs.i'"/>-->
              <xsl:choose>
                <xsl:when test="$pTrade2">
                  <!--1: InternalTransferOrAdjustment-->
                  <xsl:value-of select="'Trs.i'"/>
                </xsl:when>
                <!--2: ExternalTransferOrTranferOfAccount-->
                <xsl:otherwise>
                  <xsl:value-of select="'Late'" />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='ActionTyp2'" >
          <xsl:choose>
            <!-- POC : Position correction-->
            <xsl:when test="$pPosAction[@requestType='POC']"/>
            <!-- POT : Position transfer -->
            <xsl:when test="$pPosAction[@requestType='POT']">
              <!--InternalTransferOrAdjustment: pour les CFD, par défaut il s'agit de transfert interne-->
              <xsl:value-of select="concat('to',$gcSpace)"/>
              <xsl:choose>
                <xsl:when test="$pFamily='RTS'">
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'Acct'"/>
                    <xsl:with-param name="pTrade" select="$pTrade2"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$pFamily='DSE'">
                  <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                    <xsl:with-param name="pDataName" select="'Acct'"/>
                    <xsl:with-param name="pTrade" select="$pTrade2"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="($pFamily='LSD') or ($pFamily='ESE')">
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'Acct'"/>
                    <xsl:with-param name="pFixmlTrade" select="$pTrade2"/>
                  </xsl:call-template>
                </xsl:when>
              </xsl:choose>
            </xsl:when>
            <!--4: LateTrade-->
            <xsl:when test="$pEfsmlTrade/@trdTyp = '4'"/>
            <!--42: PortFolioTransfer-->
            <xsl:when test="$pEfsmlTrade/@trdTyp = '42'">
              <!--InternalTransferOrAdjustment: pour les CFD, par défaut il s'agit de transfert interne-->
              <xsl:value-of select="concat('from',$gcSpace)"/>
              <xsl:choose>
                <xsl:when test="$pFamily='RTS'">
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'Acct'"/>
                    <xsl:with-param name="pTrade" select="$pTrade2"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$pFamily='DSE'">
                  <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                    <xsl:with-param name="pDataName" select="'Acct'"/>
                    <xsl:with-param name="pTrade" select="$pTrade2"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="($pFamily='LSD') or ($pFamily='ESE')">
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'Acct'"/>
                    <xsl:with-param name="pFixmlTrade" select="$pTrade2"/>
                  </xsl:call-template>
                </xsl:when>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pDataLength"/>
    </xsl:call-template>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--  Section: LIQUIDATIONS                                                     -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_LiquidationsDeliveriesData             -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Liquidations and Deliveries Content     -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_LiquidationsDeliveriesData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <xsl:variable name="vQtyModel" select="'Date2Qty'"/>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
      <fo:table-column column-number="11" column-width="proportional-column-width(1)"  />
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
      <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
      <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
											   text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="2">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Fee']/@resource"/>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_FeesExclTax"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
											   display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:choose>
                <xsl:when test="$pBizBook/@sectionKey=$gcReportSectionKey_DLV">
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='SettltQty']/@resource"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='RealisedPnL']/@resource"/>
                  <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                    <xsl:value-of select="$gcLegend_RealisedPnL"/>
                  </fo:inline>
                </xsl:otherwise>
              </xsl:choose>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vDailyTrades" select="$gDailyTrades[(string-length($pBizBook/@sectionKey)=0 or $pBizBook/@sectionKey=$gcReportSectionKey_AMT) and (@bizDt=$pBizDt)] | 
                      $gDlvTrades[$pBizBook/@sectionKey=$gcReportSectionKey_DLV and @bizDt=$pBizDt]"/>
        <xsl:variable name="vBookTradeList" select="$gTrade[@tradeId=$pBizBook/asset//trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset[@assetName='etd']">
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizPosActionList" select="$vBizAsset/posAction"/>
          <xsl:variable name="vAssetPosActionList" select="$gPosActions[@bizDt=$pBizDt]/posAction[@OTCmlId=$vAssetBizPosActionList/@OTCmlId]"/>
          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset//trade"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$vDailyTrades/trade[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!-- RD 20190619 [23912] Display asset details -->
          <!--Display asset details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                           font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="20">
              <!-- Header -->
              <xsl:call-template name="UKAsset_Header">
                <xsl:with-param name="pRepository-Asset" select="$vRepository_Asset"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>

          </fo:table-row>
          <!--Display Trades details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@trdNum" data-type="text"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <xsl:variable name="vBizPosAction" select="$vBizTrade/parent::node()[name()='posAction']"/>
            <!-- RD 20170224 [22885] Check Unclearing-->
            <!--<xsl:variable name="vPosAction" select="$vAssetPosActionList[@OTCmlId=$vBizPosAction/@OTCmlId]"/>-->
            <xsl:variable name="vPosAction" select="$vAssetPosActionList[@OTCmlId=$vBizPosAction/@OTCmlId and 
                          ((string-length(@dtUnclearing)=0 and string-length($vBizPosAction/@dtUnclearing)=0) or (@dtUnclearing=$vBizPosAction/@dtUnclearing))]"/>
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId]"/>
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <!--On considère les frais sur PosAction s'ils existent sinon on considère les frais sur le trade-->
            <xsl:variable name="vFee" select="$vBizPosAction/fee | $vPosAction/fee[$vBizPosAction/fee=false()] | $vEfsmlTrade/fee[$vBizPosAction/fee=false() and $vPosAction=false()]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}" font-weight="{$gData_font-weight}" text-align="{$gData_text-align}" display-align="{$gData_display-align}">

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@trdNum"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test ="$pBizBook/@sectionKey=$gcReportSectionKey_DLV">
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test ="$pBizBook/@sectionKey=$gcReportSectionKey_DLV">
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}"
                             padding-bottom="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Maturity']/data/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'Maturity'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@lastPx" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!--First Fee -->
              <xsl:call-template name="UKDisplayTradeDet_FirstFee">
                <xsl:with-param name="pFee" select="$vFee"/>
              </xsl:call-template>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test ="$pBizBook/@sectionKey=$gcReportSectionKey_DLV">
                  <!--amount1-->
                  <xsl:call-template name ="UKDisplay_OtherAmount" >
                    <xsl:with-param name ="pOtherAmount" select ="$vBizTrade/amount/amount1"/>
                    <xsl:with-param name="pQtyPattern" select="$vBizAsset/pattern/@qty" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name ="UKDisplay_OtherAmount" >
                    <xsl:with-param name ="pOtherAmount" select ="$vBizPosAction/amount/amount1"/>
                    <xsl:with-param name="pQtyPattern" select="$vBizAsset/pattern/@qty" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>

            <xsl:variable name="vDisplayTimestamp">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'TimeStamp'"/>
                <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
              </xsl:call-template>
            </xsl:variable>
            <!--Other Fee rows, Timestamp, ...-->
            <xsl:call-template name="UKDisplayRow_TradeDet">
              <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
              <xsl:with-param name="pFee" select="$vFee"/>
              <xsl:with-param name="pOtherAmount2" select="$vBizTrade/amount/amount2"/>
              <xsl:with-param name="pOtherAmount3" select="$vBizTrade/amount/amount3"/>
              <xsl:with-param name="pOtherAmount4" select="$vBizTrade/amount/amount4"/>
              <xsl:with-param name="pQtyPattern" select="$vBizAsset/pattern/@qty" />
              <!-- FI 20180126 [XXXXX]  variable pIsQtyModel valorisée avec $vQtyModel='2DateSideQty'-->
              <xsl:with-param name="pIsQtyModel" select="$vQtyModel='2DateSideQty'"/>
            </xsl:call-template>
          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vBizSubTotal" select="$vBizAsset/subTotal"/>
          <xsl:variable name="vBizSubTotalAmount" select="$vBizSubTotal/amount"/>

          <!--Subtotal row, with first Fee-->
          <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="2">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'LongQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                  <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'ShortQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                  <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <xsl:call-template name="Display_AddAttribute-color">
                <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
              </xsl:call-template>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayData_Format">
                  <xsl:with-param name="pData" select="$vBizAsset/@expDt"/>
                  <xsl:with-param name="pDataType" select="'Date'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-weight}"
                           text-align="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}" number-columns-spanned="3">
              <xsl:call-template name="Display_AddAttribute-color">
                <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@color"/>
              </xsl:call-template>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:variable name="vFmtSettlPx" select="$vAssetPosActionList[1]/@fmtUnlPx | $vAssetEfsmlTradeList[1][$vAssetPosActionList=false()]/@fmtStlPx"/>
                <xsl:if test ="string-length($vFmtSettlPx)>0">
                  <xsl:call-template name="DisplaySubTotalColumnSettlPx">
                    <xsl:with-param name="pFmtPrice" select="$vFmtSettlPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@unlPx" />
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!--First Subtotal Fee row-->
            <xsl:call-template name="UKDisplay_SubTotal_OneFee">
              <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
            </xsl:call-template>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- Subtotal amount 1-->
            <!-- RD 20170224 [22885] Modify-->
            <xsl:call-template name="UKDisplay_SubTotal_OtherAmount">
              <xsl:with-param name="pOtherAmount" select="$vBizSubTotalAmount/amount1"/>
              <xsl:with-param name="pQtyPattern" select="$vBizAsset/pattern/@qty" />
            </xsl:call-template>
          </fo:table-row>

          <xsl:variable name="vColumns-spanned">
            <xsl:choose>
              <xsl:when test="$vQtyModel='2DateSideQty'">
                <xsl:value-of select="'12'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'11'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!--Other Subtotal Fee rows-->
          <xsl:call-template name="UKDisplayRow_SubTotalDet">
            <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
            <xsl:with-param name="pOtherAmount2" select="$vBizSubTotalAmount/amount2"/>
            <xsl:with-param name="pOtherAmount3" select="$vBizSubTotalAmount/amount3"/>
            <xsl:with-param name="pOtherAmount4" select="$vBizSubTotalAmount/amount4"/>
            <xsl:with-param name="pColumns-spanned" select="$vColumns-spanned" />
            <xsl:with-param name="pQtyPattern" select="$vBizAsset/pattern/@qty" />
          </xsl:call-template>

          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>


  <!-- .......................................................................... -->
  <!--   Section: EXERCISES / ASSIGNMENTS / ABANDONMENTS                          -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_OptionSettlementData                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Settlement Option Content               -->
  <!-- ................................................ -->
  <!-- FI 20160208 [21311] Modify-->
  <xsl:template name="Synthesis_OptionSettlementData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
      <fo:table-column column-number="11" column-width="proportional-column-width(1)"  />
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
      <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
      <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Fee']/@resource"/>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_FeesExclTax"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='RealisedPnL']/@resource"/>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_RealisedPnL"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gTrade[@tradeId=$pBizBook/asset[@assetName='etd']/posAction/trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset[@assetName='etd']">
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizPosActionList" select="$vBizAsset/posAction"/>
          <xsl:variable name="vAssetPosActionList" select="$gPosActions[@bizDt=$pBizDt]/posAction[@OTCmlId=$vAssetBizPosActionList/@OTCmlId]"/>
          <xsl:variable name="vAssetETDTradeList" select="$vBookTradeList[@tradeId=$vAssetBizPosActionList/trade/@tradeId]"/>

          <!-- RD 20190619 [23912] Display asset details -->
          <!--Display asset details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                           font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="20">
              <!-- Header -->
              <xsl:call-template name="UKAsset_Header">
                <xsl:with-param name="pRepository-Asset" select="$vRepository_Asset"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>

          </fo:table-row>
          <!--Display Trades details-->
          <xsl:for-each select="$vAssetBizPosActionList">
            <xsl:sort select="$vBookTradeList[@tradeId=current()/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt" data-type="number"/>
            <xsl:sort select="$vBookTradeList[@tradeId=current()/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx" data-type="number"/>
            <xsl:sort select="current()/trade/@trdNum" data-type="text"/>

            <xsl:variable name="vBizPosAction" select="current()"/>
            <xsl:variable name="vPosAction" select="$vAssetPosActionList[@OTCmlId=$vBizPosAction/@OTCmlId]"/>
            <xsl:variable name="vTrade" select="$vAssetETDTradeList[@tradeId=$vBizPosAction/trade/@tradeId]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}" font-weight="{$gData_font-weight}" text-align="{$gData_text-align}" display-align="{$gData_display-align}">

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBookTradeList[@tradeId=$vBizPosAction/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizPosAction/trade/@trdNum"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                    <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                    <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}"
                             padding-bottom="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Maturity']/data/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'Maturity'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'PC'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'FmtStrike'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                    <xsl:with-param name="pNumberPattern" select="$pBizDC/pattern/@strike" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$vBizPosAction/trade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@lastPx" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!--First Fee -->
              <xsl:call-template name="UKDisplayTradeDet_FirstFee">
                <xsl:with-param name="pFee" select="$vPosAction/fee"/>
              </xsl:call-template>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!-- FI 20160208 [21311] lecture de $vBizPosAction/amount -->
              <!--  amount1 -->
              <xsl:variable name="vAmount1" select ="$vBizPosAction/amount/amount1"/>
              <xsl:choose>
                <xsl:when test ="$vAmount1/*[1]">
                  <xsl:choose>
                    <xsl:when test ="$vAmount1/@det">
                      <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                                 font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                                 padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayType_Short">
                            <xsl:with-param name="pType" select="$vAmount1/@det"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell>
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vAmount1/*[1]"/>
                    <xsl:with-param name="pWithSide" select="$vAmount1/@withside=1"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>


            <!--Other Fee rows, Timestamp, ...-->
            <xsl:variable name="vDisplayTimestamp">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'TimeStamp'"/>
                <xsl:with-param name="pDataEfsml" select="$vBizPosAction/trade"/>
              </xsl:call-template>
            </xsl:variable>

            <!-- FI 20160208 [21311] lecture de $vBizPosAction/amount2 -->
            <xsl:call-template name="UKDisplayRow_TradeDet">
              <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
              <xsl:with-param name="pFee" select="$vPosAction/fee"/>
              <xsl:with-param name="pOtherAmount2" select="$vBizPosAction/amount/amount2"/>
            </xsl:call-template>
          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vBizSubTotal" select="$vBizAsset/subTotal"/>
          <xsl:variable name="vBizSubTotalAmount" select="$vBizSubTotal/amount"/>

          <!--Subtotal row, with first Fee-->
          <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="2">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'LongQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                  <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'ShortQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                  <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <xsl:call-template name="Display_AddAttribute-color">
                <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
              </xsl:call-template>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <!-- 20160229 [XXXXX] Add if pour ne pas afficher NaN-->
                <xsl:if test ="string-length($vBizAsset/@expDt)>0">
                  <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                  <xsl:value-of select="$gcSpace"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizAsset/@expDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='UnlSettlPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='UnlSettlPx']/@font-weight}"
                           text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}" number-columns-spanned="3">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>

                <xsl:if test ="string-length($vAssetPosActionList[1]/@fmtUnlPx)>0">
                  <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='UnlSettlPx']/data/@resource"/>
                  <xsl:value-of select="$gcSpace"/>
                  <xsl:call-template name="DisplayData_Efsml">
                    <xsl:with-param name="pDataName" select="'FmtUnlPx'"/>
                    <xsl:with-param name="pDataEfsml" select="$vAssetPosActionList[1]"/>
                    <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@unlPx" />
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!--First Subtotal Fee row-->
            <xsl:call-template name="UKDisplay_SubTotal_OneFee">
              <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
            </xsl:call-template>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- Subtotal amount 1-->
            <xsl:variable name="vAmount1" select ="$vBizSubTotalAmount/amount1"/>
            <xsl:choose>
              <xsl:when test ="$vAmount1/*[1]">
                <xsl:variable name="vTotal"  select="$vAmount1/*[1]/@amt"/>
                <xsl:variable name="vTotalCcy" select="$vAmount1/*[1]/@ccy"/>
                <xsl:choose>
                  <xsl:when test ="$vAmount1/@det">
                    <xsl:variable name="vTotalColor">
                      <xsl:if test="$gIsColorMode and $vAmount1/@withside=1">
                        <xsl:call-template name="GetAmount-color">
                          <xsl:with-param name="pAmount" select="$vTotal"/>
                        </xsl:call-template>
                      </xsl:if>
                    </xsl:variable>
                    <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                               font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                               padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                      <xsl:if test ="string-length($vTotalColor)>0">
                        <xsl:call-template name="Display_AddAttribute-color">
                          <xsl:with-param name="pColor" select="$vTotalColor"/>
                        </xsl:call-template>
                      </xsl:if>
                      <fo:block>
                        <xsl:call-template name="Debug_border-green"/>
                        <xsl:call-template name="DisplayType_Short">
                          <xsl:with-param name="pType" select="$vAmount1/@det"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </xsl:when>
                  <xsl:otherwise>
                    <fo:table-cell>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </xsl:otherwise>
                </xsl:choose>

                <xsl:call-template name="UKDisplay_SubTotal_Amount">
                  <xsl:with-param name="pAmount" select="$vTotal" />
                  <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                  <xsl:with-param name="pWithSide" select="$vAmount1/@withside=1"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-row>

          <!-- FI 20160208 [21311] Add Subtotal amount 2-->
          <!-- Subtotal amount 2-->

          <!--Other Subtotal Fee rows-->
          <xsl:call-template name="UKDisplayRow_SubTotalDet">
            <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
            <xsl:with-param name="pOtherAmount2" select="$vBizSubTotalAmount/amount2"/>
          </xsl:call-template>

          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>


  <!-- .......................................................................... -->
  <!--  Section: PURCHASE & SALE                                                  -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_PurchaseSaleCascadingData              -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display PurchaseSale and Cascading Content      -->
  <!-- ................................................ -->
  <!-- FI 20160208 [21311] Modify -->
  <xsl:template name="Synthesis_PurchaseSaleCascadingData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <xsl:variable name="vCBMethod" select ="$gCBTrade/cashBalanceReport/settings/cashBalanceMethod"/>

    <xsl:variable name="vQtyModel">
      <xsl:choose>
        <xsl:when test="$pBizBook/asset/@family='ESE'">
          <xsl:value-of select="'2DateSideQty'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Date2Qty'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">

      <xsl:choose>
        <xsl:when test="$vQtyModel='Date2Qty'">
          <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
          <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
          <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
          <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
          <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
          <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
          <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
          <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
          <fo:table-column column-number="11" column-width="proportional-column-width(1)"  />
          <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
          <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
        </xsl:when>
        <xsl:when test="$vQtyModel='2DateSideQty'">
          <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
          <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
          <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Side']/@column-width}" />
          <fo:table-column column-number="06" column-width="{$gBlockSettings_Data/column[@name='SideQty2']/@column-width}" />
          <fo:table-column column-number="07" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
          <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
          <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
          <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
          <fo:table-column column-number="12" column-width="proportional-column-width(1)"  />
          <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
          <fo:table-column column-number="17" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="21" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
        </xsl:when>
      </xsl:choose>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
											text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <xsl:choose>
            <xsl:when test="$vQtyModel='2DateSideQty'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='SettltDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$vQtyModel='Date2Qty'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Side']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Side']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='SideQty2']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='SideQty2']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family='LSD'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O'">
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
																 display-align="{$gData_display-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
																 display-align="{$gData_display-align}"
                                 text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="2">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name ="UnderlyingHeader">
                    <xsl:with-param name ="pFamily" select ="$pBizBook/asset/@family"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="contains(',LSD,ESE,',concat(',',$pBizBook/asset/@family,','))">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Fee']/@resource"/>
                  <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                    <xsl:value-of select="$gcLegend_FeesExclTax"/>
                  </fo:inline>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family='ESE'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="4">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='RealisedPnL']/@resource"/>
                  <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                    <xsl:value-of select="$gcLegend_RealisedPnL"/>
                  </fo:inline>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gTrade[@tradeId=$pBizBook/asset/trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>
          <xsl:sort select="$gRepository/*[name()=current()/@assetName and @OTCmlId=current()/@OTCmlId]/displayname" data-type="text"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset/trade"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!-- RD 20190619 [23912] Display asset details -->
          <!--Display asset details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                           font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="20">
              <!-- Header -->
              <xsl:call-template name="UKAsset_Header">
                <xsl:with-param name="pRepository-Asset" select="$vRepository_Asset"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>

          </fo:table-row>
          <!--Display Trades details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <!--Tri pour les deux modèles PnLOnClosing et OverallQty-->
            <xsl:sort select="current()/@closedTrdDt" data-type="text"/>
            <xsl:sort select="current()/@closedLastPx" data-type="number"/>
            <xsl:sort select="current()/@closedTrdNum" data-type="text"/>
            <xsl:sort select="current()/@closedTradeId" data-type="text"/>
            <xsl:sort select="current()/@isClosed" data-type="text" order="descending"/>
            <!--Tri pour les deux modèles PnLOnClosing et OverallQty-->
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@trdNum" data-type="text"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}" font-weight="{$gData_font-weight}" text-align="{$gData_text-align}" display-align="{$gData_display-align}">

              <xsl:choose>
                <xsl:when test="$vQtyModel='2DateSideQty'">
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@stlDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@trdNum"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='LSD'">
                  <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Type']/data/@font-size}"
                                 font-weight="{$gBlockSettings_Data/column[@name='Type']/data/@font-weight}"
                                 text-align="{$gBlockSettings_Data/column[@name='Type']/data/@text-align}"
                                 padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="'TrdTypPos'"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Type']/data/@length"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:variable name="vDataName">
                    <xsl:choose>
                      <xsl:when test="$vQtyModel='Date2Qty'">
                        <xsl:value-of select="'PosBuyQty'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'Side'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:call-template name="DisplayData_ReturnSwap">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:choose>
                        <xsl:when test="$vQtyModel='Date2Qty'">
                          <xsl:call-template name="DisplayData_ReturnSwap">
                            <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                            <xsl:with-param name="pTrade" select="$vTrade"/>
                            <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                            <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                          </xsl:call-template>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:call-template name="DisplayData_Efsml">
                            <xsl:with-param name="pDataName" select="'Qty'"/>
                            <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                            <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                          </xsl:call-template>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:choose>
                        <xsl:when test="$vQtyModel='Date2Qty'">
                          <xsl:call-template name="DisplayData_Fixml">
                            <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                            <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                            <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                            <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                          </xsl:call-template>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:call-template name="DisplayData_Efsml">
                            <xsl:with-param name="pDataName" select="'Qty'"/>
                            <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                            <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                          </xsl:call-template>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='LSD'">
                  <fo:table-cell padding-top="{$gData_padding}"
                                 padding-bottom="{$gData_padding}"
                                 text-align="{$gBlockSettings_Data/column[@name='Maturity']/data/@text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'Maturity'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <xsl:choose>
                    <xsl:when test="$pBizDC/@category = 'O'">
                      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayData_RepositoryEtd">
                            <xsl:with-param name="pDataName" select="'PC'"/>
                            <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayData_RepositoryEtd">
                            <xsl:with-param name="pDataName" select="'FmtStrike'"/>
                            <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                            <xsl:with-param name="pNumberPattern" select="$pBizDC/pattern/@strike" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell number-columns-spanned="2">
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <!-- FI 20151019 [21317] call GetCellUnderlying -->
                  <xsl:call-template name ="GetCellUnderlying">
                    <xsl:with-param name ="pRepository_Asset" select ="$vRepository_Asset"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@lastPx" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!--First Fee -->
              <xsl:call-template name="UKDisplayTradeDet_FirstFee">
                <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
              </xsl:call-template>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='LSD'">
                  <xsl:choose>
                    <!-- FI 20160208 [21113] ajout du  or $vCBMethod = 'CSBDEFAULT' -->
                    <xsl:when test="$pBizDC/@futValuationMethod = 'FUT' or $vCBMethod = 'CSBDEFAULT' ">
                      <xsl:call-template name="UKDisplay_Amount">
                        <xsl:with-param name="pAmount" select="$vBizTrade/rmg"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizTrade/rmg">
                      <xsl:call-template name="UKDisplay_Amount">
                        <xsl:with-param name="pAmount" select="number('0')"/>
                        <xsl:with-param name="pCcy" select="$vBizTrade/rmg/@ccy"/>
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vBizTrade/rmg"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
            <!--Other Fee rows, Timestamp, ...-->
            <xsl:variable name="vDisplayTimestamp">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'TimeStamp'"/>
                <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="UKDisplayRow_TradeDet">
              <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
              <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
              <!-- FI 20180126 [XXXXX]  variable pIsQtyModel valorisée avec $vQtyModel='2DateSideQty'-->
              <xsl:with-param name="pIsQtyModel" select="$vQtyModel='2DateSideQty'"/>
            </xsl:call-template>
          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vBizSubTotal" select="$vBizAsset/subTotal[1]"/>

          <!--Subtotal row-->
          <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <xsl:choose>
              <!-- EG 20160308 Migration vs2013 Padding -->
              <xsl:when test="$vQtyModel='Date2Qty'">
                <fo:table-cell number-columns-spanned="2">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block >
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="3">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <xsl:choose>
                  <xsl:when test="$pBizBook/asset/@family='ESE'">
                    <fo:table-cell>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </xsl:when>
                  <xsl:otherwise>
                    <!-- EG 20160308 Migration vs2013 Padding -->
                    <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Side']/data/@background-color}">
                      <fo:block >
                        <xsl:call-template name="Debug_border-green"/>
                        <xsl:call-template name="DisplayData_SubTotal">
                          <xsl:with-param name="pDataName" select="'BuySide'"/>
                          <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </xsl:otherwise>
                </xsl:choose>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      <xsl:with-param name="pQtyModel" select="$vQtyModel"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$vBizAsset/@family='LSD'">
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                               font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
                  <xsl:call-template name="Display_AddAttribute-color">
                    <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                  </xsl:call-template>
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <!-- 20160229 [XXXXX] Add if pour ne pas afficher NaN-->
                    <xsl:if test ="string-length($vBizAsset/@expDt)>0">
                      <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                      <xsl:value-of select="$gcSpace"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizAsset/@expDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
            <fo:table-cell number-columns-spanned="3">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!--First Subtotal Fee row-->
            <xsl:call-template name="UKDisplay_SubTotal_OneFee">
              <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
            </xsl:call-template>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$pBizBook/asset/@family='ESE'">
                <fo:table-cell number-columns-spanned="3">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <xsl:variable name="vCcy_RealisedPnL">
                  <xsl:call-template name="GetAmount-ccy">
                    <xsl:with-param name="pAmount" select="$vBizSubTotal/rmg"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:variable name="vTotal_RealisedPnL">
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='LSD'">
                      <xsl:choose>
                        <!-- FI 20160208 [21113] ajout du  or $vCBMethod = 'CSBDEFAULT' -->
                        <xsl:when test="$pBizDC/@futValuationMethod = 'FUT' or $vCBMethod = 'CSBDEFAULT'">
                          <xsl:call-template name="GetAmount-amt">
                            <xsl:with-param name="pAmount" select="$vBizSubTotal/rmg"/>
                            <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
                          </xsl:call-template>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:call-template name="GetAmount-amt">
                            <xsl:with-param name="pAmount" select="number('0')"/>
                            <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
                          </xsl:call-template>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="GetAmount-amt">
                        <xsl:with-param name="pAmount" select="$vBizSubTotal/rmg"/>
                        <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:call-template name="UKDisplay_SubTotal_Amount">
                  <xsl:with-param name="pAmount" select="$vTotal_RealisedPnL" />
                  <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-row>
          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family!='ESE' and $vQtyModel!='Date2Qty' and $vBizSubTotal/long/@fmtQty > 0 and $vBizSubTotal/short/@fmtQty > 0">
              <!--Subtotal row, with second Fee-->
              <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                            text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">

                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Side']/data/@background-color}">
                  <fo:block >
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'SellSide'"/>
                      <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <fo:table-cell number-columns-spanned="3">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!--Second Subtotal Fee row-->
                <xsl:call-template name="UKDisplay_SubTotal_OneFee">
                  <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
                  <xsl:with-param name="pFeeNumber" select="'2'" />
                </xsl:call-template>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </fo:table-row>
              <!--Other Subtotal Fee rows-->
              <xsl:call-template name="UKDisplayRow_SubTotalDet">
                <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
                <xsl:with-param name="pFeeStartNumber" select="'3'" />
                <xsl:with-param name="pColumns-spanned" select="'12'" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <!--Other Subtotal Fee rows-->
              <xsl:call-template name="UKDisplayRow_SubTotalDet">
                <xsl:with-param name="pFee" select="$vBizSubTotal/fee" />
                <xsl:with-param name="pColumns-spanned" select="'12'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pPosition" select="position()"/>
            <xsl:with-param name="pNumber-columns" select="number('21')"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>


  <!-- .......................................................................... -->
  <!--  Section: OPEN POSITIONS                                                   -->
  <!-- .......................................................................... -->
  <!-- ................................................ -->
  <!-- Synthesis_OpenPositionData                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display OpenPosition Content                    -->
  <!-- ................................................ -->
  <!-- FI 20150716 [20892] Refactoring -->
  <xsl:template name="Synthesis_OpenPositionData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <xsl:variable name="vQtyModel">
      <xsl:choose>
        <xsl:when test="$pBizBook/asset/@family='ESE' or $pBizBook/asset/@family='DSE'">
          <xsl:value-of select="'2DateSideQty'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Date2Qty'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">

      <!-- EG 20160308 Migration vs2013 ok -->
      <xsl:choose>
        <xsl:when test="$vQtyModel='Date2Qty'">
          <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
          <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
          <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
          <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
          <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
          <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
          <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
          <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
          <fo:table-column column-number="11" column-width="proportional-column-width(1)"  />
          <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
          <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
        </xsl:when>
        <xsl:when test="$vQtyModel='2DateSideQty'">
          <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
          <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
          <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
          <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Side']/@column-width}" />
          <fo:table-column column-number="06" column-width="{$gBlockSettings_Data/column[@name='SideQty2']/@column-width}" />
          <fo:table-column column-number="07" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
          <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
          <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
          <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
          <fo:table-column column-number="12" column-width="proportional-column-width(1)"  />
          <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
          <fo:table-column column-number="17" column-width="{$gColumnSpace_width}"/>
          <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
          <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
          <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
          <fo:table-column column-number="21" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
        </xsl:when>
      </xsl:choose>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <xsl:choose>
            <xsl:when test="$vQtyModel='2DateSideQty'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='SettltDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>

          <xsl:choose>
            <xsl:when test="$vQtyModel='Date2Qty'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='long']/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='long']/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='short']/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='short']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Side']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Side']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='SideQty2']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='SideQty2']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>

          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family='LSD'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O'">
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
															   display-align="{$gData_display-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                                 padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
																 display-align="{$gData_display-align}"
                                 text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="2">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name ="UnderlyingHeader">
                    <xsl:with-param name ="pFamily" select ="$pBizBook/asset/@family"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizBook/asset/@family='RTS'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Margin']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:when test="$pBizBook/asset/@family='ESE' or $pBizBook/asset/@family='DSE'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='QtyAssessed']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="4">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='MarketRevaluation']/@resource"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>

        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>

      <fo:table-body>
        <xsl:variable name="vPosTrades" select="$gPosTrades[(string-length($pBizBook/@sectionKey)=0 or $pBizBook/@sectionKey=$gcReportSectionKey_POS) and (@bizDt=$pBizDt)]
                      | $gStlPosTrades[$pBizBook/@sectionKey=$gcReportSectionKey_STL and (@bizDt=$pBizDt)]"/>

        <!--Liste des trades (de type datadocument/trade) rattaché au book courant-->
        <xsl:variable name="vBookTradeList" select="$gTrade[@tradeId=$pBizBook/asset/trade/@tradeId]"/>
        <!--Liste des ss-totaux rattaché au book courant-->
        <xsl:variable name="vBookSubTotal" select="$vPosTrades/subTotal[@idB=$pBizBook/@OTCmlId and @idI=$pBizBook/asset/@idI]"/>
        <!--Liste des trades en position rattaché au book courant-->
        <xsl:variable name="vBookEfsmlTradeList" select="$vPosTrades/trade[@tradeId=$vBookTradeList/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd['etd'=current()/@assetName and @OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>
          <xsl:sort select="$gRepository/*[name()=current()/@assetName and @OTCmlId=current()/@OTCmlId]/displayname" data-type="text"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset/trade"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <!-- vAssetPosTradeList
               => Liste des noeuds trade enfant de posTrades 
                 (Lecture des noeuds trade de posTrades même si le noeud principal est stlPosTrades) 
          -->
          <xsl:variable name="vAssetPosTradeList" select="$gPosTrades[@bizDt=$pBizDt]/trade[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!-- RD 20190619 [23912] Display asset details -->
          <!--Display asset details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}">

            <fo:table-cell text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                           font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="20">
              <!-- Header -->
              <xsl:call-template name="UKAsset_Header">
                <xsl:with-param name="pRepository-Asset" select="$vRepository_Asset"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>

          </fo:table-row>

          <!--Display Trades details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()[$vQtyModel!='2DateSideQty']/@stlDt" data-type="text"/>
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@trdNum" data-type="text"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <!--Only one EfsmlTrade-->
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId]"/>
            <!--Only one vTrade-->
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <fo:table-row font-size="{$gData_font-size}" font-weight="{$gData_font-weight}" text-align="{$gData_text-align}" display-align="{$gData_display-align}">

              <xsl:if test="position()=1">
                <xsl:attribute name="keep-with-previous">
                  <xsl:value-of select="'always'"/>
                </xsl:attribute>
              </xsl:if>

              <xsl:choose>
                <xsl:when test="$vQtyModel='2DateSideQty'">
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@stlDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@trdNum"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='LSD'">
                  <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Type']/data/@font-size}"
                                 font-weight="{$gBlockSettings_Data/column[@name='Type']/data/@font-weight}"
                                 text-align="{$gBlockSettings_Data/column[@name='Type']/data/@text-align}"
                                 padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="'TrdTypPos'"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Type']/data/@length"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:variable name="vDataName">
                    <xsl:choose>
                      <xsl:when test="$vQtyModel='Date2Qty'">
                        <xsl:value-of select="'PosBuyQty'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'PosSide'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:call-template name="DisplayData_ReturnSwap">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='DSE'">
                      <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='LSD' or $vBizAsset/@family='ESE'">
                      <xsl:call-template name="DisplayData_Fixml">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$vBizAsset/@family='RTS'">
                      <xsl:choose>
                        <xsl:when test="$vQtyModel='Date2Qty'">
                          <xsl:call-template name="DisplayData_ReturnSwap">
                            <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                            <xsl:with-param name="pTrade" select="$vTrade"/>
                            <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                            <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                          </xsl:call-template>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:call-template name="DisplayData_Efsml">
                            <xsl:with-param name="pDataName" select="'Qty'"/>
                            <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                            <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                          </xsl:call-template>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='DSE'">
                      <xsl:variable name="vDataName">
                        <xsl:choose>
                          <xsl:when test="$vQtyModel='Date2Qty'">
                            <xsl:value-of select="'PosSellQty'"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="'Qty'"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:variable>
                      <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                        <xsl:with-param name="pDataName" select="$vDataName"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
                        <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizAsset/@family='ESE' or $vBizAsset/@family='LSD'">
                      <xsl:choose>
                        <xsl:when test="$vQtyModel='Date2Qty'">
                          <xsl:call-template name="DisplayData_Fixml">
                            <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                            <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                            <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                            <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                          </xsl:call-template>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:call-template name="DisplayData_Efsml">
                            <xsl:with-param name="pDataName" select="'Qty'"/>
                            <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                            <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                          </xsl:call-template>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>

              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='LSD'">
                  <fo:table-cell padding-top="{$gData_padding}"
                                 padding-bottom="{$gData_padding}"
                                 text-align="{$gBlockSettings_Data/column[@name='Maturity']/data/@text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'Maturity'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <xsl:choose>
                    <xsl:when test="$pBizDC/@category = 'O'">
                      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayData_RepositoryEtd">
                            <xsl:with-param name="pDataName" select="'PC'"/>
                            <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayData_RepositoryEtd">
                            <xsl:with-param name="pDataName" select="'FmtStrike'"/>
                            <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                            <xsl:with-param name="pNumberPattern" select="$pBizDC/pattern/@strike" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell number-columns-spanned="2">
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:variable  name ="vNumberRowsSpanned">
                    <xsl:choose>
                      <xsl:when test ="$vBizTrade/amount/amount4">
                        <xsl:value-of select ="4"/>
                      </xsl:when>
                      <xsl:when test ="$vBizTrade/amount/amount3">
                        <xsl:value-of select ="3"/>
                      </xsl:when>
                      <xsl:when test ="$vBizTrade/amount/amount2">
                        <xsl:value-of select ="2"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select ="1"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <!-- EG 20190730 Upd : New -->
                  <xsl:variable name ="vRepository_AdditionalInfo">
                    <xsl:if test ="$vBizAsset/@family='DSE' and string-length($vBizTrade/@accIntDays)>0">
                      <xsl:value-of select ="concat('Acc. Days: ',$vBizTrade/@accIntDays)"/>
                    </xsl:if>
                  </xsl:variable>

                  <!-- FI 20151019 [21317] call GetCellUnderlying -->
                  <!-- EG 20190730 Upd : Add pAdditionalInfo parameter -->
                  <xsl:call-template name ="GetCellUnderlying">
                    <xsl:with-param name ="pRepository_Asset" select ="$vRepository_Asset"/>
                    <xsl:with-param name ="pAdditionalInfo" select ="$vRepository_AdditionalInfo"/>
                    <xsl:with-param name ="pNumberRowsSpanned" select ="$vNumberRowsSpanned"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@lastPx" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>

              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='RTS'">
                  <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                                 font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                                 padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:if test="$vEfsmlTrade/mgr/@factor">
                        <xsl:variable name="vFactor">
                          <xsl:call-template name="DisplayPercentage">
                            <xsl:with-param name="pFactor" select="$vEfsmlTrade/mgr/@factor"/>
                          </xsl:call-template>
                        </xsl:variable>
                        <xsl:value-of select="concat('(',$vFactor,')')"/>
                      </xsl:if>
                    </fo:block>
                  </fo:table-cell>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vEfsmlTrade/mgr"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$pBizBook/asset/@family='ESE' or $pBizBook/asset/@family='DSE'">
                  <!--FI 20150716 [20892] Add vQtyAssessed -->
                  <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}" number-columns-spanned="4">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:variable name ="vQtyAssessed"
                                    select="$vAssetPosTradeList[@OTCmlId=$vEfsmlTrade/@OTCmlId]/@fmtQty">

                      </xsl:variable>
                      <xsl:if test ="$vQtyAssessed">
                        <!--<xsl:call-template name="format-integer">
                          <xsl:with-param name="integer" select="$vQtyAssessed"/>
                        </xsl:call-template>-->
                        <xsl:call-template name="DisplayFmtNumber">
                          <xsl:with-param name="pFmtNumber" select="$vQtyAssessed"/>
                          <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@qty" />
                        </xsl:call-template>
                      </xsl:if>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>

              <!-- FI 20150127 [XXXXX] Lecture systematique de amount1  -->
              <!-- amount1 -->
              <xsl:variable name="vAmount1" select ="$vBizTrade/amount/amount1"/>
              <xsl:choose>
                <xsl:when test ="$vAmount1/*[1]">
                  <xsl:choose>
                    <xsl:when test ="$vAmount1/@det">
                      <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
																		 font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                                     padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                        <fo:block>
                          <xsl:call-template name="Debug_border-green"/>
                          <xsl:call-template name="DisplayType_Short">
                            <xsl:with-param name="pType" select="$vAmount1/@det"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell>
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vAmount1/*[1]"/>
                    <xsl:with-param name="pWithSide" select="$vAmount1/@withside=1"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>

            <!--Other rows: Timestamp, ...-->
            <xsl:variable name="vDisplayTimestamp">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'TimeStamp'"/>
                <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:call-template name="UKDisplayRow_TradeDet">
              <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
              <xsl:with-param name="pOtherAmount2" select="$vBizTrade/amount/amount2"/>
              <xsl:with-param name="pOtherAmount3" select="$vBizTrade/amount/amount3"/>
              <xsl:with-param name="pOtherAmount4" select="$vBizTrade/amount/amount4"/>
              <xsl:with-param name="pQtyPattern" select="$vBizAsset/pattern/@qty" />
              <!-- FI 20180126 [XXXXX]  variable pIsQtyModel valorisée avec $vQtyModel='2DateSideQty'-->
              <xsl:with-param name="pIsQtyModel" select="$vQtyModel='2DateSideQty'"/>
            </xsl:call-template>

          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vSubTotal" select="$vBookSubTotal[@idAsset=$vBizAsset/@OTCmlId and @assetCategory=$vBizAsset/@assetCategory and @idI=$vBizAsset/@idI][1]"/>
          <xsl:variable name="vBizSubTotal" select="$vBizAsset/subTotal[1]"/>
          <xsl:variable name="vIsQty2Row" select="$vQtyModel!='Date2Qty' and $vSubTotal/long/@fmtQty > 0 and $vSubTotal/short/@fmtQty > 0"/>

          <xsl:variable name="vCcy_MGR">
            <xsl:if test="$vBizAsset/@family='RTS'">
              <xsl:call-template name="GetAmount-ccy">
                <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/mgr"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:variable>
          <xsl:variable name="vTotal_MGR">
            <xsl:if test="$vBizAsset/@family='RTS'">
              <xsl:call-template name="GetAmount-amt">
                <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/mgr"/>
                <xsl:with-param name="pCcy" select="$vCcy_MGR"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:variable>
          <xsl:variable name="vColor_MGR">
            <xsl:if test="$vBizAsset/@family='RTS'">
              <xsl:call-template name="GetAmount-color">
                <xsl:with-param name="pAmount" select="$vTotal_MGR"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:variable>
          <xsl:variable name="vBGColor_MGR">
            <xsl:if test="$vBizAsset/@family='RTS'">
              <xsl:call-template name="GetAmount-background-color">
                <xsl:with-param name="pAmount" select="$vTotal_MGR"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:variable>

          <xsl:variable name="vCcy_MRV">
            <xsl:choose>
              <xsl:when test="$vBizAsset/@family='LSD' and $pBizDC/@futValuationMethod = 'EQTY'">
                <xsl:call-template name="GetAmount-ccy">
                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/nov"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$vBizAsset/@family='ESE' or $vBizAsset/@family='DSE'">
                <xsl:choose>
                  <xsl:when test="$vAssetPosTradeList/mkv">
                    <xsl:call-template name="GetAmount-ccy">
                      <xsl:with-param name="pAmount" select="$vAssetPosTradeList/mkv"/>
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="GetAmount-ccy">
                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vTotal_MRV">
            <xsl:choose>
              <xsl:when test="$vBizAsset/@family='LSD' and $pBizDC/@futValuationMethod = 'EQTY'">
                <xsl:call-template name="GetAmount-amt">
                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/nov"/>
                  <xsl:with-param name="pCcy" select="$vCcy_MRV"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$vBizAsset/@family='ESE' or $vBizAsset/@family='DSE'">
                <xsl:call-template name="GetAmount-amt">
                  <xsl:with-param name="pAmount" select="$vAssetPosTradeList/mkv"/>
                  <xsl:with-param name="pCcy" select="$vCcy_MRV"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="GetAmount-amt">
                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
                  <xsl:with-param name="pCcy" select="$vCcy_MRV"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vColor_MRV">
            <xsl:call-template name="GetAmount-color">
              <xsl:with-param name="pAmount" select="$vTotal_MRV"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vBGColor_MRV">
            <xsl:call-template name="GetAmount-background-color">
              <xsl:with-param name="pAmount" select="$vTotal_MRV"/>
            </xsl:call-template>
          </xsl:variable>

          <!--Subtotal: First Row-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="center" keep-with-previous="always">
            <!--text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">-->
            <!-- pavé : Trade infos Début -->
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$vQtyModel='Date2Qty'">
                <!-- EG 20160308 Migration vs2013 -->
                <fo:table-cell number-columns-spanned="2">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block >
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <!-- EG 20160308 Migration vs2013 -->
                <fo:table-cell number-columns-spanned="3">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Side']/data/@background-color}">
                  <fo:block >
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'NetSide'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell text-align="{$gData_Number_text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'NetQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      <xsl:with-param name="pQtyModel" select="$vQtyModel"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
            <!-- pavé : Trade infos Fin -->
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>

            <!-- Pavé :Asset + prix Début -->
            <xsl:choose>
              <xsl:when test="$vBizAsset/@family='LSD'">
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                               font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">

                  <xsl:variable name="vColor">
                    <xsl:call-template name="GetExpiry-color">
                      <xsl:with-param name="pExpInd" select="$vRepository_Asset/expInd/text()"/>
                      <xsl:with-param name="pDefault-color" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:call-template name="Display_AddAttribute-color">
                    <xsl:with-param name="pColor" select="$vColor"/>
                  </xsl:call-template>
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <!-- 20160229 [XXXXX] Add if pour ne pas afficher NaN-->
                    <xsl:if test ="string-length($vBizAsset/@expDt)>0">
                      <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                      <xsl:value-of select="$gcSpace"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizAsset/@expDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-size}"
                               font-weight="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-weight}"
                               text-align="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               number-columns-spanned="2" wrap-option="no-wrap">
                  <xsl:call-template name="Display_AddAttribute-color">
                    <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@color"/>
                  </xsl:call-template>
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:choose>
                      <xsl:when test="($vBizAsset/@family='ESE' or $vBizAsset/@family='DSE')">
                        <xsl:if test ="$vBizAsset/closing">
                          <!-- EG 20190730 Upd : AssetMeasure and accIntRt -->
                          <xsl:call-template name="DisplaySubTotalColumnAssetMeasureClosePrice">
                            <xsl:with-param name="pFmtPrice" select="$vBizAsset/closing/@clrPx"/>
                            <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@clrPx" />
                            <xsl:with-param name="pResName" select="$vBizAsset/closing/@meaClrPx"/>
                          </xsl:call-template>
                          <xsl:if test ="$vBizAsset/@family='DSE'">
                            <xsl:value-of select="$gcSpace"/>
                            <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='AccRate']/data/@resource"/>
                            <xsl:value-of select="$gcSpace"/>
                            <xsl:call-template name="DisplayFmtNumber">
                              <xsl:with-param name="pFmtNumber" select="$vBizAsset/closing/@accIntRt"/>
                              <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@accIntRt" />
                            </xsl:call-template>
                          </xsl:if>
                        </xsl:if>
                      </xsl:when>
                      <xsl:otherwise>
                        <!-- FI 20150915 [21315] Affichage de la ressource SettlPx uniquement s'il existe un prix -->
                        <xsl:if test ="$vAssetPosTradeList[1]/@fmtClrPx">
                          <xsl:call-template name="DisplaySubTotalColumnSettlPx">
                            <xsl:with-param name="pFmtPrice" select="$vAssetPosTradeList[1]/@fmtClrPx"/>
                            <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@clrPx" />
                          </xsl:call-template>
                        </xsl:if>
                      </xsl:otherwise>
                    </xsl:choose>
                  </fo:block>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>

            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-weight}"
                           text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">

              <xsl:attribute name="number-columns-spanned">
                <xsl:choose>
                  <xsl:when test="$vBizAsset/@family='LSD'">
                    <xsl:value-of select="'3'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'2'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>

              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="UKDisplay_SubTotal_AvgPx">
                  <xsl:with-param name="pDataName" select="'AvgBuy'"/>
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                  <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@avgPx"/>
                  <xsl:with-param name="pQtyModel" select="$vQtyModel"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- Pavé :Asset + prix Fin -->
            <xsl:choose>
              <xsl:when test="$vBizAsset/@family='LSD'">
                <!-- EG 20160308 Migration vs2013 Padding -->
                <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-size}"
                               font-weight="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-weight}"
                               text-align="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@text-align}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                               padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                               number-columns-spanned="4">
                  <xsl:call-template name="Display_AddAttribute-color">
                    <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@color"/>
                  </xsl:call-template>
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <!-- FI 20151007 [21311] call Template DisplaySubTotalColumnSettlPx -->
                    <xsl:call-template name="DisplaySubTotalColumnSettlPx">
                      <xsl:with-param name="pFmtPrice" select="$vAssetEfsmlTradeList[1]/@fmtClrPx"/>
                      <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@clrPx" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <xsl:when test="$vBizAsset/@family='RTS'">
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <xsl:call-template name="UKDisplay_SubTotal_Amount">
                  <xsl:with-param name="pAmount" select="$vTotal_MGR"/>
                  <xsl:with-param name="pCcy" select="$vCcy_MGR"/>
                  <xsl:with-param name="pColor" select="$vColor_MGR" />
                  <xsl:with-param name="pBackground-color" select="$vBGColor_MGR" />
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$pBizBook/asset/@family='ESE'">
                <!--TODO: Display Fees subtotal-->
                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>

            <xsl:choose>
              <xsl:when test="$vBizAsset/@family='ESE' or $vBizAsset/@family='DSE'">
                <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                               font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
															 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
															 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">

                  <xsl:call-template name="Display_AddAttribute-color">
                    <xsl:with-param name="pColor" select="$vColor_MRV"/>
                  </xsl:call-template>
                  <fo:block>
                    <xsl:call-template name="Debug_border-green"/>
                    <xsl:choose>
                      <xsl:when test="$vAssetPosTradeList/mkv">
                        <xsl:call-template name="DisplayType_Short">
                          <xsl:with-param name="pType" select="'OPV'"/>
                        </xsl:call-template>
                      </xsl:when>
                    </xsl:choose>
                  </fo:block>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:call-template name="UKDisplay_SubTotal_Amount">
              <xsl:with-param name="pAmount" select="$vTotal_MRV"/>
              <xsl:with-param name="pCcy" select="$vCcy_MRV"/>
              <xsl:with-param name="pColor" select="$vColor_MRV" />
              <xsl:with-param name="pBackground-color" select="$vBGColor_MRV" />
            </xsl:call-template>

          </fo:table-row>

          <!--Total in countervalue: First Row-->
          <xsl:if test="$gIsDisplayCountervalue = 'true' and 
                  ((string-length($vCcy_MGR) >0 and $vCcy_MGR != $gExchangeCcy) or (string-length($vCcy_MRV) >0 and $vCcy_MRV != $gExchangeCcy))">

            <xsl:variable name="vFxRate_MGR_Node">
              <xsl:if test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gExchangeCcy">
                <xsl:call-template name="GetExchangeRate_Repository">
                  <xsl:with-param name="pFlowCcy" select="$vCcy_MGR" />
                </xsl:call-template>
              </xsl:if>
            </xsl:variable>
            <xsl:variable name="vFxRate_MGR" select="msxsl:node-set($vFxRate_MGR_Node)/fxrate"/>
            <xsl:variable name="vFxRate_MRV_Node">
              <xsl:if test="string-length($vCcy_MRV) >0 and $vCcy_MRV != $gExchangeCcy">
                <xsl:choose>
                  <xsl:when test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gExchangeCcy and $vCcy_MGR = $vCcy_MRV">
                    <xsl:copy-of select="$vFxRate_MGR"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="GetExchangeRate_Repository">
                      <xsl:with-param name="pFlowCcy" select="$vCcy_MRV" />
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:if>
            </xsl:variable>
            <xsl:variable name="vFxRate_MRV" select="msxsl:node-set($vFxRate_MRV_Node)/fxrate"/>

            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                          text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">

              <!-- EG 20160308 Migration vs2013 Padding -->
              <!--RD 20170602 [xxxxx] Change number-columns-spanned from 7 to 6-->
              <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='TotalInCV']/@text-align}"
														 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                             padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                             number-columns-spanned="6">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='TotalInCV']/data/@resource"/>
                </fo:block>
              </fo:table-cell>
              <!-- EG 20160308 Migration vs2013 Padding -->
              <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SpotRate']/@font-size}"
                             font-weight="{$gBlockSettings_Data/subtotal/column[@name='SpotRate']/@font-weight}"
                             text-align="{$gData_Number_text-align}"
														 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                             padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                             number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gExchangeCcy">
                    <xsl:call-template name="DisplayeExchangeRate">
                      <xsl:with-param name="pExchangeRate" select="$vFxRate_MGR" />
                      <xsl:with-param name="pFlowCcy" select="$vCcy_MGR" />
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gExchangeCcy and 
                          string-length($vCcy_MRV) >0 and $vCcy_MRV != $gExchangeCcy and $vCcy_MGR != $vCcy_MRV">
                    <xsl:value-of select="' - '"/>
                  </xsl:if>
                  <xsl:if test="string-length($vCcy_MRV) >0 and $vCcy_MRV != $gExchangeCcy and $vCcy_MGR != $vCcy_MRV">
                    <xsl:call-template name="DisplayeExchangeRate">
                      <xsl:with-param name="pExchangeRate" select="$vFxRate_MRV" />
                      <xsl:with-param name="pFlowCcy" select="$vCcy_MRV" />
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gExchangeCcy">
                  <xsl:variable name="vExAmount">
                    <xsl:call-template name="GetExchangeAmount">
                      <xsl:with-param name="pAmount" select="$vTotal_MGR" />
                      <xsl:with-param name="pExchangeRate" select="$vFxRate_MGR"/>
                      <xsl:with-param name="pFlowCcy" select="$vCcy_MGR" />
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:call-template name="UKDisplay_SubTotal_Amount">
                    <xsl:with-param name="pAmount" select="$vExAmount" />
                    <xsl:with-param name="pCcy" select="$gReportingCcy"/>
                    <xsl:with-param name="pAmountPattern" select="$gExchangeCcyPattern" />
                    <xsl:with-param name="pColor" select="$vColor_MGR" />
                    <xsl:with-param name="pBackground-color" select="$vBGColor_MGR" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>

              <xsl:choose>
                <xsl:when test="$vBizAsset/@family='ESE' or $vBizAsset/@family='DSE'">
                  <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                                 font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
														 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
														 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">

                    <xsl:call-template name="Display_AddAttribute-color">
                      <xsl:with-param name="pColor" select="$vColor_MRV"/>
                    </xsl:call-template>
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:choose>
                        <xsl:when test="$vAssetTradeList/mkv">
                          <xsl:call-template name="DisplayType_Short">
                            <xsl:with-param name="pType" select="'OPV'"/>
                          </xsl:call-template>
                        </xsl:when>
                      </xsl:choose>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:choose>
                <xsl:when test="string-length($vCcy_MRV) >0 and $vCcy_MRV != $gExchangeCcyPattern">
                  <xsl:variable name="vExAmount">
                    <xsl:call-template name="GetExchangeAmount">
                      <xsl:with-param name="pAmount" select="$vTotal_MRV" />
                      <xsl:with-param name="pExchangeRate" select="$vFxRate_MRV"/>
                      <xsl:with-param name="pFlowCcy" select="$vCcy_MRV" />
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:call-template name="UKDisplay_SubTotal_Amount">
                    <xsl:with-param name="pAmount" select="$vExAmount" />
                    <xsl:with-param name="pCcy" select="$gReportingCcy"/>
                    <xsl:with-param name="pAmountPattern" select="$gExchangeCcyPattern" />
                    <xsl:with-param name="pColor" select="$vColor_MRV" />
                    <xsl:with-param name="pBackground-color" select="$vBGColor_MRV" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>

            </fo:table-row>
          </xsl:if>

          <!--Display underline-->
          <!-- EG 20160308 Migration vs2013 number('21') -->
          <xsl:choose>
            <xsl:when test="$vQtyModel='2DateSideQty'">
              <xsl:call-template name="Display_SubTotalSpace">
                <xsl:with-param name="pPosition" select="position()"/>
                <xsl:with-param name="pNumber-columns" select="number('21')"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="Display_SubTotalSpace">
                <xsl:with-param name="pPosition" select="position()"/>
                <xsl:with-param name="pNumber-columns" select="number('20')"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>


  <!-- ................................................ -->
  <!-- Synthesis_OpenPositionFx                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Fx OpenPosition Content                 -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_OpenPositionFx">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizCurrency"/>
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">
      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}" />
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
      <fo:table-column column-number="11" column-width="proportional-column-width(1)"  />
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
      <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}" />
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}" />
      <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}" />
      <fo:table-column column-number="20" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}" />
      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Ccy']/header[@name='Dealt']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:choose>
                <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='Expiry']/@resource"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='ValueDate']/@resource"/>
                </xsl:otherwise>
              </xsl:choose>
            </fo:block>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}"
                             text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="2">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Rate']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="2">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" number-columns-spanned="3">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
														 display-align="{$gData_display-align}" number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDF'">
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='RefAmount']/@resource"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='NetAmount']/@resource"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}" number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='MarketRevaluation']/@resource"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gTrade[@tradeId=$pBizBook/dealt/trade/@tradeId]"/>
        <xsl:variable name="vBookEfsmlTradeList" select="$gPosTrades[@bizDt=$pBizDt]/trade[@tradeId=$vBookTradeList/@tradeId]"/>

        <xsl:for-each select="$pBizBook/dealt">
          <xsl:sort select="current()/@ccy" data-type="text"/>

          <xsl:variable name="vBizDealt" select="current()"/>

          <xsl:variable name="vDealtBizTradeList" select="$vBizDealt/trade"/>
          <xsl:variable name="vDealtEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vDealtBizTradeList/@tradeId]"/>
          <xsl:variable name="vDealtTradeList" select="$vBookTradeList[@tradeId=$vDealtBizTradeList/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vDealtBizTradeList">
            <xsl:sort select="current()/@valDt" data-type="text"/>
            <xsl:sort select="current()/@expDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <!--Only one EfsmlTrade-->
            <xsl:variable name="vEfsmlTrade" select="$vDealtEfsmlTradeList[@tradeId=$vBizTrade/@tradeId][1]"/>
            <xsl:variable name="vTrade" select="$vDealtTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <!-- 1/ Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}" font-weight="{$gData_font-weight}" text-align="{$gData_text-align}" display-align="{$gData_display-align}">

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Efsml">
                    <xsl:with-param name="pDataName" select="'TrdDt'"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@tradeId"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                      <xsl:call-template name="DisplayData_FxOptionLeg">
                        <xsl:with-param name="pDataName" select="'BuyQty'"/>
                        <xsl:with-param name="pFxOptionLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_FxLeg">
                        <xsl:with-param name="pDataName" select="'BuyQty'"/>
                        <xsl:with-param name="pFxLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                      <xsl:call-template name="DisplayData_FxOptionLeg">
                        <xsl:with-param name="pDataName" select="'SellQty'"/>
                        <xsl:with-param name="pFxOptionLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_FxLeg">
                        <xsl:with-param name="pDataName" select="'SellQty'"/>
                        <xsl:with-param name="pFxLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$vBizDealt/@ccy"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <xsl:if test="$pBizBook/dealt/@family='FxNDO'">
                  <xsl:variable name="vColor">
                    <xsl:call-template name="GetExpiry-color">
                      <xsl:with-param name="pExpInd" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@expInd"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:call-template name="Display_AddAttribute-color">
                    <xsl:with-param name="pColor" select="$vColor"/>
                  </xsl:call-template>
                </xsl:if>

                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@expDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade/@valDt"/>
                        <xsl:with-param name="pDataType" select="'Date'"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                      <xsl:call-template name="DisplayData_FxOptionLeg">
                        <xsl:with-param name="pDataName" select="'Strike'"/>
                        <xsl:with-param name="pFxOptionLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayData_FxLeg">
                        <xsl:with-param name="pDataName" select="'CurrencyPair'"/>
                        <xsl:with-param name="pFxLegTrade" select="$vTrade"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizDealt/pattern/@lastPx" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="Display_NetAmount_FxLeg">
                    <xsl:with-param name="pFxLegTrade" select="$vTrade" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vEfsmlTrade/nov"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vEfsmlTrade/umg"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
            <!--RD 20200818 [25456] Faire le cumul des frais par trade -->
            <xsl:choose>
              <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                <xsl:call-template name="DisplayTradeDetRow_FxOptionLeg">
                  <xsl:with-param name="pFamily" select="$pBizBook/dealt/@family"/>
                  <xsl:with-param name="pFxOptionLegTrade" select="$vTrade"/>
                  <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="DisplayTradeDetRow_FxLeg">
                  <xsl:with-param name="pFamily" select="$pBizBook/dealt/@family"/>
                  <xsl:with-param name="pFxLegTrade" select="$vTrade"/>
                  <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vBizSubTotal" select="$vBizDealt/subTotal[1]"/>

          <!--Subtotal row-->
          <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="2">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                    <xsl:call-template name="DisplaySubtotal_FxOptionLeg">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pFxOptionLegTrades" select="$vDealtTradeList"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="DisplaySubtotal_FxLeg">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pFxLegTrades" select="$vDealtTradeList"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gData_Number_text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                    <xsl:call-template name="DisplaySubtotal_FxOptionLeg">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pFxOptionLegTrades" select="$vDealtTradeList"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="DisplaySubtotal_FxLeg">
                      <xsl:with-param name="pDataName" select="'ShortQty'"/>
                      <xsl:with-param name="pFxLegTrades" select="$vDealtTradeList"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
													 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$vBizDealt/@ccy"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-weight}"
                           text-align="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="3">
              <xsl:call-template name="Display_AddAttribute-color">
                <xsl:with-param name="pColor" select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@color"/>
              </xsl:call-template>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                    <!-- FI 20151007 [21311] call Template DisplaySubTotalColumnSettlPx -->
                    <xsl:call-template name="DisplaySubTotalColumnSettlPx">
                      <xsl:with-param name="pFmtPrice" select="$vDealtEfsmlTradeList[1]/@fmtClrPx"/>
                      <xsl:with-param name="pPattern" select="$vBizDealt/pattern/@clrPx"/>
                      <xsl:with-param name="pResName" select ="'FxNDO'"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <!-- FI 20151007 [21311] call Template DisplaySubTotalColumnSettlPx -->
                    <xsl:call-template name="DisplaySubTotalColumnSettlPx">
                      <xsl:with-param name="pFmtPrice" select="$vDealtEfsmlTradeList[1]/@fmtClrPx"/>
                      <xsl:with-param name="pPattern" select="$vBizDealt/pattern/@clrPx"/>
                      <xsl:with-param name="pResName" select ="'TheoPrice'"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="2">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                <fo:table-cell number-columns-spanned="3">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="DisplaySubtotal_NetAmount_FxLeg">
                  <xsl:with-param name="pFxLegTrades" select="$vDealtTradeList" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$pBizBook/dealt/@family='FxNDO'">
                <xsl:call-template name="UKDisplay_SubTotal_Amount">
                  <xsl:with-param name="pAmount" select="$vDealtEfsmlTradeList/nov"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="UKDisplay_SubTotal_Amount">
                  <xsl:with-param name="pAmount" select="$vDealtEfsmlTradeList/umg"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-row>
        </xsl:for-each>
        <!--Subtotal row, for main Currency-->
        <xsl:if test="$pBizBook/dealt/@family!='FxNDO'">
          <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}" display-align="{$gBlockSettings_Data/subtotal/@display-align}" keep-with-previous="always">
            <!-- EG 20160308 Migration vs2013 Padding -->
            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
													 padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="concat($gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource,$gcSpace,$pBizCurrency/@ccy)"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="12">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:call-template name="DisplaySubtotal_NetAmount_FxLeg">
              <xsl:with-param name="pFxLegTrades" select="$vBookTradeList" />
            </xsl:call-template>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:call-template name="UKDisplay_SubTotal_Amount">
              <xsl:with-param name="pAmount" select="$vBookEfsmlTradeList/umg"/>
            </xsl:call-template>
          </fo:table-row>
        </xsl:if>
        <!--Display space-->
        <xsl:call-template name="Display_SubTotalSpace">
          <xsl:with-param name="pNumber-columns" select="number('20')"/>
          <xsl:with-param name="pPosition" select="last()"/>
        </xsl:call-template>
      </fo:table-body>
    </fo:table>
  </xsl:template>


  <!-- .......................................................................... -->
  <!--  Section: ACCOUNT SUMMARY                                                  -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_AccountSummaryBody                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Account Summary Content                 -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Modify -->
  <!-- RD 20170119 [22797] Modify -->
  <xsl:template name="Synthesis_AccountSummaryBody">
    <xsl:param name="pBizGroup"/>

    <xsl:variable name="vCashBalanceStreams" select="$gCBTrade/cashBalanceReport/cashBalanceStream[@ccy=$pBizGroup/currency/@ccy]"/>
    <xsl:variable name="vExchangeCashBalanceStream" select="$gCBTrade/cashBalanceReport/exchangeCashBalanceStream"/>
    <xsl:variable name="vCBMethod" select ="$gCBTrade/cashBalanceReport/settings/cashBalanceMethod"/>

    <!--RD 20170119 [22797] 
    Afficher les taux de change:
    - Si le pavé "Base currency" est à afficher
    - Et si on n'est pas sur un groupe avec uniquement le pavé "Base currency"-->
    <!--<xsl:if test="$pBizGroup/exchangeCurrency">-->
    <xsl:if test="$pBizGroup/@isHideBaseCcy = 'false' and $pBizGroup/currency">
      <!--SpotRate-->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'SpotRate'"/>
      </xsl:call-template>
      <!--Row Empty-->
      <xsl:call-template name="Synthesis_SummaryRowEmpty">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pWithBorder" select="false()"/>
      </xsl:call-template>
    </xsl:if>

    <!-- FI 20150921 [21311] 2 types d'affichage -->
    <xsl:choose>
      <xsl:when test ="$vCBMethod = 'CSBUK'">
        <xsl:call-template name ="Synthesis_AccountSummaryBody_UKMethod">
          <xsl:with-param name ="pBizGroup" select ="$pBizGroup"/>
          <xsl:with-param name ="pCashBalanceStreams" select ="$vCashBalanceStreams"/>
          <xsl:with-param name ="pExchangeCashBalanceStream" select ="$vExchangeCashBalanceStream" />
        </xsl:call-template>
      </xsl:when>

      <xsl:when test ="$vCBMethod = 'CSBDEFAULT'">
        <xsl:call-template name ="Synthesis_AccountSummaryBody_DEFAULTMethod">
          <xsl:with-param name ="pBizGroup" select ="$pBizGroup"/>
          <xsl:with-param name ="pCashBalanceStreams" select ="$vCashBalanceStreams"/>
          <xsl:with-param name ="pExchangeCashBalanceStream" select ="$vExchangeCashBalanceStream" />
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>

  </xsl:template>


  <!-- ........................................................... -->
  <!-- Synthesis_AccountSummaryBody_DEFAULTMethod                -->
  <!-- ........................................................... -->
  <!-- Summary :                                                   -->
  <!--  Display Account Summary Content (Method Default)(IT/FR)    -->
  <!-- ........................................................... -->
  <!-- FI 20150921 [21311] Add -->
  <!-- RD 20160406 [21284] Modify -->
  <xsl:template name="Synthesis_AccountSummaryBody_DEFAULTMethod">
    <!-- BizGroup -->
    <xsl:param name="pBizGroup"/>
    <!-- Représente les streams du CashBalance dont les devises sont présente dans pBizGroup -->
    <xsl:param name="pCashBalanceStreams"/>
    <!-- Représente le exchangeStream du CashBalance  -->
    <xsl:param name="pExchangeCashBalanceStream"/>

    <xsl:variable name="vAllCashBalanceStreams" select="$gCBTrade/cashBalanceReport/cashBalanceStream"/>

    <!--PreviousCashBalance-->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="'PreviousCashBalance'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/previousCashBalance"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/previousCashBalance"/>
      <xsl:with-param name="pMode" select="'master'"/>
    </xsl:call-template>

    <!--CashBalancePayment-->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="'CashBalancePayment'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashBalancePayment"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashBalancePayment"/>
    </xsl:call-template>

    <!--RD 20170929 [23403] Corporate Actions: Equalisation payment-->
    <!-- Equalisation payment-->
    <xsl:if test="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/equalisationPayment/detail">
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'EqualisationPayment'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/equalisationPayment"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/equalisationPayment"/>
      </xsl:call-template>
    </xsl:if>

    <!--Premium-->
    <xsl:if test="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/premium/detail">
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'Premium'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/premium"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/premium"/>
      </xsl:call-template>
    </xsl:if>

    <!-- Variation Margin-->
    <xsl:if test="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/variationMargin/detail">
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'VariationMargin'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/variationMargin"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/variationMargin"/>
      </xsl:call-template>
    </xsl:if>

    <xsl:choose>
      <xsl:when test="$gIsCOMActivity=true()">
        <!--RealisedProfitLoss-->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'RealisedProfitLoss'"/>
          <!--cashSettlement alimenté à partir des GAM pour les famille EST, DST et à partir des INT (coupon) sur les DST -->
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/globalAmount
                          | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/globalAmount
                          | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <!-- CashSettlement-->
        <xsl:if test="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail">
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="'CashSettlement'"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>

    <!-- Fee-->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="'Fee'"/>
      <xsl:with-param name="pAmount" select="$pBizGroup/fee/amount | $pBizGroup/fee/tax/amount |
                                             $pBizGroup/safekeeping/amount | $pBizGroup/safekeeping/tax/amount"/>
      <xsl:with-param name="pExchangeAmount" select="$pBizGroup/exchangeCurrency/fee/amount | $pBizGroup/exchangeCurrency/fee/tax/amount"/>
    </xsl:call-template>

    <!-- RD 20200214 [25146] Dans le cas où on n'arrive pas à définir l'activité (gIsMarginingActivity/gIsCOMActivity), affichage du MarginCall s'il existe -->
    <xsl:variable name="isMarginCall" select="$vAllCashBalanceStreams/marginCall or $vAllCashBalanceStreams/previousMarginConstituent"/>
    <xsl:variable name="isMarginRequirement" select="$vAllCashBalanceStreams/marginRequirement/detail"/>

    <xsl:choose>
      <xsl:when test="$gIsMarginingActivity">
        <!-- Margin Call-->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'MarginCall_CSBDEFAULT'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marginCall"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marginCall"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$gIsCOMActivity">
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'InitialMarginCall'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marginRequirement"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marginRequirement"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:when>
      <!-- RD 20200214 [25146] Dans le cas où on n'arrive pas à définir l'activité (gIsMarginingActivity/gIsCOMActivity), affichage du MarginCall s'il existe -->
      <xsl:when test="$isMarginCall">
        <!-- Margin Call-->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'MarginCall_CSBDEFAULT'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marginCall"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marginCall"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$isMarginRequirement">
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'InitialMarginCall'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marginRequirement"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marginRequirement"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>

    <!-- CashBalance-->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="'CashBalance_CSBDEFAULT'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashBalance"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashBalance"/>
      <xsl:with-param name="pMode" select="'subtotal'"/>
    </xsl:call-template>

    <!--Empty Row-->
    <xsl:call-template name="Synthesis_SummaryRowEmpty">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>

    <!-- RD 20190619 [23912] Display additionnal amounts -->
    <!--RealisedProfitLoss-->
    <xsl:if test="$pCashBalanceStreams/realizedMargin/detail | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail">
      <xsl:choose>
        <xsl:when test="$gIsETDActivity=true()">
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="'RealisedProfitLoss'"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/future
                            | $pCashBalanceStreams/realizedMargin/option[@FutValMeth='FUT']
                            | $pCashBalanceStreams/realizedMargin/other
                            | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option
                            | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/future
                            | $pExchangeCashBalanceStream/realizedMargin/option[@FutValMeth='FUT']
                            | $pExchangeCashBalanceStream/realizedMargin/other
                            | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option
                            | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="'RealisedProfitLoss'"/>
            <!--cashSettlement alimenté à partir des GAM pour les famille EST, DST et à partir des INT (coupon) sur les DST -->
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/globalAmount
                            | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/globalAmount
                            | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <!--RealisedProfitLoss-->
    <xsl:choose>
      <xsl:when test="$gIsETDActivity=true()">
        <!--UnrealizedMargin-->
        <xsl:if test="$pCashBalanceStreams/unrealizedMargin/detail">
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="'UnrealisedMargin'"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/future
                            | $pCashBalanceStreams/unrealizedMargin/option[@FutValMeth='FUT']
                            | $pCashBalanceStreams/unrealizedMargin/other"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/future
                            | $pExchangeCashBalanceStream/unrealizedMargin/option[@FutValMeth='FUT']
                            | $pExchangeCashBalanceStream/unrealizedMargin/other"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          </xsl:call-template>
        </xsl:if>
        <!--NetOptionValue-->
        <xsl:if test="$pCashBalanceStreams/liquidatingValue">
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="'NetOptionValue'"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <xsl:when test="$gIsCOMActivity=true()">
        <!--UnsettledTransactions-->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'UnsettledTransactions'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unsettledCash"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unsettledCash"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
        <!--TotalEqty-->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'TotalEqty'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unsettledCash | $pCashBalanceStreams/cashBalance"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unsettledCash | $pExchangeCashBalanceStream/cashBalance"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pMode" select="'master'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <!--MarketRevaluation-->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'MarketRevaluation'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/globalAmount"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/globalAmount"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>


    <xsl:variable name="vIsDisplayFutPnL" select="$vAllCashBalanceStreams/realizedMargin/future[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOptPnL" select="$vAllCashBalanceStreams/realizedMargin/option[@amt!=0 and @FutValMeth='FUT'] = true()"/>
    <xsl:variable name="vIsDisplayOptCshSettl" select="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option[@amt!=0] = true()"/>

    <xsl:variable name="vIsDisplayFutUMG" select="$vAllCashBalanceStreams/unrealizedMargin/future[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOptUMG" select="$vAllCashBalanceStreams/unrealizedMargin/option[@amt!=0 and @FutValMeth='FUT'] = true()"/>
    <xsl:variable name="vIsDisplayOthersUMG" select="$vAllCashBalanceStreams/unrealizedMargin/other[@amt!=0] = true()"/>

    <xsl:variable name="vIsDisplayLongNOV" select="$vAllCashBalanceStreams/liquidatingValue/longOptionValue[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayShortNOV" select="$vAllCashBalanceStreams/liquidatingValue/shortOptionValue[@amt!=0] = true()"/>

    <!--Il existe des trades ETD et des trades OTC-->
    <xsl:variable name="vIsDisplayOthersPnL" select="$vAllCashBalanceStreams/realizedMargin/other[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOthersCshSettl" select="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other[@amt!=0] = true()"/>

    <!-- FI 20150716 [20892] Add safekeeping-->
    <xsl:variable name="vIsDisplayFees" select="(($pBizGroup/fee/amount[@amt !=0] or $pBizGroup/exchangeCurrency/fee/amount[@amt !=0]) = true()) or 
                  (($pBizGroup/safekeeping/amount[@amt !=0] or $pBizGroup/exchangeCurrency/safekeeping/amount[@amt !=0]) = true())"/>

    <xsl:variable name="vIsDisplaySummaryFees"
                  select="($gSettings-headerFooter=false() or $gSettings-headerFooter/summaryFees/text() != 'None') and ($vIsDisplayFees)"/>

    <xsl:variable name="vIsDisplaySummaryCashFlows"
                  select="(($gSettings-headerFooter=false()) or $gSettings-headerFooter/summaryCashFlows/text() != 'None') and
                  ($vIsDisplayFutPnL or $vIsDisplayOptPnL or $vIsDisplayOthersPnL or 
                  $vIsDisplayOptCshSettl or $vIsDisplayOthersCshSettl or 
                  $vIsDisplayFutUMG or $vIsDisplayOptUMG or $vIsDisplayOthersUMG) or 
                  $vIsDisplayLongNOV or $vIsDisplayShortNOV"/>

    <!-- RD 20200214 [25146] Dans le cas où on n'arrive pas à définir l'activité (gIsMarginingActivity/gIsCOMActivity), affichage du MarginCall s'il existe -->
    <xsl:if test="$vIsDisplaySummaryFees or $vIsDisplaySummaryCashFlows or $gIsMarginingActivity or $isMarginCall">
      <!-- Séparation avant affichage du Détail -->
      <xsl:call-template name ="Synthesis_SummaryBeforeDetail">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
    </xsl:if>

    <!--Fee Detail-->
    <xsl:if test="$vIsDisplaySummaryFees">
      <xsl:call-template name="Synthesis_SummaryFeeDetail">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
    </xsl:if>

    <!-- RD 20200214 [25146] Dans le cas où on n'arrive pas à définir l'activité (gIsMarginingActivity/gIsCOMActivity), affichage du MarginCall s'il existe -->
    <xsl:if test="$gIsMarginingActivity or $isMarginCall">
      <!-- MarginCall Detail -->
      <xsl:call-template name="Synthesis_SummaryMarginCallDetail">
        <xsl:with-param name="pBizGroup" select ="$pBizGroup"/>
        <xsl:with-param name="pCashBalanceStreams"  select ="$pCashBalanceStreams"/>
        <xsl:with-param name="pExchangeCashBalanceStream" select ="$pExchangeCashBalanceStream"/>
      </xsl:call-template>
    </xsl:if>

    <!--Empty Row-->
    <xsl:call-template name="Synthesis_SummaryRowTitle">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>

    <!--RealisedProfitLoss Detail-->
    <xsl:if test="$vIsDisplaySummaryCashFlows and 
            ($vIsDisplayFutPnL or $vIsDisplayOptPnL or $vIsDisplayOthersPnL or 
            $vIsDisplayOptCshSettl or $vIsDisplayOthersCshSettl)">
      <!--RealisedProfitLossDetail : title-->
      <xsl:call-template name="Synthesis_SummaryRowTitle">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pTitleName" select="'RealisedProfitLoss'"/>
      </xsl:call-template>
      <!--RealisedProfitLossFutures-->
      <xsl:if test="$vIsDisplayFutPnL">
        <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='RealisedProfitLoss']/@resourceDetailFutures"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/future"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/future"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--RealisedProfitLossOptions-->
      <xsl:if test="$vIsDisplayOptPnL">
        <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='RealisedProfitLoss']/@resourceDetailOptions"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/option[@FutValMeth='FUT']"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/option[@FutValMeth='FUT']"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--RealisedProfitLossOthers-->
      <xsl:if test="$vIsDisplayOthersPnL">
        <!-- FI 20160422 [XXXXX] Call Synthesis_SummaryRowAmountDetailAssetCategory (détail par asset category sur les CFD) -->
        <xsl:call-template name ="Synthesis_SummaryRowAmountDetailAssetCategory" >
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pDataName" select="'RealisedProfitLoss'"/>
          <xsl:with-param name="pCashBalanceStreamsDetail" select="$pCashBalanceStreams/realizedMargin/detail[@assetCategory != 'ExchangeTradedContract'] "/>
          <xsl:with-param name="pExchangeCashBalanceStreamDetail" select="$pCashBalanceStreams/realizedMargin/detail[@assetCategory != 'ExchangeTradedContract']"/>
        </xsl:call-template>
      </xsl:if>
      <!--CashSettlementOptions-->
      <xsl:if test="$vIsDisplayOptCshSettl">
        <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'CashSettlementOptions'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--CashSettlementOthers-->
      <xsl:if test="$vIsDisplayOthersCshSettl">
        <!-- FI 20151019 [21317] Call Synthesis_SummaryRowAmountDetailAssetCategory -->
        <xsl:call-template name ="Synthesis_SummaryRowAmountDetailAssetCategory" >
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pDataName" select="'CashSettlementOptions'"/>
          <xsl:with-param name="pCashBalanceStreamsDetail" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail[@assetCategory != 'ExchangeTradedContract'] "/>
          <xsl:with-param name="pExchangeCashBalanceStreamDetail" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail[@assetCategory != 'ExchangeTradedContract']"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>

    <!--UnrealizedMargin Detail-->
    <xsl:if test="$vIsDisplaySummaryCashFlows and 
            ($vIsDisplayFutUMG or $vIsDisplayOptUMG or $vIsDisplayOthersUMG)">
      <!--UnrealizedMarginDetail  : title-->
      <xsl:call-template name="Synthesis_SummaryRowTitle">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pTitleName" select="'UnrealisedMargin'"/>
      </xsl:call-template>

      <!--UnrealizedMarginFutures-->
      <xsl:if test="$vIsDisplayFutUMG">
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='UnrealisedMargin']/@resourceDetailFutures"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/future"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/future"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--UnrealizedMarginOptions-->
      <xsl:if test="$vIsDisplayOptUMG">
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='UnrealisedMargin']/@resourceDetailOptions"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/option[@FutValMeth='FUT']"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/option[@FutValMeth='FUT']"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--UnrealizedMarginOthers-->
      <xsl:if test="$vIsDisplayOthersUMG">
        <xsl:call-template name ="Synthesis_SummaryRowAmountDetailAssetCategory" >
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pDataName" select="'UnrealisedMargin'"/>
          <xsl:with-param name="pCashBalanceStreamsDetail" select="$pCashBalanceStreams/unrealizedMargin/detail[@assetCategory != 'ExchangeTradedContract'] "/>
          <xsl:with-param name="pExchangeCashBalanceStreamDetail" select="$pExchangeCashBalanceStream/unrealizedMargin/detail[@assetCategory != 'ExchangeTradedContract']"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>

    <!--NetOptionValueDetail Detail-->
    <xsl:if test="$vIsDisplaySummaryCashFlows and 
            ($vIsDisplayLongNOV or $vIsDisplayShortNOV)">
      <!--NetOptionValueDetail : title-->
      <xsl:call-template name="Synthesis_SummaryRowTitle">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pTitleName" select="'NetOptionValue'"/>
      </xsl:call-template>

      <!--LongOptionValue-->
      <xsl:if test="$vIsDisplayLongNOV">
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'LongOptionValue'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue/longOptionValue"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue/longOptionValue"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--ShortOptionValue-->
      <xsl:if test="$vIsDisplayShortNOV">
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'ShortOptionValue'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue/shortOptionValue"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue/shortOptionValue"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>

  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_AccountSummaryBody_UKMethod            -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Account Summary Content (Method UK)     -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Add -->
  <!-- FI 20150928 [XXXXX] Modfy -->
  <!-- RD 20160406 [21284] Modify -->
  <xsl:template name="Synthesis_AccountSummaryBody_UKMethod">
    <!-- BizGroup -->
    <xsl:param name="pBizGroup"/>
    <!-- Représente les streams du CashBalance dont les devises sont présente dans pBizGroup -->
    <xsl:param name="pCashBalanceStreams"/>
    <!-- Représente le exchangeStream du CashBalance  -->
    <xsl:param name="pExchangeCashBalanceStream"/>

    <xsl:variable name="vAllCashBalanceStreams" select="$gCBTrade/cashBalanceReport/cashBalanceStream"/>

    <!--PreviousCashBalance-->
    <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="'PreviousCashBalance'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/previousCashBalance"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/previousCashBalance"/>
      <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      <xsl:with-param name="pMode" select="'master'"/>
    </xsl:call-template>

    <!--RealisedProfitLoss-->
    <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
    <xsl:choose>
      <xsl:when test="$gIsETDActivity=true()">
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'RealisedProfitLoss'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/future
                          | $pCashBalanceStreams/realizedMargin/option[@FutValMeth='FUT']
                          | $pCashBalanceStreams/realizedMargin/other
                          | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option
                          | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/future
                          | $pExchangeCashBalanceStream/realizedMargin/option[@FutValMeth='FUT']
                          | $pExchangeCashBalanceStream/realizedMargin/other
                          | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option
                          | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'RealisedProfitLoss'"/>
          <!--cashSettlement alimenté à partir des GAM pour les famille EST, DST et à partir des INT (coupon) sur les DST -->
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/globalAmount
                          | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/globalAmount
                          | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
    <!--Premium: il existe au moins un montant de prime dans le flux XML, donc au moins un trade Option sur un DC Premium Style-->
    <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
    <xsl:if test="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/premium/detail">
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'Premium'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/premium"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/premium"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      </xsl:call-template>
    </xsl:if>
    <!--Fee-->
    <!-- RD 20160406 [21284] 
        - Add safekeeping fee into parametr pExchangeAmount
        - Add parameter pIsAmountMandatory = true() 
    -->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="'Fee'"/>
      <xsl:with-param name="pAmount" select="$pBizGroup/fee/amount | $pBizGroup/fee/tax/amount |
                      $pBizGroup/safekeeping/amount | $pBizGroup/safekeeping/tax/amount"/>
      <xsl:with-param name="pExchangeAmount" select="$pBizGroup/exchangeCurrency/fee/amount | $pBizGroup/exchangeCurrency/fee/tax/amount |
                      $pBizGroup/exchangeCurrency/safekeeping/amount | $pBizGroup/exchangeCurrency/safekeeping/tax/amount"/>
      <xsl:with-param name="pIsAmountMandatory" select="true()"/>
    </xsl:call-template>
    <!--Funding: il existe au moins un montant de funding dans le flux XML, donc au moins un CFD en position-->
    <!-- FI 20150928 [XXXXX] test sur l'element funding  -->
    <!--<xsl:if test="$vAllCashBalanceStreams/funding[@amt !=0]">-->
    <xsl:if test="$vAllCashBalanceStreams/funding">
      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'Funding'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/funding"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/funding"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      </xsl:call-template>
    </xsl:if>
    <!--Borrowing: il existe au moins un montant de borrowing dans le flux XML, donc au moins un CFD en position-->
    <!-- FI 20150928 [XXXXX] il n'existe pas jamais de detail sur l'element borrowing (test existence de l'élément borrowing)  -->
    <!--<xsl:if test="$vAllCashBalanceStreams/borrowing/detail">-->
    <xsl:if test="$vAllCashBalanceStreams/borrowing">
      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'Borrowing'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/borrowing"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/borrowing"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      </xsl:call-template>
    </xsl:if>
    <!--CashBalancePayment-->
    <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="'CashBalancePayment'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashBalancePayment"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashBalancePayment"/>
      <xsl:with-param name="pIsAmountMandatory" select="true()"/>
    </xsl:call-template>

    <!--RD 20170929 [23403] Corporate Actions: Equalisation payment-->
    <!-- Equalisation payment-->
    <xsl:if test="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/equalisationPayment/detail">
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'EqualisationPayment'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/equalisationPayment"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/equalisationPayment"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      </xsl:call-template>
    </xsl:if>

    <!--CashBalance-->
    <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="'CashBalance'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashBalance"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashBalance"/>
      <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      <xsl:with-param name="pMode" select="'subtotal'"/>
    </xsl:call-template>
    <!--Empty Row-->
    <xsl:call-template name="Synthesis_SummaryRowEmpty">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--OpenTradeEqty-->
    <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
    <xsl:choose>
      <xsl:when test="$gIsETDActivity=true()">
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'OpenTradeEqty'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/future
                          | $pCashBalanceStreams/unrealizedMargin/option[@FutValMeth='FUT']
                          | $pCashBalanceStreams/unrealizedMargin/other"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/future
                          | $pExchangeCashBalanceStream/unrealizedMargin/option[@FutValMeth='FUT']
                          | $pExchangeCashBalanceStream/unrealizedMargin/other"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$gIsCOMActivity=true()"/>
      <xsl:otherwise>
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'OpenTradeEqty'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/globalAmount"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/globalAmount"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
    <!--UnsettledTransactions: Il existe au moins un trade equitySecurityTransaction ou debtSecurityTransaction-->
    <xsl:if test="$gIsSecurityActivity or $gIsCOMActivity">
      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'UnsettledTransactions'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unsettledCash"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unsettledCash"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      </xsl:call-template>
    </xsl:if>
    <!--TotalEqty-->
    <xsl:variable name="vTotalEqty_AmountName">
      <xsl:choose>
        <xsl:when test="$gIsSecurityActivity">
          <xsl:value-of select="'TotalEqtyWithUT'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'TotalEqty'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pAmountName" select="$vTotalEqty_AmountName"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/equityBalance"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/equityBalance"/>
      <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      <xsl:with-param name="pMode" select="'master'"/>
    </xsl:call-template>

    <!--NetOptionValue: Il existe au moins un trade Option (ETD,FX,…)-->
    <xsl:if test="$gIsOptionActivity">
      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'NetOptionValue'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      </xsl:call-template>
    </xsl:if>

    <!--Open Position Value: Il existe au moins un trade debtSecurityTransaction ou equitySecurityTransaction-->
    <xsl:if test="$gIsSecurityActivity">
      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'OpenPositionValue'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marketValue"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marketValue"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      </xsl:call-template>
    </xsl:if>

    <!--TotalAccountValue: Il existe au moins un trade EquitySecurityTransaction ou equitySecurityTransaction et/ou un trade Option (ETD,FX,…)-->
    <xsl:if test="$gIsOptionActivity or $gIsSecurityActivity">
      <xsl:variable name="vTotalAccountValue_AmountName">
        <xsl:choose>
          <xsl:when test="$gIsOptionActivity and $gIsSecurityActivity">
            <xsl:value-of select="'TotalAccountValue'"/>
          </xsl:when>
          <xsl:when test="$gIsOptionActivity">
            <xsl:value-of select="'TotalAccountValueWithNOV'"/>
          </xsl:when>
          <xsl:when test="$gIsSecurityActivity">
            <xsl:value-of select="'TotalAccountValueWithOPV'"/>
          </xsl:when>
        </xsl:choose>
      </xsl:variable>
      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="$vTotalAccountValue_AmountName"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/totalAccountValue"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/totalAccountValue"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        <xsl:with-param name="pMode" select="'subtotal'"/>
      </xsl:call-template>
    </xsl:if>
    <!--Empty Row-->
    <xsl:call-template name="Synthesis_SummaryRowEmpty">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>

    <!-- RD 20200214 [25146] Dans le cas où on n'arrive pas à définir l'activité (gIsMarginingActivity/gIsCOMActivity), affichage du InitialMargin s'il existe -->
    <xsl:variable name="isMarginRequirement" select="$vAllCashBalanceStreams/marginRequirement/detail"/>

    <!--InitialMarginReq / SecuritiesOnDeposit / MarginDeficitExcess: Il existe une activité sur au moins un instruments marginé (ETD,CFD,…)-->
    <xsl:if test="$gIsMarginingActivity or $isMarginRequirement">
      <!--InitialMarginReq-->
      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="'InitialMarginReq_CSBUK'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marginRequirement"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marginRequirement"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
      </xsl:call-template>
      <!--SecuritiesOnDeposit-->
      <xsl:if test="$vAllCashBalanceStreams/collateralAvailable[@amt > 0]">
        <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'SecuritiesOnDeposit'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/collateralAvailable"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/collateralAvailable"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--MarginDeficitExcess-->
      <xsl:variable name="vMarginDeficitExcess_AmountName">
        <xsl:choose>
          <xsl:when test="$pCashBalanceStreams/collateralAvailable[@amt > 0]">
            <xsl:value-of select="'MarginDeficitExcess'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'MarginDeficitExcessNoSOD'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="$vMarginDeficitExcess_AmountName"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/excessDeficit"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/excessDeficit"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        <xsl:with-param name="pMode" select="'subtotal'"/>
      </xsl:call-template>
    </xsl:if>

    <!-- FI 20150716 [20892] Add safekeeping-->
    <xsl:variable name="vIsDisplayFees" select="(($pBizGroup/fee/amount[@amt !=0] or $pBizGroup/exchangeCurrency/fee/amount[@amt !=0]) = true()) or 
                                                (($pBizGroup/safekeeping/amount[@amt !=0] or $pBizGroup/exchangeCurrency/safekeeping/amount[@amt !=0]) = true())"/>


    <xsl:variable name="vIsDisplayFutPnL" select="$vAllCashBalanceStreams/realizedMargin/future[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOptPnL" select="$vAllCashBalanceStreams/realizedMargin/option[@amt!=0 and @FutValMeth='FUT'] = true()"/>
    <xsl:variable name="vIsDisplayOptCshSettl" select="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option[@amt!=0] = true()"/>

    <xsl:variable name="vIsDisplayFutOTE" select="$vAllCashBalanceStreams/unrealizedMargin/future[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOptOTE" select="$vAllCashBalanceStreams/unrealizedMargin/option[@amt!=0 and @FutValMeth='FUT'] = true()"/>
    <xsl:variable name="vIsDisplayOthersOTE" select="$vAllCashBalanceStreams/unrealizedMargin/other[@amt!=0] = true()"/>

    <!--Il existe des trades ETD et des trades OTC-->
    <xsl:variable name="vIsDisplayOthersPnL" select="$vAllCashBalanceStreams/realizedMargin/other[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOthersCshSettl" select="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other[@amt!=0] = true()"/>

    <!--Il existe des trades option ETD ou bien des FxOption en Position-->
    <xsl:variable name="vIsDisplayDetailNOV" select="$gTrade[@tradeId=$gPosTrades/trade/@tradeId and (exchangeTradedDerivative/Category/text()='O' or fxSimpleOption)] = true()"/>
    <xsl:variable name="vIsDisplayDetailOPV" select="$gIsSecurityActivity and $pCashBalanceStreams/marketValue[@amt!=0]/detail = true()"/>
    <xsl:variable name="vIsDisplayDetailUT" select="$gIsSecurityActivity and $pCashBalanceStreams/unsettledCash/dateDetail[@amt!=0]/detail = true()"/>

    <xsl:variable name="vIsDisplaySummaryCashFlows"
                  select="(($gSettings-headerFooter=false()) or $gSettings-headerFooter/summaryCashFlows/text() != 'None') and
                  ($vIsDisplayFutPnL or $vIsDisplayOptPnL or $vIsDisplayOthersPnL or 
                  $vIsDisplayOptCshSettl or $vIsDisplayOthersCshSettl or 
                  $vIsDisplayFutOTE or $vIsDisplayOptOTE or $vIsDisplayOthersOTE or 
                  $gIsOptionActivity or $vIsDisplayDetailOPV or $vIsDisplayDetailUT)"/>
    <xsl:variable name="vIsDisplaySummaryFees"
                  select="(($gSettings-headerFooter=false()) or $gSettings-headerFooter/summaryFees/text() != 'None') and $vIsDisplayFees"/>

    <xsl:if test="$vIsDisplaySummaryCashFlows or $vIsDisplaySummaryFees">

      <!--Detail separation-->
      <xsl:call-template name ="Synthesis_SummaryBeforeDetail">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>

      <!--RealisedProfitLoss Detail-->
      <xsl:if test="$vIsDisplaySummaryCashFlows and 
            ($vIsDisplayFutPnL or $vIsDisplayOptPnL or $vIsDisplayOthersPnL or 
            $vIsDisplayOptCshSettl or $vIsDisplayOthersCshSettl)">
        <!--RealisedProfitLossDetail : title-->
        <xsl:call-template name="Synthesis_SummaryRowTitle">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pTitleName" select="'RealisedProfitLoss'"/>
        </xsl:call-template>
        <!--RealisedProfitLossFutures-->
        <xsl:if test="$vIsDisplayFutPnL">
          <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='RealisedProfitLoss']/@resourceDetailFutures"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/future"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/future"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
            <xsl:with-param name="pIsDetail" select="true()"/>
          </xsl:call-template>
        </xsl:if>
        <!--RealisedProfitLossOptions-->
        <xsl:if test="$vIsDisplayOptPnL">
          <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='RealisedProfitLoss']/@resourceDetailOptions"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/option[@FutValMeth='FUT']"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/option[@FutValMeth='FUT']"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
            <xsl:with-param name="pIsDetail" select="true()"/>
          </xsl:call-template>
        </xsl:if>
        <!--RealisedProfitLossOthers-->
        <xsl:if test="$vIsDisplayOthersPnL">
          <!-- FI 20160422 [XXXXX] Call Synthesis_SummaryRowAmountDetailAssetCategory (détail par asset category sur les CFD) -->
          <xsl:call-template name ="Synthesis_SummaryRowAmountDetailAssetCategory" >
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pDataName" select="'RealisedProfitLoss'"/>
            <xsl:with-param name="pCashBalanceStreamsDetail" select="$pCashBalanceStreams/realizedMargin/detail[@assetCategory != 'ExchangeTradedContract'] "/>
            <xsl:with-param name="pExchangeCashBalanceStreamDetail" select="$pCashBalanceStreams/realizedMargin/detail[@assetCategory != 'ExchangeTradedContract']"/>
          </xsl:call-template>
        </xsl:if>
        <!--CashSettlementOptions-->
        <xsl:if test="$vIsDisplayOptCshSettl">
          <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="'CashSettlementOptions'"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
            <xsl:with-param name="pIsDetail" select="true()"/>
          </xsl:call-template>
        </xsl:if>
        <!--CashSettlementOthers-->
        <xsl:if test="$vIsDisplayOthersCshSettl">
          <!-- FI 20151019 [21317] Call Synthesis_SummaryRowAmountDetailAssetCategory -->
          <xsl:call-template name ="Synthesis_SummaryRowAmountDetailAssetCategory" >
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pDataName" select="'CashSettlementOptions'"/>
            <xsl:with-param name="pCashBalanceStreamsDetail" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail[@assetCategory != 'ExchangeTradedContract'] "/>
            <xsl:with-param name="pExchangeCashBalanceStreamDetail" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail[@assetCategory != 'ExchangeTradedContract']"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:if>
      <!--Fee Detail-->
      <xsl:if test="$vIsDisplaySummaryFees">
        <xsl:call-template name="Synthesis_SummaryFeeDetail">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        </xsl:call-template>
      </xsl:if>
      <!--OpenTradeEqty Detail-->
      <xsl:if test="$vIsDisplaySummaryCashFlows and 
            ($vIsDisplayFutOTE or $vIsDisplayOptOTE or $vIsDisplayOthersOTE)">
        <!--OpenTradeEqtyDetail  : title-->
        <xsl:call-template name="Synthesis_SummaryRowTitle">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pTitleName" select="'OpenTradeEqty'"/>
        </xsl:call-template>
        <!--OpenTradeEqtyFutures-->
        <xsl:if test="$vIsDisplayFutOTE">
          <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='OpenTradeEqty']/@resourceDetailFutures"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/future"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/future"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
            <xsl:with-param name="pIsDetail" select="true()"/>
          </xsl:call-template>
        </xsl:if>
        <!--OpenTradeEqtyOptions-->
        <xsl:if test="$vIsDisplayOptOTE">
          <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
          <xsl:call-template name="Synthesis_SummaryRowAmount">
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pAmountName" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='OpenTradeEqty']/@resourceDetailOptions"/>
            <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/option[@FutValMeth='FUT']"/>
            <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/option[@FutValMeth='FUT']"/>
            <xsl:with-param name="pIsAmountMandatory" select="true()"/>
            <xsl:with-param name="pIsDetail" select="true()"/>
          </xsl:call-template>
        </xsl:if>

        <!--OpenTradeEqtyOthers-->
        <xsl:if test="$vIsDisplayOthersOTE">
          <!-- FI 20151019 [21317] Call Synthesis_SummaryRowAmountDetailAssetCategory -->
          <xsl:call-template name ="Synthesis_SummaryRowAmountDetailAssetCategory" >
            <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
            <xsl:with-param name="pDataName" select="'OpenTradeEqty'"/>
            <xsl:with-param name="pCashBalanceStreamsDetail" select="$pCashBalanceStreams/unrealizedMargin/detail[@assetCategory != 'ExchangeTradedContract'] "/>
            <xsl:with-param name="pExchangeCashBalanceStreamDetail" select="$pExchangeCashBalanceStream/unrealizedMargin/detail[@assetCategory != 'ExchangeTradedContract']"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:if>

      <!--UnsettledTransactions Detail: Il existe des trades sur asset equity ou debtSecurity -->
      <xsl:if test="$vIsDisplaySummaryCashFlows and $vIsDisplayDetailUT">
        <!--UnsettledTransactions  : title-->
        <xsl:call-template name="Synthesis_SummaryRowTitle">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pTitleName" select="'UnsettledTransactions'"/>
        </xsl:call-template>
        <!-- FI 20151019 [21317] Call Synthesis_SummaryRowAmountDateDetailAssetCategory -->
        <xsl:call-template name ="Synthesis_SummaryRowAmountDateDetailAssetCategory" >
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pDataName" select="'UnsettledTransactions'"/>
          <xsl:with-param name="pCashBalanceStreamsDateDetail" select="$pCashBalanceStreams/unsettledCash/dateDetail"/>
          <xsl:with-param name="pExchangeCashBalanceStreamDateDetail" select="$pExchangeCashBalanceStream/unsettledCash/dateDetail"/>
        </xsl:call-template>
      </xsl:if>

      <!--NetOptionValueDetail Detail-->
      <xsl:if test="$vIsDisplaySummaryCashFlows and $vIsDisplayDetailNOV">
        <!--NetOptionValueDetail : title-->
        <xsl:call-template name="Synthesis_SummaryRowTitle">
          <xsl:with-param name="pAmountName" select="'NetOptionValue'"/>
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        </xsl:call-template>
        <!--LongOptionValue-->
        <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'LongOptionValue'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue/longOptionValue"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue/longOptionValue"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
        <!--ShortOptionValue-->
        <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="'ShortOptionValue'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue/shortOptionValue"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue/shortOptionValue"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>

      <!--Open Position Value Detail: Il existe des trades Equity or DebtSecurity -->
      <!-- FI 20151019 [21317] Modify -->
      <xsl:if test="$vIsDisplaySummaryCashFlows and $vIsDisplayDetailOPV">
        <!--OpenPositionValue  : title-->
        <xsl:call-template name="Synthesis_SummaryRowTitle">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pTitleName" select="'OpenPositionValue'"/>
        </xsl:call-template>

        <!-- FI 20151019 [21317] Call template Synthesis_SummaryRowAmountAssetCategory -->
        <!-- RD 20160406 [21284] Use template Synthesis_SummaryRowAmount instead Synthesis_SummaryRowAmountDetailAssetCategory 
              because the amount OpenPositionValue is not detailed by Assetcategory in XML flow. -->
        <!--<xsl:call-template name ="Synthesis_SummaryRowAmountDetailAssetCategory">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pDataName" select="'OpenPositionValue'"/>
          <xsl:with-param name="pCashBalanceStreamsDetail" select="$pCashBalanceStreams/marketValue/detail"/>
          <xsl:with-param name="pExchangeCashBalanceStreamDetail" select="$pExchangeCashBalanceStream/marketValue/detail"/>
        </xsl:call-template>-->

        <xsl:variable name ="vResourceDetail">
          <xsl:choose>
            <xsl:when test ="$gIsDebtSecurityActivity and $gIsEquityActivity">
              <xsl:value-of select="concat($gBlockSettings_Summary/column[@name='Designation2']/data[@name='OpenPositionValue']/@resourceDetailStocks,
                              ' / ',
                              $gBlockSettings_Summary/column[@name='Designation2']/data[@name='OpenPositionValue']/@resourceDetailDebtSecurities)"/>
            </xsl:when>
            <xsl:when test ="$gIsEquityActivity">
              <xsl:value-of select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='OpenPositionValue']/@resourceDetailStocks"/>
            </xsl:when>
            <xsl:when test ="$gIsDebtSecurityActivity">
              <xsl:value-of select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='OpenPositionValue']/@resourceDetailDebtSecurities"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name='OpenPositionValue']/@resourceDetailOthers"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pAmountName" select="$vResourceDetail"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marketValue"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marketValue"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>


  <!-- ................................................................ -->
  <!-- Synthesis_SummaryBeforeDetail                                    -->
  <!-- ................................................................ -->
  <!-- Summary :                                                        -->
  <!-- Build rows separation before the Informations of details         -->
  <!-- ................................................................ -->
  <!-- FI 20150921 [21311] Add -->
  <xsl:template name ="Synthesis_SummaryBeforeDetail">
    <xsl:param name="pBizGroup"/>
    <!--Empty Row-->
    <xsl:call-template name="Synthesis_SummaryRowEmpty">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pWithBorder" select="false()"/>
    </xsl:call-template>
    <!--Display underline-->
    <xsl:call-template name="Synthesis_SummaryRowLine">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--Empty Row-->
    <xsl:call-template name="Synthesis_SummaryRowEmpty">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pWithBorder" select="false()"/>
    </xsl:call-template>
  </xsl:template>


  <!-- ................................................................ -->
  <!-- Synthesis_SummaryRowAmountAssetCategory                          -->
  <!-- ................................................................ -->
  <!-- Summary :                                                        -->
  <!--       Affiche une ligne détail par asset category                -->
  <!-- ................................................................ -->
  <!-- FI 20151019 [21317] Add Template -->
  <xsl:template name ="Synthesis_SummaryRowAmountDetailAssetCategory">
    <!-- BizGroup -->
    <xsl:param name="pBizGroup"/>
    <!-- Type de détail ('UnsettledTransactions', 'OpenPositionValue') -->
    <xsl:param name="pDataName"/>
    <!-- Représente le détail par asset sur l'ensemble des streams-->
    <xsl:param name="pCashBalanceStreamsDetail"/>
    <!-- Représente le détail par asset sur l'exchange stream-->
    <xsl:param name="pExchangeCashBalanceStreamDetail"/>
    <!-- Représente la date lorsque l'ensemble des montants s'applique à une date donnée (ex UnsettledTransactions)  -->
    <xsl:param name="pDate"/>

    <xsl:variable name="vValDt">
      <xsl:if test ="$pDate">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$pDate" />
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <!-- Mise en place d'un noeud pour application pour obtenir la liste des categories -->
    <xsl:variable name="vCashBalanceStreamsDetailCopyNode">
      <xsl:copy-of select="$pCashBalanceStreamsDetail"/>
    </xsl:variable>
    <xsl:variable name="vCashBalanceStreamsDetailCopy" select="msxsl:node-set($vCashBalanceStreamsDetailCopyNode)/detail"/>
    <xsl:variable name="vAssetCategory_List" select="$vCashBalanceStreamsDetailCopy[generate-id()=generate-id(key('kAssetCategory',@assetCategory)[1])]"/>

    <xsl:for-each select="$vAssetCategory_List">
      <xsl:sort select ="$vAssetCategory_List/@assetCategory"/>
      <xsl:variable name="vAssetCategory" select="current()/@assetCategory"/>

      <xsl:variable name ="vResourceDetail">
        <xsl:choose>
          <xsl:when test ="$vAssetCategory = 'Bond'">
            <xsl:value-of select="'resourceDetailDebtSecurities'"/>
          </xsl:when>
          <xsl:when test ="$vAssetCategory = 'EquityAsset'">
            <xsl:value-of select="'resourceDetailStocks'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'resourceDetailOthers'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name ="vAmountName">
        <xsl:choose>
          <xsl:when test ="$pDataName = 'UnsettledTransactions'">
            <xsl:value-of select ="concat($gBlockSettings_Summary/column[@name='Designation2']/data[@name=$pDataName]/@*[name()=$vResourceDetail], ' - Settling ', $vValDt)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="$gBlockSettings_Summary/column[@name='Designation2']/data[@name=$pDataName]/@*[name()=$vResourceDetail]"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="$vAmountName"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreamsDetail[@assetCategory=$vAssetCategory]"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStreamDetail[@assetCategory=$vAssetCategory]"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        <xsl:with-param name="pIsDetail" select="true()"/>
      </xsl:call-template>
    </xsl:for-each>


  </xsl:template>


  <!-- ................................................................ -->
  <!-- Synthesis_SummaryRowAmountDateDetailAssetCategory                -->
  <!-- ................................................................ -->
  <!-- Summary :                                                        -->
  <!--       Affiche une ligne couple (Date/assetCategory)              -->
  <!-- ................................................................ -->
  <!-- FI 20151019 [21317] Add Template -->
  <xsl:template name ="Synthesis_SummaryRowAmountDateDetailAssetCategory">
    <!-- BizGroup -->
    <xsl:param name="pBizGroup"/>
    <!-- Type de détail ('UnsettledTransactions', 'OpenPositionValue') -->
    <xsl:param name="pDataName"/>
    <!-- Représente le détail par asset sur l'ensemble des streams-->
    <xsl:param name="pCashBalanceStreamsDateDetail"/>
    <!-- Représente le détail par asset sur l'exchange stream-->
    <xsl:param name="pExchangeCashBalanceStreamDateDetail"/>

    <!-- Mise en place d'un noeud pour application pour obtenir la liste des categories -->
    <xsl:variable name="vCashBalanceStreamsDateDetailCopyNode">
      <xsl:copy-of select="$pCashBalanceStreamsDateDetail"/>
    </xsl:variable>
    <xsl:variable name="vCashBalanceStreamsDateDetailCopy" select="msxsl:node-set($vCashBalanceStreamsDateDetailCopyNode)/dateDetail"/>
    <xsl:variable name="vDate_List" select="$vCashBalanceStreamsDateDetailCopy[generate-id()=generate-id(key('kValDt',@valDt)[1])]"/>

    <xsl:for-each select="$vDate_List">
      <xsl:sort select ="$vDate_List/@valDt"/>
      <xsl:variable name="vValDt" select="current()/@valDt"/>

      <xsl:call-template name ="Synthesis_SummaryRowAmountDetailAssetCategory">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pDataName" select="$pDataName"/>
        <xsl:with-param name="pCashBalanceStreamsDetail" select="$pCashBalanceStreamsDateDetail[@valDt=$vValDt]/detail"/>
        <xsl:with-param name="pExchangeCashBalanceStreamDetail" select="$pExchangeCashBalanceStreamDateDetail[@valDt=$vValDt]/detail"/>
        <xsl:with-param name="pDate" select ="$vValDt"/>
      </xsl:call-template>-->
    </xsl:for-each>
  </xsl:template>


  <!-- ................................................ -->
  <!-- Synthesis_SummaryMarginCallDetail                -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Summary Margin call details             -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Add Template -->
  <xsl:template name="Synthesis_SummaryMarginCallDetail">
    <!-- -->
    <xsl:param name="pBizGroup"/>
    <!-- Représente les streams du CashBalance dont les devises sont présente dans pBizGroup -->
    <xsl:param name="pCashBalanceStreams"/>
    <!-- Représente le exchangeStream du CashBalance  -->
    <xsl:param name="pExchangeCashBalanceStream"/>

    <!-- MarginCallDetail : title-->
    <xsl:call-template name="Synthesis_SummaryRowTitle">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pTitleName" select="'MarginCall_CSBDEFAULT'"/>
    </xsl:call-template>

    <!-- Previous -->
    <xsl:call-template name ="Synthesis_SummaryMarginCallDetail_Sheet">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pCashBalanceStreams" select="$pCashBalanceStreams"/>
      <xsl:with-param name="pExchangeCashBalanceStream" select="$pExchangeCashBalanceStream"/>
      <xsl:with-param name="pMode" select="'previous'"/>
    </xsl:call-template>

    <!-- Current -->
    <xsl:call-template name ="Synthesis_SummaryMarginCallDetail_Sheet">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pCashBalanceStreams" select="$pCashBalanceStreams"/>
      <xsl:with-param name="pExchangeCashBalanceStream" select="$pExchangeCashBalanceStream"/>
      <xsl:with-param name="pMode" select="'current'"/>
    </xsl:call-template>

    <!-- MarginCall -->
    <xsl:call-template name="Synthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'MarginCallDetail_CSBDEFAULT'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marginCall"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marginCall"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pMode" select="'bold'"/>
      <xsl:with-param name="pIsDetail" select="true()"/>
    </xsl:call-template>
  </xsl:template>


  <!-- ................................................ -->
  <!-- Synthesis_SummaryMarginCallDetail_Sheet          -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!-- Margin call details ('previous' ou 'current')    -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Add Template -->
  <xsl:template name="Synthesis_SummaryMarginCallDetail_Sheet">
    <xsl:param name="pBizGroup"/>
    <!-- Valeurs possible : 'previous' or 'current'  -->
    <xsl:param name="pMode"/>

    <xsl:variable name ="vDefaultBackground-color">
      <xsl:choose>
        <xsl:when test ="$pMode='previous'">
          <xsl:value-of select ="$gBlockSettings_Summary/@background-color2"/>
        </xsl:when>
        <xsl:when test ="$pMode='current'">
          <xsl:value-of select ="$gBlockSettings_Summary/@background-color"/>
        </xsl:when>
        <xsl:otherwise>default</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>



    <!-- row 1: Margin Requirement (Encours Deposit)  -->
    <fo:table-row font-size="{$gData_font-size}" text-align="{$gData_text-align}"
                  display-align="{$gData_display-align}" keep-with-previous="always" font-weight="{$gData_font-weight}" >

      <!--Designation-->
      <fo:table-cell number-rows-spanned="6" font-size="5pt" dispaly-align="center" >
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:variable name ="vResArr">
          <xsl:choose>
            <xsl:when test ="$pMode='previous'">
              <xsl:choose>
                <xsl:when test = "$pCurrentCulture = 'fr-FR'">
                  <xsl:value-of select ="'Arr.Préc'"/>
                </xsl:when>
                <xsl:when test = "$pCurrentCulture = 'it-IT'">
                  <xsl:value-of select ="'Chiu.Prec'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select ="'Clos.prev'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test ="$pMode='current'">
              <xsl:choose>
                <xsl:when test = "$pCurrentCulture = 'fr-FR'">
                  <xsl:value-of select ="'Arr.Jour'"/>
                </xsl:when>
                <xsl:when test = "$pCurrentCulture = 'it-IT'">
                  <xsl:value-of select ="'Chiu.Gior'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select ="'Clos.Day'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>

        <xsl:call-template name="Vertical">
          <xsl:with-param name="pVal">
            <xsl:value-of select="$vResArr"/>
          </xsl:with-param>
          <xsl:with-param name="pLen">
            <xsl:value-of select="string-length($vResArr)"/>
          </xsl:with-param>
        </xsl:call-template>
      </fo:table-cell>

      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                     text-align="{$gBlockSettings_Summary/column[@name='Designation2']/@text-align}"
                     number-columns-spanned="3">
        <xsl:call-template name="Debug_border-green"/>
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>
        <fo:block>
          <xsl:call-template name ="Synthesis_GetAmountLabel" >
            <xsl:with-param  name="pAmountName" select  ="'InitialMarginReq_CSBDEFAULT'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>
        <xsl:variable name="vCurrentCcy" select="current()/@ccy"/>
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/currency[@ccy=$vCurrentCcy]/marginCallDet/*[name()=$pMode]"/>

        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/marginRequirement" />
          <xsl:with-param name="pCcy" select="$vCurrentCcy" />
          <xsl:with-param name="pAmountPattern" select="current()/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:for-each>

      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/exchangeCurrency/marginCallDet/*[name()=$pMode]"/>

        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/marginRequirement" />
          <xsl:with-param name="pCcy" select="current()/@ccy" />
          <xsl:with-param name="pAmountPattern" select="$pBizGroup/exchangeCurrency/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:if>

      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>

    <!-- row 2  collateral Available -->
    <fo:table-row font-size="{$gData_font-size}" text-align="{$gData_text-align}"
                  display-align="{$gData_display-align}" keep-with-previous="always" font-weight="{$gData_font-weight}" >

      <!--Designation-->
      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                     text-align="{$gBlockSettings_Summary/column[@name='Designation2']/@text-align}"
                     number-columns-spanned="2" number-rows-spanned="2" font-size="7pt" display-align="before">
        <xsl:call-template name="Debug_border-green"/>
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>
        <fo:block>
          <xsl:call-template name ="Synthesis_GetAmountLabel" >
            <xsl:with-param  name="pAmountName" select  ="'Collateral'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell  color="{$gBlockSettings_Summary/@minor-color}" font-size="7pt" text-align="left">
        <xsl:call-template name="Debug_border-green"/>
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>
        <fo:block>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Available'" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>
        <xsl:variable name="vCurrentCcy" select="current()/@ccy"/>
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/currency[@ccy=$vCurrentCcy]/marginCallDet/*[name()=$pMode]"/>
        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/collateralAvailable" />
          <xsl:with-param name="pCcy" select="$vCurrentCcy" />
          <xsl:with-param name="pAmountPattern" select="current()/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pMode" select ="'minor'"/>
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:for-each>

      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/exchangeCurrency/marginCallDet/*[name()=$pMode]"/>

        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/collateralAvailable" />
          <xsl:with-param name="pCcy" select="current()/@ccy" />
          <xsl:with-param name="pAmountPattern" select="$pBizGroup/exchangeCurrency/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pMode" select ="'minor'"/>
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:if>
      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>

    <!-- row 3  collateral used -->
    <fo:table-row font-size="{$gData_font-size}" text-align="{$gData_text-align}"
                  display-align="{$gData_display-align}" keep-with-previous="always" font-weight="{$gData_font-weight}">

      <!--Designation-->
      <fo:table-cell  font-size="7pt" text-align="left">
        <xsl:call-template name="Debug_border-green"/>
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>

        <fo:block>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Used'" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>
        <xsl:variable name="vCurrentCcy" select="current()/@ccy"/>
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/currency[@ccy=$vCurrentCcy]/marginCallDet/*[name()=$pMode]"/>

        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/collateralUsed" />
          <xsl:with-param name="pCcy" select="$vCurrentCcy" />
          <xsl:with-param name="pAmountPattern" select="current()/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:for-each>

      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/exchangeCurrency/marginCallDet/*[name()=$pMode]"/>

        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/collateralUsed" />
          <xsl:with-param name="pCcy" select="current()/@ccy" />
          <xsl:with-param name="pAmountPattern" select="$pBizGroup/exchangeCurrency/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:if>
      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>

    <!-- row 4: cash available  -->
    <fo:table-row font-size="{$gData_font-size}" text-align="{$gData_text-align}"
                  display-align="{$gData_display-align}" keep-with-previous="always" font-weight="{$gData_font-weight}">

      <!--Designation-->
      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                     text-align="{$gBlockSettings_Summary/column[@name='Designation2']/@text-align}"
                     number-columns-spanned="2" number-rows-spanned="2"
                     font-size="7pt" display-align="before">
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:call-template name="Debug_border-green"/>
        <fo:block>
          <xsl:call-template name ="Synthesis_GetAmountLabel" >
            <xsl:with-param  name="pAmountName" select  ="'Cash'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell  color="{$gBlockSettings_Summary/@minor-color}" font-size="7pt" text-align="left">
        <xsl:call-template name="Debug_border-green"/>
        <xsl:call-template name="Debug_border-green"/>
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>
        <fo:block>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Available'" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>
        <xsl:variable name="vCurrentCcy" select="current()/@ccy"/>
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/currency[@ccy=$vCurrentCcy]/marginCallDet/*[name()=$pMode]"/>

        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/cashAvailable" />
          <xsl:with-param name="pCcy" select="$vCurrentCcy" />
          <xsl:with-param name="pAmountPattern" select="current()/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pMode" select="'minor'" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:for-each>

      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/exchangeCurrency/marginCallDet/*[name()=$pMode]"/>
        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/cashAvailable" />
          <xsl:with-param name="pCcy" select="current()/@ccy" />
          <xsl:with-param name="pAmountPattern" select="$pBizGroup/exchangeCurrency/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pMode" select="'minor'" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:if>
      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>

    <!-- row 5: cash used   -->
    <fo:table-row font-size="{$gData_font-size}" text-align="{$gData_text-align}"
                  display-align="{$gData_display-align}" keep-with-previous="always" font-weight="{$gData_font-weight}">

      <!--Designation-->
      <fo:table-cell  font-size="7pt" text-align="left">
        <xsl:call-template name="Debug_border-green"/>
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>

        <fo:block>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Used'" />
          </xsl:call-template>
          <fo:inline font-size="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-size}"
                 font-weight="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-weight}">
            <xsl:choose>
              <xsl:when test ="$pMode='previous'">
                <xsl:value-of select="concat($gcSpace,'(S1)')"/>
              </xsl:when>
              <xsl:when test ="$pMode='current'">
                <xsl:value-of select="concat($gcSpace,'(S2)')"/>
              </xsl:when>
            </xsl:choose>
          </fo:inline>
        </fo:block>
      </fo:table-cell>


      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>
        <xsl:variable name="vCurrentCcy" select="current()/@ccy"/>
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/currency[@ccy=$vCurrentCcy]/marginCallDet/*[name()=$pMode]"/>

        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/cashUsed" />
          <xsl:with-param name="pCcy" select="$vCurrentCcy" />
          <xsl:with-param name="pAmountPattern" select="current()/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:for-each>

      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/exchangeCurrency/marginCallDet/*[name()=$pMode]"/>
        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/cashUsed" />
          <xsl:with-param name="pCcy" select="current()/@ccy" />
          <xsl:with-param name="pAmountPattern" select="$pBizGroup/exchangeCurrency/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:if>
      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>

    <!-- row 6: uncoveredMarginRequirement -->
    <fo:table-row font-size="{$gData_font-size}" text-align="{$gData_text-align}"
                  display-align="{$gData_display-align}" keep-with-previous="always">
      <xsl:attribute name="font-weight">
        <xsl:value-of select="$gData_Master_font-weight"/>
      </xsl:attribute>

      <!--Designation-->
      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                     text-align="{$gBlockSettings_Summary/column[@name='Designation2']/@text-align}" number-columns-spanned="3">
        <xsl:call-template name="Debug_border-green"/>
        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>

        <fo:block>
          <xsl:call-template name ="Synthesis_GetAmountLabel" >
            <xsl:with-param  name="pAmountName" select  ="'UncoveredMargin'"/>
          </xsl:call-template>
          <fo:inline font-size="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-size}"
                 font-weight="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-weight}">
            <xsl:choose>
              <xsl:when test ="$pMode='previous'">
                <xsl:value-of select="concat($gcSpace,'(D1)')"/>
              </xsl:when>
              <xsl:when test ="$pMode='current'">
                <xsl:value-of select="concat($gcSpace,'(D2)')"/>
              </xsl:when>
            </xsl:choose>
          </fo:inline>
        </fo:block>
      </fo:table-cell>

      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>
        <xsl:variable name="vCurrentCcy" select="current()/@ccy"/>
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/currency[@ccy=$vCurrentCcy]/marginCallDet/*[name()=$pMode]"/>

        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/uncoveredMarginRequirement" />
          <xsl:with-param name="pCcy" select="$vCurrentCcy" />
          <xsl:with-param name="pAmountPattern" select="current()/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:for-each>

      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <xsl:variable name ="vMarginCallDetRoot" select ="$pBizGroup/exchangeCurrency/marginCallDet/*[name()=$pMode]"/>
        <xsl:call-template name="Synthesis_SummaryAmount">
          <xsl:with-param name="pAmount" select="$vMarginCallDetRoot/uncoveredMarginRequirement" />
          <xsl:with-param name="pCcy" select="current()/@ccy" />
          <xsl:with-param name="pAmountPattern" select="$pBizGroup/exchangeCurrency/@pattern" />
          <xsl:with-param name="pWithSide" select="false()" />
          <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
        </xsl:call-template>
      </xsl:if>
      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>
  </xsl:template>
</xsl:stylesheet>
