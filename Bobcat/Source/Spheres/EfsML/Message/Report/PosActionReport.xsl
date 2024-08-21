<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!--  
================================================================================================================
Summary : Spheres report - Position action report 
          XSL for generation PDF document of Position action report  
          
File    : PosActionReport.xsl 
================================================================================================================
Version : v4.6.5617                                           
Date    : 20150519
Author  : RD
Comment : [21034] Manage corporate action 
================================================================================================================
Version : v3.7.5155                                           
Date    : 20140220
Author  : RD
Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking  
================================================================================================================
Version : v1.0.2.0 (Spheres 2.6.4.0)
Date    : 20120822 
Author  : MF
Comment : [Ticket 18073] Add converted prices to the position action report. 
          The strike price, the transaction price either impacted (node trade2/convertedLastPx) and involved, 
          and the underlyer price (node convertedUnderlyerPx) will be displayed using the derivative contract 
          specific numerical base and the given style.   
================================================================================================================
Version : v1.0.1.0 (Spheres 2.6.4.0)
Date    : 20120712 
Author  : MF
Comment : [Ticket 18006] Add SecondaryTrdType [@TrdTyp2] 
================================================================================================================
Version : v1.0.0.0                                           
Date    : 20120509
Author  : RD
Comment : First version
================================================================================================================
  -->

  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="Shared/Shared_Report_PDFETD.xslt" />

  <!-- ================================================== -->
  <!--        Parameters                                  -->
  <!-- ================================================== -->
  <xsl:param name="pCurrentCulture" select="'en-GB'" />

  <!-- ================================================== -->
  <!--        Global Variables                            -->
  <!-- ================================================== -->
  <xsl:variable name="gDataDocumentTrades" select="//dataDocument/trade[tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']/text()=$gPosActions/trades/trade/@tradeIdentifier]"/>
  <xsl:variable name="gReportType" select="$gcReportTypePA"/>

  <!--Legend-->
  <xsl:variable name="gIsReportWithSpecificLegend" select="false()"/>
  <xsl:variable name="gIsReportWithOrderTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithTradeTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithPriceTypeLegend" select="true()"/>

  <!-- ================================================== -->
  <!--        Display Settings                            -->
  <!-- ================================================== -->
  <!-- ===== Display settings of all Blocks===== -->
  <xsl:variable name="gDetTrdTableTotalWidth">
    <xsl:choose>
      <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
        <xsl:value-of select="'183.5mm'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'197mm'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gGroupDisplaySettings">
    <group sort="1" name="ETDBlock" padding-top="2mm">
      <!--Title-->
      <title empty-row="1mm" withColumnHdr="{true()}" withSubColumn="{true()}" font-size="7pt">
        <column number="01" name="TS-MarketTitle" colspan="3">
          <header ressource="TS-MARKET" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="02" name="TS-DCTitle" colspan="7">
          <header ressource="TS-DERIVATIVECONTRACT" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>

        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <column number="03" name="TS-DCUNLTitle" colspan="9">
              <header ressource="TS-DERIVATIVECONTRACTUNL" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
          </xsl:when>
          <xsl:otherwise>
            <column number="03" name="TS-DCUNLTitle" colspan="8">
              <header ressource="TS-DERIVATIVECONTRACTUNL" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
          </xsl:otherwise>
        </xsl:choose>

      </title>

      <xsl:variable name="vColNumberBase">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <xsl:value-of select="number('1')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="number('0')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$gIsMonoBook = 'false' or $gHideBookForMonoBookReport = false()">
        <column number="1" name="TS-Book" width="13.5mm" >
          <header rowspan="2"/>
          <data rowspan="2" length="8"/>
        </column>
      </xsl:if>

      <column number="{number('2') - number($vColNumberBase)}" name="PA-ActType" width="10mm">
        <header master-ressource="PA-ActHdr" master-ressource-colspan="2"/>
        <data text-align="center" length="7"/>
      </column>
      <column number="{number('2') - number($vColNumberBase)}" name="PA-ActTypeDet" master-column="PA-ActType" width="10mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}" length="11"/>
      </column>
      <column number="{number('3') - number($vColNumberBase)}" name="PA-Qty" width="9mm">
        <header master-ressource="PA-ActHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="{number('4') - number($vColNumberBase)}" name="TS-Maturity" colspan="2" width="20mm">
        <data text-align="center"/>
      </column>
      <column number="{number('4') - number($vColNumberBase)}" name="TS-PutCall" master-column="TS-Maturity" width="5.5mm"/>
      <column number="{number('5') - number($vColNumberBase)}" name="TS-ConvertedStrike" master-column="TS-Maturity" width="14.5mm">
        <header text-align="right"/>
        <data text-align="right" typenode="true"/>
      </column>
      <column number="{number('6') - number($vColNumberBase)}" name="PA-TradeSide" width="3mm">
        <header master-ressource="PA-TradeConcernedHdr" master-ressource-colspan="4"/>
        <data rowspan="2" text-align="center"/>
      </column>
      <column number="{number('7') - number($vColNumberBase)}" name="TS-TradeDate" width="14mm">
        <header master-ressource="PA-TradeConcernedHdr" master-ressource-colspan="0"/>
        <data text-align="center"/>
      </column>
      <column number="{number('7') - number($vColNumberBase)}" name="TS-TradeTime" master-column="TS-TradeDate" width="14mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('8') - number($vColNumberBase)}" name="TS-TradeNum" width="18mm">
        <header master-ressource="PA-TradeConcernedHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="center" length="12"/>
      </column>
      <column number="{number('9') - number($vColNumberBase)}" name="TS-ConvertedPrice" width="13.5mm">
        <header ressource="Report-Price" master-ressource="PA-TradeConcernedHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="right" typenode="true"/>
      </column>
      <column number="{number('10') - number($vColNumberBase)}" name="PA-TradeImpactedSide" width="3mm">
        <header master-ressource="PA-TradeImpactedHdr" master-ressource-colspan="5"/>
        <data rowspan="2" text-align="center"/>
      </column>
      <column number="{number('11') - number($vColNumberBase)}" name="PA-TradeImpactedDate" width="14mm">
        <header master-ressource="PA-TradeImpactedHdr" master-ressource-colspan="0"/>
        <data text-align="center"/>
      </column>
      <column number="{number('11') - number($vColNumberBase)}" name="PA-TradeImpactedTime" master-column="PA-TradeImpactedDate" width="14mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('12') - number($vColNumberBase)}" name="PA-TradeImpactedNum" width="18mm">
        <header master-ressource="PA-TradeImpactedHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="center" length="12"/>
      </column>
      <column number="{number('13') - number($vColNumberBase)}" name="PA-TradeImpactedPriceType" width="2.5mm">
        <header colspan="0"/>
        <data rowspan="2" font-size="{$gDetTrdDetFontSizeTime}" border-right-style="none"/>
      </column>
      <column number="{number('14') - number($vColNumberBase)}" name="PA-ConvertedTradeImpactedPrice" width="11.5mm">
        <header colspan="2" ressource="Report-Price" master-ressource="PA-TradeImpactedHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="right" border-left-style="none" typenode="true"/>
      </column>
      <!--Brokerage: 1st row-->
      <xsl:choose>
        <xsl:when test="$gPaymentTypes[@id='Bro']">
          <column number="{number('15') - number($vColNumberBase)}" name="TS-Brokerage" width="12mm">
            <header colspan="2"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="TS-BrokerageDrCr" width="3mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gPaymentTypes[@id='BroTrd'] and $gPaymentTypes[@id='BroClr']">
          <column number="{number('15') - number($vColNumberBase)}" name="TS-BrokerageTrd" width="12mm">
            <header colspan="2" ressource="TS-Trd" ressource1="TS-Brokerage"/>
            <data text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="TS-BrokerageTrdDrCr" width="3mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:otherwise>
          <column number="{number('15') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="EmptyColumn" width="3mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:otherwise>
      </xsl:choose>
      <!--Brokerage: 2nd row-->
      <xsl:choose>
        <xsl:when test="$gPaymentTypes[@id='Bro']">
          <column number="{number('16') - number($vColNumberBase)}" name="EmptyColumn" master-column="TS-BrokerageDrCr" width="3mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gPaymentTypes[@id='BroTrd'] and $gPaymentTypes[@id='BroClr']">
          <column number="{number('15') - number($vColNumberBase)}" name="TS-BrokerageClr" master-column="TS-BrokerageTrd" width="12mm">
            <header colspan="2" ressource="PA-Clr"/>
            <data text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="TS-BrokerageClrDrCr" master-column="TS-BrokerageTrdDrCr" width="3mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:otherwise>
          <column number="{number('15') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="EmptyColumn" width="3mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:otherwise>
      </xsl:choose>
      <!--Fee: 1st row-->
      <xsl:choose>
        <xsl:when test="$gPaymentTypes[@id='Fee']">
          <column number="{number('17') - number($vColNumberBase)}" name="TS-Fee" width="12mm">
            <header colspan="2"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="TS-FeeDrCr" width="3mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gPaymentTypes[@id='FeeTrd'] and $gPaymentTypes[@id='FeeClr']">
          <column number="{number('17') - number($vColNumberBase)}" name="TS-FeeTrd" border-right-style="none" width="12mm">
            <header colspan="2" ressource="TS-Trd" ressource1="TS-Fee"/>
            <data text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="TS-FeeTrdDrCr" width="3mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:otherwise>
          <column number="{number('17') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="EmptyColumn" width="3mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:otherwise>
      </xsl:choose>
      <!--Fee: 2nd row-->
      <xsl:choose>
        <xsl:when test="$gPaymentTypes[@id='Fee']">
          <column number="{number('18') - number($vColNumberBase)}" name="EmptyColumn" master-column="TS-FeeDrCr" width="3mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gPaymentTypes[@id='FeeTrd'] and $gPaymentTypes[@id='FeeClr']">
          <column number="{number('17') - number($vColNumberBase)}" name="TS-FeeClr" master-column="TS-FeeTrd" width="12mm">
            <header colspan="2" ressource="PA-Clr"/>
            <data text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="TS-FeeClrDrCr" master-column="TS-FeeTrdDrCr" width="3mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:otherwise>
          <column number="{number('17') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="EmptyColumn" width="3mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:otherwise>
      </xsl:choose>
      <column number="{number('19') - number($vColNumberBase)}" name="PA-RMG" width="13mm">
        <header colspan="2" ressource1="PA-RMG1"/>
        <data text-align="right" border-right-style="none"/>
      </column>
      <column number="{number('19') - number($vColNumberBase)}" name="PA-CASHSTL" master-column="PA-RMG" width="13mm">
        <header colspan="2"/>
        <data text-align="right" border-right-style="none"/>
      </column>
      <column number="{number('20') - number($vColNumberBase)}" name="PA-RMGDrCr" width="3mm">
        <header colspan="0"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
      </column>
      <column number="{number('20') - number($vColNumberBase)}" name="PA-CASHSTLDrCr" master-column="PA-RMGDrCr" width="3mm">
        <header colspan="0"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
      </column>
    </group>
    <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom' or 
            $gReportHeaderFooter/summaryFees/text() = 'Bottom'">
      <group sort="2" name="CBStream" isDataGroup="{true()}" padding-top="0.3cm">
        <!--Titles-->
        <title empty-row="0.1cm" withColumnHdr="{false()}" withSubColumn="{false()}" font-size="7pt" font-weight="bold"
               background-color="{$gDetTrdHdrCellBackgroundColor}">
          <column number="01" name="TS-RecapCashFlowsTitle" colspan="7">
            <data border-bottom-style="solid"/>
          </column>
          <column number="02" name="TS-FxRate" colspan="3">
            <data border-bottom-style="solid" text-align="right" display-align="center" font-size="5pt"/>
          </column>
        </title>

        <!--Columns-->
        <column number="01" name="TS-Designation1" width="3mm">
          <header colspan="3"
                  ressource="CSHBAL-Designation"
                  font-size="7pt" font-weight="bold" text-align="left" border-right-style="none"/>
          <data border-bottom-style="none" border-top-style="none"/>
        </column>
        <column number="02" name="TS-Designation2" width="26mm">
          <header colspan="-1"/>
        </column>
        <column number="03" name="TS-Designation3" width="68mm">
          <header colspan="-1"/>
        </column>
        <column number="04" name="TS-Currency" width="8mm">
          <header colspan="-1"/>
          <data text-align="right" border-right-style="none" />
        </column>
        <column number="05" name="TS-Amount" width="26mm">
          <header ressource="%%RES:CSHBAL-AmountTitle%%"
                  text-align="center" colspan="3" font-size="7pt" font-weight="bold" border-left-style="none"/>
          <data text-align="right" border-left-style="none" border-right-style="none" decPattern="%%PAT:currency.PARENT%%"/>
        </column>
        <column number="06" name="TS-CrDr" width="3.5mm" font-size="5pt">
          <header colspan="-1"/>
          <data border-left-style="none" display-align="center"/>
        </column>
        <column number="07" name="TS-Desc" width="25mm" font-size="6pt">
          <header ressource=" " colspan="2" border-right-style="none"/>
          <data font-style="italic" display-align="center"/>
        </column>
        <column number="08" name="TS-ExCurrency" width="8mm">
          <header colspan="-1"/>
          <data text-align="right" border-right-style="none"/>
        </column>
        <column number="09" name="TS-ExAmount" width="26mm">
          <header ressource="%%RES:CSHBAL-CounterValueTitle%% {$gReportingCurrency}"
                  text-align="right" colspan="2" font-size="7pt" font-weight="bold" border-left-style="none"/>
          <data text-align="right" border-left-style="none" border-right-style="none" decPattern="%%PAT:RepCurrency%%"/>
        </column>
        <column number="10" name="TS-ExCrDr" width="3.5mm" font-size="5pt">
          <header colspan="-1"/>
          <data border-left-style="none" display-align="center"/>
        </column>

        <condition name="IsFeesDetailsExist" condition="feeGrossAmount"/>

        <!--data of summaryCashFlows-->
        <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom'">

          <row sort="1" name="futureRMG" font-size="7pt" >
            <column name="TS-Designation1">
              <data border-top-style="solid"/>
            </column>
            <column name="TS-Designation2">
              <data colspan="2" value="%%RES:CSHBAL-futureRMG%%"/>
            </column>
            <column name="TS-Currency"/>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%"/>
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%"/>
            </column>
            <column name="TS-Desc"/>
            <column name="TS-ExCurrency"/>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%"/>
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%"/>
            </column>
          </row>
          <row sort="2" name="optionRMG" font-size="7pt" >
            <column name="TS-Designation1"/>
            <column name="TS-Designation2">
              <data colspan="2" value="%%RES:CSHBAL-optionRMG%%"/>
            </column>
            <column name="TS-Currency"/>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%"/>
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%"/>
            </column>
            <column name="TS-Desc"/>
            <column name="TS-ExCurrency"/>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%"/>
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%"/>
            </column>
          </row>
          <row sort="3" name="cashSettlement" font-size="7pt" >
            <column name="TS-Designation1"/>
            <column name="TS-Designation2">
              <data colspan="2" value="%%RES:CSHBAL-cashSettlement%%"/>
            </column>
            <column name="TS-Currency"/>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%"/>
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%"/>
            </column>
            <column name="TS-Desc"/>
            <column name="TS-ExCurrency"/>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%"/>
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%"/>
            </column>
          </row>
          <row sort="4" name="feesVAT" font-size="7pt" >
            <column name="TS-Designation1">
              <data border-bottom-style="solid"/>
            </column>
            <column name="TS-Designation2">
              <data colspan="2" value="%%RES:CSHBAL-feesVAT%%" border-bottom-style="solid"/>
              <last border-bottom-style="solid" />
            </column>
            <column name="TS-Currency">
              <data border-bottom-style="solid"/>
            </column>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%" border-bottom-style="solid"/>
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%" border-bottom-style="solid"/>
            </column>
            <column name="TS-Desc">
              <data value="%%DESC:DetFees%%" condition="IsFeesDetailsExist" border-bottom-style="solid" />
            </column>
            <column name="TS-ExCurrency">
              <data border-bottom-style="solid"/>
            </column>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%" border-bottom-style="solid"/>
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%" border-bottom-style="solid"/>
            </column>
          </row>
        </xsl:if>
        <!--data of summaryFees-->
        <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryFees/text() = 'Bottom'">

          <row sort="4" name="%%EMPTY%%" height="0.2cm" condition="IsFeesDetailsExist"/>
          <row sort="5" name="%%FREE%%" condition="IsFeesDetailsExist"
           font-size="7pt" font-weight="bold"
           background-color="{$gDetTrdHdrCellBackgroundColor}">
            <column number="01" name="%%EMPTY%%"/>
            <column name="TS-Designation2">
              <data colspan="2" value="%%RES:CSHBAL-feesConstituents%%"/>
            </column>
            <column number="03" name="%%EMPTY%%">
              <data colspan="7" border-left-style="solid"/>
            </column>
          </row>
          <row sort="6" name="feeGrossAmount" dataLink="%%DATA:keyForSum%%" font-size="7pt">
            <column name="%%EMPTY%%"/>
            <column name="TS-Designation2">
              <data rowspan="%%DATA:rowspan%%" value="%%DATA:keyForSum.RES%%" display-align="center"/>
            </column>
            <column name="TS-Designation3">
              <data value="%%RES:CSHBAL-feeGrossAmount%%" display-align="center" />
            </column>
            <column name="TS-Currency"/>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%" display-align="center" />
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%" font-size="5pt"/>
            </column>
            <column name="TS-Desc"/>
            <column name="TS-ExCurrency"/>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%" display-align="center" />
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%" font-size="5pt"/>
            </column>
            <row sort="7" name="feeTax" dataLink="%%DATA:keyForSum%%" font-size="7pt">
              <column name="%%EMPTY%%">
                <data border-right-style="solid"/>
              </column>
              <column name="TS-Designation3">
                <data value="%%DATA:taxDetailName.RES%% (">
                  <inline decPattern="%%PAT:2Dec%%">
                    <xsl:value-of select="'%%DATA:taxDetailRate%%'"/>
                  </inline>
                  <inline>
                    <xsl:value-of select="' %)'"/>
                  </inline>
                </data>
              </column>
              <column name="TS-Currency"/>
              <column name="TS-Amount">
                <data value="%%DATA:amount%%"/>
              </column>
              <column name="TS-CrDr">
                <data value="%%DATA:crDr%%" font-size="5pt"/>
              </column>
              <column name="TS-Desc"/>
              <column name="TS-ExCurrency"/>
              <column name="TS-ExAmount">
                <data value="%%DATA:exAmount%%"/>
              </column>
              <column name="TS-ExCrDr">
                <data value="%%DATA:crDr%%" font-size="5pt"/>
              </column>
            </row>
          </row>
          <row sort="8" name="totalFeesVAT" font-size="7pt" font-weight="bold" background-color="{$gDetTrdHdrCellBackgroundColor}">
            <column name="%%EMPTY%%" background-color="white"/>
            <column name="TS-Designation2">
              <data colspan="2"
                    value="%%RES:CSHBAL-totalFeesVAT%%"
                    border-bottom-style="solid"/>
            </column>
            <column name="TS-Currency">
              <data value="%%DATA:currency.PARENT%%"
                    border-bottom-style="solid"/>
            </column>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%"
                    border-bottom-style="solid"/>
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%"
                    font-size="5pt"
                    border-bottom-style="solid"/>
            </column>
            <column name="TS-Desc">
              <data border-bottom-style="solid"/>
            </column>
            <column name="TS-ExCurrency">
              <data value="{$gReportingCurrency}"
                border-bottom-style="solid"/>
            </column>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%"
                border-bottom-style="solid"/>
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%"
                font-size="5pt"
                border-bottom-style="solid"/>
            </column>
          </row>
        </xsl:if>
      </group>
    </xsl:if>
    <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom'">
      <group sort="3" name="CBStreamEx" isDataGroup="{true()}" padding-top="3mm">
        <!--Titles-->
        <title empty-row="1mm" withColumnHdr="{false()}" withSubColumn="{false()}" font-size="7pt" font-weight="bold"
               background-color="{$gDetTrdHdrCellBackgroundColor}">
          <column number="01" name="TS-RecapCashFlowsExTitle" colspan="7">
            <data border-bottom-style="solid" value="%%RES:CSHBAL-CBStreamEx%% {$gReportingCurrency}"/>
          </column>
        </title>

        <!--Columns-->
        <column number="01" name="TS-Designation1" width="3mm">
          <header colspan="3"
                  ressource="CSHBAL-Designation"
                  font-size="7pt" font-weight="bold" text-align="left"/>
          <data border-bottom-style="none" border-top-style="none"/>
        </column>
        <column number="02" name="TS-Designation2" width="94mm">
          <header colspan="-1"/>
        </column>
        <column number="03" name="TS-Filler" width="37.5mm">
          <header colspan="-1"/>
        </column>
        <column number="04" name="TS-Desc" width="25mm">
          <header ressource=" " colspan="2" border-right-style="none"/>
        </column>
        <column number="05" name="TS-ExCurrency" width="8mm">
          <header colspan="-1"/>
          <data text-align="right" border-right-style="none"/>
        </column>
        <column number="06" name="TS-ExAmount" width="26mm">
          <header ressource="%%RES:CSHBAL-CounterValueTitle%% {$gReportingCurrency}"
                  text-align="right" colspan="2" font-size="7pt" font-weight="bold" border-left-style="none"/>
          <data text-align="right" border-left-style="none" border-right-style="none" decPattern="%%PAT:RepCurrency%%"/>
        </column>
        <column number="07" name="TS-ExCrDr" width="3.5mm">
          <header colspan="-1"/>
          <data border-left-style="none" display-align="center" font-size="5pt"/>
        </column>

        <!--data-->
        <row sort="1" name="futureRMG" font-size="7pt" >
          <column name="TS-Designation1">
            <data border-top-style="solid"/>
          </column>
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-futureRMG%%"/>
          </column>
          <column name="TS-Filler"/>
          <column name="TS-Desc"/>
          <column name="TS-ExCurrency"/>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold"/>
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold"/>
          </column>
        </row>
        <row sort="2" name="optionRMG" font-size="7pt" >
          <column name="TS-Designation1"/>
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-optionRMG%%"/>
          </column>
          <column name="TS-Filler"/>
          <column name="TS-Desc"/>
          <column name="TS-ExCurrency"/>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold"/>
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold"/>
          </column>
        </row>
        <row sort="3" name="cashSettlement" font-size="7pt" >
          <column name="TS-Designation1"/>
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-cashSettlement%%"/>
          </column>
          <column name="TS-Filler"/>
          <column name="TS-Desc"/>
          <column name="TS-ExCurrency"/>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold"/>
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold"/>
          </column>
        </row>
        <row sort="4" name="feesVAT" font-size="7pt" >
          <column name="TS-Designation1">
            <data border-bottom-style="solid"/>
          </column>
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-feesVAT%%"
                  border-bottom-style="solid"/>
          </column>
          <column name="TS-Filler">
            <data border-bottom-style="solid"/>
          </column>
          <column name="TS-Desc">
            <data border-bottom-style="solid"/>
          </column>
          <column name="TS-ExCurrency">
            <data border-bottom-style="solid"/>
          </column>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold"
                  border-bottom-style="solid"/>
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold"
                  border-bottom-style="solid"/>
          </column>
        </row>
      </group>
    </xsl:if>
  </xsl:variable>
  <!-- ===== SubTotals of principal table settings===== -->
  <xsl:variable name="gColSubTotalColDisplaySettings">
    <group name="ETDBlock">
      <row sort="1">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <subtotal sort="1" name="EmptySubTotal" empty-columns="10" />
          </xsl:when>
          <xsl:otherwise>
            <subtotal sort="1" name="TS-Book" columns-spanned="12" text-align="left"/>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test="$gPaymentTypes[@id='Bro']">
          <subtotal sort="2" name="TS-BrokerageSum" empty-columns="2" />
          <subtotal sort="3" name="TS-BrokerageSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='BroTrd']">
          <subtotal sort="2" name="TS-BrokerageTrdSum" empty-columns="2" />
          <subtotal sort="3" name="TS-BrokerageTrdSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='Fee']">
          <subtotal sort="4" name="TS-FeeSum" />
          <subtotal sort="5" name="TS-FeeSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='FeeTrd']">
          <subtotal sort="4" name="TS-FeeTrdSum"/>
          <subtotal sort="5" name="TS-FeeTrdSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <subtotal sort="6" name="PA-RMGSum" />
        <subtotal sort="7" name="PA-RMGSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
      </row>
      <row sort="2">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <condition>
              <subtotal name="TS-BrokerageClrSum"/>
              <subtotal name="TS-FeeClrSum"/>
            </condition>
            <subtotal sort="1" name="EmptySubTotal" empty-columns="10" />
          </xsl:when>
          <xsl:when test="$gIsMonoBook = 'true'">
            <condition>
              <subtotal name="TS-BrokerageClrSum"/>
              <subtotal name="TS-FeeClrSum"/>
            </condition>
            <subtotal sort="1" name="EmptySubTotal" empty-columns="12" />
          </xsl:when>
          <xsl:otherwise>
            <subtotal sort="1" name="TS-BookDisplayname" columns-spanned="12" font-size="{$gDetTrdDetFontSize}" text-align="left"/>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test="$gPaymentTypes[@id='Bro']">
          <subtotal sort="2" name="EmptySum" empty-columns="2" />
          <subtotal sort="3" name="EmptySumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='BroClr']">
          <subtotal sort="2" name="TS-BrokerageClrSum" empty-columns="2" />
          <subtotal sort="3" name="TS-BrokerageClrSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='Fee']">
          <subtotal sort="4" name="EmptySum" />
          <subtotal sort="5" name="EmptySumSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='FeeClr']">
          <subtotal sort="4" name="TS-FeeClrSum"/>
          <subtotal sort="5" name="TS-FeeClrSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <subtotal sort="6" name="PA-SCUSum" />
        <subtotal sort="7" name="PA-SCUSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
      </row>
    </group>
  </xsl:variable>
  <!-- ===== Columns of Specific table settings===== -->
  <xsl:variable name="gGroupSpecificDisplaySettings"/>

  <!-- ================================================== -->
  <!--        Main Template                               -->
  <!-- ================================================== -->
  <!-- SetPagesCaracteristicsSpecific template -->
  <xsl:template name="SetPagesCaracteristicsSpecific"/>

  <xsl:template match="/">
    <!-- Le template main est commun à tous les reports, il est dans: Shared/Shared_Report_Main.xslt -->
    <xsl:call-template name="Main" />
  </xsl:template>

  <!-- ================================================== -->
  <!--        Specific Business Template                  -->
  <!-- ================================================== -->
  <!-- BusinessSpecificTradeData -->
  <xsl:template name="BusinessSpecificTradeData">
    <xsl:param name="pBook" />
    <xsl:param name="pCategory" />

    <xsl:attribute name="cat">
      <xsl:value-of select="$pCategory"/>
    </xsl:attribute>

    <xsl:variable name="vTradeId" select="current()/tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']/text()"/>
    <xsl:variable name="vIDB" select="number($gDataDocumentRepository/book[identifier=string(current()/exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Acct)]/@OTCmlId)"/>

    <posActions>
      <xsl:for-each select="$gPosActions[trades/trade/@tradeIdentifier=$vTradeId]">
        <posAction type="{@requestType}" qty="{@qty}" dtSys="{@dtSys}">
          <xsl:call-template name="AddAttributeOptionnal">
            <xsl:with-param name="pName" select="'dtUnclearing'"/>
            <xsl:with-param name="pValue" select="string(@dtUnclearing)"/>
          </xsl:call-template>
          <!--//-->
          <xsl:variable name="vCurrentPosAction" select="."/>
          <xsl:variable name="vCurrentPosActionEvents" select="$gEvents/Event[(IDENTIFIER_TRADE=$vTradeId) and (IDPADET=$vCurrentPosAction/@idPADet)]"/>
          <!--//-->
          <xsl:variable name="vActionEvent">
            <xsl:choose>
              <xsl:when test="@requestType='EXE'">
                <xsl:copy-of select="$vCurrentPosActionEvents[EVENTCODE='EXE' and (EVENTTYPE='TOT' or EVENTTYPE='PAR')]"/>
              </xsl:when>
              <xsl:when test="@requestType='AUTOEXE'">
                <xsl:copy-of select="$vCurrentPosActionEvents[EVENTCODE='AEX' and EVENTTYPE='TOT']"/>
              </xsl:when>
              <xsl:when test="@requestType='ASS'">
                <xsl:copy-of select="$vCurrentPosActionEvents[EVENTCODE='ASS' and (EVENTTYPE='TOT' or EVENTTYPE='PAR')]"/>
              </xsl:when>
              <xsl:when test="@requestType='AUTOASS'">
                <xsl:copy-of select="$vCurrentPosActionEvents[EVENTCODE='AAS' and EVENTTYPE='TOT']"/>
              </xsl:when>
              <!--RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
              <xsl:when test="contains(',ABN,NEX,NAS,',concat(',',@requestType,','))">
                <xsl:copy-of select="$vCurrentPosActionEvents[EVENTCODE='ABN' and (EVENTTYPE='TOT' or EVENTTYPE='PAR')]"/>
              </xsl:when>
              <xsl:when test="@requestType='AUTOABN'">
                <xsl:copy-of select="$vCurrentPosActionEvents[EVENTCODE='AAB' and EVENTTYPE='TOT']"/>
              </xsl:when>
              <xsl:when test="@requestType='MOF'">
                <xsl:call-template name="GetEvent">
                  <xsl:with-param name="pEvents" select="$vCurrentPosActionEvents"/>
                  <xsl:with-param name="pEventCode" select="'LPC'"/>
                  <xsl:with-param name="pEventType" select="'RMG'"/>
                </xsl:call-template>
              </xsl:when>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vUnderlyerPx">
            <xsl:choose>
              <xsl:when test="msxsl:node-set($vActionEvent)/Event/EVENTTYPE='RMG'">
                <xsl:value-of select="number(msxsl:node-set($vActionEvent)/Event/EventDetails/CLOSINGPRICE100)"/>
              </xsl:when>
              <xsl:when test="msxsl:node-set($vActionEvent)/Event[contains(',EXE,AEX,ASS,AAS,ABN,AAB,',concat(',',EVENTCODE,','))]">
                <xsl:value-of select="number(msxsl:node-set($vActionEvent)/Event/EventDetails/SETTLTPRICE100)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="number(msxsl:node-set($vActionEvent)/Event/EventDetails/QUOTEPRICE100)"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <!--//-->
          <xsl:call-template name="AddAttributeOptionnal">
            <xsl:with-param name="pName" select="'underlyerPx'"/>
            <xsl:with-param name="pValue" select="$vUnderlyerPx"/>
          </xsl:call-template>
          <!--//-->

          <xsl:variable name="vConvertedUnderlyerPx">
            <xsl:choose>
              <xsl:when test="msxsl:node-set($vActionEvent)/Event/EVENTTYPE='RMG'">
                <xsl:call-template name="FormatConvertedPriceValue">
                  <xsl:with-param name="pContent">
                    <xsl:copy-of select="msxsl:node-set($vActionEvent)/Event/EventDetails/convertedPrices/convertedClosingPrice"/>
                  </xsl:with-param>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="msxsl:node-set($vActionEvent)/Event[contains(',EXE,AEX,ASS,AAS,ABN,AAB,',concat(',',EVENTCODE,','))]">
                <xsl:call-template name="FormatConvertedPriceValue">
                  <xsl:with-param name="pContent">
                    <xsl:copy-of select="msxsl:node-set($vActionEvent)/Event/EventDetails/convertedPrices/convertedSettltPrice"/>
                  </xsl:with-param>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="FormatConvertedPriceValue">
                  <xsl:with-param name="pContent">
                    <xsl:copy-of select="msxsl:node-set($vActionEvent)/Event/EventDetails/convertedPrices/convertedClearingPrice"/>
                  </xsl:with-param>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <convertedUnderlyerPx>
            <xsl:copy-of select="$vConvertedUnderlyerPx"/>
          </convertedUnderlyerPx>
          <!--//-->
          <xsl:if test="trades/trade2">
            <xsl:variable name="vTrade2" select="//dataDocument/trade[tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']/text()=current()/trades/trade2/@tradeIdentifier]"/>
            <xsl:variable name="vTradeId2" select="string($vTrade2/tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid' and string-length(text())>0 and text()!='0']/text())"/>

            <trade2 href="{$vTradeId2}"
                    book="{string($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Acct)}"
                    lastPx="{number($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx)}"
                    side="{number($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side)}"
                    tradeDate="{string($vTrade2/tradeHeader/tradeDate)}"
                    timeStamp="{substring($vTrade2/tradeHeader/tradeDate/@timeStamp,1,8)}">

              <xsl:call-template name="AddAttributeOptionnal">
                <xsl:with-param name="pName" select="'txt'"/>
                <xsl:with-param name="pValue" select="string($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Txt)"/>
              </xsl:call-template>
              <xsl:call-template name="AddAttributeOptionnal">
                <xsl:with-param name="pName" select="'inptSrc'"/>
                <xsl:with-param name="pValue" select="string($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@InptSrc)"/>
              </xsl:call-template>
              <xsl:call-template name="AddAttributeOptionnal">
                <xsl:with-param name="pName" select="'ordID'"/>
                <xsl:with-param name="pValue" select="string($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@OrdID)"/>
              </xsl:call-template>
              <xsl:call-template name="AddAttributeOptionnal">
                <xsl:with-param name="pName" select="'execID'"/>
                <xsl:with-param name="pValue" select="string($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/@ExecID)"/>
              </xsl:call-template>
              <xsl:call-template name="AddAttributeOptionnal">
                <xsl:with-param name="pName" select="'trdTyp'"/>
                <xsl:with-param name="pValue" select="string($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdTyp)"/>
              </xsl:call-template>
              <!-- 20120712 MF Ticket 18006 -->
              <xsl:call-template name="AddAttributeOptionnal">
                <xsl:with-param name="pName" select="'trdTyp2'"/>
                <xsl:with-param name="pValue" select="string($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdTyp2)"/>
              </xsl:call-template>
              <xsl:call-template name="AddAttributeOptionnal">
                <xsl:with-param name="pName" select="'trdSubTyp'"/>
                <xsl:with-param name="pValue" select="string($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdSubTyp)"/>
              </xsl:call-template>
              <!--//-->
              <!-- 20120821 MF Ticket 18073 -->
              <xsl:variable name="vConvertedLastPx">
                <xsl:call-template name="GetTradeConvertedPriceValue">
                  <xsl:with-param name="pIdAsset" select="number($vTrade2/exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID)"/>
                  <xsl:with-param name="pTradeId" select="string($vTrade2/tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid' and string-length(text())>0 and text()!='0']/text())"/>
                </xsl:call-template>
              </xsl:variable>
              <!-- 20120821 MF Ticket 18073 -->
              <convertedLastPx>
                <xsl:copy-of select="$vConvertedLastPx"/>
              </convertedLastPx>
              <!--//-->
            </trade2>
          </xsl:if>
          <!--//-->
          <!--Dans le cas de la Décompensation, inverser le sens (Payer/Receiver) des Frais et du RMG-->
          <xsl:variable name="vIsReversal" select="string-length(@dtUnclearing)>0"/>
          <!--//-->
          <fees>
            <xsl:variable name="vOPPEvents">
              <xsl:copy-of select="$vCurrentPosActionEvents[EVENTCODE='OPP']"/>
              <xsl:copy-of select="$vCurrentPosActionEvents//Event[EVENTCODE='OPP']"/>
            </xsl:variable>

            <xsl:apply-templates select="$gPaymentTypes" mode="create-fromEvent">
              <xsl:with-param name="pIDB" select="$vIDB"/>
              <xsl:with-param name="pOPPEvents" select="msxsl:node-set($vOPPEvents)/Event"/>
              <xsl:with-param name="pIsReversal" select="$vIsReversal"/>
            </xsl:apply-templates>
          </fees>
          <!--//-->
          <xsl:variable name="vRMGEvent">
            <xsl:call-template name="GetEvent">
              <xsl:with-param name="pEvents" select="$vCurrentPosActionEvents"/>
              <xsl:with-param name="pEventCode" select="'LPC'"/>
              <xsl:with-param name="pEventType" select="'RMG'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:if test="msxsl:node-set($vRMGEvent)/Event">
            <xsl:call-template name="CreateElementForEvent">
              <xsl:with-param name="pEvent" select="msxsl:node-set($vRMGEvent)/Event"/>
              <xsl:with-param name="pElementName" select="'rmg'"/>
              <xsl:with-param name="pIDB" select="$vIDB"/>
              <xsl:with-param name="pIsReversal" select="$vIsReversal"/>
            </xsl:call-template>
          </xsl:if>
          <!--//-->
          <xsl:variable name="vSCUEvent">
            <xsl:call-template name="GetEvent">
              <xsl:with-param name="pEvents" select="$vCurrentPosActionEvents"/>
              <xsl:with-param name="pEventCode" select="'LPC'"/>
              <xsl:with-param name="pEventType" select="'SCU'"/>
            </xsl:call-template>
            <xsl:call-template name="GetEvent">
              <xsl:with-param name="pEvents" select="$vCurrentPosActionEvents"/>
              <xsl:with-param name="pEventCode" select="'TER'"/>
              <xsl:with-param name="pEventType" select="'SCU'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:if test="msxsl:node-set($vSCUEvent)/Event">
            <xsl:call-template name="CreateElementForEvent">
              <xsl:with-param name="pEvent" select="msxsl:node-set($vSCUEvent)/Event"/>
              <xsl:with-param name="pElementName" select="'scu'"/>
              <xsl:with-param name="pIDB" select="$vIDB"/>
            </xsl:call-template>
          </xsl:if>
        </posAction>
      </xsl:for-each>
    </posActions>
  </xsl:template>
  <!-- BusinessSpecificTradeWithStatusData -->
  <xsl:template name="BusinessSpecificTradeWithStatusData"/>
  <!-- SpecificSerieSubTotal template -->
  <xsl:template name="SpecificSerieSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pGroupePaymentCu" />
    <xsl:param name="pCuPrice"/>

    <!--//-->
    <xsl:apply-templates select="$gPaymentTypes" mode="serie-subtotal">
      <xsl:with-param name="pPayments" select="$pSerieTrades//fees/payment"/>
      <xsl:with-param name="pGroupePaymentCu" select="$pGroupePaymentCu"/>
    </xsl:apply-templates>
    <!--//-->
    <xsl:call-template name="GetSCUSubTotal">
      <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
      <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
    </xsl:call-template>
    <!--//-->
    <xsl:call-template name="GetRMGSubTotal">
      <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
      <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
    </xsl:call-template>
  </xsl:template>

  <!--GetUMGSubTotal template-->
  <xsl:template name="GetRMGSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pCuPrice"/>

    <xsl:if test="($pSerieTrades//rmg)">
      <xsl:variable name="vSumRec" select="sum($pSerieTrades//rmg[@pr=$gcRec]/@amount)"/>
      <xsl:variable name="vSumPay" select="sum($pSerieTrades//rmg[@pr=$gcPay]/@amount)"/>
      <xsl:variable name="vRMGTotal" select="number($vSumRec) - number($vSumPay)"/>
      <!--//-->
      <xsl:choose>
        <xsl:when test="number($vRMGTotal) > 0">
          <rmg pr="{$gcRec}" currency="{$pCuPrice}" amount="{$vRMGTotal}"/>
        </xsl:when>
        <xsl:when test="0 > number($vRMGTotal)">
          <rmg pr="{$gcPay}" currency="{$pCuPrice}" amount="{$vRMGTotal * -1}"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
  <!--GetSCUSubTotal template-->
  <xsl:template name="GetSCUSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pCuPrice"/>

    <xsl:if test="($pSerieTrades//scu)">
      <xsl:variable name="vSumRec" select="sum($pSerieTrades//scu[@pr=$gcRec]/@amount)"/>
      <xsl:variable name="vSumPay" select="sum($pSerieTrades//scu[@pr=$gcPay]/@amount)"/>
      <xsl:variable name="vSCUTotal" select="number($vSumRec) - number($vSumPay)"/>
      <!--//-->
      <xsl:choose>
        <xsl:when test="number($vSCUTotal) > 0">
          <scu pr="{$gcRec}" currency="{$pCuPrice}" amount="{$vSCUTotal}"/>
        </xsl:when>
        <xsl:when test="0 > number($vSCUTotal)">
          <scu pr="{$gcPay}" currency="{$pCuPrice}" amount="{$vSCUTotal * -1}"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!--GetEvent template-->
  <xsl:template name="GetEvent">
    <xsl:param name="pEvents"/>
    <xsl:param name="pEventCode"/>
    <xsl:param name="pEventType"/>

    <xsl:for-each select="$pEvents">
      <xsl:if test="EVENTCODE=$pEventCode and EVENTTYPE=$pEventType">
        <xsl:copy-of select="."/>
      </xsl:if>
      <xsl:for-each select=".//Event[EVENTCODE=$pEventCode and EVENTTYPE=$pEventType]">
        <xsl:copy-of select="."/>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <!--SpecificGroupSubTotal template-->
  <!--Pas de sous totaux pour les groupes sur les avis d'opéré-->
  <xsl:template name="SpecificGroupSubTotal"/>

  <!-- ===== CreateSpecificGroups ===== -->
  <xsl:template name="CreateSpecificGroups"/>

  <!-- ===== CreateElementForEvent ===== -->
  <xsl:template name="CreateElementForEvent">
    <xsl:param name="pEvent"/>
    <xsl:param name="pElementName"/>
    <xsl:param name="pIDB"/>
    <xsl:param name="pIsReversal" select="false()"/>

    <xsl:variable name="vPayRecEvent">
      <xsl:choose>
        <xsl:when test="$pEvent/IDB_PAY = $pIDB">
          <xsl:value-of select="$gcPay"/>
        </xsl:when>
        <xsl:when test="$pEvent/IDB_REC = $pIDB">
          <xsl:value-of select="$gcRec"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vPayRec">
      <xsl:choose>
        <xsl:when test="$pIsReversal = false()">
          <xsl:value-of select="$vPayRecEvent"/>
        </xsl:when>
        <xsl:when test="$pIsReversal = true()">
          <xsl:choose>
            <xsl:when test="$vPayRecEvent = $gcPay">
              <xsl:value-of select="$gcRec"/>
            </xsl:when>
            <xsl:when test="$vPayRecEvent = $gcRec">
              <xsl:value-of select="$gcPay"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!--//-->
    <xsl:element name="{$pElementName}">
      <xsl:attribute name="pr">
        <xsl:value-of select="$vPayRec"/>
      </xsl:attribute>
      <xsl:attribute name="currency">
        <xsl:value-of select="$pEvent/UNIT"/>
      </xsl:attribute>
      <xsl:attribute name="amount">
        <xsl:value-of select="$pEvent/VALORISATION"/>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>

  <!--TradeCashBalanceTax template-->
  <xsl:template name="TradeCashBalanceTax">
    <xsl:param name="pBookTrades"/>
    <xsl:param name="pPaymentType"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pCurrencyFxRate"/>
    <xsl:param name="pRoundDir"/>
    <xsl:param name="pRoundPrec"/>
    <xsl:param name="pAllTaxDetailAmount"/>
    <xsl:param name="pAllTaxRateAmount"/>

    <xsl:call-template name="TradeCashBalanceTaxCommon">
      <xsl:with-param name="pBookTrades" select="$pBookTrades"/>
      <xsl:with-param name="pPaymentType" select="$pPaymentType"/>
      <xsl:with-param name="pCurrency" select="$pCurrency"/>
      <xsl:with-param name="pCurrencyFxRate" select="$pCurrencyFxRate"/>
      <xsl:with-param name="pRoundDir" select="$pRoundDir"/>
      <xsl:with-param name="pRoundPrec" select="$pRoundPrec"/>
      <xsl:with-param name="pAllTaxDetailAmount" select="$pAllTaxDetailAmount"/>
      <xsl:with-param name="pAllTaxRateAmount" select="$pAllTaxRateAmount"/>
    </xsl:call-template>

  </xsl:template>

  <!-- ================================================== -->
  <!--        Specific Display Template                   -->
  <!-- ================================================== -->
  <!--Affichage du trade dans le Header du report-->
  <xsl:template name="GetTradeHeader"/>
  <!-- DisplaySummaryPageBody template -->
  <xsl:template name="DisplaySummaryPageBody"/>
  <!--DisplayPageBodyStart template-->
  <xsl:template name="DisplayPageBodyStart">
    <xsl:param name="pBook" />

    <xsl:call-template name="DisplayPageBodyStartCommon">
      <xsl:with-param name="pBook" select="$pBook"/>
    </xsl:call-template>

  </xsl:template>
  <!--DisplayReceiverDetailSpecific template-->
  <xsl:template name="DisplayReceiverDetailSpecific">
    <xsl:param name="pBook" />
    <xsl:call-template name="DisplayReceiverDetailCommon">
      <xsl:with-param name="pBook" select="$pBook" />
    </xsl:call-template>
  </xsl:template>

  <!-- ===== Specific Trade display ===== -->
  <xsl:template match="trade" mode="display-specific">
    <xsl:param name="pCurrentTradeWithStatus" />
    <xsl:param name="pGroupColumnsToDisplay" />

    <xsl:variable name="vCurrentTrade" select="." />
    <xsl:variable name="vCurrentTradeWithStatus" select="$pCurrentTradeWithStatus[@href=current()/@href]" />
    <xsl:variable name="vIsFirst" select="boolean(position() = 1)"/>
    <xsl:variable name="vIsLast" select="boolean(position() = last())"/>

    <xsl:for-each select="$vCurrentTrade/posActions/posAction">
      <xsl:sort select="substring(@dtSys , 1, 4)" data-type="number"/>
      <xsl:sort select="substring(@dtSys , 6, 2)" data-type="number"/>
      <xsl:sort select="substring(@dtSys , 9, 2)" data-type="number"/>
      <xsl:sort select="substring(@dtSys , 12, 2)" data-type="number"/>
      <xsl:sort select="substring(@dtSys , 15, 2)" data-type="number"/>
      <xsl:sort select="substring(@dtSys , 18, 2)" data-type="number"/>

      <xsl:variable name="vCurrentPosActionTrade">
        <trade>
          <xsl:copy-of select="$vCurrentTrade/@*"/>
          <xsl:copy-of select="$vCurrentTrade/*[local-name()!='posActions']"/>
          <posActions>
            <xsl:copy-of select="."/>
          </posActions>
        </trade>
      </xsl:variable>

      <xsl:call-template name="Trade-display">
        <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay" />
        <xsl:with-param name="pTrade" select="msxsl:node-set($vCurrentPosActionTrade)/trade" />
        <xsl:with-param name="pTradeWithStatus" select="$vCurrentTradeWithStatus" />
        <xsl:with-param name="pIsFirstTrade" select="$vIsFirst and boolean(position() = 1)" />
        <xsl:with-param name="pIsLastTrade" select="$vIsLast and boolean(position() = last())" />
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <!-- ===== Mapping between columns to display and Data to display for each column ===== -->
  <!-- GetColumnDataToDisplay -->
  <xsl:template name="GetColumnDataToDisplay">
    <xsl:param name="pRowData" />
    <xsl:param name="pTradeWithStatus" />
    <xsl:param name="pIsGetDetail" select="false()" />

    <xsl:variable name="vColumnName" select="@name"/>

    <xsl:choose>
      <!-- ===== ActType ===== -->
      <xsl:when test="$vColumnName='PA-ActType'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <!-- POC : Position correction -->
              <xsl:when test="$pRowData/posActions/posAction/@type='POC'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Correct'" />
                </xsl:call-template>
              </xsl:when>
              <!-- MOF : Liquidation de future -->
              <xsl:when test="$pRowData/posActions/posAction/@type='MOF'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Liquidation'" />
                </xsl:call-template>
              </xsl:when>
              <!-- POT : Position transfert -->
              <xsl:when test="$pRowData/posActions/posAction/@type='POT'">
                <xsl:choose>
                  <!-- transfert = PortFolioTransfer(42) and InternalTransferOrAdjustment(1) -->
                  <xsl:when test="$pRowData/posActions/posAction/trade2/@trdTyp='42' and 
                            $pRowData/posActions/posAction/trade2/@trdSubTyp='1'">
                    <xsl:call-template name="getSpheresTranslation">
                      <xsl:with-param name="pResourceName" select="'TS-Transf'" />
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="getSpheresTranslation">
                      <xsl:with-param name="pResourceName" select="'TS-Correct'" />
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <!-- Décompensation -->
              <xsl:when test="string-length($pRowData/posActions/posAction/@dtUnclearing)>0">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Unclearing'" />
                </xsl:call-template>
              </xsl:when>
              <!-- UNCLEARING: Compensation générée suite à une décompensation partielle -->
              <xsl:when test="$pRowData/posActions/posAction/@type='UNCLEARING'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Clear'" />
                </xsl:call-template>
              </xsl:when>
              <!-- CLEARSPEC : Compensation  Spécifique -->
              <xsl:when test="$pRowData/posActions/posAction/@type='CLEARSPEC'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Clear'" />
                </xsl:call-template>
              </xsl:when>
              <!-- CLEARBULK : Compensation globale -->
              <xsl:when test="$pRowData/posActions/posAction/@type='CLEARBULK'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Clear'" />
                </xsl:call-template>
              </xsl:when>
              <!-- CLEAREOD : Compensation fin de journée -->
              <xsl:when test="$pRowData/posActions/posAction/@type='CLEAREOD'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Clear'" />
                </xsl:call-template>
              </xsl:when>
              <!-- ENTRY : Clôture STP -->
              <xsl:when test="$pRowData/posActions/posAction/@type='ENTRY'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Closing'" />
                </xsl:call-template>
              </xsl:when>
              <!-- UPDENTRY : Clôture Fin de journée -->
              <xsl:when test="$pRowData/posActions/posAction/@type='UPDENTRY'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Closing'" />
                </xsl:call-template>
              </xsl:when>
              <!-- RD 20150519 [21034] Gérer les corporate action-->
              <!-- CORPOACTION : Corporate Action -->
              <xsl:when test="$pRowData/posActions/posAction/@type='CORPOACTION'">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-CorpoAction'" />
                </xsl:call-template>
              </xsl:when>
              <!-- ABN, NEX, NAS, AUTOABN, EXE, AUTOEXE, ASS et AUTOASS -->
              <xsl:otherwise>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="concat('TS-',$pRowData/posActions/posAction/@type)" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== ActTypeDet ===== -->
      <xsl:when test="$vColumnName='PA-ActTypeDet'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <!-- Book transfer = PortFolioTransfer(42) and InternalTransferOrAdjustment(1) -->
              <xsl:when test="$pRowData/posActions/posAction/@type='POT' and 
                    $pRowData/posActions/posAction/trade2/@trdTyp='42' and 
                    $pRowData/posActions/posAction/trade2/@trdSubTyp='1'">
                <xsl:value-of select="concat('->',$pRowData/posActions/posAction/trade2/@book)"/>
              </xsl:when>
              <!-- Dénoument d'option (Auto) -->
              <xsl:when test="$pRowData/posActions/posAction/@type='AUTOEXE' or
                          $pRowData/posActions/posAction/@type='AUTOABN' or
                          $pRowData/posActions/posAction/@type='AUTOASS'">
                <xsl:variable name="vActTypeDet">
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="'TS-Auto'" />
                  </xsl:call-template>
                </xsl:variable>

                <xsl:if test="string-length($vActTypeDet) > 0">
                  <xsl:value-of select="concat('(',$vActTypeDet,')')"/>
                </xsl:if>
              </xsl:when>
              <!-- Décompensation (Clôture,Compensation) -->
              <xsl:when test="string-length($pRowData/posActions/posAction/@dtUnclearing)>0">
                <xsl:choose>
                  <!-- ENTRY      : Clôture STP -->
                  <!-- UPDENTRY   : Clôture Fin de journée -->
                  <xsl:when test="$pRowData/posActions/posAction/@type='ENTRY' or
                              $pRowData/posActions/posAction/@type='UPDENTRY'">
                    <xsl:variable name="vActTypeDet">
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'TS-Closing'" />
                      </xsl:call-template>
                    </xsl:variable>

                    <xsl:if test="string-length($vActTypeDet) > 0">
                      <xsl:value-of select="concat('(',$vActTypeDet,')')"/>
                    </xsl:if>
                  </xsl:when>
                  <!-- CLEARSPEC  : Compensation Spécifique -->
                  <!-- CLEARBULK  : Compensation Globale -->
                  <!-- CLEAREOD   : Compensation Fin de journée -->
                  <!-- UNCLEARING : Compensation restante suite à décompensation partielle -->
                  <xsl:when test="$pRowData/posActions/posAction/@type='CLEARSPEC' or
                              $pRowData/posActions/posAction/@type='CLEARBULK' or
                              $pRowData/posActions/posAction/@type='CLEAREOD' or
                              $pRowData/posActions/posAction/@type='UNCLEARING'">
                    <xsl:variable name="vActTypeDet">
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'TS-Clear'" />
                      </xsl:call-template>
                    </xsl:variable>

                    <xsl:if test="string-length($vActTypeDet) > 0">
                      <xsl:value-of select="concat('(',$vActTypeDet,')')"/>
                    </xsl:if>
                  </xsl:when>
                </xsl:choose>
              </xsl:when>
              <!-- CLEARSPEC : Compensation  Spécifique -->
              <xsl:when test="$pRowData/posActions/posAction/@type='CLEARSPEC'">
                <xsl:variable name="vActTypeDet">
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="'TS-Specific'" />
                  </xsl:call-template>
                </xsl:variable>

                <xsl:if test="string-length($vActTypeDet) > 0">
                  <xsl:value-of select="concat('(',$vActTypeDet,')')"/>
                </xsl:if>
              </xsl:when>
              <xsl:otherwise/>
            </xsl:choose>
            <!-- Dénoument d'option (In, Out) -->
            <xsl:choose>
              <xsl:when test="$pRowData/posActions/posAction/@type='EXE'">
                <xsl:variable name="vActTypeDet">
                  <xsl:choose>
                    <xsl:when test="$pRowData/putCall/@data='0' and 
                          number($pRowData/posActions/posAction/@underlyerPx) > number($pRowData/strkPx/@data)">
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'TS-Out'" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$pRowData/putCall/@data='1' and 
                          number($pRowData/strkPx/@data) > number($pRowData/posActions/posAction/@underlyerPx)">
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'TS-Out'" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise/>
                  </xsl:choose>
                </xsl:variable>

                <xsl:if test="string-length($vActTypeDet) > 0">
                  <xsl:value-of select="concat('(',$vActTypeDet,')')"/>
                </xsl:if>
              </xsl:when>
              <!-- RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
              <xsl:when test="contains(',ABN,NEX,NAT,',concat(',',$pRowData/posActions/posAction/@type,','))">
                <xsl:variable name="vActTypeDet">
                  <xsl:choose>
                    <xsl:when test="$pRowData/putCall/@data='0' and 
                              number($pRowData/strkPx/@data) > number($pRowData/posActions/posAction/@underlyerPx)">
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'TS-In'" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$pRowData/putCall/@data='1' and 
                          number($pRowData/posActions/posAction/@underlyerPx) > number($pRowData/strkPx/@data)">
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'TS-In'" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise/>
                  </xsl:choose>
                </xsl:variable>

                <xsl:if test="string-length($vActTypeDet) > 0">
                  <xsl:value-of select="concat('(',$vActTypeDet,')')"/>
                </xsl:if>
              </xsl:when>
              <xsl:when test="$pRowData/posActions/posAction/@type='ASS'">
                <xsl:variable name="vActTypeDet">
                  <xsl:choose>
                    <xsl:when test="$pRowData/putCall/@data='0' and  
                          number($pRowData/posActions/posAction/@underlyerPx) > number($pRowData/strkPx/@data)">
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'TS-Out'" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$pRowData/putCall/@data='1' and
                              number($pRowData/strkPx/@data) > number($pRowData/posActions/posAction/@underlyerPx)">
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'TS-Out'" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise/>
                  </xsl:choose>
                </xsl:variable>

                <xsl:if test="string-length($vActTypeDet) > 0">
                  <xsl:value-of select="concat('(',$vActTypeDet,')')"/>
                </xsl:if>
              </xsl:when>
              <xsl:otherwise/>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Qty ===== -->
      <xsl:when test="$vColumnName='PA-Qty'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:value-of select="$pRowData/posActions/posAction/@qty"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Side ===== -->
      <xsl:when test="$vColumnName='PA-TradeSide'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="concat('PA-TradeSide',$pRowData/@side)" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeDate ===== -->
      <xsl:when test="$vColumnName='TS-TradeDate'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="format-shortdate_ddMMMyy">
              <xsl:with-param name="year" select="substring($pRowData/@tradeDate , 1, 4)" />
              <xsl:with-param name="month" select="substring($pRowData/@tradeDate , 6, 2)" />
              <xsl:with-param name="day" select="substring($pRowData/@tradeDate , 9, 2)" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== ImpactedSide ===== -->
      <xsl:when test="$vColumnName='PA-TradeImpactedSide'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="$pRowData/posActions/posAction/trade2">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="concat('PA-TradeSide',$pRowData/posActions/posAction/trade2/@side)" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeImpactedDate ===== -->
      <xsl:when test="$vColumnName='PA-TradeImpactedDate'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="$pRowData/posActions/posAction/trade2">
              <xsl:call-template name="format-shortdate_ddMMMyy">
                <xsl:with-param name="year" select="substring($pRowData/posActions/posAction/trade2/@tradeDate , 1, 4)" />
                <xsl:with-param name="month" select="substring($pRowData/posActions/posAction/trade2/@tradeDate , 6, 2)" />
                <xsl:with-param name="day" select="substring($pRowData/posActions/posAction/trade2/@tradeDate , 9, 2)" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeImpactedTime ===== -->
      <xsl:when test="$vColumnName='PA-TradeImpactedTime'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="$pRowData/posActions/posAction/trade2">
              <xsl:call-template name="format-time2">
                <xsl:with-param name="xsd-date-time" select="$pRowData/posActions/posAction/trade2/@timeStamp" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeImpactedNum ===== -->
      <xsl:when test="$vColumnName='PA-TradeImpactedNum'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="$pRowData/posActions/posAction/trade2">
              <xsl:call-template name="GetTradeNumDataToDisplay">
                <xsl:with-param name="pExecID" select="$pRowData/posActions/posAction/trade2/@execID"/>
                <xsl:with-param name="pHref" select="$pRowData/posActions/posAction/trade2/@href"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeImpactedPriceType ===== -->
      <xsl:when test="$vColumnName='PA-TradeImpactedPriceType'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <!-- MOF : Prix de référence -->
              <xsl:when test="$pRowData/posActions/posAction/@type='MOF'">
                <xsl:value-of select="'(1)'"/>
              </xsl:when>
              <!-- Dénoument d'option : Prix du sous-jacent-->
              <!-- RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
              <xsl:when test="contains(',EXE,AUTOEXE,ABN,NEX,NAS,AUTOABN,ASS,AUTOASS,',concat(',',$pRowData/posActions/posAction/@type,','))">
                <xsl:value-of select="'(2)'"/>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeImpactedPrice ===== -->
      <xsl:when test="$vColumnName='PA-TradeImpactedPrice'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <!-- MOF : Prix de référence -->
              <xsl:when test="$pRowData/posActions/posAction/@type='MOF'">
                <xsl:if test="string-length($pRowData/posActions/posAction/@underlyerPx)>0">
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="concat($pRowData/posActions/posAction/@underlyerPx,'')" />
                    <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:when>
              <!-- Dénoument d'option : Prix du sous-jacent-->
              <!-- RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
              <xsl:when test="contains(',EXE,AUTOEXE,ABN,NEX,NAS,AUTOABN,ASS,AUTOASS,',concat(',',$pRowData/posActions/posAction/@type,','))">
                <xsl:if test="string-length($pRowData/posActions/posAction/@underlyerPx)>0">
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="concat($pRowData/posActions/posAction/@underlyerPx,'')" />
                    <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:when>
              <xsl:when test="$pRowData/posActions/posAction/trade2">
                <xsl:call-template name="format-number">
                  <xsl:with-param name="pAmount" select="concat($pRowData/posActions/posAction/trade2/@lastPx,'')" />
                  <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
                </xsl:call-template>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- 20120821 MF Ticket 18073-->
      <!-- ===== TradeImpactedPrice Converted ===== -->
      <xsl:when test="$vColumnName='PA-ConvertedTradeImpactedPrice'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <!-- RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
              <xsl:when test="contains(',MOF,EXE,AUTOEXE,ABN,NEX,NAS,AUTOABN,ASS,AUTOASS,',concat(',',$pRowData/posActions/posAction/@type,','))">
                <xsl:if test="string-length($pRowData/posActions/posAction/@underlyerPx)>0">
                  <xsl:copy-of select="$pRowData/posActions/posAction/convertedUnderlyerPx"/>
                </xsl:if>
              </xsl:when>
              <xsl:when test="$pRowData/posActions/posAction/trade2">
                <xsl:copy-of select="$pRowData/posActions/posAction/trade2/convertedLastPx"/>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== RMG ===== -->
      <xsl:when test="$vColumnName='PA-RMG'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/posActions/posAction/rmg/@amount) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData/posActions/posAction/rmg/@amount" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== RMG DrCr ===== -->
      <xsl:when test="$vColumnName='PA-RMGDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/posActions/posAction/rmg/@amount) > 0 and 
                    number($pRowData/posActions/posAction/rmg/@amount) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData/posActions/posAction/rmg/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CASHSTL ===== -->
      <xsl:when test="$vColumnName='PA-CASHSTL'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/posActions/posAction/scu/@amount) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData/posActions/posAction/scu/@amount" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CASHSTL DrCr ===== -->
      <xsl:when test="$vColumnName='PA-CASHSTLDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/posActions/posAction/scu/@amount) > 0 and 
                    number($pRowData/posActions/posAction/scu/@amount) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData/posActions/posAction/scu/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Common column ===== -->
      <xsl:otherwise>
        <xsl:call-template name="GetCommonColumnDataToDisplay">
          <xsl:with-param name="pColumnName" select="$vColumnName" />
          <xsl:with-param name="pRowData" select="$pRowData" />
          <xsl:with-param name="pTradeWithStatus" select="$pTradeWithStatus" />
          <xsl:with-param name="pIsGetDetail" select="$pIsGetDetail" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- GetColumnIsAltData-->
  <xsl:template name="GetColumnIsAltData">
    <xsl:param name="pColumnName" />
    <xsl:param name="pRowData" />
    <xsl:param name="pIsGetDetail" select="false()" />

    <xsl:variable name="vColumnName">
      <xsl:choose>
        <xsl:when test="string-length($pColumnName)>0">
          <xsl:value-of select="$pColumnName"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@name"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="GetCommonColumnIsAltData">
      <xsl:with-param name="pColumnName" select="$vColumnName" />
      <xsl:with-param name="pRowData" select="$pRowData" />
      <xsl:with-param name="pIsGetDetail" select="$pIsGetDetail" />
    </xsl:call-template>
  </xsl:template>
  <!-- GetSpecificDataFontWeight -->
  <xsl:template name="GetSpecificDataFontWeight">
    <xsl:param name="pColumnName" />
    <xsl:param name="pRowData" />
    <xsl:param name="pTradeWithStatus" />
    <xsl:param name="pIsGetDetail" select="false()" />

    <xsl:variable name="vColumnName">
      <xsl:choose>
        <xsl:when test="string-length($pColumnName)>0">
          <xsl:value-of select="$pColumnName"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@name"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <!-- ===== ActType ===== -->
      <xsl:when test="$vColumnName='PA-ActType'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <!-- POT : Position transfert -->
              <xsl:when test="$pRowData/posActions/posAction/@type='POT'">
                <xsl:value-of select="'bold'"/>
              </xsl:when>
              <!-- POC : Position correction -->
              <xsl:when test="$pRowData/posActions/posAction/@type='POC'">
                <xsl:value-of select="'bold'"/>
              </xsl:when>
              <!-- Décompensation -->
              <xsl:when test="string-length($pRowData/posActions/posAction/@dtUnclearing)>0">
                <xsl:value-of select="'bold'"/>
              </xsl:when>
              <xsl:otherwise/>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>
  </xsl:template>

  <!--GetSpecificColumnDynamicData-->
  <!--<xsl:template name="GetSpecificColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:value-of select="$pValue"/>
  </xsl:template>-->

  <!--GetSerieSubTotalLabel-->
  <xsl:template name="GetSerieSubTotalLabel">
    <xsl:param name="pSerie"/>
    <xsl:param name="pSubTotalName"/>

    <xsl:choose>
      <!-- ===== RMGSum ===== -->
      <xsl:when test="$pSubTotalName = 'PA-RMGSum'"/>
      <!-- ===== RMGSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'PA-RMGSumDrCr'"/>
      <!-- ===== SCUSum ===== -->
      <xsl:when test="$pSubTotalName = 'PA-SCUSum'"/>
      <!-- ===== SCUSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'PA-SCUSumDrCr'"/>
      <!-- ===== Common ===== -->
      <xsl:otherwise>
        <xsl:call-template name="GetCommonSerieSubTotalLabel">
          <xsl:with-param name="pSerie" select="$pSerie" />
          <xsl:with-param name="pSubTotalName" select="$pSubTotalName" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <!--GetSerieSubTotalData-->
  <xsl:template name="GetSerieSubTotalData">
    <xsl:param name="pSerie"/>
    <xsl:param name="pSubTotalName"/>

    <xsl:choose>
      <!-- ===== RMGSum ===== -->
      <xsl:when test="$pSubTotalName = 'PA-RMGSum'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/rmg/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/rmg/@amount) > 0">
            <!--<xsl:value-of select="format-number($pSerie/sum/rmg/@amount, $amountPattern, $defaultCulture)"/>-->
            <xsl:call-template name="format-money2">
              <xsl:with-param name="amount" select="$pSerie/sum/rmg/@amount" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== RMGSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'PA-RMGSumDrCr'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/rmg/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/rmg/@amount) > 0 and number($pSerie/sum/rmg/@amount) > 0">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pAmountSens" select="$pSerie/sum/rmg/@pr" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== SCUSum ===== -->
      <xsl:when test="$pSubTotalName = 'PA-SCUSum'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/scu/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/scu/@amount) > 0">
            <xsl:call-template name="format-money2">
              <xsl:with-param name="amount" select="$pSerie/sum/scu/@amount" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== SCUSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'PA-SCUSumDrCr'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/rmg/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/scu/@amount) > 0 and number($pSerie/sum/scu/@amount) > 0">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pAmountSens" select="$pSerie/sum/scu/@pr" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Common ===== -->
      <xsl:otherwise>
        <xsl:call-template name="GetCommonSerieSubTotalData">
          <xsl:with-param name="pSerie" select="$pSerie" />
          <xsl:with-param name="pSubTotalName" select="$pSubTotalName" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
