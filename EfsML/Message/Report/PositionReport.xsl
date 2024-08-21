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
 Date: 17/11/2011
 Author: RD
 Description: first version  
 
  
Version 1.0.1.0 (Spheres 2.6.4.0)
 Date: 22/08/2012 [Ticket 18073]
 Author: MF
 Description: Add converted prices to the position report. the strike price, transaction price 
 (including the average price value insiode of convertedShort node and convertedLong node, childs of the sum node) 
 and the closing price will be displayed using the derivative contract specific numerical base and the given style.  
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
  <xsl:variable name="gReportType" select="$gcReportTypePOS"/>

  <!--Legend-->
  <xsl:variable name="gIsReportWithSpecificLegend" select="false()"/>
  <xsl:variable name="gIsReportWithOrderTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithTradeTypeLegend" select="true()"/>
  <xsl:variable name="gIsReportWithPriceTypeLegend" select="false()"/>

  <!-- ===== Display settings of all Blocks===== -->
  <xsl:variable name="gDetTrdTableTotalWidth">
    <xsl:choose>
      <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
        <xsl:value-of select="'157mm'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'197mm'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gGroupDisplaySettings">
    <group sort="1" name="ETDBlock" padding-top="0.2cm">
      <!--Title-->
      <title empty-row="0.1cm" withColumnHdr="{true()}" withSubColumn="{true()}" font-size="7pt">

        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <column number="01" name="TS-MarketTitle" colspan="2">
              <header ressource="TS-MARKET" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
            <column number="02" name="TS-DCTitle" colspan="6">
              <header ressource="TS-DERIVATIVECONTRACT" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
            <column number="03" name="TS-DCUNLTitle" colspan="6">
              <header ressource="TS-DERIVATIVECONTRACTUNL" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
          </xsl:when>
          <xsl:otherwise>
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
            <column number="03" name="TS-DCUNLTitle" colspan="7">
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
        <column number="01" name="TS-Book" width="30.5mm" >
          <header rowspan="2"/>
          <data rowspan="2" length="22"/>
        </column>
      </xsl:if>

      <column number="{number('2') - number($vColNumberBase)}" name="TS-OrderNum" width="19mm">
        <header rowspan="2"/>
        <data text-align="center" length="11"/>
      </column>
      <column number="{number('2') - number($vColNumberBase)}" name="TS-Stgy" master-column="TS-OrderNum" width="19mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}" font-size-alt="{$gDetTrdFontSize}" length="11"/>
      </column>
      <column number="{number('3') - number($vColNumberBase)}" name="TS-TradeDate" width="14mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="3"/>
        <data text-align="center"/>
      </column>
      <column number="{number('3') - number($vColNumberBase)}" name="TS-TradeTime" master-column="TS-TradeDate" width="14mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('4') - number($vColNumberBase)}" name="TS-TradeNum" width="18mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="0"/>
        <data text-align="center"/>
      </column>
      <column number="{number('4') - number($vColNumberBase)}" name="TS-StgyLeg" master-column="TS-TradeNum" width="18mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('5') - number($vColNumberBase)}" name="TS-TradeType" width="9mm">
        <header master-ressource="TS-TradeHdr" master-ressource-colspan="0"/>
        <data text-align="center"/>
      </column>
      <column number="{number('5') - number($vColNumberBase)}" name="TS-SecondaryTradeType" master-column="TS-TradeType" width="7mm">
        <data text-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('6') - number($vColNumberBase)}" name="TS-QtyBuy" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="2"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="{number('7') - number($vColNumberBase)}" name="TS-QtySell" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="{number('8') - number($vColNumberBase)}" name="TS-Maturity" colspan="2" width="23mm">
        <data text-align="center"/>
      </column>
      <column number="{number('8') - number($vColNumberBase)}" name="TS-PutCall" master-column="TS-Maturity" width="5.5mm"/>
      <column number="{number('9') - number($vColNumberBase)}" name="TS-ConvertedStrike" master-column="TS-Maturity" width="17.5mm">
        <header text-align="right"/>
        <data text-align="right" typenode="true"/>
      </column>
      <column number="{number('10') - number($vColNumberBase)}" name="TS-ConvertedPrice" colspan="2" width="21mm">
        <data text-align="right" typenode="true"/>
      </column>
      <column number="{number('10') - number($vColNumberBase)}" name="TS-Premium" master-column="TS-ConvertedPrice" width="17.5mm">
        <header colspan="2"/>
        <data text-align="right"/>
      </column>
      <column number="{number('11') - number($vColNumberBase)}" name="TS-PremiumDrCr" master-column="TS-ConvertedPrice" width="3.5mm">
        <header colspan="0"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('12') - number($vColNumberBase)}" name="POS-ConvertedClearPrice" colspan="2" width="21mm">
        <data text-align="right" typenode="true"/>
      </column>
      <column number="{number('12') - number($vColNumberBase)}" name="POS-NOV" master-column="POS-ConvertedClearPrice" width="17.5mm">
        <header colspan="2" ressource="POS-LOV"/>
        <data text-align="right"/>
      </column>
      <column number="{number('13') - number($vColNumberBase)}" name="POS-NOVDrCr" master-column="POS-ConvertedClearPrice" width="3.5mm">
        <header colspan="0"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}"/>
      </column>
      <column number="{number('14') - number($vColNumberBase)}" name="POS-UMG" width="19.5mm">
        <header colspan="2"/>
        <data rowspan="2" text-align="right" border-right-style="none"/>
      </column>
      <column number="{number('15') - number($vColNumberBase)}" name="POS-UMGDrCr" width="3.5mm">
        <header colspan="0"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
      </column>
      <column number="{number('15') - number($vColNumberBase)}" name="EmptyColumn" master-column="POS-UMGDrCr" width="3.5mm">
        <header colspan="0"/>
        <data border-left-style="none"/>
      </column>
    </group>
    <xsl:if test="($gReportHeaderFooter=false()) or 
            $gReportHeaderFooter/summaryStrategies/text() = 'Bottom'">
      <group sort="2" name="ETDRecapStgy" padding-top="{$gGroupPaddingTop}">

        <!--Titles-->
        <title empty-row="0.1cm" withColumnHdr="{false()}" withSubColumn="{true()}" font-size="7pt" font-weight="bold"
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
            $gReportHeaderFooter/summaryCashFlows/text() = 'Bottom'">
      <group sort="3" name="POSRecapFlow" padding-top="{$gGroupPaddingTop}">

        <!--Titles-->
        <title empty-row="0.1cm" withColumnHdr="{false()}" withSubColumn="{false()}" font-size="7pt" font-weight="bold"
               background-color="{$gDetTrdHdrCellBackgroundColor}">
          <column number="01" name="TS-RecapCashFlowsTitle" colspan="8">
            <data border-bottom-style="solid" value="%%RES:POS-TitleGroupDet%%"/>
          </column>
        </title>

        <column number="1" name="POS-Currency" width="17mm" font-size="7pt" text-align="center">
          <header ressource="%%RES:POS-Currency%%"/>
          <data text-align="center" border-bottom-style="solid"/>
        </column>
        <column number="2" name="POS-UMGFuture" width="56.5mm" font-size="7pt">
          <header ressource="%%RES:POS-UMGFuture%%" colspan="2" />
          <data text-align="right"
                border-bottom-style="solid"
                border-right-style="none"/>
        </column>
        <column number="3" name="POS-CrDrUMGFuture" width="3.5mm">
          <header colspan="-1"/>
          <data font-size="5pt" display-align="center" border-bottom-style="solid" border-left-style="none"/>
        </column>
        <column number="4" name="POS-UMGOption" width="56.5mm" >
          <header ressource="%%RES:POS-UMGOption%%" colspan="2" text-align="center" />
          <data text-align="right"
                border-bottom-style="solid"
                border-right-style="none"/>
        </column>
        <column number="5" name="POS-CrDrUMGOption" width="3.5mm">
          <header colspan="-1"/>
          <data font-size="5pt" display-align="center" border-bottom-style="solid" border-left-style="none"/>
        </column>
        <column number="6" name="POS-PRMRecPay" width="56.5mm"  >
          <header ressource="%%RES:POS-PRMRecPay%%" colspan="2" text-align="center" />
          <data text-align="right"
                border-bottom-style="solid"
                border-right-style="none"/>
        </column>
        <column number="7" name="POS-CrDrPRMRecPay" width="3.5mm">
          <header colspan="-1"/>
          <data font-size="5pt" display-align="center" border-bottom-style="solid" border-left-style="none"/>
        </column>
      </group>
    </xsl:if>
  </xsl:variable>

  <!-- ================================================== -->
  <!--        Global Variables : Display                  -->
  <!-- ================================================== -->
  <!-- ===== SubTotals of principal table ===== -->
  <xsl:variable name="gColSubTotalColDisplaySettings">
    <group name="ETDBlock">
      <row sort="1">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <subtotal sort="2" name="TS-QtySumBuy" empty-columns="4"/>
          </xsl:when>
          <xsl:otherwise>
            <subtotal sort="1" name="TS-Book" columns-spanned="4" text-align="left"/>
            <subtotal sort="2" name="TS-QtySumBuy" empty-columns="1"/>
          </xsl:otherwise>
        </xsl:choose>
        <subtotal sort="3" name="TS-QtySumSell"/>
        <subtotal sort="4" name="TS-PRMSum" empty-columns="2" />
        <subtotal sort="5" name="TS-PRMSumDrCr" font-size="{$gDetTrdDetFontSize}" text-align="left" display-align="center"/>
        <subtotal sort="6" name="POS-NOVSum" rows-spanned="2" />
        <subtotal sort="7" name="POS-NOVSumDrCr" font-size="{$gDetTrdDetFontSize}" text-align="left" display-align="center"/>
        <subtotal sort="8" name="POS-UMGSum" rows-spanned="2" />
        <subtotal sort="9" name="POS-UMGSumDrCr" font-size="{$gDetTrdDetFontSize}" text-align="left" display-align="center"/>
      </row>
      <row sort="2">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <subtotal sort="2" name="TS-QtyPos" columns-spanned="4" text-align="right"/>
          </xsl:when>
          <xsl:otherwise>
            <subtotal sort="1" name="TS-BookDisplayname" columns-spanned="4" font-size="{$gDetTrdDetFontSize}" text-align="left"/>
            <subtotal sort="2" name="TS-QtyPos"/>
          </xsl:otherwise>
        </xsl:choose>
        <subtotal sort="3" name="TS-QtyPosBuy"/>
        <subtotal sort="4" name="TS-QtyPosSell"/>
        <subtotal sort="5" name="TS-ConvertedAvgPxBuy" columns-spanned="2" font-weight="normal" typenode="true">
          <label columns-spanned="2" font-weight="normal"/>
        </subtotal>
        <subtotal sort="6" name="EmptySum" empty-columns="2" />

      </row>
      <row sort="3">
        <condition subtotal="TS-AvgPxBuy"/>
        <condition subtotal="TS-AvgPxSell"/>

        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
            <subtotal sort="1" name="TS-ConvertedAvgPxSell" columns-spanned="2" font-weight="normal" empty-columns="6" typenode="true">
              <label columns-spanned="2" font-weight="normal"/>
            </subtotal>
          </xsl:when>
          <xsl:otherwise>
            <subtotal sort="1" name="TS-ConvertedAvgPxSell" columns-spanned="2" font-weight="normal" empty-columns="7" typenode="true">
              <label columns-spanned="2" font-weight="normal"/>
            </subtotal>
          </xsl:otherwise>
        </xsl:choose>
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
  <!--        Business Template                           -->
  <!-- ================================================== -->
  <!-- BusinessSpecificTradeData -->
  <xsl:template name="BusinessSpecificTradeData">
    <xsl:param name="pTradeId" />
    <xsl:param name="pIDB" />
    <xsl:param name="pCategory" />
    <xsl:param name="pLastQty" />
    <xsl:param name="pLastPx" />
    <xsl:param name="pPremium" />

    <xsl:attribute name="cat">
      <xsl:value-of select="$pCategory"/>
    </xsl:attribute>
    <!--//-->
    <xsl:variable name="vEventUMG" select="$gEvents/Event[(IDENTIFIER_TRADE=$pTradeId) and (EVENTCODE='LPC' and EVENTTYPE='UMG')]"/>
    <xsl:variable name="vDailyQty">
      <!-- RD 20141120 [] Fusion des tables EVENTDET_ETD avec EVENTDET-->
      <!--<xsl:if test="string-length($vEventUMG/EventDetails_ETD/DAILYQUANTITY) > 0">
        <xsl:value-of select="number($vEventUMG/EventDetails_ETD/DAILYQUANTITY)"/>
      </xsl:if>-->
      <xsl:if test="string-length($vEventUMG/EventDetails/DAILYQUANTITY) > 0">
        <xsl:value-of select="number($vEventUMG/EventDetails/DAILYQUANTITY)"/>
      </xsl:if>
    </xsl:variable>
    <!--//-->
    <xsl:attribute name="dailyQty">
      <xsl:value-of select="$vDailyQty"/>
    </xsl:attribute>
    <!--//-->
    <xsl:attribute name="totalPx">
      <xsl:value-of select="$vDailyQty*$pLastPx"/>
    </xsl:attribute>
    <!--//-->
    <xsl:variable name="vClrPx">
      <!-- RD 20141120 [] Fusion des tables EVENTDET_ETD avec EVENTDET-->
      <!--<xsl:if test="string-length($vEventUMG/EventDetails_ETD/QUOTEPRICE100) > 0">
        <xsl:value-of select="number($vEventUMG/EventDetails_ETD/QUOTEPRICE100)"/>
      </xsl:if>-->
      <xsl:if test="string-length($vEventUMG/EventDetails/QUOTEPRICE100) > 0">
        <xsl:value-of select="number($vEventUMG/EventDetails/QUOTEPRICE100)"/>
      </xsl:if>
    </xsl:variable>
    <!--//-->
    <xsl:attribute name="clrPx">
      <xsl:value-of select="$vClrPx"/>
    </xsl:attribute>
    <!--//-->
    <!-- 20120821 MF Ticket 18073 -->
    <!-- RD 20140725 [20212] Use element convertedClearingPrice instead of ConvertedTradePrice-->
    <xsl:variable name="vConvertedPositionClearingPrice">
      <!--<xsl:call-template name="GetEventConvertedPriceValue">
        -->
      <!-- UNDONE 20120821 MF - pTradeId parameter is redondant, 
              the event inside of pActionEvent is already specific to the right trade -->
      <!--
        <xsl:with-param name="pTradeId" select="$pTradeId"/>
        <xsl:with-param name="pActionEvent">
          <xsl:copy-of select="$vEventUMG"/>
        </xsl:with-param>
      </xsl:call-template>-->
      <xsl:call-template name="FormatConvertedPriceValue">
        <xsl:with-param name="pContent">
          <!-- RD 20141120 [] Fusion des tables EVENTDET_ETD avec EVENTDET-->
          <!--<xsl:copy-of select="$vEventUMG/EventDetails_ETD/convertedPrices/convertedClearingPrice"/>-->
          <xsl:copy-of select="$vEventUMG/EventDetails/convertedPrices/convertedClearingPrice"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <!-- 20120821 MF Ticket 18073 -->
    <convertedClrPx>
      <xsl:copy-of select="$vConvertedPositionClearingPrice"/>
    </convertedClrPx>
    <!--//-->
    <xsl:variable name="vUMGPayRec">
      <xsl:choose>
        <xsl:when test="$vEventUMG/IDB_PAY = $pIDB">
          <xsl:value-of select="$gcPay"/>
        </xsl:when>
        <xsl:when test="$vEventUMG/IDB_REC = $pIDB">
          <xsl:value-of select="$gcRec"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!--//-->
    <umg pr="{$vUMGPayRec}" currency="{$vEventUMG/UNIT}" amount="{$vEventUMG/VALORISATION}"/>
    <!--//-->
    <xsl:if test="$pCategory='O'">
      <xsl:variable name="vEventLOV" select="$gEvents/Event[(IDENTIFIER_TRADE=$pTradeId) and (EVENTCODE='LPC' and EVENTTYPE='LOV')]"/>
      <!--RD 20130616 [18320] Le sens est déjà inversé dans le événements, donc inutile de le refaire ici-->
      <xsl:variable name="vNOVPayRec">
        <xsl:choose>
          <xsl:when test="$vEventLOV/IDB_PAY = $pIDB">
            <xsl:value-of select="$gcPay"/>
          </xsl:when>
          <xsl:when test="$vEventLOV/IDB_REC = $pIDB">
            <xsl:value-of select="$gcRec"/>
          </xsl:when>
        </xsl:choose>
      </xsl:variable>
      <nov pr="{$vNOVPayRec}" currency="{$vEventLOV/UNIT}" amount="{$vEventLOV/VALORISATION}"/>
      <!--//-->
      <xsl:if test="$pPremium">
        <!--La prime encaissées/décaissées est au prorata de la quantité restante en position-->
        <prm pr="{$pPremium/@pr}" currency="{$pPremium/@currency}" amount="{(number($pPremium/@amount) * number($vDailyQty)) div number($pLastQty)}"/>
      </xsl:if>
    </xsl:if>
  </xsl:template>
  <!-- BusinessSpecificTradeWithStatusData -->
  <xsl:template name="BusinessSpecificTradeWithStatusData">
    <xsl:param name="pCuPrice"/>

    <xsl:if test="umg[@currency!=$pCuPrice]">
      <umg status="{$gcNOkStatus}"/>
    </xsl:if>

  </xsl:template>
  <!--SpecificSerieSubTotal template-->
  <xsl:template name="SpecificSerieSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pCuPrice"/>

    <!--//-->
    <!-- avg formula : sum( qty of trade * prix of trade) / sum ( qty of all trades) -->
    <xsl:variable name="vSumLongDailyQty" select="sum($pSerieTrades[@side=1]/@dailyQty)"/>
    <xsl:variable name="vSumLongTotalDailyQtyPx" select="sum($pSerieTrades[@side=1]/@totalPx)"/>
    <xsl:variable name="vAvgLongDailyQtyPx">
      <xsl:choose>
        <xsl:when test="$vSumLongDailyQty > 0">
          <xsl:value-of select="number($vSumLongTotalDailyQtyPx) div number($vSumLongDailyQty)"/>
        </xsl:when>
        <xsl:when test="$vSumLongDailyQty = 0">
          <xsl:value-of select="'0'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vSumShortDailyQty" select="sum($pSerieTrades[@side=2]/@dailyQty)"/>
    <xsl:variable name="vSumShortTotalDailyQtyPx" select="sum($pSerieTrades[@side=2]/@totalPx)"/>
    <xsl:variable name="vAvgShortDailyQtyPx">
      <xsl:choose>
        <xsl:when test="$vSumShortDailyQty > 0">
          <xsl:value-of select="number($vSumShortTotalDailyQtyPx) div number($vSumShortDailyQty)"/>
        </xsl:when>
        <xsl:when test="$vSumShortDailyQty = 0">
          <xsl:value-of select="'0'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!--//-->
    <xsl:variable name="vPosQty">
      <xsl:call-template name="abs">
        <xsl:with-param name="n" select="number($vSumLongDailyQty) - number($vSumShortDailyQty)" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vPosSLF">
      <xsl:choose>
        <xsl:when test="$vSumShortDailyQty > $vSumLongDailyQty">
          <xsl:value-of select="'Short'"/>
        </xsl:when>
        <xsl:when test="$vSumLongDailyQty > $vSumShortDailyQty">
          <xsl:value-of select="'Long'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Flat'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--//-->
    <xsl:if test="$vSumLongDailyQty >= 0">
      <long qty="{$vSumLongDailyQty}" avgPx="{$vAvgLongDailyQtyPx}"/>
      <!-- 20120827 MF Long converted quantity -->
      <convertedLong>
        <xsl:call-template name="GetConvertedAveragePriceValue">
          <xsl:with-param name="pIdAsset" select="$pSerieTrades/@idAsset"/>
          <xsl:with-param name="pSide" select="'1'"/>
        </xsl:call-template>
      </convertedLong>
    </xsl:if>
    <xsl:if test="$vSumShortDailyQty >= 0">
      <short qty="{$vSumShortDailyQty}" avgPx="{$vAvgShortDailyQtyPx}"/>
      <!-- 20120827 MF Long converted quantity -->
      <convertedShort>
        <xsl:call-template name="GetConvertedAveragePriceValue">
          <xsl:with-param name="pIdAsset" select="$pSerieTrades/@idAsset"/>
          <xsl:with-param name="pSide" select="'2'"/>
        </xsl:call-template>
      </convertedShort>
    </xsl:if>
    <pos qty="{$vPosQty}" slf="{$vPosSLF}"/>
    <!--//-->
    <xsl:call-template name="GetNOVSubTotal">
      <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
      <xsl:with-param name="pSerieTradesWithStatus" select="$pSerieTradesWithStatus"/>
      <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
    </xsl:call-template>
    <!--//-->
    <xsl:call-template name="GetUMGSubTotal">
      <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
      <xsl:with-param name="pSerieTradesWithStatus" select="$pSerieTradesWithStatus"/>
      <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
    </xsl:call-template>
  </xsl:template>

  <!--GetUMGSubTotal template-->
  <xsl:template name="GetUMGSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pCuPrice"/>

    <xsl:if test="count($pSerieTradesWithStatus/umg[@status = $gcNOkStatus]) = 0">
      <xsl:variable name="vSumRec" select="sum($pSerieTrades/umg[@pr=$gcRec]/@amount)"/>
      <xsl:variable name="vSumPay" select="sum($pSerieTrades/umg[@pr=$gcPay]/@amount)"/>
      <xsl:variable name="vMarginTotal" select="number($vSumRec) - number($vSumPay)"/>
      <!--//-->
      <xsl:choose>
        <xsl:when test="number($vMarginTotal) > 0">
          <umg pr="{$gcRec}" currency="{$pCuPrice}" amount="{$vMarginTotal}"/>
        </xsl:when>
        <xsl:when test="0 > number($vMarginTotal)">
          <umg pr="{$gcPay}" currency="{$pCuPrice}" amount="{$vMarginTotal * -1}"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
  <!--GetNOVSubTotal template-->
  <xsl:template name="GetNOVSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pCuPrice"/>

    <xsl:if test="$pSerieTrades/nov and count($pSerieTradesWithStatus/nov[@status = $gcNOkStatus]) = 0">
      <xsl:variable name="vSumRec" select="sum($pSerieTrades/nov[@pr=$gcRec]/@amount)"/>
      <xsl:variable name="vSumPay" select="sum($pSerieTrades/nov[@pr=$gcPay]/@amount)"/>
      <xsl:variable name="vNOVTotal" select="number($vSumRec) - number($vSumPay)"/>
      <!--//-->
      <xsl:choose>
        <xsl:when test="number($vNOVTotal) > 0">
          <nov pr="{$gcRec}" currency="{$pCuPrice}" amount="{$vNOVTotal}"/>
        </xsl:when>
        <xsl:when test="0 > number($vNOVTotal)">
          <nov pr="{$gcPay}" currency="{$pCuPrice}" amount="{$vNOVTotal * -1}"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!--SpecificGroupSubTotal template-->
  <xsl:template name="SpecificGroupSubTotal">
    <xsl:param name="pSeries"/>
    <xsl:param name="pCuPrice"/>
    <!--//-->
    <xsl:variable name="vAllUMG" select="msxsl:node-set($pSeries)/serie/sum//umg"/>
    <xsl:variable name="vAllUMGCount" select="count($vAllUMG)"/>
    <!-- 
    On vérifi si toutes les marges sont:
    1- Payées ou reçues par le Book 
    2- Sont dans la même devises
    -->
    <xsl:variable name="vIsUMGOk" select="($vAllUMGCount > 0) and ($vAllUMGCount = count($vAllUMG[(string-length(@pr) > 0) and (@currency=$pCuPrice)]))"/>
    <!--//-->
    <xsl:choose>
      <xsl:when test="$vIsUMGOk" >
        <xsl:variable name="vSumRec" select="sum($vAllUMG[@pr=$gcRec]/@amount)"/>
        <xsl:variable name="vSumPay" select="sum($vAllUMG[@pr=$gcPay]/@amount)"/>
        <xsl:variable name="vUMGTotal" select="number($vSumRec) - number($vSumPay)"/>
        <!--//-->
        <xsl:choose>
          <xsl:when test="number($vUMGTotal) >= 0">
            <sum pr="{$gcRec}" currency="{$pCuPrice}" subtotalEVAT="{$vUMGTotal}"/>
          </xsl:when>
          <xsl:when test="0 > number($vUMGTotal) ">
            <sum pr="{$gcPay}" currency="{$pCuPrice}" subtotalEVAT="{$vUMGTotal * -1}"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$vIsUMGOk = false()" >
        <sum currency="{$pCuPrice}" status="{$gcNOkStatus}"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ===== CreateSpecificGroups ===== -->
  <xsl:template name="CreateSpecificGroups">
    <xsl:param name="pBook" />
    <xsl:param name="pBookTrades" />

    <xsl:variable name="vPOSRecapFlowSettings" select="msxsl:node-set($gGroupDisplaySettings)/group[@name='POSRecapFlow']"/>

    <!-- POSRecapFlow -->
    <xsl:if test="$vPOSRecapFlowSettings">
      <xsl:variable name="vAllCurrencieAmount">
        <xsl:for-each select="$pBookTrades/umg[string-length(@currency)>0]">
          <amount currency="{@currency}"/>
        </xsl:for-each>
        <xsl:for-each select="$pBookTrades/prm[string-length(@currency)>0]">
          <amount currency="{@currency}"/>
        </xsl:for-each>
      </xsl:variable>

      <xsl:variable name="vAllCurrency" select="msxsl:node-set($vAllCurrencieAmount)/amount[generate-id()=generate-id(key('kCurrency',@currency))]"/>

      <xsl:if test="count($vAllCurrency) > 0">
        <group sort="{$vPOSRecapFlowSettings/@sort}" name="{$vPOSRecapFlowSettings/@name}">
          <xsl:for-each select="$vAllCurrency">
            <xsl:sort select="@currency" data-type="text"/>

            <xsl:variable name="vCurrency" select="string(@currency)"/>
            <xsl:variable name="vCurrencyRepository" select="$gDataDocumentRepository/currency[@id=concat($gPrefixCurrencyId, $vCurrency)]"/>

            <!--//-->
            <currency currency="{$vCurrency}" cuRoundDir="{$vCurrencyRepository/rounddir}" cuRoundPrec="{$vCurrencyRepository/roundprec}">
              <xsl:variable name="vSumUMGFutRec" select="sum($pBookTrades[@cat='F']/umg[@currency=$vCurrency and @pr=$gcRec]/@amount)"/>
              <xsl:variable name="vSumUMGFutPay" select="sum($pBookTrades[@cat='F']/umg[@currency=$vCurrency and @pr=$gcPay]/@amount)"/>
              <xsl:variable name="vSumUMGFutTot" select="number($vSumUMGFutRec) - number($vSumUMGFutPay)" />
              <xsl:choose>
                <xsl:when test="number($vSumUMGFutTot) > 0">
                  <umg cat="F" crDr="{$gcCredit}" amount="{$vSumUMGFutTot}"/>
                </xsl:when>
                <xsl:when test="0 > number($vSumUMGFutTot) ">
                  <umg cat="F" crDr="{$gcDebit}" amount="{$vSumUMGFutTot * -1}"/>
                </xsl:when>
              </xsl:choose>

              <xsl:variable name="vSumUMGOptRec" select="sum($pBookTrades[@cat='O']/umg[@currency=$vCurrency and @pr=$gcRec]/@amount)"/>
              <xsl:variable name="vSumUMGOptPay" select="sum($pBookTrades[@cat='O']/umg[@currency=$vCurrency and @pr=$gcPay]/@amount)"/>
              <xsl:variable name="vSumUMGOptTot" select="number($vSumUMGOptRec) - number($vSumUMGOptPay)" />
              <xsl:choose>
                <xsl:when test="number($vSumUMGOptTot) > 0">
                  <umg cat="O" crDr="{$gcCredit}" amount="{$vSumUMGOptTot}"/>
                </xsl:when>
                <xsl:when test="0 > number($vSumUMGOptTot) ">
                  <umg cat="O" crDr="{$gcDebit}" amount="{$vSumUMGOptTot * -1}"/>
                </xsl:when>
              </xsl:choose>

              <xsl:variable name="vSumPRMRec" select="sum($pBookTrades[@cat='O']/prm[@currency=$vCurrency and @pr=$gcRec]/@amount)"/>
              <xsl:variable name="vSumPRMPay" select="sum($pBookTrades[@cat='O']/prm[@currency=$vCurrency and @pr=$gcPay]/@amount)"/>
              <xsl:variable name="vSumPRMTot" select="number($vSumPRMRec) - number($vSumPRMPay)" />
              <xsl:choose>
                <xsl:when test="number($vSumPRMTot) > 0">
                  <prm crDr="{$gcCredit}" amount="{$vSumPRMTot}"/>
                </xsl:when>
                <xsl:when test="0 > number($vSumPRMTot) ">
                  <prm crDr="{$gcDebit}" amount="{$vSumPRMTot * -1}"/>
                </xsl:when>
              </xsl:choose>

            </currency>

          </xsl:for-each>
        </group>
      </xsl:if>
    </xsl:if>
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
  <!--        Display Template                            -->
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

  <!-- ===== Specific Currency display ===== -->
  <xsl:template match="currency" mode="display">
    <xsl:param name="pGroupColumnsToDisplay" />

    <xsl:variable name="vIsLast" select="boolean(position() = last())"/>

    <fo:table-row font-size="{$gDetTrdFontSize}" font-weight="normal">
      <xsl:apply-templates select="$pGroupColumnsToDisplay" mode="display-data">
        <xsl:sort select="@number" data-type="number"/>
        <xsl:with-param name="pRowData" select="." />
        <xsl:with-param name="pIsLast" select="$vIsLast" />
      </xsl:apply-templates>
    </fo:table-row>

  </xsl:template>

  <!-- ===== Mapping between columns to display and Data to display for each column ===== -->
  <!-- GetColumnDataToDisplay -->
  <xsl:template name="GetColumnDataToDisplay">
    <xsl:param name="pRowData" />
    <xsl:param name="pTradeWithStatus" />
    <xsl:param name="pIsGetDetail" select="false()" />

    <xsl:variable name="vColumnName" select="@name"/>

    <xsl:choose>
      <!-- ===== Currency ===== -->
      <xsl:when test="$vColumnName='POS-Currency'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:value-of select="$pRowData/@currency"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== UMGFuture ===== -->
      <xsl:when test="$vColumnName='POS-UMGFuture'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:variable name="vDecPattern">
              <xsl:call-template name="GetDecPattern">
                <xsl:with-param name="pRoundDir" select="$pRowData/@cuRoundDir"/>
                <xsl:with-param name="pRoundPrec" select="$pRowData/@cuRoundPrec"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAmount">
              <xsl:choose>
                <xsl:when test="number($pRowData/umg[@cat='F']/@amount) > 0">
                  <xsl:value-of select="$pRowData/umg[@cat='F']/@amount" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="number('0')" />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="$vAmount" />
              <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CrDrUMGFuture ===== -->
      <xsl:when test="$vColumnName='POS-CrDrUMGFuture'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="number($pRowData/umg[@cat='F']/@amount) > 0">
                <xsl:value-of select="$pRowData/umg[@cat='F']/@crDr" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gcEspace" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== UMGOption ===== -->
      <xsl:when test="$vColumnName='POS-UMGOption'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:variable name="vDecPattern">
              <xsl:call-template name="GetDecPattern">
                <xsl:with-param name="pRoundDir" select="$pRowData/@cuRoundDir"/>
                <xsl:with-param name="pRoundPrec" select="$pRowData/@cuRoundPrec"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAmount">
              <xsl:choose>
                <xsl:when test="number($pRowData/umg[@cat='O']/@amount) > 0">
                  <xsl:value-of select="$pRowData/umg[@cat='O']/@amount" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="number('0')" />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="$vAmount" />
              <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CrDrUMGOption ===== -->
      <xsl:when test="$vColumnName='POS-CrDrUMGOption'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="number($pRowData/umg[@cat='O']/@amount) > 0">
                <xsl:value-of select="$pRowData/umg[@cat='O']/@crDr" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gcEspace" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== PRMRecPay ===== -->
      <xsl:when test="$vColumnName='POS-PRMRecPay'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:variable name="vDecPattern">
              <xsl:call-template name="GetDecPattern">
                <xsl:with-param name="pRoundDir" select="$pRowData/@cuRoundDir"/>
                <xsl:with-param name="pRoundPrec" select="$pRowData/@cuRoundPrec"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAmount">
              <xsl:choose>
                <xsl:when test="number($pRowData/prm/@amount) > 0">
                  <xsl:value-of select="$pRowData/prm/@amount" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="number('0')" />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="$vAmount" />
              <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
            </xsl:call-template>

          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CrDrPRMRecPay ===== -->
      <xsl:when test="$vColumnName='POS-CrDrPRMRecPay'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="number($pRowData/prm/@amount) > 0">
                <xsl:value-of select="$pRowData/prm/@crDr" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gcEspace" />
              </xsl:otherwise>
            </xsl:choose>
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
                <xsl:with-param name="integer" select="$pRowData/@dailyQty" />
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
                <xsl:with-param name="integer" select="$pRowData/@dailyQty" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Closing Price ===== -->
      <xsl:when test="$vColumnName='Report-ClearPrice'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="concat($pRowData/@clrPx,'')" />
              <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- 20120826 MF Ticket 18073 -->
      <!-- ===== Converted Closing Price ===== -->
      <xsl:when test="$vColumnName='POS-ConvertedClearPrice'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:copy-of select="$pRowData/convertedClrPx"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Margin ===== -->
      <xsl:when test="$vColumnName='POS-UMG'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/umg/@amount) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData/umg/@amount" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Margin DrCr===== -->
      <xsl:when test="$vColumnName='POS-UMGDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pAmountSens" select="$pRowData/umg/@pr" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== NOV ===== -->
      <xsl:when test="$vColumnName='POS-NOV'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/nov/@amount) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData/nov/@amount" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== NOV DrCr ===== -->
      <xsl:when test="$vColumnName='POS-NOVDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pAmountSens" select="$pRowData/nov/@pr" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Common ===== -->
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

    <xsl:choose>
      <xsl:when test="$pSubTotalName = 'POS-NOVSum'"/>
      <xsl:when test="$pSubTotalName = 'POS-NOVSumDrCr'"/>
      <xsl:when test="$pSubTotalName = 'POS-UMGSum'"/>
      <xsl:when test="$pSubTotalName = 'POS-UMGSumDrCr'"/>
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
      <!-- ===== NOVSum ===== -->
      <xsl:when test="$pSubTotalName = 'POS-NOVSum'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/nov/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/nov/@amount) > 0">
            <xsl:call-template name="format-money2">
              <xsl:with-param name="amount" select="$pSerie/sum/nov/@amount" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== NOVSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'POS-NOVSumDrCr'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/nov/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/nov/@amount) > 0 and number($pSerie/sum/nov/@amount) > 0">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pAmountSens" select="$pSerie/sum/nov/@pr" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== UMGSum ===== -->
      <xsl:when test="$pSubTotalName = 'POS-UMGSum'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/umg/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/umg/@amount) > 0">
            <xsl:call-template name="format-money2">
              <xsl:with-param name="amount" select="$pSerie/sum/umg/@amount" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== UMGSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'POS-UMGSumDrCr'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/umg/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/umg/@amount) > 0 and number($pSerie/sum/umg/@amount) > 0">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pAmountSens" select="$pSerie/sum/umg/@pr" />
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
