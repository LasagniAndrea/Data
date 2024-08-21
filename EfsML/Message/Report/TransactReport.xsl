<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!--  
================================================================================================================
                                                     HISTORY OF THE MODIFICATIONS
Version 1.0.0.0 (Spheres 2.6.0.0)
 Date: 11/03/2011
 Author: RD
 Description: first version  
 
Version 1.0.1.0 (Spheres 2.6.4.0)
 Date: 22/08/2012 [Ticket 18073]
 Author: MF
 Description: Add converted prices to the transaction report. the strike price and the transaction price 
 (including the average price value insiode of convertedShort node and convertedLong node, childs of the sum node) 
 will be displayed using the derivative contract specific numerical base and the given style.  
 The converted values will be displayed for trade alloc and strategy issued by alloc.
 
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
  <xsl:variable name="gDataDocumentTrades" select="//dataDocument/trade"/>
  <xsl:variable name="gReportType" select="$gcReportTypeALLOC"/>
  <!--Legend-->
  <xsl:variable name="gIsReportWithSpecificLegend" select="false()"/>
  <xsl:variable name="gIsReportWithOrderTypeLegend" select="true()"/>
  <xsl:variable name="gIsReportWithTradeTypeLegend" select="true()"/>
  <xsl:variable name="gIsReportWithPriceTypeLegend" select="false()"/>

  <!-- ================================================== -->
  <!--        Display Settings                            -->
  <!-- ================================================== -->

  <!-- ===== Display settings of all Blocks===== -->
  <xsl:variable name="gDetTrdTableTotalWidth">
    <xsl:choose>
      <!-- EG 20160404 Migration vs2013 -->
      <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
        <xsl:value-of select="'175mm'"/>
      </xsl:when>
      <!-- EG 20160404 Migration vs2013 -->
      <xsl:otherwise>
        <xsl:value-of select="'197mm'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gGroupDisplaySettings">
    <!-- EG 20160404 Migration vs2013 -->
    <group sort="1" name="ETDBlock" padding-top="2mm">
      <!--Title-->
      <title empty-row="0.1cm" withColumnHdr="{true()}" withSubColumn="{true()}" font-size="7">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <column number="01" name="TS-MarketTitle" colspan="3">
              <header ressource="TS-MARKET" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
          </xsl:when>
          <xsl:otherwise>
            <column number="01" name="TS-MarketTitle" colspan="2">
              <header ressource="TS-MARKET" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
          </xsl:otherwise>
        </xsl:choose>

        <column number="02" name="TS-DCTitle" colspan="6">
          <header ressource="TS-DERIVATIVECONTRACT" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="03" name="TS-DCUNLTitle" colspan="8">
          <header ressource="TS-DERIVATIVECONTRACTUNL" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
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
        <column number="01" name="TS-Book" width="22mm" >
          <header rowspan="2"/>
          <data rowspan="2" length="14"/>
        </column>
      </xsl:if>

      <column number="{number('2') - number($vColNumberBase)}" name="TS-OrderNum" width="19mm">
        <header ressource="ALLOC-OrderNum" master-ressource="ALLOC-OrderHdr" master-ressource-colspan="2"/>
        <data text-align="center" length="11"/>
      </column>
      <column number="{number('2') - number($vColNumberBase)}" name="TS-Stgy" master-column="TS-OrderNum" width="19mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}" font-size-alt="{$gDetTrdFontSize}" length="11"/>
      </column>
      <column number="{number('3') - number($vColNumberBase)}" name="ALLOC-OrderType" width="7mm">
        <header master-ressource="ALLOC-OrderHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="center"/>
      </column>
      <column number="{number('4') - number($vColNumberBase)}" name="ALLOC-ExecBroker" width="11mm">
        <header rowspan="2"/>
        <data rowspan="2" text-align="center" length="7"/>
      </column>
      <column number="{number('5') - number($vColNumberBase)}" name="TS-TradeDate" width="14mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="3"/>
        <data text-align="center"/>
      </column>
      <column number="{number('5') - number($vColNumberBase)}" name="TS-TradeTime" master-column="TS-TradeDate" width="14mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('6') - number($vColNumberBase)}" name="TS-TradeNum" width="18mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="0"/>
        <data text-align="center" length="12"/>
      </column>
      <column number="{number('6') - number($vColNumberBase)}" name="TS-StgyLeg" master-column="TS-TradeNum" width="18mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}" length="12"/>
      </column>
      <column number="{number('7') - number($vColNumberBase)}" name="TS-TradeType" width="7mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="0"/>
        <data text-align="center"/>
      </column>
      <column number="{number('7') - number($vColNumberBase)}" name="TS-SecondaryTradeType" master-column="TS-TradeType" width="7mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('8') - number($vColNumberBase)}" name="TS-QtyBuy" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="2"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="{number('9') - number($vColNumberBase)}" name="TS-QtySell" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="{number('10') - number($vColNumberBase)}" name="TS-Maturity" colspan="2" width="23mm">
        <data text-align="center"/>
      </column>
      <column number="{number('10') - number($vColNumberBase)}" name="TS-PutCall" master-column="TS-Maturity" width="5.5mm"/>
      <column number="{number('11') - number($vColNumberBase)}" name="TS-ConvertedStrike" master-column="TS-Maturity" width="17.5mm">
        <header text-align="right"/>
        <data text-align="right" typenode="true"/>
      </column>
      <column number="{number('12') - number($vColNumberBase)}" name="TS-ConvertedPrice" colspan="2" width="18mm">
        <data text-align="right" typenode="true"/>
      </column>
      <column number="{number('12') - number($vColNumberBase)}" name="TS-Premium" master-column="TS-ConvertedPrice" width="14.5mm">
        <header colspan="2"/>
        <data text-align="right"/>
      </column>
      <column number="{number('13') - number($vColNumberBase)}" name="TS-PremiumDrCr" master-column="TS-ConvertedPrice" width="3.5mm">
        <header colspan="0"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <!-- 20120822 Ticket 18073 END -->
      <column number="{number('14') - number($vColNumberBase)}" name="ALLOC-PosEffect" width="9mm">
        <header rowspan="2"/>
        <data rowspan="2" text-align="center"/>
      </column>
      <!--Brokerage: 1st row-->
      <xsl:choose>
        <xsl:when test="$gPaymentTypes[@id='Bro']">
          <column number="{number('15') - number($vColNumberBase)}" name="TS-Brokerage" width="12mm">
            <header colspan="2"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="TS-BrokerageDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gPaymentTypes[@id='BroTrd'] and $gPaymentTypes[@id='BroClr']">
          <column number="{number('15') - number($vColNumberBase)}" name="TS-BrokerageTrd" width="12mm">
            <header colspan="2" ressource="TS-Trd" ressource1="TS-Brokerage"/>
            <data text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="TS-BrokerageTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:otherwise>
          <column number="{number('15') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="EmptyColumn" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:otherwise>
      </xsl:choose>
      <!--Brokerage: 2nd row-->
      <xsl:choose>
        <xsl:when test="$gPaymentTypes[@id='Bro']">
          <column number="{number('16') - number($vColNumberBase)}" name="EmptyColumn" master-column="TS-BrokerageDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gPaymentTypes[@id='BroTrd'] and $gPaymentTypes[@id='BroClr']">
          <column number="{number('15') - number($vColNumberBase)}" name="TS-BrokerageClr" master-column="TS-BrokerageTrd" width="12mm">
            <header colspan="2" ressource="TS-Clr"/>
            <data text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="TS-BrokerageClrDrCr" master-column="TS-BrokerageTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:otherwise>
          <column number="{number('15') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-right-style="none"/>
          </column>
          <column number="{number('16') - number($vColNumberBase)}" name="EmptyColumn" width="3.5mm">
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
          <column number="{number('18') - number($vColNumberBase)}" name="TS-FeeDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gPaymentTypes[@id='FeeTrd'] and $gPaymentTypes[@id='FeeClr']">
          <column number="{number('17') - number($vColNumberBase)}" name="TS-FeeTrd" border-right-style="none" width="12mm">
            <header colspan="2" ressource="TS-Trd" ressource1="TS-Fee"/>
            <data text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="TS-FeeTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:otherwise>
          <column number="{number('17') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:otherwise>
      </xsl:choose>
      <!--Fee: 2nd row-->
      <xsl:choose>
        <xsl:when test="$gPaymentTypes[@id='Fee']">
          <column number="{number('18') - number($vColNumberBase)}" name="EmptyColumn" master-column="TS-FeeDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gPaymentTypes[@id='FeeTrd'] and $gPaymentTypes[@id='FeeClr']">
          <column number="{number('17') - number($vColNumberBase)}" name="TS-FeeClr" master-column="TS-FeeTrd" width="12mm">
            <header colspan="2" ressource="TS-Clr"/>
            <data text-align="right" border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="TS-FeeClrDrCr" master-column="TS-FeeTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:otherwise>
          <column number="{number('17') - number($vColNumberBase)}" name="EmptyColumn" width="12mm">
            <header colspan="0"/>
            <data border-right-style="none"/>
          </column>
          <column number="{number('18') - number($vColNumberBase)}" name="EmptyColumn" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:otherwise>
      </xsl:choose>
    </group>
    <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryStrategies/text() = 'Bottom'">
      <group sort="2" name="ETDRecapStgy" padding-top="{$gGroupPaddingTop}">

        <!--Titles-->
        <title empty-row="0.1cm" withColumnHdr="{false()}" withSubColumn="{true()}" font-size="7" font-weight="bold"
               background-color="{$gDetTrdHdrCellBackgroundColor}">
          <column number="01" name="TS-RecapStgy" colspan="14">
            <data border-bottom-style="solid" value="%%RES:TS-RecapStgy%%"/>
          </column>
        </title>

        <!--Columns-->
        <column number="1" name="TS-Book" width="15mm" >
          <header rowspan="2"/>
          <data isonlyfirstrow="{true()}" rowspan="2" length="9"/>
        </column>
        <column number="2" name="TS-OrderNum" width="19mm">
          <header rowspan="2"/>
          <data isonlyfirstrow="{true()}" rowspan="2" text-align="center" length="11"/>
        </column>
        <column number="3" name="TS-StgyType" width="15mm">
          <header rowspan="2"/>
          <data isonlyfirstrow="{true()}" rowspan="2" text-align="center"/>
        </column>
        <column number="4" name="TS-TradeDate" width="16mm">
          <header master-ressource="TS-TradeHdr" master-ressource-colspan="2"/>
          <data text-align="center"/>
        </column>
        <column number="4" name="TS-TradeTime" master-column="TS-TradeDate" width="16mm">
          <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
        </column>
        <column number="5" name="TS-TradeNum" width="18mm">
          <header master-ressource="TS-TradeHdr" master-ressource-colspan="0"/>
          <data text-align="center"/>
        </column>
        <column number="5" name="TS-StgyLeg" master-column="TS-TradeNum" width="18mm">
          <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
        </column>
        <column number="6" name="TS-MARKET" width="20mm" >
          <header rowspan="2"/>
          <data rowspan="2" length="12" text-align="center"/>
        </column>
        <column number="7" name="TS-DERIVATIVECONTRACT" width="29mm" >
          <header rowspan="2"/>
          <data rowspan="2" length="24" text-align="center"/>
        </column>
        <column number="8" name="TS-QtyBuy" width="9mm">
          <header master-ressource="TS-QtyHdr" master-ressource-colspan="2"/>
          <data rowspan="2" text-align="right"/>
        </column>
        <column number="9" name="TS-QtySell" width="9mm">
          <header master-ressource="TS-QtyHdr" master-ressource-colspan="0"/>
          <data rowspan="2" text-align="right"/>
        </column>
        <column number="10" name="TS-Maturity" colspan="2" width="23mm">
          <data text-align="center"/>
        </column>
        <column number="10" name="TS-PutCall" master-column="TS-Maturity" width="5.5mm"/>
        <column number="11" name="TS-ConvertedStrike" master-column="TS-Maturity" width="17.5mm">
          <header text-align="right"/>
          <data text-align="right" typenode="true"/>
        </column>
        <column number="12" name="TS-PriceCu" width="6mm">
          <header colspan="0"/>
          <data border-bottom-style="none" border-right-style="none" font-size="{$gDetTrdDetFontSize}"/>
        </column>
        <column number="13" name="TS-ConvertedPrice" colspan="2" width="18mm">
          <header colspan="3"/>
          <data text-align="right" border-left-style="none" typenode="true"/>
        </column>
        <column number="13" name="TS-Premium" master-column="TS-ConvertedPrice" width="14.5mm">
          <header colspan="3"/>
          <data colspan="2" text-align="right" border-left-style="none"/>
        </column>
        <column number="14" name="TS-PremiumDrCr" master-column="TS-ConvertedPrice" width="3.5mm">
          <header colspan="0"/>
          <data display-align="center" font-size="{$gDetTrdDetFontSize}"/>
        </column>
      </group>
    </xsl:if>
    <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom' or 
            $gReportHeaderFooter/summaryFees/text() = 'Bottom'">
      <!-- EG 20160404 Migration vs2013 -->
      <group sort="3" name="CBStream" isDataGroup="{true()}" padding-top="3mm" table-omit-header-at-break="true">
        <!--Titles-->
        <!-- EG 20160404 Migration vs2013 -->
        <title empty-row="1mm" withColumnHdr="{false()}" withSubColumn="{false()}" font-size="7" font-weight="bold"
               background-color="{$gDetTrdHdrCellBackgroundColor}">
          <column number="01" name="TS-RecapCashFlowsTitle" colspan="7">
            <data border-bottom-style="solid"/>
          </column>
          <column number="02" name="TS-FxRate" colspan="3">
            <data border-bottom-style="solid" text-align="right" display-align="center" font-size="5"/>
          </column>
        </title>

        <!--Columns-->
        <column number="01" name="TS-Designation1" width="3mm">
          <!-- EG 20160404 Migration vs2013 -->
          <header colspan="3"
                  ressource="CSHBAL-Designation"
                  font-size="7pt" font-weight="bold" text-align="left" border-right-style="none"/>
          <data display-align="center"/>
        </column>
        <column number="02" name="TS-Designation2" width="26mm">
          <header colspan="-1"/>
          <data display-align="center"/>
        </column>
        <column number="03" name="TS-Designation3" width="68mm">
          <header colspan="-1"/>
          <data display-align="center"/>
        </column>
        <column number="04" name="TS-Currency" width="8mm">
          <header colspan="-1"/>
          <data text-align="right" display-align="center" border-right-style="none" />
        </column>
        <column number="05" name="TS-Amount" width="26mm">
          <!-- EG 20160404 Migration vs2013 -->
          <header ressource="%%RES:CSHBAL-AmountTitle%%"
                  text-align="center" colspan="3" font-size="7pt" font-weight="bold" border-left-style="none"/>
          <data text-align="right" display-align="center" border-left-style="none" border-right-style="none" decPattern="%%PAT:currency.PARENT%%"/>
        </column>
        <!-- EG 20160404 Migration vs2013 -->
        <column number="06" name="TS-CrDr" width="3.5mm" font-size="5pt">
          <header colspan="-1"/>
          <data border-left-style="none" display-align="center"/>
        </column>
        <!-- EG 20160404 Migration vs2013 -->
        <column number="07" name="TS-Desc" width="25mm" font-size="6pt">
          <header ressource=" " colspan="2" border-right-style="none"/>
          <data font-style="italic" display-align="center"/>
        </column>
        <column number="08" name="TS-ExCurrency" width="8mm">
          <header colspan="-1"/>
          <data text-align="right" display-align="center" border-right-style="none"/>
        </column>
        <column number="09" name="TS-ExAmount" width="26mm">
          <header ressource="%%RES:CSHBAL-CounterValueTitle%% {$gReportingCurrency}"
                  text-align="right" colspan="2" font-size="7" font-weight="bold" border-left-style="none"/>
          <data text-align="right" display-align="center" border-left-style="none" border-right-style="none" decPattern="%%PAT:RepCurrency%%"/>
        </column>
        <!-- EG 20160404 Migration vs2013 -->
        <column number="10" name="TS-ExCrDr" width="3.5mm" font-size="5pt">
          <header colspan="-1"/>
          <data border-left-style="none" display-align="center"/>
        </column>

        <condition name="IsFeesDetailsExist" condition="feeGrossAmount"/>

        <!--data of summaryCashFlows-->
        <xsl:if test="($gReportHeaderFooter=false()) or $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom'">

          <!-- EG 20160404 Migration vs2013 -->
          <row sort="1" name="optionPremium" font-size="7pt" >
            <column name="TS-Designation1">
              <data rowspan="2"/>
            </column>
            <column name="TS-Designation2">
              <data colspan="2" value="%%RES:CSHBAL-optionPremium%%"/>
            </column>
            <column name="TS-Currency"/>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%" font-weight="bold"/>
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%" font-weight="bold"/>
            </column>
            <column name="TS-Desc"/>
            <column name="TS-ExCurrency"/>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%" font-weight="bold"/>
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%" font-weight="bold"/>
            </column>
          </row>
          <!-- EG 20160404 Migration vs2013 -->
          <row sort="2" name="feesVAT" font-size="7pt" >
            <column name="TS-Designation2">
              <data colspan="2" value="%%RES:CSHBAL-feesVAT%%"/>
            </column>
            <column name="TS-Currency"/>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%" font-weight="bold"/>
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%" font-weight="bold"/>
            </column>
            <column name="TS-Desc">
              <data value="%%DESC:DetFees%%" condition="IsFeesDetailsExist" />
            </column>
            <column name="TS-ExCurrency"/>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%" font-weight="bold"/>
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%" font-weight="bold"/>
            </column>
          </row>
          <!-- EG 20160404 Migration vs2013 -->
          <row sort="3" name="todayCashBalance" font-size="7pt" font-weight="bold" background-color="{$gDetTrdTotalBackgroundColor}">
            <column name="TS-Designation1">
              <data colspan="3"
                value="%%RES:TS-Total%%"
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
                border-bottom-style="solid"/>
            </column>
          </row>
        </xsl:if>
        <!--data of summaryFees-->
        <xsl:if test="($gReportHeaderFooter=false()) or $gReportHeaderFooter/summaryFees/text() = 'Bottom'">

          <row sort="4" name="%%EMPTY%%" height="0.2cm" condition="IsFeesDetailsExist"/>
          <row sort="5" name="%%FREE%%" condition="IsFeesDetailsExist"
           font-size="7" font-weight="bold"
           background-color="{$gDetTrdHdrCellBackgroundColor}">
            <column number="01" name="%%EMPTY%%"/>
            <column name="TS-Designation2">
              <data colspan="2" value="%%RES:CSHBAL-feesConstituents%%"/>
            </column>
            <column number="03" name="%%EMPTY%%">
              <data colspan="7" border-left-style="solid"/>
            </column>
          </row>
          <!-- EG 20160404 Migration vs2013 -->
          <row sort="6" name="feeGrossAmount" dataLink="%%DATA:keyForSum%%" font-size="7pt">
            <column name="%%EMPTY%%"/>
            <column name="TS-Designation2">
              <data rowspan="%%DATA:rowspan%%" value="%%DATA:keyForSum.RES%%"/>
            </column>
            <column name="TS-Designation3">
              <data value="%%RES:CSHBAL-feeGrossAmount%%" />
            </column>
            <column name="TS-Currency"/>
            <column name="TS-Amount">
              <data value="%%DATA:amount%%" />
            </column>
            <column name="TS-CrDr">
              <data value="%%DATA:crDr%%" font-size="5"/>
            </column>
            <column name="TS-Desc"/>
            <column name="TS-ExCurrency"/>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%" />
            </column>
            <column name="TS-ExCrDr">
              <data value="%%DATA:crDr%%" font-size="5"/>
            </column>
            <!-- EG 20160404 Migration vs2013 -->
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
                <!-- EG 20160404 Migration vs2013 -->
                <data value="%%DATA:crDr%%" font-size="5pt"/>
              </column>
              <column name="TS-Desc"/>
              <column name="TS-ExCurrency"/>
              <column name="TS-ExAmount">
                <data value="%%DATA:exAmount%%"/>
              </column>
              <column name="TS-ExCrDr">
                <!-- EG 20160404 Migration vs2013 -->
                <data value="%%DATA:crDr%%" font-size="5pt"/>
              </column>
            </row>
          </row>
          <row sort="8" name="totalFeesVAT" font-size="7" font-weight="bold" background-color="{$gDetTrdHdrCellBackgroundColor}">
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
                font-size="5"
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
                font-size="5"
                border-bottom-style="solid"/>
            </column>
          </row>
        </xsl:if>
      </group>
    </xsl:if>
    <xsl:if test="($gReportHeaderFooter=false()) or $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom'">
      <!-- EG 20160404 Migration vs2013 -->
      <group sort="4" name="CBStreamEx" isDataGroup="{true()}" padding-top="3mm" table-omit-header-at-break="true">
        <!--Titles-->
        <!-- EG 20160404 Migration vs2013 -->
        <title empty-row="0.1cm" withColumnHdr="{false()}" withSubColumn="{false()}" font-size="7pt" font-weight="bold"
               background-color="{$gDetTrdHdrCellBackgroundColor}">
          <column number="01" name="TS-RecapCashFlowsExTitle" colspan="7">
            <data border-bottom-style="solid" value="%%RES:CSHBAL-CBStreamEx%% {$gReportingCurrency}"/>
          </column>
        </title>

        <!--Columns-->
        <column number="01" name="TS-Designation1" width="3mm">
          <header colspan="3"
                  ressource="CSHBAL-Designation"
                  font-size="7" font-weight="bold" text-align="left"/>
          <data display-align="center"/>
        </column>
        <column number="02" name="TS-Designation2" width="94mm">
          <header colspan="-1"/>
          <data display-align="center"/>
        </column>
        <column number="03" name="TS-Filler" width="37.5mm">
          <header colspan="-1"/>
          <data display-align="center"/>
        </column>
        <column number="04" name="TS-Desc" width="25mm">
          <header ressource=" " colspan="2" border-right-style="none"/>
          <data display-align="center"/>
        </column>
        <column number="05" name="TS-ExCurrency" width="8mm">
          <header colspan="-1"/>
          <data text-align="right" display-align="center" border-right-style="none"/>
        </column>
        <column number="06" name="TS-ExAmount" width="26mm">
          <!-- EG 20160404 Migration vs2013 -->
          <header ressource="%%RES:CSHBAL-CounterValueTitle%% {$gReportingCurrency}"
                  text-align="right" colspan="2" font-size="7pt" font-weight="bold" border-left-style="none"/>
          <data text-align="right" display-align="center" border-left-style="none" border-right-style="none" decPattern="%%PAT:RepCurrency%%"/>
        </column>
        <column number="07" name="TS-ExCrDr" width="3.5mm">
          <header colspan="-1"/>
          <!-- EG 20160404 Migration vs2013 -->
          <data display-align="center" border-left-style="none" font-size="5pt"/>
        </column>

        <!--data-->
        <!-- EG 20160404 Migration vs2013 -->
        <row sort="1" name="optionPremium" font-size="7pt" >
          <column name="TS-Designation1">
            <data rowspan="2"/>
          </column>
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-optionPremium%%"/>
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
        <!-- EG 20160404 Migration vs2013 -->
        <row sort="2" name="feesVAT" font-size="7pt" >
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-feesVAT%%"/>
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
        <!-- EG 20160404 Migration vs2013 -->
        <row sort="3" name="todayCashBalance" font-size="7pt" font-weight="bold" background-color="{$gDetTrdTotalBackgroundColor}">
          <column name="TS-Designation1">
            <data colspan="2"
                  value="%%RES:TS-Total%%"
                  border-bottom-style="solid"/>
          </column>
          <column name="TS-Filler">
            <data border-bottom-style="solid"/>
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
                  border-bottom-style="solid"/>
          </column>
        </row>
      </group>
    </xsl:if>
  </xsl:variable>
  <!-- ===== SubTotals settings of principal Block (ETDBlock) ===== -->
  <xsl:variable name="gColSubTotalColDisplaySettings">
    <group name="ETDBlock">
      <row sort="1">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <subtotal sort="2" name="TS-QtySumBuy" empty-columns="6"/>
          </xsl:when>
          <xsl:otherwise>
            <subtotal sort="1" name="TS-Book" columns-spanned="6" text-align="left"/>
            <subtotal sort="2" name="TS-QtySumBuy" empty-columns="1"/>
          </xsl:otherwise>
        </xsl:choose>
        <subtotal sort="3" name="TS-QtySumSell"/>
        <subtotal sort="4" name="TS-PRMSum" empty-columns="2" />
        <!-- EG 20160404 Migration vs2013 -->
        <subtotal sort="5" name="TS-PRMSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        <xsl:if test="$gPaymentTypes[@id='Bro']">
          <subtotal sort="6" name="TS-BrokerageSum" empty-columns="1" />
          <!-- EG 20160404 Migration vs2013 -->
          <subtotal sort="7" name="TS-BrokerageSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='BroTrd']">
          <subtotal sort="6" name="TS-BrokerageTrdSum" empty-columns="1" />
          <!-- EG 20160404 Migration vs2013 -->
          <subtotal sort="7" name="TS-BrokerageTrdSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='Fee']">
          <subtotal sort="8" name="TS-FeeSum" />
          <!-- EG 20160404 Migration vs2013 -->
          <subtotal sort="9" name="TS-FeeSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='FeeTrd']">
          <subtotal sort="8" name="TS-FeeTrdSum"/>
          <!-- EG 20160404 Migration vs2013 -->
          <subtotal sort="9" name="TS-FeeTrdSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
      </row>
      <row sort="2">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <subtotal sort="2" name="TS-QtyPos" columns-spanned="6" text-align="right"/>
          </xsl:when>
          <xsl:otherwise>
            <!-- EG 20160404 Migration vs2013 -->
            <subtotal sort="1" name="TS-BookDisplayname" columns-spanned="6" font-size="{$gDetTrdDetFontSize}" text-align="left"/>
            <subtotal sort="2" name="TS-QtyPos" text-align="left"/>
          </xsl:otherwise>
        </xsl:choose>
        <subtotal sort="3" name="TS-QtyPosBuy"/>
        <subtotal sort="4" name="TS-QtyPosSell"/>
        <subtotal sort="5" name="TS-ConvertedAvgPxBuy" columns-spanned="2" font-weight="normal" typenode="true">
          <label columns-spanned="2" font-weight="normal"/>
        </subtotal>
        <xsl:if test="$gPaymentTypes[@id='Bro']">
          <subtotal sort="6" name="EmptySum" empty-columns="1" />
          <!-- EG 20160404 Migration vs2013 -->
          <subtotal sort="7" name="EmptySumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='BroClr']">
          <subtotal sort="6" name="TS-BrokerageClrSum" empty-columns="1" />
          <!-- EG 20160404 Migration vs2013 -->
          <subtotal sort="7" name="TS-BrokerageClrSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='Fee']">
          <subtotal sort="8" name="EmptySum" />
          <!-- EG 20160404 Migration vs2013 -->
          <subtotal sort="9" name="EmptySumSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
        <xsl:if test="$gPaymentTypes[@id='FeeClr']">
          <subtotal sort="8" name="TS-FeeClrSum"/>
          <!-- EG 20160404 Migration vs2013 -->
          <subtotal sort="9" name="TS-FeeClrSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        </xsl:if>
      </row>
      <row sort="3">
        <condition subtotal="TS-AvgPxBuy"/>
        <condition subtotal="TS-AvgPxSell"/>

        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <subtotal sort="1" name="TS-ConvertedAvgPxSell" columns-spanned="2" font-weight="normal" empty-columns="8" typenode="true">
              <label columns-spanned="2" font-weight="normal"/>
            </subtotal>
          </xsl:when>
          <xsl:otherwise>
            <subtotal sort="1" name="TS-ConvertedAvgPxSell" columns-spanned="2" font-weight="normal" empty-columns="9" typenode="true">
              <label columns-spanned="2" font-weight="normal"/>
            </subtotal>
          </xsl:otherwise>
        </xsl:choose>
        <subtotal sort="2" name="EmptySum" empty-columns="4" />
      </row>
    </group>
  </xsl:variable>

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
    <xsl:param name="pLastQty" />
    <xsl:param name="pLastPx" />
    <xsl:param name="pPremium" />

    <xsl:attribute name="totalPx">
      <xsl:value-of select="$pLastQty*$pLastPx"/>
    </xsl:attribute>

    <xsl:call-template name="AddAttributeOptionnal">
      <xsl:with-param name="pName" select="'ordTyp'"/>
      <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@OrdTyp)"/>
    </xsl:call-template>
    <xsl:call-template name="AddAttributeOptionnal">
      <xsl:with-param name="pName" select="'execBroker'"/>
      <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/Pty[@R='1']/@ID)"/>
    </xsl:call-template>

    <xsl:copy-of select="$pPremium"/>

    <!--fees-->
    <xsl:variable name="vPty" select="./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide[@Acct=$pBook]/Pty[@R='27']/@ID"/>

    <fees>
      <xsl:apply-templates select="$gPaymentTypes" mode="create">
        <xsl:with-param name="pPty" select="$vPty" />
        <xsl:with-param name="pOtherPartyPayment" select="current()/otherPartyPayment"/>
      </xsl:apply-templates>
    </fees>
  </xsl:template>
  <!-- BusinessSpecificTradeWithStatusData -->
  <xsl:template name="BusinessSpecificTradeWithStatusData">
    <xsl:param name="pGroupePaymentCu" />

    <xsl:variable name="vPayments" select="current()/fees/payment"/>

    <xsl:if test="count($vPayments)>0">
      <fees>
        <xsl:apply-templates select="$gPaymentTypes" mode="serie-status">
          <xsl:with-param name="pPayments" select="$vPayments"/>
          <xsl:with-param name="pGroupePaymentCu" select="$pGroupePaymentCu"/>
        </xsl:apply-templates>
      </fees>
    </xsl:if>
  </xsl:template>
  <!-- SpecificSerieSubTotal template -->
  <xsl:template name="SpecificSerieSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pGroupePaymentCu" />
    <xsl:param name="pCuPrice"/>

    <xsl:call-template name="AllocAndInvSerieSubTotal">
      <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
      <xsl:with-param name="pSerieTradesWithStatus" select="$pSerieTradesWithStatus"/>
      <xsl:with-param name="pGroupePaymentCu" select="$pGroupePaymentCu"/>
      <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
    </xsl:call-template>
    <!--//-->
    <xsl:variable name="vPayments" select="$pSerieTrades/fees/payment"/>

    <xsl:apply-templates select="$gPaymentTypes" mode="serie-subtotal">
      <xsl:with-param name="pPayments" select="$vPayments"/>
      <xsl:with-param name="pGroupePaymentCu" select="$pGroupePaymentCu"/>
    </xsl:apply-templates>

  </xsl:template>

  <!--SpecificGroupSubTotal template-->
  <!--Pas de sous totaux pour les groupes sur les avis d'opéré-->
  <xsl:template name="SpecificGroupSubTotal"/>

  <!-- ===== CreateSpecificGroups ===== -->
  <xsl:template name="CreateSpecificGroups"/>

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

    <xsl:call-template name="Trade-display">
      <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay" />
      <xsl:with-param name="pTrade" select="." />
      <xsl:with-param name="pTradeWithStatus" select="$pCurrentTradeWithStatus[@href=current()/@href]" />
      <xsl:with-param name="pIsFirstTrade" select="boolean(position() = 1)" />
      <xsl:with-param name="pIsLastTrade" select="boolean(position() = last())" />
    </xsl:call-template>
  </xsl:template>

  <!-- ===== Mapping between columns to display and Data to display for each column ===== -->
  <!-- GetColumnDataToDisplay -->
  <xsl:template name="GetColumnDataToDisplay">
    <xsl:param name="pRowData" />
    <xsl:param name="pTradeWithStatus" />
    <xsl:param name="pIsGetDetail" select="false()" />

    <xsl:variable name="vColumnName" select="@name"/>

    <xsl:choose>
      <!-- ===== OrderType ===== -->
      <xsl:when test="$vColumnName='ALLOC-OrderType'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:variable name="vOrdTypeEnum" select="$gDataDocumentRepository/enums[@id='ENUMS.CODE.OrdTypeEnum']/enum[value/text()=$pRowData/@ordTyp]"/>
            <xsl:choose>
              <xsl:when test="string-length($vOrdTypeEnum/extattrb/text())>0">
                <xsl:value-of select="$vOrdTypeEnum/extattrb/text()" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vOrdTypeEnum/value/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== ExecBroker ===== -->
      <xsl:when test="$vColumnName='ALLOC-ExecBroker'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:value-of select="$pRowData/@execBroker"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== QtyBuy ===== -->
      <xsl:when test="$vColumnName='TS-QtyBuy'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="$pRowData/@side = '1'">
              <xsl:call-template name="format-integer">
                <xsl:with-param name="integer" select="$pRowData/@lastQty" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== QtySell ===== -->
      <xsl:when test="$vColumnName='TS-QtySell'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="$pRowData/@side = '2'">
              <xsl:call-template name="format-integer">
                <xsl:with-param name="integer" select="$pRowData/@lastQty" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== PosEffect ===== -->
      <xsl:when test="$vColumnName='ALLOC-PosEffect'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pRowData/@posEfct='O'">
                <xsl:value-of select="'Open'"/>
              </xsl:when>
              <xsl:when test="$pRowData/@posEfct='C'">
                <xsl:value-of select="'Close'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$pRowData/@posEfct"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
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
  <!--GetColumnIsAltData-->
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
  <xsl:template name="GetSpecificDataFontWeight"/>

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

    <!-- ===== Common ===== -->
    <xsl:call-template name="GetCommonSerieSubTotalLabel">
      <xsl:with-param name="pSerie" select="$pSerie" />
      <xsl:with-param name="pSubTotalName" select="$pSubTotalName" />
    </xsl:call-template>

  </xsl:template>
  <!--GetSerieSubTotalData-->
  <xsl:template name="GetSerieSubTotalData">
    <xsl:param name="pSerie"/>
    <xsl:param name="pSubTotalName"/>

    <!-- ===== Common ===== -->
    <xsl:call-template name="GetCommonSerieSubTotalData">
      <xsl:with-param name="pSerie" select="$pSerie" />
      <xsl:with-param name="pSubTotalName" select="$pSubTotalName" />
    </xsl:call-template>
  </xsl:template>

</xsl:stylesheet>
