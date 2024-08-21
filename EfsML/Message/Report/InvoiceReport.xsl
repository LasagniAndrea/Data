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
Version 1.0.0.0 (Spheres 2.3.1.0)
 Date: 25/01/2013
 Author: RD
 Description: first version
 
================================================================================================================
  -->
  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="Invoice/Invoice.xslt" />
  <xsl:include href="Shared/Shared_Report_PDFETD.xslt" />

  <!-- ================================================== -->
  <!--        Parameters                                  -->
  <!-- ================================================== -->
  <xsl:param name="pCurrentCulture" select="'en-GB'" />
  <xsl:param name="pTradeExtlLink" select="''" />
  <xsl:param name="pProduct" select="''" />
  <xsl:param name="pInstrument" select="//productType" />

  <!-- ================================================== -->
  <!--        Global Variables                            -->
  <!-- ================================================== -->
  <xsl:variable name="gConfirmationMessage" select="//confirmationMessage"/>
  <xsl:variable name="gDataDocumentTrades" select="msxsl:node-set($varProduct)/invoiceDetails/invoiceTrade"/>
  <xsl:variable name="gReportType" select="$gcReportTypeINV"/>
  <!--Legend-->
  <xsl:variable name="gIsReportWithSpecificLegend" select="false()"/>
  <xsl:variable name="gIsReportWithOrderTypeLegend" select="true()"/>
  <xsl:variable name="gIsReportWithTradeTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithPriceTypeLegend" select="false()"/>

  <!--<xsl:variable name="gInvoicingPaymentType" select="msxsl:node-set($gcFeeDisplaySettings)/paymentType[@name=$gDataDocumentRepository/invoicingScope/paymentType]"/>-->
  <xsl:variable name="gInvoicingFeeType" select="$gDataDocumentTrades/invoiceFees/invoiceFee/feeType/text()"/>
  <xsl:variable name="gInvoicingPaymentType" select="msxsl:node-set($gcFeeDisplaySettings)/paymentType[@eventType=$gInvoicingFeeType]"/>

  <!-- .................................. -->
  <!--   XSL Keys variables               -->
  <!-- .................................. -->
  <!-- keys -->
  <xsl:key name="kInvoiceBookKey" match="idB_Pay" use="text()"/>

  <!-- ================================================== -->
  <!--        Global Variables : Display                  -->
  <!-- ================================================== -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="gInvoicingReceiverPaddingTop">20mm</xsl:variable>
  <xsl:variable name="gDetTrdTableTotalWidth">197mm</xsl:variable>

  <!--Summury block-->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="gSummuryBlockPaddingTop">30mm</xsl:variable>
  <xsl:variable name="gSummuryBlockPadding">15mm</xsl:variable>
  <xsl:variable name="gSummaryBlockWidth" select="concat(number($varInvoiceSummaryTextWidth) + number($varInvoiceSummaryAmountWidth) + number($varInvoiceSummaryCurrencyWidth), 'mm')" />

  <!-- ===== Display settings of all Blocks===== -->
  <xsl:variable name="gGroupDisplaySettings">
    <group sort="1" name="ETDBlock" padding-top="0.2cm">
      <!--Title-->
      <!-- EG 20160404 Migration vs2013 -->
      <title empty-row="1mm" withColumnHdr="{true()}" withSubColumn="{true()}" font-size="7pt">
        <column number="01" name="TS-MarketTitle" colspan="1">
          <header ressource="TS-MARKET" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="02" name="TS-DCTitle" colspan="5">
          <header ressource="TS-DERIVATIVECONTRACT" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="03" name="TS-DCUNLTitle" colspan="6">
          <header ressource="TS-DERIVATIVECONTRACTUNL" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
      </title>

      <column number="01" name="TS-Book" width="44.5mm" >
        <header rowspan="2"/>
        <data rowspan="2" length="28"/>
      </column>
      <column number="02" name="TS-OrderNum" width="19mm">
        <header ressource="ALLOC-OrderNum" master-ressource="ALLOC-OrderHdr" master-ressource-colspan="2"/>
        <data  rowspan="2" text-align="center" length="11"/>
      </column>
      <column number="03" name="ALLOC-OrderType" width="7mm">
        <header master-ressource="ALLOC-OrderHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="center"/>
      </column>
      <column number="04" name="TS-ClearBroker" width="11mm">
        <header rowspan="2"/>
        <data rowspan="2" text-align="center" length="6"/>
      </column>
      <column number="05" name="TS-TradeDate" width="14mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="2"/>
        <data text-align="center"/>
      </column>
      <column number="05" name="TS-TradeTime" master-column="TS-TradeDate" width="14mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="06" name="TS-TradeNum" width="18mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="center" length="12"/>
      </column>
      <column number="07" name="TS-QtyBuy" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="2"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="08" name="TS-QtySell" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="09" name="TS-Maturity" colspan="2" width="23mm">
        <data text-align="center"/>
      </column>
      <column number="09" name="TS-PutCall" master-column="TS-Maturity" width="5.5mm"/>
      <column number="10" name="TS-ConvertedStrike" master-column="TS-Maturity" width="17.5mm">
        <header text-align="right"/>
        <data text-align="right" typenode="true"/>
      </column>
      <column number="11" name="TS-ConvertedPrice" width="18mm">
        <header rowspan="2"/>
        <data rowspan="2" text-align="right" typenode="true"/>
      </column>
      <column number="12" name="ALLOC-PosEffect" width="9mm">
        <header ressource="INV-PosEffect2" master-ressource="INV-PosEffect1" master-ressource-colspan="1"/>
        <data rowspan="2" text-align="center"/>
      </column>
      <!--Fee: 1st row-->
      <xsl:choose>
        <xsl:when test="$gInvoicingPaymentType[@id='Bro']">
          <column number="13" name="TS-Brokerage" width="12mm">
            <header colspan="2"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-BrokerageDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='BroTrd']">
          <column number="13" name="TS-BrokerageTrd" width="12mm">
            <header colspan="2" ressource="TS-Brokerage"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-BrokerageTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='BroClr']">
          <column number="13" name="TS-BrokerageClr" width="12mm">
            <header colspan="2" ressource="TS-Brokerage"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-BrokerageClrDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='Fee']">
          <column number="13" name="TS-Fee" width="12mm">
            <header colspan="2"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-FeeDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='FeeTrd']">
          <column number="13" name="TS-FeeTrd" width="12mm">
            <header colspan="2" ressource="TS-Fee"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-FeeTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='FeeClr']">
          <column number="13" name="TS-FeeClr" width="12mm">
            <header colspan="2" ressource="TS-Fee"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-FeeClrDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
      </xsl:choose>
      <!--Fee: 2nd row-->
      <xsl:choose>
        <xsl:when test="$gInvoicingPaymentType[@id='Bro']">
          <column number="14" name="EmptyColumn" master-column="TS-BrokerageDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='BroTrd']">
          <column number="14" name="EmptyColumn" master-column="TS-BrokerageTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='BroClr']">
          <column number="14" name="EmptyColumn" master-column="TS-BrokerageClrDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='Fee']">
          <column number="14" name="EmptyColumn" master-column="TS-FeeDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='FeeTrd']">
          <column number="14" name="EmptyColumn" master-column="TS-FeeTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='FeeClr']">
          <column number="14" name="EmptyColumn" master-column="TS-FeeClrDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
      </xsl:choose>
    </group>
    <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom' or 
            $gReportHeaderFooter/summaryFees/text() = 'Bottom'">
      <group sort="2" name="CBStream" isDataGroup="{true()}" padding-top="0.3cm" table-omit-header-at-break="true">
        <!--Titles-->
        <!-- EG 20160404 Migration vs2013 -->
        <title empty-row="1mm" withColumnHdr="{false()}" withSubColumn="{false()}" font-size="7pt" font-weight="bold"
               background-color="{$gDetTrdHdrCellBackgroundColor}">
          <column number="01" name="TS-RecapCashFlowsTitle" colspan="7">
            <data border-bottom-style="solid"/>
          </column>
          <!--RD 20151002 [21426] Use column 'Report-FxRate' instead 'TS-FxRate'-->
          <column number="02" name="Report-FxRate" colspan="3">
            <!-- EG 20160404 Migration vs2013 -->
            <data border-bottom-style="solid" text-align="right" display-align="center" font-size="5pt"/>
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
        <!-- EG 20160404 Migration vs2013 -->
        <column number="09" name="TS-ExAmount" width="26mm">
          <header ressource="%%RES:CSHBAL-CounterValueTitle%% {$gReportingCurrency}"
                  text-align="right" colspan="2" font-size="7pt" font-weight="bold" border-left-style="none"/>
          <data text-align="right" display-align="center" border-left-style="none" border-right-style="none" decPattern="%%PAT:RepCurrency%%"/>
        </column>
        <!-- EG 20160404 Migration vs2013 -->
        <column number="10" name="TS-ExCrDr" width="3.5mm" font-size="5pt">
          <header colspan="-1"/>
          <data border-left-style="none" display-align="center"/>
        </column>

        <!--data of summaryCashFlows or summaryFees-->
        <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom' or 
            $gReportHeaderFooter/summaryFees/text() = 'Bottom'">
          <!-- EG 20160404 Migration vs2013 -->
          <row sort="1" name="feeGrossAmount" dataLink="%%DATA:keyForSum%%" font-size="7pt">
            <column name="TS-Designation1">
              <data rowspan="%%DATA:rowspan%%"/>
            </column>
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
              <!-- EG 20160404 Migration vs2013 -->
              <data value="%%DATA:crDr%%" font-size="5pt"/>
            </column>
            <column name="TS-Desc"/>
            <column name="TS-ExCurrency"/>
            <column name="TS-ExAmount">
              <data value="%%DATA:exAmount%%" />
            </column>
            <column name="TS-ExCrDr">
              <!-- EG 20160404 Migration vs2013 -->
              <data value="%%DATA:crDr%%" font-size="5pt"/>
            </column>
            <!-- EG 20160404 Migration vs2013 -->
            <row sort="2" name="feeTax" dataLink="%%DATA:keyForSum%%" font-size="7pt">
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
      </group>
    </xsl:if>

    <group sort="1" name="AmendedETDBlock" padding-top="0.2cm">
      <!--Title-->
      <!-- EG 20160404 Migration vs2013 -->
      <title empty-row="1mm" withColumnHdr="{true()}" withSubColumn="{true()}" font-size="7pt">
        <column number="01" name="TS-MarketTitle" colspan="1">
          <header ressource="TS-MARKET" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="02" name="TS-DCTitle" colspan="6">
          <header ressource="TS-DERIVATIVECONTRACT" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="03" name="TS-DCUNLTitle" colspan="7">
          <header ressource="TS-DERIVATIVECONTRACTUNL" font-weight="normal" text-align="left"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
      </title>

      <column number="01" name="TS-Book" width="39.5mm" >
        <header rowspan="2"/>
        <data rowspan="2" length="28"/>
      </column>
      <!--<column number="02" name="TS-OrderNum" width="19mm">
        <header ressource="ALLOC-OrderNum" master-ressource="ALLOC-OrderHdr" master-ressource-colspan="2"/>
        <data  rowspan="2" text-align="center" length="11"/>
      </column>
      <column number="03" name="ALLOC-OrderType" width="7mm">
        <header master-ressource="ALLOC-OrderHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="center"/>
      </column>-->
      <column number="02" name="TS-ClearBroker" width="11mm">
        <header rowspan="2"/>
        <data rowspan="2" text-align="center" length="6"/>
      </column>
      <column number="03" name="TS-TradeDate" width="14mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="2"/>
        <data text-align="center"/>
      </column>
      <column number="03" name="TS-TradeTime" master-column="TS-TradeDate" width="14mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="04" name="TS-TradeNum" width="18mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="center" length="12"/>
      </column>
      <column number="05" name="TS-QtyBuy" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="2"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="06" name="TS-QtySell" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="07" name="TS-Maturity" colspan="2" width="23mm">
        <data text-align="center"/>
      </column>
      <column number="07" name="TS-PutCall" master-column="TS-Maturity" width="5.5mm"/>
      <column number="08" name="TS-ConvertedStrike" master-column="TS-Maturity" width="17.5mm">
        <header text-align="right"/>
        <data text-align="right" typenode="true"/>
      </column>
      <column number="09" name="TS-ConvertedPrice" width="18mm">
        <header rowspan="2"/>
        <data rowspan="2" text-align="right" typenode="true"/>
      </column>
      <column number="10" name="ALLOC-PosEffect" width="9mm">
        <header ressource="INV-PosEffect2" master-ressource="INV-PosEffect1" master-ressource-colspan="1"/>
        <data rowspan="2" text-align="center"/>
      </column>
      <!--Fee: 1st row-->
      <xsl:choose>
        <xsl:when test="$gInvoicingPaymentType[@id='Bro']">
          <column number="11" name="INV-BrokeragePrev" width="12mm">
            <header colspan="2" ressource="INV-BrokeragePrev"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="12" name="INV-BrokeragePrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
          <column number="13" name="TS-Brokerage" width="12mm">
            <header colspan="2" ressource="INV-BrokerageAmended"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-BrokerageDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='BroTrd']">
          <column number="11" name="INV-BrokerageTrdPrev" width="12mm">
            <header colspan="2" ressource="INV-BrokeragePrev"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="12" name="INV-BrokerageTrdPrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
          <column number="13" name="TS-BrokerageTrd" width="12mm">
            <header colspan="2" ressource="INV-BrokerageAmended"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-BrokerageTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='BroClr']">
          <column number="11" name="INV-BrokerageClrPrev" width="12mm">
            <header colspan="2" ressource="INV-BrokeragePrev"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="12" name="INV-BrokerageClrPrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
          <column number="13" name="TS-BrokerageClr" width="12mm">
            <header colspan="2" ressource="INV-BrokerageAmended"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-BrokerageClrDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='Fee']">
          <column number="11" name="TS-FeePrev" width="12mm">
            <header colspan="2" ressource="INV-FeePrev"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="12" name="TS-FeePrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
          <column number="13" name="TS-Fee" width="12mm">
            <header colspan="2" ressource="INV-FeeAmended"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-FeeDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='FeeTrd']">
          <column number="11" name="TS-FeeTrdPrev" width="12mm">
            <header colspan="2" ressource="INV-FeePrev"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="12" name="TS-FeeTrdPrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
          <column number="13" name="TS-FeeTrd" width="12mm">
            <header colspan="2" ressource="INV-FeeAmended"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-FeeTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='FeeClr']">
          <column number="11" name="TS-FeeClrPrev" width="12mm">
            <header colspan="2" ressource="INV-FeePrev"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="12" name="TS-FeeClrPrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
          <column number="13" name="TS-FeeClr" width="12mm">
            <header colspan="2" ressource="INV-FeeAmended"/>
            <data rowspan="2" text-align="right" border-right-style="none"/>
          </column>
          <column number="14" name="TS-FeeClrDrCr" width="3.5mm">
            <header colspan="0"/>
            <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
          </column>
        </xsl:when>
      </xsl:choose>
      <column number="15" name="INV-Delta" width="12mm">
        <header colspan="2"/>
        <data rowspan="2" text-align="right" border-right-style="none"/>
      </column>
      <column number="16" name="INV-DeltaDrCr" width="3.5mm">
        <header colspan="0"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
      </column>
      <!--Fee: 2nd row-->
      <xsl:choose>
        <xsl:when test="$gInvoicingPaymentType[@id='Bro']">
          <column number="12" name="EmptyColumn" master-column="INV-BrokeragePrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
          <column number="14" name="EmptyColumn" master-column="TS-BrokerageDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='BroTrd']">
          <column number="12" name="EmptyColumn" master-column="INV-BrokerageTrdPrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
          <column number="14" name="EmptyColumn" master-column="TS-BrokerageTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='BroClr']">
          <column number="12" name="EmptyColumn" master-column="INV-BrokerageClrPrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
          <column number="14" name="EmptyColumn" master-column="TS-BrokerageClrDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='Fee']">
          <column number="12" name="EmptyColumn" master-column="INV-FeePrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
          <column number="14" name="EmptyColumn" master-column="TS-FeeDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='FeeTrd']">
          <column number="12" name="EmptyColumn" master-column="INV-FeeTrdPrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
          <column number="14" name="EmptyColumn" master-column="TS-FeeTrdDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
        <xsl:when test="$gInvoicingPaymentType[@id='FeeClr']">
          <column number="12" name="EmptyColumn" master-column="INV-FeeClrPrevDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
          <column number="14" name="EmptyColumn" master-column="TS-FeeClrDrCr" width="3.5mm">
            <header colspan="0"/>
            <data border-left-style="none"/>
          </column>
        </xsl:when>
      </xsl:choose>
      <column number="16" name="EmptyColumn" master-column="INV-DeltaDrCr" width="3.5mm">
        <header colspan="0"/>
        <data border-left-style="none"/>
      </column>
    </group>
  </xsl:variable>
  <!-- ===== SubTotals settings of principal Block (ETDBlock) ===== -->
  <xsl:variable name="gColSubTotalColDisplaySettings">
    <group name="ETDBlock">
      <row sort="1">
        <subtotal sort="1" name="TS-Book" columns-spanned="5" text-align="left"/>
        <subtotal sort="2" name="TS-QtySumBuy" empty-columns="1"/>
        <subtotal sort="3" name="TS-QtySumSell"/>
        <subtotal sort="4" name="TS-ConvertedAvgPxBuy" font-weight="normal" typenode="true">
          <label columns-spanned="2" font-weight="normal"/>
        </subtotal>
        <xsl:choose>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='Bro']">
            <subtotal sort="5" name="TS-BrokerageSum" empty-columns="1" />
            <subtotal sort="6" name="TS-BrokerageSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='BroTrd']">
            <subtotal sort="5" name="TS-BrokerageTrdSum" empty-columns="1" />
            <subtotal sort="6" name="TS-BrokerageTrdSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='BroClr']">
            <subtotal sort="5" name="TS-BrokerageClrSum" empty-columns="1" />
            <subtotal sort="6" name="TS-BrokerageClrSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='Fee']">
            <subtotal sort="5" name="TS-FeeSum" empty-columns="1" />
            <subtotal sort="6" name="TS-FeeSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='FeeTrd']">
            <subtotal sort="5" name="TS-FeeTrdSum" empty-columns="1" />
            <subtotal sort="6" name="TS-FeeTrdSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='FeeClr']">
            <subtotal sort="5" name="TS-FeeClrSum" empty-columns="1" />
            <subtotal sort="6" name="TS-FeeClrSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
        </xsl:choose>
      </row>
      <row sort="2">
        <!-- EG 20160404 Migration vs2013 -->
        <subtotal sort="1" name="TS-BookDisplayname" columns-spanned="5" font-size="{$gDetTrdDetFontSize}" text-align="left"/>
        <subtotal sort="2" name="TS-QtyPos" text-align="right"/>
        <subtotal sort="3" name="TS-QtyPosBuy"/>
        <subtotal sort="4" name="TS-QtyPosSell"/>
        <subtotal sort="5" name="TS-ConvertedAvgPxSell" font-weight="normal" typenode="true">
          <label columns-spanned="2" font-weight="normal"/>
        </subtotal>
        <subtotal sort="6" name="EmptySum" columns-spanned="3"/>
      </row>
    </group>
    <group name="AmendedETDBlock">
      <row sort="1">
        <subtotal sort="1" name="TS-Book" columns-spanned="3" text-align="left"/>
        <subtotal sort="2" name="TS-QtySumBuy" empty-columns="1"/>
        <subtotal sort="3" name="TS-QtySumSell"/>
        <subtotal sort="4" name="TS-ConvertedAvgPxBuy" font-weight="normal" typenode="true">
          <label columns-spanned="2" font-weight="normal"/>
        </subtotal>
        <subtotal sort="5" name="TS-BrokeragePrevSum" empty-columns="1" />
        <!-- EG 20160404 Migration vs2013 -->
        <subtotal sort="6" name="TS-BrokeragePrevSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
        <xsl:choose>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='Bro']">
            <subtotal sort="7" name="TS-BrokerageSum" />
            <subtotal sort="8" name="TS-BrokerageSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='BroTrd']">
            <subtotal sort="7" name="TS-BrokerageTrdSum" />
            <subtotal sort="8" name="TS-BrokerageTrdSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='BroClr']">
            <subtotal sort="7" name="TS-BrokerageClrSum" />
            <subtotal sort="8" name="TS-BrokerageClrSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='Fee']">
            <subtotal sort="7" name="TS-FeeSum" />
            <subtotal sort="8" name="TS-FeeSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='FeeTrd']">
            <subtotal sort="7" name="TS-FeeTrdSum" />
            <subtotal sort="8" name="TS-FeeTrdSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:when test="$gInvoicingPaymentType[@id='FeeClr']">
            <subtotal sort="7" name="TS-FeeClrSum" />
            <subtotal sort="8" name="TS-FeeClrSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
          </xsl:when>
        </xsl:choose>
        <subtotal sort="9" name="TS-DeltaSum" />
        <!-- EG 20160404 Migration vs2013 -->
        <subtotal sort="10" name="TS-DeltaSumDrCr" font-size="{$gDetTrdDetFontSize}" display-align="center"/>
      </row>
      <row sort="2">
        <!-- EG 20160404 Migration vs2013 -->
        <subtotal sort="1" name="TS-BookDisplayname" columns-spanned="3" font-size="{$gDetTrdDetFontSize}" text-align="left"/>
        <subtotal sort="2" name="TS-QtyPos" text-align="right"/>
        <subtotal sort="3" name="TS-QtyPosBuy"/>
        <subtotal sort="4" name="TS-QtyPosSell"/>
        <subtotal sort="5" name="TS-ConvertedAvgPxSell" font-weight="normal" typenode="true">
          <label columns-spanned="2" font-weight="normal"/>
        </subtotal>
        <subtotal sort="6" name="EmptySum" columns-spanned="7"/>
      </row>
    </group>
  </xsl:variable>

  <!-- ================================================== -->
  <!--        Main Template                               -->
  <!-- ================================================== -->
  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:variable name="vBookAmendedTrades" select="msxsl:node-set($gBusinessTrades)/trade[number(fees/payment/@deltaAmountET) > 0]"/>
      <xsl:variable name="vAmendedETDBlockGroup" select="msxsl:node-set($gGroupDisplaySettings)/group[@name='AmendedETDBlock']"/>

      <xsl:choose>
        <xsl:when test="$gcIsBusinessDebugMode=false()">
          <!--Create PDF Document-->
          <xsl:call-template name="SetPagesCaracteristics" />
          <!--Display Summury Page of Invoice/Additionnal Invoice/Credit Note-->
          <xsl:call-template name="DisplayDocumentPageSequence">
            <xsl:with-param name="pBookPosition" select="1" />
            <xsl:with-param name="pCountBook" select="1" />
            <xsl:with-param name="pSetLastPage" select="false()" />
            <xsl:with-param name="pIsSummuryPage" select="true()" />
            <xsl:with-param name="pIsCreditNoteVisualization" select="true()" />
          </xsl:call-template>

          <xsl:choose>
            <xsl:when test="($IsDerivative = 'true') and ($varIsAmendedInvoice = 'false')">
              <!--Derivative: Display detailed fees of Invoice-->
              <xsl:call-template name="DisplayDocumentPageSequence">
                <xsl:with-param name="pBookPosition" select="1" />
                <xsl:with-param name="pCountBook" select="1" />
                <xsl:with-param name="pIsCreditNoteVisualization" select="true()" />
                <xsl:with-param name="pIsHFormulaMandatory" select="false()" />
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="($IsDerivative = 'true') and ($varIsAmendedInvoice = 'true')">
              <xsl:variable name="vGroups">
                <xsl:call-template name="CreateGroups">
                  <xsl:with-param name="pBookTrades" select="$vBookAmendedTrades"/>
                  <xsl:with-param name="pETDBlockGroup" select="$vAmendedETDBlockGroup"/>
                  <xsl:with-param name="pIsCreditNoteGroup" select="true()" />
                </xsl:call-template>
              </xsl:variable>
              <!--Derivative: Display detailed fees of Additionnal Invoice/Credit Note-->
              <xsl:call-template name="DisplayDocumentPageSequence">
                <xsl:with-param name="pBookPosition" select="1" />
                <xsl:with-param name="pCountBook" select="1" />
                <xsl:with-param name="pSetLastPage" select="false()" />
                <xsl:with-param name="pIsCreditNoteVisualization" select="true()" />
                <xsl:with-param name="pGroups" select="$vGroups" />
                <xsl:with-param name="pIsHFormulaMandatory" select="false()" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <!--OTC: Display detailed fees of Invoice/Additionnal Invoice/Credit Note-->
              <xsl:call-template name="DocumentContent">
                <xsl:with-param name="pDocumentType" select="'Main'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:if test="($varIsAmendedInvoice = 'true')">
            <!--Display Summury Page of Amended Invoice-->
            <xsl:call-template name="DisplayDocumentPageSequence">
              <xsl:with-param name="pBookPosition" select="1" />
              <xsl:with-param name="pCountBook" select="1" />
              <xsl:with-param name="pSetLastPage" select="false()" />
              <xsl:with-param name="pIsSummuryPage" select="true()" />
              <xsl:with-param name="pIsCreditNoteVisualization" select="false()" />
            </xsl:call-template>
            <xsl:choose>
              <xsl:when test="$IsDerivative = 'true'">
                <!--Derivative: Display detailed fees of Amended Invoice-->
                <xsl:call-template name="DisplayDocumentPageSequence">
                  <xsl:with-param name="pBookPosition" select="1" />
                  <xsl:with-param name="pCountBook" select="1" />
                  <xsl:with-param name="pIsCreditNoteVisualization" select="false()" />
                  <xsl:with-param name="pIsHFormulaMandatory" select="false()" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <!--OTC: Display detailed fees of Amended Invoice-->
                <xsl:call-template name="DocumentContent">
                  <xsl:with-param name="pDocumentType" select="'Enclosure'" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$gcIsBusinessDebugMode">
          <statement creationTimestamp="{$gCreationTimestamp}" clearingDate="{$gClearingDate}">
            <tradeSorting>
              <xsl:copy-of select="$gSortingKeys"/>
            </tradeSorting>
            <!--//-->
            <businessTrades>
              <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade"/>
            </businessTrades>
            <!--//-->
            <xsl:call-template name="CreateGroups">
              <xsl:with-param name="pBookTrades" select="$vBookAmendedTrades"/>
              <xsl:with-param name="pETDBlockGroup" select="$vAmendedETDBlockGroup"/>
              <xsl:with-param name="pIsCreditNoteGroup" select="true()" />
            </xsl:call-template>
            <xsl:call-template name="CreateGroups"/>
          </statement>
        </xsl:when>
      </xsl:choose>
    </fo:root>
  </xsl:template>

  <!-- ================================================== -->
  <!--        Business Template                           -->
  <!-- ================================================== -->
  <!--InvoiceTrade templates-->
  <xsl:template match="invoiceTrade" mode="business">

    <xsl:variable name="vCurrentTradeCopy">
      <xsl:copy-of select="."/>
    </xsl:variable>
    <xsl:variable name="vCurrentTrade" select="msxsl:node-set($vCurrentTradeCopy)/invoiceTrade"/>
    <xsl:variable name="vETD" select="$gConfirmationMessage/details/tradeDetail[@OTCmlId=$vCurrentTrade/@OTCmlId]/product/exchangeTradedDerivative"/>
    <xsl:variable name="vTrdCaptRpt" select="$vETD/FIXML/TrdCaptRpt"/>

    <xsl:variable name="vTradeId" select="string($vCurrentTrade/identifier/text())"/>
    <xsl:variable name="vCategory" select="string($vETD/Category)"/>
    <xsl:variable name="vLastQty" select="number($vTrdCaptRpt/@LastQty)"/>
    <xsl:variable name="vLastPx" select="number($vTrdCaptRpt/@LastPx)"/>
    <xsl:variable name="vIdAsset" select="number($vTrdCaptRpt/Instrmt/@ID)"/>

    <xsl:variable name="vLinkUnderlyerDelivery" select="$gDataDocumentRepository/tradeLink[identifier_a=$vTradeId and 
                    (link = 'UnderlyerDeliveryAfterAutomaticOptionAssignment'
                    or link = 'UnderlyerDeliveryAfterAutomaticOptionExercise'
                    or link = 'UnderlyerDeliveryAfterOptionAssignment'
                    or link = 'UnderlyerDeliveryAfterOptionExercise')]"/>

    <xsl:variable name="vAllInvoicingBook" select="$vCurrentTrade/invoiceFees/invoiceFee/idB_Pay[generate-id()=generate-id(key('kInvoiceBookKey',text()))]"/>

    <xsl:for-each select="$vAllInvoicingBook">
      <xsl:variable name="vIDB" select="number(./text())"/>
      <xsl:variable name="vBook" select="string($gDataDocumentRepository/book[@OTCmlId=$vIDB]/identifier/text())"/>

      <trade href="{$vTradeId}" lastQty="{$vLastQty}" lastPx="{$vLastPx}"
             side="{number($vTrdCaptRpt/RptSide/@Side)}"
             tradeDate="{string($vCurrentTrade/tradeDate)}"
             timeStamp="{substring-after($vTrdCaptRpt/@TxnTm, 'T')}"
             posEfct="{string($vTrdCaptRpt/RptSide/@PosEfct)}">

        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'ordID'"/>
          <xsl:with-param name="pValue" select="string($vTrdCaptRpt/RptSide/@OrdID)"/>
        </xsl:call-template>
        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'ordTyp'"/>
          <xsl:with-param name="pValue" select="string($vTrdCaptRpt/RptSide/@OrdTyp)"/>
        </xsl:call-template>
        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'txt'"/>
          <xsl:with-param name="pValue" select="string($vTrdCaptRpt/RptSide/@Txt)"/>
        </xsl:call-template>
        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'inptSrc'"/>
          <xsl:with-param name="pValue" select="string($vTrdCaptRpt/RptSide/@InptSrc)"/>
        </xsl:call-template>
        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'execID'"/>
          <xsl:with-param name="pValue" select="string($vTrdCaptRpt/@ExecID)"/>
        </xsl:call-template>
        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'clearBroker'"/>
          <xsl:with-param name="pValue" select="string($vTrdCaptRpt/RptSide/Pty[@R='4']/@ID)"/>
        </xsl:call-template>
        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'execID_link'"/>
          <xsl:with-param name="pValue" select="string($vLinkUnderlyerDelivery/executionId/text())"/>
        </xsl:call-template>
        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'idAsset'"/>
          <xsl:with-param name="pValue" select="$vIdAsset"/>
        </xsl:call-template>

        <xsl:attribute name="totalPx">
          <xsl:value-of select="$vLastQty*$vLastPx"/>
        </xsl:attribute>
        <!--fees-->
        <fees>
          <xsl:apply-templates select="$gInvoicingPaymentType" mode="create-fromInvoiceTrade">
            <xsl:with-param name="pIDB" select="$vIDB" />
            <xsl:with-param name="pInvoiceFee" select="$vCurrentTrade/invoiceFees/invoiceFee"/>
          </xsl:apply-templates>
        </fees>

        <!--//-->
        <xsl:variable name="vConvertedLastPx">
          <xsl:call-template name="GetTradeConvertedPriceValue">
            <xsl:with-param name="pIdAsset" select="$vIdAsset"/>
            <xsl:with-param name="pTradeId" select="$vTradeId"/>
          </xsl:call-template>
        </xsl:variable>
        <convertedLastPx>
          <xsl:copy-of select="$vConvertedLastPx"/>
        </convertedLastPx>
        <!--//-->

        <mmy data="{string($vTrdCaptRpt/Instrmt/@MMY)}"/>
        <!--//-->
        <xsl:if test="$vCategory = 'O'">
          <strkPx data="{number($vTrdCaptRpt/Instrmt/@StrkPx)}"/>
          <xsl:variable name="vPutCall">
            <xsl:choose>
              <xsl:when test="string($vTrdCaptRpt/Instrmt/@PutCall) = '0'">
                <xsl:value-of select="'Put'"/>
              </xsl:when>
              <xsl:when test="string($vTrdCaptRpt/Instrmt/@PutCall) = '1'">
                <xsl:value-of select="'Call'"/>
              </xsl:when>
            </xsl:choose>
          </xsl:variable>
          <putCall data="{$vPutCall}"/>
          <!--//-->
          <xsl:variable name="vConvertedStrkPx">
            <xsl:call-template name="GetStrikeConvertedPriceValue">
              <xsl:with-param name="pIdAsset" select="$vIdAsset"/>
            </xsl:call-template>
          </xsl:variable>
          <convertedStrkPx>
            <xsl:copy-of select="$vConvertedStrkPx"/>
          </convertedStrkPx>
          <!--//-->
        </xsl:if>
        <BOOK id="{string($gSortingKeys[text()='BOOK']/@id)}">
          <xsl:attribute name="data">
            <xsl:choose>
              <xsl:when test="string-length($vBook) > 0 ">
                <xsl:value-of select="$vBook"/>
              </xsl:when>
              <xsl:when test="string-length($vBook) = 0 ">
                <xsl:value-of select="$gcNA"/>
              </xsl:when>
            </xsl:choose>
          </xsl:attribute>
        </BOOK>
        <MARKET id="{string($gSortingKeys[text()='MARKET']/@id)}">
          <xsl:attribute name="data">
            <xsl:variable name="vTemp" select="string($vTrdCaptRpt/Instrmt/@Exch)"/>
            <!-- // -->
            <xsl:choose>
              <xsl:when test="string-length($vTemp) > 0 ">
                <xsl:value-of select="$vTemp"/>
              </xsl:when>
              <xsl:when test="string-length($vTemp) = 0 ">
                <xsl:value-of select="$gcNA"/>
              </xsl:when>
            </xsl:choose>
          </xsl:attribute>
        </MARKET>
        <DERIVATIVECONTRACT id="{string($gSortingKeys[text()='DERIVATIVECONTRACT']/@id)}">
          <xsl:attribute name="data">
            <xsl:variable name="vTemp" select="string($vTrdCaptRpt/Instrmt/@Sym)"/>
            <!-- // -->
            <xsl:choose>
              <xsl:when test="string-length($vTemp) > 0 ">
                <xsl:value-of select="$vTemp"/>
              </xsl:when>
              <xsl:when test="string-length($vTemp) = 0 ">
                <xsl:value-of select="$gcNA"/>
              </xsl:when>
            </xsl:choose>
          </xsl:attribute>
        </DERIVATIVECONTRACT>
      </trade>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="paymentType" mode="create-fromInvoiceTrade">
    <xsl:param name="pIDB" />
    <xsl:param name="pInvoiceFee" />

    <xsl:variable name="vPaymentTypeEventType" select="./@eventType"/>
    <xsl:variable name="vPaymentTypeName" select="./@name"/>

    <xsl:variable name="vPaymentTypeInvoiceFee" select="$pInvoiceFee[(string(feeType/text()) = string($vPaymentTypeEventType))
                  and ((number(idB_Pay/text()) = number($pIDB)))]"/>
    <xsl:variable name="vPaymentTypeInvoiceFeeCount" select="count($vPaymentTypeInvoiceFee)"/>
    <!--//-->
    <xsl:if test="number($vPaymentTypeInvoiceFeeCount) > 0">
      <xsl:variable name="vPaymentTypeInvoiceFeeCu" select="$vPaymentTypeInvoiceFee/feeAmount/currency/text()"/>
      <!--//-->
      <xsl:variable name="vPayRec" select="$gcPay"/>

      <xsl:variable name="vAmountET">
        <xsl:choose>
          <xsl:when test="$vPaymentTypeInvoiceFeeCount = 1">
            <xsl:choose>
              <xsl:when test="$varIsInvoice = 'true'">
                <xsl:value-of select="number($vPaymentTypeInvoiceFee/feeInitialAmount/amount/text())"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="number($vPaymentTypeInvoiceFee/feeAmount/amount/text())"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$varIsInvoice = 'true'">
                <xsl:value-of select="sum($vPaymentTypeInvoiceFee/feeInitialAmount/amount/text())"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="sum($vPaymentTypeInvoiceFee/feeAmount/amount/text())"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vBaseAmountET">
        <xsl:if test="$varIsAmendedInvoice = 'true'">
          <xsl:choose>
            <xsl:when test="$vPaymentTypeInvoiceFeeCount = 1">
              <xsl:value-of select="number($vPaymentTypeInvoiceFee/feeBaseAmount/amount/text())"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="sum($vPaymentTypeInvoiceFee/feeBaseAmount/amount/text())"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="vDeltaAmountET">
        <xsl:if test="$varIsAmendedInvoice = 'true'">
          <xsl:value-of select="number($vAmountET - $vBaseAmountET)" />
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="vAbsDeltaAmountET">
        <xsl:if test="string-length($vDeltaAmountET)>0">
          <xsl:call-template name="abs">
            <xsl:with-param name="n" select="$vDeltaAmountET" />
          </xsl:call-template>
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="vDeltaPr">
        <xsl:choose>
          <xsl:when test="number($vDeltaAmountET) > 0">
            <xsl:value-of select="$gcPay"/>
          </xsl:when>
          <xsl:when test="0 > number($vDeltaAmountET)">
            <xsl:value-of select="$gcRec"/>
          </xsl:when>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="string-length($vAmountET)>0">
        <payment paymentType="{$vPaymentTypeName}"
                 pr="{$vPayRec}"
                 currency="{$vPaymentTypeInvoiceFeeCu}"
                 amountET="{$vAmountET}">

          <xsl:if test="$varIsAmendedInvoice = 'true' and string-length($vAbsDeltaAmountET)>0 and $vAbsDeltaAmountET >0">
            <xsl:attribute name="baseAmountET">
              <xsl:value-of select="$vBaseAmountET"/>
            </xsl:attribute>
            <xsl:attribute name="deltaAmountET">
              <xsl:value-of select="$vAbsDeltaAmountET"/>
            </xsl:attribute>
            <xsl:attribute name="deltaPr">
              <xsl:value-of select="$vDeltaPr"/>
            </xsl:attribute>
          </xsl:if>
        </payment>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- paymentType template mode="serie-subtotal-forInvoice"-->
  <xsl:template match="paymentType" mode="serie-subtotal-forInvoice">
    <xsl:param name="pPayments" />
    <xsl:param name="pGroupePaymentCu" />

    <xsl:variable name="vPaymentTypeName" select="./@name"/>
    <xsl:variable name="vPaymentTypeCuLabel" select="./@cuLabel"/>
    <xsl:variable name="vPaymentTypeCu" select="$pGroupePaymentCu[@name=$vPaymentTypeCuLabel]/@value"/>

    <xsl:variable name="vDeltaAmountET">
      <xsl:if test="$varIsAmendedInvoice = 'true'">
        <xsl:value-of select="
                            sum($pPayments[@deltaPr=$gcPay]/@deltaAmountET) 
                            - 
                            sum($pPayments[@deltaPr=$gcRec]/@deltaAmountET)"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vAbsDeltaAmountET">
      <xsl:if test="string-length($vDeltaAmountET)>0">
        <xsl:call-template name="abs">
          <xsl:with-param name="n" select="$vDeltaAmountET" />
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vDeltaPr">
      <xsl:choose>
        <xsl:when test="number($vDeltaAmountET) > 0">
          <xsl:value-of select="$gcPay"/>
        </xsl:when>
        <xsl:when test="0 > number($vDeltaAmountET)">
          <xsl:value-of select="$gcRec"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <payment paymentType="{$vPaymentTypeName}"
             pr="{$pPayments/@pr}"
             currency="{$vPaymentTypeCu}"
             amountET="{sum($pPayments/@amountET)}">

      <xsl:if test="$varIsAmendedInvoice = 'true' and string-length($vAbsDeltaAmountET)>0 and $vAbsDeltaAmountET >0">
        <xsl:attribute name="baseAmountET">
          <xsl:value-of select="sum($pPayments/@baseAmountET)"/>
        </xsl:attribute>
        <xsl:attribute name="deltaAmountET">
          <xsl:value-of select="$vAbsDeltaAmountET"/>
        </xsl:attribute>
        <xsl:attribute name="deltaPr">
          <xsl:value-of select="$vDeltaPr"/>
        </xsl:attribute>
      </xsl:if>
    </payment>
  </xsl:template>

  <!-- BusinessSpecificTradeData template -->
  <xsl:template name="BusinessSpecificTradeData"/>
  <xsl:template name="BusinessSpecificTradeWithStatusData"/>

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

    <xsl:apply-templates select="$gInvoicingPaymentType" mode="serie-subtotal-forInvoice">
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
    <xsl:param name="pIsFeesDet"/>
    <xsl:param name="pPaymentType"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pCurrencyFxRate"/>
    <xsl:param name="pRoundDir"/>
    <xsl:param name="pRoundPrec"/>
    <xsl:param name="pIsCreditNoteGroup" select="false()" />

    <xsl:variable name="vTaxDetails">
      <xsl:choose>
        <xsl:when test="$varIsInvoice = 'true' or $pIsCreditNoteGroup = 'true'">
          <xsl:copy-of select="msxsl:node-set($varProduct)/tax/details"/>
        </xsl:when>
        <xsl:when test="$varIsAdditionalInvoice = 'true'">
          <xsl:copy-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/tax/details"/>
        </xsl:when>
        <xsl:when test="$varIsCreditNote = 'true'">
          <xsl:copy-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/tax/details"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vAllTaxRateAmount">
      <xsl:if test="$pIsFeesDet">
        <xsl:for-each select="msxsl:node-set($vTaxDetails)/details">
          <taxAmount paymentType="{$pPaymentType}"
                     rate="{taxSource/spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetailRate']/text() * 100}"
                     detail="{taxSource/spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetail']/text()}"
                     amount="{taxAmount/amount/amount/text()}"
                     currency="{taxAmount/amount/currency/text()}"/>
        </xsl:for-each>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vAllTaxAmount" select="msxsl:node-set($vAllTaxRateAmount)/taxAmount"/>
    <xsl:variable name="vFeeGrossAmount">
      <xsl:choose>
        <xsl:when test="$varIsInvoice = 'true'">
          <xsl:value-of select="number(msxsl:node-set($varProduct)/grossTurnOverAmount/amount/text())"/>
        </xsl:when>
        <xsl:when test="$pIsCreditNoteGroup = 'true'">
          <xsl:value-of select="number(msxsl:node-set($varProduct)/netTurnOverAmount/amount/text())"/>
        </xsl:when>
        <xsl:when test="$varIsAdditionalInvoice = 'true'">
          <xsl:value-of select="number(msxsl:node-set($varProduct)/theoricInvoiceAmount/grossTurnOverAmount/amount/text())"/>
        </xsl:when>
        <xsl:when test="$varIsCreditNote = 'true'">
          <xsl:value-of select="number(msxsl:node-set($varProduct)/theoricInvoiceAmount/grossTurnOverAmount/amount/text())"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$vFeeGrossAmount > 0">

      <xsl:variable name="vExFeeGrossAmount">
        <xsl:call-template name="ExchangeAmount">
          <xsl:with-param name="pAmount" select="$vFeeGrossAmount" />
          <xsl:with-param name="pFxRate" select="$pCurrencyFxRate" />
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vCrDrFeeGrossAmount">
        <xsl:choose>
          <xsl:when test="$varIsCreditNote = 'true'">
            <xsl:value-of select="$gcCredit"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gcDebit"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- 20120626 MF adding row span evalaution on transaction report (after enhancements financial report) -->
      <xsl:variable name="vHowManyTaxes">
        <xsl:value-of select="count($vAllTaxAmount) + 1"/>
      </xsl:variable>

      <!-- 20120626 MF adding row span attribute (rowspan) on transaction report (after enhancements financial report) -->
      <amount name="feeGrossAmount" crDr="{$vCrDrFeeGrossAmount}"
              amount="{$vFeeGrossAmount}" exAmount="{$vExFeeGrossAmount}"
              keyForSum="{$pPaymentType}" rowspan="{$vHowManyTaxes}"/>

      <!--   Details                                         -->

      <xsl:for-each select="$vAllTaxAmount">
        <xsl:sort select="@detail" data-type="text"/>
        <xsl:sort select="@rate" data-type="number"/>

        <xsl:variable name="vFeeDetailAmount" select="@amount"/>

        <xsl:if test="$vFeeDetailAmount > 0">

          <xsl:variable name="vFeeDetailAmountRounded">
            <xsl:call-template name="RoundAmount">
              <xsl:with-param name="pAmount" select="$vFeeDetailAmount" />
              <xsl:with-param name="pRoundDir" select="$pRoundDir"/>
              <xsl:with-param name="pRoundPrec" select="$pRoundPrec"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vExFeeDetailAmount">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vFeeDetailAmountRounded" />
              <xsl:with-param name="pFxRate" select="$pCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="feeTax" crDr="{$vCrDrFeeGrossAmount}"
                  amount="{$vFeeDetailAmountRounded}" exAmount="{$vExFeeDetailAmount}"
                  taxDetailName="{@detail}" taxDetailRate="{@rate}"
                  keyForSum="{concat(@detail,'-',$pPaymentType)}"/>
        </xsl:if>
      </xsl:for-each>

    </xsl:if>

  </xsl:template>

  <!-- ================================================== -->
  <!--        Specific Display Template                   -->
  <!-- ================================================== -->
  <!--Affichage du trade dans le Header du report-->
  <xsl:template name="GetTradeHeader">
    <xsl:param name="pIsCreditNoteVisualization" select="false()" />

    <!-- Show only if you are building the credit note -->
    <xsl:if test="$pIsCreditNoteVisualization and $varIsAmendedInvoice = 'true'">
      <fo:block>
        <xsl:if test="$varIsCreditNote = 'true'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'INV-CreditNote'" />
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$varIsAdditionalInvoice = 'true'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'INV-AdditionalInvoice'" />
          </xsl:call-template>
        </xsl:if>
        <xsl:value-of select="$varEspace" />
        <fo:inline font-weight="bold">
          <xsl:value-of select="$varCreditNoteNumber" />
        </fo:inline>
      </fo:block>
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'INV-On'" />
      </xsl:call-template>
      <xsl:value-of select="$varEspace" />
    </xsl:if>
    <!-- Show only if you are building the amended invoice -->
    <xsl:if test="$pIsCreditNoteVisualization = false() and $varIsAmendedInvoice = 'true'">
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'INV-AmendedInvoice'" />
      </xsl:call-template>
      <xsl:value-of select="$varEspace" />
    </xsl:if>
    <xsl:if test="($pIsCreditNoteVisualization and $varIsAmendedInvoice = 'true') or ($varIsInvoice = 'true')">
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'INV-Invoice'" />
      </xsl:call-template>
    </xsl:if>
    <xsl:value-of select="$varEspace" />
    <fo:inline font-weight="bold">
      <xsl:value-of select="$varInvoiceNumber" />
    </fo:inline>
    <xsl:value-of select="concat($varEspace,'(')" />
    <xsl:call-template name="format-shortdate2">
      <xsl:with-param name="xsd-date-time" select="$varInvoiceBeginPeriod" />
    </xsl:call-template>
    <xsl:value-of select="concat($varEspace,'-',$varEspace)" />
    <xsl:call-template name="format-shortdate2">
      <xsl:with-param name="xsd-date-time" select="$varInvoiceEndPeriod" />
    </xsl:call-template>
    <xsl:value-of select="')'" />
  </xsl:template>
  <!--DisplaySummaryPageBody template-->
  <xsl:template name="DisplaySummaryPageBody">
    <xsl:param name="pIsCreditNoteVisualization" select="false()" />

    <!-- Display receiver name and address -->
    <xsl:call-template name="DisplayReceiver">
      <xsl:with-param name="pReceiverPaddingTop" select="$gInvoicingReceiverPaddingTop" />
    </xsl:call-template>

    <!-- Display invoice number and period -->
    <!-- Ex. Invoice INV-130 from 01/03/2009 to 31/03/2009 -->
    <!-- EG 20160404 Migration vs2013 -->
    <fo:table table-layout="fixed" padding-top="{$gSummuryBlockPaddingTop}" border="{$gcTableBorderDebug}" width="{$gDetTrdTableTotalWidth}">
      <fo:table-column column-width="{$gSummuryBlockPadding}" column-number="01" />
      <fo:table-column column-width="{$gSummaryBlockWidth}" column-number="02" />
      <fo:table-column column-width="{$gSummuryBlockPadding}" column-number="03" />
      <fo:table-body>
        <fo:table-row>
          <fo:table-cell/>
          <!-- EG 20160404 Migration vs2013 -->
          <fo:table-cell border="{$varTableBorderDebug}" text-align="{$varSummaryPageInvoiceTextAlign}" font-size="{$varSummaryPageInvoiceFontSize}">
            <xsl:call-template name="getInvoiceNumber">
              <!-- Enable the credit note visualization, in case we are printing a credit note -->
              <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
            </xsl:call-template>
            <!-- Total amounts -->
            <xsl:call-template name="displayInvoiceSummary">
              <!-- Enable the credit note visualization, in case we are printing a credit note -->
              <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
            </xsl:call-template>
            <!-- Display bank account -->
            <xsl:call-template name="displayBankAccount" />
          </fo:table-cell>
          <fo:table-cell/>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>
  <!--DisplayReceiverDetailSpecific template-->
  <xsl:template name="DisplayReceiverDetailSpecific">
    <xsl:param name="pBook" />
    <xsl:if test="$varTaxAmount&gt;0">
      <fo:table>
        <fo:table-body>
          <xsl:call-template name="CreateEmptyRow"/>
          <fo:table-row font-weight="normal">
            <fo:table-cell border="{$varTableBorderDebug}"/>
            <fo:table-cell border="{$varTableBorderDebug}">
              <fo:block text-align="{$varTextAlign}" font-size="8pt">
                <xsl:call-template name="displayInvoicedTaxNumber" />
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>
  <!--DisplayPageBodyStart template-->
  <xsl:template name="DisplayPageBodyStart"/>

  <!--==========================================================================================-->
  <!-- TEMPLATE : displayReceiverAddress                                                        -->
  <!--            Display payer full adress                                                     -->
  <!--==========================================================================================-->
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
      <!-- ===== ClearBroker ===== -->
      <xsl:when test="$vColumnName='TS-ClearBroker'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:value-of select="$pRowData/@clearBroker"/>
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
      <!-- ===== BrokeragePrev ===== -->
      <xsl:when test="$vColumnName='INV-BrokeragePrev'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBro]/@baseAmountET) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcBro]/@baseAmountET" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokeragePrev DrCr===== -->
      <xsl:when test="$vColumnName='INV-BrokeragePrevDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBro]/@baseAmountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcBro]/@baseAmountET) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcBro]/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokeragePrevTrd ===== -->
      <xsl:when test="$vColumnName='INV-BrokeragePrevTrd'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBroTrd]/@baseAmountET) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcBroTrd]/@baseAmountET" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokeragePrevTrd DrCr===== -->
      <xsl:when test="$vColumnName='INV-BrokerageTrdPrevDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBroTrd]/@baseAmountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcBroTrd]/@baseAmountET) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcBroTrd]/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokeragePrevClr ===== -->
      <xsl:when test="$vColumnName='INV-BrokeragePrevClr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBroClr]/@baseAmountET) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcBroClr]/@baseAmountET" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokeragePrevClr DrCr ===== -->
      <xsl:when test="$vColumnName='INV-BrokeragePrevClrDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBroClr]/@baseAmountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcBroClr]/@baseAmountET) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcBroClr]/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeePrev ===== -->
      <xsl:when test="$vColumnName='INV-FeePrev'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFee]/@baseAmountET) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcFee]/@baseAmountET" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeePrev DrCr ===== -->
      <xsl:when test="$vColumnName='INV-FeePrevDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFee]/@baseAmountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcFee]/@baseAmountET) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcFee]/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeePrevTrd ===== -->
      <xsl:when test="$vColumnName='INV-FeePrevTrd'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFeeTrd]/@baseAmountET) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcFeeTrd]/@baseAmountET" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeePrevTrd DrCr ===== -->
      <xsl:when test="$vColumnName='INV-FeePrevTrdDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFeeTrd]/@baseAmountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcFeeTrd]/@baseAmountET) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcFeeTrd]/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeePrevClr ===== -->
      <xsl:when test="$vColumnName='INV-FeePrevClr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFeeClr]/@baseAmountET) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcFeeClr]/@baseAmountET" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeePrevClr DrCr ===== -->
      <xsl:when test="$vColumnName='INV-FeePrevClrDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFeeClr]/@baseAmountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcFeeClr]/@baseAmountET) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcFeeClr]/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Delta ===== -->
      <xsl:when test="$vColumnName='INV-Delta'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaAmountET) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaAmountET" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Delta DrCr ===== -->
      <xsl:when test="$vColumnName='INV-DeltaDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaAmountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaAmountET) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaPr" />
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

    <xsl:choose>
      <!-- ===== BrokeragePrevSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokeragePrevSum'">
        <xsl:if test="string-length($pSerie/sum/payment[@paymentType=$gInvoicingPaymentType/@name]/@baseAmountET) > 0">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="$pSerie/sum/payment[@paymentType=$gInvoicingPaymentType/@name]/@baseAmountET" />
            <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== BrokeragePrevSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokeragePrevSumDrCr'">
        <xsl:if test="string-length($pSerie/sum/payment[@paymentType=$gInvoicingPaymentType/@name]/@baseAmountET) > 0">
          <xsl:call-template name="AmountSufix">
            <xsl:with-param name="pAmountSens" select="$pSerie/sum/payment[@paymentType=$gInvoicingPaymentType/@name]/@pr" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== DeltaSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-DeltaSum'">
        <xsl:if test="string-length($pSerie/sum/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaAmountET) > 0">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="$pSerie/sum/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaAmountET" />
            <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== DeltaSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-DeltaSumDrCr'">
        <xsl:if test="string-length($pSerie/sum/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaAmountET) > 0">
          <xsl:call-template name="AmountSufix">
            <xsl:with-param name="pAmountSens" select="$pSerie/sum/payment[@paymentType=$gInvoicingPaymentType/@name]/@deltaPr" />
          </xsl:call-template>
        </xsl:if>
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