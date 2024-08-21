<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:fn="http://www.w3.org/2005/xpath-functions"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres reports - ETD Order Confirmation                                             -->
  <!-- File    : \Report_v2\OTCSynthesisReport.xsl                                                    -->
  <!-- ============================================================================================== -->
  <!-- Version : v8.1.7101                                                                            -->
  <!-- Date    : 20190611                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Settings                                          -->
  <!-- ============================================================================================== -->
  <!--Business-->
  <xsl:import href="Shared\Shared_Report_v2_Business.xslt" />
  <xsl:import href="LSD\ETD_Report_v2_Business.xslt" />

  <!--Report Format-->
  <xsl:import href="Shared\Shared_Report_v2_A4Vertical.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_UK.xslt" />

  <!--Tools-->
  <xsl:import href="Shared\Shared_Report_v2_Tools.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_FixmlTools.xslt" />
  <xsl:import href="LSD\ETD_Report_v2_Tools.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_Variables.xslt" />

  <!--Parameters-->
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>
  <!--<xsl:param name="pPathResource" select="'..\..\..\..\Resource'" />-->
  <xsl:param name="pCurrentCulture" select="'en-GB'" />
  <xsl:param name="pDebugMode" select="'None'"/>
  <!-- FI 20150921 [21311] => Paramètre qui permet de réduire l'affichage à certaines sections  
  Exemple:TRD,ACC => Ds ce cas affichage uniquement des trades jour et de la situation financière   -->
  <xsl:param name="pBizSection" select="concat($gcReportSectionKey_ORD,',',$gcReportSectionKey_TRD)"/>

  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gReportType" select="$gcReportTypeORDERALLOC"/>
  <xsl:variable name="gMainBook" select="$gRepository/book[owner/@href=$gRepository/actor[@OTCmlId=$gParty[@id=concat($gHeader/sendTo/@href,'')]/@OTCmlId]/@id]/identifier"/>
  <!--To add Spécific Timezone-->
  <!--<xsl:variable name="gHeaderTimezone" select="$gSendBy-routingAddress/city"/>-->

  <xsl:variable name="gReportSections_Node">
    <section name="orderConfirmation" sort="1" optional="{false()}"/>
    <section name="tradesConfirmations" sort="2" optional="{false()}"/>
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
    -	ETD-->
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
    <xsl:variable name="vBizETDTradesConfirmations" select="msxsl:node-set($vBizETDTradesConfirmations_Node)/date"/>

    <!--2/ Section: OrderConfirmation -->
    <xsl:variable name="vBizETDOrderConfirmationNode">
      <xsl:apply-templates select="$vBizETDTradesConfirmations" mode="delete">
        <xsl:with-param name="pDelete" select="',subTotal,subTotalAmount,'" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="vBizETDOrderConfirmation" select="msxsl:node-set($vBizETDOrderConfirmationNode)/*"/>

    <!--Activity Stat variables -->
    <xsl:variable name="vActivityCount_TradesConfirmations" select="count($vBizETDTradesConfirmations[1])"/>

    <xsl:variable name="vActivity_ETD">
      <xsl:choose>
        <xsl:when test="count($vBizETDTradesConfirmations[1]) > 0">
          <xsl:value-of select="1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vActivityCount" select="$vActivity_ETD"/>

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
        </tradesConfirmations>
        <ordersConfirmations activityCount="{$vActivityCount_TradesConfirmations}">
          <etd>
            <xsl:copy-of select="$vBizETDOrderConfirmation"/>
          </etd>
        </ordersConfirmations>
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

                <!--1/ Section: OrderConfirmation / Mandatory -->
                <xsl:choose>
                  <xsl:when test="$vActivityCount_TradesConfirmations = 0">
                    <xsl:call-template name="Synthesis_Section">
                      <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_ORD"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:if test="$vBizETDOrderConfirmation">
                      <xsl:call-template name="Synthesis_Section">
                        <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_ORD"/>
                        <xsl:with-param name="pBusiness_Section" select="$vBizETDOrderConfirmation"/>
                        <xsl:with-param name="pIsDisplayInstr" select="$vIsMultiActivity"/>
                      </xsl:call-template>
                    </xsl:if>
                  </xsl:otherwise>
                </xsl:choose>
                <!--2/ Section: TradesConfirmations / Mandatory -->
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
                  </xsl:otherwise>
                </xsl:choose>
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
        <xsl:if test="$vBizETDTradesConfirmations">

          <fo:block font-size="{$gData_font-size}" padding="{$gBlockSettings_Legend/@padding}"
                    display-align="{$gData_display-align}" keep-together.within-page="always" space-before="{$gLegend_space-before}">

            <!--Legend_FeesExclTax-->
            <xsl:if test="$vBizETDTradesConfirmations or $vBizETDOrderConfirmation">

              <xsl:call-template name="UKDisplay_Legend">
                <xsl:with-param name="pLegend" select="$gcLegend_FeesExclTax"/>
                <xsl:with-param name="pResource" select="'Report-LegendFeesExclTax'"/>
              </xsl:call-template>
            </xsl:if>
          </fo:block>
        </xsl:if>

        <fo:block id="LastPage"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- .......................................................................... -->
  <!--  Section: TRADES CONFIRMATIONS                                             -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_TradesConfirmationsData                -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display TradesConfirmations Content             -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_TradesConfirmationsData">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <xsl:variable name="vQtyModel" select="'2DateSideQty'"/>
    <xsl:variable name="vColumnCount" select="number('20')"/>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">
      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
      <!--<fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />-->
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Side']/@column-width}" />
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='SideQty2']/@column-width}" />
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
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='ClearingDate']/@resource"/>
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
          <!--<fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>-->
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
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
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
												 display-align="{$gData_display-align}"
                         number-columns-spanned="4">
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
            <xsl:when test="$pBizDC/@category = 'O' and $pBizDC/@futValuationMethod = 'EQTY'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                             display-align="{$gData_display-align}"
                             number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Premium']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             number-columns-spanned="4">
                <xsl:call-template name="Debug_border-green"/>
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

          <!--Asset details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}">

            <fo:table-cell text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                           font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="{$vColumnCount}">

              <!-- Header -->
              <xsl:call-template name="UKAsset_Header">
                <xsl:with-param name="pRepository-Asset" select="$vRepository_Asset"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>

          </fo:table-row>

          <!--Order details-->
          <xsl:variable name="vBizOrder" select="$vAssetBizTradeList[1]"/>

          <!--Display Trade Details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@trdNum" data-type="text"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <!--Only one EfsmlTrade-->
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId][1]"/>
            <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]"/>
            <xsl:variable name="vFixmlTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <xsl:variable name="vColor">
              <xsl:if test="$vBizTrade/@trdDt != $gValueDate">
                <xsl:value-of select="'#606060'"/>
              </xsl:if>
            </xsl:variable>

            <!--Data row, with first Fee and amount1-->
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}"
                          keep-together.within-page="always">

              <xsl:if test="string-length($vColor) > 0">
                <xsl:call-template name="Display_AddAttribute-color">
                  <xsl:with-param name="pColor" select="$vColor"/>
                </xsl:call-template>
              </xsl:if>

              <xsl:if test="position()=1">
                <xsl:attribute name="keep-with-previous">
                  <xsl:value-of select="'always'"/>
                </xsl:attribute>
              </xsl:if>

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
                    <xsl:with-param name="pData" select="$vCommonTrade/@bizDt"/>
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
              <!--<fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Type']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='Type']/data/@font-weight}"
                             text-align="{$gBlockSettings_Data/column[@name='Type']/data/@text-align}"
                             padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'TrdTyp'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vFixmlTrade"/>
                    <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Type']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>-->
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'Side'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vFixmlTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                    <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" 
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'Qty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vFixmlTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
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
                <!--RD 20200108 [25052] Faire le cumul des frais par trade -->
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

            <xsl:call-template name="UKDisplayRow_TradeDet">
              <!--<xsl:with-param name="pOtherPrice" select="$vDisplayOtherPrice"/>-->
              <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
              <!--RD 20200108 [25052] Faire le cumul des frais par trade -->
              <xsl:with-param name="pFee" select="$vBizTrade/fee"/>
              <xsl:with-param name="pOtherAmount2" select="$vBizTrade/amount/amount2"/>
              <xsl:with-param name="pOtherAmount3" select="$vBizTrade/amount/amount3"/>
              <xsl:with-param name="pOtherAmount4" select="$vBizTrade/amount/amount4"/>
              <xsl:with-param name="pQtyPattern" select="$vBizAsset/pattern/@qty" />
              <!--RD 20200108 [25052] Ce n'est pas le Qty model -->
              <!--<xsl:with-param name="pIsQtyModel" select="true()"/>-->
              <xsl:with-param name="pColor" select="$vColor"/>
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
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                        keep-with-previous="always">

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
            <fo:table-cell padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Side']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'BuySide'"/>
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
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
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>

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

          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pPosition" select="position()"/>
            <xsl:with-param name="pNumber-columns" select="$vColumnCount"/>
          </xsl:call-template>

          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_OrderConfirmationsData                -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display OrderConfirmations Content             -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_OrderConfirmationsData">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <xsl:variable name="vQtyModel" select="'DateSideQty'"/>
    <xsl:variable name="vColumnCount" select="number('14')"/>

    <fo:table table-layout="fixed" width="{$gSection_width}" table-omit-header-at-break="false" keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}" />
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}" />
      <!--<fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}" />-->
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Side']/@column-width}" />
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='SideQty2']/@column-width}" />
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}" />
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}" />
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}" />
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}" />
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}" />
      <fo:table-column column-number="11" column-width="proportional-column-width(1)" />
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='OrderType']/@column-width}" />
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='OrderCategory']/@column-width}" />
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='OrderStatus']/@column-width}" />

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}" font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}" background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='EntryDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <!--<xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='ClearingDate']/@resource"/>-->
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
												 display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header[@name='OrderNum']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!--<fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>-->
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
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
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
                         display-align="{$gData_display-align}"
                         text-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='OrderType']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         text-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='OrderCategory']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         text-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='OrderStatus']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
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

          <!--Asset details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}">

            <fo:table-cell text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                           font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="{$vColumnCount}">

              <!--Header-->
              <xsl:call-template name="UKAsset_Header">
                <xsl:with-param name="pRepository-Asset" select="$vRepository_Asset"/>
              </xsl:call-template>

              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>

          </fo:table-row>

          <!--Order details-->
          <!--<xsl:variable name="vBizOrder" select="$vAssetBizTradeList[1]"/>-->
          <xsl:variable name="vBizOrderFilled" select="$vAssetBizTradeList[@tradeId=$vAssetTradeList[*/FIXML/TrdCaptRpt/@OrdStat='2']/@tradeId][1]"/>
          <xsl:variable name="vBizOrder" select="$vBizOrderFilled|$vAssetBizTradeList[$vBizOrderFilled=false()][1]"/>
          <xsl:variable name="vFixmlOrder" select="$vAssetTradeList[@tradeId=$vBizOrder/@tradeId]"/>
          <xsl:variable name="vEfsmlOrder" select="$vAssetEfsmlTradeList[@tradeId=$vBizOrder/@tradeId]"/>
          <xsl:variable name="vCommonOrder" select="$gCommonData/trade[@tradeId=$vBizOrder/@tradeId]"/>

          <xsl:variable name="vSubTotal" select="$vBookSubTotal[@idAsset=$vBizAsset/@OTCmlId and @assetCategory=$vBizAsset/@assetCategory and @idI=$vBizAsset/@idI][1]"/>

          <xsl:variable name="vOrderStatus">
            <xsl:call-template name="DisplayData_Fixml">
              <xsl:with-param name="pDataName" select="'OrdStat'"/>
              <xsl:with-param name="pFixmlTrade" select="$vFixmlOrder"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vOrderCategory">
            <xsl:call-template name="DisplayData_Fixml">
              <xsl:with-param name="pDataName" select="'OrdCat'"/>
              <xsl:with-param name="pFixmlTrade" select="$vFixmlOrder"/>
            </xsl:call-template>
          </xsl:variable>

          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gData_font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gData_display-align}"
                        keep-together.within-page="always"
                        keep-with-previous="always">

            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_Format">
                  <!--RD 20191007 [24976] Use Order entry date -->
                  <!--<xsl:with-param name="pData" select="$vBizOrder/@trdDt"/>-->
                  <xsl:with-param name="pData" select="$vCommonOrder/@ordDt"/>
                  <xsl:with-param name="pDataType" select="'Date'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <!--<xsl:call-template name="DisplayData_Format">
                  <xsl:with-param name="pData" select="$vCommonOrder/@bizDt"/>
                  <xsl:with-param name="pDataType" select="'Date'"/>
                </xsl:call-template>-->
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_Format">
                  <xsl:with-param name="pData" select="$vCommonOrder/@orderId"/>
                  <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <!--<fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
              </fo:block>
            </fo:table-cell>-->
            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_Fixml">
                  <xsl:with-param name="pDataName" select="'Side'"/>
                  <xsl:with-param name="pFixmlTrade" select="$vFixmlOrder"/>
                  <xsl:with-param name="pDataEfsml" select="$vEfsmlOrder"/>
                  <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                           text-align="{$gData_Number_text-align}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="$vOrderStatus='Filled'">
                    <xsl:call-template name="DisplayData_SubTotal">
                      <xsl:with-param name="pDataName" select="'LongQty'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@qty" />
                      <xsl:with-param name="pQtyModel" select="$vQtyModel"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'N/A'"/>
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

            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}" text-align="{$gData_Number_text-align}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="$vOrderStatus='Filled'">
                    <xsl:call-template name="UKDisplay_SubTotal_AvgPx">
                      <xsl:with-param name="pDataName" select="'AvgBuy'"/>
                      <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                      <xsl:with-param name="pNumberPattern" select="$vBizAsset/pattern/@avgPx"/>
                      <xsl:with-param name="pIsPos" select="false()"/>
                      <xsl:with-param name="pWithRes" select="false()"/>
                      <xsl:with-param name="pQtyModel" select="$vQtyModel"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'N/A'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                           text-align="{$gBlockSettings_Data/column[@name='OrderType']/data/@text-align}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:variable name="vOrdTyp">
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'OrdTyp'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vFixmlOrder"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:choose>
                  <xsl:when test="string-length($vOrdTyp) > 0">
                    <xsl:value-of select="$vOrdTyp"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'N/A'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                           text-align="{$gBlockSettings_Data/column[@name='OrderCategory']/data/@text-align}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="string-length($vOrderCategory) > 0">
                    <xsl:value-of select="$vOrderCategory"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'N/A'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                           text-align="{$gBlockSettings_Data/column[@name='OrderStatus']/data/@text-align}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="string-length($vOrderStatus) > 0">
                    <xsl:value-of select="$vOrderStatus"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'N/A'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <!--Other:Timestamp, ...-->
          <xsl:variable name="vOrderTimestamp">
            <xsl:call-template name="DisplayData_Efsml">
              <xsl:with-param name="pDataName" select="'TimeStampOrd'"/>
              <xsl:with-param name="pDataEfsml" select="$vBizOrder"/>
            </xsl:call-template>
          </xsl:variable>
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gData_font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gData_display-align}"
                        keep-together.within-page="always"
                        keep-with-previous="always">

            <xsl:call-template name="UKDisplay_Timestamp">
              <xsl:with-param name="pTimestamp" select="$vOrderTimestamp"/>
            </xsl:call-template>
          </fo:table-row>

          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pPosition" select="position()"/>
            <xsl:with-param name="pNumber-columns" select="$vColumnCount"/>
          </xsl:call-template>

          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>

        <xsl:variable name="vTraderCopyNode">
          <xsl:copy-of select="$gCommonData/trade[@tradeId=$pBizBook/asset/trade/@tradeId]/trader"/>
        </xsl:variable>
        <xsl:variable name="vTraderCopy" select="msxsl:node-set($vTraderCopyNode)/trader"/>

        <xsl:variable name="vTrader-Trader_List" select="$vTraderCopy[generate-id()=generate-id(key('kTrader-traderId',@traderId)[1])]"/>

        <xsl:for-each select="$gRepository/actor[identifier/text()=$vTrader-Trader_List/@traderId]">
          <xsl:sort select="identifier/text()"/>

          <xsl:variable name="vRepository-Trader" select="current()"/>

          <!--Trader details-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}">

            <fo:table-cell text-align="{$gBlockSettings_Trader/column[@name='TraderDet']/@text-align}"
                           font-size="{$gBlockSettings_Trader/column[@name='TraderDet']/@font-size}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="{$vColumnCount}">

              <!-- Header -->
              <xsl:call-template name="UKTrader_Detail">
                <xsl:with-param name="pRepository-Actor" select="$vRepository-Trader"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>
          </fo:table-row>

          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>

        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

</xsl:stylesheet>
