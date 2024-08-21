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
Version 1.0.0.0 (Spheres 2.6.4.0)
 Date: 21/08/2012
 Author: RD
 Description: first version
 
Version 1.0.1.0 (Spheres 2.6.4.0)
 Date: 22/08/2012 [Ticket 18073]
 Author: MF
 Description: Add converted prices to the synthetic position report. the strike price, transaction price 
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
  <xsl:variable name="gDataDocumentTrades" select="//posSynthetics/posSynthetic"/>
  <xsl:variable name="gReportType" select="$gcReportTypePOSSYNT"/>

  <!--Legend-->
  <xsl:variable name="gIsReportWithSpecificLegend" select="false()"/>
  <xsl:variable name="gIsReportWithOrderTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithTradeTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithPriceTypeLegend" select="false()"/>

  <!-- ===== Display settings of all Blocks===== -->
  <xsl:variable name="gDetTrdTableTotalWidth">
    <xsl:choose>
      <xsl:when test="$gIsMonoBook = 'true' and $gHideBookForMonoBookReport">
        <xsl:value-of select="'136.5mm'"/>
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
            <column number="01" name="TS-MarketTitle" colspan="3">
              <header ressource="TS-MARKET" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
            <column number="02" name="TS-DCTitle" colspan="3">
              <header ressource="TS-DERIVATIVECONTRACT" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
            <column number="03" name="TS-DCUNLTitle" colspan="5">
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
            <column number="02" name="TS-DCTitle" colspan="6">
              <header ressource="TS-DERIVATIVECONTRACT" font-weight="normal" text-align="left"
                      background-color="{$gDetTrdHdrCellBackgroundColor}"/>
              <data font-weight="bold" border-bottom-style="solid"/>
            </column>
            <column number="03" name="TS-DCUNLTitle" colspan="4">
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
            <xsl:value-of select="number('2')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="number('0')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$gIsMonoBook = 'false' or $gHideBookForMonoBookReport = false()">
        <column number="1" name="TS-Book" width="52.5mm" >
          <header colspan="2" rowspan="2"/>
          <data rowspan="2" length="30" border-right-style="none"/>
        </column>
        <column number="2" name="TS-Empty" width="7mm" >
          <header colspan="0" />
          <data rowspan="2" border-left-style="none"/>
        </column>
      </xsl:if>

      <column number="{number('3') - number($vColNumberBase)}" name="TS-QtyBuy" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="2"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="{number('4') - number($vColNumberBase)}" name="TS-QtySell" width="9mm">
        <header master-ressource="TS-QtyHdr" master-ressource-colspan="0"/>
        <data rowspan="2" text-align="right"/>
      </column>
      <column number="{number('5') - number($vColNumberBase)}" name="TS-Maturity" colspan="2" width="23mm">
        <data text-align="center"/>
      </column>
      <column number="{number('5') - number($vColNumberBase)}" name="TS-PutCall" master-column="TS-Maturity" width="5.5mm"/>
      <column number="{number('6') - number($vColNumberBase)}" name="TS-ConvertedStrike" master-column="TS-Maturity" width="17.5mm">
        <header text-align="right"/>
        <data text-align="right" typenode="true"/>
      </column>
      <column number="{number('7') - number($vColNumberBase)}" name="TS-MaturityDate" rowspan="2" width="14mm">
        <header rowspan="2" text-align="center"  />
        <data rowspan="2" text-align="center"/>
      </column>
      <column number="{number('8') - number($vColNumberBase)}" name="POSSYNT-ConvertedPrice" width="18mm">
        <header rowspan="2" ressource="Report-AvgPrice"/>
        <data rowspan="2" text-align="right" typenode="true"/>
      </column>
      <column number="{number('9') - number($vColNumberBase)}" name="POS-ConvertedClearPrice" width="18mm">
        <data rowspan="2" text-align="right" typenode="true"/>
      </column>
      <column number="{number('10') - number($vColNumberBase)}" name="POS-NOV" width="19.5mm">
        <header rowspan="2" colspan="2" ressource="POSSYNT-LOV" ressource1="POSSYNT-LOVHdr"/>
        <data rowspan="2" text-align="right" border-right-style="none"/>
      </column>
      <column number="{number('11') - number($vColNumberBase)}" name="POS-NOVDrCr" width="3.5mm">
        <header colspan="0"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}" border-left-style="none"/>
      </column>
      <column number="{number('11') - number($vColNumberBase)}" name="EmptyColumn" master-column="POS-NOVDrCr" width="3.5mm">
        <header colspan="0"/>
        <data border-left-style="none"/>
      </column>
      <column number="{number('12') - number($vColNumberBase)}" name="POSSYNT-DELTA" width="23.5mm">
        <header rowspan="2"/>
        <data rowspan="2" text-align="right"/>
      </column>
    </group>
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
            <subtotal sort="1" name="TS-QtyPosSapanned" text-align="center" columns-spanned="2" />
            <subtotal sort="2" name="POS-NOVSum" empty-columns="5" />
            <subtotal sort="3" name="POS-NOVSumDrCr" font-size="{$gDetTrdDetFontSize}" text-align="left" display-align="center"/>
            <subtotal sort="4" name="EmptySum" columns-spanned="2" />
          </xsl:when>
          <xsl:when test="$gIsMonoBook = 'true'">
            <subtotal sort="1" name="TS-Book" text-align="left"/>
            <subtotal sort="2" name="TS-QtyPos"/>
            <subtotal sort="3" name="TS-QtyPosBuy"/>
            <subtotal sort="4" name="TS-QtyPosSell"/>
            <subtotal sort="5" name="POS-NOVSum" empty-columns="5"/>
            <subtotal sort="6" name="POS-NOVSumDrCr" font-size="{$gDetTrdDetFontSize}" text-align="left" display-align="center"/>
            <subtotal sort="7" name="EmptySum" columns-spanned="1" />
          </xsl:when>
          <xsl:otherwise>
            <subtotal sort="1" name="TS-Book" text-align="left"/>
            <subtotal sort="2" name="TS-QtyPos"/>
            <subtotal sort="3" name="TS-QtyPosBuy"/>
            <subtotal sort="4" name="TS-QtyPosSell"/>
            <subtotal sort="5" name="EmptySum" columns-spanned="5"/>
            <subtotal sort="6" name="POS-NOVSum" rows-spanned="2"/>
            <subtotal sort="7" name="POS-NOVSumDrCr" font-size="{$gDetTrdDetFontSize}" text-align="left" display-align="center"/>
            <subtotal sort="8" name="EmptySum" rows-spanned="2" />
          </xsl:otherwise>
        </xsl:choose>
      </row>
      <xsl:if test="$gIsMonoBook = 'false'">
        <row sort="2">
          <subtotal sort="1" name="TS-BookDisplayname" columns-spanned="9" font-size="{$gDetTrdDetFontSize}" text-align="left"/>
          <subtotal sort="2" name="EmptySum"/>
        </row>
      </xsl:if>
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
  <xsl:template match="posSynthetic" mode="business">

    <xsl:variable name="vETDAsset" select="$gDataDocumentRepository/etd[@OTCmlId=number(current()/@idAsset)]"/>
    <xsl:variable name="vDC" select="$gDataDocumentRepository/derivativeContract[@OTCmlId=number($vETDAsset/idDC)]"/>

    <xsl:variable name="vCategory" select="string($vDC/category)"/>

    <trade href="{concat(current()/@idb,current()/@idAsset,current()/@side)}"
           avgPrice="{number(current()/@avgPrice)}"
           fmtAvgPrice="{current()/@fmtAvgPrice}"
           side="{number(current()/@side)}"
           maturityDate="{$vETDAsset/maturityDate}"
           dailyQty="{number(current()/@dailyQty)}"
           clrPx="{number(current()/@clrPx)}"
           fmtClrPx="{current()/@fmtClrPx}">

      <!--
      La règle d'affichage du Delta
      -	Future : Pas d’affichage de Delta
      -	Option : Affichage du delta présent dans la table des cotations (donc un nombre positif pour un CALL et négatif pour un PUT)
      -->
      <xsl:if test="(number(current()/@delta) != 0) and $vCategory='O'">
        <xsl:attribute name="delta">
          <xsl:value-of select="number(current()/@delta)"/>
        </xsl:attribute>
      </xsl:if>
      <!--//-->
      <!-- 20120831 RD Warning: ne jamais rajouter des Attributs après les Eléments -->
      <xsl:if test="$vCategory='O'">

        <xsl:if test="number(current()/@delta) != 0">
          <xsl:attribute name="totalDelta">
            <xsl:choose>
              <xsl:when test="number(current()/@side) = 2">
                <xsl:value-of select="number(current()/@delta) * number(current()/@dailyQty) * number('-1')"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="number(current()/@delta) * number(current()/@dailyQty)"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>

        <strkPx data="{number($vETDAsset/strikePrice)}"/>
        <xsl:variable name="vPutCall">
          <xsl:choose>
            <xsl:when test="string($vETDAsset/putCall) = '0'">
              <xsl:value-of select="'Put'"/>
            </xsl:when>
            <xsl:when test="string($vETDAsset/putCall) = '1'">
              <xsl:value-of select="'Call'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <putCall data="{$vPutCall}"/>

        <!-- 20120821 MF Ticket 18073 -->
        <xsl:variable name="vConvertedStrkPx">
          <xsl:call-template name="GetStrikeConvertedPriceValue">
            <xsl:with-param name="pIdAsset" select="current()/@idAsset"/>
          </xsl:call-template>
        </xsl:variable>
        <!-- 20120821 MF Ticket 18073 -->
        <convertedStrkPx>
          <xsl:copy-of select="$vConvertedStrkPx"/>
        </convertedStrkPx>

        <!--C'est un montant signé-->
        <xsl:variable name="vAbsNovAmount">
          <xsl:call-template name="abs">
            <xsl:with-param name="n" select="number(current()/@lovAmount)" />
          </xsl:call-template>
        </xsl:variable>

        <!--RD 20130616 [18320] Le sens est déjà inversé dans le événements, donc inutile de le refaire ici-->
        <xsl:variable name="vNOVPayRec">
          <xsl:choose>
            <xsl:when test=" 0 > number(current()/@lovAmount)">
              <xsl:value-of select="$gcPay"/>
            </xsl:when>
            <xsl:when test="number(current()/@lovAmount) > 0">
              <xsl:value-of select="$gcRec"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <nov pr="{$vNOVPayRec}" currency="{string(current()/@lovCurrency)}" amount="{$vAbsNovAmount}"/>
      </xsl:if>
      <!--//-->
      <!-- 20120827 MF Ticket 18073 -->
      <!--<xsl:variable name="vConvertedLastPx">
        <xsl:call-template name="FormatConvertedPriceValue">
          <xsl:with-param name="pContent">
            <xsl:copy-of select="current()/convertedPrices/convertedSynthPositionPrice"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>-->
      <!-- 20120827 MF Ticket 18073 -->
      <!--<convertedLastPx>
        <xsl:copy-of select="$vConvertedLastPx"/>
      </convertedLastPx>-->
      <!--//-->
      <!-- 20120827 MF Ticket 18073 -->
      <!-- RD 20140725 [20212] Use element convertedClearingPrice instead of convertedSynthPositionClearingPrice-->
      <!--<xsl:variable name="vConvertedClearingPrice">
        <xsl:call-template name="FormatConvertedPriceValue">
          <xsl:with-param name="pContent">
            <xsl:copy-of select="current()/convertedPrices/convertedClearingPrice"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>-->
      <!-- 20120827 MF Ticket 18073 -->
      <!--<convertedClrPx>
        <xsl:copy-of select="$vConvertedClearingPrice"/>
      </convertedClrPx>-->
      <!--//-->
      <mmy data="{string($vETDAsset/maturityMonthYear)}"/>

      <BOOK id="{string($gSortingKeys[text()='BOOK']/@id)}">
        <xsl:variable name="vBook" select="string($gDataDocumentRepository/book[@OTCmlId=number(current()/@idb)]/identifier)"/>
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
        <xsl:variable name="vMarket" select="string($gDataDocumentRepository/market[@OTCmlId=number($vETDAsset/idM)]/identifier)"/>
        <xsl:attribute name="data">
          <xsl:choose>
            <xsl:when test="string-length($vMarket) > 0 ">
              <xsl:value-of select="$vMarket"/>
            </xsl:when>
            <xsl:when test="string-length($vMarket) = 0 ">
              <xsl:value-of select="$gcNA"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </MARKET>
      <DERIVATIVECONTRACT id="{string($gSortingKeys[text()='DERIVATIVECONTRACT']/@id)}">
        <xsl:variable name="vContract" select="string($vDC/identifier)"/>
        <xsl:attribute name="data">
          <xsl:choose>
            <xsl:when test="string-length($vContract) > 0 ">
              <xsl:value-of select="$vContract"/>
            </xsl:when>
            <xsl:when test="string-length($vContract) = 0 ">
              <xsl:value-of select="$gcNA"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </DERIVATIVECONTRACT>
    </trade>
  </xsl:template>
  <!-- BusinessSpecificTradeData -->
  <xsl:template name="BusinessSpecificTradeData"/>
  <!-- BusinessSpecificTradeWithStatusData -->
  <xsl:template name="BusinessSpecificTradeWithStatusData">
    <xsl:param name="pCuPrice"/>

    <xsl:if test="nov[@currency!=$pCuPrice]">
      <nov status="{$gcNOkStatus}"/>
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
    <xsl:variable name="vSumShortDailyQty" select="sum($pSerieTrades[@side=2]/@dailyQty)"/>
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
    <pos qty="{$vPosQty}" slf="{$vPosSLF}"/>
    <!--//-->
    <xsl:call-template name="GetNOVSubTotal">
      <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
      <xsl:with-param name="pSerieTradesWithStatus" select="$pSerieTradesWithStatus"/>
      <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
    </xsl:call-template>
    <!--//-->
    <xsl:call-template name="GetDELTASubTotal">
      <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
      <xsl:with-param name="pSerieTradesWithStatus" select="$pSerieTradesWithStatus"/>
      <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
    </xsl:call-template>
    <!--//-->
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

  <!--GetDELTASubTotal template-->
  <xsl:template name="GetDELTASubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pCuPrice"/>

    <xsl:variable name="vDELTATotal" select="sum($pSerieTrades/@totalDelta)"/>

    <xsl:choose>
      <xsl:when test="number($vDELTATotal) > 0">
        <delta pr="{$gcRec}" currency="{$pCuPrice}" amount="{$vDELTATotal}"/>
      </xsl:when>
      <xsl:when test="0 > number($vDELTATotal)">
        <delta pr="{$gcPay}" currency="{$pCuPrice}" amount="{$vDELTATotal * -1}"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--SpecificGroupSubTotal template-->
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

  <!-- ===== Mapping between columns to display and Data to display for each column ===== -->
  <!-- GetColumnDataToDisplay -->
  <xsl:template name="GetColumnDataToDisplay">
    <xsl:param name="pRowData" />
    <xsl:param name="pTradeWithStatus" />
    <xsl:param name="pIsGetDetail" select="false()" />

    <xsl:variable name="vColumnName" select="@name"/>

    <xsl:choose>
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
      <!-- ===== TradeDate ===== -->
      <xsl:when test="$vColumnName='TS-MaturityDate'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@maturityDate) > 0">
              <xsl:call-template name="format-shortdate_ddMMMyy">
                <xsl:with-param name="year" select="substring($pRowData/@maturityDate , 1, 4)" />
                <xsl:with-param name="month" select="substring($pRowData/@maturityDate , 6, 2)" />
                <xsl:with-param name="day" select="substring($pRowData/@maturityDate , 9, 2)" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Price ===== -->
      <xsl:when test="$vColumnName='POSSYNT-Price'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="concat($pRowData/@avgPrice,'')" />
              <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!--20120827 MF Ticket 18073-->
      <!-- ===== Converted Price ===== -->
      <xsl:when test="$vColumnName='POSSYNT-ConvertedPrice'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <!--<xsl:copy-of select="$pRowData/convertedLastPx"/>-->
            <xsl:call-template name="DisplayFmtPrice">
              <xsl:with-param name="pFmtPrice" select="$pRowData/@fmtAvgPrice"/>
            </xsl:call-template>
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
      <!--20120827 MF Ticket 18073-->
      <!-- ===== Converted Closing Price ===== -->
      <xsl:when test="$vColumnName='POS-ConvertedClearPrice'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <!--<xsl:copy-of select="$pRowData/convertedClrPx"/>-->
            <xsl:call-template name="DisplayFmtPrice">
              <xsl:with-param name="pFmtPrice" select="$pRowData/@fmtClrPx"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
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
      <!-- ===== DELTA ===== -->
      <xsl:when test="$vColumnName='POSSYNT-DELTA'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@delta) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="concat($pRowData/@delta,'')" />
                <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
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
      <xsl:when test="$pSubTotalName = 'POSSYNT-DELTASum'"/>
      <xsl:when test="$pSubTotalName = 'POSSYNT-DELTASumDrCr'"/>
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
      <!-- ===== QtySum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-QtyPosSapanned'">
        <xsl:value-of select="$pSerie/sum/pos/@slf"/>
        <xsl:if test="string-length($pSerie/sum/pos/@qty) > 0 and number($pSerie/sum/pos/@qty) > 0">
          <xsl:value-of select="$gcEspace"/>
          <xsl:call-template name="format-integer">
            <xsl:with-param name="integer" select="$pSerie/sum/pos/@qty" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
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
      <!-- ===== DELTASum ===== -->
      <xsl:when test="$pSubTotalName = 'POSSYNT-DELTASum'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/delta/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/delta/@amount) > 0">
            <xsl:call-template name="format-money2">
              <xsl:with-param name="amount" select="$pSerie/sum/delta/@amount" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== DELTASumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'POSSYNT-DELTASumDrCr'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/delta/@status = $gcNOkStatus">
            <xsl:value-of select="$gcEspace"/>
          </xsl:when>
          <xsl:when test="string-length($pSerie/sum/delta/@amount) > 0 and number($pSerie/sum/delta/@amount) > 0">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pAmountSens" select="$pSerie/sum/delta/@pr" />
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
